using System;
using System.Net;
using System.Net.Sockets;

namespace GFPro
{
    /// <summary>
    /// 封装Socket,将回调push到主线程处理
    /// </summary>
    public sealed class TChannel : AChannel
    {
        private readonly TService             Service;
        private          Socket               socket;
        private          SocketAsyncEventArgs innArgs = new();
        private          SocketAsyncEventArgs outArgs = new();

        private readonly CircularBuffer recvBuffer = new();
        private readonly CircularBuffer sendBuffer = new();

        private bool isSending;

        private bool isConnected;

        private readonly PacketParser parser;

        private readonly byte[] sendCache = new byte[Packet.OpcodeLength + Packet.ActorIdLength];

        private void OnComplete(object sender, SocketAsyncEventArgs e)
        {
            Service.Queue.Enqueue(new TArgs() { ChannelId = Id, SocketAsyncEventArgs = e });
        }

        public TChannel(long id, IPEndPoint ipEndPoint, TService service)
        {
            Service = service;
            ChannelType = ChannelType.Connect;
            Id = id;
            socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            parser = new PacketParser(recvBuffer, Service);
            innArgs.Completed += OnComplete;
            outArgs.Completed += OnComplete;

            RemoteAddress = ipEndPoint;
            isConnected = false;
            isSending = false;

            Service.Queue.Enqueue(new TArgs() { Op = TcpOp.Connect, ChannelId = Id });
        }

        public TChannel(long id, Socket socket, TService service)
        {
            Service = service;
            ChannelType = ChannelType.Accept;
            Id = id;
            this.socket = socket;
            this.socket.NoDelay = true;
            parser = new PacketParser(recvBuffer, Service);
            innArgs.Completed += OnComplete;
            outArgs.Completed += OnComplete;

            RemoteAddress = (IPEndPoint)socket.RemoteEndPoint;
            isConnected = true;
            isSending = false;

            Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = Id });
            Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartRecv, ChannelId = Id });
        }


        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Log.Info($"channel dispose: {Id} {RemoteAddress} {Error}");

            var id = Id;
            Id = 0;
            Service.Remove(id);
            socket.Close();
            innArgs.Dispose();
            outArgs.Dispose();
            innArgs = null;
            outArgs = null;
            socket = null;
        }

        public void Send(MemoryBuffer stream)
        {
            if (IsDisposed)
            {
                throw new Exception("TChannel已经被Dispose, 不能发送消息");
            }

            switch (Service.ServiceType)
            {
                case ServiceType.Inner:
                {
                    var messageSize = (int)(stream.Length - stream.Position);
                    if (messageSize > ushort.MaxValue * 16)
                    {
                        throw new Exception($"send packet too large: {stream.Length} {stream.Position}");
                    }

                    sendCache.WriteTo(0, messageSize);
                    sendBuffer.Write(sendCache, 0, PacketParser.InnerPacketSizeLength);
                    break;
                }
                case ServiceType.Outer:
                {
                    var messageSize = (ushort)(stream.Length - stream.Position);
                    sendCache.WriteTo(0, messageSize);
                    sendBuffer.Write(sendCache, 0, PacketParser.OuterPacketSizeLength);
                    break;
                }
            }

            sendBuffer.Write(stream.GetBuffer(), (int)stream.Position, (int)(stream.Length - stream.Position));
            if (!isSending)
            {
                Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = Id });
            }

            Service.Recycle(stream);
        }

        public void ConnectAsync()
        {
            outArgs.RemoteEndPoint = RemoteAddress;
            if (socket.ConnectAsync(outArgs))
            {
                return;
            }

            OnConnectComplete(outArgs);
        }

        public void OnConnectComplete(SocketAsyncEventArgs e)
        {
            if (socket == null)
            {
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                OnError((int)e.SocketError);
                return;
            }

            e.RemoteEndPoint = null;
            isConnected = true;

            Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = Id });
            Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartRecv, ChannelId = Id });
        }

        public void OnDisconnectComplete(SocketAsyncEventArgs e)
        {
            OnError((int)e.SocketError);
        }

        public void StartRecv()
        {
            while (true)
            {
                try
                {
                    if (socket == null)
                    {
                        return;
                    }

                    var size = recvBuffer.ChunkSize - recvBuffer.LastIndex;
                    innArgs.SetBuffer(recvBuffer.Last, recvBuffer.LastIndex, size);
                }
                catch (Exception e)
                {
                    Log.Error($"tchannel error: {Id}\n{e}");
                    OnError(ErrorCore.ERR_TChannelRecvError);
                    return;
                }

                if (socket.ReceiveAsync(innArgs))
                {
                    return;
                }

                HandleRecv(innArgs);
            }
        }

        public void OnRecvComplete(SocketAsyncEventArgs o)
        {
            HandleRecv(o);

            if (socket == null)
            {
                return;
            }

            Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartRecv, ChannelId = Id });
        }

        private void HandleRecv(SocketAsyncEventArgs e)
        {
            if (socket == null)
            {
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                OnError((int)e.SocketError);
                return;
            }

            if (e.BytesTransferred == 0)
            {
                OnError(ErrorCore.ERR_PeerDisconnect);
                return;
            }

            recvBuffer.LastIndex += e.BytesTransferred;
            if (recvBuffer.LastIndex == recvBuffer.ChunkSize)
            {
                recvBuffer.AddLast();
                recvBuffer.LastIndex = 0;
            }

            // 收到消息回调
            while (true)
            {
                // 这里循环解析消息执行，有可能，执行消息的过程中断开了session
                if (socket == null)
                {
                    return;
                }

                try
                {
                    if (recvBuffer.Length == 0)
                    {
                        break;
                    }

                    var ret = parser.Parse(out var memoryBuffer);
                    if (!ret)
                    {
                        break;
                    }

                    OnRead(memoryBuffer);
                }
                catch (Exception ee)
                {
                    Log.Error($"ip: {RemoteAddress} {ee}");
                    OnError(ErrorCore.ERR_SocketError);
                    return;
                }
            }
        }

        public void StartSend()
        {
            if (!isConnected)
            {
                return;
            }

            if (isSending)
            {
                return;
            }

            while (true)
            {
                try
                {
                    if (socket == null)
                    {
                        isSending = false;
                        return;
                    }

                    // 没有数据需要发送
                    if (sendBuffer.Length == 0)
                    {
                        isSending = false;
                        return;
                    }

                    isSending = true;

                    var sendSize = sendBuffer.ChunkSize - sendBuffer.FirstIndex;
                    if (sendSize > sendBuffer.Length)
                    {
                        sendSize = (int)sendBuffer.Length;
                    }

                    outArgs.SetBuffer(sendBuffer.First, sendBuffer.FirstIndex, sendSize);

                    if (socket.SendAsync(outArgs))
                    {
                        return;
                    }

                    HandleSend(outArgs);
                }
                catch (Exception e)
                {
                    throw new Exception($"socket set buffer error: {sendBuffer.First.Length}, {sendBuffer.FirstIndex}", e);
                }
            }
        }

        public void OnSendComplete(SocketAsyncEventArgs o)
        {
            HandleSend(o);

            isSending = false;

            Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = Id });
        }

        private void HandleSend(SocketAsyncEventArgs e)
        {
            if (socket == null)
            {
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                OnError((int)e.SocketError);
                return;
            }

            if (e.BytesTransferred == 0)
            {
                OnError(ErrorCore.ERR_PeerDisconnect);
                return;
            }

            sendBuffer.FirstIndex += e.BytesTransferred;
            if (sendBuffer.FirstIndex == sendBuffer.ChunkSize)
            {
                sendBuffer.FirstIndex = 0;
                sendBuffer.RemoveFirst();
            }
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

        private void OnError(int error)
        {
            Log.Info($"TChannel OnError: {error} {RemoteAddress}");

            var channelId = Id;

            Service.Remove(channelId);

            Service.ErrorCallback(channelId, error);
        }
    }
}