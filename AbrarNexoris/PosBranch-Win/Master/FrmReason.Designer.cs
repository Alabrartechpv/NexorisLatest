namespace PosBranch_Win.Master
{
    partial class FrmReason
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblReason;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtReasonName;
        private System.Windows.Forms.Label lblNature;
        private System.Windows.Forms.ComboBox cmbReasonType;
        private System.Windows.Forms.Panel panelMain;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmReason));
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance6 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance7 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance8 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance9 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance10 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance11 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance12 = new Infragistics.Win.Appearance();
            this.lblReason = new System.Windows.Forms.Label();
            this.txtReasonName = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblNature = new System.Windows.Forms.Label();
            this.cmbReasonType = new System.Windows.Forms.ComboBox();
            this.panelMain = new System.Windows.Forms.Panel();
            this.ultraPanel3 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPictureBox2 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.ultraPanel9 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPictureBox4 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.ultraPanel10 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPictureBox5 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.ultraPanel8 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPictureBox6 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.btnLookupF7 = new Infragistics.Win.Misc.UltraButton();
            ((System.ComponentModel.ISupportInitialize)(this.txtReasonName)).BeginInit();
            this.panelMain.SuspendLayout();
            this.ultraPanel3.ClientArea.SuspendLayout();
            this.ultraPanel3.SuspendLayout();
            this.ultraPanel9.ClientArea.SuspendLayout();
            this.ultraPanel9.SuspendLayout();
            this.ultraPanel10.ClientArea.SuspendLayout();
            this.ultraPanel10.SuspendLayout();
            this.ultraPanel8.ClientArea.SuspendLayout();
            this.ultraPanel8.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblReason
            // 
            this.lblReason.AutoSize = true;
            this.lblReason.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReason.Location = new System.Drawing.Point(50, 45);
            this.lblReason.Name = "lblReason";
            this.lblReason.Size = new System.Drawing.Size(64, 20);
            this.lblReason.TabIndex = 0;
            this.lblReason.Text = "Reason ";
            // 
            // txtReasonName
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(228)))), ((int)(((byte)(196)))));
            this.txtReasonName.Appearance = appearance1;
            this.txtReasonName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(228)))), ((int)(((byte)(196)))));
            this.txtReasonName.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtReasonName.Location = new System.Drawing.Point(135, 42);
            this.txtReasonName.Name = "txtReasonName";
            this.txtReasonName.Size = new System.Drawing.Size(230, 29);
            this.txtReasonName.TabIndex = 1;
            // 
            // lblNature
            // 
            this.lblNature.AutoSize = true;
            this.lblNature.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNature.Location = new System.Drawing.Point(50, 95);
            this.lblNature.Name = "lblNature";
            this.lblNature.Size = new System.Drawing.Size(62, 20);
            this.lblNature.TabIndex = 7;
            this.lblNature.Text = "Nature ";
            // 
            // cmbReasonType
            // 
            this.cmbReasonType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbReasonType.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbReasonType.FormattingEnabled = true;
            this.cmbReasonType.Location = new System.Drawing.Point(135, 92);
            this.cmbReasonType.Name = "cmbReasonType";
            this.cmbReasonType.Size = new System.Drawing.Size(444, 25);
            this.cmbReasonType.TabIndex = 8;
            // 
            // panelMain
            // 
            this.panelMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(236)))), ((int)(((byte)(255)))));
            this.panelMain.Controls.Add(this.ultraPanel3);
            this.panelMain.Controls.Add(this.ultraPanel9);
            this.panelMain.Controls.Add(this.ultraPanel10);
            this.panelMain.Controls.Add(this.ultraPanel8);
            this.panelMain.Controls.Add(this.btnLookupF7);
            this.panelMain.Controls.Add(this.lblReason);
            this.panelMain.Controls.Add(this.txtReasonName);
            this.panelMain.Controls.Add(this.lblNature);
            this.panelMain.Controls.Add(this.cmbReasonType);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(850, 480);
            this.panelMain.TabIndex = 0;
            // 
            // ultraPanel3
            // 
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            appearance2.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(202)))), ((int)(((byte)(245)))));
            appearance2.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(150)))), ((int)(((byte)(215)))));
            this.ultraPanel3.Appearance = appearance2;
            this.ultraPanel3.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanel3.ClientArea
            // 
            this.ultraPanel3.ClientArea.Controls.Add(this.ultraPictureBox2);
            this.ultraPanel3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPanel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(35)))), ((int)(((byte)(80)))));
            this.ultraPanel3.Location = new System.Drawing.Point(414, 42);
            this.ultraPanel3.Name = "ultraPanel3";
            this.ultraPanel3.Size = new System.Drawing.Size(32, 29);
            this.ultraPanel3.TabIndex = 53;
            this.ultraPanel3.UseAppStyling = false;
            this.ultraPanel3.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraPictureBox2
            // 
            appearance3.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            appearance3.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance3.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.ultraPictureBox2.Appearance = appearance3;
            this.ultraPictureBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ultraPictureBox2.BackColorInternal = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ultraPictureBox2.BorderShadowColor = System.Drawing.Color.Empty;
            this.ultraPictureBox2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox2.Image = ((object)(resources.GetObject("ultraPictureBox2.Image")));
            this.ultraPictureBox2.Location = new System.Drawing.Point(6, 4);
            this.ultraPictureBox2.Name = "ultraPictureBox2";
            this.ultraPictureBox2.Size = new System.Drawing.Size(14, 17);
            this.ultraPictureBox2.TabIndex = 193;
            // 
            // ultraPanel9
            // 
            appearance4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            appearance4.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(202)))), ((int)(((byte)(245)))));
            appearance4.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance4.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(150)))), ((int)(((byte)(215)))));
            this.ultraPanel9.Appearance = appearance4;
            this.ultraPanel9.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanel9.ClientArea
            // 
            this.ultraPanel9.ClientArea.Controls.Add(this.ultraPictureBox4);
            this.ultraPanel9.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPanel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(35)))), ((int)(((byte)(80)))));
            this.ultraPanel9.Location = new System.Drawing.Point(447, 42);
            this.ultraPanel9.Name = "ultraPanel9";
            this.ultraPanel9.Size = new System.Drawing.Size(32, 29);
            this.ultraPanel9.TabIndex = 54;
            this.ultraPanel9.UseAppStyling = false;
            this.ultraPanel9.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraPictureBox4
            // 
            appearance5.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            appearance5.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance5.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.ultraPictureBox4.Appearance = appearance5;
            this.ultraPictureBox4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ultraPictureBox4.BackColorInternal = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ultraPictureBox4.BorderShadowColor = System.Drawing.Color.Empty;
            this.ultraPictureBox4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox4.Image = ((object)(resources.GetObject("ultraPictureBox4.Image")));
            this.ultraPictureBox4.Location = new System.Drawing.Point(6, 4);
            this.ultraPictureBox4.Name = "ultraPictureBox4";
            this.ultraPictureBox4.Size = new System.Drawing.Size(14, 17);
            this.ultraPictureBox4.TabIndex = 194;
            // 
            // ultraPanel10
            // 
            appearance6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            appearance6.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(202)))), ((int)(((byte)(245)))));
            appearance6.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance6.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(150)))), ((int)(((byte)(215)))));
            this.ultraPanel10.Appearance = appearance6;
            this.ultraPanel10.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanel10.ClientArea
            // 
            this.ultraPanel10.ClientArea.Controls.Add(this.ultraPictureBox5);
            this.ultraPanel10.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPanel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(35)))), ((int)(((byte)(80)))));
            this.ultraPanel10.Location = new System.Drawing.Point(513, 42);
            this.ultraPanel10.Name = "ultraPanel10";
            this.ultraPanel10.Size = new System.Drawing.Size(32, 29);
            this.ultraPanel10.TabIndex = 55;
            this.ultraPanel10.UseAppStyling = false;
            this.ultraPanel10.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraPictureBox5
            // 
            appearance7.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            appearance7.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance7.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.ultraPictureBox5.Appearance = appearance7;
            this.ultraPictureBox5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ultraPictureBox5.BackColorInternal = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ultraPictureBox5.BorderShadowColor = System.Drawing.Color.Empty;
            this.ultraPictureBox5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox5.Image = ((object)(resources.GetObject("ultraPictureBox5.Image")));
            this.ultraPictureBox5.Location = new System.Drawing.Point(6, 4);
            this.ultraPictureBox5.Name = "ultraPictureBox5";
            this.ultraPictureBox5.Size = new System.Drawing.Size(14, 17);
            this.ultraPictureBox5.TabIndex = 195;
            // 
            // ultraPanel8
            // 
            appearance8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            appearance8.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(202)))), ((int)(((byte)(245)))));
            appearance8.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance8.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(150)))), ((int)(((byte)(215)))));
            this.ultraPanel8.Appearance = appearance8;
            this.ultraPanel8.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanel8.ClientArea
            // 
            this.ultraPanel8.ClientArea.Controls.Add(this.ultraPictureBox6);
            this.ultraPanel8.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPanel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(35)))), ((int)(((byte)(80)))));
            this.ultraPanel8.Location = new System.Drawing.Point(480, 42);
            this.ultraPanel8.Name = "ultraPanel8";
            this.ultraPanel8.Size = new System.Drawing.Size(32, 29);
            this.ultraPanel8.TabIndex = 56;
            this.ultraPanel8.UseAppStyling = false;
            this.ultraPanel8.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraPictureBox6
            // 
            appearance9.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            appearance9.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance9.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.ultraPictureBox6.Appearance = appearance9;
            this.ultraPictureBox6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ultraPictureBox6.BackColorInternal = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ultraPictureBox6.BorderShadowColor = System.Drawing.Color.Empty;
            this.ultraPictureBox6.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox6.Image = ((object)(resources.GetObject("ultraPictureBox6.Image")));
            this.ultraPictureBox6.Location = new System.Drawing.Point(6, 4);
            this.ultraPictureBox6.Name = "ultraPictureBox6";
            this.ultraPictureBox6.Size = new System.Drawing.Size(14, 17);
            this.ultraPictureBox6.TabIndex = 196;
            // 
            // btnLookupF7
            // 
            appearance10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(255)))));
            appearance10.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(212)))), ((int)(((byte)(248)))));
            appearance10.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance10.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(160)))), ((int)(((byte)(215)))));
            appearance10.FontData.BoldAsString = "True";
            appearance10.FontData.Name = "Segoe UI";
            appearance10.FontData.SizeInPoints = 9F;
            appearance10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(35)))), ((int)(((byte)(75)))));
            appearance10.TextHAlignAsString = "Center";
            appearance10.TextVAlignAsString = "Middle";
            this.btnLookupF7.Appearance = appearance10;
            this.btnLookupF7.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            appearance11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            appearance11.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(200)))), ((int)(((byte)(250)))));
            appearance11.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance11.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(90)))), ((int)(((byte)(180)))));
            appearance11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(20)))), ((int)(((byte)(55)))));
            this.btnLookupF7.HotTrackAppearance = appearance11;
            this.btnLookupF7.Location = new System.Drawing.Point(367, 42);
            this.btnLookupF7.Name = "btnLookupF7";
            appearance12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(180)))), ((int)(((byte)(235)))));
            appearance12.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(235)))), ((int)(((byte)(255)))));
            appearance12.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance12.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(130)))), ((int)(((byte)(190)))));
            this.btnLookupF7.PressedAppearance = appearance12;
            this.btnLookupF7.Size = new System.Drawing.Size(38, 29);
            this.btnLookupF7.TabIndex = 52;
            this.btnLookupF7.Text = "F7";
            this.btnLookupF7.UseHotTracking = Infragistics.Win.DefaultableBoolean.True;
            this.btnLookupF7.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // FrmReason
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 480);
            this.Controls.Add(this.panelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmReason";
            this.Text = "Reason";
            this.Load += new System.EventHandler(this.FrmReason_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmReason_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.txtReasonName)).EndInit();
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ultraPanel3.ClientArea.ResumeLayout(false);
            this.ultraPanel3.ResumeLayout(false);
            this.ultraPanel9.ClientArea.ResumeLayout(false);
            this.ultraPanel9.ResumeLayout(false);
            this.ultraPanel10.ClientArea.ResumeLayout(false);
            this.ultraPanel10.ResumeLayout(false);
            this.ultraPanel8.ClientArea.ResumeLayout(false);
            this.ultraPanel8.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanel3;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox2;
        private Infragistics.Win.Misc.UltraPanel ultraPanel9;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox4;
        private Infragistics.Win.Misc.UltraPanel ultraPanel10;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox5;
        private Infragistics.Win.Misc.UltraPanel ultraPanel8;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox6;
        private Infragistics.Win.Misc.UltraButton btnLookupF7;
    }
}
