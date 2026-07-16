using ModelClass.Master;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace PosBranch_Win.Master
{
    public partial class FrmBranch : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        Branch branch = new Branch();
        Dropdowns drop = new Dropdowns();
        BranchRepository operations = new BranchRepository();
        int Id;


        DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colAddress = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colPhone = new DataGridViewTextBoxColumn();

        public FrmBranch()
        {
            InitializeComponent();
        }

        private void ultraLabelAddress_Click(object sender, EventArgs e)
        {

        }

        private void ultraLabelBranch_Click(object sender, EventArgs e)
        {

        }

        private void FormatGrid()
        {
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView1.Columns.Clear();
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridView1.ColumnHeadersHeight = 35;
            dataGridView1.MultiSelect = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            colId.DataPropertyName = "Id";
            colId.HeaderText = "Id";
            colId.Name = "Id";
            colId.ReadOnly = false;
            colId.Visible = true;
            colId.Width = 250;
            colId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView1.Columns.Add(colId);

            colName.DataPropertyName = "BranchName";
            colName.HeaderText = "BranchName";
            colName.Name = "BranchName";
            colName.ReadOnly = false;
            colName.Visible = true;
            colName.Width = 340;
            colName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView1.Columns.Add(colName);

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

            this.RefreshBranch();

        }


        private void FrmBranch_Load(object sender, EventArgs e)
        {
            this.FormatGrid();
            this.RefreshCompany();
            KeyPreview = true;

            CompanyDDlGrid cp = drop.CompanyDDl();
            cp.List.ToString();
            //ultraGridItem.Visible = true;
            //DataBase.Operations = "GETITEM";
            //ItemDDlGrid item = dp.itemDDlGrid("", txtItemName.Text);
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;

            ts1.GridColumnStyles.Add(datagrid);
            dataGridView1.DataSource = cp.List;
            btnUpdate.Visible = false;
            this.RefreshBranch();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ultraTextName.Clear();
            ultraTextPhn.Clear();
            ultraTextAddress.Clear();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.SaveMaster();
        }
        private void SaveMaster()
        {
            branch.Id = 0;
            branch.CompanyId = Convert.ToInt32(comboBoxCmpny.GetItemText(comboBoxCmpny.SelectedValue));
            branch.BranchName = ultraTextName.Text;
            branch.Address = ultraTextAddress.Text;
            branch.Phone = ultraTextPhn.Text;
            branch._Operation = "CREATE";

            string message = operations.SaveBranch(branch);

            // MessageBox.Show(message);
            frmSuccesMsg msg = new frmSuccesMsg();
            msg.ShowDialog();

            this.RefreshBranch();

        }


        public void RefreshBranch()
        {
            BranchDDlGrid brnchgrid = drop.getBanchDDl();
            brnchgrid.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            ts1.GridColumnStyles.Add(datagrid);
            dataGridView1.DataSource = brnchgrid.List;
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int selectedId = (int)dataGridView1.Rows[e.RowIndex].Cells["Id"].Value;
                Branch bran = operations.GetById(selectedId);

                if (bran != null)
                {
                    Id = bran.Id;
                    ultraTextName.Text = bran.BranchName;
                    ultraTextAddress.Text = bran.Address;
                    ultraTextPhn.Text = bran.Phone;
                    comboBoxCmpny.SelectedValue = bran.CompanyId;
                }
                btnSave.Visible = false;
                btnUpdate.Visible = true;
            }
            else
            {
                MessageBox.Show("Branch not found.");
            }
        }

        public void RefreshCompany()
        {
            System.Data.DataRow dr;
            DataTable dt = new DataTable();
            CompanyDDlGrid grid = drop.CompanyDDl();
            comboBoxCmpny.DataSource = grid.List;
            comboBoxCmpny.DisplayMember = "CompanyName";
            comboBoxCmpny.ValueMember = "CompanyID";
            //grid.List.ToString();

            //IEnumerable result = grid.List.AsEnumerable();
            //CompanyDDl dDl = new CompanyDDl();
            //foreach (CompanyDDl dl in grid.List)
            //{
            //    comboBoxCmpny.Items.Add(dl.CompanyName);

            //}
        }

        private void comboBoxCmpny_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void comboBoxCmpny_SelectedValueChanged(object sender, EventArgs e)
        {

        }
        private void DisplayRowValues(DataGridViewRow row)
        {

            foreach (DataGridViewCell cell in row.Cells)
            {
                ultraTextPhn.Text += row.Cells[0].Value.ToString();
                ultraTextName.Text = row.Cells[1].Value.ToString();
                ultraTextAddress.Text = row.Cells[1].Value.ToString();
            }

        }


        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
                    dataGridView1.Rows[0].Selected = true;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
                    dataGridView1.Rows[0].Selected = true;
                    dataGridView1.Focus();
                }
            }
        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DataGridViewRow selectedRow = dataGridView1.CurrentRow;
                DisplayRowValues(selectedRow);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            BranchDDlGrid brgrid = operations.SearchBranch(TxtSearch.Text);
            brgrid.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            ts1.GridColumnStyles.Add(datagrid);
            dataGridView1.DataSource = brgrid.List;
        }

        private void FrmBranch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                SaveMaster();
                this.RefreshBranch();
            }
            else if (e.KeyCode == Keys.F4)
            {
                this.Close();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Id"].Value);
                Branch bran = operations.Delete(selectedId);

                if (bran != null)
                {
                    Id = bran.Id;
                    ultraTextName.Text = bran.BranchName;
                    ultraTextAddress.Text = bran.Address;
                    ultraTextPhn.Text = bran.Phone;
                    comboBoxCmpny.SelectedValue = bran.CompanyId;

                    MessageBox.Show("Record deleted successfully.");

                }

                else
                {
                    MessageBox.Show("Error deleting record.");
                }

                btnSave.Visible = false;
                btnUpdate.Visible = true;
            }
            else
            {
                MessageBox.Show("Please select a record to delete.");
            }
            this.RefreshBranch();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            branch.Id = Id;
            branch.CompanyId = Convert.ToInt32(comboBoxCmpny.GetItemText(comboBoxCmpny.SelectedValue));

            branch.BranchName = ultraTextName.Text;
            branch.Address = ultraTextAddress.Text;
            branch.Phone = ultraTextPhn.Text;
            branch._Operation = "Update";
            //Branch update = operations.UpdateBranch();
            Branch message = operations.UpdateBranch(branch);
            MessageBox.Show("Branch Update Success", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.RefreshBranch();
        }
    }
}
