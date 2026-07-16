namespace PosBranch_Win.Transaction
{
    partial class frmSalesInvoice
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
            this.components = new System.ComponentModel.Container();
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            Infragistics.Win.UltraWinGrid.UltraGridBand ultraGridBand1 = new Infragistics.Win.UltraWinGrid.UltraGridBand("Band 0", -1);
            Infragistics.Win.UltraWinGrid.UltraGridColumn ultraGridColumn1 = new Infragistics.Win.UltraWinGrid.UltraGridColumn("ItemId", -1, null, 0, Infragistics.Win.UltraWinGrid.SortIndicator.Ascending, false);
            Infragistics.Win.UltraWinGrid.UltraGridColumn ultraGridColumn2 = new Infragistics.Win.UltraWinGrid.UltraGridColumn("BarCode", 0);
            Infragistics.Win.UltraWinGrid.UltraGridColumn ultraGridColumn3 = new Infragistics.Win.UltraWinGrid.UltraGridColumn("Description", 1);
            Infragistics.Win.UltraWinGrid.UltraGridColumn ultraGridColumn4 = new Infragistics.Win.UltraWinGrid.UltraGridColumn("Unit", 2);
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
            Infragistics.Win.Appearance appearance21 = new Infragistics.Win.Appearance();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSalesInvoice));
            Infragistics.Win.Appearance appearance22 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance23 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance24 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance25 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance26 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance27 = new Infragistics.Win.Appearance();
            Infragistics.Win.UltraWinDataSource.UltraDataColumn ultraDataColumn1 = new Infragistics.Win.UltraWinDataSource.UltraDataColumn("No");
            Infragistics.Win.UltraWinDataSource.UltraDataColumn ultraDataColumn2 = new Infragistics.Win.UltraWinDataSource.UltraDataColumn("Item No");
            Infragistics.Win.UltraWinDataSource.UltraDataColumn ultraDataColumn3 = new Infragistics.Win.UltraWinDataSource.UltraDataColumn("Description");
            Infragistics.Win.UltraWinDataSource.UltraDataColumn ultraDataColumn4 = new Infragistics.Win.UltraWinDataSource.UltraDataColumn("UOM");
            Infragistics.Win.UltraWinDataSource.UltraDataColumn ultraDataColumn5 = new Infragistics.Win.UltraWinDataSource.UltraDataColumn("Qty");
            Infragistics.Win.UltraWinDataSource.UltraDataColumn ultraDataColumn6 = new Infragistics.Win.UltraWinDataSource.UltraDataColumn("Unit Price");
            Infragistics.Win.UltraWinDataSource.UltraDataColumn ultraDataColumn7 = new Infragistics.Win.UltraWinDataSource.UltraDataColumn("s/p");
            Infragistics.Win.UltraWinDataSource.UltraDataColumn ultraDataColumn8 = new Infragistics.Win.UltraWinDataSource.UltraDataColumn("s/p (Tax)");
            Infragistics.Win.UltraWinDataSource.UltraDataColumn ultraDataColumn9 = new Infragistics.Win.UltraWinDataSource.UltraDataColumn("Amount(Tax)");
            Infragistics.Win.UltraWinDataSource.UltraDataColumn ultraDataColumn10 = new Infragistics.Win.UltraWinDataSource.UltraDataColumn("BarCode");
            this.pnlBody = new Infragistics.Win.Misc.UltraPanel();
            this.frmSalesInvoice_Fill_Panel = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanel4 = new Infragistics.Win.Misc.UltraPanel();
            this.button5 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtBarcode = new System.Windows.Forms.TextBox();
            this.pbxBill = new Infragistics.Win.Misc.UltraPanel();
            this.pnlItem = new Infragistics.Win.Misc.UltraPanel();
            this.ChkSearch = new System.Windows.Forms.CheckBox();
            this.txtItemNameSearch = new System.Windows.Forms.TextBox();
            this.dgvitemlist = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanel3 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraLabel4 = new Infragistics.Win.Misc.UltraLabel();
            this.ultraLabel3 = new Infragistics.Win.Misc.UltraLabel();
            this.ultraLabel2 = new Infragistics.Win.Misc.UltraLabel();
            this.ultraLabel1 = new Infragistics.Win.Misc.UltraLabel();
            this.lblSave = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPictureBox3 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.pbxExit = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.ultraPictureBox2 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.ultraPictureBox1 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.pbxSave = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.dgvItems = new System.Windows.Forms.DataGridView();
            this.ultraPanel1 = new Infragistics.Win.Misc.UltraPanel();
            this.lblBillNo = new System.Windows.Forms.Label();
            this.lblledger = new System.Windows.Forms.Label();
            this.cmbPaymt = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.txtSalesPerson = new System.Windows.Forms.TextBox();
            this.txtCustomer = new System.Windows.Forms.TextBox();
            this.ultraPanel2 = new Infragistics.Win.Misc.UltraPanel();
            this.cmpPrice = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.txtNetTotal = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtDisc = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtSubtotal = new System.Windows.Forms.TextBox();
            this.lblsub = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.ultraDataSource1 = new Infragistics.Win.UltraWinDataSource.UltraDataSource(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ultraDataSource2 = new Infragistics.Win.UltraWinDataSource.UltraDataSource(this.components);
            this.ultraDataSource3 = new Infragistics.Win.UltraWinDataSource.UltraDataSource(this.components);
            this.pnlBody.ClientArea.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.frmSalesInvoice_Fill_Panel.ClientArea.SuspendLayout();
            this.frmSalesInvoice_Fill_Panel.SuspendLayout();
            this.ultraPanel4.ClientArea.SuspendLayout();
            this.ultraPanel4.SuspendLayout();
            this.pbxBill.ClientArea.SuspendLayout();
            this.pbxBill.SuspendLayout();
            this.pnlItem.ClientArea.SuspendLayout();
            this.pnlItem.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvitemlist)).BeginInit();
            this.ultraPanel3.ClientArea.SuspendLayout();
            this.ultraPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).BeginInit();
            this.ultraPanel1.ClientArea.SuspendLayout();
            this.ultraPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymt)).BeginInit();
            this.ultraPanel2.ClientArea.SuspendLayout();
            this.ultraPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmpPrice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNetTotal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDataSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDataSource2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDataSource3)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlBody
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(238)))), ((int)(((byte)(250)))));
            appearance1.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            appearance1.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            appearance1.BackColorDisabled2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            appearance1.BackGradientStyle = Infragistics.Win.GradientStyle.HorizontalWithGlassTop50;
            appearance1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            appearance1.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlBody.Appearance = appearance1;
            // 
            // pnlBody.ClientArea
            // 
            this.pnlBody.ClientArea.Controls.Add(this.frmSalesInvoice_Fill_Panel);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 0);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Size = new System.Drawing.Size(1170, 520);
            this.pnlBody.TabIndex = 0;
            this.pnlBody.PaintClient += new System.Windows.Forms.PaintEventHandler(this.ultraPanel1_PaintClient);
            // 
            // frmSalesInvoice_Fill_Panel
            // 
            this.frmSalesInvoice_Fill_Panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // frmSalesInvoice_Fill_Panel.ClientArea
            // 
            this.frmSalesInvoice_Fill_Panel.ClientArea.Controls.Add(this.ultraPanel4);
            this.frmSalesInvoice_Fill_Panel.ClientArea.Controls.Add(this.pbxBill);
            this.frmSalesInvoice_Fill_Panel.ClientArea.Controls.Add(this.ultraPanel1);
            this.frmSalesInvoice_Fill_Panel.ClientArea.Controls.Add(this.ultraPanel2);
            this.frmSalesInvoice_Fill_Panel.Cursor = System.Windows.Forms.Cursors.Default;
            this.frmSalesInvoice_Fill_Panel.Location = new System.Drawing.Point(0, 0);
            this.frmSalesInvoice_Fill_Panel.Name = "frmSalesInvoice_Fill_Panel";
            this.frmSalesInvoice_Fill_Panel.Size = new System.Drawing.Size(1170, 520);
            this.frmSalesInvoice_Fill_Panel.TabIndex = 1;
            // 
            // ultraPanel4
            // 
            this.ultraPanel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // ultraPanel4.ClientArea
            // 
            this.ultraPanel4.ClientArea.Controls.Add(this.button5);
            this.ultraPanel4.ClientArea.Controls.Add(this.label6);
            this.ultraPanel4.ClientArea.Controls.Add(this.txtBarcode);
            this.ultraPanel4.Location = new System.Drawing.Point(5, 91);
            this.ultraPanel4.Name = "ultraPanel4";
            this.ultraPanel4.Size = new System.Drawing.Size(1143, 36);
            this.ultraPanel4.TabIndex = 16;
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.Color.White;
            this.button5.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button5.Location = new System.Drawing.Point(447, 4);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(34, 27);
            this.button5.TabIndex = 22;
            this.button5.Text = "F7";
            this.button5.UseVisualStyleBackColor = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(3, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(92, 25);
            this.label6.TabIndex = 20;
            this.label6.Text = "Barcode";
            // 
            // txtBarcode
            // 
            this.txtBarcode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtBarcode.Font = new System.Drawing.Font("Microsoft Sans Serif", 17F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBarcode.Location = new System.Drawing.Point(98, 5);
            this.txtBarcode.Name = "txtBarcode";
            this.txtBarcode.Size = new System.Drawing.Size(383, 26);
            this.txtBarcode.TabIndex = 24;
            this.txtBarcode.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.txtBarcode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            this.txtBarcode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            this.txtBarcode.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyUp);
            // 
            // pbxBill
            // 
            this.pbxBill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // pbxBill.ClientArea
            // 
            this.pbxBill.ClientArea.Controls.Add(this.pnlItem);
            this.pbxBill.ClientArea.Controls.Add(this.ultraPanel3);
            this.pbxBill.ClientArea.Controls.Add(this.dgvItems);
            this.pbxBill.Location = new System.Drawing.Point(6, 130);
            this.pbxBill.Name = "pbxBill";
            this.pbxBill.Size = new System.Drawing.Size(1142, 258);
            this.pbxBill.TabIndex = 15;
            // 
            // pnlItem
            // 
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            appearance2.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            appearance2.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.pnlItem.Appearance = appearance2;
            this.pnlItem.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1Etched;
            // 
            // pnlItem.ClientArea
            // 
            this.pnlItem.ClientArea.Controls.Add(this.ChkSearch);
            this.pnlItem.ClientArea.Controls.Add(this.txtItemNameSearch);
            this.pnlItem.ClientArea.Controls.Add(this.dgvitemlist);
            this.pnlItem.Location = new System.Drawing.Point(127, 7);
            this.pnlItem.Name = "pnlItem";
            this.pnlItem.Size = new System.Drawing.Size(900, 251);
            this.pnlItem.TabIndex = 1;
            // 
            // ChkSearch
            // 
            this.ChkSearch.AutoSize = true;
            this.ChkSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChkSearch.Font = new System.Drawing.Font("MV Boli", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ChkSearch.Location = new System.Drawing.Point(412, 21);
            this.ChkSearch.Name = "ChkSearch";
            this.ChkSearch.Size = new System.Drawing.Size(134, 20);
            this.ChkSearch.TabIndex = 23;
            this.ChkSearch.Text = "Search with Name";
            this.ChkSearch.UseVisualStyleBackColor = true;
            this.ChkSearch.CheckedChanged += new System.EventHandler(this.ChkSearch_CheckedChanged);
            // 
            // txtItemNameSearch
            // 
            this.txtItemNameSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtItemNameSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtItemNameSearch.Location = new System.Drawing.Point(4, 18);
            this.txtItemNameSearch.Name = "txtItemNameSearch";
            this.txtItemNameSearch.Size = new System.Drawing.Size(402, 27);
            this.txtItemNameSearch.TabIndex = 1;
            this.txtItemNameSearch.TextChanged += new System.EventHandler(this.txtItemNameSearch_TextChanged);
            this.txtItemNameSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtItemNameSearch_KeyDown);
            // 
            // dgvitemlist
            // 
            appearance3.BackColor = System.Drawing.SystemColors.Window;
            appearance3.BorderColor = System.Drawing.SystemColors.InactiveCaption;
            this.dgvitemlist.DisplayLayout.Appearance = appearance3;
            ultraGridColumn1.Header.VisiblePosition = 0;
            ultraGridColumn1.RowLayoutColumnInfo.PreferredCellSize = new System.Drawing.Size(96, 23);
            ultraGridColumn2.Header.VisiblePosition = 1;
            ultraGridColumn2.RowLayoutColumnInfo.PreferredCellSize = new System.Drawing.Size(196, 23);
            ultraGridColumn3.Header.VisiblePosition = 2;
            ultraGridColumn3.RowLayoutColumnInfo.PreferredCellSize = new System.Drawing.Size(306, 23);
            ultraGridColumn4.Header.VisiblePosition = 3;
            ultraGridColumn4.RowLayoutColumnInfo.PreferredCellSize = new System.Drawing.Size(119, 23);
            ultraGridBand1.Columns.AddRange(new object[] {
            ultraGridColumn1,
            ultraGridColumn2,
            ultraGridColumn3,
            ultraGridColumn4});
            ultraGridBand1.RowLayoutStyle = Infragistics.Win.UltraWinGrid.RowLayoutStyle.ColumnLayout;
            this.dgvitemlist.DisplayLayout.BandsSerializer.Add(ultraGridBand1);
            this.dgvitemlist.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.dgvitemlist.DisplayLayout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;
            appearance4.BackColor = System.Drawing.SystemColors.ActiveBorder;
            appearance4.BackColor2 = System.Drawing.SystemColors.ControlDark;
            appearance4.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance4.BorderColor = System.Drawing.SystemColors.Window;
            this.dgvitemlist.DisplayLayout.GroupByBox.Appearance = appearance4;
            appearance5.ForeColor = System.Drawing.SystemColors.GrayText;
            this.dgvitemlist.DisplayLayout.GroupByBox.BandLabelAppearance = appearance5;
            this.dgvitemlist.DisplayLayout.GroupByBox.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            appearance6.BackColor = System.Drawing.SystemColors.ControlLightLight;
            appearance6.BackColor2 = System.Drawing.SystemColors.Control;
            appearance6.BackGradientStyle = Infragistics.Win.GradientStyle.Horizontal;
            appearance6.ForeColor = System.Drawing.SystemColors.GrayText;
            this.dgvitemlist.DisplayLayout.GroupByBox.PromptAppearance = appearance6;
            this.dgvitemlist.DisplayLayout.MaxColScrollRegions = 1;
            this.dgvitemlist.DisplayLayout.MaxRowScrollRegions = 1;
            appearance7.BackColor = System.Drawing.SystemColors.Window;
            appearance7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dgvitemlist.DisplayLayout.Override.ActiveCellAppearance = appearance7;
            appearance8.BackColor = System.Drawing.SystemColors.Highlight;
            appearance8.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgvitemlist.DisplayLayout.Override.ActiveRowAppearance = appearance8;
            this.dgvitemlist.DisplayLayout.Override.AllowRowFiltering = Infragistics.Win.DefaultableBoolean.True;
            this.dgvitemlist.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Dotted;
            this.dgvitemlist.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Dotted;
            appearance9.BackColor = System.Drawing.SystemColors.Window;
            this.dgvitemlist.DisplayLayout.Override.CardAreaAppearance = appearance9;
            appearance10.BorderColor = System.Drawing.Color.Silver;
            appearance10.TextTrimming = Infragistics.Win.TextTrimming.EllipsisCharacter;
            this.dgvitemlist.DisplayLayout.Override.CellAppearance = appearance10;
            this.dgvitemlist.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;
            this.dgvitemlist.DisplayLayout.Override.CellPadding = 0;
            this.dgvitemlist.DisplayLayout.Override.FilterUIType = Infragistics.Win.UltraWinGrid.FilterUIType.FilterRow;
            appearance11.BackColor = System.Drawing.SystemColors.Control;
            appearance11.BackColor2 = System.Drawing.SystemColors.ControlDark;
            appearance11.BackGradientAlignment = Infragistics.Win.GradientAlignment.Element;
            appearance11.BackGradientStyle = Infragistics.Win.GradientStyle.Horizontal;
            appearance11.BorderColor = System.Drawing.SystemColors.Window;
            this.dgvitemlist.DisplayLayout.Override.GroupByRowAppearance = appearance11;
            appearance12.TextHAlignAsString = "Left";
            this.dgvitemlist.DisplayLayout.Override.HeaderAppearance = appearance12;
            this.dgvitemlist.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortMulti;
            this.dgvitemlist.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
            appearance13.BackColor = System.Drawing.SystemColors.Window;
            appearance13.BorderColor = System.Drawing.Color.Silver;
            this.dgvitemlist.DisplayLayout.Override.RowAppearance = appearance13;
            this.dgvitemlist.DisplayLayout.Override.RowFilterAction = Infragistics.Win.UltraWinGrid.RowFilterAction.AppearancesOnly;
            this.dgvitemlist.DisplayLayout.Override.RowFilterMode = Infragistics.Win.UltraWinGrid.RowFilterMode.AllRowsInBand;
            this.dgvitemlist.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.False;
            appearance14.BackColor = System.Drawing.SystemColors.ControlLight;
            this.dgvitemlist.DisplayLayout.Override.TemplateAddRowAppearance = appearance14;
            this.dgvitemlist.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
            this.dgvitemlist.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;
            this.dgvitemlist.DisplayLayout.ViewStyleBand = Infragistics.Win.UltraWinGrid.ViewStyleBand.OutlookGroupBy;
            this.dgvitemlist.Location = new System.Drawing.Point(2, 51);
            this.dgvitemlist.Name = "dgvitemlist";
            this.dgvitemlist.Size = new System.Drawing.Size(888, 184);
            this.dgvitemlist.TabIndex = 0;
            this.dgvitemlist.InitializeLayout += new Infragistics.Win.UltraWinGrid.InitializeLayoutEventHandler(this.dgvitemlist_InitializeLayout);
            this.dgvitemlist.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.dgvitemlist_KeyPress);
            // 
            // ultraPanel3
            // 
            this.ultraPanel3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            appearance15.BorderColor = System.Drawing.Color.Silver;
            appearance15.BorderColor2 = System.Drawing.Color.Silver;
            appearance15.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ultraPanel3.Appearance = appearance15;
            this.ultraPanel3.BorderStyle = Infragistics.Win.UIElementBorderStyle.InsetSoft;
            // 
            // ultraPanel3.ClientArea
            // 
            this.ultraPanel3.ClientArea.Controls.Add(this.ultraLabel4);
            this.ultraPanel3.ClientArea.Controls.Add(this.ultraLabel3);
            this.ultraPanel3.ClientArea.Controls.Add(this.ultraLabel2);
            this.ultraPanel3.ClientArea.Controls.Add(this.ultraLabel1);
            this.ultraPanel3.ClientArea.Controls.Add(this.lblSave);
            this.ultraPanel3.ClientArea.Controls.Add(this.ultraPictureBox3);
            this.ultraPanel3.ClientArea.Controls.Add(this.pbxExit);
            this.ultraPanel3.ClientArea.Controls.Add(this.ultraPictureBox2);
            this.ultraPanel3.ClientArea.Controls.Add(this.ultraPictureBox1);
            this.ultraPanel3.ClientArea.Controls.Add(this.pbxSave);
            this.ultraPanel3.Location = new System.Drawing.Point(934, 7);
            this.ultraPanel3.Name = "ultraPanel3";
            this.ultraPanel3.Size = new System.Drawing.Size(200, 246);
            this.ultraPanel3.TabIndex = 2;
            // 
            // ultraLabel4
            // 
            appearance16.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance16.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance16.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            appearance16.TextHAlignAsString = "Center";
            this.ultraLabel4.Appearance = appearance16;
            this.ultraLabel4.BorderStyleInner = Infragistics.Win.UIElementBorderStyle.WindowsVista;
            this.ultraLabel4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabel4.Location = new System.Drawing.Point(70, 133);
            this.ultraLabel4.Name = "ultraLabel4";
            this.ultraLabel4.Size = new System.Drawing.Size(55, 13);
            this.ultraLabel4.TabIndex = 21;
            this.ultraLabel4.Text = "Ctrl+L";
            // 
            // ultraLabel3
            // 
            appearance17.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance17.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance17.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            appearance17.TextHAlignAsString = "Center";
            this.ultraLabel3.Appearance = appearance17;
            this.ultraLabel3.BorderStyleInner = Infragistics.Win.UIElementBorderStyle.WindowsVista;
            this.ultraLabel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabel3.Location = new System.Drawing.Point(5, 133);
            this.ultraLabel3.Name = "ultraLabel3";
            this.ultraLabel3.Size = new System.Drawing.Size(51, 13);
            this.ultraLabel3.TabIndex = 20;
            this.ultraLabel3.Text = "F4";
            // 
            // ultraLabel2
            // 
            appearance18.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance18.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance18.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            appearance18.TextHAlignAsString = "Center";
            this.ultraLabel2.Appearance = appearance18;
            this.ultraLabel2.BorderStyleInner = Infragistics.Win.UIElementBorderStyle.WindowsVista;
            this.ultraLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabel2.Location = new System.Drawing.Point(140, 58);
            this.ultraLabel2.Name = "ultraLabel2";
            this.ultraLabel2.Size = new System.Drawing.Size(48, 13);
            this.ultraLabel2.TabIndex = 19;
            this.ultraLabel2.Text = "F12";
            // 
            // ultraLabel1
            // 
            appearance19.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance19.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance19.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            appearance19.TextHAlignAsString = "Center";
            this.ultraLabel1.Appearance = appearance19;
            this.ultraLabel1.BorderStyleInner = Infragistics.Win.UIElementBorderStyle.WindowsVista;
            this.ultraLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabel1.Location = new System.Drawing.Point(72, 57);
            this.ultraLabel1.Name = "ultraLabel1";
            this.ultraLabel1.Size = new System.Drawing.Size(53, 13);
            this.ultraLabel1.TabIndex = 18;
            this.ultraLabel1.Text = "F1";
            // 
            // lblSave
            // 
            appearance20.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance20.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance20.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            appearance20.TextHAlignAsString = "Center";
            this.lblSave.Appearance = appearance20;
            this.lblSave.BorderStyleInner = Infragistics.Win.UIElementBorderStyle.WindowsVista;
            this.lblSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSave.Location = new System.Drawing.Point(5, 57);
            this.lblSave.Name = "lblSave";
            this.lblSave.Size = new System.Drawing.Size(51, 14);
            this.lblSave.TabIndex = 17;
            this.lblSave.Text = "F8";
            // 
            // ultraPictureBox3
            // 
            appearance21.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            appearance21.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance21.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.ultraPictureBox3.Appearance = appearance21;
            this.ultraPictureBox3.BorderShadowColor = System.Drawing.Color.Empty;
            this.ultraPictureBox3.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1Etched;
            this.ultraPictureBox3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox3.Image = ((object)(resources.GetObject("ultraPictureBox3.Image")));
            this.ultraPictureBox3.Location = new System.Drawing.Point(70, 81);
            this.ultraPictureBox3.Name = "ultraPictureBox3";
            this.ultraPictureBox3.Size = new System.Drawing.Size(57, 56);
            this.ultraPictureBox3.TabIndex = 16;
            this.ultraPictureBox3.Click += new System.EventHandler(this.ultraPictureBox3_Click);
            this.ultraPictureBox3.MouseHover += new System.EventHandler(this.ultraPictureBox3_MouseHover);
            // 
            // pbxExit
            // 
            appearance22.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            appearance22.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance22.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.pbxExit.Appearance = appearance22;
            this.pbxExit.BorderShadowColor = System.Drawing.Color.Empty;
            this.pbxExit.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1Etched;
            this.pbxExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbxExit.Image = ((object)(resources.GetObject("pbxExit.Image")));
            this.pbxExit.Location = new System.Drawing.Point(4, 81);
            this.pbxExit.Name = "pbxExit";
            this.pbxExit.Size = new System.Drawing.Size(53, 56);
            this.pbxExit.TabIndex = 15;
            this.pbxExit.Click += new System.EventHandler(this.pbxExit_Click);
            this.pbxExit.MouseHover += new System.EventHandler(this.pbxExit_MouseHover);
            // 
            // ultraPictureBox2
            // 
            appearance23.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            appearance23.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance23.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.ultraPictureBox2.Appearance = appearance23;
            this.ultraPictureBox2.BorderShadowColor = System.Drawing.Color.Empty;
            this.ultraPictureBox2.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1Etched;
            this.ultraPictureBox2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox2.Image = ((object)(resources.GetObject("ultraPictureBox2.Image")));
            this.ultraPictureBox2.Location = new System.Drawing.Point(140, 4);
            this.ultraPictureBox2.Name = "ultraPictureBox2";
            this.ultraPictureBox2.Size = new System.Drawing.Size(50, 56);
            this.ultraPictureBox2.TabIndex = 14;
            this.ultraPictureBox2.MouseHover += new System.EventHandler(this.ultraPictureBox2_MouseHover);
            // 
            // ultraPictureBox1
            // 
            appearance24.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            appearance24.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance24.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.ultraPictureBox1.Appearance = appearance24;
            this.ultraPictureBox1.BorderShadowColor = System.Drawing.Color.Empty;
            this.ultraPictureBox1.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1Etched;
            this.ultraPictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox1.Image = ((object)(resources.GetObject("ultraPictureBox1.Image")));
            this.ultraPictureBox1.Location = new System.Drawing.Point(70, 3);
            this.ultraPictureBox1.Name = "ultraPictureBox1";
            this.ultraPictureBox1.Size = new System.Drawing.Size(55, 55);
            this.ultraPictureBox1.TabIndex = 13;
            this.ultraPictureBox1.Click += new System.EventHandler(this.ultraPictureBox1_Click);
            this.ultraPictureBox1.MouseHover += new System.EventHandler(this.ultraPictureBox1_MouseHover);
            // 
            // pbxSave
            // 
            appearance25.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            appearance25.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            appearance25.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.pbxSave.Appearance = appearance25;
            this.pbxSave.BorderShadowColor = System.Drawing.Color.Empty;
            this.pbxSave.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1Etched;
            this.pbxSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbxSave.Image = ((object)(resources.GetObject("pbxSave.Image")));
            this.pbxSave.Location = new System.Drawing.Point(4, 4);
            this.pbxSave.Name = "pbxSave";
            this.pbxSave.Size = new System.Drawing.Size(53, 56);
            this.pbxSave.TabIndex = 12;
            this.pbxSave.Click += new System.EventHandler(this.pbxSave_Click);
            this.pbxSave.MouseHover += new System.EventHandler(this.pbxSave_MouseHover);
            // 
            // dgvItems
            // 
            this.dgvItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvItems.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            this.dgvItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvItems.Location = new System.Drawing.Point(2, 4);
            this.dgvItems.Name = "dgvItems";
            this.dgvItems.Size = new System.Drawing.Size(924, 249);
            this.dgvItems.TabIndex = 0;
            this.dgvItems.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvItems_CellClick);
            this.dgvItems.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvItems_CellValueChanged);
            this.dgvItems.Enter += new System.EventHandler(this.dgvItems_Enter);
            this.dgvItems.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvItems_KeyDown);
            this.dgvItems.KeyUp += new System.Windows.Forms.KeyEventHandler(this.dgvItems_KeyUp);
            // 
            // ultraPanel1
            // 
            this.ultraPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraPanel1.BorderStyle = Infragistics.Win.UIElementBorderStyle.InsetSoft;
            // 
            // ultraPanel1.ClientArea
            // 
            this.ultraPanel1.ClientArea.Controls.Add(this.lblBillNo);
            this.ultraPanel1.ClientArea.Controls.Add(this.lblledger);
            this.ultraPanel1.ClientArea.Controls.Add(this.cmbPaymt);
            this.ultraPanel1.ClientArea.Controls.Add(this.label5);
            this.ultraPanel1.ClientArea.Controls.Add(this.label4);
            this.ultraPanel1.ClientArea.Controls.Add(this.label3);
            this.ultraPanel1.ClientArea.Controls.Add(this.button2);
            this.ultraPanel1.ClientArea.Controls.Add(this.button1);
            this.ultraPanel1.ClientArea.Controls.Add(this.txtSalesPerson);
            this.ultraPanel1.ClientArea.Controls.Add(this.txtCustomer);
            this.ultraPanel1.Location = new System.Drawing.Point(4, 23);
            this.ultraPanel1.Name = "ultraPanel1";
            this.ultraPanel1.Size = new System.Drawing.Size(1142, 65);
            this.ultraPanel1.TabIndex = 14;
            // 
            // lblBillNo
            // 
            this.lblBillNo.AutoSize = true;
            this.lblBillNo.Location = new System.Drawing.Point(149, 5);
            this.lblBillNo.Name = "lblBillNo";
            this.lblBillNo.Size = new System.Drawing.Size(32, 13);
            this.lblBillNo.TabIndex = 28;
            this.lblBillNo.Text = "Billno";
            // 
            // lblledger
            // 
            this.lblledger.AutoSize = true;
            this.lblledger.Location = new System.Drawing.Point(84, 5);
            this.lblledger.Name = "lblledger";
            this.lblledger.Size = new System.Drawing.Size(59, 13);
            this.lblledger.TabIndex = 27;
            this.lblledger.Text = "lblLedgerId";
            // 
            // cmbPaymt
            // 
            this.cmbPaymt.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.VisualStudio2005;
            this.cmbPaymt.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbPaymt.Location = new System.Drawing.Point(593, 24);
            this.cmbPaymt.Name = "cmbPaymt";
            this.cmbPaymt.Size = new System.Drawing.Size(211, 33);
            this.cmbPaymt.TabIndex = 26;
            this.cmbPaymt.ValueChanged += new System.EventHandler(this.cmbPaymt_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(590, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 17);
            this.label5.TabIndex = 18;
            this.label5.Text = "Payment Term";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(307, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 17);
            this.label4.TabIndex = 17;
            this.label4.Text = "SalesPerson";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(1, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 17);
            this.label3.TabIndex = 16;
            this.label3.Text = "Customer";
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.White;
            this.button2.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(537, 24);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(40, 34);
            this.button2.TabIndex = 15;
            this.button2.Text = "F6";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.White;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(254, 24);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(40, 34);
            this.button1.TabIndex = 14;
            this.button1.Text = "F11";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // txtSalesPerson
            // 
            this.txtSalesPerson.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSalesPerson.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSalesPerson.Location = new System.Drawing.Point(310, 24);
            this.txtSalesPerson.Multiline = true;
            this.txtSalesPerson.Name = "txtSalesPerson";
            this.txtSalesPerson.Size = new System.Drawing.Size(267, 34);
            this.txtSalesPerson.TabIndex = 12;
            // 
            // txtCustomer
            // 
            this.txtCustomer.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCustomer.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCustomer.Location = new System.Drawing.Point(4, 24);
            this.txtCustomer.Multiline = true;
            this.txtCustomer.Name = "txtCustomer";
            this.txtCustomer.Size = new System.Drawing.Size(290, 34);
            this.txtCustomer.TabIndex = 11;
            // 
            // ultraPanel2
            // 
            this.ultraPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            appearance26.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            appearance26.BorderColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            appearance26.BorderColor3DBase = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ultraPanel2.Appearance = appearance26;
            this.ultraPanel2.BorderStyle = Infragistics.Win.UIElementBorderStyle.Dashed;
            // 
            // ultraPanel2.ClientArea
            // 
            this.ultraPanel2.ClientArea.Controls.Add(this.cmpPrice);
            this.ultraPanel2.ClientArea.Controls.Add(this.txtNetTotal);
            this.ultraPanel2.ClientArea.Controls.Add(this.textBox8);
            this.ultraPanel2.ClientArea.Controls.Add(this.label8);
            this.ultraPanel2.ClientArea.Controls.Add(this.txtDisc);
            this.ultraPanel2.ClientArea.Controls.Add(this.label7);
            this.ultraPanel2.ClientArea.Controls.Add(this.txtSubtotal);
            this.ultraPanel2.ClientArea.Controls.Add(this.lblsub);
            this.ultraPanel2.ClientArea.Controls.Add(this.label2);
            this.ultraPanel2.ClientArea.Controls.Add(this.button4);
            this.ultraPanel2.ClientArea.Controls.Add(this.textBox5);
            this.ultraPanel2.ClientArea.Controls.Add(this.label1);
            this.ultraPanel2.ClientArea.Controls.Add(this.button3);
            this.ultraPanel2.ClientArea.Controls.Add(this.textBox4);
            this.ultraPanel2.Location = new System.Drawing.Point(7, 395);
            this.ultraPanel2.Name = "ultraPanel2";
            this.ultraPanel2.Size = new System.Drawing.Size(1145, 123);
            this.ultraPanel2.TabIndex = 13;
            // 
            // cmpPrice
            // 
            this.cmpPrice.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.VisualStudio2005;
            this.cmpPrice.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpPrice.Location = new System.Drawing.Point(275, 27);
            this.cmpPrice.Name = "cmpPrice";
            this.cmpPrice.Size = new System.Drawing.Size(144, 31);
            this.cmpPrice.TabIndex = 25;
            // 
            // txtNetTotal
            // 
            this.txtNetTotal.Anchor = System.Windows.Forms.AnchorStyles.Right;
            appearance27.BackColor = System.Drawing.Color.Maroon;
            appearance27.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            appearance27.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance27.TextHAlignAsString = "Right";
            this.txtNetTotal.Appearance = appearance27;
            this.txtNetTotal.BackColor = System.Drawing.Color.Maroon;
            this.txtNetTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNetTotal.Location = new System.Drawing.Point(873, 56);
            this.txtNetTotal.Multiline = true;
            this.txtNetTotal.Name = "txtNetTotal";
            this.txtNetTotal.ReadOnly = true;
            this.txtNetTotal.Size = new System.Drawing.Size(266, 69);
            this.txtNetTotal.TabIndex = 24;
            // 
            // textBox8
            // 
            this.textBox8.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.textBox8.Location = new System.Drawing.Point(1065, 4);
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(74, 20);
            this.textBox8.TabIndex = 22;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(993, 4);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 16);
            this.label8.TabIndex = 21;
            this.label8.Text = "Rounding";
            // 
            // txtDisc
            // 
            this.txtDisc.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.txtDisc.Location = new System.Drawing.Point(873, 31);
            this.txtDisc.Name = "txtDisc";
            this.txtDisc.Size = new System.Drawing.Size(107, 20);
            this.txtDisc.TabIndex = 20;
            this.txtDisc.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(791, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 16);
            this.label7.TabIndex = 19;
            this.label7.Text = "Disc";
            // 
            // txtSubtotal
            // 
            this.txtSubtotal.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.txtSubtotal.Location = new System.Drawing.Point(873, 3);
            this.txtSubtotal.Name = "txtSubtotal";
            this.txtSubtotal.ReadOnly = true;
            this.txtSubtotal.Size = new System.Drawing.Size(107, 20);
            this.txtSubtotal.TabIndex = 18;
            this.txtSubtotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblsub
            // 
            this.lblsub.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblsub.AutoSize = true;
            this.lblsub.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblsub.Location = new System.Drawing.Point(791, 3);
            this.lblsub.Name = "lblsub";
            this.lblsub.Size = new System.Drawing.Size(75, 16);
            this.lblsub.TabIndex = 17;
            this.lblsub.Text = "Sub Total";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(272, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Price Level";
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.White;
            this.button4.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.Location = new System.Drawing.Point(179, 64);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(60, 34);
            this.button4.TabIndex = 14;
            this.button4.Text = "#BillNO";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // textBox5
            // 
            this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox5.Location = new System.Drawing.Point(2, 64);
            this.textBox5.Multiline = true;
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(217, 34);
            this.textBox5.TabIndex = 13;
            this.textBox5.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox5_KeyUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Sales Order";
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.White;
            this.button3.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Location = new System.Drawing.Point(179, 24);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(40, 34);
            this.button3.TabIndex = 11;
            this.button3.Text = "F5";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // textBox4
            // 
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox4.Location = new System.Drawing.Point(2, 24);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(217, 34);
            this.textBox4.TabIndex = 10;
            // 
            // ultraDataSource1
            // 
            ultraDataColumn6.ReadOnly = Infragistics.Win.DefaultableBoolean.False;
            this.ultraDataSource1.Band.Columns.AddRange(new object[] {
            ultraDataColumn1,
            ultraDataColumn2,
            ultraDataColumn3,
            ultraDataColumn4,
            ultraDataColumn5,
            ultraDataColumn6,
            ultraDataColumn7,
            ultraDataColumn8,
            ultraDataColumn9});
            this.ultraDataSource1.Band.Key = "Items";
            // 
            // ultraDataSource2
            // 
            this.ultraDataSource2.Band.Columns.AddRange(new object[] {
            ultraDataColumn10});
            // 
            // frmSalesInvoice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1170, 520);
            this.Controls.Add(this.pnlBody);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmSalesInvoice";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmSalesInvoice";
            this.Load += new System.EventHandler(this.frmSalesInvoice_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmSalesInvoice_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frmSalesInvoice_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.frmSalesInvoice_KeyUp);
            this.pnlBody.ClientArea.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            this.frmSalesInvoice_Fill_Panel.ClientArea.ResumeLayout(false);
            this.frmSalesInvoice_Fill_Panel.ResumeLayout(false);
            this.ultraPanel4.ClientArea.ResumeLayout(false);
            this.ultraPanel4.ClientArea.PerformLayout();
            this.ultraPanel4.ResumeLayout(false);
            this.pbxBill.ClientArea.ResumeLayout(false);
            this.pbxBill.ResumeLayout(false);
            this.pnlItem.ClientArea.ResumeLayout(false);
            this.pnlItem.ClientArea.PerformLayout();
            this.pnlItem.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvitemlist)).EndInit();
            this.ultraPanel3.ClientArea.ResumeLayout(false);
            this.ultraPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).EndInit();
            this.ultraPanel1.ClientArea.ResumeLayout(false);
            this.ultraPanel1.ClientArea.PerformLayout();
            this.ultraPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymt)).EndInit();
            this.ultraPanel2.ClientArea.ResumeLayout(false);
            this.ultraPanel2.ClientArea.PerformLayout();
            this.ultraPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmpPrice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNetTotal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDataSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDataSource2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDataSource3)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel pnlBody;
        private Infragistics.Win.Misc.UltraPanel frmSalesInvoice_Fill_Panel;
        private Infragistics.Win.Misc.UltraPanel ultraPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox textBox5;
        private Infragistics.Win.Misc.UltraPanel pbxBill;
        private Infragistics.Win.Misc.UltraPanel ultraPanel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtSalesPerson;
        private Infragistics.Win.UltraWinDataSource.UltraDataSource ultraDataSource1;
        private Infragistics.Win.Misc.UltraPanel ultraPanel4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtDisc;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblsub;
        public System.Windows.Forms.TextBox txtCustomer;
        public System.Windows.Forms.DataGridView dgvItems;
        private Infragistics.Win.Misc.UltraPanel pnlItem;
        private Infragistics.Win.UltraWinGrid.UltraGrid dgvitemlist;
        private Infragistics.Win.Misc.UltraPanel ultraPanel3;
        private Infragistics.Win.Misc.UltraLabel ultraLabel4;
        private Infragistics.Win.Misc.UltraLabel ultraLabel3;
        private Infragistics.Win.Misc.UltraLabel ultraLabel2;
        private Infragistics.Win.Misc.UltraLabel ultraLabel1;
        private Infragistics.Win.Misc.UltraLabel lblSave;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox3;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox pbxExit;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox2;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox1;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox pbxSave;
        private System.Windows.Forms.ToolTip toolTip1;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbPaymt;
        public Infragistics.Win.UltraWinEditors.UltraComboEditor cmpPrice;
        public Infragistics.Win.UltraWinEditors.UltraTextEditor txtNetTotal;
        public System.Windows.Forms.TextBox txtSubtotal;
        private System.Windows.Forms.CheckBox ChkSearch;
        public System.Windows.Forms.Label lblledger;
        private System.Windows.Forms.TextBox txtItemNameSearch;
        private Infragistics.Win.UltraWinDataSource.UltraDataSource ultraDataSource2;
        private Infragistics.Win.UltraWinDataSource.UltraDataSource ultraDataSource3;
        public System.Windows.Forms.Label lblBillNo;
        public System.Windows.Forms.TextBox txtBarcode;
    }
}