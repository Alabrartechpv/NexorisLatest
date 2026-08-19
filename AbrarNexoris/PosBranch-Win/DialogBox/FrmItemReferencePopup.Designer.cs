namespace PosBranch_Win.DialogBox
{
    partial class FrmItemReferencePopup
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
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.panelTopSearch = new System.Windows.Forms.Panel();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.lblGroup = new System.Windows.Forms.Label();
            this.cmbGroup = new System.Windows.Forms.ComboBox();
            this.lblHold = new System.Windows.Forms.Label();
            this.cmbHoldItems = new System.Windows.Forms.ComboBox();
            this.lblStockFilter = new System.Windows.Forms.Label();
            this.cmbStockFilter = new System.Windows.Forms.ComboBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.panelGridContainer = new System.Windows.Forms.Panel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelGridFooter = new Infragistics.Win.Misc.UltraPanel();
            this.panelHeader.SuspendLayout();
            this.panelTopSearch.SuspendLayout();
            this.panelGridContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.ultraPanelGridFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Height = 46;
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(151)))), ((int)(((byte)(214)))));
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Controls.Add(this.lblSubtitle);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 11.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(12, 5);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(250, 21);
            this.lblTitle.Text = "Items Master Reference (Ctrl + D)";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(245)))), ((int)(((byte)(255)))));
            this.lblSubtitle.Location = new System.Drawing.Point(13, 26);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(320, 13);
            this.lblSubtitle.Text = "Quick reference popup for product barcodes, prices, stock and details";
            // 
            // panelTopSearch
            // 
            this.panelTopSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTopSearch.Height = 44;
            this.panelTopSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(252)))), ((int)(((byte)(255)))));
            this.panelTopSearch.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.panelTopSearch.Controls.Add(this.lblSearch);
            this.panelTopSearch.Controls.Add(this.txtSearch);
            this.panelTopSearch.Controls.Add(this.lblCategory);
            this.panelTopSearch.Controls.Add(this.cmbCategory);
            this.panelTopSearch.Controls.Add(this.lblGroup);
            this.panelTopSearch.Controls.Add(this.cmbGroup);
            this.panelTopSearch.Controls.Add(this.lblHold);
            this.panelTopSearch.Controls.Add(this.cmbHoldItems);
            this.panelTopSearch.Controls.Add(this.lblStockFilter);
            this.panelTopSearch.Controls.Add(this.cmbStockFilter);
            this.panelTopSearch.Controls.Add(this.btnRefresh);
            this.panelTopSearch.Controls.Add(this.btnClose);
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblSearch.Location = new System.Drawing.Point(8, 14);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(45, 15);
            this.lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.txtSearch.Location = new System.Drawing.Point(54, 10);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(160, 23);
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCategory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblCategory.Location = new System.Drawing.Point(220, 14);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(58, 15);
            this.lblCategory.Text = "Category:";
            // 
            // cmbCategory
            // 
            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.cmbCategory.Location = new System.Drawing.Point(280, 10);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(130, 23);
            // 
            // lblGroup
            // 
            this.lblGroup.AutoSize = true;
            this.lblGroup.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblGroup.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblGroup.Location = new System.Drawing.Point(415, 14);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(43, 15);
            this.lblGroup.Text = "Group:";
            // 
            // cmbGroup
            // 
            this.cmbGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGroup.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.cmbGroup.Location = new System.Drawing.Point(460, 10);
            this.cmbGroup.Name = "cmbGroup";
            this.cmbGroup.Size = new System.Drawing.Size(120, 23);
            // 
            // lblHold
            // 
            this.lblHold.AutoSize = true;
            this.lblHold.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblHold.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblHold.Location = new System.Drawing.Point(585, 14);
            this.lblHold.Name = "lblHold";
            this.lblHold.Size = new System.Drawing.Size(36, 15);
            this.lblHold.Text = "Hold:";
            // 
            // cmbHoldItems
            // 
            this.cmbHoldItems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHoldItems.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.cmbHoldItems.Location = new System.Drawing.Point(623, 10);
            this.cmbHoldItems.Name = "cmbHoldItems";
            this.cmbHoldItems.Size = new System.Drawing.Size(100, 23);
            // 
            // lblStockFilter
            // 
            this.lblStockFilter.AutoSize = true;
            this.lblStockFilter.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblStockFilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblStockFilter.Location = new System.Drawing.Point(728, 14);
            this.lblStockFilter.Name = "lblStockFilter";
            this.lblStockFilter.Size = new System.Drawing.Size(39, 15);
            this.lblStockFilter.Text = "Stock:";
            // 
            // cmbStockFilter
            // 
            this.cmbStockFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStockFilter.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.cmbStockFilter.Location = new System.Drawing.Point(770, 10);
            this.cmbStockFilter.Name = "cmbStockFilter";
            this.cmbStockFilter.Size = new System.Drawing.Size(95, 23);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnRefresh.Location = new System.Drawing.Point(870, 9);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(65, 25);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnClose.Location = new System.Drawing.Point(940, 9);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(60, 25);
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // panelGridContainer
            // 
            this.panelGridContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGridContainer.Padding = new System.Windows.Forms.Padding(6);
            this.panelGridContainer.Controls.Add(this.gridReport);
            this.panelGridContainer.Controls.Add(this.ultraPanelGridFooter);
            // 
            // gridReport
            // 
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Location = new System.Drawing.Point(6, 6);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(998, 435);
            // 
            // ultraPanelGridFooter
            // 
            this.ultraPanelGridFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelGridFooter.Height = 26;
            this.ultraPanelGridFooter.Name = "ultraPanelGridFooter";
            // 
            // FrmItemReferencePopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1010, 555);
            this.Controls.Add(this.panelGridContainer);
            this.Controls.Add(this.panelTopSearch);
            this.Controls.Add(this.panelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(850, 450);
            this.Name = "FrmItemReferencePopup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Items Master Reference";
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelTopSearch.ResumeLayout(false);
            this.panelTopSearch.PerformLayout();
            this.panelGridContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.ultraPanelGridFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel panelTopSearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label lblGroup;
        private System.Windows.Forms.ComboBox cmbGroup;
        private System.Windows.Forms.Label lblHold;
        private System.Windows.Forms.ComboBox cmbHoldItems;
        private System.Windows.Forms.Label lblStockFilter;
        private System.Windows.Forms.ComboBox cmbStockFilter;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel panelGridContainer;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGridFooter;
    }
}
