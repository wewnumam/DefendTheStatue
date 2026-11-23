using NUnit.Framework;
using ProjectTA.Module.DevConsole;
using ProjectTA.Utility;

namespace ProjectTA.Tests.EditMode
{
    /// <summary>Cheat flags only exist when the console is available (debug build).</summary>
    public class DevConsoleModelTests
    {
        private DevConsoleModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new DevConsoleModel();
            _model.SetAvailable(true);
        }

        [Test]
        public void Toggle_FlipsAndReportsTheNewValue()
        {
            Assert.That(_model.Toggle(EnumManager.CheatType.GodMode), Is.True);
            Assert.That(_model.IsOn(EnumManager.CheatType.GodMode), Is.True);

            Assert.That(_model.Toggle(EnumManager.CheatType.GodMode), Is.False);
            Assert.That(_model.IsOn(EnumManager.CheatType.GodMode), Is.False);
        }

        [Test]
        public void Toggle_IsRefusedWhenTheConsoleIsNotAvailable()
        {
            _model.SetAvailable(false);

            Assert.That(_model.Toggle(EnumManager.CheatType.FreeBuild), Is.False);
            Assert.That(_model.IsOn(EnumManager.CheatType.FreeBuild), Is.False);
        }

        [Test]
        public void Disabling_ClosesThePanelAndResetsFlags()
        {
            _model.Toggle(EnumManager.CheatType.NoSpawn);
            _model.ToggleOpen();

            _model.SetAvailable(false);

            Assert.That(_model.IsOpen, Is.False);
            Assert.That(_model.IsOn(EnumManager.CheatType.NoSpawn), Is.False);
        }

        [Test]
        public void Reset_ClearsEveryFlag()
        {
            for (int i = 0; i < DevConsoleModel.FlagCount; i++)
            {
                _model.Toggle((EnumManager.CheatType)i);
            }

            _model.Reset();

            for (int i = 0; i < DevConsoleModel.FlagCount; i++)
            {
                Assert.That(_model.IsOn((EnumManager.CheatType)i), Is.False);
            }
        }
    }
}
