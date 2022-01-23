using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace WiiBrewToolbox
{
    public class SkinnedButton : Button
    {
        private bool IsMouseOver => GetFlag(0x0001);
        private bool IsMouseDown => GetFlag(0x0002);
        //private bool IsThisDefault => GetFlag(0x0040);
        //private bool IsDefaultButton
        //{
        //    get
        //    {
        //        var flag = !IsMouseOver && !IsMouseDown;
        //        if (!flag)
        //            return false;
        //        var form = FindForm();
        //        if (form == null)
        //            return false;
        //        return flag && form.AcceptButton == this;
        //    }
        //}

        bool inDesignMode;

        private bool GetFlag(int flag)
        {
            var dynMethod = typeof(ButtonBase).GetMethod("GetFlag", BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool)dynMethod.Invoke(this, new object[] { flag });
        }

        public SkinnedButton()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, false);

            inDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            if (inDesignMode)
            {
                base.OnPaint(pevent);
                return;
            }

            var bounds = new Rectangle(Point.Empty, Size);
            var state = GetButtonState();

            var foreground = SkinManager.GetButtonColor(state);
            var useShadow = SkinManager.GetParam("button", "shadow")?.ToLower() == "true";
            var shadowColor = useShadow ? SkinManager.GetButtonShadowColor(state) : Color.Transparent;
            var shadowOffsetX = useShadow ? int.Parse(SkinManager.GetParam("button", "shadowOffsetX") ?? "0", NumberStyles.Integer, CultureInfo.InvariantCulture) : 0;
            var shadowOffsetY = useShadow ? int.Parse(SkinManager.GetParam("button", "shadowOffsetY") ?? "0", NumberStyles.Integer, CultureInfo.InvariantCulture) : 0;

            SkinManager.RenderButtonImage(pevent.Graphics, bounds, state);

            if (Image == null)
            {
                if (useShadow)
                    TextRenderer.DrawText(pevent.Graphics, Text, Font, OffsetRect(bounds, shadowOffsetX, shadowOffsetY), shadowColor,
                        (ShowKeyboardCues ? 0 : TextFormatFlags.HidePrefix) | TextFormatFlags.HorizontalCenter | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.VerticalCenter
                        );

                TextRenderer.DrawText(pevent.Graphics, Text, Font, bounds, foreground,
                    (ShowKeyboardCues ? 0 : TextFormatFlags.HidePrefix) | TextFormatFlags.HorizontalCenter | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.VerticalCenter
                    );
            }
            else
            {
                var imgX = Width / 2 - Image.Width / 2;
                pevent.Graphics.DrawImage(Image, new Rectangle(new Point(imgX, 4), Image.Size));
                var tr = new Rectangle(
                    4,
                    Image.Height + 8,
                    bounds.Width - 8,
                    bounds.Height - 12 - Image.Height
                );

                if (useShadow)
                    TextRenderer.DrawText(pevent.Graphics, Text, Font, OffsetRect(tr, shadowOffsetX, shadowOffsetY), shadowColor, (ShowKeyboardCues ? 0 : TextFormatFlags.HidePrefix) | TextFormatFlags.HorizontalCenter | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);

                TextRenderer.DrawText(pevent.Graphics, Text, Font, tr, foreground, (ShowKeyboardCues ? 0 : TextFormatFlags.HidePrefix) | TextFormatFlags.HorizontalCenter | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            }

            if (ShowFocusCues && Focused)
                ControlPaint.DrawFocusRectangle(pevent.Graphics, new Rectangle(
                    bounds.X + 3,
                    bounds.Y + 3,
                    bounds.Width - 6,
                    bounds.Height - 6
                ));
        }

        private Rectangle OffsetRect(Rectangle bounds, int x, int y)
        {
            return new Rectangle(bounds.X + x, bounds.Y + y, bounds.Width, bounds.Height);
        }

        private ButtonImageState GetButtonState()
        {
            if (!Enabled)
                return ButtonImageState.Disabled;

            if (IsMouseDown)
                return ButtonImageState.Pressed;

            if (IsMouseOver)
                return ButtonImageState.Hot;

            return ButtonImageState.Normal;
        }
    }
}
