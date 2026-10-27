<div align="center">

<h1>ClawgueLike</h1>

<p><strong>Physics-based, single-player roguelike arcade game for Steam.</strong><br />
Pull coins from a chaotic pile, build cursed synergies, and pay the rent before the run ends.</p>

<p>
  <a href="https://unity.com/"><img src="https://img.shields.io/badge/Unity-6000.5.0f1-000000?style=flat-square&amp;logo=unity&amp;logoColor=white" alt="Unity 6000.5.0f1" /></a>
  <a href="https://learn.microsoft.com/dotnet/csharp/"><img src="https://img.shields.io/badge/C%23-239120?style=flat-square&amp;logo=csharp&amp;logoColor=white" alt="C#" /></a>
  <a href="https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.5/manual/index.html"><img src="https://img.shields.io/badge/URP-17.5-4B4B4B?style=flat-square" alt="Universal Render Pipeline 17.5" /></a>
  <a href="https://store.steampowered.com/"><img src="https://img.shields.io/badge/platform-Steam-1B2838?style=flat-square&amp;logo=steam&amp;logoColor=white" alt="Platform: Steam" /></a>
  <a href="#project-status"><img src="https://img.shields.io/badge/status-in%20development-orange?style=flat-square" alt="Status: in development" /></a>
</p>

<p>
  <a href="https://github.com/xAgesx/ClawgueLike/commits/main"><img src="https://img.shields.io/github/last-commit/xAgesx/ClawgueLike?style=flat-square&amp;color=blue" alt="GitHub last commit" /></a>
  <a href="https://github.com/xAgesx/ClawgueLike/stargazers"><img src="https://img.shields.io/github/stars/xAgesx/ClawgueLike?style=flat-square" alt="GitHub stars" /></a>
  <a href="https://github.com/xAgesx/ClawgueLike/forks"><img src="https://img.shields.io/github/forks/xAgesx/ClawgueLike?style=flat-square" alt="GitHub forks" /></a>
  <a href="https://github.com/xAgesx/ClawgueLike/issues"><img src="https://img.shields.io/github/issues/xAgesx/ClawgueLike?style=flat-square" alt="GitHub issues" /></a>
  <a href="https://github.com/xAgesx/ClawgueLike"><img src="https://img.shields.io/github/repo-size/xAgesx/ClawgueLike?style=flat-square" alt="GitHub repository size" /></a>
  <a href="https://github.com/xAgesx/ClawgueLike/search?l=c%23"><img src="https://img.shields.io/github/languages/top/xAgesx/ClawgueLike?style=flat-square" alt="Top language" /></a>
</p>

<p>
  <a href="https://github.com/xAgesx/ClawgueLike/issues/new"><img src="https://img.shields.io/badge/report%20a%20bug-open%20an%20issue-red?style=flat-square" alt="Report a bug" /></a>
  <a href="https://github.com/xAgesx/ClawgueLike/issues/new"><img src="https://img.shields.io/badge/request%20a%20feature-open%20an%20issue-brightgreen?style=flat-square" alt="Request a feature" /></a>
</p>

</div>

## Overview

**ClawgueLike** is a physics-based, single-player roguelike arcade game in development for Steam. A run is a sequence of rounds, turns, and pulls: the player steers a magnetic claw across a chaotic pile of 3D coins, drops it into the machine, and lets the magnet drag coins toward the chute. Collected coins are spent in the shop on special coins, relics, cheats, and rerolls, while Fragments buy permanent upgrades. Every round ends with rent — pay it, and the run continues; miss it, and the run is over.

Inspired by [RACCOIN: Coin Pusher Roguelike](https://store.steampowered.com/app/3784030/), ClawgueLike trades frantic, time-pressured arcade play for a deliberate, cozy-yet-tactical puzzle pace. There is no timer: players have full horizontal control on the X/Z axes, then commit to an automated drop. The result is a game about positioning, physics, and build synergy rather than reflexes.

### Design pillars

- **Physics first** — coins, gravity, magnets, and chain reactions decide every outcome.
- **Tactical pacing** — a limited number of pulls replaces twitch reflexes.
- **Double-edged progression** — every relic grants a boon and a curse.
- **Rent-gated runs** — short, replayable runs that end when the rent comes due.

## Project Status

| Area | Status |
|---|---|
| Special coins | **30 / 30** implemented, plus 1 extra coin |
| Cursed relics | **10 / 50** implemented |
| Core loop | Playable in `Assets/Scenes/GameScene.unity` |
| Target platform | Steam |

**Legend:** ✓ implemented · ✗ planned

## Table of Contents

- [Overview](#overview)
- [Project Status](#project-status)
- [Getting Started](#getting-started)
- [Game Design](#game-design)
  - [Game Loop](#game-loop)
  - [Special Coins](#special-coins)
  - [Cursed Relics](#cursed-relics)
  - [Open Design Questions](#open-design-questions)
  - [Technical Notes](#technical-notes)
- [Project Structure](#project-structure)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgements](#acknowledgements)

## Getting Started

### Prerequisites

- [Unity **6000.5.0f1**](https://unity.com/releases/editor/archive) with the modules required for your target platform
- [.NET SDK](https://dotnet.microsoft.com/download) for command-line build verification
- Git

### Run locally

```bash
git clone https://github.com/xAgesx/ClawgueLike.git
cd ClawgueLike
```

1. Add the project folder to Unity Hub and open it with Unity **6000.5.0f1**.
2. Open `Assets/Scenes/GameScene.unity`.
3. Press **Play**.

> The first import can take a few minutes while Unity rebuilds the `Library/` folder.

### Controls

| Input | Action |
|---|---|
| **WASD** | Move the claw across the X/Z plane |
| **Space** | Drop the claw and activate its magnetic field |
| **Mouse** | Navigate the shop and UI |

### Build verification

```bash
dotnet build Assembly-CSharp.csproj
```

Unity generates the `.csproj` files when the project is opened. A healthy build completes with **0 warnings and 0 errors**.

## Game Design

This README doubles as the living design document for the game.

### Game Loop

#### Run structure

A run consists of **rounds → turns → pulls**. By default, each round has **3 turns** and each turn has **2 pulls**; relics can modify either count.

```text
ROUND
├─ Turn 1 ── 2 pulls ──► Shop
├─ Turn 2 ── 2 pulls ──► Shop
└─ Turn 3 ── 2 pulls ──► Shop
                            │
                            └─► Pay rent ──► next round
                                (can't pay ──► game over)
```

#### One turn

```text
Pull:  WASD move ──► Space: drop + magnet ON ──► auto-return ──► magnet OFF ──► coins fall & score
Turn:  2 pulls  ──►  Shop
```

The player moves the claw with **WASD** and presses **Space** to drop it and activate its magnetic field. The claw then returns automatically to the drop chute and switches the field off, letting the attracted coins fall and be scored. When the turn ends, the **shop** opens.

#### Shop

Coins are spent on **special coins, relics, cheats, and rerolls**; Fragments are spent on **upgrades**.

**Rerolls**

- **Special coins** — a single reroll button; its price increases with each use and resets every turn.
- **Relics and cheats** — the stock rerolls automatically whenever one is purchased, and the slot refills.

#### Currencies

| Currency | Spent on |
|---|---|
| **Coins** | rerolls · special coins · cheats · relics |
| **Fragments** | upgrades |

#### Round end

At the end of every round, the player must **pay rent**. Paying continues the run; failing ends it in a **game over**.

#### Core rules

- The chute and the coin pile are separated by a wall. Coins can reach the chute only through a claw drop, a tornado, antigravity, or an explosion.
- Coins carry a **`baseValue`** (int, minimum 1) and a **`multiplier`** (float, default 1). All coin math operates on these values, never on a running total: `TotalValue = round(baseValue × multiplier)`. Multiplier bonuses stack multiplicatively, while base-value additions stack additively.
- The claw does **not** hold coins; it only attracts them. Designs that assume held coins are invalid.

#### Shop slots

| Slot type | Base | Max |
|---|---|---|
| Coin slots | 2 | 5 |
| Relic slots | 3 | 4 |
| Cheat slots | 3 | 5 |

### Special Coins

**Progress: 30 / 30 implemented**, plus one additional coin outside the 30.

| Status | # | Coin | Price | Trigger | Effect |
|:---:|---:|---|---:|---|---|
| ✓ | 1 | RabbitCoin | 3 | touching another RabbitCoin | Spawn 1 RabbitCoin worth half its value; both turn grey and can never breed again |
| ✓ | 2 | TornadoCoin | 10 | turn start | A tornado picks up nearby coins and sends them flying for 5 seconds |
| ✓ | 3 | CrystalCoin → **SleepingCoin** | 7 | surviving a turn untouched | +1 value per turn since spawning (shipped as SleepingCoin) |
| ✓ | 4 | SwarmCoin | 7 | touching any coin | Spawn three TinyCoins, each worth 1 |
| ✓ | 5 | SnowballCoin | 8 | touching a coin | +1 per unique coin it touches (max +6) |
| ✓ | 6 | VampireCoin | 8 | touching another coin | Permanently steal 1 base value |
| ✓ | 7 | MineCoin | 9 | being touched for the first time | Explode and scatter every nearby coin |
| ✓ | 8 | GlueCoin | 5 | touching 2+ coins at once | Fuse into one coin worth their total |
| ✓ | 9 | SpringCoin | 5 | hitting the floor | Bounce back up and knock into other coins |
| ✓ | 10 | DiceCoin | 4 | entering the chute | Pay out a random value between 1 and 6 |
| ✓ | 11 | RouletteCoin | 3 | entering the chute | Double the payout or lose it entirely (50/50) |
| ✓ | 12 | ClonerCoin | 13 | turn start | Duplicate the cheapest coin in the machine |
| ✓ | 13 | KingCoin | 12 | touching another coin | Crown it for +50%; only one crown can exist at a time |
| ✓ | 14 | HydraCoin | 11 | touching another coin | Split that coin into two, each worth half |
| ✓ | 15 | HomingCoin | 7 | turn start | Drift toward the most valuable coin it can reach |
| ✓ | 16 | BankerCoin | 9 | entering the chute | Round your coin total up to the nearest 10 |
| ✓ | 17 | CinderCoin | 2 | touching other coins (up to 8) | Give +1 to a coin, but pay 0 yourself |
| ✓ | 18 | SkeletonCoin | 1 | entering the chute | Worth nothing, but grants a free reroll |
| ✓ | 19 | EchoCoin | 6 | touching a coin that moved this turn | Replay its value gain on a random coin |
| ✓ | 20 | BlackHoleCoin | 14 | touching a coin worth 4 or more | Swallow it and keep half its value |
| ✓ | 21 | SeedCoin | 5 | entering the chute | Plant a seed where it fell, sprouting a coin worth 3 next turn |
| ✓ | 22 | MeteorCoin | 13 | dropping onto a pile | Crash in and scatter everything within reach |
| ✓ | 23 | GreedyCoin | 9 | each coin collected this turn | +1 value (max +7) |
| ✓ | 24 | AntigravCoin | 12 | turn start | Halve gravity for 5 seconds so the whole pile drifts |
| ✓ | 25 | CensusCoin | 7 | turn start | Gain +1 for every special coin sitting in the machine |
| ✓ | 26 | TallyCoin | 9 | entering the chute | Pay 1 for every coin collected this turn, doubled |
| ✓ | 27 | CaterpillarCoin | 8 | collected on its first turn | Cocoon, then hatch next turn worth 3× |
| ✓ | 28 | ChrysalisCoin | 7 | surviving a full turn untouched | Transform into a random special coin |
| ✓ | 29 | MutationCoin | 8 | touching a coin | Apply a random permanent change to it — good or bad |
| ✓ | 30 | DiamondCoin | — | not buyable / rare spawn | No effect; worth 10 instead of 1 |

#### Additional implemented coins

| Status | Coin | Price | Trigger | Effect |
|:---:|---|---:|---|---|
| ✓ | InfectionCoin *(formerly DoubleDownCoin)* | 2 | touching another coin | Doubles the value of the first two coins it touches |

#### Support coins

Support coins are spawned by effects and cannot be purchased.

| Coin | Base value | Spawned by | Notes |
|---|---:|---|---|
| MiniRabbitCoin | 2 | RabbitCoin | Worth half the parent's value; does not breed |
| TinyCoin | 1 | SwarmCoin | Its 2-turn lifetime belongs to relic #34 (not yet implemented) |

#### Implementation notes

- **#3 CrystalCoin** evolved into **SleepingCoin**: it now gains value every turn since spawning (currently `valuePerTurn = 5`) instead of only on untouched turns.
- **InfectionCoin** is the renamed former DoubleDownCoin. It sits outside the 30 and is wired into the shop coin pool.
- The shop coin pool currently holds **30 coins**: the 29 purchasable design coins plus InfectionCoin.
- **DiamondCoin (#30)** is intentionally not purchasable and has no effect. It is wired into `ItemSpawner.rareCoinPool` with a **1%** spawn chance, so it appears as a rare normal coin worth 10.
- Chute-driven coins (Dice, Roulette, Banker, Tally, Skeleton, Seed, Caterpillar) use the `CoinEffect.OnCollected` hook. Greedy uses `OnCoinCollected`, and the turn-based coins use `OnTurnStart`; the effect assets remain stateless.

### Cursed Relics

**Progress: 10 / 50 implemented.**

Every relic is double-edged: it grants a **passive boon and a curse**. Percentage modifiers act on `multiplier`; flat modifiers act on `baseValue`.

> **Example — All In (22):** chute coins get **×1.5**, but you **lose 10% of your balance at the start of every turn**.

| Status | # | Relic | Price | Trigger | Boon | Curse |
|:---:|---:|---|---:|---|---|---|
| ✗ | 1 | Lodestone | 30 | passive | Chute coins get a ×1.15 multiplier | A coin delivered alongside 9 or more other coins in the same pull pays nothing |
| ✓ | 2 | All In | 22 | passive | Chute coins get a ×1.5 multiplier | Lose 10% of your coin balance at the start of each turn |
| ✗ | 3 | Assayer's Loupe | 22 | passive | Plain coins spawn with +1 baseValue | Each plain coin you deliver destroys one random normal coin still in the machine |
| ✗ | 4 | Gilded Chute | 35 | passive | Delivered coins get +3 baseValue | Every 10th coin you deliver spawns a worthless junk coin |
| ✓ | 5 | PiggyBank | 40 | end of round | Bank 10% interest on your coin balance | Interest is forfeited if your final turn delivers fewer than 5 coins |
| ✗ | 6 | Opening Act | 20 | each turn | The first pull each turn: delivered coins get a ×2 multiplier | The final pull each turn: delivered coins get a ×0.75 multiplier |
| ✗ | 7 | Heavy Purse | 25 | passive | While holding 150+ coins, +1 pull per turn | Magnet force ×0.8 |
| ✗ | 8 | Bargain Hunter | 26 | passive | Shop prices −20% | The shop offers one fewer coin slot |
| ✓ | 9 | Long Arm | 28 | passive | Magnet radius ×1.4 | Pull force ×0.75 |
| ✗ | 10 | Anchored Grip | 28 | passive | Pull force ×1.5 | The claw swings wider, making it harder to aim |
| ✗ | 11 | Twin Talons | 50 | passive | +1 pull per turn | Coins delivered on the extra pull get a ×0.8 multiplier |
| ✗ | 12 | Feather Claw | 20 | passive | The claw moves and drops 40% faster | Deceleration is halved, so the claw overshoots |
| ✗ | 13 | Slingshot | 24 | passive | Coins near the chute are nudged in | The nudge also scatters the pile away from the chute |
| ✗ | 14 | Fat Grip | 30 | passive | +1 pull per turn | Special coins spawn with −1 baseValue (minimum 1) |
| ✗ | 15 | Steady Claw | 28 | passive | Attracted coins do not spin | Magnet force ×0.85 |
| ✗ | 16 | Wide Net | 34 | passive | The magnet attracts 2 coins at once | Attracted coins are released early |
| ✓ | 17 | Richer Vein | 30 | passive | +15 normal coins spawn each round | Magnet force ×0.8 while the machine holds 60+ coins |
| ✓ | 18 | Overstock | 34 | passive | +1 shop coin slot | −1 relic slot |
| ✓ | 19 | Haggler | 30 | passive | Shop prices −20% | Prices creep up 5% with each reroll this round |
| ✓ | 20 | Fence | 24 | passive | +2 free rerolls each shop visit | Rerolls after the free ones cost double |
| ✗ | 21 | Bulk Deal | 26 | passive | Resupply costs −25% | You cannot buy coins from the shop this round |
| ✗ | 22 | Deep Pocket | 30 | passive | Start each round with +40 coins | −1 pull on the first turn of each round |
| ✗ | 23 | Curator | 28 | passive | +1 relic slot | Buying a relic destroys a random normal coin in the machine |
| ✗ | 24 | Hourglass | 40 | passive | Rounds last +1 turn | The extra turn has no shop, and delivered coins get a ×0.5 multiplier |
| ✗ | 25 | Quickdraw | 30 | first pull | The first pull each turn is free | That free pull cannot grab special coins |
| ✗ | 26 | Metronome | 45 | on turn start | Turn-start special effects fire twice | Contact effects are 25% weaker |
| ✗ | 27 | Overclock | 34 | passive | Special coins spawn with +1 baseValue | A random special coin goes dormant for one turn each round |
| ✗ | 28 | Momentum | 26 | passive | Each consecutive pull this turn adds +8% magnet force | If a pull delivers nothing, the bonus resets and you lose 5 coins |
| ✗ | 29 | Rabbit Hutch | 32 | passive | Rabbit offspring are worth full value | Every litter also leaves a worthless Stray coin |
| ✗ | 30 | Winter Coat | 28 | passive | Snowballs gain +2 baseValue per new coin | A snowball that gains nothing for a turn loses 25% of its baseValue |
| ✗ | 31 | Fang Charm | 34 | passive | Vampires drain 2 per sip | When a vampire is collected, it drains 5 coins from your balance |
| ✗ | 32 | Percussion Cap | 30 | passive | Mines blast 50% wider | When a mine detonates, you lose 3 coins for each coin it blasts |
| ✗ | 33 | Rubber Sole | 26 | passive | Springs bounce 60% higher | A hard bounce can wedge coins into a dead corner of the machine |
| ✗ | 34 | Hive Queen | 32 | passive | Swarms spit out 2 extra TinyCoins | TinyCoins disappear after 2 turns |
| ✗ | 35 | Lullaby | 30 | passive | Sleeping coins gain +1 extra baseValue per turn | A sleeping coin resets to baseValue 1 if it is ever bumped |
| ✗ | 36 | Viral Load | 34 | passive | All InfectionCoins get a multiplier equal to the number of InfectionCoins present | Each InfectionCoin drains 3 coins from your balance at the end of the round |
| ✗ | 37 | Sticky Fingers | 30 | passive | A sticky coin gives coins attached to it a multiplier equal to the number attached | Sticky coins have a chance to destroy an attached coin |
| ✗ | 38 | Lodestone Core | 30 | passive | Special coins have double magneticAttraction | Plain coins are 50% less magnetic |
| ✗ | 39 | Overcharge | 36 | passive | Special-coin effects are 25% stronger | Special coins have a 1-in-5 chance not to spawn |
| ✗ | 40 | Sapper | 28 | passive | Mines re-arm and can detonate once each turn | Detonation radius is much smaller |
| ✗ | 41 | Mating Season | 34 | passive | Swarms and rabbits spawn one extra each | Magnet force ×0.85 |
| ✗ | 42 | Resonance | 32 | passive | Delivering a coin emits a small blast that can knock more coins into the chute | The blast also scatters the pile away from the chute |
| ✗ | 43 | Kindling | 26 | passive | Snowballs add +1 baseValue on touch | Snowballs grow twice as fast |
| ✗ | 44 | Prospector | 28 | passive | Fragment drop rate +25% | −1 special coin in the shop |
| ✗ | 45 | Gluttony | 28 | passive | Delivered coins also give Fragments equal to their baseValue | Coin balance is halved at the end of each round |
| ✗ | 46 | Greed | 24 | passive | Every 20 coins delivered spawns 10 more coins in the machine | The 20th coin pays nothing (it spawns instead) |
| ✓ | 47 | Risk Taker | 22 | passive | Each delivered coin has a 50% chance to pay double (×2) | Each delivered coin has a 50% chance to pay nothing (×0) |
| ✓ | 48 | Blood Pact | 26 | passive | +1 pull every turn | Lose 8% of your balance at the end of each round |
| ✓ | 49 | Juggernaut | 30 | passive | Magnet force ×1.4 and radius ×1.25 | −1 pull every turn |
| ✗ | 50 | Cracked Lens | 26 | passive | The shop's first slot is always 30% off | All other shop prices +10% |

#### Implemented relics

**#2** All In · **#5** PiggyBank · **#9** Long Arm · **#17** Richer Vein · **#18** Overstock · **#19** Haggler · **#20** Fence · **#47** Risk Taker · **#48** Blood Pact · **#49** Juggernaut

Card status text (three words or fewer) appears in the always-visible relic strip:

| Relic | Card text |
|---|---|
| All In | `+50% chute` |
| PiggyBank | `+{total} so far` (running total earned) |
| Long Arm | `+40% range` |
| Richer Vein | `+15 coins` |
| Overstock | `+1 coin` |
| Haggler | `-20% prices` (live, including reroll creep) |
| Fence | `{n} rerolls left` (live) |
| Risk Taker | *(none)* |
| Blood Pact | `+1 pull` |
| Juggernaut | `+40% force` |

#### Remaining relic roadmap

IDs refer to the relic table above. The list below covers the 40 relics that are not yet implemented, grouped by the additional work each one requires.

**Group A — buildable with existing hooks** (the required scalars are already exposed): 4, 7, 8, 14, 15, 21, 22, 23, 44, 45, 46, 50 *(borderline: 3, 38)*

**Group B — needs per-pull context** (pull start and end signals): 1, 3, 6, 10, 11, 24, 28, 38

**Group C — needs edits inside special-coin effects**: 26, 27, 29, 30, 31, 32, 33, 34, 35, 36, 37, 39, 40, 41, 43

**Group D — needs new physics or systems**: 12, 13, 16, 25, 33, 42, plus the TinyCoin 2-turn lifetime

> Some relics appear in more than one group (e.g., 3, 33, and 38) because they require a combination of these changes. Relic 34 is also tied to Group D through the TinyCoin lifetime.

### Open Design Questions

1. **Curse stacking** — Do multiple relics' curses stack multiplicatively? Are the resulting values floored?
2. **Active relic cap** — Should there be a hard cap of 5 owned relics, or should the cap be limited only by shop slots?
3. **Random curses** — Should the random curses on #35, #36, and #47 stay?
4. **Type renaming** — Should the C# types be renamed to `CursedRelicData` / `CursedRelicEffect`, or should `RelicData` / `RelicEffect` remain, with "Cursed Relic" treated as a display label only?
5. **Asset cleanup** — Should the orphaned `TestRelic.asset`, `TestRelic 1.asset`, and `TestRelic 2.asset` be removed?

### Technical Notes

- **Runtime architecture** — Data-only `RelicData` assets reference shared, stateless `RelicEffect : ScriptableObject` implementations. All live state is held by `RelicManager` and `GameManager`.
- **Special coins** — `CoinData` assets reference shared, stateless `CoinEffect : ScriptableObject` implementations. Per-instance state lives on `CoinInstance` / `SpecialCoin`.
- **Coin hooks** — contact (`Apply`), turn start (`OnTurnStart`), landing (`OnImpact`), chute payout (`OnCollected`), and per-collection (`OnCoinCollected`) callbacks keep effect assets stateless. Delayed pods, crowns, and movement fields own their runtime state on separate components.
- **Owned relics** — Relics are stored in `GameManager.RelicInventory`. `RelicManager` aggregates boons and curses and exposes turn-start and round-end callbacks.
- **Relic UI** — `RelicsInventoryUI` is an always-visible panel, both in the shop and during gameplay. Each owned relic shows its icon, a status of three words or fewer, and a hover tooltip with the full description and live status.
- **Build verification** — `dotnet build Assembly-CSharp.csproj` should complete with 0 warnings and 0 errors.

## Project Structure

```text
ClawgueLike/
├── Assets/
│   ├── Data/                 # ScriptableObject assets (coins, relics, cheats, items)
│   ├── Materials/
│   ├── Prefabs/
│   ├── Scenes/               # GameScene and MainMenu
│   ├── Scripts/
│   │   ├── Coins/            # Coin data, instances, and special-coin effects
│   │   ├── Data/
│   │   ├── Shop/             # Shop, relics, cheats, and relic effects
│   │   └── UI/
│   ├── Settings/             # URP configuration
│   ├── TextMesh Pro/
│   └── TutorialInfo/
├── Packages/                 # Unity package manifest
├── ProjectSettings/          # Unity project configuration
├── ClawgueLike.slnx
└── README.md
```

## Roadmap

- [x] Core run loop: rounds, turns, pulls, and rent
- [x] Shop with special coins, relics, cheats, and rerolls
- [x] 30 special coins and 1 additional coin
- [x] 10 cursed relics
- [ ] 40 remaining cursed relics
- [ ] Steam page and playable build

## Contributing

ClawgueLike is primarily a solo project, but feedback and bug reports are welcome. To contribute:

1. Open an issue describing the bug or feature before starting significant work.
2. Fork the repository and create a topic branch.
3. Keep changes focused and follow the existing project structure.
4. Verify that `dotnet build Assembly-CSharp.csproj` completes with 0 warnings and 0 errors.
5. Open a pull request with a clear description of the change.

## License

No license has been specified for this repository, so all rights are reserved by the author. If you would like to reuse any part of the project, please open an issue to discuss it.

## Acknowledgements

- Inspired by [RACCOIN: Coin Pusher Roguelike](https://store.steampowered.com/app/3784030/).
- Built with [Unity](https://unity.com/) 6000.5.0f1 and the [Universal Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.5/manual/index.html).
- UI text uses [TextMesh Pro](https://docs.unity3d.com/Packages/com.unity.ugui@2.5/manual/index.html).

<div align="center">
  <sub>Last updated: 2026-10-27</sub>
</div>
