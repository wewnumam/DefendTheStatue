using NUnit.Framework;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.PlayerCharacter;
using UnityEngine;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// Player stat rules, asserted against the shipped GameConstants asset so the tunables stay in
    /// sync with the GDD (§4): 50 HP, 20 stamina, 0 starting essence, 4 s arrow cooldown.
    /// </summary>
    public class PlayerCharacterModelTests
    {
        private SOGameConstants _constants;
        private PlayerCharacterModel _model;

        [SetUp]
        public void SetUp()
        {
            _constants = Resources.Load<SOGameConstants>("GameConstants");
            Assert.That(_constants, Is.Not.Null, "GameConstants asset missing from Resources.");

            _model = new PlayerCharacterModel();
            _model.SetPlayerConstants(_constants.PlayerConstants);
        }

        [Test]
        public void ResetStats_MatchesReferenceStartingValues()
        {
            _model.ResetStats();

            Assert.That(_model.Health, Is.EqualTo(50f), "hp");
            Assert.That(_model.Stamina, Is.EqualTo(20f), "stamina");
            Assert.That(_model.Mana, Is.EqualTo(0f), "mana starts empty");
            Assert.That(_model.MaxMana, Is.EqualTo(0f), "mana cap is earned per kill");
            Assert.That(_model.Essence, Is.EqualTo(0), "essence");
            Assert.That(_model.StatueLevel, Is.EqualTo(1), "statue.level");
            Assert.That(_model.Arrows, Is.EqualTo(1), "arrows at level 1");
            Assert.That(_model.IsAlive, Is.True);
        }

        [Test]
        public void Blocking_DrainsStaminaAtReferenceRate()
        {
            _model.ResetStats();

            _model.Tick(1f, true, false, false, false);

            // GDD §4: block costs 0.5 stamina per second.
            Assert.That(_model.Stamina, Is.EqualTo(19.5f).Within(0.001f));
        }

        [Test]
        public void Blocking_DoesNotRegenerateStaminaOrMana()
        {
            _model.ResetStats();
            GrantKills(10);
            _model.Tick(1f, true, false, false, false);
            float staminaAfterDrain = _model.Stamina;
            float manaAfterDrain = _model.Mana;

            _model.Tick(1f, true, false, false, false);

            Assert.That(_model.Stamina, Is.LessThan(staminaAfterDrain));
            Assert.That(_model.Mana, Is.EqualTo(manaAfterDrain).Within(0.001f));
        }

        [Test]
        public void Idle_RegeneratesStaminaAndManaAtReferenceRates()
        {
            _model.ResetStats();
            _model.Tick(1f, true, false, false, false);

            _model.Tick(1f, false, false, false, false);

            // 1 s of blocking costs 0.5 of the 20 stamina; 1 s of idle regains more than that, so it clamps.
            Assert.That(_model.Stamina, Is.EqualTo(_model.MaxStamina).Within(0.001f));
            Assert.That(_model.Mana, Is.EqualTo(0f).Within(0.001f), "GDD: mana has no passive regen");
        }

        [Test]
        public void Charging_BlocksIdleRegeneration()
        {
            _model.ResetStats();
            GrantKills(10);
            _model.Tick(1f, true, false, false, false);
            _model.TrySpendMana(4f);
            float staminaBefore = _model.Stamina;
            float manaBefore = _model.Mana;

            _model.Tick(1f, false, true, false, false);

            Assert.That(_model.Stamina, Is.EqualTo(staminaBefore).Within(0.001f));
            Assert.That(_model.Mana, Is.EqualTo(manaBefore).Within(0.001f));
        }

        [Test]
        public void Attacking_BlocksIdleRegeneration()
        {
            _model.ResetStats();
            GrantKills(10);
            _model.TrySpendMana(4f);
            float manaBefore = _model.Mana;

            _model.Tick(1f, false, false, true, false);

            Assert.That(_model.Mana, Is.EqualTo(manaBefore).Within(0.001f));
        }

        [Test]
        public void PassiveRegeneration_ClampsAtMaximum()
        {
            _model.ResetStats();

            _model.Tick(60f, false, false, false, false);

            Assert.That(_model.Stamina, Is.EqualTo(_model.MaxStamina));
            Assert.That(_model.Mana, Is.EqualTo(0f), "idle time never fills mana");
        }

        [Test]
        public void HealRadius_RestoresHealthAtReferenceRate()
        {
            _model.ResetStats();
            _model.ApplyDamage(20f);

            _model.Tick(1f, false, false, false, true);

            // 0.3 hp per frame at 60 fps = 18 per second.
            Assert.That(_model.Health, Is.EqualTo(48f).Within(0.001f));
        }

        [Test]
        public void HealRadius_ClampsAtMaximumHealth()
        {
            _model.ResetStats();
            _model.ApplyDamage(5f);

            _model.Tick(60f, false, false, false, true);

            Assert.That(_model.Health, Is.EqualTo(_model.MaxHealth));
        }

        [Test]
        public void ArrowRespawn_RefillsOneArrowPerCooldown()
        {
            _model.ResetStats();
            _model.SetStatueLevel(3);
            _model.RefillArrows();
            _model.TryConsumeArrows(1, false);

            _model.Tick(3.9f, false, false, false, false);
            Assert.That(_model.Arrows, Is.EqualTo(2), "GDD §4 cooldown is 4s");

            _model.Tick(0.1f, false, false, false, false);
            Assert.That(_model.Arrows, Is.EqualTo(3));
        }

        [Test]
        public void ArrowRespawn_TimerParkedWhenAtCapacity()
        {
            _model.ResetStats();
            _model.Tick(600f, false, false, false, false);

            Assert.That(_model.Arrows, Is.EqualTo(_model.MaxArrows));
        }

        [Test]
        public void MaxArrows_FollowsStatueLevelUpToSix()
        {
            _model.ResetStats();

            _model.SetStatueLevel(2);
            Assert.That(_model.MaxArrows, Is.EqualTo(2));

            _model.SetStatueLevel(6);
            Assert.That(_model.MaxArrows, Is.EqualTo(6));

            _model.SetStatueLevel(7);
            Assert.That(_model.MaxArrows, Is.EqualTo(6), "min(6, statue.level)");
        }

        [Test]
        public void SetStatueLevel_RaisesCurrentArrowsToNewCap()
        {
            _model.ResetStats();
            Assert.That(_model.Arrows, Is.EqualTo(1));

            _model.SetStatueLevel(4);

            Assert.That(_model.Arrows, Is.EqualTo(4));
        }

        [Test]
        public void SetStatueLevel_ClampsToOneThroughTen()
        {
            _model.ResetStats();

            _model.SetStatueLevel(99);
            Assert.That(_model.StatueLevel, Is.EqualTo(10));

            _model.SetStatueLevel(0);
            Assert.That(_model.StatueLevel, Is.EqualTo(1));
        }

        [Test]
        public void StaminaDepleted_FlagSetWhenBlockDrainBottomsOut()
        {
            _model.ResetStats();
            Assert.That(_model.IsStaminaDepleted, Is.False);

            // 20 stamina at 0.5/s = 40 s of blocking.
            _model.Tick(40f, true, false, false, false);

            Assert.That(_model.Stamina, Is.EqualTo(0f));
            Assert.That(_model.IsStaminaDepleted, Is.True);
        }

        [Test]
        public void ApplyDamage_ReducesHealthAndClampsAtZero()
        {
            _model.ResetStats();

            Assert.That(_model.ApplyDamage(30f), Is.True);
            Assert.That(_model.Health, Is.EqualTo(20f));

            _model.ApplyDamage(999f);
            Assert.That(_model.Health, Is.EqualTo(0f));
            Assert.That(_model.IsAlive, Is.False);
        }

        [Test]
        public void ApplyDamage_IgnoredWhileGodMode()
        {
            _model.ResetStats();
            _model.SetGodMode(true);

            Assert.That(_model.ApplyDamage(30f), Is.False);
            Assert.That(_model.Health, Is.EqualTo(_model.MaxHealth));
        }

        [Test]
        public void TrySpendEssence_FailsWithoutFunds()
        {
            _model.ResetStats();
            _model.AddEssence(20);

            Assert.That(_model.TrySpendEssence(20), Is.True);
            Assert.That(_model.Essence, Is.EqualTo(0));

            Assert.That(_model.TrySpendEssence(1), Is.False);
            Assert.That(_model.Essence, Is.EqualTo(0));
        }

        [Test]
        public void TrySpendMana_FailsWithoutMana()
        {
            _model.ResetStats();
            GrantKills(5);

            Assert.That(_model.TrySpendMana(6f), Is.False, "cap is 5");
            Assert.That(_model.TrySpendMana(5f), Is.True);
            Assert.That(_model.Mana, Is.EqualTo(0f));
        }

        [Test]
        public void TryConsumeArrows_HonoursArrowFreeCloneModeAndInfiniteArrows()
        {
            _model.ResetStats();
            _model.SetStatueLevel(2);
            Assert.That(_model.Arrows, Is.EqualTo(2));

            Assert.That(_model.TryConsumeArrows(1, false), Is.True);
            Assert.That(_model.Arrows, Is.EqualTo(1));

            Assert.That(_model.TryConsumeArrows(1, true), Is.True, "clone mode is arrow free");
            Assert.That(_model.Arrows, Is.EqualTo(1));

            _model.SetInfiniteArrows(true);
            Assert.That(_model.TryConsumeArrows(5, false), Is.True);
            Assert.That(_model.Arrows, Is.EqualTo(1));
        }

        [Test]
        public void InfiniteStamina_PinsStaminaAndManaFull()
        {
            _model.ResetStats();
            GrantKills(3);
            _model.SetInfiniteStamina(true);

            _model.Tick(10f, true, false, false, false);

            Assert.That(_model.Stamina, Is.EqualTo(_model.MaxStamina));
            Assert.That(_model.Mana, Is.EqualTo(_model.MaxMana));
        }

        [Test]
        public void Tick_WithoutConstantsIsANoOp()
        {
            PlayerCharacterModel fresh = new PlayerCharacterModel();

            fresh.Tick(1f, false, false, false, false);

            Assert.That(fresh.Health, Is.EqualTo(0f));
            Assert.That(fresh.MaxArrows, Is.EqualTo(0));
        }

        private void GrantKills(int kills)
        {
            for (int i = 0; i < kills; i++)
            {
                _model.RegisterKill();
            }
        }

        [Test]
        public void RegisterKill_GrowsManaCapByOneAndFillsThatPoint()
        {
            _model.ResetStats();

            GrantKills(3);

            Assert.That(_model.MaxMana, Is.EqualTo(3f));
            Assert.That(_model.Mana, Is.EqualTo(3f));
        }

        [Test]
        public void ManaSpent_IsNotRefilledByIdling()
        {
            _model.ResetStats();
            GrantKills(5);
            _model.TrySpendMana(5f);

            _model.Tick(60f, false, false, false, false);

            Assert.That(_model.Mana, Is.EqualTo(0f), "no regen, GDD §4");
            Assert.That(_model.MaxMana, Is.EqualTo(5f), "cap survives the spend");
        }

        [Test]
        public void Respawn_RestoresHealthAndGrantsThreeSecondsOfInvulnerability()
        {
            _model.ResetStats();
            _model.ApplyDamage(999f);
            Assert.That(_model.IsAlive, Is.False);

            _model.Respawn();

            Assert.That(_model.Health, Is.EqualTo(_model.MaxHealth));
            Assert.That(_model.IsInvulnerable, Is.True);
            Assert.That(_model.ApplyDamage(10f), Is.False, "hit swallowed by the respawn window");

            _model.Tick(3.01f, false, false, false, false);
            Assert.That(_model.IsInvulnerable, Is.False);
            Assert.That(_model.ApplyDamage(10f), Is.True);
        }
    }
}
