using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModelClass;
using Repository;
using System.IO;
using ModelClass.Master;

namespace PosBranch_Win.Master
{
    public partial class FrmBrand : Form
    {
        Brand brnd = new Brand();
        Dropdowns drp = new Dropdowns();
        ClientOperations operations = new ClientOperations();
        int Id;
        public int SelectedId;
        public FrmBrand()
        {
            InitializeComponent();
            btnUpdate.Visible = false;
        }


        private void FormatGrid()
        {
            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();
            dataGridViewBrand.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridViewBrand.Columns.Clear();
            dataGridViewBrand.AllowUserToOrderColumns = true;
            dataGridViewBrand.AllowUserToDeleteRows = false;
            dataGridViewBrand.AllowUserToAddRows = false;
            dataGridViewBrand.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dataGridViewBrand.AutoGenerateColumns = false;
            dataGridViewBrand.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewBrand.ColumnHeadersHeight = 35;
            dataGridViewBrand.MultiSelect = false;
            dataGridViewBrand.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            colId.DataPropertyName = "Id";
            colId.HeaderText = "Id";
            colId.Name = "Id";
            colId.ReadOnly = false;
            colId.Visible = true;
            colId.Width = 100;
            colId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewBrand.Columns.Add(colId);

            colName.DataPropertyName = "BrandName";
            colName.HeaderText = "BrandName";
            colName.Name = "BrandName";
            colName.ReadOnly = false;
            colName.Visible = true;
            colName.Width = 350;
            colName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewBrand.Columns.Add(colName);

            //colAddress.DataPropertyName = "Address";
            //colAddress.HeaderText = "Address";
            //colAddress.Name = "Address";
            //colAddress.ReadOnly = false;
            //colAddress.Visible = true;
            //colAddress.Width = 220;
            //colAddress.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //colAddress.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //dataGridView1.Columns.Add(colAddress);


            //colPhone.DataPropertyName = "Phone";
            //colPhone.HeaderText = "Phone";
            //colPhone.Name = "Phone";
            //colPhone.ReadOnly = false;
            //colPhone.Visible = true;
            //colPhone.Width = 210;
            //colPhone.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //colPhone.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //dataGridView1.Columns.Add(colPhone);

            this.RefreshBrand();

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            brnd.BrandName = txtBrandName.Text;

            Byte[] brandPhoto = File.ReadAllBytes(openFileDialog1.FileName);
            brnd.Photo = brandPhoto;

            brnd._Operation = "CREATE";
            string message = operations.SaveBrand(brnd);
            MessageBox.Show(message);
            this.RefreshBrand();
        }

        private void FrmBrand_Load(object sender, EventArgs e)
        {
            this.FormatGrid();
            this.RefreshBrand();
        }

        public void RefreshBrand()
        {
            BrandDDLGrid brndGrid = drp.getBrandDDL();
            brndGrid.List.ToString();
            dataGridViewBrand.DataSource = brndGrid.List;
        }

        private void dataGridViewBrand_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            Brand brd = new Brand();
            if (e.RowIndex >= 0)
            {
                brd.Id = (int)dataGridViewBrand.Rows[e.RowIndex].Cells["Id"].Value;
                Brand brnd = operations.GetBrandId(brd);
                Byte[] brandPhoto = new Byte[0];
                if (brnd != null)
                {
                    Id = brnd.Id;
                    txtBrandName.Text = brnd.BrandName;
                    brandPhoto = brnd.Photo;
                    if (brnd.Photo.Length > 0)
                    {
                        MemoryStream ms = new MemoryStream(brandPhoto);
                        pictureBoxBrand.Image = Image.FromStream(ms);
                    }
                }                
                btnSave.Visible = false;
                btnUpdate.Visible = true;

            }

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            brnd.Id = Id;
            brnd.BrandName = txtBrandName.Text;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Byte[] brdPhoto = File.ReadAllBytes(openFileDialog1.FileName);
                brnd.Photo = brdPhoto;
            }
            brnd._Operation = "Update";
            Brand message = operations.UpdateBrand(brnd);
            this.RefreshBrand();
            btnUpdate.Visible = false;
            btnSave.Visible = true;
            txtBrandName.Text = "";

        }

        private void brnClear_Click(object sender, EventArgs e)
        {
            txtBrandName.Text = "";
            pictureBoxBrand.Image = null;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            brnd.Id = SelectedId;
            operations.DeleteBrand(brnd);
            this.RefreshBrand();
            txtBrandName.Text = "";
            btnUpdate.Visible = false;
            btnSave.Visible = true;

        }



        private void txtBrandSearch_TextChanged(object sender, EventArgs e)
        {
            Brand brd = new Brand();
            brd.BrandName = txtBrandSearch.Text;

            BrandDDLGrid bddlg = new BrandDDLGrid();
            bddlg = operations.BrandSearch(brd);
            dataGridViewBrand.DataSource = bddlg.List;
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBoxBrand.Load(openFileDialog1.FileName);
                 string s =openFileDialog1.FileName;

                Byte[] bphoto = File.ReadAllBytes(openFileDialog1.FileName);
                string sphoto = Convert.ToBase64String(bphoto);

            }

        }

        public Byte[] imageToByteArray(Image imagein)
        {
            MemoryStream ms = new MemoryStream();
            imagein.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
            
        }

        private void FrmBrand_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
