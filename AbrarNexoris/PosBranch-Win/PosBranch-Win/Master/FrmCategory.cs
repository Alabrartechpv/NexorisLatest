using ModelClass.Master;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections;
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
    public partial class FrmCategory : Form
    {
        DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colCategoryName = new DataGridViewTextBoxColumn();
        int Id;

        Dropdowns dd = new Dropdowns();
        Category categ = new Category();
        CategoryRepository clientop = new CategoryRepository();

        public FrmCategory()
        {
            InitializeComponent();
            btn_update.Visible = false;
        }

        private void btn_uploadfile_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                picbx_uploadfile.Load(openFileDialog1.FileName);
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            saveMaster();
            RefreshCategory();
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            txt_categoryname.Clear();
            cb_groupname.SelectedValue = false;
            picbx_uploadfile.Image = null;
            btn_save.Visible = true;
            btn_update.Visible = false;
        }

        private void FrmCategory_Load(object sender, EventArgs e)
        {
            dgv_listcategory.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgv_listcategory.Columns.Clear();
            dgv_listcategory.AllowUserToOrderColumns = true;
            dgv_listcategory.AllowUserToDeleteRows = false;
            dgv_listcategory.AllowUserToAddRows = false;
            dgv_listcategory.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dgv_listcategory.AutoGenerateColumns = false;
            dgv_listcategory.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgv_listcategory.ColumnHeadersHeight = 35;
            dgv_listcategory.MultiSelect = false;
            dgv_listcategory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            colId.DataPropertyName = "Id";
            colId.HeaderText = "Id";
            colId.Name = "Id";
            colId.ReadOnly = false;
            colId.Visible = true;
            colId.Width = 310;
            colId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_listcategory.Columns.Add(colId);

            colCategoryName.DataPropertyName = "CategoryName";
            colCategoryName.HeaderText = "Category Name";
            colCategoryName.Name = "Category Name";
            colCategoryName.ReadOnly = false;
            colCategoryName.Visible = true;
            colCategoryName.Width = 310;
            colCategoryName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colCategoryName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_listcategory.Columns.Add(colCategoryName);

            this.RefreshCategory();

            DataRow dr;
            DataTable dt = new DataTable();
            GroupDDlGrid grid = dd.GroupDDl();
            cb_groupname.DataSource = grid.List;
            cb_groupname.DisplayMember = "GroupName";
            //cb_groupname.ValueMember = "GroupId";

            //grid.List.ToString();
            //IEnumerable result = grid.List.AsEnumerable();
            //GroupDDL dDl = new GroupDDL();
            //foreach (GroupDDL dl in grid.List)
            //{
            //    cb_groupname.Items.Add(dl.GroupName);
            //}
        }

        public void RefreshCategory()
        {
            this.RefreshGroup();
            CategoryDDlGrid catgrid = dd.getCategoryDDl("");
            catgrid.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            ts1.GridColumnStyles.Add(datagrid);
            dgv_listcategory.DataSource = catgrid.List;
        }

        private void saveMaster()
        {
            categ.CategoryName = txt_categoryname.Text;
            categ.GroupId = Convert.ToInt32(cb_groupname.GetItemText(cb_groupname.SelectedValue));
            byte[] bytes = File.ReadAllBytes(openFileDialog1.FileName);
            categ.Photo = bytes;
            categ._Operation = "CREATE";
            string message = clientop.SaveCategory(categ);
            MessageBox.Show(message);
        }

        public void RefreshGroup()
        {
            DataRow dr;
            DataTable dt = new DataTable();
            GroupDDlGrid grid = dd.GroupDDl();
            cb_groupname.DataSource = grid.List;
            cb_groupname.DisplayMember = "GroupName";
            cb_groupname.ValueMember = "Id";
        }

        private void dgv_listcategory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int selectedId = (int)dgv_listcategory.Rows[e.RowIndex].Cells["Id"].Value;
                Category categ = clientop.GetByIdCategory(selectedId);

                if (categ != null)
                {
                    Id = categ.Id;
                    txt_categoryname.Text = categ.CategoryName;
                    cb_groupname.SelectedValue = categ.GroupId;
                    cb_groupname.SelectedItem = cb_groupname.Text.ToString(); 
                    Byte[] objpic = new byte[0];
                    objpic = categ.Photo;
                    if(categ.Photo != null) {
                        if (categ.Photo.Length > 0)
                        {
                            picbx_uploadfile.Image = Image.FromStream(new MemoryStream(objpic));
                        }
                    }
                   
                }
                btn_save.Visible = false;
                btn_update.Visible = true;
            }
            else
            {
                MessageBox.Show("Category not found.");
            }
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            categ.Id = Id;
            categ.CategoryName = txt_categoryname.Text;
            categ.GroupId = Convert.ToInt32(cb_groupname.GetItemText(cb_groupname.SelectedValue));
            cb_groupname.SelectedItem = cb_groupname.Text.ToString();
            byte[] bytes = File.ReadAllBytes(openFileDialog1.FileName);
            categ.Photo = bytes;
            categ._Operation = "UPDATE";
            Category message = clientop.UpdateCategory(categ);
            this.RefreshCategory();
            txt_categoryname.Text = "";
        }
    }
}
