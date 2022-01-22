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
    public partial class SettingsForm : SkinnedForm
    {
        bool skinChanged = false;

        List<SkinInfo> sourceList;

        public SettingsForm()
        {
            InitializeComponent();

            var skins = SkinManager.GetAvailableSkins();

            sourceList = new List<SkinInfo>();

            sourceList.Add(new SkinInfo() { Filename = null, DisplayName = "(None)" });
            sourceList.AddRange(skins);

            skinComboBox.DataSource = sourceList;
            skinComboBox.DisplayMember = "DisplayName";

            skinComboBox.SelectedIndex = Array.IndexOf(skins.Select(x => x.Filename).ToArray(), SettingsManager.Get("skinFile")) + 1;

            skinComboBox.SelectedIndexChanged += SkinComboBox_SelectedIndexChanged;
        }

        private void SkinComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            skinChanged = true;
        }

        private void skinnedButton1_Click(object sender, EventArgs e)
        {
            if (skinChanged)
            {
                if (skinComboBox.SelectedIndex == 0)
                {
                    SkinManager.ResetSkin();
                    SettingsManager.Set("skinFile", null);
                }
                else
                {
                    var skin = sourceList[skinComboBox.SelectedIndex].Filename;
                    SkinManager.LoadSkin(skin + ".wtbskin");
                    SettingsManager.Set("skinFile", skin);
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
    }
}
