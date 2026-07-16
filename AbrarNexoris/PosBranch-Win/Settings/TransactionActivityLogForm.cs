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
    public class TransactionActivityLogForm : Form
    {
        private readonly string logType;
        private readonly string title;
        private readonly string searchCaption;
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color border = Color.FromArgb(190, 226, 250);
        private readonly Color skyBlueOutline = Color.FromArgb(128, 183, 220);
        private bool applyButtonHot;
        private bool applyButtonPressed;
        private DataTable currentData;

        private UltraComboEditor cmbQuickDate;
        private UltraDateTimeEditor dtpFrom;
        private UltraDateTimeEditor dtpTo;
        private UltraComboEditor cmbUser;
        private UltraComboEditor cmbActivityType;
        private UltraTextEditor txtSearch;
        private UltraComboEditor cmbAction;
        private Button btnApply;
        private Button btnReset;
        private Button btnExport;
        private DataGridView gridActivity;
        private RoundedPanel gridFrame;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblTotal;
        private Label lblToday;
        private Label lblWeek;
        private Label lblMonth;
        private Label lblShowing;

        protected TransactionActivityLogForm(string logType, string title, string searchCaption)
        {
            this.logType = logType;
            this.title = title;
            this.searchCaption = searchCaption;

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
        }

        private void InitializeLogUi()
        {
            BackColor = Color.FromArgb(247, 252, 255);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            Name = title.Replace(" ", string.Empty);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White,
                Padding = new Padding(8)
            };
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

            var filters = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 13,
                BackColor = Color.Transparent
            };
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var filterTitle = new Label
            {
                Text = "Filters",
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = navy,
                TextAlign = ContentAlignment.MiddleLeft
            };

            cmbQuickDate = new UltraComboEditor();
            foreach (string quickDate in new[] { "Today", "Yesterday", "This Week", "This Month", "Previous Month", "This Year", "Previous Year", "Custom" })
            {
                cmbQuickDate.Items.Add(quickDate);
            }
            cmbQuickDate.ValueChanged += cmbQuickDate_SelectedIndexChanged;
            cmbQuickDate.Visible = false;
            dtpFrom = new UltraDateTimeEditor();
            dtpTo = new UltraDateTimeEditor();
            dtpFrom.ValueChanged += DatePicker_ValueChanged;
            dtpTo.ValueChanged += DatePicker_ValueChanged;
            cmbUser = new UltraComboEditor();
            cmbActivityType = new UltraComboEditor();
            txtSearch = new UltraTextEditor();
            cmbAction = new UltraComboEditor();
            btnApply = new Button { Text = "Apply Filters", Height = 32, Dock = DockStyle.Top };
            btnReset = new Button { Text = "Reset", Height = 32, Dock = DockStyle.Top };
            btnApply.Click += btnApply_Click;
            btnReset.Click += btnReset_Click;

            filters.Controls.Add(filterTitle);
            AddDateRangeFilter(filters);
            AddFilter(filters, "User", cmbUser);
            AddFilter(filters, "Activity Type", cmbActivityType);
            AddFilter(filters, searchCaption, txtSearch);
            AddFilter(filters, "Action", cmbAction);
            filters.Controls.Add(new Panel { Height = 12, Dock = DockStyle.Top });
            filters.Controls.Add(btnApply);
            filters.Controls.Add(new Panel { Height = 8, Dock = DockStyle.Top });
            filters.Controls.Add(btnReset);
            filterPanel.Controls.Add(filters);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10, 4, 0, 0),
                BackColor = Color.White
            };
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

            var titlePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = navy
            };
            lblSubtitle = new Label
            {
                Text = $"Track all activities performed in {logType}.",
                Dock = DockStyle.Top,
                Height = 20,
                ForeColor = Color.FromArgb(35, 77, 145)
            };
            titlePanel.Controls.Add(lblSubtitle);
            titlePanel.Controls.Add(lblTitle);

            var cards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight
            };

            lblTotal = CreateCard(cards, "Selected Range");
            lblToday = CreateCard(cards, "Today");
            lblWeek = CreateCard(cards, "This Week");
            lblMonth = CreateCard(cards, "This Month");

            gridFrame = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(2),
                BorderColor = Color.FromArgb(176, 224, 255),
                BorderRadius = 8
            };
            gridActivity = new DataGridView();
            gridFrame.Controls.Add(gridActivity);

            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            lblShowing = new Label
            {
                Text = "Showing 0 record(s)",
                Dock = DockStyle.Fill,
                ForeColor = navy,
                TextAlign = ContentAlignment.MiddleLeft
            };
            btnExport = new Button
            {
                Text = "Export",
                Dock = DockStyle.Fill,
                Height = 32
            };
            btnExport.Click += btnExport_Click;
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
        }

        private void AddDateRangeFilter(TableLayoutPanel panel)
        {
            var label = new Label
            {
                Text = "Date Range",
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = navy,
                TextAlign = ContentAlignment.BottomLeft,
                BackColor = Color.FromArgb(245, 250, 255)
            };

            var dateRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 30,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            dateRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            dateRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            dtpFrom.Dock = DockStyle.Fill;
            dtpFrom.Margin = new Padding(0, 0, 4, 0);
            dtpTo.Dock = DockStyle.Fill;
            dtpTo.Margin = new Padding(4, 0, 0, 0);

            dateRow.Controls.Add(dtpFrom, 0, 0);
            dateRow.Controls.Add(dtpTo, 1, 0);

            panel.Controls.Add(label);
            panel.Controls.Add(dateRow);
            panel.Controls.Add(new Panel { Height = 8, Dock = DockStyle.Top });
        }

        private void AddFilter(TableLayoutPanel panel, string caption, Control control)
        {
            var label = new Label
            {
                Text = caption,
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = navy,
                TextAlign = ContentAlignment.BottomLeft,
                BackColor = Color.FromArgb(245, 250, 255)
            };
            control.Dock = DockStyle.Top;
            control.Height = 30;
            panel.Controls.Add(label);
            panel.Controls.Add(control);
            panel.Controls.Add(new Panel { Height = 8, Dock = DockStyle.Top });
        }

        private Label CreateCard(FlowLayoutPanel host, string caption)
        {
            var panel = new RoundedPanel
            {
                Size = new Size(132, 50),
                Margin = new Padding(0, 0, 10, 6),
                BackColor = Color.FromArgb(250, 253, 255),
                BorderColor = Color.FromArgb(190, 226, 250),
                BorderRadius = 8
            };
            var labelCaption = new Label
            {
                Text = caption,
                Dock = DockStyle.Top,
                Height = 21,
                Padding = new Padding(9, 4, 0, 0),
                ForeColor = Color.FromArgb(54, 78, 120)
            };
            var labelValue = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                ForeColor = navy,
                Padding = new Padding(9, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };
            panel.Controls.Add(labelValue);
            panel.Controls.Add(labelCaption);
            host.Controls.Add(panel);
            return labelValue;
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
            cmbAction.Items.Add("HOLD");
            cmbAction.Items.Add("UPDATE");
            cmbAction.Items.Add("DELETE");

            try
            {
                using (var repo = new TransactionActivityLogRepository())
                {
                    foreach (DataRow row in repo.GetActivityUsers(logType).Rows)
                    {
                        cmbUser.Items.Add(Convert.ToString(row["Value"]));
                    }

                    foreach (DataRow row in repo.GetActivityTypes(logType).Rows)
                    {
                        cmbActivityType.Items.Add(Convert.ToString(row["Value"]));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load activity filters: " + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                using (var repo = new TransactionActivityLogRepository())
                {
                    currentData = repo.GetActivityLog(
                        logType,
                        GetDateValue(dtpFrom),
                        GetDateValue(dtpTo),
                        userName,
                        activityType,
                        txtSearch.Text.Trim());
                }

                ApplyDisplayLogNumbers();
                gridActivity.DataSource = currentData;
                ConfigureGridColumns();
                UpdateSummaryCards();
                lblShowing.Text = $"Showing {currentData.Rows.Count} record(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load activity log: " + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyDisplayLogNumbers()
        {
            if (currentData == null)
            {
                return;
            }

            const string displayColumn = "DisplayLogNo";
            if (currentData.Columns.Contains(displayColumn))
            {
                currentData.Columns[displayColumn].SetOrdinal(0);
                return;
            }

            if (!currentData.Columns.Contains(displayColumn))
            {
                currentData.Columns.Add(displayColumn, typeof(int));
            }

            int logNo = currentData.Rows.Count;
            foreach (DataRow row in currentData.Rows)
            {
                row[displayColumn] = logNo--;
            }

            currentData.Columns[displayColumn].SetOrdinal(0);
        }

        private void UpdateSummaryCards()
        {
            lblTotal.Text = currentData == null ? "0" : currentData.Rows.Count.ToString();

            try
            {
                DateTime today = DateTime.Today;
                DateTime weekStart = today.AddDays(-(int)today.DayOfWeek);
                DateTime monthStart = new DateTime(today.Year, today.Month, 1);

                using (var repo = new TransactionActivityLogRepository())
                {
                    lblToday.Text = repo.CountActivity(logType, today, today).ToString();
                    lblWeek.Text = repo.CountActivity(logType, weekStart, today).ToString();
                    lblMonth.Text = repo.CountActivity(logType, monthStart, today).ToString();
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

            SetColumn("DisplayLogNo", "#", 55);
            SetColumn("CreatedOn", "Date & Time", 155);
            SetColumn("UserName", "User", 115);
            SetColumn("UserId", "User ID", 75);
            SetColumn("ActivityType", "Action", 110);
            SetColumn("TransactionNo", logType == "Purchase" ? "Purchase No" : "Bill No", 110);
            SetColumn("InvoiceNo", "Invoice No", 130);
            SetColumn("PartyName", logType == "Purchase" ? "Vendor" : "Customer", 220);
            SetColumn("PaymentMode", "Payment", 120);
            SetColumn("NetAmount", "Net Amount", 115);
            SetColumn("Qty", "Qty", 80);
            SetColumn("Cost", "Cost", 90);
            SetColumn("Unit", "Unit", 80);
            SetColumn("Barcode", "Barcode", 135);
            SetColumn("SPrice", "S/Price", 90);
            SetColumn("TaxAmt", "TaxAmt", 90);
            SetColumn("TaxPer", "Tax %", 80);
            SetColumn("BaseAmount", "BaseAmount", 110);
            SetColumn("Packing", "Packing", 80);
            SetColumn("RetailPrice", "Retail Price", 95);
            SetColumn("Free", "Free", 70);
            SetColumn("UnitSP", "Unit SP", 90);
            SetColumn("TaxType", "Tax Type", 90);
            SetColumn("Gross", "Gross", 95);
            SetColumn("ActivityDetails", "Details", 420);
            SetColumn("CompanyId", "Company", 80);
            SetColumn("BranchId", "Branch", 75);
            SetColumn("FinYearId", "Fin Year", 80);
            SetColumn("CounterName", "Counter", 130);
            SetColumn("CounterId", "Counter ID", 85);
            SetColumn("CounterSessionId", "Session", 90);
            HideColumn("CompanyId");
            HideColumn("BranchId");
            HideColumn("FinYearId");
            HideColumn("ActivityLogId");

            if (logType == "Sales")
            {
                HideColumn("Packing");
                HideColumn("RetailPrice");
                HideColumn("Free");
                HideColumn("UnitSP");
                HideColumn("TaxType");
                HideColumn("Gross");
            }
            else if (logType == "Purchase")
            {
                SetColumn("SPrice", "Selling Price", 100);
                SetColumn("BaseAmount", "Base Cost", 110);
            }
            else
            {
                HideColumn("SPrice");
                HideColumn("TaxAmt");
                HideColumn("TaxPer");
                HideColumn("BaseAmount");
                HideColumn("Packing");
                HideColumn("RetailPrice");
                HideColumn("Free");
                HideColumn("UnitSP");
                HideColumn("TaxType");
                HideColumn("Gross");
            }

            EnsureDetailsButtonColumn();

            if (gridActivity.Columns.Contains("CreatedOn"))
            {
                gridActivity.Columns["CreatedOn"].DefaultCellStyle.Format = "dd MMM yyyy hh:mm tt";
            }

            foreach (string numericColumn in new[] { "NetAmount", "Qty", "Cost", "SPrice", "TaxAmt", "TaxPer", "BaseAmount", "Packing", "RetailPrice", "Free", "UnitSP", "Gross" })
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

        private void HideColumn(string name)
        {
            if (gridActivity.Columns.Contains(name))
            {
                gridActivity.Columns[name].Visible = false;
            }
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

            MessageBox.Show(details, "Activity Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void StyleGrid()
        {
            StyleFilterDate(dtpFrom);
            StyleFilterDate(dtpTo);
            StyleFilterCombo(cmbQuickDate, true);
            StyleFilterCombo(cmbUser, true);
            StyleFilterCombo(cmbActivityType, true);
            StyleFilterCombo(cmbAction, true);
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
            gridActivity.AllowUserToAddRows = false;
            gridActivity.AllowUserToDeleteRows = false;
            gridActivity.ReadOnly = true;
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
            gridActivity.CellContentClick -= gridActivity_CellContentClick;
            gridActivity.CellContentClick += gridActivity_CellContentClick;
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
            btnApply.Paint += BtnApply_Paint;
            btnApply.MouseEnter += (s, e) => { applyButtonHot = true; btnApply.Invalidate(); };
            btnApply.MouseLeave += (s, e) => { applyButtonHot = false; applyButtonPressed = false; btnApply.Invalidate(); };
            btnApply.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { applyButtonPressed = true; btnApply.Invalidate(); } };
            btnApply.MouseUp += (s, e) => { applyButtonPressed = false; btnApply.Invalidate(); };

            btnReset.UseVisualStyleBackColor = false;
            btnReset.FlatStyle = FlatStyle.Flat;
            btnReset.BackColor = Color.White;
            btnReset.ForeColor = navy;
            btnReset.FlatAppearance.BorderColor = border;
            btnReset.FlatAppearance.MouseOverBackColor = Color.FromArgb(242, 249, 255);
            btnReset.FlatAppearance.MouseDownBackColor = Color.FromArgb(232, 246, 255);

            btnExport.UseVisualStyleBackColor = false;
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.BackColor = Color.White;
            btnExport.ForeColor = navy;
            btnExport.FlatAppearance.BorderColor = border;
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

        private void btnApply_Click(object sender, EventArgs e)
        {
            LoadActivityLog();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            cmbUser.SelectedIndex = 0;
            cmbActivityType.SelectedIndex = 0;
            cmbAction.SelectedIndex = 0;
            txtSearch.Text = string.Empty;
            cmbQuickDate.Text = "Today";
            ApplyQuickDate();
            LoadActivityLog();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportCurrentData();
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

        private void ExportCurrentData()
        {
            if (currentData == null || currentData.Rows.Count == 0)
            {
                MessageBox.Show("No activity log data to export.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Export Activity Log";
                dialog.Filter = "CSV Files (*.csv)|*.csv";
                dialog.FileName = $"{logType}ActivityLog_{DateTime.Now:yyyyMMdd_HHmm}.csv";

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
                MessageBox.Show("Activity log exported successfully.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
