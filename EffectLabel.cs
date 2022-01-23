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
    public class EffectLabel : Label
    {
        bool designmode = false;

        public EffectLabel()
        {
            DoubleBuffered = true;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, false);

            designmode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var useShadow = SkinManager.GetParam("effectLabel", "shadow")?.ToLower() == "true";
            var shadowColor = useShadow ? ColorTranslator.FromHtml(SkinManager.GetParam("effectLabel", "shadowColor") ?? "transparent") : Color.Transparent;
            var shadowOffsetX = useShadow ? int.Parse(SkinManager.GetParam("effectLabel", "shadowOffsetX") ?? "0", NumberStyles.Integer, CultureInfo.InvariantCulture) : 0;
            var shadowOffsetY = useShadow ? int.Parse(SkinManager.GetParam("effectLabel", "shadowOffsetY") ?? "0", NumberStyles.Integer, CultureInfo.InvariantCulture) : 0;

            var flags = TextFormatFlags.WordBreak;

            switch (TextAlign)
            {
                case ContentAlignment.TopLeft:
                    flags |= TextFormatFlags.Top | TextFormatFlags.Left;
                    break;
                case ContentAlignment.TopCenter:
                    flags |= TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
                    break;
                case ContentAlignment.TopRight:
                    flags |= TextFormatFlags.Top | TextFormatFlags.Right;
                    break;
                case ContentAlignment.MiddleLeft:
                    flags |= TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
                    break;
                case ContentAlignment.MiddleCenter:
                    flags |= TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                    break;
                case ContentAlignment.MiddleRight:
                    flags |= TextFormatFlags.Right | TextFormatFlags.VerticalCenter;
                    break;
                case ContentAlignment.BottomLeft:
                    flags |= TextFormatFlags.Bottom | TextFormatFlags.Left;
                    break;
                case ContentAlignment.BottomCenter:
                    flags |= TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                    break;
                case ContentAlignment.BottomRight:
                    flags |= TextFormatFlags.Bottom | TextFormatFlags.Right;
                    break;
                default:
                    break;
            }

            if (useShadow)
                TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(new Point(shadowOffsetX, 0 + shadowOffsetY), Size), shadowColor, flags);

            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(new Point(0, 0), Size), ForeColor, flags);
        }
    }
}
