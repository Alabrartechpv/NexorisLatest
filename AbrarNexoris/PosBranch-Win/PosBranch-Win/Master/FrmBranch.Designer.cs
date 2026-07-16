
namespace PosBranch_Win.Master
{
    partial class FrmBranch
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ultraLabelListBranch = new Infragistics.Win.Misc.UltraLabel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.comboBoxCmpny = new System.Windows.Forms.ComboBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.ultraLabelCmpny = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextPhn = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelPhone = new Infragistics.Win.Misc.UltraLabel();
            this.ultraLabelBranch = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextAddress = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelAddress = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextName = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLblName = new Infragistics.Win.Misc.UltraLabel();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.TxtSearch = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextPhn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextAddress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextName)).BeginInit();
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
            this.panel1.Size = new System.Drawing.Size(1280, 446);
            this.panel1.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.LightBlue;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel3.Controls.Add(this.TxtSearch);
            this.panel3.Controls.Add(this.dataGridView1);
            this.panel3.Controls.Add(this.ultraLabelListBranch);
            this.panel3.Location = new System.Drawing.Point(793, 12);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(484, 434);
            this.panel3.TabIndex = 1;
            // 
            // dataGridView1
            // 
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(3, 100);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(477, 318);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.dataGridView1_KeyUp);
            // 
            // ultraLabelListBranch
            // 
            this.ultraLabelListBranch.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelListBranch.Location = new System.Drawing.Point(3, 3);
            this.ultraLabelListBranch.Name = "ultraLabelListBranch";
            this.ultraLabelListBranch.Size = new System.Drawing.Size(134, 23);
            this.ultraLabelListBranch.TabIndex = 0;
            this.ultraLabelListBranch.Text = "LIST BRANCH";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.LightBlue;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel2.Controls.Add(this.comboBoxCmpny);
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Controls.Add(this.ultraLabelCmpny);
            this.panel2.Controls.Add(this.ultraTextPhn);
            this.panel2.Controls.Add(this.ultraLabelPhone);
            this.panel2.Controls.Add(this.ultraLabelBranch);
            this.panel2.Controls.Add(this.ultraTextAddress);
            this.panel2.Controls.Add(this.ultraLabelAddress);
            this.panel2.Controls.Add(this.ultraTextName);
            this.panel2.Controls.Add(this.ultraLblName);
            this.panel2.Location = new System.Drawing.Point(12, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(776, 431);
            this.panel2.TabIndex = 0;
            // 
            // comboBoxCmpny
            // 
            this.comboBoxCmpny.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxCmpny.FormattingEnabled = true;
            this.comboBoxCmpny.Location = new System.Drawing.Point(477, 165);
            this.comboBoxCmpny.Name = "comboBoxCmpny";
            this.comboBoxCmpny.Size = new System.Drawing.Size(277, 33);
            this.comboBoxCmpny.TabIndex = 17;
            this.comboBoxCmpny.SelectedIndexChanged += new System.EventHandler(this.comboBoxCmpny_SelectedIndexChanged);
            this.comboBoxCmpny.SelectedValueChanged += new System.EventHandler(this.comboBoxCmpny_SelectedValueChanged);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnUpdate);
            this.panel4.Controls.Add(this.btnSave);
            this.panel4.Controls.Add(this.btnDelete);
            this.panel4.Controls.Add(this.btnClear);
            this.panel4.Location = new System.Drawing.Point(6, 321);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(443, 97);
            this.panel4.TabIndex = 16;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnSave.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnSave.Location = new System.Drawing.Point(37, 31);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(92, 35);
            this.btnSave.TabIndex = 13;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnDelete.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnDelete.Location = new System.Drawing.Point(300, 32);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(95, 34);
            this.btnDelete.TabIndex = 15;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnClear.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnClear.Location = new System.Drawing.Point(169, 32);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(94, 34);
            this.btnClear.TabIndex = 14;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // ultraLabelCmpny
            // 
            this.ultraLabelCmpny.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelCmpny.Location = new System.Drawing.Point(380, 165);
            this.ultraLabelCmpny.Name = "ultraLabelCmpny";
            this.ultraLabelCmpny.Size = new System.Drawing.Size(100, 23);
            this.ultraLabelCmpny.TabIndex = 11;
            this.ultraLabelCmpny.Text = "Company";
            // 
            // ultraTextPhn
            // 
            this.ultraTextPhn.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextPhn.Location = new System.Drawing.Point(83, 155);
            this.ultraTextPhn.Name = "ultraTextPhn";
            this.ultraTextPhn.Size = new System.Drawing.Size(278, 33);
            this.ultraTextPhn.TabIndex = 10;
            // 
            // ultraLabelPhone
            // 
            this.ultraLabelPhone.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelPhone.Location = new System.Drawing.Point(3, 150);
            this.ultraLabelPhone.Name = "ultraLabelPhone";
            this.ultraLabelPhone.Size = new System.Drawing.Size(100, 23);
            this.ultraLabelPhone.TabIndex = 9;
            this.ultraLabelPhone.Text = "Phone";
            // 
            // ultraLabelBranch
            // 
            this.ultraLabelBranch.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelBranch.Location = new System.Drawing.Point(6, 3);
            this.ultraLabelBranch.Name = "ultraLabelBranch";
            this.ultraLabelBranch.Size = new System.Drawing.Size(132, 23);
            this.ultraLabelBranch.TabIndex = 8;
            this.ultraLabelBranch.Text = "ADD BRANCH";
            this.ultraLabelBranch.Click += new System.EventHandler(this.ultraLabelBranch_Click);
            // 
            // ultraTextAddress
            // 
            this.ultraTextAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextAddress.Location = new System.Drawing.Point(474, 100);
            this.ultraTextAddress.Multiline = true;
            this.ultraTextAddress.Name = "ultraTextAddress";
            this.ultraTextAddress.Size = new System.Drawing.Size(282, 59);
            this.ultraTextAddress.TabIndex = 7;
            // 
            // ultraLabelAddress
            // 
            this.ultraLabelAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelAddress.Location = new System.Drawing.Point(382, 114);
            this.ultraLabelAddress.Name = "ultraLabelAddress";
            this.ultraLabelAddress.Size = new System.Drawing.Size(100, 23);
            this.ultraLabelAddress.TabIndex = 6;
            this.ultraLabelAddress.Text = "Address";
            // 
            // ultraTextName
            // 
            this.ultraTextName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextName.Location = new System.Drawing.Point(83, 108);
            this.ultraTextName.Name = "ultraTextName";
            this.ultraTextName.Size = new System.Drawing.Size(278, 33);
            this.ultraTextName.TabIndex = 5;
            // 
            // ultraLblName
            // 
            this.ultraLblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLblName.Location = new System.Drawing.Point(3, 103);
            this.ultraLblName.Name = "ultraLblName";
            this.ultraLblName.Size = new System.Drawing.Size(100, 23);
            this.ultraLblName.TabIndex = 4;
            this.ultraLblName.Text = "Name";
            // 
            // btnUpdate
            // 
            this.btnUpdate.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnUpdate.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnUpdate.Location = new System.Drawing.Point(37, 31);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(92, 35);
            this.btnUpdate.TabIndex = 16;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click += new System.EventHandler(this.button1_Click);
            // 
            // TxtSearch
            // 
            this.TxtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtSearch.Location = new System.Drawing.Point(4, 68);
            this.TxtSearch.Name = "TxtSearch";
            this.TxtSearch.Size = new System.Drawing.Size(473, 27);
            this.TxtSearch.TabIndex = 2;
            this.TxtSearch.TextChanged += new System.EventHandler(this.TxtSearch_TextChanged);
            this.TxtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtSearch_KeyDown);
            // 
            // FrmBranch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 446);
            this.Controls.Add(this.panel1);
            this.Name = "FrmBranch";
            this.Text = "FrmBranch";
            this.Load += new System.EventHandler(this.FrmBranch_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmBranch_KeyDown);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextPhn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextAddress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextName)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private Infragistics.Win.Misc.UltraLabel ultraLabelBranch;
        private Infragistics.Win.Misc.UltraLabel ultraLabelAddress;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextName;
        private Infragistics.Win.Misc.UltraLabel ultraLblName;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnSave;
        private Infragistics.Win.Misc.UltraLabel ultraLabelCmpny;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextPhn;
        private Infragistics.Win.Misc.UltraLabel ultraLabelPhone;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextAddress;
        private Infragistics.Win.Misc.UltraLabel ultraLabelListBranch;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ComboBox comboBoxCmpny;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.TextBox TxtSearch;
    }
}