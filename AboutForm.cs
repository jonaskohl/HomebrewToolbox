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
            sb.AppendLine();
            sb.AppendLine("Application icon from \"IconExperience O-Collection v2\" by Incors GmbH, used under license");
            sb.AppendLine();
            sb.AppendLine("Other icons from Microsoft\u00AE Windows\u00AE 10");

            scrollLabel1.Text = sb.ToString();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://wii.jonaskohl.de/tools/toolbox.html");
        }
    }
}
