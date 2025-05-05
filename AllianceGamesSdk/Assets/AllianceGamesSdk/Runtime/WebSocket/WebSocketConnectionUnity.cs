using AllianceGamesSdk.Common.Transport;
using Cysharp.Threading.Tasks;
using Serilog;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace AllianceGamesSdk.Transport.Unity
{
    internal class WebSocketConnection : ITransportConnection
    {
        private IWebSocket webSocket;
        private readonly ILogger logger;
        private readonly System.Threading.Channels.Channel<byte[]> messageChannel
            = System.Threading.Channels.Channel.CreateUnbounded<byte[]>();

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        internal WebSocketConnection(IWebSocket webSocket, ILogger logger)
        {
            this.webSocket = webSocket;
            this.logger = logger;
            webSocket.OnMessage += data => messageChannel.Writer.WriteAsync(data);
            webSocket.OnClose += (code, reason) =>
            {
                logger?.Information("[Unity] WebSocketConnection: Closed with code {Code} and reason {Reason}", code, reason);
            };
            webSocket.OnError += (message) =>
            {
                logger?.Error("[Unity] WebSocketConnection: Error {Message}", message);
            };
        }

        public async Task Send(byte[] message, CancellationToken ct)
        {
            if (webSocket == null || webSocket.ReadyState != WebSocketState.Open)
            {
                logger?.Error($"Cannot send on closed socket.");
                return;
            }

            try
            {
                await semaphore.WaitAsync(ct).AsUniTask();
                webSocket.Send(message);
            }
            catch (Exception e)
            {
                logger?.Error(e, $"Error while running send task for WebSocket.");
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async IAsyncEnumerable<byte[]> Receive([EnumeratorCancellation] CancellationToken ct)
        {
            while (webSocket != null && webSocket.ReadyState != WebSocketState.Closed)
            {
                byte[] message = null;
                try
                {
                    message = await messageChannel.Reader.ReadAsync(ct).AsUniTask();
                }
                catch (OperationCanceledException)
                {
                    await Disconnect(default);
                }
                catch (Exception e)
                {
                    logger?.Error(e, $"Error while receiving message on WebSocket.");
                }

                if (message == null)
                {
                    yield return null;
                    break;
                }

                yield return message;
            }

            if (webSocket != null)
            {
                try
                {
                    if (webSocket.ReadyState != WebSocketState.Closed)
                    {
                        await webSocket.CloseAsync();
                    }
                    webSocket = null;
                }
                catch (ObjectDisposedException)
                { }
            }
        }

        public async Task Disconnect(CancellationToken ct)
        {
            if (webSocket == null)
            {
                return;
            }

            try
            {
                try
                {
                    if (webSocket.ReadyState == WebSocketState.Closed)
                    {
                        return;
                    }

                    await webSocket.CloseAsync();
                }
                catch (OperationCanceledException)
                { }
                finally
                {
                    webSocket = null;
                }
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception e)
            {
                logger?.Error(e, $"Error while disconnecting from WebSocket.");
            }
        }

        public override bool Equals(object obj)
        {
            return (obj is WebSocketConnection c) && webSocket.Equals(c.webSocket);
        }

        public override int GetHashCode()
        {
            return webSocket.GetHashCode();
        }
    }
}