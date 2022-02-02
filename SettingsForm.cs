using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiiBrewToolbox
{
    public partial class SettingsForm : SkinnedForm
    {
        bool skinChanged = false;

        List<SkinInfo> sourceList;
        int lastIndex = 0;

        public SettingsForm()
        {
            InitializeComponent();

            var skins = SkinManager.GetAvailableSkins();

            sourceList = new List<SkinInfo>();

            sourceList.Add(new SkinInfo() { Filename = null, DisplayName = "(None)", Meta = SkinInfoMeta.InternalSkin });
            sourceList.AddRange(skins);
            sourceList.Add(new SkinInfo() { Filename = null, DisplayName = null, Meta = SkinInfoMeta.NoAction });
            sourceList.Add(new SkinInfo() { Filename = null, DisplayName = "Download more skins...", Meta = SkinInfoMeta.GetSkins });

            skinComboBox.DataSource = sourceList;
            skinComboBox.DisplayMember = "DisplayName";

            lastIndex =
            skinComboBox.SelectedIndex = Array.IndexOf(skins.Select(x => x.Filename).ToArray(), SettingsManager.Get("skinFile")) + 1;

            skinComboBox.SelectedIndexChanged += SkinComboBox_SelectedIndexChanged;
        }

        private void SkinComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            skinChanged = true;

            var skin = sourceList[skinComboBox.SelectedIndex];

            if (skin.Meta == SkinInfoMeta.GetSkins)
            {
                Process.Start("https://wii.jonaskohl.de/r/wtbskins");
                skinComboBox.SelectedIndex = lastIndex;
            }
            else if (skin.Meta == SkinInfoMeta.NoAction)
            {
                skinComboBox.SelectedIndex = lastIndex;
            }
            else
                lastIndex = skinComboBox.SelectedIndex;
        }

        private void skinnedButton1_Click(object sender, EventArgs e)
        {
            if (skinChanged)
            {
                var skin = sourceList[skinComboBox.SelectedIndex];

                if (skin.Meta == SkinInfoMeta.InternalSkin)
                {
                    SkinManager.ResetSkin();
                    SettingsManager.Set("skinFile", null);
                }
                else if (skin.Meta == SkinInfoMeta.None)
                {
                    var skinFilename = skin.Filename;
                    SkinManager.LoadSkin(skinFilename + ".wtbskin");
                    SettingsManager.Set("skinFile", skinFilename);
                }

                foreach (SkinnedForm form in Application.OpenForms.OfType<SkinnedForm>())
                {
                    form.Invalidate();
                    form.ApplySkin();
                }

                foreach (Form1 form1 in Application.OpenForms.OfType<Form1>())
                    form1.ReloadEntries();
            }

            SettingsManager.SaveSettings();
        }

        private void skinComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            var skin = sourceList[e.Index];


            if (skin.Meta == SkinInfoMeta.NoAction)
            {
                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
                e.Graphics.DrawLine(SystemPens.GrayText,
                    new Point(e.Bounds.X, e.Bounds.Y + e.Bounds.Height / 2),
                    new Point(e.Bounds.Right, e.Bounds.Y + e.Bounds.Height / 2)
                );
            }
            else
            {
                e.DrawBackground();
                var style = e.Font.Style;
                if (skin.Meta == SkinInfoMeta.GetSkins)
                    style |= FontStyle.Bold;
                else if (skin.Meta == SkinInfoMeta.InternalSkin)
                    style |= FontStyle.Italic;
                using (var f = new Font(e.Font, style))
                    TextRenderer.DrawText(e.Graphics, skin.DisplayName, f, e.Bounds, e.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                e.DrawFocusRectangle();
            }
        }

        private void skinComboBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemWidth = skinComboBox.Width;

            var skin = sourceList[e.Index];

            int h;
            if (skin.Meta == SkinInfoMeta.NoAction)
                h = 3;
            else
                h = TextRenderer.MeasureText(e.Graphics, skin.DisplayName, skinComboBox.Font).Height + 4;

            e.ItemHeight = h;
        }
    }
}
