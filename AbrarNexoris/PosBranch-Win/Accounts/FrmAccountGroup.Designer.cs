namespace PosBranch_Win.Accounts
{
    partial class FrmAccountGroup
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
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            this.pnlMain = new Infragistics.Win.Misc.UltraPanel();
            this.tblMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblSectionAccountGroup = new Infragistics.Win.Misc.UltraLabel();
            this.ultralblAccountCode = new Infragistics.Win.Misc.UltraLabel();
            this.ultratxtAccCode = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.btnSearchGroup = new Infragistics.Win.Misc.UltraButton();
            this.ultralblAccountName = new Infragistics.Win.Misc.UltraLabel();
            this.ultratxtAccName = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultralblAccountCategory = new Infragistics.Win.Misc.UltraLabel();
            this.ultraDrpAccCategory = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.ultralblAccountType = new Infragistics.Win.Misc.UltraLabel();
            this.ultraDrpParentGroup = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.ultralblDescription = new Infragistics.Win.Misc.UltraLabel();
            this.ultratxtAccDescription = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.pnlMain.ClientArea.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.tblMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultratxtAccCode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultratxtAccName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDrpAccCategory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDrpParentGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultratxtAccDescription)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.pnlMain.Appearance = appearance1;
            // 
            // pnlMain.ClientArea
            // 
            this.pnlMain.ClientArea.Controls.Add(this.tblMain);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(1200, 700);
            this.pnlMain.TabIndex = 0;
            // 
            // tblMain
            // 
            this.tblMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(248)))), ((int)(((byte)(253)))));
            this.tblMain.ColumnCount = 4;
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.tblMain.Controls.Add(this.lblSectionAccountGroup, 0, 0);
            this.tblMain.Controls.Add(this.ultralblAccountCode, 0, 1);
            this.tblMain.Controls.Add(this.ultratxtAccCode, 1, 1);
            this.tblMain.Controls.Add(this.btnSearchGroup, 3, 1);
            this.tblMain.Controls.Add(this.ultralblAccountName, 0, 2);
            this.tblMain.Controls.Add(this.ultratxtAccName, 1, 2);
            this.tblMain.Controls.Add(this.ultralblAccountCategory, 2, 2);
            this.tblMain.Controls.Add(this.ultraDrpAccCategory, 3, 2);
            this.tblMain.Controls.Add(this.ultralblAccountType, 0, 3);
            this.tblMain.Controls.Add(this.ultraDrpParentGroup, 1, 3);
            this.tblMain.Controls.Add(this.ultralblDescription, 2, 3);
            this.tblMain.Controls.Add(this.ultratxtAccDescription, 3, 3);
            this.tblMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblMain.Location = new System.Drawing.Point(0, 0);
            this.tblMain.Margin = new System.Windows.Forms.Padding(0);
            this.tblMain.Name = "tblMain";
            this.tblMain.Padding = new System.Windows.Forms.Padding(30);
            this.tblMain.RowCount = 5;
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblMain.Size = new System.Drawing.Size(1200, 700);
            this.tblMain.TabIndex = 1;
            // 
            // lblSectionAccountGroup
            // 
            this.lblSectionAccountGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(225)))), ((int)(((byte)(242)))));
            appearance2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(77)))), ((int)(((byte)(128)))));
            appearance2.TextVAlignAsString = "Middle";
            this.lblSectionAccountGroup.Appearance = appearance2;
            this.tblMain.SetColumnSpan(this.lblSectionAccountGroup, 4);
            this.lblSectionAccountGroup.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblSectionAccountGroup.Location = new System.Drawing.Point(33, 33);
            this.lblSectionAccountGroup.Name = "lblSectionAccountGroup";
            this.lblSectionAccountGroup.Padding = new System.Drawing.Size(10, 0);
            this.lblSectionAccountGroup.Size = new System.Drawing.Size(1134, 20);
            this.lblSectionAccountGroup.TabIndex = 0;
            this.lblSectionAccountGroup.Text = "Account Group Details";
            // 
            // ultralblAccountCode
            // 
            appearance3.BackColor = System.Drawing.Color.Transparent;
            appearance3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(62)))), ((int)(((byte)(89)))));
            appearance3.TextVAlignAsString = "Middle";
            this.ultralblAccountCode.Appearance = appearance3;
            this.ultralblAccountCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultralblAccountCode.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ultralblAccountCode.Location = new System.Drawing.Point(33, 63);
            this.ultralblAccountCode.Name = "ultralblAccountCode";
            this.ultralblAccountCode.Size = new System.Drawing.Size(153, 36);
            this.ultralblAccountCode.TabIndex = 1;
            this.ultralblAccountCode.Text = "Group ID";
            // 
            // ultratxtAccCode
            // 
            appearance4.BackColor = System.Drawing.Color.White;
            appearance4.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(183)))), ((int)(((byte)(220)))));
            appearance4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(92)))), ((int)(((byte)(143)))));
            this.ultratxtAccCode.Appearance = appearance4;
            this.ultratxtAccCode.BackColor = System.Drawing.Color.White;
            this.ultratxtAccCode.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultratxtAccCode.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.ultratxtAccCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultratxtAccCode.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.ultratxtAccCode.Location = new System.Drawing.Point(192, 66);
            this.ultratxtAccCode.Margin = new System.Windows.Forms.Padding(3, 6, 18, 6);
            this.ultratxtAccCode.Name = "ultratxtAccCode";
            this.ultratxtAccCode.Size = new System.Drawing.Size(389, 24);
            this.ultratxtAccCode.TabIndex = 2;
            // 
            // btnSearchGroup
            // 
            this.btnSearchGroup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            appearance5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            appearance5.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(184)))));
            appearance5.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance5.FontData.BoldAsString = "True";
            appearance5.ForeColor = System.Drawing.Color.White;
            this.btnSearchGroup.Appearance = appearance5;
            this.btnSearchGroup.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            this.btnSearchGroup.Location = new System.Drawing.Point(761, 69);
            this.btnSearchGroup.Name = "btnSearchGroup";
            this.btnSearchGroup.Size = new System.Drawing.Size(150, 24);
            this.btnSearchGroup.TabIndex = 3;
            this.btnSearchGroup.Text = "Search Account Group";
            this.btnSearchGroup.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultralblAccountName
            // 
            this.ultralblAccountName.Appearance = appearance3;
            this.ultralblAccountName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultralblAccountName.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ultralblAccountName.Location = new System.Drawing.Point(33, 105);
            this.ultralblAccountName.Name = "ultralblAccountName";
            this.ultralblAccountName.Size = new System.Drawing.Size(153, 36);
            this.ultralblAccountName.TabIndex = 4;
            this.ultralblAccountName.Text = "Group Name";
            // 
            // ultratxtAccName
            // 
            this.ultratxtAccName.Appearance = appearance4;
            this.ultratxtAccName.BackColor = System.Drawing.Color.White;
            this.ultratxtAccName.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultratxtAccName.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.ultratxtAccName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultratxtAccName.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.ultratxtAccName.Location = new System.Drawing.Point(192, 108);
            this.ultratxtAccName.Margin = new System.Windows.Forms.Padding(3, 6, 18, 6);
            this.ultratxtAccName.Name = "ultratxtAccName";
            this.ultratxtAccName.Size = new System.Drawing.Size(389, 24);
            this.ultratxtAccName.TabIndex = 5;
            // 
            // ultralblAccountCategory
            // 
            this.ultralblAccountCategory.Appearance = appearance3;
            this.ultralblAccountCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultralblAccountCategory.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ultralblAccountCategory.Location = new System.Drawing.Point(602, 105);
            this.ultralblAccountCategory.Name = "ultralblAccountCategory";
            this.ultralblAccountCategory.Size = new System.Drawing.Size(153, 36);
            this.ultralblAccountCategory.TabIndex = 6;
            this.ultralblAccountCategory.Text = "Category";
            // 
            // ultraDrpAccCategory
            // 
            this.ultraDrpAccCategory.Appearance = appearance4;
            this.ultraDrpAccCategory.BackColor = System.Drawing.Color.White;
            this.ultraDrpAccCategory.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraDrpAccCategory.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.ultraDrpAccCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraDrpAccCategory.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraDrpAccCategory.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.ultraDrpAccCategory.Location = new System.Drawing.Point(761, 108);
            this.ultraDrpAccCategory.Margin = new System.Windows.Forms.Padding(3, 6, 18, 6);
            this.ultraDrpAccCategory.Name = "ultraDrpAccCategory";
            this.ultraDrpAccCategory.Size = new System.Drawing.Size(391, 24);
            this.ultraDrpAccCategory.TabIndex = 7;
            // 
            // ultralblAccountType
            // 
            this.ultralblAccountType.Appearance = appearance3;
            this.ultralblAccountType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultralblAccountType.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ultralblAccountType.Location = new System.Drawing.Point(33, 147);
            this.ultralblAccountType.Name = "ultralblAccountType";
            this.ultralblAccountType.Size = new System.Drawing.Size(153, 36);
            this.ultralblAccountType.TabIndex = 8;
            this.ultralblAccountType.Text = "Parent Group";
            // 
            // ultraDrpParentGroup
            // 
            this.ultraDrpParentGroup.Appearance = appearance4;
            this.ultraDrpParentGroup.BackColor = System.Drawing.Color.White;
            this.ultraDrpParentGroup.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraDrpParentGroup.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.ultraDrpParentGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraDrpParentGroup.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.ultraDrpParentGroup.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.ultraDrpParentGroup.Location = new System.Drawing.Point(192, 150);
            this.ultraDrpParentGroup.Margin = new System.Windows.Forms.Padding(3, 6, 18, 6);
            this.ultraDrpParentGroup.Name = "ultraDrpParentGroup";
            this.ultraDrpParentGroup.Size = new System.Drawing.Size(389, 24);
            this.ultraDrpParentGroup.TabIndex = 9;
            // 
            // ultralblDescription
            // 
            this.ultralblDescription.Appearance = appearance3;
            this.ultralblDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultralblDescription.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ultralblDescription.Location = new System.Drawing.Point(602, 147);
            this.ultralblDescription.Name = "ultralblDescription";
            this.ultralblDescription.Size = new System.Drawing.Size(153, 36);
            this.ultralblDescription.TabIndex = 10;
            this.ultralblDescription.Text = "Description";
            // 
            // ultratxtAccDescription
            // 
            this.ultratxtAccDescription.Appearance = appearance4;
            this.ultratxtAccDescription.BackColor = System.Drawing.Color.White;
            this.ultratxtAccDescription.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultratxtAccDescription.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            this.ultratxtAccDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultratxtAccDescription.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.ultratxtAccDescription.Location = new System.Drawing.Point(761, 150);
            this.ultratxtAccDescription.Margin = new System.Windows.Forms.Padding(3, 6, 18, 6);
            this.ultratxtAccDescription.Name = "ultratxtAccDescription";
            this.ultratxtAccDescription.Size = new System.Drawing.Size(391, 24);
            this.ultratxtAccDescription.TabIndex = 11;
            // 
            // FrmAccountGroup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.pnlMain);
            this.Name = "FrmAccountGroup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Account Group Master";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.pnlMain.ClientArea.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.tblMain.ResumeLayout(false);
            this.tblMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultratxtAccCode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultratxtAccName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDrpAccCategory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDrpParentGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultratxtAccDescription)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel pnlMain;
        private System.Windows.Forms.TableLayoutPanel tblMain;
        
        private Infragistics.Win.Misc.UltraLabel lblSectionAccountGroup;
        
        private Infragistics.Win.Misc.UltraLabel ultralblAccountCode;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultratxtAccCode;
        private Infragistics.Win.Misc.UltraButton btnSearchGroup;
        
        private Infragistics.Win.Misc.UltraLabel ultralblAccountName;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultratxtAccName;
        
        private Infragistics.Win.Misc.UltraLabel ultralblAccountCategory;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraDrpAccCategory;
        
        private Infragistics.Win.Misc.UltraLabel ultralblAccountType;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraDrpParentGroup;
        
        private Infragistics.Win.Misc.UltraLabel ultralblDescription;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultratxtAccDescription;
    }
}