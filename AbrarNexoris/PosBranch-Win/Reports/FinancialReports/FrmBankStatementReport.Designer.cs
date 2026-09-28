namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmBankStatementReport
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
            Infragistics.Win.Appearance appearanceFooter = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceNetBadge = new Infragistics.Win.Appearance();

            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblRowCount = new Infragistics.Win.Misc.UltraLabel();
            this.lblPaymentMethod = new Infragistics.Win.Misc.UltraLabel();
            this.cmbPaymentMethod = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
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
            this.lblMoneyInSummary = new Infragistics.Win.Misc.UltraLabel();
            this.lblMoneyOutSummary = new Infragistics.Win.Misc.UltraLabel();
            this.lblBreakdownSummary = new Infragistics.Win.Misc.UltraLabel();
            this.lblNetBadge = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymentMethod)).BeginInit();
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
            // ultraPanelControls (Filter Bar, Height: 78)
            // =========================================================================
            appearanceControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearanceControls.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelControls.Appearance = appearanceControls;
            this.ultraPanelControls.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblRowCount);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPaymentMethod);
            this.ultraPanelControls.ClientArea.Controls.Add(this.cmbPaymentMethod);
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

            // Row 1: Search & Row Count
            this.lblSearch.Location = new System.Drawing.Point(12, 10);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(55, 23);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search";

            this.txtSearch.Location = new System.Drawing.Point(72, 8);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.NullText = "Search party, reference, voucher no, payment method...";
            this.txtSearch.Size = new System.Drawing.Size(260, 24);
            this.txtSearch.TabIndex = 1;

            this.lblRowCount.Location = new System.Drawing.Point(345, 10);
            this.lblRowCount.Name = "lblRowCount";
            this.lblRowCount.Size = new System.Drawing.Size(200, 23);
            this.lblRowCount.TabIndex = 2;
            this.lblRowCount.Text = "Total: 0 rows";

            // Row 2: Pay Mode & Period & Dates
            this.lblPaymentMethod.Location = new System.Drawing.Point(12, 44);
            this.lblPaymentMethod.Name = "lblPaymentMethod";
            this.lblPaymentMethod.Size = new System.Drawing.Size(55, 23);
            this.lblPaymentMethod.TabIndex = 3;
            this.lblPaymentMethod.Text = "Pay Mode";

            this.cmbPaymentMethod.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbPaymentMethod.Location = new System.Drawing.Point(72, 42);
            this.cmbPaymentMethod.Name = "cmbPaymentMethod";
            this.cmbPaymentMethod.Size = new System.Drawing.Size(150, 24);
            this.cmbPaymentMethod.TabIndex = 4;

            this.lblPreset.Location = new System.Drawing.Point(235, 44);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(42, 23);
            this.lblPreset.TabIndex = 5;
            this.lblPreset.Text = "Period";

            this.cmbDateQuickSelect.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbDateQuickSelect.Location = new System.Drawing.Point(282, 42);
            this.cmbDateQuickSelect.Name = "cmbDateQuickSelect";
            this.cmbDateQuickSelect.Size = new System.Drawing.Size(140, 24);
            this.cmbDateQuickSelect.TabIndex = 6;

            this.lblFromDate.Location = new System.Drawing.Point(435, 44);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(38, 23);
            this.lblFromDate.TabIndex = 7;
            this.lblFromDate.Text = "From";

            this.dtFromDate.FormatString = "dd-MM-yyyy";
            this.dtFromDate.Location = new System.Drawing.Point(475, 42);
            this.dtFromDate.Name = "dtFromDate";
            this.dtFromDate.Size = new System.Drawing.Size(115, 24);
            this.dtFromDate.TabIndex = 8;

            this.lblToDate.Location = new System.Drawing.Point(600, 44);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(25, 23);
            this.lblToDate.TabIndex = 9;
            this.lblToDate.Text = "To";

            this.dtToDate.FormatString = "dd-MM-yyyy";
            this.dtToDate.Location = new System.Drawing.Point(630, 42);
            this.dtToDate.Name = "dtToDate";
            this.dtToDate.Size = new System.Drawing.Size(115, 24);
            this.dtToDate.TabIndex = 10;

            // =========================================================================
            // ultraPanelAction (Action Bar, Height: 45)
            // =========================================================================
            appearanceAction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(238)))), ((int)(((byte)(248)))));
            appearanceAction.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
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

            // ultraGridTransactions
            appearanceGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ultraGridTransactions.DisplayLayout.Appearance = appearanceGrid;
            this.ultraGridTransactions.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraGridTransactions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridTransactions.Location = new System.Drawing.Point(0, 0);
            this.ultraGridTransactions.Name = "ultraGridTransactions";
            this.ultraGridTransactions.Size = new System.Drawing.Size(1280, 543);
            this.ultraGridTransactions.TabIndex = 0;
            this.ultraGridTransactions.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // ultraPanelGridFooter
            appearanceFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(137)))));
            appearanceFooter.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(62)))), ((int)(((byte)(108)))));
            appearanceFooter.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelGridFooter.Appearance = appearanceFooter;
            this.ultraPanelGridFooter.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblMoneyInSummary);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblMoneyOutSummary);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblBreakdownSummary);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblNetBadge);
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 543);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1280, 34);
            this.ultraPanelGridFooter.TabIndex = 1;

            // Footer Labels
            this.lblMoneyInSummary.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblMoneyInSummary.Location = new System.Drawing.Point(12, 6);
            this.lblMoneyInSummary.Name = "lblMoneyInSummary";
            this.lblMoneyInSummary.Size = new System.Drawing.Size(220, 22);
            this.lblMoneyInSummary.TabIndex = 0;
            this.lblMoneyInSummary.Text = "Money In: ₹ 0.00";

            this.lblMoneyOutSummary.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblMoneyOutSummary.Location = new System.Drawing.Point(235, 6);
            this.lblMoneyOutSummary.Name = "lblMoneyOutSummary";
            this.lblMoneyOutSummary.Size = new System.Drawing.Size(220, 22);
            this.lblMoneyOutSummary.TabIndex = 1;
            this.lblMoneyOutSummary.Text = "Money Out: ₹ 0.00";

            this.lblBreakdownSummary.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular);
            this.lblBreakdownSummary.Location = new System.Drawing.Point(460, 6);
            this.lblBreakdownSummary.Name = "lblBreakdownSummary";
            this.lblBreakdownSummary.Size = new System.Drawing.Size(480, 22);
            this.lblBreakdownSummary.TabIndex = 2;
            this.lblBreakdownSummary.Text = "UPI: 0.00 | Transfer: 0.00 | Card: 0.00 | Cheque: 0.00";

            this.lblNetBadge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearanceNetBadge.TextHAlign = Infragistics.Win.HAlign.Center;
            appearanceNetBadge.TextVAlign = Infragistics.Win.VAlign.Middle;
            this.lblNetBadge.Appearance = appearanceNetBadge;
            this.lblNetBadge.BorderStyleInner = Infragistics.Win.UIElementBorderStyle.Solid;
            this.lblNetBadge.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblNetBadge.Location = new System.Drawing.Point(950, 4);
            this.lblNetBadge.Name = "lblNetBadge";
            this.lblNetBadge.Size = new System.Drawing.Size(320, 26);
            this.lblNetBadge.TabIndex = 3;
            this.lblNetBadge.Text = "NET BANK EFFECT: ₹ 0.00 In";

            // =========================================================================
            // FrmBankStatementReport Form
            // =========================================================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1280, 700);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelAction);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmBankStatementReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bank Statement Report";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymentMethod)).EndInit();
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
        private Infragistics.Win.Misc.UltraLabel lblRowCount;
        private Infragistics.Win.Misc.UltraLabel lblPaymentMethod;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbPaymentMethod;
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
        private Infragistics.Win.Misc.UltraLabel lblMoneyInSummary;
        private Infragistics.Win.Misc.UltraLabel lblMoneyOutSummary;
        private Infragistics.Win.Misc.UltraLabel lblBreakdownSummary;
        private Infragistics.Win.Misc.UltraLabel lblNetBadge;
    }
}
