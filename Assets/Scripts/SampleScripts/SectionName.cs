using DG.Tweening;
using UnityEngine;

namespace Sample
{
    public class SectionName : MonoBehaviour
    {
        [SerializeField]
        TMPro.TextMeshProUGUI sectionNameText;
        [SerializeField]
        CanvasGroup canvasGroup;

        void Start()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// セクション名が指定されたらフェードインして表示
        /// ""が指定されたらフェードアウトして非表示
        /// </summary>
        public void SetSectionName(string sectionName)
        {
            Debug.Log($"SetSectionName sectionName:{sectionName}");
            canvasGroup.DOKill();
            if (sectionName == "")
            {
                canvasGroup.DOFade(0f, 0.3f).OnComplete(() => gameObject.SetActive(false));
            }
            else
            {
                sectionNameText.text = sectionName;
                gameObject.SetActive(true);
                canvasGroup.DOFade(1f, 0.3f);
            }
        }
    }
}

