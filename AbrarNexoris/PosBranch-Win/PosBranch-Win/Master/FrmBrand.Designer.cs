
namespace PosBranch_Win.Master
{
    partial class FrmBrand
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.txtBrandSearch = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblListBrand = new System.Windows.Forms.Label();
            this.dataGridViewBrand = new System.Windows.Forms.DataGridView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pictureBoxBrand = new System.Windows.Forms.PictureBox();
            this.btnUpload = new System.Windows.Forms.Button();
            this.lblImage = new System.Windows.Forms.Label();
            this.lblAddBrand = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.brnClear = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtBrandName = new System.Windows.Forms.TextBox();
            this.lblBrand = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBrand)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBrand)).BeginInit();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightBlue;
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(864, 482);
            this.panel1.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.BackColor = System.Drawing.Color.LightBlue;
            this.panel3.Controls.Add(this.txtBrandSearch);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.lblListBrand);
            this.panel3.Controls.Add(this.dataGridViewBrand);
            this.panel3.Location = new System.Drawing.Point(28, 225);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(794, 237);
            this.panel3.TabIndex = 1;
            // 
            // txtBrandSearch
            // 
            this.txtBrandSearch.Location = new System.Drawing.Point(260, 34);
            this.txtBrandSearch.Name = "txtBrandSearch";
            this.txtBrandSearch.Size = new System.Drawing.Size(185, 20);
            this.txtBrandSearch.TabIndex = 3;
            this.txtBrandSearch.TextChanged += new System.EventHandler(this.txtBrandSearch_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(181, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "BrandSearch";
            // 
            // lblListBrand
            // 
            this.lblListBrand.AutoSize = true;
            this.lblListBrand.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblListBrand.Location = new System.Drawing.Point(317, 6);
            this.lblListBrand.Name = "lblListBrand";
            this.lblListBrand.Size = new System.Drawing.Size(99, 17);
            this.lblListBrand.TabIndex = 1;
            this.lblListBrand.Text = "LIST BRAND";
            // 
            // dataGridViewBrand
            // 
            this.dataGridViewBrand.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewBrand.Location = new System.Drawing.Point(126, 60);
            this.dataGridViewBrand.Name = "dataGridViewBrand";
            this.dataGridViewBrand.Size = new System.Drawing.Size(521, 144);
            this.dataGridViewBrand.TabIndex = 0;
            this.dataGridViewBrand.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewBrand_CellContentClick);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BackColor = System.Drawing.Color.LightBlue;
            this.panel2.Controls.Add(this.pictureBoxBrand);
            this.panel2.Controls.Add(this.btnUpload);
            this.panel2.Controls.Add(this.lblImage);
            this.panel2.Controls.Add(this.lblAddBrand);
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Controls.Add(this.txtBrandName);
            this.panel2.Controls.Add(this.lblBrand);
            this.panel2.Location = new System.Drawing.Point(28, 13);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(794, 204);
            this.panel2.TabIndex = 0;
            // 
            // pictureBoxBrand
            // 
            this.pictureBoxBrand.Location = new System.Drawing.Point(577, 12);
            this.pictureBoxBrand.Name = "pictureBoxBrand";
            this.pictureBoxBrand.Size = new System.Drawing.Size(183, 156);
            this.pictureBoxBrand.TabIndex = 6;
            this.pictureBoxBrand.TabStop = false;
            // 
            // btnUpload
            // 
            this.btnUpload.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnUpload.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnUpload.Location = new System.Drawing.Point(414, 54);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(103, 36);
            this.btnUpload.TabIndex = 5;
            this.btnUpload.Text = "Upload";
            this.btnUpload.UseVisualStyleBackColor = false;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // lblImage
            // 
            this.lblImage.AutoSize = true;
            this.lblImage.Location = new System.Drawing.Point(372, 65);
            this.lblImage.Name = "lblImage";
            this.lblImage.Size = new System.Drawing.Size(36, 13);
            this.lblImage.TabIndex = 4;
            this.lblImage.Text = "Image";
            // 
            // lblAddBrand
            // 
            this.lblAddBrand.AutoSize = true;
            this.lblAddBrand.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAddBrand.Location = new System.Drawing.Point(318, 12);
            this.lblAddBrand.Name = "lblAddBrand";
            this.lblAddBrand.Size = new System.Drawing.Size(98, 17);
            this.lblAddBrand.TabIndex = 3;
            this.lblAddBrand.Text = "ADD BRAND";
            // 
            // panel4
            // 
            this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.panel4.Controls.Add(this.btnUpdate);
            this.panel4.Controls.Add(this.btnDelete);
            this.panel4.Controls.Add(this.brnClear);
            this.panel4.Controls.Add(this.btnSave);
            this.panel4.Location = new System.Drawing.Point(161, 108);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(410, 60);
            this.panel4.TabIndex = 2;
            // 
            // btnUpdate
            // 
            this.btnUpdate.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnUpdate.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnUpdate.Location = new System.Drawing.Point(23, 11);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(103, 36);
            this.btnUpdate.TabIndex = 3;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnDelete.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnDelete.Location = new System.Drawing.Point(253, 11);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(103, 36);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // brnClear
            // 
            this.brnClear.BackColor = System.Drawing.SystemColors.HotTrack;
            this.brnClear.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.brnClear.Location = new System.Drawing.Point(144, 11);
            this.brnClear.Name = "brnClear";
            this.brnClear.Size = new System.Drawing.Size(103, 36);
            this.brnClear.TabIndex = 1;
            this.brnClear.Text = "Clear";
            this.brnClear.UseVisualStyleBackColor = false;
            this.brnClear.Click += new System.EventHandler(this.brnClear_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnSave.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnSave.Location = new System.Drawing.Point(23, 11);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(103, 36);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // txtBrandName
            // 
            this.txtBrandName.Location = new System.Drawing.Point(161, 59);
            this.txtBrandName.Name = "txtBrandName";
            this.txtBrandName.Size = new System.Drawing.Size(185, 20);
            this.txtBrandName.TabIndex = 1;
            // 
            // lblBrand
            // 
            this.lblBrand.AutoSize = true;
            this.lblBrand.Location = new System.Drawing.Point(75, 59);
            this.lblBrand.Name = "lblBrand";
            this.lblBrand.Size = new System.Drawing.Size(66, 13);
            this.lblBrand.TabIndex = 0;
            this.lblBrand.Text = "Brand Name";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // FrmBrand
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(864, 482);
            this.Controls.Add(this.panel1);
            this.KeyPreview = true;
            this.Name = "FrmBrand";
            this.Text = "FrmBrand";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmBrand_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmBrand_KeyDown);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBrand)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBrand)).EndInit();
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox txtBrandName;
        private System.Windows.Forms.Label lblBrand;
        private System.Windows.Forms.DataGridView dataGridViewBrand;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button brnClear;
        private System.Windows.Forms.Label lblListBrand;
        private System.Windows.Forms.Label lblAddBrand;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.TextBox txtBrandSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblImage;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.PictureBox pictureBoxBrand;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}