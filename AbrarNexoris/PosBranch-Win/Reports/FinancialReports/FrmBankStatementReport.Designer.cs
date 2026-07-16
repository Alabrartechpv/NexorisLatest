namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmBankStatementReport
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
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance6 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance7 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance8 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance9 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance10 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance11 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance12 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance13 = new Infragistics.Win.Appearance();
            Infragistics.Win.ValueListItem valueListItem1 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem2 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem3 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem4 = new Infragistics.Win.ValueListItem();
            this.panelMain = new Infragistics.Win.Misc.UltraPanel();
            this.panelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridTransactions = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelGridHeader = new Infragistics.Win.Misc.UltraPanel();
            this.lblPeriod = new Infragistics.Win.Misc.UltraLabel();
            this.lblRecordCount = new Infragistics.Win.Misc.UltraLabel();
            this.panelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.tableLayoutSummary = new System.Windows.Forms.TableLayoutPanel();
            this.panelMoneyIn = new Infragistics.Win.Misc.UltraPanel();
            this.lblTotalMoneyInValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalMoneyInTitle = new Infragistics.Win.Misc.UltraLabel();
            this.panelMoneyOut = new Infragistics.Win.Misc.UltraPanel();
            this.lblTotalMoneyOutValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalMoneyOutTitle = new Infragistics.Win.Misc.UltraLabel();
            this.panelNetAmount = new Infragistics.Win.Misc.UltraPanel();
            this.lblNetAmountValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblNetAmountTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblBreakdown = new Infragistics.Win.Misc.UltraLabel();
            this.panelHeader = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGroupBox1 = new Infragistics.Win.Misc.UltraGroupBox();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblReportScope = new Infragistics.Win.Misc.UltraLabel();
            this.ultraLabel3 = new Infragistics.Win.Misc.UltraLabel();
            this.cmbDateQuickSelect = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnExportCsv = new Infragistics.Win.Misc.UltraButton();
            this.btnGenerate = new Infragistics.Win.Misc.UltraButton();
            this.lblPaymentMethod = new Infragistics.Win.Misc.UltraLabel();
            this.cmbPaymentMethod = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.ultraLabel2 = new Infragistics.Win.Misc.UltraLabel();
            this.ultraLabel1 = new Infragistics.Win.Misc.UltraLabel();
            this.dtToDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.dtFromDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.panelMain.ClientArea.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.panelGrid.ClientArea.SuspendLayout();
            this.panelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTransactions)).BeginInit();
            this.panelGridHeader.ClientArea.SuspendLayout();
            this.panelGridHeader.SuspendLayout();
            this.panelSummary.ClientArea.SuspendLayout();
            this.panelSummary.SuspendLayout();
            this.tableLayoutSummary.SuspendLayout();
            this.panelMoneyIn.ClientArea.SuspendLayout();
            this.panelMoneyIn.SuspendLayout();
            this.panelMoneyOut.ClientArea.SuspendLayout();
            this.panelMoneyOut.SuspendLayout();
            this.panelNetAmount.ClientArea.SuspendLayout();
            this.panelNetAmount.SuspendLayout();
            this.panelHeader.ClientArea.SuspendLayout();
            this.panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBox1)).BeginInit();
            this.ultraGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDateQuickSelect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymentMethod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).BeginInit();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            // 
            // panelMain.ClientArea
            // 
            this.panelMain.ClientArea.Controls.Add(this.panelGrid);
            this.panelMain.ClientArea.Controls.Add(this.panelSummary);
            this.panelMain.ClientArea.Controls.Add(this.panelHeader);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1280, 700);
            this.panelMain.TabIndex = 0;
            // 
            // panelGrid
            // 
            // 
            // panelGrid.ClientArea
            // 
            this.panelGrid.ClientArea.Controls.Add(this.ultraGridTransactions);
            this.panelGrid.ClientArea.Controls.Add(this.panelGridHeader);
            this.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGrid.Location = new System.Drawing.Point(0, 218);
            this.panelGrid.Name = "panelGrid";
            this.panelGrid.Padding = new System.Windows.Forms.Padding(5);
            this.panelGrid.Size = new System.Drawing.Size(1280, 482);
            this.panelGrid.TabIndex = 2;
            // 
            // ultraGridTransactions
            // 
            this.ultraGridTransactions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridTransactions.Location = new System.Drawing.Point(0, 36);
            this.ultraGridTransactions.Name = "ultraGridTransactions";
            this.ultraGridTransactions.Size = new System.Drawing.Size(1280, 446);
            this.ultraGridTransactions.TabIndex = 0;
            // 
            // panelGridHeader
            // 
            appearance1.BackColor = System.Drawing.Color.White;
            appearance1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.panelGridHeader.Appearance = appearance1;
            this.panelGridHeader.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // panelGridHeader.ClientArea
            // 
            this.panelGridHeader.ClientArea.Controls.Add(this.lblPeriod);
            this.panelGridHeader.ClientArea.Controls.Add(this.lblRecordCount);
            this.panelGridHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelGridHeader.Location = new System.Drawing.Point(0, 0);
            this.panelGridHeader.Name = "panelGridHeader";
            this.panelGridHeader.Size = new System.Drawing.Size(1280, 36);
            this.panelGridHeader.TabIndex = 1;
            // 
            // lblPeriod
            // 
            appearance2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            appearance2.TextVAlignAsString = "Middle";
            this.lblPeriod.Appearance = appearance2;
            this.lblPeriod.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblPeriod.Location = new System.Drawing.Point(0, 0);
            this.lblPeriod.Name = "lblPeriod";
            this.lblPeriod.Size = new System.Drawing.Size(330, 34);
            this.lblPeriod.TabIndex = 0;
            this.lblPeriod.Text = "Select a period and generate the report";
            // 
            // lblRecordCount
            // 
            appearance4.TextHAlignAsString = "Right";
            appearance4.TextVAlignAsString = "Middle";
            this.lblRecordCount.Appearance = appearance4;
            this.lblRecordCount.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblRecordCount.Location = new System.Drawing.Point(1108, 0);
            this.lblRecordCount.Name = "lblRecordCount";
            this.lblRecordCount.Size = new System.Drawing.Size(170, 34);
            this.lblRecordCount.TabIndex = 2;
            this.lblRecordCount.Text = "Showing 0 of 0";
            // 
            // panelSummary
            // 
            // 
            // panelSummary.ClientArea
            // 
            this.panelSummary.ClientArea.Controls.Add(this.tableLayoutSummary);
            this.panelSummary.ClientArea.Controls.Add(this.lblBreakdown);
            this.panelSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSummary.Location = new System.Drawing.Point(0, 108);
            this.panelSummary.Name = "panelSummary";
            this.panelSummary.Size = new System.Drawing.Size(1280, 110);
            this.panelSummary.TabIndex = 1;
            // 
            // tableLayoutSummary
            // 
            this.tableLayoutSummary.ColumnCount = 3;
            this.tableLayoutSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tableLayoutSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tableLayoutSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.tableLayoutSummary.Controls.Add(this.panelMoneyIn, 0, 0);
            this.tableLayoutSummary.Controls.Add(this.panelMoneyOut, 1, 0);
            this.tableLayoutSummary.Controls.Add(this.panelNetAmount, 2, 0);
            this.tableLayoutSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutSummary.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutSummary.Name = "tableLayoutSummary";
            this.tableLayoutSummary.Padding = new System.Windows.Forms.Padding(5);
            this.tableLayoutSummary.RowCount = 1;
            this.tableLayoutSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutSummary.Size = new System.Drawing.Size(1280, 70);
            this.tableLayoutSummary.TabIndex = 0;
            // 
            // panelMoneyIn
            // 
            // 
            // panelMoneyIn.ClientArea
            // 
            this.panelMoneyIn.ClientArea.Controls.Add(this.lblTotalMoneyInValue);
            this.panelMoneyIn.ClientArea.Controls.Add(this.lblTotalMoneyInTitle);
            this.panelMoneyIn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMoneyIn.Location = new System.Drawing.Point(8, 8);
            this.panelMoneyIn.Name = "panelMoneyIn";
            this.panelMoneyIn.Size = new System.Drawing.Size(417, 54);
            this.panelMoneyIn.TabIndex = 0;
            // 
            // lblTotalMoneyInValue
            // 
            appearance5.BackColor = System.Drawing.Color.Transparent;
            appearance5.TextHAlignAsString = "Right";
            appearance5.TextVAlignAsString = "Middle";
            this.lblTotalMoneyInValue.Appearance = appearance5;
            this.lblTotalMoneyInValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalMoneyInValue.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTotalMoneyInValue.Location = new System.Drawing.Point(140, 0);
            this.lblTotalMoneyInValue.Name = "lblTotalMoneyInValue";
            this.lblTotalMoneyInValue.Size = new System.Drawing.Size(277, 54);
            this.lblTotalMoneyInValue.TabIndex = 0;
            this.lblTotalMoneyInValue.Text = "0.00";
            // 
            // lblTotalMoneyInTitle
            // 
            appearance6.BackColor = System.Drawing.Color.Transparent;
            appearance6.TextVAlignAsString = "Middle";
            this.lblTotalMoneyInTitle.Appearance = appearance6;
            this.lblTotalMoneyInTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTotalMoneyInTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.lblTotalMoneyInTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTotalMoneyInTitle.Name = "lblTotalMoneyInTitle";
            this.lblTotalMoneyInTitle.Size = new System.Drawing.Size(140, 54);
            this.lblTotalMoneyInTitle.TabIndex = 1;
            this.lblTotalMoneyInTitle.Text = "Money In (INR):";
            // 
            // panelMoneyOut
            // 
            // 
            // panelMoneyOut.ClientArea
            // 
            this.panelMoneyOut.ClientArea.Controls.Add(this.lblTotalMoneyOutValue);
            this.panelMoneyOut.ClientArea.Controls.Add(this.lblTotalMoneyOutTitle);
            this.panelMoneyOut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMoneyOut.Location = new System.Drawing.Point(431, 8);
            this.panelMoneyOut.Name = "panelMoneyOut";
            this.panelMoneyOut.Size = new System.Drawing.Size(417, 54);
            this.panelMoneyOut.TabIndex = 1;
            // 
            // lblTotalMoneyOutValue
            // 
            appearance7.BackColor = System.Drawing.Color.Transparent;
            appearance7.TextHAlignAsString = "Right";
            appearance7.TextVAlignAsString = "Middle";
            this.lblTotalMoneyOutValue.Appearance = appearance7;
            this.lblTotalMoneyOutValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalMoneyOutValue.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTotalMoneyOutValue.Location = new System.Drawing.Point(140, 0);
            this.lblTotalMoneyOutValue.Name = "lblTotalMoneyOutValue";
            this.lblTotalMoneyOutValue.Size = new System.Drawing.Size(277, 54);
            this.lblTotalMoneyOutValue.TabIndex = 0;
            this.lblTotalMoneyOutValue.Text = "0.00";
            // 
            // lblTotalMoneyOutTitle
            // 
            appearance8.BackColor = System.Drawing.Color.Transparent;
            appearance8.TextVAlignAsString = "Middle";
            this.lblTotalMoneyOutTitle.Appearance = appearance8;
            this.lblTotalMoneyOutTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTotalMoneyOutTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.lblTotalMoneyOutTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTotalMoneyOutTitle.Name = "lblTotalMoneyOutTitle";
            this.lblTotalMoneyOutTitle.Size = new System.Drawing.Size(140, 54);
            this.lblTotalMoneyOutTitle.TabIndex = 1;
            this.lblTotalMoneyOutTitle.Text = "Money Out (INR):";
            // 
            // panelNetAmount
            // 
            // 
            // panelNetAmount.ClientArea
            // 
            this.panelNetAmount.ClientArea.Controls.Add(this.lblNetAmountValue);
            this.panelNetAmount.ClientArea.Controls.Add(this.lblNetAmountTitle);
            this.panelNetAmount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelNetAmount.Location = new System.Drawing.Point(854, 8);
            this.panelNetAmount.Name = "panelNetAmount";
            this.panelNetAmount.Size = new System.Drawing.Size(418, 54);
            this.panelNetAmount.TabIndex = 2;
            // 
            // lblNetAmountValue
            // 
            appearance9.BackColor = System.Drawing.Color.Transparent;
            appearance9.TextHAlignAsString = "Right";
            appearance9.TextVAlignAsString = "Middle";
            this.lblNetAmountValue.Appearance = appearance9;
            this.lblNetAmountValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNetAmountValue.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblNetAmountValue.Location = new System.Drawing.Point(165, 0);
            this.lblNetAmountValue.Name = "lblNetAmountValue";
            this.lblNetAmountValue.Size = new System.Drawing.Size(253, 54);
            this.lblNetAmountValue.TabIndex = 0;
            this.lblNetAmountValue.Text = "0.00";
            // 
            // lblNetAmountTitle
            // 
            appearance10.BackColor = System.Drawing.Color.Transparent;
            appearance10.TextVAlignAsString = "Middle";
            this.lblNetAmountTitle.Appearance = appearance10;
            this.lblNetAmountTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblNetAmountTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.lblNetAmountTitle.Location = new System.Drawing.Point(0, 0);
            this.lblNetAmountTitle.Name = "lblNetAmountTitle";
            this.lblNetAmountTitle.Size = new System.Drawing.Size(165, 54);
            this.lblNetAmountTitle.TabIndex = 1;
            this.lblNetAmountTitle.Text = "Net Bank Movement:";
            // 
            // lblBreakdown
            // 
            appearance11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            appearance11.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(210)))), ((int)(((byte)(220)))));
            appearance11.TextHAlignAsString = "Center";
            appearance11.TextVAlignAsString = "Middle";
            this.lblBreakdown.Appearance = appearance11;
            this.lblBreakdown.BorderStyleOuter = Infragistics.Win.UIElementBorderStyle.Solid;
            this.lblBreakdown.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblBreakdown.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblBreakdown.Location = new System.Drawing.Point(0, 70);
            this.lblBreakdown.Name = "lblBreakdown";
            this.lblBreakdown.Size = new System.Drawing.Size(1280, 40);
            this.lblBreakdown.TabIndex = 1;
            this.lblBreakdown.Text = "Payment-mode breakdown will appear after the report is generated.";
            // 
            // panelHeader
            // 
            // 
            // panelHeader.ClientArea
            // 
            this.panelHeader.ClientArea.Controls.Add(this.ultraGroupBox1);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(1280, 108);
            this.panelHeader.TabIndex = 0;
            // 
            // ultraGroupBox1
            // 
            appearance12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            appearance12.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.ultraGroupBox1.Appearance = appearance12;
            this.ultraGroupBox1.Controls.Add(this.lblSearch);
            this.ultraGroupBox1.Controls.Add(this.txtSearch);
            this.ultraGroupBox1.Controls.Add(this.lblReportScope);
            this.ultraGroupBox1.Controls.Add(this.ultraLabel3);
            this.ultraGroupBox1.Controls.Add(this.cmbDateQuickSelect);
            this.ultraGroupBox1.Controls.Add(this.btnClose);
            this.ultraGroupBox1.Controls.Add(this.btnPrint);
            this.ultraGroupBox1.Controls.Add(this.btnExportCsv);
            this.ultraGroupBox1.Controls.Add(this.btnGenerate);
            this.ultraGroupBox1.Controls.Add(this.lblPaymentMethod);
            this.ultraGroupBox1.Controls.Add(this.cmbPaymentMethod);
            this.ultraGroupBox1.Controls.Add(this.ultraLabel2);
            this.ultraGroupBox1.Controls.Add(this.ultraLabel1);
            this.ultraGroupBox1.Controls.Add(this.dtToDate);
            this.ultraGroupBox1.Controls.Add(this.dtFromDate);
            this.ultraGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.ultraGroupBox1.Name = "ultraGroupBox1";
            this.ultraGroupBox1.Size = new System.Drawing.Size(1280, 108);
            this.ultraGroupBox1.TabIndex = 0;
            this.ultraGroupBox1.Text = "Bank Transaction Reconciliation";
            // 
            // lblSearch
            // 
            this.lblSearch.Location = new System.Drawing.Point(15, 67);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(45, 23);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(60, 63);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.NullText = "Party, voucher, reference, amount...";
            this.txtSearch.Size = new System.Drawing.Size(260, 25);
            this.txtSearch.TabIndex = 4;
            // 
            // lblReportScope
            // 
            appearance13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblReportScope.Appearance = appearance13;
            this.lblReportScope.Location = new System.Drawing.Point(340, 65);
            this.lblReportScope.Name = "lblReportScope";
            this.lblReportScope.Size = new System.Drawing.Size(500, 23);
            this.lblReportScope.TabIndex = 6;
            this.lblReportScope.Text = "Tip: use the Reference column to match bank entries.";
            // 
            // ultraLabel3
            // 
            this.ultraLabel3.Location = new System.Drawing.Point(15, 28);
            this.ultraLabel3.Name = "ultraLabel3";
            this.ultraLabel3.Size = new System.Drawing.Size(45, 23);
            this.ultraLabel3.TabIndex = 7;
            this.ultraLabel3.Text = "Quick:";
            // 
            // cmbDateQuickSelect
            // 
            this.cmbDateQuickSelect.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            valueListItem1.DataValue = "Today";
            valueListItem1.DisplayText = "Today";
            valueListItem2.DataValue = "This Month";
            valueListItem2.DisplayText = "This Month";
            valueListItem3.DataValue = "Last Month";
            valueListItem3.DisplayText = "Last Month";
            valueListItem4.DataValue = "This Financial Year";
            valueListItem4.DisplayText = "This Financial Year";
            this.cmbDateQuickSelect.Items.AddRange(new Infragistics.Win.ValueListItem[] {
            valueListItem1,
            valueListItem2,
            valueListItem3,
            valueListItem4});
            this.cmbDateQuickSelect.Location = new System.Drawing.Point(60, 24);
            this.cmbDateQuickSelect.Name = "cmbDateQuickSelect";
            this.cmbDateQuickSelect.Size = new System.Drawing.Size(130, 25);
            this.cmbDateQuickSelect.TabIndex = 8;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(1190, 20);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(70, 33);
            this.btnClose.TabIndex = 9;
            this.btnClose.Text = "Close";
            // 
            // btnPrint
            // 
            this.btnPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPrint.Location = new System.Drawing.Point(1110, 20);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(70, 33);
            this.btnPrint.TabIndex = 10;
            this.btnPrint.Text = "Print";
            // 
            // btnExportCsv
            // 
            this.btnExportCsv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportCsv.Location = new System.Drawing.Point(1015, 20);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(85, 33);
            this.btnExportCsv.TabIndex = 11;
            this.btnExportCsv.Text = "Export CSV";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Location = new System.Drawing.Point(905, 20);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(100, 33);
            this.btnGenerate.TabIndex = 12;
            this.btnGenerate.Text = "Generate (F5)";
            // 
            // lblPaymentMethod
            // 
            this.lblPaymentMethod.Location = new System.Drawing.Point(535, 28);
            this.lblPaymentMethod.Name = "lblPaymentMethod";
            this.lblPaymentMethod.Size = new System.Drawing.Size(65, 23);
            this.lblPaymentMethod.TabIndex = 13;
            this.lblPaymentMethod.Text = "Pay Mode:";
            // 
            // cmbPaymentMethod
            // 
            this.cmbPaymentMethod.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbPaymentMethod.Location = new System.Drawing.Point(605, 24);
            this.cmbPaymentMethod.Name = "cmbPaymentMethod";
            this.cmbPaymentMethod.Size = new System.Drawing.Size(130, 25);
            this.cmbPaymentMethod.TabIndex = 14;
            // 
            // ultraLabel2
            // 
            this.ultraLabel2.Location = new System.Drawing.Point(375, 28);
            this.ultraLabel2.Name = "ultraLabel2";
            this.ultraLabel2.Size = new System.Drawing.Size(25, 23);
            this.ultraLabel2.TabIndex = 15;
            this.ultraLabel2.Text = "To:";
            // 
            // ultraLabel1
            // 
            this.ultraLabel1.Location = new System.Drawing.Point(205, 28);
            this.ultraLabel1.Name = "ultraLabel1";
            this.ultraLabel1.Size = new System.Drawing.Size(40, 23);
            this.ultraLabel1.TabIndex = 16;
            this.ultraLabel1.Text = "From:";
            // 
            // dtToDate
            // 
            this.dtToDate.Location = new System.Drawing.Point(400, 24);
            this.dtToDate.Name = "dtToDate";
            this.dtToDate.Size = new System.Drawing.Size(120, 25);
            this.dtToDate.TabIndex = 17;
            // 
            // dtFromDate
            // 
            this.dtFromDate.Location = new System.Drawing.Point(245, 24);
            this.dtFromDate.Name = "dtFromDate";
            this.dtFromDate.Size = new System.Drawing.Size(120, 25);
            this.dtFromDate.TabIndex = 18;
            // 
            // FrmBankStatementReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 700);
            this.Controls.Add(this.panelMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(1200, 620);
            this.Name = "FrmBankStatementReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bank Statement Report";
            this.panelMain.ClientArea.ResumeLayout(false);
            this.panelMain.ResumeLayout(false);
            this.panelGrid.ClientArea.ResumeLayout(false);
            this.panelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTransactions)).EndInit();
            this.panelGridHeader.ClientArea.ResumeLayout(false);
            this.panelGridHeader.ResumeLayout(false);
            this.panelSummary.ClientArea.ResumeLayout(false);
            this.panelSummary.ResumeLayout(false);
            this.tableLayoutSummary.ResumeLayout(false);
            this.panelMoneyIn.ClientArea.ResumeLayout(false);
            this.panelMoneyIn.ResumeLayout(false);
            this.panelMoneyOut.ClientArea.ResumeLayout(false);
            this.panelMoneyOut.ResumeLayout(false);
            this.panelNetAmount.ClientArea.ResumeLayout(false);
            this.panelNetAmount.ResumeLayout(false);
            this.panelHeader.ClientArea.ResumeLayout(false);
            this.panelHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBox1)).EndInit();
            this.ultraGroupBox1.ResumeLayout(false);
            this.ultraGroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDateQuickSelect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymentMethod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel panelMain;
        private Infragistics.Win.Misc.UltraPanel panelHeader;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBox1;
        private Infragistics.Win.Misc.UltraLabel ultraLabel3;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbDateQuickSelect;
        private Infragistics.Win.Misc.UltraLabel ultraLabel1;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFromDate;
        private Infragistics.Win.Misc.UltraLabel ultraLabel2;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtToDate;
        private Infragistics.Win.Misc.UltraButton btnGenerate;
        private Infragistics.Win.Misc.UltraButton btnExportCsv;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnClose;

        private Infragistics.Win.Misc.UltraPanel panelSummary;
        private System.Windows.Forms.TableLayoutPanel tableLayoutSummary;
        private Infragistics.Win.Misc.UltraPanel panelMoneyIn;
        private Infragistics.Win.Misc.UltraLabel lblTotalMoneyInTitle;
        private Infragistics.Win.Misc.UltraLabel lblTotalMoneyInValue;

        private Infragistics.Win.Misc.UltraPanel panelMoneyOut;
        private Infragistics.Win.Misc.UltraLabel lblTotalMoneyOutTitle;
        private Infragistics.Win.Misc.UltraLabel lblTotalMoneyOutValue;

        private Infragistics.Win.Misc.UltraPanel panelNetAmount;
        private Infragistics.Win.Misc.UltraLabel lblNetAmountTitle;
        private Infragistics.Win.Misc.UltraLabel lblNetAmountValue;

        private Infragistics.Win.Misc.UltraPanel panelGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridTransactions;
        private Infragistics.Win.Misc.UltraLabel lblBreakdown;
        private Infragistics.Win.Misc.UltraLabel lblPaymentMethod;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbPaymentMethod;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblReportScope;
        private Infragistics.Win.Misc.UltraPanel panelGridHeader;
        private Infragistics.Win.Misc.UltraLabel lblPeriod;
        private Infragistics.Win.Misc.UltraLabel lblRecordCount;
    }
}
