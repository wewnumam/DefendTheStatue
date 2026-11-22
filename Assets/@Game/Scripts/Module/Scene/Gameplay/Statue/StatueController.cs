using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.Build;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.Territory;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Statue
{
    /// <summary>
    /// Statue health, the upgrade ladder and the gates in front of each level. Essence and the wall
    /// counts arrive as messages, so the statue never reaches into the player or territory modules.
    /// </summary>
    public class StatueController : ObjectController<StatueController, StatueModel, StatueView>
    {
        private int[] _wallSegmentsBuilt = System.Array.Empty<int>();
        private int _essence;
        private int _monstersKilled;
        private int _guardsAtLevel3;
        private int _guardsAtLevel5;
        private bool _bossDefeated;

        private bool _isInvulnerable;

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null || gameConstants.GameConstants == null)
            {
                Debug.LogError("GAMECONSTANTS IS NULL");
                return;
            }

            _wallSegmentsBuilt = new int[gameConstants.GameConstants.TerritoryConstants.RingCount];
        }

        public override void SetView(StatueView view)
        {
            base.SetView(view);
            _model.Reset();
            ClearProgress();
            view.EnsureVisual();
        }

        public void OnRequestUpgrade(RequestUpgradeStatueMessage message)
        {
            if (_model.IsMaxLevel)
            {
                return;
            }

            int nextLevel = _model.Level + 1;
            if (!BuildRules.CanUpgradeToLevel(
                    nextLevel,
                    AllWallsBuilt(0),
                    _guardsAtLevel3,
                    AllWallsBuilt(1),
                    _guardsAtLevel5,
                    _monstersKilled,
                    _bossDefeated))
            {
                return;
            }

            int cost = _model.UpgradeCost;
            if (_essence < cost)
            {
                return;
            }

            StatueUpgradeEffect effect = _model.Upgrade();
            _view.SetLevel(_model.Level);
            Publish(new RequestSpendEssenceMessage(cost));
            Publish(new StatueUpgradedMessage(_model.Level, effect, cost));

            if (effect.UnlocksTerritory)
            {
                Publish(new TerritoryUnlockedMessage(effect.UnlockedTerritoryIndex));
            }
        }

        public void OnRequestDamage(RequestStatueDamageMessage message)
        {
            if (_isInvulnerable)
            {
                return;
            }

            if (!_model.ApplyDamage(message.Amount))
            {
                return;
            }

            Publish(new StatueDamagedMessage(_model.Health, _model.MaxHealth));

            if (!_model.IsAlive)
            {
                // ponytail: the run clock lives in GameFlow, so the statue only reports what it saw.
                // Fill the survival time in when the results screen lands.
                Publish(new StatueDestroyedMessage(_monstersKilled, 0f));
                Publish(new GameStateMessage(EnumManager.GameState.GameOver));
            }
        }

        public void OnWallSegmentBuilt(WallSegmentBuiltMessage message)
        {
            if (message.TerritoryIndex < 0 || message.TerritoryIndex >= _wallSegmentsBuilt.Length)
            {
                return;
            }

            _wallSegmentsBuilt[message.TerritoryIndex]++;
        }

        public void OnMonsterKilled(MonsterKilledMessage message)
        {
            _monstersKilled++;
        }

        /// <summary>
        /// The ladder counts recruited guards, not foundations: level 4 wants three guards from the
        /// level 3 foundations, level 6 wants six from the level 5 ones.
        /// </summary>
        public void OnGuardBuilt(GuardBuiltMessage message)
        {
            // Ladder gates count exact tiers: perimeter guards must not satisfy the six-outer gate.
            if (message.FoundationLevel == 5)
            {
                _guardsAtLevel5++;
            }
            else if (message.FoundationLevel == 3)
            {
                _guardsAtLevel3++;
            }
        }

        /// <summary>GDD §5: levels 8-10 accept (or, at 10, demand) the boss kill.</summary>
        public void OnBossDefeated(BossDefeatedMessage message)
        {
            _bossDefeated = true;
        }

        public void OnPlayerStatsChanged(PlayerStatsChangedMessage message)
        {
            _essence = message.Essence;
        }

        public void OnCheatToggled(CheatToggledMessage message)
        {
            if (message.Cheat == EnumManager.CheatType.GodMode)
            {
                _isInvulnerable = message.Enabled;
            }
        }

        public void OnDevHeal(DevHealStatueMessage message)
        {
            _model.Heal(_model.MaxHealth);
            Publish(new StatueDamagedMessage(_model.Health, _model.MaxHealth));
        }

        /// <summary>Dev action: walk the ladder without essence or gates and announce every step.</summary>
        public void OnDevMaxLevel(DevMaxLevelMessage message)
        {
            while (!_model.IsMaxLevel)
            {
                StatueUpgradeEffect effect = _model.Upgrade();
                Publish(new StatueUpgradedMessage(_model.Level, effect, 0));

                if (effect.UnlocksTerritory)
                {
                    Publish(new TerritoryUnlockedMessage(effect.UnlockedTerritoryIndex));
                }
            }

            _model.Heal(_model.MaxHealth);
            _view?.SetLevel(_model.Level);
            Publish(new StatueDamagedMessage(_model.Health, _model.MaxHealth));
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState == EnumManager.GameState.Playing)
            {
                _model.Reset();
                ClearProgress();
                if (_view != null)
                {
                    _view.SetLevel(_model.Level);
                }
            }
        }

        private bool AllWallsBuilt(int territoryIndex)
        {
            return territoryIndex >= 0
                && territoryIndex < _wallSegmentsBuilt.Length
                && _wallSegmentsBuilt[territoryIndex] >= TerritoryState.WallSegmentCount;
        }

        private void ClearProgress()
        {
            for (int i = 0; i < _wallSegmentsBuilt.Length; i++)
            {
                _wallSegmentsBuilt[i] = 0;
            }

            _monstersKilled = 0;
            _guardsAtLevel3 = 0;
            _guardsAtLevel5 = 0;
        }
    }
}
