using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using PosBranch_Win.Accounts;
using Repository.Accounts;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PosBranch_Win.DialogBox
{
    public partial class OfficialReceiptList : Form
    {
        private CustomerReceiptInfoRepository receiptRepo = new CustomerReceiptInfoRepository();
        private VendorPaymentRepository paymentRepo = new VendorPaymentRepository();
        private bool isPaymentMode = false;
        private int currentBranchId = ModelClass.SessionContext.BranchId;
        private DataTable fullDataTable = null;
        private string frmName;
        
        // Feature Fields
        private Label lblStatus; // Status label for feedback
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private bool isOriginalOrder = true;

        /// <summary>
        /// In receipt mode: the VoucherId of the row selected by the user.
        /// FrmReceipt reads this after ShowDialog() returns DialogResult.OK.
        /// </summary>
        public long SelectedVoucherId { get; private set; } = 0;

        public OfficialReceiptList()
        {
            InitializeComponent();
            this.KeyPreview = true;
            InitializeStatusLabel(); 
            SetupUltraGridStyle(); 
            SetupColumnChooserMenu();
            InitializeSearchFilterComboBox();
            InitializeColumnOrderComboBox();
            SetupPanelHoverEffects();
            ConnectNavigationPanelEvents();

            this.ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
            this.ultraGrid1.KeyDown += ultraGrid1_KeyDown;
            this.ultraGrid1.DoubleClickRow += ultraGrid1_DoubleClickRow;
            this.textBoxsearch.KeyDown += textBox1_KeyDown;
            this.KeyDown += OfficialReceiptList_KeyDown;

            this.textBoxsearch.GotFocus += textBoxsearch_GotFocus;
            this.textBoxsearch.LostFocus += textBoxsearch_LostFocus;
            this.textBoxsearch.Text = "Search receipts...";

            AddSortToggleIcon();

            this.SizeChanged += Frm_SizeChanged;
            this.LocationChanged += (s, e) => PositionColumnChooserAtBottomRight();
            this.Activated += (s, e) => PositionColumnChooserAtBottomRight();
            this.FormClosing += Frm_FormClosing;
            this.ultraGrid1.Resize += UltraGrid1_Resize;
        }

        public OfficialReceiptList(bool isPayment) : this()
        {
            this.isPaymentMode = isPayment;
            if (this.isPaymentMode)
            {
                this.Text = "Official Payment List";
                this.textBoxsearch.Text = "Search payments...";
            }
        }

        private void OfficialReceiptList_Load(object sender, EventArgs e)
        {
            LoadReceiptData();
            this.BeginInvoke(new Action(() =>
            {
                textBoxsearch.Focus();
                textBoxsearch.Select();
            }));
        }

        private void LoadReceiptData()
        {
            try
            {
                if (isPaymentMode)
                {
                    fullDataTable = paymentRepo.GetAllPayments(currentBranchId);
                }
                else
                {
                    fullDataTable = receiptRepo.GetAllReceipts(currentBranchId);
                }
                if (fullDataTable == null) fullDataTable = new DataTable();

                // Map old SP column names to expected names if needed
                if (fullDataTable.Columns.Contains("Date") && !fullDataTable.Columns.Contains("VoucherDate"))
                    fullDataTable.Columns["Date"].ColumnName = "VoucherDate";
                if (fullDataTable.Columns.Contains("CustomerName") && !fullDataTable.Columns.Contains("LedgerName"))
                    fullDataTable.Columns["CustomerName"].ColumnName = "LedgerName";
                if (fullDataTable.Columns.Contains("VendorName") && !fullDataTable.Columns.Contains("LedgerName"))
                    fullDataTable.Columns["VendorName"].ColumnName = "LedgerName";

                if (fullDataTable.Columns.Contains("TotalAmount"))
                {
                    if (isPaymentMode && !fullDataTable.Columns.Contains("PaymentAmount"))
                        fullDataTable.Columns["TotalAmount"].ColumnName = "PaymentAmount";
                    else if (!isPaymentMode && !fullDataTable.Columns.Contains("ReceiptAmount"))
                        fullDataTable.Columns["TotalAmount"].ColumnName = "ReceiptAmount";
                }

                PreserveOriginalRowOrder(fullDataTable);
                
                ultraGrid1.DataSource = fullDataTable;
                
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("LedgerName"))
                        ultraGrid1.DisplayLayout.Bands[0].Columns["LedgerName"].Width = 250;
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("VoucherID"))
                        ultraGrid1.DisplayLayout.Bands[0].Columns["VoucherID"].Width = 120;
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("BillNo"))
                        ultraGrid1.DisplayLayout.Bands[0].Columns["BillNo"].Width = 120;
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("ReceiptAmount"))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns["ReceiptAmount"].Format = "##,##0.00";
                        ultraGrid1.DisplayLayout.Bands[0].Columns["ReceiptAmount"].CellAppearance.TextHAlign = HAlign.Right;
                    }
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("PaymentAmount"))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns["PaymentAmount"].Format = "##,##0.00";
                        ultraGrid1.DisplayLayout.Bands[0].Columns["PaymentAmount"].CellAppearance.TextHAlign = HAlign.Right;
                    }
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Balance"))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns["Balance"].Format = "##,##0.00";
                        ultraGrid1.DisplayLayout.Bands[0].Columns["Balance"].CellAppearance.TextHAlign = HAlign.Right;
                    }
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("VoucherDate"))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns["VoucherDate"].Format = "dd-MM-yyyy";
                    }

                    // Hide VendorLedgerId/CustomerLedgerId if exists
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("VendorLedgerId"))
                        ultraGrid1.DisplayLayout.Bands[0].Columns["VendorLedgerId"].Hidden = true;
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("CustomerLedgerId"))
                        ultraGrid1.DisplayLayout.Bands[0].Columns["CustomerLedgerId"].Hidden = true;
                }
                
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
                
                InitializeSavedColumnWidths();
                UpdateRecordCountLabel();
                UpdateStatus("Loaded " + fullDataTable.Rows.Count + (isPaymentMode ? " payments." : " receipts."));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        public void RefreshData() => LoadReceiptData();

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

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Hide all columns first
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in e.Layout.Bands[0].Columns)
                {
                    col.Hidden = true;
                }
                
                // Define columns to show (include both ReceiptAmount and PaymentAmount since the column is renamed based on mode)
                string[] columnsToShow = new string[] { "VoucherID", "VoucherDate", "BillNo", "LedgerName", "TotalAmount", "ReceiptAmount", "PaymentAmount", "Balance" };
                for (int i = 0; i < columnsToShow.Length; i++)
                {
                    string colKey = columnsToShow[i];
                    if (e.Layout.Bands[0].Columns.Exists(colKey))
                        e.Layout.Bands[0].Columns[colKey].Hidden = false;
                }
                
                SetColumnFriendlyNames(e.Layout.Bands[0]);
                
                if (e.Layout.Bands[0].Columns.Exists("OriginalRowOrder"))
                    e.Layout.Bands[0].Columns["OriginalRowOrder"].Hidden = true;
            }
            catch (Exception ex)
            {
                UpdateStatus("Error initializing grid layout: " + ex.Message);
            }
        }

        private void SetColumnFriendlyNames(UltraGridBand band)
        {
            if (band.Columns.Exists("VoucherID")) { band.Columns["VoucherID"].Header.Caption = "Voucher"; }
            if (band.Columns.Exists("VoucherDate")) { band.Columns["VoucherDate"].Header.Caption = "Date"; }
            if (band.Columns.Exists("BillNo")) { band.Columns["BillNo"].Header.Caption = "Bill No"; }
            if (band.Columns.Exists("LedgerName")) { band.Columns["LedgerName"].Header.Caption = isPaymentMode ? "Vendor" : "Customer"; }
            if (band.Columns.Exists("TotalAmount")) { band.Columns["TotalAmount"].Header.Caption = "Total"; }
            if (band.Columns.Exists("ReceiptAmount")) { band.Columns["ReceiptAmount"].Header.Caption = "Paid"; }
            if (band.Columns.Exists("PaymentAmount")) { band.Columns["PaymentAmount"].Header.Caption = "Paid"; }
            if (band.Columns.Exists("Balance")) { band.Columns["Balance"].Header.Caption = "Balance"; }
        }

        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            SelectReceipt();
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectReceipt();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void SelectReceipt()
        {
            if (ultraGrid1.ActiveRow != null)
            {
                try
                {
                    if (isPaymentMode)
                    {
                        // Payment mode: return the selected VoucherId to FrmPayment so it can load
                        // the full payment data into the form for editing/viewing.
                        long voucherId = GetRowLongVal("VoucherID");
                        if (voucherId > 0)
                        {
                            SelectedVoucherId = voucherId;
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Cannot load this payment. Invalid Voucher ID.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        // Receipt mode: return the selected VoucherId to FrmReceipt so it can load
                        // the full receipt data into the form for editing/viewing.
                        long voucherId = GetRowLongVal("VoucherID");
                        if (voucherId > 0)
                        {
                            SelectedVoucherId = voucherId;
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Cannot load this receipt. Invalid Voucher ID.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error selecting row: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private int GetRowIntVal(string colName) 
        {
            if (!ultraGrid1.ActiveRow.Cells.Exists(colName)) return 0;
            var val = ultraGrid1.ActiveRow.Cells[colName].Value;
            if (val == null || val == DBNull.Value) return 0;
            if (int.TryParse(val.ToString(), out int res)) return res;
            return 0;
        }

        private long GetRowLongVal(string colName) 
        {
            if (!ultraGrid1.ActiveRow.Cells.Exists(colName)) return 0;
            var val = ultraGrid1.ActiveRow.Cells[colName].Value;
            if (val == null || val == DBNull.Value) return 0;
            if (long.TryParse(val.ToString(), out long res)) return res;
            return 0;
        }

        private string GetRowStringVal(string colName) 
        {
            if (!ultraGrid1.ActiveRow.Cells.Exists(colName)) return string.Empty;
            var val = ultraGrid1.ActiveRow.Cells[colName].Value;
            return val == null ? string.Empty : val.ToString();
        }

        private float GetRowFloatVal(string colName) 
        {
            if (!ultraGrid1.ActiveRow.Cells.Exists(colName)) return 0f;
            var val = ultraGrid1.ActiveRow.Cells[colName].Value;
            if (val == null || val == DBNull.Value) return 0f;
            if (float.TryParse(val.ToString(), out float res)) return res;
            return 0f;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
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
                if (ultraGrid1.Rows.Count == 1)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                    SelectReceipt();
                }
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

        private void FilterReceipts()
        {
            try
            {
                if (fullDataTable == null) return;
                string searchText = textBoxsearch.Text.Trim();
                if (searchText == "Search receipts..." || searchText == "Search payments..." || searchText == "Search receipts by voucher, customer, amount or balance")
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
                        case "Voucher No":
                            if(fullDataTable.Columns.Contains("VoucherID")) filter = "CONVERT(VoucherID, 'System.String') LIKE '%" + escapedSearchText + "%'";
                            break;
                        case "Bill No":
                            if(fullDataTable.Columns.Contains("BillNo")) filter = "CONVERT(BillNo, 'System.String') LIKE '%" + escapedSearchText + "%'";
                            break;
                        case "Customer Name":
                            if(fullDataTable.Columns.Contains("LedgerName")) filter = "LedgerName LIKE '%" + escapedSearchText + "%'";
                            break;
                        case "All":
                        default:
                            List<string> parts = new List<string>();
                            if(fullDataTable.Columns.Contains("VoucherID")) parts.Add("CONVERT(VoucherID, 'System.String') LIKE '%" + escapedSearchText + "%'");
                            if(fullDataTable.Columns.Contains("BillNo")) parts.Add("CONVERT(BillNo, 'System.String') LIKE '%" + escapedSearchText + "%'");
                            if(fullDataTable.Columns.Contains("LedgerName")) parts.Add("LedgerName LIKE '%" + escapedSearchText + "%'");
                            if(fullDataTable.Columns.Contains("ReceiptAmount")) parts.Add("CONVERT(ReceiptAmount, 'System.String') LIKE '%" + escapedSearchText + "%'");
                            if(fullDataTable.Columns.Contains("PaymentAmount")) parts.Add("CONVERT(PaymentAmount, 'System.String') LIKE '%" + escapedSearchText + "%'");
                            if(fullDataTable.Columns.Contains("Balance")) parts.Add("CONVERT(Balance, 'System.String') LIKE '%" + escapedSearchText + "%'");
                            filter = string.Join(" OR ", parts);
                            break;
                    }
                    if(!string.IsNullOrEmpty(filter)) dv.RowFilter = filter;
                    UpdateStatus("Filter applied - Found " + dv.Count + " matching records");
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
                UpdateStatus("Error filtering data: " + ex.Message);
            }
        }

        private void OfficialReceiptList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                textBoxsearch.Focus();
                textBoxsearch.SelectAll();
                e.Handled = true;
            }
        }

        private void textBoxsearch_GotFocus(object sender, EventArgs e)
        {
            if (textBoxsearch.Text == "Search receipts..." || textBoxsearch.Text == "Search payments..." || textBoxsearch.Text == "Search receipts by voucher, customer, amount or balance")
            {
                textBoxsearch.Text = "";
                textBoxsearch.ForeColor = Color.Black;
            }
        }

        private void textBoxsearch_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxsearch.Text))
            {
                textBoxsearch.Text = isPaymentMode ? "Search payments..." : "Search receipts...";
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

        // --- Advanced Grid Styling ---
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
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
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
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ExtendLastColumn;
                ultraGrid1.DisplayLayout.Override.ColumnAutoSizeMode = Infragistics.Win.UltraWinGrid.ColumnAutoSizeMode.None;
            }
            catch (Exception ex)
            {
                UpdateStatus("Error setting up grid style: " + ex.Message);
            }
        }

        // --- Column Chooser Feature ---
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
            };
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;
            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserListBox.ScrollAlwaysVisible = false;
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
                        toolTip.SetToolTip(ultraGrid1, "Drag down to hide '" + columnName + "' column");
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
                        col.Width = savedColumnWidths[col.Key];
                }
                ultraGrid1.ResumeLayout();
                if (columnChooserForm == null || columnChooserForm.IsDisposed)
                    CreateColumnChooserForm();
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
                        columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
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
            PopulateColumnChooserListBox();
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
            string[] standardColumns = new string[] { "VoucherID", "VoucherDate", "BillNo", "LedgerName", "TotalAmount", "ReceiptAmount", "Balance" };
            Dictionary<string, string> displayNames = new Dictionary<string, string>()
            {
                { "VoucherID", "Voucher" },
                { "VoucherDate", "Date" },
                { "BillNo", "Bill No" },
                { "LedgerName", "Customer" },
                { "TotalAmount", "Total" },
                { "ReceiptAmount", "Paid" },
                { "Balance", "Balance" }
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
            if (e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn))) e.Effect = DragDropEffects.Move;
            else e.Effect = DragDropEffects.None;
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
            if (e.Data.GetDataPresent(typeof(ColumnItem))) e.Effect = DragDropEffects.Move;
            else e.Effect = DragDropEffects.None;
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
                        toolTip.Show("Column '" + item.DisplayText + "' restored", ultraGrid1, ultraGrid1.PointToClient(Control.MousePosition), 2000);
                    }
                }
            }
        }

        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            comboBox1.Items.Add("Voucher No");
            comboBox1.Items.Add("Bill No");
            comboBox1.Items.Add("Customer Name");
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            textBoxsearch.TextChanged += TextBoxsearch_TextChanged;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterReceipts();
        }
        private void TextBoxsearch_TextChanged(object sender, EventArgs e)
        {
            FilterReceipts();
        }

        private void InitializeColumnOrderComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Voucher Date");
            comboBox2.Items.Add("Bill No");
            comboBox2.Items.Add("Customer Name");
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedColumn = comboBox2.SelectedItem?.ToString() ?? "Voucher Date";
            ReorderColumns(selectedColumn);
        }

        private void ReorderColumns(string selectedColumn)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;
            Dictionary<string, int> columnWidths = new Dictionary<string, int>();
            foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                if (!col.Hidden) columnWidths[col.Key] = col.Width;
            ultraGrid1.SuspendLayout();
            List<string> columnsToShow = new List<string> { "VoucherID", "VoucherDate", "BillNo", "LedgerName", "TotalAmount", "ReceiptAmount", "Balance" };
            
            string columnKey = "VoucherDate";
            if(selectedColumn == "Bill No") columnKey = "BillNo";
            else if(selectedColumn == "Customer Name") columnKey = "LedgerName";
            
            if (columnsToShow.Contains(columnKey))
            {
                columnsToShow.Remove(columnKey);
                columnsToShow.Insert(0, columnKey);
            }
            
            foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                col.Hidden = true;
                
            for (int i = 0; i < columnsToShow.Count; i++)
            {
                string colKey = columnsToShow[i];
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colKey))
                {
                    var col = ultraGrid1.DisplayLayout.Bands[0].Columns[colKey];
                    col.Hidden = false;
                    col.Header.VisiblePosition = i;
                    if (columnWidths.ContainsKey(colKey)) col.Width = columnWidths[colKey];
                }
            }
            SetColumnFriendlyNames(ultraGrid1.DisplayLayout.Bands[0]);
            ultraGrid1.ResumeLayout();
            ultraGrid1.Refresh();
        }

        private void ConnectNavigationPanelEvents()
        {
            ultraPanel3.Click += MoveRowUp;
            ultraPanel3.ClientArea.Click += MoveRowUp;
            if (this.Controls.Find("ultraPictureBox5", true).Length > 0)
                (this.Controls.Find("ultraPictureBox5", true)[0] as Control).Click += MoveRowUp;
            ultraPanel7.Click += MoveRowDown;
            ultraPanel7.ClientArea.Click += MoveRowDown;
            if (this.Controls.Find("ultraPictureBox6", true).Length > 0)
                (this.Controls.Find("ultraPictureBox6", true)[0] as Control).Click += MoveRowDown;

            ultraPanel5.Click += (s, e) => SelectReceipt();
            label5.Click += (s, e) => SelectReceipt();

            ultraPanel6.Click += (s, e) => this.Close();
            label3.Click += (s, e) => this.Close();

            ultraPanel4.Click += (s, e) => LoadReceiptData();
            label4.Text = "Refresh";
            label4.Click += (s, e) => LoadReceiptData();
            if (this.Controls.Find("ultraPictureBox7", true).Length > 0)
                (this.Controls.Find("ultraPictureBox7", true)[0] as Control).Click += (s, e) => LoadReceiptData();
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

        private void UltraPictureBox4_Click(object sender, EventArgs e)
        {
            if (fullDataTable == null) return;
            isOriginalOrder = !isOriginalOrder;
            DataView dv = fullDataTable.DefaultView;
            if (isOriginalOrder)
            {
                dv.Sort = "OriginalRowOrder ASC";
                UpdateStatus("Displaying records in original order.");
            }
            else
            {
                dv.Sort = "OriginalRowOrder DESC";
                UpdateStatus("Displaying records in reverse order.");
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
                    label1.Text = "Showing " + currentDisplayCount + " of " + totalCount + " records";
                    label1.AutoSize = true;
                    label1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
                    label1.ForeColor = Color.FromArgb(0, 70, 170);
                }
            }
            if (textBox3 != null)
            {
                textBox3.Text = currentDisplayCount.ToString();
            }

            UpdateStatus("Showing " + currentDisplayCount + " of " + totalCount + " records");
        }

        private void SetupPanelHoverEffects()
        {
            SetupPanelGroupHoverEffects(ultraPanel4, label4, this.Controls.Find("ultraPictureBox7", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraPictureBox);
            SetupPanelGroupHoverEffects(ultraPanel6, label3, this.Controls.Find("ultraPictureBox8", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraPictureBox);
            SetupPanelGroupHoverEffects(ultraPanel3, null, this.Controls.Find("ultraPictureBox5", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraPictureBox);
            SetupPanelGroupHoverEffects(ultraPanel7, null, this.Controls.Find("ultraPictureBox6", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraPictureBox);
        }
        
        private void SetupPanelGroupHoverEffects(Infragistics.Win.Misc.UltraPanel panel, Label label, Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox)
        {
            if (panel == null) return;
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
            return Color.FromArgb(color.A, Math.Min(color.R + amount, 255), Math.Min(color.G + amount, 255), Math.Min(color.B + amount, 255));
        }
        
        private bool IsMouseOverControl(Control control)
        {
            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }

        private void AddSortToggleIcon()
        {
            if (this.Controls.Find("ultraPictureBox4", true).FirstOrDefault() != null)
            {
                var pic = this.Controls.Find("ultraPictureBox4", true)[0] as Infragistics.Win.UltraWinEditors.UltraPictureBox;
                var sortImg = (object)null;
                try { sortImg = Properties.Resources.ResourceManager.GetObject("sort_az_16"); } catch { }
                if (sortImg is Image img && pic != null)
                {
                    pic.Appearance.Image = img;
                    pic.Click += UltraPictureBox4_Click;
                    System.Windows.Forms.ToolTip tooltip = new System.Windows.Forms.ToolTip();
                    tooltip.SetToolTip(pic, "Click to toggle between original order and reverse order");
                }
            }
        }
        
        private void Frm_SizeChanged(object sender, EventArgs e)
        {
            PreserveColumnWidths();
            PositionColumnChooserAtBottomRight();
        }
        
        private void UltraGrid1_Resize(object sender, EventArgs e)
        {
            PreserveColumnWidths();
        }
        
        private void Frm_FormClosing(object sender, FormClosingEventArgs e)
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
                UpdateStatus("Error preserving widths: " + ex.Message);
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
