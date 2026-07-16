namespace PosBranch_Win.Reports.InventoryReport
{
    partial class frmStockAdjustmentReport
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
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance6 = new Infragistics.Win.Appearance();
            this.panelFilters = new Infragistics.Win.Misc.UltraPanel();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtpFromDate = new System.Windows.Forms.DateTimePicker();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtpToDate = new System.Windows.Forms.DateTimePicker();
            this.lblType = new Infragistics.Win.Misc.UltraLabel();
            this.comboType = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
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
            this.cardDocCount = new Infragistics.Win.Misc.UltraPanel();
            this.lblDocCountCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblDocCountValue = new Infragistics.Win.Misc.UltraLabel();
            this.cardStockIn = new Infragistics.Win.Misc.UltraPanel();
            this.lblStockInCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblStockInValue = new Infragistics.Win.Misc.UltraLabel();
            this.cardStockOut = new Infragistics.Win.Misc.UltraPanel();
            this.lblStockOutCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblStockOutValue = new Infragistics.Win.Misc.UltraLabel();
            this.cardNetValue = new Infragistics.Win.Misc.UltraPanel();
            this.lblNetValueCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblNetValueValue = new Infragistics.Win.Misc.UltraLabel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelFilters.ClientArea.SuspendLayout();
            this.panelFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            this.panelGrid.ClientArea.SuspendLayout();
            this.panelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.panelSummary.ClientArea.SuspendLayout();
            this.panelSummary.SuspendLayout();
            this.cardDocCount.ClientArea.SuspendLayout();
            this.cardDocCount.SuspendLayout();
            this.cardStockIn.ClientArea.SuspendLayout();
            this.cardStockIn.SuspendLayout();
            this.cardStockOut.ClientArea.SuspendLayout();
            this.cardStockOut.SuspendLayout();
            this.cardNetValue.ClientArea.SuspendLayout();
            this.cardNetValue.SuspendLayout();
            this.statusStrip.SuspendLayout();
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
            this.panelFilters.ClientArea.Controls.Add(this.dtpFromDate);
            this.panelFilters.ClientArea.Controls.Add(this.lblToDate);
            this.panelFilters.ClientArea.Controls.Add(this.dtpToDate);
            this.panelFilters.ClientArea.Controls.Add(this.lblType);
            this.panelFilters.ClientArea.Controls.Add(this.comboType);
            this.panelFilters.ClientArea.Controls.Add(this.lblSearch);
            this.panelFilters.ClientArea.Controls.Add(this.txtSearch);
            this.panelFilters.ClientArea.Controls.Add(this.btnSearch);
            this.panelFilters.ClientArea.Controls.Add(this.btnReset);
            this.panelFilters.ClientArea.Controls.Add(this.btnExport);
            this.panelFilters.ClientArea.Controls.Add(this.btnPrint);
            this.panelFilters.ClientArea.Controls.Add(this.btnClose);
            this.panelFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilters.Location = new System.Drawing.Point(0, 0);
            this.panelFilters.Name = "panelFilters";
            this.panelFilters.Size = new System.Drawing.Size(1250, 64);
            this.panelFilters.TabIndex = 5;
            this.panelFilters.UseAppStyling = false;
            this.panelFilters.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // lblFromDate
            // 
            appearance2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblFromDate.Appearance = appearance2;
            this.lblFromDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFromDate.Location = new System.Drawing.Point(15, 22);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(40, 23);
            this.lblFromDate.TabIndex = 0;
            this.lblFromDate.Text = "From:";
            // 
            // dtpFromDate
            // 
            this.dtpFromDate.CustomFormat = "dd-MM-yyyy";
            this.dtpFromDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpFromDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFromDate.Location = new System.Drawing.Point(58, 18);
            this.dtpFromDate.Name = "dtpFromDate";
            this.dtpFromDate.Size = new System.Drawing.Size(105, 23);
            this.dtpFromDate.TabIndex = 1;
            // 
            // lblToDate
            // 
            appearance3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblToDate.Appearance = appearance3;
            this.lblToDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblToDate.Location = new System.Drawing.Point(174, 22);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(25, 23);
            this.lblToDate.TabIndex = 2;
            this.lblToDate.Text = "To:";
            // 
            // dtpToDate
            // 
            this.dtpToDate.CustomFormat = "dd-MM-yyyy";
            this.dtpToDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpToDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpToDate.Location = new System.Drawing.Point(202, 18);
            this.dtpToDate.Name = "dtpToDate";
            this.dtpToDate.Size = new System.Drawing.Size(105, 23);
            this.dtpToDate.TabIndex = 3;
            // 
            // lblType
            // 
            appearance4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblType.Appearance = appearance4;
            this.lblType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblType.Location = new System.Drawing.Point(318, 22);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(38, 23);
            this.lblType.TabIndex = 4;
            this.lblType.Text = "Type:";
            // 
            // comboType
            // 
            this.comboType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.comboType.Location = new System.Drawing.Point(358, 18);
            this.comboType.Name = "comboType";
            this.comboType.Size = new System.Drawing.Size(105, 25);
            this.comboType.TabIndex = 5;
            // 
            // lblSearch
            // 
            appearance5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblSearch.Appearance = appearance5;
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSearch.Location = new System.Drawing.Point(474, 22);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(50, 23);
            this.lblSearch.TabIndex = 6;
            this.lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.Location = new System.Drawing.Point(526, 18);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(170, 25);
            this.txtSearch.TabIndex = 7;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(708, 17);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(92, 28);
            this.btnSearch.TabIndex = 8;
            this.btnSearch.Text = "Search [F5]";
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(805, 17);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(72, 28);
            this.btnReset.TabIndex = 9;
            this.btnReset.Text = "Reset";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(882, 17);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(96, 28);
            this.btnExport.TabIndex = 10;
            this.btnExport.Text = "Export (Ctrl+E)";
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(983, 17);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(90, 28);
            this.btnPrint.TabIndex = 11;
            this.btnPrint.Text = "Print (Ctrl+P)";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(1078, 17);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(72, 28);
            this.btnClose.TabIndex = 12;
            this.btnClose.Text = "Close";
            // 
            // panelGrid
            // 
            // 
            // panelGrid.ClientArea
            // 
            this.panelGrid.ClientArea.Controls.Add(this.gridReport);
            this.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGrid.Location = new System.Drawing.Point(0, 64);
            this.panelGrid.Name = "panelGrid";
            this.panelGrid.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.panelGrid.Size = new System.Drawing.Size(1250, 565);
            this.panelGrid.TabIndex = 0;
            // 
            // gridReport
            // 
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Location = new System.Drawing.Point(0, 0);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1250, 565);
            this.gridReport.TabIndex = 0;
            // 
            // panelSummary
            // 
            appearance6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            appearance6.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(226)))), ((int)(((byte)(235)))));
            this.panelSummary.Appearance = appearance6;
            this.panelSummary.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // panelSummary.ClientArea
            // 
            this.panelSummary.ClientArea.Controls.Add(this.cardDocCount);
            this.panelSummary.ClientArea.Controls.Add(this.cardStockIn);
            this.panelSummary.ClientArea.Controls.Add(this.cardStockOut);
            this.panelSummary.ClientArea.Controls.Add(this.cardNetValue);
            this.panelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelSummary.Location = new System.Drawing.Point(0, 629);
            this.panelSummary.Name = "panelSummary";
            this.panelSummary.Size = new System.Drawing.Size(1250, 98);
            this.panelSummary.TabIndex = 1;
            this.panelSummary.UseAppStyling = false;
            this.panelSummary.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // cardDocCount
            // 
            // 
            // cardDocCount.ClientArea
            // 
            this.cardDocCount.ClientArea.Controls.Add(this.lblDocCountCaption);
            this.cardDocCount.ClientArea.Controls.Add(this.lblDocCountValue);
            this.cardDocCount.Location = new System.Drawing.Point(0, 0);
            this.cardDocCount.Name = "cardDocCount";
            this.cardDocCount.Size = new System.Drawing.Size(200, 100);
            this.cardDocCount.TabIndex = 0;
            // 
            // lblDocCountCaption
            // 
            this.lblDocCountCaption.Location = new System.Drawing.Point(12, 8);
            this.lblDocCountCaption.Name = "lblDocCountCaption";
            this.lblDocCountCaption.Size = new System.Drawing.Size(200, 15);
            this.lblDocCountCaption.TabIndex = 0;
            // 
            // lblDocCountValue
            // 
            this.lblDocCountValue.Location = new System.Drawing.Point(12, 26);
            this.lblDocCountValue.Name = "lblDocCountValue";
            this.lblDocCountValue.Size = new System.Drawing.Size(200, 28);
            this.lblDocCountValue.TabIndex = 1;
            // 
            // cardStockIn
            // 
            // 
            // cardStockIn.ClientArea
            // 
            this.cardStockIn.ClientArea.Controls.Add(this.lblStockInCaption);
            this.cardStockIn.ClientArea.Controls.Add(this.lblStockInValue);
            this.cardStockIn.Location = new System.Drawing.Point(0, 0);
            this.cardStockIn.Name = "cardStockIn";
            this.cardStockIn.Size = new System.Drawing.Size(200, 100);
            this.cardStockIn.TabIndex = 1;
            // 
            // lblStockInCaption
            // 
            this.lblStockInCaption.Location = new System.Drawing.Point(12, 8);
            this.lblStockInCaption.Name = "lblStockInCaption";
            this.lblStockInCaption.Size = new System.Drawing.Size(200, 15);
            this.lblStockInCaption.TabIndex = 0;
            // 
            // lblStockInValue
            // 
            this.lblStockInValue.Location = new System.Drawing.Point(12, 26);
            this.lblStockInValue.Name = "lblStockInValue";
            this.lblStockInValue.Size = new System.Drawing.Size(200, 28);
            this.lblStockInValue.TabIndex = 1;
            // 
            // cardStockOut
            // 
            // 
            // cardStockOut.ClientArea
            // 
            this.cardStockOut.ClientArea.Controls.Add(this.lblStockOutCaption);
            this.cardStockOut.ClientArea.Controls.Add(this.lblStockOutValue);
            this.cardStockOut.Location = new System.Drawing.Point(0, 0);
            this.cardStockOut.Name = "cardStockOut";
            this.cardStockOut.Size = new System.Drawing.Size(200, 100);
            this.cardStockOut.TabIndex = 2;
            // 
            // lblStockOutCaption
            // 
            this.lblStockOutCaption.Location = new System.Drawing.Point(12, 8);
            this.lblStockOutCaption.Name = "lblStockOutCaption";
            this.lblStockOutCaption.Size = new System.Drawing.Size(200, 15);
            this.lblStockOutCaption.TabIndex = 0;
            // 
            // lblStockOutValue
            // 
            this.lblStockOutValue.Location = new System.Drawing.Point(12, 26);
            this.lblStockOutValue.Name = "lblStockOutValue";
            this.lblStockOutValue.Size = new System.Drawing.Size(200, 28);
            this.lblStockOutValue.TabIndex = 1;
            // 
            // cardNetValue
            // 
            // 
            // cardNetValue.ClientArea
            // 
            this.cardNetValue.ClientArea.Controls.Add(this.lblNetValueCaption);
            this.cardNetValue.ClientArea.Controls.Add(this.lblNetValueValue);
            this.cardNetValue.Location = new System.Drawing.Point(0, 0);
            this.cardNetValue.Name = "cardNetValue";
            this.cardNetValue.Size = new System.Drawing.Size(200, 100);
            this.cardNetValue.TabIndex = 3;
            // 
            // lblNetValueCaption
            // 
            this.lblNetValueCaption.Location = new System.Drawing.Point(12, 8);
            this.lblNetValueCaption.Name = "lblNetValueCaption";
            this.lblNetValueCaption.Size = new System.Drawing.Size(200, 15);
            this.lblNetValueCaption.TabIndex = 0;
            // 
            // lblNetValueValue
            // 
            this.lblNetValueValue.Location = new System.Drawing.Point(12, 26);
            this.lblNetValueValue.Name = "lblNetValueValue";
            this.lblNetValueValue.Size = new System.Drawing.Size(200, 28);
            this.lblNetValueValue.TabIndex = 1;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 727);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1250, 22);
            this.statusStrip.TabIndex = 4;
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(225, 17);
            this.lblStatus.Text = "Ready | Select filters and press Search (F5)";
            // 
            // frmStockAdjustmentReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1250, 749);
            this.Controls.Add(this.panelGrid);
            this.Controls.Add(this.panelSummary);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.panelFilters);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "frmStockAdjustmentReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stock Adjustment Report";
            this.Load += new System.EventHandler(this.FrmStockAdjustmentReport_Load);
            this.panelFilters.ClientArea.ResumeLayout(false);
            this.panelFilters.ClientArea.PerformLayout();
            this.panelFilters.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.comboType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            this.panelGrid.ClientArea.ResumeLayout(false);
            this.panelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.panelSummary.ClientArea.ResumeLayout(false);
            this.panelSummary.ResumeLayout(false);
            this.cardDocCount.ClientArea.ResumeLayout(false);
            this.cardDocCount.ResumeLayout(false);
            this.cardStockIn.ClientArea.ResumeLayout(false);
            this.cardStockIn.ResumeLayout(false);
            this.cardStockOut.ClientArea.ResumeLayout(false);
            this.cardStockOut.ResumeLayout(false);
            this.cardNetValue.ClientArea.ResumeLayout(false);
            this.cardNetValue.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Infragistics.Win.Misc.UltraPanel panelFilters;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private System.Windows.Forms.DateTimePicker dtpFromDate;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private System.Windows.Forms.DateTimePicker dtpToDate;
        private Infragistics.Win.Misc.UltraLabel lblType;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboType;
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
        private Infragistics.Win.Misc.UltraPanel cardDocCount;
        private Infragistics.Win.Misc.UltraLabel lblDocCountCaption;
        private Infragistics.Win.Misc.UltraLabel lblDocCountValue;
        private Infragistics.Win.Misc.UltraPanel cardStockIn;
        private Infragistics.Win.Misc.UltraLabel lblStockInCaption;
        private Infragistics.Win.Misc.UltraLabel lblStockInValue;
        private Infragistics.Win.Misc.UltraPanel cardStockOut;
        private Infragistics.Win.Misc.UltraLabel lblStockOutCaption;
        private Infragistics.Win.Misc.UltraLabel lblStockOutValue;
        private Infragistics.Win.Misc.UltraPanel cardNetValue;
        private Infragistics.Win.Misc.UltraLabel lblNetValueCaption;
        private Infragistics.Win.Misc.UltraLabel lblNetValueValue;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}
