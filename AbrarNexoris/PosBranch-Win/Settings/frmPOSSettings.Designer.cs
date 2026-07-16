
namespace PosBranch_Win.Settings
{
    partial class frmPOSSettings
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
            this.grpSalesSettings = new Infragistics.Win.Misc.UltraGroupBox();
            this.numMaxDiscountPercent = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.lblMaxDiscountPercent = new Infragistics.Win.Misc.UltraLabel();
            this.cmbRoundingMethod = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblRoundingMethod = new Infragistics.Win.Misc.UltraLabel();
            this.cmbDefaultSaleType = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblDefaultSaleType = new Infragistics.Win.Misc.UltraLabel();
            this.cmbDefaultPriceLevel = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblDefaultPriceLevel = new Infragistics.Win.Misc.UltraLabel();
            this.cmbDefaultCreditDays = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblDefaultCreditDays = new Infragistics.Win.Misc.UltraLabel();
            this.chkAllowNegativeStock = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
            this.chkAllowSaleBelowCost = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
            this.cmbDuplicateItemBehavior = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblDuplicateItemBehavior = new Infragistics.Win.Misc.UltraLabel();
            this.grpDisplaySettings = new Infragistics.Win.Misc.UltraGroupBox();
            this.chkShowMarginColumn = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
            this.chkShowCostToUser = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
            this.grpPrintingSettings = new Infragistics.Win.Misc.UltraGroupBox();
            this.numPrintCopies = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();
            this.lblPrintCopies = new Infragistics.Win.Misc.UltraLabel();
            this.chkAutoPrintAfterSave = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
            this.btnSave = new Infragistics.Win.Misc.UltraButton();
            this.btnCancel = new Infragistics.Win.Misc.UltraButton();
            this.btnResetDefaults = new Infragistics.Win.Misc.UltraButton();
            this.lblCurrentBranch = new Infragistics.Win.Misc.UltraLabel();
            this.panelHeader = new Infragistics.Win.Misc.UltraPanel();
            this.lblTitle = new Infragistics.Win.Misc.UltraLabel();
            ((System.ComponentModel.ISupportInitialize)(this.grpSalesSettings)).BeginInit();
            this.grpSalesSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxDiscountPercent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbRoundingMethod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDefaultSaleType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDefaultPriceLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDefaultCreditDays)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAllowNegativeStock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAllowSaleBelowCost)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDuplicateItemBehavior)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpDisplaySettings)).BeginInit();
            this.grpDisplaySettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowMarginColumn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowCostToUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpPrintingSettings)).BeginInit();
            this.grpPrintingSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPrintCopies)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAutoPrintAfterSave)).BeginInit();
            this.panelHeader.ClientArea.SuspendLayout();
            this.panelHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpSalesSettings
            // 
            this.grpSalesSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSalesSettings.Controls.Add(this.numMaxDiscountPercent);
            this.grpSalesSettings.Controls.Add(this.lblMaxDiscountPercent);
            this.grpSalesSettings.Controls.Add(this.cmbRoundingMethod);
            this.grpSalesSettings.Controls.Add(this.lblRoundingMethod);
            this.grpSalesSettings.Controls.Add(this.cmbDefaultSaleType);
            this.grpSalesSettings.Controls.Add(this.lblDefaultSaleType);
            this.grpSalesSettings.Controls.Add(this.cmbDefaultPriceLevel);
            this.grpSalesSettings.Controls.Add(this.lblDefaultPriceLevel);
            this.grpSalesSettings.Controls.Add(this.cmbDefaultCreditDays);
            this.grpSalesSettings.Controls.Add(this.lblDefaultCreditDays);
            this.grpSalesSettings.Controls.Add(this.chkAllowNegativeStock);
            this.grpSalesSettings.Controls.Add(this.chkAllowSaleBelowCost);
            this.grpSalesSettings.Controls.Add(this.cmbDuplicateItemBehavior);
            this.grpSalesSettings.Controls.Add(this.lblDuplicateItemBehavior);
            this.grpSalesSettings.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpSalesSettings.Location = new System.Drawing.Point(15, 80);
            this.grpSalesSettings.Name = "grpSalesSettings";
            this.grpSalesSettings.Size = new System.Drawing.Size(460, 305);
            this.grpSalesSettings.TabIndex = 1;
            this.grpSalesSettings.Text = "Sales Settings";
            // 
            // numMaxDiscountPercent
            // 
            this.numMaxDiscountPercent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.numMaxDiscountPercent.Location = new System.Drawing.Point(200, 192);
            this.numMaxDiscountPercent.MaxValue = 100;
            this.numMaxDiscountPercent.MinValue = 0;
            this.numMaxDiscountPercent.Name = "numMaxDiscountPercent";
            this.numMaxDiscountPercent.Size = new System.Drawing.Size(100, 24);
            this.numMaxDiscountPercent.TabIndex = 10;
            this.numMaxDiscountPercent.Value = 100;
            // 
            // lblMaxDiscountPercent
            // 
            this.lblMaxDiscountPercent.AutoSize = true;
            this.lblMaxDiscountPercent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMaxDiscountPercent.Location = new System.Drawing.Point(20, 195);
            this.lblMaxDiscountPercent.Name = "lblMaxDiscountPercent";
            this.lblMaxDiscountPercent.Size = new System.Drawing.Size(130, 16);
            this.lblMaxDiscountPercent.TabIndex = 9;
            this.lblMaxDiscountPercent.Text = "Max Discount %:";
            // 
            // cmbRoundingMethod
            // 
            this.cmbRoundingMethod.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbRoundingMethod.Location = new System.Drawing.Point(200, 152);
            this.cmbRoundingMethod.Name = "cmbRoundingMethod";
            this.cmbRoundingMethod.Size = new System.Drawing.Size(230, 24);
            this.cmbRoundingMethod.TabIndex = 8;
            // 
            // lblRoundingMethod
            // 
            this.lblRoundingMethod.AutoSize = true;
            this.lblRoundingMethod.Location = new System.Drawing.Point(20, 155);
            this.lblRoundingMethod.Name = "lblRoundingMethod";
            this.lblRoundingMethod.Size = new System.Drawing.Size(108, 15);
            this.lblRoundingMethod.TabIndex = 7;
            this.lblRoundingMethod.Text = "Rounding Method:";
            // 
            // cmbDefaultSaleType
            // 
            this.cmbDefaultSaleType.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbDefaultSaleType.Location = new System.Drawing.Point(200, 72);
            this.cmbDefaultSaleType.Name = "cmbDefaultSaleType";
            this.cmbDefaultSaleType.Size = new System.Drawing.Size(230, 24);
            this.cmbDefaultSaleType.TabIndex = 4;
            // 
            // lblDefaultSaleType
            // 
            this.lblDefaultSaleType.AutoSize = true;
            this.lblDefaultSaleType.Location = new System.Drawing.Point(20, 75);
            this.lblDefaultSaleType.Name = "lblDefaultSaleType";
            this.lblDefaultSaleType.Size = new System.Drawing.Size(102, 15);
            this.lblDefaultSaleType.TabIndex = 3;
            this.lblDefaultSaleType.Text = "Default Sale Type:";
            // 
            // cmbDefaultPriceLevel
            // 
            this.cmbDefaultPriceLevel.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbDefaultPriceLevel.Location = new System.Drawing.Point(200, 112);
            this.cmbDefaultPriceLevel.Name = "cmbDefaultPriceLevel";
            this.cmbDefaultPriceLevel.Size = new System.Drawing.Size(230, 24);
            this.cmbDefaultPriceLevel.TabIndex = 6;
            // 
            // lblDefaultPriceLevel
            // 
            this.lblDefaultPriceLevel.AutoSize = true;
            this.lblDefaultPriceLevel.Location = new System.Drawing.Point(20, 115);
            this.lblDefaultPriceLevel.Name = "lblDefaultPriceLevel";
            this.lblDefaultPriceLevel.Size = new System.Drawing.Size(107, 15);
            this.lblDefaultPriceLevel.TabIndex = 5;
            this.lblDefaultPriceLevel.Text = "Default Price Level:";
            // 
            // cmbDefaultCreditDays
            // 
            this.cmbDefaultCreditDays.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbDefaultCreditDays.Location = new System.Drawing.Point(200, 232);
            this.cmbDefaultCreditDays.Name = "cmbDefaultCreditDays";
            this.cmbDefaultCreditDays.Size = new System.Drawing.Size(230, 24);
            this.cmbDefaultCreditDays.TabIndex = 11;
            // 
            // lblDefaultCreditDays
            // 
            this.lblDefaultCreditDays.AutoSize = true;
            this.lblDefaultCreditDays.Location = new System.Drawing.Point(20, 235);
            this.lblDefaultCreditDays.Name = "lblDefaultCreditDays";
            this.lblDefaultCreditDays.Size = new System.Drawing.Size(111, 15);
            this.lblDefaultCreditDays.TabIndex = 10;
            this.lblDefaultCreditDays.Text = "Default Credit Days:";
            // 
            // chkAllowNegativeStock
            // 
            this.chkAllowNegativeStock.AutoSize = true;
            this.chkAllowNegativeStock.Location = new System.Drawing.Point(200, 267);
            this.chkAllowNegativeStock.Name = "chkAllowNegativeStock";
            this.chkAllowNegativeStock.Size = new System.Drawing.Size(138, 19);
            this.chkAllowNegativeStock.TabIndex = 12;
            this.chkAllowNegativeStock.Text = "Allow Negative Stock";
            // 
            // chkAllowSaleBelowCost
            // 
            this.chkAllowSaleBelowCost.AutoSize = true;
            this.chkAllowSaleBelowCost.Location = new System.Drawing.Point(20, 267);
            this.chkAllowSaleBelowCost.Name = "chkAllowSaleBelowCost";
            this.chkAllowSaleBelowCost.Size = new System.Drawing.Size(148, 19);
            this.chkAllowSaleBelowCost.TabIndex = 13;
            this.chkAllowSaleBelowCost.Text = "Allow Sale Below Cost";
            // 
            // cmbDuplicateItemBehavior
            // 
            this.cmbDuplicateItemBehavior.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbDuplicateItemBehavior.Location = new System.Drawing.Point(200, 32);
            this.cmbDuplicateItemBehavior.Name = "cmbDuplicateItemBehavior";
            this.cmbDuplicateItemBehavior.Size = new System.Drawing.Size(230, 24);
            this.cmbDuplicateItemBehavior.TabIndex = 1;
            // 
            // lblDuplicateItemBehavior
            // 
            this.lblDuplicateItemBehavior.AutoSize = true;
            this.lblDuplicateItemBehavior.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDuplicateItemBehavior.Location = new System.Drawing.Point(20, 35);
            this.lblDuplicateItemBehavior.Name = "lblDuplicateItemBehavior";
            this.lblDuplicateItemBehavior.Size = new System.Drawing.Size(136, 15);
            this.lblDuplicateItemBehavior.TabIndex = 0;
            this.lblDuplicateItemBehavior.Text = "Duplicate Item Behavior:";
            // 
            // grpDisplaySettings
            // 
            this.grpDisplaySettings.Controls.Add(this.chkShowMarginColumn);
            this.grpDisplaySettings.Controls.Add(this.chkShowCostToUser);
            this.grpDisplaySettings.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpDisplaySettings.Location = new System.Drawing.Point(490, 80);
            this.grpDisplaySettings.Name = "grpDisplaySettings";
            this.grpDisplaySettings.Size = new System.Drawing.Size(320, 100);
            this.grpDisplaySettings.TabIndex = 2;
            this.grpDisplaySettings.Text = "Display Settings";
            // 
            // chkShowMarginColumn
            // 
            this.chkShowMarginColumn.AutoSize = true;
            this.chkShowMarginColumn.Location = new System.Drawing.Point(20, 65);
            this.chkShowMarginColumn.Name = "chkShowMarginColumn";
            this.chkShowMarginColumn.Size = new System.Drawing.Size(145, 19);
            this.chkShowMarginColumn.TabIndex = 1;
            this.chkShowMarginColumn.Text = "Show Margin Column";
            // 
            // chkShowCostToUser
            // 
            this.chkShowCostToUser.AutoSize = true;
            this.chkShowCostToUser.Location = new System.Drawing.Point(20, 35);
            this.chkShowCostToUser.Name = "chkShowCostToUser";
            this.chkShowCostToUser.Size = new System.Drawing.Size(131, 19);
            this.chkShowCostToUser.TabIndex = 0;
            this.chkShowCostToUser.Text = "Show Cost Column";
            // 
            // grpPrintingSettings
            // 
            this.grpPrintingSettings.Controls.Add(this.numPrintCopies);
            this.grpPrintingSettings.Controls.Add(this.lblPrintCopies);
            this.grpPrintingSettings.Controls.Add(this.chkAutoPrintAfterSave);
            this.grpPrintingSettings.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpPrintingSettings.Location = new System.Drawing.Point(490, 190);
            this.grpPrintingSettings.Name = "grpPrintingSettings";
            this.grpPrintingSettings.Size = new System.Drawing.Size(320, 130);
            this.grpPrintingSettings.TabIndex = 3;
            this.grpPrintingSettings.Text = "Printing Settings";
            // 
            // numPrintCopies
            // 
            this.numPrintCopies.Location = new System.Drawing.Point(150, 72);
            this.numPrintCopies.MaxValue = 10;
            this.numPrintCopies.MinValue = 1;
            this.numPrintCopies.Name = "numPrintCopies";
            this.numPrintCopies.Size = new System.Drawing.Size(80, 24);
            this.numPrintCopies.TabIndex = 2;
            this.numPrintCopies.Value = 1;
            this.numPrintCopies.TabIndex = 2;
            this.numPrintCopies.Value = 1;
            // 
            // lblPrintCopies
            // 
            this.lblPrintCopies.AutoSize = true;
            this.lblPrintCopies.Location = new System.Drawing.Point(20, 75);
            this.lblPrintCopies.Name = "lblPrintCopies";
            this.lblPrintCopies.Size = new System.Drawing.Size(74, 15);
            this.lblPrintCopies.TabIndex = 1;
            this.lblPrintCopies.Text = "Print Copies:";
            // 
            // chkAutoPrintAfterSave
            // 
            this.chkAutoPrintAfterSave.AutoSize = true;
            this.chkAutoPrintAfterSave.Location = new System.Drawing.Point(20, 35);
            this.chkAutoPrintAfterSave.Name = "chkAutoPrintAfterSave";
            this.chkAutoPrintAfterSave.Size = new System.Drawing.Size(133, 19);
            this.chkAutoPrintAfterSave.TabIndex = 0;
            this.chkAutoPrintAfterSave.Text = "Auto Print After Save";
            // 
            // btnSave
            // 
            this.btnSave.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.btnSave.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnSave.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(500, 450);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 35);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom)));
            // 
            // btnCancel
            // 
            this.btnCancel.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.btnCancel.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnCancel.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Location = new System.Drawing.Point(610, 450);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom)));
            // 
            // btnResetDefaults
            // 
            this.btnResetDefaults.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnResetDefaults.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnResetDefaults.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnResetDefaults.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnResetDefaults.Location = new System.Drawing.Point(720, 450);
            this.btnResetDefaults.Name = "btnResetDefaults";
            this.btnResetDefaults.Size = new System.Drawing.Size(125, 35);
            this.btnResetDefaults.TabIndex = 6;
            this.btnResetDefaults.Text = "Reset Defaults";
            this.btnResetDefaults.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnResetDefaults.Click += new System.EventHandler(this.btnResetDefaults_Click);
            this.btnResetDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom)));
            // 
            // lblCurrentBranch
            // 
            this.lblCurrentBranch.AutoSize = true;
            this.lblCurrentBranch.Appearance.ForeColor = System.Drawing.Color.LightGray;
            this.lblCurrentBranch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCurrentBranch.Location = new System.Drawing.Point(17, 38);
            this.lblCurrentBranch.Name = "lblCurrentBranch";
            this.lblCurrentBranch.Size = new System.Drawing.Size(102, 15);
            this.lblCurrentBranch.TabIndex = 1;
            this.lblCurrentBranch.Text = "Branch: Loading...";
            // 
            // panelHeader
            // 
            this.panelHeader.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(76)))));
            this.panelHeader.ClientArea.Controls.Add(this.lblTitle);
            this.panelHeader.ClientArea.Controls.Add(this.lblCurrentBranch);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(1288, 60);
            this.panelHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            this.lblTitle.Appearance.FontData.SizeInPoints = 14F;
            this.lblTitle.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(15, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(126, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "POS Settings";
            // 
            // frmPOSSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(1288, 572);
            this.Controls.Add(this.btnResetDefaults);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.grpPrintingSettings);
            this.Controls.Add(this.grpDisplaySettings);
            this.Controls.Add(this.grpSalesSettings);
            this.Controls.Add(this.panelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPOSSettings";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sale Settings";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmPOSSettings_FormClosing);
            this.Load += new System.EventHandler(this.frmPOSSettings_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmPOSSettings_KeyDown);
            this.grpSalesSettings.ResumeLayout(false);
            this.grpSalesSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxDiscountPercent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbRoundingMethod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDefaultSaleType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDefaultPriceLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDefaultCreditDays)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAllowNegativeStock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAllowSaleBelowCost)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDuplicateItemBehavior)).EndInit();
            this.grpDisplaySettings.ResumeLayout(false);
            this.grpDisplaySettings.PerformLayout();
            this.grpPrintingSettings.ResumeLayout(false);
            this.grpPrintingSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPrintCopies)).EndInit();
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel panelHeader;
        private Infragistics.Win.Misc.UltraLabel lblTitle;
        private Infragistics.Win.Misc.UltraLabel lblCurrentBranch;
        private Infragistics.Win.Misc.UltraGroupBox grpSalesSettings;
        private Infragistics.Win.Misc.UltraLabel lblDuplicateItemBehavior;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbDuplicateItemBehavior;
        private Infragistics.Win.UltraWinEditors.UltraCheckEditor chkAllowNegativeStock;
        private Infragistics.Win.UltraWinEditors.UltraCheckEditor chkAllowSaleBelowCost;
        private Infragistics.Win.Misc.UltraLabel lblDefaultSaleType;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbDefaultSaleType;
        private Infragistics.Win.Misc.UltraLabel lblDefaultPriceLevel;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbDefaultPriceLevel;
        private Infragistics.Win.Misc.UltraLabel lblDefaultCreditDays;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbDefaultCreditDays;
        private Infragistics.Win.Misc.UltraLabel lblRoundingMethod;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbRoundingMethod;
        private Infragistics.Win.Misc.UltraLabel lblMaxDiscountPercent;
        private Infragistics.Win.UltraWinEditors.UltraNumericEditor numMaxDiscountPercent;
        private Infragistics.Win.Misc.UltraGroupBox grpDisplaySettings;
        private Infragistics.Win.UltraWinEditors.UltraCheckEditor chkShowCostToUser;
        private Infragistics.Win.UltraWinEditors.UltraCheckEditor chkShowMarginColumn;
        private Infragistics.Win.Misc.UltraGroupBox grpPrintingSettings;
        private Infragistics.Win.UltraWinEditors.UltraCheckEditor chkAutoPrintAfterSave;
        private Infragistics.Win.Misc.UltraLabel lblPrintCopies;
        private Infragistics.Win.UltraWinEditors.UltraNumericEditor numPrintCopies;
        private Infragistics.Win.Misc.UltraButton btnSave;
        private Infragistics.Win.Misc.UltraButton btnCancel;
        private Infragistics.Win.Misc.UltraButton btnResetDefaults;
    }
}
