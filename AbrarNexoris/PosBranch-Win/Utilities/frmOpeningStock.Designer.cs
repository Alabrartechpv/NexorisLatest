
namespace PosBranch_Win.Utilities
{
    partial class frmOpeningStock
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
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance6 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance7 = new Infragistics.Win.Appearance();
            this.ultraPanelMain = new Infragistics.Win.Misc.UltraPanel();
            this.ultraGrid1 = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.lblRecordCount = new System.Windows.Forms.Label();
            this.ultraPanelTop = new Infragistics.Win.Misc.UltraPanel();
            this.lblCaption = new Infragistics.Win.Misc.UltraLabel();
            this.ultraPanelSearch = new Infragistics.Win.Misc.UltraPanel();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.ultraPanelGroup = new Infragistics.Win.Misc.UltraPanel();
            this.cmbGroup = new System.Windows.Forms.ComboBox();
            this.lblGroup = new System.Windows.Forms.Label();
            this.ultraPanelButtons = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanelLoadItems = new Infragistics.Win.Misc.UltraPanel();
            this.lblLoadItems = new System.Windows.Forms.Label();
            this.ultraPanelSave = new Infragistics.Win.Misc.UltraPanel();
            this.lblSave = new System.Windows.Forms.Label();
            this.ultraPanelRefresh = new Infragistics.Win.Misc.UltraPanel();
            this.lblRefresh = new System.Windows.Forms.Label();
            this.ultraPanelClose = new Infragistics.Win.Misc.UltraPanel();
            this.lblClose = new System.Windows.Forms.Label();
            this.ultraPanelMain.ClientArea.SuspendLayout();
            this.ultraPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGrid1)).BeginInit();
            this.ultraPanelTop.ClientArea.SuspendLayout();
            this.ultraPanelTop.SuspendLayout();
            this.ultraPanelSearch.ClientArea.SuspendLayout();
            this.ultraPanelSearch.SuspendLayout();
            this.ultraPanelGroup.ClientArea.SuspendLayout();
            this.ultraPanelGroup.SuspendLayout();
            this.ultraPanelButtons.ClientArea.SuspendLayout();
            this.ultraPanelButtons.SuspendLayout();
            this.ultraPanelLoadItems.ClientArea.SuspendLayout();
            this.ultraPanelLoadItems.SuspendLayout();
            this.ultraPanelSave.ClientArea.SuspendLayout();
            this.ultraPanelSave.SuspendLayout();
            this.ultraPanelRefresh.ClientArea.SuspendLayout();
            this.ultraPanelRefresh.SuspendLayout();
            this.ultraPanelClose.ClientArea.SuspendLayout();
            this.ultraPanelClose.SuspendLayout();
            this.SuspendLayout();
            // 
            // ultraPanelMain
            // 
            appearance1.BackColor = System.Drawing.Color.White;
            appearance1.BackColor2 = System.Drawing.Color.LightSteelBlue;
            appearance1.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.ultraPanelMain.Appearance = appearance1;
            this.ultraPanelMain.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;
            // 
            // ultraPanelMain.ClientArea
            // 
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraGrid1);
            this.ultraPanelMain.ClientArea.Controls.Add(this.lblRecordCount);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraPanelTop);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraPanelSearch);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraPanelGroup);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraPanelButtons);
            this.ultraPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMain.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelMain.Name = "ultraPanelMain";
            this.ultraPanelMain.Size = new System.Drawing.Size(1135, 526);
            this.ultraPanelMain.TabIndex = 1;
            // 
            // ultraGrid1
            // 
            this.ultraGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraGrid1.Location = new System.Drawing.Point(20, 123);
            this.ultraGrid1.Name = "ultraGrid1";
            this.ultraGrid1.Size = new System.Drawing.Size(1095, 317);
            this.ultraGrid1.TabIndex = 1;
            this.ultraGrid1.Text = "Opening Stock Items";
            // 
            // lblRecordCount
            // 
            this.lblRecordCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblRecordCount.AutoSize = true;
            this.lblRecordCount.BackColor = System.Drawing.Color.Transparent;
            this.lblRecordCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRecordCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(116)))), ((int)(((byte)(217)))));
            this.lblRecordCount.Location = new System.Drawing.Point(20, 450);
            this.lblRecordCount.Name = "lblRecordCount";
            this.lblRecordCount.Size = new System.Drawing.Size(113, 17);
            this.lblRecordCount.TabIndex = 4;
            this.lblRecordCount.Text = "Total Records: 0";
            // 
            // ultraPanelTop
            // 
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(93)))), ((int)(((byte)(144)))));
            appearance2.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(116)))), ((int)(((byte)(217)))));
            appearance2.BackGradientStyle = Infragistics.Win.GradientStyle.Horizontal;
            this.ultraPanelTop.Appearance = appearance2;
            this.ultraPanelTop.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            // 
            // ultraPanelTop.ClientArea
            // 
            this.ultraPanelTop.ClientArea.Controls.Add(this.lblCaption);
            this.ultraPanelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelTop.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelTop.Name = "ultraPanelTop";
            this.ultraPanelTop.Size = new System.Drawing.Size(1135, 50);
            this.ultraPanelTop.TabIndex = 0;
            // 
            // lblCaption
            // 
            this.lblCaption.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCaption.ForeColor = System.Drawing.Color.White;
            this.lblCaption.Location = new System.Drawing.Point(406, 8);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(323, 34);
            this.lblCaption.TabIndex = 0;
            this.lblCaption.Text = "Opening Stock Entry";
            // 
            // ultraPanelSearch
            // 
            appearance3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            appearance3.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(116)))), ((int)(((byte)(217)))));
            this.ultraPanelSearch.Appearance = appearance3;
            this.ultraPanelSearch.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelSearch.ClientArea
            // 
            this.ultraPanelSearch.ClientArea.Controls.Add(this.txtSearch);
            this.ultraPanelSearch.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanelSearch.Location = new System.Drawing.Point(20, 65);
            this.ultraPanelSearch.Name = "ultraPanelSearch";
            this.ultraPanelSearch.Size = new System.Drawing.Size(600, 45);
            this.ultraPanelSearch.TabIndex = 2;
            // 
            // txtSearch
            // 
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearch.Location = new System.Drawing.Point(90, 9);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(495, 26);
            this.txtSearch.TabIndex = 1;
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.BackColor = System.Drawing.Color.Transparent;
            this.lblSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSearch.ForeColor = System.Drawing.Color.Black;
            this.lblSearch.Location = new System.Drawing.Point(13, 12);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(61, 18);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search";
            // 
            // ultraPanelGroup
            // 
            appearance7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            appearance7.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(116)))), ((int)(((byte)(217)))));
            this.ultraPanelGroup.Appearance = appearance7;
            this.ultraPanelGroup.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelGroup.ClientArea
            // 
            this.ultraPanelGroup.ClientArea.Controls.Add(this.cmbGroup);
            this.ultraPanelGroup.ClientArea.Controls.Add(this.lblGroup);
            this.ultraPanelGroup.Location = new System.Drawing.Point(640, 65);
            this.ultraPanelGroup.Name = "ultraPanelGroup";
            this.ultraPanelGroup.Size = new System.Drawing.Size(475, 45);
            this.ultraPanelGroup.TabIndex = 4;
            // 
            // cmbGroup
            // 
            this.cmbGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbGroup.FormattingEnabled = true;
            this.cmbGroup.Location = new System.Drawing.Point(90, 9);
            this.cmbGroup.Name = "cmbGroup";
            this.cmbGroup.Size = new System.Drawing.Size(370, 28);
            this.cmbGroup.TabIndex = 1;
            this.cmbGroup.SelectedIndexChanged += new System.EventHandler(this.cmbGroup_SelectedIndexChanged);
            // 
            // lblGroup
            // 
            this.lblGroup.AutoSize = true;
            this.lblGroup.BackColor = System.Drawing.Color.Transparent;
            this.lblGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGroup.ForeColor = System.Drawing.Color.Black;
            this.lblGroup.Location = new System.Drawing.Point(13, 12);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(55, 18);
            this.lblGroup.TabIndex = 0;
            this.lblGroup.Text = "Group";
            // 
            // ultraPanelButtons
            // 
            this.ultraPanelButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            appearance4.BackColor = System.Drawing.Color.Transparent;
            this.ultraPanelButtons.Appearance = appearance4;
            this.ultraPanelButtons.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;
            // 
            // ultraPanelButtons.ClientArea
            // 
            this.ultraPanelButtons.ClientArea.Controls.Add(this.ultraPanelLoadItems);
            this.ultraPanelButtons.ClientArea.Controls.Add(this.ultraPanelSave);
            this.ultraPanelButtons.ClientArea.Controls.Add(this.ultraPanelRefresh);
            this.ultraPanelButtons.ClientArea.Controls.Add(this.ultraPanelClose);
            this.ultraPanelButtons.Location = new System.Drawing.Point(564, 455);
            this.ultraPanelButtons.Name = "ultraPanelButtons";
            this.ultraPanelButtons.Size = new System.Drawing.Size(551, 60);
            this.ultraPanelButtons.TabIndex = 3;
            // 
            // ultraPanelLoadItems
            // 
            appearance5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(239)))));
            appearance5.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(116)))), ((int)(((byte)(217)))));
            appearance5.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.ultraPanelLoadItems.Appearance = appearance5;
            this.ultraPanelLoadItems.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;
            // 
            // ultraPanelLoadItems.ClientArea
            // 
            this.ultraPanelLoadItems.ClientArea.Controls.Add(this.lblLoadItems);
            this.ultraPanelLoadItems.Location = new System.Drawing.Point(10, 5);
            this.ultraPanelLoadItems.Name = "ultraPanelLoadItems";
            this.ultraPanelLoadItems.Size = new System.Drawing.Size(125, 50);
            this.ultraPanelLoadItems.TabIndex = 0;
            // 
            // lblLoadItems
            // 
            this.lblLoadItems.BackColor = System.Drawing.Color.Transparent;
            this.lblLoadItems.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLoadItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLoadItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLoadItems.ForeColor = System.Drawing.Color.White;
            this.lblLoadItems.Location = new System.Drawing.Point(0, 0);
            this.lblLoadItems.Name = "lblLoadItems";
            this.lblLoadItems.Size = new System.Drawing.Size(121, 46);
            this.lblLoadItems.TabIndex = 0;
            this.lblLoadItems.Text = "Load Items";
            this.lblLoadItems.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLoadItems.Click += new System.EventHandler(this.lblLoadItems_Click);
            // 
            // ultraPanelSave
            // 
            appearance6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(239)))));
            appearance6.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(116)))), ((int)(((byte)(217)))));
            appearance6.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.ultraPanelSave.Appearance = appearance6;
            this.ultraPanelSave.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;
            // 
            // ultraPanelSave.ClientArea
            // 
            this.ultraPanelSave.ClientArea.Controls.Add(this.lblSave);
            this.ultraPanelSave.Location = new System.Drawing.Point(145, 5);
            this.ultraPanelSave.Name = "ultraPanelSave";
            this.ultraPanelSave.Size = new System.Drawing.Size(125, 50);
            this.ultraPanelSave.TabIndex = 1;
            // 
            // lblSave
            // 
            this.lblSave.BackColor = System.Drawing.Color.Transparent;
            this.lblSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSave.ForeColor = System.Drawing.Color.White;
            this.lblSave.Location = new System.Drawing.Point(0, 0);
            this.lblSave.Name = "lblSave";
            this.lblSave.Size = new System.Drawing.Size(121, 46);
            this.lblSave.TabIndex = 0;
            this.lblSave.Text = "Save";
            this.lblSave.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSave.Click += new System.EventHandler(this.lblSave_Click);
            // 
            // ultraPanelRefresh
            // 
            this.ultraPanelRefresh.Appearance = appearance5;
            this.ultraPanelRefresh.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;
            // 
            // ultraPanelRefresh.ClientArea
            // 
            this.ultraPanelRefresh.ClientArea.Controls.Add(this.lblRefresh);
            this.ultraPanelRefresh.Location = new System.Drawing.Point(280, 5);
            this.ultraPanelRefresh.Name = "ultraPanelRefresh";
            this.ultraPanelRefresh.Size = new System.Drawing.Size(125, 50);
            this.ultraPanelRefresh.TabIndex = 2;
            // 
            // lblRefresh
            // 
            this.lblRefresh.BackColor = System.Drawing.Color.Transparent;
            this.lblRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblRefresh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRefresh.ForeColor = System.Drawing.Color.White;
            this.lblRefresh.Location = new System.Drawing.Point(0, 0);
            this.lblRefresh.Name = "lblRefresh";
            this.lblRefresh.Size = new System.Drawing.Size(121, 46);
            this.lblRefresh.TabIndex = 0;
            this.lblRefresh.Text = "Refresh";
            this.lblRefresh.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRefresh.Click += new System.EventHandler(this.lblRefresh_Click);
            // 
            // ultraPanelClose
            // 
            this.ultraPanelClose.Appearance = appearance5;
            this.ultraPanelClose.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;
            // 
            // ultraPanelClose.ClientArea
            // 
            this.ultraPanelClose.ClientArea.Controls.Add(this.lblClose);
            this.ultraPanelClose.Location = new System.Drawing.Point(415, 5);
            this.ultraPanelClose.Name = "ultraPanelClose";
            this.ultraPanelClose.Size = new System.Drawing.Size(125, 50);
            this.ultraPanelClose.TabIndex = 3;
            // 
            // lblClose
            // 
            this.lblClose.BackColor = System.Drawing.Color.Transparent;
            this.lblClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClose.ForeColor = System.Drawing.Color.White;
            this.lblClose.Location = new System.Drawing.Point(0, 0);
            this.lblClose.Name = "lblClose";
            this.lblClose.Size = new System.Drawing.Size(121, 46);
            this.lblClose.TabIndex = 0;
            this.lblClose.Text = "Close";
            this.lblClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblClose.Click += new System.EventHandler(this.lblClose_Click);
            // 
            // frmOpeningStock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1135, 526);
            this.Controls.Add(this.ultraPanelMain);
            this.Name = "frmOpeningStock";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Opening Stock Entry";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmOpeningStock_Load);
            this.ultraPanelMain.ClientArea.ResumeLayout(false);
            this.ultraPanelMain.ClientArea.PerformLayout();
            this.ultraPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGrid1)).EndInit();
            this.ultraPanelTop.ClientArea.ResumeLayout(false);
            this.ultraPanelTop.ResumeLayout(false);
            this.ultraPanelSearch.ClientArea.ResumeLayout(false);
            this.ultraPanelSearch.ClientArea.PerformLayout();
            this.ultraPanelSearch.ResumeLayout(false);
            this.ultraPanelGroup.ClientArea.ResumeLayout(false);
            this.ultraPanelGroup.ClientArea.PerformLayout();
            this.ultraPanelGroup.ResumeLayout(false);
            this.ultraPanelButtons.ClientArea.ResumeLayout(false);
            this.ultraPanelButtons.ResumeLayout(false);
            this.ultraPanelLoadItems.ClientArea.ResumeLayout(false);
            this.ultraPanelLoadItems.ResumeLayout(false);
            this.ultraPanelSave.ClientArea.ResumeLayout(false);
            this.ultraPanelSave.ResumeLayout(false);
            this.ultraPanelRefresh.ClientArea.ResumeLayout(false);
            this.ultraPanelRefresh.ResumeLayout(false);
            this.ultraPanelClose.ClientArea.ResumeLayout(false);
            this.ultraPanelClose.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelMain;
        private Infragistics.Win.Misc.UltraPanel ultraPanelTop;
        private Infragistics.Win.Misc.UltraLabel lblCaption;
        private Infragistics.Win.Misc.UltraPanel ultraPanelSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGrid1;
        private System.Windows.Forms.Label lblRecordCount;
        private Infragistics.Win.Misc.UltraPanel ultraPanelButtons;
        private Infragistics.Win.Misc.UltraPanel ultraPanelLoadItems;
        private System.Windows.Forms.Label lblLoadItems;
        private Infragistics.Win.Misc.UltraPanel ultraPanelSave;
        private System.Windows.Forms.Label lblSave;
        private Infragistics.Win.Misc.UltraPanel ultraPanelRefresh;
        private System.Windows.Forms.Label lblRefresh;
        private Infragistics.Win.Misc.UltraPanel ultraPanelClose;
        private System.Windows.Forms.Label lblClose;
        private Infragistics.Win.Misc.UltraPanel ultraPanelGroup;
        private System.Windows.Forms.ComboBox cmbGroup;
        private System.Windows.Forms.Label lblGroup;
    }
}
