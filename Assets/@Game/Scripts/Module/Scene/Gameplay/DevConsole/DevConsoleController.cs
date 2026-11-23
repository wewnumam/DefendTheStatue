using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.GameConstants;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.DevConsole
{
    /// <summary>
    /// Publishes what the console asks for. Every effect belongs to the module that owns the state:
    /// this controller never reaches into a player, statue or pool.
    /// </summary>
    public class DevConsoleController : ObjectController<DevConsoleController, DevConsoleModel, DevConsoleView>
    {
        public void InitModel(IGameConstantsModel gameConstants)
        {
            bool isDebug = gameConstants != null
                && gameConstants.GameConstants != null
                && gameConstants.GameConstants.IsDebugBuild;

            _model.SetAvailable(isDebug);
        }

        public override void SetView(DevConsoleView view)
        {
            base.SetView(view);
            view.SetCallbacks(OnToggleOpen, OnToggle, OnAction);
            view.SetAvailable(_model.IsAvailable);
        }

        private void OnToggleOpen()
        {
            bool open = _model.ToggleOpen();
            _view.SetOpen(open);

            if (open)
            {
                return;
            }

            // Closing the console drops every cheat: a half-on god mode is worse than none.
            for (int i = 0; i < DevConsoleModel.FlagCount; i++)
            {
                EnumManager.CheatType cheat = (EnumManager.CheatType)i;
                if (!_model.IsOn(cheat))
                {
                    continue;
                }

                _model.Toggle(cheat);
                _view.SetFlag(cheat, false);
                Publish(new CheatToggledMessage(cheat, false));
            }
        }

        private void OnToggle(EnumManager.CheatType cheat)
        {
            bool isOn = _model.Toggle(cheat);
            _view.SetFlag(cheat, isOn);
            Publish(new CheatToggledMessage(cheat, isOn));
        }

        private void OnAction(DevConsoleAction action)
        {
            switch (action)
            {
                case DevConsoleAction.AddEssence:
                    Publish(new DevAddEssenceMessage(1000));
                    break;

                case DevConsoleAction.AddMana:
                    Publish(new DevAddManaMessage(100f));
                    break;

                case DevConsoleAction.HealStatue:
                    Publish(new DevHealStatueMessage());
                    break;

                case DevConsoleAction.HealPlayer:
                    Publish(new DevHealPlayerMessage());
                    break;

                case DevConsoleAction.KillAll:
                    Publish(new DevKillAllMessage());
                    break;

                case DevConsoleAction.SpawnBoss:
                    Publish(new BossSpawnedMessage());
                    break;

                case DevConsoleAction.KillBoss:
                    Publish(new RequestDamageBossMessage(999999f, Vector2.zero));
                    break;

                case DevConsoleAction.MaxLevel:
                    Publish(new DevMaxLevelMessage());
                    break;
            }
        }
    }
}
