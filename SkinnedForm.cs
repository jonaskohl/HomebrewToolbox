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

            e.Graphics.Clear(SkinManager.WindowBackground);

            if (SkinManager.BackgroundImage != null)
            {
                switch (SkinManager.BackgroundSizeMode)
                {
                    case ImageSizeMode.None:
                        DrawImageWithGravity(e.Graphics, SkinManager.BackgroundImage, SkinManager.BackgroundGravity, ClientSize);
                        break;
                    case ImageSizeMode.Tile:
                        using (var b = new TextureBrush(SkinManager.BackgroundImage))
                            e.Graphics.FillRectangle(b, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
                        break;
                    case ImageSizeMode.TileX:
                        {
                            var r = GetGravityRectangleHorizontal(ClientSize, SkinManager.BackgroundImage.Size, SkinManager.BackgroundGravity);
                            DrawTiledImage(e.Graphics, SkinManager.BackgroundImage, r);
                        }
                        break;
                    case ImageSizeMode.TileY:
                        {
                            var r = GetGravityRectangleVertical(ClientSize, SkinManager.BackgroundImage.Size, SkinManager.BackgroundGravity);
                            DrawTiledImage(e.Graphics, SkinManager.BackgroundImage, r);
                        }
                        break;
                    case ImageSizeMode.Stretch:
                        e.Graphics.DrawImage(SkinManager.BackgroundImage, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
                        break;
                    default:
                        break;
                }
            }
        }

        private void DrawTiledImage(Graphics g, Image img, Rectangle r)
        {
            var w = img.Width;
            var h = img.Height;

            var timesX = (int)Math.Ceiling((decimal)r.Width / w);
            var timesY = (int)Math.Ceiling((decimal)r.Height / h);

            var c = g.Clip;
            g.SetClip(r);

            for (var x = 0; x < timesX; ++x)
            {
                for (var y = 0; y < timesY; ++y)
                {
                    g.DrawImage(img, new Rectangle(r.X + x * w, r.Y + y * h, w, h));
                }
            }

            g.Clip = c;
        }

        private Rectangle GetGravityRectangleHorizontal(Size bounds, Size imageSize, Gravity gravity)
        {
            var loc = new Point(0, bounds.Height / 2 - imageSize.Height / 2);

            if (gravity.HasFlag(Gravity.Top))
                loc.Y = 0;
            else if (gravity.HasFlag(Gravity.Bottom))
                loc.Y = bounds.Height - imageSize.Height;

            return new Rectangle(loc, new Size(bounds.Width, imageSize.Height));
        }

        private Rectangle GetGravityRectangleVertical(Size bounds, Size imageSize, Gravity gravity)
        {
            var loc = new Point(bounds.Width / 2 - imageSize.Width / 2, 0);

            if (gravity.HasFlag(Gravity.Left))
                loc.X = 0;
            else if (gravity.HasFlag(Gravity.Right))
                loc.X = bounds.Width - imageSize.Width;

            return new Rectangle(loc, new Size(imageSize.Width, bounds.Height));
        }

        private void DrawImageWithGravity(Graphics g, Image image, Gravity gravity, Size size)
        {
            var loc = new Point(size.Width / 2 - image.Width / 2, size.Height / 2 - image.Height / 2);

            if (gravity.HasFlag(Gravity.Left))
                loc.X = 0;
            else if (gravity.HasFlag(Gravity.Right))
                loc.X = size.Width - image.Width;

            if (gravity.HasFlag(Gravity.Top))
                loc.Y = 0;
            else if (gravity.HasFlag(Gravity.Bottom))
                loc.Y = size.Height - image.Height;

            var rect = new Rectangle(loc, SkinManager.BackgroundImage.Size);

            g.DrawImage(image, rect);
        }
    }
}
