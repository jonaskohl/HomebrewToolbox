using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
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
        public static Color WindowForeground { get; private set; }
        public static Color WindowBackground { get; private set; }
        public static ImageSizeMode BackgroundSizeMode { get; private set; }
        public static Gravity BackgroundGravity { get; private set; }
        public static bool UseWhiteForegrounds { get; private set; }
        public static Cursor MainCursor { get; private set; }
        public static Cursor LinkCursor { get; private set; }
        public static Cursor TextCursor { get; private set; }

        public static Icon AppIcon { get; private set; }

        public static Image ButtonSprite { get; private set; }

        public static Dictionary<string, Dictionary<string, string>> ParamMap { get; private set; }
            = new Dictionary<string, Dictionary<string, string>>();

        private static SkinControlInformation skinControlInformation;

        private static readonly string[] requiredFiles = new[] {
            "ABOUT.PNG", "ADD.PNG", "APP_GENERIC.PNG", "FOLDER.PNG", "URL.PNG", "SETTINGS.PNG",
            "BUTTONS.PNG",
            "CONTROLS.XML", "SKIN.XML"
        };

        private static Cursor defaultCursor;
        private static Cursor handCursor;
        private static Cursor textCursor;

        const string CUR_FIELD_DEFAULT = "defaultCursor";
        const string CUR_FIELD_HAND = "hand";
        const string CUR_FIELD_TEXT = "iBeam";

        static SkinManager()
        {
            defaultCursor = Cursors.Default;
            handCursor = Program.SystemHandCursor;
            textCursor = Cursors.IBeam;

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
            BackgroundGravity = Gravity.Left | Gravity.Top;
            WindowForeground = Color.Black;
            WindowBackground = Color.FromArgb(240, 240, 240);
            AppIcon = null;
            ButtonSprite = Properties.Resources.wtb_buttons_builtin;
            UseWhiteForegrounds = false;
            skinControlInformation = SkinControlInformation.Default;
            Application.VisualStyleState = VisualStyleState.ClientAndNonClientAreasEnabled;
            MainCursor = null;
            LinkCursor = null;
            TextCursor = null;
            SetFrameworkCursor(defaultCursor, CUR_FIELD_DEFAULT);
            SetFrameworkCursor(handCursor, CUR_FIELD_HAND);
            SetFrameworkCursor(textCursor, CUR_FIELD_TEXT);

            ParamMap.Clear();
        }

        private static void SetFrameworkCursor(Cursor cur, string fieldName)
        {
            typeof(Cursors).GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, cur);
        }

        public static Padding GetButtonContentPadding(ButtonImageState state)
        {
            var buttonDesc = skinControlInformation.ImageDescriptions.Where(i => i.Name == "BUTTONS").First();
            var stateDesc = buttonDesc.ImageStates.Where(s => s.ForState == state).First();
            return stateDesc.Padding;
        }

        public static Color GetButtonShadowColor(ButtonImageState state)
        {
            var buttonDesc = skinControlInformation.ColorDescriptions.Where(i => i.Name == "BUTTONS").First();
            var stateDesc = buttonDesc.ColorStates.Where(s => s.Key == "foregroundShadow").Where(s => s.ForState == state).First();
            return stateDesc.color;
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
            }).Where(i => i.DisplayName != null).ToArray();
        }

        public static void LoadSkin(string filename)
        {
            using (ZipFile zip = ZipFile.Read(Path.Combine(Application.StartupPath, filename)))
            {
                var names = zip.Entries.Select(e => e.FileName);
                if (!requiredFiles.All(r => names.Contains(r)))
                    throw new Exception("Invalid skin file. The following files are missing:\r\n" + string.Join("\r\n",
                        requiredFiles.Where(r => !names.Contains(r))
                    ));

                ResetSkin();

                foreach (ZipEntry entry in zip.Entries)
                {
                    if (entry.FileName == "ABOUT.PNG")
                        AboutImage = ReadImage(entry);
                    else if (entry.FileName == "ADD.PNG")
                        AddImage = ReadImage(entry);
                    else if (entry.FileName == "APP_GENERIC.PNG")
                        NoAppIconImage = ReadImage(entry);
                    else if (entry.FileName == "FOLDER.PNG")
                        FolderImage = ReadImage(entry);
                    else if (entry.FileName == "URL.PNG")
                        UrlImage = ReadImage(entry);
                    else if (entry.FileName == "SETTINGS.PNG")
                        SettingsImage = ReadImage(entry);

                    else if (entry.FileName == "BACKGROUND.PNG") // Optional
                        BackgroundImage = ReadImage(entry);

                    else if (entry.FileName == "APP.ICO") // Optional
                        AppIcon = ReadIcon(entry);

                    else if (entry.FileName == "BUTTONS.PNG")
                        ButtonSprite = ReadImage(entry);

                    else if (entry.FileName == "CONTROLS.XML")
                        skinControlInformation = SkinControlInformation.FromXML(ReadXML(entry));

                    else if (entry.FileName == "DEFAULT.CUR")
                    {
                        MainCursor = ReadCursor(entry);
                        SetFrameworkCursor(MainCursor, CUR_FIELD_DEFAULT);
                    }
                    else if (entry.FileName == "HAND.CUR")
                    {
                        LinkCursor = ReadCursor(entry);
                        SetFrameworkCursor(LinkCursor, CUR_FIELD_HAND);
                    }
                    else if (entry.FileName == "TEXT.CUR")
                    {
                        TextCursor = ReadCursor(entry);
                        SetFrameworkCursor(TextCursor, CUR_FIELD_TEXT);
                    }

                    else if (entry.FileName == "SKIN.XML")
                        ReadSkinInfo(ReadXML(entry));
                }
            }
        }

        private static string GetSkinName(string filename)
        {
            using (ZipFile zip = ZipFile.Read(Path.Combine(Application.StartupPath, filename)))
            {
                var names = zip.Entries.Select(e => e.FileName);
                if (!requiredFiles.All(r => names.Contains(r)))
                    return null;

                foreach (ZipEntry entry in zip.Entries)
                {
                    if (entry.FileName == "SKIN.XML")
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

        public static string GetParam(string target, string name)
        {
            if (!ParamMap.ContainsKey(target))
                return null;

            var targetParams = ParamMap[target];

            if (!targetParams.ContainsKey(name))
                return null;

            return targetParams[name];
        }

        private static void ReadSkinInfo(XDocument doc)
        {
            Debug.Assert(doc.Root.Name == "wtbSkin");
            Debug.Assert(doc.Root.Attribute("version")?.Value == "1");

            var xParams = doc.Root.Element("params");

            if (xParams != null)
            {
                var xParamElems = xParams.Elements("param");

                foreach (var xParam in xParamElems)
                {
                    var target = xParam.Attribute("target").Value;
                    var name = xParam.Attribute("name").Value;
                    var paramValue = xParam.Value;

                    if (!ParamMap.ContainsKey(target))
                        ParamMap[target] = new Dictionary<string, string>();

                    ParamMap[target][name] = paramValue;
                }
            }

            var windowForegroundParam = GetParam("window", "foreground");
            var windowBackgroundParam = GetParam("window", "background");
            var backgroundSizingModeParam = GetParam("backgroundImage", "sizingMode");
            var backgroundGravityParam = GetParam("backgroundImage", "gravity");
            var useWhiteForegroundsParam = GetParam("global", "useWhiteForegrounds");
            var useVisualStylesParam = GetParam("global", "useVisualStyles");

            if (windowForegroundParam != null)
                WindowForeground = GetColor(windowForegroundParam);

            if (windowBackgroundParam != null)
                WindowBackground = GetColor(windowBackgroundParam);

            if (backgroundSizingModeParam != null)
                BackgroundSizeMode = GetSizeMode(backgroundSizingModeParam);

            if (backgroundGravityParam != null)
                BackgroundGravity = GetGravities(backgroundGravityParam);

            if (useWhiteForegroundsParam != null)
                UseWhiteForegrounds = useWhiteForegroundsParam.ToLower() == "true";

            Application.VisualStyleState = (useVisualStylesParam?.ToLower() == "true") ? VisualStyleState.ClientAndNonClientAreasEnabled : VisualStyleState.NonClientAreaEnabled;
        }

        private static Gravity GetGravities(string val)
        {
            var vals = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var g = Gravity.None;
            foreach (var v in vals)
                g |= GetGravity(v);
            return g;
        }

        private static Gravity GetGravity(string val)
        {
            switch (val)
            {
                case "": case "none": return Gravity.None;
                case "left": return Gravity.Left;
                case "right": return Gravity.Right;
                case "top": return Gravity.Top;
                case "bottom": return Gravity.Bottom;
                default:
                    throw new Exception("Invalid gravity");
            }
        }

        private static Color GetColor(string val)
        {
            return ColorTranslator.FromHtml(val);
        }

        private static ImageSizeMode GetSizeMode(string val)
        {
            switch (val)
            {
                case "none":
                case "":
                    return ImageSizeMode.None;
                case "tile":
                    return ImageSizeMode.Tile;
                case "tile-x":
                    return ImageSizeMode.TileX;
                case "tile-y":
                    return ImageSizeMode.TileY;
                case "stretch":
                    return ImageSizeMode.Stretch;
                default:
                    throw new Exception("Invalid size mode");
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

        private static XDocument ReadXML(ZipEntry entry)
        {
            using (var stream = entry.OpenReader())
                return XDocument.Load(stream);
        }

        private static Image ReadImage(ZipEntry entry)
        {
            using (var stream = entry.OpenReader())
                return Image.FromStream(stream);
        }

        private static Icon ReadIcon(ZipEntry entry)
        {
            using (var stream = entry.OpenReader())
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

        private static Cursor ReadCursor(ZipEntry entry)
        {
            using (var stream = entry.OpenReader())
            using (var memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);
                return NativeMethods.LoadCustomCursorFromFileWithUglyTempFile(memStream);
            }
        }
    }
}
