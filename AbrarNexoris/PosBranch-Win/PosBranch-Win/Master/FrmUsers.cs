using ModelClass.Master;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Master
{
    public partial class FrmUsers : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        Users users = new Users();
        Dropdowns drop = new Dropdowns();
        UsersRepository operations = new UsersRepository();
        int Id;
        
        DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();

        public FrmUsers()
        {
            InitializeComponent();

            
        }

        private void cmbCompany_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void FormatGrid()
        {
            dgvUser.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgvUser.Columns.Clear();
            dgvUser.AllowUserToOrderColumns = true;
            dgvUser.AllowUserToDeleteRows = false;
            dgvUser.AllowUserToAddRows = false;
            dgvUser.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dgvUser.AutoGenerateColumns = false;
            dgvUser.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgvUser.ColumnHeadersHeight = 35;
            dgvUser.MultiSelect = false;
            dgvUser.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            colId.DataPropertyName = "UserID";
            colId.HeaderText = "Id";
            colId.Name = "UserID";
            colId.ReadOnly = false;
            colId.Visible = true;
            colId.Width = 200;
            colId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvUser.Columns.Add(colId);

            colName.DataPropertyName = "UserName";
            colName.HeaderText = "UserName";
            colName.Name = "UserName";
            colName.ReadOnly = false;
            colName.Visible = true;
            colName.Width = 300;
            colName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvUser.Columns.Add(colName);

            this.RefreshUser();
        }

        private void FrmUsers_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
            this.FormatGrid();
            this.RefreshCompany();
            this.RefreshBranch();
            this.RefreshUserLevel();
            btnSave.Visible = true;
            btnUpdate.Visible = false;

        }
        public void RefreshCompany()
        {
            System.Data.DataRow dr;
            DataTable dt = new DataTable();
            CompanyDDlGrid grid = drop.CompanyDDl();
            cmbCompany.DataSource = grid.List;
            cmbCompany.DisplayMember = "CompanyName";
            cmbCompany.ValueMember = "CompanyID";

        }
        public void RefreshBranch()
        {
            System.Data.DataRow dr;
            

            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)con.DataConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.AddWithValue("_Operation", "GETALL");
                    DataTable dt = new DataTable();//fdf
                    adapt.Fill(dt);
                    dr = dt.NewRow();
                    //dr.ItemArray = new object[] { 0, "--Select Branch--" };
                    //dt.Rows.InsertAt(dr, 0);
                    cmbBranch.ValueMember = "Id";
                    cmbBranch.DisplayMember = "BranchName";
                    cmbBranch.DataSource = dt;

                }
            }
        }
        public void RefreshUserLevel()
        {
            System.Data.DataRow dr;
            DataTable dt = new DataTable();
            UserLevelDDlGrid grid = drop.UserLevelDDl();
            cmbUserLevel.DataSource = grid.List;
            cmbUserLevel.DisplayMember = "UserLevel";
            cmbUserLevel.ValueMember = "UserLevelID";

        }
        private void SaveUser()
        {
            users.UserID = 0;
            users.CompanyID = Convert.ToInt32(cmbCompany.GetItemText(cmbCompany.SelectedValue));
            users.BranchID = Convert.ToInt32(cmbBranch.GetItemText(cmbBranch.SelectedValue));
            users.UserLevelID = Convert.ToInt32(cmbUserLevel.GetItemText(cmbUserLevel.SelectedValue));
            users.UserName = textUserName.Text;
            users.Password = textPassword.Text;
            users.Email = textEmail.Text;
            users._Operation = "CREATE";

            string message = operations.SaveUser(users);
            frmSuccesMsg msg = new frmSuccesMsg();
            msg.ShowDialog();

            this.RefreshUser();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.SaveUser();
            
        }

        public void RefreshUser()
        {
            UserDDlGrid usgrid = drop.getUsersDDl();
            usgrid.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            ts1.GridColumnStyles.Add(datagrid);
            dgvUser.DataSource = usgrid.List;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            textUserName.Clear();
            textPassword.Clear();
            textEmail.Clear();
            btnSave.Visible = true;
            btnUpdate.Visible = false;
        }

        private void dgvUser_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           
        }

        private void dgvUser_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int selectedId = (int)dgvUser.Rows[e.RowIndex].Cells["UserID"].Value;
                Users usr = operations.GetById(selectedId);

                if (usr != null)
                {
                    Id = usr.UserID;
                    textUserName.Text = usr.UserName;
                    textEmail.Text = usr.Email;
                    textPassword.Text = usr.Password;
                    cmbCompany.SelectedValue = usr.CompanyID;
                    cmbBranch.SelectedValue = usr.BranchID;
                    cmbUserLevel.SelectedValue = usr.UserLevelID;
                }
                btnSave.Visible = false;
                btnUpdate.Visible = true;
            }
            else
            {
                MessageBox.Show("Users not found.");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            users.UserID = Id;
            users.CompanyID = Convert.ToInt32(cmbCompany.GetItemText(cmbCompany.SelectedValue));
            users.BranchID = Convert.ToInt32(cmbBranch.GetItemText(cmbBranch.SelectedValue));
            users.UserLevelID = Convert.ToInt32(cmbUserLevel.GetItemText(cmbUserLevel.SelectedValue));
            users.UserName = textUserName.Text;
            users.Email = textEmail.Text;
            users.Password = textPassword.Text;
            users._Operation = "UPDATE";
            
            Users message = operations.Update(users);
            MessageBox.Show("User Update Success", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.RefreshUser();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvUser.SelectedRows.Count > 0)
            {
                int selectedId = Convert.ToInt32(dgvUser.SelectedRows[0].Cells["UserID"].Value);
                Users usrs = operations.Delete(selectedId);

                if (usrs != null)
                {
                    Id = usrs.UserID;
                    textUserName.Text = usrs.UserName;
                    textEmail.Text = usrs.Email;
                    textPassword.Text = usrs.Password;
                    cmbCompany.SelectedValue = usrs.CompanyID;
                    cmbBranch.SelectedValue = usrs.BranchID;
                    cmbUserLevel.SelectedValue = usrs.UserLevelID;

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
            this.RefreshUser();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            UserDDlGrid usrgrid = operations.Search(TxtSearch.Text);
            usrgrid.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            ts1.GridColumnStyles.Add(datagrid);
            dgvUser.DataSource = usrgrid.List;
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (dgvUser.Rows.Count > 0)
                {
                    dgvUser.CurrentCell = dgvUser.Rows[0].Cells[0];
                    dgvUser.Rows[0].Selected = true;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (dgvUser.Rows.Count > 0)
                {
                    dgvUser.CurrentCell = dgvUser.Rows[0].Cells[0];
                    dgvUser.Rows[0].Selected = true;
                    dgvUser.Focus();
                }
            }
        }

        private void dgvUser_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void FrmUsers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                this.SaveUser();
                this.RefreshUser();
            }
            else if (e.KeyCode == Keys.F4)
            {
                this.Close();
            }
        }

        private void textEmail_Validating(object sender, CancelEventArgs e)
        {
            Regex mRegxExpression;
            if (textEmail.Text.Trim() != string.Empty)
            {
                mRegxExpression = new Regex(@"^([a-zA-Z0-9_\-])([a-zA-Z0-9_\-\.]*)@(\[((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\.){3}|((([a-zA-Z0-9\-]+)\.)+))([a-zA-Z]{2,}|(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\])$");

                if (!mRegxExpression.IsMatch(textEmail.Text.Trim()))
                {
                    MessageBox.Show("E-mail address format is not correct.", "MojoCRM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textEmail.Focus();
                }
            }

        }
    }
}
