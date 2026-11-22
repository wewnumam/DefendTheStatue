using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.GameFlow
{
    /// <summary>Feeds the run controller: spawn cadence and the run clock live inside it.</summary>
    public class GameFlowConnector : BaseConnector
    {
        private readonly GameFlowController _gameFlow = new GameFlowController();

        protected override void Connect()
        {
            Subscribe<WallSegmentBuiltMessage>(_gameFlow.OnWallSegmentBuilt);
            Subscribe<TowerBuiltMessage>(_gameFlow.OnTowerBuilt);
            Subscribe<MonsterKilledMessage>(_gameFlow.OnMonsterKilled);
            Subscribe<TreeChoppedMessage>(_gameFlow.OnTreeChopped);
            Subscribe<GameStateMessage>(_gameFlow.OnGameState);
            Subscribe<SkillUpgradePanelToggledMessage>(_gameFlow.OnSkillUpgradePanelToggled);
            Subscribe<BossDefeatedMessage>(_gameFlow.OnBossDefeated);
            Subscribe<CheatToggledMessage>(_gameFlow.OnCheatToggled);
        }

        protected override void Disconnect()
        {
            Unsubscribe<WallSegmentBuiltMessage>(_gameFlow.OnWallSegmentBuilt);
            Unsubscribe<TowerBuiltMessage>(_gameFlow.OnTowerBuilt);
            Unsubscribe<MonsterKilledMessage>(_gameFlow.OnMonsterKilled);
            Unsubscribe<TreeChoppedMessage>(_gameFlow.OnTreeChopped);
            Unsubscribe<GameStateMessage>(_gameFlow.OnGameState);
            Unsubscribe<SkillUpgradePanelToggledMessage>(_gameFlow.OnSkillUpgradePanelToggled);
            Unsubscribe<BossDefeatedMessage>(_gameFlow.OnBossDefeated);
            Unsubscribe<CheatToggledMessage>(_gameFlow.OnCheatToggled);
        }
    }
}
