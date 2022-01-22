using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiiBrewToolbox
{
    public partial class ItemEditor : SkinnedForm
    {
        public string ItmName => nameTextBox.Text;
        public string ItmPath => pathComboBox.Text;
        public string ItmArgs => argumentsTextBox.Text;

        public ItemEditor()
        {
            InitializeComponent();
        }

        public ItemEditor(Button btn)
        {
            InitializeComponent();

            var dat = (ItmEntry)btn.Tag;
            nameTextBox.Text = dat.Name;
            pathComboBox.Text = dat.Path;
            argumentsTextBox.Text = dat.Args;

            argumentsLabel.Enabled =
                argumentsTextBox.Enabled = !PathHelper.IsURL(pathComboBox.Text);
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (ModifierKeys == Keys.Shift)
            {
                var d = new VistaFolderBrowserDialog();
                d.SelectedPath = pathComboBox.Text;
                if (d.ShowDialog(this))
                {
                    pathComboBox.Text = d.SelectedPath;
                    ApplyName();
                }
            }
            else
            {
                openFileDialog1.FileName = pathComboBox.Text;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    pathComboBox.Text = openFileDialog1.FileName;
                    ApplyName();
                }
            }
        }

        private void ApplyName()
        {
            try
            {

                if (nameTextBox.Text.Trim().Length >= 1)
                    return;


                if (PathHelper.IsURL(ItmPath))
                {
                    var wc = new WebClient();
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    wc.DownloadStringCompleted += Wc_DownloadStringCompleted;
                    wc.DownloadStringAsync(new Uri(ItmPath));
                    wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
                    loadingIcon.Show();
                    okButton.Enabled = false;
                    pathComboBox.Enabled = false;
                    browseButton.Enabled = false;
                    nameTextBox.Enabled = false;
                }
                else
                    nameTextBox.Text = Path.GetFileNameWithoutExtension(ItmPath);
            }
            catch { }
        }

        private void Wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            loadingIcon.Hide();
            okButton.Enabled = true;
            pathComboBox.Enabled = true;
            browseButton.Enabled = true;
            nameTextBox.Enabled = true;

            var matchSpecialized = Regex.Match(e.Result, @"<!--[\s\n]*de.jonaskohl.wii.toolbox.title[\s\n]*:[\s\n]*(.*?)[\s\n]*-->", RegexOptions.Singleline);
            if (matchSpecialized.Success)
            {
                var titleEscaped = matchSpecialized.Groups[1].Value;
                var titleUnescaped = WebUtility.HtmlDecode(titleEscaped);
                nameTextBox.Text = titleUnescaped;
                return;
            }

            var matchTitle = Regex.Match(e.Result, @"<[\s\n]*title[\s\n]*>(.*?)</[\s\n]*title[\s\n]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchTitle.Success)
            {
                var titleEscaped = matchTitle.Groups[1].Value;
                var titleUnescaped = WebUtility.HtmlDecode(titleEscaped);
                nameTextBox.Text = titleUnescaped;
            }
        }

        private void pathComboBox_Leave(object sender, EventArgs e)
        {
            if (
                pathComboBox.Text.StartsWith("\"") &&
                pathComboBox.Text.EndsWith("\"")
            )
                pathComboBox.Text = pathComboBox.Text.Substring(1, pathComboBox.Text.Length - 2);
            ApplyName();
            argumentsLabel.Enabled =
                argumentsTextBox.Enabled = !PathHelper.IsURL(pathComboBox.Text);
        }
    }
}
