using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class FrmFormSearch : Form
    {
        private static FrmFormSearch _activeInstance = null;

        public class FormSearchItem
        {
            public Type FormType { get; set; }
            public string DisplayName { get; set; }
            public string Category { get; set; }
            public string ClassName { get; set; }
            public string SearchKey { get; set; }
        }

        private List<FormSearchItem> _allForms = new List<FormSearchItem>();
        private List<FormSearchItem> _filteredForms = new List<FormSearchItem>();
        private Form _parentHome = null;

        public FrmFormSearch(Form parentHome = null)
        {
            InitializeComponent();
            _parentHome = parentHome;
            LoadAllApplicationForms();

            this.Shown += FrmFormSearch_Shown;
        }

        private void FrmFormSearch_Shown(object sender, EventArgs e)
        {
            txtSearch.Focus();
            txtSearch.SelectAll();
        }

        /// <summary>
        /// Displays the Form Search popup menu. If already open, brings it to front.
        /// </summary>
        public static void ShowFormSearch(Form parentHome)
        {
            try
            {
                if (_activeInstance != null && !_activeInstance.IsDisposed)
                {
                    _activeInstance.BringToFront();
                    _activeInstance.Activate();
                    _activeInstance.txtSearch.Focus();
                    _activeInstance.txtSearch.SelectAll();
                    return;
                }

                _activeInstance = new FrmFormSearch(parentHome);
                _activeInstance.StartPosition = FormStartPosition.CenterScreen;
                _activeInstance.Show(parentHome);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing FormSearch dialog: {ex.Message}");
            }
        }

        /// <summary>
        /// Dynamically discovers all Form types in the application using Reflection.
        /// Guaranteed to automatically include any future forms added to the assembly!
        /// </summary>
        private void LoadAllApplicationForms()
        {
            try
            {
                _allForms.Clear();
                Assembly executingAssembly = Assembly.GetExecutingAssembly();

                var formTypes = executingAssembly.GetTypes()
                    .Where(t => typeof(Form).IsAssignableFrom(t)
                             && !t.IsAbstract
                             && !t.IsGenericTypeDefinition
                             && t.GetConstructor(Type.EmptyTypes) != null
                             && t != typeof(FrmFormSearch)
                             && t != typeof(Home))
                    .OrderBy(t => t.Name)
                    .ToList();

                foreach (Type formType in formTypes)
                {
                    string displayName = GetFriendlyFormName(formType);
                    string category = ExtractCategory(formType);

                    FormSearchItem item = new FormSearchItem
                    {
                        FormType = formType,
                        DisplayName = displayName,
                        Category = category,
                        ClassName = formType.Name,
                        SearchKey = $"{displayName} {category} {formType.Name}".ToLower()
                    };

                    _allForms.Add(item);
                }

                // Initial populate
                FilterAndDisplayForms(string.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scanning assembly for forms: {ex.Message}");
            }
        }

        /// <summary>
        /// Formats class name into a clean, human-readable display name.
        /// </summary>
        public static string GetFriendlyFormName(Type formType)
        {
            if (formType == null) return string.Empty;
            string name = formType.Name;

            // Known specific friendly titles
            if (string.Equals(name, "frmSalesInvoice", StringComparison.OrdinalIgnoreCase)) return "Sales Invoice";
            if (string.Equals(name, "FrmPurchase", StringComparison.OrdinalIgnoreCase)) return "Purchase Entry";
            if (string.Equals(name, "frmPurchaseReturn", StringComparison.OrdinalIgnoreCase)) return "Purchase Return";
            if (string.Equals(name, "FrmStockAdjustment", StringComparison.OrdinalIgnoreCase)) return "Stock Adjustment";
            if (string.Equals(name, "frmItemMasterNew", StringComparison.OrdinalIgnoreCase)) return "Item Master";
            if (string.Equals(name, "frmPurchaseOrder", StringComparison.OrdinalIgnoreCase)) return "Purchase Order";
            if (string.Equals(name, "FrmVendor", StringComparison.OrdinalIgnoreCase)) return "Vendor Master";
            if (string.Equals(name, "FrmCustomer", StringComparison.OrdinalIgnoreCase)) return "Customer Master";
            if (string.Equals(name, "FrmRolePermissions", StringComparison.OrdinalIgnoreCase)) return "Role Permissions";
            if (string.Equals(name, "FrmUserActivityLog", StringComparison.OrdinalIgnoreCase)) return "User Activity Log";

            // Strip leading 'frm' or 'Frm'
            if (name.StartsWith("frm", StringComparison.OrdinalIgnoreCase) && name.Length > 3)
            {
                name = name.Substring(3);
            }

            // Insert space before capital letters
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (i > 0 && char.IsUpper(c) && (!char.IsUpper(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]))))
                {
                    sb.Append(' ');
                }
                sb.Append(c);
            }

            string formatted = sb.ToString().Trim();
            return string.IsNullOrEmpty(formatted) ? formType.Name : formatted;
        }

        /// <summary>
        /// Categorizes forms dynamically by namespace or name.
        /// </summary>
        private static string ExtractCategory(Type formType)
        {
            string ns = formType.Namespace ?? string.Empty;
            if (ns.Contains("Transaction")) return "Transaction";
            if (ns.Contains("Master")) return "Master";
            if (ns.Contains("Reports")) return "Reports";
            if (ns.Contains("Accounts")) return "Accounts";
            if (ns.Contains("Settings")) return "Settings";
            if (ns.Contains("Utilities")) return "Utilities";
            if (ns.Contains("DialogBox")) return "Dialog";
            return "General";
        }

        private void FilterAndDisplayForms(string searchText)
        {
            try
            {
                gridResults.Rows.Clear();
                string query = (searchText ?? string.Empty).Trim().ToLower();

                if (string.IsNullOrEmpty(query))
                {
                    _filteredForms = _allForms.ToList();
                }
                else
                {
                    _filteredForms = _allForms
                        .Where(item => item.SearchKey.Contains(query))
                        .ToList();
                }

                foreach (var item in _filteredForms)
                {
                    int rowIndex = gridResults.Rows.Add(item.DisplayName, item.Category, item.ClassName);
                    gridResults.Rows[rowIndex].Tag = item;
                }

                lblStatus.Text = $"Found {_filteredForms.Count} form(s) matching search query.";

                if (gridResults.Rows.Count > 0)
                {
                    gridResults.Rows[0].Selected = true;
                    gridResults.CurrentCell = gridResults.Rows[0].Cells[0];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error filtering forms: {ex.Message}");
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterAndDisplayForms(txtSearch.Text);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (gridResults.Rows.Count > 0)
                {
                    int currentIndex = gridResults.CurrentRow != null ? gridResults.CurrentRow.Index : -1;
                    int nextIndex = Math.Min(currentIndex + 1, gridResults.Rows.Count - 1);
                    gridResults.Rows[nextIndex].Selected = true;
                    gridResults.CurrentCell = gridResults.Rows[nextIndex].Cells[0];
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (gridResults.Rows.Count > 0)
                {
                    int currentIndex = gridResults.CurrentRow != null ? gridResults.CurrentRow.Index : 0;
                    int prevIndex = Math.Max(currentIndex - 1, 0);
                    gridResults.Rows[prevIndex].Selected = true;
                    gridResults.CurrentCell = gridResults.Rows[prevIndex].Cells[0];
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OpenSelectedForm();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                this.Close();
            }
        }

        private void gridResults_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OpenSelectedForm();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                this.Close();
            }
        }

        private void gridResults_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                OpenSelectedForm();
            }
        }

        private void OpenSelectedForm()
        {
            if (gridResults.CurrentRow == null)
                return;

            FormSearchItem selectedItem = gridResults.CurrentRow.Tag as FormSearchItem;
            if (selectedItem == null || selectedItem.FormType == null)
                return;

            this.Close();

            try
            {
                Home homeInstance = _parentHome as Home ?? Application.OpenForms.OfType<Home>().FirstOrDefault();
                if (homeInstance != null && !homeInstance.IsDisposed)
                {
                    homeInstance.OpenFormByType(selectedItem.FormType, selectedItem.DisplayName);
                }
                else
                {
                    Form form = (Form)Activator.CreateInstance(selectedItem.FormType);
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening form '{selectedItem.DisplayName}': {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
