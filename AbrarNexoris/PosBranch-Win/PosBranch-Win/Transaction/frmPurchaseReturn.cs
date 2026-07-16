using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Repository;
using ModelClass.Master;
using PosBranch_Win.DialogBox;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win;
using Infragistics.Win.Misc;
using ModelClass;
using ModelClass.TransactionModels;
using System.Data.SqlClient;

namespace PosBranch_Win.Transaction
{
    public partial class frmPurchaseReturn : Form
    {
        // Add repository instance at class level
        private Repository.TransactionRepository.PurchaseReturnRepository prRepo = new Repository.TransactionRepository.PurchaseReturnRepository();

        // Add Dropdowns instance to access branch data
        private Dropdowns drop = new Dropdowns();

        // Flag to track if purchase items are loaded from btnAddPurchaceList
        private bool _isPurchaseDataLoaded = false;

        // Flag to track original ReadOnly state of TxtBarcode
        private bool _originalTxtBarcodeReadOnly = false;

        // Track the currently loaded purchase number to prevent duplicates
        private int _currentlyLoadedPurchaseNo = -1;

        public frmPurchaseReturn()
        {
            InitializeComponent();

            // Set KeyPreview to true to enable form-level key handling
            this.KeyPreview = true;

            // Load branch data when form initializes
            LoadBranchData();

            // Add event handler for cmbBranch click event
            cmbBranch.Click += new EventHandler(cmbBranch_Click);

            // Add KeyDown event handler for TxtBarcode
            TxtBarcode.KeyDown += new KeyEventHandler(TxtBarcode_KeyDown);

            // Add form load event handler
            this.Load += new EventHandler(frmPurchaseReturn_Load);

            // Initialize the grid tag
            ultraGrid1.Tag = "NormalMode";

            // Add form-level KeyDown event handler for custom tab navigation
            this.KeyDown += new KeyEventHandler(frmPurchaseReturn_KeyDown);
        }

        private void RegisterEventHandlers()
        {
            try
            {
                // Register the BeforeCellUpdate event to manage cell update behavior
                ultraGrid1.BeforeCellUpdate += ultraGrid1_BeforeCellUpdate;

                // Register the KeyDown event to manage keypress behaviors in the grid
                ultraGrid1.KeyDown += ultraGrid1_KeyDown;

                // Register the AfterCellUpdate event to handle updates after a cell is changed
                ultraGrid1.AfterCellUpdate += ultraGrid1_AfterCellUpdate;

                // Register the InitializeLayout event to customize the grid appearance
                ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;

                // Register the CellListSelect event for handling dropdown selection
                ultraGrid1.CellListSelect += UltraGrid1_CellListSelect;

                // Register MouseDown for showing the dropdown on click
                ultraGrid1.MouseDown += UltraGrid1_MouseDown;

                // Register AfterCellActivate to handle showing dropdown when cell is activated
                ultraGrid1.AfterCellActivate += UltraGrid1_AfterCellActivate;

                // Register BeforeExitEditMode to track when a cell's value is modified
                ultraGrid1.BeforeExitEditMode += ultraGrid1_BeforeExitEditMode; // Add this line

                // Register KeyDown for textBox1 to load items on Enter
                textBox1.KeyDown += textBox1_KeyDown;

                // Register the row selection event for custom row highlighting
                ultraGrid1.AfterSelectChange += UltraGrid1_AfterSelectChange;

                // Register the panel paint handler to draw dashed line

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error registering event handlers: " + ex.Message);
            }
        }

        private void frmPurchaseReturn_Load(object sender, EventArgs e)
        {
            try
            {
                // Set default visibility state
                pbxSave.Visible = true;
                ultraPictureBox4.Visible = false;

                // Initialize the _originalTxtBarcodeReadOnly to false
                _originalTxtBarcodeReadOnly = false;
                TxtBarcode.ReadOnly = false;

                // Ensure KeyPreview is enabled for form-level key handling
                this.KeyPreview = true;

                // Set default placeholder text
                VendorName.Text = "Select Vendor";
                cmbPaymntMethod.Text = "Select Paymode";

                // Ensure branch data is loaded when form opens
                if (cmbBranch.Items.Count == 0)
                {
                    LoadBranchData();
                }

                // Generate purchase return number when form loads
                GeneratePurchaseReturnNumber();

                // Register event handlers
                RegisterEventHandlers();

                // Ensure grid panel is docked properly
                SetupGridDocking();

                // Configure the grid layout
                ConfigureItemsGridLayout();

                // Set explicit tab indices for our target controls
                Vendorbutton.TabIndex = 1;
                cmbPaymntMethod.TabIndex = 2;
                textBox1.TabIndex = 3;
                btnAddPurchaceList.TabIndex = 4;
                TxtBarcode.TabIndex = 5;

                // Disable tab stop for all controls on the form except our target controls
                DisableTabStopForAllExcept(new Control[] { Vendorbutton, cmbPaymntMethod, textBox1, btnAddPurchaceList, TxtBarcode });

                // Set the initial focus to the Vendorbutton
                this.ActiveControl = Vendorbutton;

                // Add resize event handler to ensure grid fills available space
                this.Resize += frmPurchaseReturn_Resize;

                // Force an initial resize to set column proportions
                ResizeGridColumns();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in form load: " + ex.Message);
            }
        }

        private void SetupGridDocking()
        {
            try
            {
                // Find the parent panel of the ultraGrid1
                Control gridParent = ultraGrid1.Parent;

                if (gridParent != null)
                {
                    // Set the parent panel to dock and fill its container
                    gridParent.Dock = DockStyle.Fill;

                    // Minimize any margins or padding to avoid blank spaces
                    if (gridParent is Panel panel)
                    {
                        panel.Padding = new Padding(0);
                        panel.Margin = new Padding(0);
                    }

                    // Ensure the UltraGrid fills its parent completely
                    ultraGrid1.Dock = DockStyle.Fill;
                    ultraGrid1.Margin = new Padding(0);

                    // Set sizing properties for grid rows to prevent blank space
                    if (ultraGrid1.DisplayLayout != null)
                    {
                        ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
                        ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 22;
                        ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                        ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                        ultraGrid1.DisplayLayout.Override.CellPadding = 0;
                        ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                        ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                        ultraGrid1.DisplayLayout.MaxColScrollRegions = 1;
                        ultraGrid1.DisplayLayout.MaxRowScrollRegions = 1;
                        ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting up grid docking: " + ex.Message);
            }
        }

        private void frmPurchaseReturn_Resize(object sender, EventArgs e)
        {
            // Resize grid columns to fill available width when form is resized
            ResizeGridColumns();
        }

        private void ResizeGridColumns()
        {
            try
            {
                if (ultraGrid1 == null || ultraGrid1.DisplayLayout == null ||
                    ultraGrid1.DisplayLayout.Bands.Count == 0)
                    return;

                // Get the available width (subtract the width of the row selectors and vertical scrollbar)
                int scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
                int rowSelectorsWidth = ultraGrid1.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True ? 20 : 0;
                int availableWidth = ultraGrid1.Width - scrollBarWidth - rowSelectorsWidth - 2;

                if (availableWidth <= 0)
                    return;

                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Count visible columns and get total of current widths
                int visibleColumnCount = 0;
                int totalCurrentWidth = 0;

                foreach (UltraGridColumn col in band.Columns)
                {
                    if (!col.Hidden)
                    {
                        visibleColumnCount++;
                        totalCurrentWidth += col.Width;
                    }
                }

                if (visibleColumnCount == 0 || totalCurrentWidth == 0)
                    return;

                // Calculate scaling factor
                double scaleFactor = (double)availableWidth / totalCurrentWidth;

                // Adjust column widths proportionally
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (!col.Hidden)
                    {
                        // Apply minimum width for each column type
                        int minWidth = 60; // Default minimum width

                        switch (col.Key)
                        {
                            case "Description":
                                minWidth = 150; // Wider for item description
                                break;
                            case "Amount":
                                minWidth = 100; // Wider for amounts
                                break;
                            case "Reason":
                                minWidth = 120; // Wider for dropdown
                                break;
                        }

                        // Calculate new width but respect minimum
                        int newWidth = Math.Max(minWidth, (int)(col.Width * scaleFactor));
                        col.Width = newWidth;
                    }
                }

                // Force the grid to repaint
                ultraGrid1.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error resizing grid columns: " + ex.Message);
            }
        }

        // Method to disable tab stop for all controls except the specified ones
        private void DisableTabStopForAllExcept(Control[] exceptControls)
        {
            DisableTabStopForControlsExcept(this.Controls, exceptControls);
        }

        // Helper method to recursively disable tab stop for controls
        private void DisableTabStopForControlsExcept(Control.ControlCollection controls, Control[] exceptControls)
        {
            foreach (Control control in controls)
            {
                // Skip the specified exception controls
                if (!exceptControls.Contains(control))
                {
                    // Disable tab stop for this control
                    control.TabStop = false;
                }
                else
                {
                    // Ensure tab stop is enabled for our target controls
                    control.TabStop = true;
                }

                // Process child controls recursively if this control has children
                if (control.Controls.Count > 0)
                {
                    DisableTabStopForControlsExcept(control.Controls, exceptControls);
                }
            }
        }

        // Method to load branch data into cmbBranch dropdown
        private void LoadBranchData()
        {
            try
            {
                // Show loading cursor
                Cursor.Current = Cursors.WaitCursor;

                // Get branch data from the dropdown class
                BranchDDlGrid branchGrid = drop.getBanchDDl();

                // Check if branches were found
                if (branchGrid?.List != null && branchGrid.List.Any())
                {
                    // Set up the dropdown properties
                    cmbBranch.DisplayMember = "BranchName";
                    cmbBranch.ValueMember = "Id";
                    cmbBranch.DataSource = branchGrid.List;

                    // Set a default branch if available
                    if (cmbBranch.Items.Count > 0)
                    {
                        cmbBranch.SelectedIndex = 0;
                    }
                }
                else
                {
                    // If no branches found, clear the dropdown
                    cmbBranch.DataSource = null;
                    cmbBranch.Items.Clear();

                    // Inform the user that no branches were found
                    MessageBox.Show("No branches found in the database. Please add branches first.",
                        "No Branches", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading branch data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset cursor
                Cursor.Current = Cursors.Default;
            }
        }

        private void ultraCombo1_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void lblNetAmount_Click(object sender, EventArgs e)
        {

        }

        private void ultraPanel2_PaintClient(object sender, PaintEventArgs e)
        {

        }

        private void ultraDateTimeEditor1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void lblBranch_Click(object sender, EventArgs e)
        {

        }

        private void lblVendor_Click(object sender, EventArgs e)
        {

        }

        private void lblPayment_Click(object sender, EventArgs e)
        {

        }

        // Method to get the selected branch ID
        private int GetSelectedBranchId()
        {
            try
            {
                if (cmbBranch.SelectedItem != null)
                {
                    // Get the selected branch ID by accessing the Id property of the BranchDDl object
                    BranchDDl selectedBranch = cmbBranch.SelectedItem as BranchDDl;
                    if (selectedBranch != null)
                    {
                        return selectedBranch.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting selected branch: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Return 0 if no branch is selected or an error occurs
            return 0;
        }

        private void cmbBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            // You can add code here to handle when a branch is selected
            // For example, you might want to load related data based on the selected branch
            int selectedBranchId = GetSelectedBranchId();

            // If a valid branch is selected, you can perform additional operations
            if (selectedBranchId > 0)
            {
                // Add your code here to load data related to the selected branch
                // For example, load vendors or products for this branch
            }
        }

        // Add a click event handler for the branch dropdown to ensure data is loaded when clicked
        private void cmbBranch_Click(object sender, EventArgs e)
        {
            // If the dropdown is empty, reload the branch data
            if (cmbBranch.Items.Count == 0)
            {
                LoadBranchData();
            }
        }

        private void cmbCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cmbPaymntMethod_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lblPRno_Click(object sender, EventArgs e)
        {

        }

        private void TxtSRNO_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Check if items are already loaded via btnAddPurchaceList (vendor purchase items)
            if (_isPurchaseDataLoaded)
            {
                MessageBox.Show("Invalid operation. Purchase data is already loaded.",
                    "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if items are being loaded via BtnDial
            // In the original context, btnDial is being used when TxtBarcode is read-only
            if (TxtBarcode.ReadOnly && !_originalTxtBarcodeReadOnly)
            {
                MessageBox.Show("Invalid operation. Please finish the current item selection process before using this button.",
                    "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if items are loaded by BtnDial (indicated by "WITHOUT GR")
            if (textBox1.Text == "WITHOUT GR")
            {
                MessageBox.Show("Invalid operation. Items already loaded via direct selection.",
                    "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Original functionality
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Don't do anything when text changes - only load on Enter key press
            // This allows the user to type the complete number without interruption
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Enter key was pressed
            if (e.KeyCode == Keys.Enter)
            {
                // Show pbxSave and hide ultraPictureBox4
                pbxSave.Visible = true;
                ultraPictureBox4.Visible = false;

                e.Handled = true;
                e.SuppressKeyPress = true;

                try
                {
                    // Don't do anything if text is "WITHOUT GR" (special case)
                    if (textBox1.Text == "WITHOUT GR")
                    {
                        return;
                    }

                    // Don't do anything if empty
                    if (string.IsNullOrEmpty(textBox1.Text))
                    {
                        return;
                    }

                    // Don't do anything if vendor is not selected
                    if (string.IsNullOrEmpty(vendorid.Text) || vendorid.Text == "0")
                    {
                        MessageBox.Show("Please select a vendor first.", "Vendor Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Only attempt to load purchase items if textBox1 contains a valid numeric value
                    if (int.TryParse(textBox1.Text, out int purchaseNo))
                    {
                        // Save the original readonly state of TxtBarcode
                        _originalTxtBarcodeReadOnly = TxtBarcode.ReadOnly;

                        // Make TxtBarcode read-only - We're removing this line to keep it writable
                        // TxtBarcode.ReadOnly = true;

                        // Ensure TxtBarcode remains writable - removing this line
                        // EnsureTxtBarcodeIsWritable();

                        int vendorId = int.Parse(vendorid.Text);
                        // Load purchase items based on the purchase number
                        LoadPurchaseItems(vendorId, purchaseNo);

                        // Set the flag to indicate purchase data is loaded
                        _isPurchaseDataLoaded = true;
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid purchase number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in textBox1_KeyDown: {ex.Message}");
                    MessageBox.Show($"Error loading purchase items: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ultraDateTimeEditor2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void TxtBarcode_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Show pbxSave and hide ultraPictureBox4
                pbxSave.Visible = true;
                ultraPictureBox4.Visible = false;

                // Check if we're in "Without GR" mode
                if (textBox1.Text == "WITHOUT GR")
                {
                    // In "Without GR" mode, we don't filter the grid on text changed
                    // We'll only search when Enter is pressed
                    return;
                }

                // Check if a purchase number is already selected
                if (string.IsNullOrEmpty(textBox1.Text))
                {
                    return; // No purchase selected yet, so do nothing
                }

                // Get the purchase number and vendor ID
                int purchaseNo = Convert.ToInt32(textBox1.Text);
                int vendorId = Convert.ToInt32(vendorid.Text);

                // Check if the barcode/item ID field has a value
                if (!string.IsNullOrEmpty(TxtBarcode.Text))
                {
                    // Try to parse the item ID (if it's a numeric barcode)
                    if (long.TryParse(TxtBarcode.Text, out long itemId))
                    {
                        // Filter the grid to show only the matching item ID or barcode
                        FilterGridByItemIdOrBarcode(itemId, TxtBarcode.Text);
                    }
                    else
                    {
                        // If not numeric, search by barcode text
                        FilterGridByItemIdOrBarcode(0, TxtBarcode.Text);
                    }
                }
                else
                {
                    // If barcode field is empty, reload all items for the purchase
                    LoadPurchaseItems(vendorId, purchaseNo);
                }
            }
            catch (Exception ex)
            {
                // Just log the error, don't show a message box as this is a text changed event
                System.Diagnostics.Debug.WriteLine("Error filtering by item ID or barcode: " + ex.Message);
            }
        }

        private void FilterGridByItemIdOrBarcode(long itemId, string barcode)
        {
            try
            {
                // Check if the grid has a data source
                if (ultraGrid1.DataSource == null)
                    return;

                // Get the current data source as a DataTable
                DataTable currentData = ultraGrid1.DataSource as DataTable;
                if (currentData == null)
                    return;

                // Create a filtered view of the data
                DataView dv = new DataView(currentData);

                // Build the filter condition
                string filter = "";

                if (itemId > 0)
                {
                    // If we have a numeric value, search by both ItemID and BarCode
                    filter = $"ItemID = {itemId} OR BarCode LIKE '%{barcode}%'";
                }
                else
                {
                    // If not numeric, search only by BarCode
                    filter = $"BarCode LIKE '%{barcode}%'";
                }

                dv.RowFilter = filter;

                // Apply the filter to the grid
                ultraGrid1.DataSource = dv.ToTable();

                // Configure the grid layout again
                ConfigureItemsGridLayout();

                // Set focus to the grid and select the first row if available
                ultraGrid1.Focus();
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);

                    // Set focus to the Unit cell of the first row and ensure edit mode is activated
                    if (ultraGrid1.ActiveRow.Cells.Exists("Unit"))
                    {
                        ultraGrid1.ActiveCell = ultraGrid1.ActiveRow.Cells["Unit"];
                        ultraGrid1.Focus();
                        // Enter edit mode with caret active inside the cell
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error filtering grid: " + ex.Message);
            }
        }

        private void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _enterPressed = true;
                e.Handled = true;
                e.SuppressKeyPress = true;

                try
                {
                    // Check if the barcode field has a value
                    if (!string.IsNullOrEmpty(TxtBarcode.Text))
                    {
                        // Set textBox1 to "Without GR" to indicate barcode method is being used
                        if (this.Controls.Find("textBox1", true).Length > 0)
                        {
                            TextBox textBox1 = this.Controls.Find("textBox1", true)[0] as TextBox;
                            if (textBox1 != null)
                            {
                                textBox1.Text = "WITHOUT GR"; // Changed to all caps for consistency
                            }
                        }

                        // Store the barcode value and clear the field before searching
                        string barcodeToSearch = TxtBarcode.Text;
                        TxtBarcode.Clear();

                        // Search for the barcode in the database and add to grid
                        SearchItemsByBarcode(barcodeToSearch);

                        // Don't set focus back to TxtBarcode - let it stay in the Unit cell
                        // TxtBarcode.Focus(); - Remove this line to keep focus in the Unit cell
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error searching barcode: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SearchItemsByBarcode(string barcode)
        {
            try
            {
                if (string.IsNullOrEmpty(barcode))
                {
                    MessageBox.Show("Please enter a barcode.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the barcode already exists in the grid
                bool barcodeExists = false;
                UltraGridRow existingRow = null;

                if (ultraGrid1.DataSource != null && ultraGrid1.DataSource is DataTable)
                {
                    DataTable existingData = (DataTable)ultraGrid1.DataSource;

                    // Search for the barcode in existing rows
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (row.Cells["Barcode"].Value != null &&
                            row.Cells["Barcode"].Value.ToString().Equals(barcode, StringComparison.OrdinalIgnoreCase))
                        {
                            barcodeExists = true;
                            existingRow = row;
                            break;
                        }
                    }

                    // If barcode exists, increment quantity and update amount
                    if (barcodeExists && existingRow != null)
                    {
                        // Get current quantity
                        double currentQty = Convert.ToDouble(existingRow.Cells["Quantity"].Value);

                        // Increment quantity by 1
                        double newQty = currentQty + 1;
                        existingRow.Cells["Quantity"].Value = newQty;

                        // Recalculate amount
                        decimal cost = Convert.ToDecimal(existingRow.Cells["Cost"].Value);
                        existingRow.Cells["Amount"].Value = cost * (decimal)newQty;

                        // Update the net amount total
                        UpdateNetAmount();

                        // Set focus to this row and highlight it
                        ultraGrid1.ActiveRow = existingRow;
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(existingRow);

                        // Set focus to the Quantity cell to show it was updated
                        if (existingRow.Cells.Exists("Quantity"))
                        {
                            ultraGrid1.ActiveCell = existingRow.Cells["Quantity"];
                            ultraGrid1.Focus();
                            // Flash the cell to indicate it was updated
                            ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                            // Scroll to ensure the row is visible
                            ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(existingRow);
                        }

                        return; // Exit the method since we've updated the existing item
                    }
                }

                // If barcode doesn't exist, continue with existing functionality to add new item
                // Create a BaseRepostitory instance to get the connection
                BaseRepostitory baseRepo = new BaseRepostitory();

                // Use the repository to search for the barcode
                using (System.Data.SqlClient.SqlConnection conn = (System.Data.SqlClient.SqlConnection)baseRepo.DataConnection)
                {
                    conn.Open();
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("_POS_PurchaseReturn", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@barcode", barcode);
                        cmd.Parameters.AddWithValue("@_Operation", "BARCODEPURCHASE");

                        using (System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(cmd))
                        {
                            DataTable newItemsTable = new DataTable();
                            adapter.Fill(newItemsTable);

                            // Create a new table with our specified columns
                            DataTable formattedTable = new DataTable();

                            // Add our required columns in the correct order
                            formattedTable.Columns.Add("Sl No", typeof(int));
                            formattedTable.Columns.Add("Description", typeof(string)); // Item Name
                            formattedTable.Columns.Add("ItemID", typeof(long));
                            formattedTable.Columns.Add("Barcode", typeof(string));
                            formattedTable.Columns.Add("Unit", typeof(string));
                            formattedTable.Columns.Add("Packing", typeof(double));
                            formattedTable.Columns.Add("Cost", typeof(decimal));
                            formattedTable.Columns.Add("Quantity", typeof(double));
                            formattedTable.Columns.Add("Amount", typeof(decimal));
                            formattedTable.Columns.Add("Reason", typeof(string));
                            formattedTable.Columns.Add("SELECT", typeof(bool)); // Added SELECT column for schema consistency

                            // Add tax-related columns to match btnDial schema
                            formattedTable.Columns.Add("UnitId", typeof(int));
                            formattedTable.Columns.Add("TaxPer", typeof(decimal));
                            formattedTable.Columns.Add("TaxAmt", typeof(decimal));
                            formattedTable.Columns.Add("TaxType", typeof(string));

                            if (newItemsTable != null && newItemsTable.Rows.Count > 0)
                            {
                                // Copy data from the source table to our formatted table
                                DataRow newRow = formattedTable.NewRow();

                                // Set sequential SlNo
                                newRow["Sl No"] = ultraGrid1.Rows.Count + 1;

                                // Copy Item Name
                                if (newItemsTable.Columns.Contains("Description"))
                                    newRow["Description"] = newItemsTable.Rows[0]["Description"];
                                else if (newItemsTable.Columns.Contains("ItemName"))
                                    newRow["Description"] = newItemsTable.Rows[0]["ItemName"];
                                else
                                    newRow["Description"] = "";

                                // Copy ItemID
                                if (newItemsTable.Columns.Contains("ItemID"))
                                    newRow["ItemID"] = newItemsTable.Rows[0]["ItemID"];
                                else
                                    newRow["ItemID"] = 0;

                                // Copy Barcode
                                if (newItemsTable.Columns.Contains("Barcode"))
                                    newRow["Barcode"] = newItemsTable.Rows[0]["Barcode"];
                                else if (newItemsTable.Columns.Contains("BarCode"))
                                    newRow["Barcode"] = newItemsTable.Rows[0]["BarCode"];
                                else
                                    newRow["Barcode"] = barcode;

                                // Copy Unit
                                if (newItemsTable.Columns.Contains("Unit"))
                                    newRow["Unit"] = newItemsTable.Rows[0]["Unit"];
                                else if (newItemsTable.Columns.Contains("UnitName"))
                                    newRow["Unit"] = newItemsTable.Rows[0]["UnitName"];
                                else
                                    newRow["Unit"] = "";

                                // Copy Packing
                                if (newItemsTable.Columns.Contains("Packing"))
                                    newRow["Packing"] = newItemsTable.Rows[0]["Packing"];
                                else
                                    newRow["Packing"] = 1;

                                // Copy Cost
                                if (newItemsTable.Columns.Contains("Cost"))
                                    newRow["Cost"] = newItemsTable.Rows[0]["Cost"];
                                else
                                    newRow["Cost"] = 0;

                                // Set Quantity to 1 by default
                                newRow["Quantity"] = 1;

                                // Initialize Reason
                                newRow["Reason"] = "Select Reason";

                                // Set values for the additional columns
                                newRow["SELECT"] = false;
                                newRow["UnitId"] = newItemsTable.Columns.Contains("UnitId") ?
                                    newItemsTable.Rows[0]["UnitId"] : 0;
                                newRow["TaxPer"] = newItemsTable.Columns.Contains("TaxPer") ?
                                    newItemsTable.Rows[0]["TaxPer"] : 0m;
                                newRow["TaxAmt"] = newItemsTable.Columns.Contains("TaxAmt") ?
                                    newItemsTable.Rows[0]["TaxAmt"] : 0m;
                                newRow["TaxType"] = newItemsTable.Columns.Contains("TaxType") ?
                                    newItemsTable.Rows[0]["TaxType"] : "";

                                // Calculate Amount
                                decimal cost = Convert.ToDecimal(newRow["Cost"]);
                                double quantity = Convert.ToDouble(newRow["Quantity"]);
                                newRow["Amount"] = cost * (decimal)quantity;

                                // Add the row to the table
                                formattedTable.Rows.Add(newRow);
                            }
                            else
                            {
                                // No items found with this barcode
                                // Create an empty row with default values
                                DataRow emptyRow = formattedTable.NewRow();
                                emptyRow["Sl No"] = ultraGrid1.Rows.Count + 1;
                                emptyRow["Description"] = string.Empty;
                                emptyRow["ItemID"] = 0;
                                emptyRow["Barcode"] = barcode;
                                emptyRow["Unit"] = string.Empty;
                                emptyRow["Packing"] = 1;
                                emptyRow["Cost"] = 0;
                                emptyRow["Quantity"] = 1;
                                emptyRow["Amount"] = 0;
                                emptyRow["Reason"] = "Select Reason";
                                emptyRow["SELECT"] = false;
                                emptyRow["UnitId"] = 0;
                                emptyRow["TaxPer"] = 0m;
                                emptyRow["TaxAmt"] = 0m;
                                emptyRow["TaxType"] = "";

                                formattedTable.Rows.Add(emptyRow);

                                MessageBox.Show("No items found with this barcode. A new row has been added.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            // Check if we already have items in the grid
                            if (ultraGrid1.DataSource != null && ultraGrid1.DataSource is DataTable)
                            {
                                // Get the existing data
                                DataTable existingData = (DataTable)ultraGrid1.DataSource;

                                // Add the new rows to the existing table
                                foreach (DataRow row in formattedTable.Rows)
                                {
                                    DataRow newRow = existingData.NewRow();
                                    foreach (DataColumn col in formattedTable.Columns)
                                    {
                                        if (existingData.Columns.Contains(col.ColumnName))
                                        {
                                            newRow[col.ColumnName] = row[col.ColumnName];
                                        }
                                    }
                                    existingData.Rows.Add(newRow);
                                }

                                // Refresh the grid
                                ultraGrid1.DataSource = existingData;
                            }
                            else
                            {
                                // If no existing data, just set the new formatted table as the source
                                ultraGrid1.DataSource = formattedTable;
                            }

                            // Make grid headers visible again when rows are added
                            foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                            {
                                col.Hidden = false;
                            }

                            // Configure grid layout
                            ConfigureItemsGridLayout();

                            // Select the last row (the newly added one)
                            if (ultraGrid1.Rows.Count > 0)
                            {
                                ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                                ultraGrid1.Selected.Rows.Clear();
                                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);

                                // Scroll to the newly added row
                                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);

                                // Set focus to the Unit cell and ensure edit mode is activated
                                if (ultraGrid1.ActiveRow.Cells.Exists("Unit"))
                                {
                                    ultraGrid1.ActiveCell = ultraGrid1.ActiveRow.Cells["Unit"];
                                    ultraGrid1.Focus();
                                    // Enter edit mode with caret active inside the cell
                                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                }
                            }

                            // Update the Net Amount label
                            UpdateNetAmount();

                            // Don't return focus to the barcode field - let it stay in the Unit cell
                            // TxtBarcode.Focus(); - We'll comment this out in the TxtBarcode_KeyDown method
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching barcode: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                TxtBarcode.Focus();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            lblNetAmount.Text = textBox2.Text;

            // Update TxtSubTotal to match textBox2
            TxtSubTotal.Text = textBox2.Text;
        }

        private void BtnDial_Click(object sender, EventArgs e)
        {
            try
            {
                // Remove restriction for PurchaseReturnUpdate
                // We're now allowing adding items when loaded via button1

                // Only prevent direct item selection if we're in the middle of a purchase return process
                // that was loaded via purchase number
                if (_isPurchaseDataLoaded && !string.IsNullOrEmpty(textBox1.Text) &&
                    textBox1.Text != "WITHOUT GR" && textBox1.Text != "Without GR")
                {
                    MessageBox.Show("Items are already loaded from purchase number " + textBox1.Text + ". " +
                        "Please complete this return or clear the form first.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Show pbxSave and hide ultraPictureBox4
                pbxSave.Visible = true;
                ultraPictureBox4.Visible = false;

                // Open the item master dialog without requiring vendor selection
                frmdialForItemMaster itemDialog = new frmdialForItemMaster("FromPurchaseReturn");
                if (itemDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Save the original readonly state of TxtBarcode
                        _originalTxtBarcodeReadOnly = TxtBarcode.ReadOnly;

                        // Make TxtBarcode writable when using BtnDial
                        EnsureTxtBarcodeIsWritable();

                        // Set "WITHOUT GR" in textBox1 to indicate items added directly
                        textBox1.Text = "WITHOUT GR";

                        // Dictionary to store all item data fields
                        Dictionary<string, object> itemData;

                        // First try to get the data from GetSelectedItemData method which has all fields
                        itemData = itemDialog.GetSelectedItemData();

                        // Extract the essential values from the dictionary
                        long itemId = Convert.ToInt64(itemData["ItemId"]);
                        string itemName = itemData["Description"].ToString();
                        string barcode = itemData.ContainsKey("BarCode") ? itemData["BarCode"].ToString() : "";
                        string unit = itemData["Unit"].ToString();
                        double packing = Convert.ToDouble(itemData["Packing"]);
                        decimal cost = Convert.ToDecimal(itemData["Cost"]);

                        // Extract tax-related values if available
                        decimal taxPer = itemData.ContainsKey("TaxPer") ? Convert.ToDecimal(itemData["TaxPer"]) : 0m;
                        decimal taxAmt = itemData.ContainsKey("TaxAmt") ? Convert.ToDecimal(itemData["TaxAmt"]) : 0m;
                        string taxType = itemData.ContainsKey("TaxType") ? itemData["TaxType"].ToString() : "";
                        int unitId = itemData.ContainsKey("UnitId") ? Convert.ToInt32(itemData["UnitId"]) : 0;

                        // Check if we already have grid data
                        DataTable existingData = null;

                        if (ultraGrid1.DataSource != null && ultraGrid1.DataSource is DataTable)
                        {
                            existingData = ((DataTable)ultraGrid1.DataSource).Copy(); // Make a copy to preserve the schema
                        }
                        else
                        {
                            // Create a new DataTable with only the required columns
                            existingData = new DataTable();

                            // Add only the necessary columns in the specified order
                            existingData.Columns.Add("Sl No", typeof(int));
                            existingData.Columns.Add("Description", typeof(string)); // Item Name
                            existingData.Columns.Add("Barcode", typeof(string));
                            existingData.Columns.Add("Unit", typeof(string));
                            existingData.Columns.Add("Packing", typeof(double));
                            existingData.Columns.Add("Cost", typeof(decimal));
                            existingData.Columns.Add("Quantity", typeof(double));
                            existingData.Columns.Add("Amount", typeof(decimal));
                            existingData.Columns.Add("Reason", typeof(string));
                            existingData.Columns.Add("SELECT", typeof(bool)); // Add SELECT column at the end

                            // These are hidden columns, order doesn't matter for display
                            existingData.Columns.Add("ItemID", typeof(long));
                            existingData.Columns.Add("UnitId", typeof(int));
                            existingData.Columns.Add("TaxPer", typeof(decimal));
                            existingData.Columns.Add("TaxAmt", typeof(decimal));
                            existingData.Columns.Add("TaxType", typeof(string));
                        }

                        // Create a new row
                        DataRow newRow = existingData.NewRow();

                        // Set the SlNo value for the new row (number sequentially)
                        int slNo = existingData.Rows.Count + 1;
                        newRow["Sl No"] = slNo;

                        // Set SELECT to false by default if the column exists
                        if (existingData.Columns.Contains("SELECT"))
                        {
                            newRow["SELECT"] = false;
                        }

                        // Populate the row with the selected item data
                        newRow["Description"] = itemName;
                        newRow["ItemID"] = itemId;
                        newRow["Barcode"] = barcode;
                        newRow["Unit"] = unit;
                        newRow["UnitId"] = unitId;
                        newRow["Packing"] = packing;
                        newRow["Cost"] = cost;
                        newRow["TaxPer"] = taxPer;
                        newRow["TaxAmt"] = taxAmt;
                        newRow["TaxType"] = taxType;
                        newRow["Reason"] = "Select Reason"; // Set default value for Reason
                        newRow["Quantity"] = 1.0; // Default quantity

                        // Calculate the amount using the formula: Cost × Packing × Quantity
                        decimal amount = cost * Convert.ToDecimal(packing) * 1.0m;
                        newRow["Amount"] = amount;

                        // Add the row to the table
                        existingData.Rows.Add(newRow);

                        // Update the grid with the new data
                        ultraGrid1.DataSource = existingData;

                        // Make grid headers visible again when rows are added
                        foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                        {
                            col.Hidden = false;
                        }

                        // Configure the grid layout to ensure all columns are displayed properly
                        ConfigureItemsGridLayout();

                        // Select the newly added row and scroll to it
                        if (ultraGrid1.Rows.Count > 0)
                        {
                            ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                            ultraGrid1.Selected.Rows.Clear();
                            ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);

                            // Scroll to the newly added row
                            ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                        }

                        // Update the Net Amount label to reflect the new total
                        UpdateNetAmount();

                        // Set focus to the Description cell of the new row
                        SetFocusToLastRow();

                        // Items are now loaded directly, not from a purchase number
                        _isPurchaseDataLoaded = false;
                        _currentlyLoadedPurchaseNo = -1;

                        // Clear the PurchaseReturnUpdate tag if it exists
                        if (this.Tag != null && this.Tag.ToString() == "PurchaseReturnUpdate")
                        {
                            this.Tag = null;
                        }
                    }
                    catch (Exception ex2)
                    {
                        MessageBox.Show("Error adding selected item: " + ex2.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        System.Diagnostics.Debug.WriteLine($"Error adding selected item: {ex2.Message}\n{ex2.StackTrace}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening item dialog: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in BtnDial_Click: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Method to load purchase items into ultraGrid1
        private void LoadPurchaseItems(int ledgerId, int purchaseNo)
        {
            try
            {
                // Show loading cursor
                Cursor.Current = Cursors.WaitCursor;

                // Get purchase items data
                DataTable purchaseItemsData = GetPurchaseItems(ledgerId, purchaseNo);

                if (purchaseItemsData != null && purchaseItemsData.Rows.Count > 0)
                {
                    // Log the raw data to help debug
                    System.Diagnostics.Debug.WriteLine($"Loaded {purchaseItemsData.Rows.Count} rows from purchase items for ledger {ledgerId}, purchase {purchaseNo}");

                    // Get the existing data from the grid, if any
                    DataTable existingData = null;
                    int startingSlNo = 1;

                    // Check if this method was called from btnAddPurchaceList
                    bool isCalledFromBtnAddPurchaceList = new System.Diagnostics.StackTrace().GetFrames()
                        .Any(frame => frame.GetMethod().Name == "btnAddPurchaceList_Click");

                    if (ultraGrid1.DataSource != null && ultraGrid1.DataSource is DataTable)
                    {
                        existingData = (DataTable)ultraGrid1.DataSource;

                        // Always start at 1 when called from btnAddPurchaceList as we've already cleared the grid
                        if (!isCalledFromBtnAddPurchaceList && existingData.Rows.Count > 0)
                        {
                            startingSlNo = existingData.Rows.Count + 1;
                        }
                    }

                    // If there's no existing data, create a new table
                    if (existingData == null)
                    {
                        existingData = new DataTable();

                        // Add only the visible columns that we want to display
                        existingData.Columns.Add("Sl No", typeof(int));
                        existingData.Columns.Add("Description", typeof(string)); // Item Name
                        existingData.Columns.Add("Barcode", typeof(string));
                        existingData.Columns.Add("Unit", typeof(string));
                        existingData.Columns.Add("Packing", typeof(double));
                        existingData.Columns.Add("Cost", typeof(decimal));
                        existingData.Columns.Add("Quantity", typeof(double));
                        existingData.Columns.Add("Amount", typeof(decimal));
                        existingData.Columns.Add("Reason", typeof(string));
                        existingData.Columns.Add("SELECT", typeof(bool)); // Add SELECT column at the end

                        // Add ItemID as a hidden column, but we'll hide it in the grid layout
                        existingData.Columns.Add("ItemID", typeof(long));

                        // Add UnitId if needed
                        if (!existingData.Columns.Contains("UnitId"))
                        {
                            existingData.Columns.Add("UnitId", typeof(int));
                        }
                    }

                    // Copy data from the source table to our existing data table
                    for (int i = 0; i < purchaseItemsData.Rows.Count; i++)
                    {
                        DataRow newRow = existingData.NewRow();

                        // Set the sequential SlNo, continuing from existing data
                        newRow["Sl No"] = startingSlNo + i;

                        // Set SELECT to false by default for each row if the column exists
                        if (existingData.Columns.Contains("SELECT"))
                        {
                            newRow["SELECT"] = false;
                        }

                        // Copy Item Name/Description
                        if (purchaseItemsData.Columns.Contains("Description"))
                            newRow["Description"] = purchaseItemsData.Rows[i]["Description"];
                        else if (purchaseItemsData.Columns.Contains("ItemName"))
                            newRow["Description"] = purchaseItemsData.Rows[i]["ItemName"];
                        else
                            newRow["Description"] = "";

                        // Copy ItemID (but it will be hidden)
                        if (purchaseItemsData.Columns.Contains("ItemID"))
                            newRow["ItemID"] = purchaseItemsData.Rows[i]["ItemID"];
                        else
                            newRow["ItemID"] = 0;

                        // Copy Barcode
                        if (purchaseItemsData.Columns.Contains("Barcode"))
                            newRow["Barcode"] = purchaseItemsData.Rows[i]["Barcode"];
                        else if (purchaseItemsData.Columns.Contains("BarCode"))
                            newRow["Barcode"] = purchaseItemsData.Rows[i]["BarCode"];
                        else
                            newRow["Barcode"] = "";

                        // Copy Unit
                        if (purchaseItemsData.Columns.Contains("Unit"))
                            newRow["Unit"] = purchaseItemsData.Rows[i]["Unit"];
                        else if (purchaseItemsData.Columns.Contains("UnitName"))
                            newRow["Unit"] = purchaseItemsData.Rows[i]["UnitName"];
                        else
                            newRow["Unit"] = "";

                        // Copy UnitId if it exists in the columns
                        if (existingData.Columns.Contains("UnitId"))
                        {
                            if (purchaseItemsData.Columns.Contains("UnitId"))
                                newRow["UnitId"] = purchaseItemsData.Rows[i]["UnitId"];
                            else
                                newRow["UnitId"] = 0;
                        }

                        // Copy Packing
                        if (purchaseItemsData.Columns.Contains("Packing"))
                            newRow["Packing"] = purchaseItemsData.Rows[i]["Packing"];
                        else
                            newRow["Packing"] = 1;

                        // Copy Cost
                        if (purchaseItemsData.Columns.Contains("Cost"))
                            newRow["Cost"] = purchaseItemsData.Rows[i]["Cost"];
                        else
                            newRow["Cost"] = 0;

                        // Set Quantity from existing Qty or default to 1
                        if (purchaseItemsData.Columns.Contains("Qty") && purchaseItemsData.Rows[i]["Qty"] != DBNull.Value)
                            newRow["Quantity"] = purchaseItemsData.Rows[i]["Qty"];
                        else if (purchaseItemsData.Columns.Contains("Quantity") && purchaseItemsData.Rows[i]["Quantity"] != DBNull.Value)
                            newRow["Quantity"] = purchaseItemsData.Rows[i]["Quantity"];
                        else
                            newRow["Quantity"] = 1;

                        // Copy Reason if it exists, otherwise initialize with "Select Reason"
                        if (purchaseItemsData.Columns.Contains("Reason") && purchaseItemsData.Rows[i]["Reason"] != DBNull.Value
                            && !string.IsNullOrWhiteSpace(purchaseItemsData.Rows[i]["Reason"].ToString()))
                        {
                            string reasonValue = purchaseItemsData.Rows[i]["Reason"].ToString().Trim();
                            newRow["Reason"] = reasonValue;
                        }
                        else
                        {
                            newRow["Reason"] = "Select Reason";
                        }

                        // Calculate Amount
                        decimal cost = Convert.ToDecimal(newRow["Cost"]);
                        double quantity = Convert.ToDouble(newRow["Quantity"]);
                        newRow["Amount"] = cost * (decimal)quantity;

                        // Add the row to the formatted table
                        existingData.Rows.Add(newRow);
                    }

                    // Set the data source for ultraGrid1
                    ultraGrid1.DataSource = existingData;

                    // Configure grid layout to ensure only requested columns are visible
                    ConfigureItemsGridLayout();

                    // Update the Net Amount label
                    UpdateNetAmount();

                    // Apply our grid layout to ensure only the specified columns are visible
                    ConfigureItemsGridLayout();

                    // Make sure reason values are preserved
                    PreserveReasonValues();

                    // Check if this was called from btnAddPurchaceList
                    if (isCalledFromBtnAddPurchaceList)
                    {
                        // Set focus to the first row's Description cell
                        SetFocusToFirstRow();
                    }
                    else
                    {
                        // For other cases, use the original behavior
                        SetFocusToDescription();
                    }

                    // Set the flag to indicate purchase data is loaded
                    _isPurchaseDataLoaded = true;
                }
                else
                {
                    // If no items found, show a message
                    MessageBox.Show("No items found for this purchase.",
                        "No Items", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Check if this method was called from btnAddPurchaceList
                bool isCalledFromPurchaceList = new System.Diagnostics.StackTrace().GetFrames()
                    .Any(frame => frame.GetMethod().Name == "btnAddPurchaceList_Click");

                // If called from btnAddPurchaceList, keep TxtBarcode read-only
                // Otherwise, ensure it's writable
                if (!isCalledFromPurchaceList)
                {
                    // Ensure TxtBarcode is always writable
                    EnsureTxtBarcodeIsWritable();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading purchase items: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset cursor
                Cursor.Current = Cursors.Default;
            }
        }

        // Helper method to set focus to the Description column of the first row in the grid
        private void SetFocusToFirstRow()
        {
            try
            {
                // Make sure the grid has focus and there are rows
                if (ultraGrid1.Rows.Count > 0)
                {
                    // Get the first row in the grid
                    UltraGridRow firstRow = ultraGrid1.Rows[0];

                    // Set this row as the active row and clear any existing selection
                    ultraGrid1.ActiveRow = firstRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(firstRow);

                    // Always focus specifically on the Description column (Item Name) as requested
                    if (firstRow.Cells.Exists("Description"))
                    {
                        ultraGrid1.ActiveCell = firstRow.Cells["Description"];

                        // Set focus to the grid and enter edit mode
                        ultraGrid1.Focus();
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);

                        // Explicitly show highlighting or cursor to indicate the focus is set
                        if (ultraGrid1.ActiveCell != null)
                        {
                            ultraGrid1.ActiveCell.Appearance.BackColor = Color.FromArgb(255, 233, 233);
                            ultraGrid1.Refresh();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting focus to first row Description: " + ex.Message);
            }
        }

        private void SetFocusToDescription()
        {
            try
            {
                // Make sure the grid has focus and there is an active row
                if (ultraGrid1.Rows.Count > 0)
                {
                    // Get the last row in the grid instead of the active row
                    UltraGridRow activeRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];

                    // Set this row as the active row and clear any existing selection
                    ultraGrid1.ActiveRow = activeRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(activeRow);

                    // Set focus to the Description cell if it exists
                    if (activeRow.Cells.Exists("Description"))
                    {
                        ultraGrid1.ActiveCell = activeRow.Cells["Description"];

                        // Set focus to the grid and enter edit mode
                        ultraGrid1.Focus();
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting focus to Description: " + ex.Message);
            }
        }

        // Method to get purchase items data
        private DataTable GetPurchaseItems(int ledgerId, int purchaseNo)
        {
            DataTable dt = new DataTable();

            try
            {
                // Create a BaseRepostitory instance to get the connection
                BaseRepostitory baseRepo = new BaseRepostitory();

                // Use the repository to get purchase items data
                using (System.Data.SqlClient.SqlConnection conn = (System.Data.SqlClient.SqlConnection)baseRepo.DataConnection)
                {
                    conn.Open();
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("_POS_PurchaseReturn", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@LedgerID", ledgerId);
                        cmd.Parameters.AddWithValue("@PurchaseNo", purchaseNo);
                        cmd.Parameters.AddWithValue("@_Operation", "GetAllPurchaseItems");

                        using (System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dt;
        }

        // Method to configure the items grid layout
        private void ConfigureItemsGridLayout()
        {
            try
            {
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                    // Configure columns array for visible columns - ONLY include the requested columns
                    string[] visibleColumns = new string[] {
                        "Sl No", "Description", "Barcode", "Unit", "Packing", "Cost",
                        "Quantity", "Amount", "Reason", "SELECT"
                    };

                    // Configure hidden columns - these are important for data storage but not shown in UI
                    string[] hiddenColumns = new string[] {
                        "ItemID", "UnitId", "TaxPer", "TaxAmt", "TaxType"
                    };

                    // Set default vertical alignment for all cells
                    ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = VAlign.Middle;

                    // First, hide ALL columns
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        col.Hidden = true;
                    }

                    // Ensure the SELECT column exists
                    if (!band.Columns.Exists("SELECT"))
                    {
                        band.Columns.Add("SELECT", ""); // Remove the caption text

                        // If the data source is a DataTable, make sure it has the SELECT column
                        if (ultraGrid1.DataSource is DataTable dt && !dt.Columns.Contains("SELECT"))
                        {
                            try
                            {
                                dt.Columns.Add("SELECT", typeof(bool));
                                // Set default values
                                foreach (DataRow row in dt.Rows)
                                {
                                    row["SELECT"] = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("Error adding SELECT column to DataTable: " + ex.Message);
                            }
                        }
                    }

                    // Check if we're in initial form load - determine whether to show columns or not
                    bool shouldShowColumns = (ultraGrid1.DataSource is DataTable sourceTable && sourceTable.Rows.Count > 0);

                    // Then, set up the visible columns in the specified order and make ONLY them visible if we have data
                    foreach (string columnName in visibleColumns)
                    {
                        if (band.Columns.Exists(columnName))
                        {
                            UltraGridColumn column = band.Columns[columnName];

                            // Only make columns visible if we have data, otherwise leave them hidden
                            column.Hidden = !shouldShowColumns;
                            column.CellActivation = Activation.AllowEdit;
                            column.CellAppearance.TextVAlign = VAlign.Middle;

                            // Explicitly set the display position based on the order in visibleColumns
                            int positionIndex = Array.IndexOf(visibleColumns, columnName);
                            column.Header.VisiblePosition = positionIndex;

                            // Configure specific columns
                            switch (columnName)
                            {
                                case "SELECT":
                                    column.Width = 60;
                                    column.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                                    column.Header.Caption = ""; // Remove header text
                                    column.CellAppearance.TextHAlign = HAlign.Center;
                                    column.DataType = typeof(bool);
                                    column.DefaultCellValue = false;

                                    // Always set up the header checkbox regardless of whether it's visible
                                    column.Header.CheckBoxAlignment = HeaderCheckBoxAlignment.Center;
                                    column.Header.CheckBoxVisibility = HeaderCheckBoxVisibility.Always;

                                    // Force this column to appear last (after Reason)
                                    column.NullText = "";
                                    column.Header.VisiblePosition = 9; // Force position after Reason

                                    // Add event handler for header checkbox click via cell click event
                                    // (HeaderCheckBoxClick not directly available in UltraGrid)
                                    ultraGrid1.MouseUp -= UltraGrid1_MouseUp_ForHeaderCheckbox;
                                    ultraGrid1.MouseUp += UltraGrid1_MouseUp_ForHeaderCheckbox;
                                    break;

                                case "Sl No":
                                    column.Width = 60;
                                    column.CellActivation = Activation.NoEdit;
                                    column.Header.Caption = "SlNo";
                                    column.CellAppearance.TextHAlign = HAlign.Center;
                                    break;

                                case "Description":
                                    column.Width = 200;
                                    column.CellActivation = Activation.NoEdit;
                                    column.Header.Caption = "Item Name";
                                    break;

                                case "Barcode":
                                    column.Width = 120;
                                    column.CellActivation = Activation.NoEdit;
                                    column.Header.Caption = "Barcode";
                                    break;

                                case "Unit":
                                    column.Width = 80;
                                    column.CellActivation = Activation.AllowEdit;
                                    column.Header.Caption = "Unit";
                                    column.CellAppearance.TextHAlign = HAlign.Center;
                                    break;

                                case "Packing":
                                    column.Width = 80;
                                    column.CellActivation = Activation.AllowEdit;
                                    column.Header.Caption = "Packing";
                                    column.CellAppearance.TextHAlign = HAlign.Center;
                                    break;

                                case "Cost":
                                    column.Width = 100;
                                    column.CellActivation = Activation.AllowEdit;
                                    column.Format = "N2";
                                    column.Header.Caption = "Cost";
                                    column.CellAppearance.TextHAlign = HAlign.Center;
                                    break;

                                case "Quantity":
                                    column.Width = 80;
                                    column.CellActivation = Activation.AllowEdit;
                                    column.Header.Caption = "Qty";
                                    column.CellAppearance.TextHAlign = HAlign.Center;
                                    column.Format = "N2";
                                    break;

                                case "Amount":
                                    column.Width = 120;
                                    column.CellActivation = Activation.AllowEdit;
                                    column.Format = "N2";
                                    column.Header.Caption = "Amount";
                                    column.CellAppearance.TextHAlign = HAlign.Center;
                                    break;

                                case "Reason":
                                    column.Width = 150; // Wider column to better display dropdown options
                                    column.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownList;
                                    column.CellActivation = Activation.AllowEdit;
                                    column.Header.Caption = "Reason";
                                    column.CellAppearance.TextHAlign = HAlign.Center;

                                    // Create ValueList for Reason dropdown with requested options
                                    Infragistics.Win.ValueList reasonList = new Infragistics.Win.ValueList();
                                    reasonList.ValueListItems.Add(new Infragistics.Win.ValueListItem("Select Reason"));
                                    reasonList.ValueListItems.Add(new Infragistics.Win.ValueListItem("Expired"));
                                    reasonList.ValueListItems.Add(new Infragistics.Win.ValueListItem("Damaged"));
                                    reasonList.ValueListItems.Add(new Infragistics.Win.ValueListItem("NonOrdered"));
                                    reasonList.ValueListItems.Add(new Infragistics.Win.ValueListItem("NonDemand"));

                                    // Set the default item
                                    reasonList.ValueListItems[0].DataValue = "Select Reason";

                                    column.ValueList = reasonList;
                                    column.CellAppearance.TextHAlign = HAlign.Left;

                                    // Make sure that the reason values from the database are respected
                                    // The dropdown style is already set by using ColumnStyle.DropDownList above

                                    // Log the current values for debugging
                                    System.Diagnostics.Debug.WriteLine("Current reason values in grid:");
                                    if (ultraGrid1.Rows.Count > 0)
                                    {
                                        foreach (UltraGridRow row in ultraGrid1.Rows)
                                        {
                                            if (row.Cells.Exists("Reason"))
                                            {
                                                object reasonValue = row.Cells["Reason"].Value;
                                                System.Diagnostics.Debug.WriteLine($"Row {row.Index}: Reason = {reasonValue}");
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }

                    // Ensure ItemID is always hidden 
                    if (band.Columns.Exists("ItemID"))
                    {
                        band.Columns["ItemID"].Hidden = true;
                    }

                    // Apply SalesReturn grid styling
                    // Basic grid behavior
                    ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                    ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                    ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                    ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                    ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                    ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                    ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                    // Ensure grid fills all available space
                    ultraGrid1.Dock = DockStyle.Fill;
                    ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
                    ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
                    ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                    ultraGrid1.DisplayLayout.Scrollbars = Scrollbars.Both;

                    // Configure header appearance with modern gradient look
                    ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(0, 122, 204); // Modern blue color
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(0, 102, 184); // Slightly darker blue for gradient
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                    ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

                    // Configure row appearance
                    ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                    ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = SystemColors.Menu;
                    ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.Vertical;

                    // Set consistent row spacing to match other forms (2 pixels)
                    ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 2;
                    ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                    ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 22;

                    // Configure spacing and expansion behavior
                    ultraGrid1.DisplayLayout.InterBandSpacing = 0;
                    ultraGrid1.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;

                    // Register event handlers for cell editing
                    ultraGrid1.BeforeEnterEditMode -= ultraGrid1_BeforeEnterEditMode;
                    ultraGrid1.BeforeEnterEditMode += ultraGrid1_BeforeEnterEditMode;

                    ultraGrid1.BeforeCellUpdate -= ultraGrid1_BeforeCellUpdate;
                    ultraGrid1.BeforeCellUpdate += ultraGrid1_BeforeCellUpdate;

                    ultraGrid1.KeyDown -= ultraGrid1_KeyDown;
                    ultraGrid1.KeyDown += ultraGrid1_KeyDown;

                    ultraGrid1.BeforeExitEditMode -= ultraGrid1_BeforeExitEditMode;
                    ultraGrid1.BeforeExitEditMode += ultraGrid1_BeforeExitEditMode;

                    // Register the InitializeLayout event for styling
                    ultraGrid1.InitializeLayout -= ultraGrid1_InitializeLayout;
                    ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;

                    // The following lines will now be handled by PreserveReasonValues after the grid is configured
                    PreserveReasonValues();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error configuring grid layout: " + ex.Message);
            }
        }

        // Method to ensure that any existing Reason values from the database are preserved
        private void PreserveReasonValues()
        {
            try
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Preserving Reason values in grid...");

                    // Store the current reason values
                    var reasonValues = new Dictionary<int, string>();

                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (row.Cells.Exists("Reason") && row.Cells["Reason"].Value != null)
                        {
                            string currentReason = row.Cells["Reason"].Value.ToString();
                            if (!string.IsNullOrWhiteSpace(currentReason) && currentReason != "Select Reason")
                            {
                                reasonValues[row.Index] = currentReason;
                                System.Diagnostics.Debug.WriteLine($"Storing reason '{currentReason}' for row {row.Index}");
                            }
                        }
                    }

                    // Update the values to ensure they match the dropdown options
                    foreach (var kvp in reasonValues)
                    {
                        int rowIndex = kvp.Key;
                        string reasonValue = kvp.Value;

                        if (ultraGrid1.Rows.Count > rowIndex)
                        {
                            UltraGridRow row = ultraGrid1.Rows[rowIndex];
                            if (row.Cells.Exists("Reason"))
                            {
                                // Make sure the reason is one of our valid options
                                string normalizedReason = reasonValue;

                                // Normalize common variations
                                normalizedReason = normalizedReason.Replace(" ", "").Replace("-", "").Trim();

                                if (normalizedReason.Equals("Expired", StringComparison.OrdinalIgnoreCase))
                                    row.Cells["Reason"].Value = "Expired";
                                else if (normalizedReason.Equals("Damaged", StringComparison.OrdinalIgnoreCase))
                                    row.Cells["Reason"].Value = "Damaged";
                                else if (normalizedReason.Equals("NonOrdered", StringComparison.OrdinalIgnoreCase) ||
                                         normalizedReason.Equals("Non Ordered", StringComparison.OrdinalIgnoreCase))
                                    row.Cells["Reason"].Value = "NonOrdered";
                                else if (normalizedReason.Equals("NonDemand", StringComparison.OrdinalIgnoreCase) ||
                                         normalizedReason.Equals("Non Demand", StringComparison.OrdinalIgnoreCase))
                                    row.Cells["Reason"].Value = "NonDemand";
                                else
                                    row.Cells["Reason"].Value = reasonValue; // Keep the original if it doesn't match

                                System.Diagnostics.Debug.WriteLine($"Updated row {rowIndex} reason to '{row.Cells["Reason"].Value}'");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error preserving reason values: {ex.Message}");
            }
        }

        private void ultraGrid1_BeforeEnterEditMode(object sender, CancelEventArgs e)
        {
            // Store the original value before entering edit mode
            if (ultraGrid1.ActiveCell != null)
            {
                _originalValue = ultraGrid1.ActiveCell.Value;
            }
        }

        private void ultraGrid1_BeforeExitEditMode(object sender, CancelEventArgs e)
        {
            // Track if the value was modified during edit (for Cost and Quantity cells)
            if (ultraGrid1.ActiveCell != null &&
                (ultraGrid1.ActiveCell.Column.Key == "Cost" || ultraGrid1.ActiveCell.Column.Key == "Quantity"))
            {
                // Compare original and new values
                bool valueChanged = false;

                if (_originalValue == null && ultraGrid1.ActiveCell.Value != null)
                    valueChanged = true;
                else if (_originalValue != null && ultraGrid1.ActiveCell.Value == null)
                    valueChanged = true;
                else if (_originalValue != null && ultraGrid1.ActiveCell.Value != null &&
                         !_originalValue.ToString().Equals(ultraGrid1.ActiveCell.Value.ToString()))
                    valueChanged = true;

                if (valueChanged)
                {
                    _valueEditedAfterEnter = true;
                    System.Diagnostics.Debug.WriteLine($"Value changed in {ultraGrid1.ActiveCell.Column.Key} from {_originalValue} to {ultraGrid1.ActiveCell.Value}");
                }
            }
        }

        private object _originalValue = null;
        private bool _enterPressed = false; // Used in BeforeCellUpdate to control cell updates
        private bool _isUpdatingCells = false; // Add flag to prevent recursive updates
        private DateTime _lastEnterPress = DateTime.MinValue; // Track time of last Enter key press
        private UltraGridCell _lastEnterCell = null; // Track the cell where Enter was last pressed
        private bool _valueEditedAfterEnter = false; // Track if value was edited after Enter press

        private void ultraGrid1_BeforeCellUpdate(object sender, BeforeCellUpdateEventArgs e)
        {
            // For editable columns, always allow updates
            if (e.Cell.Column.Key == "Cost" ||
                e.Cell.Column.Key == "Quantity" ||
                e.Cell.Column.Key == "Reason")
            {
                return; // Don't cancel, allow the update
            }

            // For other columns, use the default behavior
            // This allows Amount to be updated when Cost and Quantity change
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                // Handle Down Arrow key specifically
                if (e.KeyCode == Keys.Down)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    UltraGridRow currentRow = ultraGrid1.ActiveRow;
                    if (currentRow != null && currentRow.Index < ultraGrid1.Rows.Count - 1)
                    {
                        UltraGridRow nextRow = ultraGrid1.Rows[currentRow.Index + 1];
                        string currentColumnKey = ultraGrid1.ActiveCell.Column.Key;

                        // Keep focus in the same column when moving down
                        if (nextRow.Cells.Exists(currentColumnKey))
                        {
                            ultraGrid1.ActiveRow = nextRow;
                            ultraGrid1.ActiveCell = nextRow.Cells[currentColumnKey];
                            ultraGrid1.Selected.Rows.Clear();
                            ultraGrid1.Selected.Rows.Add(nextRow);

                            // Enter edit mode if appropriate
                            if (nextRow.Cells[currentColumnKey].Column.CellActivation == Activation.AllowEdit)
                            {
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            }
                        }
                    }
                    return;
                }

                // Handle Up Arrow key specifically
                if (e.KeyCode == Keys.Up)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    UltraGridRow currentRow = ultraGrid1.ActiveRow;
                    if (currentRow != null && currentRow.Index > 0)
                    {
                        UltraGridRow prevRow = ultraGrid1.Rows[currentRow.Index - 1];
                        string currentColumnKey = ultraGrid1.ActiveCell.Column.Key;

                        // Keep focus in the same column when moving up
                        if (prevRow.Cells.Exists(currentColumnKey))
                        {
                            ultraGrid1.ActiveRow = prevRow;
                            ultraGrid1.ActiveCell = prevRow.Cells[currentColumnKey];
                            ultraGrid1.Selected.Rows.Clear();
                            ultraGrid1.Selected.Rows.Add(prevRow);

                            // Enter edit mode if appropriate
                            if (prevRow.Cells[currentColumnKey].Column.CellActivation == Activation.AllowEdit)
                            {
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            }
                        }
                    }
                    // If at the first row, keep the focus on the first row
                    return;
                }

                if (e.KeyCode == Keys.Enter && !e.Control && !e.Alt && !e.Shift)
                {
                    // Mark the event as handled to prevent default enter behavior
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    // Set the _enterPressed flag to true
                    _enterPressed = true;

                    if (ultraGrid1.ActiveCell != null)
                    {
                        UltraGridRow currentRow = ultraGrid1.ActiveRow;
                        string currentColumn = ultraGrid1.ActiveCell.Column.Key;

                        // Store current cell before exiting edit mode
                        UltraGridCell currentActiveCell = ultraGrid1.ActiveCell;

                        // First, exit edit mode to apply any pending changes
                        if (ultraGrid1.ActiveCell.IsInEditMode)
                        {
                            // Force exit edit mode to apply pending changes
                            ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);

                            // Important: Allow the UI to update
                            Application.DoEvents();
                        }

                        // Special handling for Cost and Quantity cells - calculate Amount immediately
                        if ((currentColumn == "Cost" || currentColumn == "Quantity") &&
                            currentRow != null &&
                            currentRow.Cells.Exists("Cost") &&
                            currentRow.Cells.Exists("Quantity") &&
                            currentRow.Cells.Exists("Amount"))
                        {
                            // Force calculation and update of the Amount cell with highlighting if amount changed
                            CalculateAndUpdateRow(currentRow, true);

                            // Log the action
                            System.Diagnostics.Debug.WriteLine($"Enter pressed in {currentColumn}: Amount calculated");

                            // Check if this is the second Enter press in the same cell AND no value change since last Enter
                            bool isSameCell = (_lastEnterCell == currentActiveCell);
                            DateTime now = DateTime.Now;
                            TimeSpan timeSinceLastEnter = now - _lastEnterPress;

                            // If this is the second Enter press in the same cell (within 2 seconds) AND no value change since last Enter
                            if (isSameCell && timeSinceLastEnter.TotalSeconds <= 2 &&
                                _lastEnterPress != DateTime.MinValue && !_valueEditedAfterEnter)
                            {
                                // Reset tracking variables
                                _lastEnterPress = DateTime.MinValue;
                                _lastEnterCell = null;
                                _valueEditedAfterEnter = false;

                                // Move to the next logical cell
                                string nextColumn = GetNextColumnForNavigation(currentColumn);
                                if (currentRow.Cells.Exists(nextColumn))
                                {
                                    ultraGrid1.ActiveCell = currentRow.Cells[nextColumn];
                                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                    _enterPressed = false; // Reset flag before returning
                                    return;
                                }
                            }
                            else
                            {
                                // This is the first Enter press or value was edited - update tracking variables
                                _lastEnterCell = currentActiveCell;
                                _lastEnterPress = now;
                                // Reset the value edited flag for next time
                                _valueEditedAfterEnter = false;

                                // Stay in the current cell
                                ultraGrid1.ActiveCell = currentActiveCell;
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                _enterPressed = false; // Reset flag before returning
                                return;
                            }
                        }
                        else
                        {
                            // For cells other than Cost/Quantity, do NOT highlight the Amount cell
                            // even if a calculation is needed
                            if (currentRow != null &&
                                currentRow.Cells.Exists("Cost") &&
                                currentRow.Cells.Exists("Quantity") &&
                                currentRow.Cells.Exists("Amount"))
                            {
                                // We'll update the Amount, but without highlighting
                                decimal cost = 0, qty = 0;
                                if (currentRow.Cells["Cost"].Value != null)
                                    decimal.TryParse(currentRow.Cells["Cost"].Value.ToString(), out cost);
                                if (currentRow.Cells["Quantity"].Value != null)
                                    decimal.TryParse(currentRow.Cells["Quantity"].Value.ToString(), out qty);

                                decimal amount = Math.Round(cost * qty, 2);
                                currentRow.Cells["Amount"].Value = amount;

                                // Update net total without highlighting
                                UpdateNetAmount();
                            }

                            // For non-Cost/Quantity cells, move to next cell immediately on first Enter
                            // Standard navigation logic
                            if (currentColumn == "Reason" && currentRow.Index < ultraGrid1.Rows.Count - 1)
                            {
                                // Move to the first cell of the next row
                                UltraGridRow nextRow = ultraGrid1.Rows[currentRow.Index + 1];
                                UltraGridCell firstCell = FindFirstEditableCell(nextRow);
                                if (firstCell != null)
                                {
                                    ultraGrid1.ActiveRow = nextRow;
                                    ultraGrid1.ActiveCell = firstCell;
                                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                }
                            }
                            else
                            {
                                // Move to next cell in standard navigation order
                                string nextColumn = GetNextColumnForNavigation(currentColumn);
                                if (currentRow.Cells.Exists(nextColumn))
                                {
                                    ultraGrid1.ActiveCell = currentRow.Cells[nextColumn];
                                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                }
                            }
                        }
                    }

                    // Reset the _enterPressed flag before returning
                    _enterPressed = false;
                    return;
                }

                // Track numeric key presses in Cost/Quantity cells to reset the valueEdited flag
                if (ultraGrid1.ActiveCell != null &&
                    (ultraGrid1.ActiveCell.Column.Key == "Cost" || ultraGrid1.ActiveCell.Column.Key == "Quantity") &&
                    (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9 ||
                     e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9 ||
                     e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete ||
                     e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod))
                {
                    // User is typing in the cell, set the valueEdited flag
                    _valueEditedAfterEnter = true;
                }

                // Handle Tab navigation
                else if (e.KeyCode == Keys.Tab && !e.Control && !e.Alt)
                {
                    if (ultraGrid1.ActiveCell != null)
                    {
                        // End edit mode first to apply any changes
                        if (ultraGrid1.ActiveCell.IsInEditMode)
                        {
                            ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                        }

                        // Force calculation for any pending values, but WITHOUT highlighting
                        if (ultraGrid1.ActiveRow != null)
                        {
                            UltraGridRow row = ultraGrid1.ActiveRow;
                            if (row.Cells.Exists("Cost") && row.Cells.Exists("Quantity") && row.Cells.Exists("Amount"))
                            {
                                // Calculate without highlighting
                                decimal cost = 0, qty = 0;
                                if (row.Cells["Cost"].Value != null)
                                    decimal.TryParse(row.Cells["Cost"].Value.ToString(), out cost);
                                if (row.Cells["Quantity"].Value != null)
                                    decimal.TryParse(row.Cells["Quantity"].Value.ToString(), out qty);

                                decimal amount = Math.Round(cost * qty, 2);
                                row.Cells["Amount"].Value = amount;

                                // Update net total
                                UpdateNetAmount();
                            }
                        }

                        // Find next/previous cell based on Shift state
                        UltraGridCell nextCell = null;
                        UltraGridRow currentRow = ultraGrid1.ActiveRow;

                        if (e.Shift)
                        {
                            // Shift+Tab - Move to previous cell
                            nextCell = FindPreviousEditableCell(currentRow, ultraGrid1.ActiveCell);

                            // If we're at the first cell of the current row, try to move to the last cell of the previous row
                            if (nextCell == null || nextCell == ultraGrid1.ActiveCell)
                            {
                                if (currentRow.Index > 0)
                                {
                                    UltraGridRow prevRow = ultraGrid1.Rows[currentRow.Index - 1];
                                    nextCell = FindLastEditableCell(prevRow);
                                }
                                else
                                {
                                    // If we're at the first cell of the first row, cycle to form controls
                                    e.Handled = true;
                                    TxtBarcode.Focus(); // Move focus to the last form control
                                    return;
                                }
                            }
                        }
                        else
                        {
                            // Regular Tab - Move to next cell
                            nextCell = FindNextEditableCell(currentRow, ultraGrid1.ActiveCell);

                            // If we're at the last cell of the current row, try to move to the first cell of the next row
                            if (nextCell == null || nextCell == ultraGrid1.ActiveCell)
                            {
                                if (currentRow.Index < ultraGrid1.Rows.Count - 1)
                                {
                                    UltraGridRow nextRow = ultraGrid1.Rows[currentRow.Index + 1];
                                    nextCell = FindFirstEditableCell(nextRow);
                                }
                                else
                                {
                                    // If we're at the last cell of the last row, cycle to form controls
                                    e.Handled = true;
                                    Vendorbutton.Focus(); // Move focus to the first form control
                                    return;
                                }
                            }
                        }

                        // Move focus to the next/previous cell
                        if (nextCell != null)
                        {
                            ultraGrid1.ActiveCell = nextCell;
                            ultraGrid1.ActiveRow = nextCell.Row;
                            ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            e.Handled = true;
                        }
                    }
                }
                // Handle Delete key for removing rows
                else if (e.KeyCode == Keys.Delete)
                {
                    UltraGridRow activeRow = ultraGrid1.ActiveRow;
                    if (activeRow != null)
                    {
                        // Check if items are loaded via button1 (purchasereturnupdate)
                        if (this.Tag != null && this.Tag.ToString() == "PurchaseReturnUpdate")
                        {
                            MessageBox.Show("Cannot remove items when purchase return data is loaded from update form.",
                                "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Delete the row
                        if (ultraGrid1.DataSource is DataTable dt)
                        {
                            int rowIndex = activeRow.Index;
                            dt.Rows.RemoveAt(rowIndex);

                            // Renumber Sl No column for remaining rows
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                dt.Rows[i]["Sl No"] = i + 1;
                            }

                            // Update amount calculations
                            UpdateNetAmount();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in ultraGrid1_KeyDown: " + ex.Message);
            }
        }

        private void ultraGrid1_AfterCellUpdate(object sender, CellEventArgs e)
        {
            try
            {
                // Reset the _enterPressed flag after any cell update
                _enterPressed = false;

                // If we're already updating cells due to navigation or calculation,
                // avoid getting into an update loop
                if (_isUpdatingCells) return;

                // Don't process if the cell is null or the row is invalid
                if (e.Cell == null || e.Cell.Row == null) return;

                UltraGridRow currentRow = e.Cell.Row;
                string columnKey = e.Cell.Column.Key;

                // Handle different column updates differently
                switch (columnKey)
                {
                    case "Cost":
                    case "Quantity":
                        // When Cost or Quantity changes, recalculate the Amount with highlighting
                        CalculateAndUpdateRow(currentRow, true);
                        break;

                    case "Packing":
                        // Update will come from Cost/Quantity handlers - don't duplicate
                        break;

                    case "Amount":
                        // If Amount is directly edited, update net amount without recalculating
                        UpdateNetAmount();
                        break;

                    case "Reason":
                        // For Reason changes, just update the row data without visual effects
                        break;

                    case "SELECT":
                        // If the SELECT column is toggled, update its visual state
                        break;

                    default:
                        // For any other columns, just maintain the grid's state
                        break;
                }

                // If the cell that changed was the primary key (ItemID), save this value
                if (columnKey == "ItemID" && e.Cell.Value != null)
                {
                    // Store the ID if needed for reference
                    long itemId = Convert.ToInt64(e.Cell.Value);
                    System.Diagnostics.Debug.WriteLine($"Item ID changed to {itemId}");
                }

                // Log the operation for debugging
                System.Diagnostics.Debug.WriteLine($"Cell updated: {columnKey} in row {currentRow.Index}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AfterCellUpdate: {ex.Message}");
                _isUpdatingCells = false; // Reset flag in case of exception
                _enterPressed = false; // Reset enter flag in case of exception
            }
        }

        private void UpdateNetAmount()
        {
            try
            {
                // Calculate the net amount
                decimal netAmount = 0m;
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("Amount") &&
                        row.Cells["Amount"].Value != null &&
                        row.Cells["Amount"].Value != DBNull.Value)
                    {
                        netAmount += Convert.ToDecimal(row.Cells["Amount"].Value);
                    }
                }

                // Round the net amount to 2 decimal places
                netAmount = Math.Round(netAmount, 2);

                // Format the value with 2 decimal places
                string formattedAmount = netAmount.ToString("N2");

                // Update lblNetAmount
                if (lblNetAmount != null)
                {
                    lblNetAmount.Text = formattedAmount;
                    lblNetAmount.Refresh();
                }

                // Update textBox2 (purchase amount)
                if (textBox2 != null)
                {
                    textBox2.Text = formattedAmount;
                    textBox2.Refresh();
                }

                // Update TxtSubTotal to match textBox2
                if (TxtSubTotal != null)
                {
                    TxtSubTotal.Text = formattedAmount;
                    TxtSubTotal.Refresh();
                }

                // Log the update to debug output
                System.Diagnostics.Debug.WriteLine($"Updated Net Amount: {formattedAmount}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating net amount: " + ex.Message);
            }
        }

        private void pbxSave_Click(object sender, EventArgs e)
        {
            // Make sure this button stays visible when clicked
            pbxSave.Visible = true;
            ultraPictureBox4.Visible = false;

            SavePurchaseReturn();
        }

        private void ultraPictureBox1_Click(object sender, EventArgs e)
        {
            // Call ClearAllFields when ultraPictureBox1 is clicked
            ClearAllFields();
        }

        private void ultraPictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pbxExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ultraPictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void lblSave_Click(object sender, EventArgs e)
        {
            SavePurchaseReturn();
        }

        private void ultraLabel5_Click(object sender, EventArgs e)
        {
            // Call ClearAllFields method to clear all fields
            ClearAllFields();
        }

        private void ultraLabel2_Click(object sender, EventArgs e)
        {

        }

        private void ultraLabel3_Click(object sender, EventArgs e)
        {

        }

        private void ultraLabel4_Click(object sender, EventArgs e)
        {

        }

        private void ultraPanel1_PaintClient(object sender, PaintEventArgs e)
        {

        }

        private void ultraPanel4_PaintClient(object sender, PaintEventArgs e)
        {

        }

        private void NetTotal_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Show pbxSave and hide ultraPictureBox4
            pbxSave.Visible = true;
            ultraPictureBox4.Visible = false;

            // Reset the purchase data loaded flag to allow loading new items
            _isPurchaseDataLoaded = false;

            frmVendorDig frmVend = new frmVendorDig();
            if (frmVend.ShowDialog() == DialogResult.OK)
            {
                // Get the selected vendor data from the dialog
                int vendorId = frmVend.SelectedVendorId;
                string vendorName = frmVend.SelectedVendorName;

                // Update the UI with the selected vendor information
                VendorName.Text = vendorName;
                vendorid.Text = vendorId.ToString();

                // Clear related fields when a new vendor is selected
                textBox1.Text = string.Empty;
                textBox2.Text = string.Empty;

                // Clear the date editor if it exists
                if (ultraDateTimeEditor2 != null)
                {
                    ultraDateTimeEditor2.Value = null;
                }

                // Clear the grid completely
                try
                {
                    // First clear selection
                    if (ultraGrid1.Selected != null && ultraGrid1.Selected.Rows != null)
                    {
                        ultraGrid1.Selected.Rows.Clear();
                    }

                    // Clear active row
                    ultraGrid1.ActiveRow = null;

                    // Clear the data source with a new empty table
                    DataTable emptyTable = new DataTable();

                    // Create essential columns to maintain structure
                    emptyTable.Columns.Add("Sl No", typeof(int));
                    emptyTable.Columns.Add("Description", typeof(string));
                    emptyTable.Columns.Add("Barcode", typeof(string));
                    emptyTable.Columns.Add("Unit", typeof(string));
                    emptyTable.Columns.Add("Packing", typeof(double));
                    emptyTable.Columns.Add("Cost", typeof(decimal));
                    emptyTable.Columns.Add("Quantity", typeof(double));
                    emptyTable.Columns.Add("Amount", typeof(decimal));
                    emptyTable.Columns.Add("Reason", typeof(string));
                    emptyTable.Columns.Add("SELECT", typeof(bool)); // Add SELECT column at the end

                    // Hidden columns
                    emptyTable.Columns.Add("ItemID", typeof(long));
                    emptyTable.Columns.Add("UnitId", typeof(int));
                    emptyTable.Columns.Add("TaxPer", typeof(decimal));
                    emptyTable.Columns.Add("TaxAmt", typeof(decimal));
                    emptyTable.Columns.Add("TaxType", typeof(string));

                    // Remove the code that adds an empty row
                    // Set the empty table as data source without adding any rows
                    ultraGrid1.DataSource = emptyTable;
                    // Reset any tag that might have been set
                    ultraGrid1.Tag = null;

                    // Reconfigure the grid layout
                    ConfigureItemsGridLayout();

                    // Make sure columns are visible since we have a row
                    if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                    {
                        string[] visibleColumns = new string[] {
                            "Sl No", "Description", "Barcode", "Unit", "Packing", "Cost",
                            "Quantity", "Amount", "Reason", "SELECT"
                        };

                        foreach (string columnName in visibleColumns)
                        {
                            if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(columnName))
                            {
                                ultraGrid1.DisplayLayout.Bands[0].Columns[columnName].Hidden = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error clearing grid: {ex.Message}");
                }

                // Reset the Net Amount label
                lblNetAmount.Text = "0.00";

                // Reset the TxtBarcode ReadOnly state
                TxtBarcode.ReadOnly = _originalTxtBarcodeReadOnly;
            }
        }

        private void VendorName_TextChanged(object sender, EventArgs e)
        {

        }

        private void vendorid_Click(object sender, EventArgs e)
        {

        }

        private void GeneratePurchaseReturnNumber()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Get all purchase returns to find the last number
                var allPRs = prRepo.GetAll();

                if (allPRs != null && allPRs.Any())
                {
                    // Get the highest PR number and increment it
                    int lastPRNumber = allPRs.Max(x => x.PReturnNo);
                    TxtSRNO.Text = (lastPRNumber + 1).ToString("D4");
                }
                else
                {
                    // If no existing records, start with 1
                    TxtSRNO.Text = "0001";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating PR number: {ex.Message}");
                TxtSRNO.Text = "0001";
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        // Method to save the purchase return
        private void SavePurchaseReturn()
        {
            try
            {
                // Validation
                if (string.IsNullOrEmpty(vendorid.Text))
                {
                    MessageBox.Show("Please select a vendor first.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (ultraGrid1.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one item to return.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Set cursor to wait
                Cursor.Current = Cursors.WaitCursor;

                // Create the purchase return master record
                PReturnMaster prMaster = new PReturnMaster();
                prMaster.CompanyId = SessionContext.CompanyId;
                prMaster.FinYearId = SessionContext.FinYearId;
                prMaster.BranchId = SessionContext.BranchId;
                prMaster.BranchName = DataBase.Branch;
                prMaster.PReturnNo = 0; // Will be generated by the repository
                prMaster.PReturnDate = ultraDateTimeEditor1.Value != null ? (DateTime)ultraDateTimeEditor1.Value : DateTime.Now;

                // Get purchase invoice details
                prMaster.PInvoice = !string.IsNullOrEmpty(textBox1.Text) ? textBox1.Text : "";
                prMaster.InvoiceNo = !string.IsNullOrEmpty(textBox1.Text) ? textBox1.Text : "";
                prMaster.InvoiceDate = ultraDateTimeEditor2.Value != null ? (DateTime)ultraDateTimeEditor2.Value : DateTime.Now;

                // Get vendor details
                prMaster.LedgerID = Convert.ToInt32(vendorid.Text);
                prMaster.VendorName = VendorName.Text;

                // Get payment method (cmbPaymntMethod)
                if (cmbPaymntMethod.SelectedItem != null)
                {
                    prMaster.PaymodeID = Convert.ToInt32(cmbPaymntMethod.SelectedValue);
                    prMaster.Paymode = cmbPaymntMethod.Text;
                }
                else
                {
                    prMaster.PaymodeID = 0;
                    prMaster.Paymode = "";
                }

                prMaster.PaymodeLedgerID = 0;
                prMaster.CreditPeriod = 0;

                // Get subtotal and grand total
                decimal subTotal = 0;
                if (TxtSubTotal.Text != "")
                {
                    subTotal = Convert.ToDecimal(TxtSubTotal.Text);
                }
                prMaster.SubTotal = Convert.ToDouble(subTotal);

                // Set other financial fields
                prMaster.SpDisPer = 0;
                prMaster.SpDsiAmt = 0;
                prMaster.BillDiscountPer = 0;
                prMaster.BillDiscountAmt = 0;

                // Calculate total tax amount from items
                decimal totalTaxAmount = 0;
                decimal taxPercentage = 0;
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("TaxAmt") && row.Cells["TaxAmt"].Value != DBNull.Value)
                    {
                        totalTaxAmount += Convert.ToDecimal(row.Cells["TaxAmt"].Value);
                    }

                    // Use the tax percentage from the first item as a reference
                    if (taxPercentage == 0 && row.Cells.Exists("TaxPer") && row.Cells["TaxPer"].Value != DBNull.Value)
                    {
                        taxPercentage = Convert.ToDecimal(row.Cells["TaxPer"].Value);
                    }
                }

                prMaster.TaxPer = Convert.ToDouble(taxPercentage);
                prMaster.TaxAmt = Convert.ToDouble(totalTaxAmount);
                prMaster.Frieght = 0;
                prMaster.ExpenseAmt = 0;
                prMaster.OtherExpAmt = 0;

                // Get grand total from the Net Amount label
                decimal grandTotal = 0;
                if (!string.IsNullOrEmpty(lblNetAmount.Text))
                {
                    grandTotal = Convert.ToDecimal(lblNetAmount.Text);
                }
                prMaster.GrandTotal = Convert.ToDouble(grandTotal);

                // Set other fields
                prMaster.CancelFlag = false;
                prMaster.UserID = SessionContext.UserId;
                prMaster.UserName = DataBase.UserName;

                // Get tax type from the first item that has it
                string taxType = "I"; // Default to Inclusive
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("TaxType") && row.Cells["TaxType"].Value != DBNull.Value)
                    {
                        taxType = row.Cells["TaxType"].ToString();
                        break;
                    }
                }
                prMaster.TaxType = taxType;

                prMaster.Remarks = "";

                // Get round off amount
                decimal roundOff = 0;


                prMaster.CessPer = 0;
                prMaster.CessAmt = 0;
                prMaster.CalAfterTax = 0;
                prMaster.CurrencyID = 0;
                prMaster.CurSymbol = "";
                prMaster.SeriesID = 0;
                prMaster.VoucherID = 0;

                // Set the operation to CREATE
                prMaster._Operation = "CREATE";

                // Create a list to hold the purchase return details
                List<PReturnDetails> prDetailsList = new List<PReturnDetails>();

                // Process each row in the grid to create detail records
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    try
                    {
                        PReturnDetails detailItem = new PReturnDetails();
                        detailItem.CompanyId = SessionContext.CompanyId;
                        detailItem.FinYearId = SessionContext.FinYearId;
                        detailItem.BranchID = SessionContext.BranchId;
                        detailItem.BranchName = DataBase.Branch;
                        detailItem.PReturnNo = 0; // Will be set by the repository
                        detailItem.PReturnDate = prMaster.PReturnDate;
                        detailItem.InvoiceNo = prMaster.InvoiceNo;

                        // Get the SlNo from the grid
                        detailItem.SlNo = Convert.ToInt32(row.Cells["Sl No"].Value);

                        // Get the ItemID from the grid
                        detailItem.ItemID = Convert.ToInt64(row.Cells["ItemID"].Value);

                        // Get the Description from the grid
                        detailItem.Description = row.Cells["Description"].Value.ToString();

                        // Get the UnitId from the grid
                        detailItem.UnitId = row.Cells.Exists("UnitId") ?
                            Convert.ToInt32(row.Cells["UnitId"].Value) : 0;

                        // Set BaseUnit to true since we're not tracking pack sizes
                        detailItem.BaseUnit = true;

                        // Get the Packing from the grid
                        detailItem.Packing = Convert.ToDouble(row.Cells["Packing"].Value);

                        // We're not tracking expiry data, so set these to defaults
                        detailItem.IsExpiry = false;
                        detailItem.BatchNo = "";
                        detailItem.Expiry = null;

                        // Get the Quantity from the grid
                        detailItem.Qty = Convert.ToDouble(row.Cells["Quantity"].Value);

                        // Get the TaxPer and TaxAmt from the grid if available
                        if (row.Cells.Exists("TaxPer"))
                        {
                            detailItem.TaxPer = Convert.ToDouble(row.Cells["TaxPer"].Value);
                        }

                        if (row.Cells.Exists("TaxAmt"))
                        {
                            detailItem.TaxAmt = Convert.ToDouble(row.Cells["TaxAmt"].Value);
                        }

                        // Get the Reason from the grid
                        detailItem.Reason = row.Cells["Reason"].Value.ToString();

                        // We're not tracking free items, so set to 0
                        detailItem.Free = 0;

                        // Get the Cost from the grid
                        detailItem.Cost = Convert.ToDouble(row.Cells["Cost"].Value);

                        // We're not tracking discounts, so set these to 0
                        detailItem.DisPer = 0;
                        detailItem.DisAmt = 0;

                        // We're not tracking sales price, so set to same as cost
                        detailItem.SalesPrice = detailItem.Cost;
                        detailItem.OriginalCost = detailItem.Cost;

                        // Calculate TotalSP as cost * quantity
                        detailItem.TotalSP = detailItem.Cost * detailItem.Qty;

                        // Calculate TotalAmount as displayed in the grid
                        detailItem.TotalAmount = Convert.ToDouble(row.Cells["Amount"].Value);

                        // We're not tracking cess, so set these to 0
                        detailItem.CessPer = 0;
                        detailItem.CessAmt = 0;

                        // Set operation to CREATE
                        detailItem._Operation = "CREATE";

                        // Add details to the list
                        prDetailsList.Add(detailItem);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing row {row.Index}: {ex.Message}");
                    }
                }

                // Create a single detail object for the repository method call
                PReturnDetails prDetailsParam = prDetailsList.First();

                // Call the repository to save the purchase return
                string result = prRepo.savePR(prMaster, prDetailsParam, null);

                if (result.ToUpper().Contains("SUCCESS"))
                {
                    MessageBox.Show("Purchase Return saved successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Generate a new number for the next purchase return
                    GeneratePurchaseReturnNumber();

                    // Clear all fields to prepare for a new entry
                    ClearAllFields();
                }
                else
                {
                    MessageBox.Show("Error saving purchase return: " + result, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving purchase return: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"SavePurchaseReturn error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void ultraGrid1_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs e)
        {
            try
            {
                // Apply modern styling similar to SalesReturn form

                // Apply row spacing before (reduced to minimize blank space)
                e.Layout.Override.RowSpacingBefore = 0;
                e.Layout.Override.CellSpacing = 0;

                // Set selection behavior
                e.Layout.Override.SelectTypeRow = SelectType.Single;

                // Configure header click behavior
                e.Layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;

                // Configure cell click action
                e.Layout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                // Basic configuration
                e.Layout.Override.AllowAddNew = AllowAddNew.No;
                e.Layout.Override.AllowDelete = DefaultableBoolean.False;
                e.Layout.Override.AllowUpdate = DefaultableBoolean.True;
                e.Layout.Override.RowSelectors = DefaultableBoolean.True;

                // Always ensure SELECT column's checkbox header is properly configured
                if (e.Layout.Bands.Count > 0 && e.Layout.Bands[0].Columns.Exists("SELECT"))
                {
                    UltraGridColumn selectCol = e.Layout.Bands[0].Columns["SELECT"];
                    selectCol.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                    selectCol.Header.CheckBoxAlignment = HeaderCheckBoxAlignment.Center;
                    selectCol.Header.CheckBoxVisibility = HeaderCheckBoxVisibility.Always;

                    // Remove the "SELECT" text from the header
                    selectCol.Header.Caption = "";

                    // Ensure SELECT column is after Reason
                    if (e.Layout.Bands[0].Columns.Exists("Reason"))
                    {
                        int reasonPos = e.Layout.Bands[0].Columns["Reason"].Header.VisiblePosition;
                        selectCol.Header.VisiblePosition = reasonPos + 1;
                    }
                }

                // Remove any margins and padding to maximize grid space utilization
                e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
                e.Layout.Override.RowSizing = RowSizing.AutoFree;

                // Configure header appearance with modern gradient look
                e.Layout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
                e.Layout.Override.HeaderAppearance.BackColor = Color.FromArgb(0, 122, 204); // Modern blue color
                e.Layout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(0, 102, 184); // Slightly darker blue for gradient
                e.Layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                e.Layout.Override.HeaderAppearance.ForeColor = Color.White;
                e.Layout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                e.Layout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
                e.Layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                e.Layout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                e.Layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

                // Configure row appearance
                e.Layout.Override.RowAppearance.BackColor = Color.White;
                e.Layout.Override.RowAppearance.BackColor2 = SystemColors.Menu;
                e.Layout.Override.RowAppearance.BackGradientStyle = GradientStyle.Vertical;

                // Row spacing and height (minimize spacing)
                e.Layout.Override.DefaultRowHeight = 22;
                e.Layout.Override.RowSpacingBefore = 2;

                // Set border style to dotted for cells and rows
                e.Layout.Override.BorderStyleCell = UIElementBorderStyle.Dotted;
                e.Layout.Override.BorderStyleRow = UIElementBorderStyle.Dotted;

                // Configure spacing and expansion behavior
                e.Layout.InterBandSpacing = 0;
                e.Layout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;

                // Ensure grid uses all available space
                ultraGrid1.Dock = DockStyle.Fill;

                // Set ScrollBars to use space efficiently
                e.Layout.ScrollBounds = ScrollBounds.ScrollToFill;
                e.Layout.Scrollbars = Scrollbars.Both;

                // Set vertical alignment for all cells to middle
                e.Layout.Override.CellAppearance.TextVAlign = VAlign.Middle;

                // Apply to all columns
                if (e.Layout.Bands.Count > 0)
                {
                    // Ensure SELECT column is after Reason column
                    if (e.Layout.Bands[0].Columns.Exists("SELECT") && e.Layout.Bands[0].Columns.Exists("Reason"))
                    {
                        // Get the current positions
                        int reasonPos = e.Layout.Bands[0].Columns["Reason"].Header.VisiblePosition;

                        // Explicitly set SELECT column position to be after Reason
                        e.Layout.Bands[0].Columns["SELECT"].Header.VisiblePosition = reasonPos + 1;
                    }

                    // Ensure columns fill width proportionally
                    float totalWeight = 0;
                    foreach (UltraGridColumn col in e.Layout.Bands[0].Columns)
                    {
                        if (!col.Hidden)
                        {
                            // Assign weight based on current width
                            col.Tag = col.Width;
                            totalWeight += col.Width;

                            // Set vertical alignment to middle for each column
                            col.CellAppearance.TextVAlign = VAlign.Middle;
                        }
                    }

                    foreach (UltraGridColumn col in e.Layout.Bands[0].Columns)
                    {
                        // Apply text alignment based on column content
                        if (col.Key == "Cost" || col.Key == "Quantity" || col.Key == "Amount" || col.Key == "Reason")
                        {
                            col.CellAppearance.TextHAlign = HAlign.Center;
                            col.CellAppearance.TextVAlign = VAlign.Middle;
                        }

                        // Format numeric columns
                        if (col.Key == "Cost" || col.Key == "Amount")
                        {
                            col.Format = "N2";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in InitializeLayout: " + ex.Message);
            }
        }

        private void TxtRoundOff_TextChanged(object sender, EventArgs e)
        {

        }

        private void TxtSubTotal_TextChanged(object sender, EventArgs e)
        {
            // Remove the code that updates lblNetAmount directly
            // lblNetAmount.Text = TxtSubTotal.Text;
        }

        private void btnAddPurchaceList_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if items are loaded via button1 (purchasereturnupdate)
                if (this.Tag != null && this.Tag.ToString() == "PurchaseReturnUpdate")
                {
                    MessageBox.Show("Invalid operation. Purchase return data is already loaded from update form.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if items are already loaded via BtnDial (dialforitemmaster)
                if (textBox1.Text == "WITHOUT GR")
                {
                    MessageBox.Show("Invalid operation. Items already loaded via direct selection.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Only check if items are being loaded via BtnDial
                // In the original context, btnDial is being used when TxtBarcode is read-only
                if (TxtBarcode.ReadOnly && !_originalTxtBarcodeReadOnly && textBox1.Text == "WITHOUT GR")
                {
                    MessageBox.Show("Invalid operation. Please finish the current item selection process before using this button.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Show pbxSave and hide ultraPictureBox4
                pbxSave.Visible = true;
                ultraPictureBox4.Visible = false;

                // If no vendor is selected, automatically open the vendor dialog
                if (string.IsNullOrEmpty(vendorid.Text))
                {
                    // Instead of showing an alert, automatically call button2_Click to open vendor selection
                    button2_Click(sender, e);
                    return;
                }

                // Get the selected vendor information
                int vendorId = Convert.ToInt32(vendorid.Text);
                string vendorName = VendorName.Text;

                // Open the purchase details dialog for the selected vendor
                FrmVendorPurchaseDetailsDialog purchaseDialog = new FrmVendorPurchaseDetailsDialog(vendorId, vendorName);
                if (purchaseDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the selected purchase information from the dialog
                    int purchaseNo = purchaseDialog.SelectedPurchaseNo;

                    // Only update if it's not already set to true from a previous operation
                    if (!TxtBarcode.ReadOnly)
                    {
                        _originalTxtBarcodeReadOnly = false;
                    }

                    // Make TxtBarcode writable
                    EnsureTxtBarcodeIsWritable();

                    DateTime invoiceDate = purchaseDialog.SelectedPurchaseDate;
                    string invoiceNo = purchaseDialog.SelectedPurchaseNo.ToString();

                    // Set the purchase number in textBox1
                    textBox1.Text = purchaseNo.ToString();

                    // Set the purchase date in ultraDateTimeEditor2
                    if (ultraDateTimeEditor2 != null)
                    {
                        ultraDateTimeEditor2.Value = invoiceDate;
                    }

                    // Set the invoice number in txtInvoiceNo if it exists
                    if (this.Controls.Find("txtInvoiceNo", true).Length > 0)
                    {
                        TextBox txtInvoiceNo = this.Controls.Find("txtInvoiceNo", true)[0] as TextBox;
                        if (txtInvoiceNo != null)
                        {
                            txtInvoiceNo.Text = invoiceNo;
                        }
                    }

                    // Set the invoice date if the control exists
                    if (this.Controls.Find("dtInvoiceDate", true).Length > 0)
                    {
                        DateTimePicker dtInvoiceDate = this.Controls.Find("dtInvoiceDate", true)[0] as DateTimePicker;
                        if (dtInvoiceDate != null)
                        {
                            dtInvoiceDate.Value = invoiceDate;
                        }
                    }

                    // Set the form Tag to indicate this is from a vendor search
                    this.Tag = "VendorSearch";

                    // Clear the existing grid data since we're loading a different purchase
                    if (ultraGrid1.DataSource != null)
                    {
                        DataTable dt = ultraGrid1.DataSource as DataTable;
                        if (dt != null)
                        {
                            dt.Clear();
                        }
                    }

                    // Reset the purchase data loaded flag before loading new data
                    _isPurchaseDataLoaded = false;

                    // Update the currently loaded purchase number
                    _currentlyLoadedPurchaseNo = purchaseNo;

                    // Load the purchase items into ultraGrid1
                    LoadPurchaseItems(vendorId, purchaseNo);

                    // Make sure the grid has focus and there is data
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        // Use BeginInvoke to ensure UI is fully updated before setting focus
                        this.BeginInvoke(new Action(() => {
                            try
                            {
                                // Get the first row
                                UltraGridRow firstRow = ultraGrid1.Rows[0];

                                // Set this row as the active row and clear any existing selection
                                ultraGrid1.ActiveRow = firstRow;
                                ultraGrid1.Selected.Rows.Clear();
                                ultraGrid1.Selected.Rows.Add(firstRow);

                                // Find the first editable cell in the row (Unit is preferred starting point)
                                UltraGridCell targetCell = null;

                                if (firstRow.Cells.Exists("Unit"))
                                {
                                    targetCell = firstRow.Cells["Unit"];
                                }
                                else if (firstRow.Cells.Exists("Description"))
                                {
                                    targetCell = firstRow.Cells["Description"];
                                }
                                else
                                {
                                    // Find any editable cell
                                    foreach (UltraGridCell cell in firstRow.Cells)
                                    {
                                        if (cell.Column.CellActivation == Activation.AllowEdit)
                                        {
                                            targetCell = cell;
                                            break;
                                        }
                                    }
                                }

                                if (targetCell != null)
                                {
                                    // Set focus to the target cell
                                    ultraGrid1.ActiveCell = targetCell;

                                    // Set focus to the grid and enter edit mode
                                    ultraGrid1.Focus();

                                    // Use PerformAction to enter edit mode if the cell is editable
                                    if (targetCell.Column.CellActivation == Activation.AllowEdit)
                                    {
                                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                    }

                                    // Scroll to ensure the row is visible
                                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(firstRow);

                                    // Update UI to show focus
                                    Application.DoEvents();
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error setting focus to grid after loading: {ex.Message}");
                            }
                        }));
                    }

                    // Make TxtBarcode read-only ONLY when loading items via btnAddPurchaceList
                    // But ensure it becomes writable when needed
                    TxtBarcode.ReadOnly = true;
                    _originalTxtBarcodeReadOnly = false; // Store original state for reference
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading purchase data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private UltraGridCell FindNextEditableCell(UltraGridRow row, UltraGridCell currentCell)
        {
            if (row == null || currentCell == null) return null;

            // List of allowed editable column keys in the desired order
            string[] editableColumns = new string[] { "Description", "Unit", "Packing", "Cost", "Quantity", "Amount", "Reason" };

            // Find the current column in our editableColumns array
            int currentPos = -1;
            for (int i = 0; i < editableColumns.Length; i++)
            {
                if (currentCell.Column.Key == editableColumns[i])
                {
                    currentPos = i;
                    break;
                }
            }

            // If current column found, move to the next one in sequence
            if (currentPos >= 0)
            {
                // Move to next column in sequence
                int nextPos = (currentPos + 1) % editableColumns.Length;
                string nextColumn = editableColumns[nextPos];

                // Return the next column if it exists
                if (row.Cells.Exists(nextColumn))
                {
                    // For Description cell, we need to check if it's allowed to be edited
                    if (nextColumn == "Description" && row.Cells[nextColumn].Column.CellActivation == Activation.NoEdit)
                    {
                        // Even though Description is typically read-only, we want to focus on it
                        return row.Cells[nextColumn];
                    }
                    // For other cells, check if they're editable
                    else if (row.Cells[nextColumn].Column.CellActivation == Activation.AllowEdit)
                    {
                        return row.Cells[nextColumn];
                    }
                    // If the next cell isn't editable, try to find another editable cell
                    else
                    {
                        // Try subsequent cells
                        for (int i = 1; i < editableColumns.Length; i++)
                        {
                            int pos = (nextPos + i) % editableColumns.Length;
                            if (row.Cells.Exists(editableColumns[pos]))
                            {
                                if (editableColumns[pos] == "Description" ||
                                    row.Cells[editableColumns[pos]].Column.CellActivation == Activation.AllowEdit)
                                {
                                    return row.Cells[editableColumns[pos]];
                                }
                            }
                        }
                    }
                }
            }

            // If we didn't find the current column or next column, default to Description cell
            if (row.Cells.Exists("Description"))
            {
                return row.Cells["Description"];
            }

            // If Description doesn't exist, try to find any editable cell
            foreach (string columnKey in editableColumns)
            {
                if (row.Cells.Exists(columnKey) &&
                    (columnKey == "Description" || row.Cells[columnKey].Column.CellActivation == Activation.AllowEdit))
                {
                    return row.Cells[columnKey];
                }
            }

            // No appropriate cell found
            return null;
        }

        private UltraGridCell FindPreviousEditableCell(UltraGridRow row, UltraGridCell currentCell)
        {
            if (row == null || currentCell == null) return null;

            // List of allowed editable column keys in the desired order
            string[] editableColumns = new string[] { "Description", "Unit", "Packing", "Cost", "Quantity", "Amount", "Reason" };

            // Find the current column in our editableColumns array
            int currentPos = -1;
            for (int i = 0; i < editableColumns.Length; i++)
            {
                if (currentCell.Column.Key == editableColumns[i])
                {
                    currentPos = i;
                    break;
                }
            }

            // If current column found, move to the previous one in sequence
            if (currentPos >= 0)
            {
                // Move to previous column in sequence
                int prevPos = (currentPos - 1 + editableColumns.Length) % editableColumns.Length;
                string prevColumn = editableColumns[prevPos];

                // Return the previous column if it exists
                if (row.Cells.Exists(prevColumn))
                {
                    // For Description cell, we always want to focus on it
                    if (prevColumn == "Description")
                    {
                        return row.Cells[prevColumn];
                    }
                    // For other cells, check if they're editable
                    else if (row.Cells[prevColumn].Column.CellActivation == Activation.AllowEdit)
                    {
                        return row.Cells[prevColumn];
                    }
                    // If the previous cell isn't editable, try to find another editable cell
                    else
                    {
                        // Try previous cells
                        for (int i = 1; i < editableColumns.Length; i++)
                        {
                            int pos = (prevPos - i + editableColumns.Length) % editableColumns.Length;
                            if (row.Cells.Exists(editableColumns[pos]))
                            {
                                if (editableColumns[pos] == "Description" ||
                                    row.Cells[editableColumns[pos]].Column.CellActivation == Activation.AllowEdit)
                                {
                                    return row.Cells[editableColumns[pos]];
                                }
                            }
                        }
                    }
                }
            }

            // If we didn't find the current column or previous column, default to Description cell
            if (row.Cells.Exists("Description"))
            {
                return row.Cells["Description"];
            }

            // If we didn't find the Description column, default to the last editable column in our sequence
            for (int i = editableColumns.Length - 1; i >= 0; i--)
            {
                if (row.Cells.Exists(editableColumns[i]) &&
                    (editableColumns[i] == "Description" || row.Cells[editableColumns[i]].Column.CellActivation == Activation.AllowEdit))
                {
                    return row.Cells[editableColumns[i]];
                }
            }

            // No appropriate editable cell found
            return null;
        }

        private UltraGridCell FindFirstEditableCell(UltraGridRow row)
        {
            if (row == null) return null;

            // Always look for the Description cell first (Item Name) as requested
            if (row.Cells.Exists("Description"))
            {
                return row.Cells["Description"];
            }

            // If Description doesn't exist (unlikely), use fallback order
            string[] editableColumns = new string[] { "Description", "Unit", "Packing", "Cost", "Quantity", "Amount", "Reason" };

            // Return the first editable column in our sequence
            foreach (string columnKey in editableColumns)
            {
                if (row.Cells.Exists(columnKey) &&
                    (columnKey == "Description" || row.Cells[columnKey].Column.CellActivation == Activation.AllowEdit))
                {
                    return row.Cells[columnKey];
                }
            }

            // No appropriate editable cell found
            return null;
        }

        private UltraGridCell FindLastEditableCell(UltraGridRow row)
        {
            if (row == null) return null;

            // List of allowed editable column keys in the desired order
            string[] editableColumns = new string[] { "Description", "Unit", "Packing", "Cost", "Quantity", "Amount", "Reason" };

            // Return the last editable column in our sequence
            for (int i = editableColumns.Length - 1; i >= 0; i--)
            {
                if (row.Cells.Exists(editableColumns[i]) &&
                    (editableColumns[i] == "Description" || row.Cells[editableColumns[i]].Column.CellActivation == Activation.AllowEdit))
                {
                    return row.Cells[editableColumns[i]];
                }
            }

            // No appropriate editable cell found
            return null;
        }

        // Override ProcessCmdKey to handle special keys like F1
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F7)
            {
                // Remove restriction for PurchaseReturnUpdate
                // We're now allowing adding items when loaded via button1

                // Check if items are loaded from btnAddPurchaceList
                if (_isPurchaseDataLoaded && !string.IsNullOrEmpty(textBox1.Text) &&
                    textBox1.Text != "WITHOUT GR" && textBox1.Text != "Without GR")
                {
                    MessageBox.Show("Invalid input. Please clear the form first before selecting items.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return true;
                }

                BtnDial_Click(null, null);
                return true;
            }
            else if (keyData == Keys.F1)
            {
                // Call ClearAllFields when F1 is pressed
                ClearAllFields();
                return true;
            }
            else if (keyData == Keys.F4)
            {
                // Close the form when F4 is pressed
                this.Close();
                return true;
            }
            else if (keyData == (Keys.Tab | Keys.Shift))
            {
                // Check if focus is currently in the UltraGrid
                if (ultraGrid1.Focused || ultraGrid1.ActiveCell != null)
                {
                    // Let the grid handle Shift+Tab navigation internally through its KeyDown event
                    return false;
                }

                // Handle Shift+Tab for form fields
                CycleFocus(false); // Use false to indicate backward direction
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ClearAllFields()
        {
            try
            {
                // Reset buttons visibility state to default
                pbxSave.Visible = true;
                ultraPictureBox4.Visible = false;

                // Clear any special Tag values
                this.Tag = null;

                // Clear vendor information
                vendorid.Text = string.Empty;
                VendorName.Text = string.Empty;

                // Clear purchase information
                textBox1.Clear();
                textBox2.Clear();
                TxtBarcode.Clear();

                // Always reset TxtBarcode to non-readonly state when clearing
                EnsureTxtBarcodeIsWritable();
                // Reset the original state tracker as well
                _originalTxtBarcodeReadOnly = false;

                // Reset date fields
                if (ultraDateTimeEditor1 != null)
                {
                    ultraDateTimeEditor1.Value = DateTime.Now;
                }

                if (ultraDateTimeEditor2 != null)
                {
                    ultraDateTimeEditor2.Value = null;
                }

                // Reset comboboxes
                if (cmbPaymntMethod != null)
                {
                    cmbPaymntMethod.SelectedIndex = -1;
                }

                // Create a fresh DataTable with the required columns
                DataTable dt = new DataTable();
                dt.Columns.Add("Sl No", typeof(int));
                dt.Columns.Add("Description", typeof(string));
                dt.Columns.Add("Barcode", typeof(string));
                dt.Columns.Add("Unit", typeof(string));
                dt.Columns.Add("Packing", typeof(double));
                dt.Columns.Add("Cost", typeof(decimal));
                dt.Columns.Add("Quantity", typeof(double));
                dt.Columns.Add("Amount", typeof(decimal));
                dt.Columns.Add("Reason", typeof(string));
                dt.Columns.Add("SELECT", typeof(bool)); // Add SELECT column at the end

                // Hidden columns
                dt.Columns.Add("ItemID", typeof(long));
                dt.Columns.Add("UnitId", typeof(int));
                dt.Columns.Add("TaxPer", typeof(decimal));
                dt.Columns.Add("TaxAmt", typeof(decimal));
                dt.Columns.Add("TaxType", typeof(string));

                // No longer adding an empty row
                // Apply the empty DataTable to the grid
                ultraGrid1.DataSource = dt;

                // Clear any existing selections in the grid
                if (ultraGrid1.Selected != null && ultraGrid1.Selected.Rows != null)
                {
                    ultraGrid1.Selected.Rows.Clear();
                }
                ultraGrid1.ActiveRow = null;

                // Configure the grid layout
                ConfigureItemsGridLayout();

                // Make sure columns are visible since we have a row
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    string[] visibleColumns = new string[] {
                        "Sl No", "Description", "Barcode", "Unit", "Packing", "Cost",
                        "Quantity", "Amount", "Reason", "SELECT"
                    };

                    foreach (string columnName in visibleColumns)
                    {
                        if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(columnName))
                        {
                            ultraGrid1.DisplayLayout.Bands[0].Columns[columnName].Hidden = false;
                        }
                    }
                }

                // Reset financial amounts
                TxtSubTotal.Text = "0.00";

                lblNetAmount.Text = "0.00";

                // Generate a new purchase return number
                GeneratePurchaseReturnNumber();

                // Reset the purchase data loaded flag
                _isPurchaseDataLoaded = false;

                // Reset the currently loaded purchase number tracking
                _currentlyLoadedPurchaseNo = -1;

                // Set focus back to barcode field
                TxtBarcode.Focus();

                // Set default placeholder text after clearing
                VendorName.Text = "Select Vendor";
                cmbPaymntMethod.Text = "Select Paymode";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing fields: {ex.Message}");
            }
        }

        // Implement the CellListSelect handler for immediate updates when dropdown item is clicked
        private void UltraGrid1_CellListSelect(object sender, CellEventArgs e)
        {
            try
            {
                // Only handle Reason column
                if (e.Cell != null && e.Cell.Column.Key == "Reason")
                {
                    // Check if this event was triggered by a mouse click (not arrow keys)
                    // Only commit the value if it's a mouse click, not when navigating with arrow keys
                    if (Control.MouseButtons != MouseButtons.None)
                    {
                        // Get the current cell and row
                        UltraGridCell currentCell = e.Cell;
                        UltraGridRow currentRow = e.Cell.Row;

                        // Get the selected value
                        string selectedReason = currentCell.Text;
                        System.Diagnostics.Debug.WriteLine($"Reason selected: {selectedReason}");

                        // Exit edit mode to commit the changes
                        ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);

                        // Make sure the cell reflects the new value
                        if (currentCell.Value == null || !currentCell.Value.ToString().Equals(selectedReason))
                        {
                            currentCell.Value = selectedReason;
                        }

                        // Refresh the grid
                        ultraGrid1.Refresh();

                        // After selection, check if there's a next row
                        if (currentRow.Index < ultraGrid1.Rows.Count - 1)
                        {
                            // Move to the Unit cell of the next row
                            UltraGridRow nextRow = ultraGrid1.Rows[currentRow.Index + 1];
                            if (nextRow.Cells.Exists("Unit"))
                            {
                                ultraGrid1.ActiveRow = nextRow;
                                ultraGrid1.ActiveCell = nextRow.Cells["Unit"];
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            }
                        }
                        else
                        {
                            // If this is the last row, just move focus to the Unit cell of this row
                            if (currentRow.Cells.Exists("Unit"))
                            {
                                ultraGrid1.ActiveCell = currentRow.Cells["Unit"];
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"Reason dropdown selection committed: {selectedReason} on row {currentRow.Index}");
                    }
                    else
                    {
                        // If triggered by keyboard navigation (arrow keys), don't commit the value
                        System.Diagnostics.Debug.WriteLine("Arrow key navigation - not committing value");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in UltraGrid1_CellListSelect: " + ex.Message);
            }
        }

        // Implement the MouseDown handler to show dropdown when clicking on Reason cell
        private void UltraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the UI element at the mouse click point
                UIElement element = ultraGrid1.DisplayLayout.UIElement.ElementFromPoint(new Point(e.X, e.Y));
                if (element != null)
                {
                    // Check if the click is on a row selector
                    RowSelectorUIElement rowSelectorElement = element as RowSelectorUIElement;
                    if (rowSelectorElement != null && rowSelectorElement.Row != null)
                    {
                        UltraGridRow clickedRow = rowSelectorElement.Row;

                        // Clear highlighting on all rows first
                        foreach (UltraGridRow row in ultraGrid1.Rows)
                        {
                            row.Appearance.BackColor = Color.Empty;
                            row.Appearance.ForeColor = Color.Empty;
                            row.Appearance.FontData.Bold = DefaultableBoolean.Default;
                        }

                        // Apply highlighting only to the clicked row
                        clickedRow.Appearance.BackColor = Color.FromArgb(100, 255, 150, 150); // Light transparent red
                        clickedRow.Appearance.ForeColor = Color.Yellow;
                        clickedRow.Appearance.FontData.Bold = DefaultableBoolean.True;

                        // Make this row the active row and selected row
                        ultraGrid1.ActiveRow = clickedRow;
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(clickedRow);

                        return; // Don't process further since we handled the row selector click
                    }

                    // Handle Reason column cell clicks (existing functionality)
                    UltraGridCell cell = element.GetContext(typeof(UltraGridCell)) as UltraGridCell;
                    if (cell != null && cell.Column.Key == "Reason")
                    {
                        // Make this the active cell
                        ultraGrid1.ActiveCell = cell;

                        // Enter edit mode directly
                        if (!ultraGrid1.ActiveCell.IsInEditMode)
                        {
                            ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);

                            // Show dropdown immediately
                            SendKeys.SendWait("{F4}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in UltraGrid1_MouseDown: " + ex.Message);
            }
        }

        // Implement the AfterCellActivate handler
        private void UltraGrid1_AfterCellActivate(object sender, EventArgs e)
        {
            // This method intentionally left empty
            // No special handling for cell activation or arrow keys
            // All grid navigation will use default behavior
        }

        // Helper method to set focus to the Description column of the last row in the grid
        private void SetFocusToLastRow()
        {
            try
            {
                // Make sure the grid has focus and there is an active row
                if (ultraGrid1.Rows.Count > 0)
                {
                    // Get the last row in the grid instead of the active row
                    UltraGridRow activeRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];

                    // Set this row as the active row and clear any existing selection
                    ultraGrid1.ActiveRow = activeRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(activeRow);

                    // Set focus to the Description cell if it exists
                    if (activeRow.Cells.Exists("Description"))
                    {
                        ultraGrid1.ActiveCell = activeRow.Cells["Description"];

                        // Set focus to the grid and enter edit mode
                        ultraGrid1.Focus();
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting focus to Description: " + ex.Message);
            }
        }

        // Override OnShown to set focus after the form is completely shown
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Set focus to the Vendorbutton
            this.ActiveControl = Vendorbutton;

            // Force focus if needed using BeginInvoke to ensure UI is fully loaded
            this.BeginInvoke(new Action(() => {
                Vendorbutton.Focus();
            }));
        }

        private void frmPurchaseReturn_KeyDown(object sender, KeyEventArgs e)
        {
            // Only handle Tab key presses
            if (e.KeyCode == Keys.Tab)
            {
                // Check if focus is currently in the UltraGrid
                if (ultraGrid1.Focused || ultraGrid1.ActiveCell != null)
                {
                    // Let the grid handle tab navigation internally
                    // Don't modify this behavior as it allows tab key to work within the grid
                    return;
                }

                e.Handled = true; // Mark as handled to prevent default tab behavior
                e.SuppressKeyPress = true; // Suppress the key press to prevent beep sound

                // If we're in a form field and Tab is pressed, try to focus the grid
                if (ultraGrid1.Rows.Count > 0)
                {
                    // Set focus to the first row's Description cell as requested
                    SetFocusToFirstRow();
                }
                else
                {
                    // If grid has no rows, use the standard focus cycling
                    CycleFocus(!e.Shift); // Forward if regular Tab, backward if Shift+Tab
                }
            }
        }

        // Method to set focus to our target controls in sequence
        private void CycleFocus(bool forward = true)
        {
            Control currentControl = this.ActiveControl;

            if (forward)
            {
                // Forward cycling
                if (currentControl == Vendorbutton)
                    cmbPaymntMethod.Focus();
                else if (currentControl == cmbPaymntMethod)
                    textBox1.Focus();
                else if (currentControl == textBox1)
                    btnAddPurchaceList.Focus();
                else if (currentControl == btnAddPurchaceList)
                    TxtBarcode.Focus();
                else
                    Vendorbutton.Focus();
            }
            else
            {
                // Backward cycling
                if (currentControl == TxtBarcode)
                    btnAddPurchaceList.Focus();
                else if (currentControl == btnAddPurchaceList)
                    textBox1.Focus();
                else if (currentControl == textBox1)
                    cmbPaymntMethod.Focus();
                else if (currentControl == cmbPaymntMethod)
                    Vendorbutton.Focus();
                else
                    TxtBarcode.Focus();
            }
        }

        // Helper method to force update and redraw of the Amount cell
        private void ForceUpdateAmountCell(UltraGridRow row)
        {
            if (row == null || !row.Cells.Exists("Cost") || !row.Cells.Exists("Quantity") || !row.Cells.Exists("Amount"))
                return;

            try
            {
                // Mark that we're updating to prevent recursion
                _isUpdatingCells = true;

                // Get values
                decimal cost = 0, qty = 0;
                if (row.Cells["Cost"].Value != null)
                    decimal.TryParse(row.Cells["Cost"].Value.ToString(), out cost);
                if (row.Cells["Quantity"].Value != null)
                    decimal.TryParse(row.Cells["Quantity"].Value.ToString(), out qty);

                // Calculate amount
                decimal amount = Math.Round(cost * qty, 2);

                // Store current amount value to check if it's changed
                decimal oldAmount = 0;
                if (row.Cells["Amount"].Value != null)
                    decimal.TryParse(row.Cells["Amount"].Value.ToString(), out oldAmount);

                // Check if the amount has actually changed
                bool amountChanged = (amount != oldAmount);

                // Store current selection state
                UltraGridCell activeCell = ultraGrid1.ActiveCell;

                // Set the Amount value
                row.Cells["Amount"].Value = amount;

                // Only highlight if amount has changed due to Cost or Quantity changes
                if (amountChanged)
                {
                    // Create animation timer and state
                    System.Threading.Timer animationTimer = null;
                    int animationStep = 0;
                    bool isYellow = true;

                    // Create a new timer for the flashing animation effect
                    animationTimer = new System.Threading.Timer((state) => {
                        try
                        {
                            this.Invoke(new Action(() => {
                                animationStep++;

                                // Switch between yellow-red and red-yellow for the flashing effect
                                if (isYellow)
                                {
                                    row.Cells["Amount"].Appearance.BackColor = Color.Yellow;
                                    row.Cells["Amount"].Appearance.ForeColor = Color.Red;
                                    isYellow = false;
                                }
                                else
                                {
                                    row.Cells["Amount"].Appearance.BackColor = Color.Red;
                                    row.Cells["Amount"].Appearance.ForeColor = Color.Yellow;
                                    isYellow = true;
                                }

                                row.Cells["Amount"].Appearance.FontData.Bold = DefaultableBoolean.True;
                                ultraGrid1.Refresh();

                                // Stop the animation after 5 cycles
                                if (animationStep >= 5)
                                {
                                    // Reset the cell appearance
                                    row.Cells["Amount"].Appearance.ResetForeColor();
                                    row.Cells["Amount"].Appearance.ResetBackColor();
                                    row.Cells["Amount"].Appearance.FontData.Bold = DefaultableBoolean.Default;
                                    ultraGrid1.Refresh();

                                    // Dispose the timer
                                    animationTimer?.Dispose();
                                }
                            }));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in animation timer: {ex.Message}");
                            animationTimer?.Dispose();
                        }
                    }, null, 0, 150); // Flash every 150ms

                    // Log the calculation with highlighting
                    System.Diagnostics.Debug.WriteLine($"ForceUpdateAmountCell: Amount changed and highlighted: {amount} (Cost: {cost} × Quantity: {qty})");
                }
                else
                {
                    // Log the calculation without highlighting
                    System.Diagnostics.Debug.WriteLine($"ForceUpdateAmountCell: Amount unchanged: {amount} (Cost: {cost} × Quantity: {qty})");
                }

                // Update net total
                UpdateNetAmount();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ForceUpdateAmountCell: {ex.Message}");
            }
            finally
            {
                _isUpdatingCells = false;
            }
        }

        // Helper method to determine the next column to navigate to
        private string GetNextColumnForNavigation(string currentColumn)
        {
            // Map of column navigation - where to go next when Enter is pressed in each column
            switch (currentColumn)
            {
                case "Description": return "Unit";
                case "Unit": return "Packing";
                case "Packing": return "Cost";
                case "Cost": return "Quantity";
                case "Quantity": return "Reason"; // Skip Amount since it's calculated
                case "Reason": return "Unit"; // Cycle back to Unit of same or next row
                case "Amount": return "Unit"; // For completeness, though Amount is not usually editable
                default: return "Unit"; // Default to Unit if unknown column
            }
        }

        // Helper method to calculate and update a row's Amount and the Net Amount
        private void CalculateAndUpdateRow(UltraGridRow row, bool highlightIfChanged = false)
        {
            if (row == null || !row.Cells.Exists("Cost") || !row.Cells.Exists("Quantity") || !row.Cells.Exists("Amount"))
                return;

            try
            {
                // Get the current values
                decimal cost = 0;
                double quantity = 0;
                decimal currentAmount = 0;

                // Parse the current values with error handling
                if (row.Cells["Cost"].Value != null)
                    decimal.TryParse(row.Cells["Cost"].Value.ToString(), out cost);
                if (row.Cells["Quantity"].Value != null)
                    double.TryParse(row.Cells["Quantity"].Value.ToString(), out quantity);
                if (row.Cells["Amount"].Value != null)
                    decimal.TryParse(row.Cells["Amount"].Value.ToString(), out currentAmount);

                // Calculate the new amount
                decimal newAmount = Math.Round(cost * (decimal)quantity, 2);

                // Check if the amount has changed
                bool amountChanged = (newAmount != currentAmount);

                // Always update the Amount value
                row.Cells["Amount"].Value = newAmount;

                // Highlight the Amount cell if requested and the amount has changed
                if (highlightIfChanged && amountChanged)
                {
                    // Store the original cell appearance
                    System.Drawing.Color originalBackColor = row.Cells["Amount"].Appearance.BackColor;
                    System.Drawing.Color originalForeColor = row.Cells["Amount"].Appearance.ForeColor;
                    DefaultableBoolean originalBold = row.Cells["Amount"].Appearance.FontData.Bold;

                    // Set a temporary highlight appearance
                    row.Cells["Amount"].Appearance.BackColor = Color.Yellow;
                    row.Cells["Amount"].Appearance.ForeColor = Color.Red;
                    row.Cells["Amount"].Appearance.FontData.Bold = DefaultableBoolean.True;

                    // Refresh the display
                    ultraGrid1.Refresh();

                    // Create a timer to restore the original appearance after a short delay
                    System.Threading.Timer restoreTimer = null;
                    restoreTimer = new System.Threading.Timer((state) =>
                    {
                        try
                        {
                            this.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    // Restore the original appearance
                                    row.Cells["Amount"].Appearance.BackColor = originalBackColor;
                                    row.Cells["Amount"].Appearance.ForeColor = originalForeColor;
                                    row.Cells["Amount"].Appearance.FontData.Bold = originalBold;

                                    // Refresh the display
                                    ultraGrid1.Refresh();
                                }
                                catch (Exception)
                                {
                                    // Silently handle error
                                }
                            }));
                        }
                        catch (Exception)
                        {
                            // Silently handle error
                        }
                        finally
                        {
                            // Dispose the timer
                            restoreTimer?.Dispose();
                        }
                    }, null, 1500, System.Threading.Timeout.Infinite); // Restore after 1.5 seconds
                }

                // Update the net total
                UpdateNetAmount();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating row: {ex.Message}");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Check if items are already loaded via btnAddPurchaceList (vendor purchase items)
                // Only restrict if the Tag is NOT "PurchaseReturnUpdate"
                if (_isPurchaseDataLoaded && (this.Tag == null || this.Tag.ToString() != "PurchaseReturnUpdate"))
                {
                    MessageBox.Show("Invalid operation. Purchase data is already loaded.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if items are loaded by BtnDial (indicated by "WITHOUT GR")
                if (textBox1.Text == "WITHOUT GR")
                {
                    MessageBox.Show("Invalid operation. Items already loaded via direct selection.",
                        "Operation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Removed restriction that prevented loading multiple times
                // Now allowing repeated loading via button1 even when previously loaded via button1

                if (string.IsNullOrEmpty(TxtSRNO.Text))
                {
                    MessageBox.Show("Please enter a PR number.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Open the Purchase Return Update form
                PurcahseReturnUpdate updateForm = new PurcahseReturnUpdate();
                if (updateForm.ShowDialog() == DialogResult.OK)
                {
                    // Save the original readonly state of TxtBarcode
                    _originalTxtBarcodeReadOnly = TxtBarcode.ReadOnly;

                    // Make TxtBarcode read-only since items are loaded via button1 (purchasereturnupdate)
                    TxtBarcode.ReadOnly = true;

                    int selectedPRNo = updateForm.SelectedPRNo;

                    // Set the PR number in TxtSRNO
                    if (TxtSRNO != null)
                    {
                        TxtSRNO.Text = selectedPRNo.ToString();
                    }

                    // Clear the existing grid data before loading new items
                    if (ultraGrid1.DataSource is DataTable existingData)
                    {
                        existingData.Clear();
                    }

                    // Get PR details for the grid
                    DataTable prDetails = GetPRDetailsByPRNo(selectedPRNo);

                    if (prDetails != null && prDetails.Rows.Count > 0)
                    {
                        // Set the data source for the grid
                        ultraGrid1.DataSource = prDetails;

                        // Apply grid formatting to ensure only the specified columns are visible
                        ConfigureItemsGridLayout();

                        // Make sure reason values are preserved
                        PreserveReasonValues();

                        // Calculate totals
                        UpdateNetAmount();

                        // Make ultraPictureBox4 visible and hide pbxSave when items are loaded
                        ultraPictureBox4.Visible = true;
                        pbxSave.Visible = false;

                        // Set the flag to indicate purchase data is loaded via button1
                        _isPurchaseDataLoaded = true;

                        // Set a special flag in the Tag property to indicate loaded via button1
                        this.Tag = "PurchaseReturnUpdate";
                    }
                    else
                    {
                        MessageBox.Show("No items found for this purchase return.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading PR details: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to get purchase return master data
        private ModelClass.TransactionModels.PReturnMaster GetPurchaseReturnMasterData(int prNo)
        {
            try
            {
                // Use the repository to get PR master data - with more debug information
                using (SqlConnection conn = (SqlConnection)new BaseRepostitory().DataConnection)
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine($"SQL connection opened for PR: {prNo}");

                    // Let's first query the database directly to see all available columns
                    using (SqlCommand debugCmd = new SqlCommand("SELECT * FROM PReturnMaster WHERE PReturnNo = @PReturnNo", conn))
                    {
                        debugCmd.Parameters.AddWithValue("@PReturnNo", prNo);
                        using (SqlDataReader reader = debugCmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                System.Diagnostics.Debug.WriteLine("Direct query found a matching PReturnMaster record");
                                System.Diagnostics.Debug.WriteLine("Available columns in PReturnMaster:");
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    System.Diagnostics.Debug.WriteLine($"  {i}: {reader.GetName(i)}");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Direct query found NO matching record for PR: {prNo}");
                            }
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("_POS_PurchaseReturn", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@_Operation", "GETAllPurchaseReturn");
                        cmd.Parameters.AddWithValue("@PReturnNo", prNo);
                        cmd.Parameters.AddWithValue("@CompanyId", ModelClass.SessionContext.CompanyId);
                        cmd.Parameters.AddWithValue("@BranchId", ModelClass.SessionContext.BranchId);
                        cmd.Parameters.AddWithValue("@FinYearId", ModelClass.SessionContext.FinYearId);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // Debug the actual data we received
                            System.Diagnostics.Debug.WriteLine($"Stored procedure returned {dt.Rows.Count} rows");
                            System.Diagnostics.Debug.WriteLine("Columns in result set:");
                            foreach (DataColumn col in dt.Columns)
                            {
                                System.Diagnostics.Debug.WriteLine($"  {col.ColumnName} ({col.DataType})");
                            }

                            // Filter to get only the record matching the PR number
                            DataRow[] rows = dt.Select($"PReturnNo = {prNo}");
                            System.Diagnostics.Debug.WriteLine($"After filtering, found {rows.Length} matching rows for PR: {prNo}");

                            if (rows.Length > 0)
                            {
                                DataRow row = rows[0];
                                ModelClass.TransactionModels.PReturnMaster prMaster = new ModelClass.TransactionModels.PReturnMaster();

                                // Debug the actual row data
                                System.Diagnostics.Debug.WriteLine("Row data for matching PR:");
                                foreach (DataColumn col in dt.Columns)
                                {
                                    string value = row[col] == DBNull.Value ? "NULL" : row[col].ToString();
                                    System.Diagnostics.Debug.WriteLine($"  {col.ColumnName}: {value}");
                                }

                                // Map the basic fields we need
                                prMaster.PReturnNo = prNo;

                                if (dt.Columns.Contains("InvoiceNo") && row["InvoiceNo"] != DBNull.Value)
                                {
                                    prMaster.InvoiceNo = row["InvoiceNo"].ToString();
                                    System.Diagnostics.Debug.WriteLine("Invoice No: " + prMaster.InvoiceNo);
                                }

                                if (dt.Columns.Contains("InvoiceDate") && row["InvoiceDate"] != DBNull.Value)
                                {
                                    prMaster.InvoiceDate = Convert.ToDateTime(row["InvoiceDate"]);
                                    System.Diagnostics.Debug.WriteLine("Invoice Date: " + prMaster.InvoiceDate.ToString("yyyy-MM-dd"));
                                }

                                if (dt.Columns.Contains("Paymode") && row["Paymode"] != DBNull.Value)
                                {
                                    prMaster.Paymode = row["Paymode"].ToString();
                                    System.Diagnostics.Debug.WriteLine("Pay Mode: " + prMaster.Paymode);
                                }

                                if (dt.Columns.Contains("PaymodeID") && row["PaymodeID"] != DBNull.Value)
                                {
                                    prMaster.PaymodeID = Convert.ToInt32(row["PaymodeID"]);
                                    System.Diagnostics.Debug.WriteLine("Pay Mode ID: " + prMaster.PaymodeID);
                                }

                                if (dt.Columns.Contains("VendorName") && row["VendorName"] != DBNull.Value)
                                {
                                    prMaster.VendorName = row["VendorName"].ToString();
                                    System.Diagnostics.Debug.WriteLine("Vendor Name: " + prMaster.VendorName);
                                }

                                // Try a direct SQL query now to get the proper PayMode information
                                using (SqlCommand payModeCmd = new SqlCommand(
                                    "SELECT pm.PaymodeID, pm.Paymode FROM PReturnMaster prm " +
                                    "JOIN Paymode pm ON prm.PaymodeID = pm.PaymodeID " +
                                    "WHERE prm.PReturnNo = @PReturnNo", conn))
                                {
                                    payModeCmd.Parameters.AddWithValue("@PReturnNo", prNo);

                                    using (SqlDataReader reader = payModeCmd.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            if (!reader.IsDBNull(reader.GetOrdinal("PaymodeID")))
                                            {
                                                prMaster.PaymodeID = reader.GetInt32(reader.GetOrdinal("PaymodeID"));
                                            }

                                            if (!reader.IsDBNull(reader.GetOrdinal("Paymode")))
                                            {
                                                prMaster.Paymode = reader.GetString(reader.GetOrdinal("Paymode"));
                                                System.Diagnostics.Debug.WriteLine($"DIRECT QUERY - Pay Mode: {prMaster.Paymode}, ID: {prMaster.PaymodeID}");
                                            }
                                        }
                                    }
                                }

                                return prMaster;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("No purchase return found with PR No: " + prNo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting PR master data: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
            }

            return null;
        }

        private DataTable GetPRDetailsByPRNo(int prNo)
        {
            DataTable dt = new DataTable();
            try
            {
                // Create a BaseRepository instance to get the connection
                BaseRepostitory baseRepo = new BaseRepostitory();

                // Use direct SQL to fetch the exact data needed
                using (SqlConnection conn = (SqlConnection)baseRepo.DataConnection)
                {
                    conn.Open();

                    // Direct SQL query to get the correct Qty and Reason fields
                    string sql = @"SELECT 
                                  prd.SlNo,
                                  prd.ItemID, 
                                  im.Description AS ItemName,
                                  ps.BarCode,
                                  prd.Unit,
                                  prd.Packing,
                                  prd.Cost,
                                  (prd.Cost * prd.Qty) AS Amount,
                                  prd.Qty,
                                  prd.Reason,
                                  prd.UnitId
                               FROM 
                                  PReturnDetails prd
                               LEFT JOIN 
                                  ItemMaster im ON prd.ItemID = im.ItemId
                               LEFT JOIN 
                                  PriceSettings ps ON prd.ItemID = ps.ItemId AND prd.UnitId = ps.UnitId
                               WHERE 
                                  prd.PReturnNo = @PReturnNo";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@PReturnNo", prNo);

                        // Check if this is from a vendor search by looking at how the form was opened
                        bool isFromVendorSearch = false;

                        // If a form with this PR number was opened through a vendor selection dialog
                        if (this.Tag != null && this.Tag.ToString() == "VendorSearch")
                        {
                            isFromVendorSearch = true;
                        }

                        // Create a formatted DataTable with the exact column names needed for the grid
                        DataTable formattedTable = new DataTable();
                        formattedTable.Columns.Add("Sl No", typeof(int));
                        formattedTable.Columns.Add("Description", typeof(string));
                        formattedTable.Columns.Add("Barcode", typeof(string));
                        formattedTable.Columns.Add("Unit", typeof(string));
                        formattedTable.Columns.Add("Packing", typeof(double));
                        formattedTable.Columns.Add("Cost", typeof(decimal));
                        formattedTable.Columns.Add("Quantity", typeof(double));
                        formattedTable.Columns.Add("Amount", typeof(decimal));
                        formattedTable.Columns.Add("Reason", typeof(string));

                        // Only add ItemID and UnitId if not from vendor search
                        if (!isFromVendorSearch)
                        {
                            // Hidden columns for reference
                            formattedTable.Columns.Add("ItemID", typeof(long));
                            formattedTable.Columns.Add("UnitId", typeof(int));
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable rawData = new DataTable();
                            adapter.Fill(rawData);

                            System.Diagnostics.Debug.WriteLine($"Loaded {rawData.Rows.Count} rows from PReturnDetails for PR# {prNo}");

                            // Log the raw data to help debug
                            if (rawData.Rows.Count > 0)
                            {
                                System.Diagnostics.Debug.WriteLine("Columns in raw data:");
                                foreach (DataColumn col in rawData.Columns)
                                {
                                    System.Diagnostics.Debug.WriteLine($"  - {col.ColumnName} ({col.DataType})");
                                }

                                // Check if Reason column exists
                                if (rawData.Columns.Contains("Reason"))
                                {
                                    System.Diagnostics.Debug.WriteLine("Reason values in raw data:");
                                    foreach (DataRow r in rawData.Rows)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"  - {r["Reason"]}");
                                    }
                                }
                            }

                            // Process each row from the raw data
                            for (int i = 0; i < rawData.Rows.Count; i++)
                            {
                                DataRow rawRow = rawData.Rows[i];
                                DataRow newRow = formattedTable.NewRow();

                                try
                                {
                                    // Map fields using safer conversion methods
                                    newRow["Sl No"] = rawRow["SlNo"] != DBNull.Value ? Convert.ToInt32(rawRow["SlNo"]) : i + 1;

                                    // String fields
                                    newRow["Description"] = rawRow["ItemName"] != DBNull.Value ? Convert.ToString(rawRow["ItemName"]) : "";
                                    newRow["Barcode"] = rawRow["BarCode"] != DBNull.Value ? Convert.ToString(rawRow["BarCode"]) : "";
                                    newRow["Unit"] = rawRow["Unit"] != DBNull.Value ? Convert.ToString(rawRow["Unit"]) : "PCS";

                                    // Numeric fields with safe conversion
                                    double packing = 1;
                                    if (rawRow["Packing"] != DBNull.Value)
                                    {
                                        double.TryParse(rawRow["Packing"].ToString(), out packing);
                                    }
                                    newRow["Packing"] = packing;

                                    decimal cost = 0;
                                    if (rawRow["Cost"] != DBNull.Value)
                                    {
                                        decimal.TryParse(rawRow["Cost"].ToString(), out cost);
                                    }
                                    newRow["Cost"] = cost;

                                    // Quantity with safe conversion
                                    double qty = 1;
                                    if (rawRow["Qty"] != DBNull.Value)
                                    {
                                        double.TryParse(rawRow["Qty"].ToString(), out qty);
                                    }
                                    newRow["Quantity"] = qty;

                                    // Amount calculation or direct value
                                    decimal amount = 0;
                                    if (rawRow["Amount"] != DBNull.Value)
                                    {
                                        decimal.TryParse(rawRow["Amount"].ToString(), out amount);
                                    }
                                    else
                                    {
                                        amount = Math.Round(cost * Convert.ToDecimal(qty), 2);
                                    }
                                    newRow["Amount"] = amount;

                                    // Reason field - IMPORTANT: Preserve the actual reason from the database
                                    string reasonValue = "Select Reason";
                                    if (rawRow["Reason"] != DBNull.Value && !string.IsNullOrWhiteSpace(rawRow["Reason"].ToString()))
                                    {
                                        reasonValue = rawRow["Reason"].ToString().Trim();
                                    }
                                    newRow["Reason"] = reasonValue;

                                    System.Diagnostics.Debug.WriteLine($"Setting Reason for row {i} to: {reasonValue}");

                                    // Only add ItemID and UnitId if not from vendor search
                                    if (!isFromVendorSearch)
                                    {
                                        // ItemID field with safe conversion
                                        long itemId = 0;
                                        if (rawRow["ItemID"] != DBNull.Value)
                                        {
                                            long.TryParse(rawRow["ItemID"].ToString(), out itemId);
                                        }
                                        newRow["ItemID"] = itemId;

                                        // UnitId field with safe conversion
                                        int unitId = 0;
                                        if (rawRow["UnitId"] != DBNull.Value)
                                        {
                                            int.TryParse(rawRow["UnitId"].ToString(), out unitId);
                                        }
                                        newRow["UnitId"] = unitId;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // If there's an error with a specific field conversion, log it but continue
                                    System.Diagnostics.Debug.WriteLine($"Error converting row {i}: {ex.Message}");
                                }

                                formattedTable.Rows.Add(newRow);
                            }

                            return formattedTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving purchase return details: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        private void ultraPictureBox4_Click(object sender, EventArgs e)
        {
            // Make sure this button stays visible when clicked
            ultraPictureBox4.Visible = true;
            pbxSave.Visible = false;

            SavePurchaseReturn();
        }

        // Handle row selection highlighting
        private void UltraGrid1_AfterSelectChange(object sender, AfterSelectChangeEventArgs e)
        {
            try
            {
                // Don't process here as we're handling row selector clicks in MouseDown
                // Only apply highlighting for programmatic selection (like keyboard navigation)
                if (Control.MouseButtons != MouseButtons.None)
                {
                    // If mouse buttons are pressed, selection is likely from MouseDown,
                    // which already handles the highlighting
                    return;
                }

                // Check if the grid was loaded from btnAddPurchaceList
                bool loadedFromPurchaseList = _currentlyLoadedPurchaseNo > 0;

                // Reset all row appearances first
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    row.Appearance.BackColor = Color.Empty;
                    row.Appearance.ForeColor = Color.Empty;
                    row.Appearance.FontData.Bold = DefaultableBoolean.Default;
                }

                // Apply highlighting to selected rows only if loaded from btnAddPurchaceList
                if (loadedFromPurchaseList)
                {
                    foreach (UltraGridRow row in ultraGrid1.Selected.Rows)
                    {
                        // Apply light red background with yellow text
                        row.Appearance.BackColor = Color.FromArgb(100, 255, 150, 150); // Light transparent red
                        row.Appearance.ForeColor = Color.Yellow;
                        row.Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in row selection highlighting: " + ex.Message);
            }
        }

        // This event handler implements "Select All" checkbox functionality for the SELECT column
        private void UltraGrid1_MouseUp_ForHeaderCheckbox(object sender, MouseEventArgs e)
        {
            try
            {
                UltraGrid grid = sender as UltraGrid;
                if (grid == null || grid.DisplayLayout.Bands.Count == 0)
                    return;

                // Only handle left-click
                if (e.Button != MouseButtons.Left)
                    return;

                // Check if y-coordinate is in the header region (typically first ~25 pixels)
                if (e.Y > 25)
                    return;

                // Approximate position for the SELECT column (at the end after Reason)
                // Note: This is an approximation and might need adjustment based on actual column widths
                int totalWidth = 0;
                UltraGridBand band = grid.DisplayLayout.Bands[0];
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (!col.Hidden && col.Key != "SELECT")
                    {
                        totalWidth += col.Width;
                    }
                }

                // Check if the click is in the approximate x-position of the SELECT column
                // which should be somewhere after the Reason column
                int selectColumnStart = totalWidth - 60; // Approximate starting position
                int selectColumnEnd = totalWidth + 60;   // Approximate ending position

                if (e.X < selectColumnStart || e.X > selectColumnEnd)
                    return;

                // Toggle all checkboxes based on current state
                // If any are checked, uncheck all, otherwise check all
                bool isAnyChecked = false;
                foreach (UltraGridRow row in grid.Rows)
                {
                    if (row.Cells.Exists("SELECT") && row.Cells["SELECT"].Value != null && (bool)row.Cells["SELECT"].Value)
                    {
                        isAnyChecked = true;
                        break;
                    }
                }

                // Set the new state (inverse of current state)
                bool newState = !isAnyChecked;
                foreach (UltraGridRow row in grid.Rows)
                {
                    if (row.Cells.Exists("SELECT"))
                    {
                        row.Cells["SELECT"].Value = newState;
                    }
                }

                // Force refresh
                grid.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in SELECT column header click handler: " + ex.Message);
            }
        }

        private void panelSubtotalLine_PaintClient(object sender, PaintEventArgs e)
        {
            using (Pen dashedPen = new Pen(Color.Black, 1))
            {
                dashedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                // Draw a dashed line across the width of the panel
                e.Graphics.DrawLine(dashedPen, 0, ((Control)sender).Height / 2, ((Control)sender).Width, ((Control)sender).Height / 2);
            }
        }

        private void SubTotal_Click(object sender, EventArgs e)
        {
            // Handler for SubTotal label click event
        }

        // Add this helper method at the end of the class before the last closing brace
        private void EnsureTxtBarcodeIsWritable()
        {
            // Store the original ReadOnly state before modifying
            if (TxtBarcode.ReadOnly)
            {
                _originalTxtBarcodeReadOnly = TxtBarcode.ReadOnly;
            }

            // Always make TxtBarcode writable
            TxtBarcode.ReadOnly = false;
        }
    }
}


