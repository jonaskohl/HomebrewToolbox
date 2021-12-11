using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.IconLib;
using System.Drawing.IconLib.Exceptions;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace WiiBrewToolbox
{
    /// <summary>
    /// Internals are mostly from here: http://www.codeproject.com/Articles/2532/Obtaining-and-managing-file-and-folder-icons-using
    /// Caches all results.
    /// </summary>
    public static class IconManager
    {
        private static readonly Dictionary<string, Bitmap> _smallIconCache = new Dictionary<string, Bitmap>();
        private static readonly Dictionary<string, Bitmap> _largeIconCache = new Dictionary<string, Bitmap>();
        /// <summary>
        /// Get an icon for a given filename
        /// </summary>
        /// <param name="fileName">any filename</param>
        /// <param name="large">16x16 or 32x32 icon</param>
        /// <returns>null if path is null, otherwise - an icon</returns>
        public static Bitmap FindIconForFilename(string fileName, bool large)
        {
            var extension = Path.GetExtension(fileName);
            if (extension == null)
                return null;
            var cache = large ? _largeIconCache : _smallIconCache;
            Bitmap icon;
            if (cache.TryGetValue(extension, out icon))
                return icon;
            icon = ToBitmap(IconReader.GetFileIcon(fileName, large ? IconReader.IconSize.Large : IconReader.IconSize.Small, false));
            cache.Add(extension, icon);
            return icon;
        }

        public static Bitmap GetIconLnk(string path)
        {
            var inf = new Shell32.Shfileinfo();
            Shell32.SHGetFileInfo(path, 0, ref inf, (uint)Marshal.SizeOf(inf), Shell32.ShgfiIconLocation);
            var file = inf.szDisplayName;
            Debug.Assert(file != "");
            var index = inf.hIcon.ToInt32();

            try
            {
                var i = new MultiIcon();
                i.Load(file);
                if (i.Count < 1)
                    return null;
                var si = i[index];
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

        /// <summary>
        /// http://stackoverflow.com/a/6580799/1943849
        /// </summary>
        static Bitmap ToBitmap(this Icon icon)
        {
            //var Bitmap = Imaging.CreateBitmapSourceFromHIcon(
            //    icon.Handle,
            //    Int32Rect.Empty,
            //    BitmapSizeOptions.FromEmptyOptions());
            //return Bitmap;
            //return HBitmapHelper.GetBitmapFromHBitmap(icon.Handle);
            var f = Path.GetTempFileName() + ".ico";
            using (var s = File.OpenWrite(f))
            {
                icon.Save(s);
                s.Close();
            }
            var b = IconHelper.GetIconFromExe(f);
            File.Delete(f);
            return b;
        }

        static int GetIconIndex(string pszFile)
        {
            var sfi = new Shell32.Shfileinfo();
            Shell32.SHGetFileInfo(pszFile
                , 0
                , ref sfi
                , (uint)Marshal.SizeOf(sfi)
                , Shell32.ShgfiSysiconIndex| Shell32.ShgfiLargeicon | Shell32.ShgfiUsefileattributes );
            return sfi.iIcon;
        }

        static IntPtr GetJumboIcon(int iImage)
        {
            Shell32.IImageList spiml = null;
            Guid guil = new Guid(Shell32.IID_IImageList2);//or IID_IImageList

            Shell32.SHGetImageList(Shell32.SHIL_EXTRALARGE, ref guil, ref spiml);
            IntPtr hIcon = IntPtr.Zero;
            spiml.GetIcon(iImage, Shell32.ILD_TRANSPARENT | Shell32.ILD_IMAGE, ref hIcon); //

            return hIcon;
        }

        public static Bitmap GetIconEx(string name)
        {
            var index = GetIconIndex("*" + Path.GetExtension(name));
            var hIcon = GetJumboIcon(index);

            Bitmap b;
            using (var ico = (Icon)Icon.FromHandle(hIcon).Clone())
            {
                // save to file (or show in a picture box)
                b = ico.ToBitmap();
            }
            User32.DestroyIcon(hIcon); // don't forget to cleanup
            return b;
        }

        /// <summary>
        /// Provides static methods to read system icons for both folders and files.
        /// </summary>
        /// <example>
        /// <code>IconReader.GetFileIcon("c:\\general.xls");</code>
        /// </example>
        static class IconReader
        {
            /// <summary>
            /// Options to specify the size of icons to return.
            /// </summary>
            public enum IconSize
            {
                /// <summary>
                /// Specify large icon - 32 pixels by 32 pixels.
                /// </summary>
                Large = 0,
                /// <summary>
                /// Specify small icon - 16 pixels by 16 pixels.
                /// </summary>
                Small = 1
            }
            /// <summary>
            /// Returns an icon for a given file - indicated by the name parameter.
            /// </summary>
            /// <param name="name">Pathname for file.</param>
            /// <param name="size">Large or small</param>
            /// <param name="linkOverlay">Whether to include the link icon</param>
            /// <returns>System.Drawing.Icon</returns>
            public static Icon GetFileIcon(string name, IconSize size, bool linkOverlay)
            {
                var shfi = new Shell32.Shfileinfo();
                var flags = Shell32.ShgfiIcon | Shell32.ShgfiUsefileattributes;
                if (linkOverlay) flags += Shell32.ShgfiLinkoverlay;
                /* Check the size specified for return. */
                if (IconSize.Small == size)
                    flags += Shell32.ShgfiSmallicon;
                else
                    flags += Shell32.ShgfiLargeicon;
                Shell32.SHGetFileInfo(name,
                    Shell32.FileAttributeNormal,
                    ref shfi,
                    (uint)Marshal.SizeOf(shfi),
                    flags);
                // Copy (clone) the returned icon to a new object, thus allowing us to clean-up properly
                var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
                User32.DestroyIcon(shfi.hIcon);     // Cleanup
                return icon;
            }
        }
        /// <summary>
        /// Wraps necessary Shell32.dll structures and functions required to retrieve Icon Handles using SHGetFileInfo. Code
        /// courtesy of MSDN Cold Rooster Consulting case study.
        /// </summary>
        static class Shell32
        {
            public const string IID_IImageList = "46EB5926-582E-4017-9FDF-E8998DAA0950";
            public const string IID_IImageList2 = "192B9D83-50FC-457B-90A0-2B82A8B5DAE1";

            public const int SHIL_LARGE = 0x0;
            public const int SHIL_SMALL = 0x1;
            public const int SHIL_EXTRALARGE = 0x2;
            public const int SHIL_SYSSMALL = 0x3;
            public const int SHIL_JUMBO = 0x4;
            public const int SHIL_LAST = 0x4;

            public const int ILD_TRANSPARENT = 0x00000001;
            public const int ILD_IMAGE = 0x00000020;

            [DllImport("shell32.dll", EntryPoint = "#727")]
            public extern static int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv);

            [DllImport("shell32.dll")]
            public static extern uint SHGetIDListFromObject([MarshalAs(UnmanagedType.IUnknown)] object iUnknown, out IntPtr ppidl);

            [DllImport("Shell32.dll")]
            public static extern IntPtr SHGetFileInfo(
                string pszPath,
                uint dwFileAttributes,
                ref Shfileinfo psfi,
                uint cbFileInfo,
                uint uFlags
            );

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            public static extern uint ExtractIconEx(string szFileName, int nIconIndex,
                IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

            private const int MaxPath = 256;
            [StructLayout(LayoutKind.Sequential)]
            public struct Shfileinfo
            {
                public const int Namesize = 80;
                public readonly IntPtr hIcon;
                public readonly int iIcon;
                public readonly uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxPath)]
                public readonly string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Namesize)]
                public readonly string szTypeName;
            };
            public const uint ShgfiSysiconIndex = 0x000004000;
            public const uint ShgfiIcon = 0x000000100;     // get icon
            public const uint ShgfiLinkoverlay = 0x000008000;     // put a link overlay on icon
            public const uint ShgfiLargeicon = 0x000000000;     // get large icon
            public const uint ShgfiSmallicon = 0x000000001;     // get small icon
            public const uint ShgfiUsefileattributes = 0x000000010;     // use passed dwFileAttribute
            public const uint FileAttributeNormal = 0x00000080;
            public const uint ShgfiIconLocation = 0x000001000;

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left, top, right, bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                int x;
                int y;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct IMAGELISTDRAWPARAMS
            {
                public int cbSize;
                public IntPtr himl;
                public int i;
                public IntPtr hdcDst;
                public int x;
                public int y;
                public int cx;
                public int cy;
                public int xBitmap;    // x offest from the upperleft of bitmap
                public int yBitmap;    // y offset from the upperleft of bitmap
                public int rgbBk;
                public int rgbFg;
                public int fStyle;
                public int dwRop;
                public int fState;
                public int Frame;
                public int crEffect;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct IMAGEINFO
            {
                public IntPtr hbmImage;
                public IntPtr hbmMask;
                public int Unused1;
                public int Unused2;
                public RECT rcImage;
            }
            [ComImportAttribute()]
            [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
            [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IImageList
            {
                [PreserveSig]
                int Add(
                IntPtr hbmImage,
                IntPtr hbmMask,
                ref int pi);

                [PreserveSig]
                int ReplaceIcon(
                int i,
                IntPtr hicon,
                ref int pi);

                [PreserveSig]
                int SetOverlayImage(
                int iImage,
                int iOverlay);

                [PreserveSig]
                int Replace(
                int i,
                IntPtr hbmImage,
                IntPtr hbmMask);

                [PreserveSig]
                int AddMasked(
                IntPtr hbmImage,
                int crMask,
                ref int pi);

                [PreserveSig]
                int Draw(
                ref IMAGELISTDRAWPARAMS pimldp);

                [PreserveSig]
                int Remove(
                int i);

                [PreserveSig]
                int GetIcon(
                int i,
                int flags,
                ref IntPtr picon);

                [PreserveSig]
                int GetImageInfo(
                int i,
                ref IMAGEINFO pImageInfo);

                [PreserveSig]
                int Copy(
                int iDst,
                IImageList punkSrc,
                int iSrc,
                int uFlags);

                [PreserveSig]
                int Merge(
                int i1,
                IImageList punk2,
                int i2,
                int dx,
                int dy,
                ref Guid riid,
                ref IntPtr ppv);

                [PreserveSig]
                int Clone(
                ref Guid riid,
                ref IntPtr ppv);

                [PreserveSig]
                int GetImageRect(
                int i,
                ref RECT prc);

                [PreserveSig]
                int GetIconSize(
                ref int cx,
                ref int cy);

                [PreserveSig]
                int SetIconSize(
                int cx,
                int cy);

                [PreserveSig]
                int GetImageCount(
                ref int pi);

                [PreserveSig]
                int SetImageCount(
                int uNewCount);

                [PreserveSig]
                int SetBkColor(
                int clrBk,
                ref int pclr);

                [PreserveSig]
                int GetBkColor(
                ref int pclr);

                [PreserveSig]
                int BeginDrag(
                int iTrack,
                int dxHotspot,
                int dyHotspot);

                [PreserveSig]
                int EndDrag();

                [PreserveSig]
                int DragEnter(
                IntPtr hwndLock,
                int x,
                int y);

                [PreserveSig]
                int DragLeave(
                IntPtr hwndLock);

                [PreserveSig]
                int DragMove(
                int x,
                int y);

                [PreserveSig]
                int SetDragCursorImage(
                ref IImageList punk,
                int iDrag,
                int dxHotspot,
                int dyHotspot);

                [PreserveSig]
                int DragShowNolock(
                int fShow);

                [PreserveSig]
                int GetDragImage(
                ref POINT ppt,
                ref POINT pptHotspot,
                ref Guid riid,
                ref IntPtr ppv);

                [PreserveSig]
                int GetItemFlags(
                int i,
                ref int dwFlags);

                [PreserveSig]
                int GetOverlayImage(
                int iOverlay,
                ref int piIndex);
            };
        }
        /// <summary>
        /// Wraps necessary functions imported from User32.dll. Code courtesy of MSDN Cold Rooster Consulting example.
        /// </summary>
        static class User32
        {
            /// <summary>
            /// Provides access to function required to delete handle. This method is used internally
            /// and is not required to be called separately.
            /// </summary>
            /// <param name="hIcon">Pointer to icon handle.</param>
            /// <returns>N/A</returns>
            [DllImport("User32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);

            [StructLayout(LayoutKind.Sequential)]
            public struct ICONINFO
            {
                public bool fIcon;         // Specifies whether this structure defines an icon or a cursor. A value of TRUE specifies
                                           // an icon; FALSE specifies a cursor.
                public Int32 xHotspot;     // Specifies the x-coordinate of a cursor's hot spot. If this structure defines an icon, the hot
                                           // spot is always in the center of the icon, and this member is ignored.
                public Int32 yHotspot;     // Specifies the y-coordinate of the cursor's hot spot. If this structure defines an icon, the hot
                                           // spot is always in the center of the icon, and this member is ignored.
                public IntPtr hbmMask;     // (HBITMAP) Specifies the icon bitmask bitmap. If this structure defines a black and white icon,
                                           // this bitmask is formatted so that the upper half is the icon AND bitmask and the lower half is
                                           // the icon XOR bitmask. Under this condition, the height should be an even multiple of two. If
                                           // this structure defines a color icon, this mask only defines the AND bitmask of the icon.
                public IntPtr hbmColor;    // (HBITMAP) Handle to the icon color bitmap. This member can be optional if this
                                           // structure defines a black and white icon. The AND bitmask of hbmMask is applied with the SRCAND
                                           // flag to the destination; subsequently, the color bitmap is applied (using XOR) to the
                                           // destination by using the SRCINVERT flag.
            }

            [DllImport("user32.dll")]
            public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);
        }
    }
}