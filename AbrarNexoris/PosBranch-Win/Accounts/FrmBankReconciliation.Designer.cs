namespace PosBranch_Win.Accounts
{
    partial class FrmBankReconciliation
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

        private void InitializeComponent()
        {
            this.headerPanel = new Infragistics.Win.Misc.UltraPanel();
            this.lblHeader = new Infragistics.Win.Misc.UltraLabel();
            this.lblBankAccount = new Infragistics.Win.Misc.UltraLabel();
            this.cmbBankAccount = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtpFromDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtpToDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.btnLoad = new Infragistics.Win.Misc.UltraButton();
            this.lblBooksBalanceTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblBooksBalanceValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblUnclearedReceiptsTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblUnclearedReceiptsValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblUnclearedPaymentsTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblUnclearedPaymentsValue = new Infragistics.Win.Misc.UltraLabel();
            this.lblBankBalanceTitle = new Infragistics.Win.Misc.UltraLabel();
            this.lblBankBalanceValue = new Infragistics.Win.Misc.UltraLabel();
            this.gridReconciliation = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.btnSave = new Infragistics.Win.Misc.UltraButton();
            this.btnClear = new Infragistics.Win.Misc.UltraButton();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();
            this.btnReconcileAll = new Infragistics.Win.Misc.UltraButton();
            this.lblStatus = new Infragistics.Win.Misc.UltraLabel();
            this.headerPanel.ClientArea.SuspendLayout();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbBankAccount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpFromDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpToDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridReconciliation)).BeginInit();
            this.SuspendLayout();
            // 
            // headerPanel
            // 
            this.headerPanel.ClientArea.Controls.Add(this.lblHeader);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(950, 56);
            this.headerPanel.TabIndex = 0;
            this.headerPanel.Appearance.BackColor = System.Drawing.Color.FromArgb(18, 65, 89);
            this.headerPanel.Appearance.BackColor2 = System.Drawing.Color.FromArgb(28, 85, 110);
            this.headerPanel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.headerPanel.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(950, 56);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Bank Reconciliation";
            this.lblHeader.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.lblHeader.Appearance.FontData.SizeInPoints = 16F;
            this.lblHeader.Appearance.TextVAlign = Infragistics.Win.VAlign.Middle;
            this.lblHeader.Padding = new System.Drawing.Size(24, 0);
            // 
            // lblBankAccount
            // 
            this.lblBankAccount.Location = new System.Drawing.Point(24, 72);
            this.lblBankAccount.Name = "lblBankAccount";
            this.lblBankAccount.Size = new System.Drawing.Size(120, 20);
            this.lblBankAccount.TabIndex = 1;
            this.lblBankAccount.Text = "Bank Account";
            this.lblBankAccount.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblBankAccount.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            // 
            // cmbBankAccount
            // 
            this.cmbBankAccount.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbBankAccount.Location = new System.Drawing.Point(24, 92);
            this.cmbBankAccount.Name = "cmbBankAccount";
            this.cmbBankAccount.Size = new System.Drawing.Size(240, 28);
            this.cmbBankAccount.TabIndex = 2;
            this.cmbBankAccount.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(284, 72);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(120, 20);
            this.lblFromDate.TabIndex = 3;
            this.lblFromDate.Text = "From Date";
            this.lblFromDate.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblFromDate.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            // 
            // dtpFromDate
            // 
            this.dtpFromDate.Location = new System.Drawing.Point(284, 92);
            this.dtpFromDate.Name = "dtpFromDate";
            this.dtpFromDate.Size = new System.Drawing.Size(140, 28);
            this.dtpFromDate.TabIndex = 4;
            this.dtpFromDate.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(444, 72);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(120, 20);
            this.lblToDate.TabIndex = 5;
            this.lblToDate.Text = "To Date";
            this.lblToDate.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblToDate.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            // 
            // dtpToDate
            // 
            this.dtpToDate.Location = new System.Drawing.Point(444, 92);
            this.dtpToDate.Name = "dtpToDate";
            this.dtpToDate.Size = new System.Drawing.Size(140, 28);
            this.dtpToDate.TabIndex = 6;
            this.dtpToDate.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(604, 92);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(90, 28);
            this.btnLoad.TabIndex = 7;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnLoad.UseAppStyling = false;
            this.btnLoad.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnLoad.Appearance.BackColor = System.Drawing.Color.FromArgb(25, 118, 210);
            this.btnLoad.Appearance.BackColor2 = System.Drawing.Color.FromArgb(33, 150, 243);
            this.btnLoad.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.btnLoad.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnLoad.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.btnLoad.Appearance.FontData.SizeInPoints = 9.5F;
            this.btnLoad.Appearance.BorderColor = System.Drawing.Color.FromArgb(21, 101, 192);
            // 
            // lblBooksBalanceTitle
            // 
            this.lblBooksBalanceTitle.Location = new System.Drawing.Point(24, 140);
            this.lblBooksBalanceTitle.Name = "lblBooksBalanceTitle";
            this.lblBooksBalanceTitle.Size = new System.Drawing.Size(150, 20);
            this.lblBooksBalanceTitle.TabIndex = 8;
            this.lblBooksBalanceTitle.Text = "Books Balance:";
            this.lblBooksBalanceTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblBooksBalanceTitle.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            // 
            // lblBooksBalanceValue
            // 
            this.lblBooksBalanceValue.Location = new System.Drawing.Point(24, 162);
            this.lblBooksBalanceValue.Name = "lblBooksBalanceValue";
            this.lblBooksBalanceValue.Size = new System.Drawing.Size(150, 20);
            this.lblBooksBalanceValue.TabIndex = 9;
            this.lblBooksBalanceValue.Text = "0.00";
            this.lblBooksBalanceValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(17, 24, 39);
            this.lblBooksBalanceValue.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.lblBooksBalanceValue.Appearance.FontData.SizeInPoints = 11F;
            // 
            // lblUnclearedReceiptsTitle
            // 
            this.lblUnclearedReceiptsTitle.Location = new System.Drawing.Point(249, 140);
            this.lblUnclearedReceiptsTitle.Name = "lblUnclearedReceiptsTitle";
            this.lblUnclearedReceiptsTitle.Size = new System.Drawing.Size(200, 20);
            this.lblUnclearedReceiptsTitle.TabIndex = 10;
            this.lblUnclearedReceiptsTitle.Text = "(-) Uncleared Receipts:";
            this.lblUnclearedReceiptsTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblUnclearedReceiptsTitle.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            // 
            // lblUnclearedReceiptsValue
            // 
            this.lblUnclearedReceiptsValue.Location = new System.Drawing.Point(249, 162);
            this.lblUnclearedReceiptsValue.Name = "lblUnclearedReceiptsValue";
            this.lblUnclearedReceiptsValue.Size = new System.Drawing.Size(200, 20);
            this.lblUnclearedReceiptsValue.TabIndex = 11;
            this.lblUnclearedReceiptsValue.Text = "0.00";
            this.lblUnclearedReceiptsValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(17, 24, 39);
            this.lblUnclearedReceiptsValue.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.lblUnclearedReceiptsValue.Appearance.FontData.SizeInPoints = 11F;
            // 
            // lblUnclearedPaymentsTitle
            // 
            this.lblUnclearedPaymentsTitle.Location = new System.Drawing.Point(474, 140);
            this.lblUnclearedPaymentsTitle.Name = "lblUnclearedPaymentsTitle";
            this.lblUnclearedPaymentsTitle.Size = new System.Drawing.Size(200, 20);
            this.lblUnclearedPaymentsTitle.TabIndex = 12;
            this.lblUnclearedPaymentsTitle.Text = "(+) Uncleared Payments:";
            this.lblUnclearedPaymentsTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblUnclearedPaymentsTitle.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            // 
            // lblUnclearedPaymentsValue
            // 
            this.lblUnclearedPaymentsValue.Location = new System.Drawing.Point(474, 162);
            this.lblUnclearedPaymentsValue.Name = "lblUnclearedPaymentsValue";
            this.lblUnclearedPaymentsValue.Size = new System.Drawing.Size(200, 20);
            this.lblUnclearedPaymentsValue.TabIndex = 13;
            this.lblUnclearedPaymentsValue.Text = "0.00";
            this.lblUnclearedPaymentsValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(17, 24, 39);
            this.lblUnclearedPaymentsValue.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.lblUnclearedPaymentsValue.Appearance.FontData.SizeInPoints = 11F;
            // 
            // lblBankBalanceTitle
            // 
            this.lblBankBalanceTitle.Location = new System.Drawing.Point(699, 140);
            this.lblBankBalanceTitle.Name = "lblBankBalanceTitle";
            this.lblBankBalanceTitle.Size = new System.Drawing.Size(150, 20);
            this.lblBankBalanceTitle.TabIndex = 14;
            this.lblBankBalanceTitle.Text = "Bank Balance:";
            this.lblBankBalanceTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblBankBalanceTitle.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            // 
            // lblBankBalanceValue
            // 
            this.lblBankBalanceValue.Location = new System.Drawing.Point(699, 162);
            this.lblBankBalanceValue.Name = "lblBankBalanceValue";
            this.lblBankBalanceValue.Size = new System.Drawing.Size(150, 20);
            this.lblBankBalanceValue.TabIndex = 15;
            this.lblBankBalanceValue.Text = "0.00";
            this.lblBankBalanceValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(0, 121, 107);
            this.lblBankBalanceValue.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.lblBankBalanceValue.Appearance.FontData.SizeInPoints = 13F;
            // 
            // gridReconciliation
            // 
            this.gridReconciliation.Location = new System.Drawing.Point(24, 200);
            this.gridReconciliation.Name = "gridReconciliation";
            this.gridReconciliation.Size = new System.Drawing.Size(902, 380);
            this.gridReconciliation.TabIndex = 16;
            this.gridReconciliation.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.False;
            this.gridReconciliation.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;
            this.gridReconciliation.DisplayLayout.Appearance.BackColor = System.Drawing.Color.White;
            this.gridReconciliation.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Dotted;
            this.gridReconciliation.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;
            this.gridReconciliation.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(24, 590);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 36);
            this.btnSave.TabIndex = 17;
            this.btnSave.Text = "Save";
            this.btnSave.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnSave.UseAppStyling = false;
            this.btnSave.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnSave.Appearance.BackColor = System.Drawing.Color.FromArgb(46, 125, 50);
            this.btnSave.Appearance.BackColor2 = System.Drawing.Color.FromArgb(67, 160, 71);
            this.btnSave.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.btnSave.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnSave.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.btnSave.Appearance.FontData.SizeInPoints = 9.5F;
            this.btnSave.Appearance.BorderColor = System.Drawing.Color.FromArgb(27, 94, 32);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(268, 590);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(100, 36);
            this.btnClear.TabIndex = 18;
            this.btnClear.Text = "Clear";
            this.btnClear.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnClear.UseAppStyling = false;
            this.btnClear.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnClear.Appearance.BackColor = System.Drawing.Color.FromArgb(84, 110, 122);
            this.btnClear.Appearance.BackColor2 = System.Drawing.Color.FromArgb(96, 125, 139);
            this.btnClear.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.btnClear.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnClear.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.btnClear.Appearance.FontData.SizeInPoints = 9.5F;
            this.btnClear.Appearance.BorderColor = System.Drawing.Color.FromArgb(69, 90, 100);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(380, 590);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 36);
            this.btnClose.TabIndex = 20;
            this.btnClose.Text = "Close";
            this.btnClose.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnClose.UseAppStyling = false;
            this.btnClose.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnClose.Appearance.BackColor = System.Drawing.Color.FromArgb(84, 110, 122);
            this.btnClose.Appearance.BackColor2 = System.Drawing.Color.FromArgb(96, 125, 139);
            this.btnClose.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.btnClose.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnClose.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.btnClose.Appearance.FontData.SizeInPoints = 9.5F;
            this.btnClose.Appearance.BorderColor = System.Drawing.Color.FromArgb(69, 90, 100);
            // 
            // btnReconcileAll
            // 
            this.btnReconcileAll.Location = new System.Drawing.Point(136, 590);
            this.btnReconcileAll.Name = "btnReconcileAll";
            this.btnReconcileAll.Size = new System.Drawing.Size(120, 36);
            this.btnReconcileAll.TabIndex = 19;
            this.btnReconcileAll.Text = "Reconcile All";
            this.btnReconcileAll.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnReconcileAll.UseAppStyling = false;
            this.btnReconcileAll.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnReconcileAll.Appearance.BackColor = System.Drawing.Color.FromArgb(0, 121, 107);
            this.btnReconcileAll.Appearance.BackColor2 = System.Drawing.Color.FromArgb(0, 150, 136);
            this.btnReconcileAll.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.btnReconcileAll.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnReconcileAll.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.btnReconcileAll.Appearance.FontData.SizeInPoints = 9.5F;
            this.btnReconcileAll.Appearance.BorderColor = System.Drawing.Color.FromArgb(0, 105, 92);
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(500, 598);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(300, 20);
            this.lblStatus.TabIndex = 21;
            this.lblStatus.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblStatus.Appearance.FontData.SizeInPoints = 9F;
            // 
            // FrmBankReconciliation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(950, 650);
            this.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.lblBankAccount);
            this.Controls.Add(this.cmbBankAccount);
            this.Controls.Add(this.lblFromDate);
            this.Controls.Add(this.dtpFromDate);
            this.Controls.Add(this.lblToDate);
            this.Controls.Add(this.dtpToDate);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.lblBooksBalanceTitle);
            this.Controls.Add(this.lblBooksBalanceValue);
            this.Controls.Add(this.lblUnclearedReceiptsTitle);
            this.Controls.Add(this.lblUnclearedReceiptsValue);
            this.Controls.Add(this.lblUnclearedPaymentsTitle);
            this.Controls.Add(this.lblUnclearedPaymentsValue);
            this.Controls.Add(this.lblBankBalanceTitle);
            this.Controls.Add(this.lblBankBalanceValue);
            this.Controls.Add(this.gridReconciliation);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnReconcileAll);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblStatus);
            this.Name = "FrmBankReconciliation";
            this.Text = "Bank Reconciliation";
            this.headerPanel.ClientArea.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmbBankAccount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtpToDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridReconciliation)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Infragistics.Win.Misc.UltraPanel headerPanel;
        private Infragistics.Win.Misc.UltraLabel lblHeader;
        private Infragistics.Win.Misc.UltraLabel lblBankAccount;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbBankAccount;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpFromDate;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtpToDate;
        private Infragistics.Win.Misc.UltraButton btnLoad;
        private Infragistics.Win.Misc.UltraLabel lblBooksBalanceTitle;
        private Infragistics.Win.Misc.UltraLabel lblBooksBalanceValue;
        private Infragistics.Win.Misc.UltraLabel lblUnclearedReceiptsTitle;
        private Infragistics.Win.Misc.UltraLabel lblUnclearedReceiptsValue;
        private Infragistics.Win.Misc.UltraLabel lblUnclearedPaymentsTitle;
        private Infragistics.Win.Misc.UltraLabel lblUnclearedPaymentsValue;
        private Infragistics.Win.Misc.UltraLabel lblBankBalanceTitle;
        private Infragistics.Win.Misc.UltraLabel lblBankBalanceValue;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReconciliation;
        private Infragistics.Win.Misc.UltraButton btnSave;
        private Infragistics.Win.Misc.UltraButton btnClear;
        private Infragistics.Win.Misc.UltraButton btnReconcileAll;
        private Infragistics.Win.Misc.UltraButton btnClose;
        private Infragistics.Win.Misc.UltraLabel lblStatus;
    }
}
