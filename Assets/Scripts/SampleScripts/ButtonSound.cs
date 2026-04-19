using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    /// <summary>
    /// plays a sound when a button is clicked
    /// </summary>
    public class ButtonSound : MonoBehaviour
    {
        [SerializeField]
        SEType soundType;

        Button button;
        
        void Start()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnClickButton);
            }
        }

        /// <summary>
        /// plays a sound when the button is clicked
        /// </summary>
        private void OnClickButton()
        {
            if (SoundPlayer.Get() != null)
            {
                SoundPlayer.Get().PlaySE(soundType);
            }        
        }
    }
}
