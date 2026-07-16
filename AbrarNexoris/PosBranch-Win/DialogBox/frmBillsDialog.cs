﻿using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.TransactionModels;
using PosBranch_Win.Transaction;
using Repository;
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
    public partial class frmBillsDialog : Form
    {
        Int64 BillNoP = 0;
        Dropdowns drop = new Dropdowns();
        frmSalesReturn SalesReturn;
        frmSalesReturnItem returnList;
        SalesReturnRepository operations = new SalesReturnRepository();
        private SalesDDlGrid billsData;
        private readonly int customerId;
        private readonly string formName;
        private readonly Repository.TransactionRepository.SalesRepository salesRepository = new Repository.TransactionRepository.SalesRepository();
        private Label statusLabel;
        private DataTable fullDataTable = null;
        private Label lblStatus;
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private bool isOriginalOrder = true;
        public bool HasData { get; private set; } = false;

        public frmBillsDialog(int customerId, string formName)
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.customerId = customerId;
            this.formName = formName;

            System.Diagnostics.Debug.WriteLine($"frmBillsDialog constructor called with customerId: {customerId}, formName: {formName}");

            // Initialize basic features that don't depend on missing controls
            InitializeStatusLabel();
            SetupUltraGridStyle();

            // Only initialize features if the controls exist
            if (comboBox1 != null)
                InitializeSearchFilterComboBox();
            if (comboBox2 != null)
                InitializeColumnOrderComboBox();

            SetupColumnChooserMenu();
            SetupPanelHoverEffects();
            ConnectNavigationPanelEvents();

            // Add double click event handler
            ultraGrid1.DoubleClickCell += new DoubleClickCellEventHandler(ultraGrid1_DoubleClickCell);

            // Add form key down event handler
            this.KeyDown += new KeyEventHandler(frmBillsDialog_KeyDown);

            // Add event handlers for keyboard navigation and selection
            this.ultraGrid1.KeyDown += ultraGrid1_KeyDown;
            this.ultraGrid1.AfterSelectChange += ultraGrid1_AfterSelectChange;

            if (textBoxsearch != null)
            {
                this.textBoxsearch.KeyDown += textBoxsearch_KeyDown;
                this.textBoxsearch.TextChanged += textBoxsearch_TextChanged;
                // Add placeholder text handlers
                this.textBoxsearch.GotFocus += textBoxsearch_GotFocus;
                this.textBoxsearch.LostFocus += textBoxsearch_LostFocus;
                this.textBoxsearch.Text = "Search bills...";
            }

            // Add debug button
            AddDebugButton();

            // Add event wiring for resize/activation
            this.SizeChanged += FrmBillsDialog_SizeChanged;
            this.LocationChanged += (s, e) => PositionColumnChooserAtBottomRight();
            this.Activated += (s, e) => PositionColumnChooserAtBottomRight();
            this.FormClosing += FrmBillsDialog_FormClosing;
            this.ultraGrid1.Resize += UltraGrid1_Resize;

            // Wire up grid layout event for exact styling
            this.ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;

            // Load data
            LoadBills();

            System.Diagnostics.Debug.WriteLine($"frmBillsDialog constructor completed. HasData: {HasData}");
        }

        private void frmBillsDialog_Load(object sender, EventArgs e)
        {
            try
            {
                // Initialize the form controls and load data
                LoadBills();

                // Apply grid styling
                ConfigureGrid();

                // Add done button
                AddDoneButton();

                // Add sort toggle icon after form is loaded
                AddSortToggleIcon();

                // Set focus to search textbox if it exists
                if (textBoxsearch != null)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        textBoxsearch.Focus();
                        textBoxsearch.Select();
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading bills: " + ex.Message);
            }
        }

        private void ConfigureGrid()
        {
            try
            {
                // Enable GroupByBox and set its appearance
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = false;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = "Drag a column header here to group by that column";
                ultraGrid1.DisplayLayout.GroupByBox.Appearance.BackColor = Color.FromArgb(0, 122, 204);
                ultraGrid1.DisplayLayout.GroupByBox.Appearance.BackColor2 = Color.FromArgb(0, 102, 184);
                ultraGrid1.DisplayLayout.GroupByBox.Appearance.BackGradientStyle = GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.GroupByBox.Appearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.GroupByBox.Appearance.FontData.Bold = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.GroupByBox.BorderStyle = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.GroupByBox.PromptAppearance.BackColor = Color.FromArgb(0, 122, 204);
                ultraGrid1.DisplayLayout.GroupByBox.PromptAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.GroupByBox.PromptAppearance.FontData.Bold = DefaultableBoolean.True;

                // Configure the grid appearance with a modern gradient look
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
                ultraGrid1.DisplayLayout.Override.AllowGroupBy = DefaultableBoolean.True;

                // Configure grid lines
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

                // Add row spacing
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 2;

                // Configure header appearance with a modern gradient look
                ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(0, 122, 204); // Modern blue color
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(0, 102, 184); // Slightly darker blue for gradient
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

                // Configure row selector appearance to match header
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(0, 122, 204);
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = Color.FromArgb(0, 102, 184);
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;

                // Row appearance
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = SystemColors.Menu;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.Vertical;

                // Configure alternate row appearance
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(240, 245, 250);

                // Configure selected row appearance
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = SystemColors.Highlight;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = SystemColors.HighlightText;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;

                // Active row appearance - make it same as selected row
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = SystemColors.Highlight;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = SystemColors.HighlightText;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.True;

                // Configure spacing and expansion behavior
                ultraGrid1.DisplayLayout.InterBandSpacing = 10;
                ultraGrid1.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;

                // Hide specific columns
                foreach (UltraGridColumn column in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    // Hide CompanyId, BranchId, FinYearId, and CounterId columns
                    if (column.Key == "CompanyId" || column.Key == "BranchId" || column.Key == "FinYearId" || column.Key == "CounterId")
                    {
                        column.Hidden = true;
                    }
                }

                // Force the changes to take effect for all bands
                foreach (UltraGridBand band in ultraGrid1.DisplayLayout.Bands)
                {
                    band.Override.HeaderAppearance.BackColor = Color.FromArgb(0, 122, 204);
                    band.Override.HeaderAppearance.BackColor2 = Color.FromArgb(0, 102, 184);
                    band.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                }

                // Refresh the grid to apply all changes
                ultraGrid1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error configuring grid: " + ex.Message);
            }
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            SalesReturn = (frmSalesReturn)Application.OpenForms["frmSalesReturn"];

            // Check if the SalesReturn form is open
            if (SalesReturn == null)
            {
                MessageBox.Show("Sales Return form is not open", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UltraGridCell BillNo = this.ultraGrid1.ActiveRow.Cells["BillNo"];
            UltraGridCell BillDate = this.ultraGrid1.ActiveRow.Cells["BillDate"];
            UltraGridCell CustomerName = this.ultraGrid1.ActiveRow.Cells["CustomerName"];
            UltraGridCell LedgerID = this.ultraGrid1.ActiveRow.Cells["LedgerID"];
            UltraGridCell PaymodeId = this.ultraGrid1.ActiveRow.Cells["PaymodeId"];
            UltraGridCell NetAmount = this.ultraGrid1.ActiveRow.Cells["NetAmount"];
            UltraGridCell PaymodeName = this.ultraGrid1.ActiveRow.Cells["PaymodeName"];

            // Find the customerTextBox in the SalesReturn form
            TextBox customerTextBox = SalesReturn.Controls.Find("textBox2", true).FirstOrDefault() as TextBox;
            if (customerTextBox != null)
            {
                customerTextBox.Text = CustomerName.Value.ToString();
                // Store the LedgerID in the Tag property with safe conversion
                if (LedgerID.Value != null && LedgerID.Value != DBNull.Value)
                {
                    int ledgerId = 0;
                    if (int.TryParse(LedgerID.Value.ToString(), out ledgerId))
                    {
                        customerTextBox.Tag = ledgerId;
                    }
                }
            }

            SalesReturn.textBox1.Text = BillNo.Value.ToString();
            // Set the NetAmount in txtNetTotal to show the total amount
            SalesReturn.txtNetTotal.Text = NetAmount.Value.ToString();
            SalesReturn.dtInvoiceDate.Value = Convert.ToDateTime(BillDate.Value);
            // Set the Invoice Amount to match the Net Amount since there are no discounts/adjustments
            SalesReturn.TxtInvoiceAmnt.Text = NetAmount.Value.ToString();

            // Set the payment method in the combobox
            if (PaymodeId.Value != DBNull.Value &&
                PaymodeName.Value != DBNull.Value)
            {
                int paymodeId = Convert.ToInt32(PaymodeId.Value);
                string paymodeName = PaymodeName.Value.ToString();

                // Find and select the payment method in the combobox
                ComboBox cmbPaymntMethod = SalesReturn.Controls.Find("cmbPaymntMethod", true).FirstOrDefault() as ComboBox;
                if (cmbPaymntMethod != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Setting payment method - ID: {paymodeId}, Name: '{paymodeName}'");

                    // First refresh the payment methods to ensure the current method is available
                    try
                    {
                        // Use reflection to call RefreshPaymode method on the SalesReturn form
                        var refreshMethod = SalesReturn.GetType().GetMethod("RefreshPaymode");
                        if (refreshMethod != null)
                        {
                            refreshMethod.Invoke(SalesReturn, null);
                            System.Diagnostics.Debug.WriteLine("Refreshed payment methods dropdown");

                            // Allow UI to update
                            Application.DoEvents();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error refreshing payment methods: {ex.Message}");
                    }

                    // First try setting by ID
                    try
                    {
                        cmbPaymntMethod.SelectedValue = paymodeId;

                        // Check if selection was successful
                        if (cmbPaymntMethod.SelectedValue == null || Convert.ToInt32(cmbPaymntMethod.SelectedValue) != paymodeId)
                        {
                            // If not successful, try selecting by index
                            for (int i = 0; i < cmbPaymntMethod.Items.Count; i++)
                            {
                                DataRowView row = cmbPaymntMethod.Items[i] as DataRowView;
                                if (row != null && Convert.ToInt32(row["PayModeID"]) == paymodeId)
                                {
                                    cmbPaymntMethod.SelectedIndex = i;
                                    System.Diagnostics.Debug.WriteLine($"Selected payment by index: {i}");
                                    break;
                                }
                            }

                            // If still not set, try by name
                            if (cmbPaymntMethod.SelectedValue == null || Convert.ToInt32(cmbPaymntMethod.SelectedValue) != paymodeId)
                            {
                                for (int i = 0; i < cmbPaymntMethod.Items.Count; i++)
                                {
                                    DataRowView row = cmbPaymntMethod.Items[i] as DataRowView;
                                    if (row != null && row["PayModeName"].ToString().Equals(paymodeName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        cmbPaymntMethod.SelectedIndex = i;
                                        System.Diagnostics.Debug.WriteLine($"Selected payment by name: '{paymodeName}'");
                                        break;
                                    }
                                }
                            }

                            // If still not found, try to add the payment method to the dropdown
                            if (cmbPaymntMethod.SelectedValue == null ||
                                (cmbPaymntMethod.SelectedValue != null && Convert.ToInt32(cmbPaymntMethod.SelectedValue) != paymodeId))
                            {
                                try
                                {
                                    // Get the DataTable that is the current data source
                                    DataTable dt = cmbPaymntMethod.DataSource as DataTable;
                                    if (dt != null)
                                    {
                                        // Check if this payment method already exists
                                        bool exists = false;
                                        foreach (DataRow existingRow in dt.Rows)
                                        {
                                            if (Convert.ToInt32(existingRow["PayModeID"]) == paymodeId)
                                            {
                                                exists = true;
                                                break;
                                            }
                                        }

                                        // Add the payment method if it doesn't exist
                                        if (!exists)
                                        {
                                            DataRow newRow = dt.NewRow();
                                            newRow["PayModeID"] = paymodeId;
                                            newRow["PayModeName"] = paymodeName;
                                            dt.Rows.Add(newRow);

                                            // Update the dropdown and select the new item
                                            cmbPaymntMethod.DataSource = dt;

                                            // Try to select the newly added item by value
                                            cmbPaymntMethod.SelectedValue = paymodeId;
                                            System.Diagnostics.Debug.WriteLine($"Added and selected new payment method: {paymodeName}");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error adding payment method to dropdown: {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Selected payment by value successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error setting payment method: {ex.Message}");

                        // Last resort: try to select any valid payment method (not the first one, which is "Select Payment")
                        if (cmbPaymntMethod.Items.Count > 1)
                        {
                            cmbPaymntMethod.SelectedIndex = 1;
                            System.Diagnostics.Debug.WriteLine("Selected first valid payment method as fallback");
                        }
                    }

                    // Set a flag on the SalesReturn form to skip payment method reset
                    // Use reflection to access and set the private field
                    try
                    {
                        var skipResetField = SalesReturn.GetType().GetField("skipPaymentMethodReset",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (skipResetField != null)
                        {
                            skipResetField.SetValue(SalesReturn, true);
                            System.Diagnostics.Debug.WriteLine("Set skipPaymentMethodReset flag to true");

                            // Use a separate thread to reset the flag after a delay
                            new System.Threading.Thread(() =>
                            {
                                System.Threading.Thread.Sleep(2000); // 2 second delay
                                try
                                {
                                    skipResetField.SetValue(SalesReturn, false);
                                    System.Diagnostics.Debug.WriteLine("Reset skipPaymentMethodReset flag to false");
                                }
                                catch { }
                            }).Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error setting skipPaymentMethodReset flag: {ex.Message}");
                    }
                }
            }

            Int64 BNo = Convert.ToInt64(BillNo.Value.ToString());

            // Get the bill details
            SalesReturnDetailsGrid SRD = operations.GetByIdSRDWithAvailableQty(BNo);

            if (SRD != null && SRD.List != null && SRD.List.Count() > 0)
            {
                // Clear existing items in the SalesReturn ultraGrid
                ClearSalesReturnGrid();

                // Add each item directly to the SalesReturn grid
                foreach (var item in SRD.List)
                {
                    string itemId = item.ItemId.ToString();
                    string itemName = item.Description != null ? item.Description.ToString() : "";
                    string barcode = item.Barcode != null ? item.Barcode.ToString() : "";
                    string unit = item.Unit != null ? item.Unit.ToString() : "";

                    // Ensure proper type conversions
                    decimal unitPrice = 0;
                    decimal.TryParse(item.UnitPrice.ToString(), out unitPrice);

                    int qty = 1;
                    int.TryParse(item.Qty.ToString(), out qty);

                    decimal amount = 0;
                    decimal.TryParse(item.Amount.ToString(), out amount);

                    // Extract tax data from the item
                    double taxPer = item.TaxPer;
                    double taxAmt = item.TaxAmt;
                    string taxType = item.TaxType ?? "incl";

                    // Extract return quantity data
                    double returnedQty = 0;
                    double returnQty = 0;

                    // Get previously returned quantity from the item data
                    // Since these are double properties, they default to 0 if not set
                    returnedQty = item.ReturnedQty;
                    returnQty = item.ReturnQty;

                    // Add the item directly to the SalesReturn grid with all data including return quantities
                    SalesReturn.AddItemToGrid(itemId, itemName, barcode, unit, unitPrice, qty, amount,
                        taxPer, taxAmt, taxType, returnedQty, returnQty);
                }

                // Update status label to show bill was loaded
                if (statusLabel != null)
                {
                    statusLabel.Text = $"Bill #{BNo} loaded - {SRD.List.Count()} items added";
                    statusLabel.ForeColor = Color.Green;
                }

                // Automatically close the dialog after successful bill selection
                this.DialogResult = DialogResult.OK;

                // Add a small delay to show the status message before closing
                this.BeginInvoke(new Action(() =>
                {
                    System.Threading.Thread.Sleep(500); // 500ms delay
                    this.Close();
                }));
            }
            else
            {
                if (statusLabel != null)
                {
                    statusLabel.Text = $"No items found for Bill #{BNo}";
                    statusLabel.ForeColor = Color.Red;
                }
                MessageBox.Show("No items found for this bill", "No Items", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Clears the items in the SalesReturn ultraGrid
        /// </summary>
        private void ClearSalesReturnGrid()
        {
            // Access the SalesReturn form
            SalesReturn = (frmSalesReturn)Application.OpenForms["frmSalesReturn"];

            if (SalesReturn == null)
                return;

            // Find the ultraGrid in SalesReturn form
            UltraGrid ultraGrid = SalesReturn.Controls.Find("ultraGrid1", true).FirstOrDefault() as UltraGrid;

            if (ultraGrid == null)
                return;

            try
            {
                // Clear the grid by creating a new datatable/datasource
                if (ultraGrid.DataSource != null)
                {
                    // Check if the DataSource is a DataTable
                    if (ultraGrid.DataSource is DataTable)
                    {
                        DataTable dt = (DataTable)ultraGrid.DataSource;
                        dt.Rows.Clear();
                    }
                    else
                    {
                        // Try to use SalesReturn's clear method if it exists
                        SalesReturn.GetType().GetMethod("ClearItemsGrid")?.Invoke(SalesReturn, null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing grid: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                // Define bill columns to show (adapt as needed)
                string[] columnsToShow = new string[] { "BillNo", "CustomerName", "BillDate", "NetAmount", "ReturnStatus" };
                for (int i = 0; i < columnsToShow.Length; i++)
                {
                    string colKey = columnsToShow[i];
                    if (e.Layout.Bands[0].Columns.Exists(colKey))
                    {
                        var col = e.Layout.Bands[0].Columns[colKey];
                        col.Hidden = false;
                        col.Header.VisiblePosition = i;
                        SetColumnProperties(col, colKey);
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

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && ultraGrid1.ActiveRow != null)
            {
                // Select the current row and process it
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                KeyPressEventArgs keyPress = new KeyPressEventArgs((char)Keys.Enter);
                ultraGrid1_KeyPress(ultraGrid1, keyPress);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void LoadBills()
        {
            try
            {
                // Debug: Log the customer ID being used
                System.Diagnostics.Debug.WriteLine($"LoadBills: Loading bills for customer ID: {customerId}");

                // Use the enhanced method to get bills with return information
                var customerBillsWithReturnInfo = GetBillsForReturn(customerId);

                // Debug: Log the results
                System.Diagnostics.Debug.WriteLine($"LoadBills: Retrieved {customerBillsWithReturnInfo?.Count ?? 0} returnable bills for customer {customerId}");

                if (customerBillsWithReturnInfo != null && customerBillsWithReturnInfo.Count > 0)
                {
                    // Debug: Log first few bills
                    foreach (var bill in customerBillsWithReturnInfo.Take(3))
                    {
                        System.Diagnostics.Debug.WriteLine($"Bill: {bill.BillNo}, Customer: {bill.CustomerName}, Date: {bill.BillDate}, Amount: {bill.NetAmount}");
                    }

                    billsData = new SalesDDlGrid { List = customerBillsWithReturnInfo };
                    fullDataTable = ToDataTable<SalesDDl>(billsData.List.ToList());
                    PreserveOriginalRowOrder(fullDataTable);
                    ultraGrid1.DataSource = fullDataTable;

                    if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                    {
                        if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("BillNo"))
                        {
                            ultraGrid1.DisplayLayout.Bands[0].Columns["BillNo"].Width = 100;
                        }
                        if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("CustomerName"))
                        {
                            ultraGrid1.DisplayLayout.Bands[0].Columns["CustomerName"].Width = 250;
                        }
                        if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("BillDate"))
                        {
                            ultraGrid1.DisplayLayout.Bands[0].Columns["BillDate"].Width = 120;
                        }
                        if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("NetAmount"))
                        {
                            ultraGrid1.DisplayLayout.Bands[0].Columns["NetAmount"].Width = 100;
                        }
                        if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("ReturnStatus"))
                        {
                            ultraGrid1.DisplayLayout.Bands[0].Columns["ReturnStatus"].Width = 150;
                        }
                    }

                    if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                    }

                    InitializeSavedColumnWidths();
                    UpdateRecordCountLabel();
                    UpdateStatus($"Loaded {fullDataTable.Rows.Count} returnable bills for customer ID {customerId}.");
                    HasData = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"LoadBills: No returnable bills found for customer {customerId}");

                    // Clear any existing data
                    ultraGrid1.DataSource = null;

                    // Show message to user
                    MessageBox.Show("No invoices found for this customer.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    UpdateStatus($"No invoices available for return for customer ID {customerId}");
                    HasData = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadBills: Error loading returnable invoices for customer {customerId}: {ex.Message}");
                UpdateStatus($"Error loading returnable invoices for customer {customerId}: {ex.Message}");
                HasData = false;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            FilterBills();
        }

        private void textBoxsearch_TextChanged(object sender, EventArgs e)
        {
            FilterBills();
        }

        private void textBoxsearch_KeyDown(object sender, KeyEventArgs e)
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
                    // Process the selection - dialog will automatically close
                    KeyPressEventArgs keyPress = new KeyPressEventArgs((char)Keys.Enter);
                    ultraGrid1_KeyPress(ultraGrid1, keyPress);
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

        private void FilterBills()
        {
            try
            {
                if (fullDataTable == null) return;
                string searchText = textBoxsearch.Text.Trim();
                if (searchText == "Search bills...")
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
                        case "Bill No":
                            filter = $"CONVERT(BillNo, 'System.String') LIKE '%{escapedSearchText}%'";
                            break;
                        case "Customer Name":
                            filter = $"CustomerName LIKE '%{escapedSearchText}%'";
                            break;
                        case "All":
                        default:
                            filter = $"CONVERT(BillNo, 'System.String') LIKE '%{escapedSearchText}%' OR CustomerName LIKE '%{escapedSearchText}%'";
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
                UpdateStatus($"Error filtering bills: {ex.Message}");
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Handle arrow key navigation in textbox
            if (ActiveControl == textBoxsearch)
            {
                if (keyData == Keys.Down)
                {
                    // Move focus to grid on down arrow
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.Focus();
                        if (ultraGrid1.ActiveRow == null && ultraGrid1.Rows.Count > 0)
                        {
                            ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                        }
                    }
                    return true;
                }
                else if (keyData == Keys.Enter)
                {
                    // If there's only one row in filtered results, select it
                    if (ultraGrid1.Rows.Count == 1)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);

                        // Process the selection with the updated method that adds items directly
                        KeyPressEventArgs keyPress = new KeyPressEventArgs((char)Keys.Enter);
                        ultraGrid1_KeyPress(ultraGrid1, keyPress);
                        // Dialog will automatically close in ultraGrid1_KeyPress method
                        return true;
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ultraGrid1_DoubleClickCell(object sender, DoubleClickCellEventArgs e)
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    // Select the current row and process it
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);

                    // Process the selection with the updated ultraGrid1_KeyPress method
                    KeyPressEventArgs keyPress = new KeyPressEventArgs((char)Keys.Enter);
                    ultraGrid1_KeyPress(ultraGrid1, keyPress);

                    // Dialog will automatically close in ultraGrid1_KeyPress method
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling double click: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmBillsDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                e.Handled = true;
            }
        }

        private void AddDoneButton()
        {
            // Done button functionality is already handled by the OK button in the designer
            // Just initialize the status label
            if (statusLabel == null)
            {
                statusLabel = new Label();
                statusLabel.AutoSize = true;
                statusLabel.Location = new Point(260, 415);
                statusLabel.ForeColor = Color.FromArgb(0, 122, 204);
                statusLabel.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
                statusLabel.Text = "Select a bill to load items";
                statusLabel.BackColor = Color.Transparent;
                this.ultPanelPurchaseDisplay.ClientArea.Controls.Add(statusLabel);
            }
        }

        private void doneButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Helper method to get bills available for return with enhanced filtering
        private List<SalesDDl> GetBillsForReturn(int customerId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetBillsForReturn: Starting for customer {customerId}");

                // Get all bills for the customer using the repository method that uses stored procedure
                var allCustomerBills = salesRepository.GetBillsByCustomer(customerId);

                System.Diagnostics.Debug.WriteLine($"GetBillsForReturn: Retrieved {allCustomerBills?.Count ?? 0} bills from repository");

                if (allCustomerBills == null || !allCustomerBills.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"GetBillsForReturn: No bills found for customer {customerId}");
                    return new List<SalesDDl>();
                }

                // Convert SalesMaster objects to SalesDDl objects
                var salesDDlBills = allCustomerBills.Select(bill => new SalesDDl
                {
                    BillNo = bill.BillNo,
                    BillDate = bill.BillDate,
                    CustomerName = bill.CustomerName,
                    LedgerID = bill.LedgerID,
                    PaymodeId = bill.PaymodeId,
                    NetAmount = bill.NetAmount,
                    PaymodeName = bill.PaymodeName
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"GetBillsForReturn: Converted to {salesDDlBills.Count} SalesDDl objects");

                return salesDDlBills;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetBillsForReturn: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return new List<SalesDDl>();
            }
        }

        // Helper method to enrich bills with return information (temporary - will be moved to stored procedure)
        private List<SalesDDl> GetBillsWithReturnInfo(List<SalesMaster> customerBills)
        {
            return customerBills.Select(sm => new SalesDDl
            {
                BillNo = sm.BillNo,
                CustomerName = sm.CustomerName,
                BillDate = sm.BillDate,
                NetAmount = sm.NetAmount,
                LedgerID = sm.LedgerID,
                PaymodeId = sm.PaymodeId,
                PaymodeName = sm.PaymodeName
            }).ToList();
        }

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

        // Status Label Methods
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

        // --- Advanced Grid Styling (ported from frmCustomerDialog) ---
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

        // --- Column Chooser Feature (EXACT from frmCustomerDialog, adapted for bills) ---
        private void SetupColumnChooserMenu()
        {
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += ColumnChooserMenuItem_Click;
            gridContextMenu.Items.Add(columnChooserMenuItem);
            ultraGrid1.ContextMenuStrip = gridContextMenu;
            SetupDirectHeaderDragDrop();
        }

        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            comboBox1.Items.Add("Bill No");
            comboBox1.Items.Add("Customer Name");
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterBills();
        }

        private void InitializeColumnOrderComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Bill No");
            comboBox2.Items.Add("Customer Name");
            comboBox2.Items.Add("Bill Date");
            comboBox2.Items.Add("Net Amount");
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
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
            string[] standardColumns = new string[] { "BillNo", "CustomerName", "BillDate", "NetAmount" };
            Dictionary<string, string> displayNames = new Dictionary<string, string>()
            {
                { "BillNo", "Bill No" },
                { "CustomerName", "Customer Name" },
                { "BillDate", "Bill Date" },
                { "NetAmount", "Net Amount" },
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

        private void SetupPanelHoverEffects()
        {
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
                Math.Min(color.R + amount, 255),
                Math.Min(color.G + amount, 255),
                Math.Min(color.B + amount, 255));
        }

        private bool IsMouseOverControl(Control control)
        {
            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }

        private void ConnectNavigationPanelEvents()
        {
            ultraPanel3.Click += MoveRowUp;
            ultraPanel3.ClientArea.Click += MoveRowUp;
            ultraPictureBox5.Click += MoveRowUp;
            ultraPanel7.Click += MoveRowDown;
            ultraPanel7.ClientArea.Click += MoveRowDown;
            ultraPictureBox6.Click += MoveRowDown;

            // OK Button
            ultraPanel5.Click += (s, e) => SelectBill();
            ultraPictureBox1.Click += (s, e) => SelectBill();
            label5.Click += (s, e) => SelectBill();

            // Close Button
            ultraPanel6.Click += (s, e) => this.Close();
            ultraPictureBox2.Click += (s, e) => this.Close();
            label3.Click += (s, e) => this.Close();



        }

        private void SelectBill()
        {
            if (ultraGrid1.ActiveRow != null)
            {
                try
                {
                    // Process the selection with the updated ultraGrid1_KeyPress method
                    KeyPressEventArgs keyPress = new KeyPressEventArgs((char)Keys.Enter);
                    ultraGrid1_KeyPress(ultraGrid1, keyPress);

                    // Automatically close the dialog after successful selection
                    this.DialogResult = DialogResult.OK;

                    // Add a small delay to show the status message before closing
                    this.BeginInvoke(new Action(() =>
                    {
                        System.Threading.Thread.Sleep(500); // 500ms delay
                        this.Close();
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error selecting bill: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenBillForm(object sender, EventArgs e)
        {
            // This could open a new bill form or edit existing bill
            UpdateStatus("Bill form functionality not implemented yet.");
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
                string debugInfo = $"Data status:\n" +
                    $"- Total rows in data: {fullDataTable.Rows.Count}\n" +
                    $"- Visible rows in grid: {visibleRows}\n" +
                    $"- {filterStatus}\n" +
                    $"- Search text: '{textBoxsearch.Text}'";
                MessageBox.Show(debugInfo, "Search Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Debug error: {ex.Message}");
            }
        }

        private void AddSortToggleIcon()
        {
            try
            {
                // ultraPictureBox4 is already defined in the designer, just wire up the event
                if (ultraPictureBox4 != null)
                {
                    ultraPictureBox4.Click += UltraPictureBox4_Click;
                    ultraPictureBox4.Cursor = Cursors.Hand;

                    // Add tooltip
                    System.Windows.Forms.ToolTip tooltip = new System.Windows.Forms.ToolTip();
                    tooltip.SetToolTip(ultraPictureBox4, "Click to toggle between original order and reverse order");

                    System.Diagnostics.Debug.WriteLine("ultraPictureBox4 event handler wired successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ultraPictureBox4 is null!");
                }

                // Also wire up the parent panel (ultraPanel9) for easier clicking
                if (ultraPanel9 != null)
                {
                    ultraPanel9.Click += UltraPictureBox4_Click;
                    ultraPanel9.Cursor = Cursors.Hand;

                    System.Windows.Forms.ToolTip panelTooltip = new System.Windows.Forms.ToolTip();
                    panelTooltip.SetToolTip(ultraPanel9, "Click to toggle between original order and reverse order");

                    System.Diagnostics.Debug.WriteLine("ultraPanel9 event handler wired successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ultraPanel9 is null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddSortToggleIcon: {ex.Message}");
                UpdateStatus($"Error setting up sort toggle: {ex.Message}");
            }
        }

        // --- Sorting (Original/Reverse Order) ---
        private void UltraPictureBox4_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("UltraPictureBox4_Click triggered!");

                if (fullDataTable == null)
                {
                    UpdateStatus("No data available for sorting.");
                    return;
                }

                isOriginalOrder = !isOriginalOrder;

                DataView dv = fullDataTable.DefaultView;
                if (isOriginalOrder)
                {
                    dv.Sort = "OriginalRowOrder ASC";
                    UpdateStatus("Displaying bills in original order.");
                }
                else
                {
                    dv.Sort = "OriginalRowOrder DESC";
                    UpdateStatus("Displaying bills in reverse order.");
                }

                // Refresh the grid to show the new sort order
                ultraGrid1.Refresh();

                System.Diagnostics.Debug.WriteLine($"Sort order changed to: {(isOriginalOrder ? "Original" : "Reverse")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UltraPictureBox4_Click: {ex.Message}");
                UpdateStatus($"Error sorting data: {ex.Message}");
            }
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedColumn = comboBox2.SelectedItem?.ToString() ?? "Bill No";
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
            List<string> columnsToShow = new List<string> { "BillNo", "CustomerName", "BillDate", "NetAmount" };
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
                case "Bill No": return "BillNo";
                case "Customer Name": return "CustomerName";
                case "Bill Date": return "BillDate";
                case "Net Amount": return "NetAmount";
                default: return "BillNo";
            }
        }

        private void SetColumnProperties(Infragistics.Win.UltraWinGrid.UltraGridColumn col, string colKey)
        {
            switch (colKey)
            {
                case "BillNo":
                    col.Header.Caption = "Bill No";
                    if (col.Width <= 0) col.Width = 100;
                    break;
                case "CustomerName":
                    col.Header.Caption = "Customer Name";
                    if (col.Width <= 0) col.Width = 250;
                    break;
                case "BillDate":
                    col.Header.Caption = "Bill Date";
                    if (col.Width <= 0) col.Width = 120;
                    break;
                case "NetAmount":
                    col.Header.Caption = "Net Amount";
                    if (col.Width <= 0) col.Width = 100;
                    break;
            }
        }

        private void FrmBillsDialog_SizeChanged(object sender, EventArgs e)
        {
            PreserveColumnWidths();
        }

        private void UltraGrid1_Resize(object sender, EventArgs e)
        {
            PreserveColumnWidths();
        }

        private void FrmBillsDialog_FormClosing(object sender, FormClosingEventArgs e)
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

        private void textBoxsearch_GotFocus(object sender, EventArgs e)
        {
            if (textBoxsearch.Text == "Search bills...")
            {
                textBoxsearch.Text = "";
                textBoxsearch.ForeColor = Color.Black;
            }
        }

        private void textBoxsearch_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxsearch.Text))
            {
                textBoxsearch.Text = "Search bills...";
                textBoxsearch.ForeColor = Color.Gray;
            }
        }

        private void ultraGrid1_AfterSelectChange(object sender, AfterSelectChangeEventArgs e)
        {
            // Implementation for after select change
        }

    }
}
