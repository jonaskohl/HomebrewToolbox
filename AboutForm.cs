using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiiBrewToolbox
{
    public partial class AboutForm : SkinnedForm
    {
        public AboutForm()
        {
            InitializeComponent();

            var sb = new StringBuilder();

            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            sb.AppendLine("Version " + fvi.ProductVersion);
            sb.AppendLine(fvi.LegalCopyright);
            sb.AppendLine();
            sb.AppendLine("OS version: " + Environment.OSVersion.VersionString);
            sb.AppendLine("Runtime version: " + Environment.Version);
            sb.AppendLine("DotNetZip version: " + typeof(Ionic.Zip.ZipFile).Assembly.GetName().Version);
            sb.AppendLine();
            sb.AppendLine("Application icon from \"IconExperience O-Collection v2\" by Incors GmbH, used under license");
            //sb.AppendLine();
            //sb.AppendLine("Other icons from Microsoft\u00AE Windows\u00AE 10");

            scrollLabel1.Text = sb.ToString();

            ApplySkinInternal();
        }

        public override void ApplySkin()
        {
            base.ApplySkin();

            ApplySkinInternal();
        }

        private void ApplySkinInternal()
        {
            pictureBox1.Image = SkinManager.UseWhiteForegrounds ? Properties.Resources.toolbox_w : Properties.Resources.toolbox;
            shadowLabel1.ForeColor = SkinManager.UseWhiteForegrounds ? Color.White : Color.FromArgb(62, 155, 213);
            shadowLabel1.ShadowColor = SkinManager.UseWhiteForegrounds ? Color.Black : Color.LightGray;
            linkLabel1.LinkColor = SkinManager.UseWhiteForegrounds ? Color.White : SystemColors.HotTrack;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://wii.jonaskohl.de/tools/toolbox.html");
        }
    }
}
