using ChainPattern;
using DG.Tweening;
using UnityEngine;

namespace Sample
{
    public class TestSubtitle : MonoBehaviour
    {
        [SerializeField]
        TestButtons testButtons;
        [SerializeField]
        VoiceSubtitle voiceSubtitle;

        Chain chain;

        private void Awake()
        {
            testButtons.AddButton("ChainPlayVoice", async () =>
            {
                chain?.Skip();
                chain = new ChainSequence(
                    new ChainPlayVoice("univ0001")
                );
                Debug.Log("Start ChainPlayVoice");
                await chain.Start();
                Debug.Log("End ChainPlayVoice");
            });

            testButtons.AddButton("Subtitle1", async () =>
            {
                chain?.Skip();
                chain = voiceSubtitle.ChainShowSubtitle(2.0f, 1.0f, "Hello World!");
                await chain.Start();
            });


            testButtons.AddButton("Subtitle2", async () =>
            {
                chain?.Skip();
                chain = voiceSubtitle.ChainShowSubtitle(2.0f, 1.0f, "あいうえおかきくけこさしすせそたちつてと");
                await chain.Start();
            });


            testButtons.AddButton("Subtitle1and2", async () =>
            {
                chain?.Skip();
                chain = new ChainSequence(
                    voiceSubtitle.ChainShowSubtitle(2.0f, 1.0f, "Hello World!"),
                    voiceSubtitle.ChainShowSubtitle(2.0f, 1.0f, "あいうえおかきくけこさしすせそたちつてと")
                );
                await chain.Start();
            });

            testButtons.AddButton("ChainVoiceAndSubtitle", async () =>
            {
                chain?.Skip();
                chain = new ChainSequence(
                    ChainVoiceAndSubtitle("univ0001")
                );
                await chain.Start();
            });

            testButtons.AddButton("DOTween", async () =>
            {
                chain?.Skip();
                chain = new ChainDOTween(() => 
                DOTween.Sequence().AppendInterval(5).AppendCallback(() => Debug.Log("Callback1")).AppendCallback(() => Debug.Log("Callback2")));
                await chain.Start();
            });


            testButtons.AddButton("Skip", () =>
            {
                chain?.Skip();
            });
        }

        /// <summary>
        /// play voice and show subtitle at the same time. The subtitle will be hidden after the voice finished.
        /// </summary>
        private Chain ChainVoiceAndSubtitle(string voiceId)
        {
            AudioClip clip = UnityChanVoiceList.LoadAudioClip(voiceId);
            if (clip == null)
            {
                Debug.LogWarning($"AudioClip not found for voiceId: {voiceId}");
                return new ChainNop();
            }
            return new ChainParallel(
                new ChainPlayVoice(voiceId),
                voiceSubtitle.ChainShowSubtitle(
                    clip.length + 0.5f,
                    clip.length * 0.7f,
                    UnityChanVoiceList.GetText(voiceId)
                )
            );
        }
    }
}
