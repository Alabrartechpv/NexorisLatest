namespace PosBranch_Win.Reports.SalesReports
{
    partial class frmItemwiseSalesSummaryReport
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
            this.components = new System.ComponentModel.Container();
            this.panelHeader      = new Infragistics.Win.Misc.UltraPanel();
            this.lblTitle         = new Infragistics.Win.Misc.UltraLabel();
            this.lblSubtitle      = new Infragistics.Win.Misc.UltraLabel();
            this.panelFilters     = new Infragistics.Win.Misc.UltraPanel();
            this.lblFromDate      = new Infragistics.Win.Misc.UltraLabel();
            this.dtFrom           = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate        = new Infragistics.Win.Misc.UltraLabel();
            this.dtTo             = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblPeriod        = new Infragistics.Win.Misc.UltraLabel();
            this.comboPeriod      = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblGroup         = new Infragistics.Win.Misc.UltraLabel();
            this.comboGroup       = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCategory      = new Infragistics.Win.Misc.UltraLabel();
            this.comboCategory    = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSearch        = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch        = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblStockFilter   = new Infragistics.Win.Misc.UltraLabel();
            this.comboStockFilter = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.btnSearch        = new Infragistics.Win.Misc.UltraButton();
            this.btnReset         = new Infragistics.Win.Misc.UltraButton();
            this.btnExport        = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint         = new Infragistics.Win.Misc.UltraButton();
            this.btnClose         = new Infragistics.Win.Misc.UltraButton();
            this.panelGrid        = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport       = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelSummary     = new Infragistics.Win.Misc.UltraPanel();
            this.cardItems        = new Infragistics.Win.Misc.UltraPanel();
            this.lblItemsCaption  = new Infragistics.Win.Misc.UltraLabel();
            this.lblItemsValue    = new Infragistics.Win.Misc.UltraLabel();
            this.cardQty          = new Infragistics.Win.Misc.UltraPanel();
            this.lblQtyCaption    = new Infragistics.Win.Misc.UltraLabel();
            this.lblQtyValue      = new Infragistics.Win.Misc.UltraLabel();
            this.cardCostVal      = new Infragistics.Win.Misc.UltraPanel();
            this.lblCostCaption   = new Infragistics.Win.Misc.UltraLabel();
            this.lblCostValue     = new Infragistics.Win.Misc.UltraLabel();
            this.cardRetailVal    = new Infragistics.Win.Misc.UltraPanel();
            this.lblRetailCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblRetailValue   = new Infragistics.Win.Misc.UltraLabel();
            this.cardProfit       = new Infragistics.Win.Misc.UltraPanel();
            this.lblProfitCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblProfitValue   = new Infragistics.Win.Misc.UltraLabel();
            this.lblStatus        = new Infragistics.Win.Misc.UltraLabel();
            this.panelHeader.SuspendLayout();
            this.panelFilters.SuspendLayout();
            this.panelGrid.SuspendLayout();
            this.panelSummary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboCategory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboStockFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.cardItems.SuspendLayout();
            this.cardQty.SuspendLayout();
            this.cardCostVal.SuspendLayout();
            this.cardRetailVal.SuspendLayout();
            this.cardProfit.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Height = 60;
            this.panelHeader.UseAppStyling = false;
            this.panelHeader.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.panelHeader.Appearance.BackColor = System.Drawing.Color.FromArgb(30, 40, 55);
            this.panelHeader.ClientArea.Controls.Add(this.lblSubtitle);
            this.panelHeader.ClientArea.Controls.Add(this.lblTitle);
            this.panelHeader.Name = "panelHeader";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.UseAppStyling = false;
            this.lblTitle.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(16, 8);
            this.lblTitle.Text = "📈  Item-wise Sales & Profit Summary";
            this.lblTitle.Name = "lblTitle";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.UseAppStyling = false;
            this.lblSubtitle.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSubtitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(180, 200, 220);
            this.lblSubtitle.Location = new System.Drawing.Point(18, 37);
            this.lblSubtitle.Text = "Analyze sales quantity, revenue, and gross profit margins by product";
            this.lblSubtitle.Name = "lblSubtitle";
            // 
            // panelFilters
            // 
            this.panelFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilters.Size = new System.Drawing.Size(1280, 100);
            this.panelFilters.UseAppStyling = false;
            this.panelFilters.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.panelFilters.Appearance.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.panelFilters.Appearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.panelFilters.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // Row 1
            // 
            this.lblFromDate.Text = "From Date:";
            this.lblFromDate.Location = new System.Drawing.Point(15, 20);
            this.lblFromDate.Size = new System.Drawing.Size(68, 23);
            this.lblFromDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFromDate.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // dtFrom
            // 
            this.dtFrom.Location = new System.Drawing.Point(88, 16);
            this.dtFrom.Size = new System.Drawing.Size(115, 25);
            this.dtFrom.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // lblToDate
            // 
            this.lblToDate.Text = "To Date:";
            this.lblToDate.Location = new System.Drawing.Point(215, 20);
            this.lblToDate.Size = new System.Drawing.Size(58, 23);
            this.lblToDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblToDate.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // dtTo
            // 
            this.dtTo.Location = new System.Drawing.Point(278, 16);
            this.dtTo.Size = new System.Drawing.Size(115, 25);
            this.dtTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // lblPeriod
            // 
            this.lblPeriod.Text = "Period:";
            this.lblPeriod.Location = new System.Drawing.Point(406, 20);
            this.lblPeriod.Size = new System.Drawing.Size(48, 23);
            this.lblPeriod.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPeriod.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // comboPeriod
            // 
            this.comboPeriod.Location = new System.Drawing.Point(458, 16);
            this.comboPeriod.Size = new System.Drawing.Size(125, 25);
            this.comboPeriod.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // lblSearch
            // 
            this.lblSearch.Text = "Search:";
            this.lblSearch.Location = new System.Drawing.Point(597, 20);
            this.lblSearch.Size = new System.Drawing.Size(50, 23);
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSearch.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(650, 16);
            this.txtSearch.Size = new System.Drawing.Size(210, 25);
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // Row 2
            // 
            this.lblGroup.Text = "Group:";
            this.lblGroup.Location = new System.Drawing.Point(15, 60);
            this.lblGroup.Size = new System.Drawing.Size(68, 23);
            this.lblGroup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGroup.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // comboGroup
            // 
            this.comboGroup.Location = new System.Drawing.Point(88, 56);
            this.comboGroup.Size = new System.Drawing.Size(115, 25);
            this.comboGroup.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // lblCategory
            // 
            this.lblCategory.Text = "Category:";
            this.lblCategory.Location = new System.Drawing.Point(215, 60);
            this.lblCategory.Size = new System.Drawing.Size(58, 23);
            this.lblCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCategory.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // comboCategory
            // 
            this.comboCategory.Location = new System.Drawing.Point(278, 56);
            this.comboCategory.Size = new System.Drawing.Size(115, 25);
            this.comboCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // lblStockFilter
            // 
            this.lblStockFilter.Text = "Show:";
            this.lblStockFilter.Location = new System.Drawing.Point(406, 60);
            this.lblStockFilter.Size = new System.Drawing.Size(48, 23);
            this.lblStockFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStockFilter.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // comboStockFilter
            // 
            this.comboStockFilter.Location = new System.Drawing.Point(458, 56);
            this.comboStockFilter.Size = new System.Drawing.Size(125, 25);
            this.comboStockFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // Buttons
            // 
            this.btnSearch.Location = new System.Drawing.Point(630, 56);
            this.btnSearch.Size = new System.Drawing.Size(100, 28);
            this.btnSearch.Text = "Search  [F5]";
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(738, 56);
            this.btnReset.Size = new System.Drawing.Size(82, 28);
            this.btnReset.Text = "↺  Reset";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(828, 56);
            this.btnExport.Size = new System.Drawing.Size(95, 28);
            this.btnExport.Text = "⬇  Export CSV";
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(931, 56);
            this.btnPrint.Size = new System.Drawing.Size(78, 28);
            this.btnPrint.Text = "🖨  Print";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(1017, 56);
            this.btnClose.Size = new System.Drawing.Size(78, 28);
            this.btnClose.Text = "✕  Close";
            // 
            // Add to filters panel
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
            // 
            // panelGrid
            // 
            this.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGrid.ClientArea.Controls.Add(this.gridReport);
            this.panelGrid.Name = "panelGrid";
            // 
            // gridReport
            // 
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Name = "gridReport";
            // 
            // panelSummary
            // 
            this.panelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelSummary.Height = 98;
            this.panelSummary.UseAppStyling = false;
            this.panelSummary.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.panelSummary.Appearance.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.panelSummary.Appearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.panelSummary.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.panelSummary.ClientArea.Controls.Add(this.cardItems);
            this.panelSummary.ClientArea.Controls.Add(this.cardQty);
            this.panelSummary.ClientArea.Controls.Add(this.cardCostVal);
            this.panelSummary.ClientArea.Controls.Add(this.cardRetailVal);
            this.panelSummary.ClientArea.Controls.Add(this.cardProfit);
            this.panelSummary.ClientArea.Controls.Add(this.lblStatus);
            this.panelSummary.Name = "panelSummary";
            // 
            // cardItems
            // 
            this.cardItems.ClientArea.Controls.Add(this.lblItemsCaption);
            this.cardItems.ClientArea.Controls.Add(this.lblItemsValue);
            this.cardItems.Name = "pnlCardItems";
            this.cardItems.Size = new System.Drawing.Size(238, 62);
            // 
            // cardQty
            // 
            this.cardQty.ClientArea.Controls.Add(this.lblQtyCaption);
            this.cardQty.ClientArea.Controls.Add(this.lblQtyValue);
            this.cardQty.Name = "pnlCardQty";
            this.cardQty.Size = new System.Drawing.Size(238, 62);
            // 
            // cardCostVal
            // 
            this.cardCostVal.ClientArea.Controls.Add(this.lblCostCaption);
            this.cardCostVal.ClientArea.Controls.Add(this.lblCostValue);
            this.cardCostVal.Name = "pnlCardCostVal";
            this.cardCostVal.Size = new System.Drawing.Size(238, 62);
            // 
            // cardRetailVal
            // 
            this.cardRetailVal.ClientArea.Controls.Add(this.lblRetailCaption);
            this.cardRetailVal.ClientArea.Controls.Add(this.lblRetailValue);
            this.cardRetailVal.Name = "pnlCardRetailVal";
            this.cardRetailVal.Size = new System.Drawing.Size(238, 62);
            // 
            // cardProfit
            // 
            this.cardProfit.ClientArea.Controls.Add(this.lblProfitCaption);
            this.cardProfit.ClientArea.Controls.Add(this.lblProfitValue);
            this.cardProfit.Name = "pnlCardProfit";
            this.cardProfit.Size = new System.Drawing.Size(238, 62);
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(15, 74);
            this.lblStatus.Size = new System.Drawing.Size(1200, 18);
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblStatus.Appearance.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblStatus.Text = "Ready. Click Search to load sales listings.";
            this.lblStatus.Name = "lblStatus";
            // 
            // frmItemwiseSalesSummaryReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.panelGrid);
            this.Controls.Add(this.panelFilters);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.panelSummary);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "frmItemwiseSalesSummaryReport";
            this.Text = "Item-wise Sales & Profit Summary";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelFilters.ResumeLayout(false);
            this.panelFilters.PerformLayout();
            this.panelGrid.ResumeLayout(false);
            this.panelSummary.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboCategory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboStockFilter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.cardItems.ResumeLayout(false);
            this.cardQty.ResumeLayout(false);
            this.cardCostVal.ResumeLayout(false);
            this.cardRetailVal.ResumeLayout(false);
            this.cardProfit.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel panelHeader;
        private Infragistics.Win.Misc.UltraLabel lblTitle;
        private Infragistics.Win.Misc.UltraLabel lblSubtitle;
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
