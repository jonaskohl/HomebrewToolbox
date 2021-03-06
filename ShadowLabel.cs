using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiiBrewToolbox
{
    public class ShadowLabel : Label
    {

        [Category("Appearance"), DefaultValue(typeof(Point), "{X=4,Y=4}")]
        public Point ShadowOffset { get; set; }

        [Category("Appearance"), DefaultValue(typeof(Color), "Black")]
        public Color ShadowColor { get; set; }

        public ShadowLabel()
        {
            SetStyle(ControlStyles.Opaque, false);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            TextRenderer.DrawText(e.Graphics, Text, Font, ShadowOffset, ShadowColor);
            TextRenderer.DrawText(e.Graphics, Text, Font, Point.Empty, ForeColor);
        }
    }
}
