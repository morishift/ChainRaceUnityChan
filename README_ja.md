# ChainRaceUnityChan

[ChainRacePattern (CRP)](https://github.com/morishift/ChainRacePatternUnity) を UnityChan で実演する Unity デモプロジェクトです。

**[English version](README.md)**

![UnityChan-1-Main.gif](https://github.com/user-attachments/assets/7be233d9-7401-4d72-b60a-61fe27844e6d)  

## ChainRacePattern について

ChainRacePattern は、Unity における演出スキップ問題に対する設計パターンです。
演出・入力・待機などを Chain として表現し、`Sequence` / `Parallel` / `Race` の組み合わせで宣言的にフローを記述します。

コアルール・設計方針・サンプルシーン・デバッグウィンドウなどの技術詳細は、リファレンス実装を参照してください。

**[ChainRacePatternUnity](https://github.com/morishift/ChainRacePatternUnity)**

## このデモで実演していること

UnityChan を使った短い演出シーケンスを CRP で構成したデモです。以下の要素を組み合わせています。

- Animator ステート遷移 (`ChainAnimator`, `ChainCrossFade`)
- ボイス再生と字幕表示
- DOTween による移動 (`ChainDOTween`)
- 待機と `Sequence` / `Parallel` / `Race` の組み合わせ

キャラクターのアニメーション・ボイス・移動を CRP でどう構成するかの参考としてご利用ください。

## ChainDebugWindow

![UnityChan-3-DebugWindow.gif](https://github.com/user-attachments/assets/9ef797cf-babd-4147-b1dd-69eb1b1ebcc0)  

`ChainDebugWindow` は、プレイ中の **Chainツリー全体の状態** をリアルタイムで可視化するエディタウィンドウです。

**Window > Chain Debug** から開けます。

使用するには、ゲームコードから `ChainDebugWindow.Watch(chain)` を呼び出して、**監視対象となるルートChain** を登録します。

```csharp
void StartChain()
{
    var chain = new ChainSequence(/* ... */);
    ChainDebugWindow.Watch(chain);
    chain.Start();
}
```


## 動作環境

- Unity 2022.3 LTS (開発時は 2022.3.62f3)
- UniTask (Unity Package Manager により自動解決)
- DOTween (同梱)

## クレジットとライセンス

| コンポーネント | ライセンス | 備考 |
|---|---|---|
| 本プロジェクトのオリジナルコード | [MIT](LICENSE) | © 2026 Kenichi Morishita |
| [Unity-Chan!](https://unity-chan.com/) キャラクターデータ及び関連アセット | [UCL 2.02](https://unity-chan.com/contents/license_jp/) | © Unity Technologies Japan |
| [DOTween](http://dotween.demigiant.com/) | Demigiant | Free 版 |
| [UniTask](https://github.com/Cysharp/UniTask) | MIT | `Chain.Start()` の async 戻り値に使用 |

### Unity-Chan ライセンス表記

本プロジェクトは Unity-Chan License (UCL 2.02) に基づいて Unity-Chan キャラクターデータを使用しています。  
アセット配布条件は [README_UnityChan_jp.txt](Assets/README_UnityChan_jp.txt) を、ライセンス全文は [UCL 2.02 公式ページ](https://unity-chan.com/contents/license_jp/) を参照してください。  

<img src="imageLicenseLogo.png" width="150" alt="Licensed under Unity-Chan License">

## ライセンス

本プロジェクトのオリジナルソースコードは [MIT License](LICENSE) で公開しています。  
サードパーティ資産 (Unity-Chan, DOTween) はそれぞれのライセンスに従います。

