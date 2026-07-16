
namespace PosBranch_Win.DialogBox
{
    partial class FrmPurchaseItemDialog
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
            this.DgvItem = new System.Windows.Forms.DataGridView();
            this.lblItems = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.DgvItem)).BeginInit();
            this.SuspendLayout();
            // 
            // DgvItem
            // 
            this.DgvItem.AllowUserToAddRows = false;
            this.DgvItem.AllowUserToDeleteRows = false;
            this.DgvItem.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DgvItem.Location = new System.Drawing.Point(108, 101);
            this.DgvItem.MultiSelect = false;
            this.DgvItem.Name = "DgvItem";
            this.DgvItem.ReadOnly = true;
            this.DgvItem.Size = new System.Drawing.Size(745, 319);
            this.DgvItem.TabIndex = 0;
    
            this.DgvItem.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DgvItem_KeyDown);
     
            // 
            // lblItems
            // 
            this.lblItems.AutoSize = true;
            this.lblItems.Location = new System.Drawing.Point(116, 44);
            this.lblItems.Name = "lblItems";
            this.lblItems.Size = new System.Drawing.Size(81, 13);
            this.lblItems.TabIndex = 1;
            this.lblItems.Text = "ITEM MASTER";
            // 
            // FrmPurchaseItemDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(905, 473);
            this.Controls.Add(this.lblItems);
            this.Controls.Add(this.DgvItem);
            this.KeyPreview = true;
            this.Name = "FrmPurchaseItemDialog";
            this.Text = "FrmPurchaseItemDialog";
            this.Load += new System.EventHandler(this.FrmPurchaseItemDialog_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmPurchaseItemDialog_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.DgvItem)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblItems;
        public System.Windows.Forms.DataGridView DgvItem;
    }
}