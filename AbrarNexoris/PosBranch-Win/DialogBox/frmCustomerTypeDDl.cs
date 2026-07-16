using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using PosBranch_Win.Master;
using Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmCustomerTypeDDl : Form
    {
        Dropdowns drop = new Dropdowns();
        frmItemMasterNew ItemMaster = new frmItemMasterNew();

        // Add fields for data management
        private DataTable originalDataTable = null;
        private int maxRecordsToDisplay = int.MaxValue;
        private System.Windows.Forms.Timer inputDebounceTimer;
        private string lastProcessedValue = string.Empty;
        private bool isHandlingClick = false;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();

        public frmCustomerTypeDDl()
        {
            InitializeComponent();
        }

        private void frmCustomerTypeDDl_Load(object sender, EventArgs e)
        {
            // Setup grid style to match frmVendorDig
            SetupUltraGridStyle();

            // Setup panel styles to match frmVendorDig's ultraPanel12
            StyleIconPanel(ultraPanel3);
            StyleIconPanel(ultraPanel5);
            StyleIconPanel(ultraPanel6);
            StyleIconPanel(ultraPanel7);

            // Connect panel click events
            ConnectPanelClickEvents();

            // Initialize search filter comboBox1
            InitializeSearchFilterComboBox();

            // Initialize column order comboBox2
            InitializeColumnOrderComboBox();

            // Set up search box event (textBox1 is the search box)
            textBox1.TextChanged += textBox1_Search_TextChanged;
            textBox1.KeyDown += textBox1_Search_KeyDown;

            // Register textBox3 event for record limit
            textBox3.TextChanged += textBox3_TextChanged;
            textBox3.KeyDown += textBox3_KeyDown;

            // Initialize the debounce timer
            inputDebounceTimer = new System.Windows.Forms.Timer();
            inputDebounceTimer.Interval = 5;
            inputDebounceTimer.Tick += InputDebounceTimer_Tick;

            // Set up grid double-click event
            ultraGrid1.DoubleClickRow += ultraGrid1_DoubleClickRow;

            // Load customer type grid
            LoadCustomerTypeGrid();

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

        private void SetupUltraGridStyle()
        {
            try
            {
                // Reset everything first to ensure clean slate
                ultraGrid1.DisplayLayout.Reset();

                // Remove the grid caption (banner)
                ultraGrid1.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.ViewStyle = ViewStyle.SingleBand;

                // Configure the grid appearance
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;

                // Enable column moving and dragging - important for column draggability
                ultraGrid1.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.AllowColSwapping = AllowColSwapping.WithinBand;

                // Important: This setting ensures we get only row selection on click, not automatic action
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

                // Hide the group-by area (gray bar)
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

                // Configure row selector appearance with blue
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue; // Same color for no gradient
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None; // Remove numbers
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15; // Smaller width

                // Set all cells to have white background (no alternate row coloring)
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;

                // Remove alternate row appearance (make all rows white)
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

                // Configure selected row appearance with light blue highlight
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215); // Bright blue from the image
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White; // White text for better contrast

                // Configure active row appearance - make it same as selected row
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;

                // Set font size for all cells to match the image (standard text size)
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Configure scrollbar style
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;

                // Configure the scrollbar look
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

                // Configure cell appearance to increase vertical content alignment
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = HAlign.Center;

                // Allow drop operation for drag-drop support
                ultraGrid1.AllowDrop = true;

                // Configure column auto size behavior for optimal resizing
                ultraGrid1.DisplayLayout.Override.ColumnAutoSizeMode = ColumnAutoSizeMode.VisibleRows;

                // Refresh the grid to apply all changes
                ultraGrid1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up grid style: {ex.Message}");
            }
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Handle Enter key - same as OK button
            if (e.KeyChar == (char)Keys.Enter)
            {
                HandleOKButton();
            }
        }

        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            // Handle double-click - same as OK button
            HandleOKButton();
        }

        private void HandleOKButton()
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
                    if (ItemMaster != null)
                    {
                        UltraGridCell Id = this.ultraGrid1.ActiveRow.Cells["Id"];
                        UltraGridCell PriceLevel = this.ultraGrid1.ActiveRow.Cells["PriceLevel"];

                        if (Id != null && PriceLevel != null)
                        {
                            ItemMaster.txt_CustomerType.Text = PriceLevel.Value.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling OK button: {ex.Message}");
            }
            finally
            {
                this.Close();
            }
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Remove the grid caption (banner)
                e.Layout.CaptionVisible = DefaultableBoolean.False;
                e.Layout.ViewStyle = ViewStyle.SingleBand;

                // Set the row selector width to minimum
                e.Layout.Override.RowSelectorWidth = 15;

                // Apply proper grid line styles
                e.Layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

                // Set border style for the main grid
                e.Layout.BorderStyle = UIElementBorderStyle.Solid;

                // Remove ALL cell padding/spacing - critical to remove unwanted space
                e.Layout.Override.CellPadding = 0;
                e.Layout.Override.RowSpacingBefore = 0;
                e.Layout.Override.RowSpacingAfter = 0;
                e.Layout.Override.CellSpacing = 0;
                e.Layout.InterBandSpacing = 0;
                e.Layout.MaxBandDepth = 1;
                e.Layout.MaxColScrollRegions = 1;
                e.Layout.MaxRowScrollRegions = 1;

                // Configure row height to match the image
                e.Layout.Override.MinRowHeight = 30;
                e.Layout.Override.DefaultRowHeight = 30;
                e.Layout.Override.RowSizing = RowSizing.Fixed;

                // Define colors
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
                Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

                // Set default alignment for all cells
                e.Layout.Override.CellAppearance.TextHAlign = HAlign.Center;
                e.Layout.Override.CellAppearance.TextVAlign = VAlign.Middle;

                // Set font size for all cells to match the image (standard text size)
                e.Layout.Override.CellAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.RowAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                e.Layout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Enable column moving and dragging - important for column draggability
                e.Layout.Override.AllowColMoving = AllowColMoving.WithinBand;
                e.Layout.Override.AllowColSizing = AllowColSizing.Free;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in InitializeLayout: {ex.Message}");
            }
        }

        private void StyleIconPanel(UltraPanel panel)
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

                    // Add hover effect to picture box
                    pic.MouseEnter += (sender, e) =>
                    {
                        pic.Appearance.BorderColor = Color.White;
                    };

                    pic.MouseLeave += (sender, e) =>
                    {
                        pic.Appearance.BorderColor = Color.Transparent;
                    };

                    // Set cursor to indicate clickable
                    pic.Cursor = Cursors.Hand;
                }
                else if (control is Label)
                {
                    Label lbl = (Label)control;
                    lbl.ForeColor = Color.White;
                    lbl.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
                    lbl.BackColor = Color.Transparent;

                    // Set cursor to indicate clickable
                    lbl.Cursor = Cursors.Hand;

                    // Add hover effect to label - keep white color (removed yellow)
                    lbl.MouseEnter += (sender, e) =>
                    {
                        lbl.ForeColor = Color.White; // Keep white instead of yellow
                    };

                    lbl.MouseLeave += (sender, e) =>
                    {
                        lbl.ForeColor = Color.White;
                    };
                }
            }

            // Add hover effect with consistent colors
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

        private void LoadCustomerTypeGrid()
        {
            try
            {
                DataBase.Operations = "PriceLevel";
                CustomerTypeDDlGrid customerTypeGrid = drop.getCustomerTypeDDl();

                // Create DataTable from the list to allow sorting and searching
                DataTable dt = new DataTable();
                if (customerTypeGrid.List != null && customerTypeGrid.List.Count() > 0)
                {
                    // Convert List to DataTable
                    System.ComponentModel.PropertyDescriptorCollection properties =
                        System.ComponentModel.TypeDescriptor.GetProperties(typeof(ModelClass.CustomerTypeDDL));

                    foreach (System.ComponentModel.PropertyDescriptor prop in properties)
                    {
                        dt.Columns.Add(prop.Name, System.Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    }

                    foreach (var item in customerTypeGrid.List)
                    {
                        DataRow row = dt.NewRow();
                        foreach (System.ComponentModel.PropertyDescriptor prop in properties)
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }
                        dt.Rows.Add(row);
                    }

                    // Store the original DataTable
                    originalDataTable = dt.Copy();

                    // Update maxRecordsToDisplay to show all records and update textBox3
                    maxRecordsToDisplay = dt.Rows.Count;
                    textBox3.Text = maxRecordsToDisplay.ToString();

                    // Use the full DataTable as the datasource - showing all records
                    ultraGrid1.DataSource = dt;
                }
                else
                {
                    ultraGrid1.DataSource = customerTypeGrid.List;
                    textBox3.Text = "0";
                }

                ConfigureGridLayout();

                // Apply column ordering based on comboBox2 selection
                if (comboBox2.SelectedItem != null)
                {
                    ReorderColumns(comboBox2.SelectedItem.ToString());
                }

                // Update the record count label
                UpdateRecordCountLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customer type data: " + ex.Message);
            }
        }

        private void ConfigureGridLayout()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Calculate total available width
                int totalWidth = ultraGrid1.Width - ultraGrid1.DisplayLayout.Override.RowSelectorWidth - 20;

                // Get current column order from comboBox2
                string currentOrder = comboBox2.SelectedItem?.ToString() ?? "Id";

                // Show only specific columns with adjusted widths
                if (band.Columns.Exists("Id"))
                {
                    band.Columns["Id"].Hidden = false;
                    band.Columns["Id"].Header.Caption = "ID";
                    band.Columns["Id"].Width = (int)(totalWidth * 0.15);
                    band.Columns["Id"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["Id"].Header.VisiblePosition = currentOrder == "Id" ? 0 : 1;
                    if (!savedColumnWidths.ContainsKey("Id"))
                        savedColumnWidths["Id"] = band.Columns["Id"].Width;
                }

                if (band.Columns.Exists("PriceLevel"))
                {
                    band.Columns["PriceLevel"].Hidden = false;
                    band.Columns["PriceLevel"].Header.Caption = "Price Level";
                    band.Columns["PriceLevel"].Width = (int)(totalWidth * 0.85);
                    band.Columns["PriceLevel"].CellAppearance.TextHAlign = HAlign.Left;
                    band.Columns["PriceLevel"].Header.VisiblePosition = currentOrder == "Id" ? 1 : 0;
                    if (!savedColumnWidths.ContainsKey("PriceLevel"))
                        savedColumnWidths["PriceLevel"] = band.Columns["PriceLevel"].Width;
                }

                // Hide all other columns
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (col.Key != "Id" && col.Key != "PriceLevel")
                    {
                        col.Hidden = true;
                    }
                }
            }
        }

        // Initialize comboBox1 with search filter options
        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("Select all");
            comboBox1.Items.Add("Id");
            comboBox1.Items.Add("Price Level");
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        // Handle selection change in comboBox1
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1_Search_TextChanged(textBox1, EventArgs.Empty);
            }
        }

        // Initialize comboBox2 with column order options
        private void InitializeColumnOrderComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Id");
            comboBox2.Items.Add("Price Level");
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        // Handle selection change in comboBox2
        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReorderColumns(comboBox2.SelectedItem.ToString());
        }

        // Method to reorder columns based on selection
        private void ReorderColumns(string selectedOption)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                int totalWidth = ultraGrid1.Width - ultraGrid1.DisplayLayout.Override.RowSelectorWidth - 20;

                if (selectedOption == "Id")
                {
                    if (band.Columns.Exists("Id"))
                    {
                        band.Columns["Id"].Header.VisiblePosition = 0;
                        band.Columns["Id"].Width = (int)(totalWidth * 0.15);
                    }
                    if (band.Columns.Exists("PriceLevel"))
                    {
                        band.Columns["PriceLevel"].Header.VisiblePosition = 1;
                        band.Columns["PriceLevel"].Width = (int)(totalWidth * 0.85);
                    }
                }
                else if (selectedOption == "Price Level")
                {
                    if (band.Columns.Exists("PriceLevel"))
                    {
                        band.Columns["PriceLevel"].Header.VisiblePosition = 0;
                        band.Columns["PriceLevel"].Width = (int)(totalWidth * 0.85);
                    }
                    if (band.Columns.Exists("Id"))
                    {
                        band.Columns["Id"].Header.VisiblePosition = 1;
                        band.Columns["Id"].Width = (int)(totalWidth * 0.15);
                    }
                }

                ultraGrid1.Refresh();
            }
        }

        // Search box text changed event
        private void textBox1_Search_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string searchText = textBox1.Text.ToLower();

                if (originalDataTable == null)
                    return;

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

                DataTable filteredDataTable = originalDataTable.Clone();

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    ApplyRecordLimit();
                    return;
                }

                string filterOption = comboBox1.SelectedItem?.ToString() ?? "Select all";

                Func<DataRow, bool> filterFunc = row =>
                {
                    switch (filterOption)
                    {
                        case "Id":
                            return row["Id"].ToString().ToLower().Contains(searchText);
                        case "Price Level":
                            return row["PriceLevel"].ToString().ToLower().Contains(searchText);
                        case "Select all":
                        default:
                            return row["Id"].ToString().ToLower().Contains(searchText) ||
                                   row["PriceLevel"].ToString().ToLower().Contains(searchText);
                    }
                };

                var matchingRows = originalDataTable.AsEnumerable().Where(filterFunc).ToList();

                string sortField = comboBox2.SelectedItem?.ToString() == "Id" ? "Id" : "PriceLevel";
                var sortedMatchingRows = sortField == "Id"
                    ? matchingRows.OrderBy(r => Convert.ToInt32(r[sortField])).ToList()
                    : matchingRows.OrderBy(r => r[sortField].ToString()).ToList();

                var limitedRows = sortedMatchingRows.Take(maxRecordsToDisplay);

                foreach (var row in limitedRows)
                {
                    filteredDataTable.ImportRow(row);
                }

                ultraGrid1.DataSource = filteredDataTable;

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

                ConfigureGridLayout();

                if (comboBox2.SelectedItem != null)
                {
                    ReorderColumns(comboBox2.SelectedItem.ToString());
                }

                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }

                UpdateRecordCountLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while searching: " + ex.Message);
                ApplyRecordLimit();
            }
        }

        private void textBox1_Search_KeyDown(object sender, KeyEventArgs e)
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

        // Record limit textBox3 handlers
        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ProcessTextBoxValueImmediate(textBox3.Text);
                ultraGrid1.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            inputDebounceTimer.Stop();
            if (textBox3.Text == lastProcessedValue)
                return;
            inputDebounceTimer.Start();
        }

        private void InputDebounceTimer_Tick(object sender, EventArgs e)
        {
            inputDebounceTimer.Stop();
            ProcessTextBoxValueImmediate(textBox3.Text);
        }

        private void ProcessTextBoxValueImmediate(string value)
        {
            lastProcessedValue = value;

            if (string.IsNullOrWhiteSpace(value))
            {
                maxRecordsToDisplay = originalDataTable != null ? originalDataTable.Rows.Count : int.MaxValue;
                ApplyRecordLimitFast();
                return;
            }

            if (int.TryParse(value, out int recordCount))
            {
                if (recordCount > 0)
                {
                    maxRecordsToDisplay = recordCount;
                    ApplyRecordLimitFast();
                }
                else
                {
                    maxRecordsToDisplay = 1;
                    textBox3.Text = "1";
                    lastProcessedValue = "1";
                    ApplyRecordLimitFast();
                }
            }
        }

        private void ApplyRecordLimitFast()
        {
            if (originalDataTable == null || originalDataTable.Rows.Count == 0)
                return;

            try
            {
                ultraGrid1.BeginUpdate();

                DataView view = new DataView(originalDataTable);
                string sortField = comboBox2.SelectedItem?.ToString() == "Id" ? "Id" : "PriceLevel";
                view.Sort = sortField;

                if (maxRecordsToDisplay >= originalDataTable.Rows.Count)
                {
                    ultraGrid1.DataSource = view;
                }
                else
                {
                    DataTable limitedTable = originalDataTable.Clone();
                    for (int i = 0; i < maxRecordsToDisplay && i < view.Count; i++)
                    {
                        limitedTable.ImportRow(view[i].Row);
                    }
                    ultraGrid1.DataSource = limitedTable;
                }

                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                        {
                            col.Width = savedColumnWidths[col.Key];
                        }
                    }
                }

                ConfigureGridLayout();

                if (comboBox2.SelectedItem != null)
                {
                    ReorderColumns(comboBox2.SelectedItem.ToString());
                }

                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }

                UpdateRecordCountLabel();
                ultraGrid1.EndUpdate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error applying record limit: " + ex.Message);
            }
        }

        private void ApplyRecordLimit()
        {
            ApplyRecordLimitFast();
        }

        private void UpdateRecordCountLabel()
        {
            if (originalDataTable != null)
            {
                int currentDisplayCount = 0;
                if (ultraGrid1.DataSource is DataTable dt)
                {
                    currentDisplayCount = dt.Rows.Count;
                }
                else if (ultraGrid1.DataSource is DataView dv)
                {
                    currentDisplayCount = dv.Count;
                }
                else if (ultraGrid1.Rows != null)
                {
                    currentDisplayCount = ultraGrid1.Rows.Count;
                }

                int totalCount = originalDataTable.Rows.Count;
                int currentLimit = maxRecordsToDisplay;
                if (!string.IsNullOrWhiteSpace(textBox3.Text) && int.TryParse(textBox3.Text, out int limit))
                {
                    currentLimit = limit;
                }

                if (this.Controls.Find("label1", true).Length > 0)
                {
                    Label label1 = this.Controls.Find("label1", true)[0] as Label;
                    if (label1 != null)
                    {
                        label1.Text = $"Showing {currentLimit} of {totalCount} records";
                    }
                }
            }
        }

        // Panel click handlers
        private void ConnectPanelClickEvents()
        {
            // ultraPanel3 - Navigation up
            ultraPanel3.Click += (sender, e) => HandlePanelClick("ultraPanel3");
            ultraPanel3.ClientArea.Click += (sender, e) => HandlePanelClick("ultraPanel3");

            // ultraPanel5 - OK button
            ultraPanel5.Click += (sender, e) => HandlePanelClick("ultraPanel5");
            ultraPanel5.ClientArea.Click += (sender, e) => HandlePanelClick("ultraPanel5");
            if (label5 != null) label5.Click += (sender, e) => HandlePanelClick("ultraPanel5");
            if (ultraPictureBox1 != null) ultraPictureBox1.Click += (sender, e) => HandlePanelClick("ultraPanel5");
            if (ultraPictureBox10 != null) ultraPictureBox10.Click += (sender, e) => HandlePanelClick("ultraPanel5");

            // ultraPanel6 - Close button
            ultraPanel6.Click += (sender, e) => HandlePanelClick("ultraPanel6");
            ultraPanel6.ClientArea.Click += (sender, e) => HandlePanelClick("ultraPanel6");
            if (label3 != null) label3.Click += (sender, e) => HandlePanelClick("ultraPanel6");
            if (ultraPictureBox2 != null) ultraPictureBox2.Click += (sender, e) => HandlePanelClick("ultraPanel6");
            if (ultraPictureBox9 != null) ultraPictureBox9.Click += (sender, e) => HandlePanelClick("ultraPanel6");

            // ultraPanel7 - Navigation down
            ultraPanel7.Click += (sender, e) => HandlePanelClick("ultraPanel7");
            ultraPanel7.ClientArea.Click += (sender, e) => HandlePanelClick("ultraPanel7");
        }

        private void HandlePanelClick(string panelName)
        {
            if (isHandlingClick) return;

            try
            {
                isHandlingClick = true;
                Panel_Click(null, EventArgs.Empty, panelName);
            }
            finally
            {
                isHandlingClick = false;
            }
        }

        private void Panel_Click(object sender, EventArgs e, string panelName)
        {
            switch (panelName)
            {
                case "ultraPanel3":
                    // Handle ultraPanel3 click (e.g., navigation up)
                    break;
                case "ultraPanel5":
                    // Handle ultraPanel5 click - acts as OK button
                    HandleOKButton();
                    break;
                case "ultraPanel6":
                    // Handle ultraPanel6 click - acts as Close button
                    System.Diagnostics.Debug.WriteLine("Close button clicked in frmCustomerTypeDDl");
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    break;
                case "ultraPanel7":
                    // Handle ultraPanel7 click (e.g., navigation down)
                    break;
            }
        }
    }
}
