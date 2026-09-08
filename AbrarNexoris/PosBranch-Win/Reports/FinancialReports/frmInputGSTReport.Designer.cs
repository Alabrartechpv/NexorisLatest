namespace PosBranch_Win.Reports.FinancialReports
{
    partial class frmInputGSTReport
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
            this.ultraPanelAction = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();

            this.lblReportView = new Infragistics.Win.Misc.UltraLabel();
            this.lblDate = new Infragistics.Win.Misc.UltraLabel();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();

            this.ultraComboReportView = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraComboEditor();

            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewReport = new Infragistics.Win.Misc.UltraButton();
            this.btnExportGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnToggleSelection = new Infragistics.Win.Misc.UltraButton();

            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            this.ultraPanelAction.ClientArea.SuspendLayout();
            this.ultraPanelAction.SuspendLayout();
            this.ultraPanelMaster.ClientArea.SuspendLayout();
            this.ultraPanelMaster.SuspendLayout();
            this.ultraPanelGridFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboReportView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            this.SuspendLayout();

            // ultraPanelControls
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1024, 75);
            this.ultraPanelControls.TabIndex = 0;

            // ultraPanelControls.ClientArea Controls
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblReportView);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboReportView);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtTo);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtSearch);

            // Controls Layout
            this.lblReportView.Location = new System.Drawing.Point(15, 12);
            this.lblReportView.Name = "lblReportView";
            this.lblReportView.Size = new System.Drawing.Size(90, 20);
            this.lblReportView.Text = "Report View:";

            this.ultraComboReportView.Location = new System.Drawing.Point(110, 10);
            this.ultraComboReportView.Name = "ultraComboReportView";
            this.ultraComboReportView.Size = new System.Drawing.Size(220, 23);

            this.lblDate.Location = new System.Drawing.Point(350, 12);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(50, 20);
            this.lblDate.Text = "Period:";

            this.lblFromDate.Location = new System.Drawing.Point(405, 12);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(40, 20);
            this.lblFromDate.Text = "From";

            this.dtFrom.Location = new System.Drawing.Point(450, 10);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(120, 23);

            this.lblToDate.Location = new System.Drawing.Point(585, 12);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(25, 20);
            this.lblToDate.Text = "To";

            this.dtTo.Location = new System.Drawing.Point(615, 10);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(120, 23);

            this.lblSearch.Location = new System.Drawing.Point(15, 43);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(90, 20);
            this.lblSearch.Text = "Search:";

            this.txtSearch.Location = new System.Drawing.Point(110, 41);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(625, 23);

            // ultraPanelAction
            this.ultraPanelAction.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelAction.Location = new System.Drawing.Point(0, 75);
            this.ultraPanelAction.Name = "ultraPanelAction";
            this.ultraPanelAction.Size = new System.Drawing.Size(1024, 40);
            this.ultraPanelAction.TabIndex = 1;

            this.ultraPanelAction.ClientArea.Controls.Add(this.btnViewGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnPreviewReport);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnExportGrid);
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

            this.btnToggleSelection.Location = new System.Drawing.Point(506, 6);
            this.btnToggleSelection.Name = "btnToggleSelection";
            this.btnToggleSelection.Size = new System.Drawing.Size(120, 28);
            this.btnToggleSelection.Text = "Hide Selection";

            // ultraPanelMaster
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 115);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1024, 535);
            this.ultraPanelMaster.TabIndex = 2;

            // ultraPanelGridFooter
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(0, 507);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1024, 28);

            // gridReport
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Location = new System.Drawing.Point(0, 0);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1024, 507);

            this.ultraPanelMaster.ClientArea.Controls.Add(this.gridReport);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 650);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelAction);
            this.Controls.Add(this.ultraPanelControls);
            this.Name = "frmInputGSTReport";
            this.Text = "Input GST & ITC Report";

            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ResumeLayout(false);
            this.ultraPanelAction.ClientArea.ResumeLayout(false);
            this.ultraPanelAction.ResumeLayout(false);
            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            this.ultraPanelGridFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboReportView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraPanel ultraPanelAction;
        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;

        private Infragistics.Win.Misc.UltraLabel lblReportView;
        private Infragistics.Win.Misc.UltraLabel lblDate;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.Misc.UltraLabel lblSearch;

        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboReportView;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor txtSearch;

        private Infragistics.Win.Misc.UltraButton btnViewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewReport;
        private Infragistics.Win.Misc.UltraButton btnExportGrid;
        private Infragistics.Win.Misc.UltraButton btnToggleSelection;
    }
}
