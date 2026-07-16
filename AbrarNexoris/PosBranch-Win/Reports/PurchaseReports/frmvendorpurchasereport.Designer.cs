namespace PosBranch_Win.Reports.PurchaseReports
{
    partial class frmvendorpurchasereport
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
            this.panelPage = new System.Windows.Forms.Panel();
            this.tableContent = new System.Windows.Forms.TableLayoutPanel();
            this.panelGrid = new System.Windows.Forms.Panel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.panelGridFooter = new System.Windows.Forms.Panel();
            this.lblShowing = new System.Windows.Forms.Label();
            this.btnPreviewReport = new Infragistics.Win.Misc.UltraButton();
            this.footerButtonSpacer1 = new System.Windows.Forms.Panel();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.footerButtonSpacer2 = new System.Windows.Forms.Panel();
            this.btnExportGrid = new Infragistics.Win.Misc.UltraButton();
            this.tableSummary = new System.Windows.Forms.TableLayoutPanel();
            this.panelRowsCard = new System.Windows.Forms.Panel();
            this.ultraLabel1 = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalRows = new System.Windows.Forms.Label();
            this.panelBillsCard = new System.Windows.Forms.Panel();
            this.ultraLabel4 = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalPurchases = new System.Windows.Forms.Label();
            this.panelQtyCard = new System.Windows.Forms.Panel();
            this.ultraLabel3 = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalQty = new System.Windows.Forms.Label();
            this.panelAmountCard = new System.Windows.Forms.Panel();
            this.Amount = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalAmount = new System.Windows.Forms.Label();
            this.panelFilters = new System.Windows.Forms.Panel();
            this.tableFilters = new System.Windows.Forms.TableLayoutPanel();
            this.panelQuick = new System.Windows.Forms.Panel();
            this.cmbQuickDate = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblQuick = new System.Windows.Forms.Label();
            this.panelFrom = new System.Windows.Forms.Panel();
            this.dtpFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblFrom = new System.Windows.Forms.Label();
            this.panelTo = new System.Windows.Forms.Panel();
            this.dtpTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblTo = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.panelVendor = new System.Windows.Forms.Panel();
            this.txtVendor = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblVendor = new System.Windows.Forms.Label();
            this.panelItem = new System.Windows.Forms.Panel();
            this.txtItem = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblItem = new System.Windows.Forms.Label();
            this.btnVendor = new System.Windows.Forms.Button();
            this.btnItem = new System.Windows.Forms.Button();
            this.btnBoth = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelPage.SuspendLayout();
            this.tableContent.SuspendLayout();
            this.panelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.ultraPanelGridFooter.SuspendLayout();
            this.panelGridFooter.SuspendLayout();
            this.tableSummary.SuspendLayout();
            this.panelRowsCard.SuspendLayout();
            this.panelBillsCard.SuspendLayout();
            this.panelQtyCard.SuspendLayout();
            this.panelAmountCard.SuspendLayout();
            this.panelFilters.SuspendLayout();
            this.tableFilters.SuspendLayout();
            this.panelQuick.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbQuickDate)).BeginInit();
            this.panelFrom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtpFrom)).BeginInit();
            this.panelTo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtpTo)).BeginInit();
            this.panelVendor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtVendor)).BeginInit();
            this.panelItem.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtItem)).BeginInit();
            this.SuspendLayout();
            // 
            // panelPage
            // 
            this.panelPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.panelPage.Controls.Add(this.tableContent);
            this.panelPage.Controls.Add(this.tableSummary);
            this.panelPage.Controls.Add(this.panelFilters);
            this.panelPage.Controls.Add(this.lblTitle);
            this.panelPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPage.Location = new System.Drawing.Point(0, 0);
            this.panelPage.Name = "panelPage";
            this.panelPage.Padding = new System.Windows.Forms.Padding(18, 14, 18, 8);
            this.panelPage.Size = new System.Drawing.Size(1036, 520);
            this.panelPage.TabIndex = 0;
            // 
            // tableContent
            // 
            this.tableContent.BackColor = System.Drawing.Color.Transparent;
            this.tableContent.ColumnCount = 1;
            this.tableContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableContent.Controls.Add(this.panelGrid, 0, 0);
            this.tableContent.Controls.Add(this.panelGridFooter, 0, 1);
            this.tableContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableContent.Location = new System.Drawing.Point(18, 238);
            this.tableContent.Name = "tableContent";
            this.tableContent.RowCount = 2;
            this.tableContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableContent.Size = new System.Drawing.Size(1000, 274);
            this.tableContent.TabIndex = 3;
            // 
            // panelGrid
            // 
            this.panelGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.panelGrid.Controls.Add(this.gridReport);
            this.panelGrid.Controls.Add(this.ultraPanelGridFooter);
            this.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGrid.Location = new System.Drawing.Point(0, 0);
            this.panelGrid.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.panelGrid.Name = "panelGrid";
            this.panelGrid.Padding = new System.Windows.Forms.Padding(2);
            this.panelGrid.Size = new System.Drawing.Size(1000, 232);
            this.panelGrid.TabIndex = 0;
            // 
            // gridReport
            // 
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Location = new System.Drawing.Point(2, 2);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(996, 202);
            this.gridReport.TabIndex = 1;
            this.gridReport.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraPanelGridFooter
            // 
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(2, 204);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(996, 26);
            this.ultraPanelGridFooter.TabIndex = 2;
            // 
            // panelGridFooter
            // 
            this.panelGridFooter.BackColor = System.Drawing.Color.Transparent;
            this.panelGridFooter.Controls.Add(this.lblShowing);
            this.panelGridFooter.Controls.Add(this.btnPreviewReport);
            this.panelGridFooter.Controls.Add(this.footerButtonSpacer1);
            this.panelGridFooter.Controls.Add(this.btnPreviewGrid);
            this.panelGridFooter.Controls.Add(this.footerButtonSpacer2);
            this.panelGridFooter.Controls.Add(this.btnExportGrid);
            this.panelGridFooter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGridFooter.Location = new System.Drawing.Point(0, 238);
            this.panelGridFooter.Margin = new System.Windows.Forms.Padding(0);
            this.panelGridFooter.Name = "panelGridFooter";
            this.panelGridFooter.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.panelGridFooter.Size = new System.Drawing.Size(1000, 36);
            this.panelGridFooter.TabIndex = 1;
            // 
            // lblShowing
            // 
            this.lblShowing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblShowing.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblShowing.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(98)))), ((int)(((byte)(138)))));
            this.lblShowing.Location = new System.Drawing.Point(0, 4);
            this.lblShowing.Name = "lblShowing";
            this.lblShowing.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.lblShowing.Size = new System.Drawing.Size(644, 32);
            this.lblShowing.TabIndex = 0;
            this.lblShowing.Text = "Showing 0 record(s)";
            this.lblShowing.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnPreviewReport
            // 
            this.btnPreviewReport.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnPreviewReport.Location = new System.Drawing.Point(644, 4);
            this.btnPreviewReport.Margin = new System.Windows.Forms.Padding(4, 4, 4, 3);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(112, 32);
            this.btnPreviewReport.TabIndex = 8;
            this.btnPreviewReport.Text = "Preview Report";
            this.btnPreviewReport.Click += new System.EventHandler(this.btnPreviewReport_Click);
            // 
            // footerButtonSpacer1
            // 
            this.footerButtonSpacer1.Dock = System.Windows.Forms.DockStyle.Right;
            this.footerButtonSpacer1.Location = new System.Drawing.Point(756, 4);
            this.footerButtonSpacer1.Name = "footerButtonSpacer1";
            this.footerButtonSpacer1.Size = new System.Drawing.Size(10, 32);
            this.footerButtonSpacer1.TabIndex = 9;
            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnPreviewGrid.Location = new System.Drawing.Point(766, 4);
            this.btnPreviewGrid.Margin = new System.Windows.Forms.Padding(4, 4, 4, 3);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(112, 32);
            this.btnPreviewGrid.TabIndex = 7;
            this.btnPreviewGrid.Text = "Preview Grid";
            this.btnPreviewGrid.Click += new System.EventHandler(this.btnPreviewGrid_Click);
            // 
            // footerButtonSpacer2
            // 
            this.footerButtonSpacer2.Dock = System.Windows.Forms.DockStyle.Right;
            this.footerButtonSpacer2.Location = new System.Drawing.Point(878, 4);
            this.footerButtonSpacer2.Name = "footerButtonSpacer2";
            this.footerButtonSpacer2.Size = new System.Drawing.Size(10, 32);
            this.footerButtonSpacer2.TabIndex = 10;
            // 
            // btnExportGrid
            // 
            this.btnExportGrid.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnExportGrid.Location = new System.Drawing.Point(888, 4);
            this.btnExportGrid.Margin = new System.Windows.Forms.Padding(4, 4, 4, 3);
            this.btnExportGrid.Name = "btnExportGrid";
            this.btnExportGrid.Size = new System.Drawing.Size(112, 32);
            this.btnExportGrid.TabIndex = 6;
            this.btnExportGrid.Text = "Export Grid";
            this.btnExportGrid.Click += new System.EventHandler(this.btnExportGrid_Click);
            // 
            // tableSummary
            // 
            this.tableSummary.ColumnCount = 4;
            this.tableSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableSummary.Controls.Add(this.panelRowsCard, 0, 0);
            this.tableSummary.Controls.Add(this.panelBillsCard, 1, 0);
            this.tableSummary.Controls.Add(this.panelQtyCard, 2, 0);
            this.tableSummary.Controls.Add(this.panelAmountCard, 3, 0);
            this.tableSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableSummary.Location = new System.Drawing.Point(18, 160);
            this.tableSummary.Name = "tableSummary";
            this.tableSummary.Padding = new System.Windows.Forms.Padding(0, 10, 0, 8);
            this.tableSummary.RowCount = 1;
            this.tableSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableSummary.Size = new System.Drawing.Size(1000, 78);
            this.tableSummary.TabIndex = 2;
            // 
            // panelRowsCard
            // 
            this.panelRowsCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.panelRowsCard.Controls.Add(this.ultraLabel1);
            this.panelRowsCard.Controls.Add(this.lblTotalRows);
            this.panelRowsCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRowsCard.Location = new System.Drawing.Point(0, 10);
            this.panelRowsCard.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.panelRowsCard.Name = "panelRowsCard";
            this.panelRowsCard.Padding = new System.Windows.Forms.Padding(14, 8, 12, 8);
            this.panelRowsCard.Size = new System.Drawing.Size(244, 60);
            this.panelRowsCard.TabIndex = 0;
            // 
            // ultraLabel1
            // 
            appearance1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.ultraLabel1.Appearance = appearance1;
            this.ultraLabel1.Location = new System.Drawing.Point(10, 2);
            this.ultraLabel1.Name = "ultraLabel1";
            this.ultraLabel1.Size = new System.Drawing.Size(34, 15);
            this.ultraLabel1.TabIndex = 2;
            this.ultraLabel1.Text = "Rows";
            // 
            // lblTotalRows
            // 
            this.lblTotalRows.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalRows.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lblTotalRows.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblTotalRows.Location = new System.Drawing.Point(14, 8);
            this.lblTotalRows.Name = "lblTotalRows";
            this.lblTotalRows.Size = new System.Drawing.Size(218, 44);
            this.lblTotalRows.TabIndex = 1;
            this.lblTotalRows.Text = "0";
            this.lblTotalRows.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelBillsCard
            // 
            this.panelBillsCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.panelBillsCard.Controls.Add(this.ultraLabel4);
            this.panelBillsCard.Controls.Add(this.lblTotalPurchases);
            this.panelBillsCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBillsCard.Location = new System.Drawing.Point(256, 10);
            this.panelBillsCard.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.panelBillsCard.Name = "panelBillsCard";
            this.panelBillsCard.Padding = new System.Windows.Forms.Padding(14, 8, 12, 8);
            this.panelBillsCard.Size = new System.Drawing.Size(238, 60);
            this.panelBillsCard.TabIndex = 1;
            // 
            // ultraLabel4
            // 
            appearance2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.ultraLabel4.Appearance = appearance2;
            this.ultraLabel4.Location = new System.Drawing.Point(9, 3);
            this.ultraLabel4.Name = "ultraLabel4";
            this.ultraLabel4.Size = new System.Drawing.Size(82, 15);
            this.ultraLabel4.TabIndex = 5;
            this.ultraLabel4.Text = "Purchase bills";
            // 
            // lblTotalPurchases
            // 
            this.lblTotalPurchases.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalPurchases.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lblTotalPurchases.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblTotalPurchases.Location = new System.Drawing.Point(14, 8);
            this.lblTotalPurchases.Name = "lblTotalPurchases";
            this.lblTotalPurchases.Size = new System.Drawing.Size(212, 44);
            this.lblTotalPurchases.TabIndex = 1;
            this.lblTotalPurchases.Text = "0";
            this.lblTotalPurchases.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelQtyCard
            // 
            this.panelQtyCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.panelQtyCard.Controls.Add(this.ultraLabel3);
            this.panelQtyCard.Controls.Add(this.lblTotalQty);
            this.panelQtyCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelQtyCard.Location = new System.Drawing.Point(506, 10);
            this.panelQtyCard.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.panelQtyCard.Name = "panelQtyCard";
            this.panelQtyCard.Padding = new System.Windows.Forms.Padding(14, 8, 12, 8);
            this.panelQtyCard.Size = new System.Drawing.Size(238, 60);
            this.panelQtyCard.TabIndex = 2;
            // 
            // ultraLabel3
            // 
            appearance3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.ultraLabel3.Appearance = appearance3;
            this.ultraLabel3.Location = new System.Drawing.Point(11, 3);
            this.ultraLabel3.Name = "ultraLabel3";
            this.ultraLabel3.Size = new System.Drawing.Size(53, 15);
            this.ultraLabel3.TabIndex = 4;
            this.ultraLabel3.Text = "Quantity";
            // 
            // lblTotalQty
            // 
            this.lblTotalQty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalQty.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lblTotalQty.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblTotalQty.Location = new System.Drawing.Point(14, 8);
            this.lblTotalQty.Name = "lblTotalQty";
            this.lblTotalQty.Size = new System.Drawing.Size(212, 44);
            this.lblTotalQty.TabIndex = 1;
            this.lblTotalQty.Text = "0.00";
            this.lblTotalQty.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelAmountCard
            // 
            this.panelAmountCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.panelAmountCard.Controls.Add(this.Amount);
            this.panelAmountCard.Controls.Add(this.lblTotalAmount);
            this.panelAmountCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAmountCard.Location = new System.Drawing.Point(756, 10);
            this.panelAmountCard.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.panelAmountCard.Name = "panelAmountCard";
            this.panelAmountCard.Padding = new System.Windows.Forms.Padding(14, 8, 12, 8);
            this.panelAmountCard.Size = new System.Drawing.Size(244, 60);
            this.panelAmountCard.TabIndex = 3;
            // 
            // Amount
            // 
            appearance4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.Amount.Appearance = appearance4;
            this.Amount.Location = new System.Drawing.Point(13, 3);
            this.Amount.Name = "Amount";
            this.Amount.Size = new System.Drawing.Size(56, 15);
            this.Amount.TabIndex = 3;
            this.Amount.Text = "Amount";
            // 
            // lblTotalAmount
            // 
            this.lblTotalAmount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalAmount.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lblTotalAmount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblTotalAmount.Location = new System.Drawing.Point(14, 8);
            this.lblTotalAmount.Name = "lblTotalAmount";
            this.lblTotalAmount.Size = new System.Drawing.Size(218, 44);
            this.lblTotalAmount.TabIndex = 1;
            this.lblTotalAmount.Text = "Rs 0.00";
            this.lblTotalAmount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelFilters
            // 
            this.panelFilters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.panelFilters.Controls.Add(this.tableFilters);
            this.panelFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilters.Location = new System.Drawing.Point(18, 48);
            this.panelFilters.Name = "panelFilters";
            this.panelFilters.Size = new System.Drawing.Size(1000, 112);
            this.panelFilters.TabIndex = 1;
            // 
            // tableFilters
            // 
            this.tableFilters.ColumnCount = 12;
            this.tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.581967F));
            this.tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.016394F));
            this.tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableFilters.Controls.Add(this.panelQuick, 0, 0);
            this.tableFilters.Controls.Add(this.panelFrom, 2, 0);
            this.tableFilters.Controls.Add(this.panelTo, 4, 0);
            this.tableFilters.Controls.Add(this.btnApply, 6, 0);
            this.tableFilters.Controls.Add(this.btnClear, 7, 0);
            this.tableFilters.Controls.Add(this.panelVendor, 0, 1);
            this.tableFilters.Controls.Add(this.panelItem, 4, 1);
            this.tableFilters.Controls.Add(this.btnVendor, 7, 1);
            this.tableFilters.Controls.Add(this.btnItem, 8, 1);
            this.tableFilters.Controls.Add(this.btnBoth, 9, 1);
            this.tableFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableFilters.Location = new System.Drawing.Point(0, 0);
            this.tableFilters.Name = "tableFilters";
            this.tableFilters.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.tableFilters.RowCount = 2;
            this.tableFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableFilters.Size = new System.Drawing.Size(1000, 112);
            this.tableFilters.TabIndex = 0;
            // 
            // panelQuick
            // 
            this.tableFilters.SetColumnSpan(this.panelQuick, 2);
            this.panelQuick.Controls.Add(this.cmbQuickDate);
            this.panelQuick.Controls.Add(this.lblQuick);
            this.panelQuick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelQuick.Location = new System.Drawing.Point(12, 10);
            this.panelQuick.Margin = new System.Windows.Forms.Padding(0, 0, 8, 2);
            this.panelQuick.Name = "panelQuick";
            this.panelQuick.Size = new System.Drawing.Size(154, 44);
            this.panelQuick.TabIndex = 0;
            // 
            // cmbQuickDate
            // 
            this.cmbQuickDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbQuickDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbQuickDate.Location = new System.Drawing.Point(0, 18);
            this.cmbQuickDate.Name = "cmbQuickDate";
            this.cmbQuickDate.Size = new System.Drawing.Size(154, 25);
            this.cmbQuickDate.TabIndex = 1;
            this.cmbQuickDate.ValueChanged += new System.EventHandler(this.cmbQuickDate_ValueChanged);
            // 
            // lblQuick
            // 
            this.lblQuick.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblQuick.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            this.lblQuick.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(98)))), ((int)(((byte)(138)))));
            this.lblQuick.Location = new System.Drawing.Point(0, 0);
            this.lblQuick.Name = "lblQuick";
            this.lblQuick.Size = new System.Drawing.Size(154, 18);
            this.lblQuick.TabIndex = 0;
            this.lblQuick.Text = "Quick";
            // 
            // panelFrom
            // 
            this.tableFilters.SetColumnSpan(this.panelFrom, 2);
            this.panelFrom.Controls.Add(this.dtpFrom);
            this.panelFrom.Controls.Add(this.lblFrom);
            this.panelFrom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelFrom.Location = new System.Drawing.Point(174, 10);
            this.panelFrom.Margin = new System.Windows.Forms.Padding(0, 0, 8, 2);
            this.panelFrom.Name = "panelFrom";
            this.panelFrom.Size = new System.Drawing.Size(154, 44);
            this.panelFrom.TabIndex = 1;
            // 
            // dtpFrom
            // 
            this.dtpFrom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpFrom.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpFrom.Location = new System.Drawing.Point(0, 18);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(154, 25);
            this.dtpFrom.TabIndex = 1;
            this.dtpFrom.ValueChanged += new System.EventHandler(this.DatePicker_ValueChanged);
            // 
            // lblFrom
            // 
            this.lblFrom.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblFrom.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            this.lblFrom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(98)))), ((int)(((byte)(138)))));
            this.lblFrom.Location = new System.Drawing.Point(0, 0);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(154, 18);
            this.lblFrom.TabIndex = 0;
            this.lblFrom.Text = "From";
            // 
            // panelTo
            // 
            this.tableFilters.SetColumnSpan(this.panelTo, 2);
            this.panelTo.Controls.Add(this.dtpTo);
            this.panelTo.Controls.Add(this.lblTo);
            this.panelTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTo.Location = new System.Drawing.Point(336, 10);
            this.panelTo.Margin = new System.Windows.Forms.Padding(0, 0, 8, 2);
            this.panelTo.Name = "panelTo";
            this.panelTo.Size = new System.Drawing.Size(154, 44);
            this.panelTo.TabIndex = 2;
            // 
            // dtpTo
            // 
            this.dtpTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpTo.Location = new System.Drawing.Point(0, 18);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(154, 25);
            this.dtpTo.TabIndex = 1;
            this.dtpTo.ValueChanged += new System.EventHandler(this.DatePicker_ValueChanged);
            // 
            // lblTo
            // 
            this.lblTo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTo.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            this.lblTo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(98)))), ((int)(((byte)(138)))));
            this.lblTo.Location = new System.Drawing.Point(0, 0);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(154, 18);
            this.lblTo.TabIndex = 0;
            this.lblTo.Text = "To";
            // 
            // btnApply
            // 
            this.btnApply.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnApply.Location = new System.Drawing.Point(502, 30);
            this.btnApply.Margin = new System.Windows.Forms.Padding(4, 20, 4, 3);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(73, 23);
            this.btnApply.TabIndex = 4;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = false;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnClear
            // 
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClear.Location = new System.Drawing.Point(583, 30);
            this.btnClear.Margin = new System.Windows.Forms.Padding(4, 20, 4, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(73, 23);
            this.btnClear.TabIndex = 5;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // panelVendor
            // 
            this.tableFilters.SetColumnSpan(this.panelVendor, 4);
            this.panelVendor.Controls.Add(this.txtVendor);
            this.panelVendor.Controls.Add(this.lblVendor);
            this.panelVendor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelVendor.Location = new System.Drawing.Point(12, 56);
            this.panelVendor.Margin = new System.Windows.Forms.Padding(0, 0, 8, 2);
            this.panelVendor.Name = "panelVendor";
            this.panelVendor.Size = new System.Drawing.Size(316, 44);
            this.panelVendor.TabIndex = 7;
            // 
            // txtVendor
            // 
            this.txtVendor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtVendor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtVendor.Location = new System.Drawing.Point(0, 18);
            this.txtVendor.Name = "txtVendor";
            this.txtVendor.ReadOnly = true;
            this.txtVendor.Size = new System.Drawing.Size(316, 25);
            this.txtVendor.TabIndex = 1;
            // 
            // lblVendor
            // 
            this.lblVendor.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblVendor.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            this.lblVendor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(98)))), ((int)(((byte)(138)))));
            this.lblVendor.Location = new System.Drawing.Point(0, 0);
            this.lblVendor.Name = "lblVendor";
            this.lblVendor.Size = new System.Drawing.Size(316, 18);
            this.lblVendor.TabIndex = 0;
            this.lblVendor.Text = "Vendor";
            // 
            // panelItem
            // 
            this.tableFilters.SetColumnSpan(this.panelItem, 3);
            this.panelItem.Controls.Add(this.txtItem);
            this.panelItem.Controls.Add(this.lblItem);
            this.panelItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelItem.Location = new System.Drawing.Point(336, 56);
            this.panelItem.Margin = new System.Windows.Forms.Padding(0, 0, 8, 2);
            this.panelItem.Name = "panelItem";
            this.panelItem.Size = new System.Drawing.Size(235, 44);
            this.panelItem.TabIndex = 8;
            // 
            // txtItem
            // 
            this.txtItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtItem.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtItem.Location = new System.Drawing.Point(0, 18);
            this.txtItem.Name = "txtItem";
            this.txtItem.ReadOnly = true;
            this.txtItem.Size = new System.Drawing.Size(235, 25);
            this.txtItem.TabIndex = 1;
            // 
            // lblItem
            // 
            this.lblItem.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblItem.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            this.lblItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(98)))), ((int)(((byte)(138)))));
            this.lblItem.Location = new System.Drawing.Point(0, 0);
            this.lblItem.Name = "lblItem";
            this.lblItem.Size = new System.Drawing.Size(235, 18);
            this.lblItem.TabIndex = 0;
            this.lblItem.Text = "Item";
            // 
            // btnVendor
            // 
            this.btnVendor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnVendor.Location = new System.Drawing.Point(583, 76);
            this.btnVendor.Margin = new System.Windows.Forms.Padding(4, 20, 4, 3);
            this.btnVendor.Name = "btnVendor";
            this.btnVendor.Size = new System.Drawing.Size(73, 23);
            this.btnVendor.TabIndex = 9;
            this.btnVendor.Text = "Vendor";
            this.btnVendor.UseVisualStyleBackColor = false;
            this.btnVendor.Click += new System.EventHandler(this.btnVendor_Click);
            // 
            // btnItem
            // 
            this.btnItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnItem.Location = new System.Drawing.Point(664, 76);
            this.btnItem.Margin = new System.Windows.Forms.Padding(4, 20, 4, 3);
            this.btnItem.Name = "btnItem";
            this.btnItem.Size = new System.Drawing.Size(66, 23);
            this.btnItem.TabIndex = 10;
            this.btnItem.Text = "Item";
            this.btnItem.UseVisualStyleBackColor = false;
            this.btnItem.Click += new System.EventHandler(this.btnItem_Click);
            // 
            // btnBoth
            // 
            this.btnBoth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBoth.Location = new System.Drawing.Point(738, 76);
            this.btnBoth.Margin = new System.Windows.Forms.Padding(4, 20, 4, 3);
            this.btnBoth.Name = "btnBoth";
            this.btnBoth.Size = new System.Drawing.Size(80, 23);
            this.btnBoth.TabIndex = 11;
            this.btnBoth.Text = "Both";
            this.btnBoth.UseVisualStyleBackColor = false;
            this.btnBoth.Click += new System.EventHandler(this.btnBoth_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblTitle.Location = new System.Drawing.Point(18, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1000, 34);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Vendor Purchase Report";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frmvendorpurchasereport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1036, 520);
            this.Controls.Add(this.panelPage);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "frmvendorpurchasereport";
            this.Text = "Vendor Purchase Report";
            this.Load += new System.EventHandler(this.frmvendorpurchasereport_Load);
            this.panelPage.ResumeLayout(false);
            this.tableContent.ResumeLayout(false);
            this.panelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.ultraPanelGridFooter.ResumeLayout(false);
            this.panelGridFooter.ResumeLayout(false);
            this.tableSummary.ResumeLayout(false);
            this.panelRowsCard.ResumeLayout(false);
            this.panelBillsCard.ResumeLayout(false);
            this.panelQtyCard.ResumeLayout(false);
            this.panelAmountCard.ResumeLayout(false);
            this.panelFilters.ResumeLayout(false);
            this.tableFilters.ResumeLayout(false);
            this.panelQuick.ResumeLayout(false);
            this.panelQuick.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbQuickDate)).EndInit();
            this.panelFrom.ResumeLayout(false);
            this.panelFrom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtpFrom)).EndInit();
            this.panelTo.ResumeLayout(false);
            this.panelTo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtpTo)).EndInit();
            this.panelVendor.ResumeLayout(false);
            this.panelVendor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtVendor)).EndInit();
            this.panelItem.ResumeLayout(false);
            this.panelItem.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtItem)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelPage;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TableLayoutPanel tableContent;
        private System.Windows.Forms.Panel panelGridFooter;
        private System.Windows.Forms.Panel panelFilters;
        private System.Windows.Forms.TableLayoutPanel tableFilters;
        private System.Windows.Forms.Panel panelQuick;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbQuickDate;
        private System.Windows.Forms.Label lblQuick;
        private System.Windows.Forms.Panel panelFrom;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpFrom;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.Panel panelTo;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpTo;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnClear;
        private Infragistics.Win.Misc.UltraButton btnExportGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewReport;
        private System.Windows.Forms.Panel panelVendor;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtVendor;
        private System.Windows.Forms.Label lblVendor;
        private System.Windows.Forms.Panel panelItem;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtItem;
        private System.Windows.Forms.Label lblItem;
        private System.Windows.Forms.Button btnVendor;
        private System.Windows.Forms.Button btnItem;
        private System.Windows.Forms.Button btnBoth;
        private System.Windows.Forms.TableLayoutPanel tableSummary;
        private System.Windows.Forms.Panel panelRowsCard;
        private System.Windows.Forms.Label lblTotalRows;
        private System.Windows.Forms.Panel panelBillsCard;
        private System.Windows.Forms.Label lblTotalPurchases;
        private System.Windows.Forms.Panel panelQtyCard;
        private System.Windows.Forms.Label lblTotalQty;
        private System.Windows.Forms.Panel panelAmountCard;
        private System.Windows.Forms.Label lblTotalAmount;
        private System.Windows.Forms.Panel panelGrid;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;
        private System.Windows.Forms.Label lblShowing;
        private System.Windows.Forms.Panel footerButtonSpacer1;
        private System.Windows.Forms.Panel footerButtonSpacer2;
        private Infragistics.Win.Misc.UltraLabel ultraLabel1;
        private Infragistics.Win.Misc.UltraLabel ultraLabel4;
        private Infragistics.Win.Misc.UltraLabel ultraLabel3;
        private Infragistics.Win.Misc.UltraLabel Amount;
    }
}
