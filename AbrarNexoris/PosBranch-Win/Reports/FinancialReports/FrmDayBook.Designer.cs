using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmDayBook
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
            this.ultraLabel3 = new Infragistics.Win.Misc.UltraLabel();
            this.cmbDateQuickSelect = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.ultraLabel1 = new Infragistics.Win.Misc.UltraLabel();
            this.dtFromDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.ultraLabel2 = new Infragistics.Win.Misc.UltraLabel();
            this.dtToDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.btnGenerate = new Infragistics.Win.Misc.UltraButton();
            this.btnExportCsv = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();

            this.panelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.panelReceipts = new Infragistics.Win.Misc.UltraPanel();
            this.lblTotalReceiptsTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalReceiptsValue = new Infragistics.Win.Misc.UltraLabel();
            this.panelPayments = new Infragistics.Win.Misc.UltraPanel();
            this.lblTotalPaymentsTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalPaymentsValue = new Infragistics.Win.Misc.UltraLabel();
            this.panelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridTransactions = new Infragistics.Win.UltraWinGrid.UltraGrid();

            this.panelMain.ClientArea.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.panelHeader.ClientArea.SuspendLayout();
            this.panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBox1)).BeginInit();
            this.ultraGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDateQuickSelect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).BeginInit();
            this.panelSummary.ClientArea.SuspendLayout();
            this.panelSummary.SuspendLayout();
            this.panelReceipts.ClientArea.SuspendLayout();
            this.panelReceipts.SuspendLayout();
            this.panelPayments.ClientArea.SuspendLayout();
            this.panelPayments.SuspendLayout();
            this.panelGrid.ClientArea.SuspendLayout();
            this.panelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTransactions)).BeginInit();
            this.SuspendLayout();

            // 
            // panelMain
            // 
            this.panelMain.ClientArea.Controls.Add(this.panelGrid);
            this.panelMain.ClientArea.Controls.Add(this.panelSummary);
            this.panelMain.ClientArea.Controls.Add(this.panelHeader);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1280, 720);

            // 
            // panelHeader
            // 
            this.panelHeader.ClientArea.Controls.Add(this.ultraGroupBox1);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(1280, 95);
            this.panelHeader.Padding = new Padding(10, 10, 10, 5);

            // 
            // ultraGroupBox1
            // 
            this.ultraGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGroupBox1.Location = new System.Drawing.Point(10, 10);
            this.ultraGroupBox1.Name = "ultraGroupBox1";
            this.ultraGroupBox1.Size = new System.Drawing.Size(1260, 80);
            this.ultraGroupBox1.Text = "Filter Parameters";
            
            // Filters
            this.ultraLabel3.Location = new System.Drawing.Point(15, 28);
            this.ultraLabel3.Name = "ultraLabel3";
            this.ultraLabel3.Size = new System.Drawing.Size(45, 23);
            this.ultraLabel3.Text = "Quick:";

            this.cmbDateQuickSelect.Location = new System.Drawing.Point(60, 24);
            this.cmbDateQuickSelect.Name = "cmbDateQuickSelect";
            this.cmbDateQuickSelect.Size = new System.Drawing.Size(120, 24);
            this.cmbDateQuickSelect.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbDateQuickSelect.Items.Add("Today");
            this.cmbDateQuickSelect.Items.Add("Yesterday");
            this.cmbDateQuickSelect.Items.Add("This Month");
            this.cmbDateQuickSelect.Items.Add("Last Month");
            this.cmbDateQuickSelect.Items.Add("This Financial Year");

            this.ultraLabel1.Location = new System.Drawing.Point(195, 28);
            this.ultraLabel1.Name = "ultraLabel1";
            this.ultraLabel1.Size = new System.Drawing.Size(40, 23);
            this.ultraLabel1.Text = "From:";
            this.dtFromDate.Location = new System.Drawing.Point(235, 24);
            this.dtFromDate.Name = "dtFromDate";
            this.dtFromDate.Size = new System.Drawing.Size(110, 24);

            this.ultraLabel2.Location = new System.Drawing.Point(355, 28);
            this.ultraLabel2.Name = "ultraLabel2";
            this.ultraLabel2.Size = new System.Drawing.Size(25, 23);
            this.ultraLabel2.Text = "To:";
            this.dtToDate.Location = new System.Drawing.Point(380, 24);
            this.dtToDate.Name = "dtToDate";
            this.dtToDate.Size = new System.Drawing.Size(110, 24);

            // Search Box
            var lblSearch = new Infragistics.Win.Misc.UltraLabel();
            lblSearch.Text = "Search:";
            lblSearch.Size = new System.Drawing.Size(50, 23);
            lblSearch.Location = new System.Drawing.Point(15, 60);

            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.txtSearch.Size = new System.Drawing.Size(200, 24);
            this.txtSearch.Location = new System.Drawing.Point(60, 56);
            this.txtSearch.NullText = "Search particulars/narration...";

            this.chkGroupByVoucher = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
            this.chkGroupByVoucher.Location = new System.Drawing.Point(280, 56);
            this.chkGroupByVoucher.Name = "chkGroupByVoucher";
            this.chkGroupByVoucher.Size = new System.Drawing.Size(150, 24);
            this.chkGroupByVoucher.Text = "Group By Voucher";
            this.chkGroupByVoucher.Checked = false;

            this.ultraGroupBox1.Controls.Add(this.ultraLabel3);
            this.ultraGroupBox1.Controls.Add(this.cmbDateQuickSelect);
            this.ultraGroupBox1.Controls.Add(this.ultraLabel1);
            this.ultraGroupBox1.Controls.Add(this.dtToDate);
            this.ultraGroupBox1.Controls.Add(this.dtFromDate);
            this.ultraGroupBox1.Controls.Add(this.ultraLabel2);
            this.ultraGroupBox1.Controls.Add(lblSearch);
            this.ultraGroupBox1.Controls.Add(this.txtSearch);
            this.ultraGroupBox1.Controls.Add(this.chkGroupByVoucher);

            // Buttons
            this.btnGenerate.Location = new System.Drawing.Point(510, 20);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(90, 30);
            this.btnGenerate.Text = "▶ Generate";
            this.btnGenerate.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            this.btnExportCsv.Location = new System.Drawing.Point(610, 20);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(80, 30);
            this.btnExportCsv.Text = "⬇ CSV";
            this.btnExportCsv.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            this.btnPrint.Location = new System.Drawing.Point(700, 20);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(80, 30);
            this.btnPrint.Text = "🖨 Print";
            this.btnPrint.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            this.btnClose.Location = new System.Drawing.Point(790, 20);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(80, 30);
            this.btnClose.Text = "✕ Close";
            this.btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            this.ultraGroupBox1.Controls.Add(this.btnGenerate);
            this.ultraGroupBox1.Controls.Add(this.btnExportCsv);
            this.ultraGroupBox1.Controls.Add(this.btnPrint);
            this.ultraGroupBox1.Controls.Add(this.btnClose);

            // 
            // panelSummary
            // 
            this.panelSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSummary.Location = new System.Drawing.Point(0, 95);
            this.panelSummary.Name = "panelSummary";
            this.panelSummary.Size = new System.Drawing.Size(1280, 60);
            this.panelSummary.Padding = new Padding(10, 5, 10, 5);

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.ColumnCount = 2;
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp.RowCount = 1;
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlp.Dock = DockStyle.Fill;
            tlp.Padding = new Padding(0);
            tlp.Margin = new Padding(0);

            tlp.Controls.Add(this.panelReceipts, 0, 0);
            tlp.Controls.Add(this.panelPayments, 1, 0);
            this.panelSummary.ClientArea.Controls.Add(tlp);

            SetupSummaryPanel(this.panelReceipts, this.lblTotalReceiptsTitle, this.lblTotalReceiptsValue, "Total Debits:");
            SetupSummaryPanel(this.panelPayments, this.lblTotalPaymentsTitle, this.lblTotalPaymentsValue, "Total Credits:");

            // 
            // panelGrid
            // 
            this.panelGrid.ClientArea.Controls.Add(this.ultraGridTransactions);
            this.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGrid.Location = new System.Drawing.Point(0, 155);
            this.panelGrid.Name = "panelGrid";
            this.panelGrid.Size = new System.Drawing.Size(1280, 565);
            this.panelGrid.Padding = new Padding(10);

            // 
            // ultraGridTransactions
            // 
            this.ultraGridTransactions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridTransactions.Location = new System.Drawing.Point(10, 10);
            this.ultraGridTransactions.Name = "ultraGridTransactions";
            this.ultraGridTransactions.Size = new System.Drawing.Size(1260, 545);

            // 
            // FrmDayBook
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.panelMain);
            this.Name = "FrmDayBook";
            this.Text = "Day Book";
            this.BackColor = System.Drawing.Color.White;

            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTransactions)).EndInit();
            this.panelGrid.ClientArea.ResumeLayout(false);
            this.panelGrid.ResumeLayout(false);
            this.panelPayments.ClientArea.ResumeLayout(false);
            this.panelPayments.ResumeLayout(false);
            this.panelReceipts.ClientArea.ResumeLayout(false);
            this.panelReceipts.ResumeLayout(false);
            this.panelSummary.ClientArea.ResumeLayout(false);
            this.panelSummary.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDateQuickSelect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBox1)).EndInit();
            this.ultraGroupBox1.ResumeLayout(false);
            this.panelHeader.ClientArea.ResumeLayout(false);
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

        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.UltraWinEditors.UltraCheckEditor chkGroupByVoucher;

        private Infragistics.Win.Misc.UltraPanel panelSummary;
        private Infragistics.Win.Misc.UltraPanel panelReceipts;
        private Infragistics.Win.Misc.UltraLabel lblTotalReceiptsTitle;
        private Infragistics.Win.Misc.UltraLabel lblTotalReceiptsValue;
        private Infragistics.Win.Misc.UltraPanel panelPayments;
        private Infragistics.Win.Misc.UltraLabel lblTotalPaymentsTitle;
        private Infragistics.Win.Misc.UltraLabel lblTotalPaymentsValue;

        private Infragistics.Win.Misc.UltraPanel panelGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridTransactions;
    }
}
