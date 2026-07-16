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
using Repository;
using ModelClass;
using Infragistics.Win.UltraWinGrid;
using PosBranch_Win.Transaction;

namespace PosBranch_Win.DialogBox
{
    public partial class FrmItemPurchaseHistory : Form
    {
        // Properties to store selected item data
        public long SelectedItemId { get; private set; }
        public string SelectedItemName { get; private set; }
        public string SelectedBarcode { get; private set; }
        public string SelectedUnit { get; private set; }
        public double SelectedPacking { get; private set; }
        public decimal SelectedCost { get; private set; }
        public double SelectedQuantity { get; private set; }
        public decimal SelectedAmount { get; private set; }
        public int SelectedUnitId { get; private set; }
        public string SelectedTaxType { get; private set; }
        public decimal SelectedTaxPer { get; private set; }
        public decimal SelectedTaxAmt { get; private set; }
        public bool ItemSelected { get; private set; }

        // Fields for search, filtering, and record limiting
        private DataTable originalDataTable = null; // Store the original data
        private int maxRecordsToDisplay = int.MaxValue; // Show all records by default
        private bool isOriginalOrder = true; // Track sort order
        private System.Windows.Forms.Timer inputDebounceTimer; // Timer for debouncing textBox3 input
        private string lastProcessedValue = string.Empty; // Track last processed value for textBox3

        public FrmItemPurchaseHistory()
        {
            InitializeComponent();
            InitializeProperties();
            RegisterEventHandlers();
            SetupUltraGridStyle();
            SetupUltraPanelStyle();
            SetupPanelHoverEffects();

            // Initialize search and filter combo boxes
            InitializeSearchFilterComboBox();
            InitializeColumnOrderComboBox();

            // Initialize debounce timer for textBox3
            inputDebounceTimer = new System.Windows.Forms.Timer();
            inputDebounceTimer.Interval = 5; // Very short interval for instant response
            inputDebounceTimer.Tick += InputDebounceTimer_Tick;

            // Register textBox3 events for record limiting
            if (textBox3 != null)
            {
                textBox3.TextChanged += textBox3_TextChanged;
                textBox3.KeyDown += TextBox3_KeyDown;
            }

            // Register textBoxsearch events for searching
            if (textBoxsearch != null)
            {
                textBoxsearch.TextChanged += textBoxsearch_TextChanged;
                textBoxsearch.KeyDown += textBoxsearch_KeyDown;
            }

            // Register ultraPictureBox4 click event for sorting toggle
            if (ultraPictureBox4 != null)
            {
                ultraPictureBox4.Click += UltraPictureBox4_Click;
                ultraPictureBox4.Cursor = Cursors.Hand;
            }

            // Register InitializeLayout event to ensure group-by box stays hidden
            if (ultraGrid1 != null)
            {
                ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
            }
        }

        private void InitializeProperties()
        {
            SelectedItemId = 0;
            SelectedItemName = "";
            SelectedBarcode = "";
            SelectedUnit = "";
            SelectedPacking = 1.0;
            SelectedCost = 0;
            SelectedQuantity = 0;
            SelectedAmount = 0;
            SelectedUnitId = 0;
            SelectedTaxType = "I";
            SelectedTaxPer = 0;
            SelectedTaxAmt = 0;
            ItemSelected = false;
        }

        private void RegisterEventHandlers()
        {
            // Register double-click event for grid
            if (ultraGrid1 != null)
            {
                ultraGrid1.DoubleClick += UltraGrid1_DoubleClick;
                ultraGrid1.KeyDown += UltraGrid1_KeyDown;
            }

            // Register OK button click (ultraPictureBox1 based on designer)
            if (ultraPictureBox1 != null)
            {
                ultraPictureBox1.Click += UltraPictureBox1_Click;
            }

            // Register Close button click (ultraPictureBox2)
            if (ultraPictureBox2 != null)
            {
                ultraPictureBox2.Click += UltraPictureBox2_Click;
            }
        }

        /// <summary>
        /// Loads purchase history for a specific item ID
        /// </summary>
        /// <param name="itemId">The Item ID to get purchase history for</param>
        public void LoadPurchaseHistoryByItemId(long itemId)
        {
            try
            {
                BaseRepostitory baseRepo = new BaseRepostitory();
                DataTable dt = new DataTable();

                using (SqlConnection conn = (SqlConnection)baseRepo.DataConnection)
                {
                    conn.Open();

                    // Query to get all purchase details for the specified item ID
                    // Join with PMaster to get purchase date, invoice info, and vendor details
                    string query = @"
                        SELECT 
                            pd.PurchaseNo,
                            pm.PurchaseDate,
                            pm.InvoiceNo,
                            pm.InvoiceDate,
                            ISNULL(lm.LedgerName, '') as VendorName,
                            pd.SlNo,
                            pd.ItemID,
                            ISNULL(im.[Description], '') as ItemName,
                            ISNULL(ps.BarCode, '') as BarCode,
                            pd.UnitId,
                            pd.Unit,
                            pd.Packing,
                            pd.Qty,
                            pd.Cost,
                            ISNULL(pd.TaxType, 'I') as TaxType,
                            ISNULL(pd.TaxPer, 0) as TaxPer,
                            ISNULL(pd.TaxAmt, 0) as TaxAmt,
                            ISNULL((pd.Cost * pd.Packing * pd.Qty), 0) as Amount
                        FROM PDetails pd
                        INNER JOIN PMaster pm ON pd.PurchaseNo = pm.PurchaseNo 
                            AND pd.CompanyId = pm.CompanyId 
                            AND pd.BranchID = pm.BranchId
                            AND pd.FinYearId = pm.FinYearId
                        LEFT JOIN ItemMaster im ON pd.ItemID = im.ItemId
                        LEFT JOIN PriceSettings ps ON pd.ItemID = ps.ItemId 
                            AND (ps.UnitId IS NULL OR pd.UnitId = ps.UnitId)
                        LEFT JOIN LedgerMaster lm ON pm.LedgerID = lm.LedgerID
                        WHERE pd.ItemID = @ItemID
                            AND pd.CompanyId = @CompanyId
                            AND pd.BranchID = @BranchId
                        ORDER BY pm.PurchaseDate DESC, pd.PurchaseNo DESC, pd.SlNo";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ItemID", itemId);
                        cmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(DataBase.CompanyId));
                        cmd.Parameters.AddWithValue("@BranchId", Convert.ToInt32(DataBase.BranchId));

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }

                // Store original data table for searching and filtering
                originalDataTable = dt.Copy();

                // Add a column to preserve the original row order
                PreserveOriginalRowOrder(originalDataTable);

                // Update maxRecordsToDisplay to show all records and update textBox3
                maxRecordsToDisplay = dt.Rows.Count;
                if (textBox3 != null)
                {
                    textBox3.Text = maxRecordsToDisplay.ToString();
                }

                // Bind the data to the grid
                if (ultraGrid1 != null)
                {
                    ultraGrid1.DataSource = dt;

                    // Ensure group-by box stays hidden after data binding
                    ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                    ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;

                    // Update label to show item information
                    if (dt.Rows.Count > 0 && label1 != null)
                    {
                        string itemName = dt.Rows[0]["ItemName"].ToString();
                        label1.Text = $"Purchase History for Item: {itemName} (ID: {itemId}) - {dt.Rows.Count} record(s)";
                    }
                    else if (label1 != null)
                    {
                        label1.Text = $"No purchase history found for Item ID: {itemId}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading purchase history: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectCurrentRow()
        {
            try
            {
                if (ultraGrid1.ActiveRow == null)
                {
                    MessageBox.Show("Please select a purchase record from the grid.",
                        "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Get data from the selected row
                var row = ultraGrid1.ActiveRow;

                SelectedItemId = Convert.ToInt64(row.Cells["ItemID"].Value ?? 0);
                SelectedItemName = row.Cells["ItemName"].Value?.ToString() ?? "";
                SelectedBarcode = row.Cells["BarCode"].Value?.ToString() ?? "";
                SelectedUnit = row.Cells["Unit"].Value?.ToString() ?? "";
                SelectedPacking = Convert.ToDouble(row.Cells["Packing"].Value ?? 1.0);
                SelectedCost = Convert.ToDecimal(row.Cells["Cost"].Value ?? 0);
                SelectedQuantity = Convert.ToDouble(row.Cells["Qty"].Value ?? 0);
                SelectedAmount = Convert.ToDecimal(row.Cells["Amount"].Value ?? 0);
                SelectedUnitId = Convert.ToInt32(row.Cells["UnitId"].Value ?? 0);
                SelectedTaxType = row.Cells["TaxType"].Value?.ToString() ?? "I";
                SelectedTaxPer = Convert.ToDecimal(row.Cells["TaxPer"].Value ?? 0);
                SelectedTaxAmt = Convert.ToDecimal(row.Cells["TaxAmt"].Value ?? 0);
                ItemSelected = true;

                // Close dialog with OK result
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting item: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraGrid1_DoubleClick(object sender, EventArgs e)
        {
            SelectCurrentRow();
        }

        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                SelectCurrentRow();
            }
        }

        private void UltraPictureBox1_Click(object sender, EventArgs e)
        {
            SelectCurrentRow();
        }

        private void UltraPictureBox2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SetupUltraGridStyle()
        {
            try
            {
                // Reset everything first to ensure clean slate
                ultraGrid1.DisplayLayout.Reset();

                // Configure the grid appearance - matching view.cs exactly
                ultraGrid1.DisplayLayout.Appearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.MaxColScrollRegions = 1;
                ultraGrid1.DisplayLayout.MaxRowScrollRegions = 1;

                // Active row appearance - SystemColors.Highlight (matching view.cs)
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = SystemColors.Highlight;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = SystemColors.HighlightText;

                // Card area appearance
                ultraGrid1.DisplayLayout.Override.CardAreaAppearance.BackColor = Color.Transparent;

                // Cell click action - EditAndSelectText (matching view.cs)
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;

                // Header appearance - matching view.cs gradient blue
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(0, 122, 204);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(0, 102, 184);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Arial";
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9F;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Header click action - SortMulti (matching view.cs)
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortMulti;

                // Row selector appearance - matching view.cs gradient blue
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(0, 122, 204);
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = Color.FromArgb(0, 102, 184);
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 25; // Matching view.cs (25 instead of 15)

                // Selected row appearance - SystemColors.Highlight (matching view.cs)
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = SystemColors.Highlight;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = SystemColors.Highlight;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = SystemColors.HighlightText;

                // Scroll settings
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;

                // Hide the group-by area (gray bar) - This removes "drag a column here..."
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;

                // Refresh the grid to apply all changes
                ultraGrid1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up grid style: {ex.Message}");
            }
        }

        private void SetupUltraPanelStyle()
        {
            // First apply the base styling to all panels
            StyleIconPanel(ultraPanel3);
            StyleIconPanel(ultraPanel4);
            StyleIconPanel(ultraPanel5);
            StyleIconPanel(ultraPanel6);
            StyleIconPanel(ultraPanel7);

            // Now explicitly set ultraPanel3 and ultraPanel7 to match ultraPanel4's colors
            // This ensures they have exactly the same appearance
            ultraPanel3.Appearance.BackColor = ultraPanel4.Appearance.BackColor;
            ultraPanel3.Appearance.BackColor2 = ultraPanel4.Appearance.BackColor2;
            ultraPanel3.Appearance.BackGradientStyle = ultraPanel4.Appearance.BackGradientStyle;
            ultraPanel3.Appearance.BorderColor = ultraPanel4.Appearance.BorderColor;

            ultraPanel7.Appearance.BackColor = ultraPanel4.Appearance.BackColor;
            ultraPanel7.Appearance.BackColor2 = ultraPanel4.Appearance.BackColor2;
            ultraPanel7.Appearance.BackGradientStyle = ultraPanel4.Appearance.BackGradientStyle;
            ultraPanel7.Appearance.BorderColor = ultraPanel4.Appearance.BorderColor;

            // Set appearance for the main panel
            ultPanelPurchaseDisplay.Appearance.BackColor = Color.White;
            ultPanelPurchaseDisplay.Appearance.BackColor2 = Color.FromArgb(200, 230, 250);
            ultPanelPurchaseDisplay.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            ultPanelPurchaseDisplay.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
        }

        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            // Define consistent colors for all panels
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
            panel.ClientArea.MouseEnter += (sender, e) => {
                panel.Appearance.BackColor = Color.FromArgb(160, 230, 255);
                panel.Appearance.BackColor2 = Color.FromArgb(30, 140, 230);
            };

            panel.ClientArea.MouseLeave += (sender, e) => {
                panel.Appearance.BackColor = lightBlue;
                panel.Appearance.BackColor2 = darkBlue;
            };

            // Set cursor to hand to indicate clickable
            panel.ClientArea.Cursor = Cursors.Hand;
        }

        // Add hover effects to all panels and their child controls
        private void SetupPanelHoverEffects()
        {
            // Set up hover effects for each panel group
            SetupPanelGroupHoverEffects(ultraPanel4, label4, ultraPictureBox3);
            SetupPanelGroupHoverEffects(ultraPanel6, label3, ultraPictureBox2);
            SetupPanelGroupHoverEffects(ultraPanel3, null, ultraPictureBox5);
            SetupPanelGroupHoverEffects(ultraPanel7, null, ultraPictureBox6);
            SetupPanelGroupHoverEffects(ultraPanel5, label5, ultraPictureBox1);

            // Add click handler for ultraPanel4 to open purchase form
            if (ultraPanel4 != null)
            {
                ultraPanel4.Click += UltraPanel4_Click;
                ultraPanel4.ClientArea.Click += UltraPanel4_Click;
            }
            if (label4 != null)
            {
                label4.Click += UltraPanel4_Click;
            }
            if (ultraPictureBox3 != null)
            {
                ultraPictureBox3.Click += UltraPanel4_Click;
            }
        }

        // Set up hover effects for a panel group (panel, label, picturebox)
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
            Action applyHoverEffect = () => {
                // Change panel colors
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;

                // Change cursor to hand
                panel.ClientArea.Cursor = Cursors.Hand;
            };

            Action removeHoverEffect = () => {
                // Restore original colors
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;

                // Restore cursor
                panel.ClientArea.Cursor = Cursors.Default;
            };

            // Add hover effects to the panel
            panel.MouseEnter += (s, e) => {
                applyHoverEffect();
            };

            panel.MouseLeave += (s, e) => {
                removeHoverEffect();
            };

            // Add hover effects to the picture box if provided
            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (s, e) => {
                    // Apply hover effect to the panel
                    applyHoverEffect();

                    // Change cursor to hand
                    pictureBox.Cursor = Cursors.Hand;
                };

                pictureBox.MouseLeave += (s, e) => {
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
                label.MouseEnter += (s, e) => {
                    // Apply hover effect to the panel
                    applyHoverEffect();

                    // Change cursor to hand
                    label.Cursor = Cursors.Hand;
                };

                label.MouseLeave += (s, e) => {
                    // Only restore panel colors if mouse is not still over the panel
                    if (!IsMouseOverControl(panel))
                    {
                        removeHoverEffect();
                    }
                };
            }
        }

        // Helper method to brighten a color
        private Color BrightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(color.R + amount, 255),
                Math.Min(color.G + amount, 255),
                Math.Min(color.B + amount, 255));
        }

        // Helper method to check if mouse is over a control
        private bool IsMouseOverControl(Control control)
        {
            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Hide the group-by area (gray bar) - This removes "drag a column here..."
                e.Layout.GroupByBox.Hidden = true;
                e.Layout.GroupByBox.Prompt = string.Empty;

                // Match view.cs grid appearance exactly
                e.Layout.Appearance.BackColor = Color.White;
                e.Layout.MaxColScrollRegions = 1;
                e.Layout.MaxRowScrollRegions = 1;

                // Active row appearance - SystemColors.Highlight (matching view.cs)
                e.Layout.Override.ActiveRowAppearance.BackColor = SystemColors.Highlight;
                e.Layout.Override.ActiveRowAppearance.ForeColor = SystemColors.HighlightText;

                // Card area appearance
                e.Layout.Override.CardAreaAppearance.BackColor = Color.Transparent;

                // Cell click action - EditAndSelectText (matching view.cs)
                e.Layout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;

                // Header appearance - matching view.cs gradient blue
                e.Layout.Override.HeaderAppearance.BackColor = Color.FromArgb(0, 122, 204);
                e.Layout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(0, 102, 184);
                e.Layout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                e.Layout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                e.Layout.Override.HeaderAppearance.FontData.Name = "Arial";
                e.Layout.Override.HeaderAppearance.FontData.SizeInPoints = 9F;
                e.Layout.Override.HeaderAppearance.ForeColor = Color.White;
                e.Layout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                e.Layout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Header click action - SortMulti (matching view.cs)
                e.Layout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortMulti;

                // Row selector appearance - matching view.cs gradient blue
                e.Layout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(0, 122, 204);
                e.Layout.Override.RowSelectorAppearance.BackColor2 = Color.FromArgb(0, 102, 184);
                e.Layout.Override.RowSelectorAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                e.Layout.Override.RowSelectorAppearance.ForeColor = Color.White;
                e.Layout.Override.RowSelectorWidth = 25; // Matching view.cs (25 instead of 15)

                // Selected row appearance - SystemColors.Highlight (matching view.cs)
                e.Layout.Override.SelectedRowAppearance.BackColor = SystemColors.Highlight;
                e.Layout.Override.SelectedRowAppearance.BackColor2 = SystemColors.Highlight;
                e.Layout.Override.SelectedRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                e.Layout.Override.SelectedRowAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                e.Layout.Override.SelectedRowAppearance.ForeColor = SystemColors.HighlightText;

                // Scroll settings
                e.Layout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                e.Layout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing grid layout: {ex.Message}");
            }
        }

        // Initialize the search filter comboBox
        private void InitializeSearchFilterComboBox()
        {
            if (comboBox1 == null) return;

            // Clear any existing items
            comboBox1.Items.Clear();

            // Add search filter options based on available columns
            comboBox1.Items.Add("Select all");
            comboBox1.Items.Add("Purchase No");
            comboBox1.Items.Add("Invoice No");
            comboBox1.Items.Add("Vendor Name");
            comboBox1.Items.Add("Item Name");
            comboBox1.Items.Add("Barcode");
            comboBox1.Items.Add("Unit");

            // Select "Select all" by default
            comboBox1.SelectedIndex = 0;

            // Add event handler
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        // Handle comboBox1 selection change
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Re-apply the current filter with the new search column selection
            if (textBoxsearch != null)
            {
                string searchText = textBoxsearch.Text.Trim();
                textBoxsearch_TextChanged(textBoxsearch, EventArgs.Empty);
            }
        }

        // Initialize the column order comboBox
        private void InitializeColumnOrderComboBox()
        {
            if (comboBox2 == null) return;

            // Clear any existing items
            comboBox2.Items.Clear();

            // Add column options for reordering
            comboBox2.Items.Add("Purchase No");
            comboBox2.Items.Add("Purchase Date");
            comboBox2.Items.Add("Invoice No");
            comboBox2.Items.Add("Vendor Name");
            comboBox2.Items.Add("Item Name");

            // Select "Purchase No" by default
            comboBox2.SelectedIndex = 0;

            // Add event handler
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        // Handle comboBox2 selection change (column ordering)
        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the selected column option
                string selectedColumn = comboBox2.SelectedItem?.ToString() ?? "Purchase No";

                // Reorder columns based on selection
                ReorderColumns(selectedColumn);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error changing column order: {ex.Message}");
            }
        }

        // Reorder columns based on selected option
        private void ReorderColumns(string selectedColumn)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;

            try
            {
                ultraGrid1.SuspendLayout();

                // Map display names to column keys
                Dictionary<string, string> columnMap = new Dictionary<string, string>
                {
                    { "Purchase No", "PurchaseNo" },
                    { "Purchase Date", "PurchaseDate" },
                    { "Invoice No", "InvoiceNo" },
                    { "Vendor Name", "VendorName" },
                    { "Item Name", "ItemName" }
                };

                string columnKey = columnMap.ContainsKey(selectedColumn) ? columnMap[selectedColumn] : "PurchaseNo";

                // Get all visible columns
                var visibleColumns = new List<UltraGridColumn>();
                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        visibleColumns.Add(col);
                    }
                }

                // Move selected column to the front
                var selectedCol = visibleColumns.FirstOrDefault(c => c.Key == columnKey);
                if (selectedCol != null)
                {
                    visibleColumns.Remove(selectedCol);
                    visibleColumns.Insert(0, selectedCol);
                }

                // Set the new order
                for (int i = 0; i < visibleColumns.Count; i++)
                {
                    visibleColumns[i].Header.VisiblePosition = i;
                }

                ultraGrid1.ResumeLayout(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reordering columns: {ex.Message}");
            }
        }

        // Handle textBoxsearch text change for searching
        private void textBoxsearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string searchText = textBoxsearch.Text.ToLower().Trim();

                if (originalDataTable == null)
                    return;

                // Store the current column widths and positions before changing the data source
                Dictionary<string, int> columnWidths = new Dictionary<string, int>();
                Dictionary<string, int> columnPositions = new Dictionary<string, int>();

                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (!col.Hidden)
                        {
                            columnWidths[col.Key] = col.Width;
                            columnPositions[col.Key] = col.Header.VisiblePosition;
                        }
                    }
                }

                // Create a new DataTable with the same schema
                DataTable filteredDataTable = originalDataTable.Clone();

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // If search box is empty, reapply the record limit to show the default view
                    ApplyRecordLimit();
                    return;
                }

                // Get the selected search filter option
                string filterOption = comboBox1?.SelectedItem?.ToString() ?? "Select all";

                // Build the filter function based on selected search field
                Func<DataRow, bool> filterFunc = row =>
                {
                    switch (filterOption)
                    {
                        case "Purchase No":
                            return row["PurchaseNo"]?.ToString().ToLower().Contains(searchText) ?? false;
                        case "Invoice No":
                            return row["InvoiceNo"]?.ToString().ToLower().Contains(searchText) ?? false;
                        case "Vendor Name":
                            return row["VendorName"]?.ToString().ToLower().Contains(searchText) ?? false;
                        case "Item Name":
                            return row["ItemName"]?.ToString().ToLower().Contains(searchText) ?? false;
                        case "Barcode":
                            return row["BarCode"]?.ToString().ToLower().Contains(searchText) ?? false;
                        case "Unit":
                            return row["Unit"]?.ToString().ToLower().Contains(searchText) ?? false;
                        case "Select all":
                        default:
                            // Search in all relevant columns
                            return (row["PurchaseNo"]?.ToString().ToLower().Contains(searchText) ?? false) ||
                                   (row["InvoiceNo"]?.ToString().ToLower().Contains(searchText) ?? false) ||
                                   (row["VendorName"]?.ToString().ToLower().Contains(searchText) ?? false) ||
                                   (row["ItemName"]?.ToString().ToLower().Contains(searchText) ?? false) ||
                                   (row["BarCode"]?.ToString().ToLower().Contains(searchText) ?? false) ||
                                   (row["Unit"]?.ToString().ToLower().Contains(searchText) ?? false);
                    }
                };

                // Apply the filter to get matching rows
                var matchingRows = originalDataTable.AsEnumerable().Where(filterFunc).ToList();

                // Get current sort field from comboBox2
                string sortField = comboBox2?.SelectedItem?.ToString() ?? "Purchase No";
                Dictionary<string, string> sortMap = new Dictionary<string, string>
                {
                    { "Purchase No", "PurchaseNo" },
                    { "Purchase Date", "PurchaseDate" },
                    { "Invoice No", "InvoiceNo" },
                    { "Vendor Name", "VendorName" },
                    { "Item Name", "ItemName" }
                };
                string sortColumn = sortMap.ContainsKey(sortField) ? sortMap[sortField] : "PurchaseNo";

                // Sort the matching rows
                if (sortColumn == "PurchaseDate")
                {
                    matchingRows = matchingRows.OrderByDescending(r =>
                        r[sortColumn] != DBNull.Value ? Convert.ToDateTime(r[sortColumn]) : DateTime.MinValue).ToList();
                }
                else if (sortColumn == "PurchaseNo")
                {
                    matchingRows = matchingRows.OrderByDescending(r =>
                        r[sortColumn] != DBNull.Value ? Convert.ToInt32(r[sortColumn]) : 0).ToList();
                }
                else
                {
                    matchingRows = matchingRows.OrderBy(r => r[sortColumn]?.ToString() ?? "").ToList();
                }

                // Take the first maxRecordsToDisplay rows from the sorted result
                var limitedRows = matchingRows.Take(maxRecordsToDisplay);

                // Add the filtered rows to the new DataTable
                foreach (var row in limitedRows)
                {
                    filteredDataTable.ImportRow(row);
                }

                // Set the filtered DataTable as the DataSource
                ultraGrid1.DataSource = filteredDataTable;

                // Restore column widths and positions
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
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
                if (comboBox2?.SelectedItem != null)
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
                System.Diagnostics.Debug.WriteLine($"Error while searching: {ex.Message}");
            }
        }

        // Handle textBoxsearch key down
        private void textBoxsearch_KeyDown(object sender, KeyEventArgs e)
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

        // Handle ultraPictureBox4 click to toggle sort order
        private void UltraPictureBox4_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we have data to sort
                if (ultraGrid1.DataSource == null || ultraGrid1.Rows.Count == 0)
                    return;

                // Toggle between original order and reverse order
                isOriginalOrder = !isOriginalOrder;

                // Suspend layout while sorting
                ultraGrid1.BeginUpdate();

                // Get the current DataView or DataTable
                DataView dataView = null;
                if (ultraGrid1.DataSource is DataView dv)
                {
                    dataView = dv;
                }
                else if (ultraGrid1.DataSource is DataTable dt)
                {
                    dataView = dt.DefaultView;
                }

                if (dataView != null)
                {
                    // Check if we have the OriginalRowOrder column
                    if (dataView.Table.Columns.Contains("OriginalRowOrder"))
                    {
                        // Sort by the original row order column
                        if (isOriginalOrder)
                        {
                            dataView.Sort = "OriginalRowOrder ASC";
                        }
                        else
                        {
                            dataView.Sort = "OriginalRowOrder DESC";
                        }
                    }
                    else
                    {
                        // Fallback to sorting by PurchaseDate or PurchaseNo
                        if (dataView.Table.Columns.Contains("PurchaseDate"))
                        {
                            dataView.Sort = isOriginalOrder ? "PurchaseDate DESC" : "PurchaseDate ASC";
                        }
                        else if (dataView.Table.Columns.Contains("PurchaseNo"))
                        {
                            dataView.Sort = isOriginalOrder ? "PurchaseNo DESC" : "PurchaseNo ASC";
                        }
                    }

                    // Set the sorted view as the data source
                    ultraGrid1.DataSource = dataView;
                }

                // Resume layout
                ultraGrid1.EndUpdate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during sort: {ex.Message}");
            }
        }

        // Handler for textBox3 text change event (record limiting)
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // Reset and restart the timer on each keystroke
            inputDebounceTimer.Stop();

            // If the value is the same as last processed, don't reprocess
            if (textBox3.Text == lastProcessedValue)
                return;

            // Start the timer to process after a very short delay
            inputDebounceTimer.Start();
        }

        // Timer tick handler for debounced processing
        private void InputDebounceTimer_Tick(object sender, EventArgs e)
        {
            // Stop the timer
            inputDebounceTimer.Stop();

            // Process the value
            ProcessTextBox3ValueImmediate(textBox3.Text);
        }

        // Process the textBox3 value immediately
        private void ProcessTextBox3ValueImmediate(string value)
        {
            // Store the value being processed to avoid reprocessing
            lastProcessedValue = value;

            // Handle empty text case
            if (string.IsNullOrWhiteSpace(value))
            {
                // Show all records if empty
                maxRecordsToDisplay = originalDataTable != null ? originalDataTable.Rows.Count : int.MaxValue;
                ApplyRecordLimit();
                return;
            }

            // Try to parse the input as integer
            if (int.TryParse(value, out int recordCount))
            {
                // Ensure the value is at least 1
                if (recordCount > 0)
                {
                    maxRecordsToDisplay = recordCount;
                    ApplyRecordLimit();
                }
                else
                {
                    // If zero or negative, set to minimum of 1
                    maxRecordsToDisplay = 1;
                    textBox3.Text = "1";
                    lastProcessedValue = "1";
                    ApplyRecordLimit();
                }
            }
            // If not a valid integer, don't update (keep previous value)
        }

        // Handle textBox3 key down
        private void TextBox3_KeyDown(object sender, KeyEventArgs e)
        {
            // Apply changes immediately on Enter key press
            if (e.KeyCode == Keys.Enter)
            {
                // Process the value immediately
                ProcessTextBox3ValueImmediate(textBox3.Text);

                // Move focus away to trigger immediate application
                ultraGrid1.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        // Apply record limit (respects current search filter if any)
        private void ApplyRecordLimit()
        {
            if (originalDataTable == null || originalDataTable.Rows.Count == 0)
                return;

            try
            {
                // If there's a search text, use the search method instead
                if (textBoxsearch != null && !string.IsNullOrWhiteSpace(textBoxsearch.Text))
                {
                    textBoxsearch_TextChanged(textBoxsearch, EventArgs.Empty);
                    return;
                }

                // Suspend layout to prevent flickering
                ultraGrid1.BeginUpdate();

                // Use direct DataView filtering
                DataView view = new DataView(originalDataTable);

                // Get current sort field from comboBox2
                string sortField = comboBox2?.SelectedItem?.ToString() ?? "Purchase No";
                Dictionary<string, string> sortMap = new Dictionary<string, string>
                {
                    { "Purchase No", "PurchaseNo" },
                    { "Purchase Date", "PurchaseDate" },
                    { "Invoice No", "InvoiceNo" },
                    { "Vendor Name", "VendorName" },
                    { "Item Name", "ItemName" }
                };
                string sortColumn = sortMap.ContainsKey(sortField) ? sortMap[sortField] : "PurchaseNo";

                // Set the sort on the view
                if (sortColumn == "PurchaseDate")
                {
                    view.Sort = "PurchaseDate DESC";
                }
                else if (sortColumn == "PurchaseNo")
                {
                    view.Sort = "PurchaseNo DESC";
                }
                else
                {
                    view.Sort = sortColumn;
                }

                // Apply the limit directly to the grid
                if (maxRecordsToDisplay >= originalDataTable.Rows.Count)
                {
                    // Show all records
                    ultraGrid1.DataSource = view;
                }
                else
                {
                    // Create a limited table more efficiently
                    DataTable limitedTable = originalDataTable.Clone();

                    // Take only the rows we need
                    for (int i = 0; i < maxRecordsToDisplay && i < view.Count; i++)
                    {
                        limitedTable.ImportRow(view[i].Row);
                    }

                    ultraGrid1.DataSource = limitedTable;
                }

                // Re-apply column ordering
                if (comboBox2?.SelectedItem != null)
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

                // Resume layout
                ultraGrid1.EndUpdate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error applying record limit: " + ex.Message);
            }
        }

        // Add a method to preserve the original row order
        private void PreserveOriginalRowOrder(DataTable table)
        {
            try
            {
                // Check if the table already has a row order column
                if (!table.Columns.Contains("OriginalRowOrder"))
                {
                    // Add a column to track the original row order
                    DataColumn orderColumn = new DataColumn("OriginalRowOrder", typeof(int));
                    table.Columns.Add(orderColumn);

                    // Set the row order values
                    int rowIndex = 0;
                    foreach (DataRow row in table.Rows)
                    {
                        row["OriginalRowOrder"] = rowIndex++;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error preserving row order: {ex.Message}");
            }
        }

        // Handle ultraPanel4 click to open purchase form in tab
        private void UltraPanel4_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if a row is selected
                if (ultraGrid1.ActiveRow == null)
                {
                    MessageBox.Show("Please select a purchase record from the grid.",
                        "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Get the purchase number from the selected row
                var row = ultraGrid1.ActiveRow;
                if (!row.Cells.Exists("PurchaseNo") || row.Cells["PurchaseNo"].Value == null)
                {
                    MessageBox.Show("Purchase number not found in the selected row.",
                        "Invalid Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int purchaseNo = Convert.ToInt32(row.Cells["PurchaseNo"].Value);

                // Get Pid from PurchaseNo
                int pid = GetPidFromPurchaseNo(purchaseNo);
                if (pid <= 0)
                {
                    MessageBox.Show($"Could not find purchase with number: {purchaseNo}",
                        "Purchase Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Find the Home form to access the tab control
                Form homeForm = FindHomeForm();
                if (homeForm == null)
                {
                    MessageBox.Show("Home form not found. Cannot open purchase in tab.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Use reflection to call OpenFormInTabSafe method
                var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTabSafe",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (openFormInTabMethod == null)
                {
                    // Fallback to OpenFormInTab if OpenFormInTabSafe doesn't exist
                    openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                }

                if (openFormInTabMethod != null)
                {
                    // Check if purchase form already exists in a tab
                    var tabControlMainField = homeForm.GetType().GetField("tabControlMain",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (tabControlMainField != null)
                    {
                        var tabControl = tabControlMainField.GetValue(homeForm) as Infragistics.Win.UltraWinTabControl.UltraTabControl;

                        if (tabControl != null)
                        {
                            // Check for existing Purchase tab
                            string tabName = $"Purchase - #{purchaseNo}";
                            foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControl.Tabs)
                            {
                                if (tab.Text == tabName && tab.TabPage.Controls.Count > 0 &&
                                    tab.TabPage.Controls[0] is PosBranch_Win.Transaction.FrmPurchase existingForm &&
                                    !existingForm.IsDisposed)
                                {
                                    // Activate existing tab and load data
                                    tabControl.SelectedTab = tab;
                                    existingForm.BringToFront();
                                    existingForm.Focus();
                                    existingForm.LoadPurchaseData(pid);
                                    // Close this form after opening purchase form
                                    this.Close();
                                    return;
                                }
                            }
                        }
                    }

                    // Create new purchase form and open in tab
                    var purchaseForm = new PosBranch_Win.Transaction.FrmPurchase();
                    string newTabName = $"Purchase - #{purchaseNo}";

                    // Open the form in tab
                    openFormInTabMethod.Invoke(homeForm, new object[] { purchaseForm, newTabName });

                    // Schedule the data load to occur after the form is fully initialized in the tab
                    System.Threading.Tasks.Task.Delay(200).ContinueWith(_ =>
                    {
                        try
                        {
                            // Use purchaseForm's Invoke instead of this.Invoke since this form will be closed
                            if (purchaseForm != null && !purchaseForm.IsDisposed && purchaseForm.IsHandleCreated)
                            {
                                purchaseForm.Invoke(new Action(() =>
                                {
                                    try
                                    {
                                        purchaseForm.LoadPurchaseData(pid);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("Error loading purchase data: " + ex.Message, "Error",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }));
                            }
                            else
                            {
                                // If purchaseForm is not ready, try direct call
                                try
                                {
                                    purchaseForm.LoadPurchaseData(pid);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Error loading purchase data: " + ex.Message, "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        catch
                        {
                            // If invoke fails, try direct call as fallback
                            try
                            {
                                if (purchaseForm != null && !purchaseForm.IsDisposed)
                                {
                                    purchaseForm.LoadPurchaseData(pid);
                                }
                            }
                            catch
                            {
                                // Silently fail if we can't load the data
                            }
                        }
                    });
                    
                    // Close this form after opening purchase form in tab
                    this.Close();
                }
                else
                {
                    // Fallback: show as regular form
                    var purchaseForm = new PosBranch_Win.Transaction.FrmPurchase();
                    purchaseForm.Show();
                    purchaseForm.BringToFront();
                    purchaseForm.LoadPurchaseData(pid);
                    
                    // Close this form after opening purchase form
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening purchase form: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper method to find Home form
        private Form FindHomeForm()
        {
            try
            {
                // Look for Home form in all open forms
                foreach (Form form in Application.OpenForms)
                {
                    if (form.GetType().Name == "Home" || form.Name == "Home")
                    {
                        return form;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding Home form: {ex.Message}");
                return null;
            }
        }

        // Helper method to get Pid from PurchaseNo
        private int GetPidFromPurchaseNo(int purchaseNo)
        {
            try
            {
                BaseRepostitory repo = new BaseRepostitory();
                using (SqlConnection conn = (SqlConnection)repo.DataConnection)
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT TOP 1 Pid FROM PMaster WHERE PurchaseNo = @PurchaseNo AND BranchId = @BranchId AND CompanyId = @CompanyId ORDER BY Pid DESC",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("@PurchaseNo", purchaseNo);
                        cmd.Parameters.AddWithValue("@BranchId", Convert.ToInt32(DataBase.BranchId));
                        cmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(DataBase.CompanyId));

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting Pid from PurchaseNo: {ex.Message}");
            }
            return 0;
        }
    }
}
