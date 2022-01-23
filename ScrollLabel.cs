using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiiBrewToolbox
{
    public class ScrollLabel : Label
    {
        Timer timer;
        bool gotHeight = false;
        int top = 0;
        bool designmode = false;

        public ScrollLabel()
        {
            DoubleBuffered = true;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, false);

            designmode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            timer = new Timer();
            timer.Interval = 1000 / 20; // 20fps
            timer.Tick += timer_Tick;

            if (!designmode)
                timer.Enabled = true;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!gotHeight)
            {
                gotHeight = true;
                top = Height;
                return;
            }

            --top;
            Invalidate();
            if (top < 0)
                top = 0;

            if (top <= 0)
                timer.Enabled = false;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            top = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!designmode && !gotHeight)
                return;

            var useShadow = SkinManager.GetParam("effectLabel", "shadow")?.ToLower() == "true";
            var shadowColor = useShadow ? ColorTranslator.FromHtml(SkinManager.GetParam("effectLabel", "shadowColor") ?? "transparent") : Color.Transparent;
            var shadowOffsetX = useShadow ? int.Parse(SkinManager.GetParam("effectLabel", "shadowOffsetX") ?? "0", NumberStyles.Integer, CultureInfo.InvariantCulture) : 0;
            var shadowOffsetY = useShadow ? int.Parse(SkinManager.GetParam("effectLabel", "shadowOffsetY") ?? "0", NumberStyles.Integer, CultureInfo.InvariantCulture) : 0;

            if (useShadow)
                TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(new Point(shadowOffsetX, top + shadowOffsetY), Size), shadowColor, TextFormatFlags.WordBreak);

            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(new Point(0, top), Size), ForeColor, TextFormatFlags.WordBreak);
        }
    }
}
