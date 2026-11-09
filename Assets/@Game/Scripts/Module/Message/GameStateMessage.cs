using ProjectTA.Utility;

namespace ProjectTA.Message
{
    public struct GameStateMessage
    {
        public EnumManager.GameState GameState { get; }

        public GameStateMessage(EnumManager.GameState gameState)
        {
            GameState = gameState;
        }
    }
}