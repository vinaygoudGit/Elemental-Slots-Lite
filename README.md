[README.md](https://github.com/user-attachments/files/34315422/README.md)
# Elemental Slots Lite

A 5×4 reel slot game prototype themed around Fire, Water, Earth and Air, built in Unity 6.3 LTS (6000.3.12f1) for the Arkain Games Junior Unity Developer assignment.

It has a full slot loop (bet, spin, evaluate, pay), 1024-ways wins, DOTween animations, a Free Spins bonus, JSON gameplay analytics in the Console, and an RTP of about 95.5% verified by a built-in simulator.

## Features against the brief

| Requirement                         | Implementation                                                                                                                 |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| 5 reels × 4 rows                    | `SlotConfig.Reels` and `SlotConfig.Rows`, drawn by five `ReelController`s                                                      |
| 13 symbols                          | 10 regular, Wild, Scatter, Bonus (`SymbolId`). Bonus is optional and never spawns                                              |
| 1024-ways evaluation                | `WaysEvaluator` (4⁵ = 1024 paths)                                                                                              |
| Spin button and full spin cycle     | `UIController` → `GameController` → `ReelController`                                                                           |
| Win animation                       | Scale up and down pulse on the winning cells (`SymbolView.PlayWin`), driven by DOTween. The win amount also counts up and pops |
| RTP logged after every spin, 95–96% | `[RTP]` log line per spin, verified at about 95.5% by `RtpSimulator`                                                           |
| Random symbols each spin            | `SpinGenerator` (weighted random)                                                                                              |
| Win or loss shown on UI             | `WIN <amount>` or `NO WIN`                                                                                                     |
| Credit, bet and win displayed       | HUD texts                                                                                                                      |
| Gameplay events in the Console      | `AnalyticsManager`                                                                                                             |
| Bonus feature                       | **Free Spins** (3 Scatters give 3 free spins)                                                                                  |
| Unity Event System for UI           | Buttons are driven by the scene's `EventSystem`, plus C# static events between scripts                                         |

## How to run

**Build:** run `ElementalSlotsLite.exe` from the build folder. Keep the `_Data` folder next to it.

**Editor:** open the project in Unity 6.3 LTS, open `Assets/Scenes/SampleScene`, and press Play. The project uses the free DOTween package (in `Assets/Plugins/Demigiant`), which is already set up.

**Seeing the logs in the build:** a built `.exe` has no Console window, so `Debug.Log` output (the `[Analytics]` JSON lines and the `[RTP]` line) is written to `Player.log`. On Windows it is at `%USERPROFILE%\AppData\LocalLow\<Company Name>\<Product Name>\Player.log`. The same output appears in the Console when running in the editor.

**Controls:** click **Spin** to play a round. **Bet+** and **Bet-** change the bet between rounds (100, 200, 500, 1000). The buttons are locked while a round runs. **Quit** closes the game.

Starting credits are 100,000. Bets are multiples of 100 so every payout is an exact whole number of credits, which keeps the real RTP equal to the simulated one.

## Project structure

```
Assets/Scripts/
├── Core/        Pure C#, no Unity dependency (runs in a console too)
│   ├── SymbolId.cs          Enum of the 13 symbols
│   ├── SlotConfig.cs        Grid size, paytable, reel weights
│   ├── SpinGenerator.cs     Weighted random grid
│   ├── WaysEvaluator.cs     1024-ways scoring
│   ├── SpinResultData.cs    Result data types
│   └── RtpSimulator.cs      Headless RTP simulation
├── Game/
│   ├── GameController.cs    Game loop, credits, bet, free spins
│   └── GameEvents.cs        Static event hub
├── Reels/
│   ├── ReelController.cs    Scroll, stop and settle one reel
│   ├── SymbolView.cs        One symbol cell and its win pulse
│   └── SymbolSpriteLibrary.cs  SymbolId to sprite and tint
├── UI/
│   └── UIController.cs      HUD and buttons
├── Analytics/
│   └── AnalyticsManager.cs  JSON event logging and session RTP
└── Editor/
    └── RtpSimulatorMenu.cs  Tools > Slots > Simulate ... Spins
```

### Scene setup

- A `Canvas` with a `ReelsFrame` that holds five `Reel_0` to `Reel_4` objects (each with a `RectMask2D` and a `ReelController`) in a Horizontal Layout Group.
- A `SymbolCell` prefab (`SymbolView` with Background, Icon and Label children) that each reel instantiates at runtime.
- HUD texts and the Spin, Bet+, Bet- and Quit buttons, referenced by `UIController`.
- A `Systems` object with `GameController`, `UIController` and `AnalyticsManager`.
- A `SymbolSpriteLibrary` asset (`Create > Elemental Slots > Symbol Sprite Library`) with one entry per symbol.

## Code flow of one spin

```
Spin button click (EventSystem)
  → UIController raises OnSpinRequested
  → GameController: busy? enough credits?
  → deduct bet                        (OnCreditsChanged)
  → OnSpinStart
  → SpinGenerator.Generate()          random grid
  → WaysEvaluator.Evaluate()          win decided here
  → ReelController.Spin() × 5         animate towards the result
  → pay the win                       (OnCreditsChanged)
  → OnSpinResult                      UI shows WIN or NO WIN, analytics logs JSON and RTP
  → highlight winning cells (pulse)
  → if 3+ Scatters: free spins
```

The result is decided before any reel moves. The reel animation only presents it, so it can never change the outcome.

The reels work as a masked column of cells. While spinning, the cells scroll down and recycle. To stop, the target symbols are fed in from the top, bottom row first, so they land exactly on the visible rows, then the reel eases out with a small bounce (DOTween `OutBack`).

## Win evaluation: 1024 ways

There are no paylines. A symbol wins when it appears on **consecutive reels starting from reel 1**, in any row, with at least 3 reels in a row.

For each regular symbol:

1. Count how many cells on reel 1 are that symbol or a Wild, then on reel 2, and so on.
2. Stop at the first reel with a count of 0.
3. If the chain reached 3 or more reels, the number of ways is the product of the counts.
4. Win = ways × pay per way (for 3, 4 or 5 reels) × bet.

Example: Fire has counts 2, 1, 3 and then 0 on reel 4. That is 2 × 1 × 3 = 6 ways over 3 reels. At 0.78 per way that pays 4.68× the bet.

Wild substitutes for every regular symbol and only lands on reels 2 to 4. Scatter never joins a line. Wins only count from the left.

### Paytable

Pay per way, as a multiple of the total bet.

| Symbol | 3 reels | 4 reels | 5 reels |
| ------ | ------- | ------- | ------- |
| Fire   | 0.78    | 2.33    | 7.75    |
| Water  | 0.62    | 1.86    | 6.20    |
| Earth  | 0.46    | 1.55    | 4.65    |
| Air    | 0.39    | 1.24    | 3.88    |
| Lava   | 0.31    | 0.78    | 2.33    |
| Ice    | 0.23    | 0.62    | 1.86    |
| Leaf   | 0.16    | 0.46    | 1.55    |
| Cloud  | 0.16    | 0.46    | 1.55    |
| Gem    | 0.08    | 0.31    | 0.93    |
| Rune   | 0.08    | 0.31    | 0.93    |

## RTP

**How it is reached.** RTP (return to player) is the share of all money bet that the game pays back over the long run. It comes from two tables in `SlotConfig`: the paytable (what each symbol pays) and the per-reel symbol weights (how often each symbol appears). Rare symbols pay more.

**How it is verified.** `RtpSimulator` plays millions of spins using the real `SpinGenerator` and `WaysEvaluator`, with free spins included and no bet charged for them. The weights were fixed first, then the pays were scaled until the simulated RTP landed in the target band.

Results (six runs of 10 million spins, different seeds):

| Measure                 | Value                              |
| ----------------------- | ---------------------------------- |
| Total RTP               | 95.52% to 95.65%                   |
| Base game RTP           | about 90.8%                        |
| Free spins share of RTP | about 4.7%                         |
| Hit frequency           | about 48.8% of spins win something |
| Free spins trigger      | about 1 spin in 57                 |

**Reproduce it.** In the editor, use `Tools > Slots > Simulate 1M Spins` or `Simulate 10M Spins`. The report is printed to the Console.

**The RTP in the log.** After every spin the Console shows `[RTP] session=... reference=95.60%`. The session value is the real total won divided by total bet so far, so it swings widely over the first spins and only settles near the reference after thousands of spins. This is normal for a random game. The reference is the simulator's converged value.

## Bonus: Free Spins

- **Trigger:** 3 or more Scatters anywhere on the grid.
- **Effect:** 3 automatic spins that cost nothing and pay normally.
- **No retrigger:** Scatters during free spins do nothing.
- **Presentation:** the Scatter cells pulse, a banner shows the spins left, and the total bonus win is shown at the end.
- **Math:** the simulator plays the free spins too, so the RTP above already includes the bonus.

## Analytics

`AnalyticsManager` listens to the game events and prints one JSON string per event to the Console, prefixed with `[Analytics]`.

| Event             | When                                                                                                        |
| ----------------- | ----------------------------------------------------------------------------------------------------------- |
| `game_start`      | Once, when the scene loads                                                                                  |
| `spin_start`      | When a spin begins (paid or free)                                                                           |
| `spin_result`     | After all reels stop                                                                                        |
| `bonus_triggered` | When Scatters start the free spins                                                                          |
| `bonus_complete`  | After the last free spin                                                                                    |
| `bet_changed`     | When the player changes the bet                                                                             |
| `session_end`     | When the game closes, from the Quit button or by closing the window (logged once, from `OnApplicationQuit`) |

Examples:

```json
{"event":"spin_start","bet":100,"free_spin":false}
{"event":"spin_result","bet":100,"win":2173,"free_spin":false,"outcome":"win","scatters":0,"symbols":[["Fire","Water","Fire","Earth"],["Air","Fire","Water","Earth"],["Wild","Fire","Air","Fire"],["Water","Earth","Air","Water"],["Fire","Earth","Water","Air"]],"session_rtp":21.7300}
{"event":"bonus_triggered","type":"free_spins","spins":3}
{"event":"bonus_complete","type":"free_spins","win":450}
{"event":"bet_changed","bet":200}
{"event":"session_end","spins":42,"total_bet":4200,"total_win":3980,"session_rtp":0.9476,"credits":99780}
```

`symbols` lists the full grid, one array per reel from top to bottom.

## Event system

- **UI input** uses Unity's built-in `EventSystem`: the Button `onClick` events (Spin, Bet+, Bet-, Quit) are handled by the scene's EventSystem and input module. `UIController` turns each click into a request event, and `GameController` handles it.
- **Between scripts** a static C# event hub (`GameEvents`) is used. The controller raises events and the UI and analytics only subscribe, so no script depends on another directly. The hub clears its subscribers on entering Play mode so nothing stays attached between runs.

## Design decisions and assumptions

- **DOTween for animation.** The win pulse, the win amount count-up, the bonus banner and the final reel settle are tweens. The reel scroll itself is a frame loop, because symbols have to be recycled as the reel moves.
- **Result first, animation second.** The outcome is computed up front, and the reels animate towards it.
- **Core without Unity.** Everything in `Core` is plain C#, which is why the simulator can run millions of spins quickly, in the editor or a console.
- **Weighted independent cells.** Each cell is an independent weighted draw per reel. This is simple to tune and easy to reason about.
- **Bet levels of 100, 200, 500, 1000.** All paytable values have two decimals, so these bets always give whole-number payouts and never introduce rounding error into the RTP.
- **Scatter does not pay directly.** It only triggers the bonus.
- **Bonus symbol unused.** It is optional in the brief, so it exists in the enum but never appears.
- **No retrigger** keeps the bonus math simple and exactly simulated.

## Known limitations

- Wins are evaluated left to right only.
- A single session RTP reading is not meaningful on its own. Use the simulator for the converged value.
- The game uses a standard pseudo-random generator, which is fine for a prototype but not for a certified product.

## Credits

DOTween (Demigiant) is used for the animations: win pulse, win count-up, bonus banner pop and the reel settle.
