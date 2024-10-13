using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace GFPro
{
    public enum TcpOp
    {
        StartSend,
        StartRecv,
        Connect
    }

    public struct TArgs
    {
        public TcpOp                Op;
        public long                 ChannelId;
        public SocketAsyncEventArgs SocketAsyncEventArgs;
    }

    public sealed class TService : AService
    {
        private readonly Dictionary<long, TChannel> idChannels = new();

        private readonly SocketAsyncEventArgs innArgs = new();

        private Socket acceptor;

        public ConcurrentQueue<TArgs> Queue = new();

        public TService(AddressFamily addressFamily, ServiceType serviceType)
        {
            ServiceType = serviceType;
        }

        public TService(IPEndPoint ipEndPoint, ServiceType serviceType)
        {
            ServiceType = serviceType;
            acceptor = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // 容易出问题，先注释掉，按需开启
            //this.acceptor.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            innArgs.Completed += OnComplete;
            try
            {
                acceptor.Bind(ipEndPoint);
            }
            catch (Exception e)
            {
                throw new Exception($"bind error: {ipEndPoint}", e);
            }

            acceptor.Listen(1000);

            AcceptAsync();
        }

        private void OnComplete(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    Queue.Enqueue(new TArgs() { SocketAsyncEventArgs = e });
                    break;
                default:
                    throw new Exception($"socket error: {e.LastOperation}");
            }
        }

        private void OnAcceptComplete(SocketError socketError, Socket acceptSocket)
        {
            if (acceptor == null)
            {
                return;
            }

            if (socketError != SocketError.Success)
            {
                Log.Error($"accept error {socketError}");
                AcceptAsync();
                return;
            }

            try
            {
                long id = NetServices.Instance.CreateAcceptChannelId();
                var channel = new TChannel(id, acceptSocket, this);
                idChannels.Add(channel.Id, channel);
                var channelId = channel.Id;

                AcceptCallback(channelId, channel.RemoteAddress);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            // 开始新的accept
            AcceptAsync();
        }

        private void AcceptAsync()
        {
            innArgs.AcceptSocket = null;
            if (acceptor.AcceptAsync(innArgs))
            {
                return;
            }

            OnAcceptComplete(innArgs.SocketError, innArgs.AcceptSocket);
        }

        public override void Create(long id, IPEndPoint ipEndPoint)
        {
            if (idChannels.TryGetValue(id, out var _))
            {
                return;
            }

            TChannel channel = new(id, ipEndPoint, this);
            idChannels.Add(channel.Id, channel);
        }

        public TChannel Get(long id)
        {
            TChannel channel = null;
            idChannels.TryGetValue(id, out channel);
            return channel;
        }

        public override void Dispose()
        {
            base.Dispose();

            acceptor?.Close();
            acceptor = null;
            innArgs.Dispose();

            foreach (var id in idChannels.Keys.ToArray())
            {
                var channel = idChannels[id];
                channel.Dispose();
            }

            idChannels.Clear();
        }

        public override void Remove(long id, int error = 0)
        {
            if (idChannels.TryGetValue(id, out var channel))
            {
                channel.Error = error;
                channel.Dispose();
            }

            idChannels.Remove(id);
        }

        public override void Send(long channelId, MemoryBuffer memoryBuffer)
        {
            try
            {
                var aChannel = Get(channelId);
                if (aChannel == null)
                {
                    ErrorCallback(channelId, ErrorCore.ERR_SendMessageNotFoundTChannel);
                    return;
                }

                aChannel.Send(memoryBuffer);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public override void Update()
        {
            while (true)
            {
                if (!Queue.TryDequeue(out var result))
                {
                    break;
                }

                var e = result.SocketAsyncEventArgs;
                if (e == null)
                {
                    switch (result.Op)
                    {
                        case TcpOp.StartSend:
                        {
                            var tChannel = Get(result.ChannelId);
                            if (tChannel != null)
                            {
                                tChannel.StartSend();
                            }

                            break;
                        }
                        case TcpOp.StartRecv:
                        {
                            var tChannel = Get(result.ChannelId);
                            if (tChannel != null)
                            {
                                tChannel.StartRecv();
                            }

                            break;
                        }
                        case TcpOp.Connect:
                        {
                            var tChannel = Get(result.ChannelId);
                            if (tChannel != null)
                            {
                                tChannel.ConnectAsync();
                            }

                            break;
                        }
                    }

                    continue;
                }

                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Accept:
                    {
                        var socketError = e.SocketError;
                        var acceptSocket = e.AcceptSocket;
                        OnAcceptComplete(socketError, acceptSocket);
                        break;
                    }
                    case SocketAsyncOperation.Connect:
                    {
                        var tChannel = Get(result.ChannelId);
                        if (tChannel != null)
                        {
                            tChannel.OnConnectComplete(e);
                        }

                        break;
                    }
                    case SocketAsyncOperation.Disconnect:
                    {
                        var tChannel = Get(result.ChannelId);
                        if (tChannel != null)
                        {
                            tChannel.OnDisconnectComplete(e);
                        }

                        break;
                    }
                    case SocketAsyncOperation.Receive:
                    {
                        var tChannel = Get(result.ChannelId);
                        if (tChannel != null)
                        {
                            tChannel.OnRecvComplete(e);
                        }

                        break;
                    }
                    case SocketAsyncOperation.Send:
                    {
                        var tChannel = Get(result.ChannelId);
                        if (tChannel != null)
                        {
                            tChannel.OnSendComplete(e);
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException($"{e.LastOperation}");
                }
            }
        }

        public override bool IsDisposed()
        {
            return acceptor == null;
        }
    }
}