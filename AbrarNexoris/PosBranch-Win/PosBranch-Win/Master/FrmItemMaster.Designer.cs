
namespace PosBranch_Win.Master
{
    partial class FrmItemMaster
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
            this.pnl_itemmaster = new System.Windows.Forms.Panel();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.add_tab = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_itemname = new System.Windows.Forms.Label();
            this.cb_brand = new System.Windows.Forms.ComboBox();
            this.cb_baseunit = new System.Windows.Forms.ComboBox();
            this.lbl_localname = new System.Windows.Forms.Label();
            this.lbl_brand = new System.Windows.Forms.Label();
            this.txt_itemname = new System.Windows.Forms.TextBox();
            this.lbl_category = new System.Windows.Forms.Label();
            this.txt_localname = new System.Windows.Forms.TextBox();
            this.lbl_group = new System.Windows.Forms.Label();
            this.lbl_itemtype = new System.Windows.Forms.Label();
            this.cb_category = new System.Windows.Forms.ComboBox();
            this.lbl_availablestock = new System.Windows.Forms.Label();
            this.cb_itemtype = new System.Windows.Forms.ComboBox();
            this.lbl_orderdstock = new System.Windows.Forms.Label();
            this.cb_group = new System.Windows.Forms.ComboBox();
            this.lbl_custtype = new System.Windows.Forms.Label();
            this.lbl_unitcost = new System.Windows.Forms.Label();
            this.cb_custtype = new System.Windows.Forms.ComboBox();
            this.lbl_baseunit = new System.Windows.Forms.Label();
            this.txt_unitcost = new System.Windows.Forms.TextBox();
            this.txt_orderedstock = new System.Windows.Forms.TextBox();
            this.txt_availablestock = new System.Windows.Forms.TextBox();
            this.pnl_btngroup = new System.Windows.Forms.Panel();
            this.btn_update = new System.Windows.Forms.Button();
            this.btn_close = new System.Windows.Forms.Button();
            this.btn_clear = new System.Windows.Forms.Button();
            this.btn_save = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tab_uom = new System.Windows.Forms.TabPage();
            this.dgv_uomtab = new System.Windows.Forms.DataGridView();
            this.tab_price = new System.Windows.Forms.TabPage();
            this.dgv_pricetab = new System.Windows.Forms.DataGridView();
            this.tab_images = new System.Windows.Forms.TabPage();
            this.dgv_Imagetab = new System.Windows.Forms.DataGridView();
            this.tab_batch = new System.Windows.Forms.TabPage();
            this.tab_stock = new System.Windows.Forms.TabPage();
            this.tab_vendors = new System.Windows.Forms.TabPage();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.list_tab = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_searchDescription = new System.Windows.Forms.Label();
            this.txt_searchBarcode = new System.Windows.Forms.TextBox();
            this.lbl_searchBarcode = new System.Windows.Forms.Label();
            this.txt_searchDescription = new System.Windows.Forms.TextBox();
            this.dgv_list_itemmaster = new System.Windows.Forms.DataGridView();
            this.lbl_addgroup = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.pnl_itemmaster.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.add_tab.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.pnl_btngroup.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tab_uom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_uomtab)).BeginInit();
            this.tab_price.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_pricetab)).BeginInit();
            this.tab_images.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Imagetab)).BeginInit();
            this.list_tab.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_list_itemmaster)).BeginInit();
            this.SuspendLayout();
            // 
            // pnl_itemmaster
            // 
            this.pnl_itemmaster.BackColor = System.Drawing.Color.LightBlue;
            this.pnl_itemmaster.Controls.Add(this.tabControl2);
            this.pnl_itemmaster.Controls.Add(this.lbl_addgroup);
            this.pnl_itemmaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl_itemmaster.Location = new System.Drawing.Point(0, 0);
            this.pnl_itemmaster.Name = "pnl_itemmaster";
            this.pnl_itemmaster.Size = new System.Drawing.Size(943, 486);
            this.pnl_itemmaster.TabIndex = 1;
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl2.Controls.Add(this.add_tab);
            this.tabControl2.Controls.Add(this.list_tab);
            this.tabControl2.Location = new System.Drawing.Point(15, 42);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(915, 429);
            this.tabControl2.TabIndex = 12;
            this.tabControl2.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // add_tab
            // 
            this.add_tab.Controls.Add(this.tableLayoutPanel1);
            this.add_tab.Controls.Add(this.pnl_btngroup);
            this.add_tab.Controls.Add(this.tabControl1);
            this.add_tab.Controls.Add(this.label11);
            this.add_tab.Controls.Add(this.label10);
            this.add_tab.Controls.Add(this.label9);
            this.add_tab.Controls.Add(this.label8);
            this.add_tab.Controls.Add(this.label7);
            this.add_tab.Controls.Add(this.label6);
            this.add_tab.Controls.Add(this.label5);
            this.add_tab.Controls.Add(this.label4);
            this.add_tab.Controls.Add(this.label3);
            this.add_tab.Controls.Add(this.label2);
            this.add_tab.Controls.Add(this.label1);
            this.add_tab.Location = new System.Drawing.Point(4, 22);
            this.add_tab.Name = "add_tab";
            this.add_tab.Padding = new System.Windows.Forms.Padding(3);
            this.add_tab.Size = new System.Drawing.Size(907, 403);
            this.add_tab.TabIndex = 0;
            this.add_tab.Text = "Add New";
            this.add_tab.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.tableLayoutPanel1.ColumnCount = 10;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.653846F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 91.34615F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 119F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 98F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 118F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel1.Controls.Add(this.lbl_itemname, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cb_brand, 6, 2);
            this.tableLayoutPanel1.Controls.Add(this.cb_baseunit, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.lbl_localname, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbl_brand, 5, 2);
            this.tableLayoutPanel1.Controls.Add(this.txt_itemname, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbl_category, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.txt_localname, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbl_group, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lbl_itemtype, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.cb_category, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.lbl_availablestock, 7, 1);
            this.tableLayoutPanel1.Controls.Add(this.cb_itemtype, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbl_orderdstock, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.cb_group, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.lbl_custtype, 7, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbl_unitcost, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.cb_custtype, 8, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbl_baseunit, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.txt_unitcost, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.txt_orderedstock, 6, 1);
            this.tableLayoutPanel1.Controls.Add(this.txt_availablestock, 8, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 49.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(887, 100);
            this.tableLayoutPanel1.TabIndex = 41;
            // 
            // lbl_itemname
            // 
            this.lbl_itemname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_itemname.AutoSize = true;
            this.lbl_itemname.Location = new System.Drawing.Point(10, 9);
            this.lbl_itemname.Name = "lbl_itemname";
            this.lbl_itemname.Size = new System.Drawing.Size(75, 13);
            this.lbl_itemname.TabIndex = 29;
            this.lbl_itemname.Text = "ItemName";
            // 
            // cb_brand
            // 
            this.cb_brand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_brand.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cb_brand.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cb_brand.FormattingEnabled = true;
            this.cb_brand.Location = new System.Drawing.Point(525, 71);
            this.cb_brand.Name = "cb_brand";
            this.cb_brand.Size = new System.Drawing.Size(112, 21);
            this.cb_brand.TabIndex = 39;
            // 
            // cb_baseunit
            // 
            this.cb_baseunit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_baseunit.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cb_baseunit.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cb_baseunit.FormattingEnabled = true;
            this.cb_baseunit.Location = new System.Drawing.Point(91, 36);
            this.cb_baseunit.Name = "cb_baseunit";
            this.cb_baseunit.Size = new System.Drawing.Size(113, 21);
            this.cb_baseunit.TabIndex = 40;
            this.cb_baseunit.SelectedIndexChanged += new System.EventHandler(this.cb_baseunit_SelectedIndexChanged);
            this.cb_baseunit.SelectionChangeCommitted += new System.EventHandler(this.cb_baseunit_SelectionChangeCommitted);
            this.cb_baseunit.Click += new System.EventHandler(this.cb_baseunit_Click);
            this.cb_baseunit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cb_baseunit_KeyDown);
            // 
            // lbl_localname
            // 
            this.lbl_localname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_localname.AutoSize = true;
            this.lbl_localname.Location = new System.Drawing.Point(210, 9);
            this.lbl_localname.Name = "lbl_localname";
            this.lbl_localname.Size = new System.Drawing.Size(94, 13);
            this.lbl_localname.TabIndex = 28;
            this.lbl_localname.Text = "Local Name";
            // 
            // lbl_brand
            // 
            this.lbl_brand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_brand.AutoSize = true;
            this.lbl_brand.Location = new System.Drawing.Point(427, 75);
            this.lbl_brand.Name = "lbl_brand";
            this.lbl_brand.Size = new System.Drawing.Size(92, 13);
            this.lbl_brand.TabIndex = 37;
            this.lbl_brand.Text = "Brand";
            // 
            // txt_itemname
            // 
            this.txt_itemname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_itemname.Location = new System.Drawing.Point(91, 5);
            this.txt_itemname.Name = "txt_itemname";
            this.txt_itemname.Size = new System.Drawing.Size(113, 20);
            this.txt_itemname.TabIndex = 28;
            // 
            // lbl_category
            // 
            this.lbl_category.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_category.AutoSize = true;
            this.lbl_category.Location = new System.Drawing.Point(210, 75);
            this.lbl_category.Name = "lbl_category";
            this.lbl_category.Size = new System.Drawing.Size(94, 13);
            this.lbl_category.TabIndex = 36;
            this.lbl_category.Text = "Category";
            // 
            // txt_localname
            // 
            this.txt_localname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_localname.Location = new System.Drawing.Point(310, 5);
            this.txt_localname.Name = "txt_localname";
            this.txt_localname.Size = new System.Drawing.Size(111, 20);
            this.txt_localname.TabIndex = 26;
            // 
            // lbl_group
            // 
            this.lbl_group.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_group.AutoSize = true;
            this.lbl_group.Location = new System.Drawing.Point(10, 75);
            this.lbl_group.Name = "lbl_group";
            this.lbl_group.Size = new System.Drawing.Size(75, 13);
            this.lbl_group.TabIndex = 35;
            this.lbl_group.Text = "Group";
            // 
            // lbl_itemtype
            // 
            this.lbl_itemtype.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_itemtype.AutoSize = true;
            this.lbl_itemtype.Location = new System.Drawing.Point(427, 9);
            this.lbl_itemtype.Name = "lbl_itemtype";
            this.lbl_itemtype.Size = new System.Drawing.Size(92, 13);
            this.lbl_itemtype.TabIndex = 29;
            this.lbl_itemtype.Text = "Item Type";
            // 
            // cb_category
            // 
            this.cb_category.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_category.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cb_category.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cb_category.FormattingEnabled = true;
            this.cb_category.Location = new System.Drawing.Point(310, 71);
            this.cb_category.Name = "cb_category";
            this.cb_category.Size = new System.Drawing.Size(111, 21);
            this.cb_category.TabIndex = 19;
            // 
            // lbl_availablestock
            // 
            this.lbl_availablestock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_availablestock.AutoSize = true;
            this.lbl_availablestock.Location = new System.Drawing.Point(643, 40);
            this.lbl_availablestock.Name = "lbl_availablestock";
            this.lbl_availablestock.Size = new System.Drawing.Size(99, 13);
            this.lbl_availablestock.TabIndex = 34;
            this.lbl_availablestock.Text = "Available Stock";
            // 
            // cb_itemtype
            // 
            this.cb_itemtype.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_itemtype.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cb_itemtype.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cb_itemtype.FormattingEnabled = true;
            this.cb_itemtype.Location = new System.Drawing.Point(525, 5);
            this.cb_itemtype.Name = "cb_itemtype";
            this.cb_itemtype.Size = new System.Drawing.Size(112, 21);
            this.cb_itemtype.TabIndex = 8;
            // 
            // lbl_orderdstock
            // 
            this.lbl_orderdstock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_orderdstock.AutoSize = true;
            this.lbl_orderdstock.Location = new System.Drawing.Point(427, 40);
            this.lbl_orderdstock.Name = "lbl_orderdstock";
            this.lbl_orderdstock.Size = new System.Drawing.Size(92, 13);
            this.lbl_orderdstock.TabIndex = 33;
            this.lbl_orderdstock.Text = "Orderd Stock";
            // 
            // cb_group
            // 
            this.cb_group.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_group.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cb_group.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cb_group.FormattingEnabled = true;
            this.cb_group.Location = new System.Drawing.Point(91, 71);
            this.cb_group.Name = "cb_group";
            this.cb_group.Size = new System.Drawing.Size(113, 21);
            this.cb_group.TabIndex = 18;
            // 
            // lbl_custtype
            // 
            this.lbl_custtype.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_custtype.AutoSize = true;
            this.lbl_custtype.Location = new System.Drawing.Point(643, 9);
            this.lbl_custtype.Name = "lbl_custtype";
            this.lbl_custtype.Size = new System.Drawing.Size(99, 13);
            this.lbl_custtype.TabIndex = 30;
            this.lbl_custtype.Text = "Customer Type";
            // 
            // lbl_unitcost
            // 
            this.lbl_unitcost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_unitcost.AutoSize = true;
            this.lbl_unitcost.Location = new System.Drawing.Point(210, 40);
            this.lbl_unitcost.Name = "lbl_unitcost";
            this.lbl_unitcost.Size = new System.Drawing.Size(94, 13);
            this.lbl_unitcost.TabIndex = 32;
            this.lbl_unitcost.Text = "Unit Cost";
            // 
            // cb_custtype
            // 
            this.cb_custtype.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_custtype.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cb_custtype.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cb_custtype.FormattingEnabled = true;
            this.cb_custtype.Location = new System.Drawing.Point(748, 5);
            this.cb_custtype.Name = "cb_custtype";
            this.cb_custtype.Size = new System.Drawing.Size(111, 21);
            this.cb_custtype.TabIndex = 9;
            // 
            // lbl_baseunit
            // 
            this.lbl_baseunit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_baseunit.AutoSize = true;
            this.lbl_baseunit.Location = new System.Drawing.Point(10, 40);
            this.lbl_baseunit.Name = "lbl_baseunit";
            this.lbl_baseunit.Size = new System.Drawing.Size(75, 13);
            this.lbl_baseunit.TabIndex = 31;
            this.lbl_baseunit.Text = "Base Unit";
            // 
            // txt_unitcost
            // 
            this.txt_unitcost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_unitcost.Location = new System.Drawing.Point(310, 37);
            this.txt_unitcost.Name = "txt_unitcost";
            this.txt_unitcost.Size = new System.Drawing.Size(111, 20);
            this.txt_unitcost.TabIndex = 11;
            this.txt_unitcost.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_unitcost_KeyDown);
            // 
            // txt_orderedstock
            // 
            this.txt_orderedstock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_orderedstock.Location = new System.Drawing.Point(525, 37);
            this.txt_orderedstock.Name = "txt_orderedstock";
            this.txt_orderedstock.Size = new System.Drawing.Size(112, 20);
            this.txt_orderedstock.TabIndex = 12;
            // 
            // txt_availablestock
            // 
            this.txt_availablestock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_availablestock.Location = new System.Drawing.Point(748, 37);
            this.txt_availablestock.Name = "txt_availablestock";
            this.txt_availablestock.Size = new System.Drawing.Size(111, 20);
            this.txt_availablestock.TabIndex = 13;
            // 
            // pnl_btngroup
            // 
            this.pnl_btngroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_btngroup.Controls.Add(this.btn_update);
            this.pnl_btngroup.Controls.Add(this.btn_close);
            this.pnl_btngroup.Controls.Add(this.btn_clear);
            this.pnl_btngroup.Controls.Add(this.btn_save);
            this.pnl_btngroup.Location = new System.Drawing.Point(304, 344);
            this.pnl_btngroup.Name = "pnl_btngroup";
            this.pnl_btngroup.Size = new System.Drawing.Size(288, 49);
            this.pnl_btngroup.TabIndex = 38;
            // 
            // btn_update
            // 
            this.btn_update.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btn_update.ForeColor = System.Drawing.Color.White;
            this.btn_update.Location = new System.Drawing.Point(26, 9);
            this.btn_update.Name = "btn_update";
            this.btn_update.Size = new System.Drawing.Size(78, 32);
            this.btn_update.TabIndex = 11;
            this.btn_update.Text = "Update";
            this.btn_update.UseVisualStyleBackColor = false;
            this.btn_update.Click += new System.EventHandler(this.btn_update_Click);
            // 
            // btn_close
            // 
            this.btn_close.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btn_close.ForeColor = System.Drawing.Color.White;
            this.btn_close.Location = new System.Drawing.Point(188, 9);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(78, 32);
            this.btn_close.TabIndex = 10;
            this.btn_close.Text = "Close";
            this.btn_close.UseVisualStyleBackColor = false;
            // 
            // btn_clear
            // 
            this.btn_clear.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btn_clear.ForeColor = System.Drawing.Color.White;
            this.btn_clear.Location = new System.Drawing.Point(107, 9);
            this.btn_clear.Name = "btn_clear";
            this.btn_clear.Size = new System.Drawing.Size(78, 32);
            this.btn_clear.TabIndex = 9;
            this.btn_clear.Text = "Clear";
            this.btn_clear.UseVisualStyleBackColor = false;
            this.btn_clear.Click += new System.EventHandler(this.btn_clear_Click);
            // 
            // btn_save
            // 
            this.btn_save.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btn_save.ForeColor = System.Drawing.Color.White;
            this.btn_save.Location = new System.Drawing.Point(26, 9);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(78, 32);
            this.btn_save.TabIndex = 8;
            this.btn_save.Text = "Save";
            this.btn_save.UseVisualStyleBackColor = false;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tab_uom);
            this.tabControl1.Controls.Add(this.tab_price);
            this.tabControl1.Controls.Add(this.tab_images);
            this.tabControl1.Controls.Add(this.tab_batch);
            this.tabControl1.Controls.Add(this.tab_stock);
            this.tabControl1.Controls.Add(this.tab_vendors);
            this.tabControl1.Location = new System.Drawing.Point(6, 151);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(894, 196);
            this.tabControl1.TabIndex = 24;
            // 
            // tab_uom
            // 
            this.tab_uom.Controls.Add(this.dgv_uomtab);
            this.tab_uom.Location = new System.Drawing.Point(4, 22);
            this.tab_uom.Name = "tab_uom";
            this.tab_uom.Padding = new System.Windows.Forms.Padding(3);
            this.tab_uom.Size = new System.Drawing.Size(886, 170);
            this.tab_uom.TabIndex = 0;
            this.tab_uom.Text = "UOM";
            this.tab_uom.UseVisualStyleBackColor = true;
            // 
            // dgv_uomtab
            // 
            this.dgv_uomtab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_uomtab.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_uomtab.Location = new System.Drawing.Point(3, 3);
            this.dgv_uomtab.Name = "dgv_uomtab";
            this.dgv_uomtab.Size = new System.Drawing.Size(880, 164);
            this.dgv_uomtab.TabIndex = 0;
            this.dgv_uomtab.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_add_itemmaster_CellContentClick);
            this.dgv_uomtab.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_uomtab_CellValueChanged);
            // 
            // tab_price
            // 
            this.tab_price.Controls.Add(this.dgv_pricetab);
            this.tab_price.Location = new System.Drawing.Point(4, 22);
            this.tab_price.Name = "tab_price";
            this.tab_price.Padding = new System.Windows.Forms.Padding(3);
            this.tab_price.Size = new System.Drawing.Size(886, 170);
            this.tab_price.TabIndex = 1;
            this.tab_price.Text = "Price";
            this.tab_price.UseVisualStyleBackColor = true;
            // 
            // dgv_pricetab
            // 
            this.dgv_pricetab.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_pricetab.Location = new System.Drawing.Point(6, 6);
            this.dgv_pricetab.Name = "dgv_pricetab";
            this.dgv_pricetab.Size = new System.Drawing.Size(874, 158);
            this.dgv_pricetab.TabIndex = 0;
            this.dgv_pricetab.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_pricetab_CellContentClick);
            this.dgv_pricetab.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_pricetab_CellEndEdit);
            // 
            // tab_images
            // 
            this.tab_images.Controls.Add(this.dgv_Imagetab);
            this.tab_images.Location = new System.Drawing.Point(4, 22);
            this.tab_images.Name = "tab_images";
            this.tab_images.Size = new System.Drawing.Size(886, 170);
            this.tab_images.TabIndex = 3;
            this.tab_images.Text = "Images";
            this.tab_images.UseVisualStyleBackColor = true;
            // 
            // dgv_Imagetab
            // 
            this.dgv_Imagetab.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgv_Imagetab.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Imagetab.Location = new System.Drawing.Point(3, 3);
            this.dgv_Imagetab.Name = "dgv_Imagetab";
            this.dgv_Imagetab.Size = new System.Drawing.Size(880, 164);
            this.dgv_Imagetab.TabIndex = 0;
            this.dgv_Imagetab.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_Imagetab_CellContentClick);
            this.dgv_Imagetab.KeyUp += new System.Windows.Forms.KeyEventHandler(this.dgv_Imagetab_KeyUp);
            // 
            // tab_batch
            // 
            this.tab_batch.Location = new System.Drawing.Point(4, 22);
            this.tab_batch.Name = "tab_batch";
            this.tab_batch.Size = new System.Drawing.Size(886, 170);
            this.tab_batch.TabIndex = 4;
            this.tab_batch.Text = "Batch";
            this.tab_batch.UseVisualStyleBackColor = true;
            // 
            // tab_stock
            // 
            this.tab_stock.Location = new System.Drawing.Point(4, 22);
            this.tab_stock.Name = "tab_stock";
            this.tab_stock.Size = new System.Drawing.Size(886, 170);
            this.tab_stock.TabIndex = 5;
            this.tab_stock.Text = "Stock";
            this.tab_stock.UseVisualStyleBackColor = true;
            // 
            // tab_vendors
            // 
            this.tab_vendors.Location = new System.Drawing.Point(4, 22);
            this.tab_vendors.Name = "tab_vendors";
            this.tab_vendors.Size = new System.Drawing.Size(886, 170);
            this.tab_vendors.TabIndex = 6;
            this.tab_vendors.Text = "Vendors";
            this.tab_vendors.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(358, 88);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(0, 13);
            this.label11.TabIndex = 23;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(198, 85);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(0, 13);
            this.label10.TabIndex = 22;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(39, 85);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(0, 13);
            this.label9.TabIndex = 21;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(516, 51);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 13);
            this.label8.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(343, 50);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(0, 13);
            this.label7.TabIndex = 16;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(201, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 13);
            this.label6.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(48, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(0, 13);
            this.label5.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(514, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 13);
            this.label4.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(357, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(0, 13);
            this.label3.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(187, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 13);
            this.label2.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 0;
            // 
            // list_tab
            // 
            this.list_tab.Controls.Add(this.tableLayoutPanel2);
            this.list_tab.Controls.Add(this.dgv_list_itemmaster);
            this.list_tab.Location = new System.Drawing.Point(4, 22);
            this.list_tab.Name = "list_tab";
            this.list_tab.Padding = new System.Windows.Forms.Padding(3);
            this.list_tab.Size = new System.Drawing.Size(907, 403);
            this.list_tab.TabIndex = 1;
            this.list_tab.Text = "List Items";
            this.list_tab.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 4.489796F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 95.5102F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 339F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 118F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 317F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel2.Controls.Add(this.lbl_searchDescription, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.txt_searchBarcode, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.lbl_searchBarcode, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.txt_searchDescription, 2, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(11, 13);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(890, 41);
            this.tableLayoutPanel2.TabIndex = 30;
            // 
            // lbl_searchDescription
            // 
            this.lbl_searchDescription.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lbl_searchDescription.AutoSize = true;
            this.lbl_searchDescription.Location = new System.Drawing.Point(29, 7);
            this.lbl_searchDescription.Name = "lbl_searchDescription";
            this.lbl_searchDescription.Size = new System.Drawing.Size(60, 26);
            this.lbl_searchDescription.TabIndex = 28;
            this.lbl_searchDescription.Text = "Search Description";
            // 
            // txt_searchBarcode
            // 
            this.txt_searchBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_searchBarcode.Location = new System.Drawing.Point(552, 10);
            this.txt_searchBarcode.Name = "txt_searchBarcode";
            this.txt_searchBarcode.Size = new System.Drawing.Size(311, 20);
            this.txt_searchBarcode.TabIndex = 2;
            // 
            // lbl_searchBarcode
            // 
            this.lbl_searchBarcode.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lbl_searchBarcode.AutoSize = true;
            this.lbl_searchBarcode.Location = new System.Drawing.Point(462, 14);
            this.lbl_searchBarcode.Name = "lbl_searchBarcode";
            this.lbl_searchBarcode.Size = new System.Drawing.Size(84, 13);
            this.lbl_searchBarcode.TabIndex = 29;
            this.lbl_searchBarcode.Text = "Search Barcode";
            // 
            // txt_searchDescription
            // 
            this.txt_searchDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_searchDescription.Location = new System.Drawing.Point(95, 10);
            this.txt_searchDescription.Name = "txt_searchDescription";
            this.txt_searchDescription.Size = new System.Drawing.Size(333, 20);
            this.txt_searchDescription.TabIndex = 1;
            this.txt_searchDescription.TextChanged += new System.EventHandler(this.txt_searchDescription_TextChanged);
            // 
            // dgv_list_itemmaster
            // 
            this.dgv_list_itemmaster.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_list_itemmaster.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_list_itemmaster.Location = new System.Drawing.Point(11, 71);
            this.dgv_list_itemmaster.Name = "dgv_list_itemmaster";
            this.dgv_list_itemmaster.Size = new System.Drawing.Size(890, 320);
            this.dgv_list_itemmaster.TabIndex = 0;
            this.dgv_list_itemmaster.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_list_itemmaster_CellClick);
            // 
            // lbl_addgroup
            // 
            this.lbl_addgroup.AutoSize = true;
            this.lbl_addgroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_addgroup.Location = new System.Drawing.Point(11, 8);
            this.lbl_addgroup.Name = "lbl_addgroup";
            this.lbl_addgroup.Size = new System.Drawing.Size(105, 20);
            this.lbl_addgroup.TabIndex = 0;
            this.lbl_addgroup.Text = "Item Master";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // FrmItemMaster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(943, 486);
            this.Controls.Add(this.pnl_itemmaster);
            this.Name = "FrmItemMaster";
            this.Text = "FrmItemMaster";
            this.Load += new System.EventHandler(this.FrmItemMaster_Load);
            this.pnl_itemmaster.ResumeLayout(false);
            this.pnl_itemmaster.PerformLayout();
            this.tabControl2.ResumeLayout(false);
            this.add_tab.ResumeLayout(false);
            this.add_tab.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.pnl_btngroup.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tab_uom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_uomtab)).EndInit();
            this.tab_price.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_pricetab)).EndInit();
            this.tab_images.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Imagetab)).EndInit();
            this.list_tab.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_list_itemmaster)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnl_itemmaster;
        private System.Windows.Forms.Label lbl_addgroup;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage list_tab;
        private System.Windows.Forms.TabPage add_tab;
        private System.Windows.Forms.TextBox txt_localname;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tab_uom;
        private System.Windows.Forms.DataGridView dgv_uomtab;
        private System.Windows.Forms.TabPage tab_price;
        private System.Windows.Forms.TabPage tab_images;
        private System.Windows.Forms.TabPage tab_batch;
        private System.Windows.Forms.TabPage tab_stock;
        private System.Windows.Forms.TabPage tab_vendors;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cb_category;
        private System.Windows.Forms.ComboBox cb_group;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txt_availablestock;
        private System.Windows.Forms.TextBox txt_orderedstock;
        private System.Windows.Forms.TextBox txt_unitcost;
        private System.Windows.Forms.ComboBox cb_custtype;
        private System.Windows.Forms.ComboBox cb_itemtype;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_brand;
        private System.Windows.Forms.Label lbl_category;
        private System.Windows.Forms.Label lbl_group;
        private System.Windows.Forms.Label lbl_availablestock;
        private System.Windows.Forms.Label lbl_orderdstock;
        private System.Windows.Forms.Label lbl_unitcost;
        private System.Windows.Forms.Label lbl_baseunit;
        private System.Windows.Forms.Label lbl_custtype;
        private System.Windows.Forms.Label lbl_itemtype;
        private System.Windows.Forms.Label lbl_localname;
        private System.Windows.Forms.Panel pnl_btngroup;
        private System.Windows.Forms.Button btn_update;
        private System.Windows.Forms.Button btn_close;
        private System.Windows.Forms.Button btn_clear;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.Label lbl_searchBarcode;
        private System.Windows.Forms.Label lbl_searchDescription;
        private System.Windows.Forms.TextBox txt_searchBarcode;
        private System.Windows.Forms.TextBox txt_searchDescription;
        private System.Windows.Forms.DataGridView dgv_list_itemmaster;
        private System.Windows.Forms.ComboBox cb_brand;
        private System.Windows.Forms.ComboBox cb_baseunit;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lbl_itemname;
        private System.Windows.Forms.TextBox txt_itemname;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.DataGridView dgv_pricetab;
        private System.Windows.Forms.DataGridView dgv_Imagetab;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}