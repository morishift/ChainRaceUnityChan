// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using ChainPattern;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace Sample
{
    /// <summary>
    /// Chain that plays a voice clip via SoundPlayer and completes when it finishes
    /// </summary>
    public class ChainPlayVoice : Chain
    {
        string voiceId;
        int audioSourceIndex = -1;
        CancellationTokenSource cts;

        public ChainPlayVoice(string voiceId)
        {
            this.voiceId = voiceId;
        }

        protected override void StartInternal()
        {
            if (isFastForward)
            {
                Complete();
                return;
            }
            var soundPlayer = SoundPlayer.Get();
            float length = UnityChanVoiceList.LoadAudioClip(voiceId).length;
            audioSourceIndex = soundPlayer.PlayAudioClip(UnityChanVoiceList.LoadAudioClip(voiceId));
            cts = new CancellationTokenSource();
            WaitAsync(length, cts.Token).Forget();
        }

        protected override void SkipInternal()
        {
            cts?.Cancel();
            if (audioSourceIndex >= 0)
            {
                SoundPlayer.Get()?.Stop(audioSourceIndex);
                audioSourceIndex = -1;
            }
        }

        private async UniTask WaitAsync(float seconds, CancellationToken token)
        {
            try
            {
                await UniTask.Delay((int)(seconds * 1000), cancellationToken: token);
                Complete();
            }
            catch (OperationCanceledException) { }
            finally
            {
                cts?.Dispose();
                cts = null;
            }
        }
    }
}
