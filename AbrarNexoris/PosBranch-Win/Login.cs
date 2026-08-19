using ModelClass;
using ModelClass.Settings;
using Repository;
using Repository.MasterRepositry;
using Repository.SettingsRepo;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PosBranch_Win
{
    public partial class Login : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        EncryptionAndDecryptionHelper enc = new EncryptionAndDecryptionHelper();
        TextBox activeTextBox = null;
        private Timer animTimer;
        private int picAnimStep = 0;
        private Point picOriginalLocation;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        public Login()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            pnlContainer.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlContainer.Width, pnlContainer.Height, 14, 14));

            // Load logo image
            try
            {
                string[] possiblePaths = new string[]
                {
                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources", "ChatGPT Image Feb 3, 2026, 12_16_25 AM.png"),
                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Resources", "ChatGPT Image Feb 3, 2026, 12_16_25 AM.png"),
                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "Resources", "ChatGPT Image Feb 3, 2026, 12_16_25 AM.png")
                };


            }
            catch { }

            txtUserName.Enter += (s, e) => activeTextBox = txtUserName;
            txtPassword.Enter += (s, e) => activeTextBox = txtPassword;
            activeTextBox = txtUserName;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            DataBase.Status = "Online";
            this.RefreshBranch();

            // Auto select branch 1 so user does not need combobox
            if (comboBox1.Items.Count > 1)
            {
                comboBox1.SelectedIndex = 1;
                DataBase.Branch = comboBox1.GetItemText(comboBox1.SelectedItem);
                DataBase.BranchId = comboBox1.SelectedValue?.ToString() ?? "0";
            }

            timer1.Start();
            lblCurrentDate.Text = DateTime.Now.ToString("dddd dd-MMM-yy");
            lblCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
            txtDate.Text = DateTime.Now.ToString("dd/MMM/yyyy");

            // Wire UltraButton numpad clicks
            ultraButton3.Click += UltraNumPad_Click;  // 1
            ultraButton2.Click += UltraNumPad_Click;  // 2
            ultraButton1.Click += UltraNumPad_Click;  // 3
            ultraButton7.Click += UltraNumPad_Click;  // 4
            ultraButton6.Click += UltraNumPad_Click;  // 5
            ultraButton5.Click += UltraNumPad_Click;  // 6
            ultraButton10.Click += UltraNumPad_Click; // 7
            ultraButton9.Click += UltraNumPad_Click;  // 8
            ultraButton8.Click += UltraNumPad_Click;  // 9
            ultraButton4.Click += UltraNumPad_Click;  // 0

            // Style all UltraButtons with curved borders and light sky blue outlines
            var ultraButtons = new Infragistics.Win.Misc.UltraButton[]
            {
                ultraButton1, ultraButton2, ultraButton3, ultraButton4, ultraButton5,
                ultraButton6, ultraButton7, ultraButton8, ultraButton9, ultraButton10
            };

            foreach (var ub in ultraButtons)
            {
                ub.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
                ub.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
                ub.Appearance.BackColor = Color.DeepSkyBlue;
                ub.Appearance.BackColor2 = Color.LightSkyBlue;
                ub.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.GlassTop50;
                ub.Appearance.ForeColor = Color.White;
                ub.Appearance.BorderColor = Color.LightSkyBlue;
                ub.Font = new Font("Arial", 12F, FontStyle.Bold);
                ub.Cursor = Cursors.Hand;
                // Apply rounded corners
                ub.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, ub.Width, ub.Height, 12, 12));
            }

            // Style standard buttons (Clear, Cancel, OK) similarly
            var stdButtons = new Button[] { btnClear, btnCancel, btnOK };
            foreach (var sb in stdButtons)
            {
                sb.FlatStyle = FlatStyle.Flat;
                sb.FlatAppearance.BorderColor = Color.LightSkyBlue;
                sb.FlatAppearance.BorderSize = 2;
                sb.BackColor = Color.DeepSkyBlue;
                sb.ForeColor = Color.White;
                sb.Cursor = Cursors.Hand;
                sb.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, sb.Width, sb.Height, 12, 12));
            }
            // OK button special style
            btnOK.BackColor = Color.FromArgb(0, 191, 255);
            btnOK.ForeColor = Color.White;
            btnOK.Font = new Font("Arial", 14F, FontStyle.Bold);

            // Set initial focus to comboBox1
            this.ActiveControl = comboBox1;
            comboBox1.KeyDown += comboBox1_KeyDown;

            // Start ultraPictureBox1 slide-in animation
            picOriginalLocation = ultraPictureBox1.Location;
            ultraPictureBox1.Location = new Point(picOriginalLocation.X - 80, picOriginalLocation.Y);
            animTimer = new Timer();
            animTimer.Interval = 20;
            animTimer.Tick += AnimTimer_Tick;
            animTimer.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
            if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
            {
                lblCurrentDate.Text = DateTime.Now.ToString("dddd dd-MMM-yy");
                txtDate.Text = DateTime.Now.ToString("dd/MMM/yyyy");
            }
        }

        private void UltraNumPad_Click(object sender, EventArgs e)
        {
            if (activeTextBox != null)
            {
                var btn = sender as Infragistics.Win.Misc.UltraButton;
                if (btn != null)
                {
                    activeTextBox.Text += btn.Text;
                    activeTextBox.SelectionStart = activeTextBox.Text.Length;
                    activeTextBox.SelectionLength = 0;
                }
            }
        }

        private void btnNum_Click(object sender, EventArgs e)
        {
            if (activeTextBox != null)
            {
                Button btn = sender as Button;
                activeTextBox.Text += btn.Text;
                // Move cursor to end
                activeTextBox.SelectionStart = activeTextBox.Text.Length;
                activeTextBox.SelectionLength = 0;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (activeTextBox != null)
            {
                activeTextBox.Text = "";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Validate input fields
            if (string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                MessageBox.Show("Please enter your username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUserName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter your password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            // Validate branch selection
            if (string.IsNullOrEmpty(DataBase.BranchId) || DataBase.BranchId == "0")
            {
                MessageBox.Show("Please select a branch before logging in.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                            if (dt.Tables[0].Columns.Contains("FinYearID") && dt.Tables[0].Rows[0]["FinYearID"] != DBNull.Value)
                            {
                                DataBase.FinyearId = dt.Tables[0].Rows[0]["FinYearID"].ToString();
                            }

                            // Initialize new SessionContext
                            try
                            {
                                int branchId = Convert.ToInt32(dt.Tables[0].Rows[0][0]);
                                int companyId = Convert.ToInt32(dt.Tables[0].Rows[0][1]);
                                string emailId = dt.Tables[0].Rows[0][2].ToString();
                                int userId = Convert.ToInt32(dt.Tables[0].Rows[0][3]);
                                string userName = dt.Tables[0].Rows[0][4].ToString();
                                string userLevel = dt.Tables[0].Rows[0][5].ToString();
                                int counterId = ReadLocalCounterId();
                                string counterName = GetCounterName(counterId);

                                int finYearId = 1;
                                if (dt.Tables[0].Columns.Contains("FinYearID") && dt.Tables[0].Rows[0]["FinYearID"] != DBNull.Value)
                                {
                                    int.TryParse(dt.Tables[0].Rows[0]["FinYearID"].ToString(), out finYearId);
                                }
                                if (finYearId <= 0)
                                {
                                    finYearId = 1;
                                }

                                SessionContext.InitializeFromLogin(
                                    companyId: companyId,
                                    branchId: branchId,
                                    finYearId: finYearId,
                                    userId: userId,
                                    userName: userName,
                                    userLevel: userLevel,
                                    emailId: emailId,
                                    branchName: DataBase.Branch,
                                    counterId: counterId,
                                    counterName: counterName
                                );

                                bool isBillingUser = userLevel?.Equals("Cashier", StringComparison.OrdinalIgnoreCase) == true ||
                                                     userLevel?.Equals("Sales Man", StringComparison.OrdinalIgnoreCase) == true;

                                bool needsCounterSession = isBillingUser ||
                                                           (SessionContext.IsAdmin && counterId > 0);

                                if (needsCounterSession)
                                {
                                    if (counterId <= 0)
                                    {
                                        MessageBox.Show("Counter is not configured for this computer. Please add CounterId to C:\\Connection\\Config.txt before billing.",
                                            "Counter Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        return;
                                    }
                                    else
                                    {
                                        using (var sessionRepo = new ShiftSessionRepo())
                                        {
                                            if (!sessionRepo.StartOrResumeSession())
                                            {
                                                MessageBox.Show("Counter session could not be started. Please contact administrator.",
                                                    "Counter Session", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                return;
                                            }
                                            else if (isBillingUser && SessionContext.RequiresClosing)
                                            {
                                                MessageBox.Show("This counter has a pending closing. Please complete closing before continuing transactions.",
                                                    "Counter Closing Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            }
                                        }
                                    }
                                }

                                LoadRolePermissions(userLevel, userId);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Warning: Failed to initialize session context: {ex.Message}",
                                    "Session Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            LoadPOSSettings(Convert.ToInt32(DataBase.CompanyId), Convert.ToInt32(DataBase.BranchId));

                            try
                            {
                                using (var userActivityRepo = new UserActivityLogRepository())
                                {
                                    userActivityRepo.SaveUserActivity(
                                        userId: SessionContext.UserId,
                                        userName: SessionContext.UserName,
                                        userRole: SessionContext.UserLevel,
                                        counterId: SessionContext.CounterId,
                                        counterName: SessionContext.CounterName,
                                        activityType: "Login",
                                        activityDetails: "User logged in successfully",
                                        formName: null,
                                        sessionId: SessionContext.CounterSessionId
                                    );
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Failed to write user login log: {ex.Message}");
                            }

                            Home hm = new Home();
                            hm.FormClosed += (s, args) =>
                            {
                                if (!hm.IsLoggingOff)
                                {
                                    this.Close();
                                }
                            };
                            hm.ApplyRolePermissions();
                            hm.Show();
                            this.Hide();
                        }
                        else
                        {
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
                MessageBox.Show($"An error occurred during login:\n{ex.Message}", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);
                    dr = dt.NewRow();
                    dr.ItemArray = new object[] { 0, "--Select Branch--" };
                    dt.Rows.InsertAt(dr, 0);
                    comboBox1.ValueMember = "Id";
                    comboBox1.DisplayMember = "BranchName";
                    comboBox1.DataSource = dt;
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null && comboBox1.SelectedIndex > 0)
            {
                DataBase.Branch = comboBox1.GetItemText(comboBox1.SelectedItem);
                DataBase.BranchId = comboBox1.SelectedValue?.ToString() ?? "0";
            }
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                txtUserName.Focus();
            }
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                txtPassword.Focus();
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnOK_Click(sender, e);
            }
        }

        private void txtUserName_MouseClick(object sender, MouseEventArgs e)
        {
            activeTextBox = txtUserName;
        }

        private void txtPassword_MouseClick(object sender, MouseEventArgs e)
        {
            activeTextBox = txtPassword;
        }

        private void LoadPOSSettings(int companyId, int branchId)
        {
            try
            {
                using (var settingsRepo = new POSSettingsRepository())
                {
                    var settings = settingsRepo.GetSettings(companyId, branchId);
                    if (settings.Count == 0)
                    {
                        settingsRepo.InitializeDefaultSettings(companyId, branchId);
                        settings = settingsRepo.GetSettings(companyId, branchId);
                    }
                    SessionContext.LoadSettings(settings);
                }
            }
            catch (Exception) { }
        }

        private int ReadLocalCounterId()
        {
            const string configPath = @"C:\Connection\Config.txt";

            try
            {
                if (!System.IO.File.Exists(configPath))
                    return 0;

                string config = System.IO.File.ReadLines(configPath).FirstOrDefault() ?? string.Empty;
                string[] parts = config.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string part in parts)
                {
                    string[] keyValue = part.Split(new[] { '=' }, 2);
                    if (keyValue.Length != 2)
                        continue;

                    if (keyValue[0].Trim().Equals("CounterId", StringComparison.OrdinalIgnoreCase) &&
                        int.TryParse(keyValue[1].Trim(), out int counterId))
                    {
                        return counterId;
                    }
                }
            }
            catch
            {
                return 0;
            }

            return 0;
        }

        private string GetCounterName(int counterId)
        {
            if (counterId <= 0)
                return string.Empty;

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Counter, (SqlConnection)con.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    cmd.Parameters.AddWithValue("@CounterID", counterId);
                    if (con.DataConnection.State != ConnectionState.Open) con.DataConnection.Open();
                    object result = cmd.ExecuteScalar();
                    if (con.DataConnection.State == ConnectionState.Open) con.DataConnection.Close();

                    if (result != null && result != DBNull.Value)
                        return result.ToString();
                }
            }
            catch
            {
                if (con.DataConnection.State == ConnectionState.Open) con.DataConnection.Close();
            }

            return $"COUNTER{counterId}";
        }

        private void LoadRolePermissions(string userLevel, int userId = 0)
        {
            try
            {
                using (var permRepo = new RolePermissionRepository())
                {
                    var allRoles = permRepo.GetAllRoles();
                    int roleId = 0;
                    string matchedRoleName = userLevel;

                    // 1. Try parsing userLevel as integer ID
                    if (!string.IsNullOrWhiteSpace(userLevel) && int.TryParse(userLevel.Trim(), out int parsedId))
                    {
                        var roleObj = allRoles.FirstOrDefault(r => r.RoleID == parsedId || r.UserLevelID == parsedId);
                        if (roleObj != null)
                        {
                            roleId = roleObj.RoleID;
                            matchedRoleName = roleObj.RoleName;
                        }
                    }

                    // 2. Try matching userLevel as RoleName string
                    if (roleId <= 0 && !string.IsNullOrWhiteSpace(userLevel))
                    {
                        var roleObj = allRoles.FirstOrDefault(r => string.Equals(r.RoleName, userLevel.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (roleObj != null)
                        {
                            roleId = roleObj.RoleID;
                            matchedRoleName = roleObj.RoleName;
                        }
                    }

                    // 3. Fallback: Lookup UserLevelID from database table
                    if (roleId <= 0 && userId > 0)
                    {
                        int dbUserLevelId = GetUserLevelIdFromUsersTable(userId);
                        if (dbUserLevelId > 0)
                        {
                            var roleObj = allRoles.FirstOrDefault(r => r.RoleID == dbUserLevelId || r.UserLevelID == dbUserLevelId);
                            if (roleObj != null)
                            {
                                roleId = roleObj.RoleID;
                                matchedRoleName = roleObj.RoleName;
                            }
                            else
                            {
                                roleId = dbUserLevelId;
                            }
                        }
                    }

                    if (roleId > 0)
                    {
                        SessionContext.RoleId = roleId;
                        SessionContext.UserLevel = matchedRoleName; // Crucial: Ensures SessionContext.IsAdmin evaluates correctly!

                        var permissions = permRepo.GetPermissionsByRoleId(roleId);
                        SessionContext.LoadPermissions(permissions);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading role permissions: {ex.Message}");
            }
        }

        private int GetUserLevelIdFromUsersTable(int userId)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_User, (SqlConnection)con.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    if (con.DataConnection.State != ConnectionState.Open) con.DataConnection.Open();
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);
                        if (dt != null && dt.Rows.Count > 0 && dt.Columns.Contains("UserLevelID"))
                        {
                            return dt.Rows[0]["UserLevelID"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["UserLevelID"]) : 0;
                        }
                    }
                }
            }
            catch (Exception) { }
            return 0;
        }

        // Feature icons click handlers
        private void picSettings_Click(object sender, EventArgs e)
        {
            using (Connection connForm = new Connection())
            {
                if (connForm.ShowDialog() == DialogResult.OK)
                {
                    con = new BaseRepostitory();
                    this.RefreshBranch();

                    if (comboBox1.Items.Count > 1)
                    {
                        comboBox1.SelectedIndex = 1;
                        DataBase.Branch = comboBox1.GetItemText(comboBox1.SelectedItem);
                        DataBase.BranchId = comboBox1.SelectedValue?.ToString() ?? "0";
                    }
                }
            }
        }



        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            picAnimStep++;
            int targetX = picOriginalLocation.X;
            int currentX = ultraPictureBox1.Location.X;

            // Ease-out: move towards target, decelerating
            int diff = targetX - currentX;
            int step = Math.Max(1, diff / 4);
            int newX = currentX + step;

            if (newX >= targetX)
            {
                newX = targetX;
                animTimer.Stop();
                animTimer.Dispose();
                animTimer = null;
            }

            ultraPictureBox1.Location = new Point(newX, picOriginalLocation.Y);
        }

        public void ResetLoginFields()
        {
            txtUserName.Text = string.Empty;
            txtPassword.Text = string.Empty;
            this.ActiveControl = txtUserName;
            activeTextBox = txtUserName;
        }

    }
}
