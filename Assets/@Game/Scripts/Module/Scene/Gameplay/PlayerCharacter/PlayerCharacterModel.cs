using Agate.MVC.Base;
using ProjectTA.Module.GameConstants;
using UnityEngine;

namespace ProjectTA.Module.PlayerCharacter
{
    /// <summary>
    /// Owns every player-authored number: stats, essence, mana, ammo. Mana follows the GDD: it starts
    /// at 0, the cap grows by 1 per kill and there is no passive regeneration.
    /// </summary>
    public class PlayerCharacterModel : BaseModel
    {
        /// <summary>GDD §4: respawn grants 3 seconds of invincibility.</summary>
        public const float RespawnInvulnerableSeconds = 3f;

        /// <summary>GDD §4: dying costs the statue 30 HP.</summary>
        public const float DeathStatuePenalty = 30f;

        public PlayerConstants PlayerConstants { get; private set; } = null;
        public bool IsJoystickActive { get; private set; }

        public float Health { get; private set; }
        public float Stamina { get; private set; }
        public float Mana { get; private set; }
        public int Essence { get; private set; }
        public int Arrows { get; private set; }
        public int StatueLevel { get; private set; } = 1;

        public bool IsGodMode { get; private set; }
        public bool IsInfiniteStamina { get; private set; }
        public bool IsInfiniteArrows { get; private set; }
        public bool IsInfiniteEssence { get; private set; }

        public float MaxHealth => PlayerConstants != null ? PlayerConstants.MaxHealth : 0f;
        public float MaxStamina => PlayerConstants != null ? PlayerConstants.MaxStamina : 0f;

        /// <summary>GDD §4: the mana cap is earned, one point per kill.</summary>
        public float MaxMana => _manaCapacity;

        public bool IsInvulnerable => _invulnerableRemaining > 0f;

        /// <summary>Reference game: maxArrows = Math.min(6, statue.level).</summary>
        public int MaxArrows =>
            PlayerConstants == null ? 0 : Mathf.Min(PlayerConstants.MaxArrowCapacity, StatueLevel);

        public bool IsAlive => Health > 0f;

        /// <summary>Reference game drops blocking the moment stamina bottoms out.</summary>
        public bool IsStaminaDepleted => Stamina <= 0f;

        private float _arrowRespawnTimer;
        private float _manaCapacity;
        private float _invulnerableRemaining;

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
            _manaCapacity = PlayerConstants.StartingManaCapacity;
            Mana = 0f;
            _invulnerableRemaining = 0f;
            Essence = PlayerConstants.StartingEssence;
            Arrows = MaxArrows;
            _arrowRespawnTimer = 0f;

            IsGodMode = false;
            IsInfiniteStamina = false;
            IsInfiniteArrows = false;
            IsInfiniteEssence = false;

            SetDataAsDirty();
        }

        #endregion

        #region TICK

        /// <summary>
        /// Heal aura, block drain, stamina regen, respawn invulnerability and the arrow clock. Mana has
        /// no passive regen (GDD §4), so nothing here touches it.
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
            }

            if (_invulnerableRemaining > 0f)
            {
                _invulnerableRemaining = Mathf.Max(0f, _invulnerableRemaining - deltaTime);
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

        public bool TrySpendEssence(int amount)
        {
            if (amount <= 0 || IsInfiniteEssence)
            {
                return true;
            }

            if (Essence < amount)
            {
                return false;
            }

            Essence -= amount;
            SetDataAsDirty();
            return true;
        }

        public void AddEssence(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Essence += amount;
            SetDataAsDirty();
        }

        /// <summary>GDD §4: each kill raises the mana cap by 1 and tops the pool up with that point.</summary>
        public void RegisterKill()
        {
            _manaCapacity += 1f;
            Mana = Mathf.Min(_manaCapacity, Mana + 1f);
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
            StatueLevel = Mathf.Clamp(level, 1, 10);

            if (Arrows < MaxArrows)
            {
                Arrows = MaxArrows;
            }

            SetDataAsDirty();
        }

        #endregion

        #region DAMAGE

        /// <summary>Returns false when the hit was swallowed by god mode or the respawn window.</summary>
        public bool ApplyDamage(float amount)
        {
            if (amount <= 0f || IsGodMode || IsInvulnerable)
            {
                return false;
            }

            Health = Mathf.Max(0f, Health - amount);
            SetDataAsDirty();
            return true;
        }

        /// <summary>
        /// GDD §4: back on your feet at full health with 3 s of invincibility. Mana is untouched — the
        /// cap is earned per kill, not per life.
        /// </summary>
        public void Respawn()
        {
            Health = MaxHealth;
            Stamina = MaxStamina;
            _invulnerableRemaining = RespawnInvulnerableSeconds;
            SetDataAsDirty();
        }

        /// <summary>Dev-console mana grant; clamped to the current cap.</summary>
        public void AddMana(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            Mana = Mathf.Min(MaxMana, Mana + amount);
            SetDataAsDirty();
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

        /// <summary>Dev console: building and upgrading never dip into the purse.</summary>
        public void SetInfiniteEssence(bool isActive)
        {
            IsInfiniteEssence = isActive;
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
