using Agate.MVC.Base;
using ProjectTA.Module.Boss;
using ProjectTA.Module.Build;
using ProjectTA.Module.CameraRig;
using ProjectTA.Module.DevConsole;
using ProjectTA.Module.BulletPool;
using ProjectTA.Module.GameFlow;
using ProjectTA.Module.Guard;
using ProjectTA.Module.GameStateUi;
using ProjectTA.Module.Hud;
using ProjectTA.Module.Monster;
using ProjectTA.Module.PlayerCharacter;
using ProjectTA.Module.PlayerSkill;
using ProjectTA.Module.Settings;
using ProjectTA.Module.SlowField;
using ProjectTA.Module.Statue;
using ProjectTA.Module.Sword;
using ProjectTA.Module.Teleport;
using ProjectTA.Module.Territory;
using ProjectTA.Module.Tower;
using ProjectTA.Module.Trap;
using ProjectTA.Module.Tree;
using ProjectTA.Module.Tutorial;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectTA.Scene.Gameplay
{
    public class GameplayView : BaseSceneView
    {
        [field: SerializeField]
        public TutorialView TutorialView { get; private set; }
        [field: SerializeField]
        public SettingsView SettingsView { get; private set; }
        [field: SerializeField]
        public PlayerCharacterView PlayerCharacterView { get; private set; }
        [field: SerializeField]
        public BulletPoolView BulletPoolView { get; private set; }
        [field: SerializeField]
        public SwordView SwordView { get; private set; }
        [field: SerializeField]
        public GameFlowView GameFlowView { get; private set; }
        [field: SerializeField]
        public MonsterPoolView MonsterPoolView { get; private set; }
        [field: SerializeField]
        public StatueView StatueView { get; private set; }
        [field: SerializeField]
        public TerritoryView TerritoryView { get; private set; }
        [field: SerializeField]
        public BuildView BuildView { get; private set; }
        [field: SerializeField]
        public TowerView TowerView { get; private set; }
        [field: SerializeField]
        public TreeView TreeView { get; private set; }
        [field: SerializeField]
        public TrapView TrapView { get; private set; }
        [field: SerializeField]
        public GuardView GuardView { get; private set; }
        [field: SerializeField]
        public BossView BossView { get; private set; }
        [field: SerializeField]
        public DevConsoleView DevConsoleView { get; private set; }
        [field: SerializeField]
        public CameraRigView CameraRigView { get; private set; }
        [field: SerializeField]
        public PlayerSkillView PlayerSkillView { get; private set; }
        [field: SerializeField]
        public HudView HudView { get; private set; }
        [field: SerializeField]
        public SlowFieldView SlowFieldView { get; private set; }
        [field: SerializeField]
        public GameStateUiView GameStateUiView { get; private set; }
        [field: SerializeField]
        public TeleportView TeleportView { get; private set; }

        private UnityAction _onMainMenu;
        private UnityAction _onPlayAgain;

        public void MainMenu()
        {
            _onMainMenu?.Invoke();
        }

        public void PlayAgain()
        {
            _onPlayAgain?.Invoke();
        }

        public void SetCallbacks(UnityAction onMainMenu, UnityAction onPlayAgain)
        {
            _onMainMenu = onMainMenu;
            _onPlayAgain = onPlayAgain;
        }
    }
}
