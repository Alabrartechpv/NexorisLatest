using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using Repository.Accounts;
using ModelClass;

namespace PosBranch_Win.Accounts
{
    public partial class FrmGeneralVoucherHistory : Form
    {
        private const string SearchPlaceholder = "Search vouchers...";
        private readonly GeneralVoucherRepository _repository = new GeneralVoucherRepository();
        private readonly string _voucherType;
        private DataTable _historyTable;

        public long SelectedVoucherId { get; private set; }

        public FrmGeneralVoucherHistory(string voucherType)
        {
            _voucherType = voucherType;
            InitializeComponent();
            ApplyModernTheme();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            this.Load += FrmGeneralVoucherHistory_Load;
            btnSelect.Click += (s, e) => SelectVoucher();
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            
            txtSearch.TextChanged += (s, e) => ApplyFilter();
            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text == SearchPlaceholder)
                {
                    txtSearch.Text = "";
                    txtSearch.Appearance.ForeColor = Color.FromArgb(31, 42, 55);
                }
            };
            txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = SearchPlaceholder;
                    txtSearch.Appearance.ForeColor = Color.Gray;
                }
            };
            txtSearch.KeyDown += TxtSearch_KeyDown;

            cmbSearchBy.ValueChanged += (s, e) => ApplyFilter();
            cmbSortBy.ValueChanged += (s, e) => ApplySort();
            
            gridHistory.InitializeLayout += gridHistory_InitializeLayout;
            gridHistory.DoubleClickRow += (s, e) => SelectVoucher();
            gridHistory.KeyDown += gridHistory_KeyDown;

            this.KeyDown += FrmGeneralVoucherHistory_KeyDown;
        }

        private void FrmGeneralVoucherHistory_Load(object sender, EventArgs e)
        {
            // Populate dropdowns
            cmbSearchBy.Items.Clear();
            cmbSearchBy.Items.Add("All", "All");
            cmbSearchBy.Items.Add("Voucher No", "Voucher No");
            cmbSearchBy.Items.Add("Narration", "Narration");
            cmbSearchBy.SelectedIndex = 0;

            cmbSortBy.Items.Clear();
            cmbSortBy.Items.Add("Newest First", "Newest First");
            cmbSortBy.Items.Add("Oldest First", "Oldest First");
            cmbSortBy.SelectedIndex = 0;

            LoadHistory();

            this.BeginInvoke(new Action(() =>
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
            }));
        }

        private void LoadHistory()
        {
            try
            {
                int branchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : Convert.ToInt32(DataBase.BranchId);
                _historyTable = _repository.GetVoucherHistory(_voucherType, branchId);
                gridHistory.DataSource = _historyTable;
                
                ApplySort();
                UpdateCountLabel();

                if (gridHistory.Rows.Count > 0)
                {
                    gridHistory.ActiveRow = gridHistory.Rows[0];
                    gridHistory.Selected.Rows.Clear();
                    gridHistory.Selected.Rows.Add(gridHistory.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading voucher history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilter()
        {
            if (_historyTable == null) return;

            string searchText = txtSearch.Text.Trim();
            if (searchText == SearchPlaceholder) searchText = "";

            string filter = searchText.Replace("'", "''");
            if (string.IsNullOrWhiteSpace(filter))
            {
                _historyTable.DefaultView.RowFilter = string.Empty;
                UpdateCountLabel();
                return;
            }

            string searchCol = cmbSearchBy.Value?.ToString() ?? "All";
            if (searchCol == "Narration")
            {
                _historyTable.DefaultView.RowFilter = $"Narration LIKE '%{filter}%'";
            }
            else if (searchCol == "Voucher No")
            {
                _historyTable.DefaultView.RowFilter = $"VoucherNumber LIKE '%{filter}%' OR CONVERT(VoucherID, 'System.String') LIKE '%{filter}%'";
            }
            else
            {
                _historyTable.DefaultView.RowFilter = $"VoucherNumber LIKE '%{filter}%' OR CONVERT(VoucherID, 'System.String') LIKE '%{filter}%' OR Narration LIKE '%{filter}%'";
            }

            if (gridHistory.Rows.Count > 0)
            {
                gridHistory.ActiveRow = gridHistory.Rows[0];
                gridHistory.Selected.Rows.Clear();
                gridHistory.Selected.Rows.Add(gridHistory.Rows[0]);
            }

            UpdateCountLabel();
        }

        private void ApplySort()
        {
            if (_historyTable == null) return;

            string sort = cmbSortBy.Value?.ToString();
            _historyTable.DefaultView.Sort = sort == "Oldest First"
                ? "VoucherDate ASC, VoucherID ASC"
                : "VoucherDate DESC, VoucherID DESC";
        }

        private void UpdateCountLabel()
        {
            int count = _historyTable?.DefaultView.Count ?? 0;
            string typeLabel = _voucherType == "GENPAY" ? "payment(s)" : "receipt(s)";
            lblCount.Text = $"{count} general {typeLabel} found.";
        }

        private void SelectVoucher()
        {
            if (gridHistory.ActiveRow == null || !gridHistory.ActiveRow.IsDataRow)
                return;

            object value = gridHistory.ActiveRow.Cells["VoucherID"].Value;
            if (value != null && value != DBNull.Value && long.TryParse(value.ToString(), out long voucherId))
            {
                SelectedVoucherId = voucherId;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void gridHistory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectVoucher();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                e.Handled = true;
            }
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
            {
                gridHistory.Focus();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                SelectVoucher();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                e.Handled = true;
            }
        }

        private void FrmGeneralVoucherHistory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                e.Handled = true;
            }
        }

        private void gridHistory_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            e.Layout.CaptionVisible = DefaultableBoolean.False;
            e.Layout.GroupByBox.Hidden = true;
            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            e.Layout.Override.CellClickAction = CellClickAction.RowSelect;
            e.Layout.Override.SelectTypeRow = SelectType.Single;

            // Grid header style
            Color headerBlue = Color.FromArgb(0, 123, 255);
            Color lightBlue = Color.FromArgb(173, 216, 230);

            e.Layout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            e.Layout.Override.HeaderAppearance.BackColor = headerBlue;
            e.Layout.Override.HeaderAppearance.BackColor2 = headerBlue;
            e.Layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
            e.Layout.Override.HeaderAppearance.ForeColor = Color.White;
            e.Layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            e.Layout.Override.HeaderAppearance.TextHAlign = HAlign.Center;

            // Borders
            e.Layout.BorderStyle = UIElementBorderStyle.Solid;
            e.Layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            e.Layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            e.Layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            e.Layout.Override.CellAppearance.BorderColor = lightBlue;
            e.Layout.Override.RowAppearance.BorderColor = lightBlue;
            e.Layout.Override.HeaderAppearance.BorderColor = headerBlue;

            // Row heights & fonts
            e.Layout.Override.MinRowHeight = 30;
            e.Layout.Override.DefaultRowHeight = 30;
            e.Layout.Override.CellAppearance.FontData.SizeInPoints = 9.5F;
            e.Layout.Override.CellAppearance.TextVAlign = VAlign.Middle;

            // Row alternate appearance
            e.Layout.Override.RowAppearance.BackColor = Color.White;
            e.Layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(245, 250, 255);

            // Active / selected row
            e.Layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
            e.Layout.Override.ActiveRowAppearance.ForeColor = Color.White;
            e.Layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
            e.Layout.Override.SelectedRowAppearance.ForeColor = Color.White;

            UltraGridBand band = e.Layout.Bands[0];
            if (band.Columns.Exists("VoucherID")) band.Columns["VoucherID"].Hidden = true;
            
            if (band.Columns.Exists("VoucherNumber"))
            {
                band.Columns["VoucherNumber"].Header.Caption = "Voucher No";
                band.Columns["VoucherNumber"].Width = 120;
            }

            if (band.Columns.Exists("VoucherDate"))
            {
                band.Columns["VoucherDate"].Header.Caption = "Voucher Date";
                band.Columns["VoucherDate"].Format = "dd-MMM-yyyy";
                band.Columns["VoucherDate"].CellAppearance.TextHAlign = HAlign.Center;
                band.Columns["VoucherDate"].Width = 120;
            }

            if (band.Columns.Exists("Narration"))
            {
                band.Columns["Narration"].Header.Caption = "Narration";
                band.Columns["Narration"].Width = 320;
            }

            if (band.Columns.Exists("TotalAmount"))
            {
                band.Columns["TotalAmount"].Header.Caption = "Amount";
                band.Columns["TotalAmount"].Format = "N2";
                band.Columns["TotalAmount"].CellAppearance.TextHAlign = HAlign.Right;
                band.Columns["TotalAmount"].Width = 120;
            }
        }

        private void ApplyModernTheme()
        {
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 9.5F);
            this.MinimumSize = new Size(800, 560);
            this.KeyPreview = true;

            // Title styling
            lblTitle.Appearance.ForeColor = Color.FromArgb(18, 65, 89);
            lblTitle.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblTitle.Appearance.FontData.SizeInPoints = 14F;
            lblTitle.Text = _voucherType == "GENPAY" ? "General Payment History" : "General Receipt History";
            lblTitle.AutoSize = true;

            // Label styles
            lblSearchBy.Appearance.ForeColor = Color.FromArgb(75, 85, 99);
            lblSearchBy.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblSearchBy.AutoSize = true;

            lblSearch.Appearance.ForeColor = Color.FromArgb(75, 85, 99);
            lblSearch.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblSearch.AutoSize = true;

            lblSortBy.Appearance.ForeColor = Color.FromArgb(75, 85, 99);
            lblSortBy.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblSortBy.AutoSize = true;

            lblCount.Appearance.ForeColor = Color.FromArgb(18, 65, 89);
            lblCount.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblCount.AutoSize = true;

            // Flatten Inputs
            txtSearch.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            txtSearch.Text = SearchPlaceholder;
            txtSearch.Appearance.ForeColor = Color.Gray;

            cmbSearchBy.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            cmbSortBy.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;

            // Button Text & Styling
            btnSelect.Text = "Select (OK)";
            btnCancel.Text = "Close (Esc)";

            StyleGradientButton(btnSelect, Color.FromArgb(0, 123, 255), Color.FromArgb(0, 105, 217), Color.FromArgb(0, 90, 190), Color.FromArgb(30, 144, 255), 120);
            StyleGradientButton(btnCancel, Color.FromArgb(108, 117, 125), Color.FromArgb(90, 98, 104), Color.FromArgb(75, 82, 88), Color.FromArgb(130, 140, 150), 120);

            LayoutControls();
            this.SizeChanged += (s, e) => LayoutControls();
        }

        private void StyleGradientButton(UltraButton button, Color backColor, Color backColor2, Color borderColor, Color hoverColor, int width)
        {
            button.UseOsThemes = DefaultableBoolean.False;
            button.UseAppStyling = false;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.Size = new Size(width, 34);
            button.Appearance.BackColor = backColor;
            button.Appearance.BackColor2 = backColor2;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.Appearance.FontData.SizeInPoints = 9.5F;
            button.Appearance.BorderColor = borderColor;
            button.HotTrackAppearance.BackColor = hoverColor;
            button.HotTrackAppearance.ForeColor = Color.White;
            button.HotTrackAppearance.BorderColor = borderColor;
        }

        private void LayoutControls()
        {
            lblTitle.Location = new Point(20, 15);

            int filterTop = 50;
            lblSearchBy.Location = new Point(20, filterTop);
            cmbSearchBy.Location = new Point(20, filterTop + 22);
            cmbSearchBy.Size = new Size(130, 28);

            lblSearch.Location = new Point(cmbSearchBy.Right + 15, filterTop);
            txtSearch.Location = new Point(cmbSearchBy.Right + 15, filterTop + 22);
            txtSearch.Size = new Size(280, 28);

            lblSortBy.Location = new Point(txtSearch.Right + 15, filterTop);
            cmbSortBy.Location = new Point(txtSearch.Right + 15, filterTop + 22);
            cmbSortBy.Size = new Size(140, 28);

            gridHistory.Location = new Point(20, cmbSearchBy.Bottom + 15);
            gridHistory.Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - gridHistory.Top - 70);

            lblCount.Location = new Point(20, gridHistory.Bottom + 18);

            btnCancel.Location = new Point(this.ClientSize.Width - 20 - btnCancel.Width, gridHistory.Bottom + 12);
            btnSelect.Location = new Point(btnCancel.Left - 12 - btnSelect.Width, gridHistory.Bottom + 12);
        }
    }
}
