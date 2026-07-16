namespace PosBranch_Win.Master
{
    partial class FrmUsers
    {
        private System.ComponentModel.IContainer components = null;

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
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance6 = new Infragistics.Win.Appearance();
            this.ultraPanel1 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGroupBoxEntry = new Infragistics.Win.Misc.UltraGroupBox();
            this.labelShortcutHint = new System.Windows.Forms.Label();
            this.labelRequiredNote = new System.Windows.Forms.Label();
            this.btnClearForm = new Infragistics.Win.Misc.UltraButton();
            this.chkShowPassword = new System.Windows.Forms.CheckBox();
            this.labelRequiredLevel = new System.Windows.Forms.Label();
            this.labelRequiredPassword = new System.Windows.Forms.Label();
            this.labelRequiredName = new System.Windows.Forms.Label();
            this.labelModeStatus = new System.Windows.Forms.Label();
            this.btnUsersList = new Infragistics.Win.Misc.UltraButton();
            this.cmbUserLevel = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.labelUserLevel = new System.Windows.Forms.Label();
            this.textPassword = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textEmail = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.labelEmail = new System.Windows.Forms.Label();
            this.textUserName = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.labelUserName = new System.Windows.Forms.Label();
            this.ultraLabelTitle = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanel1.ClientArea.SuspendLayout();
            this.ultraPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxEntry)).BeginInit();
            this.ultraGroupBoxEntry.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbUserLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textPassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEmail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textUserName)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraPanel1
            // 
            appearance1.BackColor = System.Drawing.Color.White;
            appearance1.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            appearance1.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.ultraPanel1.Appearance = appearance1;
            // 
            // ultraPanel1.ClientArea
            // 
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraGroupBoxEntry);
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraLabelTitle);
            this.ultraPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanel1.Location = new System.Drawing.Point(0, 0);
            this.ultraPanel1.Name = "ultraPanel1";
            this.ultraPanel1.Size = new System.Drawing.Size(980, 430);
            this.ultraPanel1.TabIndex = 0;
            // 
            // ultraGroupBoxEntry
            // 
            appearance2.BackColor = System.Drawing.Color.White;
            appearance2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(225)))), ((int)(((byte)(238)))));
            this.ultraGroupBoxEntry.Appearance = appearance2;
            this.ultraGroupBoxEntry.Controls.Add(this.labelShortcutHint);
            this.ultraGroupBoxEntry.Controls.Add(this.labelRequiredNote);
            this.ultraGroupBoxEntry.Controls.Add(this.btnClearForm);
            this.ultraGroupBoxEntry.Controls.Add(this.chkShowPassword);
            this.ultraGroupBoxEntry.Controls.Add(this.labelRequiredLevel);
            this.ultraGroupBoxEntry.Controls.Add(this.labelRequiredPassword);
            this.ultraGroupBoxEntry.Controls.Add(this.labelRequiredName);
            this.ultraGroupBoxEntry.Controls.Add(this.labelModeStatus);
            this.ultraGroupBoxEntry.Controls.Add(this.btnUsersList);
            this.ultraGroupBoxEntry.Controls.Add(this.cmbUserLevel);
            this.ultraGroupBoxEntry.Controls.Add(this.labelUserLevel);
            this.ultraGroupBoxEntry.Controls.Add(this.textPassword);
            this.ultraGroupBoxEntry.Controls.Add(this.labelPassword);
            this.ultraGroupBoxEntry.Controls.Add(this.textEmail);
            this.ultraGroupBoxEntry.Controls.Add(this.labelEmail);
            this.ultraGroupBoxEntry.Controls.Add(this.textUserName);
            this.ultraGroupBoxEntry.Controls.Add(this.labelUserName);
            this.ultraGroupBoxEntry.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.ultraGroupBoxEntry.HeaderBorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            this.ultraGroupBoxEntry.Location = new System.Drawing.Point(40, 82);
            this.ultraGroupBoxEntry.Name = "ultraGroupBoxEntry";
            this.ultraGroupBoxEntry.Size = new System.Drawing.Size(900, 240);
            this.ultraGroupBoxEntry.TabIndex = 1;
            this.ultraGroupBoxEntry.Text = "User Details";
            // 
            // labelShortcutHint
            // 
            this.labelShortcutHint.AutoSize = true;
            this.labelShortcutHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelShortcutHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(109)))), ((int)(((byte)(126)))));
            this.labelShortcutHint.Location = new System.Drawing.Point(70, 214);
            this.labelShortcutHint.Name = "labelShortcutHint";
            this.labelShortcutHint.Size = new System.Drawing.Size(130, 15);
            this.labelShortcutHint.TabIndex = 0;
            this.labelShortcutHint.Text = "F8 Save    F4/Esc Close";
            // 
            // labelRequiredNote
            // 
            this.labelRequiredNote.AutoSize = true;
            this.labelRequiredNote.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelRequiredNote.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.labelRequiredNote.Location = new System.Drawing.Point(70, 190);
            this.labelRequiredNote.Name = "labelRequiredNote";
            this.labelRequiredNote.Size = new System.Drawing.Size(93, 15);
            this.labelRequiredNote.TabIndex = 0;
            this.labelRequiredNote.Text = "* Required fields";
            // 
            // btnClearForm
            // 
            appearance6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            appearance6.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(98)))), ((int)(((byte)(104)))));
            appearance6.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance6.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(98)))), ((int)(((byte)(104)))));
            appearance6.FontData.BoldAsString = "True";
            appearance6.FontData.Name = "Segoe UI";
            appearance6.FontData.SizeInPoints = 9F;
            appearance6.ForeColor = System.Drawing.Color.White;
            appearance6.TextHAlignAsString = "Center";
            appearance6.TextVAlignAsString = "Middle";
            this.btnClearForm.Appearance = appearance6;
            this.btnClearForm.Location = new System.Drawing.Point(600, 198);
            this.btnClearForm.Name = "btnClearForm";
            this.btnClearForm.Size = new System.Drawing.Size(112, 31);
            this.btnClearForm.TabIndex = 6;
            this.btnClearForm.Text = "New / Clear";
            this.btnClearForm.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnClearForm.Click += new System.EventHandler(this.btnClearForm_Click);
            // 
            // chkShowPassword
            // 
            this.chkShowPassword.AutoSize = true;
            this.chkShowPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkShowPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(80)))), ((int)(((byte)(87)))));
            this.chkShowPassword.Location = new System.Drawing.Point(610, 88);
            this.chkShowPassword.Name = "chkShowPassword";
            this.chkShowPassword.Size = new System.Drawing.Size(106, 19);
            this.chkShowPassword.TabIndex = 0;
            this.chkShowPassword.Text = "Show Password";
            this.chkShowPassword.UseVisualStyleBackColor = true;
            this.chkShowPassword.CheckedChanged += new System.EventHandler(this.chkShowPassword_CheckedChanged);
            // 
            // labelRequiredLevel
            // 
            this.labelRequiredLevel.AutoSize = true;
            this.labelRequiredLevel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.labelRequiredLevel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.labelRequiredLevel.Location = new System.Drawing.Point(580, 125);
            this.labelRequiredLevel.Name = "labelRequiredLevel";
            this.labelRequiredLevel.Size = new System.Drawing.Size(16, 20);
            this.labelRequiredLevel.TabIndex = 0;
            this.labelRequiredLevel.Text = "*";
            // 
            // labelRequiredPassword
            // 
            this.labelRequiredPassword.AutoSize = true;
            this.labelRequiredPassword.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.labelRequiredPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.labelRequiredPassword.Location = new System.Drawing.Point(580, 57);
            this.labelRequiredPassword.Name = "labelRequiredPassword";
            this.labelRequiredPassword.Size = new System.Drawing.Size(16, 20);
            this.labelRequiredPassword.TabIndex = 0;
            this.labelRequiredPassword.Text = "*";
            // 
            // labelRequiredName
            // 
            this.labelRequiredName.AutoSize = true;
            this.labelRequiredName.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.labelRequiredName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.labelRequiredName.Location = new System.Drawing.Point(154, 57);
            this.labelRequiredName.Name = "labelRequiredName";
            this.labelRequiredName.Size = new System.Drawing.Size(16, 20);
            this.labelRequiredName.TabIndex = 0;
            this.labelRequiredName.Text = "*";
            // 
            // labelModeStatus
            // 
            this.labelModeStatus.AutoSize = true;
            this.labelModeStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelModeStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(184)))));
            this.labelModeStatus.Location = new System.Drawing.Point(70, 32);
            this.labelModeStatus.Name = "labelModeStatus";
            this.labelModeStatus.Size = new System.Drawing.Size(69, 19);
            this.labelModeStatus.TabIndex = 0;
            this.labelModeStatus.Text = "New User";
            // 
            // btnUsersList
            // 
            appearance3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            appearance3.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(184)))));
            appearance3.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance3.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(184)))));
            appearance3.FontData.BoldAsString = "True";
            appearance3.FontData.Name = "Segoe UI";
            appearance3.FontData.SizeInPoints = 9F;
            appearance3.ForeColor = System.Drawing.Color.White;
            appearance3.TextHAlignAsString = "Center";
            appearance3.TextVAlignAsString = "Middle";
            this.btnUsersList.Appearance = appearance3;
            this.btnUsersList.Location = new System.Drawing.Point(730, 54);
            this.btnUsersList.Name = "btnUsersList";
            this.btnUsersList.Size = new System.Drawing.Size(112, 31);
            this.btnUsersList.TabIndex = 5;
            this.btnUsersList.Text = "User List";
            this.btnUsersList.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnUsersList.Click += new System.EventHandler(this.btnUsersList_Click);
            // 
            // cmbUserLevel
            // 
            appearance4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            appearance4.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.cmbUserLevel.Appearance = appearance4;
            this.cmbUserLevel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.cmbUserLevel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.cmbUserLevel.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.cmbUserLevel.Location = new System.Drawing.Point(610, 122);
            this.cmbUserLevel.Name = "cmbUserLevel";
            this.cmbUserLevel.Size = new System.Drawing.Size(220, 27);
            this.cmbUserLevel.TabIndex = 4;
            this.cmbUserLevel.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // labelUserLevel
            // 
            this.labelUserLevel.AutoSize = true;
            this.labelUserLevel.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.labelUserLevel.Location = new System.Drawing.Point(500, 125);
            this.labelUserLevel.Name = "labelUserLevel";
            this.labelUserLevel.Size = new System.Drawing.Size(76, 20);
            this.labelUserLevel.TabIndex = 0;
            this.labelUserLevel.Text = "User Level";
            // 
            // textPassword
            // 
            this.textPassword.Appearance = appearance4;
            this.textPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.textPassword.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.textPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.textPassword.Location = new System.Drawing.Point(610, 54);
            this.textPassword.Name = "textPassword";
            this.textPassword.PasswordChar = '*';
            this.textPassword.Size = new System.Drawing.Size(220, 27);
            this.textPassword.TabIndex = 3;
            this.textPassword.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.labelPassword.Location = new System.Drawing.Point(500, 57);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(70, 20);
            this.labelPassword.TabIndex = 0;
            this.labelPassword.Text = "Password";
            // 
            // textEmail
            // 
            this.textEmail.Appearance = appearance4;
            this.textEmail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.textEmail.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.textEmail.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.textEmail.Location = new System.Drawing.Point(150, 122);
            this.textEmail.Name = "textEmail";
            this.textEmail.Size = new System.Drawing.Size(300, 27);
            this.textEmail.TabIndex = 2;
            this.textEmail.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.textEmail.Validating += new System.ComponentModel.CancelEventHandler(this.textEmail_Validating);
            // 
            // labelEmail
            // 
            this.labelEmail.AutoSize = true;
            this.labelEmail.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.labelEmail.Location = new System.Drawing.Point(70, 125);
            this.labelEmail.Name = "labelEmail";
            this.labelEmail.Size = new System.Drawing.Size(52, 20);
            this.labelEmail.TabIndex = 0;
            this.labelEmail.Text = "E-mail";
            // 
            // textUserName
            // 
            this.textUserName.Appearance = appearance4;
            this.textUserName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.textUserName.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.textUserName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.textUserName.Location = new System.Drawing.Point(150, 54);
            this.textUserName.Name = "textUserName";
            this.textUserName.Size = new System.Drawing.Size(300, 27);
            this.textUserName.TabIndex = 1;
            this.textUserName.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // labelUserName
            // 
            this.labelUserName.AutoSize = true;
            this.labelUserName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.labelUserName.Location = new System.Drawing.Point(70, 57);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(82, 20);
            this.labelUserName.TabIndex = 0;
            this.labelUserName.Text = "User Name";
            // 
            // ultraLabelTitle
            // 
            appearance5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            appearance5.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(184)))));
            appearance5.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance5.FontData.BoldAsString = "True";
            appearance5.FontData.Name = "Segoe UI";
            appearance5.FontData.SizeInPoints = 18F;
            appearance5.ForeColor = System.Drawing.Color.White;
            appearance5.TextHAlignAsString = "Center";
            appearance5.TextVAlignAsString = "Middle";
            this.ultraLabelTitle.Appearance = appearance5;
            this.ultraLabelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraLabelTitle.Location = new System.Drawing.Point(0, 0);
            this.ultraLabelTitle.Name = "ultraLabelTitle";
            this.ultraLabelTitle.Size = new System.Drawing.Size(980, 48);
            this.ultraLabelTitle.TabIndex = 0;
            this.ultraLabelTitle.Text = "User Management";
            // 
            // FrmUsers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(980, 430);
            this.Controls.Add(this.ultraPanel1);
            this.Name = "FrmUsers";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "User Management";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmUsers_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmUsers_KeyDown);
            this.ultraPanel1.ClientArea.ResumeLayout(false);
            this.ultraPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxEntry)).EndInit();
            this.ultraGroupBoxEntry.ResumeLayout(false);
            this.ultraGroupBoxEntry.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbUserLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textPassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEmail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textUserName)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanel1;
        private Infragistics.Win.Misc.UltraLabel ultraLabelTitle;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxEntry;
        private System.Windows.Forms.Label labelShortcutHint;
        private System.Windows.Forms.Label labelRequiredNote;
        private Infragistics.Win.Misc.UltraButton btnClearForm;
        private System.Windows.Forms.CheckBox chkShowPassword;
        private System.Windows.Forms.Label labelRequiredLevel;
        private System.Windows.Forms.Label labelRequiredPassword;
        private System.Windows.Forms.Label labelRequiredName;
        private System.Windows.Forms.Label labelModeStatus;
        private Infragistics.Win.Misc.UltraButton btnUsersList;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor textUserName;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor textEmail;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor textPassword;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbUserLevel;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.Label labelEmail;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.Label labelUserLevel;
    }
}
