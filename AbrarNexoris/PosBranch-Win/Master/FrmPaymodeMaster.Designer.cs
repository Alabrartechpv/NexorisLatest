namespace PosBranch_Win.Master
{
    partial class FrmPaymodeMaster
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
            Infragistics.Win.Appearance appearanceTitle = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceSave = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceRefresh = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceClose = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceBottom = new Infragistics.Win.Appearance();

            this.ultraPanelMain = new Infragistics.Win.Misc.UltraPanel();
            this.ultraLabelTitle = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxGrid = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraGridPaymode = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanelBottom = new Infragistics.Win.Misc.UltraPanel();
            this.btnSave = new Infragistics.Win.Misc.UltraButton();
            this.btnRefresh = new Infragistics.Win.Misc.UltraButton();
            this.btnClose = new Infragistics.Win.Misc.UltraButton();

            this.ultraPanelMain.ClientArea.SuspendLayout();
            this.ultraPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxGrid)).BeginInit();
            this.ultraGroupBoxGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridPaymode)).BeginInit();
            this.ultraPanelBottom.ClientArea.SuspendLayout();
            this.ultraPanelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // ultraPanelMain
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.ultraPanelMain.Appearance = appearance1;
            // 
            // ultraPanelMain.ClientArea
            // 
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraGroupBoxGrid);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraPanelBottom);
            this.ultraPanelMain.ClientArea.Controls.Add(this.ultraLabelTitle);
            this.ultraPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelMain.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelMain.Name = "ultraPanelMain";
            this.ultraPanelMain.Size = new System.Drawing.Size(900, 580);
            this.ultraPanelMain.TabIndex = 0;
            // 
            // ultraLabelTitle
            // 
            appearanceTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            appearanceTitle.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(160)))));
            appearanceTitle.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceTitle.FontData.BoldAsString = "True";
            appearanceTitle.FontData.Name = "Segoe UI";
            appearanceTitle.FontData.SizeInPoints = 14F;
            appearanceTitle.ForeColor = System.Drawing.Color.White;
            appearanceTitle.TextHAlignAsString = "Center";
            appearanceTitle.TextVAlignAsString = "Middle";
            this.ultraLabelTitle.Appearance = appearanceTitle;
            this.ultraLabelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraLabelTitle.Location = new System.Drawing.Point(0, 0);
            this.ultraLabelTitle.Name = "ultraLabelTitle";
            this.ultraLabelTitle.Size = new System.Drawing.Size(900, 45);
            this.ultraLabelTitle.TabIndex = 0;
            this.ultraLabelTitle.Text = "Paymode Account Setup";
            // 
            // ultraGroupBoxGrid
            // 
            this.ultraGroupBoxGrid.Controls.Add(this.ultraGridPaymode);
            this.ultraGroupBoxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGroupBoxGrid.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.ultraGroupBoxGrid.HeaderBorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            this.ultraGroupBoxGrid.Location = new System.Drawing.Point(10, 55);
            this.ultraGroupBoxGrid.Name = "ultraGroupBoxGrid";
            this.ultraGroupBoxGrid.Size = new System.Drawing.Size(880, 460);
            this.ultraGroupBoxGrid.TabIndex = 1;
            this.ultraGroupBoxGrid.Text = "Payment Mode to Chart of Accounts Ledger Mapping";
            this.ultraGroupBoxGrid.ViewStyle = Infragistics.Win.Misc.GroupBoxViewStyle.Office2007;
            // 
            // ultraGridPaymode
            // 
            this.ultraGridPaymode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridPaymode.Location = new System.Drawing.Point(3, 23);
            this.ultraGridPaymode.Name = "ultraGridPaymode";
            this.ultraGridPaymode.Size = new System.Drawing.Size(874, 434);
            this.ultraGridPaymode.TabIndex = 0;
            // 
            // ultraPanelBottom
            // 
            appearanceBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(245)))));
            appearanceBottom.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(215)))), ((int)(((byte)(220)))));
            this.ultraPanelBottom.Appearance = appearanceBottom;
            this.ultraPanelBottom.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            // 
            // ultraPanelBottom.ClientArea
            // 
            this.ultraPanelBottom.ClientArea.Controls.Add(this.btnSave);
            this.ultraPanelBottom.ClientArea.Controls.Add(this.btnRefresh);
            this.ultraPanelBottom.ClientArea.Controls.Add(this.btnClose);
            this.ultraPanelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelBottom.Location = new System.Drawing.Point(0, 525);
            this.ultraPanelBottom.Name = "ultraPanelBottom";
            this.ultraPanelBottom.Size = new System.Drawing.Size(900, 55);
            this.ultraPanelBottom.TabIndex = 2;
            // 
            // btnSave
            // 
            appearanceSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            appearanceSave.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(184)))));
            appearanceSave.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceSave.FontData.BoldAsString = "True";
            appearanceSave.FontData.Name = "Segoe UI";
            appearanceSave.FontData.SizeInPoints = 10F;
            appearanceSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Appearance = appearanceSave;
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(530, 10);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(115, 35);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save Mapping";
            this.btnSave.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnRefresh
            // 
            appearanceRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            appearanceRefresh.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(130)))), ((int)(((byte)(118)))));
            appearanceRefresh.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceRefresh.FontData.BoldAsString = "True";
            appearanceRefresh.FontData.Name = "Segoe UI";
            appearanceRefresh.FontData.SizeInPoints = 10F;
            appearanceRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Appearance = appearanceRefresh;
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(655, 10);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(105, 35);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnClose
            // 
            appearanceClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(47)))), ((int)(((byte)(47)))));
            appearanceClose.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            appearanceClose.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceClose.FontData.BoldAsString = "True";
            appearanceClose.FontData.Name = "Segoe UI";
            appearanceClose.FontData.SizeInPoints = 10F;
            appearanceClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Appearance = appearanceClose;
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(770, 10);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(105, 35);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // FrmPaymodeMaster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 580);
            this.Controls.Add(this.ultraPanelMain);
            this.Name = "FrmPaymodeMaster";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Paymode Account Setup";
            this.Load += new System.EventHandler(this.FrmPaymodeMaster_Load);
            this.ultraPanelMain.ClientArea.ResumeLayout(false);
            this.ultraPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxGrid)).EndInit();
            this.ultraGroupBoxGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGridPaymode)).EndInit();
            this.ultraPanelBottom.ClientArea.ResumeLayout(false);
            this.ultraPanelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.Misc.UltraPanel ultraPanelMain;
        private Infragistics.Win.Misc.UltraLabel ultraLabelTitle;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxGrid;
        private Infragistics.Win.UltraWinGrid.UltraGrid ultraGridPaymode;
        private Infragistics.Win.Misc.UltraPanel ultraPanelBottom;
        private Infragistics.Win.Misc.UltraButton btnSave;
        private Infragistics.Win.Misc.UltraButton btnRefresh;
        private Infragistics.Win.Misc.UltraButton btnClose;
    }
}
