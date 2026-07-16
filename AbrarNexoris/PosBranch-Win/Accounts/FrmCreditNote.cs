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
    public partial class FrmCreditNote : Form
    {
        Dropdowns ObjDropd = new Dropdowns();
        private decimal totalCreditAmount = 0;
        private bool isAdjusting = false;
        private CreditNoteRepository creditNoteRepo;
        private int currentCustomerLedgerId = 0;
        private int currentBranchId = 0;
        private int currentCompanyId = 0;
        private int currentUserId = 0;
        private int selectionOrderCounter = 0;
        private int _sReturnNo = 0;
        private string _invoiceNo = "";
        private int currentCreditNoteId = 0; // Tracks the currently loaded Credit Note record ID

        public FrmCreditNote()
        {
            InitializeComponent();

            // Initialize branch ID from application context
            currentBranchId = Convert.ToInt32(DataBase.BranchId);
            currentCompanyId = Convert.ToInt32(DataBase.CompanyId);
            currentUserId = Convert.ToInt32(DataBase.UserId);

            ultraGrid1.DataSource = CreateEmptyInvoiceTable();
            ConfigureGrid();
            ConfigureGridColumns();
            InitializeEventHandlers();
            LoadAdjustmentAccounts();
            ultraTextEditor1.ReadOnly = true;
            creditNoteRepo = new CreditNoteRepository();
            rdbtnoutstanding.Checked = true;

            // Ensure form captures key events
            this.KeyPreview = true;
            this.KeyDown += FrmCreditNote_KeyDown;

            // Wire up picture box click events
            ultraPictureBox1.Click += (s, e) => ClearForm();
            ultraPictureBox2.Click += (s, e) => CloseFormFromTab();
            ultraPictureBox7.Click += ultraPictureBox7_Click; // Salesman dialog

            // btnSearchSalesReturn opens Sales Return Lookup (Goods Return List for Customer)
            btnSearchSalesReturn.Click += btnSearchSalesReturn_Click;

            // Wire up button click events
            btnSales.Click += btnSales_Click;
        }

        /// <summary>
        /// Constructor for creating credit note from Sales Return
        /// </summary>
        public FrmCreditNote(int sReturnNo, int customerLedgerId, string customerName,
            string invoiceNo, double creditAmount) : this()
        {
            _sReturnNo = sReturnNo;
            _invoiceNo = invoiceNo;

            // Set customer information
            currentCustomerLedgerId = customerLedgerId;
            txtCustomer.Text = customerName;
            textBox4.Text = customerLedgerId.ToString();
            txtReceivedAmount.Text = creditAmount.ToString("N2");
            totalCreditAmount = (decimal)creditAmount;

            // Set the visible Goods Return Note No. field
            if (sReturnNo > 0)
            {
                txtGoodsReturnNo.Text = $"GR{sReturnNo.ToString().PadLeft(8, '0')}";
            }

            // Load invoices for this customer
            LoadCustomerInvoices();
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

        /// <summary>
        /// Opens the Sales Return lookup dialog to select a pending goods return
        /// Filters by current customer if one is selected (like IRS POS "Goods Return List")
        /// </summary>
        private void btnSearchSalesReturn_Click(object sender, EventArgs e)
        {
            try
            {
                // Pass customer ID to filter returns for this customer only
                using (var dlg = new PosBranch_Win.DialogBox.frmSalesReturnLookup(currentCustomerLedgerId))
                {
                    dlg.OnSalesReturnSelected += (sReturnNo, ledgerId, customerName, invoiceNo, grandTotal, paymodeId) =>
                    {
                        // Load the selected sales return data into the form
                        LoadSalesReturnData(sReturnNo, ledgerId, customerName, invoiceNo, grandTotal);
                    };

                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        // Data is already loaded via the event handler
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening sales return lookup: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads Sales Return data into the Credit Note form
        /// </summary>
        private void LoadSalesReturnData(int sReturnNo, int ledgerId, string customerName, string invoiceNo, double grandTotal)
        {
            try
            {
                creditNoteRepo = new CreditNoteRepository();
                // Set internal tracking fields
                _sReturnNo = sReturnNo;
                _invoiceNo = invoiceNo;

                // Set customer information
                currentCustomerLedgerId = ledgerId;
                txtCustomer.Text = customerName;
                textBox4.Text = ledgerId.ToString();

                // Set credit amount
                txtReceivedAmount.Text = grandTotal.ToString("N2");
                totalCreditAmount = (decimal)grandTotal;

                // Set the Goods Return Note No. field
                txtGoodsReturnNo.Text = $"GR{sReturnNo.ToString().PadLeft(8, '0')}";

                // Load invoices for this customer
                LoadCustomerInvoices();

                MessageBox.Show($"Goods Return GR{sReturnNo.ToString().PadLeft(8, '0')} loaded.\n" +
                    $"Customer: {customerName}\nCredit Amount: {grandTotal:N2}\n\n" +
                    "Please select invoices to apply this credit to.",
                    "Goods Return Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales return data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the selected row from the grid
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

                // Create and show the sales return form (Credit Notes are related to Sales Returns)
                var salesReturnForm = new PosBranch_Win.Transaction.frmSalesReturn();

                // Open the sales return form in a new tab
                OpenSalesReturnInTab(salesReturnForm, $"Sales Return - Bill #{billNo}");

                // Load the data
                System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
                {
                    try
                    {
                        if (!this.IsDisposed && this.IsHandleCreated)
                        {
                            this.Invoke(new Action(() =>
                            {
                                LoadBillIntoSalesReturnForm(salesReturnForm, billNo);
                            }));
                        }
                    }
                    catch (ObjectDisposedException) { }
                    catch (InvalidOperationException) { }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening sales return: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBillIntoSalesReturnForm(PosBranch_Win.Transaction.frmSalesReturn salesReturnForm, Int64 billNo)
        {
            try
            {
                // Load the bill data from repository
                var salesRepo = new Repository.TransactionRepository.SalesRepository();
                var saleData = salesRepo.GetById(billNo);

                if (saleData == null || saleData.ListSales == null || !saleData.ListSales.Any())
                {
                    MessageBox.Show("Bill not found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var salesMaster = saleData.ListSales.First();

                // Set customer info first (required before loading bill)
                var customerTextBox = salesReturnForm.Controls.Find("textBox2", true).FirstOrDefault();
                if (customerTextBox != null && salesMaster.CustomerName != null)
                {
                    customerTextBox.Text = salesMaster.CustomerName;
                    customerTextBox.Tag = salesMaster.LedgerID;
                }

                // Also update the label1 to show customer ID
                var label1 = salesReturnForm.Controls.Find("label1", true).FirstOrDefault() as Label;
                if (label1 != null && salesMaster.LedgerID > 0)
                {
                    label1.Text = salesMaster.LedgerID.ToString();
                }

                // Set the invoice/bill number textbox (textBox1)
                var textBox1 = salesReturnForm.Controls.Find("textBox1", true).FirstOrDefault();
                if (textBox1 != null)
                {
                    textBox1.Text = billNo.ToString();
                }

                // Set invoice amount
                var txtInvoiceAmnt = salesReturnForm.Controls.Find("TxtInvoiceAmnt", true).FirstOrDefault();
                if (txtInvoiceAmnt != null)
                {
                    txtInvoiceAmnt.Text = salesMaster.NetAmount.ToString("N2");
                }

                // Set invoice date
                var dtInvoiceDate = salesReturnForm.Controls.Find("dtInvoiceDate", true).FirstOrDefault();
                if (dtInvoiceDate != null && dtInvoiceDate is Infragistics.Win.UltraWinEditors.UltraDateTimeEditor)
                {
                    ((Infragistics.Win.UltraWinEditors.UltraDateTimeEditor)dtInvoiceDate).Value = salesMaster.BillDate;
                }

                // Set net total
                var txtNetTotal = salesReturnForm.Controls.Find("txtNetTotal", true).FirstOrDefault();
                if (txtNetTotal != null)
                {
                    txtNetTotal.Text = salesMaster.NetAmount.ToString("N2");
                }

                // Load items using the same pattern as frmBillsDialog
                var operations = new Repository.TransactionRepository.SalesReturnRepository();
                var salesReturnDetails = operations.GetByIdSRDWithAvailableQty(billNo);

                if (salesReturnDetails != null && salesReturnDetails.List != null && salesReturnDetails.List.Any())
                {
                    // Clear existing items in the grid
                    var ultraGrid1 = salesReturnForm.Controls.Find("ultraGrid1", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                    if (ultraGrid1 != null && ultraGrid1.DataSource != null)
                    {
                        if (ultraGrid1.DataSource is DataTable dt)
                        {
                            dt.Rows.Clear();
                        }
                    }

                    // Add each item to the SalesReturn grid using AddItemToGrid
                    foreach (var item in salesReturnDetails.List)
                    {
                        string itemId = item.ItemId.ToString();
                        string itemName = item.Description ?? "";
                        string barcode = item.Barcode ?? "";
                        string unit = item.Unit ?? "";

                        decimal unitPrice = 0;
                        decimal.TryParse(item.UnitPrice.ToString(), out unitPrice);

                        int qty = 1;
                        int.TryParse(item.Qty.ToString(), out qty);

                        decimal amount = 0;
                        decimal.TryParse(item.Amount.ToString(), out amount);

                        double taxPer = item.TaxPer;
                        double taxAmt = item.TaxAmt;
                        string taxType = item.TaxType ?? "incl";
                        double returnedQty = item.ReturnedQty;
                        double returnQty = item.ReturnQty;

                        // Call AddItemToGrid on the sales return form
                        salesReturnForm.AddItemToGrid(itemId, itemName, barcode, unit, unitPrice, qty, amount,
                            taxPer, taxAmt, taxType, returnedQty, returnQty);
                    }

                    MessageBox.Show($"Bill #{billNo} loaded with {salesReturnDetails.List.Count()} items.\n\n" +
                        $"Customer: {salesMaster.CustomerName}\n" +
                        $"Amount: {salesMaster.NetAmount:N2}",
                        "Bill Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"No items found for Bill #{billNo}", "No Items", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading bill data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenSalesReturnInTab(PosBranch_Win.Transaction.frmSalesReturn salesReturnForm, string tabName)
        {
            try
            {
                var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                if (homeForm != null)
                {
                    var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (openFormInTabMethod != null)
                    {
                        openFormInTabMethod.Invoke(homeForm, new object[] { salesReturnForm, tabName });
                        return;
                    }
                }

                salesReturnForm.Show();
                salesReturnForm.BringToFront();
            }
            catch (Exception ex)
            {
                salesReturnForm.Show();
                salesReturnForm.BringToFront();
                System.Diagnostics.Debug.WriteLine($"Error opening in tab: {ex.Message}");
            }
        }

        private void LoadAdjustmentAccounts()
        {
            try
            {
                var ledgerRepo = new Repository.Accounts.LedgerRepository();
                DataTable ledgers = ledgerRepo.GetAllLedgers(currentBranchId);

                if (ledgers != null && ledgers.Rows.Count > 0)
                {
                    // Filter out Customer (16), Vendor (17), Cash (14), and Bank (15) groups if needed,
                    // but user specifically said "Cash involved: No", so we definitely filter out Cash and Bank.
                    var filteredRows = ledgers.AsEnumerable()
                        .Where(row =>
                        {
                            int groupId = row.Field<int>("GroupID");
                            return groupId != (int)AccountGroup.CASH_IN_HAND &&
                                   groupId != (int)AccountGroup.BANK_ACCOUNTS &&
                                   groupId != (int)AccountGroup.SUNDRY_DEBTORS &&
                                   groupId != (int)AccountGroup.SUNDRY_CREDITORS;
                        });

                    if (filteredRows.Any())
                    {
                        DataTable dtFiltered = filteredRows.CopyToDataTable();


                        // Try to default to "SALES" or "DISCOUNT ALLOWED"
                        foreach (DataRow row in dtFiltered.Rows)
                        {
                            string name = row["LedgerName"].ToString().ToUpper();
                            if (name.Contains("SALE") || name.Contains("DISCOUNT"))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading adjustment accounts: {ex.Message}");
            }
        }

        private void InitializeEventHandlers()
        {
            txtReceivedAmount.TextChanged += txtReceivedAmount_TextChanged;

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
                if (!ValidateCreditNote())
                {
                    return;
                }

                // Create master record with all required properties
                var master = new CreditNoteMaster
                {
                    CompanyId = currentCompanyId,
                    BranchId = currentBranchId,
                    FinYearId = SessionContext.FinYearId,
                    VoucherDate = dtpPurchaseDate.DateTime,
                    CustomerLedgerId = currentCustomerLedgerId,
                    CustomerName = txtCustomer.Text,
                    SReturnNo = _sReturnNo,
                    InvoiceNo = _invoiceNo ?? "",
                    CreditAmount = (double)totalCreditAmount,
                    PaymentMethodLedgerId = 1, // Default adjustment ledger
                    Narration = richTextBox2.Text ?? "",
                    UserId = currentUserId
                };

                // Build detail rows: only selected rows where credit was actually applied (> 0).
                // For fully-paid invoices (outstanding = 0), Credit Amount stays 0 and
                // no detail row is created — the credit is held at master level as store credit.
                var details = new List<CreditNoteDetails>();
                var selectedRows = ultraGrid1.Rows
                    .Where(row => row.Cells["Select"].Value != null && Convert.ToBoolean(row.Cells["Select"].Value))
                    .Where(row => GetSafeDecimal(row.Cells["Credit Amount"].Value) > 0)
                    .ToList();

                foreach (var row in selectedRows)
                {
                    decimal creditAmount = GetSafeDecimal(row.Cells["Credit Amount"].Value);
                    int billNo = GetSafeInt(row.Cells["BillNo"].Value);
                    DateTime billDate = row.Cells["BillDate"].Value != null ? Convert.ToDateTime(row.Cells["BillDate"].Value) : DateTime.Now;
                    decimal invoiceAmount = GetSafeDecimal(row.Cells["InvoiceAmount"].Value);
                    decimal currentBalance = GetSafeDecimal(row.Cells["Balance"].Value);

                    details.Add(new CreditNoteDetails
                    {
                        BranchId = currentBranchId,
                        FinYearId = SessionContext.FinYearId,
                        BillNo = billNo,
                        BillDate = billDate,
                        BillAmount = (double)invoiceAmount,
                        CreditAmount = (double)creditAmount,
                        BalanceAmount = (double)currentBalance,
                        OldBillAmount = (double)invoiceAmount
                    });
                }

                // NOTE: details.Count == 0 is VALID for fully-paid invoice returns.
                // The credit amount on the master record is the store credit for the customer.
                // Do NOT block saving here.

                // Save all data
                try
                {
                    // Skip voucher creation if coming from Sales Return (vouchers already created there)
                    bool skipVoucherCreation = _sReturnNo > 0;
                    if (creditNoteRepo.SaveCreditNote(master, details, skipVoucherCreation))
                    {
                        // Phase 2: use the lifecycle fields stamped by SaveCreditNote.
                        string statusLine;
                        if (master.CreditStatus == ModelClass.Accounts.CreditNoteStatus.Closed)
                            statusLine = "Status : Closed  (fully applied)";
                        else if (master.CreditStatus == ModelClass.Accounts.CreditNoteStatus.Partial)
                            statusLine = $"Status : Partial — Remaining: {master.RemainingAmount:N2}";
                        else
                            statusLine = $"Status : Open    — Store credit: {master.CreditAmount:N2}";

                        string message =
                            $"Credit Note saved successfully.\n\n" +
                            $"  Credit issued : {master.CreditAmount:N2}\n" +
                            $"  Applied       : {master.AppliedAmount:N2}\n" +
                            $"  Remaining     : {master.RemainingAmount:N2}\n" +
                            $"  {statusLine}";

                        MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Failed to save credit note.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving credit note: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving credit note: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateCreditNote()
        {
            // Rule 1: Customer is required.
            if (currentCustomerLedgerId <= 0)
            {
                MessageBox.Show("Please select a customer.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Rule 2: A positive credit amount is required.
            if (totalCreditAmount <= 0)
            {
                MessageBox.Show("Please enter a valid credit amount.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Rule 3: Applied credit cannot exceed the issued credit amount.
            decimal totalApplied = GetTotalCreditAmount();
            if (totalApplied > totalCreditAmount)
            {
                MessageBox.Show($"Total applied credit ({totalApplied:N2}) exceeds the credit amount ({totalCreditAmount:N2}).\n\nPlease adjust the values before saving.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Rule 4: FIX — Only require invoice allocation when there IS outstanding debt.
            // For fully-paid invoices (outstanding = 0), the credit amount alone is sufficient
            // to save; the credit will remain as available store credit for the customer.
            decimal totalOutstanding = GetTotalOutstandingAmount();
            if (totalOutstanding > 0 && totalApplied <= 0)
            {
                MessageBox.Show(
                    "This customer has invoices with an outstanding balance.\n" +
                    "Please tick at least one invoice and apply the credit amount to it.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Rule 5: Warn (but allow) when credit is not fully allocated.
            if (totalApplied > 0 && totalApplied < totalCreditAmount)
            {
                decimal unapplied = totalCreditAmount - totalApplied;
                var result = MessageBox.Show(
                    $"You have an unapplied amount of {unapplied:N2}.\n\n" +
                    "This will remain as available store credit for the customer.\n\n" +
                    "Do you want to proceed?",
                    "Confirm Unapplied Credit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No) return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the total outstanding balance across ALL invoices in the grid
        /// using the live formula: InvoiceAmount - ReceivedAmount.
        /// </summary>
        private decimal GetTotalOutstandingAmount()
        {
            decimal total = 0m;
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                decimal outstanding = row.Cells.Exists("OriginalBalance")
                    ? GetSafeDecimal(row.Cells["OriginalBalance"].Value)
                    : 0m;
                if (outstanding > 0) total += outstanding;
            }
            return total;
        }

        private decimal GetTotalReceivableAmount()
        {
            return ultraGrid1.Rows.Sum(row =>
                GetSafeDecimal(row.Cells["InvoiceAmount"].Value));
        }

        private void ClearForm()
        {
            currentCreditNoteId = 0;
            txtCustomer.Text = string.Empty;
            textBox4.Text = string.Empty;
            txtSalesMan.Text = string.Empty;
            txtReceivedAmount.Text = string.Empty;
            textBox3.Text = "0.00";
            richTextBox2.Text = string.Empty;
            txtGoodsReturnNo.Text = string.Empty; // Clear Goods Return No.
            ultraGrid1.DataSource = null;
            currentCustomerLedgerId = 0;
            totalCreditAmount = 0;
            selectionOrderCounter = 0;
            _sReturnNo = 0;
            _invoiceNo = "";
            ultraTextEditor1.Text = "0.00";
            dtpPurchaseDate.Value = DateTime.Now;
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
                selectionOrderCounter = 0;

                if (currentCustomerLedgerId <= 0)
                {
                    ultraGrid1.DataSource = CreateEmptyInvoiceTable();
                    return;
                }

                DataTable invoices;
                if (rdbtnoutstanding.Checked)
                {
                    invoices = creditNoteRepo.GetOutstandingInvoices(currentCustomerLedgerId, currentBranchId);
                }
                else
                {
                    invoices = creditNoteRepo.GetAllInvoices(currentCustomerLedgerId, currentBranchId);
                }

                // If a sales return is loaded and its source invoice is not in the list, force-append it!
                if (invoices != null && !string.IsNullOrEmpty(_invoiceNo) && !IsSalesReturnWithoutInvoice() && currentCustomerLedgerId > 0)
                {
                    bool found = false;
                    foreach (DataRow row in invoices.Rows)
                    {
                        if (row["BillNo"].ToString().Trim().Equals(_invoiceNo.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        DataRow sourceInvoiceRow = creditNoteRepo.GetInvoiceByBillNo(_invoiceNo, currentCustomerLedgerId, currentBranchId);
                        if (sourceInvoiceRow != null)
                        {
                            if (invoices.Columns.Count == 0)
                            {
                                invoices.Columns.Add("BillNo", typeof(string));
                                invoices.Columns.Add("BillDate", typeof(DateTime));
                                invoices.Columns.Add("DueDate", typeof(DateTime));
                                invoices.Columns.Add("InvoiceAmount", typeof(decimal));
                                invoices.Columns.Add("ReceivedAmount", typeof(decimal));
                                invoices.Columns.Add("ReturnedAmount", typeof(decimal));
                                invoices.Columns.Add("Balance", typeof(decimal));
                            }

                            DataRow newRow = invoices.NewRow();
                            newRow["BillNo"] = sourceInvoiceRow["BillNo"];
                            newRow["BillDate"] = sourceInvoiceRow["BillDate"];
                            newRow["DueDate"] = sourceInvoiceRow["DueDate"];
                            newRow["InvoiceAmount"] = sourceInvoiceRow["InvoiceAmount"];
                            newRow["ReceivedAmount"] = sourceInvoiceRow["ReceivedAmount"];
                            newRow["ReturnedAmount"] = sourceInvoiceRow.Table.Columns.Contains("ReturnedAmount") ? sourceInvoiceRow["ReturnedAmount"] : 0m;
                            newRow["Balance"] = sourceInvoiceRow["Balance"];
                            invoices.Rows.Add(newRow);
                        }
                    }
                }

                if (invoices == null)
                {
                    invoices = CreateEmptyInvoiceTable();
                }

                // Add columns if they do not exist
                if (!invoices.Columns.Contains("OriginalBalance"))
                    invoices.Columns.Add("OriginalBalance", typeof(decimal));
                if (!invoices.Columns.Contains("ReturnedAmount"))
                    invoices.Columns.Add("ReturnedAmount", typeof(decimal));
                if (!invoices.Columns.Contains("Select"))
                    invoices.Columns.Add("Select", typeof(bool));
                if (!invoices.Columns.Contains("Credit Amount"))
                    invoices.Columns.Add("Credit Amount", typeof(decimal));
                if (!invoices.Columns.Contains("SelectionOrder"))
                    invoices.Columns.Add("SelectionOrder", typeof(int));

                // Recalculate and set balances
                foreach (DataRow row in invoices.Rows)
                {
                    decimal inv = row.Table.Columns.Contains("InvoiceAmount") && !row.IsNull("InvoiceAmount") ? Convert.ToDecimal(row["InvoiceAmount"]) : 0m;
                    decimal rcv = row.Table.Columns.Contains("ReceivedAmount") && !row.IsNull("ReceivedAmount") ? Convert.ToDecimal(row["ReceivedAmount"]) : 0m;
                    decimal ret = row.Table.Columns.Contains("ReturnedAmount") && !row.IsNull("ReturnedAmount") ? Convert.ToDecimal(row["ReturnedAmount"]) : 0m;
                    
                    decimal outstanding = inv - rcv - ret;
                    if (outstanding < 0m) outstanding = 0m;

                    row["OriginalBalance"] = outstanding;
                    row["ReturnedAmount"] = ret;
                    if (invoices.Columns.Contains("Balance"))
                    {
                        row["Balance"] = outstanding;
                    }
                }

                if (rdbtnoutstanding.Checked)
                {
                    var rows = invoices.AsEnumerable()
                        .Where(row =>
                        {
                            decimal originalBalance = GetSafeDecimal(row["OriginalBalance"]);
                            bool isSourceInvoice = !string.IsNullOrEmpty(_invoiceNo)
                                && row.Table.Columns.Contains("BillNo")
                                && row["BillNo"].ToString().Trim().Equals(_invoiceNo.Trim(), StringComparison.OrdinalIgnoreCase);
                            return originalBalance > 0m || isSourceInvoice;
                        })
                        .ToList();

                    invoices = rows.Count > 0 ? rows.CopyToDataTable() : invoices.Clone();
                }

                ultraGrid1.DataSource = invoices;
                ConfigureGridColumns();

                // Set all checkboxes to false and credit amounts to 0
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("Select")) row.Cells["Select"].Value = false;
                    if (row.Cells.Exists("Credit Amount")) row.Cells["Credit Amount"].Value = 0m;
                    if (row.Cells.Exists("SelectionOrder")) row.Cells["SelectionOrder"].Value = DBNull.Value;
                }

                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.Focus();
                    ultraGrid1.Rows[0].Activated = true;
                    ultraGrid1.Rows[0].Selected = true;
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading invoices: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            // Modern grid appearance
            ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

            // Header styling
            ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(0, 122, 204);
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(0, 102, 184);
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;

            // Row appearance
            ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(240, 248, 255);
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.LightSkyBlue;

            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Configure existing columns
                if (band.Columns.Exists("BillNo"))
                {
                    band.Columns["BillNo"].Header.Caption = "Bill No";
                    band.Columns["BillNo"].Width = 100;
                    band.Columns["BillNo"].CellActivation = Activation.NoEdit;
                }

                if (band.Columns.Exists("BillDate"))
                {
                    band.Columns["BillDate"].Header.Caption = "Bill Date";
                    band.Columns["BillDate"].Width = 100;
                    band.Columns["BillDate"].CellActivation = Activation.NoEdit;
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

                if (band.Columns.Exists("ReturnedAmount"))
                {
                    band.Columns["ReturnedAmount"].Header.Caption = "Returned Amount";
                    band.Columns["ReturnedAmount"].Width = 120;
                    band.Columns["ReturnedAmount"].Format = "##,##0.00";
                    band.Columns["ReturnedAmount"].CellActivation = Activation.NoEdit;
                }

                if (band.Columns.Exists("Balance"))
                {
                    band.Columns["Balance"].Header.Caption = "Balance";
                    band.Columns["Balance"].Width = 120;
                    band.Columns["Balance"].Format = "##,##0.00";
                    band.Columns["Balance"].CellActivation = Activation.NoEdit;
                }

                // Add checkbox column if not exists
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
                    band.Columns["SelectionOrder"].Hidden = true;
                }

                // Add Credit Amount column if not exists
                if (!band.Columns.Exists("Credit Amount"))
                {
                    band.Columns.Add("Credit Amount", "Credit Amount");
                    band.Columns["Credit Amount"].DataType = typeof(decimal);
                    band.Columns["Credit Amount"].CellActivation = Activation.AllowEdit;
                    band.Columns["Credit Amount"].Format = "##,##0.00";
                    band.Columns["Credit Amount"].Width = 120;
                }

                // Add OriginalBalance column if not exists (hidden)
                if (!band.Columns.Exists("OriginalBalance"))
                {
                    band.Columns.Add("OriginalBalance", "OriginalBalance");
                    band.Columns["OriginalBalance"].DataType = typeof(decimal);
                }
                band.Columns["OriginalBalance"].Hidden = true;
            }
        }

        public void SetCustomerInfo(int ledgerId, string customerName)
        {
            creditNoteRepo = new CreditNoteRepository();

            currentCustomerLedgerId = ledgerId;
            txtCustomer.Text = customerName;
            textBox4.Text = ledgerId.ToString();

            // Get and display the total outstanding amount
            try
            {
                decimal outstandingTotal = creditNoteRepo.GetCustomerOutstandingTotal(ledgerId, currentBranchId);
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
                band.Columns["SelectionOrder"].Hidden = true;
            }

            // Add Credit Amount column
            if (!band.Columns.Exists("Credit Amount"))
            {
                band.Columns.Add("Credit Amount", "Credit Amount");
                band.Columns["Credit Amount"].DataType = typeof(decimal);
                band.Columns["Credit Amount"].CellActivation = Activation.AllowEdit;
                band.Columns["Credit Amount"].Format = "##,##0.00";
            }

            // Handle events
            ultraGrid1.AfterCellUpdate += ultraGrid1_AfterCellUpdate;
            ultraGrid1.BeforeCellUpdate += UltraGrid1_BeforeCellUpdate;
            ultraGrid1.CellChange += ultraGrid1_CellChange;
        }

        private void UltraGrid1_BeforeCellUpdate(object sender, BeforeCellUpdateEventArgs e)
        {
            if (e.Cell.Column.Key == "Credit Amount")
            {
                if (!decimal.TryParse(e.NewValue?.ToString(), out decimal newAmount))
                {
                    e.Cancel = true;
                    return;
                }

                if (newAmount < 0)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void ultraGrid1_AfterCellUpdate(object sender, CellEventArgs e)
        {
            if (isAdjusting) return;

            if (e.Cell.Column.Key == "Select")
            {
                var row = e.Cell.Row;
                bool isSelected = Convert.ToBoolean(row.Cells["Select"].Value);

                if (isSelected)
                {
                    selectionOrderCounter++;
                    row.Cells["SelectionOrder"].Value = selectionOrderCounter;
                }
                else
                {
                    row.Cells["SelectionOrder"].Value = DBNull.Value;
                    ResetSelectionOrder();
                }

                DistributeCreditAmounts();
            }
            else if (e.Cell.Column.Key == "Credit Amount")
            {
                UpdateBalances();
                UpdateRemainingAmount();
            }
        }

        private void ResetSelectionOrder()
        {
            selectionOrderCounter = 0;

            var selectedRows = ultraGrid1.Rows
                .Where(row => row.Cells.Exists("Select") &&
                             row.Cells["Select"].Value != null &&
                             Convert.ToBoolean(row.Cells["Select"].Value))
                .OrderBy(row =>
                {
                    if (row.Cells.Exists("SelectionOrder") &&
                        row.Cells["SelectionOrder"].Value != null &&
                        row.Cells["SelectionOrder"].Value != DBNull.Value)
                    {
                        return Convert.ToInt32(row.Cells["SelectionOrder"].Value);
                    }
                    return int.MaxValue;
                })
                .ToList();

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
                ultraGrid1.UpdateData();
            }
        }

        private void DistributeCreditAmounts()
        {
            isAdjusting = true;

            // Reset all credit amounts first
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells.Exists("Credit Amount"))
                    row.Cells["Credit Amount"].Value = 0m;
            }

            // Get selected rows ordered by SelectionOrder
            var selectedRows = ultraGrid1.Rows
                .Where(row => row.Cells.Exists("Select") &&
                              row.Cells["Select"].Value != null &&
                              Convert.ToBoolean(row.Cells["Select"].Value))
                .Where(row => row.Cells.Exists("SelectionOrder") &&
                              row.Cells["SelectionOrder"].Value != null &&
                              row.Cells["SelectionOrder"].Value != DBNull.Value)
                .OrderBy(row => GetSafeInt(row.Cells["SelectionOrder"].Value))
                .ToList();

            decimal remaining = totalCreditAmount;

            foreach (UltraGridRow row in selectedRows)
            {
                if (remaining <= 0) break;

                // Compute outstanding using OriginalBalance.
                decimal outstanding = row.Cells.Exists("OriginalBalance")
                    ? GetSafeDecimal(row.Cells["OriginalBalance"].Value)
                    : 0m;

                // FIX Bug #4: Skip invoices that have no outstanding balance.
                // Previously these rows received the remaining credit, causing
                // a negative balance display (e.g. 0 - 180 = -180).
                if (outstanding <= 0)
                    continue;

                // Allocate up to the outstanding amount from remaining credit.
                decimal toApply = Math.Min(outstanding, remaining);
                if (row.Cells.Exists("Credit Amount"))
                    row.Cells["Credit Amount"].Value = toApply;

                remaining -= toApply;
            }

            // Refresh displayed balance for every row.
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
            // Only update if we have the Credit Amount and Balance columns
            if (!row.Cells.Exists("Credit Amount") || !row.Cells.Exists("Balance"))
                return;

            decimal creditAmount = GetSafeDecimal(row.Cells["Credit Amount"].Value);

            // Retrieve the original balance from the hidden column
            decimal originalBalance = 0m;
            if (row.Cells.Exists("OriginalBalance"))
            {
                originalBalance = GetSafeDecimal(row.Cells["OriginalBalance"].Value);
            }
            else if (row.Cells.Exists("InvoiceAmount"))
            {
                decimal invoiceAmount = GetSafeDecimal(row.Cells["InvoiceAmount"].Value);
                decimal receivedAmount = GetSafeDecimal(row.Cells["ReceivedAmount"].Value);
                originalBalance = invoiceAmount - receivedAmount;
                if (originalBalance < 0m) originalBalance = 0m;
            }

            // Ensure credit amount doesn't exceed original balance (only for invoices with positive outstanding balance)
            if (originalBalance > 0 && creditAmount > originalBalance)
            {
                isAdjusting = true;
                creditAmount = originalBalance;
                row.Cells["Credit Amount"].Value = creditAmount;
                isAdjusting = false;
            }

            // New balance = Original balance - Credit being applied
            row.Cells["Balance"].Value = originalBalance - creditAmount;
        }

        private decimal GetTotalCreditAmount()
        {
            decimal total = 0;
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells.Exists("Credit Amount"))
                {
                    total += GetSafeDecimal(row.Cells["Credit Amount"].Value);
                }
            }
            return total;
        }

        private void UpdateRemainingAmount()
        {
            decimal totalApplied = GetTotalCreditAmount();
            decimal remaining = totalCreditAmount - totalApplied;
            ultraTextEditor1.Text = remaining.ToString("N2");
        }

        private void txtReceivedAmount_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtReceivedAmount.Text, out decimal amount))
            {
                totalCreditAmount = amount;
            }
            else
            {
                totalCreditAmount = 0m;
            }

            if (ultraGrid1.Rows.Count > 0)
            {
                DistributeCreditAmounts();
            }
            else
            {
                UpdateRemainingAmount();
            }
        }

        private void btnF11_Click(object sender, EventArgs e)
        {
            frmCustomerDialog customerDialog = new frmCustomerDialog("FrmCreditNote");
            customerDialog.Owner = this;
            customerDialog.ShowDialog();
            ultraGrid1.Focus();
            if (ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.Rows[0].Activated = true;
                ultraGrid1.Rows[0].Selected = true;
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
            }
        }

        private void FrmCreditNote_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                btnSave_Click(btnSave, EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F1)
            {
                ClearForm();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F4)
            {
                CloseFormFromTab();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F11)
            {
                btnF11_Click(btnF11, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void CloseFormFromTab()
        {
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
                UltraGridRow selectedRow = ultraGrid1.ActiveRow;

                if (selectedRow == null)
                {
                    MessageBox.Show("Please select a bill to view credit note history.", "Selection Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!selectedRow.Cells.Exists("BillNo") || selectedRow.Cells["BillNo"].Value == null)
                {
                    return;
                }

                string billNo = selectedRow.Cells["BillNo"].Value.ToString();
                decimal originalBillAmount = selectedRow.Cells.Exists("InvoiceAmount")
                    ? GetSafeDecimal(selectedRow.Cells["InvoiceAmount"].Value) : 0m;

                MessageBox.Show($"Credit Note history for Bill #: {billNo}\nAmount: {originalBillAmount:N2}",
                    "Credit Note History", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing credit note history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadReceipt_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new DialogBox.frmCreditNoteList())
                {
                    form.OnCreditNoteSelected += (voucherId) =>
                    {
                        try
                        {
                            DataSet ds = creditNoteRepo.GetCreditNoteById(voucherId, currentBranchId);
                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                LoadCreditNoteData(ds);
                            }
                            else
                            {
                                MessageBox.Show("Credit Note not found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error loading Credit Note details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Credit Note list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCreditNoteData(DataSet ds)
        {
            DataRow masterRow = ds.Tables[0].Rows[0];

            currentCreditNoteId = Convert.ToInt32(masterRow["Id"]);
            currentCustomerLedgerId = Convert.ToInt32(masterRow["CustomerLedgerId"]);
            _sReturnNo = Convert.ToInt32(masterRow["SReturnNo"] ?? 0);
            _invoiceNo = masterRow["InvoiceNo"]?.ToString();

            txtPurchaseNo.Text = masterRow["VoucherId"].ToString();
            textBox4.Text = currentCustomerLedgerId.ToString();
            dtpPurchaseDate.Value = Convert.ToDateTime(masterRow["VoucherDate"]);
            txtReceivedAmount.Text = Convert.ToDouble(masterRow["CreditAmount"]).ToString("N2");
            richTextBox2.Text = masterRow["Narration"]?.ToString() ?? "";

            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
            {
                DataTable dtDetails = ds.Tables[1];

                // Map columns to match the standard grid layout
                if (!dtDetails.Columns.Contains("InvoiceAmount") && dtDetails.Columns.Contains("BillAmount"))
                {
                    dtDetails.Columns.Add("InvoiceAmount", typeof(decimal));
                }
                if (!dtDetails.Columns.Contains("Credit Amount") && dtDetails.Columns.Contains("CreditAmount"))
                {
                    dtDetails.Columns.Add("Credit Amount", typeof(decimal));
                }
                if (!dtDetails.Columns.Contains("Balance") && dtDetails.Columns.Contains("BalanceAmount"))
                {
                    dtDetails.Columns.Add("Balance", typeof(decimal));
                }
                if (!dtDetails.Columns.Contains("Select"))
                {
                    dtDetails.Columns.Add("Select", typeof(bool));
                }
                if (!dtDetails.Columns.Contains("OriginalBalance"))
                {
                    dtDetails.Columns.Add("OriginalBalance", typeof(decimal));
                }

                foreach (DataRow row in dtDetails.Rows)
                {
                    row["Select"] = true;

                    decimal invoiceAmt = row.Table.Columns.Contains("BillAmount") ? GetSafeDecimal(row["BillAmount"]) : 0m;
                    decimal creditAmt = row.Table.Columns.Contains("CreditAmount") ? GetSafeDecimal(row["CreditAmount"]) : 0m;
                    decimal balAmt = row.Table.Columns.Contains("BalanceAmount") ? GetSafeDecimal(row["BalanceAmount"]) : 0m;

                    if (row.Table.Columns.Contains("InvoiceAmount"))
                        row["InvoiceAmount"] = invoiceAmt;

                    if (row.Table.Columns.Contains("Credit Amount"))
                        row["Credit Amount"] = creditAmt;

                    if (row.Table.Columns.Contains("Balance"))
                        row["Balance"] = balAmt;

                    // Since it was saved, original balance before credit note was applied was (BalanceAmount + CreditAmount)
                    row["OriginalBalance"] = balAmt + creditAmt;
                }

                ultraGrid1.DataSource = dtDetails;
                ConfigureGridColumns();
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
            if (currentCreditNoteId <= 0)
            {
                MessageBox.Show("No saved Credit Note is loaded to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this Credit Note?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (creditNoteRepo.DeleteCreditNote(currentCreditNoteId))
                    {
                        MessageBox.Show("Credit Note deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete Credit Note.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting Credit Note: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
                    var customerList = ObjDropd.CustomerDDl().List;
                    var customer = customerList.FirstOrDefault(c => c.LedgerID == ledgerId);
                    if (customer != null)
                    {
                        SetCustomerInfo(customer.LedgerID, customer.LedgerName);
                    }
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
            dt.Columns.Add("BillNo", typeof(string));
            dt.Columns.Add("BillDate", typeof(DateTime));
            dt.Columns.Add("DueDate", typeof(DateTime));
            dt.Columns.Add("InvoiceAmount", typeof(decimal));
            dt.Columns.Add("ReceivedAmount", typeof(decimal));
            dt.Columns.Add("ReturnedAmount", typeof(decimal));
            dt.Columns.Add("Balance", typeof(decimal));
            dt.Columns.Add("Select", typeof(bool));
            dt.Columns.Add("Credit Amount", typeof(decimal));
            dt.Columns.Add("SelectionOrder", typeof(int));
            dt.Columns.Add("OriginalBalance", typeof(decimal));
            return dt;
        }

        private void btn_Add_Custm_Click(object sender, EventArgs e)
        {

        }

        private bool IsSalesReturnWithoutInvoice()
        {
            if (_sReturnNo <= 0)
            {
                return false;
            }

            string invoiceNo = (_invoiceNo ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(invoiceNo))
            {
                return true;
            }

            string normalized = invoiceNo.ToUpperInvariant();
            return normalized == "WITHOUT BILL" || normalized == "WITHOUT SALES BILL" || normalized == "WITHOUT GR";
        }

        private decimal GetSafeDecimal(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0m;
            if (decimal.TryParse(value.ToString(), out decimal res))
                return res;
            return 0m;
        }

        private double GetSafeDouble(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;
            if (double.TryParse(value.ToString(), out double res))
                return res;
            return 0;
        }

        private int GetSafeInt(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;
            if (int.TryParse(value.ToString(), out int res))
                return res;
            return 0;
        }
    }
}
