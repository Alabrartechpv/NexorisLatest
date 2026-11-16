namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmDailyFinancialMovement
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
            Infragistics.Win.Appearance appearanceControls = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceMaster = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceHdrLiab = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceHdrAssets = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGridL = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceHeaderL = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGridA = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceHeaderA = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceFooter = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceSelected = new Infragistics.Win.Appearance();

            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFromDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtToDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblDaySelect = new Infragistics.Win.Misc.UltraLabel();
            this.cmbDaySelect = new System.Windows.Forms.ComboBox();
            this.btnToday = new System.Windows.Forms.Button();
            this.btnYesterday = new System.Windows.Forms.Button();
            this.btnLast7Days = new System.Windows.Forms.Button();
            this.btnThisMonth = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();

            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();

            // Left Side: Liabilities
            this.pnlLiabilitiesContainer = new Infragistics.Win.Misc.UltraPanel();
            this.pnlLiabilitiesHeader = new Infragistics.Win.Misc.UltraPanel();
            this.lblLiabilitiesTitle = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGridLiabilities = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelLiabilitiesTotal = new Infragistics.Win.Misc.UltraPanel();
            this.lblLiabTotalCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblLiabTotalValue = new Infragistics.Win.Misc.UltraLabel();

            // Right Side: Assets
            this.pnlAssetsContainer = new Infragistics.Win.Misc.UltraPanel();
            this.pnlAssetsHeader = new Infragistics.Win.Misc.UltraPanel();
            this.lblAssetsTitle = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGridAssets = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelAssetsTotal = new Infragistics.Win.Misc.UltraPanel();
            this.lblAssetsTotalCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblAssetsTotalValue = new Infragistics.Win.Misc.UltraLabel();

            // Bottom Footer (Exact Balance Sheet Footer)
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.lblTotalLiabilities = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalAssets = new Infragistics.Win.Misc.UltraLabel();
            this.lblNetPositionBadge = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).BeginInit();
            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.pnlLiabilitiesContainer.ClientArea.SuspendLayout();
            this.pnlLiabilitiesContainer.SuspendLayout();
            this.pnlLiabilitiesHeader.ClientArea.SuspendLayout();
            this.pnlLiabilitiesHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridLiabilities)).BeginInit();
            this.panelLiabilitiesTotal.ClientArea.SuspendLayout();
            this.panelLiabilitiesTotal.SuspendLayout();
            this.pnlAssetsContainer.ClientArea.SuspendLayout();
            this.pnlAssetsContainer.SuspendLayout();
            this.pnlAssetsHeader.ClientArea.SuspendLayout();
            this.pnlAssetsHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridAssets)).BeginInit();
            this.panelAssetsTotal.ClientArea.SuspendLayout();
            this.panelAssetsTotal.SuspendLayout();
            this.ultraPanelGridFooter.ClientArea.SuspendLayout();
            this.ultraPanelGridFooter.SuspendLayout();
            this.SuspendLayout();

            // 
            // ultraPanelControls (Top Filter Bar)
            // 
            appearanceControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearanceControls.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelControls.Appearance = appearanceControls;
            this.ultraPanelControls.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnPrint);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnExportCsv);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnGenerate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnThisMonth);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnLast7Days);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnYesterday);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnToday);
            this.ultraPanelControls.ClientArea.Controls.Add(this.cmbDaySelect);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblDaySelect);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1264, 46);
            this.ultraPanelControls.TabIndex = 0;

            // 
            // lblFromDate
            // 
            this.lblFromDate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFromDate.Location = new System.Drawing.Point(12, 14);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(42, 20);
            this.lblFromDate.TabIndex = 0;
            this.lblFromDate.Text = "From:";

            // 
            // dtFromDate
            // 
            this.dtFromDate.DateTime = new System.DateTime(2026, 11, 1, 0, 0, 0, 0);
            this.dtFromDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtFromDate.FormatString = "dd-MM-yyyy";
            this.dtFromDate.Location = new System.Drawing.Point(56, 10);
            this.dtFromDate.Name = "dtFromDate";
            this.dtFromDate.Size = new System.Drawing.Size(100, 24);
            this.dtFromDate.TabIndex = 1;

            // 
            // lblToDate
            // 
            this.lblToDate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblToDate.Location = new System.Drawing.Point(164, 14);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(26, 20);
            this.lblToDate.TabIndex = 2;
            this.lblToDate.Text = "To:";

            // 
            // dtToDate
            // 
            this.dtToDate.DateTime = new System.DateTime(2026, 11, 13, 0, 0, 0, 0);
            this.dtToDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtToDate.FormatString = "dd-MM-yyyy";
            this.dtToDate.Location = new System.Drawing.Point(192, 10);
            this.dtToDate.Name = "dtToDate";
            this.dtToDate.Size = new System.Drawing.Size(100, 24);
            this.dtToDate.TabIndex = 3;

            // 
            // lblDaySelect
            // 
            this.lblDaySelect.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDaySelect.Location = new System.Drawing.Point(302, 14);
            this.lblDaySelect.Name = "lblDaySelect";
            this.lblDaySelect.Size = new System.Drawing.Size(70, 20);
            this.lblDaySelect.TabIndex = 4;
            this.lblDaySelect.Text = "Show Day:";

            // 
            // cmbDaySelect
            // 
            this.cmbDaySelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDaySelect.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbDaySelect.FormattingEnabled = true;
            this.cmbDaySelect.Location = new System.Drawing.Point(374, 10);
            this.cmbDaySelect.Name = "cmbDaySelect";
            this.cmbDaySelect.Size = new System.Drawing.Size(165, 23);
            this.cmbDaySelect.TabIndex = 5;

            // 
            // btnToday
            // 
            this.btnToday.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(252)))));
            this.btnToday.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToday.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnToday.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(47)))), ((int)(((byte)(108)))));
            this.btnToday.Location = new System.Drawing.Point(548, 9);
            this.btnToday.Name = "btnToday";
            this.btnToday.Size = new System.Drawing.Size(52, 25);
            this.btnToday.TabIndex = 6;
            this.btnToday.Text = "Today";
            this.btnToday.UseVisualStyleBackColor = false;

            // 
            // btnYesterday
            // 
            this.btnYesterday.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(252)))));
            this.btnYesterday.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnYesterday.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnYesterday.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(47)))), ((int)(((byte)(108)))));
            this.btnYesterday.Location = new System.Drawing.Point(604, 9);
            this.btnYesterday.Name = "btnYesterday";
            this.btnYesterday.Size = new System.Drawing.Size(68, 25);
            this.btnYesterday.TabIndex = 7;
            this.btnYesterday.Text = "Yesterday";
            this.btnYesterday.UseVisualStyleBackColor = false;

            // 
            // btnLast7Days
            // 
            this.btnLast7Days.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(252)))));
            this.btnLast7Days.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLast7Days.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnLast7Days.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(47)))), ((int)(((byte)(108)))));
            this.btnLast7Days.Location = new System.Drawing.Point(676, 9);
            this.btnLast7Days.Name = "btnLast7Days";
            this.btnLast7Days.Size = new System.Drawing.Size(75, 25);
            this.btnLast7Days.TabIndex = 8;
            this.btnLast7Days.Text = "Last 7 Days";
            this.btnLast7Days.UseVisualStyleBackColor = false;

            // 
            // btnThisMonth
            // 
            this.btnThisMonth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(252)))));
            this.btnThisMonth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnThisMonth.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnThisMonth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(47)))), ((int)(((byte)(108)))));
            this.btnThisMonth.Location = new System.Drawing.Point(755, 9);
            this.btnThisMonth.Name = "btnThisMonth";
            this.btnThisMonth.Size = new System.Drawing.Size(78, 25);
            this.btnThisMonth.TabIndex = 9;
            this.btnThisMonth.Text = "This Month";
            this.btnThisMonth.UseVisualStyleBackColor = false;

            // 
            // btnGenerate
            // 
            this.btnGenerate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            this.btnGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerate.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnGenerate.ForeColor = System.Drawing.Color.White;
            this.btnGenerate.Location = new System.Drawing.Point(842, 8);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(95, 27);
            this.btnGenerate.TabIndex = 10;
            this.btnGenerate.Text = "View / Load";
            this.btnGenerate.UseVisualStyleBackColor = false;

            // 
            // btnExportCsv
            // 
            this.btnExportCsv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportCsv.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(252)))));
            this.btnExportCsv.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportCsv.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnExportCsv.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(47)))), ((int)(((byte)(108)))));
            this.btnExportCsv.Location = new System.Drawing.Point(1075, 9);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(85, 25);
            this.btnExportCsv.TabIndex = 11;
            this.btnExportCsv.Text = "Export CSV";
            this.btnExportCsv.UseVisualStyleBackColor = false;

            // 
            // btnPrint
            // 
            this.btnPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPrint.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(252)))));
            this.btnPrint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrint.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnPrint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(47)))), ((int)(((byte)(108)))));
            this.btnPrint.Location = new System.Drawing.Point(1166, 9);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(85, 25);
            this.btnPrint.TabIndex = 12;
            this.btnPrint.Text = "Print Preview";
            this.btnPrint.UseVisualStyleBackColor = false;

            // 
            // ultraPanelMaster (Main Container holding Split and Footer)
            // 
            appearanceMaster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearanceMaster.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelMaster.Appearance = appearanceMaster;
            this.ultraPanelMaster.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraPanelMaster.ClientArea.Controls.Add(this.splitContainerMain);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 46);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1264, 617);
            this.ultraPanelMaster.TabIndex = 1;

            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.pnlLiabilitiesContainer);
            this.splitContainerMain.Panel1.Padding = new System.Windows.Forms.Padding(4, 4, 2, 4);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.pnlAssetsContainer);
            this.splitContainerMain.Panel2.Padding = new System.Windows.Forms.Padding(2, 4, 4, 4);
            this.splitContainerMain.Size = new System.Drawing.Size(1264, 583);
            this.splitContainerMain.SplitterDistance = 630;
            this.splitContainerMain.SplitterWidth = 6;
            this.splitContainerMain.TabIndex = 0;

            // 
            // pnlLiabilitiesContainer
            // 
            this.pnlLiabilitiesContainer.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.pnlLiabilitiesContainer.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.pnlLiabilitiesContainer.ClientArea.Controls.Add(this.ultraGridLiabilities);
            this.pnlLiabilitiesContainer.ClientArea.Controls.Add(this.panelLiabilitiesTotal);
            this.pnlLiabilitiesContainer.ClientArea.Controls.Add(this.pnlLiabilitiesHeader);
            this.pnlLiabilitiesContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLiabilitiesContainer.Location = new System.Drawing.Point(4, 4);
            this.pnlLiabilitiesContainer.Name = "pnlLiabilitiesContainer";
            this.pnlLiabilitiesContainer.Size = new System.Drawing.Size(624, 575);
            this.pnlLiabilitiesContainer.TabIndex = 0;

            // 
            // pnlLiabilitiesHeader
            // 
            appearanceHdrLiab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceHdrLiab.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(118)))), ((int)(((byte)(184)))));
            appearanceHdrLiab.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.pnlLiabilitiesHeader.Appearance = appearanceHdrLiab;
            this.pnlLiabilitiesHeader.ClientArea.Controls.Add(this.lblLiabilitiesTitle);
            this.pnlLiabilitiesHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLiabilitiesHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlLiabilitiesHeader.Name = "pnlLiabilitiesHeader";
            this.pnlLiabilitiesHeader.Size = new System.Drawing.Size(624, 28);
            this.pnlLiabilitiesHeader.TabIndex = 0;

            // 
            // lblLiabilitiesTitle
            // 
            this.lblLiabilitiesTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLiabilitiesTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLiabilitiesTitle.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblLiabilitiesTitle.Appearance.TextHAlign = Infragistics.Win.HAlign.Left;
            this.lblLiabilitiesTitle.Appearance.TextVAlign = Infragistics.Win.VAlign.Middle;
            this.lblLiabilitiesTitle.Location = new System.Drawing.Point(8, 0);
            this.lblLiabilitiesTitle.Name = "lblLiabilitiesTitle";
            this.lblLiabilitiesTitle.Size = new System.Drawing.Size(616, 28);
            this.lblLiabilitiesTitle.TabIndex = 0;
            this.lblLiabilitiesTitle.Text = "  LIABILITIES & SUPPLIER DEBT (WHAT YOU OWE)";

            // 
            // ultraGridLiabilities
            // 
            appearanceGridL.BackColor = System.Drawing.Color.White;
            appearanceGridL.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraGridLiabilities.DisplayLayout.Appearance = appearanceGridL;
            this.ultraGridLiabilities.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            appearanceHeaderL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceHeaderL.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(118)))), ((int)(((byte)(184)))));
            appearanceHeaderL.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceHeaderL.ForeColor = System.Drawing.Color.White;
            this.ultraGridLiabilities.DisplayLayout.Override.HeaderAppearance = appearanceHeaderL;
            this.ultraGridLiabilities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridLiabilities.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ultraGridLiabilities.Location = new System.Drawing.Point(0, 28);
            this.ultraGridLiabilities.Name = "ultraGridLiabilities";
            this.ultraGridLiabilities.Size = new System.Drawing.Size(624, 517);
            this.ultraGridLiabilities.TabIndex = 1;
            this.ultraGridLiabilities.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // 
            // panelLiabilitiesTotal
            // 
            this.panelLiabilitiesTotal.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.panelLiabilitiesTotal.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.panelLiabilitiesTotal.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.panelLiabilitiesTotal.ClientArea.Controls.Add(this.lblLiabTotalValue);
            this.panelLiabilitiesTotal.ClientArea.Controls.Add(this.lblLiabTotalCaption);
            this.panelLiabilitiesTotal.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelLiabilitiesTotal.Location = new System.Drawing.Point(0, 545);
            this.panelLiabilitiesTotal.Name = "panelLiabilitiesTotal";
            this.panelLiabilitiesTotal.Size = new System.Drawing.Size(624, 30);
            this.panelLiabilitiesTotal.TabIndex = 2;

            // 
            // lblLiabTotalCaption
            // 
            this.lblLiabTotalCaption.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblLiabTotalCaption.Location = new System.Drawing.Point(10, 6);
            this.lblLiabTotalCaption.Name = "lblLiabTotalCaption";
            this.lblLiabTotalCaption.Size = new System.Drawing.Size(220, 20);
            this.lblLiabTotalCaption.TabIndex = 0;
            this.lblLiabTotalCaption.Text = "TOTAL LIABILITIES TONIGHT:";

            // 
            // lblLiabTotalValue
            // 
            this.lblLiabTotalValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLiabTotalValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.lblLiabTotalValue.Appearance.TextHAlign = Infragistics.Win.HAlign.Right;
            this.lblLiabTotalValue.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLiabTotalValue.Location = new System.Drawing.Point(380, 5);
            this.lblLiabTotalValue.Name = "lblLiabTotalValue";
            this.lblLiabTotalValue.Size = new System.Drawing.Size(230, 22);
            this.lblLiabTotalValue.TabIndex = 1;
            this.lblLiabTotalValue.Text = "₹ 0.00";

            // 
            // pnlAssetsContainer
            // 
            this.pnlAssetsContainer.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.pnlAssetsContainer.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.pnlAssetsContainer.ClientArea.Controls.Add(this.ultraGridAssets);
            this.pnlAssetsContainer.ClientArea.Controls.Add(this.panelAssetsTotal);
            this.pnlAssetsContainer.ClientArea.Controls.Add(this.pnlAssetsHeader);
            this.pnlAssetsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAssetsContainer.Location = new System.Drawing.Point(2, 4);
            this.pnlAssetsContainer.Name = "pnlAssetsContainer";
            this.pnlAssetsContainer.Size = new System.Drawing.Size(628, 575);
            this.pnlAssetsContainer.TabIndex = 0;

            // 
            // pnlAssetsHeader
            // 
            appearanceHdrAssets.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceHdrAssets.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(118)))), ((int)(((byte)(184)))));
            appearanceHdrAssets.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.pnlAssetsHeader.Appearance = appearanceHdrAssets;
            this.pnlAssetsHeader.ClientArea.Controls.Add(this.lblAssetsTitle);
            this.pnlAssetsHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAssetsHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlAssetsHeader.Name = "pnlAssetsHeader";
            this.pnlAssetsHeader.Size = new System.Drawing.Size(628, 28);
            this.pnlAssetsHeader.TabIndex = 0;

            // 
            // lblAssetsTitle
            // 
            this.lblAssetsTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAssetsTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblAssetsTitle.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblAssetsTitle.Appearance.TextHAlign = Infragistics.Win.HAlign.Left;
            this.lblAssetsTitle.Appearance.TextVAlign = Infragistics.Win.VAlign.Middle;
            this.lblAssetsTitle.Location = new System.Drawing.Point(8, 0);
            this.lblAssetsTitle.Name = "lblAssetsTitle";
            this.lblAssetsTitle.Size = new System.Drawing.Size(620, 28);
            this.lblAssetsTitle.TabIndex = 0;
            this.lblAssetsTitle.Text = "  ASSETS (WHAT YOU HAVE)";

            // 
            // ultraGridAssets
            // 
            appearanceGridA.BackColor = System.Drawing.Color.White;
            appearanceGridA.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraGridAssets.DisplayLayout.Appearance = appearanceGridA;
            this.ultraGridAssets.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            appearanceHeaderA.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceHeaderA.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(118)))), ((int)(((byte)(184)))));
            appearanceHeaderA.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceHeaderA.ForeColor = System.Drawing.Color.White;
            this.ultraGridAssets.DisplayLayout.Override.HeaderAppearance = appearanceHeaderA;
            this.ultraGridAssets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridAssets.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ultraGridAssets.Location = new System.Drawing.Point(0, 28);
            this.ultraGridAssets.Name = "ultraGridAssets";
            this.ultraGridAssets.Size = new System.Drawing.Size(628, 517);
            this.ultraGridAssets.TabIndex = 1;
            this.ultraGridAssets.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // 
            // panelAssetsTotal
            // 
            this.panelAssetsTotal.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.panelAssetsTotal.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(250)))), ((int)(((byte)(240)))));
            this.panelAssetsTotal.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.panelAssetsTotal.ClientArea.Controls.Add(this.lblAssetsTotalValue);
            this.panelAssetsTotal.ClientArea.Controls.Add(this.lblAssetsTotalCaption);
            this.panelAssetsTotal.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelAssetsTotal.Location = new System.Drawing.Point(0, 545);
            this.panelAssetsTotal.Name = "panelAssetsTotal";
            this.panelAssetsTotal.Size = new System.Drawing.Size(628, 30);
            this.panelAssetsTotal.TabIndex = 2;

            // 
            // lblAssetsTotalCaption
            // 
            this.lblAssetsTotalCaption.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAssetsTotalCaption.Location = new System.Drawing.Point(10, 6);
            this.lblAssetsTotalCaption.Name = "lblAssetsTotalCaption";
            this.lblAssetsTotalCaption.Size = new System.Drawing.Size(220, 20);
            this.lblAssetsTotalCaption.TabIndex = 0;
            this.lblAssetsTotalCaption.Text = "TOTAL ASSETS TONIGHT:";

            // 
            // lblAssetsTotalValue
            // 
            this.lblAssetsTotalValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAssetsTotalValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(94)))), ((int)(((byte)(32)))));
            this.lblAssetsTotalValue.Appearance.TextHAlign = Infragistics.Win.HAlign.Right;
            this.lblAssetsTotalValue.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblAssetsTotalValue.Location = new System.Drawing.Point(380, 5);
            this.lblAssetsTotalValue.Name = "lblAssetsTotalValue";
            this.lblAssetsTotalValue.Size = new System.Drawing.Size(235, 22);
            this.lblAssetsTotalValue.TabIndex = 1;
            this.lblAssetsTotalValue.Text = "₹ 0.00";

            // 
            // ultraPanelGridFooter (Exact Balance Sheet Footer)
            // 
            appearanceFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceFooter.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(118)))), ((int)(((byte)(184)))));
            appearanceFooter.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelGridFooter.Appearance = appearanceFooter;
            this.ultraPanelGridFooter.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblTotalLiabilities);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblTotalAssets);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblNetPositionBadge);
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 583);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1264, 34);
            this.ultraPanelGridFooter.TabIndex = 1;

            // 
            // lblTotalLiabilities
            // 
            this.lblTotalLiabilities.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalLiabilities.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblTotalLiabilities.Location = new System.Drawing.Point(12, 7);
            this.lblTotalLiabilities.Name = "lblTotalLiabilities";
            this.lblTotalLiabilities.Size = new System.Drawing.Size(260, 20);
            this.lblTotalLiabilities.TabIndex = 0;
            this.lblTotalLiabilities.Text = "Total Liabilities: ₹ 0.00";

            // 
            // lblTotalAssets
            // 
            this.lblTotalAssets.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalAssets.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblTotalAssets.Location = new System.Drawing.Point(310, 7);
            this.lblTotalAssets.Name = "lblTotalAssets";
            this.lblTotalAssets.Size = new System.Drawing.Size(260, 20);
            this.lblTotalAssets.TabIndex = 1;
            this.lblTotalAssets.Text = "Total Assets: ₹ 0.00";

            // 
            // lblNetPositionBadge
            // 
            this.lblNetPositionBadge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNetPositionBadge.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblNetPositionBadge.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblNetPositionBadge.Appearance.TextHAlign = Infragistics.Win.HAlign.Right;
            this.lblNetPositionBadge.Location = new System.Drawing.Point(680, 6);
            this.lblNetPositionBadge.Name = "lblNetPositionBadge";
            this.lblNetPositionBadge.Size = new System.Drawing.Size(570, 22);
            this.lblNetPositionBadge.TabIndex = 2;
            this.lblNetPositionBadge.Text = "NET STORE WORTH: ₹ 0.00 (GAIN: +₹ 0.00)";

            // 
            // FrmDailyFinancialMovement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1264, 663);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmDailyFinancialMovement";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Daily Movement (Balance Sheet View)";
            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).EndInit();
            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.pnlLiabilitiesContainer.ClientArea.ResumeLayout(false);
            this.pnlLiabilitiesContainer.ResumeLayout(false);
            this.pnlLiabilitiesHeader.ClientArea.ResumeLayout(false);
            this.pnlLiabilitiesHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridLiabilities)).EndInit();
            this.panelLiabilitiesTotal.ClientArea.ResumeLayout(false);
            this.panelLiabilitiesTotal.ResumeLayout(false);
            this.pnlAssetsContainer.ClientArea.ResumeLayout(false);
            this.pnlAssetsContainer.ResumeLayout(false);
            this.pnlAssetsHeader.ClientArea.ResumeLayout(false);
            this.pnlAssetsHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridAssets)).EndInit();
            this.panelAssetsTotal.ClientArea.ResumeLayout(false);
            this.panelAssetsTotal.ResumeLayout(false);
            this.ultraPanelGridFooter.ClientArea.ResumeLayout(false);
            this.ultraPanelGridFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFromDate;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtToDate;
        private Infragistics.Win.Misc.UltraLabel lblDaySelect;
        private System.Windows.Forms.ComboBox cmbDaySelect;
        private System.Windows.Forms.Button btnToday;
        private System.Windows.Forms.Button btnYesterday;
        private System.Windows.Forms.Button btnLast7Days;
        private System.Windows.Forms.Button btnThisMonth;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnExportCsv;
        private System.Windows.Forms.Button btnPrint;

        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private System.Windows.Forms.SplitContainer splitContainerMain;

        private Infragistics.Win.Misc.UltraPanel pnlLiabilitiesContainer;
        private Infragistics.Win.Misc.UltraPanel pnlLiabilitiesHeader;
        private Infragistics.Win.Misc.UltraLabel lblLiabilitiesTitle;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridLiabilities;
        private Infragistics.Win.Misc.UltraPanel panelLiabilitiesTotal;
        private Infragistics.Win.Misc.UltraLabel lblLiabTotalCaption;
        private Infragistics.Win.Misc.UltraLabel lblLiabTotalValue;

        private Infragistics.Win.Misc.UltraPanel pnlAssetsContainer;
        private Infragistics.Win.Misc.UltraPanel pnlAssetsHeader;
        private Infragistics.Win.Misc.UltraLabel lblAssetsTitle;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridAssets;
        private Infragistics.Win.Misc.UltraPanel panelAssetsTotal;
        private Infragistics.Win.Misc.UltraLabel lblAssetsTotalCaption;
        private Infragistics.Win.Misc.UltraLabel lblAssetsTotalValue;

        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
        private Infragistics.Win.Misc.UltraLabel lblTotalLiabilities;
        private Infragistics.Win.Misc.UltraLabel lblTotalAssets;
        private Infragistics.Win.Misc.UltraLabel lblNetPositionBadge;
    }
}
