using ModelClass;
using ModelClass.Settings;
using Repository;
using Repository.SettingsRepo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PosBranch_Win
{
    public partial class Login : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        EncryptionAndDecryptionHelper enc = new EncryptionAndDecryptionHelper();

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );


        public Login()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Login_Load(object sender, EventArgs e)
        {
            DataBase.Status = "Online";
            this.RefreshBranch();
            // Random rnd = new Random();
            // panel1.BackColor = Color.FromArgb(rnd.Next(10), rnd.Next(98), rnd.Next(121), rnd.Next(100));
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Validate input fields
            if (string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                MessageBox.Show("Please enter your username.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUserName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter your password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            // Validate branch selection
            if (string.IsNullOrEmpty(DataBase.BranchId) || DataBase.BranchId == "0")
            {
                MessageBox.Show("Please select a branch before logging in.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox1.Focus();
                return;
            }

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Login, (SqlConnection)con.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", txtUserName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Password", enc.Encrypt(txtPassword.Text, true));
                    cmd.Parameters.AddWithValue("@BranchId", Convert.ToInt64(DataBase.BranchId));
                    DataBase.UserName = txtUserName.Text.Trim();

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet dt = new DataSet();
                        adapt.Fill(dt);

                        if ((dt != null) && (dt.Tables.Count > 0) && (dt.Tables[0] != null) && (dt.Tables[0].Rows.Count > 0))
                        {
                            // Legacy DataBase properties (kept for backward compatibility)
                            DataBase.BranchId = dt.Tables[0].Rows[0][0].ToString();
                            DataBase.CompanyId = dt.Tables[0].Rows[0][1].ToString();
                            DataBase.EmailId = dt.Tables[0].Rows[0][2].ToString();
                            DataBase.UserId = dt.Tables[0].Rows[0][3].ToString();
                            DataBase.UserName = dt.Tables[0].Rows[0][4].ToString();
                            DataBase.UserLevel = dt.Tables[0].Rows[0][5].ToString();
                            DataBase.Message = dt.Tables[0].Rows[0][6].ToString();

                            // Initialize new SessionContext
                            try
                            {
                                int branchId = Convert.ToInt32(dt.Tables[0].Rows[0][0]);
                                int companyId = Convert.ToInt32(dt.Tables[0].Rows[0][1]);
                                string emailId = dt.Tables[0].Rows[0][2].ToString();
                                int userId = Convert.ToInt32(dt.Tables[0].Rows[0][3]);
                                string userName = dt.Tables[0].Rows[0][4].ToString();
                                string userLevel = dt.Tables[0].Rows[0][5].ToString();

                                // Get FinYearId from DataBase if available, otherwise use default
                                int finYearId = !string.IsNullOrEmpty(DataBase.FinyearId) ? SessionContext.FinYearId : 1;

                                // Initialize session context
                                SessionContext.InitializeFromLogin(
                                    companyId: companyId,
                                    branchId: branchId,
                                    finYearId: finYearId,
                                    userId: userId,
                                    userName: userName,
                                    userLevel: userLevel,
                                    emailId: emailId,
                                    branchName: DataBase.Branch
                                );

                                // Load POS Settings from database
                                LoadPOSSettings(companyId, branchId);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Warning: Failed to initialize session context: {ex.Message}",
                                    "Session Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }

                            Home hm = new Home();
                            hm.Show();
                            this.Hide();
                        }
                        else
                        {
                            // Login failed - no matching user found
                            MessageBox.Show("Invalid username or password.\nPlease check your credentials and try again.",
                                "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            txtPassword.Clear();
                            txtPassword.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during login:\n{ex.Message}",
                    "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void RefreshBranch()
        {
            DataRow dr;
            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)con.DataConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.AddWithValue("_Operation", "GETALL");
                    DataTable dt = new DataTable();//fdf
                    adapt.Fill(dt);
                    dr = dt.NewRow();
                    dr.ItemArray = new object[] { 0, "--Select Branch--" };
                    dt.Rows.InsertAt(dr, 0);
                    comboBox1.ValueMember = "Id";
                    comboBox1.DisplayMember = "BranchName";
                    comboBox1.DataSource = dt;
                    // dataGridView1.DataSource = ds.Tables[0];
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataBase.Branch = comboBox1.GetItemText(comboBox1.SelectedItem);
            DataBase.BranchId = comboBox1.GetItemText(comboBox1.SelectedValue);


        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void comboBox1_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtUserName.Focus();
            }
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtPassword.Focus();
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.Focus();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Loads POS settings from database into SessionContext
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="branchId">Branch ID</param>
        private void LoadPOSSettings(int companyId, int branchId)
        {
            try
            {
                using (var settingsRepo = new POSSettingsRepository())
                {
                    var settings = settingsRepo.GetSettings(companyId, branchId);
                    
                    if (settings.Count == 0)
                    {
                        // Initialize default settings if none exist
                        settingsRepo.InitializeDefaultSettings(companyId, branchId);
                        settings = settingsRepo.GetSettings(companyId, branchId);
                    }

                    SessionContext.LoadSettings(settings);
                    System.Diagnostics.Debug.WriteLine($"POS Settings loaded: {settings.Count} settings for Company {companyId}, Branch {branchId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to load POS settings: {ex.Message}");
                // Continue with default settings - don't block login
            }
        }
    }
}
