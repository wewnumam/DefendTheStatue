using Agate.MVC.Base;
using ProjectTA.Message;

namespace ProjectTA.Module.SlowField
{
    public class SlowFieldConnector : BaseConnector
    {
        private readonly SlowFieldController _slowField = new SlowFieldController();

        protected override void Connect()
        {
            Subscribe<SlowFieldAddedMessage>(_slowField.OnSlowFieldAdded);
            Subscribe<GameStateMessage>(_slowField.OnGameState);
        }

        protected override void Disconnect()
        {
            Unsubscribe<SlowFieldAddedMessage>(_slowField.OnSlowFieldAdded);
            Unsubscribe<GameStateMessage>(_slowField.OnGameState);
        }
    }
}