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
using ModelClass;
using Repository.Accounts;

namespace PosBranch_Win.DialogBox
{
    /// <summary>
    /// Dialog for selecting a Purchase Return to link to a Debit Note.
    /// Cloned and adapted from frmSalesReturnLookup.
    /// </summary>
    public partial class frmPurchaseReturnLookup : Form
    {
        private PurchaseReturnRepository purchaseReturnRepo = new PurchaseReturnRepository();
        private List<PReturnGetAll> purchaseReturnData;
        private int _vendorLedgerId = 0; // Filter by vendor if > 0
        private int _excludeDebitNoteId = 0; // The current Debit Note ID to exclude from adjusted calculations

        // Event for when a Purchase Return is selected
        public delegate void PurchaseReturnSelectedHandler(int pReturnNo, int ledgerId, string vendorName, string invoiceNo, double grandTotal);
        public event PurchaseReturnSelectedHandler OnPurchaseReturnSelected;

        public frmPurchaseReturnLookup()
        {
            InitializeComponent();
            this.KeyPreview = true;

            SetupGrid();
            SetupEventHandlers();
        }

        /// <summary>
        /// Constructor to filter purchase returns by vendor and handle partial adjustments.
        /// </summary>
        /// <param name="vendorLedgerId">The vendor's LedgerID. Pass 0 to open without a vendor (shows empty grid).</param>
        /// <param name="excludeDebitNoteId">The ID of the Debit Note currently being edited, to exclude its own adjustments from calculations.</param>
        public frmPurchaseReturnLookup(int vendorLedgerId, int excludeDebitNoteId) : this()
        {
            _vendorLedgerId = vendorLedgerId;
            _excludeDebitNoteId = excludeDebitNoteId;
            this.Text = vendorLedgerId > 0
                ? "Purchase Returns — Vendor"
                : "Purchase Returns — No vendor selected";
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

                string[] columnsToShow = { "PReturnNo", "PReturnDate", "InvoiceNo", "VendorName", "GrandTotal" };
                for (int i = 0; i < columnsToShow.Length; i++)
                {
                    if (e.Layout.Bands[0].Columns.Exists(columnsToShow[i]))
                    {
                        var col = e.Layout.Bands[0].Columns[columnsToShow[i]];
                        col.Hidden = false;
                        col.Header.VisiblePosition = i;
                        switch (columnsToShow[i])
                        {
                            case "PReturnNo": col.Header.Caption = "Return No"; col.Width = 80; break;
                            case "PReturnDate": col.Header.Caption = "Date"; col.Width = 100; col.Format = "dd-MM-yyyy"; break;
                            case "InvoiceNo": col.Header.Caption = "Invoice No"; col.Width = 100; break;
                            case "VendorName": col.Header.Caption = "Vendor"; col.Width = 180; col.CellAppearance.TextHAlign = HAlign.Left; break;
                            case "GrandTotal": col.Header.Caption = "Amount"; col.Width = 100; col.Format = "N2"; col.CellAppearance.TextHAlign = HAlign.Right; break;
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
            this.Load += frmPurchaseReturnLookup_Load;
            this.KeyDown += frmPurchaseReturnLookup_KeyDown;
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

        private void frmPurchaseReturnLookup_Load(object sender, EventArgs e)
        {
            LoadPurchaseReturns();
            textBoxsearch.Focus();
        }

        private void LoadPurchaseReturns()
        {
            try
            {
                if (_vendorLedgerId <= 0)
                {
                    // No vendor selected — show nothing and hint the user.
                    purchaseReturnData = new List<PReturnGetAll>();
                    ultraGrid1.DataSource = purchaseReturnData;
                    label1.Text = "Please select a vendor first.";
                    return;
                }

                var debitRepo = new DebitNoteRepository();
                int branchId = Convert.ToInt32(DataBase.BranchId);
                purchaseReturnData = debitRepo.GetAvailablePurchaseReturns(_vendorLedgerId, branchId, _excludeDebitNoteId);

                ultraGrid1.DataSource = purchaseReturnData;

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
                MessageBox.Show($"Error loading purchase returns: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterData()
        {
            if (purchaseReturnData == null) return;

            string searchText = textBoxsearch.Text.Trim().ToLower();
            var filtered = purchaseReturnData.Where(pr =>
                pr.PReturnNo.ToString().Contains(searchText) ||
                (pr.InvoiceNo?.ToLower().Contains(searchText) ?? false) ||
                (pr.VendorName?.ToLower().Contains(searchText) ?? false)
            ).ToList();

            ultraGrid1.DataSource = filtered;

            if (ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
            }
            label1.Text = $"Count: {ultraGrid1.Rows.Count}";
        }

        private void SelectPurchaseReturn()
        {
            if (ultraGrid1.ActiveRow == null) return;

            try
            {
                var pr = ultraGrid1.ActiveRow.ListObject as PReturnGetAll;
                if (pr != null)
                {
                    OnPurchaseReturnSelected?.Invoke(pr.PReturnNo, (int)pr.LedgerID, pr.VendorName, pr.InvoiceNo, (double)pr.GrandTotal);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting purchase return: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (ultraGrid1.Rows.Count == 1) SelectPurchaseReturn();
                else ultraGrid1.Focus();
                e.Handled = true;
            }
        }

        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { SelectPurchaseReturn(); e.Handled = true; }
        }

        private void UltraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e) => SelectPurchaseReturn();
        private void BtnSelect_Click(object sender, EventArgs e) => SelectPurchaseReturn();
        private void BtnCancel_Click(object sender, EventArgs e) { this.DialogResult = DialogResult.Cancel; this.Close(); }

        private void frmPurchaseReturnLookup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { this.DialogResult = DialogResult.Cancel; this.Close(); }
        }
    }
}
