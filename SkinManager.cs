using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WiiBrewToolbox
{
    public static class SkinManager
    {
        public static Image AboutImage { get; private set; }
        public static Image AddImage { get; private set; }
        public static Image NoAppIconImage { get; private set; }
        public static Image FolderImage { get; private set; }
        public static Image UrlImage { get; private set; }
        public static Image SettingsImage { get; private set; }
        public static Image BackgroundImage { get; private set; }
        public static ImageSizeMode BackgroundSizeMode { get; set; }
        public static Color WindowForeground { get; set; }

        public static Icon AppIcon { get; private set; }

        public static Image ButtonSprite { get; private set; }

        private static SkinControlInformation skinControlInformation;

        private static readonly string[] requiredFiles = new[] {
            "ABOUT.PNG", "ADD.PNG", "APP_GENERIC.PNG", "FOLDER.PNG", "URL.PNG", "SETTINGS.PNG",
            "BUTTONS.PNG",
            "CONTROLS.XML", "SKIN.XML"
        };

        static SkinManager()
        {
            ResetSkin();
        }

        public static void ResetSkin()
        {
            AboutImage = Properties.Resources.wininfo;
            AddImage = Properties.Resources.winadd;
            NoAppIconImage = Properties.Resources.appgeneric;
            FolderImage = Properties.Resources.folder;
            UrlImage = Properties.Resources.wininet;
            SettingsImage = Properties.Resources.settings;
            BackgroundImage = null;
            BackgroundSizeMode = ImageSizeMode.None;
            WindowForeground = Color.Black;
            AppIcon = null;
            ButtonSprite = Properties.Resources.wtb_buttons_builtin;
            skinControlInformation = SkinControlInformation.Default;
        }

        public static bool SkinExists(string skin)
        {
            var skins = GetAvailableSkins();
            return skins.Any(x => x.Filename == skin);
        }

        public static Color GetButtonColor(ButtonImageState state)
        {
            var buttonDesc = skinControlInformation.ColorDescriptions.Where(i => i.Name == "BUTTONS").First();
            var stateDesc = buttonDesc.ColorStates.Where(s => s.Key == "foreground").Where(s => s.ForState == state).First();
            return stateDesc.color;
        }

        public static SkinInfo[] GetAvailableSkins()
        {
            return Directory.GetFiles(Application.StartupPath, "*.wtbskin").Select(i => new SkinInfo()
            {
                Filename = Path.GetFileNameWithoutExtension(i),
                DisplayName = GetSkinName(i)
            }).ToArray();
        }

        public static void LoadSkin(string filename)
        {
            using (ZipArchive zip = ZipFile.Open(Path.Combine(Application.StartupPath, filename), ZipArchiveMode.Read))
            {
                var names = zip.Entries.Select(e => e.Name);
                if (!requiredFiles.All(r => names.Contains(r)))
                    throw new Exception("Invalid skin file. The following files are missing:\r\n" + string.Join("\r\n",
                        requiredFiles.Where(r => !names.Contains(r))
                    ));

                ResetSkin();

                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.Name == "ABOUT.PNG")
                        AboutImage = ReadImage(entry);
                    else if (entry.Name == "ADD.PNG")
                        AddImage = ReadImage(entry);
                    else if (entry.Name == "APP_GENERIC.PNG")
                        NoAppIconImage = ReadImage(entry);
                    else if (entry.Name == "FOLDER.PNG")
                        FolderImage = ReadImage(entry);
                    else if (entry.Name == "URL.PNG")
                        UrlImage = ReadImage(entry);
                    else if (entry.Name == "SETTINGS.PNG")
                        SettingsImage = ReadImage(entry);

                    else if (entry.Name == "BACKGROUND.PNG") // Optional
                        BackgroundImage = ReadImage(entry);

                    else if (entry.Name == "APP.ICO") // Optional
                        AppIcon = ReadIcon(entry);

                    else if (entry.Name == "BUTTONS.PNG")
                        ButtonSprite = ReadImage(entry);

                    else if (entry.Name == "CONTROLS.XML")
                        skinControlInformation = SkinControlInformation.FromXML(ReadXML(entry));

                    else if (entry.Name == "SKIN.XML")
                        ReadSkinInfo(ReadXML(entry));
                }
            }
        }

        private static string GetSkinName(string filename)
        {
            using (ZipArchive zip = ZipFile.Open(Path.Combine(Application.StartupPath, filename), ZipArchiveMode.Read))
            {
                var names = zip.Entries.Select(e => e.Name);
                if (!requiredFiles.All(r => names.Contains(r)))
                    return null;

                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.Name == "SKIN.XML")
                        return GetSkinName(ReadXML(entry));
                }
            }

            return null;
        }

        private static string GetSkinName(XDocument doc)
        {
            if (doc.Root.Name != "wtbSkin") return null;
            if (doc.Root.Attribute("version")?.Value != "1") return null;

            return doc.Root.Element("name")?.Value;
        }

        private static void ReadSkinInfo(XDocument doc)
        {
            Debug.Assert(doc.Root.Name == "wtbSkin");
            Debug.Assert(doc.Root.Attribute("version")?.Value == "1");

            var xParams = doc.Root.Element("params");

            if (xParams != null)
            {
                var xParamElems = xParams.Elements("param");

                // TODO Create a proper param map
                foreach (var xParam in xParamElems)
                {
                    if (xParam.Attribute("target")?.Value == "backgroundImage" && xParam.Attribute("name")?.Value == "sizingMode")
                    {
                        var paramValue = xParam.Value;

                        switch (paramValue)
                        {
                            case "none":
                            case "":
                                BackgroundSizeMode = ImageSizeMode.None;
                                break;
                            case "tile":
                                BackgroundSizeMode = ImageSizeMode.Tile;
                                break;
                            case "stretch":
                                BackgroundSizeMode = ImageSizeMode.Stretch;
                                break;
                            default:
                                throw new Exception("Invalid size mode");
                        }
                    }

                    if (xParam.Attribute("target")?.Value == "window" && xParam.Attribute("name")?.Value == "foreground")
                    {
                        WindowForeground = ColorTranslator.FromHtml(xParam.Value);
                    }
                }
            }
        }

        public static void RenderBackgroundImage(Graphics g, Control control)
        {
            if (BackgroundImage == null)
                return;

            var pt = control.PointToClient(control.PointToScreen(Point.Empty));
            var rect = new Rectangle(pt, new Size(pt.X + control.Width, pt.Y + control.Height));

            using (var b = new TextureBrush(BackgroundImage))
                g.FillRectangle(b, rect);
        }

        public static void RenderButtonImage(Graphics g, Rectangle dest, ButtonImageState state)
        {
            var buttonDesc = skinControlInformation.ImageDescriptions.Where(i => i.Name == "BUTTONS").First();
            var stateDesc = buttonDesc.ImageStates.Where(s => s.ForState == state).First();
            var from = stateDesc.From;
            var size = stateDesc.Size;
            var slices = stateDesc.Slices;

            using (var subImage = new Bitmap(size.Width, size.Height))
            using (var sG = Graphics.FromImage(subImage))
            {
                sG.DrawImage(ButtonSprite, new Rectangle(Point.Empty, size), new Rectangle(from, size), GraphicsUnit.Pixel);
                DrawNineSliceBitmap(g, subImage, dest, slices);
            }
        }

        public static void RenderTransparentBackground(Graphics g, Control control)
        {
            if ((control.BackColor == Color.Transparent) && (control.Parent != null))
            {
                Bitmap behind = new Bitmap(control.Parent.Width, control.Parent.Height);
                foreach (Control c in control.Parent.Controls)
                {
                    if (c != control && c.Bounds.IntersectsWith(control.Bounds))
                    {
                        c.DrawToBitmap(behind, c.Bounds);
                    }
                }
                g.DrawImage(behind, -control.Left, -control.Top);
                behind.Dispose();
            }
        }

        public static void DrawNineSliceBitmap(Graphics graphics, Image sourceImage, Rectangle destinationRectangle, Padding sizingMargins = new Padding())
        {
            //calculate sizes for each slice to cut from the original image
            int leftX = 0;
            int rightX = sourceImage.Width - sizingMargins.Right;
            int centerX = 0 + sizingMargins.Left;

            int topY = 0;
            int bottomY = sourceImage.Height - sizingMargins.Bottom;
            int centerY = sizingMargins.Top;

            int topHeight = sizingMargins.Top;
            int bottomHeight = sizingMargins.Bottom;
            int centerHeight = sourceImage.Height - sizingMargins.Vertical;

            int leftWidth = sizingMargins.Left;
            int rightWidth = sizingMargins.Right;
            int centerWidth = sourceImage.Width - sizingMargins.Horizontal;

            //declare the bounds for each slice using the values above
            Rectangle topLeftSrc = new Rectangle(leftX, topY, leftWidth, topHeight);
            Rectangle topCenterSrc = new Rectangle(centerX, topY, centerWidth, topHeight);
            Rectangle topRightSrc = new Rectangle(rightX, topY, rightWidth, topHeight);

            Rectangle bottomLeftSrc = new Rectangle(leftX, bottomY, leftWidth, bottomHeight);
            Rectangle bottomCenterSrc = new Rectangle(centerX, bottomY, centerWidth, bottomHeight);
            Rectangle bottomRightSrc = new Rectangle(rightX, bottomY, rightWidth, bottomHeight);

            Rectangle centerLeftSrc = new Rectangle(leftX, centerY, leftWidth, centerHeight);
            Rectangle centerCenterSrc = new Rectangle(centerX, centerY, centerWidth, centerHeight);
            Rectangle centerRightSrc = new Rectangle(rightX, centerY, rightWidth, centerHeight);

            //calculate sizes for each slice to be drawn to the screen

            //x positions for left, right and center slices
            leftX = destinationRectangle.Left;
            rightX = destinationRectangle.Right - sizingMargins.Right;
            centerX = destinationRectangle.Left + sizingMargins.Left;

            //y positions for top, bottom and center slices
            topY = destinationRectangle.Top;
            bottomY = destinationRectangle.Bottom - sizingMargins.Bottom;
            centerY = destinationRectangle.Top + sizingMargins.Top;

            //heights for left, right and center slices
            topHeight = sizingMargins.Top;
            bottomHeight = sizingMargins.Bottom;
            centerHeight = destinationRectangle.Height - sizingMargins.Vertical;

            //widths for top, bottom and center slices
            leftWidth = sizingMargins.Left;
            rightWidth = sizingMargins.Right;
            centerWidth = destinationRectangle.Width - sizingMargins.Horizontal;

            //declare the bounds for each slice using the values above
            Rectangle topLeftDest = new Rectangle(leftX, topY, leftWidth, topHeight);
            Rectangle topCenterDest = new Rectangle(centerX, topY, centerWidth, topHeight);
            Rectangle topRightDest = new Rectangle(rightX, topY, rightWidth, topHeight);

            Rectangle bottomLeftDest = new Rectangle(leftX, bottomY, leftWidth, bottomHeight);
            Rectangle bottomCenterDest = new Rectangle(centerX, bottomY, centerWidth, bottomHeight);
            Rectangle bottomRightDest = new Rectangle(rightX, bottomY, rightWidth, bottomHeight);

            Rectangle centerLeftDest = new Rectangle(leftX, centerY, leftWidth, centerHeight);
            Rectangle centerCenterDest = new Rectangle(centerX, centerY, centerWidth, centerHeight);
            Rectangle centerRightDest = new Rectangle(rightX, centerY, rightWidth, centerHeight);

            //draw each slice to the screen

            graphics.DrawImage(sourceImage, topLeftDest, topLeftSrc, GraphicsUnit.Pixel);
            graphics.DrawImage(sourceImage, topCenterDest, topCenterSrc, GraphicsUnit.Pixel);
            graphics.DrawImage(sourceImage, topRightDest, topRightSrc, GraphicsUnit.Pixel);

            graphics.DrawImage(sourceImage, bottomLeftDest, bottomLeftSrc, GraphicsUnit.Pixel);
            graphics.DrawImage(sourceImage, bottomCenterDest, bottomCenterSrc, GraphicsUnit.Pixel);
            graphics.DrawImage(sourceImage, bottomRightDest, bottomRightSrc, GraphicsUnit.Pixel);

            graphics.DrawImage(sourceImage, centerLeftDest, centerLeftSrc, GraphicsUnit.Pixel);
            graphics.DrawImage(sourceImage, centerCenterDest, centerCenterSrc, GraphicsUnit.Pixel);
            graphics.DrawImage(sourceImage, centerRightDest, centerRightSrc, GraphicsUnit.Pixel);
        }

        private static XDocument ReadXML(ZipArchiveEntry entry)
        {
            using (var stream = entry.Open())
                return XDocument.Load(stream);
        }

        private static Image ReadImage(ZipArchiveEntry entry)
        {
            using (var stream = entry.Open())
                return Image.FromStream(stream);
        }

        private static Icon ReadIcon(ZipArchiveEntry entry)
        {
            using (var stream = entry.Open())
            using (var memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);
                var data = memStream.ToArray();

                var instance = (Icon)Activator.CreateInstance(typeof(Icon), true);

                var iconDataField = instance.GetType().GetField("iconData", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                iconDataField.SetValue(instance, data);

                var initializeMethod = instance.GetType().GetMethod("Initialize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                initializeMethod.Invoke(instance, new object[] { 0, 0 });

                return instance;
            }
        }
    }
}
