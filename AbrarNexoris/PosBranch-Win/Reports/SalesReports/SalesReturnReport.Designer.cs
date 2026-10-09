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
    partial class SalesReturnReport
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
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            this.ultraPanelSelection = new Infragistics.Win.Misc.UltraPanel();
            this.lblDate = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboDateMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFromDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtToDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblReturnNo = new Infragistics.Win.Misc.UltraLabel();
            this.txtReturnNo = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblPaymentMode = new Infragistics.Win.Misc.UltraLabel();
            this.cmbPaymentMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCustomer = new Infragistics.Win.Misc.UltraLabel();
            this.cmbCustomer = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.ultraPanelActionBar = new Infragistics.Win.Misc.UltraPanel();
            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewReport = new Infragistics.Win.Misc.UltraButton();
            this.btnExportExcel = new Infragistics.Win.Misc.UltraButton();
            this.btnColumnChooser = new Infragistics.Win.Misc.UltraButton();
            this.btnHideSelection = new Infragistics.Win.Misc.UltraButton();
            this.ultraPanelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.gridFooterPanel = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridMaster = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelSelection.ClientArea.SuspendLayout();
            this.ultraPanelSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtReturnNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymentMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCustomer)).BeginInit();
            this.ultraPanelActionBar.ClientArea.SuspendLayout();
            this.ultraPanelActionBar.SuspendLayout();
            this.ultraPanelGrid.ClientArea.SuspendLayout();
            this.ultraPanelGrid.SuspendLayout();
            this.gridFooterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridMaster)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraPanelSelection
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearance1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelSelection.Appearance = appearance1;
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
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblReturnNo);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.txtReturnNo);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblPaymentMode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbPaymentMode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblCustomer);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbCustomer);
            this.ultraPanelSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelSelection.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelSelection.Name = "ultraPanelSelection";
            this.ultraPanelSelection.Size = new System.Drawing.Size(1280, 80);
            this.ultraPanelSelection.TabIndex = 0;
            // 
            // lblDate
            // 
            this.lblDate.Location = new System.Drawing.Point(15, 14);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(75, 20);
            this.lblDate.TabIndex = 0;
            this.lblDate.Text = "Date";
            // 
            // ultraComboDateMode
            // 
            this.ultraComboDateMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboDateMode.Location = new System.Drawing.Point(95, 11);
            this.ultraComboDateMode.Name = "ultraComboDateMode";
            this.ultraComboDateMode.Size = new System.Drawing.Size(120, 25);
            this.ultraComboDateMode.TabIndex = 1;
            this.ultraComboDateMode.ValueChanged += new System.EventHandler(this.UltraComboDateMode_ValueChanged);
            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(230, 14);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(65, 20);
            this.lblFromDate.TabIndex = 2;
            this.lblFromDate.Text = "From Date";
            // 
            // dtFromDate
            // 
            this.dtFromDate.DateTime = new System.DateTime(2026, 1, 1, 0, 0, 0, 0);
            this.dtFromDate.FormatString = "dd/MM/yyyy";
            this.dtFromDate.Location = new System.Drawing.Point(300, 11);
            this.dtFromDate.Name = "dtFromDate";
            this.dtFromDate.Size = new System.Drawing.Size(115, 25);
            this.dtFromDate.TabIndex = 3;
            this.dtFromDate.Value = new System.DateTime(2026, 1, 1, 0, 0, 0, 0);
            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(430, 14);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(55, 20);
            this.lblToDate.TabIndex = 4;
            this.lblToDate.Text = "To Date";
            // 
            // dtToDate
            // 
            this.dtToDate.DateTime = new System.DateTime(2026, 1, 1, 0, 0, 0, 0);
            this.dtToDate.FormatString = "dd/MM/yyyy";
            this.dtToDate.Location = new System.Drawing.Point(490, 11);
            this.dtToDate.Name = "dtToDate";
            this.dtToDate.Size = new System.Drawing.Size(115, 25);
            this.dtToDate.TabIndex = 5;
            this.dtToDate.Value = new System.DateTime(2026, 1, 1, 0, 0, 0, 0);
            // 
            // lblReturnNo
            // 
            this.lblReturnNo.Location = new System.Drawing.Point(620, 14);
            this.lblReturnNo.Name = "lblReturnNo";
            this.lblReturnNo.Size = new System.Drawing.Size(70, 20);
            this.lblReturnNo.TabIndex = 6;
            this.lblReturnNo.Text = "Return / Inv";
            // 
            // txtReturnNo
            // 
            this.txtReturnNo.Location = new System.Drawing.Point(695, 11);
            this.txtReturnNo.Name = "txtReturnNo";
            this.txtReturnNo.NullText = "Search Return/Inv...";
            this.txtReturnNo.Size = new System.Drawing.Size(145, 25);
            this.txtReturnNo.TabIndex = 7;
            // 
            // lblPaymentMode
            // 
            this.lblPaymentMode.Location = new System.Drawing.Point(15, 44);
            this.lblPaymentMode.Name = "lblPaymentMode";
            this.lblPaymentMode.Size = new System.Drawing.Size(90, 20);
            this.lblPaymentMode.TabIndex = 8;
            this.lblPaymentMode.Text = "Payment Mode";
            // 
            // cmbPaymentMode
            // 
            this.cmbPaymentMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbPaymentMode.Location = new System.Drawing.Point(110, 41);
            this.cmbPaymentMode.Name = "cmbPaymentMode";
            this.cmbPaymentMode.Size = new System.Drawing.Size(160, 25);
            this.cmbPaymentMode.TabIndex = 9;
            // 
            // lblCustomer
            // 
            this.lblCustomer.Location = new System.Drawing.Point(290, 44);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Size = new System.Drawing.Size(65, 20);
            this.lblCustomer.TabIndex = 10;
            this.lblCustomer.Text = "Customer";
            // 
            // cmbCustomer
            // 
            this.cmbCustomer.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbCustomer.Location = new System.Drawing.Point(360, 41);
            this.cmbCustomer.Name = "cmbCustomer";
            this.cmbCustomer.Size = new System.Drawing.Size(245, 25);
            this.cmbCustomer.TabIndex = 11;
            // 
            // ultraPanelActionBar
            // 
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            appearance2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelActionBar.Appearance = appearance2;
            this.ultraPanelActionBar.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelActionBar.ClientArea
            // 
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnViewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewReport);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnExportExcel);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnColumnChooser);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnHideSelection);
            this.ultraPanelActionBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelActionBar.Location = new System.Drawing.Point(0, 80);
            this.ultraPanelActionBar.Name = "ultraPanelActionBar";
            this.ultraPanelActionBar.Size = new System.Drawing.Size(1280, 42);
            this.ultraPanelActionBar.TabIndex = 1;
            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Location = new System.Drawing.Point(15, 6);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(125, 30);
            this.btnViewGrid.TabIndex = 0;
            this.btnViewGrid.Text = "View Grid";
            this.btnViewGrid.Click += new System.EventHandler(this.BtnViewGrid_Click);
            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(150, 6);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(125, 30);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid";
            this.btnPreviewGrid.Click += new System.EventHandler(this.BtnPreviewGrid_Click);
            // 
            // btnPreviewReport
            // 
            this.btnPreviewReport.Location = new System.Drawing.Point(285, 6);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(125, 30);
            this.btnPreviewReport.TabIndex = 2;
            this.btnPreviewReport.Text = "Preview Report";
            this.btnPreviewReport.Click += new System.EventHandler(this.BtnPreviewReport_Click);
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Location = new System.Drawing.Point(420, 6);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(125, 30);
            this.btnExportExcel.TabIndex = 3;
            this.btnExportExcel.Text = "Export Excel";
            this.btnExportExcel.Click += new System.EventHandler(this.BtnExportExcel_Click);
            // 
            // btnColumnChooser
            // 
            this.btnColumnChooser.Location = new System.Drawing.Point(555, 6);
            this.btnColumnChooser.Name = "btnColumnChooser";
            this.btnColumnChooser.Size = new System.Drawing.Size(125, 30);
            this.btnColumnChooser.TabIndex = 4;
            this.btnColumnChooser.Text = "Column Chooser";
            this.btnColumnChooser.Click += new System.EventHandler(this.BtnColumnChooser_Click);
            // 
            // btnHideSelection
            // 
            this.btnHideSelection.Location = new System.Drawing.Point(690, 6);
            this.btnHideSelection.Name = "btnHideSelection";
            this.btnHideSelection.Size = new System.Drawing.Size(125, 30);
            this.btnHideSelection.TabIndex = 5;
            this.btnHideSelection.Text = "Hide Selection";
            this.btnHideSelection.Click += new System.EventHandler(this.BtnHideSelection_Click);
            // 
            // ultraPanelGrid
            // 
            appearance3.BackColor = System.Drawing.Color.White;
            this.ultraPanelGrid.Appearance = appearance3;
            // 
            // ultraPanelGrid.ClientArea
            // 
            this.ultraPanelGrid.ClientArea.Controls.Add(this.gridFooterPanel);
            this.ultraPanelGrid.ClientArea.Controls.Add(this.ultraGridMaster);
            this.ultraPanelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelGrid.Location = new System.Drawing.Point(0, 122);
            this.ultraPanelGrid.Name = "ultraPanelGrid";
            this.ultraPanelGrid.Size = new System.Drawing.Size(1280, 598);
            this.ultraPanelGrid.TabIndex = 2;
            // 
            // gridFooterPanel
            // 
            this.gridFooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            appearance4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearance4.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearance4.BackGradientStyle = Infragistics.Win.GradientStyle.None;
            appearance4.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(144)))), ((int)(((byte)(181)))), ((int)(((byte)(223)))));
            this.gridFooterPanel.Appearance = appearance4;
            this.gridFooterPanel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.gridFooterPanel.Location = new System.Drawing.Point(0, 572);
            this.gridFooterPanel.Name = "gridFooterPanel";
            this.gridFooterPanel.Size = new System.Drawing.Size(1280, 26);
            this.gridFooterPanel.TabIndex = 1;
            // 
            // ultraGridMaster
            // 
            this.ultraGridMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridMaster.Location = new System.Drawing.Point(0, 0);
            this.ultraGridMaster.Name = "ultraGridMaster";
            this.ultraGridMaster.Size = new System.Drawing.Size(1280, 572);
            this.ultraGridMaster.TabIndex = 0;
            // 
            // SalesReturnReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = false;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.ultraPanelGrid);
            this.Controls.Add(this.ultraPanelActionBar);
            this.Controls.Add(this.ultraPanelSelection);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "SalesReturnReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sales Return Report - Master Detail View";
            this.ultraPanelSelection.ClientArea.ResumeLayout(false);
            this.ultraPanelSelection.ClientArea.PerformLayout();
            this.ultraPanelSelection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtReturnNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPaymentMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCustomer)).EndInit();
            this.ultraPanelActionBar.ClientArea.ResumeLayout(false);
            this.ultraPanelActionBar.ResumeLayout(false);
            this.ultraPanelGrid.ClientArea.ResumeLayout(false);
            this.ultraPanelGrid.ResumeLayout(false);
            this.gridFooterPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridMaster)).EndInit();
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
        private Infragistics.Win.Misc.UltraLabel lblReturnNo;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtReturnNo;
        private Infragistics.Win.Misc.UltraLabel lblPaymentMode;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbPaymentMode;
        private Infragistics.Win.Misc.UltraLabel lblCustomer;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbCustomer;

        private Infragistics.Win.Misc.UltraPanel ultraPanelActionBar;
        private Infragistics.Win.Misc.UltraButton btnViewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewReport;
        private Infragistics.Win.Misc.UltraButton btnExportExcel;
        private Infragistics.Win.Misc.UltraButton btnColumnChooser;
        private Infragistics.Win.Misc.UltraButton btnHideSelection;

        private Infragistics.Win.Misc.UltraPanel ultraPanelGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridMaster;
        private Infragistics.Win.Misc.UltraPanel gridFooterPanel;
    }
}
