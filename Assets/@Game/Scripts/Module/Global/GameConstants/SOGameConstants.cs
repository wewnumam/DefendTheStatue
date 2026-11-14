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

        public bool IsJoystickActive => _isJoystickActive;
        public PlayerConstants PlayerConstants => _playerConstants;
        public PlayerSkillConstants SkillConstants => _skillConstants;
        public ShootingConstants Shooting => _shootingConstants;
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
