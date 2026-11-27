using UnityEngine;
using System;

namespace ProjectTA.Module.GameConstants
{
    [CreateAssetMenu(fileName = "GameConstants", menuName = "ProjectTA/GameConstants", order = 0)]
    public class SOGameConstants : ScriptableObject
    {
        [Header("General Settings")]
        [SerializeField] private bool _isJoystickActive;
        [SerializeField] private bool _isDebugBuild;

        [Header("Player Constants")]
        [SerializeField] private PlayerConstants _playerConstants;

        [Header("Skill Constants")]
        [SerializeField] private PlayerSkillConstants _skillConstants;

        [Header("Shooting Constants")]
        [SerializeField] private ShootingConstants _shootingConstants;

        [Header("Monster Constants")]
        [SerializeField] private MonsterConstants _monsterConstants;

        [Header("Territory Constants")]
        [SerializeField] private TerritoryConstants _territoryConstants;

        [Header("Tower Constants")]
        [SerializeField] private TowerConstants _towerConstants;

        [Header("Tree Constants")]
        [SerializeField] private TreeConstants _treeConstants;

        [Header("Trap Constants")]
        [SerializeField] private TrapConstants _trapConstants;

        [Header("Guard Constants")]
        [SerializeField] private GuardConstants _guardConstants;

        [Header("Boss Constants")]
        [SerializeField] private BossConstants _bossConstants;

        public bool IsJoystickActive => _isJoystickActive;
        public bool IsDebugBuild => _isDebugBuild;
        public PlayerConstants PlayerConstants => _playerConstants;
        public PlayerSkillConstants SkillConstants => _skillConstants;
        public ShootingConstants Shooting => _shootingConstants;
        public MonsterConstants MonsterConstants => _monsterConstants;
        public TerritoryConstants TerritoryConstants => _territoryConstants;
        public TowerConstants TowerConstants => _towerConstants;
        public TreeConstants TreeConstants => _treeConstants;
        public TrapConstants TrapConstants => _trapConstants;
        public GuardConstants GuardConstants => _guardConstants;
        public BossConstants BossConstants => _bossConstants;
    }

    /// <summary>
    /// The Destroyer, straight from GDD §8: 20 HP, 10 melee damage, no projectile and no enrage. The
    /// only phase lever is the summon bundle, plus a doubled attack rate in phase 3.
    /// </summary>
    [Serializable]
    public class BossConstants
    {
        [SerializeField] private float _maxHealth = 20f;
        [SerializeField] private int _damage = 10;

        /// <summary>GDD §8: attack rate doubles below 30% health.</summary>
        [SerializeField] private float _attackIntervalSeconds = 1f;

        /// <summary>GDD §19: the boss walks at 0.5, half the Stacker's 1.0.</summary>
        [SerializeField] private float _speed = 2.5f;

        [SerializeField] private float _attackRange = 40f;
        [SerializeField] private float _spawnDistance = 60f;
        [SerializeField] private float _summonRadius = 20f;

        [Header("Escort at spawn (GDD §8: 4 Stacker, 2 Waltroy, 2 Deasor)")]
        [SerializeField] private Utility.EnumManager.MonsterKind[] _escort =
        {
            Utility.EnumManager.MonsterKind.Stacker,
            Utility.EnumManager.MonsterKind.Stacker,
            Utility.EnumManager.MonsterKind.Stacker,
            Utility.EnumManager.MonsterKind.Stacker,
            Utility.EnumManager.MonsterKind.Waltroy,
            Utility.EnumManager.MonsterKind.Waltroy,
            Utility.EnumManager.MonsterKind.Deasor,
            Utility.EnumManager.MonsterKind.Deasor
        };

        [Header("Phase summons")]
        [SerializeField] private Utility.EnumManager.MonsterKind[] _phase1Summons =
        {
            Utility.EnumManager.MonsterKind.Stacker,
            Utility.EnumManager.MonsterKind.Stacker
        };

        [SerializeField] private Utility.EnumManager.MonsterKind[] _phase2Summons =
        {
            Utility.EnumManager.MonsterKind.Stacker,
            Utility.EnumManager.MonsterKind.Waltroy
        };

        [SerializeField] private Utility.EnumManager.MonsterKind[] _phase3Summons =
        {
            Utility.EnumManager.MonsterKind.Waltroy,
            Utility.EnumManager.MonsterKind.Deasor
        };

        public float MaxHealth => _maxHealth;
        public int Damage => _damage;
        public float AttackIntervalSeconds => _attackIntervalSeconds;
        public float Speed => _speed;
        public float AttackRange => _attackRange;
        public float SpawnDistance => _spawnDistance;
        public float SummonRadius => _summonRadius;

        /// <summary>GDD §8: phase 3 attacks twice as often; phases 1 and 2 are steady.</summary>
        public float AttackIntervalForPhase(int phase)
        {
            return phase >= 3 ? _attackIntervalSeconds * 0.5f : _attackIntervalSeconds;
        }

        public System.Collections.Generic.IReadOnlyList<Utility.EnumManager.MonsterKind> Escort => _escort;

        public System.Collections.Generic.IReadOnlyList<Utility.EnumManager.MonsterKind> SummonsForPhase(int phase)
        {
            switch (phase)
            {
                case 1:
                    return _phase1Summons;
                case 2:
                    return _phase2Summons;
                case 3:
                    return _phase3Summons;
                default:
                    return System.Array.Empty<Utility.EnumManager.MonsterKind>();
            }
        }

        public int SummonCountForPhase(int phase)
        {
            System.Collections.Generic.IReadOnlyList<Utility.EnumManager.MonsterKind> summons = SummonsForPhase(phase);
            return summons?.Count ?? 0;
        }
    }

    /// <summary>
    /// Recruited guards: reach, damage and cadence. Level 3 guards cover the inner territory, level 5
    /// guards cover the outer ring. Numbers are rebased to this project's statue scale.
    /// </summary>
    [Serializable]
    public class GuardConstants
    {
        /// <summary>GDD §6: a guard hits for 1 unless bow skill 4 is empowering it.</summary>
        [SerializeField] private int _damage = 1;
        [SerializeField] private float _attackRange = 250f;
        [SerializeField] private float _attackIntervalSeconds = 1f;

        /// <summary>GDD §6 HP by role: Inner 100, Outer 150, Perimeter 200.</summary>
        [SerializeField] private float[] _healthByLevel = { 100f, 150f, 200f };

        /// <summary>GDD §19: guards walk at 2.0, twice the Stacker's 1.0 — a multiplier over monster speed.</summary>
        [SerializeField] private float _speedMultiplier = 2f;

        /// <summary>GDD §19/§6: retreating guards move at 1.5x.</summary>
        [SerializeField] private float _retreatSpeedMultiplier = 1.5f;

        /// <summary>GDD §6 state machine thresholds: below 30% retreats, above 70% returns.</summary>
        [SerializeField] private float _retreatHealthFraction = 0.3f;
        [SerializeField] private float _returnHealthFraction = 0.7f;

        /// <summary>Fraction of max HP healed per second while standing at the statue.</summary>
        [SerializeField] private float _healFractionPerSecond = 0.1f;

        /// <summary>GDD §6 patrol: how far a guard wanders from its foundation when nothing is in sight.</summary>
        [SerializeField] private float _patrolRadius = 60f;

        /// <summary>How far a guard will leave its post to engage a monster.</summary>
        [SerializeField] private float _chaseRadius = 350f;

        /// <summary>Hard leash: a guard never chases further than this from its foundation.</summary>
        [SerializeField] private float _leashRadius = 350f;

        public int Damage => _damage;
        public float AttackRange => _attackRange;
        public float AttackIntervalSeconds => _attackIntervalSeconds;
        public float SpeedMultiplier => _speedMultiplier;
        public float RetreatSpeedMultiplier => _retreatSpeedMultiplier;
        public float RetreatHealthFraction => _retreatHealthFraction;
        public float ReturnHealthFraction => _returnHealthFraction;
        public float HealFractionPerSecond => _healFractionPerSecond;
        public float PatrolRadius => _patrolRadius;
        public float ChaseRadius => _chaseRadius;
        public float LeashRadius => _leashRadius;

        /// <summary>Max HP for a foundation level: 3 -&gt; 100, 5 -&gt; 150, 10 -&gt; 200.</summary>
        public float HealthForLevel(int foundationLevel)
        {
            int index = foundationLevel >= 10 ? 2 : foundationLevel >= 5 ? 1 : 0;
            if (_healthByLevel == null || _healthByLevel.Length <= index)
            {
                return 100f;
            }

            return _healthByLevel[index];
        }
    }

    /// <summary>
    /// Spike traps: one trigger per cooldown, a durability pool, and a short slow on whatever it
    /// spikes. Reference game's SPIKE_TRAP_CONFIG, rebased to this project's statue scale.
    /// </summary>
    [Serializable]
    public class TrapConstants
    {
        /// <summary>GDD §12: 5 essence, 1 damage, 10 triggers, 45 frames cooldown, 120 frames slow.</summary>
        [SerializeField] private int _cost = 5;
        [SerializeField] private int _damage = 1;
        [SerializeField] private int _maxDurability = 10;

        /// <summary>Reference radius 35 against an 800-wide ring; the ring here is 100 wide.</summary>
        [SerializeField] private float _radius = 4.5f;

        /// <summary>Reference cooldown 45 frames.</summary>
        [SerializeField] private float _cooldownSeconds = 0.75f;

        /// <summary>Reference slowFactor 0.4 and slowmoDuration 120 frames.</summary>
        [SerializeField] private float _slowFactor = 0.4f;
        [SerializeField] private float _slowDurationSeconds = 2f;

        public int Cost => _cost;
        public int Damage => _damage;
        public int MaxDurability => _maxDurability;
        public float Radius => _radius;
        public float CooldownSeconds => _cooldownSeconds;
        public float SlowFactor => _slowFactor;
        public float SlowDurationSeconds => _slowDurationSeconds;
    }

    /// <summary>One tree kind, straight from the GDD §10 table.</summary>
    [Serializable]
    public class TreeKindStats
    {
        public TreeKindStats()
        {
        }

        /// <summary>Test/asset-free construction: the GDD rows are the defaults below, so keep them in sync.</summary>
        public TreeKindStats(float health, float hpDrop, float manaDrop, float respawnSeconds, int countPerTerritory)
        {
            _health = health;
            _hpDrop = hpDrop;
            _manaDrop = manaDrop;
            _respawnSeconds = respawnSeconds;
            _countPerTerritory = countPerTerritory;
        }

        [SerializeField] private float _health = 5f;

        /// <summary>HP the eater/hewer gets back or takes: Red 1, Magic 2, Arcana 3, Purple none.</summary>
        [SerializeField] private float _hpDrop = 1f;

        /// <summary>Mana the player gains by chopping it: Purple 3, Magic 2, Arcana 3, Red none.</summary>
        [SerializeField] private float _manaDrop;

        [SerializeField] private float _respawnSeconds = 30f;
        [SerializeField] private int _countPerTerritory = 15;

        public float Health => _health;
        public float HpDrop => _hpDrop;
        public float ManaDrop => _manaDrop;
        public float RespawnSeconds => _respawnSeconds;
        public int CountPerTerritory => _countPerTerritory;
    }

    /// <summary>
    /// Trees: the four kind tables, plus the spacing rules for scattering them. Reference game values
    /// (40 size, 50 clear radius, 80 edge inset) are rebased to this project's statue scale, where the
    /// statue is 3 across instead of 50.
    /// </summary>
    [Serializable]
    public class TreeConstants
    {
        /// <summary>GDD §10 order: Red, Purple, Magic, Arcana.</summary>
        [SerializeField] private TreeKindStats[] _kinds =
        {
            new TreeKindStats(5f, 1f, 0f, 30f, 15),   // Red
            new TreeKindStats(5f, 0f, 3f, 30f, 15),   // Purple
            new TreeKindStats(9f, 2f, 2f, 60f, 4),    // Magic
            new TreeKindStats(12f, 3f, 3f, 120f, 2)   // Arcana
        };

        public TreeKindStats GetKind(Utility.EnumManager.TreeKind kind)
        {
            if (_kinds == null || _kinds.Length == 0)
            {
                return new TreeKindStats();
            }

            int index = (int)kind;
            return index >= 0 && index < _kinds.Length ? _kinds[index] : _kinds[0];
        }

        /// <summary>Reference treeGrowthTimer: one new tree every 3600 frames.</summary>
        [SerializeField] private float _growthIntervalSeconds = 60f;

        [Header("Body (world units)")]
        [SerializeField] private float _size = 2.5f;

        /// <summary>How close a built tower or wall segment must be to flatten a tree.</summary>
        [SerializeField] private float _clearRadius = 3f;

        /// <summary>Trees never spawn closer to the statue than this.</summary>
        [SerializeField] private float _statueClearance = 8f;

        /// <summary>Minimum gap between two trees, and between a tree and a tower.</summary>
        [SerializeField] private float _minSpacing = 5f;

        /// <summary>Trees stay this far inside the ring's edge.</summary>
        [SerializeField] private float _placementInset = 8f;


        /// <summary>Reference findNearestHealTree maxDist.</summary>
        [SerializeField] private float _healSearchRange = 500f;

        /// <summary>Reference d &gt; 30: how close a monster must be to bite the tree.</summary>
        [SerializeField] private float _monsterHealRadius = 40f;

        /// <summary>Reference damageTree(maxHp * 0.5): the tree loses half its health per heal.</summary>
        [SerializeField] private float _monsterHealTreeDamageFraction = 0.5f;

        public float GrowthIntervalSeconds => _growthIntervalSeconds;
        public float Size => _size;
        public float ClearRadius => _clearRadius;
        public float StatueClearance => _statueClearance;
        public float MinSpacing => _minSpacing;
        public float PlacementInset => _placementInset;
        public float HealSearchRange => _healSearchRange;
        public float MonsterHealRadius => _monsterHealRadius;
        public float MonsterHealTreeDamageFraction => _monsterHealTreeDamageFraction;
    }

    /// <summary>
    /// Ring layout: sizes, the gap in the middle of every side, and how thick a built wall is. World
    /// units, with the statue (50 across) at the origin.
    /// </summary>
    [Serializable]
    public class TerritoryConstants
    {
        [SerializeField]
        private Vector2[] _ringSizes =
        {
            new Vector2(800f, 600f),
            new Vector2(1600f, 1200f),
            new Vector2(3200f, 2400f)
        };

        /// <summary>Half of the reference game's 120-wide opening at the middle of each side.</summary>
        [SerializeField] private float _gapHalfWidth = 60f;

        /// <summary>How thick the wall bar and its collider are.</summary>
        [SerializeField] private float _wallThickness = 8f;

        /// <summary>GDD §9: one wall segment has 100 HP.</summary>
        [SerializeField] private float _wallHealth = 100f;

        public int RingCount => _ringSizes != null ? _ringSizes.Length : 0;
        public float GapHalfWidth => _gapHalfWidth;
        public float WallThickness => _wallThickness;
        public float WallHealth => _wallHealth;

        public Vector2 GetRingSize(int index)
        {
            if (_ringSizes == null || _ringSizes.Length == 0)
            {
                return Vector2.zero;
            }

            return index >= 0 && index < _ringSizes.Length ? _ringSizes[index] : _ringSizes[0];
        }
    }

    /// <summary>One built tower: footprint, health, reach, cadence and damage. GDD: 40 hp, 1 damage, 50 frames.</summary>
    [Serializable]
    public class TowerConstants
    {
        [SerializeField] private float _size = 32f;

        /// <summary>GDD §9: a tower has 40 HP and can be destroyed.</summary>
        [SerializeField] private float _health = 40f;

        [SerializeField] private float _attackRange = 300f;

        /// <summary>GDD §9 cadence, in 60 fps frames.</summary>
        [SerializeField] private int _attackRateFrames = 50;

        [SerializeField] private float _damage = 1f;

        public float Size => _size;
        public float Health => _health;
        public float AttackRange => _attackRange;
        public int AttackRateFrames => _attackRateFrames;
        public float Damage => _damage;
    }

    /// <summary>
    /// One monster role's numbers, straight from the GDD §7 table. Damage is split by target because a
    /// Waltroy chews walls while a Deasor hunts the player and the guards.
    /// </summary>
    [Serializable]
    public class MonsterKindStats
    {
        public MonsterKindStats()
        {
        }

        /// <summary>Test/asset-free construction; the GDD §7 rows are the defaults below.</summary>
        public MonsterKindStats(
            float health,
            float speedMultiplier,
            int essence,
            float statueDamage,
            float structureDamage,
            float playerDamage,
            float guardDamage)
        {
            _health = health;
            _speedMultiplier = speedMultiplier;
            _essence = essence;
            _statueDamage = statueDamage;
            _structureDamage = structureDamage;
            _playerDamage = playerDamage;
            _guardDamage = guardDamage;
        }

        [SerializeField] private float _health = 3f;

        /// <summary>Multiplier over MonsterConstants.SpeedPerSecond: Stacker 1, Waltroy 0.5, Deasor 2.</summary>
        [SerializeField] private float _speedMultiplier = 1f;

        [SerializeField] private int _essence = 1;

        [Header("Damage by target")]
        [SerializeField] private float _statueDamage = 1f;
        [SerializeField] private float _structureDamage = 1f;
        [SerializeField] private float _playerDamage = 1f;
        [SerializeField] private float _guardDamage = 1f;

        public float Health => _health;
        public float SpeedMultiplier => _speedMultiplier;
        public int Essence => _essence;
        public float StatueDamage => _statueDamage;
        public float StructureDamage => _structureDamage;
        public float PlayerDamage => _playerDamage;
        public float GuardDamage => _guardDamage;
    }

    /// <summary>
    /// Monster body, shared cadence and the per-role stat block. World units are the statue's space,
    /// where the statue itself is 50 across.
    /// </summary>
    [Serializable]
    public class MonsterConstants
    {
        [Header("Body (world units)")]
        [SerializeField] private float _size = 25f;
        [SerializeField] private float _statueSize = 50f;

        /// <summary>Reference: 1.5 units per frame at 60 fps.</summary>
        [SerializeField] private float _speedPerSecond = 90f;

        [Header("Stats")]
        [Header("Attack")]
        /// <summary>Reference: attackCooldown 60 frames.</summary>
        [SerializeField] private float _attackIntervalSeconds = 1f;
        [SerializeField] private float _stopPadding = 2f;
        [SerializeField] private float _attackReachPadding = 5f;

        [Header("Spawn")]
        /// <summary>Gap between the outermost built wall ring and where a monster appears.</summary>
        [SerializeField] private float _spawnOffset = 100f;

        /// <summary>Distance from the camera centre when no wall exists yet.</summary>
        [SerializeField] private float _cameraSpawnDistance = 500f;

        [Header("Healing")]
        /// <summary>Reference hpRatio &lt; 0.3: a hurt monster looks for a tree.</summary>
        [SerializeField] private float _healHealthFraction = 0.3f;

        [Header("Roles (GDD §7: Stacker, Waltroy, Deasor)")]
        [SerializeField]
        private MonsterKindStats[] _kinds =
        {
            // Stacker: swarm, 2 into the statue.
            new MonsterKindStats(3f, 1f, 1, 2f, 1f, 1f, 1f),
            // Waltroy: siege, 4 into walls and towers.
            new MonsterKindStats(10f, 0.5f, 2, 1f, 4f, 1f, 1f),
            // Deasor: assassin, 4 into the player and guards.
            new MonsterKindStats(2f, 2f, 2, 1f, 1f, 4f, 4f)
        };

        /// <summary>GDD §7: a hurt monster runs at 1.5x speed.</summary>
        [SerializeField] private float _retreatSpeedMultiplier = 1.5f;

        public MonsterKindStats GetKind(Utility.EnumManager.MonsterKind kind)
        {
            if (_kinds == null || _kinds.Length == 0)
            {
                return new MonsterKindStats();
            }

            int index = (int)kind;
            return index >= 0 && index < _kinds.Length ? _kinds[index] : _kinds[0];
        }

        public float RetreatSpeedMultiplier => _retreatSpeedMultiplier;

        /// <summary>Reference healCooldown 180 frames: how long a monster waits before healing again.</summary>
        [SerializeField] private float _healCooldownSeconds = 1.5f;

        public float Size => _size;
        public float StatueSize => _statueSize;
        public float SpeedPerSecond => _speedPerSecond;

        public float AttackIntervalSeconds => _attackIntervalSeconds;
        public float SpawnOffset => _spawnOffset;
        public float CameraSpawnDistance => _cameraSpawnDistance;
        public float HealHealthFraction => _healHealthFraction;
        public float HealCooldownSeconds => _healCooldownSeconds;

        /// <summary>(monster + statue) / 2 + padding: how close a monster walks before it bites.</summary>
        public float StopDistance => (_size + _statueSize) * 0.5f + _stopPadding;

        /// <summary>A bite lands a little before the monster is at rest, matching the reference's +5.</summary>
        public float AttackReach => StopDistance + _attackReachPadding;
    }

    [Serializable]
    public class PlayerConstants
    {
        [SerializeField] private LayerMask _enemyLayer;
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private float _rayDistance;
        [SerializeField] private float _fixedYPosition;
        [SerializeField] private float _knockbackForce;

        [Header("Stats (reference game parity)")]
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _maxStamina;
        /// <summary>GDD §4: mana starts at 0 and the cap grows +1 per kill; this is that starting cap.</summary>
        [SerializeField] private float _maxMana;
        [SerializeField] private int _startingEssence;
        [SerializeField] private float _staminaBlockDrainPerSecond;
        [SerializeField] private float _staminaRegenPerSecond;
        [SerializeField] private float _healPerSecond;
        [SerializeField] private float _arrowRespawnSeconds;
        [SerializeField] private int _maxArrowCapacity;
        [SerializeField] private float _dashSpeed;
        [SerializeField] private float _dashDurationSeconds;

        public LayerMask EnemyLayer => _enemyLayer;
        public float MovementSpeed => _movementSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float RayDistance => _rayDistance;
        public float FixedYPosition => _fixedYPosition;
        public float KnockbackForce => _knockbackForce;

        public float MaxHealth => _maxHealth;
        public float MaxStamina => _maxStamina;
        public float StartingManaCapacity => _maxMana;
        public int StartingEssence => _startingEssence;
        public float StaminaBlockDrainPerSecond => _staminaBlockDrainPerSecond;
        public float StaminaRegenPerSecond => _staminaRegenPerSecond;
        public float HealPerSecond => _healPerSecond;
        public float ArrowRespawnSeconds => _arrowRespawnSeconds;
        public int MaxArrowCapacity => _maxArrowCapacity;
        public float DashSpeed => _dashSpeed;
        public float DashDurationSeconds => _dashDurationSeconds;
    }

    /// <summary>
    /// Skill costs, cooldowns and effect durations. Every duration was frames at 60 fps in the
    /// reference game and is stored here in seconds.
    /// </summary>
    [Serializable]
    public class PlayerSkillConstants
    {
        [Header("Unlock gates (statue level)")]
        [SerializeField] private int _skill1RequiredStatueLevel;
        [SerializeField] private int _skill2RequiredStatueLevel;
        [SerializeField] private int _skill3RequiredStatueLevel;
        [SerializeField] private int _skill4RequiredStatueLevel;

        [Header("Skill 1")]
        [SerializeField] private float _skill1CooldownSeconds;
        [SerializeField] private float _skill1ManaCost;

        [Header("Skill 2")]
        [SerializeField] private float _skill2CooldownSeconds;
        [SerializeField] private float _skill2ManaCost;
        [SerializeField] private float _lockShootDurationSeconds;
        [SerializeField] private float _timeSlashRadius;
        [SerializeField] private float _timeSlashDurationSeconds;
        [SerializeField] private float _timeSlashSlowFactor;

        [Header("Skill 3")]
        [SerializeField] private float _skill3CooldownSeconds;
        [SerializeField] private float _skill3ManaCost;
        [SerializeField] private float _skill3DurationSeconds;
        [SerializeField] private float _berserkerSpeedMultiplier;
        [SerializeField] private float _berserkerDamageMultiplier;
        [SerializeField] private float _cloneModeAttackCooldownMultiplier;

        /// <summary>GDD §13 skill 4: sword mind control and bow guard empower, both 6 s / 50 mana.</summary>
        [Header("Skill 4")]
        [SerializeField] private float _skill4CooldownSeconds;
        [SerializeField] private float _skill4ManaCost;
        [SerializeField] private float _skill4DurationSeconds;
        [SerializeField] private float _mindControlRadius;
        [SerializeField] private float _mindControlDamageMultiplier;
        [SerializeField] private float _guardEmpowerRadius;
        [SerializeField] private float _guardEmpowerDamageMultiplier;
        [SerializeField] private float _guardEmpowerAttackSpeedMultiplier;

        public int Skill1RequiredStatueLevel => _skill1RequiredStatueLevel;
        public int Skill2RequiredStatueLevel => _skill2RequiredStatueLevel;
        public int Skill3RequiredStatueLevel => _skill3RequiredStatueLevel;
        public int Skill4RequiredStatueLevel => _skill4RequiredStatueLevel;

        public float Skill1CooldownSeconds => _skill1CooldownSeconds;
        public float Skill1ManaCost => _skill1ManaCost;

        public float Skill2CooldownSeconds => _skill2CooldownSeconds;
        public float Skill2ManaCost => _skill2ManaCost;
        public float LockShootDurationSeconds => _lockShootDurationSeconds;
        public float TimeSlashRadius => _timeSlashRadius;
        public float TimeSlashDurationSeconds => _timeSlashDurationSeconds;
        public float TimeSlashSlowFactor => _timeSlashSlowFactor;

        public float Skill3CooldownSeconds => _skill3CooldownSeconds;
        public float Skill3ManaCost => _skill3ManaCost;
        public float Skill3DurationSeconds => _skill3DurationSeconds;
        public float BerserkerSpeedMultiplier => _berserkerSpeedMultiplier;
        public float BerserkerDamageMultiplier => _berserkerDamageMultiplier;
        public float CloneModeAttackCooldownMultiplier => _cloneModeAttackCooldownMultiplier;

        public float Skill4CooldownSeconds => _skill4CooldownSeconds;
        public float Skill4ManaCost => _skill4ManaCost;
        public float Skill4DurationSeconds => _skill4DurationSeconds;
        public float MindControlRadius => _mindControlRadius;
        public float MindControlDamageMultiplier => _mindControlDamageMultiplier;
        public float GuardEmpowerRadius => _guardEmpowerRadius;
        public float GuardEmpowerDamageMultiplier => _guardEmpowerDamageMultiplier;
        public float GuardEmpowerAttackSpeedMultiplier => _guardEmpowerAttackSpeedMultiplier;

        /// <summary>GDD §13 effect names, one per weapon, in slot order. Shared by the HUD and the upgrade panel.</summary>
        public static readonly string[] SwordSkillNames = { "Dash", "Time Slash", "Berserker", "Mind Control" };
        public static readonly string[] BowSkillNames = { "Triple Shot", "Lock-On", "Clone Mode", "Guard Empower" };

        /// <summary>GDD §15: 4 sword + 4 bow boxes with their unlock level and mana cost.</summary>
        public string[] BuildSkillLines()
        {
            string[] lines = new string[8];

            for (int i = 0; i < 4; i++)
            {
                lines[i] = $"{i + 1} Sword {SwordSkillNames[i]} — Lv {RequiredLevel(i)} · {ManaCost(i)} MP";
                lines[i + 4] = $"{i + 1} Bow {BowSkillNames[i]} — Lv {RequiredLevel(i)} · {ManaCost(i)} MP";
            }

            return lines;
        }

        public int RequiredLevel(int slot)
        {
            switch (slot)
            {
                case 0:
                    return _skill1RequiredStatueLevel;
                case 1:
                    return _skill2RequiredStatueLevel;
                case 2:
                    return _skill3RequiredStatueLevel;
                default:
                    return _skill4RequiredStatueLevel;
            }
        }

        public int ManaCost(int slot)
        {
            switch (slot)
            {
                case 0:
                    return UnityEngine.Mathf.RoundToInt(_skill1ManaCost);
                case 1:
                    return UnityEngine.Mathf.RoundToInt(_skill2ManaCost);
                case 2:
                    return UnityEngine.Mathf.RoundToInt(_skill3ManaCost);
                default:
                    return UnityEngine.Mathf.RoundToInt(_skill4ManaCost);
            }
        }
    }

    [Serializable]
    public class ShootingConstants
    {
        [Range(0.0f, 1.0f)]
        [SerializeField] private float _joystickShootingRange;
        [SerializeField] private int _bulletPoolSize;
        [SerializeField] private float _bulletShootingRate;
        [SerializeField] private float _bulletForce;
        [SerializeField] private float _bulletDestroyDelay;

        public float JoystickShootingRange => _joystickShootingRange;
        public int BulletPoolSize => _bulletPoolSize;
        public float BulletShootingRate => _bulletShootingRate;
        public float BulletForce => _bulletForce;
        public float BulletDestroyDelay => _bulletDestroyDelay;
    }
}
