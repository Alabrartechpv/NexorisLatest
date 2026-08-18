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
            Infragistics.Win.Appearance appearanceList = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceSearch = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance15 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance16 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance17 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance18 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance19 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance20 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance21 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance22 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance23 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance24 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance25 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance26 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance27 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance28 = new Infragistics.Win.Appearance();
            this.ultraPanel1 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraLabelTitle = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxEntry = new Infragistics.Win.Misc.UltraGroupBox();
            this.labelModeStatus = new System.Windows.Forms.Label();
            this.labelUserName = new System.Windows.Forms.Label();
            this.labelRequiredName = new System.Windows.Forms.Label();
            this.textUserName = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.labelPassword = new System.Windows.Forms.Label();
            this.labelRequiredPassword = new System.Windows.Forms.Label();
            this.textPassword = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.chkShowPassword = new System.Windows.Forms.CheckBox();
            this.labelEmail = new System.Windows.Forms.Label();
            this.textEmail = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.labelUserLevel = new System.Windows.Forms.Label();
            this.labelRequiredLevel = new System.Windows.Forms.Label();
            this.cmbUserLevel = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.labelRequiredNote = new System.Windows.Forms.Label();
            this.labelShortcutHint = new System.Windows.Forms.Label();
            this.ultraGroupBoxList = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraLabelSearch = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraGridUsers = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanel1.ClientArea.SuspendLayout();
            this.ultraPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxEntry)).BeginInit();
            this.ultraGroupBoxEntry.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textUserName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textPassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEmail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbUserLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxList)).BeginInit();
            this.ultraGroupBoxList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridUsers)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraPanel1
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(243)))), ((int)(((byte)(246)))));
            this.ultraPanel1.Appearance = appearance1;
            // 
            // ultraPanel1.ClientArea
            // 
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraLabelTitle);
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraGroupBoxEntry);
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraGroupBoxList);
            this.ultraPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanel1.Location = new System.Drawing.Point(0, 0);
            this.ultraPanel1.Name = "ultraPanel1";
            this.ultraPanel1.Size = new System.Drawing.Size(1349, 730);
            this.ultraPanel1.TabIndex = 0;
            // 
            // ultraLabelTitle
            // 
            appearance5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            appearance5.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(155)))));
            appearance5.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance5.FontData.BoldAsString = "True";
            appearance5.FontData.Name = "Segoe UI";
            appearance5.FontData.SizeInPoints = 16F;
            appearance5.ForeColor = System.Drawing.Color.White;
            appearance5.TextHAlignAsString = "Center";
            appearance5.TextVAlignAsString = "Middle";
            this.ultraLabelTitle.Appearance = appearance5;
            this.ultraLabelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraLabelTitle.Location = new System.Drawing.Point(0, 0);
            this.ultraLabelTitle.Name = "ultraLabelTitle";
            this.ultraLabelTitle.Size = new System.Drawing.Size(1349, 45);
            this.ultraLabelTitle.TabIndex = 0;
            this.ultraLabelTitle.Text = "USER MANAGEMENT";
            // 
            // ultraGroupBoxEntry
            // 
            appearance2.BackColor = System.Drawing.Color.White;
            appearance2.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(253)))), ((int)(((byte)(254)))));
            appearance2.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(224)))), ((int)(((byte)(233)))));
            this.ultraGroupBoxEntry.Appearance = appearance2;
            this.ultraGroupBoxEntry.Controls.Add(this.labelModeStatus);
            this.ultraGroupBoxEntry.Controls.Add(this.labelUserName);
            this.ultraGroupBoxEntry.Controls.Add(this.labelRequiredName);
            this.ultraGroupBoxEntry.Controls.Add(this.textUserName);
            this.ultraGroupBoxEntry.Controls.Add(this.labelPassword);
            this.ultraGroupBoxEntry.Controls.Add(this.labelRequiredPassword);
            this.ultraGroupBoxEntry.Controls.Add(this.textPassword);
            this.ultraGroupBoxEntry.Controls.Add(this.chkShowPassword);
            this.ultraGroupBoxEntry.Controls.Add(this.labelEmail);
            this.ultraGroupBoxEntry.Controls.Add(this.textEmail);
            this.ultraGroupBoxEntry.Controls.Add(this.labelUserLevel);
            this.ultraGroupBoxEntry.Controls.Add(this.labelRequiredLevel);
            this.ultraGroupBoxEntry.Controls.Add(this.cmbUserLevel);
            this.ultraGroupBoxEntry.Controls.Add(this.labelRequiredNote);
            this.ultraGroupBoxEntry.Controls.Add(this.labelShortcutHint);
            this.ultraGroupBoxEntry.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraGroupBoxEntry.HeaderBorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            this.ultraGroupBoxEntry.Location = new System.Drawing.Point(20, 60);
            this.ultraGroupBoxEntry.Name = "ultraGroupBoxEntry";
            this.ultraGroupBoxEntry.Size = new System.Drawing.Size(480, 395);
            this.ultraGroupBoxEntry.TabIndex = 1;
            this.ultraGroupBoxEntry.Text = "User Details";
            // 
            // labelModeStatus
            // 
            this.labelModeStatus.AutoSize = true;
            this.labelModeStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelModeStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(184)))));
            this.labelModeStatus.Location = new System.Drawing.Point(22, 28);
            this.labelModeStatus.Name = "labelModeStatus";
            this.labelModeStatus.Size = new System.Drawing.Size(70, 19);
            this.labelModeStatus.TabIndex = 0;
            this.labelModeStatus.Text = "New User";
            // 
            // labelUserName
            // 
            this.labelUserName.AutoSize = true;
            this.labelUserName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelUserName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(53)))), ((int)(((byte)(65)))));
            this.labelUserName.Location = new System.Drawing.Point(22, 58);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(86, 19);
            this.labelUserName.TabIndex = 1;
            this.labelUserName.Text = "User Name:";
            // 
            // labelRequiredName
            // 
            this.labelRequiredName.AutoSize = true;
            this.labelRequiredName.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.labelRequiredName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.labelRequiredName.Location = new System.Drawing.Point(108, 56);
            this.labelRequiredName.Name = "labelRequiredName";
            this.labelRequiredName.Size = new System.Drawing.Size(16, 20);
            this.labelRequiredName.TabIndex = 2;
            this.labelRequiredName.Text = "*";
            // 
            // textUserName
            // 
            appearance4.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.textUserName.Appearance = appearance4;
            this.textUserName.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.textUserName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.textUserName.Location = new System.Drawing.Point(22, 80);
            this.textUserName.Name = "textUserName";
            this.textUserName.Size = new System.Drawing.Size(435, 29);
            this.textUserName.TabIndex = 3;
            this.textUserName.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(53)))), ((int)(((byte)(65)))));
            this.labelPassword.Location = new System.Drawing.Point(22, 122);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(77, 19);
            this.labelPassword.TabIndex = 4;
            this.labelPassword.Text = "Password:";
            // 
            // labelRequiredPassword
            // 
            this.labelRequiredPassword.AutoSize = true;
            this.labelRequiredPassword.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.labelRequiredPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.labelRequiredPassword.Location = new System.Drawing.Point(100, 120);
            this.labelRequiredPassword.Name = "labelRequiredPassword";
            this.labelRequiredPassword.Size = new System.Drawing.Size(16, 20);
            this.labelRequiredPassword.TabIndex = 5;
            this.labelRequiredPassword.Text = "*";
            // 
            // textPassword
            // 
            this.textPassword.Appearance = appearance4;
            this.textPassword.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.textPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.textPassword.Location = new System.Drawing.Point(22, 144);
            this.textPassword.Name = "textPassword";
            this.textPassword.PasswordChar = '*';
            this.textPassword.Size = new System.Drawing.Size(435, 29);
            this.textPassword.TabIndex = 6;
            this.textPassword.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // chkShowPassword
            // 
            this.chkShowPassword.AutoSize = true;
            this.chkShowPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkShowPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(80)))), ((int)(((byte)(87)))));
            this.chkShowPassword.Location = new System.Drawing.Point(22, 178);
            this.chkShowPassword.Name = "chkShowPassword";
            this.chkShowPassword.Size = new System.Drawing.Size(108, 19);
            this.chkShowPassword.TabIndex = 7;
            this.chkShowPassword.Text = "Show Password";
            this.chkShowPassword.UseVisualStyleBackColor = true;
            this.chkShowPassword.CheckedChanged += new System.EventHandler(this.chkShowPassword_CheckedChanged);
            // 
            // labelEmail
            // 
            this.labelEmail.AutoSize = true;
            this.labelEmail.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelEmail.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(53)))), ((int)(((byte)(65)))));
            this.labelEmail.Location = new System.Drawing.Point(22, 208);
            this.labelEmail.Name = "labelEmail";
            this.labelEmail.Size = new System.Drawing.Size(55, 19);
            this.labelEmail.TabIndex = 8;
            this.labelEmail.Text = "E-mail:";
            // 
            // textEmail
            // 
            this.textEmail.Appearance = appearance4;
            this.textEmail.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.textEmail.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.textEmail.Location = new System.Drawing.Point(22, 230);
            this.textEmail.Name = "textEmail";
            this.textEmail.Size = new System.Drawing.Size(435, 29);
            this.textEmail.TabIndex = 9;
            this.textEmail.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.textEmail.Validating += new System.ComponentModel.CancelEventHandler(this.textEmail_Validating);
            // 
            // labelUserLevel
            // 
            this.labelUserLevel.AutoSize = true;
            this.labelUserLevel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelUserLevel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(53)))), ((int)(((byte)(65)))));
            this.labelUserLevel.Location = new System.Drawing.Point(22, 272);
            this.labelUserLevel.Name = "labelUserLevel";
            this.labelUserLevel.Size = new System.Drawing.Size(81, 19);
            this.labelUserLevel.TabIndex = 10;
            this.labelUserLevel.Text = "User Level:";
            // 
            // labelRequiredLevel
            // 
            this.labelRequiredLevel.AutoSize = true;
            this.labelRequiredLevel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.labelRequiredLevel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.labelRequiredLevel.Location = new System.Drawing.Point(105, 270);
            this.labelRequiredLevel.Name = "labelRequiredLevel";
            this.labelRequiredLevel.Size = new System.Drawing.Size(16, 20);
            this.labelRequiredLevel.TabIndex = 11;
            this.labelRequiredLevel.Text = "*";
            // 
            // cmbUserLevel
            // 
            this.cmbUserLevel.Appearance = appearance4;
            this.cmbUserLevel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.cmbUserLevel.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.cmbUserLevel.Location = new System.Drawing.Point(22, 294);
            this.cmbUserLevel.Name = "cmbUserLevel";
            this.cmbUserLevel.Size = new System.Drawing.Size(435, 29);
            this.cmbUserLevel.TabIndex = 12;
            this.cmbUserLevel.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // labelRequiredNote
            // 
            this.labelRequiredNote.AutoSize = true;
            this.labelRequiredNote.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelRequiredNote.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.labelRequiredNote.Location = new System.Drawing.Point(22, 342);
            this.labelRequiredNote.Name = "labelRequiredNote";
            this.labelRequiredNote.Size = new System.Drawing.Size(93, 15);
            this.labelRequiredNote.TabIndex = 13;
            this.labelRequiredNote.Text = "* Required fields";
            // 
            // labelShortcutHint
            // 
            this.labelShortcutHint.AutoSize = true;
            this.labelShortcutHint.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.labelShortcutHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.labelShortcutHint.Location = new System.Drawing.Point(22, 362);
            this.labelShortcutHint.Name = "labelShortcutHint";
            this.labelShortcutHint.Size = new System.Drawing.Size(320, 15);
            this.labelShortcutHint.TabIndex = 14;
            this.labelShortcutHint.Text = "Shortcuts: F8 Save | F1 Clear | F4/Esc Close | Enter Next";
            // 
            // ultraGroupBoxList
            // 
            appearanceList.BackColor = System.Drawing.Color.White;
            appearanceList.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(224)))), ((int)(((byte)(233)))));
            this.ultraGroupBoxList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraGroupBoxList.Appearance = appearanceList;
            this.ultraGroupBoxList.Controls.Add(this.ultraLabelSearch);
            this.ultraGroupBoxList.Controls.Add(this.ultraTextSearch);
            this.ultraGroupBoxList.Controls.Add(this.ultraGridUsers);
            this.ultraGroupBoxList.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraGroupBoxList.HeaderBorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            this.ultraGroupBoxList.Location = new System.Drawing.Point(515, 60);
            this.ultraGroupBoxList.Name = "ultraGroupBoxList";
            this.ultraGroupBoxList.Size = new System.Drawing.Size(810, 645);
            this.ultraGroupBoxList.TabIndex = 2;
            this.ultraGroupBoxList.Text = "User Master List";
            // 
            // ultraLabelSearch
            // 
            this.ultraLabelSearch.AutoSize = true;
            this.ultraLabelSearch.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(53)))), ((int)(((byte)(65)))));
            this.ultraLabelSearch.Location = new System.Drawing.Point(20, 32);
            this.ultraLabelSearch.Name = "ultraLabelSearch";
            this.ultraLabelSearch.Size = new System.Drawing.Size(52, 20);
            this.ultraLabelSearch.TabIndex = 0;
            this.ultraLabelSearch.Text = "Search:";
            // 
            // ultraTextSearch
            // 
            this.ultraTextSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            appearanceSearch.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.ultraTextSearch.Appearance = appearanceSearch;
            this.ultraTextSearch.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraTextSearch.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextSearch.Location = new System.Drawing.Point(80, 28);
            this.ultraTextSearch.Name = "ultraTextSearch";
            this.ultraTextSearch.Size = new System.Drawing.Size(705, 28);
            this.ultraTextSearch.TabIndex = 1;
            this.ultraTextSearch.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraGridUsers
            // 
            this.ultraGridUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraGridUsers.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;
            this.ultraGridUsers.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraGridUsers.DisplayLayout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;
            appearance15.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            appearance15.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(245)))));
            appearance15.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance15.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.ultraGridUsers.DisplayLayout.GroupByBox.Appearance = appearance15;
            appearance16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.ultraGridUsers.DisplayLayout.GroupByBox.BandLabelAppearance = appearance16;
            this.ultraGridUsers.DisplayLayout.GroupByBox.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraGridUsers.DisplayLayout.GroupByBox.Hidden = true;
            appearance17.BackColor = System.Drawing.Color.White;
            appearance17.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            appearance17.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.ultraGridUsers.DisplayLayout.GroupByBox.PromptAppearance = appearance17;
            this.ultraGridUsers.DisplayLayout.InterBandSpacing = 10;
            this.ultraGridUsers.DisplayLayout.MaxColScrollRegions = 1;
            this.ultraGridUsers.DisplayLayout.MaxRowScrollRegions = 1;
            appearance18.ForeColor = System.Drawing.Color.Black;
            this.ultraGridUsers.DisplayLayout.Override.ActiveCellAppearance = appearance18;
            appearance19.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(242)))), ((int)(((byte)(253)))));
            appearance19.BackGradientStyle = Infragistics.Win.GradientStyle.None;
            appearance19.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(71)))), ((int)(((byte)(161)))));
            this.ultraGridUsers.DisplayLayout.Override.ActiveRowAppearance = appearance19;
            this.ultraGridUsers.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
            this.ultraGridUsers.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
            this.ultraGridUsers.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.False;
            this.ultraGridUsers.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraGridUsers.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
            appearance20.BackColor = System.Drawing.Color.White;
            this.ultraGridUsers.DisplayLayout.Override.CardAreaAppearance = appearance20;
            appearance21.FontData.Name = "Segoe UI";
            appearance21.FontData.SizeInPoints = 9.5F;
            appearance21.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            appearance21.TextHAlignAsString = "Left";
            this.ultraGridUsers.DisplayLayout.Override.CellAppearance = appearance21;
            this.ultraGridUsers.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;
            this.ultraGridUsers.DisplayLayout.Override.CellPadding = 5;
            this.ultraGridUsers.DisplayLayout.Override.DefaultRowHeight = 28;
            this.ultraGridUsers.DisplayLayout.Override.ExpansionIndicator = Infragistics.Win.UltraWinGrid.ShowExpansionIndicator.Never;
            appearance22.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            appearance22.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            appearance22.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance22.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.ultraGridUsers.DisplayLayout.Override.GroupByRowAppearance = appearance22;
            appearance23.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            appearance23.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(155)))));
            appearance23.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance23.FontData.BoldAsString = "True";
            appearance23.FontData.Name = "Segoe UI";
            appearance23.FontData.SizeInPoints = 9.5F;
            appearance23.ForeColor = System.Drawing.Color.White;
            appearance23.TextHAlignAsString = "Center";
            appearance23.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;
            this.ultraGridUsers.DisplayLayout.Override.HeaderAppearance = appearance23;
            this.ultraGridUsers.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
            this.ultraGridUsers.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
            appearance24.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ultraGridUsers.DisplayLayout.Override.RowAlternateAppearance = appearance24;
            appearance25.BackColor = System.Drawing.Color.White;
            this.ultraGridUsers.DisplayLayout.Override.RowAppearance = appearance25;
            this.ultraGridUsers.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
            this.ultraGridUsers.DisplayLayout.Override.RowSizing = Infragistics.Win.UltraWinGrid.RowSizing.Fixed;
            this.ultraGridUsers.DisplayLayout.Override.RowSizingArea = Infragistics.Win.UltraWinGrid.RowSizingArea.EntireRow;
            this.ultraGridUsers.DisplayLayout.Override.RowSpacingBefore = 0;
            appearance26.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(242)))), ((int)(((byte)(253)))));
            appearance26.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            appearance26.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(71)))), ((int)(((byte)(161)))));
            this.ultraGridUsers.DisplayLayout.Override.SelectedCellAppearance = appearance26;
            appearance27.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            appearance27.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(92)))), ((int)(((byte)(155)))));
            appearance27.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance27.FontData.BoldAsString = "True";
            appearance27.ForeColor = System.Drawing.Color.White;
            this.ultraGridUsers.DisplayLayout.Override.SelectedRowAppearance = appearance27;
            this.ultraGridUsers.DisplayLayout.Override.SelectTypeCell = Infragistics.Win.UltraWinGrid.SelectType.Single;
            this.ultraGridUsers.DisplayLayout.Override.SelectTypeCol = Infragistics.Win.UltraWinGrid.SelectType.None;
            this.ultraGridUsers.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
            appearance28.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.ultraGridUsers.DisplayLayout.Override.TemplateAddRowAppearance = appearance28;
            this.ultraGridUsers.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
            this.ultraGridUsers.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;
            this.ultraGridUsers.DisplayLayout.ViewStyleBand = Infragistics.Win.UltraWinGrid.ViewStyleBand.OutlookGroupBy;
            this.ultraGridUsers.Location = new System.Drawing.Point(20, 68);
            this.ultraGridUsers.Name = "ultraGridUsers";
            this.ultraGridUsers.Size = new System.Drawing.Size(765, 555);
            this.ultraGridUsers.TabIndex = 2;
            this.ultraGridUsers.Text = "ultraGridUsers";
            this.ultraGridUsers.UseOsThemes = Infragistics.Win.DefaultableBoolean.True;
            // 
            // FrmUsers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1349, 730);
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
            ((System.ComponentModel.ISupportInitialize)(this.textUserName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textPassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEmail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbUserLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxList)).EndInit();
            this.ultraGroupBoxList.ResumeLayout(false);
            this.ultraGroupBoxList.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridUsers)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanel1;
        private Infragistics.Win.Misc.UltraLabel ultraLabelTitle;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxEntry;
        private System.Windows.Forms.Label labelShortcutHint;
        private System.Windows.Forms.Label labelRequiredNote;
        private System.Windows.Forms.CheckBox chkShowPassword;
        private System.Windows.Forms.Label labelRequiredLevel;
        private System.Windows.Forms.Label labelRequiredPassword;
        private System.Windows.Forms.Label labelRequiredName;
        private System.Windows.Forms.Label labelModeStatus;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor textUserName;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor textEmail;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor textPassword;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbUserLevel;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.Label labelEmail;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.Label labelUserLevel;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxList;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridUsers;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextSearch;
        private Infragistics.Win.Misc.UltraLabel ultraLabelSearch;
    }
}
