using Repository.SettingsRepo;
using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;
using PosBranch_Win.Master;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public partial class ActivityLog : Form
    {
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color border = Color.FromArgb(190, 226, 250);
        private readonly Color skyBlueOutline = Color.FromArgb(128, 183, 220);
        private bool applyButtonHot;
        private bool applyButtonPressed;
        private DataTable currentData;
        private System.Windows.Forms.Timer _pollTimer;
        private int _lastKnownMaxLogId = 0;

        public ActivityLog()
        {
            InitializeComponent();
            StyleGrid();
        }

        private void ActivityLog_Load(object sender, EventArgs e)
        {
            LoadFilterLists();
            cmbQuickDate.Text = "Today";
            ApplyQuickDate();
            LoadActivityLog();

            // Subscribe to real-time item save/update notifications
            frmItemMasterNew.OnItemMasterUpdated += OnItemSavedOrUpdated;

            // Poll DB every 10 seconds for cross-counter/cross-session real-time refresh
            _pollTimer = new System.Windows.Forms.Timer { Interval = 10000 };
            _pollTimer.Tick += PollForNewActivityLogs;
            _pollTimer.Start();

            this.FormClosed += (s, args) =>
            {
                frmItemMasterNew.OnItemMasterUpdated -= OnItemSavedOrUpdated;
                _pollTimer?.Stop();
                _pollTimer?.Dispose();
            };
        }

        private void OnItemSavedOrUpdated(int itemId)
        {
            // Marshal back to UI thread safely
            if (this.IsDisposed || !this.IsHandleCreated) return;
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => OnItemSavedOrUpdated(itemId)));
                return;
            }
            LoadActivityLog();
        }

        private int GetCurrentMaxLogId()
        {
            try
            {
                using (var repo = new ItemActivityLogRepository())
                {
                    return repo.GetLatestActivityLogId();
                }
            }
            catch
            {
                return 0;
            }
        }

        private void PollForNewActivityLogs(object sender, EventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            try
            {
                int latestId = GetCurrentMaxLogId();
                if (latestId > 0 && latestId > _lastKnownMaxLogId)
                {
                    _lastKnownMaxLogId = latestId;
                    LoadActivityLog();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ActivityLog poll error: {ex.Message}");
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
            SetCustomQuickDate();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            LoadActivityLog();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            cmbUser.SelectedIndex = 0;
            cmbActivityType.SelectedIndex = 0;
            cmbAction.SelectedIndex = 0;
            txtItemSearch.Text = string.Empty;
            cmbQuickDate.Text = "Today";
            ApplyQuickDate();
            LoadActivityLog();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportCurrentData();
        }

        private void LoadFilterLists()
        {
            cmbUser.Items.Clear();
            cmbActivityType.Items.Clear();
            cmbAction.Items.Clear();
            cmbUser.Items.Add("All Users");
            cmbActivityType.Items.Add("All Activities");
            cmbAction.Items.Add("All Actions");
            cmbAction.Items.Add("SAVE");
            cmbAction.Items.Add("UPDATE");
            cmbAction.Items.Add("DELETE");

            try
            {
                using (var repo = new ItemActivityLogRepository())
                {
                    foreach (DataRow row in repo.GetItemActivityUsers().Rows)
                    {
                        cmbUser.Items.Add(Convert.ToString(row["Value"]));
                    }

                    foreach (DataRow row in repo.GetItemActivityTypes().Rows)
                    {
                        cmbActivityType.Items.Add(Convert.ToString(row["Value"]));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load activity filters: " + ex.Message, "Activity Log", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            cmbUser.SelectedIndex = 0;
            cmbActivityType.SelectedIndex = 0;
            cmbAction.SelectedIndex = 0;
        }

        private void LoadActivityLog()
        {
            try
            {
                string userName = cmbUser.SelectedIndex > 0 ? cmbUser.Text : string.Empty;
                string activityType = cmbActivityType.SelectedIndex > 0 ? cmbActivityType.Text : string.Empty;
                if (cmbAction.SelectedIndex > 0)
                {
                    activityType = cmbAction.Text;
                }

                using (var repo = new ItemActivityLogRepository())
                {
                    currentData = repo.GetItemActivityLog(
                        GetDateValue(dtpFrom),
                        GetDateValue(dtpTo),
                        userName,
                        activityType,
                        txtItemSearch.Text.Trim());
                }

                gridActivity.DataSource = currentData;
                ConfigureGridColumns();
                UpdateSummaryCards();
                lblShowing.Text = $"Showing {currentData.Rows.Count} record(s)";

                int maxId = GetCurrentMaxLogId();
                if (maxId > 0)
                {
                    _lastKnownMaxLogId = maxId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load activity log: " + ex.Message, "Activity Log", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSummaryCards()
        {
            lblTotal.Text = currentData == null ? "0" : currentData.Rows.Count.ToString();

            try
            {
                DateTime today = DateTime.Today;
                DateTime weekStart = today.AddDays(-(int)today.DayOfWeek);
                DateTime monthStart = new DateTime(today.Year, today.Month, 1);

                using (var repo = new ItemActivityLogRepository())
                {
                    lblToday.Text = repo.CountItemActivity(today, today).ToString();
                    lblWeek.Text = repo.CountItemActivity(weekStart, today).ToString();
                    lblMonth.Text = repo.CountItemActivity(monthStart, today).ToString();
                }
            }
            catch
            {
                lblToday.Text = "0";
                lblWeek.Text = "0";
                lblMonth.Text = "0";
            }
        }

        private void ConfigureGridColumns()
        {
            if (gridActivity.Columns.Count == 0)
            {
                return;
            }

            SetColumn("ItemActivityLogId", "#", 55);
            SetColumn("CreatedOn", "Date & Time", 155);
            SetColumn("UserName", "User", 115);
            SetColumn("UserId", "User ID", 75);
            SetColumn("ActivityType", "Action", 110);
            SetColumn("ItemNo", "Item Code", 95);
            SetColumn("ItemName", "Item Name", 240);
            SetColumn("Barcode", "Barcode", 160);
            SetColumn("ActivityDetails", "Details", 420);
            SetColumn("UnitCost", "Unit Cost", 95);
            SetColumn("RetailPrice", "Retail", 90);
            SetColumn("WalkinPrice", "Walkin", 90);
            SetColumn("CompanyId", "Company", 80);
            SetColumn("BranchId", "Branch", 75);
            SetColumn("FinYearId", "Fin Year", 80);
            SetColumn("CounterName", "Counter", 130);
            SetColumn("CounterId", "Counter ID", 85);
            SetColumn("CounterSessionId", "Session", 90);
            SetColumn("Quantity", "Quantity", 90);
            SetColumn("Available", "Available", 90);
            SetColumn("OnHold", "On Hold", 80);
            SetColumn("Reorder", "Reorder", 80);
            SetColumn("OrderCycleDays", "Order Cycle Days", 120);
            SetColumn("BoxQty", "Box Qty", 80);
            SetColumn("ItemType", "Item Type", 100);
            SetColumn("Category", "Category", 100);
            SetColumn("ItemGroup", "Group", 100);
            SetColumn("HSN", "HSN", 100);
            SetColumn("ItemStatus", "Status", 90);

            if (gridActivity.Columns.Contains("CreatedOn"))
            {
                gridActivity.Columns["CreatedOn"].DefaultCellStyle.Format = "dd MMM yyyy hh:mm tt";
            }

            foreach (string moneyColumn in new[] { "UnitCost", "RetailPrice", "WalkinPrice" })
            {
                if (gridActivity.Columns.Contains(moneyColumn))
                {
                    gridActivity.Columns[moneyColumn].DefaultCellStyle.Format = "0.####";
                    gridActivity.Columns[moneyColumn].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            foreach (string numColumn in new[] { "Quantity", "Available", "OnHold", "Reorder" })
            {
                if (gridActivity.Columns.Contains(numColumn))
                {
                    gridActivity.Columns[numColumn].DefaultCellStyle.Format = "0.####";
                    gridActivity.Columns[numColumn].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            foreach (string intColumn in new[] { "OrderCycleDays", "BoxQty" })
            {
                if (gridActivity.Columns.Contains(intColumn))
                {
                    gridActivity.Columns[intColumn].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            EnsureDetailsButtonColumn();
        }

        private void EnsureDetailsButtonColumn()
        {
            const string columnName = "ViewDetails";
            if (gridActivity.Columns.Contains(columnName))
            {
                gridActivity.Columns[columnName].DisplayIndex = 0;
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
                FlatStyle = FlatStyle.Flat
            };
            gridActivity.Columns.Insert(0, buttonColumn);
        }

        private void SetColumn(string name, string header, int width)
        {
            if (!gridActivity.Columns.Contains(name))
            {
                return;
            }

            var column = gridActivity.Columns[name];
            column.HeaderText = header;
            column.Width = width;
            column.MinimumWidth = Math.Min(width, 80);
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
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
            txtItemSearch.KeyDown -= txtItemSearch_KeyDown;
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
            gridActivity.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 246, 255);
            gridActivity.ColumnHeadersDefaultCellStyle.ForeColor = navy;
            gridActivity.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            gridActivity.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 246, 255);
            gridActivity.DefaultCellStyle.BackColor = Color.White;
            gridActivity.DefaultCellStyle.ForeColor = Color.FromArgb(30, 62, 120);
            gridActivity.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 238, 255);
            gridActivity.DefaultCellStyle.SelectionForeColor = navy;
            gridActivity.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            gridActivity.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);
            gridActivity.RowTemplate.Height = 30;
            gridActivity.CellContentClick -= gridActivity_CellContentClick;
            gridActivity.CellContentClick += gridActivity_CellContentClick;

            gridFrame.BorderColor = Color.FromArgb(176, 224, 255);
            gridFrame.BorderRadius = 8;
            gridFrame.Padding = new Padding(2);
        }

        private void txtItemSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                string searchText = txtItemSearch.Text?.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    var popup = new frmItemHistoryPopup(searchText);
                    popup.ShowDialog(this);
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

            string details = Convert.ToString(gridActivity.Rows[e.RowIndex].Cells["ActivityDetails"].Value);
            if (string.IsNullOrWhiteSpace(details))
            {
                details = "No additional details available for this log entry.";
            }
            else
            {
                details = FilterActivityDetails(details);
            }

            MessageBox.Show(details, "Activity Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Strips display lines that should not be shown to the user,
        /// such as per-unit Retail Price and Walkin Price change lines
        /// that were logged in older records before the fix.
        /// </summary>
        private static string FilterActivityDetails(string details)
        {
            if (string.IsNullOrWhiteSpace(details)) return details;

            var lines = details.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var filtered = new StringBuilder();
            foreach (var line in lines)
            {
                // Skip per-unit Retail Price and Walkin Price change lines
                // e.g. "- Unit 'UNIT' Retail Price changed from X to Y"
                //      "- Unit 'UNIT' Walkin Price changed from X to Y"
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

        private void StyleActionButtons()
        {
            Color applyBlue = Color.FromArgb(38, 119, 237);
            Color applyHover = Color.FromArgb(54, 139, 250);
            Color applyPressed = Color.FromArgb(26, 96, 205);

            btnApply.UseVisualStyleBackColor = false;
            btnApply.FlatStyle = FlatStyle.Flat;
            btnApply.BackColor = applyBlue;
            btnApply.ForeColor = Color.White;
            btnApply.FlatAppearance.BorderColor = applyBlue;
            btnApply.FlatAppearance.MouseOverBackColor = applyHover;
            btnApply.FlatAppearance.MouseDownBackColor = applyPressed;
            btnApply.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            btnApply.Text = "Apply Filters";
            btnApply.Paint -= BtnApply_Paint;
            btnApply.Paint += BtnApply_Paint;
            btnApply.MouseEnter -= BtnApply_MouseEnter;
            btnApply.MouseEnter += BtnApply_MouseEnter;
            btnApply.MouseLeave -= BtnApply_MouseLeave;
            btnApply.MouseLeave += BtnApply_MouseLeave;
            btnApply.MouseDown -= BtnApply_MouseDown;
            btnApply.MouseDown += BtnApply_MouseDown;
            btnApply.MouseUp -= BtnApply_MouseUp;
            btnApply.MouseUp += BtnApply_MouseUp;

            btnReset.UseVisualStyleBackColor = false;
            btnReset.FlatStyle = FlatStyle.Flat;
            btnReset.BackColor = Color.White;
            btnReset.ForeColor = navy;
            btnReset.FlatAppearance.BorderColor = border;
            btnReset.FlatAppearance.MouseOverBackColor = Color.FromArgb(242, 249, 255);
            btnReset.FlatAppearance.MouseDownBackColor = Color.FromArgb(232, 246, 255);
        }

        private void BtnApply_Paint(object sender, PaintEventArgs e)
        {
            Color fill = applyButtonPressed
                ? Color.FromArgb(26, 96, 205)
                : applyButtonHot ? Color.FromArgb(54, 139, 250) : Color.FromArgb(38, 119, 237);

            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(fill))
            {
                e.Graphics.FillRectangle(brush, btnApply.ClientRectangle);
                e.Graphics.DrawRectangle(pen, 0, 0, btnApply.Width - 1, btnApply.Height - 1);
            }

            TextRenderer.DrawText(
                e.Graphics,
                btnApply.Text,
                btnApply.Font,
                btnApply.ClientRectangle,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void BtnApply_MouseEnter(object sender, EventArgs e)
        {
            applyButtonHot = true;
            btnApply.Invalidate();
        }

        private void BtnApply_MouseLeave(object sender, EventArgs e)
        {
            applyButtonHot = false;
            applyButtonPressed = false;
            btnApply.Invalidate();
        }

        private void BtnApply_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                applyButtonPressed = true;
                btnApply.Invalidate();
            }
        }

        private void BtnApply_MouseUp(object sender, MouseEventArgs e)
        {
            applyButtonPressed = false;
            btnApply.Invalidate();
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
            editor.Appearance.FontData.Name = "Segoe UI";
            editor.Appearance.FontData.SizeInPoints = 8.5F;
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
            combo.Appearance.FontData.Name = "Segoe UI";
            combo.Appearance.FontData.SizeInPoints = 8.75F;
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
            editor.Appearance.FontData.Name = "Segoe UI";
            editor.Appearance.FontData.SizeInPoints = 8.75F;
        }

        private void ApplyQuickDate()
        {
            DateTime today = DateTime.Today;
            string selected = cmbQuickDate.Text;

            if (selected == "Today")
            {
                SetDateRange(today, today);
            }
            else if (selected == "Yesterday")
            {
                SetDateRange(today.AddDays(-1), today.AddDays(-1));
            }
            else if (selected == "This Week")
            {
                SetDateRange(today.AddDays(-(int)today.DayOfWeek), today);
            }
            else if (selected == "This Month")
            {
                SetDateRange(new DateTime(today.Year, today.Month, 1), today);
            }
            else if (selected == "Previous Month")
            {
                DateTime firstThisMonth = new DateTime(today.Year, today.Month, 1);
                DateTime firstPreviousMonth = firstThisMonth.AddMonths(-1);
                SetDateRange(firstPreviousMonth, firstThisMonth.AddDays(-1));
            }
            else if (selected == "This Year")
            {
                SetDateRange(new DateTime(today.Year, 1, 1), today);
            }
            else if (selected == "Previous Year")
            {
                SetDateRange(new DateTime(today.Year - 1, 1, 1), new DateTime(today.Year - 1, 12, 31));
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

        private void SetCustomQuickDate()
        {
            if (cmbQuickDate != null && cmbQuickDate.SelectedItem != null)
            {
                cmbQuickDate.Text = "Custom";
            }
        }

        private void ExportCurrentData()
        {
            if (currentData == null || currentData.Rows.Count == 0)
            {
                MessageBox.Show("No activity log data to export.", "Activity Log", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Export Activity Log";
                dialog.Filter = "CSV Files (*.csv)|*.csv";
                dialog.FileName = $"ItemActivityLog_{DateTime.Now:yyyyMMdd_HHmm}.csv";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

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

    public class RoundedPanel : Panel
    {
        public Color BorderColor { get; set; } = Color.FromArgb(176, 224, 255);
        public int BorderRadius { get; set; } = 8;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = CreateRoundRectangle(ClientRectangle, BorderRadius))
            using (var pen = new Pen(BorderColor))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Invalidate();
        }

        private static GraphicsPath CreateRoundRectangle(Rectangle bounds, int radius)
        {
            int diameter = Math.Max(1, radius * 2);
            var path = new GraphicsPath();
            bounds.Width -= 1;
            bounds.Height -= 1;
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
