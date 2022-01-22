
namespace WiiBrewToolbox
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.addButton = new WiiBrewToolbox.SkinnedButton();
            this.infoButton = new WiiBrewToolbox.SkinnedButton();
            this.settingsButton = new WiiBrewToolbox.SkinnedButton();
            this.spacer = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeEntryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spacer)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanel1.Controls.Add(this.addButton);
            this.flowLayoutPanel1.Controls.Add(this.infoButton);
            this.flowLayoutPanel1.Controls.Add(this.settingsButton);
            this.flowLayoutPanel1.Controls.Add(this.spacer);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(12);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(452, 122);
            this.flowLayoutPanel1.TabIndex = 0;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // addButton
            // 
            this.addButton.Image = global::WiiBrewToolbox.Properties.Resources.winadd;
            this.addButton.Location = new System.Drawing.Point(15, 15);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(78, 78);
            this.addButton.TabIndex = 0;
            this.addButton.Text = "&Add...";
            this.addButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.addButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // infoButton
            // 
            this.infoButton.Image = global::WiiBrewToolbox.Properties.Resources.wininfo;
            this.infoButton.Location = new System.Drawing.Point(99, 15);
            this.infoButton.Name = "infoButton";
            this.infoButton.Size = new System.Drawing.Size(78, 78);
            this.infoButton.TabIndex = 1;
            this.infoButton.Text = "A&bout...";
            this.infoButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.infoButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.infoButton.UseVisualStyleBackColor = true;
            this.infoButton.Click += new System.EventHandler(this.infoButton_Click);
            // 
            // settingsButton
            // 
            this.settingsButton.Image = global::WiiBrewToolbox.Properties.Resources.settings;
            this.settingsButton.Location = new System.Drawing.Point(183, 15);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(78, 78);
            this.settingsButton.TabIndex = 3;
            this.settingsButton.Text = "&Settings...";
            this.settingsButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.settingsButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.settingsButton.UseVisualStyleBackColor = true;
            this.settingsButton.Click += new System.EventHandler(this.settingsButton_Click);
            // 
            // spacer
            // 
            this.spacer.Location = new System.Drawing.Point(267, 15);
            this.spacer.Name = "spacer";
            this.spacer.Size = new System.Drawing.Size(12, 78);
            this.spacer.TabIndex = 2;
            this.spacer.TabStop = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.removeEntryToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(148, 48);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Image = global::WiiBrewToolbox.Properties.Resources.pencil;
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.editToolStripMenuItem.Text = "&Edit...";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // removeEntryToolStripMenuItem
            // 
            this.removeEntryToolStripMenuItem.Image = global::WiiBrewToolbox.Properties.Resources.delete;
            this.removeEntryToolStripMenuItem.Name = "removeEntryToolStripMenuItem";
            this.removeEntryToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.removeEntryToolStripMenuItem.Text = "&Remove entry";
            this.removeEntryToolStripMenuItem.Click += new System.EventHandler(this.removeEntryToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(452, 122);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Homebrew Toolbox";
            this.flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spacer)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private WiiBrewToolbox.SkinnedButton addButton;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem removeEntryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private WiiBrewToolbox.SkinnedButton infoButton;
        private System.Windows.Forms.PictureBox spacer;
        private SkinnedButton settingsButton;
    }
}

