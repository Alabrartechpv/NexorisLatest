namespace PosBranch_Win.Utilities
{
    partial class FrmExcelImport
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
            Infragistics.Win.UltraWinTabControl.UltraTab ultraTabImport = new Infragistics.Win.UltraWinTabControl.UltraTab();
            Infragistics.Win.UltraWinTabControl.UltraTab ultraTabExport = new Infragistics.Win.UltraWinTabControl.UltraTab();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance6 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance7 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance8 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance9 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance10 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance11 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance12 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance13 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance14 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance15 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance16 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance17 = new Infragistics.Win.Appearance();
            this.tabControlMain = new Infragistics.Win.UltraWinTabControl.UltraTabControl();
            this.ultraTabSharedControlsPage1 = new Infragistics.Win.UltraWinTabControl.UltraTabSharedControlsPage();
            this.tabImport = new Infragistics.Win.UltraWinTabControl.UltraTabPageControl();
            this.splitContainerImport = new System.Windows.Forms.SplitContainer();
            this.grpMapping = new Infragistics.Win.Misc.UltraGroupBox();
            this.pnlMappingGrid = new Infragistics.Win.Misc.UltraPanel();
            this.pnlAutoMapButton = new Infragistics.Win.Misc.UltraPanel();
            this.btnAutoMap = new Infragistics.Win.Misc.UltraButton();
            this.btnPreview = new Infragistics.Win.Misc.UltraButton();
            this.grpPreview = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraGridPreview = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.pnlPreviewBanner = new Infragistics.Win.Misc.UltraPanel();
            this.lblStats = new Infragistics.Win.Misc.UltraLabel();
            this.btnDownloadErrorLog = new Infragistics.Win.Misc.UltraButton();
            this.pnlImportProgress = new Infragistics.Win.Misc.UltraPanel();
            this.progressBarImport = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new Infragistics.Win.Misc.UltraLabel();
            this.btnImport = new Infragistics.Win.Misc.UltraButton();
            this.pnlOptions = new Infragistics.Win.Misc.UltraPanel();
            this.lblBackupWarning = new Infragistics.Win.Misc.UltraLabel();
            this.chkAutoGenerateBarcodes = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
            this.chkAutoCreate = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
            this.cmbDuplicateBehavior = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblDuplicateBehavior = new Infragistics.Win.Misc.UltraLabel();
            this.pnlFileSelection = new Infragistics.Win.Misc.UltraPanel();
            this.btnDownloadTemplate = new Infragistics.Win.Misc.UltraButton();
            this.btnLoad = new Infragistics.Win.Misc.UltraButton();
            this.btnBrowse = new Infragistics.Win.Misc.UltraButton();
            this.txtFilePath = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblFilePath = new Infragistics.Win.Misc.UltraLabel();
            this.tabExport = new Infragistics.Win.UltraWinTabControl.UltraTabPageControl();
            this.grpExportPreview = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraGridExportPreview = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.grpFilters = new Infragistics.Win.Misc.UltraGroupBox();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnLoadPreview = new Infragistics.Win.Misc.UltraButton();
            this.progressBarExport = new System.Windows.Forms.ProgressBar();
            this.lblProgressExport = new Infragistics.Win.Misc.UltraLabel();
            this.txtExportSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblExportSearch = new Infragistics.Win.Misc.UltraLabel();
            this.cmbExportGroup = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblExportGroup = new Infragistics.Win.Misc.UltraLabel();
            this.cmbExportBrand = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblExportBrand = new Infragistics.Win.Misc.UltraLabel();
            this.cmbExportCategory = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblExportCategory = new Infragistics.Win.Misc.UltraLabel();
            this.bgWorkerValidate = new System.ComponentModel.BackgroundWorker();
            this.bgWorkerImport = new System.ComponentModel.BackgroundWorker();
            this.bgWorkerExport = new System.ComponentModel.BackgroundWorker();
            this.bgWorkerExportPreview = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.tabControlMain)).BeginInit();
            this.tabControlMain.SuspendLayout();
            this.tabImport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerImport)).BeginInit();
            this.splitContainerImport.Panel1.SuspendLayout();
            this.splitContainerImport.Panel2.SuspendLayout();
            this.splitContainerImport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpMapping)).BeginInit();
            this.grpMapping.SuspendLayout();
            this.pnlMappingGrid.SuspendLayout();
            this.pnlAutoMapButton.ClientArea.SuspendLayout();
            this.pnlAutoMapButton.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpPreview)).BeginInit();
            this.grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridPreview)).BeginInit();
            this.pnlPreviewBanner.ClientArea.SuspendLayout();
            this.pnlPreviewBanner.SuspendLayout();
            this.pnlImportProgress.ClientArea.SuspendLayout();
            this.pnlImportProgress.SuspendLayout();
            this.pnlOptions.ClientArea.SuspendLayout();
            this.pnlOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkAutoGenerateBarcodes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAutoCreate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDuplicateBehavior)).BeginInit();
            this.pnlFileSelection.ClientArea.SuspendLayout();
            this.pnlFileSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtFilePath)).BeginInit();
            this.tabExport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpExportPreview)).BeginInit();
            this.grpExportPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridExportPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpFilters)).BeginInit();
            this.grpFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtExportSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbExportGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbExportBrand)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbExportCategory)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlMain
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(86)))));
            appearance1.ForeColor = System.Drawing.Color.White;
            this.tabControlMain.ActiveTabAppearance = appearance1;
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.tabControlMain.ClientAreaAppearance = appearance2;
            this.tabControlMain.Controls.Add(this.ultraTabSharedControlsPage1);
            this.tabControlMain.Controls.Add(this.tabImport);
            this.tabControlMain.Controls.Add(this.tabExport);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SharedControlsPage = this.ultraTabSharedControlsPage1;
            this.tabControlMain.Size = new System.Drawing.Size(950, 600);
            this.tabControlMain.Style = Infragistics.Win.UltraWinTabControl.UltraTabControlStyle.VisualStudio2005;
            this.tabControlMain.TabIndex = 0;
            this.tabControlMain.TabOrientation = Infragistics.Win.UltraWinTabs.TabOrientation.TopLeft;
            ultraTabImport.TabPage = this.tabImport;
            ultraTabImport.Text = "Import Products";
            ultraTabExport.TabPage = this.tabExport;
            ultraTabExport.Text = "Export Products";
            this.tabControlMain.Tabs.AddRange(new Infragistics.Win.UltraWinTabControl.UltraTab[] {
            ultraTabImport,
            ultraTabExport});
            this.tabControlMain.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraTabSharedControlsPage1
            // 
            this.ultraTabSharedControlsPage1.Location = new System.Drawing.Point(-10000, -10000);
            this.ultraTabSharedControlsPage1.Name = "ultraTabSharedControlsPage1";
            this.ultraTabSharedControlsPage1.Size = new System.Drawing.Size(948, 576);
            // 
            // tabImport
            // 
            this.tabImport.Controls.Add(this.splitContainerImport);
            this.tabImport.Controls.Add(this.pnlImportProgress);
            this.tabImport.Controls.Add(this.pnlOptions);
            this.tabImport.Controls.Add(this.pnlFileSelection);
            this.tabImport.Location = new System.Drawing.Point(1, 23);
            this.tabImport.Name = "tabImport";
            this.tabImport.Padding = new System.Windows.Forms.Padding(10);
            this.tabImport.Size = new System.Drawing.Size(948, 576);
            // 
            // splitContainerImport
            // 
            this.splitContainerImport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerImport.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerImport.Location = new System.Drawing.Point(10, 110);
            this.splitContainerImport.Name = "splitContainerImport";
            // 
            // splitContainerImport.Panel1
            // 
            this.splitContainerImport.Panel1.Controls.Add(this.grpMapping);
            this.splitContainerImport.Panel1.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            // 
            // splitContainerImport.Panel2
            // 
            this.splitContainerImport.Panel2.Controls.Add(this.grpPreview);
            this.splitContainerImport.Panel2.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.splitContainerImport.Size = new System.Drawing.Size(928, 406);
            this.splitContainerImport.SplitterDistance = 320;
            this.splitContainerImport.TabIndex = 2;
            // 
            // grpMapping
            // 
            appearance3.BackColor = System.Drawing.Color.White;
            appearance3.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(224)))), ((int)(((byte)(233)))));
            this.grpMapping.Appearance = appearance3;
            this.grpMapping.Controls.Add(this.pnlMappingGrid);
            this.grpMapping.Controls.Add(this.pnlAutoMapButton);
            this.grpMapping.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMapping.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            appearance6.FontData.BoldAsString = "True";
            appearance6.FontData.Name = "Segoe UI";
            appearance6.FontData.SizeInPoints = 9.5F;
            appearance6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(86)))));
            this.grpMapping.HeaderAppearance = appearance6;
            this.grpMapping.HeaderPosition = Infragistics.Win.Misc.GroupBoxHeaderPosition.TopInsideBorder;
            this.grpMapping.Location = new System.Drawing.Point(0, 0);
            this.grpMapping.Name = "grpMapping";
            this.grpMapping.Padding = new System.Windows.Forms.Padding(8);
            this.grpMapping.Size = new System.Drawing.Size(315, 406);
            this.grpMapping.TabIndex = 0;
            this.grpMapping.Text = "Column Mapping";
            this.grpMapping.ViewStyle = Infragistics.Win.Misc.GroupBoxViewStyle.Office2007;
            // 
            // pnlMappingGrid
            // 
            this.pnlMappingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMappingGrid.Location = new System.Drawing.Point(3, 21);
            this.pnlMappingGrid.Name = "pnlMappingGrid";
            this.pnlMappingGrid.Size = new System.Drawing.Size(309, 337);
            this.pnlMappingGrid.TabIndex = 0;
            // 
            // pnlAutoMapButton
            // 
            // 
            // pnlAutoMapButton.ClientArea
            // 
            this.pnlAutoMapButton.ClientArea.Controls.Add(this.btnAutoMap);
            this.pnlAutoMapButton.ClientArea.Controls.Add(this.btnPreview);
            this.pnlAutoMapButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlAutoMapButton.Location = new System.Drawing.Point(3, 358);
            this.pnlAutoMapButton.Name = "pnlAutoMapButton";
            this.pnlAutoMapButton.Size = new System.Drawing.Size(309, 45);
            this.pnlAutoMapButton.TabIndex = 1;
            // 
            // btnAutoMap
            // 
            appearance4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            appearance4.ForeColor = System.Drawing.Color.White;
            this.btnAutoMap.Appearance = appearance4;
            this.btnAutoMap.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnAutoMap.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAutoMap.Location = new System.Drawing.Point(3, 10);
            this.btnAutoMap.Name = "btnAutoMap";
            this.btnAutoMap.Size = new System.Drawing.Size(110, 30);
            this.btnAutoMap.TabIndex = 0;
            this.btnAutoMap.Text = "Auto Map";
            this.btnAutoMap.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnAutoMap.Click += new System.EventHandler(this.btnAutoMap_Click);
            // 
            // btnPreview
            // 
            appearance5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            appearance5.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            appearance5.ForeColor = System.Drawing.Color.White;
            appearance5.ForeColorDisabled = System.Drawing.Color.White;
            this.btnPreview.Appearance = appearance5;
            this.btnPreview.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnPreview.Enabled = false;
            this.btnPreview.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPreview.Location = new System.Drawing.Point(119, 10);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(177, 30);
            this.btnPreview.TabIndex = 1;
            this.btnPreview.Text = "Validate & Preview";
            this.btnPreview.UseAppStyling = false;
            this.btnPreview.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // grpPreview
            // 
            this.grpPreview.Appearance = appearance3;
            this.grpPreview.Controls.Add(this.ultraGridPreview);
            this.grpPreview.Controls.Add(this.pnlPreviewBanner);
            this.grpPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpPreview.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpPreview.HeaderAppearance = appearance6;
            this.grpPreview.HeaderPosition = Infragistics.Win.Misc.GroupBoxHeaderPosition.TopInsideBorder;
            this.grpPreview.Location = new System.Drawing.Point(5, 0);
            this.grpPreview.Name = "grpPreview";
            this.grpPreview.Padding = new System.Windows.Forms.Padding(8);
            this.grpPreview.Size = new System.Drawing.Size(599, 406);
            this.grpPreview.TabIndex = 0;
            this.grpPreview.Text = "Product Import Preview";
            this.grpPreview.ViewStyle = Infragistics.Win.Misc.GroupBoxViewStyle.Office2007;
            // 
            // ultraGridPreview
            // 
            this.ultraGridPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridPreview.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraGridPreview.Location = new System.Drawing.Point(3, 61);
            this.ultraGridPreview.Name = "ultraGridPreview";
            this.ultraGridPreview.Size = new System.Drawing.Size(593, 342);
            this.ultraGridPreview.TabIndex = 1;
            this.ultraGridPreview.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.ultraGridPreview.InitializeLayout += new Infragistics.Win.UltraWinGrid.InitializeLayoutEventHandler(this.ultraGridPreview_InitializeLayout);
            this.ultraGridPreview.InitializeRow += new Infragistics.Win.UltraWinGrid.InitializeRowEventHandler(this.ultraGridPreview_InitializeRow);
            // 
            // pnlPreviewBanner
            // 
            appearance7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.pnlPreviewBanner.Appearance = appearance7;
            // 
            // pnlPreviewBanner.ClientArea
            // 
            this.pnlPreviewBanner.ClientArea.Controls.Add(this.lblStats);
            this.pnlPreviewBanner.ClientArea.Controls.Add(this.btnDownloadErrorLog);
            this.pnlPreviewBanner.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPreviewBanner.Location = new System.Drawing.Point(3, 21);
            this.pnlPreviewBanner.Name = "pnlPreviewBanner";
            this.pnlPreviewBanner.Size = new System.Drawing.Size(593, 40);
            this.pnlPreviewBanner.TabIndex = 0;
            // 
            // lblStats
            // 
            this.lblStats.AutoSize = true;
            this.lblStats.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStats.Location = new System.Drawing.Point(10, 12);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(263, 18);
            this.lblStats.TabIndex = 0;
            this.lblStats.Text = "No file loaded. Please select a CSV file to begin.";
            // 
            // btnDownloadErrorLog
            // 
            this.btnDownloadErrorLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            appearance8.ForeColor = System.Drawing.Color.White;
            this.btnDownloadErrorLog.Appearance = appearance8;
            this.btnDownloadErrorLog.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnDownloadErrorLog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDownloadErrorLog.Location = new System.Drawing.Point(450, 4);
            this.btnDownloadErrorLog.Name = "btnDownloadErrorLog";
            this.btnDownloadErrorLog.Size = new System.Drawing.Size(138, 30);
            this.btnDownloadErrorLog.TabIndex = 1;
            this.btnDownloadErrorLog.Text = "Download Failure Log";
            this.btnDownloadErrorLog.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnDownloadErrorLog.Visible = false;
            this.btnDownloadErrorLog.Click += new System.EventHandler(this.btnDownloadErrorLog_Click);
            // 
            // pnlImportProgress
            // 
            // 
            // pnlImportProgress.ClientArea
            // 
            this.pnlImportProgress.ClientArea.Controls.Add(this.progressBarImport);
            this.pnlImportProgress.ClientArea.Controls.Add(this.lblProgress);
            this.pnlImportProgress.ClientArea.Controls.Add(this.btnImport);
            this.pnlImportProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlImportProgress.Location = new System.Drawing.Point(10, 516);
            this.pnlImportProgress.Name = "pnlImportProgress";
            this.pnlImportProgress.Size = new System.Drawing.Size(928, 50);
            this.pnlImportProgress.TabIndex = 3;
            // 
            // progressBarImport
            // 
            this.progressBarImport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarImport.Location = new System.Drawing.Point(3, 17);
            this.progressBarImport.Name = "progressBarImport";
            this.progressBarImport.Size = new System.Drawing.Size(606, 20);
            this.progressBarImport.TabIndex = 0;
            // 
            // lblProgress
            // 
            this.lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgress.AutoSize = true;
            this.lblProgress.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProgress.Location = new System.Drawing.Point(615, 20);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(38, 18);
            this.lblProgress.TabIndex = 1;
            this.lblProgress.Text = "Ready";
            // 
            // btnImport
            // 
            this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(150)))), ((int)(((byte)(105)))));
            appearance9.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(150)))), ((int)(((byte)(105)))));
            appearance9.ForeColor = System.Drawing.Color.White;
            appearance9.ForeColorDisabled = System.Drawing.Color.White;
            this.btnImport.Appearance = appearance9;
            this.btnImport.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnImport.Enabled = false;
            this.btnImport.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnImport.Location = new System.Drawing.Point(798, 10);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(127, 30);
            this.btnImport.TabIndex = 2;
            this.btnImport.Text = "Start Import";
            this.btnImport.UseAppStyling = false;
            this.btnImport.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // pnlOptions
            // 
            appearance10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.pnlOptions.Appearance = appearance10;
            // 
            // pnlOptions.ClientArea
            // 
            this.pnlOptions.ClientArea.Controls.Add(this.lblBackupWarning);
            this.pnlOptions.ClientArea.Controls.Add(this.chkAutoGenerateBarcodes);
            this.pnlOptions.ClientArea.Controls.Add(this.chkAutoCreate);
            this.pnlOptions.ClientArea.Controls.Add(this.cmbDuplicateBehavior);
            this.pnlOptions.ClientArea.Controls.Add(this.lblDuplicateBehavior);
            this.pnlOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOptions.Location = new System.Drawing.Point(10, 60);
            this.pnlOptions.Name = "pnlOptions";
            this.pnlOptions.Size = new System.Drawing.Size(928, 50);
            this.pnlOptions.TabIndex = 1;
            // 
            // lblBackupWarning
            // 
            this.lblBackupWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance11.ForeColor = System.Drawing.Color.Crimson;
            this.lblBackupWarning.Appearance = appearance11;
            this.lblBackupWarning.AutoSize = true;
            this.lblBackupWarning.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBackupWarning.Location = new System.Drawing.Point(593, 18);
            this.lblBackupWarning.Name = "lblBackupWarning";
            this.lblBackupWarning.Size = new System.Drawing.Size(328, 17);
            this.lblBackupWarning.TabIndex = 4;
            this.lblBackupWarning.Text = "* Warning: Back up your database before running bulk imports.";
            // 
            // chkAutoGenerateBarcodes
            // 
            this.chkAutoGenerateBarcodes.Checked = true;
            this.chkAutoGenerateBarcodes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoGenerateBarcodes.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkAutoGenerateBarcodes.Location = new System.Drawing.Point(438, 14);
            this.chkAutoGenerateBarcodes.Name = "chkAutoGenerateBarcodes";
            this.chkAutoGenerateBarcodes.Size = new System.Drawing.Size(160, 22);
            this.chkAutoGenerateBarcodes.TabIndex = 3;
            this.chkAutoGenerateBarcodes.Text = "Auto-Generate Barcodes";
            // 
            // chkAutoCreate
            // 
            this.chkAutoCreate.Checked = true;
            this.chkAutoCreate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoCreate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkAutoCreate.Location = new System.Drawing.Point(267, 14);
            this.chkAutoCreate.Name = "chkAutoCreate";
            this.chkAutoCreate.Size = new System.Drawing.Size(168, 22);
            this.chkAutoCreate.TabIndex = 2;
            this.chkAutoCreate.Text = "Auto-Create Master Data";
            // 
            // cmbDuplicateBehavior
            // 
            this.cmbDuplicateBehavior.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbDuplicateBehavior.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbDuplicateBehavior.Location = new System.Drawing.Point(125, 14);
            this.cmbDuplicateBehavior.Name = "cmbDuplicateBehavior";
            this.cmbDuplicateBehavior.Size = new System.Drawing.Size(121, 25);
            this.cmbDuplicateBehavior.TabIndex = 1;
            // 
            // lblDuplicateBehavior
            // 
            this.lblDuplicateBehavior.AutoSize = true;
            this.lblDuplicateBehavior.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDuplicateBehavior.Location = new System.Drawing.Point(10, 18);
            this.lblDuplicateBehavior.Name = "lblDuplicateBehavior";
            this.lblDuplicateBehavior.Size = new System.Drawing.Size(112, 18);
            this.lblDuplicateBehavior.TabIndex = 0;
            this.lblDuplicateBehavior.Text = "Duplicate Barcodes:";
            // 
            // pnlFileSelection
            // 
            // 
            // pnlFileSelection.ClientArea
            // 
            this.pnlFileSelection.ClientArea.Controls.Add(this.btnDownloadTemplate);
            this.pnlFileSelection.ClientArea.Controls.Add(this.btnLoad);
            this.pnlFileSelection.ClientArea.Controls.Add(this.btnBrowse);
            this.pnlFileSelection.ClientArea.Controls.Add(this.txtFilePath);
            this.pnlFileSelection.ClientArea.Controls.Add(this.lblFilePath);
            this.pnlFileSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFileSelection.Location = new System.Drawing.Point(10, 10);
            this.pnlFileSelection.Name = "pnlFileSelection";
            this.pnlFileSelection.Size = new System.Drawing.Size(928, 50);
            this.pnlFileSelection.TabIndex = 0;
            // 
            // btnDownloadTemplate
            // 
            this.btnDownloadTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            appearance12.ForeColor = System.Drawing.Color.White;
            this.btnDownloadTemplate.Appearance = appearance12;
            this.btnDownloadTemplate.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnDownloadTemplate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDownloadTemplate.Location = new System.Drawing.Point(798, 10);
            this.btnDownloadTemplate.Name = "btnDownloadTemplate";
            this.btnDownloadTemplate.Size = new System.Drawing.Size(127, 30);
            this.btnDownloadTemplate.TabIndex = 4;
            this.btnDownloadTemplate.Text = "Download Template";
            this.btnDownloadTemplate.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnDownloadTemplate.Click += new System.EventHandler(this.btnDownloadTemplate_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance13.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            appearance13.ForeColor = System.Drawing.Color.White;
            this.btnLoad.Appearance = appearance13;
            this.btnLoad.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnLoad.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLoad.Location = new System.Drawing.Point(701, 10);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(91, 30);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "Load File";
            this.btnLoad.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance14.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            appearance14.ForeColor = System.Drawing.Color.White;
            this.btnBrowse.Appearance = appearance14;
            this.btnBrowse.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnBrowse.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBrowse.Location = new System.Drawing.Point(615, 10);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(80, 30);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtFilePath
            // 
            this.txtFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilePath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFilePath.Location = new System.Drawing.Point(125, 14);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.ReadOnly = true;
            this.txtFilePath.Size = new System.Drawing.Size(484, 25);
            this.txtFilePath.TabIndex = 1;
            // 
            // lblFilePath
            // 
            appearance15.FontData.BoldAsString = "True";
            this.lblFilePath.Appearance = appearance15;
            this.lblFilePath.AutoSize = true;
            this.lblFilePath.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFilePath.Location = new System.Drawing.Point(10, 16);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(101, 19);
            this.lblFilePath.TabIndex = 0;
            this.lblFilePath.Text = "Select Data File:";
            // 
            // tabExport
            // 
            this.tabExport.Controls.Add(this.grpExportPreview);
            this.tabExport.Controls.Add(this.grpFilters);
            this.tabExport.Location = new System.Drawing.Point(-10000, -10000);
            this.tabExport.Name = "tabExport";
            this.tabExport.Padding = new System.Windows.Forms.Padding(10);
            this.tabExport.Size = new System.Drawing.Size(948, 576);
            // 
            // grpExportPreview
            // 
            this.grpExportPreview.Appearance = appearance3;
            this.grpExportPreview.Controls.Add(this.ultraGridExportPreview);
            this.grpExportPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpExportPreview.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpExportPreview.HeaderAppearance = appearance6;
            this.grpExportPreview.HeaderPosition = Infragistics.Win.Misc.GroupBoxHeaderPosition.TopInsideBorder;
            this.grpExportPreview.Location = new System.Drawing.Point(10, 220);
            this.grpExportPreview.Name = "grpExportPreview";
            this.grpExportPreview.Padding = new System.Windows.Forms.Padding(8);
            this.grpExportPreview.Size = new System.Drawing.Size(922, 340);
            this.grpExportPreview.TabIndex = 2;
            this.grpExportPreview.Text = "Export Products Preview";
            this.grpExportPreview.ViewStyle = Infragistics.Win.Misc.GroupBoxViewStyle.Office2007;
            // 
            // ultraGridExportPreview
            // 
            this.ultraGridExportPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridExportPreview.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraGridExportPreview.Location = new System.Drawing.Point(3, 21);
            this.ultraGridExportPreview.Name = "ultraGridExportPreview";
            this.ultraGridExportPreview.Size = new System.Drawing.Size(916, 316);
            this.ultraGridExportPreview.TabIndex = 0;
            this.ultraGridExportPreview.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.ultraGridExportPreview.InitializeLayout += new Infragistics.Win.UltraWinGrid.InitializeLayoutEventHandler(this.ultraGridExportPreview_InitializeLayout);
            // 
            // grpFilters
            // 
            this.grpFilters.Appearance = appearance3;
            this.grpFilters.Controls.Add(this.btnExport);
            this.grpFilters.Controls.Add(this.btnLoadPreview);
            this.grpFilters.Controls.Add(this.progressBarExport);
            this.grpFilters.Controls.Add(this.lblProgressExport);
            this.grpFilters.Controls.Add(this.txtExportSearch);
            this.grpFilters.Controls.Add(this.lblExportSearch);
            this.grpFilters.Controls.Add(this.cmbExportGroup);
            this.grpFilters.Controls.Add(this.lblExportGroup);
            this.grpFilters.Controls.Add(this.cmbExportBrand);
            this.grpFilters.Controls.Add(this.lblExportBrand);
            this.grpFilters.Controls.Add(this.cmbExportCategory);
            this.grpFilters.Controls.Add(this.lblExportCategory);
            this.grpFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpFilters.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpFilters.HeaderAppearance = appearance6;
            this.grpFilters.HeaderPosition = Infragistics.Win.Misc.GroupBoxHeaderPosition.TopInsideBorder;
            this.grpFilters.Location = new System.Drawing.Point(10, 10);
            this.grpFilters.Name = "grpFilters";
            this.grpFilters.Size = new System.Drawing.Size(922, 210);
            this.grpFilters.TabIndex = 0;
            this.grpFilters.Text = "Export Filters";
            this.grpFilters.ViewStyle = Infragistics.Win.Misc.GroupBoxViewStyle.Office2007;
            // 
            // btnExport
            // 
            appearance16.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(150)))), ((int)(((byte)(105)))));
            appearance16.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(243)))), ((int)(((byte)(208)))));
            appearance16.ForeColor = System.Drawing.Color.White;
            appearance16.ForeColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnExport.Appearance = appearance16;
            this.btnExport.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnExport.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExport.Location = new System.Drawing.Point(450, 90);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(140, 35);
            this.btnExport.TabIndex = 9;
            this.btnExport.Text = "Export Products";
            this.btnExport.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnLoadPreview
            // 
            appearance17.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            appearance17.ForeColor = System.Drawing.Color.White;
            this.btnLoadPreview.Appearance = appearance17;
            this.btnLoadPreview.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnLoadPreview.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLoadPreview.Location = new System.Drawing.Point(450, 40);
            this.btnLoadPreview.Name = "btnLoadPreview";
            this.btnLoadPreview.Size = new System.Drawing.Size(140, 35);
            this.btnLoadPreview.TabIndex = 8;
            this.btnLoadPreview.Text = "Load Preview";
            this.btnLoadPreview.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnLoadPreview.Click += new System.EventHandler(this.btnLoadPreview_Click);
            // 
            // progressBarExport
            // 
            this.progressBarExport.Location = new System.Drawing.Point(450, 140);
            this.progressBarExport.Name = "progressBarExport";
            this.progressBarExport.Size = new System.Drawing.Size(350, 20);
            this.progressBarExport.TabIndex = 10;
            // 
            // lblProgressExport
            // 
            this.lblProgressExport.AutoSize = true;
            this.lblProgressExport.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProgressExport.Location = new System.Drawing.Point(450, 165);
            this.lblProgressExport.Name = "lblProgressExport";
            this.lblProgressExport.Size = new System.Drawing.Size(38, 18);
            this.lblProgressExport.TabIndex = 11;
            this.lblProgressExport.Text = "Ready";
            // 
            // txtExportSearch
            // 
            this.txtExportSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtExportSearch.Location = new System.Drawing.Point(115, 160);
            this.txtExportSearch.Name = "txtExportSearch";
            this.txtExportSearch.Size = new System.Drawing.Size(250, 25);
            this.txtExportSearch.TabIndex = 7;
            // 
            // lblExportSearch
            // 
            this.lblExportSearch.AutoSize = true;
            this.lblExportSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExportSearch.Location = new System.Drawing.Point(20, 163);
            this.lblExportSearch.Name = "lblExportSearch";
            this.lblExportSearch.Size = new System.Drawing.Size(99, 18);
            this.lblExportSearch.TabIndex = 6;
            this.lblExportSearch.Text = "Search Name/BC:";
            // 
            // cmbExportGroup
            // 
            this.cmbExportGroup.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbExportGroup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbExportGroup.Location = new System.Drawing.Point(115, 120);
            this.cmbExportGroup.Name = "cmbExportGroup";
            this.cmbExportGroup.Size = new System.Drawing.Size(250, 25);
            this.cmbExportGroup.TabIndex = 5;
            // 
            // lblExportGroup
            // 
            this.lblExportGroup.AutoSize = true;
            this.lblExportGroup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExportGroup.Location = new System.Drawing.Point(20, 123);
            this.lblExportGroup.Name = "lblExportGroup";
            this.lblExportGroup.Size = new System.Drawing.Size(41, 18);
            this.lblExportGroup.TabIndex = 4;
            this.lblExportGroup.Text = "Group:";
            // 
            // cmbExportBrand
            // 
            this.cmbExportBrand.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbExportBrand.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbExportBrand.Location = new System.Drawing.Point(115, 80);
            this.cmbExportBrand.Name = "cmbExportBrand";
            this.cmbExportBrand.Size = new System.Drawing.Size(250, 25);
            this.cmbExportBrand.TabIndex = 3;
            // 
            // lblExportBrand
            // 
            this.lblExportBrand.AutoSize = true;
            this.lblExportBrand.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExportBrand.Location = new System.Drawing.Point(20, 83);
            this.lblExportBrand.Name = "lblExportBrand";
            this.lblExportBrand.Size = new System.Drawing.Size(39, 18);
            this.lblExportBrand.TabIndex = 2;
            this.lblExportBrand.Text = "Brand:";
            // 
            // cmbExportCategory
            // 
            this.cmbExportCategory.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbExportCategory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbExportCategory.Location = new System.Drawing.Point(115, 40);
            this.cmbExportCategory.Name = "cmbExportCategory";
            this.cmbExportCategory.Size = new System.Drawing.Size(250, 25);
            this.cmbExportCategory.TabIndex = 1;
            // 
            // lblExportCategory
            // 
            this.lblExportCategory.AutoSize = true;
            this.lblExportCategory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExportCategory.Location = new System.Drawing.Point(20, 43);
            this.lblExportCategory.Name = "lblExportCategory";
            this.lblExportCategory.Size = new System.Drawing.Size(57, 18);
            this.lblExportCategory.TabIndex = 0;
            this.lblExportCategory.Text = "Category:";
            // 
            // bgWorkerValidate
            // 
            this.bgWorkerValidate.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorkerValidate_DoWork);
            this.bgWorkerValidate.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorkerValidate_RunWorkerCompleted);
            // 
            // bgWorkerImport
            // 
            this.bgWorkerImport.WorkerReportsProgress = true;
            this.bgWorkerImport.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorkerImport_DoWork);
            this.bgWorkerImport.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorkerImport_ProgressChanged);
            this.bgWorkerImport.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorkerImport_RunWorkerCompleted);
            // 
            // bgWorkerExport
            // 
            this.bgWorkerExport.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorkerExport_DoWork);
            this.bgWorkerExport.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorkerExport_RunWorkerCompleted);
            // 
            // bgWorkerExportPreview
            // 
            this.bgWorkerExportPreview.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorkerExportPreview_DoWork);
            this.bgWorkerExportPreview.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorkerExportPreview_RunWorkerCompleted);
            // 
            // FrmExcelImport
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.ClientSize = new System.Drawing.Size(950, 600);
            this.Controls.Add(this.tabControlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmExcelImport";
            this.Text = "Bulk Product Import / Export";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmExcelImport_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tabControlMain)).EndInit();
            this.tabControlMain.ResumeLayout(false);
            this.tabImport.ResumeLayout(false);
            this.splitContainerImport.Panel1.ResumeLayout(false);
            this.splitContainerImport.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerImport)).EndInit();
            this.splitContainerImport.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grpMapping)).EndInit();
            this.grpMapping.ResumeLayout(false);
            this.pnlMappingGrid.ResumeLayout(false);
            this.pnlAutoMapButton.ClientArea.ResumeLayout(false);
            this.pnlAutoMapButton.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grpPreview)).EndInit();
            this.grpPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridPreview)).EndInit();
            this.pnlPreviewBanner.ClientArea.ResumeLayout(false);
            this.pnlPreviewBanner.ClientArea.PerformLayout();
            this.pnlPreviewBanner.ResumeLayout(false);
            this.pnlImportProgress.ClientArea.ResumeLayout(false);
            this.pnlImportProgress.ClientArea.PerformLayout();
            this.pnlImportProgress.ResumeLayout(false);
            this.pnlOptions.ClientArea.ResumeLayout(false);
            this.pnlOptions.ClientArea.PerformLayout();
            this.pnlOptions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chkAutoGenerateBarcodes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAutoCreate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDuplicateBehavior)).EndInit();
            this.pnlFileSelection.ClientArea.ResumeLayout(false);
            this.pnlFileSelection.ClientArea.PerformLayout();
            this.pnlFileSelection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtFilePath)).EndInit();
            this.tabExport.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grpExportPreview)).EndInit();
            this.grpExportPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridExportPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpFilters)).EndInit();
            this.grpFilters.ResumeLayout(false);
            this.grpFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtExportSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbExportGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbExportBrand)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbExportCategory)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.UltraWinTabControl.UltraTabControl tabControlMain;
        private Infragistics.Win.UltraWinTabControl.UltraTabPageControl tabImport;
        private Infragistics.Win.UltraWinTabControl.UltraTabPageControl tabExport;
        private Infragistics.Win.UltraWinTabControl.UltraTabSharedControlsPage ultraTabSharedControlsPage1;
        private Infragistics.Win.Misc.UltraPanel pnlFileSelection;
        private Infragistics.Win.Misc.UltraLabel lblFilePath;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtFilePath;
        private Infragistics.Win.Misc.UltraButton btnBrowse;
        private Infragistics.Win.Misc.UltraButton btnLoad;
        private Infragistics.Win.Misc.UltraPanel pnlOptions;
        private Infragistics.Win.Misc.UltraLabel lblDuplicateBehavior;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbDuplicateBehavior;
        private Infragistics.Win.UltraWinEditors.UltraCheckEditor chkAutoCreate;
        private Infragistics.Win.UltraWinEditors.UltraCheckEditor chkAutoGenerateBarcodes;
        private Infragistics.Win.Misc.UltraLabel lblBackupWarning;
        private System.Windows.Forms.SplitContainer splitContainerImport;
        private Infragistics.Win.Misc.UltraGroupBox grpMapping;
        private Infragistics.Win.Misc.UltraPanel pnlMappingGrid;
        private Infragistics.Win.Misc.UltraPanel pnlAutoMapButton;
        private Infragistics.Win.Misc.UltraButton btnAutoMap;
        private Infragistics.Win.Misc.UltraButton btnPreview;
        private Infragistics.Win.Misc.UltraGroupBox grpPreview;
        private Infragistics.Win.Misc.UltraPanel pnlPreviewBanner;
        private Infragistics.Win.Misc.UltraLabel lblStats;
        private Infragistics.Win.Misc.UltraButton btnDownloadErrorLog;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridPreview;
        private Infragistics.Win.Misc.UltraPanel pnlImportProgress;
        private System.Windows.Forms.ProgressBar progressBarImport;
        private Infragistics.Win.Misc.UltraLabel lblProgress;
        private Infragistics.Win.Misc.UltraButton btnImport;
        private Infragistics.Win.Misc.UltraGroupBox grpFilters;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbExportGroup;
        private Infragistics.Win.Misc.UltraLabel lblExportGroup;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbExportBrand;
        private Infragistics.Win.Misc.UltraLabel lblExportBrand;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbExportCategory;
        private Infragistics.Win.Misc.UltraLabel lblExportCategory;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private System.Windows.Forms.ProgressBar progressBarExport;
        private Infragistics.Win.Misc.UltraLabel lblProgressExport;
        private Infragistics.Win.Misc.UltraButton btnDownloadTemplate;
        private System.ComponentModel.BackgroundWorker bgWorkerValidate;
        private System.ComponentModel.BackgroundWorker bgWorkerImport;
        private System.ComponentModel.BackgroundWorker bgWorkerExport;
        private Infragistics.Win.Misc.UltraLabel lblExportSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtExportSearch;
        private Infragistics.Win.Misc.UltraButton btnLoadPreview;
        private Infragistics.Win.Misc.UltraGroupBox grpExportPreview;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridExportPreview;
        private System.ComponentModel.BackgroundWorker bgWorkerExportPreview;
    }
}
