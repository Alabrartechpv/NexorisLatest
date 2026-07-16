namespace PosBranch_Win.ChartOfAccount
{
    partial class FrmChartOfAcc
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
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            this.ultraPanelTreeViewAcc = new Infragistics.Win.Misc.UltraPanel();
            this.ultrPanelBtn = new Infragistics.Win.Misc.UltraPanel();
            this.btnRefresh = new Infragistics.Win.Misc.UltraButton();
            this.btnExpandAll = new Infragistics.Win.Misc.UltraButton();
            this.btnCollapseAll = new Infragistics.Win.Misc.UltraButton();
            this.ultraPanelDataEntri = new Infragistics.Win.Misc.UltraPanel();
            this.UltraPanelTreeView = new Infragistics.Win.Misc.UltraPanel();
            this.ultraTree1 = new Infragistics.Win.UltraWinTree.UltraTree();
            this.label1 = new System.Windows.Forms.Label();
            this.ultraPanelTreeViewAcc.ClientArea.SuspendLayout();
            this.ultraPanelTreeViewAcc.SuspendLayout();
            this.ultrPanelBtn.ClientArea.SuspendLayout();
            this.ultrPanelBtn.SuspendLayout();
            this.ultraPanelDataEntri.SuspendLayout();
            this.UltraPanelTreeView.ClientArea.SuspendLayout();
            this.UltraPanelTreeView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTree1)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraPanelTreeViewAcc
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.ultraPanelTreeViewAcc.Appearance = appearance1;
            // 
            // ultraPanelTreeViewAcc.ClientArea
            // 
            this.ultraPanelTreeViewAcc.ClientArea.Controls.Add(this.ultraPanelDataEntri);
            this.ultraPanelTreeViewAcc.ClientArea.Controls.Add(this.UltraPanelTreeView);
            this.ultraPanelTreeViewAcc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelTreeViewAcc.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelTreeViewAcc.Name = "ultraPanelTreeViewAcc";
            this.ultraPanelTreeViewAcc.Size = new System.Drawing.Size(1200, 533);
            this.ultraPanelTreeViewAcc.TabIndex = 0;
            // 
            // ultrPanelBtn
            // 
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ultrPanelBtn.Appearance = appearance2;
            // 
            // ultrPanelBtn.ClientArea
            // 
            this.ultrPanelBtn.ClientArea.Controls.Add(this.btnRefresh);
            this.ultrPanelBtn.ClientArea.Controls.Add(this.btnExpandAll);
            this.ultrPanelBtn.ClientArea.Controls.Add(this.btnCollapseAll);
            this.ultrPanelBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultrPanelBtn.Location = new System.Drawing.Point(0, 493);
            this.ultrPanelBtn.Name = "ultrPanelBtn";
            this.ultrPanelBtn.Size = new System.Drawing.Size(371, 40);
            this.ultrPanelBtn.TabIndex = 2;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(12, 8);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(101, 24);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Refresh";
            // 
            // btnExpandAll
            // 
            this.btnExpandAll.Location = new System.Drawing.Point(125, 8);
            this.btnExpandAll.Name = "btnExpandAll";
            this.btnExpandAll.Size = new System.Drawing.Size(101, 24);
            this.btnExpandAll.TabIndex = 1;
            this.btnExpandAll.Text = "Expand All";
            // 
            // btnCollapseAll
            // 
            this.btnCollapseAll.Location = new System.Drawing.Point(238, 8);
            this.btnCollapseAll.Name = "btnCollapseAll";
            this.btnCollapseAll.Size = new System.Drawing.Size(101, 24);
            this.btnCollapseAll.TabIndex = 2;
            this.btnCollapseAll.Text = "Collapse All";
            // 
            // ultraPanelDataEntri
            // 
            appearance3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ultraPanelDataEntri.Appearance = appearance3;
            this.ultraPanelDataEntri.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelDataEntri.Location = new System.Drawing.Point(371, 0);
            this.ultraPanelDataEntri.Name = "ultraPanelDataEntri";
            this.ultraPanelDataEntri.Size = new System.Drawing.Size(829, 533);
            this.ultraPanelDataEntri.TabIndex = 1;
            // 
            // UltraPanelTreeView
            // 
            appearance4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.UltraPanelTreeView.Appearance = appearance4;
            // 
            // UltraPanelTreeView.ClientArea
            // 
            this.UltraPanelTreeView.ClientArea.Controls.Add(this.ultrPanelBtn);
            this.UltraPanelTreeView.ClientArea.Controls.Add(this.ultraTree1);
            this.UltraPanelTreeView.ClientArea.Controls.Add(this.label1);
            this.UltraPanelTreeView.Dock = System.Windows.Forms.DockStyle.Left;
            this.UltraPanelTreeView.Location = new System.Drawing.Point(0, 0);
            this.UltraPanelTreeView.Name = "UltraPanelTreeView";
            this.UltraPanelTreeView.Size = new System.Drawing.Size(371, 533);
            this.UltraPanelTreeView.TabIndex = 0;
            // 
            // ultraTree1
            // 
            this.ultraTree1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraTree1.Location = new System.Drawing.Point(12, 45);
            this.ultraTree1.Name = "ultraTree1";
            this.ultraTree1.Size = new System.Drawing.Size(347, 435);
            this.ultraTree1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(77)))), ((int)(((byte)(128)))));
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "Chart of Accounts";
            // 
            // FrmChartOfAcc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 533);
            this.Controls.Add(this.ultraPanelTreeViewAcc);
            this.Name = "FrmChartOfAcc";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Chart of Accounts";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ultraPanelTreeViewAcc.ClientArea.ResumeLayout(false);
            this.ultraPanelTreeViewAcc.ResumeLayout(false);
            this.ultrPanelBtn.ClientArea.ResumeLayout(false);
            this.ultrPanelBtn.ResumeLayout(false);
            this.ultraPanelDataEntri.ResumeLayout(false);
            this.UltraPanelTreeView.ClientArea.ResumeLayout(false);
            this.UltraPanelTreeView.ClientArea.PerformLayout();
            this.UltraPanelTreeView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraTree1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelTreeViewAcc;
        private Infragistics.Win.Misc.UltraPanel ultraPanelDataEntri;
        private Infragistics.Win.Misc.UltraPanel ultrPanelBtn;
        private Infragistics.Win.Misc.UltraPanel UltraPanelTreeView;
        private Infragistics.Win.UltraWinTree.UltraTree ultraTree1;
        private Infragistics.Win.Misc.UltraButton btnRefresh;
        private Infragistics.Win.Misc.UltraButton btnExpandAll;
        private Infragistics.Win.Misc.UltraButton btnCollapseAll;
        private System.Windows.Forms.Label label1;
    }
}