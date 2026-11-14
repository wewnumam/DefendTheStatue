using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.Build;
using ProjectTA.Module.Territory;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.Statue
{
    /// <summary>
    /// Statue health, the upgrade ladder and the gates in front of each level. Gold and the wall
    /// counts arrive as messages, so the statue never reaches into the player or territory modules.
    /// </summary>
    public class StatueController : ObjectController<StatueController, StatueModel, StatueView>
    {
        private readonly int[] _wallSegmentsBuilt = new int[TerritoryModel.RingSizes.Length];
        private int _gold;
        private int _monstersKilled;

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
                    _model.GuardFoundationsSpawned,
                    AllWallsBuilt(1),
                    _model.GuardFoundationsSpawned,
                    _monstersKilled))
            {
                return;
            }

            int cost = _model.UpgradeCost;
            if (_gold < cost)
            {
                return;
            }

            StatueUpgradeEffect effect = _model.Upgrade();
            _view.SetLevel(_model.Level);
            Publish(new RequestSpendGoldMessage(cost));
            Publish(new StatueUpgradedMessage(_model.Level, effect, cost));

            if (effect.UnlocksTerritory)
            {
                Publish(new TerritoryUnlockedMessage(effect.UnlockedTerritoryIndex));
            }
        }

        public void OnRequestDamage(RequestStatueDamageMessage message)
        {
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

        public void OnPlayerStatsChanged(PlayerStatsChangedMessage message)
        {
            _gold = message.Gold;
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
        }
    }
}
