using Agate.MVC.Base;
using ProjectTA.Utility;

namespace ProjectTA.Module.DevConsole
{
    /// <summary>The eight console actions, in panel order.</summary>
    public enum DevConsoleAction
    {
        AddEssence = 0,
        AddMana = 1,
        HealStatue = 2,
        HealPlayer = 3,
        KillAll = 4,
        SpawnBoss = 5,
        KillBoss = 6,
        MaxLevel = 7
    }

    /// <summary>
    /// Eight cheat flags and eight one-shot actions. The model decides nothing about the effects: the
    /// controller publishes CheatToggledMessage / Dev* messages and the owning modules react.
    /// </summary>
    public class DevConsoleModel : BaseModel
    {
        public const int FlagCount = 9;

        private readonly bool[] _flags = new bool[FlagCount];

        public bool IsAvailable { get; private set; }
        public bool IsOpen { get; private set; }

        public void SetAvailable(bool isAvailable)
        {
            IsAvailable = isAvailable;
            if (!isAvailable)
            {
                IsOpen = false;
            }

            SetDataAsDirty();
        }

        public bool IsOn(EnumManager.CheatType cheat)
        {
            return IsAvailable && _flags[(int)cheat];
        }

        public bool Toggle(EnumManager.CheatType cheat)
        {
            if (!IsAvailable)
            {
                return false;
            }

            _flags[(int)cheat] = !_flags[(int)cheat];
            SetDataAsDirty();
            return _flags[(int)cheat];
        }

        public bool ToggleOpen()
        {
            if (!IsAvailable)
            {
                return false;
            }

            IsOpen = !IsOpen;
            SetDataAsDirty();
            return IsOpen;
        }

        public void Reset()
        {
            for (int i = 0; i < _flags.Length; i++)
            {
                _flags[i] = false;
            }

            IsOpen = false;
            SetDataAsDirty();
        }
    }
}
