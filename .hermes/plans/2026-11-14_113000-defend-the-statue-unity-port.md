> **SUPERSEDED 2026-11-22.** Spec source is now `README.md` (GDD "Albal"). Current plan:
> `.hermes/plans/2026-11-22_albal-gdd-parity.md`. This file stays as the build history of the
> HTML-reference port (module shell, messages, tests, wiring).

# Defend the Statue — HTML → Unity (Agate MVC) Port Plan

**Goal:** Port the single-file HTML reference game (`game.html`, 10-min survive + boss) into the existing Unity 2D project as Agate-MVC modules wired by pub/sub messages, with all game rules living in plain-C# Models covered by EditMode tests.

**Architecture:** One module folder per domain under `Assets/@Game/Scripts/Module/Scene/Gameplay/<Domain>/` (`Controller`/`Model`/`View`/`Connector`), one readonly-struct message per intent under `Module/Message/`, tunables in ScriptableObjects under `Assets/Resources/SOGameConstants.asset`, cross-module effects carried by messages only — no direct controller-to-controller references.

**Tech stack:** Unity 6 (URP 2D, `Rigidbody2D.linearVelocity` confirmed in `PlayerCharacterView.cs`), `com.agate.mvc` 1.0.0, Input System 1.20, Unity Test Framework 1.8 (EditMode), NaughtyAttributes, DOTween.

**Decisions locked (user, 2026-11-14):**

| # | Question | Decision |
|---|---|---|
| D1 | Scope | **Playable core = Phases 0–2.** Player, skills, statue, territories, walls, towers, trees, traps, HUD, monsters (Task 3.1–3.4 are required for Phase 2 to be playable). Boss, guards, dev console are post-core. |
| D2 | Physics | **Unity colliders** — `Rigidbody2D` + `BoxCollider2D`/`CircleCollider2D`. The HTML `resolve*Collision` push-out loops are **not** ported to runtime paths; `WallModel.PointToSegmentDistance` survives only for build-placement validation and gap-zone tests. |
| D3 | Art | **Placeholder primitives** (square player/monster, circle tree, rect walls). Every view exposes one sprite slot so real art is a pure swap. No asset dependency in any phase. |
| D4 | Namespace | Keep `ProjectTA.*` and `GameplayLauncherLauncher`. Renaming mid-port touches every file for zero behaviour. |
| D5 | Persistence | None. A run is pure in-memory; restart = `SceneLoader.Instance.LoadScene(TagManager.SCENE_GAMEPLAY)`. |
| D6 | Admin panel | Deferred with Phase 5. When it lands: dev-only console (cheat flags broadcast via `CheatToggledMessage`), not a UI clone of the HTML panel. |

---

## 0. AS-BUILT STATUS (re-checked against disk, 2026-11-17 @ `8992e16`)

Plan phases 0, 1, 2 and Tasks 3.1–3.4 are **done**. Phases 4–5 untouched. Everything below in sections 1–9 is the original spec; where disk disagrees, this block wins.

| Plan item | State | Where |
|---|---|---|
| 0.1 EditMode asmdef | done | `Assets/@Game/Scripts/ProjectTA.asmdef`, `Assets/@Game/Tests/EditMode/ProjectTA.Tests.EditMode.asmdef` |
| Tests | 136 `[Test]` in 12 files | `Tests/EditMode/*.cs` (BuildRules 13, GameFlow 13, Input 2, Monster 22, PlayerCharacter 22, PlayerSkill 17, PlayerWeapon 5, SlowField 6, Statue 8, Territory 10, Tower 10, Tree 8) |
| 1.1–1.8 Player slice | done | `PlayerCharacterModel`, `PlayerWeaponModel`, `PlayerSkillModel`, `SlowFieldModel`, `CombatMessages.cs` (RequestCastSkill/SkillCast/SlowFieldAdded/SpendGold/SpendArrows/SpendStamina/HeavyShot/PlayerStatsChanged/WeaponSwitched/SkillEffectToggled/SkillMultipliers), `HudView` sliders |
| 2.1 Statue | done | `Statue/{Model,Controller,View,Connector}`, `StatueMessages.cs`; ladder lives in `BuildRules.CanUpgradeToLevel` and is called from `StatueController` |
| 2.2–2.3 Territory | done | `TerritoryModel` + `TerritoryLayout` + `TerritoryView` (4 bars/ring dimmed until unlocked, `AddWall`/`ClearWalls` over `_wallPrefab`) |
| 2.4 Walls | done, different shape | **no `Wall/` module.** `BuildController` emits `WallSegmentBuiltMessage(territory, slot, start, end)`; `TerritoryView.AddWall` instantiates the wall prefab (collider lives on the prefab). Gap geometry stays in `TerritoryModel`/`BuildRules` |
| 2.5 Tower | done | `Tower/{Model,Controller,View,Connector}`; fire = `TowerFiredMessage` + `RequestDamageMonsterMessage` (hitscan, not a projectile) |
| 2.6 `EnemyProjectilePool` | **skipped** | tower damage is applied by message. Only add a pool when the boss needs orbs |
| 2.7 Tree | done | `Tree/{Model,Controller,View,Connector,Hitbox}`, `TreeMessages.cs`, `Resources/Prefabs/Tree.prefab`, `TreeView` in scene; growth loop, build clearing, attack/arrow damage, 30 s respawn, monster heal-at-tree; `TreeChoppedMessage` → `GameFlowModel.RegisterTreeChopped()` |
| 2.8 SpikeTrap | done | `Trap/{Model,Controller,View,Connector,Trigger}`, `TrapMessages.cs`, `Resources/Prefabs/Trap.prefab`, `TrapView` in scene; foundations spawn at each ring's 4 wall gaps once walls finish, player builds via `TrapBuiltMessage`, trigger → `RequestDamageMonsterMessage` + `SlowFieldAddedMessage`, durability/cooldown in `TrapModel` |
| 3.1–3.4 Monster | done | `Monster/{Model,Controller,View,Pool*,Hitbox}`, `MonsterMessages.cs` |
| 3.5 Guard | done | `Guard/{Model,Controller,View,Connector,Layout}`, `GuardMessages.cs`, `Resources/Prefabs/Guard.prefab`, `GuardView` in scene; foundations at statue lv3 (3) / lv5 (5), recruited by walking to one (`GuardBuiltMessage`, 500/750 gold), lv3 guards only hit monsters inside territory 0, lv5 only outside; `StatueController` now counts recruited guards for the lv4/lv5 ladder gates |
| 4.1 Boss trigger + win/lose | done | `GameFlowModel.BossTriggerSeconds = 600`, `TryTriggerBoss()` once per run, `MarkDefeatIfDead`, `MarkVictory` (final, blocked by defeat); `BossMessages.cs` (`BossSpawnedMessage`, `BossDefeatedMessage`); controller publishes `BossSpawnedMessage` at 600 s, `GameStateMessage(GameWin)` on boss defeat, `GameOver` on player death |
| 4.2 Boss model | done | `Boss/BossModel.cs`, `BossConstants` in `SOGameConstants` + asset; 3 phases at 60%/30% hp, enrage only at phase 3, speed ×1/1.25/1.5, damage ×1/1.25/2, summons owed 2/0/3 drained by `TakeSummons()` |
| 4.3 Boss view + controller | done | `Boss/{BossView,BossController,BossConnector,BossHitbox}`, `CheatMessages.cs`, `CheatType.GodMode`; walks at the statue, fans 1/3/5 tracers by phase, summons 2/0/3, `RequestStatueDamageMessage` skipped while god mode is on; arrows hit via `BossHitbox`, swords via `RequestAreaDamageMessage`; scene `Boss` object + `GameplayView.BossView` wired |
| 4.4 Timer HUD + boss bar | done | `RunClockMessage` published once per whole second from `GameFlowController`; `HudView` builds mm:ss clock and boss bar at runtime, blinks from 9:30, solid red ≥10:00; `BossSpawnedMessage` / `BossDamagedMessage` / `BossPhaseChangedMessage` / `BossDefeatedMessage` drive bar + phase label |
| 4.6 Results screen | done | `GameStateUiModel` mirrors clock/kills/skill3/traps/trees/gold/statue level; `GameStateUiView` prints `RunStats.Format()` on GameOver/GameWin; both Restart buttons call `GameStateUiView.RestartRequested` → `SceneLoader.LoadScene("Gameplay")`; `TrapTriggeredMessage` added for the trap count |
| 5.1 Dev console | **done** | `DevConsole/` model/view/controller; F1 panel, 8 flags, 8 actions, gated by `SOGameConstants.IsDebugBuild` |
| 5.2 Skill 3 unlock at lv7 | **done (already present)** | `PlayerSkillModel.IsUnlocked` reads `StatueLevel` against constants 3/5/7; `PlayerSkillController.OnStatueUpgraded` re-reads the level; `PlayerSkillModelTests` covers 3/5/7 |
| 5.3 Camera rig | **done** | `CameraRig/` model/view/controller/connector; view sits on Main Camera, pushes `CinemachineCamera.Lens.OrthographicSize` toward the target in `LateUpdate` (lerp 0.1); follow stays with Cinemachine, plain position lerp only as fallback when no CinemachineCamera is wired; zoom 1.0 ↔ 0.85 on `SkillUpgradePanelToggledMessage`, restored on run end |
| 5.4 Balance | **instrumented, pass pending** | `BalanceLog/` model/controller/connector (view-less, connector-only wiring): writes `persistentDataPath/balance_<stamp>.csv` in dev builds — header `seconds,statue_hp,monsters,kills,boss`, one row per 30 s off `RunClockMessage`, `summary,<last>,<boss_arrival>,<kills>,<rows>` on GameOver/GameWin. The 10-minute comparison session itself still has to be played |

**Divergences to keep (not regressions):**
- **StatueMenu replaced by Build module.** Build/upgrade happen by walking to the statue (`BuildView.IsOverStatue`, cursor placement), no `Time.timeScale = 0.15` panel, no movement auto-close. Cheaper, and R3 (global timeScale) never had to be solved. Do not re-add the panel.
- **Skill unlock is a paid upgrade panel, not statue lv3/5/7.** `RequestToggleSkillUpgradePanelMessage` / `SkillUpgradePanelToggledMessage` + `GameStateUi`. `StatueUpgradeEffect.UnlockedSkill` still names the gated slot.
- **`GameStateUi` module exists although the plan never listed it** — pause/`PauseToggledMessage`, game-over state, skill upgrade panel. Keep it; results screen belongs here later.
- **`TowerModel`/`MonsterModel` own more state than planned** (`MonsterState` per instance, `CollectStatueDamage`, `FindNearestInRange`). Tests already cover it — extend, don't rewrite.
- Statue `CanUpgrade(gold)` checks gold only; the guard/kill/wall ladder is `BuildRules.CanUpgradeToLevel` called by the controller. Two methods, one truth — keep it that way.

**Next up (unchanged order, core-then-post):** play one 10-minute session on a debug build, read `balance_<stamp>.csv` (boss arrival, monster count and statue hp at 5/10 min), tune `GameConstants.asset` to match the reference. `Wall/`, `StatueMenu/`, `EnemyProjectilePool/` stay deleted from the module map.

---

## 1. Verified current state (read from disk, not assumed)

| Thing | Fact |
|---|---|
| Namespace root | `ProjectTA.*` — **keep it**, do not rename to the folder name `DefendTheStatue` |
| Framework | `Agate.MVC.Base` / `Agate.MVC.Core`; launcher is `SceneLauncher<TLauncher, TView>` named `GameplayLauncherLauncher` (typo is upstream — leave it) |
| Existing gameplay modules | `PlayerCharacter`, `Bullet`, `BulletPool`, `Sword`, `Input`, plus global `GameConstants`, `GameSettings` |
| Input asset | `Character` map already has **Move, Shoot, Aim, Switch, Build**; `Build` is already bound to `<Keyboard>/e` in `InputActionManager.inputactions` — port prep has started |
| Scene | `Assets/@Game/Scenes/Gameplay.unity`, only 2 `Collider2D` in the whole scene → no physics bodies exist yet |
| Constants | `Assets/Resources/GameConstants.asset` (`SOGameConstants`) holds `PlayerConstants` + `ShootingConstants` only |
| Git | `397abeb feat: switch weapon`, `f510749 feat: move and aim` — project is at the "one weapon works" stage |
| Framework quirk | DI is **type-keyed, first-registration-wins**. `GetSceneDependencies()` instance is the live one; launcher/connector `new()` fields are placeholders overwritten by field-type injection. A per-object sub-controller built with `new` (one per pooled object) is never registered — pass its deps through an explicit `Init(view, ...)`. |

### Reuse / delete
- **Reuse unchanged:** `GameplayLauncherLauncher`, `GameplayView`, `SceneLoader`, `GameMain`, `SettingsController`, `TutorialController`, `GameConstantsController`, `InputController` (extend bindings), `BulletPool` (player arrows), `BulletView`, `BulletController`, `SwordView`/`SwordController`.
- **Migrate:** `BulletPoolModel.IsWeaponActive` currently doubles as the sword/gun switch; the reference game has 2 weapons × 3 skills, so weapon state moves into a new `PlayerWeaponModel` and `BulletPoolModel` keeps only fire-rate/ammo gating.
- **Delete:** nothing destructive — no dead 3D code present.

## 2. Reference-game inventory (what must exist at parity)

Grouped exactly as the HTML implements them; each line is a module or a rule inside one.

1. **Player** — WASD move (`player.speed 4`), mouse aim, scroll weapon switch (sword/bow), LMB attack, RMB block (sword) / charge shot (bow), hp 100, stamina 50, mana 20, gold 50000.
2. **Skills** (2 sets × 3, unlocked at statue level 3/5/7):
   - Sword: (1) dash to cursor, 12f, 18 speed, contact 15 dmg; (2) Time Slash — 360r field, 6s, enemies ×0.2 speed; (3) Berserker — 5s, speed ×3, damage ×3.
   - Bow: (1) 5-arrow spread, 10 dmg, spread ±0.35; (2) Lock Shoot — 8s homing, damage ×2, range 600; (3) Clone Mode — weapon lock + 2 clones, attack cooldown ×0.5.
3. **Ammo** — `arrows = min(6, statueLevel)`, +1 per 10s, multi-shot costs 5.
4. **Statue** — hp 200 (+100 @lv6, +200 @lv7), level 1→7, heal radius 120, interaction radius 100.
5. **Territories** ×3 — 800×600 / 1600×1200 / 3200×2400, unlocked at level 2 and 4.
6. **Towers** ×4 per territory (corner positions, offset 50, cost 200, range 300, 15 dmg, rate 50).
7. **Walls** ×8 segments per territory (cost 125, gap half-width 60 → 4 gaps) + wall collision + gap-zone escape test.
8. **Guards** — lv3: 3 × cost 500, inner territory; lv5: 6 × cost 800, outer ring, must stay out of inner territory. Patrol + detect 200 + attack 40r/20dmg.
9. **Spike traps** — one per wall gap, cost 150, 15 dmg, 10 durability, 45f cooldown, 35r, slow ×0.4 for 2s.
10. **Trees** — 20 per territory + 1 growth per 60s, 100 hp, block movement, drop on tower/wall build.
11. **Monsters** — spawn interval `max(40, 180 − minutes·14)` frames, path to nearest wall gap, attack statue, hp 20/40 (30/40 near heal radius), gold 5/15.
12. **Boss** — spawns at 10:00, hp 2000, 3 phases (>60% chase+summon, >30% ranged 3-shot, ≤30% enrage ×2 speed/dmg), defeat = victory.
13. **Game flow** — survive/lose when statue hp ≤ 0 or player hp ≤ 0; win = boss dead; stats screen.
14. **HUD** — statue bar, boss bar+phase, timer (blink at 9:30/10:00), stamina/hp/mana radial gauges, weapon+gold+ammo box, 2×3 skill grid, defense counters.
15. **Statue interaction** — **as-built: no panel.** Walk to statue → `BuildView.IsOverStatue` enables build/upgrade via `BuildController` (`RequestBuildTowerMessage`/`RequestBuildWallSegmentMessage`/`RequestUpgradeStatueMessage`). No timeScale slow-mo. Original panel design is dead.
16. **Admin panel** — 8 cheats + 8 instant actions. Port as a **dev-only** console module (see Task 8.3), not a UI clone.

## 3. Module map (target)

```
Module/Scene/Gameplay/
  GameplayLauncher.cs            (DONE: 17 deps + 13 connectors wired)
  GameplayView.cs                (DONE: 15 view fields)
  Input/                         (DONE: skills, block, heavy shot)
  PlayerCharacter/               (DONE: hp/stamina/mana/gold, dash, block, charge)
  PlayerWeapon/                  (DONE — model only, no view/controller by design)
  PlayerSkill/                   (DONE)
  Statue/                        (DONE)
  Territory/                     (DONE: + TerritoryLayout geometry)
  Tower/                         (DONE)
  Build/                         (NEW vs plan — replaces StatueMenu: placement, costs, BuildRules)
  GameStateUi/                   (NEW vs plan — pause, game-over state, skill upgrade panel)
  Monster/                       (DONE: Model/Controller/View/Pool/Hitbox)
  Wall/                          (CANCELLED — BuildController + TerritoryView.AddWall)
  Guard/                         (NEXT)
  SpikeTrap/                     (NEXT, after Tree)
  Tree/                          (NEXT)
  Boss/                          (post-core)
  EnemyProjectilePool/           (CANCELLED until boss orbs need it)
  SlowField/                     (DONE)
  GameFlow/                      (DONE for clock/defeat; boss trigger + victory pending)
  Hud/                           (DONE: hp/stam/mana/statue sliders, gold/arrow text; skill grid pending)
  StatueMenu/                    (CANCELLED — Build module)
  CameraRig/                     (post-core)
  DevConsole/                    (post-core)
Module/Message/*.cs              (DONE: CombatMessages, MonsterMessages, StatueMessages, TowerMessages, PauseToggled, RequestToggleSkillUpgradePanel, SkillUpgradePanelToggled)
Assets/Resources/                (DONE: GameConstants.asset blocks, MonsterView/BulletView prefabs)
Assets/@Game/Tests/EditMode/     (DONE: 125 tests)
```

### Message map (publish → subscribers)

| Message | Published by | Consumed by |
|---|---|---|
| `MovePlayerCharacterMessage` (exists) | Input | PlayerCharacter |
| `RotatePlayerCharacterMessage` (exists) | Input | PlayerCharacter |
| `SwitchWeaponMessage` (exists) | Input | PlayerWeapon, Sword, BulletPool |
| `PlayerCharacterShootStart/EndMessage` (exists) | Input | PlayerWeapon, Sword, BulletPool |
| `BlockStartMessage` / `BlockEndMessage` | Input | PlayerWeapon, PlayerCharacter |
| `CastSkillMessage(int skillIndex)` | Input (1/2/3) | PlayerSkill |
| `SkillActivatedMessage(int skillIndex, float duration)` | PlayerSkill | Hud, PlayerCharacter, SlowField |
| `PlayerStatsChangedMessage(hp,stamina,mana,gold,arrows)` | PlayerCharacter | Hud, StatueMenu |
| `InteractMessage` | Input (E) | BuildSystem / StatueMenu router |
| `BuildRequestMessage(BuildKind, position, cost)` | StatueMenu / interaction | Territory |
| `TerritoryStateChangedMessage(index, unlocked, towers, walls)` | Territory | Hud, StatueMenu, Wall, Tower, Tree |
| `WallSegmentBuiltMessage(index, segment)` | Territory | Wall |
| `TowerBuiltMessage(index, corner)` | Territory | Tower |
| `StatueUpgradedMessage(level)` | Statue | PlayerSkill, Territory, Guard, Hud, Monster, BulletPool |
| `StatueDamagedMessage(current, max)` | Statue | Hud, GameFlow |
| `SpawnMonsterMessage(Vector2 position, MonsterTier tier)` | Monster spawner | MonsterPool |
| `DespawnMonsterMessage(GameObject)` | Monster | MonsterPool |
| `MonsterKilledMessage(int goldValue)` | Monster | PlayerCharacter, Statue (kill counter) |
| `SpawnEnemyProjectileMessage(origin, dir, damage, kind)` | Tower, Boss | EnemyProjectilePool |
| `DespawnEnemyProjectileMessage(GameObject)` | EnemyProjectile | EnemyProjectilePool |
| `SlowFieldChangedMessage(center, radius, expiryTime)` | SlowField | MonsterPool, Boss |
| `BossSpawnedMessage` / `BossPhaseChangedMessage(phase)` / `BossDefeatedMessage` | Boss | Hud, GameFlow |
| `GameClockMessage(elapsedSeconds)` | GameFlow (1 s tick) | Hud, Monster spawner, Boss trigger |
| `GameStateMessage` (exists) | GameFlow, launcher | everything |
| `CheatToggledMessage(CheatKind, bool)` | DevConsole | every model that reads a cheat flag |
| `RestartGameMessage` | GameOverUI | GameFlow |

Rule: a message is a **readonly struct with get-only properties**, name is `<Subject><Verb>Message`, and it carries no component references except pooled `GameObject` handles (existing `DespawnBulletMessage` precedent).

## 4. Layering rules (the part that keeps this testable)

1. **All rules in Models.** `MonsterSpawnModel.NextInterval(minutes)`, `StatueModel.CanUpgradeTo(nextLevel, territory, guardCount, killCount)`, `WallModel.Segments(territory)`, `WallModel.PointToSegmentDistance(...)`, `TerritoryModel.IsInside(index, pos)`, `WallModel.IsInGapZone(...)`. Plain C#, no `MonoBehaviour`, no `UnityEngine.Object` refs beyond `Vector2`/`Mathf`.
2. **Views are dumb.** Move a `Rigidbody2D`, play an animation, flip a sprite. A view may detect a collision and report it via a `UnityAction` callback — it may not decide damage (follow `BulletView.OnCollisionEnter2D` → callback → controller publish).
3. **Physics is Unity's job, not the port's.** The HTML resolves walls/towers/trees by hand; here they are `BoxCollider2D`/`CircleCollider2D` with the existing manual `resolveSingleWallSegment` semantics **only** where a body must be pushed out of a static wall (use colliders, drop the iterative push loops). Delete `pointToSegmentDist`-based collision from runtime paths; keep it for build-placement validation only.
4. **One controller instance per type.** Never `new` a controller outside `GetSceneDependencies()` and never `new` a launcher/connector field expecting it to survive — injection overwrites it.
5. **Per-object controllers** (`BulletController`, `MonsterController`, …) are constructed in the pool/manager `Create…Object()`, `InjectDependencies` called, then `Init(view, params)`. No `SetView` from the launcher for those.
6. **Tunables live only in ScriptableObjects.** A number that appears both in an SO and as a prefab field is a bug (existing `SwordView`/`BulletPoolModel` duplicate already caused the weapon-state tangle — don't repeat it).

## 5. Constants assets to create

Extend `SOGameConstants.cs` (one file, new `[Serializable]` classes — avoids 8 new assets and 8 resource loads):

```csharp
[Serializable] public class StatueConstants   { maxLevel, baseHp, hpPerLevel6, hpPerLevel7, healRadius, interactionRadius, upgradeCostPerLevel }
[Serializable] public class TerritoryConstants{ width, height per index; unlockLevel; towerCost; wallCost; towerOffset }
[Serializable] public class GuardConstants    { cost, maxCount per level; detectionRange; attackRange; damage; patrolSpeed; chaseSpeed }
[Serializable] public class MonsterConstants  { baseInterval, intervalPerMinute, minInterval, speed, hp, hpElite, damage, goldValue }
[Serializable] public class BossConstants     { spawnTimeSeconds, hp, speed, damage, phaseThresholds, summonCount per phase, projectileDamage }
[Serializable] public class SkillConstants    { per skill: manaCost, maxCooldown, duration, magnitude }
[Serializable] public class TreeConstants     { countPerTerritory, hp, growthIntervalSeconds }
[Serializable] public class TrapConstants     { cost, damage, durability, cooldown, radius, slowFactor, slowDuration }
```

Then `Assets/Resources/GameConstants.asset`: add the corresponding top-level `_statueConstants:` … blocks with the exact numbers from section 2 (the HTML constants are the spec). `TagManager.SO_GAMECONSTANTS` unchanged — one asset, no new loader path.

---

## 6. Phased tasks

Every task ends with a **commit**. Test tasks use EditMode (no play mode needed → no Unity-in-the-loop for the logic half).

### Phase 0 — Groundwork (before any port)

**Task 0.1: Create the EditMode test assembly.**
Files: `Assets/@Game/Tests/EditMode/ProjectTA.Tests.EditMode.asmdef` (references `Assembly-CSharp`… — see 0.2), `Assets/@Game/Tests/EditMode/SmokeTest.cs`.
Verify: Unity Test Runner → EditMode → 1 passed.
Note: the project has **no** asmdef under `Assets/@Game/Scripts`, so gameplay code is in `Assembly-CSharp`. An EditMode test asmdef cannot reference `Assembly-CSharp`. **Decision: create `Assets/@Game/Scripts/ProjectTA.asmdef` first** (named `ProjectTA`, no `noEngineReferences`), let Unity regenerate `.meta`, then reference it from the test asmdef. Do this in one Editor session and commit the generated guids — hand-writing asmdefs without the Editor leaves references dangling.

**Task 0.2: Pin the reference numbers.** Add section 2's table as XML doc-comments on each constants field, so a reviewer can diff code against `game.html` without re-reading it.

**Task 0.3: Confirm open question Q1 (art)** — see section 9. Everything from Phase 1 on uses placeholder primitives (squares/circles) so art is a pure swap later.

### Phase 1 — Player vertical slice (move → aim → sword → bow → skills)

**Task 1.1: `PlayerCharacterModel` as the stat authority.**
Files: create `Module/Scene/Gameplay/PlayerCharacter/PlayerCharacterModel.cs` (extend existing: add hp/maxHp, stamina, mana, gold, arrows, `ApplyDamage()`, `Tick(dt, isBlocking, isCharging, isAttacking)`, `AddGold()`); test `Tests/EditMode/PlayerCharacterModelTests.cs`.
TDD: assert mana regen 0.05/frame only when idle, stamina drain 0.4/frame while blocking, clamp at max, `ApplyDamage` no-ops when `IsGodMode`.
Verify: `pytest` equivalent → Unity Test Runner EditMode, 4 passed. (Run: Test Runner window; expected: 4 passed, 0 failed.)

**Task 1.2: `PlayerStatsChangedMessage` + HUD stub.**
Files: create `Module/Message/PlayerStatsChangedMessage.cs`, `Module/Scene/Gameplay/Hud/HudView.cs` + `HudController.cs` + `HudConnector.cs`; edit `GameplayView.cs`, `GameplayLauncher.cs`.
Verify: play mode, HP/MP/SP text updates while walking. (Play-mode check is manual — no CLI.)

**Task 1.3: Dash skill (sword 1).**
Files: edit `PlayerCharacterView.cs` (`StartDash(Vector2 dir)` sets velocity, timer), create `Module/Scene/Gameplay/PlayerSkill/PlayerSkillModel.cs` (`CanCast(index, weaponIndex, mana, cooldownLeft, isUnlocked)`), `PlayerSkillController.cs` (`CastSkillMessage` → validate → publish `SkillActivatedMessage`), `PlayerSkillConnector.cs`, `Module/Message/CastSkillMessage.cs`, `SkillActivatedMessage.cs`.
TDD: `CanCast` false on locked, false on cooldown, false on insufficient mana, false when already active for the same weapon.
Verify: EditMode 4 passed; play mode dash works.

**Task 1.4: Sword 2 Time Slash → `SlowField` module (do it now, trap slow reuses it).**
Files: create `Module/Scene/Gameplay/SlowField/SlowFieldModel.cs` (list of active fields, `IsInside(pos, time)`, `SpeedMultiplierAt(pos)`), `SlowFieldController.cs`, `SlowFieldConnector.cs`, `Module/Message/SlowFieldChangedMessage.cs`.
TDD: two overlapping fields → multiplier = product; expired field → 1.0.
Verify: EditMode 3 passed.

**Task 1.5: Sword 3 Berserker + bow equivalents.**
Files: edit `PlayerSkillModel`/`PlayerSkillController` (per-weapon branches identical in shape to 1.3/1.4), `PlayerWeapon/PlayerWeaponModel.cs` (`IsWeaponLocked`, `AttackCooldownMultiplier`, `DamageMultiplier`, `SpeedMultiplier`).
TDD: berserker ×3 speed/damage applies only for `weaponIndex == sword`; clone mode locks weapon and halves cooldown only for bow.
Verify: EditMode 5 passed.

**Task 1.6: Bow multi-shot + lock-shoot homing.**
Files: edit `BulletPool`/`BulletView` (add `ShootAt(angle, damage, kind)`, homing flag), new `Module/Message/ShootArrowsMessage.cs`.
TDD: `BulletPoolModel.ConsumeArrows(5)` with 3 arrows → reject; with `InfiniteArrows` cheat → no-op.
Verify: EditMode 3 passed; play mode 5-arrow fan visible.

**Task 1.7: Block + charge shot (RMB).**
Files: edit `InputActionManager.inputactions` (add `Block` bound to `<Mouse>/rightButton`), regenerate `InputActionManager.cs` **in the Editor**, edit `InputController.cs` → `BlockStartMessage`/`BlockEndMessage`.
Verify: play mode, sword blocks (stamina drains), bow charges and releases a heavy arrow.

**Task 1.8: Stamina/mana/gold HUD + skill grid (2×3).**
Files: edit `HudView.cs` (radial gauges, `drawSkillBox` equivalent as 6 `Image`+`TMP` prefab entries driven by a `SkillBoxView` struct list), `HudController.cs`.
Verify: play mode; locked skills show `Lv.3/5/7`, cooldowns count down.

### Phase 2 — Statue, territories, building

**Task 2.1: `StatueModel` + upgrade gating.**
Files: create `Module/Scene/Gameplay/Statue/{StatueModel,StatueController,StatueView,StatueConnector}.cs`, `Module/Message/StatueUpgradedMessage.cs`, `StatueDamagedMessage.cs`.
TDD (the gating table is the whole point — 7 cases): lv2 free; lv3 needs territory 0 all walls; lv4 needs 3 lv3 guards; lv5 needs territory 1 all walls; lv6 needs 6 lv5 guards; lv7 needs 100 kills; cost = `500 × level`.
Verify: EditMode 7 passed.

**Task 2.2: `TerritoryModel` geometry (pure math, no GameObjects).**
Files: create `Module/Scene/Gameplay/Territory/TerritoryModel.cs`, `Tests/EditMode/TerritoryModelTests.cs`.
TDD: `Corner(i)` for 800×600 territory → (350,250) etc.; `WallSegments(t)` returns 8 segments with a 120-wide gap centred on each cardinal axis; `Unlocked(t)` follows level 2/4.
Verify: EditMode 6 passed.

**Task 2.3: `TerritoryController` + `TerritoryStateChangedMessage` broadcast on upgrade.**
Verify: EditMode for model; play mode logs 3 territory states after `StatueUpgradedMessage(2)`.

**Task 2.4: Wall build.** `WallView` spawns one `BoxCollider2D` per built segment from `TerritoryStateChangedMessage`; `SlowField`-style model math reused for gap zones.
Verify: play mode — build 8 walls with E, player collides, gaps walkable.

**Task 2.5: Tower build + targeting + fire.**
Files: `Tower/{TowerModel,TowerController,TowerView,TowerConnector}.cs`, `Module/Message/{TowerBuiltMessage,SpawnEnemyProjectileMessage}.cs`.
TDD: `TowerModel.HasTarget(range, monsters, boss)` picks nearest inside range; `CooldownTick` respects rate 50.
Verify: EditMode 4 passed; play mode tower kills a monster.

**Task 2.6: `EnemyProjectilePool`** (one pool, two kinds: tower arrow / boss orb).
Files: mirror `BulletPool` structure exactly (prefab, `activeInHierarchy` availability test, `SetParent(pool, false)` on despawn, `CancelInvoke` in `OnDisable`).
Verify: play mode — both kinds fly, hit intended target only.

**Task 2.7: `Tree` module** — 20 per territory, growth every 60 s, hp 100, blocked movement, cleared by `WallSegmentBuiltMessage`/`TowerBuiltMessage`.
TDD: `TreeModel.ClearAlongSegment(seg, radius)` returns indices within `radius`; growth counter advances only when unlocked.
Verify: EditMode 3 passed; play mode trees appear/block/vanish.

**Task 2.8: `SpikeTrap`** + trap slow via `SlowFieldAddedMessage`, durability, cooldown 45.
TDD: durability decrements once per trigger, cooldown blocks re-trigger, at 0 durability trap deactivates.
Verify: EditMode 8 passed (`TrapModelTests`); suite 105/0/39.

### Phase 3 — Enemies

**Task 3.1: `MonsterModel` spawn curve + tier.** TDD: `NextInterval(0 min) == 3.0 s`-equivalent (`180/60`), `NextInterval(10) == 40/60` floor, monotone decreasing.
**Task 3.2: `MonsterPool`** (pooled, mirror `BulletPool`; reset hit flags/colliders in `OnEnable`).
**Task 3.3: `MonsterView` pathing** — walk toward the nearest wall gap of the outermost built wall, else toward the statue; attack when within `(size+statue)/2+5`.
TDD: `MonsterModel.NextWaypoint(pos, gaps, statuePos, hasOuterWall)` → gap when distance ≥ 100, statue otherwise.
**Task 3.4: Damage/kill/gold flow** — `MonsterKilledMessage` → `PlayerCharacterModel.AddGold` + `StatueModel.MonsterKilled()`.
**Task 3.5: `Guard` module** — recruit from foundations, lv3 inner / lv5 outer ring, `IsEligibleTarget(monsterPos, guardLevel)`.
TDD: lv5 guard ignores monsters inside territory 0; lv3 guard ignores monsters outside territory 0.
Verify: EditMode 11 passed (`GuardModelTests`); suite 116/0/39.

### Phase 4 — Flow, boss, UI

**Task 4.1: `GameFlowModel` clock + boss trigger + win/lose.** TDD: clock advances only in `Playing`; boss fires once at 600 s; `Defeat` when statue hp ≤ 0 or player hp ≤ 0; `Victory` on `BossDefeatedMessage`.
**Task 4.2: `BossModel` phases.** TDD: hp 60%/30% boundaries map to phase 1/2/3; enrage only at 3; speed/damage multipliers per phase; summon counts 2/0/3.
Verify: EditMode 10 passed (`BossModelTests`); suite 131/0/39.
**Task 4.3: `BossView`** — 3-phase visuals, projectile fan, summons, `statue.hp -= bossDamage` gated by `CheatToggledMessage(godMode)`.
Verify: EditMode 12 passed (`BossModelTests`); suite 133/0/39.
**Task 4.4: Timer HUD** — blink at 9:30 and ≥10:00, boss bar + phase label.
Verify: suite 133/0/39 (compile-only for the view; clock/blink need a play-mode glance).
**Task 4.5: `StatueMenu`** — E near statue opens, `Time.timeScale = 0.15`, ESC closes, auto-close after 2 s of movement (count frames in `StatueMenuModel.IsMovementExpired(frames)`, testable), builds tower/wall/trap/guard via `BuildRequestMessage`, upgrade button with gating tooltip from `StatueModel.UpgradeBlockReason(nextLevel)`.
TDD: `UpgradeBlockReason` returns the exact strings for each unmet condition.
**Task 4.6: Game over / victory screens** — stats (time, kills, skill3 uses, traps triggered, trees chopped, gold, statue level), `RestartGameMessage` → every model resets (a `ResetMessage` per module is the honest way; a single "reload scene" is the lazy correct way — **use `SceneLoader.Instance.LoadScene(TagManager.SCENE_GAMEPLAY)`**, matching `OnMainMenu`).
Verify: EditMode 3 passed (`GameStateUiModelTests`); suite 136/0/39.

### Phase 5 — Dev console & polish

**Task 5.1: `DevConsoleModel` with the 8 cheat flags + 8 actions**, gated by `SOGameConstants.IsDebugBuild`; `CheatToggledMessage` consumed by `PlayerCharacterModel`, `StatueModel`, `MonsterPool` (no-spawn), `BulletPool` (infinite arrows), `TerritoryModel` (free build), `PlayerSkillModel` (no cooldown), `GameFlowModel` (fast-forward ×2).
Verify: EditMode 4 tests (`DevConsoleModelTests`); suite 140/0/39.
**Task 5.2: Skill 3 unlock at statue lv7 → `PlayerSkillModel` re-reads `StatueUpgradedMessage`.**
Verify: already implemented before this pass; `PlayerSkillModelTests.Skills_UnlockAtStatueLevelsThreeFiveSeven` covers 3/5/7.
**Task 5.3: Camera zoom on statue menu + smooth follow** (`CameraRigView`, `LateUpdate`, `Vector2.Lerp` 0.1, `zoom` 1.0 ↔ 0.85).
**Task 5.4: Balance pass against the reference** — run one 10-minute session, compare: boss arrival time, monster count at 5/10 min, statue hp curve. Numbers are the acceptance test for the port.

---

## 7. Files likely to change (hot spots)

- `Assets/@Game/Scripts/Module/Scene/Gameplay/GameplayLauncher.cs` — **every** new controller lands in *both* `GetSceneDependencies()` and `GetSceneConnectors()` (a controller missing from dependencies is never registered; its connector then stays viewless). 14 new controllers → this file becomes the wiring checklist.
- `Assets/@Game/Scripts/Module/Scene/Gameplay/GameplayView.cs` — one `[field: SerializeField] public XView XView` per new view.
- `Assets/@Game/Scenes/Gameplay.unity` — new GameObjects + `--- !u!114` blocks per view. **Build these in the Editor**, do not hand-write fileIDs/guids unless the Editor is unavailable; hand-written guids that collide with an existing `.meta` dangle silently.
- `Assets/@Game/Scripts/Module/Scene/Gameplay/Input/InputActionManager.inputactions` + generated `.cs` — regenerate in Editor.
- `Assets/Resources/GameConstants.asset` — new serialized blocks.
- `Assets/@Game/Scripts/Module/Scene/Gameplay/BulletPool/BulletPoolModel.cs` — strip weapon state, keep ammo/fire-rate.

## 8. Tests / validation

| Layer | How | Command |
|---|---|---|
| Models (rules) | Unity Test Runner EditMode | Editor → Window → General → Test Runner → EditMode → Run All |
| Physics/interaction | Manual play mode | Editor → Play |
| Full-loop parity | 10-min play session, compare against `game.html` in a browser | manual, Task 5.4 |

Target coverage: every `Model` has ≥3 EditMode assertions; the gating tables (`StatueModel.CanUpgradeTo`, `PlayerSkillModel.CanCast`, `GuardModel.IsEligibleTarget`) get one assertion per branch.

**Honest limit:** there is no Unity CLI on this host — nothing written from the shell is *compiled or verified* until someone opens the Editor. Treat every "Verify: play mode" line as a manual step for the user, and every EditMode line as a real gate once the assembly exists.

## 9. Core-scope marker (updated 2026-11-17)

**Built:** Phases 0, 1, 2 (incl. Trap), Tasks 3.1–3.5, plus unplanned `Build/` and `GameStateUi/`.
**Core still open:** none — core scope is complete; everything below is post-core.
**Post-core:** Phase 4 (boss, boss trigger, victory, results screen), Phase 5 (dev console, camera rig, balance), HUD skill grid + defeat/victory counters.
Boss-spawn trigger and win condition stay in `GameFlowModel` from Task 4.1 even in core scope? — **No.** Core runs endless-survival; `GameFlowModel` in core only owns clock + defeat. The boss trigger lands with Task 4.1 in the post-core pass.

## 10. Risks, tradeoffs, remaining questions

**R1 — Scene/prefab authoring is Editor-only.** ~20 views need GameObjects. The code half can be written from the shell; the scene half cannot be safely hand-edited at this volume. Mitigation: Phase 0.1 asmdef + Phase 1 HUD proves the pipeline on 1 view before scaling.
**R2 — Physics: RESOLVED (D2).** Colliders. Consequence to watch: `Rigidbody2D` walls make the player slide along segments, whereas the HTML snapped position exactly. If sliding feels wrong in Task 2.4, add a low `PhysicsMaterial2D` friction (0.4) rather than reintroducing the push-out loop.
**R3 — Global `Time.timeScale` slow-mo** slows *everything* including UI tweening and the Input System. The reference slows only entities inside fields. Split: `Time.timeScale` for the statue menu; `SlowFieldModel.SpeedMultiplierAt()` for Time Slash / traps. Do not mix them.
**R4 — 14 new modules is a lot of surface** — reason D1 cut it to ~9 for the core pass. Monsters are the only addition beyond Phases 0–2.
**R5 — Duplicate weapon state.** `BulletPoolModel.IsWeaponActive` and `SwordView._isEquipped` already both track the weapon. Adding `PlayerWeaponModel` makes three. Consolidate in Task 1.5 or the bug returns.

**Q — still open:** none blocking. Q1 art (D3), Q2 physics (D2), Q3 admin (D6), Q4 naming (D4), Q5 persistence (D5) are all answered.

