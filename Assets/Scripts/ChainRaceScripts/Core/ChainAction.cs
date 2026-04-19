// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using System;

namespace ChainPattern
{
    /// <summary>
    /// Chain that executes a single action/function
    /// </summary>
    public class ChainAction : Chain
    {
        Action actionToCall;

        public ChainAction()
        {
        }

        /// <summary>
        /// Creates a Chain with a specified action/function
        /// </summary>        
        public ChainAction(Action action)
        {
            actionToCall = action;
        }

        /// <summary>
        /// Creates a Chain that executes an action which takes isFastForward as a parameter
        /// </summary>        
        public ChainAction(Action<bool> action)
        {
            actionToCall = () => action?.Invoke(isFastForward);
        }

        /// <summary>
        /// Sets the action to be executed
        /// </summary>
        public void SetAction(Action action)
        {
            actionToCall = action;
        }

        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal()
        {
            actionToCall?.Invoke();
            actionToCall = null;
            Complete();
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
            actionToCall = null;
        }
    }
}
