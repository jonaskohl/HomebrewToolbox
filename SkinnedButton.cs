using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

            var b = new Rectangle(Point.Empty, Size);
            var s = GetButtonState();

            var c = SkinManager.GetButtonColor(s);

            SkinManager.RenderButtonImage(pevent.Graphics, b, s);

            if (Image == null)
            {
                TextRenderer.DrawText(pevent.Graphics, Text, Font, b, c,
                    (ShowKeyboardCues ? 0 : TextFormatFlags.HidePrefix) | TextFormatFlags.HorizontalCenter | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.VerticalCenter
                    );
            } else
            {
                var imgX = Width / 2 - Image.Width / 2;
                pevent.Graphics.DrawImage(Image, new Rectangle(new Point(imgX, 4), Image.Size));
                using (var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far, HotkeyPrefix = ShowKeyboardCues ? System.Drawing.Text.HotkeyPrefix.Show : System.Drawing.Text.HotkeyPrefix.Hide })
                    TextRenderer.DrawText(pevent.Graphics, Text, Font, new Rectangle(
                        4,
                        Image.Height + 8,
                        b.Width - 8,
                        b.Height - 12 - Image.Height
                    ), c, (ShowKeyboardCues ? 0 : TextFormatFlags.HidePrefix) | TextFormatFlags.HorizontalCenter | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            }

            if (ShowFocusCues && Focused)
                ControlPaint.DrawFocusRectangle(pevent.Graphics, new Rectangle(
                    b.X + 3,
                    b.Y + 3,
                    b.Width - 6,
                    b.Height - 6
                ));
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
