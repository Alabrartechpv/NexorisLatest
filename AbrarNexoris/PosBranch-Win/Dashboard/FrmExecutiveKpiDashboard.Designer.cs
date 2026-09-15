namespace PosBranch_Win.Dashboard
{
    partial class FrmExecutiveKpiDashboard
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
            this.pnlTopHeader = new System.Windows.Forms.Panel();
            this.pnlFilters = new System.Windows.Forms.Panel();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.dtTo = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblTo = new System.Windows.Forms.Label();
            this.dtFrom = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.lblFrom = new System.Windows.Forms.Label();
            this.comboPeriod = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblDateRange = new System.Windows.Forms.Label();
            this.lblDashboardSubtitle = new System.Windows.Forms.Label();
            this.lblDashboardTitle = new System.Windows.Forms.Label();
            this.pnlScrollableContent = new System.Windows.Forms.Panel();
            this.pnlTopHeader.SuspendLayout();
            this.pnlFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboPeriod)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlTopHeader
            // 
            this.pnlTopHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.pnlTopHeader.Controls.Add(this.pnlFilters);
            this.pnlTopHeader.Controls.Add(this.lblDashboardSubtitle);
            this.pnlTopHeader.Controls.Add(this.lblDashboardTitle);
            this.pnlTopHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTopHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlTopHeader.Name = "pnlTopHeader";
            this.pnlTopHeader.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this.pnlTopHeader.Size = new System.Drawing.Size(1260, 46);
            this.pnlTopHeader.TabIndex = 0;
            // 
            // pnlFilters
            // 
            this.pnlFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlFilters.Controls.Add(this.btnPrint);
            this.pnlFilters.Controls.Add(this.btnRefresh);
            this.pnlFilters.Controls.Add(this.btnApply);
            this.pnlFilters.Controls.Add(this.dtTo);
            this.pnlFilters.Controls.Add(this.lblTo);
            this.pnlFilters.Controls.Add(this.dtFrom);
            this.pnlFilters.Controls.Add(this.lblFrom);
            this.pnlFilters.Controls.Add(this.comboPeriod);
            this.pnlFilters.Controls.Add(this.lblDateRange);
            this.pnlFilters.Location = new System.Drawing.Point(544, 4);
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.Size = new System.Drawing.Size(704, 38);
            this.pnlFilters.TabIndex = 2;
            // 
            // btnPrint
            // 
            this.btnPrint.BackColor = System.Drawing.Color.White;
            this.btnPrint.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrint.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(210)))), ((int)(((byte)(255)))));
            this.btnPrint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnPrint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.btnPrint.Location = new System.Drawing.Point(616, 6);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(82, 26);
            this.btnPrint.TabIndex = 8;
            this.btnPrint.Text = "🖨️ Export";
            this.btnPrint.UseVisualStyleBackColor = false;
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.White;
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(210)))), ((int)(((byte)(255)))));
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnRefresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.btnRefresh.Location = new System.Drawing.Point(534, 6);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(76, 26);
            this.btnRefresh.TabIndex = 7;
            this.btnRefresh.Text = "🔄 Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            // 
            // btnApply
            // 
            this.btnApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(126)))), ((int)(((byte)(235)))));
            this.btnApply.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApply.FlatAppearance.BorderSize = 0;
            this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApply.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            this.btnApply.ForeColor = System.Drawing.Color.White;
            this.btnApply.Location = new System.Drawing.Point(456, 6);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(72, 26);
            this.btnApply.TabIndex = 6;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = false;
            // 
            // dtTo
            // 
            this.dtTo.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.dtTo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.dtTo.Location = new System.Drawing.Point(346, 7);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(102, 24);
            this.dtTo.TabIndex = 5;
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
            this.lblTo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblTo.Location = new System.Drawing.Point(320, 11);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(22, 13);
            this.lblTo.TabIndex = 4;
            this.lblTo.Text = "To:";
            // 
            // dtFrom
            // 
            this.dtFrom.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.dtFrom.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.dtFrom.Location = new System.Drawing.Point(212, 7);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(102, 24);
            this.dtFrom.TabIndex = 3;
            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
            this.lblFrom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblFrom.Location = new System.Drawing.Point(171, 11);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(36, 13);
            this.lblFrom.TabIndex = 2;
            this.lblFrom.Text = "From:";
            // 
            // comboPeriod
            // 
            this.comboPeriod.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.comboPeriod.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.comboPeriod.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.comboPeriod.Location = new System.Drawing.Point(53, 7);
            this.comboPeriod.Name = "comboPeriod";
            this.comboPeriod.Size = new System.Drawing.Size(110, 24);
            this.comboPeriod.TabIndex = 1;
            // 
            // lblDateRange
            // 
            this.lblDateRange.AutoSize = true;
            this.lblDateRange.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
            this.lblDateRange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblDateRange.Location = new System.Drawing.Point(6, 11);
            this.lblDateRange.Name = "lblDateRange";
            this.lblDateRange.Size = new System.Drawing.Size(43, 13);
            this.lblDateRange.TabIndex = 0;
            this.lblDateRange.Text = "Period:";
            // 
            // lblDashboardSubtitle
            // 
            this.lblDashboardSubtitle.AutoSize = true;
            this.lblDashboardSubtitle.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lblDashboardSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(110)))), ((int)(((byte)(150)))));
            this.lblDashboardSubtitle.Location = new System.Drawing.Point(14, 27);
            this.lblDashboardSubtitle.Name = "lblDashboardSubtitle";
            this.lblDashboardSubtitle.Size = new System.Drawing.Size(306, 12);
            this.lblDashboardSubtitle.TabIndex = 1;
            this.lblDashboardSubtitle.Text = "36 Key Business Metrics, Financial Health, Inventory Valuation & Growth Matrix";
            // 
            // lblDashboardTitle
            // 
            this.lblDashboardTitle.AutoSize = true;
            this.lblDashboardTitle.Font = new System.Drawing.Font("Segoe UI", 11.5F, System.Drawing.FontStyle.Bold);
            this.lblDashboardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(49)))), ((int)(((byte)(102)))));
            this.lblDashboardTitle.Location = new System.Drawing.Point(12, 4);
            this.lblDashboardTitle.Name = "lblDashboardTitle";
            this.lblDashboardTitle.Size = new System.Drawing.Size(232, 21);
            this.lblDashboardTitle.TabIndex = 0;
            this.lblDashboardTitle.Text = "Executive Business Dashboard";
            // 
            // pnlScrollableContent
            // 
            this.pnlScrollableContent.AutoScroll = true;
            this.pnlScrollableContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.pnlScrollableContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlScrollableContent.Location = new System.Drawing.Point(0, 46);
            this.pnlScrollableContent.Name = "pnlScrollableContent";
            this.pnlScrollableContent.Size = new System.Drawing.Size(1260, 734);
            this.pnlScrollableContent.TabIndex = 1;
            // 
            // FrmExecutiveKpiDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.ClientSize = new System.Drawing.Size(1260, 780);
            this.Controls.Add(this.pnlScrollableContent);
            this.Controls.Add(this.pnlTopHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.Name = "FrmExecutiveKpiDashboard";
            this.Text = "Executive Business Dashboard (30 KPIs)";
            this.pnlTopHeader.ResumeLayout(false);
            this.pnlTopHeader.PerformLayout();
            this.pnlFilters.ResumeLayout(false);
            this.pnlFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboPeriod)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTopHeader;
        private System.Windows.Forms.Label lblDashboardTitle;
        private System.Windows.Forms.Label lblDashboardSubtitle;
        private System.Windows.Forms.Panel pnlFilters;
        private System.Windows.Forms.Label lblDateRange;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboPeriod;
        private System.Windows.Forms.Label lblFrom;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtFrom;
        private System.Windows.Forms.Label lblTo;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtTo;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Panel pnlScrollableContent;
    }
}
