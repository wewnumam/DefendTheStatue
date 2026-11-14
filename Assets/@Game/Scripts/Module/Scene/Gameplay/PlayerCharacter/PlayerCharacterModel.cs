using Agate.MVC.Base;
using ProjectTA.Module.GameConstants;
using UnityEngine;

namespace ProjectTA.Module.PlayerCharacter
{
    /// <summary>
    /// Owns every player-authored number: stats, gold, ammo. Rules are a direct port of the
    /// reference game's per-frame player update, converted from 60 fps frames to seconds.
    /// </summary>
    public class PlayerCharacterModel : BaseModel
    {
        public PlayerConstants PlayerConstants { get; private set; } = null;
        public bool IsJoystickActive { get; private set; }

        public float Health { get; private set; }
        public float Stamina { get; private set; }
        public float Mana { get; private set; }
        public int Gold { get; private set; }
        public int Arrows { get; private set; }
        public int StatueLevel { get; private set; } = 1;

        public bool IsGodMode { get; private set; }
        public bool IsInfiniteStamina { get; private set; }
        public bool IsInfiniteArrows { get; private set; }

        public float MaxHealth => PlayerConstants != null ? PlayerConstants.MaxHealth : 0f;
        public float MaxStamina => PlayerConstants != null ? PlayerConstants.MaxStamina : 0f;
        public float MaxMana => PlayerConstants != null ? PlayerConstants.MaxMana : 0f;

        /// <summary>Reference game: maxArrows = Math.min(6, statue.level).</summary>
        public int MaxArrows =>
            PlayerConstants == null ? 0 : Mathf.Min(PlayerConstants.MaxArrowCapacity, StatueLevel);

        public bool IsAlive => Health > 0f;

        /// <summary>Reference game drops blocking the moment stamina bottoms out.</summary>
        public bool IsStaminaDepleted => Stamina <= 0f;

        private float _arrowRespawnTimer;

        #region SETUP

        public void SetPlayerConstants(PlayerConstants playerConstants)
        {
            PlayerConstants = playerConstants;
            SetDataAsDirty();
        }

        public void SetJoystickActive(bool isActive)
        {
            IsJoystickActive = isActive;
            SetDataAsDirty();
        }

        /// <summary>Port of the reference game's reset block: back to a fresh run.</summary>
        public void ResetStats()
        {
            if (PlayerConstants == null)
            {
                return;
            }

            StatueLevel = 1;
            Health = MaxHealth;
            Stamina = MaxStamina;
            Mana = MaxMana;
            Gold = PlayerConstants.StartingGold;
            Arrows = MaxArrows;
            _arrowRespawnTimer = 0f;

            IsGodMode = false;
            IsInfiniteStamina = false;
            IsInfiniteArrows = false;

            SetDataAsDirty();
        }

        #endregion

        #region TICK

        /// <summary>
        /// Port of the reference game's player block: heal aura, block drain, idle regen, arrow
        /// respawn. Blocking, charging and attacking are mutually exclusive regen locks.
        /// </summary>
        public void Tick(
            float deltaTime,
            bool isBlocking,
            bool isCharging,
            bool isAttacking,
            bool isInHealRadius)
        {
            if (PlayerConstants == null)
            {
                return;
            }

            if (isInHealRadius && Health < MaxHealth)
            {
                Health = Mathf.Min(MaxHealth, Health + PlayerConstants.HealPerSecond * deltaTime);
            }

            if (isBlocking && !IsInfiniteStamina)
            {
                Stamina = Mathf.Max(0f, Stamina - PlayerConstants.StaminaBlockDrainPerSecond * deltaTime);
            }

            if (!isBlocking && !isCharging && !isAttacking)
            {
                Stamina = Mathf.Min(MaxStamina, Stamina + PlayerConstants.StaminaRegenPerSecond * deltaTime);
                Mana = Mathf.Min(MaxMana, Mana + PlayerConstants.ManaRegenPerSecond * deltaTime);
            }

            if (IsInfiniteStamina)
            {
                Stamina = MaxStamina;
                Mana = MaxMana;
            }

            Stamina = Mathf.Min(Stamina, MaxStamina);
            Mana = Mathf.Min(Mana, MaxMana);

            TickArrows(deltaTime);

            SetDataAsDirty();
        }

        /// <summary>At capacity the respawn timer is parked at zero, matching the reference game.</summary>
        private void TickArrows(float deltaTime)
        {
            if (Arrows >= MaxArrows)
            {
                _arrowRespawnTimer = 0f;
                return;
            }

            _arrowRespawnTimer += deltaTime;
            if (_arrowRespawnTimer < PlayerConstants.ArrowRespawnSeconds)
            {
                return;
            }

            _arrowRespawnTimer = 0f;
            if (Arrows < MaxArrows)
            {
                Arrows++;
            }
        }

        #endregion

        #region RESOURCES

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (Gold < amount)
            {
                return false;
            }

            Gold -= amount;
            SetDataAsDirty();
            return true;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Gold += amount;
            SetDataAsDirty();
        }

        public bool TrySpendStamina(float amount)
        {
            if (IsInfiniteStamina || amount <= 0f)
            {
                return true;
            }

            if (Stamina < amount)
            {
                return false;
            }

            Stamina -= amount;
            SetDataAsDirty();
            return true;
        }

        public bool TrySpendMana(float amount)
        {
            if (IsInfiniteStamina || amount <= 0f)
            {
                return true;
            }

            if (Mana < amount)
            {
                return false;
            }

            Mana -= amount;
            SetDataAsDirty();
            return true;
        }

        /// <summary>Reference game: skill 3 clone mode fires without spending arrows.</summary>
        public bool TryConsumeArrows(int count, bool isArrowFree)
        {
            if (IsInfiniteArrows || isArrowFree || count <= 0)
            {
                return true;
            }

            if (Arrows < count)
            {
                return false;
            }

            Arrows -= count;
            SetDataAsDirty();
            return true;
        }

        public void RefillArrows()
        {
            Arrows = MaxArrows;
            _arrowRespawnTimer = 0f;
            SetDataAsDirty();
        }

        /// <summary>Upgrading the statue raises both the arrow cap and the current arrows.</summary>
        public void SetStatueLevel(int level)
        {
            StatueLevel = Mathf.Clamp(level, 1, 7);

            if (Arrows < MaxArrows)
            {
                Arrows = MaxArrows;
            }

            SetDataAsDirty();
        }

        #endregion

        #region DAMAGE

        /// <summary>Returns false when the hit was swallowed by god mode.</summary>
        public bool ApplyDamage(float amount)
        {
            if (amount <= 0f || IsGodMode)
            {
                return false;
            }

            Health = Mathf.Max(0f, Health - amount);
            SetDataAsDirty();
            return true;
        }

        public void Heal(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            Health = Mathf.Min(MaxHealth, Health + amount);
            SetDataAsDirty();
        }

        public void SetGodMode(bool isActive)
        {
            IsGodMode = isActive;
            SetDataAsDirty();
        }

        public void SetInfiniteStamina(bool isActive)
        {
            IsInfiniteStamina = isActive;
            SetDataAsDirty();
        }

        public void SetInfiniteArrows(bool isActive)
        {
            IsInfiniteArrows = isActive;
            SetDataAsDirty();
        }

        #endregion
    }
}
