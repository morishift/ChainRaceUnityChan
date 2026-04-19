using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Sample
{

    /// <summary>
    /// ボタンを押したときに縮むアニメーション。
    /// Buttonコンポーネントと同じGameObjectにアタッチして使う。
    /// </summary>
    public class ButtonScaleEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("押下時のスケール")]
        [SerializeField] float pressScale = 0.99f;

        [Header("アニメーション時間(秒)")]
        [SerializeField] float duration = 0.50f;

        [Header("対象(未設定なら自分のRectTransform)")]
        [SerializeField] RectTransform target;

        Vector3 originalScale;
        Tween currentTween;
        Button button;

        void Awake()
        {
            if (target == null)
                target = GetComponent<RectTransform>();
            button = GetComponent<Button>();
            originalScale = target.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable)
            {
                return;
            }
            currentTween?.Kill();
            currentTween = target
                .DOScale(originalScale * pressScale, duration)
                .SetEase(Ease.Linear)
                .SetUpdate(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            currentTween?.Kill();
            currentTween = target
                .DOScale(originalScale, duration)
                .SetEase(Ease.Linear)
                .SetUpdate(true);
        }

        void OnDisable()
        {
            currentTween?.Kill();
            if (target != null)
                target.localScale = originalScale;
        }
    }

}
