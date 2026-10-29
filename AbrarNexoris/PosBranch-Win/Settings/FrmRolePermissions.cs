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
            if (chkShowCheckedOnly != null) chkShowCheckedOnly.CheckedChanged += (s, ev) => ApplyFilter();
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

            if (btnSyncForms != null)
            {
                btnSyncForms.FlatStyle = FlatStyle.Flat;
                btnSyncForms.FlatAppearance.BorderSize = 0;
                btnSyncForms.BackColor = Color.FromArgb(111, 66, 193);
                btnSyncForms.ForeColor = Color.White;
                btnSyncForms.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            }

            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.BackColor = Color.FromArgb(52, 58, 64);
            btnClose.ForeColor = Color.White;
            btnClose.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        }

        private void LoadCategoryFilter()
        {
            string currentSelected = cmbCategoryFilter.SelectedItem?.ToString();
            cmbCategoryFilter.Items.Clear();
            cmbCategoryFilter.Items.Add("All Categories");
            cmbCategoryFilter.Items.Add("Granted / Checked Only");

            var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Master", "Transaction", "Accounts", "Reports", "Settings", "Utilities"
            };

            if (_currentPermissions != null)
            {
                foreach (var p in _currentPermissions)
                {
                    if (!string.IsNullOrWhiteSpace(p.Category))
                        categories.Add(p.Category.Trim());
                }
            }

            foreach (var cat in categories.OrderBy(c => c))
            {
                cmbCategoryFilter.Items.Add(cat);
            }

            if (!string.IsNullOrEmpty(currentSelected) && cmbCategoryFilter.Items.Contains(currentSelected))
                cmbCategoryFilter.SelectedItem = currentSelected;
            else
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
                Width = 110,
                ReadOnly = true
            });

            dgvPermissions.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FormName",
                Name = "FormName",
                HeaderText = "Form / Module",
                Width = 240,
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
            dgvPermissions.CellFormatting += DgvPermissions_CellFormatting;
            dgvPermissions.CurrentCellDirtyStateChanged += DgvPermissions_CurrentCellDirtyStateChanged;
            dgvPermissions.CellValueChanged += DgvPermissions_CellValueChanged;
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
                string roleText = cmbRoles.Text?.Trim();
                if (string.IsNullOrWhiteSpace(roleText) && cmbRoles.SelectedItem is Role r) roleText = r.RoleName?.Trim();
                _currentPermissions = _permRepo.GetFormsWithPermissions(roleId, roleText);
                LoadCategoryFilter();
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
            bool showCheckedOnly = (chkShowCheckedOnly != null && chkShowCheckedOnly.Checked) ||
                                   string.Equals(selectedCategory, "Granted / Checked Only", StringComparison.OrdinalIgnoreCase);

            var filtered = _currentPermissions.Where(p =>
            {
                bool isChecked = p.CanView || p.CanAdd || p.CanEdit || p.CanDelete;
                if (showCheckedOnly && !isChecked) return false;
                bool matchCat = selectedCategory == "All Categories" || selectedCategory == "Granted / Checked Only" ||
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

            UpdateStatusCounts(filtered.Count);
        }

        /// <summary>
        /// Saves permissions for the selected role
        /// </summary>
        private void UpdateStatusCounts(int filteredCount = -1)
        {
            if (_currentPermissions == null) return;

            int total = _currentPermissions.Count;
            int checkedCount = _currentPermissions.Count(p => p.CanView || p.CanAdd || p.CanEdit || p.CanDelete);
            int vCount = _currentPermissions.Count(p => p.CanView);
            int aCount = _currentPermissions.Count(p => p.CanAdd);
            int eCount = _currentPermissions.Count(p => p.CanEdit);
            int dCount = _currentPermissions.Count(p => p.CanDelete);

            string roleText = cmbRoles.Text;
            if (string.IsNullOrWhiteSpace(roleText) && cmbRoles.SelectedItem is Role r)
            {
                roleText = r.RoleName;
            }

            int showing = filteredCount >= 0 ? filteredCount : dgvPermissions.Rows.Count;
            lblStatus.Text = $"Role: {roleText} | Checked / Granted: {checkedCount}/{total} (View: {vCount}, Add: {aCount}, Edit: {eCount}, Delete: {dCount}) | Showing: {showing} forms";
        }

        private void DgvPermissions_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvPermissions.Rows.Count) return;
            var row = dgvPermissions.Rows[e.RowIndex];
            if (row.DataBoundItem is FormPermissionGrid item)
            {
                bool isGranted = item.CanView || item.CanAdd || item.CanEdit || item.CanDelete;
                if (isGranted)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(238, 252, 238);
                    if (e.ColumnIndex >= 0 && e.ColumnIndex < dgvPermissions.Columns.Count && dgvPermissions.Columns[e.ColumnIndex].Name == "FormName")
                    {
                        e.CellStyle.Font = new Font(dgvPermissions.Font, FontStyle.Bold);
                        e.CellStyle.ForeColor = Color.FromArgb(0, 100, 0);
                    }
                }
            }
        }

        private void DgvPermissions_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvPermissions.IsCurrentCellDirty && dgvPermissions.CurrentCell is DataGridViewCheckBoxCell)
            {
                dgvPermissions.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvPermissions_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _currentPermissions != null)
            {
                dgvPermissions.InvalidateRow(e.RowIndex);
                UpdateStatusCounts();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbRoles.SelectedValue != null && int.TryParse(cmbRoles.SelectedValue.ToString(), out int cboVal) && cboVal > 0) _selectedRoleId = cboVal; else if (cmbRoles.SelectedItem is Role selRole && selRole.RoleID > 0) _selectedRoleId = selRole.RoleID;
            if (_selectedRoleId <= 0)
            {
                MessageBox.Show("Please select a role first.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                dgvPermissions.EndEdit();

                if (_currentPermissions == null || _currentPermissions.Count == 0)
                {
                    MessageBox.Show("No permissions available to save.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Sync visible grid rows to _currentPermissions
                foreach (DataGridViewRow row in dgvPermissions.Rows)
                {
                    if (row.DataBoundItem is FormPermissionGrid gridItem)
                    {
                        if (row.Cells["CanView"].Value != null)
                            gridItem.CanView = Convert.ToBoolean(row.Cells["CanView"].Value);
                        if (row.Cells["CanAdd"].Value != null)
                            gridItem.CanAdd = Convert.ToBoolean(row.Cells["CanAdd"].Value);
                        if (row.Cells["CanEdit"].Value != null)
                            gridItem.CanEdit = Convert.ToBoolean(row.Cells["CanEdit"].Value);
                        if (row.Cells["CanDelete"].Value != null)
                            gridItem.CanDelete = Convert.ToBoolean(row.Cells["CanDelete"].Value);
                    }
                }

                // Save master permissions list to database
                string result = _permRepo.SavePermissions(_selectedRoleId, _currentPermissions, cmbRoles.Text?.Trim());

                if (result == "Success")
                {
                    MessageBox.Show("Permissions saved successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    lblStatus.Text = $"Permissions saved for role: {cmbRoles.Text}";

                    try
                    {
                        var targetIds = _permRepo.GetTargetRoleIds(_selectedRoleId, cmbRoles.Text?.Trim());
                        if (targetIds.Contains(SessionContext.RoleId) || string.Equals(cmbRoles.Text?.Trim(), SessionContext.UserLevel?.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            var updatedPerms = _permRepo.GetPermissionsByRoleId(SessionContext.RoleId, SessionContext.UserLevel?.Trim());
                            SessionContext.LoadPermissions(updatedPerms);
                            foreach (Form f in Application.OpenForms)
                            {
                                if (f is Home homeForm)
                                {
                                    homeForm.ApplyRolePermissions();
                                }
                            }
                        }
                    }
                    catch { }
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
            if (_currentPermissions != null) { foreach (var p in _currentPermissions) { p.CanView = true; } }
            BtnGrantViewAll_Impl(sender, e);
        }

        private void BtnGrantViewAll_Impl(object sender, EventArgs e)
        {
            dgvPermissions.EndEdit();
            foreach (DataGridViewRow row in dgvPermissions.Rows)
            {
                row.Cells["CanView"].Value = true;
                if (row.DataBoundItem is FormPermissionGrid item)
                    item.CanView = true;
            }
            dgvPermissions.Invalidate();
            UpdateStatusCounts();
        }

        /// <summary>
        /// Grants all permissions to the selected role
        /// </summary>
        private void btnGrantAll_Click(object sender, EventArgs e)
        {
            if (_currentPermissions != null) { foreach (var p in _currentPermissions) { p.CanView = true; p.CanAdd = true; p.CanEdit = true; p.CanDelete = true; } }
            btnGrantAll_Impl(sender, e);
        }

        private void btnGrantAll_Impl(object sender, EventArgs e)
        {
            dgvPermissions.EndEdit();
            foreach (DataGridViewRow row in dgvPermissions.Rows)
            {
                row.Cells["CanView"].Value = true;
                row.Cells["CanAdd"].Value = true;
                row.Cells["CanEdit"].Value = true;
                row.Cells["CanDelete"].Value = true;
                if (row.DataBoundItem is FormPermissionGrid item)
                {
                    item.CanView = true;
                    item.CanAdd = true;
                    item.CanEdit = true;
                    item.CanDelete = true;
                }
            }
            dgvPermissions.Invalidate();
            UpdateStatusCounts();
        }

        /// <summary>
        /// Revokes all permissions from the selected role
        /// </summary>
        private void btnRevokeAll_Click(object sender, EventArgs e)
        {
            if (_currentPermissions != null) { foreach (var p in _currentPermissions) { p.CanView = false; p.CanAdd = false; p.CanEdit = false; p.CanDelete = false; } }
            btnRevokeAll_Impl(sender, e);
        }

        private void btnRevokeAll_Impl(object sender, EventArgs e)
        {
            dgvPermissions.EndEdit();
            foreach (DataGridViewRow row in dgvPermissions.Rows)
            {
                row.Cells["CanView"].Value = false;
                row.Cells["CanAdd"].Value = false;
                row.Cells["CanEdit"].Value = false;
                row.Cells["CanDelete"].Value = false;
                if (row.DataBoundItem is FormPermissionGrid item)
                {
                    item.CanView = false;
                    item.CanAdd = false;
                    item.CanEdit = false;
                    item.CanDelete = false;
                }
            }
            dgvPermissions.Invalidate();
            UpdateStatusCounts();
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
        /// Handles adding newly added forms (auto-scan or manual add)
        /// </summary>
        private void btnSyncForms_Click(object sender, EventArgs e)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Font = new Font("Segoe UI", 9.5f);

            var itemScan = new ToolStripMenuItem("🔍 Auto-Scan & Add All Project Forms", null, (s, ev) => AutoScanAndSyncProjectForms());
            var itemManual = new ToolStripMenuItem("➕ Manually Register New Form / Module...", null, (s, ev) => ShowManualAddFormDialog());

            menu.Items.Add(itemScan);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemManual);

            menu.Show(btnSyncForms, new Point(0, btnSyncForms.Height));
        }

        private void AutoScanAndSyncProjectForms()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                var assembly = typeof(FrmRolePermissions).Assembly;
                var formTypes = assembly.GetTypes()
                    .Where(t => typeof(Form).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericTypeDefinition)
                    .ToList();

                var discoveredForms = new List<FormPermissionGrid>();

                foreach (var t in formTypes)
                {
                    string typeName = t.Name;
                    string ns = t.Namespace ?? "";

                    // Exclude internal helper types
                    if (typeName.StartsWith("<") || typeName.Contains("Anon") || typeName == "FloatingTabHostForm")
                        continue;

                    // Determine Category from namespace or type name
                    string category = "Utilities";
                    if (ns.IndexOf("Reports", StringComparison.OrdinalIgnoreCase) >= 0 || typeName.IndexOf("Report", StringComparison.OrdinalIgnoreCase) >= 0)
                        category = "Reports";
                    else if (ns.IndexOf("Master", StringComparison.OrdinalIgnoreCase) >= 0)
                        category = "Master";
                    else if (ns.IndexOf("Transaction", StringComparison.OrdinalIgnoreCase) >= 0)
                        category = "Transaction";
                    else if (ns.IndexOf("Accounts", StringComparison.OrdinalIgnoreCase) >= 0 || ns.IndexOf("Account", StringComparison.OrdinalIgnoreCase) >= 0)
                        category = "Accounts";
                    else if (ns.IndexOf("Settings", StringComparison.OrdinalIgnoreCase) >= 0)
                        category = "Settings";

                    // Generate clean form key
                    string key = typeName.ToLowerInvariant();
                    if (key.StartsWith("frm")) key = key.Substring(3);

                    // Generate friendly title
                    string friendlyName = GenerateFriendlyFormName(typeName);

                    discoveredForms.Add(new FormPermissionGrid
                    {
                        FormKey = key,
                        FormName = friendlyName,
                        Category = category
                    });
                }

                int synced = _permRepo.SyncAllDiscoveredForms(discoveredForms);

                if (_selectedRoleId > 0)
                {
                    LoadPermissionsForRole(_selectedRoleId);
                }

                LoadCategoryFilter();

                MessageBox.Show($"Scanned {discoveredForms.Count} project forms.\n{synced} forms synchronized with database successfully!",
                    "Forms Synchronization", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error syncing project forms: {ex.Message}", "Sync Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private static string GenerateFriendlyFormName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return "";
            string name = typeName;
            if (name.StartsWith("Frm", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(3);

            // Insert spaces between CamelCase words
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]) && (!char.IsUpper(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]))))
                {
                    sb.Append(' ');
                }
                sb.Append(name[i]);
            }
            return sb.ToString();
        }

        private void ShowManualAddFormDialog()
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "Register New Form / Module";
                dlg.Size = new Size(420, 260);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.White;

                Label lblName = new Label { Text = "Form / Module Name:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
                TextBox txtName = new TextBox { Location = new Point(20, 42), Size = new Size(360, 24), Font = new Font("Segoe UI", 9.5f) };

                Label lblKey = new Label { Text = "Form Key / Tool Key:", Location = new Point(20, 75), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
                TextBox txtKey = new TextBox { Location = new Point(20, 97), Size = new Size(360, 24), Font = new Font("Segoe UI", 9.5f) };

                Label lblCat = new Label { Text = "Category:", Location = new Point(20, 130), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
                ComboBox cmbCat = new ComboBox { Location = new Point(20, 152), Size = new Size(360, 24), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5f) };
                cmbCat.Items.AddRange(new object[] { "Reports", "Master", "Transaction", "Accounts", "Settings", "Utilities" });
                cmbCat.SelectedIndex = 0;

                txtName.TextChanged += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtKey.Text) || txtKey.Tag == null)
                    {
                        txtKey.Text = txtName.Text.Replace(" ", "").ToLowerInvariant();
                        txtKey.Tag = null;
                    }
                };
                txtKey.KeyDown += (s, e) => txtKey.Tag = "manual";

                Button btnAdd = new Button
                {
                    Text = "Register Form",
                    DialogResult = DialogResult.OK,
                    Location = new Point(160, 190),
                    Size = new Size(110, 30),
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                };
                btnAdd.FlatAppearance.BorderSize = 0;

                Button btnCancel = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(280, 190),
                    Size = new Size(100, 30),
                    BackColor = Color.Gray,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                };
                btnCancel.FlatAppearance.BorderSize = 0;

                dlg.Controls.AddRange(new Control[] { lblName, txtName, lblKey, txtKey, lblCat, cmbCat, btnAdd, btnCancel });
                dlg.AcceptButton = btnAdd;
                dlg.CancelButton = btnCancel;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    string formName = txtName.Text.Trim();
                    string formKey = txtKey.Text.Trim();
                    string category = cmbCat.SelectedItem?.ToString() ?? "Reports";

                    if (string.IsNullOrEmpty(formName))
                    {
                        MessageBox.Show("Please enter a form name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (string.IsNullOrEmpty(formKey))
                        formKey = formName.Replace(" ", "").ToLowerInvariant();

                    bool registered = _permRepo.RegisterNewForm(formKey, formName, category);
                    if (registered)
                    {
                        if (_selectedRoleId > 0)
                        {
                            LoadPermissionsForRole(_selectedRoleId);
                        }
                        LoadCategoryFilter();
                        MessageBox.Show($"Form '{formName}' registered successfully and added to permissions list!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to register new form in database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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
