using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace GFPro
{
    public interface IKcpTransport : IDisposable
    {
        void Send(byte[] bytes, int index, int length, EndPoint endPoint);
        int Recv(byte[] buffer, ref EndPoint endPoint);
        int Available();
        void Update();
        void OnError(long id, int error);
    }

    public class UdpTransport : IKcpTransport
    {
        private readonly Socket socket;

        public UdpTransport(AddressFamily addressFamily)
        {
            socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
            NetworkHelper.SetSioUdpConnReset(socket);
        }

        public UdpTransport(IPEndPoint ipEndPoint)
        {
            socket = new Socket(ipEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                socket.SendBufferSize = Kcp.OneM * 64;
                socket.ReceiveBufferSize = Kcp.OneM * 64;
            }

            try
            {
                socket.Bind(ipEndPoint);
            }
            catch (Exception e)
            {
                throw new Exception($"bind error: {ipEndPoint}", e);
            }

            NetworkHelper.SetSioUdpConnReset(socket);
        }

        public void Send(byte[] bytes, int index, int length, EndPoint endPoint)
        {
            socket.SendTo(bytes, index, length, SocketFlags.None, endPoint);
        }

        public int Recv(byte[] buffer, ref EndPoint endPoint)
        {
            return socket.ReceiveFrom(buffer, ref endPoint);
        }

        public int Available()
        {
            return socket.Available;
        }

        public void Update()
        {
        }

        public void OnError(long id, int error)
        {
        }

        public void Dispose()
        {
            socket?.Dispose();
        }
    }

    public class TcpTransport : IKcpTransport
    {
        private readonly TService tService;

        private readonly DoubleMap<long, EndPoint> idEndpoints = new();

        private readonly Queue<(EndPoint, MemoryBuffer)> channelRecvDatas = new();

        private readonly Dictionary<long, long> readWriteTime = new();

        private readonly Queue<long> channelIds = new();

        public TcpTransport(AddressFamily addressFamily)
        {
            tService = new TService(addressFamily, ServiceType.Outer);
            tService.ErrorCallback = OnError;
            tService.ReadCallback = OnRead;
        }

        public TcpTransport(IPEndPoint ipEndPoint)
        {
            tService = new TService(ipEndPoint, ServiceType.Outer);
            tService.AcceptCallback = OnAccept;
            tService.ErrorCallback = OnError;
            tService.ReadCallback = OnRead;
        }

        private void OnAccept(long id, IPEndPoint ipEndPoint)
        {
            TChannel channel = tService.Get(id);
            var timeNow = TimeInfo.Instance.ClientFrameTime();
            readWriteTime[id] = timeNow;
            channelIds.Enqueue(id);
            idEndpoints.Add(id, channel.RemoteAddress);
        }

        public void OnError(long id, int error)
        {
            Log.Warning($"IKcpTransport tcp error: {id} {error}");
            tService.Remove(id, error);
            idEndpoints.RemoveByKey(id);
            readWriteTime.Remove(id);
        }

        private void OnRead(long id, MemoryBuffer memoryBuffer)
        {
            var timeNow = TimeInfo.Instance.ClientFrameTime();
            readWriteTime[id] = timeNow;
            TChannel channel = tService.Get(id);
            channelRecvDatas.Enqueue((channel.RemoteAddress, memoryBuffer));
        }

        public void Send(byte[] bytes, int index, int length, EndPoint endPoint)
        {
            var id = idEndpoints.GetKeyByValue(endPoint);
            if (id == 0)
            {
                id = IdGenerater.Instance.GenerateInstanceId();
                tService.Create(id, (IPEndPoint)endPoint);
                idEndpoints.Add(id, endPoint);
                channelIds.Enqueue(id);
            }

            MemoryBuffer memoryBuffer = tService.Fetch();
            memoryBuffer.Write(bytes, index, length);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            tService.Send(id, memoryBuffer);

            var timeNow = TimeInfo.Instance.ClientFrameTime();
            readWriteTime[id] = timeNow;
        }

        public int Recv(byte[] buffer, ref EndPoint endPoint)
        {
            return RecvNonAlloc(buffer, ref endPoint);
        }

        public int RecvNonAlloc(byte[] buffer, ref EndPoint endPoint)
        {
            (var e, var memoryBuffer) = channelRecvDatas.Dequeue();
            endPoint = e;
            var count = memoryBuffer.Read(buffer);
            tService.Recycle(memoryBuffer);
            return count;
        }

        public int Available()
        {
            return channelRecvDatas.Count;
        }

        public void Update()
        {
            // 检查长时间不读写的TChannel, 超时断开, 一次update检查10个
            var timeNow = TimeInfo.Instance.ClientFrameTime();
            const int MaxCheckNum = 10;
            var n = channelIds.Count < MaxCheckNum ? channelIds.Count : MaxCheckNum;
            for (var i = 0; i < n; ++i)
            {
                var id = channelIds.Dequeue();
                if (!readWriteTime.TryGetValue(id, out var rwTime))
                {
                    continue;
                }

                if (timeNow - rwTime > 30 * 1000)
                {
                    OnError(id, ErrorCore.ERR_KcpReadWriteTimeout);
                    continue;
                }

                channelIds.Enqueue(id);
            }

            tService.Update();
        }

        public void Dispose()
        {
            tService?.Dispose();
        }
    }
}