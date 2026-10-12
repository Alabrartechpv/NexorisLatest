namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmCashInOutReport
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
            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.lblPreset = new Infragistics.Win.Misc.UltraLabel();
            this.cmbDateQuickSelect = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblCategory = new Infragistics.Win.Misc.UltraLabel();
            this.cmbCategory = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();

            this.ultraPanelActionBar = new Infragistics.Win.Misc.UltraPanel();
            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewReport = new Infragistics.Win.Misc.UltraButton();
            this.btnExportExcel = new Infragistics.Win.Misc.UltraButton();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnColumnChooser = new Infragistics.Win.Misc.UltraButton();
            this.btnHideSelection = new Infragistics.Win.Misc.UltraButton();

            this.ultraPanelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.pnlCardNet = new System.Windows.Forms.Panel();
            this.lblCardNetValue = new System.Windows.Forms.Label();
            this.lblCardNetTitle = new System.Windows.Forms.Label();
            this.pnlCardOut = new System.Windows.Forms.Panel();
            this.lblCardOutValue = new System.Windows.Forms.Label();
            this.lblCardOutTitle = new System.Windows.Forms.Label();
            this.pnlCardIn = new System.Windows.Forms.Panel();
            this.lblCardInValue = new System.Windows.Forms.Label();
            this.lblCardInTitle = new System.Windows.Forms.Label();
            this.pnlCardBalance = new System.Windows.Forms.Panel();
            this.lblCardBalanceValue = new System.Windows.Forms.Label();
            this.lblCardBalanceTitle = new System.Windows.Forms.Label();

            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.lblFooterSummary = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDateQuickSelect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();

            this.ultraPanelActionBar.ClientArea.SuspendLayout();
            this.ultraPanelActionBar.SuspendLayout();

            this.ultraPanelSummary.ClientArea.SuspendLayout();
            this.ultraPanelSummary.SuspendLayout();
            this.pnlCardNet.SuspendLayout();
            this.pnlCardOut.SuspendLayout();
            this.pnlCardIn.SuspendLayout();
            this.pnlCardBalance.SuspendLayout();

            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.ultraPanelGridFooter.ClientArea.SuspendLayout();
            this.ultraPanelGridFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // ultraPanelControls (Filter Selection Bar)
            // 
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.cmbDateQuickSelect);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtTo);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblCategory);
            this.ultraPanelControls.ClientArea.Controls.Add(this.cmbCategory);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtSearch);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1280, 48);
            this.ultraPanelControls.TabIndex = 0;
            // 
            // lblPreset
            // 
            this.lblPreset.Location = new System.Drawing.Point(12, 14);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(48, 18);
            this.lblPreset.TabIndex = 0;
            this.lblPreset.Text = "Period:";
            // 
            // cmbDateQuickSelect
            // 
            this.cmbDateQuickSelect.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbDateQuickSelect.Location = new System.Drawing.Point(62, 11);
            this.cmbDateQuickSelect.Name = "cmbDateQuickSelect";
            this.cmbDateQuickSelect.Size = new System.Drawing.Size(125, 24);
            this.cmbDateQuickSelect.TabIndex = 1;
            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(198, 14);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(40, 18);
            this.lblFromDate.TabIndex = 2;
            this.lblFromDate.Text = "From:";
            // 
            // dtFrom
            // 
            this.dtFrom.FormatString = "dd/MM/yyyy";
            this.dtFrom.Location = new System.Drawing.Point(240, 11);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(115, 24);
            this.dtFrom.TabIndex = 3;
            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(365, 14);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(26, 18);
            this.lblToDate.TabIndex = 4;
            this.lblToDate.Text = "To:";
            // 
            // dtTo
            // 
            this.dtTo.FormatString = "dd/MM/yyyy";
            this.dtTo.Location = new System.Drawing.Point(395, 11);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(115, 24);
            this.dtTo.TabIndex = 5;
            // 
            // lblCategory
            // 
            this.lblCategory.Location = new System.Drawing.Point(522, 14);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(40, 18);
            this.lblCategory.TabIndex = 6;
            this.lblCategory.Text = "Type:";
            // 
            // cmbCategory
            // 
            this.cmbCategory.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbCategory.Location = new System.Drawing.Point(565, 11);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(155, 24);
            this.cmbCategory.TabIndex = 7;
            // 
            // lblSearch
            // 
            this.lblSearch.Location = new System.Drawing.Point(732, 14);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(48, 18);
            this.lblSearch.TabIndex = 8;
            this.lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(782, 11);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(260, 24);
            this.txtSearch.TabIndex = 9;
            // 
            // ultraPanelActionBar (Standard Nexoris Action Button Bar)
            // 
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnViewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewReport);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnExportExcel);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnClearFilters);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnColumnChooser);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnHideSelection);
            this.ultraPanelActionBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelActionBar.Location = new System.Drawing.Point(0, 48);
            this.ultraPanelActionBar.Name = "ultraPanelActionBar";
            this.ultraPanelActionBar.Size = new System.Drawing.Size(1280, 45);
            this.ultraPanelActionBar.TabIndex = 1;
            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Location = new System.Drawing.Point(10, 8);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(136, 28);
            this.btnViewGrid.TabIndex = 0;
            this.btnViewGrid.Text = "&View Grid (F5)";
            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(153, 8);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(138, 28);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "&Preview Grid";
            // 
            // btnPreviewReport
            // 
            this.btnPreviewReport.Location = new System.Drawing.Point(298, 8);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(145, 28);
            this.btnPreviewReport.TabIndex = 2;
            this.btnPreviewReport.Text = "Preview &Report";
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Location = new System.Drawing.Point(450, 8);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(110, 28);
            this.btnExportExcel.TabIndex = 3;
            this.btnExportExcel.Text = "&Export Grid";
            // 
            // btnClearFilters
            // 
            this.btnClearFilters.Location = new System.Drawing.Point(568, 8);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(110, 28);
            this.btnClearFilters.TabIndex = 4;
            this.btnClearFilters.Text = "&Reset Filters";
            // 
            // btnColumnChooser
            // 
            this.btnColumnChooser.Location = new System.Drawing.Point(686, 8);
            this.btnColumnChooser.Name = "btnColumnChooser";
            this.btnColumnChooser.Size = new System.Drawing.Size(130, 28);
            this.btnColumnChooser.TabIndex = 5;
            this.btnColumnChooser.Text = "Column &Chooser";
            // 
            // btnHideSelection
            // 
            this.btnHideSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHideSelection.Location = new System.Drawing.Point(1135, 8);
            this.btnHideSelection.Name = "btnHideSelection";
            this.btnHideSelection.Size = new System.Drawing.Size(135, 28);
            this.btnHideSelection.TabIndex = 6;
            this.btnHideSelection.Text = "&Hide Selection";
            // 
            // ultraPanelSummary (KPI Cards)
            // 
            this.ultraPanelSummary.ClientArea.Controls.Add(this.pnlCardNet);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.pnlCardOut);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.pnlCardIn);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.pnlCardBalance);
            this.ultraPanelSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelSummary.Location = new System.Drawing.Point(0, 93);
            this.ultraPanelSummary.Name = "ultraPanelSummary";
            this.ultraPanelSummary.Size = new System.Drawing.Size(1280, 68);
            this.ultraPanelSummary.TabIndex = 2;
            // 
            // pnlCardIn
            // 
            this.pnlCardIn.BackColor = System.Drawing.Color.White;
            this.pnlCardIn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardIn.Controls.Add(this.lblCardInValue);
            this.pnlCardIn.Controls.Add(this.lblCardInTitle);
            this.pnlCardIn.Location = new System.Drawing.Point(10, 6);
            this.pnlCardIn.Name = "pnlCardIn";
            this.pnlCardIn.Size = new System.Drawing.Size(260, 54);
            this.pnlCardIn.TabIndex = 0;
            // 
            // lblCardInTitle
            // 
            this.lblCardInTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardInTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.lblCardInTitle.Location = new System.Drawing.Point(8, 4);
            this.lblCardInTitle.Name = "lblCardInTitle";
            this.lblCardInTitle.Size = new System.Drawing.Size(240, 16);
            this.lblCardInTitle.TabIndex = 0;
            this.lblCardInTitle.Text = "TOTAL CASH IN";
            // 
            // lblCardInValue
            // 
            this.lblCardInValue.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblCardInValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.lblCardInValue.Location = new System.Drawing.Point(8, 22);
            this.lblCardInValue.Name = "lblCardInValue";
            this.lblCardInValue.Size = new System.Drawing.Size(240, 26);
            this.lblCardInValue.TabIndex = 1;
            this.lblCardInValue.Text = "₹ 0.00";
            // 
            // pnlCardOut
            // 
            this.pnlCardOut.BackColor = System.Drawing.Color.White;
            this.pnlCardOut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardOut.Controls.Add(this.lblCardOutValue);
            this.pnlCardOut.Controls.Add(this.lblCardOutTitle);
            this.pnlCardOut.Location = new System.Drawing.Point(280, 6);
            this.pnlCardOut.Name = "pnlCardOut";
            this.pnlCardOut.Size = new System.Drawing.Size(260, 54);
            this.pnlCardOut.TabIndex = 1;
            // 
            // lblCardOutTitle
            // 
            this.lblCardOutTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardOutTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.lblCardOutTitle.Location = new System.Drawing.Point(8, 4);
            this.lblCardOutTitle.Name = "lblCardOutTitle";
            this.lblCardOutTitle.Size = new System.Drawing.Size(240, 16);
            this.lblCardOutTitle.TabIndex = 0;
            this.lblCardOutTitle.Text = "TOTAL CASH OUT";
            // 
            // lblCardOutValue
            // 
            this.lblCardOutValue.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblCardOutValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.lblCardOutValue.Location = new System.Drawing.Point(8, 22);
            this.lblCardOutValue.Name = "lblCardOutValue";
            this.lblCardOutValue.Size = new System.Drawing.Size(240, 26);
            this.lblCardOutValue.TabIndex = 1;
            this.lblCardOutValue.Text = "₹ 0.00";
            // 
            // pnlCardNet
            // 
            this.pnlCardNet.BackColor = System.Drawing.Color.White;
            this.pnlCardNet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardNet.Controls.Add(this.lblCardNetValue);
            this.pnlCardNet.Controls.Add(this.lblCardNetTitle);
            this.pnlCardNet.Location = new System.Drawing.Point(550, 6);
            this.pnlCardNet.Name = "pnlCardNet";
            this.pnlCardNet.Size = new System.Drawing.Size(260, 54);
            this.pnlCardNet.TabIndex = 2;
            // 
            // lblCardNetTitle
            // 
            this.lblCardNetTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardNetTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(138)))));
            this.lblCardNetTitle.Location = new System.Drawing.Point(8, 4);
            this.lblCardNetTitle.Name = "lblCardNetTitle";
            this.lblCardNetTitle.Size = new System.Drawing.Size(240, 16);
            this.lblCardNetTitle.TabIndex = 0;
            this.lblCardNetTitle.Text = "NET CASH FLOW";
            // 
            // lblCardNetValue
            // 
            this.lblCardNetValue.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblCardNetValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(138)))));
            this.lblCardNetValue.Location = new System.Drawing.Point(8, 22);
            this.lblCardNetValue.Name = "lblCardNetValue";
            this.lblCardNetValue.Size = new System.Drawing.Size(240, 26);
            this.lblCardNetValue.TabIndex = 1;
            this.lblCardNetValue.Text = "₹ 0.00";
            // 
            // pnlCardBalance
            // 
            this.pnlCardBalance.BackColor = System.Drawing.Color.White;
            this.pnlCardBalance.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCardBalance.Controls.Add(this.lblCardBalanceValue);
            this.pnlCardBalance.Controls.Add(this.lblCardBalanceTitle);
            this.pnlCardBalance.Location = new System.Drawing.Point(820, 6);
            this.pnlCardBalance.Name = "pnlCardBalance";
            this.pnlCardBalance.Size = new System.Drawing.Size(260, 54);
            this.pnlCardBalance.TabIndex = 3;
            // 
            // lblCardBalanceTitle
            // 
            this.lblCardBalanceTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardBalanceTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblCardBalanceTitle.Location = new System.Drawing.Point(8, 4);
            this.lblCardBalanceTitle.Name = "lblCardBalanceTitle";
            this.lblCardBalanceTitle.Size = new System.Drawing.Size(240, 16);
            this.lblCardBalanceTitle.TabIndex = 0;
            this.lblCardBalanceTitle.Text = "CLOSING BALANCE";
            // 
            // lblCardBalanceValue
            // 
            this.lblCardBalanceValue.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblCardBalanceValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.lblCardBalanceValue.Location = new System.Drawing.Point(8, 22);
            this.lblCardBalanceValue.Name = "lblCardBalanceValue";
            this.lblCardBalanceValue.Size = new System.Drawing.Size(240, 26);
            this.lblCardBalanceValue.TabIndex = 1;
            this.lblCardBalanceValue.Text = "₹ 0.00";
            // 
            // ultraPanelMaster (Grid Container with Footer)
            // 
            this.ultraPanelMaster.ClientArea.Controls.Add(this.gridReport);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 161);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1280, 561);
            this.ultraPanelMaster.TabIndex = 3;
            // 
            // gridReport
            // 
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Location = new System.Drawing.Point(0, 0);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1280, 529);
            this.gridReport.TabIndex = 0;
            // 
            // ultraPanelGridFooter
            // 
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblFooterSummary);
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 529);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1280, 32);
            this.ultraPanelGridFooter.TabIndex = 1;
            // 
            // lblFooterSummary
            // 
            this.lblFooterSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFooterSummary.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblFooterSummary.Location = new System.Drawing.Point(0, 0);
            this.lblFooterSummary.Name = "lblFooterSummary";
            this.lblFooterSummary.Size = new System.Drawing.Size(1280, 32);
            this.lblFooterSummary.TabIndex = 0;
            this.lblFooterSummary.Text = "Ready";
            // 
            // FrmCashInOutReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 722);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelSummary);
            this.Controls.Add(this.ultraPanelActionBar);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmCashInOutReport";
            this.Text = "Cash In / Cash Out Flow Report";
            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmbDateQuickSelect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            this.ultraPanelActionBar.ClientArea.ResumeLayout(false);
            this.ultraPanelActionBar.ResumeLayout(false);
            this.ultraPanelSummary.ClientArea.ResumeLayout(false);
            this.ultraPanelSummary.ResumeLayout(false);
            this.pnlCardNet.ResumeLayout(false);
            this.pnlCardOut.ResumeLayout(false);
            this.pnlCardIn.ResumeLayout(false);
            this.pnlCardBalance.ResumeLayout(false);
            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.ultraPanelGridFooter.ClientArea.ResumeLayout(false);
            this.ultraPanelGridFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraLabel lblPreset;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbDateQuickSelect;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.Misc.UltraLabel lblCategory;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbCategory;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;

        private Infragistics.Win.Misc.UltraPanel ultraPanelActionBar;
        private Infragistics.Win.Misc.UltraButton btnViewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewReport;
        private Infragistics.Win.Misc.UltraButton btnExportExcel;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnColumnChooser;
        private Infragistics.Win.Misc.UltraButton btnHideSelection;

        private Infragistics.Win.Misc.UltraPanel ultraPanelSummary;
        private System.Windows.Forms.Panel pnlCardIn;
        private System.Windows.Forms.Label lblCardInTitle;
        private System.Windows.Forms.Label lblCardInValue;
        private System.Windows.Forms.Panel pnlCardOut;
        private System.Windows.Forms.Label lblCardOutTitle;
        private System.Windows.Forms.Label lblCardOutValue;
        private System.Windows.Forms.Panel pnlCardNet;
        private System.Windows.Forms.Label lblCardNetTitle;
        private System.Windows.Forms.Label lblCardNetValue;
        private System.Windows.Forms.Panel pnlCardBalance;
        private System.Windows.Forms.Label lblCardBalanceTitle;
        private System.Windows.Forms.Label lblCardBalanceValue;

        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
        private Infragistics.Win.Misc.UltraLabel lblFooterSummary;
    }
}
