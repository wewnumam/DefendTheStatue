using NUnit.Framework;
using ProjectTA.Module.Hud;
using ProjectTA.Utility;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// The pointer priority: a button beats the weapon, the weapon beats the arrow, and a frozen
    /// run always falls back to the arrow.
    /// </summary>
    public class PointerStateTests
    {
        [Test]
        public void ButtonHover_WinsOverBothWeapons()
        {
            Assert.That(HudView.ResolvePointerState(true, EnumManager.WeaponKind.Sword, true),
                Is.EqualTo(HudView.PointerState.Hand));
            Assert.That(HudView.ResolvePointerState(true, EnumManager.WeaponKind.Bow, true),
                Is.EqualTo(HudView.PointerState.Hand));
        }

        [Test]
        public void Playing_ShowsTheWeaponInHand()
        {
            Assert.That(HudView.ResolvePointerState(false, EnumManager.WeaponKind.Sword, true),
                Is.EqualTo(HudView.PointerState.Sword));
            Assert.That(HudView.ResolvePointerState(false, EnumManager.WeaponKind.Bow, true),
                Is.EqualTo(HudView.PointerState.Bow));
        }

        [Test]
        public void FrozenRun_FallsBackToTheArrow()
        {
            Assert.That(HudView.ResolvePointerState(false, EnumManager.WeaponKind.Sword, false),
                Is.EqualTo(HudView.PointerState.Default));
            Assert.That(HudView.ResolvePointerState(false, EnumManager.WeaponKind.Bow, false),
                Is.EqualTo(HudView.PointerState.Default));
        }
    }
}
