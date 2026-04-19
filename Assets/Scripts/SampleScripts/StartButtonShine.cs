using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    public class StartButtonShine : MonoBehaviour
    {
        [SerializeField] private float startAlpha = 0.1f;
        [SerializeField] private float endAlpha = 1.0f;
        [SerializeField] private float duration = 0.4f;

        private Image shineImage;
        private Tween currentTween;

        void Awake()
        {
            shineImage = GetComponent<Image>();
        }

        void OnEnable()
        {
            // Kill any existing tween
            currentTween?.Kill();
            // Start from alpha 0
            Color c = shineImage.color;
            c.a = startAlpha;
            shineImage.color = c;
            Debug.Log($"StartButtonShine c:{c}");
            currentTween = shineImage.DOFade(endAlpha, duration)
                .SetEase(Ease.InOutSine)
                .SetDelay(0.25f)
                .SetLoops(-1, LoopType.Yoyo);
        }

        void OnDisable()
        {
            currentTween?.Kill();
            currentTween = null;
        }
    }
}

