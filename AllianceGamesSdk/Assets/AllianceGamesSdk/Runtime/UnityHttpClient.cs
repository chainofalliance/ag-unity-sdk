
using AllianceGamesSdk.Common.Transport;
using Cysharp.Threading.Tasks;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AllianceGamesSdk.Unity
{
    public class UnityHttpClient : IHttpClient
    {
        private readonly UnityTransport transport = new UnityTransport();
        private HttpListener listener;


        public event Action<HttpListenerContext> OnRequest;

        public bool Start(string ip, int port, CancellationToken ct)
        {
            if (listener != null && listener.IsListening)
            {
                return false;
            }
            listener = new HttpListener();

            listener.Prefixes.Add($"http://{ip}:{port}/");
            try
            {
                listener.Start();
            }
            catch (Exception)
            {
                return false;
            }

            _ = UniTask.RunOnThreadPool(() => Listen(ct));
            return true;
        }

        private void Listen(CancellationToken ct)
        {
            try
            {
                while (listener.IsListening && !ct.IsCancellationRequested)
                {
                    var result = listener.BeginGetContext(ListenerCallback, listener);
                    result.AsyncWaitHandle.WaitOne();
                }
            }
            finally
            {
                listener?.Close();
                listener = null;
            }
        }

        private void ListenerCallback(IAsyncResult result)
        {
            var context = listener.EndGetContext(result);
            OnRequest?.Invoke(context);
        }

        public void Stop()
        {
            if (listener != null && listener.IsListening)
            {
                listener.Close();
                listener = null;
            }
        }

        public async Task<string> Get(Uri uri, CancellationToken ct)
        {
            return await transport.Get(uri, ct);
        }
    }
}