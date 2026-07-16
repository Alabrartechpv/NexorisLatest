namespace PosBranch_Win.Transaction
{
    partial class FrmStockTransfer
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
        /// Required method for Designer do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmStockTransfer));
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance7 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance6 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance8 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance9 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance10 = new Infragistics.Win.Appearance();
            this.ultraPanelMain = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGrid1 = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelTop = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPictureBox1 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.grpSourceDest = new Infragistics.Win.Misc.UltraGroupBox();
            this.lblSourceBranch = new Infragistics.Win.Misc.UltraLabel();
            this.txtb_sourceBranch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblTargetBranch = new Infragistics.Win.Misc.UltraLabel();
            this.cmb_targetBranch = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.grpDocInfo = new Infragistics.Win.Misc.UltraGroupBox();
            this.lblDocNo = new Infragistics.Win.Misc.UltraLabel();
            this.txt_DocNo = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.pic_DocNoSearch = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.lblDate = new Infragistics.Win.Misc.UltraLabel();
            this.dateTimePicker1 = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblRemarks = new Infragistics.Win.Misc.UltraLabel();
            this.txteditor_remark = new Infragistics.Win.FormattedLinkLabel.UltraFormattedTextEditor();
            this.lblBarcode = new Infragistics.Win.Misc.UltraLabel();
            this.txtb_barcode = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultralblId = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelMain.ClientArea.SuspendLayout();
            this.ultraPanelMain.SuspendLayout();
            this.ultraPanelGrid.ClientArea.SuspendLayout();
            this.ultraPanelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGrid1)).BeginInit();
            this.ultraPanelTop.ClientArea.SuspendLayout();
            this.ultraPanelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpSourceDest)).BeginInit();
            this.grpSourceDest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtb_sourceBranch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmb_targetBranch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpDocInfo)).BeginInit();
            this.grpDocInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txt_DocNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePicker1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtb_barcode)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraPanelMain
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.ultraPanelMain.Appearance = appearance1;
            // 
            // ultraPanelMain.ClientArea
            // 
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraPanelGrid);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraPanelTop);
            this.ultraPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMain.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelMain.Name = "ultraPanelMain";
            this.ultraPanelMain.Size = new System.Drawing.Size(1364, 730);
            this.ultraPanelMain.TabIndex = 0;
            // 
            // ultraPanelGrid
            // 
            this.ultraPanelGrid.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;
            // 
            // ultraPanelGrid.ClientArea
            // 
            this.ultraPanelGrid.ClientArea.Controls.Add(this.ultraGrid1);
            this.ultraPanelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelGrid.Location = new System.Drawing.Point(0, 240);
            this.ultraPanelGrid.Name = "ultraPanelGrid";
            this.ultraPanelGrid.Size = new System.Drawing.Size(1364, 490);
            this.ultraPanelGrid.TabIndex = 1;
            // 
            // ultraGrid1
            // 
            appearance2.BackColor = System.Drawing.Color.White;
            appearance2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(224)))), ((int)(((byte)(233)))));
            this.ultraGrid1.DisplayLayout.Appearance = appearance2;
            this.ultraGrid1.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraGrid1.DisplayLayout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;
            this.ultraGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGrid1.Location = new System.Drawing.Point(0, 0);
            this.ultraGrid1.Name = "ultraGrid1";
            this.ultraGrid1.Size = new System.Drawing.Size(1364, 490);
            this.ultraGrid1.TabIndex = 0;
            // 
            // ultraPanelTop
            // 
            appearance3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.ultraPanelTop.Appearance = appearance3;
            // 
            // ultraPanelTop.ClientArea
            // 
            this.ultraPanelTop.ClientArea.Controls.Add(this.ultraPictureBox1);
            this.ultraPanelTop.ClientArea.Controls.Add(this.grpSourceDest);
            this.ultraPanelTop.ClientArea.Controls.Add(this.grpDocInfo);
            this.ultraPanelTop.ClientArea.Controls.Add(this.lblBarcode);
            this.ultraPanelTop.ClientArea.Controls.Add(this.txtb_barcode);
            this.ultraPanelTop.ClientArea.Controls.Add(this.ultralblId);
            this.ultraPanelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelTop.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelTop.Name = "ultraPanelTop";
            this.ultraPanelTop.Size = new System.Drawing.Size(1364, 240);
            this.ultraPanelTop.TabIndex = 0;
            // 
            // ultraPictureBox1
            // 
            this.ultraPictureBox1.BorderShadowColor = System.Drawing.Color.Empty;
            this.ultraPictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox1.Image = ((object)(resources.GetObject("ultraPictureBox1.Image")));
            this.ultraPictureBox1.Location = new System.Drawing.Point(466, 188);
            this.ultraPictureBox1.Name = "ultraPictureBox1";
            this.ultraPictureBox1.Size = new System.Drawing.Size(31, 31);
            this.ultraPictureBox1.TabIndex = 6;
            this.ultraPictureBox1.Click += new System.EventHandler(this.btn_ItemLoad_Click);
            // 
            // grpSourceDest
            // 
            appearance4.BackColor = System.Drawing.Color.White;
            appearance4.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(224)))), ((int)(((byte)(233)))));
            this.grpSourceDest.Appearance = appearance4;
            this.grpSourceDest.Controls.Add(this.lblSourceBranch);
            this.grpSourceDest.Controls.Add(this.txtb_sourceBranch);
            this.grpSourceDest.Controls.Add(this.lblTargetBranch);
            this.grpSourceDest.Controls.Add(this.cmb_targetBranch);
            appearance7.FontData.BoldAsString = "True";
            appearance7.FontData.Name = "Segoe UI";
            appearance7.FontData.SizeInPoints = 9.5F;
            appearance7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(86)))));
            this.grpSourceDest.HeaderAppearance = appearance7;
            this.grpSourceDest.HeaderPosition = Infragistics.Win.Misc.GroupBoxHeaderPosition.TopInsideBorder;
            this.grpSourceDest.Location = new System.Drawing.Point(20, 15);
            this.grpSourceDest.Name = "grpSourceDest";
            this.grpSourceDest.Size = new System.Drawing.Size(480, 150);
            this.grpSourceDest.TabIndex = 0;
            this.grpSourceDest.Text = "Transfer Source & Destination";
            this.grpSourceDest.ViewStyle = Infragistics.Win.Misc.GroupBoxViewStyle.Office2007;
            // 
            // lblSourceBranch
            // 
            this.lblSourceBranch.AutoSize = true;
            this.lblSourceBranch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSourceBranch.Location = new System.Drawing.Point(15, 45);
            this.lblSourceBranch.Name = "lblSourceBranch";
            this.lblSourceBranch.Size = new System.Drawing.Size(81, 18);
            this.lblSourceBranch.TabIndex = 0;
            this.lblSourceBranch.Text = "From (Source)";
            // 
            // txtb_sourceBranch
            // 
            appearance5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(245)))));
            appearance5.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(212)))), ((int)(((byte)(218)))));
            this.txtb_sourceBranch.Appearance = appearance5;
            this.txtb_sourceBranch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(245)))));
            this.txtb_sourceBranch.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.txtb_sourceBranch.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtb_sourceBranch.Location = new System.Drawing.Point(120, 41);
            this.txtb_sourceBranch.Name = "txtb_sourceBranch";
            this.txtb_sourceBranch.ReadOnly = true;
            this.txtb_sourceBranch.Size = new System.Drawing.Size(320, 26);
            this.txtb_sourceBranch.TabIndex = 0;
            // 
            // lblTargetBranch
            // 
            this.lblTargetBranch.AutoSize = true;
            this.lblTargetBranch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTargetBranch.Location = new System.Drawing.Point(15, 92);
            this.lblTargetBranch.Name = "lblTargetBranch";
            this.lblTargetBranch.Size = new System.Drawing.Size(64, 18);
            this.lblTargetBranch.TabIndex = 1;
            this.lblTargetBranch.Text = "To (Target)";
            // 
            // cmb_targetBranch
            // 
            appearance6.BackColor = System.Drawing.Color.White;
            appearance6.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(212)))), ((int)(((byte)(218)))));
            this.cmb_targetBranch.Appearance = appearance6;
            this.cmb_targetBranch.BackColor = System.Drawing.Color.White;
            this.cmb_targetBranch.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.cmb_targetBranch.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmb_targetBranch.Location = new System.Drawing.Point(120, 88);
            this.cmb_targetBranch.Name = "cmb_targetBranch";
            this.cmb_targetBranch.Size = new System.Drawing.Size(320, 26);
            this.cmb_targetBranch.TabIndex = 1;
            // 
            // grpDocInfo
            // 
            this.grpDocInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpDocInfo.Appearance = appearance4;
            this.grpDocInfo.Controls.Add(this.lblDocNo);
            this.grpDocInfo.Controls.Add(this.txt_DocNo);
            this.grpDocInfo.Controls.Add(this.pic_DocNoSearch);
            this.grpDocInfo.Controls.Add(this.lblDate);
            this.grpDocInfo.Controls.Add(this.dateTimePicker1);
            this.grpDocInfo.Controls.Add(this.lblRemarks);
            this.grpDocInfo.Controls.Add(this.txteditor_remark);
            this.grpDocInfo.HeaderAppearance = appearance7;
            this.grpDocInfo.HeaderPosition = Infragistics.Win.Misc.GroupBoxHeaderPosition.TopInsideBorder;
            this.grpDocInfo.Location = new System.Drawing.Point(520, 15);
            this.grpDocInfo.Name = "grpDocInfo";
            this.grpDocInfo.Size = new System.Drawing.Size(820, 150);
            this.grpDocInfo.TabIndex = 1;
            this.grpDocInfo.Text = "Transaction Info";
            this.grpDocInfo.ViewStyle = Infragistics.Win.Misc.GroupBoxViewStyle.Office2007;
            // 
            // lblDocNo
            // 
            this.lblDocNo.AutoSize = true;
            this.lblDocNo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDocNo.Location = new System.Drawing.Point(20, 45);
            this.lblDocNo.Name = "lblDocNo";
            this.lblDocNo.Size = new System.Drawing.Size(46, 18);
            this.lblDocNo.TabIndex = 0;
            this.lblDocNo.Text = "Doc No";
            // 
            // txt_DocNo
            // 
            appearance8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(251)))), ((int)(((byte)(234)))));
            appearance8.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(212)))), ((int)(((byte)(218)))));
            this.txt_DocNo.Appearance = appearance8;
            this.txt_DocNo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(251)))), ((int)(((byte)(234)))));
            this.txt_DocNo.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.txt_DocNo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.txt_DocNo.Location = new System.Drawing.Point(100, 41);
            this.txt_DocNo.Name = "txt_DocNo";
            this.txt_DocNo.ReadOnly = true;
            this.txt_DocNo.Size = new System.Drawing.Size(180, 26);
            this.txt_DocNo.TabIndex = 0;
            // 
            // pic_DocNoSearch
            // 
            this.pic_DocNoSearch.BorderShadowColor = System.Drawing.Color.Empty;
            this.pic_DocNoSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pic_DocNoSearch.Image = ((object)(resources.GetObject("pic_DocNoSearch.Image")));
            this.pic_DocNoSearch.Location = new System.Drawing.Point(286, 39);
            this.pic_DocNoSearch.Name = "pic_DocNoSearch";
            this.pic_DocNoSearch.Size = new System.Drawing.Size(31, 31);
            this.pic_DocNoSearch.TabIndex = 10;
            this.pic_DocNoSearch.Click += new System.EventHandler(this.pic_DocNoSearch_Click);
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDate.Location = new System.Drawing.Point(330, 45);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(30, 18);
            this.lblDate.TabIndex = 1;
            this.lblDate.Text = "Date";
            // 
            // dateTimePicker1
            // 
            appearance9.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(212)))), ((int)(((byte)(218)))));
            this.dateTimePicker1.Appearance = appearance9;
            this.dateTimePicker1.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.dateTimePicker1.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dateTimePicker1.Location = new System.Drawing.Point(380, 41);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(160, 26);
            this.dateTimePicker1.TabIndex = 1;
            // 
            // lblRemarks
            // 
            this.lblRemarks.AutoSize = true;
            this.lblRemarks.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRemarks.Location = new System.Drawing.Point(20, 92);
            this.lblRemarks.Name = "lblRemarks";
            this.lblRemarks.Size = new System.Drawing.Size(51, 18);
            this.lblRemarks.TabIndex = 2;
            this.lblRemarks.Text = "Remarks";
            // 
            // txteditor_remark
            // 
            this.txteditor_remark.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txteditor_remark.Location = new System.Drawing.Point(100, 85);
            this.txteditor_remark.Name = "txteditor_remark";
            this.txteditor_remark.Size = new System.Drawing.Size(700, 50);
            this.txteditor_remark.TabIndex = 2;
            this.txteditor_remark.Value = "";
            // 
            // lblBarcode
            // 
            this.lblBarcode.AutoSize = true;
            this.lblBarcode.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblBarcode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(86)))));
            this.lblBarcode.Location = new System.Drawing.Point(25, 192);
            this.lblBarcode.Name = "lblBarcode";
            this.lblBarcode.Size = new System.Drawing.Size(89, 20);
            this.lblBarcode.TabIndex = 1;
            this.lblBarcode.Text = "Scan Barcode";
            // 
            // txtb_barcode
            // 
            appearance10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(251)))), ((int)(((byte)(234)))));
            appearance10.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(86)))));
            this.txtb_barcode.Appearance = appearance10;
            this.txtb_barcode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(251)))), ((int)(((byte)(234)))));
            this.txtb_barcode.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.txtb_barcode.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.txtb_barcode.Location = new System.Drawing.Point(140, 188);
            this.txtb_barcode.Name = "txtb_barcode";
            this.txtb_barcode.Size = new System.Drawing.Size(320, 31);
            this.txtb_barcode.TabIndex = 2;
            this.txtb_barcode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtb_barcode_KeyDown);
            // 
            // ultralblId
            // 
            this.ultralblId.AutoSize = true;
            this.ultralblId.Location = new System.Drawing.Point(508, 194);
            this.ultralblId.Name = "ultralblId";
            this.ultralblId.Size = new System.Drawing.Size(14, 14);
            this.ultralblId.TabIndex = 5;
            this.ultralblId.Text = "Id";
            this.ultralblId.Visible = false;
            // 
            // FrmStockTransfer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1364, 730);
            this.Controls.Add(this.ultraPanelMain);
            this.Name = "FrmStockTransfer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stock Transfer Entry";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmStockTransfer_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmStockTransfer_KeyDown);
            this.ultraPanelMain.ClientArea.ResumeLayout(false);
            this.ultraPanelMain.ResumeLayout(false);
            this.ultraPanelGrid.ClientArea.ResumeLayout(false);
            this.ultraPanelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGrid1)).EndInit();
            this.ultraPanelTop.ClientArea.ResumeLayout(false);
            this.ultraPanelTop.ClientArea.PerformLayout();
            this.ultraPanelTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grpSourceDest)).EndInit();
            this.grpSourceDest.ResumeLayout(false);
            this.grpSourceDest.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtb_sourceBranch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmb_targetBranch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpDocInfo)).EndInit();
            this.grpDocInfo.ResumeLayout(false);
            this.grpDocInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txt_DocNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePicker1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtb_barcode)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelMain;
        private Infragistics.Win.Misc.UltraPanel ultraPanelTop;
        private Infragistics.Win.Misc.UltraGroupBox grpSourceDest;
        private Infragistics.Win.Misc.UltraLabel lblSourceBranch;
        private Infragistics.Win.Misc.UltraLabel lblTargetBranch;
        public Infragistics.Win.UltraWinEditors.UltraTextEditor txtb_sourceBranch;
        public Infragistics.Win.UltraWinEditors.UltraComboEditor cmb_targetBranch;
        private Infragistics.Win.Misc.UltraGroupBox grpDocInfo;
        private Infragistics.Win.Misc.UltraLabel lblDocNo;
        public Infragistics.Win.UltraWinEditors.UltraTextEditor txt_DocNo;
        private Infragistics.Win.Misc.UltraLabel lblDate;
        public Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dateTimePicker1;
        public Infragistics.Win.FormattedLinkLabel.UltraFormattedTextEditor txteditor_remark;
        private Infragistics.Win.Misc.UltraLabel lblRemarks;
        public Infragistics.Win.UltraWinEditors.UltraTextEditor txtb_barcode;
        private Infragistics.Win.Misc.UltraLabel lblBarcode;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGrid;
        public Infragistics.Win.UltraWinGrid.UltraGrid ultraGrid1;
        public Infragistics.Win.Misc.UltraLabel ultralblId;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox1;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox pic_DocNoSearch;
    }
}
