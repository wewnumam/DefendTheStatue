using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Utility;

namespace ProjectTA.Module.SlowField
{
    /// <summary>
    /// Owns the live slow zones for the whole run: the time slash and, later, spike traps both post
    /// SlowFieldAddedMessage and land here. Monsters keep their own copy of the same data, so this
    /// module is responsible for the discs and the expiry clock only.
    /// </summary>
    public class SlowFieldController : ObjectController<SlowFieldController, SlowFieldModel, SlowFieldView>
    {
        public override void SetView(SlowFieldView view)
        {
            base.SetView(view);
            _model.Clear();
            view.Clear();
            view.SetTickCallback(OnTick);
        }

        public void OnSlowFieldAdded(SlowFieldAddedMessage message)
        {
            float now = UnityEngine.Time.time;
            _model.Add(message.Center, message.Radius, message.SlowFactor, now, message.DurationSeconds);
            _view.Draw(new SlowField(message.Center, message.Radius, message.SlowFactor, now + message.DurationSeconds));
        }

        private void OnTick(float now)
        {
            _model.RemoveExpired(now);
        }

        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState != EnumManager.GameState.Playing)
            {
                return;
            }

            _model.Clear();

            if (_view != null)
            {
                _view.Clear();
            }
        }
    }
}