using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace WiiBrewToolbox
{
    public class SkinControlColorDescription
    {
        public string Name { get; private set; }

        private List<SkinControlColorState> states = new List<SkinControlColorState>();

        public ReadOnlyCollection<SkinControlColorState> ColorStates
        {
            get => states.AsReadOnly();
        }

        public SkinControlColorDescription(string name, IEnumerable<SkinControlColorState> states)
        {
            Name = name;
            this.states.AddRange(states);
        }

        public static readonly SkinControlColorDescription ButtonsDefault = new SkinControlColorDescription("BUTTONS", new[] {
            new SkinControlColorState() { ForState = ButtonImageState.Normal, Key = "foreground", color = Color.Black },
            new SkinControlColorState() { ForState = ButtonImageState.Hot, Key = "foreground", color = Color.Black },
            new SkinControlColorState() { ForState = ButtonImageState.Pressed, Key = "foreground", color = Color.Black },
            new SkinControlColorState() { ForState = ButtonImageState.Disabled, Key = "foreground", color = Color.Gray }
        });
    }
}