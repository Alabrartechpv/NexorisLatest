using ModelClass;
using ModelClass.Master;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.Master
{
    public partial class FrmUsers : Form
    {
        private readonly Users users = new Users();
        private readonly UsersRepository operations = new UsersRepository();
        private readonly Dropdowns dropdowns = new Dropdowns();
        private readonly EncryptionAndDecryptionHelper enc = new EncryptionAndDecryptionHelper();
        private List<UsersDDl> _usersCache = new List<UsersDDl>();
        private int Id;
        private bool isEditMode;

        public FrmUsers()
        {
            InitializeComponent();
        }

        private void FrmUsers_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
            WireEvents();
            RefreshUserLevel();
            LoadUserGrid();
            SetFormMode(false);
        }

        private void WireEvents()
        {
            if (ultraGridUsers != null)
            {
                ultraGridUsers.ClickCell += UltraGridUsers_ClickCell;
                ultraGridUsers.DoubleClickRow += UltraGridUsers_DoubleClickRow;
                ultraGridUsers.KeyDown += UltraGridUsers_KeyDown;
            }

            if (ultraTextSearch != null)
            {
                ultraTextSearch.TextChanged += UltraTextSearch_TextChanged;
                ultraTextSearch.KeyDown += UltraTextSearch_KeyDown;
            }

            if (textUserName != null)
                textUserName.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { textPassword.Focus(); e.Handled = true; } };

            if (textPassword != null)
                textPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { textEmail.Focus(); e.Handled = true; } };

            if (textEmail != null)
                textEmail.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { cmbUserLevel.Focus(); e.Handled = true; } };

            if (cmbUserLevel != null)
                cmbUserLevel.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { Save(); e.Handled = true; } };
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F8)
            {
                Save();
                return true;
            }
            if (keyData == Keys.F1 || keyData == (Keys.Control | Keys.N))
            {
                ClearForm();
                return true;
            }
            if (keyData == (Keys.Control | Keys.B))
            {
                DeleteUser();
                return true;
            }
            if (keyData == Keys.F4 || keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void UltraTextSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                if (ultraGridUsers != null && ultraGridUsers.Rows.Count > 0)
                {
                    ultraGridUsers.ActiveRow = ultraGridUsers.Rows[0];
                    ultraGridUsers.Focus();
                    e.Handled = true;
                }
            }
        }

        private void UltraGridUsers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (ultraGridUsers != null && ultraGridUsers.ActiveRow != null)
                {
                    SelectUserFromRow(ultraGridUsers.ActiveRow);
                    textUserName.Focus();
                    e.Handled = true;
                }
            }
        }

        private void LoadUserGrid()
        {
            try
            {
                var result = dropdowns.getUsersDDl();
                _usersCache = (result?.List ?? Enumerable.Empty<UsersDDl>()).ToList();
                BindGridData(_usersCache);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user list: {ex.Message}");
            }
        }

        private void BindGridData(List<UsersDDl> list)
        {
            if (ultraGridUsers == null) return;

            ultraGridUsers.DataSource = null;
            ultraGridUsers.DataSource = list;

            if (ultraGridUsers.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGridUsers.DisplayLayout.Bands[0];
                if (band.Columns.Exists("UserID"))
                {
                    band.Columns["UserID"].Header.Caption = "User ID";
                    band.Columns["UserID"].Width = 100;
                }
                if (band.Columns.Exists("UserName"))
                {
                    band.Columns["UserName"].Header.Caption = "User Name";
                    band.Columns["UserName"].Width = 300;
                }
            }
        }

        private void UltraTextSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string search = ultraTextSearch.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(search))
                {
                    BindGridData(_usersCache);
                }
                else
                {
                    var filtered = _usersCache.Where(u =>
                        (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
                        u.UserID.ToString().Contains(search)
                    ).ToList();
                    BindGridData(filtered);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
            }
        }

        private void UltraGridUsers_ClickCell(object sender, ClickCellEventArgs e)
        {
            if (e.Cell != null && e.Cell.Row != null)
            {
                SelectUserFromRow(e.Cell.Row);
            }
        }

        private void UltraGridUsers_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row != null)
            {
                SelectUserFromRow(e.Row);
            }
        }

        private void SelectUserFromRow(UltraGridRow row)
        {
            try
            {
                if (row.Cells.Exists("UserID") && row.Cells["UserID"].Value != null)
                {
                    int userId = Convert.ToInt32(row.Cells["UserID"].Value);
                    if (userId > 0)
                    {
                        LoadUser(userId);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error selecting user row: {ex.Message}");
            }
        }

        public void RefreshUserLevel()
        {
            try
            {
                RolePermissionRepository roleRepo = new RolePermissionRepository();
                var roles = roleRepo.GetAllRoles();
                cmbUserLevel.DataSource = roles;
                cmbUserLevel.DisplayMember = "RoleName";
                cmbUserLevel.ValueMember = "RoleID";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading roles: {ex.Message}");
                MessageBox.Show("Error loading user roles. Please check database connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetCurrentCompanyId()
        {
            if (SessionContext.CompanyId > 0)
                return SessionContext.CompanyId;

            int companyId;
            return int.TryParse(DataBase.CompanyId, out companyId) ? companyId : 0;
        }

        private int GetCurrentBranchId()
        {
            if (SessionContext.BranchId > 0)
                return SessionContext.BranchId;

            int branchId;
            return int.TryParse(DataBase.BranchId, out branchId) ? branchId : 0;
        }

        #region Ribbon Actions & Public Interface

        public void Save()
        {
            if (isEditMode)
                UpdateUser();
            else
                SaveUser();
        }

        public void SaveRecord() => Save();
        public void SaveData() => Save();
        public void RibbonSave() => Save();

        public new void Update() => UpdateUser();
        public void UpdateRecord() => UpdateUser();
        public void UpdateData() => UpdateUser();

        public void Delete() => DeleteUser();
        public void DeleteRecord() => DeleteUser();
        public void RibbonDeleteInvoice() => DeleteUser();

        public void Clear() => ClearForm();
        public void ClearFields() => ClearForm();
        public void ClearRecord() => ClearForm();
        public void RibbonClear() => ClearForm();

        public void New() => ClearForm();
        public void NewRecord() => ClearForm();

        public void CloseForm() => Close();

        #endregion

        public void ClearForm()
        {
            textUserName.Clear();
            textPassword.Clear();
            textEmail.Clear();
            chkShowPassword.Checked = false;

            if (cmbUserLevel.Items.Count > 0)
                cmbUserLevel.SelectedIndex = -1;

            Id = 0;
            SetFormMode(false);
            textUserName.Focus();
        }

        private void SaveUser()
        {
            if (!ValidateUserInput(out int companyId, out int branchId, out int userLevelId))
                return;

            users.UserID = 0;
            users.CompanyID = companyId;
            users.BranchID = branchId;
            users.UserLevelID = userLevelId;
            users.UserName = textUserName.Text.Trim();
            users.Password = enc.Encrypt(textPassword.Text, true);
            users.Email = textEmail.Text.Trim();
            users._Operation = "CREATE";

            operations.SaveUser(users);
            using (frmSuccesMsg msg = new frmSuccesMsg())
            {
                msg.ShowDialog();
            }

            ClearForm();
            LoadUserGrid();
        }

        private void UpdateUser()
        {
            if (Id <= 0)
            {
                MessageBox.Show("Please select a user from the user list before updating.", "No User Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateUserInput(out int companyId, out int branchId, out int userLevelId))
                return;

            users.UserID = Id;
            users.CompanyID = companyId;
            users.BranchID = branchId;
            users.UserLevelID = userLevelId;
            users.UserName = textUserName.Text.Trim();
            users.Email = textEmail.Text.Trim();
            users.Password = enc.Encrypt(textPassword.Text, true);
            users._Operation = "UPDATE";

            operations.Update(users);
            MessageBox.Show("User Update Success", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearForm();
            LoadUserGrid();
        }

        private void DeleteUser()
        {
            if (Id <= 0)
            {
                MessageBox.Show("Please select a user from the user list before deleting.", "No User Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete user '{textUserName.Text}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            Users deletedUser = operations.Delete(Id);
            if (deletedUser != null)
            {
                MessageBox.Show("Record deleted successfully.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadUserGrid();
            }
            else
            {
                MessageBox.Show("Error deleting record.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateUserInput(out int companyId, out int branchId, out int userLevelId)
        {
            companyId = GetCurrentCompanyId();
            branchId = GetCurrentBranchId();
            userLevelId = 0;

            if (string.IsNullOrWhiteSpace(textUserName.Text))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textUserName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textPassword.Text))
            {
                MessageBox.Show("Please enter a password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textPassword.Focus();
                return false;
            }

            if (companyId <= 0)
            {
                MessageBox.Show("Current company is not available. Please login again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (branchId <= 0)
            {
                MessageBox.Show("Current branch is not available. Please login again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbUserLevel.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a user level.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbUserLevel.Focus();
                return false;
            }

            int selectedRoleId = Convert.ToInt32(cmbUserLevel.Value ?? 0);
            if (selectedRoleId <= 0)
            {
                MessageBox.Show("No role selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            RolePermissionRepository roleRepo = new RolePermissionRepository();
            var selectedRole = roleRepo.GetAllRoles().FirstOrDefault(r => r.RoleID == selectedRoleId);
            if (selectedRole != null && selectedRole.UserLevelID.HasValue)
            {
                userLevelId = selectedRole.UserLevelID.Value;
            }
            else
            {
                userLevelId = selectedRoleId;
            }

            return true;
        }

        private void OpenUsersList()
        {
            using (FrmUsersList usersList = new FrmUsersList())
            {
                if (usersList.ShowDialog(this) == DialogResult.OK && usersList.SelectedUserId > 0)
                {
                    LoadUser(usersList.SelectedUserId);
                }
            }
        }

        private void LoadUser(int userId)
        {
            try
            {
                Users usr = operations.GetById(userId);
                if (usr == null)
                {
                    MessageBox.Show("User data not found in database.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ClearForm();
                    return;
                }

                Id = usr.UserID;
                textUserName.Text = usr.UserName ?? "";
                textEmail.Text = usr.Email ?? "";
                
                try
                {
                    textPassword.Text = enc.Decrypt(usr.Password, true);
                }
                catch
                {
                    textPassword.Text = usr.Password ?? "";
                }

                if (usr.UserLevelID > 0 && cmbUserLevel.DataSource is System.Collections.Generic.List<Role> roles)
                {
                    Role matchingRole = roles.FirstOrDefault(r => r.UserLevelID == usr.UserLevelID);
                    if (matchingRole != null)
                        cmbUserLevel.Value = matchingRole.RoleID;
                }

                SetFormMode(true);
                textUserName.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearForm();
            }
        }

        private void SetFormMode(bool editMode)
        {
            isEditMode = editMode;
            if (labelModeStatus == null)
                return;

            string userName = textUserName.Text.Trim();
            labelModeStatus.Text = editMode && Id > 0
                ? $"Editing User: {(userName == string.Empty ? Id.ToString() : userName)}"
                : "New User";
            labelModeStatus.ForeColor = editMode
                ? System.Drawing.Color.FromArgb(198, 111, 0)
                : System.Drawing.Color.FromArgb(0, 102, 184);
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            textPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '*';
        }


        private void FrmUsers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                Save();
            }
            else if (e.KeyCode == Keys.F4 || e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }


        private void textEmail_Validating(object sender, CancelEventArgs e)
        {
            if (textEmail.Text.Trim() == string.Empty)
                return;

            Regex expression = new Regex(@"^([a-zA-Z0-9_\-])([a-zA-Z0-9_\-\.]*)@(\[((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\.){3}|((([a-zA-Z0-9\-]+)\.)+))([a-zA-Z]{2,}|(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\])$");

            if (!expression.IsMatch(textEmail.Text.Trim()))
            {
                MessageBox.Show("E-mail address format is not correct.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textEmail.Focus();
            }
        }
    }
}
