namespace PosBranch_Win.Reports.FinancialReports
{
    partial class frmcustomerreportdetails
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
            this.ultraPanelControls = new Infragistics.Win.Misc.UltraPanel();
            this.lblCustomer = new Infragistics.Win.Misc.UltraLabel();
            this.txtCustomerSearch = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.btnCustomerPicker = new System.Windows.Forms.Button();
            this.ultraComboCustomer = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.chkPaymentDueOnly = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
            this.lblDate = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboDateMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.ultraPanelAction = new Infragistics.Win.Misc.UltraPanel();
            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewReport = new Infragistics.Win.Misc.UltraButton();
            this.btnExportGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnColumnChooser = new Infragistics.Win.Misc.UltraButton();
            this.btnExpandAll = new Infragistics.Win.Misc.UltraButton();
            this.btnCollapseAll = new Infragistics.Win.Misc.UltraButton();
            this.btnToggleSelection = new Infragistics.Win.Misc.UltraButton();
            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtCustomerSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboCustomer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPaymentDueOnly)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            this.ultraPanelAction.ClientArea.SuspendLayout();
            this.ultraPanelAction.SuspendLayout();
            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraPanelControls
            // 
            // 
            // ultraPanelControls.ClientArea
            // 
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblCustomer);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtCustomerSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.btnCustomerPicker);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboCustomer);
            this.ultraPanelControls.ClientArea.Controls.Add(this.chkPaymentDueOnly);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboDateMode);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtTo);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1364, 84);
            this.ultraPanelControls.TabIndex = 0;
            // 
            // lblCustomer
            // 
            this.lblCustomer.Location = new System.Drawing.Point(18, 15);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Size = new System.Drawing.Size(68, 20);
            this.lblCustomer.TabIndex = 0;
            this.lblCustomer.Text = "Customer";
            // 
            // txtCustomerSearch
            // 
            this.txtCustomerSearch.Location = new System.Drawing.Point(92, 12);
            this.txtCustomerSearch.Name = "txtCustomerSearch";
            this.txtCustomerSearch.Size = new System.Drawing.Size(180, 24);
            this.txtCustomerSearch.TabIndex = 1;
            // 
            // btnCustomerPicker
            // 
            this.btnCustomerPicker.Location = new System.Drawing.Point(276, 12);
            this.btnCustomerPicker.Name = "btnCustomerPicker";
            this.btnCustomerPicker.Size = new System.Drawing.Size(42, 25);
            this.btnCustomerPicker.TabIndex = 2;
            this.btnCustomerPicker.Text = "F11";
            this.btnCustomerPicker.UseVisualStyleBackColor = false;
            // 
            // ultraComboCustomer
            // 
            this.ultraComboCustomer.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
            this.ultraComboCustomer.Location = new System.Drawing.Point(324, 12);
            this.ultraComboCustomer.Name = "ultraComboCustomer";
            this.ultraComboCustomer.Size = new System.Drawing.Size(380, 24);
            this.ultraComboCustomer.TabIndex = 3;
            // 
            // chkPaymentDueOnly
            // 
            this.chkPaymentDueOnly.Location = new System.Drawing.Point(720, 14);
            this.chkPaymentDueOnly.Name = "chkPaymentDueOnly";
            this.chkPaymentDueOnly.Size = new System.Drawing.Size(160, 20);
            this.chkPaymentDueOnly.TabIndex = 4;
            this.chkPaymentDueOnly.Text = "Payment Due Only";
            // 
            // lblDate
            // 
            this.lblDate.Location = new System.Drawing.Point(18, 48);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(68, 20);
            this.lblDate.TabIndex = 5;
            this.lblDate.Text = "Date";
            // 
            // ultraComboDateMode
            // 
            this.ultraComboDateMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboDateMode.Location = new System.Drawing.Point(92, 45);
            this.ultraComboDateMode.Name = "ultraComboDateMode";
            this.ultraComboDateMode.Size = new System.Drawing.Size(180, 24);
            this.ultraComboDateMode.TabIndex = 6;
            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(280, 48);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(40, 20);
            this.lblFromDate.TabIndex = 7;
            this.lblFromDate.Text = "From";
            // 
            // dtFrom
            // 
            this.dtFrom.Location = new System.Drawing.Point(324, 45);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(130, 24);
            this.dtFrom.TabIndex = 8;
            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(468, 48);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(26, 20);
            this.lblToDate.TabIndex = 9;
            this.lblToDate.Text = "To";
            // 
            // dtTo
            // 
            this.dtTo.Location = new System.Drawing.Point(500, 45);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(130, 24);
            this.dtTo.TabIndex = 10;
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
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnClearFilters);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnColumnChooser);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnExpandAll);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnCollapseAll);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnToggleSelection);
            this.ultraPanelAction.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelAction.Location = new System.Drawing.Point(0, 84);
            this.ultraPanelAction.Name = "ultraPanelAction";
            this.ultraPanelAction.Size = new System.Drawing.Size(1364, 47);
            this.ultraPanelAction.TabIndex = 1;
            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Location = new System.Drawing.Point(10, 9);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(110, 28);
            this.btnViewGrid.TabIndex = 0;
            this.btnViewGrid.Text = "View Grid";
            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(126, 9);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(110, 28);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid";
            // 
            // btnPreviewReport
            // 
            this.btnPreviewReport.Location = new System.Drawing.Point(242, 9);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(115, 28);
            this.btnPreviewReport.TabIndex = 2;
            this.btnPreviewReport.Text = "Preview Report";
            // 
            // btnExportGrid
            // 
            this.btnExportGrid.Location = new System.Drawing.Point(363, 9);
            this.btnExportGrid.Name = "btnExportGrid";
            this.btnExportGrid.Size = new System.Drawing.Size(110, 28);
            this.btnExportGrid.TabIndex = 3;
            this.btnExportGrid.Text = "Export Grid";
            // 
            // btnClearFilters
            // 
            this.btnClearFilters.Location = new System.Drawing.Point(479, 9);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(80, 28);
            this.btnClearFilters.TabIndex = 4;
            this.btnClearFilters.Text = "Clear";
            // 
            // btnColumnChooser
            // 
            this.btnColumnChooser.Location = new System.Drawing.Point(565, 9);
            this.btnColumnChooser.Name = "btnColumnChooser";
            this.btnColumnChooser.Size = new System.Drawing.Size(125, 28);
            this.btnColumnChooser.TabIndex = 5;
            this.btnColumnChooser.Text = "Column Chooser";
            // 
            // btnExpandAll
            // 
            this.btnExpandAll.Location = new System.Drawing.Point(696, 9);
            this.btnExpandAll.Name = "btnExpandAll";
            this.btnExpandAll.Size = new System.Drawing.Size(100, 28);
            this.btnExpandAll.TabIndex = 6;
            this.btnExpandAll.Text = "Expand All";
            // 
            // btnCollapseAll
            // 
            this.btnCollapseAll.Location = new System.Drawing.Point(802, 9);
            this.btnCollapseAll.Name = "btnCollapseAll";
            this.btnCollapseAll.Size = new System.Drawing.Size(100, 28);
            this.btnCollapseAll.TabIndex = 7;
            this.btnCollapseAll.Text = "Collapse All";
            // 
            // btnToggleSelection
            // 
            this.btnToggleSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleSelection.Location = new System.Drawing.Point(1235, 9);
            this.btnToggleSelection.Name = "btnToggleSelection";
            this.btnToggleSelection.Size = new System.Drawing.Size(118, 28);
            this.btnToggleSelection.TabIndex = 8;
            this.btnToggleSelection.Text = "Hide Selection";
            // 
            // ultraPanelMaster
            // 
            // 
            // ultraPanelMaster.ClientArea
            // 
            this.ultraPanelMaster.ClientArea.Controls.Add(this.gridReport);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 131);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1364, 618);
            this.ultraPanelMaster.TabIndex = 2;
            // 
            // ultraPanelGridFooter
            // 
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 592);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1364, 26);
            this.ultraPanelGridFooter.TabIndex = 1;
            // 
            // gridReport
            // 
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Location = new System.Drawing.Point(0, 0);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1364, 592);
            this.gridReport.TabIndex = 0;
            this.gridReport.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // frmcustomerreportdetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1364, 749);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelAction);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "frmcustomerreportdetails";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Customer Details & Bills Report";
            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtCustomerSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboCustomer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPaymentDueOnly)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            this.ultraPanelAction.ClientArea.ResumeLayout(false);
            this.ultraPanelAction.ResumeLayout(false);
            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraLabel lblCustomer;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor txtCustomerSearch;
        private System.Windows.Forms.Button btnCustomerPicker;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboCustomer;
        private Infragistics.Win.UltraWinEditors.UltraCheckEditor chkPaymentDueOnly;
        private Infragistics.Win.Misc.UltraLabel lblDate;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboDateMode;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.Misc.UltraPanel ultraPanelAction;
        private Infragistics.Win.Misc.UltraButton btnViewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewReport;
        private Infragistics.Win.Misc.UltraButton btnExportGrid;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnColumnChooser;
        private Infragistics.Win.Misc.UltraButton btnExpandAll;
        private Infragistics.Win.Misc.UltraButton btnCollapseAll;
        private Infragistics.Win.Misc.UltraButton btnToggleSelection;
        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;
    }
}