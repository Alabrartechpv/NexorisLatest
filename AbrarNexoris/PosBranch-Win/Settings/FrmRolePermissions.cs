using ModelClass;
using ModelClass.Master;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    /// <summary>
    /// Role Permission Management Form.
    /// Allows administrators to configure permissions (View, Add, Edit, Delete) for each role.
    /// </summary>
    public partial class FrmRolePermissions : Form
    {
        private RolePermissionRepository _permRepo;
        private List<FormPermissionGrid> _currentPermissions;
        private int _selectedRoleId = 0;

        public FrmRolePermissions()
        {
            InitializeComponent();
            _permRepo = new RolePermissionRepository();
            _currentPermissions = new List<FormPermissionGrid>();
        }

        private void FrmRolePermissions_Load(object sender, EventArgs e)
        {
            // Set form appearance
            this.Text = "Role Permission Management";

            // Configure DataGridView
            ConfigureGrid();

            // Load categories filter dropdown
            LoadCategoryFilter();

            // Load roles into combo box
            LoadRoles();

            // Apply explicit button colors
            ApplyButtonStyles();

            // Event handlers for filtering
            cmbCategoryFilter.SelectedIndexChanged += (s, ev) => ApplyFilter();
            txtSearchForm.TextChanged += (s, ev) => ApplyFilter();
            btnGrantViewAll.Click += BtnGrantViewAll_Click;

            LanguageManager.ApplyLanguageToForm(this);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyButtonStyles();
            if (_selectedRoleId <= 0 && cmbRoles.Items.Count > 0)
            {
                cmbRoles.SelectedIndex = 0;
                cmbRoles_SelectedIndexChanged(cmbRoles, EventArgs.Empty);
            }
        }

        private void ApplyButtonStyles()
        {
            pnlButtons.BackColor = Color.FromArgb(230, 240, 250);

            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.BackColor = Color.FromArgb(0, 120, 215);
            btnSave.ForeColor = Color.White;
            btnSave.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            btnGrantViewAll.FlatStyle = FlatStyle.Flat;
            btnGrantViewAll.FlatAppearance.BorderSize = 0;
            btnGrantViewAll.BackColor = Color.FromArgb(23, 162, 184);
            btnGrantViewAll.ForeColor = Color.White;
            btnGrantViewAll.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            btnGrantAll.FlatStyle = FlatStyle.Flat;
            btnGrantAll.FlatAppearance.BorderSize = 0;
            btnGrantAll.BackColor = Color.FromArgb(40, 167, 69);
            btnGrantAll.ForeColor = Color.White;
            btnGrantAll.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            btnRevokeAll.FlatStyle = FlatStyle.Flat;
            btnRevokeAll.FlatAppearance.BorderSize = 0;
            btnRevokeAll.BackColor = Color.FromArgb(220, 53, 69);
            btnRevokeAll.ForeColor = Color.White;
            btnRevokeAll.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.BackColor = Color.FromArgb(108, 117, 125);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.BackColor = Color.FromArgb(52, 58, 64);
            btnClose.ForeColor = Color.White;
            btnClose.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        }

        private void LoadCategoryFilter()
        {
            cmbCategoryFilter.Items.Clear();
            cmbCategoryFilter.Items.Add("All Categories");
            cmbCategoryFilter.Items.Add("Master");
            cmbCategoryFilter.Items.Add("Transaction");
            cmbCategoryFilter.Items.Add("Accounts");
            cmbCategoryFilter.Items.Add("Reports");
            cmbCategoryFilter.Items.Add("Settings");
            cmbCategoryFilter.Items.Add("Utilities");
            cmbCategoryFilter.SelectedIndex = 0;
        }

        /// <summary>
        /// Loads all active roles into the combo box
        /// </summary>
        private void LoadRoles()
        {
            try
            {
                var roles = _permRepo.GetAllRoles();
                cmbRoles.DataSource = null;
                cmbRoles.DisplayMember = "RoleName";
                cmbRoles.ValueMember = "RoleID";
                cmbRoles.DataSource = roles;

                if (roles != null && roles.Count > 0)
                {
                    cmbRoles.SelectedIndex = 0;
                    cmbRoles_SelectedIndexChanged(cmbRoles, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading roles: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Configures the DataGridView for displaying permissions
        /// </summary>
        private void ConfigureGrid()
        {
            dgvPermissions.AutoGenerateColumns = false;
            dgvPermissions.Columns.Clear();

            // Add columns
            dgvPermissions.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FormID",
                Name = "FormID",
                Visible = false
            });

            dgvPermissions.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FormKey",
                Name = "FormKey",
                Visible = false
            });

            dgvPermissions.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Category",
                Name = "Category",
                HeaderText = "Category",
                Width = 100,
                ReadOnly = true
            });

            dgvPermissions.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FormName",
                Name = "FormName",
                HeaderText = "Form / Module",
                Width = 180,
                ReadOnly = true
            });

            dgvPermissions.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "CanView",
                Name = "CanView",
                HeaderText = "View",
                Width = 60
            });

            dgvPermissions.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "CanAdd",
                Name = "CanAdd",
                HeaderText = "Add",
                Width = 60
            });

            dgvPermissions.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "CanEdit",
                Name = "CanEdit",
                HeaderText = "Edit",
                Width = 60
            });

            dgvPermissions.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "CanDelete",
                Name = "CanDelete",
                HeaderText = "Delete",
                Width = 60
            });

            // Style the grid
            dgvPermissions.RowHeadersVisible = false;
            dgvPermissions.AllowUserToAddRows = false;
            dgvPermissions.AllowUserToDeleteRows = false;
            dgvPermissions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPermissions.BackgroundColor = Color.White;
            dgvPermissions.GridColor = Color.LightGray;

            // Header style
            dgvPermissions.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            dgvPermissions.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPermissions.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvPermissions.EnableHeadersVisualStyles = false;

            // Alternating row colors
            dgvPermissions.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
        }

        /// <summary>
        /// Called when selected role changes - loads permissions for selected role
        /// </summary>
        private void cmbRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            int roleId = 0;

            if (cmbRoles.SelectedValue != null)
            {
                if (cmbRoles.SelectedValue is int idInt)
                {
                    roleId = idInt;
                }
                else if (int.TryParse(cmbRoles.SelectedValue.ToString(), out int parsedId))
                {
                    roleId = parsedId;
                }
            }

            if (roleId <= 0 && cmbRoles.SelectedItem is Role r)
            {
                roleId = r.RoleID;
            }

            if (roleId > 0)
            {
                _selectedRoleId = roleId;
                LoadPermissionsForRole(roleId);
            }
        }

        /// <summary>
        /// Loads permissions for the selected role
        /// </summary>
        /// <param name="roleId">Role ID to load permissions for</param>
        private void LoadPermissionsForRole(int roleId)
        {
            try
            {
                _currentPermissions = _permRepo.GetFormsWithPermissions(roleId);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading permissions: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilter()
        {
            if (_currentPermissions == null) return;

            string selectedCategory = cmbCategoryFilter.SelectedItem?.ToString() ?? "All Categories";
            string searchText = txtSearchForm.Text?.Trim().ToLower() ?? "";

            var filtered = _currentPermissions.Where(p =>
            {
                bool matchCat = selectedCategory == "All Categories" ||
                                string.Equals(p.Category?.Trim(), selectedCategory.Trim(), StringComparison.OrdinalIgnoreCase);
                bool matchSearch = string.IsNullOrEmpty(searchText) ||
                                   (p.FormName != null && p.FormName.ToLower().Contains(searchText)) ||
                                   (p.FormKey != null && p.FormKey.ToLower().Contains(searchText));
                return matchCat && matchSearch;
            }).ToList();

            dgvPermissions.DataSource = null;
            dgvPermissions.DataSource = filtered;

            string roleText = cmbRoles.Text;
            if (string.IsNullOrWhiteSpace(roleText) && cmbRoles.SelectedItem is Role r)
            {
                roleText = r.RoleName;
            }

            lblStatus.Text = $"Showing {filtered.Count} of {_currentPermissions.Count} forms for role: {roleText}";
        }

        /// <summary>
        /// Saves permissions for the selected role
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_selectedRoleId <= 0)
            {
                MessageBox.Show("Please select a role first.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_currentPermissions == null || _currentPermissions.Count == 0)
                {
                    MessageBox.Show("No permissions available to save.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save master permissions list to database
                string result = _permRepo.SavePermissions(_selectedRoleId, _currentPermissions);

                if (result == "Success")
                {
                    MessageBox.Show("Permissions saved successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    lblStatus.Text = $"Permissions saved for role: {cmbRoles.Text}";
                }
                else
                {
                    MessageBox.Show("Failed to save permissions.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving permissions: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGrantViewAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvPermissions.Rows)
            {
                row.Cells["CanView"].Value = true;
            }
            lblStatus.Text = "View permission granted for displayed forms. Click Save to apply.";
        }

        /// <summary>
        /// Grants all permissions to the selected role
        /// </summary>
        private void btnGrantAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvPermissions.Rows)
            {
                row.Cells["CanView"].Value = true;
                row.Cells["CanAdd"].Value = true;
                row.Cells["CanEdit"].Value = true;
                row.Cells["CanDelete"].Value = true;
            }
            lblStatus.Text = "All permissions granted for displayed forms. Click Save to apply.";
        }

        /// <summary>
        /// Revokes all permissions from the selected role
        /// </summary>
        private void btnRevokeAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvPermissions.Rows)
            {
                row.Cells["CanView"].Value = false;
                row.Cells["CanAdd"].Value = false;
                row.Cells["CanEdit"].Value = false;
                row.Cells["CanDelete"].Value = false;
            }
            lblStatus.Text = "All permissions revoked for displayed forms. Click Save to apply.";
        }

        /// <summary>
        /// Clears and reloads permissions for current role
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (_selectedRoleId > 0)
            {
                LoadPermissionsForRole(_selectedRoleId);
            }
        }

        /// <summary>
        /// Closes the form
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
