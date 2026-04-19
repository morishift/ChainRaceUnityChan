# CLAUDE.md - ChainRacePatternUnity

> このファイルはプロジェクトの設計判断・規約・注意事項を記録する。作業を通じて必要と判断された考慮事項は随時このファイルに追記していく。

## プロジェクト概要

ChainRacePattern（CRP）は、演出フローの順次・並列・競争制御とスキップを宣言的に記述する設計パターン。
Unity向けの参照実装であり、Chain/Sequence/Parallel/Raceの組み合わせで複雑な演出を構成する。

## 最重要ルール：Chain実装の規約

新しいChainを実装する際は、以下のルールを**必ず**守ること。

### StartInternal()
- 開始時の処理を実装する
- 処理が完了したら `Complete()` を呼び出す
- `isFastForward` が `true` の場合、アニメーション開始やリソース確保などの不要な処理を省略し、最終状態に設定してから即座に `Complete()` を呼ぶ

### SkipInternal()
- 呼び出された場合は、直ちに最終状態へ遷移する
- **`Complete()` を呼び出してはならない**（呼び出しても無視される。Chain基盤側が処理する）
- スキップは「停止」ではなく「最終状態への即時遷移」である

### その他
- `StartInternal()` / `SkipInternal()` はそれぞれ最大1回しか呼ばれない
- Chainは内部に状態を持つため、使い回さず毎回 `new` して利用する

## 設計哲学

- **スキップ＝最終状態への即時遷移**であり、停止や中断ではない
- スキップ時の責務は外側で一括処理せず、各Chainに閉じ込める
- 演出全体は `Sequence` / `Parallel` / `Race` の合成として表現する
- CancellationTokenによるスキップ制御は使わない。CRPの哲学と相容れない
- DOTweenは数値補間のツールとして使い、フロー制御はCRP側で行う

## 演出スキップの実装

ゲームやアプリの演出では、ユーザーがボタンを押してカットシーンや演出を飛ばせるようにすることがよくあります。
通常の実装では、「スキップボタンが押されたときに、実行中のアニメーションを個別に停止・完了させる処理」を書く必要があり、演出が複雑になるほどスキップ処理も煩雑になります。

ChainRacePattern では、この問題を以下のように扱います。

1. ユーザー入力を `ChainButton` のようなChainとして表現する
2. `ChainRace` にアニメーションのChainと `ChainButton` を並べて実行する
3. ボタンが押されれば `ChainRace` が残りのアニメーションを自動的にスキップする

```csharp
// スキップボタンが押されるか、アニメーションが最後まで再生されるか、
// どちらか先に起きた方で次に進む
new ChainRace(
    new ChainButton(skipButton),
    new ChainSequence(
        演出A,
        演出B,
        演出C
    )
)
```

各Chainは `SkipInternal()` でスキップ時の終了処理を実装しているため、スキップが発生しても正しい最終状態に遷移します。そのため、外側のフロー制御に場当たり的なスキップ処理を書き散らさずに済みます。

## ChainRace で最初に完了した Chain をその場で判別する

`ChainRace` では、最初に完了した `Chain` 以外はスキップされます。
そのため、各候補の末尾に `ChainAction(Action<bool>)` を置き、`isFastForward` を見れば、どの経路が最初に完了したかをその場で判別できます。

```csharp
new ChainRace(
    new ChainSequence(
        new ChainSample1(),
        new ChainAction(ff =>
        {
            if (!ff)
            {
                Debug.Log("Sample1が最初に完了");
            }
        })
    ),
    new ChainSequence(
        new ChainSample2(),
        new ChainAction(ff =>
        {
            if (!ff)
            {
                Debug.Log("Sample2が最初に完了");
            }
        })
    ),
    new ChainSequence(
        new ChainSample3(),
        new ChainAction(ff =>
        {
            if (!ff)
            {
                Debug.Log("Sample3が最初に完了");
            }
        })
    )
);
```

### 仕組み

`ChainRace` では、ある `Chain` が先に完了すると、残りの経路はスキップ消費されます。
このとき、敗者側の後続 `Chain` は fastForward 扱いで実行されるため、`ChainAction(Action<bool>)` に渡される `ff` を見れば勝者を判別できます。

* `ff == false` → fastForward されていない → その経路が最初に完了した
* `ff == true` → スキップ消費中に実行された → 敗者側の経路

### 向いているケース

この書き方は、**勝者が誰かをその場で処理すれば十分なとき** に向いています。

* 勝者だけログを出したい
* 勝者ごとに即座に分岐したい
* 後段でまとめて判定する必要がない

### 使用上の位置づけ

この方法は便利ですが、ややトリッキーです。
`ChainRace` の内部挙動と fastForward の伝搬を前提にしているため、常用するというよりは、必要な場面で限定的に使う想定です。

### 注意：ChainRace 自体が fastForward された場合

この方法で勝者を判別できるのは、`ChainRace` 自体が通常進行で終了した場合に限ります。
`ChainRace` 自体が fastForward された場合は、配下の各レーンも fastForward 扱いで消費されるため、`ChainAction` に渡される `ff` はすべて `true` になります。
そのため、このケースでは「どの Chain が最初に完了したか」は判別できません。

## フォルダ構成

```
Assets/Scripts/ChainRaceScripts/
├── Core/          # CRPの根幹クラス
│   ├── Chain.cs
│   ├── ChainAction.cs
│   ├── ChainHalt.cs
│   ├── ChainNop.cs
│   ├── ChainParallel.cs
│   ├── ChainRace.cs
│   └── ChainSequence.cs
├── Ext/           # 外部依存のある拡張Chain
│   ├── ChainDelay.cs
│   ├── ChainAnimator.cs
│   ├── ChainWork.cs
│   ├── ChainButton.cs
│   ├── ChainCrossFade.cs
│   ├── ChainCrossFadeWaitState.cs
│   └── ChainDOTween.cs
└── Editor/        # Editorのみ
    └── ChainDebugWindow.cs
```

- **Core**: 外部依存なし。CRPの構造そのもの
- **Ext**: Unity API、DOTween等に依存する拡張Chain
- **Editor**: `#if UNITY_EDITOR` 専用。ビルドには含まれない

## コアクラスの役割

| クラス | 説明 |
|---|---|
| `Chain` | 基底クラス。全てのChainはこれを継承する |
| `ChainSequence` | 複数のChainを順番に実行 |
| `ChainParallel` | 複数のChainを同時に実行。全て完了で終了 |
| `ChainRace` | 複数のChainを同時に実行。1つ完了で残りをスキップ |
| `ChainAction` | 単一のActionデリゲートを実行して即完了。`Action` と `Action<bool>` の2種のオーバーロードがあり、`bool` 版には `isFastForward` が渡される |
| `ChainHalt` | 完了しないChain（外部からのSkipでのみ終了） |
| `ChainNop` | 何もせず即完了 |

## DOTweenとの連携ルール

- DOTweenは個々のアニメーション（移動、色変化、フェード等）に使用する
- `ChainDOTween` でDOTweenのTweenをChainに乗せる
- DOTweenのSequence/Joinによるフロー制御は使わない（CRP側で行う）
- SkipInternal()では `tween.Complete(withCallbacks: true)` で最終状態に飛ばす

## ChainDOTween実装パターン

```csharp
public class ChainDOTween : Chain
{
    Func<Tween> tweenFactory;
    Tween tween;

    public ChainDOTween(Func<Tween> tweenFactory)
    {
        this.tweenFactory = tweenFactory;
    }

    protected override void StartInternal()
    {
        if (isFastForward)
        {
            tween = tweenFactory.Invoke();
            tween?.Complete(withCallbacks: true);
            tween = null;
            Complete();
            return;
        }
        tween = tweenFactory.Invoke();
        tween.OnComplete(() => {
            tween = null;
            Complete();
        });
    }

    protected override void SkipInternal()
    {
        if (tween != null && tween.IsActive())
        {
            tween.Complete(withCallbacks: true);
            tween = null;
        }
    }
}
```

## 新しいExtension Chainを作る際のテンプレート

```csharp
public class ChainXxx : Chain
{
    // コンストラクタでパラメータを受け取る

    protected override void StartInternal()
    {
        if (isFastForward)
        {
            // 最終状態を設定
            Complete();
            return;
        }
        // 通常の開始処理
        // 完了時に Complete() を呼ぶ
    }

    protected override void SkipInternal()
    {
        // 最終状態への即時遷移
        // Complete() は呼ばない
    }
}
```

## やってはいけないこと

- SkipInternal() の中で Complete() を呼ぶ
- CancellationToken でスキップを実装する
- DOTweenのSequence/Join でフロー制御する（CRPの責務）
- Chainインスタンスを使い回す（毎回newする）

## Unity UI Image.material の共有マテリアル問題

Unity の UI `Image` コンポーネントは、`.material` プロパティが**共有マテリアルアセットを直接返す**。
`MeshRenderer.material` のようなインスタンス自動生成は行われないため、Play Mode 中に `SetFloat` や DOTween で値を変更するとアセット本体が書き換わる。

**対策：** `Start()` で明示的にインスタンスを作って差し替える。

```csharp
void Start()
{
    // Instantiate the material to avoid modifying the shared material asset at runtime
    irisImage.material = new Material(irisImage.material);
}
```

これ以降は `irisImage.material` がインスタンスを返すため、アセット本体は汚染されない。

## Chain デバッグ機能

### ChainDebugWindow

`Window > Chain Debug` で開くEditorWindow。Play Mode中にChainのステータスをツリー形式でリアルタイム表示する。

**使い方：**
```csharp
#if UNITY_EDITOR
ChainPattern.Editor.ChainDebugWindow.Watch(chain);
#endif
```
`Watch()` を呼ぶと以降そのChainを監視する。`UnityChanScene.ChainStart()` で毎ループ呼んでいる。

**表示内容：**
- 各Chainのクラス名・状態（Ready / Started / Skipped / Completed）
- 開始後の経過秒数（Started中はカウント中、Completed/Skipped時点で停止）
- FastForwardで実行された場合は `[FF]` 表示
- 状態別の個数サマリー
- `Show Completed` トグルでCompleted済みChainの表示切替（OFFでも完了から約1秒間は表示し続け、その後フェードアウト）

**ファイル：**
`Assets/Scripts/ChainRaceScripts/Editor/ChainDebugWindow.cs`

### Chain に追加したデバッグプロパティ

`Chain` 基底クラスに以下のプロパティを追加している（すべて `#if UNITY_EDITOR` のみ有効）：

| プロパティ | 内容 |
|---|---|
| `DebugTypeName` | クラス名 |
| `DebugState` | 状態名（"Ready" / "Started" / "Skipped" / "Completed"） |
| `DebugElapsedSeconds` | 開始後の経過秒数。未開始は -1、完了/スキップ後は固定値 |
| `DebugIsFastForward` | FastForwardで実行されたか |
| `DebugChildren` | 子Chainの一覧（複合Chainでオーバーライド） |

### 複合Chainのデバッグ対応

`ChainSequence` / `ChainParallel` / `ChainRace` はそれぞれ `debugChainList` を持ち、コンストラクタや `Add()` で子Chainが登録されたタイミングで追加する。`DebugChildren` はこのリストをそのまま返すため、未実行・実行中・完了済み・スキップ済みを問わず全ての子Chainを網羅できる。表示順序は登録順（定義順）になる。

## 依存ライブラリ

- [UniTask](https://github.com/Cysharp/UniTask) - Start() の戻り値として使用
- [DOTween](http://dotween.demigiant.com/) - Ext/ChainDOTween で使用（無料版）

## コミットメッセージ規約

Conventional Commits形式を使用する。
- `feat:` 新機能
- `fix:` バグ修正
- `chore:` 雑務・素材追加
- `docs:` ドキュメント
- `refactor:` リファクタリング
- `test:` テスト
