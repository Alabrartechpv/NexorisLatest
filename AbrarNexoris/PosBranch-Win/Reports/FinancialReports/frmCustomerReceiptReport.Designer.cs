namespace PosBranch_Win.Reports.FinancialReports
{
    partial class frmCustomerReceiptReport
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
            this.comboBox1 = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.button1 = new System.Windows.Forms.Button();
            this.txtSearch = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblSearch = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboPreset = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblPreset = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboCustomer = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblCustomer = new Infragistics.Win.Misc.UltraLabel();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblToDate = new Infragistics.Win.Misc.UltraLabel();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblFromDate = new Infragistics.Win.Misc.UltraLabel();
            this.btnClearFilters = new Infragistics.Win.Misc.UltraButton();
            this.btnSearch = new Infragistics.Win.Misc.UltraButton();
            this.btnExport = new Infragistics.Win.Misc.UltraButton();
            this.ultraPanelMaster = new Infragistics.Win.Misc.UltraPanel();
            this.ultraButton3 = new Infragistics.Win.Misc.UltraButton();
            this.ultraButton2 = new Infragistics.Win.Misc.UltraButton();
            this.ultraButton1 = new Infragistics.Win.Misc.UltraButton();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelControls.ClientArea.SuspendLayout();
            this.ultraPanelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPreset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboCustomer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
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
            this.ultraPanelControls.ClientArea.Controls.Add(this.comboBox1);
            this.ultraPanelControls.ClientArea.Controls.Add(this.button1);
            this.ultraPanelControls.ClientArea.Controls.Add(this.txtSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblPreset);
            this.ultraPanelControls.ClientArea.Controls.Add(this.ultraComboCustomer);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblCustomer);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtTo);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblToDate);
            this.ultraPanelControls.ClientArea.Controls.Add(this.dtFrom);
            this.ultraPanelControls.ClientArea.Controls.Add(this.lblFromDate);
            this.ultraPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelControls.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelControls.Name = "ultraPanelControls";
            this.ultraPanelControls.Size = new System.Drawing.Size(1349, 127);
            this.ultraPanelControls.TabIndex = 0;
            // 
            // comboBox1
            // 
            this.comboBox1.Location = new System.Drawing.Point(116, 12);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(164, 25);
            this.comboBox1.TabIndex = 18;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(424, 46);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(42, 26);
            this.button1.TabIndex = 17;
            this.button1.Text = "F11";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(116, 42);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(164, 25);
            this.txtSearch.TabIndex = 7;
            // 
            // lblSearch
            // 
            this.lblSearch.Location = new System.Drawing.Point(60, 46);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(72, 23);
            this.lblSearch.TabIndex = 6;
            this.lblSearch.Text = "Customer";
            // 
            // ultraComboPreset
            // 
            this.ultraComboPreset.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraComboPreset.Location = new System.Drawing.Point(116, 74);
            this.ultraComboPreset.Name = "ultraComboPreset";
            this.ultraComboPreset.Size = new System.Drawing.Size(164, 25);
            this.ultraComboPreset.TabIndex = 9;
            // 
            // lblPreset
            // 
            this.lblPreset.Location = new System.Drawing.Point(66, 78);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(66, 23);
            this.lblPreset.TabIndex = 8;
            this.lblPreset.Text = "Options";
            // 
            // ultraComboCustomer
            // 
            this.ultraComboCustomer.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
            this.ultraComboCustomer.Location = new System.Drawing.Point(472, 46);
            this.ultraComboCustomer.Name = "ultraComboCustomer";
            this.ultraComboCustomer.Size = new System.Drawing.Size(406, 25);
            this.ultraComboCustomer.TabIndex = 5;
            // 
            // lblCustomer
            // 
            this.lblCustomer.Location = new System.Drawing.Point(83, 15);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Size = new System.Drawing.Size(46, 23);
            this.lblCustomer.TabIndex = 4;
            this.lblCustomer.Text = "Date";
            // 
            // dtTo
            // 
            this.dtTo.DateTime = new System.DateTime(2026, 4, 16, 0, 0, 0, 0);
            this.dtTo.Location = new System.Drawing.Point(744, 12);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(134, 25);
            this.dtTo.TabIndex = 3;
            this.dtTo.Value = new System.DateTime(2026, 4, 16, 0, 0, 0, 0);
            // 
            // lblToDate
            // 
            this.lblToDate.Location = new System.Drawing.Point(680, 14);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(57, 23);
            this.lblToDate.TabIndex = 2;
            this.lblToDate.Text = "To Date";
            // 
            // dtFrom
            // 
            this.dtFrom.DateTime = new System.DateTime(2026, 4, 16, 0, 0, 0, 0);
            this.dtFrom.Location = new System.Drawing.Point(472, 12);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(134, 25);
            this.dtFrom.TabIndex = 1;
            this.dtFrom.Value = new System.DateTime(2026, 4, 16, 0, 0, 0, 0);
            // 
            // lblFromDate
            // 
            this.lblFromDate.Location = new System.Drawing.Point(391, 14);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(74, 23);
            this.lblFromDate.TabIndex = 0;
            this.lblFromDate.Text = "From Date";
            // 
            // btnClearFilters
            // 
            this.btnClearFilters.Location = new System.Drawing.Point(123, 8);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(102, 28);
            this.btnClearFilters.TabIndex = 13;
            this.btnClearFilters.Text = "Clear";
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(12, 8);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(103, 28);
            this.btnSearch.TabIndex = 12;
            this.btnSearch.Text = "View Grid";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(234, 8);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(108, 28);
            this.btnExport.TabIndex = 14;
            this.btnExport.Text = "Export Grid";
            // 
            // ultraPanelMaster
            // 
            // 
            // ultraPanelMaster.ClientArea
            // 
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraButton3);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraButton2);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraButton1);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.ultraPanelGridFooter);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.gridReport);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.btnClearFilters);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.btnSearch);
            this.ultraPanelMaster.ClientArea.Controls.Add(this.btnExport);
            this.ultraPanelMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMaster.Location = new System.Drawing.Point(0, 127);
            this.ultraPanelMaster.Name = "ultraPanelMaster";
            this.ultraPanelMaster.Size = new System.Drawing.Size(1349, 434);
            this.ultraPanelMaster.TabIndex = 1;
            // 
            // ultraButton3
            // 
            this.ultraButton3.Location = new System.Drawing.Point(362, 8);
            this.ultraButton3.Name = "ultraButton3";
            this.ultraButton3.Size = new System.Drawing.Size(103, 28);
            this.ultraButton3.TabIndex = 17;
            this.ultraButton3.Text = "Preview Grid ";
            // 
            // ultraButton2
            // 
            this.ultraButton2.Location = new System.Drawing.Point(485, 8);
            this.ultraButton2.Name = "ultraButton2";
            this.ultraButton2.Size = new System.Drawing.Size(103, 28);
            this.ultraButton2.TabIndex = 16;
            this.ultraButton2.Text = "Preview Report";
            // 
            // ultraButton1
            // 
            this.ultraButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraButton1.Location = new System.Drawing.Point(1212, 8);
            this.ultraButton1.Name = "ultraButton1";
            this.ultraButton1.Size = new System.Drawing.Size(120, 28);
            this.ultraButton1.TabIndex = 15;
            this.ultraButton1.Text = "Hide Selection";
            // 
            // ultraPanelGridFooter
            // 
            this.ultraPanelGridFooter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraPanelGridFooter.Location = new System.Drawing.Point(3, 314);
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            this.ultraPanelGridFooter.Size = new System.Drawing.Size(1343, 26);
            this.ultraPanelGridFooter.TabIndex = 18;
            // 
            // gridReport
            // 
            this.gridReport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridReport.Location = new System.Drawing.Point(3, 42);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(1343, 298);
            this.gridReport.TabIndex = 0;
            this.gridReport.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // frmCustomerReceiptReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(1349, 561);
            this.Controls.Add(this.ultraPanelMaster);
            this.Controls.Add(this.ultraPanelControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1024, 600);
            this.Name = "frmCustomerReceiptReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Customer Receipt Report";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ultraPanelControls.ClientArea.ResumeLayout(false);
            this.ultraPanelControls.ClientArea.PerformLayout();
            this.ultraPanelControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.comboBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPreset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboCustomer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            this.ultraPanelMaster.ClientArea.ResumeLayout(false);
            this.ultraPanelMaster.ResumeLayout(false);
            this.ultraPanelGridFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelControls;
        private Infragistics.Win.Misc.UltraButton btnClearFilters;
        private Infragistics.Win.Misc.UltraButton btnSearch;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor txtSearch;
        private Infragistics.Win.Misc.UltraLabel lblSearch;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboPreset;
        private Infragistics.Win.Misc.UltraLabel lblPreset;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboCustomer;
        private Infragistics.Win.Misc.UltraLabel lblCustomer;
        private Infragistics.Win.Misc.UltraButton btnExport;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private Infragistics.Win.Misc.UltraLabel lblToDate;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private Infragistics.Win.Misc.UltraLabel lblFromDate;
        private Infragistics.Win.Misc.UltraPanel ultraPanelMaster;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;
        private System.Windows.Forms.Button button1;
        private Infragistics.Win.Misc.UltraButton ultraButton1;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboBox1;
        private Infragistics.Win.Misc.UltraButton ultraButton3;
        private Infragistics.Win.Misc.UltraButton ultraButton2;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
    }
}
