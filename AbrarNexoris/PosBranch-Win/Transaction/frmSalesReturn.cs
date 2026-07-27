using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.TransactionModels;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.SettingsRepo;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace PosBranch_Win.Transaction
{
    public partial class frmSalesReturn : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        SalesReturnDetails SRDetails = new SalesReturnDetails();
        SalesReturn SReturn = new SalesReturn();
        //SMaster Sm = new SMaster();
        SalesReturnRepository operations = new SalesReturnRepository();
        bool CheckExists = false;
        Dropdowns dp = new Dropdowns();

        // Constants for default values
        private const int DEFAULT_UNIT_ID = 1;
        private const decimal DEFAULT_PACKING = 1m;
        private const decimal DEFAULT_COST = 0m;
        private const string DEFAULT_UNIT_NAME = "PCS";
        private const string DEFAULT_REASON = "Select Reason";

        // Add a reference to the customer TextBox and Button
        private Infragistics.Win.UltraWinEditors.UltraTextEditor customerTextBox;
        private System.Windows.Forms.Button customerButton;

        // In the class, add a new field to control payment method reset
        private bool skipPaymentMethodReset = false;

        // Add these member variables to track the current data source
        private enum DataSource
        {
            None,
            Bill,
            Barcode,
            DirectItem
        }

        private DataSource currentDataSource = DataSource.None;

        // --- Column Chooser and Drag-to-Hide Logic fields ---
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private Point startPoint;
        private Infragistics.Win.UltraWinGrid.UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;
        private System.Windows.Forms.ToolTip columnToolTip = new System.Windows.Forms.ToolTip();

        // --- UltraGrid Footer Summary Panel Logic fields ---
        // NOTE: gridFooterPanel is defined in the Designer file, so we reference it directly
        private Infragistics.Win.Misc.UltraPanel summaryFooterPanel;
        private Dictionary<string, Label> summaryLabels = new Dictionary<string, Label>();
        private string currentSummaryType = "None";
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };
        private Dictionary<string, string> columnAggregations = new Dictionary<string, string>();

        public frmSalesReturn()
        {
            InitializeComponent();
            InitializeUltraGrid();

            // Add additional grid events for automatic editing
            AddAdditionalGridEvents();

            // Add double click event handler for the grid
            ultraGrid1.DoubleClickCell += new DoubleClickCellEventHandler(ultraGrid1_DoubleClickCell);

            // Create and configure the customer TextBox if it doesn't exist
            if (Controls.Find("textBox2", true).Length == 0)
            {
                customerTextBox = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
                customerTextBox.Name = "textBox2";
                customerTextBox.Location = new Point(120, 80); // Adjust position as needed
                customerTextBox.Size = new Size(200, 20);
                customerTextBox.ReadOnly = true;
                customerTextBox.Value = "Select Customer";

                // Add a label for the customer TextBox
                Label customerLabel = new Label();
                customerLabel.Text = "Customer:";
                customerLabel.Location = new Point(20, 83); // Adjust position as needed
                customerLabel.AutoSize = true;

                // Create and configure the customer Button
                customerButton = new System.Windows.Forms.Button();
                customerButton.Name = "button2";
                customerButton.Text = "...";
                customerButton.Location = new Point(325, 79); // Adjust position as needed
                customerButton.Size = new Size(30, 23);
                customerButton.Click += new EventHandler(button2_Click_1);

                // Add controls to the form
                Controls.Add(customerLabel);
                Controls.Add(customerTextBox);
                Controls.Add(customerButton);
            }
            else
            {
                // Get references to existing controls
                customerTextBox = (Infragistics.Win.UltraWinEditors.UltraTextEditor)Controls.Find("textBox2", true)[0];
                customerButton = (System.Windows.Forms.Button)Controls.Find("button2", true)[0];

                // Set initial state
                customerTextBox.ReadOnly = true;
                customerTextBox.Value = "Select Customer";
                customerTextBox.Tag = null;
            }

            // Add event handlers for controls
            ultraPictureBox1.Click += new EventHandler(ultraPictureBox1_Click);
            ultraLabel1.Click += new EventHandler(ultraLabel1_Click);
            ultraLabel3.Click += new EventHandler(ultraLabel3_Click);
            pbxExit.Click += new EventHandler(pbxExit_Click);

            // Setup column chooser and summary footer panel (like frmSalesInvoice)
            SetupColumnChooserMenu();
            InitializeGridFooterPanel();
            InitializeSummaryFooterPanel();
        }
        private void FormatGrid()
        {
            try
            {
                // Create a DataTable to hold the grid data
                DataTable dt = new DataTable();
                dt.Columns.Add("SlNo", typeof(int));
                dt.Columns.Add("ItemId", typeof(int));
                dt.Columns.Add("ItemName", typeof(string));
                dt.Columns.Add("Barcode", typeof(string));
                dt.Columns.Add("Unit", typeof(string));
                dt.Columns.Add("UnitPrice", typeof(decimal));
                dt.Columns.Add("Qty", typeof(int));
                dt.Columns.Add("ReturnQty", typeof(decimal)); // New column for current return quantity
                dt.Columns.Add("ReturnedQty", typeof(decimal)); // New column for previously returned quantity
                dt.Columns.Add("Packing", typeof(decimal));
                dt.Columns.Add("Cost", typeof(decimal));
                dt.Columns.Add("Reason", typeof(string));
                dt.Columns.Add("Amount", typeof(decimal));
                dt.Columns.Add("TaxPer", typeof(decimal)); // Tax percentage column
                dt.Columns.Add("TaxAmt", typeof(decimal)); // Tax amount column
                dt.Columns.Add("TaxType", typeof(string)); // Tax type column (incl/excl)
                dt.Columns.Add("BaseAmount", typeof(decimal)); // Taxable value before tax for GST compliance
                dt.Columns.Add("Select", typeof(bool));

                // Set the DataSource
                ultraGrid1.DataSource = dt;

                // Define colors for light blue styling (matching FrmPurchase.cs)
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders and grid lines
                Color selectedBlue = Color.FromArgb(173, 216, 255); // Light blue for selection
                Color lightYellow = Color.FromArgb(255, 255, 224); // Light yellow for read-only historical data
                Color lightGreen = Color.FromArgb(224, 255, 224); // Light green for user-entered current values

                // Configure the grid appearance
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                // Apply compact design
                ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 22;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellPadding = 2;
                ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Appearance.BorderColor = lightBlue;

                // Configure header appearance - keep existing header color but apply modern styling
                ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = Color.SteelBlue; // Keep existing header color
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False; // Regular font weight
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Segoe UI";
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = lightBlue;

                // Configure row appearance with light blue styling
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.FromArgb(250, 250, 250);
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;

                // Configure alternate row appearance
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(250, 250, 250);

                // Set selected row appearance with light blue highlighting
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = selectedBlue;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = selectedBlue;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.Black;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;

                // Active row appearance - same as selected row
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = selectedBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = selectedBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.Black;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.False;

                // Configure cell and row borders
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Segoe UI";
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Bold = DefaultableBoolean.False;

                // Configure individual columns
                foreach (UltraGridColumn column in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    switch (column.Key)
                    {
                        case "SlNo":
                            column.Header.Caption = "SlNo";
                            column.Width = 100;
                            break;
                        case "ItemId":
                            column.Header.Caption = "ItemId";
                            column.Hidden = true;
                            break;
                        case "ItemName":
                            column.Header.Caption = "Item Name";
                            column.Width = 200;
                            break;
                        case "Barcode":
                            column.Header.Caption = "Barcode";
                            column.Width = 100;
                            break;
                        case "Unit":
                            column.Header.Caption = "Unit";
                            column.Width = 80;
                            column.CellActivation = Activation.AllowEdit;
                            break;
                        case "UnitPrice":
                            column.Header.Caption = "Unit Price";
                            column.Width = 100;
                            column.Format = "N2";
                            column.CellAppearance.TextHAlign = HAlign.Right;
                            column.CellActivation = Activation.AllowEdit;
                            break;
                        case "Qty":
                            column.Header.Caption = "Qty";
                            column.Width = 80;
                            column.CellAppearance.TextHAlign = HAlign.Right;
                            column.Format = "N2";
                            column.CellActivation = Activation.NoEdit; // Read-only - original sold quantity
                            column.CellAppearance.BackColor = lightYellow; // Light yellow for historical data
                            break;
                        case "ReturnQty":
                            column.Header.Caption = "Return Qty";
                            column.Width = 100;
                            column.CellAppearance.TextHAlign = HAlign.Right;
                            column.Format = "N2";
                            column.CellActivation = Activation.AllowEdit; // Editable - current return quantity
                            column.CellAppearance.BackColor = lightGreen; // Light green for user-entered values
                            break;
                        case "ReturnedQty":
                            column.Header.Caption = "Returned Qty";
                            column.Width = 100;
                            column.CellAppearance.TextHAlign = HAlign.Right;
                            column.Format = "N2";
                            column.CellActivation = Activation.NoEdit; // Read-only - previously returned quantity
                            column.CellAppearance.BackColor = lightYellow; // Light yellow for historical data
                            break;
                        case "Packing":
                            column.Header.Caption = "Packing";
                            column.Width = 100;
                            column.CellAppearance.TextHAlign = HAlign.Right;
                            column.Format = "N2";
                            column.CellActivation = Activation.AllowEdit;
                            column.Hidden = true; // Hide the column like in frmSalesInvoice
                            break;
                        case "Cost":
                            column.Header.Caption = "Cost";
                            column.Width = 100;
                            column.CellAppearance.TextHAlign = HAlign.Right;
                            column.Format = "N2";
                            column.CellActivation = Activation.AllowEdit;
                            column.Hidden = true; // Hide the column like Packing
                            break;
                        case "Reason":
                            column.Header.Caption = "Reason";
                            column.Width = 150;
                            // Make Reason editable
                            column.CellActivation = Activation.AllowEdit;
                            column.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownList;

                            // Create a dropdown values
                            Infragistics.Win.ValueList valueList = new Infragistics.Win.ValueList();
                            valueList.ValueListItems.Add(new Infragistics.Win.ValueListItem("Select Reason"));
                            valueList.ValueListItems.Add(new Infragistics.Win.ValueListItem("Damaged"));
                            valueList.ValueListItems.Add(new Infragistics.Win.ValueListItem("Expired"));
                            valueList.ValueListItems.Add(new Infragistics.Win.ValueListItem("Other"));

                            // Set the default item
                            valueList.ValueListItems[0].DataValue = "Select Reason";

                            // Assign the dropdown to the column
                            column.ValueList = valueList;
                            break;
                        case "Amount":
                            column.Header.Caption = "Amount";
                            column.Width = 100;
                            column.Format = "N2";
                            column.CellAppearance.TextHAlign = HAlign.Right;
                            column.CellActivation = Activation.NoEdit;
                            break;
                        case "TaxPer":
                            column.Header.Caption = "Tax %";
                            column.Width = 80;
                            column.CellAppearance.TextHAlign = HAlign.Right;
                            column.Format = "N2";
                            column.CellActivation = Activation.NoEdit;
                            // Show tax percentage column so users can see tax rates
                            column.Hidden = false;
                            break;
                        case "TaxAmt":
                            column.Header.Caption = "Tax Amount";
                            column.Width = 100;
                            column.CellAppearance.TextHAlign = HAlign.Right;
                            column.Format = "N2";
                            column.CellActivation = Activation.NoEdit;
                            break;
                        case "TaxType":
                            column.Header.Caption = "Tax Type";
                            column.Width = 80;
                            column.CellActivation = Activation.NoEdit;
                            column.Hidden = true; // Hide by default - technical column
                            break;
                        case "BaseAmount":
                            column.Header.Caption = "Base Amount";
                            column.Width = 100;
                            column.CellAppearance.TextHAlign = HAlign.Right;
                            column.Format = "N2";
                            column.CellActivation = Activation.NoEdit;
                            column.Hidden = true; // Hide by default - for GST reports only
                            break;
                        case "Select":
                            // Remove caption text for "Select" column, only show checkbox
                            column.Header.Caption = "";
                            column.Width = 70;
                            column.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                            column.CellActivation = Activation.AllowEdit;
                            // Add a checkbox to the header for "Select All" functionality
                            column.Header.CheckBoxVisibility = HeaderCheckBoxVisibility.Always;
                            // Add a handler for the header checkbox click
                            column.Header.CheckBoxAlignment = HeaderCheckBoxAlignment.Center;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error formatting grid: " + ex.Message);
            }
        }
        private void frmSalesReturn_Load(object sender, EventArgs e)
        {
            try
            {
                // Initialize references to key controls
                Control[] updateButtons = this.Controls.Find("updtbtn", true);
                if (updateButtons.Length > 0)
                {
                    Button updtbtn = updateButtons[0] as Button;
                    if (updtbtn != null)
                    {
                        // Initially hide the update button
                        updtbtn.Visible = false;
                    }
                }

                // Adjust form size to fit screen
                this.Size = Screen.PrimaryScreen.WorkingArea.Size;
                this.Location = Screen.PrimaryScreen.WorkingArea.Location;

                KeyPreview = true;

                // Load data in the proper sequence to ensure dependencies are met
                this.RefreshPaymode();

                // Add a delay to ensure payment methods are loaded before continuing
                Application.DoEvents();

                this.RefreshInvoice();
                this.RefreshCustomer();

                // Set up default customer like frmSalesInvoice
                SetupDefaultCustomer();

                this.FormatGrid();

                int SReturnNo = operations.GenerateSReturnNo();
                TxtSRNO.Value = SReturnNo.ToString();

                // Set tab order
                SetTabOrder();

                // Set initial focus to button2
                this.ActiveControl = button2;

                // Set default payment method to Cash
                SetupDefaultPaymentMode();

                // Register all event handlers
                AddEventHandlers();

                // Load custom DS-Digital font from embedded resources
                try
                {
                    Utilities.CustomFontLoader.Initialize();
                    txtNetTotal.Font = Utilities.CustomFontLoader.GetDSDigitalFont(36, FontStyle.Bold);
                }
                catch (Exception fontEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load custom font: {fontEx.Message}");
                    // Font will fall back to default if loading fails
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading form: " + ex.Message);
            }
        }

        private void SetTabOrder()
        {
            // Set tab indexes for controls in the specified order
            button2.TabIndex = 1;
            cmbPaymntMethod.TabIndex = 2;
            dtSReturnDate.TabIndex = 3;
            TxtBarcode.TabIndex = 4;
            btn_Add_Custm.TabIndex = 5;
            button1.TabIndex = 6;
            dtInvoiceDate.TabIndex = 7;
        }

        // Override ProcessTabKey to implement custom tab order
        protected override bool ProcessTabKey(bool forward)
        {
            Control currentControl = this.ActiveControl;

            if (forward)
            {
                if (currentControl == button2)
                {
                    cmbPaymntMethod.Focus();
                    return true;
                }
                else if (currentControl == cmbPaymntMethod)
                {
                    dtSReturnDate.Focus();
                    return true;
                }
                else if (currentControl == dtSReturnDate)
                {
                    TxtBarcode.Focus();
                    return true;
                }
                else if (currentControl == TxtBarcode)
                {
                    btn_Add_Custm.Focus();
                    return true;
                }
                else if (currentControl == btn_Add_Custm)
                {
                    button1.Focus();
                    return true;
                }
                else if (currentControl == button1)
                {
                    // Loop back to button2 instead of going to dtInvoiceDate
                    button2.Focus();
                    return true;
                }
            }

            // Default behavior for any other cases
            return base.ProcessTabKey(forward);
        }

        public void RefreshPaymode()
        {
            try
            {

                // Store current selection if any
                int currentPayModeId = 0;
                string currentPayModeName = "";
                if (cmbPaymntMethod.SelectedItem != null)
                {
                    Infragistics.Win.ValueListItem selectedItem = cmbPaymntMethod.SelectedItem as Infragistics.Win.ValueListItem;
                    if (selectedItem != null && selectedItem.DataValue is DataRowView)
                    {
                        DataRowView selectedRow = selectedItem.DataValue as DataRowView;
                        currentPayModeId = Convert.ToInt32(selectedRow["PayModeID"]);
                        currentPayModeName = selectedRow["PayModeName"].ToString();
                    }
                }

                // Create a new DataTable with an additional default item
                DataTable dt = new DataTable();
                dt.Columns.Add("PayModeID", typeof(int));
                dt.Columns.Add("PayModeName", typeof(string));

                // Add "Select Payment" as the first item with clearer text that it's required
                DataRow defaultRow = dt.NewRow();
                defaultRow["PayModeID"] = 0;
                defaultRow["PayModeName"] = "Select Payment";
                dt.Rows.Add(defaultRow);

                // Get payment methods from the database
                PaymodeDDlGrid grid = dp.GetPaymode();

                // Check if we have payment methods
                if (grid != null && grid.List != null && grid.List.Any())
                {

                    // Add the actual payment methods
                    foreach (var item in grid.List)
                    {
                        DataRow row = dt.NewRow();
                        row["PayModeID"] = item.PayModeID;
                        row["PayModeName"] = item.PayModeName;
                        dt.Rows.Add(row);
                    }
                }
                else
                {
                }

                // If we had a previously selected payment method that's not in the list yet, add it
                if (currentPayModeId > 0 && !dt.AsEnumerable().Any(row => Convert.ToInt32(row["PayModeID"]) == currentPayModeId))
                {
                    DataRow customRow = dt.NewRow();
                    customRow["PayModeID"] = currentPayModeId;
                    customRow["PayModeName"] = currentPayModeName;
                    dt.Rows.Add(customRow);
                }

                // Update the data source
                cmbPaymntMethod.DataSource = dt;
                cmbPaymntMethod.DisplayMember = "PayModeName";
                cmbPaymntMethod.ValueMember = "PayModeID";

                // Restore selection if possible, otherwise select first item (but not if skipping is requested)
                if (currentPayModeId > 0)
                {
                    // Try to find the previously selected payment method
                    for (int i = 0; i < cmbPaymntMethod.Items.Count; i++)
                    {
                        Infragistics.Win.ValueListItem item = cmbPaymntMethod.Items[i] as Infragistics.Win.ValueListItem;
                        if (item != null && item.DataValue is DataRowView)
                        {
                            DataRowView row = item.DataValue as DataRowView;
                            if (Convert.ToInt32(row["PayModeID"]) == currentPayModeId)
                            {
                                cmbPaymntMethod.SelectedIndex = i;
                                return;
                            }
                        }
                    }
                }

                // If we couldn't restore the selection or had no previous selection, select the first item
                // But only do this if skipPaymentMethodReset is not set
                if (!skipPaymentMethodReset && cmbPaymntMethod.Items.Count > 0)
                {
                    cmbPaymntMethod.SelectedIndex = 0;
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payment methods: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void RefreshCustomer()
        {
            // Set default text in the TextBox
            if (customerTextBox != null)
            {
                customerTextBox.Value = "Select Customer";
                customerTextBox.Tag = null; // Clear any previous LedgerID
                customerTextBox.ReadOnly = true; // Make sure it's read-only
            }
        }
        public void RefreshInvoice()
        {
            DataTable dt = new DataTable();
            SalesDDlGrid grid = dp.SalesDDl();
        }


        private void SaveSalesReturn()
        {
            try
            {
                // First make sure any active cell's value is committed
                CommitActiveCell();

                if (ultraGrid1.Rows.Count == 0)
                {
                    MessageBox.Show("No items to save.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create a temporary DataGridView to store only selected rows
                DataGridView tempDgv = new DataGridView();
                tempDgv.Columns.Add("SlNo", "SlNo");
                tempDgv.Columns.Add("ItemId", "ItemId");
                tempDgv.Columns.Add("ItemName", "ItemName");
                tempDgv.Columns.Add("BarCode", "BarCode");
                tempDgv.Columns.Add("Unit", "Unit");
                tempDgv.Columns.Add("UnitPrice", "UnitPrice");
                tempDgv.Columns.Add("Qty", "Qty");
                tempDgv.Columns.Add("ReturnQty", "ReturnQty"); // New column for current return quantity
                tempDgv.Columns.Add("ReturnedQty", "ReturnedQty"); // New column for previously returned quantity
                tempDgv.Columns.Add("Packing", "Packing");
                tempDgv.Columns.Add("Cost", "Cost");
                tempDgv.Columns.Add("Amount", "Amount");
                tempDgv.Columns.Add("Reason", "Reason");
                tempDgv.Columns.Add("TaxPer", "TaxPer");
                tempDgv.Columns.Add("TaxAmt", "TaxAmt");
                tempDgv.Columns.Add("TaxType", "TaxType");
                tempDgv.Columns.Add("BaseAmount", "BaseAmount"); // Taxable value for GST compliance

                // Copy only selected rows to the temporary grid
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells["Select"].Value != null && Convert.ToBoolean(row.Cells["Select"].Value))
                    {
                        int index = tempDgv.Rows.Add();
                        tempDgv.Rows[index].Cells["SlNo"].Value = row.Cells["SlNo"].Value;
                        tempDgv.Rows[index].Cells["ItemId"].Value = row.Cells["ItemId"].Value;
                        tempDgv.Rows[index].Cells["ItemName"].Value = row.Cells["ItemName"].Value;
                        tempDgv.Rows[index].Cells["BarCode"].Value = row.Cells["Barcode"].Value; // Fixed: use "Barcode" not "BarCode"
                        tempDgv.Rows[index].Cells["Unit"].Value = row.Cells["Unit"].Value;
                        tempDgv.Rows[index].Cells["UnitPrice"].Value = row.Cells["UnitPrice"].Value;
                        tempDgv.Rows[index].Cells["Qty"].Value = row.Cells["Qty"].Value;

                        // Safe access to ReturnQty and ReturnedQty columns
                        if (row.Cells.Exists("ReturnQty"))
                        {
                            tempDgv.Rows[index].Cells["ReturnQty"].Value = row.Cells["ReturnQty"].Value ?? row.Cells["Qty"].Value;
                        }
                        else
                        {
                            tempDgv.Rows[index].Cells["ReturnQty"].Value = row.Cells["Qty"].Value; // Default to Qty
                        }

                        if (row.Cells.Exists("ReturnedQty"))
                        {
                            tempDgv.Rows[index].Cells["ReturnedQty"].Value = row.Cells["ReturnedQty"].Value ?? 0;
                        }
                        else
                        {
                            tempDgv.Rows[index].Cells["ReturnedQty"].Value = 0; // Default to 0
                        }

                        // Safe access for Packing column
                        decimal packing = 1;
                        if (row.Cells.Exists("Packing") && row.Cells["Packing"].Value != null && row.Cells["Packing"].Value != DBNull.Value)
                        {
                            decimal.TryParse(row.Cells["Packing"].Value.ToString(), out packing);
                        }
                        tempDgv.Rows[index].Cells["Packing"].Value = packing; // Default to 1 if null

                        // Safe access for Cost column
                        decimal cost = 0;
                        if (row.Cells.Exists("Cost") && row.Cells["Cost"].Value != null && row.Cells["Cost"].Value != DBNull.Value)
                        {
                            decimal.TryParse(row.Cells["Cost"].Value.ToString(), out cost);
                        }
                        tempDgv.Rows[index].Cells["Cost"].Value = cost; // Default to 0 if null
                        tempDgv.Rows[index].Cells["Amount"].Value = row.Cells["Amount"].Value;
                        tempDgv.Rows[index].Cells["Reason"].Value = row.Cells["Reason"].Value;

                        // Safe access for tax-related columns
                        if (row.Cells.Exists("TaxPer") && row.Cells["TaxPer"].Value != null)
                        {
                            tempDgv.Rows[index].Cells["TaxPer"].Value = row.Cells["TaxPer"].Value;
                        }
                        else
                        {
                            tempDgv.Rows[index].Cells["TaxPer"].Value = 0;
                        }

                        if (row.Cells.Exists("TaxAmt") && row.Cells["TaxAmt"].Value != null)
                        {
                            tempDgv.Rows[index].Cells["TaxAmt"].Value = row.Cells["TaxAmt"].Value;
                        }
                        else
                        {
                            tempDgv.Rows[index].Cells["TaxAmt"].Value = 0;
                        }

                        if (row.Cells.Exists("TaxType") && row.Cells["TaxType"].Value != null)
                        {
                            tempDgv.Rows[index].Cells["TaxType"].Value = row.Cells["TaxType"].Value;
                        }
                        else
                        {
                            tempDgv.Rows[index].Cells["TaxType"].Value = "incl";
                        }

                        // Safe access for BaseAmount column (taxable value for GST)
                        if (row.Cells.Exists("BaseAmount") && row.Cells["BaseAmount"].Value != null)
                        {
                            tempDgv.Rows[index].Cells["BaseAmount"].Value = row.Cells["BaseAmount"].Value;
                        }
                        else
                        {
                            // Calculate BaseAmount if not in grid
                            decimal taxPer = 0;
                            decimal amount = 0;
                            string taxType = "incl";
                            if (row.Cells.Exists("TaxPer") && row.Cells["TaxPer"].Value != null)
                                decimal.TryParse(row.Cells["TaxPer"].Value.ToString(), out taxPer);
                            if (row.Cells.Exists("Amount") && row.Cells["Amount"].Value != null)
                                decimal.TryParse(row.Cells["Amount"].Value.ToString(), out amount);
                            if (row.Cells.Exists("TaxType") && row.Cells["TaxType"].Value != null)
                                taxType = row.Cells["TaxType"].Value.ToString();

                            if (taxType.ToLower() == "incl" && taxPer > 0)
                            {
                                decimal divisor = 1 + (taxPer / 100);
                                tempDgv.Rows[index].Cells["BaseAmount"].Value = Math.Round(amount / divisor, 2);
                            }
                            else
                            {
                                tempDgv.Rows[index].Cells["BaseAmount"].Value = amount;
                            }
                        }
                    }
                }

                if (tempDgv.Rows.Count == 0)
                {
                    MessageBox.Show("Please select at least one item to return.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Determine if this is an update or new record
                bool isUpdate = false;
                int existingSReturnNo = 0;
                if (!string.IsNullOrEmpty(TxtSRNO.Text) && int.TryParse(TxtSRNO.Text, out existingSReturnNo) && existingSReturnNo > 0)
                {
                    // Check if the update button is visible, which indicates we're in update mode
                    Control[] updateButtons = this.Controls.Find("updtbtn", true);
                    if (updateButtons.Length > 0 && updateButtons[0] is Button updateBtn && updateBtn.Visible)
                    {
                        isUpdate = true;
                    }
                }

                // Set up the SalesReturn object with safe conversions
                SReturn.BranchId = SessionContext.BranchId;
                SReturn.CompanyId = SessionContext.CompanyId;

                // Set customer information
                if (customerTextBox != null)
                {
                    SReturn.CustomerName = customerTextBox.Value?.ToString() ?? "";
                    SReturn.LedgerID = customerTextBox.Tag != null ? Convert.ToInt32(customerTextBox.Tag) : 0;
                }
                else
                {
                    SReturn.CustomerName = "DEFAULT CUSTOMER";
                    SReturn.LedgerID = 0;
                }

                // Validate payment method selection
                if (!ValidatePaymentMethod())
                {
                    return;
                }

                // Log selected payment method
                Infragistics.Win.ValueListItem selectedPaymentItem = cmbPaymntMethod.SelectedItem as Infragistics.Win.ValueListItem;
                if (selectedPaymentItem != null && selectedPaymentItem.DataValue is DataRowView)
                {
                    DataRowView selectedPaymentRow = selectedPaymentItem.DataValue as DataRowView;
                }

                // Set payment method information
                SReturn.Paymode = cmbPaymntMethod.Text;
                SReturn.PaymodeID = Convert.ToInt32(cmbPaymntMethod.Value ?? 0);

                // Set SReturnNo - 0 for new records, existing id for updates
                SReturn.SReturnNo = isUpdate ? existingSReturnNo : 0;

                // Set other fields
                SReturn.InvoiceNo = textBox1.Text;
                SReturn.SpDisPer = 0;
                SReturn.SpDsiAmt = 0;
                SReturn.BillDiscountPer = 0;
                SReturn.BillDiscountAmt = 0;
                // Calculate and set tax values
                SReturn.TaxPer = CalculateWeightedAverageTaxPercentage();
                SReturn.TaxAmt = CalculateTotalTaxAmount();
                SReturn.Frieght = 0;
                SReturn.GrandTotal = Convert.ToDouble(txtNetTotal.Text);
                SReturn.SubTotal = Convert.ToDouble(TxtSubTotal.Text);
                SReturn.UserID = SessionContext.UserId;
                SReturn.UserName = SessionContext.UserName;
                SReturn.TaxType = DetermineTaxType();
                SReturn.RoundOff = 0;
                SReturn.CessAmt = 0;
                SReturn.CessPer = 0;
                SReturn.VoucherID = 0;
                SReturn.BranchName = SessionContext.BranchName;
                SReturn.CalAfterTax = 0;
                SReturn.CurSymbol = "";
                SReturn.SReturnDate = (DateTime)dtSReturnDate.Value;
                SReturn.InvoiceDate = (DateTime)dtInvoiceDate.Value;

                // Set operation based on whether this is an update or new record
                SReturn._Operation = isUpdate ? "UPDATE" : "CREATE";

                // Call the appropriate method based on operation type
                string message;
                if (isUpdate)
                {
                    message = operations.UpdateSalesReturn(SReturn, tempDgv);
                }
                else
                {
                    message = operations.saveSR(SReturn, null, tempDgv);
                }

                MessageBox.Show(message);

                // Clear the form for next entry after successful save/update
                if (message.Contains("Successfully") || message.Contains("success"))
                {
                    SaveSalesReturnActivityLog(isUpdate ? "UPDATE" : "SAVE", SReturn, message);
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving sales return: " + ex.Message);
            }
        }

        private void SaveSalesReturnActivityLog(string activityType, SalesReturn salesReturn, string saveMessage)
        {
            if (salesReturn == null)
            {
                return;
            }

            int returnNo = salesReturn.SReturnNo > 0
                ? salesReturn.SReturnNo
                : ExtractSalesReturnNo(saveMessage);

            if (returnNo <= 0)
            {
                return;
            }

            try
            {
                using (var repo = new TransactionActivityLogRepository())
                {
                    repo.SaveSalesReturnActivity(
                        returnNo,
                        salesReturn.InvoiceNo,
                        salesReturn.CustomerName,
                        salesReturn.Paymode,
                        Convert.ToDecimal(salesReturn.GrandTotal),
                        activityType,
                        $"Sales Return No: {returnNo}, Customer: {salesReturn.CustomerName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Unable to save sales return activity log: " + ex.Message);
            }
        }

        private static int ExtractSalesReturnNo(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return 0;
            }

            int hashIndex = message.IndexOf('#');
            if (hashIndex >= 0)
            {
                int start = hashIndex + 1;
                while (start < message.Length && char.IsWhiteSpace(message[start]))
                {
                    start++;
                }

                int end = start;
                while (end < message.Length && char.IsDigit(message[end]))
                {
                    end++;
                }

                int parsed;
                if (end > start && int.TryParse(message.Substring(start, end - start), out parsed))
                {
                    return parsed;
                }
            }

            return 0;
        }

        private void pbxSave_Click(object sender, EventArgs e)
        {
            try
            {
                int selectedCount = 0;
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.IsDataRow && !row.IsFilteredOut)
                    {
                        bool isSelected = row.Cells.Exists("Select") && row.Cells["Select"].Value != null &&
                                         row.Cells["Select"].Value != DBNull.Value && Convert.ToBoolean(row.Cells["Select"].Value);
                        if (isSelected)
                        {
                            selectedCount++;
                        }
                    }
                }

                if (selectedCount == 0)
                {
                    MessageBox.Show("Please select at least one item by checking the checkbox in the Select column.",
                                   "No Items Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Validate payment method first
                if (!ValidatePaymentMethod())
                {
                    return;
                }

                // Show confirmation dialog for saving sales return
                DialogResult result = MessageBox.Show("Save goods return?", "Confirm Save",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveSalesReturnWithCreditNote();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in save operation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(TxtBarcode.Text))
            {
                try
                {
                    // Check if a customer is selected - only required for bill-based returns
                    // For "without bill" scenario, customer selection is optional
                    int customerId = customerTextBox.Tag != null ? Convert.ToInt32(customerTextBox.Tag) : 0;
                    bool isWithoutBill = textBox1.Value?.ToString() == "without bill" || string.IsNullOrEmpty(textBox1.Value?.ToString());

                    if (customerId <= 0 && !isWithoutBill)
                    {
                        MessageBox.Show("Please select a customer first for bill-based returns", "No Customer Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // If data source is Bill, show a message and don't proceed
                    if (currentDataSource == DataSource.Bill)
                    {
                        MessageBox.Show("You have already loaded items from bills. This data comes from saved transactions and cannot be mixed with barcode entries.",
                            "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        TxtBarcode.Clear();
                        return;
                    }

                    // If there are existing items from a different source, clear them first
                    if (currentDataSource != DataSource.None && currentDataSource != DataSource.Barcode && !string.IsNullOrEmpty(textBox1.Value?.ToString()) && textBox1.Value?.ToString() != "without bill")
                    {
                        DialogResult result = MessageBox.Show(
                            "Adding items by barcode will replace your current items. Do you want to continue?",
                            "Confirm Change",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.No)
                        {
                            TxtBarcode.Clear();
                            return;
                        }

                        // Clear the grid but preserve customer selection
                        ClearGridOnly();
                    }

                    // Set textBox1 to indicate items are being added without a bill
                    textBox1.Value = "without bill";
                    currentDataSource = DataSource.Barcode;

                    DataBase.Operations = "GETITEMBYBARCODE";
                    ItemDDlGrid item = dp.itemDDlGrid(TxtBarcode.Text, "");
                    if (item.List != null && item.List.Count() == 1)
                    {
                        CheckData(TxtBarcode.Text);
                        if (!CheckExists)
                        {
                            var itemData = item.List.First();
                            DataTable dt = (DataTable)ultraGrid1.DataSource;
                            DataRow newRow = dt.NewRow();

                            newRow["SlNo"] = dt.Rows.Count + 1;
                            newRow["ItemId"] = itemData.ItemId;
                            newRow["ItemName"] = itemData.Description;
                            newRow["Barcode"] = itemData.BarCode;
                            newRow["Unit"] = itemData.Unit;
                            newRow["UnitPrice"] = itemData.RetailPrice;
                            newRow["Qty"] = 1;
                            newRow["ReturnQty"] = 1; // Default return quantity
                            newRow["ReturnedQty"] = 0; // Previously returned quantity (starts at 0)
                            newRow["Packing"] = itemData.Packing; // Use actual packing from item master
                            newRow["Cost"] = itemData.Cost; // Add cost from item data
                            newRow["Reason"] = "Select Reason";
                            newRow["Amount"] = itemData.RetailPrice;

                            // Calculate tax values for this item
                            double sellingPrice = (double)itemData.RetailPrice;
                            double taxPercentage = itemData.TaxPer;
                            string taxType = itemData.TaxType ?? "excl";

                            // Calculate tax amount and total with tax
                            double taxAmount = CalculateTaxAmount(sellingPrice, taxPercentage, taxType);
                            double totalWithTax = CalculateTotalWithTax(sellingPrice, taxPercentage, taxType);

                            newRow["TaxPer"] = taxPercentage;
                            newRow["TaxAmt"] = taxAmount;
                            newRow["TaxType"] = taxType;

                            // Update amount to include tax if needed
                            newRow["Amount"] = totalWithTax;
                            newRow["Select"] = true; // Default to selected for sales return items

                            dt.Rows.Add(newRow);
                            ultraGrid1.DataSource = dt;

                            // Focus the Unit cell in the newly added row with enhanced method
                            System.Threading.Thread.Sleep(50);
                            ForceUnitCellEditMode();
                        }
                        CheckExists = false;
                        TxtBarcode.Clear();

                        // Update totals after adding item
                        UpdateTotalAmount();
                    }
                    else
                    {
                        MessageBox.Show("Item not found with this barcode", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        TxtBarcode.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error processing barcode: " + ex.Message);
                }
            }
        }

        public void CheckData(string Barcode)
        {
            try
            {
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells["Barcode"].Value?.ToString() == Barcode)
                    {
                        CheckExists = true;
                        // Parse existing quantity safely
                        decimal existingQty = 0;
                        decimal.TryParse(row.Cells["Qty"].Value?.ToString(), out existingQty);

                        // Add 1 to existing quantity
                        decimal newQty = existingQty + 1;
                        row.Cells["Qty"].Value = newQty;

                        // Update ReturnQty if the column exists and validate against available quantity
                        if (row.Cells.Exists("ReturnQty"))
                        {
                            decimal existingReturnQty = 0;
                            decimal.TryParse(row.Cells["ReturnQty"].Value?.ToString(), out existingReturnQty);
                            decimal newReturnQty = existingReturnQty + 1;

                            // Validate against available quantity
                            decimal returnedQty = 0;
                            decimal.TryParse(row.Cells["ReturnedQty"].Value?.ToString(), out returnedQty);
                            decimal availableQty = newQty - returnedQty;

                            if (newReturnQty <= availableQty)
                            {
                                row.Cells["ReturnQty"].Value = newReturnQty;

                                // Calculate amount based on ReturnQty (FIXED: was using newQty)
                                decimal unitPrice = 0;
                                decimal.TryParse(row.Cells["UnitPrice"].Value?.ToString(), out unitPrice);
                                row.Cells["Amount"].Value = newReturnQty * unitPrice;
                            }
                            else
                            {
                                // Keep the previous value and show warning
                                row.Cells["ReturnQty"].Value = existingReturnQty;
                                MessageBox.Show($"Cannot add more items. Available quantity: {availableQty}",
                                    "Quantity Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                // Recalculate amount with existing ReturnQty
                                decimal unitPrice = 0;
                                decimal.TryParse(row.Cells["UnitPrice"].Value?.ToString(), out unitPrice);
                                row.Cells["Amount"].Value = existingReturnQty * unitPrice;
                            }
                        }
                        else
                        {
                            // Fallback for backward compatibility - calculate amount based on Qty
                            decimal unitPrice = 0;
                            decimal.TryParse(row.Cells["UnitPrice"].Value?.ToString(), out unitPrice);
                            row.Cells["Amount"].Value = newQty * unitPrice;
                        }

                        // Update totals
                        UpdateTotalAmount();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking barcode: " + ex.Message);
            }
        }
        private void UpdateTotalAmount()
        {
            try
            {
                decimal total = 0;
                decimal selectedTotal = 0;
                int selectedCount = 0;

                // Force update the grid data before calculating totals
                ultraGrid1.UpdateData();

                // Check header checkbox state first (indicates "Select All" was clicked)
                bool headerChecked = false;
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Select") &&
                    ultraGrid1.DisplayLayout.Bands[0].Columns["Select"].Header.CheckBoxVisibility == HeaderCheckBoxVisibility.Always)
                {
                    // Try to get header checkbox state - this approach varies by Infragistics version
                    try
                    {
                        var headerCell = ultraGrid1.DisplayLayout.Bands[0].Columns["Select"].Header;
                        // Use reflection to try to get checkbox state since direct property might not exist
                        var propertyInfo = headerCell.GetType().GetProperty("CheckBoxChecked");
                        if (propertyInfo != null)
                        {
                            headerChecked = (bool)propertyInfo.GetValue(headerCell, null);
                        }
                    }
                    catch
                    {
                        // Continue with normal calculation if we can't get header state
                    }
                }

                // Loop through all rows and calculate totals
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (decimal.TryParse(row.Cells["Amount"].Value?.ToString(), out decimal amount))
                    {
                        // Always calculate full total
                        total += amount;

                        // Check if this row is selected - handle all possible value types
                        bool isSelected = headerChecked; // Default to header state

                        if (!headerChecked && row.Cells.Exists("Select") && row.Cells["Select"].Value != null)
                        {
                            // Try different approaches to safely get a boolean value
                            object selectValue = row.Cells["Select"].Value;

                            if (selectValue is bool)
                            {
                                isSelected = (bool)selectValue;
                            }
                            else if (selectValue is int)
                            {
                                isSelected = ((int)selectValue) != 0;
                            }
                            else if (selectValue is string)
                            {
                                string strValue = selectValue.ToString().ToLower();
                                isSelected = strValue == "true" || strValue == "1" || strValue == "yes";
                            }
                            else
                            {
                                // As a last resort, try safe conversion
                                try
                                {
                                    isSelected = Convert.ToBoolean(selectValue);
                                }
                                catch
                                {
                                    // If conversion fails, default to false
                                    isSelected = false;
                                }
                            }
                        }

                        // Add to selected total if row is checked
                        if (isSelected)
                        {
                            selectedTotal += amount;
                            selectedCount++;
                        }
                    }
                }


                // Calculate proper subtotal and tax amounts
                double subtotalAmount = CalculateSubtotal();
                double totalTaxAmount = CalculateTotalTaxAmount();

                // For sales return, net total is always subtotal + tax (regardless of tax type)
                // because we're dealing with return amounts that should include tax
                double netTotalAmount = subtotalAmount + totalTaxAmount;

                // Update UI with calculated totals
                if (TxtSubTotal != null)
                {
                    // Show the calculated subtotal (base amount before tax)
                    TxtSubTotal.Value = subtotalAmount.ToString("0.00");
                }

                if (txtNetTotal != null)
                {
                    // For net amount, show selected total + tax if any rows are selected, otherwise show full total + tax
                    txtNetTotal.Text = netTotalAmount.ToString("0.00");
                }

                if (TxtInvoiceAmnt != null)
                {
                    // For invoice amount, show selected total + tax if any rows are selected, otherwise show full total + tax
                    TxtInvoiceAmnt.Value = netTotalAmount.ToString("0.00");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating total amount: " + ex.Message);
            }
        }

        private void ultraGrid1_AfterCellUpdate(object sender, CellEventArgs e)
        {
            try
            {
                if (e.Cell != null && (e.Cell.Column.Key == "ReturnQty" || e.Cell.Column.Key == "UnitPrice"))
                {
                    UltraGridRow row = e.Cell.Row;
                    decimal returnQty = 0;
                    decimal unitPrice = 0;
                    decimal soldQty = 0;
                    decimal returnedQty = 0;

                    if (decimal.TryParse(row.Cells["ReturnQty"].Value?.ToString(), out returnQty) &&
                        decimal.TryParse(row.Cells["UnitPrice"].Value?.ToString(), out unitPrice) &&
                        decimal.TryParse(row.Cells["Qty"].Value?.ToString(), out soldQty) &&
                        decimal.TryParse(row.Cells["ReturnedQty"].Value?.ToString(), out returnedQty))
                    {
                        // Ensure return quantity is not negative
                        if (returnQty < 0)
                        {
                            returnQty = 0;
                            row.Cells["ReturnQty"].Value = returnQty;
                        }

                        // Validate that ReturnQty does not exceed available quantity (SoldQty - ReturnedQty)
                        // For "without bill" returns, we don't enforce this check and sync Qty with ReturnQty
                        bool isWithoutBill = textBox1.Value?.ToString() == "without bill" || string.IsNullOrEmpty(textBox1.Value?.ToString());
                        if (isWithoutBill)
                        {
                            decimal currentQty = 0;
                            decimal.TryParse(row.Cells["Qty"].Value?.ToString(), out currentQty);
                            if (currentQty != returnQty)
                            {
                                row.Cells["Qty"].Value = returnQty;
                            }
                        }
                        else
                        {
                            decimal availableQty = soldQty - returnedQty;
                            if (returnQty > availableQty)
                            {
                                MessageBox.Show($"Cannot return {returnQty} items. Only {availableQty} items are available for return.\n\n" +
                                               $"Details:\n" +
                                               $"• Originally Sold: {soldQty}\n" +
                                               $"• Previously Returned: {returnedQty}\n" +
                                               $"• Maximum Returnable: {availableQty}",
                                               "Return Quantity Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                returnQty = availableQty;
                                row.Cells["ReturnQty"].Value = returnQty;
                            }
                        }

                        // Calculate tax values for this item
                        double sellingPrice = (double)(returnQty * unitPrice);
                        double taxPercentage = 0;
                        string taxType = "excl";

                        // Get tax values from the row if they exist
                        if (row.Cells["TaxPer"].Value != null && double.TryParse(row.Cells["TaxPer"].Value.ToString(), out double existingTaxPer))
                        {
                            taxPercentage = existingTaxPer;
                        }
                        if (row.Cells["TaxType"].Value != null && !string.IsNullOrEmpty(row.Cells["TaxType"].Value.ToString()))
                        {
                            taxType = row.Cells["TaxType"].Value.ToString();
                        }

                        // Calculate tax amount and total with tax
                        double taxAmount = CalculateTaxAmount(sellingPrice, taxPercentage, taxType);
                        double totalWithTax = CalculateTotalWithTax(sellingPrice, taxPercentage, taxType);

                        // Update tax values in the row
                        row.Cells["TaxPer"].Value = taxPercentage;
                        row.Cells["TaxAmt"].Value = taxAmount;
                        row.Cells["TaxType"].Value = taxType;

                        // Update amount for this row based on ReturnQty (with tax if applicable)
                        row.Cells["Amount"].Value = totalWithTax;

                        // Update totals
                        UpdateTotalAmount();
                    }
                }
                // Handle legacy Qty column for backward compatibility
                else if (e.Cell != null && e.Cell.Column.Key == "Qty")
                {
                    UltraGridRow row = e.Cell.Row;
                    decimal qty = 0;
                    decimal unitPrice = 0;
                    if (decimal.TryParse(row.Cells["Qty"].Value?.ToString(), out qty) &&
                        decimal.TryParse(row.Cells["UnitPrice"].Value?.ToString(), out unitPrice))
                    {
                        // Ensure quantity is not negative
                        if (qty < 0)
                        {
                            qty = 0;
                            row.Cells["Qty"].Value = qty;
                        }

                        // If ReturnQty column exists, sync it with Qty for backward compatibility
                        if (row.Cells.Exists("ReturnQty"))
                        {
                            // Validate against available quantity first
                            decimal returnedQty = 0;
                            decimal.TryParse(row.Cells["ReturnedQty"].Value?.ToString(), out returnedQty);
                            decimal availableQty = qty - returnedQty;

                            if (qty <= availableQty + returnedQty) // Allow qty to be set to sold amount
                            {
                                decimal currentReturnQty = 0;
                                decimal.TryParse(row.Cells["ReturnQty"].Value?.ToString(), out currentReturnQty);
                                if (currentReturnQty != qty)
                                {
                                    row.Cells["ReturnQty"].Value = qty;
                                }
                                // FIXED: Calculate amount based on ReturnQty, not Qty
                                row.Cells["Amount"].Value = qty * unitPrice;
                            }
                            else
                            {
                                // Reset to previous valid value
                                row.Cells["Qty"].Value = qty - 1; // Revert the change
                                MessageBox.Show($"Quantity cannot exceed available amount. Available: {availableQty + returnedQty}",
                                    "Invalid Quantity", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return; // Don't update totals if validation failed
                            }
                        }
                        else
                        {
                            // Fallback: Update amount for this row based on Qty
                            row.Cells["Amount"].Value = qty * unitPrice;
                        }

                        // Update totals
                        UpdateTotalAmount();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating cell: " + ex.Message);
            }
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only handle Enter key for navigation between cells
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                HandleEnterNavigation();
            }
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                e.Layout.Override.RowSpacingBefore = 2;
                e.Layout.Override.SelectTypeRow = SelectType.Single;
                e.Layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                e.Layout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                // Set horizontal scroll behavior to ensure all columns are visible
                e.Layout.ScrollBounds = ScrollBounds.ScrollToFill;
                e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;

                // Remove "Select" caption text from the Select column while keeping the checkbox
                if (e.Layout.Bands[0].Columns.Exists("Select"))
                {
                    e.Layout.Bands[0].Columns["Select"].Header.Caption = "";
                    e.Layout.Bands[0].Columns["Select"].Header.CheckBoxVisibility = HeaderCheckBoxVisibility.Always;
                    e.Layout.Bands[0].Columns["Select"].Header.CheckBoxAlignment = HeaderCheckBoxAlignment.Center;
                }

                // Adjust width proportions for optimal display without horizontal scrolling
                int totalWidth = ultraGrid1.Width - 25; // Account for vertical scrollbar and padding

                // Set minimum widths for columns to prevent them from becoming too small
                foreach (UltraGridColumn column in e.Layout.Bands[0].Columns)
                {
                    if (!column.Hidden)
                    {
                        // Ensure each column maintains minimum width
                        column.MinWidth = 50;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in InitializeLayout: " + ex.Message);
            }
        }
        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Delete)
                {
                    UltraGridRow activeRow = ultraGrid1.ActiveRow;
                    if (activeRow != null)
                    {
                        DataTable dt = (DataTable)ultraGrid1.DataSource;
                        dt.Rows.RemoveAt(activeRow.Index);

                        // Renumber the SlNo column
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            dt.Rows[i]["SlNo"] = i + 1;
                        }

                        ultraGrid1.DataSource = dt;
                        UpdateTotalAmount();
                    }
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    // Mark the event as handled to prevent default enter behavior
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    // Handle Enter key navigation
                    HandleEnterNavigation();
                }
                // Up arrow key - Move to previous row maintaining the same column
                else if (e.KeyCode == Keys.Up)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    // Always try to exit edit mode first (will do nothing if not in edit mode)
                    bool wasInEditMode = false;
                    try
                    {
                        ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                        wasInEditMode = true;
                    }
                    catch
                    {
                        // Not in edit mode
                        wasInEditMode = false;
                    }

                    UltraGridCell activeCell = ultraGrid1.ActiveCell;
                    if (activeCell != null)
                    {
                        UltraGridRow activeRow = activeCell.Row;
                        int currentRowIndex = activeRow.Index;

                        // If not in first row, go to same column in previous row
                        if (currentRowIndex > 0)
                        {
                            string currentColumn = activeCell.Column.Key;
                            UltraGridRow prevRow = ultraGrid1.Rows[currentRowIndex - 1];

                            // Activate same cell in previous row
                            prevRow.Cells[currentColumn].Activate();
                            ultraGrid1.ActiveCell = prevRow.Cells[currentColumn];

                            // If cell is editable and was in edit mode before, enter edit mode again
                            if (wasInEditMode && prevRow.Cells[currentColumn].Column.CellActivation == Activation.AllowEdit)
                            {
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            }
                        }
                        // If in first row, go to the previous control in tab order
                        else
                        {
                            TxtBarcode.Focus();
                            TxtBarcode.SelectAll();
                        }
                    }
                }
                // Down arrow key - Improved handling to move to next row maintaining the same column
                else if (e.KeyCode == Keys.Down)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    // Always try to exit edit mode first (will do nothing if not in edit mode)
                    bool wasInEditMode = false;
                    try
                    {
                        ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                        wasInEditMode = true;
                    }
                    catch
                    {
                        // Not in edit mode
                        wasInEditMode = false;
                    }

                    UltraGridCell activeCell = ultraGrid1.ActiveCell;
                    if (activeCell != null)
                    {
                        UltraGridRow activeRow = activeCell.Row;
                        int currentRowIndex = activeRow.Index;

                        // If not in last row, go to same column in next row
                        if (currentRowIndex < ultraGrid1.Rows.Count - 1)
                        {
                            string currentColumn = activeCell.Column.Key;
                            UltraGridRow nextRow = ultraGrid1.Rows[currentRowIndex + 1];

                            // Activate same cell in next row
                            nextRow.Cells[currentColumn].Activate();
                            ultraGrid1.ActiveCell = nextRow.Cells[currentColumn];

                            // If cell is editable and was in edit mode before, enter edit mode again
                            if (wasInEditMode && nextRow.Cells[currentColumn].Column.CellActivation == Activation.AllowEdit)
                            {
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            }
                        }
                        // If in last row, handle special case
                        else
                        {
                            // Cycle back to first row, same column
                            if (ultraGrid1.Rows.Count > 0)
                            {
                                string currentColumn = activeCell.Column.Key;
                                UltraGridRow firstRow = ultraGrid1.Rows[0];
                                firstRow.Cells[currentColumn].Activate();
                                ultraGrid1.ActiveCell = firstRow.Cells[currentColumn];

                                // If cell is editable and was in edit mode before, enter edit mode again
                                if (wasInEditMode && firstRow.Cells[currentColumn].Column.CellActivation == Activation.AllowEdit)
                                {
                                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                }
                            }
                        }
                    }
                }
                // Tab key - Move to next editable cell (similar to Right arrow but skips read-only cells)
                else if (e.KeyCode == Keys.Tab)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    UltraGridCell activeCell = ultraGrid1.ActiveCell;
                    UltraGridRow activeRow = activeCell.Row;
                    int columnIndex = ultraGrid1.DisplayLayout.Bands[0].Columns.IndexOf(activeCell.Column);
                    int maxColumns = ultraGrid1.DisplayLayout.Bands[0].Columns.Count;
                    int currentRowIndex = activeRow.Index;
                    bool foundNextCell = false;

                    // If Shift+Tab, move backward
                    if (e.Shift)
                    {
                        // Start from current column and move left
                        for (int i = columnIndex - 1; i >= 0; i--)
                        {
                            if (!ultraGrid1.DisplayLayout.Bands[0].Columns[i].Hidden &&
                                ultraGrid1.DisplayLayout.Bands[0].Columns[i].CellActivation == Activation.AllowEdit)
                            {
                                string colKey = ultraGrid1.DisplayLayout.Bands[0].Columns[i].Key;
                                activeRow.Cells[colKey].Activate();
                                ultraGrid1.ActiveCell = activeRow.Cells[colKey];
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                foundNextCell = true;
                                break;
                            }
                        }

                        // If no cell found in this row, go to previous row's last editable column
                        if (!foundNextCell && currentRowIndex > 0)
                        {
                            UltraGridRow prevRow = ultraGrid1.Rows[currentRowIndex - 1];

                            // Find the last editable column in the previous row
                            for (int i = maxColumns - 1; i >= 0; i--)
                            {
                                if (!ultraGrid1.DisplayLayout.Bands[0].Columns[i].Hidden &&
                                    ultraGrid1.DisplayLayout.Bands[0].Columns[i].CellActivation == Activation.AllowEdit)
                                {
                                    string colKey = ultraGrid1.DisplayLayout.Bands[0].Columns[i].Key;
                                    prevRow.Cells[colKey].Activate();
                                    ultraGrid1.ActiveCell = prevRow.Cells[colKey];
                                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                    break;
                                }
                            }
                        }
                        // If in first row's first editable cell, cycle to last control in tab order
                        else if (!foundNextCell && currentRowIndex == 0)
                        {
                            // Focus on TxtBarcode which is the previous control in tab order
                            TxtBarcode.Focus();
                            TxtBarcode.SelectAll();
                        }
                    }
                    // Regular Tab, move forward
                    else
                    {
                        // For Reason column, always move to next row's first editable cell
                        if (activeCell.Column.Key == "Reason")
                        {
                            // Move to next row's first editable cell
                            if (currentRowIndex < ultraGrid1.Rows.Count - 1)
                            {
                                UltraGridRow nextRow = ultraGrid1.Rows[currentRowIndex + 1];

                                // Find the first editable column in the next row
                                for (int i = 0; i < maxColumns; i++)
                                {
                                    if (!ultraGrid1.DisplayLayout.Bands[0].Columns[i].Hidden &&
                                        ultraGrid1.DisplayLayout.Bands[0].Columns[i].CellActivation == Activation.AllowEdit)
                                    {
                                        string colKey = ultraGrid1.DisplayLayout.Bands[0].Columns[i].Key;
                                        nextRow.Cells[colKey].Activate();
                                        ultraGrid1.ActiveCell = nextRow.Cells[colKey];
                                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                        return;
                                    }
                                }
                            }
                            // If in last row, loop back to first row
                            else
                            {
                                // Loop back to first row, first editable column
                                UltraGridRow firstRow = ultraGrid1.Rows[0];

                                // Find the first editable column in the first row
                                for (int i = 0; i < maxColumns; i++)
                                {
                                    if (!ultraGrid1.DisplayLayout.Bands[0].Columns[i].Hidden &&
                                        ultraGrid1.DisplayLayout.Bands[0].Columns[i].CellActivation == Activation.AllowEdit)
                                    {
                                        string colKey = ultraGrid1.DisplayLayout.Bands[0].Columns[i].Key;
                                        firstRow.Cells[colKey].Activate();
                                        ultraGrid1.ActiveCell = firstRow.Cells[colKey];
                                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                        return;
                                    }
                                }
                            }
                        }

                        // Standard tabbing behavior for other columns
                        // Start from current column and move right
                        for (int i = columnIndex + 1; i < maxColumns; i++)
                        {
                            if (!ultraGrid1.DisplayLayout.Bands[0].Columns[i].Hidden &&
                                ultraGrid1.DisplayLayout.Bands[0].Columns[i].CellActivation == Activation.AllowEdit)
                            {
                                string colKey = ultraGrid1.DisplayLayout.Bands[0].Columns[i].Key;
                                activeRow.Cells[colKey].Activate();
                                ultraGrid1.ActiveCell = activeRow.Cells[colKey];
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                foundNextCell = true;
                                break;
                            }
                        }

                        // If no cell found in this row, go to next row's first editable column
                        if (!foundNextCell && currentRowIndex < ultraGrid1.Rows.Count - 1)
                        {
                            UltraGridRow nextRow = ultraGrid1.Rows[currentRowIndex + 1];

                            // Find the first editable column in the next row
                            for (int i = 0; i < maxColumns; i++)
                            {
                                if (!ultraGrid1.DisplayLayout.Bands[0].Columns[i].Hidden &&
                                    ultraGrid1.DisplayLayout.Bands[0].Columns[i].CellActivation == Activation.AllowEdit)
                                {
                                    string colKey = ultraGrid1.DisplayLayout.Bands[0].Columns[i].Key;
                                    nextRow.Cells[colKey].Activate();
                                    ultraGrid1.ActiveCell = nextRow.Cells[colKey];
                                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                    break;
                                }
                            }
                        }
                        else if (!foundNextCell)
                        {
                            // If we're in the last editable cell of the last row,
                            // Loop back to first row, first editable column
                            UltraGridRow firstRow = ultraGrid1.Rows[0];

                            // Find the first editable column in the first row
                            for (int i = 0; i < maxColumns; i++)
                            {
                                if (!ultraGrid1.DisplayLayout.Bands[0].Columns[i].Hidden &&
                                    ultraGrid1.DisplayLayout.Bands[0].Columns[i].CellActivation == Activation.AllowEdit)
                                {
                                    string colKey = ultraGrid1.DisplayLayout.Bands[0].Columns[i].Key;
                                    firstRow.Cells[colKey].Activate();
                                    ultraGrid1.ActiveCell = firstRow.Cells[colKey];
                                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling key down: {ex.Message}");
            }
        }

        private void HandleEnterNavigation()
        {
            try
            {
                if (ultraGrid1.ActiveCell != null)
                {
                    UltraGridRow currentRow = ultraGrid1.ActiveRow;
                    UltraGridCell activeCell = ultraGrid1.ActiveCell;
                    string currentColumn = activeCell.Column.Key;

                    // Update any quantity or price changes
                    UpdateRowAmount(currentRow);

                    // Determine which cell to move to next based on current column
                    switch (currentColumn)
                    {
                        case "Unit":
                            // Move to UnitPrice in same row
                            currentRow.Cells["UnitPrice"].Activate();
                            ultraGrid1.ActiveCell = currentRow.Cells["UnitPrice"];
                            break;

                        case "UnitPrice":
                            // Move to Qty in same row
                            currentRow.Cells["Qty"].Activate();
                            ultraGrid1.ActiveCell = currentRow.Cells["Qty"];
                            break;

                        case "Qty":
                            // Move to Reason in same row (skip hidden Packing column)
                            currentRow.Cells["Reason"].Activate();
                            ultraGrid1.ActiveCell = currentRow.Cells["Reason"];

                            // Special handling for dropdown - explicitly show dropdown when navigating to Reason
                            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.Edit;
                            ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                            ShowDropDownForReasonCell();
                            return; // Important: return here to prevent the final EnterEditMode call

                        case "Reason":
                            // Handle Reason column navigation, in any row
                            if (currentRow.Index < ultraGrid1.Rows.Count - 1)
                            {
                                // Move to Unit in next row
                                int nextRowIndex = currentRow.Index + 1;
                                ultraGrid1.Rows[nextRowIndex].Cells["Unit"].Activate();
                                ultraGrid1.ActiveCell = ultraGrid1.Rows[nextRowIndex].Cells["Unit"];
                            }
                            else
                            {
                                // In last row, cycle back to first row's Unit cell
                                if (ultraGrid1.Rows.Count > 0)
                                {
                                    ultraGrid1.Rows[0].Cells["Unit"].Activate();
                                    ultraGrid1.ActiveCell = ultraGrid1.Rows[0].Cells["Unit"];
                                }
                            }
                            break;

                        case "Amount":
                            // For Amount column, always move to next row's Unit or barcode textbox
                            if (currentRow.Index < ultraGrid1.Rows.Count - 1)
                            {
                                // Move to Unit in next row
                                int nextRowIndex = currentRow.Index + 1;
                                ultraGrid1.Rows[nextRowIndex].Cells["Unit"].Activate();
                                ultraGrid1.ActiveCell = ultraGrid1.Rows[nextRowIndex].Cells["Unit"];
                            }
                            else
                            {
                                // Last row, go to barcode
                                TxtBarcode.Focus();
                                TxtBarcode.SelectAll();
                                return;
                            }
                            break;

                        default:
                            // For any other column, move down to the same column in the next row if possible
                            if (currentRow.Index < ultraGrid1.Rows.Count - 1)
                            {
                                int nextRowIndex = currentRow.Index + 1;
                                ultraGrid1.Rows[nextRowIndex].Cells[currentColumn].Activate();
                                ultraGrid1.ActiveCell = ultraGrid1.Rows[nextRowIndex].Cells[currentColumn];
                            }
                            else
                            {
                                // Focus on barcode textbox for next item
                                TxtBarcode.Focus();
                                TxtBarcode.SelectAll();
                                return;
                            }
                            break;
                    }

                    // Enter edit mode on the active cell
                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in enter navigation: {ex.Message}");
            }
        }

        private void UpdateRowAmount(UltraGridRow row)
        {
            try
            {
                if (row != null)
                {
                    decimal qty = 0;
                    decimal unitPrice = 0;

                    if (decimal.TryParse(row.Cells["Qty"].Value?.ToString(), out qty) &&
                        decimal.TryParse(row.Cells["UnitPrice"].Value?.ToString(), out unitPrice))
                    {
                        // Ensure quantity is not negative
                        if (qty < 0)
                        {
                            qty = 0;
                            row.Cells["Qty"].Value = qty;
                        }

                        // Prefer ReturnQty when present; fall back to Qty
                        if (row.Cells.Exists("ReturnQty") && decimal.TryParse(row.Cells["ReturnQty"].Value?.ToString(), out decimal returnQtyLocal))
                        {
                            if (returnQtyLocal < 0) returnQtyLocal = 0;
                            row.Cells["Amount"].Value = returnQtyLocal * unitPrice;
                        }
                        else
                        {
                            row.Cells["Amount"].Value = qty * unitPrice;
                        }

                        // Update totals
                        UpdateTotalAmount();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating row amount: {ex.Message}");
            }
        }

        private void ultraGrid1_BeforeCellActivate(object sender, CancelableCellEventArgs e)
        {
            try
            {
                // Make sure editable cells aren't blocked
                if (e.Cell != null)
                {
                    if (e.Cell.Column.Key == "Unit" ||
                        e.Cell.Column.Key == "UnitPrice" ||
                        e.Cell.Column.Key == "Qty" ||
                        e.Cell.Column.Key == "ReturnQty" ||
                        e.Cell.Column.Key == "Reason")
                    {
                        // Ensure this cell is editable by setting its activation directly
                        e.Cell.Activation = Activation.AllowEdit;
                        e.Cell.Column.CellActivation = Activation.AllowEdit;

                        // Don't cancel the event
                        e.Cancel = false;
                    }
                }
            }
            catch
            {
            }
        }

        private void ultraGrid1_ClickCell(object sender, ClickCellEventArgs e)
        {
            try
            {
                // Check if the clicked cell is the header of the Select column
                if (e.Cell != null && e.Cell.Column.Key == "Select")
                {
                    // Check if this is a header cell by checking the type
                    if (e.Cell.GetType().Name.Contains("HeaderCell"))
                    {
                        // Skip header checkbox handling here - will be handled by ultraGrid1_SelectAllClickCell
                        // This prevents duplicate execution or interference between handlers
                        return;
                    }
                    // If it's a regular cell (not header), update the row appearance after the automatic toggle
                    else
                    {
                        // The checkbox will be toggled automatically by the grid
                        // We just need to update the appearance in the next UI cycle
                        this.BeginInvoke(new Action(() =>
                        {
                            // Only the active row should be highlighted by Infragistics
                            // No need to manually color rows based on selection

                            // Update totals
                            UpdateTotalAmount();
                        }));
                    }
                }
                // Special handling for Unit cells to ensure they always become editable
                else if (e.Cell != null && e.Cell.Column.Key == "Unit")
                {
                    // Explicit settings to ensure editability
                    e.Cell.Activation = Activation.AllowEdit;
                    e.Cell.Column.CellActivation = Activation.AllowEdit;

                    // Activate the cell
                    e.Cell.Activate();
                    ultraGrid1.ActiveCell = e.Cell;

                    // Force the grid's click action to ensure edit mode works
                    ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                    // Try multiple approaches to enter edit mode
                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                    Application.DoEvents();
                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                }
                // Handle other editable cells
                else if (e.Cell != null)
                {
                    if (e.Cell.Column.Key == "UnitPrice" ||
                        e.Cell.Column.Key == "Qty" ||
                        e.Cell.Column.Key == "Reason")
                    {
                        e.Cell.Activate();
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);

                        // Special handling for Reason cells - show dropdown immediately
                        if (e.Cell.Column.Key == "Reason")
                        {
                            ShowDropDownForReasonCell();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public void AddItemToGrid(string itemId, string itemName, string barcode, string unit, decimal unitPrice, int qty, decimal amount, double taxPer = 0, double taxAmt = 0, string taxType = "incl", double returnedQty = 0, double returnQty = 0)
        {
            try
            {
                // Set a flag to prevent automatic reset of payment method
                skipPaymentMethodReset = true;

                // Check if item already exists in grid
                bool itemExists = false;
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells["Barcode"].Value?.ToString() == barcode)
                    {
                        decimal existingQty = 0;
                        decimal.TryParse(row.Cells["Qty"].Value?.ToString(), out existingQty);
                        decimal newQty = existingQty + qty;
                        row.Cells["Qty"].Value = newQty;

                        // Update ReturnQty as well for existing items
                        decimal existingReturnQty = 0;
                        if (row.Cells.Exists("ReturnQty"))
                        {
                            decimal.TryParse(row.Cells["ReturnQty"].Value?.ToString(), out existingReturnQty);
                            row.Cells["ReturnQty"].Value = existingReturnQty + qty;

                            // Recalculate amount based on ReturnQty
                            row.Cells["Amount"].Value = (existingReturnQty + qty) * unitPrice;
                        }
                        else
                        {
                            // Fallback if ReturnQty column doesn't exist
                            row.Cells["Amount"].Value = newQty * unitPrice;
                        }

                        itemExists = true;

                        // Activate the Unit cell of this row
                        row.Cells["Unit"].Activate();
                        ultraGrid1.ActiveCell = row.Cells["Unit"];
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                        ultraGrid1.Focus();
                        Application.DoEvents(); // Ensure UI updates
                        break;
                    }
                }

                // Add new row if item doesn't exist
                if (!itemExists)
                {
                    // Get item data including cost and packing from database using the barcode
                    decimal cost = 0;
                    decimal packing = 1; // Default packing value
                    if (!string.IsNullOrEmpty(barcode))
                    {
                        DataBase.Operations = "GETITEMBYBARCODE";
                        ItemDDlGrid itemData = dp.itemDDlGrid(barcode, null);
                        if (itemData != null && itemData.List != null && itemData.List.Any())
                        {
                            var item = itemData.List.First();
                            cost = (decimal)item.Cost;
                            packing = (decimal)item.Packing; // Get actual packing from item master
                        }
                    }

                    DataTable dt = (DataTable)ultraGrid1.DataSource;
                    DataRow newRow = dt.NewRow();

                    newRow["SlNo"] = dt.Rows.Count + 1;
                    newRow["ItemId"] = itemId;
                    newRow["ItemName"] = itemName;
                    newRow["Barcode"] = barcode;
                    newRow["Unit"] = unit;
                    newRow["UnitPrice"] = unitPrice;
                    newRow["Qty"] = qty; // Original sold quantity

                    // Set return quantities based on parameters
                    // If returnQty is provided and > 0, use it; otherwise calculate available quantity
                    double availableQty = Math.Max(0, qty - returnedQty);
                    newRow["ReturnQty"] = returnQty > 0 ? returnQty : availableQty; // Default to available quantity
                    newRow["ReturnedQty"] = returnedQty; // Previously returned quantity from parameter

                    newRow["Packing"] = packing; // Use actual packing from item master
                    newRow["Cost"] = cost; // Use actual cost from database
                    newRow["Reason"] = "Select Reason";

                    // Calculate tax values for this item using passed parameters
                    double sellingPrice = (double)(qty * unitPrice);
                    double taxPercentage = taxPer; // Use passed tax percentage
                    string taxTypeValue = taxType; // Use passed tax type

                    // Calculate tax amount and total with tax (with rounding for GST compliance)
                    double taxAmount = Math.Round(CalculateTaxAmount(sellingPrice, taxPercentage, taxTypeValue), 2);
                    double totalWithTax = Math.Round(CalculateTotalWithTax(sellingPrice, taxPercentage, taxTypeValue), 2);

                    // Calculate BaseAmount (taxable value before tax) for GST compliance
                    double baseAmount;
                    if (taxTypeValue.ToLower() == "incl" && taxPercentage > 0)
                    {
                        double divisor = 1.0 + (taxPercentage / 100.0);
                        baseAmount = Math.Round(sellingPrice / divisor, 2);
                    }
                    else
                    {
                        baseAmount = sellingPrice;
                    }

                    newRow["TaxPer"] = taxPercentage;
                    newRow["TaxAmt"] = taxAmount;
                    newRow["TaxType"] = taxType;
                    newRow["BaseAmount"] = baseAmount; // Store taxable value for GST compliance

                    // Update amount to include tax if needed
                    newRow["Amount"] = totalWithTax; // Amount calculated on ReturnQty with tax
                    newRow["Select"] = true; // Default to selected for sales return items from bills

                    dt.Rows.Add(newRow);
                    ultraGrid1.DataSource = dt;

                    // Activate the Unit cell of the new row
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        int lastRowIndex = ultraGrid1.Rows.Count - 1;
                        ultraGrid1.Rows[lastRowIndex].Cells["Unit"].Activate();
                        ultraGrid1.ActiveCell = ultraGrid1.Rows[lastRowIndex].Cells["Unit"];
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                        ultraGrid1.Focus();
                        Application.DoEvents(); // Ensure UI updates
                    }
                }

                UpdateTotalAmount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding item to grid: " + ex.Message);
            }
            finally
            {
                // Reset the flag after a brief delay to allow other operations to complete
                System.Threading.Timer timer = new System.Threading.Timer((state) =>
                {
                    skipPaymentMethodReset = false;
                }, null, 2000, Timeout.Infinite); // Reset after 2 seconds
            }
        }

        private void frmSalesReturn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7)
            {
                // If there are existing items from an invoice, clear them first
                if (!string.IsNullOrEmpty(textBox1.Value?.ToString()) && textBox1.Value?.ToString() != "without bill")
                {
                    // Clear the form but preserve customer selection
                    string customerName = customerTextBox.Value?.ToString() ?? "";
                    object customerTag = customerTextBox.Tag;
                    ClearForm();
                    customerTextBox.Value = customerName;
                    customerTextBox.Tag = customerTag;
                }

                // Set textBox1 to indicate items are being added without a bill
                textBox1.Value = "without bill";

                // Open the item selection dialog with parameter indicating it's from Sales Return
                frmdialForItemMaster itemDialog = new frmdialForItemMaster("frmSalesReturn");
                itemDialog.Owner = this;

                // Set KeyPreview to handle ESC key at the form level
                itemDialog.KeyPreview = true;

                // Add KeyDown handler to catch ESC key
                itemDialog.KeyDown += (s, args) =>
                {
                    if (args.KeyCode == Keys.Escape)
                    {
                        // Close the item dialog when ESC is pressed
                        itemDialog.Close();
                        args.Handled = true; // Mark as handled to prevent bubbling
                    }
                };

                itemDialog.ShowDialog();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // Check if any modal forms are open and close them in the correct order
                Form activeModalForm = null;

                foreach (Form form in Application.OpenForms)
                {
                    if (form != this && form.Modal && form.Visible)
                    {
                        activeModalForm = form;
                        break;
                    }
                }

                if (activeModalForm != null)
                {
                    // Close the active modal form first
                    activeModalForm.Close();
                }
                else
                {
                    // Show confirmation dialog for closing
                    DialogResult result = MessageBox.Show("Are you sure you want to close?", "Confirm Close",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // If no modal forms are open, close this form
                        this.Close();
                    }
                }
            }
            else if (e.KeyCode == Keys.F1)
            {
                ClearForm();
            }
            else if (e.KeyCode == Keys.F8)
            {
                try
                {
                    // Validate payment method first
                    if (!ValidatePaymentMethod())
                    {
                        return;
                    }

                    // Show confirmation dialog
                    DialogResult result = MessageBox.Show("Save goods return?", "Confirm Save",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Save the sales return data when F8 is pressed
                        SaveSalesReturnWithCreditNote();
                        e.Handled = true; // Mark the event as handled

                        // Find the parent TabPage and TabControl
                        if (this.Parent is TabPage tabPage && tabPage.Parent is TabControl tabControl)
                        {
                            // Remove the TabPage from the TabControl
                            tabControl.TabPages.Remove(tabPage);
                        }

                        // Close the form itself
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while saving and closing: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (e.KeyCode == Keys.F4)
            {
                try
                {
                    // Show confirmation dialog for closing
                    DialogResult result = MessageBox.Show("Are you sure you want to close?", "Confirm Close",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Find the parent TabPage and TabControl
                        if (this.Parent is TabPage tabPage && tabPage.Parent is TabControl tabControl)
                        {
                            // Remove the TabPage from the TabControl
                            tabControl.TabPages.Remove(tabPage);
                        }

                        // Close the form itself
                        this.Close();

                        e.Handled = true; // Mark the event as handled
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error closing form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (e.KeyCode == Keys.F12) // Add F12 key handler for delete
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete this sales return?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    DeleteSalesReturn();
                    e.Handled = true;
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Check if customer is selected and bill number is entered
                // Customer is required for bill-based returns
                int customerId = customerTextBox.Tag != null ? Convert.ToInt32(customerTextBox.Tag) : 0;
                if (customerId <= 0)
                {
                    MessageBox.Show("Please select a customer first to load items from bills", "No Customer Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string billNoStr = textBox1.Text.Trim();
                if (string.IsNullOrWhiteSpace(billNoStr))
                {
                    MessageBox.Show("Please enter a bill number", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // If current data source is not None or Bill, show confirmation message
                if (currentDataSource != DataSource.None && currentDataSource != DataSource.Bill)
                {
                    DialogResult result = MessageBox.Show(
                        "You already have items added from barcode/direct entry. Loading data from bill will clear the current items. Do you want to continue?",
                        "Confirm Change",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                    {
                        return;
                    }

                    // Clear existing items
                    ClearGridOnly();
                }

                // Try to parse the bill number
                Int64 billNo;
                if (!Int64.TryParse(billNoStr, out billNo))
                {
                    MessageBox.Show("Please enter a valid bill number", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the bill exists and belongs to the selected customer
                Repository.TransactionRepository.SalesRepository salesRepo = new Repository.TransactionRepository.SalesRepository();
                var bills = salesRepo.GetBillsByCustomer(customerId);
                var bill = bills?.FirstOrDefault(b => b.BillNo == billNo);

                if (bill == null)
                {
                    MessageBox.Show("Bill not found for this customer", "Bill Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Set the data source to Bill
                currentDataSource = DataSource.Bill;

                // Set the bill information in the form
                textBox1.Value = bill.BillNo.ToString();
                txtNetTotal.Text = bill.NetAmount.ToString();
                dtInvoiceDate.Value = bill.BillDate;
                TxtInvoiceAmnt.Value = bill.NetAmount.ToString();

                // Get bill details and show in the item selection form
                SalesReturnDetailsGrid SRD = operations.GetByIdSRDWithAvailableQty(billNo);
                if (SRD != null && SRD.List != null && SRD.List.Any())
                {
                    frmSalesReturnItem itemForm = new frmSalesReturnItem(this);
                    itemForm.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
                    itemForm.ultraGrid1.DataSource = SRD.List;

                    // Set KeyPreview to true to handle ESC key at the form level
                    itemForm.KeyPreview = true;

                    // Add KeyDown event handler to handle ESC key
                    itemForm.KeyDown += (s, args) =>
                    {
                        if (args.KeyCode == Keys.Escape)
                        {
                            // Close the item form when ESC is pressed
                            itemForm.Close();
                            args.Handled = true;
                        }
                    };

                    itemForm.ShowDialog();

                    // After dialog closes, set focus to the UltraGrid's first row's "Unit" cell
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.Rows[0].Cells["Unit"].Activate();
                        ultraGrid1.ActiveCell = ultraGrid1.Rows[0].Cells["Unit"];
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                        ultraGrid1.Focus();
                        Application.DoEvents();
                    }
                }
                else
                {
                    MessageBox.Show("No items found for this bill", "No Items", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btn_Add_Custm_Click(object sender, EventArgs e)
        {
            // Check if a customer is selected
            int customerId = customerTextBox.Tag != null ? Convert.ToInt32(customerTextBox.Tag) : 0;
            if (customerId <= 0)
            {
                MessageBox.Show("Please select a customer first", "No Customer Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // If current data source is not None or Bill, show confirmation message
            if (currentDataSource != DataSource.None && currentDataSource != DataSource.Bill)
            {
                DialogResult result = MessageBox.Show(
                    "You already have items added from barcode/direct entry. Loading data from bills will clear the current items. Do you want to continue?",
                    "Confirm Change",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    return;
                }
            }

            // Check if customer has any bills before opening the bills dialog
            Repository.TransactionRepository.SalesRepository salesRepo = new Repository.TransactionRepository.SalesRepository();
            var bills = salesRepo.GetBillsByCustomer(customerId);

            if (bills != null && bills.Any())
            {
                // Clear existing items if switching from a different data source
                if (currentDataSource != DataSource.Bill && ultraGrid1.Rows.Count > 0)
                {
                    ClearGridOnly();
                }

                // Update data source
                currentDataSource = DataSource.Bill;

                // Show bills dialog
                using (frmBillsDialog billsDialog = new frmBillsDialog(customerId, "frmSalesReturn"))
                {
                    // Set KeyPreview to true to handle ESC key at the form level
                    billsDialog.KeyPreview = true;

                    // Add KeyDown event handler to handle ESC key
                    billsDialog.KeyDown += (s, args) =>
                    {
                        if (args.KeyCode == Keys.Escape)
                        {
                            // Close the bill dialog when ESC is pressed
                            billsDialog.Close();
                            args.Handled = true;
                        }
                    };

                    // Always show the dialog - it will handle the "no bills" scenario internally
                    DialogResult result = billsDialog.ShowDialog(this);

                    // After dialog closes, force the Unit cell to be editable if data was loaded
                    if (result == DialogResult.OK && ultraGrid1.Rows.Count > 0)
                    {
                        // Use a delay to make sure data is fully loaded first
                        System.Threading.Thread.Sleep(100);
                        ForceUnitCellEditMode();
                    }
                }
            }
            else
            {
                MessageBox.Show("No invoices found for this customer", "No Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (currentDataSource == DataSource.Bill)
            {
                MessageBox.Show("You have already loaded items from bills. This data comes from saved transactions and cannot be mixed with other sales returns.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Only show confirmation if we're switching from a different data source and there are existing items
            if (currentDataSource != DataSource.DirectItem && ultraGrid1.Rows.Count > 0)
            {
                DialogResult result = MessageBox.Show(
                    "Loading data from Returned items will replace your current items. Do you want to continue?",
                    "Confirm Change",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    return;
                }

                ClearGridOnly();
            }

            // Set data source to DirectItem after confirmation
            currentDataSource = DataSource.DirectItem;

            frmCommonDialog com = new frmCommonDialog();
            // Subscribe to the OnDataSelected event
            com.OnDataSelected += Com_OnDataSelected;
            com.ShowDialog();

            // After dialog closes, ensure focus goes to the grid if items exist
            if (ultraGrid1.Rows.Count > 0)
            {
                // Delay slightly to ensure data is loaded
                System.Threading.Thread.Sleep(100);

                // Force Unit cell to be editable
                ForceUnitCellEditMode();
            }
        }

        private void Com_OnDataSelected(SRgetAll selectedData)
        {
            if (selectedData != null)
            {

                // Set the data source to DirectItem to prevent confirmation dialog on subsequent selections
                currentDataSource = DataSource.DirectItem;
            }

            try
            {
                if (selectedData != null)
                {
                    // Get invoice number and payment method directly from database
                    string invoiceNo;
                    int paymodeId;
                    string paymodeName;
                    operations.GetInvoiceAndPaymentInfo(selectedData.SReturnNo, out invoiceNo, out paymodeId, out paymodeName);

                    // Add debug output for payment method

                    // Important: Store payment information before clearing form
                    int cachedPaymodeId = paymodeId;
                    if (cachedPaymodeId == 0 && selectedData.PaymodeID > 0)
                    {
                        cachedPaymodeId = selectedData.PaymodeID;
                    }

                    string cachedPaymodeName = paymodeName;
                    if (string.IsNullOrEmpty(cachedPaymodeName) && !string.IsNullOrEmpty(selectedData.Paymode))
                    {
                        cachedPaymodeName = selectedData.Paymode;
                    }

                    // Ensure RefreshPaymode is called before using the control
                    // This will ensure the payment methods are properly loaded
                    RefreshPaymode();

                    // Set flag to skip payment method reset during ClearForm
                    skipPaymentMethodReset = true;

                    // Clear any existing data first
                    ClearForm();

                    // Load the master data into the form controls
                    TxtSRNO.Value = selectedData.SReturnNo.ToString();
                    dtSReturnDate.Value = selectedData.SReturnDate;

                    // Set invoice number
                    if (!string.IsNullOrEmpty(invoiceNo))
                    {
                        textBox1.Value = invoiceNo;
                    }
                    else if (!string.IsNullOrEmpty(selectedData.InvoiceNo))
                    {
                        textBox1.Value = selectedData.InvoiceNo;
                    }

                    // Get full sales return data to get customer ID
                    SalesReturn fullData = operations.GetBySalesReturnId(selectedData.SReturnNo);
                    if (fullData != null)
                    {
                        // Set customer data
                        if (!string.IsNullOrEmpty(selectedData.CustomerName))
                        {
                            customerTextBox.Value = selectedData.CustomerName;
                            // Store customer ID in tag
                            customerTextBox.Tag = fullData.LedgerID;

                            // Show customer ID in label1 and hide "C ID" text
                            if (fullData.LedgerID > 0)
                            {
                                // Find label1 using Controls.Find
                                Control[] controls = this.Controls.Find("label1", true);
                                if (controls.Length > 0)
                                {
                                    Label label1 = controls[0] as Label;
                                    if (label1 != null)
                                    {
                                        // Update the label to show only the customer ID
                                        label1.Text = fullData.LedgerID.ToString();
                                    }
                                }
                            }
                        }

                        // Debug: Output payment information from all sources

                        // Use the most reliable payment info available (prioritizing)
                        if (cachedPaymodeId == 0 && fullData.PaymodeID > 0)
                        {
                            cachedPaymodeId = fullData.PaymodeID;
                        }

                        if (string.IsNullOrEmpty(cachedPaymodeName) && !string.IsNullOrEmpty(fullData.Paymode))
                        {
                            cachedPaymodeName = fullData.Paymode;
                        }

                        // More reliable way to set payment method - first check if cached method matches an item
                        bool paymentSet = false;

                        // Direct attempt to set payment method using ID
                        if (cachedPaymodeId > 0)
                        {
                            try
                            {
                                cmbPaymntMethod.Value = cachedPaymodeId;
                                paymentSet = cmbPaymntMethod.Value != null &&
                                            Convert.ToInt32(cmbPaymntMethod.Value) == cachedPaymodeId;

                                if (paymentSet)
                                {
                                }
                            }
                            catch
                            {
                            }
                        }

                        // If setting by ID failed, try setting by index
                        if (!paymentSet && cachedPaymodeId > 0)
                        {
                            for (int i = 0; i < cmbPaymntMethod.Items.Count; i++)
                            {
                                Infragistics.Win.ValueListItem item = cmbPaymntMethod.Items[i] as Infragistics.Win.ValueListItem;
                                if (item != null && item.DataValue is DataRowView)
                                {
                                    DataRowView row = item.DataValue as DataRowView;
                                    if (Convert.ToInt32(row["PayModeID"]) == cachedPaymodeId)
                                    {
                                        cmbPaymntMethod.SelectedIndex = i;
                                        paymentSet = true;
                                        break;
                                    }
                                }
                            }
                        }

                        // If still not set, try using the name
                        if (!paymentSet && !string.IsNullOrEmpty(cachedPaymodeName))
                        {
                            for (int i = 0; i < cmbPaymntMethod.Items.Count; i++)
                            {
                                Infragistics.Win.ValueListItem item = cmbPaymntMethod.Items[i] as Infragistics.Win.ValueListItem;
                                if (item != null && item.DataValue is DataRowView)
                                {
                                    DataRowView row = item.DataValue as DataRowView;
                                    if (row["PayModeName"].ToString().Equals(cachedPaymodeName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        cmbPaymntMethod.SelectedIndex = i;
                                        paymentSet = true;
                                        break;
                                    }
                                }
                            }
                        }

                        // Last resort: If still not set but we have valid IDs, add the item to the combo
                        if (!paymentSet && (cachedPaymodeId > 0 || !string.IsNullOrEmpty(cachedPaymodeName)))
                        {
                            // Add payment method to list if not found
                            DataTable dt = cmbPaymntMethod.DataSource as DataTable;
                            if (dt != null)
                            {
                                DataRow newRow = dt.NewRow();
                                newRow["PayModeID"] = cachedPaymodeId > 0 ? cachedPaymodeId : 999; // Use fallback ID if needed
                                newRow["PayModeName"] = !string.IsNullOrEmpty(cachedPaymodeName) ? cachedPaymodeName : "Unknown";
                                dt.Rows.Add(newRow);

                                // Try to select the newly added item
                                cmbPaymntMethod.SelectedIndex = cmbPaymntMethod.Items.Count - 1;
                            }
                        }
                    }

                    // Load sales return details into the grid
                    LoadSalesReturnDetails(selectedData.SReturnNo);

                    // Hide save button and show update button - this is critical for edit mode
                    if (pbxSave != null)
                    {
                        pbxSave.Visible = false;
                    }

                    // Find and show the update button
                    Control[] updateButtons = this.Controls.Find("updtbtn", true);
                    if (updateButtons.Length > 0)
                    {
                        Button updtbtn = updateButtons[0] as Button;
                        if (updtbtn != null)
                        {
                            updtbtn.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading sales return: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset the flag
                skipPaymentMethodReset = false;
            }
        }

        // Helper method to find update button recursively
        private void GetUpdateButtonRecursive(Control control)
        {
            if (control.Name.ToLower() == "updtbtn" && control is Button)
            {
                control.Visible = true;
            }

            foreach (Control child in control.Controls)
            {
                GetUpdateButtonRecursive(child);
            }
        }

        // Helper method to get all controls of a form recursively
        private IEnumerable<Control> GetAllControls()
        {
            var controls = new List<Control>();
            GetAllControlsRecursive(this, controls);
            return controls;
        }

        private void GetAllControlsRecursive(Control control, List<Control> controls)
        {
            controls.Add(control);

            foreach (Control child in control.Controls)
            {
                GetAllControlsRecursive(child, controls);
            }
        }

        private void LoadSalesReturnDetails(int returnNo)
        {
            try
            {
                // Get the details data using the stored procedure
                DataTable details = operations.GetSalesReturnDetails(returnNo);

                // Add debugging - check if details are empty
                if (details == null || details.Rows.Count == 0)
                {
                    MessageBox.Show("No details found for Sales Return #" + returnNo, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }


                // Clear the existing grid data
                if (ultraGrid1.DataSource != null)
                {
                    DataTable dt = (DataTable)ultraGrid1.DataSource;
                    dt.Clear(); // Just clear rows, don't reset the DataSource

                    // Debug info - column names
                    string columnNames = string.Join(", ", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName));

                    // Print details column names for debugging
                    string detailColumns = string.Join(", ", details.Columns.Cast<DataColumn>().Select(c => c.ColumnName));

                    // Loop through the details and add to the grid
                    foreach (DataRow row in details.Rows)
                    {
                        DataRow newRow = dt.NewRow();

                        // Use TryParse methods to handle potential data conversion issues
                        int slNo;
                        if (row["SlNo"] != DBNull.Value && int.TryParse(row["SlNo"].ToString(), out slNo))
                            newRow["SlNo"] = slNo;

                        // Add ItemId if it exists in both tables
                        if (dt.Columns.Contains("ItemId") && details.Columns.Contains("ItemId") && row["ItemId"] != DBNull.Value)
                            newRow["ItemId"] = row["ItemId"];

                        newRow["ItemName"] = row["ItemName"] != DBNull.Value ? row["ItemName"].ToString() : string.Empty;
                        
                        // Fix for "Column 'BarCode' does not belong to table" error
                        if (details.Columns.Contains("BarCode") && row["BarCode"] != DBNull.Value)
                        {
                            newRow["Barcode"] = row["BarCode"].ToString();
                        }
                        else
                        {
                            newRow["Barcode"] = string.Empty;
                        }

                        newRow["Unit"] = row["Unit"] != DBNull.Value ? row["Unit"].ToString() : string.Empty;

                        decimal unitPrice = 0;
                        if (row["UnitPrice"] != DBNull.Value && decimal.TryParse(row["UnitPrice"].ToString(), out unitPrice))
                            newRow["UnitPrice"] = unitPrice;
                        else if (row["SalesPrice"] != DBNull.Value && decimal.TryParse(row["SalesPrice"].ToString(), out unitPrice))
                            newRow["UnitPrice"] = unitPrice;

                        decimal qty = 0;
                        if (row["Qty"] != DBNull.Value && decimal.TryParse(row["Qty"].ToString(), out qty))
                            newRow["Qty"] = qty;

                        // Preserve ReturnQty from DB so user sees previously returned quantity
                        decimal returnQtyFromDB = 0;
                        if (details.Columns.Contains("ReturnQty") && row["ReturnQty"] != DBNull.Value && decimal.TryParse(row["ReturnQty"].ToString(), out returnQtyFromDB))
                        {
                            newRow["ReturnQty"] = returnQtyFromDB;
                        }
                        else
                        {
                            newRow["ReturnQty"] = 0m;
                        }

                        // Preserve ReturnedQty if provided; default to 0
                        decimal returnedQtyFromDB = 0;
                        if (details.Columns.Contains("ReturnedQty") && row["ReturnedQty"] != DBNull.Value && decimal.TryParse(row["ReturnedQty"].ToString(), out returnedQtyFromDB))
                        {
                            newRow["ReturnedQty"] = returnedQtyFromDB;
                        }
                        else
                        {
                            newRow["ReturnedQty"] = 0m;
                        }

                        decimal packing = DEFAULT_PACKING; // Default packing value
                        if (row["Packing"] != DBNull.Value && decimal.TryParse(row["Packing"].ToString(), out packing))
                            newRow["Packing"] = packing;
                        else
                            newRow["Packing"] = DEFAULT_PACKING; // Default to 1 if not available

                        decimal cost = DEFAULT_COST; // Default cost value
                        if (row["Cost"] != DBNull.Value && decimal.TryParse(row["Cost"].ToString(), out cost))
                            newRow["Cost"] = cost;
                        else
                            newRow["Cost"] = DEFAULT_COST; // Default to 0 if not available

                        newRow["Reason"] = row["Reason"] != DBNull.Value ? row["Reason"].ToString() : DEFAULT_REASON;

                        // Load tax information from database
                        decimal taxPer = 0;
                        if (details.Columns.Contains("TaxPer") && row["TaxPer"] != DBNull.Value && decimal.TryParse(row["TaxPer"].ToString(), out taxPer))
                            newRow["TaxPer"] = taxPer;
                        else
                            newRow["TaxPer"] = 0;

                        decimal taxAmt = 0;
                        if (details.Columns.Contains("TaxAmt") && row["TaxAmt"] != DBNull.Value && decimal.TryParse(row["TaxAmt"].ToString(), out taxAmt))
                            newRow["TaxAmt"] = taxAmt;
                        else
                            newRow["TaxAmt"] = 0;

                        // Set tax type
                        if (details.Columns.Contains("TaxType") && row["TaxType"] != DBNull.Value && !string.IsNullOrEmpty(row["TaxType"].ToString()))
                            newRow["TaxType"] = row["TaxType"].ToString();
                        else
                            newRow["TaxType"] = "excl";

                        // For saved returns, Amount should reflect the value of what was actually returned
                        decimal amount = 0;
                        decimal computedAmount = unitPrice * Convert.ToDecimal(newRow["ReturnedQty"]);
                        if (row["Amount"] != DBNull.Value && decimal.TryParse(row["Amount"].ToString(), out amount))
                            newRow["Amount"] = computedAmount;
                        else
                            newRow["Amount"] = computedAmount;

                        // For loaded items from existing sales returns, they should be selected by default
                        // since they are being loaded for potential update/editing
                        if (row["Select"] != DBNull.Value)
                            newRow["Select"] = Convert.ToBoolean(row["Select"]);
                        else
                            newRow["Select"] = true; // Default to checked for existing sales return items

                        // Add the row to the DataTable
                        dt.Rows.Add(newRow);

                        // Debug - print what we added
                    }

                    // Critical: Reset all editable columns after adding data
                    ResetGridEditableColumns();

                    // Ensure editable columns are properly set
                    foreach (UltraGridColumn column in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (column.Key == "Unit" || column.Key == "UnitPrice" || column.Key == "Qty" || column.Key == "Reason")
                        {
                            column.CellActivation = Activation.AllowEdit;
                        }
                    }



                    // Update the total amount
                    UpdateTotalAmount();

                    // Force the grid to refresh and redraw
                    ultraGrid1.Refresh();

                    // First focus the Unit cell
                    FocusFirstUnitCell();
                }
                else
                {
                    // If ultraGrid1.DataSource is null, we need to create a new DataTable
                    DataTable dt = new DataTable();

                    // Add columns to match your grid structure
                    dt.Columns.Add("SlNo", typeof(int));
                    dt.Columns.Add("ItemId", typeof(long));
                    dt.Columns.Add("ItemName", typeof(string));
                    dt.Columns.Add("Barcode", typeof(string));
                    dt.Columns.Add("Unit", typeof(string));
                    dt.Columns.Add("UnitPrice", typeof(decimal));
                    dt.Columns.Add("Qty", typeof(decimal));
                    dt.Columns.Add("ReturnQty", typeof(decimal));
                    dt.Columns.Add("ReturnedQty", typeof(decimal));
                    dt.Columns.Add("Packing", typeof(decimal));
                    dt.Columns.Add("Cost", typeof(decimal));
                    dt.Columns.Add("Amount", typeof(decimal));
                    dt.Columns.Add("TaxPer", typeof(decimal)); // Tax percentage column
                    dt.Columns.Add("TaxAmt", typeof(decimal)); // Tax amount column
                    dt.Columns.Add("TaxType", typeof(string)); // Tax type column (incl/excl)
                    dt.Columns.Add("Reason", typeof(string));
                    dt.Columns.Add("Select", typeof(bool));

                    // Loop through and add rows from details
                    foreach (DataRow row in details.Rows)
                    {
                        DataRow newRow = dt.NewRow();

                        // Use TryParse methods to handle potential data conversion issues
                        int slNo;
                        if (row["SlNo"] != DBNull.Value && int.TryParse(row["SlNo"].ToString(), out slNo))
                            newRow["SlNo"] = slNo;

                        // Add ItemId if it exists
                        if (details.Columns.Contains("ItemId") && row["ItemId"] != DBNull.Value)
                            newRow["ItemId"] = row["ItemId"];

                        newRow["ItemName"] = row["ItemName"] != DBNull.Value ? row["ItemName"].ToString() : string.Empty;
                        newRow["Barcode"] = row["BarCode"] != DBNull.Value ? row["BarCode"].ToString() : string.Empty;
                        newRow["Unit"] = row["Unit"] != DBNull.Value ? row["Unit"].ToString() : string.Empty;

                        decimal unitPrice = 0;
                        if (row["UnitPrice"] != DBNull.Value && decimal.TryParse(row["UnitPrice"].ToString(), out unitPrice))
                            newRow["UnitPrice"] = unitPrice;
                        else if (row["SalesPrice"] != DBNull.Value && decimal.TryParse(row["SalesPrice"].ToString(), out unitPrice))
                            newRow["UnitPrice"] = unitPrice;

                        decimal qty = 0;
                        if (row["Qty"] != DBNull.Value && decimal.TryParse(row["Qty"].ToString(), out qty))
                            newRow["Qty"] = qty;

                        // Preserve ReturnQty from DB so user sees previously returned quantity
                        decimal returnQtyFromDB = 0;
                        if (details.Columns.Contains("ReturnQty") && row["ReturnQty"] != DBNull.Value && decimal.TryParse(row["ReturnQty"].ToString(), out returnQtyFromDB))
                        {
                            newRow["ReturnQty"] = returnQtyFromDB;
                        }
                        else
                        {
                            newRow["ReturnQty"] = 0m;
                        }

                        // Preserve ReturnedQty if provided; default to 0
                        decimal returnedQtyFromDB = 0;
                        if (details.Columns.Contains("ReturnedQty") && row["ReturnedQty"] != DBNull.Value && decimal.TryParse(row["ReturnedQty"].ToString(), out returnedQtyFromDB))
                        {
                            newRow["ReturnedQty"] = returnedQtyFromDB;
                        }
                        else
                        {
                            newRow["ReturnedQty"] = 0m;
                        }

                        decimal packing = DEFAULT_PACKING; // Default packing value
                        if (row["Packing"] != DBNull.Value && decimal.TryParse(row["Packing"].ToString(), out packing))
                            newRow["Packing"] = packing;
                        else
                            newRow["Packing"] = DEFAULT_PACKING; // Default to 1 if not available

                        decimal cost = DEFAULT_COST; // Default cost value
                        if (row["Cost"] != DBNull.Value && decimal.TryParse(row["Cost"].ToString(), out cost))
                            newRow["Cost"] = cost;
                        else
                            newRow["Cost"] = DEFAULT_COST; // Default to 0 if not available

                        newRow["Reason"] = row["Reason"] != DBNull.Value ? row["Reason"].ToString() : DEFAULT_REASON;

                        // Load tax information from database
                        decimal taxPer = 0;
                        if (details.Columns.Contains("TaxPer") && row["TaxPer"] != DBNull.Value && decimal.TryParse(row["TaxPer"].ToString(), out taxPer))
                            newRow["TaxPer"] = taxPer;
                        else
                            newRow["TaxPer"] = 0;

                        decimal taxAmt = 0;
                        if (details.Columns.Contains("TaxAmt") && row["TaxAmt"] != DBNull.Value && decimal.TryParse(row["TaxAmt"].ToString(), out taxAmt))
                            newRow["TaxAmt"] = taxAmt;
                        else
                            newRow["TaxAmt"] = 0;

                        // Set tax type
                        if (details.Columns.Contains("TaxType") && row["TaxType"] != DBNull.Value && !string.IsNullOrEmpty(row["TaxType"].ToString()))
                            newRow["TaxType"] = row["TaxType"].ToString();
                        else
                            newRow["TaxType"] = "excl";

                        // For saved returns, Amount should reflect the value of what was actually returned
                        decimal amount = 0;
                        decimal computedAmount = unitPrice * Convert.ToDecimal(newRow["ReturnedQty"]);
                        if (row["Amount"] != DBNull.Value && decimal.TryParse(row["Amount"].ToString(), out amount))
                            newRow["Amount"] = computedAmount;
                        else
                            newRow["Amount"] = computedAmount;

                        // Default to checked for items from sales return details (they should be selected by default)
                        newRow["Select"] = true;

                        // Add the row to the DataTable
                        dt.Rows.Add(newRow);
                    }

                    // Set the DataSource for the grid
                    ultraGrid1.DataSource = dt;

                    // Configure columns to ensure they're editable
                    foreach (UltraGridColumn column in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (column.Key == "Unit" || column.Key == "UnitPrice" || column.Key == "Qty" || column.Key == "Reason")
                        {
                            column.CellActivation = Activation.AllowEdit;
                        }
                    }



                    // Update the total amount
                    UpdateTotalAmount();

                    // Force focus to the first editable cell
                    FocusFirstUnitCell();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading sales return details: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void pbxExit_Click(object sender, EventArgs e)
        {
            try
            {
                // Show confirmation dialog for closing
                DialogResult result = MessageBox.Show("Are you sure you want to close?", "Confirm Close",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Find the parent TabPage and TabControl
                    if (this.Parent is TabPage tabPage && tabPage.Parent is TabControl tabControl)
                    {
                        // Remove the TabPage from the TabControl
                        tabControl.TabPages.Remove(tabPage);
                    }

                    // Close the form itself
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error closing form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void button2_Click_1(object sender, EventArgs e)
        {
            // First check if we have data in the grid already
            if (ultraGrid1.Rows.Count > 0)
            {
                DialogResult result = MessageBox.Show(
                    "Changing the customer will clear all items in the grid. Do you want to continue?",
                    "Confirm Change",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    return;
                }
            }

            // Clear any existing items and data
            ClearForm();

            // Show customer dialog
            using (frmCustomerDialog customerDialog = new frmCustomerDialog())
            {
                customerDialog.Owner = this;
                if (customerDialog.ShowDialog() == DialogResult.OK)
                {
                    // Update the customer TextBox with selected customer info
                    if (!string.IsNullOrEmpty(frmCustomerDialog.SetValueForText1))
                    {
                        // Parse the customer data (assuming format: "Name|ID")
                        string[] customerData = frmCustomerDialog.SetValueForText1.Split('|');
                        if (customerData.Length >= 2)
                        {
                            customerTextBox.Value = customerData[0];
                            // Safe conversion for customer ID
                            int customerId = 0;
                            if (int.TryParse(customerData[1], out customerId))
                            {
                                customerTextBox.Tag = customerId;
                            }
                        }
                    }
                }
            }
        }

        private void InitializeUltraGrid()
        {
            try
            {
                // Unregister any existing event handlers first to prevent duplicates
                ultraGrid1.InitializeLayout -= new InitializeLayoutEventHandler(ultraGrid1_InitializeLayout);
                ultraGrid1.InitializeLayout -= UltraGrid1_InitializeLayout;
                ultraGrid1.KeyDown -= new KeyEventHandler(ultraGrid1_KeyDown);
                ultraGrid1.KeyPress -= new KeyPressEventHandler(ultraGrid1_KeyPress);
                ultraGrid1.ClickCell -= new ClickCellEventHandler(ultraGrid1_ClickCell);
                ultraGrid1.ClickCell -= new ClickCellEventHandler(ultraGrid1_SelectAllClickCell);
                ultraGrid1.AfterCellUpdate -= new CellEventHandler(ultraGrid1_AfterCellUpdate);
                ultraGrid1.BeforeCellActivate -= new CancelableCellEventHandler(ultraGrid1_BeforeCellActivate);
                ultraGrid1.InitializeRow -= new InitializeRowEventHandler(ultraGrid1_InitializeRow);
                ultraGrid1.BeforeExitEditMode -= new Infragistics.Win.UltraWinGrid.BeforeExitEditModeEventHandler(ultraGrid1_BeforeExitEditMode);
                ultraGrid1.AfterRowActivate -= new EventHandler(ultraGrid1_AfterRowActivate);
                ultraGrid1.AfterSelectChange -= new AfterSelectChangeEventHandler(ultraGrid1_AfterSelectChange);
                ultraGrid1.CellChange -= new Infragistics.Win.UltraWinGrid.CellEventHandler(this.ultraGrid1_CellChange);
                ultraGrid1.AfterHeaderCheckStateChanged -= new Infragistics.Win.UltraWinGrid.AfterHeaderCheckStateChangedEventHandler(ultraGrid1_AfterHeaderCheckStateChanged);

                // Now register all event handlers
                ultraGrid1.InitializeLayout += new InitializeLayoutEventHandler(ultraGrid1_InitializeLayout);
                ultraGrid1.KeyDown += new KeyEventHandler(ultraGrid1_KeyDown);
                ultraGrid1.KeyPress += new KeyPressEventHandler(ultraGrid1_KeyPress);
                ultraGrid1.ClickCell += new ClickCellEventHandler(ultraGrid1_ClickCell);
                ultraGrid1.AfterCellUpdate += new CellEventHandler(ultraGrid1_AfterCellUpdate);
                ultraGrid1.BeforeCellActivate += new CancelableCellEventHandler(ultraGrid1_BeforeCellActivate);
                ultraGrid1.InitializeRow += new InitializeRowEventHandler(ultraGrid1_InitializeRow);
                ultraGrid1.BeforeExitEditMode += new Infragistics.Win.UltraWinGrid.BeforeExitEditModeEventHandler(ultraGrid1_BeforeExitEditMode);
                ultraGrid1.AfterRowActivate += new EventHandler(ultraGrid1_AfterRowActivate);
                ultraGrid1.AfterSelectChange += new AfterSelectChangeEventHandler(ultraGrid1_AfterSelectChange);

                // Register the CellChange handler for checkbox tracking
                ultraGrid1.CellChange += new Infragistics.Win.UltraWinGrid.CellEventHandler(this.ultraGrid1_CellChange);

                // Register the AfterHeaderCheckStateChanged event for "Select All" checkbox handling
                ultraGrid1.AfterHeaderCheckStateChanged += new Infragistics.Win.UltraWinGrid.AfterHeaderCheckStateChangedEventHandler(ultraGrid1_AfterHeaderCheckStateChanged);

                // Create initial DataTable
                DataTable dt = new DataTable();
                dt.Columns.Add("SlNo", typeof(int));
                dt.Columns.Add("ItemId", typeof(int));
                dt.Columns.Add("ItemName", typeof(string));
                dt.Columns.Add("Barcode", typeof(string));
                dt.Columns.Add("Unit", typeof(string));
                dt.Columns.Add("UnitPrice", typeof(decimal));
                dt.Columns.Add("Qty", typeof(decimal));
                dt.Columns.Add("Reason", typeof(string));
                dt.Columns.Add("Amount", typeof(decimal));
                dt.Columns.Add("TaxPer", typeof(decimal)); // Tax percentage column
                dt.Columns.Add("TaxAmt", typeof(decimal)); // Tax amount column
                dt.Columns.Add("TaxType", typeof(string)); // Tax type column (incl/excl)
                dt.Columns.Add("Select", typeof(bool));

                ultraGrid1.DataSource = dt;

                // Define colors for light blue styling (from FrmPurchase.cs)
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders and grid lines
                Color selectedBlue = Color.FromArgb(173, 216, 255); // Light blue for selection

                // Configure the grid appearance with explicit settings for editability
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;

                // CRITICAL: Make sure CellClickAction is set to EditAndSelectText
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                // Apply compact design with reduced spacing
                ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 22; // 22px row height for compact design
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0; // No row spacing for efficiency
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellPadding = 2; // Reduced cell padding (2px)
                ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Appearance.BorderColor = lightBlue; // Set main grid border to light blue

                // Configure header appearance with modern look but keep header color unchanged
                ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(0, 122, 204); // Keep existing header color
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(0, 102, 184); // Keep existing gradient
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False; // Regular font weight
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Segoe UI"; // Modern font
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = lightBlue; // Light blue header borders

                // Configure row appearances with light blue styling
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White; // Clean white rows
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.FromArgb(250, 250, 250); // Very light gray alternating
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue; // Light blue row borders

                // Configure alternate row appearance
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(250, 250, 250); // Very light gray

                // Set selected row appearance with light blue highlighting
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = selectedBlue;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = selectedBlue;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.Black; // Black text for better readability
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;

                // Active row appearance - same as selected row
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = selectedBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = selectedBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.Black;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.False;

                // Configure cell and row borders with light blue
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Segoe UI"; // Modern font throughout
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Bold = DefaultableBoolean.False; // Regular font weight

                // Configure row selector with modern styling
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(0, 122, 204); // Match header
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = Color.FromArgb(0, 102, 184);
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None;

                // Add horizontal scrollbar visibility settings to ensure all columns are visible
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
                ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;

                // Calculate total available width for proper column sizing
                int totalAvailableWidth = ultraGrid1.Width - 25; // Subtract scrollbar width and some padding

                // Calculate appropriate column widths to fit all columns without scrolling
                int slNoWidth = (int)(totalAvailableWidth * 0.07);
                int itemNameWidth = (int)(totalAvailableWidth * 0.20);
                int barcodeWidth = (int)(totalAvailableWidth * 0.11);
                int unitWidth = (int)(totalAvailableWidth * 0.08);
                int unitPriceWidth = (int)(totalAvailableWidth * 0.10);
                int qtyWidth = (int)(totalAvailableWidth * 0.08);
                int reasonWidth = (int)(totalAvailableWidth * 0.16);
                int amountWidth = (int)(totalAvailableWidth * 0.10);
                int selectWidth = (int)(totalAvailableWidth * 0.06);

                // Configure individual columns - ensure Unit cell is ALWAYS editable
                foreach (UltraGridColumn column in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    // Set all columns as non-editable by default
                    column.CellActivation = Activation.NoEdit;

                    switch (column.Key)
                    {
                        case "SlNo":
                            column.Header.Caption = "SlNo";
                            column.Width = slNoWidth;
                            break;
                        case "ItemId":
                            column.Header.Caption = "ItemId";
                            column.Hidden = true;
                            break;
                        case "ItemName":
                            column.Header.Caption = "Item Name";
                            column.Width = itemNameWidth;
                            break;
                        case "Barcode":
                            column.Header.Caption = "Barcode";
                            column.Width = barcodeWidth;
                            break;
                        case "Unit":
                            column.Header.Caption = "Unit";
                            column.Width = unitWidth;
                            column.CellActivation = Activation.AllowEdit; // CRITICAL: Allow editing
                            column.CellAppearance.TextHAlign = HAlign.Center;
                            // Explicitly set CellClickAction for this column
                            column.CellClickAction = CellClickAction.Edit;
                            break;
                        case "UnitPrice":
                            column.Header.Caption = "Unit Price";
                            column.Width = unitPriceWidth;
                            column.Format = "N2";
                            column.CellAppearance.TextHAlign = HAlign.Right;
                            column.CellActivation = Activation.AllowEdit;
                            break;
                        case "Qty":
                            column.Header.Caption = "Qty";
                            column.Width = qtyWidth;
                            column.CellAppearance.TextHAlign = HAlign.Right;
                            column.Format = "N2";
                            column.CellActivation = Activation.AllowEdit;
                            break;
                        case "Reason":
                            column.Header.Caption = "Reason";
                            column.Width = reasonWidth;
                            column.CellActivation = Activation.AllowEdit;
                            column.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownList;

                            // Create a dropdown values
                            Infragistics.Win.ValueList valueList = new Infragistics.Win.ValueList();
                            valueList.ValueListItems.Add(new Infragistics.Win.ValueListItem("Select Reason"));
                            valueList.ValueListItems.Add(new Infragistics.Win.ValueListItem("Damaged"));
                            valueList.ValueListItems.Add(new Infragistics.Win.ValueListItem("Expired"));
                            valueList.ValueListItems.Add(new Infragistics.Win.ValueListItem("Other"));

                            // Set the default item
                            valueList.ValueListItems[0].DataValue = "Select Reason";

                            // Assign the dropdown to the column
                            column.ValueList = valueList;
                            break;
                        case "Amount":
                            column.Header.Caption = "Amount";
                            column.Width = amountWidth;
                            column.Format = "N2";
                            column.CellAppearance.TextHAlign = HAlign.Right;
                            column.CellActivation = Activation.NoEdit;
                            break;
                        case "Select":
                            // Remove caption text for "Select" column, only show checkbox
                            column.Header.Caption = "";
                            column.Width = selectWidth;
                            column.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                            column.CellActivation = Activation.AllowEdit;
                            // Add a checkbox to the header for "Select All" functionality
                            column.Header.CheckBoxVisibility = HeaderCheckBoxVisibility.Always;
                            // Add a handler for the header checkbox click
                            column.Header.CheckBoxAlignment = HeaderCheckBoxAlignment.Center;

                            // Override click behavior for this cell to ensure header checkbox works properly
                            column.CellClickAction = CellClickAction.Edit;
                            break;
                    }
                }

                // Configure spacing and expansion behavior
                ultraGrid1.DisplayLayout.InterBandSpacing = 0; // Reduced spacing for compact design
                ultraGrid1.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;
                ultraGrid1.DisplayLayout.UseFixedHeaders = true;

                // Ensure header checkbox is always visible
                ultraGrid1.DisplayLayout.Bands[0].Columns["Select"].Header.CheckBoxVisibility = HeaderCheckBoxVisibility.Always;

                // Add event handler for cell changes
                this.ultraGrid1.CellChange += new Infragistics.Win.UltraWinGrid.CellEventHandler(this.ultraGrid1_CellChange);

                // Register InitializeLayout event for consistent styling
                ultraGrid1.InitializeLayout += UltraGrid1_InitializeLayout;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing grid: " + ex.Message);
            }
        }

        private void UltraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Define colors for light blue styling (matching FrmPurchase.cs)
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders and grid lines
                Color selectedBlue = Color.FromArgb(173, 216, 255); // Light blue for selection

                // Apply proper grid line styles - solid lines for clean appearance
                e.Layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

                // Set grid line colors to light blue for modern appearance
                e.Layout.Override.RowAppearance.BorderColor = lightBlue;
                e.Layout.Override.CellAppearance.BorderColor = lightBlue;
                e.Layout.Appearance.BorderColor = lightBlue; // Main grid border

                // Set border style for the main grid
                e.Layout.BorderStyle = UIElementBorderStyle.Solid;

                // Apply compact spacing
                e.Layout.Override.CellPadding = 2; // Reduced cell padding for more data visibility
                e.Layout.Override.RowSpacingBefore = 0; // No row spacing for efficient space usage
                e.Layout.Override.RowSpacingAfter = 0;
                e.Layout.Override.CellSpacing = 0;
                e.Layout.InterBandSpacing = 0;

                // Ensure Segoe UI font is applied throughout
                e.Layout.Override.CellAppearance.FontData.Name = "Segoe UI";
                e.Layout.Override.CellAppearance.FontData.Bold = DefaultableBoolean.False;
                e.Layout.Override.HeaderAppearance.FontData.Name = "Segoe UI";
                e.Layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;

                // Apply modern styling to all bands
                if (e.Layout.Bands.Count > 0)
                {
                    UltraGridBand band = e.Layout.Bands[0];

                    // Set default vertical alignment for all cells
                    band.Override.CellAppearance.TextVAlign = VAlign.Middle;
                    band.Override.CellAppearance.FontData.Bold = DefaultableBoolean.False;
                    band.Override.CellAppearance.FontData.Name = "Segoe UI";
                    band.Override.CellAppearance.BorderColor = lightBlue;

                    // Configure individual columns
                    foreach (UltraGridColumn column in band.Columns)
                    {
                        switch (column.Key)
                        {
                            case "SlNo":
                                column.Header.Caption = "SlNo";
                                column.Width = 50;
                                column.CellActivation = Activation.NoEdit;
                                break;
                            case "ItemId":
                                column.Header.Caption = "ItemId";
                                column.Width = 60;
                                column.Hidden = true; // Hide ItemId column
                                break;
                            case "ItemName":
                                column.Header.Caption = "Item Name";
                                column.Width = 150;
                                column.CellActivation = Activation.NoEdit;
                                break;
                            case "Barcode":
                                column.Header.Caption = "Barcode";
                                column.Width = 100;
                                column.CellActivation = Activation.NoEdit;
                                break;
                            case "Unit":
                                column.Header.Caption = "Unit";
                                column.Width = 60;
                                column.CellActivation = Activation.AllowEdit;
                                break;
                            case "UnitPrice":
                                column.Header.Caption = "Unit Price";
                                column.Width = 80;
                                column.CellActivation = Activation.AllowEdit;
                                column.Format = "0.00";
                                break;
                            case "Qty":
                                column.Header.Caption = "Qty";
                                column.Width = 60;
                                column.CellActivation = Activation.NoEdit;
                                column.Format = "0.00";
                                break;
                            case "ReturnQty":
                                column.Header.Caption = "Return Qty";
                                column.Width = 80;
                                column.CellActivation = Activation.AllowEdit;
                                column.Format = "0.00";
                                break;
                            case "ReturnedQty":
                                column.Header.Caption = "Returned Qty";
                                column.Width = 80;
                                column.CellActivation = Activation.NoEdit;
                                column.Format = "0.00";
                                break;
                            case "Packing":
                                column.Header.Caption = "Packing";
                                column.Width = 60;
                                column.CellActivation = Activation.NoEdit;
                                column.Format = "0.00";
                                break;
                            case "Cost":
                                column.Header.Caption = "Cost";
                                column.Width = 60;
                                column.CellActivation = Activation.NoEdit;
                                column.Format = "0.00";
                                break;
                            case "Reason":
                                column.Header.Caption = "Reason";
                                column.Width = 120;
                                column.CellActivation = Activation.AllowEdit;
                                break;
                            case "Amount":
                                column.Header.Caption = "Amount";
                                column.Width = 80;
                                column.CellActivation = Activation.NoEdit;
                                column.Format = "0.00";
                                break;
                            case "TaxPer":
                                column.Header.Caption = "Tax %";
                                column.Width = 60;
                                column.CellActivation = Activation.NoEdit;
                                column.Format = "0.00";
                                break;
                            case "TaxAmt":
                                column.Header.Caption = "Tax Amt";
                                column.Width = 80;
                                column.CellActivation = Activation.NoEdit;
                                column.Format = "0.00";
                                break;
                            case "TaxType":
                                column.Header.Caption = "Tax Type";
                                column.Width = 70;
                                column.CellActivation = Activation.NoEdit;
                                break;
                            case "Select":
                                column.Header.Caption = "Select";
                                column.Width = 60;
                                column.CellActivation = Activation.AllowEdit;
                                column.Header.CheckBoxVisibility = HeaderCheckBoxVisibility.Always;
                                break;
                            default:
                                column.CellActivation = Activation.NoEdit;
                                break;
                        }
                    }
                }

                // Apply selection colors
                e.Layout.Override.SelectedRowAppearance.BackColor = selectedBlue;
                e.Layout.Override.SelectedRowAppearance.BackColor2 = selectedBlue;
                e.Layout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                e.Layout.Override.SelectedRowAppearance.ForeColor = Color.Black;

                e.Layout.Override.ActiveRowAppearance.BackColor = selectedBlue;
                e.Layout.Override.ActiveRowAppearance.BackColor2 = selectedBlue;
                e.Layout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                e.Layout.Override.ActiveRowAppearance.ForeColor = Color.Black;
            }
            catch
            {
            }
        }

        private void ultraGrid1_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            try
            {
                // Set default value for Reason column in each new row
                if (e.Row.Cells["Reason"].Value == null || string.IsNullOrEmpty(e.Row.Cells["Reason"].Value.ToString()))
                {
                    e.Row.Cells["Reason"].Value = "Select Reason";
                }
            }
            catch
            {
            }
        }

        private void lblSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveSalesReturnWithCreditNote();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving sales return: " + ex.Message);
            }
        }

        private void ClearForm()
        {
            try
            {
                // Store source of items before clearing
                bool wasCustomerBill = !string.IsNullOrEmpty(textBox1.Value?.ToString()) && textBox1.Value?.ToString() != "without bill";
                bool wasBarcodeItems = textBox1.Value?.ToString() == "without bill";

                // Reset customer information to default customer
                if (customerTextBox != null)
                {
                    SetupDefaultCustomer();
                }

                // Clear invoice information
                textBox1.Text = "";

                // Reset dates to current date
                dtSReturnDate.Value = DateTime.Now;
                dtInvoiceDate.Value = DateTime.Now;

                // Reset payment method to default (Cash) only if not skipping the reset
                if (!skipPaymentMethodReset)
                {
                    SetupDefaultPaymentMode();
                }
                else
                {
                    // Reset the flag for next time
                    skipPaymentMethodReset = false;
                }

                // Clear grid
                DataTable dt = (DataTable)ultraGrid1.DataSource;

                // Ensure the data table has all columns including Select
                if (!dt.Columns.Contains("Select"))
                {
                    dt.Columns.Add("Select", typeof(bool));
                }

                dt.Rows.Clear();
                ultraGrid1.DataSource = dt;

                // Clear totals
                TxtSubTotal.Text = "0.00";
                txtNetTotal.Text = "0.00";
                TxtInvoiceAmnt.Text = "0.00";

                // Clear barcode textbox
                TxtBarcode.Clear();

                // Clear label1 (Customer ID)
                Label label1 = Controls.Find("label1", true).FirstOrDefault() as Label;
                if (label1 != null)
                {
                    label1.Text = "C ID";
                }

                // Generate new return number
                int SReturnNo = operations.GenerateSReturnNo();
                TxtSRNO.Value = SReturnNo.ToString();

                // Hide the update button if it exists
                Control[] updateButtons = this.Controls.Find("updtbtn", true);
                if (updateButtons.Length > 0)
                {
                    Button updtbtn = updateButtons[0] as Button;
                    if (updtbtn != null)
                    {
                        updtbtn.Visible = false;
                    }
                }

                // Show the save button
                if (pbxSave != null)
                {
                    pbxSave.Visible = true;
                }

                // Reset the data source tracker
                currentDataSource = DataSource.None;

                // Set focus based on the source of previous items
                if (wasCustomerBill)
                {
                    button2.Focus();
                }
                else if (wasBarcodeItems)
                {
                    TxtBarcode.Focus();
                }
                else
                {
                    // Default focus to button2 for new form
                    button2.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error clearing form: " + ex.Message);
            }
        }

        private void ultraPictureBox1_Click(object sender, EventArgs e)
        {
            ClearForm();
            // Ensure focus is set to button2
            button2.Focus();
        }

        private void ultraLabel1_Click(object sender, EventArgs e)
        {
            ClearForm();
            // Ensure focus is set to button2
            button2.Focus();
        }

        private void TxtInvoiceAmnt_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Parse the invoice amount
                decimal invoiceAmount = 0;
                if (decimal.TryParse(TxtInvoiceAmnt.Text, out invoiceAmount))
                {
                    // Update the subtotal and net amount if no items are in the grid
                    if (ultraGrid1.Rows.Count == 0)
                    {
                        TxtSubTotal.Text = invoiceAmount.ToString("0.00");
                        txtNetTotal.Text = invoiceAmount.ToString("0.00");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing invoice amount: {ex.Message}");
            }
        }

        private void barbtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if a customer is selected
                int customerId = customerTextBox.Tag != null ? Convert.ToInt32(customerTextBox.Tag) : 0;
                if (customerId <= 0)
                {
                    MessageBox.Show("Please select a customer first", "No Customer Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // If data source is Bill, show a message and don't proceed
                if (currentDataSource == DataSource.Bill)
                {
                    MessageBox.Show("You have already loaded items from bills. This data comes from saved transactions and cannot be mixed with direct item selection.",
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // If there are existing items from an invoice or different source, check if we should clear them
                if (currentDataSource != DataSource.None && currentDataSource != DataSource.Barcode && !string.IsNullOrEmpty(textBox1.Text) && textBox1.Text != "without bill")
                {
                    DialogResult result = MessageBox.Show(
                        "Adding items directly will replace your current items. Do you want to continue?",
                        "Confirm Change",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                    {
                        return;
                    }

                    // Clear the grid but preserve customer selection
                    ClearGridOnly();
                }

                // Set textBox1 to indicate items are being added without a bill
                textBox1.Value = "without bill";
                currentDataSource = DataSource.Barcode;

                // Open the item selection dialog with parameter indicating it's from Sales Return
                frmdialForItemMaster itemDialog = new frmdialForItemMaster("frmSalesReturn");
                itemDialog.Owner = this;

                // Set KeyPreview to handle ESC key at the form level
                itemDialog.KeyPreview = true;

                // Add KeyDown handler to catch ESC key
                itemDialog.KeyDown += (s, args) =>
                {
                    if (args.KeyCode == Keys.Escape)
                    {
                        // Close the item dialog when ESC is pressed
                        itemDialog.Close();
                        args.Handled = true; // Mark as handled to prevent bubbling
                    }
                };

                itemDialog.ShowDialog();

                // After dialog closes, ensure focus goes to the grid if items exist
                if (ultraGrid1.Rows.Count > 0)
                {
                    // Delay slightly to ensure data is fully loaded
                    System.Threading.Thread.Sleep(100);

                    // Use enhanced method to ensure Unit cell is editable
                    ForceUnitCellEditMode();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening item dialog: {ex.Message}");
            }
        }

        private void ultraLabel3_Click(object sender, EventArgs e)
        {
            try
            {
                // Show confirmation dialog for closing
                DialogResult result = MessageBox.Show("Are you sure you want to close?", "Confirm Close",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Find the parent TabPage and TabControl
                    if (this.Parent is TabPage tabPage && tabPage.Parent is TabControl tabControl)
                    {
                        // Remove the TabPage from the TabControl
                        tabControl.TabPages.Remove(tabPage);
                    }

                    // Close the form itself
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error closing form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Helper method to show dropdown for Reason cell
        private void ShowDropDownForReasonCell()
        {
            try
            {
                // Use SendKeys with a delay to ensure proper timing
                Application.DoEvents();
                System.Threading.Thread.Sleep(50);
                SendKeys.SendWait(" ");
                Application.DoEvents();
            }
            catch
            {
                // Silently handle errors in dropdown display
            }
        }

        private void ultraGrid1_DoubleClickCell(object sender, DoubleClickCellEventArgs e)
        {
            try
            {
                // If it's a Reason cell, force the dropdown to show
                if (e.Cell != null && e.Cell.Column.Key == "Reason")
                {
                    e.Cell.Activate();
                    ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.Edit;
                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);

                    // Send a space key to open the dropdown
                    this.BeginInvoke(new Action(() =>
                    {
                        SendKeys.SendWait(" ");

                        // After selecting from dropdown, handle Enter key press to continue with navigation
                        this.BeginInvoke(new Action(() =>
                        {
                            System.Threading.Thread.Sleep(500); // Give time for selection

                            // Simulate Enter key press for consistent navigation
                            ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                            HandleEnterNavigation();
                        }), null);
                    }), null);
                }
            }
            catch (Exception ex)
            {
                // Silently handle errors
            }
        }

        private void labelInvoiceAmount_Click(object sender, EventArgs e)
        {
            // Focus the TxtInvoiceAmnt textbox when the label is clicked
            TxtInvoiceAmnt.Focus();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Focus the customer selection button when the label is clicked
            button2.Focus();
        }

        private void ultraLabel4_Click(object sender, EventArgs e)
        {
            // Focus the barcode textbox when Ctrl+L label is clicked
            TxtBarcode.Focus();
        }

        private void updtbtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate payment method first
                if (!ValidatePaymentMethod())
                {
                    return;
                }

                // Ensure all selected rows have valid reasons before updating
                if (!ValidateReasons())
                {
                    return;
                }

                // Force commit any changes in the active cell before updating
                CommitActiveCell();

                // Call the dedicated update method instead of SaveSalesReturn
                UpdateSalesReturn();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating: " + ex.Message, "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper method to ensure active cell values are committed
        private void CommitActiveCell()
        {
            try
            {
                // If we have an active cell in edit mode, commit its value first
                if (ultraGrid1.ActiveCell != null && ultraGrid1.ActiveCell.IsInEditMode)
                {
                    // Force exit edit mode to commit the value
                    ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);

                    // If it was a reason cell, make sure to mark the row as selected
                    if (ultraGrid1.ActiveCell.Column.Key == "Reason" &&
                        ultraGrid1.ActiveCell.Value != null &&
                        ultraGrid1.ActiveCell.Value.ToString() != "Select Reason")
                    {
                        if (ultraGrid1.ActiveRow.Cells.Exists("Select"))
                        {
                            ultraGrid1.ActiveRow.Cells["Select"].Value = true;
                        }
                    }

                    // Give UI thread time to process
                    Application.DoEvents();
                }

                // Make sure all rows with reasons are properly selected
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("Reason") &&
                        row.Cells["Reason"].Value != null &&
                        row.Cells["Reason"].Value.ToString() != "Select Reason")
                    {
                        if (row.Cells.Exists("Select"))
                        {
                            row.Cells["Select"].Value = true;
                        }
                    }
                }

                // Update totals to reflect any selection changes
                UpdateTotalAmount();
            }
            catch
            {
            }
        }

        private void ultraGrid1_SelectAllClickCell(object sender, ClickCellEventArgs e)
        {
            try
            {
                // Remove the check for IsHeaderCell which doesn't exist
                // Instead check if the cell is in the column header
                if (e.Cell.Column.Key == "Select" && e.Cell.Row == null)
                {

                    // Commit all pending changes to the grid data
                    ultraGrid1.UpdateData();

                    // Use BeginInvoke to ensure the UI has fully processed the checkbox toggle
                    this.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            // Update row appearances if needed for active row only
                            // (No coloring needed as Infragistics handles active row highlighting)

                            // Now safely recalculate totals
                            UpdateTotalAmount();

                            // Refresh UI to ensure everything shows up
                            this.Refresh();
                        }
                        catch (Exception ex)
                        {
                        }
                    }));
                }
            }
            catch
            {
            }
        }


        // Helper method to force immediate update of totals
        private void ForceUpdateTotals(decimal amount)
        {
            try
            {

                // Format the amount to 2 decimal places
                string formattedAmount = amount.ToString("0.00");

                // Use BeginInvoke to ensure UI thread updates
                this.BeginInvoke(new Action(() =>
                {
                    // Update all three total displays
                    if (TxtSubTotal != null)
                        TxtSubTotal.Text = formattedAmount;

                    if (txtNetTotal != null)
                        txtNetTotal.Text = formattedAmount;

                    if (TxtInvoiceAmnt != null)
                        TxtInvoiceAmnt.Text = formattedAmount;

                    // Force immediate refresh of the controls
                    TxtSubTotal?.Refresh();
                    txtNetTotal?.Refresh();
                    TxtInvoiceAmnt?.Refresh();

                    // Force the entire form to refresh
                    this.Refresh();
                }));
            }
            catch
            {
            }
        }


        private void ultraGrid1_CellChange(object sender, CellEventArgs e)
        {
            if (e.Cell.Column.Key == "Select")
            {
                ultraGrid1.UpdateData();
                UpdateTotalAmount();
            }
        }

        // Add this helper method near the other helper methods
        private void FocusFirstUnitCell()
        {
            try
            {
                // Make sure there are rows in the grid
                if (ultraGrid1.Rows.Count > 0)
                {
                    // Reset editable columns to ensure they're truly editable
                    ResetGridEditableColumns();

                    // Force any pending operations to complete
                    Application.DoEvents();

                    // Set focus to the grid first
                    ultraGrid1.Focus();

                    // Get a reference to the Unit cell and ensure it's editable
                    UltraGridCell unitCell = ultraGrid1.Rows[0].Cells["Unit"];
                    unitCell.Activation = Activation.AllowEdit;

                    // Activate the cell (make it the active cell)
                    unitCell.Activate();
                    ultraGrid1.ActiveCell = unitCell;

                    // Short delay to allow the UI to update
                    System.Threading.Thread.Sleep(10);

                    // Force enter edit mode directly
                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);

                    // If edit mode failed, use a different approach
                    if (unitCell == ultraGrid1.ActiveCell)
                    {
                        // Try to click the cell to force edit mode
                        ultraGrid1_ClickCell(ultraGrid1, new ClickCellEventArgs(unitCell));

                        // Immediate second attempt to enter edit mode
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                    }

                    // Final force update of UI
                    Application.DoEvents();
                }
            }
            catch
            {
            }
        }

        // Only add the new ResetGridEditableColumns method and remove the duplicates
        // Add a method to fix the grid after data is loaded
        private void ResetGridEditableColumns()
        {
            try
            {
                // Explicitly reset all editable columns to ensure they can be edited
                if (ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                    ultraGrid1.DisplayLayout.Bands[0].Columns.Count > 0)
                {
                    // Unit column
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Unit"))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns["Unit"].CellActivation = Activation.AllowEdit;
                        ultraGrid1.DisplayLayout.Bands[0].Columns["Unit"].CellClickAction = CellClickAction.Edit;
                    }

                    // UnitPrice column
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("UnitPrice"))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns["UnitPrice"].CellActivation = Activation.AllowEdit;
                    }

                    // Qty column
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Qty"))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns["Qty"].CellActivation = Activation.AllowEdit;
                    }

                    // Packing column (hidden - no need to set activation)

                    // Cost column (hidden - no need to set activation)

                    // Reason column
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Reason"))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns["Reason"].CellActivation = Activation.AllowEdit;
                    }

                    // Also ensure row/grid settings allow editing
                    ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                    ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
                }
            }
            catch
            {
            }
        }

        // Add this method to initialize event handlers that aren't in InitializeUltraGrid
        private void AddAdditionalGridEvents()
        {
            try
            {
                // Add AfterRowActivate event handler to automatically edit Unit cell when a row is activated
                ultraGrid1.AfterRowActivate += new EventHandler(ultraGrid1_AfterRowActivate);

                // We'll use the ClickCell event instead of AfterHeaderCheckBoxClick since it's already implemented
                // and properly handles the header checkbox click
            }
            catch
            {
            }
        }

        // Add this event handler for row activation
        private void ultraGrid1_AfterRowActivate(object sender, EventArgs e)
        {
            try
            {
                // Get the active row
                UltraGridRow activeRow = ultraGrid1.ActiveRow;
                if (activeRow != null)
                {
                    // Get the Unit cell
                    UltraGridCell unitCell = activeRow.Cells["Unit"];
                    if (unitCell != null)
                    {
                        // Force column to be editable
                        unitCell.Column.CellActivation = Activation.AllowEdit;

                        // Force cell to be editable
                        unitCell.Activation = Activation.AllowEdit;

                        // Focus and select the cell
                        unitCell.Activate();
                        ultraGrid1.ActiveCell = unitCell;

                        // Small delay to allow UI updates
                        System.Threading.Thread.Sleep(10);

                        // Force edit mode
                        ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);

                        // Update UI and try again
                        Application.DoEvents();
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                    }
                }
            }
            catch
            {
            }
        }

        // Add a method specifically for forcing edit mode on the Unit cell after data is loaded from a dialog
        public void ForceUnitCellEditMode()
        {
            try
            {
                // Make sure there are rows in the grid
                if (ultraGrid1.Rows.Count > 0)
                {
                    // Reset editable columns to ensure they're truly editable
                    ResetGridEditableColumns();

                    // Force any grid-wide settings that might prevent editing
                    ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                    ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                    // Focus the grid first
                    ultraGrid1.Focus();
                    Application.DoEvents();

                    // Get the Unit cell from the first row
                    UltraGridCell unitCell = ultraGrid1.Rows[0].Cells["Unit"];

                    // Ensure it's editable at the column level
                    unitCell.Column.CellActivation = Activation.AllowEdit;

                    // Ensure it's editable at the cell level
                    unitCell.Activation = Activation.AllowEdit;

                    // Force selection of the cell
                    unitCell.Activate();
                    ultraGrid1.ActiveCell = unitCell;
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];

                    // Small delay for UI to update
                    System.Threading.Thread.Sleep(20);

                    // Force edit mode with a direct approach
                    ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);

                    // Update UI
                    Application.DoEvents();

                    // Secondary approach if first fails
                    if (unitCell == ultraGrid1.ActiveCell)
                    {
                        // Try one more time with a different cell click action
                        ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.Edit;
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                        Application.DoEvents();

                        // Reset to normal click action
                        ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                        // Last resort - simulate a click on the cell
                        ultraGrid1_ClickCell(ultraGrid1, new ClickCellEventArgs(unitCell));
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                        Application.DoEvents();
                    }
                }
            }
            catch
            {
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        // Add this validation method to check for missing required values
        private bool ValidateItemRow(UltraGridRow row, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Check for ItemId - critical for database operations
                if (!row.Cells.Exists("ItemId") || row.Cells["ItemId"].Value == null || row.Cells["ItemId"].Value == DBNull.Value)
                {
                    errorMessage = "Item ID is missing";
                    return false;
                }
                else
                {
                    // Try to parse to verify it's a valid integer
                    int itemId;
                    if (!int.TryParse(row.Cells["ItemId"].Value.ToString(), out itemId))
                    {
                        errorMessage = "Item ID is invalid";
                        return false;
                    }
                    // Allow ItemId = 0 for existing returns (repository will handle lookup)
                    if (itemId == 0)
                    {
                    }
                }

                // Check for ItemName - required for display
                if (!row.Cells.Exists("ItemName") || row.Cells["ItemName"].Value == null ||
                    row.Cells["ItemName"].Value == DBNull.Value || string.IsNullOrWhiteSpace(row.Cells["ItemName"].Value.ToString()))
                {
                    errorMessage = "Item Name is missing";
                    return false;
                }

                // Check for Unit - critical for database operations (be more lenient)
                if (!row.Cells.Exists("Unit") || row.Cells["Unit"].Value == null ||
                    row.Cells["Unit"].Value == DBNull.Value || string.IsNullOrWhiteSpace(row.Cells["Unit"].Value.ToString()))
                {
                    // Auto-fix: Set a default unit if missing
                    if (row.Cells.Exists("Unit"))
                    {
                        row.Cells["Unit"].Value = DEFAULT_UNIT_NAME; // Set default unit
                    }
                    else
                    {
                        errorMessage = "Unit is missing";
                        return false;
                    }
                }

                // Check for Quantity (be more lenient with parsing)
                if (!row.Cells.Exists("Qty") || row.Cells["Qty"].Value == null || row.Cells["Qty"].Value == DBNull.Value)
                {
                    errorMessage = "Quantity is missing";
                    return false;
                }
                else
                {
                    // Verify quantity is a valid number
                    decimal qty;
                    if (!decimal.TryParse(row.Cells["Qty"].Value.ToString(), out qty) || qty <= 0)
                    {
                        errorMessage = "Quantity must be a valid positive number";
                        return false;
                    }
                }

                // Check for UnitPrice (be more lenient with parsing)
                if (!row.Cells.Exists("UnitPrice") || row.Cells["UnitPrice"].Value == null || row.Cells["UnitPrice"].Value == DBNull.Value)
                {
                    errorMessage = "Unit Price is missing";
                    return false;
                }
                else
                {
                    // Verify unit price is a valid number
                    decimal unitPrice;
                    if (!decimal.TryParse(row.Cells["UnitPrice"].Value.ToString(), out unitPrice) || unitPrice < 0)
                    {
                        errorMessage = "Unit Price must be a valid number";
                        return false;
                    }
                }

                // Check for Reason (be more specific about what's valid)
                if (!row.Cells.Exists("Reason") || row.Cells["Reason"].Value == null ||
                    row.Cells["Reason"].Value == DBNull.Value || string.IsNullOrWhiteSpace(row.Cells["Reason"].Value.ToString()))
                {
                    errorMessage = "Reason is missing";
                    return false;
                }
                else
                {
                    string reason = row.Cells["Reason"].Value.ToString().Trim();
                    if (reason == "Select Reason" || reason == "" || reason.Length == 0)
                    {
                        errorMessage = "Please select a valid reason";
                        return false;
                    }
                }

                // Additional validation: Check if ReturnQty is valid (if present)
                if (row.Cells.Exists("ReturnQty") && row.Cells["ReturnQty"].Value != null && row.Cells["ReturnQty"].Value != DBNull.Value)
                {
                    decimal returnQty;
                    if (!decimal.TryParse(row.Cells["ReturnQty"].Value.ToString(), out returnQty) || returnQty < 0)
                    {
                        errorMessage = "Return Quantity must be a valid non-negative number";
                        return false;
                    }

                    // For partial returns, allow ReturnQty = 0 as valid (user can set it later)
                    // Only validate against available quantity if ReturnQty > 0
                    if (returnQty > 0)
                    {
                        // Get original and returned quantities for validation
                        decimal originalQty = 0;
                        decimal returnedQty = 0;

                        if (row.Cells.Exists("Qty") && row.Cells["Qty"].Value != null)
                            decimal.TryParse(row.Cells["Qty"].Value.ToString(), out originalQty);

                        if (row.Cells.Exists("ReturnedQty") && row.Cells["ReturnedQty"].Value != null)
                            decimal.TryParse(row.Cells["ReturnedQty"].Value.ToString(), out returnedQty);

                        decimal availableQty = originalQty - returnedQty;

                        if (returnQty > availableQty)
                        {
                            errorMessage = $"Return quantity ({returnQty}) cannot exceed available quantity ({availableQty})";
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Validation error: {ex.Message}";
                return false;
            }
        }

        // Helper method to get UnitId from Unit name using repository
        private int GetUnitId(string unitName)
        {
            try
            {
                // Use the repository method instead of direct database access
                return operations.GetUnitId(unitName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting UnitId from repository: {ex.Message}");
                return DEFAULT_UNIT_ID;
            }
        }

        private void UpdateSalesReturn()
        {
            try
            {
                if (!ShiftSessionGuard.CanDoTransaction(out string transactionError))
                {
                    MessageBox.Show(transactionError, "Shift Closing Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // First make sure any active cell's value is committed
                CommitActiveCell();

                // Check if we have a valid SR number to update
                if (string.IsNullOrEmpty(TxtSRNO.Text))
                {
                    MessageBox.Show("No Sales Return number found to update.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int returnNo;
                if (!int.TryParse(TxtSRNO.Text.Trim(), out returnNo))
                {
                    // Try to remove any formatting
                    string cleanedSRNo = TxtSRNO.Text.Trim().TrimStart('0');
                    if (!int.TryParse(cleanedSRNo, out returnNo))
                    {
                        MessageBox.Show("Invalid Sales Return number format.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Check if any items are selected for update
                bool hasSelectedItems = false;
                List<string> invalidItems = new List<string>();

                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.IsDataRow && !row.IsFilteredOut && row.Cells.Count > 0)
                    {
                        // Check if this row is selected via the Select column
                        bool isSelected = false;
                        if (row.Cells.Exists("Select"))
                        {
                            var selectVal = row.Cells["Select"].Value;
                            isSelected = selectVal != null && selectVal != DBNull.Value && Convert.ToBoolean(selectVal);
                        }

                        if (isSelected)
                        {
                            hasSelectedItems = true;
                            break; // Found at least one selected item, that's enough
                        }
                    }
                }

                if (!hasSelectedItems)
                {
                    MessageBox.Show("Please select at least one item to update by checking the Select box.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate payment method selection
                if (cmbPaymntMethod.SelectedIndex <= 0)
                {
                    MessageBox.Show("Please select a valid payment method before updating.",
                                   "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Replace with validation method
                if (!ValidatePaymentMethod())
                {
                    return;
                }

                // Set cursor to wait
                Cursor.Current = Cursors.WaitCursor;

                try
                {
                    // Get existing sales return data directly from the repository
                    SalesReturn existingSR = operations.GetBySalesReturnId(returnNo);

                    if (existingSR == null)
                    {
                        MessageBox.Show($"Sales Return #{returnNo} not found in the database.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                    // Create a temporary DataGridView to store only selected rows
                    DataGridView tempDgv = new DataGridView();
                    tempDgv.Columns.Add("SlNo", "SlNo");
                    tempDgv.Columns.Add("ItemId", "ItemId");
                    tempDgv.Columns.Add("ItemName", "ItemName");
                    tempDgv.Columns.Add("BarCode", "BarCode");
                    tempDgv.Columns.Add("Unit", "Unit");
                    tempDgv.Columns.Add("UnitId", "UnitId"); // Add UnitId column
                    tempDgv.Columns.Add("UnitPrice", "UnitPrice");
                    tempDgv.Columns.Add("Qty", "Qty");
                    tempDgv.Columns.Add("ReturnQty", "ReturnQty"); // New column for current return quantity
                    tempDgv.Columns.Add("ReturnedQty", "ReturnedQty"); // New column for previously returned quantity
                    tempDgv.Columns.Add("Packing", "Packing"); // Add missing Packing column
                    tempDgv.Columns.Add("Cost", "Cost"); // Add missing Cost column
                    tempDgv.Columns.Add("Amount", "Amount");
                    tempDgv.Columns.Add("Reason", "Reason");
                    tempDgv.Columns.Add("TaxPer", "TaxPer");
                    tempDgv.Columns.Add("TaxAmt", "TaxAmt");
                    tempDgv.Columns.Add("TaxType", "TaxType");
                    tempDgv.Columns.Add("Select", "Select");

                    // First, ensure every row in the original grid has a valid Unit value
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (row.Cells["Select"].Value != null && Convert.ToBoolean(row.Cells["Select"].Value))
                        {
                            if (row.Cells["Unit"].Value == null || row.Cells["Unit"].Value == DBNull.Value ||
                                string.IsNullOrWhiteSpace(row.Cells["Unit"].Value?.ToString()))
                            {
                                row.Cells["Unit"].Value = DEFAULT_UNIT_NAME; // Default to PCS if null or empty
                            }
                        }
                    }

                    // Copy only selected rows to the temporary grid
                    int slNo = 1;
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (row.Cells["Select"].Value != null && Convert.ToBoolean(row.Cells["Select"].Value))
                        {
                            // Double-check for NULL values in critical fields
                            if (row.Cells["Unit"].Value == null || row.Cells["Unit"].Value == DBNull.Value ||
                                string.IsNullOrWhiteSpace(row.Cells["Unit"].Value?.ToString()))
                            {
                                string itemName = row.Cells["ItemName"].Value?.ToString() ?? "Unknown item";
                                MessageBox.Show($"Missing Unit value for {itemName}. Please set a valid unit before updating.",
                                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                Cursor.Current = Cursors.Default;
                                return;
                            }

                            // Get the UnitId for this unit
                            string unitName = row.Cells["Unit"].Value.ToString().Trim();
                            int unitId = 0;

                            // Use the refactored GetUnitId method that uses stored procedures
                            unitId = GetUnitId(unitName);

                            int index = tempDgv.Rows.Add();
                            tempDgv.Rows[index].Cells["SlNo"].Value = slNo++;
                            tempDgv.Rows[index].Cells["ItemId"].Value = row.Cells["ItemId"].Value;
                            tempDgv.Rows[index].Cells["ItemName"].Value = row.Cells["ItemName"].Value;

                            // Handle potentially missing values with defaults
                            tempDgv.Rows[index].Cells["BarCode"].Value = row.Cells["Barcode"].Value ?? ""; // Fixed: use "Barcode" not "BarCode"
                            tempDgv.Rows[index].Cells["Unit"].Value = unitName;
                            tempDgv.Rows[index].Cells["UnitId"].Value = unitId; // Set the UnitId for the stored procedure

                            // Get decimal values with validation
                            decimal unitPrice = 0;
                            if (row.Cells["UnitPrice"].Value != null && row.Cells["UnitPrice"].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells["UnitPrice"].Value.ToString(), out unitPrice);
                            }
                            tempDgv.Rows[index].Cells["UnitPrice"].Value = unitPrice;

                            decimal qty = 0;
                            if (row.Cells["Qty"].Value != null && row.Cells["Qty"].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells["Qty"].Value.ToString(), out qty);
                            }
                            tempDgv.Rows[index].Cells["Qty"].Value = qty;

                            // Safe access to ReturnQty and ReturnedQty columns
                            decimal returnQty = 0;
                            if (row.Cells.Exists("ReturnQty") && row.Cells["ReturnQty"].Value != null && row.Cells["ReturnQty"].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells["ReturnQty"].Value.ToString(), out returnQty);
                            }
                            else
                            {
                                returnQty = qty; // Default to Qty if ReturnQty doesn't exist
                            }
                            tempDgv.Rows[index].Cells["ReturnQty"].Value = returnQty;

                            decimal returnedQty = 0;
                            if (row.Cells.Exists("ReturnedQty") && row.Cells["ReturnedQty"].Value != null && row.Cells["ReturnedQty"].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells["ReturnedQty"].Value.ToString(), out returnedQty);
                            }
                            tempDgv.Rows[index].Cells["ReturnedQty"].Value = returnedQty;

                            decimal amount = 0;
                            if (row.Cells["Amount"].Value != null && row.Cells["Amount"].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells["Amount"].Value.ToString(), out amount);
                            }
                            else
                            {
                                // Calculate amount if not provided
                                amount = unitPrice * qty;
                            }
                            tempDgv.Rows[index].Cells["Amount"].Value = amount;

                            // Safe access for Packing column
                            decimal packing = 1;
                            if (row.Cells.Exists("Packing") && row.Cells["Packing"].Value != null && row.Cells["Packing"].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells["Packing"].Value.ToString(), out packing);
                            }
                            tempDgv.Rows[index].Cells["Packing"].Value = packing;

                            // Safe access for Cost column
                            decimal cost = 0;
                            if (row.Cells.Exists("Cost") && row.Cells["Cost"].Value != null && row.Cells["Cost"].Value != DBNull.Value)
                            {
                                decimal.TryParse(row.Cells["Cost"].Value.ToString(), out cost);
                            }
                            tempDgv.Rows[index].Cells["Cost"].Value = cost;

                            tempDgv.Rows[index].Cells["Reason"].Value = row.Cells["Reason"].Value.ToString().Trim();
                            tempDgv.Rows[index].Cells["Select"].Value = true;
                        }
                    }

                    // Set up the SalesReturn object
                    // Safe conversion for DataBase fields
                    SReturn.BranchId = SessionContext.BranchId;
                    SReturn.CompanyId = SessionContext.CompanyId;
                    SReturn.FinYearId = existingSR.FinYearId > 0 ? existingSR.FinYearId : SessionContext.FinYearId;

                    // Set the ID of the existing record for update
                    SReturn.SReturnNo = returnNo;
                    SReturn.VoucherID = existingSR.VoucherID;

                    // Set customer information
                    if (customerTextBox != null)
                    {
                        SReturn.CustomerName = customerTextBox.Value?.ToString() ?? "";
                        SReturn.LedgerID = customerTextBox.Tag != null ? Convert.ToInt32(customerTextBox.Tag) : 0;
                    }
                    else
                    {
                        // Use existing values if customer textbox is not available
                        SReturn.CustomerName = existingSR.CustomerName;
                        SReturn.LedgerID = existingSR.LedgerID;
                    }

                    // Set payment method information
                    SReturn.Paymode = cmbPaymntMethod.Text;

                    // Safe conversion for PaymodeID
                    int paymodeId = 0;
                    if (cmbPaymntMethod.Value != null && cmbPaymntMethod.Value != DBNull.Value)
                    {
                        int.TryParse(cmbPaymntMethod.Value.ToString(), out paymodeId);
                    }
                    SReturn.PaymodeID = paymodeId;

                    SReturn.PaymodeLedgerID = existingSR.PaymodeLedgerID; // Preserve the existing value

                    // Set other fields
                    // Normalize "without bill" marker to empty invoice number
                    string invText = textBox1.Text != null ? textBox1.Text.Trim() : string.Empty;
                    if (!string.IsNullOrEmpty(invText) && invText.Equals("without bill", StringComparison.OrdinalIgnoreCase))
                    {
                        invText = string.Empty;
                    }
                    SReturn.InvoiceNo = invText;
                    SReturn.SpDisPer = 0;
                    SReturn.SpDsiAmt = 0;
                    SReturn.BillDiscountPer = 0;
                    SReturn.BillDiscountAmt = 0;
                    SReturn.TaxPer = 0;
                    SReturn.TaxAmt = 0;
                    SReturn.Frieght = 0;

                    // Safe conversion for GrandTotal
                    double grandTotal = 0;
                    if (!string.IsNullOrEmpty(txtNetTotal.Text) && double.TryParse(txtNetTotal.Text, out grandTotal))
                    {
                        SReturn.GrandTotal = grandTotal;
                    }
                    else
                    {
                        SReturn.GrandTotal = 0;
                    }

                    // Safe conversion for SubTotal
                    double subTotal = 0;
                    if (!string.IsNullOrEmpty(TxtSubTotal.Text) && double.TryParse(TxtSubTotal.Text, out subTotal))
                    {
                        SReturn.SubTotal = subTotal;
                    }
                    else
                    {
                        SReturn.SubTotal = 0;
                    }

                    SReturn.UserID = SessionContext.UserId;
                    SReturn.CessAmt = 0;
                    SReturn.CessPer = 0;
                    SReturn.BranchName = DataBase.Branch;
                    SReturn.CalAfterTax = 0;
                    SReturn.CurSymbol = "";
                    SReturn.SReturnDate = (DateTime)dtSReturnDate.Value;
                    SReturn.InvoiceDate = (DateTime)dtInvoiceDate.Value;
                    SReturn._Operation = "UPDATE"; // Explicitly set the operation to UPDATE


                    // Call the repository to update the sales return
                    string message = operations.UpdateSalesReturn(SReturn, tempDgv);
                    MessageBox.Show(message);

                    // Reset form for next entry if update was successful
                    if (message.Contains("success"))
                    {
                        SaveSalesReturnActivityLog("UPDATE", SReturn, message);
                        ClearForm();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating sales return: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // Reset cursor
                    Cursor.Current = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating sales return: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (ex.InnerException != null)
                {
                }
            }
            finally
            {
                // Reset cursor
                Cursor.Current = Cursors.Default;
            }
        }

        // Add this new event handler for ultraPictureBox2
        private void ultraPictureBox2_Click(object sender, EventArgs e)
        {
            DeleteSalesReturn();
        }

        // Add the DeleteSalesReturn method
        private void DeleteSalesReturn()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Check if we have a valid sales return number
                if (string.IsNullOrEmpty(TxtSRNO.Text))
                {
                    MessageBox.Show("Please load a sales return record first.", "No Record Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int sReturnNo;
                if (!int.TryParse(TxtSRNO.Text, out sReturnNo))
                {
                    MessageBox.Show("Invalid sales return number.", "Invalid Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the VoucherID is available
                int voucherId = 0;
                if (SReturn != null && SReturn.VoucherID > 0)
                {
                    voucherId = (int)SReturn.VoucherID;
                }
                else
                {
                    // Try to load the sales return data to get the VoucherID
                    try
                    {
                        SalesReturn existingReturn = operations.GetBySalesReturnId(sReturnNo);
                        if (existingReturn != null && existingReturn.VoucherID > 0)
                        {
                            voucherId = (int)existingReturn.VoucherID;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Continue without VoucherID if we can't retrieve it
                    }
                }

                // Confirm deletion with the user
                DialogResult result = MessageBox.Show(
                    $"Are you sure you want to delete Sales Return #{sReturnNo}?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    return;
                }

                // Display a "Processing..." message

                // Call the repository to delete the record
                string message = operations.DeleteSalesReturn(sReturnNo, voucherId);

                if (message.Contains("success"))
                {
                    SaveSalesReturnActivityLog("DELETE", new SalesReturn
                    {
                        SReturnNo = sReturnNo,
                        CustomerName = customerTextBox != null && customerTextBox.Value != null ? customerTextBox.Value.ToString() : string.Empty,
                        GrandTotal = Convert.ToDouble(decimal.TryParse(txtNetTotal != null ? txtNetTotal.Text : "0", out decimal _srNetTotal) ? _srNetTotal : 0m)
                    }, "Sales Return Deleted");

                    MessageBox.Show("Sales return deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear the form
                    ClearForm();

                    // Force a refresh of any open frmCommonDialog
                    Form[] forms = Application.OpenForms.Cast<Form>().ToArray();
                    foreach (Form form in forms)
                    {
                        if (form is frmCommonDialog commonDialog)
                        {
                            // Force the dialog to clear its cache and fully reload from the database

                            // First force the dialog to close its connection if open
                            try
                            {
                                // Access the repository through reflection to ensure connection is closed
                                var repositoryField = typeof(DialogBox.frmCommonDialog).GetField("srRepository",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                                if (repositoryField != null)
                                {
                                    var repository = repositoryField.GetValue(commonDialog);
                                    if (repository != null)
                                    {
                                        var dataConnectionProperty = repository.GetType().BaseType.GetProperty("DataConnection");
                                        if (dataConnectionProperty != null)
                                        {
                                            var connection = dataConnectionProperty.GetValue(repository);
                                            var stateProperty = connection.GetType().GetProperty("State");

                                            if (stateProperty != null)
                                            {
                                                var state = stateProperty.GetValue(connection);
                                                if (state.ToString() == "Open")
                                                {
                                                    var closeMethod = connection.GetType().GetMethod("Close");
                                                    if (closeMethod != null)
                                                    {
                                                        closeMethod.Invoke(connection, null);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // Ignore errors here, we're just trying to be thorough
                            }

                            // Then call refresh
                            commonDialog.RefreshData();

                            // Force the UI to update
                            commonDialog.Refresh();
                            Application.DoEvents();

                        }
                    }
                }
                else
                {
                    // Show more detailed error message
                    MessageBox.Show(message, "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting sales return: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (ex.InnerException != null)
                {
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        // In constructor or form load, add the event handler hook for ultraPictureBox2
        // Add this to the constructor after the other ultraPictureBox event handler registrations
        private void AddEventHandlers()
        {
            // ... existing code ...
            // Add the ultraPictureBox2 click handler
            if (ultraPictureBox2 != null)
            {
                ultraPictureBox2.Click += new EventHandler(ultraPictureBox2_Click);
            }
        }

        private bool ValidatePaymentMethod(bool showErrorMessage = true)
        {
            // Check if payment method is selected and is valid (not the placeholder "Select Payment")
            if (cmbPaymntMethod.SelectedIndex <= 0)
            {
                if (showErrorMessage)
                {
                    MessageBox.Show("Please select a payment method before saving.", "Payment Method Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }

            // Get the selected payment method name
            string selectedPaymentMethod = cmbPaymntMethod.Text.ToLower();

            // Remove the restriction to only allow credit or debit cards
            // All payment methods are now valid as long as one is selected
            return true;
        }

        // Add a new validation method for checking reasons in selected rows
        private bool ValidateReasons(bool showErrorMessage = true)
        {
            try
            {
                // First make sure any active cell's value is committed
                CommitActiveCell();

                // Check if any row is selected
                bool anyRowSelected = false;
                bool allReasonsValid = true;

                // Track rows with missing reasons for highlighting
                List<UltraGridRow> rowsWithMissingReasons = new List<UltraGridRow>();

                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells["Select"].Value != null && Convert.ToBoolean(row.Cells["Select"].Value))
                    {
                        anyRowSelected = true;

                        // Check if this row has a valid reason
                        if (row.Cells["Reason"].Value == null ||
                            string.IsNullOrWhiteSpace(row.Cells["Reason"].Value.ToString()) ||
                            row.Cells["Reason"].Value.ToString() == "Select Reason")
                        {
                            allReasonsValid = false;
                            rowsWithMissingReasons.Add(row);
                        }
                    }
                }

                // If no rows are selected, we don't need to check reasons
                if (!anyRowSelected)
                {
                    if (showErrorMessage)
                    {
                        MessageBox.Show("Please select at least one item to return.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return false;
                }

                // If there are rows with missing reasons, highlight them and show an error
                if (!allReasonsValid)
                {
                    if (showErrorMessage)
                    {
                        MessageBox.Show("Please select a reason for all checked items. Items without a reason cannot be processed.", "Missing Reason", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // Highlight rows with missing reasons with a different color
                        foreach (UltraGridRow row in rowsWithMissingReasons)
                        {
                            // Temporarily mark the row with a distinctive color
                            row.Appearance.BackColor = System.Drawing.Color.MistyRose;

                            // If this is the first invalid row, activate its Reason cell for editing
                            if (row == rowsWithMissingReasons[0])
                            {
                                row.Cells["Reason"].Activate();
                                ultraGrid1.ActiveCell = row.Cells["Reason"];
                                ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);

                                // Show dropdown for easy selection
                                ShowDropDownForReasonCell();
                            }
                        }

                        // Schedule to reset highlight after a short delay (5 seconds)
                        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                        timer.Interval = 5000;
                        timer.Tick += (sender, e) =>
                        {
                            // Reset highlight colors for all rows
                            foreach (UltraGridRow row in rowsWithMissingReasons)
                            {
                                if (row.Cells["Select"].Value != null && Convert.ToBoolean(row.Cells["Select"].Value))
                                {
                                    row.Appearance.BackColor = System.Drawing.Color.LightBlue;
                                }
                                else
                                {
                                    row.Appearance.BackColor = System.Drawing.SystemColors.Window;
                                }
                            }
                            timer.Stop();
                            timer.Dispose();
                        };
                        timer.Start();
                    }
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Credit Note functionality
        public class CreditNote
        {
            public int CreditNoteNo { get; set; }
            public DateTime CreditNoteDate { get; set; }
            public string CustomerName { get; set; }
            public string InvoiceNo { get; set; }
            public DateTime InvoiceDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string PaymentMethod { get; set; }
        }

        private CreditNote GenerateCreditNote(SalesReturn salesReturn)
        {
            // Generate a credit note based on the sales return
            CreditNote creditNote = new CreditNote
            {
                CreditNoteNo = salesReturn.SReturnNo, // Use the same number as the sales return for simplicity
                CreditNoteDate = DateTime.Now,
                CustomerName = salesReturn.CustomerName,
                InvoiceNo = salesReturn.InvoiceNo,
                InvoiceDate = salesReturn.InvoiceDate,
                TotalAmount = (decimal)salesReturn.GrandTotal,
                PaymentMethod = salesReturn.Paymode
            };

            return creditNote;
        }

        private void DisplayCreditNote(CreditNote creditNote)
        {
            bool applyCreditNoteOpened = false;

            // Create a form to display the credit note
            Form creditNoteForm = new Form();
            creditNoteForm.Text = "Credit Note";
            creditNoteForm.Size = new Size(500, 600);
            creditNoteForm.StartPosition = FormStartPosition.CenterScreen;
            creditNoteForm.MaximizeBox = false;
            creditNoteForm.MinimizeBox = false;
            creditNoteForm.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Create a panel for the content
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.White;
            panel.AutoScroll = true;

            // Add credit note details
            Label titleLabel = new Label();
            titleLabel.Text = "CREDIT NOTE";
            titleLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Size = new Size(460, 40);
            titleLabel.Location = new Point(20, 20);

            Label creditNoteNoLabel = new Label();
            creditNoteNoLabel.Text = $"Credit Note #: {creditNote.CreditNoteNo}";
            creditNoteNoLabel.Font = new Font("Arial", 10);
            creditNoteNoLabel.Size = new Size(300, 20);
            creditNoteNoLabel.Location = new Point(20, 70);

            Label dateLabel = new Label();
            dateLabel.Text = $"Date: {creditNote.CreditNoteDate.ToString("dd/MM/yyyy")}";
            dateLabel.Font = new Font("Arial", 10);
            dateLabel.Size = new Size(300, 20);
            dateLabel.Location = new Point(20, 90);

            Label customerLabel = new Label();
            customerLabel.Text = $"Customer: {creditNote.CustomerName}";
            customerLabel.Font = new Font("Arial", 10);
            customerLabel.Size = new Size(300, 20);
            customerLabel.Location = new Point(20, 120);

            Label invoiceLabel = new Label();
            invoiceLabel.Text = $"Original Invoice #: {creditNote.InvoiceNo}";
            invoiceLabel.Font = new Font("Arial", 10);
            invoiceLabel.Size = new Size(300, 20);
            invoiceLabel.Location = new Point(20, 140);

            Label invoiceDateLabel = new Label();
            invoiceDateLabel.Text = $"Invoice Date: {creditNote.InvoiceDate.ToString("dd/MM/yyyy")}";
            invoiceDateLabel.Font = new Font("Arial", 10);
            invoiceDateLabel.Size = new Size(300, 20);
            invoiceDateLabel.Location = new Point(20, 160);

            Label amountLabel = new Label();
            amountLabel.Text = $"Total Amount: {creditNote.TotalAmount.ToString("0.00")}";
            amountLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            amountLabel.Size = new Size(300, 25);
            amountLabel.Location = new Point(20, 190);

            Label paymentMethodLabel = new Label();
            paymentMethodLabel.Text = $"Payment Method: {creditNote.PaymentMethod}";
            paymentMethodLabel.Font = new Font("Arial", 10);
            paymentMethodLabel.Size = new Size(300, 20);
            paymentMethodLabel.Location = new Point(20, 220);

            // Create a panel for returned items
            Panel itemsPanel = new Panel();
            itemsPanel.Location = new Point(20, 250);
            itemsPanel.Size = new Size(440, 200);
            itemsPanel.AutoScroll = true;
            itemsPanel.BorderStyle = BorderStyle.FixedSingle;

            // Get items from UltraGrid and add to panel
            int yOffset = 10;

            // Add header
            Label itemHeaderLabel = new Label();
            itemHeaderLabel.Text = "Returned Items:";
            itemHeaderLabel.Font = new Font("Arial", 10, FontStyle.Bold);
            itemHeaderLabel.Size = new Size(400, 20);
            itemHeaderLabel.Location = new Point(10, yOffset);
            itemsPanel.Controls.Add(itemHeaderLabel);
            yOffset += 30;

            // Add column headers
            Label itemNameHeader = new Label();
            itemNameHeader.Text = "Item";
            itemNameHeader.Font = new Font("Arial", 9, FontStyle.Bold);
            itemNameHeader.Size = new Size(200, 20);
            itemNameHeader.Location = new Point(10, yOffset);
            itemsPanel.Controls.Add(itemNameHeader);

            Label qtyHeader = new Label();
            qtyHeader.Text = "Qty";
            qtyHeader.Font = new Font("Arial", 9, FontStyle.Bold);
            qtyHeader.Size = new Size(50, 20);
            qtyHeader.Location = new Point(220, yOffset);
            itemsPanel.Controls.Add(qtyHeader);

            Label priceHeader = new Label();
            priceHeader.Text = "Price";
            priceHeader.Font = new Font("Arial", 9, FontStyle.Bold);
            priceHeader.Size = new Size(80, 20);
            priceHeader.Location = new Point(270, yOffset);
            itemsPanel.Controls.Add(priceHeader);

            Label amountHeader = new Label();
            amountHeader.Text = "Amount";
            amountHeader.Font = new Font("Arial", 9, FontStyle.Bold);
            amountHeader.Size = new Size(80, 20);
            amountHeader.Location = new Point(350, yOffset);
            itemsPanel.Controls.Add(amountHeader);

            yOffset += 25;

            // Add returned items
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells["Select"].Value != null && Convert.ToBoolean(row.Cells["Select"].Value))
                {
                    Label itemNameLabel = new Label();
                    itemNameLabel.Text = row.Cells["ItemName"].Value?.ToString() ?? "";
                    itemNameLabel.Font = new Font("Arial", 9);
                    itemNameLabel.Size = new Size(200, 20);
                    itemNameLabel.Location = new Point(10, yOffset);
                    itemsPanel.Controls.Add(itemNameLabel);

                    Label qtyLabel = new Label();
                    qtyLabel.Text = row.Cells.Exists("ReturnQty") && row.Cells["ReturnQty"].Value != null ?
                        row.Cells["ReturnQty"].Value.ToString() : (row.Cells["Qty"].Value?.ToString() ?? "");
                    qtyLabel.Font = new Font("Arial", 9);
                    qtyLabel.Size = new Size(50, 20);
                    qtyLabel.Location = new Point(220, yOffset);
                    itemsPanel.Controls.Add(qtyLabel);

                    Label priceLabel = new Label();
                    priceLabel.Text = row.Cells["UnitPrice"].Value != null ?
                        Convert.ToDecimal(row.Cells["UnitPrice"].Value).ToString("0.00") : "0.00";
                    priceLabel.Font = new Font("Arial", 9);
                    priceLabel.Size = new Size(80, 20);
                    priceLabel.Location = new Point(270, yOffset);
                    itemsPanel.Controls.Add(priceLabel);

                    Label itemAmountLabel = new Label();
                    itemAmountLabel.Text = row.Cells["Amount"].Value != null ?
                        Convert.ToDecimal(row.Cells["Amount"].Value).ToString("0.00") : "0.00";
                    itemAmountLabel.Font = new Font("Arial", 9);
                    itemAmountLabel.Size = new Size(80, 20);
                    itemAmountLabel.Location = new Point(350, yOffset);
                    itemsPanel.Controls.Add(itemAmountLabel);

                    yOffset += 25;
                }
            }

            // Add note text
            Label noteLabel = new Label();
            noteLabel.Text = "This credit note is issued for the returned items. This is not a tax invoice.";
            noteLabel.Font = new Font("Arial", 9, FontStyle.Italic);
            noteLabel.Size = new Size(400, 40);
            noteLabel.Location = new Point(20, 460);

            // Add print, view receipt, and close buttons
            Button printButton = new Button();
            printButton.Text = "Print";
            printButton.Size = new Size(100, 30);
            printButton.Location = new Point(50, 510);
            printButton.Click += (sender, e) => PrintCreditNote(creditNote);

            Button viewReceiptButton = new Button();
            viewReceiptButton.Text = "Apply Credit Note";
            viewReceiptButton.Size = new Size(120, 30);
            viewReceiptButton.Location = new Point(170, 510);
            viewReceiptButton.Click += (sender, e) =>
            {
                applyCreditNoteOpened = true;
                OpenReceiptForm(creditNote, creditNoteForm);
            };

            Button closeButton = new Button();
            closeButton.Text = "Close";
            closeButton.Size = new Size(100, 30);
            closeButton.Location = new Point(310, 510);
            closeButton.Click += (sender, e) => creditNoteForm.Close();

            // Add controls to the panel
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(creditNoteNoLabel);
            panel.Controls.Add(dateLabel);
            panel.Controls.Add(customerLabel);
            panel.Controls.Add(invoiceLabel);
            panel.Controls.Add(invoiceDateLabel);
            panel.Controls.Add(amountLabel);
            panel.Controls.Add(paymentMethodLabel);
            panel.Controls.Add(itemsPanel);
            panel.Controls.Add(noteLabel);
            panel.Controls.Add(printButton);
            panel.Controls.Add(viewReceiptButton);
            panel.Controls.Add(closeButton);

            // Add panel to the form
            creditNoteForm.Controls.Add(panel);

            // Show the form
            creditNoteForm.ShowDialog();

            if (!applyCreditNoteOpened)
            {
                EnsureCreditNoteMasterExists(creditNote);
            }
        }

        private void EnsureCreditNoteMasterExists(CreditNote creditNote)
        {
            try
            {
                if (creditNote == null || SReturn == null || SReturn.SReturnNo <= 0)
                {
                    return;
                }

                var creditNoteRepo = new Repository.Accounts.CreditNoteRepository();
                DataTable existingCreditNote = creditNoteRepo.GetCreditNoteBySReturnNo(
                    SReturn.SReturnNo,
                    SessionContext.BranchId,
                    SessionContext.FinYearId);

                if (existingCreditNote != null && existingCreditNote.Rows.Count > 0)
                {
                    return;
                }

                var master = new ModelClass.Accounts.CreditNoteMaster
                {
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    VoucherId = Convert.ToInt32(SReturn.VoucherID),
                    VoucherDate = DateTime.Now,
                    CustomerLedgerId = Convert.ToInt32(SReturn.LedgerID),
                    CustomerName = SReturn.CustomerName,
                    SReturnNo = SReturn.SReturnNo,
                    InvoiceNo = SReturn.InvoiceNo ?? string.Empty,
                    CreditAmount = Convert.ToDouble(creditNote.TotalAmount),
                    PaymentMethodLedgerId = 1,
                    Narration = "Auto-generated store credit from Sales Return",
                    UserId = SessionContext.UserId
                };

                creditNoteRepo.SaveCreditNote(
                    master,
                    new List<ModelClass.Accounts.CreditNoteDetails>(),
                    skipVoucherCreation: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring credit note master exists: {ex.Message}");
            }
        }

        private void PrintCreditNote(CreditNote creditNote)
        {
            try
            {
                // This is a simplified print function - in a real application, 
                // you would implement proper printing logic
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Printing credit note...", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Actual printing code would go here
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error printing credit note: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenReceiptForm(CreditNote creditNote, Form creditNoteForm)
        {
            try
            {
                // Get the customer's LedgerID from the current sales return or customer control
                int customerLedgerId = SReturn != null && SReturn.LedgerID > 0 ? Convert.ToInt32(SReturn.LedgerID) : 0;
                if (customerLedgerId <= 0 && customerTextBox != null && customerTextBox.Tag != null)
                {
                    int.TryParse(customerTextBox.Tag.ToString(), out customerLedgerId);
                }

                string customerName = creditNote.CustomerName;
                decimal returnedAmount = (decimal)creditNote.TotalAmount;
                int sReturnNo = SReturn != null && SReturn.SReturnNo > 0 ? SReturn.SReturnNo : creditNote.CreditNoteNo;

                // Create and configure the Credit Note form instead of Receipt form
                var creditNoteFormInstance = new PosBranch_Win.Accounts.FrmCreditNote(
                    sReturnNo,
                    customerLedgerId,
                    customerName,
                    creditNote.InvoiceNo,
                    (double)returnedAmount
                );

                // Close the credit note form
                creditNoteForm.Close();

                // Open the credit note form in a new tab
                OpenReceiptInTab(creditNoteFormInstance, $"Credit Note - {customerName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening receipt form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenReceiptInTab(Form form, string tabName)
        {
            try
            {
                // Find the Home form (parent form with tab control)
                var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                if (homeForm != null)
                {
                    // Use reflection to call the OpenFormInTab method
                    var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (openFormInTabMethod != null)
                    {
                        // Call the OpenFormInTab method
                        openFormInTabMethod.Invoke(homeForm, new object[] { form, tabName });
                        return;
                    }
                }

                // Fallback: show as regular form
                form.Show();
                form.BringToFront();
            }
            catch (Exception ex)
            {
                // Fallback: show as regular form
                form.Show();
                form.BringToFront();
                System.Diagnostics.Debug.WriteLine($"Error opening in tab, showing as regular form: {ex.Message}");
            }
        }

        // Modify the SaveSalesReturn method to also generate and show a credit note
        private void SaveSalesReturnWithCreditNote()
        {
            try
            {
                if (!ShiftSessionGuard.CanDoTransaction(out string transactionError))
                {
                    MessageBox.Show(transactionError, "Shift Closing Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // First make sure any active cell's value is committed
                CommitActiveCell();

                if (ultraGrid1.Rows.Count == 0)
                {
                    MessageBox.Show("No items to save.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Skip the detailed validation - just check basic requirements

                // Validate that all selected rows have a reason
                if (!ValidateReasons())
                {
                    return;
                }

                // Create a temporary DataGridView to store only selected rows
                DataGridView tempDgv = new DataGridView();
                tempDgv.Columns.Add("SlNo", "SlNo");
                tempDgv.Columns.Add("ItemId", "ItemId");
                tempDgv.Columns.Add("ItemName", "ItemName");
                tempDgv.Columns.Add("BarCode", "BarCode");
                tempDgv.Columns.Add("Unit", "Unit");
                tempDgv.Columns.Add("UnitPrice", "UnitPrice");
                tempDgv.Columns.Add("Qty", "Qty");
                tempDgv.Columns.Add("ReturnQty", "ReturnQty"); // New column for current return quantity
                tempDgv.Columns.Add("ReturnedQty", "ReturnedQty"); // New column for previously returned quantity
                tempDgv.Columns.Add("Packing", "Packing");
                tempDgv.Columns.Add("Cost", "Cost");
                tempDgv.Columns.Add("Amount", "Amount");
                tempDgv.Columns.Add("Reason", "Reason");
                tempDgv.Columns.Add("TaxPer", "TaxPer");
                tempDgv.Columns.Add("TaxAmt", "TaxAmt");
                tempDgv.Columns.Add("TaxType", "TaxType");

                // Copy only selected rows to the temporary grid
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells["Select"].Value != null && Convert.ToBoolean(row.Cells["Select"].Value))
                    {
                        int index = tempDgv.Rows.Add();
                        tempDgv.Rows[index].Cells["SlNo"].Value = row.Cells["SlNo"].Value;
                        tempDgv.Rows[index].Cells["ItemId"].Value = row.Cells["ItemId"].Value;
                        tempDgv.Rows[index].Cells["ItemName"].Value = row.Cells["ItemName"].Value;
                        tempDgv.Rows[index].Cells["BarCode"].Value = row.Cells["Barcode"].Value; // Fixed: use "Barcode" not "BarCode"
                        tempDgv.Rows[index].Cells["Unit"].Value = row.Cells["Unit"].Value;
                        tempDgv.Rows[index].Cells["UnitPrice"].Value = row.Cells["UnitPrice"].Value;
                        tempDgv.Rows[index].Cells["Qty"].Value = row.Cells["Qty"].Value;

                        // Safe access to ReturnQty and ReturnedQty columns
                        if (row.Cells.Exists("ReturnQty"))
                        {
                            tempDgv.Rows[index].Cells["ReturnQty"].Value = row.Cells["ReturnQty"].Value ?? row.Cells["Qty"].Value;
                        }
                        else
                        {
                            tempDgv.Rows[index].Cells["ReturnQty"].Value = row.Cells["Qty"].Value; // Default to Qty
                        }

                        if (row.Cells.Exists("ReturnedQty"))
                        {
                            tempDgv.Rows[index].Cells["ReturnedQty"].Value = row.Cells["ReturnedQty"].Value ?? 0;
                        }
                        else
                        {
                            tempDgv.Rows[index].Cells["ReturnedQty"].Value = 0; // Default to 0
                        }

                        // Safe access for Packing column
                        decimal packing = 1;
                        if (row.Cells.Exists("Packing") && row.Cells["Packing"].Value != null && row.Cells["Packing"].Value != DBNull.Value)
                        {
                            decimal.TryParse(row.Cells["Packing"].Value.ToString(), out packing);
                        }
                        tempDgv.Rows[index].Cells["Packing"].Value = packing; // Default to 1 if null

                        // Safe access for Cost column
                        decimal cost = 0;
                        if (row.Cells.Exists("Cost") && row.Cells["Cost"].Value != null && row.Cells["Cost"].Value != DBNull.Value)
                        {
                            decimal.TryParse(row.Cells["Cost"].Value.ToString(), out cost);
                        }
                        tempDgv.Rows[index].Cells["Cost"].Value = cost; // Default to 0 if null
                        tempDgv.Rows[index].Cells["Amount"].Value = row.Cells["Amount"].Value;
                        tempDgv.Rows[index].Cells["Reason"].Value = row.Cells["Reason"].Value;

                        // Safe access for tax-related columns
                        if (row.Cells.Exists("TaxPer") && row.Cells["TaxPer"].Value != null)
                        {
                            tempDgv.Rows[index].Cells["TaxPer"].Value = row.Cells["TaxPer"].Value;
                        }
                        else
                        {
                            tempDgv.Rows[index].Cells["TaxPer"].Value = 0;
                        }

                        if (row.Cells.Exists("TaxAmt") && row.Cells["TaxAmt"].Value != null)
                        {
                            tempDgv.Rows[index].Cells["TaxAmt"].Value = row.Cells["TaxAmt"].Value;
                        }
                        else
                        {
                            tempDgv.Rows[index].Cells["TaxAmt"].Value = 0;
                        }

                        if (row.Cells.Exists("TaxType") && row.Cells["TaxType"].Value != null)
                        {
                            tempDgv.Rows[index].Cells["TaxType"].Value = row.Cells["TaxType"].Value;
                        }
                        else
                        {
                            tempDgv.Rows[index].Cells["TaxType"].Value = "excl";
                        }
                    }
                }

                if (tempDgv.Rows.Count == 0)
                {
                    MessageBox.Show("Please select at least one item to return.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Determine if this is an update or new record
                bool isUpdate = false;
                int existingSReturnNo = 0;
                if (!string.IsNullOrEmpty(TxtSRNO.Text) && int.TryParse(TxtSRNO.Text, out existingSReturnNo) && existingSReturnNo > 0)
                {
                    // Check if the update button is visible, which indicates we're in update mode
                    Control[] updateButtons = this.Controls.Find("updtbtn", true);
                    if (updateButtons.Length > 0 && updateButtons[0] is Button updateBtn && updateBtn.Visible)
                    {
                        isUpdate = true;
                    }
                }

                // Set up the SalesReturn object with safe conversions
                SReturn.BranchId = SessionContext.BranchId;
                SReturn.CompanyId = SessionContext.CompanyId;

                // Set customer information
                if (customerTextBox != null)
                {
                    SReturn.CustomerName = customerTextBox.Value?.ToString() ?? "";
                    SReturn.LedgerID = customerTextBox.Tag != null ? Convert.ToInt32(customerTextBox.Tag) : 0;
                }
                else
                {
                    SReturn.CustomerName = "DEFAULT CUSTOMER";
                    SReturn.LedgerID = 0;
                }

                // Validate payment method selection
                if (!ValidatePaymentMethod())
                {
                    return;
                }

                // Log selected payment method
                Infragistics.Win.ValueListItem selectedPaymentItem = cmbPaymntMethod.SelectedItem as Infragistics.Win.ValueListItem;
                if (selectedPaymentItem != null && selectedPaymentItem.DataValue is DataRowView)
                {
                    DataRowView selectedPaymentRow = selectedPaymentItem.DataValue as DataRowView;
                }

                // Set payment method information
                SReturn.Paymode = cmbPaymntMethod.Text;
                SReturn.PaymodeID = Convert.ToInt32(cmbPaymntMethod.Value ?? 0);

                // Set SReturnNo - 0 for new records, existing id for updates
                SReturn.SReturnNo = isUpdate ? existingSReturnNo : 0;

                // Set other fields
                // Normalize "without bill" marker to empty invoice number
                string invText2 = textBox1.Text != null ? textBox1.Text.Trim() : string.Empty;
                if (!string.IsNullOrEmpty(invText2) && invText2.Equals("without bill", StringComparison.OrdinalIgnoreCase))
                {
                    invText2 = string.Empty;
                }
                SReturn.InvoiceNo = invText2;
                SReturn.SpDisPer = 0;
                SReturn.SpDsiAmt = 0;
                SReturn.BillDiscountPer = 0;
                SReturn.BillDiscountAmt = 0;
                // Calculate and set tax values
                SReturn.TaxPer = CalculateWeightedAverageTaxPercentage();
                SReturn.TaxAmt = CalculateTotalTaxAmount();
                SReturn.Frieght = 0;
                SReturn.GrandTotal = Convert.ToDouble(txtNetTotal.Text);
                SReturn.SubTotal = Convert.ToDouble(TxtSubTotal.Text);
                SReturn.UserID = SessionContext.UserId;
                SReturn.UserName = SessionContext.UserName;
                SReturn.BranchName = SessionContext.BranchName;
                SReturn.CalAfterTax = 0;
                SReturn.CurSymbol = "";
                SReturn.SReturnDate = (DateTime)dtSReturnDate.Value;
                SReturn.InvoiceDate = (DateTime)dtInvoiceDate.Value;

                // Set operation based on whether this is an update or new record
                SReturn._Operation = isUpdate ? "UPDATE" : "CREATE";

                // Call the appropriate method based on operation type
                string message;
                if (isUpdate)
                {
                    message = operations.UpdateSalesReturn(SReturn, tempDgv);
                }
                else
                {
                    message = operations.saveSR(SReturn, null, tempDgv);
                }

                MessageBox.Show(message);

                // Show credit note if the operation was successful
                if (message.Contains("Successfully") || message.Contains("success"))
                {
                    // Get the latest SalesReturn data with the updated SReturnNo
                    if (!isUpdate)
                    {
                        // For a new record, get the SReturnNo from the database
                        try
                        {
                            // Extract SReturnNo from success message
                            string[] parts = message.Split('#');
                            if (parts.Length > 1)
                            {
                                string numberPart = parts[1].Split(' ')[0]; // Get just the number
                                int sReturnNo;
                                if (int.TryParse(numberPart, out sReturnNo))
                                {
                                    SalesReturn savedReturn = operations.GetBySalesReturnId(sReturnNo);
                                    if (savedReturn != null && savedReturn.SReturnNo > 0)
                                    {
                                        SReturn = savedReturn;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    SaveSalesReturnActivityLog(isUpdate ? "UPDATE" : "SAVE", SReturn, message);

                    // Generate and display credit note
                    CreditNote creditNote = GenerateCreditNote(SReturn);
                    DisplayCreditNote(creditNote);

                    // Clear the form for next entry
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving sales return: " + ex.Message);
            }
        }

        private void cmbPaymntMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Handle payment method selection change
            Infragistics.Win.ValueListItem selectedItem = cmbPaymntMethod.SelectedItem as Infragistics.Win.ValueListItem;
            if (selectedItem != null && selectedItem.DataValue is DataRowView)
            {
                DataRowView selectedRow = selectedItem.DataValue as DataRowView;
                // You can add payment method-specific logic here if needed
            }
        }

        // Add BeforeExitEditMode event handler to ensure changes are committed
        private void ultraGrid1_BeforeExitEditMode(object sender, Infragistics.Win.UltraWinGrid.BeforeExitEditModeEventArgs e)
        {
            try
            {
                // Commit any pending changes
                ultraGrid1.UpdateData();

                // If the active cell is a checkbox, update totals
                if (ultraGrid1.ActiveCell != null && ultraGrid1.ActiveCell.Column.Key == "Select")
                {
                    // Use BeginInvoke to ensure this happens after the grid has fully processed the change
                    this.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            // Get the current value of the checkbox - safely handle different value types
                            bool isChecked = false;

                            // Check if the value exists and convert safely
                            if (ultraGrid1.ActiveCell.Value != null)
                            {
                                // Try different approaches to safely get a boolean value
                                if (ultraGrid1.ActiveCell.Value is bool)
                                {
                                    isChecked = (bool)ultraGrid1.ActiveCell.Value;
                                }
                                else if (ultraGrid1.ActiveCell.Value is int)
                                {
                                    isChecked = ((int)ultraGrid1.ActiveCell.Value) != 0;
                                }
                                else if (ultraGrid1.ActiveCell.Value is string)
                                {
                                    string strValue = ultraGrid1.ActiveCell.Value.ToString().ToLower();
                                    isChecked = strValue == "true" || strValue == "1" || strValue == "yes";
                                }
                                else
                                {
                                    // As a last resort, try safe conversion
                                    try
                                    {
                                        isChecked = Convert.ToBoolean(ultraGrid1.ActiveCell.Value);
                                    }
                                    catch
                                    {
                                        // If conversion fails, default to false
                                        isChecked = false;
                                    }
                                }
                            }

                            // Update row appearance
                            if (isChecked)
                            {
                                ultraGrid1.ActiveCell.Row.Appearance.BackColor = System.Drawing.Color.LightBlue;
                            }
                            else
                            {
                                ultraGrid1.ActiveCell.Row.Appearance.BackColor = System.Drawing.SystemColors.Window;
                            }

                            // Update totals
                            UpdateTotalAmount();
                        }
                        catch (Exception innerEx)
                        {
                        }
                    }));
                }
            }
            catch
            {
            }
        }
        // Add AfterSelectChange event handler to ensure checkbox selection captures all changes
        private void ultraGrid1_AfterSelectChange(object sender, AfterSelectChangeEventArgs e)
        {
            try
            {
                // First commit any pending changes
                ultraGrid1.UpdateData();

                // Force UI update
                Application.DoEvents();

                // Update the totals to reflect the selection change
                UpdateTotalAmount();
            }
            catch
            {
            }
        }

        private void ultraGrid1_AfterHeaderCheckStateChanged(object sender, AfterHeaderCheckStateChangedEventArgs e)
        {
            try
            {
                // Check if it's the "Select" column's header checkbox
                if (e.Column.Key == "Select")
                {

                    // Make sure data is committed
                    ultraGrid1.UpdateData();

                    // Recalculate totals
                    UpdateTotalAmount();
                }
            }
            catch
            {
            }
        }

        // Helper method to clear just the grid but preserve customer and other form data
        private void ClearGridOnly()
        {
            try
            {
                // Clear grid
                DataTable dt = (DataTable)ultraGrid1.DataSource;

                // Ensure the data table has all columns including Select
                if (!dt.Columns.Contains("Select"))
                {
                    dt.Columns.Add("Select", typeof(bool));
                }

                dt.Rows.Clear();
                ultraGrid1.DataSource = dt;

                // Clear totals
                TxtSubTotal.Text = "0.00";
                txtNetTotal.Text = "0.00";
                TxtInvoiceAmnt.Text = "0.00";

                // Keep textBox1 unchanged as it indicates the source
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error clearing grid: " + ex.Message);
            }
        }

        #region Tax Calculation Methods

        /// <summary>
        /// Calculates tax amount based on selling price, tax percentage, and tax type
        /// </summary>
        /// <param name="sellingPrice">The selling price</param>
        /// <param name="taxPercentage">Tax percentage</param>
        /// <param name="taxType">Tax type: "incl" or "excl"</param>
        /// <returns>Tax amount</returns>
        private double CalculateTaxAmount(double sellingPrice, double taxPercentage, string taxType)
        {
            // Check global tax setting - if disabled, return 0 for all tax calculations
            if (!DataBase.IsTaxEnabled)
            {
                return 0;
            }

            if (sellingPrice <= 0 || taxPercentage <= 0) return 0;

            if (taxType?.ToLower() == "incl")
            {
                // For inclusive tax: taxAmount = sellingPrice - (sellingPrice / (1 + taxPercentage/100))
                double divisor = 1.0 + (taxPercentage / 100.0);
                double basePrice = divisor > 0 ? (sellingPrice / divisor) : sellingPrice;
                return sellingPrice - basePrice;
            }
            else
            {
                // For exclusive tax: taxAmount = sellingPrice * (taxPercentage / 100)
                return sellingPrice * (taxPercentage / 100.0);
            }
        }

        /// <summary>
        /// Calculates total amount with tax based on tax type
        /// </summary>
        /// <param name="sellingPrice">The selling price</param>
        /// <param name="taxPercentage">Tax percentage</param>
        /// <param name="taxType">Tax type: "incl" or "excl"</param>
        /// <returns>Total amount including tax</returns>
        private double CalculateTotalWithTax(double sellingPrice, double taxPercentage, string taxType)
        {
            if (sellingPrice <= 0) return 0;

            // Check global tax setting - if disabled, return selling price without tax
            if (!DataBase.IsTaxEnabled)
            {
                return sellingPrice;
            }

            if (taxType?.ToLower() == "incl")
            {
                // For inclusive tax, selling price already includes tax
                return sellingPrice;
            }
            else
            {
                // For exclusive tax, add tax to selling price
                return sellingPrice + CalculateTaxAmount(sellingPrice, taxPercentage, taxType);
            }
        }

        /// <summary>
        /// Calculates total tax amount for all items in the grid
        /// </summary>
        /// <returns>Total tax amount</returns>
        private double CalculateTotalTaxAmount()
        {
            // Check global tax setting - if disabled, return 0
            if (!DataBase.IsTaxEnabled)
            {
                return 0;
            }

            double totalTax = 0;
            try
            {
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("TaxAmt") && row.Cells["TaxAmt"].Value != null)
                    {
                        if (double.TryParse(row.Cells["TaxAmt"].Value.ToString(), out double taxAmt))
                        {
                            totalTax += taxAmt;
                        }
                    }
                }
            }
            catch
            {
            }
            return totalTax;
        }

        /// <summary>
        /// Calculates subtotal (amount before tax) for all items
        /// </summary>
        /// <returns>Subtotal amount</returns>
        private double CalculateSubtotal()
        {
            double subtotal = 0;
            try
            {
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("Amount") && row.Cells["Amount"].Value != null)
                    {
                        if (double.TryParse(row.Cells["Amount"].Value.ToString(), out double amount))
                        {
                            // Check global tax setting - if disabled, use amount directly
                            if (!DataBase.IsTaxEnabled)
                            {
                                subtotal += amount;
                            }
                            else
                            {
                                // For tax-enabled mode, calculate proper subtotal based on tax type
                                string taxType = "excl"; // Default
                                if (row.Cells.Exists("TaxType") && row.Cells["TaxType"].Value != null)
                                {
                                    taxType = row.Cells["TaxType"].Value.ToString().ToLower();
                                }

                                if (taxType == "incl" || taxType == "inclusive")
                                {
                                    // For inclusive tax, subtract tax amount to get base price
                                    double taxAmt = 0;
                                    if (row.Cells.Exists("TaxAmt") && row.Cells["TaxAmt"].Value != null)
                                    {
                                        double.TryParse(row.Cells["TaxAmt"].Value.ToString(), out taxAmt);
                                    }
                                    subtotal += (amount - taxAmt);
                                }
                                else
                                {
                                    // For exclusive tax, amount is already the base price
                                    subtotal += amount;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return subtotal;
        }

        /// <summary>
        /// Calculates weighted average tax percentage for all items in the grid
        /// </summary>
        /// <returns>Weighted average tax percentage</returns>
        private double CalculateWeightedAverageTaxPercentage()
        {
            // Check global tax setting - if disabled, return 0
            if (!DataBase.IsTaxEnabled)
            {
                return 0;
            }

            double totalBaseAmount = 0;
            double totalTaxAmount = 0;

            try
            {
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("Amount") && row.Cells["Amount"].Value != null &&
                        row.Cells.Exists("TaxPer") && row.Cells["TaxPer"].Value != null)
                    {
                        if (double.TryParse(row.Cells["Amount"].Value.ToString(), out double amount) &&
                            double.TryParse(row.Cells["TaxPer"].Value.ToString(), out double taxPer))
                        {
                            totalBaseAmount += amount;
                            totalTaxAmount += amount * (taxPer / 100.0);
                        }
                    }
                }

                if (totalBaseAmount > 0)
                {
                    return (totalTaxAmount / totalBaseAmount) * 100.0;
                }
            }
            catch
            {
            }

            return 0;
        }

        /// <summary>
        /// Determines the tax type (inclusive/exclusive) based on the items in the grid
        /// </summary>
        /// <returns>Tax type: "incl" if any item has inclusive tax, otherwise "excl"</returns>
        private string DetermineTaxType()
        {
            try
            {
                // Check if any items have inclusive tax
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.IsDataRow && !row.IsFilteredOut &&
                        row.Cells.Exists("TaxType") && row.Cells["TaxType"].Value != null)
                    {
                        string taxType = row.Cells["TaxType"].Value.ToString().ToLower();
                        if (taxType == "incl" || taxType == "inclusive")
                        {
                            return "incl";
                        }
                    }
                }
            }
            catch
            {
            }

            return "excl"; // Default to exclusive tax
        }

        /// <summary>
        /// Checks if any items in the grid have inclusive tax
        /// </summary>
        /// <returns>True if any item has inclusive tax, false otherwise</returns>
        private bool HasInclusiveTax()
        {
            try
            {
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("TaxType") && row.Cells["TaxType"].Value != null)
                    {
                        string taxType = row.Cells["TaxType"].Value.ToString().ToLower();
                        if (taxType == "incl" || taxType == "inclusive")
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        /// Helper method to parse float values safely
        /// </summary>
        private double ParseFloat(object value, double defaultValue)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            if (double.TryParse(value.ToString(), out double result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Sets up default customer like frmSalesInvoice
        /// </summary>
        private void SetupDefaultCustomer()
        {
            try
            {
                if (customerTextBox != null)
                {
                    // Get customer list and find DEFAULT CUSTOMER
                    CustomerDDlGrid cs = dp.CustomerDDl();
                    if (cs?.List != null && cs.List.Any())
                    {
                        var defaultCustomer = cs.List.Where(f => f.LedgerName == "DEFAULT CUSTOMER").FirstOrDefault();
                        if (defaultCustomer != null)
                        {
                            customerTextBox.Value = defaultCustomer.LedgerName;
                            customerTextBox.Tag = defaultCustomer.LedgerID;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If setup fails, continue without default customer
                System.Diagnostics.Debug.WriteLine($"Error setting up default customer: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets up default payment mode to Cash
        /// </summary>
        private void SetupDefaultPaymentMode()
        {
            try
            {
                if (cmbPaymntMethod != null && cmbPaymntMethod.Items.Count > 0)
                {
                    // Find Cash payment mode and set it as default
                    for (int i = 0; i < cmbPaymntMethod.Items.Count; i++)
                    {
                        var item = cmbPaymntMethod.Items[i];
                        if (item.DisplayText != null && item.DisplayText.ToLower().Contains("cash"))
                        {
                            cmbPaymntMethod.SelectedIndex = i;
                            return;
                        }
                    }

                    // If Cash not found, set to first item
                    if (cmbPaymntMethod.SelectedIndex < 0)
                    {
                        cmbPaymntMethod.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // If setup fails, continue without default payment mode
                System.Diagnostics.Debug.WriteLine($"Error setting up default payment mode: {ex.Message}");
            }
        }

        #region Column Chooser and Drag-to-Hide Logic

        /// <summary>
        /// Sets up the context menu for column chooser
        /// </summary>
        private void SetupColumnChooserMenu()
        {
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += ColumnChooserMenuItem_Click;
            gridContextMenu.Items.Add(columnChooserMenuItem);
            ultraGrid1.ContextMenuStrip = gridContextMenu;
            SetupDirectHeaderDragDrop();
        }

        /// <summary>
        /// Sets up drag and drop for column headers to hide columns
        /// </summary>
        private void SetupDirectHeaderDragDrop()
        {
            ultraGrid1.AllowDrop = true;
            ultraGrid1.MouseDown += UltraGrid1_MouseDown_ColumnChooser;
            ultraGrid1.MouseMove += UltraGrid1_MouseMove_ColumnChooser;
            ultraGrid1.MouseUp += UltraGrid1_MouseUp_ColumnChooser;
            ultraGrid1.DragOver += UltraGrid1_DragOver_ColumnChooser;
            ultraGrid1.DragDrop += UltraGrid1_DragDrop_ColumnChooser;
            CreateColumnChooserForm();
        }

        /// <summary>
        /// Creates the column chooser form (popup listbox)
        /// </summary>
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
            columnChooserForm.FormClosing += (s, e) => { columnChooserListBox = null; };
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
            // Custom DrawItem handler for blue rounded button style
            columnChooserListBox.DrawItem += (s, evt) =>
            {
                if (evt.Index < 0) return;
                ColumnItem item = columnChooserListBox.Items[evt.Index] as ColumnItem;
                if (item == null) return;
                Rectangle rect = evt.Bounds;
                rect.Inflate(-3, -3);
                Color bgColor = Color.FromArgb(33, 150, 243); // Bright blue
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
            PopulateColumnChooserListBox();
        }

        /// <summary>
        /// Positions the column chooser form at the bottom right of the parent form
        /// </summary>
        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(
                    this.Right - columnChooserForm.Width - 20,
                    this.Bottom - columnChooserForm.Height - 20);
                columnChooserForm.TopMost = true;
                columnChooserForm.BringToFront();
            }
        }

        /// <summary>
        /// Handler for the column chooser menu item click
        /// </summary>
        private void ColumnChooserMenuItem_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }

        /// <summary>
        /// Shows the column chooser form
        /// </summary>
        private void ShowColumnChooser()
        {
            PopulateColumnChooserListBox();
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Show();
                PositionColumnChooserAtBottomRight();
                return;
            }
            CreateColumnChooserForm();
            columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
        }

        /// <summary>
        /// Populates the column chooser list box with hidden columns
        /// </summary>
        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;
            columnChooserListBox.Items.Clear();
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (col.Hidden)
                    {
                        string displayText = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                        columnChooserListBox.Items.Add(new ColumnItem(col.Key, displayText));
                    }
                }
            }
        }

        /// <summary>
        /// Helper class for column items in the column chooser
        /// </summary>
        private class ColumnItem
        {
            public string ColumnKey { get; set; }
            public string DisplayText { get; set; }
            public ColumnItem(string key, string text) { ColumnKey = key; DisplayText = text; }
            public override string ToString() => DisplayText;
        }

        /// <summary>
        /// Handler for mouse down on grid header - starts column drag
        /// </summary>
        private void UltraGrid1_MouseDown_ColumnChooser(object sender, MouseEventArgs e)
        {
            isDraggingColumn = false;
            columnToMove = null;
            startPoint = new Point(e.X, e.Y);
            if (e.Y < 40 && ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                int xPos = 0;
                if (ultraGrid1.DisplayLayout.Override.RowSelectors == Infragistics.Win.DefaultableBoolean.True)
                    xPos += ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
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

        /// <summary>
        /// Handler for mouse move on grid - handles column drag to hide
        /// </summary>
        private void UltraGrid1_MouseMove_ColumnChooser(object sender, MouseEventArgs e)
        {
            if (isDraggingColumn && columnToMove != null && e.Button == MouseButtons.Left)
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
                        columnToolTip.SetToolTip(ultraGrid1, $"Drag down to hide '{columnName}' column");
                        if (e.Y - startPoint.Y > 50)
                        {
                            HideColumn(columnToMove);
                            columnToMove = null;
                            isDraggingColumn = false;
                            ultraGrid1.Cursor = Cursors.Default;
                            columnToolTip.SetToolTip(ultraGrid1, "");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handler for mouse up on grid - ends column drag
        /// </summary>
        private void UltraGrid1_MouseUp_ColumnChooser(object sender, MouseEventArgs e)
        {
            ultraGrid1.Cursor = Cursors.Default;
            columnToolTip.SetToolTip(ultraGrid1, "");
            isDraggingColumn = false;
            columnToMove = null;
        }

        /// <summary>
        /// Hides a column and adds it to the column chooser
        /// </summary>
        private void HideColumn(Infragistics.Win.UltraWinGrid.UltraGridColumn column)
        {
            if (column != null && !column.Hidden)
            {
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
                        string columnName = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                        columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                    }
                }
                PopulateColumnChooserListBox();
            }
        }

        /// <summary>
        /// Handler for mouse down in the column chooser listbox - starts drag
        /// </summary>
        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                if (columnChooserListBox.Items[index] is ColumnItem item)
                {
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }

        /// <summary>
        /// Handler for drag over in the column chooser listbox
        /// </summary>
        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        /// <summary>
        /// Handler for drag drop in the column chooser listbox - hides the column
        /// </summary>
        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)) is Infragistics.Win.UltraWinGrid.UltraGridColumn column && !column.Hidden)
            {
                string name = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                column.Hidden = true;
                columnChooserListBox.Items.Add(new ColumnItem(column.Key, name));
            }
        }

        /// <summary>
        /// Handler for drag over on the grid - allows drop from column chooser
        /// </summary>
        private void UltraGrid1_DragOver_ColumnChooser(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(ColumnItem)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        /// <summary>
        /// Handler for drag drop on the grid - shows the column
        /// </summary>
        private void UltraGrid1_DragDrop_ColumnChooser(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnItem)) is ColumnItem item)
            {
                if (ultraGrid1.DisplayLayout.Bands.Count > 0 && ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                {
                    var column = ultraGrid1.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                    column.Hidden = false;
                    columnChooserListBox.Items.Remove(item);
                    columnToolTip.Show($"'{item.DisplayText}' restored", ultraGrid1, ultraGrid1.PointToClient(MousePosition), 1500);
                }
            }
        }

        #endregion

        #region Grid Footer Summary Panel Logic

        /// <summary>
        /// Initializes the grid footer panel (uses existing panel from Designer)
        /// </summary>
        private void InitializeGridFooterPanel()
        {
            // The gridFooterPanel is already defined in the Designer file
            // Just ensure it's visible and styled properly
            if (gridFooterPanel != null)
            {
                gridFooterPanel.Appearance.BackColor = Color.SteelBlue;
                gridFooterPanel.Visible = true;
            }
        }


        /// <summary>
        /// Initializes the summary footer panel functionality
        /// </summary>
        private void InitializeSummaryFooterPanel()
        {
            summaryFooterPanel = gridFooterPanel;
            if (summaryFooterPanel == null)
                return;
            summaryFooterPanel.Paint += (s, e) => { AlignSummaryLabels(); };
            summaryFooterPanel.Resize += (s, e) => { AlignSummaryLabels(); };
            ultraGrid1.AfterColPosChanged += (s, e) => AlignSummaryLabels();
            ultraGrid1.AfterSortChange += (s, e) => AlignSummaryLabels();
            ultraGrid1.AfterRowFilterChanged += (s, e) => AlignSummaryLabels();
            ultraGrid1.InitializeLayout += (s, e) => AlignSummaryLabels();
            ultraGrid1.SizeChanged += (s, e) => AlignSummaryLabels();

            // Panel-wide context menu (all columns)
            var panelMenu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type, null, OnPanelSummaryTypeSelected) { Tag = type };
                panelMenu.Items.Add(item);
            }
            summaryFooterPanel.ClientArea.ContextMenuStrip = panelMenu;
            summaryFooterPanel.ClientArea.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var ctrl = summaryFooterPanel.ClientArea.GetChildAtPoint(e.Location);
                    if (ctrl == null || !(ctrl is Label))
                        panelMenu.Show(summaryFooterPanel.ClientArea, e.Location);
                }
            };
        }

        /// <summary>
        /// Creates a context menu for an individual footer label
        /// </summary>
        private ContextMenuStrip CreateFooterLabelMenu(string columnKey)
        {
            var menu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type)
                {
                    Tag = type
                };
                item.Click += (s, e) =>
                {
                    columnAggregations[columnKey] = type;
                    UpdateFooterValues();
                };
                menu.Items.Add(item);
            }
            menu.Opening += (s, e) =>
            {
                foreach (ToolStripMenuItem item in menu.Items)
                {
                    item.Checked = columnAggregations.ContainsKey(columnKey) && columnAggregations[columnKey] == (string)item.Tag;
                }
            };
            return menu;
        }

        /// <summary>
        /// Handler for panel-wide summary type selection
        /// </summary>
        private void OnPanelSummaryTypeSelected(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string type)
            {
                currentSummaryType = type;
                // Set all visible numeric columns to this type
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    foreach (var col in ultraGrid1.DisplayLayout.Bands[0].Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
                    {
                        if (!col.Hidden && IsNumericColumn(col))
                            columnAggregations[col.Key] = type;
                    }
                }
                UpdateSummaryFooter();
            }
        }

        /// <summary>
        /// Updates the summary footer by recreating all labels
        /// </summary>
        private void UpdateSummaryFooter()
        {
            if (summaryFooterPanel == null || summaryFooterPanel.ClientArea == null || ultraGrid1 == null || ultraGrid1.DisplayLayout == null || ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;
            summaryFooterPanel.ClientArea.SuspendLayout();
            summaryFooterPanel.ClientArea.Controls.Clear();
            summaryLabels.Clear();
            var band = ultraGrid1.DisplayLayout.Bands[0];
            foreach (var col in band.Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
            {
                if (col.Hidden) continue;
                if (!IsNumericColumn(col)) continue;
                if (!columnAggregations.ContainsKey(col.Key) || columnAggregations[col.Key] == "None") continue;
                string agg = columnAggregations[col.Key];
                var lbl = new Label
                {
                    Name = $"lblSummary_{col.Key}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Height = summaryFooterPanel.Height - 4,
                    ContextMenuStrip = CreateFooterLabelMenu(col.Key)
                };
                summaryFooterPanel.ClientArea.Controls.Add(lbl);
                summaryLabels[col.Key] = lbl;
            }
            UpdateFooterValues();
            AlignSummaryLabels();
            summaryFooterPanel.ClientArea.ResumeLayout();
        }

        /// <summary>
        /// Updates the text of each summary label based on its aggregation type
        /// </summary>
        private void UpdateFooterValues()
        {
            if (ultraGrid1 == null || ultraGrid1.DataSource == null) return;
            if (ultraGrid1.DataSource is DataTable dt)
            {
                foreach (var kvp in summaryLabels)
                {
                    string colKey = kvp.Key;
                    Label lbl = kvp.Value;
                    string agg = columnAggregations.ContainsKey(colKey) ? columnAggregations[colKey] : "None";
                    var values = dt.AsEnumerable()
                        .Where(r => r[colKey] != DBNull.Value)
                        .Select(r => Convert.ToDouble(r[colKey]))
                        .ToList();
                    string text = "";
                    switch (agg)
                    {
                        case "Sum":
                            text = values.Count > 0 ? values.Sum().ToString("N2") : "0.00";
                            break;
                        case "Min":
                            text = values.Count > 0 ? values.Min().ToString("N2") : "-";
                            break;
                        case "Max":
                            text = values.Count > 0 ? values.Max().ToString("N2") : "-";
                            break;
                        case "Average":
                            text = values.Count > 0 ? values.Average().ToString("N2") : "-";
                            break;
                        case "Count":
                            text = values.Count.ToString();
                            break;
                    }
                    lbl.Text = text;
                }
            }
        }

        /// <summary>
        /// Aligns summary labels to match the grid column positions
        /// </summary>
        private void AlignSummaryLabels()
        {
            if (summaryFooterPanel == null || summaryFooterPanel.ClientArea == null || ultraGrid1 == null || ultraGrid1.DisplayLayout == null || ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;
            var band = ultraGrid1.DisplayLayout.Bands[0];
            foreach (var col in band.Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
            {
                if (col.Hidden) continue;
                if (!summaryLabels.TryGetValue(col.Key, out var lbl)) continue;
                var headerUI = ultraGrid1.DisplayLayout.Bands[0].Columns[col.Key].Header?.GetUIElement();
                if (headerUI != null)
                {
                    var gridPoint = ultraGrid1.PointToScreen(Point.Empty);
                    var headerPoint = headerUI.Control.PointToScreen(headerUI.Rect.Location);
                    int colLeft = headerPoint.X - summaryFooterPanel.PointToScreen(Point.Empty).X;
                    int colWidth = headerUI.Rect.Width;
                    lbl.Left = colLeft;
                    lbl.Width = colWidth;
                    lbl.Top = 2;
                    lbl.Height = summaryFooterPanel.Height - 4;
                }
            }
        }

        /// <summary>
        /// Checks if a column is numeric (can be aggregated)
        /// </summary>
        private bool IsNumericColumn(Infragistics.Win.UltraWinGrid.UltraGridColumn col)
        {
            var t = col.DataType;
            return t == typeof(int) || t == typeof(float) || t == typeof(double) || t == typeof(decimal) || t == typeof(long) || t == typeof(short);
        }

        /// <summary>
        /// Refreshes the summary footer values (call after grid data changes)
        /// </summary>
        private void RefreshSummaryFooter()
        {
            UpdateFooterValues();
            AlignSummaryLabels();
        }

        #endregion

        #endregion
    }
}
