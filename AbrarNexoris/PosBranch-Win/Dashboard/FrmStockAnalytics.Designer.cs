namespace PosBranch_Win.Dashboard
{
    partial class FrmStockAnalytics
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label lblTitle;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbQuickDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private System.Windows.Forms.ComboBox cmbAnalysisMode;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.TableLayoutPanel metricsLayout;
        private System.Windows.Forms.Panel cardCurrentStockValue;
        private System.Windows.Forms.Panel cardTotalItems;
        private System.Windows.Forms.Panel cardStockValue;
        private System.Windows.Forms.Panel cardStockQuantity;
        private System.Windows.Forms.Panel cardLowStock;
        private System.Windows.Forms.Panel cardOutStock;
        private System.Windows.Forms.Label iconCurrentStockValue;
        private System.Windows.Forms.Label iconTotalItems;
        private System.Windows.Forms.Label iconStockValue;
        private System.Windows.Forms.Label iconStockQuantity;
        private System.Windows.Forms.Label iconLowStock;
        private System.Windows.Forms.Label iconOutStock;
        private System.Windows.Forms.Label lblCurrentStockValueTitle;
        private System.Windows.Forms.Label lblTotalItemsTitle;
        private System.Windows.Forms.Label lblStockValueTitle;
        private System.Windows.Forms.Label lblStockQuantityTitle;
        private System.Windows.Forms.Label lblLowStockTitle;
        private System.Windows.Forms.Label lblOutStockTitle;
        private System.Windows.Forms.Label lblCurrentStockValue;
        private System.Windows.Forms.Label lblTotalItems;
        private System.Windows.Forms.Label lblStockValue;
        private System.Windows.Forms.Label lblStockQuantity;
        private System.Windows.Forms.Label lblLowStock;
        private System.Windows.Forms.Label lblOutStock;
        private System.Windows.Forms.Label lblCurrentStockValueFooter;
        private System.Windows.Forms.Label lblTotalItemsFooter;
        private System.Windows.Forms.Label lblStockValueFooter;
        private System.Windows.Forms.Label lblStockQuantityFooter;
        private System.Windows.Forms.Label lblLowStockFooter;
        private System.Windows.Forms.Label lblOutStockFooter;
        private System.Windows.Forms.TableLayoutPanel topContentLayout;
        private System.Windows.Forms.Panel cardTrend;
        private System.Windows.Forms.Panel cardTopItems;
        private System.Windows.Forms.Panel trendCanvas;
        private System.Windows.Forms.DataGridView gridTopItems;
        private System.Windows.Forms.TableLayoutPanel middleContentLayout;
        private System.Windows.Forms.Panel itemGraphCanvas;
        private System.Windows.Forms.Panel categoryCanvas;
        private System.Windows.Forms.Panel categoryLegendPanel;
        private System.Windows.Forms.TableLayoutPanel bottomContentLayout;
        private System.Windows.Forms.Panel cardLowStockList;
        private System.Windows.Forms.Panel cardMovement;
        private System.Windows.Forms.Panel cardOutStockList;
        private System.Windows.Forms.Label lblLowStockListTitle;
        private System.Windows.Forms.Label lblMovementTitle;
        private System.Windows.Forms.Label lblOutStockListTitle;
        private System.Windows.Forms.DataGridView gridLowStock;
        private System.Windows.Forms.TableLayoutPanel movementLayout;
        private System.Windows.Forms.Label lblFastMoving;
        private System.Windows.Forms.Label lblSlowMoving;
        private System.Windows.Forms.Label lblDeadStock;
        private System.Windows.Forms.DataGridView gridOutStock;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.cmbQuickDate = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.btnApply = new System.Windows.Forms.Button();
            this.metricsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.cardCurrentStockValue = new System.Windows.Forms.Panel();
            this.iconCurrentStockValue = new System.Windows.Forms.Label();
            this.lblCurrentStockValueTitle = new System.Windows.Forms.Label();
            this.lblCurrentStockValue = new System.Windows.Forms.Label();
            this.lblCurrentStockValueFooter = new System.Windows.Forms.Label();
            this.cardTotalItems = new System.Windows.Forms.Panel();
            this.iconTotalItems = new System.Windows.Forms.Label();
            this.lblTotalItemsTitle = new System.Windows.Forms.Label();
            this.lblTotalItems = new System.Windows.Forms.Label();
            this.lblTotalItemsFooter = new System.Windows.Forms.Label();
            this.cardStockValue = new System.Windows.Forms.Panel();
            this.iconStockValue = new System.Windows.Forms.Label();
            this.lblStockValueTitle = new System.Windows.Forms.Label();
            this.lblStockValue = new System.Windows.Forms.Label();
            this.lblStockValueFooter = new System.Windows.Forms.Label();
            this.cardStockQuantity = new System.Windows.Forms.Panel();
            this.iconStockQuantity = new System.Windows.Forms.Label();
            this.lblStockQuantityTitle = new System.Windows.Forms.Label();
            this.lblStockQuantity = new System.Windows.Forms.Label();
            this.lblStockQuantityFooter = new System.Windows.Forms.Label();
            this.cardLowStock = new System.Windows.Forms.Panel();
            this.iconLowStock = new System.Windows.Forms.Label();
            this.lblLowStockTitle = new System.Windows.Forms.Label();
            this.lblLowStock = new System.Windows.Forms.Label();
            this.lblLowStockFooter = new System.Windows.Forms.Label();
            this.cardOutStock = new System.Windows.Forms.Panel();
            this.iconOutStock = new System.Windows.Forms.Label();
            this.lblOutStockTitle = new System.Windows.Forms.Label();
            this.lblOutStock = new System.Windows.Forms.Label();
            this.lblOutStockFooter = new System.Windows.Forms.Label();
            this.topContentLayout = new System.Windows.Forms.TableLayoutPanel();
            this.cardTrend = new System.Windows.Forms.Panel();
            this.cmbAnalysisMode = new System.Windows.Forms.ComboBox();
            this.trendCanvas = new System.Windows.Forms.Panel();
            this.lblTrendTitle = new System.Windows.Forms.Label();
            this.cardTopItems = new System.Windows.Forms.Panel();
            this.gridTopItems = new System.Windows.Forms.DataGridView();
            this.lblTopItemsTitle = new System.Windows.Forms.Label();
            this.middleContentLayout = new System.Windows.Forms.TableLayoutPanel();
            this.cardItemGraph = new System.Windows.Forms.Panel();
            this.itemGraphCanvas = new System.Windows.Forms.Panel();
            this.lblItemGraphTitle = new System.Windows.Forms.Label();
            this.cardCategory = new System.Windows.Forms.Panel();
            this.categoryLegendPanel = new System.Windows.Forms.Panel();
            this.categoryCanvas = new System.Windows.Forms.Panel();
            this.lblCategoryTitle = new System.Windows.Forms.Label();
            this.cardSummary = new System.Windows.Forms.Panel();
            this.lblSummary = new System.Windows.Forms.Label();
            this.lblSummaryTitle = new System.Windows.Forms.Label();
            this.bottomContentLayout = new System.Windows.Forms.TableLayoutPanel();
            this.cardLowStockList = new System.Windows.Forms.Panel();
            this.gridLowStock = new System.Windows.Forms.DataGridView();
            this.lblLowStockListTitle = new System.Windows.Forms.Label();
            this.cardMovement = new System.Windows.Forms.Panel();
            this.movementLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblFastMoving = new System.Windows.Forms.Label();
            this.lblSlowMoving = new System.Windows.Forms.Label();
            this.lblDeadStock = new System.Windows.Forms.Label();
            this.lblMovementTitle = new System.Windows.Forms.Label();
            this.cardOutStockList = new System.Windows.Forms.Panel();
            this.gridOutStock = new System.Windows.Forms.DataGridView();
            this.lblOutStockListTitle = new System.Windows.Forms.Label();
            this.mainLayout.SuspendLayout();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbQuickDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            this.metricsLayout.SuspendLayout();
            this.cardCurrentStockValue.SuspendLayout();
            this.cardTotalItems.SuspendLayout();
            this.cardStockValue.SuspendLayout();
            this.cardStockQuantity.SuspendLayout();
            this.cardLowStock.SuspendLayout();
            this.cardOutStock.SuspendLayout();
            this.topContentLayout.SuspendLayout();
            this.cardTrend.SuspendLayout();
            this.cardTopItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTopItems)).BeginInit();
            this.middleContentLayout.SuspendLayout();
            this.cardItemGraph.SuspendLayout();
            this.cardCategory.SuspendLayout();
            this.cardSummary.SuspendLayout();
            this.bottomContentLayout.SuspendLayout();
            this.cardLowStockList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLowStock)).BeginInit();
            this.cardMovement.SuspendLayout();
            this.movementLayout.SuspendLayout();
            this.cardOutStockList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridOutStock)).BeginInit();
            this.SuspendLayout();
            // 
            // mainLayout
            // 
            this.mainLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.mainLayout.ColumnCount = 1;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.Controls.Add(this.headerPanel, 0, 0);
            this.mainLayout.Controls.Add(this.metricsLayout, 0, 1);
            this.mainLayout.Controls.Add(this.topContentLayout, 0, 2);
            this.mainLayout.Controls.Add(this.middleContentLayout, 0, 3);
            this.mainLayout.Controls.Add(this.bottomContentLayout, 0, 4);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.Padding = new System.Windows.Forms.Padding(18, 8, 18, 8);
            this.mainLayout.RowCount = 5;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 86F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 56F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 44F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 152F));
            this.mainLayout.Size = new System.Drawing.Size(1180, 650);
            this.mainLayout.TabIndex = 0;
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.headerPanel.Controls.Add(this.lblTitle);
            this.headerPanel.Controls.Add(this.cmbQuickDate);
            this.headerPanel.Controls.Add(this.dtFrom);
            this.headerPanel.Controls.Add(this.dtTo);
            this.headerPanel.Controls.Add(this.btnApply);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerPanel.Location = new System.Drawing.Point(21, 11);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(1138, 38);
            this.headerPanel.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 12.5F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTitle.Location = new System.Drawing.Point(4, 3);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(203, 23);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Stock Analytics Overview";
            // 
            // cmbQuickDate
            // 
            this.cmbQuickDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbQuickDate.Location = new System.Drawing.Point(896, 7);
            this.cmbQuickDate.Name = "cmbQuickDate";
            this.cmbQuickDate.Size = new System.Drawing.Size(104, 21);
            this.cmbQuickDate.TabIndex = 1;
            // 
            // dtFrom
            // 
            this.dtFrom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dtFrom.Location = new System.Drawing.Point(624, 8);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(128, 21);
            this.dtFrom.TabIndex = 2;
            // 
            // dtTo
            // 
            this.dtTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dtTo.Location = new System.Drawing.Point(762, 8);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(128, 21);
            this.dtTo.TabIndex = 3;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(126)))), ((int)(((byte)(235)))));
            this.btnApply.FlatAppearance.BorderSize = 0;
            this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApply.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.btnApply.ForeColor = System.Drawing.Color.White;
            this.btnApply.Location = new System.Drawing.Point(1020, 5);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(88, 28);
            this.btnApply.TabIndex = 5;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = false;
            this.btnApply.Click += new System.EventHandler(this.BtnApply_Click);
            // 
            // metricsLayout
            // 
            this.metricsLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.metricsLayout.ColumnCount = 6;
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.metricsLayout.Controls.Add(this.cardCurrentStockValue, 0, 0);
            this.metricsLayout.Controls.Add(this.cardTotalItems, 1, 0);
            this.metricsLayout.Controls.Add(this.cardStockValue, 2, 0);
            this.metricsLayout.Controls.Add(this.cardStockQuantity, 3, 0);
            this.metricsLayout.Controls.Add(this.cardLowStock, 4, 0);
            this.metricsLayout.Controls.Add(this.cardOutStock, 5, 0);
            this.metricsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metricsLayout.Location = new System.Drawing.Point(18, 52);
            this.metricsLayout.Margin = new System.Windows.Forms.Padding(0);
            this.metricsLayout.Name = "metricsLayout";
            this.metricsLayout.Padding = new System.Windows.Forms.Padding(0, 4, 0, 8);
            this.metricsLayout.RowCount = 1;
            this.metricsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.metricsLayout.Size = new System.Drawing.Size(1144, 86);
            this.metricsLayout.TabIndex = 1;
            // 
            // cardCurrentStockValue
            // 
            this.cardCurrentStockValue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardCurrentStockValue.Controls.Add(this.iconCurrentStockValue);
            this.cardCurrentStockValue.Controls.Add(this.lblCurrentStockValueTitle);
            this.cardCurrentStockValue.Controls.Add(this.lblCurrentStockValue);
            this.cardCurrentStockValue.Controls.Add(this.lblCurrentStockValueFooter);
            this.cardCurrentStockValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardCurrentStockValue.Location = new System.Drawing.Point(0, 4);
            this.cardCurrentStockValue.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardCurrentStockValue.Name = "cardCurrentStockValue";
            this.cardCurrentStockValue.Padding = new System.Windows.Forms.Padding(12);
            this.cardCurrentStockValue.Size = new System.Drawing.Size(178, 74);
            this.cardCurrentStockValue.TabIndex = 0;
            this.cardCurrentStockValue.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // iconCurrentStockValue
            // 
            this.iconCurrentStockValue.BackColor = System.Drawing.Color.Transparent;
            this.iconCurrentStockValue.Location = new System.Drawing.Point(12, 20);
            this.iconCurrentStockValue.Name = "iconCurrentStockValue";
            this.iconCurrentStockValue.Size = new System.Drawing.Size(38, 38);
            this.iconCurrentStockValue.TabIndex = 0;
            this.iconCurrentStockValue.Tag = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(126)))), ((int)(((byte)(235)))));
            this.iconCurrentStockValue.Paint += new System.Windows.Forms.PaintEventHandler(this.MetricIcon_Paint);
            // 
            // lblCurrentStockValueTitle
            // 
            this.lblCurrentStockValueTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblCurrentStockValueTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 7.7F, System.Drawing.FontStyle.Bold);
            this.lblCurrentStockValueTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblCurrentStockValueTitle.Location = new System.Drawing.Point(55, 10);
            this.lblCurrentStockValueTitle.Name = "lblCurrentStockValueTitle";
            this.lblCurrentStockValueTitle.Size = new System.Drawing.Size(108, 18);
            this.lblCurrentStockValueTitle.TabIndex = 1;
            this.lblCurrentStockValueTitle.Text = "Current Stock Value";
            // 
            // lblCurrentStockValue
            // 
            this.lblCurrentStockValue.BackColor = System.Drawing.Color.Transparent;
            this.lblCurrentStockValue.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold);
            this.lblCurrentStockValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblCurrentStockValue.Location = new System.Drawing.Point(55, 28);
            this.lblCurrentStockValue.Name = "lblCurrentStockValue";
            this.lblCurrentStockValue.Size = new System.Drawing.Size(108, 23);
            this.lblCurrentStockValue.TabIndex = 2;
            this.lblCurrentStockValue.Text = "0";
            // 
            // lblCurrentStockValueFooter
            // 
            this.lblCurrentStockValueFooter.BackColor = System.Drawing.Color.Transparent;
            this.lblCurrentStockValueFooter.Font = new System.Drawing.Font("Segoe UI", 7.2F);
            this.lblCurrentStockValueFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(93)))), ((int)(((byte)(132)))));
            this.lblCurrentStockValueFooter.Location = new System.Drawing.Point(55, 52);
            this.lblCurrentStockValueFooter.Name = "lblCurrentStockValueFooter";
            this.lblCurrentStockValueFooter.Size = new System.Drawing.Size(108, 18);
            this.lblCurrentStockValueFooter.TabIndex = 3;
            this.lblCurrentStockValueFooter.Text = "Total inventory value";
            // 
            // cardTotalItems
            // 
            this.cardTotalItems.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardTotalItems.Controls.Add(this.iconTotalItems);
            this.cardTotalItems.Controls.Add(this.lblTotalItemsTitle);
            this.cardTotalItems.Controls.Add(this.lblTotalItems);
            this.cardTotalItems.Controls.Add(this.lblTotalItemsFooter);
            this.cardTotalItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardTotalItems.Location = new System.Drawing.Point(190, 4);
            this.cardTotalItems.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardTotalItems.Name = "cardTotalItems";
            this.cardTotalItems.Padding = new System.Windows.Forms.Padding(12);
            this.cardTotalItems.Size = new System.Drawing.Size(178, 74);
            this.cardTotalItems.TabIndex = 1;
            this.cardTotalItems.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // iconTotalItems
            // 
            this.iconTotalItems.Location = new System.Drawing.Point(12, 20);
            this.iconTotalItems.Name = "iconTotalItems";
            this.iconTotalItems.Size = new System.Drawing.Size(38, 38);
            this.iconTotalItems.TabIndex = 0;
            this.iconTotalItems.Tag = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(178)))), ((int)(((byte)(72)))));
            this.iconTotalItems.Paint += new System.Windows.Forms.PaintEventHandler(this.MetricIcon_Paint);
            // 
            // lblTotalItemsTitle
            // 
            this.lblTotalItemsTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 7.7F, System.Drawing.FontStyle.Bold);
            this.lblTotalItemsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTotalItemsTitle.Location = new System.Drawing.Point(55, 10);
            this.lblTotalItemsTitle.Name = "lblTotalItemsTitle";
            this.lblTotalItemsTitle.Size = new System.Drawing.Size(80, 18);
            this.lblTotalItemsTitle.TabIndex = 1;
            this.lblTotalItemsTitle.Text = "Total Items";
            // 
            // lblTotalItems
            // 
            this.lblTotalItems.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold);
            this.lblTotalItems.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTotalItems.Location = new System.Drawing.Point(53, 28);
            this.lblTotalItems.Name = "lblTotalItems";
            this.lblTotalItems.Size = new System.Drawing.Size(123, 23);
            this.lblTotalItems.TabIndex = 2;
            this.lblTotalItems.Text = "0";
            // 
            // lblTotalItemsFooter
            // 
            this.lblTotalItemsFooter.Font = new System.Drawing.Font("Segoe UI", 7.2F);
            this.lblTotalItemsFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(93)))), ((int)(((byte)(132)))));
            this.lblTotalItemsFooter.Location = new System.Drawing.Point(55, 52);
            this.lblTotalItemsFooter.Name = "lblTotalItemsFooter";
            this.lblTotalItemsFooter.Size = new System.Drawing.Size(108, 18);
            this.lblTotalItemsFooter.TabIndex = 3;
            this.lblTotalItemsFooter.Text = "Total unique items";
            // 
            // cardStockValue
            // 
            this.cardStockValue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardStockValue.Controls.Add(this.iconStockValue);
            this.cardStockValue.Controls.Add(this.lblStockValueTitle);
            this.cardStockValue.Controls.Add(this.lblStockValue);
            this.cardStockValue.Controls.Add(this.lblStockValueFooter);
            this.cardStockValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardStockValue.Location = new System.Drawing.Point(380, 4);
            this.cardStockValue.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardStockValue.Name = "cardStockValue";
            this.cardStockValue.Padding = new System.Windows.Forms.Padding(12);
            this.cardStockValue.Size = new System.Drawing.Size(178, 74);
            this.cardStockValue.TabIndex = 2;
            this.cardStockValue.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // iconStockValue
            // 
            this.iconStockValue.Location = new System.Drawing.Point(12, 20);
            this.iconStockValue.Name = "iconStockValue";
            this.iconStockValue.Size = new System.Drawing.Size(38, 38);
            this.iconStockValue.TabIndex = 0;
            this.iconStockValue.Tag = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(75)))), ((int)(((byte)(218)))));
            this.iconStockValue.Paint += new System.Windows.Forms.PaintEventHandler(this.MetricIcon_Paint);
            // 
            // lblStockValueTitle
            // 
            this.lblStockValueTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 7.7F, System.Drawing.FontStyle.Bold);
            this.lblStockValueTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblStockValueTitle.Location = new System.Drawing.Point(55, 10);
            this.lblStockValueTitle.Name = "lblStockValueTitle";
            this.lblStockValueTitle.Size = new System.Drawing.Size(78, 18);
            this.lblStockValueTitle.TabIndex = 1;
            this.lblStockValueTitle.Text = "Stock Value";
            // 
            // lblStockValue
            // 
            this.lblStockValue.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold);
            this.lblStockValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblStockValue.Location = new System.Drawing.Point(53, 28);
            this.lblStockValue.Name = "lblStockValue";
            this.lblStockValue.Size = new System.Drawing.Size(123, 23);
            this.lblStockValue.TabIndex = 2;
            this.lblStockValue.Text = "0";
            // 
            // lblStockValueFooter
            // 
            this.lblStockValueFooter.Font = new System.Drawing.Font("Segoe UI", 7.2F);
            this.lblStockValueFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(93)))), ((int)(((byte)(132)))));
            this.lblStockValueFooter.Location = new System.Drawing.Point(55, 52);
            this.lblStockValueFooter.Name = "lblStockValueFooter";
            this.lblStockValueFooter.Size = new System.Drawing.Size(89, 18);
            this.lblStockValueFooter.TabIndex = 3;
            this.lblStockValueFooter.Text = "Selected range";
            // 
            // cardStockQuantity
            // 
            this.cardStockQuantity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardStockQuantity.Controls.Add(this.iconStockQuantity);
            this.cardStockQuantity.Controls.Add(this.lblStockQuantityTitle);
            this.cardStockQuantity.Controls.Add(this.lblStockQuantity);
            this.cardStockQuantity.Controls.Add(this.lblStockQuantityFooter);
            this.cardStockQuantity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardStockQuantity.Location = new System.Drawing.Point(570, 4);
            this.cardStockQuantity.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardStockQuantity.Name = "cardStockQuantity";
            this.cardStockQuantity.Padding = new System.Windows.Forms.Padding(12);
            this.cardStockQuantity.Size = new System.Drawing.Size(178, 74);
            this.cardStockQuantity.TabIndex = 3;
            this.cardStockQuantity.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // iconStockQuantity
            // 
            this.iconStockQuantity.Location = new System.Drawing.Point(12, 20);
            this.iconStockQuantity.Name = "iconStockQuantity";
            this.iconStockQuantity.Size = new System.Drawing.Size(38, 38);
            this.iconStockQuantity.TabIndex = 0;
            this.iconStockQuantity.Tag = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(141)))), ((int)(((byte)(35)))));
            this.iconStockQuantity.Paint += new System.Windows.Forms.PaintEventHandler(this.MetricIcon_Paint);
            // 
            // lblStockQuantityTitle
            // 
            this.lblStockQuantityTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 7.7F, System.Drawing.FontStyle.Bold);
            this.lblStockQuantityTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblStockQuantityTitle.Location = new System.Drawing.Point(58, 10);
            this.lblStockQuantityTitle.Name = "lblStockQuantityTitle";
            this.lblStockQuantityTitle.Size = new System.Drawing.Size(91, 18);
            this.lblStockQuantityTitle.TabIndex = 1;
            this.lblStockQuantityTitle.Text = "Stock Quantity";
            // 
            // lblStockQuantity
            // 
            this.lblStockQuantity.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold);
            this.lblStockQuantity.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblStockQuantity.Location = new System.Drawing.Point(57, 28);
            this.lblStockQuantity.Name = "lblStockQuantity";
            this.lblStockQuantity.Size = new System.Drawing.Size(120, 23);
            this.lblStockQuantity.TabIndex = 2;
            this.lblStockQuantity.Text = "0";
            // 
            // lblStockQuantityFooter
            // 
            this.lblStockQuantityFooter.Font = new System.Drawing.Font("Segoe UI", 7.2F);
            this.lblStockQuantityFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(93)))), ((int)(((byte)(132)))));
            this.lblStockQuantityFooter.Location = new System.Drawing.Point(58, 52);
            this.lblStockQuantityFooter.Name = "lblStockQuantityFooter";
            this.lblStockQuantityFooter.Size = new System.Drawing.Size(105, 18);
            this.lblStockQuantityFooter.TabIndex = 3;
            this.lblStockQuantityFooter.Text = "Total stock in units";
            // 
            // cardLowStock
            // 
            this.cardLowStock.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardLowStock.Controls.Add(this.iconLowStock);
            this.cardLowStock.Controls.Add(this.lblLowStockTitle);
            this.cardLowStock.Controls.Add(this.lblLowStock);
            this.cardLowStock.Controls.Add(this.lblLowStockFooter);
            this.cardLowStock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardLowStock.Location = new System.Drawing.Point(760, 4);
            this.cardLowStock.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardLowStock.Name = "cardLowStock";
            this.cardLowStock.Padding = new System.Windows.Forms.Padding(12);
            this.cardLowStock.Size = new System.Drawing.Size(178, 74);
            this.cardLowStock.TabIndex = 4;
            this.cardLowStock.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // iconLowStock
            // 
            this.iconLowStock.Location = new System.Drawing.Point(12, 20);
            this.iconLowStock.Name = "iconLowStock";
            this.iconLowStock.Size = new System.Drawing.Size(38, 38);
            this.iconLowStock.TabIndex = 0;
            this.iconLowStock.Tag = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(52)))), ((int)(((byte)(72)))));
            this.iconLowStock.Paint += new System.Windows.Forms.PaintEventHandler(this.MetricIcon_Paint);
            // 
            // lblLowStockTitle
            // 
            this.lblLowStockTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 7.7F, System.Drawing.FontStyle.Bold);
            this.lblLowStockTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblLowStockTitle.Location = new System.Drawing.Point(58, 10);
            this.lblLowStockTitle.Name = "lblLowStockTitle";
            this.lblLowStockTitle.Size = new System.Drawing.Size(105, 18);
            this.lblLowStockTitle.TabIndex = 1;
            this.lblLowStockTitle.Text = "Low Stock Items";
            // 
            // lblLowStock
            // 
            this.lblLowStock.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold);
            this.lblLowStock.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblLowStock.Location = new System.Drawing.Point(57, 28);
            this.lblLowStock.Name = "lblLowStock";
            this.lblLowStock.Size = new System.Drawing.Size(120, 23);
            this.lblLowStock.TabIndex = 2;
            this.lblLowStock.Text = "0";
            // 
            // lblLowStockFooter
            // 
            this.lblLowStockFooter.Font = new System.Drawing.Font("Segoe UI", 7.2F);
            this.lblLowStockFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(93)))), ((int)(((byte)(132)))));
            this.lblLowStockFooter.Location = new System.Drawing.Point(58, 52);
            this.lblLowStockFooter.Name = "lblLowStockFooter";
            this.lblLowStockFooter.Size = new System.Drawing.Size(106, 18);
            this.lblLowStockFooter.TabIndex = 3;
            this.lblLowStockFooter.Text = "Items below reorder";
            // 
            // cardOutStock
            // 
            this.cardOutStock.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardOutStock.Controls.Add(this.iconOutStock);
            this.cardOutStock.Controls.Add(this.lblOutStockTitle);
            this.cardOutStock.Controls.Add(this.lblOutStock);
            this.cardOutStock.Controls.Add(this.lblOutStockFooter);
            this.cardOutStock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardOutStock.Location = new System.Drawing.Point(950, 4);
            this.cardOutStock.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardOutStock.Name = "cardOutStock";
            this.cardOutStock.Padding = new System.Windows.Forms.Padding(12);
            this.cardOutStock.Size = new System.Drawing.Size(182, 74);
            this.cardOutStock.TabIndex = 5;
            this.cardOutStock.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // iconOutStock
            // 
            this.iconOutStock.Location = new System.Drawing.Point(12, 20);
            this.iconOutStock.Name = "iconOutStock";
            this.iconOutStock.Size = new System.Drawing.Size(38, 38);
            this.iconOutStock.TabIndex = 0;
            this.iconOutStock.Tag = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(75)))), ((int)(((byte)(140)))));
            this.iconOutStock.Paint += new System.Windows.Forms.PaintEventHandler(this.MetricIcon_Paint);
            // 
            // lblOutStockTitle
            // 
            this.lblOutStockTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 7.7F, System.Drawing.FontStyle.Bold);
            this.lblOutStockTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblOutStockTitle.Location = new System.Drawing.Point(47, 10);
            this.lblOutStockTitle.Name = "lblOutStockTitle";
            this.lblOutStockTitle.Size = new System.Drawing.Size(114, 18);
            this.lblOutStockTitle.TabIndex = 1;
            this.lblOutStockTitle.Text = "Out of Stock Items";
            // 
            // lblOutStock
            // 
            this.lblOutStock.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold);
            this.lblOutStock.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblOutStock.Location = new System.Drawing.Point(58, 28);
            this.lblOutStock.Name = "lblOutStock";
            this.lblOutStock.Size = new System.Drawing.Size(114, 23);
            this.lblOutStock.TabIndex = 2;
            this.lblOutStock.Text = "0";
            // 
            // lblOutStockFooter
            // 
            this.lblOutStockFooter.Font = new System.Drawing.Font("Segoe UI", 7.2F);
            this.lblOutStockFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(93)))), ((int)(((byte)(132)))));
            this.lblOutStockFooter.Location = new System.Drawing.Point(58, 52);
            this.lblOutStockFooter.Name = "lblOutStockFooter";
            this.lblOutStockFooter.Size = new System.Drawing.Size(114, 18);
            this.lblOutStockFooter.TabIndex = 3;
            this.lblOutStockFooter.Text = "Items out of stock";
            // 
            // topContentLayout
            // 
            this.topContentLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.topContentLayout.ColumnCount = 2;
            this.topContentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.topContentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.topContentLayout.Controls.Add(this.cardTrend, 0, 0);
            this.topContentLayout.Controls.Add(this.cardTopItems, 1, 0);
            this.topContentLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topContentLayout.Location = new System.Drawing.Point(18, 138);
            this.topContentLayout.Margin = new System.Windows.Forms.Padding(0);
            this.topContentLayout.Name = "topContentLayout";
            this.topContentLayout.RowCount = 1;
            this.topContentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.topContentLayout.Size = new System.Drawing.Size(1144, 196);
            this.topContentLayout.TabIndex = 2;
            // 
            // cardTrend
            // 
            this.cardTrend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardTrend.Controls.Add(this.cmbAnalysisMode);
            this.cardTrend.Controls.Add(this.trendCanvas);
            this.cardTrend.Controls.Add(this.lblTrendTitle);
            this.cardTrend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardTrend.Location = new System.Drawing.Point(0, 0);
            this.cardTrend.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            this.cardTrend.Name = "cardTrend";
            this.cardTrend.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.cardTrend.Size = new System.Drawing.Size(617, 186);
            this.cardTrend.TabIndex = 0;
            this.cardTrend.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // cmbAnalysisMode
            // 
            this.cmbAnalysisMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbAnalysisMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAnalysisMode.Font = new System.Drawing.Font("Segoe UI", 8.75F);
            this.cmbAnalysisMode.FormattingEnabled = true;
            this.cmbAnalysisMode.Items.AddRange(new object[] {
            "Daily",
            "Weekly",
            "Monthly"});
            this.cmbAnalysisMode.Location = new System.Drawing.Point(509, 4);
            this.cmbAnalysisMode.Name = "cmbAnalysisMode";
            this.cmbAnalysisMode.Size = new System.Drawing.Size(82, 23);
            this.cmbAnalysisMode.TabIndex = 4;
            // 
            // trendCanvas
            // 
            this.trendCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.trendCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trendCanvas.Location = new System.Drawing.Point(10, 29);
            this.trendCanvas.Name = "trendCanvas";
            this.trendCanvas.Size = new System.Drawing.Size(597, 149);
            this.trendCanvas.TabIndex = 0;
            // 
            // lblTrendTitle
            // 
            this.lblTrendTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTrendTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTrendTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTrendTitle.Location = new System.Drawing.Point(10, 8);
            this.lblTrendTitle.Name = "lblTrendTitle";
            this.lblTrendTitle.Padding = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.lblTrendTitle.Size = new System.Drawing.Size(597, 21);
            this.lblTrendTitle.TabIndex = 1;
            this.lblTrendTitle.Text = "Stock Trend (Value)";
            // 
            // cardTopItems
            // 
            this.cardTopItems.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardTopItems.Controls.Add(this.gridTopItems);
            this.cardTopItems.Controls.Add(this.lblTopItemsTitle);
            this.cardTopItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardTopItems.Location = new System.Drawing.Point(629, 0);
            this.cardTopItems.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            this.cardTopItems.Name = "cardTopItems";
            this.cardTopItems.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.cardTopItems.Size = new System.Drawing.Size(503, 186);
            this.cardTopItems.TabIndex = 1;
            this.cardTopItems.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // gridTopItems
            // 
            this.gridTopItems.AllowUserToAddRows = false;
            this.gridTopItems.AllowUserToDeleteRows = false;
            this.gridTopItems.AllowUserToResizeRows = false;
            this.gridTopItems.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridTopItems.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.gridTopItems.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(252)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 7.6F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.gridTopItems.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gridTopItems.ColumnHeadersHeight = 25;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 7.2F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(64)))), ((int)(((byte)(105)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(232)))), ((int)(((byte)(250)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridTopItems.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridTopItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTopItems.EnableHeadersVisualStyles = false;
            this.gridTopItems.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(233)))), ((int)(((byte)(246)))));
            this.gridTopItems.Location = new System.Drawing.Point(10, 29);
            this.gridTopItems.Name = "gridTopItems";
            this.gridTopItems.ReadOnly = true;
            this.gridTopItems.RowHeadersVisible = false;
            this.gridTopItems.RowTemplate.Height = 23;
            this.gridTopItems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridTopItems.Size = new System.Drawing.Size(483, 149);
            this.gridTopItems.TabIndex = 0;
            // 
            // lblTopItemsTitle
            // 
            this.lblTopItemsTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTopItemsTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTopItemsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTopItemsTitle.Location = new System.Drawing.Point(10, 8);
            this.lblTopItemsTitle.Name = "lblTopItemsTitle";
            this.lblTopItemsTitle.Padding = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.lblTopItemsTitle.Size = new System.Drawing.Size(483, 21);
            this.lblTopItemsTitle.TabIndex = 1;
            this.lblTopItemsTitle.Text = "Top Stock Items (By Quantity)";
            // 
            // middleContentLayout
            // 
            this.middleContentLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.middleContentLayout.ColumnCount = 3;
            this.middleContentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.middleContentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24F));
            this.middleContentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26F));
            this.middleContentLayout.Controls.Add(this.cardItemGraph, 0, 0);
            this.middleContentLayout.Controls.Add(this.cardCategory, 1, 0);
            this.middleContentLayout.Controls.Add(this.cardSummary, 2, 0);
            this.middleContentLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.middleContentLayout.Location = new System.Drawing.Point(18, 334);
            this.middleContentLayout.Margin = new System.Windows.Forms.Padding(0);
            this.middleContentLayout.Name = "middleContentLayout";
            this.middleContentLayout.RowCount = 1;
            this.middleContentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.middleContentLayout.Size = new System.Drawing.Size(1144, 156);
            this.middleContentLayout.TabIndex = 3;
            // 
            // cardItemGraph
            // 
            this.cardItemGraph.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardItemGraph.Controls.Add(this.itemGraphCanvas);
            this.cardItemGraph.Controls.Add(this.lblItemGraphTitle);
            this.cardItemGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardItemGraph.Location = new System.Drawing.Point(0, 0);
            this.cardItemGraph.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            this.cardItemGraph.Name = "cardItemGraph";
            this.cardItemGraph.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.cardItemGraph.Size = new System.Drawing.Size(560, 146);
            this.cardItemGraph.TabIndex = 0;
            this.cardItemGraph.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // itemGraphCanvas
            // 
            this.itemGraphCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.itemGraphCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.itemGraphCanvas.Location = new System.Drawing.Point(10, 29);
            this.itemGraphCanvas.Name = "itemGraphCanvas";
            this.itemGraphCanvas.Size = new System.Drawing.Size(540, 109);
            this.itemGraphCanvas.TabIndex = 0;
            // 
            // lblItemGraphTitle
            // 
            this.lblItemGraphTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblItemGraphTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblItemGraphTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblItemGraphTitle.Location = new System.Drawing.Point(10, 8);
            this.lblItemGraphTitle.Name = "lblItemGraphTitle";
            this.lblItemGraphTitle.Padding = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.lblItemGraphTitle.Size = new System.Drawing.Size(540, 21);
            this.lblItemGraphTitle.TabIndex = 1;
            this.lblItemGraphTitle.Text = "Item Stock (Top 10 by Quantity)";
            // 
            // cardCategory
            // 
            this.cardCategory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardCategory.Controls.Add(this.categoryLegendPanel);
            this.cardCategory.Controls.Add(this.categoryCanvas);
            this.cardCategory.Controls.Add(this.lblCategoryTitle);
            this.cardCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardCategory.Location = new System.Drawing.Point(572, 0);
            this.cardCategory.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            this.cardCategory.Name = "cardCategory";
            this.cardCategory.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.cardCategory.Size = new System.Drawing.Size(262, 146);
            this.cardCategory.TabIndex = 1;
            this.cardCategory.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // categoryLegendPanel
            // 
            this.categoryLegendPanel.AutoScroll = true;
            this.categoryLegendPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.categoryLegendPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.categoryLegendPanel.Location = new System.Drawing.Point(86, 29);
            this.categoryLegendPanel.Name = "categoryLegendPanel";
            this.categoryLegendPanel.Size = new System.Drawing.Size(166, 109);
            this.categoryLegendPanel.TabIndex = 2;
            // 
            // categoryCanvas
            // 
            this.categoryCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.categoryCanvas.Dock = System.Windows.Forms.DockStyle.Left;
            this.categoryCanvas.Location = new System.Drawing.Point(10, 29);
            this.categoryCanvas.Name = "categoryCanvas";
            this.categoryCanvas.Size = new System.Drawing.Size(76, 109);
            this.categoryCanvas.TabIndex = 0;
            // 
            // lblCategoryTitle
            // 
            this.lblCategoryTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCategoryTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCategoryTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblCategoryTitle.Location = new System.Drawing.Point(10, 8);
            this.lblCategoryTitle.Name = "lblCategoryTitle";
            this.lblCategoryTitle.Padding = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.lblCategoryTitle.Size = new System.Drawing.Size(242, 21);
            this.lblCategoryTitle.TabIndex = 1;
            this.lblCategoryTitle.Text = "Stock Category Distribution (Value)";
            // 
            // cardSummary
            // 
            this.cardSummary.AutoScroll = true;
            this.cardSummary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardSummary.Controls.Add(this.lblSummary);
            this.cardSummary.Controls.Add(this.lblSummaryTitle);
            this.cardSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardSummary.Location = new System.Drawing.Point(846, 0);
            this.cardSummary.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            this.cardSummary.Name = "cardSummary";
            this.cardSummary.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.cardSummary.Size = new System.Drawing.Size(286, 146);
            this.cardSummary.TabIndex = 2;
            this.cardSummary.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // lblSummary
            // 
            this.lblSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSummary.Font = new System.Drawing.Font("Segoe UI", 8.4F);
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblSummary.Location = new System.Drawing.Point(10, 29);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
            this.lblSummary.Size = new System.Drawing.Size(266, 109);
            this.lblSummary.TabIndex = 0;
            this.lblSummary.Text = "Total Stock Value        Rs. 0.00\r\nTotal Stock Quantity     0.00 Units\r\nAverage Item Value      Rs. 0.00\r\nStock Turnover Rate     0x\r\nStock Accuracy          0%";
            // 
            // lblSummaryTitle
            // 
            this.lblSummaryTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSummaryTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSummaryTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblSummaryTitle.Location = new System.Drawing.Point(10, 8);
            this.lblSummaryTitle.Name = "lblSummaryTitle";
            this.lblSummaryTitle.Padding = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.lblSummaryTitle.Size = new System.Drawing.Size(266, 21);
            this.lblSummaryTitle.TabIndex = 1;
            this.lblSummaryTitle.Text = "Stock Summary";
            // 
            // bottomContentLayout
            // 
            this.bottomContentLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.bottomContentLayout.ColumnCount = 3;
            this.bottomContentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.bottomContentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
            this.bottomContentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.bottomContentLayout.Controls.Add(this.cardLowStockList, 0, 0);
            this.bottomContentLayout.Controls.Add(this.cardMovement, 1, 0);
            this.bottomContentLayout.Controls.Add(this.cardOutStockList, 2, 0);
            this.bottomContentLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomContentLayout.Location = new System.Drawing.Point(18, 490);
            this.bottomContentLayout.Margin = new System.Windows.Forms.Padding(0);
            this.bottomContentLayout.MinimumSize = new System.Drawing.Size(0, 152);
            this.bottomContentLayout.Name = "bottomContentLayout";
            this.bottomContentLayout.RowCount = 1;
            this.bottomContentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bottomContentLayout.Size = new System.Drawing.Size(1144, 152);
            this.bottomContentLayout.TabIndex = 4;
            // 
            // cardLowStockList
            // 
            this.cardLowStockList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardLowStockList.Controls.Add(this.gridLowStock);
            this.cardLowStockList.Controls.Add(this.lblLowStockListTitle);
            this.cardLowStockList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardLowStockList.Location = new System.Drawing.Point(0, 0);
            this.cardLowStockList.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            this.cardLowStockList.Name = "cardLowStockList";
            this.cardLowStockList.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.cardLowStockList.Size = new System.Drawing.Size(399, 142);
            this.cardLowStockList.TabIndex = 0;
            this.cardLowStockList.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // gridLowStock
            // 
            this.gridLowStock.AllowUserToAddRows = false;
            this.gridLowStock.AllowUserToDeleteRows = false;
            this.gridLowStock.AllowUserToResizeRows = false;
            this.gridLowStock.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridLowStock.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.gridLowStock.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(252)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI Semibold", 7.6F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.gridLowStock.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.gridLowStock.ColumnHeadersHeight = 25;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 7.2F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(64)))), ((int)(((byte)(105)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(232)))), ((int)(((byte)(250)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridLowStock.DefaultCellStyle = dataGridViewCellStyle4;
            this.gridLowStock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridLowStock.EnableHeadersVisualStyles = false;
            this.gridLowStock.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(233)))), ((int)(((byte)(246)))));
            this.gridLowStock.Location = new System.Drawing.Point(10, 29);
            this.gridLowStock.Name = "gridLowStock";
            this.gridLowStock.ReadOnly = true;
            this.gridLowStock.RowHeadersVisible = false;
            this.gridLowStock.RowTemplate.Height = 23;
            this.gridLowStock.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridLowStock.Size = new System.Drawing.Size(379, 105);
            this.gridLowStock.TabIndex = 0;
            // 
            // lblLowStockListTitle
            // 
            this.lblLowStockListTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLowStockListTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblLowStockListTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblLowStockListTitle.Location = new System.Drawing.Point(10, 8);
            this.lblLowStockListTitle.Name = "lblLowStockListTitle";
            this.lblLowStockListTitle.Padding = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.lblLowStockListTitle.Size = new System.Drawing.Size(379, 21);
            this.lblLowStockListTitle.TabIndex = 1;
            this.lblLowStockListTitle.Text = "Low Stock Alert List";
            // 
            // cardMovement
            // 
            this.cardMovement.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardMovement.Controls.Add(this.movementLayout);
            this.cardMovement.Controls.Add(this.lblMovementTitle);
            this.cardMovement.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardMovement.Location = new System.Drawing.Point(411, 0);
            this.cardMovement.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            this.cardMovement.MinimumSize = new System.Drawing.Size(0, 142);
            this.cardMovement.Name = "cardMovement";
            this.cardMovement.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.cardMovement.Size = new System.Drawing.Size(308, 142);
            this.cardMovement.TabIndex = 1;
            this.cardMovement.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // movementLayout
            // 
            this.movementLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.movementLayout.ColumnCount = 3;
            this.movementLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.movementLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.movementLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.movementLayout.Controls.Add(this.lblFastMoving, 0, 0);
            this.movementLayout.Controls.Add(this.lblSlowMoving, 1, 0);
            this.movementLayout.Controls.Add(this.lblDeadStock, 2, 0);
            this.movementLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.movementLayout.Location = new System.Drawing.Point(10, 29);
            this.movementLayout.MinimumSize = new System.Drawing.Size(0, 105);
            this.movementLayout.Name = "movementLayout";
            this.movementLayout.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.movementLayout.RowCount = 1;
            this.movementLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.movementLayout.Size = new System.Drawing.Size(288, 105);
            this.movementLayout.TabIndex = 0;
            // 
            // lblFastMoving
            // 
            this.lblFastMoving.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(251)))), ((int)(((byte)(244)))));
            this.lblFastMoving.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFastMoving.Font = new System.Drawing.Font("Segoe UI Semibold", 8.3F, System.Drawing.FontStyle.Bold);
            this.lblFastMoving.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(178)))), ((int)(((byte)(72)))));
            this.lblFastMoving.Location = new System.Drawing.Point(4, 10);
            this.lblFastMoving.Margin = new System.Windows.Forms.Padding(4);
            this.lblFastMoving.MinimumSize = new System.Drawing.Size(0, 91);
            this.lblFastMoving.Name = "lblFastMoving";
            this.lblFastMoving.Size = new System.Drawing.Size(88, 91);
            this.lblFastMoving.TabIndex = 0;
            this.lblFastMoving.Text = "Fast Moving\r\n52 Items\r\nGood turnover";
            this.lblFastMoving.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSlowMoving
            // 
            this.lblSlowMoving.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(248)))), ((int)(((byte)(237)))));
            this.lblSlowMoving.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSlowMoving.Font = new System.Drawing.Font("Segoe UI Semibold", 8.3F, System.Drawing.FontStyle.Bold);
            this.lblSlowMoving.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(141)))), ((int)(((byte)(35)))));
            this.lblSlowMoving.Location = new System.Drawing.Point(100, 10);
            this.lblSlowMoving.Margin = new System.Windows.Forms.Padding(4);
            this.lblSlowMoving.MinimumSize = new System.Drawing.Size(0, 91);
            this.lblSlowMoving.Name = "lblSlowMoving";
            this.lblSlowMoving.Size = new System.Drawing.Size(88, 91);
            this.lblSlowMoving.TabIndex = 1;
            this.lblSlowMoving.Text = "Slow Moving\r\n18 Items\r\nLow turnover";
            this.lblSlowMoving.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDeadStock
            // 
            this.lblDeadStock.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.lblDeadStock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDeadStock.Font = new System.Drawing.Font("Segoe UI Semibold", 8.3F, System.Drawing.FontStyle.Bold);
            this.lblDeadStock.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(52)))), ((int)(((byte)(72)))));
            this.lblDeadStock.Location = new System.Drawing.Point(196, 10);
            this.lblDeadStock.Margin = new System.Windows.Forms.Padding(4);
            this.lblDeadStock.MinimumSize = new System.Drawing.Size(0, 91);
            this.lblDeadStock.Name = "lblDeadStock";
            this.lblDeadStock.Size = new System.Drawing.Size(88, 91);
            this.lblDeadStock.TabIndex = 2;
            this.lblDeadStock.Text = "Dead Stock\r\n6 Items\r\nNo movement";
            this.lblDeadStock.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMovementTitle
            // 
            this.lblMovementTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMovementTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblMovementTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblMovementTitle.Location = new System.Drawing.Point(10, 8);
            this.lblMovementTitle.Name = "lblMovementTitle";
            this.lblMovementTitle.Padding = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.lblMovementTitle.Size = new System.Drawing.Size(288, 21);
            this.lblMovementTitle.TabIndex = 1;
            this.lblMovementTitle.Text = "Fast / Slow / Dead Stock";
            // 
            // cardOutStockList
            // 
            this.cardOutStockList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardOutStockList.Controls.Add(this.gridOutStock);
            this.cardOutStockList.Controls.Add(this.lblOutStockListTitle);
            this.cardOutStockList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardOutStockList.Location = new System.Drawing.Point(731, 0);
            this.cardOutStockList.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            this.cardOutStockList.Name = "cardOutStockList";
            this.cardOutStockList.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.cardOutStockList.Size = new System.Drawing.Size(401, 142);
            this.cardOutStockList.TabIndex = 2;
            this.cardOutStockList.Paint += new System.Windows.Forms.PaintEventHandler(this.Card_Paint);
            // 
            // gridOutStock
            // 
            this.gridOutStock.AllowUserToAddRows = false;
            this.gridOutStock.AllowUserToDeleteRows = false;
            this.gridOutStock.AllowUserToResizeRows = false;
            this.gridOutStock.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridOutStock.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.gridOutStock.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(252)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI Semibold", 7.6F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.gridOutStock.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.gridOutStock.ColumnHeadersHeight = 25;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI", 7.2F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(64)))), ((int)(((byte)(105)))));
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(232)))), ((int)(((byte)(250)))));
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridOutStock.DefaultCellStyle = dataGridViewCellStyle6;
            this.gridOutStock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridOutStock.EnableHeadersVisualStyles = false;
            this.gridOutStock.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(233)))), ((int)(((byte)(246)))));
            this.gridOutStock.Location = new System.Drawing.Point(10, 29);
            this.gridOutStock.Name = "gridOutStock";
            this.gridOutStock.ReadOnly = true;
            this.gridOutStock.RowHeadersVisible = false;
            this.gridOutStock.RowTemplate.Height = 23;
            this.gridOutStock.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridOutStock.Size = new System.Drawing.Size(381, 105);
            this.gridOutStock.TabIndex = 0;
            // 
            // lblOutStockListTitle
            // 
            this.lblOutStockListTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblOutStockListTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblOutStockListTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblOutStockListTitle.Location = new System.Drawing.Point(10, 8);
            this.lblOutStockListTitle.Name = "lblOutStockListTitle";
            this.lblOutStockListTitle.Padding = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.lblOutStockListTitle.Size = new System.Drawing.Size(381, 21);
            this.lblOutStockListTitle.TabIndex = 1;
            this.lblOutStockListTitle.Text = "Out of Stock Items List";
            // 
            // FrmStockAnalytics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.ClientSize = new System.Drawing.Size(1180, 650);
            this.Controls.Add(this.mainLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmStockAnalytics";
            this.Text = "Stock Analytics";
            this.mainLayout.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbQuickDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            this.metricsLayout.ResumeLayout(false);
            this.cardCurrentStockValue.ResumeLayout(false);
            this.cardTotalItems.ResumeLayout(false);
            this.cardStockValue.ResumeLayout(false);
            this.cardStockQuantity.ResumeLayout(false);
            this.cardLowStock.ResumeLayout(false);
            this.cardOutStock.ResumeLayout(false);
            this.topContentLayout.ResumeLayout(false);
            this.cardTrend.ResumeLayout(false);
            this.cardTopItems.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridTopItems)).EndInit();
            this.middleContentLayout.ResumeLayout(false);
            this.cardItemGraph.ResumeLayout(false);
            this.cardCategory.ResumeLayout(false);
            this.cardSummary.ResumeLayout(false);
            this.bottomContentLayout.ResumeLayout(false);
            this.cardLowStockList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridLowStock)).EndInit();
            this.cardMovement.ResumeLayout(false);
            this.movementLayout.ResumeLayout(false);
            this.cardOutStockList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridOutStock)).EndInit();
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.Panel cardItemGraph;
        private System.Windows.Forms.Panel cardSummary;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Label lblSummaryTitle;
        private System.Windows.Forms.Label lblTrendTitle;
        private System.Windows.Forms.Label lblTopItemsTitle;
        private System.Windows.Forms.Label lblItemGraphTitle;
        private System.Windows.Forms.Panel cardCategory;
        private System.Windows.Forms.Label lblCategoryTitle;
    }
}




