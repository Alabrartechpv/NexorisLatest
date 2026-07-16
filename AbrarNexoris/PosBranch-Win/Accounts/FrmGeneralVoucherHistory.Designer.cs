namespace PosBranch_Win.Accounts
{
    partial class FrmGeneralVoucherHistory
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
            this.lblTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblSearchBy = new Infragistics.Win.Misc.UltraLabel();
            this.cmbSearchBy = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblSortBy = new Infragistics.Win.Misc.UltraLabel();
            this.cmbSortBy = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.gridHistory = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.btnSelect = new Infragistics.Win.Misc.UltraButton();
            this.btnCancel = new Infragistics.Win.Misc.UltraButton();
            this.lblCount = new Infragistics.Win.Misc.UltraLabel();
            
            ((System.ComponentModel.ISupportInitialize)(this.cmbSearchBy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbSortBy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridHistory)).BeginInit();
            this.SuspendLayout();

            // 
            // FrmGeneralVoucherHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(780, 520);
            
            // Add Controls
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSearchBy);
            this.Controls.Add(this.cmbSearchBy);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.lblSortBy);
            this.Controls.Add(this.cmbSortBy);
            this.Controls.Add(this.gridHistory);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblCount);

            this.Name = "FrmGeneralVoucherHistory";
            this.Text = "Voucher History";

            ((System.ComponentModel.ISupportInitialize)(this.cmbSearchBy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbSortBy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridHistory)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Infragistics.Win.Misc.UltraLabel lblTitle;
        private Infragistics.Win.Misc.UltraLabel lblSearchBy;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbSearchBy;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblSortBy;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbSortBy;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridHistory;
        private Infragistics.Win.Misc.UltraButton btnSelect;
        private Infragistics.Win.Misc.UltraButton btnCancel;
        private Infragistics.Win.Misc.UltraLabel lblCount;
    }
}
