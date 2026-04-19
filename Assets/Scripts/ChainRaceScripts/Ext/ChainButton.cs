// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using UnityEngine.UI;

namespace ChainPattern
{
    /// <summary>
    /// Chain that completes when a button is clicked
    /// </summary>
    public class ChainButton : Chain
    {
        Button targetButton;

        public ChainButton(Button button)
        {
            targetButton = button;
        }

        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal()
        {
            if (isFastForward || targetButton == null)
            {
                // Do nothing if it will be skipped immediately
                Complete();
                return;
            }
            targetButton.interactable = true;
            targetButton.onClick.AddListener(OnClickButton);
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
            if (targetButton != null)
            {
                targetButton.onClick.RemoveListener(OnClickButton);
                targetButton.interactable = false;
            }
        }

        /// <summary>
        /// Called when the button is clicked
        /// </summary>
        private void OnClickButton()
        {
            if (targetButton != null)
            {
                targetButton.onClick.RemoveListener(OnClickButton);
                targetButton.interactable = false;
            }
            Complete();
        }
    }
}
