namespace ProjectTA.Message
{
    /// <summary>
    /// Raised by the skill upgrade interface: opening it drops the run into slow motion, closing it
    /// restores normal speed. GameFlow owns the time scale, so nobody else touches Time.timeScale.
    /// </summary>
    public struct SkillUpgradePanelToggledMessage
    {
        public bool IsOpen { get; }

        public SkillUpgradePanelToggledMessage(bool isOpen)
        {
            IsOpen = isOpen;
        }
    }
}
