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
using Repository.MasterRepositry;
using ModelClass;
using PosBranch_Win.DialogBox;
using Infragistics.Win.UltraWinGrid;
using Repository.Accounts;
using ModelClass.Accounts;
using Infragistics.Win;
using PosBranch_Win.Transaction;

namespace PosBranch_Win.Accounts
{
    public partial class FrmReceipt : Form
    {
        Dropdowns ObjDropd = new Dropdowns();
        // LedgerRepository ledgerRepo = new LedgerRepository();
        private decimal totalReceivedAmount = 0;
        private bool isAdjusting = false;
        private CustomerReceiptInfoRepository receiptRepo;
        private int currentCustomerLedgerId = 0;
        private int currentBranchId = 0; // Will be set from DataBase.BranchId in constructor
        private int selectionOrderCounter = 0; // Added for tracking selection order
        private int currentReceiptId = 0; // Tracks the currently loaded Receipt record ID
        private const string ReceiptStatusActive = "Active";
        private const string ReceiptStatusCancel = "Cancel";

        public FrmReceipt()
        {
            InitializeComponent();
            dtpPurchaseDate.Value = DateTime.Now;

            // Initialize branch ID from application context
            currentBranchId = SessionContext.BranchId;

            ultraGrid1.DataSource = CreateEmptyInvoiceTable(); // Bind empty table for consistent headers
            ConfigureGrid();
            ConfigureGridColumns(); // Set up appearance and headers immediately
            InitializeEventHandlers();
            LoadPaimentMethod();
            receiptRepo = new CustomerReceiptInfoRepository();
            InitializeReceiptStatusCombo();
            rdbtnoutstanding.Checked = true; // Ensure Outstanding is selected by default


            // Ensure form captures key events
            this.KeyPreview = true;
            this.KeyDown += FrmReceipt_KeyDown;

            // Wire up picture box click events
            ultraPictureBox1.Click += (s, e) => ClearForm();
            ultraPictureBox2.Click += (s, e) => CloseFormFromTab();
            this.ultraPictureBox7.Click += ultraPictureBox7_Click;

            // Wire up navigation panel events
            ultraPanel6.Click += (s, e) => NavigateReceipt("GETFIRST");
            ultraPictureBox3.Click += (s, e) => NavigateReceipt("GETFIRST");

            ultraPanel9.Click += (s, e) => NavigateReceipt("GETPREVIOUS");
            ultraPictureBox4.Click += (s, e) => NavigateReceipt("GETPREVIOUS");

            ultraPanel8.Click += (s, e) => NavigateReceipt("GETNEXT");
            ultraPictureBox6.Click += (s, e) => NavigateReceipt("GETNEXT");

            ultraPanel10.Click += (s, e) => NavigateReceipt("GETLAST");
            ultraPictureBox5.Click += (s, e) => NavigateReceipt("GETLAST");

            // Wire up button click events
            btnSales.Click += btnSales_Click;
            btnUpdate.Click += (s, e) =>
            {
                if (IsCancelStatusSelected())
                {
                    CancelLoadedReceipt();
                    return;
                }
                MessageBox.Show("Please select Cancel status to cancel/reverse a loaded customer receipt.",
                    "Receipt Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }
        private void ultraPictureBox7_Click(object sender, EventArgs e)
        {
            using (var dlg = new PosBranch_Win.DialogBox.frmSalesPersonDial())
            {
                dlg.OnSalesPersonSelected += (name) =>
                {
                    txtSalesMan.Text = name;
                };
                dlg.ShowDialog(this);
            }
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the selected row from the receipt grid
                if (ultraGrid1.ActiveRow == null)
                {
                    MessageBox.Show("Please select a bill from the list first.", "No Bill Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get the bill number from the selected row
                var billNoValue = ultraGrid1.ActiveRow.Cells["BillNo"].Value;
                if (billNoValue == null || billNoValue == DBNull.Value)
                {
                    return;
                }

                Int64 billNo = Convert.ToInt64(billNoValue);

                // Create and show the sales invoice form directly
                var salesInvoiceForm = new PosBranch_Win.Transaction.frmSalesInvoice();

                // Open the sales invoice form in a new tab first
                OpenSalesInvoiceInTab(salesInvoiceForm, $"Sales Invoice - Bill #{billNo}");

                // Wait a moment for the form to be fully embedded, then load the data
                System.Threading.Tasks.Task.Delay(100).ContinueWith(_ => {
                    try
                    {
                        if (!this.IsDisposed && this.IsHandleCreated)
                        {
                            this.Invoke(new Action(() => {
                                LoadBillIntoSalesForm(salesInvoiceForm, billNo);
                            }));
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // Form was disposed before we could invoke - ignore
                    }
                    catch (InvalidOperationException)
                    {
                        // Handle not created yet or form is being disposed - ignore
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening sales invoice: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBillIntoSalesForm(PosBranch_Win.Transaction.frmSalesInvoice salesForm, Int64 billNo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== LOADING BILL DATA FOR BILL #{billNo} ===");

                // Get the sales data from repository
                var salesRepo = new Repository.TransactionRepository.SalesRepository();
                var saleData = salesRepo.GetById(billNo);

                System.Diagnostics.Debug.WriteLine($"SaleData retrieved: {saleData != null}");
                if (saleData != null)
                {
                    System.Diagnostics.Debug.WriteLine($"ListSales count: {(saleData.ListSales != null ? saleData.ListSales.Count() : 0)}");
                    System.Diagnostics.Debug.WriteLine($"ListSDetails count: {(saleData.ListSDetails != null ? saleData.ListSDetails.Count() : 0)}");
                }

                if (saleData == null || saleData.ListSales == null || !saleData.ListSales.Any())
                {
                    return;
                }

                // Get the first sales master record
                var salesMaster = saleData.ListSales.First();
                System.Diagnostics.Debug.WriteLine($"SalesMaster: BillNo={salesMaster.BillNo}, CustomerName={salesMaster.CustomerName}, NetAmount={salesMaster.NetAmount}");

                // Use the GetMyBill method to populate the sales form
                salesForm.GetMyBill(salesMaster);
                System.Diagnostics.Debug.WriteLine("GetMyBill called successfully");

                // Load the bill items if available
                if (saleData.ListSDetails != null && saleData.ListSDetails.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"Loading {saleData.ListSDetails.Count()} bill items");
                    salesForm.LoadBillItems(saleData.ListSDetails.ToArray());
                    System.Diagnostics.Debug.WriteLine("LoadBillItems called successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No bill items to load");
                }

                System.Diagnostics.Debug.WriteLine("=== BILL DATA LOADING COMPLETE ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading bill data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show("Error loading bill data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenSalesInvoiceInTab(PosBranch_Win.Transaction.frmSalesInvoice salesForm, string tabName)
        {
            try
            {
                // Find the Home form (parent form with tab control)
                var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                if (homeForm != null)
                {
                    // Use reflection to call the improved OpenFormInTab method
                    var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (openFormInTabMethod != null)
                    {
                        // Call the improved OpenFormInTab method
                        openFormInTabMethod.Invoke(homeForm, new object[] { salesForm, tabName });
                        return;
                    }
                }

                // Fallback: show as regular form
                salesForm.Show();
                salesForm.BringToFront();
            }
            catch (Exception ex)
            {
                // Fallback: show as regular form
                salesForm.Show();
                salesForm.BringToFront();
                System.Diagnostics.Debug.WriteLine($"Error opening in tab, showing as regular form: {ex.Message}");
            }
        }
        private void LoadPaimentMethod()
        {
            PaymodeDDlGrid ObjPayModeDDL = ObjDropd.GetPaymode();
            CmboPayment.DataSource = ObjPayModeDDL.List;
            CmboPayment.DisplayMember = "PayModeName";
            CmboPayment.ValueMember = "PayModeID"; // UltraComboEditor supports these properties

            // Set default to 'Cash' if available
            if (ObjPayModeDDL.List != null)
            {
                foreach (var item in ObjPayModeDDL.List)
                {
                    var payModeNameProp = item.GetType().GetProperty("PayModeName");
                    var payModeIdProp = item.GetType().GetProperty("PayModeID");
                    if (payModeNameProp != null && payModeIdProp != null)
                    {
                        string payModeName = payModeNameProp.GetValue(item)?.ToString();
                        if (!string.IsNullOrEmpty(payModeName) && payModeName.Trim().ToLower() == "cash")
                        {
                            var payModeId = payModeIdProp.GetValue(item);
                            CmboPayment.Value = payModeId;
                            break;
                        }
                    }
                }
            }
        }
        private void InitializeEventHandlers()
        {
            txtReceivedAmount.TextChanged += txtReceivedAmount_TextChanged_1;

            // Add radio button event handlers
            rdbtnoutstanding.CheckedChanged += RadioButton_CheckedChanged;
            radioBtnAllDocument.CheckedChanged += RadioButton_CheckedChanged;
            btnSave.Click += btnSave_Click;

            // Add ViewReceipt button event handler
            btnViewReceipt.Click += btnViewReceipt_Click;

            // Add customer selection button event handler
            btnF11.Click += btnF11_Click;

            // Add LoadReceipt button event handler
            btnLoadReceipt.Click += btnLoadReceipt_Click;

            // Add KeyDown event for textBox4 (customer ID input)
            textBox4.KeyDown += textBox4_KeyDown;

            // Hide the local action panel (Save, Clear, Close) and stretch grid panel to fill the space
            ultraPanel2.Visible = false;
            ultraPanel5.Width = ultraPanel2.Right - ultraPanel5.Left;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsCancelStatusSelected())
                {
                    CancelLoadedReceipt();
                    return;
                }

                if (!ValidateReceipt())
                {
                    return;
                }

                // Get payment method ID from the combo box
                int paymentMethodId = CmboPayment.Value != null ? Convert.ToInt32(CmboPayment.Value) : 1; // Default to Cash (ID: 1)

                // Create master record with all required properties
                var master = new CustomerReceiptMaster
                {
                    CustomerName = txtCustomer.Text,
                    CustomerLedgerId = currentCustomerLedgerId,
                    // Store payment method ID as a string in the model
                    PaymentMethod = paymentMethodId.ToString(),
                    SalesPerson = txtSalesMan.Text + "|" + richTextBox2.Text,
                    TotalReceivableAmount = GetTotalReceivableAmount(),
                    TotalReceiptAmount = totalReceivedAmount,
                    ReceiptDate = DateTime.Now,
                    VoucherNo = GenerateVoucherNumber(),
                    BranchId = currentBranchId,
                    CreatedBy = SessionContext.UserId.ToString(), // Use current user ID from session
                    CreatedDate = DateTime.Now,
                    VoucherId = 0 // Will be generated by repository/stored procedure
                };

                // Create details records from grid - only for selected rows with non-zero adjustment
                var details = new List<CustomerReceiptDetails>();
                // Build a list of selected rows with their adjusted amounts
                var selectedRows = ultraGrid1.Rows
                    .Where(row => row.Cells["Select"].Value != null && Convert.ToBoolean(row.Cells["Select"].Value))
                    .Where(row => row.Cells["Adjusted Amount"].Value != null && Convert.ToDecimal(row.Cells["Adjusted Amount"].Value) > 0)
                    .ToList();

                foreach (var row in selectedRows)
                {
                    decimal adjustedAmount = Convert.ToDecimal(row.Cells["Adjusted Amount"].Value);
                    string billNo = row.Cells["BillNo"].Value?.ToString();
                    DateTime billDate = row.Cells["BillDate"].Value != null ? Convert.ToDateTime(row.Cells["BillDate"].Value) : DateTime.Now;

                    // IMPORTANT: use original outstanding (InvoiceAmount - ReceivedAmount - ReturnedAmount),
                    // not the grid Balance column because Balance is already reduced by Adjusted Amount.
                    decimal invoiceAmount = row.Cells["InvoiceAmount"].Value != null ? Convert.ToDecimal(row.Cells["InvoiceAmount"].Value) : 0m;
                    decimal alreadyReceived = row.Cells["ReceivedAmount"].Value != null ? Convert.ToDecimal(row.Cells["ReceivedAmount"].Value) : 0m;
                    decimal returnedAmount = row.Cells.Exists("ReturnedAmount") && row.Cells["ReturnedAmount"].Value != null && row.Cells["ReturnedAmount"].Value != DBNull.Value
                        ? Convert.ToDecimal(row.Cells["ReturnedAmount"].Value) : 0m;
                    decimal originalOutstanding = invoiceAmount - alreadyReceived - returnedAmount;
                    if (originalOutstanding < 0m)
                    {
                        originalOutstanding = 0m;
                    }

                    if (adjustedAmount > originalOutstanding)
                    {
                        MessageBox.Show($"Adjusted amount cannot exceed outstanding for Bill No {billNo}.",
                            "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    decimal balanceAfterAdjustment = originalOutstanding - adjustedAmount;
                    if (balanceAfterAdjustment < 0m)
                    {
                        balanceAfterAdjustment = 0m;
                    }

                    details.Add(new CustomerReceiptDetails
                    {
                        BillNo = billNo,
                        BillDate = billDate,
                        InvoiceAmount = originalOutstanding, // Outstanding before this receipt
                        AdjustedAmount = adjustedAmount,
                        Balance = balanceAfterAdjustment
                    });
                }

                // Check if we have any valid details to save
                if (details.Count == 0)
                {
                    MessageBox.Show("No valid invoice details to save. Please select at least one invoice and adjust the amount.",
                        "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create voucher entries
                var voucherEntries = new List<VoucherEntry>();

                // Credit entry for customer
                voucherEntries.Add(new VoucherEntry
                {
                    LedgerId = currentCustomerLedgerId,
                    CreditAmount = totalReceivedAmount,
                    DebitAmount = 0,
                    Narration = $"Receipt from {master.CustomerName}"
                });

                // Debit entry for cash/bank based on payment method
                voucherEntries.Add(new VoucherEntry
                {
                    LedgerId = paymentMethodId,
                    DebitAmount = totalReceivedAmount,
                    CreditAmount = 0,
                    Narration = $"Receipt from {master.CustomerName}"
                });

                // Save all data
                try
                {
                    if (receiptRepo.SaveCustomerReceipt(master, details, voucherEntries))
                    {
                        MessageBox.Show("Receipt saved successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Failed to save receipt.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving receipt: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving receipt: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private bool ValidateReceipt()
        {
            // Simplified validation - remove redundant checks
            if (totalReceivedAmount <= 0)
            {
                MessageBox.Show("Please enter a valid receipt amount.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var selectedRows = ultraGrid1.Rows.Where(row =>
                Convert.ToBoolean(row.Cells["Select"].Value)).ToList();

            if (!selectedRows.Any())
            {
                MessageBox.Show("Please select at least one invoice.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private decimal GetTotalReceivableAmount()
        {
            decimal totalReceivable = 0m;
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                decimal balance = 0m;
                if (row.Cells.Exists("Balance") && row.Cells["Balance"].Value != null && row.Cells["Balance"].Value != DBNull.Value)
                {
                    decimal.TryParse(row.Cells["Balance"].Value.ToString(), out balance);
                }

                if (balance > 0m)
                {
                    totalReceivable += balance;
                }
            }
            return totalReceivable;
        }

        private string GenerateVoucherNumber()
        {
            // Implement your voucher number generation logic here
            return $"RCP{DateTime.Now:yyyyMMddHHmmss}";
        }


        private void ClearForm()
        {
            currentReceiptId = 0;
            txtCustomer.Text = string.Empty;
            textBox4.Text = string.Empty;
            CmboPayment.Value = null;
            txtSalesMan.Text = string.Empty;
            richTextBox2.Text = string.Empty;
            txtReceivedAmount.Text = string.Empty;
            textBox3.Text = "0.00"; // Clear the outstanding amount field
            ultraGrid1.DataSource = null;
            currentCustomerLedgerId = 0;
            totalReceivedAmount = 0;
            selectionOrderCounter = 0; // Reset selection counter
            lblOutStanding.Text = "Outstanding";
            ultraTextEditor1.Text = "0.00";
            dtpPurchaseDate.Value = DateTime.Now;
            ultraTextEditor2.Value = ReceiptStatusActive;
            // Repository is already initialized in constructor - no need to recreate
        }
        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (currentCustomerLedgerId > 0)
            {
                LoadCustomerInvoices();
            }
        }
        private void LoadCustomerInvoices()
        {
            try
            {
                // Reset selection counter when loading new invoices
                selectionOrderCounter = 0;

                DataTable invoices;
                if (rdbtnoutstanding.Checked)
                {
                    invoices = receiptRepo.GetOutstandingInvoices(currentCustomerLedgerId, currentBranchId);
                    // Filter out invoices with Balance <= 0
                    if (invoices != null && invoices.Columns.Contains("Balance"))
                    {
                        var rows = invoices.AsEnumerable()
    .Where(row => {
        var val = row["Balance"];
        decimal balance = 0;
        if (val != DBNull.Value && val != null)
        {
            decimal.TryParse(val.ToString(), out balance);
        }
        return balance > 0;
    })
    .ToList();
                        if (rows.Count > 0)
                            invoices = rows.CopyToDataTable();
                        else
                            invoices = invoices.Clone(); // Return empty table with same schema
                    }
                }
                else
                {
                    invoices = receiptRepo.GetAllInvoices(currentCustomerLedgerId, SessionContext.BranchId); // Updated from currentBranchId
                }

                // Clear existing data
                ultraGrid1.DataSource = null;

                if (invoices != null && invoices.Rows.Count > 0)
                {
                    ultraGrid1.DataSource = invoices;
                    ConfigureGridColumns();

                    // Set all checkboxes to false, AdjustedAmount to 0, and clear SelectionOrder
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (row.Cells.Exists("Select")) row.Cells["Select"].Value = false;
                        if (row.Cells.Exists("Adjusted Amount")) row.Cells["Adjusted Amount"].Value = 0m;
                        if (row.Cells.Exists("SelectionOrder")) row.Cells["SelectionOrder"].Value = DBNull.Value;
                    }

                    // Set focus to grid and select first row
                    ultraGrid1.Focus();
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.Rows[0].Activated = true;
                        ultraGrid1.Rows[0].Selected = true;
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    }
                }
                // Removed unnecessary message - empty grid is normal behavior
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading invoices: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            // Modern grid appearance (match frmSalesInvoice)
            ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
            ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
            ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
            ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
            ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;

            // Header gradient and font
            ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = System.Drawing.Color.FromArgb(0, 102, 184);
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = System.Drawing.Color.White;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

            // Reset appearance
            ultraGrid1.DisplayLayout.Override.CellAppearance.Reset();
            ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.Reset();
            ultraGrid1.DisplayLayout.Override.SelectedCellAppearance.Reset();
            ultraGrid1.DisplayLayout.Override.RowAppearance.Reset();
            ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Reset();
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Reset();

            // Selection and navigation
            ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
            ultraGrid1.DisplayLayout.Override.SelectTypeCell = Infragistics.Win.UltraWinGrid.SelectType.Single;
            ultraGrid1.DisplayLayout.Override.SelectTypeCol = Infragistics.Win.UltraWinGrid.SelectType.None;
            ultraGrid1.DisplayLayout.TabNavigation = Infragistics.Win.UltraWinGrid.TabNavigation.NextCell;

            // Row and active row appearance
            ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = System.Drawing.Color.White;
            ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = System.Drawing.Color.FromArgb(240, 248, 255); // Light blue for alternating rows
            ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = System.Drawing.Color.FromArgb(220, 230, 240);
            ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = System.Drawing.Color.Black;
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = System.Drawing.Color.LightSkyBlue;
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = System.Drawing.Color.Black;
            ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.BackColor = System.Drawing.Color.Empty;
            ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.ForeColor = System.Drawing.Color.Black;
            ultraGrid1.DisplayLayout.Override.CellPadding = 4;
            ultraGrid1.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
            ultraGrid1.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;

            // Scrolling and sizing
            ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
            ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.None;

            // Set compact POS-like widths for each column
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                if (band.Columns.Exists("No")) band.Columns["No"].Width = 40;
                if (band.Columns.Exists("Document No")) band.Columns["Document No"].Width = 110;
                if (band.Columns.Exists("Date")) band.Columns["Date"].Width = 80;
                if (band.Columns.Exists("Due Date")) band.Columns["Due Date"].Width = 80;
                if (band.Columns.Exists("Amount")) band.Columns["Amount"].Width = 70;
                if (band.Columns.Exists("Outstanding")) band.Columns["Outstanding"].Width = 80;
                if (band.Columns.Exists("Payment")) band.Columns["Payment"].Width = 70;
                if (band.Columns.Exists("Selected")) band.Columns["Selected"].Width = 60;
            }
            ultraGrid1.DisplayLayout.Override.RowSizing = Infragistics.Win.UltraWinGrid.RowSizing.AutoFree;
            ultraGrid1.DisplayLayout.InterBandSpacing = 10;
            ultraGrid1.DisplayLayout.Override.ExpansionIndicator = Infragistics.Win.UltraWinGrid.ShowExpansionIndicator.Never;

            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Configure columns
                if (band.Columns.Exists("BillNo"))
                {
                    band.Columns["BillNo"].Header.Caption = "Bill No";
                    band.Columns["BillNo"].Width = 100;
                }

                if (band.Columns.Exists("BillDate"))
                {
                    band.Columns["BillDate"].Header.Caption = "Bill Date";
                    band.Columns["BillDate"].Width = 100;
                }

                if (band.Columns.Exists("InvoiceAmount"))
                {
                    band.Columns["InvoiceAmount"].Header.Caption = "Invoice Amount";
                    band.Columns["InvoiceAmount"].Width = 120;
                    band.Columns["InvoiceAmount"].Format = "##,##0.00";
                    band.Columns["InvoiceAmount"].CellActivation = Activation.NoEdit;
                }

                if (band.Columns.Exists("ReceivedAmount"))
                {
                    band.Columns["ReceivedAmount"].Header.Caption = "Received Amount";
                    band.Columns["ReceivedAmount"].Width = 120;
                    band.Columns["ReceivedAmount"].Format = "##,##0.00";
                    band.Columns["ReceivedAmount"].CellActivation = Activation.NoEdit;
                }

                if (band.Columns.Exists("Balance"))
                {
                    band.Columns["Balance"].Header.Caption = "Balance";
                    band.Columns["Balance"].Width = 120;
                    band.Columns["Balance"].Format = "##,##0.00";
                    band.Columns["Balance"].CellActivation = Activation.NoEdit;
                }

                if (band.Columns.Exists("ReturnedAmount"))
                {
                    band.Columns["ReturnedAmount"].Header.Caption = "Returned Amount";
                    band.Columns["ReturnedAmount"].Width = 120;
                    band.Columns["ReturnedAmount"].Format = "##,##0.00";
                    band.Columns["ReturnedAmount"].CellActivation = Activation.NoEdit;
                }

                // Add checkbox column if not exists
                if (!band.Columns.Exists("Select"))
                {
                    band.Columns.Add("Select", "Select");
                    band.Columns["Select"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                    band.Columns["Select"].Width = 50;
                }

                // Add SelectionOrder column if not exists (hidden)
                if (!band.Columns.Exists("SelectionOrder"))
                {
                    band.Columns.Add("SelectionOrder", "SelectionOrder");
                    band.Columns["SelectionOrder"].DataType = typeof(int);
                    band.Columns["SelectionOrder"].Hidden = true; // Hide this column
                }

                // Add Adjusted Amount column if not exists
                if (!band.Columns.Exists("Adjusted Amount"))
                {
                    band.Columns.Add("Adjusted Amount", "Adjusted Amount");
                    band.Columns["Adjusted Amount"].DataType = typeof(decimal);
                    band.Columns["Adjusted Amount"].CellActivation = Activation.AllowEdit;
                    band.Columns["Adjusted Amount"].Format = "##,##0.00";
                    band.Columns["Adjusted Amount"].Width = 120;
                }
            }
        }

        public void SetCustomerInfo(int ledgerId, string customerName)
        {
            // Ensure repository is initialized
            if (receiptRepo == null)
                receiptRepo = new CustomerReceiptInfoRepository();

            currentCustomerLedgerId = ledgerId;
            txtCustomer.Text = customerName;
            textBox4.Text = ledgerId.ToString();

            // Get and display the total outstanding amount
            try
            {
                decimal outstandingTotal = receiptRepo.GetCustomerOutstandingTotal(ledgerId);
                textBox3.Text = outstandingTotal.ToString("N2");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching outstanding total: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox3.Text = "0.00";
            }

            LoadCustomerInvoices();
        }

        private void ConfigureGrid()
        {
            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

            UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

            // Add checkbox column
            if (!band.Columns.Exists("Select"))
            {
                band.Columns.Add("Select", "Select");
                band.Columns["Select"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                band.Columns["Select"].Width = 50;
            }

            // Add SelectionOrder column (hidden)
            if (!band.Columns.Exists("SelectionOrder"))
            {
                band.Columns.Add("SelectionOrder", "SelectionOrder");
                band.Columns["SelectionOrder"].DataType = typeof(int);
                band.Columns["SelectionOrder"].Hidden = true; // Hide this column
            }

            // Configure Invoice Amount column
            if (band.Columns.Exists("InvoiceAmount"))
            {
                band.Columns["InvoiceAmount"].Header.Caption = "Invoice Amount";
                band.Columns["InvoiceAmount"].CellActivation = Activation.NoEdit;
            }

            // Add Adjusted Amount column
            if (!band.Columns.Exists("Adjusted Amount"))
            {
                band.Columns.Add("Adjusted Amount", "Adjusted Amount");
                band.Columns["Adjusted Amount"].DataType = typeof(decimal);
                band.Columns["Adjusted Amount"].CellActivation = Activation.AllowEdit;
                band.Columns["Adjusted Amount"].Format = "##,##0.00";
            }

            // Add Balance column
            if (!band.Columns.Exists("Balance"))
            {
                band.Columns.Add("Balance", "Balance");
                band.Columns["Balance"].DataType = typeof(decimal);
                band.Columns["Balance"].CellActivation = Activation.NoEdit;
                band.Columns["Balance"].Format = "##,##0.00";
            }

            // Handle events
            ultraGrid1.AfterCellUpdate += ultraGrid1_AfterCellUpdate_1;
            ultraGrid1.BeforeCellUpdate += UltraGrid1_BeforeCellUpdate;
            // Subscribe to CellChange for immediate checkbox update
            ultraGrid1.CellChange += ultraGrid1_CellChange;
        }

        private void UltraGrid1_BeforeCellUpdate(object sender, BeforeCellUpdateEventArgs e)
        {
            if (e.Cell.Column.Key == "Adjusted Amount")
            {
                if (!decimal.TryParse(e.NewValue?.ToString(), out decimal newAmount))
                {
                    e.Cancel = true;
                    return;
                }

                // Simplified validation - only check for negative values
                if (newAmount < 0)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void ultraGrid1_AfterCellUpdate_1(object sender, CellEventArgs e)
        {
            if (isAdjusting) return;

            if (e.Cell.Column.Key == "Select")
            {
                var row = e.Cell.Row;
                bool isSelected = Convert.ToBoolean(row.Cells["Select"].Value);

                if (isSelected)
                {
                    // Assign selection order
                    selectionOrderCounter++;
                    row.Cells["SelectionOrder"].Value = selectionOrderCounter;
                }
                else
                {
                    // Clear selection order and reset counter if needed
                    row.Cells["SelectionOrder"].Value = DBNull.Value;

                    // Reset selection counter and reassign order to remaining selected rows
                    ResetSelectionOrder();
                }

                DistributeAdjustedAmounts();
            }
            else if (e.Cell.Column.Key == "Adjusted Amount")
            {
                UpdateBalances();
                UpdateRemainingAmount();
            }
        }

        // Add this new method to reset selection order
        private void ResetSelectionOrder()
        {
            selectionOrderCounter = 0;

            // Get all selected rows and reassign order
            var selectedRows = ultraGrid1.Rows
                .Where(row => row.Cells.Exists("Select") &&
                             row.Cells["Select"].Value != null &&
                             Convert.ToBoolean(row.Cells["Select"].Value))
                .OrderBy(row => {
                    // Get current selection order, if any
                    if (row.Cells.Exists("SelectionOrder") &&
                        row.Cells["SelectionOrder"].Value != null &&
                        row.Cells["SelectionOrder"].Value != DBNull.Value)
                    {
                        return Convert.ToInt32(row.Cells["SelectionOrder"].Value);
                    }
                    return int.MaxValue; // Put rows without order at the end
                })
                .ToList();

            // Reassign consecutive order numbers
            foreach (var row in selectedRows)
            {
                selectionOrderCounter++;
                row.Cells["SelectionOrder"].Value = selectionOrderCounter;
            }
        }

        private void ultraGrid1_CellChange(object sender, CellEventArgs e)
        {
            if (e.Cell.Column.Key == "Select")
            {
                ultraGrid1.UpdateData(); // Commit the cell value
                // Don't call DistributeAdjustedAmounts here as it's already called in AfterCellUpdate
            }
        }

        private void DistributeAdjustedAmounts()
        {
            isAdjusting = true;

            // Reset all adjusted amounts first
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells.Exists("Adjusted Amount"))
                {
                    row.Cells["Adjusted Amount"].Value = 0m;
                }
            }

            // Get selected rows ordered by SelectionOrder (the order they were checked)
            var selectedRows = ultraGrid1.Rows
                .Where(row => row.Cells.Exists("Select") &&
                             row.Cells["Select"].Value != null &&
                             Convert.ToBoolean(row.Cells["Select"].Value))
                .Where(row => row.Cells.Exists("SelectionOrder") &&
                             row.Cells["SelectionOrder"].Value != null &&
                             row.Cells["SelectionOrder"].Value != DBNull.Value)
                .OrderBy(row => Convert.ToInt32(row.Cells["SelectionOrder"].Value)) // Order by selection sequence
                .ToList();

            decimal remaining = totalReceivedAmount;

            // Distribute amount to selected rows in the order they were selected
            foreach (UltraGridRow row in selectedRows)
            {
                if (remaining <= 0) break;

                if (row.Cells.Exists("InvoiceAmount") && row.Cells.Exists("ReceivedAmount"))
                {
                    // Safe null/DBNull checks before conversion
                    decimal invoiceAmount = row.Cells["InvoiceAmount"].Value != null && row.Cells["InvoiceAmount"].Value != DBNull.Value
                        ? Convert.ToDecimal(row.Cells["InvoiceAmount"].Value) : 0m;
                    decimal receivedAmount = row.Cells["ReceivedAmount"].Value != null && row.Cells["ReceivedAmount"].Value != DBNull.Value
                        ? Convert.ToDecimal(row.Cells["ReceivedAmount"].Value) : 0m;
                    decimal originalBalance = invoiceAmount - receivedAmount;

                    if (originalBalance > 0)
                    {
                        decimal adjusted = Math.Min(originalBalance, remaining);
                        if (row.Cells.Exists("Adjusted Amount"))
                        {
                            row.Cells["Adjusted Amount"].Value = adjusted;
                        }
                        remaining -= adjusted;
                    }
                }
            }

            // Update all balances
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                UpdateRowBalance(row);
            }

            isAdjusting = false;
            UpdateRemainingAmount();
        }

        private void UpdateBalances()
        {
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                UpdateRowBalance(row);
            }
        }

        private void UpdateRowBalance(UltraGridRow row)
        {
            if (row.Cells.Exists("InvoiceAmount") && row.Cells.Exists("ReceivedAmount") &&
                row.Cells.Exists("Adjusted Amount") && row.Cells.Exists("Balance"))
            {
                decimal invoiceAmount = Convert.ToDecimal(row.Cells["InvoiceAmount"].Value);
                decimal receivedAmount = Convert.ToDecimal(row.Cells["ReceivedAmount"].Value);
                decimal adjustedAmount = Convert.ToDecimal(row.Cells["Adjusted Amount"].Value);
                decimal returnedAmount = row.Cells.Exists("ReturnedAmount") && row.Cells["ReturnedAmount"].Value != null && row.Cells["ReturnedAmount"].Value != DBNull.Value
                    ? Convert.ToDecimal(row.Cells["ReturnedAmount"].Value) : 0m;
                // Original balance = InvoiceAmount - ReceivedAmount - ReturnedAmount.
                decimal originalBalance = invoiceAmount - receivedAmount - returnedAmount;
                if (originalBalance < 0m) originalBalance = 0m;
                // New balance = Original balance - Adjusted amount
                row.Cells["Balance"].Value = originalBalance - adjustedAmount;
            }
        }

        private decimal GetTotalAdjustedAmount()
        {
            decimal total = 0;
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells.Exists("Adjusted Amount") && row.Cells["Adjusted Amount"].Value != null)
                {
                    total += Convert.ToDecimal(row.Cells["Adjusted Amount"].Value);
                }
            }
            return total;
        }

        private decimal GetCurrentAdjustedAmount(UltraGridRow row)
        {
            if (row.Cells.Exists("Adjusted Amount") && row.Cells["Adjusted Amount"].Value != null)
            {
                return Convert.ToDecimal(row.Cells["Adjusted Amount"].Value);
            }
            return 0m;
        }

        private void UpdateRemainingAmount()
        {
            decimal totalAdjusted = GetTotalAdjustedAmount();
            decimal remaining = totalReceivedAmount - totalAdjusted;
            // Show remaining amount in ultraTextEditor1, not lblOutStanding
            ultraTextEditor1.Text = remaining.ToString("N2");
        }

        private void txtReceivedAmount_TextChanged_1(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtReceivedAmount.Text, out decimal amount))
            {
                totalReceivedAmount = amount;
            }
            else
            {
                totalReceivedAmount = 0m;
            }

            // Redistribute amounts when total received amount changes
            if (ultraGrid1.Rows.Count > 0)
            {
                DistributeAdjustedAmounts();
            }
        }

        private void btnF11_Click(object sender, EventArgs e)
        {
            frmCustomerDialog customerDialog = new frmCustomerDialog("FrmReceipt");
            customerDialog.Owner = this; // Set owner to enable proper communication back
            customerDialog.ShowDialog();
            // After dialog closes, set focus to grid and select first row
            ultraGrid1.Focus();
            if (ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.Rows[0].Activated = true;
                ultraGrid1.Rows[0].Selected = true;
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
            }
        }

        private void FrmReceipt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                // Save
                btnSave_Click(btnSave, EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F1)
            {
                // Clear
                ClearForm();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F4)
            {
                // Close
                CloseFormFromTab();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F11)
            {
                // Open customer dialog (same as btnF11_Click)
                btnF11_Click(btnF11, EventArgs.Empty);
                e.Handled = true;
            }
        }

        // Helper to close the form, including from tab control if needed
        private void CloseFormFromTab()
        {
            // Try to close the parent tab if inside a TabControl, else close the form
            if (this.Parent is TabPage tabPage && tabPage.Parent is TabControl tabControl)
            {
                tabControl.TabPages.Remove(tabPage);
            }
            this.Close();
        }

        private void btnViewReceipt_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the selected row from the grid (using ActiveRow instead of checking Selected property)
                UltraGridRow selectedRow = ultraGrid1.ActiveRow;

                if (selectedRow == null)
                {
                    MessageBox.Show("Please select a bill to view receipt history.", "Selection Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Get the BillNo from the selected row
                if (!selectedRow.Cells.Exists("BillNo") || selectedRow.Cells["BillNo"].Value == null)
                {
                    return;
                }

                string billNo = selectedRow.Cells["BillNo"].Value.ToString();

                // Get the original bill amount (InvoiceAmount) from the selected row
                decimal originalBillAmount = selectedRow.Cells.Exists("InvoiceAmount") && selectedRow.Cells["InvoiceAmount"].Value != null
                    ? Convert.ToDecimal(selectedRow.Cells["InvoiceAmount"].Value) : 0;

                // Open the receipt history form with the original bill amount
                FrmViewReceipt receiptHistoryForm = new FrmViewReceipt(currentCustomerLedgerId, billNo, originalBillAmount);
                receiptHistoryForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing receipt history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadReceipt_Click(object sender, EventArgs e)
        {
            try
            {
                using (OfficialReceiptList officialReceiptList = new OfficialReceiptList())
                {
                    if (officialReceiptList.ShowDialog(this) == DialogResult.OK && officialReceiptList.SelectedVoucherId > 0)
                    {
                        LoadReceipt(officialReceiptList.SelectedVoucherId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                int ledgerId;
                if (int.TryParse(textBox4.Text.Trim(), out ledgerId))
                {
                    // Use Dropdowns to get customer list
                    var customerList = ObjDropd.CustomerDDl().List;
                    var customer = customerList.FirstOrDefault(c => c.LedgerID == ledgerId);
                    if (customer != null)
                    {
                        SetCustomerInfo(customer.LedgerID, customer.LedgerName);
                    }
                    // Customer not found - silently continue
                }
                else
                {
                    MessageBox.Show("Please enter a valid numeric customer ID.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private DataTable CreateEmptyInvoiceTable()
        {
            var dt = new DataTable();
            // Column names must match exactly what ConfigureGridColumns() expects
            dt.Columns.Add("BillNo", typeof(string));
            dt.Columns.Add("BillDate", typeof(DateTime));
            dt.Columns.Add("DueDate", typeof(DateTime));
            dt.Columns.Add("InvoiceAmount", typeof(decimal));
            dt.Columns.Add("ReceivedAmount", typeof(decimal));
            dt.Columns.Add("ReturnedAmount", typeof(decimal));
            dt.Columns.Add("Balance", typeof(decimal));
            dt.Columns.Add("Select", typeof(bool));
            dt.Columns.Add("Adjusted Amount", typeof(decimal));
            dt.Columns.Add("SelectionOrder", typeof(int)); // For tracking selection order
            return dt;
        }

        private void NavigateReceipt(string direction)
        {
            try
            {
                long currentVoucherId = 0;
                long.TryParse(txtPurchaseNo.Text, out currentVoucherId);

                long nextVoucherId = receiptRepo.GetNavigationVoucherId(currentVoucherId, currentBranchId, direction);
                if (nextVoucherId > 0)
                {
                    LoadReceipt(nextVoucherId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error navigating receipts: " + ex.Message);
            }
        }

        private void LoadReceipt(long voucherId)
        {
            try
            {
                DataSet ds = receiptRepo.GetReceiptDataByVoucherId(voucherId, currentBranchId);
                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return;

                isAdjusting = true;
                DataRow master = ds.Tables[0].Rows[0];

                currentReceiptId = Convert.ToInt32(master["Id"]);
                txtPurchaseNo.Text = master["VoucherId"].ToString();
                currentCustomerLedgerId = Convert.ToInt32(master["CustomerLedgerId"]);
                textBox4.Text = currentCustomerLedgerId.ToString();
                ultraTextEditor2.Value = ReceiptStatusActive;
                
                // Get customer name from the 3rd table if available (Customer Info / Balance)
                if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
                {
                    txtCustomer.Text = ds.Tables[2].Rows[0]["LedgerName"]?.ToString() ?? "";
                    textBox3.Text = ds.Tables[2].Rows[0][1]?.ToString() ?? "0.00";
                }
                else
                {
                    // Fallback to searching customer name
                    var customerList = ObjDropd.CustomerDDl().List;
                    var customer = customerList.FirstOrDefault(c => c.LedgerID == currentCustomerLedgerId);
                    if (customer != null) txtCustomer.Text = customer.LedgerName;
                }

                txtReceivedAmount.Text = master["ReceiptAmount"]?.ToString() ?? "0.00";
                totalReceivedAmount = Convert.ToDecimal(master["ReceiptAmount"] ?? 0);
                
                if (master["PaymentMethodLedgerId"] != DBNull.Value)
                    CmboPayment.Value = master["PaymentMethodLedgerId"];
                
                if (master["VoucherDate"] != DBNull.Value)
                    dtpPurchaseDate.Value = Convert.ToDateTime(master["VoucherDate"]);

                string narration = master["Narration"]?.ToString() ?? "";
                if (narration.Contains("|"))
                {
                    string[] parts = narration.Split('|');
                    txtSalesMan.Text = parts[0];
                    richTextBox2.Text = parts[1];
                }
                else
                {
                    txtSalesMan.Text = narration;
                    richTextBox2.Text = string.Empty;
                }

                // Populate Grid
                DataTable dtGrid = CreateEmptyInvoiceTable();
                if (ds.Tables.Count > 1)
                {
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        DataRow newRow = dtGrid.NewRow();
                        newRow["BillNo"] = dr["BillNo"]?.ToString();
                        newRow["BillDate"] = dr["BillDate"];
                        newRow["InvoiceAmount"] = dr["BillAmount"];
                        newRow["ReceivedAmount"] = dr["ReceivedAmount"];
                        newRow["ReturnedAmount"] = dr.Table.Columns.Contains("ReturnedAmount") && dr["ReturnedAmount"] != DBNull.Value ? dr["ReturnedAmount"] : 0m;
                        newRow["Balance"] = dr["BalanceAmount"];
                        newRow["Select"] = true;
                        newRow["Adjusted Amount"] = dr["ReceiptAmount"];
                        dtGrid.Rows.Add(newRow);
                    }
                }
                ultraGrid1.DataSource = dtGrid;
                ConfigureGridColumns();

                isAdjusting = false;
            }
            catch (Exception ex)
            {
                isAdjusting = false;
                MessageBox.Show("Error loading receipt: " + ex.Message);
            }
        }

        // Ribbon Action Integrations
        public void Save()
        {
            btnSave_Click(null, EventArgs.Empty);
        }

        public void Clear()
        {
            ClearForm();
        }

        public void Delete()
        {
            if (currentReceiptId <= 0)
            {
                MessageBox.Show("No saved Receipt is loaded to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this Receipt?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (receiptRepo.DeleteCustomerReceipt(currentReceiptId))
                    {
                        MessageBox.Show("Receipt deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete Receipt.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting Receipt: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void InitializeReceiptStatusCombo()
        {
            ultraTextEditor2.Items.Clear();
            ultraTextEditor2.Items.Add(ReceiptStatusActive, ReceiptStatusActive);
            ultraTextEditor2.Items.Add(ReceiptStatusCancel, ReceiptStatusCancel);
            ultraTextEditor2.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            ultraTextEditor2.Value = ReceiptStatusActive;
        }

        private bool IsCancelStatusSelected()
        {
            string status = Convert.ToString(ultraTextEditor2.Value ?? ultraTextEditor2.Text);
            return string.Equals(status, ReceiptStatusCancel, StringComparison.OrdinalIgnoreCase);
        }

        private void CancelLoadedReceipt()
        {
            try
            {
                if (currentReceiptId <= 0)
                {
                    MessageBox.Show("Please load an existing customer receipt before selecting Cancel.",
                        "Receipt Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraTextEditor2.Value = ReceiptStatusActive;
                    return;
                }

                string voucherText = string.IsNullOrWhiteSpace(txtPurchaseNo.Text) ? currentReceiptId.ToString() : txtPurchaseNo.Text.Trim();
                DialogResult confirm = MessageBox.Show(
                    "Do you want to cancel/reverse customer receipt voucher " + voucherText + "?"
                    + Environment.NewLine + Environment.NewLine
                    + "All active bill allocations in this receipt voucher will be reversed and the receipt will be preserved as cancelled.",
                    "Cancel Customer Receipt",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                {
                    ultraTextEditor2.Value = ReceiptStatusActive;
                    return;
                }

                CustomerReceiptInfoRepository.CustomerReceiptCancellationSummary cancelled =
                    receiptRepo.CancelCustomerReceipt(
                        currentReceiptId,
                        currentBranchId,
                        SessionContext.UserId,
                        "Cancelled from Receipt screen");

                MessageBox.Show(
                    "Customer receipt cancelled successfully."
                    + Environment.NewLine + "Cancelled voucher(s): " + cancelled.ReceiptVoucherCount
                    + Environment.NewLine + "Reversed amount: " + cancelled.ReceiptAmount.ToString("N2"),
                    "Receipt Cancelled",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cancelling receipt: " + ex.Message,
                    "Receipt Cancellation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
