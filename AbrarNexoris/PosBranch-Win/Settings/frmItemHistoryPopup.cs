using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public class frmItemHistoryPopup : Form
    {
        private readonly string searchText;
        private DataGridView gridHistory;
        private Label lblHeader;
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color border = Color.FromArgb(190, 226, 250);

        public frmItemHistoryPopup(string searchText)
        {
            this.searchText = searchText;
            InitializeComponent();
            LoadHistory();
        }

        private void InitializeComponent()
        {
            Text = "Item History Details";
            Size = new Size(1100, 580);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(247, 252, 255);
            Font = new Font("Segoe UI", 9F);

            var panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(232, 246, 255)
            };

            lblHeader = new Label
            {
                Text = $"Activity History for: '{searchText}'",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                ForeColor = navy,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };
            panelTop.Controls.Add(lblHeader);

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
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                ScrollBars = ScrollBars.Both,
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
            gridHistory.CellContentClick += GridHistory_CellContentClick;
            gridHistory.DataBindingComplete += (s, e) => ApplyActionColors();

            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(245, 250, 255)
            };

            var btnClose = new Button
            {
                Text = "Close",
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(990, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = navy,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderColor = navy;
            btnClose.Click += (s, e) => Close();
            panelBottom.Controls.Add(btnClose);

            Controls.Add(gridHistory);
            Controls.Add(panelTop);
            Controls.Add(panelBottom);
        }

        private void LoadHistory()
        {
            try
            {
                DataTable data;
                using (var repo = new ItemHistoryLogRepository())
                {
                    data = repo.GetItemDedicatedHistory(searchText);
                }
                gridHistory.DataSource = data;
                ConfigureGrid();
                ApplyActionColors();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load item history: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGrid()
        {
            if (gridHistory.Columns.Count == 0) return;
            GridPinningHelper.Attach(gridHistory);

            HideColumn("SortNo");
            HideColumn("ActivityType");

            SetColumn("CreatedOn", "Date & Time", 140);
            SetColumn("Action", "Action", 170);
            SetColumn("Source", "Source", 130);
            SetColumn("UserName", "User", 100);
            SetColumn("ItemNo", "Item Code", 85);
            SetColumn("ItemName", "Item Name", 180);
            SetColumn("Barcode", "Barcode", 110);
            SetColumn("UOM", "UOM", 70);
            SetColumn("Qty", "Qty", 70);
            SetColumn("StockIn", "Stock In", 80);
            SetColumn("StockOut", "Stock Out", 85);
            SetColumn("QtyDifference", "Qty Difference", 110);
            SetColumn("UnitCost", "Unit Cost", 90);
            SetColumn("RetailPrice", "Retail Price", 95);
            SetColumn("WalkinPrice", "Walkin Price", 95);
            SetColumn("TransactionNo", "Doc No", 95);
            SetColumn("InvoiceNo", "Invoice No", 110);
            SetColumn("PartyName", "Party", 150);
            SetColumn("ActivityDetails", "Details", 300);
            SetColumn("CounterName", "Counter", 120);
            SetColumn("CounterSessionId", "Session", 90);

            EnsureDetailsColumn();

            if (gridHistory.Columns.Contains("CreatedOn"))
            {
                gridHistory.Columns["CreatedOn"].DefaultCellStyle.Format = "dd MMM yyyy hh:mm tt";
            }

            foreach (string numericColumn in new[] { "Qty", "StockIn", "StockOut", "QtyDifference", "UnitCost", "RetailPrice", "WalkinPrice" })
            {
                if (gridHistory.Columns.Contains(numericColumn))
                {
                    gridHistory.Columns[numericColumn].DefaultCellStyle.Format = "0.####";
                    gridHistory.Columns[numericColumn].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        private void EnsureDetailsColumn()
        {
            const string colName = "ViewDetailsHistory";
            if (gridHistory.Columns.Contains(colName))
            {
                gridHistory.Columns[colName].DisplayIndex = 0;
                return;
            }

            var btn = new DataGridViewButtonColumn
            {
                Name = colName,
                HeaderText = "",
                Text = "+",
                UseColumnTextForButtonValue = true,
                Width = 38,
                FlatStyle = FlatStyle.Flat
            };
            gridHistory.Columns.Insert(0, btn);
        }

        private void HideColumn(string name)
        {
            if (gridHistory.Columns.Contains(name))
            {
                gridHistory.Columns[name].Visible = false;
            }
        }

        private void SetColumn(string name, string header, int width)
        {
            if (gridHistory.Columns.Contains(name))
            {
                var col = gridHistory.Columns[name];
                col.HeaderText = header;
                col.Width = width;
                col.MinimumWidth = Math.Min(width, 80);
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
        }

        private void GridHistory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (gridHistory.Columns[e.ColumnIndex].Name == "ViewDetailsHistory")
            {
                string details = Convert.ToString(gridHistory.Rows[e.RowIndex].Cells["ActivityDetails"].Value);
                if (string.IsNullOrWhiteSpace(details))
                {
                    details = "No additional details available.";
                }
                else
                {
                    details = FilterActivityDetails(details);
                }
                MessageBox.Show(details, "Activity Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string FilterActivityDetails(string details)
        {
            if (string.IsNullOrWhiteSpace(details)) return details;

            var lines = details.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var filtered = new System.Text.StringBuilder();
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

        private void ApplyActionColors()
        {
            ItemHistoryLog.ApplyActionColors(gridHistory);
        }
    }
}
