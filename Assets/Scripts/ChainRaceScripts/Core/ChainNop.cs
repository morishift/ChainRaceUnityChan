// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

namespace ChainPattern
{
    /// <summary>
    /// Chain class that completes immediately after starting (no operation)
    /// </summary>
    public class ChainNop : Chain
    {
        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal()
        {
            Complete();
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
        }
    }
}


