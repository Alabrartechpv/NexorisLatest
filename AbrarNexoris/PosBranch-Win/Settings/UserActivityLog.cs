using Repository.SettingsRepo;
using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public partial class UserActivityLog : Form
    {
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color border = Color.FromArgb(190, 226, 250);
        private readonly Color skyBlueOutline = Color.FromArgb(128, 183, 220);
        private bool applyButtonHot;
        private bool applyButtonPressed;
        private DataTable currentData;
        private System.Windows.Forms.Timer _pollTimer;
        private int _lastKnownMaxLogId = 0;

        public UserActivityLog()
        {
            InitializeComponent();
            StyleGrid();
        }

        private void UserActivityLog_Load(object sender, EventArgs e)
        {
            LoadFilterLists();
            cmbQuickDate.Text = "Today";
            ApplyQuickDate();
            LoadActivityLog();

            // Poll DB every 10 seconds for cross-counter/cross-session real-time refresh
            _pollTimer = new System.Windows.Forms.Timer { Interval = 10000 };
            _pollTimer.Tick += PollForNewActivityLogs;
            _pollTimer.Start();

            this.FormClosed += (s, args) =>
            {
                _pollTimer?.Stop();
                _pollTimer?.Dispose();
            };
        }

        private int GetCurrentMaxLogId()
        {
            try
            {
                using (var repo = new UserActivityLogRepository())
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
                System.Diagnostics.Debug.WriteLine($"UserActivityLog poll error: {ex.Message}");
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
            txtSearch.Text = string.Empty;
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
            cmbUser.Items.Add("All Users");
            cmbActivityType.Items.Add("All Actions");
            cmbActivityType.Items.Add("Login");
            cmbActivityType.Items.Add("Logout");
            cmbActivityType.Items.Add("FormEntry");

            try
            {
                using (var repo = new UserActivityLogRepository())
                {
                    foreach (DataRow row in repo.GetUserActivityUsers().Rows)
                    {
                        cmbUser.Items.Add(Convert.ToString(row["Value"]));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load activity filters: " + ex.Message, "User Activity Log", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            cmbUser.SelectedIndex = 0;
            cmbActivityType.SelectedIndex = 0;
        }

        private void LoadActivityLog()
        {
            try
            {
                string userName = cmbUser.SelectedIndex > 0 ? cmbUser.Text : string.Empty;
                string activityType = cmbActivityType.SelectedIndex > 0 ? cmbActivityType.Text : string.Empty;

                using (var repo = new UserActivityLogRepository())
                {
                    currentData = repo.GetUserActivityLog(
                        GetDateValue(dtpFrom),
                        GetDateValue(dtpTo),
                        userName,
                        activityType,
                        txtSearch.Text.Trim());
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
                MessageBox.Show("Unable to load user activity log: " + ex.Message, "User Activity Log", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSummaryCards()
        {
            lblTotal.Text = currentData == null ? "0" : currentData.Rows.Count.ToString();

            try
            {
                DateTime today = DateTime.Today;

                using (var repo = new UserActivityLogRepository())
                {
                    lblToday.Text = repo.CountUserActivity(today, today, "Login").ToString();
                    lblWeek.Text = repo.CountUserActivity(today, today, "Logout").ToString();
                    lblMonth.Text = repo.CountUserActivity(today, today, "FormEntry").ToString();
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

            // Hide columns we don't want to display directly
            if (gridActivity.Columns.Contains("CompanyId")) gridActivity.Columns["CompanyId"].Visible = false;
            if (gridActivity.Columns.Contains("BranchId")) gridActivity.Columns["BranchId"].Visible = false;
            if (gridActivity.Columns.Contains("FinYearId")) gridActivity.Columns["FinYearId"].Visible = false;

            SetColumn("UserActivityLogId", "Log ID", 70);
            SetColumn("CreatedOn", "Date & Time", 160);
            SetColumn("UserName", "User", 130);
            SetColumn("UserRole", "User Role", 110);
            SetColumn("CounterName", "Counter", 130);
            SetColumn("ActivityType", "Action", 110);
            SetColumn("ActivityDetails", "Details", 380);
            SetColumn("FormName", "Form Name", 180);
            SetColumn("SessionId", "Session ID", 90);

            if (gridActivity.Columns.Contains("CreatedOn"))
            {
                gridActivity.Columns["CreatedOn"].DefaultCellStyle.Format = "dd MMM yyyy hh:mm tt";
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
            StyleFilterText(txtSearch);
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

            MessageBox.Show(details, "User Log Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("No user activity log data to export.", "User Activity Log", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Export User Activity Log";
                dialog.Filter = "CSV Files (*.csv)|*.csv";
                dialog.FileName = $"UserActivityLog_{DateTime.Now:yyyyMMdd_HHmm}.csv";

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
                MessageBox.Show("User activity log exported successfully.", "User Activity Log", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
