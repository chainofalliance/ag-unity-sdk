
using AllianceGamesSdk.Common;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AllianceGamesSdk.Unity
{
    public class UniTaskRunner : ITaskRunner
    {
        public async Task Delay(int milliseconds, CancellationToken cancellationToken)
        {
            await UniTask.Delay(milliseconds, cancellationToken: cancellationToken);
        }

        public async IAsyncEnumerable<T> Yield<T>(
            System.Threading.Channels.ChannelReader<T> reader,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                yield return await reader.ReadAsync(cancellationToken).AsUniTask();
            }
        }
    }
}