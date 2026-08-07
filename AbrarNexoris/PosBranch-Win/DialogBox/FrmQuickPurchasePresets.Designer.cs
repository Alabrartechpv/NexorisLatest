namespace PosBranch_Win.DialogBox
{
    partial class FrmQuickPurchasePresets
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
            Infragistics.Win.Appearance appearanceFooterLeft = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceFooterCenter = new Infragistics.Win.Appearance();
            this._titleBar = new System.Windows.Forms.Panel();
            this._lblTitle = new System.Windows.Forms.Label();
            this._btnClose = new System.Windows.Forms.Button();
            this._mainTable = new System.Windows.Forms.TableLayoutPanel();
            this._leftPanel = new System.Windows.Forms.Panel();
            this._lblPresetsHeader = new System.Windows.Forms.Label();
            this._gridPresets = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this._footerPanelPresets = new Infragistics.Win.Misc.UltraPanel();
            this._btnFlowLeft = new System.Windows.Forms.FlowLayoutPanel();
            this.ultraPanel1 = new Infragistics.Win.Misc.UltraPanel();
            this.lblNewPreset = new System.Windows.Forms.Label();
            this.ultraPanel3 = new Infragistics.Win.Misc.UltraPanel();
            this.lblDeletePreset = new System.Windows.Forms.Label();
            this._centerPanel = new System.Windows.Forms.Panel();
            this._lblItemsHeader = new System.Windows.Forms.Label();
            this._txtItemSearch = new System.Windows.Forms.TextBox();
            this.ultraPanel7 = new Infragistics.Win.Misc.UltraPanel();
            this.lblAddItem = new System.Windows.Forms.Label();
            this._gridItems = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this._footerPanelItems = new Infragistics.Win.Misc.UltraPanel();
            this._btnFlowCenter = new System.Windows.Forms.FlowLayoutPanel();
            this.ultraPanel6 = new Infragistics.Win.Misc.UltraPanel();
            this.lblRemoveItem = new System.Windows.Forms.Label();
            this._rightPanel = new System.Windows.Forms.Panel();
            this._lblVendorHeader = new System.Windows.Forms.Label();
            this._lblVendor = new System.Windows.Forms.Label();
            this._cmbVendor = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.ultraPanel4 = new Infragistics.Win.Misc.UltraPanel();
            this.lblClearVendor = new System.Windows.Forms.Label();
            this._lblVendorHint = new System.Windows.Forms.Label();
            this.ultraPanel5 = new Infragistics.Win.Misc.UltraPanel();
            this.lblExport = new System.Windows.Forms.Label();
            this._titleBar.SuspendLayout();
            this._mainTable.SuspendLayout();
            this._leftPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._gridPresets)).BeginInit();
            this._footerPanelPresets.SuspendLayout();
            this._btnFlowLeft.SuspendLayout();
            this.ultraPanel1.ClientArea.SuspendLayout();
            this.ultraPanel1.SuspendLayout();
            this.ultraPanel3.ClientArea.SuspendLayout();
            this.ultraPanel3.SuspendLayout();
            this._centerPanel.SuspendLayout();
            this.ultraPanel7.ClientArea.SuspendLayout();
            this.ultraPanel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._gridItems)).BeginInit();
            this._footerPanelItems.SuspendLayout();
            this._btnFlowCenter.SuspendLayout();
            this.ultraPanel6.ClientArea.SuspendLayout();
            this.ultraPanel6.SuspendLayout();
            this._rightPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._cmbVendor)).BeginInit();
            this.ultraPanel4.ClientArea.SuspendLayout();
            this.ultraPanel4.SuspendLayout();
            this.ultraPanel5.ClientArea.SuspendLayout();
            this.ultraPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // _titleBar
            // 
            this._titleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(100)))), ((int)(((byte)(180)))));
            this._titleBar.Controls.Add(this._lblTitle);
            this._titleBar.Controls.Add(this._btnClose);
            this._titleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this._titleBar.Location = new System.Drawing.Point(0, 0);
            this._titleBar.Name = "_titleBar";
            this._titleBar.Size = new System.Drawing.Size(1060, 44);
            this._titleBar.TabIndex = 0;
            // 
            // _lblTitle
            // 
            this._lblTitle.AutoSize = true;
            this._lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this._lblTitle.ForeColor = System.Drawing.Color.White;
            this._lblTitle.Location = new System.Drawing.Point(14, 10);
            this._lblTitle.Name = "_lblTitle";
            this._lblTitle.Size = new System.Drawing.Size(215, 21);
            this._lblTitle.TabIndex = 0;
            this._lblTitle.Text = "⚡  Quick Purchase Presets";
            // 
            // _btnClose
            // 
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.BackColor = System.Drawing.Color.Transparent;
            this._btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnClose.FlatAppearance.BorderSize = 0;
            this._btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this._btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnClose.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this._btnClose.ForeColor = System.Drawing.Color.White;
            this._btnClose.Location = new System.Drawing.Point(1016, 5);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(34, 34);
            this._btnClose.TabIndex = 1;
            this._btnClose.Text = "✕";
            this._btnClose.UseVisualStyleBackColor = false;
            // 
            // _mainTable
            // 
            this._mainTable.ColumnCount = 3;
            this._mainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this._mainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._mainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this._mainTable.Controls.Add(this._leftPanel, 0, 0);
            this._mainTable.Controls.Add(this._centerPanel, 1, 0);
            this._mainTable.Controls.Add(this._rightPanel, 2, 0);
            this._mainTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mainTable.Location = new System.Drawing.Point(0, 44);
            this._mainTable.Name = "_mainTable";
            this._mainTable.Padding = new System.Windows.Forms.Padding(8);
            this._mainTable.RowCount = 1;
            this._mainTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._mainTable.Size = new System.Drawing.Size(1060, 536);
            this._mainTable.TabIndex = 1;
            // 
            // _leftPanel
            // 
            this._leftPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(238)))), ((int)(((byte)(255)))));
            this._leftPanel.Controls.Add(this._lblPresetsHeader);
            this._leftPanel.Controls.Add(this._gridPresets);
            this._leftPanel.Controls.Add(this._footerPanelPresets);
            this._leftPanel.Controls.Add(this._btnFlowLeft);
            this._leftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._leftPanel.Location = new System.Drawing.Point(8, 8);
            this._leftPanel.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this._leftPanel.Name = "_leftPanel";
            this._leftPanel.Size = new System.Drawing.Size(244, 520);
            this._leftPanel.TabIndex = 0;
            // 
            // _lblPresetsHeader
            // 
            this._lblPresetsHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this._lblPresetsHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(31)))), ((int)(((byte)(79)))));
            this._lblPresetsHeader.Location = new System.Drawing.Point(6, 8);
            this._lblPresetsHeader.Name = "_lblPresetsHeader";
            this._lblPresetsHeader.Size = new System.Drawing.Size(230, 22);
            this._lblPresetsHeader.TabIndex = 0;
            this._lblPresetsHeader.Text = "📁  Presets";
            // 
            // _gridPresets
            // 
            this._gridPresets.Location = new System.Drawing.Point(6, 34);
            this._gridPresets.Name = "_gridPresets";
            this._gridPresets.Size = new System.Drawing.Size(232, 380);
            this._gridPresets.TabIndex = 1;
            this._gridPresets.UseAppStyling = false;
            this._gridPresets.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // _footerPanelPresets
            // 
            appearanceFooterLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceFooterLeft.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceFooterLeft.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this._footerPanelPresets.Appearance = appearanceFooterLeft;
            this._footerPanelPresets.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this._footerPanelPresets.Location = new System.Drawing.Point(6, 416);
            this._footerPanelPresets.Name = "_footerPanelPresets";
            this._footerPanelPresets.Size = new System.Drawing.Size(232, 24);
            this._footerPanelPresets.TabIndex = 2;
            this._footerPanelPresets.UseAppStyling = false;
            // 
            // _btnFlowLeft
            // 
            this._btnFlowLeft.Controls.Add(this.ultraPanel1);
            this._btnFlowLeft.Controls.Add(this.ultraPanel3);
            this._btnFlowLeft.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._btnFlowLeft.Location = new System.Drawing.Point(0, 470);
            this._btnFlowLeft.Name = "_btnFlowLeft";
            this._btnFlowLeft.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this._btnFlowLeft.Size = new System.Drawing.Size(244, 50);
            this._btnFlowLeft.TabIndex = 3;
            // 
            // ultraPanel1
            // 
            // 
            // ultraPanel1.ClientArea
            // 
            this.ultraPanel1.ClientArea.Controls.Add(this.lblNewPreset);
            this.ultraPanel1.Location = new System.Drawing.Point(7, 5);
            this.ultraPanel1.Name = "ultraPanel1";
            this.ultraPanel1.Size = new System.Drawing.Size(110, 38);
            this.ultraPanel1.TabIndex = 0;
            // 
            // lblNewPreset
            // 
            this.lblNewPreset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNewPreset.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.lblNewPreset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(47)))), ((int)(((byte)(108)))));
            this.lblNewPreset.Location = new System.Drawing.Point(0, 0);
            this.lblNewPreset.Name = "lblNewPreset";
            this.lblNewPreset.Size = new System.Drawing.Size(110, 38);
            this.lblNewPreset.TabIndex = 0;
            this.lblNewPreset.Text = "+ New Preset";
            this.lblNewPreset.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ultraPanel3
            // 
            // 
            // ultraPanel3.ClientArea
            // 
            this.ultraPanel3.ClientArea.Controls.Add(this.lblDeletePreset);
            this.ultraPanel3.Location = new System.Drawing.Point(123, 5);
            this.ultraPanel3.Name = "ultraPanel3";
            this.ultraPanel3.Size = new System.Drawing.Size(110, 38);
            this.ultraPanel3.TabIndex = 1;
            // 
            // lblDeletePreset
            // 
            this.lblDeletePreset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDeletePreset.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.lblDeletePreset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(47)))), ((int)(((byte)(108)))));
            this.lblDeletePreset.Location = new System.Drawing.Point(0, 0);
            this.lblDeletePreset.Name = "lblDeletePreset";
            this.lblDeletePreset.Size = new System.Drawing.Size(110, 38);
            this.lblDeletePreset.TabIndex = 0;
            this.lblDeletePreset.Text = "🗑 Delete";
            this.lblDeletePreset.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _centerPanel
            // 
            this._centerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(238)))), ((int)(((byte)(255)))));
            this._centerPanel.Controls.Add(this._lblItemsHeader);
            this._centerPanel.Controls.Add(this._txtItemSearch);
            this._centerPanel.Controls.Add(this.ultraPanel7);
            this._centerPanel.Controls.Add(this._gridItems);
            this._centerPanel.Controls.Add(this._footerPanelItems);
            this._centerPanel.Controls.Add(this._btnFlowCenter);
            this._centerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._centerPanel.Location = new System.Drawing.Point(258, 8);
            this._centerPanel.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this._centerPanel.Name = "_centerPanel";
            this._centerPanel.Size = new System.Drawing.Size(568, 520);
            this._centerPanel.TabIndex = 1;
            // 
            // _lblItemsHeader
            // 
            this._lblItemsHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this._lblItemsHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(31)))), ((int)(((byte)(79)))));
            this._lblItemsHeader.Location = new System.Drawing.Point(6, 8);
            this._lblItemsHeader.Name = "_lblItemsHeader";
            this._lblItemsHeader.Size = new System.Drawing.Size(300, 22);
            this._lblItemsHeader.TabIndex = 0;
            this._lblItemsHeader.Text = "🛒  Items in Preset";
            // 
            // _txtItemSearch
            // 
            this._txtItemSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtItemSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._txtItemSearch.ForeColor = System.Drawing.Color.Gray;
            this._txtItemSearch.Location = new System.Drawing.Point(6, 36);
            this._txtItemSearch.Name = "_txtItemSearch";
            this._txtItemSearch.Size = new System.Drawing.Size(240, 23);
            this._txtItemSearch.TabIndex = 1;
            this._txtItemSearch.Text = "Search items…";
            // 
            // ultraPanel7
            // 
            // 
            // ultraPanel7.ClientArea
            // 
            this.ultraPanel7.ClientArea.Controls.Add(this.lblAddItem);
            this.ultraPanel7.Location = new System.Drawing.Point(254, 33);
            this.ultraPanel7.Name = "ultraPanel7";
            this.ultraPanel7.Size = new System.Drawing.Size(120, 28);
            this.ultraPanel7.TabIndex = 2;
            // 
            // lblAddItem
            // 
            this.lblAddItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAddItem.Font = new System.Drawing.Font("Tahoma", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblAddItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(47)))), ((int)(((byte)(108)))));
            this.lblAddItem.Location = new System.Drawing.Point(0, 0);
            this.lblAddItem.Name = "lblAddItem";
            this.lblAddItem.Size = new System.Drawing.Size(120, 28);
            this.lblAddItem.TabIndex = 0;
            this.lblAddItem.Text = "+ Add Item (F7)";
            this.lblAddItem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _gridItems
            // 
            this._gridItems.Location = new System.Drawing.Point(6, 68);
            this._gridItems.Name = "_gridItems";
            this._gridItems.Size = new System.Drawing.Size(556, 346);
            this._gridItems.TabIndex = 3;
            this._gridItems.UseAppStyling = false;
            this._gridItems.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // _footerPanelItems
            // 
            appearanceFooterCenter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceFooterCenter.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceFooterCenter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this._footerPanelItems.Appearance = appearanceFooterCenter;
            this._footerPanelItems.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this._footerPanelItems.Location = new System.Drawing.Point(6, 416);
            this._footerPanelItems.Name = "_footerPanelItems";
            this._footerPanelItems.Size = new System.Drawing.Size(556, 24);
            this._footerPanelItems.TabIndex = 4;
            this._footerPanelItems.UseAppStyling = false;
            // 
            // _btnFlowCenter
            // 
            this._btnFlowCenter.Controls.Add(this.ultraPanel6);
            this._btnFlowCenter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._btnFlowCenter.Location = new System.Drawing.Point(0, 470);
            this._btnFlowCenter.Name = "_btnFlowCenter";
            this._btnFlowCenter.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this._btnFlowCenter.Size = new System.Drawing.Size(568, 50);
            this._btnFlowCenter.TabIndex = 5;
            // 
            // ultraPanel6
            // 
            // 
            // ultraPanel6.ClientArea
            // 
            this.ultraPanel6.ClientArea.Controls.Add(this.lblRemoveItem);
            this.ultraPanel6.Location = new System.Drawing.Point(7, 5);
            this.ultraPanel6.Name = "ultraPanel6";
            this.ultraPanel6.Size = new System.Drawing.Size(160, 38);
            this.ultraPanel6.TabIndex = 0;
            // 
            // lblRemoveItem
            // 
            this.lblRemoveItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRemoveItem.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.lblRemoveItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(47)))), ((int)(((byte)(108)))));
            this.lblRemoveItem.Location = new System.Drawing.Point(0, 0);
            this.lblRemoveItem.Name = "lblRemoveItem";
            this.lblRemoveItem.Size = new System.Drawing.Size(160, 38);
            this.lblRemoveItem.TabIndex = 0;
            this.lblRemoveItem.Text = "✖ Remove Selected";
            this.lblRemoveItem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _rightPanel
            // 
            this._rightPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(238)))), ((int)(((byte)(255)))));
            this._rightPanel.Controls.Add(this._lblVendorHeader);
            this._rightPanel.Controls.Add(this._lblVendor);
            this._rightPanel.Controls.Add(this._cmbVendor);
            this._rightPanel.Controls.Add(this.ultraPanel4);
            this._rightPanel.Controls.Add(this._lblVendorHint);
            this._rightPanel.Controls.Add(this.ultraPanel5);
            this._rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rightPanel.Location = new System.Drawing.Point(832, 8);
            this._rightPanel.Margin = new System.Windows.Forms.Padding(0);
            this._rightPanel.Name = "_rightPanel";
            this._rightPanel.Size = new System.Drawing.Size(220, 520);
            this._rightPanel.TabIndex = 2;
            // 
            // _lblVendorHeader
            // 
            this._lblVendorHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this._lblVendorHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(31)))), ((int)(((byte)(79)))));
            this._lblVendorHeader.Location = new System.Drawing.Point(8, 8);
            this._lblVendorHeader.Name = "_lblVendorHeader";
            this._lblVendorHeader.Size = new System.Drawing.Size(200, 22);
            this._lblVendorHeader.TabIndex = 0;
            this._lblVendorHeader.Text = "🏢  Vendor (Optional)";
            // 
            // _lblVendor
            // 
            this._lblVendor.AutoSize = true;
            this._lblVendor.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._lblVendor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(31)))), ((int)(((byte)(79)))));
            this._lblVendor.Location = new System.Drawing.Point(8, 38);
            this._lblVendor.Name = "_lblVendor";
            this._lblVendor.Size = new System.Drawing.Size(81, 15);
            this._lblVendor.TabIndex = 1;
            this._lblVendor.Text = "Select Vendor:";
            // 
            // _cmbVendor
            // 
            this._cmbVendor.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
            this._cmbVendor.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this._cmbVendor.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this._cmbVendor.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this._cmbVendor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._cmbVendor.Location = new System.Drawing.Point(8, 58);
            this._cmbVendor.Name = "_cmbVendor";
            this._cmbVendor.Size = new System.Drawing.Size(200, 23);
            this._cmbVendor.TabIndex = 2;
            this._cmbVendor.UseAppStyling = false;
            this._cmbVendor.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraPanel4
            // 
            // 
            // ultraPanel4.ClientArea
            // 
            this.ultraPanel4.ClientArea.Controls.Add(this.lblClearVendor);
            this.ultraPanel4.Location = new System.Drawing.Point(8, 92);
            this.ultraPanel4.Name = "ultraPanel4";
            this.ultraPanel4.Size = new System.Drawing.Size(130, 32);
            this.ultraPanel4.TabIndex = 3;
            // 
            // lblClearVendor
            // 
            this.lblClearVendor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblClearVendor.Font = new System.Drawing.Font("Tahoma", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblClearVendor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(47)))), ((int)(((byte)(108)))));
            this.lblClearVendor.Location = new System.Drawing.Point(0, 0);
            this.lblClearVendor.Name = "lblClearVendor";
            this.lblClearVendor.Size = new System.Drawing.Size(130, 32);
            this.lblClearVendor.TabIndex = 0;
            this.lblClearVendor.Text = "✕ Clear Vendor";
            this.lblClearVendor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _lblVendorHint
            // 
            this._lblVendorHint.BackColor = System.Drawing.Color.Transparent;
            this._lblVendorHint.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Italic);
            this._lblVendorHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(100)))), ((int)(((byte)(160)))));
            this._lblVendorHint.Location = new System.Drawing.Point(8, 136);
            this._lblVendorHint.Name = "_lblVendorHint";
            this._lblVendorHint.Size = new System.Drawing.Size(200, 70);
            this._lblVendorHint.TabIndex = 4;
            this._lblVendorHint.Text = "📌 If a vendor is set here,\r\nit will be auto-selected\r\nin FrmPurchase when\r\nyou e" +
    "xport.";
            // 
            // ultraPanel5
            // 
            // 
            // ultraPanel5.ClientArea
            // 
            this.ultraPanel5.ClientArea.Controls.Add(this.lblExport);
            this.ultraPanel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanel5.Location = new System.Drawing.Point(0, 472);
            this.ultraPanel5.Name = "ultraPanel5";
            this.ultraPanel5.Size = new System.Drawing.Size(220, 48);
            this.ultraPanel5.TabIndex = 5;
            // 
            // lblExport
            // 
            this.lblExport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblExport.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblExport.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(47)))), ((int)(((byte)(108)))));
            this.lblExport.Location = new System.Drawing.Point(0, 0);
            this.lblExport.Name = "lblExport";
            this.lblExport.Size = new System.Drawing.Size(220, 48);
            this.lblExport.TabIndex = 0;
            this.lblExport.Text = "📤  Export to Purchase";
            this.lblExport.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FrmQuickPurchasePresets
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1060, 580);
            this.Controls.Add(this._mainTable);
            this.Controls.Add(this._titleBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "FrmQuickPurchasePresets";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quick Purchase Presets";
            this._titleBar.ResumeLayout(false);
            this._titleBar.PerformLayout();
            this._mainTable.ResumeLayout(false);
            this._leftPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._gridPresets)).EndInit();
            this._footerPanelPresets.ResumeLayout(false);
            this._btnFlowLeft.ResumeLayout(false);
            this.ultraPanel1.ClientArea.ResumeLayout(false);
            this.ultraPanel1.ResumeLayout(false);
            this.ultraPanel3.ClientArea.ResumeLayout(false);
            this.ultraPanel3.ResumeLayout(false);
            this._centerPanel.ResumeLayout(false);
            this._centerPanel.PerformLayout();
            this.ultraPanel7.ClientArea.ResumeLayout(false);
            this.ultraPanel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._gridItems)).EndInit();
            this._footerPanelItems.ResumeLayout(false);
            this._btnFlowCenter.ResumeLayout(false);
            this.ultraPanel6.ClientArea.ResumeLayout(false);
            this.ultraPanel6.ResumeLayout(false);
            this._rightPanel.ResumeLayout(false);
            this._rightPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._cmbVendor)).EndInit();
            this.ultraPanel4.ClientArea.ResumeLayout(false);
            this.ultraPanel4.ResumeLayout(false);
            this.ultraPanel5.ClientArea.ResumeLayout(false);
            this.ultraPanel5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _titleBar;
        private System.Windows.Forms.Label _lblTitle;
        private System.Windows.Forms.Button _btnClose;
        private System.Windows.Forms.TableLayoutPanel _mainTable;
        private System.Windows.Forms.Panel _leftPanel;
        private System.Windows.Forms.Label _lblPresetsHeader;
        private Infragistics.Win.UltraWinGrid.UltraGrid _gridPresets;
        private Infragistics.Win.Misc.UltraPanel _footerPanelPresets;
        private System.Windows.Forms.FlowLayoutPanel _btnFlowLeft;
        private Infragistics.Win.Misc.UltraPanel ultraPanel1;
        private System.Windows.Forms.Label lblNewPreset;
        private Infragistics.Win.Misc.UltraPanel ultraPanel3;
        private System.Windows.Forms.Label lblDeletePreset;
        private System.Windows.Forms.Panel _centerPanel;
        private System.Windows.Forms.Label _lblItemsHeader;
        private System.Windows.Forms.TextBox _txtItemSearch;
        private Infragistics.Win.Misc.UltraPanel ultraPanel7;
        private System.Windows.Forms.Label lblAddItem;
        private Infragistics.Win.UltraWinGrid.UltraGrid _gridItems;
        private Infragistics.Win.Misc.UltraPanel _footerPanelItems;
        private System.Windows.Forms.FlowLayoutPanel _btnFlowCenter;
        private Infragistics.Win.Misc.UltraPanel ultraPanel6;
        private System.Windows.Forms.Label lblRemoveItem;
        private System.Windows.Forms.Panel _rightPanel;
        private System.Windows.Forms.Label _lblVendorHeader;
        private System.Windows.Forms.Label _lblVendor;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor _cmbVendor;
        private Infragistics.Win.Misc.UltraPanel ultraPanel4;
        private System.Windows.Forms.Label lblClearVendor;
        private System.Windows.Forms.Label _lblVendorHint;
        private Infragistics.Win.Misc.UltraPanel ultraPanel5;
        private System.Windows.Forms.Label lblExport;
    }
}
