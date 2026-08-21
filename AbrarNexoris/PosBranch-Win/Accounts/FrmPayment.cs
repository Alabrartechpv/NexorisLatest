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
using PosBranch_Win.DialogBox;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win;
using ModelClass.Accounts;
using Repository.Accounts;
using PosBranch_Win.Transaction;
using Repository.TransactionRepository;

namespace PosBranch_Win.Accounts
{
    public partial class FrmPayment : Form
    {
        Dropdowns ObjDrop = new Dropdowns();
        private decimal totalPaymentAmount = 0;
        private bool isAdjusting = false;
        private VendorPaymentRepository paymentRepo;
        private int currentVendorLedgerId = 0;
        private int currentCompanyId = ModelClass.SessionContext.CompanyId;
        private int currentBranchId = ModelClass.SessionContext.BranchId; // Use SessionContext instead of hardcoded value
        private int currentUserId = ModelClass.SessionContext.UserId; // Use SessionContext instead of hardcoded value
        private int currentPaymentMasterId = 0;
        private const string PaymentStatusActive  = "Active";
        private const string PaymentStatusCancel  = "Cancel";
        private const string MsgSelectVendorFirst  = "Please select a vendor first before cancelling GRN payments.";
        private const string MsgSelectVendorTitle  = "Vendor Required";
        private const string GrnCancelledPrefix    = "Cancelled: ";
        private const string CashPayModeName       = "cash";   // lowercase for comparison

        // Remembers the user's chosen payment mode across ClearForm calls
        private object _userSelectedPayModeId = null;

        public FrmPayment()
        {
            InitializeComponent();
            this.KeyPreview = true; // Enable form-level key events
            this.KeyDown += FrmPayment_KeyDown;
            this.Load += FrmPayment_Load; // Ensure Load event is wired
            ultraGrid1.InitializeLayout += UltraGrid1_InitializeLayout; // Ensure header captions always set
            ConfigureGrid();
            InitializeEventHandlers();
            paymentRepo = new VendorPaymentRepository();
            LoadPaymentMethods();
            InitializePaymentStatusCombo();

            // Ensure outstanding is selected by default
            rdbtnoutstanding.Checked = true;

            // Wire up picture box click events
            ultraPictureBox1.Click += (s, e) => ClearForm();
            ultraPictureBox2.Click += (s, e) => CloseFormFromTab();
            this.ultraPictureBox7.Click  += ultraPictureBox7_Click;
            this.ultraPictureBox11.Click += ultraPictureBox11_Click;

            // Save user's payment method choice so it persists after ClearForm
            CmboPayment.ValueChanged += (s, e) =>
            {
                if (CmboPayment.Value != null)
                    _userSelectedPayModeId = CmboPayment.Value;
            };
        }

        private void ultraPictureBox7_Click(object sender, EventArgs e)
        {
            using (var dlg = new PosBranch_Win.DialogBox.frmSalesPersonDial())
            {
                dlg.OnSalesPersonSelected += (name) =>
                {
                    textBox2.Text = name;
                };
                dlg.ShowDialog(this);
            }
        }

        private void ultraPictureBox11_Click(object sender, EventArgs e)
        {
            if (currentVendorLedgerId <= 0)
            {
                MessageBox.Show(MsgSelectVendorFirst, MsgSelectVendorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dlg = new PosBranch_Win.DialogBox.FrmGrnPaymentCancel(
                currentVendorLedgerId,
                txtVendorName.Text,
                currentBranchId,
                currentUserId,
                paymentRepo))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.CancelledGrnNumbers.Count > 0)
                {
                    // Load the cancelled GRN numbers into ultraTextEditor3
                    ultraTextEditor3.Text = GrnCancelledPrefix +
                        string.Join(", ", dlg.CancelledGrnNumbers);

                    // Refresh the vendor invoices grid to reflect updated balances
                    LoadVendorInvoices();
                }
            }
        }

        private void UltraGrid1_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];

            // Configure columns
            if (band.Columns.Exists("BillNo"))
            {
                band.Columns["BillNo"].Header.Caption = "Purchase No";
                band.Columns["BillNo"].Width = 100;
            }

            if (band.Columns.Exists("InvoiceNo"))
            {
                band.Columns["InvoiceNo"].Header.Caption = "Invoice No";
                band.Columns["InvoiceNo"].Width = 100;
            }

            if (band.Columns.Exists("InvoiceAmount"))
            {
                band.Columns["InvoiceAmount"].Header.Caption = "Invoice Amount";
                band.Columns["InvoiceAmount"].Width = 120;
                band.Columns["InvoiceAmount"].Format = "##,##0.00";
                band.Columns["InvoiceAmount"].CellActivation = Activation.NoEdit;
            }

            if (band.Columns.Exists("PayedAmount"))
            {
                band.Columns["PayedAmount"].Header.Caption = "Paid Amount";
                band.Columns["PayedAmount"].Width = 120;
                band.Columns["PayedAmount"].Format = "##,##0.00";
                band.Columns["PayedAmount"].CellActivation = Activation.NoEdit;
            }

            if (band.Columns.Exists("Balance"))
            {
                band.Columns["Balance"].Header.Caption = "Balance";
                band.Columns["Balance"].Width = 120;
                band.Columns["Balance"].Format = "##,##0.00";
                band.Columns["Balance"].CellActivation = Activation.NoEdit;
            }

            if (band.Columns.Exists("BillDate"))
            {
                band.Columns["BillDate"].Header.Caption = "Bill Date";
                band.Columns["BillDate"].Width = 100;
                band.Columns["BillDate"].CellActivation = Activation.NoEdit;
            }

            // Add checkbox column if not exists
            if (!band.Columns.Exists("Select"))
            {
                band.Columns.Add("Select", "Select");
                band.Columns["Select"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                band.Columns["Select"].Width = 50;
            }

            // Add Adjusted Amount column if not exists
            if (!band.Columns.Exists("AdjustedAmount"))
            {
                band.Columns.Add("AdjustedAmount", "Adjusted Amount");
                band.Columns["AdjustedAmount"].DataType = typeof(decimal);
                band.Columns["AdjustedAmount"].CellActivation = Activation.AllowEdit;
                band.Columns["AdjustedAmount"].Format = "##,##0.00";
            }

            if (band.Columns.Exists("Paymode")) band.Columns["Paymode"].Hidden = true;
            if (band.Columns.Exists("PayMode")) band.Columns["PayMode"].Hidden = true;
            if (band.Columns.Exists("PaymodeID")) band.Columns["PaymodeID"].Hidden = true;
            if (band.Columns.Exists("PayModeId")) band.Columns["PayModeId"].Hidden = true;
            if (band.Columns.Exists("PayModeID")) band.Columns["PayModeID"].Hidden = true;
            if (band.Columns.Exists("PaymodeName")) band.Columns["PaymodeName"].Hidden = true;

            if (radioBtnAllDocument != null && radioBtnAllDocument.Checked)
            {
                if (band.Columns.Exists("Select")) band.Columns["Select"].Hidden = true;
                if (band.Columns.Exists("AdjustedAmount")) band.Columns["AdjustedAmount"].Hidden = true;
            }
            else
            {
                if (band.Columns.Exists("Select")) band.Columns["Select"].Hidden = false;
                if (band.Columns.Exists("AdjustedAmount")) band.Columns["AdjustedAmount"].Hidden = false;
            }
        }

        private void LoadPaymentMethods()
        {
            // Load payment methods from database
            var paymentMethods = ObjDrop.GetPaymode();
            CmboPayment.DataSource = paymentMethods.List;
            CmboPayment.DisplayMember = "PayModeName";
            CmboPayment.ValueMember = "PayModeID";
            // Set default to 'Cash' if available; also seed _userSelectedPayModeId
            if (paymentMethods.List != null)
            {
                foreach (var item in paymentMethods.List)
                {
                    var payModeNameProp = item.GetType().GetProperty("PayModeName");
                    var payModeIdProp   = item.GetType().GetProperty("PayModeID");
                    if (payModeNameProp != null && payModeIdProp != null)
                    {
                        string payModeName = payModeNameProp.GetValue(item)?.ToString();
                        if (!string.IsNullOrEmpty(payModeName) &&
                            payModeName.Trim().ToLower() == CashPayModeName)
                        {
                            var payModeId = payModeIdProp.GetValue(item);
                            CmboPayment.Value       = payModeId;
                            _userSelectedPayModeId  = payModeId;   // seed default
                            break;
                        }
                    }
                }
            }
        }

        private void InitializePaymentStatusCombo()
        {
            ultraTextEditor2.Items.Clear();
            ultraTextEditor2.Items.Add(PaymentStatusActive, PaymentStatusActive);
            ultraTextEditor2.Items.Add(PaymentStatusCancel, PaymentStatusCancel);
            ultraTextEditor2.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            ultraTextEditor2.Value = PaymentStatusActive;
        }

        private void InitializeEventHandlers()
        {
            textBox1.TextChanged += TxtPaymentAmount_TextChanged;
            rdbtnoutstanding.CheckedChanged += RadioButton_CheckedChanged;
            radioBtnAllDocument.CheckedChanged += RadioButton_CheckedChanged;
            btnViewPayment.Click += btnViewPayment_Click;
            btnPurch.Click += btnPurch_Click;
            btnF11.Click += btnF11_Click;
            ultraPictureBox3.Click += UltraPictureBox3_Click; // Save (F8)
            ultraPictureBox4.Click += UltraPictureBox4_Click; // Update
            ultraPictureBox1.Click += UltraPictureBox1_Click; // Clear (F1)
            ultraPictureBox2.Click += UltraPictureBox2_Click; // Close (F4)
            // Grid event handlers
            ultraGrid1.AfterCellUpdate += UltraGrid1_AfterCellUpdate;
            ultraGrid1.BeforeCellUpdate += UltraGrid1_BeforeCellUpdate;
            ultraGrid1.CellChange += UltraGrid1_CellChange;
            // Add KeyDown event for textBox4 (vendor ID input)
            textBox4.KeyDown += textBox4_KeyDown;
            // Add click event for ultraPictureBox10
            ultraPictureBox10.Click += ultraPictureBox10_Click;
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
                    // Use Dropdowns to get vendor list
                    var vendorList = ObjDrop.VendorDDL().List;
                    var vendor = vendorList.FirstOrDefault(v => v.LedgerID == ledgerId);
                    if (vendor != null)
                    {
                        SetVendorInfo(vendor.LedgerID, vendor.LedgerName);
                    }
                    else
                    {
                        MessageBox.Show("No vendor found for this ID.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid numeric vendor ID.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ultraPictureBox10_Click(object sender, EventArgs e)
        {
            try
            {
                // Open the payment list history dialog using the OfficialReceiptList form in payment mode
                using (var paymentListForm = new PosBranch_Win.DialogBox.OfficialReceiptList(isPayment: true))
                {
                    if (paymentListForm.ShowDialog(this) == DialogResult.OK && paymentListForm.SelectedVoucherId > 0)
                    {
                        LoadPayment(paymentListForm.SelectedVoucherId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening payment list: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtPaymentAmount_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(textBox1.Text, out decimal amount))
            {
                totalPaymentAmount = amount;
            }
            else
            {
                totalPaymentAmount = 0m;
            }
            UpdateAdjustedAmounts();
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (currentVendorLedgerId > 0)
            {
                LoadVendorInvoices();
            }
        }

        private static decimal SafeToDecimal(object value, decimal defaultValue = 0m)
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            if (decimal.TryParse(value.ToString(), out decimal result)) return result;
            return defaultValue;
        }

        private static int SafeToInt(object value, int defaultValue = 0)
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            if (int.TryParse(value.ToString(), out int result)) return result;
            return defaultValue;
        }

        private static DateTime SafeToDateTime(object value)
        {
            if (value == null || value == DBNull.Value) return DateTime.Now;
            if (DateTime.TryParse(value.ToString(), out DateTime result)) return result;
            return DateTime.Now;
        }

        private void LoadVendorInvoices()
        {
            try
            {
                DataTable invoices;
                if (rdbtnoutstanding.Checked)
                {
                    invoices = paymentRepo.GetOutstandingInvoices(currentVendorLedgerId);
                    // Filter out invoices with Balance <= 0
                    if (invoices != null && invoices.Columns.Contains("Balance"))
                    {
                        var rows = invoices.AsEnumerable()
                            .Where(row => {
                                var val = row["Balance"];
                                decimal balance = SafeToDecimal(val);
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
                    invoices = paymentRepo.GetAllInvoices(currentVendorLedgerId);
                }

                ultraGrid1.DataSource = null;
                if (invoices != null && invoices.Rows.Count > 0)
                {
                    ultraGrid1.DataSource = NormalizeInvoiceTable(invoices);
                    ConfigureGridColumns();
                    // Set all checkboxes to false and AdjustedAmount to 0
                    foreach (UltraGridRow row in ultraGrid1.Rows)
                    {
                        if (row.Cells.Exists("Select")) row.Cells["Select"].Value = false;
                        if (row.Cells.Exists("AdjustedAmount")) row.Cells["AdjustedAmount"].Value = 0m;
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
                else
                {
                    MessageBox.Show("No invoices found for the selected vendor.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            // Modern grid appearance (copied/adapted from FrmReceipt)
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

            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

                // Configure columns
                if (band.Columns.Exists("BillNo"))
                {
                    band.Columns["BillNo"].Header.Caption = "Purchase No";
                    band.Columns["BillNo"].Width = 100;
                }

                if (band.Columns.Exists("InvoiceNo"))
                {
                    band.Columns["InvoiceNo"].Header.Caption = "Invoice No";
                    band.Columns["InvoiceNo"].Width = 100;
                }

                if (band.Columns.Exists("InvoiceAmount"))
                {
                    band.Columns["InvoiceAmount"].Header.Caption = "Invoice Amount";
                    band.Columns["InvoiceAmount"].Width = 120;
                    band.Columns["InvoiceAmount"].Format = "##,##0.00";
                    band.Columns["InvoiceAmount"].CellActivation = Activation.NoEdit;
                }

                if (band.Columns.Exists("PayedAmount"))
                {
                    band.Columns["PayedAmount"].Header.Caption = "Paid Amount";
                    band.Columns["PayedAmount"].Width = 120;
                    band.Columns["PayedAmount"].Format = "##,##0.00";
                    band.Columns["PayedAmount"].CellActivation = Activation.NoEdit;
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

                if (band.Columns.Exists("BillDate"))
                {
                    band.Columns["BillDate"].Header.Caption = "Purchase Date";
                    band.Columns["BillDate"].Width = 100;
                    band.Columns["BillDate"].CellActivation = Activation.NoEdit;
                }

                // Add checkbox column if not exists
                if (!band.Columns.Exists("Select"))
                {
                    band.Columns.Add("Select", "Select");
                    band.Columns["Select"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                    band.Columns["Select"].Width = 50;
                }

                // Add Adjusted Amount column if not exists
                if (!band.Columns.Exists("AdjustedAmount"))
                {
                    band.Columns.Add("AdjustedAmount", "Adjusted Amount");
                    band.Columns["AdjustedAmount"].DataType = typeof(decimal);
                    band.Columns["AdjustedAmount"].CellActivation = Activation.AllowEdit;
                    band.Columns["AdjustedAmount"].Format = "##,##0.00";
                }

                if (band.Columns.Exists("Paymode")) band.Columns["Paymode"].Hidden = true;
                if (band.Columns.Exists("PayMode")) band.Columns["PayMode"].Hidden = true;
                if (band.Columns.Exists("PaymodeID")) band.Columns["PaymodeID"].Hidden = true;
                if (band.Columns.Exists("PayModeId")) band.Columns["PayModeId"].Hidden = true;
                if (band.Columns.Exists("PayModeID")) band.Columns["PayModeID"].Hidden = true;
                if (band.Columns.Exists("PaymodeName")) band.Columns["PaymodeName"].Hidden = true;

                if (radioBtnAllDocument != null && radioBtnAllDocument.Checked)
                {
                    if (band.Columns.Exists("Select")) band.Columns["Select"].Hidden = true;
                    if (band.Columns.Exists("AdjustedAmount")) band.Columns["AdjustedAmount"].Hidden = true;
                }
                else
                {
                    if (band.Columns.Exists("Select")) band.Columns["Select"].Hidden = false;
                    if (band.Columns.Exists("AdjustedAmount")) band.Columns["AdjustedAmount"].Hidden = false;
                }
            }
        }

        public void SetVendorInfo(int ledgerId, string vendorName)
        {
            // Ensure repository is initialized
            if (paymentRepo == null)
                paymentRepo = new VendorPaymentRepository();

            currentVendorLedgerId = ledgerId;
            txtVendorName.Text = vendorName;
            textBox4.Text = ledgerId.ToString();

            // Get and display the total outstanding amount
            try
            {
                decimal outstandingTotal = paymentRepo.GetVendorOutstandingTotal(ledgerId);
                txtOutstanding.Text = outstandingTotal.ToString("N2");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching outstanding total: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtOutstanding.Text = "0.00";
            }

            LoadVendorInvoices();
        }

        public void SetPaymentAmount(double amount)
        {
            // Set the total payment amount to textBox1
            if (textBox1 != null)
            {
                textBox1.Text = amount.ToString("N2");
                // Trigger the text changed event to update adjusted amounts
                if (decimal.TryParse(textBox1.Text, out decimal paymentAmount))
                {
                    totalPaymentAmount = paymentAmount;
                    UpdateAdjustedAmounts();
                }
            }
        }

        private void ConfigureGrid()
        {
            ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
            if (!band.Columns.Exists("Select"))
            {
                band.Columns.Add("Select", "Select");
                band.Columns["Select"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                band.Columns["Select"].Width = 50;
            }
            if (band.Columns.Exists("InvoiceAmount"))
            {
                band.Columns["InvoiceAmount"].Header.Caption = "Invoice Amount";
                band.Columns["InvoiceAmount"].CellActivation = Activation.NoEdit;
            }
            if (!band.Columns.Exists("AdjustedAmount"))
            {
                band.Columns.Add("AdjustedAmount", "Adjusted Amount");
                band.Columns["AdjustedAmount"].DataType = typeof(decimal);
                band.Columns["AdjustedAmount"].CellActivation = Activation.AllowEdit;
                band.Columns["AdjustedAmount"].Format = "##,##0.00";
            }
            if (!band.Columns.Exists("Balance"))
            {
                band.Columns.Add("Balance", "Balance");
                band.Columns["Balance"].DataType = typeof(decimal);
                band.Columns["Balance"].CellActivation = Activation.NoEdit;
                band.Columns["Balance"].Format = "##,##0.00";
            }
            // Event handlers
            ultraGrid1.AfterCellUpdate += UltraGrid1_AfterCellUpdate;
            ultraGrid1.BeforeCellUpdate += UltraGrid1_BeforeCellUpdate;
            ultraGrid1.CellChange += UltraGrid1_CellChange;
        }

        private void UltraGrid1_BeforeCellUpdate(object sender, BeforeCellUpdateEventArgs e)
        {
            if (e.Cell.Column.Key == "AdjustedAmount")
            {
                if (!decimal.TryParse(e.NewValue?.ToString(), out decimal newAmount))
                {
                    e.Cancel = true;
                    return;
                }

                decimal invoiceAmount = SafeToDecimal(e.Cell.Row.Cells["InvoiceAmount"].Value);
                decimal paidAmount = SafeToDecimal(e.Cell.Row.Cells["PayedAmount"].Value);
                decimal originalBalance = invoiceAmount - paidAmount;

                // For payments, handle negative balances (vendor overpaid) differently
                if (originalBalance < 0)
                {
                    // If balance is negative, allow adjustment up to the absolute value
                    decimal maxAdjustment = Math.Abs(originalBalance);
                    if (newAmount > maxAdjustment)
                    {
                        MessageBox.Show($"Adjusted amount cannot be greater than the overpayment amount ({maxAdjustment:N2})!", "Warning",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                        return;
                    }
                }
                else
                {
                    // Normal case: positive balance
                    if (newAmount > originalBalance)
                    {
                        MessageBox.Show($"Adjusted amount cannot be greater than the outstanding balance ({originalBalance:N2})!", "Warning",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                        return;
                    }
                }

                decimal totalAdjusted = GetTotalAdjustedAmount() - GetCurrentAdjustedAmount(e.Cell.Row);
                if (totalAdjusted + newAmount > totalPaymentAmount)
                {
                    MessageBox.Show("Total adjusted amount cannot exceed payment amount!", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
            }
        }

        private void UltraGrid1_AfterCellUpdate(object sender, CellEventArgs e)
        {
            if (isAdjusting) return;
            if (e.Cell.Column.Key == "Select")
            {
                var row = e.Cell.Row;
                bool isSelected = Convert.ToBoolean(row.Cells["Select"].Value);
                // Remove SelectionOrder logic
                UpdateAdjustedAmounts();
            }
            else if (e.Cell.Column.Key == "AdjustedAmount")
            {
                UpdateBalances();
                UpdateRemainingAmount();
            }
        }

        private void UpdateAdjustedAmounts()
        {
            isAdjusting = true;
            
            // Reset all rows first
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                if (row.Cells.Exists("AdjustedAmount"))
                {
                    row.Cells["AdjustedAmount"].Value = 0m;
                }
                UpdateRowBalance(row);
            }

            decimal remainingAmount = totalPaymentAmount;
            // Get selected rows in selection order
            var selectedRows = ultraGrid1.Rows
                .Where(row => row.Cells.Exists("Select") &&
                              row.Cells["Select"].Value != null &&
                              Convert.ToBoolean(row.Cells["Select"].Value))
                .OrderBy(row => row.Index) // Use row.Index for simple top-to-bottom order
                .ToList();

            // Distribute amount to selected rows in order
            foreach (var row in selectedRows)
            {
                if (remainingAmount <= 0) break;
                decimal invoiceAmount = SafeToDecimal(row.Cells["InvoiceAmount"].Value);
                decimal paidAmount = SafeToDecimal(row.Cells["PayedAmount"].Value);
                decimal returnedAmount = row.Cells.Exists("ReturnedAmount") && row.Cells["ReturnedAmount"].Value != null && row.Cells["ReturnedAmount"].Value != DBNull.Value 
                    ? SafeToDecimal(row.Cells["ReturnedAmount"].Value) 
                    : 0m;
                decimal originalBalance = invoiceAmount - paidAmount - returnedAmount;
                decimal adjustedAmount;
                // Handle negative balances (vendor overpaid) for payments
                if (originalBalance < 0)
                {
                    // For negative balance, we can pay back up to the absolute value
                    decimal maxAdjustment = Math.Abs(originalBalance);
                    adjustedAmount = Math.Min(remainingAmount, maxAdjustment);
                }
                else
                {
                    // Normal case: positive balance
                    adjustedAmount = Math.Min(remainingAmount, originalBalance);
                }
                remainingAmount -= adjustedAmount;
                row.Cells["AdjustedAmount"].Value = adjustedAmount;
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
            decimal invoiceAmount = SafeToDecimal(row.Cells["InvoiceAmount"].Value);
            decimal paidAmount = SafeToDecimal(row.Cells["PayedAmount"].Value);
            decimal returnedAmount = row.Cells.Exists("ReturnedAmount") && row.Cells["ReturnedAmount"].Value != null && row.Cells["ReturnedAmount"].Value != DBNull.Value 
                ? SafeToDecimal(row.Cells["ReturnedAmount"].Value) 
                : 0m;
            decimal adjustedAmount = SafeToDecimal(row.Cells["AdjustedAmount"].Value);

            // Calculate the new balance: Original outstanding balance - adjusted amount
            // Original outstanding balance = InvoiceAmount - PayedAmount - ReturnedAmount
            decimal originalOutstandingBalance = invoiceAmount - paidAmount - returnedAmount;
            decimal newBalance = originalOutstandingBalance - adjustedAmount;

            row.Cells["Balance"].Value = newBalance;
        }

        private decimal GetTotalAdjustedAmount()
        {
            return ultraGrid1.Rows.Sum(row => SafeToDecimal(row.Cells["AdjustedAmount"].Value));
        }

        private decimal GetCurrentAdjustedAmount(UltraGridRow row)
        {
            return SafeToDecimal(row.Cells["AdjustedAmount"].Value);
        }

        private void UpdateRemainingAmount()
        {
            decimal totalAdjusted = GetTotalAdjustedAmount();
            decimal remaining = totalPaymentAmount - totalAdjusted;
            ultraTextEditor1.Text = remaining.ToString("N2");
        }

        private DataTable CreateEmptyInvoiceTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("BillNo", typeof(string));
            dt.Columns.Add("InvoiceNo", typeof(string));
            dt.Columns.Add("InvoiceAmount", typeof(decimal));
            dt.Columns.Add("PayedAmount", typeof(decimal)); // Match the stored procedure column name
            dt.Columns.Add("ReturnedAmount", typeof(decimal));
            dt.Columns.Add("Balance", typeof(decimal));
            dt.Columns.Add("Select", typeof(bool));
            dt.Columns.Add("AdjustedAmount", typeof(decimal));
            dt.Columns.Add("BillDate", typeof(DateTime));
            return dt;
        }

        private DataTable NormalizeInvoiceTable(DataTable dt)
        {
            if (dt == null) return CreateEmptyInvoiceTable();

            // Ensure all columns exist and in the correct order
            var correctOrder = new[] { "BillNo", "InvoiceNo", "InvoiceAmount", "PayedAmount", "ReturnedAmount", "Balance", "Select", "AdjustedAmount", "BillDate" };

            // Create new table with correct structure
            var newDt = new DataTable();
            foreach (string colName in correctOrder)
            {
                if (dt.Columns.Contains(colName))
                {
                    newDt.Columns.Add(colName, dt.Columns[colName].DataType);
                }
                else if (colName == "Select")
                {
                    newDt.Columns.Add(colName, typeof(bool));
                }
                else if (colName == "AdjustedAmount")
                {
                    newDt.Columns.Add(colName, typeof(decimal));
                }
                else if (colName == "ReturnedAmount")
                {
                    newDt.Columns.Add(colName, typeof(decimal));
                }
            }

            // Copy data and recalculate balance
            foreach (DataRow row in dt.Rows)
            {
                var newRow = newDt.NewRow();
                foreach (string colName in correctOrder)
                {
                    if (dt.Columns.Contains(colName) && newDt.Columns.Contains(colName))
                    {
                        newRow[colName] = row[colName];
                    }
                }

                // Set default values for new columns
                if (newDt.Columns.Contains("Select"))
                    newRow["Select"] = false;
                if (newDt.Columns.Contains("AdjustedAmount"))
                    newRow["AdjustedAmount"] = 0m;
                if (newDt.Columns.Contains("ReturnedAmount") && newRow["ReturnedAmount"] == DBNull.Value)
                    newRow["ReturnedAmount"] = 0m;

                // Recalculate balance: InvoiceAmount - PayedAmount - ReturnedAmount
                if (newDt.Columns.Contains("InvoiceAmount") && newDt.Columns.Contains("PayedAmount") && newDt.Columns.Contains("Balance"))
                {
                    decimal invoiceAmount = SafeToDecimal(newRow["InvoiceAmount"]);
                    decimal paidAmount = SafeToDecimal(newRow["PayedAmount"]);
                    decimal returnedAmount = newDt.Columns.Contains("ReturnedAmount") && newRow["ReturnedAmount"] != DBNull.Value ? SafeToDecimal(newRow["ReturnedAmount"]) : 0m;

                    // Detect cash purchases where full payment was settled at purchase time
                    bool isCashPurchase = false;
                    if (dt.Columns.Contains("Paymode") && row["Paymode"] != DBNull.Value && row["Paymode"] != null)
                    {
                        string pm = row["Paymode"].ToString().Trim().ToLower();
                        if (pm == "cash" || pm == "2") isCashPurchase = true;
                    }
                    if (dt.Columns.Contains("PaymodeID") && row["PaymodeID"] != DBNull.Value && row["PaymodeID"] != null)
                    {
                        if (SafeToInt(row["PaymodeID"]) == 2) isCashPurchase = true;
                    }
                    if (dt.Columns.Contains("PayModeID") && row["PayModeID"] != DBNull.Value && row["PayModeID"] != null)
                    {
                        if (SafeToInt(row["PayModeID"]) == 2) isCashPurchase = true;
                    }

                    if (isCashPurchase)
                    {
                        paidAmount = invoiceAmount;
                    }

                    // Clamping logic to prevent over-allocation and negative balances (similar to FrmReceipt)
                    decimal maxPaidAndReturned = paidAmount + returnedAmount;
                    if (invoiceAmount > 0m && maxPaidAndReturned > invoiceAmount)
                    {
                        if (paidAmount > invoiceAmount)
                        {
                            paidAmount = invoiceAmount;
                            returnedAmount = 0m;
                        }
                        else
                        {
                            returnedAmount = invoiceAmount - paidAmount;
                        }
                    }

                    newRow["PayedAmount"] = paidAmount;
                    if (newDt.Columns.Contains("ReturnedAmount"))
                    {
                        newRow["ReturnedAmount"] = returnedAmount;
                    }
                    newRow["Balance"] = invoiceAmount - paidAmount - returnedAmount;
                }

                newDt.Rows.Add(newRow);
            }

            return newDt;
        }

        private void FrmPayment_Load(object sender, EventArgs e)
        {
            // Bind an empty table with all expected columns so grid style is visible on open
            ultraGrid1.DataSource = CreateEmptyInvoiceTable();
            ConfigureGridColumns();
            dtpPurchaseDate.Value = DateTime.Now;
        }

        private void btnF11_Click(object sender, EventArgs e)
        {
            using (var vendorDialog = new PosBranch_Win.DialogBox.frmVendorDig())
            {
                vendorDialog.Owner = this;
                if (vendorDialog.ShowDialog() == DialogResult.OK)
                {
                    int ledgerId = vendorDialog.SelectedVendorId;
                    string vendorName = vendorDialog.SelectedVendorName;
                    SetVendorInfo(ledgerId, vendorName);
                }
            }
            // After dialog closes, set focus to grid and select first row
            ultraGrid1.Focus();
            if (ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.Rows[0].Activated = true;
                ultraGrid1.Rows[0].Selected = true;
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
            }
        }

        private void UltraPictureBox3_Click(object sender, EventArgs e)
        {
            btnSave_Click(sender, e); // Save logic
        }

        private void UltraPictureBox4_Click(object sender, EventArgs e)
        {
            if (IsCancelStatusSelected())
            {
                CancelLoadedPayment();
                return;
            }

            MessageBox.Show("Please select Cancel status to cancel/reverse a loaded vendor payment.",
                "Payment Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UltraPictureBox1_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void UltraPictureBox2_Click(object sender, EventArgs e)
        {
            CloseFormFromTab();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsCancelStatusSelected())
                {
                    CancelLoadedPayment();
                    return;
                }

                if (!ValidatePayment())
                {
                    return;
                }

                // Generate a voucher number
                string voucherNo = GenerateVoucherNumber();
                int paymentMethodId = CmboPayment.Value != null ? Convert.ToInt32(CmboPayment.Value) : 1;
                DateTime voucherDate = Convert.ToDateTime(dtpPurchaseDate.Value).Date;
                int vendorLedgerId = currentVendorLedgerId > 0 ? currentVendorLedgerId : Convert.ToInt32(textBox4.Text);

                // Create payment master
                var master = new VendorPaymentMaster
                {
                    CompanyId = currentCompanyId,
                    VendorLedgerId = vendorLedgerId,
                    VendorName = txtVendorName.Text,
                    PaymentMethod = CmboPayment.Text,
                    PaymentMethodLedgerId = paymentMethodId,
                    SalesPerson = textBox2.Text,
                    TotalPaymentAmount = totalPaymentAmount,
                    OutstandingAmount = Convert.ToDecimal(ultraTextEditor1.Text),
                    PaymentDate = voucherDate,
                    VoucherNo = voucherNo,
                    Remarks = richTextBox2.Text,
                    BranchId = currentBranchId,
                    CreatedBy = currentUserId
                };

                // Create payment details
                var details = new List<VendorPaymentDetails>();
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    bool isSelected = row.Cells.Exists("Select") && row.Cells["Select"].Value != null && Convert.ToBoolean(row.Cells["Select"].Value);
                    decimal adjustedAmt = row.Cells.Exists("AdjustedAmount") ? SafeToDecimal(row.Cells["AdjustedAmount"].Value) : 0m;

                    if (isSelected && adjustedAmt > 0)
                    {
                        var detail = new VendorPaymentDetails
                        {
                            BillNo = row.Cells.Exists("BillNo") ? (row.Cells["BillNo"].Value?.ToString() ?? "0") : "0",
                            InvoiceAmount = row.Cells.Exists("InvoiceAmount") ? SafeToDecimal(row.Cells["InvoiceAmount"].Value) : 0m,
                            AdjustedAmount = adjustedAmt,
                            Balance = row.Cells.Exists("Balance") ? SafeToDecimal(row.Cells["Balance"].Value) : 0m,
                            BillDate = row.Cells.Exists("BillDate") && row.Cells["BillDate"].Value != null && row.Cells["BillDate"].Value != DBNull.Value
                                ? SafeToDateTime(row.Cells["BillDate"].Value)
                                : Convert.ToDateTime(dtpPurchaseDate.Value),
                            CreatedBy = currentUserId
                        };
                        details.Add(detail);
                    }
                }

                if (details.Count == 0)
                {
                    MessageBox.Show("Please select at least one invoice and adjust the amount.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create voucher entries
                var vouchers = new List<VoucherEntry>();

                // Debit entry (Vendor account)
                vouchers.Add(new VoucherEntry
                {
                    VoucherNo = voucherNo,
                    LedgerId = vendorLedgerId,
                    LedgerName = txtVendorName.Text,
                    DebitAmount = totalPaymentAmount,
                    CreditAmount = 0,
                    VoucherDate = voucherDate,
                    Narration = $"Payment to {txtVendorName.Text}",
                    BranchId = currentBranchId,
                    CreatedBy = currentUserId
                });

                // Credit entry (Cash/Bank account)
                vouchers.Add(new VoucherEntry
                {
                    VoucherNo = voucherNo,
                    LedgerId = paymentMethodId,
                    LedgerName = CmboPayment.Text,
                    DebitAmount = 0,
                    CreditAmount = totalPaymentAmount,
                    VoucherDate = voucherDate,
                    Narration = $"Payment to {txtVendorName.Text}",
                    BranchId = currentBranchId,
                    CreatedBy = currentUserId
                });

                // Save to database using stored procedures
                int paymentMasterId = paymentRepo.SaveVendorPayment(master, details, vouchers);

                MessageBox.Show("Payment saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving payment: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateVoucherNumber()
        {
            // Generate a unique voucher number
            return $"PAY{DateTime.Now:yyyyMMddHHmmss}";
        }

        private bool ValidatePayment()
        {
            if (radioBtnAllDocument != null && radioBtnAllDocument.Checked)
            {
                MessageBox.Show("Please switch to Outstanding mode to select invoices and process payments.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(textBox4.Text))
            {
                MessageBox.Show("Please select a vendor", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(CmboPayment.Text))
            {
                MessageBox.Show("Please select payment method", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (totalPaymentAmount <= 0)
            {
                MessageBox.Show("Please enter payment amount", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var selectedRows = ultraGrid1.Rows.Where(row =>
                row.Cells.Exists("Select") && row.Cells["Select"].Value != null && Convert.ToBoolean(row.Cells["Select"].Value)).ToList();
            if (!selectedRows.Any())
            {
                MessageBox.Show("Please select at least one invoice", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Check if total adjusted amount matches total payment amount
            decimal totalAdjusted = GetTotalAdjustedAmount();
            if (Math.Abs(totalAdjusted - totalPaymentAmount) > 0.01m)
            {
                MessageBox.Show($"The total adjusted amount ({totalAdjusted:N2}) does not match " +
                    $"the total payment amount ({totalPaymentAmount:N2}).", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            txtVendorName.Text = "";
            textBox4.Text = "";
            currentVendorLedgerId = 0;
            currentPaymentMasterId = 0;
            // Restore the user's preferred payment method (defaults to Cash on first load)
            if (_userSelectedPayModeId != null)
                CmboPayment.Value = _userSelectedPayModeId;
            else
                CmboPayment.SelectedIndex = -1;
            textBox1.Text = "0";
            textBox2.Text = "";
            ultraTextEditor2.Value = PaymentStatusActive;
            richTextBox2.Text = "";
            dtpPurchaseDate.Value = DateTime.Now;
            ultraGrid1.DataSource = CreateEmptyInvoiceTable();
            ConfigureGridColumns();
            ultraTextEditor1.Text = "0.00";
            ultraTextEditor3.Text = string.Empty;    // clear cancelled GRN display
            txtOutstanding.Text = "0.00";
            totalPaymentAmount = 0;
            // Re-initialize the repository to ensure connection string is set
            paymentRepo = new VendorPaymentRepository();
        }

        private bool IsCancelStatusSelected()
        {
            string status = Convert.ToString(ultraTextEditor2.Value ?? ultraTextEditor2.Text);
            return string.Equals(status, PaymentStatusCancel, StringComparison.OrdinalIgnoreCase);
        }

        private void CancelLoadedPayment()
        {
            try
            {
                if (currentPaymentMasterId <= 0)
                {
                    MessageBox.Show("Please load an existing vendor payment before selecting Cancel.",
                        "Payment Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraTextEditor2.Value = PaymentStatusActive;
                    return;
                }

                string voucherText = string.IsNullOrWhiteSpace(txtPurchaseNo.Text) ? currentPaymentMasterId.ToString() : txtPurchaseNo.Text.Trim();
                DialogResult confirm = MessageBox.Show(
                    "Do you want to cancel/reverse vendor payment voucher " + voucherText + "?"
                    + Environment.NewLine + Environment.NewLine
                    + "All active bill allocations in this payment voucher will be reversed and the payment will be preserved as cancelled.",
                    "Cancel Vendor Payment",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                {
                    ultraTextEditor2.Value = PaymentStatusActive;
                    return;
                }

                VendorPaymentRepository.PurchasePaymentCancellationSummary cancelled =
                    paymentRepo.CancelVendorPayment(
                        currentPaymentMasterId,
                        currentBranchId,
                        currentUserId,
                        "Cancelled from Payment screen");

                MessageBox.Show(
                    "Vendor payment cancelled successfully."
                    + Environment.NewLine + "Cancelled voucher(s): " + cancelled.PaymentVoucherCount
                    + Environment.NewLine + "Reversed amount: " + cancelled.PaymentAmount.ToString("N2"),
                    "Payment Cancelled",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cancelling payment: " + ex.Message,
                    "Payment Cancellation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnViewPayment_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the selected row from the grid - using ActiveRow instead of checking Selected property
                UltraGridRow selectedRow = ultraGrid1.ActiveRow;

                if (selectedRow == null)
                {
                    MessageBox.Show("Please select a bill to view payment history.", "Selection Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Get the BillNo from the selected row
                if (!selectedRow.Cells.Exists("BillNo") || selectedRow.Cells["BillNo"].Value == null)
                {
                    MessageBox.Show("Selected row does not contain a valid Bill Number.", "Invalid Selection",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string billNo = selectedRow.Cells["BillNo"].Value.ToString();

                // Open the payment history form using our FrmViewPayment class
                FrmViewPayment paymentHistoryForm = new FrmViewPayment(currentVendorLedgerId, billNo);
                paymentHistoryForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing payment history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPurch_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the selected row from the payment grid
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
                    MessageBox.Show("Invalid bill number in selected row.", "Invalid Data",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string billNo = billNoValue.ToString();

                // Create and show the purchase form directly
                var purchaseForm = new PosBranch_Win.Transaction.FrmPurchase();

                // Open the purchase form in a new tab first
                OpenPurchaseInTab(purchaseForm, $"Purchase - Bill #{billNo}");

                // Wait a moment for the form to be fully embedded, then load the data
                System.Threading.Tasks.Task.Delay(100).ContinueWith(_ => {
                    this.Invoke(new Action(() => {
                        LoadPurchaseIntoSalesForm(purchaseForm, billNo);
                    }));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening purchase: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void OpenPurchaseInTab(PosBranch_Win.Transaction.FrmPurchase purchaseForm, string tabName)
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
                        openFormInTabMethod.Invoke(homeForm, new object[] { purchaseForm, tabName });
                        return;
                    }
                }

                // Fallback: show as regular form
                purchaseForm.Show();
                purchaseForm.BringToFront();
            }
            catch (Exception ex)
            {
                // Fallback: show as regular form
                purchaseForm.Show();
                purchaseForm.BringToFront();
                System.Diagnostics.Debug.WriteLine($"Error opening in tab, showing as regular form: {ex.Message}");
            }
        }

        private void LoadPurchaseIntoSalesForm(PosBranch_Win.Transaction.FrmPurchase purchaseForm, string billNo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== LOADING PURCHASE DATA FOR BILL #{billNo} ===");

                // Convert billNo to integer for the purchase number
                if (!int.TryParse(billNo, out int purchaseNo))
                {
                    MessageBox.Show("Invalid purchase number format: " + billNo, "Invalid Data",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Purchase Number: {purchaseNo}");

                // Get the Pid from PurchaseNo
                int purchaseId = GetPidFromPurchaseNo(purchaseNo);
                if (purchaseId <= 0)
                {
                    MessageBox.Show($"Could not find purchase with number: {purchaseNo}", "Purchase Not Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Purchase ID: {purchaseId}");

                // Load the purchase data using the LoadPurchaseData method
                purchaseForm.LoadPurchaseData(purchaseId);

                System.Diagnostics.Debug.WriteLine("=== PURCHASE DATA LOADING COMPLETE ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading purchase data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show("Error loading purchase data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetPidFromPurchaseNo(int purchaseNo)
        {
            try
            {
                // Get connection string from the payment repository
                var paymentRepo = new Repository.Accounts.VendorPaymentRepository();
                using (var connection = new System.Data.SqlClient.SqlConnection(paymentRepo.DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("SELECT Pid FROM PMaster WHERE PurchaseNo = @PurchaseNo AND BranchId = @BranchId", connection))
                    {
                        cmd.Parameters.AddWithValue("@PurchaseNo", purchaseNo);
                        cmd.Parameters.AddWithValue("@BranchId", currentBranchId); // Use the current branch ID

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

        private void UltraGrid1_CellChange(object sender, CellEventArgs e)
        {
            if (e.Cell.Column.Key == "Select")
            {
                ultraGrid1.UpdateData(); // Commit the cell value
                UpdateAdjustedAmounts();
            }
        }

        private void DistributeAdjustedAmounts()
        {
            decimal remaining = totalPaymentAmount;
            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                bool isSelected = false;
                if (row.Cells.Exists("Select") && row.Cells["Select"].Value != null)
                    isSelected = Convert.ToBoolean(row.Cells["Select"].Value);

                decimal invoiceAmount = row.Cells.Exists("InvoiceAmount") && row.Cells["InvoiceAmount"].Value != null ? Convert.ToDecimal(row.Cells["InvoiceAmount"].Value) : 0m;
                decimal paidAmount = row.Cells.Exists("PayedAmount") && row.Cells["PayedAmount"].Value != null ? Convert.ToDecimal(row.Cells["PayedAmount"].Value) : 0m;
                decimal originalBalance = invoiceAmount - paidAmount;

                if (isSelected && remaining > 0)
                {
                    // For payments, handle negative balances (vendor overpaid) differently
                    if (originalBalance < 0)
                    {
                        // If balance is negative, allow adjustment up to the absolute value
                        decimal maxAdjustment = Math.Abs(originalBalance);
                        decimal adjusted = Math.Min(remaining, maxAdjustment);
                        row.Cells["AdjustedAmount"].Value = adjusted;
                        remaining -= adjusted;
                    }
                    else
                    {
                        // Normal case: positive balance
                        decimal adjusted = Math.Min(originalBalance, remaining);
                        row.Cells["AdjustedAmount"].Value = adjusted;
                        remaining -= adjusted;
                    }
                }
                else
                {
                    row.Cells["AdjustedAmount"].Value = 0m;
                }
                UpdateRowBalance(row);
            }
            UpdateRemainingAmount();
        }

        // KeyDown handler to open vendor dialog on F11
        private void FrmPayment_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                // Save
                btnSave_Click(sender, e);
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
                // Open vendor dialog (same as btnF11_Click)
                btnF11_Click(sender, e);
                e.Handled = true;
            }
        }

        private void LoadPayment(long voucherId)
        {
            try
            {
                DataSet ds = paymentRepo.GetPaymentDataByVoucherId(voucherId, currentBranchId);
                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return;

                isAdjusting = true;
                DataRow master = ds.Tables[0].Rows[0];

                currentPaymentMasterId = master.Table.Columns.Contains("Id") && master["Id"] != DBNull.Value
                    ? Convert.ToInt32(master["Id"])
                    : 0;
                txtPurchaseNo.Text = master["VoucherId"].ToString();
                currentVendorLedgerId = Convert.ToInt32(master["VendorLedgerId"]);
                textBox4.Text = currentVendorLedgerId.ToString();
                ultraTextEditor2.Value = PaymentStatusActive;
                
                // Get vendor name from the 3rd table if available (Vendor Info / Balance)
                if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
                {
                    txtVendorName.Text = ds.Tables[2].Rows[0]["LedgerName"]?.ToString() ?? "";
                    txtOutstanding.Text = ds.Tables[2].Rows[0][1]?.ToString() ?? "0.00";
                }
                else
                {
                    // Fallback to searching vendor name
                    var vendorList = ObjDrop.VendorDDL().List;
                    var vendor = vendorList.FirstOrDefault(v => v.LedgerID == currentVendorLedgerId);
                    if (vendor != null) txtVendorName.Text = vendor.LedgerName;
                }

                textBox1.Text = master["PaymentAmount"]?.ToString() ?? "0.00";
                totalPaymentAmount = SafeToDecimal(master["PaymentAmount"]);
                
                if (master["PaymentMethodLedgerId"] != DBNull.Value)
                    CmboPayment.Value = master["PaymentMethodLedgerId"];
                
                if (master["VoucherDate"] != DBNull.Value)
                    dtpPurchaseDate.Value = Convert.ToDateTime(master["VoucherDate"]);

                textBox2.Text = "";
                richTextBox2.Text = master["Narration"]?.ToString() ?? "";

                // Populate Grid
                DataTable dtGrid = CreateEmptyInvoiceTable();
                if (ds.Tables.Count > 1)
                {
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        DataRow newRow = dtGrid.NewRow();
                        newRow["BillNo"] = dr["BillNo"]?.ToString();
                        newRow["InvoiceNo"] = dr.Table.Columns.Contains("InvoiceNo") ? dr["InvoiceNo"]?.ToString() : dr["BillNo"]?.ToString();
                        newRow["InvoiceAmount"] = SafeToDecimal(dr["BillAmount"]);
                        newRow["PayedAmount"] = SafeToDecimal(dr["PayedAmount"]);
                        newRow["ReturnedAmount"] = dr.Table.Columns.Contains("ReturnedAmount") && dr["ReturnedAmount"] != DBNull.Value ? SafeToDecimal(dr["ReturnedAmount"]) : 0m;
                        newRow["Balance"] = SafeToDecimal(dr["BalanceAmount"]);
                        newRow["Select"] = true;
                        newRow["AdjustedAmount"] = SafeToDecimal(dr["PaymentAmount"]);
                        newRow["BillDate"] = dr.Table.Columns.Contains("BillDate") && dr["BillDate"] != DBNull.Value ? SafeToDateTime(dr["BillDate"]) : DateTime.Now;
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
                MessageBox.Show("Error loading payment: " + ex.Message);
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
    }
}
