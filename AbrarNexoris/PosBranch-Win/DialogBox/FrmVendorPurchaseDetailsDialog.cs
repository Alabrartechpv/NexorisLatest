using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModelClass;
using ModelClass.Master;
using Repository;
using Repository.MasterRepositry;
using Repository.TransactionRepository;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.DialogBox
{
    public partial class FrmVendorPurchaseDetailsDialog : Form
    {
        private PurchaseReturnRepository prRepo;
        private Dropdowns drop;
        private int selectedVendorId;

        // Add properties to store selected purchase information
        public int SelectedPurchaseNo { get; private set; }
        public DateTime SelectedPurchaseDate { get; private set; }
        public string SelectedInvoiceNo { get; private set; }
        public DateTime SelectedInvoiceDate { get; private set; }
        public decimal SelectedGrandTotal { get; private set; }

        // Store original data for filtering
        private DataTable originalPurchaseData;

        public FrmVendorPurchaseDetailsDialog(int vendorId, string vendorName)
        {
            InitializeComponent();

            // Store the vendor ID
            this.selectedVendorId = vendorId;

            // Set the form title to include vendor name
            this.Text = $"Purchase Details for {vendorName}";

            // Initialize repositories
            prRepo = new PurchaseReturnRepository();
            drop = new Dropdowns();

            // Initialize grid and load data
            InitializeUltraGrid();
            LoadPurchaseGrid(vendorId);

            // Initialize combo boxes for filtering and column ordering
            InitializeSearchFilterComboBox();
            InitializeColumnOrderComboBox();

            // Set up search box event
            txtSearch.TextChanged += txtSearch_TextChanged;
            txtSearch.KeyDown += txtSearch_KeyDown;

            // Set up grid events
            ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
            ultraGrid1.KeyDown += ultraGrid1_KeyDown;
            ultraGrid1.DoubleClickRow += ultraGrid1_DoubleClickRow;

            // Set up panel styles (for any panels that need button styling)
            SetupPanelStyles();

            // Connect panel click events for OK/Close buttons
            ConnectPanelClickEvents();
        }

        private void InitializeUltraGrid()
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

                // Remove the grid caption (header) - matching frmVendorDig
                ultraGrid1.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.ViewStyle = ViewStyle.SingleBand;

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

        private void LoadPurchaseGrid(int vendorId)
        {
            try
            {
                // Show loading cursor
                Cursor.Current = Cursors.WaitCursor;

                // Get purchase data for the selected vendor
                DataTable purchaseData = GetVendorPurchases(vendorId);

                if (purchaseData != null && purchaseData.Rows.Count > 0)
                {
                    // Store original data for filtering
                    originalPurchaseData = purchaseData.Copy();

                    // Set the data source
                    ultraGrid1.DataSource = purchaseData;

                    // Configure columns and layout
                    ConfigureGridLayout();

                    // Apply column ordering if comboBox2 is initialized
                    var comboBox2 = GetComboBox2();
                    if (comboBox2 != null && comboBox2.SelectedItem != null)
                    {
                        ReorderColumns(comboBox2.SelectedItem.ToString());
                    }

                    // Force a refresh to ensure all styling is applied
                    ultraGrid1.DisplayLayout.UseFixedHeaders = true; // Reset the header display
                    ultraGrid1.Refresh();

                    // Set the active row to the first row
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    }
                }
                else
                {
                    // If no purchases found, show a message
                    MessageBox.Show("No purchase records found for this vendor.",
                        "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading purchase data: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset cursor
                Cursor.Current = Cursors.Default;
            }
        }

        private DataTable GetVendorPurchases(int vendorId)
        {
            DataTable dt = new DataTable();

            try
            {
                // Create a BaseRepostitory instance to get the connection
                BaseRepostitory baseRepo = new BaseRepostitory();

                // Use the repository to get purchase data for the vendor
                using (System.Data.SqlClient.SqlConnection conn = (System.Data.SqlClient.SqlConnection)baseRepo.DataConnection)
                {
                    conn.Open();
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("_POS_PurchaseReturn", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@LedgerID", vendorId);
                        cmd.Parameters.AddWithValue("@_Operation", "DDlVendor");

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

        private void ConfigureGridLayout()
        {
            try
            {
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                    // Define colors - matching frmdialForItemMaster
                    Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
                    Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

                    // Apply light blue borders at band level - matching frmdialForItemMaster
                    band.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                    band.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                    band.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                    band.Override.RowAppearance.BorderColor = lightBlue;
                    band.Override.CellAppearance.BorderColor = lightBlue;
                    band.Override.HeaderAppearance.BorderColor = headerBlue;

                    // Configure columns
                    if (band.Columns.Exists("PurchaseNo"))
                    {
                        band.Columns["PurchaseNo"].Header.Caption = "Purchase No";
                        band.Columns["PurchaseNo"].Width = 100;
                        band.Columns["PurchaseNo"].CellAppearance.TextHAlign = HAlign.Center;
                        band.Columns["PurchaseNo"].CellAppearance.BorderColor = lightBlue;
                        band.Columns["PurchaseNo"].Header.Appearance.BorderColor = headerBlue;
                    }

                    if (band.Columns.Exists("PurchaseDate"))
                    {
                        band.Columns["PurchaseDate"].Header.Caption = "Purchase Date";
                        band.Columns["PurchaseDate"].Width = 120;
                        band.Columns["PurchaseDate"].CellAppearance.TextHAlign = HAlign.Center;
                        band.Columns["PurchaseDate"].Format = "dd-MMM-yyyy";
                        band.Columns["PurchaseDate"].CellAppearance.BorderColor = lightBlue;
                        band.Columns["PurchaseDate"].Header.Appearance.BorderColor = headerBlue;
                    }

                    if (band.Columns.Exists("InvoiceNo"))
                    {
                        band.Columns["InvoiceNo"].Header.Caption = "Invoice No";
                        band.Columns["InvoiceNo"].Width = 120;
                        band.Columns["InvoiceNo"].CellAppearance.TextHAlign = HAlign.Left;
                        band.Columns["InvoiceNo"].CellAppearance.BorderColor = lightBlue;
                        band.Columns["InvoiceNo"].Header.Appearance.BorderColor = headerBlue;
                    }

                    if (band.Columns.Exists("InvoiceDate"))
                    {
                        band.Columns["InvoiceDate"].Header.Caption = "Invoice Date";
                        band.Columns["InvoiceDate"].Width = 120;
                        band.Columns["InvoiceDate"].CellAppearance.TextHAlign = HAlign.Center;
                        band.Columns["InvoiceDate"].Format = "dd-MMM-yyyy";
                        band.Columns["InvoiceDate"].CellAppearance.BorderColor = lightBlue;
                        band.Columns["InvoiceDate"].Header.Appearance.BorderColor = headerBlue;
                    }

                    if (band.Columns.Exists("GrandTotal"))
                    {
                        band.Columns["GrandTotal"].Header.Caption = "Grand Total";
                        band.Columns["GrandTotal"].Width = 120;
                        band.Columns["GrandTotal"].CellAppearance.TextHAlign = HAlign.Right;
                        band.Columns["GrandTotal"].Format = "N2";
                        band.Columns["GrandTotal"].CellAppearance.BorderColor = lightBlue;
                        band.Columns["GrandTotal"].Header.Appearance.BorderColor = headerBlue;
                    }

                    // Hide unnecessary columns
                    if (band.Columns.Exists("LedgerID"))
                    {
                        band.Columns["LedgerID"].Hidden = true;
                    }

                    if (band.Columns.Exists("VendorName"))
                    {
                        band.Columns["VendorName"].Hidden = true;
                    }

                    if (band.Columns.Exists("PaymodeID"))
                    {
                        band.Columns["PaymodeID"].Hidden = true;
                    }

                    // Apply header styling to all columns - matching frmdialForItemMaster
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        if (!col.Hidden)
                        {
                            // Header styling - matching frmdialForItemMaster
                            col.Header.Appearance.BackColor = headerBlue;
                            col.Header.Appearance.BackColor2 = headerBlue;
                            col.Header.Appearance.BackGradientStyle = GradientStyle.None;
                            col.Header.Appearance.ForeColor = Color.White;
                            col.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                            col.Header.Appearance.FontData.SizeInPoints = 9;
                            col.Header.Appearance.TextHAlign = HAlign.Center;
                            col.Header.Appearance.TextVAlign = VAlign.Middle;
                            col.Header.Appearance.FontData.Name = "Microsoft Sans Serif";
                            col.Header.Appearance.BorderColor = headerBlue;

                            // Cell styling - matching frmdialForItemMaster
                            col.CellAppearance.TextVAlign = VAlign.Middle;
                            col.CellAppearance.BorderColor = lightBlue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error configuring columns: {ex.Message}");
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
                    if (band.Columns.Exists("PurchaseDate"))
                    {
                        band.Columns["PurchaseDate"].Format = "dd-MMM-yyyy";
                        band.Columns["PurchaseDate"].Header.Appearance.BorderColor = headerBlue;
                        band.Columns["PurchaseDate"].CellAppearance.BorderColor = lightBlue;
                    }
                    if (band.Columns.Exists("InvoiceDate"))
                    {
                        band.Columns["InvoiceDate"].Format = "dd-MMM-yyyy";
                        band.Columns["InvoiceDate"].Header.Appearance.BorderColor = headerBlue;
                        band.Columns["InvoiceDate"].CellAppearance.BorderColor = lightBlue;
                    }

                    // Set number formats for numeric columns
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
                        if (!col.Hidden)
                        {
                            col.Header.Appearance.BorderColor = headerBlue;
                            col.CellAppearance.BorderColor = lightBlue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in InitializeLayout: " + ex.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string searchText = txtSearch.Text.ToLower().Trim();

                if (originalPurchaseData == null)
                    return;

                // Get combo boxes once
                var comboBox1 = GetComboBox1();
                var comboBox2 = GetComboBox2();

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
                DataTable filteredDataTable = originalPurchaseData.Clone();

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // If search box is empty, show all data sorted by PurchaseNo descending
                    if (originalPurchaseData.Rows.Count > 0 && originalPurchaseData.Columns.Contains("PurchaseNo"))
                    {
                        DataView dv = new DataView(originalPurchaseData);
                        dv.Sort = "PurchaseNo DESC";
                        ultraGrid1.DataSource = dv.ToTable();
                    }
                    else
                    {
                        ultraGrid1.DataSource = originalPurchaseData;
                    }

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

                    // Re-apply column ordering
                    if (comboBox2 != null && comboBox2.SelectedItem != null)
                    {
                        ReorderColumns(comboBox2.SelectedItem.ToString());
                    }

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
                        case "Purchase No":
                            // Search only in PurchaseNo column
                            return row["PurchaseNo"].ToString().ToLower().Contains(searchText);

                        case "Invoice No":
                            // Search only in InvoiceNo column
                            return row["InvoiceNo"].ToString().ToLower().Contains(searchText);

                        case "Select all":
                        default:
                            // Search in all relevant columns
                            return row["PurchaseNo"].ToString().ToLower().Contains(searchText) ||
                                   (row.Table.Columns.Contains("InvoiceNo") && row["InvoiceNo"].ToString().ToLower().Contains(searchText));
                    }
                };

                // Apply the filter to get matching rows
                var matchingRows = originalPurchaseData.AsEnumerable().Where(filterFunc).ToList();

                // Get current sort field from comboBox2
                string sortField = "PurchaseNo";
                if (comboBox2 != null && comboBox2.SelectedItem != null)
                {
                    string selectedOption = comboBox2.SelectedItem.ToString();
                    if (selectedOption == "Purchase No")
                        sortField = "PurchaseNo";
                    else if (selectedOption == "Purchase Date")
                        sortField = "PurchaseDate";
                    else if (selectedOption == "Invoice No")
                        sortField = "InvoiceNo";
                    else if (selectedOption == "Invoice Date")
                        sortField = "InvoiceDate";
                }

                // Sort the matching rows based on the current sort field
                var sortedMatchingRows = sortField == "PurchaseNo"
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
                comboBox2 = GetComboBox2();
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

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.Focus();
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
            }
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                var visibleRows = ultraGrid1.Rows.Cast<UltraGridRow>()
                    .Where(r => !r.Hidden)
                    .ToList();

                int currentIndex = visibleRows.IndexOf(ultraGrid1.ActiveRow);
                UltraGridRow nextRow = null;

                if (e.KeyCode == Keys.Up)
                {
                    if (currentIndex <= 0)
                    {
                        // If at first row, go back to search box
                        txtSearch.Focus();
                        return;
                    }
                    nextRow = visibleRows[currentIndex - 1];
                }
                else
                {
                    nextRow = currentIndex < visibleRows.Count - 1 ? visibleRows[currentIndex + 1] : visibleRows.LastOrDefault();
                }

                if (nextRow != null)
                {
                    ultraGrid1.ActiveRow = nextRow;
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(nextRow);
                }

                e.Handled = true;
            }

            if (e.KeyCode == Keys.Enter && ultraGrid1.ActiveRow != null)
            {
                // Set the selected purchase properties when Enter is pressed
                SetSelectedPurchase();
                DialogResult = DialogResult.OK;
                Close();
                e.Handled = true;
            }
        }

        private void ClearHighlighting()
        {
            if (ultraGrid1.Rows == null) return;

            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                row.Hidden = false;
                foreach (UltraGridCell cell in row.Cells)
                {
                    cell.Appearance.BackColor = Color.Empty;
                    cell.Appearance.ForeColor = Color.Empty;
                }
            }
        }

        // Add a method to set the selected purchase properties
        private void SetSelectedPurchase()
        {
            if (ultraGrid1.ActiveRow != null)
            {
                // Get the selected purchase data from the active row
                if (ultraGrid1.ActiveRow.Cells["PurchaseNo"] != null &&
                    ultraGrid1.ActiveRow.Cells["PurchaseDate"] != null &&
                    ultraGrid1.ActiveRow.Cells["InvoiceNo"] != null &&
                    ultraGrid1.ActiveRow.Cells["InvoiceDate"] != null &&
                    ultraGrid1.ActiveRow.Cells["GrandTotal"] != null)
                {
                    SelectedPurchaseNo = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["PurchaseNo"].Value);
                    SelectedPurchaseDate = Convert.ToDateTime(ultraGrid1.ActiveRow.Cells["PurchaseDate"].Value);
                    SelectedInvoiceNo = ultraGrid1.ActiveRow.Cells["InvoiceNo"].Value.ToString();
                    SelectedInvoiceDate = Convert.ToDateTime(ultraGrid1.ActiveRow.Cells["InvoiceDate"].Value);
                    SelectedGrandTotal = Convert.ToDecimal(ultraGrid1.ActiveRow.Cells["GrandTotal"].Value);
                }
            }
        }

        // Add double-click event handler
        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row != null)
            {
                SetSelectedPurchase();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        // Helper method to get comboBox1
        private ComboBox GetComboBox1()
        {
            return this.Controls.Find("comboBox1", true).FirstOrDefault() as ComboBox;
        }

        // Helper method to get comboBox2
        private ComboBox GetComboBox2()
        {
            return this.Controls.Find("comboBox2", true).FirstOrDefault() as ComboBox;
        }

        // Initialize comboBox1 with search filter options
        private void InitializeSearchFilterComboBox()
        {
            // Find comboBox1 if it exists
            var comboBox1 = GetComboBox1();
            if (comboBox1 == null) return;

            // Clear existing items
            comboBox1.Items.Clear();

            // Add search options for Purchase
            comboBox1.Items.Add("Select all");
            comboBox1.Items.Add("Purchase No");
            comboBox1.Items.Add("Invoice No");

            // Set default selection
            comboBox1.SelectedIndex = 0;

            // Add event handler for selection change
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        // Handle selection change in comboBox1
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reapply search with current filter if there's text in the search box
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch_TextChanged(txtSearch, EventArgs.Empty);
            }
        }

        // Initialize comboBox2 with column order options
        private void InitializeColumnOrderComboBox()
        {
            // Find comboBox2 if it exists
            var comboBox2 = GetComboBox2();
            if (comboBox2 == null) return;

            // Clear existing items
            comboBox2.Items.Clear();

            // Add column order options for Purchase
            comboBox2.Items.Add("Purchase No");
            comboBox2.Items.Add("Purchase Date");
            comboBox2.Items.Add("Invoice No");
            comboBox2.Items.Add("Invoice Date");

            // Set default selection to Purchase No
            comboBox2.SelectedIndex = 0;

            // Add event handler for selection change
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        // Handle selection change in comboBox2
        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reorder columns based on selection
            var comboBox2 = GetComboBox2();
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

            // Reorder columns based on selection
            switch (selectedOption)
            {
                case "Purchase No":
                    // PurchaseNo first
                    if (band.Columns.Exists("PurchaseNo"))
                    {
                        band.Columns["PurchaseNo"].Header.VisiblePosition = 0;
                    }
                    if (band.Columns.Exists("PurchaseDate"))
                    {
                        band.Columns["PurchaseDate"].Header.VisiblePosition = 1;
                    }
                    if (band.Columns.Exists("InvoiceNo"))
                    {
                        band.Columns["InvoiceNo"].Header.VisiblePosition = 2;
                    }
                    if (band.Columns.Exists("InvoiceDate"))
                    {
                        band.Columns["InvoiceDate"].Header.VisiblePosition = 3;
                    }
                    if (band.Columns.Exists("GrandTotal"))
                    {
                        band.Columns["GrandTotal"].Header.VisiblePosition = 4;
                    }
                    break;

                case "Purchase Date":
                    // PurchaseDate first
                    if (band.Columns.Exists("PurchaseDate"))
                    {
                        band.Columns["PurchaseDate"].Header.VisiblePosition = 0;
                    }
                    if (band.Columns.Exists("PurchaseNo"))
                    {
                        band.Columns["PurchaseNo"].Header.VisiblePosition = 1;
                    }
                    if (band.Columns.Exists("InvoiceNo"))
                    {
                        band.Columns["InvoiceNo"].Header.VisiblePosition = 2;
                    }
                    if (band.Columns.Exists("InvoiceDate"))
                    {
                        band.Columns["InvoiceDate"].Header.VisiblePosition = 3;
                    }
                    if (band.Columns.Exists("GrandTotal"))
                    {
                        band.Columns["GrandTotal"].Header.VisiblePosition = 4;
                    }
                    break;

                case "Invoice No":
                    // InvoiceNo first
                    if (band.Columns.Exists("InvoiceNo"))
                    {
                        band.Columns["InvoiceNo"].Header.VisiblePosition = 0;
                    }
                    if (band.Columns.Exists("InvoiceDate"))
                    {
                        band.Columns["InvoiceDate"].Header.VisiblePosition = 1;
                    }
                    if (band.Columns.Exists("PurchaseNo"))
                    {
                        band.Columns["PurchaseNo"].Header.VisiblePosition = 2;
                    }
                    if (band.Columns.Exists("PurchaseDate"))
                    {
                        band.Columns["PurchaseDate"].Header.VisiblePosition = 3;
                    }
                    if (band.Columns.Exists("GrandTotal"))
                    {
                        band.Columns["GrandTotal"].Header.VisiblePosition = 4;
                    }
                    break;

                case "Invoice Date":
                    // InvoiceDate first
                    if (band.Columns.Exists("InvoiceDate"))
                    {
                        band.Columns["InvoiceDate"].Header.VisiblePosition = 0;
                    }
                    if (band.Columns.Exists("InvoiceNo"))
                    {
                        band.Columns["InvoiceNo"].Header.VisiblePosition = 1;
                    }
                    if (band.Columns.Exists("PurchaseNo"))
                    {
                        band.Columns["PurchaseNo"].Header.VisiblePosition = 2;
                    }
                    if (band.Columns.Exists("PurchaseDate"))
                    {
                        band.Columns["PurchaseDate"].Header.VisiblePosition = 3;
                    }
                    if (band.Columns.Exists("GrandTotal"))
                    {
                        band.Columns["GrandTotal"].Header.VisiblePosition = 4;
                    }
                    break;
            }

            // Force the layout to update
            ultraGrid1.Refresh();
        }

        // Setup panel styles to match ultraPanel3 from frmdialForItemMaster
        private void SetupPanelStyles()
        {
            try
            {
                // Find and style any panels that should have button-like appearance
                // Check for common panel names that might be used as buttons
                string[] panelNames = { "ultraPanel11", "ultraPanel12", "ultraPanel4", "ultraPanel5", "btnOK", "btnClose" };

                foreach (string panelName in panelNames)
                {
                    var panel = this.Controls.Find(panelName, true).FirstOrDefault() as Infragistics.Win.Misc.UltraPanel;
                    if (panel != null)
                    {
                        StyleIconPanel(panel);
                    }
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

                panel.ClientArea.MouseEnter += (sender, e) => {
                    panel.Appearance.BackColor = Color.FromArgb(160, 230, 255);
                    panel.Appearance.BackColor2 = Color.FromArgb(30, 140, 230);
                };

                panel.ClientArea.MouseLeave += (sender, e) => {
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

        // Connect panel click events
        private void ConnectPanelClickEvents()
        {
            // Try to find OK button panel (ultraPanel12 or similar)
            var okPanel = this.Controls.Find("ultraPanel12", true).FirstOrDefault() as Infragistics.Win.Misc.UltraPanel;
            if (okPanel == null)
            {
                okPanel = this.Controls.Find("btnOK", true).FirstOrDefault() as Infragistics.Win.Misc.UltraPanel;
            }

            if (okPanel != null)
            {
                okPanel.Click += OkPanel_Click;
                okPanel.ClientArea.Click += OkPanel_Click;

                // Connect click events for child controls
                foreach (Control control in okPanel.ClientArea.Controls)
                {
                    if (control is Label || control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                    {
                        control.Click += OkPanel_Click;
                    }
                }
            }

            // Try to find Close button panel (ultraPanel11 or similar)
            var closePanel = this.Controls.Find("ultraPanel11", true).FirstOrDefault() as Infragistics.Win.Misc.UltraPanel;
            if (closePanel == null)
            {
                closePanel = this.Controls.Find("btnClose", true).FirstOrDefault() as Infragistics.Win.Misc.UltraPanel;
            }

            if (closePanel != null)
            {
                closePanel.Click += ClosePanel_Click;
                closePanel.ClientArea.Click += ClosePanel_Click;

                // Connect click events for child controls
                foreach (Control control in closePanel.ClientArea.Controls)
                {
                    if (control is Label || control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                    {
                        control.Click += ClosePanel_Click;
                    }
                }
            }
        }

        // Event handler for OK panel
        private void OkPanel_Click(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    SetSelectedPurchase();
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Please select a purchase first.", "Selection Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event handler for Close panel
        private void ClosePanel_Click(object sender, EventArgs e)
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

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Call the same handler as OK panel
            OkPanel_Click(sender, e);
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

        private void ultraLabel1_Click(object sender, EventArgs e)
        {
            // Not needed, but keeping for compatibility
        }
    }
}
