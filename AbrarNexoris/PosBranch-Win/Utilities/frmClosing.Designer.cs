
namespace PosBranch_Win.Utilities
{
    partial class frmClosing
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
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmClosing));
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            this.ultraPanel1 = new Infragistics.Win.Misc.UltraPanel();
            this.btnClosingHistory = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.txtPurchaseNo = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblDocNo = new Infragistics.Win.Misc.UltraLabel();
            this.cboReportSelection = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblReportSelection = new Infragistics.Win.Misc.UltraLabel();
            this.dtpDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblDate = new Infragistics.Win.Misc.UltraLabel();
            this.txtCounter = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblCounter = new Infragistics.Win.Misc.UltraLabel();
            this.txtTotal = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblTotal = new Infragistics.Win.Misc.UltraLabel();
            this.grpCashCalculation = new Infragistics.Win.Misc.UltraGroupBox();
            this.gridCash = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanel1.ClientArea.SuspendLayout();
            this.ultraPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtPurchaseNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboReportSelection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCounter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTotal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpCashCalculation)).BeginInit();
            this.grpCashCalculation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCash)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraPanel1
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(236)))), ((int)(((byte)(255)))));
            this.ultraPanel1.Appearance = appearance1;
            // 
            // ultraPanel1.ClientArea
            // 
            this.ultraPanel1.ClientArea.Controls.Add(this.btnClosingHistory);
            this.ultraPanel1.ClientArea.Controls.Add(this.txtPurchaseNo);
            this.ultraPanel1.ClientArea.Controls.Add(this.lblDocNo);
            this.ultraPanel1.ClientArea.Controls.Add(this.cboReportSelection);
            this.ultraPanel1.ClientArea.Controls.Add(this.lblReportSelection);
            this.ultraPanel1.ClientArea.Controls.Add(this.dtpDate);
            this.ultraPanel1.ClientArea.Controls.Add(this.lblDate);
            this.ultraPanel1.ClientArea.Controls.Add(this.txtCounter);
            this.ultraPanel1.ClientArea.Controls.Add(this.lblCounter);
            this.ultraPanel1.ClientArea.Controls.Add(this.txtTotal);
            this.ultraPanel1.ClientArea.Controls.Add(this.lblTotal);
            this.ultraPanel1.ClientArea.Controls.Add(this.grpCashCalculation);
            this.ultraPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanel1.Location = new System.Drawing.Point(0, 0);
            this.ultraPanel1.Name = "ultraPanel1";
            this.ultraPanel1.Size = new System.Drawing.Size(1180, 690);
            this.ultraPanel1.TabIndex = 0;
            // 
            // btnClosingHistory
            // 
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(210)))), ((int)(((byte)(255)))));
            this.btnClosingHistory.Appearance = appearance2;
            this.btnClosingHistory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(210)))), ((int)(((byte)(255)))));
            this.btnClosingHistory.BackColorInternal = System.Drawing.Color.AliceBlue;
            this.btnClosingHistory.BorderShadowColor = System.Drawing.Color.Empty;
            this.btnClosingHistory.Image = ((object)(resources.GetObject("btnClosingHistory.Image")));
            this.btnClosingHistory.Location = new System.Drawing.Point(1112, 32);
            this.btnClosingHistory.Name = "btnClosingHistory";
            this.btnClosingHistory.Size = new System.Drawing.Size(26, 17);
            this.btnClosingHistory.TabIndex = 72;
            // 
            // txtPurchaseNo
            // 
            appearance3.BackColor = System.Drawing.Color.White;
            appearance3.BorderColor = System.Drawing.Color.DeepSkyBlue;
            this.txtPurchaseNo.Appearance = appearance3;
            this.txtPurchaseNo.BackColor = System.Drawing.Color.White;
            this.txtPurchaseNo.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.txtPurchaseNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.txtPurchaseNo.Location = new System.Drawing.Point(938, 28);
            this.txtPurchaseNo.Multiline = true;
            this.txtPurchaseNo.Name = "txtPurchaseNo";
            this.txtPurchaseNo.ReadOnly = true;
            this.txtPurchaseNo.Size = new System.Drawing.Size(160, 21);
            this.txtPurchaseNo.TabIndex = 71;
            this.txtPurchaseNo.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // lblDocNo
            // 
            this.lblDocNo.AutoSize = true;
            this.lblDocNo.Location = new System.Drawing.Point(885, 32);
            this.lblDocNo.Name = "lblDocNo";
            this.lblDocNo.Size = new System.Drawing.Size(45, 14);
            this.lblDocNo.TabIndex = 7;
            this.lblDocNo.Text = "Doc No:";
            // 
            // cboReportSelection
            // 
            this.cboReportSelection.Location = new System.Drawing.Point(476, 31);
            this.cboReportSelection.Name = "cboReportSelection";
            this.cboReportSelection.Size = new System.Drawing.Size(180, 21);
            this.cboReportSelection.TabIndex = 6;
            this.cboReportSelection.Text = "Shift Collection";
            // 
            // lblReportSelection
            // 
            this.lblReportSelection.AutoSize = true;
            this.lblReportSelection.Location = new System.Drawing.Point(370, 34);
            this.lblReportSelection.Name = "lblReportSelection";
            this.lblReportSelection.Size = new System.Drawing.Size(91, 14);
            this.lblReportSelection.TabIndex = 5;
            this.lblReportSelection.Text = "Report Selection:";
            // 
            // dtpDate
            // 
            this.dtpDate.DateTime = new System.DateTime(2025, 12, 5, 9, 57, 2, 514);
            this.dtpDate.Location = new System.Drawing.Point(177, 65);
            this.dtpDate.Name = "dtpDate";
            this.dtpDate.Size = new System.Drawing.Size(160, 21);
            this.dtpDate.TabIndex = 4;
            this.dtpDate.Value = new System.DateTime(2025, 12, 5, 9, 57, 2, 514);
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Location = new System.Drawing.Point(32, 68);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(94, 14);
            this.lblDate.TabIndex = 3;
            this.lblDate.Text = "Transaction Date:";
            // 
            // txtCounter
            // 
            this.txtCounter.Location = new System.Drawing.Point(177, 31);
            this.txtCounter.Name = "txtCounter";
            this.txtCounter.ReadOnly = true;
            this.txtCounter.Size = new System.Drawing.Size(160, 21);
            this.txtCounter.TabIndex = 2;
            // 
            // lblCounter
            // 
            this.lblCounter.AutoSize = true;
            this.lblCounter.Location = new System.Drawing.Point(32, 34);
            this.lblCounter.Name = "lblCounter";
            this.lblCounter.Size = new System.Drawing.Size(48, 14);
            this.lblCounter.TabIndex = 1;
            this.lblCounter.Text = "Counter:";
            // 
            // txtTotal
            // 
            appearance4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(220)))));
            appearance4.FontData.BoldAsString = "True";
            appearance4.FontData.SizeInPoints = 14F;
            appearance4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(0)))));
            appearance4.TextHAlignAsString = "Right";
            this.txtTotal.Appearance = appearance4;
            this.txtTotal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(220)))));
            this.txtTotal.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.txtTotal.Location = new System.Drawing.Point(229, 618);
            this.txtTotal.Name = "txtTotal";
            this.txtTotal.ReadOnly = true;
            this.txtTotal.Size = new System.Drawing.Size(210, 35);
            this.txtTotal.TabIndex = 11;
            this.txtTotal.Text = "$0.00";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTotal.Location = new System.Drawing.Point(32, 623);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(102, 24);
            this.lblTotal.TabIndex = 10;
            this.lblTotal.Text = "Grand Total:";
            // grpCashCalculation
            // 
            this.grpCashCalculation.Controls.Add(this.gridCash);
            this.grpCashCalculation.Location = new System.Drawing.Point(32, 126);
            this.grpCashCalculation.Name = "grpCashCalculation";
            this.grpCashCalculation.Size = new System.Drawing.Size(642, 474);
            this.grpCashCalculation.TabIndex = 0;
            this.grpCashCalculation.Text = "Cash Calculation";
            // 
            // gridCash
            // 
            this.gridCash.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridCash.Location = new System.Drawing.Point(3, 16);
            this.gridCash.Name = "gridCash";
            this.gridCash.Size = new System.Drawing.Size(636, 455);
            this.gridCash.TabIndex = 0;
            // 
            // frmClosing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1180, 690);
            this.Controls.Add(this.ultraPanel1);
            this.Name = "frmClosing";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cash Closing";
            this.ultraPanel1.ClientArea.ResumeLayout(false);
            this.ultraPanel1.ClientArea.PerformLayout();
            this.ultraPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtPurchaseNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboReportSelection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCounter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTotal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpCashCalculation)).EndInit();
            this.grpCashCalculation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridCash)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanel1;
        private Infragistics.Win.Misc.UltraGroupBox grpCashCalculation;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridCash;
        private Infragistics.Win.Misc.UltraLabel lblCounter;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtCounter;
        private Infragistics.Win.Misc.UltraLabel lblDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpDate;
        private Infragistics.Win.Misc.UltraLabel lblReportSelection;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cboReportSelection;
        private Infragistics.Win.Misc.UltraLabel lblDocNo;
        private Infragistics.Win.Misc.UltraLabel lblTotal;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtTotal;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox btnClosingHistory;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtPurchaseNo;
    }
}
