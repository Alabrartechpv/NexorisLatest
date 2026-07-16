using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using Repository;

namespace PosBranch_Win.DialogBox
{
    public partial class PurcahseReturnUpdate : Form
    {
        // Make this accessible to other forms that might need to refresh it
        public UltraGrid DataGrid { get { return ultraGrid1; } }
        private Repository.TransactionRepository.PurchaseReturnRepository prRepo = new Repository.TransactionRepository.PurchaseReturnRepository();
        private DataTable purchaseReturnData;

        // Property to store the selected PR's details to pass back to parent form
        public DataTable SelectedPRDetails { get; private set; }
        public int SelectedPRNo { get; private set; }

        // Store original data table for filtering
        private DataTable originalPurchaseReturnData = null;

        // Store comboBox references
        private ComboBox comboBox1 = null;
        private ComboBox comboBox2 = null;

        public PurcahseReturnUpdate()
        {
            InitializeComponent();
            this.Load += PurcahseReturnUpdate_Load;
            this.Text = "Purchase Return History";
        }

        private void PurcahseReturnUpdate_Load(object sender, EventArgs e)
        {
            try
            {
                // Load data first
                LoadPurchaseReturnData();

                // Apply styling with black borders
                ConfigureGridLayout();

                // Ensure styling is applied by forcing a refresh
                ultraGrid1.DisplayLayout.UseFixedHeaders = true;
                ultraGrid1.Refresh();

                // Change VEndorlBl text to reflect search functionality

                // Add double-click event to grid
                ultraGrid1.DoubleClickRow += UltraGrid1_DoubleClickRow;
                ultraGrid1.DoubleClick += UltraGrid1_DoubleClick;

                // Add KeyDown event to search box and grid for navigation
                Searchbx.KeyDown += Searchbx_KeyDown;
                ultraGrid1.KeyDown += UltraGrid1_KeyDown;

                // Style panels to match ultraPanel3 from frmdialForItemMaster
                SetupPanelStyles();

                // Initialize comboBox1 and comboBox2
                InitializeSearchFilterComboBox();
                InitializeColumnOrderComboBox();

                // Connect panel click events
                ConnectPanelClickEvents();

                // Focus on search box
                this.BeginInvoke(new Action(() =>
                {
                    Searchbx.Focus();
                    Searchbx.Select();
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraGrid1_DoubleClick(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow != null)
            {
                LoadPRDetailsAndClose(ultraGrid1.ActiveRow);
            }
        }

        private void UltraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            LoadPRDetailsAndClose(e.Row);
        }

        private void LoadPRDetailsAndClose(UltraGridRow row)
        {
            try
            {
                if (row != null)
                {
                    // Get the PR number from the selected row
                    int prNo = Convert.ToInt32(row.Cells["PReturnNo"].Value);
                    SelectedPRNo = prNo;

                    // Load the PR details for this PR number
                    LoadPRDetailsForSelectedPR(prNo);

                    // Close the dialog with OK result
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading PR details: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPRDetailsForSelectedPR(int prNo)
        {
            try
            {
                // Create a BaseRepository instance to get the connection
                BaseRepostitory baseRepo = new BaseRepostitory();

                // Use the repository to get PR detail data
                using (SqlConnection conn = (SqlConnection)baseRepo.DataConnection)
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("_POS_PurchaseReturn", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@_Operation", "GetPRDetailsByPRNo");
                        cmd.Parameters.AddWithValue("@PReturnNo", prNo);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            SelectedPRDetails = new DataTable();
                            adapter.Fill(SelectedPRDetails);

                            // If the table doesn't have the expected columns, create them
                            string[] requiredColumns = new string[] {
                                    "SlNo", "SLno", "Sl No", "Description", "BarCode", "Unit", "Packing",
                                    "Cost", "Quantity", "Amount", "Reason"
                                };

                            foreach (string column in requiredColumns)
                            {
                                if (!SelectedPRDetails.Columns.Contains(column))
                                {
                                    SelectedPRDetails.Columns.Add(column);
                                }
                            }

                            // Ensure all rows have values for all variants of SlNo
                            if (SelectedPRDetails.Rows.Count > 0)
                            {
                                for (int i = 0; i < SelectedPRDetails.Rows.Count; i++)
                                {
                                    // Determine the correct SlNo value
                                    int slNoValue = i + 1;

                                    // Try to get existing value if available
                                    if (SelectedPRDetails.Rows[i]["SlNo"] != DBNull.Value)
                                        slNoValue = Convert.ToInt32(SelectedPRDetails.Rows[i]["SlNo"]);
                                    else if (SelectedPRDetails.Rows[i]["Sl No"] != DBNull.Value)
                                        slNoValue = Convert.ToInt32(SelectedPRDetails.Rows[i]["Sl No"]);
                                    else if (SelectedPRDetails.Rows[i]["SLno"] != DBNull.Value)
                                        slNoValue = Convert.ToInt32(SelectedPRDetails.Rows[i]["SLno"]);

                                    // Set the value for all variations to ensure consistency
                                    SelectedPRDetails.Rows[i]["SlNo"] = slNoValue;
                                    SelectedPRDetails.Rows[i]["SLno"] = slNoValue;
                                    SelectedPRDetails.Rows[i]["Sl No"] = slNoValue;
                                }
                            }

                            // Ensure we have ItemID and UnitId columns for proper grid functionality
                            if (!SelectedPRDetails.Columns.Contains("ItemID"))
                                SelectedPRDetails.Columns.Add("ItemID");

                            if (!SelectedPRDetails.Columns.Contains("UnitId"))
                                SelectedPRDetails.Columns.Add("UnitId");

                            // Map any differently named columns to our required format
                            if (SelectedPRDetails.Columns.Contains("ItemId") && !SelectedPRDetails.Columns.Contains("ItemID"))
                            {
                                foreach (DataRow row in SelectedPRDetails.Rows)
                                {
                                    row["ItemID"] = row["ItemId"];
                                }
                            }

                            if (SelectedPRDetails.Columns.Contains("Qty") && !SelectedPRDetails.Columns.Contains("Quantity"))
                            {
                                foreach (DataRow row in SelectedPRDetails.Rows)
                                {
                                    row["Quantity"] = row["Qty"];
                                }
                            }

                            if (SelectedPRDetails.Columns.Contains("ItemName") && !SelectedPRDetails.Columns.Contains("Description"))
                            {
                                foreach (DataRow row in SelectedPRDetails.Rows)
                                {
                                    row["Description"] = row["ItemName"];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting PR details: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Changed to public to allow external access
        public void LoadPurchaseReturnData()
        {
            try
            {
                // Create a BaseRepository instance to get the connection
                BaseRepostitory baseRepo = new BaseRepostitory();

                // Use the repository to get purchase items data
                using (System.Data.SqlClient.SqlConnection conn = (System.Data.SqlClient.SqlConnection)baseRepo.DataConnection)
                {
                    conn.Open();
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("_POS_PurchaseReturn", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@_Operation", "GETAllPurchaseReturn");

                        using (System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(cmd))
                        {
                            purchaseReturnData = new DataTable();
                            adapter.Fill(purchaseReturnData);

                            // Store original data for filtering
                            originalPurchaseReturnData = purchaseReturnData.Copy();

                            // Sort by PReturnNo descending to show newest first
                            if (purchaseReturnData.Rows.Count > 0 && purchaseReturnData.Columns.Contains("PReturnNo"))
                            {
                                purchaseReturnData.DefaultView.Sort = "PReturnNo DESC";
                                ultraGrid1.DataSource = purchaseReturnData.DefaultView.ToTable();
                            }
                            else
                            {
                                // Set the data source for the UltraGrid
                                ultraGrid1.DataSource = purchaseReturnData;
                            }

                            // Apply column ordering if comboBox2 is initialized
                            if (comboBox2 != null && comboBox2.SelectedItem != null)
                            {
                                ReorderColumns(comboBox2.SelectedItem.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading purchase return data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridLayout()
        {
            try
            {
                // Reset everything first to ensure clean slate
                ultraGrid1.DisplayLayout.Reset();

                // Configure the grid appearance
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;

                // Enable column moving and dragging
                ultraGrid1.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.AllowColSwapping = AllowColSwapping.WithinBand;

                // Important: This setting ensures we get only row selection on click, not automatic action
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

                // Remove the grid caption (header label showing "ultraGrid1")
                ultraGrid1.DisplayLayout.CaptionVisible = DefaultableBoolean.False;

                // Hide the group-by area (gray bar) - matching frmdialForItemMaster
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;

                // Set rounded borders for the entire grid
                ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;

                // Configure grid lines - single line borders for rows and columns
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

                // Set border width to single line
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderAlpha = Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderAlpha = Alpha.Opaque;

                // Remove cell padding/spacing
                ultraGrid1.DisplayLayout.Override.CellPadding = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                ultraGrid1.DisplayLayout.InterBandSpacing = 0;

                // Define colors - matching frmdialForItemMaster
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
                Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

                // Set light blue border color for cells - matching frmdialForItemMaster
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;

                // Configure row height - matching frmdialForItemMaster (30 pixels)
                ultraGrid1.DisplayLayout.Override.MinRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 30;

                // Add header styling - blue headers - matching frmdialForItemMaster
                ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue; // Same color for no gradient
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

                // Configure row selector appearance with blue - matching frmdialForItemMaster
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue; // Same color for no gradient
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None; // Remove numbers
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15; // Smaller width - matching frmdialForItemMaster

                // Set these properties to completely clean the row headers - remove all indicators
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Image = null;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Image = null;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.Image = null;

                // Set all cells to have white background (no alternate row coloring) - matching frmdialForItemMaster
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;

                // Remove alternate row appearance (make all rows white) - matching frmdialForItemMaster
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

                // Configure selected row appearance - matching frmdialForItemMaster
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215); // Bright blue
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White; // White text for better contrast
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;

                // Configure active row appearance - make it same as selected row - matching frmdialForItemMaster
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.True;

                // Set font size for all cells - matching frmdialForItemMaster
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Configure scrollbar style - matching frmdialForItemMaster
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;

                // Configure the scrollbar look - matching frmdialForItemMaster
                if (ultraGrid1.DisplayLayout.ScrollBarLook != null)
                {
                    // Configure button appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;

                    // Configure track appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;

                    // Configure thumb appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }

                // Configure cell appearance to increase vertical content alignment - matching frmdialForItemMaster
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = HAlign.Center;

                // Auto-fit settings - DISABLE to prevent column resizing during filtering
                ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.None;

                // Disable automatic column sizing
                ultraGrid1.DisplayLayout.Override.ColumnAutoSizeMode = ColumnAutoSizeMode.None;

                // Disable filter indicators - matching frmdialForItemMaster
                ultraGrid1.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.FilterUIType = FilterUIType.FilterRow;
                ultraGrid1.DisplayLayout.Override.WrapHeaderText = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextTrimming = TextTrimming.None;

                // REMOVE pin icons/expansion indicators from headers
                ultraGrid1.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;
                ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.Fixed;
                ultraGrid1.DisplayLayout.UseFixedHeaders = true;

                // Configure columns if bands exist
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                    // Disable outlining/grouping features for clean row headers
                    band.Indentation = 0;
                    band.RowLayoutStyle = RowLayoutStyle.ColumnLayout;

                    // Define column order
                    string[] visibleColumns = new string[] {
                        "PReturnNo", "PReturnDate", "InvoiceNo", "InvoiceDate", "VendorName", "Paymode", "SubTotal", "GrandTotal"
                    };

                    // Configure columns
                    foreach (string columnName in visibleColumns)
                    {
                        if (band.Columns.Exists(columnName))
                        {
                            UltraGridColumn column = band.Columns[columnName];
                            column.Hidden = false;
                            column.CellActivation = Activation.NoEdit;
                            column.CellAppearance.TextVAlign = VAlign.Middle;

                            // Set light blue borders for all cells and headers - matching frmdialForItemMaster
                            column.CellAppearance.BorderColor = lightBlue;
                            column.Header.Appearance.BorderColor = headerBlue;

                            // Header styling - matching frmdialForItemMaster
                            column.Header.Appearance.BackColor = headerBlue;
                            column.Header.Appearance.BackColor2 = headerBlue;
                            column.Header.Appearance.BackGradientStyle = GradientStyle.None;
                            column.Header.Appearance.ForeColor = Color.White;
                            column.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                            column.Header.Appearance.TextHAlign = HAlign.Center;
                            column.Header.Appearance.TextVAlign = VAlign.Middle;
                            column.Header.Appearance.FontData.SizeInPoints = 9;
                            column.Header.Appearance.FontData.Name = "Microsoft Sans Serif";

                            // Set formats and alignment for specific columns
                            if (columnName == "PReturnNo" || columnName == "InvoiceNo" || columnName == "Paymode")
                            {
                                column.CellAppearance.TextHAlign = HAlign.Center;
                            }
                            else if (columnName == "PReturnDate" || columnName == "InvoiceDate")
                            {
                                column.Format = "dd-MMM-yyyy";
                                column.CellAppearance.TextHAlign = HAlign.Center;
                            }
                            else if (columnName == "SubTotal" || columnName == "GrandTotal")
                            {
                                column.Format = "N2";
                                column.CellAppearance.TextHAlign = HAlign.Right;
                            }
                            else if (columnName == "VendorName")
                            {
                                column.CellAppearance.TextHAlign = HAlign.Left;
                            }
                        }
                    }

                    // Apply styling to band level
                    band.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                    band.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                    band.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                    band.Override.HeaderAppearance.BackColor = headerBlue;
                    band.Override.HeaderAppearance.BackColor2 = headerBlue;
                    band.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
                    band.Override.HeaderAppearance.ForeColor = Color.White;
                    band.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Override.HeaderAppearance.BorderColor = headerBlue;
                    band.Override.CellAppearance.BorderColor = lightBlue;
                    band.Override.RowAppearance.BorderColor = lightBlue;
                }

                // Refresh the grid to apply all changes
                ultraGrid1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up grid style: {ex.Message}");
            }
        }

        private void Searchbx_KeyDown(object sender, KeyEventArgs e)
        {
            // If down arrow key is pressed, move focus to the grid
            if (e.KeyCode == Keys.Down)
            {
                e.Handled = true;

                // Set focus to the UltraGrid
                ultraGrid1.Focus();

                // If grid has rows, select the first row
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
            }
            // If Enter key is pressed, load the selected PR details
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;

                // If grid has rows and a row is selected, load that PR
                if (ultraGrid1.Rows.Count > 0)
                {
                    if (ultraGrid1.ActiveRow == null && ultraGrid1.Rows.Count > 0)
                    {
                        // If no row is active yet, select the first one
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                    }

                    if (ultraGrid1.ActiveRow != null)
                    {
                        // Load the selected PR details and close
                        LoadPRDetailsAndClose(ultraGrid1.ActiveRow);
                    }
                }
            }
        }

        private void Searchbx_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string searchText = Searchbx.Text.ToLower().Trim();

                if (originalPurchaseReturnData == null)
                    return;

                // Store the current column widths and positions before changing the data source
                Dictionary<string, int> columnWidths = new Dictionary<string, int>();
                Dictionary<string, int> columnPositions = new Dictionary<string, int>();

                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        if (!col.Hidden)
                        {
                            columnWidths[col.Key] = col.Width;
                            columnPositions[col.Key] = col.Header.VisiblePosition;
                        }
                    }
                }

                // Create a new DataTable with the same schema
                DataTable filteredDataTable = originalPurchaseReturnData.Clone();

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // If search box is empty, show all data sorted by PReturnNo descending
                    if (originalPurchaseReturnData.Rows.Count > 0 && originalPurchaseReturnData.Columns.Contains("PReturnNo"))
                    {
                        DataView dv = new DataView(originalPurchaseReturnData);
                        dv.Sort = "PReturnNo DESC";
                        ultraGrid1.DataSource = dv.ToTable();
                    }
                    else
                    {
                        ultraGrid1.DataSource = originalPurchaseReturnData;
                    }

                    // Show all columns again
                    SetColumnVisibility();
                    return;
                }

                // Get the selected search filter option
                string filterOption = "Select all";
                if (comboBox1 != null && comboBox1.SelectedItem != null)
                {
                    filterOption = comboBox1.SelectedItem.ToString();
                }

                // Build the filter function based on selected search field
                Func<DataRow, bool> filterFunc = row =>
                {
                    switch (filterOption)
                    {
                        case "PR No":
                            // Search only in PReturnNo column
                            return row["PReturnNo"].ToString().ToLower().Contains(searchText);

                        case "Vendor Name":
                            // Search only in VendorName column
                            return row["VendorName"].ToString().ToLower().Contains(searchText);

                        case "Select all":
                        default:
                            // Search in all relevant columns
                            return row["PReturnNo"].ToString().ToLower().Contains(searchText) ||
                                   row["VendorName"].ToString().ToLower().Contains(searchText);
                    }
                };

                // Apply the filter to get matching rows
                var matchingRows = originalPurchaseReturnData.AsEnumerable().Where(filterFunc).ToList();

                // Get current sort field from comboBox2
                string sortField = "PReturnNo";
                if (comboBox2 != null && comboBox2.SelectedItem != null)
                {
                    string selectedOption = comboBox2.SelectedItem.ToString();
                    if (selectedOption == "PR No")
                        sortField = "PReturnNo";
                    else if (selectedOption == "Vendor Name")
                        sortField = "VendorName";
                    else if (selectedOption == "PReturnDate")
                        sortField = "PReturnDate";
                    else if (selectedOption == "InvoiceNo")
                        sortField = "InvoiceNo";
                }

                // Sort the matching rows based on the current sort field
                var sortedMatchingRows = sortField == "PReturnNo"
                    ? matchingRows.OrderByDescending(r => Convert.ToInt32(r[sortField])).ToList()
                    : matchingRows.OrderBy(r => r[sortField].ToString()).ToList();

                // Add the filtered rows to the new DataTable
                foreach (var row in sortedMatchingRows)
                {
                    filteredDataTable.ImportRow(row);
                }

                // Set the filtered DataTable as the DataSource
                ultraGrid1.DataSource = filteredDataTable;

                // Restore column widths and positions
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        if (!col.Hidden && columnWidths.ContainsKey(col.Key))
                        {
                            col.Width = columnWidths[col.Key];
                            if (columnPositions.ContainsKey(col.Key))
                            {
                                col.Header.VisiblePosition = columnPositions[col.Key];
                            }
                        }
                    }
                }

                // Re-apply column styling to ensure consistent appearance
                ConfigureGridLayout();

                // Re-apply column ordering
                if (comboBox2 != null && comboBox2.SelectedItem != null)
                {
                    ReorderColumns(comboBox2.SelectedItem.ToString());
                }

                // Select first visible row if any
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                // Don't show message box for filter errors as this happens during typing
                System.Diagnostics.Debug.WriteLine("Error filtering data: " + ex.Message);
            }
        }

        // Helper method to hide the ItemID column
        private void HideItemIdColumn()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                if (band.Columns.Exists("ItemID"))
                {
                    band.Columns["ItemID"].Hidden = true;
                }
            }
        }

        // Helper method to restore column visibility
        private void SetColumnVisibility()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Define column order and visibility
                string[] visibleColumns = new string[] {
                        "PReturnNo", "PReturnDate", "InvoiceNo", "InvoiceDate", "VendorName", "Paymode", "SubTotal", "GrandTotal"
                    };

                // Set visibility for each column
                foreach (UltraGridColumn column in band.Columns)
                {
                    if (visibleColumns.Contains(column.Key))
                    {
                        column.Hidden = false;
                    }
                    else
                    {
                        column.Hidden = true;
                    }
                }
            }
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Apply proper grid line styles - matching frmdialForItemMaster
                e.Layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

                // Set border style for the main grid
                e.Layout.BorderStyle = UIElementBorderStyle.Solid;

                // Remove cell padding/spacing - matching frmdialForItemMaster
                e.Layout.Override.CellPadding = 0;
                e.Layout.Override.RowSpacingBefore = 0;
                e.Layout.Override.RowSpacingAfter = 0;
                e.Layout.Override.CellSpacing = 0;
                e.Layout.InterBandSpacing = 0;

                // Configure row height to match frmdialForItemMaster (30 pixels)
                e.Layout.Override.MinRowHeight = 30;
                e.Layout.Override.DefaultRowHeight = 30;

                // Define colors - matching frmdialForItemMaster
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
                Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

                // Set default alignment for all cells - matching frmdialForItemMaster
                e.Layout.Override.CellAppearance.TextHAlign = HAlign.Center;
                e.Layout.Override.CellAppearance.TextVAlign = VAlign.Middle;

                // Set font size for all cells - matching frmdialForItemMaster
                e.Layout.Override.CellAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.RowAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                e.Layout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Remove the grid caption (header) - matching frmVendorDig
                e.Layout.CaptionVisible = DefaultableBoolean.False;
                e.Layout.ViewStyle = ViewStyle.SingleBand;

                // Hide the group-by area (gray bar) - matching frmdialForItemMaster
                e.Layout.GroupByBox.Hidden = true;
                e.Layout.GroupByBox.Prompt = string.Empty;

                // Apply row selector appearance with blue - matching frmdialForItemMaster
                e.Layout.Override.RowSelectorAppearance.BackColor = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.None;
                e.Layout.Override.RowSelectorAppearance.ForeColor = Color.White;
                e.Layout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
                e.Layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None;
                e.Layout.Override.RowSelectorWidth = 15; // Smaller width - matching frmdialForItemMaster

                // Set these properties to completely clean the row headers - remove all indicators
                e.Layout.Override.ActiveRowAppearance.Image = null;
                e.Layout.Override.SelectedRowAppearance.Image = null;
                e.Layout.Override.RowSelectorAppearance.Image = null;

                // Set all cells to white background - matching frmdialForItemMaster
                e.Layout.Override.RowAppearance.BackColor = Color.White;
                e.Layout.Override.RowAppearance.BackColor2 = Color.White;
                e.Layout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;

                // Remove alternate row appearance (make all rows white) - matching frmdialForItemMaster
                e.Layout.Override.RowAlternateAppearance.BackColor = Color.White;
                e.Layout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                e.Layout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

                // Configure selected row appearance - matching frmdialForItemMaster
                e.Layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215); // Bright blue
                e.Layout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                e.Layout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                e.Layout.Override.SelectedRowAppearance.ForeColor = Color.White; // White text for better contrast
                e.Layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;

                // Active row appearance - make it same as selected row - matching frmdialForItemMaster
                e.Layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                e.Layout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                e.Layout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                e.Layout.Override.ActiveRowAppearance.ForeColor = Color.White;
                e.Layout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.True;

                // Configure scrollbar style - matching frmdialForItemMaster
                e.Layout.ScrollBounds = ScrollBounds.ScrollToFill;
                e.Layout.ScrollStyle = ScrollStyle.Immediate;

                // Configure the scrollbar look - matching frmdialForItemMaster
                if (e.Layout.ScrollBarLook != null)
                {
                    // Style track and buttons with blue colors
                    e.Layout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    e.Layout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    e.Layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
                    e.Layout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    e.Layout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    e.Layout.ScrollBarLook.TrackAppearance.BackGradientStyle = GradientStyle.None;
                    e.Layout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }

                // Disable filter indicators - matching frmdialForItemMaster
                e.Layout.Override.AllowRowFiltering = DefaultableBoolean.False;
                e.Layout.Override.FilterUIType = FilterUIType.FilterRow;
                e.Layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                e.Layout.Override.WrapHeaderText = DefaultableBoolean.False;
                e.Layout.Override.HeaderAppearance.TextTrimming = TextTrimming.None;

                // Configure header style - matching frmdialForItemMaster
                e.Layout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
                e.Layout.Override.HeaderAppearance.BackColor = headerBlue;
                e.Layout.Override.HeaderAppearance.BackColor2 = headerBlue;
                e.Layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
                e.Layout.Override.HeaderAppearance.ForeColor = Color.White;
                e.Layout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                e.Layout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
                e.Layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                e.Layout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                e.Layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

                // Set light blue border colors - matching frmdialForItemMaster
                e.Layout.Override.CellAppearance.BorderColor = lightBlue;
                e.Layout.Override.RowAppearance.BorderColor = lightBlue;
                e.Layout.Override.HeaderAppearance.BorderColor = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BorderColor = headerBlue;

                if (e.Layout.Bands.Count > 0)
                {
                    UltraGridBand band = e.Layout.Bands[0];

                    // Disable outlining/grouping features for clean row headers
                    band.Indentation = 0;
                    band.RowLayoutStyle = RowLayoutStyle.ColumnLayout;

                    // Apply header styling at band level - matching frmdialForItemMaster
                    band.Override.HeaderAppearance.BackColor = headerBlue;
                    band.Override.HeaderAppearance.BackColor2 = headerBlue;
                    band.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
                    band.Override.HeaderAppearance.ForeColor = Color.White;
                    band.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Override.HeaderAppearance.BorderColor = headerBlue;

                    // Apply border styles at band level - matching frmdialForItemMaster
                    band.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                    band.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                    band.Override.BorderStyleRow = UIElementBorderStyle.Solid;

                    // Set border colors at band level - matching frmdialForItemMaster
                    band.Override.CellAppearance.BorderColor = lightBlue;
                    band.Override.RowAppearance.BorderColor = lightBlue;
                    band.Override.HeaderAppearance.BorderColor = headerBlue;

                    // Set date formats for date columns
                    if (band.Columns.Exists("PReturnDate"))
                    {
                        band.Columns["PReturnDate"].Format = "dd-MMM-yyyy";
                        band.Columns["PReturnDate"].Header.Appearance.BorderColor = headerBlue;
                        band.Columns["PReturnDate"].CellAppearance.BorderColor = lightBlue;
                    }
                    if (band.Columns.Exists("InvoiceDate"))
                    {
                        band.Columns["InvoiceDate"].Format = "dd-MMM-yyyy";
                        band.Columns["InvoiceDate"].Header.Appearance.BorderColor = headerBlue;
                        band.Columns["InvoiceDate"].CellAppearance.BorderColor = lightBlue;
                    }

                    // Set number formats for numeric columns
                    if (band.Columns.Exists("SubTotal"))
                    {
                        band.Columns["SubTotal"].Format = "N2";
                        band.Columns["SubTotal"].CellAppearance.TextHAlign = HAlign.Right;
                        band.Columns["SubTotal"].Header.Appearance.BorderColor = headerBlue;
                        band.Columns["SubTotal"].CellAppearance.BorderColor = lightBlue;
                    }
                    if (band.Columns.Exists("GrandTotal"))
                    {
                        band.Columns["GrandTotal"].Format = "N2";
                        band.Columns["GrandTotal"].CellAppearance.TextHAlign = HAlign.Right;
                        band.Columns["GrandTotal"].Header.Appearance.BorderColor = headerBlue;
                        band.Columns["GrandTotal"].CellAppearance.BorderColor = lightBlue;
                    }

                    // Apply light blue borders to all columns - matching frmdialForItemMaster
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        col.Header.Appearance.BorderColor = headerBlue;
                        col.CellAppearance.BorderColor = lightBlue;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in InitializeLayout: " + ex.Message);
            }
        }

        // Add a KeyDown handler for the UltraGrid
        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            // If Enter key is pressed while a row is selected, load the PR details
            if (e.KeyCode == Keys.Enter && ultraGrid1.ActiveRow != null)
            {
                e.Handled = true;

                // Load the selected PR details and close - same behavior as double-click
                LoadPRDetailsAndClose(ultraGrid1.ActiveRow);
            }
        }

        // Public method to refresh the purchase return data
        public void RefreshData()
        {
            try
            {
                LoadPurchaseReturnData();

                // Re-apply column ordering if comboBox2 is set
                if (comboBox2 != null && comboBox2.SelectedItem != null)
                {
                    ReorderColumns(comboBox2.SelectedItem.ToString());
                }

                System.Diagnostics.Debug.WriteLine("PurchaseReturnUpdate data refreshed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing data: {ex.Message}");
            }
        }

        // Public method to remove a specific PR from the grid (kept for compatibility)
        public void RemovePurchaseReturnById(int prNoToRemove)
        {
            RefreshData(); // Call the safer refresh method
        }

        // Setup panel styles to match ultraPanel3 from frmdialForItemMaster
        private void SetupPanelStyles()
        {
            try
            {
                // Style ultraPanel4 (exists in Designer)
                if (ultraPanel4 != null)
                {
                    StyleIconPanel(ultraPanel4);
                }

                // Style ultraPanel5 (if exists)
                var ultraPanel5 = this.Controls.Find("ultraPanel5", true).FirstOrDefault() as Infragistics.Win.Misc.UltraPanel;
                if (ultraPanel5 != null)
                {
                    StyleIconPanel(ultraPanel5);
                }

                // Style ultraPanel11 (if exists)
                var ultraPanel11 = this.Controls.Find("ultraPanel11", true).FirstOrDefault() as Infragistics.Win.Misc.UltraPanel;
                if (ultraPanel11 != null)
                {
                    StyleIconPanel(ultraPanel11);
                }

                // Style ultraPanel12 (if exists)
                var ultraPanel12 = this.Controls.Find("ultraPanel12", true).FirstOrDefault() as Infragistics.Win.Misc.UltraPanel;
                if (ultraPanel12 != null)
                {
                    StyleIconPanel(ultraPanel12);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up panel styles: {ex.Message}");
            }
        }

        // Style icon panel to match ultraPanel3 from frmdialForItemMaster
        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            if (panel == null) return;

            try
            {
                // Define consistent colors for all panels - matching frmdialForItemMaster
                Color lightBlue = Color.FromArgb(127, 219, 255); // Light blue
                Color darkBlue = Color.FromArgb(0, 116, 217);    // Darker blue
                Color borderBlue = Color.FromArgb(0, 150, 220);  // Border blue
                Color borderBase = Color.FromArgb(0, 100, 180);  // Border base color

                // Create a gradient from light to dark blue with exact specified colors
                panel.Appearance.BackColor = lightBlue;
                panel.Appearance.BackColor2 = darkBlue;
                panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;

                // Set highly rounded border style
                panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;

                // Add exact specified border color
                panel.Appearance.BorderColor = borderBlue;
                panel.Appearance.BorderColor3DBase = borderBase;

                // Ensure icons inside have transparent background
                foreach (Control control in panel.ClientArea.Controls)
                {
                    if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                    {
                        Infragistics.Win.UltraWinEditors.UltraPictureBox pic = (Infragistics.Win.UltraWinEditors.UltraPictureBox)control;
                        pic.BackColor = Color.Transparent;
                        pic.BackColorInternal = Color.Transparent;
                        pic.BorderShadowColor = Color.Transparent;
                    }
                    else if (control is Label)
                    {
                        ((Label)control).ForeColor = Color.White;
                        ((Label)control).Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
                        ((Label)control).BackColor = Color.Transparent;
                    }
                }

                // Add hover effect with consistent colors
                Color originalBackColor = lightBlue;
                Color originalBackColor2 = darkBlue;

                panel.ClientArea.MouseEnter += (sender, e) =>
                {
                    panel.Appearance.BackColor = Color.FromArgb(160, 230, 255);
                    panel.Appearance.BackColor2 = Color.FromArgb(30, 140, 230);
                };


                panel.ClientArea.MouseLeave += (sender, e) =>
                {
                    panel.Appearance.BackColor = originalBackColor;
                    panel.Appearance.BackColor2 = originalBackColor2;
                };


                // Set cursor to hand to indicate clickable
                panel.ClientArea.Cursor = Cursors.Hand;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error styling panel {panel.Name}: {ex.Message}");
            }
        }

        // Initialize comboBox1 with search filter options
        private void InitializeSearchFilterComboBox()
        {
            // Find comboBox1 if it exists
            comboBox1 = this.Controls.Find("comboBox1", true).FirstOrDefault() as ComboBox;
            if (comboBox1 == null) return;

            // Clear existing items
            comboBox1.Items.Clear();

            // Add search options for Purchase Return
            comboBox1.Items.Add("Select all");
            comboBox1.Items.Add("PR No");
            comboBox1.Items.Add("Vendor Name");

            // Set default selection
            comboBox1.SelectedIndex = 0;

            // Add event handler for selection change
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        // Handle selection change in comboBox1
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reapply search with current filter if there's text in the search box
            if (!string.IsNullOrWhiteSpace(Searchbx.Text))
            {
                Searchbx_TextChanged(Searchbx, EventArgs.Empty);
            }
        }

        // Initialize comboBox2 with column order options
        private void InitializeColumnOrderComboBox()
        {
            // Find comboBox2 if it exists
            comboBox2 = this.Controls.Find("comboBox2", true).FirstOrDefault() as ComboBox;
            if (comboBox2 == null) return;

            // Clear existing items
            comboBox2.Items.Clear();

            // Add column order options for Purchase Return
            comboBox2.Items.Add("PR No");
            comboBox2.Items.Add("Vendor Name");
            comboBox2.Items.Add("PReturnDate");
            comboBox2.Items.Add("InvoiceNo");

            // Set default selection to PR No
            comboBox2.SelectedIndex = 0;

            // Add event handler for selection change
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        // Handle selection change in comboBox2
        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reorder columns based on selection
            if (comboBox2 != null && comboBox2.SelectedItem != null)
            {
                ReorderColumns(comboBox2.SelectedItem.ToString());
            }
        }

        // Method to reorder columns based on selection
        private void ReorderColumns(string selectedOption)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

            // Calculate total available width
            int totalWidth = ultraGrid1.Width - ultraGrid1.DisplayLayout.Override.RowSelectorWidth - 20; // 20 for scrollbar and borders

            // Reorder columns based on selection
            switch (selectedOption)
            {
                case "PR No":
                    // PReturnNo first
                    if (band.Columns.Exists("PReturnNo"))
                    {
                        band.Columns["PReturnNo"].Header.VisiblePosition = 0;
                    }
                    if (band.Columns.Exists("PReturnDate"))
                    {
                        band.Columns["PReturnDate"].Header.VisiblePosition = 1;
                    }
                    if (band.Columns.Exists("InvoiceNo"))
                    {
                        band.Columns["InvoiceNo"].Header.VisiblePosition = 2;
                    }
                    if (band.Columns.Exists("InvoiceDate"))
                    {
                        band.Columns["InvoiceDate"].Header.VisiblePosition = 3;
                    }
                    if (band.Columns.Exists("VendorName"))
                    {
                        band.Columns["VendorName"].Header.VisiblePosition = 4;
                    }
                    if (band.Columns.Exists("Paymode"))
                    {
                        band.Columns["Paymode"].Header.VisiblePosition = 5;
                    }
                    if (band.Columns.Exists("SubTotal"))
                    {
                        band.Columns["SubTotal"].Header.VisiblePosition = 6;
                    }
                    if (band.Columns.Exists("GrandTotal"))
                    {
                        band.Columns["GrandTotal"].Header.VisiblePosition = 7;
                    }
                    break;

                case "Vendor Name":
                    // VendorName first
                    if (band.Columns.Exists("VendorName"))
                    {
                        band.Columns["VendorName"].Header.VisiblePosition = 0;
                    }
                    if (band.Columns.Exists("PReturnNo"))
                    {
                        band.Columns["PReturnNo"].Header.VisiblePosition = 1;
                    }
                    if (band.Columns.Exists("PReturnDate"))
                    {
                        band.Columns["PReturnDate"].Header.VisiblePosition = 2;
                    }
                    if (band.Columns.Exists("InvoiceNo"))
                    {
                        band.Columns["InvoiceNo"].Header.VisiblePosition = 3;
                    }
                    if (band.Columns.Exists("InvoiceDate"))
                    {
                        band.Columns["InvoiceDate"].Header.VisiblePosition = 4;
                    }
                    if (band.Columns.Exists("Paymode"))
                    {
                        band.Columns["Paymode"].Header.VisiblePosition = 5;
                    }
                    if (band.Columns.Exists("SubTotal"))
                    {
                        band.Columns["SubTotal"].Header.VisiblePosition = 6;
                    }
                    if (band.Columns.Exists("GrandTotal"))
                    {
                        band.Columns["GrandTotal"].Header.VisiblePosition = 7;
                    }
                    break;

                case "PReturnDate":
                    // PReturnDate first
                    if (band.Columns.Exists("PReturnDate"))
                    {
                        band.Columns["PReturnDate"].Header.VisiblePosition = 0;
                    }
                    if (band.Columns.Exists("PReturnNo"))
                    {
                        band.Columns["PReturnNo"].Header.VisiblePosition = 1;
                    }
                    if (band.Columns.Exists("VendorName"))
                    {
                        band.Columns["VendorName"].Header.VisiblePosition = 2;
                    }
                    if (band.Columns.Exists("InvoiceNo"))
                    {
                        band.Columns["InvoiceNo"].Header.VisiblePosition = 3;
                    }
                    if (band.Columns.Exists("InvoiceDate"))
                    {
                        band.Columns["InvoiceDate"].Header.VisiblePosition = 4;
                    }
                    if (band.Columns.Exists("Paymode"))
                    {
                        band.Columns["Paymode"].Header.VisiblePosition = 5;
                    }
                    if (band.Columns.Exists("SubTotal"))
                    {
                        band.Columns["SubTotal"].Header.VisiblePosition = 6;
                    }
                    if (band.Columns.Exists("GrandTotal"))
                    {
                        band.Columns["GrandTotal"].Header.VisiblePosition = 7;
                    }
                    break;

                case "InvoiceNo":
                    // InvoiceNo first
                    if (band.Columns.Exists("InvoiceNo"))
                    {
                        band.Columns["InvoiceNo"].Header.VisiblePosition = 0;
                    }
                    if (band.Columns.Exists("InvoiceDate"))
                    {
                        band.Columns["InvoiceDate"].Header.VisiblePosition = 1;
                    }
                    if (band.Columns.Exists("PReturnNo"))
                    {
                        band.Columns["PReturnNo"].Header.VisiblePosition = 2;
                    }
                    if (band.Columns.Exists("PReturnDate"))
                    {
                        band.Columns["PReturnDate"].Header.VisiblePosition = 3;
                    }
                    if (band.Columns.Exists("VendorName"))
                    {
                        band.Columns["VendorName"].Header.VisiblePosition = 4;
                    }
                    if (band.Columns.Exists("Paymode"))
                    {
                        band.Columns["Paymode"].Header.VisiblePosition = 5;
                    }
                    if (band.Columns.Exists("SubTotal"))
                    {
                        band.Columns["SubTotal"].Header.VisiblePosition = 6;
                    }
                    if (band.Columns.Exists("GrandTotal"))
                    {
                        band.Columns["GrandTotal"].Header.VisiblePosition = 7;
                    }
                    break;
            }

            // Force the layout to update
            ultraGrid1.Refresh();
        }

        // Connect panel click events
        private void ConnectPanelClickEvents()
        {
            // Connect ultraPanel12 as OK button
            var ultraPanel12 = this.Controls.Find("ultraPanel12", true).FirstOrDefault() as Infragistics.Win.Misc.UltraPanel;
            if (ultraPanel12 != null)
            {
                ultraPanel12.Click += UltraPanel12_Click;
                ultraPanel12.ClientArea.Click += UltraPanel12_Click;

                // Connect click events for child controls
                foreach (Control control in ultraPanel12.ClientArea.Controls)
                {
                    if (control is Label || control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                    {
                        control.Click += UltraPanel12_Click;
                    }
                }
            }

            // Connect ultraPanel11 as Close button
            var ultraPanel11 = this.Controls.Find("ultraPanel11", true).FirstOrDefault() as Infragistics.Win.Misc.UltraPanel;
            if (ultraPanel11 != null)
            {
                ultraPanel11.Click += UltraPanel11_Click;
                ultraPanel11.ClientArea.Click += UltraPanel11_Click;

                // Connect click events for child controls
                foreach (Control control in ultraPanel11.ClientArea.Controls)
                {
                    if (control is Label || control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                    {
                        control.Click += UltraPanel11_Click;
                    }
                }
            }
        }

        // Event handler for ultraPanel12 (OK button)
        private void UltraPanel12_Click(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    // Load the selected PR details and close with OK
                    LoadPRDetailsAndClose(ultraGrid1.ActiveRow);
                }
                else
                {
                    MessageBox.Show("Please select a Purchase Return first.", "Selection Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event handler for ultraPanel11 (Close button)
        private void UltraPanel11_Click(object sender, EventArgs e)
        {
            try
            {
                // Close the form without selecting anything
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Override ProcessCmdKey to handle Escape key
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
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
    }
}
