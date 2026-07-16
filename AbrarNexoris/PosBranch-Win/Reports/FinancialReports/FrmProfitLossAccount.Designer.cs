namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmProfitLossAccount
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
            this.ultraGroupBoxPL = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraGridProfitLoss = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelNetProfit = new Infragistics.Win.Misc.UltraPanel();
            this.lblNetProfitCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblNetProfitValue = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.lblIndirectExpensesValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblIndirectExpensesCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblIndirectIncomesValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblIndirectIncomesCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblGrossProfitBfValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblGrossProfitBfCaption = new Infragistics.Win.Misc.UltraLabel();
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
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxPL)).BeginInit();
            this.ultraGroupBoxPL.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridProfitLoss)).BeginInit();
            this.panelNetProfit.ClientArea.SuspendLayout();
            this.panelNetProfit.SuspendLayout();
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
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraGroupBoxPL);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraPanelSummary);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraGroupBoxFilters);
            this.ultraPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMain.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelMain.Name = "ultraPanelMain";
            this.ultraPanelMain.Size = new System.Drawing.Size(1100, 700);
            this.ultraPanelMain.TabIndex = 0;
            // 
            // ultraGroupBoxPL
            // 
            this.ultraGroupBoxPL.Controls.Add(this.ultraGridProfitLoss);
            this.ultraGroupBoxPL.Controls.Add(this.panelNetProfit);
            this.ultraGroupBoxPL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGroupBoxPL.Location = new System.Drawing.Point(0, 80);
            this.ultraGroupBoxPL.Name = "ultraGroupBoxPL";
            this.ultraGroupBoxPL.Size = new System.Drawing.Size(1100, 550);
            this.ultraGroupBoxPL.TabIndex = 1;
            this.ultraGroupBoxPL.Text = "Profit & Loss Details";
            // 
            // ultraGridProfitLoss
            // 
            this.ultraGridProfitLoss.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridProfitLoss.Location = new System.Drawing.Point(3, 19);
            this.ultraGridProfitLoss.Name = "ultraGridProfitLoss";
            this.ultraGridProfitLoss.Size = new System.Drawing.Size(1094, 478);
            this.ultraGridProfitLoss.TabIndex = 0;
            // 
            // panelNetProfit
            // 
            // 
            // panelNetProfit.ClientArea
            // 
            this.panelNetProfit.ClientArea.Controls.Add(this.lblNetProfitCaption);
            this.panelNetProfit.ClientArea.Controls.Add(this.lblNetProfitValue);
            this.panelNetProfit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelNetProfit.Location = new System.Drawing.Point(3, 497);
            this.panelNetProfit.Name = "panelNetProfit";
            this.panelNetProfit.Size = new System.Drawing.Size(1094, 50);
            this.panelNetProfit.TabIndex = 1;
            // 
            // lblNetProfitCaption
            // 
            this.lblNetProfitCaption.Location = new System.Drawing.Point(20, 15);
            this.lblNetProfitCaption.Name = "lblNetProfitCaption";
            this.lblNetProfitCaption.Size = new System.Drawing.Size(200, 22);
            this.lblNetProfitCaption.TabIndex = 0;
            this.lblNetProfitCaption.Text = "★ NET PROFIT:";
            // 
            // lblNetProfitValue
            // 
            this.lblNetProfitValue.Location = new System.Drawing.Point(220, 12);
            this.lblNetProfitValue.Name = "lblNetProfitValue";
            this.lblNetProfitValue.Size = new System.Drawing.Size(250, 28);
            this.lblNetProfitValue.TabIndex = 1;
            this.lblNetProfitValue.Text = "₹ 0.00";
            // 
            // ultraPanelSummary
            // 
            // 
            // ultraPanelSummary.ClientArea
            // 
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblIndirectExpensesValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblIndirectExpensesCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblIndirectIncomesValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblIndirectIncomesCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblGrossProfitBfValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblGrossProfitBfCaption);
            this.ultraPanelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelSummary.Location = new System.Drawing.Point(0, 630);
            this.ultraPanelSummary.Name = "ultraPanelSummary";
            this.ultraPanelSummary.Size = new System.Drawing.Size(1100, 70);
            this.ultraPanelSummary.TabIndex = 2;
            // 
            // lblIndirectExpensesValue
            // 
            this.lblIndirectExpensesValue.Location = new System.Drawing.Point(650, 31);
            this.lblIndirectExpensesValue.Name = "lblIndirectExpensesValue";
            this.lblIndirectExpensesValue.Size = new System.Drawing.Size(180, 28);
            this.lblIndirectExpensesValue.TabIndex = 5;
            this.lblIndirectExpensesValue.Text = "₹ 0.00";
            // 
            // lblIndirectExpensesCaption
            // 
            this.lblIndirectExpensesCaption.Location = new System.Drawing.Point(650, 10);
            this.lblIndirectExpensesCaption.Name = "lblIndirectExpensesCaption";
            this.lblIndirectExpensesCaption.Size = new System.Drawing.Size(140, 18);
            this.lblIndirectExpensesCaption.TabIndex = 4;
            this.lblIndirectExpensesCaption.Text = "Indirect Expenses:";
            // 
            // lblIndirectIncomesValue
            // 
            this.lblIndirectIncomesValue.Location = new System.Drawing.Point(380, 31);
            this.lblIndirectIncomesValue.Name = "lblIndirectIncomesValue";
            this.lblIndirectIncomesValue.Size = new System.Drawing.Size(180, 28);
            this.lblIndirectIncomesValue.TabIndex = 3;
            this.lblIndirectIncomesValue.Text = "₹ 0.00";
            // 
            // lblIndirectIncomesCaption
            // 
            this.lblIndirectIncomesCaption.Location = new System.Drawing.Point(380, 10);
            this.lblIndirectIncomesCaption.Name = "lblIndirectIncomesCaption";
            this.lblIndirectIncomesCaption.Size = new System.Drawing.Size(140, 18);
            this.lblIndirectIncomesCaption.TabIndex = 2;
            this.lblIndirectIncomesCaption.Text = "Indirect Incomes:";
            // 
            // lblGrossProfitBfValue
            // 
            this.lblGrossProfitBfValue.Location = new System.Drawing.Point(100, 31);
            this.lblGrossProfitBfValue.Name = "lblGrossProfitBfValue";
            this.lblGrossProfitBfValue.Size = new System.Drawing.Size(180, 28);
            this.lblGrossProfitBfValue.TabIndex = 1;
            this.lblGrossProfitBfValue.Text = "₹ 0.00";
            // 
            // lblGrossProfitBfCaption
            // 
            this.lblGrossProfitBfCaption.Location = new System.Drawing.Point(100, 10);
            this.lblGrossProfitBfCaption.Name = "lblGrossProfitBfCaption";
            this.lblGrossProfitBfCaption.Size = new System.Drawing.Size(200, 18);
            this.lblGrossProfitBfCaption.TabIndex = 0;
            this.lblGrossProfitBfCaption.Text = "Gross Profit (B/F):";
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
            // FrmProfitLossAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.ultraPanelMain);
            this.Name = "FrmProfitLossAccount";
            this.Text = "Profit & Loss Account";
            this.Load += new System.EventHandler(this.FrmProfitLossAccount_Load);
            this.ultraPanelMain.ClientArea.ResumeLayout(false);
            this.ultraPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxPL)).EndInit();
            this.ultraGroupBoxPL.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridProfitLoss)).EndInit();
            this.panelNetProfit.ClientArea.ResumeLayout(false);
            this.panelNetProfit.ResumeLayout(false);
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
        private Infragistics.Win.Misc.UltraLabel lblGrossProfitBfCaption;
        private Infragistics.Win.Misc.UltraLabel lblGrossProfitBfValue;
        private Infragistics.Win.Misc.UltraLabel lblIndirectIncomesCaption;
        private Infragistics.Win.Misc.UltraLabel lblIndirectIncomesValue;
        private Infragistics.Win.Misc.UltraLabel lblIndirectExpensesCaption;
        private Infragistics.Win.Misc.UltraLabel lblIndirectExpensesValue;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxPL;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridProfitLoss;
        private Infragistics.Win.Misc.UltraPanel panelNetProfit;
        private Infragistics.Win.Misc.UltraLabel lblNetProfitCaption;
        private Infragistics.Win.Misc.UltraLabel lblNetProfitValue;
    }
}
