namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmBalanceSheet
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
            Infragistics.Win.Appearance appearanceAction = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceMaster = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGridL = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGridA = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceHeaderL = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceHeaderA = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceSelected = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGridFooter = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceHdrLiab = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceHdrAsset = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceNetProfit = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceDifference = new Infragistics.Win.Appearance();

            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.lblPreset = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboPresetDates = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.ultraDateTimeFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.ultraDateTimeTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();

            this.ultraPanelAction = new Infragistics.Win.Misc.UltraPanel();
            this.btnGenerate = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnToggleSelection = new Infragistics.Win.Misc.UltraButton();

            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            
            // Left (Liabilities)
            this.pnlLiabilitiesContainer = new Infragistics.Win.Misc.UltraPanel();
            this.pnlLiabilitiesHeader = new Infragistics.Win.Misc.UltraPanel();
            this.lblLiabilitiesTitle = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGridLiabilities = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelNetProfit = new Infragistics.Win.Misc.UltraPanel();
            this.lblNetProfitCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblNetProfitValue = new Infragistics.Win.Misc.UltraLabel();

            // Right (Assets)
            this.pnlAssetsContainer = new Infragistics.Win.Misc.UltraPanel();
            this.pnlAssetsHeader = new Infragistics.Win.Misc.UltraPanel();
            this.lblAssetsTitle = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGridAssets = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelDifference = new Infragistics.Win.Misc.UltraPanel();
            this.lblDifferenceCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblDifferenceValue = new Infragistics.Win.Misc.UltraLabel();

            // Footer
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.lblTotalLiabilities = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCapital = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalAssets = new Infragistics.Win.Misc.UltraLabel();
            this.lblDifferenceBadge = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPresetDates)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).BeginInit();

            this.ultraPanelAction.ClientArea.SuspendLayout();
            this.ultraPanelAction.SuspendLayout();

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
            this.panelNetProfit.ClientArea.SuspendLayout();
            this.panelNetProfit.SuspendLayout();

            this.pnlAssetsContainer.ClientArea.SuspendLayout();
            this.pnlAssetsContainer.SuspendLayout();
            this.pnlAssetsHeader.ClientArea.SuspendLayout();
            this.pnlAssetsHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridAssets)).BeginInit();
            this.panelDifference.ClientArea.SuspendLayout();
            this.panelDifference.SuspendLayout();

            this.ultraPanelGridFooter.ClientArea.SuspendLayout();
            this.ultraPanelGridFooter.SuspendLayout();
            this.SuspendLayout();

            // 
            // ultraPanelControls
            // 
            appearanceControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearanceControls.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelControls.Appearance = appearanceControls;
            this.ultraPanelControls.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelControls.ClientArea
            // 
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboPresetDates);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraDateTimeFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraDateTimeTo);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1200, 58);
            this.ultraPanelControls.TabIndex = 0;

            // 
            // lblPreset
            // 
            this.lblPreset.Location = new System.Drawing.Point(20, 18);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(40, 20);
            this.lblPreset.TabIndex = 0;
            this.lblPreset.Text = "Period";

            // 
            // ultraComboPresetDates
            // 
            this.ultraComboPresetDates.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboPresetDates.Location = new System.Drawing.Point(65, 15);
            this.ultraComboPresetDates.Name = "ultraComboPresetDates";
            this.ultraComboPresetDates.Size = new System.Drawing.Size(160, 24);
            this.ultraComboPresetDates.TabIndex = 1;
            this.ultraComboPresetDates.ValueChanged += new System.EventHandler(this.UltraComboPresetDates_ValueChanged);

            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(245, 18);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(40, 20);
            this.lblFromDate.TabIndex = 2;
            this.lblFromDate.Text = "From";

            // 
            // ultraDateTimeFrom
            // 
            this.ultraDateTimeFrom.FormatString = "dd-MM-yyyy";
            this.ultraDateTimeFrom.Location = new System.Drawing.Point(290, 15);
            this.ultraDateTimeFrom.Name = "ultraDateTimeFrom";
            this.ultraDateTimeFrom.Size = new System.Drawing.Size(120, 24);
            this.ultraDateTimeFrom.TabIndex = 3;

            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(425, 18);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(25, 20);
            this.lblToDate.TabIndex = 4;
            this.lblToDate.Text = "To";

            // 
            // ultraDateTimeTo
            // 
            this.ultraDateTimeTo.FormatString = "dd-MM-yyyy";
            this.ultraDateTimeTo.Location = new System.Drawing.Point(455, 15);
            this.ultraDateTimeTo.Name = "ultraDateTimeTo";
            this.ultraDateTimeTo.Size = new System.Drawing.Size(120, 24);
            this.ultraDateTimeTo.TabIndex = 5;

            // 
            // ultraPanelAction
            // 
            appearanceAction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(223)))), ((int)(((byte)(238)))));
            appearanceAction.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelAction.Appearance = appearanceAction;
            this.ultraPanelAction.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelAction.ClientArea
            // 
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnGenerate);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPrint);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnExport);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnClearFilters);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnToggleSelection);
            this.ultraPanelAction.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelAction.Location = new System.Drawing.Point(0, 58);
            this.ultraPanelAction.Name = "ultraPanelAction";
            this.ultraPanelAction.Size = new System.Drawing.Size(1200, 45);
            this.ultraPanelAction.TabIndex = 1;

            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(10, 8);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(120, 28);
            this.btnGenerate.TabIndex = 0;
            this.btnGenerate.Text = "View Grid";
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);

            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(136, 8);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(120, 28);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid";
            this.btnPreviewGrid.Click += new System.EventHandler(this.btnPreviewGrid_Click);

            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(262, 8);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(130, 28);
            this.btnPrint.TabIndex = 2;
            this.btnPrint.Text = "Preview Report";
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);

            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(398, 8);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(110, 28);
            this.btnExport.TabIndex = 3;
            this.btnExport.Text = "Export Grid";
            this.btnExport.Click += new System.EventHandler(this.btnExportCsv_Click);

            // 
            // btnClearFilters
            // 
            this.btnClearFilters.Location = new System.Drawing.Point(514, 8);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(110, 28);
            this.btnClearFilters.TabIndex = 4;
            this.btnClearFilters.Text = "Reset Filters";
            this.btnClearFilters.Click += new System.EventHandler(this.btnClearFilters_Click);

            // 
            // btnToggleSelection
            // 
            this.btnToggleSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleSelection.Location = new System.Drawing.Point(1055, 8);
            this.btnToggleSelection.Name = "btnToggleSelection";
            this.btnToggleSelection.Size = new System.Drawing.Size(135, 28);
            this.btnToggleSelection.TabIndex = 5;
            this.btnToggleSelection.Text = "Hide Selection";
            this.btnToggleSelection.Click += new System.EventHandler(this.btnToggleSelection_Click);

            // 
            // ultraPanelMaster
            // 
            appearanceMaster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearanceMaster.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelMaster.Appearance = appearanceMaster;
            this.ultraPanelMaster.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelMaster.ClientArea
            // 
            this.ultraPanelMaster.ClientArea.Controls.Add(this.splitContainerMain);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 103);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1200, 597);
            this.ultraPanelMaster.TabIndex = 2;

            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Size = new System.Drawing.Size(1200, 563);
            this.splitContainerMain.SplitterDistance = 595;
            this.splitContainerMain.SplitterWidth = 6;
            this.splitContainerMain.TabIndex = 0;

            // 
            // splitContainerMain.Panel1 (Liabilities)
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.pnlLiabilitiesContainer);
            this.splitContainerMain.Panel1.Padding = new System.Windows.Forms.Padding(4, 4, 2, 4);

            // 
            // pnlLiabilitiesContainer
            // 
            this.pnlLiabilitiesContainer.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.pnlLiabilitiesContainer.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.pnlLiabilitiesContainer.ClientArea.Controls.Add(this.ultraGridLiabilities);
            this.pnlLiabilitiesContainer.ClientArea.Controls.Add(this.panelNetProfit);
            this.pnlLiabilitiesContainer.ClientArea.Controls.Add(this.pnlLiabilitiesHeader);
            this.pnlLiabilitiesContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLiabilitiesContainer.Location = new System.Drawing.Point(4, 4);
            this.pnlLiabilitiesContainer.Name = "pnlLiabilitiesContainer";
            this.pnlLiabilitiesContainer.Size = new System.Drawing.Size(589, 555);
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
            this.pnlLiabilitiesHeader.Size = new System.Drawing.Size(589, 28);
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
            this.lblLiabilitiesTitle.Size = new System.Drawing.Size(581, 28);
            this.lblLiabilitiesTitle.TabIndex = 0;
            this.lblLiabilitiesTitle.Text = "  LIABILITIES & CAPITAL";

            // 
            // ultraGridLiabilities
            // 
            appearanceGridL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ultraGridLiabilities.DisplayLayout.Appearance = appearanceGridL;
            this.ultraGridLiabilities.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            appearanceHeaderL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceHeaderL.ForeColor = System.Drawing.Color.White;
            this.ultraGridLiabilities.DisplayLayout.Override.HeaderAppearance = appearanceHeaderL;
            this.ultraGridLiabilities.DisplayLayout.Override.SelectedRowAppearance = appearanceSelected;
            this.ultraGridLiabilities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridLiabilities.Location = new System.Drawing.Point(0, 28);
            this.ultraGridLiabilities.Name = "ultraGridLiabilities";
            this.ultraGridLiabilities.Size = new System.Drawing.Size(589, 497);
            this.ultraGridLiabilities.TabIndex = 1;
            this.ultraGridLiabilities.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // 
            // panelNetProfit
            // 
            appearanceNetProfit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(245)))), ((int)(((byte)(233)))));
            appearanceNetProfit.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.panelNetProfit.Appearance = appearanceNetProfit;
            this.panelNetProfit.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.panelNetProfit.ClientArea.Controls.Add(this.lblNetProfitValue);
            this.panelNetProfit.ClientArea.Controls.Add(this.lblNetProfitCaption);
            this.panelNetProfit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelNetProfit.Location = new System.Drawing.Point(0, 525);
            this.panelNetProfit.Name = "panelNetProfit";
            this.panelNetProfit.Size = new System.Drawing.Size(589, 30);
            this.panelNetProfit.TabIndex = 2;

            // 
            // lblNetProfitCaption
            // 
            this.lblNetProfitCaption.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblNetProfitCaption.Location = new System.Drawing.Point(10, 6);
            this.lblNetProfitCaption.Name = "lblNetProfitCaption";
            this.lblNetProfitCaption.Size = new System.Drawing.Size(180, 20);
            this.lblNetProfitCaption.TabIndex = 0;
            this.lblNetProfitCaption.Text = "★ NET PROFIT:";

            // 
            // lblNetProfitValue
            // 
            this.lblNetProfitValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNetProfitValue.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblNetProfitValue.Location = new System.Drawing.Point(380, 5);
            this.lblNetProfitValue.Name = "lblNetProfitValue";
            this.lblNetProfitValue.Size = new System.Drawing.Size(200, 22);
            this.lblNetProfitValue.Appearance.TextHAlign = Infragistics.Win.HAlign.Right;
            this.lblNetProfitValue.TabIndex = 1;
            this.lblNetProfitValue.Text = "₹ 0.00";

            // 
            // splitContainerMain.Panel2 (Assets)
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.pnlAssetsContainer);
            this.splitContainerMain.Panel2.Padding = new System.Windows.Forms.Padding(2, 4, 4, 4);

            // 
            // pnlAssetsContainer
            // 
            this.pnlAssetsContainer.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.pnlAssetsContainer.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.pnlAssetsContainer.ClientArea.Controls.Add(this.ultraGridAssets);
            this.pnlAssetsContainer.ClientArea.Controls.Add(this.panelDifference);
            this.pnlAssetsContainer.ClientArea.Controls.Add(this.pnlAssetsHeader);
            this.pnlAssetsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAssetsContainer.Location = new System.Drawing.Point(2, 4);
            this.pnlAssetsContainer.Name = "pnlAssetsContainer";
            this.pnlAssetsContainer.Size = new System.Drawing.Size(593, 555);
            this.pnlAssetsContainer.TabIndex = 0;

            // 
            // pnlAssetsHeader
            // 
            appearanceHdrAsset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceHdrAsset.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(118)))), ((int)(((byte)(184)))));
            appearanceHdrAsset.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.pnlAssetsHeader.Appearance = appearanceHdrAsset;
            this.pnlAssetsHeader.ClientArea.Controls.Add(this.lblAssetsTitle);
            this.pnlAssetsHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAssetsHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlAssetsHeader.Name = "pnlAssetsHeader";
            this.pnlAssetsHeader.Size = new System.Drawing.Size(593, 28);
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
            this.lblAssetsTitle.Size = new System.Drawing.Size(585, 28);
            this.lblAssetsTitle.TabIndex = 0;
            this.lblAssetsTitle.Text = "  ASSETS";

            // 
            // ultraGridAssets
            // 
            appearanceGridA.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ultraGridAssets.DisplayLayout.Appearance = appearanceGridA;
            this.ultraGridAssets.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            appearanceHeaderA.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceHeaderA.ForeColor = System.Drawing.Color.White;
            this.ultraGridAssets.DisplayLayout.Override.HeaderAppearance = appearanceHeaderA;
            this.ultraGridAssets.DisplayLayout.Override.SelectedRowAppearance = appearanceSelected;
            this.ultraGridAssets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridAssets.Location = new System.Drawing.Point(0, 28);
            this.ultraGridAssets.Name = "ultraGridAssets";
            this.ultraGridAssets.Size = new System.Drawing.Size(593, 497);
            this.ultraGridAssets.TabIndex = 1;
            this.ultraGridAssets.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // 
            // panelDifference
            // 
            appearanceDifference.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(245)))), ((int)(((byte)(233)))));
            appearanceDifference.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.panelDifference.Appearance = appearanceDifference;
            this.panelDifference.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.panelDifference.ClientArea.Controls.Add(this.lblDifferenceValue);
            this.panelDifference.ClientArea.Controls.Add(this.lblDifferenceCaption);
            this.panelDifference.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelDifference.Location = new System.Drawing.Point(0, 525);
            this.panelDifference.Name = "panelDifference";
            this.panelDifference.Size = new System.Drawing.Size(593, 30);
            this.panelDifference.TabIndex = 2;

            // 
            // lblDifferenceCaption
            // 
            this.lblDifferenceCaption.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDifferenceCaption.Location = new System.Drawing.Point(10, 6);
            this.lblDifferenceCaption.Name = "lblDifferenceCaption";
            this.lblDifferenceCaption.Size = new System.Drawing.Size(220, 20);
            this.lblDifferenceCaption.TabIndex = 0;
            this.lblDifferenceCaption.Text = "DIFFERENCE (Books Balance):";

            // 
            // lblDifferenceValue
            // 
            this.lblDifferenceValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDifferenceValue.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDifferenceValue.Location = new System.Drawing.Point(380, 5);
            this.lblDifferenceValue.Name = "lblDifferenceValue";
            this.lblDifferenceValue.Size = new System.Drawing.Size(200, 22);
            this.lblDifferenceValue.Appearance.TextHAlign = Infragistics.Win.HAlign.Right;
            this.lblDifferenceValue.TabIndex = 1;
            this.lblDifferenceValue.Text = "₹ 0.00";

            // 
            // appearanceSelected
            // 
            appearanceSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(126)))), ((int)(((byte)(245)))));
            appearanceSelected.ForeColor = System.Drawing.Color.White;

            // 
            // ultraPanelGridFooter
            // 
            appearanceGridFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceGridFooter.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(118)))), ((int)(((byte)(184)))));
            appearanceGridFooter.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceGridFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelGridFooter.Appearance = appearanceGridFooter;
            this.ultraPanelGridFooter.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelGridFooter.ClientArea
            // 
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblTotalLiabilities);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblTotalCapital);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblTotalAssets);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblDifferenceBadge);
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 563);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1200, 34);
            this.ultraPanelGridFooter.TabIndex = 1;

            // 
            // lblTotalLiabilities
            // 
            this.lblTotalLiabilities.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalLiabilities.Location = new System.Drawing.Point(12, 6);
            this.lblTotalLiabilities.Name = "lblTotalLiabilities";
            this.lblTotalLiabilities.Size = new System.Drawing.Size(230, 22);
            this.lblTotalLiabilities.TabIndex = 0;
            this.lblTotalLiabilities.Text = "Total Liabilities: ₹ 0.00";

            // 
            // lblTotalCapital
            // 
            this.lblTotalCapital.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalCapital.Location = new System.Drawing.Point(255, 6);
            this.lblTotalCapital.Name = "lblTotalCapital";
            this.lblTotalCapital.Size = new System.Drawing.Size(230, 22);
            this.lblTotalCapital.TabIndex = 1;
            this.lblTotalCapital.Text = "Total Capital: ₹ 0.00";

            // 
            // lblTotalAssets
            // 
            this.lblTotalAssets.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalAssets.Location = new System.Drawing.Point(500, 6);
            this.lblTotalAssets.Name = "lblTotalAssets";
            this.lblTotalAssets.Size = new System.Drawing.Size(230, 22);
            this.lblTotalAssets.TabIndex = 2;
            this.lblTotalAssets.Text = "Total Assets: ₹ 0.00";

            // 
            // lblDifferenceBadge
            // 
            this.lblDifferenceBadge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDifferenceBadge.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDifferenceBadge.Location = new System.Drawing.Point(820, 5);
            this.lblDifferenceBadge.Name = "lblDifferenceBadge";
            this.lblDifferenceBadge.Size = new System.Drawing.Size(370, 24);
            this.lblDifferenceBadge.TabIndex = 3;
            this.lblDifferenceBadge.Text = "★ BALANCED: ₹ 0.00";

            // 
            // FrmBalanceSheet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelAction);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmBalanceSheet";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Balance Sheet";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPresetDates)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).EndInit();

            this.ultraPanelAction.ClientArea.ResumeLayout(false);
            this.ultraPanelAction.ResumeLayout(false);

            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);

            this.pnlLiabilitiesHeader.ClientArea.ResumeLayout(false);
            this.pnlLiabilitiesHeader.ResumeLayout(false);
            this.panelNetProfit.ClientArea.ResumeLayout(false);
            this.panelNetProfit.ResumeLayout(false);
            this.pnlLiabilitiesContainer.ClientArea.ResumeLayout(false);
            this.pnlLiabilitiesContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridLiabilities)).EndInit();

            this.pnlAssetsHeader.ClientArea.ResumeLayout(false);
            this.pnlAssetsHeader.ResumeLayout(false);
            this.panelDifference.ClientArea.ResumeLayout(false);
            this.panelDifference.ResumeLayout(false);
            this.pnlAssetsContainer.ClientArea.ResumeLayout(false);
            this.pnlAssetsContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridAssets)).EndInit();

            this.ultraPanelGridFooter.ClientArea.ResumeLayout(false);
            this.ultraPanelGridFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraLabel lblPreset;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboPresetDates;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeFrom;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeTo;

        private Infragistics.Win.Misc.UltraPanel ultraPanelAction;
        private Infragistics.Win.Misc.UltraButton btnGenerate;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnToggleSelection;

        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        
        private Infragistics.Win.Misc.UltraPanel pnlLiabilitiesContainer;
        private Infragistics.Win.Misc.UltraPanel pnlLiabilitiesHeader;
        private Infragistics.Win.Misc.UltraLabel lblLiabilitiesTitle;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridLiabilities;
        private Infragistics.Win.Misc.UltraPanel panelNetProfit;
        private Infragistics.Win.Misc.UltraLabel lblNetProfitCaption;
        private Infragistics.Win.Misc.UltraLabel lblNetProfitValue;

        private Infragistics.Win.Misc.UltraPanel pnlAssetsContainer;
        private Infragistics.Win.Misc.UltraPanel pnlAssetsHeader;
        private Infragistics.Win.Misc.UltraLabel lblAssetsTitle;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridAssets;
        private Infragistics.Win.Misc.UltraPanel panelDifference;
        private Infragistics.Win.Misc.UltraLabel lblDifferenceCaption;
        private Infragistics.Win.Misc.UltraLabel lblDifferenceValue;

        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
        private Infragistics.Win.Misc.UltraLabel lblTotalLiabilities;
        private Infragistics.Win.Misc.UltraLabel lblTotalCapital;
        private Infragistics.Win.Misc.UltraLabel lblTotalAssets;
        private Infragistics.Win.Misc.UltraLabel lblDifferenceBadge;
    }
}
