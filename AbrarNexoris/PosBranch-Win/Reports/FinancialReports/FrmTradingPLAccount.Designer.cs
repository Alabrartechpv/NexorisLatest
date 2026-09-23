namespace PosBranch_Win.Reports.FinancialReports
{
    partial class FrmTradingPLAccount
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
            Infragistics.Win.Appearance appearanceControls = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceAction = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceMaster = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGrid = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceHeader = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceSelected = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGridFooter = new Infragistics.Win.Appearance();

            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.lblPreset = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboPresetDates = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.ultraDateTimeFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.ultraDateTimeTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();

            this.ultraPanelAction = new Infragistics.Win.Misc.UltraPanel();
            this.btnGenerate = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnToggleSelection = new Infragistics.Win.Misc.UltraButton();

            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridTrading = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.lblOpeningStock = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalPurchases = new Infragistics.Win.Misc.UltraLabel();
            this.lblTotalSales = new Infragistics.Win.Misc.UltraLabel();
            this.lblClosingStock = new Infragistics.Win.Misc.UltraLabel();
            this.lblGrossProfit = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPresetDates)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).BeginInit();

            this.ultraPanelAction.ClientArea.SuspendLayout();
            this.ultraPanelAction.SuspendLayout();

            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTrading)).BeginInit();
            this.ultraPanelGridFooter.ClientArea.SuspendLayout();
            this.ultraPanelGridFooter.SuspendLayout();
            this.SuspendLayout();

            // 
            // ultraPanelControls
            // 
            appearanceControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearanceControls.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelControls.Appearance = appearanceControls;
            this.ultraPanelControls.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelControls.ClientArea
            // 
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboPresetDates);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraDateTimeFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraDateTimeTo);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1180, 58);
            this.ultraPanelControls.TabIndex = 0;

            // 
            // lblPreset
            // 
            this.lblPreset.Location = new System.Drawing.Point(20, 18);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(40, 20);
            this.lblPreset.TabIndex = 0;
            this.lblPreset.Text = "Period";

            // 
            // ultraComboPresetDates
            // 
            this.ultraComboPresetDates.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboPresetDates.Location = new System.Drawing.Point(65, 15);
            this.ultraComboPresetDates.Name = "ultraComboPresetDates";
            this.ultraComboPresetDates.Size = new System.Drawing.Size(160, 24);
            this.ultraComboPresetDates.TabIndex = 1;
            this.ultraComboPresetDates.ValueChanged += new System.EventHandler(this.UltraComboPresetDates_ValueChanged);

            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(245, 18);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(40, 20);
            this.lblFromDate.TabIndex = 2;
            this.lblFromDate.Text = "From";

            // 
            // ultraDateTimeFrom
            // 
            this.ultraDateTimeFrom.FormatString = "dd-MM-yyyy";
            this.ultraDateTimeFrom.Location = new System.Drawing.Point(290, 15);
            this.ultraDateTimeFrom.Name = "ultraDateTimeFrom";
            this.ultraDateTimeFrom.Size = new System.Drawing.Size(120, 24);
            this.ultraDateTimeFrom.TabIndex = 3;

            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(425, 18);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(25, 20);
            this.lblToDate.TabIndex = 4;
            this.lblToDate.Text = "To";

            // 
            // ultraDateTimeTo
            // 
            this.ultraDateTimeTo.FormatString = "dd-MM-yyyy";
            this.ultraDateTimeTo.Location = new System.Drawing.Point(455, 15);
            this.ultraDateTimeTo.Name = "ultraDateTimeTo";
            this.ultraDateTimeTo.Size = new System.Drawing.Size(120, 24);
            this.ultraDateTimeTo.TabIndex = 5;

            // 
            // ultraPanelAction
            // 
            appearanceAction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(223)))), ((int)(((byte)(238)))));
            appearanceAction.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelAction.Appearance = appearanceAction;
            this.ultraPanelAction.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelAction.ClientArea
            // 
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnGenerate);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPrint);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnExport);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnClearFilters);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnToggleSelection);
            this.ultraPanelAction.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelAction.Location = new System.Drawing.Point(0, 58);
            this.ultraPanelAction.Name = "ultraPanelAction";
            this.ultraPanelAction.Size = new System.Drawing.Size(1180, 45);
            this.ultraPanelAction.TabIndex = 1;

            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(10, 8);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(120, 28);
            this.btnGenerate.TabIndex = 0;
            this.btnGenerate.Text = "View Grid";
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);

            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(136, 8);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(120, 28);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid";
            this.btnPreviewGrid.Click += new System.EventHandler(this.btnPreviewGrid_Click);

            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(262, 8);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(130, 28);
            this.btnPrint.TabIndex = 2;
            this.btnPrint.Text = "Preview Report";
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);

            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(398, 8);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(110, 28);
            this.btnExport.TabIndex = 3;
            this.btnExport.Text = "Export Grid";
            this.btnExport.Click += new System.EventHandler(this.btnExportCsv_Click);

            // 
            // btnClearFilters
            // 
            this.btnClearFilters.Location = new System.Drawing.Point(514, 8);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(110, 28);
            this.btnClearFilters.TabIndex = 4;
            this.btnClearFilters.Text = "Reset Filters";
            this.btnClearFilters.Click += new System.EventHandler(this.btnClearFilters_Click);

            // 
            // btnToggleSelection
            // 
            this.btnToggleSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleSelection.Location = new System.Drawing.Point(1035, 8);
            this.btnToggleSelection.Name = "btnToggleSelection";
            this.btnToggleSelection.Size = new System.Drawing.Size(135, 28);
            this.btnToggleSelection.TabIndex = 5;
            this.btnToggleSelection.Text = "Hide Selection";
            this.btnToggleSelection.Click += new System.EventHandler(this.btnToggleSelection_Click);

            // 
            // ultraPanelMaster
            // 
            appearanceMaster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearanceMaster.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelMaster.Appearance = appearanceMaster;
            this.ultraPanelMaster.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelMaster.ClientArea
            // 
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraGridTrading);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 103);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1180, 597);
            this.ultraPanelMaster.TabIndex = 2;

            // 
            // ultraGridTrading
            // 
            appearanceGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ultraGridTrading.DisplayLayout.Appearance = appearanceGrid;
            this.ultraGridTrading.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            appearanceHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceHeader.ForeColor = System.Drawing.Color.White;
            this.ultraGridTrading.DisplayLayout.Override.HeaderAppearance = appearanceHeader;
            appearanceSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(126)))), ((int)(((byte)(245)))));
            appearanceSelected.ForeColor = System.Drawing.Color.White;
            this.ultraGridTrading.DisplayLayout.Override.SelectedRowAppearance = appearanceSelected;
            this.ultraGridTrading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridTrading.Location = new System.Drawing.Point(0, 0);
            this.ultraGridTrading.Name = "ultraGridTrading";
            this.ultraGridTrading.Size = new System.Drawing.Size(1180, 563);
            this.ultraGridTrading.TabIndex = 0;
            this.ultraGridTrading.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // 
            // ultraPanelGridFooter
            // 
            appearanceGridFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceGridFooter.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(118)))), ((int)(((byte)(184)))));
            appearanceGridFooter.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceGridFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelGridFooter.Appearance = appearanceGridFooter;
            this.ultraPanelGridFooter.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelGridFooter.ClientArea
            // 
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblOpeningStock);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblTotalPurchases);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblTotalSales);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblClosingStock);
            this.ultraPanelGridFooter.ClientArea.Controls.Add(this.lblGrossProfit);
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 563);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1180, 34);
            this.ultraPanelGridFooter.TabIndex = 1;

            // 
            // lblOpeningStock
            // 
            this.lblOpeningStock.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblOpeningStock.Location = new System.Drawing.Point(10, 6);
            this.lblOpeningStock.Name = "lblOpeningStock";
            this.lblOpeningStock.Size = new System.Drawing.Size(185, 22);
            this.lblOpeningStock.TabIndex = 0;
            this.lblOpeningStock.Text = "Opening Stock: ₹ 0.00";

            // 
            // lblTotalPurchases
            // 
            this.lblTotalPurchases.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTotalPurchases.Location = new System.Drawing.Point(200, 6);
            this.lblTotalPurchases.Name = "lblTotalPurchases";
            this.lblTotalPurchases.Size = new System.Drawing.Size(195, 22);
            this.lblTotalPurchases.TabIndex = 1;
            this.lblTotalPurchases.Text = "Purchases: ₹ 0.00";

            // 
            // lblTotalSales
            // 
            this.lblTotalSales.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTotalSales.Location = new System.Drawing.Point(400, 6);
            this.lblTotalSales.Name = "lblTotalSales";
            this.lblTotalSales.Size = new System.Drawing.Size(185, 22);
            this.lblTotalSales.TabIndex = 2;
            this.lblTotalSales.Text = "Sales: ₹ 0.00";

            // 
            // lblClosingStock
            // 
            this.lblClosingStock.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblClosingStock.Location = new System.Drawing.Point(590, 6);
            this.lblClosingStock.Name = "lblClosingStock";
            this.lblClosingStock.Size = new System.Drawing.Size(185, 22);
            this.lblClosingStock.TabIndex = 3;
            this.lblClosingStock.Text = "Closing Stock: ₹ 0.00";

            // 
            // lblGrossProfit
            // 
            this.lblGrossProfit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGrossProfit.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblGrossProfit.Location = new System.Drawing.Point(820, 5);
            this.lblGrossProfit.Name = "lblGrossProfit";
            this.lblGrossProfit.Size = new System.Drawing.Size(350, 24);
            this.lblGrossProfit.TabIndex = 4;
            this.lblGrossProfit.Text = "★ GROSS PROFIT: ₹ 0.00";

            // 
            // FrmTradingPLAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1180, 700);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelAction);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmTradingPLAccount";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Trading Account";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPresetDates)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).EndInit();

            this.ultraPanelAction.ClientArea.ResumeLayout(false);
            this.ultraPanelAction.ResumeLayout(false);

            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridTrading)).EndInit();

            this.ultraPanelGridFooter.ClientArea.ResumeLayout(false);
            this.ultraPanelGridFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraLabel lblPreset;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboPresetDates;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeFrom;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeTo;

        private Infragistics.Win.Misc.UltraPanel ultraPanelAction;
        private Infragistics.Win.Misc.UltraButton btnGenerate;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnToggleSelection;

        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridTrading;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
        private Infragistics.Win.Misc.UltraLabel lblOpeningStock;
        private Infragistics.Win.Misc.UltraLabel lblTotalPurchases;
        private Infragistics.Win.Misc.UltraLabel lblTotalSales;
        private Infragistics.Win.Misc.UltraLabel lblClosingStock;
        private Infragistics.Win.Misc.UltraLabel lblGrossProfit;
    }
}
