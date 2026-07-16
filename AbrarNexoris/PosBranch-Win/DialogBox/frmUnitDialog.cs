using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinTabControl;
using ModelClass;
using ModelClass.Master;
using PosBranch_Win.Master;
using Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmUnitDialog : Form
    {
        Dropdowns drop = new Dropdowns();
        frmItemMasterNew ItemMaster = new frmItemMasterNew();
        string FormName;
        bool checke = false;
        private int currentRecord = 0;
        private int totalRecords = 0;
        private int _itemId = 0; // Store the item ID

        // Add these fields to track drag operations at the class level
        private Point startPoint;
        private Infragistics.Win.UltraWinGrid.UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();

        // Add debounce timer and tracking for textBox3
        private System.Windows.Forms.Timer textBox3DebounceTimer;
        private string lastProcessedTextBox3Value = string.Empty;

        // Helper method to find a control in a form by name
        private Control FindControlInForm(Control container, string name)
        {
            // Check if the container itself is the control we're looking for
            if (container.Name == name)
                return container;

            // Search through all child controls
            foreach (Control ctrl in container.Controls)
            {
                // Recursively check this control and its children
                Control foundControl = FindControlInForm(ctrl, name);
                if (foundControl != null)
                    return foundControl;
            }

            // Not found in this container
            return null;
        }

        // Helper method to safely set price fields for a DataRow with error handling
        private void SafeSetPriceFieldForDataRow(DataRow targetRow, DataRow sourceRow, string columnName, float defaultValue, string packingValue)
        {
            try
            {
                if (sourceRow != null && sourceRow[columnName] != DBNull.Value)
                {
                    float sourceValue = Convert.ToSingle(sourceRow[columnName]);
                    float packingMultiplier = float.Parse(packingValue);
                    targetRow[columnName] = sourceValue * packingMultiplier;
                }
                else
                {
                    targetRow[columnName] = defaultValue;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting {columnName}: {ex.Message}");
                try
                {
                    targetRow[columnName] = defaultValue;
                }
                catch { }
            }
        }

        public frmUnitDialog(string Params, int itemId = 0)
        {
            InitializeComponent();
            FormName = Params;
            _itemId = itemId; // Store the item ID

            // Set form title to "Item UOM"
            this.Text = "Item UOM";
        }

        private void frmUnitDialog_Load(object sender, EventArgs e)
        {
            try
            {
                // Set the window size to exactly match the screenshot


                // Configure grid appearance for better display
                ConfigureGridAppearance();

                // Load unit data
                LoadUnitData();

                // Configure column widths and styling
                FormatGridColumns();

                // Update record count in status strip
                UpdateRecordCount();

                // Configure search and sort by comboboxes
                ConfigureComboBoxes();

                // Set up navigation buttons with arrow icons
                SetupNavigationButtons();

                // Set up key handlers for navigation
                SetupKeyHandlers();

                // Style panels to match frmdialForItemMaster
                StyleIconPanel(ultraPanel5);
                StyleIconPanel(ultraPanel6);
                StyleIconPanel(ultraPanel3);
                StyleIconPanel(ultraPanel7);
                StyleIconPanel(ultraPanel4);

                // Setup hover effects and click handlers for panels
                SetupPanelHoverEffects();

                // Set up navigation panel click handlers
                SetupNavigationPanels();

                // Set the form title label - check if it exists first to prevent NullReferenceException
                try
                {
                    // Check if lblTitle exists and is accessible
                    if (this.Controls.Find("lblTitle", true).Length > 0)
                    {
                        Label lblTitle = (Label)this.Controls.Find("lblTitle", true)[0];
                        lblTitle.Text = "Item UOM List";
                    }
                    else
                    {
                        // Create the label if it doesn't exist
                        Label lblTitle = new Label();
                        lblTitle.Name = "lblTitle";
                        lblTitle.Text = "Item UOM List";
                        lblTitle.Font = new Font("Microsoft Sans Serif", 14, FontStyle.Bold);
                        lblTitle.ForeColor = Color.White;
                        lblTitle.AutoSize = true;
                        lblTitle.Location = new Point(12, 10);

                        // Add to the top panel if it exists
                        if (this.Controls.Find("panel1", true).Length > 0)
                        {
                            Panel panel1 = (Panel)this.Controls.Find("panel1", true)[0];
                            panel1.Controls.Add(lblTitle);
                        }
                        else
                        {
                            this.Controls.Add(lblTitle);
                        }
                    }
                }
                catch (Exception titleEx)
                {
                    // Just log the error but continue - this is not critical
                    System.Diagnostics.Debug.WriteLine("Error setting title: " + titleEx.Message);
                }

                // Add double-click handler for row selection
                ultraGrid1.DoubleClickRow += UltraGrid1_DoubleClickRow;

                // Add resize event to adjust columns when form is resized
                this.Resize += frmUnitDialog_Resize;

                // Wire search box (textBox1) for filtering and selection
                WireSearchBoxHandlers();

                // Initialize count box with totals, then wire its handlers
                InitializeCountBoxAndLabel();
                WireCountBoxHandlers();

                // Initialize debounce timer for textBox3
                InitializeTextBox3DebounceTimer();

                // Position the form in the center of the screen for best appearance
                this.StartPosition = FormStartPosition.CenterScreen;

                // Use Shown event to set focus on textBox1 (more reliable than Load event)
                this.Shown += (s, evt) =>
                {
                    // Set focus to the search textbox when form is fully displayed
                    if (textBox1 != null)
                    {
                        textBox1.Focus();
                        textBox1.SelectAll();
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading unit data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Override ProcessCmdKey to handle Escape key
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Handle Escape key to close the form
            if (keyData == Keys.Escape)
            {
                System.Diagnostics.Debug.WriteLine("Escape key pressed, closing form");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ConfigureGridAppearance()
        {
            try
            {
                // Check if the grid exists
                if (ultraGrid1 == null)
                {
                    System.Diagnostics.Debug.WriteLine("ultraGrid1 is null");
                    return;
                }

                // Reset everything first to ensure clean slate
                ultraGrid1.DisplayLayout.Reset();

                // Remove the grid caption (banner)
                ultraGrid1.DisplayLayout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.ViewStyle = Infragistics.Win.UltraWinGrid.ViewStyle.SingleBand;

                // Configure the grid appearance
                ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCell = Infragistics.Win.UltraWinGrid.SelectType.None;
                ultraGrid1.DisplayLayout.Override.SelectTypeCol = Infragistics.Win.UltraWinGrid.SelectType.None;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;

                // Enable column moving and dragging - important for column draggability
                ultraGrid1.DisplayLayout.Override.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = Infragistics.Win.UltraWinGrid.AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.AllowColSwapping = Infragistics.Win.UltraWinGrid.AllowColSwapping.WithinBand;

                // Important: This setting ensures we get only row selection on click, not automatic action
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;

                // Hide the group-by area (gray bar)
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;

                // Set rounded borders for the entire grid
                ultraGrid1.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;

                // Configure grid lines - single line borders for rows and columns
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = Infragistics.Win.UIElementBorderStyle.Solid;

                // Set border width to single line
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderAlpha = Infragistics.Win.Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderAlpha = Infragistics.Win.Alpha.Opaque;

                // Remove ALL cell padding/spacing - critical to remove unwanted space
                ultraGrid1.DisplayLayout.Override.CellPadding = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                ultraGrid1.DisplayLayout.InterBandSpacing = 0;
                ultraGrid1.DisplayLayout.MaxBandDepth = 1;
                ultraGrid1.DisplayLayout.MaxColScrollRegions = 1;
                ultraGrid1.DisplayLayout.MaxRowScrollRegions = 1;

                // Define colors
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
                Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

                // Set light blue border color for cells
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;

                // Configure row height - increase to match the image (about 26-30 pixels)
                ultraGrid1.DisplayLayout.Override.MinRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.Fixed;

                // Add header styling - blue headers
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue; // Same color for no gradient
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Configure row selector appearance with blue
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue; // Same color for no gradient
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = Infragistics.Win.UltraWinGrid.RowSelectorHeaderStyle.Default;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = Infragistics.Win.UltraWinGrid.RowSelectorNumberStyle.None; // Remove numbers
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15; // Smaller width

                // Set all cells to have white background (no alternate row coloring)
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Remove alternate row appearance (make all rows white)
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Configure selected row appearance with light blue highlight
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215); // Bright blue from the image
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White; // White text for better contrast

                // Configure active row appearance - make it same as selected row
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;

                // Set font size for all cells to match the image (standard text size)
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Configure scrollbar style
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;

                // Configure the scrollbar look
                if (ultraGrid1.DisplayLayout.ScrollBarLook != null)
                {
                    // Configure button appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;

                    // Configure track appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;

                    // Configure thumb appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }

                // Configure cell appearance to increase vertical content alignment
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;

                // Connect events
                ultraGrid1.DoubleClickRow += UltraGrid1_DoubleClickRow;
                ultraGrid1.ClickCell += UltraGrid1_ClickCell;

                // Allow drop operation for drag-drop support
                ultraGrid1.AllowDrop = true;

                // Configure column auto size behavior for optimal resizing
                ultraGrid1.DisplayLayout.Override.ColumnAutoSizeMode = Infragistics.Win.UltraWinGrid.ColumnAutoSizeMode.VisibleRows;

                // Refresh the grid to apply all changes
                ultraGrid1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up grid style: {ex.Message}");
            }
        }

        private void LoadUnitData()
        {
            try
            {
                // Check if the grid exists
                if (ultraGrid1 == null)
                {
                    System.Diagnostics.Debug.WriteLine("ultraGrid1 is null");
                    return;
                }

                // Get UOM data from stored procedure
                DataTable dt = GetUOMDataFromStoredProcedure();

                // If no data returned or empty table, show a message and create an empty table
                if (dt == null || dt.Rows.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No UOM data available from database");

                    if (dt == null)
                    {
                        dt = new DataTable();
                        dt.Columns.Add("UnitID", typeof(int));
                        dt.Columns.Add("UnitName", typeof(string));
                        dt.Columns.Add("Packing", typeof(decimal));
                        dt.Columns.Add("IU_UOM_A", typeof(string));
                        // IU_RATE column removed - prices are now managed in PriceSettings
                    }

                    // Show a message to the user
                    MessageBox.Show("No Unit of Measurement data available. Please check your database connection.",
                        "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Make sure the required columns exist
                    if (!dt.Columns.Contains("IU_UOM_A"))
                    {
                        dt.Columns.Add("IU_UOM_A", typeof(string));

                        // Copy UnitName values to IU_UOM_A
                        foreach (DataRow row in dt.Rows)
                        {
                            string unitName = "";

                            // Check if we have UnitName or Unit column
                            if (dt.Columns.Contains("UnitName") && row["UnitName"] != DBNull.Value)
                            {
                                unitName = row["UnitName"].ToString();
                            }
                            else if (dt.Columns.Contains("Unit") && row["Unit"] != DBNull.Value)
                            {
                                unitName = row["Unit"].ToString();
                            }

                            row["IU_UOM_A"] = unitName.ToUpper();
                        }
                    }

                    // IU_RATE column removed - prices are now managed in PriceSettings
                }

                // Set the data source to the grid
                ultraGrid1.DataSource = dt;

                // Store total record count for navigation
                totalRecords = dt.Rows.Count;

                // Identify which units are already assigned to the item and keep only one row highlighted
                List<string> assignedUnits = GetItemAssignedUnits(_itemId);
                Infragistics.Win.UltraWinGrid.UltraGridRow rowToSelect = null;

                if (assignedUnits.Count > 0 && ultraGrid1.Rows.Count > 0)
                {
                    foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in ultraGrid1.Rows)
                    {
                        string unitName = string.Empty;

                        // Check IU_UOM_A column (displayed name)
                        if (row.Cells.Exists("IU_UOM_A") && row.Cells["IU_UOM_A"].Value != null)
                            unitName = row.Cells["IU_UOM_A"].Value.ToString();

                        // If not found, check UnitName hidden column if exists
                        if (string.IsNullOrEmpty(unitName) && row.Cells.Exists("UnitName") && row.Cells["UnitName"].Value != null)
                            unitName = row.Cells["UnitName"].Value.ToString();

                        if (!string.IsNullOrEmpty(unitName) && assignedUnits.Contains(unitName.ToUpperInvariant()))
                        {
                            rowToSelect = row;
                            break;
                        }
                    }
                }

                // If nothing matched, select the first row by default
                if (rowToSelect == null && totalRecords > 0 && ultraGrid1.Rows.Count > 0)
                {
                    rowToSelect = ultraGrid1.Rows[0];
                }

                ultraGrid1.Selected.Rows.Clear();
                if (rowToSelect != null)
                {
                    ultraGrid1.ActiveRow = rowToSelect;
                    ultraGrid1.Selected.Rows.Add(rowToSelect);
                }

                if (ultraGrid1.ActiveRow != null)
                    currentRecord = ultraGrid1.ActiveRow.Index;
                else
                    currentRecord = 0;

                // Update record count display
                UpdateRecordCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading UOM data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gets UOM data from stored procedure
        /// </summary>
        /// <returns>DataTable containing UOM data from the database</returns>
        private DataTable GetUOMDataFromStoredProcedure()
        {
            DataTable dt = new DataTable();
            try
            {
                // Check if drop is initialized
                if (drop == null)
                {
                    System.Diagnostics.Debug.WriteLine("Dropdowns object is null, initializing it");
                    drop = new Dropdowns();
                }

                // Check if we have a specific item ID to filter by
                // Item-specific filtering: show only units for the specified item
                // EXCEPTION: If called from Item Master (adding units), show ALL units
                bool isItemMasterContext = (!string.IsNullOrEmpty(FormName) &&
                                           (FormName == "ItemMasterMaster" || FormName == "ItemMasterGrid"));

                if (_itemId > 0 && !isItemMasterContext)
                {
                    try
                    {
                        // Item-specific filtering: show only units for the specified item
                        // Using GetItemUnits which maps to "ItemUnit" operation
                        ItemDDlGrid itemUnitsGrid = drop.GetItemUnits(_itemId);

                        if (itemUnitsGrid != null && itemUnitsGrid.List != null && itemUnitsGrid.List.Any())
                        {
                            // Convert list to DataTable
                            // LoadUnitData will automatically map the "Unit" property (from ItemDDl) to "IU_UOM_A"
                            dt = ConvertToDataTable(itemUnitsGrid.List);
                            System.Diagnostics.Debug.WriteLine($"Successfully loaded {dt.Rows.Count} Item-Specific UOM records");
                            return dt;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error fetching item units: " + ex.Message);
                    }
                }

                // Fallback: If no item ID or no item-specific units found, load all units
                DataBase.Operations = "Unit";
                UnitDDlGrid allUnitsGrid = drop.getUnitDDl();

                if (allUnitsGrid != null && allUnitsGrid.List != null && allUnitsGrid.List.Any())
                {
                    // Convert list to DataTable
                    dt = ConvertToDataTable(allUnitsGrid.List);

                    System.Diagnostics.Debug.WriteLine($"Successfully loaded {dt.Rows.Count} UOM records from database");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No unit data returned from repository");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetUOMDataFromStoredProcedure: " + ex.Message);
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("Inner exception: " + ex.InnerException.Message);
                }
            }

            return dt;
        }

        /// <summary>
        /// Gets list of unit names assigned to the specific item
        /// </summary>
        private List<string> GetItemAssignedUnits(int itemId)
        {
            List<string> assignedUnits = new List<string>();
            if (itemId <= 0) return assignedUnits;

            DataTable dt = new DataTable();
            try
            {
                if (drop == null) drop = new Dropdowns();

                drop.DataConnection.Open();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(Repository.STOREDPROCEDURE.POS_dropdown, (SqlConnection)drop.DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@BranchId", Convert.ToInt64(DataBase.BranchId));
                        cmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt64(DataBase.CompanyId));
                        cmd.Parameters.AddWithValue("@FinyearId", 0);
                        cmd.Parameters.AddWithValue("@Barcode", "");
                        cmd.Parameters.AddWithValue("@ItemId", itemId);
                        cmd.Parameters.AddWithValue("@description", "");
                        cmd.Parameters.AddWithValue("@Operation", "ItemUnit");

                        using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                        {
                            adapt.Fill(dt);
                            foreach (DataRow row in dt.Rows)
                            {
                                string unitName = "";
                                if (dt.Columns.Contains("UnitName") && row["UnitName"] != DBNull.Value)
                                    unitName = row["UnitName"].ToString();
                                else if (dt.Columns.Contains("Unit") && row["Unit"] != DBNull.Value)
                                    unitName = row["Unit"].ToString();

                                if (!string.IsNullOrEmpty(unitName))
                                    assignedUnits.Add(unitName.ToUpper());
                            }
                        }
                    }
                }
                finally
                {
                    if (drop.DataConnection.State == ConnectionState.Open)
                        drop.DataConnection.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting item assigned units: " + ex.Message);
            }
            return assignedUnits;
        }

        private DataTable ConvertToDataTable<T>(IEnumerable<T> data)
        {
            // Create a DataTable from the list
            DataTable table = new DataTable();

            try
            {
                // Check if data is null
                if (data == null)
                {
                    System.Diagnostics.Debug.WriteLine("Data is null in ConvertToDataTable");
                    return table;
                }

                // Check if there are any items
                if (!data.Any())
                {
                    System.Diagnostics.Debug.WriteLine("Data collection is empty in ConvertToDataTable");
                    return table;
                }

                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));

                // Add columns to the table
                foreach (PropertyDescriptor prop in properties)
                {
                    table.Columns.Add(prop.Name, System.Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }

                // Add rows to the table
                foreach (T item in data)
                {
                    if (item == null) continue;

                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                    {
                        try
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }
                        catch
                        {
                            // If getting a property value fails, use DBNull.Value
                            row[prop.Name] = DBNull.Value;
                        }
                    }
                    table.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in ConvertToDataTable: " + ex.Message);
            }

            return table;
        }

        private void FormatGridColumns()
        {
            try
            {
                // Check if the grid exists
                if (ultraGrid1 == null)
                {
                    System.Diagnostics.Debug.WriteLine("ultraGrid1 is null");
                    return;
                }

                // Make sure we have data and bands
                if (ultraGrid1.DisplayLayout.Bands.Count == 0)
                    return;

                // Hide all columns by default
                foreach (UltraGridColumn column in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    column.Hidden = true;
                }

                // Show exactly two columns: IU_UOM_A and IU_RATE as in the screenshot
                Color lightBlue = Color.FromArgb(0, 204, 255); // #00CCFF light blue

                // Set up the IU_UOM_A column
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("IU_UOM_A"))
                {
                    UltraGridColumn colUom = ultraGrid1.DisplayLayout.Bands[0].Columns["IU_UOM_A"];
                    colUom.Hidden = false;
                    colUom.Header.Caption = "IU_UOM_A"; // Match the header in the screenshot
                    colUom.Header.Appearance.BackColor = Color.FromArgb(0, 122, 204);
                    colUom.Header.Appearance.ForeColor = Color.White;
                    colUom.Header.Appearance.TextHAlign = Infragistics.Win.HAlign.Center;
                    colUom.Header.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                    colUom.Width = 140;

                    // Only selected rows will have blue background, normal rows have white background
                    colUom.CellAppearance.BackColorDisabled = Color.White;
                    colUom.CellAppearance.ForeColor = Color.Black; // Black text for normal rows
                    colUom.Header.VisiblePosition = 0; // First column
                }

                // Show Packing column
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Packing"))
                {
                    UltraGridColumn colPacking = ultraGrid1.DisplayLayout.Bands[0].Columns["Packing"];
                    colPacking.Hidden = false;
                    colPacking.Header.Caption = "Packing";
                    colPacking.Header.Appearance.BackColor = Color.FromArgb(0, 122, 204);
                    colPacking.Header.Appearance.ForeColor = Color.White;
                    colPacking.Header.Appearance.TextHAlign = Infragistics.Win.HAlign.Center;
                    colPacking.Header.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                    colPacking.Width = 80;
                    colPacking.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                    colPacking.CellAppearance.BackColorDisabled = Color.White;
                    colPacking.CellAppearance.ForeColor = Color.Black;
                    colPacking.Header.VisiblePosition = 1; // Second column, after IU_UOM_A
                }

                // IU_RATE column removed - now showing IU_UOM_A and Packing columns

                // Add click handling for grid cells
                ultraGrid1.ClickCell += UltraGrid1_ClickCell;

                // Add double-click handler for row selection
                ultraGrid1.DoubleClickRow += UltraGrid1_DoubleClickRow;

                // Setup column drag and drop
                SetupColumnDragDrop();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error formatting grid columns: " + ex.Message);
            }
        }

        // Setup column drag and drop functionality
        private void SetupColumnDragDrop()
        {
            // Enable the grid as a drop target
            ultraGrid1.AllowDrop = true;

            // Add drag event handlers for headers
            ultraGrid1.MouseDown += UltraGrid1_MouseDown;
            ultraGrid1.MouseMove += UltraGrid1_MouseMove;
            ultraGrid1.MouseUp += UltraGrid1_MouseUp;
            ultraGrid1.DragOver += UltraGrid1_DragOver;
            ultraGrid1.DragDrop += UltraGrid1_DragDrop;

            // Add context menu for column chooser
            SetupColumnChooserMenu();
        }

        // Add a method to set up the column chooser right-click menu
        private void SetupColumnChooserMenu()
        {
            // Create a context menu for the grid
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();

            // Add the column chooser menu item
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += ColumnChooserMenuItem_Click;
            gridContextMenu.Items.Add(columnChooserMenuItem);

            // Assign the context menu to the grid
            ultraGrid1.ContextMenuStrip = gridContextMenu;
        }

        // Event handler for the column chooser menu item
        private void ColumnChooserMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Column chooser functionality will be added soon!", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Handle mouse down on the grid to initiate potential drag
        private void UltraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // Reset drag state
                isDraggingColumn = false;
                columnToMove = null;

                // Store the mouse down position
                startPoint = new Point(e.X, e.Y);

                // If we're in the header area, try to determine which column
                if (e.Y < 40) // Assuming header is in the top 40 pixels
                {
                    // Use a simpler approach to find which column was clicked
                    // Calculate horizontal position of each column
                    int xPos = 0;

                    // Account for row selector width if present
                    if (ultraGrid1.DisplayLayout.Override.RowSelectors == Infragistics.Win.DefaultableBoolean.True)
                    {
                        xPos += ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
                    }

                    // Find which column contains the x position
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (!col.Hidden)
                        {
                            // Check if click is within this column's width
                            if (e.X >= xPos && e.X < xPos + col.Width)
                            {
                                columnToMove = col;
                                isDraggingColumn = true;
                                break;
                            }

                            xPos += col.Width;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in mouse down: " + ex.Message);
                isDraggingColumn = false;
                columnToMove = null;
            }
        }

        // Handle mouse move to initiate drag if needed
        private void UltraGrid1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Only track movement if we're dragging a column
                if (e.Button == MouseButtons.Left && columnToMove != null && isDraggingColumn)
                {
                    // Calculate how far the mouse has moved
                    int deltaX = Math.Abs(e.X - startPoint.X);
                    int deltaY = Math.Abs(e.Y - startPoint.Y);

                    // Only start drag if moved beyond threshold
                    if (deltaX > SystemInformation.DragSize.Width || deltaY > SystemInformation.DragSize.Height)
                    {
                        // Change cursor to indicate a drag operation
                        ultraGrid1.Cursor = Cursors.SizeWE;

                        // Show tooltip with hint
                        string columnName = !string.IsNullOrEmpty(columnToMove.Header.Caption) ?
                                        columnToMove.Header.Caption : columnToMove.Key;
                        if (toolTip != null)
                            toolTip.SetToolTip(ultraGrid1, $"Drag to reposition '{columnName}' column");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in mouse move: " + ex.Message);
                ultraGrid1.Cursor = Cursors.Default;
                if (toolTip != null)
                    toolTip.SetToolTip(ultraGrid1, "");
            }
        }

        // Update the UltraGrid1_MouseUp method
        private void UltraGrid1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                // Reset cursor
                ultraGrid1.Cursor = Cursors.Default;
                if (toolTip != null)
                    toolTip.SetToolTip(ultraGrid1, "");

                // Reset drag state
                isDraggingColumn = false;
                columnToMove = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in mouse up: " + ex.Message);
            }
        }

        // Event handler for drag over in the grid
        private void UltraGrid1_DragOver(object sender, DragEventArgs e)
        {
            // Check if the dragged data is a column item or a column
            if (e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        // Event handler for drop in the grid
        private void UltraGrid1_DragDrop(object sender, DragEventArgs e)
        {
            // This is a placeholder for future column drag-drop implementation
            ultraGrid1.Cursor = Cursors.Default;
        }

        private void UltraGrid1_ClickCell(object sender, ClickCellEventArgs e)
        {
            if (e.Cell != null)
            {
                // Highlight the row when a cell is clicked
                ultraGrid1.ActiveRow = e.Cell.Row;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(e.Cell.Row);

                // Update row position
                currentRecord = e.Cell.Row.Index;
                UpdateRecordCount();
            }
        }

        private void UltraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row != null)
            {
                // Call HandleUnitSelection to process the selection
                HandleUnitSelection();
            }
        }

        private void ConfigureComboBoxes()
        {
            // Set up search combobox - exactly matching the screenshot

        }

        private void SetupKeyHandlers()
        {
            try
            {
                // Add key handlers if grid exists
                if (ultraGrid1 != null)
                {
                    ultraGrid1.KeyDown += UltraGrid1_KeyDown;
                }

                // Set up navigation button handlers if buttons exist

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting up key handlers: " + ex.Message);
            }
        }

        private void BtnUp_Click(object sender, EventArgs e)
        {
            // Navigate to previous record
            if (currentRecord > 0)
            {
                currentRecord--;
                SelectRecord(currentRecord);
                UpdateRecordCount();
            }
        }

        private void BtnDown_Click(object sender, EventArgs e)
        {
            // Navigate to next record
            if (currentRecord < totalRecords - 1)
            {
                currentRecord++;
                SelectRecord(currentRecord);
                UpdateRecordCount();
            }
        }

        private void SelectRecord(int index)
        {
            if (index >= 0 && index < ultraGrid1.Rows.Count)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[index];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[index]);
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.Rows[index]);
            }
        }

        private void UpdateRecordCount()
        {
            try
            {
                //if (label1 != null)
                {
                    if (totalRecords > 0)
                    {
                        // Format to match "Record 1 of 246" as in screenshot
                        //label1.Text = $"Record {currentRecord + 1} of {totalRecords}";
                    }
                    else
                    {
                        //label1.Text = "Record 0 of 0";
                    }

                    // Format the label with blue text and proper font
                    //label1.ForeColor = Color.FromArgb(0, 70, 170); // Dark blue color
                    //label1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
                    //label1.AutoSize = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating record count: " + ex.Message);
            }
        }

        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle Enter key press the same way as the KeyPress event
            if (e.KeyCode == Keys.Enter)
            {
                HandleUnitSelection();
                e.Handled = true;
            }
            // Handle arrow keys for navigation
            else if (e.KeyCode == Keys.Up && currentRecord > 0)
            {
                currentRecord--;
                SelectRecord(currentRecord);
                UpdateRecordCount();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down && currentRecord < totalRecords - 1)
            {
                currentRecord++;
                SelectRecord(currentRecord);
                UpdateRecordCount();
                e.Handled = true;
            }
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // When Enter key is pressed, handle unit selection
            if (e.KeyChar == (char)Keys.Enter)
            {
                HandleUnitSelection();
                e.Handled = true;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Same as Enter key - handle unit selection
            HandleUnitSelection();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Close the form without selection
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void HandleUnitSelection()
        {
            try
            {
                if (ultraGrid1.ActiveRow == null)
                    return;

                // Handle frmSalesInvoice context
                if (FormName == "frmSalesInvoice")
                {
                    // Get the selected UOM value from IU_UOM_A column
                    string selectedUom = "";

                    if (ultraGrid1.ActiveRow.Cells["IU_UOM_A"] != null &&
                        ultraGrid1.ActiveRow.Cells["IU_UOM_A"].Value != null)
                    {
                        selectedUom = ultraGrid1.ActiveRow.Cells["IU_UOM_A"].Value.ToString();
                    }

                    // IU_RATE no longer used - prices are now managed in PriceSettings
                    // Just pass the selected UOM, the calling form will lookup price from PriceSettings

                    // Store the selected UOM in Tag property
                    // The calling form (FrmPurchase) will lookup the rate from PriceSettings
                    this.Tag = selectedUom;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                }

                // Handle ItemMasterGrid context
                if (FormName == "ItemMasterGrid")
                {
                    try
                    {
                        // Get reference to the ItemMaster form
                        ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
                        if (ItemMaster == null)
                        {
                            MessageBox.Show("Item Master form not found.");
                            return;
                        }

                        // Process all selected rows
                        bool processedAny = false;
                        if (ultraGrid1.Selected.Rows.Count > 0)
                        {
                            foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in ultraGrid1.Selected.Rows)
                            {
                                if (!row.IsDataRow) continue;
                                if (row.Cells["UnitName"] == null || row.Cells["Packing"] == null || row.Cells["UnitID"] == null)
                                    continue;

                                string uName = row.Cells["UnitName"].Value != null ? row.Cells["UnitName"].Value.ToString() : "";
                                int uId = Convert.ToInt32(row.Cells["UnitID"].Value ?? 0);
                                float pack = Convert.ToSingle(row.Cells["Packing"].Value ?? 1);

                                // Add/update UOM row
                                ItemMaster.AddOrUpdateUomRow(uName, uId, pack, 5, "0", 0);

                                // Add/update price row
                                ItemMaster.AddOrUpdatePriceRowFromBase(uName, pack);
                                processedAny = true;
                            }
                        }

                        // Fallback to ActiveRow if no rows were selected/processed
                        if (!processedAny && ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.IsDataRow)
                        {
                            UltraGridCell UnitID = this.ultraGrid1.ActiveRow.Cells["UnitID"];
                            UltraGridCell UnitName = this.ultraGrid1.ActiveRow.Cells["UnitName"];
                            UltraGridCell Packing = this.ultraGrid1.ActiveRow.Cells["Packing"];

                            if (UnitName != null && Packing != null && UnitID != null)
                            {
                                ItemMaster.AddOrUpdateUomRow(UnitName.Value.ToString(), Convert.ToInt32(UnitID.Value), Convert.ToSingle(Packing.Value), 5, "0", 0);
                                ItemMaster.AddOrUpdatePriceRowFromBase(UnitName.Value.ToString(), Convert.ToSingle(Packing.Value));
                            }
                        }

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error in HandleUnitSelection for ItemMasterGrid: " + ex.Message);

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    return;
                }

                // Handle ItemMasterMaster context
                if (FormName == "ItemMasterMaster")
                {
                    try
                    {
                        // Get reference to the ItemMaster form
                        ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
                        if (ItemMaster == null)
                        {
                            MessageBox.Show("Item Master form not found.");
                            return;
                        }

                        // Get the selected unit information
                        UltraGridCell UnitID = this.ultraGrid1.ActiveRow.Cells["UnitID"];
                        UltraGridCell UnitName = this.ultraGrid1.ActiveRow.Cells["UnitName"];
                        UltraGridCell Packing = this.ultraGrid1.ActiveRow.Cells["Packing"];

                        // Always update the base unit text and ID
                        ItemMaster.SetBaseUnitText(UnitName.Value.ToString());


                        // Initialize the UomDataGridView if needed
                        if (ItemMaster.UomDataGridView == null || ItemMaster.UomDataGridView.Columns.Count == 0)
                        {
                            // The grid hasn't been initialized yet, so we can't add rows
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                            return;
                        }

                        // Clear existing rows if any
                        if (ItemMaster.UomDataGridView.Rows.Count > 0)
                        {
                            ItemMaster.UomDataGridView.Rows.Clear();
                        }

                        // Find Ult_Price control
                        Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                            FindControlInForm(ItemMaster, "Ult_Price") as Infragistics.Win.UltraWinGrid.UltraGrid;

                        if (Ult_Price != null)
                        {
                            // Create a new empty DataTable for Ult_Price
                            DataTable dt = new DataTable();
                            dt.Columns.Add("Unit", typeof(string));
                            dt.Columns.Add("Packing", typeof(string));
                            dt.Columns.Add("Cost", typeof(float));
                            dt.Columns.Add("MarginAmt", typeof(float));
                            dt.Columns.Add("MarginPer", typeof(float));
                            dt.Columns.Add("TaxPer", typeof(float));
                            dt.Columns.Add("TaxAmt", typeof(float));
                            dt.Columns.Add("MRP", typeof(float));
                            dt.Columns.Add("RetailPrice", typeof(float));
                            dt.Columns.Add("WholeSalePrice", typeof(float));
                            dt.Columns.Add("CreditPrice", typeof(float));
                            dt.Columns.Add("CardPrice", typeof(float));
                            dt.Columns.Add("StaffPrice", typeof(float));
                            dt.Columns.Add("MinPrice", typeof(float));
                            dt.Columns.Add("AliasBarcode", typeof(string));

                            // Set the empty DataTable as the DataSource for Ult_Price
                            Ult_Price.DataSource = dt;
                        }

                        // Add the selected unit to UomDataGridView
                        int count = ItemMaster.UomDataGridView.Rows.Add();
                        ItemMaster.UomDataGridView.Rows[count].Cells["Unit"].Value = UnitName.Value.ToString();
                        ItemMaster.UomDataGridView.Rows[count].Cells["UnitId"].Value = UnitID.Value.ToString();
                        ItemMaster.UomDataGridView.Rows[count].Cells["Packing"].Value = Packing.Value.ToString();
                        ItemMaster.UomDataGridView.Rows[count].Cells["Reorder"].Value = 5;
                        ItemMaster.UomDataGridView.Rows[count].Cells["BarCode"].Value = 0;
                        ItemMaster.UomDataGridView.Rows[count].Cells["OpnStk"].Value = 0;

                        // Add the selected unit to Ult_Price
                        if (Ult_Price != null)
                        {
                            try
                            {
                                // Get the DataTable from Ult_Price
                                DataTable dt = Ult_Price.DataSource as DataTable;
                                if (dt == null)
                                {
                                    dt = new DataTable();
                                    dt.Columns.Add("Unit", typeof(string));
                                    dt.Columns.Add("Packing", typeof(string));
                                    dt.Columns.Add("Cost", typeof(float));
                                    dt.Columns.Add("MarginAmt", typeof(float));
                                    dt.Columns.Add("MarginPer", typeof(float));
                                    dt.Columns.Add("TaxPer", typeof(float));
                                    dt.Columns.Add("TaxAmt", typeof(float));
                                    dt.Columns.Add("MRP", typeof(float));
                                    dt.Columns.Add("RetailPrice", typeof(float));
                                    dt.Columns.Add("WholeSalePrice", typeof(float));
                                    dt.Columns.Add("CreditPrice", typeof(float));
                                    dt.Columns.Add("CardPrice", typeof(float));
                                    dt.Columns.Add("StaffPrice", typeof(float));
                                    dt.Columns.Add("MinPrice", typeof(float));
                                    dt.Columns.Add("AliasBarcode", typeof(string));
                                }

                                // Create a new row
                                DataRow newRow = dt.NewRow();
                                newRow["Unit"] = UnitName.Value.ToString();
                                newRow["Packing"] = Packing.Value.ToString();
                                newRow["Cost"] = 0f;
                                newRow["MRP"] = 0f;
                                newRow["RetailPrice"] = 0f;
                                newRow["WholeSalePrice"] = 0f;
                                newRow["CreditPrice"] = 0f;
                                newRow["CardPrice"] = 0f;
                                newRow["StaffPrice"] = 0f;
                                newRow["MinPrice"] = 0f;
                                newRow["MarginAmt"] = 0f;
                                newRow["MarginPer"] = 0f;

                                // Add tax information if available
                                if (!string.IsNullOrEmpty(ItemMaster.txt_TaxPer.Text))
                                {
                                    float taxPer;
                                    if (float.TryParse(ItemMaster.txt_TaxPer.Text, out taxPer))
                                    {
                                        newRow["TaxPer"] = taxPer;
                                    }
                                    else
                                    {
                                        newRow["TaxPer"] = 0f;
                                    }
                                }
                                else
                                {
                                    newRow["TaxPer"] = 0f;
                                }

                                if (!string.IsNullOrEmpty(ItemMaster.txt_TaxAmount.Text))
                                {
                                    newRow["TaxAmt"] = 0f;
                                }
                                else
                                {
                                    newRow["TaxAmt"] = 0f;
                                }

                                // Add the row to the DataTable
                                dt.Rows.Add(newRow);

                                // Set the DataTable as the DataSource for Ult_Price
                                Ult_Price.DataSource = dt;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("Error setting price grid: " + ex.Message);
                                // Continue without showing error to user
                            }
                        }

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error in HandleUnitSelection for ItemMasterMaster: " + ex.Message);
                        // Set the base unit text even if other operations fail
                        try
                        {
                            if (ItemMaster != null && ultraGrid1.ActiveRow != null &&
                                ultraGrid1.ActiveRow.Cells["UnitName"] != null &&
                                ultraGrid1.ActiveRow.Cells["UnitName"].Value != null)
                            {
                                ItemMaster.SetBaseUnitText(ultraGrid1.ActiveRow.Cells["UnitName"].Value.ToString());
                            }
                        }
                        catch { }

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    return;
                }

                // Default case for any other form
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting unit: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool checkUnitExist(string Unit)
        {
            this.checke = false;
            try
            {
                // Get reference to the ItemMaster form
                ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];

                // If ItemMaster form doesn't exist or UomDataGridView is not initialized, return false
                if (ItemMaster == null || ItemMaster.UomDataGridView == null)
                    return false;

                // If no rows in UomDataGridView, return false
                if (ItemMaster.UomDataGridView.Rows.Count <= 0)
                    return false;

                // Check if the unit already exists
                for (int i = 0; i < ItemMaster.UomDataGridView.Rows.Count; i++)
                {
                    try
                    {
                        // Skip null rows or cells
                        if (ItemMaster.UomDataGridView.Rows[i] == null)
                            continue;

                        if (ItemMaster.UomDataGridView.Rows[i].Cells["Unit"] == null)
                            continue;

                        if (ItemMaster.UomDataGridView.Rows[i].Cells["Unit"].Value == null)
                            continue;

                        // Compare unit names
                        string existingUnit = ItemMaster.UomDataGridView.Rows[i].Cells["Unit"].Value.ToString();
                        if (existingUnit == Unit)
                        {
                            this.checke = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error checking row " + i + ": " + ex.Message);
                        // Continue checking other rows
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in checkUnitExist: " + ex.Message);
                // Return false on error
                return false;
            }
            return checke;
        }

        private void SetupNavigationButtons()
        {
            try
            {
                // Check if buttons exist before configuring them


                // Configure the up button


                // Create up arrow icon
                Bitmap upArrow = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(upArrow))
                {
                    g.Clear(Color.FromArgb(0, 204, 255));
                    using (Pen pen = new Pen(Color.White, 3))
                    {
                        // Draw arrow pointing up
                        g.DrawLine(pen, 16, 8, 16, 24);
                        g.DrawLine(pen, 16, 8, 8, 16);
                        g.DrawLine(pen, 16, 8, 24, 16);
                    }
                }

                // Create down arrow icon
                Bitmap downArrow = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(downArrow))
                {
                    g.Clear(Color.FromArgb(0, 204, 255));
                    using (Pen pen = new Pen(Color.White, 3))
                    {
                        // Draw arrow pointing down
                        g.DrawLine(pen, 16, 24, 16, 8);
                        g.DrawLine(pen, 16, 24, 8, 16);
                        g.DrawLine(pen, 16, 24, 24, 16);
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting up navigation buttons: " + ex.Message);
            }
        }

        private void frmUnitDialog_Resize(object sender, EventArgs e)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Calculate total available width
                int totalWidth = ultraGrid1.Width - ultraGrid1.DisplayLayout.Override.RowSelectorWidth - 20; // 20 for scrollbar and borders

                // Adjust widths for the single main column
                if (band.Columns.Exists("IU_UOM_A") && !band.Columns["IU_UOM_A"].Hidden)
                {
                    band.Columns["IU_UOM_A"].Width = totalWidth; // Use full width since IU_RATE is removed
                }

                // IU_RATE column removed - no longer setting its width
            }
        }

        /// <summary>
        /// Styles panels to match the appearance in frmdialForItemMaster
        /// </summary>
        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            // Define consistent colors for all panels - exact match from frmdialForItemMaster.cs
            Color lightBlue = Color.FromArgb(127, 219, 255); // Light blue
            Color darkBlue = Color.FromArgb(0, 116, 217);    // Darker blue
            Color borderBlue = Color.FromArgb(0, 150, 220);  // Border blue
            Color borderBase = Color.FromArgb(0, 100, 180);  // Border base color

            // Create a gradient from light to dark blue with exact specified colors
            panel.Appearance.BackColor = lightBlue;
            panel.Appearance.BackColor2 = darkBlue;
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;

            // Special case for navigation panels (ultraPanel3 and ultraPanel7)
            if (panel == ultraPanel3 || panel == ultraPanel7)
            {
                // Set the exact appearance from frmdialForItemMaster.cs
                panel.Appearance.BackColor = lightBlue;
                panel.Appearance.BackColor2 = darkBlue;
                panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;
                panel.Appearance.BorderColor = borderBlue;
                panel.Appearance.BorderColor3DBase = borderBase;

                // Make slightly larger size for better appearance
                if (panel == ultraPanel3)
                {
                    panel.Size = new Size(61, 55); // Match the size in frmdialForItemMaster
                }
                else if (panel == ultraPanel7)
                {
                    panel.Size = new Size(61, 55); // Match the size in frmdialForItemMaster
                }
            }
            else
            {
                // Set highly rounded border style for other panels
                panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;
                panel.Appearance.BorderColor = borderBlue;
                panel.Appearance.BorderColor3DBase = borderBase;
            }

            // Add shadow/3D effect
            panel.Appearance.BorderColor3DBase = Color.FromArgb(0, 100, 180);

            // Add glass-like reflection effect
            panel.Appearance.BackHatchStyle = Infragistics.Win.BackHatchStyle.None;
            panel.Appearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

            // Ensure icons inside have transparent background
            foreach (Control control in panel.ClientArea.Controls)
            {
                if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                {
                    Infragistics.Win.UltraWinEditors.UltraPictureBox pic = (Infragistics.Win.UltraWinEditors.UltraPictureBox)control;
                    pic.BackColor = Color.Transparent;
                    pic.BackColorInternal = Color.Transparent;
                    pic.BorderShadowColor = Color.Transparent;

                    // Center the picture in navigation panels
                    if (panel == ultraPanel3 || panel == ultraPanel7)
                    {
                        pic.Size = new Size(46, 31);
                        pic.Location = new Point((panel.Width - pic.Width) / 2, (panel.Height - pic.Height) / 2);
                    }
                }
                else if (control is Label)
                {
                    ((Label)control).ForeColor = Color.White;
                    ((Label)control).Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
                    ((Label)control).BackColor = Color.Transparent;
                    ((Label)control).TextAlign = ContentAlignment.MiddleCenter; // Center-align text
                }
            }

            // Add hover effect with consistent colors - different treatment for navigation panels
            if (panel == ultraPanel3 || panel == ultraPanel7)
            {
                panel.ClientArea.MouseEnter += (sender, e) =>
                {
                    panel.Appearance.BackColor = Color.FromArgb(160, 230, 255);
                    panel.Appearance.BackColor2 = Color.FromArgb(30, 140, 230);
                    panel.ClientArea.Cursor = Cursors.Hand;
                };

                panel.ClientArea.MouseLeave += (sender, e) =>
                {
                    panel.Appearance.BackColor = lightBlue;
                    panel.Appearance.BackColor2 = darkBlue;
                    panel.ClientArea.Cursor = Cursors.Default;
                };
            }
            else
            {
                // Standard hover effect for other panels
                panel.ClientArea.MouseEnter += (sender, e) =>
                {
                    panel.Appearance.BackColor = Color.FromArgb(160, 230, 255);
                    panel.Appearance.BackColor2 = Color.FromArgb(30, 140, 230);
                };

                panel.ClientArea.MouseLeave += (sender, e) =>
                {
                    panel.Appearance.BackColor = lightBlue;
                    panel.Appearance.BackColor2 = darkBlue;
                };

                // Set cursor to hand to indicate clickable
                panel.ClientArea.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// Setup hover effects and click handlers for panels
        /// </summary>
        private void SetupPanelHoverEffects()
        {
            try
            {
                // Set up hover effects for each panel with its controls
                SetupPanelGroupHoverEffects(ultraPanel5, label5, ultraPictureBox1);
                SetupPanelGroupHoverEffects(ultraPanel6, label3, ultraPictureBox2);
                SetupPanelGroupHoverEffects(ultraPanel3, null, ultraPictureBox5);
                SetupPanelGroupHoverEffects(ultraPanel7, null, ultraPictureBox6);
                SetupPanelGroupHoverEffects(ultraPanel4, label4, ultraPictureBox3);

                // Add click handlers for ultraPanel5, ultraPanel6, and ultraPanel4
                ultraPanel5.Click += Panel5_Click;
                ultraPanel5.ClientArea.Click += Panel5_Click;
                label5.Click += Panel5_Click;
                ultraPictureBox1.Click += Panel5_Click;

                ultraPanel6.Click += Panel6_Click;
                ultraPanel6.ClientArea.Click += Panel6_Click;
                label3.Click += Panel6_Click;
                ultraPictureBox2.Click += Panel6_Click;

                ultraPanel4.Click += Panel4_Click;
                ultraPanel4.ClientArea.Click += Panel4_Click;
                label4.Click += Panel4_Click;
                ultraPictureBox3.Click += Panel4_Click;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting up panel effects: " + ex.Message);
            }
        }

        /// <summary>
        /// Click handler for Panel5 (OK button)
        /// </summary>
        private void Panel5_Click(object sender, EventArgs e)
        {
            try
            {
                // Handle OK action - select the unit and close
                if (ultraGrid1.ActiveRow != null)
                {
                    // Handle unit selection like double-click
                    HandleUnitSelection();
                }
                else if (ultraGrid1.Rows != null && ultraGrid1.Rows.Count > 0)
                {
                    // Select first row if none is selected
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    // Then handle the selection
                    HandleUnitSelection();
                }
                else
                {
                    MessageBox.Show("Please select a unit first.", "No Unit Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in OK panel click: " + ex.Message);
            }
        }

        /// <summary>
        /// Click handler for Panel6 (Close button)
        /// </summary>
        private void Panel6_Click(object sender, EventArgs e)
        {
            try
            {
                // Close the form without selecting
                System.Diagnostics.Debug.WriteLine("Close button clicked in frmUnitDialog");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Close panel click: " + ex.Message);
            }
        }

        /// <summary>
        /// Click handler for Panel4 - Opens FrmUnitMaster in UltraTabControl
        /// </summary>
        private void Panel4_Click(object sender, EventArgs e)
        {
            try
            {
                // Find the main Home form that contains tabControlMain
                Form homeForm = FindHomeForm();

                if (homeForm != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Home form found: {homeForm.Name}");

                    // Get the tabControlMain from the Home form
                    var tabControlMain = GetTabControlFromHome(homeForm);

                    if (tabControlMain != null)
                    {
                        // Check if Unit Master tab already exists
                        foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
                        {
                            if (tab.Text == "Unit Master")
                            {
                                tabControlMain.SelectedTab = tab;
                                System.Diagnostics.Debug.WriteLine("Unit Master tab already exists, selected existing tab");
                                return;
                            }
                        }

                        // Create new tab using the same approach as Home.cs
                        string uniqueKey = $"Tab_{DateTime.Now.Ticks}_Unit Master";
                        var newTab = tabControlMain.Tabs.Add(uniqueKey, "Unit Master");

                        // Create and configure FrmUnitMaster for embedding (same as Home.cs)
                        FrmUnitMaster unitMasterForm = new FrmUnitMaster();
                        unitMasterForm.TopLevel = false;
                        unitMasterForm.FormBorderStyle = FormBorderStyle.None;
                        unitMasterForm.Dock = DockStyle.Fill;
                        unitMasterForm.Visible = true;
                        unitMasterForm.BackColor = SystemColors.Control;

                        // Ensure form is properly initialized
                        if (!unitMasterForm.IsHandleCreated)
                        {
                            unitMasterForm.CreateControl();
                        }

                        // Add the form to the tab page
                        newTab.TabPage.Controls.Add(unitMasterForm);

                        // Show the form AFTER adding to tab page
                        unitMasterForm.Show();
                        unitMasterForm.BringToFront();

                        // Set the new tab as active/selected
                        tabControlMain.SelectedTab = newTab;

                        // Force refresh to ensure proper display
                        newTab.TabPage.Refresh();
                        unitMasterForm.Refresh();
                        tabControlMain.Refresh();

                        // Wire up the form's FormClosed event to remove the tab
                        unitMasterForm.FormClosed += (formSender, formE) =>
                        {
                            try
                            {
                                if (newTab != null && tabControlMain.Tabs.Contains(newTab))
                                {
                                    tabControlMain.Tabs.Remove(newTab);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error removing tab: {ex.Message}");
                            }
                        };

                        System.Diagnostics.Debug.WriteLine("FrmUnitMaster opened in UltraTabControl using Home.cs approach");

                        // Close the frmUnitDialog form after successfully opening FrmUnitMaster
                        // Add a small delay to ensure FrmUnitMaster is fully loaded
                        System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
                        {
                            this.Invoke(new Action(() =>
                            {
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            }));
                        });
                    }
                    else
                    {
                        // Fallback: show as regular form
                        FrmUnitMaster unitMasterForm = new FrmUnitMaster();
                        unitMasterForm.Show();
                        System.Diagnostics.Debug.WriteLine("FrmUnitMaster opened as regular form (tabControlMain not found)");

                        // Close the frmUnitDialog form with delay
                        System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
                        {
                            this.Invoke(new Action(() =>
                            {
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            }));
                        });
                    }
                }
                else
                {
                    // If no Home form found, show as a regular form
                    FrmUnitMaster unitMasterForm = new FrmUnitMaster();
                    unitMasterForm.Show();
                    System.Diagnostics.Debug.WriteLine("FrmUnitMaster opened as regular form (no Home form found)");

                    // Close the frmUnitDialog form with delay
                    System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
                    {
                        this.Invoke(new Action(() =>
                        {
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }));
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error opening FrmUnitMaster: " + ex.Message);
                MessageBox.Show("Error opening Unit Master: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Helper method to find a searchbox in the form
        /// </summary>
        private TextBox FindSearchBox()
        {
            // Look for textbox controls that might be used for search
            foreach (Control control in this.Controls)
            {
                if (control is TextBox)
                {
                    // Check if name or tag suggests it's a search box
                    if (control.Name.Contains("search") || control.Name.Contains("filter") ||
                        control.Name == "textBox1" || control.Name == "txtSearch")
                    {
                        return control as TextBox;
                    }
                }

                // Look in child containers
                if (control.HasChildren)
                {
                    foreach (Control child in control.Controls)
                    {
                        if (child is TextBox)
                        {
                            if (child.Name.Contains("search") || child.Name.Contains("filter") ||
                                child.Name == "textBox1" || child.Name == "txtSearch")
                            {
                                return child as TextBox;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Helper method to find the Home form
        /// </summary>
        private Form FindHomeForm()
        {
            try
            {
                // Look for Home form in all open forms
                foreach (Form form in Application.OpenForms)
                {
                    if (form.GetType().Name == "Home" || form.Name == "Home")
                    {
                        System.Diagnostics.Debug.WriteLine($"Found Home form: {form.Name}");
                        return form;
                    }
                }

                System.Diagnostics.Debug.WriteLine("Home form not found");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding Home form: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Helper method to get tabControlMain from Home form
        /// </summary>
        private Infragistics.Win.UltraWinTabControl.UltraTabControl GetTabControlFromHome(Form homeForm)
        {
            try
            {
                // Look for tabControlMain in the Home form
                var tabControl = FindControlByName(homeForm, "tabControlMain");
                if (tabControl is Infragistics.Win.UltraWinTabControl.UltraTabControl)
                {
                    System.Diagnostics.Debug.WriteLine("Found tabControlMain in Home form");
                    return tabControl as Infragistics.Win.UltraWinTabControl.UltraTabControl;
                }

                System.Diagnostics.Debug.WriteLine("tabControlMain not found in Home form");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting tabControlMain: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Helper method to find a control by name recursively
        /// </summary>
        private Control FindControlByName(Control container, string name)
        {
            try
            {
                // Check if the container itself is the control we're looking for
                if (container.Name == name)
                    return container;

                // Search through all child controls
                foreach (Control ctrl in container.Controls)
                {
                    // Recursively check this control and its children
                    Control foundControl = FindControlByName(ctrl, name);
                    if (foundControl != null)
                        return foundControl;
                }

                // Not found in this container
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding control '{name}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Helper method to find UltraTabControl in a form
        /// </summary>
        private Infragistics.Win.UltraWinTabControl.UltraTabControl FindUltraTabControl(Control container)
        {
            try
            {
                // Check if the container itself is the UltraTabControl
                if (container is Infragistics.Win.UltraWinTabControl.UltraTabControl)
                {
                    System.Diagnostics.Debug.WriteLine($"Found UltraTabControl: {container.Name}");
                    return container as Infragistics.Win.UltraWinTabControl.UltraTabControl;
                }

                // Search through all child controls
                foreach (Control ctrl in container.Controls)
                {
                    System.Diagnostics.Debug.WriteLine($"Searching in control: {ctrl.Name} ({ctrl.GetType().Name})");

                    // Recursively check this control and its children
                    Infragistics.Win.UltraWinTabControl.UltraTabControl foundControl = FindUltraTabControl(ctrl);
                    if (foundControl != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found UltraTabControl in child: {ctrl.Name}");
                        return foundControl;
                    }
                }

                // Not found in this container
                System.Diagnostics.Debug.WriteLine($"UltraTabControl not found in: {container.Name}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching for UltraTabControl: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Set up hover effects for a panel group
        /// </summary>
        private void SetupPanelGroupHoverEffects(
            Infragistics.Win.Misc.UltraPanel panel,
            Label label,
            Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox)
        {
            if (panel == null) return;

            // Store original colors
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;

            // Define hover colors - brighter versions of the original colors
            Color hoverBackColor = BrightenColor(originalBackColor, 30);
            Color hoverBackColor2 = BrightenColor(originalBackColor2, 30);

            // Create actions for mouse enter and leave
            Action applyHoverEffect = () =>
            {
                // Change panel colors
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;

                // Change cursor to hand
                panel.ClientArea.Cursor = Cursors.Hand;
            };

            Action removeHoverEffect = () =>
            {
                // Restore original colors
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;

                // Restore cursor
                panel.ClientArea.Cursor = Cursors.Default;
            };

            // Add hover effects to the panel
            panel.MouseEnter += (s, e) =>
            {
                applyHoverEffect();
            };

            panel.MouseLeave += (s, e) =>
            {
                removeHoverEffect();
            };

            // Add hover effects to the picture box if provided
            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (s, e) =>
                {
                    // Apply hover effect to the panel
                    applyHoverEffect();

                    // Change cursor to hand
                    pictureBox.Cursor = Cursors.Hand;
                };

                pictureBox.MouseLeave += (s, e) =>
                {
                    // Only restore panel colors if mouse is not still over the panel
                    if (!IsMouseOverControl(panel))
                    {
                        removeHoverEffect();
                    }
                };
            }

            // Add hover effects to the label if provided
            if (label != null)
            {
                label.MouseEnter += (s, e) =>
                {
                    // Apply hover effect to the panel
                    applyHoverEffect();

                    // Change cursor to hand
                    label.Cursor = Cursors.Hand;
                };

                label.MouseLeave += (s, e) =>
                {
                    // Only restore panel colors if mouse is not still over the panel
                    if (!IsMouseOverControl(panel))
                    {
                        removeHoverEffect();
                    }
                };
            }
        }

        /// <summary>
        /// Helper method to brighten a color
        /// </summary>
        private Color BrightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(color.R + amount, 255),
                Math.Min(color.G + amount, 255),
                Math.Min(color.B + amount, 255));
        }

        /// <summary>
        /// Helper method to check if mouse is over a control
        /// </summary>
        private bool IsMouseOverControl(Control control)
        {
            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }

        /// <summary>
        /// Set up navigation panel click handlers for ultraPanel3 and ultraPanel7
        /// </summary>
        private void SetupNavigationPanels()
        {
            try
            {
                // Connect panel 3 (Up) click events
                ConnectNavigationPanelEvents(ultraPanel3, ultraPictureBox5, MoveItemHighlighterUp);

                // Connect panel 7 (Down) click events
                ConnectNavigationPanelEvents(ultraPanel7, ultraPictureBox6, MoveItemHighlighterDown);

                // Add tooltips for navigation clarity
                System.Windows.Forms.ToolTip navigationTooltip = new System.Windows.Forms.ToolTip();
                navigationTooltip.AutoPopDelay = 5000;
                navigationTooltip.InitialDelay = 500;
                navigationTooltip.ReshowDelay = 500;
                navigationTooltip.ShowAlways = true;

                // Set tooltips for each panel and pictureBox
                navigationTooltip.SetToolTip(ultraPanel3, "Move to previous record (Up)");
                navigationTooltip.SetToolTip(ultraPictureBox5, "Move to previous record (Up)");

                navigationTooltip.SetToolTip(ultraPanel7, "Move to next record (Down)");
                navigationTooltip.SetToolTip(ultraPictureBox6, "Move to next record (Down)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting up navigation panels: " + ex.Message);
            }
        }

        /// <summary>
        /// Connect events for navigation panels
        /// </summary>
        private void ConnectNavigationPanelEvents(Infragistics.Win.Misc.UltraPanel panel,
            Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox,
            EventHandler handler)
        {
            // Store original colors for visual feedback
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;

            // Create brighter colors for feedback
            Color pressedBackColor = BrightenColor(originalBackColor, 30);
            Color pressedBackColor2 = BrightenColor(originalBackColor2, 30);

            // Connect the click events
            panel.Click += handler;
            panel.ClientArea.Click += handler;
            if (pictureBox != null)
            {
                pictureBox.Click += handler;
            }

            // Add visual feedback without causing the handler to fire multiple times
            panel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    // Change color for visual feedback only
                    panel.Appearance.BackColor = pressedBackColor;
                    panel.Appearance.BackColor2 = pressedBackColor2;
                }
            };

            panel.MouseUp += (s, e) =>
            {
                // Restore original colors
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
            };

            // Same for client area
            panel.ClientArea.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    panel.Appearance.BackColor = pressedBackColor;
                    panel.Appearance.BackColor2 = pressedBackColor2;
                }
            };

            panel.ClientArea.MouseUp += (s, e) =>
            {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
            };

            // Same for picture box
            if (pictureBox != null)
            {
                pictureBox.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        panel.Appearance.BackColor = pressedBackColor;
                        panel.Appearance.BackColor2 = pressedBackColor2;
                    }
                };

                pictureBox.MouseUp += (s, e) =>
                {
                    panel.Appearance.BackColor = originalBackColor;
                    panel.Appearance.BackColor2 = originalBackColor2;
                };
            }
        }

        /// <summary>
        /// Event handler to move the item highlighter up
        /// </summary>
        private void MoveItemHighlighterUp(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.Rows.Count == 0)
                    return;

                // If no active row, select the last row
                if (ultraGrid1.ActiveRow == null)
                {
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                        ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                    }
                    return;
                }

                int currentIndex = ultraGrid1.ActiveRow.Index;
                if (currentIndex > 0)
                {
                    // Get the row to activate before making any changes
                    UltraGridRow rowToActivate = ultraGrid1.Rows[currentIndex - 1];

                    // Ensure the row will be visible before activating it
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);

                    // Now activate the row and update selection
                    ultraGrid1.ActiveRow = rowToActivate;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(rowToActivate);

                    // Update current record position
                    currentRecord = currentIndex - 1;
                    UpdateRecordCount();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error moving highlighter up: " + ex.Message);
            }
        }

        /// <summary>
        /// Event handler to move the item highlighter down
        /// </summary>
        private void MoveItemHighlighterDown(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.Rows.Count == 0)
                    return;

                // If no active row, select the first row
                if (ultraGrid1.ActiveRow == null)
                {
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                        ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                    }
                    return;
                }

                int currentIndex = ultraGrid1.ActiveRow.Index;
                if (currentIndex < ultraGrid1.Rows.Count - 1)
                {
                    // Get the row to activate before making any changes
                    UltraGridRow rowToActivate = ultraGrid1.Rows[currentIndex + 1];

                    // Ensure the row will be visible before activating it
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);

                    // Now activate the row and update selection
                    ultraGrid1.ActiveRow = rowToActivate;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(rowToActivate);

                    // Update current record position
                    currentRecord = currentIndex + 1;
                    UpdateRecordCount();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error moving highlighter down: " + ex.Message);
            }
        }

        // Locate textBox1 and attach search handlers
        private void WireSearchBoxHandlers()
        {
            try
            {
                Control[] found = this.Controls.Find("textBox1", true);
                if (found != null && found.Length > 0 && found[0] is TextBox)
                {
                    TextBox tb = (TextBox)found[0];
                    tb.TextChanged -= textBox1_TextChanged;
                    tb.TextChanged += textBox1_TextChanged;
                    tb.KeyDown -= textBox1_KeyDown;
                    tb.KeyDown += textBox1_KeyDown;
                }
            }
            catch { }
        }

        // Live filter grid rows by IU_UOM_A containing the search text
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string term = ((TextBox)sender).Text ?? string.Empty;
                ApplyRowFilter(term);
            }
            catch { }
        }

        // On Enter in search, focus grid and select current highlighted row
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    // Ensure grid has focus and an active row
                    if (this.ultraGrid1.Rows != null && this.ultraGrid1.Rows.VisibleRowCount > 0)
                    {
                        // If no active row, set first visible row active
                        if (this.ultraGrid1.ActiveRow == null || this.ultraGrid1.ActiveRow.Hidden)
                        {
                            UltraGridRow firstVisible = this.ultraGrid1.Rows.GetRowAtVisibleIndex(0);
                            if (firstVisible != null)
                            {
                                this.ultraGrid1.ActiveRow = firstVisible;
                                this.ultraGrid1.Selected.Rows.Clear();
                                this.ultraGrid1.Selected.Rows.Add(firstVisible);
                            }
                        }

                        // Select the active row (same as pressing Enter on grid)
                        if (this.ultraGrid1.ActiveRow != null && !this.ultraGrid1.ActiveRow.Hidden)
                        {
                            HandleUnitSelection();
                        }
                    }

                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    // Move focus to grid and select first visible
                    this.ultraGrid1.Focus();
                    if (this.ultraGrid1.Rows != null && this.ultraGrid1.Rows.VisibleRowCount > 0)
                    {
                        UltraGridRow firstVisible = this.ultraGrid1.Rows.GetRowAtVisibleIndex(0);
                        if (firstVisible != null)
                        {
                            this.ultraGrid1.ActiveRow = firstVisible;
                            this.ultraGrid1.Selected.Rows.Clear();
                            this.ultraGrid1.Selected.Rows.Add(firstVisible);
                        }
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            catch { }
        }

        // Apply filtering by hiding rows that do not match IU_UOM_A (case-insensitive contains)
        private void ApplyRowFilter(string searchText)
        {
            try
            {
                string term = (searchText ?? string.Empty).Trim();
                bool hasTerm = term.Length > 0;
                string termLower = term.ToLowerInvariant();

                if (this.ultraGrid1.Rows == null) return;

                // Suspend painting for performance
                this.ultraGrid1.SuspendLayout();

                foreach (UltraGridRow row in this.ultraGrid1.Rows)
                {
                    bool match = true;
                    if (hasTerm)
                    {
                        string value = string.Empty;
                        if (row.Cells.Exists("IU_UOM_A") && row.Cells["IU_UOM_A"].Value != null)
                        {
                            value = Convert.ToString(row.Cells["IU_UOM_A"].Value);
                        }
                        match = value != null && value.ToLowerInvariant().Contains(termLower);
                    }
                    row.Hidden = !match;
                }

                // Ensure an active visible row after filtering
                if (this.ultraGrid1.Rows.VisibleRowCount > 0)
                {
                    UltraGridRow firstVisible = this.ultraGrid1.Rows.GetRowAtVisibleIndex(0);
                    if (firstVisible != null)
                    {
                        this.ultraGrid1.ActiveRow = firstVisible;
                        this.ultraGrid1.Selected.Rows.Clear();
                        this.ultraGrid1.Selected.Rows.Add(firstVisible);
                    }
                }

                this.ultraGrid1.ResumeLayout();
            }
            catch { }
        }

        // Initialize textBox3 with total count and update label1
        private void InitializeCountBoxAndLabel()
        {
            try
            {
                int total = (this.ultraGrid1.Rows != null) ? this.ultraGrid1.Rows.Count : 0;
                Control[] found3 = this.Controls.Find("textBox3", true);
                if (found3 != null && found3.Length > 0 && found3[0] is TextBox)
                {
                    ((TextBox)found3[0]).Text = total.ToString();
                }
                UpdateCountsLabel();
            }
            catch { }
        }

        // Hook up textBox3 handlers
        private void WireCountBoxHandlers()
        {
            try
            {
                Control[] found3 = this.Controls.Find("textBox3", true);
                if (found3 != null && found3.Length > 0 && found3[0] is TextBox)
                {
                    TextBox tb = (TextBox)found3[0];
                    tb.TextChanged -= textBox3_TextChanged;
                    tb.TextChanged += textBox3_TextChanged;
                    tb.KeyDown -= textBox3_KeyDown;
                    tb.KeyDown += textBox3_KeyDown;
                }
            }
            catch { }
        }

        // Initialize debounce timer for textBox3
        private void InitializeTextBox3DebounceTimer()
        {
            try
            {
                textBox3DebounceTimer = new System.Windows.Forms.Timer();
                textBox3DebounceTimer.Interval = 5; // Very short interval for instant response
                textBox3DebounceTimer.Tick += TextBox3DebounceTimer_Tick;
            }
            catch { }
        }

        // When textBox3 changes, apply numeric filter/limit live and update label
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Reset and restart the timer on each keystroke
                textBox3DebounceTimer.Stop();

                // If the value is the same as last processed, don't reprocess
                string currentValue = ((TextBox)sender).Text ?? string.Empty;
                if (currentValue == lastProcessedTextBox3Value)
                    return;

                // Start the timer to process after a very short delay
                textBox3DebounceTimer.Start();
            }
            catch { }
        }

        // Timer tick handler for debounced processing
        private void TextBox3DebounceTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Stop the timer
                textBox3DebounceTimer.Stop();

                // Process the value
                ProcessTextBox3ValueImmediate();
            }
            catch { }
        }

        // Process the textBox3 value immediately
        private void ProcessTextBox3ValueImmediate()
        {
            try
            {
                // Get current value from textBox3
                string currentValue = string.Empty;
                Control[] found3 = this.Controls.Find("textBox3", true);
                if (found3 != null && found3.Length > 0 && found3[0] is TextBox)
                {
                    currentValue = ((TextBox)found3[0]).Text ?? string.Empty;
                }

                // Store the value being processed to avoid reprocessing
                lastProcessedTextBox3Value = currentValue;

                // Apply filters and update label
                ApplyFiltersFromTextBox3();
                UpdateCountsLabel();
            }
            catch { }
        }

        // On Enter in textBox3, apply filter/limit and update label
        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    // Stop timer and process immediately
                    textBox3DebounceTimer.Stop();
                    ProcessTextBox3ValueImmediate();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            catch { }
        }

        // Update label1 with current visible vs total counts
        private void UpdateCountsLabel()
        {
            try
            {
                int total = (this.ultraGrid1.Rows != null) ? this.ultraGrid1.Rows.Count : 0;
                int visible = 0;
                if (this.ultraGrid1.Rows != null)
                {
                    foreach (UltraGridRow r in this.ultraGrid1.Rows)
                    {
                        if (!r.Hidden) visible++;
                    }
                }

                Control[] found = this.Controls.Find("label1", true);
                if (found != null && found.Length > 0 && found[0] is Label)
                {
                    Label lbl = (Label)found[0];
                    lbl.Text = $"Showing {visible} of {total} records";
                }
            }
            catch { }
        }

        // Apply filters based on textBox3 input: numbers filter rows; empty/all resets to show all
        private void ApplyFiltersFromTextBox3()
        {
            try
            {
                if (this.ultraGrid1.Rows == null) return;

                // Read term
                string input = string.Empty;
                Control[] found3 = this.Controls.Find("textBox3", true);
                if (found3 != null && found3.Length > 0 && found3[0] is TextBox)
                {
                    input = ((TextBox)found3[0]).Text.Trim();
                }

                // If empty or "all" → show all items (reset any previous filtering)
                if (string.IsNullOrEmpty(input) || string.Equals(input, "all", StringComparison.OrdinalIgnoreCase))
                {
                    // Show all rows (reset any hiding from previous filters)
                    this.ultraGrid1.SuspendLayout();
                    foreach (UltraGridRow row in this.ultraGrid1.Rows)
                    {
                        row.Hidden = false;
                    }
                    this.ultraGrid1.ResumeLayout();
                    EnsureFirstVisibleRowActive();
                    return;
                }

                // Try to parse as number for row limiting
                if (int.TryParse(input, out int limit) && limit > 0)
                {
                    // Show all rows first, then apply limit
                    this.ultraGrid1.SuspendLayout();
                    foreach (UltraGridRow row in this.ultraGrid1.Rows)
                    {
                        row.Hidden = false;
                    }
                    this.ultraGrid1.ResumeLayout();

                    // Apply row limit to show only last N items
                    ApplyRowLimit(limit);
                    EnsureFirstVisibleRowActive();
                    return;
                }

                // If not a valid number, show all items (reset)
                this.ultraGrid1.SuspendLayout();
                foreach (UltraGridRow row in this.ultraGrid1.Rows)
                {
                    row.Hidden = false;
                }
                this.ultraGrid1.ResumeLayout();
                EnsureFirstVisibleRowActive();
            }
            catch { }
        }

        // Apply a limit to currently visible rows (keep only the last N visible rows)
        private void ApplyRowLimit(int limit)
        {
            try
            {
                if (this.ultraGrid1.Rows == null) return;

                // Build list of indices for currently visible rows
                List<UltraGridRow> visibleRows = new List<UltraGridRow>();
                foreach (UltraGridRow r in this.ultraGrid1.Rows)
                {
                    if (!r.Hidden) visibleRows.Add(r);
                }

                // If limit is invalid or >= visible count, show all visible rows
                if (limit <= 0 || limit >= visibleRows.Count)
                {
                    return;
                }

                // Determine rows to keep (last N visible)
                int keepStart = Math.Max(0, visibleRows.Count - limit);
                HashSet<UltraGridRow> keep = new HashSet<UltraGridRow>(visibleRows.GetRange(keepStart, visibleRows.Count - keepStart));

                // Now hide any visible rows not in keep set
                foreach (UltraGridRow r in visibleRows)
                {
                    r.Hidden = !keep.Contains(r);
                }
            }
            catch { }
        }

        // Ensure first visible row is active/selected
        private void EnsureFirstVisibleRowActive()
        {
            try
            {
                if (this.ultraGrid1.Rows != null && this.ultraGrid1.Rows.VisibleRowCount > 0)
                {
                    UltraGridRow firstVisible = this.ultraGrid1.Rows.GetRowAtVisibleIndex(0);
                    if (firstVisible != null)
                    {
                        this.ultraGrid1.ActiveRow = firstVisible;
                        this.ultraGrid1.Selected.Rows.Clear();
                        this.ultraGrid1.Selected.Rows.Add(firstVisible);
                    }
                }
            }
            catch { }
        }

        // This method is replaced by SafeSetPriceFieldForDataRow
    }
}
