using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Repository.SettingsRepo;

namespace PosBranch_Win.Settings
{
    public class frmItemHistoryPopup : Form
    {
        private string searchText;
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
            this.Text = "Item History Details";
            this.Size = new Size(950, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(247, 252, 255);
            this.Font = new Font("Segoe UI", 9F);

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
                RowTemplate = { Height = 30 }
            };

            gridHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 55, 120);
            gridHistory.GridColor = border;
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
                Location = new Point(830, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = navy,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderColor = navy;
            btnClose.Click += (s, e) => this.Close();
            panelBottom.Controls.Add(btnClose);

            this.Controls.Add(gridHistory);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelBottom);
        }

        private void LoadHistory()
        {
            try
            {
                using (var repo = new ItemActivityLogRepository())
                {
                    DataTable data = repo.GetItemHistoryLog(searchText);

                    gridHistory.DataSource = data;
                    ConfigureGrid();
                    ApplyActionColors();
                }
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

            // Hide unneeded database IDs
            HideColumn("ItemActivityLogId");
            HideColumn("CompanyId");
            HideColumn("BranchId");
            HideColumn("FinYearId");
            HideColumn("UserId");
            HideColumn("CounterId");
            HideColumn("CounterSessionId");

            // Format columns
            SetColumn("CreatedOn", "Date & Time", 140);
            SetColumn("UserName", "User", 100);
            SetColumn("ActivityType", "Action", 80);
            SetColumn("ItemNo", "Item Code", 85);
            SetColumn("ItemName", "Item Name", 180);
            SetColumn("Barcode", "Barcode", 110);
            SetColumn("ActivityDetails", "Details", 300);

            // Add details button
            EnsureDetailsColumn();

            if (gridHistory.Columns.Contains("CreatedOn"))
            {
                gridHistory.Columns["CreatedOn"].DefaultCellStyle.Format = "dd MMM yyyy hh:mm tt";
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
                gridHistory.Columns[name].Visible = false;
        }

        private void SetColumn(string name, string header, int width)
        {
            if (gridHistory.Columns.Contains(name))
            {
                var col = gridHistory.Columns[name];
                col.HeaderText = header;
                col.Width = width;
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
            if (gridHistory == null || !gridHistory.Columns.Contains("ActivityType")) return;

            foreach (DataGridViewRow row in gridHistory.Rows)
            {
                string action = Convert.ToString(row.Cells["ActivityType"].Value);
                Color color = GetActionColor(action);
                if (color == Color.Empty) continue;

                row.DefaultCellStyle.ForeColor = color;
                row.Cells["ActivityType"].Style.ForeColor = color;
                row.Cells["ActivityType"].Style.Font = new Font(gridHistory.Font, FontStyle.Bold);
            }
        }

        private static Color GetActionColor(string action)
        {
            if (action == null) return Color.Empty;

            if (action.IndexOf("(Purchase)", StringComparison.OrdinalIgnoreCase) >= 0 ||
                action.IndexOf("(Stock IN)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Color.FromArgb(24, 128, 70);
            }

            if (action.IndexOf("(Sales)", StringComparison.OrdinalIgnoreCase) >= 0 ||
                action.IndexOf("(Stock OUT)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Color.FromArgb(190, 35, 35);
            }

            if (string.Equals(action, "Sales", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Stock OUT", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Stock Out", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "DELETE", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "REMOVE", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(190, 35, 35);
            }

            if (string.Equals(action, "Purchase", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Stock IN", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Stock In", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "SAVE", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "ADD", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "CREATE", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(24, 128, 70);
            }

            if (string.Equals(action, "Sales Return", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(204, 112, 0);
            }

            if (string.Equals(action, "Purchase Return", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "UPDATE", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "EDIT", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(35, 95, 190);
            }

            return Color.Empty;
        }
    }
}
