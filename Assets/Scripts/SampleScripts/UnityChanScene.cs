using ChainPattern;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityChan;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    /// <summary>
    /// ユニティちゃんのアニメーションとボイスを順に再生するシーン。
    /// オープニングとクロージングは固定で、中間のパターンは複数の候補からランダムに選択される。
    /// クロージング後はオープニングに戻りループする。
    /// </summary>
    public class UnityChanScene : MonoBehaviour
    {
        [SerializeField]
        Animator unityChanAnimator;
        [SerializeField]
        SpringManager springManager;
        [SerializeField]
        CameraDirection cameraDirection;
        [SerializeField]
        Transform mainCamera;
        [SerializeField]
        Image irisImage;
        [SerializeField]
        VoiceSubtitle voiceSubtitle;
        [SerializeField]
        Image unityChanLogoImage;
        [SerializeField]
        Button startButton;
        [SerializeField]
        Button skipButton;
        [SerializeField]
        SectionName sectionName;
        [SerializeField]
        Button touchTheScreenButton;

        static readonly string[] shoutVoices =
        {
            "univ0001", // やっ！ (0.6s)
            "univ0002", // それっ！ (0.7s)
            "univ1254", // とぅ！ (0.5s)
            "univ0005", // いけっ！ (1.0s)
        };
        static readonly string[] victoryVoices =
        {
            "univ0006", // やったー！ (1.0s)
            "univ0007", // ナイスっ！ (1.0s)
            "univ0004", // ふふっ (1.2s)
            "univ0009", // ふふんっ (1.3s)
            "univ1027", // そうこなくっちゃねっ！ (1.5s)
            "univ1029", // おっけーっ！ (1.1s)
        };

        static readonly string[] mumbleVoices =
        {
            "univ1107", // さてさて、これからどうしよっか？ (2.9s)
            "univ1244", // う〜ん…どうしよっかなぁ… (2.9s)
            "univ0008", // さてと！ (1.3s)
        };

        Chain chain;
        Vector3 unityChanPosition;

        /// <summary>
        /// 初期状態を設定してChainを開始する
        /// </summary>
        void Start()
        {
            // Instantiate the material to avoid modifying the shared material asset at runtime
            irisImage.material = new Material(irisImage.material);
            ChainStart();
        }

        /// <summary>
        /// シーンを初期化し、シーン全体のChainを組み立てて実行する。
        /// 完了後は自分自身を呼び直してループする
        /// </summary>
        private async void ChainStart()
        {
            unityChanLogoImage.gameObject.SetActive(false);
            irisImage.gameObject.SetActive(true);
            irisImage.material.SetFloat("_Radius", 0.0f);
            startButton.gameObject.SetActive(false);
            startButton.interactable = false;
            skipButton.interactable = false;
            touchTheScreenButton.gameObject.SetActive(false);
            touchTheScreenButton.interactable = false;
            unityChanAnimator.transform.position = new Vector3(0, 0, 1);
            unityChanAnimator.transform.rotation = Quaternion.Euler(0, 180, 0);
            unityChanPosition = unityChanAnimator.transform.position;
            sectionName.SetSectionName("");

            chain?.Skip();
            chain = new ChainSequence(
                new ChainDelay(0.5f),
                ChainStartButton(),
                new ChainDelay(0.25f),
                ChainOpening(),                
                ChainPickRandom(5),                
                ChainClosing()
            );
#if UNITY_EDITOR
            ChainPattern.Editor.ChainDebugWindow.Watch(chain);
#endif
            await chain.Start();
            ChainStart(); // Loop the chain
        }

        /// <summary>
        /// 演出パターンをランダムに count 個選び、スキップボタン付きで順に実行するChainを返す
        /// </summary>
        private Chain ChainPickRandom(int count)
        {
            Func<Chain>[] picks = PickRandom(new Func<Chain>[]
            {
                ChainUnityChan1,
                ChainUnityChan2,
                ChainUnityChan3,
                ChainUnityChan4,
                ChainUnityChan5,
                ChainUnityChan6,
                ChainUnityChan7,
                ChainUnityChan8,
                ChainUnityChan9,
                ChainUnityChan10,
            }, count);


            int section = 1;
            var seq = new ChainSequence();
            foreach (Func<Chain> pick in picks)
            {
                int n = section;
                seq.Add(new ChainRace(
                    new ChainButton(skipButton),
                    new ChainSequence(
                        new ChainAction(() => sectionName.SetSectionName($"Section {n}")),
                        new ChainDelay(0.1f),
                        pick(),
                        new ChainAction(ff =>
                        {
                            // スキップ時はスプリングとカメラをリセットして不自然な動きを防ぐ
                            if (ff)
                            {
                                springManager.ResetAllSprings();
                                cameraDirection.ResetDirection();
                            }
                        })
                    )
                ));
                ++section;
            }
            seq.Add(new ChainAction(() => sectionName.SetSectionName("")));
            return seq;
        }

        /// <summary>
        /// source からランダムに count 個を重複なしで選んで返す
        /// </summary>
        private Func<Chain>[] PickRandom(Func<Chain>[] source, int count)
        {
            var list = new List<Func<Chain>>(source);
            var result = new Func<Chain>[Mathf.Min(count, list.Count)];
            for (int i = 0; i < result.Length; i++)
            {
                int idx = UnityEngine.Random.Range(0, list.Count);
                result[i] = list[idx];
                list.RemoveAt(idx);
            }
            return result;
        }

        /// <summary>
        /// スタートボタンをフェードインで表示し、押されたらフェードアウトして非表示にするChainを返す
        /// </summary>
        private Chain ChainStartButton()
        {
            CanvasGroup g = startButton.GetComponent<CanvasGroup>();
            return new ChainSequence(
                new ChainAction(() =>
                {
                    startButton.gameObject.SetActive(true);
                    g.alpha = 0.0f;
                }),
                new ChainDOTween(() => g.DOFade(endValue: 1.0f, duration: 0.5f)),
                new ChainButton(startButton),
                new ChainDOTween(() => g.DOFade(endValue: 0.0f, duration: 0.5f)),
                new ChainAction(() =>
                {
                    startButton.gameObject.SetActive(false);
                })
            );
        }

        /// <summary>
        /// 伸びアニメーションと掛け声ボイスを並行再生する
        /// </summary>
        private Chain ChainUnityChan1()
        {
            return new ChainSequence(
                ChainMoveToTarget(new Vector3(0, 0, 1)),
                ChainLookAtCamera(),
                new ChainParallel(
                    new ChainSequence(
                        new ChainCrossFade(unityChanAnimator, "WAIT01", 0.25f, 0.25f), // 伸び 5sec
                        new ChainCrossFadeWaitState(unityChanAnimator, "Idle", 0.25f)
                    ),
                    new ChainSequence(
                        ChainVoiceAndSubtitle("univ1003"), // そんじゃ、始めるとしますか！
                        new ChainDelay(2.0f),
                        ChainVoiceAndSubtitle("univ0008")  // さてと！
                    )
                )
            );
        }

        /// <summary>
        /// 回転キックから勝利ポーズ、かけ声と勝利ボイスを並行再生する
        /// </summary>
        private Chain ChainUnityChan2()
        {
            return new ChainSequence(
                ChainMoveToTarget(new Vector3(0, 0, 1)),
                ChainLookAtCamera(),
                new ChainParallel(
                    new ChainSequence(
                        new ChainCrossFade(unityChanAnimator, "WAIT04", 0.25f, 0.25f), // 回転キック 3sec
                        new ChainCrossFade(unityChanAnimator, "WIN00", 0.25f, 0.25f),  // 勝利ポーズ 3sec
                        new ChainCrossFadeWaitState(unityChanAnimator, "Idle", 0.25f)
                    ),
                    new ChainSequence(
                        new ChainDelay(0.5f),
                        ChainRandomVoice(shoutVoices),
                        new ChainDelay(2.0f),
                        ChainRandomVoice(victoryVoices)
                    )
                )
            );
        }

        /// <summary>
        /// 少し後退してから大ジャンプとかけ声を披露する
        /// </summary>
        private Chain ChainUnityChan3()
        {
            return new ChainSequence(
                ChainMoveToTarget(new Vector3(0, 0, 1)),
                ChainLookAtCamera(),
                ChainVoiceAndSubtitle("univ0008"), // さてと！
                ChainMoveToTarget(new Vector3(0, 0, 2)),
                ChainLookAtCamera(),
                new ChainParallel(
                    new ChainSequence(
                        new ChainCrossFade(unityChanAnimator, "JUMP00", 0.25f, 0.25f), // 大ジャンプ
                        new ChainCrossFadeWaitState(unityChanAnimator, "Idle", 0.25f)
                    ),
                    new ChainSequence(
                        new ChainDelay(0.3f),
                        ChainRandomVoice(shoutVoices)
                    )
                ),
                ChainRandomVoice(victoryVoices)
            );
        }

        /// <summary>
        /// 自己紹介
        /// </summary>
        private Chain ChainUnityChan4()
        {
            return new ChainSequence(
                ChainMoveToTarget(new Vector3(0, 0, 1)),
                ChainLookAtCamera(),
                new ChainParallel(
                    new ChainSequence(
                        new ChainCrossFade(unityChanAnimator, "WAIT01", 0.25f, 0.25f), // 伸び
                        new ChainCrossFadeWaitState(unityChanAnimator, "Idle", 0.25f)
                    ),
                    new ChainSequence(
                        ChainVoiceAndSubtitle("univ0015"), // やっほー！
                        new ChainDelay(0.5f),
                        ChainVoiceAndSubtitle("univ0019"), // こんにちは。わたしユニティちゃん
                        new ChainDelay(0.5f),
                        ChainVoiceAndSubtitle("univ0025")  // 一緒に遊ぼう！
                    )
                )
            );
        }

        /// <summary>
        /// 失敗からの跳び箱リベンジ
        /// </summary>
        private Chain ChainUnityChan5()
        {
            return new ChainSequence(
                ChainMoveToTarget(new Vector3(0, 0, 1)),
                ChainLookAtCamera(),
                new ChainParallel(
                    new ChainSequence(
                        new ChainCrossFade(unityChanAnimator, "LOSE00", 0.25f, 0.25f), // 負けポーズ
                        new ChainCrossFadeWaitState(unityChanAnimator, "Idle", 0.25f)
                    ),
                    new ChainSequence(
                        new ChainDelay(0.5f),
                        ChainVoiceAndSubtitle("univ1077") // くっ…ここまで来たのに…
                    )
                ),
                ChainVoiceAndSubtitle("univ1098"), // ここから一気に逆転よっ！
                new ChainParallel(
                    new ChainSequence(
                        new ChainCrossFade(unityChanAnimator, "UMATOBI00", 0.25f, 0.25f), // 跳び箱
                        new ChainCrossFade(unityChanAnimator, "WIN00", 0.25f, 0.25f),      // 決めポーズ
                        new ChainCrossFadeWaitState(unityChanAnimator, "Idle", 0.25f)
                    ),
                    new ChainSequence(
                        new ChainDelay(0.3f),
                        ChainRandomVoice(shoutVoices),
                        new ChainDelay(1.5f),
                        ChainVoiceAndSubtitle("univ1018")  // クリアっ！やったねっ！
                    )
                )
            );
        }

        /// <summary>
        /// 斜め後方に移動してからダメージ→起き上がりアニメーションを再生する
        /// </summary>
        private Chain ChainUnityChan6()
        {
            return new ChainSequence(
                ChainMoveToTarget(new Vector3(0, 0, 3)),
                new ChainRotate(unityChanAnimator, new Vector3(1, 0, 1)),

                new ChainParallel(
                    new ChainSequence(
                        new ChainCrossFade(unityChanAnimator, "DAMAGED01", 0.25f, 0.25f), // ひっくり返って起き上がる
                        new ChainCrossFadeWaitState(unityChanAnimator, "Idle", 0.25f),
                        ChainLookAtCamera()
                    ),
                    new ChainSequence(
                        new ChainDelay(0.3f),
                        ChainVoiceAndSubtitle("univ1038"), // きゃっ！                        
                        new ChainDelay(2.0f),
                        ChainVoiceAndSubtitle("univ1024")  // どんまいっ！
                    )
                )
            );
        }

        /// <summary>
        /// スタートの号令からスライディングでゴールを決める
        /// </summary>
        private Chain ChainUnityChan7()
        {
            return new ChainSequence(
                ChainMoveToTarget(new Vector3(0, 0, 2)),
                ChainLookAtCamera(),
                ChainVoiceAndSubtitle("univ1016"), // スタートっ！
                new ChainParallel(
                    new ChainCrossFade(unityChanAnimator, "SLIDE00", 0.25f, 0.25f), // スライディング
                    new ChainSequence(
                        new ChainDelay(0.5f),
                        ChainRandomVoice(shoutVoices)
                    )
                ),
                new ChainParallel(
                    new ChainSequence(
                        new ChainCrossFade(unityChanAnimator, "WIN00", 0.25f, 0.25f), // 勝利ポーズ
                        new ChainCrossFadeWaitState(unityChanAnimator, "Idle", 0.25f)
                    ),
                    ChainVoiceAndSubtitle("univ1017") // ゴーーーールッ！
                )
            );
        }

        /// <summary>
        /// 息切れから気力を取り戻してジャンプで決めポーズを披露する
        /// </summary>
        private Chain ChainUnityChan8()
        {
            return new ChainSequence(
                ChainMoveToTarget(new Vector3(0, 0, 2)),
                ChainLookAtCamera(),
                new ChainParallel(
                    new ChainSequence(
                        new ChainCrossFade(unityChanAnimator, "REFRESH00", 0.25f, 0.25f), // 息切れ
                        new ChainCrossFadeWaitState(unityChanAnimator, "Idle", 0.25f)
                    ),
                    new ChainSequence(
                        new ChainDelay(1.0f),
                        ChainVoiceAndSubtitle("univ1099") // まだまだいけるよっ！
                    )
                ),
                new ChainParallel(
                    new ChainSequence(
                        new ChainCrossFade(unityChanAnimator, "JUMP01B", 0.25f, 0.25f), // 勢いよく手を伸ばす
                        new ChainCrossFadeWaitState(unityChanAnimator, "Idle", 0.25f)
                    ),
                    new ChainSequence(
                        new ChainDelay(0.3f),
                        ChainRandomVoice(victoryVoices)
                    )
                )
            );
        }

        /// <summary>
        /// 左右に動いてから中央で決めポーズ
        /// </summary>
        private Chain ChainUnityChan9()
        {
            return new ChainSequence(
                ChainMoveToTarget(new Vector3(1, 0, 1.5f)),
                ChainLookAtCamera(),
                ChainVoiceAndSubtitle("univ1057"), // ふむふむ…なかなかいいところを突いてくるねぇ…
                ChainMoveToTarget(new Vector3(-4, 0, 4)),
                ChainLookAtCamera(),
                new ChainParallel(
                    new ChainSequence(
                        new ChainCrossFade(unityChanAnimator, "WAIT04", 0.25f, 0.25f), // 回転キック
                        new ChainCrossFadeWaitState(unityChanAnimator, "Idle", 0.25f)
                    ),
                    new ChainSequence(
                        new ChainDelay(0.8f),
                        ChainRandomVoice(shoutVoices)
                    )
                ),
                new ChainParallel(
                    ChainMoveToTarget(new Vector3(0, 0, 1)),
                    new ChainSequence(
                        new ChainDelay(0.3f),
                        ChainRandomVoice(mumbleVoices)
                    )
                ),
                ChainLookAtCamera(),
                new ChainParallel(
                    new ChainSequence(
                        new ChainCrossFade(unityChanAnimator, "JUMP01B", 0.25f, 0.25f), // 勢いよく手を伸ばす
                        new ChainCrossFadeWaitState(unityChanAnimator, "Idle", 0.25f)
                    ),
                    new ChainSequence(
                        new ChainDelay(0.3f),
                        ChainVoiceAndSubtitle("univ1072") // オーケィ、わたしにまかせてっ！
                    )
                )
            );
        }

        /// <summary>
        /// 手を振ってぽちっと促し、タップで勝利ポーズ、タイムアウトで跳び箱
        /// </summary>
        private Chain ChainUnityChan10()
        {
            ChainSequence branchSequence = new ChainSequence();
            return new ChainSequence(
                ChainMoveToTarget(new Vector3(0, 0, 1)),
                ChainLookAtCamera(),                
                new ChainCrossFadeWaitState(unityChanAnimator, "WAIT03", 0.25f), // 手を振る ステートに入るのを待つ
                ChainVoiceAndSubtitle("univ1032"), // ぽちっと！
                new ChainAction(() =>
                {
                    touchTheScreenButton.gameObject.SetActive(true);
                    touchTheScreenButton.GetComponent<CanvasGroup>().alpha = 0.0f;
                }),
                new ChainDOTween(() => touchTheScreenButton.GetComponent<CanvasGroup>().DOFade(1.0f, 0.25f)),
                new ChainRace(
                    new ChainSequence(
                        new ChainButton(touchTheScreenButton),
                        new ChainAction(ff =>
                        {
                            if (!ff)
                            {
                                // タップした
                                branchSequence.Add(new ChainParallel(
                                    new ChainCrossFade(unityChanAnimator, "JUMP00", 0.25f, 0.25f), // 大ジャンプ
                                    ChainVoiceAndSubtitle("univ1020") // エクセレントっ！すっごーいっ！
                                ));
                            }
                        })
                    ),
                    new ChainSequence(
                        new ChainDelay(5.0f),
                        new ChainAction(ff =>
                        {
                            if (!ff)
                            {
                                // タイムアウト
                                branchSequence.Add(new ChainParallel(
                                    new ChainCrossFade(unityChanAnimator, "UMATOBI00", 0.25f, 0.25f), // 跳び箱
                                    ChainVoiceAndSubtitle("univ1121") // むぅーっ！遅ぉーーーーいっ！
                                ));
                            }
                        })
                    )
                ),
                new ChainDOTween(() => touchTheScreenButton.GetComponent<CanvasGroup>().DOFade(0.0f, 0.25f)),
                new ChainAction(() => touchTheScreenButton.gameObject.SetActive(false)),
                branchSequence,
                new ChainCrossFadeWaitState(unityChanAnimator, "Idle", 0.25f)
            );
        }

        /// <summary>
        /// ボイスを再生しながら字幕を表示する。ボイス終了後に字幕をフェードアウトする
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

        /// <summary>
        /// ユニティちゃんを目標位置の方向に向かせてから移動させるChainを返す
        /// </summary>
        private Chain ChainMoveToTarget(Vector3 target)
        {
            Vector3 pos = unityChanPosition;
            unityChanPosition = target;
            return new ChainSequence(
                new ChainRotate(unityChanAnimator, target - pos),
                new ChainForward(unityChanAnimator, target)
            );
        }

        /// <summary>
        /// アイリスを開きながら挨拶ボイスを再生するオープニングChainを返す
        /// </summary>
        private Chain ChainOpening()
        {
            return new ChainSequence(
                new ChainAction(() =>
                {
                    irisImage.gameObject.SetActive(true);
                    irisImage.material.SetFloat("_Radius", 0.0f);
                    cameraDirection.targetFacePriority = true;
                }),
                new ChainDelay(0.5f),
                ChainIris(duration: 1.0f, radius: 0.2f),
                // Opening
                ChainRandomVoice(
                    "univ1040", // ねぇねぇ…今なにしてるのかなっ！？
                    "univ1114", // ねぇねぇ…今、時間あるかな？
                    "univ0015", // やっほー！
                    "univ1088", // わたしと一緒に…ゲームしよっ？
                    "univ1108", // なにして遊ぼうか？
                    "univ1105"  // わたしでいいの…？
                ),
                new ChainAction(() => cameraDirection.targetFacePriority = false),
                ChainIris(duration: 1.5f, radius: 1.3f)
            );
        }

        /// <summary>
        /// お別れボイスを再生してアイリスを閉じ、ロゴを表示するクロージングChainを返す
        /// </summary>
        private Chain ChainClosing()
        {
            return new ChainSequence(
                ChainMoveToTarget(new Vector3(0, 0, 1)),
                ChainLookAtCamera(),
                new ChainAction(() =>
                {
                    cameraDirection.targetFacePriority = true;
                }),
                ChainIris(duration: 1.0f, radius: 0.2f),
                new ChainDelay(0.5f),
                ChainRandomVoice(
                    "univ1013", // また一緒に遊ぼうねっ！バイバイーっ！
                    "univ1110", // 今日はとっても楽しかった！また一緒に遊ぼうねっ！
                    "univ1196", // まったねぇ〜っ！バイバイ〜っ！
                    "univ1014"  // えーっ！もう行っちゃうのーっ！？
                ),
                new ChainDelay(0.5f),
                ChainIris(duration: 1.0f, radius: 0.0f),
                new ChainAction(() =>
                {
                    unityChanLogoImage.gameObject.SetActive(true);
                    unityChanLogoImage.color = new Color(1, 1, 1, 0);
                }),
                new ChainDelay(1.0f),
                new ChainDOTween(() => unityChanLogoImage.DOFade(1.0f, 0.5f)),
                new ChainDelay(2.0f),
                new ChainDOTween(() => unityChanLogoImage.DOFade(0.0f, 0.5f)),
                new ChainAction(() =>
                {
                    unityChanLogoImage.gameObject.SetActive(false);
                }),
                new ChainDelay(0.25f)
            );
        }

        /// <summary>
        /// アイリスの半径を指定の値までアニメーションさせるChainを返す
        /// </summary>
        private Chain ChainIris(float duration, float radius)
        {
            return new ChainSequence(
                new ChainAction(() =>
                {
                    irisImage.gameObject.SetActive(true);
                }),
                new ChainDOTween(() => irisImage.material.DOFloat(radius, "_Radius", duration)),
                new ChainAction(() =>
                {
                    irisImage.gameObject.SetActive(radius < 1.0);
                })
            );
        }

        /// <summary>
        /// 指定したボイスIDの中からランダムに1つ選んで再生するChainを返す
        /// </summary>
        private Chain ChainRandomVoice(params string[] voiceIds)
        {
            string id = voiceIds[UnityEngine.Random.Range(0, voiceIds.Length)];
            return ChainVoiceAndSubtitle(id);
        }

        /// <summary>
        /// ユニティちゃんをカメラの方向に向かせるChainを返す
        /// </summary>
        private Chain ChainLookAtCamera()
        {
            return new ChainRotate(unityChanAnimator, mainCamera.position - unityChanPosition);
        }

    }
}





