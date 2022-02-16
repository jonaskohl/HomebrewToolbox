using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Drawing;
using System.IO;

namespace WiiBrewToolbox
{
    public static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursorFromFile(string path);

        public static Cursor LoadCustomCursorFromFile(string path)
        {
            IntPtr hCurs = LoadCursorFromFile(path);
            if (hCurs == IntPtr.Zero) throw new Win32Exception();
            var curs = new Cursor(hCurs);
            // Note: force the cursor to own the handle so it gets released properly
            var fi = typeof(Cursor).GetField("ownHandle", BindingFlags.NonPublic | BindingFlags.Instance);
            fi.SetValue(curs, true);
            return curs;
        }

        public static Cursor LoadCustomCursorFromFileWithUglyTempFile(Stream stream)
        {
            var tmpfile = Path.GetTempFileName();
            try
            {
                using (var hFile = File.OpenWrite(tmpfile))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(hFile);
                }
                var cur = LoadCustomCursorFromFile(tmpfile);
                return cur;
            }
            finally
            {
                File.Delete(tmpfile);
            }
        }
    }
}
