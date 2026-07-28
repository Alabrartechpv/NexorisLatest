using Repository.SettingsRepo;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public class frmItemStockHistoryPopup : Form
    {
        private readonly string searchText;
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color border = Color.FromArgb(190, 226, 250);
        private DataGridView gridHistory;

        public frmItemStockHistoryPopup(string searchText)
        {
            this.searchText = searchText;
            InitializeComponent();
            LoadHistory();
        }

        private void InitializeComponent()
        {
            Text = "Item Stock History";
            Size = new Size(1120, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(247, 252, 255);
            Font = new Font("Segoe UI", 9F);

            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(232, 246, 255) };
            headerPanel.Controls.Add(new Label
            {
                Text = $"Stock History for: '{searchText}'",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                ForeColor = navy,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            });

            gridHistory = new DataGridView
            {
                Dock = DockStyle.Fill,
                EnableHeadersVisualStyles = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                GridColor = border,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 30 }
            };
            gridHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 55, 120);
            gridHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            gridHistory.DefaultCellStyle.ForeColor = Color.FromArgb(30, 62, 120);
            gridHistory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 238, 255);
            gridHistory.DefaultCellStyle.SelectionForeColor = navy;
            gridHistory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);
            GridPinningHelper.Attach(gridHistory);
            gridHistory.DataBindingComplete += (s, e) => ApplyActionColors();

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(245, 250, 255) };
            var btnClose = new Button
            {
                Text = "Close",
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(1010, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = navy,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderColor = navy;
            btnClose.Click += (s, e) => Close();
            bottomPanel.Controls.Add(btnClose);
            bottomPanel.Resize += (s, e) => btnClose.Left = bottomPanel.ClientSize.Width - btnClose.Width - 20;

            Controls.Add(gridHistory);
            Controls.Add(headerPanel);
            Controls.Add(bottomPanel);
        }

        private void LoadHistory()
        {
            try
            {
                using (var repo = new ItemStockActivityLogRepository())
                {
                    DataTable data = repo.GetItemStockHistoryLog(searchText);
                    gridHistory.DataSource = BuildDisplayTable(data);
                    ConfigureGrid();
                    ApplyActionColors();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load item stock history: " + ex.Message, "Item Stock History", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGrid()
        {
            if (gridHistory.Columns.Count == 0) return;
            GridPinningHelper.Attach(gridHistory);

            SetColumn("Action", "Action", 220, 0);
            SetColumn("DisplayLogNo", "#", 55, 1);
            SetColumn("CreatedOn", "Date & Time", 155, 2);
            SetColumn("DocNo", "Doc No", 95, 3);
            SetColumn("StockIn", "Stock In", 90, 4);
            SetColumn("StockOut", "Stock Out", 90, 5);
            SetColumn("AdjustmentQty", "Adjustment Qty", 115, 6);
            SetColumn("NewBalance", "New Balance", 105, 7);
            SetColumn("QtyDifference", "Qty Difference", 115, 8);
            SetColumn("Reason", "Reason", 150, 9);
            SetColumn("Stock", "Stock", 100, 10);
            SetColumn("Available", "Available", 100, 11);
            SetColumn("Hold", "Hold", 85, 12);
            SetColumn("Qty", "Qty", 90, 13);
            SetColumn("BillNo", "Bill No", 100, 14);
            SetColumn("PurchaseNo", "Purchase No", 110, 15);
            SetColumn("Counter", "Counter", 160, 16);
            SetColumn("Session", "Session", 90, 17);

            if (gridHistory.Columns.Contains("Action"))
            {
                gridHistory.Columns["Action"].DisplayIndex = 0;
                gridHistory.Columns["Action"].Frozen = true;
            }

            if (gridHistory.Columns.Contains("CreatedOn"))
            {
                gridHistory.Columns["CreatedOn"].DefaultCellStyle.Format = "dd MMM yyyy hh:mm tt";
            }

            foreach (string numericColumn in new[] { "DisplayLogNo", "Qty", "Stock", "StockIn", "StockOut", "AdjustmentQty", "NewBalance", "QtyDifference", "Available", "Hold", "Session" })
            {
                if (gridHistory.Columns.Contains(numericColumn))
                {
                    gridHistory.Columns[numericColumn].DefaultCellStyle.Format = "0.####";
                    gridHistory.Columns[numericColumn].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        private static DataTable BuildDisplayTable(DataTable source)
        {
            DataTable display = new DataTable();
            display.Columns.Add("DisplayLogNo", typeof(int));
            display.Columns.Add("CreatedOn", typeof(DateTime));
            display.Columns.Add("Action", typeof(string));
            display.Columns.Add("DocNo", typeof(string));
            display.Columns.Add("StockIn", typeof(decimal));
            display.Columns.Add("StockOut", typeof(decimal));
            display.Columns.Add("AdjustmentQty", typeof(decimal));
            display.Columns.Add("NewBalance", typeof(decimal));
            display.Columns.Add("QtyDifference", typeof(decimal));
            display.Columns.Add("Reason", typeof(string));
            display.Columns.Add("Stock", typeof(decimal));
            display.Columns.Add("Available", typeof(decimal));
            display.Columns.Add("Hold", typeof(decimal));
            display.Columns.Add("Qty", typeof(decimal));
            display.Columns.Add("BillNo", typeof(string));
            display.Columns.Add("PurchaseNo", typeof(string));
            display.Columns.Add("Counter", typeof(string));
            display.Columns.Add("Session", typeof(long));

            if (source == null)
            {
                return display;
            }

            foreach (DataRow row in source.Rows)
            {
                string action = Convert.ToString(row["Action"]);
                decimal stockIn = ToDecimalAny(row, "StockIn", "Stock In");
                decimal stockOut = ToDecimalAny(row, "StockOut", "Stock Out");
                decimal qtyDifference = stockIn != 0m || stockOut != 0m
                    ? stockIn - stockOut
                    : ToDecimalAny(row, "QtyDifference", "Qty Difference", "MovementQty");
                if (qtyDifference == 0m && string.Equals(action, "Purchase Return", StringComparison.OrdinalIgnoreCase))
                {
                    decimal returnedQty = ToDecimalAny(row, "Returned", "ReturnedQty", "ReturnQty", "Returnqty", "Returned qty");
                    if (returnedQty != 0m)
                    {
                        qtyDifference = 0m - Math.Abs(returnedQty);
                    }
                }
                if (qtyDifference == 0m)
                {
                    qtyDifference = ToDecimalAny(row, "Qty");
                    if (IsStockOutAction(action))
                    {
                        qtyDifference = 0m - qtyDifference;
                    }
                }
                if (stockIn == 0m && IsStockInAction(action) && qtyDifference > 0m)
                {
                    stockIn = qtyDifference;
                }
                if (stockOut == 0m && IsStockOutAction(action) && qtyDifference < 0m)
                {
                    stockOut = Math.Abs(qtyDifference);
                }

                decimal adjustmentQty = ToDecimalAny(row, "AdjustmentQty", "Adjustment Qty");
                if (adjustmentQty == 0m && IsManualStockAction(action))
                {
                    adjustmentQty = qtyDifference;
                }

                decimal itemQty = ToDecimal(row, "Qty");
                if (string.Equals(action, "Purchase Return", StringComparison.OrdinalIgnoreCase))
                {
                    decimal returnedQty = ToDecimalAny(row, "Returned", "ReturnedQty", "ReturnQty", "Returnqty", "Returned qty");
                    if (returnedQty > 0m)
                    {
                        itemQty = returnedQty;
                    }
                }

                display.Rows.Add(
                    ToInt(row, "DisplayLogNo"),
                    ToDateTime(row, "CreatedOn"),
                    FormatDisplayAction(row, action, qtyDifference),
                    FirstText(row, "TransactionNo", "DocNo"),
                    stockIn,
                    stockOut,
                    adjustmentQty,
                    ToDecimalAny(row, "NewBalance", "New Balance", "PhysicalStock"),
                    qtyDifference,
                    FirstText(row, "Reason", "Comments", "Remarks"),
                    ToDecimal(row, "Stock"),
                    ToDecimal(row, "Available"),
                    ToDecimal(row, "Hold"),
                    itemQty,
                    ToText(row, "SalesBillNo"),
                    FormatPurchaseNo(ToText(row, "PurchaseNo")),
                    FirstText(row, "CounterName", "Counter"),
                    ToLong(row, "CounterSessionId"));
            }

            return display;
        }

        private static string FormatDisplayAction(DataRow row, string action, decimal qtyDifference)
        {
            if (!string.Equals(FirstText(row, "ActivityType"), "UPDATE", StringComparison.OrdinalIgnoreCase))
            {
                return action;
            }

            return "Doc " + FirstText(row, "TransactionNo", "DocNo") + " updated (" + action + ")";
        }

        private static bool IsStockInAction(string action)
        {
            return string.Equals(action, "Stock IN", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(action, "Stock In", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(action, "Purchase", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(action, "Sales Return", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsManualStockAction(string action)
        {
            return string.Equals(action, "Stock IN", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(action, "Stock In", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(action, "Stock OUT", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(action, "Stock Out", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStockOutAction(string action)
        {
            return string.Equals(action, "Stock OUT", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(action, "Stock Out", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(action, "Sales", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(action, "Purchase Return", StringComparison.OrdinalIgnoreCase);
        }

        private static string FirstText(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                string value = ToText(row, columnName);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static decimal ToDecimalAny(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (row != null && row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    decimal value;
                    return decimal.TryParse(Convert.ToString(row[columnName]), out value) ? value : 0m;
                }
            }

            return 0m;
        }

        private static string ToText(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert.ToString(row[columnName]);
        }

        private static string FormatPurchaseNo(string purchaseNo)
        {
            if (string.IsNullOrWhiteSpace(purchaseNo))
            {
                return string.Empty;
            }

            return purchaseNo.StartsWith("GRN-", StringComparison.OrdinalIgnoreCase)
                ? purchaseNo
                : "GRN-" + purchaseNo;
        }

        private static decimal ToDecimal(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return 0m;
            }

            decimal value;
            return decimal.TryParse(Convert.ToString(row[columnName]), out value) ? value : 0m;
        }

        private static int ToInt(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return 0;
            }

            int value;
            return int.TryParse(Convert.ToString(row[columnName]), out value) ? value : 0;
        }

        private static DateTime ToDateTime(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return DateTime.MinValue;
            }

            DateTime value;
            return DateTime.TryParse(Convert.ToString(row[columnName]), out value) ? value : DateTime.MinValue;
        }

        private static long ToLong(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return 0;
            }

            long value;
            return long.TryParse(Convert.ToString(row[columnName]), out value) ? value : 0;
        }

        private void SetColumn(string name, string header, int width, int displayIndex)
        {
            if (!gridHistory.Columns.Contains(name)) return;
            var column = gridHistory.Columns[name];
            column.HeaderText = header;
            column.Width = width;
            column.DisplayIndex = displayIndex;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        }

        private void ApplyActionColors()
        {
            if (gridHistory == null || !gridHistory.Columns.Contains("Action")) return;

            foreach (DataGridViewRow row in gridHistory.Rows)
            {
                string action = Convert.ToString(row.Cells["Action"].Value);
                Color color = GetActionColor(action);
                if (color == Color.Empty) continue;

                row.DefaultCellStyle.ForeColor = color;
                if (gridHistory.Columns.Contains("Action"))
                {
                    row.Cells["Action"].Style.ForeColor = color;
                    row.Cells["Action"].Style.Font = new Font(gridHistory.Font, FontStyle.Bold);
                }
            }
        }

        private static Color GetActionColor(string action)
        {
            if (action != null && action.IndexOf("(Purchase)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Color.FromArgb(24, 128, 70);
            }

            if (action != null && action.IndexOf("(Sales)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Color.FromArgb(190, 35, 35);
            }

            if (action != null && action.IndexOf("(Stock IN)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Color.FromArgb(24, 128, 70);
            }

            if (action != null && action.IndexOf("(Stock OUT)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Color.FromArgb(190, 35, 35);
            }

            if (string.Equals(action, "Sales", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(190, 35, 35);
            }

            if (string.Equals(action, "Purchase", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(24, 128, 70);
            }

            if (string.Equals(action, "Sales Return", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(204, 112, 0);
            }

            if (string.Equals(action, "Purchase Return", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(35, 95, 190);
            }

            if (string.Equals(action, "Stock IN", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Stock In", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(24, 128, 70);
            }

            if (string.Equals(action, "Stock OUT", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Stock Out", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(190, 35, 35);
            }

            return Color.Empty;
        }

        private static Color GetActionBackColor(string action)
        {
            if (action != null && action.IndexOf("(Purchase)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Color.FromArgb(235, 250, 241);
            }

            if (action != null && action.IndexOf("(Sales)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Color.FromArgb(255, 238, 238);
            }

            if (action != null && action.IndexOf("(Stock IN)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Color.FromArgb(235, 250, 241);
            }

            if (action != null && action.IndexOf("(Stock OUT)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Color.FromArgb(255, 238, 238);
            }

            if (string.Equals(action, "Sales", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(255, 238, 238);
            }

            if (string.Equals(action, "Purchase", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(235, 250, 241);
            }

            if (string.Equals(action, "Sales Return", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(255, 244, 229);
            }

            if (string.Equals(action, "Purchase Return", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(235, 242, 255);
            }

            if (string.Equals(action, "Stock IN", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Stock In", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(235, 250, 241);
            }

            if (string.Equals(action, "Stock OUT", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Stock Out", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(255, 238, 238);
            }

            return Color.White;
        }
    }
}
