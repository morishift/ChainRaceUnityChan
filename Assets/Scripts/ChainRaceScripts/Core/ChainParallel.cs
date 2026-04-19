// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using System.Collections.Generic;

namespace ChainPattern
{
    /// <summary>
    /// Chain that executes multiple chains in parallel
    /// </summary>
    public class ChainParallel : Chain
    {
        List<Chain> chainList = new List<Chain>();
        List<Chain> startedChainList = new List<Chain>();

        enum ParallelState
        {
            Ready,
            Starting,
            Started,
            Consuming,
            Finished,
        }
        ParallelState parallelState;

#if UNITY_EDITOR
        // Full list of all registered children, preserved in definition order for the debug view.
        // Unlike chainList/startedChainList, entries are never removed even after completion.
        List<Chain> debugChainList = new List<Chain>();
#endif

        public ChainParallel(params Chain[] chains)
        {
            parallelState = ParallelState.Ready;
            chainList.AddRange(chains);
#if UNITY_EDITOR
            debugChainList.AddRange(chains);
#endif
        }

        /// <summary>
        /// Adds a chain to the parallel execution.
        /// If already Started, the chain begins immediately.
        /// Chains added after the parallel has finished are ignored.
        /// </summary>
        public ChainParallel Add(Chain chain)
        {
#if UNITY_EDITOR
            debugChainList.Add(chain);
#endif
            if (parallelState == ParallelState.Finished)
            {
                // Ignore
            }
            else if (parallelState == ParallelState.Started)
            {
                startedChainList.Add(chain);
                chain.SetCompleteCallback(() => OnChainComplete(chain));
                chain.SetIsFastForward(isFastForward);
                chain.Start();
            }
            else
            {
                // For all states except Started/Finished, queue into pending list
                // During Consuming, Add() may still happen reentrantly from chains being skipped.
                // Queue it into chainList so it will also be consumed in this pass.
                chainList.Add(chain);
            }
            return this;
        }

        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal()
        {
            if (chainList.Count <= 0)
            {
                parallelState = ParallelState.Finished;
                Complete();
                return;
            }
            parallelState = ParallelState.Starting;
            while (chainList.Count > 0 && parallelState == ParallelState.Starting)
            {
                Chain c = chainList[0];
                chainList.RemoveAt(0);
                startedChainList.Add(c);
                c.SetCompleteCallback(() => OnChainComplete(c));
                c.SetIsFastForward(isFastForward);
                c.Start();
            }
            if (parallelState == ParallelState.Starting)
            {
                parallelState = ParallelState.Started;
            }
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
            parallelState = ParallelState.Consuming;
            ConsumeStartedAndPendingChains();
            parallelState = ParallelState.Finished;
        }

        /// <summary>
        /// Callback invoked when a chain completes
        /// </summary>        
        private void OnChainComplete(Chain chain)
        {
            if (startedChainList.Contains(chain))
            {
                startedChainList.Remove(chain);
                if (chainList.Count <= 0 && startedChainList.Count <= 0)
                {
                    parallelState = ParallelState.Finished;
                    Complete();
                }
            }
        }

        /// <summary>
        /// Consumes (completes or skips) all started and pending chains
        /// </summary>
        private void ConsumeStartedAndPendingChains()
        {
            while (startedChainList.Count > 0)
            {
                Chain c = startedChainList[0];
                startedChainList.RemoveAt(0);
                c.Skip();
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
