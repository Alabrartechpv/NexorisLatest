using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using Repository.Accounts;

namespace PosBranch_Win.DialogBox
{
    /// <summary>
    /// Popup dialog that lists all active payment vouchers for a vendor grouped by GRN number.
    /// The user can select individual vouchers (or all) and cancel them in bulk.
    /// </summary>
    public class FrmGrnPaymentCancel : Form
    {
        // ─── Constants ────────────────────────────────────────────────────────
        private const string ColSelect          = "Select";
        private const string ColGrnNo           = "GrnNo";
        private const string ColVoucherNo       = "VoucherNo";
        private const string ColVoucherDate     = "VoucherDate";
        private const string ColPaymentAmount   = "PaymentAmount";
        private const string ColPaymentMasterId = "PaymentMasterId";
        private const string ColVendorLedgerId  = "VendorLedgerId";

        private const string MsgNoVouchers   = "No active payment vouchers found for this vendor.";
        private const string MsgNoneSelected = "Please select at least one payment voucher to cancel.";
        private const string MsgConfirmTitle = "Confirm Cancellation";
        private const string MsgSuccessTitle = "Cancellation Successful";
        private const string MsgErrorTitle   = "Cancellation Error";
        private const string CancelReason    = "Cancelled from GRN Payment Cancel screen";

        // ─── State ────────────────────────────────────────────────────────────
        private readonly int    _vendorLedgerId;
        private readonly string _vendorName;
        private readonly int    _branchId;
        private readonly int    _userId;
        private readonly VendorPaymentRepository _repo;

        /// <summary>GRN numbers whose payments were successfully cancelled.</summary>
        public List<string> CancelledGrnNumbers { get; private set; } = new List<string>();

        // ─── Controls ─────────────────────────────────────────────────────────
        private UltraGrid  _grid;
        private Button     _btnSelectAll;
        private Button     _btnDeselectAll;
        private Button     _btnCancel;
        private Button     _btnClose;
        private Button     _btnSearch;
        private Label      _lblTitle;
        private Label      _lblStatus;
        private Label      _lblSearch;
        private TextBox    _txtSearch;
        private Panel      _toolbarPanel;
        private Panel      _bottomPanel;

        // Full unfiltered data table (for resetting search)
        private DataTable  _fullData;

        // ─── Constructor ──────────────────────────────────────────────────────
        public FrmGrnPaymentCancel(
            int vendorLedgerId,
            string vendorName,
            int branchId,
            int userId,
            VendorPaymentRepository repo)
        {
            _vendorLedgerId = vendorLedgerId;
            _vendorName     = vendorName;
            _branchId       = branchId;
            _userId         = userId;
            _repo           = repo;

            BuildUI();
            this.Load += (s, e) => LoadData();
        }

        // ─── UI Construction ──────────────────────────────────────────────────
        private void BuildUI()
        {
            this.Text            = $"Cancel GRN Payments — {_vendorName}";
            this.Size            = new Size(820, 540);
            this.MinimumSize     = new Size(760, 420);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor       = Color.White;
            this.Font            = new Font("Segoe UI", 9.5f);

            // Title bar
            _lblTitle = new Label
            {
                Text      = $"Cancel GRN Payment Vouchers  ·  Vendor: {_vendorName}",
                Dock      = DockStyle.Top,
                Height    = 38,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 114, 198),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(10, 0, 0, 0)
            };

            // Toolbar (Select All / Deselect All)
            _toolbarPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 40,
                BackColor = Color.FromArgb(240, 245, 250),
                Padding   = new Padding(6, 6, 6, 4)
            };

            _btnSelectAll = MakeToolbarButton("Select All",   Color.FromArgb(0, 122, 204));
            _btnDeselectAll = MakeToolbarButton("Deselect All", Color.FromArgb(120, 130, 140));
            _btnSelectAll.Click   += BtnSelectAll_Click;
            _btnDeselectAll.Click += BtnDeselectAll_Click;

            // Search controls
            _lblSearch = new Label
            {
                Text      = "GRN No:",
                AutoSize  = true,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(50, 50, 50),
                Top       = 10
            };

            _txtSearch = new TextBox
            {
                Width  = 140,
                Height = 26,
                Top    = 6,
                Font   = new Font("Segoe UI", 9.5f),
                Text   = "GRN-5 or 5"
            };
            // Simulate placeholder: clear on focus, restore if empty
            _txtSearch.ForeColor = Color.Gray;
            _txtSearch.GotFocus  += (s, e) => { if (_txtSearch.ForeColor == Color.Gray) { _txtSearch.Text = string.Empty; _txtSearch.ForeColor = SystemColors.WindowText; } };
            _txtSearch.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_txtSearch.Text)) { _txtSearch.Text = "GRN-5 or 5"; _txtSearch.ForeColor = Color.Gray; } };
            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SearchData(); };

            _btnSearch = MakeToolbarButton("Search", Color.FromArgb(0, 150, 136));
            _btnSearch.Click += (s, e) => SearchData();

            var _btnClearSearch = MakeToolbarButton("Clear", Color.FromArgb(160, 90, 40));
            _btnClearSearch.Width = 70;
            _btnClearSearch.Click += (s, e) => { _txtSearch.Text = string.Empty; SearchData(); };

            _toolbarPanel.Controls.Add(_btnDeselectAll);
            _toolbarPanel.Controls.Add(_btnSelectAll);
            _toolbarPanel.Controls.Add(_lblSearch);
            _toolbarPanel.Controls.Add(_txtSearch);
            _toolbarPanel.Controls.Add(_btnSearch);
            _toolbarPanel.Controls.Add(_btnClearSearch);

            _btnSelectAll.Left   = 6;
            _btnDeselectAll.Left = _btnSelectAll.Right + 8;
            _lblSearch.Left      = _btnDeselectAll.Right + 20;
            _lblSearch.Top       = 12;
            _txtSearch.Left      = _lblSearch.Right + 4;
            _btnSearch.Left      = _txtSearch.Right + 6;
            _btnClearSearch.Left = _btnSearch.Right + 4;

            // Grid
            _grid = new UltraGrid
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.White
            };
            _grid.InitializeLayout += Grid_InitializeLayout;

            // Status label
            _lblStatus = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 22,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(80, 80, 80),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(10, 0, 0, 0),
                BackColor = Color.FromArgb(245, 248, 252)
            };

            // Bottom action panel
            _bottomPanel = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 48,
                BackColor = Color.FromArgb(240, 245, 250),
                Padding   = new Padding(6, 6, 10, 6)
            };

            _btnCancel = MakeActionButton("Cancel Selected Bill(s)", Color.FromArgb(192, 0, 0), Color.White);
            _btnClose  = MakeActionButton("Close",                   Color.FromArgb(200, 200, 200), Color.Black);
            _btnCancel.Click += BtnCancelBills_Click;
            _btnClose.Click  += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // Right-align buttons
            _btnClose.Anchor  = AnchorStyles.Right | AnchorStyles.Top;
            _btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Top;

            _bottomPanel.Controls.Add(_btnClose);
            _bottomPanel.Controls.Add(_btnCancel);

            // Position right-aligned manually after panel created
            this.Shown += (s, e) =>
            {
                _btnClose.Left  = _bottomPanel.ClientSize.Width - _btnClose.Width - 8;
                _btnCancel.Left = _btnClose.Left - _btnCancel.Width - 8;
            };
            _bottomPanel.SizeChanged += (s, e) =>
            {
                _btnClose.Left  = _bottomPanel.ClientSize.Width - _btnClose.Width - 8;
                _btnCancel.Left = _btnClose.Left - _btnCancel.Width - 8;
            };

            // Compose
            this.Controls.Add(_grid);
            this.Controls.Add(_toolbarPanel);
            this.Controls.Add(_lblTitle);
            this.Controls.Add(_lblStatus);
            this.Controls.Add(_bottomPanel);
        }

        private static Button MakeToolbarButton(string text, Color back)
        {
            return new Button
            {
                Text      = text,
                Width     = 110,
                Height    = 28,
                Top       = 5,
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
        }

        private static Button MakeActionButton(string text, Color back, Color fore)
        {
            return new Button
            {
                Text      = text,
                Width     = 180,
                Height    = 34,
                Top       = 7,
                BackColor = back,
                ForeColor = fore,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
        }

        // ─── Grid Layout ──────────────────────────────────────────────────────
        private void Grid_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var layout = e.Layout;
            layout.Override.AllowAddNew      = AllowAddNew.No;
            layout.Override.AllowDelete      = DefaultableBoolean.False;
            layout.Override.AllowUpdate      = DefaultableBoolean.True;
            layout.Override.RowSelectors     = DefaultableBoolean.False;
            layout.Override.SelectTypeRow    = SelectType.Single;
            layout.Override.CellClickAction  = CellClickAction.EditAndSelectText;
            layout.AutoFitStyle              = AutoFitStyle.None;
            layout.ScrollBounds              = ScrollBounds.ScrollToFill;

            // Header style
            layout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            layout.Override.HeaderAppearance.BackColor          = Color.FromArgb(0, 114, 198);
            layout.Override.HeaderAppearance.BackColor2         = Color.FromArgb(0, 90, 170);
            layout.Override.HeaderAppearance.BackGradientStyle  = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor          = Color.White;
            layout.Override.HeaderAppearance.TextHAlign         = HAlign.Center;
            layout.Override.HeaderAppearance.FontData.Bold      = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            // Row styling
            layout.Override.RowAppearance.BackColor           = Color.White;
            layout.Override.RowAlternateAppearance.BackColor  = Color.FromArgb(240, 248, 255);
            layout.Override.ActiveRowAppearance.BackColor     = Color.LightSkyBlue;
            layout.Override.ActiveRowAppearance.ForeColor     = Color.Black;
            layout.Override.CellPadding                       = 4;
            layout.Override.DefaultRowHeight                  = 26;
            layout.Override.BorderStyleCell                   = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow                    = UIElementBorderStyle.Solid;

            if (layout.Bands.Count == 0) return;
            var band = layout.Bands[0];

            // Select checkbox column
            if (band.Columns.Exists(ColSelect))
            {
                band.Columns[ColSelect].Style            = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                band.Columns[ColSelect].Width            = 55;
                band.Columns[ColSelect].Header.Caption   = "Select";
                band.Columns[ColSelect].CellAppearance.TextHAlign = HAlign.Center;
                band.Columns[ColSelect].Header.Appearance.TextHAlign = HAlign.Center;
            }

            // GRN No
            if (band.Columns.Exists(ColGrnNo))
            {
                band.Columns[ColGrnNo].Header.Caption   = "GRN No";
                band.Columns[ColGrnNo].Width            = 90;
                band.Columns[ColGrnNo].CellActivation   = Activation.NoEdit;
                band.Columns[ColGrnNo].CellAppearance.TextHAlign = HAlign.Center;
                band.Columns[ColGrnNo].Header.Appearance.TextHAlign = HAlign.Center;
            }

            // Voucher No
            if (band.Columns.Exists(ColVoucherNo))
            {
                band.Columns[ColVoucherNo].Header.Caption  = "Voucher No";
                band.Columns[ColVoucherNo].Width           = 150;
                band.Columns[ColVoucherNo].CellActivation  = Activation.NoEdit;
            }

            // Voucher Date
            if (band.Columns.Exists(ColVoucherDate))
            {
                band.Columns[ColVoucherDate].Header.Caption = "Voucher Date";
                band.Columns[ColVoucherDate].Width          = 110;
                band.Columns[ColVoucherDate].CellActivation = Activation.NoEdit;
                band.Columns[ColVoucherDate].Format         = "dd-MM-yyyy";
                band.Columns[ColVoucherDate].CellAppearance.TextHAlign = HAlign.Center;
                band.Columns[ColVoucherDate].Header.Appearance.TextHAlign = HAlign.Center;
            }

            // Payment Amount
            if (band.Columns.Exists(ColPaymentAmount))
            {
                band.Columns[ColPaymentAmount].Header.Caption  = "Payment Amount";
                band.Columns[ColPaymentAmount].Width           = 130;
                band.Columns[ColPaymentAmount].CellActivation  = Activation.NoEdit;
                band.Columns[ColPaymentAmount].Format          = "##,##0.00";
                band.Columns[ColPaymentAmount].CellAppearance.TextHAlign = HAlign.Right;
                band.Columns[ColPaymentAmount].Header.Appearance.TextHAlign = HAlign.Center;
            }

            // Hidden columns
            if (band.Columns.Exists(ColPaymentMasterId))
                band.Columns[ColPaymentMasterId].Hidden = true;
            if (band.Columns.Exists(ColVendorLedgerId))
                band.Columns[ColVendorLedgerId].Hidden  = true;
        }

        // ─── Data Loading ─────────────────────────────────────────────────────
        private void LoadData()
        {
            try
            {
                DataTable raw = _repo.GetActiveVouchersByVendor(_vendorLedgerId, _branchId);

                // Build display table with Select column
                DataTable dt = new DataTable();
                dt.Columns.Add(ColSelect,          typeof(bool));
                dt.Columns.Add(ColGrnNo,           typeof(string));
                dt.Columns.Add(ColVoucherNo,       typeof(string));
                dt.Columns.Add(ColVoucherDate,     typeof(DateTime));
                dt.Columns.Add(ColPaymentAmount,   typeof(decimal));
                dt.Columns.Add(ColPaymentMasterId, typeof(int));
                dt.Columns.Add(ColVendorLedgerId,  typeof(int));

                foreach (DataRow row in raw.Rows)
                {
                    int grnNo = row["GrnNo"] != DBNull.Value ? Convert.ToInt32(row["GrnNo"]) : 0;
                    dt.Rows.Add(
                        false,
                        grnNo > 0 ? $"GRN-{grnNo}" : row["GrnNo"]?.ToString(),
                        row["VoucherNo"]?.ToString(),
                        row["VoucherDate"] != DBNull.Value ? Convert.ToDateTime(row["VoucherDate"]) : (object)DBNull.Value,
                        row["PaymentAmount"] != DBNull.Value ? Convert.ToDecimal(row["PaymentAmount"]) : 0m,
                        row["PaymentMasterId"] != DBNull.Value ? Convert.ToInt32(row["PaymentMasterId"]) : 0,
                        row["VendorLedgerId"] != DBNull.Value ? Convert.ToInt32(Convert.ToDecimal(row["VendorLedgerId"])) : 0
                    );
                }

                _fullData        = dt;
                _grid.DataSource = null;
                _grid.DataSource = dt;

                UpdateStatusLabel();

                if (dt.Rows.Count == 0)
                {
                    _lblStatus.Text      = MsgNoVouchers;
                    _lblStatus.ForeColor = Color.OrangeRed;
                    _btnCancel.Enabled   = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading GRN payment data: " + ex.Message,
                    MsgErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatusLabel()
        {
            if (_grid.Rows == null) return;
            int total    = _grid.Rows.Count;
            int selected = _grid.Rows.Count(r =>
                r.Cells.Exists(ColSelect) &&
                r.Cells[ColSelect].Value != null &&
                Convert.ToBoolean(r.Cells[ColSelect].Value));
            _lblStatus.Text      = $"{total} voucher(s) — {selected} selected";
            _lblStatus.ForeColor = Color.FromArgb(60, 60, 60);
        }

        // ─── Search ───────────────────────────────────────────────────────────
        private void SearchData()
        {
            if (_fullData == null) return;

            // Ignore placeholder text
            string term = _txtSearch.ForeColor == Color.Gray
                ? string.Empty
                : _txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(term))
            {
                _grid.DataSource = null;
                _grid.DataSource = _fullData;
                UpdateStatusLabel();
                return;
            }

            // Strip "grn-" prefix so user can type "5" or "GRN-5"
            string numericTerm = term.StartsWith("grn-") ? term.Substring(4) : term;

            DataTable filtered = _fullData.Clone();
            foreach (DataRow row in _fullData.Rows)
            {
                string grnVal = row[ColGrnNo]?.ToString()?.ToLower() ?? string.Empty;
                // grnVal is like "grn-5" — match on full value OR numeric part
                string grnNumeric = grnVal.StartsWith("grn-") ? grnVal.Substring(4) : grnVal;

                if (grnVal.Contains(term) || grnNumeric.Contains(numericTerm))
                    filtered.ImportRow(row);
            }

            _grid.DataSource = null;
            _grid.DataSource = filtered;
            UpdateStatusLabel();
        }

        // ─── Button Handlers ──────────────────────────────────────────────────
        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            SetAllCheckboxes(true);
        }

        private void BtnDeselectAll_Click(object sender, EventArgs e)
        {
            SetAllCheckboxes(false);
        }

        private void SetAllCheckboxes(bool value)
        {
            foreach (UltraGridRow row in _grid.Rows)
            {
                if (row.Cells.Exists(ColSelect))
                    row.Cells[ColSelect].Value = value;
            }
            _grid.UpdateData();
            UpdateStatusLabel();
        }

        private void BtnCancelBills_Click(object sender, EventArgs e)
        {
            // Collect selected rows
            var selectedRows = _grid.Rows
                .Where(r => r.Cells.Exists(ColSelect) &&
                            r.Cells[ColSelect].Value != null &&
                            Convert.ToBoolean(r.Cells[ColSelect].Value))
                .ToList();

            if (selectedRows.Count == 0)
            {
                MessageBox.Show(MsgNoneSelected, MsgConfirmTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Build confirm message
            decimal totalAmount = selectedRows.Sum(r =>
                r.Cells.Exists(ColPaymentAmount) && r.Cells[ColPaymentAmount].Value != null
                    ? Convert.ToDecimal(r.Cells[ColPaymentAmount].Value) : 0m);

            string grnList = string.Join(", ", selectedRows
                .Select(r => r.Cells[ColGrnNo].Value?.ToString())
                .Where(g => !string.IsNullOrEmpty(g))
                .Distinct());

            DialogResult confirm = MessageBox.Show(
                $"Cancel {selectedRows.Count} payment voucher(s) for:\n{grnList}\n\n" +
                $"Total amount reversed: {totalAmount:N2}\n\n" +
                "This action cannot be undone.",
                MsgConfirmTitle,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            // Execute cancellations
            var errors          = new List<string>();
            var cancelledGrns   = new List<string>();
            int cancelledCount  = 0;

            _btnCancel.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                foreach (UltraGridRow row in selectedRows)
                {
                    int paymentMasterId = row.Cells.Exists(ColPaymentMasterId) &&
                                         row.Cells[ColPaymentMasterId].Value != null
                        ? Convert.ToInt32(row.Cells[ColPaymentMasterId].Value)
                        : 0;

                    string grnLabel = row.Cells[ColGrnNo].Value?.ToString() ?? "?";

                    if (paymentMasterId <= 0)
                    {
                        errors.Add($"{grnLabel}: Invalid PaymentMasterId");
                        continue;
                    }

                    try
                    {
                        _repo.CancelVendorPayment(paymentMasterId, _branchId, _userId, CancelReason);
                        cancelledCount++;
                        if (!cancelledGrns.Contains(grnLabel))
                            cancelledGrns.Add(grnLabel);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{grnLabel}: {ex.Message}");
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
                _btnCancel.Enabled = true;
            }

            // Report results
            if (errors.Count > 0)
            {
                string errSummary = cancelledCount > 0
                    ? $"Cancelled {cancelledCount} voucher(s).\n\nErrors:\n{string.Join("\n", errors)}"
                    : $"No vouchers were cancelled.\n\nErrors:\n{string.Join("\n", errors)}";

                MessageBox.Show(errSummary, MsgErrorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(
                    $"Successfully cancelled {cancelledCount} payment voucher(s).\n\n" +
                    $"GRN(s) affected: {string.Join(", ", cancelledGrns)}",
                    MsgSuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (cancelledCount > 0)
            {
                CancelledGrnNumbers = cancelledGrns;
                this.DialogResult   = DialogResult.OK;
                this.Close();
            }
            else
            {
                // Refresh grid to reflect any partial changes
                LoadData();
            }
        }
    }
}
