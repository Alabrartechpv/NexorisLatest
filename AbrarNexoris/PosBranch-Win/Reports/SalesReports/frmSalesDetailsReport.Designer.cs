using System;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    partial class frmSalesReportMasterDetail
    {
        private IContainer components = null;

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
            Infragistics.Win.Appearance appearanceSelection = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceActionBar = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGridPanel = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceGrid = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceHeader = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceSelected = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceFooter = new Infragistics.Win.Appearance();

            this.ultraPanelSelection = new Infragistics.Win.Misc.UltraPanel();
            this.lblDate = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboDateMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFromDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtToDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblCustomer = new Infragistics.Win.Misc.UltraLabel();
            this.cmbCustomer = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblPaymentMode = new Infragistics.Win.Misc.UltraLabel();
            this.cmbPaymentMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSalesType = new Infragistics.Win.Misc.UltraLabel();
            this.cmbSalesType = new Infragistics.Win.UltraWinEditors.UltraComboEditor();

            this.ultraPanelActionBar = new Infragistics.Win.Misc.UltraPanel();
            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewReport = new Infragistics.Win.Misc.UltraButton();
            this.btnExportExcel = new Infragistics.Win.Misc.UltraButton();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnHideSelection = new Infragistics.Win.Misc.UltraButton();

            this.ultraPanelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridMaster = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.gridFooterPanel = new Infragistics.Win.Misc.UltraPanel();

            this.ultraPanelSelection.ClientArea.SuspendLayout();
            this.ultraPanelSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCustomer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymentMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbSalesType)).BeginInit();

            this.ultraPanelActionBar.ClientArea.SuspendLayout();
            this.ultraPanelActionBar.SuspendLayout();

            this.ultraPanelGrid.ClientArea.SuspendLayout();
            this.ultraPanelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridMaster)).BeginInit();

            this.gridFooterPanel.SuspendLayout();
            this.SuspendLayout();

            // 
            // ultraPanelSelection
            // 
            appearanceSelection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearanceSelection.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelSelection.Appearance = appearanceSelection;
            this.ultraPanelSelection.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelSelection.ClientArea
            // 
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.ultraComboDateMode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.dtFromDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.dtToDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblCustomer);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbCustomer);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblPaymentMode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbPaymentMode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblSalesType);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbSalesType);
            this.ultraPanelSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelSelection.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelSelection.Name = "ultraPanelSelection";
            this.ultraPanelSelection.Size = new System.Drawing.Size(1280, 80);
            this.ultraPanelSelection.TabIndex = 0;

            // 
            // lblCustomer
            // 
            this.lblCustomer.Location = new System.Drawing.Point(20, 15);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Size = new System.Drawing.Size(65, 20);
            this.lblCustomer.TabIndex = 0;
            this.lblCustomer.Text = "Customer";
            // 
            // cmbCustomer
            // 
            this.cmbCustomer.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbCustomer.Location = new System.Drawing.Point(90, 13);
            this.cmbCustomer.Name = "cmbCustomer";
            this.cmbCustomer.Size = new System.Drawing.Size(220, 24);
            this.cmbCustomer.TabIndex = 1;
            // 
            // lblPaymentMode
            // 
            this.lblPaymentMode.Location = new System.Drawing.Point(330, 15);
            this.lblPaymentMode.Name = "lblPaymentMode";
            this.lblPaymentMode.Size = new System.Drawing.Size(90, 20);
            this.lblPaymentMode.TabIndex = 2;
            this.lblPaymentMode.Text = "Payment Mode";
            // 
            // cmbPaymentMode
            // 
            this.cmbPaymentMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbPaymentMode.Location = new System.Drawing.Point(425, 13);
            this.cmbPaymentMode.Name = "cmbPaymentMode";
            this.cmbPaymentMode.Size = new System.Drawing.Size(140, 24);
            this.cmbPaymentMode.TabIndex = 3;
            // 
            // lblSalesType
            // 
            this.lblSalesType.Location = new System.Drawing.Point(585, 15);
            this.lblSalesType.Name = "lblSalesType";
            this.lblSalesType.Size = new System.Drawing.Size(70, 20);
            this.lblSalesType.TabIndex = 4;
            this.lblSalesType.Text = "Sales Type";
            // 
            // cmbSalesType
            // 
            this.cmbSalesType.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbSalesType.Location = new System.Drawing.Point(660, 13);
            this.cmbSalesType.Name = "cmbSalesType";
            this.cmbSalesType.Size = new System.Drawing.Size(140, 24);
            this.cmbSalesType.TabIndex = 5;
            // 
            // lblDate
            // 
            this.lblDate.Location = new System.Drawing.Point(20, 46);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(40, 20);
            this.lblDate.TabIndex = 6;
            this.lblDate.Text = "Date";
            // 
            // ultraComboDateMode
            // 
            this.ultraComboDateMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboDateMode.Location = new System.Drawing.Point(90, 44);
            this.ultraComboDateMode.Name = "ultraComboDateMode";
            this.ultraComboDateMode.Size = new System.Drawing.Size(120, 24);
            this.ultraComboDateMode.TabIndex = 7;
            this.ultraComboDateMode.ValueChanged += new System.EventHandler(this.UltraComboDateMode_ValueChanged);
            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(230, 46);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(40, 20);
            this.lblFromDate.TabIndex = 8;
            this.lblFromDate.Text = "From";
            // 
            // dtFromDate
            // 
            this.dtFromDate.DateTime = new global::System.DateTime(2026, 1, 1, 0, 0, 0, 0);
            this.dtFromDate.FormatString = "dd/MM/yyyy";
            this.dtFromDate.Location = new System.Drawing.Point(275, 44);
            this.dtFromDate.Name = "dtFromDate";
            this.dtFromDate.Size = new System.Drawing.Size(115, 24);
            this.dtFromDate.TabIndex = 9;
            this.dtFromDate.Value = new global::System.DateTime(2026, 1, 1, 0, 0, 0, 0);
            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(405, 46);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(25, 20);
            this.lblToDate.TabIndex = 10;
            this.lblToDate.Text = "To";
            // 
            // dtToDate
            // 
            this.dtToDate.DateTime = new global::System.DateTime(2026, 1, 1, 0, 0, 0, 0);
            this.dtToDate.FormatString = "dd/MM/yyyy";
            this.dtToDate.Location = new System.Drawing.Point(435, 44);
            this.dtToDate.Name = "dtToDate";
            this.dtToDate.Size = new System.Drawing.Size(115, 24);
            this.dtToDate.TabIndex = 11;
            this.dtToDate.Value = new global::System.DateTime(2026, 1, 1, 0, 0, 0, 0);

            // 
            // ultraPanelActionBar
            // 
            appearanceActionBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(223)))), ((int)(((byte)(238)))));
            appearanceActionBar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelActionBar.Appearance = appearanceActionBar;
            this.ultraPanelActionBar.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelActionBar.ClientArea
            // 
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnViewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewReport);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnExportExcel);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnClearFilters);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnHideSelection);
            this.ultraPanelActionBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelActionBar.Location = new System.Drawing.Point(0, 80);
            this.ultraPanelActionBar.Name = "ultraPanelActionBar";
            this.ultraPanelActionBar.Size = new System.Drawing.Size(1280, 45);
            this.ultraPanelActionBar.TabIndex = 1;

            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Location = new System.Drawing.Point(10, 8);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(136, 28);
            this.btnViewGrid.TabIndex = 0;
            this.btnViewGrid.Text = "View Grid";
            this.btnViewGrid.Click += new System.EventHandler(this.BtnViewGrid_Click);

            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(153, 8);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(138, 28);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid";
            this.btnPreviewGrid.Click += new System.EventHandler(this.BtnPreviewGrid_Click);

            // 
            // btnPreviewReport
            // 
            this.btnPreviewReport.Location = new System.Drawing.Point(298, 8);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(145, 28);
            this.btnPreviewReport.TabIndex = 2;
            this.btnPreviewReport.Text = "Preview Report";
            this.btnPreviewReport.Click += new System.EventHandler(this.BtnPreviewReport_Click);

            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Location = new System.Drawing.Point(450, 8);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(110, 28);
            this.btnExportExcel.TabIndex = 3;
            this.btnExportExcel.Text = "Export Grid";
            this.btnExportExcel.Click += new System.EventHandler(this.BtnExportExcel_Click);

            // 
            // btnClearFilters
            // 
            this.btnClearFilters.Location = new System.Drawing.Point(568, 8);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(110, 28);
            this.btnClearFilters.TabIndex = 4;
            this.btnClearFilters.Text = "Reset Filters";
            this.btnClearFilters.Click += new System.EventHandler(this.BtnClearFilters_Click);

            // 
            // btnHideSelection
            // 
            this.btnHideSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHideSelection.Location = new System.Drawing.Point(1135, 8);
            this.btnHideSelection.Name = "btnHideSelection";
            this.btnHideSelection.Size = new System.Drawing.Size(135, 28);
            this.btnHideSelection.TabIndex = 5;
            this.btnHideSelection.Text = "Hide Selection";
            this.btnHideSelection.Click += new System.EventHandler(this.BtnHideSelection_Click);

            // 
            // ultraPanelGrid
            // 
            appearanceGridPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearanceGridPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelGrid.Appearance = appearanceGridPanel;
            this.ultraPanelGrid.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelGrid.ClientArea
            // 
            this.ultraPanelGrid.ClientArea.Controls.Add(this.ultraGridMaster);
            this.ultraPanelGrid.ClientArea.Controls.Add(this.gridFooterPanel);
            this.ultraPanelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelGrid.Location = new System.Drawing.Point(0, 125);
            this.ultraPanelGrid.Name = "ultraPanelGrid";
            this.ultraPanelGrid.Size = new System.Drawing.Size(1280, 595);
            this.ultraPanelGrid.TabIndex = 2;

            // 
            // ultraGridMaster
            // 
            appearanceGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ultraGridMaster.DisplayLayout.Appearance = appearanceGrid;
            this.ultraGridMaster.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            appearanceHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceHeader.ForeColor = System.Drawing.Color.White;
            this.ultraGridMaster.DisplayLayout.Override.HeaderAppearance = appearanceHeader;
            appearanceSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(126)))), ((int)(((byte)(245)))));
            appearanceSelected.ForeColor = System.Drawing.Color.White;
            this.ultraGridMaster.DisplayLayout.Override.SelectedRowAppearance = appearanceSelected;
            this.ultraGridMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridMaster.Location = new System.Drawing.Point(0, 0);
            this.ultraGridMaster.Name = "ultraGridMaster";
            this.ultraGridMaster.Size = new System.Drawing.Size(1280, 569);
            this.ultraGridMaster.TabIndex = 0;
            this.ultraGridMaster.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // 
            // gridFooterPanel
            // 
            appearanceFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearanceFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.gridFooterPanel.Appearance = appearanceFooter;
            this.gridFooterPanel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.gridFooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gridFooterPanel.Location = new System.Drawing.Point(0, 569);
            this.gridFooterPanel.Name = "gridFooterPanel";
            this.gridFooterPanel.Size = new System.Drawing.Size(1280, 26);
            this.gridFooterPanel.TabIndex = 1;

            // 
            // frmSalesReportMasterDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.ultraPanelGrid);
            this.Controls.Add(this.ultraPanelActionBar);
            this.Controls.Add(this.ultraPanelSelection);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(0, 0);
            this.Name = "frmSalesReportMasterDetail";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sales Details Report";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.ultraPanelSelection.ClientArea.ResumeLayout(false);
            this.ultraPanelSelection.ClientArea.PerformLayout();
            this.ultraPanelSelection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCustomer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymentMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbSalesType)).EndInit();

            this.ultraPanelActionBar.ClientArea.ResumeLayout(false);
            this.ultraPanelActionBar.ResumeLayout(false);

            this.ultraPanelGrid.ClientArea.ResumeLayout(false);
            this.ultraPanelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridMaster)).EndInit();

            this.gridFooterPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelSelection;
        private Infragistics.Win.Misc.UltraLabel lblDate;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboDateMode;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFromDate;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtToDate;
        private Infragistics.Win.Misc.UltraLabel lblCustomer;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbCustomer;
        private Infragistics.Win.Misc.UltraLabel lblPaymentMode;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbPaymentMode;
        private Infragistics.Win.Misc.UltraLabel lblSalesType;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbSalesType;

        private Infragistics.Win.Misc.UltraPanel ultraPanelActionBar;
        private Infragistics.Win.Misc.UltraButton btnViewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewReport;
        private Infragistics.Win.Misc.UltraButton btnExportExcel;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnHideSelection;

        private Infragistics.Win.Misc.UltraPanel ultraPanelGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridMaster;
        private Infragistics.Win.Misc.UltraPanel gridFooterPanel;
    }
}