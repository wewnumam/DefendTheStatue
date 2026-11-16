using UnityEngine;
using System;

namespace ProjectTA.Module.GameConstants
{
    [CreateAssetMenu(fileName = "GameConstants", menuName = "ProjectTA/GameConstants", order = 0)]
    public class SOGameConstants : ScriptableObject
    {
        [Header("General Settings")]
        [SerializeField] private bool _isJoystickActive;

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

        public bool IsJoystickActive => _isJoystickActive;
        public PlayerConstants PlayerConstants => _playerConstants;
        public PlayerSkillConstants SkillConstants => _skillConstants;
        public ShootingConstants Shooting => _shootingConstants;
        public MonsterConstants MonsterConstants => _monsterConstants;
        public TerritoryConstants TerritoryConstants => _territoryConstants;
        public TowerConstants TowerConstants => _towerConstants;
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

        public int RingCount => _ringSizes != null ? _ringSizes.Length : 0;
        public float GapHalfWidth => _gapHalfWidth;
        public float WallThickness => _wallThickness;

        public Vector2 GetRingSize(int index)
        {
            if (_ringSizes == null || _ringSizes.Length == 0)
            {
                return Vector2.zero;
            }

            return index >= 0 && index < _ringSizes.Length ? _ringSizes[index] : _ringSizes[0];
        }
    }

    /// <summary>One built tower: footprint, reach, cadence and damage. Reference: 32 / 300 / 50f / 15.</summary>
    [Serializable]
    public class TowerConstants
    {
        [SerializeField] private float _size = 32f;
        [SerializeField] private float _attackRange = 300f;

        /// <summary>Reference cadence, in 60 fps frames.</summary>
        [SerializeField] private int _attackRateFrames = 50;

        [SerializeField] private float _damage = 15f;

        public float Size => _size;
        public float AttackRange => _attackRange;
        public int AttackRateFrames => _attackRateFrames;
        public float Damage => _damage;
    }

    /// <summary>
    /// Monster body, stats and bite cadence. Defaults are the reference game's spawnMonster values, so a
    /// fresh asset behaves the same before anyone tunes it; world units are the statue's space, where the
    /// statue itself is 50 across.
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
        [SerializeField] private float _baseHealth = 20f;
        [SerializeField] private float _healRadiusHealth = 40f;
        [SerializeField] private float _baseDamage = 10f;
        [SerializeField] private float _healRadiusDamage = 30f;
        [SerializeField] private int _baseGoldValue = 5;
        [SerializeField] private int _healRadiusGoldValue = 15;

        [Header("Attack")]
        /// <summary>Reference: attackCooldown 60 frames.</summary>
        [SerializeField] private float _attackIntervalSeconds = 1f;
        [SerializeField] private float _stopPadding = 2f;
        [SerializeField] private float _attackReachPadding = 5f;

        public float Size => _size;
        public float StatueSize => _statueSize;
        public float SpeedPerSecond => _speedPerSecond;

        public float BaseHealth => _baseHealth;
        public float HealRadiusHealth => _healRadiusHealth;
        public float BaseDamage => _baseDamage;
        public float HealRadiusDamage => _healRadiusDamage;
        public int BaseGoldValue => _baseGoldValue;
        public int HealRadiusGoldValue => _healRadiusGoldValue;

        public float AttackIntervalSeconds => _attackIntervalSeconds;

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
        [SerializeField] private float _maxMana;
        [SerializeField] private int _startingGold;
        [SerializeField] private float _staminaBlockDrainPerSecond;
        [SerializeField] private float _staminaRegenPerSecond;
        [SerializeField] private float _manaRegenPerSecond;
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
        public float MaxMana => _maxMana;
        public int StartingGold => _startingGold;
        public float StaminaBlockDrainPerSecond => _staminaBlockDrainPerSecond;
        public float StaminaRegenPerSecond => _staminaRegenPerSecond;
        public float ManaRegenPerSecond => _manaRegenPerSecond;
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

        public int Skill1RequiredStatueLevel => _skill1RequiredStatueLevel;
        public int Skill2RequiredStatueLevel => _skill2RequiredStatueLevel;
        public int Skill3RequiredStatueLevel => _skill3RequiredStatueLevel;

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
