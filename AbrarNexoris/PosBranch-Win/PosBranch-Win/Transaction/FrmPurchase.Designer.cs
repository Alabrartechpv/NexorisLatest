
namespace PosBranch_Win.Transaction
{
    partial class FrmPurchase
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
            this.lblPurchase = new System.Windows.Forms.Label();
            this.lblBranch = new System.Windows.Forms.Label();
            this.CmboBranch = new System.Windows.Forms.ComboBox();
            this.lblPurchaseNo = new System.Windows.Forms.Label();
            this.txtPurchaseNo = new System.Windows.Forms.TextBox();
            this.lblPurchaseDate = new System.Windows.Forms.Label();
            this.dtpPurchaseDate = new System.Windows.Forms.DateTimePicker();
            this.lblVendor = new System.Windows.Forms.Label();
            this.CmboVendor = new System.Windows.Forms.ComboBox();
            this.lblPayMode = new System.Windows.Forms.Label();
            this.CmboPayment = new System.Windows.Forms.ComboBox();
            this.groupBoxMain = new System.Windows.Forms.GroupBox();
            this.groupBoxInvoice = new System.Windows.Forms.GroupBox();
            this.txtInvoiceAmt = new System.Windows.Forms.TextBox();
            this.lblInvoiceNo = new System.Windows.Forms.Label();
            this.DtpInoviceDate = new System.Windows.Forms.DateTimePicker();
            this.lblInvoiceDate = new System.Windows.Forms.Label();
            this.txtInvoiceNo = new System.Windows.Forms.TextBox();
            this.lblnvoice = new System.Windows.Forms.Label();
            this.txtOrderNo = new System.Windows.Forms.TextBox();
            this.lblOrder = new System.Windows.Forms.Label();
            this.groupBoxItems = new System.Windows.Forms.GroupBox();
            this.btnFInd = new System.Windows.Forms.Button();
            this.txtBarcode = new System.Windows.Forms.TextBox();
            this.lblBarcode = new System.Windows.Forms.Label();
            this.dgvItem = new System.Windows.Forms.DataGridView();
            this.groupBoxOthers = new System.Windows.Forms.GroupBox();
            this.lblPayedAmt = new System.Windows.Forms.Label();
            this.lblBilledBy = new System.Windows.Forms.Label();
            this.txtPayedAmt = new System.Windows.Forms.TextBox();
            this.txtBilledBy = new System.Windows.Forms.TextBox();
            this.txtRoundOff = new System.Windows.Forms.TextBox();
            this.lblRoundOf = new System.Windows.Forms.Label();
            this.GrandToalAmt = new System.Windows.Forms.Label();
            this.lblGrandTotal = new System.Windows.Forms.Label();
            this.lblSubTotalAmt = new System.Windows.Forms.Label();
            this.lblSubTotal = new System.Windows.Forms.Label();
            this.panelButton = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.groupBoxMain.SuspendLayout();
            this.groupBoxInvoice.SuspendLayout();
            this.groupBoxItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItem)).BeginInit();
            this.groupBoxOthers.SuspendLayout();
            this.panelButton.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPurchase
            // 
            this.lblPurchase.AutoSize = true;
            this.lblPurchase.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPurchase.Location = new System.Drawing.Point(477, -4);
            this.lblPurchase.Name = "lblPurchase";
            this.lblPurchase.Size = new System.Drawing.Size(95, 24);
            this.lblPurchase.TabIndex = 0;
            this.lblPurchase.Text = "Purchase ";
            // 
            // lblBranch
            // 
            this.lblBranch.AutoSize = true;
            this.lblBranch.Location = new System.Drawing.Point(12, 16);
            this.lblBranch.Name = "lblBranch";
            this.lblBranch.Size = new System.Drawing.Size(46, 15);
            this.lblBranch.TabIndex = 0;
            this.lblBranch.Text = "Branch";
            // 
            // CmboBranch
            // 
            this.CmboBranch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CmboBranch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CmboBranch.FormattingEnabled = true;
            this.CmboBranch.Location = new System.Drawing.Point(12, 35);
            this.CmboBranch.Name = "CmboBranch";
            this.CmboBranch.Size = new System.Drawing.Size(210, 23);
            this.CmboBranch.TabIndex = 1;
            // 
            // lblPurchaseNo
            // 
            this.lblPurchaseNo.AutoSize = true;
            this.lblPurchaseNo.Location = new System.Drawing.Point(255, 16);
            this.lblPurchaseNo.Name = "lblPurchaseNo";
            this.lblPurchaseNo.Size = new System.Drawing.Size(75, 15);
            this.lblPurchaseNo.TabIndex = 2;
            this.lblPurchaseNo.Text = "PurchaseNo";
            // 
            // txtPurchaseNo
            // 
            this.txtPurchaseNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPurchaseNo.Location = new System.Drawing.Point(255, 35);
            this.txtPurchaseNo.Name = "txtPurchaseNo";
            this.txtPurchaseNo.Size = new System.Drawing.Size(154, 24);
            this.txtPurchaseNo.TabIndex = 3;
            // 
            // lblPurchaseDate
            // 
            this.lblPurchaseDate.AutoSize = true;
            this.lblPurchaseDate.Location = new System.Drawing.Point(420, 16);
            this.lblPurchaseDate.Name = "lblPurchaseDate";
            this.lblPurchaseDate.Size = new System.Drawing.Size(85, 15);
            this.lblPurchaseDate.TabIndex = 4;
            this.lblPurchaseDate.Text = "PurchaseDate";
            // 
            // dtpPurchaseDate
            // 
            this.dtpPurchaseDate.Location = new System.Drawing.Point(420, 35);
            this.dtpPurchaseDate.Name = "dtpPurchaseDate";
            this.dtpPurchaseDate.Size = new System.Drawing.Size(172, 21);
            this.dtpPurchaseDate.TabIndex = 5;
            // 
            // lblVendor
            // 
            this.lblVendor.AutoSize = true;
            this.lblVendor.Location = new System.Drawing.Point(615, 16);
            this.lblVendor.Name = "lblVendor";
            this.lblVendor.Size = new System.Drawing.Size(46, 15);
            this.lblVendor.TabIndex = 6;
            this.lblVendor.Text = "Vendor";
            // 
            // CmboVendor
            // 
            this.CmboVendor.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CmboVendor.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CmboVendor.FormattingEnabled = true;
            this.CmboVendor.Location = new System.Drawing.Point(615, 35);
            this.CmboVendor.Name = "CmboVendor";
            this.CmboVendor.Size = new System.Drawing.Size(307, 23);
            this.CmboVendor.TabIndex = 7;
            // 
            // lblPayMode
            // 
            this.lblPayMode.AutoSize = true;
            this.lblPayMode.Location = new System.Drawing.Point(931, 16);
            this.lblPayMode.Name = "lblPayMode";
            this.lblPayMode.Size = new System.Drawing.Size(97, 15);
            this.lblPayMode.TabIndex = 8;
            this.lblPayMode.Text = "PaymentMethod";
            // 
            // CmboPayment
            // 
            this.CmboPayment.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CmboPayment.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CmboPayment.FormattingEnabled = true;
            this.CmboPayment.Location = new System.Drawing.Point(931, 35);
            this.CmboPayment.Name = "CmboPayment";
            this.CmboPayment.Size = new System.Drawing.Size(150, 23);
            this.CmboPayment.TabIndex = 9;
            // 
            // groupBoxMain
            // 
            this.groupBoxMain.Controls.Add(this.CmboPayment);
            this.groupBoxMain.Controls.Add(this.lblBranch);
            this.groupBoxMain.Controls.Add(this.lblPayMode);
            this.groupBoxMain.Controls.Add(this.CmboBranch);
            this.groupBoxMain.Controls.Add(this.CmboVendor);
            this.groupBoxMain.Controls.Add(this.lblPurchaseNo);
            this.groupBoxMain.Controls.Add(this.lblVendor);
            this.groupBoxMain.Controls.Add(this.txtPurchaseNo);
            this.groupBoxMain.Controls.Add(this.dtpPurchaseDate);
            this.groupBoxMain.Controls.Add(this.lblPurchaseDate);
            this.groupBoxMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxMain.Location = new System.Drawing.Point(12, 25);
            this.groupBoxMain.Name = "groupBoxMain";
            this.groupBoxMain.Size = new System.Drawing.Size(1211, 64);
            this.groupBoxMain.TabIndex = 2;
            this.groupBoxMain.TabStop = false;
            this.groupBoxMain.Text = "Main";
            // 
            // groupBoxInvoice
            // 
            this.groupBoxInvoice.Controls.Add(this.txtInvoiceAmt);
            this.groupBoxInvoice.Controls.Add(this.lblInvoiceNo);
            this.groupBoxInvoice.Controls.Add(this.DtpInoviceDate);
            this.groupBoxInvoice.Controls.Add(this.lblInvoiceDate);
            this.groupBoxInvoice.Controls.Add(this.txtInvoiceNo);
            this.groupBoxInvoice.Controls.Add(this.lblnvoice);
            this.groupBoxInvoice.Controls.Add(this.txtOrderNo);
            this.groupBoxInvoice.Controls.Add(this.lblOrder);
            this.groupBoxInvoice.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxInvoice.Location = new System.Drawing.Point(13, 95);
            this.groupBoxInvoice.Name = "groupBoxInvoice";
            this.groupBoxInvoice.Size = new System.Drawing.Size(1210, 64);
            this.groupBoxInvoice.TabIndex = 3;
            this.groupBoxInvoice.TabStop = false;
            this.groupBoxInvoice.Text = "Invoice";
            // 
            // txtInvoiceAmt
            // 
            this.txtInvoiceAmt.Location = new System.Drawing.Point(610, 36);
            this.txtInvoiceAmt.Name = "txtInvoiceAmt";
            this.txtInvoiceAmt.Size = new System.Drawing.Size(157, 21);
            this.txtInvoiceAmt.TabIndex = 7;
            // 
            // lblInvoiceNo
            // 
            this.lblInvoiceNo.AutoSize = true;
            this.lblInvoiceNo.Location = new System.Drawing.Point(610, 18);
            this.lblInvoiceNo.Name = "lblInvoiceNo";
            this.lblInvoiceNo.Size = new System.Drawing.Size(87, 15);
            this.lblInvoiceNo.TabIndex = 6;
            this.lblInvoiceNo.Text = "InvoiceAmount";
            // 
            // DtpInoviceDate
            // 
            this.DtpInoviceDate.Location = new System.Drawing.Point(423, 36);
            this.DtpInoviceDate.Name = "DtpInoviceDate";
            this.DtpInoviceDate.Size = new System.Drawing.Size(168, 21);
            this.DtpInoviceDate.TabIndex = 5;
            // 
            // lblInvoiceDate
            // 
            this.lblInvoiceDate.AutoSize = true;
            this.lblInvoiceDate.Location = new System.Drawing.Point(423, 18);
            this.lblInvoiceDate.Name = "lblInvoiceDate";
            this.lblInvoiceDate.Size = new System.Drawing.Size(71, 15);
            this.lblInvoiceDate.TabIndex = 4;
            this.lblInvoiceDate.Text = "InvoiceDate";
            // 
            // txtInvoiceNo
            // 
            this.txtInvoiceNo.Location = new System.Drawing.Point(254, 36);
            this.txtInvoiceNo.Name = "txtInvoiceNo";
            this.txtInvoiceNo.Size = new System.Drawing.Size(154, 21);
            this.txtInvoiceNo.TabIndex = 3;
            // 
            // lblnvoice
            // 
            this.lblnvoice.AutoSize = true;
            this.lblnvoice.Location = new System.Drawing.Point(254, 18);
            this.lblnvoice.Name = "lblnvoice";
            this.lblnvoice.Size = new System.Drawing.Size(64, 15);
            this.lblnvoice.TabIndex = 2;
            this.lblnvoice.Text = "Invoice No";
            // 
            // txtOrderNo
            // 
            this.txtOrderNo.Location = new System.Drawing.Point(13, 36);
            this.txtOrderNo.Name = "txtOrderNo";
            this.txtOrderNo.Size = new System.Drawing.Size(217, 21);
            this.txtOrderNo.TabIndex = 1;
            // 
            // lblOrder
            // 
            this.lblOrder.AutoSize = true;
            this.lblOrder.Location = new System.Drawing.Point(13, 18);
            this.lblOrder.Name = "lblOrder";
            this.lblOrder.Size = new System.Drawing.Size(54, 15);
            this.lblOrder.TabIndex = 0;
            this.lblOrder.Text = "OrderNo";
            // 
            // groupBoxItems
            // 
            this.groupBoxItems.Controls.Add(this.btnFInd);
            this.groupBoxItems.Controls.Add(this.txtBarcode);
            this.groupBoxItems.Controls.Add(this.lblBarcode);
            this.groupBoxItems.Controls.Add(this.dgvItem);
            this.groupBoxItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxItems.Location = new System.Drawing.Point(13, 185);
            this.groupBoxItems.Name = "groupBoxItems";
            this.groupBoxItems.Size = new System.Drawing.Size(1210, 209);
            this.groupBoxItems.TabIndex = 4;
            this.groupBoxItems.TabStop = false;
            this.groupBoxItems.Text = "Items";
            // 
            // btnFInd
            // 
            this.btnFInd.Location = new System.Drawing.Point(315, 17);
            this.btnFInd.Name = "btnFInd";
            this.btnFInd.Size = new System.Drawing.Size(75, 23);
            this.btnFInd.TabIndex = 3;
            this.btnFInd.Text = "F7";
            this.btnFInd.UseVisualStyleBackColor = true;
            // 
            // txtBarcode
            // 
            this.txtBarcode.Location = new System.Drawing.Point(103, 18);
            this.txtBarcode.Name = "txtBarcode";
            this.txtBarcode.Size = new System.Drawing.Size(197, 21);
            this.txtBarcode.TabIndex = 2;
            this.txtBarcode.TextChanged += new System.EventHandler(this.txtBarcode_TextChanged);
            this.txtBarcode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtBarcode_KeyDown);
            // 
            // lblBarcode
            // 
            this.lblBarcode.AutoSize = true;
            this.lblBarcode.Location = new System.Drawing.Point(35, 22);
            this.lblBarcode.Name = "lblBarcode";
            this.lblBarcode.Size = new System.Drawing.Size(53, 15);
            this.lblBarcode.TabIndex = 1;
            this.lblBarcode.Text = "Barcode";
            // 
            // dgvItem
            // 
            this.dgvItem.AllowUserToAddRows = false;
            this.dgvItem.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvItem.Location = new System.Drawing.Point(15, 49);
            this.dgvItem.Name = "dgvItem";
            this.dgvItem.Size = new System.Drawing.Size(1179, 148);
            this.dgvItem.TabIndex = 0;
            this.dgvItem.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvItem_CellValueChanged);
            // 
            // groupBoxOthers
            // 
            this.groupBoxOthers.Controls.Add(this.lblPayedAmt);
            this.groupBoxOthers.Controls.Add(this.lblBilledBy);
            this.groupBoxOthers.Controls.Add(this.txtPayedAmt);
            this.groupBoxOthers.Controls.Add(this.txtBilledBy);
            this.groupBoxOthers.Controls.Add(this.txtRoundOff);
            this.groupBoxOthers.Controls.Add(this.lblRoundOf);
            this.groupBoxOthers.Controls.Add(this.GrandToalAmt);
            this.groupBoxOthers.Controls.Add(this.lblGrandTotal);
            this.groupBoxOthers.Controls.Add(this.lblSubTotalAmt);
            this.groupBoxOthers.Controls.Add(this.lblSubTotal);
            this.groupBoxOthers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxOthers.Location = new System.Drawing.Point(24, 400);
            this.groupBoxOthers.Name = "groupBoxOthers";
            this.groupBoxOthers.Size = new System.Drawing.Size(1199, 62);
            this.groupBoxOthers.TabIndex = 5;
            this.groupBoxOthers.TabStop = false;
            this.groupBoxOthers.Text = "Other";
            // 
            // lblPayedAmt
            // 
            this.lblPayedAmt.AutoSize = true;
            this.lblPayedAmt.Location = new System.Drawing.Point(10, 21);
            this.lblPayedAmt.Name = "lblPayedAmt";
            this.lblPayedAmt.Size = new System.Drawing.Size(76, 15);
            this.lblPayedAmt.TabIndex = 9;
            this.lblPayedAmt.Text = "PayedAmont";
            // 
            // lblBilledBy
            // 
            this.lblBilledBy.AutoSize = true;
            this.lblBilledBy.Location = new System.Drawing.Point(254, 21);
            this.lblBilledBy.Name = "lblBilledBy";
            this.lblBilledBy.Size = new System.Drawing.Size(54, 15);
            this.lblBilledBy.TabIndex = 8;
            this.lblBilledBy.Text = "Billed By";
            // 
            // txtPayedAmt
            // 
            this.txtPayedAmt.Location = new System.Drawing.Point(92, 21);
            this.txtPayedAmt.Name = "txtPayedAmt";
            this.txtPayedAmt.Size = new System.Drawing.Size(135, 21);
            this.txtPayedAmt.TabIndex = 7;
            // 
            // txtBilledBy
            // 
            this.txtBilledBy.Location = new System.Drawing.Point(314, 21);
            this.txtBilledBy.Name = "txtBilledBy";
            this.txtBilledBy.Size = new System.Drawing.Size(149, 21);
            this.txtBilledBy.TabIndex = 6;
            // 
            // txtRoundOff
            // 
            this.txtRoundOff.Location = new System.Drawing.Point(583, 21);
            this.txtRoundOff.Name = "txtRoundOff";
            this.txtRoundOff.Size = new System.Drawing.Size(140, 21);
            this.txtRoundOff.TabIndex = 5;
            // 
            // lblRoundOf
            // 
            this.lblRoundOf.AutoSize = true;
            this.lblRoundOf.Location = new System.Drawing.Point(512, 21);
            this.lblRoundOf.Name = "lblRoundOf";
            this.lblRoundOf.Size = new System.Drawing.Size(65, 15);
            this.lblRoundOf.TabIndex = 4;
            this.lblRoundOf.Text = "Round Off:";
            // 
            // GrandToalAmt
            // 
            this.GrandToalAmt.AutoSize = true;
            this.GrandToalAmt.Location = new System.Drawing.Point(855, 21);
            this.GrandToalAmt.Name = "GrandToalAmt";
            this.GrandToalAmt.Size = new System.Drawing.Size(59, 15);
            this.GrandToalAmt.TabIndex = 3;
            this.GrandToalAmt.Text = "2000.000";
            // 
            // lblGrandTotal
            // 
            this.lblGrandTotal.AutoSize = true;
            this.lblGrandTotal.Location = new System.Drawing.Point(758, 21);
            this.lblGrandTotal.Name = "lblGrandTotal";
            this.lblGrandTotal.Size = new System.Drawing.Size(74, 15);
            this.lblGrandTotal.TabIndex = 2;
            this.lblGrandTotal.Text = "Grand Total:";
            // 
            // lblSubTotalAmt
            // 
            this.lblSubTotalAmt.AutoSize = true;
            this.lblSubTotalAmt.Location = new System.Drawing.Point(1082, 21);
            this.lblSubTotalAmt.Name = "lblSubTotalAmt";
            this.lblSubTotalAmt.Size = new System.Drawing.Size(59, 15);
            this.lblSubTotalAmt.TabIndex = 1;
            this.lblSubTotalAmt.Text = "1999.000";
            // 
            // lblSubTotal
            // 
            this.lblSubTotal.AutoSize = true;
            this.lblSubTotal.Location = new System.Drawing.Point(1013, 21);
            this.lblSubTotal.Name = "lblSubTotal";
            this.lblSubTotal.Size = new System.Drawing.Size(59, 15);
            this.lblSubTotal.TabIndex = 0;
            this.lblSubTotal.Text = "SubTotal:";
            // 
            // panelButton
            // 
            this.panelButton.Controls.Add(this.btnClose);
            this.panelButton.Controls.Add(this.btnClear);
            this.panelButton.Controls.Add(this.btnSave);
            this.panelButton.Location = new System.Drawing.Point(372, 468);
            this.panelButton.Name = "panelButton";
            this.panelButton.Size = new System.Drawing.Size(330, 52);
            this.panelButton.TabIndex = 6;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnClose.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnClose.Location = new System.Drawing.Point(221, 8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(97, 34);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnClear.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnClear.Location = new System.Drawing.Point(114, 8);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(97, 34);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnSave.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnSave.Location = new System.Drawing.Point(6, 8);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(97, 34);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // FrmPurchase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1247, 532);
            this.Controls.Add(this.panelButton);
            this.Controls.Add(this.groupBoxOthers);
            this.Controls.Add(this.groupBoxItems);
            this.Controls.Add(this.groupBoxInvoice);
            this.Controls.Add(this.groupBoxMain);
            this.Controls.Add(this.lblPurchase);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.Name = "FrmPurchase";
            this.Text = "FrmPurchase";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmPurchase_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmPurchase_KeyDown);
            this.groupBoxMain.ResumeLayout(false);
            this.groupBoxMain.PerformLayout();
            this.groupBoxInvoice.ResumeLayout(false);
            this.groupBoxInvoice.PerformLayout();
            this.groupBoxItems.ResumeLayout(false);
            this.groupBoxItems.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItem)).EndInit();
            this.groupBoxOthers.ResumeLayout(false);
            this.groupBoxOthers.PerformLayout();
            this.panelButton.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPurchase;
        private System.Windows.Forms.Label lblBranch;
        private System.Windows.Forms.ComboBox CmboBranch;
        private System.Windows.Forms.Label lblPurchaseNo;
        private System.Windows.Forms.TextBox txtPurchaseNo;
        private System.Windows.Forms.Label lblPurchaseDate;
        private System.Windows.Forms.DateTimePicker dtpPurchaseDate;
        private System.Windows.Forms.Label lblVendor;
        private System.Windows.Forms.ComboBox CmboVendor;
        private System.Windows.Forms.Label lblPayMode;
        private System.Windows.Forms.ComboBox CmboPayment;
        private System.Windows.Forms.GroupBox groupBoxMain;
        private System.Windows.Forms.GroupBox groupBoxInvoice;
        private System.Windows.Forms.TextBox txtInvoiceAmt;
        private System.Windows.Forms.Label lblInvoiceNo;
        private System.Windows.Forms.DateTimePicker DtpInoviceDate;
        private System.Windows.Forms.Label lblInvoiceDate;
        private System.Windows.Forms.TextBox txtInvoiceNo;
        private System.Windows.Forms.Label lblnvoice;
        private System.Windows.Forms.TextBox txtOrderNo;
        private System.Windows.Forms.Label lblOrder;
        private System.Windows.Forms.GroupBox groupBoxItems;
        private System.Windows.Forms.GroupBox groupBoxOthers;
        private System.Windows.Forms.Label lblPayedAmt;
        private System.Windows.Forms.Label lblBilledBy;
        private System.Windows.Forms.TextBox txtPayedAmt;
        private System.Windows.Forms.TextBox txtBilledBy;
        private System.Windows.Forms.TextBox txtRoundOff;
        private System.Windows.Forms.Label lblRoundOf;
        private System.Windows.Forms.Label GrandToalAmt;
        private System.Windows.Forms.Label lblGrandTotal;
        private System.Windows.Forms.Label lblSubTotalAmt;
        private System.Windows.Forms.Label lblSubTotal;
        private System.Windows.Forms.Panel panelButton;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnFInd;
        private System.Windows.Forms.TextBox txtBarcode;
        private System.Windows.Forms.Label lblBarcode;
        public System.Windows.Forms.DataGridView dgvItem;
    }
}