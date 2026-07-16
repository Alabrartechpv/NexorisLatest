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

namespace PosBranch_Win.DialogBox
{
    /// <summary>
    /// Dialog for selecting a Sales Return to link to a Credit Note.
    /// Refactored to partial class with separate designer.
    /// </summary>
    public partial class frmSalesReturnLookup : Form
    {
        private SalesReturnRepository salesReturnRepo = new SalesReturnRepository();
        private List<SRgetAll> salesReturnData;
        private int _customerLedgerId = 0; // Filter by customer if > 0

        // Event for when a Sales Return is selected
        public delegate void SalesReturnSelectedHandler(int sReturnNo, int ledgerId, string customerName, string invoiceNo, double grandTotal, int paymodeId);
        public event SalesReturnSelectedHandler OnSalesReturnSelected;

        public frmSalesReturnLookup()
        {
            InitializeComponent();
            this.KeyPreview = true;

            SetupGrid();
            SetupEventHandlers();
        }

        /// <summary>
        /// Constructor to filter goods returns by customer (like IRS POS "Goods Return List")
        /// </summary>
        /// <param name="customerLedgerId">The customer's LedgerID.</param>
        public frmSalesReturnLookup(int customerLedgerId) : this()
        {
            _customerLedgerId = customerLedgerId;
            if (customerLedgerId > 0)
            {
                this.Text = "Goods Returns for Customer";
            }
        }

        private void SetupGrid()
        {
            var o = ultraGrid1.DisplayLayout.Override;
            o.AllowAddNew = AllowAddNew.No;
            o.AllowDelete = DefaultableBoolean.False;
            o.AllowUpdate = DefaultableBoolean.False;
            o.RowSelectors = DefaultableBoolean.True;
            o.SelectTypeRow = SelectType.Single;
            o.HeaderClickAction = HeaderClickAction.SortSingle;
            o.CellClickAction = CellClickAction.RowSelect;

            ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
            ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;

            Color headerBlue = Color.FromArgb(0, 123, 255);
            o.MinRowHeight = 30;
            o.DefaultRowHeight = 30;
            o.HeaderAppearance.BackColor = headerBlue;
            o.HeaderAppearance.ForeColor = Color.White;
            o.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            o.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
            o.SelectedRowAppearance.ForeColor = Color.White;
            o.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
            o.ActiveRowAppearance.ForeColor = Color.White;

            ultraGrid1.InitializeLayout += UltraGrid1_InitializeLayout;
        }

        private void UltraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                e.Layout.Bands[0].Columns.Cast<UltraGridColumn>().ToList().ForEach(c => c.Hidden = true);

                string[] columnsToShow = { "SReturnNo", "SReturnDate", "InvoiceNo", "CustomerName", "GrandTotal", "Paymode" };
                for (int i = 0; i < columnsToShow.Length; i++)
                {
                    if (e.Layout.Bands[0].Columns.Exists(columnsToShow[i]))
                    {
                        var col = e.Layout.Bands[0].Columns[columnsToShow[i]];
                        col.Hidden = false;
                        col.Header.VisiblePosition = i;
                        switch (columnsToShow[i])
                        {
                            case "SReturnNo": col.Header.Caption = "Return No"; col.Width = 80; break;
                            case "SReturnDate": col.Header.Caption = "Date"; col.Width = 100; col.Format = "dd-MM-yyyy"; break;
                            case "InvoiceNo": col.Header.Caption = "Invoice No"; col.Width = 100; break;
                            case "CustomerName": col.Header.Caption = "Customer"; col.Width = 180; col.CellAppearance.TextHAlign = HAlign.Left; break;
                            case "GrandTotal": col.Header.Caption = "Amount"; col.Width = 100; col.Format = "N2"; col.CellAppearance.TextHAlign = HAlign.Right; break;
                            case "Paymode": col.Header.Caption = "Mode"; col.Width = 80; break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Grid layout error: {ex.Message}");
            }
        }

        private void SetupEventHandlers()
        {
            this.Load += frmSalesReturnLookup_Load;
            this.KeyDown += frmSalesReturnLookup_KeyDown;
            textBoxsearch.TextChanged += TextBoxSearch_TextChanged;
            textBoxsearch.KeyDown += TextBoxSearch_KeyDown;
            ultraGrid1.DoubleClickRow += UltraGrid1_DoubleClickRow;
            ultraGrid1.KeyDown += UltraGrid1_KeyDown;

            // OK button
            ultraPanel5.Click += BtnSelect_Click;
            ultraPictureBox1.Click += BtnSelect_Click;
            label5.Click += BtnSelect_Click;

            // Close button
            ultraPanel6.Click += BtnCancel_Click;
            ultraPictureBox2.Click += BtnCancel_Click;
            label3.Click += BtnCancel_Click;
        }

        private void frmSalesReturnLookup_Load(object sender, EventArgs e)
        {
            LoadSalesReturns();
            textBoxsearch.Focus();
        }

        private void LoadSalesReturns()
        {
            try
            {
                salesReturnData = salesReturnRepo.GetPendingByCustomer(_customerLedgerId)
                    .OrderByDescending(sr => sr.SReturnNo)
                    .ToList();

                ultraGrid1.DataSource = salesReturnData;

                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
                label1.Text = $"Count: {ultraGrid1.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales returns: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterData()
        {
            if (salesReturnData == null) return;

            string searchText = textBoxsearch.Text.Trim().ToLower();
            var filtered = salesReturnData.Where(sr =>
                sr.SReturnNo.ToString().Contains(searchText) ||
                (sr.InvoiceNo?.ToLower().Contains(searchText) ?? false) ||
                (sr.CustomerName?.ToLower().Contains(searchText) ?? false)
            ).ToList();

            ultraGrid1.DataSource = filtered;

            if (ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
            }
            label1.Text = $"Count: {ultraGrid1.Rows.Count}";
        }

        private void SelectSalesReturn()
        {
            if (ultraGrid1.ActiveRow == null) return;

            try
            {
                var sr = ultraGrid1.ActiveRow.ListObject as SRgetAll;
                if (sr != null)
                {
                    int finalLedgerId = (int)sr.LedgerID;
                    if (finalLedgerId == 0)
                    {
                        var fullReturn = salesReturnRepo.GetBySalesReturnId(sr.SReturnNo);
                        if (fullReturn != null)
                        {
                            finalLedgerId = (int)fullReturn.LedgerID;
                        }
                    }

                    OnSalesReturnSelected?.Invoke(sr.SReturnNo, finalLedgerId, sr.CustomerName, sr.InvoiceNo, (double)sr.GrandTotal, sr.PaymodeID);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting sales return: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TextBoxSearch_TextChanged(object sender, EventArgs e) => FilterData();

        private void TextBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.Focus();
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && ultraGrid1.Rows.Count > 0)
            {
                if (ultraGrid1.Rows.Count == 1) SelectSalesReturn();
                else ultraGrid1.Focus();
                e.Handled = true;
            }
        }

        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { SelectSalesReturn(); e.Handled = true; }
        }

        private void UltraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e) => SelectSalesReturn();
        private void BtnSelect_Click(object sender, EventArgs e) => SelectSalesReturn();
        private void BtnCancel_Click(object sender, EventArgs e) { this.DialogResult = DialogResult.Cancel; this.Close(); }

        private void frmSalesReturnLookup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { this.DialogResult = DialogResult.Cancel; this.Close(); }
        }
    }
}
