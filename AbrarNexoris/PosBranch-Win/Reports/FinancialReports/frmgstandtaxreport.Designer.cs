namespace PosBranch_Win.Reports.FinancialReports
{
    partial class frmgstandtaxreport
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
            this.lblTrnsType = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboTrnsType = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblTaxType = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboTaxType = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblTaxPer = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboTaxPer = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.btnSearchItem = new Infragistics.Win.Misc.UltraButton();
            this.lblDate = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboDateMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblViewMode = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboViewMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.ultraPanelAction = new Infragistics.Win.Misc.UltraPanel();
            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewReport = new Infragistics.Win.Misc.UltraButton();
            this.btnExportGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnToggleSelection = new Infragistics.Win.Misc.UltraButton();
            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboTrnsType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboTaxType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboTaxPer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboViewMode)).BeginInit();
            this.ultraPanelAction.ClientArea.SuspendLayout();
            this.ultraPanelAction.SuspendLayout();
            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            this.ultraPanelGridFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraPanelControls
            // 
            // 
            // ultraPanelControls.ClientArea
            // 
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblTrnsType);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboTrnsType);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblTaxType);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboTaxType);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblTaxPer);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboTaxPer);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnSearchItem);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboDateMode);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtTo);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblViewMode);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboViewMode);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1364, 95);
            this.ultraPanelControls.TabIndex = 0;
            // 
            // lblTrnsType
            // 
            this.lblTrnsType.Location = new System.Drawing.Point(20, 18);
            this.lblTrnsType.Name = "lblTrnsType";
            this.lblTrnsType.Size = new System.Drawing.Size(100, 18);
            this.lblTrnsType.TabIndex = 0;
            this.lblTrnsType.Text = "Transaction Type";
            // 
            // ultraComboTrnsType
            // 
            this.ultraComboTrnsType.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboTrnsType.Location = new System.Drawing.Point(125, 16);
            this.ultraComboTrnsType.Name = "ultraComboTrnsType";
            this.ultraComboTrnsType.Size = new System.Drawing.Size(160, 21);
            this.ultraComboTrnsType.TabIndex = 1;
            // 
            // lblTaxType
            // 
            this.lblTaxType.Location = new System.Drawing.Point(295, 18);
            this.lblTaxType.Name = "lblTaxType";
            this.lblTaxType.Size = new System.Drawing.Size(60, 18);
            this.lblTaxType.TabIndex = 2;
            this.lblTaxType.Text = "Tax Mode";
            // 
            // ultraComboTaxType
            // 
            this.ultraComboTaxType.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboTaxType.Location = new System.Drawing.Point(360, 16);
            this.ultraComboTaxType.Name = "ultraComboTaxType";
            this.ultraComboTaxType.Size = new System.Drawing.Size(115, 21);
            this.ultraComboTaxType.TabIndex = 3;
            // 
            // lblTaxPer
            // 
            this.lblTaxPer.Location = new System.Drawing.Point(485, 18);
            this.lblTaxPer.Name = "lblTaxPer";
            this.lblTaxPer.Size = new System.Drawing.Size(45, 18);
            this.lblTaxPer.TabIndex = 12;
            this.lblTaxPer.Text = "Tax %";
            // 
            // ultraComboTaxPer
            // 
            this.ultraComboTaxPer.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboTaxPer.Location = new System.Drawing.Point(535, 16);
            this.ultraComboTaxPer.Name = "ultraComboTaxPer";
            this.ultraComboTaxPer.Size = new System.Drawing.Size(100, 21);
            this.ultraComboTaxPer.TabIndex = 13;
            // 
            // lblSearch
            // 
            this.lblSearch.Location = new System.Drawing.Point(645, 18);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(50, 18);
            this.lblSearch.TabIndex = 4;
            this.lblSearch.Text = "Search";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(700, 16);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(220, 21);
            this.txtSearch.TabIndex = 5;
            // 
            // btnSearchItem
            // 
            this.btnSearchItem.Location = new System.Drawing.Point(925, 15);
            this.btnSearchItem.Name = "btnSearchItem";
            this.btnSearchItem.Size = new System.Drawing.Size(55, 23);
            this.btnSearchItem.TabIndex = 14;
            this.btnSearchItem.Text = "Item...";
            // 
            // lblDate
            // 
            this.lblDate.Location = new System.Drawing.Point(20, 52);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(100, 18);
            this.lblDate.TabIndex = 6;
            this.lblDate.Text = "Date Filter";
            // 
            // ultraComboDateMode
            // 
            this.ultraComboDateMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboDateMode.Location = new System.Drawing.Point(125, 50);
            this.ultraComboDateMode.Name = "ultraComboDateMode";
            this.ultraComboDateMode.Size = new System.Drawing.Size(160, 21);
            this.ultraComboDateMode.TabIndex = 7;
            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(295, 52);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(42, 18);
            this.lblFromDate.TabIndex = 8;
            this.lblFromDate.Text = "From";
            // 
            // dtFrom
            // 
            this.dtFrom.Location = new System.Drawing.Point(360, 50);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(115, 21);
            this.dtFrom.TabIndex = 9;
            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(485, 52);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(26, 18);
            this.lblToDate.TabIndex = 10;
            this.lblToDate.Text = "To";
            // 
            // dtTo
            // 
            this.dtTo.Location = new System.Drawing.Point(535, 50);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(100, 21);
            this.dtTo.TabIndex = 11;
            // 
            // lblViewMode
            // 
            this.lblViewMode.Location = new System.Drawing.Point(645, 52);
            this.lblViewMode.Name = "lblViewMode";
            this.lblViewMode.Size = new System.Drawing.Size(70, 18);
            this.lblViewMode.TabIndex = 15;
            this.lblViewMode.Text = "View Mode";
            // 
            // ultraComboViewMode
            // 
            this.ultraComboViewMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboViewMode.Location = new System.Drawing.Point(720, 50);
            this.ultraComboViewMode.Name = "ultraComboViewMode";
            this.ultraComboViewMode.Size = new System.Drawing.Size(140, 21);
            this.ultraComboViewMode.TabIndex = 16;
            // 
            // ultraPanelAction
            // 
            // 
            // ultraPanelAction.ClientArea
            // 
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnViewGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPreviewReport);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnExportGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnToggleSelection);
            this.ultraPanelAction.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelAction.Location = new System.Drawing.Point(0, 95);
            this.ultraPanelAction.Name = "ultraPanelAction";
            this.ultraPanelAction.Size = new System.Drawing.Size(1364, 45);
            this.ultraPanelAction.TabIndex = 1;
            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Location = new System.Drawing.Point(10, 9);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(136, 28);
            this.btnViewGrid.TabIndex = 0;
            this.btnViewGrid.Text = "View Grid (F5)";
            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(153, 9);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(138, 28);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid (F6)";
            // 
            // btnPreviewReport
            // 
            this.btnPreviewReport.Location = new System.Drawing.Point(298, 9);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(145, 28);
            this.btnPreviewReport.TabIndex = 2;
            this.btnPreviewReport.Text = "Preview Report (F8)";
            // 
            // btnExportGrid
            // 
            this.btnExportGrid.Location = new System.Drawing.Point(450, 9);
            this.btnExportGrid.Name = "btnExportGrid";
            this.btnExportGrid.Size = new System.Drawing.Size(110, 28);
            this.btnExportGrid.TabIndex = 3;
            this.btnExportGrid.Text = "Export Grid (F7)";
            // 
            // btnToggleSelection
            // 
            this.btnToggleSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleSelection.Location = new System.Drawing.Point(1217, 9);
            this.btnToggleSelection.Name = "btnToggleSelection";
            this.btnToggleSelection.Size = new System.Drawing.Size(135, 28);
            this.btnToggleSelection.TabIndex = 4;
            this.btnToggleSelection.Text = "Hide Selection";
            // 
            // ultraPanelMaster
            // 
            // 
            // ultraPanelMaster.ClientArea
            // 
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.gridReport);
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 140);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1364, 561);
            this.ultraPanelMaster.TabIndex = 2;
            // 
            // ultraPanelGridFooter
            // 
            this.ultraPanelGridFooter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 534);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1364, 26);
            this.ultraPanelGridFooter.TabIndex = 1;
            // 
            // gridReport
            // 
            this.gridReport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.gridReport.DisplayLayout.Appearance = appearance1;
            this.gridReport.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearance2.ForeColor = System.Drawing.Color.White;
            this.gridReport.DisplayLayout.Override.HeaderAppearance = appearance2;
            appearance3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(126)))), ((int)(((byte)(245)))));
            appearance3.ForeColor = System.Drawing.Color.White;
            this.gridReport.DisplayLayout.Override.SelectedRowAppearance = appearance3;
            this.gridReport.Location = new System.Drawing.Point(3, 0);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1358, 534);
            this.gridReport.TabIndex = 0;
            this.gridReport.Text = "GST & Tax Report";
            // 
            // frmgstandtaxreport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1364, 701);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelAction);
            this.Controls.Add(this.ultraPanelControls);
            this.KeyPreview = true;
            this.Name = "frmgstandtaxreport";
            this.Text = "GST & Tax Report";
            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboTrnsType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboTaxType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboTaxPer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboViewMode)).EndInit();
            this.ultraPanelAction.ClientArea.ResumeLayout(false);
            this.ultraPanelAction.ResumeLayout(false);
            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            this.ultraPanelGridFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraPanel ultraPanelAction;
        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private Infragistics.Win.Misc.UltraLabel lblTrnsType;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboTrnsType;
        private Infragistics.Win.Misc.UltraLabel lblTaxType;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboTaxType;
        private Infragistics.Win.Misc.UltraLabel lblTaxPer;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboTaxPer;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraButton btnSearchItem;
        private Infragistics.Win.Misc.UltraLabel lblDate;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboDateMode;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.Misc.UltraLabel lblViewMode;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboViewMode;
        private Infragistics.Win.Misc.UltraButton btnViewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewReport;
        private Infragistics.Win.Misc.UltraButton btnExportGrid;
        private Infragistics.Win.Misc.UltraButton btnToggleSelection;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
    }
}
