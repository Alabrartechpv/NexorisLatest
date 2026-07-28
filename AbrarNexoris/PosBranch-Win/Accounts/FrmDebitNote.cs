using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win;
using Repository;
using Repository.Accounts;
using ModelClass;
using ModelClass.Accounts;

using ModelClass.Accounts;
using PosBranch_Win.DialogBox;

namespace PosBranch_Win.Accounts
{
    public partial class FrmDebitNote : Form
    {
        private DebitNoteRepository debitNoteRepo;
        private Dropdowns ObjDropd = new Dropdowns();
        private int currentVendorLedgerId = 0;
        private int currentBranchId;
        private decimal totalDebitAmount = 0;
        private int _pReturnNo = 0;
        private string _invoiceNo = "";
        private bool isAdjusting = false;
        private bool isLoadingData = false;  // Prevents event loops during programmatic loads
        private int selectionOrderCounter = 0;
        private int currentDebitNoteId = 0; // Tracks the currently loaded Debit Note record ID

        public FrmDebitNote()
        {
            InitializeComponent();
            InitializeForm();
        }

        // Constructor for opening from Purchase Return
        public FrmDebitNote(int pReturnNo, int vendorLedgerId, string vendorName, decimal returnAmount, string invoiceNo = "")
        {
            InitializeComponent();
            InitializeForm();

            // Set Purchase Return data
            _pReturnNo = pReturnNo;
            _invoiceNo = invoiceNo;
            currentVendorLedgerId = vendorLedgerId;

            // Pre-fill the form
            txtPurchaseNo.Text = pReturnNo.ToString();
            textBox4.Text = vendorLedgerId.ToString();
            txtVendorName.Text = vendorName;
            isLoadingData = true;
            textBox1.Text = returnAmount.ToString("N2");
            isLoadingData = false;
            totalDebitAmount = returnAmount;

            // Load vendor outstanding
            LoadVendorOutstanding();

            // Load vendor invoices
            LoadVendorInvoices();
        }

        private void InitializeForm()
        {
            debitNoteRepo = new DebitNoteRepository();
            currentBranchId = Convert.ToInt32(DataBase.BranchId);

            // Initialize date
            dtpPurchaseDate.Value = DateTime.Now;


            // Load payment methods - Not used for Credit/Debit Note adjustments
            // LoadPaymentMethods();

            // Set button texts
            btnViewPayment.Text = "View Debit Note";
            btnPurch.Text = "Pending Returns";

            // Configure grid
            ConfigureGrid();
            ultraGrid1.DataSource = CreateEmptyInvoiceTable();
            ConfigureGridColumns();

            // Wire up events
            this.KeyPreview = true;
            this.KeyDown += FrmDebitNote_KeyDown;
            btnF11.Click += btnF11_Click;
            ultraPictureBox1.Click += btnSave_Click;
            ultraPictureBox2.Click += btnClear_Click;
            ultraPictureBox3.Click += btnClose_Click;
            btnViewPayment.Click += btnViewDebitNote_Click;
            btnPurch.Click += btnSearchPurchaseReturn_Click;
            // Wire the UltraPictureBox controls for Purchase Return lookup 
            btnSearchPurchaseReturn.Click += btnSearchPurchaseReturn_Click;
            btnCreatePurchaseReturn.Click += btnCreatePurchaseReturn_Click;
            ultraPictureBox10.Click += btnViewDebitNote_Click;
            textBox1.TextChanged += txtDebitAmount_TextChanged;
            textBox4.KeyDown += textBox4_KeyDown;
            rdbtnoutstanding.CheckedChanged += rdbtnOutstanding_CheckedChanged;
            radioBtnAllDocument.CheckedChanged += radioBtnAllDocument_CheckedChanged;
            ultraGrid1.BeforeCellUpdate += UltraGrid1_BeforeCellUpdate;
            ultraGrid1.AfterCellUpdate += ultraGrid1_AfterCellUpdate;
            ultraGrid1.CellChange += ultraGrid1_CellChange;

            // Set default radio button
            rdbtnoutstanding.Checked = true;

            // Hide the local action buttons panel (Save, Clear, Close) and stretch grid panel to fill the space
            ultraPanel6.Visible = false;
            ultraPanel5.Width = ultraPanel6.Right - ultraPanel5.Left;
        }

        // LoadPaymentMethods removed - payment method selection not needed for Debit Notes

        public void SetVendorInfo(int ledgerId, string vendorName)
        {
            debitNoteRepo = new DebitNoteRepository();
            currentVendorLedgerId = ledgerId;
            textBox4.Text = ledgerId.ToString();
            txtVendorName.Text = vendorName;

            // Load vendor outstanding
            LoadVendorOutstanding();

            // Load vendor invoices
            LoadVendorInvoices();
        }

        private void LoadVendorOutstanding()
        {
            try
            {
                if (currentVendorLedgerId > 0)
                {
                    decimal outstanding = debitNoteRepo.GetVendorOutstandingTotal(currentVendorLedgerId, currentBranchId);
                    txtOutstanding.Text = outstanding.ToString("N2");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vendor outstanding: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadVendorInvoices()
        {
            try
            {
                if (currentVendorLedgerId <= 0 && !string.IsNullOrEmpty(_invoiceNo))
                {
                    currentVendorLedgerId = debitNoteRepo.GetVendorLedgerIdByInvoiceNo(_invoiceNo, currentBranchId);
                    if (currentVendorLedgerId > 0)
                    {
                        textBox4.Text = currentVendorLedgerId.ToString();
                        string vendorName = debitNoteRepo.GetVendorNameByLedgerId(currentVendorLedgerId);
                        if (!string.IsNullOrEmpty(vendorName) && string.IsNullOrEmpty(txtVendorName.Text))
                        {
                            txtVendorName.Text = vendorName;
                        }
                    }
                }

                if (currentVendorLedgerId <= 0)
                {
                    ultraGrid1.DataSource = CreateEmptyInvoiceTable();
                    return;
                }

                DataTable dt;
                if (rdbtnoutstanding.Checked)
                {
                    dt = debitNoteRepo.GetOutstandingInvoices(currentVendorLedgerId, currentBranchId);
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        dt = debitNoteRepo.GetAllInvoices(currentVendorLedgerId, currentBranchId);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            radioBtnAllDocument.Checked = true;
                        }
                    }
                }
                else
                {
                    dt = debitNoteRepo.GetAllInvoices(currentVendorLedgerId, currentBranchId);
                }

                // If a purchase return is loaded and its source invoice is not in the list, force-append it!
                if (dt != null && !string.IsNullOrEmpty(_invoiceNo) && !IsPurchaseReturnWithoutInvoice() && currentVendorLedgerId > 0)
                {
                    string normalizedInvoiceNo = _invoiceNo.Trim();
                    if (normalizedInvoiceNo.StartsWith("GRN-", StringComparison.OrdinalIgnoreCase))
                    {
                        normalizedInvoiceNo = normalizedInvoiceNo.Substring(4).Trim();
                    }

                    bool found = false;
                    foreach (DataRow row in dt.Rows)
                    {
                        string billNoStr = row["BillNo"].ToString().Trim();
                        if (billNoStr.StartsWith("GRN-", StringComparison.OrdinalIgnoreCase))
                        {
                            billNoStr = billNoStr.Substring(4).Trim();
                        }

                        if (billNoStr.Equals(normalizedInvoiceNo, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        DataRow sourceInvoiceRow = debitNoteRepo.GetInvoiceByPurchaseNo(_invoiceNo, currentVendorLedgerId, currentBranchId);
                        if (sourceInvoiceRow != null)
                        {
                            if (dt.Columns.Count == 0)
                            {
                                dt.Columns.Add("BillNo", typeof(string));
                                dt.Columns.Add("BillDate", typeof(DateTime));
                                dt.Columns.Add("DueDate", typeof(DateTime));
                                dt.Columns.Add("InvoiceAmount", typeof(decimal));
                                dt.Columns.Add("PaidAmount", typeof(decimal));
                                dt.Columns.Add("ReturnedAmount", typeof(decimal));
                                dt.Columns.Add("Balance", typeof(decimal));
                            }

                            DataRow newRow = dt.NewRow();
                            newRow["BillNo"] = sourceInvoiceRow["BillNo"];
                            newRow["BillDate"] = sourceInvoiceRow["BillDate"];
                            newRow["DueDate"] = sourceInvoiceRow["DueDate"];
                            newRow["InvoiceAmount"] = sourceInvoiceRow["InvoiceAmount"];
                            newRow["PaidAmount"] = sourceInvoiceRow["PaidAmount"];
                            newRow["ReturnedAmount"] = sourceInvoiceRow.Table.Columns.Contains("ReturnedAmount") ? sourceInvoiceRow["ReturnedAmount"] : 0m;
                            newRow["Balance"] = sourceInvoiceRow["Balance"];
                            dt.Rows.Add(newRow);
                        }
                    }
                }

                if (dt == null)
                {
                    dt = CreateEmptyInvoiceTable();
                }

                // Add additional columns for UI
                if (!dt.Columns.Contains("ReturnedAmount"))
                    dt.Columns.Add("ReturnedAmount", typeof(decimal));
                if (!dt.Columns.Contains("Select"))
                    dt.Columns.Add("Select", typeof(bool));
                if (!dt.Columns.Contains("Debit Amount"))
                    dt.Columns.Add("Debit Amount", typeof(decimal));
                if (!dt.Columns.Contains("SelectionOrder"))
                    dt.Columns.Add("SelectionOrder", typeof(int));
                if (!dt.Columns.Contains("OriginalBalance"))
                    dt.Columns.Add("OriginalBalance", typeof(decimal));

                // Initialize values and recalculate balance: InvoiceAmount - PaidAmount - ReturnedAmount
                foreach (DataRow row in dt.Rows)
                {
                    row["Select"] = false;
                    row["Debit Amount"] = 0m;

                    decimal invoiceAmount = dt.Columns.Contains("InvoiceAmount") && row["InvoiceAmount"] != DBNull.Value ? Convert.ToDecimal(row["InvoiceAmount"]) : 0m;
                    decimal paidAmount = dt.Columns.Contains("PaidAmount") && row["PaidAmount"] != DBNull.Value ? Convert.ToDecimal(row["PaidAmount"]) : 0m;
                    decimal returnedAmount = dt.Columns.Contains("ReturnedAmount") && row["ReturnedAmount"] != DBNull.Value ? Convert.ToDecimal(row["ReturnedAmount"]) : 0m;

                    row["ReturnedAmount"] = returnedAmount;

                    // Clamp and calculate actual balance (PaidAmount already includes settled amounts)
                    decimal balance = invoiceAmount - paidAmount;
                    if (balance < 0) balance = 0;

                    row["Balance"] = balance;
                    row["OriginalBalance"] = balance;
                }

                // Filter out invoices with Balance <= 0 if viewing outstanding only
                if (rdbtnoutstanding.Checked)
                {
                    var rows = dt.AsEnumerable()
                        .Where(row => {
                            decimal balance = GetSafeDecimal(row["Balance"]);
                             bool isSourceInvoice = false;
                             if (!string.IsNullOrEmpty(_invoiceNo))
                             {
                                 string normalizedSrcInv = _invoiceNo.Trim();
                                 if (normalizedSrcInv.StartsWith("GRN-", StringComparison.OrdinalIgnoreCase))
                                     normalizedSrcInv = normalizedSrcInv.Substring(4).Trim();

                                 string rowBillNo = row["BillNo"].ToString().Trim();
                                 if (rowBillNo.StartsWith("GRN-", StringComparison.OrdinalIgnoreCase))
                                     rowBillNo = rowBillNo.Substring(4).Trim();

                                 isSourceInvoice = rowBillNo.Equals(normalizedSrcInv, StringComparison.OrdinalIgnoreCase);
                             }
                             return balance > 0 || isSourceInvoice;
                        })
                        .ToList();

                    if (rows.Count > 0)
                        dt = rows.CopyToDataTable();
                    else
                        dt = dt.Clone();
                }

                ultraGrid1.DataSource = dt;
                ConfigureGridColumns();

                // Auto-distribute if amount is entered
                if (totalDebitAmount > 0 && !IsPurchaseReturnWithoutInvoice())
                {
                    DistributeDebitAmounts();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading invoices: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGrid()
        {
            ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            ultraGrid1.DisplayLayout.Override.SelectTypeCell = SelectType.Single;
            ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
        }

        private void ConfigureGridColumns()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;

            var band = ultraGrid1.DisplayLayout.Bands[0];

            // Modern grid appearance (match Payment/Receipt)
            ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

            ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(0, 122, 204);
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(0, 102, 184);
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            ultraGrid1.DisplayLayout.Override.CellAppearance.Reset();
            ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.Reset();
            ultraGrid1.DisplayLayout.Override.SelectedCellAppearance.Reset();
            ultraGrid1.DisplayLayout.Override.RowAppearance.Reset();
            ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Reset();
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Reset();

            ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(240, 248, 255);
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.LightSkyBlue;
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.Black;
            ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.BackColor = Color.Empty;
            ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.ForeColor = Color.Black;
            ultraGrid1.DisplayLayout.Override.CellPadding = 4;
            ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;

            ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
            ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
            ultraGrid1.DisplayLayout.InterBandSpacing = 10;
            ultraGrid1.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;

            if (band.Columns.Exists("BillNo"))
            {
                band.Columns["BillNo"].Header.Caption = "Purchase No";
                band.Columns["BillNo"].Width = 100;
                band.Columns["BillNo"].CellActivation = Activation.NoEdit;
            }

            if (band.Columns.Exists("BillDate"))
            {
                band.Columns["BillDate"].Header.Caption = "Purchase Date";
                band.Columns["BillDate"].Width = 100;
                band.Columns["BillDate"].CellActivation = Activation.NoEdit;
                band.Columns["BillDate"].Format = "dd-MM-yyyy";
            }

            if (band.Columns.Exists("DueDate"))
            {
                band.Columns["DueDate"].Header.Caption = "Due Date";
                band.Columns["DueDate"].Width = 100;
                band.Columns["DueDate"].CellActivation = Activation.NoEdit;
                band.Columns["DueDate"].Format = "dd-MM-yyyy";
            }

            if (band.Columns.Exists("InvoiceAmount"))
            {
                band.Columns["InvoiceAmount"].Header.Caption = "Purchase Amount";
                band.Columns["InvoiceAmount"].Width = 120;
                band.Columns["InvoiceAmount"].Format = "##,##0.00";
                band.Columns["InvoiceAmount"].CellActivation = Activation.NoEdit;
            }

            if (band.Columns.Exists("PaidAmount"))
            {
                band.Columns["PaidAmount"].Header.Caption = "Paid Amount";
                band.Columns["PaidAmount"].Width = 120;
                band.Columns["PaidAmount"].Format = "##,##0.00";
                band.Columns["PaidAmount"].CellActivation = Activation.NoEdit;
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

            if (band.Columns.Exists("Select"))
            {
                band.Columns["Select"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                band.Columns["Select"].Width = 50;
                band.Columns["Select"].CellActivation = Activation.AllowEdit;
            }

            if (band.Columns.Exists("Debit Amount"))
            {
                band.Columns["Debit Amount"].Header.Caption = "Debit Amount";
                band.Columns["Debit Amount"].Width = 120;
                band.Columns["Debit Amount"].Format = "##,##0.00";
                band.Columns["Debit Amount"].CellActivation = Activation.AllowEdit;
            }

            if (band.Columns.Exists("SelectionOrder"))
            {
                band.Columns["SelectionOrder"].Hidden = true;
            }

            if (band.Columns.Exists("OriginalBalance"))
            {
                band.Columns["OriginalBalance"].Hidden = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateDebitNote())
                {
                    return;
                }

                // Create master record
                int purchaseReturnLedgerId = GetPurchaseReturnLedgerId();
                if (purchaseReturnLedgerId <= 0)
                {
                    MessageBox.Show("Purchase Return ledger not found. Please configure Purchase Return ledger in the system.",
                        "Ledger Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DebitNoteMaster master = new DebitNoteMaster
                {
                    CompanyId = Convert.ToInt32(DataBase.CompanyId),
                    BranchId = currentBranchId,
                    FinYearId = SessionContext.FinYearId,
                    VoucherDate = (DateTime)dtpPurchaseDate.Value,
                    VendorLedgerId = currentVendorLedgerId,
                    PReturnNo = _pReturnNo,
                    InvoiceNo = _invoiceNo,
                    DebitAmount = (double)totalDebitAmount,
                    PaymentMethodLedgerId = purchaseReturnLedgerId,
                    Narration = richTextBox2.Text,
                    UserId = Convert.ToInt32(DataBase.UserId)
                };

                // Create detail records from grid
                List<DebitNoteDetails> details = new List<DebitNoteDetails>();
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("Debit Amount") && GetSafeDecimal(row.Cells["Debit Amount"].Value) > 0)
                    {
                        var detail = new DebitNoteDetails
                        {
                            BranchId = currentBranchId,
                            FinYearId = SessionContext.FinYearId,
                            BillNo = GetSafeInt(row.Cells["BillNo"].Value),
                            BillDate = row.Cells.Exists("BillDate") && row.Cells["BillDate"].Value != DBNull.Value
                                ? Convert.ToDateTime(row.Cells["BillDate"].Value)
                                : DateTime.Now,
                            BillAmount = row.Cells.Exists("InvoiceAmount") ? GetSafeDouble(row.Cells["InvoiceAmount"].Value) : 0,
                            OldBillAmount = row.Cells.Exists("InvoiceAmount") ? GetSafeDouble(row.Cells["InvoiceAmount"].Value) : 0,
                            DebitAmount = GetSafeDouble(row.Cells["Debit Amount"].Value),
                            BalanceAmount = row.Cells.Exists("Balance") ? GetSafeDouble(row.Cells["Balance"].Value) : 0
                        };
                        details.Add(detail);
                    }
                }

                if (!details.Any() && !IsPurchaseReturnWithoutInvoice())
                {
                    MessageBox.Show("Please allocate debit amount to at least one invoice.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Purchase Return is pending stock movement only; Debit Note is the accounting posting point.
                bool skipVoucher = false;
                bool success = debitNoteRepo.SaveDebitNote(master, details, skipVoucher);

                if (success)
                {
                    MessageBox.Show("Debit Note saved successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Failed to save Debit Note.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving debit note: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetPurchaseReturnLedgerId()
        {
            try
            {
                var ledgerRepo = new Repository.MasterRepositry.LedgerRepository();
                return ledgerRepo.GetLedgerId(DefaultLedgers.PURCHASERETURN, (int)AccountGroup.PURCHASE_ACCOUNT, currentBranchId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting Purchase Return ledger ID: {ex.Message}");
                return 0;
            }
        }

        private bool IsPurchaseReturnWithoutInvoice()
        {
            if (_pReturnNo <= 0)
            {
                return false;
            }

            string invoiceNo = (_invoiceNo ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(invoiceNo))
            {
                return true;
            }

            string normalized = invoiceNo.ToUpperInvariant();
            return normalized == "WITHOUT GR" || normalized == "WITHOUT BILL" || normalized == "WITHOUT PURCHASE BILL";
        }

        private bool ValidateDebitNote()
        {
            if (currentVendorLedgerId <= 0)
            {
                MessageBox.Show("Please select a vendor.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (totalDebitAmount <= 0)
            {
                MessageBox.Show("Please enter a valid debit amount.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            decimal totalApplied = GetTotalDebitAmount();
            if (totalApplied <= 0)
            {
                if (IsPurchaseReturnWithoutInvoice())
                {
                    return true;
                }

                MessageBox.Show("Please allocate debit amount to invoices.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (totalApplied > totalDebitAmount)
            {
                MessageBox.Show($"Total applied debit ({totalApplied:N2}) exceeds the debit amount ({totalDebitAmount:N2}).\n\nPlease adjust the values before saving.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (totalApplied < totalDebitAmount)
            {
                var result = MessageBox.Show(
                    $"You have an unapplied amount of {(totalDebitAmount - totalApplied):N2}.\n\nThis amount will be debited to the vendor account but not linked to any specific invoice.\n\nDo you want to proceed?",
                    "Confirm Unapplied Debit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    return false;
                }
            }

            return true;
        }

        private void DistributeDebitAmounts()
        {
            isAdjusting = true;

            // Reset all debit amounts first
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells.Exists("Debit Amount"))
                {
                    row.Cells["Debit Amount"].Value = 0m;
                }
                if (row.Cells.Exists("Select"))
                {
                    row.Cells["Select"].Value = false;
                }
            }

            selectionOrderCounter = 0;
            decimal remaining = totalDebitAmount;

            // Auto-select and allocate to invoices
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (remaining <= 0) break;

                decimal originalBalance = row.Cells.Exists("OriginalBalance")
                    ? GetSafeDecimal(row.Cells["OriginalBalance"].Value) : 0m;

                if (originalBalance > 0)
                {
                    decimal adjusted = Math.Min(originalBalance, remaining);
                    if (row.Cells.Exists("Debit Amount"))
                    {
                        row.Cells["Debit Amount"].Value = adjusted;
                    }
                    if (row.Cells.Exists("Select"))
                    {
                        row.Cells["Select"].Value = true;
                    }
                    if (row.Cells.Exists("SelectionOrder"))
                    {
                        selectionOrderCounter++;
                        row.Cells["SelectionOrder"].Value = selectionOrderCounter;
                    }
                    remaining -= adjusted;
                }
                else
                {
                    if (row.Cells.Exists("Debit Amount"))
                    {
                        row.Cells["Debit Amount"].Value = remaining;
                    }
                    if (row.Cells.Exists("Select"))
                    {
                        row.Cells["Select"].Value = true;
                    }
                    if (row.Cells.Exists("SelectionOrder"))
                    {
                        selectionOrderCounter++;
                        row.Cells["SelectionOrder"].Value = selectionOrderCounter;
                    }
                    remaining = 0;
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

        private decimal GetTotalDebitAmount()
        {
            decimal total = 0;
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells.Exists("Debit Amount"))
                {
                    total += GetSafeDecimal(row.Cells["Debit Amount"].Value);
                }
            }
            return total;
        }

        private void UpdateRemainingAmount()
        {
            decimal totalApplied = GetTotalDebitAmount();
            decimal remaining = totalDebitAmount - totalApplied;
            ultraTextEditor1.Text = remaining.ToString("N2");
        }

        private void ClearForm()
        {
            currentDebitNoteId = 0;
            currentVendorLedgerId = 0;
            _pReturnNo = 0;
            _invoiceNo = "";
            totalDebitAmount = 0;
            selectionOrderCounter = 0;

            txtPurchaseNo.Text = "";
            textBox4.Text = "";
            txtVendorName.Text = "";
            txtOutstanding.Text = "";
            textBox1.Text = "";
            richTextBox2.Text = "";
            ultraTextEditor1.Text = "0.00";
            dtpPurchaseDate.Value = DateTime.Now;

            ultraGrid1.DataSource = CreateEmptyInvoiceTable();
        }

        private DataTable CreateEmptyInvoiceTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("BillNo", typeof(string));
            dt.Columns.Add("BillDate", typeof(DateTime));
            dt.Columns.Add("DueDate", typeof(DateTime));
            dt.Columns.Add("InvoiceAmount", typeof(decimal));
            dt.Columns.Add("PaidAmount", typeof(decimal));
            dt.Columns.Add("ReturnedAmount", typeof(decimal));
            dt.Columns.Add("Balance", typeof(decimal));
            dt.Columns.Add("Select", typeof(bool));
            dt.Columns.Add("Debit Amount", typeof(decimal));
            dt.Columns.Add("SelectionOrder", typeof(int));
            dt.Columns.Add("OriginalBalance", typeof(decimal));
            return dt;
        }

        private void UltraGrid1_BeforeCellUpdate(object sender, BeforeCellUpdateEventArgs e)
        {
            if (e.Cell.Column.Key == "Debit Amount")
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

                decimal originalBalance = e.Cell.Row.Cells.Exists("OriginalBalance")
                    ? GetSafeDecimal(e.Cell.Row.Cells["OriginalBalance"].Value)
                    : 0m;

                if (newAmount > originalBalance)
                {
                    MessageBox.Show($"Debit amount cannot be greater than the outstanding balance ({originalBalance:N2})!", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                    return;
                }

                decimal totalAdjusted = GetTotalDebitAmount() - GetSafeDecimal(e.Cell.Row.Cells["Debit Amount"].Value);
                if (totalAdjusted + newAmount > totalDebitAmount)
                {
                    MessageBox.Show("Total adjusted amount cannot exceed debit amount!", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
            }
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
            if (!row.Cells.Exists("Debit Amount") || !row.Cells.Exists("Balance"))
                return;

            decimal debitAmount = GetSafeDecimal(row.Cells["Debit Amount"].Value);
            decimal originalBalance = row.Cells.Exists("OriginalBalance")
                ? GetSafeDecimal(row.Cells["OriginalBalance"].Value)
                : 0m;

            if (originalBalance > 0 && debitAmount > originalBalance)
            {
                isAdjusting = true;
                debitAmount = originalBalance;
                row.Cells["Debit Amount"].Value = debitAmount;
                isAdjusting = false;
            }

            row.Cells["Balance"].Value = originalBalance - debitAmount;
        }

        #region Event Handlers

        private void FrmDebitNote_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                btnSave_Click(ultraPictureBox1, EventArgs.Empty);
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

        private void btnF11_Click(object sender, EventArgs e)
        {
            // Open vendor selection dialog
            using (var vendorDialog = new DialogBox.frmVendorDig())
            {
                vendorDialog.Owner = this;
                if (vendorDialog.ShowDialog() == DialogResult.OK && vendorDialog.SelectedVendorId > 0)
                {
                    SetVendorInfo(vendorDialog.SelectedVendorId, vendorDialog.SelectedVendorName);
                }
            }
            ultraGrid1.Focus();
            if (ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.Rows[0].Activated = true;
                ultraGrid1.Rows[0].Selected = true;
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseFormFromTab();
        }

        private void txtDebitAmount_TextChanged(object sender, EventArgs e)
        {
            if (isLoadingData) return;  // Ignore event during programmatic loads

            if (decimal.TryParse(textBox1.Text,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.CurrentCulture,
                    out decimal amount))
            {
                totalDebitAmount = amount;
            }
            else
            {
                totalDebitAmount = 0m;
            }

            if (ultraGrid1.Rows.Count > 0 && !IsPurchaseReturnWithoutInvoice())
            {
                DistributeDebitAmounts();
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
                    var vendorList = ObjDropd.VendorDDL().List;
                    var vendor = vendorList.FirstOrDefault(v => v.LedgerID == ledgerId);
                    if (vendor != null)
                    {
                        SetVendorInfo(vendor.LedgerID, vendor.LedgerName);
                    }
                    else
                    {
                        MessageBox.Show("Vendor not found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid numeric vendor ID.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void rdbtnOutstanding_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbtnoutstanding.Checked && currentVendorLedgerId > 0)
            {
                LoadVendorInvoices();
            }
        }

        private void radioBtnAllDocument_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtnAllDocument.Checked && currentVendorLedgerId > 0)
            {
                LoadVendorInvoices();
            }
        }

        private void ultraGrid1_AfterCellUpdate(object sender, CellEventArgs e)
        {
            if (isAdjusting) return;

            if (e.Cell.Column.Key == "Select")
            {
                bool isSelected = Convert.ToBoolean(e.Cell.Value);
                if (isSelected)
                {
                    selectionOrderCounter++;
                    if (e.Cell.Row.Cells.Exists("SelectionOrder"))
                    {
                        e.Cell.Row.Cells["SelectionOrder"].Value = selectionOrderCounter;
                    }
                }
                else
                {
                    if (e.Cell.Row.Cells.Exists("SelectionOrder"))
                    {
                        e.Cell.Row.Cells["SelectionOrder"].Value = DBNull.Value;
                    }
                    if (e.Cell.Row.Cells.Exists("Debit Amount"))
                    {
                        e.Cell.Row.Cells["Debit Amount"].Value = 0m;
                    }
                    ResetSelectionOrder();
                }

                if (totalDebitAmount > 0)
                {
                    DistributeDebitAmountsToSelected();
                }
            }
            else if (e.Cell.Column.Key == "Debit Amount")
            {
                UpdateBalances();
                UpdateRemainingAmount();
            }
        }

        private void ultraGrid1_CellChange(object sender, CellEventArgs e)
        {
            if (e.Cell.Column.Key == "Select")
            {
                ultraGrid1.UpdateData();
            }
        }

        private void ResetSelectionOrder()
        {
            selectionOrderCounter = 0;

            var selectedRows = ultraGrid1.Rows
                .Where(row => row.Cells.Exists("Select") &&
                              row.Cells["Select"].Value != null &&
                              Convert.ToBoolean(row.Cells["Select"].Value))
                .Where(row => row.Cells.Exists("SelectionOrder") &&
                              row.Cells["SelectionOrder"].Value != null &&
                              row.Cells["SelectionOrder"].Value != DBNull.Value)
                .OrderBy(row => Convert.ToInt32(row.Cells["SelectionOrder"].Value))
                .ToList();

            foreach (var row in selectedRows)
            {
                selectionOrderCounter++;
                row.Cells["SelectionOrder"].Value = selectionOrderCounter;
            }
        }

        private void DistributeDebitAmountsToSelected()
        {
            isAdjusting = true;

            // Reset all debit amounts first
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells.Exists("Debit Amount"))
                {
                    row.Cells["Debit Amount"].Value = 0m;
                }
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

            decimal remaining = totalDebitAmount;

            foreach (UltraGridRow row in selectedRows)
            {
                if (remaining <= 0) break;

                decimal originalBalance = row.Cells.Exists("OriginalBalance")
                    ? GetSafeDecimal(row.Cells["OriginalBalance"].Value) : 0m;

                if (originalBalance > 0)
                {
                    decimal adjusted = Math.Min(originalBalance, remaining);
                    if (row.Cells.Exists("Debit Amount"))
                    {
                        row.Cells["Debit Amount"].Value = adjusted;
                    }
                    remaining -= adjusted;
                }
                else
                {
                    if (row.Cells.Exists("Debit Amount"))
                    {
                        row.Cells["Debit Amount"].Value = remaining;
                    }
                    remaining = 0;
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


        private void btnViewDebitNote_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new frmDebitNoteList())
                {
                    dlg.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing debit note list: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearchPurchaseReturn_Click(object sender, EventArgs e)
        {
            try
            {
                // Open Purchase Return Lookup, passing the current Debit Note ID to exclude its own adjustments
                using (var dlg = new frmPurchaseReturnLookup(currentVendorLedgerId, currentDebitNoteId))
                {
                    dlg.OnPurchaseReturnSelected += (pReturnNo, ledgerId, vendorName, invoiceNo, grandTotal) =>
                    {
                        LoadPurchaseReturnData(pReturnNo, ledgerId, vendorName, invoiceNo, grandTotal);
                    };

                    dlg.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening purchase return lookup: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPurchaseReturnData(int pReturnNo, int ledgerId, string vendorName, string invoiceNo, double grandTotal)
        {
            try
            {
                // Set internal tracking fields FIRST so LoadVendorInvoices
                // has the correct _invoiceNo and totalDebitAmount when it runs.
                _pReturnNo = pReturnNo;
                _invoiceNo = invoiceNo;

                // Set debit amount BEFORE SetVendorInfo so that when
                // LoadVendorInvoices is called inside SetVendorInfo,
                // DistributeDebitAmounts fires with the correct amount.
                // Use isLoadingData to prevent the TextChanged event from
                // interfering with the totalDebitAmount we're setting.
                isLoadingData = true;
                textBox1.Text = grandTotal.ToString("N2");
                isLoadingData = false;
                totalDebitAmount = (decimal)grandTotal;

                // Set vendor information (calls LoadVendorInvoices internally)
                SetVendorInfo(ledgerId, vendorName);

                // Show info message
                MessageBox.Show($"Purchase Return PR{pReturnNo} loaded.\n" +
                   $"Vendor: {vendorName}\nReturn Amount: {grandTotal:N2}\n\n" +
                   "Please select invoices to apply this debit to.",
                   "Purchase Return Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading purchase return data: {ex.Message}", "Error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadDebitNote_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new DialogBox.frmDebitNoteList())
                {
                    form.OnDebitNoteSelected += (voucherId) =>
                    {
                        try
                        {
                            DataSet ds = debitNoteRepo.GetDebitNoteById(voucherId, currentBranchId);
                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                LoadDebitNoteData(ds);
                            }
                            else
                            {
                                MessageBox.Show("Debit Note not found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error loading Debit Note details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Debit Note list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDebitNoteData(DataSet ds)
        {
            DataRow masterRow = ds.Tables[0].Rows[0];

            currentDebitNoteId = Convert.ToInt32(masterRow["Id"]);
            currentVendorLedgerId = Convert.ToInt32(masterRow["VendorLedgerId"]);
            _pReturnNo = Convert.ToInt32(masterRow["PReturnNo"] ?? 0);
            _invoiceNo = masterRow["InvoiceNo"]?.ToString();

            txtPurchaseNo.Text = masterRow["VoucherId"].ToString();
            textBox4.Text = currentVendorLedgerId.ToString();
            dtpPurchaseDate.Value = Convert.ToDateTime(masterRow["VoucherDate"]);
            textBox1.Text = Convert.ToDouble(masterRow["DebitAmount"]).ToString("N2");
            richTextBox2.Text = masterRow["Narration"]?.ToString() ?? "";

            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
            {
                DataTable dtDetails = ds.Tables[1];

                // Map columns to match the standard grid layout
                if (!dtDetails.Columns.Contains("InvoiceAmount") && dtDetails.Columns.Contains("BillAmount"))
                {
                    dtDetails.Columns.Add("InvoiceAmount", typeof(decimal));
                }
                if (!dtDetails.Columns.Contains("Debit Amount") && dtDetails.Columns.Contains("DebitAmount"))
                {
                    dtDetails.Columns.Add("Debit Amount", typeof(decimal));
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
                    decimal debitAmt = row.Table.Columns.Contains("DebitAmount") ? GetSafeDecimal(row["DebitAmount"]) : 0m;
                    decimal balAmt = row.Table.Columns.Contains("BalanceAmount") ? GetSafeDecimal(row["BalanceAmount"]) : 0m;

                    if (row.Table.Columns.Contains("InvoiceAmount"))
                        row["InvoiceAmount"] = invoiceAmt;

                    if (row.Table.Columns.Contains("Debit Amount"))
                        row["Debit Amount"] = debitAmt;

                    if (row.Table.Columns.Contains("Balance"))
                        row["Balance"] = balAmt;

                    // Since it was saved, the original balance before this debit was (BalanceAmount + DebitAmount)
                    row["OriginalBalance"] = balAmt + debitAmt;
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
            if (currentDebitNoteId <= 0)
            {
                MessageBox.Show("No saved Debit Note is loaded to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this Debit Note?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (debitNoteRepo.DeleteDebitNote(currentDebitNoteId))
                    {
                        MessageBox.Show("Debit Note deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete Debit Note.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting Debit Note: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion
        private void btnCreatePurchaseReturn_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentVendorLedgerId <= 0)
                {
                    MessageBox.Show("Please select a vendor first.", "No Vendor Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create Purchase Return form
                var prForm = new PosBranch_Win.Transaction.frmPurchaseReturn();

                // Open in tab
                OpenPurchaseReturnInTab(prForm, "Purchase Return - " + txtVendorName.Text);

                // Pre-fill vendor data
                // We need to use Find for controls since they might be private
                var txtVendor = prForm.Controls.Find("VendorName", true).FirstOrDefault() as TextBox;
                if (txtVendor != null)
                {
                    txtVendor.Text = txtVendorName.Text;
                }

                var lblVendorId = prForm.Controls.Find("vendorid", true).FirstOrDefault() as Infragistics.Win.Misc.UltraLabel;
                if (lblVendorId != null)
                {
                    lblVendorId.Text = currentVendorLedgerId.ToString();
                }

                // Ensure pbxSave is visible (as done in button2_Click in frmPurchaseReturn)
                var pbxSave = prForm.Controls.Find("pbxSave", true).FirstOrDefault();
                if (pbxSave != null) pbxSave.Visible = true;

                var ultraPictureBox4 = prForm.Controls.Find("ultraPictureBox4", true).FirstOrDefault();
                if (ultraPictureBox4 != null) ultraPictureBox4.Visible = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Purchase Return: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenPurchaseReturnInTab(Form form, string tabName)
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
                        openFormInTabMethod.Invoke(homeForm, new object[] { form, tabName });
                        return;
                    }
                }

                form.Show();
                form.BringToFront();
            }
            catch (Exception ex)
            {
                form.Show();
                form.BringToFront();
                System.Diagnostics.Debug.WriteLine($"Error opening in tab: {ex.Message}");
            }
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
