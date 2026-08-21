namespace PosBranch_Win.Settings
{
    partial class FrmLanguageSettings
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
            this.panelPage = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelHeaderCard = new System.Windows.Forms.Panel();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.panelCard = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lstLanguages = new System.Windows.Forms.ListBox();
            this.lblSelectPrompt = new System.Windows.Forms.Label();
            this.panelFooter = new System.Windows.Forms.Panel();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnResetDefault = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.panelPage.SuspendLayout();
            this.panelHeaderCard.SuspendLayout();
            this.panelCard.SuspendLayout();
            this.panelFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelPage
            // 
            this.panelPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.panelPage.Controls.Add(this.panelFooter);
            this.panelPage.Controls.Add(this.panelCard);
            this.panelPage.Controls.Add(this.panelHeaderCard);
            this.panelPage.Controls.Add(this.lblTitle);
            this.panelPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPage.Location = new System.Drawing.Point(0, 0);
            this.panelPage.Name = "panelPage";
            this.panelPage.Padding = new System.Windows.Forms.Padding(18, 14, 18, 12);
            this.panelPage.Size = new System.Drawing.Size(860, 520);
            this.panelPage.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lblTitle.Location = new System.Drawing.Point(18, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(824, 34);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "App Language Settings";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelHeaderCard
            // 
            this.panelHeaderCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.panelHeaderCard.Controls.Add(this.lblSubtitle);
            this.panelHeaderCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeaderCard.Location = new System.Drawing.Point(18, 48);
            this.panelHeaderCard.Name = "panelHeaderCard";
            this.panelHeaderCard.Padding = new System.Windows.Forms.Padding(14, 10, 14, 10);
            this.panelHeaderCard.Size = new System.Drawing.Size(824, 44);
            this.panelHeaderCard.TabIndex = 1;
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.25F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(98)))), ((int)(((byte)(138)))));
            this.lblSubtitle.Location = new System.Drawing.Point(14, 10);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(796, 24);
            this.lblSubtitle.TabIndex = 0;
            this.lblSubtitle.Text = "Select your preferred application language or import custom translations";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelCard
            // 
            this.panelCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(253)))), ((int)(((byte)(255)))));
            this.panelCard.Controls.Add(this.lblStatus);
            this.panelCard.Controls.Add(this.lstLanguages);
            this.panelCard.Controls.Add(this.lblSelectPrompt);
            this.panelCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCard.Location = new System.Drawing.Point(18, 102);
            this.panelCard.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.panelCard.Name = "panelCard";
            this.panelCard.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.panelCard.Size = new System.Drawing.Size(824, 340);
            this.panelCard.TabIndex = 2;
            // 
            // lblSelectPrompt
            // 
            this.lblSelectPrompt.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSelectPrompt.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblSelectPrompt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(98)))), ((int)(((byte)(138)))));
            this.lblSelectPrompt.Location = new System.Drawing.Point(16, 12);
            this.lblSelectPrompt.Name = "lblSelectPrompt";
            this.lblSelectPrompt.Size = new System.Drawing.Size(792, 26);
            this.lblSelectPrompt.TabIndex = 0;
            this.lblSelectPrompt.Text = "Available Languages:";
            this.lblSelectPrompt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lstLanguages
            // 
            this.lstLanguages.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstLanguages.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstLanguages.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lstLanguages.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(120)))));
            this.lstLanguages.ItemHeight = 36;
            this.lstLanguages.Location = new System.Drawing.Point(16, 38);
            this.lstLanguages.Name = "lstLanguages";
            this.lstLanguages.Size = new System.Drawing.Size(792, 240);
            this.lstLanguages.TabIndex = 1;
            // 
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(140)))), ((int)(((byte)(90)))));
            this.lblStatus.Location = new System.Drawing.Point(16, 298);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(792, 30);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Ready: English (EN)";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelFooter
            // 
            this.panelFooter.BackColor = System.Drawing.Color.Transparent;
            this.panelFooter.Controls.Add(this.btnApply);
            this.panelFooter.Controls.Add(this.btnResetDefault);
            this.panelFooter.Controls.Add(this.btnImport);
            this.panelFooter.Controls.Add(this.btnExport);
            this.panelFooter.Controls.Add(this.btnClose);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Location = new System.Drawing.Point(18, 452);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.panelFooter.Size = new System.Drawing.Size(824, 56);
            this.panelFooter.TabIndex = 3;
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(0, 10);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(140, 36);
            this.btnApply.TabIndex = 0;
            this.btnApply.Text = "Apply Language";
            this.btnApply.UseVisualStyleBackColor = false;
            // 
            // btnResetDefault
            // 
            this.btnResetDefault.Location = new System.Drawing.Point(150, 10);
            this.btnResetDefault.Name = "btnResetDefault";
            this.btnResetDefault.Size = new System.Drawing.Size(185, 36);
            this.btnResetDefault.TabIndex = 1;
            this.btnResetDefault.Text = "Reset to Default (English)";
            this.btnResetDefault.UseVisualStyleBackColor = false;
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(345, 10);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(175, 36);
            this.btnImport.TabIndex = 2;
            this.btnImport.Text = "Import Custom Language";
            this.btnImport.UseVisualStyleBackColor = false;
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(530, 10);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(145, 36);
            this.btnExport.TabIndex = 3;
            this.btnExport.Text = "Export Template";
            this.btnExport.UseVisualStyleBackColor = false;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(685, 10);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 36);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // FrmLanguageSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(860, 520);
            this.Controls.Add(this.panelPage);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmLanguageSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "App Language Settings";
            this.panelPage.ResumeLayout(false);
            this.panelHeaderCard.ResumeLayout(false);
            this.panelCard.ResumeLayout(false);
            this.panelFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelPage;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelHeaderCard;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel panelCard;
        private System.Windows.Forms.Label lblSelectPrompt;
        private System.Windows.Forms.ListBox lstLanguages;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnResetDefault;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnClose;
    }
}
