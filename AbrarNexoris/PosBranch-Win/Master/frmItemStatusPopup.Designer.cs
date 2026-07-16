namespace PosBranch_Win.Master
{
    partial class frmItemStatusPopup
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
            this.label48 = new System.Windows.Forms.Label();
            this.cmbItemStatus = new System.Windows.Forms.ComboBox();
            this.lblItemStatusReason = new System.Windows.Forms.Label();
            this.txtItemStatusReason = new System.Windows.Forms.TextBox();
            this.label47 = new System.Windows.Forms.Label();
            this.dtpItemStatusDate = new System.Windows.Forms.DateTimePicker();
            this.lblItemStatusSaleRule = new System.Windows.Forms.Label();
            this.lblItemStatusPurchaseRule = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label48.Location = new System.Drawing.Point(14, 18);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(49, 16);
            this.label48.TabIndex = 0;
            this.label48.Text = "Status";
            // 
            // cmbItemStatus
            // 
            this.cmbItemStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbItemStatus.Font = new System.Drawing.Font("Microsoft PhagsPa", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbItemStatus.FormattingEnabled = true;
            this.cmbItemStatus.Location = new System.Drawing.Point(78, 12);
            this.cmbItemStatus.Name = "cmbItemStatus";
            this.cmbItemStatus.Size = new System.Drawing.Size(208, 27);
            this.cmbItemStatus.TabIndex = 1;
            this.cmbItemStatus.SelectedIndexChanged += new System.EventHandler(this.cmbItemStatus_SelectedIndexChanged);
            // 
            // lblItemStatusReason
            // 
            this.lblItemStatusReason.AutoSize = true;
            this.lblItemStatusReason.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItemStatusReason.Location = new System.Drawing.Point(14, 57);
            this.lblItemStatusReason.Name = "lblItemStatusReason";
            this.lblItemStatusReason.Size = new System.Drawing.Size(52, 16);
            this.lblItemStatusReason.TabIndex = 2;
            this.lblItemStatusReason.Text = "Reason";
            // 
            // txtItemStatusReason
            // 
            this.txtItemStatusReason.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtItemStatusReason.Font = new System.Drawing.Font("Microsoft PhagsPa", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtItemStatusReason.Location = new System.Drawing.Point(78, 51);
            this.txtItemStatusReason.MaxLength = 500;
            this.txtItemStatusReason.Name = "txtItemStatusReason";
            this.txtItemStatusReason.Size = new System.Drawing.Size(381, 26);
            this.txtItemStatusReason.TabIndex = 3;
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label47.Location = new System.Drawing.Point(14, 95);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(33, 16);
            this.label47.TabIndex = 4;
            this.label47.Text = "Date";
            // 
            // dtpItemStatusDate
            // 
            this.dtpItemStatusDate.CustomFormat = "dd/MM/yyyy";
            this.dtpItemStatusDate.Font = new System.Drawing.Font("Microsoft PhagsPa", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpItemStatusDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpItemStatusDate.Location = new System.Drawing.Point(78, 89);
            this.dtpItemStatusDate.Name = "dtpItemStatusDate";
            this.dtpItemStatusDate.Size = new System.Drawing.Size(122, 25);
            this.dtpItemStatusDate.TabIndex = 5;
            // 
            // lblItemStatusSaleRule
            // 
            this.lblItemStatusSaleRule.AutoSize = true;
            this.lblItemStatusSaleRule.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItemStatusSaleRule.Location = new System.Drawing.Point(222, 94);
            this.lblItemStatusSaleRule.Name = "lblItemStatusSaleRule";
            this.lblItemStatusSaleRule.Size = new System.Drawing.Size(92, 16);
            this.lblItemStatusSaleRule.TabIndex = 6;
            this.lblItemStatusSaleRule.Text = "Sale: Allowed";
            // 
            // lblItemStatusPurchaseRule
            // 
            this.lblItemStatusPurchaseRule.AutoSize = true;
            this.lblItemStatusPurchaseRule.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItemStatusPurchaseRule.Location = new System.Drawing.Point(332, 94);
            this.lblItemStatusPurchaseRule.Name = "lblItemStatusPurchaseRule";
            this.lblItemStatusPurchaseRule.Size = new System.Drawing.Size(122, 16);
            this.lblItemStatusPurchaseRule.TabIndex = 7;
            this.lblItemStatusPurchaseRule.Text = "Purchase: Allowed";
            // 
            // btnOk
            // 
            this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(239)))));
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.Location = new System.Drawing.Point(282, 130);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(84, 30);
            this.btnOk.TabIndex = 8;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Location = new System.Drawing.Point(375, 130);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(84, 30);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // frmItemStatusPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 173);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lblItemStatusPurchaseRule);
            this.Controls.Add(this.lblItemStatusSaleRule);
            this.Controls.Add(this.dtpItemStatusDate);
            this.Controls.Add(this.label47);
            this.Controls.Add(this.txtItemStatusReason);
            this.Controls.Add(this.lblItemStatusReason);
            this.Controls.Add(this.cmbItemStatus);
            this.Controls.Add(this.label48);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmItemStatusPopup";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Item Status";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.ComboBox cmbItemStatus;
        private System.Windows.Forms.Label lblItemStatusReason;
        private System.Windows.Forms.TextBox txtItemStatusReason;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.DateTimePicker dtpItemStatusDate;
        private System.Windows.Forms.Label lblItemStatusSaleRule;
        private System.Windows.Forms.Label lblItemStatusPurchaseRule;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
