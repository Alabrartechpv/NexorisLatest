using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using PosBranch_Win.Accounts;
using PosBranch_Win.Transaction;
using Repository;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;

namespace PosBranch_Win.DialogBox
{
    public partial class frmCustomerDialog : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        Dropdowns dp = new Dropdowns();
        private DataTable fullDataTable = null;
        public static string SetValueForText1 = "";
        public int SelectedCustomerId { get; private set; }
        public string SelectedCustomerName { get; private set; }
        private string frmName;
        // Feature Fields

        private Label lblStatus; // Status label for feedback
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private bool isOriginalOrder = true;


        public frmCustomerDialog()
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
            this.textBoxsearch.KeyDown += textBox1_KeyDown;
            this.ultraGrid1.AfterSelectChange += ultraGrid1_AfterSelectChange;

            // Add form-level key handler
            this.KeyDown += frmCustomerDialog_KeyDown;

            // Add placeholder text handlers
            this.textBoxsearch.GotFocus += textBoxsearch_GotFocus;
            this.textBoxsearch.LostFocus += textBoxsearch_LostFocus;
            this.textBoxsearch.Text = "Search customers...";

            // Add Sort Toggle Icon
            AddSortToggleIcon();

            // Event wiring for resize/activation

            this.SizeChanged += FrmCustomerDialog_SizeChanged;
            this.LocationChanged += (s, e) => PositionColumnChooserAtBottomRight();
            this.Activated += (s, e) => PositionColumnChooserAtBottomRight();
            this.FormClosing += FrmCustomerDialog_FormClosing;
            this.ultraGrid1.Resize += UltraGrid1_Resize;
        }

        public frmCustomerDialog(string formName) : this()
        {
            this.frmName = formName;
        }

        private void LoadCustomerData()
        {
            try
            {
                var customerData = dp.CustomerDDl();
                fullDataTable = ToDataTable<ModelClass.CustomerDDl>(customerData.List.ToList());
                PreserveOriginalRowOrder(fullDataTable);
                ultraGrid1.DataSource = fullDataTable;
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("LedgerName"))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns["LedgerName"].Width = 350;
                    }
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("LedgerID"))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns["LedgerID"].Width = 80;
                    }
                }
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
                InitializeSavedColumnWidths(); // Save initial column widths
                UpdateRecordCountLabel();
                UpdateStatus($"Loaded {fullDataTable.Rows.Count} customers.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customer data: " + ex.Message);
            }
        }

        /// <summary>Reloads customer data — called after a customer is saved or updated.</summary>
        public void RefreshData() => LoadCustomerData();

        // Helper to convert List to DataTable
        private DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, System.Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
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

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmCustomerDialog_Load(object sender, EventArgs e)
        {
            LoadCustomerData();
            // Ensure focus is on the search textbox, not the grid
            this.BeginInvoke(new Action(() =>
            {
                textBoxsearch.Focus();
                textBoxsearch.Select();
            }));
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Hide all columns first
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in e.Layout.Bands[0].Columns)
                {
                    col.Hidden = true;
                }
                // Define customer columns to show (adapt as needed)
                string[] columnsToShow = new string[] { "LedgerID", "LedgerName" };
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
                            case "LedgerID":
                                col.Header.Caption = "ID";
                                col.Width = 80;
                                break;
                            case "LedgerName":
                                col.Header.Caption = "Customer Name";
                                col.Width = 350;
                                break;
                        }
                    }
                }
                if (e.Layout.Bands[0].Columns.Exists("OriginalRowOrder"))
                {
                    e.Layout.Bands[0].Columns["OriginalRowOrder"].Hidden = true;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error initializing grid layout: {ex.Message}");
            }
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // This is kept for compatibility with the designer
        }

        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            SelectCustomer();
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectCustomer();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void SelectCustomer()
        {
            if (ultraGrid1.ActiveRow != null)
            {
                try
                {
                    UltraGridCell aCell = ultraGrid1.ActiveRow.Cells["LedgerName"];
                    UltraGridCell aCellLedgerId = ultraGrid1.ActiveRow.Cells["LedgerID"];

                    if (aCell != null && aCellLedgerId != null && aCell.Value != null && aCellLedgerId.Value != null)
                    {
                        string name = aCell.Value.ToString();
                        int ledgerId = Convert.ToInt32(aCellLedgerId.Value.ToString());

                        // Set the static string for compatibility with existing code
                        SetValueForText1 = $"{name}|{ledgerId}";

                        // Set the selected customer properties
                        SelectedCustomerId = ledgerId;
                        SelectedCustomerName = name;

                        // Get the parent form
                        Form parentForm = this.Owner;
                        if (parentForm != null)
                        {
                            // Handle FrmReceipt
                            if (parentForm is PosBranch_Win.Accounts.FrmReceipt && string.Equals(frmName, "FrmReceipt", StringComparison.OrdinalIgnoreCase))
                            {
                                PosBranch_Win.Accounts.FrmReceipt receiptForm = (PosBranch_Win.Accounts.FrmReceipt)parentForm;
                                receiptForm.SetCustomerInfo(ledgerId, name);
                            }
                            // Handle FrmCreditNote
                            else if (parentForm is PosBranch_Win.Accounts.FrmCreditNote && string.Equals(frmName, "FrmCreditNote", StringComparison.OrdinalIgnoreCase))
                            {
                                PosBranch_Win.Accounts.FrmCreditNote creditNoteForm = (PosBranch_Win.Accounts.FrmCreditNote)parentForm;
                                creditNoteForm.SetCustomerInfo(ledgerId, name);
                            }
                            // Handle frmSalesInvoice
                            else if (parentForm is frmSalesInvoice && string.Equals(frmName, "frmSalesInvoice", StringComparison.OrdinalIgnoreCase))
                            {
                                frmSalesInvoice salesInvoice = (frmSalesInvoice)parentForm;
                                salesInvoice.SetCustomerInfo(name, ledgerId);
                            }
                            // Handle frmSalesReturn
                            else if (parentForm is frmSalesReturn && string.Equals(frmName, "frmSalesReturn", StringComparison.OrdinalIgnoreCase))
                            {
                                frmSalesReturn salesReturn = (frmSalesReturn)parentForm;
                                TextBox textBox2 = salesReturn.Controls.Find("textBox2", true).FirstOrDefault() as TextBox;
                                if (textBox2 != null)
                                {
                                    textBox2.Text = name;
                                    textBox2.Tag = ledgerId;
                                }
                            }
                        }

                        // Set DialogResult and close the form
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error selecting customer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
                    SelectCustomer();
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            FilterCustomers();
        }

        private void FilterCustomers()
        {
            try
            {
                if (fullDataTable == null) return;
                string searchText = textBoxsearch.Text.Trim();
                if (searchText == "Search customers...")
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
                        case "ID":
                            filter = $"CONVERT(LedgerID, 'System.String') LIKE '%{escapedSearchText}%'";
                            break;
                        case "Customer Name":
                            filter = $"LedgerName LIKE '%{escapedSearchText}%'";
                            break;
                        case "All":
                        default:
                            filter = $"CONVERT(LedgerID, 'System.String') LIKE '%{escapedSearchText}%' OR LedgerName LIKE '%{escapedSearchText}%'";
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
                UpdateStatus($"Error filtering customers: {ex.Message}");
            }
        }

        private void ultraPanel1_PaintClient(object sender, PaintEventArgs e)
        {
            // Not needed but kept for designer compatibility
        }

        private void frmCustomerDialog_KeyDown(object sender, KeyEventArgs e)
        {
            // NOTE: Arrow key navigation is handled natively by UltraGrid when it has focus.
            // Do NOT handle Up/Down arrow keys here when the grid is focused, as it causes 
            // double navigation (skipping rows). The grid's native navigation is sufficient.

            // Only handle arrow keys when the grid is NOT focused (for other controls if needed)
            // The grid's built-in navigation will handle Up/Down when it has focus.
        }

        private void ultraGrid1_AfterSelectChange(object sender, AfterSelectChangeEventArgs e)
        {
            if (ultraGrid1.ActiveRow != null)
            {
                try
                {
                    UltraGridCell aCellLedgerId = ultraGrid1.ActiveRow.Cells["LedgerID"];
                    if (aCellLedgerId != null && aCellLedgerId.Value != null)
                    {
                        // Check if the form that opened this dialog is frmSalesReturn
                        Form parentForm = this.Owner;
                        if (parentForm != null && parentForm is frmSalesReturn)
                        {
                            frmSalesReturn salesReturnForm = (frmSalesReturn)parentForm;

                            // Try to find label1 in the form
                            Label label1 = salesReturnForm.Controls.Find("label1", true).FirstOrDefault() as Label;
                            if (label1 != null)
                            {
                                label1.Text = aCellLedgerId.Value.ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating customer ID: " + ex.Message);
                }
            }
        }

        private void textBoxsearch_GotFocus(object sender, EventArgs e)
        {
            if (textBoxsearch.Text == "Search customers...")
            {
                textBoxsearch.Text = "";
                textBoxsearch.ForeColor = Color.Black;
            }
        }

        private void textBoxsearch_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxsearch.Text))
            {
                textBoxsearch.Text = "Search customers...";
                textBoxsearch.ForeColor = Color.Gray;
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

        // --- Column Chooser Feature (EXACT from frmdialForItemMaster, adapted for customers) ---
        private void SetupColumnChooserMenu()
        {
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += ColumnChooserMenuItem_Click;
            gridContextMenu.Items.Add(columnChooserMenuItem);
            ultraGrid1.ContextMenuStrip = gridContextMenu;
            SetupDirectHeaderDragDrop();
        }
        private void SetupDirectHeaderDragDrop()
        {
            ultraGrid1.AllowDrop = true;
            ultraGrid1.MouseDown += UltraGrid1_MouseDown;
            ultraGrid1.MouseMove += UltraGrid1_MouseMove;
            ultraGrid1.MouseUp += UltraGrid1_MouseUp;
            ultraGrid1.DragOver += UltraGrid1_DragOver;
            ultraGrid1.DragDrop += UltraGrid1_DragDrop;
            CreateColumnChooserForm();
        }
        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization",
                Size = new Size(220, 280),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 240, 240),
                ShowIcon = false,
                ShowInTaskbar = false
            };
            columnChooserForm.FormClosing += ColumnChooserForm_FormClosing;
            columnChooserForm.Shown += (s, e) => PositionColumnChooserAtBottomRight();
            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 240, 240),
                ItemHeight = 30,
                IntegralHeight = false
            };
            columnChooserListBox.DrawItem += (s, evt) =>
            {
                if (evt.Index < 0) return;
                ColumnItem item = columnChooserListBox.Items[evt.Index] as ColumnItem;
                if (item == null) return;
                Rectangle rect = evt.Bounds;
                rect.Inflate(-3, -3);
                Color bgColor = Color.FromArgb(33, 150, 243);
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        int radius = 4;
                        int diameter = radius * 2;
                        Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
                        path.AddArc(arcRect, 180, 90);
                        arcRect.X = rect.Right - diameter;
                        path.AddArc(arcRect, 270, 90);
                        arcRect.Y = rect.Bottom - diameter;
                        path.AddArc(arcRect, 0, 90);
                        arcRect.X = rect.Left;
                        path.AddArc(arcRect, 90, 90);
                        path.CloseFigure();
                        evt.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        evt.Graphics.FillPath(bgBrush, path);
                    }
                }
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat();
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;
                    evt.Graphics.DrawString(item.DisplayText, evt.Font, textBrush, rect, sf);
                }
                if ((evt.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    using (Pen focusPen = new Pen(Color.White, 1.5f))
                    {
                        focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        Rectangle focusRect = rect;
                        focusRect.Inflate(-2, -2);
                        evt.Graphics.DrawRectangle(focusPen, focusRect);
                    }
                }
            };
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;
            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserListBox.ScrollAlwaysVisible = false;
            PopulateColumnChooserListBox();
        }
        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(this.Right - columnChooserForm.Width - 20, this.Bottom - columnChooserForm.Height - 20);
                columnChooserForm.TopMost = true;
                columnChooserForm.BringToFront();
            }
        }
        private Point startPoint;
        private Infragistics.Win.UltraWinGrid.UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
        private void UltraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingColumn = false;
            columnToMove = null;
            startPoint = new Point(e.X, e.Y);
            if (e.Y < 40)
            {
                int xPos = 0;
                if (ultraGrid1.DisplayLayout.Override.RowSelectors == Infragistics.Win.DefaultableBoolean.True)
                {
                    xPos += ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
                }
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
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
        private void UltraGrid1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnToMove != null && isDraggingColumn)
            {
                int deltaX = Math.Abs(e.X - startPoint.X);
                int deltaY = Math.Abs(e.Y - startPoint.Y);
                if (deltaX > SystemInformation.DragSize.Width || deltaY > SystemInformation.DragSize.Height)
                {
                    bool isDraggingDown = (e.Y > startPoint.Y && deltaY > deltaX);
                    if (isDraggingDown)
                    {
                        ultraGrid1.Cursor = Cursors.No;
                        string columnName = !string.IsNullOrEmpty(columnToMove.Header.Caption) ? columnToMove.Header.Caption : columnToMove.Key;
                        toolTip.SetToolTip(ultraGrid1, $"Drag down to hide '{columnName}' column");
                        if (e.Y - startPoint.Y > 50)
                        {
                            HideColumn(columnToMove);
                            columnToMove = null;
                            isDraggingColumn = false;
                            ultraGrid1.Cursor = Cursors.Default;
                            toolTip.SetToolTip(ultraGrid1, "");
                        }
                    }
                }
            }
        }
        private void UltraGrid1_MouseUp(object sender, MouseEventArgs e)
        {
            ultraGrid1.Cursor = Cursors.Default;
            toolTip.SetToolTip(ultraGrid1, "");
            isDraggingColumn = false;
            columnToMove = null;
        }
        private void HideColumn(Infragistics.Win.UltraWinGrid.UltraGridColumn column)
        {
            if (column != null && !column.Hidden)
            {
                string columnName = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                savedColumnWidths[column.Key] = column.Width;
                ultraGrid1.SuspendLayout();
                column.Hidden = true;
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                    {
                        col.Width = savedColumnWidths[col.Key];
                    }
                }
                ultraGrid1.ResumeLayout();
                if (columnChooserForm == null || columnChooserForm.IsDisposed)
                {
                    CreateColumnChooserForm();
                }
                if (columnChooserListBox != null)
                {
                    bool alreadyExists = false;
                    foreach (object item in columnChooserListBox.Items)
                    {
                        if (item is ColumnItem columnItem && columnItem.ColumnKey == column.Key)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }
                    if (!alreadyExists)
                    {
                        columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                    }
                }
            }
        }
        private void ColumnChooserMenuItem_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }
        private void ShowColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Show();
                PositionColumnChooserAtBottomRight();
                return;
            }
            CreateColumnChooserForm();
            columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
            this.LocationChanged += (s, evt) => PositionColumnChooserAtBottomRight();
            this.SizeChanged += (s, evt) => PositionColumnChooserAtBottomRight();
            this.Activated += (s, evt) => PositionColumnChooserAtBottomRight();
        }
        private void ColumnChooserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (columnChooserListBox != null)
            {
                columnChooserListBox.MouseDown -= ColumnChooserListBox_MouseDown;
                columnChooserListBox.DragOver -= ColumnChooserListBox_DragOver;
                columnChooserListBox.DragDrop -= ColumnChooserListBox_DragDrop;
                columnChooserListBox = null;
            }
            UpdateStatus("Column customization closed");
        }
        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;
            columnChooserListBox.Items.Clear();
            string[] standardColumns = new string[] { "LedgerID", "LedgerName" };
            Dictionary<string, string> displayNames = new Dictionary<string, string>()
            {
                { "LedgerID", "ID" },
                { "LedgerName", "Customer Name" },
            };
            HashSet<string> addedColumns = new HashSet<string>();
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                foreach (string colKey in standardColumns)
                {
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colKey) &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns[colKey].Hidden &&
                        !addedColumns.Contains(colKey))
                    {
                        string displayText = displayNames.ContainsKey(colKey) ? displayNames[colKey] : colKey;
                        columnChooserListBox.Items.Add(new ColumnItem(colKey, displayText));
                        addedColumns.Add(colKey);
                    }
                }
            }
        }
        private class ColumnItem
        {
            public string ColumnKey { get; set; }
            public string DisplayText { get; set; }
            public ColumnItem(string columnKey, string displayText)
            {
                ColumnKey = columnKey;
                DisplayText = displayText;
            }
            public override string ToString() { return DisplayText; }
        }
        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                ColumnItem item = columnChooserListBox.Items[index] as ColumnItem;
                if (item != null)
                {
                    columnChooserListBox.SelectedIndex = index;
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }
        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)))
            {
                var column = (Infragistics.Win.UltraWinGrid.UltraGridColumn)e.Data.GetData(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn));
                if (column != null && !column.Hidden)
                {
                    string columnName = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                    column.Hidden = true;
                    columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                }
            }
        }
        private void UltraGrid1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void UltraGrid1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                ColumnItem item = (ColumnItem)e.Data.GetData(typeof(ColumnItem));
                if (item != null)
                {
                    if (ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                    {
                        var column = ultraGrid1.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                        ultraGrid1.SuspendLayout();
                        column.Hidden = false;
                        columnChooserListBox.Items.Remove(item);
                        ultraGrid1.ResumeLayout();
                        toolTip.Show($"Column '{item.DisplayText}' restored",
                                ultraGrid1,
                                ultraGrid1.PointToClient(Control.MousePosition),
                                2000);
                    }
                }
            }
        }

        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            comboBox1.Items.Add("ID");
            comboBox1.Items.Add("Customer Name");
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            textBoxsearch.TextChanged += TextBoxsearch_TextChanged;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterCustomers();
        }
        private void TextBoxsearch_TextChanged(object sender, EventArgs e)
        {
            FilterCustomers();
        }

        private void InitializeColumnOrderComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add("ID");
            comboBox2.Items.Add("Customer Name");
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedColumn = comboBox2.SelectedItem?.ToString() ?? "ID";
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
            List<string> columnsToShow = new List<string> { "LedgerID", "LedgerName" };
            // Move selected column to the front
            string columnKey = selectedColumn == "ID" ? "LedgerID" : "LedgerName";
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
                    if (colKey == "LedgerID")
                    {
                        col.Header.Caption = "ID";
                        if (col.Width <= 0) col.Width = 80;
                    }
                    else if (colKey == "LedgerName")
                    {
                        col.Header.Caption = "Customer Name";
                        if (col.Width <= 0) col.Width = 350;
                    }
                }
            }
            ultraGrid1.ResumeLayout();
            ultraGrid1.Refresh();
        }

        // --- Navigation Panels (Up/Down) ---
        private void ConnectNavigationPanelEvents()
        {
            ultraPanel3.Click += MoveRowUp;
            ultraPanel3.ClientArea.Click += MoveRowUp;
            ultraPictureBox5.Click += MoveRowUp;
            ultraPanel7.Click += MoveRowDown;
            ultraPanel7.ClientArea.Click += MoveRowDown;
            ultraPictureBox6.Click += MoveRowDown;

            // OK Button
            ultraPanel5.Click += (s, e) => SelectCustomer();
            ultraPictureBox1.Click += (s, e) => SelectCustomer();
            label5.Click += (s, e) => SelectCustomer();

            // Close Button
            ultraPanel6.Click += (s, e) => this.Close();
            ultraPictureBox2.Click += (s, e) => this.Close();
            label3.Click += (s, e) => this.Close();

            // New/Edit Button
            ultraPanel4.Click += OpenCustomerForm;
            ultraPictureBox3.Click += OpenCustomerForm;
            label4.Click += OpenCustomerForm;
        }

        /// <summary>
        /// Opens FrmCustomer in the Home form's UltraTabControl
        /// </summary>
        private void OpenCustomerForm(object sender, EventArgs e)
        {
            try
            {
                // Find the main Home form that contains tabControlMain
                Form homeForm = FindHomeForm();

                if (homeForm != null)
                {

                    // Get the tabControlMain from the Home form
                    var tabControlMain = GetTabControlFromHome(homeForm);

                    if (tabControlMain != null)
                    {
                        // Check if Customer Master tab already exists
                        foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
                        {
                            if (tab.Text == "Customer Master")
                            {
                                tabControlMain.SelectedTab = tab;

                                // Close the frmCustomerDialog form
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                                return;
                            }
                        }

                        // Create new tab using the same approach as Home.cs
                        string uniqueKey = $"Tab_{DateTime.Now.Ticks}_Customer Master";
                        var newTab = tabControlMain.Tabs.Add(uniqueKey, "Customer Master");

                        // Create and configure FrmCustomer for embedding (same as Home.cs)
                        FrmCustomer customerForm = new FrmCustomer();
                        customerForm.TopLevel = false;
                        customerForm.FormBorderStyle = FormBorderStyle.None;
                        customerForm.Dock = DockStyle.Fill;
                        customerForm.Visible = true;
                        customerForm.BackColor = SystemColors.Control;

                        // Ensure form is properly initialized
                        if (!customerForm.IsHandleCreated)
                        {
                            customerForm.CreateControl();
                        }

                        // Add the form to the tab page
                        newTab.TabPage.Controls.Add(customerForm);

                        // Show the form AFTER adding to tab page
                        customerForm.Show();
                        customerForm.BringToFront();

                        // Set the new tab as active/selected
                        tabControlMain.SelectedTab = newTab;

                        // Force refresh to ensure proper display
                        newTab.TabPage.Refresh();
                        customerForm.Refresh();
                        tabControlMain.Refresh();

                        // Wire up the form's FormClosed event to remove the tab
                        customerForm.FormClosed += (formSender, formE) =>
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
                            }
                        };


                        // Close the frmCustomerDialog form after successfully opening FrmCustomer
                        // Add a small delay to ensure FrmCustomer is fully loaded
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
                        FrmCustomer customerForm = new FrmCustomer();
                        customerForm.Show();

                        // Close the frmCustomerDialog form with delay
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
                    FrmCustomer customerForm = new FrmCustomer();
                    customerForm.Show();

                    // Close the frmCustomerDialog form with delay
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
                MessageBox.Show("Error opening Customer Master: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                        return form;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
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
                    return tabControl as Infragistics.Win.UltraWinTabControl.UltraTabControl;
                }

                return null;
            }
            catch (Exception ex)
            {
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
                return null;
            }
        }

        private void MoveRowUp(object sender, EventArgs e)
        {
            if (ultraGrid1.Rows.Count == 0) return;
            if (ultraGrid1.ActiveRow == null)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                return;
            }
            int currentIndex = ultraGrid1.ActiveRow.Index;
            if (currentIndex > 0)
            {
                var rowToActivate = ultraGrid1.Rows[currentIndex - 1];
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);
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
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                return;
            }
            int currentIndex = ultraGrid1.ActiveRow.Index;
            if (currentIndex < ultraGrid1.Rows.Count - 1)
            {
                var rowToActivate = ultraGrid1.Rows[currentIndex + 1];
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);
                ultraGrid1.ActiveRow = rowToActivate;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(rowToActivate);
            }
            UpdateRecordCountLabel();
        }

        // --- Sorting (Original/Reverse Order) ---
        private void UltraPictureBox4_Click(object sender, EventArgs e)
        {
            if (fullDataTable == null) return;

            isOriginalOrder = !isOriginalOrder;

            DataView dv = fullDataTable.DefaultView;
            if (isOriginalOrder)
            {
                dv.Sort = "OriginalRowOrder ASC";
                UpdateStatus("Displaying customers in original order.");
            }
            else
            {
                dv.Sort = "OriginalRowOrder DESC";
                UpdateStatus("Displaying customers in reverse order.");
            }
        }

        // --- Status Label Updates ---
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
            SetupPanelGroupHoverEffects(ultraPanel4, label4, ultraPictureBox3);
            SetupPanelGroupHoverEffects(ultraPanel6, label3, ultraPictureBox2);
            SetupPanelGroupHoverEffects(ultraPanel3, null, ultraPictureBox5);
            SetupPanelGroupHoverEffects(ultraPanel7, null, ultraPictureBox6);
        }
        private void SetupPanelGroupHoverEffects(
            Infragistics.Win.Misc.UltraPanel panel,
            Label label,
            Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox)
        {
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;
            Color hoverBackColor = BrightenColor(originalBackColor, 30);
            Color hoverBackColor2 = BrightenColor(originalBackColor2, 30);
            Action applyHoverEffect = () =>
            {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
                panel.ClientArea.Cursor = Cursors.Hand;
            };
            Action removeHoverEffect = () =>
            {
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
            return Color.FromArgb(
                color.A,
                Math.Min(color.R + amount,
           255),
                Math.Min(color.G + amount, 255),
                Math.Min(color.B + amount, 255));
        }
        private bool IsMouseOverControl(Control control)
        {

            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }


        /// <summary>
        /// Add Sort Toggle Icon (ultraPictureBox4)
        /// </summary>
        private void AddSortToggleIcon()
        {
            if (this.Controls.Find("ultraPictureBox4", true).FirstOrDefault() == null)
            {
                // ultraPictureBox4 is declared once as a private field above. Do not redeclare elsewhere.
                // Set image if available, else skip
                var sortImg = (object)null;
                try { sortImg = Properties.Resources.ResourceManager.GetObject("sort_az_16"); } catch { }
                if (sortImg is Image img)
                {
                    ultraPictureBox4.Appearance.Image = img;
                    ultraPictureBox4.Click += UltraPictureBox4_Click;
                    System.Windows.Forms.ToolTip tooltip = new System.Windows.Forms.ToolTip();
                    tooltip.SetToolTip(ultraPictureBox4, "Click to toggle between original order and reverse order");
                    this.Controls.Add(ultraPictureBox4);
                }
            }
        }
        private void FrmCustomerDialog_SizeChanged(object sender, EventArgs e)
        {
            PreserveColumnWidths();
            PositionColumnChooserAtBottomRight();
        }
        private void UltraGrid1_Resize(object sender, EventArgs e)
        {
            PreserveColumnWidths();
        }
        private void FrmCustomerDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Close();
                columnChooserForm = null;
            }
        }
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
    }
}
