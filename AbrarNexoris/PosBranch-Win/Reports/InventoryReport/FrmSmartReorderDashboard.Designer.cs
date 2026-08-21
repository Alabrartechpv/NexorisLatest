using System;
using System.ComponentModel;
using System.Windows.Forms;
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

        // Custom action panel ultraPanel19 next to Barcode
        private UltraPanel ultraPanel19;
        private Label lblItemSearch;

        // Preset action button ultraPanel1 matching ultraPanel5 theme
        private UltraPanel ultraPanel1;
        private Label lblPreset;

        // Export Grid button ultraPanelExport matching IRS blue theme
        private UltraPanel ultraPanelExport;
        private Label lblExport;

        // Custom action panels matching frmReportFormatDialog's ultraPanel6 theme
        private UltraPanel ultraPanel2;
        private Label lblViewGrid;

        private UltraPanel ultraPanel3;
        private Label lblGeneratePO;

        private UltraPanel ultraPanel4;
        private Label lblGenBranchPO;

        private UltraPanel ultraPanel5;
        private Label lblRefreshStats;

        private UltraPanel ultraPanel6;
        private Label lblHideSelection;

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
            this.ultraPanel19 = new Infragistics.Win.Misc.UltraPanel();
            this.lblItemSearch = new System.Windows.Forms.Label();
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
            this.ultraPanel2 = new Infragistics.Win.Misc.UltraPanel();
            this.lblViewGrid = new System.Windows.Forms.Label();
            this.ultraPanel3 = new Infragistics.Win.Misc.UltraPanel();
            this.lblGeneratePO = new System.Windows.Forms.Label();
            this.ultraPanel4 = new Infragistics.Win.Misc.UltraPanel();
            this.lblGenBranchPO = new System.Windows.Forms.Label();
            this.ultraPanel5 = new Infragistics.Win.Misc.UltraPanel();
            this.lblRefreshStats = new System.Windows.Forms.Label();
            this.ultraPanel1 = new Infragistics.Win.Misc.UltraPanel();
            this.lblPreset = new System.Windows.Forms.Label();
            this.ultraPanelExport = new Infragistics.Win.Misc.UltraPanel();
            this.lblExport = new System.Windows.Forms.Label();
            this.lblCount = new Infragistics.Win.Misc.UltraLabel();
            this.lblExceptionCount = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanel6 = new Infragistics.Win.Misc.UltraPanel();
            this.lblHideSelection = new System.Windows.Forms.Label();
            this.ultraPanelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGridMaster = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.gridFooterPanel = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanelSelection.ClientArea.SuspendLayout();
            this.ultraPanelSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbItemNoMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFromBarcode)).BeginInit();
            this.ultraPanel19.ClientArea.SuspendLayout();
            this.ultraPanel19.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbAlert)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowOnlyExceptions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbMoreOptions)).BeginInit();
            this.ultraPanelActionBar.ClientArea.SuspendLayout();
            this.ultraPanelActionBar.SuspendLayout();
            this.ultraPanel2.ClientArea.SuspendLayout();
            this.ultraPanel2.SuspendLayout();
            this.ultraPanel3.ClientArea.SuspendLayout();
            this.ultraPanel3.SuspendLayout();
            this.ultraPanel4.ClientArea.SuspendLayout();
            this.ultraPanel4.SuspendLayout();
            this.ultraPanel5.ClientArea.SuspendLayout();
            this.ultraPanel5.SuspendLayout();
            this.ultraPanel1.ClientArea.SuspendLayout();
            this.ultraPanel1.SuspendLayout();
            this.ultraPanelExport.ClientArea.SuspendLayout();
            this.ultraPanelExport.SuspendLayout();
            this.ultraPanel6.ClientArea.SuspendLayout();
            this.ultraPanel6.SuspendLayout();
            this.ultraPanelGrid.ClientArea.SuspendLayout();
            this.ultraPanelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridMaster)).BeginInit();
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
            this.ultraPanelSelection.ClientArea.Controls.Add(this.ultraPanel19);
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
            this.lblItemNo.Location = new System.Drawing.Point(15, 15);
            this.lblItemNo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblItemNo.Name = "lblItemNo";
            this.lblItemNo.Size = new System.Drawing.Size(85, 19);
            this.lblItemNo.TabIndex = 0;
            this.lblItemNo.Text = "Item No.";
            // 
            // cmbItemNoMode
            // 
            this.cmbItemNoMode.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbItemNoMode.Location = new System.Drawing.Point(105, 12);
            this.cmbItemNoMode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbItemNoMode.Name = "cmbItemNoMode";
            this.cmbItemNoMode.Size = new System.Drawing.Size(145, 21);
            this.cmbItemNoMode.TabIndex = 1;
            // 
            // lblFromBarcode
            // 
            this.lblFromBarcode.AutoSize = true;
            this.lblFromBarcode.Location = new System.Drawing.Point(275, 15);
            this.lblFromBarcode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblFromBarcode.Name = "lblFromBarcode";
            this.lblFromBarcode.Size = new System.Drawing.Size(46, 14);
            this.lblFromBarcode.TabIndex = 2;
            this.lblFromBarcode.Text = "Barcode";
            // 
            // txtFromBarcode
            // 
            this.txtFromBarcode.Location = new System.Drawing.Point(355, 12);
            this.txtFromBarcode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtFromBarcode.Name = "txtFromBarcode";
            this.txtFromBarcode.Size = new System.Drawing.Size(155, 21);
            this.txtFromBarcode.TabIndex = 3;
            // 
            // ultraPanel19
            // 
            this.ultraPanel19.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            // 
            // ultraPanel19.ClientArea
            // 
            this.ultraPanel19.ClientArea.Controls.Add(this.lblItemSearch);
            this.ultraPanel19.Location = new System.Drawing.Point(515, 10);
            this.ultraPanel19.Name = "ultraPanel19";
            this.ultraPanel19.Size = new System.Drawing.Size(75, 24);
            this.ultraPanel19.TabIndex = 4;
            // 
            // lblItemSearch
            // 
            this.lblItemSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblItemSearch.Location = new System.Drawing.Point(0, 0);
            this.lblItemSearch.Name = "lblItemSearch";
            this.lblItemSearch.Size = new System.Drawing.Size(71, 20);
            this.lblItemSearch.TabIndex = 0;
            this.lblItemSearch.Text = "Search";
            this.lblItemSearch.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCategory
            // 
            this.lblCategory.Location = new System.Drawing.Point(15, 44);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(85, 19);
            this.lblCategory.TabIndex = 6;
            this.lblCategory.Text = "Category";
            // 
            // cmbCategory
            // 
            this.cmbCategory.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbCategory.Location = new System.Drawing.Point(105, 41);
            this.cmbCategory.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(145, 21);
            this.cmbCategory.TabIndex = 7;
            // 
            // lblGroup
            // 
            this.lblGroup.Location = new System.Drawing.Point(275, 44);
            this.lblGroup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(75, 19);
            this.lblGroup.TabIndex = 8;
            this.lblGroup.Text = "Group";
            // 
            // cmbGroup
            // 
            this.cmbGroup.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbGroup.Location = new System.Drawing.Point(355, 41);
            this.cmbGroup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbGroup.Name = "cmbGroup";
            this.cmbGroup.Size = new System.Drawing.Size(155, 21);
            this.cmbGroup.TabIndex = 9;
            this.cmbGroup.ValueChanged += new System.EventHandler(this.CmbGroup_ValueChanged);
            // 
            // lblAlert
            // 
            this.lblAlert.Location = new System.Drawing.Point(535, 44);
            this.lblAlert.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblAlert.Name = "lblAlert";
            this.lblAlert.Size = new System.Drawing.Size(50, 19);
            this.lblAlert.TabIndex = 10;
            this.lblAlert.Text = "Alert";
            // 
            // cmbAlert
            // 
            this.cmbAlert.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbAlert.Location = new System.Drawing.Point(590, 41);
            this.cmbAlert.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbAlert.Name = "cmbAlert";
            this.cmbAlert.Size = new System.Drawing.Size(145, 21);
            this.cmbAlert.TabIndex = 11;
            this.cmbAlert.ValueChanged += new System.EventHandler(this.CmbAlert_ValueChanged);
            // 
            // chkShowOnlyExceptions
            // 
            this.chkShowOnlyExceptions.Location = new System.Drawing.Point(760, 43);
            this.chkShowOnlyExceptions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chkShowOnlyExceptions.Name = "chkShowOnlyExceptions";
            this.chkShowOnlyExceptions.Size = new System.Drawing.Size(170, 19);
            this.chkShowOnlyExceptions.TabIndex = 12;
            this.chkShowOnlyExceptions.Text = "Show Only Exceptions";
            this.chkShowOnlyExceptions.CheckedChanged += new System.EventHandler(this.ChkShowOnlyExceptions_CheckedChanged);
            // 
            // lblMoreOptions
            // 
            this.lblMoreOptions.Location = new System.Drawing.Point(15, 73);
            this.lblMoreOptions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblMoreOptions.Name = "lblMoreOptions";
            this.lblMoreOptions.Size = new System.Drawing.Size(85, 19);
            this.lblMoreOptions.TabIndex = 13;
            this.lblMoreOptions.Text = "More Options";
            // 
            // cmbMoreOptions
            // 
            this.cmbMoreOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbMoreOptions.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbMoreOptions.Location = new System.Drawing.Point(105, 71);
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
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.ultraPanel2);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.ultraPanel3);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.ultraPanel4);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.ultraPanel5);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.ultraPanel1);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.ultraPanelExport);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.lblCount);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.lblExceptionCount);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.ultraPanel6);
            this.ultraPanelActionBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelActionBar.Location = new System.Drawing.Point(0, 102);
            this.ultraPanelActionBar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ultraPanelActionBar.Name = "ultraPanelActionBar";
            this.ultraPanelActionBar.Size = new System.Drawing.Size(1097, 36);
            this.ultraPanelActionBar.TabIndex = 1;
            // 
            // ultraPanel2
            // 
            this.ultraPanel2.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            // 
            // ultraPanel2.ClientArea
            // 
            this.ultraPanel2.ClientArea.Controls.Add(this.lblViewGrid);
            this.ultraPanel2.Location = new System.Drawing.Point(12, 4);
            this.ultraPanel2.Name = "ultraPanel2";
            this.ultraPanel2.Size = new System.Drawing.Size(95, 27);
            this.ultraPanel2.TabIndex = 0;
            // 
            // lblViewGrid
            // 
            this.lblViewGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblViewGrid.Location = new System.Drawing.Point(0, 0);
            this.lblViewGrid.Name = "lblViewGrid";
            this.lblViewGrid.Size = new System.Drawing.Size(91, 23);
            this.lblViewGrid.TabIndex = 0;
            this.lblViewGrid.Text = "View Grid";
            this.lblViewGrid.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ultraPanel3
            // 
            this.ultraPanel3.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            // 
            // ultraPanel3.ClientArea
            // 
            this.ultraPanel3.ClientArea.Controls.Add(this.lblGeneratePO);
            this.ultraPanel3.Location = new System.Drawing.Point(113, 4);
            this.ultraPanel3.Name = "ultraPanel3";
            this.ultraPanel3.Size = new System.Drawing.Size(105, 27);
            this.ultraPanel3.TabIndex = 1;
            // 
            // lblGeneratePO
            // 
            this.lblGeneratePO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGeneratePO.Location = new System.Drawing.Point(0, 0);
            this.lblGeneratePO.Name = "lblGeneratePO";
            this.lblGeneratePO.Size = new System.Drawing.Size(101, 23);
            this.lblGeneratePO.TabIndex = 0;
            this.lblGeneratePO.Text = "Generate PO";
            this.lblGeneratePO.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ultraPanel4
            // 
            this.ultraPanel4.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            // 
            // ultraPanel4.ClientArea
            // 
            this.ultraPanel4.ClientArea.Controls.Add(this.lblGenBranchPO);
            this.ultraPanel4.Location = new System.Drawing.Point(224, 4);
            this.ultraPanel4.Name = "ultraPanel4";
            this.ultraPanel4.Size = new System.Drawing.Size(115, 27);
            this.ultraPanel4.TabIndex = 2;
            // 
            // lblGenBranchPO
            // 
            this.lblGenBranchPO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGenBranchPO.Location = new System.Drawing.Point(0, 0);
            this.lblGenBranchPO.Name = "lblGenBranchPO";
            this.lblGenBranchPO.Size = new System.Drawing.Size(111, 23);
            this.lblGenBranchPO.TabIndex = 0;
            this.lblGenBranchPO.Text = "Gen. Branch PO";
            this.lblGenBranchPO.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ultraPanel5
            // 
            this.ultraPanel5.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            // 
            // ultraPanel5.ClientArea
            // 
            this.ultraPanel5.ClientArea.Controls.Add(this.lblRefreshStats);
            this.ultraPanel5.Location = new System.Drawing.Point(345, 4);
            this.ultraPanel5.Name = "ultraPanel5";
            this.ultraPanel5.Size = new System.Drawing.Size(105, 27);
            this.ultraPanel5.TabIndex = 3;
            // 
            // lblRefreshStats
            // 
            this.lblRefreshStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRefreshStats.Location = new System.Drawing.Point(0, 0);
            this.lblRefreshStats.Name = "lblRefreshStats";
            this.lblRefreshStats.Size = new System.Drawing.Size(101, 23);
            this.lblRefreshStats.TabIndex = 0;
            this.lblRefreshStats.Text = "Refresh Stats";
            this.lblRefreshStats.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ultraPanel1
            // 
            this.ultraPanel1.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            // 
            // ultraPanel1.ClientArea
            // 
            this.ultraPanel1.ClientArea.Controls.Add(this.lblPreset);
            this.ultraPanel1.Location = new System.Drawing.Point(456, 4);
            this.ultraPanel1.Name = "ultraPanel1";
            this.ultraPanel1.Size = new System.Drawing.Size(85, 27);
            this.ultraPanel1.TabIndex = 4;
            // 
            // lblPreset
            // 
            this.lblPreset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPreset.Location = new System.Drawing.Point(0, 0);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(81, 23);
            this.lblPreset.TabIndex = 0;
            this.lblPreset.Text = "Preset";
            this.lblPreset.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ultraPanelExport
            // 
            this.ultraPanelExport.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            // 
            // ultraPanelExport.ClientArea
            // 
            this.ultraPanelExport.ClientArea.Controls.Add(this.lblExport);
            this.ultraPanelExport.Location = new System.Drawing.Point(547, 4);
            this.ultraPanelExport.Name = "ultraPanelExport";
            this.ultraPanelExport.Size = new System.Drawing.Size(95, 27);
            this.ultraPanelExport.TabIndex = 5;
            // 
            // lblExport
            // 
            this.lblExport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblExport.Location = new System.Drawing.Point(0, 0);
            this.lblExport.Name = "lblExport";
            this.lblExport.Size = new System.Drawing.Size(91, 23);
            this.lblExport.TabIndex = 0;
            this.lblExport.Text = "Export Grid";
            this.lblExport.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCount
            // 
            this.lblCount.Location = new System.Drawing.Point(650, 9);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(95, 20);
            this.lblCount.TabIndex = 6;
            this.lblCount.Text = "Rows: 0";
            // 
            // lblExceptionCount
            // 
            this.lblExceptionCount.Location = new System.Drawing.Point(655, 9);
            this.lblExceptionCount.Name = "lblExceptionCount";
            this.lblExceptionCount.Size = new System.Drawing.Size(130, 20);
            this.lblExceptionCount.TabIndex = 6;
            this.lblExceptionCount.Text = "Exceptions: 0";
            // 
            // ultraPanel6
            // 
            this.ultraPanel6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraPanel6.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            // 
            // ultraPanel6.ClientArea
            // 
            this.ultraPanel6.ClientArea.Controls.Add(this.lblHideSelection);
            this.ultraPanel6.Location = new System.Drawing.Point(975, 4);
            this.ultraPanel6.Name = "ultraPanel6";
            this.ultraPanel6.Size = new System.Drawing.Size(110, 27);
            this.ultraPanel6.TabIndex = 7;
            // 
            // lblHideSelection
            // 
            this.lblHideSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHideSelection.Location = new System.Drawing.Point(0, 0);
            this.lblHideSelection.Name = "lblHideSelection";
            this.lblHideSelection.Size = new System.Drawing.Size(106, 23);
            this.lblHideSelection.TabIndex = 0;
            this.lblHideSelection.Text = "Hide Selection";
            this.lblHideSelection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ultraPanelGrid
            // 
            // 
            // ultraPanelGrid.ClientArea
            // 
            this.ultraPanelGrid.ClientArea.Controls.Add(this.ultraGridMaster);
            this.ultraPanelGrid.ClientArea.Controls.Add(this.gridFooterPanel);
            this.ultraPanelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelGrid.Location = new System.Drawing.Point(0, 138);
            this.ultraPanelGrid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ultraPanelGrid.Name = "ultraPanelGrid";
            this.ultraPanelGrid.Size = new System.Drawing.Size(1097, 447);
            this.ultraPanelGrid.TabIndex = 2;
            // 
            // ultraGridMaster
            // 
            this.ultraGridMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridMaster.Location = new System.Drawing.Point(0, 0);
            this.ultraGridMaster.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ultraGridMaster.Name = "ultraGridMaster";
            this.ultraGridMaster.Size = new System.Drawing.Size(1097, 422);
            this.ultraGridMaster.TabIndex = 0;
            this.ultraGridMaster.AfterCellUpdate += new Infragistics.Win.UltraWinGrid.CellEventHandler(this.UltraGridMaster_AfterCellUpdate);
            this.ultraGridMaster.InitializeLayout += new Infragistics.Win.UltraWinGrid.InitializeLayoutEventHandler(this.UltraGridMaster_InitializeLayout);
            this.ultraGridMaster.InitializeRow += new Infragistics.Win.UltraWinGrid.InitializeRowEventHandler(this.UltraGridMaster_InitializeRow);
            // 
            // gridFooterPanel
            // 
            this.gridFooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gridFooterPanel.Location = new System.Drawing.Point(0, 422);
            this.gridFooterPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gridFooterPanel.Name = "gridFooterPanel";
            this.gridFooterPanel.Size = new System.Drawing.Size(1097, 25);
            this.gridFooterPanel.TabIndex = 1;
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
            this.ultraPanel19.ClientArea.ResumeLayout(false);
            this.ultraPanel19.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbAlert)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowOnlyExceptions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbMoreOptions)).EndInit();
            this.ultraPanelActionBar.ClientArea.ResumeLayout(false);
            this.ultraPanelActionBar.ResumeLayout(false);
            this.ultraPanel2.ClientArea.ResumeLayout(false);
            this.ultraPanel2.ResumeLayout(false);
            this.ultraPanel3.ClientArea.ResumeLayout(false);
            this.ultraPanel3.ResumeLayout(false);
            this.ultraPanel4.ClientArea.ResumeLayout(false);
            this.ultraPanel4.ResumeLayout(false);
            this.ultraPanel5.ClientArea.ResumeLayout(false);
            this.ultraPanel5.ResumeLayout(false);
            this.ultraPanel1.ClientArea.ResumeLayout(false);
            this.ultraPanel1.ResumeLayout(false);
            this.ultraPanel6.ClientArea.ResumeLayout(false);
            this.ultraPanel6.ResumeLayout(false);
            this.ultraPanelGrid.ClientArea.ResumeLayout(false);
            this.ultraPanelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridMaster)).EndInit();
            this.gridFooterPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
