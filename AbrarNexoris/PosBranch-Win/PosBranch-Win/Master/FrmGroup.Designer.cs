
namespace PosBranch_Win.Master
{
    partial class FrmGroup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        //
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
            this.pnl_listgroup = new System.Windows.Forms.Panel();
            this.dgv_listgroup = new System.Windows.Forms.DataGridView();
            this.lbl_listgroup = new System.Windows.Forms.Label();
            this.pnl_addgroup = new System.Windows.Forms.Panel();
            this.picb_uploadfile = new System.Windows.Forms.PictureBox();
            this.pnl_btngroup = new System.Windows.Forms.Panel();
            this.btn_update = new System.Windows.Forms.Button();
            this.btn_delete = new System.Windows.Forms.Button();
            this.btn_clear = new System.Windows.Forms.Button();
            this.btn_save = new System.Windows.Forms.Button();
            this.btn_upload = new System.Windows.Forms.Button();
            this.txt_gname = new System.Windows.Forms.TextBox();
            this.lbl_gname = new System.Windows.Forms.Label();
            this.lbl_addgroup = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.panel1.SuspendLayout();
            this.pnl_listgroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_listgroup)).BeginInit();
            this.pnl_addgroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picb_uploadfile)).BeginInit();
            this.pnl_btngroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pnl_listgroup);
            this.panel1.Controls.Add(this.pnl_addgroup);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 450);
            this.panel1.TabIndex = 0;
            // 
            // pnl_listgroup
            // 
            this.pnl_listgroup.BackColor = System.Drawing.Color.LightBlue;
            this.pnl_listgroup.Controls.Add(this.dgv_listgroup);
            this.pnl_listgroup.Controls.Add(this.lbl_listgroup);
            this.pnl_listgroup.Location = new System.Drawing.Point(12, 233);
            this.pnl_listgroup.Name = "pnl_listgroup";
            this.pnl_listgroup.Size = new System.Drawing.Size(776, 205);
            this.pnl_listgroup.TabIndex = 1;
            // 
            // dgv_listgroup
            // 
            this.dgv_listgroup.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_listgroup.Location = new System.Drawing.Point(48, 55);
            this.dgv_listgroup.Name = "dgv_listgroup";
            this.dgv_listgroup.Size = new System.Drawing.Size(681, 131);
            this.dgv_listgroup.TabIndex = 3;
            this.dgv_listgroup.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_listgroup_CellClick);
            // 
            // lbl_listgroup
            // 
            this.lbl_listgroup.AutoSize = true;
            this.lbl_listgroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_listgroup.Location = new System.Drawing.Point(313, 21);
            this.lbl_listgroup.Name = "lbl_listgroup";
            this.lbl_listgroup.Size = new System.Drawing.Size(93, 20);
            this.lbl_listgroup.TabIndex = 1;
            this.lbl_listgroup.Text = "List Group";
            // 
            // pnl_addgroup
            // 
            this.pnl_addgroup.BackColor = System.Drawing.Color.LightBlue;
            this.pnl_addgroup.Controls.Add(this.picb_uploadfile);
            this.pnl_addgroup.Controls.Add(this.pnl_btngroup);
            this.pnl_addgroup.Controls.Add(this.btn_upload);
            this.pnl_addgroup.Controls.Add(this.txt_gname);
            this.pnl_addgroup.Controls.Add(this.lbl_gname);
            this.pnl_addgroup.Controls.Add(this.lbl_addgroup);
            this.pnl_addgroup.Location = new System.Drawing.Point(12, 12);
            this.pnl_addgroup.Name = "pnl_addgroup";
            this.pnl_addgroup.Size = new System.Drawing.Size(776, 215);
            this.pnl_addgroup.TabIndex = 0;
            // 
            // picb_uploadfile
            // 
            this.picb_uploadfile.Location = new System.Drawing.Point(516, 49);
            this.picb_uploadfile.Name = "picb_uploadfile";
            this.picb_uploadfile.Size = new System.Drawing.Size(108, 50);
            this.picb_uploadfile.TabIndex = 12;
            this.picb_uploadfile.TabStop = false;
            // 
            // pnl_btngroup
            // 
            this.pnl_btngroup.Controls.Add(this.btn_update);
            this.pnl_btngroup.Controls.Add(this.btn_delete);
            this.pnl_btngroup.Controls.Add(this.btn_clear);
            this.pnl_btngroup.Controls.Add(this.btn_save);
            this.pnl_btngroup.Location = new System.Drawing.Point(223, 115);
            this.pnl_btngroup.Name = "pnl_btngroup";
            this.pnl_btngroup.Size = new System.Drawing.Size(288, 88);
            this.pnl_btngroup.TabIndex = 11;
            // 
            // btn_update
            // 
            this.btn_update.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btn_update.ForeColor = System.Drawing.Color.White;
            this.btn_update.Location = new System.Drawing.Point(26, 29);
            this.btn_update.Name = "btn_update";
            this.btn_update.Size = new System.Drawing.Size(78, 32);
            this.btn_update.TabIndex = 11;
            this.btn_update.Text = "Update";
            this.btn_update.UseVisualStyleBackColor = false;
            this.btn_update.Click += new System.EventHandler(this.btn_update_Click);
            // 
            // btn_delete
            // 
            this.btn_delete.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btn_delete.ForeColor = System.Drawing.Color.White;
            this.btn_delete.Location = new System.Drawing.Point(188, 30);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(78, 32);
            this.btn_delete.TabIndex = 10;
            this.btn_delete.Text = "Delete";
            this.btn_delete.UseVisualStyleBackColor = false;
            // 
            // btn_clear
            // 
            this.btn_clear.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btn_clear.ForeColor = System.Drawing.Color.White;
            this.btn_clear.Location = new System.Drawing.Point(107, 30);
            this.btn_clear.Name = "btn_clear";
            this.btn_clear.Size = new System.Drawing.Size(78, 32);
            this.btn_clear.TabIndex = 9;
            this.btn_clear.Text = "Clear";
            this.btn_clear.UseVisualStyleBackColor = false;
            this.btn_clear.Click += new System.EventHandler(this.btn_clear_Click);
            // 
            // btn_save
            // 
            this.btn_save.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btn_save.ForeColor = System.Drawing.Color.White;
            this.btn_save.Location = new System.Drawing.Point(26, 29);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(78, 32);
            this.btn_save.TabIndex = 8;
            this.btn_save.Text = "Save";
            this.btn_save.UseVisualStyleBackColor = false;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // btn_upload
            // 
            this.btn_upload.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btn_upload.Location = new System.Drawing.Point(424, 62);
            this.btn_upload.Name = "btn_upload";
            this.btn_upload.Size = new System.Drawing.Size(75, 23);
            this.btn_upload.TabIndex = 3;
            this.btn_upload.Text = "Upload File";
            this.btn_upload.UseVisualStyleBackColor = false;
            this.btn_upload.Click += new System.EventHandler(this.btn_upload_Click);
            // 
            // txt_gname
            // 
            this.txt_gname.Location = new System.Drawing.Point(210, 65);
            this.txt_gname.Multiline = true;
            this.txt_gname.Name = "txt_gname";
            this.txt_gname.Size = new System.Drawing.Size(117, 20);
            this.txt_gname.TabIndex = 2;
            // 
            // lbl_gname
            // 
            this.lbl_gname.AutoSize = true;
            this.lbl_gname.Location = new System.Drawing.Point(155, 68);
            this.lbl_gname.Name = "lbl_gname";
            this.lbl_gname.Size = new System.Drawing.Size(35, 13);
            this.lbl_gname.TabIndex = 1;
            this.lbl_gname.Text = "Name";
            // 
            // lbl_addgroup
            // 
            this.lbl_addgroup.AutoSize = true;
            this.lbl_addgroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_addgroup.Location = new System.Drawing.Point(313, 18);
            this.lbl_addgroup.Name = "lbl_addgroup";
            this.lbl_addgroup.Size = new System.Drawing.Size(96, 20);
            this.lbl_addgroup.TabIndex = 0;
            this.lbl_addgroup.Text = "Add Group";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // FrmGroup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panel1);
            this.Name = "FrmGroup";
            this.Text = "FrmGroup";
            this.Load += new System.EventHandler(this.FrmGroup_Load);
            this.panel1.ResumeLayout(false);
            this.pnl_listgroup.ResumeLayout(false);
            this.pnl_listgroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_listgroup)).EndInit();
            this.pnl_addgroup.ResumeLayout(false);
            this.pnl_addgroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picb_uploadfile)).EndInit();
            this.pnl_btngroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel pnl_listgroup;
        private System.Windows.Forms.Label lbl_listgroup;
        private System.Windows.Forms.Panel pnl_addgroup;
        private System.Windows.Forms.Button btn_upload;
        private System.Windows.Forms.TextBox txt_gname;
        private System.Windows.Forms.Label lbl_gname;
        private System.Windows.Forms.Label lbl_addgroup;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Panel pnl_btngroup;
        private System.Windows.Forms.Button btn_delete;
        private System.Windows.Forms.Button btn_clear;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.PictureBox picb_uploadfile;
        private System.Windows.Forms.DataGridView dgv_listgroup;
        private System.Windows.Forms.Button btn_update;
    }
}