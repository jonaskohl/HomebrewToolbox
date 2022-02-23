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
    public partial class Form1 : SkinnedForm
    {
        public delegate void LoadingStageChangedEventHandler(object sender, LoadingStateChangedEventArgs e);

        public event LoadingStageChangedEventHandler LoadingStageChanged;

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
                    flowLayoutPanel1.Controls.OfType<Button>().Where(x => x != addButton && x != infoButton).Where(x => x != null && x.Tag != null).Select(x =>
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
            Stage("Loading settings...", 0);
            SettingsManager.LoadSettings();

            var skin = SettingsManager.Get("skinFile");

            if (skin != null && SkinManager.SkinExists(skin))
            {
                Stage("Loading skin...", 0);
                try
                {
                    SkinManager.LoadSkin(skin + ".wtbskin");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed to load skin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            LoadEntries();

            ApplySkin();
        }

        public override void ApplySkin()
        {
            addButton.Image = SkinManager.AddImage;
            infoButton.Image = SkinManager.AboutImage;
            settingsButton.Image = SkinManager.SettingsImage;

            Icon = SkinManager.AppIcon ?? Properties.Resources.toolbox1;

            base.ApplySkin();
        }

        public void ReloadEntries()
        {
            flowLayoutPanel1.SuspendLayout();
            for (var i = flowLayoutPanel1.Controls.Count - 1; i >= 0; --i)
            {
                var ctrl = flowLayoutPanel1.Controls[i];
                if (ctrl == addButton || ctrl == infoButton || ctrl == settingsButton || ctrl == spacer)
                    continue;

                flowLayoutPanel1.Controls.Remove(ctrl);
            }
            flowLayoutPanel1.ResumeLayout();
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
                Image img = null;

                if (e.Attribute("image")?.Value != null)
                    img = Image.FromFile(e.Attribute("image").Value);

                var b = MakeButton(name, path, args, img);
                flowLayoutPanel1.Controls.Add(b);
                flowLayoutPanel1.Controls.SetChildIndex(b, flowLayoutPanel1.Controls.Count - 5);
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

        public ImageIconInfo GetImageIconInfo(string path, Image ovrImage = null)
        {
            ImageIconInfo imInfo;

            if (ovrImage != null)
                imInfo = IconHelper.InfoFromImage(ovrImage);
            else
            {
                imInfo = IconHelper.GetIcon(path);
                if (imInfo.Image != null && !imInfo.IsBuiltinImage)
                {
                    var t = IconHelper.ResizeTo(imInfo.Image);
                    imInfo.Image.Dispose();
                    imInfo.Image = t;
                }
            }

            return imInfo;
        }

        private Button MakeButton(string text, string path, string args, Image ovrImage = null)
        {
            var imInfo = GetImageIconInfo(path, ovrImage);
            var b = new SkinnedButton()
            {
                Text = text,
                Image = imInfo.Image ?? SkinManager.NoAppIconImage,
                Size = addButton.Size,
                TextImageRelation = TextImageRelation.ImageAboveText,
                ContextMenuStrip = contextMenuStrip1,
                Tag = new ItmEntry()
                {
                    Name = text,
                    Path = path,
                    Args = args,
                    ImInfo = imInfo
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
                    flowLayoutPanel1.Controls.SetChildIndex(b, flowLayoutPanel1.Controls.Count - 5);
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
            var prevEntry = (ItmEntry)button.Tag;

            using (var f = new ItemEditor(button))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    if (f.ItmPath.Trim().Length < 1)
                    {
                        MessageBox.Show("Empty path! Entry won't be changed!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var newEntry = new ItmEntry()
                    {
                        Name = f.ItmName,
                        Path = f.ItmPath,
                        Args = f.ItmArgs,
                        ImInfo = prevEntry.ImInfo
                    };

                    button.Text = f.ItmName;

                    if (!prevEntry.ImInfo.IsOverwriteImage)
                        newEntry.ImInfo = GetImageIconInfo(f.ItmPath);

                    button.Tag = newEntry;
                    SaveEntries();
                }
            }
        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            using (var f = new AboutForm())
                f.ShowDialog(this);
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            using (var f = new SettingsForm())
            {
                f.ShowDialog(this);
            }
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
        public ImageIconInfo ImInfo;
    }
}
