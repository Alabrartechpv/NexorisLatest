
namespace PosBranch_Win.Transaction
{
    partial class frmSalesReturnItem
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.ultraGrid1 = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGrid1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ultraGrid1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(876, 487);
            this.panel1.TabIndex = 0;
            // 
            // ultraGrid1
            // 
            appearance1.BackColor = System.Drawing.Color.Green;
            appearance1.BackColor2 = System.Drawing.Color.PaleGreen;
            appearance1.BackGradientStyle = Infragistics.Win.GradientStyle.BackwardDiagonal;
            this.ultraGrid1.DisplayLayout.Appearance = appearance1;
            this.ultraGrid1.DisplayLayout.InterBandSpacing = 10;
            this.ultraGrid1.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.None;
            this.ultraGrid1.DisplayLayout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = Infragistics.Win.UIElementBorderStyle.Solid;
            appearance2.BackColor = System.Drawing.Color.Transparent;
            this.ultraGrid1.DisplayLayout.Override.CardAreaAppearance = appearance2;
            appearance3.BackColor = System.Drawing.Color.PaleGreen;
            appearance3.BackColor2 = System.Drawing.Color.LimeGreen;
            appearance3.BackGradientAlignment = Infragistics.Win.GradientAlignment.Client;
            appearance3.BackGradientStyle = Infragistics.Win.GradientStyle.BackwardDiagonal;
            this.ultraGrid1.DisplayLayout.Override.CellAppearance = appearance3;
            this.ultraGrid1.DisplayLayout.Override.CellSpacing = 3;
            appearance4.BackColor = System.Drawing.Color.LightGreen;
            appearance4.FontData.Name = "Verdana";
            appearance4.ForeColor = System.Drawing.Color.DarkGreen;
            appearance4.TextHAlignAsString = "Left";
            appearance4.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;
            this.ultraGrid1.DisplayLayout.Override.HeaderAppearance = appearance4;
            appearance5.BackColor = System.Drawing.Color.Transparent;
            this.ultraGrid1.DisplayLayout.Override.RowAppearance = appearance5;
            appearance6.BackColor = System.Drawing.Color.ForestGreen;
            this.ultraGrid1.DisplayLayout.Override.RowSelectorAppearance = appearance6;
            this.ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 10;
            this.ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 3;
            this.ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 2;
            appearance7.BackColor = System.Drawing.Color.LimeGreen;
            appearance7.BackColor2 = System.Drawing.Color.PaleGreen;
            appearance7.BackGradientStyle = Infragistics.Win.GradientStyle.VerticalBump;
            this.ultraGrid1.DisplayLayout.Override.SelectedRowAppearance = appearance7;
            this.ultraGrid1.DisplayLayout.RowConnectorStyle = Infragistics.Win.UltraWinGrid.RowConnectorStyle.None;
            this.ultraGrid1.Location = new System.Drawing.Point(8, 67);
            this.ultraGrid1.Name = "ultraGrid1";
            this.ultraGrid1.Size = new System.Drawing.Size(861, 408);
            this.ultraGrid1.TabIndex = 2;
            this.ultraGrid1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ultraGrid1_KeyPress);
            // 
            // frmSalesReturnItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightBlue;
            this.ClientSize = new System.Drawing.Size(876, 487);
            this.Controls.Add(this.panel1);
            this.Name = "frmSalesReturnItem";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SalesReturnItem";
            this.Load += new System.EventHandler(this.SalesReturnItem_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmSalesReturnItem_KeyDown);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGrid1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        public Infragistics.Win.UltraWinGrid.UltraGrid ultraGrid1;
    }
}