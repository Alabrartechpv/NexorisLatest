namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmCashBankBook
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
            this.panelMain = new Infragistics.Win.Misc.UltraPanel();
            this.panelHeader = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGroupBox1 = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraLabel4 = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboLedger = new Infragistics.Win.UltraWinGrid.UltraCombo();
            this.ultraLabel3 = new Infragistics.Win.Misc.UltraLabel();
            this.cmbDateQuickSelect = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnExportCsv = new Infragistics.Win.Misc.UltraButton();
            this.btnGenerate = new Infragistics.Win.Misc.UltraButton();
            this.ultraLabel2 = new Infragistics.Win.Misc.UltraLabel();
            this.ultraLabel1 = new Infragistics.Win.Misc.UltraLabel();
            this.dtToDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.dtFromDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.panelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.panelClosing = new Infragistics.Win.Misc.UltraPanel();
            this.lblClosingBalanceValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblClosingBalanceTitle = new Infragistics.Win.Misc.UltraLabel();
            this.panelPayments = new Infragistics.Win.Misc.UltraPanel();
            this.lblTotalPaymentsValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalPaymentsTitle = new Infragistics.Win.Misc.UltraLabel();
            this.panelReceipts = new Infragistics.Win.Misc.UltraPanel();
            this.lblTotalReceiptsValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalReceiptsTitle = new Infragistics.Win.Misc.UltraLabel();
            this.panelOpening = new Infragistics.Win.Misc.UltraPanel();
            this.lblOpeningBalanceValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblOpeningBalanceTitle = new Infragistics.Win.Misc.UltraLabel();
            this.panelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridTransactions = new Infragistics.Win.UltraWinGrid.UltraGrid();

            // Suspend layout
            this.panelMain.SuspendLayout();
            this.panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBox1)).BeginInit();
            this.ultraGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboLedger)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDateQuickSelect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).BeginInit();
            this.panelSummary.SuspendLayout();
            this.panelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTransactions)).BeginInit();
            this.SuspendLayout();

            // ==========================================
            // panelMain — Dock.Fill, wraps everything
            // ==========================================
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1280, 700);
            this.panelMain.TabIndex = 0;
            this.panelMain.ClientArea.Controls.Add(this.panelGrid);      // Fill (added first = fills remaining)
            this.panelMain.ClientArea.Controls.Add(this.panelSummary);   // Top
            this.panelMain.ClientArea.Controls.Add(this.panelHeader);    // Top (added last = topmost)

            // ==========================================
            // panelHeader — Dock.Top, height 70
            // ==========================================
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(1280, 70);
            this.panelHeader.TabIndex = 0;
            this.panelHeader.ClientArea.Controls.Add(this.ultraGroupBox1);

            // ultraGroupBox1 — Dock.Fill inside header
            this.ultraGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGroupBox1.Name = "ultraGroupBox1";
            this.ultraGroupBox1.Text = "Filter Parameters";
            this.ultraGroupBox1.Controls.Add(this.ultraLabel4);
            this.ultraGroupBox1.Controls.Add(this.ultraComboLedger);
            this.ultraGroupBox1.Controls.Add(this.ultraLabel3);
            this.ultraGroupBox1.Controls.Add(this.cmbDateQuickSelect);
            this.ultraGroupBox1.Controls.Add(this.btnClose);
            this.ultraGroupBox1.Controls.Add(this.btnPrint);
            this.ultraGroupBox1.Controls.Add(this.btnExportCsv);
            this.ultraGroupBox1.Controls.Add(this.btnGenerate);
            this.ultraGroupBox1.Controls.Add(this.ultraLabel2);
            this.ultraGroupBox1.Controls.Add(this.ultraLabel1);
            this.ultraGroupBox1.Controls.Add(this.dtToDate);
            this.ultraGroupBox1.Controls.Add(this.dtFromDate);

            // Ledger Label & Combo
            this.ultraLabel4.Location = new System.Drawing.Point(15, 28);
            this.ultraLabel4.Name = "ultraLabel4";
            this.ultraLabel4.Size = new System.Drawing.Size(55, 23);
            this.ultraLabel4.Text = "Ledger:";

            this.ultraComboLedger.Location = new System.Drawing.Point(70, 24);
            this.ultraComboLedger.Name = "ultraComboLedger";
            this.ultraComboLedger.Size = new System.Drawing.Size(200, 24);
            this.ultraComboLedger.TabIndex = 0;

            // Quick Date
            this.ultraLabel3.Location = new System.Drawing.Point(285, 28);
            this.ultraLabel3.Name = "ultraLabel3";
            this.ultraLabel3.Size = new System.Drawing.Size(45, 23);
            this.ultraLabel3.Text = "Quick:";

            this.cmbDateQuickSelect.Location = new System.Drawing.Point(330, 24);
            this.cmbDateQuickSelect.Name = "cmbDateQuickSelect";
            this.cmbDateQuickSelect.Size = new System.Drawing.Size(120, 24);
            this.cmbDateQuickSelect.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbDateQuickSelect.Items.Add("Today");
            this.cmbDateQuickSelect.Items.Add("This Month");
            this.cmbDateQuickSelect.Items.Add("Last Month");
            this.cmbDateQuickSelect.Items.Add("This Financial Year");

            // From / To Dates
            this.ultraLabel1.Location = new System.Drawing.Point(465, 28);
            this.ultraLabel1.Name = "ultraLabel1";
            this.ultraLabel1.Size = new System.Drawing.Size(40, 23);
            this.ultraLabel1.Text = "From:";
            this.dtFromDate.Location = new System.Drawing.Point(505, 24);
            this.dtFromDate.Name = "dtFromDate";
            this.dtFromDate.Size = new System.Drawing.Size(110, 24);

            this.ultraLabel2.Location = new System.Drawing.Point(625, 28);
            this.ultraLabel2.Name = "ultraLabel2";
            this.ultraLabel2.Size = new System.Drawing.Size(25, 23);
            this.ultraLabel2.Text = "To:";
            this.dtToDate.Location = new System.Drawing.Point(650, 24);
            this.dtToDate.Name = "dtToDate";
            this.dtToDate.Size = new System.Drawing.Size(110, 24);

            // 1. Search Box
            var lblSearch = new Infragistics.Win.Misc.UltraLabel();
            lblSearch.Text = "Search:";
            lblSearch.Size = new System.Drawing.Size(50, 23);
            lblSearch.Location = new System.Drawing.Point(15, 60);

            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.txtSearch.Size = new System.Drawing.Size(200, 24);
            this.txtSearch.Location = new System.Drawing.Point(70, 56);
            this.txtSearch.NullText = "Search particulars...";
            this.txtSearch.ValueChanged += TxtSearch_ValueChanged;

            this.ultraGroupBox1.Controls.Add(lblSearch);
            this.ultraGroupBox1.Controls.Add(txtSearch);

            // Expand header for search
            this.panelHeader.Size = new System.Drawing.Size(1280, 95);

            // Buttons — anchored to the right
            this.btnGenerate.Location = new System.Drawing.Point(780, 20);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(85, 33);
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;

            this.btnExportCsv.Location = new System.Drawing.Point(870, 20);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(70, 33);
            this.btnExportCsv.Text = "CSV";
            this.btnExportCsv.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;

            this.btnPrint.Location = new System.Drawing.Point(945, 20);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(70, 33);
            this.btnPrint.Text = "Print";
            this.btnPrint.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;

            this.btnClose.Location = new System.Drawing.Point(1020, 20);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(70, 33);
            this.btnClose.Text = "Close";
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;

            // ==========================================
            // panelSummary — Dock.Top, height 65
            // ==========================================
            this.panelSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSummary.Location = new System.Drawing.Point(0, 70);
            this.panelSummary.Name = "panelSummary";
            this.panelSummary.Size = new System.Drawing.Size(1280, 65);
            this.panelSummary.TabIndex = 1;

            // Use TableLayoutPanel inside summary for equal distribution
            var tlp = new System.Windows.Forms.TableLayoutPanel();
            tlp.Dock = System.Windows.Forms.DockStyle.Fill;
            tlp.ColumnCount = 4;
            tlp.RowCount = 1;
            tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlp.Padding = new System.Windows.Forms.Padding(5);

            // Opening Balance panel
            SetupSummaryPanel(this.panelOpening, this.lblOpeningBalanceTitle, this.lblOpeningBalanceValue, "Opening Balance:");
            tlp.Controls.Add(this.panelOpening, 0, 0);

            // Total Receipts panel
            SetupSummaryPanel(this.panelReceipts, this.lblTotalReceiptsTitle, this.lblTotalReceiptsValue, "Total Receipts:");
            tlp.Controls.Add(this.panelReceipts, 1, 0);

            // Total Payments panel
            SetupSummaryPanel(this.panelPayments, this.lblTotalPaymentsTitle, this.lblTotalPaymentsValue, "Total Payments:");
            tlp.Controls.Add(this.panelPayments, 2, 0);

            // Closing Balance panel
            SetupSummaryPanel(this.panelClosing, this.lblClosingBalanceTitle, this.lblClosingBalanceValue, "Closing Balance:");
            tlp.Controls.Add(this.panelClosing, 3, 0);

            this.panelSummary.ClientArea.Controls.Add(tlp);

            // ==========================================
            // panelGrid — Dock.Fill (takes remaining space)
            // ==========================================
            this.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGrid.Name = "panelGrid";
            this.panelGrid.Padding = new System.Windows.Forms.Padding(5);
            this.panelGrid.TabIndex = 2;

            this.ultraGridTransactions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridTransactions.Name = "ultraGridTransactions";
            this.ultraGridTransactions.TabIndex = 0;
            this.panelGrid.ClientArea.Controls.Add(this.ultraGridTransactions);

            // ==========================================
            // Form
            // ==========================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 700);
            this.Controls.Add(this.panelMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "FrmCashBankBook";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cash & Bank Book";

            // Resume layout
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTransactions)).EndInit();
            this.panelGrid.ResumeLayout(false);
            this.panelSummary.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDateQuickSelect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboLedger)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBox1)).EndInit();
            this.ultraGroupBox1.ResumeLayout(false);
            this.panelHeader.ResumeLayout(false);
            this.panelMain.ClientArea.ResumeLayout(false);
            this.panelMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void SetupSummaryPanel(Infragistics.Win.Misc.UltraPanel panel, Infragistics.Win.Misc.UltraLabel lblTitle, Infragistics.Win.Misc.UltraLabel lblVal, string title)
        {
            panel.Dock = System.Windows.Forms.DockStyle.Fill;
            panel.Margin = new System.Windows.Forms.Padding(3);
            panel.Name = "pnl" + title.Replace(" ", "").Replace(":", "");

            lblTitle.Dock = System.Windows.Forms.DockStyle.Left;
            lblTitle.Size = new System.Drawing.Size(140, 50);
            lblTitle.Text = title;
            lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            lblTitle.Appearance.TextVAlign = Infragistics.Win.VAlign.Middle;
            lblTitle.Appearance.BackColor = System.Drawing.Color.Transparent;

            lblVal.Dock = System.Windows.Forms.DockStyle.Fill;
            lblVal.Text = "0.00";
            lblVal.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblVal.Appearance.TextHAlign = Infragistics.Win.HAlign.Right;
            lblVal.Appearance.TextVAlign = Infragistics.Win.VAlign.Middle;
            lblVal.Appearance.BackColor = System.Drawing.Color.Transparent;

            panel.ClientArea.Controls.Add(lblVal);
            panel.ClientArea.Controls.Add(lblTitle);
        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel panelMain;
        private Infragistics.Win.Misc.UltraPanel panelHeader;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBox1;
        private Infragistics.Win.UltraWinGrid.UltraCombo ultraComboLedger;
        private Infragistics.Win.Misc.UltraLabel ultraLabel4;
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
        private Infragistics.Win.Misc.UltraPanel panelOpening;
        private Infragistics.Win.Misc.UltraLabel lblOpeningBalanceTitle;
        private Infragistics.Win.Misc.UltraLabel lblOpeningBalanceValue;

        private Infragistics.Win.Misc.UltraPanel panelReceipts;
        private Infragistics.Win.Misc.UltraLabel lblTotalReceiptsTitle;
        private Infragistics.Win.Misc.UltraLabel lblTotalReceiptsValue;

        private Infragistics.Win.Misc.UltraPanel panelPayments;
        private Infragistics.Win.Misc.UltraLabel lblTotalPaymentsTitle;
        private Infragistics.Win.Misc.UltraLabel lblTotalPaymentsValue;

        private Infragistics.Win.Misc.UltraPanel panelClosing;
        private Infragistics.Win.Misc.UltraLabel lblClosingBalanceTitle;
        private Infragistics.Win.Misc.UltraLabel lblClosingBalanceValue;

        private Infragistics.Win.Misc.UltraPanel panelGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridTransactions;
    }
}
