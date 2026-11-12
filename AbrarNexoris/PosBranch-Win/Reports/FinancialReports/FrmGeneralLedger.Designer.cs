namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmGeneralLedger
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
            Infragistics.Win.Appearance appearanceHeader = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceAction = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceMaster = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceFooter = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGrid = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceClosingBadge = new Infragistics.Win.Appearance();

            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblGroupFilter = new Infragistics.Win.Misc.UltraLabel();
            this.cmbGroupFilter = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblRowCount = new Infragistics.Win.Misc.UltraLabel();
            this.lblLedger = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboLedger = new Infragistics.Win.UltraWinGrid.UltraCombo();
            this.lblPreset = new Infragistics.Win.Misc.UltraLabel();
            this.cmbDateQuickSelect = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFromDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtToDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();

            this.ultraPanelAction = new Infragistics.Win.Misc.UltraPanel();
            this.btnGenerate = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnExportCsv = new Infragistics.Win.Misc.UltraButton();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnToggleSelection = new Infragistics.Win.Misc.UltraButton();

            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridTransactions = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.lblOpeningSummary = new Infragistics.Win.Misc.UltraLabel();
            this.lblDebitSummary = new Infragistics.Win.Misc.UltraLabel();
            this.lblCreditSummary = new Infragistics.Win.Misc.UltraLabel();
            this.lblClosingBadge = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbGroupFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboLedger)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDateQuickSelect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).BeginInit();

            this.ultraPanelAction.ClientArea.SuspendLayout();
            this.ultraPanelAction.SuspendLayout();

            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTransactions)).BeginInit();
            this.ultraPanelGridFooter.ClientArea.SuspendLayout();
            this.ultraPanelGridFooter.SuspendLayout();

            this.SuspendLayout();

            // =========================================================================
            // ultraPanelControls (Top Filter Panel, Height: 78)
            // =========================================================================
            this.ultraPanelControls.Appearance = appearanceHeader;
            this.ultraPanelControls.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblGroupFilter);
            this.ultraPanelControls.ClientArea.Controls.Add(this.cmbGroupFilter);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblRowCount);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblLedger);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboLedger);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.cmbDateQuickSelect);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtToDate);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1280, 78);
            this.ultraPanelControls.TabIndex = 0;

            // Row 1: Search, Account Group Filter & Row Count
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSearch.Location = new System.Drawing.Point(12, 12);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(55, 23);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search";

            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.Location = new System.Drawing.Point(72, 9);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.NullText = "Search voucher, contra account, narration, user...";
            this.txtSearch.Size = new System.Drawing.Size(300, 24);
            this.txtSearch.TabIndex = 1;

            this.lblGroupFilter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblGroupFilter.Location = new System.Drawing.Point(385, 12);
            this.lblGroupFilter.Name = "lblGroupFilter";
            this.lblGroupFilter.Size = new System.Drawing.Size(45, 23);
            this.lblGroupFilter.TabIndex = 2;
            this.lblGroupFilter.Text = "Group";

            this.cmbGroupFilter.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbGroupFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbGroupFilter.Location = new System.Drawing.Point(435, 9);
            this.cmbGroupFilter.Name = "cmbGroupFilter";
            this.cmbGroupFilter.Size = new System.Drawing.Size(210, 24);
            this.cmbGroupFilter.TabIndex = 3;

            this.lblRowCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.lblRowCount.Location = new System.Drawing.Point(660, 12);
            this.lblRowCount.Name = "lblRowCount";
            this.lblRowCount.Size = new System.Drawing.Size(420, 20);
            this.lblRowCount.TabIndex = 4;
            this.lblRowCount.Text = "Total: 0 rows";

            // Row 2: Ledger & Period & Dates
            this.lblLedger.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblLedger.Location = new System.Drawing.Point(12, 44);
            this.lblLedger.Name = "lblLedger";
            this.lblLedger.Size = new System.Drawing.Size(55, 23);
            this.lblLedger.TabIndex = 5;
            this.lblLedger.Text = "Ledger";

            this.ultraComboLedger.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ultraComboLedger.Location = new System.Drawing.Point(72, 42);
            this.ultraComboLedger.Name = "ultraComboLedger";
            this.ultraComboLedger.Size = new System.Drawing.Size(300, 24);
            this.ultraComboLedger.TabIndex = 6;

            this.lblPreset.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPreset.Location = new System.Drawing.Point(385, 44);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(45, 23);
            this.lblPreset.TabIndex = 7;
            this.lblPreset.Text = "Period";

            this.cmbDateQuickSelect.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbDateQuickSelect.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbDateQuickSelect.Location = new System.Drawing.Point(435, 42);
            this.cmbDateQuickSelect.Name = "cmbDateQuickSelect";
            this.cmbDateQuickSelect.Size = new System.Drawing.Size(150, 24);
            this.cmbDateQuickSelect.TabIndex = 8;

            this.lblFromDate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFromDate.Location = new System.Drawing.Point(600, 44);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(42, 23);
            this.lblFromDate.TabIndex = 9;
            this.lblFromDate.Text = "From";

            this.dtFromDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtFromDate.FormatString = "dd-MM-yyyy";
            this.dtFromDate.Location = new System.Drawing.Point(645, 42);
            this.dtFromDate.Name = "dtFromDate";
            this.dtFromDate.Size = new System.Drawing.Size(115, 24);
            this.dtFromDate.TabIndex = 10;

            this.lblToDate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblToDate.Location = new System.Drawing.Point(772, 44);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(28, 23);
            this.lblToDate.TabIndex = 11;
            this.lblToDate.Text = "To";

            this.dtToDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtToDate.FormatString = "dd-MM-yyyy";
            this.dtToDate.Location = new System.Drawing.Point(804, 42);
            this.dtToDate.Name = "dtToDate";
            this.dtToDate.Size = new System.Drawing.Size(115, 24);
            this.dtToDate.TabIndex = 12;

            // =========================================================================
            // ultraPanelAction (Action Bar, Height: 45)
            // =========================================================================
            this.ultraPanelAction.Appearance = appearanceAction;
            this.ultraPanelAction.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnGenerate);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPrint);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnExportCsv);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnClearFilters);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnToggleSelection);
            this.ultraPanelAction.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelAction.Location = new System.Drawing.Point(0, 78);
            this.ultraPanelAction.Name = "ultraPanelAction";
            this.ultraPanelAction.Size = new System.Drawing.Size(1280, 45);
            this.ultraPanelAction.TabIndex = 1;

            this.btnGenerate.Location = new System.Drawing.Point(12, 7);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(95, 30);
            this.btnGenerate.TabIndex = 0;
            this.btnGenerate.Text = "View Grid";

            this.btnPreviewGrid.Location = new System.Drawing.Point(114, 7);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(95, 30);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid";

            this.btnPrint.Location = new System.Drawing.Point(216, 7);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(100, 30);
            this.btnPrint.TabIndex = 2;
            this.btnPrint.Text = "Preview Report";

            this.btnExportCsv.Location = new System.Drawing.Point(323, 7);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(95, 30);
            this.btnExportCsv.TabIndex = 3;
            this.btnExportCsv.Text = "Export Grid";

            this.btnClearFilters.Location = new System.Drawing.Point(425, 7);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(95, 30);
            this.btnClearFilters.TabIndex = 4;
            this.btnClearFilters.Text = "Reset Filters";

            this.btnToggleSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleSelection.Location = new System.Drawing.Point(1160, 7);
            this.btnToggleSelection.Name = "btnToggleSelection";
            this.btnToggleSelection.Size = new System.Drawing.Size(110, 30);
            this.btnToggleSelection.TabIndex = 5;
            this.btnToggleSelection.Text = "Hide Selection";

            // =========================================================================
            // ultraPanelMaster (Dock: Fill)
            // =========================================================================
            appearanceMaster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearanceMaster.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelMaster.Appearance = appearanceMaster;
            this.ultraPanelMaster.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraGridTransactions);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 123);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1280, 577);
            this.ultraPanelMaster.TabIndex = 2;

            appearanceGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ultraGridTransactions.DisplayLayout.Appearance = appearanceGrid;
            this.ultraGridTransactions.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraGridTransactions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridTransactions.Location = new System.Drawing.Point(0, 0);
            this.ultraGridTransactions.Name = "ultraGridTransactions";
            this.ultraGridTransactions.Size = new System.Drawing.Size(1280, 543);
            this.ultraGridTransactions.TabIndex = 0;
            this.ultraGridTransactions.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            appearanceFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceFooter.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(118)))), ((int)(((byte)(184)))));
            appearanceFooter.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelGridFooter.Appearance = appearanceFooter;
            this.ultraPanelGridFooter.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblOpeningSummary);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblDebitSummary);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblCreditSummary);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblClosingBadge);
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 543);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1280, 34);
            this.ultraPanelGridFooter.TabIndex = 1;

            this.lblOpeningSummary.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblOpeningSummary.Location = new System.Drawing.Point(12, 6);
            this.lblOpeningSummary.Name = "lblOpeningSummary";
            this.lblOpeningSummary.Size = new System.Drawing.Size(260, 22);
            this.lblOpeningSummary.TabIndex = 0;
            this.lblOpeningSummary.Text = "Opening: ₹ 0.00 Dr";

            this.lblDebitSummary.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblDebitSummary.Location = new System.Drawing.Point(280, 6);
            this.lblDebitSummary.Name = "lblDebitSummary";
            this.lblDebitSummary.Size = new System.Drawing.Size(260, 22);
            this.lblDebitSummary.TabIndex = 1;
            this.lblDebitSummary.Text = "Total Debit (Dr): ₹ 0.00";

            this.lblCreditSummary.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCreditSummary.Location = new System.Drawing.Point(550, 6);
            this.lblCreditSummary.Name = "lblCreditSummary";
            this.lblCreditSummary.Size = new System.Drawing.Size(260, 22);
            this.lblCreditSummary.TabIndex = 2;
            this.lblCreditSummary.Text = "Total Credit (Cr): ₹ 0.00";

            this.lblClosingBadge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearanceClosingBadge.TextHAlign = Infragistics.Win.HAlign.Center;
            appearanceClosingBadge.TextVAlign = Infragistics.Win.VAlign.Middle;
            this.lblClosingBadge.Appearance = appearanceClosingBadge;
            this.lblClosingBadge.BorderStyleInner = Infragistics.Win.UIElementBorderStyle.Solid;
            this.lblClosingBadge.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblClosingBadge.Location = new System.Drawing.Point(980, 4);
            this.lblClosingBadge.Name = "lblClosingBadge";
            this.lblClosingBadge.Size = new System.Drawing.Size(290, 26);
            this.lblClosingBadge.TabIndex = 3;
            this.lblClosingBadge.Text = "CLOSING: ₹ 0.00 Dr";

            // =========================================================================
            // FrmGeneralLedger Form
            // =========================================================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1280, 700);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelAction);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmGeneralLedger";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "General Ledger Statement";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbGroupFilter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboLedger)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDateQuickSelect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).EndInit();

            this.ultraPanelAction.ClientArea.ResumeLayout(false);
            this.ultraPanelAction.ResumeLayout(false);

            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTransactions)).EndInit();
            this.ultraPanelGridFooter.ClientArea.ResumeLayout(false);
            this.ultraPanelGridFooter.ResumeLayout(false);

            this.ResumeLayout(false);
        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblGroupFilter;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbGroupFilter;
        private Infragistics.Win.Misc.UltraLabel lblRowCount;
        private Infragistics.Win.Misc.UltraLabel lblLedger;
        private Infragistics.Win.UltraWinGrid.UltraCombo ultraComboLedger;
        private Infragistics.Win.Misc.UltraLabel lblPreset;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbDateQuickSelect;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFromDate;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtToDate;

        private Infragistics.Win.Misc.UltraPanel ultraPanelAction;
        private Infragistics.Win.Misc.UltraButton btnGenerate;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnExportCsv;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnToggleSelection;

        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridTransactions;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
        private Infragistics.Win.Misc.UltraLabel lblOpeningSummary;
        private Infragistics.Win.Misc.UltraLabel lblDebitSummary;
        private Infragistics.Win.Misc.UltraLabel lblCreditSummary;
        private Infragistics.Win.Misc.UltraLabel lblClosingBadge;
    }
}

