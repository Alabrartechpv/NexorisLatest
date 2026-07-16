using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmLastBillDig : Form
    {
        private BaseRepostitory repository;
        private List<Sales_Daily> allBills = new List<Sales_Daily>();
        private readonly string paymentModeFilter;

        public long SelectedBillNo { get; private set; }

        private bool IsDesignerHosted =>
            LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
            (Site?.DesignMode ?? false);

        public frmLastBillDig() : this("Cash")
        {
        }

        public frmLastBillDig(string paymentModeFilter)
        {
            InitializeComponent();
            if (IsDesignerHosted)
            {
                return;
            }

            this.paymentModeFilter = NormalizePaymentMode(paymentModeFilter);
            KeyPreview = true;
            InitializeUi();
            WireEvents();
        }

        private void InitializeUi()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new object[] { "All", "Bill No", "Customer Name", "Payment Mode" });
            comboBox1.SelectedIndex = 0;

            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(new object[] { "Latest First", "Oldest First", "Amount High-Low", "Amount Low-High" });
            comboBox2.SelectedIndex = 0;

            label1.Text = $"{paymentModeFilter} sales";
            textBox3.ReadOnly = true;
            textBox3.Text = "0";
            label4.Text = "Refresh";

            ultraPanel4.Visible = true;
            ConfigureGrid();
        }

        private void WireEvents()
        {
            Load += frmLastBillDig_Load;
            KeyDown += frmLastBillDig_KeyDown;
            textBoxsearch.TextChanged += (s, e) => ApplyFilterAndSort();
            comboBox1.SelectedIndexChanged += (s, e) => ApplyFilterAndSort();
            comboBox2.SelectedIndexChanged += (s, e) => ApplyFilterAndSort();
            ultraGrid1.DoubleClickRow += (s, e) => SelectActiveBill();
            ultraGrid1.KeyDown += ultraGrid1_KeyDown;

            ultraPanel5.Click += (s, e) => SelectActiveBill();
            label5.Click += (s, e) => SelectActiveBill();

            ultraPanel6.Click += (s, e) => CloseWithCancel();
            label3.Click += (s, e) => CloseWithCancel();

            ultraPanel3.Click += MoveUp;
            ultraPanel7.Click += MoveDown;

            ultraPanel4.Click += (s, e) => LoadBills();
            label4.Click += (s, e) => LoadBills();
        }

        private void frmLastBillDig_Load(object sender, EventArgs e)
        {
            if (IsDesignerHosted)
            {
                return;
            }

            LoadBills();
            BeginInvoke(new Action(() =>
            {
                textBoxsearch.Focus();
                textBoxsearch.Select();
            }));
        }

        private void frmLastBillDig_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectActiveBill();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                CloseWithCancel();
                e.Handled = true;
            }
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectActiveBill();
                e.Handled = true;
            }
        }

        private void ConfigureGrid()
        {
            ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
            ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(0, 122, 204);
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(0, 102, 184);
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
            ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(245, 250, 255);
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;
            ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
            ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            ultraGrid1.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;
            ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;

            foreach (UltraGridColumn column in e.Layout.Bands[0].Columns)
            {
                column.Hidden = true;
            }

            ShowColumn(e.Layout.Bands[0], "BillNo", "Bill No", 90, HAlign.Center);
            ShowColumn(e.Layout.Bands[0], "BillDate", "Date", 95, HAlign.Center, "dd-MM-yyyy");
            ShowColumn(e.Layout.Bands[0], "CustomerName", "Customer", 220, HAlign.Left);
            ShowColumn(e.Layout.Bands[0], "PaymodeName", "Payment", 110, HAlign.Left);
            ShowColumn(e.Layout.Bands[0], "NetAmount", "Amount", 110, HAlign.Right, "N2");
            ShowColumn(e.Layout.Bands[0], "Status", "Status", 90, HAlign.Center);
        }

        private void ShowColumn(UltraGridBand band, string key, string caption, int width, HAlign align, string format = null)
        {
            if (!band.Columns.Exists(key)) return;

            UltraGridColumn column = band.Columns[key];
            column.Hidden = false;
            column.Header.Caption = caption;
            column.Width = width;
            column.CellAppearance.TextHAlign = align;
            if (!string.IsNullOrEmpty(format))
            {
                column.Format = format;
            }
        }

        private void LoadBills()
        {
            if (IsDesignerHosted)
            {
                return;
            }

            try
            {
                allBills = GetLastBillsFromDatabase();
                ApplyFilterAndSort();
                label1.Text = $"{paymentModeFilter} sales - {allBills.Count} bill(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading last bills: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<Sales_Daily> GetLastBillsFromDatabase()
        {
            List<Sales_Daily> bills = new List<Sales_Daily>();
            if (IsDesignerHosted)
            {
                return bills;
            }

            if (repository == null)
            {
                repository = new BaseRepostitory();
            }

            if (repository.DataConnection == null)
            {
                return bills;
            }

            string filterSql = string.Empty;

            if (paymentModeFilter == "Credit")
            {
                filterSql = " AND (ISNULL(sm.IsPaid, 0) = 0 OR ISNULL(sm.[Status], '') = 'Pending' OR UPPER(ISNULL(sm.PaymodeName, '')) = 'CREDIT' OR ISNULL(sm.CreditDays, 0) > 0)";
            }
            else if (paymentModeFilter == "Cash")
            {
                filterSql = " AND ISNULL(sm.IsPaid, 0) = 1 AND ISNULL(sm.[Status], '') = 'Complete' AND UPPER(ISNULL(sm.PaymodeName, '')) <> 'CREDIT' AND ISNULL(sm.CreditDays, 0) = 0";
            }

            string sql = @"
SELECT TOP 200
    sm.BillNo,
    sm.BillDate,
    ISNULL(NULLIF(sm.CustomerName, ''), 'Walk In Customer') AS CustomerName,
    ISNULL(sm.PaymodeName, '') AS PaymodeName,
    ISNULL(sm.SubTotal, 0) AS SubTotal,
    ISNULL(sm.NetAmount, 0) AS NetAmount,
    ISNULL(sm.ReceivedAmount, 0) AS ReceivedAmount,
    ISNULL(sm.[Status], '') AS [Status]
FROM SMaster sm
WHERE sm.BranchId = @BranchId
  AND sm.CompanyId = @CompanyId
  AND sm.FinYearId = @FinYearId
  AND (sm.CancelFlag = 0 OR sm.CancelFlag IS NULL)
  AND ISNULL(sm.[Status], '') <> 'Hold'" + filterSql + @"
ORDER BY sm.BillNo DESC";

            try
            {
                repository.DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)repository.DataConnection))
                {
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bills.Add(new Sales_Daily
                            {
                                BillNo = reader["BillNo"] != DBNull.Value ? Convert.ToInt64(reader["BillNo"]) : 0,
                                BillDate = reader["BillDate"] != DBNull.Value ? Convert.ToDateTime(reader["BillDate"]) : DateTime.MinValue,
                                CustomerName = reader["CustomerName"]?.ToString() ?? string.Empty,
                                PaymodeName = reader["PaymodeName"]?.ToString() ?? string.Empty,
                                SubTotal = reader["SubTotal"] != DBNull.Value ? Convert.ToDouble(reader["SubTotal"]) : 0,
                                NetAmount = reader["NetAmount"] != DBNull.Value ? Convert.ToDouble(reader["NetAmount"]) : 0,
                                ReceivedAmount = reader["ReceivedAmount"] != DBNull.Value ? Convert.ToDouble(reader["ReceivedAmount"]) : 0,
                                Status = reader["Status"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }
            finally
            {
                if (repository != null &&
                    repository.DataConnection != null &&
                    repository.DataConnection.State == ConnectionState.Open)
                {
                    repository.DataConnection.Close();
                }
            }

            return bills;
        }

        private void ApplyFilterAndSort()
        {
            IEnumerable<Sales_Daily> query = allBills;
            string searchText = textBoxsearch.Text.Trim().ToLowerInvariant();
            string searchField = comboBox1.SelectedItem?.ToString() ?? "All";
            string sortField = comboBox2.SelectedItem?.ToString() ?? "Latest First";

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(bill =>
                {
                    switch (searchField)
                    {
                        case "Bill No":
                            return bill.BillNo.ToString().Contains(searchText);
                        case "Customer Name":
                            return (bill.CustomerName ?? string.Empty).ToLowerInvariant().Contains(searchText);
                        case "Payment Mode":
                            return (bill.PaymodeName ?? string.Empty).ToLowerInvariant().Contains(searchText);
                        default:
                            return bill.BillNo.ToString().Contains(searchText) ||
                                   (bill.CustomerName ?? string.Empty).ToLowerInvariant().Contains(searchText) ||
                                   (bill.PaymodeName ?? string.Empty).ToLowerInvariant().Contains(searchText);
                    }
                });
            }

            switch (sortField)
            {
                case "Oldest First":
                    query = query.OrderBy(x => x.BillNo);
                    break;
                case "Amount High-Low":
                    query = query.OrderByDescending(x => x.NetAmount).ThenByDescending(x => x.BillNo);
                    break;
                case "Amount Low-High":
                    query = query.OrderBy(x => x.NetAmount).ThenByDescending(x => x.BillNo);
                    break;
                default:
                    query = query.OrderByDescending(x => x.BillNo);
                    break;
            }

            List<Sales_Daily> filtered = query.ToList();
            ultraGrid1.DataSource = filtered;
            textBox3.Text = filtered.Count.ToString();

            if (ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
            }
        }

        private void SelectActiveBill()
        {
            if (ultraGrid1.ActiveRow == null) return;

            object value = ultraGrid1.ActiveRow.Cells["BillNo"].Value;
            if (value == null || value == DBNull.Value) return;

            SelectedBillNo = Convert.ToInt64(value);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void MoveUp(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.Index > 0)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.ActiveRow.Index - 1];
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
            }
        }

        private void MoveDown(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.Index < ultraGrid1.Rows.Count - 1)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.ActiveRow.Index + 1];
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
            }
        }

        private void CloseWithCancel()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private string NormalizePaymentMode(string mode)
        {
            return string.Equals(mode, "Credit", StringComparison.OrdinalIgnoreCase) ? "Credit" : "Cash";
        }

        
    }
}
