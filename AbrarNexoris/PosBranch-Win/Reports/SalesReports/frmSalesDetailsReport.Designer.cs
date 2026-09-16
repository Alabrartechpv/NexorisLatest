using System;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    partial class frmSalesReportMasterDetail
    {
        private IContainer components = null;

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
            Infragistics.Win.Appearance appearanceSelection = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceActionBar = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGridPanel = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceFooter = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceSummaryCards = new Infragistics.Win.Appearance();

            Infragistics.Win.Appearance appCardBills = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appCardQty = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appCardSubTotal = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appCardTax = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appCardNet = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appCardProfit = new Infragistics.Win.Appearance();

            this.ultraPanelSelection = new Infragistics.Win.Misc.UltraPanel();
            this.lblDate = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboDateMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFromDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtToDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblPaymentMode = new Infragistics.Win.Misc.UltraLabel();
            this.cmbPaymentMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSalesType = new Infragistics.Win.Misc.UltraLabel();
            this.cmbSalesType = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCustomer = new Infragistics.Win.Misc.UltraLabel();
            this.cmbCustomer = new Infragistics.Win.UltraWinEditors.UltraComboEditor();

            this.ultraPanelActionBar = new Infragistics.Win.Misc.UltraPanel();
            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewReport = new Infragistics.Win.Misc.UltraButton();
            this.btnExportExcel = new Infragistics.Win.Misc.UltraButton();
            this.btnColumnChooser = new Infragistics.Win.Misc.UltraButton();
            this.btnHideSelection = new Infragistics.Win.Misc.UltraButton();

            this.ultraPanelSummaryCards = new Infragistics.Win.Misc.UltraPanel();
            this.pnlCardBills = new Infragistics.Win.Misc.UltraPanel();
            this.lblCardBillsTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblCardBillsValue = new Infragistics.Win.Misc.UltraLabel();

            this.pnlCardQty = new Infragistics.Win.Misc.UltraPanel();
            this.lblCardQtyTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblCardQtyValue = new Infragistics.Win.Misc.UltraLabel();

            this.pnlCardSubTotal = new Infragistics.Win.Misc.UltraPanel();
            this.lblCardSubTotalTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblCardSubTotalValue = new Infragistics.Win.Misc.UltraLabel();

            this.pnlCardTax = new Infragistics.Win.Misc.UltraPanel();
            this.lblCardTaxTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblCardTaxValue = new Infragistics.Win.Misc.UltraLabel();

            this.pnlCardNet = new Infragistics.Win.Misc.UltraPanel();
            this.lblCardNetTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblCardNetValue = new Infragistics.Win.Misc.UltraLabel();

            this.pnlCardProfit = new Infragistics.Win.Misc.UltraPanel();
            this.lblCardProfitTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblCardProfitValue = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridMaster = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.gridFooterPanel = new Infragistics.Win.Misc.UltraPanel();
            this.lblCount = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanelSelection.ClientArea.SuspendLayout();
            this.ultraPanelSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymentMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbSalesType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCustomer)).BeginInit();

            this.ultraPanelActionBar.ClientArea.SuspendLayout();
            this.ultraPanelActionBar.SuspendLayout();

            this.ultraPanelSummaryCards.ClientArea.SuspendLayout();
            this.ultraPanelSummaryCards.SuspendLayout();
            this.pnlCardBills.ClientArea.SuspendLayout();
            this.pnlCardBills.SuspendLayout();
            this.pnlCardQty.ClientArea.SuspendLayout();
            this.pnlCardQty.SuspendLayout();
            this.pnlCardSubTotal.ClientArea.SuspendLayout();
            this.pnlCardSubTotal.SuspendLayout();
            this.pnlCardTax.ClientArea.SuspendLayout();
            this.pnlCardTax.SuspendLayout();
            this.pnlCardNet.ClientArea.SuspendLayout();
            this.pnlCardNet.SuspendLayout();
            this.pnlCardProfit.ClientArea.SuspendLayout();
            this.pnlCardProfit.SuspendLayout();

            this.ultraPanelGrid.ClientArea.SuspendLayout();
            this.ultraPanelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridMaster)).BeginInit();

            this.gridFooterPanel.ClientArea.SuspendLayout();
            this.gridFooterPanel.SuspendLayout();
            this.SuspendLayout();

            // 
            // ultraPanelSelection
            // 
            appearanceSelection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearanceSelection.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelSelection.Appearance = appearanceSelection;
            this.ultraPanelSelection.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelSelection.ClientArea
            // 
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.ultraComboDateMode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.dtFromDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.dtToDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblPaymentMode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbPaymentMode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblSalesType);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbSalesType);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblCustomer);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbCustomer);
            this.ultraPanelSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelSelection.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelSelection.Name = "ultraPanelSelection";
            this.ultraPanelSelection.Size = new System.Drawing.Size(1280, 80);
            this.ultraPanelSelection.TabIndex = 0;

            // 
            // lblDate
            // 
            this.lblDate.Location = new System.Drawing.Point(15, 14);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(75, 20);
            this.lblDate.TabIndex = 0;
            this.lblDate.Text = "Date";

            // 
            // ultraComboDateMode
            // 
            this.ultraComboDateMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboDateMode.Location = new System.Drawing.Point(95, 11);
            this.ultraComboDateMode.Name = "ultraComboDateMode";
            this.ultraComboDateMode.Size = new System.Drawing.Size(120, 22);
            this.ultraComboDateMode.TabIndex = 1;
            this.ultraComboDateMode.ValueChanged += new System.EventHandler(this.UltraComboDateMode_ValueChanged);

            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(230, 14);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(65, 20);
            this.lblFromDate.TabIndex = 2;
            this.lblFromDate.Text = "From Date";

            // 
            // dtFromDate
            // 
            this.dtFromDate.DateTime = new global::System.DateTime(2026, 1, 1, 0, 0, 0, 0);
            this.dtFromDate.FormatString = "dd/MM/yyyy";
            this.dtFromDate.Location = new System.Drawing.Point(300, 11);
            this.dtFromDate.Name = "dtFromDate";
            this.dtFromDate.Size = new System.Drawing.Size(115, 22);
            this.dtFromDate.TabIndex = 3;
            this.dtFromDate.Value = new global::System.DateTime(2026, 1, 1, 0, 0, 0, 0);

            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(430, 14);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(55, 20);
            this.lblToDate.TabIndex = 4;
            this.lblToDate.Text = "To Date";

            // 
            // dtToDate
            // 
            this.dtToDate.DateTime = new global::System.DateTime(2026, 1, 1, 0, 0, 0, 0);
            this.dtToDate.FormatString = "dd/MM/yyyy";
            this.dtToDate.Location = new System.Drawing.Point(490, 11);
            this.dtToDate.Name = "dtToDate";
            this.dtToDate.Size = new System.Drawing.Size(115, 22);
            this.dtToDate.TabIndex = 5;
            this.dtToDate.Value = new global::System.DateTime(2026, 1, 1, 0, 0, 0, 0);

            // 
            // lblPaymentMode
            // 
            this.lblPaymentMode.Location = new System.Drawing.Point(15, 44);
            this.lblPaymentMode.Name = "lblPaymentMode";
            this.lblPaymentMode.Size = new System.Drawing.Size(90, 20);
            this.lblPaymentMode.TabIndex = 6;
            this.lblPaymentMode.Text = "Payment Mode";

            // 
            // cmbPaymentMode
            // 
            this.cmbPaymentMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbPaymentMode.Location = new System.Drawing.Point(110, 41);
            this.cmbPaymentMode.Name = "cmbPaymentMode";
            this.cmbPaymentMode.Size = new System.Drawing.Size(160, 22);
            this.cmbPaymentMode.TabIndex = 7;

            // 
            // lblSalesType
            // 
            this.lblSalesType.Location = new System.Drawing.Point(290, 44);
            this.lblSalesType.Name = "lblSalesType";
            this.lblSalesType.Size = new System.Drawing.Size(75, 20);
            this.lblSalesType.TabIndex = 8;
            this.lblSalesType.Text = "Sales Type";

            // 
            // cmbSalesType
            // 
            this.cmbSalesType.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbSalesType.Location = new System.Drawing.Point(370, 41);
            this.cmbSalesType.Name = "cmbSalesType";
            this.cmbSalesType.Size = new System.Drawing.Size(160, 22);
            this.cmbSalesType.TabIndex = 9;

            // 
            // lblCustomer
            // 
            this.lblCustomer.Location = new System.Drawing.Point(550, 44);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Size = new System.Drawing.Size(65, 20);
            this.lblCustomer.TabIndex = 10;
            this.lblCustomer.Text = "Customer";

            // 
            // cmbCustomer
            // 
            this.cmbCustomer.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbCustomer.Location = new System.Drawing.Point(620, 41);
            this.cmbCustomer.Name = "cmbCustomer";
            this.cmbCustomer.Size = new System.Drawing.Size(220, 22);
            this.cmbCustomer.TabIndex = 11;

            // 
            // ultraPanelActionBar
            // 
            appearanceActionBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearanceActionBar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelActionBar.Appearance = appearanceActionBar;
            this.ultraPanelActionBar.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelActionBar.ClientArea
            // 
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnViewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewReport);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnExportExcel);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnColumnChooser);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnHideSelection);
            this.ultraPanelActionBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelActionBar.Location = new System.Drawing.Point(0, 80);
            this.ultraPanelActionBar.Name = "ultraPanelActionBar";
            this.ultraPanelActionBar.Size = new System.Drawing.Size(1280, 42);
            this.ultraPanelActionBar.TabIndex = 1;

            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Location = new System.Drawing.Point(15, 6);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(125, 30);
            this.btnViewGrid.TabIndex = 0;
            this.btnViewGrid.Text = "View Grid";
            this.btnViewGrid.Click += new System.EventHandler(this.BtnViewGrid_Click);

            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(150, 6);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(125, 30);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid";
            this.btnPreviewGrid.Click += new System.EventHandler(this.BtnPreviewGrid_Click);

            // 
            // btnPreviewReport
            // 
            this.btnPreviewReport.Location = new System.Drawing.Point(285, 6);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(125, 30);
            this.btnPreviewReport.TabIndex = 2;
            this.btnPreviewReport.Text = "Preview Report";
            this.btnPreviewReport.Click += new System.EventHandler(this.BtnPreviewReport_Click);

            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Location = new System.Drawing.Point(420, 6);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(125, 30);
            this.btnExportExcel.TabIndex = 3;
            this.btnExportExcel.Text = "Export Excel";
            this.btnExportExcel.Click += new System.EventHandler(this.BtnExportExcel_Click);

            // 
            // btnColumnChooser
            // 
            this.btnColumnChooser.Location = new System.Drawing.Point(555, 6);
            this.btnColumnChooser.Name = "btnColumnChooser";
            this.btnColumnChooser.Size = new System.Drawing.Size(125, 30);
            this.btnColumnChooser.TabIndex = 4;
            this.btnColumnChooser.Text = "Column Chooser";
            this.btnColumnChooser.Click += new System.EventHandler(this.BtnColumnChooser_Click);

            // 
            // btnHideSelection
            // 
            this.btnHideSelection.Location = new System.Drawing.Point(690, 6);
            this.btnHideSelection.Name = "btnHideSelection";
            this.btnHideSelection.Size = new System.Drawing.Size(125, 30);
            this.btnHideSelection.TabIndex = 5;
            this.btnHideSelection.Text = "Hide Selection";
            this.btnHideSelection.Click += new System.EventHandler(this.BtnHideSelection_Click);

            // 
            // ultraPanelSummaryCards
            // 
            appearanceSummaryCards.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(246)))), ((int)(((byte)(251)))));
            appearanceSummaryCards.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(205)))), ((int)(((byte)(225)))));
            this.ultraPanelSummaryCards.Appearance = appearanceSummaryCards;
            this.ultraPanelSummaryCards.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelSummaryCards.ClientArea
            // 
            this.ultraPanelSummaryCards.ClientArea.Controls.Add(this.pnlCardBills);
            this.ultraPanelSummaryCards.ClientArea.Controls.Add(this.pnlCardQty);
            this.ultraPanelSummaryCards.ClientArea.Controls.Add(this.pnlCardSubTotal);
            this.ultraPanelSummaryCards.ClientArea.Controls.Add(this.pnlCardTax);
            this.ultraPanelSummaryCards.ClientArea.Controls.Add(this.pnlCardNet);
            this.ultraPanelSummaryCards.ClientArea.Controls.Add(this.pnlCardProfit);
            this.ultraPanelSummaryCards.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelSummaryCards.Location = new System.Drawing.Point(0, 642);
            this.ultraPanelSummaryCards.Name = "ultraPanelSummaryCards";
            this.ultraPanelSummaryCards.Size = new System.Drawing.Size(1280, 78);
            this.ultraPanelSummaryCards.TabIndex = 3;
            this.ultraPanelSummaryCards.ClientArea.Resize += new System.EventHandler(this.SummaryCards_Resize);

            // 
            // pnlCardBills
            // 
            appCardBills.BackColor = System.Drawing.Color.White;
            appCardBills.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(197)))), ((int)(((byte)(253)))));
            this.pnlCardBills.Appearance = appCardBills;
            this.pnlCardBills.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.pnlCardBills.ClientArea.Controls.Add(this.lblCardBillsTitle);
            this.pnlCardBills.ClientArea.Controls.Add(this.lblCardBillsValue);
            this.pnlCardBills.Location = new System.Drawing.Point(12, 7);
            this.pnlCardBills.Name = "pnlCardBills";
            this.pnlCardBills.Size = new System.Drawing.Size(185, 58);
            this.pnlCardBills.TabIndex = 0;

            // 
            // lblCardBillsTitle
            // 
            this.lblCardBillsTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardBillsTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblCardBillsTitle.Location = new System.Drawing.Point(8, 5);
            this.lblCardBillsTitle.Name = "lblCardBillsTitle";
            this.lblCardBillsTitle.Size = new System.Drawing.Size(170, 16);
            this.lblCardBillsTitle.TabIndex = 0;
            this.lblCardBillsTitle.Text = "📋 TOTAL BILLS";

            // 
            // lblCardBillsValue
            // 
            this.lblCardBillsValue.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCardBillsValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(138)))));
            this.lblCardBillsValue.Location = new System.Drawing.Point(8, 24);
            this.lblCardBillsValue.Name = "lblCardBillsValue";
            this.lblCardBillsValue.Size = new System.Drawing.Size(170, 26);
            this.lblCardBillsValue.TabIndex = 1;
            this.lblCardBillsValue.Text = "0";

            // 
            // pnlCardQty
            // 
            appCardQty.BackColor = System.Drawing.Color.White;
            appCardQty.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.pnlCardQty.Appearance = appCardQty;
            this.pnlCardQty.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.pnlCardQty.ClientArea.Controls.Add(this.lblCardQtyTitle);
            this.pnlCardQty.ClientArea.Controls.Add(this.lblCardQtyValue);
            this.pnlCardQty.Location = new System.Drawing.Point(205, 7);
            this.pnlCardQty.Name = "pnlCardQty";
            this.pnlCardQty.Size = new System.Drawing.Size(185, 58);
            this.pnlCardQty.TabIndex = 1;

            // 
            // lblCardQtyTitle
            // 
            this.lblCardQtyTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardQtyTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblCardQtyTitle.Location = new System.Drawing.Point(8, 5);
            this.lblCardQtyTitle.Name = "lblCardQtyTitle";
            this.lblCardQtyTitle.Size = new System.Drawing.Size(170, 16);
            this.lblCardQtyTitle.TabIndex = 0;
            this.lblCardQtyTitle.Text = "📦 TOTAL QTY";

            // 
            // lblCardQtyValue
            // 
            this.lblCardQtyValue.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCardQtyValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblCardQtyValue.Location = new System.Drawing.Point(8, 24);
            this.lblCardQtyValue.Name = "lblCardQtyValue";
            this.lblCardQtyValue.Size = new System.Drawing.Size(170, 26);
            this.lblCardQtyValue.TabIndex = 1;
            this.lblCardQtyValue.Text = "0.00";

            // 
            // pnlCardSubTotal
            // 
            appCardSubTotal.BackColor = System.Drawing.Color.White;
            appCardSubTotal.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(243)))), ((int)(((byte)(208)))));
            this.pnlCardSubTotal.Appearance = appCardSubTotal;
            this.pnlCardSubTotal.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.pnlCardSubTotal.ClientArea.Controls.Add(this.lblCardSubTotalTitle);
            this.pnlCardSubTotal.ClientArea.Controls.Add(this.lblCardSubTotalValue);
            this.pnlCardSubTotal.Location = new System.Drawing.Point(398, 7);
            this.pnlCardSubTotal.Name = "pnlCardSubTotal";
            this.pnlCardSubTotal.Size = new System.Drawing.Size(185, 58);
            this.pnlCardSubTotal.TabIndex = 2;

            // 
            // lblCardSubTotalTitle
            // 
            this.lblCardSubTotalTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardSubTotalTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblCardSubTotalTitle.Location = new System.Drawing.Point(8, 5);
            this.lblCardSubTotalTitle.Name = "lblCardSubTotalTitle";
            this.lblCardSubTotalTitle.Size = new System.Drawing.Size(170, 16);
            this.lblCardSubTotalTitle.TabIndex = 0;
            this.lblCardSubTotalTitle.Text = "💰 SUB TOTAL";

            // 
            // lblCardSubTotalValue
            // 
            this.lblCardSubTotalValue.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCardSubTotalValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(148)))), ((int)(((byte)(136)))));
            this.lblCardSubTotalValue.Location = new System.Drawing.Point(8, 24);
            this.lblCardSubTotalValue.Name = "lblCardSubTotalValue";
            this.lblCardSubTotalValue.Size = new System.Drawing.Size(170, 26);
            this.lblCardSubTotalValue.TabIndex = 1;
            this.lblCardSubTotalValue.Text = "₹ 0.00";

            // 
            // pnlCardTax
            // 
            appCardTax.BackColor = System.Drawing.Color.White;
            appCardTax.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(215)))), ((int)(((byte)(170)))));
            this.pnlCardTax.Appearance = appCardTax;
            this.pnlCardTax.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.pnlCardTax.ClientArea.Controls.Add(this.lblCardTaxTitle);
            this.pnlCardTax.ClientArea.Controls.Add(this.lblCardTaxValue);
            this.pnlCardTax.Location = new System.Drawing.Point(591, 7);
            this.pnlCardTax.Name = "pnlCardTax";
            this.pnlCardTax.Size = new System.Drawing.Size(185, 58);
            this.pnlCardTax.TabIndex = 3;

            // 
            // lblCardTaxTitle
            // 
            this.lblCardTaxTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardTaxTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblCardTaxTitle.Location = new System.Drawing.Point(8, 5);
            this.lblCardTaxTitle.Name = "lblCardTaxTitle";
            this.lblCardTaxTitle.Size = new System.Drawing.Size(170, 16);
            this.lblCardTaxTitle.TabIndex = 0;
            this.lblCardTaxTitle.Text = "🧾 TAX TOTAL";

            // 
            // lblCardTaxValue
            // 
            this.lblCardTaxValue.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCardTaxValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(88)))), ((int)(((byte)(12)))));
            this.lblCardTaxValue.Location = new System.Drawing.Point(8, 24);
            this.lblCardTaxValue.Name = "lblCardTaxValue";
            this.lblCardTaxValue.Size = new System.Drawing.Size(170, 26);
            this.lblCardTaxValue.TabIndex = 1;
            this.lblCardTaxValue.Text = "₹ 0.00";

            // 
            // pnlCardNet
            // 
            appCardNet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(253)))), ((int)(((byte)(244)))));
            appCardNet.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(197)))), ((int)(((byte)(94)))));
            this.pnlCardNet.Appearance = appCardNet;
            this.pnlCardNet.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.pnlCardNet.ClientArea.Controls.Add(this.lblCardNetTitle);
            this.pnlCardNet.ClientArea.Controls.Add(this.lblCardNetValue);
            this.pnlCardNet.Location = new System.Drawing.Point(784, 7);
            this.pnlCardNet.Name = "pnlCardNet";
            this.pnlCardNet.Size = new System.Drawing.Size(195, 58);
            this.pnlCardNet.TabIndex = 4;

            // 
            // lblCardNetTitle
            // 
            this.lblCardNetTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardNetTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(128)))), ((int)(((byte)(61)))));
            this.lblCardNetTitle.Location = new System.Drawing.Point(8, 5);
            this.lblCardNetTitle.Name = "lblCardNetTitle";
            this.lblCardNetTitle.Size = new System.Drawing.Size(180, 16);
            this.lblCardNetTitle.TabIndex = 0;
            this.lblCardNetTitle.Text = "💵 NET TOTAL";

            // 
            // lblCardNetValue
            // 
            this.lblCardNetValue.Font = new System.Drawing.Font("Segoe UI", 12.5F, System.Drawing.FontStyle.Bold);
            this.lblCardNetValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(101)))), ((int)(((byte)(52)))));
            this.lblCardNetValue.Location = new System.Drawing.Point(8, 24);
            this.lblCardNetValue.Name = "lblCardNetValue";
            this.lblCardNetValue.Size = new System.Drawing.Size(180, 26);
            this.lblCardNetValue.TabIndex = 1;
            this.lblCardNetValue.Text = "₹ 0.00";

            // 
            // pnlCardProfit
            // 
            appCardProfit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            appCardProfit.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(92)))), ((int)(((byte)(246)))));
            this.pnlCardProfit.Appearance = appCardProfit;
            this.pnlCardProfit.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.pnlCardProfit.ClientArea.Controls.Add(this.lblCardProfitTitle);
            this.pnlCardProfit.ClientArea.Controls.Add(this.lblCardProfitValue);
            this.pnlCardProfit.Location = new System.Drawing.Point(987, 7);
            this.pnlCardProfit.Name = "pnlCardProfit";
            this.pnlCardProfit.Size = new System.Drawing.Size(195, 58);
            this.pnlCardProfit.TabIndex = 5;

            // 
            // lblCardProfitTitle
            // 
            this.lblCardProfitTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardProfitTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(58)))), ((int)(((byte)(237)))));
            this.lblCardProfitTitle.Location = new System.Drawing.Point(8, 5);
            this.lblCardProfitTitle.Name = "lblCardProfitTitle";
            this.lblCardProfitTitle.Size = new System.Drawing.Size(180, 16);
            this.lblCardProfitTitle.TabIndex = 0;
            this.lblCardProfitTitle.Text = "📈 TOTAL PROFIT";

            // 
            // lblCardProfitValue
            // 
            this.lblCardProfitValue.Font = new System.Drawing.Font("Segoe UI", 12.5F, System.Drawing.FontStyle.Bold);
            this.lblCardProfitValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(40)))), ((int)(((byte)(217)))));
            this.lblCardProfitValue.Location = new System.Drawing.Point(8, 24);
            this.lblCardProfitValue.Name = "lblCardProfitValue";
            this.lblCardProfitValue.Size = new System.Drawing.Size(180, 26);
            this.lblCardProfitValue.TabIndex = 1;
            this.lblCardProfitValue.Text = "₹ 0.00";

            // 
            // ultraPanelGrid
            // 
            appearanceGridPanel.BackColor = System.Drawing.Color.White;
            this.ultraPanelGrid.Appearance = appearanceGridPanel;
            // 
            // ultraPanelGrid.ClientArea
            // 
            this.ultraPanelGrid.ClientArea.Controls.Add(this.ultraGridMaster);
            this.ultraPanelGrid.ClientArea.Controls.Add(this.gridFooterPanel);
            this.ultraPanelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelGrid.Location = new System.Drawing.Point(0, 122);
            this.ultraPanelGrid.Name = "ultraPanelGrid";
            this.ultraPanelGrid.Size = new System.Drawing.Size(1280, 524);
            this.ultraPanelGrid.TabIndex = 2;

            // 
            // ultraGridMaster
            // 
            this.ultraGridMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridMaster.Location = new System.Drawing.Point(0, 0);
            this.ultraGridMaster.Name = "ultraGridMaster";
            this.ultraGridMaster.Size = new System.Drawing.Size(1280, 496);
            this.ultraGridMaster.TabIndex = 0;

            // 
            // gridFooterPanel
            // 
            appearanceFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(248)))));
            appearanceFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(198)))), ((int)(((byte)(220)))));
            this.gridFooterPanel.Appearance = appearanceFooter;
            this.gridFooterPanel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // gridFooterPanel.ClientArea
            // 
            this.gridFooterPanel.ClientArea.Controls.Add(this.lblCount);
            this.gridFooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gridFooterPanel.Location = new System.Drawing.Point(0, 492);
            this.gridFooterPanel.Name = "gridFooterPanel";
            this.gridFooterPanel.Size = new System.Drawing.Size(1280, 32);
            this.gridFooterPanel.TabIndex = 1;

            // 
            // lblCount
            // 
            this.lblCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCount.Location = new System.Drawing.Point(6, 4);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(160, 20);
            this.lblCount.TabIndex = 0;
            this.lblCount.Text = "Total Records: 0";

            // 
            // frmSalesReportMasterDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.ultraPanelGrid);
            this.Controls.Add(this.ultraPanelSummaryCards);
            this.Controls.Add(this.ultraPanelActionBar);
            this.Controls.Add(this.ultraPanelSelection);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1024, 600);
            this.Name = "frmSalesReportMasterDetail";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sales Listing By Date Range";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ultraPanelSelection.ClientArea.ResumeLayout(false);
            this.ultraPanelSelection.ClientArea.PerformLayout();
            this.ultraPanelSelection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymentMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbSalesType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCustomer)).EndInit();

            this.ultraPanelActionBar.ClientArea.ResumeLayout(false);
            this.ultraPanelActionBar.ResumeLayout(false);

            this.ultraPanelSummaryCards.ClientArea.ResumeLayout(false);
            this.ultraPanelSummaryCards.ResumeLayout(false);
            this.pnlCardBills.ClientArea.ResumeLayout(false);
            this.pnlCardBills.ResumeLayout(false);
            this.pnlCardQty.ClientArea.ResumeLayout(false);
            this.pnlCardQty.ResumeLayout(false);
            this.pnlCardSubTotal.ClientArea.ResumeLayout(false);
            this.pnlCardSubTotal.ResumeLayout(false);
            this.pnlCardTax.ClientArea.ResumeLayout(false);
            this.pnlCardTax.ResumeLayout(false);
            this.pnlCardNet.ClientArea.ResumeLayout(false);
            this.pnlCardNet.ResumeLayout(false);
            this.pnlCardProfit.ClientArea.ResumeLayout(false);
            this.pnlCardProfit.ResumeLayout(false);

            this.ultraPanelGrid.ClientArea.ResumeLayout(false);
            this.ultraPanelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridMaster)).EndInit();

            this.gridFooterPanel.ClientArea.ResumeLayout(false);
            this.gridFooterPanel.ResumeLayout(false);

            this.ResumeLayout(false);
        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelSelection;
        private Infragistics.Win.Misc.UltraLabel lblDate;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboDateMode;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFromDate;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtToDate;
        private Infragistics.Win.Misc.UltraLabel lblPaymentMode;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbPaymentMode;
        private Infragistics.Win.Misc.UltraLabel lblSalesType;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbSalesType;
        private Infragistics.Win.Misc.UltraLabel lblCustomer;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbCustomer;

        private Infragistics.Win.Misc.UltraPanel ultraPanelActionBar;
        private Infragistics.Win.Misc.UltraButton btnViewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewReport;
        private Infragistics.Win.Misc.UltraButton btnExportExcel;
        private Infragistics.Win.Misc.UltraButton btnColumnChooser;
        private Infragistics.Win.Misc.UltraButton btnHideSelection;

        private Infragistics.Win.Misc.UltraPanel ultraPanelSummaryCards;
        private Infragistics.Win.Misc.UltraPanel pnlCardBills;
        private Infragistics.Win.Misc.UltraLabel lblCardBillsTitle;
        private Infragistics.Win.Misc.UltraLabel lblCardBillsValue;
        private Infragistics.Win.Misc.UltraPanel pnlCardQty;
        private Infragistics.Win.Misc.UltraLabel lblCardQtyTitle;
        private Infragistics.Win.Misc.UltraLabel lblCardQtyValue;
        private Infragistics.Win.Misc.UltraPanel pnlCardSubTotal;
        private Infragistics.Win.Misc.UltraLabel lblCardSubTotalTitle;
        private Infragistics.Win.Misc.UltraLabel lblCardSubTotalValue;
        private Infragistics.Win.Misc.UltraPanel pnlCardTax;
        private Infragistics.Win.Misc.UltraLabel lblCardTaxTitle;
        private Infragistics.Win.Misc.UltraLabel lblCardTaxValue;
        private Infragistics.Win.Misc.UltraPanel pnlCardNet;
        private Infragistics.Win.Misc.UltraLabel lblCardNetTitle;
        private Infragistics.Win.Misc.UltraLabel lblCardNetValue;
        private Infragistics.Win.Misc.UltraPanel pnlCardProfit;
        private Infragistics.Win.Misc.UltraLabel lblCardProfitTitle;
        private Infragistics.Win.Misc.UltraLabel lblCardProfitValue;

        private Infragistics.Win.Misc.UltraPanel ultraPanelGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridMaster;
        private Infragistics.Win.Misc.UltraPanel gridFooterPanel;
        private Infragistics.Win.Misc.UltraLabel lblCount;
    }
}