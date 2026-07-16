using ModelClass;
using ModelClass.Master;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;


namespace PosBranch_Win.Master
{
    public partial class FrmBranch : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        Branch branch = new Branch();
        Dropdowns drop = new Dropdowns();
        BranchRepository operations = new BranchRepository();
        int Id;


        // UltraGrid columns will be configured in FormatGrid method

        public FrmBranch()
        {
            InitializeComponent();
        }

        // Event handlers for labels - can be removed if not needed
        private void ultraLabelAddress_Click(object sender, EventArgs e)
        {

        }

        private void ultraLabelBranch_Click(object sender, EventArgs e)
        {

        }

        private void FormatGrid()
        {
            try
            {
                // Configure main grid appearance with frmSalesInvoice styling
                ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;

                // Configure header appearance with modern gradient look (exactly like frmSalesInvoice)
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = System.Drawing.Color.FromArgb(0, 102, 184);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Reset appearance settings (like frmSalesInvoice)
                ultraGrid1.DisplayLayout.Override.CellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.RowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Reset();

                // Set key navigation and selection properties (like frmSalesInvoice)
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCell = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCol = Infragistics.Win.UltraWinGrid.SelectType.None;
                ultraGrid1.DisplayLayout.TabNavigation = Infragistics.Win.UltraWinGrid.TabNavigation.NextCell;

                // Enable row selection and activation
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;

                // Set basic row appearance
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = System.Drawing.Color.White;

                // Configure active row highlighting (professional blue like frmSalesInvoice)
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = System.Drawing.Color.LightSkyBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = System.Drawing.Color.Black;

                // Remove active cell highlighting (like frmSalesInvoice)
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.BackColor = System.Drawing.Color.Empty;
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.ForeColor = System.Drawing.Color.Black;

                // Configure scrolling and layout (like frmSalesInvoice)
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;

                // Configure spacing and expansion behavior (like frmSalesInvoice)
                ultraGrid1.DisplayLayout.InterBandSpacing = 10;
                ultraGrid1.DisplayLayout.Override.ExpansionIndicator = Infragistics.Win.UltraWinGrid.ShowExpansionIndicator.Never;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;

                // Set row sizing to fixed height for compact appearance
                ultraGrid1.DisplayLayout.Override.RowSizing = Infragistics.Win.UltraWinGrid.RowSizing.Fixed;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 25;
                ultraGrid1.DisplayLayout.Override.CellPadding = 2;

                // Configure columns after data is loaded
                ConfigureGridColumns();

                // Connect grid events
                ConnectGridEvents();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error formatting grid: {ex.Message}");
            }
        }

        private void ConfigureGridColumns()
        {
            try
            {
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    var band = ultraGrid1.DisplayLayout.Bands[0];

                    // Id column - styled like frmSalesInvoice SlNo column
                    if (band.Columns.Exists("Id"))
                    {
                        band.Columns["Id"].Header.Caption = "ID";
                        band.Columns["Id"].Width = 60;
                        band.Columns["Id"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        band.Columns["Id"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["Id"].MinWidth = 50;

                        // Reset any appearance settings for the ID column specifically (like frmSalesInvoice)
                        band.Columns["Id"].CellAppearance.BackColor = System.Drawing.Color.Empty;
                        band.Columns["Id"].CellAppearance.BackColor2 = System.Drawing.Color.Empty;
                        band.Columns["Id"].CellAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    }

                    // BranchName column - styled like frmSalesInvoice ItemName column
                    if (band.Columns.Exists("BranchName"))
                    {
                        band.Columns["BranchName"].Header.Caption = "Branch Name";
                        band.Columns["BranchName"].Width = 350;
                        band.Columns["BranchName"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                        band.Columns["BranchName"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["BranchName"].MinWidth = 200;
                    }

                    // Hide any other columns that may not be needed for branch display
                    // This follows the pattern from frmSalesInvoice of hiding unnecessary columns
                    foreach (var column in band.Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
                    {
                        if (column.Key != "Id" && column.Key != "BranchName")
                        {
                            // You can choose to hide other columns or configure them as needed
                            // For now, we'll keep them visible but set minimum width
                            if (!column.Hidden)
                            {
                                column.MinWidth = 50;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configuring grid columns: {ex.Message}");
            }
        }

        private void ConnectGridEvents()
        {
            try
            {
                // Add keyboard event handler for navigation
                ultraGrid1.KeyDown += UltraGrid1_KeyDown;

                // Add row selection event
                ultraGrid1.AfterRowActivate += UltraGrid1_AfterRowActivate;

                // Add cell click event for better row selection
                ultraGrid1.ClickCell += UltraGrid1_ClickCell;

                // Add double-click row event for better user experience
                ultraGrid1.DoubleClickRow += UltraGrid1_DoubleClickRow;

                // Add selection change event for better row selection tracking
                ultraGrid1.AfterSelectChange += UltraGrid1_AfterSelectChange;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error connecting grid events: {ex.Message}");
            }
        }

        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter && ultraGrid1.ActiveRow != null)
                {
                    // Handle Enter key press
                    DisplayRowValues(ultraGrid1.ActiveRow);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Space && ultraGrid1.ActiveRow != null)
                {
                    // Handle Space key press for row selection
                    DisplayRowValues(ultraGrid1.ActiveRow);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in grid key handler: {ex.Message}");
            }
        }

        private void UltraGrid1_AfterRowActivate(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    // Handle row activation for selection
                    DisplayRowValues(ultraGrid1.ActiveRow);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in row activate handler: {ex.Message}");
            }
        }

        private void UltraGrid1_ClickCell(object sender, Infragistics.Win.UltraWinGrid.ClickCellEventArgs e)
        {
            try
            {
                if (e.Cell != null && e.Cell.Row != null)
                {
                    // Use the DisplayRowValues method for consistency
                    DisplayRowValues(e.Cell.Row);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in cell click handler: {ex.Message}");
            }
        }

        private void UltraGrid1_DoubleClickRow(object sender, Infragistics.Win.UltraWinGrid.DoubleClickRowEventArgs e)
        {
            try
            {
                if (e.Row != null)
                {
                    // Use the DisplayRowValues method for consistency
                    DisplayRowValues(e.Row);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in double-click row handler: {ex.Message}");
            }
        }

        private void UltraGrid1_AfterSelectChange(object sender, Infragistics.Win.UltraWinGrid.AfterSelectChangeEventArgs e)
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    // Handle selection change for better row tracking
                    DisplayRowValues(ultraGrid1.ActiveRow);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in selection change handler: {ex.Message}");
            }
        }


        private void FrmBranch_Load(object sender, EventArgs e)
        {
            this.FormatGrid();
            this.RefreshCompany();
            KeyPreview = true;

            btnUpdate.Visible = false;
            this.RefreshBranch();

            // Add form closing event handler
            this.FormClosing += FrmBranch_FormClosing;
        }

        private void FrmBranch_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Check if there are unsaved changes
            if (HasUnsavedChanges())
            {
                DialogResult result = MessageBox.Show("You have unsaved changes. Do you want to save them before closing?", "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        if (btnUpdate.Visible)
                            button1_Click(sender, e);
                        else
                            SaveMaster();
                    }
                    catch
                    {
                        // If save fails, don't close
                        e.Cancel = true;
                        return;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                // If No, close without saving
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ultraTextName.Clear();
            ultraTextPhn.Clear();
            ultraTextAddress.Clear();
            ultraComboCompany.Value = null;
            Id = 0;
            originalValues = null;

            // Reset button visibility
            btnSave.Visible = true;
            btnUpdate.Visible = false;
        }

        public void Save()
        {
            btnSave_Click(this, EventArgs.Empty);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (btnSave.Visible)
            {
                this.SaveMaster();
            }
            else if (btnUpdate.Visible)
            {
                button1_Click(sender, e);
            }
        }
        private void SaveMaster()
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(ultraTextName.Text))
                {
                    MessageBox.Show("Please enter Branch Name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraTextName.Focus();
                    return;
                }

                if (ultraComboCompany.Value == null)
                {
                    MessageBox.Show("Please select a Company.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraComboCompany.Focus();
                    return;
                }

                // Check if all fields are empty (no data to save)
                if (string.IsNullOrWhiteSpace(ultraTextName.Text.Trim()) &&
                    string.IsNullOrWhiteSpace(ultraTextAddress.Text.Trim()) &&
                    string.IsNullOrWhiteSpace(ultraTextPhn.Text.Trim()))
                {
                    MessageBox.Show("Please enter at least Branch Name to save.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraTextName.Focus();
                    return;
                }

                branch.Id = 0;
                branch.CompanyId = Convert.ToInt32(ultraComboCompany.Value);
                branch.BranchName = ultraTextName.Text.Trim();
                branch.Address = ultraTextAddress.Text.Trim();
                branch.Phone = ultraTextPhn.Text.Trim();
                branch.FinYearId = SessionContext.FinYearId;
                branch._Operation = "CREATE";

                string message = operations.SaveBranch(branch);

                frmSuccesMsg msg = new frmSuccesMsg();
                msg.ShowDialog();

                // Clear form after successful save
                btnClear_Click(null, null);
                this.RefreshBranch();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving branch: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public void RefreshBranch()
        {
            try
            {
                BranchDDlGrid brnchgrid = drop.getBanchDDl();
                ultraGrid1.DataSource = brnchgrid.List;

                // Configure columns after data is loaded
                ConfigureGridColumns();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing branch: {ex.Message}");
            }
        }
        private void ultraGrid1_CellContentClick(object sender, Infragistics.Win.UltraWinGrid.CellEventArgs e)
        {
            try
            {
                if (e.Cell != null && e.Cell.Row != null)
                {
                    // Use the DisplayRowValues method for consistency
                    DisplayRowValues(e.Cell.Row);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting branch: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RefreshCompany()
        {
            CompanyDDlGrid grid = drop.CompanyDDl();
            ultraComboCompany.DataSource = grid.List;
            ultraComboCompany.DisplayMember = "CompanyName";
            ultraComboCompany.ValueMember = "CompanyID";
        }

        private void ultraComboCompany_ValueChanged(object sender, EventArgs e)
        {
            // Handle company selection change
        }
        private void DisplayRowValues(Infragistics.Win.UltraWinGrid.UltraGridRow row)
        {
            try
            {
                if (row != null && row.Cells.Count > 0)
                {
                    // Get values by column name instead of index
                    if (row.Cells.Exists("Id"))
                    {
                        int selectedId = Convert.ToInt32(row.Cells["Id"].Value);
                        Branch bran = operations.GetById(selectedId);

                        if (bran != null)
                        {
                            Id = bran.Id;
                            ultraTextName.Text = bran.BranchName;
                            ultraTextAddress.Text = bran.Address;
                            ultraTextPhn.Text = bran.Phone;
                            ultraComboCompany.Value = bran.CompanyId;

                            // Store original values for change detection
                            StoreOriginalValues(bran);

                            // Show update button, hide save button
                            btnSave.Visible = false;
                            btnUpdate.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error displaying row values: {ex.Message}");
            }
        }

        private Branch originalValues = null;

        private void StoreOriginalValues(Branch branch)
        {
            originalValues = new Branch
            {
                Id = branch.Id,
                BranchName = branch.BranchName,
                Address = branch.Address,
                Phone = branch.Phone,
                CompanyId = branch.CompanyId
            };
        }


        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.ActiveCell = ultraGrid1.Rows[0].Cells[0];

                    // Display the selected row values
                    DisplayRowValues(ultraGrid1.Rows[0]);
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.ActiveCell = ultraGrid1.Rows[0].Cells[0];
                    ultraGrid1.Focus();
                }
            }
        }

        private void ultraGrid1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                UltraGridRow selectedRow = ultraGrid1.ActiveRow;
                if (selectedRow != null)
                {
                    DisplayRowValues(selectedRow);
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ultraTextSearch.Text))
                {
                    // If search is empty, refresh with all branches
                    this.RefreshBranch();
                }
                else
                {
                    // Perform search
                    BranchDDlGrid brgrid = operations.SearchBranch(ultraTextSearch.Text);
                    ultraGrid1.DataSource = brgrid.List;

                    // Configure columns after search results are loaded
                    ConfigureGridColumns();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in search: {ex.Message}");
            }
        }

        private void FrmBranch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                SaveMaster();
                this.RefreshBranch();
            }
            else if (e.KeyCode == Keys.F4)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // Check if there are unsaved changes
                if (HasUnsavedChanges())
                {
                    DialogResult result = MessageBox.Show("You have unsaved changes. Do you want to save them before clearing?", "Unsaved Changes",
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        if (btnUpdate.Visible)
                            button1_Click(sender, e);
                        else
                            SaveMaster();
                    }
                    else if (result == DialogResult.No)
                    {
                        btnClear_Click(sender, e);
                    }
                    // If Cancel, do nothing
                }
                else
                {
                    btnClear_Click(sender, e);
                }
            }
        }

        private bool HasUnsavedChanges()
        {
            if (originalValues == null)
                return false;

            return (originalValues.BranchName != ultraTextName.Text.Trim() ||
                    originalValues.Address != ultraTextAddress.Text.Trim() ||
                    originalValues.Phone != ultraTextPhn.Text.Trim() ||
                    originalValues.CompanyId != Convert.ToInt32(ultraComboCompany.Value ?? 0));
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    int selectedId = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["Id"].Value);

                    // Confirm deletion
                    DialogResult result = MessageBox.Show("Are you sure you want to delete this branch?", "Confirm Delete",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        Branch bran = operations.Delete(selectedId);

                        if (bran != null)
                        {
                            MessageBox.Show("Record deleted successfully.");

                            // Clear the form after successful deletion
                            btnClear_Click(sender, e);
                            btnSave.Visible = true;
                            btnUpdate.Visible = false;
                        }
                        else
                        {
                            MessageBox.Show("Error deleting record.");
                        }

                        this.RefreshBranch();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a record to delete.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (Id == 0)
                {
                    MessageBox.Show("Please select a branch to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(ultraTextName.Text))
                {
                    MessageBox.Show("Please enter Branch Name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraTextName.Focus();
                    return;
                }

                if (ultraComboCompany.Value == null)
                {
                    MessageBox.Show("Please select a Company.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraComboCompany.Focus();
                    return;
                }

                // Check if any changes were made using stored original values
                if (originalValues != null)
                {
                    bool hasChanges = false;

                    if (originalValues.BranchName != ultraTextName.Text.Trim())
                        hasChanges = true;
                    if (originalValues.Address != ultraTextAddress.Text.Trim())
                        hasChanges = true;
                    if (originalValues.Phone != ultraTextPhn.Text.Trim())
                        hasChanges = true;
                    if (originalValues.CompanyId != Convert.ToInt32(ultraComboCompany.Value))
                        hasChanges = true;

                    if (!hasChanges)
                    {
                        MessageBox.Show("No changes were made to update.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Please select a branch to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                branch.Id = Id;
                branch.CompanyId = Convert.ToInt32(ultraComboCompany.Value);
                branch.BranchName = ultraTextName.Text.Trim();
                branch.Address = ultraTextAddress.Text.Trim();
                branch.Phone = ultraTextPhn.Text.Trim();
                branch._Operation = "Update";

                Branch message = operations.UpdateBranch(branch);
                MessageBox.Show("Branch Update Success", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear form and refresh
                btnClear_Click(sender, e);
                this.RefreshBranch();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating branch: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
