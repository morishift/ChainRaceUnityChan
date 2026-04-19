// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

namespace ChainPattern
{
    /// <summary>
    /// Chain class that halts execution after starting
    /// </summary>
    public class ChainHalt : Chain
    {
        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal()
        {
            // Does not call Complete()
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
        }
    }
}

