using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace GFPro
{
    public class KChannel : AChannel
    {
        private const int MaxKcpMessageSize = 10000;

        private readonly KService Service;

        private Kcp kcp { get; set; }

        private readonly Queue<MemoryBuffer> waitSendMessages = new();

        public readonly uint CreateTime;

        public uint LocalConn
        {
            get => (uint)Id;
            private set => Id = value;
        }

        public uint RemoteConn { get; set; }

        private readonly byte[] sendCache = new byte[2 * 1024];

        public bool IsConnected { get; set; }

        public string RealAddress { get; set; }

        private MemoryBuffer readMemory;
        private int          needReadSplitCount;

        private void InitKcp()
        {
            switch (Service.ServiceType)
            {
                case ServiceType.Inner:
                    kcp.SetNoDelay(1, 10, 2, 1);
                    kcp.SetWindowSize(1024, 1024);
                    kcp.SetMtu(1400); // 默认1400
                    kcp.SetMinrto(30);
                    break;
                case ServiceType.Outer:
                    kcp.SetNoDelay(1, 10, 2, 1);
                    kcp.SetWindowSize(256, 256);
                    kcp.SetMtu(470);
                    kcp.SetMinrto(30);
                    break;
            }
        }

        // connect
        public KChannel(uint localConn, IPEndPoint remoteEndPoint, KService kService)
        {
            Service = kService;
            LocalConn = localConn;
            ChannelType = ChannelType.Connect;

            Log.Info($"channel create: {LocalConn} {remoteEndPoint} {ChannelType}");


            RemoteAddress = remoteEndPoint;
            CreateTime = kService.TimeNow;

            Connect(CreateTime);
        }

        // accept
        public KChannel(uint localConn, uint remoteConn, IPEndPoint remoteEndPoint, KService kService)
        {
            Service = kService;
            ChannelType = ChannelType.Accept;

            Log.Info($"channel create: {localConn} {remoteConn} {remoteEndPoint} {ChannelType}");
            LocalConn = localConn;
            RemoteConn = remoteConn;
            RemoteAddress = remoteEndPoint;
            kcp = new Kcp(RemoteConn, Output);
            InitKcp();

            CreateTime = kService.TimeNow;
        }


        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            var localConn = LocalConn;
            var remoteConn = RemoteConn;
            Log.Info($"channel dispose: {localConn} {remoteConn} {Error}");

            var id = Id;
            Id = 0;
            Service.Remove(id);

            try
            {
                if (Error != ErrorCore.ERR_PeerDisconnect)
                {
                    Service.Disconnect(localConn, remoteConn, Error, RemoteAddress, 3);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            kcp = null;
        }

        public void HandleConnnect()
        {
            // 如果连接上了就不用处理了
            if (IsConnected)
            {
                return;
            }

            kcp = new Kcp(RemoteConn, Output);
            InitKcp();

            Log.Info($"channel connected: {LocalConn} {RemoteConn} {RemoteAddress}");
            IsConnected = true;

            while (true)
            {
                if (waitSendMessages.Count <= 0)
                {
                    break;
                }

                var buffer = waitSendMessages.Dequeue();
                Send(buffer);
            }
        }

        private long lastConnectTime = long.MaxValue;

        /// <summary>
        /// 发送请求连接消息
        /// </summary>
        private void Connect(uint timeNow)
        {
            try
            {
                if (IsConnected)
                {
                    return;
                }

                // 300毫秒后再次update发送connect请求
                if (timeNow < lastConnectTime + 300)
                {
                    Service.AddToUpdate(300, Id);
                    return;
                }

                // 10秒连接超时
                if (timeNow > CreateTime + KService.ConnectTimeoutTime)
                {
                    Log.Error($"kChannel connect timeout: {Id} {RemoteConn} {timeNow} {CreateTime} {ChannelType} {RemoteAddress}");
                    OnError(ErrorCore.ERR_KcpConnectTimeout);
                    return;
                }

                var buffer = sendCache;
                buffer.WriteTo(0, KcpProtocalType.SYN);
                buffer.WriteTo(1, LocalConn);
                buffer.WriteTo(5, RemoteConn);
                Service.Transport.Send(buffer, 0, 9, RemoteAddress);
                // 这里很奇怪 调用socket.LocalEndPoint会动到this.RemoteAddressNonAlloc里面的temp，这里就不仔细研究了
                Log.Info($"kchannel connect {LocalConn} {RemoteConn} {RealAddress}");

                lastConnectTime = timeNow;

                Service.AddToUpdate(300, Id);
            }
            catch (Exception e)
            {
                Log.Error(e);
                OnError(ErrorCore.ERR_SocketCantSend);
            }
        }

        public void Update(uint timeNow)
        {
            if (IsDisposed)
            {
                return;
            }

            // 如果还没连接上，发送连接请求
            if (!IsConnected && ChannelType == ChannelType.Connect)
            {
                Connect(timeNow);
                return;
            }

            if (kcp == null)
            {
                return;
            }

            try
            {
                kcp.Update(timeNow);
            }
            catch (Exception e)
            {
                Log.Error(e);
                OnError(ErrorCore.ERR_SocketError);
                return;
            }

            var nextUpdateTime = kcp.Check(timeNow);
            Service.AddToUpdate(nextUpdateTime, Id);
        }

        public void HandleRecv(byte[] date, int offset, int length)
        {
            if (IsDisposed)
            {
                return;
            }

            kcp.Input(date.AsSpan(offset, length));
            Service.AddToUpdate(0, Id);
            while (true)
            {
                if (IsDisposed)
                {
                    break;
                }

                var n = kcp.PeekSize();
                if (n < 0)
                {
                    break;
                }

                if (n == 0)
                {
                    OnError((int)SocketError.NetworkReset);
                    return;
                }

                if (needReadSplitCount > 0) // 说明消息分片了
                {
                    var buffer = readMemory.GetBuffer();
                    var count = kcp.Receive(buffer.AsSpan((int)(readMemory.Length - needReadSplitCount), n));
                    needReadSplitCount -= count;
                    if (n != count)
                    {
                        Log.Error($"kchannel read error1: {LocalConn} {RemoteConn}");
                        OnError(ErrorCore.ERR_KcpReadNotSame);
                        return;
                    }

                    if (needReadSplitCount < 0)
                    {
                        Log.Error($"kchannel read error2: {LocalConn} {RemoteConn}");
                        OnError(ErrorCore.ERR_KcpSplitError);
                        return;
                    }

                    // 没有读完
                    if (needReadSplitCount != 0)
                    {
                        continue;
                    }
                }
                else
                {
                    readMemory = Service.Fetch(n);
                    readMemory.SetLength(n);
                    readMemory.Seek(0, SeekOrigin.Begin);

                    var buffer = readMemory.GetBuffer();

                    var count = kcp.Receive(buffer.AsSpan(0, n));
                    if (n != count)
                    {
                        break;
                    }

                    // 判断是不是分片
                    if (n == 8)
                    {
                        var headInt = BitConverter.ToInt32(readMemory.GetBuffer(), 0);
                        if (headInt == 0)
                        {
                            needReadSplitCount = BitConverter.ToInt32(readMemory.GetBuffer(), 4);
                            if (needReadSplitCount <= MaxKcpMessageSize)
                            {
                                Log.Error($"kchannel read error3: {needReadSplitCount} {LocalConn} {RemoteConn}");
                                OnError(ErrorCore.ERR_KcpSplitCountError);
                                return;
                            }

                            readMemory.SetLength(needReadSplitCount);
                            readMemory.Seek(0, SeekOrigin.Begin);
                            continue;
                        }
                    }
                }

                var memoryBuffer = readMemory;
                readMemory = null;

                memoryBuffer.Seek(0, SeekOrigin.Begin);

                OnRead(memoryBuffer);
            }
        }

        private void Output(byte[] bytes, int count)
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                // 没连接上 kcp不往外发消息, 其实本来没连接上不会调用update，这里只是做一层保护
                if (!IsConnected)
                {
                    return;
                }

                if (count == 0)
                {
                    Log.Error($"output 0");
                    return;
                }

                bytes.WriteTo(0, KcpProtocalType.MSG);
                // 每个消息头部写下该channel的id;
                bytes.WriteTo(1, LocalConn);
                Service.Transport.Send(bytes, 0, count + 5, RemoteAddress);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void KcpSend(MemoryBuffer memoryStream)
        {
            if (IsDisposed)
            {
                return;
            }

            var count = (int)(memoryStream.Length - memoryStream.Position);

            // 超出maxPacketSize需要分片

            if (count <= MaxKcpMessageSize)
            {
                kcp.Send(memoryStream.GetBuffer().AsSpan((int)memoryStream.Position, count));
            }
            else
            {
                // 先发分片信息
                sendCache.WriteTo(0, 0);
                sendCache.WriteTo(4, count);
                kcp.Send(sendCache.AsSpan(0, 8));

                // 分片发送
                var alreadySendCount = 0;
                while (alreadySendCount < count)
                {
                    var leftCount = count - alreadySendCount;

                    var sendCount = leftCount < MaxKcpMessageSize ? leftCount : MaxKcpMessageSize;

                    kcp.Send(memoryStream.GetBuffer().AsSpan((int)memoryStream.Position + alreadySendCount, sendCount));

                    alreadySendCount += sendCount;
                }
            }

            Service.AddToUpdate(0, Id);
        }

        public void Send(MemoryBuffer memoryBuffer)
        {
            if (!IsConnected)
            {
                waitSendMessages.Enqueue(memoryBuffer);
                return;
            }

            if (kcp == null)
            {
                throw new Exception("kchannel connected but kcp is zero!");
            }

            // 检查等待发送的消息，如果超出最大等待大小，应该断开连接
            var n = (int)kcp.WaitSendCount;
            var maxWaitSize = 0;
            switch (Service.ServiceType)
            {
                case ServiceType.Inner:
                    maxWaitSize = Kcp.InnerMaxWaitSize;
                    break;
                case ServiceType.Outer:
                    maxWaitSize = Kcp.OuterMaxWaitSize;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (n > maxWaitSize)
            {
                Log.Error($"kcp wait snd too large: {n}: {LocalConn} {RemoteConn}");
                OnError(ErrorCore.ERR_KcpWaitSendSizeTooLarge);
                return;
            }

            KcpSend(memoryBuffer);
            Service.Recycle(memoryBuffer);
        }

        private void OnRead(MemoryBuffer memoryStream)
        {
            try
            {
                Service.ReadCallback(Id, memoryStream);
            }
            catch (Exception e)
            {
                Log.Error(e);
                OnError(ErrorCore.ERR_PacketParserError);
            }
        }

        public void OnError(int error)
        {
            var channelId = Id;
            Service.Remove(channelId, error);
            Service.ErrorCallback(channelId, error);
        }
    }
}