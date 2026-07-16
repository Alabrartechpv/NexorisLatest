namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmTrialBalance
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
            this.ultraPanelMain = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridTrialBalance = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.panelDifference = new Infragistics.Win.Misc.UltraPanel();
            this.lblDifferenceCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblDifferenceValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalClosingCrValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalClosingCrCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalClosingDrValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalClosingDrCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalTransactionCrValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalTransactionCrCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalTransactionDrValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalTransactionDrCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalOpeningCrValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalOpeningCrCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalOpeningDrValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalOpeningDrCaption = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxFilters = new Infragistics.Win.Misc.UltraGroupBox();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnGenerate = new Infragistics.Win.Misc.UltraButton();
            this.lblSearchStatus = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.ultraDateTimeTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new System.Windows.Forms.Label();
            this.ultraDateTimeFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblFromDate = new System.Windows.Forms.Label();
            this.ultraPanelMain.ClientArea.SuspendLayout();
            this.ultraPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTrialBalance)).BeginInit();
            this.ultraPanelSummary.ClientArea.SuspendLayout();
            this.ultraPanelSummary.SuspendLayout();
            this.panelDifference.ClientArea.SuspendLayout();
            this.panelDifference.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFilters)).BeginInit();
            this.ultraGroupBoxFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).BeginInit();
            this.SuspendLayout();

            this.ultraPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMain.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelMain.Name = "ultraPanelMain";
            this.ultraPanelMain.Size = new System.Drawing.Size(1200, 700);
            this.ultraPanelMain.TabIndex = 0;
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraGridTrialBalance);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraPanelSummary);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraGroupBoxFilters);

            this.ultraGridTrialBalance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridTrialBalance.Location = new System.Drawing.Point(0, 60);
            this.ultraGridTrialBalance.Name = "ultraGridTrialBalance";
            this.ultraGridTrialBalance.Size = new System.Drawing.Size(1200, 595);
            this.ultraGridTrialBalance.TabIndex = 1;

            this.ultraPanelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelSummary.Location = new System.Drawing.Point(0, 655);
            this.ultraPanelSummary.Name = "ultraPanelSummary";
            this.ultraPanelSummary.Size = new System.Drawing.Size(1200, 45);
            this.ultraPanelSummary.TabIndex = 2;
            this.ultraPanelSummary.ClientArea.Controls.Add(this.panelDifference);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalClosingCrValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalClosingCrCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalClosingDrValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalClosingDrCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalTransactionCrValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalTransactionCrCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalTransactionDrValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalTransactionDrCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalOpeningCrValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalOpeningCrCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalOpeningDrValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalOpeningDrCaption);

            this.panelDifference.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
            this.panelDifference.Location = new System.Drawing.Point(920, 2);
            this.panelDifference.Name = "panelDifference";
            this.panelDifference.Size = new System.Drawing.Size(278, 41);
            this.panelDifference.TabIndex = 12;
            this.panelDifference.ClientArea.Controls.Add(this.lblDifferenceValue);
            this.panelDifference.ClientArea.Controls.Add(this.lblDifferenceCaption);

            this.lblDifferenceCaption.Location = new System.Drawing.Point(8, 12);
            this.lblDifferenceCaption.Name = "lblDifferenceCaption";
            this.lblDifferenceCaption.Size = new System.Drawing.Size(120, 20);
            this.lblDifferenceCaption.Text = "DIFFERENCE:";

            this.lblDifferenceValue.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
            this.lblDifferenceValue.Location = new System.Drawing.Point(135, 10);
            this.lblDifferenceValue.Name = "lblDifferenceValue";
            this.lblDifferenceValue.Size = new System.Drawing.Size(135, 20);
            this.lblDifferenceValue.Appearance.TextHAlign = Infragistics.Win.HAlign.Right;
            this.lblDifferenceValue.Text = "0.00";

            this.lblTotalOpeningDrCaption.Location = new System.Drawing.Point(10, 4);
            this.lblTotalOpeningDrCaption.Name = "lblTotalOpeningDrCaption";
            this.lblTotalOpeningDrCaption.Size = new System.Drawing.Size(120, 16);
            this.lblTotalOpeningDrCaption.Text = "Total Opening Dr:";

            this.lblTotalOpeningDrValue.Location = new System.Drawing.Point(10, 22);
            this.lblTotalOpeningDrValue.Name = "lblTotalOpeningDrValue";
            this.lblTotalOpeningDrValue.Size = new System.Drawing.Size(130, 18);
            this.lblTotalOpeningDrValue.Text = "0.00";

            this.lblTotalOpeningCrCaption.Location = new System.Drawing.Point(150, 4);
            this.lblTotalOpeningCrCaption.Name = "lblTotalOpeningCrCaption";
            this.lblTotalOpeningCrCaption.Size = new System.Drawing.Size(120, 16);
            this.lblTotalOpeningCrCaption.Text = "Total Opening Cr:";

            this.lblTotalOpeningCrValue.Location = new System.Drawing.Point(150, 22);
            this.lblTotalOpeningCrValue.Name = "lblTotalOpeningCrValue";
            this.lblTotalOpeningCrValue.Size = new System.Drawing.Size(130, 18);
            this.lblTotalOpeningCrValue.Text = "0.00";

            this.lblTotalTransactionDrCaption.Location = new System.Drawing.Point(300, 4);
            this.lblTotalTransactionDrCaption.Name = "lblTotalTransactionDrCaption";
            this.lblTotalTransactionDrCaption.Size = new System.Drawing.Size(120, 16);
            this.lblTotalTransactionDrCaption.Text = "Total Period Dr:";

            this.lblTotalTransactionDrValue.Location = new System.Drawing.Point(300, 22);
            this.lblTotalTransactionDrValue.Name = "lblTotalTransactionDrValue";
            this.lblTotalTransactionDrValue.Size = new System.Drawing.Size(130, 18);
            this.lblTotalTransactionDrValue.Text = "0.00";

            this.lblTotalTransactionCrCaption.Location = new System.Drawing.Point(440, 4);
            this.lblTotalTransactionCrCaption.Name = "lblTotalTransactionCrCaption";
            this.lblTotalTransactionCrCaption.Size = new System.Drawing.Size(120, 16);
            this.lblTotalTransactionCrCaption.Text = "Total Period Cr:";

            this.lblTotalTransactionCrValue.Location = new System.Drawing.Point(440, 22);
            this.lblTotalTransactionCrValue.Name = "lblTotalTransactionCrValue";
            this.lblTotalTransactionCrValue.Size = new System.Drawing.Size(130, 18);
            this.lblTotalTransactionCrValue.Text = "0.00";

            this.lblTotalClosingDrCaption.Location = new System.Drawing.Point(600, 4);
            this.lblTotalClosingDrCaption.Name = "lblTotalClosingDrCaption";
            this.lblTotalClosingDrCaption.Size = new System.Drawing.Size(120, 16);
            this.lblTotalClosingDrCaption.Text = "Total Closing Dr:";

            this.lblTotalClosingDrValue.Location = new System.Drawing.Point(600, 22);
            this.lblTotalClosingDrValue.Name = "lblTotalClosingDrValue";
            this.lblTotalClosingDrValue.Size = new System.Drawing.Size(130, 18);
            this.lblTotalClosingDrValue.Text = "0.00";

            this.lblTotalClosingCrCaption.Location = new System.Drawing.Point(740, 4);
            this.lblTotalClosingCrCaption.Name = "lblTotalClosingCrCaption";
            this.lblTotalClosingCrCaption.Size = new System.Drawing.Size(120, 16);
            this.lblTotalClosingCrCaption.Text = "Total Closing Cr:";

            this.lblTotalClosingCrValue.Location = new System.Drawing.Point(740, 22);
            this.lblTotalClosingCrValue.Name = "lblTotalClosingCrValue";
            this.lblTotalClosingCrValue.Size = new System.Drawing.Size(130, 18);
            this.lblTotalClosingCrValue.Text = "0.00";

            this.ultraGroupBoxFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraGroupBoxFilters.Location = new System.Drawing.Point(0, 0);
            this.ultraGroupBoxFilters.Name = "ultraGroupBoxFilters";
            this.ultraGroupBoxFilters.Size = new System.Drawing.Size(1200, 60);
            this.ultraGroupBoxFilters.TabIndex = 3;
            this.ultraGroupBoxFilters.Controls.Add(this.lblFromDate);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraDateTimeFrom);
            this.ultraGroupBoxFilters.Controls.Add(this.lblToDate);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraDateTimeTo);
            this.ultraGroupBoxFilters.Controls.Add(this.lblSearch);
            this.ultraGroupBoxFilters.Controls.Add(this.txtSearch);
            this.ultraGroupBoxFilters.Controls.Add(this.lblSearchStatus);
            this.ultraGroupBoxFilters.Controls.Add(this.btnGenerate);
            this.ultraGroupBoxFilters.Controls.Add(this.btnExport);
            this.ultraGroupBoxFilters.Controls.Add(this.btnPrint);
            this.ultraGroupBoxFilters.Controls.Add(this.btnClose);

            this.lblFromDate.AutoSize = true;
            this.lblFromDate.Location = new System.Drawing.Point(15, 23);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(33, 13);
            this.lblFromDate.Text = "From:";

            this.ultraDateTimeFrom.Location = new System.Drawing.Point(55, 19);
            this.ultraDateTimeFrom.Name = "ultraDateTimeFrom";
            this.ultraDateTimeFrom.Size = new System.Drawing.Size(120, 21);
            this.ultraDateTimeFrom.TabIndex = 0;

            this.lblToDate.AutoSize = true;
            this.lblToDate.Location = new System.Drawing.Point(190, 23);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(23, 13);
            this.lblToDate.Text = "To:";

            this.ultraDateTimeTo.Location = new System.Drawing.Point(220, 19);
            this.ultraDateTimeTo.Name = "ultraDateTimeTo";
            this.ultraDateTimeTo.Size = new System.Drawing.Size(120, 21);
            this.ultraDateTimeTo.TabIndex = 1;

            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(365, 23);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(44, 15);
            this.lblSearch.Text = "Search:";

            this.txtSearch.Location = new System.Drawing.Point(415, 19);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(260, 21);
            this.txtSearch.TabIndex = 2;

            this.lblSearchStatus.Location = new System.Drawing.Point(685, 21);
            this.lblSearchStatus.Name = "lblSearchStatus";
            this.lblSearchStatus.Size = new System.Drawing.Size(145, 18);
            this.lblSearchStatus.Text = "";

            this.btnGenerate.Location = new System.Drawing.Point(845, 17);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(90, 26);
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);

            this.btnExport.Location = new System.Drawing.Point(945, 17);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 26);
            this.btnExport.Text = "CSV";
            this.btnExport.Click += new System.EventHandler(this.btnExportCsv_Click);

            this.btnPrint.Location = new System.Drawing.Point(1030, 17);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(75, 26);
            this.btnPrint.Text = "Print";
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);

            this.btnClose.Location = new System.Drawing.Point(1115, 17);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 26);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.ultraPanelMain);
            this.Name = "FrmTrialBalance";
            this.Text = "Trial Balance";
            this.Load += new System.EventHandler(this.FrmTrialBalance_Load);
            this.ultraPanelMain.ClientArea.ResumeLayout(false);
            this.ultraPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTrialBalance)).EndInit();
            this.ultraPanelSummary.ClientArea.ResumeLayout(false);
            this.ultraPanelSummary.ResumeLayout(false);
            this.panelDifference.ClientArea.ResumeLayout(false);
            this.panelDifference.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFilters)).EndInit();
            this.ultraGroupBoxFilters.ResumeLayout(false);
            this.ultraGroupBoxFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelMain;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridTrialBalance;
        private Infragistics.Win.Misc.UltraPanel ultraPanelSummary;
        private Infragistics.Win.Misc.UltraPanel panelDifference;
        private Infragistics.Win.Misc.UltraLabel lblDifferenceCaption;
        private Infragistics.Win.Misc.UltraLabel lblDifferenceValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalClosingCrValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalClosingCrCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalClosingDrValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalClosingDrCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalTransactionCrValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalTransactionCrCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalTransactionDrValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalTransactionDrCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalOpeningCrValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalOpeningCrCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalOpeningDrValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalOpeningDrCaption;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxFilters;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnGenerate;
        private Infragistics.Win.Misc.UltraLabel lblSearchStatus;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeTo;
        private System.Windows.Forms.Label lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeFrom;
        private System.Windows.Forms.Label lblFromDate;
    }
}
