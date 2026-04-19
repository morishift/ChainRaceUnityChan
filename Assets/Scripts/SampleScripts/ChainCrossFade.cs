
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using ChainPattern;

namespace Sample
{
    /// <summary>
    /// Chain that triggers an Animator CrossFade and completes when the state reaches
    /// (stateDuration - endMargin) seconds from the start.
    /// </summary>
    public class ChainCrossFade : Chain
    {
        Animator animator;
        string stateName;
        float crossFadeDuration;
        float endMargin;
        int layer;
        CancellationTokenSource cts;

        /// <summary>
        /// Creates a ChainCrossFade.
        /// </summary>
        /// <param name="animator">Target Animator</param>
        /// <param name="stateName">Destination state name</param>
        /// <param name="crossFadeDuration">CrossFade blend duration in seconds</param>
        /// <param name="endMargin">Seconds before the end of the state at which this chain completes</param>
        /// <param name="layer">Animator layer index</param>
        public ChainCrossFade(Animator animator, string stateName, float crossFadeDuration, float endMargin, int layer = 0)
        {
            this.animator = animator;
            this.stateName = stateName;
            this.crossFadeDuration = crossFadeDuration;
            this.endMargin = endMargin;
            this.layer = layer;
        }

        protected override void StartInternal()
        {
            if (animator == null)
            {
                Complete();
                return;
            }
            if (isFastForward)
            {
                JumpToEnd();
                Complete();
                return;
            }

            Debug.Log($"CrossFade stateName:{stateName} crossFadeDuration:{crossFadeDuration}");
            animator.CrossFadeInFixedTime(stateName, crossFadeDuration, layer);
            cts = new CancellationTokenSource();
            WaitAsync(cts.Token).Forget();
        }

        protected override void SkipInternal()
        {
            cts?.Cancel();
            JumpToEnd();
        }

        // 対象ステートの終了マージン位置に即ジャンプ
        void JumpToEnd()
        {
            // Play で強制遷移してから Update(0) で StateInfo を確定させる
            animator.Play(stateName, layer, 0f);
            animator.Update(0f);
            var info = animator.GetCurrentAnimatorStateInfo(layer);
            float normalizedEnd = info.length > 0f ? Mathf.Clamp01(1f - endMargin / info.length) : 1f;
            animator.Play(stateName, layer, normalizedEnd);
            animator.Update(0f);
        }

        async UniTask WaitAsync(CancellationToken token)
        {
            try
            {
                // Phase 1: CrossFade のトランジションが終わり対象ステートに入るまで待つ
                while (animator.IsInTransition(layer) ||
                       !animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName))
                {
                    // Debug.Log($"WaitTransition stateName:{stateName} Time.time:{Time.time}");
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                // Phase 2: normalizedTime が終了マージン位置に達するまで待つ
                while (true)
                {
                    var info = animator.GetCurrentAnimatorStateInfo(layer);
                    //Debug.Log($"info.length:{info.length}");
                    float normalizedEnd = info.length > 0f ? Mathf.Clamp01(1f - endMargin / info.length) : 1f;
                    //Debug.Log($"Time.time:{Time.time} progress stateName:{stateName} info.normalizedTime:{info.normalizedTime} normalizedEnd:{normalizedEnd}");
                    if (info.normalizedTime >= normalizedEnd) break;
                    //Debug.Log($"normalizedEnd:{normalizedEnd}");
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                Debug.Log($"END stateName:{stateName}");

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
