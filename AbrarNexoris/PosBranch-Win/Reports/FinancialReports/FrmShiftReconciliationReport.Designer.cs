namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmShiftReconciliationReport
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
            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.txtCounterFilter = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblCounterFilter = new Infragistics.Win.Misc.UltraLabel();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnSearch = new Infragistics.Win.Misc.UltraButton();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboPreset = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblPreset = new Infragistics.Win.Misc.UltraLabel();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.lblVariance = new Infragistics.Win.Misc.UltraLabel();
            this.lblVarianceCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblCounted = new Infragistics.Win.Misc.UltraLabel();
            this.lblCountedCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblExpected = new Infragistics.Win.Misc.UltraLabel();
            this.lblExpectedCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblSales = new Infragistics.Win.Misc.UltraLabel();
            this.lblSalesCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCashSale = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCashSaleCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCardSale = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCardSaleCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalUpiSale = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalUpiSaleCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCreditSale = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCreditSaleCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCustReceipt = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCustReceiptCaption = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtCounterFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPreset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.ultraPanelSummary.ClientArea.SuspendLayout();
            this.ultraPanelSummary.SuspendLayout();
            this.SuspendLayout();
            // 
            // ultraPanelControls
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(245)))));
            appearance1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(210)))), ((int)(((byte)(220)))));
            this.ultraPanelControls.Appearance = appearance1;
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtCounterFilter);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblCounterFilter);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnClearFilters);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnPrint);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnExport);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnClose);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtTo);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1349, 118);
            this.ultraPanelControls.TabIndex = 0;
            // 
            // txtCounterFilter
            // 
            this.txtCounterFilter.Location = new System.Drawing.Point(499, 16);
            this.txtCounterFilter.Name = "txtCounterFilter";
            this.txtCounterFilter.Size = new System.Drawing.Size(200, 25);
            this.txtCounterFilter.TabIndex = 5;
            // 
            // lblCounterFilter
            // 
            this.lblCounterFilter.Location = new System.Drawing.Point(428, 19);
            this.lblCounterFilter.Name = "lblCounterFilter";
            this.lblCounterFilter.Size = new System.Drawing.Size(65, 23);
            this.lblCounterFilter.TabIndex = 4;
            this.lblCounterFilter.Text = "Counter:";
            // 
            // btnClearFilters
            // 
            this.btnClearFilters.Location = new System.Drawing.Point(191, 81);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(95, 28);
            this.btnClearFilters.TabIndex = 13;
            this.btnClearFilters.Text = "Clear";
            this.btnClearFilters.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(86, 81);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(95, 28);
            this.btnSearch.TabIndex = 12;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(86, 49);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(250, 25);
            this.txtSearch.TabIndex = 7;
            // 
            // lblSearch
            // 
            this.lblSearch.Location = new System.Drawing.Point(12, 52);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(70, 23);
            this.lblSearch.TabIndex = 6;
            this.lblSearch.Text = "Doc No:";
            // 
            // ultraComboPreset
            // 
            this.ultraComboPreset.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboPreset.Location = new System.Drawing.Point(499, 49);
            this.ultraComboPreset.Name = "ultraComboPreset";
            this.ultraComboPreset.Size = new System.Drawing.Size(125, 25);
            this.ultraComboPreset.TabIndex = 9;
            // 
            // lblPreset
            // 
            this.lblPreset.Location = new System.Drawing.Point(355, 52);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(140, 23);
            this.lblPreset.TabIndex = 8;
            this.lblPreset.Text = "Quick Range:";
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(720, 13);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(115, 30);
            this.btnPrint.TabIndex = 16;
            this.btnPrint.Text = "Print Z-Report";
            this.btnPrint.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(845, 13);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(105, 30);
            this.btnExport.TabIndex = 14;
            this.btnExport.Text = "Export CSV";
            this.btnExport.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(960, 13);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.TabIndex = 15;
            this.btnClose.Text = "Close";
            this.btnClose.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // dtTo
            // 
            this.dtTo.Location = new System.Drawing.Point(320, 16);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(100, 25);
            this.dtTo.TabIndex = 3;
            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(260, 19);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(55, 23);
            this.lblToDate.TabIndex = 2;
            this.lblToDate.Text = "To Date:";
            // 
            // dtFrom
            // 
            this.dtFrom.Location = new System.Drawing.Point(86, 16);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(100, 25);
            this.dtFrom.TabIndex = 1;
            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(12, 19);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(70, 23);
            this.lblFromDate.TabIndex = 0;
            this.lblFromDate.Text = "From Date:";
            // 
            // ultraPanelMaster
            // 
            this.ultraPanelMaster.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraPanelMaster.ClientArea.Controls.Add(this.gridReport);
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 119);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1349, 274);
            this.ultraPanelMaster.TabIndex = 1;
            // 
            // gridReport
            // 
            this.gridReport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridReport.Location = new System.Drawing.Point(12, 7);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1325, 261);
            this.gridReport.TabIndex = 0;
            this.gridReport.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraPanelSummary
            // 
            this.ultraPanelSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblVariance);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblVarianceCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblCounted);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblCountedCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblExpected);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblExpectedCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblSales);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblSalesCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalCashSale);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalCashSaleCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalCardSale);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalCardSaleCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalUpiSale);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalUpiSaleCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalCreditSale);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalCreditSaleCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalCustReceipt);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalCustReceiptCaption);
            this.ultraPanelSummary.Location = new System.Drawing.Point(0, 399);
            this.ultraPanelSummary.Name = "ultraPanelSummary";
            this.ultraPanelSummary.Size = new System.Drawing.Size(1349, 160);
            this.ultraPanelSummary.TabIndex = 2;
            // 
            // lblTotalCashSale
            // 
            this.lblTotalCashSale.Location = new System.Drawing.Point(20, 110);
            this.lblTotalCashSale.Name = "lblTotalCashSale";
            this.lblTotalCashSale.Size = new System.Drawing.Size(180, 30);
            this.lblTotalCashSale.TabIndex = 9;
            this.lblTotalCashSale.Text = "₹ 0.00";
            // 
            // lblTotalCashSaleCaption
            // 
            this.lblTotalCashSaleCaption.Location = new System.Drawing.Point(20, 85);
            this.lblTotalCashSaleCaption.Name = "lblTotalCashSaleCaption";
            this.lblTotalCashSaleCaption.Size = new System.Drawing.Size(180, 25);
            this.lblTotalCashSaleCaption.TabIndex = 8;
            this.lblTotalCashSaleCaption.Text = "Total Cash Sales:";
            // 
            // lblTotalCardSale
            // 
            this.lblTotalCardSale.Location = new System.Drawing.Point(235, 110);
            this.lblTotalCardSale.Name = "lblTotalCardSale";
            this.lblTotalCardSale.Size = new System.Drawing.Size(180, 30);
            this.lblTotalCardSale.TabIndex = 11;
            this.lblTotalCardSale.Text = "₹ 0.00";
            // 
            // lblTotalCardSaleCaption
            // 
            this.lblTotalCardSaleCaption.Location = new System.Drawing.Point(235, 85);
            this.lblTotalCardSaleCaption.Name = "lblTotalCardSaleCaption";
            this.lblTotalCardSaleCaption.Size = new System.Drawing.Size(180, 25);
            this.lblTotalCardSaleCaption.TabIndex = 10;
            this.lblTotalCardSaleCaption.Text = "Total Card Sales:";
            // 
            // lblTotalUpiSale
            // 
            this.lblTotalUpiSale.Location = new System.Drawing.Point(490, 110);
            this.lblTotalUpiSale.Name = "lblTotalUpiSale";
            this.lblTotalUpiSale.Size = new System.Drawing.Size(180, 30);
            this.lblTotalUpiSale.TabIndex = 13;
            this.lblTotalUpiSale.Text = "₹ 0.00";
            // 
            // lblTotalUpiSaleCaption
            // 
            this.lblTotalUpiSaleCaption.Location = new System.Drawing.Point(490, 85);
            this.lblTotalUpiSaleCaption.Name = "lblTotalUpiSaleCaption";
            this.lblTotalUpiSaleCaption.Size = new System.Drawing.Size(180, 25);
            this.lblTotalUpiSaleCaption.TabIndex = 12;
            this.lblTotalUpiSaleCaption.Text = "Total UPI Sales:";
            // 
            // lblTotalCreditSale
            // 
            this.lblTotalCreditSale.Location = new System.Drawing.Point(715, 110);
            this.lblTotalCreditSale.Name = "lblTotalCreditSale";
            this.lblTotalCreditSale.Size = new System.Drawing.Size(180, 30);
            this.lblTotalCreditSale.TabIndex = 15;
            this.lblTotalCreditSale.Text = "₹ 0.00";
            // 
            // lblTotalCreditSaleCaption
            // 
            this.lblTotalCreditSaleCaption.Location = new System.Drawing.Point(715, 85);
            this.lblTotalCreditSaleCaption.Name = "lblTotalCreditSaleCaption";
            this.lblTotalCreditSaleCaption.Size = new System.Drawing.Size(180, 25);
            this.lblTotalCreditSaleCaption.TabIndex = 14;
            this.lblTotalCreditSaleCaption.Text = "Total Credit Sales:";
            // 
            // lblTotalCustReceipt
            // 
            this.lblTotalCustReceipt.Location = new System.Drawing.Point(960, 110);
            this.lblTotalCustReceipt.Name = "lblTotalCustReceipt";
            this.lblTotalCustReceipt.Size = new System.Drawing.Size(180, 30);
            this.lblTotalCustReceipt.TabIndex = 17;
            this.lblTotalCustReceipt.Text = "₹ 0.00";
            // 
            // lblTotalCustReceiptCaption
            // 
            this.lblTotalCustReceiptCaption.Location = new System.Drawing.Point(960, 85);
            this.lblTotalCustReceiptCaption.Name = "lblTotalCustReceiptCaption";
            this.lblTotalCustReceiptCaption.Size = new System.Drawing.Size(180, 25);
            this.lblTotalCustReceiptCaption.TabIndex = 16;
            this.lblTotalCustReceiptCaption.Text = "Total Cust Receipts:";
            // 
            // lblVariance
            // 
            this.lblVariance.Location = new System.Drawing.Point(715, 45);
            this.lblVariance.Name = "lblVariance";
            this.lblVariance.Size = new System.Drawing.Size(210, 30);
            this.lblVariance.TabIndex = 7;
            this.lblVariance.Text = "₹ 0.00";
            // 
            // lblVarianceCaption
            // 
            this.lblVarianceCaption.Location = new System.Drawing.Point(715, 20);
            this.lblVarianceCaption.Name = "lblVarianceCaption";
            this.lblVarianceCaption.Size = new System.Drawing.Size(210, 25);
            this.lblVarianceCaption.TabIndex = 6;
            this.lblVarianceCaption.Text = "Net Cash Variance:";
            // 
            // lblCounted
            // 
            this.lblCounted.Location = new System.Drawing.Point(490, 45);
            this.lblCounted.Name = "lblCounted";
            this.lblCounted.Size = new System.Drawing.Size(180, 30);
            this.lblCounted.TabIndex = 5;
            this.lblCounted.Text = "₹ 0.00";
            // 
            // lblCountedCaption
            // 
            this.lblCountedCaption.Location = new System.Drawing.Point(490, 20);
            this.lblCountedCaption.Name = "lblCountedCaption";
            this.lblCountedCaption.Size = new System.Drawing.Size(180, 25);
            this.lblCountedCaption.TabIndex = 4;
            this.lblCountedCaption.Text = "Physical Cash Counted:";
            // 
            // lblExpected
            // 
            this.lblExpected.Location = new System.Drawing.Point(235, 45);
            this.lblExpected.Name = "lblExpected";
            this.lblExpected.Size = new System.Drawing.Size(210, 30);
            this.lblExpected.TabIndex = 3;
            this.lblExpected.Text = "₹ 0.00";
            // 
            // lblExpectedCaption
            // 
            this.lblExpectedCaption.Location = new System.Drawing.Point(235, 20);
            this.lblExpectedCaption.Name = "lblExpectedCaption";
            this.lblExpectedCaption.Size = new System.Drawing.Size(210, 25);
            this.lblExpectedCaption.TabIndex = 2;
            this.lblExpectedCaption.Text = "System Expected Cash:";
            // 
            // lblSales
            // 
            this.lblSales.Location = new System.Drawing.Point(20, 45);
            this.lblSales.Name = "lblSales";
            this.lblSales.Size = new System.Drawing.Size(150, 30);
            this.lblSales.TabIndex = 1;
            this.lblSales.Text = "₹ 0.00";
            // 
            // lblSalesCaption
            // 
            this.lblSalesCaption.Location = new System.Drawing.Point(20, 20);
            this.lblSalesCaption.Name = "lblSalesCaption";
            this.lblSalesCaption.Size = new System.Drawing.Size(150, 25);
            this.lblSalesCaption.TabIndex = 0;
            this.lblSalesCaption.Text = "Total Net Sales:";
            // 
            // FrmShiftReconciliationReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(251)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(1349, 561);
            this.Controls.Add(this.ultraPanelSummary);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1024, 600);
            this.Name = "FrmShiftReconciliationReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Shift Reconciliation & Z-Report Audit";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtCounterFilter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPreset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.ultraPanelSummary.ClientArea.ResumeLayout(false);
            this.ultraPanelSummary.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboPreset;
        private Infragistics.Win.Misc.UltraLabel lblPreset;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtCounterFilter;
        private Infragistics.Win.Misc.UltraLabel lblCounterFilter;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;
        private Infragistics.Win.Misc.UltraPanel ultraPanelSummary;
        private Infragistics.Win.Misc.UltraLabel lblVariance;
        private Infragistics.Win.Misc.UltraLabel lblVarianceCaption;
        private Infragistics.Win.Misc.UltraLabel lblCounted;
        private Infragistics.Win.Misc.UltraLabel lblCountedCaption;
        private Infragistics.Win.Misc.UltraLabel lblExpected;
        private Infragistics.Win.Misc.UltraLabel lblExpectedCaption;
        private Infragistics.Win.Misc.UltraLabel lblSales;
        private Infragistics.Win.Misc.UltraLabel lblSalesCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalCashSale;
        private Infragistics.Win.Misc.UltraLabel lblTotalCashSaleCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalCardSale;
        private Infragistics.Win.Misc.UltraLabel lblTotalCardSaleCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalUpiSale;
        private Infragistics.Win.Misc.UltraLabel lblTotalUpiSaleCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalCreditSale;
        private Infragistics.Win.Misc.UltraLabel lblTotalCreditSaleCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalCustReceipt;
        private Infragistics.Win.Misc.UltraLabel lblTotalCustReceiptCaption;
    }
}
