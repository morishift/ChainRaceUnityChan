// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace ChainPattern
{
    /// <summary>
    /// Chain that waits for a specified duration
    /// </summary>
    public class ChainDelay : Chain
    {
        float delaySeconds;
        CancellationTokenSource cts;

        /// <summary>
        /// Creates a delay chain with duration in seconds
        /// </summary>
        public ChainDelay(float seconds)
        {
            delaySeconds = seconds;
        }

        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal()
        {
            if (isFastForward)
            {
                // Do nothing if it will be skipped immediately
                Complete();
                return;
            }
            cts = new CancellationTokenSource();
            DelayAsync(cts.Token).Forget();
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }
        }

        /// <summary>
        /// Execute UniTask.Delay
        /// </summary>
        private async UniTask DelayAsync(CancellationToken token)
        {
            try
            {
                await UniTask.Delay((int)(delaySeconds * 1000), cancellationToken: token);
                Complete();
            }
            catch (OperationCanceledException)
            {
                // Executed when canceled                
            }
            finally
            {
                // Dispose of resources after completion or cancellation
                cts?.Dispose();
                cts = null;
            }
        }
    }
}
