namespace PosBranch_Win.Reports.FinancialReports
{
    partial class frmCustomerOutstandingReport
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
            this.chkPositiveBalance = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnSearch = new Infragistics.Win.Misc.UltraButton();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboPreset = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblPreset = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboCustomer = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCustomer = new Infragistics.Win.Misc.UltraLabel();
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
            this.lblBalance = new Infragistics.Win.Misc.UltraLabel();
            this.lblBalanceCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblReceived = new Infragistics.Win.Misc.UltraLabel();
            this.lblReceivedCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblOutstanding = new Infragistics.Win.Misc.UltraLabel();
            this.lblOutstandingCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblCustomerCount = new Infragistics.Win.Misc.UltraLabel();
            this.lblCustomerCountCaption = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkPositiveBalance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPreset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboCustomer)).BeginInit();
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
            this.ultraPanelControls.ClientArea.Controls.Add(this.chkPositiveBalance);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnClearFilters);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboCustomer);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblCustomer);
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
            // chkPositiveBalance
            // 
            this.chkPositiveBalance.Location = new System.Drawing.Point(625, 52);
            this.chkPositiveBalance.Name = "chkPositiveBalance";
            this.chkPositiveBalance.Size = new System.Drawing.Size(150, 20);
            this.chkPositiveBalance.TabIndex = 10;
            this.chkPositiveBalance.Text = "Only positive balance";
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
            this.lblSearch.Text = "Search:";
            // 
            // ultraComboPreset
            // 
            this.ultraComboPreset.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboPreset.Location = new System.Drawing.Point(499, 49);
            this.ultraComboPreset.Name = "ultraComboPreset";
            this.ultraComboPreset.Size = new System.Drawing.Size(112, 25);
            this.ultraComboPreset.TabIndex = 9;
            // 
            // lblPreset
            // 
            this.lblPreset.Location = new System.Drawing.Point(355, 52);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(140, 23);
            this.lblPreset.TabIndex = 8;
            this.lblPreset.Text = "Quick date:";
            // 
            // ultraComboCustomer
            // 
            this.ultraComboCustomer.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboCustomer.Location = new System.Drawing.Point(625, 16);
            this.ultraComboCustomer.Name = "ultraComboCustomer";
            this.ultraComboCustomer.Size = new System.Drawing.Size(260, 25);
            this.ultraComboCustomer.TabIndex = 5;
            // 
            // lblCustomer
            // 
            this.lblCustomer.Location = new System.Drawing.Point(558, 19);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Size = new System.Drawing.Size(65, 23);
            this.lblCustomer.TabIndex = 4;
            this.lblCustomer.Text = "Customer:";
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(904, 13);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(105, 30);
            this.btnPrint.TabIndex = 16;
            this.btnPrint.Text = "Print Bill";
            this.btnPrint.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(1019, 13);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(105, 30);
            this.btnExport.TabIndex = 14;
            this.btnExport.Text = "Export";
            this.btnExport.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(1134, 13);
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
            this.dtTo.Size = new System.Drawing.Size(110, 25);
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
            this.dtFrom.Size = new System.Drawing.Size(110, 25);
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
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblBalance);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblBalanceCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblReceived);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblReceivedCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblOutstanding);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblOutstandingCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblCustomerCount);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblCustomerCountCaption);
            this.ultraPanelSummary.Location = new System.Drawing.Point(0, 399);
            this.ultraPanelSummary.Name = "ultraPanelSummary";
            this.ultraPanelSummary.Size = new System.Drawing.Size(1349, 160);
            this.ultraPanelSummary.TabIndex = 2;
            // 
            // lblBalance
            // 
            this.lblBalance.Location = new System.Drawing.Point(715, 45);
            this.lblBalance.Name = "lblBalance";
            this.lblBalance.Size = new System.Drawing.Size(210, 30);
            this.lblBalance.TabIndex = 7;
            this.lblBalance.Text = "Rs. 0.00";
            // 
            // lblBalanceCaption
            // 
            this.lblBalanceCaption.Location = new System.Drawing.Point(715, 20);
            this.lblBalanceCaption.Name = "lblBalanceCaption";
            this.lblBalanceCaption.Size = new System.Drawing.Size(210, 25);
            this.lblBalanceCaption.TabIndex = 6;
            this.lblBalanceCaption.Text = "Net Outstanding:";
            // 
            // lblReceived
            // 
            this.lblReceived.Location = new System.Drawing.Point(490, 45);
            this.lblReceived.Name = "lblReceived";
            this.lblReceived.Size = new System.Drawing.Size(180, 30);
            this.lblReceived.TabIndex = 5;
            this.lblReceived.Text = "Rs. 0.00";
            // 
            // lblReceivedCaption
            // 
            this.lblReceivedCaption.Location = new System.Drawing.Point(490, 20);
            this.lblReceivedCaption.Name = "lblReceivedCaption";
            this.lblReceivedCaption.Size = new System.Drawing.Size(180, 25);
            this.lblReceivedCaption.TabIndex = 4;
            this.lblReceivedCaption.Text = "Total Received:";
            // 
            // lblOutstanding
            // 
            this.lblOutstanding.Location = new System.Drawing.Point(235, 45);
            this.lblOutstanding.Name = "lblOutstanding";
            this.lblOutstanding.Size = new System.Drawing.Size(210, 30);
            this.lblOutstanding.TabIndex = 3;
            this.lblOutstanding.Text = "Rs. 0.00";
            // 
            // lblOutstandingCaption
            // 
            this.lblOutstandingCaption.Location = new System.Drawing.Point(235, 20);
            this.lblOutstandingCaption.Name = "lblOutstandingCaption";
            this.lblOutstandingCaption.Size = new System.Drawing.Size(210, 25);
            this.lblOutstandingCaption.TabIndex = 2;
            this.lblOutstandingCaption.Text = "Total Invoice Amount:";
            // 
            // lblCustomerCount
            // 
            this.lblCustomerCount.Location = new System.Drawing.Point(20, 45);
            this.lblCustomerCount.Name = "lblCustomerCount";
            this.lblCustomerCount.Size = new System.Drawing.Size(150, 30);
            this.lblCustomerCount.TabIndex = 1;
            this.lblCustomerCount.Text = "0";
            // 
            // lblCustomerCountCaption
            // 
            this.lblCustomerCountCaption.Location = new System.Drawing.Point(20, 20);
            this.lblCustomerCountCaption.Name = "lblCustomerCountCaption";
            this.lblCustomerCountCaption.Size = new System.Drawing.Size(150, 25);
            this.lblCustomerCountCaption.TabIndex = 0;
            this.lblCustomerCountCaption.Text = "Bills:";
            // 
            // frmCustomerOutstandingReport
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
            this.Name = "frmCustomerOutstandingReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Customer Outstanding Report";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chkPositiveBalance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPreset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboCustomer)).EndInit();
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
        private Infragistics.Win.UltraWinEditors.UltraCheckEditor chkPositiveBalance;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboPreset;
        private Infragistics.Win.Misc.UltraLabel lblPreset;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboCustomer;
        private Infragistics.Win.Misc.UltraLabel lblCustomer;
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
        private Infragistics.Win.Misc.UltraLabel lblBalance;
        private Infragistics.Win.Misc.UltraLabel lblBalanceCaption;
        private Infragistics.Win.Misc.UltraLabel lblReceived;
        private Infragistics.Win.Misc.UltraLabel lblReceivedCaption;
        private Infragistics.Win.Misc.UltraLabel lblOutstanding;
        private Infragistics.Win.Misc.UltraLabel lblOutstandingCaption;
        private Infragistics.Win.Misc.UltraLabel lblCustomerCount;
        private Infragistics.Win.Misc.UltraLabel lblCustomerCountCaption;
    }
}
