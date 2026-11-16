using Agate.MVC.Base;
using ProjectTA.Module.Statue;

namespace ProjectTA.Module.Hud
{
    /// <summary>What the HUD has to remember between messages: the statue bar's last known pair.</summary>
    public class HudModel : BaseModel
    {
        public float StatueHealth { get; private set; } = StatueModel.BaseMaxHealth;
        public float StatueMaxHealth { get; private set; } = StatueModel.BaseMaxHealth;

        public void Reset()
        {
            StatueHealth = StatueModel.BaseMaxHealth;
            StatueMaxHealth = StatueModel.BaseMaxHealth;
            SetDataAsDirty();
        }

        public void SetStatue(float health, float maxHealth)
        {
            StatueHealth = health;
            StatueMaxHealth = maxHealth;
            SetDataAsDirty();
        }

        /// <summary>An upgrade raises both the cap and the current health, so the bar reads full again.</summary>
        public void ApplyUpgrade(float maxHealthDelta)
        {
            StatueMaxHealth += maxHealthDelta;
            StatueHealth = StatueMaxHealth;
            SetDataAsDirty();
        }
    }
}