namespace PosBranch_Win.Reports.InventoryReport
{
    partial class frmLowStockAlertReport
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
            this.panelHeader = new Infragistics.Win.Misc.UltraPanel();
            this.lblTitle = new Infragistics.Win.Misc.UltraLabel();
            this.panelFilters = new Infragistics.Win.Misc.UltraPanel();
            this.lblGroup = new Infragistics.Win.Misc.UltraLabel();
            this.comboGroup = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCategory = new Infragistics.Win.Misc.UltraLabel();
            this.comboCategory = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.btnSearch = new Infragistics.Win.Misc.UltraButton();
            this.btnReset = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.panelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.cardItemCount = new Infragistics.Win.Misc.UltraPanel();
            this.lblItemCountCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblItemCountValue = new Infragistics.Win.Misc.UltraLabel();
            this.cardStockValueCost = new Infragistics.Win.Misc.UltraPanel();
            this.lblStockValueCostCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblStockValueCostValue = new Infragistics.Win.Misc.UltraLabel();
            this.cardShortageCost = new Infragistics.Win.Misc.UltraPanel();
            this.lblShortageCostCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblShortageCostValue = new Infragistics.Win.Misc.UltraLabel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            
            this.panelHeader.SuspendLayout();
            this.panelFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboCategory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            this.panelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.panelSummary.SuspendLayout();
            this.cardItemCount.SuspendLayout();
            this.cardStockValueCost.SuspendLayout();
            this.cardShortageCost.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Height = 45;
            this.panelHeader.UseAppStyling = false;
            this.panelHeader.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.panelHeader.Appearance.BackColor = System.Drawing.Color.FromArgb(30, 40, 55);
            this.panelHeader.ClientArea.Controls.Add(this.lblTitle);
            this.panelHeader.Name = "panelHeader";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.UseAppStyling = false;
            this.lblTitle.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(15, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(262, 21);
            this.lblTitle.Text = "Low Stock Alert Report (Reorder Levels)";
            // 
            // panelFilters
            // 
            this.panelFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilters.Height = 60;
            this.panelFilters.UseAppStyling = false;
            this.panelFilters.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.panelFilters.Appearance.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.panelFilters.Appearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.panelFilters.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.panelFilters.ClientArea.Controls.Add(this.lblGroup);
            this.panelFilters.ClientArea.Controls.Add(this.comboGroup);
            this.panelFilters.ClientArea.Controls.Add(this.lblCategory);
            this.panelFilters.ClientArea.Controls.Add(this.comboCategory);
            this.panelFilters.ClientArea.Controls.Add(this.lblSearch);
            this.panelFilters.ClientArea.Controls.Add(this.txtSearch);
            this.panelFilters.ClientArea.Controls.Add(this.btnSearch);
            this.panelFilters.ClientArea.Controls.Add(this.btnReset);
            this.panelFilters.ClientArea.Controls.Add(this.btnExport);
            this.panelFilters.ClientArea.Controls.Add(this.btnPrint);
            this.panelFilters.ClientArea.Controls.Add(this.btnClose);
            this.panelFilters.Name = "panelFilters";
            // 
            // lblGroup
            // 
            this.lblGroup.Location = new System.Drawing.Point(15, 20);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(48, 23);
            this.lblGroup.Text = "Group:";
            this.lblGroup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGroup.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // comboGroup
            // 
            this.comboGroup.Location = new System.Drawing.Point(65, 16);
            this.comboGroup.Name = "comboGroup";
            this.comboGroup.Size = new System.Drawing.Size(125, 25);
            this.comboGroup.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // lblCategory
            // 
            this.lblCategory.Location = new System.Drawing.Point(205, 20);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(60, 23);
            this.lblCategory.Text = "Category:";
            this.lblCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCategory.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // comboCategory
            // 
            this.comboCategory.Location = new System.Drawing.Point(270, 16);
            this.comboCategory.Name = "comboCategory";
            this.comboCategory.Size = new System.Drawing.Size(125, 25);
            this.comboCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // lblSearch
            // 
            this.lblSearch.Location = new System.Drawing.Point(410, 20);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(50, 23);
            this.lblSearch.Text = "Search:";
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSearch.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(462, 16);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(160, 25);
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(635, 15);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(95, 28);
            this.btnSearch.Text = "Search [F5]";
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(735, 15);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 28);
            this.btnReset.Text = "Reset";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(815, 15);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(95, 28);
            this.btnExport.Text = "Export (Ctrl+E)";
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(915, 15);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(90, 28);
            this.btnPrint.Text = "Print (Ctrl+P)";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(1010, 15);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 28);
            this.btnClose.Text = "Close";
            // 
            // panelGrid
            // 
            this.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGrid.ClientArea.Controls.Add(this.gridReport);
            this.panelGrid.Name = "panelGrid";
            this.panelGrid.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            // 
            // gridReport
            // 
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1220, 505);
            // 
            // panelSummary
            // 
            this.panelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelSummary.Height = 98;
            this.panelSummary.UseAppStyling = false;
            this.panelSummary.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.panelSummary.Appearance.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.panelSummary.Appearance.BorderColor = System.Drawing.Color.FromArgb(218, 226, 235);
            this.panelSummary.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.panelSummary.ClientArea.Controls.Add(this.cardItemCount);
            this.panelSummary.ClientArea.Controls.Add(this.cardStockValueCost);
            this.panelSummary.ClientArea.Controls.Add(this.cardShortageCost);
            this.panelSummary.Name = "panelSummary";
            // 
            // cardItemCount
            // 
            this.cardItemCount.ClientArea.Controls.Add(this.lblItemCountCaption);
            this.cardItemCount.ClientArea.Controls.Add(this.lblItemCountValue);
            this.cardItemCount.Name = "cardItemCount";
            // 
            // lblItemCountCaption
            // 
            this.lblItemCountCaption.Location = new System.Drawing.Point(12, 8);
            this.lblItemCountCaption.Name = "lblItemCountCaption";
            this.lblItemCountCaption.Size = new System.Drawing.Size(200, 15);
            // 
            // lblItemCountValue
            // 
            this.lblItemCountValue.Location = new System.Drawing.Point(12, 26);
            this.lblItemCountValue.Name = "lblItemCountValue";
            this.lblItemCountValue.Size = new System.Drawing.Size(200, 28);
            // 
            // cardStockValueCost
            // 
            this.cardStockValueCost.ClientArea.Controls.Add(this.lblStockValueCostCaption);
            this.cardStockValueCost.ClientArea.Controls.Add(this.lblStockValueCostValue);
            this.cardStockValueCost.Name = "cardStockValueCost";
            // 
            // lblStockValueCostCaption
            // 
            this.lblStockValueCostCaption.Location = new System.Drawing.Point(12, 8);
            this.lblStockValueCostCaption.Name = "lblStockValueCostCaption";
            this.lblStockValueCostCaption.Size = new System.Drawing.Size(200, 15);
            // 
            // lblStockValueCostValue
            // 
            this.lblStockValueCostValue.Location = new System.Drawing.Point(12, 26);
            this.lblStockValueCostValue.Name = "lblStockValueCostValue";
            this.lblStockValueCostValue.Size = new System.Drawing.Size(200, 28);
            // 
            // cardShortageCost
            // 
            this.cardShortageCost.ClientArea.Controls.Add(this.lblShortageCostCaption);
            this.cardShortageCost.ClientArea.Controls.Add(this.lblShortageCostValue);
            this.cardShortageCost.Name = "cardShortageCost";
            // 
            // lblShortageCostCaption
            // 
            this.lblShortageCostCaption.Location = new System.Drawing.Point(12, 8);
            this.lblShortageCostCaption.Name = "lblShortageCostCaption";
            this.lblShortageCostCaption.Size = new System.Drawing.Size(200, 15);
            // 
            // lblShortageCostValue
            // 
            this.lblShortageCostValue.Location = new System.Drawing.Point(12, 26);
            this.lblShortageCostValue.Name = "lblShortageCostValue";
            this.lblShortageCostValue.Size = new System.Drawing.Size(200, 28);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 728);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1250, 22);
            this.statusStrip.TabIndex = 4;
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(294, 17);
            this.lblStatus.Text = "Ready  |  Select filters and press Search (F5)";
            // 
            // frmLowStockAlertReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1250, 750);
            this.Controls.Add(this.panelGrid);
            this.Controls.Add(this.panelSummary);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.panelFilters);
            this.Controls.Add(this.panelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "frmLowStockAlertReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Low Stock Alert Report";
            this.Load += new System.EventHandler(this.FrmLowStockAlertReport_Load);
            
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelFilters.ResumeLayout(false);
            this.panelFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboCategory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            this.panelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.panelSummary.ResumeLayout(false);
            this.cardItemCount.ResumeLayout(false);
            this.cardItemCount.PerformLayout();
            this.cardStockValueCost.ResumeLayout(false);
            this.cardStockValueCost.PerformLayout();
            this.cardShortageCost.ResumeLayout(false);
            this.cardShortageCost.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel panelHeader;
        private Infragistics.Win.Misc.UltraLabel lblTitle;
        private Infragistics.Win.Misc.UltraPanel panelFilters;
        private Infragistics.Win.Misc.UltraLabel lblGroup;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboGroup;
        private Infragistics.Win.Misc.UltraLabel lblCategory;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboCategory;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraButton btnSearch;
        private Infragistics.Win.Misc.UltraButton btnReset;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private Infragistics.Win.Misc.UltraPanel panelGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;
        private Infragistics.Win.Misc.UltraPanel panelSummary;
        
        private Infragistics.Win.Misc.UltraPanel cardItemCount;
        private Infragistics.Win.Misc.UltraLabel lblItemCountCaption;
        private Infragistics.Win.Misc.UltraLabel lblItemCountValue;
        
        private Infragistics.Win.Misc.UltraPanel cardStockValueCost;
        private Infragistics.Win.Misc.UltraLabel lblStockValueCostCaption;
        private Infragistics.Win.Misc.UltraLabel lblStockValueCostValue;
        
        private Infragistics.Win.Misc.UltraPanel cardShortageCost;
        private Infragistics.Win.Misc.UltraLabel lblShortageCostCaption;
        private Infragistics.Win.Misc.UltraLabel lblShortageCostValue;
        
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}
