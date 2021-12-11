using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WiiBrewToolbox
{
    public partial class Form1 : Form
    {
        public event EventHandler<LoadingStateChangedEventArgs> LoadingStageChanged;

        public Form1()
        {
            InitializeComponent();
        }

        private string SavePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "entries.xml");

        private void SaveEntries()
        {
            var doc = new XDocument(
                new XElement(
                    "entries",
                    flowLayoutPanel1.Controls.OfType<Button>().Where(x => x != addButton && x != infoButton).Select(x =>
                    {
                        var dat = (ItmEntry)x.Tag;
                        return new XElement(
                            "entry",
                            new XAttribute("name", dat.Name),
                            new XAttribute("path", dat.Path),
                            new XAttribute("args", dat.Args)
                        );
                    })
                )
            );
            doc.Save(SavePath);
        }

        public void InitialLoad()
        {
            LoadEntries();
        }

        private void LoadEntries()
        {
            Stage("Loading saved entries...", 0);

            if (!File.Exists(SavePath))
                return;

            var doc = XDocument.Load(SavePath);
            flowLayoutPanel1.SuspendLayout();
            Stage("Loading icons...", 0);
            var els = doc.Root.Elements("entry");
            int current = 0, max = els.Count();
            foreach (var e in els)
            {
                ++current;
                Stage("Loading icons...", (ushort)(current / (double)max * ushort.MaxValue));
                var name = e.Attribute("name").Value;
                var path = e.Attribute("path").Value;
                var args = e.Attribute("args")?.Value ?? "";
                var b = MakeButton(name, path, args);
                flowLayoutPanel1.Controls.Add(b);
                flowLayoutPanel1.Controls.SetChildIndex(b, flowLayoutPanel1.Controls.Count - 4);
            }
            Stage("Finalizing...", ushort.MaxValue);
            flowLayoutPanel1.ResumeLayout();
            flowLayoutPanel1.PerformLayout();
            flowLayoutPanel1.Controls[0].Select();
        }

        private void Stage(string msg, ushort progress)
        {
            LoadingStageChanged?.Invoke(this, new LoadingStateChangedEventArgs() { Message = msg, Progress = progress });
        }

        private Button MakeButton(string text, string path, string args)
        {
            var image = IconHelper.GetIcon(path);
            if (image != null)
            {
                var t = IconHelper.ResizeTo(image);
                image.Dispose();
                image = t;
            }
            var b = new Button()
            {
                Text = text,
                Image = image ?? Properties.Resources.appgeneric,
                Size = addButton.Size,
                TextImageRelation = TextImageRelation.ImageAboveText,
                ContextMenuStrip = contextMenuStrip1,
                Tag = new ItmEntry()
                {
                    Name = text,
                    Path = path,
                    Args = args
                }
            };
            b.Click += B_Click;
            return b;
        }

        private void B_Click(object sender, EventArgs e)
        {
            var dat = (ItmEntry)(sender as Button).Tag;
            var path = dat.Path;
            if (PathHelper.IsURL(path))
            {
                var _psi = new ProcessStartInfo();
                _psi.FileName = "explorer";
                _psi.Arguments = "\"" + Regex.Replace(path, @"(\\+)$", @"$1$1") + "\"";
                _psi.UseShellExecute = true;
                Process.Start(_psi);
                return;
            }
            if (!PathHelper.ExpandPath(ref path))
            {
                MessageBox.Show("Could not find '" + path + "'!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var dir = Path.GetDirectoryName(path);
            var psi = new ProcessStartInfo();
            psi.WorkingDirectory = dir;
            psi.FileName = path;
            psi.Arguments = dat.Args;
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            using (var f = new ItemEditor())
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    if (f.ItmPath.Trim().Length < 1)
                    {
                        MessageBox.Show("Empty path! Entry won't be created!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var b = MakeButton(f.ItmName, f.ItmPath, f.ItmArgs);
                    flowLayoutPanel1.Controls.Add(b);
                    flowLayoutPanel1.Controls.SetChildIndex(b, flowLayoutPanel1.Controls.Count - 4);
                    SaveEntries();
                }
            }
        }

        private void removeEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    Control sourceControl = owner.SourceControl;

                    flowLayoutPanel1.Controls.Remove(sourceControl);
                    SaveEntries();
                }
            }
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    Control sourceControl = owner.SourceControl;

                    Edit(sourceControl as Button);
                }
            }
        }

        private void Edit(Button button)
        {
            using (var f = new ItemEditor(button))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    if (f.ItmPath.Trim().Length < 1)
                    {
                        MessageBox.Show("Empty path! Entry won't be changed!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    button.Text = f.ItmName;
                    button.Tag = new ItmEntry()
                    {
                        Name = f.ItmName,
                        Path = f.ItmPath,
                        Args = f.ItmArgs
                    };
                    var image = IconHelper.GetIcon(f.ItmPath);
                    if (image != null)
                    {
                        var t = IconHelper.ResizeTo(image);
                        image.Dispose();
                        image = t;
                    }
                    button.Image = image ?? Properties.Resources.appgeneric;
                    SaveEntries();
                }
            }
        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            using (var f = new AboutForm())
                f.ShowDialog(this);
        }
    }

    public class LoadingStateChangedEventArgs
    {
        public string Message;
        public ushort Progress;
    }

    struct ItmEntry
    {
        public string Name;
        public string Path;
        public string Args;
    }
}
