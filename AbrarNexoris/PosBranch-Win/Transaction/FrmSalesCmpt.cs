using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.Misc;
using ModelClass.TransactionModels;
using Repository;
using Repository.TransactionRepository;

namespace PosBranch_Win.Transaction
{
    public partial class FrmSalesCmpt : Form
    {
        #region Private Fields
        private SalesMaster _salesMaster;
        private SalesRepository _operations;
        private bool _isCreditMode = false;
        private string _netAmount;
        private PaymentResult _paymentResult;
        private string _selectedPaymentMethod = "Cash";
        private bool _isProcessing = false;

        // Split Payment Fields
        private List<SalesPaymentDetail> _paymentDetailsList = new List<SalesPaymentDetail>();
        private DataTable _paymentGridSource;
        private decimal _totalAmount;
        private decimal _remainingAmount;
        #endregion

        #region Public Properties
        public PaymentResult PaymentResult => _paymentResult;
        public bool IsPaymentSuccessful => _paymentResult?.IsSuccess ?? false;
        public string BillNumber => _paymentResult?.BillNumber ?? string.Empty;
        public string ErrorMessage => _paymentResult?.ErrorMessage ?? string.Empty;
        #endregion

        #region Constructor
        public FrmSalesCmpt(SalesMaster salesMaster, string netAmount, bool isCreditMode = false)
        {
            InitializeComponent();
            _salesMaster = salesMaster ?? throw new ArgumentNullException(nameof(salesMaster));

            // Validate and clean the net amount string
            if (string.IsNullOrWhiteSpace(netAmount))
            {
                _netAmount = "0.00";
            }
            else
            {
                // Try to parse and reformat the amount to ensure it's valid
                if (decimal.TryParse(netAmount, out decimal parsedAmount))
                {
                    _netAmount = parsedAmount.ToString("F2");
                }
                else
                {
                    // If parsing fails, try to clean the string and parse again
                    string cleanedAmount = netAmount.Replace(",", "").Replace(" ", "").Trim();
                    if (decimal.TryParse(cleanedAmount, out decimal cleanedParsedAmount))
                    {
                        _netAmount = cleanedParsedAmount.ToString("F2");
                    }
                    else
                    {
                        _netAmount = "0.00";
                        System.Diagnostics.Debug.WriteLine($"Warning: Could not parse net amount '{netAmount}', using default '0.00'");
                    }
                }
            }

            _isCreditMode = isCreditMode;
            _operations = new SalesRepository();

            InitializePaymentDialog();
        }
        #endregion

        #region Initialization
        private void InitializePaymentDialog()
        {
            // Set form properties
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            // Initialize UI
            InitializePaymentModes();
            InitializeAmounts();
            CreatePaymentGrid(); // Create the payment grid display
            InitializeSplitPayment(); // Initialize split payment values
            SetupEventHandlers();
            UpdateUIForPaymentMode();
        }

        /// <summary>
        /// Creates and configures the payment grid to display payment entries (IRS POS Style)
        /// Using Infragistics UltraGrid
        /// </summary>
        private void CreatePaymentGrid()
        {
            // Hide the single-entry controls (we'll use the grid instead)
            cmbPaymodefc.Visible = false;
            TxtPayedAmtfc.Visible = false;
            txtReffc.Visible = false;

            // Create DataSource for UltraGrid
            _paymentGridSource = new DataTable();
            _paymentGridSource.Columns.Add("PayMode", typeof(string));
            _paymentGridSource.Columns.Add("Amount", typeof(decimal));
            _paymentGridSource.Columns.Add("Reference", typeof(string));

            // Bind data source to UltraGrid
            ultraGridPayments.DataSource = _paymentGridSource;

            // Configure UltraGrid appearance and behavior
            ConfigureUltraGridPayments();

            // Handle cell value changes for real-time updates
            ultraGridPayments.AfterCellUpdate += UltraGridPayments_AfterCellUpdate;
            ultraGridPayments.KeyDown += UltraGridPayments_KeyDown;
            ultraGridPayments.CellChange += UltraGridPayments_CellChange; // Added CellChange wiring
        }

        /// <summary>
        /// Configures the UltraGrid appearance and column settings to match IRS POS exactly
        /// </summary>
        private void ConfigureUltraGridPayments()
        {
            try
            {
                ultraGridPayments.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

                var layout = ultraGridPayments.DisplayLayout;
                var overrideLayout = layout.Override;

                // 1️⃣ Row Height & Font
                // 1️⃣ Row Height & Font
                overrideLayout.DefaultRowHeight = 40; // Taller rows for touch/visibility
                overrideLayout.CellAppearance.FontData.Name = "Segoe UI";
                overrideLayout.CellAppearance.FontData.SizeInPoints = 12; // Balanced size
                overrideLayout.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;

                // 2️⃣ Header Styling (Aggressive)
                overrideLayout.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                overrideLayout.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent; // DISABLE THEME for Header
                overrideLayout.HeaderAppearance.BackColor = Color.FromArgb(66, 126, 219); // Professional POS Blue
                overrideLayout.HeaderAppearance.BackColor2 = Color.FromArgb(66, 126, 219);
                overrideLayout.HeaderAppearance.ForeColor = Color.White; // White Text
                overrideLayout.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                overrideLayout.HeaderAppearance.FontData.SizeInPoints = 11;
                overrideLayout.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;

                // 3️⃣ Borders
                overrideLayout.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                overrideLayout.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                overrideLayout.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
                overrideLayout.CellAppearance.BorderColor = Color.LightGray;
                overrideLayout.RowAppearance.BorderColor = Color.LightGray;
                overrideLayout.HeaderAppearance.BorderColor = Color.White;
                overrideLayout.RowAppearance.BackColor = Color.White;

                // Remove dotted focus rect
                overrideLayout.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.None;
                overrideLayout.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;

                // 4️⃣ Interaction
                layout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;
                overrideLayout.AllowColSizing = Infragistics.Win.UltraWinGrid.AllowColSizing.None; // Corrected property name

                // Hide row selectors
                overrideLayout.RowSelectors = Infragistics.Win.DefaultableBoolean.False;

                // Cell padding
                overrideLayout.CellPadding = 2; // Tighter vertical lines

                // 4️⃣ Selected Row Highlight (Moved from original 4)
                overrideLayout.SelectedRowAppearance.BackColor = Color.FromArgb(214, 229, 255); // Soft Selection Blue
                overrideLayout.SelectedRowAppearance.ForeColor = Color.Black;
                overrideLayout.ActiveRowAppearance.BackColor = Color.FromArgb(214, 229, 255);
                overrideLayout.ActiveRowAppearance.ForeColor = Color.Black;

                // 7️⃣ Hover Effect (Moved from original 7)
                overrideLayout.HotTrackRowAppearance.BackColor = Color.FromArgb(235, 245, 255); // Very soft hover

                overrideLayout.RowSelectors = Infragistics.Win.DefaultableBoolean.False;
                overrideLayout.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.NotAllowed;
                overrideLayout.AllowColSizing = Infragistics.Win.UltraWinGrid.AllowColSizing.None;
                overrideLayout.AllowRowFiltering = Infragistics.Win.DefaultableBoolean.False;
                layout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;
                layout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;

                // General Behavior
                overrideLayout.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                overrideLayout.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                overrideLayout.AllowUpdate = Infragistics.Win.DefaultableBoolean.True;
                overrideLayout.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.EditAndSelectText;

                // Ensure empty space is white
                layout.Appearance.BackColor = Color.White;

                // Check if columns exist before configuring bands
                if (layout.Bands.Count > 0 && layout.Bands[0].Columns.Count > 0)
                {
                    var band = layout.Bands[0];

                    // 5️⃣ Column Alignment
                    if (band.Columns.Exists("PayMode"))
                    {
                        band.Columns["PayMode"].Header.Caption = "Pymt Mode";
                        band.Columns["PayMode"].Width = 140;
                        band.Columns["PayMode"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                        band.Columns["PayMode"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                    }

                    if (band.Columns.Exists("Amount"))
                    {
                        band.Columns["Amount"].Header.Caption = "Pymt Amt";
                        band.Columns["Amount"].Width = 140;
                        band.Columns["Amount"].Format = "0.00";
                        band.Columns["Amount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        band.Columns["Amount"].CellAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                        band.Columns["Amount"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.AllowEdit;
                    }

                    if (band.Columns.Exists("Reference"))
                    {
                        band.Columns["Reference"].Header.Caption = "Reference";
                        band.Columns["Reference"].Width = 170;
                        band.Columns["Reference"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.AllowEdit; // Made editable
                    }
                }

                // edit appearance settings
                overrideLayout.EditCellAppearance.BackColor = Color.White;
                overrideLayout.EditCellAppearance.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configuring UltraGrid: {ex.Message}");
            }
        }

        private void InitializePaymentModes()
        {
            try
            {
                var dp = new Dropdowns();
                var pm = dp.PaymodeDDl();

                // Create DataTable for payment modes (exclude Credit for payment panel)
                var paymentModesTable = new DataTable();
                paymentModesTable.Columns.Add("PayModeId", typeof(int));
                paymentModesTable.Columns.Add("PayModeName", typeof(string));

                foreach (var item in pm.List)
                {
                    if (!item.PayModeName.Equals("Credit", StringComparison.OrdinalIgnoreCase))
                    {
                        paymentModesTable.Rows.Add(item.PayModeID, item.PayModeName);
                    }
                }

                cmbPaymodefc.DataSource = paymentModesTable;
                cmbPaymodefc.DisplayMember = "PayModeName";
                cmbPaymodefc.ValueMember = "PayModeId";
                cmbPaymodefc.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError($"Error loading payment modes: {ex.Message}");
            }
        }

        private void InitializeAmounts()
        {
            label23.Text = _netAmount;
            TxtPayedAmtfc.Text = _netAmount;
            txtBalance.Text = "0.00";
            txtReffc.Text = string.Empty;

            // Set button tooltips
            toolTip1.SetToolTip(ultraButton1, "Process payment and complete transaction (Enter)");
            toolTip1.SetToolTip(ultraButton2, "Cancel payment and return (Esc)");

            // Set payment mode button tooltips
            toolTip1.SetToolTip(pictureBox1, "Cash Payment (F1)");
            toolTip1.SetToolTip(label13, "Cash Payment (F1)");
            toolTip1.SetToolTip(pictureBox2, "Card Payment (F2)");
            toolTip1.SetToolTip(label15, "Card Payment (F2)");
            toolTip1.SetToolTip(pictureBox3, "Bank Transfer Payment (F3)");
            toolTip1.SetToolTip(label14, "Bank Transfer Payment (F3)");
            toolTip1.SetToolTip(pictureBox4, "UPI Payment (F4)");
            toolTip1.SetToolTip(label16, "UPI Payment (F4)");
        }

        private void SetupEventHandlers()
        {
            TxtPayedAmtfc.TextChanged += TxtPayedAmtfc_TextChanged;
            TxtPayedAmtfc.KeyDown += TxtPayedAmtfc_KeyDown;
            TxtPayedAmtfc.KeyPress += TxtPayedAmtfc_KeyPress;
            txtReffc.KeyDown += TxtReffc_KeyDown;
            cmbPaymodefc.ValueChanged += CmbPaymodefc_ValueChanged;
            ultraButton1.Click += UltraButton1_Click;
            ultraButton2.Click += UltraButton2_Click;
            this.KeyDown += FrmSalesCmpt_KeyDown;
            this.KeyPreview = true;

            // IRS POS STYLE: Payment method buttons ADD to grid instead of just selecting
            // Clicking adds a new row with the remaining balance
            pictureBox1.Click += (s, e) => AddPaymentToGrid("Cash");
            label13.Click += (s, e) => AddPaymentToGrid("Cash");
            pictureBox2.Click += (s, e) => AddPaymentToGrid("Card");
            label15.Click += (s, e) => AddPaymentToGrid("Card");
            pictureBox3.Click += (s, e) => AddPaymentToGrid("Transfer");
            label14.Click += (s, e) => AddPaymentToGrid("Transfer");
            pictureBox4.Click += (s, e) => AddPaymentToGrid("UPI");
            label16.Click += (s, e) => AddPaymentToGrid("UPI");

            // Add hover effects to payment buttons
            AddPaymentButtonHoverEffect(pictureBox1, label13);
            AddPaymentButtonHoverEffect(pictureBox2, label15);
            AddPaymentButtonHoverEffect(pictureBox3, label14);
            AddPaymentButtonHoverEffect(pictureBox4, label16);

            // Set cursor for clickable elements
            pictureBox1.Cursor = Cursors.Hand;
            pictureBox2.Cursor = Cursors.Hand;
            pictureBox3.Cursor = Cursors.Hand;
            pictureBox4.Cursor = Cursors.Hand;
            label13.Cursor = Cursors.Hand;
            label15.Cursor = Cursors.Hand;
            label14.Cursor = Cursors.Hand;
            label16.Cursor = Cursors.Hand;
            ultraButton1.Cursor = Cursors.Hand;
            ultraButton2.Cursor = Cursors.Hand;

            // Add button hover effects
            AddButtonHoverEffect(ultraButton1, true);
            AddButtonHoverEffect(ultraButton2, false);
        }
        #endregion

        #region Event Handlers
        private void FrmSalesCmpt_Load(object sender, EventArgs e)
        {
            // Load custom DS-Digital font from embedded resources
            try
            {
                Utilities.CustomFontLoader.Initialize();
                txtBalance.Font = Utilities.CustomFontLoader.GetDSDigitalFont(34, FontStyle.Bold);
                label23.Font = Utilities.CustomFontLoader.GetDSDigitalFont(34, FontStyle.Bold);
            }
            catch (Exception fontEx)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load custom font: {fontEx.Message}");
                // Font will fall back to default if loading fails
            }

            // Focus on payment grid's Amount cell
            this.BeginInvoke(new Action(() =>
            {
                if (ultraGridPayments.Rows.Count > 0)
                {
                    this.ActiveControl = ultraGridPayments; // FORCE focus to grid control
                    ultraGridPayments.Focus();

                    var row = ultraGridPayments.Rows[0];
                    ultraGridPayments.ActiveRow = row;

                    if (row.Cells.Exists("Amount"))
                    {
                        ultraGridPayments.ActiveCell = row.Cells["Amount"];
                        ultraGridPayments.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.EnterEditMode);
                    }
                }
            }));

            // Wire up Grid KeyDown event manually to ensure it's captured
            this.ultraGridPayments.KeyDown += new KeyEventHandler(this.ultraGridPayments_KeyDown);
        }

        private void ultraGridPayments_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Commit any pending edits
                ultraGridPayments.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.ExitEditMode);
                ultraGridPayments.UpdateData();

                // Process the payment
                ProcessPayment();
                e.Handled = true;
            }
        }

        private void TxtPayedAmtfc_TextChanged(object sender, EventArgs e)
        {
            CalculateBalance();
        }

        private void TxtPayedAmtfc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ProcessPayment();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                CancelPayment();
                e.Handled = true;
            }
        }

        private void TxtReffc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ProcessPayment();
                e.Handled = true;
            }
        }

        private void TxtPayedAmtfc_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digits, one decimal point, backspace, and control keys
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
                return;
            }

            // Allow only one decimal point
            if (e.KeyChar == '.' && TxtPayedAmtfc.Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles key down events for the form
        /// </summary>
        private void FrmSalesCmpt_KeyDown(object sender, KeyEventArgs e)
        {
            // F1 = Cash
            if (e.KeyCode == Keys.F1)
            {
                AddPaymentToGrid("Cash");
                e.Handled = true;
            }
            // F2 = Card
            else if (e.KeyCode == Keys.F2)
            {
                AddPaymentToGrid("Card");
                e.Handled = true;
            }
            // F3 = Transfer
            else if (e.KeyCode == Keys.F3)
            {
                AddPaymentToGrid("Transfer");
                e.Handled = true;
            }
            // F4 = UPI
            else if (e.KeyCode == Keys.F4)
            {
                AddPaymentToGrid("UPI");
                e.Handled = true;
            }
            // Enter = Process Payment (only if not in grid edit mode)
            else if (e.KeyCode == Keys.Enter && !ultraGridPayments.Focused)
            {
                ProcessPayment();
                e.Handled = true;
            }
            // Escape = Cancel
            else if (e.KeyCode == Keys.Escape)
            {
                CancelPayment();
                e.Handled = true;
            }
        }

        private void CmbPaymodefc_ValueChanged(object sender, EventArgs e)
        {
            UpdatePaymentReferenceHint();
        }

        private void UltraButton1_Click(object sender, EventArgs e)
        {
            // Add visual click feedback
            AnimateButtonClick(ultraButton1);
            ProcessPayment();
        }

        private void UltraButton2_Click(object sender, EventArgs e)
        {
            // Add visual click feedback
            AnimateButtonClick(ultraButton2);
            CancelPayment();
        }


        #endregion

        #region Payment Processing
        private void ProcessPayment()
        {
            try
            {
                // Prevent double-click processing
                if (_isProcessing)
                    return;

                if (!ValidatePaymentInput())
                    return;

                _isProcessing = true;
                SetProcessingState(true);

                // Update sales master with payment information
                UpdateSalesMasterWithPayment();

                // Process the payment
                string result = ProcessPaymentSale();

                // Handle result
                HandlePaymentResult(result);
            }
            catch (Exception ex)
            {
                ShowError($"Error processing payment: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
                SetProcessingState(false);
            }
        }

        private bool ValidatePaymentInput()
        {
            // Sync any pending edits from grid
            if (ultraGridPayments.ActiveRow != null)
            {
                ultraGridPayments.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.ExitEditMode);
                SyncGridToPaymentList();
            }

            // check if there are any payments
            if (_paymentDetailsList.Count == 0)
            {
                ShowError("Please add at least one payment entry.");
                return false;
            }

            // Check for zero or negative amounts in the list
            foreach (var payment in _paymentDetailsList)
            {
                if (payment.Amount <= 0)
                {
                    ShowError($"Invalid payment amount: {payment.Amount:F2}. Payment must be greater than zero.");
                    return false;
                }
            }

            // Check total coverage
            decimal totalPaid = _paymentDetailsList.Sum(p => p.Amount);
            decimal totalBill = _totalAmount; // Already parsed in InitializeSplitPayment

            if (totalPaid < totalBill)
            {
                decimal remaining = totalBill - totalPaid;
                var result = MessageBox.Show(
                    $"Total payments ({totalPaid:F2}) are less than bill amount ({totalBill:F2}).\n\n" +
                    $"Remaining: {remaining:F2}\n\n" +
                    "Do you want to continue with partial payment?",
                    "Insufficient Payment",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    return false;
                }
            }

            return true;
        }

        private void UpdateSalesMasterWithPayment()
        {
            // Calculate totals from the payment grid list (not hidden controls)
            decimal totalPaid = _paymentDetailsList.Sum(p => p.Amount);
            decimal change = totalPaid > _totalAmount ? totalPaid - _totalAmount : 0;
            decimal balance = totalPaid < _totalAmount ? _totalAmount - totalPaid : 0;

            _salesMaster.TenderedAmount = (float)totalPaid;
            _salesMaster.Balance = (float)change; // Change to give back
            _salesMaster.ReceivedAmount = (float)totalPaid;
            _salesMaster.IsPaid = balance == 0;
            _salesMaster.NetAmount = (float)_totalAmount;

            // NOTE: Do NOT update _salesMaster.PaymodeId or PaymodeName here!
            // SMaster.PaymodeName should remain as "Cash" or "Credit" (from cmbPaymt in frmSalesInvoice)
            // The detailed payment methods (UPI, Card, etc.) are saved separately to SPaymentDetails table

            // Set payment reference for tracking
            if (_paymentDetailsList.Count > 0)
            {
                if (IsSplitPayment())
                {
                    _salesMaster.PaymentReference = $"SPLIT:{_paymentDetailsList.Count} payments";
                }
                else
                {
                    _salesMaster.PaymentReference = _paymentDetailsList[0].Reference;
                }
            }
        }

        private string ProcessPaymentSale()
        {
            // NEW LOGIC: Always return PENDING_PARENT_SAVE and let the parent form (frmSalesInvoice)
            // handle the actual saving/updating. This ensures that stock reduction and other 
            // complex logic in SalesRepository.UpdateSales is used correctly for held bills.
            // Previously, held bills used a shortcut CompleteSale method that missed stock reduction.

            System.Diagnostics.Debug.WriteLine($"FrmSalesCmpt: Delegating save to parent form for {(_salesMaster.BillNo > 0 ? "held bill #" + _salesMaster.BillNo : "new bill")}");
            return "PENDING_PARENT_SAVE";
        }

        private void HandlePaymentResult(string result)
        {
            if (string.IsNullOrEmpty(result) || result == "0" || result.StartsWith("Error"))
            {
                _paymentResult = new PaymentResult
                {
                    IsSuccess = false,
                    ErrorMessage = result.StartsWith("Error") ? result : "Payment processing failed."
                };
                ShowError(_paymentResult.ErrorMessage);
            }
            else
            {
                // Calculate from grid list (not hidden controls)
                decimal totalPaid = _paymentDetailsList.Sum(p => p.Amount);
                decimal change = totalPaid > _totalAmount ? totalPaid - _totalAmount : 0;

                string tenderedAmount = totalPaid.ToString("F2");
                string changeAmount = change.ToString("F2");

                // Determine payment mode from list
                string paymentMode = "Cash";
                string paymentReference = "";

                if (IsSplitPayment())
                {
                    paymentMode = "Split Payment";
                    paymentReference = $"SPLIT_PAYMENT:{_paymentDetailsList.Count}";
                }
                else if (_paymentDetailsList.Count == 1)
                {
                    paymentMode = _paymentDetailsList[0].PaymodeName;
                    paymentReference = _paymentDetailsList[0].Reference ?? "";
                }

                _paymentResult = new PaymentResult
                {
                    IsSuccess = true,
                    BillNumber = result,
                    TenderedAmount = tenderedAmount,
                    ChangeAmount = changeAmount,
                    PaymentMode = paymentMode,
                    PaymentReference = paymentReference,
                    PaymentDetails = _paymentDetailsList
                };

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void CancelPayment()
        {
            _paymentResult = new PaymentResult { IsSuccess = false };
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        #endregion

        #region UI Helper Methods
        private void CalculateBalance()
        {
            try
            {
                // IRS POS STYLE: Update current payment amount in list when user edits
                UpdateCurrentPaymentAmount();

                // The remaining balance is already updated by UpdateRemainingAmount()
                // which is called by UpdateCurrentPaymentAmount()
            }
            catch (Exception ex)
            {
                txtBalance.Text = "0.00";
                txtBalance.ForeColor = Color.White;
                System.Diagnostics.Debug.WriteLine($"Error calculating balance: {ex.Message}");
            }
        }

        private void SetPaymentMode(string modeName)
        {
            _selectedPaymentMethod = modeName;

            // Map display names to actual payment mode names in database
            string searchName = modeName;
            if (modeName.Equals("Transfer", StringComparison.OrdinalIgnoreCase))
            {
                searchName = "BankTransfer";
            }

            // Search through the combobox items
            for (int i = 0; i < cmbPaymodefc.Items.Count; i++)
            {
                var item = cmbPaymodefc.Items[i];
                string itemText = string.Empty;

                // Get the display text from the ValueListItem
                if (item is Infragistics.Win.ValueListItem valueListItem)
                {
                    itemText = valueListItem.DisplayText ?? string.Empty;
                }
                else
                {
                    itemText = item.ToString();
                }

                // Try exact match first
                if (itemText.Equals(searchName, StringComparison.OrdinalIgnoreCase))
                {
                    cmbPaymodefc.SelectedIndex = i;
                    break;
                }
                // Try partial match (e.g., "BankTransfer" contains "Transfer")
                else if (itemText.IndexOf(searchName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    cmbPaymodefc.SelectedIndex = i;
                    break;
                }
            }

            // Update visual feedback
            UpdatePaymentMethodSelection();

            // Focus on payment amount
            TxtPayedAmtfc.Focus();
            TxtPayedAmtfc.SelectAll();
        }

        private void UpdatePaymentMethodSelection()
        {
            // Reset all borders and backgrounds
            ResetPaymentButton(pictureBox1, label13);
            ResetPaymentButton(pictureBox2, label15);
            ResetPaymentButton(pictureBox3, label14);
            ResetPaymentButton(pictureBox4, label16);

            // Highlight selected payment method
            if (_selectedPaymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase))
            {
                HighlightPaymentButton(pictureBox1, label13);
            }
            else if (_selectedPaymentMethod.Equals("Card", StringComparison.OrdinalIgnoreCase))
            {
                HighlightPaymentButton(pictureBox2, label15);
            }
            else if (_selectedPaymentMethod.Equals("Transfer", StringComparison.OrdinalIgnoreCase))
            {
                HighlightPaymentButton(pictureBox3, label14);
            }
            else if (_selectedPaymentMethod.Equals("UPI", StringComparison.OrdinalIgnoreCase))
            {
                HighlightPaymentButton(pictureBox4, label16);
            }
        }

        private void ResetPaymentButton(PictureBox pictureBox, Label label)
        {
            pictureBox.BackColor = Color.White;
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            label.BackColor = Color.Transparent;
            label.ForeColor = Color.FromArgb(52, 58, 64);
        }

        private void HighlightPaymentButton(PictureBox pictureBox, Label label)
        {
            pictureBox.BackColor = Color.FromArgb(220, 240, 255);
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            label.BackColor = Color.FromArgb(220, 240, 255);
            label.ForeColor = Color.FromArgb(0, 86, 179);
        }

        private void AddPaymentButtonHoverEffect(PictureBox pictureBox, Label label)
        {
            // Store original colors
            Color originalPicBoxColor = pictureBox.BackColor;
            Color originalLabelColor = label.BackColor;
            Color hoverColor = Color.FromArgb(240, 248, 255);

            // PictureBox hover
            pictureBox.MouseEnter += (s, e) =>
            {
                if (pictureBox.BackColor != Color.FromArgb(220, 240, 255))
                {
                    pictureBox.BackColor = hoverColor;
                    label.BackColor = hoverColor;
                }
            };
            pictureBox.MouseLeave += (s, e) =>
            {
                if (pictureBox.BackColor != Color.FromArgb(220, 240, 255))
                {
                    pictureBox.BackColor = Color.White;
                    label.BackColor = Color.Transparent;
                }
            };

            // Label hover
            label.MouseEnter += (s, e) =>
            {
                if (pictureBox.BackColor != Color.FromArgb(220, 240, 255))
                {
                    pictureBox.BackColor = hoverColor;
                    label.BackColor = hoverColor;
                }
            };
            label.MouseLeave += (s, e) =>
            {
                if (pictureBox.BackColor != Color.FromArgb(220, 240, 255))
                {
                    pictureBox.BackColor = Color.White;
                    label.BackColor = Color.Transparent;
                }
            };
        }

        private void AddButtonHoverEffect(UltraButton button, bool isPrimary)
        {
            // Store original colors
            Color originalBackColor = button.Appearance.BackColor;
            Color originalBackColor2 = button.Appearance.BackColor2;

            // Hover colors (lighter shade)
            Color hoverBackColor = isPrimary
                ? Color.FromArgb(48, 195, 81)  // Lighter green
                : Color.FromArgb(240, 73, 89); // Lighter red

            Color hoverBackColor2 = isPrimary
                ? Color.FromArgb(40, 167, 69)  // Medium green
                : Color.FromArgb(220, 53, 69); // Medium red

            // Pressed colors (darker shade)
            Color pressedBackColor = isPrimary
                ? Color.FromArgb(25, 120, 45)  // Darker green
                : Color.FromArgb(160, 20, 36); // Darker red

            Color pressedBackColor2 = isPrimary
                ? Color.FromArgb(20, 100, 38)  // Darker green
                : Color.FromArgb(140, 15, 31); // Darker red

            button.MouseEnter += (s, e) =>
            {
                if (button.Enabled)
                {
                    button.Appearance.BackColor = hoverBackColor;
                    button.Appearance.BackColor2 = hoverBackColor2;
                }
            };

            button.MouseLeave += (s, e) =>
            {
                if (button.Enabled)
                {
                    button.Appearance.BackColor = originalBackColor;
                    button.Appearance.BackColor2 = originalBackColor2;
                }
            };

            button.MouseDown += (s, e) =>
            {
                if (button.Enabled)
                {
                    button.Appearance.BackColor = pressedBackColor;
                    button.Appearance.BackColor2 = pressedBackColor2;
                }
            };

            button.MouseUp += (s, e) =>
            {
                if (button.Enabled)
                {
                    button.Appearance.BackColor = hoverBackColor;
                    button.Appearance.BackColor2 = hoverBackColor2;
                }
            };
        }

        private async void AnimateButtonClick(UltraButton button)
        {
            // Store original size
            var originalSize = button.Size;
            var originalLocation = button.Location;

            // Shrink slightly
            button.Size = new Size(originalSize.Width - 4, originalSize.Height - 4);
            button.Location = new Point(originalLocation.X + 2, originalLocation.Y + 2);

            await Task.Delay(100);

            // Return to original size
            button.Size = originalSize;
            button.Location = originalLocation;
        }

        private void SetProcessingState(bool isProcessing)
        {
            ultraButton1.Enabled = !isProcessing;
            ultraButton2.Enabled = !isProcessing;
            TxtPayedAmtfc.Enabled = !isProcessing;
            cmbPaymodefc.Enabled = !isProcessing;
            txtReffc.Enabled = !isProcessing;

            if (isProcessing)
            {
                this.Cursor = Cursors.WaitCursor;
                ultraButton1.Text = "Processing...";

                // Dim the button during processing
                ultraButton1.Appearance.BackColor = Color.FromArgb(200, ultraButton1.Appearance.BackColor);
                ultraButton1.Appearance.BackColor2 = Color.FromArgb(200, ultraButton1.Appearance.BackColor2);
            }
            else
            {
                this.Cursor = Cursors.Default;
                ultraButton1.Text = _isCreditMode ? "Confirm" : "Process";

                // Restore original colors
                ultraButton1.Appearance.BackColor = Color.FromArgb(40, 167, 69);
                ultraButton1.Appearance.BackColor2 = Color.FromArgb(33, 136, 56);
            }
        }

        private void UpdatePaymentReferenceHint()
        {
            string paymentMode = cmbPaymodefc.Text?.ToLower() ?? "";
            string hint = "Enter payment reference (optional)";

            if (paymentMode.Contains("card"))
            {
                hint = "Enter card reference or transaction ID";
            }
            else if (paymentMode.Contains("transfer") || paymentMode.Contains("bank"))
            {
                hint = "Enter transfer reference or transaction ID";
            }
            else if (paymentMode.Contains("upi"))
            {
                hint = "Enter UPI transaction ID or reference";
            }
            else if (paymentMode.Contains("cheque"))
            {
                hint = "Enter cheque number";
            }

            toolTip1.SetToolTip(txtReffc, hint);
        }

        private void UpdateUIForPaymentMode()
        {
            // Update UI based on credit mode
            if (_isCreditMode)
            {
                this.Text = "Credit Sale Confirmation";
                ultraButton1.Text = "Confirm";
                // Hide payment amount field for credit sales
                TxtPayedAmtfc.Visible = false;
                txtBalance.Visible = false;
            }
            else
            {
                this.Text = "Payment Processing";
                ultraButton1.Text = "Process";
            }
        }
        #endregion

        #region Split Payment Methods

        /// <summary>
        /// Adds current payment entry to the split payment list
        /// </summary>
        public void AddCurrentPaymentToList()
        {
            try
            {
                // Validate current payment input
                string amountText = TxtPayedAmtfc.Text?.Replace(",", "").Replace(" ", "").Trim() ?? "0";
                if (!decimal.TryParse(amountText, out decimal amount) || amount <= 0)
                {
                    ShowError("Please enter a valid payment amount greater than 0.");
                    return;
                }

                // Check if amount exceeds remaining
                if (amount > _remainingAmount && _remainingAmount > 0)
                {
                    var result = MessageBox.Show(
                        $"Payment amount (₹{amount:N2}) exceeds remaining balance (₹{_remainingAmount:N2}).\n\nContinue anyway?",
                        "Excess Payment",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result != DialogResult.Yes)
                        return;
                }

                // Get payment mode details
                int paymodeId = 1;
                string paymodeName = "Cash";

                if (cmbPaymodefc.SelectedItem != null && cmbPaymodefc.SelectedItem.DataValue != null)
                {
                    paymodeId = (int)cmbPaymodefc.SelectedItem.DataValue;
                    paymodeName = cmbPaymodefc.Text;
                }

                // Create payment detail
                var paymentDetail = new SalesPaymentDetail
                {
                    CompanyId = ModelClass.SessionContext.CompanyId,
                    BranchId = ModelClass.SessionContext.BranchId,
                    FinYearId = ModelClass.SessionContext.FinYearId,
                    PaymodeId = paymodeId,
                    PaymodeName = paymodeName,
                    Amount = amount,
                    Reference = txtReffc.Text?.Trim() ?? ""
                };

                _paymentDetailsList.Add(paymentDetail);

                // Update remaining amount
                UpdateRemainingAmount();

                // Clear input fields for next entry
                TxtPayedAmtfc.Text = _remainingAmount > 0 ? _remainingAmount.ToString("F2") : "0.00";
                txtReffc.Text = "";

                System.Diagnostics.Debug.WriteLine($"Added payment: {paymodeName} - ₹{amount:N2}. Total payments: {_paymentDetailsList.Count}");
            }
            catch (Exception ex)
            {
                ShowError($"Error adding payment: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a payment entry from the list
        /// </summary>
        public void RemovePaymentFromList(int index)
        {
            if (index >= 0 && index < _paymentDetailsList.Count)
            {
                var removed = _paymentDetailsList[index];
                _paymentDetailsList.RemoveAt(index);
                UpdateRemainingAmount();
                System.Diagnostics.Debug.WriteLine($"Removed payment: {removed.PaymodeName} - ₹{removed.Amount:N2}");
            }
        }

        /// <summary>
        /// Clears all payment entries
        /// </summary>
        public void ClearPaymentList()
        {
            _paymentDetailsList.Clear();
            UpdateRemainingAmount();
            TxtPayedAmtfc.Text = _totalAmount.ToString("F2");
        }

        /// <summary>
        /// Updates the remaining amount based on payments entered
        /// </summary>
        private void UpdateRemainingAmount()
        {
            decimal totalPaid = _paymentDetailsList.Sum(p => p.Amount);
            _remainingAmount = _totalAmount - totalPaid;

            // Show actual remaining value:
            // Negative = still owe money (e.g., -10 means owe 10)
            // Positive = change to return (e.g., +5 means return 5)
            // Zero = exact match

            if (_remainingAmount > 0)
            {
                // Underpaid - show NEGATIVE value in RED (standard debt color)
                txtBalance.Text = "-" + _remainingAmount.ToString("F2");
                txtBalance.ForeColor = Color.Red; // Red is clearer for negative/shortage
            }
            else if (_remainingAmount < 0)
            {
                // Overpaid - show POSITIVE value (change to return)
                txtBalance.Text = Math.Abs(_remainingAmount).ToString("F2");
                txtBalance.ForeColor = Color.FromArgb(255, 255, 128); // Yellow - classic IRS POS
            }
            else
            {
                // Exact amount
                txtBalance.Text = "0.00";
                txtBalance.ForeColor = Color.LimeGreen; // Green - success
            }
        }

        /// <summary>
        /// Gets the summary of all payments for display
        /// </summary>
        public string GetPaymentsSummary()
        {
            if (_paymentDetailsList.Count == 0)
                return "No payments added";

            var sb = new StringBuilder();
            foreach (var payment in _paymentDetailsList)
            {
                sb.AppendLine($"{payment.PaymodeName}: ₹{payment.Amount:N2}" +
                    (string.IsNullOrEmpty(payment.Reference) ? "" : $" ({payment.Reference})"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the list of payment details for saving
        /// </summary>
        public List<SalesPaymentDetail> GetPaymentDetails()
        {
            return _paymentDetailsList;
        }

        /// <summary>
        /// Checks if split payment is being used (more than one payment method)
        /// </summary>
        public bool IsSplitPayment()
        {
            return _paymentDetailsList.Count > 1;
        }

        /// <summary>
        /// Validates that total payments cover the bill amount
        /// </summary>
        private bool ValidateTotalPayments()
        {
            decimal totalPaid = _paymentDetailsList.Sum(p => p.Amount);

            if (totalPaid < _totalAmount)
            {
                var remaining = _totalAmount - totalPaid;
                var result = MessageBox.Show(
                    $"Total payments (₹{totalPaid:N2}) are less than bill amount (₹{_totalAmount:N2}).\n\n" +
                    $"Remaining: ₹{remaining:N2}\n\n" +
                    "Do you want to continue with partial payment?",
                    "Insufficient Payment",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                return result == DialogResult.Yes;
            }

            return true;
        }

        /// <summary>
        /// Initializes split payment mode by parsing the total amount and adding default Cash row
        /// IRS POS STYLE: Default is Cash with full amount
        /// </summary>
        private void InitializeSplitPayment()
        {
            if (decimal.TryParse(_netAmount, out decimal total))
            {
                _totalAmount = total;
                _remainingAmount = total;

                // IRS POS STYLE: Add default Cash row with full amount
                if (_totalAmount > 0)
                {
                    AddPaymentToGridInternal("Cash", _totalAmount, "");

                    // Auto-focus the Amount cell for immediate editing
                    if (ultraGridPayments.Rows.Count > 0)
                    {
                        var row = ultraGridPayments.Rows[0];
                        ultraGridPayments.ActiveRow = row;
                        if (row.Cells.Exists("Amount"))
                        {
                            ultraGridPayments.ActiveCell = row.Cells["Amount"];
                            ultraGridPayments.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.EnterEditMode);
                            ultraGridPayments.Focus();
                        }
                    }
                }
            }
            else
            {
                _totalAmount = 0;
                _remainingAmount = 0;
            }
        }

        /// <summary>
        /// IRS POS STYLE: Adds a new payment row to the grid with remaining balance
        /// Called when user clicks Cash/Card/Transfer/UPI buttons
        /// </summary>
        private void AddPaymentToGrid(string paymodeName)
        {
            // Sync any pending edits from grid FIRST
            if (ultraGridPayments.ActiveRow != null)
            {
                ultraGridPayments.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.ExitEditMode);
                ultraGridPayments.UpdateData();
                SyncGridToPaymentList();
            }

            // If remaining is <= 0, check if we should REPLACE existing payment
            if (_remainingAmount <= 0)
            {
                // If there's exactly one payment covering the full amount, allow changing payment mode
                if (_paymentDetailsList.Count == 1 && _paymentDetailsList[0].Amount >= _totalAmount)
                {
                    // Replace the existing payment mode with the new one
                    int newPaymodeId = GetPaymodeIdByName(paymodeName);
                    _paymentDetailsList[0].PaymodeId = newPaymodeId;
                    _paymentDetailsList[0].PaymodeName = paymodeName;
                    RefreshPaymentDisplay();

                    // Focus the Amount cell for editing if needed
                    if (ultraGridPayments.Rows.Count > 0)
                    {
                        ultraGridPayments.ActiveRow = ultraGridPayments.Rows[0];
                        if (ultraGridPayments.Rows[0].Cells.Exists("Amount"))
                        {
                            ultraGridPayments.ActiveCell = ultraGridPayments.Rows[0].Cells["Amount"];
                            ultraGridPayments.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.EnterEditMode);
                        }
                    }
                    return;
                }

                MessageBox.Show("Invoice amount is fully covered. Remove an existing payment to add another.",
                    "Fully Paid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Add new payment with remaining balance
            AddPaymentToGridInternal(paymodeName, _remainingAmount, "");

            // Update the display
            RefreshPaymentDisplay();
        }

        /// <summary>
        /// Internal method to add payment entry
        /// </summary>
        private void AddPaymentToGridInternal(string paymodeName, decimal amount, string reference)
        {
            // Get PaymodeId from name
            int paymodeId = GetPaymodeIdByName(paymodeName);

            var paymentDetail = new SalesPaymentDetail
            {
                CompanyId = ModelClass.SessionContext.CompanyId,
                BranchId = ModelClass.SessionContext.BranchId,
                FinYearId = ModelClass.SessionContext.FinYearId,
                PaymodeId = paymodeId,
                PaymodeName = paymodeName,
                Amount = amount,
                Reference = reference
            };

            _paymentDetailsList.Add(paymentDetail);
            UpdateRemainingAmount();
            RefreshPaymentDisplay();


        }

        /// <summary>
        /// Gets PaymodeId by payment mode name
        /// </summary>
        private int GetPaymodeIdByName(string paymodeName)
        {
            int foundId = 0;

            // Try to find in combo box
            if (cmbPaymodefc.DataSource is DataTable dt)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string name = row["PayModeName"]?.ToString() ?? "";
                    if (name.Equals(paymodeName, StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("Bank" + paymodeName, StringComparison.OrdinalIgnoreCase) ||
                        (paymodeName == "Transfer" && name.Equals("BankTransfer", StringComparison.OrdinalIgnoreCase)))
                    {
                        return Convert.ToInt32(row["PayModeId"]);
                    }
                }
            }

            // Fallback: Query from database directly
            try
            {
                var dp = new Repository.Dropdowns();
                var pm = dp.PaymodeDDl();
                if (pm?.List != null)
                {
                    foreach (var item in pm.List)
                    {
                        if (item.PayModeName.Equals(paymodeName, StringComparison.OrdinalIgnoreCase) ||
                            item.PayModeName.Equals("Bank" + paymodeName, StringComparison.OrdinalIgnoreCase) ||
                            (paymodeName == "Transfer" && item.PayModeName.Equals("BankTransfer", StringComparison.OrdinalIgnoreCase)))
                        {
                            return item.PayModeID;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetPaymodeIdByName: DB lookup failed: {ex.Message}");
            }

            // Default IDs based on YOUR database (PayMode table)
            // 1=Credit, 2=Cash, 3=Card, 4=BankTransfer, 5=UPI, 6=Cheque
            switch (paymodeName.ToLower())
            {
                case "cash": foundId = 2; break;      // Cash = ID 2
                case "card": foundId = 3; break;      // Card = ID 3
                case "transfer":
                case "banktransfer": foundId = 4; break; // BankTransfer = ID 4
                case "upi": foundId = 5; break;          // UPI = ID 5
                default: foundId = 2; break;             // Default to Cash
            }


            return foundId;
        }

        /// <summary>
        /// Refreshes the payment display grid from the payment list (IRS POS Style)
        /// </summary>
        private void RefreshPaymentDisplay()
        {
            // Update the DataGridView from the payment list
            if (_paymentGridSource != null)
            {
                _paymentGridSource.Rows.Clear();

                foreach (var payment in _paymentDetailsList)
                {
                    _paymentGridSource.Rows.Add(payment.PaymodeName, payment.Amount, payment.Reference ?? "");
                }
            }
        }

        /// <summary>
        /// Syncs grid data back to payment list after grid edit
        /// </summary>
        private void SyncGridToPaymentList()
        {
            if (ultraGridPayments == null || _paymentGridSource == null) return;

            for (int i = 0; i < _paymentGridSource.Rows.Count && i < _paymentDetailsList.Count; i++)
            {
                DataRow row = _paymentGridSource.Rows[i];

                // Update amount from grid
                if (decimal.TryParse(row["Amount"]?.ToString(), out decimal amount))
                {
                    _paymentDetailsList[i].Amount = amount;
                }

                // Update reference from grid
                _paymentDetailsList[i].Reference = row["Reference"]?.ToString() ?? "";
            }

            // Recalculate remaining balance
            UpdateRemainingAmount();
        }

        /// <summary>
        /// Event handler for UltraGrid cell value changed - triggers real-time calculation
        /// </summary>
        private void UltraGridPayments_CellChange(object sender, Infragistics.Win.UltraWinGrid.CellEventArgs e)
        {
            try
            {
                // Only handle real-time calculation for Amount column
                if (e.Cell.Column.Key == "Amount")
                {
                    // Get the text currently being typed
                    string currentText = e.Cell.Text?.Trim() ?? "";

                    // Treat empty or invalid as 0
                    decimal currentAmount = 0;
                    if (!string.IsNullOrEmpty(currentText))
                    {
                        decimal.TryParse(currentText, out currentAmount);
                    }

                    // Calculate total of ALL OTHER rows
                    decimal otherRowsTotal = 0;
                    foreach (var row in ultraGridPayments.Rows)
                    {
                        if (row != e.Cell.Row && decimal.TryParse(row.Cells["Amount"].Value?.ToString() ?? "0", out decimal val))
                        {
                            otherRowsTotal += val;
                        }
                    }

                    // Calculate remaining/change
                    decimal newTotalPaid = otherRowsTotal + currentAmount;
                    decimal remaining = _totalAmount - newTotalPaid;

                    // Show actual remaining value:
                    // Negative = still owe money (e.g., -10)
                    // Positive = change to return (e.g., +5)
                    // Zero = exact match
                    if (remaining > 0)
                    {
                        txtBalance.Text = "-" + remaining.ToString("F2");
                        txtBalance.ForeColor = Color.Red; // Red for negative
                    }
                    else if (remaining < 0)
                    {
                        txtBalance.Text = Math.Abs(remaining).ToString("F2");
                        txtBalance.ForeColor = Color.FromArgb(255, 255, 128); // Yellow
                    }
                    else
                    {
                        txtBalance.Text = "0.00";
                        txtBalance.ForeColor = Color.LimeGreen; // Green
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CellChange: {ex.Message}");
            }
        }

        private void UltraGridPayments_AfterCellUpdate(object sender, Infragistics.Win.UltraWinGrid.CellEventArgs e)
        {
            SyncGridToPaymentList();
        }

        /// <summary>
        /// Event handler for UltraGrid key down - Delete key removes row
        /// </summary>
        private void UltraGridPayments_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && ultraGridPayments.ActiveRow != null)
            {
                int selectedIndex = ultraGridPayments.ActiveRow.Index;

                // Don't allow deleting the last payment if it's the only one
                if (_paymentDetailsList.Count == 1)
                {
                    MessageBox.Show("Cannot remove the last payment. At least one payment method is required.",
                        "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Handled = true;
                    return;
                }

                // Remove from list
                if (selectedIndex >= 0 && selectedIndex < _paymentDetailsList.Count)
                {
                    _paymentDetailsList.RemoveAt(selectedIndex);
                    RefreshPaymentDisplay();
                    UpdateRemainingAmount();
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Sets the payment mode in the combobox (legacy, kept for compatibility)
        /// </summary>
        private void SetPaymentModeInCombo(string paymodeName)
        {
            for (int i = 0; i < cmbPaymodefc.Items.Count; i++)
            {
                var item = cmbPaymodefc.Items[i];
                string itemText = item.DisplayText ?? "";
                if (itemText.Equals(paymodeName, StringComparison.OrdinalIgnoreCase) ||
                    (paymodeName == "Transfer" && itemText.Contains("Transfer")))
                {
                    cmbPaymodefc.SelectedIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Updates payment amount when user edits the amount field
        /// IRS POS STYLE: Editing amount updates remaining balance in real-time
        /// </summary>
        public void UpdateCurrentPaymentAmount()
        {
            if (_paymentDetailsList.Count > 0)
            {
                string amountText = TxtPayedAmtfc.Text?.Replace(",", "").Replace(" ", "").Trim() ?? "0";
                if (decimal.TryParse(amountText, out decimal newAmount))
                {
                    // Update the last payment entry
                    _paymentDetailsList[_paymentDetailsList.Count - 1].Amount = newAmount;

                    // Recalculate remaining
                    UpdateRemainingAmount();
                }
            }
        }

        #endregion

        #region Error Handling
        private void ShowError(string message)
        {
            MessageBox.Show(message, "Payment Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowWarning(string message)
        {
            MessageBox.Show(message, "Payment Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        #endregion
    }
}
