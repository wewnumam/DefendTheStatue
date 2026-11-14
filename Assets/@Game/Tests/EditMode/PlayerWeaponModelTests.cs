using NUnit.Framework;
using ProjectTA.Module.PlayerWeapon;
using ProjectTA.Utility;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>Weapon switching: scroll up arms the bow, scroll down returns the sword.</summary>
    public class PlayerWeaponModelTests
    {
        private PlayerWeaponModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new PlayerWeaponModel();
        }

        [Test]
        public void DefaultsToSword()
        {
            Assert.That(_model.Current, Is.EqualTo(EnumManager.WeaponKind.Sword));
            Assert.That(_model.IsWeaponLocked, Is.False);
        }

        [Test]
        public void ScrollUp_ArmsBow_ScrollDown_ReturnsSword()
        {
            Assert.That(_model.TrySwitch(1f), Is.True);
            Assert.That(_model.Current, Is.EqualTo(EnumManager.WeaponKind.Bow));

            Assert.That(_model.TrySwitch(-1f), Is.True);
            Assert.That(_model.Current, Is.EqualTo(EnumManager.WeaponKind.Sword));
        }

        [Test]
        public void SwitchingToTheWeaponAlreadyInHandReportsNoChange()
        {
            Assert.That(_model.TrySwitch(-1f), Is.False);
            Assert.That(_model.TrySwitch(0f), Is.False);
        }

        [Test]
        public void WeaponLock_RefusesSwitching()
        {
            _model.TrySwitch(1f);
            _model.SetWeaponLocked(true);

            Assert.That(_model.TrySwitch(-1f), Is.False);
            Assert.That(_model.Current, Is.EqualTo(EnumManager.WeaponKind.Bow));
        }

        [Test]
        public void Reset_ReturnsToSwordAndUnlocks()
        {
            _model.TrySwitch(1f);
            _model.SetWeaponLocked(true);

            _model.Reset();

            Assert.That(_model.Current, Is.EqualTo(EnumManager.WeaponKind.Sword));
            Assert.That(_model.IsWeaponLocked, Is.False);
        }
    }
}
