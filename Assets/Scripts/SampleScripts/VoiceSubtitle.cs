using ChainPattern;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    public class VoiceSubtitle : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI textSubtitle;
        [SerializeField]
        ContentSizeFitter contentSizeFitter;
        [SerializeField]
        CanvasGroup canvasGroup;
        private void Awake()
        {
            canvasGroup.alpha = 0;
        }

        /// <summary>
        /// Returns a Chain that reveals the current subtitle text character by character over the specified duration.
        /// </summary>
        public Chain ChainTypeText(float duration, string text)
        {            
            return new ChainDOTween(() => DOVirtual.Float(0, text.Length, duration, value => textSubtitle.text = text.Substring(0, Mathf.RoundToInt(value))).SetEase(Ease.Linear));
        }

        /// <summary>
        /// Shows subtitle text with a fade-in, typing effect, and fade-out.
        /// </summary>
        public Chain ChainShowSubtitle(float duration, float typingduration, string text)
        {
            float waitTime = Mathf.Max(duration - typingduration - 0.5f, 0.0f); // Total duration minus typing and fade times
            return new ChainSequence(
                new ChainAction(() =>
                {
                    // Measure layout with full text first, then clear to animate from empty
                    contentSizeFitter.enabled = true;
                    textSubtitle.text = text;
                    Canvas.ForceUpdateCanvases();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
                    contentSizeFitter.enabled = false;
                    textSubtitle.text = "";
                }),
                new ChainDOTween(() => canvasGroup.DOFade(1f, 0.25f)),
                ChainTypeText(typingduration, text),                
                new ChainDelay(waitTime),
                new ChainDOTween(() => canvasGroup.DOFade(0f, 0.25f))
            );
        }
    }
}

