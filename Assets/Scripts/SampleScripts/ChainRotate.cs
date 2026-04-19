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
    /// Chain that rotates Unity-chan to face a target world direction,
    /// driving the Animator's "Direction" parameter during the turn.
    /// Rotation follows a trapezoidal angular velocity profile:
    /// accelerates up to MaxAngularVelocity, cruises, then decelerates.
    /// </summary>
    public class ChainRotate : Chain
    {
        const float MaxAngularVelocity = 450f; // deg/s
        const float AngularAcceleration = 630f; // deg/s²

        Animator animator;
        Vector3 targetDirection;
        CancellationTokenSource cts;

        /// <summary>
        /// Creates a ChainRotate.
        /// </summary>
        /// <param name="animator">Unity-chan's Animator</param>
        /// <param name="targetDirection">World direction to face after rotation</param>
        /// <param name="maxAngularVelocity">Maximum rotation speed in degrees/second</param>
        /// <param name="angularAcceleration">Angular acceleration/deceleration in degrees/second²</param>
        public ChainRotate(Animator animator, Vector3 targetDirection)
        {
            this.animator = animator;
            this.targetDirection = targetDirection;
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
            RotateAsync(cts.Token).Forget();
        }

        protected override void SkipInternal()
        {
            cts?.Cancel();
            ApplyFinalState();
        }

        void ApplyFinalState()
        {
            if (animator == null) return;
            Vector3 flatDir = new Vector3(targetDirection.x, 0f, targetDirection.z).normalized;
            if (flatDir != Vector3.zero)
            {
                animator.transform.rotation = Quaternion.LookRotation(flatDir);
            }
            animator.SetFloat("Direction", 0f);
            animator.Update(0.0f); // Apply changes immediately
        }

        async UniTask RotateAsync(CancellationToken token)
        {
            try
            {
                var t = animator.transform;
                var flatDir = new Vector3(targetDirection.x, 0f, targetDirection.z).normalized;
                if (flatDir == Vector3.zero)
                {
                    animator.SetFloat("Direction", 0f);
                    animator.Update(0.0f); // Apply changes immediately
                    Complete();
                    return;
                }

                Quaternion startRot = t.rotation;
                Quaternion endRot = Quaternion.LookRotation(flatDir);

                float totalAngle = Quaternion.Angle(startRot, endRot);
                if (totalAngle < 0.001f)
                {
                    ApplyFinalState();
                    Complete();
                    return;
                }

                // 右回り(+1)か左回り(-1)かを外積のY成分で判定
                float cross = Vector3.Cross(t.forward, flatDir).y;
                float dirSign = cross >= 0f ? 1f : -1f;

                // 台形速度プロファイルを計算
                // 加速フェーズで到達できる最大角度 = v² / (2a)
                float accelAngle = 0.5f * MaxAngularVelocity * MaxAngularVelocity / AngularAcceleration;
                float accelTime, peakVelocity, totalTime;

                if (totalAngle >= 2f * accelAngle)
                {
                    // 台形: 加速 → 定速 → 減速
                    peakVelocity = MaxAngularVelocity;
                    accelTime = peakVelocity / AngularAcceleration;
                    float cruiseTime = (totalAngle - 2f * accelAngle) / peakVelocity;
                    totalTime = 2f * accelTime + cruiseTime;
                }
                else
                {
                    // 三角形: 加速 → 減速 (MaxAngularVelocityに達しない)
                    peakVelocity = Mathf.Sqrt(totalAngle * AngularAcceleration);
                    accelTime = peakVelocity / AngularAcceleration;
                    totalTime = 2f * accelTime;
                }

                float elapsed = 0f;
                while (elapsed < totalTime)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    elapsed += Time.deltaTime;
                    float progress = CalcProgress(elapsed, accelTime, peakVelocity, totalAngle, totalTime);
                    t.rotation = Quaternion.Slerp(startRot, endRot, progress);
                    float velocity = CalcAngularVelocity(elapsed, accelTime, peakVelocity, totalTime);
                    animator.SetFloat("Direction", dirSign * velocity / MaxAngularVelocity);
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

        float CalcAngularVelocity(float elapsed, float accelTime, float peakVelocity, float totalTime)
        {
            float t = Mathf.Clamp(elapsed, 0f, totalTime);
            float decelStart = totalTime - accelTime;
            if (t <= accelTime)
                return AngularAcceleration * t;
            if (t <= decelStart)
                return peakVelocity;
            return AngularAcceleration * (totalTime - t);
        }

        float CalcProgress(float elapsed, float accelTime, float peakVelocity, float totalAngle, float totalTime)
        {
            float t = Mathf.Clamp(elapsed, 0f, totalTime);
            float decelStart = totalTime - accelTime;
            float angle;
            if (t <= accelTime)
            {
                // 加速フェーズ
                angle = 0.5f * AngularAcceleration * t * t;
            }
            else if (t <= decelStart)
            {
                // 定速フェーズ
                float accelCoveredAngle = 0.5f * AngularAcceleration * accelTime * accelTime;
                angle = accelCoveredAngle + peakVelocity * (t - accelTime);
            }
            else
            {
                // 減速フェーズ
                float dt = totalTime - t;
                angle = totalAngle - 0.5f * AngularAcceleration * dt * dt;
            }
            return Mathf.Clamp01(angle / totalAngle);
        }
    }
}
