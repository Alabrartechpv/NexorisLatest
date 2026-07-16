using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Printing;


namespace PosBranch_Win.DialogBox
{
    public partial class frmClosingHistory : Form
    {
        private ClosingRepo _repo;
        private List<ClosingModel> closingHistoryListData;

        public delegate void SelectedClosingHandler(string docNo);
        public event SelectedClosingHandler OnClosingSelected;

        private Label lblStatus;
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private bool isOriginalOrder = true;
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
        private Point startPoint;
        private UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;
        private readonly bool showSensitiveCashDetails;

        public frmClosingHistory()
        {
            InitializeComponent();
            this.KeyPreview = true;

            _repo = new ClosingRepo();
            showSensitiveCashDetails = CanViewSensitiveCashDetails();

            InitializeStatusLabel();
            SetupUltraGridStyle();
            SetupUltraPanelStyle();
            InitializeSearchFilterComboBox();
            InitializeColumnOrderComboBox();
            SetupPanelHoverEffects();
            ConnectNavigationPanelEvents();
            SetupColumnChooserMenu();

            this.Load += frmClosingHistory_Load;
            this.ultraGridCountry.InitializeLayout += ultraGridCountry_InitializeLayout;
            this.ultraGridCountry.KeyDown += ultraGridCountry_KeyDown;
            this.ultraGridCountry.DoubleClickRow += ultraGridCountry_DoubleClickRow;
            this.textBoxsearch.KeyDown += textBoxsearch_KeyDown;
            this.ultraGridCountry.MouseDown += UltraGrid_MouseDown;
            this.ultraGridCountry.MouseMove += UltraGrid_MouseMove;
            this.ultraGridCountry.MouseUp += UltraGrid_MouseUp;
            this.ultraGridCountry.AfterSortChange += (s, e) => PreserveColumnWidths();
            this.ultraGridCountry.AfterColPosChanged += (s, e) => PreserveColumnWidths();
            this.SizeChanged += (s, e) => PreserveColumnWidths();
            this.ultraGridCountry.Resize += (s, e) => PreserveColumnWidths();
            this.ultraGridCountry.AfterRowActivate += ultraGridCountry_AfterRowActivate;
            this.FormClosing += (s, e) =>
            {
                if (columnChooserForm != null && !columnChooserForm.IsDisposed)
                {
                    columnChooserForm.Close();
                }
            };
        }

        private void frmClosingHistory_Load(object sender, EventArgs e)
        {
            if (!showSensitiveCashDetails)
            {
                Text = "Closing History";
            }

            // Configure ultraPanel4 as a Print button
            label4.Text = "Print";
            ultraPanel4.Visible = true;
            LoadClosingHistory();
            this.BeginInvoke(new Action(() =>
            {
                textBoxsearch.Focus();
                textBoxsearch.Select();
            }));
            UpdateRecordCountLabel();
            UpdateStatus("Ready");
        }

        private void LoadClosingHistory()
        {
            try
            {
                // Fetch data from repository
                closingHistoryListData = _repo.GetShiftHistory();

                ultraGridCountry.DataSource = closingHistoryListData;

                if (ultraGridCountry.Rows.Count > 0)
                {
                    ultraGridCountry.ActiveRow = ultraGridCountry.Rows[0];
                    ultraGridCountry.Selected.Rows.Clear();
                    ultraGridCountry.Selected.Rows.Add(ultraGridCountry.Rows[0]);
                }
                UpdateRecordCountLabel();
                InitializeSavedColumnWidths();
            }
            catch (Exception ex)
            {
                UpdateStatus("Error loading closing history: " + ex.Message);
                MessageBox.Show("Error loading closing history: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool CanViewSensitiveCashDetails()
        {
            return SessionContext.UserLevel?.Equals("Administrator", StringComparison.OrdinalIgnoreCase) == true;
        }

        private void InitializeSavedColumnWidths()
        {
            savedColumnWidths.Clear();
            if (ultraGridCountry.DisplayLayout.Bands.Count > 0)
            {
                foreach (UltraGridColumn column in ultraGridCountry.DisplayLayout.Bands[0].Columns)
                {
                    if (!column.Hidden)
                    {
                        savedColumnWidths[column.Key] = column.Width;
                    }
                }
            }
        }

        private void PreserveColumnWidths()
        {
            try
            {
                ultraGridCountry.SuspendLayout();
                if (ultraGridCountry.DisplayLayout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn col in ultraGridCountry.DisplayLayout.Bands[0].Columns)
                    {
                        if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                            col.Width = savedColumnWidths[col.Key];
                    }
                }
                ultraGridCountry.ResumeLayout();
            }
            catch (Exception ex) { UpdateStatus($"Error preserving column widths: {ex.Message}"); }
        }

        #region User Interaction
        private void LoadSelectedClosing()
        {
            if (ultraGridCountry.ActiveRow == null) return;
            try
            {
                string docNo = ultraGridCountry.ActiveRow.Cells["DocNo"]?.Value?.ToString() ?? "N/A";

                OnClosingSelected?.Invoke(docNo);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading selected closing: " + ex.Message);
            }
        }

        private void ultraGridCountry_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            LoadSelectedClosing();
        }

        private void ultraGridCountry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadSelectedClosing();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                e.Handled = true;
            }
        }

        private void textBoxsearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && ultraGridCountry.Rows.Count > 0)
            {
                ultraGridCountry.Focus();
                ultraGridCountry.ActiveRow = ultraGridCountry.Rows[0];
            }
            else if (e.KeyCode == Keys.Enter && ultraGridCountry.Rows.Count > 0)
            {
                if (ultraGridCountry.Rows.Count == 1) LoadSelectedClosing();
                else ultraGridCountry.Focus();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                e.Handled = true;
            }
        }
        #endregion

        #region UI and Styling (Status, Panels, Grid)
        private void InitializeStatusLabel()
        {
            lblStatus = new Label { Name = "lblStatus", AutoSize = false, Dock = DockStyle.Bottom, Height = 30, BackColor = Color.LightYellow, BorderStyle = BorderStyle.FixedSingle, TextAlign = ContentAlignment.MiddleLeft, Text = "Ready" };
            this.Controls.Add(lblStatus);
        }

        private void UpdateStatus(string message)
        {
            if (lblStatus != null) lblStatus.Text = message;
        }

        private void UpdateRecordCountLabel()
        {
            int currentDisplayCount = ultraGridCountry.Rows?.Count ?? 0;
            int totalCount = closingHistoryListData?.Count ?? 0;
            string recordText = $"Showing {currentDisplayCount} of {totalCount} records";

            if (this.Controls.Find("label1", true).FirstOrDefault() is Label label1)
            {
                label1.Text = recordText;
                label1.AutoSize = true;
            }
            if (this.Controls.Find("textBox3", true).FirstOrDefault() is TextBox textBox3)
            {
                textBox3.Text = totalCount.ToString();
            }
            UpdateStatus(recordText);
        }

        private void SetupUltraPanelStyle()
        {
            StyleIconPanel(ultraPanel3); // Up arrow
            StyleIconPanel(ultraPanel4); // New/Edit
            StyleIconPanel(ultraPanel5); // OK
            StyleIconPanel(ultraPanel6); // Close
            StyleIconPanel(ultraPanel7); // Down arrow
        }

        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            if (panel == null) return;
            Color lightBlue = Color.FromArgb(127, 219, 255), darkBlue = Color.FromArgb(0, 116, 217);
            panel.Appearance.BackColor = lightBlue;
            panel.Appearance.BackColor2 = darkBlue;
            panel.Appearance.BackGradientStyle = GradientStyle.Vertical;
            panel.BorderStyle = UIElementBorderStyle.Rounded4;
            panel.Appearance.BorderColor = Color.FromArgb(0, 150, 220);
        }

        private void SetupUltraGridStyle()
        {
            try
            {
                var o = ultraGridCountry.DisplayLayout.Override;
                o.AllowAddNew = AllowAddNew.No;
                o.AllowDelete = DefaultableBoolean.False;
                o.AllowUpdate = DefaultableBoolean.False;
                o.RowSelectors = DefaultableBoolean.True;
                o.SelectTypeRow = SelectType.Single;
                o.HeaderClickAction = HeaderClickAction.SortSingle;
                o.AllowColMoving = AllowColMoving.WithinBand;
                o.CellClickAction = CellClickAction.RowSelect;
                ultraGridCountry.DisplayLayout.GroupByBox.Hidden = true;
                ultraGridCountry.DisplayLayout.GroupByBox.Prompt = string.Empty;
                ultraGridCountry.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                o.BorderStyleRow = UIElementBorderStyle.Solid;
                o.BorderStyleCell = UIElementBorderStyle.Solid;
                o.BorderStyleHeader = UIElementBorderStyle.Solid;
                o.BorderStyleRowSelector = UIElementBorderStyle.Solid;
                o.RowAppearance.BorderAlpha = Alpha.Opaque;
                o.CellAppearance.BorderAlpha = Alpha.Opaque;
                o.CellPadding = 0;
                o.RowSpacingBefore = 0;
                o.RowSpacingAfter = 0;
                o.CellSpacing = 0;
                ultraGridCountry.DisplayLayout.InterBandSpacing = 0;
                Color lightBlue = Color.FromArgb(173, 216, 230);
                Color headerBlue = Color.FromArgb(0, 123, 255);
                o.CellAppearance.BorderColor = lightBlue;
                o.RowAppearance.BorderColor = lightBlue;
                o.HeaderAppearance.BorderColor = headerBlue;
                o.RowSelectorAppearance.BorderColor = headerBlue;
                o.MinRowHeight = 30;
                o.DefaultRowHeight = 30;
                o.HeaderStyle = HeaderStyle.WindowsXPCommand;
                o.HeaderAppearance.BackColor = headerBlue;
                o.HeaderAppearance.BackColor2 = headerBlue;
                o.HeaderAppearance.BackGradientStyle = GradientStyle.None;
                o.HeaderAppearance.ForeColor = Color.White;
                o.HeaderAppearance.TextHAlign = HAlign.Center;
                o.HeaderAppearance.TextVAlign = VAlign.Middle;
                o.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                o.HeaderAppearance.FontData.SizeInPoints = 9;
                o.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                o.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
                o.RowSelectorAppearance.BackColor = headerBlue;
                o.RowSelectorAppearance.BackColor2 = headerBlue;
                o.RowSelectorAppearance.BackGradientStyle = GradientStyle.None;
                o.RowSelectorAppearance.ForeColor = Color.White;
                o.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
                o.RowSelectorNumberStyle = RowSelectorNumberStyle.None;
                o.RowSelectorWidth = 15;
                o.ActiveRowAppearance.Image = null;
                o.SelectedRowAppearance.Image = null;
                o.RowSelectorAppearance.Image = null;
                o.RowAppearance.BackColor = Color.White;
                o.RowAppearance.BackColor2 = Color.White;
                o.RowAppearance.BackGradientStyle = GradientStyle.None;
                o.RowAlternateAppearance.BackColor = Color.White;
                o.RowAlternateAppearance.BackColor2 = Color.White;
                o.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;
                o.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                o.SelectedRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                o.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                o.SelectedRowAppearance.ForeColor = Color.White;
                o.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                o.ActiveRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                o.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                o.ActiveRowAppearance.ForeColor = Color.White;
                o.CellAppearance.FontData.SizeInPoints = 10;
                o.RowAppearance.FontData.SizeInPoints = 10;
                o.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                o.RowAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGridCountry.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                ultraGridCountry.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;
                if (ultraGridCountry.DisplayLayout.ScrollBarLook != null)
                {
                    ultraGridCountry.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    ultraGridCountry.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    ultraGridCountry.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
                    ultraGridCountry.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;
                    ultraGridCountry.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    ultraGridCountry.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    ultraGridCountry.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGridCountry.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;
                    ultraGridCountry.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    ultraGridCountry.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    ultraGridCountry.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGridCountry.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }
                o.CellAppearance.TextVAlign = VAlign.Middle;
                o.CellAppearance.TextHAlign = HAlign.Center;
                ultraGridCountry.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
                o.ColumnAutoSizeMode = ColumnAutoSizeMode.None;
            }
            catch (Exception ex) { UpdateStatus($"Grid style error: {ex.Message}"); }
        }

        private void ultraGridCountry_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                e.Layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;
                e.Layout.BorderStyle = UIElementBorderStyle.Solid;
                e.Layout.Override.CellPadding = 0;
                e.Layout.Override.RowSpacingBefore = 0;
                e.Layout.Override.RowSpacingAfter = 0;
                e.Layout.Override.CellSpacing = 0;
                e.Layout.InterBandSpacing = 0;
                e.Layout.Override.MinRowHeight = 30;
                e.Layout.Override.DefaultRowHeight = 30;
                Color lightBlue = Color.FromArgb(173, 216, 230);
                Color headerBlue = Color.FromArgb(0, 123, 255);
                e.Layout.Override.CellAppearance.BorderColor = lightBlue;
                e.Layout.Override.RowAppearance.BorderColor = lightBlue;
                e.Layout.Override.HeaderAppearance.BorderColor = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BorderColor = headerBlue;
                e.Layout.Override.CellAppearance.TextHAlign = HAlign.Center;
                e.Layout.Override.CellAppearance.TextVAlign = VAlign.Middle;
                e.Layout.Override.CellAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.RowAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                e.Layout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";
                e.Layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                e.Layout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                e.Layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";

                var band = e.Layout.Bands[0];

                // Configure visible columns for closing history based on SHIFTHISTORY SP result
                int position = 0;

                if (band.Columns.Exists("DocNo"))
                {
                    var col = band.Columns["DocNo"];
                    col.Hidden = false;
                    col.Header.VisiblePosition = position++;
                    col.Header.Caption = "Doc No";
                    col.CellAppearance.TextHAlign = HAlign.Left;
                    col.Width = 100;
                }
                if (band.Columns.Exists("TransactionDate"))
                {
                    var col = band.Columns["TransactionDate"];
                    col.Hidden = false;
                    col.Header.VisiblePosition = position++;
                    col.Header.Caption = "Closing Date";
                    col.CellAppearance.TextHAlign = HAlign.Center;
                    col.Format = "dd/MM/yyyy HH:mm";
                    col.Width = 140;
                }
                if (band.Columns.Exists("NetSales"))
                {
                    var col = band.Columns["NetSales"];
                    col.Hidden = !showSensitiveCashDetails;
                    if (showSensitiveCashDetails)
                        col.Header.VisiblePosition = position++;
                    col.Header.Caption = "Net Sales";
                    col.CellAppearance.TextHAlign = HAlign.Right;
                    col.Format = "#,##0.00";
                    col.Width = 100;
                }
                if (band.Columns.Exists("TotalCollection"))
                {
                    var col = band.Columns["TotalCollection"];
                    col.Hidden = !showSensitiveCashDetails;
                    if (showSensitiveCashDetails)
                        col.Header.VisiblePosition = position++;
                    col.Header.Caption = "Total Collection";
                    col.CellAppearance.TextHAlign = HAlign.Right;
                    col.Format = "#,##0.00";
                    col.Width = 110;
                }
                if (band.Columns.Exists("CashSale"))
                {
                    var col = band.Columns["CashSale"];
                    col.Hidden = !showSensitiveCashDetails;
                    if (showSensitiveCashDetails)
                        col.Header.VisiblePosition = position++;
                    col.Header.Caption = "Cash";
                    col.CellAppearance.TextHAlign = HAlign.Right;
                    col.Format = "#,##0.00";
                    col.Width = 90;
                }
                if (band.Columns.Exists("CardSale"))
                {
                    var col = band.Columns["CardSale"];
                    col.Hidden = !showSensitiveCashDetails;
                    if (showSensitiveCashDetails)
                        col.Header.VisiblePosition = position++;
                    col.Header.Caption = "Card";
                    col.CellAppearance.TextHAlign = HAlign.Right;
                    col.Format = "#,##0.00";
                    col.Width = 90;
                }
                if (band.Columns.Exists("UpiSale"))
                {
                    var col = band.Columns["UpiSale"];
                    col.Hidden = !showSensitiveCashDetails;
                    if (showSensitiveCashDetails)
                        col.Header.VisiblePosition = position++;
                    col.Header.Caption = "UPI";
                    col.CellAppearance.TextHAlign = HAlign.Right;
                    col.Format = "#,##0.00";
                    col.Width = 90;
                }
                if (band.Columns.Exists("PhysicalCashCounted"))
                {
                    var col = band.Columns["PhysicalCashCounted"];
                    col.Hidden = !showSensitiveCashDetails;
                    if (showSensitiveCashDetails)
                        col.Header.VisiblePosition = position++;
                    col.Header.Caption = "Physical Cash";
                    col.CellAppearance.TextHAlign = HAlign.Right;
                    col.Format = "#,##0.00";
                    col.Width = 100;
                }
                if (band.Columns.Exists("CashDifference"))
                {
                    var col = band.Columns["CashDifference"];
                    col.Hidden = !showSensitiveCashDetails;
                    if (showSensitiveCashDetails)
                        col.Header.VisiblePosition = position++;
                    col.Header.Caption = "Difference";
                    col.CellAppearance.TextHAlign = HAlign.Right;
                    col.Format = "#,##0.00";
                    col.Width = 90;
                }
                if (band.Columns.Exists("Status"))
                {
                    var col = band.Columns["Status"];
                    col.Hidden = false;
                    col.Header.VisiblePosition = position++;
                    col.Header.Caption = "Status";
                    col.CellAppearance.TextHAlign = HAlign.Center;
                    col.Width = 80;
                }

                // Hide other columns that are not explicitly shown
                string[] visibleColumns = showSensitiveCashDetails
                    ? new[] { "DocNo", "TransactionDate", "NetSales", "TotalCollection",
                        "CashSale", "CardSale", "UpiSale", "PhysicalCashCounted", "CashDifference", "Status" }
                    : new[] { "DocNo", "TransactionDate", "Status" };
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (!visibleColumns.Contains(col.Key))
                    {
                        col.Hidden = true;
                    }
                }
            }
            catch (Exception ex) { UpdateStatus($"Layout error: {ex.Message}"); }
        }
        #endregion

        #region Search, Filter, Sort, Reorder
        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new object[] { "All", "Doc No", "Counter" });
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += (s, e) => FilterData();
            textBoxsearch.TextChanged += (s, e) => FilterData();
        }

        private void FilterData()
        {
            if (closingHistoryListData == null) return;
            try
            {
                string searchText = textBoxsearch.Text.Trim().ToLower();
                IEnumerable<ClosingModel> filtered = closingHistoryListData;

                if (!string.IsNullOrEmpty(searchText))
                {
                    string selectedFilter = comboBox1.SelectedItem?.ToString() ?? "All";

                    switch (selectedFilter)
                    {
                        case "Doc No":
                            filtered = closingHistoryListData.Where(p => (p.DocNo ?? "").ToLower().Contains(searchText));
                            break;
                        case "Counter":
                            filtered = closingHistoryListData.Where(p => (p.Counter ?? "").ToLower().Contains(searchText));
                            break;
                        default: // All
                            filtered = closingHistoryListData.Where(p =>
                                (p.DocNo ?? "").ToLower().Contains(searchText) ||
                                (p.Counter ?? "").ToLower().Contains(searchText));
                            break;
                    }
                }

                ultraGridCountry.DataSource = filtered.ToList();
                if (ultraGridCountry.Rows.Count > 0) ultraGridCountry.ActiveRow = ultraGridCountry.Rows[0];
                UpdateRecordCountLabel();
            }
            catch (Exception ex) { UpdateStatus($"Filter error: {ex.Message}"); }
        }

        private void InitializeColumnOrderComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(new object[] { "Doc No", "Counter", "Date", "Amount" });
            comboBox2.SelectedIndex = 0;
            comboBox2.Enabled = true;
        }

        private void UltraPictureBox4_Click(object sender, EventArgs e)
        {
            if (closingHistoryListData == null) return;
            isOriginalOrder = !isOriginalOrder;

            if (isOriginalOrder)
            {
                ultraGridCountry.DataSource = closingHistoryListData.OrderByDescending(p => p.TransactionDate).ToList();
            }
            else
            {
                ultraGridCountry.DataSource = closingHistoryListData.OrderBy(p => p.TransactionDate).ToList();
            }
        }
        #endregion

        #region Column Chooser
        private void SetupColumnChooserMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Field/Column Chooser", null, (s, e) => ShowColumnChooser());
            ultraGridCountry.ContextMenuStrip = menu;
            SetupDirectHeaderDragDrop();
        }

        private void SetupDirectHeaderDragDrop()
        {
            ultraGridCountry.AllowDrop = true;
            ultraGridCountry.DragOver += UltraGrid_DragOver;
            ultraGridCountry.DragDrop += UltraGrid_DragDrop;
            CreateColumnChooserForm();
        }

        private void ShowColumnChooser()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed) CreateColumnChooserForm();
            columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization",
                Size = new Size(220, 280),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 240, 240),
                ShowIcon = false,
                ShowInTaskbar = false
            };
            columnChooserForm.FormClosing += (s, e) => { columnChooserListBox = null; };
            columnChooserForm.Shown += (s, e) => PositionColumnChooserAtBottomRight();

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 240, 240),
                ItemHeight = 30,
                IntegralHeight = false
            };
            columnChooserListBox.DrawItem += (s, evt) =>
            {
                if (evt.Index < 0) return;
                var item = columnChooserListBox.Items[evt.Index] as ColumnItem;
                if (item == null) return;
                Rectangle rect = evt.Bounds;
                rect.Inflate(-3, -3);
                using (var bgBrush = new SolidBrush(Color.FromArgb(33, 150, 243)))
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int radius = 4, diameter = radius * 2;
                    Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
                    path.AddArc(arcRect, 180, 90);
                    arcRect.X = rect.Right - diameter;
                    path.AddArc(arcRect, 270, 90);
                    arcRect.Y = rect.Bottom - diameter;
                    path.AddArc(arcRect, 0, 90);
                    arcRect.X = rect.Left;
                    path.AddArc(arcRect, 90, 90);
                    path.CloseFigure();
                    evt.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    evt.Graphics.FillPath(bgBrush, path);
                }
                using (var textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
                    evt.Graphics.DrawString(item.DisplayText, evt.Font, textBrush, rect, sf);
                }
            };
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;
            columnChooserForm.Controls.Add(columnChooserListBox);
            PopulateColumnChooserListBox();
        }

        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(this.Right - columnChooserForm.Width - 20, this.Bottom - columnChooserForm.Height - 20);
            }
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;
            columnChooserListBox.Items.Clear();
            string[] standardColumns = { "DocNo", "Counter", "TransactionDate", "TotalAmount" };
            var displayNames = new Dictionary<string, string>
            {
                { "DocNo", "Document No" },
                { "Counter", "Counter" },
                { "TransactionDate", "Date" },
                { "TotalAmount", "Total Amount" }
            };
            var addedColumns = new HashSet<string>();
            if (ultraGridCountry.DisplayLayout.Bands.Count > 0)
            {
                foreach (string colKey in standardColumns)
                {
                    if (ultraGridCountry.DisplayLayout.Bands[0].Columns.Exists(colKey) &&
                        ultraGridCountry.DisplayLayout.Bands[0].Columns[colKey].Hidden &&
                        !addedColumns.Contains(colKey))
                    {
                        string displayText = displayNames.ContainsKey(colKey) ? displayNames[colKey] : colKey;
                        columnChooserListBox.Items.Add(new ColumnItem(colKey, displayText));
                        addedColumns.Add(colKey);
                    }
                }
            }
        }

        private class ColumnItem
        {
            public string ColumnKey { get; set; }
            public string DisplayText { get; set; }
            public ColumnItem(string key, string text) { ColumnKey = key; DisplayText = text; }
            public override string ToString() => DisplayText;
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                if (columnChooserListBox.Items[index] is ColumnItem item)
                {
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }

        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(UltraGridColumn)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(UltraGridColumn)) is UltraGridColumn column && !column.Hidden)
            {
                string name = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                column.Hidden = true;
                columnChooserListBox.Items.Add(new ColumnItem(column.Key, name));
            }
        }

        private void UltraGrid_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(ColumnItem)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void UltraGrid_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnItem)) is ColumnItem item)
            {
                if (ultraGridCountry.DisplayLayout.Bands.Count > 0 && ultraGridCountry.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                {
                    var column = ultraGridCountry.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                    column.Hidden = false;
                    columnChooserListBox.Items.Remove(item);
                    toolTip.Show($"'{item.DisplayText}' restored", ultraGridCountry, ultraGridCountry.PointToClient(MousePosition), 1500);
                }
            }
        }

        private void HideColumn(UltraGridColumn column)
        {
            if (column != null && !column.Hidden)
            {
                savedColumnWidths[column.Key] = column.Width;
                column.Hidden = true;
                if (columnChooserForm == null || columnChooserForm.IsDisposed) CreateColumnChooserForm();
                if (columnChooserListBox != null && !columnChooserListBox.Items.Cast<ColumnItem>().Any(i => i.ColumnKey == column.Key))
                {
                    string name = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                    columnChooserListBox.Items.Add(new ColumnItem(column.Key, name));
                }
            }
        }
        #endregion

        #region Navigation and Hover
        private void ConnectNavigationPanelEvents()
        {
            ultraPanel3.Click += MoveRowUp; // Up
            ultraPictureBox5.Click += MoveRowUp;
            ultraPanel7.Click += MoveRowDown; // Down
            ultraPictureBox6.Click += MoveRowDown;
            ultraPanel5.Click += (s, e) => LoadSelectedClosing(); // OK
            ultraPictureBox1.Click += (s, e) => LoadSelectedClosing();
            label5.Click += (s, e) => LoadSelectedClosing();
            ultraPanel6.Click += (s, e) => this.Close(); // Close
            ultraPictureBox2.Click += (s, e) => this.Close();
            label3.Click += (s, e) => this.Close();

            ultraPanel4.Click += PrintSelectedClosingReport;
            ultraPictureBox3.Click += PrintSelectedClosingReport;
            label4.Click += PrintSelectedClosingReport;
        }

        private void MoveRowUp(object sender, EventArgs e)
        {
            if (ultraGridCountry.ActiveRow != null && ultraGridCountry.ActiveRow.Index > 0)
            {
                var newRow = ultraGridCountry.Rows[ultraGridCountry.ActiveRow.Index - 1];
                ultraGridCountry.ActiveRow = newRow;
                ultraGridCountry.ActiveRowScrollRegion.ScrollRowIntoView(newRow);
            }
        }

        private void MoveRowDown(object sender, EventArgs e)
        {
            if (ultraGridCountry.ActiveRow != null && ultraGridCountry.ActiveRow.Index < ultraGridCountry.Rows.Count - 1)
            {
                var newRow = ultraGridCountry.Rows[ultraGridCountry.ActiveRow.Index + 1];
                ultraGridCountry.ActiveRow = newRow;
                ultraGridCountry.ActiveRowScrollRegion.ScrollRowIntoView(newRow);
            }
        }

        private void SetupPanelHoverEffects()
        {
            SetupPanelGroupHoverEffects(ultraPanel4, label4, ultraPictureBox3);
            SetupPanelGroupHoverEffects(ultraPanel6, label3, ultraPictureBox2);
            SetupPanelGroupHoverEffects(ultraPanel5, label5, ultraPictureBox1);
            SetupPanelGroupHoverEffects(ultraPanel3, null, ultraPictureBox5);
            SetupPanelGroupHoverEffects(ultraPanel7, null, ultraPictureBox6);
        }

        private void SetupPanelGroupHoverEffects(
            Infragistics.Win.Misc.UltraPanel panel,
            Label label,
            Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox)
        {
            if (panel == null) return;
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;
            Color hoverBackColor = BrightenColor(originalBackColor, 30);
            Color hoverBackColor2 = BrightenColor(originalBackColor2, 30);

            Action applyHoverEffect = () =>
            {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
                panel.ClientArea.Cursor = Cursors.Hand;
            };
            Action removeHoverEffect = () =>
            {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
                panel.ClientArea.Cursor = Cursors.Default;
            };

            panel.MouseEnter += (s, e) => applyHoverEffect();
            panel.MouseLeave += (s, e) => removeHoverEffect();

            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (s, e) => { applyHoverEffect(); pictureBox.Cursor = Cursors.Hand; };
                pictureBox.MouseLeave += (s, e) => { if (!IsMouseOverControl(panel)) removeHoverEffect(); };
            }
            if (label != null)
            {
                label.MouseEnter += (s, e) => { applyHoverEffect(); label.Cursor = Cursors.Hand; };
                label.MouseLeave += (s, e) => { if (!IsMouseOverControl(panel)) removeHoverEffect(); };
            }
        }

        private Color BrightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(color.R + amount, 255),
                Math.Min(color.G + amount, 255),
                Math.Min(color.B + amount, 255));
        }

        private bool IsMouseOverControl(Control control)
        {
            if (control == null || !control.IsHandleCreated) return false;
            return control.ClientRectangle.Contains(control.PointToClient(Control.MousePosition));
        }
        #endregion

        #region Column Chooser Drag/Drop Logic
        private void UltraGrid_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingColumn = false;
            columnToMove = null;
            startPoint = new Point(e.X, e.Y);
            if (e.Y < 40)
            {
                int xPos = 0;
                if (ultraGridCountry.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True)
                    xPos += ultraGridCountry.DisplayLayout.Override.RowSelectorWidth;
                foreach (UltraGridColumn col in ultraGridCountry.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        if (e.X >= xPos && e.X < xPos + col.Width)
                        {
                            columnToMove = col;
                            isDraggingColumn = true;
                            break;
                        }
                        xPos += col.Width;
                    }
                }
            }
        }

        private void UltraGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnToMove != null && isDraggingColumn)
            {
                int deltaX = Math.Abs(e.X - startPoint.X);
                int deltaY = Math.Abs(e.Y - startPoint.Y);
                if (deltaX > SystemInformation.DragSize.Width || deltaY > SystemInformation.DragSize.Height)
                {
                    bool isDraggingDown = (e.Y > startPoint.Y && deltaY > deltaX);
                    if (isDraggingDown)
                    {
                        ultraGridCountry.Cursor = Cursors.No;
                        string columnName = !string.IsNullOrEmpty(columnToMove.Header.Caption) ? columnToMove.Header.Caption : columnToMove.Key;
                        toolTip.SetToolTip(ultraGridCountry, $"Drag down to hide '{columnName}' column");
                        if (e.Y - startPoint.Y > 50)
                        {
                            HideColumn(columnToMove);
                            columnToMove = null;
                            isDraggingColumn = false;
                            ultraGridCountry.Cursor = Cursors.Default;
                            toolTip.SetToolTip(ultraGridCountry, "");
                        }
                    }
                }
            }
        }

        private void UltraGrid_MouseUp(object sender, MouseEventArgs e)
        {
            ultraGridCountry.Cursor = Cursors.Default;
            toolTip.SetToolTip(ultraGridCountry, "");
            isDraggingColumn = false;
            columnToMove = null;
        }
        #endregion

        private void ultraGridCountry_AfterRowActivate(object sender, EventArgs e)
        {
            if (ultraGridCountry.ActiveRow != null)
            {
                ultraGridCountry.Selected.Rows.Clear();
                ultraGridCountry.Selected.Rows.Add(ultraGridCountry.ActiveRow);
            }
        }

        private void PrintSelectedClosingReport(object sender, EventArgs e)
        {
            if (ultraGridCountry.ActiveRow == null)
            {
                MessageBox.Show("Please select a closing record first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                var selectedModel = ultraGridCountry.ActiveRow.ListObject as ClosingModel;
                if (selectedModel == null)
                {
                    MessageBox.Show("Selected item is not a valid closing record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                this.Cursor = Cursors.WaitCursor;

                // Load denominations
                var denominations = _repo.GetClosingDenominations(selectedModel.ShiftClosingId);
                selectedModel.CashDetails = denominations;

                // Trigger print
                PrintDocument printDocument = new PrintDocument();
                printDocument.DocumentName = $"Closing Report - {selectedModel.DocNo}";

                printDocument.PrintPage += (s, ev) =>
                {
                    try
                    {
                        Font titleFont = new Font("Arial", 14, FontStyle.Bold);
                        Font subtitleFont = new Font("Arial", 11, FontStyle.Bold);
                        Font headerFont = new Font("Arial", 9.5f, FontStyle.Bold);
                        Font dataFont = new Font("Arial", 9);
                        Font boldFont = new Font("Arial", 9, FontStyle.Bold);
                        Font totalFont = new Font("Arial", 10.5f, FontStyle.Bold);

                        float y = 40;
                        float margin = 40;
                        float width = ev.PageBounds.Width - 80;

                        // 1. Header
                        string title = "SHIFT CLOSING AUDIT REPORT";
                        SizeF titleSize = ev.Graphics.MeasureString(title, titleFont);
                        ev.Graphics.DrawString(title, titleFont, Brushes.Black, (width - titleSize.Width) / 2 + margin, y);
                        y += 30;

                        ev.Graphics.DrawLine(Pens.Black, margin, y, width + margin, y);
                        y += 10;

                        // 2. Metadata (Two columns)
                        float colWidth = width / 2;
                        ev.Graphics.DrawString($"Doc No: {selectedModel.DocNo}", boldFont, Brushes.Black, margin, y);
                        ev.Graphics.DrawString($"Counter: {selectedModel.Counter}", dataFont, Brushes.Black, margin + colWidth, y);
                        y += 18;

                        ev.Graphics.DrawString($"Closing Date: {selectedModel.TransactionDate:dd-MMM-yyyy HH:mm}", dataFont, Brushes.Black, margin, y);
                        ev.Graphics.DrawString($"Status: {selectedModel.Status}", dataFont, Brushes.Black, margin + colWidth, y);
                        y += 18;

                        ev.Graphics.DrawString($"Report Selection: {selectedModel.ReportSelection}", dataFont, Brushes.Black, margin, y);
                        y += 15;

                        ev.Graphics.DrawLine(Pens.Gray, margin, y, width + margin, y);
                        y += 15;

                        // 3. Financial Summary
                        ev.Graphics.DrawString("FINANCIAL SUMMARY", subtitleFont, Brushes.Black, margin, y);
                        y += 20;

                        float labelX = margin + 10;
                        float valX = margin + 260;

                        ev.Graphics.DrawString("Total Gross Sales:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.TotalGrossSales:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Total Discount Given:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.TotalDiscount:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Total Returns:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.TotalReturn:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Net Sales:", boldFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.NetSales:N2}", boldFont, Brushes.Black, valX, y);
                        y += 22;

                        ev.Graphics.DrawLine(Pens.LightGray, margin, y, width + margin, y);
                        y += 10;

                        // 4. Collection Breakdown
                        ev.Graphics.DrawString("COLLECTIONS BREAKDOWN", subtitleFont, Brushes.Black, margin, y);
                        y += 20;

                        ev.Graphics.DrawString("Cash Sales Collection:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.CashSale:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Card Sales Collection:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.CardSale:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("UPI Sales Collection:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.UpiSale:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Credit Sales (Unpaid):", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.CreditSale:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Customer Receipts (Cash/Bank):", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.CustomerReceipt:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Total Collected Amount:", boldFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.TotalCollection:N2}", boldFont, Brushes.Black, valX, y);
                        y += 22;

                        ev.Graphics.DrawLine(Pens.LightGray, margin, y, width + margin, y);
                        y += 10;

                        // 5. Cash Drawer Reconciliation
                        ev.Graphics.DrawString("CASH RECONCILIATION & VARIANCE", subtitleFont, Brushes.Black, margin, y);
                        y += 20;

                        ev.Graphics.DrawString("System Expected Cash:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.SystemExpectedCash:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Physical Cash Counted:", boldFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.PhysicalCashCounted:N2}", boldFont, Brushes.Black, valX, y);
                        y += 18;

                        string diffText = selectedModel.CashDifference >= 0 ? "Cash Excess (Surplus):" : "Cash Shortage (Deficit):";
                        Color diffColor = selectedModel.CashDifference >= 0 ? Color.Green : Color.Red;
                        using (Brush diffBrush = new SolidBrush(diffColor))
                        {
                            ev.Graphics.DrawString(diffText, boldFont, diffBrush, labelX, y);
                            ev.Graphics.DrawString($"₹ {Math.Abs(selectedModel.CashDifference):N2}", boldFont, diffBrush, valX, y);
                        }
                        y += 18;

                        if (!string.IsNullOrWhiteSpace(selectedModel.DifferenceReason))
                        {
                            ev.Graphics.DrawString($"Reason: {selectedModel.DifferenceReason}", dataFont, Brushes.Black, labelX, y);
                            y += 18;
                        }
                        y += 22;

                        ev.Graphics.DrawLine(Pens.Black, margin, y, width + margin, y);
                        y += 15;

                        // 6. Denominations Table
                        if (selectedModel.CashDetails != null && selectedModel.CashDetails.Any())
                        {
                            ev.Graphics.DrawString("CASH DENOMINATIONS DETAILS", subtitleFont, Brushes.Black, margin, y);
                            y += 25;

                            float dCol1 = margin;
                            float dCol2 = margin + 120;
                            float dCol3 = margin + 220;
                            float dCol4 = margin + 320;

                            ev.Graphics.DrawString("#", headerFont, Brushes.Black, dCol1, y);
                            ev.Graphics.DrawString("Denomination", headerFont, Brushes.Black, dCol2, y);
                            ev.Graphics.DrawString("Quantity", headerFont, Brushes.Black, dCol3, y);
                            ev.Graphics.DrawString("Amount", headerFont, Brushes.Black, dCol4, y);
                            y += 20;

                            ev.Graphics.DrawLine(Pens.Gray, margin, y, width + margin, y);
                            y += 10;

                            int rowNum = 1;
                            foreach (var detail in selectedModel.CashDetails)
                            {
                                ev.Graphics.DrawString(rowNum.ToString(), dataFont, Brushes.Black, dCol1, y);
                                ev.Graphics.DrawString($"₹{detail.Denomination:N2}", dataFont, Brushes.Black, dCol2, y);
                                ev.Graphics.DrawString(detail.Quantity.ToString(), dataFont, Brushes.Black, dCol3, y);
                                ev.Graphics.DrawString($"₹{detail.Amount:N2}", dataFont, Brushes.Black, dCol4, y);
                                y += 18;
                                rowNum++;
                            }

                            y += 10;
                            ev.Graphics.DrawLine(Pens.Black, margin, y, width + margin, y);
                            y += 15;
                        }

                        // Footer
                        ev.Graphics.DrawString($"Printed on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss} by Auditor", dataFont, Brushes.Gray, margin, y);
                    }
                    catch (Exception ex)
                    {
                        ev.Graphics.DrawString($"Error drawing report page: {ex.Message}", new Font("Arial", 9), Brushes.Red, 50, 50);
                    }
                };

                PrintDialog printDialog = new PrintDialog();
                printDialog.Document = printDocument;

                if (printDialog.ShowDialog(this) == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing report: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
}
