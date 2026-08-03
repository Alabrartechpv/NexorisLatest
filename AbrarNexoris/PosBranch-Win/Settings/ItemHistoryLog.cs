using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;
using PosBranch_Win.DialogBox;
using PosBranch_Win.Master;
using Repository.SettingsRepo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public class ItemHistoryLog : Form
    {
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color border = Color.FromArgb(190, 226, 250);
        private readonly Color skyBlueOutline = Color.FromArgb(128, 183, 220);
        private bool applyButtonHot;
        private bool applyButtonPressed;
        private DataTable currentData;
        private Timer pollTimer;
        private DateTime lastKnownActivityStamp = DateTime.MinValue;

        private UltraComboEditor cmbQuickDate;
        private UltraDateTimeEditor dtpFrom;
        private UltraDateTimeEditor dtpTo;
        private UltraComboEditor cmbUser;
        private UltraTextEditor txtItemSearch;
        private Button btnItemSearchBrowse;
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
        private Label lblDedicatedItemSummary;

        public ItemHistoryLog()
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
            frmItemMasterNew.OnItemMasterUpdated += OnItemMasterUpdated;
            StartNetworkRefresh();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            frmItemMasterNew.OnItemMasterUpdated -= OnItemMasterUpdated;
            if (pollTimer != null)
            {
                pollTimer.Stop();
                pollTimer.Dispose();
                pollTimer = null;
            }
            base.OnFormClosed(e);
        }

        private void InitializeLogUi()
        {
            Text = "Activity Log - Item History";
            Name = "ItemHistoryLog";
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
            txtItemSearch = new UltraTextEditor();

            var itemSearchContainer = new TableLayoutPanel { Dock = DockStyle.Top, Height = 30, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            itemSearchContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            itemSearchContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38F));

            txtItemSearch.Dock = DockStyle.Fill;
            txtItemSearch.Margin = new Padding(0, 0, 4, 0);
            btnItemSearchBrowse = new Button
            {
                Text = "...",
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = navy,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
            };
            btnItemSearchBrowse.FlatAppearance.BorderColor = skyBlueOutline;
            btnItemSearchBrowse.Click += btnItemSearchBrowse_Click;
            itemSearchContainer.Controls.Add(txtItemSearch, 0, 0);
            itemSearchContainer.Controls.Add(btnItemSearchBrowse, 1, 0);

            cmbAction = new UltraComboEditor();
            btnApply = new Button { Text = "Apply Filters", Height = 32, Dock = DockStyle.Top };
            btnReset = new Button { Text = "Reset", Height = 32, Dock = DockStyle.Top };

            AddDateRangeFilter(filters);
            AddFilter(filters, "User", cmbUser);
            AddFilter(filters, "Item", itemSearchContainer);
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
            lblDedicatedItemSummary = new Label
            {
                Text = string.Empty,
                Dock = DockStyle.Bottom,
                Height = 18,
                ForeColor = Color.FromArgb(55, 95, 150),
                TextAlign = ContentAlignment.MiddleLeft
            };
            titlePanel.Controls.Add(new Label
            {
                Text = "Track item creation, edits, purchase, sales, returns, and stock adjustments.",
                Dock = DockStyle.Top,
                Height = 20,
                ForeColor = Color.FromArgb(35, 77, 145)
            });
            titlePanel.Controls.Add(new Label
            {
                Text = "Activity Log - Item History",
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = navy
            });
            titlePanel.Controls.Add(lblDedicatedItemSummary);

            var cards = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.White, WrapContents = false, FlowDirection = FlowDirection.LeftToRight };
            lblTotal = CreateCard(cards, "Selected Range");
            lblToday = CreateCard(cards, "Today");
            lblMonth = CreateCard(cards, "This Month");
            lblYear = CreateCard(cards, "This Year");

            var gridFrame = new RoundedPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(247, 252, 255), Padding = new Padding(2), BorderColor = Color.FromArgb(176, 224, 255), BorderRadius = 8 };
            gridActivity = new DataGridView();
            GridPinningHelper.Attach(gridActivity);
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
            cmbAction.Items.Clear();
            cmbUser.Items.Add("All Users");
            cmbAction.Items.Add("All Actions");

            using (var repo = new ItemHistoryLogRepository())
            {
                foreach (DataRow row in repo.GetItemHistoryActions().Rows)
                {
                    cmbAction.Items.Add(Convert.ToString(row["Value"]));
                }
            }

            try
            {
                using (var repo = new ItemHistoryLogRepository())
                {
                    foreach (DataRow row in repo.GetItemHistoryUsers().Rows)
                    {
                        cmbUser.Items.Add(Convert.ToString(row["Value"]));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load item history filters: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            cmbUser.SelectedIndex = 0;
            cmbAction.SelectedIndex = 0;
        }

        private void LoadActivityLog()
        {
            try
            {
                string userName = cmbUser.SelectedIndex > 0 ? cmbUser.Text : string.Empty;
                string action = cmbAction.SelectedIndex > 0 ? cmbAction.Text : string.Empty;
                using (var repo = new ItemHistoryLogRepository())
                {
                    currentData = repo.GetItemHistoryLog(GetDateValue(dtpFrom), GetDateValue(dtpTo), userName, action, txtItemSearch.Text.Trim());
                }
                ApplyBriefActivityDetails(currentData);
                gridActivity.DataSource = currentData;
                ConfigureGridColumns();
                ApplyActionColors(gridActivity);
                UpdateDedicatedItemSummary();
                UpdateSummaryCards();
                lblShowing.Text = "Showing " + currentData.Rows.Count + " record(s)";
                UpdateLastKnownActivityStamp();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load item history activity: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static DataTable BuildItemDedicatedHistory(string searchText)
        {
            using (var repo = new ItemHistoryLogRepository())
            {
                return repo.GetItemDedicatedHistory(searchText);
            }
        }

        public static DataTable BuildCombinedHistory(DateTime fromDate, DateTime toDate, string userName, string actionFilter, string itemSearch)
        {
            using (var repo = new ItemHistoryLogRepository())
            {
                return repo.GetItemHistoryLog(fromDate, toDate, userName, actionFilter, itemSearch);
            }
        }

        private static DataTable CreateCombinedTable()
        {
            var table = new DataTable();
            table.Columns.Add("SortNo", typeof(long));
            table.Columns.Add("CreatedOn", typeof(DateTime));
            table.Columns.Add("Action", typeof(string));
            table.Columns.Add("Source", typeof(string));
            table.Columns.Add("ActivityType", typeof(string));
            table.Columns.Add("UserName", typeof(string));
            table.Columns.Add("ItemNo", typeof(string));
            table.Columns.Add("ItemName", typeof(string));
            table.Columns.Add("Barcode", typeof(string));
            table.Columns.Add("UOM", typeof(string));
            table.Columns.Add("Qty", typeof(decimal));
            table.Columns.Add("StockIn", typeof(decimal));
            table.Columns.Add("StockOut", typeof(decimal));
            table.Columns.Add("QtyDifference", typeof(decimal));
            table.Columns.Add("UnitCost", typeof(decimal));
            table.Columns.Add("RetailPrice", typeof(decimal));
            table.Columns.Add("WalkinPrice", typeof(decimal));
            table.Columns.Add("TransactionNo", typeof(string));
            table.Columns.Add("InvoiceNo", typeof(string));
            table.Columns.Add("PartyName", typeof(string));
            table.Columns.Add("ActivityDetails", typeof(string));
            table.Columns.Add("CounterName", typeof(string));
            table.Columns.Add("CounterSessionId", typeof(string));
            return table;
        }

        private static void AddItemMasterRow(DataTable target, DataRow source)
        {
            DataRow row = target.NewRow();
            string activityType = FirstText(source, "ActivityType");
            row["SortNo"] = FirstLong(source, "ItemActivityLogId");
            row["CreatedOn"] = FirstDate(source, "CreatedOn");
            row["Action"] = GetItemMasterAction(activityType);
            row["Source"] = "Item Master";
            row["ActivityType"] = activityType;
            row["UserName"] = FirstText(source, "UserName");
            row["ItemNo"] = FirstText(source, "ItemNo");
            row["ItemName"] = FirstText(source, "ItemName");
            row["Barcode"] = FirstText(source, "Barcode");
            row["Qty"] = FirstDecimal(source, "Quantity", "Available");
            row["UnitCost"] = FirstDecimal(source, "UnitCost");
            row["RetailPrice"] = FirstDecimal(source, "RetailPrice");
            row["WalkinPrice"] = FirstDecimal(source, "WalkinPrice");
            row["ActivityDetails"] = FirstText(source, "ActivityDetails");
            row["CounterName"] = FirstText(source, "CounterName");
            row["CounterSessionId"] = FirstText(source, "CounterSessionId");
            target.Rows.Add(row);
        }

        private static void AddStockRow(DataTable target, DataRow source)
        {
            DataRow row = target.NewRow();
            string sourceAction = NormalizeStockSource(FirstText(source, "Action"));
            string activityType = FirstText(source, "ActivityType");
            decimal stockIn = FirstDecimal(source, "StockIn", "Stock In");
            decimal stockOut = FirstDecimal(source, "StockOut", "Stock Out");
            decimal movement = FirstDecimal(source, "QtyDifference", "Qty Difference", "MovementQty");

            if (movement == 0m && stockIn != 0m) movement = stockIn;
            if (movement == 0m && stockOut != 0m) movement = 0m - Math.Abs(stockOut);

            row["SortNo"] = FirstLong(source, "ActivityLogId", "SlNo");
            row["CreatedOn"] = FirstDate(source, "CreatedOn");
            row["Action"] = IsUpdate(activityType) ? sourceAction + " Updated" : sourceAction;
            row["Source"] = sourceAction;
            row["ActivityType"] = activityType;
            row["UserName"] = FirstText(source, "UserName");
            row["ItemNo"] = FirstText(source, "ItemNo");
            row["ItemName"] = FirstText(source, "ItemName");
            row["Barcode"] = FirstText(source, "Barcode");
            row["UOM"] = FirstText(source, "UOM");
            row["Qty"] = FirstDecimal(source, "Qty");
            row["StockIn"] = stockIn;
            row["StockOut"] = stockOut;
            row["QtyDifference"] = movement;
            row["UnitCost"] = FirstDecimal(source, "UnitCost", "UnitPrice");
            row["RetailPrice"] = FirstDecimal(source, "RetailPrice", "SellingPrice");
            row["WalkinPrice"] = FirstDecimal(source, "WalkinPrice");
            row["TransactionNo"] = FirstText(source, "TransactionNo", "DocNo", "PurchaseNo", "SalesBillNo");
            row["InvoiceNo"] = FirstText(source, "InvoiceNo");
            row["PartyName"] = FirstText(source, "PartyName", "SupplierName", "CustomerName");
            row["ActivityDetails"] = FirstText(source, "ActivityDetails", "Reason", "Comments", "Remarks");
            row["CounterName"] = FirstText(source, "CounterName");
            row["CounterSessionId"] = FirstText(source, "CounterSessionId");
            target.Rows.Add(row);
        }

        private void ConfigureGridColumns()
        {
            if (gridActivity.Columns.Count == 0) return;
            GridPinningHelper.Attach(gridActivity);
            HideColumn("SortNo");
            HideColumn("ActivityType");

            SetColumn("Action", "Action", 170);
            SetColumn("Source", "Source", 130);
            SetColumn("CreatedOn", "Date & Time", 155);
            SetColumn("UserName", "User", 115);
            SetColumn("ItemNo", "Item Code", 95);
            SetColumn("ItemName", "Item Name", 230);
            SetColumn("Barcode", "Barcode", 145);
            SetColumn("UOM", "UOM", 75);
            SetColumn("Qty", "Qty", 80);
            SetColumn("StockIn", "Stock In", 85);
            SetColumn("StockOut", "Stock Out", 90);
            SetColumn("QtyDifference", "Qty Difference", 110);
            SetColumn("UnitCost", "Unit Cost", 90);
            SetColumn("RetailPrice", "Retail Price", 95);
            SetColumn("WalkinPrice", "Walkin Price", 100);
            SetColumn("TransactionNo", "Doc No", 100);
            SetColumn("InvoiceNo", "Invoice No", 120);
            SetColumn("PartyName", "Party", 160);
            SetColumn("ActivityDetails", "Details", 360);
            SetColumn("CounterName", "Counter", 130);
            SetColumn("CounterSessionId", "Session", 90);

            EnsureDetailsButtonColumn();

            if (gridActivity.Columns.Contains("Action"))
            {
                gridActivity.Columns["Action"].DisplayIndex = gridActivity.Columns.Contains("ViewDetails") ? 1 : 0;
                gridActivity.Columns["Action"].Frozen = true;
            }

            if (gridActivity.Columns.Contains("CreatedOn"))
            {
                gridActivity.Columns["CreatedOn"].DefaultCellStyle.Format = "dd MMM yyyy hh:mm tt";
            }

            foreach (string numericColumn in new[] { "Qty", "StockIn", "StockOut", "QtyDifference", "UnitCost", "RetailPrice", "WalkinPrice" })
            {
                if (gridActivity.Columns.Contains(numericColumn))
                {
                    gridActivity.Columns[numericColumn].DefaultCellStyle.Format = "0.####";
                    gridActivity.Columns[numericColumn].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        private void EnsureDetailsButtonColumn()
        {
            const string columnName = "ViewDetails";
            if (gridActivity.Columns.Contains(columnName))
            {
                gridActivity.Columns[columnName].DisplayIndex = 0;
                gridActivity.Columns[columnName].Frozen = true;
                return;
            }

            var buttonColumn = new DataGridViewButtonColumn
            {
                Name = columnName,
                HeaderText = "",
                Text = "+",
                UseColumnTextForButtonValue = true,
                Width = 38,
                MinimumWidth = 38,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                FlatStyle = FlatStyle.Flat,
                Frozen = true
            };
            gridActivity.Columns.Insert(0, buttonColumn);
        }

        private void UpdateSummaryCards()
        {
            lblTotal.Text = currentData == null ? "0" : currentData.Rows.Count.ToString();
            DateTime today = DateTime.Today;
            DateTime monthStart = new DateTime(today.Year, today.Month, 1);
            DateTime yearStart = new DateTime(today.Year, 1, 1);
            string itemSearch = txtItemSearch.Text.Trim();
            string userName = cmbUser.SelectedIndex > 0 ? cmbUser.Text : string.Empty;
            lblToday.Text = BuildCombinedHistory(today, today, userName, string.Empty, itemSearch).Rows.Count.ToString();
            lblMonth.Text = BuildCombinedHistory(monthStart, today, userName, string.Empty, itemSearch).Rows.Count.ToString();
            lblYear.Text = BuildCombinedHistory(yearStart, today, userName, string.Empty, itemSearch).Rows.Count.ToString();
        }

        private void ResetFilters()
        {
            cmbUser.SelectedIndex = 0;
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
            gridActivity.CellContentClick -= gridActivity_CellContentClick;
            gridActivity.CellContentClick += gridActivity_CellContentClick;
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
                string text = txtItemSearch.Text.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    using (var popup = new frmItemHistoryPopup(text))
                    {
                        popup.ShowDialog(this);
                    }
                }
            }
        }

        private void gridActivity_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || !gridActivity.Columns.Contains("ViewDetails"))
            {
                return;
            }

            if (gridActivity.Columns[e.ColumnIndex].Name != "ViewDetails")
            {
                return;
            }

            MessageBox.Show(BuildBriefActivityDetails(gridActivity.Rows[e.RowIndex]), "Activity Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnItemSearchBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dialog = new frmdialForItemMaster("ItemHistoryLog"))
                {
                    Form topLevelParent = FindForm();
                    DialogResult result = topLevelParent != null && topLevelParent != this ? dialog.ShowDialog(topLevelParent) : dialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string selected = !string.IsNullOrWhiteSpace(dialog.SelectedItemName) ? dialog.SelectedItemName :
                            (!string.IsNullOrWhiteSpace(dialog.SelectedBarcode) ? dialog.SelectedBarcode : Convert.ToString(dialog.Tag));
                        if (!string.IsNullOrWhiteSpace(selected))
                        {
                            txtItemSearch.Text = selected;
                            SetDedicatedQuickDateRange();
                            LoadActivityLog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open Item Master lookup: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (cmbQuickDate.Text == "Today") SetDateRange(today, today);
            else if (cmbQuickDate.Text == "Yesterday") SetDateRange(today.AddDays(-1), today.AddDays(-1));
            else if (cmbQuickDate.Text == "This Month") SetDateRange(new DateTime(today.Year, today.Month, 1), today);
            else if (cmbQuickDate.Text == "This Year") SetDateRange(new DateTime(today.Year, 1, 1), today);
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

        public static void ApplyActionColors(DataGridView grid)
        {
            if (grid == null || !grid.Columns.Contains("Action")) return;

            foreach (DataGridViewRow row in grid.Rows)
            {
                string action = Convert.ToString(row.Cells["Action"].Value);
                Color color = GetActionColor(action);
                if (color == Color.Empty) continue;
                row.DefaultCellStyle.ForeColor = color;
                row.Cells["Action"].Style.ForeColor = color;
                row.Cells["Action"].Style.Font = new Font(grid.Font, FontStyle.Bold);
            }
        }

        private static Color GetActionColor(string action)
        {
            if (string.IsNullOrWhiteSpace(action)) return Color.Empty;
            if (action.IndexOf("Sales", StringComparison.OrdinalIgnoreCase) >= 0 && action.IndexOf("Return", StringComparison.OrdinalIgnoreCase) < 0) return Color.FromArgb(190, 35, 35);
            if (action.IndexOf("Purchase", StringComparison.OrdinalIgnoreCase) >= 0 && action.IndexOf("Return", StringComparison.OrdinalIgnoreCase) < 0) return Color.FromArgb(24, 128, 70);
            if (action.IndexOf("Sales Return", StringComparison.OrdinalIgnoreCase) >= 0) return Color.FromArgb(204, 112, 0);
            if (action.IndexOf("Purchase Return", StringComparison.OrdinalIgnoreCase) >= 0) return Color.FromArgb(35, 95, 190);
            if (action.IndexOf("Stock Adjustment", StringComparison.OrdinalIgnoreCase) >= 0) return Color.FromArgb(112, 76, 180);
            if (action.IndexOf("Created", StringComparison.OrdinalIgnoreCase) >= 0) return Color.FromArgb(24, 128, 70);
            if (action.IndexOf("Updated", StringComparison.OrdinalIgnoreCase) >= 0) return Color.FromArgb(35, 95, 190);
            return Color.Empty;
        }

        private static IEnumerable<string> GetActionFilterValues()
        {
            return new[]
            {
                "Item Created", "Item Updated", "Purchase", "Purchase Updated", "Sales", "Sales Updated",
                "Purchase Return", "Purchase Return Updated", "Sales Return", "Sales Return Updated",
                "Stock Adjustment", "Stock Adjustment Updated"
            };
        }

        private static IEnumerable<string> GetCombinedUsers()
        {
            var users = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var repo = new ItemActivityLogRepository())
            {
                AddUserRows(users, repo.GetItemActivityUsers());
            }
            using (var repo = new ItemStockActivityLogRepository())
            {
                AddUserRows(users, repo.GetItemStockActivityUsers());
            }
            return users;
        }

        private static void AddUserRows(ISet<string> users, DataTable table)
        {
            if (table == null || !table.Columns.Contains("Value")) return;
            foreach (DataRow row in table.Rows)
            {
                string value = Convert.ToString(row["Value"]);
                if (!string.IsNullOrWhiteSpace(value)) users.Add(value);
            }
        }

        private static string GetItemMasterAction(string activityType)
        {
            if (IsCreate(activityType)) return "Item Created";
            if (IsUpdate(activityType)) return "Item Updated";
            return "Item " + (string.IsNullOrWhiteSpace(activityType) ? "Saved" : activityType);
        }

        private static string GetStockActionFilter(string actionFilter)
        {
            if (string.IsNullOrWhiteSpace(actionFilter)) return string.Empty;
            if (actionFilter.IndexOf("Purchase Return", StringComparison.OrdinalIgnoreCase) >= 0) return "Purchase Return";
            if (actionFilter.IndexOf("Sales Return", StringComparison.OrdinalIgnoreCase) >= 0) return "Sales Return";
            if (actionFilter.IndexOf("Purchase", StringComparison.OrdinalIgnoreCase) >= 0) return "Purchase";
            if (actionFilter.IndexOf("Sales", StringComparison.OrdinalIgnoreCase) >= 0) return "Sales";
            return string.Empty;
        }

        private static void ApplyActionFilter(DataTable table, string actionFilter)
        {
            if (string.IsNullOrWhiteSpace(actionFilter) || actionFilter == "All Actions") return;
            for (int i = table.Rows.Count - 1; i >= 0; i--)
            {
                if (!string.Equals(Convert.ToString(table.Rows[i]["Action"]), actionFilter, StringComparison.OrdinalIgnoreCase))
                {
                    table.Rows.RemoveAt(i);
                }
            }
        }

        private static string NormalizeStockSource(string action)
        {
            if (string.IsNullOrWhiteSpace(action)) return "Stock Adjustment";
            if (action.IndexOf("Purchase Return", StringComparison.OrdinalIgnoreCase) >= 0) return "Purchase Return";
            if (action.IndexOf("Sales Return", StringComparison.OrdinalIgnoreCase) >= 0) return "Sales Return";
            if (action.IndexOf("Purchase", StringComparison.OrdinalIgnoreCase) >= 0) return "Purchase";
            if (action.IndexOf("Sales", StringComparison.OrdinalIgnoreCase) >= 0) return "Sales";
            if (action.IndexOf("Stock", StringComparison.OrdinalIgnoreCase) >= 0) return "Stock Adjustment";
            return action;
        }

        private static bool IsCreate(string value)
        {
            return string.Equals(value, "SAVE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "ADD", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "CREATE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "CREATED", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUpdate(string value)
        {
            return string.Equals(value, "UPDATE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "EDIT", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "UPDATED", StringComparison.OrdinalIgnoreCase);
        }

        private void OnItemMasterUpdated(int itemId)
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action<int>(OnItemMasterUpdated), itemId);
                return;
            }
            LoadActivityLog();
        }

        private void StartNetworkRefresh()
        {
            pollTimer = new Timer { Interval = 10000 };
            pollTimer.Tick += PollTimer_Tick;
            pollTimer.Start();
        }

        private void PollTimer_Tick(object sender, EventArgs e)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                using (var repo = new ItemHistoryLogRepository())
                {
                    DateTime latest = repo.GetLatestActivityStamp();
                    if (latest > lastKnownActivityStamp)
                    {
                        lastKnownActivityStamp = latest;
                        LoadActivityLog();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Item history refresh failed: " + ex.Message);
            }
        }

        private void UpdateLastKnownActivityStamp()
        {
            try
            {
                using (var repo = new ItemHistoryLogRepository())
                {
                    DateTime latest = repo.GetLatestActivityStamp();
                    if (latest > lastKnownActivityStamp)
                    {
                        lastKnownActivityStamp = latest;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Item history stamp update failed: " + ex.Message);
            }
        }

        private void UpdateDedicatedItemSummary()
        {
            if (lblDedicatedItemSummary == null) return;

            string searchText = txtItemSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                lblDedicatedItemSummary.Text = string.Empty;
                return;
            }

            try
            {
                using (var repo = new ItemHistoryLogRepository())
                {
                    DataTable summary = repo.GetItemHistorySummary(searchText);
                    if (summary == null || summary.Rows.Count == 0)
                    {
                        lblDedicatedItemSummary.Text = "Loaded item: " + searchText;
                        return;
                    }

                    DataRow row = summary.Rows[0];
                    string itemName = Convert.ToString(row["ItemName"]);
                    string barcode = Convert.ToString(row["Barcode"]);
                    string stock = FormatDecimal(FirstDecimal(row, "CurrentStock"));
                    DateTime createdOn = FirstDate(row, "CreatedOn");
                    lblDedicatedItemSummary.Text =
                        "Loaded item: " + FirstNonEmpty(itemName, searchText) +
                        (!string.IsNullOrWhiteSpace(barcode) ? " | Barcode: " + barcode : string.Empty) +
                        " | Current Stock: " + (string.IsNullOrWhiteSpace(stock) ? "0" : stock) +
                        (createdOn != DateTime.MinValue ? " | Created: " + FormatDate(createdOn) : string.Empty);
                }
            }
            catch (Exception ex)
            {
                lblDedicatedItemSummary.Text = "Loaded item: " + searchText;
                System.Diagnostics.Debug.WriteLine("Item history summary display failed: " + ex.Message);
            }
        }

        private void SetDedicatedQuickDateRange()
        {
            cmbQuickDate.Text = "Custom";
            SetDateRange(new DateTime(2000, 1, 1), DateTime.Today.AddYears(5));
        }

        public static void ApplyBriefActivityDetails(DataTable table)
        {
            if (table == null || !table.Columns.Contains("ActivityDetails")) return;

            foreach (DataRow row in table.Rows)
            {
                row["ActivityDetails"] = BuildBriefActivityDetails(row);
            }
        }

        public static string BuildBriefActivityDetails(DataGridViewRow row)
        {
            if (row == null) return "No additional details available for this log entry.";

            string details = BuildBriefActivityDetails(
                CellText(row, "Action"),
                CellText(row, "ItemName"),
                CellText(row, "ItemNo"),
                CellText(row, "Barcode"),
                CellText(row, "UOM"),
                CellText(row, "TransactionNo"),
                CellText(row, "InvoiceNo"),
                CellText(row, "PartyName"),
                CellText(row, "Qty"),
                CellText(row, "StockIn"),
                CellText(row, "StockOut"),
                CellText(row, "QtyDifference"),
                CellText(row, "UnitCost"),
                CellText(row, "RetailPrice"),
                CellText(row, "WalkinPrice"),
                CellText(row, "UserName"),
                CellText(row, "CounterName"),
                CellText(row, "CounterSessionId"),
                CellText(row, "CreatedOn"),
                CellText(row, "ActivityDetails"));

            return string.IsNullOrWhiteSpace(details) ? "No additional details available for this log entry." : details;
        }

        private static string BuildBriefActivityDetails(DataRow row)
        {
            if (row == null) return string.Empty;

            return BuildBriefActivityDetails(
                FirstText(row, "Action"),
                FirstText(row, "ItemName"),
                FirstText(row, "ItemNo"),
                FirstText(row, "Barcode"),
                FirstText(row, "UOM"),
                FirstText(row, "TransactionNo"),
                FirstText(row, "InvoiceNo"),
                FirstText(row, "PartyName"),
                FormatDecimal(FirstDecimal(row, "Qty")),
                FormatDecimal(FirstDecimal(row, "StockIn")),
                FormatDecimal(FirstDecimal(row, "StockOut")),
                FormatDecimal(FirstDecimal(row, "QtyDifference")),
                FormatDecimal(FirstDecimal(row, "UnitCost")),
                FormatDecimal(FirstDecimal(row, "RetailPrice")),
                FormatDecimal(FirstDecimal(row, "WalkinPrice")),
                FirstText(row, "UserName"),
                FirstText(row, "CounterName"),
                FirstText(row, "CounterSessionId"),
                FormatDate(FirstDate(row, "CreatedOn")),
                FirstText(row, "ActivityDetails"));
        }

        private static string BuildBriefActivityDetails(
            string action,
            string itemName,
            string itemNo,
            string barcode,
            string uom,
            string transactionNo,
            string invoiceNo,
            string partyName,
            string qty,
            string stockIn,
            string stockOut,
            string qtyDifference,
            string unitCost,
            string retailPrice,
            string walkinPrice,
            string userName,
            string counterName,
            string counterSessionId,
            string createdOn,
            string rawDetails)
        {
            var builder = new StringBuilder();
            string displayAction = GetBriefAction(action, stockIn, stockOut, qtyDifference);
            string itemCaption = FirstNonEmpty(itemName, itemNo, barcode);

            AppendLine(builder, "Action", displayAction);
            AppendLine(builder, "Item", BuildItemCaption(itemCaption, itemNo, barcode));
            AppendLine(builder, GetDocumentLabel(displayAction), transactionNo);
            AppendLine(builder, "Invoice", invoiceNo);
            AppendLine(builder, GetPartyLabel(displayAction), partyName);
            AppendLine(builder, "Qty", BuildQtyCaption(qty, uom));
            AppendLine(builder, "Stock In", stockIn);
            AppendLine(builder, "Stock Out", stockOut);
            AppendLine(builder, "Stock Change", qtyDifference);
            AppendLine(builder, "Unit Cost", unitCost);
            AppendLine(builder, "Retail Price", retailPrice);
            AppendLine(builder, "Walkin Price", walkinPrice);
            AppendLine(builder, "User", userName);
            AppendLine(builder, "Counter", BuildCounterCaption(counterName, counterSessionId));
            AppendLine(builder, "Date", createdOn);

            string filteredDetails = FilterActivityDetails(rawDetails);
            if (!string.IsNullOrWhiteSpace(filteredDetails) && !LooksLikeBriefDetails(filteredDetails))
            {
                builder.AppendLine();
                builder.AppendLine(IsUpdate(action) ? "Updated:" : "Notes:");
                builder.Append(NormalizeDetailSection(filteredDetails, IsUpdate(action)));
            }

            return builder.ToString().Trim();
        }

        private static string GetBriefAction(string action, string stockIn, string stockOut, string qtyDifference)
        {
            if (string.IsNullOrWhiteSpace(action)) return "Activity";
            if (action.IndexOf("Stock", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                decimal stockInQty = ParseDecimal(stockIn);
                decimal stockOutQty = ParseDecimal(stockOut);
                decimal movementQty = ParseDecimal(qtyDifference);
                bool isUpdateAction = action.IndexOf("Updated", StringComparison.OrdinalIgnoreCase) >= 0;
                if (isUpdateAction) return "Stock Update";
                if (stockInQty > 0m || movementQty > 0m) return "Stock In";
                if (stockOutQty > 0m || movementQty < 0m) return "Stock Out";
            }
            return action;
        }

        private static string GetDocumentLabel(string action)
        {
            if (action.IndexOf("Purchase", StringComparison.OrdinalIgnoreCase) >= 0) return "Purchase No";
            if (action.IndexOf("Sales", StringComparison.OrdinalIgnoreCase) >= 0) return "Bill No";
            if (action.IndexOf("Stock", StringComparison.OrdinalIgnoreCase) >= 0) return "Stock Doc No";
            return "Doc No";
        }

        private static string GetPartyLabel(string action)
        {
            if (action.IndexOf("Purchase", StringComparison.OrdinalIgnoreCase) >= 0) return "Vendor";
            if (action.IndexOf("Sales", StringComparison.OrdinalIgnoreCase) >= 0) return "Customer";
            return "Party";
        }

        private static string BuildItemCaption(string itemName, string itemNo, string barcode)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(itemName)) parts.Add(itemName);
            if (!string.IsNullOrWhiteSpace(itemNo) && !string.Equals(itemNo, itemName, StringComparison.OrdinalIgnoreCase)) parts.Add("Code: " + itemNo);
            if (!string.IsNullOrWhiteSpace(barcode) && !string.Equals(barcode, itemName, StringComparison.OrdinalIgnoreCase)) parts.Add("Barcode: " + barcode);
            return string.Join(" | ", parts.ToArray());
        }

        private static string BuildQtyCaption(string qty, string uom)
        {
            if (string.IsNullOrWhiteSpace(qty) || IsZero(qty)) return string.Empty;
            return string.IsNullOrWhiteSpace(uom) ? qty : qty + " " + uom;
        }

        private static string BuildCounterCaption(string counterName, string counterSessionId)
        {
            if (string.IsNullOrWhiteSpace(counterSessionId) || IsZero(counterSessionId)) return counterName;
            return string.IsNullOrWhiteSpace(counterName) ? "Session " + counterSessionId : counterName + " | Session " + counterSessionId;
        }

        private static void AppendLine(StringBuilder builder, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || IsZero(value)) return;
            builder.Append(label);
            builder.Append(": ");
            builder.AppendLine(value);
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }
            return string.Empty;
        }

        private static bool LooksLikeBriefDetails(string details)
        {
            return details.StartsWith("Action:", StringComparison.OrdinalIgnoreCase) &&
                   details.IndexOf(Environment.NewLine + "Item:", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string FilterActivityDetails(string details)
        {
            if (string.IsNullOrWhiteSpace(details)) return details;

            var lines = details.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var filtered = new StringBuilder();
            foreach (var line in lines)
            {
                string trimmed = line.TrimStart('-', ' ');
                if (trimmed.StartsWith("Unit '", StringComparison.OrdinalIgnoreCase) &&
                    (trimmed.Contains("Retail Price changed") || trimmed.Contains("Walkin Price changed")))
                {
                    continue;
                }
                filtered.AppendLine(line);
            }
            return filtered.ToString().TrimEnd();
        }

        private static string NormalizeDetailSection(string details, bool isUpdate)
        {
            if (string.IsNullOrWhiteSpace(details)) return string.Empty;

            var lines = details.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var normalized = new StringBuilder();
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0) continue;
                if (isUpdate && (trimmed.Equals("Changes:", StringComparison.OrdinalIgnoreCase) ||
                                 trimmed.Equals("Updated:", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                normalized.AppendLine(trimmed);
            }
            return normalized.ToString().TrimEnd();
        }

        private static string CellText(DataGridViewRow row, string columnName)
        {
            if (row.DataGridView == null || !row.DataGridView.Columns.Contains(columnName)) return string.Empty;
            object value = row.Cells[columnName].Value;
            if (value == null || value == DBNull.Value) return string.Empty;
            if (value is DateTime) return FormatDate((DateTime)value);
            if (value is decimal) return FormatDecimal((decimal)value);
            return Convert.ToString(value);
        }

        private static string FormatDate(DateTime value)
        {
            return value == DateTime.MinValue ? string.Empty : value.ToString("dd MMM yyyy hh:mm tt");
        }

        private static string FormatDecimal(decimal value)
        {
            return value == 0m ? string.Empty : value.ToString("0.####");
        }

        private static decimal ParseDecimal(string value)
        {
            decimal parsed;
            return decimal.TryParse(value, out parsed) ? parsed : 0m;
        }

        private static bool IsZero(string value)
        {
            decimal parsed;
            return decimal.TryParse(value, out parsed) && parsed == 0m;
        }

        private static string FirstText(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    string value = Convert.ToString(row[columnName]);
                    if (!string.IsNullOrWhiteSpace(value)) return value;
                }
            }
            return string.Empty;
        }

        private static decimal FirstDecimal(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    decimal value;
                    if (decimal.TryParse(Convert.ToString(row[columnName]), out value)) return value;
                }
            }
            return 0m;
        }

        private static long FirstLong(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    long value;
                    if (long.TryParse(Convert.ToString(row[columnName]), out value)) return value;
                }
            }
            return 0L;
        }

        private static DateTime FirstDate(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    DateTime value;
                    if (DateTime.TryParse(Convert.ToString(row[columnName]), out value)) return value;
                }
            }
            return DateTime.MinValue;
        }

        private void ExportCurrentData()
        {
            if (currentData == null || currentData.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Activity Log", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv", FileName = "ItemHistoryLog.csv" })
            {
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
                MessageBox.Show("Activity log exported successfully.", "Activity Log", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string EscapeCsv(string value)
        {
            value = value ?? string.Empty;
            if (value.Contains(",") || value.Contains("\"") || value.Contains(Environment.NewLine))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}
