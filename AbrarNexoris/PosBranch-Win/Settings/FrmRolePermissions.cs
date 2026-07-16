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

            // Load roles into combo box
            LoadRoles();

            // Configure DataGridView
            ConfigureGrid();
        }

        /// <summary>
        /// Loads all active roles into the combo box
        /// </summary>
        private void LoadRoles()
        {
            try
            {
                var roles = _permRepo.GetAllRoles();
                cmbRoles.DataSource = roles;
                cmbRoles.DisplayMember = "RoleName";
                cmbRoles.ValueMember = "RoleID";

                if (roles.Count > 0)
                {
                    cmbRoles.SelectedIndex = 0;
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
            if (cmbRoles.SelectedValue != null && cmbRoles.SelectedValue is int roleId)
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
                dgvPermissions.DataSource = null;
                dgvPermissions.DataSource = _currentPermissions;

                lblStatus.Text = $"Loaded {_currentPermissions.Count} forms for role: {cmbRoles.Text}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading permissions: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                // Update the list from the grid
                _currentPermissions = (List<FormPermissionGrid>)dgvPermissions.DataSource;

                // Save to database
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
            lblStatus.Text = "All permissions granted. Click Save to apply.";
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
            lblStatus.Text = "All permissions revoked. Click Save to apply.";
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
