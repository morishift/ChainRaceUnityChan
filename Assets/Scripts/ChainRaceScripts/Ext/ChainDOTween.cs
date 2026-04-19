// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using DG.Tweening;
using System;

namespace ChainPattern
{
    /// <summary>
    /// Chain that plays a DOTween Tween and completes when it finishes
    /// </summary>
    public class ChainDOTween : Chain
    {
        Func<Tween> tweenFactory;
        Tween tween;

        /// <summary>
        /// Creates a ChainDOTween with a factory function that returns a Tween
        /// </summary>
        public ChainDOTween(Func<Tween> tweenFactory)
        {
            this.tweenFactory = tweenFactory;
        }

        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal()
        {
            if (tweenFactory == null)
            {
                Complete();
                return;
            }
            if (isFastForward)
            {
                tween = tweenFactory.Invoke();
                tween?.Complete(withCallbacks: true);
                tween = null;
                Complete();
                return;
            }
            tween = tweenFactory.Invoke();
            if (tween == null)
            {
                Complete();
                return;
            }
            tween.OnComplete(() =>
            {
                tween = null;
                Complete();
            });
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
            if (tween != null && tween.IsActive())
            {
                tween.Complete(withCallbacks: true);
                tween = null;
            }
        }
    }
}
