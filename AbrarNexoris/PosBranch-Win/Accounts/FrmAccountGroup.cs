using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.Accounts;
using ModelClass.Accounts;
using ModelClass.Master;
using ModelClass;

namespace PosBranch_Win.Accounts
{
    public partial class FrmAccountGroup : Form
    {
        private AccountGroupRepository accountGroupRepo;

        public FrmAccountGroup()
        {
            InitializeComponent();
            accountGroupRepo = new AccountGroupRepository();
            this.KeyPreview = true;

            this.Load += FrmAccountGroup_Load;
            btnSearchGroup.Click += BtnSearchGroup_Click;

            // Setup uppercase formatting for text inputs
            ultratxtAccName.CharacterCasing = CharacterCasing.Upper;
            ultratxtAccDescription.CharacterCasing = CharacterCasing.Upper;

            // Hover effects
            btnSearchGroup.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            btnSearchGroup.HotTrackAppearance.BackColor = Color.FromArgb(206, 231, 247);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F8 || keyData == (Keys.Control | Keys.S))
            {
                Save();
                return true;
            }
            if (keyData == Keys.F1 || keyData == (Keys.Control | Keys.N))
            {
                Clear();
                return true;
            }
            if (keyData == Keys.F3 || keyData == (Keys.Control | Keys.F))
            {
                BtnSearchGroup_Click(null, null);
                return true;
            }
            if (keyData == Keys.F4 || keyData == (Keys.Control | Keys.D))
            {
                Delete();
                return true;
            }
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void FrmAccountGroup_Load(object sender, EventArgs e)
        {
            try
            {
                LoadParentGroup();
                LoadAccountCategories();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing form: " + ex.Message, "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadParentGroup()
        {
            try
            {
                AccountGroupHeadDDL parentGroups = accountGroupRepo.GetAllParentGroups();

                if (parentGroups != null && parentGroups.List != null && parentGroups.List.Any())
                {
                    DataTable dtParentGroups = new DataTable();
                    dtParentGroups.Columns.Add("GroupID", typeof(int));
                    dtParentGroups.Columns.Add("GroupName", typeof(string));

                    // Add [None] option at the top
                    dtParentGroups.Rows.Add(0, "[None]");

                    foreach (var group in parentGroups.List)
                    {
                        dtParentGroups.Rows.Add(group.GroupID, group.GroupName);
                    }

                    ultraDrpParentGroup.DataSource = dtParentGroups;
                    ultraDrpParentGroup.DisplayMember = "GroupName";
                    ultraDrpParentGroup.ValueMember = "GroupID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading parent groups: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAccountCategories()
        {
            try
            {
                DataTable dtCategories = new DataTable();
                dtCategories.Columns.Add("CategoryID", typeof(int));
                dtCategories.Columns.Add("CategoryName", typeof(string));

                var accountHeads = accountGroupRepo.LoadAccountCategories();

                if (accountHeads != null && accountHeads.List != null && accountHeads.List.Any())
                {
                    foreach (var accountHead in accountHeads.List)
                    {
                        dtCategories.Rows.Add(accountHead.AcHeadId, accountHead.AcHeadName);
                    }
                }
                else
                {
                    // Default categories if database is empty
                    dtCategories.Rows.Add(1, "Asset");
                    dtCategories.Rows.Add(2, "Liability");
                    dtCategories.Rows.Add(3, "Income");
                    dtCategories.Rows.Add(4, "Expense");
                    dtCategories.Rows.Add(5, "Equity");
                }

                ultraDrpAccCategory.DataSource = dtCategories;
                ultraDrpAccCategory.DisplayMember = "CategoryName";
                ultraDrpAccCategory.ValueMember = "CategoryID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading account categories: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            ultratxtAccName.Text = string.Empty;
            ultratxtAccDescription.Text = string.Empty;
            ultratxtAccCode.Tag = null;

            if (ultraDrpParentGroup.Items.Count > 0)
                ultraDrpParentGroup.SelectedIndex = 0;

            if (ultraDrpAccCategory.Items.Count > 0)
                ultraDrpAccCategory.SelectedIndex = 0;

            GenerateNextGroupID();
            ultratxtAccName.Focus();
        }

        private void GenerateNextGroupID()
        {
            try
            {
                int nextId = accountGroupRepo.GetNextGroupID();
                ultratxtAccCode.Text = nextId.ToString();
                ultratxtAccCode.ReadOnly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting next ID: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(ultratxtAccCode.Text))
            {
                MessageBox.Show("Account Code is missing.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ultratxtAccName.Text))
            {
                MessageBox.Show("Please enter an Account Group Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultratxtAccName.Focus();
                return false;
            }

            if (ultraDrpAccCategory.SelectedIndex == -1 || ultraDrpAccCategory.Value == null)
            {
                MessageBox.Show("Please select an Account Category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraDrpAccCategory.Focus();
                return false;
            }

            if (ultraDrpParentGroup.SelectedIndex == -1 || ultraDrpParentGroup.Value == null)
            {
                MessageBox.Show("Please select a Parent Group (or select [None]).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraDrpParentGroup.Focus();
                return false;
            }

            // Duplicate Validation
            string groupName = ultratxtAccName.Text.Trim();
            int branchId = SessionContext.BranchId;
            int excludeId = ultratxtAccCode.Tag != null ? Convert.ToInt32(ultratxtAccCode.Tag) : 0;

            if (accountGroupRepo.IsAccountGroupNameExists(groupName, branchId, excludeId))
            {
                MessageBox.Show($"An Account Group with the name '{groupName}' already exists.", "Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultratxtAccName.Focus();
                return false;
            }

            return true;
        }

        public void Save()
        {
            try
            {
                if (!ValidateInput()) return;

                int accountCode = Convert.ToInt32(ultratxtAccCode.Text.Trim());
                string accountName = ultratxtAccName.Text.Trim();
                string description = ultratxtAccDescription.Text.Trim();
                int branchID = SessionContext.BranchId; // Secured branch fetch

                int parentGroupID = Convert.ToInt32(ultraDrpParentGroup.Value);
                int accountCategoryID = Convert.ToInt32(ultraDrpAccCategory.Value);
                string groupType = ultraDrpAccCategory.Text.Trim();
                
                string groupUnder = parentGroupID > 0 ? $"{parentGroupID}/{accountCode}" : $"0/{accountCode}";

                AccountGroupHead accountGroup = new AccountGroupHead
                {
                    GroupID = accountCode,
                    GroupName = accountName,
                    Description = description,
                    BranchID = branchID,
                    GroupCategoryID = accountCategoryID,
                    ParentGroupId = parentGroupID,
                    GroupType = groupType,
                    GroupUnder = groupUnder
                };

                if (ultratxtAccCode.Tag != null)
                {
                    // Confirm Update
                    if (MessageBox.Show("Are you sure you want to update this account group?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        return;

                    accountGroup.GroupID = Convert.ToInt32(ultratxtAccCode.Tag);
                    if (accountGroupRepo.UpdateAccountGroup(accountGroup))
                    {
                        MessageBox.Show("Account group updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                    }
                }
                else
                {
                    // Confirm Save
                    if (MessageBox.Show("Are you sure you want to save this new account group?", "Confirm Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        return;

                    if (accountGroupRepo.CreateAccountGroup(accountGroup))
                    {
                        MessageBox.Show("Account group added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                    }
                }
                
                // Refresh parent groups dropdown in case a new one was added
                LoadParentGroup();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving account group: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Clear()
        {
            ClearForm();
        }

        public void Delete()
        {
            if (ultratxtAccCode.Tag == null)
            {
                MessageBox.Show("Please select an account group to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this account group?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (accountGroupRepo.DeleteAccountGroup(Convert.ToInt32(ultratxtAccCode.Tag)))
                {
                    MessageBox.Show("Account group deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadParentGroup();
                }
                else
                {
                    MessageBox.Show("Failed to delete account group. It may be in use.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSearchGroup_Click(object sender, EventArgs e)
        {
            using (FrmAccountGroupSearch searchForm = new FrmAccountGroupSearch())
            {
                if (searchForm.ShowDialog() == DialogResult.OK && searchForm.SelectedGroupId > 0)
                {
                    LoadGroupById(searchForm.SelectedGroupId);
                }
            }
        }

        public void LoadGroupById(int groupId)
        {
            try
            {
                DataRow row = accountGroupRepo.GetAccountGroupById(groupId);

                if (row != null)
                {
                    ultratxtAccCode.Text = row["GroupID"].ToString();
                    ultratxtAccName.Text = row["GroupName"].ToString();
                    ultratxtAccDescription.Text = (row.Table.Columns.Contains("Description") && row["Description"] != DBNull.Value) ? row["Description"].ToString() : string.Empty;
                    ultratxtAccCode.ReadOnly = true;

                    // Set Parent Group
                    int parentId = 0;
                    if (row.Table.Columns.Contains("ParentGroupId") && row["ParentGroupId"] != DBNull.Value)
                        parentId = Convert.ToInt32(row["ParentGroupId"]);
                    else if (row.Table.Columns.Contains("ParentGroupID") && row["ParentGroupID"] != DBNull.Value)
                        parentId = Convert.ToInt32(row["ParentGroupID"]);

                    ultraDrpParentGroup.Value = parentId;

                    // Set Account Category - First try by GroupCategoryID ID directly
                    bool categorySet = false;
                    if (row.Table.Columns.Contains("GroupCategoryID") && row["GroupCategoryID"] != DBNull.Value)
                    {
                        int catId = Convert.ToInt32(row["GroupCategoryID"]);
                        if (catId > 0)
                        {
                            ultraDrpAccCategory.Value = catId;
                            categorySet = (ultraDrpAccCategory.Value != null && Convert.ToInt32(ultraDrpAccCategory.Value) == catId);
                        }
                    }

                    // Fallback to text matching if ID matching did not set the category
                    if (!categorySet)
                    {
                        string catName = string.Empty;
                        if (row.Table.Columns.Contains("GroupCategoryName") && row["GroupCategoryName"] != DBNull.Value)
                            catName = row["GroupCategoryName"].ToString();
                        else if (row.Table.Columns.Contains("AccountCategory") && row["AccountCategory"] != DBNull.Value)
                            catName = row["AccountCategory"].ToString();
                        else if (row.Table.Columns.Contains("GroupType") && row["GroupType"] != DBNull.Value)
                            catName = row["GroupType"].ToString();

                        if (!string.IsNullOrWhiteSpace(catName))
                        {
                            SelectComboItemByText(ultraDrpAccCategory, catName);
                        }
                    }

                    ultratxtAccCode.Tag = groupId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading account group: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectComboItemByText(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            string target = text.Trim();

            // Try matching ValueListItem DisplayText (case-insensitive)
            foreach (Infragistics.Win.ValueListItem item in combo.Items)
            {
                if (string.Equals(item.DisplayText?.Trim(), target, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedItem = item;
                    return;
                }
            }

            // Fallback for DataBound UltraComboEditor: iterate rows in DataSource if DataTable
            if (combo.DataSource is DataTable dt)
            {
                string displayCol = string.IsNullOrEmpty(combo.DisplayMember) ? dt.Columns[1].ColumnName : combo.DisplayMember;
                string valueCol = string.IsNullOrEmpty(combo.ValueMember) ? dt.Columns[0].ColumnName : combo.ValueMember;

                foreach (DataRow dr in dt.Rows)
                {
                    string dispText = dr[displayCol]?.ToString()?.Trim();
                    if (string.Equals(dispText, target, StringComparison.OrdinalIgnoreCase))
                    {
                        combo.Value = dr[valueCol];
                        return;
                    }
                }
            }

            // Final fallback
            try
            {
                combo.Text = target;
            }
            catch { }
        }
    }
}
