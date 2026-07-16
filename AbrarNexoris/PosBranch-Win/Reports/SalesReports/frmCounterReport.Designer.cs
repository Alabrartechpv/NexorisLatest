namespace PosBranch_Win.Reports.SalesReports
{
    partial class frmCounterReport
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
            this.ultraGroupBoxGrid = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraGridCounterReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.lblDifferenceValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblDifferenceCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCollectionValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCollectionCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalNetSalesValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalNetSalesCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalSessionsValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalSessionsCaption = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxFilters = new Infragistics.Win.Misc.UltraGroupBox();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnClear = new Infragistics.Win.Misc.UltraButton();
            this.btnSearch = new Infragistics.Win.Misc.UltraButton();
            this.ultraComboPresetDates = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblQuickDate = new System.Windows.Forms.Label();
            this.ultraComboUser = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblUser = new System.Windows.Forms.Label();
            this.ultraComboCounter = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCounter = new System.Windows.Forms.Label();
            this.ultraDateTimeTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new System.Windows.Forms.Label();
            this.ultraDateTimeFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblFromDate = new System.Windows.Forms.Label();
            this.ultraPanelMain.ClientArea.SuspendLayout();
            this.ultraPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxGrid)).BeginInit();
            this.ultraGroupBoxGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridCounterReport)).BeginInit();
            this.ultraPanelSummary.ClientArea.SuspendLayout();
            this.ultraPanelSummary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFilters)).BeginInit();
            this.ultraGroupBoxFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPresetDates)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboCounter)).BeginInit();
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
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraGroupBoxGrid);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraPanelSummary);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraGroupBoxFilters);
            this.ultraPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMain.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelMain.Name = "ultraPanelMain";
            this.ultraPanelMain.Size = new System.Drawing.Size(1200, 600);
            this.ultraPanelMain.TabIndex = 0;
            // 
            // ultraGroupBoxGrid
            // 
            this.ultraGroupBoxGrid.Controls.Add(this.ultraGridCounterReport);
            this.ultraGroupBoxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGroupBoxGrid.Location = new System.Drawing.Point(0, 80);
            this.ultraGroupBoxGrid.Name = "ultraGroupBoxGrid";
            this.ultraGroupBoxGrid.Size = new System.Drawing.Size(1200, 450);
            this.ultraGroupBoxGrid.TabIndex = 1;
            this.ultraGroupBoxGrid.Text = "Counter Reports Data";
            // 
            // ultraGridCounterReport
            // 
            this.ultraGridCounterReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridCounterReport.Location = new System.Drawing.Point(3, 16);
            this.ultraGridCounterReport.Name = "ultraGridCounterReport";
            this.ultraGridCounterReport.Size = new System.Drawing.Size(1194, 431);
            this.ultraGridCounterReport.TabIndex = 0;
            this.ultraGridCounterReport.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraPanelSummary
            // 
            // 
            // ultraPanelSummary.ClientArea
            // 
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblDifferenceValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblDifferenceCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalCollectionValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalCollectionCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalNetSalesValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalNetSalesCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalSessionsValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalSessionsCaption);
            this.ultraPanelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelSummary.Location = new System.Drawing.Point(0, 530);
            this.ultraPanelSummary.Name = "ultraPanelSummary";
            this.ultraPanelSummary.Size = new System.Drawing.Size(1200, 70);
            this.ultraPanelSummary.TabIndex = 2;
            // 
            // lblDifferenceValue
            // 
            this.lblDifferenceValue.Location = new System.Drawing.Point(700, 32);
            this.lblDifferenceValue.Name = "lblDifferenceValue";
            this.lblDifferenceValue.Size = new System.Drawing.Size(150, 25);
            this.lblDifferenceValue.TabIndex = 7;
            this.lblDifferenceValue.Text = "₹ 0.00";
            // 
            // lblDifferenceCaption
            // 
            this.lblDifferenceCaption.Location = new System.Drawing.Point(700, 10);
            this.lblDifferenceCaption.Name = "lblDifferenceCaption";
            this.lblDifferenceCaption.Size = new System.Drawing.Size(120, 18);
            this.lblDifferenceCaption.TabIndex = 6;
            this.lblDifferenceCaption.Text = "Total Variance:";
            // 
            // lblTotalCollectionValue
            // 
            this.lblTotalCollectionValue.Location = new System.Drawing.Point(450, 32);
            this.lblTotalCollectionValue.Name = "lblTotalCollectionValue";
            this.lblTotalCollectionValue.Size = new System.Drawing.Size(160, 25);
            this.lblTotalCollectionValue.TabIndex = 5;
            this.lblTotalCollectionValue.Text = "₹ 0.00";
            // 
            // lblTotalCollectionCaption
            // 
            this.lblTotalCollectionCaption.Location = new System.Drawing.Point(450, 10);
            this.lblTotalCollectionCaption.Name = "lblTotalCollectionCaption";
            this.lblTotalCollectionCaption.Size = new System.Drawing.Size(120, 18);
            this.lblTotalCollectionCaption.TabIndex = 4;
            this.lblTotalCollectionCaption.Text = "Total Collection:";
            // 
            // lblTotalNetSalesValue
            // 
            this.lblTotalNetSalesValue.Location = new System.Drawing.Point(200, 32);
            this.lblTotalNetSalesValue.Name = "lblTotalNetSalesValue";
            this.lblTotalNetSalesValue.Size = new System.Drawing.Size(160, 25);
            this.lblTotalNetSalesValue.TabIndex = 3;
            this.lblTotalNetSalesValue.Text = "₹ 0.00";
            // 
            // lblTotalNetSalesCaption
            // 
            this.lblTotalNetSalesCaption.Location = new System.Drawing.Point(200, 10);
            this.lblTotalNetSalesCaption.Name = "lblTotalNetSalesCaption";
            this.lblTotalNetSalesCaption.Size = new System.Drawing.Size(120, 18);
            this.lblTotalNetSalesCaption.TabIndex = 2;
            this.lblTotalNetSalesCaption.Text = "Total Net Sales:";
            // 
            // lblTotalSessionsValue
            // 
            this.lblTotalSessionsValue.Location = new System.Drawing.Point(15, 32);
            this.lblTotalSessionsValue.Name = "lblTotalSessionsValue";
            this.lblTotalSessionsValue.Size = new System.Drawing.Size(100, 25);
            this.lblTotalSessionsValue.TabIndex = 1;
            this.lblTotalSessionsValue.Text = "0";
            // 
            // lblTotalSessionsCaption
            // 
            this.lblTotalSessionsCaption.Location = new System.Drawing.Point(15, 10);
            this.lblTotalSessionsCaption.Name = "lblTotalSessionsCaption";
            this.lblTotalSessionsCaption.Size = new System.Drawing.Size(100, 18);
            this.lblTotalSessionsCaption.TabIndex = 0;
            this.lblTotalSessionsCaption.Text = "Total Sessions:";
            // 
            // ultraGroupBoxFilters
            // 
            this.ultraGroupBoxFilters.Controls.Add(this.btnClose);
            this.ultraGroupBoxFilters.Controls.Add(this.btnPrint);
            this.ultraGroupBoxFilters.Controls.Add(this.btnExport);
            this.ultraGroupBoxFilters.Controls.Add(this.btnClear);
            this.ultraGroupBoxFilters.Controls.Add(this.btnSearch);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraComboPresetDates);
            this.ultraGroupBoxFilters.Controls.Add(this.lblQuickDate);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraComboUser);
            this.ultraGroupBoxFilters.Controls.Add(this.lblUser);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraComboCounter);
            this.ultraGroupBoxFilters.Controls.Add(this.lblCounter);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraDateTimeTo);
            this.ultraGroupBoxFilters.Controls.Add(this.lblToDate);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraDateTimeFrom);
            this.ultraGroupBoxFilters.Controls.Add(this.lblFromDate);
            this.ultraGroupBoxFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraGroupBoxFilters.Location = new System.Drawing.Point(0, 0);
            this.ultraGroupBoxFilters.Name = "ultraGroupBoxFilters";
            this.ultraGroupBoxFilters.Size = new System.Drawing.Size(1200, 80);
            this.ultraGroupBoxFilters.TabIndex = 0;
            this.ultraGroupBoxFilters.Text = "Filters";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(1108, 25);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(80, 28);
            this.btnClose.TabIndex = 14;
            this.btnClose.Text = "✖ Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(1022, 25);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(80, 28);
            this.btnPrint.TabIndex = 13;
            this.btnPrint.Text = "🖨 Print";
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(936, 25);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(80, 28);
            this.btnExport.TabIndex = 12;
            this.btnExport.Text = "Export";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(850, 25);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(80, 28);
            this.btnClear.TabIndex = 11;
            this.btnClear.Text = "🗑 Clear";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(764, 25);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(80, 28);
            this.btnSearch.TabIndex = 10;
            this.btnSearch.Text = "Search";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // ultraComboPresetDates
            // 
            this.ultraComboPresetDates.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboPresetDates.Location = new System.Drawing.Point(642, 28);
            this.ultraComboPresetDates.Name = "ultraComboPresetDates";
            this.ultraComboPresetDates.Size = new System.Drawing.Size(110, 21);
            this.ultraComboPresetDates.TabIndex = 9;
            this.ultraComboPresetDates.ValueChanged += new System.EventHandler(this.ultraComboPresetDates_ValueChanged);
            // 
            // lblQuickDate
            // 
            this.lblQuickDate.AutoSize = true;
            this.lblQuickDate.Font = new System.Drawing.Font("Segoe UI", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQuickDate.Location = new System.Drawing.Point(595, 30);
            this.lblQuickDate.Name = "lblQuickDate";
            this.lblQuickDate.Size = new System.Drawing.Size(43, 17);
            this.lblQuickDate.TabIndex = 8;
            this.lblQuickDate.Text = "Range";
            // 
            // ultraComboUser
            // 
            this.ultraComboUser.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboUser.Location = new System.Drawing.Point(475, 28);
            this.ultraComboUser.Name = "ultraComboUser";
            this.ultraComboUser.Size = new System.Drawing.Size(110, 21);
            this.ultraComboUser.TabIndex = 7;
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.Font = new System.Drawing.Font("Segoe UI", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUser.Location = new System.Drawing.Point(423, 30);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(51, 17);
            this.lblUser.TabIndex = 6;
            this.lblUser.Text = "Cashier";
            // 
            // ultraComboCounter
            // 
            this.ultraComboCounter.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboCounter.Location = new System.Drawing.Point(308, 28);
            this.ultraComboCounter.Name = "ultraComboCounter";
            this.ultraComboCounter.Size = new System.Drawing.Size(110, 21);
            this.ultraComboCounter.TabIndex = 5;
            // 
            // lblCounter
            // 
            this.lblCounter.AutoSize = true;
            this.lblCounter.Font = new System.Drawing.Font("Segoe UI", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCounter.Location = new System.Drawing.Point(252, 30);
            this.lblCounter.Name = "lblCounter";
            this.lblCounter.Size = new System.Drawing.Size(54, 17);
            this.lblCounter.TabIndex = 4;
            this.lblCounter.Text = "Counter";
            // 
            // ultraDateTimeTo
            // 
            this.ultraDateTimeTo.Location = new System.Drawing.Point(162, 28);
            this.ultraDateTimeTo.Name = "ultraDateTimeTo";
            this.ultraDateTimeTo.Size = new System.Drawing.Size(85, 21);
            this.ultraDateTimeTo.TabIndex = 3;
            // 
            // lblToDate
            // 
            this.lblToDate.AutoSize = true;
            this.lblToDate.Font = new System.Drawing.Font("Segoe UI", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToDate.Location = new System.Drawing.Point(138, 30);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(22, 17);
            this.lblToDate.TabIndex = 2;
            this.lblToDate.Text = "To";
            // 
            // ultraDateTimeFrom
            // 
            this.ultraDateTimeFrom.Location = new System.Drawing.Point(48, 28);
            this.ultraDateTimeFrom.Name = "ultraDateTimeFrom";
            this.ultraDateTimeFrom.Size = new System.Drawing.Size(85, 21);
            this.ultraDateTimeFrom.TabIndex = 1;
            // 
            // lblFromDate
            // 
            this.lblFromDate.AutoSize = true;
            this.lblFromDate.Font = new System.Drawing.Font("Segoe UI", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFromDate.Location = new System.Drawing.Point(6, 30);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(38, 17);
            this.lblFromDate.TabIndex = 0;
            this.lblFromDate.Text = "From";
            // 
            // frmCounterReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 600);
            this.Controls.Add(this.ultraPanelMain);
            this.Name = "frmCounterReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Counter Session Closing Report";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmCounterReport_Load);
            this.ultraPanelMain.ClientArea.ResumeLayout(false);
            this.ultraPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxGrid)).EndInit();
            this.ultraGroupBoxGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridCounterReport)).EndInit();
            this.ultraPanelSummary.ClientArea.ResumeLayout(false);
            this.ultraPanelSummary.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFilters)).EndInit();
            this.ultraGroupBoxFilters.ResumeLayout(false);
            this.ultraGroupBoxFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPresetDates)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboUser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboCounter)).EndInit();
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
        private System.Windows.Forms.Label lblCounter;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboCounter;
        private System.Windows.Forms.Label lblUser;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboUser;
        private Infragistics.Win.Misc.UltraButton btnSearch;
        private Infragistics.Win.Misc.UltraButton btnClear;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridCounterReport;
        private Infragistics.Win.Misc.UltraPanel ultraPanelSummary;
        private Infragistics.Win.Misc.UltraLabel lblTotalSessionsCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalSessionsValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalNetSalesCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalNetSalesValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalCollectionCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalCollectionValue;
        private Infragistics.Win.Misc.UltraLabel lblDifferenceCaption;
        private Infragistics.Win.Misc.UltraLabel lblDifferenceValue;
        private System.Windows.Forms.Label lblQuickDate;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboPresetDates;
    }
}
