using NUnit.Framework;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>
    /// GDD §13 skill 1 numbers. Kept apart from the skill-model tests because those load the
    /// GameConstants asset, which the headless harness cannot do; these are plain constants.
    /// </summary>
    public class Skill1GddTests
    {
        [Test]
        public void Skill1_SwordDashDealsTwo()
        {
            Assert.That(ProjectTA.Module.PlayerCharacter.PlayerCharacterController.DashDamage,
                Is.EqualTo(2f), "GDD §13: lv 3 sword dash damage 2");
        }

        [Test]
        public void Skill1_BowTripleShotDealsFourPerArrow()
        {
            Assert.That(ProjectTA.Module.BulletPool.BulletPoolController.TripleShotDamage,
                Is.EqualTo(4f), "GDD §13: lv 3 bow triple shot 4 per arrow");
        }
    }
}
