# Albal — GDD Parity Plan (supersedes the HTML-port plan)

**Spec source of truth: `README.md` (GDD, 2026-11-22).** The old single-file `game.html` reference and `2026-11-14_113000-defend-the-statue-unity-port.md` are superseded. Where GDD and code disagree, the GDD wins. Where the GDD disagrees with itself, see §6 — those need a user decision.

**Goal:** bring the existing Agate-MVC Unity port to GDD parity: 10-minute survive + boss "The Destroyer", 10-level statue, 4 skills per weapon, 3 monster types, 3 guard types, 4 tree types, teleport pods, adaptive difficulty.

**Architecture (unchanged):** one module folder per domain under `Assets/@Game/Scripts/Module/Scene/Gameplay/<Domain>/` (`Controller`/`Model`/`View`/`Connector`), one readonly-struct message per intent under `Module/Message/`, all tunables in `SOGameConstants` (`Assets/Resources/GameConstants.asset`), cross-module effects by message only. Rules live in plain-C# Models with EditMode tests; views only move/play/detect.

**Tech:** Unity 6 (URP 2D), `com.agate.mvc` 1.0.0, Input System 1.20, Test Framework 1.8 (EditMode), DOTween.

**Decisions locked (user, 2026-11-14):** D1 core scope played; D2 Unity colliders (no ported push-out loops; `PointToSegmentDistance` only for build validation); D3 placeholder primitives with one sprite slot per view; D4 keep `ProjectTA.*` namespace; D5 no persistence (restart = reload scene); D6 dev-only console, not a UI clone of the GDD admin panel.

---

## 0. AS-BUILT (re-checked against disk, 2026-11-22)

The old plan's Phases 0–5 are done: modules, messages, wiring, 136 EditMode tests, dev console, camera rig, balance log all exist. What exists is the **shell**; the **GDD numbers and mechanics** mostly do not.

| Module | Disk state | GDD delta |
|---|---|---|
| `PlayerCharacter/` | hp/stamina/gold, dash, block, charge; mana regen 0.05/frame idle | hp 50 / stamina 20 / speed 2 / size 30; mana starts 0 and cap grows +1 per kill; death → −30 statue HP + 3 s invincible respawn |
| `PlayerWeapon/` | model-only, damage/speed multipliers | keep; add damage table (sword 1/2/4, bow 1/2/4) |
| `PlayerSkill/` | `SlotCount = 3`, `SkillSlot` Skill1–3, unlocks 3/5/7, **paid upgrade panel** | 4 slots + `Skill4`, unlocks 3/5/7/9, mana 5/20/20/50; new effects Mind Control (sword 4), Guard Empower (bow 4); skill 3 sword = dmg 4, speed ×4, free stamina |
| `Statue/` | `MaxLevel = 7`, base HP 200, cost `500 × level`, 7-row effect table | 10 levels, HP 50→140 (+10/level), costs 20/25/40/50/60/70/90/110/140, gates on walls / recruited guards / kill counts, lv 8 teleport, lv 10 perimeter guards |
| `Territory/` | 3 rings, walls, gaps | keep geometry; wall HP 100, 8 segments/ring |
| `Tower/` | damage/range/rate, message-based hitscan | add HP 40, cost 20 essence, 4 per ring |
| `Trap/` | damage/durability/cooldown/radius/slow in constants | damage 1, cost 5, durability 10, cooldown 45 f, radius 35, slow 120 f |
| `Tree/` | single type, 20/ring, respawn 30 s, heal fraction | 4 types (Red/Purple/Magic/Arcana) hp 5/5/9/12, HP drop 1/0/2/3, mana drop 0/3/2/3, respawn 30/60/120 s, distribution 15/15/4/2 per ring; purple never eaten by monsters |
| `Guard/` | 2 tiers (lv3 inner, lv5 outer), no HP/speed in constants | 3 types: Inner (hp 100, 20e), Outer (hp 150, 25e), Perimeter (hp 200, 30e, lv 10); PATROL→RETREATING→HEALING→RETURNING→PURGING; Empower ×4 for 6 s |
| `Monster/` | one type, heal-radius stat variant only | 3 types: Stacker (hp 3, spd 1.0) / Waltroy (hp 10, spd 0.5) / Deasor (hp 2, spd 2.0); per-target damage matrix; eat-tree heal by tree type; retreat 1.5× under 30 % hp; mind-control ally |
| `Boss/` | hp 2000, projectile fan, enrage, 2/0/3 summons | hp 20, no projectile, no enrage, melee 10, phases >60 % / 30–60 % / <30 % summon 2 Stacker / 1 Stacker+1 Waltroy / 1 Waltroy+1 Deasor, phase-3 attack rate ×2, 8-minion escort (4/2/2), never retreats |
| `GameFlow/` | clock, boss trigger at 600 s, win/lose | keep; add player-death statue penalty route + GDD §17 stats |
| `Hud/` | hp/stam/mana/statue sliders, gold/arrows, 3+3 skill boxes | 8 skill boxes, circular stamina/HP/mana gauges, essence count, timer red at 9:00+, boss bar |
| `GameStateUi/` | pause, game over, results | add per-type kills, deaths, essence, max mana, statue level |
| `DevConsole/` | 8 flags + 8 actions | extend to GDD §16 list |
| `CameraRig/`, `BalanceLog/` | done | camera 0.45× preset for teleport select mode; `threat` column |
| — | **missing** | `Teleport/` (pods 20/10, unlock lv 8), `AdaptiveAi/` (Turtle/Guard/Neglect, safety net −50 %, buff cap 1.5×), visual state indicators (§21) |

**Divergences to keep:** `Build/` replaces `StatueMenu` (walk-to-statue placement, no timeScale panel); `TerritoryView.AddWall` owns wall instantiation (no `Wall/` module); tower damage stays message-based and the enemy projectile pool stays cancelled (GDD boss has no projectile).

---

## 1. Rebase rules (apply before feature work)

1. **Currency rename:** `Gold` → `Essence` in `PlayerCharacterModel`, `PlayerConstants.StartingGold`, `SpendGold`/`AddGold`, HUD text, `GameStateUiModel`, `BalanceLog` CSV column, tests. Mechanical commit, no logic change.
2. **Constants first, models second.** Every GDD number lands in `SOGameConstants` + `GameConstants.asset` before the model that reads it. No duplicate tunable in a prefab.
3. **Tests stay green.** 136 `[Test]` across 12 files must pass through the rebase; changed rules get their assertion updated in the same commit.
4. **One rule inside an existing model beats a new module** (monster retreat lives in `MonsterState`, not `Retreat/`).

---

## 1b. Progress log

**M0 — done on disk 2026-11-22, NOT yet compiled/tested (no Unity CLI on this host).**
- `Gold` → `Essence` everywhere: `PlayerCharacterModel.Essence`/`TrySpendEssence`/`AddEssence`, `StartingEssence`, `BaseEssenceValue`/`HealRadiusEssenceValue` (property + asset key), `RequestSpendEssenceMessage`, `DevAddEssenceMessage`, `DevConsoleAction.AddEssence`, `EssenceSpent`, `EssenceValue`, `RunStats.Essence`, HUD text `Essence {n}`, `GameConstants.asset` keys, `Gameplay.unity` (`_startingEssence`, `_essenceText`, `EssenceText (TMP)`), tests.
- Player: 50 HP, 20 stamina, block drain 0.5/s, arrow cooldown 4 s, starting essence 0. Swing 1 stamina, arrow 1 stamina, charged shot 2 stamina / 2 damage.
- Damage: sword 1, arrow 1; berserker ×4 speed/damage (was ×3).
- Structures: tower 40 HP (new `TowerConstants.Health`), tower damage 1, rate 50 f, size 32, range 300; wall HP 100 (new `TerritoryConstants.WallHealth`); trap cost 5.
- Costs: tower 20, wall 20, trap 5, guard lv3 20, lv5 25, new `GuardCostLevel10 = 30`.
- Tests updated: `PlayerCharacterModelTests` (hp/stamina/essence/block-drain/heal/arrow cadence/stamina depletion/damage), `TowerModelTests` (damage 1), `TrapModelTests` (cost 5), `PlayerSkillModelTests` (berserker ×4).

**M1 — done on disk 2026-11-22, not compiled/tested.**
- `PlayerCharacterModel`: `_manaCapacity` (cap earned, `+1`/kill via `RegisterKill()` which also adds the point), `Mana` starts 0, mana regen deleted (`PlayerConstants.ManaRegenPerSecond` + asset key gone, `_maxMana` = `StartingManaCapacity` = 0), `IsInvulnerable`/`Respawn()` (full HP + 3 s), `ApplyDamage` ignores hits while invulnerable, `DeathStatuePenalty = 30`.
- Death is no longer a game over: `GameFlowController.OnPlayerStatsChanged` deleted with its subscription; player death = teleport to statue + `RequestStatueDamageMessage(30)` + respawn (`PlayerCharacterController.OnRequestPlayerDamage`, new `RequestPlayerDamageMessage`).
- `PlayerCharacterView.Teleport(Vector2)` snaps the body and zeroes velocity.
- Heal-zone taint: `MonsterPoolController` publishes `HealZoneTaintMessage` on change (any live monster within 120 of origin); the player controller suppresses heal while tainted.
- Tests: mana suite rewritten (`GrantKills` helper, cap-per-kill, no-regen, spend clamp) + `Respawn_RestoresHealthAndGrantsThreeSecondsOfInvulnerability`.
- Still open in M1: nothing damages the **player** yet — monsters only bite the statue today. `RequestPlayerDamageMessage` exists but its publisher lands with M3 (monster damage matrix).

**Follow-ups discovered during M0** (fold into the listed milestone, do not forget):
- Sword skill 1 dash has **no contact damage** path today (GDD: 2). → M2.
- Bow triple shot / clone damage still come from multipliers over base 1 (GDD: triple 4/arrow, clone 2, lock-on ×2). → M2.
- Stamina passive regen is 9/s (GDD silent) — left untouched; revisit at M10.

## 2. Milestones (each ends in a commit)

**M0 — Rebase numbers (§4, §9, §12).** Currency rename; player 50/20/2/30; arrow cooldown 4 s; tower 40 HP + cost 20; wall 100 HP; trap retune; sword/bow damage table in constants.
Verify: EditMode green; play-mode glance (player, tower, trap still work).

**M1 — Mana + death cycle (§4).** Mana starts 0, `MaxMana` +1 per kill, **no regen** (idle regen deleted), tree chop is the only other mana source (lands with M5); death = −30 statue HP, 3 s invincible respawn near statue; heal zone 0.3 HP/frame, suppressed while an enemy is inside (tainted flag → HUD).
Files: `PlayerCharacterModel/Controller`, `StatueModel.ApplyDamage`, `Hud`.
TDD: mana cap grows on kill only; cast fails above mana; death applies exactly −30; regen stops when tainted.
Verify: EditMode (~6 asserts).

**M2 — Statue 10 levels + 4 skill slots (§5, §13).**
`StatueModel`: 10-row effect table (territory 2 @2, GuardInner @3, territory 3 @4, GuardOuter @5, Skill3 @7, teleport @8, Skill4 @9, GuardPerimeter @10) and the gate table (walls W1, 3 inner guards, walls W2, 6 outer guards, 100 kills, boss-or-150, boss-or-200, boss-and-250). `BuildRules.CanUpgradeToLevel` returns a reason enum, not a bool. lv 10 stays post-victory by design.
`PlayerSkillModel`: `SlotCount = 4`, `Skill4`, mana 5/20/20/50, unlocks 3/5/7/9; `SkillUpgradePanel`/`RequestToggleSkillUpgradePanelMessage` and its essence cost deleted outright (not just disabled).
TDD: 10 upgrade transitions with one blocked case per gate; unlock boundary at 3/5/7/9; skill 4 casts only above 50 mana.
Verify: EditMode; play mode — buy to lv 3, cast skill 1.

**M2a — done on disk 2026-11-22, not compiled/tested.**
- `EnumManager.SkillSlot.Skill4 = 4`; `PlayerSkillConstants` + asset gain skill-4 block (level 9, 50 mana, 20 s cooldown, 6 s, mind-control radius 180 / ally ×1.5, guard-empower radius 180 / ×4 damage / ×2 attack speed) — cooldown 20 s is the one number the GDD never states.
- `PlayerSkillModel`/`Controller`: `SlotCount = 4`, statu-level clamp 1…10, skill-4 arms in every switch, skill 4 refuses a recast while running (`HasRunningEffectGate`), duration 6 s. `PlayerSkillView` binds digit 4.
- `StatueModel`: 10 rows, `MaxLevel = 10`, HP 50 + 10/level, GDD price list `20/25/40/50/60/70/90/110/140`, new `UnlocksTeleport` on level 8, guard foundations at 3/5/10, skills at 3/5/7/9.
- `BuildRules.CanUpgradeToLevel` gains `bossDefeated` and levels 8/9/10 (boss-or-150, boss-or-200, boss-and-250); `StatueController` tracks `BossDefeatedMessage` and passes it.
- Dash implemented: `PlayerCharacterView.StartDash(AimDirection)` (Constants `DashSpeed`/`DashDurationSeconds`), controller pulses `RequestAreaDamageMessage(radius 25, 2 dmg, oncePerMonster)` while dashing and resets dash-hit flags on cast.
- Tests rewritten/added: `StatueModelTests` (10-row ladder, price list, HP curve), `BuildRulesTests` (levels 8–10 gates), `PlayerSkillModelTests` (unlock at 3/5/7/9, skill-4 mana + duration + recast), `PlayerCharacterModelTests` (clamp 10).
- Deliberately **not** in M2a: bow skill effects (triple shot 4/arrow, lock-on ×2, clone arrows at 2), mind control, guard empower, and the dead `SkillUpgradePanel` (it sells nothing — unlocks are statue-level; only a UI toggle). Those belong to M2b.

**M2b — done on disk 2026-11-22, not compiled/tested.**
- Bow skill 1 triple shot: `BulletPoolController.FireTripleShot()` — 3 arrows, ±15°, 4 damage each, 1 stamina + 1 arrow for the burst (GDD silent on ammo cost).
- Bow skill 2 lock-on: `RequestLockOnTargetMessage` → `MonsterPoolController` answers `LockOnTargetMessage` (nearest live monster, 600 range); next arrow is aimed at it via `Quaternion.FromToRotation(Vector3.up, -dir)` and does ×2.
- Bow skill 3 clone: main arrow at 2 damage + two clones at ±10°, 2 damage, ammo free (flag from `SkillEffectToggledMessage`).
- Sword skill 4 mind control: `MindControlMessage` → `MonsterModel.ApplyMindControl(center, radius, 6 s, ×1.5)`. Controlled monsters stop biting the statue and `CollectAllyAttacks` reports their swings against the nearest hostile monster; the pool applies them through the normal damage path so kills still pay essence. `MonsterState` gained `IsMindControlled` / `MindControlRemaining` / `AllyDamageMultiplier` + `StartAttackCooldown()`.
- Bow skill 4 guard empower: `GuardEmpowerMessage` → `GuardModel.ApplyEmpower` sets per-guard `DamageMultiplier` / `AttackSpeedMultiplier` / `EmpowerRemaining`; `TryFire` divides the interval by attack speed, `GetDamage` scales the hit, timers reset on expiry.
- Guard base damage default 2 → 1 (GDD §6) in `GuardConstants`.
- Tests: `GuardModelTests` (+3 empower: radius filter, interval halving, expiry, unrecruited guard), `MonsterModelTests` (+3 mind control: no statue bites, ally swing reporting, lapse).
- Not done on purpose: bow triple shot still fires alongside held-LMB fire (no burst cooldown of its own — GDD gives none); lock-on "homing" is one-shot aim, not steering.

**M3 — done on disk 2026-11-22, not compiled/tested.**
- `EnumManager.MonsterKind` (Stacker/Waltroy/Deasor); `MonsterKindStats` block per role (hp 3/10/2, speed ×1/×0.5/×2, essence 1/2/2, damage columns statue/structure/player/guard) in `MonsterConstants._kinds` + `GameConstants.asset`. Heal-radius stat variant deleted (base/healRadius hp, damage, essence fields gone) — kinds replace it.
- `MonsterState` carries `Kind`, the four damage columns, `SpeedMultiplier`, `IsRetreating`; `GetVelocity` multiplies base speed by the role multiplier.
- `CollectPlayerDamage(deltaSeconds, playerPosition, statuePosition)` — whoever is closer bites the pawn; `MonsterPoolController` publishes `RequestPlayerDamageMessage`, so the M1 death/respawn path is now live. Player transform arrives via `SetPlayerTransform` (wired in `GameplayLauncher`).
- Retreat: hurt (<30 %) + player within 120 → flee at ×1.5, no biting, steering away from the player.
- Spawn mix: `BuildRules.PickMonsterKind(seconds, roll)` — Stackers first 4 min, Waltroys join for 4–8 min, all three after 8 min; `RequestSpawnMonsterMessage` carries the kind.
- Tests: `MonsterModelTests` moved off 20 HP monsters onto real kind stats (Stacker, Waltroy-as-tank, Deasor-as-assassin, area/dash/heal cases rebased), `BuildRulesTests` +spawn mix.
- Still open in M3: structure damage (Waltroy ×4 vs walls/towers) and guard damage (Deasor ×4) have constants but no consumers — walls have no health model and guards die in M4. Eat-tree heal already skips nothing yet: Purple exclusion lands with the M5 tree kinds.

**M4 — done on disk 2026-11-22, not compiled/tested.**
- `GuardConstants`: HP by role (100/150/200), speed ×2 over the beast base speed, retreat ×1.5, retreat <30 % / return >70 %, heal 10 %/s.
- `GuardState`: `HomePosition`, `MaxHealth`/`Health`, `GuardStateKind` (Patrol/Retreating/Healing/Returning/Purging), bite clock. `GuardModel.Tick(dt, baseSpeed, zoneTainted, statueRadius)` walks the machine; `ApplyDamage` kills the guard and unarms the slot; `TryTakeBite` gates incoming bites.
- Zones: lv3 → territory 1, lv5 → band between territories 1 and 2, lv10 → outside territory 2; PURGING ignores its zone entirely.
- Foundations: 3 inner / **6 outer** (the level-6 gate needs six — the old 5 made the gate unreachable) / 4 perimeter. `GuardLayout.GetFoundationCount`, `BuildController` slot table (13 slots), `BuildRules.GuardCostLevel10`.
- Monster → guard damage: `GuardController` mirrors monster kinds from `MonsterSpawnedMessage`, bites any guard within 40 for that kind's `GuardDamage`, publishes `GuardDamagedMessage`.
- `GuardView.SetPosition`/`SetState` (green recovering, red purging) and `GuardConnector` subscribes `HealZoneTaintMessage`.
- `StatueController` gate counters now match exact tiers so perimeter guards cannot satisfy the six-outer gate.
- Tests: `GuardModelTests` (foundation counts, role HP, guard death, perimeter zone, full state-machine walk, purging).
- Not done: guard kill count (Guard Score for M7), guard count laid out per ring edge rather than exactly on territory 2's border.

**M5 — done on disk 2026-11-22, not compiled/tested.**
- `EnumManager.TreeKind` (Red/Purple/Magic/Arcana); `TreeKindStats` (health, hpDrop, manaDrop, respawn, count) with the GDD §10 table as both class defaults and asset values: 5/5/9/12 HP, hpDrop 1/0/2/3, mana 0/3/2/3, respawn 30/30/60/120, count 15/15/4/2 **per ring** (108 trees over three rings).
- `TreeState` carries `Kind` + `Stats` + `MaxHealth`; respawn/tick restore the kind's own numbers; `IsEdibleByMonsters` is false for Purple.
- `TreeController.SpawnTerritory` spawns the four kinds per ring; growth still adds Reds; chopping pays `RequestManaGainMessage` for the kinds with mana (area chop and single-tree hit, never a monster bite — `RequestTreeDamageMessage.FromMonster`).
- `TreeView.SpawnTree(..., kind)` tints by kind (red/purple/blue/gold).
- `MonsterPoolController` mirrors tree kinds, skips Purple in `FindNearestTree`, heals the monster by that kind's `HpDrop`, and flags its tree damage as `FromMonster`.
- `PlayerCharacterController.OnRequestManaGain` feeds the new second mana source (clamped to the earned cap).
- Tests: `TreeModelTests` (kind table, purple immunity, respawn per kind) and existing cases rebased off the old 100-HP tree.
- Not done: tree drops never despawn by themselves anymore (they were never itemised) — GDD only asks for the mana/HP drop, which is applied directly.

**M6 — done on disk 2026-11-22, not compiled/tested.**
- New `Teleport/` module (`TeleportModel`, `TeleportController`, `TeleportView`, `TeleportConnector`) + `TeleportMessages.cs`: 3 pods (one per ring, parked on the ring's top edge), unlock at statue level 8, `BuildCost = 20`, `TeleportCost = 10`.
- Flow: E on an unbuilt pod → spend 20, build, `TeleportPodBuiltMessage`; E on a built pod → select mode (`TeleportSelectModeMessage(true)`); E on a second built pod → spend 10, `TeleportPerformedMessage(destination)`; E again on the source or ESC cancels.
- `PlayerCharacterController.OnTeleportPerformed` → `PlayerCharacterView.Teleport`, so the pawn snaps without collision checks.
- `CameraRigModel.SetSelectMode` pulls the lens to `1/0.45` while select mode is on, restored on run end.
- Registered in both launcher lists, `GameplayView.TeleportView` slot added.
- Tests: `TeleportModelTests` (level gate, build once, two-pod rule, select/cancel, reset).
- **Editor step still required:** create a `Teleport` GameObject with `TeleportView` in `Gameplay.unity`, assign it to `GameplayView.TeleportView`, and optionally a pod prefab sprite. Without the view the logic still runs but no pods are visible or clickable.
- Not done on purpose: GDD's WASD camera pan in select mode (the same keys drive the pawn; zoom alone ships), and no confirm dialog — the second pod press is the confirmation.

**M7 — done on disk 2026-11-22, not compiled/tested.**
- New view-less `AdaptiveAi/` (`AdaptiveAiModel`, `AdaptiveAiController`, `AdaptiveAiConnector`) ticking on `RunClockMessage` (1/s).
- Scores: Turtle = smoothed (20 s) "camping the statue with nothing on it"; Guard = guard shots ÷ (guard shots + player kills) — a proxy, because kills are not tagged by killer; Neglect = 0.7·(1 − statue hp) + 0.3 if monsters stand within 120.
- `Threat = 0.4·turtle + 0.3·guard + 0.3·neglect`, halved when the statue or the player is under 25 %.
- `MonsterBuff = 1 + 0.5·threat` (cap 1.5). Published as `ThreatLevelMessage`; `MonsterPoolController` holds it and stamps every spawn; `MonsterModel.Add(..., statMultiplier)` scales hp, all four damage columns and speed.
- Tests: `AdaptiveAiModelTests` (turtle up/down, guard share, neglect, safety net, buff cap) and `MonsterModelTests.AdaptiveBuff_ScalesHealthDamageAndSpeed`.
- Not done: `BalanceLog` still writes the 5-column CSV — the `threat` column lands with the M10 balance session, where it is actually read. Monster spawn *rate* is untouched (threat only buffs quality, per GDD §14).

**M8 — done on disk 2026-11-22, not compiled/tested.**
- `BossConstants` rebuilt from GDD §8: 20 HP, 10 melee, 1 s base attack interval (×0.5 in phase 3), speed 2.5 (half the Stacker's base), spawn distance 60, summon radius 20. Per-phase kind bundles (`_phase1Summons` 2 Stacker, `_phase2Summons` Stacker+Waltroy, `_phase3Summons` Waltroy+Deasor) and the 8-minion `_escort` (4/2/2). Projectile fields and the speed/damage multipliers are gone.
- `BossModel`: `_pendingSummons` is a kind list, `TakeSummons()` returns it, `AttackIntervalSeconds` reads the phase, `IsEnraged`/`SpeedMultiplier`/`DamageMultiplier` deleted.
- `BossController`: no projectile fan, damage is the flat 10 (god mode still gates it), escort spawned on arrival in a ring around the boss, summons use each bundle's kinds.
- Tests: `BossModelTests` rewritten (20 HP, bundle per phase, escort composition, attack-rate doubling, drain-once, reset).
- Leftovers on purpose: `BossView.SpawnProjectileFan` is now unused API (delete with the M9 view pass); boss size 80 is not applied (prefab scale is an Editor edit).

**M9 — partially done on disk 2026-11-22, not compiled/tested.**
- Done: skill grid text (8 lines, sword + bow, unlock level + mana cost) built at runtime; statue bar turns red on `HealZoneTaintMessage`; timer starts red at 9:00 (was 9:30); results screen gains per-kind kills, deaths and max mana (`RunStats` + `Format`, fed by `MonsterKilledMessage.Kind` and the new `PlayerDiedMessage`); `CheatType.InfiniteEssence` flag wired through `PlayerCharacterModel.TrySpendEssence` and the console panel.
- Still open (all need Editor work, not code): circular gauges, per-skill cooldown countdown (needs a skill-state message), monster state icons (eating/retreat/mind-controlled) — art-dependent, guard state circles are already colour-coded in `GuardView`, DevConsole `Unlock All` (map to `DevMaxLevelMessage`) and `Build All` actions.

**M10 — Balance (§14, §17).** One 10-minute debug session → `balance_<stamp>.csv`; tune `GameConstants.asset` only. Acceptance: statue alive at 10:00 in a competent session, boss beatable, run length 600 ± 60 s.

---

## 3. Test policy

| Layer | How |
|---|---|
| Rules (Models) | Test Runner → EditMode → Run All (real gate) |
| Physics, HUD, feel | Editor Play mode (manual; no Unity CLI on this host) |
| Full loop | M10 balance session + CSV |

Every model keeps ≥3 EditMode assertions; each GDD table (monster damage matrix, tree types, statue ladder, guard transitions, boss phases) gets one assertion per row/cell. Shell-written code is unverified until the Editor runs it.

---

## 4. Hot spots

- `GameplayLauncher.cs` — 2 new controllers (`Teleport`, `AdaptiveAi`) must land in *both* `GetSceneDependencies()` and `GetSceneConnectors()`.
- `GameplayView.cs` + `Gameplay.unity` — one serialized field per new view; scene edits in the Editor only.
- `SOGameConstants.cs` + `GameConstants.asset` — all GDD numbers, one asset.
- `EnumManager.cs` — append `SkillSlot.Skill4` (serialized order: append, never renumber), new `MonsterKind`, `GuardKind`, `TreeKind`.
- `StatueModel.Effects` 7→10 rows; `BuildRules.CanUpgradeToLevel` gold-only → full gate set.
- `PlayerCharacterModel.Gold` → `Essence` (ripples into HUD, results, balance CSV, tests).

---

## 5. Order rationale

M0–M2 change shared numbers and enums (everything reads them), so they go first and alone. M3–M5 are independent gameplay domains and can be taken in any order after M2. M6–M7 depend on statue levels and monster spawns. M8 depends on M3 (summons spawn monsters by kind). M9 depends on all of the above. M10 is the acceptance run.

---

## 6. Resolved readings (user 2026-11-22: "the GDD is the main instruction, follow the GDD")

GDD wins over both the HTML reference and existing code. Where the GDD is silent, the reading below is the assumption to implement — change only on an explicit GDD edit.

| # | Question | GDD-literal decision |
|---|---|---|
| 1 | Mana regen | **None.** Mana starts 0, `MaxMana` +1 per kill, skills spend it. Only other source is chopping magic-bearing trees (Purple 3 / Magic 2 / Arcana 3 — §10 "mana drop"). `PlayerCharacterModel` idle regen is deleted, not tuned. |
| 2 | Tree count | **Per ring**, as written. 3 rings → 45 Red / 45 Purple / 12 Magic / 6 Arcana = 108 trees. |
| 3 | Statue ladder | As written, including lv 10 gate "Boss kalah **DAN** 250 kill" — so lv 10 (perimeter guards) is only reachable after the boss dies; lv 8/9 use the kill path pre-boss (150 / 200). |
| 4 | Skill unlock | Unlock by statue level 3/5/7/9, mana cost only. The paid upgrade panel and its cost are deleted. |
| 5 | Boss HP | **20**. 20 sword hits, 10 charged-bow shots, 5 berserker hits. Not a typo. |
| 6 | Essence sources | **Monster kills only** (1/2/2). Trees and guards yield no essence. |

Consequences to absorb, not to "fix": skill 4 needs 50 mana = 50 kills minimum; lv 10 is effectively post-victory; 108 trees on a 3-ring map is dense (M5 placement spacing may need a GDD edit rather than a code workaround — raise it then, don't silently cap).
