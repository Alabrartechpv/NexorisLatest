namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmTrialBalance
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
            Infragistics.Win.Appearance appearanceGrid = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceHeader = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceSelected = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGridFooter = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceDiffBadge = new Infragistics.Win.Appearance();

            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblSearchStatus = new Infragistics.Win.Misc.UltraLabel();
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
            this.ultraGridTrialBalance = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.lblOpeningSummary = new Infragistics.Win.Misc.UltraLabel();
            this.lblTransactionSummary = new Infragistics.Win.Misc.UltraLabel();
            this.lblClosingSummary = new Infragistics.Win.Misc.UltraLabel();
            this.lblDifferenceBadge = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPresetDates)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).BeginInit();

            this.ultraPanelAction.ClientArea.SuspendLayout();
            this.ultraPanelAction.SuspendLayout();

            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTrialBalance)).BeginInit();
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
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearchStatus);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboPresetDates);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraDateTimeFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraDateTimeTo);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1200, 78);
            this.ultraPanelControls.TabIndex = 0;

            // 
            // lblSearch
            // 
            this.lblSearch.Location = new System.Drawing.Point(20, 14);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(55, 20);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search";

            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(80, 11);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.NullText = "Search Ledger or Account Group...";
            this.txtSearch.Size = new System.Drawing.Size(280, 24);
            this.txtSearch.TabIndex = 1;

            // 
            // lblSearchStatus
            // 
            this.lblSearchStatus.Location = new System.Drawing.Point(375, 14);
            this.lblSearchStatus.Name = "lblSearchStatus";
            this.lblSearchStatus.Size = new System.Drawing.Size(200, 20);
            this.lblSearchStatus.TabIndex = 2;
            this.lblSearchStatus.Text = "";

            // 
            // lblPreset
            // 
            this.lblPreset.Location = new System.Drawing.Point(20, 46);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(55, 20);
            this.lblPreset.TabIndex = 3;
            this.lblPreset.Text = "Period";

            // 
            // ultraComboPresetDates
            // 
            this.ultraComboPresetDates.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboPresetDates.Location = new System.Drawing.Point(80, 43);
            this.ultraComboPresetDates.Name = "ultraComboPresetDates";
            this.ultraComboPresetDates.Size = new System.Drawing.Size(160, 24);
            this.ultraComboPresetDates.TabIndex = 4;
            this.ultraComboPresetDates.ValueChanged += new System.EventHandler(this.UltraComboPresetDates_ValueChanged);

            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(255, 46);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(40, 20);
            this.lblFromDate.TabIndex = 5;
            this.lblFromDate.Text = "From";

            // 
            // ultraDateTimeFrom
            // 
            this.ultraDateTimeFrom.FormatString = "dd-MM-yyyy";
            this.ultraDateTimeFrom.Location = new System.Drawing.Point(300, 43);
            this.ultraDateTimeFrom.Name = "ultraDateTimeFrom";
            this.ultraDateTimeFrom.Size = new System.Drawing.Size(120, 24);
            this.ultraDateTimeFrom.TabIndex = 6;

            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(435, 46);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(25, 20);
            this.lblToDate.TabIndex = 7;
            this.lblToDate.Text = "To";

            // 
            // ultraDateTimeTo
            // 
            this.ultraDateTimeTo.FormatString = "dd-MM-yyyy";
            this.ultraDateTimeTo.Location = new System.Drawing.Point(465, 43);
            this.ultraDateTimeTo.Name = "ultraDateTimeTo";
            this.ultraDateTimeTo.Size = new System.Drawing.Size(120, 24);
            this.ultraDateTimeTo.TabIndex = 8;

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
            this.ultraPanelAction.Location = new System.Drawing.Point(0, 78);
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
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraGridTrialBalance);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 123);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1200, 577);
            this.ultraPanelMaster.TabIndex = 2;

            // 
            // ultraGridTrialBalance
            // 
            appearanceGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ultraGridTrialBalance.DisplayLayout.Appearance = appearanceGrid;
            this.ultraGridTrialBalance.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            appearanceHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceHeader.ForeColor = System.Drawing.Color.White;
            this.ultraGridTrialBalance.DisplayLayout.Override.HeaderAppearance = appearanceHeader;
            appearanceSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(126)))), ((int)(((byte)(245)))));
            appearanceSelected.ForeColor = System.Drawing.Color.White;
            this.ultraGridTrialBalance.DisplayLayout.Override.SelectedRowAppearance = appearanceSelected;
            this.ultraGridTrialBalance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridTrialBalance.Location = new System.Drawing.Point(0, 0);
            this.ultraGridTrialBalance.Name = "ultraGridTrialBalance";
            this.ultraGridTrialBalance.Size = new System.Drawing.Size(1200, 543);
            this.ultraGridTrialBalance.TabIndex = 0;
            this.ultraGridTrialBalance.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

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
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblOpeningSummary);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblTransactionSummary);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblClosingSummary);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblDifferenceBadge);
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 543);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1200, 34);
            this.ultraPanelGridFooter.TabIndex = 1;

            // 
            // lblOpeningSummary
            // 
            this.lblOpeningSummary.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblOpeningSummary.Location = new System.Drawing.Point(12, 6);
            this.lblOpeningSummary.Name = "lblOpeningSummary";
            this.lblOpeningSummary.Size = new System.Drawing.Size(265, 22);
            this.lblOpeningSummary.TabIndex = 0;
            this.lblOpeningSummary.Text = "Opening: Dr ₹ 0.00 | Cr ₹ 0.00";

            // 
            // lblTransactionSummary
            // 
            this.lblTransactionSummary.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTransactionSummary.Location = new System.Drawing.Point(285, 6);
            this.lblTransactionSummary.Name = "lblTransactionSummary";
            this.lblTransactionSummary.Size = new System.Drawing.Size(275, 22);
            this.lblTransactionSummary.TabIndex = 1;
            this.lblTransactionSummary.Text = "Period: Dr ₹ 0.00 | Cr ₹ 0.00";

            // 
            // lblClosingSummary
            // 
            this.lblClosingSummary.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblClosingSummary.Location = new System.Drawing.Point(568, 6);
            this.lblClosingSummary.Name = "lblClosingSummary";
            this.lblClosingSummary.Size = new System.Drawing.Size(275, 22);
            this.lblClosingSummary.TabIndex = 2;
            this.lblClosingSummary.Text = "Closing: Dr ₹ 0.00 | Cr ₹ 0.00";

            // 
            // lblDifferenceBadge
            // 
            this.lblDifferenceBadge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearanceDiffBadge.TextHAlign = Infragistics.Win.HAlign.Center;
            appearanceDiffBadge.TextVAlign = Infragistics.Win.VAlign.Middle;
            this.lblDifferenceBadge.Appearance = appearanceDiffBadge;
            this.lblDifferenceBadge.BorderStyleInner = Infragistics.Win.UIElementBorderStyle.Solid;
            this.lblDifferenceBadge.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDifferenceBadge.Location = new System.Drawing.Point(920, 4);
            this.lblDifferenceBadge.Name = "lblDifferenceBadge";
            this.lblDifferenceBadge.Size = new System.Drawing.Size(270, 26);
            this.lblDifferenceBadge.TabIndex = 3;
            this.lblDifferenceBadge.Text = "BALANCED: ₹ 0.00";

            // 
            // FrmTrialBalance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelAction);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmTrialBalance";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Trial Balance";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPresetDates)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).EndInit();

            this.ultraPanelAction.ClientArea.ResumeLayout(false);
            this.ultraPanelAction.ResumeLayout(false);

            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTrialBalance)).EndInit();

            this.ultraPanelGridFooter.ClientArea.ResumeLayout(false);
            this.ultraPanelGridFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblSearchStatus;
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
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridTrialBalance;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
        private Infragistics.Win.Misc.UltraLabel lblOpeningSummary;
        private Infragistics.Win.Misc.UltraLabel lblTransactionSummary;
        private Infragistics.Win.Misc.UltraLabel lblClosingSummary;
        private Infragistics.Win.Misc.UltraLabel lblDifferenceBadge;
    }
}
