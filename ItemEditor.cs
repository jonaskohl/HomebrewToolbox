using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiiBrewToolbox
{
    public partial class ItemEditor : Form
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
                if (nameTextBox.Text.Trim().Length < 1)
                    nameTextBox.Text = Path.GetFileNameWithoutExtension(ItmPath);
            }
            catch { }
        }

        private void pathComboBox_Leave(object sender, EventArgs e)
        {
            if (
                pathComboBox.Text.StartsWith("\"") &&
                pathComboBox.Text.EndsWith("\"")
            )
                pathComboBox.Text = pathComboBox.Text.Substring(1, pathComboBox.Text.Length - 2);
            ApplyName();
        }
    }
}
