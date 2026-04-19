// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using System.Collections.Generic;

namespace ChainPattern
{
    /// <summary>
    /// Chain that executes multiple chains sequentially
    /// </summary>
    public class ChainSequence : Chain
    {
        List<Chain> chainList = new List<Chain>();
        Chain currentChain;
        bool isEnabled;

#if UNITY_EDITOR
        // Full list of all registered children, preserved in definition order for the debug view.
        // Unlike chainList, entries are never removed from this list even after execution.
        List<Chain> debugChainList = new List<Chain>();
#endif

        public ChainSequence(params Chain[] chains)
        {
            isEnabled = true;
            chainList.AddRange(chains);
#if UNITY_EDITOR
            debugChainList.AddRange(chains);
#endif
        }

        /// <summary>
        /// Adds a chain to the sequence.
        /// Chains added after the sequence has finished are ignored.
        /// </summary>
        public ChainSequence Add(Chain chain)
        {
            if (isEnabled)
            {
                chainList.Add(chain);
#if UNITY_EDITOR
                debugChainList.Add(chain);
#endif
            }
            return this;
        }

        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal()
        {
            NextChain();
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
            if (currentChain != null)
            {
                currentChain.Skip();
                currentChain = null;
            }
            while (chainList.Count > 0)
            {
                Chain c = chainList[0];
                chainList.RemoveAt(0);
                bool complete = false;
                c.SetCompleteCallback(() => complete = true);
                c.SetIsFastForward(true);
                c.Start();
                if (!complete)
                {
                    c.Skip();
                }
            }
            isEnabled = false;
        }

        /// <summary>
        /// Executes the next chain in the sequence
        /// </summary>
        private void NextChain()
        {
            if (chainList.Count <= 0)
            {
                isEnabled = false;
                Complete();
                return;
            }
            currentChain = chainList[0];
            chainList.RemoveAt(0);
            currentChain.SetIsFastForward(isFastForward);
            currentChain.SetCompleteCallback(() =>
            {
                currentChain = null;
                NextChain();
            });
            currentChain.Start();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Returns all registered children in definition order for the debug tree view.
        /// Includes chains regardless of their current state (Ready/Started/Completed/Skipped).
        /// </summary>
        public override Chain[] DebugChildren => debugChainList.ToArray();
#endif
    }
}


