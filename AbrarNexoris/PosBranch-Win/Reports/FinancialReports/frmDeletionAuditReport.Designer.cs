namespace PosBranch_Win.Reports.FinancialReports
{
    partial class frmDeletionAuditReport
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
            this.lblPeriod = new Infragistics.Win.Misc.UltraLabel();
            this.comboPeriod = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblTransType = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboTransactionType = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblUser = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboUser = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraComboEditor();

            this.ultraPanelAction = new Infragistics.Win.Misc.UltraPanel();
            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewReport = new Infragistics.Win.Misc.UltraButton();
            this.btnExportGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnColumnChooser = new Infragistics.Win.Misc.UltraButton();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnToggleSelection = new Infragistics.Win.Misc.UltraButton();

            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();

            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            this.ultraPanelAction.ClientArea.SuspendLayout();
            this.ultraPanelAction.SuspendLayout();
            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            this.ultraPanelGridFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboTransactionType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            this.SuspendLayout();

            // ultraPanelControls
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1100, 75);
            this.ultraPanelControls.TabIndex = 0;

            // Controls in ultraPanelControls
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPeriod);
            this.ultraPanelControls.ClientArea.Controls.Add(this.comboPeriod);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtTo);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblTransType);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboTransactionType);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblUser);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboUser);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtSearch);

            // Row 1
            this.lblPeriod.Location = new System.Drawing.Point(12, 12);
            this.lblPeriod.Name = "lblPeriod";
            this.lblPeriod.Size = new System.Drawing.Size(50, 20);
            this.lblPeriod.Text = "Period:";

            this.comboPeriod.Location = new System.Drawing.Point(65, 10);
            this.comboPeriod.Name = "comboPeriod";
            this.comboPeriod.Size = new System.Drawing.Size(110, 23);

            this.lblFromDate.Location = new System.Drawing.Point(185, 12);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(40, 20);
            this.lblFromDate.Text = "From";

            this.dtFrom.Location = new System.Drawing.Point(230, 10);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(105, 23);

            this.lblToDate.Location = new System.Drawing.Point(345, 12);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(25, 20);
            this.lblToDate.Text = "To";

            this.dtTo.Location = new System.Drawing.Point(375, 10);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(105, 23);

            this.lblTransType.Location = new System.Drawing.Point(490, 12);
            this.lblTransType.Name = "lblTransType";
            this.lblTransType.Size = new System.Drawing.Size(85, 20);
            this.lblTransType.Text = "Transaction:";

            this.ultraComboTransactionType.Location = new System.Drawing.Point(580, 10);
            this.ultraComboTransactionType.Name = "ultraComboTransactionType";
            this.ultraComboTransactionType.Size = new System.Drawing.Size(230, 23);

            this.lblUser.Location = new System.Drawing.Point(820, 12);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(40, 20);
            this.lblUser.Text = "User:";

            this.ultraComboUser.Location = new System.Drawing.Point(865, 10);
            this.ultraComboUser.Name = "ultraComboUser";
            this.ultraComboUser.Size = new System.Drawing.Size(140, 23);

            // Row 2
            this.lblSearch.Location = new System.Drawing.Point(12, 43);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(50, 20);
            this.lblSearch.Text = "Search:";

            this.txtSearch.Location = new System.Drawing.Point(65, 41);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(940, 23);

            // ultraPanelAction
            this.ultraPanelAction.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelAction.Location = new System.Drawing.Point(0, 75);
            this.ultraPanelAction.Name = "ultraPanelAction";
            this.ultraPanelAction.Size = new System.Drawing.Size(1100, 40);
            this.ultraPanelAction.TabIndex = 1;

            this.ultraPanelAction.ClientArea.Controls.Add(this.btnViewGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPreviewReport);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnExportGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnColumnChooser);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnClearFilters);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnToggleSelection);

            this.btnViewGrid.Location = new System.Drawing.Point(12, 6);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(110, 28);
            this.btnViewGrid.Text = "View Grid (F5)";

            this.btnPreviewGrid.Location = new System.Drawing.Point(128, 6);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(120, 28);
            this.btnPreviewGrid.Text = "Preview Grid (F6)";

            this.btnPreviewReport.Location = new System.Drawing.Point(254, 6);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(130, 28);
            this.btnPreviewReport.Text = "Preview Report (F8)";

            this.btnExportGrid.Location = new System.Drawing.Point(390, 6);
            this.btnExportGrid.Name = "btnExportGrid";
            this.btnExportGrid.Size = new System.Drawing.Size(110, 28);
            this.btnExportGrid.Text = "Export Grid (F7)";

            this.btnColumnChooser.Location = new System.Drawing.Point(506, 6);
            this.btnColumnChooser.Name = "btnColumnChooser";
            this.btnColumnChooser.Size = new System.Drawing.Size(120, 28);
            this.btnColumnChooser.Text = "Column Chooser";

            this.btnClearFilters.Location = new System.Drawing.Point(632, 6);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(110, 28);
            this.btnClearFilters.Text = "Clear Filters";

            this.btnToggleSelection.Location = new System.Drawing.Point(748, 6);
            this.btnToggleSelection.Name = "btnToggleSelection";
            this.btnToggleSelection.Size = new System.Drawing.Size(120, 28);
            this.btnToggleSelection.Text = "Hide Selection";

            // ultraPanelMaster
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 115);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1100, 535);
            this.ultraPanelMaster.TabIndex = 2;

            // ultraPanelGridFooter
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 507);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1100, 28);

            // gridReport
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Location = new System.Drawing.Point(0, 0);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1100, 507);

            this.ultraPanelMaster.ClientArea.Controls.Add(this.gridReport);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelAction);
            this.Controls.Add(this.ultraPanelControls);
            this.Name = "frmDeletionAuditReport";
            this.Text = "Deletion & Cancellation Audit Report (Admin Only)";

            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ResumeLayout(false);
            this.ultraPanelAction.ClientArea.ResumeLayout(false);
            this.ultraPanelAction.ResumeLayout(false);
            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            this.ultraPanelGridFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboTransactionType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboUser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraPanel ultraPanelAction;
        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;

        private Infragistics.Win.Misc.UltraLabel lblPeriod;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.Misc.UltraLabel lblTransType;
        private Infragistics.Win.Misc.UltraLabel lblUser;
        private Infragistics.Win.Misc.UltraLabel lblSearch;

        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboPeriod;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboTransactionType;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboUser;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor txtSearch;

        private Infragistics.Win.Misc.UltraButton btnViewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewReport;
        private Infragistics.Win.Misc.UltraButton btnExportGrid;
        private Infragistics.Win.Misc.UltraButton btnColumnChooser;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnToggleSelection;
    }
}
