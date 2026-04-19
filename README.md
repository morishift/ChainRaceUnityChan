# ChainRaceUnityChan

A Unity demo project that showcases [ChainRacePattern (CRP)](https://github.com/morishift/ChainRacePatternUnity) using Unity-Chan.

**[日本語版はこちら (Japanese)](README_ja.md)**

- **Demo**: [ChainRaceUnityChan](https://morishift.github.io/ChainRacePatternUnity-Demo/ChainRaceUnityChan/)  

![UnityChan-1-Main.gif](https://github.com/user-attachments/assets/7be233d9-7401-4d72-b60a-61fe27844e6d)  

## About ChainRacePattern

ChainRacePattern is a design pattern for handling skippable presentation flows in Unity.
It represents animations, input, delays, and other presentation steps as Chains, and composes them declaratively with `Sequence`, `Parallel`, and `Race`.

For the full technical documentation — core rules, design philosophy, sample scenes, and the debug window — see the reference implementation:

**[ChainRacePatternUnity](https://github.com/morishift/ChainRacePatternUnity)**

## What This Project Demonstrates

This project is a self-contained demo that plays a short Unity-Chan presentation sequence using CRP. It combines:

- Animator state transitions (`ChainAnimator`, `ChainCrossFade`)
- Voice playback with subtitles
- Movement tweens via DOTween (`ChainDOTween`)
- Timed delays composed with `Sequence` / `Parallel` / `Race`

Use it as a reference for how CRP can orchestrate character animation, voice, and movement in a real scene.

## ChainDebugWindow

![UnityChan-3-DebugWindow.gif](https://github.com/user-attachments/assets/9ef797cf-babd-4147-b1dd-69eb1b1ebcc0)  

`ChainDebugWindow` is an editor window that visualizes the **state of the entire Chain tree** in real time while the game is playing.

Open it from **Window > Chain Debug**.

To use it, call `ChainDebugWindow.Watch(chain)` from your game code to register the **root Chain to watch**.

```csharp
void StartChain()
{
    var chain = new ChainSequence(/* ... */);
    ChainDebugWindow.Watch(chain);
    chain.Start();
}
```


## Requirements

- Unity 2022.3 LTS (developed on 2022.3.62f3)
- UniTask (resolved automatically via Package Manager)
- DOTween (bundled)

## Credits and Licenses

| Component | License | Notes |
|---|---|---|
| This project's original code | [MIT](LICENSE) | © 2026 Kenichi Morishita |
| [Unity-Chan!](https://unity-chan.com/) character data and related assets | [UCL 2.02](https://unity-chan.com/contents/license_en/) | © Unity Technologies Japan |
| [DOTween](http://dotween.demigiant.com/) | Demigiant | Free version |
| [UniTask](https://github.com/Cysharp/UniTask) | MIT | Used for async return value of `Chain.Start()` |

### Unity-Chan License Notice

This project uses Unity-Chan character data under the Unity-Chan License (UCL 2.02).
See [README_UnityChan_en.txt](Assets/README_UnityChan_en.txt) for the asset distribution notice and the [UCL 2.02 official text](https://unity-chan.com/contents/license_en/) for the license terms.

<img src="imageLicenseLogo.png" width="150" alt="Licensed under Unity-Chan License">

## License

This project's original source code is released under the [MIT License](LICENSE).
Third-party assets (Unity-Chan, DOTween) are subject to their own licenses as listed above.
