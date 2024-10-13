using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;

namespace GFPro
{
    public class WService : AService
    {
        private long idGenerater = 200000000;

        private HttpListener httpListener;

        private readonly Dictionary<long, WChannel> channels = new();

        public ThreadSynchronizationContext ThreadSynchronizationContext;

        public WService(IEnumerable<string> prefixs)
        {
            ThreadSynchronizationContext = new ThreadSynchronizationContext();

            httpListener = new HttpListener();

            StartAccept(prefixs).Coroutine();
        }

        public WService()
        {
            this.ServiceType = ServiceType.Outer;
            ThreadSynchronizationContext = new ThreadSynchronizationContext();
        }

        private long GetId => ++idGenerater;

        public override void Create(long id, IPEndPoint ipEndpoint)
        {
            ClientWebSocket webSocket = new();
            WChannel channel = new(id, webSocket, ipEndpoint, this);
            channels[channel.Id] = channel;
        }

        public override void Update()
        {
            ThreadSynchronizationContext.Update();
        }

        public override void Remove(long id, int error = 0)
        {
            WChannel channel;
            if (!channels.TryGetValue(id, out channel))
            {
                return;
            }

            channel.Error = error;

            channels.Remove(id);
            channel.Dispose();
        }

        public override bool IsDisposed()
        {
            return ThreadSynchronizationContext == null;
        }

        protected void Get(long id, IPEndPoint ipEndPoint)
        {
            if (!channels.TryGetValue(id, out _))
            {
                Create(id, ipEndPoint);
            }
        }

        public WChannel Get(long id)
        {
            WChannel channel = null;
            channels.TryGetValue(id, out channel);
            return channel;
        }

        public override void Dispose()
        {
            base.Dispose();

            ThreadSynchronizationContext = null;
            httpListener?.Close();
            httpListener = null;
        }

        private async ETTask StartAccept(IEnumerable<string> prefixs)
        {
            try
            {
                foreach (var prefix in prefixs)
                {
                    httpListener.Prefixes.Add(prefix);
                }

                httpListener.Start();

                while (true)
                {
                    try
                    {
                        var httpListenerContext = await httpListener.GetContextAsync();

                        var webSocketContext = await httpListenerContext.AcceptWebSocketAsync(null);

                        var channel = new WChannel(GetId, webSocketContext, this);
                        channel.RemoteAddress = httpListenerContext.Request.RemoteEndPoint;
                        channels[channel.Id] = channel;

                        this.AcceptCallback(channel.Id, channel.RemoteAddress);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
            catch (HttpListenerException e)
            {
                if (e.ErrorCode == 5)
                {
                    throw new Exception($"CMD管理员中输入: netsh http add urlacl url=http://*:8080/ user=Everyone   {prefixs.ToList().ListToString()}", e);
                }

                Log.Error(e);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public override void Send(long channelId, MemoryBuffer memoryBuffer)
        {
            channels.TryGetValue(channelId, out var channel);
            if (channel == null)
            {
                return;
            }

            channel.Send(memoryBuffer);
        }
    }
}