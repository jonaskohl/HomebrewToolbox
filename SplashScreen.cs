using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiiBrewToolbox
{
    public partial class SplashScreen : Form
    {
        bool _allowClose = false;

        public SplashScreen()
        {
            InitializeComponent();

            progressBar1.Maximum = ushort.MaxValue;
        }

        public void SetStatus(string status)
        {
            label1.Text = status;
        }

        public void SetProgress(ushort progress)
        {
            progressBar1.Value = progress;
        }

        public void _Close()
        {
            _allowClose = true;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.UserClosing && !_allowClose)
                e.Cancel = true;
        }
    }
}
