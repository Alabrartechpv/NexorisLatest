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
    partial class FrmSalesHoldReport
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
            this.lblViewMode = new Infragistics.Win.Misc.UltraLabel();
            this.cmbViewMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCustomer = new Infragistics.Win.Misc.UltraLabel();
            this.cmbCustomer = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblUser = new Infragistics.Win.Misc.UltraLabel();
            this.cmbUser = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblStatus = new Infragistics.Win.Misc.UltraLabel();
            this.cmbStatus = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.ultraPanelActionBar = new Infragistics.Win.Misc.UltraPanel();
            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnExportExcel = new Infragistics.Win.Misc.UltraButton();
            this.btnColumnChooser = new Infragistics.Win.Misc.UltraButton();
            this.btnHideSelection = new Infragistics.Win.Misc.UltraButton();
            this.ultraPanelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.gridHold = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.gridFooterPanel = new Infragistics.Win.Misc.UltraPanel();
            this.lblCount = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelSelection.ClientArea.SuspendLayout();
            this.ultraPanelSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbViewMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCustomer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbStatus)).BeginInit();
            this.ultraPanelActionBar.ClientArea.SuspendLayout();
            this.ultraPanelActionBar.SuspendLayout();
            this.ultraPanelGrid.ClientArea.SuspendLayout();
            this.ultraPanelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridHold)).BeginInit();
            this.gridFooterPanel.ClientArea.SuspendLayout();
            this.gridFooterPanel.SuspendLayout();
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
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblViewMode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbViewMode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblCustomer);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbCustomer);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblUser);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbUser);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblStatus);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbStatus);
            this.ultraPanelSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelSelection.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelSelection.Name = "ultraPanelSelection";
            this.ultraPanelSelection.Size = new System.Drawing.Size(1280, 80);
            this.ultraPanelSelection.TabIndex = 1;
            // 
            // lblDate
            // 
            this.lblDate.Location = new System.Drawing.Point(15, 14);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(70, 20);
            this.lblDate.TabIndex = 0;
            this.lblDate.Text = "Date";
            // 
            // ultraComboDateMode
            // 
            this.ultraComboDateMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboDateMode.Location = new System.Drawing.Point(90, 11);
            this.ultraComboDateMode.Name = "ultraComboDateMode";
            this.ultraComboDateMode.Size = new System.Drawing.Size(130, 22);
            this.ultraComboDateMode.TabIndex = 1;
            this.ultraComboDateMode.ValueChanged += new System.EventHandler(this.UltraComboDateMode_ValueChanged);
            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(232, 14);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(70, 20);
            this.lblFromDate.TabIndex = 2;
            this.lblFromDate.Text = "From Date";
            // 
            // dtFromDate
            // 
            this.dtFromDate.DateTime = new global::System.DateTime(2026, 1, 1, 0, 0, 0, 0);
            this.dtFromDate.FormatString = "dd/MM/yyyy";
            this.dtFromDate.Location = new System.Drawing.Point(305, 11);
            this.dtFromDate.Name = "dtFromDate";
            this.dtFromDate.Size = new System.Drawing.Size(125, 22);
            this.dtFromDate.TabIndex = 3;
            this.dtFromDate.Value = new global::System.DateTime(2026, 1, 1, 0, 0, 0, 0);
            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(442, 14);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(55, 20);
            this.lblToDate.TabIndex = 4;
            this.lblToDate.Text = "To Date";
            // 
            // dtToDate
            // 
            this.dtToDate.DateTime = new global::System.DateTime(2026, 1, 1, 0, 0, 0, 0);
            this.dtToDate.FormatString = "dd/MM/yyyy";
            this.dtToDate.Location = new System.Drawing.Point(500, 11);
            this.dtToDate.Name = "dtToDate";
            this.dtToDate.Size = new System.Drawing.Size(125, 22);
            this.dtToDate.TabIndex = 5;
            this.dtToDate.Value = new global::System.DateTime(2026, 1, 1, 0, 0, 0, 0);
            // 
            // lblViewMode
            // 
            this.lblViewMode.Location = new System.Drawing.Point(645, 14);
            this.lblViewMode.Name = "lblViewMode";
            this.lblViewMode.Size = new System.Drawing.Size(75, 20);
            this.lblViewMode.TabIndex = 6;
            this.lblViewMode.Text = "View Mode";
            // 
            // cmbViewMode
            // 
            this.cmbViewMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbViewMode.Location = new System.Drawing.Point(725, 11);
            this.cmbViewMode.Name = "cmbViewMode";
            this.cmbViewMode.Size = new System.Drawing.Size(140, 22);
            this.cmbViewMode.TabIndex = 7;
            this.cmbViewMode.ValueChanged += new System.EventHandler(this.CmbViewMode_ValueChanged);
            // 
            // lblCustomer
            // 
            this.lblCustomer.Location = new System.Drawing.Point(15, 45);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Size = new System.Drawing.Size(70, 20);
            this.lblCustomer.TabIndex = 8;
            this.lblCustomer.Text = "Customer";
            // 
            // cmbCustomer
            // 
            this.cmbCustomer.Location = new System.Drawing.Point(90, 42);
            this.cmbCustomer.Name = "cmbCustomer";
            this.cmbCustomer.Size = new System.Drawing.Size(205, 22);
            this.cmbCustomer.TabIndex = 9;
            // 
            // lblUser
            // 
            this.lblUser.Location = new System.Drawing.Point(310, 45);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(95, 20);
            this.lblUser.TabIndex = 10;
            this.lblUser.Text = "User / Cashier";
            // 
            // cmbUser
            // 
            this.cmbUser.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbUser.Location = new System.Drawing.Point(410, 42);
            this.cmbUser.Name = "cmbUser";
            this.cmbUser.Size = new System.Drawing.Size(165, 22);
            this.cmbUser.TabIndex = 11;
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(595, 45);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(50, 20);
            this.lblStatus.TabIndex = 12;
            this.lblStatus.Text = "Status";
            // 
            // cmbStatus
            // 
            this.cmbStatus.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbStatus.Location = new System.Drawing.Point(650, 42);
            this.cmbStatus.Name = "cmbStatus";
            this.cmbStatus.Size = new System.Drawing.Size(130, 22);
            this.cmbStatus.TabIndex = 13;
            // 
            // ultraPanelActionBar
            // 
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(223)))), ((int)(((byte)(238)))));
            appearance2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(154)))), ((int)(((byte)(198)))));
            this.ultraPanelActionBar.Appearance = appearance2;
            this.ultraPanelActionBar.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelActionBar.ClientArea
            // 
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnViewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnExportExcel);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnColumnChooser);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnHideSelection);
            this.ultraPanelActionBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelActionBar.Location = new System.Drawing.Point(0, 80);
            this.ultraPanelActionBar.Name = "ultraPanelActionBar";
            this.ultraPanelActionBar.Size = new System.Drawing.Size(1280, 44);
            this.ultraPanelActionBar.TabIndex = 2;
            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Location = new System.Drawing.Point(12, 6);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(115, 30);
            this.btnViewGrid.TabIndex = 0;
            this.btnViewGrid.Text = "&View Grid";
            this.btnViewGrid.Click += new System.EventHandler(this.BtnViewGrid_Click);
            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(135, 6);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(115, 30);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "&Preview Grid";
            this.btnPreviewGrid.Click += new System.EventHandler(this.BtnPreviewGrid_Click);
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Location = new System.Drawing.Point(258, 6);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(115, 30);
            this.btnExportExcel.TabIndex = 2;
            this.btnExportExcel.Text = "&Export Excel";
            this.btnExportExcel.Click += new System.EventHandler(this.BtnExportExcel_Click);
            // 
            // btnColumnChooser
            // 
            this.btnColumnChooser.Location = new System.Drawing.Point(381, 6);
            this.btnColumnChooser.Name = "btnColumnChooser";
            this.btnColumnChooser.Size = new System.Drawing.Size(125, 30);
            this.btnColumnChooser.TabIndex = 3;
            this.btnColumnChooser.Text = "&Column Chooser";
            this.btnColumnChooser.Click += new System.EventHandler(this.BtnColumnChooser_Click);
            // 
            // btnHideSelection
            // 
            this.btnHideSelection.Location = new System.Drawing.Point(514, 6);
            this.btnHideSelection.Name = "btnHideSelection";
            this.btnHideSelection.Size = new System.Drawing.Size(125, 30);
            this.btnHideSelection.TabIndex = 4;
            this.btnHideSelection.Text = "&Hide Selection";
            this.btnHideSelection.Click += new System.EventHandler(this.BtnHideSelection_Click);
            // 
            // ultraPanelGrid
            // 
            appearance3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ultraPanelGrid.Appearance = appearance3;
            // 
            // ultraPanelGrid.ClientArea
            // 
            this.ultraPanelGrid.ClientArea.Controls.Add(this.gridHold);
            this.ultraPanelGrid.ClientArea.Controls.Add(this.gridFooterPanel);
            this.ultraPanelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelGrid.Location = new System.Drawing.Point(0, 124);
            this.ultraPanelGrid.Name = "ultraPanelGrid";
            this.ultraPanelGrid.Size = new System.Drawing.Size(1280, 596);
            this.ultraPanelGrid.TabIndex = 3;
            // 
            // gridHold
            // 
            this.gridHold.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridHold.Location = new System.Drawing.Point(0, 0);
            this.gridHold.Name = "gridHold";
            this.gridHold.Size = new System.Drawing.Size(1280, 568);
            this.gridHold.TabIndex = 0;
            // 
            // gridFooterPanel
            // 
            appearance4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            appearance4.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(144)))), ((int)(((byte)(181)))), ((int)(((byte)(223)))));
            this.gridFooterPanel.Appearance = appearance4;
            this.gridFooterPanel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // gridFooterPanel.ClientArea
            // 
            this.gridFooterPanel.ClientArea.Controls.Add(this.lblCount);
            this.gridFooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gridFooterPanel.Location = new System.Drawing.Point(0, 568);
            this.gridFooterPanel.Name = "gridFooterPanel";
            this.gridFooterPanel.Size = new System.Drawing.Size(1280, 28);
            this.gridFooterPanel.TabIndex = 1;
            // 
            // lblCount
            // 
            this.lblCount.Location = new System.Drawing.Point(8, 5);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(150, 18);
            this.lblCount.TabIndex = 0;
            this.lblCount.Text = "Count : 0";
            // 
            // FrmSalesHoldReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.ultraPanelGrid);
            this.Controls.Add(this.ultraPanelActionBar);
            this.Controls.Add(this.ultraPanelSelection);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.Name = "FrmSalesHoldReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sales Hold Report";
            this.ultraPanelSelection.ClientArea.ResumeLayout(false);
            this.ultraPanelSelection.ClientArea.PerformLayout();
            this.ultraPanelSelection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboDateMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbViewMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCustomer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbUser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbStatus)).EndInit();
            this.ultraPanelActionBar.ClientArea.ResumeLayout(false);
            this.ultraPanelActionBar.ResumeLayout(false);
            this.ultraPanelGrid.ClientArea.ResumeLayout(false);
            this.ultraPanelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridHold)).EndInit();
            this.gridFooterPanel.ClientArea.ResumeLayout(false);
            this.gridFooterPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private Infragistics.Win.Misc.UltraPanel ultraPanelSelection;
        private Infragistics.Win.Misc.UltraLabel lblDate;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboDateMode;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFromDate;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtToDate;
        private Infragistics.Win.Misc.UltraLabel lblViewMode;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbViewMode;
        private Infragistics.Win.Misc.UltraLabel lblCustomer;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbCustomer;
        private Infragistics.Win.Misc.UltraLabel lblUser;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbUser;
        private Infragistics.Win.Misc.UltraLabel lblStatus;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbStatus;
        private Infragistics.Win.Misc.UltraPanel ultraPanelActionBar;
        private Infragistics.Win.Misc.UltraButton btnViewGrid;
        private Infragistics.Win.Misc.UltraButton btnPreviewGrid;
        private Infragistics.Win.Misc.UltraButton btnExportExcel;
        private Infragistics.Win.Misc.UltraButton btnColumnChooser;
        private Infragistics.Win.Misc.UltraButton btnHideSelection;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridHold;
        private Infragistics.Win.Misc.UltraPanel gridFooterPanel;
        private Infragistics.Win.Misc.UltraLabel lblCount;
    }
}
