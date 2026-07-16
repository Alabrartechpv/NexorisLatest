namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmManualPartyBalanceReport
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
            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnLoad = new Infragistics.Win.Misc.UltraButton();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblPartyName = new Infragistics.Win.Misc.UltraLabel();
            this.cmbBalanceType = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblBalanceType = new Infragistics.Win.Misc.UltraLabel();
            this.cmbPartyType = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblPartyType = new Infragistics.Win.Misc.UltraLabel();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.chkOpenOnly = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();

            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();

            this.ultraPanelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.lblTotalRemaining = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalRemainingCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalSettled = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalSettledCaption = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalAmount = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalAmountCaption = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbBalanceType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPartyType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkOpenOnly)).BeginInit();

            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();

            this.ultraPanelSummary.ClientArea.SuspendLayout();
            this.ultraPanelSummary.SuspendLayout();
            this.SuspendLayout();

            // 
            // ultraPanelControls
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(245)))));
            appearance1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(210)))), ((int)(((byte)(220)))));
            this.ultraPanelControls.Appearance = appearance1;
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnClearFilters);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnLoad);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPartyName);
            this.ultraPanelControls.ClientArea.Controls.Add(this.cmbBalanceType);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblBalanceType);
            this.ultraPanelControls.ClientArea.Controls.Add(this.cmbPartyType);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPartyType);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnExport);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnClose);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtTo);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.chkOpenOnly);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1349, 118);
            this.ultraPanelControls.TabIndex = 0;

            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(12, 19);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(80, 23);
            this.lblFromDate.TabIndex = 1;
            this.lblFromDate.Text = "From Date:";

            // 
            // dtFrom
            // 
            this.dtFrom.Location = new System.Drawing.Point(85, 16);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(100, 25);
            this.dtFrom.TabIndex = 2;

            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(195, 19);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(50, 23);
            this.lblToDate.TabIndex = 3;
            this.lblToDate.Text = "To Date:";

            // 
            // dtTo
            // 
            this.dtTo.Location = new System.Drawing.Point(250, 16);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(100, 25);
            this.dtTo.TabIndex = 4;

            // 
            // lblPartyType
            // 
            this.lblPartyType.Location = new System.Drawing.Point(365, 19);
            this.lblPartyType.Name = "lblPartyType";
            this.lblPartyType.Size = new System.Drawing.Size(70, 23);
            this.lblPartyType.TabIndex = 5;
            this.lblPartyType.Text = "Party Type:";

            // 
            // cmbPartyType
            // 
            this.cmbPartyType.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            Infragistics.Win.ValueListItem valueListItem1 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem2 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem3 = new Infragistics.Win.ValueListItem();
            valueListItem1.DataValue = "All";
            valueListItem2.DataValue = "Customer";
            valueListItem3.DataValue = "Vendor";
            this.cmbPartyType.Items.AddRange(new Infragistics.Win.ValueListItem[] {
            valueListItem1,
            valueListItem2,
            valueListItem3});
            this.cmbPartyType.Location = new System.Drawing.Point(440, 16);
            this.cmbPartyType.Name = "cmbPartyType";
            this.cmbPartyType.Size = new System.Drawing.Size(130, 25);
            this.cmbPartyType.TabIndex = 6;

            // 
            // lblBalanceType
            // 
            this.lblBalanceType.Location = new System.Drawing.Point(590, 19);
            this.lblBalanceType.Name = "lblBalanceType";
            this.lblBalanceType.Size = new System.Drawing.Size(80, 23);
            this.lblBalanceType.TabIndex = 7;
            this.lblBalanceType.Text = "Balance Type:";

            // 
            // cmbBalanceType
            // 
            this.cmbBalanceType.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            Infragistics.Win.ValueListItem valueListItem4 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem5 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem6 = new Infragistics.Win.ValueListItem();
            valueListItem4.DataValue = "All";
            valueListItem5.DataValue = "Balance";
            valueListItem6.DataValue = "Excess";
            this.cmbBalanceType.Items.AddRange(new Infragistics.Win.ValueListItem[] {
            valueListItem4,
            valueListItem5,
            valueListItem6});
            this.cmbBalanceType.Location = new System.Drawing.Point(675, 16);
            this.cmbBalanceType.Name = "cmbBalanceType";
            this.cmbBalanceType.Size = new System.Drawing.Size(130, 25);
            this.cmbBalanceType.TabIndex = 8;

            // 
            // lblPartyName
            // 
            this.lblPartyName.Location = new System.Drawing.Point(12, 53);
            this.lblPartyName.Name = "lblPartyName";
            this.lblPartyName.Size = new System.Drawing.Size(70, 23);
            this.lblPartyName.TabIndex = 9;
            this.lblPartyName.Text = "Party Name:";

            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(85, 50);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(265, 25);
            this.txtSearch.TabIndex = 10;
            this.txtSearch.NullText = "Search by party name...";

            // 
            // chkOpenOnly
            // 
            this.chkOpenOnly.Location = new System.Drawing.Point(365, 52);
            this.chkOpenOnly.Name = "chkOpenOnly";
            this.chkOpenOnly.Size = new System.Drawing.Size(120, 20);
            this.chkOpenOnly.TabIndex = 11;
            this.chkOpenOnly.Text = "Only Open";

            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(85, 82);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(95, 28);
            this.btnLoad.TabIndex = 12;
            this.btnLoad.Text = "Search";
            this.btnLoad.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // 
            // btnClearFilters
            // 
            this.btnClearFilters.Location = new System.Drawing.Point(190, 82);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(95, 28);
            this.btnClearFilters.TabIndex = 13;
            this.btnClearFilters.Text = "Clear";
            this.btnClearFilters.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(820, 14);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(105, 30);
            this.btnExport.TabIndex = 14;
            this.btnExport.Text = "Export";
            this.btnExport.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(935, 14);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.TabIndex = 15;
            this.btnClose.Text = "Close";
            this.btnClose.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // 
            // ultraPanelMaster
            // 
            this.ultraPanelMaster.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraPanelMaster.ClientArea.Controls.Add(this.gridReport);
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 119);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1349, 274);
            this.ultraPanelMaster.TabIndex = 1;

            // 
            // gridReport
            // 
            this.gridReport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridReport.Location = new System.Drawing.Point(12, 7);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1325, 261);
            this.gridReport.TabIndex = 0;
            this.gridReport.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // 
            // ultraPanelSummary
            // 
            this.ultraPanelSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalRemaining);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalRemainingCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalSettled);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalSettledCaption);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalAmount);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblTotalAmountCaption);
            this.ultraPanelSummary.Location = new System.Drawing.Point(0, 399);
            this.ultraPanelSummary.Name = "ultraPanelSummary";
            this.ultraPanelSummary.Size = new System.Drawing.Size(1349, 160);
            this.ultraPanelSummary.TabIndex = 2;

            // 
            // lblTotalAmountCaption
            // 
            this.lblTotalAmountCaption.Location = new System.Drawing.Point(20, 20);
            this.lblTotalAmountCaption.Name = "lblTotalAmountCaption";
            this.lblTotalAmountCaption.Size = new System.Drawing.Size(180, 25);
            this.lblTotalAmountCaption.TabIndex = 0;
            this.lblTotalAmountCaption.Text = "Total Amount:";

            // 
            // lblTotalAmount
            // 
            this.lblTotalAmount.Location = new System.Drawing.Point(20, 45);
            this.lblTotalAmount.Name = "lblTotalAmount";
            this.lblTotalAmount.Size = new System.Drawing.Size(180, 30);
            this.lblTotalAmount.TabIndex = 1;
            this.lblTotalAmount.Text = "₹ 0.00";

            // 
            // lblTotalSettledCaption
            // 
            this.lblTotalSettledCaption.Location = new System.Drawing.Point(220, 20);
            this.lblTotalSettledCaption.Name = "lblTotalSettledCaption";
            this.lblTotalSettledCaption.Size = new System.Drawing.Size(180, 25);
            this.lblTotalSettledCaption.TabIndex = 2;
            this.lblTotalSettledCaption.Text = "✅ Total Settled:";

            // 
            // lblTotalSettled
            // 
            this.lblTotalSettled.Location = new System.Drawing.Point(220, 45);
            this.lblTotalSettled.Name = "lblTotalSettled";
            this.lblTotalSettled.Size = new System.Drawing.Size(180, 30);
            this.lblTotalSettled.TabIndex = 3;
            this.lblTotalSettled.Text = "₹ 0.00";

            // 
            // lblTotalRemainingCaption
            // 
            this.lblTotalRemainingCaption.Location = new System.Drawing.Point(420, 20);
            this.lblTotalRemainingCaption.Name = "lblTotalRemainingCaption";
            this.lblTotalRemainingCaption.Size = new System.Drawing.Size(200, 25);
            this.lblTotalRemainingCaption.TabIndex = 4;
            this.lblTotalRemainingCaption.Text = "💰 Total Remaining:";

            // 
            // lblTotalRemaining
            // 
            this.lblTotalRemaining.Location = new System.Drawing.Point(420, 45);
            this.lblTotalRemaining.Name = "lblTotalRemaining";
            this.lblTotalRemaining.Size = new System.Drawing.Size(200, 30);
            this.lblTotalRemaining.TabIndex = 5;
            this.lblTotalRemaining.Text = "₹ 0.00";

            // 
            // FrmManualPartyBalanceReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(251)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(1349, 561);
            this.Controls.Add(this.ultraPanelSummary);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1024, 600);
            this.Name = "FrmManualPartyBalanceReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " Manual Party Balance Report";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbBalanceType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPartyType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkOpenOnly)).EndInit();

            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();

            this.ultraPanelSummary.ClientArea.ResumeLayout(false);
            this.ultraPanelSummary.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnLoad;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblPartyName;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbBalanceType;
        private Infragistics.Win.Misc.UltraLabel lblBalanceType;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbPartyType;
        private Infragistics.Win.Misc.UltraLabel lblPartyType;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraCheckEditor chkOpenOnly;

        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;

        private Infragistics.Win.Misc.UltraPanel ultraPanelSummary;
        private Infragistics.Win.Misc.UltraLabel lblTotalRemaining;
        private Infragistics.Win.Misc.UltraLabel lblTotalRemainingCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalSettled;
        private Infragistics.Win.Misc.UltraLabel lblTotalSettledCaption;
        private Infragistics.Win.Misc.UltraLabel lblTotalAmount;
        private Infragistics.Win.Misc.UltraLabel lblTotalAmountCaption;
    }
}
