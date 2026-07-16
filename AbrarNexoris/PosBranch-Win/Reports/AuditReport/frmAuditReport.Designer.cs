using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.AuditReport
{
    partial class frmAuditReport
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
            this.lblItemNo = new Infragistics.Win.Misc.UltraLabel();
            this.cmbItemNo = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblDate = new Infragistics.Win.Misc.UltraLabel();
            this.cmbDatePreset = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFromDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtToDate = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblCategory = new Infragistics.Win.Misc.UltraLabel();
            this.cmbCategory = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblGroup = new Infragistics.Win.Misc.UltraLabel();
            this.cmbGroup = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblLocation = new Infragistics.Win.Misc.UltraLabel();
            this.cmbLocation = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblBrand = new Infragistics.Win.Misc.UltraLabel();
            this.cmbBrand = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblModel = new Infragistics.Win.Misc.UltraLabel();
            this.cmbModel = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblMoreOptions = new Infragistics.Win.Misc.UltraLabel();
            this.cmbAction = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.ultraPanelActionBar = new Infragistics.Win.Misc.UltraPanel();
            this.btnViewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewGrid = new Infragistics.Win.Misc.UltraButton();
            this.btnPreviewReport = new Infragistics.Win.Misc.UltraButton();
            this.btnHideSelection = new Infragistics.Win.Misc.UltraButton();
            this.ultraPanelGrid = new Infragistics.Win.Misc.UltraPanel();
            this.gridAudit = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.gridFooterPanel = new Infragistics.Win.Misc.UltraPanel();
            this.lblCount = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelSelection.ClientArea.SuspendLayout();
            this.ultraPanelSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbItemNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDatePreset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbLocation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbBrand)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbModel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbAction)).BeginInit();
            this.ultraPanelActionBar.ClientArea.SuspendLayout();
            this.ultraPanelActionBar.SuspendLayout();
            this.ultraPanelGrid.ClientArea.SuspendLayout();
            this.ultraPanelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAudit)).BeginInit();
            this.gridFooterPanel.ClientArea.SuspendLayout();
            this.gridFooterPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ultraPanelSelection
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(235)))), ((int)(((byte)(247)))));
            appearance1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(188)))), ((int)(((byte)(214)))));
            this.ultraPanelSelection.Appearance = appearance1;
            this.ultraPanelSelection.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelSelection.ClientArea
            // 
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblItemNo);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbItemNo);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbDatePreset);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.dtFromDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.dtToDate);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblCategory);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbCategory);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblGroup);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbGroup);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblLocation);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbLocation);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblBrand);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbBrand);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblModel);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbModel);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.lblMoreOptions);
            this.ultraPanelSelection.ClientArea.Controls.Add(this.cmbAction);
            this.ultraPanelSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelSelection.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelSelection.Name = "ultraPanelSelection";
            this.ultraPanelSelection.Size = new System.Drawing.Size(1280, 156);
            this.ultraPanelSelection.TabIndex = 1;
            // 
            // lblItemNo
            // 
            this.lblItemNo.Location = new System.Drawing.Point(44, 18);
            this.lblItemNo.Name = "lblItemNo";
            this.lblItemNo.Size = new System.Drawing.Size(76, 23);
            this.lblItemNo.TabIndex = 0;
            this.lblItemNo.Text = "Item No";
            // 
            // cmbItemNo
            // 
            this.cmbItemNo.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
            this.cmbItemNo.Location = new System.Drawing.Point(130, 15);
            this.cmbItemNo.Name = "cmbItemNo";
            this.cmbItemNo.Size = new System.Drawing.Size(185, 25);
            this.cmbItemNo.TabIndex = 1;
            this.cmbItemNo.UseAppStyling = false;
            this.cmbItemNo.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.cmbItemNo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CmbItemNo_KeyDown);
            // 
            // lblDate
            // 
            this.lblDate.Location = new System.Drawing.Point(68, 54);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(52, 23);
            this.lblDate.TabIndex = 2;
            this.lblDate.Text = "Date";
            // 
            // cmbDatePreset
            // 
            this.cmbDatePreset.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbDatePreset.Location = new System.Drawing.Point(130, 51);
            this.cmbDatePreset.Name = "cmbDatePreset";
            this.cmbDatePreset.Size = new System.Drawing.Size(185, 25);
            this.cmbDatePreset.TabIndex = 3;
            this.cmbDatePreset.UseAppStyling = false;
            this.cmbDatePreset.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.cmbDatePreset.ValueChanged += new System.EventHandler(this.CmbDatePreset_ValueChanged);
            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(420, 18);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(72, 23);
            this.lblFromDate.TabIndex = 4;
            this.lblFromDate.Text = "From Date";
            // 
            // dtFromDate
            // 
            this.dtFromDate.FormatString = "dd/MM/yyyy";
            this.dtFromDate.Location = new System.Drawing.Point(495, 15);
            this.dtFromDate.MaskInput = "{date}";
            this.dtFromDate.Name = "dtFromDate";
            this.dtFromDate.Size = new System.Drawing.Size(150, 25);
            this.dtFromDate.TabIndex = 5;
            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(729, 18);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(58, 23);
            this.lblToDate.TabIndex = 6;
            this.lblToDate.Text = "To Date";
            // 
            // dtToDate
            // 
            this.dtToDate.FormatString = "dd/MM/yyyy";
            this.dtToDate.Location = new System.Drawing.Point(789, 15);
            this.dtToDate.MaskInput = "{date}";
            this.dtToDate.Name = "dtToDate";
            this.dtToDate.Size = new System.Drawing.Size(150, 25);
            this.dtToDate.TabIndex = 7;
            // 
            // lblCategory
            // 
            this.lblCategory.Location = new System.Drawing.Point(422, 54);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(70, 23);
            this.lblCategory.TabIndex = 8;
            this.lblCategory.Text = "Category";
            // 
            // cmbCategory
            // 
            this.cmbCategory.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbCategory.Location = new System.Drawing.Point(495, 51);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(205, 25);
            this.cmbCategory.TabIndex = 9;
            this.cmbCategory.UseAppStyling = false;
            this.cmbCategory.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // lblGroup
            // 
            this.lblGroup.Location = new System.Drawing.Point(736, 54);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(47, 23);
            this.lblGroup.TabIndex = 10;
            this.lblGroup.Text = "Group";
            // 
            // cmbGroup
            // 
            this.cmbGroup.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbGroup.Location = new System.Drawing.Point(788, 51);
            this.cmbGroup.Name = "cmbGroup";
            this.cmbGroup.Size = new System.Drawing.Size(205, 25);
            this.cmbGroup.TabIndex = 11;
            this.cmbGroup.UseAppStyling = false;
            this.cmbGroup.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.cmbGroup.ValueChanged += new System.EventHandler(this.CmbGroup_ValueChanged);
            // 
            // lblLocation
            // 
            this.lblLocation.Location = new System.Drawing.Point(58, 90);
            this.lblLocation.Name = "lblLocation";
            this.lblLocation.Size = new System.Drawing.Size(62, 23);
            this.lblLocation.TabIndex = 12;
            this.lblLocation.Text = "Location";
            // 
            // cmbLocation
            // 
            this.cmbLocation.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbLocation.Location = new System.Drawing.Point(130, 87);
            this.cmbLocation.Name = "cmbLocation";
            this.cmbLocation.Size = new System.Drawing.Size(185, 25);
            this.cmbLocation.TabIndex = 13;
            this.cmbLocation.UseAppStyling = false;
            this.cmbLocation.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // lblBrand
            // 
            this.lblBrand.Location = new System.Drawing.Point(447, 90);
            this.lblBrand.Name = "lblBrand";
            this.lblBrand.Size = new System.Drawing.Size(45, 23);
            this.lblBrand.TabIndex = 14;
            this.lblBrand.Text = "Brand";
            // 
            // cmbBrand
            // 
            this.cmbBrand.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbBrand.Location = new System.Drawing.Point(495, 87);
            this.cmbBrand.Name = "cmbBrand";
            this.cmbBrand.Size = new System.Drawing.Size(205, 25);
            this.cmbBrand.TabIndex = 15;
            this.cmbBrand.UseAppStyling = false;
            this.cmbBrand.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // lblModel
            // 
            this.lblModel.Location = new System.Drawing.Point(741, 90);
            this.lblModel.Name = "lblModel";
            this.lblModel.Size = new System.Drawing.Size(42, 23);
            this.lblModel.TabIndex = 16;
            this.lblModel.Text = "Model";
            // 
            // cmbModel
            // 
            this.cmbModel.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbModel.Location = new System.Drawing.Point(788, 87);
            this.cmbModel.Name = "cmbModel";
            this.cmbModel.Size = new System.Drawing.Size(205, 25);
            this.cmbModel.TabIndex = 17;
            this.cmbModel.UseAppStyling = false;
            this.cmbModel.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // lblMoreOptions
            // 
            this.lblMoreOptions.Location = new System.Drawing.Point(34, 126);
            this.lblMoreOptions.Name = "lblMoreOptions";
            this.lblMoreOptions.Size = new System.Drawing.Size(86, 23);
            this.lblMoreOptions.TabIndex = 18;
            this.lblMoreOptions.Text = "More Options";
            // 
            // cmbAction
            // 
            this.cmbAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbAction.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbAction.Location = new System.Drawing.Point(130, 123);
            this.cmbAction.Name = "cmbAction";
            this.cmbAction.Size = new System.Drawing.Size(1126, 25);
            this.cmbAction.TabIndex = 19;
            this.cmbAction.UseAppStyling = false;
            this.cmbAction.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraPanelActionBar
            // 
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(230)))), ((int)(((byte)(246)))));
            appearance2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(188)))), ((int)(((byte)(214)))));
            this.ultraPanelActionBar.Appearance = appearance2;
            this.ultraPanelActionBar.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelActionBar.ClientArea
            // 
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnViewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewGrid);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnPreviewReport);
            this.ultraPanelActionBar.ClientArea.Controls.Add(this.btnHideSelection);
            this.ultraPanelActionBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelActionBar.Location = new System.Drawing.Point(0, 156);
            this.ultraPanelActionBar.Name = "ultraPanelActionBar";
            this.ultraPanelActionBar.Size = new System.Drawing.Size(1280, 42);
            this.ultraPanelActionBar.TabIndex = 2;
            // 
            // btnViewGrid
            // 
            this.btnViewGrid.Location = new System.Drawing.Point(16, 7);
            this.btnViewGrid.Name = "btnViewGrid";
            this.btnViewGrid.Size = new System.Drawing.Size(110, 28);
            this.btnViewGrid.TabIndex = 0;
            this.btnViewGrid.Text = "View Grid";
            this.btnViewGrid.Click += new System.EventHandler(this.BtnViewGrid_Click);
            // 
            // btnPreviewGrid
            // 
            this.btnPreviewGrid.Location = new System.Drawing.Point(132, 7);
            this.btnPreviewGrid.Name = "btnPreviewGrid";
            this.btnPreviewGrid.Size = new System.Drawing.Size(120, 28);
            this.btnPreviewGrid.TabIndex = 1;
            this.btnPreviewGrid.Text = "Preview Grid";
            this.btnPreviewGrid.Click += new System.EventHandler(this.BtnPreviewGrid_Click);
            // 
            // btnPreviewReport
            // 
            this.btnPreviewReport.Location = new System.Drawing.Point(258, 7);
            this.btnPreviewReport.Name = "btnPreviewReport";
            this.btnPreviewReport.Size = new System.Drawing.Size(126, 28);
            this.btnPreviewReport.TabIndex = 2;
            this.btnPreviewReport.Text = "Preview Report";
            this.btnPreviewReport.Click += new System.EventHandler(this.BtnPreviewReport_Click);
            // 
            // btnHideSelection
            // 
            this.btnHideSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHideSelection.Location = new System.Drawing.Point(1144, 7);
            this.btnHideSelection.Name = "btnHideSelection";
            this.btnHideSelection.Size = new System.Drawing.Size(120, 28);
            this.btnHideSelection.TabIndex = 3;
            this.btnHideSelection.Text = "Hide Selection";
            this.btnHideSelection.Click += new System.EventHandler(this.BtnHideSelection_Click);
            // 
            // ultraPanelGrid
            // 
            // 
            // ultraPanelGrid.ClientArea
            // 
            this.ultraPanelGrid.ClientArea.Controls.Add(this.gridAudit);
            this.ultraPanelGrid.ClientArea.Controls.Add(this.gridFooterPanel);
            this.ultraPanelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelGrid.Location = new System.Drawing.Point(0, 198);
            this.ultraPanelGrid.Name = "ultraPanelGrid";
            this.ultraPanelGrid.Size = new System.Drawing.Size(1280, 522);
            this.ultraPanelGrid.TabIndex = 3;
            // 
            // gridAudit
            // 
            this.gridAudit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridAudit.Location = new System.Drawing.Point(0, 0);
            this.gridAudit.Name = "gridAudit";
            this.gridAudit.Size = new System.Drawing.Size(1280, 496);
            this.gridAudit.TabIndex = 0;
            this.gridAudit.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // gridFooterPanel
            // 
            appearance3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            appearance3.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(184)))));
            appearance3.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.gridFooterPanel.Appearance = appearance3;
            // 
            // gridFooterPanel.ClientArea
            // 
            this.gridFooterPanel.ClientArea.Controls.Add(this.lblCount);
            this.gridFooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gridFooterPanel.Location = new System.Drawing.Point(0, 496);
            this.gridFooterPanel.Name = "gridFooterPanel";
            this.gridFooterPanel.Size = new System.Drawing.Size(1280, 26);
            this.gridFooterPanel.TabIndex = 1;
            // 
            // lblCount
            // 
            appearance4.FontData.BoldAsString = "True";
            appearance4.ForeColor = System.Drawing.Color.White;
            this.lblCount.Appearance = appearance4;
            this.lblCount.Location = new System.Drawing.Point(12, 4);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(160, 18);
            this.lblCount.TabIndex = 0;
            this.lblCount.Text = "Rows: 0";
            // 
            // frmAuditReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(249)))), ((int)(((byte)(254)))));
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.ultraPanelGrid);
            this.Controls.Add(this.ultraPanelActionBar);
            this.Controls.Add(this.ultraPanelSelection);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.Name = "frmAuditReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Inventory Audit Trail";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ultraPanelSelection.ClientArea.ResumeLayout(false);
            this.ultraPanelSelection.ClientArea.PerformLayout();
            this.ultraPanelSelection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmbItemNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbDatePreset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtToDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCategory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbLocation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbBrand)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbModel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbAction)).EndInit();
            this.ultraPanelActionBar.ClientArea.ResumeLayout(false);
            this.ultraPanelActionBar.ResumeLayout(false);
            this.ultraPanelGrid.ClientArea.ResumeLayout(false);
            this.ultraPanelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridAudit)).EndInit();
            this.gridFooterPanel.ClientArea.ResumeLayout(false);
            this.gridFooterPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private UltraPanel ultraPanelSelection;
        private UltraLabel lblItemNo;
        private UltraComboEditor cmbItemNo;
        private UltraLabel lblDate;
        private UltraComboEditor cmbDatePreset;
        private UltraLabel lblFromDate;
        private UltraDateTimeEditor dtFromDate;
        private UltraLabel lblToDate;
        private UltraDateTimeEditor dtToDate;
        private UltraLabel lblCategory;
        private UltraComboEditor cmbCategory;
        private UltraLabel lblGroup;
        private UltraComboEditor cmbGroup;
        private UltraLabel lblLocation;
        private UltraComboEditor cmbLocation;
        private UltraLabel lblBrand;
        private UltraComboEditor cmbBrand;
        private UltraLabel lblModel;
        private UltraComboEditor cmbModel;
        private UltraLabel lblMoreOptions;
        private UltraComboEditor cmbAction;
        private UltraPanel ultraPanelActionBar;
        private UltraButton btnViewGrid;
        private UltraButton btnPreviewGrid;
        private UltraButton btnPreviewReport;
        private UltraButton btnHideSelection;
        private UltraPanel ultraPanelGrid;
        private UltraGrid gridAudit;
        private UltraPanel gridFooterPanel;
        private UltraLabel lblCount;
    }
}
