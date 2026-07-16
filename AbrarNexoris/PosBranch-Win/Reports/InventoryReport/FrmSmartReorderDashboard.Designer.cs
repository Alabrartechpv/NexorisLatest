using System;
using System.ComponentModel;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.Reports.InventoryReport
{
    partial class FrmSmartReorderDashboard
    {
        private IContainer components = null;
        private UltraPanel ultraPanelSelection;
        private UltraPanel ultraPanelActionBar;
        private UltraPanel ultraPanelGrid;
        private UltraPanel gridFooterPanel;
        private UltraLabel lblItemNo;
        private UltraLabel lblFromBarcode;
        private UltraLabel lblCategory;
        private UltraLabel lblGroup;
        private UltraLabel lblMoreOptions;
        private UltraLabel lblAlert;
        private UltraComboEditor cmbItemNoMode;
        private UltraTextEditor txtFromBarcode;
        private UltraComboEditor cmbCategory;
        private UltraComboEditor cmbGroup;
        private UltraCheckEditor chkShowOnlyExceptions;
        private UltraComboEditor cmbMoreOptions;
        private UltraComboEditor cmbAlert;
        private UltraButton btnViewGrid;
        private UltraButton btnGeneratePO;
        private UltraButton btnGenBranchPO;
        private UltraButton btnRefreshStats;
        private UltraButton btnHideSelection;
        private UltraGrid ultraGridMaster;
        private UltraLabel lblCount;
        private UltraLabel lblExceptionCount;

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
            this.ultraPanelSelection = new Infragistics.Win.Misc.UltraPanel();
            this.lblItemNo = new Infragistics.Win.Misc.UltraLabel();
            this.cmbItemNoMode = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromBarcode = new Infragistics.Win.Misc.UltraLabel();
            this.txtFromBarcode = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblCategory = new Infragistics.Win.Misc.UltraLabel();
            this.cmbCategory = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblGroup = new Infragistics.Win.Misc.UltraLabel();
            this.cmbGroup = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblAlert = new Infragistics.Win.Misc.UltraLabel();
            this.cmbAlert = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.chkShowOnlyExceptions = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
            this.lblMoreOptions = new Infragistics.Win.Misc.UltraLabel();
            this.cmbMoreOptions = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.ultraPanelActionBar = new Infragistics.Win.Misc.UltraPanel();
            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnGeneratePO = new Infragistics.Win.Misc.UltraButton();
            this.btnGenBranchPO = new Infragistics.Win.Misc.UltraButton();
            this.btnRefreshStats = new Infragistics.Win.Misc.UltraButton();
            this.btnHideSelection = new Infragistics.Win.Misc.UltraButton();
            this.ultraPanelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridMaster = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.gridFooterPanel = new Infragistics.Win.Misc.UltraPanel();
            this.lblCount = new Infragistics.Win.Misc.UltraLabel();
            this.lblExceptionCount = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelSelection.ClientArea.SuspendLayout();
            this.ultraPanelSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbItemNoMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFromBarcode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbAlert)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowOnlyExceptions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbMoreOptions)).BeginInit();
            this.ultraPanelActionBar.ClientArea.SuspendLayout();
            this.ultraPanelActionBar.SuspendLayout();
            this.ultraPanelGrid.ClientArea.SuspendLayout();
            this.ultraPanelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridMaster)).BeginInit();
            this.gridFooterPanel.ClientArea.SuspendLayout();
            this.gridFooterPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ultraPanelSelection
            // 
            // 
            // ultraPanelSelection.ClientArea
            // 
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblItemNo);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbItemNoMode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblFromBarcode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.txtFromBarcode);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblCategory);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbCategory);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblGroup);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbGroup);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblAlert);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbAlert);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.chkShowOnlyExceptions);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblMoreOptions);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbMoreOptions);
            this.ultraPanelSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelSelection.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelSelection.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ultraPanelSelection.Name = "ultraPanelSelection";
            this.ultraPanelSelection.Size = new System.Drawing.Size(1097, 102);
            this.ultraPanelSelection.TabIndex = 0;
            // 
            // lblItemNo
            // 
            this.lblItemNo.Location = new System.Drawing.Point(39, 15);
            this.lblItemNo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblItemNo.Name = "lblItemNo";
            this.lblItemNo.Size = new System.Drawing.Size(63, 19);
            this.lblItemNo.TabIndex = 0;
            this.lblItemNo.Text = "Item No.";
            // 
            // cmbItemNoMode
            // 
            this.cmbItemNoMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbItemNoMode.Location = new System.Drawing.Point(111, 12);
            this.cmbItemNoMode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbItemNoMode.Name = "cmbItemNoMode";
            this.cmbItemNoMode.Size = new System.Drawing.Size(137, 21);
            this.cmbItemNoMode.TabIndex = 1;
            // 
            // lblFromBarcode
            // 
            this.lblFromBarcode.AutoSize = true;
            this.lblFromBarcode.Location = new System.Drawing.Point(315, 15);
            this.lblFromBarcode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblFromBarcode.Name = "lblFromBarcode";
            this.lblFromBarcode.Size = new System.Drawing.Size(46, 14);
            this.lblFromBarcode.TabIndex = 2;
            this.lblFromBarcode.Text = "Barcode";
            // 
            // txtFromBarcode
            // 
            this.txtFromBarcode.Location = new System.Drawing.Point(370, 12);
            this.txtFromBarcode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtFromBarcode.Name = "txtFromBarcode";
            this.txtFromBarcode.Size = new System.Drawing.Size(141, 21);
            this.txtFromBarcode.TabIndex = 3;
            // 
            // lblCategory
            // 
            this.lblCategory.Location = new System.Drawing.Point(51, 44);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(51, 19);
            this.lblCategory.TabIndex = 6;
            this.lblCategory.Text = "Category";
            // 
            // cmbCategory
            // 
            this.cmbCategory.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbCategory.Location = new System.Drawing.Point(111, 41);
            this.cmbCategory.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(137, 21);
            this.cmbCategory.TabIndex = 7;
            // 
            // lblGroup
            // 
            this.lblGroup.Location = new System.Drawing.Point(327, 44);
            this.lblGroup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(39, 19);
            this.lblGroup.TabIndex = 8;
            this.lblGroup.Text = "Group";
            // 
            // cmbGroup
            // 
            this.cmbGroup.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbGroup.Location = new System.Drawing.Point(370, 41);
            this.cmbGroup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbGroup.Name = "cmbGroup";
            this.cmbGroup.Size = new System.Drawing.Size(137, 21);
            this.cmbGroup.TabIndex = 9;
            this.cmbGroup.ValueChanged += new System.EventHandler(this.CmbGroup_ValueChanged);
            // 
            // lblAlert
            // 
            this.lblAlert.Location = new System.Drawing.Point(575, 44);
            this.lblAlert.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblAlert.Name = "lblAlert";
            this.lblAlert.Size = new System.Drawing.Size(39, 19);
            this.lblAlert.TabIndex = 10;
            this.lblAlert.Text = "Alert";
            // 
            // cmbAlert
            // 
            this.cmbAlert.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbAlert.Location = new System.Drawing.Point(617, 41);
            this.cmbAlert.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbAlert.Name = "cmbAlert";
            this.cmbAlert.Size = new System.Drawing.Size(141, 21);
            this.cmbAlert.TabIndex = 11;
            this.cmbAlert.ValueChanged += new System.EventHandler(this.CmbAlert_ValueChanged);
            // 
            // chkShowOnlyExceptions
            // 
            this.chkShowOnlyExceptions.Location = new System.Drawing.Point(789, 43);
            this.chkShowOnlyExceptions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chkShowOnlyExceptions.Name = "chkShowOnlyExceptions";
            this.chkShowOnlyExceptions.Size = new System.Drawing.Size(146, 16);
            this.chkShowOnlyExceptions.TabIndex = 12;
            this.chkShowOnlyExceptions.Text = "Show Only Exceptions";
            this.chkShowOnlyExceptions.CheckedChanged += new System.EventHandler(this.ChkShowOnlyExceptions_CheckedChanged);
            // 
            // lblMoreOptions
            // 
            this.lblMoreOptions.Location = new System.Drawing.Point(26, 73);
            this.lblMoreOptions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblMoreOptions.Name = "lblMoreOptions";
            this.lblMoreOptions.Size = new System.Drawing.Size(77, 19);
            this.lblMoreOptions.TabIndex = 13;
            this.lblMoreOptions.Text = "More Options";
            // 
            // cmbMoreOptions
            // 
            this.cmbMoreOptions.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbMoreOptions.Location = new System.Drawing.Point(111, 71);
            this.cmbMoreOptions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbMoreOptions.Name = "cmbMoreOptions";
            this.cmbMoreOptions.Size = new System.Drawing.Size(965, 21);
            this.cmbMoreOptions.TabIndex = 14;
            // 
            // ultraPanelActionBar
            // 
            // 
            // ultraPanelActionBar.ClientArea
            // 
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnViewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnGeneratePO);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnGenBranchPO);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnRefreshStats);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnHideSelection);
            this.ultraPanelActionBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelActionBar.Location = new System.Drawing.Point(0, 102);
            this.ultraPanelActionBar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ultraPanelActionBar.Name = "ultraPanelActionBar";
            this.ultraPanelActionBar.Size = new System.Drawing.Size(1097, 34);
            this.ultraPanelActionBar.TabIndex = 1;
            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Location = new System.Drawing.Point(14, 6);
            this.btnViewGrid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(94, 23);
            this.btnViewGrid.TabIndex = 0;
            this.btnViewGrid.Text = "View Grid";
            this.btnViewGrid.Click += new System.EventHandler(this.BtnViewGrid_Click);
            // 
            // btnGeneratePO
            // 
            this.btnGeneratePO.Location = new System.Drawing.Point(111, 6);
            this.btnGeneratePO.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnGeneratePO.Name = "btnGeneratePO";
            this.btnGeneratePO.Size = new System.Drawing.Size(101, 23);
            this.btnGeneratePO.TabIndex = 3;
            this.btnGeneratePO.Text = "Generate PO";
            this.btnGeneratePO.Click += new System.EventHandler(this.BtnGeneratePO_Click);
            // 
            // btnGenBranchPO
            // 
            this.btnGenBranchPO.Location = new System.Drawing.Point(218, 6);
            this.btnGenBranchPO.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnGenBranchPO.Name = "btnGenBranchPO";
            this.btnGenBranchPO.Size = new System.Drawing.Size(110, 23);
            this.btnGenBranchPO.TabIndex = 4;
            this.btnGenBranchPO.Text = "Gen. Branch PO";
            this.btnGenBranchPO.Click += new System.EventHandler(this.BtnGenBranchPO_Click);
            // 
            // btnRefreshStats
            // 
            this.btnRefreshStats.Location = new System.Drawing.Point(332, 6);
            this.btnRefreshStats.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnRefreshStats.Name = "btnRefreshStats";
            this.btnRefreshStats.Size = new System.Drawing.Size(101, 23);
            this.btnRefreshStats.TabIndex = 5;
            this.btnRefreshStats.Text = "Refresh Stats";
            this.btnRefreshStats.Click += new System.EventHandler(this.BtnRefreshStats_Click);
            // 
            // btnHideSelection
            // 
            this.btnHideSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHideSelection.Location = new System.Drawing.Point(981, 6);
            this.btnHideSelection.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnHideSelection.Name = "btnHideSelection";
            this.btnHideSelection.Size = new System.Drawing.Size(103, 23);
            this.btnHideSelection.TabIndex = 7;
            this.btnHideSelection.Text = "Hide Selection";
            this.btnHideSelection.Click += new System.EventHandler(this.BtnHideSelection_Click);
            // 
            // ultraPanelGrid
            // 
            // 
            // ultraPanelGrid.ClientArea
            // 
            this.ultraPanelGrid.ClientArea.Controls.Add(this.ultraGridMaster);
            this.ultraPanelGrid.ClientArea.Controls.Add(this.gridFooterPanel);
            this.ultraPanelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelGrid.Location = new System.Drawing.Point(0, 136);
            this.ultraPanelGrid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ultraPanelGrid.Name = "ultraPanelGrid";
            this.ultraPanelGrid.Size = new System.Drawing.Size(1097, 449);
            this.ultraPanelGrid.TabIndex = 2;
            // 
            // ultraGridMaster
            // 
            this.ultraGridMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridMaster.Location = new System.Drawing.Point(0, 0);
            this.ultraGridMaster.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ultraGridMaster.Name = "ultraGridMaster";
            this.ultraGridMaster.Size = new System.Drawing.Size(1097, 428);
            this.ultraGridMaster.TabIndex = 0;
            this.ultraGridMaster.AfterCellUpdate += new Infragistics.Win.UltraWinGrid.CellEventHandler(this.UltraGridMaster_AfterCellUpdate);
            this.ultraGridMaster.InitializeLayout += new Infragistics.Win.UltraWinGrid.InitializeLayoutEventHandler(this.UltraGridMaster_InitializeLayout);
            this.ultraGridMaster.InitializeRow += new Infragistics.Win.UltraWinGrid.InitializeRowEventHandler(this.UltraGridMaster_InitializeRow);
            // 
            // gridFooterPanel
            // 
            // 
            // gridFooterPanel.ClientArea
            // 
            this.gridFooterPanel.ClientArea.Controls.Add(this.lblCount);
            this.gridFooterPanel.ClientArea.Controls.Add(this.lblExceptionCount);
            this.gridFooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gridFooterPanel.Location = new System.Drawing.Point(0, 428);
            this.gridFooterPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gridFooterPanel.Name = "gridFooterPanel";
            this.gridFooterPanel.Size = new System.Drawing.Size(1097, 21);
            this.gridFooterPanel.TabIndex = 1;
            // 
            // lblCount
            // 
            this.lblCount.Location = new System.Drawing.Point(10, 3);
            this.lblCount.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(103, 15);
            this.lblCount.TabIndex = 0;
            this.lblCount.Text = "Rows: 0";
            // 
            // lblExceptionCount
            // 
            this.lblExceptionCount.Location = new System.Drawing.Point(137, 3);
            this.lblExceptionCount.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblExceptionCount.Name = "lblExceptionCount";
            this.lblExceptionCount.Size = new System.Drawing.Size(129, 15);
            this.lblExceptionCount.TabIndex = 1;
            this.lblExceptionCount.Text = "Exceptions: 0";
            // 
            // FrmSmartReorderDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1097, 585);
            this.Controls.Add(this.ultraPanelGrid);
            this.Controls.Add(this.ultraPanelActionBar);
            this.Controls.Add(this.ultraPanelSelection);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FrmSmartReorderDashboard";
            this.Text = "Smart Reorder Dashboard";
            this.ultraPanelSelection.ClientArea.ResumeLayout(false);
            this.ultraPanelSelection.ClientArea.PerformLayout();
            this.ultraPanelSelection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmbItemNoMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFromBarcode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbAlert)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowOnlyExceptions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbMoreOptions)).EndInit();
            this.ultraPanelActionBar.ClientArea.ResumeLayout(false);
            this.ultraPanelActionBar.ResumeLayout(false);
            this.ultraPanelGrid.ClientArea.ResumeLayout(false);
            this.ultraPanelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridMaster)).EndInit();
            this.gridFooterPanel.ClientArea.ResumeLayout(false);
            this.gridFooterPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
