namespace PosBranch_Win.Reports.InventoryReport
{
    partial class frmdatewisestockadjustmentreport
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
            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.lblPeriodMode = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboPeriodMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblMonth = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboMonth = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblYear = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboYear = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblViewMode = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboViewMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblItem = new Infragistics.Win.Misc.UltraLabel();
            this.txtItemSearch = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.btnItemPicker = new System.Windows.Forms.Button();
            this.lblAdjustmentType = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboAdjustmentType = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.ultraPanelAction = new Infragistics.Win.Misc.UltraPanel();
            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewReport = new Infragistics.Win.Misc.UltraButton();
            this.btnExportGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnColumnChooser = new Infragistics.Win.Misc.UltraButton();
            this.btnToggleValueMode = new Infragistics.Win.Misc.UltraButton();
            this.btnExpandAll = new Infragistics.Win.Misc.UltraButton();
            this.btnCollapseAll = new Infragistics.Win.Misc.UltraButton();
            this.btnToggleSelection = new Infragistics.Win.Misc.UltraButton();
            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPeriodMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboMonth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboYear)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboViewMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboAdjustmentType)).BeginInit();
            this.ultraPanelAction.ClientArea.SuspendLayout();
            this.ultraPanelAction.SuspendLayout();
            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraPanelControls
            // 
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPeriodMode);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboPeriodMode);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtTo);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblMonth);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboMonth);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblYear);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboYear);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblViewMode);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboViewMode);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblItem);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtItemSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnItemPicker);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblAdjustmentType);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboAdjustmentType);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1364, 80);
            this.ultraPanelControls.TabIndex = 0;
            // 
            // lblPeriodMode
            // 
            this.lblPeriodMode.Location = new System.Drawing.Point(14, 14);
            this.lblPeriodMode.Name = "lblPeriodMode";
            this.lblPeriodMode.Size = new System.Drawing.Size(80, 18);
            this.lblPeriodMode.TabIndex = 0;
            this.lblPeriodMode.Text = "Period Mode";
            // 
            // ultraComboPeriodMode
            // 
            this.ultraComboPeriodMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboPeriodMode.Location = new System.Drawing.Point(98, 12);
            this.ultraComboPeriodMode.Name = "ultraComboPeriodMode";
            this.ultraComboPeriodMode.Size = new System.Drawing.Size(115, 21);
            this.ultraComboPeriodMode.TabIndex = 1;
            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(220, 14);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(42, 18);
            this.lblFromDate.TabIndex = 2;
            this.lblFromDate.Text = "From";
            // 
            // dtFrom
            // 
            this.dtFrom.Location = new System.Drawing.Point(264, 12);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(110, 21);
            this.dtFrom.TabIndex = 3;
            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(380, 14);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(26, 18);
            this.lblToDate.TabIndex = 4;
            this.lblToDate.Text = "To";
            // 
            // dtTo
            // 
            this.dtTo.Location = new System.Drawing.Point(410, 12);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(110, 21);
            this.dtTo.TabIndex = 5;
            // 
            // lblMonth
            // 
            this.lblMonth.Location = new System.Drawing.Point(220, 14);
            this.lblMonth.Name = "lblMonth";
            this.lblMonth.Size = new System.Drawing.Size(42, 18);
            this.lblMonth.TabIndex = 6;
            this.lblMonth.Text = "Month";
            this.lblMonth.Visible = false;
            // 
            // ultraComboMonth
            // 
            this.ultraComboMonth.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboMonth.Location = new System.Drawing.Point(264, 12);
            this.ultraComboMonth.Name = "ultraComboMonth";
            this.ultraComboMonth.Size = new System.Drawing.Size(110, 21);
            this.ultraComboMonth.TabIndex = 7;
            this.ultraComboMonth.Visible = false;
            // 
            // lblYear
            // 
            this.lblYear.Location = new System.Drawing.Point(380, 14);
            this.lblYear.Name = "lblYear";
            this.lblYear.Size = new System.Drawing.Size(32, 18);
            this.lblYear.TabIndex = 8;
            this.lblYear.Text = "Year";
            this.lblYear.Visible = false;
            // 
            // ultraComboYear
            // 
            this.ultraComboYear.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboYear.Location = new System.Drawing.Point(415, 12);
            this.ultraComboYear.Name = "ultraComboYear";
            this.ultraComboYear.Size = new System.Drawing.Size(90, 21);
            this.ultraComboYear.TabIndex = 9;
            this.ultraComboYear.Visible = false;
            // 
            // lblViewMode
            // 
            this.lblViewMode.Location = new System.Drawing.Point(550, 14);
            this.lblViewMode.Name = "lblViewMode";
            this.lblViewMode.Size = new System.Drawing.Size(70, 18);
            this.lblViewMode.TabIndex = 10;
            this.lblViewMode.Text = "View Mode";
            // 
            // ultraComboViewMode
            // 
            this.ultraComboViewMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboViewMode.Location = new System.Drawing.Point(626, 12);
            this.ultraComboViewMode.Name = "ultraComboViewMode";
            this.ultraComboViewMode.Size = new System.Drawing.Size(200, 21);
            this.ultraComboViewMode.TabIndex = 11;
            // 
            // lblItem
            // 
            this.lblItem.Location = new System.Drawing.Point(14, 46);
            this.lblItem.Name = "lblItem";
            this.lblItem.Size = new System.Drawing.Size(80, 18);
            this.lblItem.TabIndex = 12;
            this.lblItem.Text = "Item Filter";
            // 
            // txtItemSearch
            // 
            this.txtItemSearch.Location = new System.Drawing.Point(98, 44);
            this.txtItemSearch.Name = "txtItemSearch";
            this.txtItemSearch.Size = new System.Drawing.Size(295, 21);
            this.txtItemSearch.TabIndex = 13;
            // 
            // btnItemPicker
            // 
            this.btnItemPicker.Location = new System.Drawing.Point(396, 42);
            this.btnItemPicker.Name = "btnItemPicker";
            this.btnItemPicker.Size = new System.Drawing.Size(34, 24);
            this.btnItemPicker.TabIndex = 14;
            this.btnItemPicker.Text = "F11";
            this.btnItemPicker.UseVisualStyleBackColor = false;
            // 
            // lblAdjustmentType
            // 
            this.lblAdjustmentType.Location = new System.Drawing.Point(445, 46);
            this.lblAdjustmentType.Name = "lblAdjustmentType";
            this.lblAdjustmentType.Size = new System.Drawing.Size(65, 18);
            this.lblAdjustmentType.TabIndex = 15;
            this.lblAdjustmentType.Text = "Adj. Type";
            // 
            // ultraComboAdjustmentType
            // 
            this.ultraComboAdjustmentType.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboAdjustmentType.Location = new System.Drawing.Point(515, 44);
            this.ultraComboAdjustmentType.Name = "ultraComboAdjustmentType";
            this.ultraComboAdjustmentType.Size = new System.Drawing.Size(180, 21);
            this.ultraComboAdjustmentType.TabIndex = 16;
            // 
            // ultraPanelAction
            // 
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnViewGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPreviewReport);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnExportGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnClearFilters);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnColumnChooser);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnToggleValueMode);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnExpandAll);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnCollapseAll);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnToggleSelection);
            this.ultraPanelAction.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelAction.Location = new System.Drawing.Point(0, 80);
            this.ultraPanelAction.Name = "ultraPanelAction";
            this.ultraPanelAction.Size = new System.Drawing.Size(1364, 45);
            this.ultraPanelAction.TabIndex = 1;
            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Location = new System.Drawing.Point(12, 6);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(100, 32);
            this.btnViewGrid.TabIndex = 0;
            this.btnViewGrid.Text = "View Grid";
            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(118, 6);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(100, 32);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid";
            // 
            // btnPreviewReport
            // 
            this.btnPreviewReport.Location = new System.Drawing.Point(224, 6);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(105, 32);
            this.btnPreviewReport.TabIndex = 2;
            this.btnPreviewReport.Text = "Preview Report";
            // 
            // btnExportGrid
            // 
            this.btnExportGrid.Location = new System.Drawing.Point(335, 6);
            this.btnExportGrid.Name = "btnExportGrid";
            this.btnExportGrid.Size = new System.Drawing.Size(95, 32);
            this.btnExportGrid.TabIndex = 3;
            this.btnExportGrid.Text = "Export Grid";
            // 
            // btnClearFilters
            // 
            this.btnClearFilters.Location = new System.Drawing.Point(436, 6);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(95, 32);
            this.btnClearFilters.TabIndex = 4;
            this.btnClearFilters.Text = "Clear Filters";
            // 
            // btnColumnChooser
            // 
            this.btnColumnChooser.Location = new System.Drawing.Point(537, 6);
            this.btnColumnChooser.Name = "btnColumnChooser";
            this.btnColumnChooser.Size = new System.Drawing.Size(110, 32);
            this.btnColumnChooser.TabIndex = 5;
            this.btnColumnChooser.Text = "Column Chooser";
            // 
            // btnToggleValueMode
            // 
            this.btnToggleValueMode.Location = new System.Drawing.Point(653, 6);
            this.btnToggleValueMode.Name = "btnToggleValueMode";
            this.btnToggleValueMode.Size = new System.Drawing.Size(110, 32);
            this.btnToggleValueMode.TabIndex = 6;
            this.btnToggleValueMode.Text = "Show Amount";
            // 
            // btnExpandAll
            // 
            this.btnExpandAll.Location = new System.Drawing.Point(769, 6);
            this.btnExpandAll.Name = "btnExpandAll";
            this.btnExpandAll.Size = new System.Drawing.Size(105, 32);
            this.btnExpandAll.TabIndex = 7;
            this.btnExpandAll.Text = "Expand All";
            // 
            // btnCollapseAll
            // 
            this.btnCollapseAll.Location = new System.Drawing.Point(880, 6);
            this.btnCollapseAll.Name = "btnCollapseAll";
            this.btnCollapseAll.Size = new System.Drawing.Size(105, 32);
            this.btnCollapseAll.TabIndex = 8;
            this.btnCollapseAll.Text = "Collapse All";
            // 
            // btnToggleSelection
            // 
            this.btnToggleSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleSelection.Location = new System.Drawing.Point(1234, 6);
            this.btnToggleSelection.Name = "btnToggleSelection";
            this.btnToggleSelection.Size = new System.Drawing.Size(118, 32);
            this.btnToggleSelection.TabIndex = 9;
            this.btnToggleSelection.Text = "Hide Selection";
            // 
            // ultraPanelMaster
            // 
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.gridReport);
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 125);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1364, 434);
            this.ultraPanelMaster.TabIndex = 2;
            // 
            // ultraPanelGridFooter
            // 
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 404);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1364, 30);
            this.ultraPanelGridFooter.TabIndex = 1;
            // 
            // gridReport
            // 
            this.gridReport.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Location = new System.Drawing.Point(0, 0);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1364, 404);
            this.gridReport.TabIndex = 0;
            // 
            // frmdatewisestockadjustmentreport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1364, 559);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelAction);
            this.Controls.Add(this.ultraPanelControls);
            this.Name = "frmdatewisestockadjustmentreport";
            this.Text = "Date-wise Stock Adjustment Report";
            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPeriodMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboMonth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboYear)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboViewMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboAdjustmentType)).EndInit();
            this.ultraPanelAction.ClientArea.ResumeLayout(false);
            this.ultraPanelAction.ResumeLayout(false);
            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraLabel lblPeriodMode;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboPeriodMode;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.Misc.UltraLabel lblMonth;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboMonth;
        private Infragistics.Win.Misc.UltraLabel lblYear;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboYear;
        private Infragistics.Win.Misc.UltraLabel lblViewMode;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboViewMode;
        private Infragistics.Win.Misc.UltraLabel lblItem;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor txtItemSearch;
        private System.Windows.Forms.Button btnItemPicker;
        private Infragistics.Win.Misc.UltraLabel lblAdjustmentType;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboAdjustmentType;
        private Infragistics.Win.Misc.UltraPanel ultraPanelAction;
        private Infragistics.Win.Misc.UltraButton btnViewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewReport;
        private Infragistics.Win.Misc.UltraButton btnExportGrid;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnColumnChooser;
        private Infragistics.Win.Misc.UltraButton btnToggleValueMode;
        private Infragistics.Win.Misc.UltraButton btnExpandAll;
        private Infragistics.Win.Misc.UltraButton btnCollapseAll;
        private Infragistics.Win.Misc.UltraButton btnToggleSelection;
        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;
    }
}
