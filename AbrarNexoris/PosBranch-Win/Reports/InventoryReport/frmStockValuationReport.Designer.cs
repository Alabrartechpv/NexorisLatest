namespace PosBranch_Win.Reports.InventoryReport
{
    partial class frmStockValuationReport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance6 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance7 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance8 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance9 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance10 = new Infragistics.Win.Appearance();
            this.panelFilters = new Infragistics.Win.Misc.UltraPanel();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblPeriod = new Infragistics.Win.Misc.UltraLabel();
            this.comboPeriod = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblGroup = new Infragistics.Win.Misc.UltraLabel();
            this.comboGroup = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCategory = new Infragistics.Win.Misc.UltraLabel();
            this.comboCategory = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblStockFilter = new Infragistics.Win.Misc.UltraLabel();
            this.comboStockFilter = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.btnSearch = new Infragistics.Win.Misc.UltraButton();
            this.btnReset = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.panelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.cardItems = new Infragistics.Win.Misc.UltraPanel();
            this.lblItemsCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblItemsValue = new Infragistics.Win.Misc.UltraLabel();
            this.cardQty = new Infragistics.Win.Misc.UltraPanel();
            this.lblQtyCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblQtyValue = new Infragistics.Win.Misc.UltraLabel();
            this.cardCostVal = new Infragistics.Win.Misc.UltraPanel();
            this.lblCostCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblCostValue = new Infragistics.Win.Misc.UltraLabel();
            this.cardRetailVal = new Infragistics.Win.Misc.UltraPanel();
            this.lblRetailCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblRetailValue = new Infragistics.Win.Misc.UltraLabel();
            this.cardProfit = new Infragistics.Win.Misc.UltraPanel();
            this.lblProfitCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblProfitValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblStatus = new Infragistics.Win.Misc.UltraLabel();
            this.panelFilters.ClientArea.SuspendLayout();
            this.panelFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboCategory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboStockFilter)).BeginInit();
            this.panelGrid.ClientArea.SuspendLayout();
            this.panelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.panelSummary.ClientArea.SuspendLayout();
            this.panelSummary.SuspendLayout();
            this.cardItems.ClientArea.SuspendLayout();
            this.cardItems.SuspendLayout();
            this.cardQty.ClientArea.SuspendLayout();
            this.cardQty.SuspendLayout();
            this.cardCostVal.ClientArea.SuspendLayout();
            this.cardCostVal.SuspendLayout();
            this.cardRetailVal.ClientArea.SuspendLayout();
            this.cardRetailVal.SuspendLayout();
            this.cardProfit.ClientArea.SuspendLayout();
            this.cardProfit.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelFilters
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            appearance1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.panelFilters.Appearance = appearance1;
            this.panelFilters.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // panelFilters.ClientArea
            // 
            this.panelFilters.ClientArea.Controls.Add(this.lblFromDate);
            this.panelFilters.ClientArea.Controls.Add(this.dtFrom);
            this.panelFilters.ClientArea.Controls.Add(this.lblToDate);
            this.panelFilters.ClientArea.Controls.Add(this.dtTo);
            this.panelFilters.ClientArea.Controls.Add(this.lblPeriod);
            this.panelFilters.ClientArea.Controls.Add(this.comboPeriod);
            this.panelFilters.ClientArea.Controls.Add(this.lblSearch);
            this.panelFilters.ClientArea.Controls.Add(this.txtSearch);
            this.panelFilters.ClientArea.Controls.Add(this.lblGroup);
            this.panelFilters.ClientArea.Controls.Add(this.comboGroup);
            this.panelFilters.ClientArea.Controls.Add(this.lblCategory);
            this.panelFilters.ClientArea.Controls.Add(this.comboCategory);
            this.panelFilters.ClientArea.Controls.Add(this.lblStockFilter);
            this.panelFilters.ClientArea.Controls.Add(this.comboStockFilter);
            this.panelFilters.ClientArea.Controls.Add(this.btnSearch);
            this.panelFilters.ClientArea.Controls.Add(this.btnReset);
            this.panelFilters.ClientArea.Controls.Add(this.btnExport);
            this.panelFilters.ClientArea.Controls.Add(this.btnPrint);
            this.panelFilters.ClientArea.Controls.Add(this.btnClose);
            this.panelFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilters.Location = new System.Drawing.Point(0, 0);
            this.panelFilters.Name = "panelFilters";
            this.panelFilters.Size = new System.Drawing.Size(1280, 100);
            this.panelFilters.TabIndex = 1;
            this.panelFilters.UseAppStyling = false;
            this.panelFilters.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // lblFromDate
            // 
            appearance2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblFromDate.Appearance = appearance2;
            this.lblFromDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFromDate.Location = new System.Drawing.Point(15, 20);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(68, 23);
            this.lblFromDate.TabIndex = 0;
            this.lblFromDate.Text = "From Date:";
            // 
            // dtFrom
            // 
            this.dtFrom.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtFrom.Location = new System.Drawing.Point(88, 16);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(115, 25);
            this.dtFrom.TabIndex = 1;
            // 
            // lblToDate
            // 
            appearance3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblToDate.Appearance = appearance3;
            this.lblToDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblToDate.Location = new System.Drawing.Point(215, 20);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(58, 23);
            this.lblToDate.TabIndex = 2;
            this.lblToDate.Text = "To Date:";
            // 
            // dtTo
            // 
            this.dtTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtTo.Location = new System.Drawing.Point(278, 16);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(115, 25);
            this.dtTo.TabIndex = 3;
            // 
            // lblPeriod
            // 
            appearance4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblPeriod.Appearance = appearance4;
            this.lblPeriod.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPeriod.Location = new System.Drawing.Point(406, 20);
            this.lblPeriod.Name = "lblPeriod";
            this.lblPeriod.Size = new System.Drawing.Size(48, 23);
            this.lblPeriod.TabIndex = 4;
            this.lblPeriod.Text = "Period:";
            // 
            // comboPeriod
            // 
            this.comboPeriod.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.comboPeriod.Location = new System.Drawing.Point(458, 16);
            this.comboPeriod.Name = "comboPeriod";
            this.comboPeriod.Size = new System.Drawing.Size(125, 25);
            this.comboPeriod.TabIndex = 5;
            // 
            // lblSearch
            // 
            appearance5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblSearch.Appearance = appearance5;
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSearch.Location = new System.Drawing.Point(597, 20);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(50, 23);
            this.lblSearch.TabIndex = 6;
            this.lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.Location = new System.Drawing.Point(650, 16);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(210, 25);
            this.txtSearch.TabIndex = 7;
            // 
            // lblGroup
            // 
            appearance6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblGroup.Appearance = appearance6;
            this.lblGroup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGroup.Location = new System.Drawing.Point(15, 60);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(68, 23);
            this.lblGroup.TabIndex = 8;
            this.lblGroup.Text = "Group:";
            // 
            // comboGroup
            // 
            this.comboGroup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.comboGroup.Location = new System.Drawing.Point(88, 56);
            this.comboGroup.Name = "comboGroup";
            this.comboGroup.Size = new System.Drawing.Size(115, 25);
            this.comboGroup.TabIndex = 9;
            // 
            // lblCategory
            // 
            appearance7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblCategory.Appearance = appearance7;
            this.lblCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCategory.Location = new System.Drawing.Point(215, 60);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(58, 23);
            this.lblCategory.TabIndex = 10;
            this.lblCategory.Text = "Category:";
            // 
            // comboCategory
            // 
            this.comboCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.comboCategory.Location = new System.Drawing.Point(278, 56);
            this.comboCategory.Name = "comboCategory";
            this.comboCategory.Size = new System.Drawing.Size(115, 25);
            this.comboCategory.TabIndex = 11;
            // 
            // lblStockFilter
            // 
            appearance8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblStockFilter.Appearance = appearance8;
            this.lblStockFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStockFilter.Location = new System.Drawing.Point(406, 60);
            this.lblStockFilter.Name = "lblStockFilter";
            this.lblStockFilter.Size = new System.Drawing.Size(48, 23);
            this.lblStockFilter.TabIndex = 12;
            this.lblStockFilter.Text = "Show:";
            // 
            // comboStockFilter
            // 
            this.comboStockFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.comboStockFilter.Location = new System.Drawing.Point(458, 56);
            this.comboStockFilter.Name = "comboStockFilter";
            this.comboStockFilter.Size = new System.Drawing.Size(125, 25);
            this.comboStockFilter.TabIndex = 13;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(630, 56);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(100, 28);
            this.btnSearch.TabIndex = 14;
            this.btnSearch.Text = "Search  [F5]";
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(738, 56);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(82, 28);
            this.btnReset.TabIndex = 15;
            this.btnReset.Text = "↺  Reset";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(828, 56);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(95, 28);
            this.btnExport.TabIndex = 16;
            this.btnExport.Text = "⬇  Export CSV";
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(931, 56);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(78, 28);
            this.btnPrint.TabIndex = 17;
            this.btnPrint.Text = "🖨  Print";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(1017, 56);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(78, 28);
            this.btnClose.TabIndex = 18;
            this.btnClose.Text = "✕  Close";
            // 
            // panelGrid
            // 
            // 
            // panelGrid.ClientArea
            // 
            this.panelGrid.ClientArea.Controls.Add(this.gridReport);
            this.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGrid.Location = new System.Drawing.Point(0, 100);
            this.panelGrid.Name = "panelGrid";
            this.panelGrid.Size = new System.Drawing.Size(1280, 522);
            this.panelGrid.TabIndex = 0;
            // 
            // gridReport
            // 
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Location = new System.Drawing.Point(0, 0);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1280, 522);
            this.gridReport.TabIndex = 0;
            // 
            // panelSummary
            // 
            appearance9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            appearance9.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.panelSummary.Appearance = appearance9;
            this.panelSummary.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // panelSummary.ClientArea
            // 
            this.panelSummary.ClientArea.Controls.Add(this.cardItems);
            this.panelSummary.ClientArea.Controls.Add(this.cardQty);
            this.panelSummary.ClientArea.Controls.Add(this.cardCostVal);
            this.panelSummary.ClientArea.Controls.Add(this.cardRetailVal);
            this.panelSummary.ClientArea.Controls.Add(this.cardProfit);
            this.panelSummary.ClientArea.Controls.Add(this.lblStatus);
            this.panelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelSummary.Location = new System.Drawing.Point(0, 622);
            this.panelSummary.Name = "panelSummary";
            this.panelSummary.Size = new System.Drawing.Size(1280, 98);
            this.panelSummary.TabIndex = 3;
            this.panelSummary.UseAppStyling = false;
            this.panelSummary.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // cardItems
            // 
            // 
            // cardItems.ClientArea
            // 
            this.cardItems.ClientArea.Controls.Add(this.lblItemsCaption);
            this.cardItems.ClientArea.Controls.Add(this.lblItemsValue);
            this.cardItems.Location = new System.Drawing.Point(0, 0);
            this.cardItems.Name = "cardItems";
            this.cardItems.Size = new System.Drawing.Size(238, 62);
            this.cardItems.TabIndex = 0;
            // 
            // lblItemsCaption
            // 
            this.lblItemsCaption.Location = new System.Drawing.Point(0, 0);
            this.lblItemsCaption.Name = "lblItemsCaption";
            this.lblItemsCaption.Size = new System.Drawing.Size(100, 23);
            this.lblItemsCaption.TabIndex = 0;
            // 
            // lblItemsValue
            // 
            this.lblItemsValue.Location = new System.Drawing.Point(0, 0);
            this.lblItemsValue.Name = "lblItemsValue";
            this.lblItemsValue.Size = new System.Drawing.Size(100, 23);
            this.lblItemsValue.TabIndex = 1;
            // 
            // cardQty
            // 
            // 
            // cardQty.ClientArea
            // 
            this.cardQty.ClientArea.Controls.Add(this.lblQtyCaption);
            this.cardQty.ClientArea.Controls.Add(this.lblQtyValue);
            this.cardQty.Location = new System.Drawing.Point(0, 0);
            this.cardQty.Name = "cardQty";
            this.cardQty.Size = new System.Drawing.Size(238, 62);
            this.cardQty.TabIndex = 1;
            // 
            // lblQtyCaption
            // 
            this.lblQtyCaption.Location = new System.Drawing.Point(0, 0);
            this.lblQtyCaption.Name = "lblQtyCaption";
            this.lblQtyCaption.Size = new System.Drawing.Size(100, 23);
            this.lblQtyCaption.TabIndex = 0;
            // 
            // lblQtyValue
            // 
            this.lblQtyValue.Location = new System.Drawing.Point(0, 0);
            this.lblQtyValue.Name = "lblQtyValue";
            this.lblQtyValue.Size = new System.Drawing.Size(100, 23);
            this.lblQtyValue.TabIndex = 1;
            // 
            // cardCostVal
            // 
            // 
            // cardCostVal.ClientArea
            // 
            this.cardCostVal.ClientArea.Controls.Add(this.lblCostCaption);
            this.cardCostVal.ClientArea.Controls.Add(this.lblCostValue);
            this.cardCostVal.Location = new System.Drawing.Point(0, 0);
            this.cardCostVal.Name = "cardCostVal";
            this.cardCostVal.Size = new System.Drawing.Size(238, 62);
            this.cardCostVal.TabIndex = 2;
            // 
            // lblCostCaption
            // 
            this.lblCostCaption.Location = new System.Drawing.Point(0, 0);
            this.lblCostCaption.Name = "lblCostCaption";
            this.lblCostCaption.Size = new System.Drawing.Size(100, 23);
            this.lblCostCaption.TabIndex = 0;
            // 
            // lblCostValue
            // 
            this.lblCostValue.Location = new System.Drawing.Point(0, 0);
            this.lblCostValue.Name = "lblCostValue";
            this.lblCostValue.Size = new System.Drawing.Size(100, 23);
            this.lblCostValue.TabIndex = 1;
            // 
            // cardRetailVal
            // 
            // 
            // cardRetailVal.ClientArea
            // 
            this.cardRetailVal.ClientArea.Controls.Add(this.lblRetailCaption);
            this.cardRetailVal.ClientArea.Controls.Add(this.lblRetailValue);
            this.cardRetailVal.Location = new System.Drawing.Point(0, 0);
            this.cardRetailVal.Name = "cardRetailVal";
            this.cardRetailVal.Size = new System.Drawing.Size(238, 62);
            this.cardRetailVal.TabIndex = 3;
            // 
            // lblRetailCaption
            // 
            this.lblRetailCaption.Location = new System.Drawing.Point(0, 0);
            this.lblRetailCaption.Name = "lblRetailCaption";
            this.lblRetailCaption.Size = new System.Drawing.Size(100, 23);
            this.lblRetailCaption.TabIndex = 0;
            // 
            // lblRetailValue
            // 
            this.lblRetailValue.Location = new System.Drawing.Point(0, 0);
            this.lblRetailValue.Name = "lblRetailValue";
            this.lblRetailValue.Size = new System.Drawing.Size(100, 23);
            this.lblRetailValue.TabIndex = 1;
            // 
            // cardProfit
            // 
            // 
            // cardProfit.ClientArea
            // 
            this.cardProfit.ClientArea.Controls.Add(this.lblProfitCaption);
            this.cardProfit.ClientArea.Controls.Add(this.lblProfitValue);
            this.cardProfit.Location = new System.Drawing.Point(0, 0);
            this.cardProfit.Name = "cardProfit";
            this.cardProfit.Size = new System.Drawing.Size(238, 62);
            this.cardProfit.TabIndex = 4;
            // 
            // lblProfitCaption
            // 
            this.lblProfitCaption.Location = new System.Drawing.Point(0, 0);
            this.lblProfitCaption.Name = "lblProfitCaption";
            this.lblProfitCaption.Size = new System.Drawing.Size(100, 23);
            this.lblProfitCaption.TabIndex = 0;
            // 
            // lblProfitValue
            // 
            this.lblProfitValue.Location = new System.Drawing.Point(0, 0);
            this.lblProfitValue.Name = "lblProfitValue";
            this.lblProfitValue.Size = new System.Drawing.Size(100, 23);
            this.lblProfitValue.TabIndex = 1;
            // 
            // lblStatus
            // 
            appearance10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblStatus.Appearance = appearance10;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblStatus.Location = new System.Drawing.Point(15, 74);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(1200, 18);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Ready. Click Search to load valuation listings.";
            // 
            // frmStockValuationReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.panelGrid);
            this.Controls.Add(this.panelFilters);
            this.Controls.Add(this.panelSummary);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "frmStockValuationReport";
            this.Text = "Stock Valuation Report";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panelFilters.ClientArea.ResumeLayout(false);
            this.panelFilters.ClientArea.PerformLayout();
            this.panelFilters.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboCategory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboStockFilter)).EndInit();
            this.panelGrid.ClientArea.ResumeLayout(false);
            this.panelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.panelSummary.ClientArea.ResumeLayout(false);
            this.panelSummary.ResumeLayout(false);
            this.cardItems.ClientArea.ResumeLayout(false);
            this.cardItems.ResumeLayout(false);
            this.cardQty.ClientArea.ResumeLayout(false);
            this.cardQty.ResumeLayout(false);
            this.cardCostVal.ClientArea.ResumeLayout(false);
            this.cardCostVal.ResumeLayout(false);
            this.cardRetailVal.ClientArea.ResumeLayout(false);
            this.cardRetailVal.ResumeLayout(false);
            this.cardProfit.ClientArea.ResumeLayout(false);
            this.cardProfit.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Infragistics.Win.Misc.UltraPanel panelFilters;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.Misc.UltraLabel lblPeriod;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboPeriod;
        private Infragistics.Win.Misc.UltraLabel lblGroup;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboGroup;
        private Infragistics.Win.Misc.UltraLabel lblCategory;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboCategory;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblStockFilter;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboStockFilter;
        private Infragistics.Win.Misc.UltraButton btnSearch;
        private Infragistics.Win.Misc.UltraButton btnReset;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private Infragistics.Win.Misc.UltraPanel panelGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;
        private Infragistics.Win.Misc.UltraPanel panelSummary;
        private Infragistics.Win.Misc.UltraPanel cardItems;
        private Infragistics.Win.Misc.UltraLabel lblItemsCaption;
        private Infragistics.Win.Misc.UltraLabel lblItemsValue;
        private Infragistics.Win.Misc.UltraPanel cardQty;
        private Infragistics.Win.Misc.UltraLabel lblQtyCaption;
        private Infragistics.Win.Misc.UltraLabel lblQtyValue;
        private Infragistics.Win.Misc.UltraPanel cardCostVal;
        private Infragistics.Win.Misc.UltraLabel lblCostCaption;
        private Infragistics.Win.Misc.UltraLabel lblCostValue;
        private Infragistics.Win.Misc.UltraPanel cardRetailVal;
        private Infragistics.Win.Misc.UltraLabel lblRetailCaption;
        private Infragistics.Win.Misc.UltraLabel lblRetailValue;
        private Infragistics.Win.Misc.UltraPanel cardProfit;
        private Infragistics.Win.Misc.UltraLabel lblProfitCaption;
        private Infragistics.Win.Misc.UltraLabel lblProfitValue;
        private Infragistics.Win.Misc.UltraLabel lblStatus;
    }
}
