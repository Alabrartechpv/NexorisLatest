using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass.TransactionModels;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace PosBranch_Win.DialogBox
{
    public partial class frmCommonDialog : Form
    {
        SalesReturnRepository srRepository = new SalesReturnRepository();
        private List<SRgetAll> originalData;
        private DataTable fullDataTable = null;

        // Define a delegate and event for returning selected data
        public delegate void SelectedDataHandler(SRgetAll selectedData);
        public event SelectedDataHandler OnDataSelected;

        // --- Advanced Feature Fields (ported from frmCustomerDialog) ---
        private Label lblStatus; // Status label for feedback
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private bool isOriginalOrder = true;
        private string frmName;

        public frmCommonDialog()
        {
            InitializeComponent();
            this.KeyPreview = true;
            InitializeStatusLabel(); // Add status label
            SetupUltraGridStyle(); // Advanced grid styling
            SetupColumnChooserMenu();
            InitializeSearchFilterComboBox();
            InitializeColumnOrderComboBox();
            SetupPanelHoverEffects();
            ConnectNavigationPanelEvents();

            // Wire up grid layout event for exact styling
            this.ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;

            // Add event handlers for keyboard navigation and selection
            this.ultraGrid1.KeyDown += ultraGrid1_KeyDown;
            this.ultraGrid1.DoubleClickRow += ultraGrid1_DoubleClickRow;
            this.ultraGrid1.MouseDown += ultraGrid1_MouseDown;
            this.textBoxsearch.KeyDown += textBox1_KeyDown;
            this.ultraGrid1.AfterSelectChange += ultraGrid1_AfterSelectChange;
            this.ultraGrid1.ClickCell += ultraGrid1_ClickCell;

            // Add OK button event handler
            this.ultraPanel5.Click += ultraPanel5_Click;

            // Add form-level key handler
            this.KeyDown += frmCommonDialog_KeyDown;

            // Add placeholder text handlers
            this.textBoxsearch.GotFocus += textBoxsearch_GotFocus;
            this.textBoxsearch.LostFocus += textBoxsearch_LostFocus;
            this.textBoxsearch.Text = "Search sales returns...";

            // --- MISSING FEATURE: Add Debug Button ---
            AddDebugButton();

            // --- MISSING FEATURE: Add Sort Toggle Icon (ultraPictureBox4) ---
            AddSortToggleIcon();

            // --- MISSING FEATURE: Add event wiring for resize/activation ---
            this.SizeChanged += FrmCommonDialog_SizeChanged;
            this.LocationChanged += (s, e) => { /* PositionColumnChooserAtBottomRight(); */ };
            this.Activated += (s, e) => { /* PositionColumnChooserAtBottomRight(); */ };
            this.FormClosing += FrmCommonDialog_FormClosing;
            this.ultraGrid1.Resize += UltraGrid1_Resize;
        }

        public frmCommonDialog(string formName) : this()
        {
            this.frmName = formName;
        }

        // --- Advanced Grid Styling (ported and adapted) ---
        private void SetupUltraGridStyle()
        {
            try
            {
                ultraGrid1.DisplayLayout.Reset();
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = Infragistics.Win.UltraWinGrid.AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.AllowColSwapping = Infragistics.Win.UltraWinGrid.AllowColSwapping.WithinBand;
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;
                ultraGrid1.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderAlpha = Infragistics.Win.Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderAlpha = Infragistics.Win.Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellPadding = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                ultraGrid1.DisplayLayout.InterBandSpacing = 0;
                Color lightBlue = Color.FromArgb(173, 216, 230);
                Color headerBlue = Color.FromArgb(0, 123, 255);
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.MinRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = Infragistics.Win.UltraWinGrid.RowSelectorHeaderStyle.Default;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = Infragistics.Win.UltraWinGrid.RowSelectorNumberStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Image = null;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Image = null;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.Image = null;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;
                if (ultraGrid1.DisplayLayout.ScrollBarLook != null)
                {
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.None;
                ultraGrid1.DisplayLayout.Override.ColumnAutoSizeMode = Infragistics.Win.UltraWinGrid.ColumnAutoSizeMode.None;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error setting up grid style: {ex.Message}");
            }
        }

        // --- Status Label Methods ---
        private void InitializeStatusLabel()
        {
            lblStatus = new Label();
            lblStatus.Name = "lblStatus";
            lblStatus.AutoSize = false;
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 30;
            lblStatus.BackColor = Color.LightYellow;
            lblStatus.BorderStyle = BorderStyle.FixedSingle;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Text = "Ready";
            this.Controls.Add(lblStatus);
        }

        private void UpdateStatus(string message)
        {
            if (lblStatus != null)
            {
                lblStatus.Text = message;
                lblStatus.Update();
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            FilterSalesReturns();
        }

        private void FilterSalesReturns()
        {
            try
            {
                if (fullDataTable == null) return;
                string searchText = textBoxsearch.Text.Trim();
                if (searchText == "Search sales returns...")
                {
                    searchText = "";
                }
                DataView dv = fullDataTable.DefaultView;
                if (!string.IsNullOrEmpty(searchText))
                {
                    string selectedField = comboBox1.SelectedItem?.ToString() ?? "All";
                    string filter = "";
                    string escapedSearchText = searchText.Replace("'", "''");
                    switch (selectedField)
                    {
                        case "Return No":
                            filter = $"CONVERT(SReturnNo, 'System.String') LIKE '%{escapedSearchText}%'";
                            break;
                        case "Invoice No":
                            filter = $"InvoiceNo LIKE '%{escapedSearchText}%'";
                            break;
                        case "Customer Name":
                            filter = $"CustomerName LIKE '%{escapedSearchText}%'";
                            break;
                        case "Payment Mode":
                            filter = $"Paymode LIKE '%{escapedSearchText}%'";
                            break;
                        case "All":
                        default:
                            filter = $"CONVERT(SReturnNo, 'System.String') LIKE '%{escapedSearchText}%' OR InvoiceNo LIKE '%{escapedSearchText}%' OR CustomerName LIKE '%{escapedSearchText}%' OR Paymode LIKE '%{escapedSearchText}%'";
                            break;
                    }
                    dv.RowFilter = filter;
                    UpdateStatus($"Filter applied: '{filter}' - Found {dv.Count} matching rows");
                }
                else
                {
                    dv.RowFilter = string.Empty;
                    UpdateStatus("No filter applied - showing all rows");
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
                UpdateStatus($"Error filtering sales returns: {ex.Message}");
            }
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectAndReturnData();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            SelectAndReturnData();
        }

        private void ultraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            // Get the row at the mouse position
            UltraGridRow row = ultraGrid1.ActiveRow;
            if (row == null)
            {
                // Try to get the row from the mouse position
                var hitInfo = ultraGrid1.DisplayLayout.UIElement.ElementFromPoint(e.Location);
                if (hitInfo != null)
                {
                    var cellUIElement = hitInfo.GetAncestor(typeof(Infragistics.Win.UltraWinGrid.CellUIElement)) as Infragistics.Win.UltraWinGrid.CellUIElement;
                    if (cellUIElement != null)
                    {
                        row = cellUIElement.Cell.Row;
                    }
                }
            }

            if (row != null)
            {
                System.Diagnostics.Debug.WriteLine($"MouseDown: Setting active row to {row.Index}");
                ultraGrid1.ActiveRow = row;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(row);
                row.Activated = true;
                UpdateStatus($"Selected row {row.Index + 1}");
            }
        }

        private void ultraGrid1_ClickCell(object sender, ClickCellEventArgs e)
        {
            // Ensure the clicked row becomes the active row
            if (e.Cell != null && e.Cell.Row != null)
            {
                System.Diagnostics.Debug.WriteLine($"ClickCell: Setting active row to {e.Cell.Row.Index}");
                ultraGrid1.ActiveRow = e.Cell.Row;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(e.Cell.Row);

                // Force the row to be activated
                e.Cell.Row.Activated = true;

                // Update status
                UpdateStatus($"Selected row {e.Cell.Row.Index + 1}");
            }
        }

        private void SelectAndReturnData()
        {
            System.Diagnostics.Debug.WriteLine("SelectAndReturnData called");
            System.Diagnostics.Debug.WriteLine($"ActiveRow: {ultraGrid1.ActiveRow != null}");
            System.Diagnostics.Debug.WriteLine($"Selected Rows Count: {ultraGrid1.Selected.Rows.Count}");

            if (ultraGrid1.ActiveRow == null)
            {
                MessageBox.Show("No row selected. Please select a row first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Get the selected data directly from grid cells (same as frmSalesListDialog)
                Int64 sReturnNo = Convert.ToInt64(ultraGrid1.ActiveRow.Cells["SReturnNo"].Value);
                string invoiceNo = ultraGrid1.ActiveRow.Cells["InvoiceNo"].Value?.ToString() ?? "";
                string customerName = ultraGrid1.ActiveRow.Cells["CustomerName"].Value?.ToString() ?? "";
                DateTime sReturnDate = Convert.ToDateTime(ultraGrid1.ActiveRow.Cells["SReturnDate"].Value);
                string paymode = ultraGrid1.ActiveRow.Cells["Paymode"].Value?.ToString() ?? "";
                double grandTotal = Convert.ToDouble(ultraGrid1.ActiveRow.Cells["GrandTotal"].Value);
                int paymodeID = 0;

                // Try to get PaymodeID if the column exists
                if (ultraGrid1.ActiveRow.Cells.Exists("PaymodeID"))
                {
                    paymodeID = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["PaymodeID"].Value ?? 0);
                }

                // Create SRgetAll object
                SRgetAll selectedData = new SRgetAll
                {
                    SReturnNo = (int)sReturnNo,
                    InvoiceNo = invoiceNo,
                    CustomerName = customerName,
                    SReturnDate = sReturnDate,
                    Paymode = paymode,
                    GrandTotal = grandTotal,
                    PaymodeID = paymodeID
                };

                // Debug output
                System.Diagnostics.Debug.WriteLine($"Selected Sales Return: {selectedData.SReturnNo}, Customer: {selectedData.CustomerName}, Invoice: {selectedData.InvoiceNo}");
                System.Diagnostics.Debug.WriteLine($"OnDataSelected event handler: {OnDataSelected != null}");

                // Trigger the event with the selected data
                if (OnDataSelected != null)
                {
                    OnDataSelected.Invoke(selectedData);
                    System.Diagnostics.Debug.WriteLine("Event triggered successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No event handler subscribed!");
                    MessageBox.Show("No event handler subscribed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Close the dialog
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SelectAndReturnData: {ex.Message}");
                MessageBox.Show("Error selecting sales return: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                // Move focus to the grid and select the first row if available
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.Focus();
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                // If there are rows in the grid, move focus to the grid and select the last row
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.Focus();
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                // If there's only one row in the filtered results, select it
                if (ultraGrid1.Rows.Count == 1)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                    SelectAndReturnData();
                }
                // Otherwise, move focus to the grid
                else if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.Focus();
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void frmCommonDialog_Load(object sender, EventArgs e)
        {
            LoadSalesReturnData();
            // Ensure focus is on the search textbox, not the grid
            this.BeginInvoke(new Action(() =>
            {
                textBoxsearch.Focus();
                textBoxsearch.Select();
            }));
        }

        private void LoadSalesReturnData()
        {
            try
            {
                // Get data
                originalData = srRepository.GetAll();

                // Sort data in descending order by SReturnNo
                if (originalData != null && originalData.Count > 0)
                {
                    originalData = originalData.OrderByDescending(x => x.SReturnNo).ToList();
                }

                // Convert to DataTable for filtering
                fullDataTable = ToDataTable<SRgetAll>(originalData);
                PreserveOriginalRowOrder(fullDataTable);

                // Set data source
                ultraGrid1.DataSource = fullDataTable;

                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
                InitializeSavedColumnWidths(); // Save initial column widths
                UpdateRecordCountLabel();
                UpdateStatus($"Loaded {fullDataTable.Rows.Count} sales returns.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading sales return data: " + ex.Message);
            }
            finally
            {
                // Ensure connection is closed after loading data
                try
                {
                    if (srRepository != null && 
                        srRepository.DataConnection != null && 
                        srRepository.DataConnection.State == ConnectionState.Open)
                    {
                        srRepository.DataConnection.Close();
                    }
                }
                catch (Exception connEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Dialog: Error closing connection: {connEx.Message}");
                }
            }
        }

        // Helper to convert List to DataTable
        private DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
            {
                Type columnType = System.Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                // Handle specific type conversions to avoid Double to Single conversion issues
                if (columnType == typeof(double))
                {
                    columnType = typeof(decimal); // Use decimal instead of double to avoid precision issues
                }

                table.Columns.Add(prop.Name, columnType);
            }

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    object value = prop.GetValue(item);
                    if (value == null)
                    {
                        row[prop.Name] = DBNull.Value;
                    }
                    else
                    {
                        // Handle double to decimal conversion for numeric columns
                        if (prop.PropertyType == typeof(double) && value is double doubleValue)
                        {
                            row[prop.Name] = Convert.ToDecimal(doubleValue);
                        }
                        else
                        {
                            row[prop.Name] = value;
                        }
                    }
                }
                table.Rows.Add(row);
            }
            return table;
        }

        private void PreserveOriginalRowOrder(DataTable table)
        {
            if (!table.Columns.Contains("OriginalRowOrder"))
            {
                DataColumn orderColumn = new DataColumn("OriginalRowOrder", typeof(int));
                table.Columns.Add(orderColumn);
                int rowIndex = 0;
                foreach (DataRow row in table.Rows)
                {
                    row["OriginalRowOrder"] = rowIndex++;
                }
            }
        }

        private void ultraPanel1_PaintClient(object sender, PaintEventArgs e)
        {
        }

        private void frmCommonDialog_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle arrow keys at the form level when the grid has focus
            if (ultraGrid1.Focused && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
            {
                // If the grid has an active row, handle navigation
                if (ultraGrid1.ActiveRow != null)
                {
                    int currentIndex = ultraGrid1.ActiveRow.Index;

                    if (e.KeyCode == Keys.Up && currentIndex > 0)
                    {
                        // Move to previous row
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex - 1];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    }
                    else if (e.KeyCode == Keys.Down && currentIndex < ultraGrid1.Rows.Count - 1)
                    {
                        // Move to next row
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex + 1];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    }
                }
                // If no row is selected but there are rows, select the first row
                else if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                }

                e.Handled = true;
            }
        }

        // Add this public method to refresh the data
        public void RefreshData()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Clear any existing data
                originalData = null;
                fullDataTable = null;
                ultraGrid1.DataSource = null;

                // Close any open connections
                try
                {
                    if (srRepository != null &&
                        srRepository.DataConnection != null &&
                        srRepository.DataConnection.State == ConnectionState.Open)
                    {
                        srRepository.DataConnection.Close();
                        System.Diagnostics.Debug.WriteLine("Closed existing connection before refresh");
                    }
                }
                catch (Exception connEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error closing connection: {connEx.Message}");
                    // Continue with refresh even if we can't close the connection
                }

                // Force garbage collection to ensure any stale data is cleared
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Create a new repository instance to ensure fresh data
                srRepository = new SalesReturnRepository();

                // Refresh the data from the database
                LoadSalesReturnData();

                // Clear any filter
                textBoxsearch.Text = "";

                // Make sure the grid is refreshed
                if (fullDataTable != null)
                {
                    ultraGrid1.DataSource = fullDataTable;
                    ultraGrid1.Refresh();

                    // Log the refresh result
                    System.Diagnostics.Debug.WriteLine($"Refreshed dialog with {fullDataTable.Rows.Count} records");
                    if (fullDataTable.Rows.Count > 0)
                    {
                        // Log a few sample records to verify they have the correct CancelFlag
                        for (int i = 0; i < Math.Min(3, fullDataTable.Rows.Count); i++)
                        {
                            DataRow row = fullDataTable.Rows[i];
                            System.Diagnostics.Debug.WriteLine($"Record #{row["SReturnNo"]}, InvoiceNo: {row["InvoiceNo"]}, Customer: {row["CustomerName"]}");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Refresh returned no data");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing data: {ex.Message}", "Refresh Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in RefreshData: {ex.Message}");

                // Try to recover by setting an empty data source
                try
                {
                    originalData = new List<SRgetAll>();
                    fullDataTable = new DataTable();
                    ultraGrid1.DataSource = fullDataTable;
                }
                catch { /* Ignore errors in the recovery attempt */ }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        // --- Grid Layout and Column Management ---
        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Hide all columns first
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in e.Layout.Bands[0].Columns)
                {
                    col.Hidden = true;
                }
                // Define sales return columns to show
                string[] columnsToShow = new string[] { "SReturnNo", "InvoiceNo", "CustomerName", "SReturnDate", "Paymode", "GrandTotal" };
                for (int i = 0; i < columnsToShow.Length; i++)
                {
                    string colKey = columnsToShow[i];
                    if (e.Layout.Bands[0].Columns.Exists(colKey))
                    {
                        var col = e.Layout.Bands[0].Columns[colKey];
                        col.Hidden = false;
                        col.Header.VisiblePosition = i;
                        switch (colKey)
                        {
                            case "SReturnNo":
                                col.Header.Caption = "Return No";
                                col.Width = 80;
                                break;
                            case "InvoiceNo":
                                col.Header.Caption = "Invoice No";
                                col.Width = 100;
                                break;
                            case "CustomerName":
                                col.Header.Caption = "Customer Name";
                                col.Width = 200;
                                break;
                            case "SReturnDate":
                                col.Header.Caption = "Return Date";
                                col.Width = 100;
                                col.Format = "dd/MM/yyyy";
                                break;
                            case "Paymode":
                                col.Header.Caption = "Payment Mode";
                                col.Width = 120;
                                break;
                            case "GrandTotal":
                                col.Header.Caption = "Total Amount";
                                col.Width = 100;
                                col.Format = "N2";
                                col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                                break;
                        }
                    }
                }
                if (e.Layout.Bands[0].Columns.Exists("OriginalRowOrder"))
                {
                    e.Layout.Bands[0].Columns["OriginalRowOrder"].Hidden = true;
                }

                // Enable column dragging to hide (drag column header down to hide)
                e.Layout.Override.AllowColSizing = AllowColSizing.Free;
                e.Layout.Override.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.WithinBand;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error initializing grid layout: {ex.Message}");
            }
        }

        private void ultraGrid1_AfterSelectChange(object sender, AfterSelectChangeEventArgs e)
        {
            if (ultraGrid1.ActiveRow != null)
            {
                try
                {
                    UltraGridCell aCellSReturnNo = ultraGrid1.ActiveRow.Cells["SReturnNo"];
                    if (aCellSReturnNo != null && aCellSReturnNo.Value != null)
                    {
                        // Update any parent form if needed
                        Form parentForm = this.Owner;
                        if (parentForm != null)
                        {
                            // Handle specific parent forms if needed
                            // Add specific handling here based on the parent form type
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating selection: " + ex.Message);
                }
            }
        }

        // --- Placeholder Text Methods ---
        private void textBoxsearch_GotFocus(object sender, EventArgs e)
        {
            if (textBoxsearch.Text == "Search sales returns...")
            {
                textBoxsearch.Text = "";
                textBoxsearch.ForeColor = Color.Black;
            }
        }

        private void textBoxsearch_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxsearch.Text))
            {
                textBoxsearch.Text = "Search sales returns...";
                textBoxsearch.ForeColor = Color.Gray;
            }
        }

        // --- Search Filter ComboBox ---
        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            comboBox1.Items.Add("Return No");
            comboBox1.Items.Add("Invoice No");
            comboBox1.Items.Add("Customer Name");
            comboBox1.Items.Add("Payment Mode");
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            textBoxsearch.TextChanged += TextBoxsearch_TextChanged;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterSalesReturns();
        }

        private void TextBoxsearch_TextChanged(object sender, EventArgs e)
        {
            FilterSalesReturns();
        }

        // --- Column Order ComboBox ---
        private void InitializeColumnOrderComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Return No");
            comboBox2.Items.Add("Invoice No");
            comboBox2.Items.Add("Customer Name");
            comboBox2.Items.Add("Return Date");
            comboBox2.Items.Add("Payment Mode");
            comboBox2.Items.Add("Total Amount");
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedColumn = comboBox2.SelectedItem?.ToString() ?? "Return No";
            ReorderColumns(selectedColumn);
        }

        private void ReorderColumns(string selectedColumn)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;
            // Store current column widths
            Dictionary<string, int> columnWidths = new Dictionary<string, int>();
            foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
            {
                if (!col.Hidden)
                {
                    columnWidths[col.Key] = col.Width;
                }
            }
            ultraGrid1.SuspendLayout();
            List<string> columnsToShow = new List<string> { "SReturnNo", "InvoiceNo", "CustomerName", "SReturnDate", "Paymode", "GrandTotal" };
            // Move selected column to the front
            string columnKey = GetColumnKeyFromDisplayName(selectedColumn);
            if (columnsToShow.Contains(columnKey))
            {
                columnsToShow.Remove(columnKey);
                columnsToShow.Insert(0, columnKey);
            }
            // Hide all columns
            foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
            {
                col.Hidden = true;
            }
            // Show and configure columns in the new order
            for (int i = 0; i < columnsToShow.Count; i++)
            {
                string colKey = columnsToShow[i];
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colKey))
                {
                    var col = ultraGrid1.DisplayLayout.Bands[0].Columns[colKey];
                    col.Hidden = false;
                    col.Header.VisiblePosition = i;
                    if (columnWidths.ContainsKey(colKey))
                        col.Width = columnWidths[colKey];
                    SetColumnProperties(col, colKey);
                }
            }
            ultraGrid1.ResumeLayout();
            ultraGrid1.Refresh();
        }

        private string GetColumnKeyFromDisplayName(string displayName)
        {
            switch (displayName)
            {
                case "Return No": return "SReturnNo";
                case "Invoice No": return "InvoiceNo";
                case "Customer Name": return "CustomerName";
                case "Return Date": return "SReturnDate";
                case "Payment Mode": return "Paymode";
                case "Total Amount": return "GrandTotal";
                default: return "SReturnNo";
            }
        }

        private void SetColumnProperties(Infragistics.Win.UltraWinGrid.UltraGridColumn col, string colKey)
        {
            switch (colKey)
            {
                case "SReturnNo":
                    col.Header.Caption = "Return No";
                    if (col.Width <= 0) col.Width = 80;
                    break;
                case "InvoiceNo":
                    col.Header.Caption = "Invoice No";
                    if (col.Width <= 0) col.Width = 100;
                    break;
                case "CustomerName":
                    col.Header.Caption = "Customer Name";
                    if (col.Width <= 0) col.Width = 200;
                    break;
                case "SReturnDate":
                    col.Header.Caption = "Return Date";
                    if (col.Width <= 0) col.Width = 100;
                    col.Format = "dd/MM/yyyy";
                    break;
                case "Paymode":
                    col.Header.Caption = "Payment Mode";
                    if (col.Width <= 0) col.Width = 120;
                    break;
                case "GrandTotal":
                    col.Header.Caption = "Total Amount";
                    if (col.Width <= 0) col.Width = 100;
                    col.Format = "N2";
                    col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                    break;
            }
        }

        // --- Navigation Panels ---
        private void ConnectNavigationPanelEvents()
        {
            ultraPanel3.Click += MoveRowUp;
            ultraPanel3.ClientArea.Click += MoveRowUp;
            ultraPictureBox5.Click += MoveRowUp;
            ultraPanel7.Click += MoveRowDown;
            ultraPanel7.ClientArea.Click += MoveRowDown;
            ultraPictureBox6.Click += MoveRowDown;
            ultraPanel5.Click += (s, e) => SelectAndReturnData();
            ultraPictureBox1.Click += (s, e) => SelectAndReturnData();
            label5.Click += (s, e) => SelectAndReturnData();
            ultraPanel6.Click += (s, e) => this.Close();
            ultraPictureBox2.Click += (s, e) => this.Close();
            label3.Click += (s, e) => this.Close();
        }

        private void MoveRowUp(object sender, EventArgs e)
        {
            if (ultraGrid1.Rows.Count == 0) return;
            if (ultraGrid1.ActiveRow == null)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                return;
            }
            int currentIndex = ultraGrid1.ActiveRow.Index;
            if (currentIndex > 0)
            {
                var rowToActivate = ultraGrid1.Rows[currentIndex - 1];
                ultraGrid1.ActiveRow = rowToActivate;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(rowToActivate);
            }
            UpdateRecordCountLabel();
        }

        private void MoveRowDown(object sender, EventArgs e)
        {
            if (ultraGrid1.Rows.Count == 0) return;
            if (ultraGrid1.ActiveRow == null)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                return;
            }
            int currentIndex = ultraGrid1.ActiveRow.Index;
            if (currentIndex < ultraGrid1.Rows.Count - 1)
            {
                var rowToActivate = ultraGrid1.Rows[currentIndex + 1];
                ultraGrid1.ActiveRow = rowToActivate;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(rowToActivate);
            }
            UpdateRecordCountLabel();
        }

        // --- Record Count Display ---
        private void UpdateRecordCountLabel()
        {
            int currentDisplayCount = ultraGrid1.Rows?.Count ?? 0;
            int totalCount = fullDataTable?.Rows.Count ?? 0;

            if (this.Controls.Find("label1", true).Length > 0)
            {
                Label label1 = this.Controls.Find("label1", true)[0] as Label;
                if (label1 != null)
                {
                    label1.Text = $"Showing {currentDisplayCount} of {totalCount} records";
                    label1.AutoSize = true;
                    label1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
                    label1.ForeColor = Color.FromArgb(0, 70, 170);
                }
            }
            if (textBox3 != null)
            {
                textBox3.Text = currentDisplayCount.ToString();
            }
            UpdateStatus($"Showing {currentDisplayCount} of {totalCount} records");
        }

        // --- Panel Hover Effects ---
        private void SetupPanelHoverEffects()
        {
            SetupPanelGroupHoverEffects(ultraPanel6, label3, ultraPictureBox2);
            SetupPanelGroupHoverEffects(ultraPanel3, null, ultraPictureBox5);
            SetupPanelGroupHoverEffects(ultraPanel7, null, ultraPictureBox6);
        }

        private void SetupPanelGroupHoverEffects(Infragistics.Win.Misc.UltraPanel panel, Label label, Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox)
        {
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;
            Color hoverBackColor = BrightenColor(originalBackColor, 30);
            Color hoverBackColor2 = BrightenColor(originalBackColor2, 30);
            Action applyHoverEffect = () => {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
                panel.ClientArea.Cursor = Cursors.Hand;
            };
            Action removeHoverEffect = () => {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
                panel.ClientArea.Cursor = Cursors.Default;
            };
            panel.MouseEnter += (s, e) => { applyHoverEffect(); };
            panel.MouseLeave += (s, e) => { removeHoverEffect(); };
            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (s, e) => { applyHoverEffect(); pictureBox.Cursor = Cursors.Hand; };
                pictureBox.MouseLeave += (s, e) => { if (!IsMouseOverControl(panel)) removeHoverEffect(); };
            }
            if (label != null)
            {
                label.MouseEnter += (s, e) => { applyHoverEffect(); label.Cursor = Cursors.Hand; };
                label.MouseLeave += (s, e) => { if (!IsMouseOverControl(panel)) removeHoverEffect(); };
            }
        }

        private Color BrightenColor(Color color, int amount)
        {
            return Color.FromArgb(color.A, Math.Min(color.R + amount, 255), Math.Min(color.G + amount, 255), Math.Min(color.B + amount, 255));
        }

        private bool IsMouseOverControl(Control control)
        {
            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }

        // --- Debug Button ---
        private void AddDebugButton()
        {
            if (this.Controls.Find("btnDebug", true).FirstOrDefault() == null)
            {
                Button btnDebug = new Button();
                btnDebug.Name = "btnDebug";
                btnDebug.Text = "Debug";
                btnDebug.Size = new Size(60, 25);
                btnDebug.Location = new Point(textBoxsearch.Right + 10, textBoxsearch.Top);
                btnDebug.Click += BtnDebug_Click;
                this.Controls.Add(btnDebug);
            }
        }

        private void BtnDebug_Click(object sender, EventArgs e)
        {
            try
            {
                if (fullDataTable == null)
                {
                    MessageBox.Show("No data loaded yet");
                    return;
                }
                string filterStatus = "No filter applied";
                int visibleRows = fullDataTable.Rows.Count;
                if (ultraGrid1.DataSource is DataView dv && !string.IsNullOrEmpty(dv.RowFilter))
                {
                    filterStatus = $"Current filter: {dv.RowFilter}";
                    visibleRows = dv.Count;
                }
                string debugInfo = $"Data status:\n- Total rows in data: {fullDataTable.Rows.Count}\n- Visible rows in grid: {visibleRows}\n- {filterStatus}\n- Search text: '{textBoxsearch.Text}'";
                MessageBox.Show(debugInfo, "Search Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Debug error: {ex.Message}");
            }
        }

        // --- Sort Toggle Icon ---
        private void AddSortToggleIcon()
        {
            // Wire up the sort toggle button (ultraPanel9 contains ultraPictureBox3)
            ultraPanel9.Click += UltraPictureBox4_Click;
            ultraPictureBox3.Click += UltraPictureBox4_Click;
            System.Windows.Forms.ToolTip tooltip = new System.Windows.Forms.ToolTip();
            tooltip.SetToolTip(ultraPanel9, "Click to toggle between ascending and descending sort order for the selected column");
            tooltip.SetToolTip(ultraPictureBox3, "Click to toggle between ascending and descending sort order for the selected column");
        }

        private void UltraPictureBox4_Click(object sender, EventArgs e)
        {
            if (fullDataTable == null) return;
            isOriginalOrder = !isOriginalOrder;
            DataView dv = fullDataTable.DefaultView;

            // Get the currently selected sort column from comboBox2, or default to "Return No"
            string selectedColumn = comboBox2.SelectedItem?.ToString() ?? "Return No";
            string columnKey = GetColumnKeyFromDisplayName(selectedColumn);

            if (isOriginalOrder)
            {
                dv.Sort = $"{columnKey} ASC";
                UpdateStatus($"Displaying sales returns sorted by {selectedColumn} in ascending order.");
            }
            else
            {
                dv.Sort = $"{columnKey} DESC";
                UpdateStatus($"Displaying sales returns sorted by {selectedColumn} in descending order.");
            }

            // Maintain selection if possible
            if (ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
            }
        }

        // --- Event Handlers ---
        private void FrmCommonDialog_SizeChanged(object sender, EventArgs e)
        {
            PreserveColumnWidths();
        }

        private void UltraGrid1_Resize(object sender, EventArgs e)
        {
            PreserveColumnWidths();
        }

        private void FrmCommonDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up column chooser form
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Close();
                columnChooserForm = null;
            }

            // Ensure connection is closed when dialog closes
            try
            {
                if (srRepository != null && 
                    srRepository.DataConnection != null && 
                    srRepository.DataConnection.State == ConnectionState.Open)
                {
                    srRepository.DataConnection.Close();
                }
            }
            catch (Exception connEx)
            {
                System.Diagnostics.Debug.WriteLine($"Dialog: Error closing connection on form close: {connEx.Message}");
            }
        }

        // --- Column Width Preservation ---
        private void PreserveColumnWidths()
        {
            try
            {
                ultraGrid1.SuspendLayout();
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                    {
                        col.Width = savedColumnWidths[col.Key];
                    }
                }
                ultraGrid1.ResumeLayout();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error preserving column widths: {ex.Message}");
            }
        }

        private void InitializeSavedColumnWidths()
        {
            savedColumnWidths.Clear();
            foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn column in ultraGrid1.DisplayLayout.Bands[0].Columns)
            {
                if (!column.Hidden)
                {
                    savedColumnWidths[column.Key] = column.Width;
                }
            }
        }

        // --- Column Chooser (Enhanced) ---
        private void SetupColumnChooserMenu()
        {
            // Enable column dragging to hide
            ultraGrid1.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            ultraGrid1.DisplayLayout.Override.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.WithinBand;

            // Add context menu for column chooser
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += ColumnChooserMenuItem_Click;
            gridContextMenu.Items.Add(columnChooserMenuItem);
            ultraGrid1.ContextMenuStrip = gridContextMenu;

            // Note: ultraPanel9 and ultraPictureBox4 are already wired for sort toggle
            // Column chooser is available via right-click context menu
        }

        private void ColumnChooserMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;

                // Create a simple column chooser form
                Form columnForm = new Form();
                columnForm.Text = "Column Chooser";
                columnForm.Size = new Size(300, 400);
                columnForm.StartPosition = FormStartPosition.CenterParent;

                CheckedListBox columnList = new CheckedListBox();
                columnList.Dock = DockStyle.Fill;
                columnList.CheckOnClick = true;

                // Add columns to the list
                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (col.Key != "OriginalRowOrder") // Hide the internal column
                    {
                        columnList.Items.Add(col.Header.Caption, !col.Hidden);
                    }
                }

                Button okButton = new Button();
                okButton.Text = "OK";
                okButton.Dock = DockStyle.Bottom;
                okButton.Height = 30;
                okButton.Click += (s, ev) => {
                    // Apply column visibility changes
                    for (int i = 0; i < columnList.Items.Count; i++)
                    {
                        string columnCaption = columnList.Items[i].ToString();
                        foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                        {
                            if (col.Header.Caption == columnCaption)
                            {
                                col.Hidden = !columnList.GetItemChecked(i);
                                break;
                            }
                        }
                    }
                    columnForm.Close();
                };

                columnForm.Controls.Add(columnList);
                columnForm.Controls.Add(okButton);
                columnForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening column chooser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- OK Button Event Handler ---
        private void ultraPanel5_Click(object sender, EventArgs e)
        {
            SelectAndReturnData();
        }
    }
}
