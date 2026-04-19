// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using ChainPattern;

namespace Sample
{
    /// <summary>
    /// Chain that moves Unity-chan to a target world position,
    /// driving the Animator's "Speed" parameter during the move.
    /// Movement follows a trapezoidal velocity profile:
    /// accelerates up to MaxSpeed, cruises, then decelerates.
    /// Only XZ plane movement; Y is preserved.
    /// </summary>
    public class ChainForward : Chain
    {
        Animator animator;
        Vector3 targetPosition;
        float maxSpeed;        // m/s
        float acceleration;    // m/s²
        CancellationTokenSource cts;

        /// <summary>
        /// Creates a ChainForward.
        /// </summary>
        /// <param name="animator">Unity-chan's Animator</param>
        /// <param name="targetPosition">World position to move to</param>
        /// <param name="maxSpeed">Maximum movement speed in meters/second</param>
        /// <param name="acceleration">Acceleration/deceleration in meters/second²</param>
        public ChainForward(Animator animator, Vector3 targetPosition, float maxSpeed = 1.0f, float acceleration = 2.0f)
        {
            this.animator = animator;
            this.targetPosition = targetPosition;
            this.maxSpeed = maxSpeed;
            this.acceleration = acceleration;
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
                ApplyFinalState();
                Complete();
                return;
            }
            cts = new CancellationTokenSource();
            MoveAsync(cts.Token).Forget();
        }

        protected override void SkipInternal()
        {
            cts?.Cancel();
            ApplyFinalState();
        }

        void ApplyFinalState()
        {
            if (animator == null) return;
            var t = animator.transform;
            t.position = new Vector3(targetPosition.x, t.position.y, targetPosition.z);
            animator.SetFloat("Speed", 0f);
            animator.Update(0.0f);
        }

        async UniTask MoveAsync(CancellationToken token)
        {
            try
            {
                var t = animator.transform;
                Vector3 startPos = new Vector3(t.position.x, 0f, t.position.z);
                Vector3 endPos   = new Vector3(targetPosition.x, 0f, targetPosition.z);
                Vector3 delta    = endPos - startPos;
                float totalDist  = delta.magnitude;

                if (totalDist < 0.001f)
                {
                    ApplyFinalState();
                    Complete();
                    return;
                }

                Vector3 dir = delta / totalDist;

                // 台形速度プロファイルを計算
                // 加速フェーズで到達できる最大距離 = v² / (2a)
                float accelDist = 0.5f * maxSpeed * maxSpeed / acceleration;
                float accelTime, peakVelocity, totalTime;

                if (totalDist >= 2f * accelDist)
                {
                    // 台形: 加速 → 定速 → 減速
                    peakVelocity = maxSpeed;
                    accelTime = peakVelocity / acceleration;
                    float cruiseTime = (totalDist - 2f * accelDist) / peakVelocity;
                    totalTime = 2f * accelTime + cruiseTime;
                }
                else
                {
                    // 三角形: 加速 → 減速 (MaxSpeedに達しない)
                    peakVelocity = Mathf.Sqrt(totalDist * acceleration);
                    accelTime = peakVelocity / acceleration;
                    totalTime = 2f * accelTime;
                }

                float elapsed = 0f;
                while (elapsed < totalTime)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    elapsed += Time.deltaTime;
                    float progress = CalcProgress(elapsed, accelTime, peakVelocity, totalDist, totalTime);
                    float y = t.position.y;
                    t.position = new Vector3(startPos.x, y, startPos.z) + dir * (totalDist * progress);
                    float velocity = CalcVelocity(elapsed, accelTime, peakVelocity, totalTime);
                    animator.SetFloat("Speed", velocity / maxSpeed);
                }

                ApplyFinalState();
                Complete();
            }
            catch (OperationCanceledException) { }
            finally
            {
                cts?.Dispose();
                cts = null;
            }
        }

        float CalcVelocity(float elapsed, float accelTime, float peakVelocity, float totalTime)
        {
            float t = Mathf.Clamp(elapsed, 0f, totalTime);
            float decelStart = totalTime - accelTime;
            if (t <= accelTime)
                return acceleration * t;
            if (t <= decelStart)
                return peakVelocity;
            return acceleration * (totalTime - t);
        }

        float CalcProgress(float elapsed, float accelTime, float peakVelocity, float totalDist, float totalTime)
        {
            float t = Mathf.Clamp(elapsed, 0f, totalTime);
            float decelStart = totalTime - accelTime;
            float dist;
            if (t <= accelTime)
            {
                // 加速フェーズ
                dist = 0.5f * acceleration * t * t;
            }
            else if (t <= decelStart)
            {
                // 定速フェーズ
                float accelCoveredDist = 0.5f * acceleration * accelTime * accelTime;
                dist = accelCoveredDist + peakVelocity * (t - accelTime);
            }
            else
            {
                // 減速フェーズ
                float dt = totalTime - t;
                dist = totalDist - 0.5f * acceleration * dt * dt;
            }
            return Mathf.Clamp01(dist / totalDist);
        }
    }
}
