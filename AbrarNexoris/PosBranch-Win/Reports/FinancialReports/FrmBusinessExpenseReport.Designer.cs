namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmBusinessExpenseReport
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
            Infragistics.Win.Appearance appearanceTotalBadge = new Infragistics.Win.Appearance();

            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblViewMode = new Infragistics.Win.Misc.UltraLabel();
            this.cmbViewMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCategory = new Infragistics.Win.Misc.UltraLabel();
            this.cmbCategory = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblRowCount = new Infragistics.Win.Misc.UltraLabel();
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
            this.btnExportExcel = new Infragistics.Win.Misc.UltraButton();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnToggleSelection = new Infragistics.Win.Misc.UltraButton();

            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridExpenses = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.lblDirectExpensesSummary = new Infragistics.Win.Misc.UltraLabel();
            this.lblIndirectExpensesSummary = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalVouchersSummary = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalExpensesSummary = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbViewMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDateQuickSelect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).BeginInit();

            this.ultraPanelAction.ClientArea.SuspendLayout();
            this.ultraPanelAction.SuspendLayout();

            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridExpenses)).BeginInit();
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
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblViewMode);
            this.ultraPanelControls.ClientArea.Controls.Add(this.cmbViewMode);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblCategory);
            this.ultraPanelControls.ClientArea.Controls.Add(this.cmbCategory);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblRowCount);
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

            // Row 1: Search, View Mode, Category & Row Count
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSearch.Location = new System.Drawing.Point(12, 12);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(55, 23);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search";

            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.Location = new System.Drawing.Point(72, 9);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.NullText = "Search ledger name, group, voucher no, narration...";
            this.txtSearch.Size = new System.Drawing.Size(280, 24);
            this.txtSearch.TabIndex = 1;

            this.lblViewMode.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblViewMode.Location = new System.Drawing.Point(365, 12);
            this.lblViewMode.Name = "lblViewMode";
            this.lblViewMode.Size = new System.Drawing.Size(45, 23);
            this.lblViewMode.TabIndex = 2;
            this.lblViewMode.Text = "View";

            this.cmbViewMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbViewMode.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbViewMode.Location = new System.Drawing.Point(415, 9);
            this.cmbViewMode.Name = "cmbViewMode";
            this.cmbViewMode.Size = new System.Drawing.Size(175, 24);
            this.cmbViewMode.TabIndex = 3;

            this.lblCategory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCategory.Location = new System.Drawing.Point(605, 12);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(60, 23);
            this.lblCategory.TabIndex = 4;
            this.lblCategory.Text = "Category";

            this.cmbCategory.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbCategory.Location = new System.Drawing.Point(670, 9);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(155, 24);
            this.cmbCategory.TabIndex = 5;

            this.lblRowCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.lblRowCount.Location = new System.Drawing.Point(845, 12);
            this.lblRowCount.Name = "lblRowCount";
            this.lblRowCount.Size = new System.Drawing.Size(220, 20);
            this.lblRowCount.TabIndex = 6;
            this.lblRowCount.Text = "Total: 0 records";

            // Row 2: Period & Date Range
            this.lblPreset.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPreset.Location = new System.Drawing.Point(12, 44);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(55, 23);
            this.lblPreset.TabIndex = 7;
            this.lblPreset.Text = "Period";

            this.cmbDateQuickSelect.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbDateQuickSelect.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbDateQuickSelect.Location = new System.Drawing.Point(72, 42);
            this.cmbDateQuickSelect.Name = "cmbDateQuickSelect";
            this.cmbDateQuickSelect.Size = new System.Drawing.Size(165, 24);
            this.cmbDateQuickSelect.TabIndex = 8;

            this.lblFromDate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFromDate.Location = new System.Drawing.Point(255, 44);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(42, 23);
            this.lblFromDate.TabIndex = 9;
            this.lblFromDate.Text = "From";

            this.dtFromDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtFromDate.FormatString = "dd-MM-yyyy";
            this.dtFromDate.Location = new System.Drawing.Point(300, 42);
            this.dtFromDate.Name = "dtFromDate";
            this.dtFromDate.Size = new System.Drawing.Size(115, 24);
            this.dtFromDate.TabIndex = 10;

            this.lblToDate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblToDate.Location = new System.Drawing.Point(430, 44);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(30, 23);
            this.lblToDate.TabIndex = 11;
            this.lblToDate.Text = "To";

            this.dtToDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtToDate.FormatString = "dd-MM-yyyy";
            this.dtToDate.Location = new System.Drawing.Point(465, 42);
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
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnExportExcel);
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
            this.btnExportCsv.Text = "Export CSV";

            this.btnExportExcel.Location = new System.Drawing.Point(425, 7);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(95, 30);
            this.btnExportExcel.TabIndex = 4;
            this.btnExportExcel.Text = "Export Excel";

            this.btnClearFilters.Location = new System.Drawing.Point(527, 7);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(95, 30);
            this.btnClearFilters.TabIndex = 5;
            this.btnClearFilters.Text = "Reset Filters";

            this.btnToggleSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleSelection.Location = new System.Drawing.Point(1160, 7);
            this.btnToggleSelection.Name = "btnToggleSelection";
            this.btnToggleSelection.Size = new System.Drawing.Size(110, 30);
            this.btnToggleSelection.TabIndex = 6;
            this.btnToggleSelection.Text = "Hide Selection";

            // =========================================================================
            // ultraPanelMaster (Dock: Fill)
            // =========================================================================
            appearanceMaster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearanceMaster.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelMaster.Appearance = appearanceMaster;
            this.ultraPanelMaster.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraGridExpenses);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 123);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1280, 577);
            this.ultraPanelMaster.TabIndex = 2;

            // 
            // ultraGridExpenses
            // 
            appearanceGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ultraGridExpenses.DisplayLayout.Appearance = appearanceGrid;
            this.ultraGridExpenses.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraGridExpenses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridExpenses.Location = new System.Drawing.Point(0, 0);
            this.ultraGridExpenses.Name = "ultraGridExpenses";
            this.ultraGridExpenses.Size = new System.Drawing.Size(1280, 541);
            this.ultraGridExpenses.TabIndex = 0;
            this.ultraGridExpenses.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // 
            // ultraPanelGridFooter
            // 
            appearanceFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceFooter.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(118)))), ((int)(((byte)(184)))));
            appearanceFooter.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelGridFooter.Appearance = appearanceFooter;
            this.ultraPanelGridFooter.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblDirectExpensesSummary);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblIndirectExpensesSummary);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblTotalVouchersSummary);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblTotalExpensesSummary);
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 541);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1280, 36);
            this.ultraPanelGridFooter.TabIndex = 1;

            // Footer Labels
            this.lblDirectExpensesSummary.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDirectExpensesSummary.Location = new System.Drawing.Point(14, 7);
            this.lblDirectExpensesSummary.Name = "lblDirectExpensesSummary";
            this.lblDirectExpensesSummary.Size = new System.Drawing.Size(260, 22);
            this.lblDirectExpensesSummary.TabIndex = 0;
            this.lblDirectExpensesSummary.Text = "Direct Expenses: ₹ 0.00";

            this.lblIndirectExpensesSummary.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblIndirectExpensesSummary.Location = new System.Drawing.Point(285, 7);
            this.lblIndirectExpensesSummary.Name = "lblIndirectExpensesSummary";
            this.lblIndirectExpensesSummary.Size = new System.Drawing.Size(270, 22);
            this.lblIndirectExpensesSummary.TabIndex = 1;
            this.lblIndirectExpensesSummary.Text = "Indirect Expenses: ₹ 0.00";

            this.lblTotalVouchersSummary.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalVouchersSummary.Location = new System.Drawing.Point(565, 7);
            this.lblTotalVouchersSummary.Name = "lblTotalVouchersSummary";
            this.lblTotalVouchersSummary.Size = new System.Drawing.Size(220, 22);
            this.lblTotalVouchersSummary.TabIndex = 2;
            this.lblTotalVouchersSummary.Text = "Vouchers: 0";

            this.lblTotalExpensesSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearanceTotalBadge.TextHAlign = Infragistics.Win.HAlign.Center;
            appearanceTotalBadge.TextVAlign = Infragistics.Win.VAlign.Middle;
            this.lblTotalExpensesSummary.Appearance = appearanceTotalBadge;
            this.lblTotalExpensesSummary.BorderStyleInner = Infragistics.Win.UIElementBorderStyle.Solid;
            this.lblTotalExpensesSummary.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalExpensesSummary.Location = new System.Drawing.Point(960, 4);
            this.lblTotalExpensesSummary.Name = "lblTotalExpensesSummary";
            this.lblTotalExpensesSummary.Size = new System.Drawing.Size(310, 28);
            this.lblTotalExpensesSummary.TabIndex = 3;
            this.lblTotalExpensesSummary.Text = "TOTAL EXPENSES: ₹ 0.00";

            // =========================================================================
            // FrmBusinessExpenseReport Form
            // =========================================================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1280, 700);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelAction);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmBusinessExpenseReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Business Expenses Report";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbViewMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDateQuickSelect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).EndInit();

            this.ultraPanelAction.ClientArea.ResumeLayout(false);
            this.ultraPanelAction.ResumeLayout(false);

            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridExpenses)).EndInit();
            this.ultraPanelGridFooter.ClientArea.ResumeLayout(false);
            this.ultraPanelGridFooter.ResumeLayout(false);

            this.ResumeLayout(false);
        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblViewMode;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbViewMode;
        private Infragistics.Win.Misc.UltraLabel lblCategory;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbCategory;
        private Infragistics.Win.Misc.UltraLabel lblRowCount;
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
        private Infragistics.Win.Misc.UltraButton btnExportExcel;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnToggleSelection;

        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridExpenses;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
        private Infragistics.Win.Misc.UltraLabel lblDirectExpensesSummary;
        private Infragistics.Win.Misc.UltraLabel lblIndirectExpensesSummary;
        private Infragistics.Win.Misc.UltraLabel lblTotalVouchersSummary;
        private Infragistics.Win.Misc.UltraLabel lblTotalExpensesSummary;
    }
}
