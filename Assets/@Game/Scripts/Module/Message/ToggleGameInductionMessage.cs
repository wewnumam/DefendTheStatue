namespace ProjectTA.Message
{
    public struct ToggleGameInductionMessage
    {
        public bool IsGameInductionActive { get; }

        public ToggleGameInductionMessage(bool isGameInductionActive)
        {
            IsGameInductionActive = isGameInductionActive;
        }
    }
}