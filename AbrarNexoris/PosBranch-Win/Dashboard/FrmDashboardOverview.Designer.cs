namespace PosBranch_Win.Dashboard
{
    partial class FrmDashboardOverview
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubTitle;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbQuickDate;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.TableLayoutPanel cardsLayout;
        private System.Windows.Forms.Panel cardSales;
        private System.Windows.Forms.Panel cardPurchase;
        private System.Windows.Forms.Panel cardSalesReturn;
        private System.Windows.Forms.Panel cardPurchaseReturn;
        private System.Windows.Forms.Panel cardReceipts;
        private System.Windows.Forms.Panel cardPayments;
        private System.Windows.Forms.Label lblSalesIcon;
        private System.Windows.Forms.Label lblSalesCardTitle;
        private System.Windows.Forms.Label lblSalesValue;
        private System.Windows.Forms.Label lblSalesFooter;
        private System.Windows.Forms.Label lblPurchaseIcon;
        private System.Windows.Forms.Label lblPurchaseCardTitle;
        private System.Windows.Forms.Label lblPurchaseValue;
        private System.Windows.Forms.Label lblPurchaseFooter;
        private System.Windows.Forms.Label lblSalesReturnIcon;
        private System.Windows.Forms.Label lblSalesReturnCardTitle;
        private System.Windows.Forms.Label lblSalesReturnValue;
        private System.Windows.Forms.Label lblSalesReturnFooter;
        private System.Windows.Forms.Label lblPurchaseReturnIcon;
        private System.Windows.Forms.Label lblPurchaseReturnCardTitle;
        private System.Windows.Forms.Label lblPurchaseReturnValue;
        private System.Windows.Forms.Label lblPurchaseReturnFooter;
        private System.Windows.Forms.Label lblReceiptsIcon;
        private System.Windows.Forms.Label lblReceiptsCardTitle;
        private System.Windows.Forms.Label lblReceiptsValue;
        private System.Windows.Forms.Label lblReceiptsFooter;
        private System.Windows.Forms.Label lblPaymentsIcon;
        private System.Windows.Forms.Label lblPaymentsCardTitle;
        private System.Windows.Forms.Label lblPaymentsValue;
        private System.Windows.Forms.Label lblPaymentsFooter;
        private System.Windows.Forms.TableLayoutPanel middleLayout;
        private System.Windows.Forms.Panel chartPanelWrapper;
        private System.Windows.Forms.Label lblChartTitle;
        private System.Windows.Forms.Panel pnlSalesTrend;
        private System.Windows.Forms.Panel topItemsPanel;
        private System.Windows.Forms.Label lblTopItemsTitle;
        private System.Windows.Forms.DataGridView dgvTopItems;
        private System.Windows.Forms.TableLayoutPanel bottomLayout;
        private System.Windows.Forms.Panel stockPanel;
        private System.Windows.Forms.Label lblStockIcon;
        private System.Windows.Forms.Label lblStockTitle;
        private System.Windows.Forms.Label lblStockSummary;
        private System.Windows.Forms.Label lblStockTotalCaption;
        private System.Windows.Forms.Label lblStockLowCaption;
        private System.Windows.Forms.Label lblStockOutCaption;
        private System.Windows.Forms.Label lblStockTotalIcon;
        private System.Windows.Forms.Label lblStockLowIcon;
        private System.Windows.Forms.Label lblStockOutIcon;
        private System.Windows.Forms.Label lblStockTotalValue;
        private System.Windows.Forms.Label lblStockLowValue;
        private System.Windows.Forms.Label lblStockOutValue;
        private System.Windows.Forms.Panel customerPanel;
        private System.Windows.Forms.Label lblCustomerIcon;
        private System.Windows.Forms.Label lblCustomerTitle;
        private System.Windows.Forms.Label lblCustomerValue;
        private System.Windows.Forms.Label lblCustomerCaption;
        private System.Windows.Forms.Panel vendorPanel;
        private System.Windows.Forms.Label lblVendorIcon;
        private System.Windows.Forms.Label lblVendorTitle;
        private System.Windows.Forms.Label lblVendorValue;
        private System.Windows.Forms.Label lblVendorCaption;
        private System.Windows.Forms.Panel duePanel;
        private System.Windows.Forms.Label lblDueIcon;
        private System.Windows.Forms.Label lblDueTitle;
        private System.Windows.Forms.Label lblDueSummary;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubTitle = new System.Windows.Forms.Label();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.cmbQuickDate = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.btnApply = new System.Windows.Forms.Button();
            this.cardsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.cardSales = new System.Windows.Forms.Panel();
            this.lblSalesCardTitle = new System.Windows.Forms.Label();
            this.lblSalesIcon = new System.Windows.Forms.Label();
            this.lblSalesFooter = new System.Windows.Forms.Label();
            this.lblSalesValue = new System.Windows.Forms.Label();
            this.cardPurchase = new System.Windows.Forms.Panel();
            this.lblPurchaseCardTitle = new System.Windows.Forms.Label();
            this.lblPurchaseIcon = new System.Windows.Forms.Label();
            this.lblPurchaseFooter = new System.Windows.Forms.Label();
            this.lblPurchaseValue = new System.Windows.Forms.Label();
            this.cardSalesReturn = new System.Windows.Forms.Panel();
            this.lblSalesReturnCardTitle = new System.Windows.Forms.Label();
            this.lblSalesReturnIcon = new System.Windows.Forms.Label();
            this.lblSalesReturnFooter = new System.Windows.Forms.Label();
            this.lblSalesReturnValue = new System.Windows.Forms.Label();
            this.cardPurchaseReturn = new System.Windows.Forms.Panel();
            this.lblPurchaseReturnCardTitle = new System.Windows.Forms.Label();
            this.lblPurchaseReturnIcon = new System.Windows.Forms.Label();
            this.lblPurchaseReturnFooter = new System.Windows.Forms.Label();
            this.lblPurchaseReturnValue = new System.Windows.Forms.Label();
            this.cardReceipts = new System.Windows.Forms.Panel();
            this.lblReceiptsCardTitle = new System.Windows.Forms.Label();
            this.lblReceiptsIcon = new System.Windows.Forms.Label();
            this.lblReceiptsFooter = new System.Windows.Forms.Label();
            this.lblReceiptsValue = new System.Windows.Forms.Label();
            this.cardPayments = new System.Windows.Forms.Panel();
            this.lblPaymentsCardTitle = new System.Windows.Forms.Label();
            this.lblPaymentsIcon = new System.Windows.Forms.Label();
            this.lblPaymentsFooter = new System.Windows.Forms.Label();
            this.lblPaymentsValue = new System.Windows.Forms.Label();
            this.middleLayout = new System.Windows.Forms.TableLayoutPanel();
            this.chartPanelWrapper = new System.Windows.Forms.Panel();
            this.pnlSalesTrend = new System.Windows.Forms.Panel();
            this.lblChartTitle = new System.Windows.Forms.Label();
            this.topItemsPanel = new System.Windows.Forms.Panel();
            this.dgvTopItems = new System.Windows.Forms.DataGridView();
            this.lblTopItemsTitle = new System.Windows.Forms.Label();
            this.bottomLayout = new System.Windows.Forms.TableLayoutPanel();
            this.stockPanel = new System.Windows.Forms.Panel();
            this.lblStockOutValue = new System.Windows.Forms.Label();
            this.lblStockLowValue = new System.Windows.Forms.Label();
            this.lblStockTotalValue = new System.Windows.Forms.Label();
            this.lblStockOutIcon = new System.Windows.Forms.Label();
            this.lblStockLowIcon = new System.Windows.Forms.Label();
            this.lblStockTotalIcon = new System.Windows.Forms.Label();
            this.lblStockOutCaption = new System.Windows.Forms.Label();
            this.lblStockLowCaption = new System.Windows.Forms.Label();
            this.lblStockTotalCaption = new System.Windows.Forms.Label();
            this.lblStockSummary = new System.Windows.Forms.Label();
            this.lblStockTitle = new System.Windows.Forms.Label();
            this.lblStockIcon = new System.Windows.Forms.Label();
            this.customerPanel = new System.Windows.Forms.Panel();
            this.lblCustomerCaption = new System.Windows.Forms.Label();
            this.lblCustomerValue = new System.Windows.Forms.Label();
            this.lblCustomerTitle = new System.Windows.Forms.Label();
            this.lblCustomerIcon = new System.Windows.Forms.Label();
            this.vendorPanel = new System.Windows.Forms.Panel();
            this.lblVendorCaption = new System.Windows.Forms.Label();
            this.lblVendorValue = new System.Windows.Forms.Label();
            this.lblVendorTitle = new System.Windows.Forms.Label();
            this.lblVendorIcon = new System.Windows.Forms.Label();
            this.duePanel = new System.Windows.Forms.Panel();
            this.lblDueSummary = new System.Windows.Forms.Label();
            this.lblDueTitle = new System.Windows.Forms.Label();
            this.lblDueIcon = new System.Windows.Forms.Label();
            this.mainLayout.SuspendLayout();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbQuickDate)).BeginInit();
            this.cardsLayout.SuspendLayout();
            this.cardSales.SuspendLayout();
            this.cardPurchase.SuspendLayout();
            this.cardSalesReturn.SuspendLayout();
            this.cardPurchaseReturn.SuspendLayout();
            this.cardReceipts.SuspendLayout();
            this.cardPayments.SuspendLayout();
            this.middleLayout.SuspendLayout();
            this.chartPanelWrapper.SuspendLayout();
            this.topItemsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTopItems)).BeginInit();
            this.bottomLayout.SuspendLayout();
            this.stockPanel.SuspendLayout();
            this.customerPanel.SuspendLayout();
            this.vendorPanel.SuspendLayout();
            this.duePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayout
            // 
            this.mainLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.mainLayout.ColumnCount = 1;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.Controls.Add(this.headerPanel, 0, 0);
            this.mainLayout.Controls.Add(this.cardsLayout, 0, 1);
            this.mainLayout.Controls.Add(this.middleLayout, 0, 2);
            this.mainLayout.Controls.Add(this.bottomLayout, 0, 3);
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.Padding = new System.Windows.Forms.Padding(18, 12, 18, 12);
            this.mainLayout.RowCount = 4;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 146F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.mainLayout.Size = new System.Drawing.Size(1180, 610);
            this.mainLayout.TabIndex = 0;
            // 
            // headerPanel
            // 
            this.headerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.headerPanel.Controls.Add(this.lblTitle);
            this.headerPanel.Controls.Add(this.lblSubTitle);
            this.headerPanel.Controls.Add(this.dtFrom);
            this.headerPanel.Controls.Add(this.dtTo);
            this.headerPanel.Controls.Add(this.cmbQuickDate);
            this.headerPanel.Controls.Add(this.btnApply);
            this.headerPanel.Location = new System.Drawing.Point(21, 15);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(1138, 52);
            this.headerPanel.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(55)))), ((int)(((byte)(98)))));
            this.lblTitle.Location = new System.Drawing.Point(2, 2);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(91, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Overview";
            // 
            // lblSubTitle
            // 
            this.lblSubTitle.AutoSize = true;
            this.lblSubTitle.Font = new System.Drawing.Font("Segoe UI", 8.75F);
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(96)))), ((int)(((byte)(132)))));
            this.lblSubTitle.Location = new System.Drawing.Point(4, 32);
            this.lblSubTitle.Name = "lblSubTitle";
            this.lblSubTitle.Size = new System.Drawing.Size(234, 15);
            this.lblSubTitle.TabIndex = 1;
            this.lblSubTitle.Text = "Here is what is happening in your business.";
            // 
            // dtFrom
            // 
            this.dtFrom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            appearance1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(144)))), ((int)(((byte)(181)))), ((int)(((byte)(223)))));
            appearance1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(73)))), ((int)(((byte)(126)))));
            appearance1.TextHAlignAsString = "Center";
            this.dtFrom.Appearance = appearance1;
            this.dtFrom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.dtFrom.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.dtFrom.DateTime = new System.DateTime(2026, 5, 25, 0, 0, 0, 0);
            this.dtFrom.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.dtFrom.Font = new System.Drawing.Font("Segoe UI", 8.75F);
            this.dtFrom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(73)))), ((int)(((byte)(126)))));
            this.dtFrom.Location = new System.Drawing.Point(642, 13);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(128, 23);
            this.dtFrom.TabIndex = 2;
            this.dtFrom.UseAppStyling = false;
            this.dtFrom.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.dtFrom.Value = new System.DateTime(2026, 5, 25, 0, 0, 0, 0);
            // 
            // dtTo
            // 
            this.dtTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            appearance2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(144)))), ((int)(((byte)(181)))), ((int)(((byte)(223)))));
            appearance2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(73)))), ((int)(((byte)(126)))));
            appearance2.TextHAlignAsString = "Center";
            this.dtTo.Appearance = appearance2;
            this.dtTo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.dtTo.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.dtTo.DateTime = new System.DateTime(2026, 5, 25, 0, 0, 0, 0);
            this.dtTo.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.dtTo.Font = new System.Drawing.Font("Segoe UI", 8.75F);
            this.dtTo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(73)))), ((int)(((byte)(126)))));
            this.dtTo.Location = new System.Drawing.Point(780, 13);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(128, 23);
            this.dtTo.TabIndex = 3;
            this.dtTo.UseAppStyling = false;
            this.dtTo.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.dtTo.Value = new System.DateTime(2026, 5, 25, 0, 0, 0, 0);
            // 
            // cmbQuickDate
            // 
            this.cmbQuickDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            appearance3.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(144)))), ((int)(((byte)(181)))), ((int)(((byte)(223)))));
            appearance3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(73)))), ((int)(((byte)(126)))));
            this.cmbQuickDate.Appearance = appearance3;
            this.cmbQuickDate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cmbQuickDate.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.cmbQuickDate.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.cmbQuickDate.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbQuickDate.Font = new System.Drawing.Font("Segoe UI", 8.75F);
            this.cmbQuickDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(73)))), ((int)(((byte)(126)))));
            this.cmbQuickDate.Location = new System.Drawing.Point(918, 13);
            this.cmbQuickDate.Name = "cmbQuickDate";
            this.cmbQuickDate.Size = new System.Drawing.Size(124, 23);
            this.cmbQuickDate.TabIndex = 4;
            this.cmbQuickDate.UseAppStyling = false;
            this.cmbQuickDate.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(126)))), ((int)(((byte)(235)))));
            this.btnApply.FlatAppearance.BorderSize = 0;
            this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApply.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.btnApply.ForeColor = System.Drawing.Color.White;
            this.btnApply.Location = new System.Drawing.Point(1052, 10);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(80, 28);
            this.btnApply.TabIndex = 5;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = false;
            // 
            // cardsLayout
            // 
            this.cardsLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cardsLayout.ColumnCount = 6;
            this.cardsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.cardsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.cardsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.cardsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.cardsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.cardsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.cardsLayout.Controls.Add(this.cardSales, 0, 0);
            this.cardsLayout.Controls.Add(this.cardPurchase, 1, 0);
            this.cardsLayout.Controls.Add(this.cardSalesReturn, 2, 0);
            this.cardsLayout.Controls.Add(this.cardPurchaseReturn, 3, 0);
            this.cardsLayout.Controls.Add(this.cardReceipts, 4, 0);
            this.cardsLayout.Controls.Add(this.cardPayments, 5, 0);
            this.cardsLayout.Location = new System.Drawing.Point(18, 70);
            this.cardsLayout.Margin = new System.Windows.Forms.Padding(0);
            this.cardsLayout.Name = "cardsLayout";
            this.cardsLayout.RowCount = 1;
            this.cardsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cardsLayout.Size = new System.Drawing.Size(1144, 146);
            this.cardsLayout.TabIndex = 1;
            // 
            // cardSales
            // 
            this.cardSales.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cardSales.Controls.Add(this.lblSalesCardTitle);
            this.cardSales.Controls.Add(this.lblSalesIcon);
            this.cardSales.Controls.Add(this.lblSalesFooter);
            this.cardSales.Controls.Add(this.lblSalesValue);
            this.cardSales.Location = new System.Drawing.Point(3, 3);
            this.cardSales.Name = "cardSales";
            this.cardSales.Size = new System.Drawing.Size(184, 140);
            this.cardSales.TabIndex = 0;
            // 
            // lblSalesCardTitle
            // 
            this.lblSalesCardTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.lblSalesCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(103)))));
            this.lblSalesCardTitle.Location = new System.Drawing.Point(19, 60);
            this.lblSalesCardTitle.Name = "lblSalesCardTitle";
            this.lblSalesCardTitle.Size = new System.Drawing.Size(150, 18);
            this.lblSalesCardTitle.TabIndex = 2;
            this.lblSalesCardTitle.Text = "Total Sales";
            this.lblSalesCardTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSalesIcon
            // 
            this.lblSalesIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(126)))), ((int)(((byte)(235)))));
            this.lblSalesIcon.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblSalesIcon.ForeColor = System.Drawing.Color.White;
            this.lblSalesIcon.Location = new System.Drawing.Point(22, 12);
            this.lblSalesIcon.Name = "lblSalesIcon";
            this.lblSalesIcon.Size = new System.Drawing.Size(42, 42);
            this.lblSalesIcon.TabIndex = 3;
            this.lblSalesIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSalesFooter
            // 
            this.lblSalesFooter.Font = new System.Drawing.Font("Segoe UI", 7.8F);
            this.lblSalesFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(91)))), ((int)(((byte)(145)))));
            this.lblSalesFooter.Location = new System.Drawing.Point(18, 101);
            this.lblSalesFooter.Name = "lblSalesFooter";
            this.lblSalesFooter.Size = new System.Drawing.Size(150, 18);
            this.lblSalesFooter.TabIndex = 0;
            this.lblSalesFooter.Text = "Yesterday: Rs 0.00";
            this.lblSalesFooter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSalesValue
            // 
            this.lblSalesValue.Font = new System.Drawing.Font("Segoe UI Semibold", 12.5F, System.Drawing.FontStyle.Bold);
            this.lblSalesValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(48)))), ((int)(((byte)(92)))));
            this.lblSalesValue.Location = new System.Drawing.Point(18, 76);
            this.lblSalesValue.Name = "lblSalesValue";
            this.lblSalesValue.Size = new System.Drawing.Size(150, 26);
            this.lblSalesValue.TabIndex = 1;
            this.lblSalesValue.Text = "Rs 0.00";
            this.lblSalesValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cardPurchase
            // 
            this.cardPurchase.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cardPurchase.Controls.Add(this.lblPurchaseCardTitle);
            this.cardPurchase.Controls.Add(this.lblPurchaseIcon);
            this.cardPurchase.Controls.Add(this.lblPurchaseFooter);
            this.cardPurchase.Controls.Add(this.lblPurchaseValue);
            this.cardPurchase.Location = new System.Drawing.Point(193, 3);
            this.cardPurchase.Name = "cardPurchase";
            this.cardPurchase.Size = new System.Drawing.Size(184, 140);
            this.cardPurchase.TabIndex = 1;
            // 
            // lblPurchaseCardTitle
            // 
            this.lblPurchaseCardTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.lblPurchaseCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(103)))));
            this.lblPurchaseCardTitle.Location = new System.Drawing.Point(18, 60);
            this.lblPurchaseCardTitle.Name = "lblPurchaseCardTitle";
            this.lblPurchaseCardTitle.Size = new System.Drawing.Size(150, 18);
            this.lblPurchaseCardTitle.TabIndex = 2;
            this.lblPurchaseCardTitle.Text = "Total Purchase";
            this.lblPurchaseCardTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPurchaseIcon
            // 
            this.lblPurchaseIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(176)))), ((int)(((byte)(69)))));
            this.lblPurchaseIcon.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblPurchaseIcon.ForeColor = System.Drawing.Color.White;
            this.lblPurchaseIcon.Location = new System.Drawing.Point(18, 12);
            this.lblPurchaseIcon.Name = "lblPurchaseIcon";
            this.lblPurchaseIcon.Size = new System.Drawing.Size(42, 42);
            this.lblPurchaseIcon.TabIndex = 3;
            this.lblPurchaseIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPurchaseFooter
            // 
            this.lblPurchaseFooter.Font = new System.Drawing.Font("Segoe UI", 7.8F);
            this.lblPurchaseFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(91)))), ((int)(((byte)(145)))));
            this.lblPurchaseFooter.Location = new System.Drawing.Point(18, 101);
            this.lblPurchaseFooter.Name = "lblPurchaseFooter";
            this.lblPurchaseFooter.Size = new System.Drawing.Size(150, 18);
            this.lblPurchaseFooter.TabIndex = 0;
            this.lblPurchaseFooter.Text = "Yesterday: Rs 0.00";
            this.lblPurchaseFooter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPurchaseValue
            // 
            this.lblPurchaseValue.Font = new System.Drawing.Font("Segoe UI Semibold", 12.5F, System.Drawing.FontStyle.Bold);
            this.lblPurchaseValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(48)))), ((int)(((byte)(92)))));
            this.lblPurchaseValue.Location = new System.Drawing.Point(18, 76);
            this.lblPurchaseValue.Name = "lblPurchaseValue";
            this.lblPurchaseValue.Size = new System.Drawing.Size(150, 26);
            this.lblPurchaseValue.TabIndex = 1;
            this.lblPurchaseValue.Text = "Rs 0.00";
            this.lblPurchaseValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cardSalesReturn
            // 
            this.cardSalesReturn.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cardSalesReturn.Controls.Add(this.lblSalesReturnCardTitle);
            this.cardSalesReturn.Controls.Add(this.lblSalesReturnIcon);
            this.cardSalesReturn.Controls.Add(this.lblSalesReturnFooter);
            this.cardSalesReturn.Controls.Add(this.lblSalesReturnValue);
            this.cardSalesReturn.Location = new System.Drawing.Point(383, 3);
            this.cardSalesReturn.Name = "cardSalesReturn";
            this.cardSalesReturn.Size = new System.Drawing.Size(184, 140);
            this.cardSalesReturn.TabIndex = 2;
            // 
            // lblSalesReturnCardTitle
            // 
            this.lblSalesReturnCardTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.lblSalesReturnCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(103)))));
            this.lblSalesReturnCardTitle.Location = new System.Drawing.Point(18, 60);
            this.lblSalesReturnCardTitle.Name = "lblSalesReturnCardTitle";
            this.lblSalesReturnCardTitle.Size = new System.Drawing.Size(150, 18);
            this.lblSalesReturnCardTitle.TabIndex = 2;
            this.lblSalesReturnCardTitle.Text = "Sales Return";
            this.lblSalesReturnCardTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSalesReturnIcon
            // 
            this.lblSalesReturnIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(99)))), ((int)(((byte)(94)))));
            this.lblSalesReturnIcon.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblSalesReturnIcon.ForeColor = System.Drawing.Color.White;
            this.lblSalesReturnIcon.Location = new System.Drawing.Point(18, 12);
            this.lblSalesReturnIcon.Name = "lblSalesReturnIcon";
            this.lblSalesReturnIcon.Size = new System.Drawing.Size(42, 42);
            this.lblSalesReturnIcon.TabIndex = 3;
            this.lblSalesReturnIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSalesReturnFooter
            // 
            this.lblSalesReturnFooter.Font = new System.Drawing.Font("Segoe UI", 7.8F);
            this.lblSalesReturnFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(91)))), ((int)(((byte)(145)))));
            this.lblSalesReturnFooter.Location = new System.Drawing.Point(18, 101);
            this.lblSalesReturnFooter.Name = "lblSalesReturnFooter";
            this.lblSalesReturnFooter.Size = new System.Drawing.Size(150, 18);
            this.lblSalesReturnFooter.TabIndex = 0;
            this.lblSalesReturnFooter.Text = "Today returns";
            this.lblSalesReturnFooter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSalesReturnValue
            // 
            this.lblSalesReturnValue.Font = new System.Drawing.Font("Segoe UI Semibold", 12.5F, System.Drawing.FontStyle.Bold);
            this.lblSalesReturnValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(48)))), ((int)(((byte)(92)))));
            this.lblSalesReturnValue.Location = new System.Drawing.Point(18, 76);
            this.lblSalesReturnValue.Name = "lblSalesReturnValue";
            this.lblSalesReturnValue.Size = new System.Drawing.Size(150, 26);
            this.lblSalesReturnValue.TabIndex = 1;
            this.lblSalesReturnValue.Text = "Rs 0.00";
            this.lblSalesReturnValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cardPurchaseReturn
            // 
            this.cardPurchaseReturn.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cardPurchaseReturn.Controls.Add(this.lblPurchaseReturnCardTitle);
            this.cardPurchaseReturn.Controls.Add(this.lblPurchaseReturnIcon);
            this.cardPurchaseReturn.Controls.Add(this.lblPurchaseReturnFooter);
            this.cardPurchaseReturn.Controls.Add(this.lblPurchaseReturnValue);
            this.cardPurchaseReturn.Location = new System.Drawing.Point(573, 3);
            this.cardPurchaseReturn.Name = "cardPurchaseReturn";
            this.cardPurchaseReturn.Size = new System.Drawing.Size(184, 140);
            this.cardPurchaseReturn.TabIndex = 3;
            // 
            // lblPurchaseReturnCardTitle
            // 
            this.lblPurchaseReturnCardTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.lblPurchaseReturnCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(103)))));
            this.lblPurchaseReturnCardTitle.Location = new System.Drawing.Point(18, 60);
            this.lblPurchaseReturnCardTitle.Name = "lblPurchaseReturnCardTitle";
            this.lblPurchaseReturnCardTitle.Size = new System.Drawing.Size(150, 18);
            this.lblPurchaseReturnCardTitle.TabIndex = 2;
            this.lblPurchaseReturnCardTitle.Text = "Purchase Return";
            this.lblPurchaseReturnCardTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPurchaseReturnIcon
            // 
            this.lblPurchaseReturnIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(141)))), ((int)(((byte)(35)))));
            this.lblPurchaseReturnIcon.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblPurchaseReturnIcon.ForeColor = System.Drawing.Color.White;
            this.lblPurchaseReturnIcon.Location = new System.Drawing.Point(18, 12);
            this.lblPurchaseReturnIcon.Name = "lblPurchaseReturnIcon";
            this.lblPurchaseReturnIcon.Size = new System.Drawing.Size(42, 42);
            this.lblPurchaseReturnIcon.TabIndex = 3;
            this.lblPurchaseReturnIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPurchaseReturnFooter
            // 
            this.lblPurchaseReturnFooter.Font = new System.Drawing.Font("Segoe UI", 7.8F);
            this.lblPurchaseReturnFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(91)))), ((int)(((byte)(145)))));
            this.lblPurchaseReturnFooter.Location = new System.Drawing.Point(18, 101);
            this.lblPurchaseReturnFooter.Name = "lblPurchaseReturnFooter";
            this.lblPurchaseReturnFooter.Size = new System.Drawing.Size(150, 18);
            this.lblPurchaseReturnFooter.TabIndex = 0;
            this.lblPurchaseReturnFooter.Text = "Today returns";
            this.lblPurchaseReturnFooter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPurchaseReturnValue
            // 
            this.lblPurchaseReturnValue.Font = new System.Drawing.Font("Segoe UI Semibold", 12.5F, System.Drawing.FontStyle.Bold);
            this.lblPurchaseReturnValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(48)))), ((int)(((byte)(92)))));
            this.lblPurchaseReturnValue.Location = new System.Drawing.Point(18, 76);
            this.lblPurchaseReturnValue.Name = "lblPurchaseReturnValue";
            this.lblPurchaseReturnValue.Size = new System.Drawing.Size(150, 26);
            this.lblPurchaseReturnValue.TabIndex = 1;
            this.lblPurchaseReturnValue.Text = "Rs 0.00";
            this.lblPurchaseReturnValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cardReceipts
            // 
            this.cardReceipts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cardReceipts.Controls.Add(this.lblReceiptsCardTitle);
            this.cardReceipts.Controls.Add(this.lblReceiptsIcon);
            this.cardReceipts.Controls.Add(this.lblReceiptsFooter);
            this.cardReceipts.Controls.Add(this.lblReceiptsValue);
            this.cardReceipts.Location = new System.Drawing.Point(763, 3);
            this.cardReceipts.Name = "cardReceipts";
            this.cardReceipts.Size = new System.Drawing.Size(184, 140);
            this.cardReceipts.TabIndex = 4;
            // 
            // lblReceiptsCardTitle
            // 
            this.lblReceiptsCardTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.lblReceiptsCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(103)))));
            this.lblReceiptsCardTitle.Location = new System.Drawing.Point(18, 60);
            this.lblReceiptsCardTitle.Name = "lblReceiptsCardTitle";
            this.lblReceiptsCardTitle.Size = new System.Drawing.Size(150, 18);
            this.lblReceiptsCardTitle.TabIndex = 2;
            this.lblReceiptsCardTitle.Text = "Total Receipts";
            this.lblReceiptsCardTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblReceiptsIcon
            // 
            this.lblReceiptsIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(78)))), ((int)(((byte)(218)))));
            this.lblReceiptsIcon.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblReceiptsIcon.ForeColor = System.Drawing.Color.White;
            this.lblReceiptsIcon.Location = new System.Drawing.Point(18, 12);
            this.lblReceiptsIcon.Name = "lblReceiptsIcon";
            this.lblReceiptsIcon.Size = new System.Drawing.Size(42, 42);
            this.lblReceiptsIcon.TabIndex = 3;
            this.lblReceiptsIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblReceiptsFooter
            // 
            this.lblReceiptsFooter.Font = new System.Drawing.Font("Segoe UI", 7.8F);
            this.lblReceiptsFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(91)))), ((int)(((byte)(145)))));
            this.lblReceiptsFooter.Location = new System.Drawing.Point(18, 101);
            this.lblReceiptsFooter.Name = "lblReceiptsFooter";
            this.lblReceiptsFooter.Size = new System.Drawing.Size(150, 18);
            this.lblReceiptsFooter.TabIndex = 0;
            this.lblReceiptsFooter.Text = "Voucher receipts";
            this.lblReceiptsFooter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblReceiptsValue
            // 
            this.lblReceiptsValue.Font = new System.Drawing.Font("Segoe UI Semibold", 12.5F, System.Drawing.FontStyle.Bold);
            this.lblReceiptsValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(48)))), ((int)(((byte)(92)))));
            this.lblReceiptsValue.Location = new System.Drawing.Point(18, 76);
            this.lblReceiptsValue.Name = "lblReceiptsValue";
            this.lblReceiptsValue.Size = new System.Drawing.Size(150, 26);
            this.lblReceiptsValue.TabIndex = 1;
            this.lblReceiptsValue.Text = "Rs 0.00";
            this.lblReceiptsValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cardPayments
            // 
            this.cardPayments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cardPayments.Controls.Add(this.lblPaymentsCardTitle);
            this.cardPayments.Controls.Add(this.lblPaymentsIcon);
            this.cardPayments.Controls.Add(this.lblPaymentsFooter);
            this.cardPayments.Controls.Add(this.lblPaymentsValue);
            this.cardPayments.Location = new System.Drawing.Point(953, 3);
            this.cardPayments.Name = "cardPayments";
            this.cardPayments.Size = new System.Drawing.Size(188, 140);
            this.cardPayments.TabIndex = 5;
            // 
            // lblPaymentsCardTitle
            // 
            this.lblPaymentsCardTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.lblPaymentsCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(103)))));
            this.lblPaymentsCardTitle.Location = new System.Drawing.Point(18, 60);
            this.lblPaymentsCardTitle.Name = "lblPaymentsCardTitle";
            this.lblPaymentsCardTitle.Size = new System.Drawing.Size(150, 18);
            this.lblPaymentsCardTitle.TabIndex = 2;
            this.lblPaymentsCardTitle.Text = "Total Payments";
            this.lblPaymentsCardTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPaymentsIcon
            // 
            this.lblPaymentsIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(163)))), ((int)(((byte)(181)))));
            this.lblPaymentsIcon.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblPaymentsIcon.ForeColor = System.Drawing.Color.White;
            this.lblPaymentsIcon.Location = new System.Drawing.Point(18, 12);
            this.lblPaymentsIcon.Name = "lblPaymentsIcon";
            this.lblPaymentsIcon.Size = new System.Drawing.Size(42, 42);
            this.lblPaymentsIcon.TabIndex = 3;
            this.lblPaymentsIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPaymentsFooter
            // 
            this.lblPaymentsFooter.Font = new System.Drawing.Font("Segoe UI", 7.8F);
            this.lblPaymentsFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(91)))), ((int)(((byte)(145)))));
            this.lblPaymentsFooter.Location = new System.Drawing.Point(18, 101);
            this.lblPaymentsFooter.Name = "lblPaymentsFooter";
            this.lblPaymentsFooter.Size = new System.Drawing.Size(150, 18);
            this.lblPaymentsFooter.TabIndex = 0;
            this.lblPaymentsFooter.Text = "Voucher payments";
            this.lblPaymentsFooter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPaymentsValue
            // 
            this.lblPaymentsValue.Font = new System.Drawing.Font("Segoe UI Semibold", 12.5F, System.Drawing.FontStyle.Bold);
            this.lblPaymentsValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(48)))), ((int)(((byte)(92)))));
            this.lblPaymentsValue.Location = new System.Drawing.Point(18, 76);
            this.lblPaymentsValue.Name = "lblPaymentsValue";
            this.lblPaymentsValue.Size = new System.Drawing.Size(150, 26);
            this.lblPaymentsValue.TabIndex = 1;
            this.lblPaymentsValue.Text = "Rs 0.00";
            this.lblPaymentsValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // middleLayout
            // 
            this.middleLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.middleLayout.ColumnCount = 2;
            this.middleLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58F));
            this.middleLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42F));
            this.middleLayout.Controls.Add(this.chartPanelWrapper, 0, 0);
            this.middleLayout.Controls.Add(this.topItemsPanel, 1, 0);
            this.middleLayout.Location = new System.Drawing.Point(18, 216);
            this.middleLayout.Margin = new System.Windows.Forms.Padding(0);
            this.middleLayout.Name = "middleLayout";
            this.middleLayout.RowCount = 1;
            this.middleLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.middleLayout.Size = new System.Drawing.Size(1144, 270);
            this.middleLayout.TabIndex = 2;
            // 
            // chartPanelWrapper
            // 
            this.chartPanelWrapper.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chartPanelWrapper.Controls.Add(this.pnlSalesTrend);
            this.chartPanelWrapper.Controls.Add(this.lblChartTitle);
            this.chartPanelWrapper.Location = new System.Drawing.Point(3, 3);
            this.chartPanelWrapper.Name = "chartPanelWrapper";
            this.chartPanelWrapper.Padding = new System.Windows.Forms.Padding(14, 36, 14, 14);
            this.chartPanelWrapper.Size = new System.Drawing.Size(657, 264);
            this.chartPanelWrapper.TabIndex = 0;
            // 
            // pnlSalesTrend
            // 
            this.pnlSalesTrend.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSalesTrend.Location = new System.Drawing.Point(14, 36);
            this.pnlSalesTrend.Name = "pnlSalesTrend";
            this.pnlSalesTrend.Size = new System.Drawing.Size(629, 214);
            this.pnlSalesTrend.TabIndex = 1;
            // 
            // lblChartTitle
            // 
            this.lblChartTitle.AutoSize = true;
            this.lblChartTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblChartTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(60)))), ((int)(((byte)(112)))));
            this.lblChartTitle.Location = new System.Drawing.Point(14, 12);
            this.lblChartTitle.Name = "lblChartTitle";
            this.lblChartTitle.Size = new System.Drawing.Size(137, 15);
            this.lblChartTitle.TabIndex = 0;
            this.lblChartTitle.Text = "Sales Trend (Last 7 Days)";
            // 
            // topItemsPanel
            // 
            this.topItemsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topItemsPanel.Controls.Add(this.dgvTopItems);
            this.topItemsPanel.Controls.Add(this.lblTopItemsTitle);
            this.topItemsPanel.Location = new System.Drawing.Point(666, 3);
            this.topItemsPanel.Name = "topItemsPanel";
            this.topItemsPanel.Padding = new System.Windows.Forms.Padding(14, 36, 14, 14);
            this.topItemsPanel.Size = new System.Drawing.Size(475, 264);
            this.topItemsPanel.TabIndex = 1;
            // 
            // dgvTopItems
            // 
            this.dgvTopItems.AllowUserToAddRows = false;
            this.dgvTopItems.AllowUserToDeleteRows = false;
            this.dgvTopItems.AllowUserToResizeRows = false;
            this.dgvTopItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTopItems.BackgroundColor = System.Drawing.Color.White;
            this.dgvTopItems.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvTopItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTopItems.Location = new System.Drawing.Point(14, 36);
            this.dgvTopItems.MultiSelect = false;
            this.dgvTopItems.Name = "dgvTopItems";
            this.dgvTopItems.ReadOnly = true;
            this.dgvTopItems.RowHeadersVisible = false;
            this.dgvTopItems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTopItems.Size = new System.Drawing.Size(447, 214);
            this.dgvTopItems.TabIndex = 1;
            // 
            // lblTopItemsTitle
            // 
            this.lblTopItemsTitle.AutoSize = true;
            this.lblTopItemsTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTopItemsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(60)))), ((int)(((byte)(112)))));
            this.lblTopItemsTitle.Location = new System.Drawing.Point(14, 12);
            this.lblTopItemsTitle.Name = "lblTopItemsTitle";
            this.lblTopItemsTitle.Size = new System.Drawing.Size(142, 15);
            this.lblTopItemsTitle.TabIndex = 0;
            this.lblTopItemsTitle.Text = "Top Selling Items (Today)";
            // 
            // bottomLayout
            // 
            this.bottomLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomLayout.ColumnCount = 4;
            this.bottomLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.bottomLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.bottomLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.bottomLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.bottomLayout.Controls.Add(this.stockPanel, 0, 0);
            this.bottomLayout.Controls.Add(this.customerPanel, 1, 0);
            this.bottomLayout.Controls.Add(this.vendorPanel, 2, 0);
            this.bottomLayout.Controls.Add(this.duePanel, 3, 0);
            this.bottomLayout.Location = new System.Drawing.Point(18, 486);
            this.bottomLayout.Margin = new System.Windows.Forms.Padding(0);
            this.bottomLayout.Name = "bottomLayout";
            this.bottomLayout.RowCount = 1;
            this.bottomLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bottomLayout.Size = new System.Drawing.Size(1144, 112);
            this.bottomLayout.TabIndex = 3;
            // 
            // stockPanel
            // 
            this.stockPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stockPanel.Controls.Add(this.lblStockOutValue);
            this.stockPanel.Controls.Add(this.lblStockLowValue);
            this.stockPanel.Controls.Add(this.lblStockTotalValue);
            this.stockPanel.Controls.Add(this.lblStockOutIcon);
            this.stockPanel.Controls.Add(this.lblStockLowIcon);
            this.stockPanel.Controls.Add(this.lblStockTotalIcon);
            this.stockPanel.Controls.Add(this.lblStockOutCaption);
            this.stockPanel.Controls.Add(this.lblStockLowCaption);
            this.stockPanel.Controls.Add(this.lblStockTotalCaption);
            this.stockPanel.Controls.Add(this.lblStockSummary);
            this.stockPanel.Controls.Add(this.lblStockTitle);
            this.stockPanel.Controls.Add(this.lblStockIcon);
            this.stockPanel.Location = new System.Drawing.Point(3, 3);
            this.stockPanel.Name = "stockPanel";
            this.stockPanel.Size = new System.Drawing.Size(280, 106);
            this.stockPanel.TabIndex = 0;
            // 
            // lblStockOutValue
            // 
            this.lblStockOutValue.BackColor = System.Drawing.Color.Transparent;
            this.lblStockOutValue.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblStockOutValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.lblStockOutValue.Location = new System.Drawing.Point(157, 71);
            this.lblStockOutValue.Name = "lblStockOutValue";
            this.lblStockOutValue.Size = new System.Drawing.Size(86, 16);
            this.lblStockOutValue.TabIndex = 5;
            this.lblStockOutValue.Text = "7";
            this.lblStockOutValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStockLowValue
            // 
            this.lblStockLowValue.BackColor = System.Drawing.Color.Transparent;
            this.lblStockLowValue.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblStockLowValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(126)))), ((int)(((byte)(22)))));
            this.lblStockLowValue.Location = new System.Drawing.Point(146, 52);
            this.lblStockLowValue.Name = "lblStockLowValue";
            this.lblStockLowValue.Size = new System.Drawing.Size(97, 16);
            this.lblStockLowValue.TabIndex = 4;
            this.lblStockLowValue.Text = "23";
            this.lblStockLowValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStockTotalValue
            // 
            this.lblStockTotalValue.BackColor = System.Drawing.Color.Transparent;
            this.lblStockTotalValue.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblStockTotalValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(160)))), ((int)(((byte)(70)))));
            this.lblStockTotalValue.Location = new System.Drawing.Point(188, 33);
            this.lblStockTotalValue.Name = "lblStockTotalValue";
            this.lblStockTotalValue.Size = new System.Drawing.Size(56, 16);
            this.lblStockTotalValue.TabIndex = 3;
            this.lblStockTotalValue.Text = "1,248";
            this.lblStockTotalValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStockOutIcon
            // 
            this.lblStockOutIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.lblStockOutIcon.Location = new System.Drawing.Point(18, 74);
            this.lblStockOutIcon.Name = "lblStockOutIcon";
            this.lblStockOutIcon.Size = new System.Drawing.Size(12, 12);
            this.lblStockOutIcon.TabIndex = 8;
            // 
            // lblStockLowIcon
            // 
            this.lblStockLowIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(126)))), ((int)(((byte)(22)))));
            this.lblStockLowIcon.Location = new System.Drawing.Point(18, 55);
            this.lblStockLowIcon.Name = "lblStockLowIcon";
            this.lblStockLowIcon.Size = new System.Drawing.Size(12, 12);
            this.lblStockLowIcon.TabIndex = 7;
            // 
            // lblStockTotalIcon
            // 
            this.lblStockTotalIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(160)))), ((int)(((byte)(70)))));
            this.lblStockTotalIcon.Location = new System.Drawing.Point(18, 36);
            this.lblStockTotalIcon.Name = "lblStockTotalIcon";
            this.lblStockTotalIcon.Size = new System.Drawing.Size(12, 12);
            this.lblStockTotalIcon.TabIndex = 6;
            // 
            // lblStockOutCaption
            // 
            this.lblStockOutCaption.BackColor = System.Drawing.Color.Transparent;
            this.lblStockOutCaption.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblStockOutCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(73)))), ((int)(((byte)(126)))));
            this.lblStockOutCaption.Location = new System.Drawing.Point(36, 71);
            this.lblStockOutCaption.Name = "lblStockOutCaption";
            this.lblStockOutCaption.Size = new System.Drawing.Size(132, 16);
            this.lblStockOutCaption.TabIndex = 11;
            this.lblStockOutCaption.Text = "Out of Stock Items";
            this.lblStockOutCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStockLowCaption
            // 
            this.lblStockLowCaption.BackColor = System.Drawing.Color.Transparent;
            this.lblStockLowCaption.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblStockLowCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(73)))), ((int)(((byte)(126)))));
            this.lblStockLowCaption.Location = new System.Drawing.Point(36, 52);
            this.lblStockLowCaption.Name = "lblStockLowCaption";
            this.lblStockLowCaption.Size = new System.Drawing.Size(132, 16);
            this.lblStockLowCaption.TabIndex = 10;
            this.lblStockLowCaption.Text = "Low Stock Items";
            this.lblStockLowCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStockTotalCaption
            // 
            this.lblStockTotalCaption.BackColor = System.Drawing.Color.Transparent;
            this.lblStockTotalCaption.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblStockTotalCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(73)))), ((int)(((byte)(126)))));
            this.lblStockTotalCaption.Location = new System.Drawing.Point(36, 33);
            this.lblStockTotalCaption.Name = "lblStockTotalCaption";
            this.lblStockTotalCaption.Size = new System.Drawing.Size(132, 16);
            this.lblStockTotalCaption.TabIndex = 9;
            this.lblStockTotalCaption.Text = "Total Items";
            this.lblStockTotalCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStockSummary
            // 
            this.lblStockSummary.Location = new System.Drawing.Point(40, 36);
            this.lblStockSummary.Name = "lblStockSummary";
            this.lblStockSummary.Size = new System.Drawing.Size(1, 1);
            this.lblStockSummary.TabIndex = 0;
            this.lblStockSummary.Visible = false;
            // 
            // lblStockTitle
            // 
            this.lblStockTitle.Location = new System.Drawing.Point(16, 9);
            this.lblStockTitle.Name = "lblStockTitle";
            this.lblStockTitle.Size = new System.Drawing.Size(230, 18);
            this.lblStockTitle.TabIndex = 1;
            this.lblStockTitle.Text = "Stock Summary";
            // 
            // lblStockIcon
            // 
            this.lblStockIcon.BackColor = System.Drawing.Color.Transparent;
            this.lblStockIcon.Location = new System.Drawing.Point(18, 42);
            this.lblStockIcon.Name = "lblStockIcon";
            this.lblStockIcon.Size = new System.Drawing.Size(1, 1);
            this.lblStockIcon.TabIndex = 2;
            this.lblStockIcon.Visible = false;
            // 
            // customerPanel
            // 
            this.customerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customerPanel.Controls.Add(this.lblCustomerCaption);
            this.customerPanel.Controls.Add(this.lblCustomerValue);
            this.customerPanel.Controls.Add(this.lblCustomerTitle);
            this.customerPanel.Controls.Add(this.lblCustomerIcon);
            this.customerPanel.Location = new System.Drawing.Point(289, 3);
            this.customerPanel.Name = "customerPanel";
            this.customerPanel.Size = new System.Drawing.Size(280, 106);
            this.customerPanel.TabIndex = 1;
            // 
            // lblCustomerCaption
            // 
            this.lblCustomerCaption.BackColor = System.Drawing.Color.Transparent;
            this.lblCustomerCaption.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCustomerCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(73)))), ((int)(((byte)(126)))));
            this.lblCustomerCaption.Location = new System.Drawing.Point(78, 63);
            this.lblCustomerCaption.Name = "lblCustomerCaption";
            this.lblCustomerCaption.Size = new System.Drawing.Size(150, 18);
            this.lblCustomerCaption.TabIndex = 3;
            this.lblCustomerCaption.Text = "Active Customers";
            this.lblCustomerCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCustomerValue
            // 
            this.lblCustomerValue.BackColor = System.Drawing.Color.Transparent;
            this.lblCustomerValue.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lblCustomerValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(48)))), ((int)(((byte)(92)))));
            this.lblCustomerValue.Location = new System.Drawing.Point(78, 37);
            this.lblCustomerValue.Name = "lblCustomerValue";
            this.lblCustomerValue.Size = new System.Drawing.Size(150, 28);
            this.lblCustomerValue.TabIndex = 0;
            this.lblCustomerValue.Text = "563";
            this.lblCustomerValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCustomerTitle
            // 
            this.lblCustomerTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblCustomerTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblCustomerTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(60)))), ((int)(((byte)(112)))));
            this.lblCustomerTitle.Location = new System.Drawing.Point(16, 6);
            this.lblCustomerTitle.Name = "lblCustomerTitle";
            this.lblCustomerTitle.Size = new System.Drawing.Size(230, 18);
            this.lblCustomerTitle.TabIndex = 1;
            this.lblCustomerTitle.Text = "Total Customers";
            // 
            // lblCustomerIcon
            // 
            this.lblCustomerIcon.BackColor = System.Drawing.Color.Transparent;
            this.lblCustomerIcon.Location = new System.Drawing.Point(22, 38);
            this.lblCustomerIcon.Name = "lblCustomerIcon";
            this.lblCustomerIcon.Size = new System.Drawing.Size(42, 42);
            this.lblCustomerIcon.TabIndex = 2;
            // 
            // vendorPanel
            // 
            this.vendorPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vendorPanel.Controls.Add(this.lblVendorCaption);
            this.vendorPanel.Controls.Add(this.lblVendorValue);
            this.vendorPanel.Controls.Add(this.lblVendorTitle);
            this.vendorPanel.Controls.Add(this.lblVendorIcon);
            this.vendorPanel.Location = new System.Drawing.Point(575, 3);
            this.vendorPanel.Name = "vendorPanel";
            this.vendorPanel.Size = new System.Drawing.Size(280, 106);
            this.vendorPanel.TabIndex = 2;
            // 
            // lblVendorCaption
            // 
            this.lblVendorCaption.BackColor = System.Drawing.Color.Transparent;
            this.lblVendorCaption.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblVendorCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(73)))), ((int)(((byte)(126)))));
            this.lblVendorCaption.Location = new System.Drawing.Point(78, 63);
            this.lblVendorCaption.Name = "lblVendorCaption";
            this.lblVendorCaption.Size = new System.Drawing.Size(150, 18);
            this.lblVendorCaption.TabIndex = 3;
            this.lblVendorCaption.Text = "Active Vendors";
            this.lblVendorCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblVendorValue
            // 
            this.lblVendorValue.BackColor = System.Drawing.Color.Transparent;
            this.lblVendorValue.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lblVendorValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(48)))), ((int)(((byte)(92)))));
            this.lblVendorValue.Location = new System.Drawing.Point(78, 37);
            this.lblVendorValue.Name = "lblVendorValue";
            this.lblVendorValue.Size = new System.Drawing.Size(150, 28);
            this.lblVendorValue.TabIndex = 0;
            this.lblVendorValue.Text = "198";
            this.lblVendorValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblVendorTitle
            // 
            this.lblVendorTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblVendorTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblVendorTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(60)))), ((int)(((byte)(112)))));
            this.lblVendorTitle.Location = new System.Drawing.Point(16, 6);
            this.lblVendorTitle.Name = "lblVendorTitle";
            this.lblVendorTitle.Size = new System.Drawing.Size(230, 18);
            this.lblVendorTitle.TabIndex = 1;
            this.lblVendorTitle.Text = "Total Vendors";
            // 
            // lblVendorIcon
            // 
            this.lblVendorIcon.BackColor = System.Drawing.Color.Transparent;
            this.lblVendorIcon.Location = new System.Drawing.Point(22, 38);
            this.lblVendorIcon.Name = "lblVendorIcon";
            this.lblVendorIcon.Size = new System.Drawing.Size(42, 42);
            this.lblVendorIcon.TabIndex = 2;
            // 
            // duePanel
            // 
            this.duePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.duePanel.Controls.Add(this.lblDueSummary);
            this.duePanel.Controls.Add(this.lblDueTitle);
            this.duePanel.Controls.Add(this.lblDueIcon);
            this.duePanel.Location = new System.Drawing.Point(861, 3);
            this.duePanel.Name = "duePanel";
            this.duePanel.Size = new System.Drawing.Size(280, 106);
            this.duePanel.TabIndex = 3;
            // 
            // lblDueSummary
            // 
            this.lblDueSummary.Location = new System.Drawing.Point(65, 33);
            this.lblDueSummary.Name = "lblDueSummary";
            this.lblDueSummary.Size = new System.Drawing.Size(210, 54);
            this.lblDueSummary.TabIndex = 0;
            // 
            // lblDueTitle
            // 
            this.lblDueTitle.Location = new System.Drawing.Point(16, 10);
            this.lblDueTitle.Name = "lblDueTitle";
            this.lblDueTitle.Size = new System.Drawing.Size(230, 18);
            this.lblDueTitle.TabIndex = 1;
            this.lblDueTitle.Text = "Outstanding";
            // 
            // lblDueIcon
            // 
            this.lblDueIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(141)))), ((int)(((byte)(35)))));
            this.lblDueIcon.Location = new System.Drawing.Point(9, 34);
            this.lblDueIcon.Name = "lblDueIcon";
            this.lblDueIcon.Size = new System.Drawing.Size(42, 42);
            this.lblDueIcon.TabIndex = 2;
            // 
            // FrmDashboardOverview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.ClientSize = new System.Drawing.Size(1180, 610);
            this.Controls.Add(this.mainLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmDashboardOverview";
            this.Text = "Overview";
            this.mainLayout.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbQuickDate)).EndInit();
            this.cardsLayout.ResumeLayout(false);
            this.cardSales.ResumeLayout(false);
            this.cardPurchase.ResumeLayout(false);
            this.cardSalesReturn.ResumeLayout(false);
            this.cardPurchaseReturn.ResumeLayout(false);
            this.cardReceipts.ResumeLayout(false);
            this.cardPayments.ResumeLayout(false);
            this.middleLayout.ResumeLayout(false);
            this.chartPanelWrapper.ResumeLayout(false);
            this.chartPanelWrapper.PerformLayout();
            this.topItemsPanel.ResumeLayout(false);
            this.topItemsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTopItems)).EndInit();
            this.bottomLayout.ResumeLayout(false);
            this.stockPanel.ResumeLayout(false);
            this.customerPanel.ResumeLayout(false);
            this.vendorPanel.ResumeLayout(false);
            this.duePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

    }
}
