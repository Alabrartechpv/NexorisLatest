using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;
using Repository.SettingsRepo;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public class ItemStockActivity : Form
    {
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color border = Color.FromArgb(190, 226, 250);
        private readonly Color skyBlueOutline = Color.FromArgb(128, 183, 220);
        private bool applyButtonHot;
        private bool applyButtonPressed;
        private DataTable currentData;
        private Timer pollTimer;
        private DateTime lastKnownActivityStamp = DateTime.MinValue;
        private bool isLoadingActivityLog;

        private UltraComboEditor cmbQuickDate;
        private UltraDateTimeEditor dtpFrom;
        private UltraDateTimeEditor dtpTo;
        private UltraComboEditor cmbUser;
        private UltraComboEditor cmbActivityType;
        private UltraTextEditor txtItemSearch;
        private UltraComboEditor cmbAction;
        private Button btnApply;
        private Button btnReset;
        private Button btnExport;
        private DataGridView gridActivity;
        private Label lblTotal;
        private Label lblToday;
        private Label lblMonth;
        private Label lblYear;
        private Label lblShowing;

        public ItemStockActivity()
        {
            InitializeLogUi();
            StyleGrid();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadFilterLists();
            cmbQuickDate.Text = "Today";
            ApplyQuickDate();
            LoadActivityLog();

            pollTimer = new Timer { Interval = 2000 };
            pollTimer.Tick += PollForNewActivityLogs;
            pollTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            pollTimer?.Stop();
            pollTimer?.Dispose();
            base.OnFormClosed(e);
        }

        private void InitializeLogUi()
        {
            Text = "Activity Log - Item Stock";
            Name = "ItemStockActivity";
            BackColor = Color.FromArgb(247, 252, 255);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.White, Padding = new Padding(8) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var filterPanel = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 250, 255),
                Padding = new Padding(12),
                BorderColor = Color.FromArgb(176, 224, 255),
                BorderRadius = 8
            };

            var filters = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Color.Transparent };
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            filters.Controls.Add(new Label
            {
                Text = "Filters",
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = navy,
                TextAlign = ContentAlignment.MiddleLeft
            });

            cmbQuickDate = new UltraComboEditor();
            foreach (string quickDate in new[] { "Today", "Yesterday", "This Month", "This Year", "Custom" })
            {
                cmbQuickDate.Items.Add(quickDate);
            }
            cmbQuickDate.ValueChanged += cmbQuickDate_SelectedIndexChanged;

            dtpFrom = new UltraDateTimeEditor();
            dtpTo = new UltraDateTimeEditor();
            dtpFrom.ValueChanged += DatePicker_ValueChanged;
            dtpTo.ValueChanged += DatePicker_ValueChanged;
            cmbUser = new UltraComboEditor();
            cmbActivityType = new UltraComboEditor();
            txtItemSearch = new UltraTextEditor();
            cmbAction = new UltraComboEditor();
            btnApply = new Button { Text = "Apply Filters", Height = 32, Dock = DockStyle.Top };
            btnReset = new Button { Text = "Reset", Height = 32, Dock = DockStyle.Top };

            AddDateRangeFilter(filters);
            AddFilter(filters, "User", cmbUser);
            AddFilter(filters, "Activity Type", cmbActivityType);
            AddFilter(filters, "Item", txtItemSearch);
            AddFilter(filters, "Action", cmbAction);
            filters.Controls.Add(new Panel { Height = 12, Dock = DockStyle.Top });
            filters.Controls.Add(btnApply);
            filters.Controls.Add(new Panel { Height = 8, Dock = DockStyle.Top });
            filters.Controls.Add(btnReset);
            filterPanel.Controls.Add(filters);

            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(10, 4, 0, 0), BackColor = Color.White };
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

            var titlePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            titlePanel.Controls.Add(new Label
            {
                Text = "Track item stock movements from sales, purchase, and returns.",
                Dock = DockStyle.Top,
                Height = 20,
                ForeColor = Color.FromArgb(35, 77, 145)
            });
            titlePanel.Controls.Add(new Label
            {
                Text = "Activity Log - Item Stock",
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = navy
            });

            var cards = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.White, WrapContents = false, FlowDirection = FlowDirection.LeftToRight };
            lblTotal = CreateCard(cards, "Selected Range");
            lblToday = CreateCard(cards, "Today");
            lblMonth = CreateCard(cards, "This Month");
            lblYear = CreateCard(cards, "This Year");

            var gridFrame = new RoundedPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(247, 252, 255), Padding = new Padding(2), BorderColor = Color.FromArgb(176, 224, 255), BorderRadius = 8 };
            gridActivity = new DataGridView();
            gridFrame.Controls.Add(gridActivity);

            var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            lblShowing = new Label { Text = "Showing 0 record(s)", Dock = DockStyle.Fill, ForeColor = navy, TextAlign = ContentAlignment.MiddleLeft };
            btnExport = new Button { Text = "Export", Dock = DockStyle.Fill, Height = 32 };
            footer.Controls.Add(lblShowing, 0, 0);
            footer.Controls.Add(new Label(), 1, 0);
            footer.Controls.Add(btnExport, 2, 0);

            content.Controls.Add(titlePanel, 0, 0);
            content.Controls.Add(cards, 0, 1);
            content.Controls.Add(gridFrame, 0, 2);
            content.Controls.Add(footer, 0, 3);
            root.Controls.Add(filterPanel, 0, 0);
            root.Controls.Add(content, 1, 0);
            Controls.Add(root);

            btnApply.Click += (s, e) => LoadActivityLog();
            btnReset.Click += (s, e) => ResetFilters();
            btnExport.Click += (s, e) => ExportCurrentData();
        }

        private void AddDateRangeFilter(TableLayoutPanel panel)
        {
            var row = new TableLayoutPanel { Dock = DockStyle.Top, Height = 30, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            dtpFrom.Dock = DockStyle.Fill;
            dtpFrom.Margin = new Padding(0, 0, 4, 0);
            dtpTo.Dock = DockStyle.Fill;
            dtpTo.Margin = new Padding(4, 0, 0, 0);
            row.Controls.Add(dtpFrom, 0, 0);
            row.Controls.Add(dtpTo, 1, 0);
            panel.Controls.Add(CreateFilterLabel("Date Range"));
            panel.Controls.Add(row);
            panel.Controls.Add(new Panel { Height = 8, Dock = DockStyle.Top });
        }

        private void AddFilter(TableLayoutPanel panel, string caption, Control control)
        {
            panel.Controls.Add(CreateFilterLabel(caption));
            control.Dock = DockStyle.Top;
            control.Height = 30;
            panel.Controls.Add(control);
            panel.Controls.Add(new Panel { Height = 8, Dock = DockStyle.Top });
        }

        private Label CreateFilterLabel(string caption)
        {
            return new Label { Text = caption, Dock = DockStyle.Top, Height = 22, ForeColor = navy, TextAlign = ContentAlignment.BottomLeft, BackColor = Color.FromArgb(245, 250, 255) };
        }

        private Label CreateCard(FlowLayoutPanel host, string caption)
        {
            var panel = new RoundedPanel { Size = new Size(132, 50), Margin = new Padding(0, 0, 10, 6), BackColor = Color.FromArgb(250, 253, 255), BorderColor = border, BorderRadius = 8 };
            var captionLabel = new Label { Text = caption, Dock = DockStyle.Top, Height = 21, Padding = new Padding(9, 4, 0, 0), ForeColor = Color.FromArgb(54, 78, 120) };
            var valueLabel = new Label { Text = "0", Dock = DockStyle.Fill, Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), ForeColor = navy, Padding = new Padding(9, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft };
            panel.Controls.Add(valueLabel);
            panel.Controls.Add(captionLabel);
            host.Controls.Add(panel);
            return valueLabel;
        }

        private void LoadFilterLists()
        {
            cmbUser.Items.Clear();
            cmbActivityType.Items.Clear();
            cmbAction.Items.Clear();
            cmbUser.Items.Add("All Users");
            cmbActivityType.Items.Add("All Activities");
            cmbActivityType.Items.Add("Stock Movement");
            cmbAction.Items.Add("All Actions");

            try
            {
                using (var repo = new ItemStockActivityLogRepository())
                {
                    foreach (DataRow row in repo.GetItemStockActivityUsers().Rows)
                    {
                        cmbUser.Items.Add(Convert.ToString(row["Value"]));
                    }

                    foreach (DataRow row in repo.GetItemStockActivityActions().Rows)
                    {
                        cmbAction.Items.Add(Convert.ToString(row["Value"]));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load item stock filters: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            cmbUser.SelectedIndex = 0;
            cmbActivityType.SelectedIndex = 0;
            cmbAction.SelectedIndex = 0;
        }

        private void LoadActivityLog()
        {
            LoadActivityLog(false);
        }

        private void LoadActivityLog(bool preserveGridState)
        {
            if (isLoadingActivityLog) return;

            int firstDisplayedRow = -1;
            int currentColumn = -1;
            string selectedRowKey = null;

            if (preserveGridState && gridActivity != null && gridActivity.Rows.Count > 0)
            {
                try
                {
                    firstDisplayedRow = gridActivity.FirstDisplayedScrollingRowIndex;
                }
                catch
                {
                    firstDisplayedRow = -1;
                }

                if (gridActivity.CurrentCell != null)
                {
                    currentColumn = gridActivity.CurrentCell.ColumnIndex;
                    selectedRowKey = GetActivityRowKey(gridActivity.CurrentCell.OwningRow);
                }
            }

            try
            {
                isLoadingActivityLog = true;
                string userName = cmbUser.SelectedIndex > 0 ? cmbUser.Text : string.Empty;
                string action = cmbAction.SelectedIndex > 0 ? cmbAction.Text : string.Empty;

                using (var repo = new ItemStockActivityLogRepository())
                {
                    currentData = BuildDisplayTable(repo.GetItemStockActivityLog(GetDateValue(dtpFrom), GetDateValue(dtpTo), userName, action, txtItemSearch.Text.Trim()));
                    lastKnownActivityStamp = repo.GetLatestActivityStamp();
                }

                gridActivity.DataSource = currentData;
                ConfigureGridColumns();
                ApplyActionColors(gridActivity);
                if (preserveGridState)
                {
                    RestoreGridState(selectedRowKey, currentColumn, firstDisplayedRow);
                }
                UpdateSummaryCards();
                lblShowing.Text = $"Showing {currentData.Rows.Count} record(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load item stock activity: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoadingActivityLog = false;
            }
        }

        private void ConfigureGridColumns()
        {
            if (gridActivity.Columns.Count == 0) return;

            SetColumn("DisplayLogNo", "#", 55);
            SetColumn("CreatedOn", "Date & Time", 155);
            SetColumn("UserName", "User", 115);
            SetColumn("Action", "Action", 220);
            SetColumn("ItemName", "Item Name", 240);
            SetColumn("Barcode", "Barcode", 150);
            SetColumn("UOM", "UOM", 80);
            SetColumn("Qty", "Qty", 80);
            SetColumn("UnitPrice", "Unit Price", 95);
            SetColumn("SellingPrice", "Selling Price", 105);
            SetColumn("Stock", "Stock", 85);
            SetColumn("StockIn", "Stock In", 90);
            SetColumn("StockOut", "Stock Out", 90);
            SetColumn("AdjustmentQty", "Adjustment Qty", 115);
            SetColumn("NewBalance", "New Balance", 105);
            SetColumn("QtyDifference", "Qty Difference", 115);
            SetColumn("TransactionNo", "Doc No", 95);
            SetColumn("DocNo", "Doc No", 95);
            SetColumn("InvoiceNo", "Invoice No", 120);
            SetColumn("Reason", "Reason", 160);
            SetColumn("ActivityDetails", "Details", 360);
            SetColumn("CounterName", "Counter", 130);
            SetColumn("CounterSessionId", "Session", 90);

            foreach (string name in new[] { "CompanyId", "BranchId", "FinYearId", "UserId", "CounterId", "Available", "Hold", "Cycle", "BoxQty", "ActivityLogId", "ActionSort", "SlNo", "ItemId", "UnitId", "Stock In", "Stock Out", "Adjustment Qty", "New Balance", "Qty Difference", "PhysicalStock", "Comments", "Remarks" })
            {
                HideColumn(name);
            }

            if (gridActivity.Columns.Contains("CreatedOn"))
            {
                gridActivity.Columns["CreatedOn"].DefaultCellStyle.Format = "dd MMM yyyy hh:mm tt";
            }

            foreach (string numericColumn in new[] { "Qty", "UnitPrice", "SellingPrice", "Stock", "StockIn", "StockOut", "AdjustmentQty", "NewBalance", "QtyDifference", "Available", "Hold" })
            {
                if (gridActivity.Columns.Contains(numericColumn))
                {
                    gridActivity.Columns[numericColumn].DefaultCellStyle.Format = "0.####";
                    gridActivity.Columns[numericColumn].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            foreach (string intColumn in new[] { "Cycle", "BoxQty" })
            {
                if (gridActivity.Columns.Contains(intColumn))
                {
                    gridActivity.Columns[intColumn].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        private static DataTable BuildDisplayTable(DataTable source)
        {
            if (source == null)
            {
                return new DataTable();
            }

            DataTable display = source.Copy();
            EnsureDecimalColumn(display, "StockIn");
            EnsureDecimalColumn(display, "StockOut");
            EnsureDecimalColumn(display, "AdjustmentQty");
            EnsureDecimalColumn(display, "NewBalance");
            EnsureDecimalColumn(display, "QtyDifference");
            EnsureStringColumn(display, "Reason");

            foreach (DataRow row in display.Rows)
            {
                string action = FirstText(row, "Action");
                decimal stockIn = FirstDecimal(row, "StockIn", "Stock In");
                decimal stockOut = FirstDecimal(row, "StockOut", "Stock Out");
                decimal qtyDifference = stockIn != 0m || stockOut != 0m
                    ? stockIn - stockOut
                    : FirstDecimal(row, "QtyDifference", "Qty Difference", "MovementQty");
                if (qtyDifference == 0m)
                {
                    qtyDifference = FirstDecimal(row, "AdjustmentQty", "Adjustment Qty");
                }
                if (qtyDifference == 0m && string.Equals(action, "Purchase Return", StringComparison.OrdinalIgnoreCase))
                {
                    decimal returnedQty = FirstDecimal(row, "Returned", "ReturnedQty", "ReturnQty", "Returnqty", "Returned qty");
                    if (returnedQty != 0m)
                    {
                        qtyDifference = 0m - Math.Abs(returnedQty);
                    }
                }
                if (qtyDifference == 0m)
                {
                    qtyDifference = FirstDecimal(row, "Qty");
                    if (IsStockOutAction(action))
                    {
                        qtyDifference = 0m - qtyDifference;
                    }
                }

                SetIfEmpty(row, "QtyDifference", qtyDifference);
                decimal adjustmentQty = FirstDecimal(row, "AdjustmentQty", "Adjustment Qty");
                if (adjustmentQty == 0m && IsManualStockAction(action))
                {
                    adjustmentQty = qtyDifference;
                }
                SetIfEmpty(row, "AdjustmentQty", adjustmentQty);
                SetIfEmpty(row, "NewBalance", FirstDecimal(row, "NewBalance", "New Balance", "PhysicalStock"));

                if (stockIn == 0m && IsStockInAction(action) && qtyDifference > 0m)
                {
                    stockIn = qtyDifference;
                }
                if (stockOut == 0m && IsStockOutAction(action) && qtyDifference < 0m)
                {
                    stockOut = Math.Abs(qtyDifference);
                }

                SetIfEmpty(row, "StockIn", stockIn);
                SetIfEmpty(row, "StockOut", stockOut);
                if (string.IsNullOrWhiteSpace(Convert.ToString(row["Reason"])))
                {
                    row["Reason"] = FirstText(row, "Reason", "Comments", "Remarks");
                }

                if (string.Equals(FirstText(row, "ActivityType"), "UPDATE", StringComparison.OrdinalIgnoreCase))
                {
                    row["Action"] = "Doc " + FirstText(row, "TransactionNo", "DocNo") + " updated (" + action + ")";
                }
            }

            return display;
        }

        private static void EnsureDecimalColumn(DataTable table, string columnName)
        {
            if (!table.Columns.Contains(columnName))
            {
                table.Columns.Add(columnName, typeof(decimal));
            }
        }

        private static void EnsureStringColumn(DataTable table, string columnName)
        {
            if (!table.Columns.Contains(columnName))
            {
                table.Columns.Add(columnName, typeof(string));
            }
        }

        private static decimal FirstDecimal(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    decimal value;
                    if (decimal.TryParse(Convert.ToString(row[columnName]), out value))
                    {
                        return value;
                    }
                }
            }

            return 0m;
        }

        private static string FirstText(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    string value = Convert.ToString(row[columnName]);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }

            return string.Empty;
        }

        private static void SetIfEmpty(DataRow row, string columnName, decimal value)
        {
            if (!row.Table.Columns.Contains(columnName))
            {
                return;
            }

            if (row[columnName] == DBNull.Value || FirstDecimal(row, columnName) == 0m)
            {
                row[columnName] = value;
            }
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

        private void UpdateSummaryCards()
        {
            lblTotal.Text = currentData == null ? "0" : currentData.Rows.Count.ToString();

            try
            {
                DateTime today = DateTime.Today;
                DateTime monthStart = new DateTime(today.Year, today.Month, 1);
                DateTime yearStart = new DateTime(today.Year, 1, 1);
                using (var repo = new ItemStockActivityLogRepository())
                {
                    lblToday.Text = repo.CountItemStockActivity(today, today).ToString();
                    lblMonth.Text = repo.CountItemStockActivity(monthStart, today).ToString();
                    lblYear.Text = repo.CountItemStockActivity(yearStart, today).ToString();
                }
            }
            catch
            {
                lblToday.Text = "0";
                lblMonth.Text = "0";
                lblYear.Text = "0";
            }
        }

        private void PollForNewActivityLogs(object sender, EventArgs e)
        {
            if (IsDisposed || !IsHandleCreated || isLoadingActivityLog) return;

            try
            {
                DateTime latestStamp;
                using (var repo = new ItemStockActivityLogRepository())
                {
                    latestStamp = repo.GetLatestActivityStamp();
                }

                if (latestStamp > lastKnownActivityStamp)
                {
                    LoadActivityLog(true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ItemStockActivity poll error: {ex.Message}");
            }
        }

        private static string GetActivityRowKey(DataGridViewRow row)
        {
            if (row == null) return null;

            return string.Join("|",
                GetCellText(row, "Action"),
                GetCellText(row, "TransactionNo"),
                GetCellText(row, "ItemName"),
                GetCellText(row, "Qty"),
                GetCellText(row, "CreatedOn"));
        }

        private static string GetCellText(DataGridViewRow row, string columnName)
        {
            if (row?.DataGridView == null || !row.DataGridView.Columns.Contains(columnName))
            {
                return string.Empty;
            }

            return Convert.ToString(row.Cells[columnName].Value);
        }

        private void RestoreGridState(string selectedRowKey, int currentColumn, int firstDisplayedRow)
        {
            if (gridActivity == null || gridActivity.Rows.Count == 0) return;

            int rowToSelect = -1;
            if (!string.IsNullOrEmpty(selectedRowKey))
            {
                foreach (DataGridViewRow row in gridActivity.Rows)
                {
                    if (string.Equals(GetActivityRowKey(row), selectedRowKey, StringComparison.Ordinal))
                    {
                        rowToSelect = row.Index;
                        break;
                    }
                }
            }

            if (rowToSelect >= 0)
            {
                gridActivity.ClearSelection();
                int columnToSelect = currentColumn >= 0 && currentColumn < gridActivity.Columns.Count ? currentColumn : 0;
                gridActivity.CurrentCell = gridActivity.Rows[rowToSelect].Cells[columnToSelect];
                gridActivity.Rows[rowToSelect].Selected = true;
            }

            if (firstDisplayedRow >= 0 && firstDisplayedRow < gridActivity.Rows.Count)
            {
                try
                {
                    gridActivity.FirstDisplayedScrollingRowIndex = firstDisplayedRow;
                }
                catch
                {
                    // The row can be temporarily unavailable while the grid is repainting.
                }
            }
        }

        private void ResetFilters()
        {
            cmbUser.SelectedIndex = 0;
            cmbActivityType.SelectedIndex = 0;
            cmbAction.SelectedIndex = 0;
            txtItemSearch.Text = string.Empty;
            cmbQuickDate.Text = "Today";
            ApplyQuickDate();
            LoadActivityLog();
        }

        private void StyleGrid()
        {
            StyleFilterDate(dtpFrom);
            StyleFilterDate(dtpTo);
            StyleFilterCombo(cmbQuickDate, true);
            StyleFilterCombo(cmbUser, true);
            StyleFilterCombo(cmbActivityType, true);
            StyleFilterCombo(cmbAction, true);
            StyleFilterText(txtItemSearch);
            txtItemSearch.KeyDown += txtItemSearch_KeyDown;
            StyleActionButtons();

            gridActivity.Dock = DockStyle.Fill;
            gridActivity.Margin = Padding.Empty;
            gridActivity.EnableHeadersVisualStyles = false;
            gridActivity.BorderStyle = BorderStyle.None;
            gridActivity.BackgroundColor = Color.FromArgb(247, 252, 255);
            gridActivity.GridColor = border;
            gridActivity.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            gridActivity.ScrollBars = ScrollBars.Both;
            gridActivity.AllowUserToAddRows = false;
            gridActivity.AllowUserToDeleteRows = false;
            gridActivity.ReadOnly = true;
            gridActivity.RowHeadersVisible = false;
            gridActivity.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridActivity.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 246, 255);
            gridActivity.ColumnHeadersDefaultCellStyle.ForeColor = navy;
            gridActivity.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            gridActivity.DefaultCellStyle.BackColor = Color.White;
            gridActivity.DefaultCellStyle.ForeColor = Color.FromArgb(30, 62, 120);
            gridActivity.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 238, 255);
            gridActivity.DefaultCellStyle.SelectionForeColor = navy;
            gridActivity.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);
            gridActivity.RowTemplate.Height = 30;
            gridActivity.DataBindingComplete += (s, e) => ApplyActionColors(gridActivity);
        }

        private void StyleActionButtons()
        {
            Color applyBlue = Color.FromArgb(38, 119, 237);
            Color applyHover = Color.FromArgb(54, 139, 250);
            Color applyPressed = Color.FromArgb(26, 96, 205);

            btnApply.Cursor = Cursors.Hand;
            btnApply.UseVisualStyleBackColor = false;
            btnApply.FlatStyle = FlatStyle.Flat;
            btnApply.BackColor = applyBlue;
            btnApply.ForeColor = Color.White;
            btnApply.FlatAppearance.BorderColor = applyBlue;
            btnApply.FlatAppearance.MouseOverBackColor = applyHover;
            btnApply.FlatAppearance.MouseDownBackColor = applyPressed;
            btnApply.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            btnApply.Paint += BtnApply_Paint;
            btnApply.MouseEnter += (s, e) => { applyButtonHot = true; btnApply.Invalidate(); };
            btnApply.MouseLeave += (s, e) => { applyButtonHot = false; applyButtonPressed = false; btnApply.Invalidate(); };
            btnApply.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { applyButtonPressed = true; btnApply.Invalidate(); } };
            btnApply.MouseUp += (s, e) => { applyButtonPressed = false; btnApply.Invalidate(); };

            foreach (Button button in new[] { btnReset, btnExport })
            {
                button.Cursor = Cursors.Hand;
                button.UseVisualStyleBackColor = false;
                button.FlatStyle = FlatStyle.Flat;
                button.BackColor = Color.White;
                button.ForeColor = navy;
                button.FlatAppearance.BorderColor = border;
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(242, 249, 255);
                button.FlatAppearance.MouseDownBackColor = Color.FromArgb(232, 246, 255);
            }
        }

        private void BtnApply_Paint(object sender, PaintEventArgs e)
        {
            Color fill = applyButtonPressed ? Color.FromArgb(26, 96, 205) : applyButtonHot ? Color.FromArgb(54, 139, 250) : Color.FromArgb(38, 119, 237);
            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(fill))
            {
                e.Graphics.FillRectangle(brush, btnApply.ClientRectangle);
                e.Graphics.DrawRectangle(pen, 0, 0, btnApply.Width - 1, btnApply.Height - 1);
            }

            TextRenderer.DrawText(e.Graphics, btnApply.Text, btnApply.Font, btnApply.ClientRectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void StyleFilterDate(UltraDateTimeEditor editor)
        {
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = Color.White;
            editor.Appearance.BorderColor = skyBlueOutline;
            editor.Appearance.ForeColor = navy;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            editor.DropDownButtonDisplayStyle = ButtonDisplayStyle.Always;
            editor.FormatString = "dd MMM yyyy";
            editor.MaskInput = "{date}";
        }

        private void StyleFilterCombo(UltraComboEditor combo, bool isDropDownList)
        {
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = Color.White;
            combo.Appearance.BorderColor = skyBlueOutline;
            combo.Appearance.ForeColor = navy;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = isDropDownList ? DropDownStyle.DropDownList : DropDownStyle.DropDown;
        }

        private void StyleFilterText(UltraTextEditor editor)
        {
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = Color.White;
            editor.Appearance.BorderColor = skyBlueOutline;
            editor.Appearance.ForeColor = navy;
        }

        private void txtItemSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                string searchText = txtItemSearch.Text?.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    using (var popup = new frmItemStockHistoryPopup(searchText))
                    {
                        popup.ShowDialog(this);
                    }
                }
            }
        }

        private void cmbQuickDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(cmbQuickDate.Text) && cmbQuickDate.Text != "Custom")
            {
                ApplyQuickDate();
            }
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            if (cmbQuickDate != null && cmbQuickDate.SelectedItem != null)
            {
                cmbQuickDate.Text = "Custom";
            }
        }

        private void ApplyQuickDate()
        {
            DateTime today = DateTime.Today;
            if (cmbQuickDate.Text == "Today")
            {
                SetDateRange(today, today);
            }
            else if (cmbQuickDate.Text == "Yesterday")
            {
                SetDateRange(today.AddDays(-1), today.AddDays(-1));
            }
            else if (cmbQuickDate.Text == "This Month")
            {
                SetDateRange(new DateTime(today.Year, today.Month, 1), today);
            }
            else if (cmbQuickDate.Text == "This Year")
            {
                SetDateRange(new DateTime(today.Year, 1, 1), today);
            }
        }

        private void SetDateRange(DateTime from, DateTime to)
        {
            dtpFrom.ValueChanged -= DatePicker_ValueChanged;
            dtpTo.ValueChanged -= DatePicker_ValueChanged;
            dtpFrom.Value = from;
            dtpTo.Value = to;
            dtpFrom.ValueChanged += DatePicker_ValueChanged;
            dtpTo.ValueChanged += DatePicker_ValueChanged;
        }

        private DateTime GetDateValue(UltraDateTimeEditor editor)
        {
            return editor.Value == null ? DateTime.Today : Convert.ToDateTime(editor.Value).Date;
        }

        private void HideColumn(string name)
        {
            if (gridActivity.Columns.Contains(name))
            {
                gridActivity.Columns[name].Visible = false;
            }
        }

        private void SetColumn(string name, string header, int width)
        {
            if (!gridActivity.Columns.Contains(name)) return;
            var column = gridActivity.Columns[name];
            column.HeaderText = header;
            column.Width = width;
            column.MinimumWidth = Math.Min(width, 80);
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        }

        private static void ApplyActionColors(DataGridView grid)
        {
            if (grid == null || !grid.Columns.Contains("Action")) return;

            foreach (DataGridViewRow row in grid.Rows)
            {
                string action = Convert.ToString(row.Cells["Action"].Value);
                Color color = GetActionColor(action);
                if (color == Color.Empty) continue;

                row.DefaultCellStyle.ForeColor = color;
                if (grid.Columns.Contains("Action"))
                {
                    row.Cells["Action"].Style.ForeColor = color;
                    row.Cells["Action"].Style.Font = new Font(grid.Font, FontStyle.Bold);
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

        private void ExportCurrentData()
        {
            if (currentData == null || currentData.Rows.Count == 0)
            {
                MessageBox.Show("No item stock activity data to export.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Export Item Stock Activity";
                dialog.Filter = "CSV Files (*.csv)|*.csv";
                dialog.FileName = $"ItemStockActivity_{DateTime.Now:yyyyMMdd_HHmm}.csv";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                var builder = new StringBuilder();
                for (int i = 0; i < currentData.Columns.Count; i++)
                {
                    if (i > 0) builder.Append(",");
                    builder.Append(EscapeCsv(currentData.Columns[i].ColumnName));
                }
                builder.AppendLine();
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
                foreach (DataRow row in currentData.Rows)
                {
                    for (int i = 0; i < currentData.Columns.Count; i++)
                    {
                        if (i > 0) builder.Append(",");
                        builder.Append(EscapeCsv(Convert.ToString(row[i])));
                    }
                    builder.AppendLine();
                }

                File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
                MessageBox.Show("Item stock activity exported successfully.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string EscapeCsv(string value)
        {
            value = value ?? string.Empty;
            return value.Contains(",") || value.Contains("\"") || value.Contains(Environment.NewLine)
                ? "\"" + value.Replace("\"", "\"\"") + "\""
                : value;
        }
    }
}
