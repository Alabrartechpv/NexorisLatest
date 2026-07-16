namespace PosBranch_Win.Accounts
{
    partial class FrmContra
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
            this.lblHeader = new Infragistics.Win.Misc.UltraLabel();
            this.headerPanel = new Infragistics.Win.Misc.UltraPanel();
            this.dtpVoucherDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblBranch = new Infragistics.Win.Misc.UltraLabel();
            this.lblVoucherDate = new Infragistics.Win.Misc.UltraLabel();
            this.CmboBranch = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblVocuherNo = new Infragistics.Win.Misc.UltraLabel();
            this.txtVoucherNo = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.gridContra = new Infragistics.Win.UltraWinGrid.UltraGrid();
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
            ((System.ComponentModel.ISupportInitialize)(this.txtVoucherNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridContra)).BeginInit();
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
            this.lblHeader.Text = "Contra Voucher";
            // 
            // headerPanel
            // 
            appearance2.BackColor = System.Drawing.Color.FromArgb(248, 251, 252);
            this.headerPanel.Appearance = appearance2;
            // 
            // headerPanel.ClientArea
            // 
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
            // gridContra
            // 
            appearance6.BackColor = System.Drawing.Color.White;
            this.gridContra.DisplayLayout.Appearance = appearance6;
            this.gridContra.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;
            this.gridContra.DisplayLayout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;
            this.gridContra.DisplayLayout.GroupByBox.Hidden = true;
            this.gridContra.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridContra.Location = new System.Drawing.Point(0, 142);
            this.gridContra.Name = "gridContra";
            this.gridContra.Size = new System.Drawing.Size(1215, 253);
            this.gridContra.TabIndex = 2;
            this.gridContra.Text = "";
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
            appearance8.TextVAlignAsString = "Middle";
            this.lblDifferenceValue.Appearance = appearance8;
            this.lblDifferenceValue.Location = new System.Drawing.Point(1017, 36);
            this.lblDifferenceValue.Name = "lblDifferenceValue";
            this.lblDifferenceValue.Size = new System.Drawing.Size(170, 30);
            this.lblDifferenceValue.TabIndex = 10;
            this.lblDifferenceValue.Text = "0.00";
            // 
            // lblDifference
            // 
            this.lblDifference.AutoSize = true;
            this.lblDifference.Location = new System.Drawing.Point(1017, 14);
            this.lblDifference.Name = "lblDifference";
            this.lblDifference.Size = new System.Drawing.Size(60, 15);
            this.lblDifference.TabIndex = 9;
            this.lblDifference.Text = "Difference";
            // 
            // lblTotalCreditValue
            // 
            this.lblTotalCreditValue.Appearance = appearance8;
            this.lblTotalCreditValue.Location = new System.Drawing.Point(813, 36);
            this.lblTotalCreditValue.Name = "lblTotalCreditValue";
            this.lblTotalCreditValue.Size = new System.Drawing.Size(170, 30);
            this.lblTotalCreditValue.TabIndex = 7;
            this.lblTotalCreditValue.Text = "0.00";
            // 
            // lblTotalCredit
            // 
            this.lblTotalCredit.AutoSize = true;
            this.lblTotalCredit.Location = new System.Drawing.Point(813, 14);
            this.lblTotalCredit.Name = "lblTotalCredit";
            this.lblTotalCredit.Size = new System.Drawing.Size(66, 15);
            this.lblTotalCredit.TabIndex = 6;
            this.lblTotalCredit.Text = "Total Credit";
            // 
            // lblTotalDebitValue
            // 
            this.lblTotalDebitValue.Appearance = appearance8;
            this.lblTotalDebitValue.Location = new System.Drawing.Point(609, 36);
            this.lblTotalDebitValue.Name = "lblTotalDebitValue";
            this.lblTotalDebitValue.Size = new System.Drawing.Size(170, 30);
            this.lblTotalDebitValue.TabIndex = 5;
            this.lblTotalDebitValue.Text = "0.00";
            // 
            // lblTotalDebit
            // 
            this.lblTotalDebit.AutoSize = true;
            this.lblTotalDebit.Location = new System.Drawing.Point(609, 14);
            this.lblTotalDebit.Name = "lblTotalDebit";
            this.lblTotalDebit.Size = new System.Drawing.Size(65, 15);
            this.lblTotalDebit.TabIndex = 4;
            this.lblTotalDebit.Text = "Total Debit";
            // 
            // narrationPanel
            // 
            appearance9.BackColor = System.Drawing.Color.FromArgb(248, 251, 252);
            this.narrationPanel.Appearance = appearance9;
            // 
            // narrationPanel.ClientArea
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
            this.lblNarration.Size = new System.Drawing.Size(80, 15);
            this.lblNarration.TabIndex = 0;
            this.lblNarration.Text = "Main Narration";
            // 
            // txtNarration
            // 
            appearance10.BackColor = System.Drawing.Color.White;
            appearance10.ForeColor = System.Drawing.Color.FromArgb(31, 42, 55);
            this.txtNarration.Appearance = appearance10;
            this.txtNarration.Location = new System.Drawing.Point(28, 33);
            this.txtNarration.Multiline = true;
            this.txtNarration.Name = "txtNarration";
            this.txtNarration.Size = new System.Drawing.Size(1159, 52);
            this.txtNarration.TabIndex = 5;
            // 
            // FrmContra
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(236, 244, 247);
            this.ClientSize = new System.Drawing.Size(1215, 573);
            this.Controls.Add(this.gridContra);
            this.Controls.Add(this.narrationPanel);
            this.Controls.Add(this.footerPanel);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.lblHeader);
            this.Name = "FrmContra";
            this.Text = "Contra Voucher";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmContra_Load);
            this.headerPanel.ClientArea.ResumeLayout(false);
            this.headerPanel.ClientArea.PerformLayout();
            this.headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtpVoucherDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CmboBranch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtVoucherNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridContra)).EndInit();
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
        public Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpVoucherDate;
        private Infragistics.Win.Misc.UltraLabel lblBranch;
        private Infragistics.Win.Misc.UltraLabel lblVoucherDate;
        public Infragistics.Win.UltraWinEditors.UltraComboEditor CmboBranch;
        private Infragistics.Win.Misc.UltraLabel lblVocuherNo;
        public Infragistics.Win.UltraWinEditors.UltraTextEditor txtVoucherNo;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridContra;
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
