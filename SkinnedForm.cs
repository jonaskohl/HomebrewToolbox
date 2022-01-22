using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiiBrewToolbox
{
    public class SkinnedForm : Form
    {
        public SkinnedForm() : base()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            ApplySkinInternal();
        }

        public virtual void ApplySkin()
        {
            ApplySkinInternal();
        }

        private void ApplySkinInternal()
        {
            ForeColor = SkinManager.WindowForeground;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            if (SkinManager.BackgroundImage != null)
            {
                switch (SkinManager.BackgroundSizeMode)
                {
                    case ImageSizeMode.None:
                        e.Graphics.DrawImage(SkinManager.BackgroundImage, new Rectangle(0, 0, SkinManager.BackgroundImage.Width, SkinManager.BackgroundImage.Height));
                        break;
                    case ImageSizeMode.Tile:
                        using (var b = new TextureBrush(SkinManager.BackgroundImage))
                            e.Graphics.FillRectangle(b, new Rectangle(0, 0, Width, Height));
                        break;
                    case ImageSizeMode.Stretch:
                        e.Graphics.DrawImage(SkinManager.BackgroundImage, new Rectangle(0, 0, Width, Height));
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
