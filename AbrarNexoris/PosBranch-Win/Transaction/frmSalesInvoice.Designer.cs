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
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                if (barcodeFocusTimer != null)
                {
                    barcodeFocusTimer.Stop();
                    barcodeFocusTimer.Dispose();
                }
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
            Infragistics.Win.Appearance appearance22 = new Infragistics.Win.Appearance();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSalesInvoice));
            Infragistics.Win.Appearance appearance23 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance24 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance25 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance26 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance27 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance28 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance29 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance30 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance31 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance32 = new Infragistics.Win.Appearance();
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
            this.pbxBill = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGrid1 = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.gridFooterPanel = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanel5 = new Infragistics.Win.Misc.UltraPanel();
            this.button5 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtBarcode = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraPanel1 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanel7 = new Infragistics.Win.Misc.UltraPanel();
            this.label21 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.lblBillNo = new System.Windows.Forms.Label();
            this.lblledger = new System.Windows.Forms.Label();
            this.cmbPaymt = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.txtCustomer = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.txtSalesPerson = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraPictureBox5 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.ultraPictureBox6 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.ultraPanel14 = new Infragistics.Win.Misc.UltraPanel();
            this.cmpPrice = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.label2 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.textBoxhold = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.textBox5 = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.textBoxround = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.txtSubtotal = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraCheckEditorApplyRounding = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
            this.ultraPictureBox7 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.ultraPanel8 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanel9 = new Infragistics.Win.Misc.UltraPanel();
            this.txtNetTotal = new Infragistics.Win.Misc.UltraLabel();
            this.label7 = new System.Windows.Forms.Label();
            this.lblsub = new System.Windows.Forms.Label();
            this.txtDisc = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraDataSource1 = new Infragistics.Win.UltraWinDataSource.UltraDataSource(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ultraDataSource2 = new Infragistics.Win.UltraWinDataSource.UltraDataSource(this.components);
            this.ultraDataSource3 = new Infragistics.Win.UltraWinDataSource.UltraDataSource(this.components);
            this.pnlBody.ClientArea.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.frmSalesInvoice_Fill_Panel.ClientArea.SuspendLayout();
            this.frmSalesInvoice_Fill_Panel.SuspendLayout();
            this.pbxBill.ClientArea.SuspendLayout();
            this.pbxBill.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGrid1)).BeginInit();
            this.gridFooterPanel.SuspendLayout();
            this.ultraPanel5.ClientArea.SuspendLayout();
            this.ultraPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtBarcode)).BeginInit();
            this.ultraPanel1.ClientArea.SuspendLayout();
            this.ultraPanel1.SuspendLayout();
            this.ultraPanel7.ClientArea.SuspendLayout();
            this.ultraPanel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCustomer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSalesPerson)).BeginInit();
            this.ultraPanel14.ClientArea.SuspendLayout();
            this.ultraPanel14.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmpPrice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxhold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxround)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSubtotal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraCheckEditorApplyRounding)).BeginInit();
            this.ultraPanel8.ClientArea.SuspendLayout();
            this.ultraPanel8.SuspendLayout();
            this.ultraPanel9.ClientArea.SuspendLayout();
            this.ultraPanel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtDisc)).BeginInit();
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
            this.pnlBody.Size = new System.Drawing.Size(1349, 520);
            this.pnlBody.TabIndex = 0;
            // 
            // frmSalesInvoice_Fill_Panel
            // 
            appearance2.BackColor = System.Drawing.Color.SkyBlue;
            this.frmSalesInvoice_Fill_Panel.Appearance = appearance2;
            // 
            // frmSalesInvoice_Fill_Panel.ClientArea
            // 
            this.frmSalesInvoice_Fill_Panel.ClientArea.Controls.Add(this.pbxBill);
            this.frmSalesInvoice_Fill_Panel.ClientArea.Controls.Add(this.ultraPanel5);
            this.frmSalesInvoice_Fill_Panel.ClientArea.Controls.Add(this.ultraPanel1);
            this.frmSalesInvoice_Fill_Panel.ClientArea.Controls.Add(this.ultraPanel14);
            this.frmSalesInvoice_Fill_Panel.Cursor = System.Windows.Forms.Cursors.Default;
            this.frmSalesInvoice_Fill_Panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.frmSalesInvoice_Fill_Panel.Location = new System.Drawing.Point(0, 0);
            this.frmSalesInvoice_Fill_Panel.Name = "frmSalesInvoice_Fill_Panel";
            this.frmSalesInvoice_Fill_Panel.Size = new System.Drawing.Size(1349, 520);
            this.frmSalesInvoice_Fill_Panel.TabIndex = 1;
            // 
            // pbxBill
            // 
            appearance3.BorderColor = System.Drawing.Color.DeepSkyBlue;
            this.pbxBill.Appearance = appearance3;
            this.pbxBill.BorderStyle = Infragistics.Win.UIElementBorderStyle.Etched;
            // 
            // pbxBill.ClientArea
            // 
            this.pbxBill.ClientArea.Controls.Add(this.ultraGrid1);
            this.pbxBill.ClientArea.Controls.Add(this.gridFooterPanel);
            this.pbxBill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbxBill.Location = new System.Drawing.Point(0, 129);
            this.pbxBill.Name = "pbxBill";
            this.pbxBill.Size = new System.Drawing.Size(1349, 271);
            this.pbxBill.TabIndex = 15;
            // 
            // ultraGrid1
            // 
            appearance4.BackColor = System.Drawing.SystemColors.Window;
            appearance4.BorderColor = System.Drawing.SystemColors.InactiveCaption;
            this.ultraGrid1.DisplayLayout.Appearance = appearance4;
            this.ultraGrid1.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraGrid1.DisplayLayout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;
            appearance5.BackColor = System.Drawing.SystemColors.ActiveBorder;
            appearance5.BackColor2 = System.Drawing.SystemColors.ControlDark;
            appearance5.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance5.BorderColor = System.Drawing.SystemColors.Window;
            this.ultraGrid1.DisplayLayout.GroupByBox.Appearance = appearance5;
            appearance6.ForeColor = System.Drawing.SystemColors.GrayText;
            this.ultraGrid1.DisplayLayout.GroupByBox.BandLabelAppearance = appearance6;
            this.ultraGrid1.DisplayLayout.GroupByBox.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
            appearance7.BackColor = System.Drawing.SystemColors.ControlLightLight;
            appearance7.BackColor2 = System.Drawing.SystemColors.Control;
            appearance7.BackGradientStyle = Infragistics.Win.GradientStyle.Horizontal;
            appearance7.ForeColor = System.Drawing.SystemColors.GrayText;
            this.ultraGrid1.DisplayLayout.GroupByBox.PromptAppearance = appearance7;
            this.ultraGrid1.DisplayLayout.MaxColScrollRegions = 1;
            this.ultraGrid1.DisplayLayout.MaxRowScrollRegions = 1;
            appearance8.BackColor = System.Drawing.SystemColors.Window;
            appearance8.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ultraGrid1.DisplayLayout.Override.ActiveCellAppearance = appearance8;
            appearance9.BackColor = System.Drawing.SystemColors.Highlight;
            appearance9.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.ultraGrid1.DisplayLayout.Override.ActiveRowAppearance = appearance9;
            this.ultraGrid1.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Dotted;
            this.ultraGrid1.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Dotted;
            appearance10.BackColor = System.Drawing.SystemColors.Window;
            this.ultraGrid1.DisplayLayout.Override.CardAreaAppearance = appearance10;
            appearance11.BorderColor = System.Drawing.Color.Silver;
            appearance11.TextTrimming = Infragistics.Win.TextTrimming.EllipsisCharacter;
            this.ultraGrid1.DisplayLayout.Override.CellAppearance = appearance11;
            this.ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;
            this.ultraGrid1.DisplayLayout.Override.CellPadding = 0;
            appearance12.BackColor = System.Drawing.SystemColors.Control;
            appearance12.BackColor2 = System.Drawing.SystemColors.ControlDark;
            appearance12.BackGradientAlignment = Infragistics.Win.GradientAlignment.Element;
            appearance12.BackGradientStyle = Infragistics.Win.GradientStyle.Horizontal;
            appearance12.BorderColor = System.Drawing.SystemColors.Window;
            this.ultraGrid1.DisplayLayout.Override.GroupByRowAppearance = appearance12;
            appearance13.BackColor = System.Drawing.Color.DodgerBlue;
            appearance13.FontData.BoldAsString = "True";
            appearance13.ForeColor = System.Drawing.Color.White;
            appearance13.TextHAlignAsString = "Center";
            this.ultraGrid1.DisplayLayout.Override.HeaderAppearance = appearance13;
            this.ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortMulti;
            this.ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
            appearance14.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(250)))), ((int)(((byte)(255)))));
            this.ultraGrid1.DisplayLayout.Override.RowAlternateAppearance = appearance14;
            appearance15.BackColor = System.Drawing.Color.White;
            appearance15.BorderColor = System.Drawing.Color.LightGray;
            this.ultraGrid1.DisplayLayout.Override.RowAppearance = appearance15;
            this.ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
            appearance16.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ultraGrid1.DisplayLayout.Override.TemplateAddRowAppearance = appearance16;
            this.ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
            this.ultraGrid1.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;
            this.ultraGrid1.DisplayLayout.ViewStyleBand = Infragistics.Win.UltraWinGrid.ViewStyleBand.OutlookGroupBy;
            this.ultraGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGrid1.Location = new System.Drawing.Point(0, 0);
            this.ultraGrid1.Name = "ultraGrid1";
            this.ultraGrid1.Size = new System.Drawing.Size(1345, 241);
            this.ultraGrid1.TabIndex = 3;
            this.ultraGrid1.Text = "ultraGrid1";
            this.ultraGrid1.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // gridFooterPanel
            // 
            appearance17.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            appearance17.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(184)))));
            appearance17.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance17.FontData.BoldAsString = "True";
            appearance17.ForeColor = System.Drawing.Color.White;
            this.gridFooterPanel.Appearance = appearance17;
            this.gridFooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gridFooterPanel.Location = new System.Drawing.Point(0, 241);
            this.gridFooterPanel.Name = "gridFooterPanel";
            this.gridFooterPanel.Size = new System.Drawing.Size(1345, 26);
            this.gridFooterPanel.TabIndex = 4;
            // 
            // ultraPanel5
            // 
            // 
            // ultraPanel5.ClientArea
            // 
            this.ultraPanel5.ClientArea.Controls.Add(this.button5);
            this.ultraPanel5.ClientArea.Controls.Add(this.label6);
            this.ultraPanel5.ClientArea.Controls.Add(this.txtBarcode);
            this.ultraPanel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanel5.Location = new System.Drawing.Point(0, 75);
            this.ultraPanel5.Name = "ultraPanel5";
            this.ultraPanel5.Size = new System.Drawing.Size(1349, 54);
            this.ultraPanel5.TabIndex = 37;
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.button5.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(182)))));
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button5.ForeColor = System.Drawing.Color.White;
            this.button5.Location = new System.Drawing.Point(468, 17);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(41, 22);
            this.button5.TabIndex = 36;
            this.button5.Text = "F7";
            this.button5.UseVisualStyleBackColor = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(25, 14);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(99, 25);
            this.label6.TabIndex = 35;
            this.label6.Text = "Barcode";
            // 
            // txtBarcode
            // 
            appearance18.BorderColor = System.Drawing.Color.DeepSkyBlue;
            this.txtBarcode.Appearance = appearance18;
            this.txtBarcode.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.txtBarcode.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.txtBarcode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBarcode.Location = new System.Drawing.Point(130, 15);
            this.txtBarcode.Name = "txtBarcode";
            this.txtBarcode.Size = new System.Drawing.Size(381, 26);
            this.txtBarcode.TabIndex = 37;
            this.txtBarcode.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.txtBarcode.TextChanged += new System.EventHandler(this.txtBarcode_TextChanged);
            this.txtBarcode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtBarcode_KeyDown);
            // 
            // ultraPanel1
            // 
            this.ultraPanel1.BorderStyle = Infragistics.Win.UIElementBorderStyle.InsetSoft;
            // 
            // ultraPanel1.ClientArea
            // 
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraPanel7);
            this.ultraPanel1.ClientArea.Controls.Add(this.lblBillNo);
            this.ultraPanel1.ClientArea.Controls.Add(this.lblledger);
            this.ultraPanel1.ClientArea.Controls.Add(this.cmbPaymt);
            this.ultraPanel1.ClientArea.Controls.Add(this.label5);
            this.ultraPanel1.ClientArea.Controls.Add(this.label4);
            this.ultraPanel1.ClientArea.Controls.Add(this.label3);
            this.ultraPanel1.ClientArea.Controls.Add(this.button2);
            this.ultraPanel1.ClientArea.Controls.Add(this.button1);
            this.ultraPanel1.ClientArea.Controls.Add(this.txtCustomer);
            this.ultraPanel1.ClientArea.Controls.Add(this.txtSalesPerson);
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraPictureBox5);
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraPictureBox6);
            this.ultraPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanel1.Location = new System.Drawing.Point(0, 0);
            this.ultraPanel1.Name = "ultraPanel1";
            this.ultraPanel1.Size = new System.Drawing.Size(1349, 75);
            this.ultraPanel1.TabIndex = 14;
            // 
            // ultraPanel7
            // 
            this.ultraPanel7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance19.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(47)))), ((int)(((byte)(73)))));
            appearance19.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(74)))), ((int)(((byte)(110)))));
            appearance19.BackGradientStyle = Infragistics.Win.GradientStyle.ForwardDiagonal;
            appearance19.BorderAlpha = Infragistics.Win.Alpha.Opaque;
            appearance19.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(132)))), ((int)(((byte)(199)))));
            this.ultraPanel7.Appearance = appearance19;
            this.ultraPanel7.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanel7.ClientArea
            // 
            this.ultraPanel7.ClientArea.Controls.Add(this.label21);
            this.ultraPanel7.ClientArea.Controls.Add(this.label20);
            this.ultraPanel7.ClientArea.Controls.Add(this.label19);
            this.ultraPanel7.ClientArea.Controls.Add(this.label18);
            this.ultraPanel7.ClientArea.Controls.Add(this.label17);
            this.ultraPanel7.ClientArea.Controls.Add(this.label16);
            this.ultraPanel7.Location = new System.Drawing.Point(1154, 2);
            this.ultraPanel7.Name = "ultraPanel7";
            this.ultraPanel7.Size = new System.Drawing.Size(190, 71);
            this.ultraPanel7.TabIndex = 29;
            // 
            // label21
            // 
            this.label21.BackColor = System.Drawing.Color.Transparent;
            this.label21.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(222)))), ((int)(((byte)(128)))));
            this.label21.Location = new System.Drawing.Point(75, 46);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(105, 21);
            this.label21.TabIndex = 5;
            this.label21.Text = "0.00";
            this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label20
            // 
            this.label20.BackColor = System.Drawing.Color.Transparent;
            this.label20.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.ForeColor = System.Drawing.Color.White;
            this.label20.Location = new System.Drawing.Point(75, 26);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(105, 17);
            this.label20.TabIndex = 4;
            this.label20.Text = "0.00";
            this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label19
            // 
            this.label19.BackColor = System.Drawing.Color.Transparent;
            this.label19.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.ForeColor = System.Drawing.Color.White;
            this.label19.Location = new System.Drawing.Point(75, 5);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(105, 17);
            this.label19.TabIndex = 3;
            this.label19.Text = "0.00";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.BackColor = System.Drawing.Color.Transparent;
            this.label18.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(242)))), ((int)(((byte)(254)))));
            this.label18.Location = new System.Drawing.Point(10, 48);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(58, 17);
            this.label18.TabIndex = 2;
            this.label18.Text = "Change:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.BackColor = System.Drawing.Color.Transparent;
            this.label17.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(230)))), ((int)(((byte)(253)))));
            this.label17.Location = new System.Drawing.Point(10, 26);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(39, 17);
            this.label17.TabIndex = 1;
            this.label17.Text = "Pymt:";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.BackColor = System.Drawing.Color.Transparent;
            this.label16.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(230)))), ((int)(((byte)(253)))));
            this.label16.Location = new System.Drawing.Point(10, 5);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(39, 17);
            this.label16.TabIndex = 0;
            this.label16.Text = "Total:";
            // 
            // lblBillNo
            // 
            this.lblBillNo.AutoSize = true;
            this.lblBillNo.Location = new System.Drawing.Point(179, 5);
            this.lblBillNo.Name = "lblBillNo";
            this.lblBillNo.Size = new System.Drawing.Size(32, 13);
            this.lblBillNo.TabIndex = 28;
            this.lblBillNo.Text = "Billno";
            // 
            // lblledger
            // 
            this.lblledger.AutoSize = true;
            this.lblledger.Location = new System.Drawing.Point(112, 5);
            this.lblledger.Name = "lblledger";
            this.lblledger.Size = new System.Drawing.Size(59, 13);
            this.lblledger.TabIndex = 27;
            this.lblledger.Text = "lblLedgerId";
            // 
            // cmbPaymt
            // 
            appearance20.BorderColor = System.Drawing.Color.DeepSkyBlue;
            this.cmbPaymt.Appearance = appearance20;
            this.cmbPaymt.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.cmbPaymt.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbPaymt.Location = new System.Drawing.Point(628, 26);
            this.cmbPaymt.Name = "cmbPaymt";
            this.cmbPaymt.Size = new System.Drawing.Size(187, 33);
            this.cmbPaymt.TabIndex = 26;
            this.cmbPaymt.ValueChanged += new System.EventHandler(this.cmbPaymt_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(625, 2);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(145, 24);
            this.label5.TabIndex = 18;
            this.label5.Text = "Payment Term";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(330, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(127, 24);
            this.label4.TabIndex = 17;
            this.label4.Text = "SalesPerson";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(16, 1);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 24);
            this.label3.TabIndex = 16;
            this.label3.Text = "Customer";
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.button2.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(182)))));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.Color.White;
            this.button2.Location = new System.Drawing.Point(556, 27);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(40, 31);
            this.button2.TabIndex = 15;
            this.button2.Text = "F6";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(182)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(263, 27);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(40, 31);
            this.button1.TabIndex = 14;
            this.button1.Text = "F11";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtCustomer
            // 
            appearance21.BorderColor = System.Drawing.Color.DeepSkyBlue;
            this.txtCustomer.Appearance = appearance21;
            this.txtCustomer.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.txtCustomer.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.txtCustomer.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCustomer.Location = new System.Drawing.Point(20, 25);
            this.txtCustomer.Multiline = true;
            this.txtCustomer.Name = "txtCustomer";
            this.txtCustomer.Size = new System.Drawing.Size(285, 35);
            this.txtCustomer.TabIndex = 32;
            this.txtCustomer.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // txtSalesPerson
            // 
            appearance22.BorderColor = System.Drawing.Color.DeepSkyBlue;
            this.txtSalesPerson.Appearance = appearance22;
            this.txtSalesPerson.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.txtSalesPerson.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.txtSalesPerson.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSalesPerson.Location = new System.Drawing.Point(334, 25);
            this.txtSalesPerson.Multiline = true;
            this.txtSalesPerson.Name = "txtSalesPerson";
            this.txtSalesPerson.Size = new System.Drawing.Size(264, 35);
            this.txtSalesPerson.TabIndex = 33;
            this.txtSalesPerson.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraPictureBox5
            // 
            this.ultraPictureBox5.BackColor = System.Drawing.Color.Transparent;
            this.ultraPictureBox5.BackColorInternal = System.Drawing.Color.Transparent;
            this.ultraPictureBox5.BorderShadowColor = System.Drawing.Color.Empty;
            this.ultraPictureBox5.Image = ((object)(resources.GetObject("ultraPictureBox5.Image")));
            this.ultraPictureBox5.Location = new System.Drawing.Point(836, 0);
            this.ultraPictureBox5.Name = "ultraPictureBox5";
            this.ultraPictureBox5.Size = new System.Drawing.Size(66, 70);
            this.ultraPictureBox5.TabIndex = 30;
            this.ultraPictureBox5.Click += new System.EventHandler(this.ultraPictureBox5_Click);
            // 
            // ultraPictureBox6
            // 
            this.ultraPictureBox6.BorderShadowColor = System.Drawing.Color.Empty;
            this.ultraPictureBox6.Image = ((object)(resources.GetObject("ultraPictureBox6.Image")));
            this.ultraPictureBox6.Location = new System.Drawing.Point(836, 0);
            this.ultraPictureBox6.Name = "ultraPictureBox6";
            this.ultraPictureBox6.Size = new System.Drawing.Size(67, 70);
            this.ultraPictureBox6.TabIndex = 31;
            // 
            // ultraPanel14
            // 
            // 
            // ultraPanel14.ClientArea
            // 
            this.ultraPanel14.ClientArea.Controls.Add(this.cmpPrice);
            this.ultraPanel14.ClientArea.Controls.Add(this.label2);
            this.ultraPanel14.ClientArea.Controls.Add(this.button4);
            this.ultraPanel14.ClientArea.Controls.Add(this.label1);
            this.ultraPanel14.ClientArea.Controls.Add(this.button3);
            this.ultraPanel14.ClientArea.Controls.Add(this.textBoxhold);
            this.ultraPanel14.ClientArea.Controls.Add(this.textBox5);
            this.ultraPanel14.ClientArea.Controls.Add(this.textBoxround);
            this.ultraPanel14.ClientArea.Controls.Add(this.txtSubtotal);
            this.ultraPanel14.ClientArea.Controls.Add(this.ultraCheckEditorApplyRounding);
            this.ultraPanel14.ClientArea.Controls.Add(this.ultraPictureBox7);
            this.ultraPanel14.ClientArea.Controls.Add(this.ultraPanel8);
            this.ultraPanel14.ClientArea.Controls.Add(this.label7);
            this.ultraPanel14.ClientArea.Controls.Add(this.lblsub);
            this.ultraPanel14.ClientArea.Controls.Add(this.txtDisc);
            this.ultraPanel14.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanel14.Location = new System.Drawing.Point(0, 400);
            this.ultraPanel14.Name = "ultraPanel14";
            this.ultraPanel14.Size = new System.Drawing.Size(1349, 120);
            this.ultraPanel14.TabIndex = 36;
            // 
            // cmpPrice
            // 
            appearance23.BorderColor = System.Drawing.Color.DeepSkyBlue;
            this.cmpPrice.Appearance = appearance23;
            this.cmpPrice.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.VisualStudio2005;
            this.cmpPrice.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpPrice.Location = new System.Drawing.Point(272, 32);
            this.cmpPrice.Name = "cmpPrice";
            this.cmpPrice.Size = new System.Drawing.Size(144, 31);
            this.cmpPrice.TabIndex = 87;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(271, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 86;
            this.label2.Text = "Price Level";
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.button4.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(182)))));
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.ForeColor = System.Drawing.Color.White;
            this.button4.Location = new System.Drawing.Point(152, 70);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(71, 31);
            this.button4.TabIndex = 85;
            this.button4.Text = "#BillNO";
            this.button4.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 84;
            this.label1.Text = "Sales Order";
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.button3.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(182)))));
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.ForeColor = System.Drawing.Color.White;
            this.button3.Location = new System.Drawing.Point(172, 28);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(50, 31);
            this.button3.TabIndex = 83;
            this.button3.Text = "F5";
            this.button3.UseVisualStyleBackColor = false;
            // 
            // textBoxhold
            // 
            appearance24.BorderColor = System.Drawing.Color.DeepSkyBlue;
            this.textBoxhold.Appearance = appearance24;
            this.textBoxhold.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.textBoxhold.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.textBoxhold.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxhold.Location = new System.Drawing.Point(6, 26);
            this.textBoxhold.Multiline = true;
            this.textBoxhold.Name = "textBoxhold";
            this.textBoxhold.Size = new System.Drawing.Size(218, 35);
            this.textBoxhold.TabIndex = 88;
            this.textBoxhold.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // textBox5
            // 
            appearance25.BorderColor = System.Drawing.Color.DeepSkyBlue;
            this.textBox5.Appearance = appearance25;
            this.textBox5.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.textBox5.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.textBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox5.Location = new System.Drawing.Point(7, 68);
            this.textBox5.Multiline = true;
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(218, 35);
            this.textBox5.TabIndex = 89;
            this.textBox5.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // textBoxround
            // 
            this.textBoxround.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance26.BorderColor = System.Drawing.Color.DeepSkyBlue;
            this.textBoxround.Appearance = appearance26;
            this.textBoxround.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.textBoxround.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.textBoxround.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxround.Location = new System.Drawing.Point(1178, 13);
            this.textBoxround.Multiline = true;
            this.textBoxround.Name = "textBoxround";
            this.textBoxround.Size = new System.Drawing.Size(163, 21);
            this.textBoxround.TabIndex = 82;
            this.textBoxround.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // txtSubtotal
            // 
            this.txtSubtotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance27.BorderColor = System.Drawing.Color.DeepSkyBlue;
            this.txtSubtotal.Appearance = appearance27;
            this.txtSubtotal.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.txtSubtotal.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.txtSubtotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSubtotal.Location = new System.Drawing.Point(950, 15);
            this.txtSubtotal.Multiline = true;
            this.txtSubtotal.Name = "txtSubtotal";
            this.txtSubtotal.Size = new System.Drawing.Size(122, 21);
            this.txtSubtotal.TabIndex = 80;
            this.txtSubtotal.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraCheckEditorApplyRounding
            // 
            this.ultraCheckEditorApplyRounding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance28.FontData.BoldAsString = "True";
            this.ultraCheckEditorApplyRounding.Appearance = appearance28;
            this.ultraCheckEditorApplyRounding.BackColor = System.Drawing.Color.Transparent;
            this.ultraCheckEditorApplyRounding.BackColorInternal = System.Drawing.Color.Transparent;
            this.ultraCheckEditorApplyRounding.Location = new System.Drawing.Point(1097, 15);
            this.ultraCheckEditorApplyRounding.Name = "ultraCheckEditorApplyRounding";
            this.ultraCheckEditorApplyRounding.Size = new System.Drawing.Size(74, 20);
            this.ultraCheckEditorApplyRounding.TabIndex = 79;
            this.ultraCheckEditorApplyRounding.Text = "Rounding";
            // 
            // ultraPictureBox7
            // 
            this.ultraPictureBox7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraPictureBox7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.ultraPictureBox7.BackColorInternal = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.ultraPictureBox7.BorderShadowColor = System.Drawing.Color.White;
            this.ultraPictureBox7.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraPictureBox7.Image = ((object)(resources.GetObject("ultraPictureBox7.Image")));
            this.ultraPictureBox7.Location = new System.Drawing.Point(1025, 55);
            this.ultraPictureBox7.Name = "ultraPictureBox7";
            this.ultraPictureBox7.Size = new System.Drawing.Size(42, 31);
            this.ultraPictureBox7.TabIndex = 78;
            // 
            // ultraPanel8
            // 
            this.ultraPanel8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            appearance29.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            appearance29.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance29.BorderColor = System.Drawing.Color.SlateGray;
            appearance29.BorderColor2 = System.Drawing.Color.Black;
            appearance29.BorderColor3DBase = System.Drawing.Color.Aqua;
            appearance29.ForeColor = System.Drawing.Color.Black;
            this.ultraPanel8.Appearance = appearance29;
            this.ultraPanel8.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded3;
            // 
            // ultraPanel8.ClientArea
            // 
            this.ultraPanel8.ClientArea.Controls.Add(this.ultraPanel9);
            this.ultraPanel8.Location = new System.Drawing.Point(1085, 40);
            this.ultraPanel8.Name = "ultraPanel8";
            this.ultraPanel8.Size = new System.Drawing.Size(259, 69);
            this.ultraPanel8.TabIndex = 77;
            // 
            // ultraPanel9
            // 
            appearance30.BackColor = System.Drawing.Color.Maroon;
            appearance30.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            appearance30.BackGradientStyle = Infragistics.Win.GradientStyle.GlassTop50;
            appearance30.BorderColor = System.Drawing.Color.White;
            appearance30.ForegroundAlpha = Infragistics.Win.Alpha.UseAlphaLevel;
            this.ultraPanel9.Appearance = appearance30;
            this.ultraPanel9.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanel9.ClientArea
            // 
            this.ultraPanel9.ClientArea.Controls.Add(this.txtNetTotal);
            this.ultraPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanel9.ForeColor = System.Drawing.Color.Beige;
            this.ultraPanel9.Location = new System.Drawing.Point(0, 0);
            this.ultraPanel9.Name = "ultraPanel9";
            this.ultraPanel9.Size = new System.Drawing.Size(255, 65);
            this.ultraPanel9.TabIndex = 0;
            // 
            // txtNetTotal
            // 
            appearance31.BackColor = System.Drawing.Color.Transparent;
            appearance31.TextHAlignAsString = "Center";
            appearance31.TextVAlignAsString = "Middle";
            this.txtNetTotal.Appearance = appearance31;
            this.txtNetTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNetTotal.Font = new System.Drawing.Font("DS-Digital", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNetTotal.Location = new System.Drawing.Point(0, 0);
            this.txtNetTotal.Name = "txtNetTotal";
            this.txtNetTotal.Size = new System.Drawing.Size(253, 63);
            this.txtNetTotal.TabIndex = 39;
            this.txtNetTotal.Text = "0.00";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(907, 53);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 16);
            this.label7.TabIndex = 76;
            this.label7.Text = "Disc";
            // 
            // lblsub
            // 
            this.lblsub.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblsub.AutoSize = true;
            this.lblsub.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblsub.Location = new System.Drawing.Point(870, 18);
            this.lblsub.Name = "lblsub";
            this.lblsub.Size = new System.Drawing.Size(75, 16);
            this.lblsub.TabIndex = 75;
            this.lblsub.Text = "Sub Total";
            // 
            // txtDisc
            // 
            this.txtDisc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            appearance32.BorderColor = System.Drawing.Color.DeepSkyBlue;
            this.txtDisc.Appearance = appearance32;
            this.txtDisc.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.txtDisc.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.txtDisc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDisc.Location = new System.Drawing.Point(950, 53);
            this.txtDisc.Multiline = true;
            this.txtDisc.Name = "txtDisc";
            this.txtDisc.Size = new System.Drawing.Size(119, 35);
            this.txtDisc.TabIndex = 81;
            this.txtDisc.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1349, 520);
            this.Controls.Add(this.pnlBody);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmSalesInvoice";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmSalesInvoice";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmSalesInvoice_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmSalesInvoice_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.frmSalesInvoice_KeyUp);
            this.pnlBody.ClientArea.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            this.frmSalesInvoice_Fill_Panel.ClientArea.ResumeLayout(false);
            this.frmSalesInvoice_Fill_Panel.ResumeLayout(false);
            this.pbxBill.ClientArea.ResumeLayout(false);
            this.pbxBill.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGrid1)).EndInit();
            this.gridFooterPanel.ResumeLayout(false);
            this.ultraPanel5.ClientArea.ResumeLayout(false);
            this.ultraPanel5.ClientArea.PerformLayout();
            this.ultraPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtBarcode)).EndInit();
            this.ultraPanel1.ClientArea.ResumeLayout(false);
            this.ultraPanel1.ClientArea.PerformLayout();
            this.ultraPanel1.ResumeLayout(false);
            this.ultraPanel7.ClientArea.ResumeLayout(false);
            this.ultraPanel7.ClientArea.PerformLayout();
            this.ultraPanel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCustomer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSalesPerson)).EndInit();
            this.ultraPanel14.ClientArea.ResumeLayout(false);
            this.ultraPanel14.ClientArea.PerformLayout();
            this.ultraPanel14.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmpPrice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxhold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxround)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSubtotal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraCheckEditorApplyRounding)).EndInit();
            this.ultraPanel8.ClientArea.ResumeLayout(false);
            this.ultraPanel8.ResumeLayout(false);
            this.ultraPanel9.ClientArea.ResumeLayout(false);
            this.ultraPanel9.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtDisc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDataSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDataSource2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDataSource3)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel pnlBody;
        private Infragistics.Win.Misc.UltraPanel frmSalesInvoice_Fill_Panel;
        private Infragistics.Win.Misc.UltraPanel pbxBill;
        private Infragistics.Win.Misc.UltraPanel ultraPanel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private Infragistics.Win.UltraWinDataSource.UltraDataSource ultraDataSource1;
        private System.Windows.Forms.ToolTip toolTip1;
        public System.Windows.Forms.Label lblledger;
        private Infragistics.Win.UltraWinDataSource.UltraDataSource ultraDataSource2;
        private Infragistics.Win.UltraWinDataSource.UltraDataSource ultraDataSource3;
        public System.Windows.Forms.Label lblBillNo;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGrid1;
        private Infragistics.Win.Misc.UltraPanel ultraPanel7;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox5;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox6;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtCustomer;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSalesPerson;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbPaymt;
        private Infragistics.Win.Misc.UltraPanel gridFooterPanel;

        // Add declarations for dynamically created controls
        private System.Windows.Forms.DataGridView dgvItems;
        private Infragistics.Win.UltraWinGrid.UltraGrid dgvitemlist;
        private System.Windows.Forms.CheckBox ChkSearch;
        private System.Windows.Forms.Panel pnlItem;
        private System.Windows.Forms.TextBox txtItemNameSearch;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button customerSelectButton;
        private Infragistics.Win.Misc.UltraPanel ultraPanel14;
        private Infragistics.Win.Misc.UltraPanel ultraPanel5;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label6;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtBarcode;
        public Infragistics.Win.UltraWinEditors.UltraComboEditor cmpPrice;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button3;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor textBoxhold;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor textBox5;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor textBoxround;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSubtotal;
        private Infragistics.Win.UltraWinEditors.UltraCheckEditor ultraCheckEditorApplyRounding;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox7;
        private Infragistics.Win.Misc.UltraPanel ultraPanel8;
        private Infragistics.Win.Misc.UltraPanel ultraPanel9;
        private Infragistics.Win.Misc.UltraLabel txtNetTotal;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblsub;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtDisc;
    }
}