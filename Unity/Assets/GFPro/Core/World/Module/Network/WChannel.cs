using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace GFPro
{
    public class WChannel : AChannel
    {
        public HttpListenerWebSocketContext WebSocketContext { get; }

        private readonly WService Service;

        private readonly WebSocket webSocket;

        private readonly Queue<MemoryBuffer> queue = new();

        private bool isSending;

        private bool isConnected;

        private CancellationTokenSource cancellationTokenSource = new();

        public WChannel(long id, HttpListenerWebSocketContext webSocketContext, WService service)
        {
            Service = service;
            Id = id;
            ChannelType = ChannelType.Accept;
            WebSocketContext = webSocketContext;
            webSocket = webSocketContext.WebSocket;

            isConnected = true;

            Service.ThreadSynchronizationContext.Post(() =>
            {
                StartRecv().Coroutine();
                StartSend().Coroutine();
            });
        }

        public WChannel(long id, WebSocket webSocket, IPEndPoint ipEndPoint, WService service)
        {
            Service = service;
            Id = id;
            ChannelType = ChannelType.Connect;
            this.webSocket = webSocket;

            isConnected = false;

            Service.ThreadSynchronizationContext.Post(() => ConnectAsync($"ws://{ipEndPoint}").Coroutine());
        }

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;

            webSocket.Dispose();
        }

        private async ETTask ConnectAsync(string url)
        {
            try
            {
                await ((ClientWebSocket)webSocket).ConnectAsync(new Uri(url), cancellationTokenSource.Token);
                isConnected = true;

                StartRecv().Coroutine();
                StartSend().Coroutine();
            }
            catch (Exception e)
            {
                Log.Error(e);
                OnError(ErrorCore.ERR_WebsocketConnectError);
            }
        }

        public void Send(MemoryBuffer memoryBuffer)
        {
            queue.Enqueue(memoryBuffer);

            if (isConnected)
            {
                StartSend().Coroutine();
            }
        }

        private async ETTask StartSend()
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                if (isSending)
                {
                    return;
                }

                isSending = true;

                while (true)
                {
                    if (queue.Count == 0)
                    {
                        isSending = false;
                        return;
                    }

                    var stream = queue.Dequeue();

                    try
                    {
                        await webSocket.SendAsync(stream.GetMemory(), WebSocketMessageType.Binary, true, cancellationTokenSource.Token);

                        Service.Recycle(stream);

                        if (IsDisposed)
                        {
                            return;
                        }
                    }
                    catch (TaskCanceledException e)
                    {
                        Log.Warning(e.ToString());
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        OnError(ErrorCore.ERR_WebsocketSendError);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private readonly byte[] cache = new byte[ushort.MaxValue];

        public async ETTask StartRecv()
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                while (true)
                {
                    ValueWebSocketReceiveResult receiveResult;
                    var receiveCount = 0;
                    do
                    {
                        receiveResult = await webSocket.ReceiveAsync(
                            new Memory<byte>(cache, receiveCount, cache.Length - receiveCount),
                            cancellationTokenSource.Token);
                        if (IsDisposed)
                        {
                            return;
                        }

                        receiveCount += receiveResult.Count;
                    } while (!receiveResult.EndOfMessage);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        OnError(ErrorCore.ERR_WebsocketPeerReset);
                        return;
                    }

                    if (receiveResult.Count > ushort.MaxValue)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig, $"message too big: {receiveCount}",
                            cancellationTokenSource.Token);
                        OnError(ErrorCore.ERR_WebsocketMessageTooBig);
                        return;
                    }

                    MemoryBuffer memoryBuffer = Service.Fetch(receiveCount);
                    memoryBuffer.SetLength(receiveCount);
                    memoryBuffer.Seek(0, SeekOrigin.Begin);
                    Array.Copy(cache, 0, memoryBuffer.GetBuffer(), 0, receiveCount);
                    OnRead(memoryBuffer);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                OnError(ErrorCore.ERR_WebsocketRecvError);
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
            Log.Info($"WChannel error: {error} {RemoteAddress}");

            var channelId = Id;

            Service.Remove(channelId);

            Service.ErrorCallback(channelId, error);
        }
    }
}