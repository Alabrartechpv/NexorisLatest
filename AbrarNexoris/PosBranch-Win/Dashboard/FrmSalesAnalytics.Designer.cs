namespace PosBranch_Win.Dashboard
{
    partial class FrmSalesAnalytics
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubTitle;
        private System.Windows.Forms.DateTimePicker dtFrom;
        private System.Windows.Forms.DateTimePicker dtTo;
        private System.Windows.Forms.ComboBox cmbQuickDate;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.TableLayoutPanel metricsLayout;
        private System.Windows.Forms.Panel cardTotalSales;
        private System.Windows.Forms.Panel cardOrders;
        private System.Windows.Forms.Panel cardAverageOrder;
        private System.Windows.Forms.Panel cardProfit;
        private System.Windows.Forms.Panel cardItemsSold;
        private System.Windows.Forms.PictureBox iconSales;
        private System.Windows.Forms.PictureBox iconOrders;
        private System.Windows.Forms.PictureBox iconAverage;
        private System.Windows.Forms.PictureBox iconProfit;
        private System.Windows.Forms.PictureBox iconItems;
        private System.Windows.Forms.Label lblTotalSales;
        private System.Windows.Forms.Label lblOrders;
        private System.Windows.Forms.Label lblAverageOrder;
        private System.Windows.Forms.Label lblProfit;
        private System.Windows.Forms.Label lblItemsSold;
        private System.Windows.Forms.Label lblTotalSalesTitle;
        private System.Windows.Forms.Label lblOrdersTitle;
        private System.Windows.Forms.Label lblAverageOrderTitle;
        private System.Windows.Forms.Label lblProfitTitle;
        private System.Windows.Forms.Label lblItemsSoldTitle;
        private System.Windows.Forms.Label lblSalesChange;
        private System.Windows.Forms.Label lblOrdersChange;
        private System.Windows.Forms.Label lblAverageChange;
        private System.Windows.Forms.Label lblProfitChange;
        private System.Windows.Forms.Label lblItemsChange;
        private System.Windows.Forms.TableLayoutPanel contentLayout;
        private System.Windows.Forms.Panel trendPanel;
        private System.Windows.Forms.Panel topQtyPanel;
        private System.Windows.Forms.Panel topAmountPanel;
        private System.Windows.Forms.Panel itemMapPanel;
        private System.Windows.Forms.Panel paymentPanel;
        private System.Windows.Forms.Panel categoryPanel;
        private System.Windows.Forms.Label lblTrendTitle;
        private System.Windows.Forms.Label lblTopQtyTitle;
        private System.Windows.Forms.Label lblTopAmountTitle;
        private System.Windows.Forms.Label lblItemMapTitle;
        private System.Windows.Forms.Label lblPaymentTitle;
        private System.Windows.Forms.Label lblCategoryTitle;
        private System.Windows.Forms.Panel trendCanvas;
        private System.Windows.Forms.Panel itemMapCanvas;
        private System.Windows.Forms.Panel paymentCanvas;
        private System.Windows.Forms.Panel categoryCanvas;
        private System.Windows.Forms.ComboBox cmbItemMapSort;
        private System.Windows.Forms.DataGridView gridTopQty;
        private System.Windows.Forms.DataGridView gridTopAmount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQtyRank;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQtyItemName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQtySold;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAmountRank;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAmountItemName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAmountValue;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubTitle = new System.Windows.Forms.Label();
            this.cmbQuickDate = new System.Windows.Forms.ComboBox();
            this.dtFrom = new System.Windows.Forms.DateTimePicker();
            this.dtTo = new System.Windows.Forms.DateTimePicker();
            this.btnApply = new System.Windows.Forms.Button();
            this.metricsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.cardTotalSales = new System.Windows.Forms.Panel();
            this.iconSales = new System.Windows.Forms.PictureBox();
            this.lblTotalSalesTitle = new System.Windows.Forms.Label();
            this.lblTotalSales = new System.Windows.Forms.Label();
            this.lblSalesChange = new System.Windows.Forms.Label();
            this.cardOrders = new System.Windows.Forms.Panel();
            this.iconOrders = new System.Windows.Forms.PictureBox();
            this.lblOrdersTitle = new System.Windows.Forms.Label();
            this.lblOrders = new System.Windows.Forms.Label();
            this.lblOrdersChange = new System.Windows.Forms.Label();
            this.cardAverageOrder = new System.Windows.Forms.Panel();
            this.iconAverage = new System.Windows.Forms.PictureBox();
            this.lblAverageOrderTitle = new System.Windows.Forms.Label();
            this.lblAverageOrder = new System.Windows.Forms.Label();
            this.lblAverageChange = new System.Windows.Forms.Label();
            this.cardProfit = new System.Windows.Forms.Panel();
            this.iconProfit = new System.Windows.Forms.PictureBox();
            this.lblProfitTitle = new System.Windows.Forms.Label();
            this.lblProfit = new System.Windows.Forms.Label();
            this.lblProfitChange = new System.Windows.Forms.Label();
            this.cardItemsSold = new System.Windows.Forms.Panel();
            this.iconItems = new System.Windows.Forms.PictureBox();
            this.lblItemsSoldTitle = new System.Windows.Forms.Label();
            this.lblItemsSold = new System.Windows.Forms.Label();
            this.lblItemsChange = new System.Windows.Forms.Label();
            this.contentLayout = new System.Windows.Forms.TableLayoutPanel();
            this.trendPanel = new System.Windows.Forms.Panel();
            this.lblTrendTitle = new System.Windows.Forms.Label();
            this.trendCanvas = new System.Windows.Forms.Panel();
            this.topQtyPanel = new System.Windows.Forms.Panel();
            this.lblTopQtyTitle = new System.Windows.Forms.Label();
            this.gridTopQty = new System.Windows.Forms.DataGridView();
            this.colQtyRank = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colQtyItemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colQtySold = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.topAmountPanel = new System.Windows.Forms.Panel();
            this.lblTopAmountTitle = new System.Windows.Forms.Label();
            this.gridTopAmount = new System.Windows.Forms.DataGridView();
            this.colAmountRank = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAmountItemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAmountValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.itemMapPanel = new System.Windows.Forms.Panel();
            this.itemMapCanvas = new System.Windows.Forms.Panel();
            this.lblItemMapTitle = new System.Windows.Forms.Label();
            this.cmbItemMapSort = new System.Windows.Forms.ComboBox();
            this.paymentPanel = new System.Windows.Forms.Panel();
            this.lblPaymentTitle = new System.Windows.Forms.Label();
            this.paymentCanvas = new System.Windows.Forms.Panel();
            this.categoryPanel = new System.Windows.Forms.Panel();
            this.lblCategoryTitle = new System.Windows.Forms.Label();
            this.categoryCanvas = new System.Windows.Forms.Panel();
            this.mainLayout.SuspendLayout();
            this.headerPanel.SuspendLayout();
            this.metricsLayout.SuspendLayout();
            this.cardTotalSales.SuspendLayout();
            this.cardOrders.SuspendLayout();
            this.cardAverageOrder.SuspendLayout();
            this.cardProfit.SuspendLayout();
            this.cardItemsSold.SuspendLayout();
            this.contentLayout.SuspendLayout();
            this.trendPanel.SuspendLayout();
            this.topQtyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTopQty)).BeginInit();
            this.topAmountPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTopAmount)).BeginInit();
            this.itemMapPanel.SuspendLayout();
            this.paymentPanel.SuspendLayout();
            this.categoryPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayout
            // 
            this.mainLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.mainLayout.ColumnCount = 1;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.Controls.Add(this.headerPanel, 0, 0);
            this.mainLayout.Controls.Add(this.metricsLayout, 0, 1);
            this.mainLayout.Controls.Add(this.contentLayout, 0, 2);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.Padding = new System.Windows.Forms.Padding(18, 14, 18, 14);
            this.mainLayout.RowCount = 3;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 126F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.Size = new System.Drawing.Size(1180, 650);
            this.mainLayout.TabIndex = 0;
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.headerPanel.Controls.Add(this.lblTitle);
            this.headerPanel.Controls.Add(this.lblSubTitle);
            this.headerPanel.Controls.Add(this.cmbQuickDate);
            this.headerPanel.Controls.Add(this.dtFrom);
            this.headerPanel.Controls.Add(this.dtTo);
            this.headerPanel.Controls.Add(this.btnApply);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerPanel.Location = new System.Drawing.Point(21, 17);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(1138, 58);
            this.headerPanel.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTitle.Location = new System.Drawing.Point(4, 3);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(235, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Sales Analytics Overview";
            // 
            // lblSubTitle
            // 
            this.lblSubTitle.AutoSize = true;
            this.lblSubTitle.Font = new System.Drawing.Font("Segoe UI", 8.75F);
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(93)))), ((int)(((byte)(132)))));
            this.lblSubTitle.Location = new System.Drawing.Point(6, 35);
            this.lblSubTitle.Name = "lblSubTitle";
            this.lblSubTitle.Size = new System.Drawing.Size(460, 15);
            this.lblSubTitle.TabIndex = 1;
            this.lblSubTitle.Text = "Summary of sales performance, item movement, customer activity, and payment mix.";
            // 
            // cmbQuickDate
            // 
            this.cmbQuickDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbQuickDate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbQuickDate.Font = new System.Drawing.Font("Segoe UI", 8.75F);
            this.cmbQuickDate.FormattingEnabled = true;
            this.cmbQuickDate.Location = new System.Drawing.Point(637, 16);
            this.cmbQuickDate.Name = "cmbQuickDate";
            this.cmbQuickDate.Size = new System.Drawing.Size(104, 23);
            this.cmbQuickDate.TabIndex = 2;
            // 
            // dtFrom
            // 
            this.dtFrom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dtFrom.CustomFormat = "dd MMM yyyy";
            this.dtFrom.Font = new System.Drawing.Font("Segoe UI", 8.75F);
            this.dtFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtFrom.Location = new System.Drawing.Point(753, 16);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(128, 23);
            this.dtFrom.TabIndex = 3;
            // 
            // dtTo
            // 
            this.dtTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dtTo.CustomFormat = "dd MMM yyyy";
            this.dtTo.Font = new System.Drawing.Font("Segoe UI", 8.75F);
            this.dtTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtTo.Location = new System.Drawing.Point(892, 16);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(128, 23);
            this.dtTo.TabIndex = 4;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(126)))), ((int)(((byte)(235)))));
            this.btnApply.FlatAppearance.BorderSize = 0;
            this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApply.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.btnApply.ForeColor = System.Drawing.Color.White;
            this.btnApply.Location = new System.Drawing.Point(1034, 13);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(88, 28);
            this.btnApply.TabIndex = 5;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = false;
            // 
            // metricsLayout
            // 
            this.metricsLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.metricsLayout.ColumnCount = 5;
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.metricsLayout.Controls.Add(this.cardTotalSales, 0, 0);
            this.metricsLayout.Controls.Add(this.cardOrders, 1, 0);
            this.metricsLayout.Controls.Add(this.cardAverageOrder, 2, 0);
            this.metricsLayout.Controls.Add(this.cardProfit, 3, 0);
            this.metricsLayout.Controls.Add(this.cardItemsSold, 4, 0);
            this.metricsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metricsLayout.Location = new System.Drawing.Point(18, 78);
            this.metricsLayout.Margin = new System.Windows.Forms.Padding(0);
            this.metricsLayout.Name = "metricsLayout";
            this.metricsLayout.Padding = new System.Windows.Forms.Padding(0, 6, 0, 10);
            this.metricsLayout.RowCount = 1;
            this.metricsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.metricsLayout.Size = new System.Drawing.Size(1144, 126);
            this.metricsLayout.TabIndex = 1;
            // 
            // cardTotalSales
            // 
            this.cardTotalSales.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardTotalSales.Controls.Add(this.iconSales);
            this.cardTotalSales.Controls.Add(this.lblTotalSalesTitle);
            this.cardTotalSales.Controls.Add(this.lblTotalSales);
            this.cardTotalSales.Controls.Add(this.lblSalesChange);
            this.cardTotalSales.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardTotalSales.Location = new System.Drawing.Point(0, 6);
            this.cardTotalSales.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardTotalSales.Name = "cardTotalSales";
            this.cardTotalSales.Size = new System.Drawing.Size(216, 110);
            this.cardTotalSales.TabIndex = 0;
            // 
            // iconSales
            // 
            this.iconSales.BackColor = System.Drawing.Color.Transparent;
            this.iconSales.Location = new System.Drawing.Point(16, 26);
            this.iconSales.Name = "iconSales";
            this.iconSales.Size = new System.Drawing.Size(46, 46);
            this.iconSales.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.iconSales.TabIndex = 0;
            this.iconSales.TabStop = false;
            // 
            // lblTotalSalesTitle
            // 
            this.lblTotalSalesTitle.AutoSize = true;
            this.lblTotalSalesTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTotalSalesTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTotalSalesTitle.Location = new System.Drawing.Point(76, 18);
            this.lblTotalSalesTitle.Name = "lblTotalSalesTitle";
            this.lblTotalSalesTitle.Size = new System.Drawing.Size(63, 15);
            this.lblTotalSalesTitle.TabIndex = 1;
            this.lblTotalSalesTitle.Text = "Total Sales";
            // 
            // lblTotalSales
            // 
            this.lblTotalSales.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblTotalSales.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTotalSales.Location = new System.Drawing.Point(76, 39);
            this.lblTotalSales.Name = "lblTotalSales";
            this.lblTotalSales.Size = new System.Drawing.Size(137, 26);
            this.lblTotalSales.TabIndex = 2;
            this.lblTotalSales.Text = "Rs 0.00";
            // 
            // lblSalesChange
            // 
            this.lblSalesChange.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblSalesChange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(178)))), ((int)(((byte)(72)))));
            this.lblSalesChange.Location = new System.Drawing.Point(76, 68);
            this.lblSalesChange.Name = "lblSalesChange";
            this.lblSalesChange.Size = new System.Drawing.Size(137, 20);
            this.lblSalesChange.TabIndex = 3;
            this.lblSalesChange.Text = "0% vs previous period";
            // 
            // cardOrders
            // 
            this.cardOrders.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardOrders.Controls.Add(this.iconOrders);
            this.cardOrders.Controls.Add(this.lblOrdersTitle);
            this.cardOrders.Controls.Add(this.lblOrders);
            this.cardOrders.Controls.Add(this.lblOrdersChange);
            this.cardOrders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardOrders.Location = new System.Drawing.Point(228, 6);
            this.cardOrders.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardOrders.Name = "cardOrders";
            this.cardOrders.Size = new System.Drawing.Size(216, 110);
            this.cardOrders.TabIndex = 1;
            // 
            // iconOrders
            // 
            this.iconOrders.BackColor = System.Drawing.Color.Transparent;
            this.iconOrders.Location = new System.Drawing.Point(16, 26);
            this.iconOrders.Name = "iconOrders";
            this.iconOrders.Size = new System.Drawing.Size(46, 46);
            this.iconOrders.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.iconOrders.TabIndex = 0;
            this.iconOrders.TabStop = false;
            // 
            // lblOrdersTitle
            // 
            this.lblOrdersTitle.AutoSize = true;
            this.lblOrdersTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblOrdersTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblOrdersTitle.Location = new System.Drawing.Point(76, 18);
            this.lblOrdersTitle.Name = "lblOrdersTitle";
            this.lblOrdersTitle.Size = new System.Drawing.Size(71, 15);
            this.lblOrdersTitle.TabIndex = 1;
            this.lblOrdersTitle.Text = "Total Orders";
            // 
            // lblOrders
            // 
            this.lblOrders.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblOrders.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblOrders.Location = new System.Drawing.Point(76, 39);
            this.lblOrders.Name = "lblOrders";
            this.lblOrders.Size = new System.Drawing.Size(137, 26);
            this.lblOrders.TabIndex = 2;
            this.lblOrders.Text = "0";
            // 
            // lblOrdersChange
            // 
            this.lblOrdersChange.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblOrdersChange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(178)))), ((int)(((byte)(72)))));
            this.lblOrdersChange.Location = new System.Drawing.Point(76, 68);
            this.lblOrdersChange.Name = "lblOrdersChange";
            this.lblOrdersChange.Size = new System.Drawing.Size(137, 20);
            this.lblOrdersChange.TabIndex = 3;
            this.lblOrdersChange.Text = "0% vs previous period";
            // 
            // cardAverageOrder
            // 
            this.cardAverageOrder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardAverageOrder.Controls.Add(this.iconAverage);
            this.cardAverageOrder.Controls.Add(this.lblAverageOrderTitle);
            this.cardAverageOrder.Controls.Add(this.lblAverageOrder);
            this.cardAverageOrder.Controls.Add(this.lblAverageChange);
            this.cardAverageOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardAverageOrder.Location = new System.Drawing.Point(456, 6);
            this.cardAverageOrder.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardAverageOrder.Name = "cardAverageOrder";
            this.cardAverageOrder.Size = new System.Drawing.Size(216, 110);
            this.cardAverageOrder.TabIndex = 2;
            // 
            // iconAverage
            // 
            this.iconAverage.BackColor = System.Drawing.Color.Transparent;
            this.iconAverage.Location = new System.Drawing.Point(16, 26);
            this.iconAverage.Name = "iconAverage";
            this.iconAverage.Size = new System.Drawing.Size(46, 46);
            this.iconAverage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.iconAverage.TabIndex = 0;
            this.iconAverage.TabStop = false;
            // 
            // lblAverageOrderTitle
            // 
            this.lblAverageOrderTitle.AutoSize = true;
            this.lblAverageOrderTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblAverageOrderTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblAverageOrderTitle.Location = new System.Drawing.Point(76, 18);
            this.lblAverageOrderTitle.Name = "lblAverageOrderTitle";
            this.lblAverageOrderTitle.Size = new System.Drawing.Size(115, 15);
            this.lblAverageOrderTitle.TabIndex = 1;
            this.lblAverageOrderTitle.Text = "Average Order Value";
            // 
            // lblAverageOrder
            // 
            this.lblAverageOrder.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblAverageOrder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblAverageOrder.Location = new System.Drawing.Point(76, 39);
            this.lblAverageOrder.Name = "lblAverageOrder";
            this.lblAverageOrder.Size = new System.Drawing.Size(137, 26);
            this.lblAverageOrder.TabIndex = 2;
            this.lblAverageOrder.Text = "Rs 0.00";
            // 
            // lblAverageChange
            // 
            this.lblAverageChange.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblAverageChange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(178)))), ((int)(((byte)(72)))));
            this.lblAverageChange.Location = new System.Drawing.Point(76, 68);
            this.lblAverageChange.Name = "lblAverageChange";
            this.lblAverageChange.Size = new System.Drawing.Size(137, 20);
            this.lblAverageChange.TabIndex = 3;
            this.lblAverageChange.Text = "0% vs previous period";
            // 
            // cardProfit
            // 
            this.cardProfit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardProfit.Controls.Add(this.iconProfit);
            this.cardProfit.Controls.Add(this.lblProfitTitle);
            this.cardProfit.Controls.Add(this.lblProfit);
            this.cardProfit.Controls.Add(this.lblProfitChange);
            this.cardProfit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardProfit.Location = new System.Drawing.Point(684, 6);
            this.cardProfit.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardProfit.Name = "cardProfit";
            this.cardProfit.Size = new System.Drawing.Size(216, 110);
            this.cardProfit.TabIndex = 3;
            // 
            // iconProfit
            // 
            this.iconProfit.BackColor = System.Drawing.Color.Transparent;
            this.iconProfit.Location = new System.Drawing.Point(16, 26);
            this.iconProfit.Name = "iconProfit";
            this.iconProfit.Size = new System.Drawing.Size(46, 46);
            this.iconProfit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.iconProfit.TabIndex = 0;
            this.iconProfit.TabStop = false;
            // 
            // lblProfitTitle
            // 
            this.lblProfitTitle.AutoSize = true;
            this.lblProfitTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblProfitTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblProfitTitle.Location = new System.Drawing.Point(76, 18);
            this.lblProfitTitle.Name = "lblProfitTitle";
            this.lblProfitTitle.Size = new System.Drawing.Size(65, 15);
            this.lblProfitTitle.TabIndex = 1;
            this.lblProfitTitle.Text = "Total Profit";
            // 
            // lblProfit
            // 
            this.lblProfit.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblProfit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblProfit.Location = new System.Drawing.Point(76, 39);
            this.lblProfit.Name = "lblProfit";
            this.lblProfit.Size = new System.Drawing.Size(137, 26);
            this.lblProfit.TabIndex = 2;
            this.lblProfit.Text = "Rs 0.00";
            // 
            // lblProfitChange
            // 
            this.lblProfitChange.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblProfitChange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(178)))), ((int)(((byte)(72)))));
            this.lblProfitChange.Location = new System.Drawing.Point(76, 68);
            this.lblProfitChange.Name = "lblProfitChange";
            this.lblProfitChange.Size = new System.Drawing.Size(137, 20);
            this.lblProfitChange.TabIndex = 3;
            this.lblProfitChange.Text = "0% vs previous period";
            // 
            // cardItemsSold
            // 
            this.cardItemsSold.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardItemsSold.Controls.Add(this.iconItems);
            this.cardItemsSold.Controls.Add(this.lblItemsSoldTitle);
            this.cardItemsSold.Controls.Add(this.lblItemsSold);
            this.cardItemsSold.Controls.Add(this.lblItemsChange);
            this.cardItemsSold.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardItemsSold.Location = new System.Drawing.Point(912, 6);
            this.cardItemsSold.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardItemsSold.Name = "cardItemsSold";
            this.cardItemsSold.Size = new System.Drawing.Size(220, 110);
            this.cardItemsSold.TabIndex = 4;
            // 
            // iconItems
            // 
            this.iconItems.BackColor = System.Drawing.Color.Transparent;
            this.iconItems.Location = new System.Drawing.Point(16, 26);
            this.iconItems.Name = "iconItems";
            this.iconItems.Size = new System.Drawing.Size(46, 46);
            this.iconItems.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.iconItems.TabIndex = 0;
            this.iconItems.TabStop = false;
            // 
            // lblItemsSoldTitle
            // 
            this.lblItemsSoldTitle.AutoSize = true;
            this.lblItemsSoldTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblItemsSoldTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblItemsSoldTitle.Location = new System.Drawing.Point(76, 18);
            this.lblItemsSoldTitle.Name = "lblItemsSoldTitle";
            this.lblItemsSoldTitle.Size = new System.Drawing.Size(93, 15);
            this.lblItemsSoldTitle.TabIndex = 1;
            this.lblItemsSoldTitle.Text = "Total Items Sold";
            // 
            // lblItemsSold
            // 
            this.lblItemsSold.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblItemsSold.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblItemsSold.Location = new System.Drawing.Point(76, 39);
            this.lblItemsSold.Name = "lblItemsSold";
            this.lblItemsSold.Size = new System.Drawing.Size(129, 26);
            this.lblItemsSold.TabIndex = 2;
            this.lblItemsSold.Text = "0";
            // 
            // lblItemsChange
            // 
            this.lblItemsChange.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblItemsChange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(178)))), ((int)(((byte)(72)))));
            this.lblItemsChange.Location = new System.Drawing.Point(76, 68);
            this.lblItemsChange.Name = "lblItemsChange";
            this.lblItemsChange.Size = new System.Drawing.Size(129, 20);
            this.lblItemsChange.TabIndex = 3;
            this.lblItemsChange.Text = "0% vs previous period";
            // 
            // contentLayout
            // 
            this.contentLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.contentLayout.ColumnCount = 3;
            this.contentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 53F));
            this.contentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.5F));
            this.contentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.5F));
            this.contentLayout.Controls.Add(this.trendPanel, 0, 0);
            this.contentLayout.Controls.Add(this.topQtyPanel, 1, 0);
            this.contentLayout.Controls.Add(this.topAmountPanel, 2, 0);
            this.contentLayout.Controls.Add(this.itemMapPanel, 0, 1);
            this.contentLayout.Controls.Add(this.paymentPanel, 1, 1);
            this.contentLayout.Controls.Add(this.categoryPanel, 2, 1);
            this.contentLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentLayout.Location = new System.Drawing.Point(18, 204);
            this.contentLayout.Margin = new System.Windows.Forms.Padding(0);
            this.contentLayout.Name = "contentLayout";
            this.contentLayout.RowCount = 3;
            this.contentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 52F));
            this.contentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 48F));
            this.contentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.contentLayout.Size = new System.Drawing.Size(1144, 432);
            this.contentLayout.TabIndex = 2;
            // 
            // trendPanel
            // 
            this.trendPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.trendPanel.Controls.Add(this.lblTrendTitle);
            this.trendPanel.Controls.Add(this.trendCanvas);
            this.trendPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trendPanel.Location = new System.Drawing.Point(0, 0);
            this.trendPanel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.trendPanel.Name = "trendPanel";
            this.trendPanel.Size = new System.Drawing.Size(594, 212);
            this.trendPanel.TabIndex = 0;
            // 
            // lblTrendTitle
            // 
            this.lblTrendTitle.AutoSize = true;
            this.lblTrendTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTrendTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTrendTitle.Location = new System.Drawing.Point(9, 2);
            this.lblTrendTitle.Name = "lblTrendTitle";
            this.lblTrendTitle.Size = new System.Drawing.Size(105, 15);
            this.lblTrendTitle.TabIndex = 0;
            this.lblTrendTitle.Text = "Sales Trend (Daily)";
            // 
            // trendCanvas
            // 
            this.trendCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trendCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.trendCanvas.Location = new System.Drawing.Point(14, 20);
            this.trendCanvas.Name = "trendCanvas";
            this.trendCanvas.Size = new System.Drawing.Size(567, 179);
            this.trendCanvas.TabIndex = 1;
            // 
            // topQtyPanel
            // 
            this.topQtyPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.topQtyPanel.Controls.Add(this.lblTopQtyTitle);
            this.topQtyPanel.Controls.Add(this.gridTopQty);
            this.topQtyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topQtyPanel.Location = new System.Drawing.Point(606, 0);
            this.topQtyPanel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.topQtyPanel.Name = "topQtyPanel";
            this.topQtyPanel.Size = new System.Drawing.Size(256, 212);
            this.topQtyPanel.TabIndex = 1;
            // 
            // lblTopQtyTitle
            // 
            this.lblTopQtyTitle.AutoSize = true;
            this.lblTopQtyTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTopQtyTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTopQtyTitle.Location = new System.Drawing.Point(1, 2);
            this.lblTopQtyTitle.Name = "lblTopQtyTitle";
            this.lblTopQtyTitle.Size = new System.Drawing.Size(172, 15);
            this.lblTopQtyTitle.TabIndex = 0;
            this.lblTopQtyTitle.Text = "Top Selling Items (By Quantity)";
            // 
            // gridTopQty
            // 
            this.gridTopQty.AllowUserToAddRows = false;
            this.gridTopQty.AllowUserToDeleteRows = false;
            this.gridTopQty.AllowUserToResizeRows = false;
            this.gridTopQty.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridTopQty.AutoGenerateColumns = false;
            this.gridTopQty.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridTopQty.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.gridTopQty.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(252)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridTopQty.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.gridTopQty.ColumnHeadersHeight = 26;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(64)))), ((int)(((byte)(105)))));
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(232)))), ((int)(((byte)(250)))));
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridTopQty.DefaultCellStyle = dataGridViewCellStyle6;
            this.gridTopQty.EnableHeadersVisualStyles = false;
            this.gridTopQty.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(233)))), ((int)(((byte)(246)))));
            this.gridTopQty.Location = new System.Drawing.Point(12, 20);
            this.gridTopQty.Name = "gridTopQty";
            this.gridTopQty.ReadOnly = true;
            this.gridTopQty.RowHeadersVisible = false;
            this.gridTopQty.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridTopQty.Size = new System.Drawing.Size(227, 179);
            this.gridTopQty.TabIndex = 1;
            // 
            // colQtyRank
            // 
            this.colQtyRank.DataPropertyName = "Rank";
            this.colQtyRank.FillWeight = 34F;
            this.colQtyRank.HeaderText = "#";
            this.colQtyRank.Name = "QtyRank";
            this.colQtyRank.ReadOnly = true;
            // 
            // colQtyItemName
            // 
            this.colQtyItemName.DataPropertyName = "ItemName";
            this.colQtyItemName.FillWeight = 130F;
            this.colQtyItemName.HeaderText = "Item Name";
            this.colQtyItemName.Name = "QtyItemName";
            this.colQtyItemName.ReadOnly = true;
            // 
            // colQtySold
            // 
            this.colQtySold.DataPropertyName = "QtySold";
            this.colQtySold.FillWeight = 72F;
            this.colQtySold.HeaderText = "QtySold";
            this.colQtySold.Name = "QtySoldCol";
            this.colQtySold.ReadOnly = true;
            this.gridTopQty.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colQtyRank,
            this.colQtyItemName,
            this.colQtySold});
            // 
            // topAmountPanel
            // 
            this.topAmountPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.topAmountPanel.Controls.Add(this.lblTopAmountTitle);
            this.topAmountPanel.Controls.Add(this.gridTopAmount);
            this.topAmountPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topAmountPanel.Location = new System.Drawing.Point(874, 0);
            this.topAmountPanel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.topAmountPanel.Name = "topAmountPanel";
            this.topAmountPanel.Size = new System.Drawing.Size(258, 212);
            this.topAmountPanel.TabIndex = 2;
            // 
            // lblTopAmountTitle
            // 
            this.lblTopAmountTitle.AutoSize = true;
            this.lblTopAmountTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTopAmountTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTopAmountTitle.Location = new System.Drawing.Point(3, 1);
            this.lblTopAmountTitle.Name = "lblTopAmountTitle";
            this.lblTopAmountTitle.Size = new System.Drawing.Size(170, 15);
            this.lblTopAmountTitle.TabIndex = 0;
            this.lblTopAmountTitle.Text = "Top Selling Items (By Amount)";
            // 
            // gridTopAmount
            // 
            this.gridTopAmount.AllowUserToAddRows = false;
            this.gridTopAmount.AllowUserToDeleteRows = false;
            this.gridTopAmount.AllowUserToResizeRows = false;
            this.gridTopAmount.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridTopAmount.AutoGenerateColumns = false;
            this.gridTopAmount.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridTopAmount.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.gridTopAmount.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(252)))));
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridTopAmount.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.gridTopAmount.ColumnHeadersHeight = 26;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(64)))), ((int)(((byte)(105)))));
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(232)))), ((int)(((byte)(250)))));
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridTopAmount.DefaultCellStyle = dataGridViewCellStyle8;
            this.gridTopAmount.EnableHeadersVisualStyles = false;
            this.gridTopAmount.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(233)))), ((int)(((byte)(246)))));
            this.gridTopAmount.Location = new System.Drawing.Point(11, 20);
            this.gridTopAmount.Name = "gridTopAmount";
            this.gridTopAmount.ReadOnly = true;
            this.gridTopAmount.RowHeadersVisible = false;
            this.gridTopAmount.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridTopAmount.Size = new System.Drawing.Size(237, 179);
            this.gridTopAmount.TabIndex = 1;
            // 
            // colAmountRank
            // 
            this.colAmountRank.DataPropertyName = "Rank";
            this.colAmountRank.FillWeight = 34F;
            this.colAmountRank.HeaderText = "#";
            this.colAmountRank.Name = "AmountRank";
            this.colAmountRank.ReadOnly = true;
            // 
            // colAmountItemName
            // 
            this.colAmountItemName.DataPropertyName = "ItemName";
            this.colAmountItemName.FillWeight = 130F;
            this.colAmountItemName.HeaderText = "Item Name";
            this.colAmountItemName.Name = "AmountItemName";
            this.colAmountItemName.ReadOnly = true;
            // 
            // colAmountValue
            // 
            this.colAmountValue.DataPropertyName = "Amount";
            this.colAmountValue.FillWeight = 86F;
            this.colAmountValue.HeaderText = "Amount";
            this.colAmountValue.Name = "AmountValue";
            this.colAmountValue.ReadOnly = true;
            this.gridTopAmount.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colAmountRank,
            this.colAmountItemName,
            this.colAmountValue});
            // 
            // itemMapPanel
            // 
            this.itemMapPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.itemMapPanel.Controls.Add(this.cmbItemMapSort);
            this.itemMapPanel.Controls.Add(this.itemMapCanvas);
            this.itemMapPanel.Controls.Add(this.lblItemMapTitle);
            this.itemMapPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.itemMapPanel.Location = new System.Drawing.Point(0, 224);
            this.itemMapPanel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.itemMapPanel.Name = "itemMapPanel";
            this.itemMapPanel.Size = new System.Drawing.Size(594, 195);
            this.itemMapPanel.TabIndex = 3;
            // 
            // itemMapCanvas
            // 
            this.itemMapCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.itemMapCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.itemMapCanvas.Location = new System.Drawing.Point(10, 34);
            this.itemMapCanvas.Name = "itemMapCanvas";
            this.itemMapCanvas.Size = new System.Drawing.Size(574, 151);
            this.itemMapCanvas.TabIndex = 1;
            // 
            // lblItemMapTitle
            // 
            this.lblItemMapTitle.AutoSize = true;
            this.lblItemMapTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblItemMapTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblItemMapTitle.Location = new System.Drawing.Point(14, 8);
            this.lblItemMapTitle.Name = "lblItemMapTitle";
            this.lblItemMapTitle.Size = new System.Drawing.Size(99, 15);
            this.lblItemMapTitle.TabIndex = 0;
            this.lblItemMapTitle.Text = "Item Sales (Brief)";
            // 
            // cmbItemMapSort
            // 
            this.cmbItemMapSort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbItemMapSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbItemMapSort.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.cmbItemMapSort.FormattingEnabled = true;
            this.cmbItemMapSort.Items.AddRange(new object[] {
            "By Amount",
            "By Quantity"});
            this.cmbItemMapSort.Location = new System.Drawing.Point(476, 6);
            this.cmbItemMapSort.Name = "cmbItemMapSort";
            this.cmbItemMapSort.SelectedIndex = 0;
            this.cmbItemMapSort.Size = new System.Drawing.Size(104, 21);
            this.cmbItemMapSort.TabIndex = 2;
            // 
            // paymentPanel
            // 
            this.paymentPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.paymentPanel.Controls.Add(this.lblPaymentTitle);
            this.paymentPanel.Controls.Add(this.paymentCanvas);
            this.paymentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.paymentPanel.Location = new System.Drawing.Point(606, 224);
            this.paymentPanel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.paymentPanel.Name = "paymentPanel";
            this.paymentPanel.Size = new System.Drawing.Size(256, 195);
            this.paymentPanel.TabIndex = 4;
            // 
            // lblPaymentTitle
            // 
            this.lblPaymentTitle.AutoSize = true;
            this.lblPaymentTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblPaymentTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblPaymentTitle.Location = new System.Drawing.Point(4, 1);
            this.lblPaymentTitle.Name = "lblPaymentTitle";
            this.lblPaymentTitle.Size = new System.Drawing.Size(145, 15);
            this.lblPaymentTitle.TabIndex = 0;
            this.lblPaymentTitle.Text = "Sales by Payment Method";
            // 
            // paymentCanvas
            // 
            this.paymentCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.paymentCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.paymentCanvas.Location = new System.Drawing.Point(6, 19);
            this.paymentCanvas.Name = "paymentCanvas";
            this.paymentCanvas.Size = new System.Drawing.Size(247, 173);
            this.paymentCanvas.TabIndex = 1;
            // 
            // categoryPanel
            // 
            this.categoryPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.categoryPanel.Controls.Add(this.lblCategoryTitle);
            this.categoryPanel.Controls.Add(this.categoryCanvas);
            this.categoryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.categoryPanel.Location = new System.Drawing.Point(874, 224);
            this.categoryPanel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.categoryPanel.Name = "categoryPanel";
            this.categoryPanel.Size = new System.Drawing.Size(258, 195);
            this.categoryPanel.TabIndex = 5;
            // 
            // lblCategoryTitle
            // 
            this.lblCategoryTitle.AutoSize = true;
            this.lblCategoryTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblCategoryTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblCategoryTitle.Location = new System.Drawing.Point(8, 1);
            this.lblCategoryTitle.Name = "lblCategoryTitle";
            this.lblCategoryTitle.Size = new System.Drawing.Size(100, 15);
            this.lblCategoryTitle.TabIndex = 0;
            this.lblCategoryTitle.Text = "Sales by Category";
            // 
            // categoryCanvas
            // 
            this.categoryCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.categoryCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.categoryCanvas.Location = new System.Drawing.Point(5, 19);
            this.categoryCanvas.Name = "categoryCanvas";
            this.categoryCanvas.Size = new System.Drawing.Size(250, 173);
            this.categoryCanvas.TabIndex = 1;
            // 
            // FrmSalesAnalytics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.ClientSize = new System.Drawing.Size(1180, 650);
            this.Controls.Add(this.mainLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmSalesAnalytics";
            this.Text = "Sales Analytics";
            this.mainLayout.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.metricsLayout.ResumeLayout(false);
            this.cardTotalSales.ResumeLayout(false);
            this.cardTotalSales.PerformLayout();
            this.cardOrders.ResumeLayout(false);
            this.cardOrders.PerformLayout();
            this.cardAverageOrder.ResumeLayout(false);
            this.cardAverageOrder.PerformLayout();
            this.cardProfit.ResumeLayout(false);
            this.cardProfit.PerformLayout();
            this.cardItemsSold.ResumeLayout(false);
            this.cardItemsSold.PerformLayout();
            this.contentLayout.ResumeLayout(false);
            this.trendPanel.ResumeLayout(false);
            this.trendPanel.PerformLayout();
            this.topQtyPanel.ResumeLayout(false);
            this.topQtyPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTopQty)).EndInit();
            this.topAmountPanel.ResumeLayout(false);
            this.topAmountPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTopAmount)).EndInit();
            this.itemMapPanel.ResumeLayout(false);
            this.itemMapPanel.PerformLayout();
            this.paymentPanel.ResumeLayout(false);
            this.paymentPanel.PerformLayout();
            this.categoryPanel.ResumeLayout(false);
            this.categoryPanel.PerformLayout();
            this.ResumeLayout(false);

        }

    }
}
