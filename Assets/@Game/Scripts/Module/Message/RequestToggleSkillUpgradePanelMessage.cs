namespace ProjectTA.Message
{
    /// <summary>
    /// Asks for the skill upgrade interface to open or close. Only the skill module holds the open
    /// flag, so a second key (E over the god statue) can toggle the same panel without desyncing it.
    /// </summary>
    public struct RequestToggleSkillUpgradePanelMessage
    {
    }
}
