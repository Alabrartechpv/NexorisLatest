
namespace PosBranch_Win.Master
{
    partial class FrmCategory
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
            this.pnl_form = new System.Windows.Forms.Panel();
            this.pnl_listcategory = new System.Windows.Forms.Panel();
            this.dgv_listcategory = new System.Windows.Forms.DataGridView();
            this.lbl_listcategory = new System.Windows.Forms.Label();
            this.pnl_addcategory = new System.Windows.Forms.Panel();
            this.cb_groupname = new System.Windows.Forms.ComboBox();
            this.pnl_btngroup = new System.Windows.Forms.Panel();
            this.btn_update = new System.Windows.Forms.Button();
            this.btn_close = new System.Windows.Forms.Button();
            this.btn_clear = new System.Windows.Forms.Button();
            this.btn_save = new System.Windows.Forms.Button();
            this.picbx_uploadfile = new System.Windows.Forms.PictureBox();
            this.btn_uploadfile = new System.Windows.Forms.Button();
            this.lbl_group = new System.Windows.Forms.Label();
            this.txt_categoryname = new System.Windows.Forms.TextBox();
            this.lbl_name = new System.Windows.Forms.Label();
            this.lbl_addcategory = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.pnl_form.SuspendLayout();
            this.pnl_listcategory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_listcategory)).BeginInit();
            this.pnl_addcategory.SuspendLayout();
            this.pnl_btngroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picbx_uploadfile)).BeginInit();
            this.SuspendLayout();
            // 
            // pnl_form
            // 
            this.pnl_form.Controls.Add(this.pnl_listcategory);
            this.pnl_form.Controls.Add(this.pnl_addcategory);
            this.pnl_form.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl_form.Location = new System.Drawing.Point(0, 0);
            this.pnl_form.Name = "pnl_form";
            this.pnl_form.Size = new System.Drawing.Size(800, 450);
            this.pnl_form.TabIndex = 0;
            // 
            // pnl_listcategory
            // 
            this.pnl_listcategory.BackColor = System.Drawing.Color.LightBlue;
            this.pnl_listcategory.Controls.Add(this.dgv_listcategory);
            this.pnl_listcategory.Controls.Add(this.lbl_listcategory);
            this.pnl_listcategory.Location = new System.Drawing.Point(10, 231);
            this.pnl_listcategory.Name = "pnl_listcategory";
            this.pnl_listcategory.Size = new System.Drawing.Size(778, 207);
            this.pnl_listcategory.TabIndex = 1;
            // 
            // dgv_listcategory
            // 
            this.dgv_listcategory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_listcategory.Location = new System.Drawing.Point(52, 68);
            this.dgv_listcategory.Name = "dgv_listcategory";
            this.dgv_listcategory.Size = new System.Drawing.Size(681, 121);
            this.dgv_listcategory.TabIndex = 2;
            this.dgv_listcategory.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_listcategory_CellClick);
            // 
            // lbl_listcategory
            // 
            this.lbl_listcategory.AutoSize = true;
            this.lbl_listcategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_listcategory.Location = new System.Drawing.Point(313, 32);
            this.lbl_listcategory.Name = "lbl_listcategory";
            this.lbl_listcategory.Size = new System.Drawing.Size(115, 20);
            this.lbl_listcategory.TabIndex = 1;
            this.lbl_listcategory.Text = "List Category";
            // 
            // pnl_addcategory
            // 
            this.pnl_addcategory.BackColor = System.Drawing.Color.LightBlue;
            this.pnl_addcategory.Controls.Add(this.cb_groupname);
            this.pnl_addcategory.Controls.Add(this.pnl_btngroup);
            this.pnl_addcategory.Controls.Add(this.picbx_uploadfile);
            this.pnl_addcategory.Controls.Add(this.btn_uploadfile);
            this.pnl_addcategory.Controls.Add(this.lbl_group);
            this.pnl_addcategory.Controls.Add(this.txt_categoryname);
            this.pnl_addcategory.Controls.Add(this.lbl_name);
            this.pnl_addcategory.Controls.Add(this.lbl_addcategory);
            this.pnl_addcategory.Location = new System.Drawing.Point(10, 10);
            this.pnl_addcategory.Name = "pnl_addcategory";
            this.pnl_addcategory.Size = new System.Drawing.Size(778, 218);
            this.pnl_addcategory.TabIndex = 0;
            // 
            // cb_groupname
            // 
            this.cb_groupname.FormattingEnabled = true;
            this.cb_groupname.Location = new System.Drawing.Point(333, 73);
            this.cb_groupname.Name = "cb_groupname";
            this.cb_groupname.Size = new System.Drawing.Size(150, 21);
            this.cb_groupname.TabIndex = 11;
            // 
            // pnl_btngroup
            // 
            this.pnl_btngroup.Controls.Add(this.btn_update);
            this.pnl_btngroup.Controls.Add(this.btn_close);
            this.pnl_btngroup.Controls.Add(this.btn_clear);
            this.pnl_btngroup.Controls.Add(this.btn_save);
            this.pnl_btngroup.Location = new System.Drawing.Point(242, 127);
            this.pnl_btngroup.Name = "pnl_btngroup";
            this.pnl_btngroup.Size = new System.Drawing.Size(288, 88);
            this.pnl_btngroup.TabIndex = 10;
            // 
            // btn_update
            // 
            this.btn_update.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btn_update.Location = new System.Drawing.Point(26, 29);
            this.btn_update.Name = "btn_update";
            this.btn_update.Size = new System.Drawing.Size(78, 32);
            this.btn_update.TabIndex = 11;
            this.btn_update.Text = "Update";
            this.btn_update.UseVisualStyleBackColor = false;
            this.btn_update.Click += new System.EventHandler(this.btn_update_Click);
            // 
            // btn_close
            // 
            this.btn_close.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btn_close.Location = new System.Drawing.Point(188, 30);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(78, 32);
            this.btn_close.TabIndex = 10;
            this.btn_close.Text = "Close";
            this.btn_close.UseVisualStyleBackColor = false;
            // 
            // btn_clear
            // 
            this.btn_clear.BackColor = System.Drawing.SystemColors.HotTrack;
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
            this.btn_save.Location = new System.Drawing.Point(26, 29);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(78, 32);
            this.btn_save.TabIndex = 8;
            this.btn_save.Text = "Save";
            this.btn_save.UseVisualStyleBackColor = false;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // picbx_uploadfile
            // 
            this.picbx_uploadfile.Location = new System.Drawing.Point(625, 58);
            this.picbx_uploadfile.Name = "picbx_uploadfile";
            this.picbx_uploadfile.Size = new System.Drawing.Size(108, 50);
            this.picbx_uploadfile.TabIndex = 6;
            this.picbx_uploadfile.TabStop = false;
            // 
            // btn_uploadfile
            // 
            this.btn_uploadfile.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btn_uploadfile.Location = new System.Drawing.Point(546, 72);
            this.btn_uploadfile.Name = "btn_uploadfile";
            this.btn_uploadfile.Size = new System.Drawing.Size(75, 23);
            this.btn_uploadfile.TabIndex = 5;
            this.btn_uploadfile.Text = "Upload File";
            this.btn_uploadfile.UseVisualStyleBackColor = false;
            this.btn_uploadfile.Click += new System.EventHandler(this.btn_uploadfile_Click);
            // 
            // lbl_group
            // 
            this.lbl_group.AutoSize = true;
            this.lbl_group.Location = new System.Drawing.Point(291, 76);
            this.lbl_group.Name = "lbl_group";
            this.lbl_group.Size = new System.Drawing.Size(36, 13);
            this.lbl_group.TabIndex = 3;
            this.lbl_group.Text = "Group";
            // 
            // txt_categoryname
            // 
            this.txt_categoryname.Location = new System.Drawing.Point(89, 76);
            this.txt_categoryname.Name = "txt_categoryname";
            this.txt_categoryname.Size = new System.Drawing.Size(154, 20);
            this.txt_categoryname.TabIndex = 2;
            // 
            // lbl_name
            // 
            this.lbl_name.AutoSize = true;
            this.lbl_name.Location = new System.Drawing.Point(49, 78);
            this.lbl_name.Name = "lbl_name";
            this.lbl_name.Size = new System.Drawing.Size(35, 13);
            this.lbl_name.TabIndex = 1;
            this.lbl_name.Text = "Name";
            // 
            // lbl_addcategory
            // 
            this.lbl_addcategory.AutoSize = true;
            this.lbl_addcategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_addcategory.Location = new System.Drawing.Point(329, 18);
            this.lbl_addcategory.Name = "lbl_addcategory";
            this.lbl_addcategory.Size = new System.Drawing.Size(118, 20);
            this.lbl_addcategory.TabIndex = 0;
            this.lbl_addcategory.Text = "Add Category";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "JPEG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|All file" +
    "s (*.*)|*.*";
            // 
            // FrmCategory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pnl_form);
            this.Name = "FrmCategory";
            this.Text = "FrmCategory";
            this.Load += new System.EventHandler(this.FrmCategory_Load);
            this.pnl_form.ResumeLayout(false);
            this.pnl_listcategory.ResumeLayout(false);
            this.pnl_listcategory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_listcategory)).EndInit();
            this.pnl_addcategory.ResumeLayout(false);
            this.pnl_addcategory.PerformLayout();
            this.pnl_btngroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picbx_uploadfile)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnl_form;
        private System.Windows.Forms.Panel pnl_listcategory;
        private System.Windows.Forms.Label lbl_listcategory;
        private System.Windows.Forms.Panel pnl_addcategory;
        private System.Windows.Forms.Panel pnl_btngroup;
        private System.Windows.Forms.Button btn_close;
        private System.Windows.Forms.Button btn_clear;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.PictureBox picbx_uploadfile;
        private System.Windows.Forms.Button btn_uploadfile;
        private System.Windows.Forms.Label lbl_group;
        private System.Windows.Forms.TextBox txt_categoryname;
        private System.Windows.Forms.Label lbl_name;
        private System.Windows.Forms.Label lbl_addcategory;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.DataGridView dgv_listcategory;
        private System.Windows.Forms.ComboBox cb_groupname;
        private System.Windows.Forms.Button btn_update;
    }
}