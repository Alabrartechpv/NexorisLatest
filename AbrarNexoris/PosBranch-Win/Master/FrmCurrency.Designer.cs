namespace PosBranch_Win.Master
{
    partial class FrmCurrency
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
            this.components = new System.ComponentModel.Container();
            Infragistics.Win.Appearance appearanceBtn = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearanceBtnHot = new Infragistics.Win.Appearance();

            appearanceBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            appearanceBtn.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(205)))), ((int)(((byte)(245)))));
            appearanceBtn.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceBtn.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(165)))), ((int)(((byte)(210)))));
            appearanceBtn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(95)))));
            appearanceBtn.FontData.BoldAsString = "True";
            appearanceBtn.FontData.Name = "Segoe UI";
            appearanceBtn.FontData.SizeInPoints = 8.5F;
            appearanceBtn.TextHAlignAsString = "Center";
            appearanceBtn.TextVAlignAsString = "Middle";

            appearanceBtnHot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            appearanceBtnHot.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(222)))), ((int)(((byte)(255)))));
            appearanceBtnHot.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearanceBtnHot.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(140)))), ((int)(((byte)(200)))));

            this.pnlMainBackground = new System.Windows.Forms.Panel();

            this.lblCurrencyCode = new Infragistics.Win.Misc.UltraLabel();
            this.txtCurrencyCode = new Infragistics.Win.UltraWinEditors.UltraTextEditor();

            this.btnLookupF7 = new Infragistics.Win.Misc.UltraButton();
            this.pnlNavStrip = new System.Windows.Forms.Panel();
            this.btnFirst = new Infragistics.Win.Misc.UltraButton();
            this.btnPrev = new Infragistics.Win.Misc.UltraButton();
            this.btnNext = new Infragistics.Win.Misc.UltraButton();
            this.btnLast = new Infragistics.Win.Misc.UltraButton();

            this.lblFormulaOne = new Infragistics.Win.Misc.UltraLabel();
            this.txtExchangeRate = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.lblBaseCurrencySymbol = new Infragistics.Win.Misc.UltraLabel();

            this.lblCurrencyName = new Infragistics.Win.Misc.UltraLabel();
            this.txtCurrencyName = new Infragistics.Win.UltraWinEditors.UltraTextEditor();

            this.picCurrency = new System.Windows.Forms.PictureBox();
            this.lblImageHint = new Infragistics.Win.Misc.UltraLabel();
            this.contextMenuImage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuAddImage = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDeleteImage = new System.Windows.Forms.ToolStripMenuItem();

            this.pnlMainBackground.SuspendLayout();
            this.pnlNavStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtCurrencyCode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtExchangeRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCurrencyName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCurrency)).BeginInit();
            this.contextMenuImage.SuspendLayout();
            this.SuspendLayout();

            // pnlMainBackground (IRS POS Exact Background Canvas)
            this.pnlMainBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(234)))), ((int)(((byte)(254)))));
            this.pnlMainBackground.Controls.Add(this.lblCurrencyCode);
            this.pnlMainBackground.Controls.Add(this.txtCurrencyCode);
            this.pnlMainBackground.Controls.Add(this.btnLookupF7);
            this.pnlMainBackground.Controls.Add(this.pnlNavStrip);
            this.pnlMainBackground.Controls.Add(this.lblFormulaOne);
            this.pnlMainBackground.Controls.Add(this.txtExchangeRate);
            this.pnlMainBackground.Controls.Add(this.lblBaseCurrencySymbol);
            this.pnlMainBackground.Controls.Add(this.lblCurrencyName);
            this.pnlMainBackground.Controls.Add(this.txtCurrencyName);
            this.pnlMainBackground.Controls.Add(this.picCurrency);
            this.pnlMainBackground.Controls.Add(this.lblImageHint);
            this.pnlMainBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlMainBackground.Name = "pnlMainBackground";
            this.pnlMainBackground.Size = new System.Drawing.Size(650, 480);

            // lblCurrencyCode
            this.lblCurrencyCode.Appearance.FontData.Name = "Segoe UI";
            this.lblCurrencyCode.Appearance.FontData.SizeInPoints = 9F;
            this.lblCurrencyCode.Appearance.ForeColor = System.Drawing.Color.Black;
            this.lblCurrencyCode.Location = new System.Drawing.Point(25, 26);
            this.lblCurrencyCode.Name = "lblCurrencyCode";
            this.lblCurrencyCode.Size = new System.Drawing.Size(100, 23);
            this.lblCurrencyCode.Text = "Currency Code";

            // txtCurrencyCode (IRS POS Signature Peach Fill)
            this.txtCurrencyCode.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(228)))), ((int)(((byte)(208)))));
            this.txtCurrencyCode.Appearance.FontData.Name = "Segoe UI";
            this.txtCurrencyCode.Appearance.FontData.SizeInPoints = 9.5F;
            this.txtCurrencyCode.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.txtCurrencyCode.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(185)))), ((int)(((byte)(215)))));
            this.txtCurrencyCode.Location = new System.Drawing.Point(130, 22);
            this.txtCurrencyCode.Name = "txtCurrencyCode";
            this.txtCurrencyCode.Size = new System.Drawing.Size(120, 24);
            this.txtCurrencyCode.TabIndex = 0;

            // btnLookupF7
            this.btnLookupF7.Appearance = appearanceBtn;
            this.btnLookupF7.HotTrackAppearance = appearanceBtnHot;
            this.btnLookupF7.UseHotTracking = Infragistics.Win.DefaultableBoolean.True;
            this.btnLookupF7.Location = new System.Drawing.Point(258, 22);
            this.btnLookupF7.Name = "btnLookupF7";
            this.btnLookupF7.Size = new System.Drawing.Size(32, 24);
            this.btnLookupF7.TabIndex = 1;
            this.btnLookupF7.Text = "F7";

            // pnlNavStrip (Toolbar Container for Navigation Buttons)
            this.pnlNavStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(222)))), ((int)(((byte)(250)))));
            this.pnlNavStrip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlNavStrip.Controls.Add(this.btnFirst);
            this.pnlNavStrip.Controls.Add(this.btnPrev);
            this.pnlNavStrip.Controls.Add(this.btnNext);
            this.pnlNavStrip.Controls.Add(this.btnLast);
            this.pnlNavStrip.Location = new System.Drawing.Point(296, 22);
            this.pnlNavStrip.Name = "pnlNavStrip";
            this.pnlNavStrip.Size = new System.Drawing.Size(124, 24);
            this.pnlNavStrip.TabIndex = 2;

            // btnFirst
            this.btnFirst.Appearance = appearanceBtn;
            this.btnFirst.HotTrackAppearance = appearanceBtnHot;
            this.btnFirst.UseHotTracking = Infragistics.Win.DefaultableBoolean.True;
            this.btnFirst.Location = new System.Drawing.Point(1, 1);
            this.btnFirst.Name = "btnFirst";
            this.btnFirst.Size = new System.Drawing.Size(28, 20);
            this.btnFirst.TabIndex = 0;
            this.btnFirst.Text = "|<";

            // btnPrev
            this.btnPrev.Appearance = appearanceBtn;
            this.btnPrev.HotTrackAppearance = appearanceBtnHot;
            this.btnPrev.UseHotTracking = Infragistics.Win.DefaultableBoolean.True;
            this.btnPrev.Location = new System.Drawing.Point(31, 1);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(28, 20);
            this.btnPrev.TabIndex = 1;
            this.btnPrev.Text = "<";

            // btnNext
            this.btnNext.Appearance = appearanceBtn;
            this.btnNext.HotTrackAppearance = appearanceBtnHot;
            this.btnNext.UseHotTracking = Infragistics.Win.DefaultableBoolean.True;
            this.btnNext.Location = new System.Drawing.Point(61, 1);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(28, 20);
            this.btnNext.TabIndex = 2;
            this.btnNext.Text = ">";

            // btnLast
            this.btnLast.Appearance = appearanceBtn;
            this.btnLast.HotTrackAppearance = appearanceBtnHot;
            this.btnLast.UseHotTracking = Infragistics.Win.DefaultableBoolean.True;
            this.btnLast.Location = new System.Drawing.Point(91, 1);
            this.btnLast.Name = "btnLast";
            this.btnLast.Size = new System.Drawing.Size(28, 20);
            this.btnLast.TabIndex = 3;
            this.btnLast.Text = ">|";

            // lblFormulaOne
            this.lblFormulaOne.Appearance.FontData.BoldAsString = "True";
            this.lblFormulaOne.Appearance.FontData.Name = "Segoe UI";
            this.lblFormulaOne.Appearance.FontData.SizeInPoints = 9.5F;
            this.lblFormulaOne.Appearance.ForeColor = System.Drawing.Color.Black;
            this.lblFormulaOne.Appearance.TextHAlignAsString = "Right";
            this.lblFormulaOne.Location = new System.Drawing.Point(25, 60);
            this.lblFormulaOne.Name = "lblFormulaOne";
            this.lblFormulaOne.Size = new System.Drawing.Size(100, 23);
            this.lblFormulaOne.Text = "1 unit =";

            // txtExchangeRate (IRS POS Signature Peach Fill)
            this.txtExchangeRate.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(228)))), ((int)(((byte)(208)))));
            this.txtExchangeRate.Appearance.FontData.Name = "Segoe UI";
            this.txtExchangeRate.Appearance.FontData.SizeInPoints = 9.5F;
            this.txtExchangeRate.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.txtExchangeRate.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(185)))), ((int)(((byte)(215)))));
            this.txtExchangeRate.Location = new System.Drawing.Point(130, 56);
            this.txtExchangeRate.Name = "txtExchangeRate";
            this.txtExchangeRate.Size = new System.Drawing.Size(120, 24);
            this.txtExchangeRate.TabIndex = 3;
            this.txtExchangeRate.Text = "1.0000";

            // lblBaseCurrencySymbol
            this.lblBaseCurrencySymbol.Appearance.FontData.BoldAsString = "True";
            this.lblBaseCurrencySymbol.Appearance.FontData.Name = "Segoe UI";
            this.lblBaseCurrencySymbol.Appearance.FontData.SizeInPoints = 9.5F;
            this.lblBaseCurrencySymbol.Appearance.ForeColor = System.Drawing.Color.Black;
            this.lblBaseCurrencySymbol.Location = new System.Drawing.Point(258, 60);
            this.lblBaseCurrencySymbol.Name = "lblBaseCurrencySymbol";
            this.lblBaseCurrencySymbol.Size = new System.Drawing.Size(60, 23);
            this.lblBaseCurrencySymbol.Text = "₹";

            // lblCurrencyName
            this.lblCurrencyName.Appearance.FontData.Name = "Segoe UI";
            this.lblCurrencyName.Appearance.FontData.SizeInPoints = 9F;
            this.lblCurrencyName.Appearance.ForeColor = System.Drawing.Color.Black;
            this.lblCurrencyName.Location = new System.Drawing.Point(25, 94);
            this.lblCurrencyName.Name = "lblCurrencyName";
            this.lblCurrencyName.Size = new System.Drawing.Size(100, 23);
            this.lblCurrencyName.Text = "Currency Name";

            // txtCurrencyName
            this.txtCurrencyName.Appearance.BackColor = System.Drawing.Color.White;
            this.txtCurrencyName.Appearance.FontData.Name = "Segoe UI";
            this.txtCurrencyName.Appearance.FontData.SizeInPoints = 9.5F;
            this.txtCurrencyName.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.txtCurrencyName.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(185)))), ((int)(((byte)(215)))));
            this.txtCurrencyName.Location = new System.Drawing.Point(130, 90);
            this.txtCurrencyName.Name = "txtCurrencyName";
            this.txtCurrencyName.Size = new System.Drawing.Size(290, 24);
            this.txtCurrencyName.TabIndex = 4;

            // PictureBox Image Container (IRS POS Classic Picture Box)
            this.picCurrency.BackColor = System.Drawing.Color.White;
            this.picCurrency.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picCurrency.ContextMenuStrip = this.contextMenuImage;
            this.picCurrency.Location = new System.Drawing.Point(130, 126);
            this.picCurrency.Name = "picCurrency";
            this.picCurrency.Size = new System.Drawing.Size(220, 175);
            this.picCurrency.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picCurrency.TabIndex = 5;
            this.picCurrency.TabStop = false;

            // lblImageHint
            this.lblImageHint.Appearance.FontData.Name = "Segoe UI";
            this.lblImageHint.Appearance.FontData.SizeInPoints = 8.5F;
            this.lblImageHint.Appearance.ForeColor = System.Drawing.Color.Black;
            this.lblImageHint.Location = new System.Drawing.Point(125, 306);
            this.lblImageHint.Name = "lblImageHint";
            this.lblImageHint.Size = new System.Drawing.Size(270, 22);
            this.lblImageHint.Text = "(Right Click On Image To Add/Delete Image)";

            // contextMenuImage
            this.menuAddImage.Text = "Add Image...";
            this.menuDeleteImage.Text = "Delete Image";
            this.contextMenuImage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuAddImage,
            this.menuDeleteImage});

            // FrmCurrency Form Settings
            this.ClientSize = new System.Drawing.Size(650, 480);
            this.Controls.Add(this.pnlMainBackground);
            this.Name = "FrmCurrency";
            this.Text = "Currency";
            this.Load += new System.EventHandler(this.FrmCurrency_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmCurrency_KeyDown);

            this.pnlMainBackground.ResumeLayout(false);
            this.pnlNavStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtCurrencyCode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtExchangeRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCurrencyName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCurrency)).EndInit();
            this.contextMenuImage.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlMainBackground;
        private Infragistics.Win.Misc.UltraLabel lblCurrencyCode;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtCurrencyCode;

        private Infragistics.Win.Misc.UltraButton btnLookupF7;
        private System.Windows.Forms.Panel pnlNavStrip;
        private Infragistics.Win.Misc.UltraButton btnFirst;
        private Infragistics.Win.Misc.UltraButton btnPrev;
        private Infragistics.Win.Misc.UltraButton btnNext;
        private Infragistics.Win.Misc.UltraButton btnLast;

        private Infragistics.Win.Misc.UltraLabel lblFormulaOne;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtExchangeRate;
        private Infragistics.Win.Misc.UltraLabel lblBaseCurrencySymbol;

        private Infragistics.Win.Misc.UltraLabel lblCurrencyName;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtCurrencyName;

        private System.Windows.Forms.PictureBox picCurrency;
        private Infragistics.Win.Misc.UltraLabel lblImageHint;
        private System.Windows.Forms.ContextMenuStrip contextMenuImage;
        private System.Windows.Forms.ToolStripMenuItem menuAddImage;
        private System.Windows.Forms.ToolStripMenuItem menuDeleteImage;
    }
}
