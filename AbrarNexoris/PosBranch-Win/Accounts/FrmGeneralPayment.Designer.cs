namespace PosBranch_Win.Accounts
{
    partial class FrmGeneralPayment
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
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
            Infragistics.Win.Appearance appearanceCashBank = new Infragistics.Win.Appearance();
            this.lblHeader = new Infragistics.Win.Misc.UltraLabel();
            this.headerPanel = new Infragistics.Win.Misc.UltraPanel();
            this.dtpVoucherDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblBranch = new Infragistics.Win.Misc.UltraLabel();
            this.lblVoucherDate = new Infragistics.Win.Misc.UltraLabel();
            this.CmboBranch = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCashBank = new Infragistics.Win.Misc.UltraLabel();
            this.CmboCashBank = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblVocuherNo = new Infragistics.Win.Misc.UltraLabel();
            this.txtVoucherNo = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.gridPayment = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.footerPanel = new Infragistics.Win.Misc.UltraPanel();
            this.lblDifferenceValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblDifference = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCreditValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalCredit = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalDebitValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalDebit = new Infragistics.Win.Misc.UltraLabel();
            this.narrationPanel = new Infragistics.Win.Misc.UltraPanel();
            this.lblNarration = new Infragistics.Win.Misc.UltraLabel();
            this.txtNarration = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.headerPanel.ClientArea.SuspendLayout();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtpVoucherDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CmboBranch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CmboCashBank)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtVoucherNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridPayment)).BeginInit();
            this.footerPanel.ClientArea.SuspendLayout();
            this.footerPanel.SuspendLayout();
            this.narrationPanel.ClientArea.SuspendLayout();
            this.narrationPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtNarration)).BeginInit();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(205, 229, 236);
            appearance1.FontData.BoldAsString = "True";
            appearance1.FontData.SizeInPoints = 18F;
            appearance1.ForeColor = System.Drawing.Color.FromArgb(8, 47, 73);
            appearance1.TextHAlignAsString = "Left";
            appearance1.TextVAlignAsString = "Middle";
            this.lblHeader.Appearance = appearance1;
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Padding = new System.Drawing.Size(28, 0);
            this.lblHeader.Size = new System.Drawing.Size(1215, 50);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "General Payment Voucher";
            // 
            // headerPanel
            // 
            appearance2.BackColor = System.Drawing.Color.FromArgb(248, 251, 252);
            this.headerPanel.Appearance = appearance2;
            // 
            // headerPanel.ClientArea
            // 
            this.headerPanel.ClientArea.Controls.Add(this.CmboCashBank);
            this.headerPanel.ClientArea.Controls.Add(this.lblCashBank);
            this.headerPanel.ClientArea.Controls.Add(this.dtpVoucherDate);
            this.headerPanel.ClientArea.Controls.Add(this.lblBranch);
            this.headerPanel.ClientArea.Controls.Add(this.lblVoucherDate);
            this.headerPanel.ClientArea.Controls.Add(this.CmboBranch);
            this.headerPanel.ClientArea.Controls.Add(this.lblVocuherNo);
            this.headerPanel.ClientArea.Controls.Add(this.txtVoucherNo);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 50);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(1215, 92);
            this.headerPanel.TabIndex = 1;
            // 
            // dtpVoucherDate
            // 
            appearance3.BackColor = System.Drawing.Color.White;
            appearance3.ForeColor = System.Drawing.Color.FromArgb(31, 42, 55);
            this.dtpVoucherDate.Appearance = appearance3;
            this.dtpVoucherDate.DateTime = new System.DateTime(2026, 5, 22, 0, 0, 0, 0);
            this.dtpVoucherDate.Location = new System.Drawing.Point(362, 39);
            this.dtpVoucherDate.Name = "dtpVoucherDate";
            this.dtpVoucherDate.Size = new System.Drawing.Size(150, 21);
            this.dtpVoucherDate.TabIndex = 3;
            this.dtpVoucherDate.Value = new System.DateTime(2026, 5, 22, 0, 0, 0, 0);
            // 
            // lblBranch
            // 
            this.lblBranch.AutoSize = true;
            this.lblBranch.Location = new System.Drawing.Point(536, 14);
            this.lblBranch.Name = "lblBranch";
            this.lblBranch.Size = new System.Drawing.Size(44, 15);
            this.lblBranch.TabIndex = 4;
            this.lblBranch.Text = "Branch";
            // 
            // lblVoucherDate
            // 
            this.lblVoucherDate.AutoSize = true;
            this.lblVoucherDate.Location = new System.Drawing.Point(362, 14);
            this.lblVoucherDate.Name = "lblVoucherDate";
            this.lblVoucherDate.Size = new System.Drawing.Size(77, 15);
            this.lblVoucherDate.TabIndex = 2;
            this.lblVoucherDate.Text = "Voucher Date";
            // 
            // CmboBranch
            // 
            appearance4.BackColor = System.Drawing.Color.White;
            appearance4.ForeColor = System.Drawing.Color.FromArgb(31, 42, 55);
            this.CmboBranch.Appearance = appearance4;
            this.CmboBranch.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
            this.CmboBranch.Location = new System.Drawing.Point(536, 39);
            this.CmboBranch.Name = "CmboBranch";
            this.CmboBranch.Size = new System.Drawing.Size(260, 21);
            this.CmboBranch.TabIndex = 4;
            this.CmboBranch.ValueChanged += new System.EventHandler(this.CmboBranch_ValueChanged);
            // 
            // lblCashBank
            // 
            this.lblCashBank.AutoSize = true;
            this.lblCashBank.Location = new System.Drawing.Point(820, 14);
            this.lblCashBank.Name = "lblCashBank";
            this.lblCashBank.Size = new System.Drawing.Size(130, 15);
            this.lblCashBank.TabIndex = 5;
            this.lblCashBank.Text = "Paid From (Cash/Bank)";
            // 
            // CmboCashBank
            // 
            appearanceCashBank.BackColor = System.Drawing.Color.White;
            appearanceCashBank.ForeColor = System.Drawing.Color.FromArgb(31, 42, 55);
            this.CmboCashBank.Appearance = appearanceCashBank;
            this.CmboCashBank.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
            this.CmboCashBank.Location = new System.Drawing.Point(820, 39);
            this.CmboCashBank.Name = "CmboCashBank";
            this.CmboCashBank.Size = new System.Drawing.Size(280, 21);
            this.CmboCashBank.TabIndex = 5;
            // 
            // lblVocuherNo
            // 
            this.lblVocuherNo.AutoSize = true;
            this.lblVocuherNo.Location = new System.Drawing.Point(28, 14);
            this.lblVocuherNo.Name = "lblVocuherNo";
            this.lblVocuherNo.Size = new System.Drawing.Size(69, 15);
            this.lblVocuherNo.TabIndex = 0;
            this.lblVocuherNo.Text = "Voucher No.";
            // 
            // txtVoucherNo
            // 
            appearance5.BackColor = System.Drawing.Color.White;
            appearance5.FontData.BoldAsString = "True";
            appearance5.ForeColor = System.Drawing.Color.FromArgb(31, 42, 55);
            this.txtVoucherNo.Appearance = appearance5;
            this.txtVoucherNo.Location = new System.Drawing.Point(28, 39);
            this.txtVoucherNo.Name = "txtVoucherNo";
            this.txtVoucherNo.Size = new System.Drawing.Size(310, 21);
            this.txtVoucherNo.TabIndex = 1;
            // 
            // gridPayment
            // 
            appearance6.BackColor = System.Drawing.Color.White;
            this.gridPayment.DisplayLayout.Appearance = appearance6;
            this.gridPayment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPayment.Location = new System.Drawing.Point(0, 142);
            this.gridPayment.Name = "gridPayment";
            this.gridPayment.DisplayLayout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;
            this.gridPayment.DisplayLayout.GroupByBox.Hidden = true;
            this.gridPayment.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;
            this.gridPayment.Size = new System.Drawing.Size(1215, 253);
            this.gridPayment.TabIndex = 2;
            this.gridPayment.Text = "";
            // 
            // footerPanel
            // 
            appearance7.BackColor = System.Drawing.Color.FromArgb(248, 251, 252);
            this.footerPanel.Appearance = appearance7;
            // 
            // footerPanel.ClientArea
            // 
            this.footerPanel.ClientArea.Controls.Add(this.lblDifferenceValue);
            this.footerPanel.ClientArea.Controls.Add(this.lblDifference);
            this.footerPanel.ClientArea.Controls.Add(this.lblTotalCreditValue);
            this.footerPanel.ClientArea.Controls.Add(this.lblTotalCredit);
            this.footerPanel.ClientArea.Controls.Add(this.lblTotalDebitValue);
            this.footerPanel.ClientArea.Controls.Add(this.lblTotalDebit);
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footerPanel.Location = new System.Drawing.Point(0, 495);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(1215, 78);
            this.footerPanel.TabIndex = 4;
            // 
            // lblDifferenceValue
            // 
            appearance8.FontData.BoldAsString = "True";
            appearance8.FontData.SizeInPoints = 13F;
            appearance8.ForeColor = System.Drawing.Color.FromArgb(46, 125, 50);
            appearance8.TextHAlignAsString = "Right";
            this.lblDifferenceValue.Appearance = appearance8;
            this.lblDifferenceValue.Location = new System.Drawing.Point(920, 36);
            this.lblDifferenceValue.Name = "lblDifferenceValue";
            this.lblDifferenceValue.Size = new System.Drawing.Size(260, 30);
            this.lblDifferenceValue.TabIndex = 5;
            this.lblDifferenceValue.Text = "0.00";
            // 
            // lblDifference
            // 
            this.lblDifference.AutoSize = true;
            this.lblDifference.Location = new System.Drawing.Point(920, 16);
            this.lblDifference.Name = "lblDifference";
            this.lblDifference.Size = new System.Drawing.Size(78, 15);
            this.lblDifference.TabIndex = 4;
            this.lblDifference.Text = "Total Amount";
            // 
            // lblTotalCreditValue
            // 
            appearance9.FontData.BoldAsString = "True";
            appearance9.FontData.SizeInPoints = 13F;
            appearance9.ForeColor = System.Drawing.Color.FromArgb(31, 42, 55);
            appearance9.TextHAlignAsString = "Right";
            this.lblTotalCreditValue.Appearance = appearance9;
            this.lblTotalCreditValue.Location = new System.Drawing.Point(620, 36);
            this.lblTotalCreditValue.Name = "lblTotalCreditValue";
            this.lblTotalCreditValue.Size = new System.Drawing.Size(260, 30);
            this.lblTotalCreditValue.TabIndex = 3;
            this.lblTotalCreditValue.Text = "0.00";
            this.lblTotalCreditValue.Visible = false;
            // 
            // lblTotalCredit
            // 
            this.lblTotalCredit.AutoSize = true;
            this.lblTotalCredit.Location = new System.Drawing.Point(620, 16);
            this.lblTotalCredit.Name = "lblTotalCredit";
            this.lblTotalCredit.Size = new System.Drawing.Size(68, 15);
            this.lblTotalCredit.TabIndex = 2;
            this.lblTotalCredit.Text = "Total Credit";
            this.lblTotalCredit.Visible = false;
            // 
            // lblTotalDebitValue
            // 
            appearance10.FontData.BoldAsString = "True";
            appearance10.FontData.SizeInPoints = 13F;
            appearance10.ForeColor = System.Drawing.Color.FromArgb(31, 42, 55);
            appearance10.TextHAlignAsString = "Right";
            this.lblTotalDebitValue.Appearance = appearance10;
            this.lblTotalDebitValue.Location = new System.Drawing.Point(320, 36);
            this.lblTotalDebitValue.Name = "lblTotalDebitValue";
            this.lblTotalDebitValue.Size = new System.Drawing.Size(260, 30);
            this.lblTotalDebitValue.TabIndex = 1;
            this.lblTotalDebitValue.Text = "0.00";
            this.lblTotalDebitValue.Visible = false;
            // 
            // lblTotalDebit
            // 
            this.lblTotalDebit.AutoSize = true;
            this.lblTotalDebit.Location = new System.Drawing.Point(320, 16);
            this.lblTotalDebit.Name = "lblTotalDebit";
            this.lblTotalDebit.Size = new System.Drawing.Size(64, 15);
            this.lblTotalDebit.TabIndex = 0;
            this.lblTotalDebit.Text = "Total Debit";
            this.lblTotalDebit.Visible = false;
            // 
            // narrationPanel
            // 
            this.narrationPanel.ClientArea.Controls.Add(this.lblNarration);
            this.narrationPanel.ClientArea.Controls.Add(this.txtNarration);
            this.narrationPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.narrationPanel.Location = new System.Drawing.Point(0, 395);
            this.narrationPanel.Name = "narrationPanel";
            this.narrationPanel.Size = new System.Drawing.Size(1215, 100);
            this.narrationPanel.TabIndex = 3;
            // 
            // lblNarration
            // 
            this.lblNarration.AutoSize = true;
            this.lblNarration.Location = new System.Drawing.Point(28, 10);
            this.lblNarration.Name = "lblNarration";
            this.lblNarration.Size = new System.Drawing.Size(71, 15);
            this.lblNarration.TabIndex = 0;
            this.lblNarration.Text = "Description";
            // 
            // txtNarration
            // 
            this.txtNarration.AlwaysInEditMode = true;
            this.txtNarration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNarration.Location = new System.Drawing.Point(28, 31);
            this.txtNarration.Multiline = true;
            this.txtNarration.Name = "txtNarration";
            this.txtNarration.Scrollbars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtNarration.Size = new System.Drawing.Size(1159, 58);
            this.txtNarration.TabIndex = 1;
            // 
            // FrmGeneralPayment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(247)))));
            this.ClientSize = new System.Drawing.Size(1215, 573);
            this.Controls.Add(this.gridPayment);
            this.Controls.Add(this.narrationPanel);
            this.Controls.Add(this.footerPanel);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.lblHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "FrmGeneralPayment";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "General Payment";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.headerPanel.ClientArea.ResumeLayout(false);
            this.headerPanel.ClientArea.PerformLayout();
            this.headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtpVoucherDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CmboBranch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CmboCashBank)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtVoucherNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridPayment)).EndInit();
            this.footerPanel.ClientArea.ResumeLayout(false);
            this.footerPanel.ClientArea.PerformLayout();
            this.footerPanel.ResumeLayout(false);
            this.narrationPanel.ClientArea.ResumeLayout(false);
            this.narrationPanel.ClientArea.PerformLayout();
            this.narrationPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtNarration)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private Infragistics.Win.Misc.UltraLabel lblHeader;
        private Infragistics.Win.Misc.UltraPanel headerPanel;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpVoucherDate;
        private Infragistics.Win.Misc.UltraLabel lblBranch;
        private Infragistics.Win.Misc.UltraLabel lblVoucherDate;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor CmboBranch;
        private Infragistics.Win.Misc.UltraLabel lblCashBank;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor CmboCashBank;
        private Infragistics.Win.Misc.UltraLabel lblVocuherNo;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtVoucherNo;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridPayment;
        private Infragistics.Win.Misc.UltraPanel footerPanel;
        private Infragistics.Win.Misc.UltraLabel lblDifferenceValue;
        private Infragistics.Win.Misc.UltraLabel lblDifference;
        private Infragistics.Win.Misc.UltraLabel lblTotalCreditValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalCredit;
        private Infragistics.Win.Misc.UltraLabel lblTotalDebitValue;
        private Infragistics.Win.Misc.UltraLabel lblTotalDebit;
        private Infragistics.Win.Misc.UltraPanel narrationPanel;
        private Infragistics.Win.Misc.UltraLabel lblNarration;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtNarration;
    }
}
