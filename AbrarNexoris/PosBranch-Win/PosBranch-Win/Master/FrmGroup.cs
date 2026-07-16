
using ModelClass;
using ModelClass.Master;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Master
{
    public partial class FrmGroup : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        Group grp = new Group();
        GroupRepository  operations = new GroupRepository();
        Dropdowns dd = new Dropdowns();
        int Id;

        DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colGroupName = new DataGridViewTextBoxColumn();

        public FrmGroup()
        {
            InitializeComponent();
        }

        private void btn_upload_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                picb_uploadfile.Load(openFileDialog1.FileName);
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            SaveMaster();
        }

        private void FrmGroup_Load(object sender, EventArgs e)
        {
            dgv_listgroup.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgv_listgroup.Columns.Clear();
            dgv_listgroup.AllowUserToOrderColumns = true;
            dgv_listgroup.AllowUserToDeleteRows = false;
            dgv_listgroup.AllowUserToAddRows = false;
            dgv_listgroup.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dgv_listgroup.AutoGenerateColumns = false;
            dgv_listgroup.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgv_listgroup.ColumnHeadersHeight = 35;
            dgv_listgroup.MultiSelect = false;
            dgv_listgroup.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            colId.DataPropertyName = "Id";
            colId.HeaderText = "Id";
            colId.Name = "Id";
            colId.ReadOnly = false;
            colId.Visible = true;
            colId.Width = 310;
            colId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_listgroup.Columns.Add(colId);

            colGroupName.DataPropertyName = "GroupName";
            colGroupName.HeaderText = "Group Name";
            colGroupName.Name = "Group Name";
            colGroupName.ReadOnly = false;
            colGroupName.Visible = true;
            colGroupName.Width = 310;
            colGroupName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colGroupName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_listgroup.Columns.Add(colGroupName);
            this.RefreshGroup();
            btn_save.Visible = true;
            btn_update.Visible = false;

        }
        public void RefreshGroup()
        {
             GroupDDlGrid grpgrid = dd.getGroupDDl();
             grpgrid.List.ToString();
             DataGridTableStyle ts1 = new DataGridTableStyle();
             DataGridColumnStyle datagrid = new DataGridBoolColumn();
             ts1.GridColumnStyles.Add(datagrid);
             dgv_listgroup.DataSource = grpgrid.List;
        }

        private void SaveMaster()
        {
            grp.GroupName = txt_gname.Text;
            byte[] bytes = File.ReadAllBytes(openFileDialog1.FileName);
            grp.Photo = bytes;
            grp._Operation = "CREATE";
            string message = operations.SaveGroup(grp);
            MessageBox.Show(message);
            this.RefreshGroup();
        }

        private void dgv_listgroup_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int selectedId = (int)dgv_listgroup.Rows[e.RowIndex].Cells["Id"].Value;
                Group gr = operations.GetById1(selectedId);

                if (gr != null)
                {
                    Id = gr.Id;
                    txt_gname.Text = gr.GroupName;
                    Byte[] bobj = new byte[0];
                    bobj = gr.Photo;
                    picb_uploadfile.Image = Image.FromStream(new MemoryStream(bobj));
                }
                btn_save.Visible = false;
                btn_update.Visible = true;
            }
            else
            {
                MessageBox.Show("Group not found.");
            }
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            txt_gname.Clear();
            picb_uploadfile.Visible = false;
            btn_save.Visible = true;
            btn_update.Visible = false;
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            grp.Id = Id;
            grp.GroupName = txt_gname.Text;
            Byte [] bytes = File.ReadAllBytes(openFileDialog1.FileName);
            grp.Photo = bytes;
            grp._Operation = "UPDATE";
            Group message = operations.UpdateGroup(grp);
            this.RefreshGroup();
        }
    }
}