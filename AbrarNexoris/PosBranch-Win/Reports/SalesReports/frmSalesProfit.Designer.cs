namespace PosBranch_Win.Reports.SalesReports
{
    partial class frmSalesProfit
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
            Infragistics.Win.Appearance appearanceControls = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceAction = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceMaster = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGrid = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceHeader = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceSelected = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGridFooter = new Infragistics.Win.Appearance();

            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblSearchStatus = new Infragistics.Win.Misc.UltraLabel();
            this.lblPreset = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboPresetDates = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.ultraDateTimeFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.ultraDateTimeTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblBillNo = new Infragistics.Win.Misc.UltraLabel();
            this.ultraNumericBillNo = new Infragistics.Win.UltraWinEditors.UltraNumericEditor();

            this.ultraPanelAction = new Infragistics.Win.Misc.UltraPanel();
            this.btnGenerate = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPrint = new Infragistics.Win.Misc.UltraButton();
            this.btnExportCsv = new Infragistics.Win.Misc.UltraButton();
            this.btnColumnChooser = new Infragistics.Win.Misc.UltraButton();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnToggleSelection = new Infragistics.Win.Misc.UltraButton();

            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridProfit = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();

            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPresetDates)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraNumericBillNo)).BeginInit();

            this.ultraPanelAction.ClientArea.SuspendLayout();
            this.ultraPanelAction.SuspendLayout();

            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridProfit)).BeginInit();

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
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearchStatus);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboPresetDates);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraDateTimeFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraDateTimeTo);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblBillNo);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraNumericBillNo);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1200, 78);
            this.ultraPanelControls.TabIndex = 0;

            // 
            // lblSearch
            // 
            this.lblSearch.Location = new System.Drawing.Point(20, 14);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(55, 20);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search";

            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(80, 11);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.NullText = "Search by Bill No, Pay Mode, Cash Mode...";
            this.txtSearch.Size = new System.Drawing.Size(280, 24);
            this.txtSearch.TabIndex = 1;

            // 
            // lblSearchStatus
            // 
            this.lblSearchStatus.Location = new System.Drawing.Point(375, 14);
            this.lblSearchStatus.Name = "lblSearchStatus";
            this.lblSearchStatus.Size = new System.Drawing.Size(240, 20);
            this.lblSearchStatus.TabIndex = 2;
            this.lblSearchStatus.Text = "";

            // 
            // lblPreset
            // 
            this.lblPreset.Location = new System.Drawing.Point(20, 46);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(55, 20);
            this.lblPreset.TabIndex = 3;
            this.lblPreset.Text = "Period";

            // 
            // ultraComboPresetDates
            // 
            this.ultraComboPresetDates.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboPresetDates.Location = new System.Drawing.Point(80, 43);
            this.ultraComboPresetDates.Name = "ultraComboPresetDates";
            this.ultraComboPresetDates.Size = new System.Drawing.Size(160, 24);
            this.ultraComboPresetDates.TabIndex = 4;

            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(255, 46);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(40, 20);
            this.lblFromDate.TabIndex = 5;
            this.lblFromDate.Text = "From";

            // 
            // ultraDateTimeFrom
            // 
            this.ultraDateTimeFrom.FormatString = "dd-MM-yyyy";
            this.ultraDateTimeFrom.Location = new System.Drawing.Point(300, 43);
            this.ultraDateTimeFrom.Name = "ultraDateTimeFrom";
            this.ultraDateTimeFrom.Size = new System.Drawing.Size(120, 24);
            this.ultraDateTimeFrom.TabIndex = 6;

            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(435, 46);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(25, 20);
            this.lblToDate.TabIndex = 7;
            this.lblToDate.Text = "To";

            // 
            // ultraDateTimeTo
            // 
            this.ultraDateTimeTo.FormatString = "dd-MM-yyyy";
            this.ultraDateTimeTo.Location = new System.Drawing.Point(465, 43);
            this.ultraDateTimeTo.Name = "ultraDateTimeTo";
            this.ultraDateTimeTo.Size = new System.Drawing.Size(120, 24);
            this.ultraDateTimeTo.TabIndex = 8;

            // 
            // lblBillNo
            // 
            this.lblBillNo.Location = new System.Drawing.Point(600, 46);
            this.lblBillNo.Name = "lblBillNo";
            this.lblBillNo.Size = new System.Drawing.Size(50, 20);
            this.lblBillNo.TabIndex = 9;
            this.lblBillNo.Text = "Bill No";

            // 
            // ultraNumericBillNo
            // 
            this.ultraNumericBillNo.FormatString = "0";
            this.ultraNumericBillNo.Location = new System.Drawing.Point(655, 43);
            this.ultraNumericBillNo.Name = "ultraNumericBillNo";
            this.ultraNumericBillNo.NullText = "All";
            this.ultraNumericBillNo.Size = new System.Drawing.Size(110, 24);
            this.ultraNumericBillNo.TabIndex = 10;

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
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnExportCsv);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnColumnChooser);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnClearFilters);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnToggleSelection);
            this.ultraPanelAction.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelAction.Location = new System.Drawing.Point(0, 78);
            this.ultraPanelAction.Name = "ultraPanelAction";
            this.ultraPanelAction.Size = new System.Drawing.Size(1200, 45);
            this.ultraPanelAction.TabIndex = 1;

            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(10, 8);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(110, 28);
            this.btnGenerate.TabIndex = 0;
            this.btnGenerate.Text = "View Grid";

            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(126, 8);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(110, 28);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid";

            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(242, 8);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(120, 28);
            this.btnPrint.TabIndex = 2;
            this.btnPrint.Text = "Preview Report";

            // 
            // btnExportCsv
            // 
            this.btnExportCsv.Location = new System.Drawing.Point(368, 8);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(105, 28);
            this.btnExportCsv.TabIndex = 3;
            this.btnExportCsv.Text = "Export Grid";

            // 
            // btnColumnChooser
            // 
            this.btnColumnChooser.Location = new System.Drawing.Point(479, 8);
            this.btnColumnChooser.Name = "btnColumnChooser";
            this.btnColumnChooser.Size = new System.Drawing.Size(130, 28);
            this.btnColumnChooser.TabIndex = 4;
            this.btnColumnChooser.Text = "Column Chooser";

            // 
            // btnClearFilters
            // 
            this.btnClearFilters.Location = new System.Drawing.Point(615, 8);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(105, 28);
            this.btnClearFilters.TabIndex = 5;
            this.btnClearFilters.Text = "Reset Filters";

            // 
            // btnToggleSelection
            // 
            this.btnToggleSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleSelection.Location = new System.Drawing.Point(1055, 8);
            this.btnToggleSelection.Name = "btnToggleSelection";
            this.btnToggleSelection.Size = new System.Drawing.Size(135, 28);
            this.btnToggleSelection.TabIndex = 6;
            this.btnToggleSelection.Text = "Hide Selection";

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
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraGridProfit);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 123);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1200, 577);
            this.ultraPanelMaster.TabIndex = 2;

            // 
            // ultraGridProfit
            // 
            appearanceGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ultraGridProfit.DisplayLayout.Appearance = appearanceGrid;
            this.ultraGridProfit.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            appearanceHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceHeader.ForeColor = System.Drawing.Color.White;
            this.ultraGridProfit.DisplayLayout.Override.HeaderAppearance = appearanceHeader;
            appearanceSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(126)))), ((int)(((byte)(245)))));
            appearanceSelected.ForeColor = System.Drawing.Color.White;
            this.ultraGridProfit.DisplayLayout.Override.SelectedRowAppearance = appearanceSelected;
            this.ultraGridProfit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridProfit.Location = new System.Drawing.Point(0, 0);
            this.ultraGridProfit.Name = "ultraGridProfit";
            this.ultraGridProfit.Size = new System.Drawing.Size(1200, 541);
            this.ultraGridProfit.TabIndex = 0;
            this.ultraGridProfit.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // 
            // ultraPanelGridFooter
            // 
            appearanceGridFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceGridFooter.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(118)))), ((int)(((byte)(184)))));
            appearanceGridFooter.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceGridFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelGridFooter.Appearance = appearanceGridFooter;
            this.ultraPanelGridFooter.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 541);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1200, 36);
            this.ultraPanelGridFooter.TabIndex = 1;

            // 
            // frmSalesProfit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelAction);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "frmSalesProfit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sales Profit Report";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPresetDates)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraNumericBillNo)).EndInit();

            this.ultraPanelAction.ClientArea.ResumeLayout(false);
            this.ultraPanelAction.ResumeLayout(false);

            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridProfit)).EndInit();

            this.ultraPanelGridFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblSearchStatus;
        private Infragistics.Win.Misc.UltraLabel lblPreset;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboPresetDates;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeFrom;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeTo;
        private Infragistics.Win.Misc.UltraLabel lblBillNo;
        private Infragistics.Win.UltraWinEditors.UltraNumericEditor ultraNumericBillNo;

        private Infragistics.Win.Misc.UltraPanel ultraPanelAction;
        private Infragistics.Win.Misc.UltraButton btnGenerate;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPrint;
        private Infragistics.Win.Misc.UltraButton btnExportCsv;
        private Infragistics.Win.Misc.UltraButton btnColumnChooser;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnToggleSelection;

        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridProfit;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
    }
}
