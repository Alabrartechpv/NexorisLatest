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
    partial class SalesReturnReport
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

            Infragistics.Win.Appearance appCardReturns = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appCardQty = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appCardSubTotal = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appCardTax = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appCardGrandTotal = new Infragistics.Win.Appearance();

            this.ultraPanelSelection = new Infragistics.Win.Misc.UltraPanel();
            this.lblDate = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboDateMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFromDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtToDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblReturnNo = new Infragistics.Win.Misc.UltraLabel();
            this.txtReturnNo = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblPaymentMode = new Infragistics.Win.Misc.UltraLabel();
            this.cmbPaymentMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
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
            this.pnlCardReturns = new Infragistics.Win.Misc.UltraPanel();
            this.lblCardReturnsTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblCardReturnsValue = new Infragistics.Win.Misc.UltraLabel();

            this.pnlCardQty = new Infragistics.Win.Misc.UltraPanel();
            this.lblCardQtyTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblCardQtyValue = new Infragistics.Win.Misc.UltraLabel();

            this.pnlCardSubTotal = new Infragistics.Win.Misc.UltraPanel();
            this.lblCardSubTotalTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblCardSubTotalValue = new Infragistics.Win.Misc.UltraLabel();

            this.pnlCardTax = new Infragistics.Win.Misc.UltraPanel();
            this.lblCardTaxTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblCardTaxValue = new Infragistics.Win.Misc.UltraLabel();

            this.pnlCardGrandTotal = new Infragistics.Win.Misc.UltraPanel();
            this.lblCardGrandTotalTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblCardGrandTotalValue = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridMaster = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.gridFooterPanel = new Infragistics.Win.Misc.UltraPanel();
            this.lblCount = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanelSelection.ClientArea.SuspendLayout();
            this.ultraPanelSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtReturnNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymentMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCustomer)).BeginInit();

            this.ultraPanelActionBar.ClientArea.SuspendLayout();
            this.ultraPanelActionBar.SuspendLayout();

            this.ultraPanelSummaryCards.ClientArea.SuspendLayout();
            this.ultraPanelSummaryCards.SuspendLayout();
            this.pnlCardReturns.ClientArea.SuspendLayout();
            this.pnlCardReturns.SuspendLayout();
            this.pnlCardQty.ClientArea.SuspendLayout();
            this.pnlCardQty.SuspendLayout();
            this.pnlCardSubTotal.ClientArea.SuspendLayout();
            this.pnlCardSubTotal.SuspendLayout();
            this.pnlCardTax.ClientArea.SuspendLayout();
            this.pnlCardTax.SuspendLayout();
            this.pnlCardGrandTotal.ClientArea.SuspendLayout();
            this.pnlCardGrandTotal.SuspendLayout();

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
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblReturnNo);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.txtReturnNo);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblPaymentMode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbPaymentMode);
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
            // lblReturnNo
            // 
            this.lblReturnNo.Location = new System.Drawing.Point(620, 14);
            this.lblReturnNo.Name = "lblReturnNo";
            this.lblReturnNo.Size = new System.Drawing.Size(70, 20);
            this.lblReturnNo.TabIndex = 6;
            this.lblReturnNo.Text = "Return / Inv";

            // 
            // txtReturnNo
            // 
            this.txtReturnNo.Location = new System.Drawing.Point(695, 11);
            this.txtReturnNo.Name = "txtReturnNo";
            this.txtReturnNo.NullText = "Search Return/Inv...";
            this.txtReturnNo.Size = new System.Drawing.Size(145, 22);
            this.txtReturnNo.TabIndex = 7;

            // 
            // lblPaymentMode
            // 
            this.lblPaymentMode.Location = new System.Drawing.Point(15, 44);
            this.lblPaymentMode.Name = "lblPaymentMode";
            this.lblPaymentMode.Size = new System.Drawing.Size(90, 20);
            this.lblPaymentMode.TabIndex = 8;
            this.lblPaymentMode.Text = "Payment Mode";

            // 
            // cmbPaymentMode
            // 
            this.cmbPaymentMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbPaymentMode.Location = new System.Drawing.Point(110, 41);
            this.cmbPaymentMode.Name = "cmbPaymentMode";
            this.cmbPaymentMode.Size = new System.Drawing.Size(160, 22);
            this.cmbPaymentMode.TabIndex = 9;

            // 
            // lblCustomer
            // 
            this.lblCustomer.Location = new System.Drawing.Point(290, 44);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Size = new System.Drawing.Size(65, 20);
            this.lblCustomer.TabIndex = 10;
            this.lblCustomer.Text = "Customer";

            // 
            // cmbCustomer
            // 
            this.cmbCustomer.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbCustomer.Location = new System.Drawing.Point(360, 41);
            this.cmbCustomer.Name = "cmbCustomer";
            this.cmbCustomer.Size = new System.Drawing.Size(245, 22);
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
            this.ultraPanelSummaryCards.ClientArea.Controls.Add(this.pnlCardReturns);
            this.ultraPanelSummaryCards.ClientArea.Controls.Add(this.pnlCardQty);
            this.ultraPanelSummaryCards.ClientArea.Controls.Add(this.pnlCardSubTotal);
            this.ultraPanelSummaryCards.ClientArea.Controls.Add(this.pnlCardTax);
            this.ultraPanelSummaryCards.ClientArea.Controls.Add(this.pnlCardGrandTotal);
            this.ultraPanelSummaryCards.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelSummaryCards.Location = new System.Drawing.Point(0, 642);
            this.ultraPanelSummaryCards.Name = "ultraPanelSummaryCards";
            this.ultraPanelSummaryCards.Size = new System.Drawing.Size(1280, 78);
            this.ultraPanelSummaryCards.TabIndex = 3;
            this.ultraPanelSummaryCards.ClientArea.Resize += new System.EventHandler(this.SummaryCards_Resize);

            // 
            // pnlCardReturns
            // 
            appCardReturns.BackColor = System.Drawing.Color.White;
            appCardReturns.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(197)))), ((int)(((byte)(253)))));
            this.pnlCardReturns.Appearance = appCardReturns;
            this.pnlCardReturns.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.pnlCardReturns.ClientArea.Controls.Add(this.lblCardReturnsTitle);
            this.pnlCardReturns.ClientArea.Controls.Add(this.lblCardReturnsValue);
            this.pnlCardReturns.Location = new System.Drawing.Point(12, 7);
            this.pnlCardReturns.Name = "pnlCardReturns";
            this.pnlCardReturns.Size = new System.Drawing.Size(220, 58);
            this.pnlCardReturns.TabIndex = 0;

            // 
            // lblCardReturnsTitle
            // 
            this.lblCardReturnsTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardReturnsTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblCardReturnsTitle.Location = new System.Drawing.Point(8, 5);
            this.lblCardReturnsTitle.Name = "lblCardReturnsTitle";
            this.lblCardReturnsTitle.Size = new System.Drawing.Size(200, 16);
            this.lblCardReturnsTitle.TabIndex = 0;
            this.lblCardReturnsTitle.Text = "📋 TOTAL RETURNS";

            // 
            // lblCardReturnsValue
            // 
            this.lblCardReturnsValue.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCardReturnsValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(138)))));
            this.lblCardReturnsValue.Location = new System.Drawing.Point(8, 24);
            this.lblCardReturnsValue.Name = "lblCardReturnsValue";
            this.lblCardReturnsValue.Size = new System.Drawing.Size(200, 26);
            this.lblCardReturnsValue.TabIndex = 1;
            this.lblCardReturnsValue.Text = "0";

            // 
            // pnlCardQty
            // 
            appCardQty.BackColor = System.Drawing.Color.White;
            appCardQty.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.pnlCardQty.Appearance = appCardQty;
            this.pnlCardQty.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.pnlCardQty.ClientArea.Controls.Add(this.lblCardQtyTitle);
            this.pnlCardQty.ClientArea.Controls.Add(this.lblCardQtyValue);
            this.pnlCardQty.Location = new System.Drawing.Point(240, 7);
            this.pnlCardQty.Name = "pnlCardQty";
            this.pnlCardQty.Size = new System.Drawing.Size(220, 58);
            this.pnlCardQty.TabIndex = 1;

            // 
            // lblCardQtyTitle
            // 
            this.lblCardQtyTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardQtyTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblCardQtyTitle.Location = new System.Drawing.Point(8, 5);
            this.lblCardQtyTitle.Name = "lblCardQtyTitle";
            this.lblCardQtyTitle.Size = new System.Drawing.Size(200, 16);
            this.lblCardQtyTitle.TabIndex = 0;
            this.lblCardQtyTitle.Text = "📦 TOTAL QTY";

            // 
            // lblCardQtyValue
            // 
            this.lblCardQtyValue.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCardQtyValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblCardQtyValue.Location = new System.Drawing.Point(8, 24);
            this.lblCardQtyValue.Name = "lblCardQtyValue";
            this.lblCardQtyValue.Size = new System.Drawing.Size(200, 26);
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
            this.pnlCardSubTotal.Location = new System.Drawing.Point(468, 7);
            this.pnlCardSubTotal.Name = "pnlCardSubTotal";
            this.pnlCardSubTotal.Size = new System.Drawing.Size(220, 58);
            this.pnlCardSubTotal.TabIndex = 2;

            // 
            // lblCardSubTotalTitle
            // 
            this.lblCardSubTotalTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardSubTotalTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblCardSubTotalTitle.Location = new System.Drawing.Point(8, 5);
            this.lblCardSubTotalTitle.Name = "lblCardSubTotalTitle";
            this.lblCardSubTotalTitle.Size = new System.Drawing.Size(200, 16);
            this.lblCardSubTotalTitle.TabIndex = 0;
            this.lblCardSubTotalTitle.Text = "💰 SUB TOTAL";

            // 
            // lblCardSubTotalValue
            // 
            this.lblCardSubTotalValue.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCardSubTotalValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(148)))), ((int)(((byte)(136)))));
            this.lblCardSubTotalValue.Location = new System.Drawing.Point(8, 24);
            this.lblCardSubTotalValue.Name = "lblCardSubTotalValue";
            this.lblCardSubTotalValue.Size = new System.Drawing.Size(200, 26);
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
            this.pnlCardTax.Location = new System.Drawing.Point(696, 7);
            this.pnlCardTax.Name = "pnlCardTax";
            this.pnlCardTax.Size = new System.Drawing.Size(220, 58);
            this.pnlCardTax.TabIndex = 3;

            // 
            // lblCardTaxTitle
            // 
            this.lblCardTaxTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardTaxTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblCardTaxTitle.Location = new System.Drawing.Point(8, 5);
            this.lblCardTaxTitle.Name = "lblCardTaxTitle";
            this.lblCardTaxTitle.Size = new System.Drawing.Size(200, 16);
            this.lblCardTaxTitle.TabIndex = 0;
            this.lblCardTaxTitle.Text = "🧾 TAX TOTAL";

            // 
            // lblCardTaxValue
            // 
            this.lblCardTaxValue.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCardTaxValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(88)))), ((int)(((byte)(12)))));
            this.lblCardTaxValue.Location = new System.Drawing.Point(8, 24);
            this.lblCardTaxValue.Name = "lblCardTaxValue";
            this.lblCardTaxValue.Size = new System.Drawing.Size(200, 26);
            this.lblCardTaxValue.TabIndex = 1;
            this.lblCardTaxValue.Text = "₹ 0.00";

            // 
            // pnlCardGrandTotal
            // 
            appCardGrandTotal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(253)))), ((int)(((byte)(244)))));
            appCardGrandTotal.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(197)))), ((int)(((byte)(94)))));
            this.pnlCardGrandTotal.Appearance = appCardGrandTotal;
            this.pnlCardGrandTotal.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.pnlCardGrandTotal.ClientArea.Controls.Add(this.lblCardGrandTotalTitle);
            this.pnlCardGrandTotal.ClientArea.Controls.Add(this.lblCardGrandTotalValue);
            this.pnlCardGrandTotal.Location = new System.Drawing.Point(924, 7);
            this.pnlCardGrandTotal.Name = "pnlCardGrandTotal";
            this.pnlCardGrandTotal.Size = new System.Drawing.Size(230, 58);
            this.pnlCardGrandTotal.TabIndex = 4;

            // 
            // lblCardGrandTotalTitle
            // 
            this.lblCardGrandTotalTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCardGrandTotalTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(128)))), ((int)(((byte)(61)))));
            this.lblCardGrandTotalTitle.Location = new System.Drawing.Point(8, 5);
            this.lblCardGrandTotalTitle.Name = "lblCardGrandTotalTitle";
            this.lblCardGrandTotalTitle.Size = new System.Drawing.Size(210, 16);
            this.lblCardGrandTotalTitle.TabIndex = 0;
            this.lblCardGrandTotalTitle.Text = "💵 GRAND TOTAL";

            // 
            // lblCardGrandTotalValue
            // 
            this.lblCardGrandTotalValue.Font = new System.Drawing.Font("Segoe UI", 12.5F, System.Drawing.FontStyle.Bold);
            this.lblCardGrandTotalValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(101)))), ((int)(((byte)(52)))));
            this.lblCardGrandTotalValue.Location = new System.Drawing.Point(8, 24);
            this.lblCardGrandTotalValue.Name = "lblCardGrandTotalValue";
            this.lblCardGrandTotalValue.Size = new System.Drawing.Size(210, 26);
            this.lblCardGrandTotalValue.TabIndex = 1;
            this.lblCardGrandTotalValue.Text = "₹ 0.00";

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
            this.ultraPanelGrid.Size = new System.Drawing.Size(1280, 520);
            this.ultraPanelGrid.TabIndex = 2;

            // 
            // ultraGridMaster
            // 
            this.ultraGridMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridMaster.Location = new System.Drawing.Point(0, 0);
            this.ultraGridMaster.Name = "ultraGridMaster";
            this.ultraGridMaster.Size = new System.Drawing.Size(1280, 488);
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
            this.gridFooterPanel.Location = new System.Drawing.Point(0, 488);
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
            this.lblCount.Text = "Total Returns: 0";

            // 
            // SalesReturnReport
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
            this.Name = "SalesReturnReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sales Return Report - Master Detail View";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.ultraPanelSelection.ClientArea.ResumeLayout(false);
            this.ultraPanelSelection.ClientArea.PerformLayout();
            this.ultraPanelSelection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtReturnNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymentMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCustomer)).EndInit();

            this.ultraPanelActionBar.ClientArea.ResumeLayout(false);
            this.ultraPanelActionBar.ResumeLayout(false);

            this.ultraPanelSummaryCards.ClientArea.ResumeLayout(false);
            this.ultraPanelSummaryCards.ResumeLayout(false);
            this.pnlCardReturns.ClientArea.ResumeLayout(false);
            this.pnlCardReturns.ResumeLayout(false);
            this.pnlCardQty.ClientArea.ResumeLayout(false);
            this.pnlCardQty.ResumeLayout(false);
            this.pnlCardSubTotal.ClientArea.ResumeLayout(false);
            this.pnlCardSubTotal.ResumeLayout(false);
            this.pnlCardTax.ClientArea.ResumeLayout(false);
            this.pnlCardTax.ResumeLayout(false);
            this.pnlCardGrandTotal.ClientArea.ResumeLayout(false);
            this.pnlCardGrandTotal.ResumeLayout(false);

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
        private Infragistics.Win.Misc.UltraLabel lblReturnNo;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtReturnNo;
        private Infragistics.Win.Misc.UltraLabel lblPaymentMode;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbPaymentMode;
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
        private Infragistics.Win.Misc.UltraPanel pnlCardReturns;
        private Infragistics.Win.Misc.UltraLabel lblCardReturnsTitle;
        private Infragistics.Win.Misc.UltraLabel lblCardReturnsValue;
        private Infragistics.Win.Misc.UltraPanel pnlCardQty;
        private Infragistics.Win.Misc.UltraLabel lblCardQtyTitle;
        private Infragistics.Win.Misc.UltraLabel lblCardQtyValue;
        private Infragistics.Win.Misc.UltraPanel pnlCardSubTotal;
        private Infragistics.Win.Misc.UltraLabel lblCardSubTotalTitle;
        private Infragistics.Win.Misc.UltraLabel lblCardSubTotalValue;
        private Infragistics.Win.Misc.UltraPanel pnlCardTax;
        private Infragistics.Win.Misc.UltraLabel lblCardTaxTitle;
        private Infragistics.Win.Misc.UltraLabel lblCardTaxValue;
        private Infragistics.Win.Misc.UltraPanel pnlCardGrandTotal;
        private Infragistics.Win.Misc.UltraLabel lblCardGrandTotalTitle;
        private Infragistics.Win.Misc.UltraLabel lblCardGrandTotalValue;

        private Infragistics.Win.Misc.UltraPanel ultraPanelGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridMaster;
        private Infragistics.Win.Misc.UltraPanel gridFooterPanel;
        private Infragistics.Win.Misc.UltraLabel lblCount;
    }
}
