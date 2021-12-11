using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiiBrewToolbox
{
    public static class PathHelper
    {
        public static bool IsURL(string p)
        {
            return p.StartsWith("http://") || p.StartsWith("https://") || p.StartsWith("ftp://");
        }

        public static bool IsLocalPath(string p)
        {
            if (!Uri.TryCreate(p, UriKind.Absolute, out Uri uri))
                return false;
            return uri.IsFile;
        }

        public static bool ExpandPath(ref string fileName)
        {
            if (Directory.Exists(fileName) || File.Exists(fileName))
            {
                fileName = Path.GetFullPath(fileName);
                return true;
            }

            var values = Environment.GetEnvironmentVariable("PATH");
            var exts = Environment.GetEnvironmentVariable("PATHEXT");
            foreach (var path in values.Split(Path.PathSeparator))
            {
                foreach (var ext in exts.Split(Path.PathSeparator))
                {
                    var fullPath = Path.Combine(path, fileName + ext);
                    if (File.Exists(fullPath))
                    {
                        fileName = fullPath;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
