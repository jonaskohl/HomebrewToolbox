using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.IconLib;
using System.Drawing.IconLib.Exceptions;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiiBrewToolbox
{
    public static class IconHelper
    {
        public static Bitmap GetLnkIcon(string path)
        {
            return IconManager.GetIconLnk(path);
        }

        public static Bitmap GetIcon(string path)
        {
            var fullPath = path;
            if (PathHelper.IsURL(path))
                return Properties.Resources.wininet;

            if (!PathHelper.ExpandPath(ref fullPath))
                return null;
            var ext = Path.GetExtension(fullPath).ToLower();
            var attr = File.GetAttributes(fullPath);

            if (attr.HasFlag(FileAttributes.Directory))
                return Properties.Resources.folder;
            else if (ext == ".exe")
                return GetIconFromExe(fullPath);
            //else if (ext == ".lnk")
            //    return GetLnkIcon(path);
            else
                return IconManager.GetIconEx(fullPath);
        }

        public static Bitmap GetIconFromExe(string path)
        {
            try
            {
                var i = new MultiIcon();
                i.Load(path);
                if (i.Count < 1)
                    return null;
                var si = i[0];
                var seq = si.OrderBy(x => x.Size.Width);
                if (seq.Where(x => x.Size.Width >= Constants.ICONSIZE).Count() < 1)
                    return seq.Last().Transparent;
                var ii = seq.FirstOrDefault(x => x.Size.Width >= Constants.ICONSIZE);
                if (ii == null)
                    return null;
                return ii.Transparent;
            }
            catch (InvalidFileException)
            {
                return null;
            }
        }

        public static Bitmap ResizeTo(Bitmap src, int size = Constants.ICONSIZE)
        {
            var b = new Bitmap(size, size);
            using (var g = Graphics.FromImage(b))
            {
                if (size > src.Width && size % src.Width == 0)
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                else
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(src, new Rectangle(0, 0, size, size));
            }
            return b;
        }
    }
}
