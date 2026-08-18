using ModelClass;
using ModelClass.Master;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.MasterRepositry;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PosBranch_Win.Master
{
    public partial class FrmUsers : Form
    {
        private readonly Users users = new Users();
        private readonly UsersRepository operations = new UsersRepository();
        private readonly EncryptionAndDecryptionHelper enc = new EncryptionAndDecryptionHelper();
        private int Id;
        private bool isEditMode;

        public FrmUsers()
        {
            InitializeComponent();
            Resize += FrmUsers_Resize;
        }

        private void FrmUsers_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
            LayoutUserManagementForm();
            RefreshUserLevel();
            SetFormMode(false);
        }

        private void FrmUsers_Resize(object sender, EventArgs e)
        {
            LayoutUserManagementForm();
        }

        private void LayoutUserManagementForm()
        {
            if (ultraPanel1 == null || ultraGroupBoxEntry == null)
                return;

            int clientWidth = ultraPanel1.ClientArea.Width;
            if (clientWidth <= 0)
                return;

            int formWidth = Math.Min(Math.Max(clientWidth - 96, 820), 1040);
            int formHeight = 320;
            int formLeft = Math.Max(24, (clientWidth - formWidth) / 2);
            int formTop = ultraLabelTitle.Height + Math.Max(34, (ultraPanel1.ClientArea.Height - ultraLabelTitle.Height - formHeight) / 3);

            ultraGroupBoxEntry.SetBounds(formLeft, formTop, formWidth, formHeight);

            int labelWidth = 100;
            int inputHeight = 27;
            int buttonWidth = 112;
            int inputGap = 18;
            int columnGap = 58;
            int rowTop = 88;
            int rowGap = 66;
            int leftLabelX = 70;
            int leftInputX = leftLabelX + labelWidth + inputGap;
            int availableWidth = formWidth - leftInputX - 70;
            int columnWidth = (availableWidth - columnGap) / 2;
            int rightLabelX = leftInputX + columnWidth + columnGap;
            int rightInputX = rightLabelX + labelWidth + inputGap;
            int leftInputWidth = Math.Max(260, columnWidth);
            int rightInputWidth = Math.Max(220, formWidth - rightInputX - 70);

            labelModeStatus.SetBounds(leftLabelX, 38, 220, labelModeStatus.Height);

            labelUserName.SetBounds(leftLabelX, rowTop + 3, labelWidth, labelUserName.Height);
            textUserName.SetBounds(leftInputX, rowTop, leftInputWidth, inputHeight);
            labelRequiredName.SetBounds(leftInputX - 18, rowTop + 3, labelRequiredName.Width, labelRequiredName.Height);

            labelEmail.SetBounds(leftLabelX, rowTop + rowGap + 3, labelWidth, labelEmail.Height);
            textEmail.SetBounds(leftInputX, rowTop + rowGap, leftInputWidth, inputHeight);

            labelPassword.SetBounds(rightLabelX, rowTop + 3, labelWidth, labelPassword.Height);
            textPassword.SetBounds(rightInputX, rowTop, rightInputWidth, inputHeight);
            labelRequiredPassword.SetBounds(rightInputX - 18, rowTop + 3, labelRequiredPassword.Width, labelRequiredPassword.Height);
            chkShowPassword.SetBounds(rightInputX, rowTop + 33, 130, chkShowPassword.Height);

            labelUserLevel.SetBounds(rightLabelX, rowTop + rowGap + 3, labelWidth, labelUserLevel.Height);
            cmbUserLevel.SetBounds(rightInputX, rowTop + rowGap, rightInputWidth, inputHeight);
            labelRequiredLevel.SetBounds(rightInputX - 18, rowTop + rowGap + 3, labelRequiredLevel.Width, labelRequiredLevel.Height);

            int footerTop = formHeight - 56;
            labelRequiredNote.SetBounds(leftLabelX, footerTop, 150, labelRequiredNote.Height);
            labelShortcutHint.SetBounds(leftLabelX, footerTop + 24, 190, labelShortcutHint.Height);
            btnUsersList.SetBounds(formWidth - buttonWidth - 70, footerTop + 2, buttonWidth, 31);
            btnClearForm.SetBounds(formWidth - (buttonWidth * 2) - 84, footerTop + 2, buttonWidth, 31);
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
                MessageBox.Show("Error loading user roles. Please check the database connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        public void Save()
        {
            if (isEditMode)
                UpdateUser();
            else
                SaveUser();
        }

        public void UpdateRecord()
        {
            UpdateUser();
        }

        public void DeleteRecord()
        {
            DeleteUser();
        }

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
                MessageBox.Show("Please select a valid user role.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbUserLevel.Focus();
                return false;
            }

            RolePermissionRepository roleRepo = new RolePermissionRepository();
            var selectedRole = roleRepo.GetAllRoles().FirstOrDefault(r => r.RoleID == selectedRoleId);
            userLevelId = (selectedRole != null && selectedRole.UserLevelID.HasValue && selectedRole.UserLevelID.Value > 0)
                ? selectedRole.UserLevelID.Value
                : selectedRoleId;

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
                textPassword.Text = usr.Password ?? "";

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

        private void btnClearForm_Click(object sender, EventArgs e)
        {
            ClearForm();
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

        private void btnUsersList_Click(object sender, EventArgs e)
        {
            OpenUsersList();
        }

        private void textEmail_Validating(object sender, CancelEventArgs e)
        {
            if (textEmail.Text.Trim() == string.Empty)
                return;

            Regex expression = new Regex(@"^([a-zA-Z0-9_\-])([a-zA-Z0-9_\-\.]*)@(\[((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\.){3}|((([a-zA-Z0-9\-]+)\.)+))([a-zA-Z]{2,}|(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\])$");

            if (!expression.IsMatch(textEmail.Text.Trim()))
            {
                MessageBox.Show("E-mail address format is not correct.", "MojoCRM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textEmail.Focus();
            }
        }
    }
}
