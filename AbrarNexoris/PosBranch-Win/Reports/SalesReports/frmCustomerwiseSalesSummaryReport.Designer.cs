namespace PosBranch_Win.Reports.SalesReports
{
    partial class frmCustomerwiseSalesSummaryReport
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
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance6 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance7 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance8 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance9 = new Infragistics.Win.Appearance();
            this.panelFilters = new Infragistics.Win.Misc.UltraPanel();
            this.lblFrom = new Infragistics.Win.Misc.UltraLabel();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblTo = new Infragistics.Win.Misc.UltraLabel();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblPeriod = new Infragistics.Win.Misc.UltraLabel();
            this.comboPeriod = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblCustomer = new Infragistics.Win.Misc.UltraLabel();
            this.txtCustomerName = new System.Windows.Forms.TextBox();
            this.btnSelectCustomer = new Infragistics.Win.Misc.UltraButton();
            this.btnClearCustomer = new Infragistics.Win.Misc.UltraButton();
            this.lblGroup = new Infragistics.Win.Misc.UltraLabel();
            this.comboGroup = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCategory = new Infragistics.Win.Misc.UltraLabel();
            this.comboCategory = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.btnSearch = new Infragistics.Win.Misc.UltraButton();
            this.btnReset = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.panelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.cardCustCount = new Infragistics.Win.Misc.UltraPanel();
            this.lblCustCountCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblCustCountValue = new Infragistics.Win.Misc.UltraLabel();
            this.cardItemCount = new Infragistics.Win.Misc.UltraPanel();
            this.lblItemCountCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblItemCountValue = new Infragistics.Win.Misc.UltraLabel();
            this.cardTotalQty = new Infragistics.Win.Misc.UltraPanel();
            this.lblTotalQtyCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalQtyValue = new Infragistics.Win.Misc.UltraLabel();
            this.cardTotalSales = new Infragistics.Win.Misc.UltraPanel();
            this.lblTotalSalesCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalSalesValue = new Infragistics.Win.Misc.UltraLabel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelFilters.ClientArea.SuspendLayout();
            this.panelFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboCategory)).BeginInit();
            this.panelGrid.ClientArea.SuspendLayout();
            this.panelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.panelSummary.ClientArea.SuspendLayout();
            this.panelSummary.SuspendLayout();
            this.cardCustCount.ClientArea.SuspendLayout();
            this.cardCustCount.SuspendLayout();
            this.cardItemCount.ClientArea.SuspendLayout();
            this.cardItemCount.SuspendLayout();
            this.cardTotalQty.ClientArea.SuspendLayout();
            this.cardTotalQty.SuspendLayout();
            this.cardTotalSales.ClientArea.SuspendLayout();
            this.cardTotalSales.SuspendLayout();
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
            this.panelFilters.ClientArea.Controls.Add(this.lblFrom);
            this.panelFilters.ClientArea.Controls.Add(this.dtFrom);
            this.panelFilters.ClientArea.Controls.Add(this.lblTo);
            this.panelFilters.ClientArea.Controls.Add(this.dtTo);
            this.panelFilters.ClientArea.Controls.Add(this.lblPeriod);
            this.panelFilters.ClientArea.Controls.Add(this.comboPeriod);
            this.panelFilters.ClientArea.Controls.Add(this.lblSearch);
            this.panelFilters.ClientArea.Controls.Add(this.txtSearch);
            this.panelFilters.ClientArea.Controls.Add(this.lblCustomer);
            this.panelFilters.ClientArea.Controls.Add(this.txtCustomerName);
            this.panelFilters.ClientArea.Controls.Add(this.btnSelectCustomer);
            this.panelFilters.ClientArea.Controls.Add(this.btnClearCustomer);
            this.panelFilters.ClientArea.Controls.Add(this.lblGroup);
            this.panelFilters.ClientArea.Controls.Add(this.comboGroup);
            this.panelFilters.ClientArea.Controls.Add(this.lblCategory);
            this.panelFilters.ClientArea.Controls.Add(this.comboCategory);
            this.panelFilters.ClientArea.Controls.Add(this.btnSearch);
            this.panelFilters.ClientArea.Controls.Add(this.btnReset);
            this.panelFilters.ClientArea.Controls.Add(this.btnExport);
            this.panelFilters.ClientArea.Controls.Add(this.btnPrint);
            this.panelFilters.ClientArea.Controls.Add(this.btnClose);
            this.panelFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilters.Location = new System.Drawing.Point(0, 0);
            this.panelFilters.Name = "panelFilters";
            this.panelFilters.Size = new System.Drawing.Size(1250, 100);
            this.panelFilters.TabIndex = 5;
            this.panelFilters.UseAppStyling = false;
            this.panelFilters.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // lblFrom
            // 
            appearance2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblFrom.Appearance = appearance2;
            this.lblFrom.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFrom.Location = new System.Drawing.Point(15, 20);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(68, 23);
            this.lblFrom.TabIndex = 0;
            this.lblFrom.Text = "From Date:";
            // 
            // dtFrom
            // 
            this.dtFrom.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtFrom.FormatString = "yyyy-MM-dd";
            this.dtFrom.Location = new System.Drawing.Point(88, 16);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(115, 25);
            this.dtFrom.TabIndex = 1;
            // 
            // lblTo
            // 
            appearance3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblTo.Appearance = appearance3;
            this.lblTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTo.Location = new System.Drawing.Point(215, 20);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(58, 23);
            this.lblTo.TabIndex = 2;
            this.lblTo.Text = "To Date:";
            // 
            // dtTo
            // 
            this.dtTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtTo.FormatString = "yyyy-MM-dd";
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
            // lblCustomer
            // 
            appearance6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblCustomer.Appearance = appearance6;
            this.lblCustomer.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCustomer.Location = new System.Drawing.Point(15, 60);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Size = new System.Drawing.Size(68, 23);
            this.lblCustomer.TabIndex = 8;
            this.lblCustomer.Text = "Customer:";
            // 
            // txtCustomerName
            // 
            this.txtCustomerName.BackColor = System.Drawing.Color.White;
            this.txtCustomerName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtCustomerName.Location = new System.Drawing.Point(88, 56);
            this.txtCustomerName.Name = "txtCustomerName";
            this.txtCustomerName.ReadOnly = true;
            this.txtCustomerName.Size = new System.Drawing.Size(130, 23);
            this.txtCustomerName.TabIndex = 9;
            // 
            // btnSelectCustomer
            // 
            this.btnSelectCustomer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSelectCustomer.Location = new System.Drawing.Point(222, 55);
            this.btnSelectCustomer.Name = "btnSelectCustomer";
            this.btnSelectCustomer.Size = new System.Drawing.Size(28, 25);
            this.btnSelectCustomer.TabIndex = 10;
            this.btnSelectCustomer.Text = "...";
            // 
            // btnClearCustomer
            // 
            this.btnClearCustomer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnClearCustomer.Location = new System.Drawing.Point(252, 55);
            this.btnClearCustomer.Name = "btnClearCustomer";
            this.btnClearCustomer.Size = new System.Drawing.Size(24, 25);
            this.btnClearCustomer.TabIndex = 11;
            this.btnClearCustomer.Text = "X";
            // 
            // lblGroup
            // 
            appearance7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblGroup.Appearance = appearance7;
            this.lblGroup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGroup.Location = new System.Drawing.Point(286, 60);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(48, 23);
            this.lblGroup.TabIndex = 12;
            this.lblGroup.Text = "Group:";
            // 
            // comboGroup
            // 
            this.comboGroup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.comboGroup.Location = new System.Drawing.Point(336, 56);
            this.comboGroup.Name = "comboGroup";
            this.comboGroup.Size = new System.Drawing.Size(120, 25);
            this.comboGroup.TabIndex = 13;
            // 
            // lblCategory
            // 
            appearance8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblCategory.Appearance = appearance8;
            this.lblCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCategory.Location = new System.Drawing.Point(466, 60);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(58, 23);
            this.lblCategory.TabIndex = 14;
            this.lblCategory.Text = "Category:";
            // 
            // comboCategory
            // 
            this.comboCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.comboCategory.Location = new System.Drawing.Point(528, 56);
            this.comboCategory.Name = "comboCategory";
            this.comboCategory.Size = new System.Drawing.Size(120, 25);
            this.comboCategory.TabIndex = 15;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(660, 56);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(95, 28);
            this.btnSearch.TabIndex = 16;
            this.btnSearch.Text = "Search [F5]";
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(760, 56);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 28);
            this.btnReset.TabIndex = 17;
            this.btnReset.Text = "Reset";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(840, 56);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(95, 28);
            this.btnExport.TabIndex = 18;
            this.btnExport.Text = "Export (Ctrl+E)";
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(940, 56);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(90, 28);
            this.btnPrint.TabIndex = 19;
            this.btnPrint.Text = "Print (Ctrl+P)";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(1035, 56);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 28);
            this.btnClose.TabIndex = 20;
            this.btnClose.Text = "Close";
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
            this.panelGrid.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.panelGrid.Size = new System.Drawing.Size(1250, 529);
            this.panelGrid.TabIndex = 0;
            // 
            // gridReport
            // 
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Location = new System.Drawing.Point(0, 0);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1250, 529);
            this.gridReport.TabIndex = 0;
            // 
            // panelSummary
            // 
            appearance9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            appearance9.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(226)))), ((int)(((byte)(235)))));
            this.panelSummary.Appearance = appearance9;
            this.panelSummary.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // panelSummary.ClientArea
            // 
            this.panelSummary.ClientArea.Controls.Add(this.cardCustCount);
            this.panelSummary.ClientArea.Controls.Add(this.cardItemCount);
            this.panelSummary.ClientArea.Controls.Add(this.cardTotalQty);
            this.panelSummary.ClientArea.Controls.Add(this.cardTotalSales);
            this.panelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelSummary.Location = new System.Drawing.Point(0, 629);
            this.panelSummary.Name = "panelSummary";
            this.panelSummary.Size = new System.Drawing.Size(1250, 98);
            this.panelSummary.TabIndex = 1;
            this.panelSummary.UseAppStyling = false;
            this.panelSummary.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // cardCustCount
            // 
            // 
            // cardCustCount.ClientArea
            // 
            this.cardCustCount.ClientArea.Controls.Add(this.lblCustCountCaption);
            this.cardCustCount.ClientArea.Controls.Add(this.lblCustCountValue);
            this.cardCustCount.Location = new System.Drawing.Point(0, 0);
            this.cardCustCount.Name = "cardCustCount";
            this.cardCustCount.Size = new System.Drawing.Size(200, 100);
            this.cardCustCount.TabIndex = 0;
            // 
            // lblCustCountCaption
            // 
            this.lblCustCountCaption.Location = new System.Drawing.Point(12, 8);
            this.lblCustCountCaption.Name = "lblCustCountCaption";
            this.lblCustCountCaption.Size = new System.Drawing.Size(210, 15);
            this.lblCustCountCaption.TabIndex = 0;
            // 
            // lblCustCountValue
            // 
            this.lblCustCountValue.Location = new System.Drawing.Point(12, 26);
            this.lblCustCountValue.Name = "lblCustCountValue";
            this.lblCustCountValue.Size = new System.Drawing.Size(210, 28);
            this.lblCustCountValue.TabIndex = 1;
            // 
            // cardItemCount
            // 
            // 
            // cardItemCount.ClientArea
            // 
            this.cardItemCount.ClientArea.Controls.Add(this.lblItemCountCaption);
            this.cardItemCount.ClientArea.Controls.Add(this.lblItemCountValue);
            this.cardItemCount.Location = new System.Drawing.Point(0, 0);
            this.cardItemCount.Name = "cardItemCount";
            this.cardItemCount.Size = new System.Drawing.Size(200, 100);
            this.cardItemCount.TabIndex = 1;
            // 
            // lblItemCountCaption
            // 
            this.lblItemCountCaption.Location = new System.Drawing.Point(12, 8);
            this.lblItemCountCaption.Name = "lblItemCountCaption";
            this.lblItemCountCaption.Size = new System.Drawing.Size(210, 15);
            this.lblItemCountCaption.TabIndex = 0;
            // 
            // lblItemCountValue
            // 
            this.lblItemCountValue.Location = new System.Drawing.Point(12, 26);
            this.lblItemCountValue.Name = "lblItemCountValue";
            this.lblItemCountValue.Size = new System.Drawing.Size(210, 28);
            this.lblItemCountValue.TabIndex = 1;
            // 
            // cardTotalQty
            // 
            // 
            // cardTotalQty.ClientArea
            // 
            this.cardTotalQty.ClientArea.Controls.Add(this.lblTotalQtyCaption);
            this.cardTotalQty.ClientArea.Controls.Add(this.lblTotalQtyValue);
            this.cardTotalQty.Location = new System.Drawing.Point(0, 0);
            this.cardTotalQty.Name = "cardTotalQty";
            this.cardTotalQty.Size = new System.Drawing.Size(200, 100);
            this.cardTotalQty.TabIndex = 2;
            // 
            // lblTotalQtyCaption
            // 
            this.lblTotalQtyCaption.Location = new System.Drawing.Point(12, 8);
            this.lblTotalQtyCaption.Name = "lblTotalQtyCaption";
            this.lblTotalQtyCaption.Size = new System.Drawing.Size(210, 15);
            this.lblTotalQtyCaption.TabIndex = 0;
            // 
            // lblTotalQtyValue
            // 
            this.lblTotalQtyValue.Location = new System.Drawing.Point(12, 26);
            this.lblTotalQtyValue.Name = "lblTotalQtyValue";
            this.lblTotalQtyValue.Size = new System.Drawing.Size(210, 28);
            this.lblTotalQtyValue.TabIndex = 1;
            // 
            // cardTotalSales
            // 
            // 
            // cardTotalSales.ClientArea
            // 
            this.cardTotalSales.ClientArea.Controls.Add(this.lblTotalSalesCaption);
            this.cardTotalSales.ClientArea.Controls.Add(this.lblTotalSalesValue);
            this.cardTotalSales.Location = new System.Drawing.Point(0, 0);
            this.cardTotalSales.Name = "cardTotalSales";
            this.cardTotalSales.Size = new System.Drawing.Size(200, 100);
            this.cardTotalSales.TabIndex = 3;
            // 
            // lblTotalSalesCaption
            // 
            this.lblTotalSalesCaption.Location = new System.Drawing.Point(12, 8);
            this.lblTotalSalesCaption.Name = "lblTotalSalesCaption";
            this.lblTotalSalesCaption.Size = new System.Drawing.Size(210, 15);
            this.lblTotalSalesCaption.TabIndex = 0;
            // 
            // lblTotalSalesValue
            // 
            this.lblTotalSalesValue.Location = new System.Drawing.Point(12, 26);
            this.lblTotalSalesValue.Name = "lblTotalSalesValue";
            this.lblTotalSalesValue.Size = new System.Drawing.Size(210, 28);
            this.lblTotalSalesValue.TabIndex = 1;
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
            this.lblStatus.Size = new System.Drawing.Size(284, 17);
            this.lblStatus.Text = "Ready  |  Select customer filters and press Search (F5)";
            // 
            // frmCustomerwiseSalesSummaryReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1250, 749);
            this.Controls.Add(this.panelGrid);
            this.Controls.Add(this.panelSummary);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.panelFilters);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "frmCustomerwiseSalesSummaryReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Customer-wise Sales Summary Report";
            this.Load += new System.EventHandler(this.FrmCustomerwiseSalesSummaryReport_Load);
            this.panelFilters.ClientArea.ResumeLayout(false);
            this.panelFilters.ClientArea.PerformLayout();
            this.panelFilters.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboCategory)).EndInit();
            this.panelGrid.ClientArea.ResumeLayout(false);
            this.panelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.panelSummary.ClientArea.ResumeLayout(false);
            this.panelSummary.ResumeLayout(false);
            this.cardCustCount.ClientArea.ResumeLayout(false);
            this.cardCustCount.ResumeLayout(false);
            this.cardItemCount.ClientArea.ResumeLayout(false);
            this.cardItemCount.ResumeLayout(false);
            this.cardTotalQty.ClientArea.ResumeLayout(false);
            this.cardTotalQty.ResumeLayout(false);
            this.cardTotalSales.ClientArea.ResumeLayout(false);
            this.cardTotalSales.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Infragistics.Win.Misc.UltraPanel panelFilters;
        private Infragistics.Win.Misc.UltraLabel lblFrom;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.Misc.UltraLabel lblTo;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.Misc.UltraLabel lblPeriod;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboPeriod;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblCustomer;
        private System.Windows.Forms.TextBox txtCustomerName;
        private Infragistics.Win.Misc.UltraButton btnSelectCustomer;
        private Infragistics.Win.Misc.UltraButton btnClearCustomer;
        private Infragistics.Win.Misc.UltraLabel lblGroup;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboGroup;
        private Infragistics.Win.Misc.UltraLabel lblCategory;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboCategory;
        private Infragistics.Win.Misc.UltraButton btnSearch;
        private Infragistics.Win.Misc.UltraButton btnReset;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private Infragistics.Win.Misc.UltraPanel panelGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;
        private Infragistics.Win.Misc.UltraPanel panelSummary;
        
        private Infragistics.Win.Misc.UltraPanel cardCustCount;
        private Infragistics.Win.Misc.UltraLabel lblCustCountCaption;
        private Infragistics.Win.Misc.UltraLabel lblCustCountValue;
        
        private Infragistics.Win.Misc.UltraPanel cardItemCount;
        private Infragistics.Win.Misc.UltraLabel lblItemCountCaption;
        private Infragistics.Win.Misc.UltraLabel lblItemCountValue;
        
        private Infragistics.Win.Misc.UltraPanel cardTotalQty;
        private Infragistics.Win.Misc.UltraLabel lblTotalQtyCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalQtyValue;
        
        private Infragistics.Win.Misc.UltraPanel cardTotalSales;
        private Infragistics.Win.Misc.UltraLabel lblTotalSalesCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalSalesValue;
        
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}
