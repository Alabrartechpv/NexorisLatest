namespace PosBranch_Win.Dashboard
{
    partial class FrmPurchaseAnalytics
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubTitle;
        private System.Windows.Forms.DateTimePicker dtFrom;
        private System.Windows.Forms.DateTimePicker dtTo;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.TableLayoutPanel metricsLayout;
        private System.Windows.Forms.Panel cardTotalPurchase;
        private System.Windows.Forms.Panel cardTotalVendors;
        private System.Windows.Forms.Panel cardAveragePurchase;
        private System.Windows.Forms.Panel cardItemsPurchased;
        private System.Windows.Forms.PictureBox iconPurchase;
        private System.Windows.Forms.PictureBox iconVendors;
        private System.Windows.Forms.PictureBox iconAveragePurchase;
        private System.Windows.Forms.PictureBox iconItemsPurchased;
        private System.Windows.Forms.Label lblTotalPurchase;
        private System.Windows.Forms.Label lblVendors;
        private System.Windows.Forms.Label lblAveragePurchase;
        private System.Windows.Forms.Label lblItemsPurchased;
        private System.Windows.Forms.Label lblTotalPurchaseTitle;
        private System.Windows.Forms.Label lblVendorsTitle;
        private System.Windows.Forms.Label lblAveragePurchaseTitle;
        private System.Windows.Forms.Label lblItemsPurchasedTitle;
        private System.Windows.Forms.Label lblPurchaseChange;
        private System.Windows.Forms.Label lblVendorsChange;

        private System.Windows.Forms.Label lblVendorsFooter;

        private System.Windows.Forms.Label lblAveragePurchaseChange;
        private System.Windows.Forms.Label lblItemsChange;
        private System.Windows.Forms.TableLayoutPanel contentLayout;
        private System.Windows.Forms.Panel trendPanel;
        private System.Windows.Forms.Panel topQtyPanel;
        private System.Windows.Forms.Panel topAmountPanel;
        private System.Windows.Forms.Panel briefPanel;
        private System.Windows.Forms.Panel paymentPanel;
        private System.Windows.Forms.Panel categoryPanel;
        private System.Windows.Forms.Label lblTrendTitle;
        private System.Windows.Forms.Label lblTopQtyTitle;
        private System.Windows.Forms.Label lblTopAmountTitle;
        private System.Windows.Forms.Label lblBriefTitle;
        private System.Windows.Forms.Label lblPaymentTitle;
        private System.Windows.Forms.Label lblCategoryTitle;
        private System.Windows.Forms.Panel trendCanvas;
        private System.Windows.Forms.Panel briefCanvas;
        private System.Windows.Forms.ComboBox cmbItemMapSort;

        private System.Windows.Forms.Panel paymentCanvas;
        private System.Windows.Forms.Panel categoryCanvas;
        private System.Windows.Forms.DataGridView gridTopQty;
        private System.Windows.Forms.DataGridView gridTopAmount;

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
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubTitle = new System.Windows.Forms.Label();
            this.dtFrom = new System.Windows.Forms.DateTimePicker();
            this.dtTo = new System.Windows.Forms.DateTimePicker();
            this.btnApply = new System.Windows.Forms.Button();
            this.metricsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.cardTotalPurchase = new System.Windows.Forms.Panel();
            this.iconPurchase = new System.Windows.Forms.PictureBox();
            this.lblTotalPurchaseTitle = new System.Windows.Forms.Label();
            this.lblTotalPurchase = new System.Windows.Forms.Label();
            this.lblPurchaseChange = new System.Windows.Forms.Label();
            this.cardTotalVendors = new System.Windows.Forms.Panel();
            this.iconVendors = new System.Windows.Forms.PictureBox();
            this.lblVendorsTitle = new System.Windows.Forms.Label();
            this.lblVendors = new System.Windows.Forms.Label();
            this.lblVendorsChange = new System.Windows.Forms.Label();
            this.lblVendorsFooter = new System.Windows.Forms.Label();
            this.cardAveragePurchase = new System.Windows.Forms.Panel();
            this.iconAveragePurchase = new System.Windows.Forms.PictureBox();
            this.lblAveragePurchaseTitle = new System.Windows.Forms.Label();
            this.lblAveragePurchase = new System.Windows.Forms.Label();
            this.lblAveragePurchaseChange = new System.Windows.Forms.Label();
            this.cardItemsPurchased = new System.Windows.Forms.Panel();
            this.iconItemsPurchased = new System.Windows.Forms.PictureBox();
            this.lblItemsPurchasedTitle = new System.Windows.Forms.Label();
            this.lblItemsPurchased = new System.Windows.Forms.Label();
            this.lblItemsChange = new System.Windows.Forms.Label();
            this.contentLayout = new System.Windows.Forms.TableLayoutPanel();
            this.trendPanel = new System.Windows.Forms.Panel();
            this.lblTrendTitle = new System.Windows.Forms.Label();
            this.trendCanvas = new System.Windows.Forms.Panel();
            this.topQtyPanel = new System.Windows.Forms.Panel();
            this.lblTopQtyTitle = new System.Windows.Forms.Label();
            this.gridTopQty = new System.Windows.Forms.DataGridView();
            this.topAmountPanel = new System.Windows.Forms.Panel();
            this.lblTopAmountTitle = new System.Windows.Forms.Label();
            this.gridTopAmount = new System.Windows.Forms.DataGridView();
            this.briefPanel = new System.Windows.Forms.Panel();
            this.briefCanvas = new System.Windows.Forms.Panel();
            this.lblBriefTitle = new System.Windows.Forms.Label();
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
            this.cardTotalPurchase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconPurchase)).BeginInit();
            this.cardTotalVendors.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconVendors)).BeginInit();
            this.cardAveragePurchase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconAveragePurchase)).BeginInit();
            this.cardItemsPurchased.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconItemsPurchased)).BeginInit();
            this.contentLayout.SuspendLayout();
            this.trendPanel.SuspendLayout();
            this.topQtyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTopQty)).BeginInit();
            this.topAmountPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTopAmount)).BeginInit();
            this.briefPanel.SuspendLayout();
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
            this.mainLayout.RowCount = 4;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 126F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.mainLayout.Size = new System.Drawing.Size(1180, 650);
            this.mainLayout.TabIndex = 0;
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.headerPanel.Controls.Add(this.lblTitle);
            this.headerPanel.Controls.Add(this.lblSubTitle);
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
            this.lblTitle.Size = new System.Drawing.Size(271, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Purchase Analytics Overview";
            // 
            // lblSubTitle
            // 
            this.lblSubTitle.AutoSize = true;
            this.lblSubTitle.Font = new System.Drawing.Font("Segoe UI", 8.75F);
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(93)))), ((int)(((byte)(132)))));
            this.lblSubTitle.Location = new System.Drawing.Point(6, 35);
            this.lblSubTitle.Name = "lblSubTitle";
            this.lblSubTitle.Size = new System.Drawing.Size(434, 15);
            this.lblSubTitle.TabIndex = 1;
            this.lblSubTitle.Text = "Summary of purchase performance, item movement, vendors, and payment mix.";
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
            this.dtFrom.TabIndex = 2;
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
            this.btnApply.Location = new System.Drawing.Point(1034, 13);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(88, 28);
            this.btnApply.TabIndex = 4;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = false;
            // 
            // metricsLayout
            // 
            this.metricsLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.metricsLayout.ColumnCount = 4;
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.metricsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.metricsLayout.Controls.Add(this.cardTotalPurchase, 0, 0);
            this.metricsLayout.Controls.Add(this.cardTotalVendors, 1, 0);
            this.metricsLayout.Controls.Add(this.cardAveragePurchase, 2, 0);
            this.metricsLayout.Controls.Add(this.cardItemsPurchased, 3, 0);
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
            // cardTotalPurchase
            // 
            this.cardTotalPurchase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardTotalPurchase.Controls.Add(this.iconPurchase);
            this.cardTotalPurchase.Controls.Add(this.lblTotalPurchaseTitle);
            this.cardTotalPurchase.Controls.Add(this.lblTotalPurchase);
            this.cardTotalPurchase.Controls.Add(this.lblPurchaseChange);
            this.cardTotalPurchase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardTotalPurchase.Location = new System.Drawing.Point(0, 6);
            this.cardTotalPurchase.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardTotalPurchase.Name = "cardTotalPurchase";
            this.cardTotalPurchase.Size = new System.Drawing.Size(274, 110);
            this.cardTotalPurchase.TabIndex = 0;
            // 
            // iconPurchase
            // 
            this.iconPurchase.BackColor = System.Drawing.Color.Transparent;
            this.iconPurchase.Location = new System.Drawing.Point(16, 26);
            this.iconPurchase.Name = "iconPurchase";
            this.iconPurchase.Size = new System.Drawing.Size(46, 46);
            this.iconPurchase.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.iconPurchase.TabIndex = 0;
            this.iconPurchase.TabStop = false;
            // 
            // lblTotalPurchaseTitle
            // 
            this.lblTotalPurchaseTitle.AutoSize = true;
            this.lblTotalPurchaseTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTotalPurchaseTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTotalPurchaseTitle.Location = new System.Drawing.Point(76, 18);
            this.lblTotalPurchaseTitle.Name = "lblTotalPurchaseTitle";
            this.lblTotalPurchaseTitle.Size = new System.Drawing.Size(84, 15);
            this.lblTotalPurchaseTitle.TabIndex = 1;
            this.lblTotalPurchaseTitle.Text = "Total Purchase";
            // 
            // lblTotalPurchase
            // 
            this.lblTotalPurchase.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblTotalPurchase.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTotalPurchase.Location = new System.Drawing.Point(76, 39);
            this.lblTotalPurchase.Name = "lblTotalPurchase";
            this.lblTotalPurchase.Size = new System.Drawing.Size(137, 26);
            this.lblTotalPurchase.TabIndex = 2;
            this.lblTotalPurchase.Text = "Rs 0.00";
            // 
            // lblPurchaseChange
            // 
            this.lblPurchaseChange.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblPurchaseChange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(178)))), ((int)(((byte)(72)))));
            this.lblPurchaseChange.Location = new System.Drawing.Point(76, 68);
            this.lblPurchaseChange.Name = "lblPurchaseChange";
            this.lblPurchaseChange.Size = new System.Drawing.Size(137, 20);
            this.lblPurchaseChange.TabIndex = 3;
            this.lblPurchaseChange.Text = "0% vs previous period";
            // 
            // cardTotalVendors
            // 
            this.cardTotalVendors.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardTotalVendors.Controls.Add(this.iconVendors);
            this.cardTotalVendors.Controls.Add(this.lblVendorsTitle);
            this.cardTotalVendors.Controls.Add(this.lblVendors);
            this.cardTotalVendors.Controls.Add(this.lblVendorsChange);
            this.cardTotalVendors.Controls.Add(this.lblVendorsFooter);
            this.cardTotalVendors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardTotalVendors.Location = new System.Drawing.Point(286, 6);
            this.cardTotalVendors.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardTotalVendors.Name = "cardTotalVendors";
            this.cardTotalVendors.Size = new System.Drawing.Size(274, 110);
            this.cardTotalVendors.TabIndex = 1;
            // 
            // iconVendors
            // 
            this.iconVendors.BackColor = System.Drawing.Color.Transparent;
            this.iconVendors.Location = new System.Drawing.Point(16, 26);
            this.iconVendors.Name = "iconVendors";
            this.iconVendors.Size = new System.Drawing.Size(46, 46);
            this.iconVendors.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.iconVendors.TabIndex = 0;
            this.iconVendors.TabStop = false;
            // 
            // lblVendorsTitle
            // 
            this.lblVendorsTitle.AutoSize = true;
            this.lblVendorsTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblVendorsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblVendorsTitle.Location = new System.Drawing.Point(76, 18);
            this.lblVendorsTitle.Name = "lblVendorsTitle";
            this.lblVendorsTitle.Size = new System.Drawing.Size(79, 15);
            this.lblVendorsTitle.TabIndex = 1;
            this.lblVendorsTitle.Text = "Total Vendors";
            // 
            // lblVendors
            // 
            this.lblVendors.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblVendors.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblVendors.Location = new System.Drawing.Point(76, 39);
            this.lblVendors.Name = "lblVendors";
            this.lblVendors.Size = new System.Drawing.Size(137, 26);
            this.lblVendors.TabIndex = 2;
            this.lblVendors.Text = "0";
            // 
            // lblVendorsChange
            // 
            this.lblVendorsChange.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblVendorsChange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(178)))), ((int)(((byte)(72)))));
            this.lblVendorsChange.Location = new System.Drawing.Point(76, 68);
            this.lblVendorsChange.Name = "lblVendorsChange";
            this.lblVendorsChange.Size = new System.Drawing.Size(137, 20);
            this.lblVendorsChange.TabIndex = 3;
            this.lblVendorsChange.Text = "0% vs previous period";
            // 
            // lblVendorsFooter
            // 
            this.lblVendorsFooter.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblVendorsFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(93)))), ((int)(((byte)(132)))));
            this.lblVendorsFooter.Location = new System.Drawing.Point(76, 52);
            this.lblVendorsFooter.Name = "lblVendorsFooter";
            this.lblVendorsFooter.Size = new System.Drawing.Size(137, 16);
            this.lblVendorsFooter.TabIndex = 4;
            this.lblVendorsFooter.Text = "Active Vendors";
            // 
            // cardAveragePurchase
            // 
            this.cardAveragePurchase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardAveragePurchase.Controls.Add(this.iconAveragePurchase);
            this.cardAveragePurchase.Controls.Add(this.lblAveragePurchaseTitle);
            this.cardAveragePurchase.Controls.Add(this.lblAveragePurchase);
            this.cardAveragePurchase.Controls.Add(this.lblAveragePurchaseChange);
            this.cardAveragePurchase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardAveragePurchase.Location = new System.Drawing.Point(572, 6);
            this.cardAveragePurchase.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardAveragePurchase.Name = "cardAveragePurchase";
            this.cardAveragePurchase.Size = new System.Drawing.Size(274, 110);
            this.cardAveragePurchase.TabIndex = 2;
            // 
            // iconAveragePurchase
            // 
            this.iconAveragePurchase.BackColor = System.Drawing.Color.Transparent;
            this.iconAveragePurchase.Location = new System.Drawing.Point(16, 26);
            this.iconAveragePurchase.Name = "iconAveragePurchase";
            this.iconAveragePurchase.Size = new System.Drawing.Size(46, 46);
            this.iconAveragePurchase.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.iconAveragePurchase.TabIndex = 0;
            this.iconAveragePurchase.TabStop = false;
            // 
            // lblAveragePurchaseTitle
            // 
            this.lblAveragePurchaseTitle.AutoSize = true;
            this.lblAveragePurchaseTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblAveragePurchaseTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblAveragePurchaseTitle.Location = new System.Drawing.Point(76, 18);
            this.lblAveragePurchaseTitle.Name = "lblAveragePurchaseTitle";
            this.lblAveragePurchaseTitle.Size = new System.Drawing.Size(133, 15);
            this.lblAveragePurchaseTitle.TabIndex = 1;
            this.lblAveragePurchaseTitle.Text = "Average Purchase Value";
            // 
            // lblAveragePurchase
            // 
            this.lblAveragePurchase.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblAveragePurchase.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblAveragePurchase.Location = new System.Drawing.Point(76, 39);
            this.lblAveragePurchase.Name = "lblAveragePurchase";
            this.lblAveragePurchase.Size = new System.Drawing.Size(137, 26);
            this.lblAveragePurchase.TabIndex = 2;
            this.lblAveragePurchase.Text = "Rs 0.00";
            // 
            // lblAveragePurchaseChange
            // 
            this.lblAveragePurchaseChange.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblAveragePurchaseChange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(178)))), ((int)(((byte)(72)))));
            this.lblAveragePurchaseChange.Location = new System.Drawing.Point(76, 68);
            this.lblAveragePurchaseChange.Name = "lblAveragePurchaseChange";
            this.lblAveragePurchaseChange.Size = new System.Drawing.Size(137, 20);
            this.lblAveragePurchaseChange.TabIndex = 3;
            this.lblAveragePurchaseChange.Text = "0% vs previous period";
            // 
            // cardItemsPurchased
            // 
            this.cardItemsPurchased.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.cardItemsPurchased.Controls.Add(this.iconItemsPurchased);
            this.cardItemsPurchased.Controls.Add(this.lblItemsPurchasedTitle);
            this.cardItemsPurchased.Controls.Add(this.lblItemsPurchased);
            this.cardItemsPurchased.Controls.Add(this.lblItemsChange);
            this.cardItemsPurchased.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardItemsPurchased.Location = new System.Drawing.Point(858, 6);
            this.cardItemsPurchased.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardItemsPurchased.Name = "cardItemsPurchased";
            this.cardItemsPurchased.Size = new System.Drawing.Size(274, 110);
            this.cardItemsPurchased.TabIndex = 4;
            // 
            // iconItemsPurchased
            // 
            this.iconItemsPurchased.BackColor = System.Drawing.Color.Transparent;
            this.iconItemsPurchased.Location = new System.Drawing.Point(16, 26);
            this.iconItemsPurchased.Name = "iconItemsPurchased";
            this.iconItemsPurchased.Size = new System.Drawing.Size(46, 46);
            this.iconItemsPurchased.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.iconItemsPurchased.TabIndex = 0;
            this.iconItemsPurchased.TabStop = false;
            // 
            // lblItemsPurchasedTitle
            // 
            this.lblItemsPurchasedTitle.AutoSize = true;
            this.lblItemsPurchasedTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblItemsPurchasedTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblItemsPurchasedTitle.Location = new System.Drawing.Point(76, 18);
            this.lblItemsPurchasedTitle.Name = "lblItemsPurchasedTitle";
            this.lblItemsPurchasedTitle.Size = new System.Drawing.Size(124, 15);
            this.lblItemsPurchasedTitle.TabIndex = 1;
            this.lblItemsPurchasedTitle.Text = "Total Items Purchased";
            // 
            // lblItemsPurchased
            // 
            this.lblItemsPurchased.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblItemsPurchased.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblItemsPurchased.Location = new System.Drawing.Point(76, 39);
            this.lblItemsPurchased.Name = "lblItemsPurchased";
            this.lblItemsPurchased.Size = new System.Drawing.Size(129, 26);
            this.lblItemsPurchased.TabIndex = 2;
            this.lblItemsPurchased.Text = "0";
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
            this.contentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.97203F));
            this.contentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.42657F));
            this.contentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.5F));
            this.contentLayout.Controls.Add(this.trendPanel, 0, 0);
            this.contentLayout.Controls.Add(this.topQtyPanel, 1, 0);
            this.contentLayout.Controls.Add(this.topAmountPanel, 2, 0);
            this.contentLayout.Controls.Add(this.briefPanel, 0, 1);
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
            this.contentLayout.Size = new System.Drawing.Size(1144, 424);
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
            this.trendPanel.Size = new System.Drawing.Size(594, 208);
            this.trendPanel.TabIndex = 0;
            // 
            // lblTrendTitle
            // 
            this.lblTrendTitle.AutoSize = true;
            this.lblTrendTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTrendTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTrendTitle.Location = new System.Drawing.Point(9, 2);
            this.lblTrendTitle.Name = "lblTrendTitle";
            this.lblTrendTitle.Size = new System.Drawing.Size(126, 15);
            this.lblTrendTitle.TabIndex = 0;
            this.lblTrendTitle.Text = "Purchase Trend (Daily)";
            // 
            // trendCanvas
            // 
            this.trendCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trendCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.trendCanvas.Location = new System.Drawing.Point(14, 20);
            this.trendCanvas.Name = "trendCanvas";
            this.trendCanvas.Size = new System.Drawing.Size(567, 175);
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
            this.topQtyPanel.Size = new System.Drawing.Size(256, 208);
            this.topQtyPanel.TabIndex = 1;
            // 
            // lblTopQtyTitle
            // 
            this.lblTopQtyTitle.AutoSize = true;
            this.lblTopQtyTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTopQtyTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTopQtyTitle.Location = new System.Drawing.Point(1, 2);
            this.lblTopQtyTitle.Name = "lblTopQtyTitle";
            this.lblTopQtyTitle.Size = new System.Drawing.Size(191, 15);
            this.lblTopQtyTitle.TabIndex = 0;
            this.lblTopQtyTitle.Text = "Top Purchased Items (By Quantity)";
            // 
            // gridTopQty
            // 
            this.gridTopQty.AllowUserToAddRows = false;
            this.gridTopQty.AllowUserToDeleteRows = false;
            this.gridTopQty.AllowUserToResizeRows = false;
            this.gridTopQty.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridTopQty.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridTopQty.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.gridTopQty.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(252)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridTopQty.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gridTopQty.ColumnHeadersHeight = 26;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(64)))), ((int)(((byte)(105)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(232)))), ((int)(((byte)(250)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridTopQty.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridTopQty.EnableHeadersVisualStyles = false;
            this.gridTopQty.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(233)))), ((int)(((byte)(246)))));
            this.gridTopQty.Location = new System.Drawing.Point(12, 20);
            this.gridTopQty.Name = "gridTopQty";
            this.gridTopQty.ReadOnly = true;
            this.gridTopQty.RowHeadersVisible = false;
            this.gridTopQty.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridTopQty.Size = new System.Drawing.Size(227, 175);
            this.gridTopQty.TabIndex = 1;
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
            this.topAmountPanel.Size = new System.Drawing.Size(258, 208);
            this.topAmountPanel.TabIndex = 2;
            // 
            // lblTopAmountTitle
            // 
            this.lblTopAmountTitle.AutoSize = true;
            this.lblTopAmountTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTopAmountTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTopAmountTitle.Location = new System.Drawing.Point(3, 1);
            this.lblTopAmountTitle.Name = "lblTopAmountTitle";
            this.lblTopAmountTitle.Size = new System.Drawing.Size(189, 15);
            this.lblTopAmountTitle.TabIndex = 0;
            this.lblTopAmountTitle.Text = "Top Purchased Items (By Amount)";
            // 
            // gridTopAmount
            // 
            this.gridTopAmount.AllowUserToAddRows = false;
            this.gridTopAmount.AllowUserToDeleteRows = false;
            this.gridTopAmount.AllowUserToResizeRows = false;
            this.gridTopAmount.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridTopAmount.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridTopAmount.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.gridTopAmount.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(241)))), ((int)(((byte)(252)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridTopAmount.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.gridTopAmount.ColumnHeadersHeight = 26;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(64)))), ((int)(((byte)(105)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(232)))), ((int)(((byte)(250)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridTopAmount.DefaultCellStyle = dataGridViewCellStyle4;
            this.gridTopAmount.EnableHeadersVisualStyles = false;
            this.gridTopAmount.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(233)))), ((int)(((byte)(246)))));
            this.gridTopAmount.Location = new System.Drawing.Point(11, 20);
            this.gridTopAmount.Name = "gridTopAmount";
            this.gridTopAmount.ReadOnly = true;
            this.gridTopAmount.RowHeadersVisible = false;
            this.gridTopAmount.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridTopAmount.Size = new System.Drawing.Size(237, 175);
            this.gridTopAmount.TabIndex = 1;
            // 
            // briefPanel
            // 
            this.briefPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.briefPanel.Controls.Add(this.cmbItemMapSort);
            this.briefPanel.Controls.Add(this.briefCanvas);
            this.briefPanel.Controls.Add(this.lblBriefTitle);
            this.briefPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.briefPanel.Location = new System.Drawing.Point(0, 220);
            this.briefPanel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.briefPanel.Name = "briefPanel";
            this.briefPanel.Size = new System.Drawing.Size(594, 191);
            this.briefPanel.TabIndex = 3;
            // 
            // briefCanvas
            // 
            this.briefCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.briefCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.briefCanvas.Location = new System.Drawing.Point(10, 34);
            this.briefCanvas.Name = "briefCanvas";
            this.briefCanvas.Size = new System.Drawing.Size(574, 151);
            this.briefCanvas.TabIndex = 1;
            // 
            // lblBriefTitle
            // 
            this.lblBriefTitle.AutoSize = true;
            this.lblBriefTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblBriefTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblBriefTitle.Location = new System.Drawing.Point(14, 8);
            this.lblBriefTitle.Name = "lblBriefTitle";
            this.lblBriefTitle.Size = new System.Drawing.Size(82, 15);
            this.lblBriefTitle.TabIndex = 0;
            this.lblBriefTitle.Text = "Purchase Brief";
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
            this.cmbItemMapSort.Size = new System.Drawing.Size(104, 21);
            this.cmbItemMapSort.TabIndex = 2;
            // 
            // paymentPanel
            // 
            this.paymentPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.paymentPanel.Controls.Add(this.lblPaymentTitle);
            this.paymentPanel.Controls.Add(this.paymentCanvas);
            this.paymentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.paymentPanel.Location = new System.Drawing.Point(606, 220);
            this.paymentPanel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.paymentPanel.Name = "paymentPanel";
            this.paymentPanel.Size = new System.Drawing.Size(256, 191);
            this.paymentPanel.TabIndex = 4;
            // 
            // lblPaymentTitle
            // 
            this.lblPaymentTitle.AutoSize = true;
            this.lblPaymentTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblPaymentTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblPaymentTitle.Location = new System.Drawing.Point(4, 1);
            this.lblPaymentTitle.Name = "lblPaymentTitle";
            this.lblPaymentTitle.Size = new System.Drawing.Size(166, 15);
            this.lblPaymentTitle.TabIndex = 0;
            this.lblPaymentTitle.Text = "Purchase by Payment Method";
            // 
            // paymentCanvas
            // 
            this.paymentCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.paymentCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.paymentCanvas.Location = new System.Drawing.Point(6, 19);
            this.paymentCanvas.Name = "paymentCanvas";
            this.paymentCanvas.Size = new System.Drawing.Size(247, 169);
            this.paymentCanvas.TabIndex = 1;
            // 
            // categoryPanel
            // 
            this.categoryPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.categoryPanel.Controls.Add(this.lblCategoryTitle);
            this.categoryPanel.Controls.Add(this.categoryCanvas);
            this.categoryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.categoryPanel.Location = new System.Drawing.Point(874, 220);
            this.categoryPanel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.categoryPanel.Name = "categoryPanel";
            this.categoryPanel.Size = new System.Drawing.Size(258, 191);
            this.categoryPanel.TabIndex = 5;
            // 
            // lblCategoryTitle
            // 
            this.lblCategoryTitle.AutoSize = true;
            this.lblCategoryTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblCategoryTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblCategoryTitle.Location = new System.Drawing.Point(8, 1);
            this.lblCategoryTitle.Name = "lblCategoryTitle";
            this.lblCategoryTitle.Size = new System.Drawing.Size(121, 15);
            this.lblCategoryTitle.TabIndex = 0;
            this.lblCategoryTitle.Text = "Purchase by Category";
            // 
            // categoryCanvas
            // 
            this.categoryCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.categoryCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.categoryCanvas.Location = new System.Drawing.Point(5, 19);
            this.categoryCanvas.Name = "categoryCanvas";
            this.categoryCanvas.Size = new System.Drawing.Size(250, 169);
            this.categoryCanvas.TabIndex = 1;
            // 
            // FrmPurchaseAnalytics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.ClientSize = new System.Drawing.Size(1180, 650);
            this.Controls.Add(this.mainLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmPurchaseAnalytics";
            this.Text = "Purchase Analytics";
            this.mainLayout.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.metricsLayout.ResumeLayout(false);
            this.cardTotalPurchase.ResumeLayout(false);
            this.cardTotalPurchase.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconPurchase)).EndInit();
            this.cardTotalVendors.ResumeLayout(false);
            this.cardTotalVendors.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconVendors)).EndInit();
            this.cardAveragePurchase.ResumeLayout(false);
            this.cardAveragePurchase.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconAveragePurchase)).EndInit();
            this.cardItemsPurchased.ResumeLayout(false);
            this.cardItemsPurchased.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconItemsPurchased)).EndInit();
            this.contentLayout.ResumeLayout(false);
            this.trendPanel.ResumeLayout(false);
            this.trendPanel.PerformLayout();
            this.topQtyPanel.ResumeLayout(false);
            this.topQtyPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTopQty)).EndInit();
            this.topAmountPanel.ResumeLayout(false);
            this.topAmountPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTopAmount)).EndInit();
            this.briefPanel.ResumeLayout(false);
            this.briefPanel.PerformLayout();
            this.paymentPanel.ResumeLayout(false);
            this.paymentPanel.PerformLayout();
            this.categoryPanel.ResumeLayout(false);
            this.categoryPanel.PerformLayout();
            this.ResumeLayout(false);

        }

    }
}
