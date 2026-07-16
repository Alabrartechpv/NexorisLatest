
namespace PosBranch_Win.Reports.SalesReports
{
    partial class frmSalesProfit
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
            this.ultraGridProfit = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.lblProfitPercentValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblProfitPercentCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalProfitValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalProfitCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalAmountValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalAmountCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalBillsValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalBillsCaption = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxFilters = new Infragistics.Win.Misc.UltraGroupBox();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnClear = new Infragistics.Win.Misc.UltraButton();
            this.btnSearch = new Infragistics.Win.Misc.UltraButton();
            this.ultraComboPresetDates = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblQuickDate = new System.Windows.Forms.Label();
            this.ultraNumericBillNo = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.lblBillNo = new System.Windows.Forms.Label();
            this.ultraDateTimeTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new System.Windows.Forms.Label();
            this.ultraDateTimeFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblFromDate = new System.Windows.Forms.Label();
            this.ultraPanelMain.ClientArea.SuspendLayout();
            this.ultraPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxGrid)).BeginInit();
            this.ultraGroupBoxGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridProfit)).BeginInit();
            this.ultraPanelSummary.ClientArea.SuspendLayout();
            this.ultraPanelSummary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFilters)).BeginInit();
            this.ultraGroupBoxFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPresetDates)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraNumericBillNo)).BeginInit();
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
            this.ultraPanelMain.Size = new System.Drawing.Size(1100, 600);
            this.ultraPanelMain.TabIndex = 0;
            // 
            // ultraGroupBoxGrid
            // 
            this.ultraGroupBoxGrid.Controls.Add(this.ultraGridProfit);
            this.ultraGroupBoxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGroupBoxGrid.Location = new System.Drawing.Point(0, 80);
            this.ultraGroupBoxGrid.Name = "ultraGroupBoxGrid";
            this.ultraGroupBoxGrid.Size = new System.Drawing.Size(1100, 450);
            this.ultraGroupBoxGrid.TabIndex = 1;
            this.ultraGroupBoxGrid.Text = "Sales Profit Report";
            // 
            // ultraGridProfit
            // 
            this.ultraGridProfit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridProfit.Location = new System.Drawing.Point(3, 16);
            this.ultraGridProfit.Name = "ultraGridProfit";
            this.ultraGridProfit.Size = new System.Drawing.Size(1094, 431);
            this.ultraGridProfit.TabIndex = 0;
            this.ultraGridProfit.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.ultraGridProfit.InitializeLayout += new Infragistics.Win.UltraWinGrid.InitializeLayoutEventHandler(this.ultraGridProfit_InitializeLayout);
            // 
            // ultraPanelSummary
            // 
            // 
            // ultraPanelSummary.ClientArea
            // 
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblProfitPercentValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblProfitPercentCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalProfitValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalProfitCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalAmountValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalAmountCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalBillsValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalBillsCaption);
            this.ultraPanelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelSummary.Location = new System.Drawing.Point(0, 530);
            this.ultraPanelSummary.Name = "ultraPanelSummary";
            this.ultraPanelSummary.Size = new System.Drawing.Size(1100, 70);
            this.ultraPanelSummary.TabIndex = 2;
            // 
            // lblProfitPercentValue
            // 
            this.lblProfitPercentValue.Location = new System.Drawing.Point(700, 32);
            this.lblProfitPercentValue.Name = "lblProfitPercentValue";
            this.lblProfitPercentValue.Size = new System.Drawing.Size(120, 25);
            this.lblProfitPercentValue.TabIndex = 7;
            this.lblProfitPercentValue.Text = "0.00 %";
            // 
            // lblProfitPercentCaption
            // 
            this.lblProfitPercentCaption.Location = new System.Drawing.Point(700, 10);
            this.lblProfitPercentCaption.Name = "lblProfitPercentCaption";
            this.lblProfitPercentCaption.Size = new System.Drawing.Size(100, 18);
            this.lblProfitPercentCaption.TabIndex = 6;
            this.lblProfitPercentCaption.Text = "Profit %:";
            // 
            // lblTotalProfitValue
            // 
            this.lblTotalProfitValue.Location = new System.Drawing.Point(450, 32);
            this.lblTotalProfitValue.Name = "lblTotalProfitValue";
            this.lblTotalProfitValue.Size = new System.Drawing.Size(160, 25);
            this.lblTotalProfitValue.TabIndex = 5;
            this.lblTotalProfitValue.Text = "₹ 0.00";
            // 
            // lblTotalProfitCaption
            // 
            this.lblTotalProfitCaption.Location = new System.Drawing.Point(450, 10);
            this.lblTotalProfitCaption.Name = "lblTotalProfitCaption";
            this.lblTotalProfitCaption.Size = new System.Drawing.Size(100, 18);
            this.lblTotalProfitCaption.TabIndex = 4;
            this.lblTotalProfitCaption.Text = "Total Profit:";
            // 
            // lblTotalAmountValue
            // 
            this.lblTotalAmountValue.Location = new System.Drawing.Point(200, 32);
            this.lblTotalAmountValue.Name = "lblTotalAmountValue";
            this.lblTotalAmountValue.Size = new System.Drawing.Size(160, 25);
            this.lblTotalAmountValue.TabIndex = 3;
            this.lblTotalAmountValue.Text = "₹ 0.00";
            // 
            // lblTotalAmountCaption
            // 
            this.lblTotalAmountCaption.Location = new System.Drawing.Point(200, 10);
            this.lblTotalAmountCaption.Name = "lblTotalAmountCaption";
            this.lblTotalAmountCaption.Size = new System.Drawing.Size(100, 18);
            this.lblTotalAmountCaption.TabIndex = 2;
            this.lblTotalAmountCaption.Text = "Total Amount:";
            // 
            // lblTotalBillsValue
            // 
            this.lblTotalBillsValue.Location = new System.Drawing.Point(15, 32);
            this.lblTotalBillsValue.Name = "lblTotalBillsValue";
            this.lblTotalBillsValue.Size = new System.Drawing.Size(100, 25);
            this.lblTotalBillsValue.TabIndex = 1;
            this.lblTotalBillsValue.Text = "0";
            // 
            // lblTotalBillsCaption
            // 
            this.lblTotalBillsCaption.Location = new System.Drawing.Point(15, 10);
            this.lblTotalBillsCaption.Name = "lblTotalBillsCaption";
            this.lblTotalBillsCaption.Size = new System.Drawing.Size(80, 18);
            this.lblTotalBillsCaption.TabIndex = 0;
            this.lblTotalBillsCaption.Text = "Total Bills:";
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
            this.ultraGroupBoxFilters.Controls.Add(this.ultraNumericBillNo);
            this.ultraGroupBoxFilters.Controls.Add(this.lblBillNo);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraDateTimeTo);
            this.ultraGroupBoxFilters.Controls.Add(this.lblToDate);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraDateTimeFrom);
            this.ultraGroupBoxFilters.Controls.Add(this.lblFromDate);
            this.ultraGroupBoxFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraGroupBoxFilters.Location = new System.Drawing.Point(0, 0);
            this.ultraGroupBoxFilters.Name = "ultraGroupBoxFilters";
            this.ultraGroupBoxFilters.Size = new System.Drawing.Size(1100, 80);
            this.ultraGroupBoxFilters.TabIndex = 0;
            this.ultraGroupBoxFilters.Text = "Filters";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(1189, 25);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(85, 28);
            this.btnClose.TabIndex = 10;
            this.btnClose.Text = "✖ Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(1098, 25);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(85, 28);
            this.btnPrint.TabIndex = 9;
            this.btnPrint.Text = "🖨 Print";
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(1007, 25);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(85, 28);
            this.btnExport.TabIndex = 8;
            this.btnExport.Text = "Export";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(916, 25);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(85, 28);
            this.btnClear.TabIndex = 7;
            this.btnClear.Text = "🗑 Clear";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(825, 25);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(85, 28);
            this.btnSearch.TabIndex = 6;
            this.btnSearch.Text = "Search";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // ultraComboPresetDates
            // 
            this.ultraComboPresetDates.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboPresetDates.Location = new System.Drawing.Point(695, 28);
            this.ultraComboPresetDates.Name = "ultraComboPresetDates";
            this.ultraComboPresetDates.Size = new System.Drawing.Size(120, 21);
            this.ultraComboPresetDates.TabIndex = 7;
            this.ultraComboPresetDates.ValueChanged += new System.EventHandler(this.ultraComboPresetDates_ValueChanged);
            // 
            // lblQuickDate
            // 
            this.lblQuickDate.AutoSize = true;
            this.lblQuickDate.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQuickDate.Location = new System.Drawing.Point(620, 30);
            this.lblQuickDate.Name = "lblQuickDate";
            this.lblQuickDate.Size = new System.Drawing.Size(71, 17);
            this.lblQuickDate.TabIndex = 6;
            this.lblQuickDate.Text = "Quick Date";
            // 
            // ultraNumericBillNo
            // 
            this.ultraNumericBillNo.Location = new System.Drawing.Point(505, 28);
            this.ultraNumericBillNo.Name = "ultraNumericBillNo";
            this.ultraNumericBillNo.Size = new System.Drawing.Size(100, 21);
            this.ultraNumericBillNo.TabIndex = 5;
            // 
            // lblBillNo
            // 
            this.lblBillNo.AutoSize = true;
            this.lblBillNo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBillNo.Location = new System.Drawing.Point(455, 30);
            this.lblBillNo.Name = "lblBillNo";
            this.lblBillNo.Size = new System.Drawing.Size(46, 17);
            this.lblBillNo.TabIndex = 4;
            this.lblBillNo.Text = "Bill No";
            // 
            // ultraDateTimeTo
            // 
            this.ultraDateTimeTo.Location = new System.Drawing.Point(298, 28);
            this.ultraDateTimeTo.Name = "ultraDateTimeTo";
            this.ultraDateTimeTo.Size = new System.Drawing.Size(140, 21);
            this.ultraDateTimeTo.TabIndex = 3;
            // 
            // lblToDate
            // 
            this.lblToDate.AutoSize = true;
            this.lblToDate.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToDate.Location = new System.Drawing.Point(240, 30);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(53, 17);
            this.lblToDate.TabIndex = 2;
            this.lblToDate.Text = "To Date";
            // 
            // ultraDateTimeFrom
            // 
            this.ultraDateTimeFrom.Location = new System.Drawing.Point(85, 28);
            this.ultraDateTimeFrom.Name = "ultraDateTimeFrom";
            this.ultraDateTimeFrom.Size = new System.Drawing.Size(140, 21);
            this.ultraDateTimeFrom.TabIndex = 1;
            // 
            // lblFromDate
            // 
            this.lblFromDate.AutoSize = true;
            this.lblFromDate.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFromDate.Location = new System.Drawing.Point(10, 30);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(69, 17);
            this.lblFromDate.TabIndex = 0;
            this.lblFromDate.Text = "From Date";
            // 
            // frmSalesProfit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 600);
            this.Controls.Add(this.ultraPanelMain);
            this.Name = "frmSalesProfit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sales Profit Report";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmSalesProfit_Load);
            this.ultraPanelMain.ClientArea.ResumeLayout(false);
            this.ultraPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxGrid)).EndInit();
            this.ultraGroupBoxGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridProfit)).EndInit();
            this.ultraPanelSummary.ClientArea.ResumeLayout(false);
            this.ultraPanelSummary.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFilters)).EndInit();
            this.ultraGroupBoxFilters.ResumeLayout(false);
            this.ultraGroupBoxFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPresetDates)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraNumericBillNo)).EndInit();
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
        private System.Windows.Forms.Label lblBillNo;
        private Infragistics.Win.UltraWinEditors.UltraNumericEditor ultraNumericBillNo;
        private Infragistics.Win.Misc.UltraButton btnSearch;
        private Infragistics.Win.Misc.UltraButton btnClear;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridProfit;
        private Infragistics.Win.Misc.UltraPanel ultraPanelSummary;
        private Infragistics.Win.Misc.UltraLabel lblTotalBillsCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalBillsValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalAmountCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalAmountValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalProfitCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalProfitValue;
        private Infragistics.Win.Misc.UltraLabel lblProfitPercentCaption;
        private Infragistics.Win.Misc.UltraLabel lblProfitPercentValue;
        private System.Windows.Forms.Label lblQuickDate;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboPresetDates;
    }
}
