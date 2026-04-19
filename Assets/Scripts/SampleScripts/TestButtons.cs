using System;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    /// <summary>
    /// Button helper for sample testing
    /// </summary>
    public class TestButtons : MonoBehaviour
    {
        [SerializeField]
        Button sourceButton;

        private void Awake()
        {
            sourceButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Adds a test button with the specified caption
        /// </summary>        
        public Button AddButton(string caption)
        {
            return AddButton(caption, null);
        }

        /// <summary>
        /// Adds a test button with the specified caption and optional click event
        /// </summary>
        public Button AddButton(string caption, Action action)
        {
            GameObject go = Instantiate<GameObject>(sourceButton.gameObject, sourceButton.transform.parent);
            go.SetActive(true);
            Button button = go.GetComponent<Button>();
            TMPro.TextMeshProUGUI text = go.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            text.text = caption;

            if (action != null)
            {
                button.onClick.AddListener(() =>
                {
                    action?.Invoke();
                });
            }

            return button;
        }
    }
}
