namespace PosBranch_Win.Reports.SalesReports
{
    partial class frmSalesmanwiseSalesSummaryReport
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
            this.lblFrom = new Infragistics.Win.Misc.UltraLabel();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblTo = new Infragistics.Win.Misc.UltraLabel();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblPeriod = new Infragistics.Win.Misc.UltraLabel();
            this.comboPeriod = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblSalesman = new Infragistics.Win.Misc.UltraLabel();
            this.txtSalesmanName = new System.Windows.Forms.TextBox();
            this.btnSelectSalesman = new Infragistics.Win.Misc.UltraButton();
            this.btnClearSalesman = new Infragistics.Win.Misc.UltraButton();
            this.lblCommission = new Infragistics.Win.Misc.UltraLabel();
            this.numCommissionPercent = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.btnSearch = new Infragistics.Win.Misc.UltraButton();
            this.btnReset = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.panelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.cardSalesmanCount = new Infragistics.Win.Misc.UltraPanel();
            this.cardInvoiceCount = new Infragistics.Win.Misc.UltraPanel();
            this.cardTotalQty = new Infragistics.Win.Misc.UltraPanel();
            this.cardTotalSales = new Infragistics.Win.Misc.UltraPanel();
            this.cardTotalCommission = new Infragistics.Win.Misc.UltraPanel();
            this.lblSalesmanCountCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblSalesmanCountValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblInvoiceCountCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblInvoiceCountValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalQtyCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalQtyValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalSalesCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalSalesValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCommissionCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCommissionValue = new Infragistics.Win.Misc.UltraLabel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            
            this.panelHeader.SuspendLayout();
            this.panelFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCommissionPercent)).BeginInit();
            this.panelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.panelSummary.SuspendLayout();
            this.cardSalesmanCount.SuspendLayout();
            this.cardInvoiceCount.SuspendLayout();
            this.cardTotalQty.SuspendLayout();
            this.cardTotalSales.SuspendLayout();
            this.cardTotalCommission.SuspendLayout();
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
            this.lblTitle.Text = "Salesman-wise Sales Summary";
            // 
            // panelFilters
            // 
            this.panelFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilters.Height = 100;
            this.panelFilters.UseAppStyling = false;
            this.panelFilters.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.panelFilters.Appearance.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.panelFilters.Appearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.panelFilters.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.panelFilters.ClientArea.Controls.Add(this.lblFrom);
            this.panelFilters.ClientArea.Controls.Add(this.dtFrom);
            this.panelFilters.ClientArea.Controls.Add(this.lblTo);
            this.panelFilters.ClientArea.Controls.Add(this.dtTo);
            this.panelFilters.ClientArea.Controls.Add(this.lblPeriod);
            this.panelFilters.ClientArea.Controls.Add(this.comboPeriod);
            this.panelFilters.ClientArea.Controls.Add(this.lblSearch);
            this.panelFilters.ClientArea.Controls.Add(this.txtSearch);
            this.panelFilters.ClientArea.Controls.Add(this.lblSalesman);
            this.panelFilters.ClientArea.Controls.Add(this.txtSalesmanName);
            this.panelFilters.ClientArea.Controls.Add(this.btnSelectSalesman);
            this.panelFilters.ClientArea.Controls.Add(this.btnClearSalesman);
            this.panelFilters.ClientArea.Controls.Add(this.lblCommission);
            this.panelFilters.ClientArea.Controls.Add(this.numCommissionPercent);
            this.panelFilters.ClientArea.Controls.Add(this.btnSearch);
            this.panelFilters.ClientArea.Controls.Add(this.btnReset);
            this.panelFilters.ClientArea.Controls.Add(this.btnExport);
            this.panelFilters.ClientArea.Controls.Add(this.btnPrint);
            this.panelFilters.ClientArea.Controls.Add(this.btnClose);
            this.panelFilters.Name = "panelFilters";
            // 
            // lblFrom
            // 
            this.lblFrom.Location = new System.Drawing.Point(15, 20);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(68, 23);
            this.lblFrom.Text = "From Date:";
            this.lblFrom.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFrom.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // dtFrom
            // 
            this.dtFrom.DateTime = new System.DateTime(2026, 7, 9, 0, 0, 0, 0);
            this.dtFrom.FormatString = "yyyy-MM-dd";
            this.dtFrom.Location = new System.Drawing.Point(88, 16);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(115, 25);
            this.dtFrom.Value = new System.DateTime(2026, 7, 9, 0, 0, 0, 0);
            this.dtFrom.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // lblTo
            // 
            this.lblTo.Location = new System.Drawing.Point(215, 20);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(58, 23);
            this.lblTo.Text = "To Date:";
            this.lblTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTo.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // dtTo
            // 
            this.dtTo.DateTime = new System.DateTime(2026, 7, 9, 0, 0, 0, 0);
            this.dtTo.FormatString = "yyyy-MM-dd";
            this.dtTo.Location = new System.Drawing.Point(278, 16);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(115, 25);
            this.dtTo.Value = new System.DateTime(2026, 7, 9, 0, 0, 0, 0);
            this.dtTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // lblPeriod
            // 
            this.lblPeriod.Location = new System.Drawing.Point(406, 20);
            this.lblPeriod.Name = "lblPeriod";
            this.lblPeriod.Size = new System.Drawing.Size(48, 23);
            this.lblPeriod.Text = "Period:";
            this.lblPeriod.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPeriod.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // comboPeriod
            // 
            this.comboPeriod.Location = new System.Drawing.Point(458, 16);
            this.comboPeriod.Name = "comboPeriod";
            this.comboPeriod.Size = new System.Drawing.Size(125, 25);
            this.comboPeriod.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // lblSearch
            // 
            this.lblSearch.Location = new System.Drawing.Point(597, 20);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(50, 23);
            this.lblSearch.Text = "Search:";
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSearch.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(650, 16);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(210, 25);
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // lblSalesman
            // 
            this.lblSalesman.Location = new System.Drawing.Point(15, 60);
            this.lblSalesman.Name = "lblSalesman";
            this.lblSalesman.Size = new System.Drawing.Size(68, 23);
            this.lblSalesman.Text = "Salesman:";
            this.lblSalesman.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSalesman.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // txtSalesmanName
            // 
            this.txtSalesmanName.BackColor = System.Drawing.Color.White;
            this.txtSalesmanName.Location = new System.Drawing.Point(88, 56);
            this.txtSalesmanName.Name = "txtSalesmanName";
            this.txtSalesmanName.ReadOnly = true;
            this.txtSalesmanName.Size = new System.Drawing.Size(130, 23);
            this.txtSalesmanName.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // btnSelectSalesman
            // 
            this.btnSelectSalesman.Location = new System.Drawing.Point(222, 55);
            this.btnSelectSalesman.Name = "btnSelectSalesman";
            this.btnSelectSalesman.Size = new System.Drawing.Size(28, 25);
            this.btnSelectSalesman.Text = "...";
            this.btnSelectSalesman.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            // 
            // btnClearSalesman
            // 
            this.btnClearSalesman.Location = new System.Drawing.Point(252, 55);
            this.btnClearSalesman.Name = "btnClearSalesman";
            this.btnClearSalesman.Size = new System.Drawing.Size(24, 25);
            this.btnClearSalesman.Text = "X";
            this.btnClearSalesman.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            // 
            // lblCommission
            // 
            this.lblCommission.Location = new System.Drawing.Point(290, 60);
            this.lblCommission.Name = "lblCommission";
            this.lblCommission.Size = new System.Drawing.Size(95, 23);
            this.lblCommission.Text = "Commission Rate %:";
            this.lblCommission.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCommission.Appearance.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            // 
            // numCommissionPercent
            // 
            this.numCommissionPercent.Location = new System.Drawing.Point(390, 56);
            this.numCommissionPercent.Name = "numCommissionPercent";
            this.numCommissionPercent.NumericType = Infragistics.Win.UltraWinEditors.NumericType.Double;
            this.numCommissionPercent.MaskInput = "{double:3.2}";
            this.numCommissionPercent.Value = 5.00D;
            this.numCommissionPercent.Size = new System.Drawing.Size(75, 25);
            this.numCommissionPercent.Font = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(490, 56);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(95, 28);
            this.btnSearch.Text = "Search [F5]";
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(590, 56);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 28);
            this.btnReset.Text = "Reset";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(670, 56);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(95, 28);
            this.btnExport.Text = "Export (Ctrl+E)";
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(770, 56);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(90, 28);
            this.btnPrint.Text = "Print (Ctrl+P)";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(865, 56);
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
            this.gridReport.Size = new System.Drawing.Size(1220, 465);
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
            this.panelSummary.ClientArea.Controls.Add(this.cardSalesmanCount);
            this.panelSummary.ClientArea.Controls.Add(this.cardInvoiceCount);
            this.panelSummary.ClientArea.Controls.Add(this.cardTotalQty);
            this.panelSummary.ClientArea.Controls.Add(this.cardTotalSales);
            this.panelSummary.ClientArea.Controls.Add(this.cardTotalCommission);
            this.panelSummary.Name = "panelSummary";
            // 
            // cardSalesmanCount
            // 
            this.cardSalesmanCount.ClientArea.Controls.Add(this.lblSalesmanCountCaption);
            this.cardSalesmanCount.ClientArea.Controls.Add(this.lblSalesmanCountValue);
            this.cardSalesmanCount.Name = "cardSalesmanCount";
            // 
            // cardInvoiceCount
            // 
            this.cardInvoiceCount.ClientArea.Controls.Add(this.lblInvoiceCountCaption);
            this.cardInvoiceCount.ClientArea.Controls.Add(this.lblInvoiceCountValue);
            this.cardInvoiceCount.Name = "cardInvoiceCount";
            // 
            // cardTotalQty
            // 
            this.cardTotalQty.ClientArea.Controls.Add(this.lblTotalQtyCaption);
            this.cardTotalQty.ClientArea.Controls.Add(this.lblTotalQtyValue);
            this.cardTotalQty.Name = "cardTotalQty";
            // 
            // cardTotalSales
            // 
            this.cardTotalSales.ClientArea.Controls.Add(this.lblTotalSalesCaption);
            this.cardTotalSales.ClientArea.Controls.Add(this.lblTotalSalesValue);
            this.cardTotalSales.Name = "cardTotalSales";
            // 
            // cardTotalCommission
            // 
            this.cardTotalCommission.ClientArea.Controls.Add(this.lblTotalCommissionCaption);
            this.cardTotalCommission.ClientArea.Controls.Add(this.lblTotalCommissionValue);
            this.cardTotalCommission.Name = "cardTotalCommission";
            // 
            // lblSalesmanCountCaption
            // 
            this.lblSalesmanCountCaption.Location = new System.Drawing.Point(12, 8);
            this.lblSalesmanCountCaption.Name = "lblSalesmanCountCaption";
            this.lblSalesmanCountCaption.Size = new System.Drawing.Size(210, 15);
            // 
            // lblSalesmanCountValue
            // 
            this.lblSalesmanCountValue.Location = new System.Drawing.Point(12, 26);
            this.lblSalesmanCountValue.Name = "lblSalesmanCountValue";
            this.lblSalesmanCountValue.Size = new System.Drawing.Size(210, 28);
            // 
            // lblInvoiceCountCaption
            // 
            this.lblInvoiceCountCaption.Location = new System.Drawing.Point(12, 8);
            this.lblInvoiceCountCaption.Name = "lblInvoiceCountCaption";
            this.lblInvoiceCountCaption.Size = new System.Drawing.Size(210, 15);
            // 
            // lblInvoiceCountValue
            // 
            this.lblInvoiceCountValue.Location = new System.Drawing.Point(12, 26);
            this.lblInvoiceCountValue.Name = "lblInvoiceCountValue";
            this.lblInvoiceCountValue.Size = new System.Drawing.Size(210, 28);
            // 
            // lblTotalQtyCaption
            // 
            this.lblTotalQtyCaption.Location = new System.Drawing.Point(12, 8);
            this.lblTotalQtyCaption.Name = "lblTotalQtyCaption";
            this.lblTotalQtyCaption.Size = new System.Drawing.Size(210, 15);
            // 
            // lblTotalQtyValue
            // 
            this.lblTotalQtyValue.Location = new System.Drawing.Point(12, 26);
            this.lblTotalQtyValue.Name = "lblTotalQtyValue";
            this.lblTotalQtyValue.Size = new System.Drawing.Size(210, 28);
            // 
            // lblTotalSalesCaption
            // 
            this.lblTotalSalesCaption.Location = new System.Drawing.Point(12, 8);
            this.lblTotalSalesCaption.Name = "lblTotalSalesCaption";
            this.lblTotalSalesCaption.Size = new System.Drawing.Size(210, 15);
            // 
            // lblTotalSalesValue
            // 
            this.lblTotalSalesValue.Location = new System.Drawing.Point(12, 26);
            this.lblTotalSalesValue.Name = "lblTotalSalesValue";
            this.lblTotalSalesValue.Size = new System.Drawing.Size(210, 28);
            // 
            // lblTotalCommissionCaption
            // 
            this.lblTotalCommissionCaption.Location = new System.Drawing.Point(12, 8);
            this.lblTotalCommissionCaption.Name = "lblTotalCommissionCaption";
            this.lblTotalCommissionCaption.Size = new System.Drawing.Size(210, 15);
            // 
            // lblTotalCommissionValue
            // 
            this.lblTotalCommissionValue.Location = new System.Drawing.Point(12, 26);
            this.lblTotalCommissionValue.Name = "lblTotalCommissionValue";
            this.lblTotalCommissionValue.Size = new System.Drawing.Size(210, 28);
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
            // frmSalesmanwiseSalesSummaryReport
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
            this.Name = "frmSalesmanwiseSalesSummaryReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Salesman-wise Sales Summary Report";
            this.Load += new System.EventHandler(this.FrmSalesmanwiseSalesSummaryReport_Load);
            
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelFilters.ResumeLayout(false);
            this.panelFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCommissionPercent)).EndInit();
            this.panelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.panelSummary.ResumeLayout(false);
            this.cardSalesmanCount.ResumeLayout(false);
            this.cardSalesmanCount.PerformLayout();
            this.cardInvoiceCount.ResumeLayout(false);
            this.cardInvoiceCount.PerformLayout();
            this.cardTotalQty.ResumeLayout(false);
            this.cardTotalQty.PerformLayout();
            this.cardTotalSales.ResumeLayout(false);
            this.cardTotalSales.PerformLayout();
            this.cardTotalCommission.ResumeLayout(false);
            this.cardTotalCommission.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel panelHeader;
        private Infragistics.Win.Misc.UltraLabel lblTitle;
        private Infragistics.Win.Misc.UltraPanel panelFilters;
        private Infragistics.Win.Misc.UltraLabel lblFrom;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.Misc.UltraLabel lblTo;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.Misc.UltraLabel lblPeriod;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboPeriod;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblSalesman;
        private System.Windows.Forms.TextBox txtSalesmanName;
        private Infragistics.Win.Misc.UltraButton btnSelectSalesman;
        private Infragistics.Win.Misc.UltraButton btnClearSalesman;
        private Infragistics.Win.Misc.UltraLabel lblCommission;
        private Infragistics.Win.UltraWinEditors.UltraNumericEditor numCommissionPercent;
        private Infragistics.Win.Misc.UltraButton btnSearch;
        private Infragistics.Win.Misc.UltraButton btnReset;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private Infragistics.Win.Misc.UltraPanel panelGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;
        private Infragistics.Win.Misc.UltraPanel panelSummary;
        
        private Infragistics.Win.Misc.UltraPanel cardSalesmanCount;
        private Infragistics.Win.Misc.UltraLabel lblSalesmanCountCaption;
        private Infragistics.Win.Misc.UltraLabel lblSalesmanCountValue;
        
        private Infragistics.Win.Misc.UltraPanel cardInvoiceCount;
        private Infragistics.Win.Misc.UltraLabel lblInvoiceCountCaption;
        private Infragistics.Win.Misc.UltraLabel lblInvoiceCountValue;
        
        private Infragistics.Win.Misc.UltraPanel cardTotalQty;
        private Infragistics.Win.Misc.UltraLabel lblTotalQtyCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalQtyValue;
        
        private Infragistics.Win.Misc.UltraPanel cardTotalSales;
        private Infragistics.Win.Misc.UltraLabel lblTotalSalesCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalSalesValue;
        
        private Infragistics.Win.Misc.UltraPanel cardTotalCommission;
        private Infragistics.Win.Misc.UltraLabel lblTotalCommissionCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalCommissionValue;
        
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}
