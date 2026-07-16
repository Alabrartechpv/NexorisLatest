namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmBalanceSheet
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
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.ultraGroupBoxLiabilities = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraGridLiabilities = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelNetProfit = new Infragistics.Win.Misc.UltraPanel();
            this.lblNetProfitCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblNetProfitValue = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxAssets = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraGridAssets = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelDifference = new Infragistics.Win.Misc.UltraPanel();
            this.lblDifferenceCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblDifferenceValue = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.lblTotalCapitalValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCapitalCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalLiabilitiesValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalLiabilitiesCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalAssetsValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalAssetsCaption = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxFilters = new Infragistics.Win.Misc.UltraGroupBox();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnGenerate = new Infragistics.Win.Misc.UltraButton();
            this.ultraDateTimeTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new System.Windows.Forms.Label();
            this.ultraDateTimeFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblFromDate = new System.Windows.Forms.Label();
            
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxLiabilities)).BeginInit();
            this.ultraGroupBoxLiabilities.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridLiabilities)).BeginInit();
            this.panelNetProfit.ClientArea.SuspendLayout();
            this.panelNetProfit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxAssets)).BeginInit();
            this.ultraGroupBoxAssets.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridAssets)).BeginInit();
            this.panelDifference.ClientArea.SuspendLayout();
            this.panelDifference.SuspendLayout();
            this.ultraPanelSummary.ClientArea.SuspendLayout();
            this.ultraPanelSummary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFilters)).BeginInit();
            this.ultraGroupBoxFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).BeginInit();
            this.SuspendLayout();

            // ultraPanelMain
            this.ultraPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMain.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelMain.Name = "ultraPanelMain";
            this.ultraPanelMain.Size = new System.Drawing.Size(1200, 700);
            this.ultraPanelMain.TabIndex = 0;
            this.ultraPanelMain.ClientArea.Controls.Add(this.splitContainerMain);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraPanelSummary);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraGroupBoxFilters);

            // splitContainerMain
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 60);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Size = new System.Drawing.Size(1200, 600);
            this.splitContainerMain.SplitterDistance = 600;
            this.splitContainerMain.TabIndex = 1;

            // Panel1 (Liabilities)
            this.splitContainerMain.Panel1.Controls.Add(this.ultraGroupBoxLiabilities);
            this.splitContainerMain.Panel1.Padding = new System.Windows.Forms.Padding(5);

            // ultraGroupBoxLiabilities
            this.ultraGroupBoxLiabilities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGroupBoxLiabilities.Location = new System.Drawing.Point(5, 5);
            this.ultraGroupBoxLiabilities.Name = "ultraGroupBoxLiabilities";
            this.ultraGroupBoxLiabilities.Size = new System.Drawing.Size(590, 590);
            this.ultraGroupBoxLiabilities.TabIndex = 0;
            this.ultraGroupBoxLiabilities.Text = "LIABILITIES & CAPITAL";
            this.ultraGroupBoxLiabilities.Controls.Add(this.ultraGridLiabilities);
            this.ultraGroupBoxLiabilities.Controls.Add(this.panelNetProfit);

            // ultraGridLiabilities
            this.ultraGridLiabilities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridLiabilities.Location = new System.Drawing.Point(3, 23);
            this.ultraGridLiabilities.Name = "ultraGridLiabilities";
            this.ultraGridLiabilities.Size = new System.Drawing.Size(584, 524);
            this.ultraGridLiabilities.TabIndex = 0;

            // panelNetProfit
            this.panelNetProfit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelNetProfit.Location = new System.Drawing.Point(3, 547);
            this.panelNetProfit.Name = "panelNetProfit";
            this.panelNetProfit.Size = new System.Drawing.Size(584, 40);
            this.panelNetProfit.TabIndex = 1;
            this.panelNetProfit.ClientArea.Controls.Add(this.lblNetProfitValue);
            this.panelNetProfit.ClientArea.Controls.Add(this.lblNetProfitCaption);

            this.lblNetProfitCaption.Location = new System.Drawing.Point(10, 10);
            this.lblNetProfitCaption.Name = "lblNetProfitCaption";
            this.lblNetProfitCaption.Size = new System.Drawing.Size(150, 20);
            this.lblNetProfitCaption.Text = "★ NET PROFIT:";

            this.lblNetProfitValue.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
            this.lblNetProfitValue.Location = new System.Drawing.Point(400, 8);
            this.lblNetProfitValue.Name = "lblNetProfitValue";
            this.lblNetProfitValue.Size = new System.Drawing.Size(170, 25);
            this.lblNetProfitValue.Appearance.TextHAlign = Infragistics.Win.HAlign.Right;
            this.lblNetProfitValue.Text = "0.00";

            // Panel2 (Assets)
            this.splitContainerMain.Panel2.Controls.Add(this.ultraGroupBoxAssets);
            this.splitContainerMain.Panel2.Padding = new System.Windows.Forms.Padding(5);

            // ultraGroupBoxAssets
            this.ultraGroupBoxAssets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGroupBoxAssets.Location = new System.Drawing.Point(5, 5);
            this.ultraGroupBoxAssets.Name = "ultraGroupBoxAssets";
            this.ultraGroupBoxAssets.Size = new System.Drawing.Size(586, 590);
            this.ultraGroupBoxAssets.TabIndex = 0;
            this.ultraGroupBoxAssets.Text = "ASSETS";
            this.ultraGroupBoxAssets.Controls.Add(this.ultraGridAssets);
            this.ultraGroupBoxAssets.Controls.Add(this.panelDifference);

            // ultraGridAssets
            this.ultraGridAssets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridAssets.Location = new System.Drawing.Point(3, 23);
            this.ultraGridAssets.Name = "ultraGridAssets";
            this.ultraGridAssets.Size = new System.Drawing.Size(580, 524);
            this.ultraGridAssets.TabIndex = 0;

            // panelDifference
            this.panelDifference.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelDifference.Location = new System.Drawing.Point(3, 547);
            this.panelDifference.Name = "panelDifference";
            this.panelDifference.Size = new System.Drawing.Size(580, 40);
            this.panelDifference.TabIndex = 1;
            this.panelDifference.ClientArea.Controls.Add(this.lblDifferenceValue);
            this.panelDifference.ClientArea.Controls.Add(this.lblDifferenceCaption);

            this.lblDifferenceCaption.Location = new System.Drawing.Point(10, 10);
            this.lblDifferenceCaption.Name = "lblDifferenceCaption";
            this.lblDifferenceCaption.Size = new System.Drawing.Size(200, 20);
            this.lblDifferenceCaption.Text = "DIFFERENCE:";

            this.lblDifferenceValue.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
            this.lblDifferenceValue.Location = new System.Drawing.Point(400, 10);
            this.lblDifferenceValue.Name = "lblDifferenceValue";
            this.lblDifferenceValue.Size = new System.Drawing.Size(170, 20);
            this.lblDifferenceValue.Appearance.TextHAlign = Infragistics.Win.HAlign.Right;
            this.lblDifferenceValue.Text = "0.00";

            // ultraPanelSummary (Bottom)
            this.ultraPanelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelSummary.Location = new System.Drawing.Point(0, 660);
            this.ultraPanelSummary.Name = "ultraPanelSummary";
            this.ultraPanelSummary.Size = new System.Drawing.Size(1200, 40);
            this.ultraPanelSummary.TabIndex = 2;
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalAssetsValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalAssetsCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalCapitalValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalCapitalCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalLiabilitiesValue);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalLiabilitiesCaption);

            this.lblTotalLiabilitiesCaption.Location = new System.Drawing.Point(20, 10);
            this.lblTotalLiabilitiesCaption.Name = "lblTotalLiabilitiesCaption";
            this.lblTotalLiabilitiesCaption.Size = new System.Drawing.Size(100, 20);
            this.lblTotalLiabilitiesCaption.Text = "Total Liabilities:";

            this.lblTotalLiabilitiesValue.Location = new System.Drawing.Point(120, 8);
            this.lblTotalLiabilitiesValue.Name = "lblTotalLiabilitiesValue";
            this.lblTotalLiabilitiesValue.Size = new System.Drawing.Size(150, 20);
            this.lblTotalLiabilitiesValue.Text = "0.00";

            this.lblTotalCapitalCaption.Location = new System.Drawing.Point(300, 10);
            this.lblTotalCapitalCaption.Name = "lblTotalCapitalCaption";
            this.lblTotalCapitalCaption.Size = new System.Drawing.Size(100, 20);
            this.lblTotalCapitalCaption.Text = "Total Capital:";

            this.lblTotalCapitalValue.Location = new System.Drawing.Point(400, 8);
            this.lblTotalCapitalValue.Name = "lblTotalCapitalValue";
            this.lblTotalCapitalValue.Size = new System.Drawing.Size(150, 20);
            this.lblTotalCapitalValue.Text = "0.00";

            this.lblTotalAssetsCaption.Location = new System.Drawing.Point(600, 10);
            this.lblTotalAssetsCaption.Name = "lblTotalAssetsCaption";
            this.lblTotalAssetsCaption.Size = new System.Drawing.Size(100, 20);
            this.lblTotalAssetsCaption.Text = "Total Assets:";

            this.lblTotalAssetsValue.Location = new System.Drawing.Point(700, 8);
            this.lblTotalAssetsValue.Name = "lblTotalAssetsValue";
            this.lblTotalAssetsValue.Size = new System.Drawing.Size(150, 20);
            this.lblTotalAssetsValue.Text = "0.00";

            // ultraGroupBoxFilters (Top)
            this.ultraGroupBoxFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraGroupBoxFilters.Location = new System.Drawing.Point(0, 0);
            this.ultraGroupBoxFilters.Name = "ultraGroupBoxFilters";
            this.ultraGroupBoxFilters.Size = new System.Drawing.Size(1200, 60);
            this.ultraGroupBoxFilters.TabIndex = 3;
            this.ultraGroupBoxFilters.Controls.Add(this.lblFromDate);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraDateTimeFrom);
            this.ultraGroupBoxFilters.Controls.Add(this.lblToDate);
            this.ultraGroupBoxFilters.Controls.Add(this.ultraDateTimeTo);
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

            this.btnGenerate.Location = new System.Drawing.Point(365, 17);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(90, 26);
            this.btnGenerate.Text = "▶ Generate";
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);

            this.btnExport.Location = new System.Drawing.Point(465, 17);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 26);
            this.btnExport.Text = "↓ CSV";
            this.btnExport.Click += new System.EventHandler(this.btnExportCsv_Click);

            this.btnPrint.Location = new System.Drawing.Point(550, 17);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(75, 26);
            this.btnPrint.Text = "🖨 Print";
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);

            this.btnClose.Location = new System.Drawing.Point(635, 17);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 26);
            this.btnClose.Text = "✖ Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.ultraPanelMain);
            this.Name = "FrmBalanceSheet";
            this.Text = "Balance Sheet";
            this.Load += new System.EventHandler(this.FrmBalanceSheet_Load);

            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            this.splitContainerMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxLiabilities)).EndInit();
            this.ultraGroupBoxLiabilities.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridLiabilities)).EndInit();
            this.panelNetProfit.ClientArea.ResumeLayout(false);
            this.panelNetProfit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxAssets)).EndInit();
            this.ultraGroupBoxAssets.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridAssets)).EndInit();
            this.panelDifference.ClientArea.ResumeLayout(false);
            this.panelDifference.ResumeLayout(false);
            this.ultraPanelSummary.ClientArea.ResumeLayout(false);
            this.ultraPanelSummary.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFilters)).EndInit();
            this.ultraGroupBoxFilters.ResumeLayout(false);
            this.ultraGroupBoxFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelMain;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxLiabilities;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridLiabilities;
        private Infragistics.Win.Misc.UltraPanel panelNetProfit;
        private Infragistics.Win.Misc.UltraLabel lblNetProfitCaption;
        private Infragistics.Win.Misc.UltraLabel lblNetProfitValue;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxAssets;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridAssets;
        private Infragistics.Win.Misc.UltraPanel panelDifference;
        private Infragistics.Win.Misc.UltraLabel lblDifferenceCaption;
        private Infragistics.Win.Misc.UltraLabel lblDifferenceValue;
        private Infragistics.Win.Misc.UltraPanel ultraPanelSummary;
        private Infragistics.Win.Misc.UltraLabel lblTotalCapitalValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalCapitalCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalLiabilitiesValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalLiabilitiesCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalAssetsValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalAssetsCaption;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxFilters;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnGenerate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeTo;
        private System.Windows.Forms.Label lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeFrom;
        private System.Windows.Forms.Label lblFromDate;
    }
}
