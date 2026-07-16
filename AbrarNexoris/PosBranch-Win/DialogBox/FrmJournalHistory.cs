using System;
using System.Data;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using Repository.Accounts;

namespace PosBranch_Win.DialogBox
{
    public partial class FrmJournalHistory : Form
    {
        private readonly JournalVoucherRepository journalRepository = new JournalVoucherRepository();
        private readonly int branchId;
        private DataTable historyTable;

        public long SelectedVoucherId { get; private set; }

        public FrmJournalHistory(int branchId)
        {
            this.branchId = branchId;
            InitializeComponent();
            ConfigureForm();
        }

        public FrmJournalHistory() : this(0)
        {
        }

        private void ConfigureForm()
        {
            Text = "Journal History";
            label1.Text = string.Empty;

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new object[] { "Voucher", "Date", "Narration" });
            comboBox1.SelectedIndex = 0;
            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(new object[] { "Newest First", "Oldest First", "Voucher No" });
            comboBox2.SelectedIndex = 0;

            label5.Text = "OK";
            label3.Text = "Close";

            textBoxsearch.TextChanged += (sender, args) => ApplyFilter();
            comboBox1.SelectedIndexChanged += (sender, args) => ApplyFilter();
            comboBox2.SelectedIndexChanged += (sender, args) => ApplySort();
            ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
            ultraGrid1.DoubleClick += (sender, args) => SelectCurrentVoucher();
            ultraGrid1.KeyDown += ultraGrid1_KeyDown;
            ultraPanel5.Click += (sender, args) => SelectCurrentVoucher();
            label5.Click += (sender, args) => SelectCurrentVoucher();
            ultraPictureBox1.Click += (sender, args) => SelectCurrentVoucher();
            ultraPanel6.Click += (sender, args) => Close();
            label3.Click += (sender, args) => Close();
            ultraPictureBox2.Click += (sender, args) => Close();

            Load += (sender, args) => LoadHistory();
        }

        private void LoadHistory()
        {
            historyTable = journalRepository.GetVoucherHistory(branchId);
            ultraGrid1.DataSource = historyTable;
            ApplySort();
            UpdateCountLabel();
        }

        private void ApplyFilter()
        {
            if (historyTable == null)
            {
                return;
            }

            string filter = textBoxsearch.Text.Trim().Replace("'", "''");
            if (string.IsNullOrWhiteSpace(filter))
            {
                historyTable.DefaultView.RowFilter = string.Empty;
                UpdateCountLabel();
                return;
            }

            string column = comboBox1.SelectedItem?.ToString();
            if (column == "Date")
            {
                historyTable.DefaultView.RowFilter = $"CONVERT(VoucherDate, 'System.String') LIKE '%{filter}%'";
            }
            else if (column == "Narration")
            {
                historyTable.DefaultView.RowFilter = $"Narration LIKE '%{filter}%'";
            }
            else
            {
                historyTable.DefaultView.RowFilter = $"VoucherNumber LIKE '%{filter}%' OR CONVERT(VoucherID, 'System.String') LIKE '%{filter}%'";
            }

            UpdateCountLabel();
        }

        private void ApplySort()
        {
            if (historyTable == null)
            {
                return;
            }

            string sort = comboBox2.SelectedItem?.ToString();
            historyTable.DefaultView.Sort = sort == "Oldest First"
                ? "VoucherDate ASC, VoucherID ASC"
                : sort == "Voucher No"
                    ? "VoucherNumber ASC"
                    : "VoucherDate DESC, VoucherID DESC";
        }

        private void UpdateCountLabel()
        {
            int count = historyTable?.DefaultView.Count ?? 0;
            label1.Text = $"{count} journal voucher(s)";
        }

        private void SelectCurrentVoucher()
        {
            if (ultraGrid1.ActiveRow == null ||
                ultraGrid1.ActiveRow.Band == null ||
                !ultraGrid1.ActiveRow.Band.Columns.Exists("VoucherID"))
            {
                return;
            }

            object value = ultraGrid1.ActiveRow.Cells["VoucherID"].Value;
            if (value == null || value == DBNull.Value || !long.TryParse(value.ToString(), out long voucherId))
            {
                return;
            }

            SelectedVoucherId = voucherId;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectCurrentVoucher();
                e.Handled = true;
            }
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            e.Layout.CaptionVisible = DefaultableBoolean.False;
            e.Layout.GroupByBox.Hidden = true;
            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            e.Layout.Override.CellClickAction = CellClickAction.RowSelect;
            e.Layout.Override.SelectTypeRow = SelectType.Single;

            UltraGridBand band = e.Layout.Bands[0];
            if (band.Columns.Exists("VoucherID")) band.Columns["VoucherID"].Hidden = true;
            if (band.Columns.Exists("VoucherNumber")) band.Columns["VoucherNumber"].Header.Caption = "Voucher No";
            if (band.Columns.Exists("VoucherDate"))
            {
                band.Columns["VoucherDate"].Header.Caption = "Date";
                band.Columns["VoucherDate"].Format = "dd-MMM-yyyy";
            }
            if (band.Columns.Exists("TotalDebit")) band.Columns["TotalDebit"].Format = "N2";
            if (band.Columns.Exists("TotalCredit")) band.Columns["TotalCredit"].Format = "N2";
        }
    }
}
