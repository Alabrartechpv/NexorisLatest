namespace PosBranch_Win.Settings
{
    partial class FrmFinancialYearClosing
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance9 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance6 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance7 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance8 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance10 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance11 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance12 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance13 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance14 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance15 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance16 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance17 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance18 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance19 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance20 = new Infragistics.Win.Appearance();
            this.panelHeader = new Infragistics.Win.Misc.UltraPanel();
            this.lblTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblSubtitle = new Infragistics.Win.Misc.UltraLabel();
            this.groupBoxCurrent = new Infragistics.Win.Misc.UltraGroupBox();
            this.lblCurrentBadge = new Infragistics.Win.Misc.UltraLabel();
            this.lblCurTo = new Infragistics.Win.Misc.UltraLabel();
            this.lblCurFrom = new Infragistics.Win.Misc.UltraLabel();
            this.lblCurId = new Infragistics.Win.Misc.UltraLabel();
            this.groupBoxNew = new Infragistics.Win.Misc.UltraGroupBox();
            this.dtpNewTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.dtpNewFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.txtNewId = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.label3 = new Infragistics.Win.Misc.UltraLabel();
            this.label2 = new Infragistics.Win.Misc.UltraLabel();
            this.label1 = new Infragistics.Win.Misc.UltraLabel();
            this.pnlWarning = new Infragistics.Win.Misc.UltraPanel();
            this.lblWarningTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblWarningText = new Infragistics.Win.Misc.UltraLabel();
            this.groupBoxChecks = new Infragistics.Win.Misc.UltraGroupBox();
            this.lstChecks = new System.Windows.Forms.ListBox();
            this.btnVerify = new Infragistics.Win.Misc.UltraButton();
            this.btnRunClosing = new Infragistics.Win.Misc.UltraButton();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgressStatus = new Infragistics.Win.Misc.UltraLabel();
            this.panelHeader.ClientArea.SuspendLayout();
            this.panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxCurrent)).BeginInit();
            this.groupBoxCurrent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxNew)).BeginInit();
            this.groupBoxNew.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtpNewTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpNewFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNewId)).BeginInit();
            this.pnlWarning.ClientArea.SuspendLayout();
            this.pnlWarning.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxChecks)).BeginInit();
            this.groupBoxChecks.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
            appearance1.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(111)))), ((int)(((byte)(184)))));
            appearance1.BackGradientStyle = Infragistics.Win.GradientStyle.Horizontal;
            this.panelHeader.Appearance = appearance1;
            // 
            // panelHeader.ClientArea
            // 
            this.panelHeader.ClientArea.Controls.Add(this.lblTitle);
            this.panelHeader.ClientArea.Controls.Add(this.lblSubtitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(980, 72);
            this.panelHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            appearance2.BackColor = System.Drawing.Color.Transparent;
            appearance2.ForeColor = System.Drawing.Color.White;
            appearance2.TextVAlignAsString = "Middle";
            this.lblTitle.Appearance = appearance2;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 17F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(28, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(460, 34);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Financial Year Closing";
            // 
            // lblSubtitle
            // 
            appearance3.BackColor = System.Drawing.Color.Transparent;
            appearance3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(232)))), ((int)(((byte)(248)))));
            this.lblSubtitle.Appearance = appearance3;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSubtitle.Location = new System.Drawing.Point(31, 43);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(700, 22);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Validate and roll balances, stock, and transaction sequences into the next financ" +
    "ial year.";
            // 
            // groupBoxCurrent
            // 
            appearance4.BackColor = System.Drawing.Color.White;
            appearance4.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(226)))), ((int)(((byte)(236)))));
            this.groupBoxCurrent.Appearance = appearance4;
            this.groupBoxCurrent.Controls.Add(this.lblCurrentBadge);
            this.groupBoxCurrent.Controls.Add(this.lblCurTo);
            this.groupBoxCurrent.Controls.Add(this.lblCurFrom);
            this.groupBoxCurrent.Controls.Add(this.lblCurId);
            this.groupBoxCurrent.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            appearance9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(56)))), ((int)(((byte)(82)))));
            this.groupBoxCurrent.HeaderAppearance = appearance9;
            this.groupBoxCurrent.Location = new System.Drawing.Point(28, 88);
            this.groupBoxCurrent.Name = "groupBoxCurrent";
            this.groupBoxCurrent.Size = new System.Drawing.Size(442, 132);
            this.groupBoxCurrent.TabIndex = 1;
            this.groupBoxCurrent.Text = "Current financial year";
            // 
            // lblCurrentBadge
            // 
            appearance5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(242)))), ((int)(((byte)(254)))));
            appearance5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(105)))), ((int)(((byte)(161)))));
            appearance5.TextHAlignAsString = "Center";
            appearance5.TextVAlignAsString = "Middle";
            this.lblCurrentBadge.Appearance = appearance5;
            this.lblCurrentBadge.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblCurrentBadge.Location = new System.Drawing.Point(322, 25);
            this.lblCurrentBadge.Name = "lblCurrentBadge";
            this.lblCurrentBadge.Size = new System.Drawing.Size(88, 24);
            this.lblCurrentBadge.TabIndex = 3;
            this.lblCurrentBadge.Text = "ACTIVE";
            // 
            // lblCurTo
            // 
            appearance6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(91)))), ((int)(((byte)(110)))));
            this.lblCurTo.Appearance = appearance6;
            this.lblCurTo.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblCurTo.Location = new System.Drawing.Point(25, 98);
            this.lblCurTo.Name = "lblCurTo";
            this.lblCurTo.Size = new System.Drawing.Size(350, 25);
            this.lblCurTo.TabIndex = 4;
            this.lblCurTo.Text = "Date To: --";
            // 
            // lblCurFrom
            // 
            appearance7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(91)))), ((int)(((byte)(110)))));
            this.lblCurFrom.Appearance = appearance7;
            this.lblCurFrom.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblCurFrom.Location = new System.Drawing.Point(25, 67);
            this.lblCurFrom.Name = "lblCurFrom";
            this.lblCurFrom.Size = new System.Drawing.Size(350, 25);
            this.lblCurFrom.TabIndex = 5;
            this.lblCurFrom.Text = "Date From: --";
            // 
            // lblCurId
            // 
            appearance8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
            this.lblCurId.Appearance = appearance8;
            this.lblCurId.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblCurId.Location = new System.Drawing.Point(25, 35);
            this.lblCurId.Name = "lblCurId";
            this.lblCurId.Size = new System.Drawing.Size(280, 25);
            this.lblCurId.TabIndex = 6;
            this.lblCurId.Text = "Year ID: --";
            // 
            // groupBoxNew
            // 
            this.groupBoxNew.Appearance = appearance4;
            this.groupBoxNew.Controls.Add(this.dtpNewTo);
            this.groupBoxNew.Controls.Add(this.dtpNewFrom);
            this.groupBoxNew.Controls.Add(this.txtNewId);
            this.groupBoxNew.Controls.Add(this.label3);
            this.groupBoxNew.Controls.Add(this.label2);
            this.groupBoxNew.Controls.Add(this.label1);
            this.groupBoxNew.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.groupBoxNew.HeaderAppearance = appearance9;
            this.groupBoxNew.Location = new System.Drawing.Point(490, 88);
            this.groupBoxNew.Name = "groupBoxNew";
            this.groupBoxNew.Size = new System.Drawing.Size(462, 132);
            this.groupBoxNew.TabIndex = 2;
            this.groupBoxNew.Text = "Next financial year";
            // 
            // dtpNewTo
            // 
            appearance10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            appearance10.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(209)))), ((int)(((byte)(223)))));
            appearance10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.dtpNewTo.Appearance = appearance10;
            this.dtpNewTo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.dtpNewTo.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.dtpNewTo.FormatString = "dd-MM-yyyy";
            this.dtpNewTo.Location = new System.Drawing.Point(178, 95);
            this.dtpNewTo.Name = "dtpNewTo";
            this.dtpNewTo.Size = new System.Drawing.Size(245, 28);
            this.dtpNewTo.TabIndex = 5;
            // 
            // dtpNewFrom
            // 
            this.dtpNewFrom.Appearance = appearance10;
            this.dtpNewFrom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.dtpNewFrom.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.dtpNewFrom.FormatString = "dd-MM-yyyy";
            this.dtpNewFrom.Location = new System.Drawing.Point(178, 62);
            this.dtpNewFrom.Name = "dtpNewFrom";
            this.dtpNewFrom.Size = new System.Drawing.Size(245, 28);
            this.dtpNewFrom.TabIndex = 4;
            // 
            // txtNewId
            // 
            this.txtNewId.Appearance = appearance10;
            this.txtNewId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.txtNewId.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            this.txtNewId.Location = new System.Drawing.Point(178, 29);
            this.txtNewId.Name = "txtNewId";
            this.txtNewId.ReadOnly = true;
            this.txtNewId.Size = new System.Drawing.Size(245, 28);
            this.txtNewId.TabIndex = 3;
            // 
            // label3
            // 
            appearance11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(91)))), ((int)(((byte)(110)))));
            this.label3.Appearance = appearance11;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label3.Location = new System.Drawing.Point(25, 99);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 23);
            this.label3.TabIndex = 6;
            this.label3.Text = "Ends on";
            // 
            // label2
            // 
            appearance12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(91)))), ((int)(((byte)(110)))));
            this.label2.Appearance = appearance12;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label2.Location = new System.Drawing.Point(25, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 23);
            this.label2.TabIndex = 7;
            this.label2.Text = "Starts on";
            // 
            // label1
            // 
            appearance13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(91)))), ((int)(((byte)(110)))));
            this.label1.Appearance = appearance13;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label1.Location = new System.Drawing.Point(25, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 23);
            this.label1.TabIndex = 8;
            this.label1.Text = "Year ID";
            // 
            // pnlWarning
            // 
            appearance14.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(247)))), ((int)(((byte)(237)))));
            appearance14.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(186)))), ((int)(((byte)(116)))));
            appearance14.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(186)))), ((int)(((byte)(116)))));
            this.pnlWarning.Appearance = appearance14;
            this.pnlWarning.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // pnlWarning.ClientArea
            // 
            this.pnlWarning.ClientArea.Controls.Add(this.lblWarningTitle);
            this.pnlWarning.ClientArea.Controls.Add(this.lblWarningText);
            this.pnlWarning.Location = new System.Drawing.Point(28, 232);
            this.pnlWarning.Name = "pnlWarning";
            this.pnlWarning.Size = new System.Drawing.Size(924, 52);
            this.pnlWarning.TabIndex = 3;
            // 
            // lblWarningTitle
            // 
            appearance15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(52)))), ((int)(((byte)(18)))));
            this.lblWarningTitle.Appearance = appearance15;
            this.lblWarningTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblWarningTitle.Location = new System.Drawing.Point(18, 5);
            this.lblWarningTitle.Name = "lblWarningTitle";
            this.lblWarningTitle.Size = new System.Drawing.Size(250, 21);
            this.lblWarningTitle.TabIndex = 0;
            this.lblWarningTitle.Text = "Important — irreversible operation";
            // 
            // lblWarningText
            // 
            appearance16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(45)))), ((int)(((byte)(18)))));
            this.lblWarningText.Appearance = appearance16;
            this.lblWarningText.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblWarningText.Location = new System.Drawing.Point(18, 27);
            this.lblWarningText.Name = "lblWarningText";
            this.lblWarningText.Size = new System.Drawing.Size(880, 20);
            this.lblWarningText.TabIndex = 1;
            this.lblWarningText.Text = "All counter sessions must be closed. Verify dates and ensure a tested database ba" +
    "ckup exists before continuing.";
            // 
            // groupBoxChecks
            // 
            this.groupBoxChecks.Appearance = appearance4;
            this.groupBoxChecks.Controls.Add(this.lstChecks);
            this.groupBoxChecks.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.groupBoxChecks.HeaderAppearance = appearance9;
            this.groupBoxChecks.Location = new System.Drawing.Point(28, 296);
            this.groupBoxChecks.Name = "groupBoxChecks";
            this.groupBoxChecks.Size = new System.Drawing.Size(924, 126);
            this.groupBoxChecks.TabIndex = 4;
            this.groupBoxChecks.Text = "Pre-closing validation results";
            // 
            // lstChecks
            // 
            this.lstChecks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.lstChecks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstChecks.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lstChecks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
            this.lstChecks.FormattingEnabled = true;
            this.lstChecks.ItemHeight = 17;
            this.lstChecks.Location = new System.Drawing.Point(18, 30);
            this.lstChecks.Name = "lstChecks";
            this.lstChecks.Size = new System.Drawing.Size(886, 70);
            this.lstChecks.TabIndex = 0;
            // 
            // btnVerify
            // 
            appearance17.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            appearance17.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            appearance17.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(64)))), ((int)(((byte)(175)))));
            appearance17.TextHAlignAsString = "Center";
            this.btnVerify.Appearance = appearance17;
            this.btnVerify.ButtonStyle = Infragistics.Win.UIElementButtonStyle.FlatBorderless;
            this.btnVerify.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnVerify.Location = new System.Drawing.Point(28, 436);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new System.Drawing.Size(190, 46);
            this.btnVerify.TabIndex = 5;
            this.btnVerify.Text = "Run verifications";
            this.btnVerify.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnVerify.Click += new System.EventHandler(this.btnVerify_Click);
            // 
            // btnRunClosing
            // 
            appearance18.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            appearance18.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(128)))), ((int)(((byte)(61)))));
            appearance18.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance18.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(128)))), ((int)(((byte)(61)))));
            appearance18.TextHAlignAsString = "Center";
            this.btnRunClosing.Appearance = appearance18;
            this.btnRunClosing.ButtonStyle = Infragistics.Win.UIElementButtonStyle.FlatBorderless;
            this.btnRunClosing.Enabled = false;
            this.btnRunClosing.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnRunClosing.Location = new System.Drawing.Point(232, 436);
            this.btnRunClosing.Name = "btnRunClosing";
            this.btnRunClosing.Size = new System.Drawing.Size(248, 46);
            this.btnRunClosing.TabIndex = 6;
            this.btnRunClosing.Text = "Perform year-end closing";
            this.btnRunClosing.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnRunClosing.Click += new System.EventHandler(this.btnRunClosing_Click);
            // 
            // btnClose
            // 
            appearance19.BackColor = System.Drawing.Color.White;
            appearance19.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            appearance19.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.btnClose.Appearance = appearance19;
            this.btnClose.ButtonStyle = Infragistics.Win.UIElementButtonStyle.FlatBorderless;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnClose.Location = new System.Drawing.Point(802, 436);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(150, 46);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Close";
            this.btnClose.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(28, 511);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(924, 9);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 9;
            this.progressBar.Visible = false;
            // 
            // lblProgressStatus
            // 
            appearance20.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblProgressStatus.Appearance = appearance20;
            this.lblProgressStatus.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Italic);
            this.lblProgressStatus.Location = new System.Drawing.Point(28, 489);
            this.lblProgressStatus.Name = "lblProgressStatus";
            this.lblProgressStatus.Size = new System.Drawing.Size(924, 22);
            this.lblProgressStatus.TabIndex = 8;
            this.lblProgressStatus.Text = "Status: Ready";
            this.lblProgressStatus.Visible = false;
            // 
            // FrmFinancialYearClosing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.ClientSize = new System.Drawing.Size(980, 532);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblProgressStatus);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRunClosing);
            this.Controls.Add(this.btnVerify);
            this.Controls.Add(this.groupBoxChecks);
            this.Controls.Add(this.pnlWarning);
            this.Controls.Add(this.groupBoxNew);
            this.Controls.Add(this.groupBoxCurrent);
            this.Controls.Add(this.panelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(900, 500);
            this.Name = "FrmFinancialYearClosing";
            this.Text = "Financial Year Closing";
            this.Load += new System.EventHandler(this.FrmFinancialYearClosing_Load);
            this.panelHeader.ClientArea.ResumeLayout(false);
            this.panelHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxCurrent)).EndInit();
            this.groupBoxCurrent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxNew)).EndInit();
            this.groupBoxNew.ResumeLayout(false);
            this.groupBoxNew.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtpNewTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpNewFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNewId)).EndInit();
            this.pnlWarning.ClientArea.ResumeLayout(false);
            this.pnlWarning.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxChecks)).EndInit();
            this.groupBoxChecks.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel panelHeader;
        private Infragistics.Win.Misc.UltraLabel lblTitle;
        private Infragistics.Win.Misc.UltraLabel lblSubtitle;
        private Infragistics.Win.Misc.UltraGroupBox groupBoxCurrent;
        private Infragistics.Win.Misc.UltraLabel lblCurrentBadge;
        private Infragistics.Win.Misc.UltraLabel lblCurTo;
        private Infragistics.Win.Misc.UltraLabel lblCurFrom;
        private Infragistics.Win.Misc.UltraLabel lblCurId;
        private Infragistics.Win.Misc.UltraGroupBox groupBoxNew;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpNewTo;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpNewFrom;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtNewId;
        private Infragistics.Win.Misc.UltraLabel label3;
        private Infragistics.Win.Misc.UltraLabel label2;
        private Infragistics.Win.Misc.UltraLabel label1;
        private Infragistics.Win.Misc.UltraPanel pnlWarning;
        private Infragistics.Win.Misc.UltraLabel lblWarningTitle;
        private Infragistics.Win.Misc.UltraLabel lblWarningText;
        private Infragistics.Win.Misc.UltraGroupBox groupBoxChecks;
        private System.Windows.Forms.ListBox lstChecks;
        private Infragistics.Win.Misc.UltraButton btnVerify;
        private Infragistics.Win.Misc.UltraButton btnRunClosing;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private System.Windows.Forms.ProgressBar progressBar;
        private Infragistics.Win.Misc.UltraLabel lblProgressStatus;
    }
}
