using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiiBrewToolbox
{
    static class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private const int IDC_HAND = 32649;
        private static Cursor SystemHandCursor;

        private static void ApplyHandCursorFix()
        {
            try
            {
                SystemHandCursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));

                typeof(Cursors).GetField("hand", BindingFlags.Static | BindingFlags.NonPublic)
                               .SetValue(null, SystemHandCursor);
            }
            catch { }
        }

        static Form1 mainForm;
        static SplashScreen splashScreen;
        static ApplicationContext context;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ApplyHandCursorFix();

            splashScreen = new SplashScreen();

            context = new ApplicationContext();

            Application.Idle += Application_Idle;
            splashScreen.Show();

            Application.Run(context);
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            if (context.MainForm == null)
            {
                Application.Idle -= Application_Idle;

                mainForm = new Form1();
                mainForm.LoadingStageChanged += MainForm_LoadingStageChanged;
                mainForm.InitialLoad();
                context.MainForm = mainForm;
                context.MainForm.Show();

                splashScreen._Close();
            }
        }

        private static void MainForm_LoadingStageChanged(object sender, LoadingStateChangedEventArgs e)
        {
            splashScreen.SetStatus(e.Message);
            splashScreen.SetProgress(e.Progress);
            Application.DoEvents();
        }
    }
}
