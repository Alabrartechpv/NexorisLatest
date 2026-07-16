using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;
using Repository.ReportRepository;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public class FinalAnalysis : Form
    {
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color border = Color.FromArgb(190, 226, 250);
        private readonly Color skyBlueOutline = Color.FromArgb(128, 183, 220);
        private readonly Color positiveClr = Color.FromArgb(0, 140, 70);
        private readonly Color negativeClr = Color.FromArgb(200, 30, 30);
        private readonly Color warnClr = Color.FromArgb(180, 100, 0);

        private UltraComboEditor cmbQuickDate;
        private UltraDateTimeEditor dtpFrom;
        private UltraDateTimeEditor dtpTo;
        private UltraComboEditor cmbScope;
        private Button btnModeAll;
        private Button btnModeCounter;
        private Button btnModeUser;
        private Label lblScope;
        private Button btnApply;
        private Button btnReset;
        private Button btnExport;
        private DataGridView gridAnalysis;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblTotalSale;
        private Label lblTotalProfit;
        private Label lblOutstanding;
        private Label lblOutOfStock;
        private Label lblShowing;
        private DataTable currentData;
        private AnalysisScopeMode scopeMode = AnalysisScopeMode.All;
        private FinalAnalysisModel currentModel;
        private ContextMenuStrip metricMenu;
        private int metricMenuRowIndex = -1;

        public FinalAnalysis()
        {
            InitializeAnalysisUi();
            StyleGrid();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadFilterLists();
            cmbQuickDate.Text = "Today";
            ApplyQuickDate();
            LoadData();
        }

        private void InitializeAnalysisUi()
        {
            BackColor = Color.FromArgb(247, 252, 255);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FinalAnalysis";

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

            dtpFrom = new UltraDateTimeEditor();
            dtpTo = new UltraDateTimeEditor();
            dtpFrom.ValueChanged += DatePicker_ValueChanged;
            dtpTo.ValueChanged += DatePicker_ValueChanged;

            cmbScope = new UltraComboEditor();
            btnModeAll = MakeModeButton("ALL");
            btnModeCounter = MakeModeButton("COUNTER");
            btnModeUser = MakeModeButton("USER");
            btnModeAll.Click += (s, e) => SetScopeMode(AnalysisScopeMode.All);
            btnModeCounter.Click += (s, e) => SetScopeMode(AnalysisScopeMode.Counter);
            btnModeUser.Click += (s, e) => SetScopeMode(AnalysisScopeMode.User);

            btnApply = new Button { Text = "Apply Filters", Height = 32, Dock = DockStyle.Top };
            btnReset = new Button { Text = "Reset", Height = 32, Dock = DockStyle.Top };
            btnApply.Click += btnApply_Click;
            btnReset.Click += btnReset_Click;

            filters.Controls.Add(filterTitle);
            AddFilter(filters, "Quick Dates", cmbQuickDate);
            AddDateRangeFilter(filters);
            AddScopeModeFilter(filters);
            lblScope = AddFilter(filters, "Selection", cmbScope);
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
                Text = "Business Summary - Final Analysis",
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = navy
            };
            lblSubtitle = new Label
            {
                Text = "Select date range and classify by all, counter, or user.",
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
            lblTotalSale = CreateCard(cards, "Total Sale");
            lblTotalProfit = CreateCard(cards, "Total Profit");
            lblOutstanding = CreateCard(cards, "Outstanding");
            lblOutOfStock = CreateCard(cards, "Out of Stock");

            var gridFrame = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(2),
                BorderColor = Color.FromArgb(176, 224, 255),
                BorderRadius = 8
            };
            gridAnalysis = new DataGridView();
            gridAnalysis.CellFormatting += gridAnalysis_CellFormatting;
            gridAnalysis.DataBindingComplete += gridAnalysis_DataBindingComplete;
            gridAnalysis.CellMouseDown += gridAnalysis_CellMouseDown;
            gridFrame.Controls.Add(gridAnalysis);

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
            btnExport = new Button { Text = "Export", Dock = DockStyle.Fill, Height = 32 };
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

        private Label AddFilter(TableLayoutPanel panel, string caption, Control control)
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
            return label;
        }

        private void AddScopeModeFilter(TableLayoutPanel panel)
        {
            var label = new Label
            {
                Text = "Classify By",
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = navy,
                TextAlign = ContentAlignment.BottomLeft,
                BackColor = Color.FromArgb(245, 250, 255)
            };

            var modeRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 30,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            modeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            modeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            modeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            modeRow.Controls.Add(btnModeAll, 0, 0);
            modeRow.Controls.Add(btnModeCounter, 1, 0);
            modeRow.Controls.Add(btnModeUser, 2, 0);

            panel.Controls.Add(label);
            panel.Controls.Add(modeRow);
            panel.Controls.Add(new Panel { Height = 8, Dock = DockStyle.Top });
        }

        private Label CreateCard(FlowLayoutPanel host, string caption)
        {
            var panel = new RoundedPanel
            {
                Size = new Size(145, 50),
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
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
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
            SetScopeMode(scopeMode);
        }

        private void LoadScopeItems()
        {
            cmbScope.Items.Clear();

            try
            {
                using (var repo = new FinalAnalysisRepository())
                {
                    if (scopeMode == AnalysisScopeMode.Counter)
                    {
                        foreach (DataRow row in repo.GetCounters().Rows)
                        {
                            int counterId = Convert.ToInt32(row["Id"]);
                            string counterName = Convert.ToString(row["Name"]);
                            cmbScope.Items.Add(new ValueListItem(new AnalysisScopeSelection(0, counterId), counterName));
                        }
                    }
                    else if (scopeMode == AnalysisScopeMode.User)
                    {
                        DataTable userScopes = repo.GetUserScopes();
                        if (userScopes.Rows.Count > 0)
                        {
                            foreach (DataRow row in userScopes.Rows)
                            {
                                int userId = Convert.ToInt32(row["UserId"]);
                                int counterId = Convert.ToInt32(row["CounterId"]);
                                string displayName = Convert.ToString(row["DisplayName"]);
                                cmbScope.Items.Add(new ValueListItem(new AnalysisScopeSelection(userId, counterId), displayName));
                            }
                        }
                        else
                        {
                            foreach (DataRow row in repo.GetUsers().Rows)
                            {
                                int userId = Convert.ToInt32(row["Id"]);
                                string userName = Convert.ToString(row["Name"]);
                                cmbScope.Items.Add(new ValueListItem(new AnalysisScopeSelection(userId, 0), userName + " - All Counters"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load final analysis filters: " + ex.Message, "Final Analysis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (cmbScope.Items.Count > 0)
            {
                cmbScope.SelectedIndex = 0;
            }
        }

        private void LoadData()
        {
            try
            {
                btnApply.Enabled = false;
                using (var repo = new FinalAnalysisRepository())
                {
                    AnalysisScopeSelection selection = GetSelectedScope();
                    FinalAnalysisModel model = repo.GetFinalAnalysis(
                        GetDateValue(dtpFrom),
                        GetDateValue(dtpTo),
                        selection.UserId,
                        selection.CounterId);

                    currentModel = model;
                    currentData = model.DetailTable;
                    gridAnalysis.DataSource = currentData;
                    ConfigureGridColumns();
                    UpdateSummaryCards(model);
                    lblSubtitle.Text = $"{model.FromDate:dd MMM yyyy} to {model.ToDate:dd MMM yyyy} | {model.UserName} | {model.CounterName}";
                    lblShowing.Text = $"Showing {currentData.Rows.Count} record(s)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load final analysis: " + ex.Message, "Final Analysis", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnApply.Enabled = true;
            }
        }

        private void ConfigureGridColumns()
        {
            if (gridAnalysis.Columns.Count == 0)
            {
                return;
            }

            EnsureBusinessMetricTextColumn();
            SetColumn("BusinessMetric", "Business Metric", 260);
            SetColumn("Amount", "Amount", 130);
            SetColumn("Count", "Count", 95);
            SetColumn("FromDate", "From Date", 115);
            SetColumn("ToDate", "To Date", 115);
            SetColumn("User", "User", 135);
            SetColumn("Counter", "Counter", 135);
            HideColumn("Category");

            if (gridAnalysis.Columns.Contains("Amount"))
            {
                gridAnalysis.Columns["Amount"].DefaultCellStyle.Format = "N2";
                gridAnalysis.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (gridAnalysis.Columns.Contains("Count"))
            {
                gridAnalysis.Columns["Count"].DefaultCellStyle.Format = "N0";
                gridAnalysis.Columns["Count"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (gridAnalysis.Columns.Contains("FromDate"))
            {
                gridAnalysis.Columns["FromDate"].DefaultCellStyle.Format = "dd MMM yyyy";
            }
            if (gridAnalysis.Columns.Contains("ToDate"))
            {
                gridAnalysis.Columns["ToDate"].DefaultCellStyle.Format = "dd MMM yyyy";
            }
        }

        private void UpdateSummaryCards(FinalAnalysisModel model)
        {
            lblTotalSale.Text = model.TotalSale.ToString("N2");
            lblTotalProfit.Text = model.TotalProfit.ToString("N2");
            lblTotalProfit.ForeColor = model.TotalProfit >= 0 ? positiveClr : negativeClr;
            lblOutstanding.Text = model.TotalOutstandingVendor.ToString("N2");
            lblOutOfStock.Text = model.OutOfStockItems.ToString("N0");
        }

        private void StyleGrid()
        {
            StyleFilterCombo(cmbQuickDate, true);
            StyleFilterCombo(cmbScope, true);
            StyleFilterDate(dtpFrom);
            StyleFilterDate(dtpTo);
            StyleActionButtons();

            gridAnalysis.Dock = DockStyle.Fill;
            gridAnalysis.Margin = Padding.Empty;
            gridAnalysis.EnableHeadersVisualStyles = false;
            gridAnalysis.BorderStyle = BorderStyle.None;
            gridAnalysis.BackgroundColor = Color.FromArgb(247, 252, 255);
            gridAnalysis.GridColor = border;
            gridAnalysis.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            gridAnalysis.ScrollBars = ScrollBars.Both;
            gridAnalysis.AllowUserToAddRows = false;
            gridAnalysis.AllowUserToDeleteRows = false;
            gridAnalysis.ReadOnly = true;
            gridAnalysis.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridAnalysis.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 246, 255);
            gridAnalysis.ColumnHeadersDefaultCellStyle.ForeColor = navy;
            gridAnalysis.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            gridAnalysis.DefaultCellStyle.BackColor = Color.White;
            gridAnalysis.DefaultCellStyle.ForeColor = Color.FromArgb(30, 62, 120);
            gridAnalysis.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 238, 255);
            gridAnalysis.DefaultCellStyle.SelectionForeColor = navy;
            gridAnalysis.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);
            gridAnalysis.RowTemplate.Height = 30;
        }

        private void StyleActionButtons()
        {
            StyleSkyBlueButton(btnApply);
            btnApply.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);

            StyleSkyBlueButton(btnReset);
            btnReset.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);

            btnExport.UseVisualStyleBackColor = false;
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.BackColor = Color.White;
            btnExport.ForeColor = navy;
            btnExport.FlatAppearance.BorderColor = border;
        }

        private void StyleSkyBlueButton(Button button)
        {
            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.FromArgb(38, 119, 237);
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderColor = Color.FromArgb(38, 119, 237);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(54, 139, 250);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(26, 96, 205);
            button.Paint -= SkyBlueButton_Paint;
            button.Paint += SkyBlueButton_Paint;
            button.MouseEnter -= SkyBlueButton_MouseStateChanged;
            button.MouseLeave -= SkyBlueButton_MouseStateChanged;
            button.MouseDown -= SkyBlueButton_MouseStateChanged;
            button.MouseUp -= SkyBlueButton_MouseStateChanged;
            button.MouseEnter += SkyBlueButton_MouseStateChanged;
            button.MouseLeave += SkyBlueButton_MouseStateChanged;
            button.MouseDown += SkyBlueButton_MouseStateChanged;
            button.MouseUp += SkyBlueButton_MouseStateChanged;
            button.Invalidate();
        }

        private void SkyBlueButton_MouseStateChanged(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.Invalidate();
            }
        }

        private void SkyBlueButton_Paint(object sender, PaintEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }

            Color backColor = button.ClientRectangle.Contains(button.PointToClient(Cursor.Position))
                ? Color.FromArgb(54, 139, 250)
                : Color.FromArgb(38, 119, 237);

            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, button.ClientRectangle);
            }

            TextRenderer.DrawText(
                e.Graphics,
                button.Text,
                button.Font,
                button.ClientRectangle,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            using (var pen = new Pen(Color.FromArgb(38, 119, 237)))
            {
                Rectangle borderRect = button.ClientRectangle;
                borderRect.Width -= 1;
                borderRect.Height -= 1;
                e.Graphics.DrawRectangle(pen, borderRect);
            }
        }

        private Button MakeModeButton(string text)
        {
            return new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 4, 0),
                Height = 30,
                UseVisualStyleBackColor = false,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
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

        private void SetColumn(string name, string header, int width)
        {
            if (!gridAnalysis.Columns.Contains(name))
            {
                return;
            }

            DataGridViewColumn column = gridAnalysis.Columns[name];
            column.HeaderText = header;
            column.Width = width;
            column.MinimumWidth = Math.Min(width, 80);
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        }

        private void HideColumn(string name)
        {
            if (gridAnalysis.Columns.Contains(name))
            {
                gridAnalysis.Columns[name].Visible = false;
            }
        }

        private void gridAnalysis_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || !gridAnalysis.Columns.Contains("Category"))
            {
                return;
            }

            string category = Convert.ToString(gridAnalysis.Rows[e.RowIndex].Cells["Category"].Value);
            if (gridAnalysis.Columns[e.ColumnIndex].Name != "Amount" && gridAnalysis.Columns[e.ColumnIndex].Name != "Count")
            {
                return;
            }

            if (category == "profit")
            {
                e.CellStyle.ForeColor = positiveClr;
                e.CellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            }
            else if (category == "loss")
            {
                e.CellStyle.ForeColor = negativeClr;
                e.CellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            }
            else if (category == "warn")
            {
                e.CellStyle.ForeColor = warnClr;
                e.CellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            }
        }

        private void gridAnalysis_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            EnsureBusinessMetricTextColumn();

            if (gridAnalysis.Columns.Contains("BusinessMetric"))
            {
                gridAnalysis.Columns["BusinessMetric"].ReadOnly = true;
            }
        }

        private void gridAnalysis_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || !gridAnalysis.Columns.Contains("BusinessMetric") || currentModel == null)
            {
                return;
            }

            string columnName = gridAnalysis.Columns[e.ColumnIndex].Name;
            string metric = Convert.ToString(gridAnalysis.Rows[e.RowIndex].Cells["BusinessMetric"].Value);

            if (columnName == "Count" && string.Equals(metric, "Partial Payment", StringComparison.OrdinalIgnoreCase))
            {
                gridAnalysis.CurrentCell = gridAnalysis.Rows[e.RowIndex].Cells[e.ColumnIndex];
                gridAnalysis.Rows[e.RowIndex].Selected = true;
                ShowPartialPaymentDetails();
                return;
            }

            if (columnName != "BusinessMetric")
            {
                return;
            }

            if (!IsPurchaseMetric(metric) && !IsSalesMetric(metric))
            {
                return;
            }

            gridAnalysis.CurrentCell = gridAnalysis.Rows[e.RowIndex].Cells[e.ColumnIndex];
            gridAnalysis.Rows[e.RowIndex].Selected = true;
            ShowMetricMenu(e.RowIndex, IsPurchaseMetric(metric));
        }

        private void EnsureBusinessMetricTextColumn()
        {
            if (!gridAnalysis.Columns.Contains("BusinessMetric"))
            {
                return;
            }

            DataGridViewColumn column = gridAnalysis.Columns["BusinessMetric"];
            if (column is DataGridViewComboBoxColumn)
            {
                int index = column.Index;
                int displayIndex = column.DisplayIndex;
                int width = column.Width;
                bool visible = column.Visible;
                string headerText = column.HeaderText;

                var textColumn = new DataGridViewTextBoxColumn
                {
                    Name = column.Name,
                    DataPropertyName = column.DataPropertyName,
                    HeaderText = headerText,
                    Width = width,
                    Visible = visible,
                    ReadOnly = true
                };

                gridAnalysis.Columns.RemoveAt(index);
                gridAnalysis.Columns.Insert(index, textColumn);
                textColumn.DisplayIndex = displayIndex;
            }

            foreach (DataGridViewRow row in gridAnalysis.Rows)
            {
                DataGridViewCell cell = row.Cells["BusinessMetric"];
                if (cell is DataGridViewComboBoxCell)
                {
                    object value = cell.Value;
                    var textCell = new DataGridViewTextBoxCell { Value = value };
                    row.Cells["BusinessMetric"] = textCell;
                }
            }
        }

        private void ShowMetricMenu(int rowIndex, bool isPurchaseMetric)
        {
            metricMenuRowIndex = rowIndex;
            if (metricMenu == null)
            {
                metricMenu = new ContextMenuStrip();
                metricMenu.ShowImageMargin = false;
            }

            metricMenu.Items.Clear();
            string currentMetric = Convert.ToString(gridAnalysis.Rows[rowIndex].Cells["BusinessMetric"].Value);
            string[] options = isPurchaseMetric ? GetPurchaseMetricOptions() : GetSalesMetricOptions();
            foreach (string option in options)
            {
                var item = new ToolStripMenuItem(option);
                item.Checked = string.Equals(option, currentMetric, StringComparison.OrdinalIgnoreCase);
                item.Click += metricMenuItem_Click;
                metricMenu.Items.Add(item);
            }

            Rectangle cellBounds = gridAnalysis.GetCellDisplayRectangle(
                gridAnalysis.Columns["BusinessMetric"].Index,
                rowIndex,
                true);
            Point location = new Point(cellBounds.Left, cellBounds.Bottom);
            metricMenu.Show(gridAnalysis, location);
        }

        private void ShowPartialPaymentDetails()
        {
            try
            {
                AnalysisScopeSelection selection = GetSelectedScope();
                DataTable details;
                using (var repo = new FinalAnalysisRepository())
                {
                    details = repo.GetPartialPaymentSaleDetails(
                        GetDateValue(dtpFrom),
                        GetDateValue(dtpTo),
                        selection.UserId,
                        selection.CounterId);
                }

                if (details == null || details.Rows.Count == 0)
                {
                    MessageBox.Show("No partial payment sale details found for the selected filters.", "Partial Payment Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var dialog = CreateDetailsDialog("Partial Payment Sale Details", details))
                {
                    dialog.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load partial payment details: " + ex.Message, "Partial Payment Details", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Form CreateDetailsDialog(string title, DataTable data)
        {
            var dialog = new Form
            {
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(980, 540),
                MinimizeBox = false,
                MaximizeBox = true,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };

            var header = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 38,
                Padding = new Padding(12, 8, 0, 0),
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = navy
            };

            var detailsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = data,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };

            detailsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 246, 255);
            detailsGrid.ColumnHeadersDefaultCellStyle.ForeColor = navy;
            detailsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            detailsGrid.DefaultCellStyle.ForeColor = Color.FromArgb(30, 62, 120);
            detailsGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 238, 255);
            detailsGrid.DefaultCellStyle.SelectionForeColor = navy;
            detailsGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);
            detailsGrid.DataBindingComplete += (s, e) => ConfigurePartialPaymentDetailsGrid(detailsGrid);

            dialog.Controls.Add(detailsGrid);
            dialog.Controls.Add(header);
            return dialog;
        }

        private void ConfigurePartialPaymentDetailsGrid(DataGridView detailsGrid)
        {
            ConfigureDetailsColumn(detailsGrid, "BillNo", "Bill No", 90, DataGridViewContentAlignment.MiddleRight, null);
            ConfigureDetailsColumn(detailsGrid, "BillDate", "Bill Date", 105, DataGridViewContentAlignment.MiddleLeft, "dd MMM yyyy");
            ConfigureDetailsColumn(detailsGrid, "ItemName", "Item Name", 210, DataGridViewContentAlignment.MiddleLeft, null);
            ConfigureDetailsColumn(detailsGrid, "Qty", "Qty", 75, DataGridViewContentAlignment.MiddleRight, "N2");
            ConfigureDetailsColumn(detailsGrid, "SellingPrice", "Selling Price", 110, DataGridViewContentAlignment.MiddleRight, "N2");
            ConfigureDetailsColumn(detailsGrid, "LineTotal", "Line Total", 110, DataGridViewContentAlignment.MiddleRight, "N2");
            ConfigureDetailsColumn(detailsGrid, "BillTotal", "Bill Total", 110, DataGridViewContentAlignment.MiddleRight, "N2");
            ConfigureDetailsColumn(detailsGrid, "PartiallyPaid", "Partially Paid", 120, DataGridViewContentAlignment.MiddleRight, "N2");
            ConfigureDetailsColumn(detailsGrid, "Balance", "Balance", 100, DataGridViewContentAlignment.MiddleRight, "N2");
            ConfigureDetailsColumn(detailsGrid, "PaymentSplit", "Payment Split", 220, DataGridViewContentAlignment.MiddleLeft, null);
        }

        private void ConfigureDetailsColumn(DataGridView grid, string name, string header, int width, DataGridViewContentAlignment alignment, string format)
        {
            if (!grid.Columns.Contains(name))
            {
                return;
            }

            DataGridViewColumn column = grid.Columns[name];
            column.HeaderText = header;
            column.Width = width;
            column.DefaultCellStyle.Alignment = alignment;
            if (!string.IsNullOrWhiteSpace(format))
            {
                column.DefaultCellStyle.Format = format;
            }
        }

        private string[] GetPurchaseMetricOptions()
        {
            return new[]
            {
                "Total Purchase",
                "Total Purchase by Cash",
                "Total Purchase by Credit",
                "Total Purchase by UPI",
                "Total Purchase by Bank Transfer",
                "Total Purchase by Cheque"
            };
        }

        private string[] GetSalesMetricOptions()
        {
            return new[]
            {
                "Total Sale",
                "Total Sales by Cash",
                "Total Sales by Credit",
                "Total Sales by UPI",
                "Total Sales by Bank Transfer",
                "Total Sales by Cheque",
                "Total Sales by Card"
            };
        }

        private void metricMenuItem_Click(object sender, EventArgs e)
        {
            if (metricMenuRowIndex < 0 || metricMenuRowIndex >= gridAnalysis.Rows.Count)
            {
                return;
            }

            var item = sender as ToolStripMenuItem;
            if (item == null)
            {
                return;
            }

            UpdateMetricRow(metricMenuRowIndex, item.Text);
        }

        private void UpdateMetricRow(int rowIndex, string metric)
        {
            decimal amount;
            int count;
            string category;
            GetDynamicMetricValue(metric, out amount, out count, out category);
            gridAnalysis.Rows[rowIndex].Cells["BusinessMetric"].Value = metric;
            gridAnalysis.Rows[rowIndex].Cells["Amount"].Value = amount;
            gridAnalysis.Rows[rowIndex].Cells["Count"].Value = count;
            gridAnalysis.Rows[rowIndex].Cells["Category"].Value = category;
        }

        private void GetDynamicMetricValue(string metric, out decimal amount, out int count, out string category)
        {
            amount = 0;
            count = 0;
            category = "currency";

            switch (metric)
            {
                case "Total Purchase":
                    amount = currentModel.TotalPurchase;
                    count = currentModel.TotalPurchaseCount;
                    break;
                case "Total Purchase by Cash":
                    amount = currentModel.TotalPurchaseCash;
                    count = currentModel.TotalPurchaseCashCount;
                    break;
                case "Total Purchase by Credit":
                    amount = currentModel.TotalPurchaseCredit;
                    count = currentModel.TotalPurchaseCreditCount;
                    break;
                case "Total Purchase by UPI":
                    amount = currentModel.TotalPurchaseUpi;
                    count = currentModel.TotalPurchaseUpiCount;
                    break;
                case "Total Purchase by Bank Transfer":
                    amount = currentModel.TotalPurchaseBank;
                    count = currentModel.TotalPurchaseBankCount;
                    break;
                case "Total Purchase by Cheque":
                    amount = currentModel.TotalPurchaseCheque;
                    count = currentModel.TotalPurchaseChequeCount;
                    break;
                case "Total Sale":
                    amount = currentModel.TotalSale;
                    count = currentModel.TotalSaleCount;
                    break;
                case "Total Sales by Cash":
                    amount = currentModel.TotalSaleCash;
                    count = currentModel.TotalSaleCashCount;
                    break;
                case "Total Sales by Credit":
                    amount = currentModel.TotalSaleCredit;
                    count = currentModel.TotalSaleCreditCount;
                    break;
                case "Total Sales by UPI":
                    amount = currentModel.TotalSaleUpi;
                    count = currentModel.TotalSaleUpiCount;
                    break;
                case "Total Sales by Bank Transfer":
                    amount = currentModel.TotalSaleBank;
                    count = currentModel.TotalSaleBankCount;
                    break;
                case "Total Sales by Cheque":
                    amount = currentModel.TotalSaleCheque;
                    count = currentModel.TotalSaleChequeCount;
                    break;
                case "Total Sales by Card":
                    amount = currentModel.TotalSaleCard;
                    count = currentModel.TotalSaleCardCount;
                    break;
            }
        }

        private bool IsPurchaseMetric(string metric)
        {
            return !string.IsNullOrWhiteSpace(metric) && metric.StartsWith("Total Purchase", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsSalesMetric(string metric)
        {
            return !string.IsNullOrWhiteSpace(metric) &&
                   (metric.Equals("Total Sale", StringComparison.OrdinalIgnoreCase) ||
                    metric.StartsWith("Total Sales", StringComparison.OrdinalIgnoreCase));
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
            LoadData();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            SetScopeMode(AnalysisScopeMode.All);
            cmbQuickDate.Text = "Today";
            ApplyQuickDate();
            LoadData();
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

        private AnalysisScopeSelection GetSelectedScope()
        {
            if (scopeMode == AnalysisScopeMode.All)
            {
                return new AnalysisScopeSelection(0, 0);
            }

            ValueListItem item = cmbScope.SelectedItem as ValueListItem;
            return item == null || !(item.DataValue is AnalysisScopeSelection)
                ? new AnalysisScopeSelection(0, 0)
                : (AnalysisScopeSelection)item.DataValue;
        }

        private void SetScopeMode(AnalysisScopeMode mode)
        {
            scopeMode = mode;
            cmbScope.Enabled = mode != AnalysisScopeMode.All;
            cmbScope.Items.Clear();

            if (lblScope != null)
            {
                lblScope.Text = mode == AnalysisScopeMode.Counter ? "Counter" : mode == AnalysisScopeMode.User ? "User / Counter" : "Selection";
            }

            if (mode == AnalysisScopeMode.All)
            {
                cmbScope.Items.Add(new ValueListItem(new AnalysisScopeSelection(0, 0), "All Users / All Counters"));
                cmbScope.SelectedIndex = 0;
            }
            else
            {
                LoadScopeItems();
            }

            StyleModeButtons();
        }

        private void StyleModeButtons()
        {
            StyleModeButton(btnModeAll, scopeMode == AnalysisScopeMode.All);
            StyleModeButton(btnModeCounter, scopeMode == AnalysisScopeMode.Counter);
            StyleModeButton(btnModeUser, scopeMode == AnalysisScopeMode.User);
        }

        private void StyleModeButton(Button button, bool active)
        {
            StyleSkyBlueButton(button);
        }

        private void ExportCurrentData()
        {
            if (currentData == null || currentData.Rows.Count == 0 || currentModel == null)
            {
                MessageBox.Show("No final analysis data to export.", "Final Analysis", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Export Final Analysis";
                dialog.Filter = "CSV Files (*.csv)|*.csv";
                dialog.FileName = $"FinalAnalysis_{DateTime.Now:yyyyMMdd_HHmm}.csv";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var builder = new StringBuilder();
                AppendFinalAnalysisSummaryExport(builder);
                AppendPartialPaymentDetailsExport(builder);

                File.WriteAllText(dialog.FileName, builder.ToString());
                MessageBox.Show("Final analysis exported successfully.", "Final Analysis", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AppendFinalAnalysisSummaryExport(StringBuilder builder)
        {
            AppendCsvRow(builder, "Business Metric", "Amount", "Count", "From Date", "To Date", "User", "Counter");

            bool purchaseMetricsExported = false;
            bool salesMetricsExported = false;

            foreach (DataGridViewRow row in gridAnalysis.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                string metric = Convert.ToString(row.Cells["BusinessMetric"].Value);
                if (IsPurchaseMetric(metric))
                {
                    if (!purchaseMetricsExported)
                    {
                        foreach (string option in GetPurchaseMetricOptions())
                        {
                            AppendDynamicMetricExportRow(builder, option);
                        }
                        purchaseMetricsExported = true;
                    }
                    continue;
                }

                if (IsSalesMetric(metric))
                {
                    if (!salesMetricsExported)
                    {
                        foreach (string option in GetSalesMetricOptions())
                        {
                            AppendDynamicMetricExportRow(builder, option);
                        }
                        salesMetricsExported = true;
                    }
                    continue;
                }

                AppendCsvRow(
                    builder,
                    metric,
                    FormatExportValue(row.Cells["Amount"].Value, "N2"),
                    FormatExportValue(row.Cells["Count"].Value, "N0"),
                    FormatExportValue(row.Cells["FromDate"].Value, "dd MMM yyyy"),
                    FormatExportValue(row.Cells["ToDate"].Value, "dd MMM yyyy"),
                    Convert.ToString(row.Cells["User"].Value),
                    Convert.ToString(row.Cells["Counter"].Value));
            }
        }

        private void AppendDynamicMetricExportRow(StringBuilder builder, string metric)
        {
            decimal amount;
            int count;
            string category;
            GetDynamicMetricValue(metric, out amount, out count, out category);

            AppendCsvRow(
                builder,
                metric,
                amount.ToString("N2"),
                count.ToString("N0"),
                currentModel.FromDate.ToString("dd MMM yyyy"),
                currentModel.ToDate.ToString("dd MMM yyyy"),
                currentModel.UserName,
                currentModel.CounterName);
        }

        private void AppendPartialPaymentDetailsExport(StringBuilder builder)
        {
            AnalysisScopeSelection selection = GetSelectedScope();
            DataTable details;
            using (var repo = new FinalAnalysisRepository())
            {
                details = repo.GetPartialPaymentSaleDetails(
                    GetDateValue(dtpFrom),
                    GetDateValue(dtpTo),
                    selection.UserId,
                    selection.CounterId);
            }

            if (details == null || details.Rows.Count == 0)
            {
                return;
            }

            builder.AppendLine();
            AppendCsvRow(builder, "Partial Payment Details");

            bool firstHeader = true;
            foreach (DataColumn column in details.Columns)
            {
                if (!firstHeader) builder.Append(",");
                builder.Append(EscapeCsv(GetPartialPaymentExportHeader(column.ColumnName)));
                firstHeader = false;
            }
            builder.AppendLine();

            foreach (DataRow row in details.Rows)
            {
                bool firstValue = true;
                foreach (DataColumn column in details.Columns)
                {
                    if (!firstValue) builder.Append(",");
                    builder.Append(EscapeCsv(FormatExportValue(row[column], GetPartialPaymentExportFormat(column.ColumnName))));
                    firstValue = false;
                }
                builder.AppendLine();
            }
        }

        private string GetPartialPaymentExportHeader(string columnName)
        {
            switch (columnName)
            {
                case "BillNo": return "Bill No";
                case "BillDate": return "Bill Date";
                case "ItemName": return "Item Name";
                case "SellingPrice": return "Selling Price";
                case "LineTotal": return "Line Total";
                case "BillTotal": return "Bill Total";
                case "PartiallyPaid": return "Partially Paid";
                case "PaymentSplit": return "Payment Split";
                default: return columnName;
            }
        }

        private string GetPartialPaymentExportFormat(string columnName)
        {
            switch (columnName)
            {
                case "BillDate": return "dd MMM yyyy";
                case "Qty": return "N2";
                case "SellingPrice":
                case "LineTotal":
                case "BillTotal":
                case "PartiallyPaid":
                case "Balance":
                    return "N2";
                default:
                    return null;
            }
        }

        private void AppendCsvRow(StringBuilder builder, params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) builder.Append(",");
                builder.Append(EscapeCsv(values[i]));
            }
            builder.AppendLine();
        }

        private string FormatExportValue(object value, string format)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(format))
            {
                return Convert.ToString(value);
            }

            if (format.StartsWith("N", StringComparison.OrdinalIgnoreCase))
            {
                decimal number;
                return decimal.TryParse(Convert.ToString(value), out number) ? number.ToString(format) : Convert.ToString(value);
            }

            if (format.Contains("yyyy") || format.Contains("MMM") || format.Contains("dd"))
            {
                DateTime date;
                return DateTime.TryParse(Convert.ToString(value), out date) ? date.ToString(format) : Convert.ToString(value);
            }

            return Convert.ToString(value);
        }

        private string EscapeCsv(string value)
        {
            value = value ?? string.Empty;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private enum AnalysisScopeMode
        {
            All,
            Counter,
            User
        }

        private class AnalysisScopeSelection
        {
            public AnalysisScopeSelection(int userId, int counterId)
            {
                UserId = userId;
                CounterId = counterId;
            }

            public int UserId { get; }
            public int CounterId { get; }
        }
    }
}
