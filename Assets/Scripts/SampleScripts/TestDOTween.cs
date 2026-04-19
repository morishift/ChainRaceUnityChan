using ChainPattern;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    /// <summary>
    /// Demonstrates ChainDOTween usage with move and color animations.
    /// Press Skip at any time to jump all pending tweens to their final state.
    /// </summary>
    public class TestDOTween : MonoBehaviour
    {
        [SerializeField]
        TestButtons testButtons;
        [SerializeField]
        RectTransform rectPanel;
        [SerializeField]
        Image colorPanel;

        Chain chain;

        private void Awake()
        {
            // Moves rectPanel along a rectangular path and returns to the start position.
            // If a chain is already running, it is skipped before starting a new one.
            testButtons.AddButton("MoveTest", async () =>
            {
                chain?.Skip();
                var startPos = rectPanel.anchoredPosition;
                chain = new ChainSequence(
                    new ChainDOTween(() => rectPanel.DOAnchorPos(startPos + new Vector2(200f, 0f), 1f)),
                    new ChainDOTween(() => rectPanel.DOAnchorPos(startPos + new Vector2(200f, -200f), 1f)),
                    new ChainDOTween(() => rectPanel.DOAnchorPos(startPos + new Vector2(0f, -200f), 1f)),
                    new ChainDOTween(() => rectPanel.DOAnchorPos(startPos, 1f))
                );
                Debug.Log("Start MoveTest");
                await chain.Start();
                Debug.Log("End MoveTest");
            });

            // Cycles colorPanel through red, blue, and green, then restores the original color.
            testButtons.AddButton("ColorTest", async () =>
            {
                chain?.Skip();
                var startColor = colorPanel.color;
                chain = new ChainSequence(
                    new ChainDOTween(() => colorPanel.DOColor(Color.red, 1f)),
                    new ChainDOTween(() => colorPanel.DOColor(Color.blue, 1f)),
                    new ChainDOTween(() => colorPanel.DOColor(Color.green, 1f)),
                    new ChainDOTween(() => colorPanel.DOColor(startColor, 1f))
                );
                Debug.Log("Start ColorTest");
                await chain.Start();
                Debug.Log("End ColorTest");
            });

            // Runs the move and color sequences simultaneously using ChainParallel.
            testButtons.AddButton("MoveAndColor", async () =>
            {
                chain?.Skip();
                var startPos = rectPanel.anchoredPosition;
                var startColor = colorPanel.color;
                chain = new ChainParallel(
                    new ChainSequence(
                        new ChainDOTween(() => rectPanel.DOAnchorPos(startPos + new Vector2(200f, 0f), 1f)),
                        new ChainDOTween(() => rectPanel.DOAnchorPos(startPos + new Vector2(200f, -200f), 1f)),
                        new ChainDOTween(() => rectPanel.DOAnchorPos(startPos + new Vector2(0f, -200f), 1f)),
                        new ChainDOTween(() => rectPanel.DOAnchorPos(startPos, 1f))
                    ),
                    new ChainSequence(
                        new ChainDOTween(() => colorPanel.DOColor(Color.red, 1f)),
                        new ChainDOTween(() => colorPanel.DOColor(Color.blue, 1f)),
                        new ChainDOTween(() => colorPanel.DOColor(Color.green, 1f)),
                        new ChainDOTween(() => colorPanel.DOColor(startColor, 1f))
                    )
                );
                Debug.Log("Start MoveAndColor");
                await chain.Start();
                Debug.Log("End MoveAndColor");
            });

            // Tests whether AppendCallback fires when Complete(withCallbacks) is called.
            // Sequence: wait 2s → AppendCallback(log "AppendCallback fired") → wait 2s → OnComplete(log "OnComplete fired")
            // Complete(withCallbacks: false): neither fires
            // Complete(withCallbacks: true):  only OnComplete fires (AppendCallback does NOT fire)
            testButtons.AddButton("AppendCallback_CompleteTrue", () =>
            {
                Debug.Log("--- AppendCallback test: Complete(withCallbacks: true) ---");
                var seq = DOTween.Sequence();
                seq.AppendInterval(2f);
                seq.AppendCallback(() => Debug.Log("AppendCallback fired"));
                seq.AppendInterval(2f);
                seq.OnComplete(() => Debug.Log("OnComplete fired"));
                seq.Complete(withCallbacks: true);
            });

            testButtons.AddButton("AppendCallback_CompleteFalse", () =>
            {
                Debug.Log("--- AppendCallback test: Complete(withCallbacks: false) ---");
                var seq = DOTween.Sequence();
                seq.AppendInterval(2f);
                seq.AppendCallback(() => Debug.Log("AppendCallback fired"));
                seq.AppendInterval(2f);
                seq.OnComplete(() => Debug.Log("OnComplete fired"));
                seq.Complete(withCallbacks: false);
            });

            // Skips the currently running chain, jumping all tweens to their final state.
            testButtons.AddButton("Skip", () =>
            {
                chain?.Skip();
            });
        }
    }
}
