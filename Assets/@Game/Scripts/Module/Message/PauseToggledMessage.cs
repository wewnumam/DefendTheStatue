namespace ProjectTA.Message
{
    /// <summary>
    /// Pause is a time scale owned by GameFlow, not a run state, so it needs its own message: a UI
    /// that showed the pause menu off GameStateMessage would have to publish Playing to close it,
    /// and every module resets on Playing.
    /// </summary>
    public struct PauseToggledMessage
    {
        public bool IsPaused { get; }

        public PauseToggledMessage(bool isPaused)
        {
            IsPaused = isPaused;
        }
    }
}
