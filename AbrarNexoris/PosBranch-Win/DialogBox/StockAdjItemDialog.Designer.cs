
namespace PosBranch_Win.DialogBox
{
    partial class StockAdjItemDialog
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
            this.lblItems = new System.Windows.Forms.Label();
            this.dgv_Item = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Item)).BeginInit();
            this.SuspendLayout();
            // 
            // lblItems
            // 
            this.lblItems.AutoSize = true;
            this.lblItems.Location = new System.Drawing.Point(25, 28);
            this.lblItems.Name = "lblItems";
            this.lblItems.Size = new System.Drawing.Size(81, 13);
            this.lblItems.TabIndex = 2;
            this.lblItems.Text = "ITEM MASTER";
            // 
            // dgv_Item
            // 
            this.dgv_Item.AllowUserToAddRows = false;
            this.dgv_Item.AllowUserToDeleteRows = false;
            this.dgv_Item.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Item.Location = new System.Drawing.Point(28, 66);
            this.dgv_Item.MultiSelect = false;
            this.dgv_Item.Name = "dgv_Item";
            this.dgv_Item.ReadOnly = true;
            this.dgv_Item.Size = new System.Drawing.Size(745, 319);
            this.dgv_Item.TabIndex = 3;
            this.dgv_Item.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgv_Item_KeyDown);
            // 
            // StockAdjItemDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgv_Item);
            this.Controls.Add(this.lblItems);
            this.Name = "StockAdjItemDialog";
            this.Text = "StockAdjItemDialog";
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Item)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblItems;
        public System.Windows.Forms.DataGridView dgv_Item;
    }
}