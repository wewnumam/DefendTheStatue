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

        [Header("Shooting Constants")]
        [SerializeField] private ShootingConstants _shootingConstants;

        public bool IsJoystickActive => _isJoystickActive;
        public PlayerConstants PlayerConstants => _playerConstants;
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

        public LayerMask EnemyLayer => _enemyLayer;
        public float MovementSpeed => _movementSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float RayDistance => _rayDistance;
        public float FixedYPosition => _fixedYPosition;
        public float KnockbackForce => _knockbackForce;
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
