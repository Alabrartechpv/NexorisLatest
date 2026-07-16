namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmTradingPLAccount
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            this.ultraPanelMain = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGroupBoxTrading = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraGridTrading = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelGrossProfit = new Infragistics.Win.Misc.UltraPanel();
            this.lblGrossProfitCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblGrossProfitValue = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.lblClosingStockValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblClosingStockCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblOpeningStockValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblOpeningStockCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalPurchasesValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalPurchasesCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalSalesValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalSalesCaption = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxFilters = new Infragistics.Win.Misc.UltraGroupBox();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnGenerate = new Infragistics.Win.Misc.UltraButton();
            this.ultraDateTimeTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new System.Windows.Forms.Label();
            this.ultraDateTimeFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblFromDate = new System.Windows.Forms.Label();
            this.ultraPanelMain.ClientArea.SuspendLayout();
            this.ultraPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxTrading)).BeginInit();
            this.ultraGroupBoxTrading.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTrading)).BeginInit();
            this.panelGrossProfit.ClientArea.SuspendLayout();
            this.panelGrossProfit.SuspendLayout();
            this.ultraPanelSummary.ClientArea.SuspendLayout();
            this.ultraPanelSummary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFilters)).BeginInit();
            this.ultraGroupBoxFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraPanelMain
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(245)))));
            this.ultraPanelMain.Appearance = appearance1;
            // 
            // ultraPanelMain.ClientArea
            // 
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraGroupBoxTrading);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraPanelSummary);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraGroupBoxFilters);
            this.ultraPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMain.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelMain.Name = "ultraPanelMain";
            this.ultraPanelMain.Size = new System.Drawing.Size(1100, 700);
            this.ultraPanelMain.TabIndex = 0;
            // 
            // ultraGroupBoxTrading
            // 
            this.ultraGroupBoxTrading.Controls.Add(this.ultraGridTrading);
            this.ultraGroupBoxTrading.Controls.Add(this.panelGrossProfit);
            this.ultraGroupBoxTrading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGroupBoxTrading.Location = new System.Drawing.Point(0, 80);
            this.ultraGroupBoxTrading.Name = "ultraGroupBoxTrading";
            this.ultraGroupBoxTrading.Size = new System.Drawing.Size(1100, 550);
            this.ultraGroupBoxTrading.TabIndex = 1;
            this.ultraGroupBoxTrading.Text = "Trading Account Details";
            // 
            // ultraGridTrading
            // 
            this.ultraGridTrading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridTrading.Location = new System.Drawing.Point(3, 19);
            this.ultraGridTrading.Name = "ultraGridTrading";
            this.ultraGridTrading.Size = new System.Drawing.Size(1094, 483);
            this.ultraGridTrading.TabIndex = 0;
            // 
            // panelGrossProfit
            // 
            // 
            // panelGrossProfit.ClientArea
            // 
            this.panelGrossProfit.ClientArea.Controls.Add(this.lblGrossProfitCaption);
            this.panelGrossProfit.ClientArea.Controls.Add(this.lblGrossProfitValue);
            this.panelGrossProfit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelGrossProfit.Location = new System.Drawing.Point(3, 502);
            this.panelGrossProfit.Name = "panelGrossProfit";
            this.panelGrossProfit.Size = new System.Drawing.Size(1094, 45);
            this.panelGrossProfit.TabIndex = 1;
            // 
            // lblGrossProfitCaption
            // 
            this.lblGrossProfitCaption.Location = new System.Drawing.Point(20, 12);
            this.lblGrossProfitCaption.Name = "lblGrossProfitCaption";
            this.lblGrossProfitCaption.Size = new System.Drawing.Size(200, 22);
            this.lblGrossProfitCaption.TabIndex = 0;
            this.lblGrossProfitCaption.Text = "GROSS PROFIT:";
            // 
            // lblGrossProfitValue
            // 
            this.lblGrossProfitValue.Location = new System.Drawing.Point(220, 9);
            this.lblGrossProfitValue.Name = "lblGrossProfitValue";
            this.lblGrossProfitValue.Size = new System.Drawing.Size(250, 28);
            this.lblGrossProfitValue.TabIndex = 1;
            this.lblGrossProfitValue.Text = "₹ 0.00";
            // 
            // ultraPanelSummary
            // 
            // 
            // ultraPanelSummary.ClientArea
            // 
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblClosingStockValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblClosingStockCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblOpeningStockValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblOpeningStockCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalPurchasesValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalPurchasesCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalSalesValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalSalesCaption);
            this.ultraPanelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelSummary.Location = new System.Drawing.Point(0, 630);
            this.ultraPanelSummary.Name = "ultraPanelSummary";
            this.ultraPanelSummary.Size = new System.Drawing.Size(1100, 70);
            this.ultraPanelSummary.TabIndex = 2;
            // 
            // lblClosingStockValue
            // 
            this.lblClosingStockValue.Location = new System.Drawing.Point(820, 31);
            this.lblClosingStockValue.Name = "lblClosingStockValue";
            this.lblClosingStockValue.Size = new System.Drawing.Size(180, 28);
            this.lblClosingStockValue.TabIndex = 7;
            this.lblClosingStockValue.Text = "₹ 0.00";
            // 
            // lblClosingStockCaption
            // 
            this.lblClosingStockCaption.Location = new System.Drawing.Point(820, 10);
            this.lblClosingStockCaption.Name = "lblClosingStockCaption";
            this.lblClosingStockCaption.Size = new System.Drawing.Size(140, 18);
            this.lblClosingStockCaption.TabIndex = 6;
            this.lblClosingStockCaption.Text = "Closing Stock:";
            // 
            // lblOpeningStockValue
            // 
            this.lblOpeningStockValue.Location = new System.Drawing.Point(50, 31);
            this.lblOpeningStockValue.Name = "lblOpeningStockValue";
            this.lblOpeningStockValue.Size = new System.Drawing.Size(180, 28);
            this.lblOpeningStockValue.TabIndex = 5;
            this.lblOpeningStockValue.Text = "₹ 0.00";
            // 
            // lblOpeningStockCaption
            // 
            this.lblOpeningStockCaption.Location = new System.Drawing.Point(50, 10);
            this.lblOpeningStockCaption.Name = "lblOpeningStockCaption";
            this.lblOpeningStockCaption.Size = new System.Drawing.Size(140, 18);
            this.lblOpeningStockCaption.TabIndex = 4;
            this.lblOpeningStockCaption.Text = "Opening Stock:";
            // 
            // lblTotalPurchasesValue
            // 
            this.lblTotalPurchasesValue.Location = new System.Drawing.Point(300, 31);
            this.lblTotalPurchasesValue.Name = "lblTotalPurchasesValue";
            this.lblTotalPurchasesValue.Size = new System.Drawing.Size(180, 28);
            this.lblTotalPurchasesValue.TabIndex = 3;
            this.lblTotalPurchasesValue.Text = "₹ 0.00";
            // 
            // lblTotalPurchasesCaption
            // 
            this.lblTotalPurchasesCaption.Location = new System.Drawing.Point(300, 10);
            this.lblTotalPurchasesCaption.Name = "lblTotalPurchasesCaption";
            this.lblTotalPurchasesCaption.Size = new System.Drawing.Size(140, 18);
            this.lblTotalPurchasesCaption.TabIndex = 2;
            this.lblTotalPurchasesCaption.Text = "Total Purchases:";
            // 
            // lblTotalSalesValue
            // 
            this.lblTotalSalesValue.Location = new System.Drawing.Point(560, 31);
            this.lblTotalSalesValue.Name = "lblTotalSalesValue";
            this.lblTotalSalesValue.Size = new System.Drawing.Size(180, 28);
            this.lblTotalSalesValue.TabIndex = 1;
            this.lblTotalSalesValue.Text = "₹ 0.00";
            // 
            // lblTotalSalesCaption
            // 
            this.lblTotalSalesCaption.Location = new System.Drawing.Point(560, 10);
            this.lblTotalSalesCaption.Name = "lblTotalSalesCaption";
            this.lblTotalSalesCaption.Size = new System.Drawing.Size(120, 18);
            this.lblTotalSalesCaption.TabIndex = 0;
            this.lblTotalSalesCaption.Text = "Total Sales:";
            // 
            // ultraGroupBoxFilters
            // 
            this.ultraGroupBoxFilters.Controls.Add(this.btnClose);
            this.ultraGroupBoxFilters.Controls.Add(this.btnPrint);
            this.ultraGroupBoxFilters.Controls.Add(this.btnExport);
            this.ultraGroupBoxFilters.Controls.Add(this.btnGenerate);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraDateTimeTo);
            this.ultraGroupBoxFilters.Controls.Add(this.lblToDate);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraDateTimeFrom);
            this.ultraGroupBoxFilters.Controls.Add(this.lblFromDate);
            this.ultraGroupBoxFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraGroupBoxFilters.Location = new System.Drawing.Point(0, 0);
            this.ultraGroupBoxFilters.Name = "ultraGroupBoxFilters";
            this.ultraGroupBoxFilters.Size = new System.Drawing.Size(1100, 80);
            this.ultraGroupBoxFilters.TabIndex = 0;
            this.ultraGroupBoxFilters.Text = "Search Criteria";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(850, 33);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(85, 28);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(750, 33);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(85, 28);
            this.btnPrint.TabIndex = 6;
            this.btnPrint.Text = "Print";
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(650, 33);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(85, 28);
            this.btnExport.TabIndex = 5;
            this.btnExport.Text = "Export";
            this.btnExport.Click += new System.EventHandler(this.btnExportCsv_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(530, 33);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(100, 28);
            this.btnGenerate.TabIndex = 4;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // ultraDateTimeTo
            // 
            this.ultraDateTimeTo.Location = new System.Drawing.Point(350, 35);
            this.ultraDateTimeTo.Name = "ultraDateTimeTo";
            this.ultraDateTimeTo.Size = new System.Drawing.Size(140, 21);
            this.ultraDateTimeTo.TabIndex = 3;
            // 
            // lblToDate
            // 
            this.lblToDate.AutoSize = true;
            this.lblToDate.Location = new System.Drawing.Point(280, 39);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(53, 17);
            this.lblToDate.TabIndex = 2;
            this.lblToDate.Text = "To Date";
            // 
            // ultraDateTimeFrom
            // 
            this.ultraDateTimeFrom.Location = new System.Drawing.Point(110, 35);
            this.ultraDateTimeFrom.Name = "ultraDateTimeFrom";
            this.ultraDateTimeFrom.Size = new System.Drawing.Size(140, 21);
            this.ultraDateTimeFrom.TabIndex = 1;
            // 
            // lblFromDate
            // 
            this.lblFromDate.AutoSize = true;
            this.lblFromDate.Location = new System.Drawing.Point(30, 39);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(69, 17);
            this.lblFromDate.TabIndex = 0;
            this.lblFromDate.Text = "From Date";
            // 
            // FrmTradingPLAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.ultraPanelMain);
            this.Name = "FrmTradingPLAccount";
            this.Text = "Trading Account";
            this.Load += new System.EventHandler(this.FrmTradingPLAccount_Load);
            this.ultraPanelMain.ClientArea.ResumeLayout(false);
            this.ultraPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxTrading)).EndInit();
            this.ultraGroupBoxTrading.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTrading)).EndInit();
            this.panelGrossProfit.ClientArea.ResumeLayout(false);
            this.panelGrossProfit.ResumeLayout(false);
            this.ultraPanelSummary.ClientArea.ResumeLayout(false);
            this.ultraPanelSummary.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFilters)).EndInit();
            this.ultraGroupBoxFilters.ResumeLayout(false);
            this.ultraGroupBoxFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelMain;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxFilters;
        private System.Windows.Forms.Label lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeFrom;
        private System.Windows.Forms.Label lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeTo;
        private Infragistics.Win.Misc.UltraButton btnGenerate;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private Infragistics.Win.Misc.UltraPanel ultraPanelSummary;
        private Infragistics.Win.Misc.UltraLabel lblTotalSalesCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalSalesValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalPurchasesCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalPurchasesValue;
        private Infragistics.Win.Misc.UltraLabel lblOpeningStockCaption;
        private Infragistics.Win.Misc.UltraLabel lblOpeningStockValue;
        private Infragistics.Win.Misc.UltraLabel lblClosingStockCaption;
        private Infragistics.Win.Misc.UltraLabel lblClosingStockValue;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxTrading;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridTrading;
        private Infragistics.Win.Misc.UltraPanel panelGrossProfit;
        private Infragistics.Win.Misc.UltraLabel lblGrossProfitCaption;
        private Infragistics.Win.Misc.UltraLabel lblGrossProfitValue;
    }
}
