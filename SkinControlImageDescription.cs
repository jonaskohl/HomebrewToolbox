using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;

namespace WiiBrewToolbox
{
    public class SkinControlImageDescription
    {
        public string Name { get; private set; }

        private List<SkinControlImageState> states = new List<SkinControlImageState>();

        public ReadOnlyCollection<SkinControlImageState> ImageStates
        {
            get => states.AsReadOnly();
        }

        public SkinControlImageDescription(string name, IEnumerable<SkinControlImageState> states)
        {
            Name = name;
            this.states.AddRange(states);
        }

        public static readonly SkinControlImageDescription ButtonsDefault = new SkinControlImageDescription("BUTTONS", new[] {
            new SkinControlImageState() { ForState = ButtonImageState.Normal, From = new Point(0, 0), Size = new Size(24, 24), Slices = new Padding(8), Padding = Padding.Empty },
            new SkinControlImageState() { ForState = ButtonImageState.Hot, From = new Point(0, 24), Size = new Size(24, 24), Slices = new Padding(8), Padding = Padding.Empty },
            new SkinControlImageState() { ForState = ButtonImageState.Pressed, From = new Point(0, 48), Size = new Size(24, 24), Slices = new Padding(8), Padding = Padding.Empty },
            new SkinControlImageState() { ForState = ButtonImageState.Disabled, From = new Point(0, 72), Size = new Size(24, 24), Slices = new Padding(8), Padding = Padding.Empty }
        });
    }
}
