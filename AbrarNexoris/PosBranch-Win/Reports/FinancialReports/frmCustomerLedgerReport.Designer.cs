using Infragistics.Win;

namespace PosBranch_Win.Reports.FinancialReports
{
    partial class frmCustomerLedgerReport
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

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
            Infragistics.Win.Appearance appearance10 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance11 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance12 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance13 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance14 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance15 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance16 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance17 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance18 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance19 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance20 = new Infragistics.Win.Appearance();
            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblPreset = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboPreset = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.btnSearch = new Infragistics.Win.Misc.UltraButton();
            this.btnReset = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.lblCustomer = new Infragistics.Win.Misc.UltraLabel();
            this.txtCustomerName = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.btnSelectCustomer = new Infragistics.Win.Misc.UltraButton();
            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelSummary = new Infragistics.Win.Misc.UltraPanel();
            this.pnlCardOpening = new Infragistics.Win.Misc.UltraPanel();
            this.lblOpeningVal = new Infragistics.Win.Misc.UltraLabel();
            this.lblOpeningCap = new Infragistics.Win.Misc.UltraLabel();
            this.pnlCardDebit = new Infragistics.Win.Misc.UltraPanel();
            this.lblDebitVal = new Infragistics.Win.Misc.UltraLabel();
            this.lblDebitCap = new Infragistics.Win.Misc.UltraLabel();
            this.pnlCardCredit = new Infragistics.Win.Misc.UltraPanel();
            this.lblCreditVal = new Infragistics.Win.Misc.UltraLabel();
            this.lblCreditCap = new Infragistics.Win.Misc.UltraLabel();
            this.pnlCardClosing = new Infragistics.Win.Misc.UltraPanel();
            this.lblClosingVal = new Infragistics.Win.Misc.UltraLabel();
            this.lblClosingCap = new Infragistics.Win.Misc.UltraLabel();
            this.lblStatus = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPreset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCustomerName)).BeginInit();
            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.ultraPanelSummary.ClientArea.SuspendLayout();
            this.ultraPanelSummary.SuspendLayout();
            this.pnlCardOpening.ClientArea.SuspendLayout();
            this.pnlCardOpening.SuspendLayout();
            this.pnlCardDebit.ClientArea.SuspendLayout();
            this.pnlCardDebit.SuspendLayout();
            this.pnlCardCredit.ClientArea.SuspendLayout();
            this.pnlCardCredit.SuspendLayout();
            this.pnlCardClosing.ClientArea.SuspendLayout();
            this.pnlCardClosing.SuspendLayout();
            this.SuspendLayout();
            // 
            // ultraPanelControls
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            appearance1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.ultraPanelControls.Appearance = appearance1;
            this.ultraPanelControls.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelControls.ClientArea
            // 
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtTo);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnReset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnExport);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnPrint);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnClose);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblCustomer);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtCustomerName);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnSelectCustomer);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1264, 100);
            this.ultraPanelControls.TabIndex = 0;
            // 
            // lblFromDate
            // 
            appearance2.FontData.Name = "Segoe UI";
            appearance2.FontData.SizeInPoints = 9F;
            appearance2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblFromDate.Appearance = appearance2;
            this.lblFromDate.Location = new System.Drawing.Point(15, 20);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(68, 23);
            this.lblFromDate.TabIndex = 0;
            this.lblFromDate.Text = "From Date:";
            // 
            // dtFrom
            // 
            this.dtFrom.Location = new System.Drawing.Point(88, 16);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(115, 25);
            this.dtFrom.TabIndex = 1;
            // 
            // lblToDate
            // 
            appearance3.FontData.Name = "Segoe UI";
            appearance3.FontData.SizeInPoints = 9F;
            appearance3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblToDate.Appearance = appearance3;
            this.lblToDate.Location = new System.Drawing.Point(215, 20);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(58, 23);
            this.lblToDate.TabIndex = 2;
            this.lblToDate.Text = "To Date:";
            // 
            // dtTo
            // 
            this.dtTo.Location = new System.Drawing.Point(278, 16);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(115, 25);
            this.dtTo.TabIndex = 2;
            // 
            // lblPreset
            // 
            appearance4.FontData.Name = "Segoe UI";
            appearance4.FontData.SizeInPoints = 9F;
            appearance4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblPreset.Appearance = appearance4;
            this.lblPreset.Location = new System.Drawing.Point(406, 20);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(48, 23);
            this.lblPreset.TabIndex = 3;
            this.lblPreset.Text = "Period:";
            // 
            // ultraComboPreset
            // 
            this.ultraComboPreset.Location = new System.Drawing.Point(458, 16);
            this.ultraComboPreset.Name = "ultraComboPreset";
            this.ultraComboPreset.Size = new System.Drawing.Size(125, 25);
            this.ultraComboPreset.TabIndex = 3;
            // 
            // lblSearch
            // 
            appearance5.FontData.Name = "Segoe UI";
            appearance5.FontData.SizeInPoints = 9F;
            appearance5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblSearch.Appearance = appearance5;
            this.lblSearch.Location = new System.Drawing.Point(597, 20);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(50, 23);
            this.lblSearch.TabIndex = 4;
            this.lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(650, 16);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(210, 25);
            this.txtSearch.TabIndex = 4;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(630, 56);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(100, 28);
            this.btnSearch.TabIndex = 7;
            this.btnSearch.Text = "Search  [F5]";
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(738, 56);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(82, 28);
            this.btnReset.TabIndex = 8;
            this.btnReset.Text = "↺  Reset";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(828, 56);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(95, 28);
            this.btnExport.TabIndex = 9;
            this.btnExport.Text = "⬇  Export CSV";
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(931, 56);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(78, 28);
            this.btnPrint.TabIndex = 10;
            this.btnPrint.Text = "🖨  Print";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(1017, 56);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(78, 28);
            this.btnClose.TabIndex = 11;
            this.btnClose.Text = "✕  Close";
            // 
            // lblCustomer
            // 
            appearance6.FontData.Name = "Segoe UI";
            appearance6.FontData.SizeInPoints = 9F;
            appearance6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblCustomer.Appearance = appearance6;
            this.lblCustomer.Location = new System.Drawing.Point(15, 62);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Size = new System.Drawing.Size(70, 23);
            this.lblCustomer.TabIndex = 5;
            this.lblCustomer.Text = "Customer:";
            // 
            // txtCustomerName
            // 
            this.txtCustomerName.Location = new System.Drawing.Point(88, 58);
            this.txtCustomerName.Name = "txtCustomerName";
            this.txtCustomerName.NullText = "Click \"Select Customer\" or press F3…";
            this.txtCustomerName.ReadOnly = true;
            this.txtCustomerName.Size = new System.Drawing.Size(380, 25);
            this.txtCustomerName.TabIndex = 5;
            // 
            // btnSelectCustomer
            // 
            this.btnSelectCustomer.Location = new System.Drawing.Point(475, 56);
            this.btnSelectCustomer.Name = "btnSelectCustomer";
            this.btnSelectCustomer.Size = new System.Drawing.Size(140, 30);
            this.btnSelectCustomer.TabIndex = 6;
            this.btnSelectCustomer.Text = "Select Customer";
            // 
            // ultraPanelMaster
            // 
            this.ultraPanelMaster.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;
            // 
            // ultraPanelMaster.ClientArea
            // 
            this.ultraPanelMaster.ClientArea.Controls.Add(this.gridReport);
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 100);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1264, 463);
            this.ultraPanelMaster.TabIndex = 1;
            // 
            // gridReport
            // 
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Location = new System.Drawing.Point(0, 0);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1264, 463);
            this.gridReport.TabIndex = 0;
            // 
            // ultraPanelSummary
            // 
            appearance7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            appearance7.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.ultraPanelSummary.Appearance = appearance7;
            this.ultraPanelSummary.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelSummary.ClientArea
            // 
            this.ultraPanelSummary.ClientArea.Controls.Add(this.pnlCardOpening);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.pnlCardDebit);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.pnlCardCredit);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.pnlCardClosing);
            this.ultraPanelSummary.ClientArea.Controls.Add(this.lblStatus);
            this.ultraPanelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelSummary.Location = new System.Drawing.Point(0, 563);
            this.ultraPanelSummary.Name = "ultraPanelSummary";
            this.ultraPanelSummary.Size = new System.Drawing.Size(1264, 98);
            this.ultraPanelSummary.TabIndex = 2;
            // 
            // pnlCardOpening
            // 
            appearance8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearance8.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.pnlCardOpening.Appearance = appearance8;
            this.pnlCardOpening.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // pnlCardOpening.ClientArea
            // 
            this.pnlCardOpening.ClientArea.Controls.Add(this.lblOpeningVal);
            this.pnlCardOpening.ClientArea.Controls.Add(this.lblOpeningCap);
            this.pnlCardOpening.Location = new System.Drawing.Point(15, 6);
            this.pnlCardOpening.Name = "pnlCardOpening";
            this.pnlCardOpening.Size = new System.Drawing.Size(238, 62);
            this.pnlCardOpening.TabIndex = 0;
            // 
            // lblOpeningVal
            // 
            appearance9.FontData.BoldAsString = "True";
            appearance9.FontData.Name = "Segoe UI";
            appearance9.FontData.SizeInPoints = 14F;
            appearance9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.lblOpeningVal.Appearance = appearance9;
            this.lblOpeningVal.Location = new System.Drawing.Point(12, 26);
            this.lblOpeningVal.Name = "lblOpeningVal";
            this.lblOpeningVal.Size = new System.Drawing.Size(210, 28);
            this.lblOpeningVal.TabIndex = 0;
            this.lblOpeningVal.Text = "–";
            // 
            // lblOpeningCap
            // 
            appearance10.FontData.BoldAsString = "True";
            appearance10.FontData.Name = "Segoe UI";
            appearance10.FontData.SizeInPoints = 7.5F;
            appearance10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblOpeningCap.Appearance = appearance10;
            this.lblOpeningCap.Location = new System.Drawing.Point(12, 8);
            this.lblOpeningCap.Name = "lblOpeningCap";
            this.lblOpeningCap.Size = new System.Drawing.Size(210, 15);
            this.lblOpeningCap.TabIndex = 1;
            this.lblOpeningCap.Text = "OPENING BALANCE";
            // 
            // pnlCardDebit
            // 
            appearance11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(253)))), ((int)(((byte)(244)))));
            appearance11.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.pnlCardDebit.Appearance = appearance11;
            this.pnlCardDebit.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // pnlCardDebit.ClientArea
            // 
            this.pnlCardDebit.ClientArea.Controls.Add(this.lblDebitVal);
            this.pnlCardDebit.ClientArea.Controls.Add(this.lblDebitCap);
            this.pnlCardDebit.Location = new System.Drawing.Point(265, 6);
            this.pnlCardDebit.Name = "pnlCardDebit";
            this.pnlCardDebit.Size = new System.Drawing.Size(238, 62);
            this.pnlCardDebit.TabIndex = 1;
            // 
            // lblDebitVal
            // 
            appearance12.FontData.BoldAsString = "True";
            appearance12.FontData.Name = "Segoe UI";
            appearance12.FontData.SizeInPoints = 14F;
            appearance12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(128)))), ((int)(((byte)(61)))));
            this.lblDebitVal.Appearance = appearance12;
            this.lblDebitVal.Location = new System.Drawing.Point(12, 26);
            this.lblDebitVal.Name = "lblDebitVal";
            this.lblDebitVal.Size = new System.Drawing.Size(210, 28);
            this.lblDebitVal.TabIndex = 0;
            this.lblDebitVal.Text = "–";
            // 
            // lblDebitCap
            // 
            appearance13.FontData.BoldAsString = "True";
            appearance13.FontData.Name = "Segoe UI";
            appearance13.FontData.SizeInPoints = 7.5F;
            appearance13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblDebitCap.Appearance = appearance13;
            this.lblDebitCap.Location = new System.Drawing.Point(12, 8);
            this.lblDebitCap.Name = "lblDebitCap";
            this.lblDebitCap.Size = new System.Drawing.Size(210, 15);
            this.lblDebitCap.TabIndex = 1;
            this.lblDebitCap.Text = "TOTAL DEBIT (Dr)";
            // 
            // pnlCardCredit
            // 
            appearance14.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(241)))), ((int)(((byte)(242)))));
            appearance14.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.pnlCardCredit.Appearance = appearance14;
            this.pnlCardCredit.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // pnlCardCredit.ClientArea
            // 
            this.pnlCardCredit.ClientArea.Controls.Add(this.lblCreditVal);
            this.pnlCardCredit.ClientArea.Controls.Add(this.lblCreditCap);
            this.pnlCardCredit.Location = new System.Drawing.Point(515, 6);
            this.pnlCardCredit.Name = "pnlCardCredit";
            this.pnlCardCredit.Size = new System.Drawing.Size(238, 62);
            this.pnlCardCredit.TabIndex = 2;
            // 
            // lblCreditVal
            // 
            appearance15.FontData.BoldAsString = "True";
            appearance15.FontData.Name = "Segoe UI";
            appearance15.FontData.SizeInPoints = 14F;
            appearance15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.lblCreditVal.Appearance = appearance15;
            this.lblCreditVal.Location = new System.Drawing.Point(12, 26);
            this.lblCreditVal.Name = "lblCreditVal";
            this.lblCreditVal.Size = new System.Drawing.Size(210, 28);
            this.lblCreditVal.TabIndex = 0;
            this.lblCreditVal.Text = "–";
            // 
            // lblCreditCap
            // 
            appearance16.FontData.BoldAsString = "True";
            appearance16.FontData.Name = "Segoe UI";
            appearance16.FontData.SizeInPoints = 7.5F;
            appearance16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblCreditCap.Appearance = appearance16;
            this.lblCreditCap.Location = new System.Drawing.Point(12, 8);
            this.lblCreditCap.Name = "lblCreditCap";
            this.lblCreditCap.Size = new System.Drawing.Size(210, 15);
            this.lblCreditCap.TabIndex = 1;
            this.lblCreditCap.Text = "TOTAL CREDIT (Cr)";
            // 
            // pnlCardClosing
            // 
            appearance17.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(252)))), ((int)(((byte)(232)))));
            appearance17.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.pnlCardClosing.Appearance = appearance17;
            this.pnlCardClosing.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // pnlCardClosing.ClientArea
            // 
            this.pnlCardClosing.ClientArea.Controls.Add(this.lblClosingVal);
            this.pnlCardClosing.ClientArea.Controls.Add(this.lblClosingCap);
            this.pnlCardClosing.Location = new System.Drawing.Point(765, 6);
            this.pnlCardClosing.Name = "pnlCardClosing";
            this.pnlCardClosing.Size = new System.Drawing.Size(238, 62);
            this.pnlCardClosing.TabIndex = 3;
            // 
            // lblClosingVal
            // 
            appearance18.FontData.BoldAsString = "True";
            appearance18.FontData.Name = "Segoe UI";
            appearance18.FontData.SizeInPoints = 14F;
            appearance18.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(161)))), ((int)(((byte)(98)))), ((int)(((byte)(7)))));
            this.lblClosingVal.Appearance = appearance18;
            this.lblClosingVal.Location = new System.Drawing.Point(12, 26);
            this.lblClosingVal.Name = "lblClosingVal";
            this.lblClosingVal.Size = new System.Drawing.Size(210, 28);
            this.lblClosingVal.TabIndex = 0;
            this.lblClosingVal.Text = "–";
            // 
            // lblClosingCap
            // 
            appearance19.FontData.BoldAsString = "True";
            appearance19.FontData.Name = "Segoe UI";
            appearance19.FontData.SizeInPoints = 7.5F;
            appearance19.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblClosingCap.Appearance = appearance19;
            this.lblClosingCap.Location = new System.Drawing.Point(12, 8);
            this.lblClosingCap.Name = "lblClosingCap";
            this.lblClosingCap.Size = new System.Drawing.Size(210, 15);
            this.lblClosingCap.TabIndex = 1;
            this.lblClosingCap.Text = "CLOSING BALANCE";
            // 
            // lblStatus
            // 
            appearance20.FontData.Name = "Segoe UI";
            appearance20.FontData.SizeInPoints = 8F;
            appearance20.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblStatus.Appearance = appearance20;
            this.lblStatus.Location = new System.Drawing.Point(15, 74);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(1200, 18);
            this.lblStatus.TabIndex = 10;
            this.lblStatus.Text = "Ready. Select a customer to view the ledger.";
            // 
            // frmCustomerLedgerReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 661);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelSummary);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "frmCustomerLedgerReport";
            this.Text = "Customer Ledger Statement";
            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPreset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCustomerName)).EndInit();
            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.ultraPanelSummary.ClientArea.ResumeLayout(false);
            this.ultraPanelSummary.ResumeLayout(false);
            this.pnlCardOpening.ClientArea.ResumeLayout(false);
            this.pnlCardOpening.ResumeLayout(false);
            this.pnlCardDebit.ClientArea.ResumeLayout(false);
            this.pnlCardDebit.ResumeLayout(false);
            this.pnlCardCredit.ClientArea.ResumeLayout(false);
            this.pnlCardCredit.ResumeLayout(false);
            this.pnlCardClosing.ClientArea.ResumeLayout(false);
            this.pnlCardClosing.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel   ultraPanelControls;
        private Infragistics.Win.Misc.UltraButton  btnReset;
        private Infragistics.Win.Misc.UltraButton  btnSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor   txtSearch;
        private Infragistics.Win.Misc.UltraLabel   lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor  ultraComboPreset;
        private Infragistics.Win.Misc.UltraLabel   lblPreset;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor   txtCustomerName;
        private Infragistics.Win.Misc.UltraButton  btnSelectCustomer;
        private Infragistics.Win.Misc.UltraLabel   lblCustomer;
        private Infragistics.Win.Misc.UltraButton  btnExport;
        private Infragistics.Win.Misc.UltraButton  btnPrint;
        private Infragistics.Win.Misc.UltraButton  btnClose;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.Misc.UltraLabel   lblFromDate;
        private Infragistics.Win.Misc.UltraLabel   lblToDate;

        private Infragistics.Win.Misc.UltraPanel   ultraPanelMaster;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;

        private Infragistics.Win.Misc.UltraPanel   ultraPanelSummary;
        private Infragistics.Win.Misc.UltraLabel   lblStatus;

        private Infragistics.Win.Misc.UltraPanel   pnlCardOpening;
        private Infragistics.Win.Misc.UltraLabel   lblOpeningVal;
        private Infragistics.Win.Misc.UltraLabel   lblOpeningCap;

        private Infragistics.Win.Misc.UltraPanel   pnlCardDebit;
        private Infragistics.Win.Misc.UltraLabel   lblDebitVal;
        private Infragistics.Win.Misc.UltraLabel   lblDebitCap;

        private Infragistics.Win.Misc.UltraPanel   pnlCardCredit;
        private Infragistics.Win.Misc.UltraLabel   lblCreditVal;
        private Infragistics.Win.Misc.UltraLabel   lblCreditCap;

        private Infragistics.Win.Misc.UltraPanel   pnlCardClosing;
        private Infragistics.Win.Misc.UltraLabel   lblClosingVal;
        private Infragistics.Win.Misc.UltraLabel   lblClosingCap;
    }
}
