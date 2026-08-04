namespace PosBranch_Win.Master
{
    partial class frmItemType
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelPage = new System.Windows.Forms.Panel();
            this.tableContent = new System.Windows.Forms.TableLayoutPanel();
            this.panelGrid = new System.Windows.Forms.Panel();
            this.gridReport = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panelGridFooter = new System.Windows.Forms.Panel();
            this.lblShowing = new System.Windows.Forms.Label();
            this.panelFilters = new System.Windows.Forms.Panel();
            this.lblItemType = new System.Windows.Forms.Label();
            this.txt_ItemType = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.btnSearchF11 = new Infragistics.Win.Misc.UltraButton();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnSetDefault = new Infragistics.Win.Misc.UltraButton();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelPage.SuspendLayout();
            this.tableContent.SuspendLayout();
            this.panelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).BeginInit();
            this.panelGridFooter.SuspendLayout();
            this.panelFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txt_ItemType)).BeginInit();
            this.SuspendLayout();
            // 
            // panelPage
            // 
            this.panelPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.panelPage.Controls.Add(this.tableContent);
            this.panelPage.Controls.Add(this.panelFilters);
            this.panelPage.Controls.Add(this.lblTitle);
            this.panelPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPage.Location = new System.Drawing.Point(0, 0);
            this.panelPage.Name = "panelPage";
            this.panelPage.Padding = new System.Windows.Forms.Padding(18, 14, 18, 8);
            this.panelPage.Size = new System.Drawing.Size(1000, 600);
            this.panelPage.TabIndex = 0;
            // 
            // tableContent
            // 
            this.tableContent.BackColor = System.Drawing.Color.Transparent;
            this.tableContent.ColumnCount = 1;
            this.tableContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableContent.Controls.Add(this.panelGrid, 0, 0);
            this.tableContent.Controls.Add(this.panelGridFooter, 0, 1);
            this.tableContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableContent.Location = new System.Drawing.Point(18, 163);
            this.tableContent.Name = "tableContent";
            this.tableContent.RowCount = 2;
            this.tableContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableContent.Size = new System.Drawing.Size(964, 429);
            this.tableContent.TabIndex = 2;
            // 
            // panelGrid
            // 
            this.panelGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.panelGrid.Controls.Add(this.gridReport);
            this.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGrid.Location = new System.Drawing.Point(0, 8);
            this.panelGrid.Margin = new System.Windows.Forms.Padding(0, 8, 0, 6);
            this.panelGrid.Name = "panelGrid";
            this.panelGrid.Padding = new System.Windows.Forms.Padding(2);
            this.panelGrid.Size = new System.Drawing.Size(964, 379);
            this.panelGrid.TabIndex = 0;
            // 
            // gridReport
            // 
            this.gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridReport.Location = new System.Drawing.Point(2, 2);
            this.gridReport.Name = "gridReport";
            this.gridReport.Size = new System.Drawing.Size(960, 375);
            this.gridReport.TabIndex = 0;
            this.gridReport.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // panelGridFooter
            // 
            this.panelGridFooter.BackColor = System.Drawing.Color.Transparent;
            this.panelGridFooter.Controls.Add(this.lblShowing);
            this.panelGridFooter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGridFooter.Location = new System.Drawing.Point(0, 393);
            this.panelGridFooter.Margin = new System.Windows.Forms.Padding(0);
            this.panelGridFooter.Name = "panelGridFooter";
            this.panelGridFooter.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.panelGridFooter.Size = new System.Drawing.Size(964, 36);
            this.panelGridFooter.TabIndex = 1;
            // 
            // lblShowing
            // 
            this.lblShowing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblShowing.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblShowing.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(98)))), ((int)(((byte)(138)))));
            this.lblShowing.Location = new System.Drawing.Point(0, 4);
            this.lblShowing.Name = "lblShowing";
            this.lblShowing.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.lblShowing.Size = new System.Drawing.Size(964, 32);
            this.lblShowing.TabIndex = 0;
            this.lblShowing.Text = "Showing 0 record(s)";
            this.lblShowing.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelFilters
            // 
            this.panelFilters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.panelFilters.Controls.Add(this.lblItemType);
            this.panelFilters.Controls.Add(this.txt_ItemType);
            this.panelFilters.Controls.Add(this.btnSearchF11);
            this.panelFilters.Controls.Add(this.btnSave);
            this.panelFilters.Controls.Add(this.btnUpdate);
            this.panelFilters.Controls.Add(this.btnSetDefault);
            this.panelFilters.Controls.Add(this.btnDelete);
            this.panelFilters.Controls.Add(this.btnClear);
            this.panelFilters.Controls.Add(this.btnClose);
            this.panelFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilters.Location = new System.Drawing.Point(18, 48);
            this.panelFilters.Name = "panelFilters";
            this.panelFilters.Size = new System.Drawing.Size(964, 115);
            this.panelFilters.TabIndex = 1;
            // 
            // lblItemType
            // 
            this.lblItemType.AutoSize = true;
            this.lblItemType.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblItemType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(98)))), ((int)(((byte)(138)))));
            this.lblItemType.Location = new System.Drawing.Point(15, 22);
            this.lblItemType.Name = "lblItemType";
            this.lblItemType.Size = new System.Drawing.Size(60, 15);
            this.lblItemType.TabIndex = 0;
            this.lblItemType.Text = "Item Type";
            // 
            // txt_ItemType
            // 
            this.txt_ItemType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txt_ItemType.Location = new System.Drawing.Point(85, 17);
            this.txt_ItemType.Name = "txt_ItemType";
            this.txt_ItemType.Size = new System.Drawing.Size(260, 25);
            this.txt_ItemType.TabIndex = 1;
            // 
            // btnSearchF11
            // 
            this.btnSearchF11.Location = new System.Drawing.Point(352, 17);
            this.btnSearchF11.Name = "btnSearchF11";
            this.btnSearchF11.Size = new System.Drawing.Size(45, 25);
            this.btnSearchF11.TabIndex = 2;
            this.btnSearchF11.Text = "F11";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(15, 62);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(85, 32);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(15, 62);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(85, 32);
            this.btnUpdate.TabIndex = 8;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = false;
            // 
            // btnSetDefault
            // 
            this.btnSetDefault.Location = new System.Drawing.Point(106, 62);
            this.btnSetDefault.Name = "btnSetDefault";
            this.btnSetDefault.Size = new System.Drawing.Size(100, 32);
            this.btnSetDefault.TabIndex = 9;
            this.btnSetDefault.Text = "Set Default";
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(212, 62);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(85, 32);
            this.btnDelete.TabIndex = 10;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(303, 62);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(85, 32);
            this.btnClear.TabIndex = 11;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(394, 62);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(85, 32);
            this.btnClose.TabIndex = 12;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblTitle.Location = new System.Drawing.Point(18, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(964, 34);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Item Type Master";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frmItemType
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.panelPage);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.KeyPreview = true;
            this.Name = "frmItemType";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Item Type Master";
            this.panelPage.ResumeLayout(false);
            this.tableContent.ResumeLayout(false);
            this.panelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridReport)).EndInit();
            this.panelGridFooter.ResumeLayout(false);
            this.panelFilters.ResumeLayout(false);
            this.panelFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txt_ItemType)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelPage;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelFilters;
        private System.Windows.Forms.Label lblItemType;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txt_ItemType;
        private Infragistics.Win.Misc.UltraButton btnSearchF11;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnUpdate;
        private Infragistics.Win.Misc.UltraButton btnSetDefault;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TableLayoutPanel tableContent;
        private System.Windows.Forms.Panel panelGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid gridReport;
        private System.Windows.Forms.Panel panelGridFooter;
        private System.Windows.Forms.Label lblShowing;
    }
}
