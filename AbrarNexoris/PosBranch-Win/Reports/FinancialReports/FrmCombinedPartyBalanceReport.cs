using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Accounts;
using ModelClass.Report;
using Repository.Accounts;
using Repository;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public class FrmCombinedPartyBalanceReport : Form
    {
        private const string GRID_LAYOUT_FILE = "CombinedPartyBalanceGridLayout.xml";

        private readonly CombinedPartyBalanceReportRepository _repository;
        private readonly ManualPartyBalanceRepository _manualRepository;
        private readonly CustomerOutstandingReportRepository _customerRepository;
        private readonly VendorOutstandingReportRepository _vendorRepository;
        private readonly Dictionary<string, string> _customerNameMap;
        private readonly Dictionary<string, string> _vendorNameMap;
        private readonly List<string> _customerMasterNames;
        private readonly List<string> _vendorMasterNames;

        private Panel _filterPanel;
        private Panel _summaryPanel;
        private UltraGrid _gridReport;
        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private ComboBox _cmbPartyType;
        private TextBox _txtSearch;
        private CheckBox _chkOpenOnly;
        private CheckBox _chkMatchedOnly;
        private Button _btnSearch;
        private Button _btnClear;
        private Button _btnExport;
        private Button _btnClose;
        private Label _lblCount;
        private Label _lblManual;
        private Label _lblOutstanding;
        private Label _lblTotal;
        private bool _isLoading;
        private bool _gridLayoutLoaded;
        private Form _columnChooserForm;
        private ListBox _columnChooserListBox;
        private readonly Dictionary<string, int> _savedColumnWidths = new Dictionary<string, int>();
        private Point _dragStartPoint;
        private UltraGridColumn _columnToMove;
        private bool _isDraggingColumn;
        private readonly System.Windows.Forms.ToolTip _toolTip = new System.Windows.Forms.ToolTip();
        private string GridLayoutPath => Path.Combine(Application.StartupPath, GRID_LAYOUT_FILE);

        public FrmCombinedPartyBalanceReport()
        {
            _repository = new CombinedPartyBalanceReportRepository();
            _manualRepository = new ManualPartyBalanceRepository();
            _customerRepository = new CustomerOutstandingReportRepository();
            _vendorRepository = new VendorOutstandingReportRepository();
            _customerNameMap = BuildNameMap(_customerRepository.GetCustomers().Select(x => x.LedgerName));
            _vendorNameMap = BuildNameMap(_vendorRepository.GetVendors().Select(x => x.LedgerName));
            _customerMasterNames = _customerNameMap.Values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            _vendorMasterNames = _vendorNameMap.Values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            InitializeComponent();
            Load += FrmCombinedPartyBalanceReport_Load;
            FormClosing += FrmCombinedPartyBalanceReport_FormClosing;
            LocationChanged += (s, e) => PositionColumnChooserAtBottomRight();
            SizeChanged += (s, e) => PositionColumnChooserAtBottomRight();
            Activated += (s, e) => PositionColumnChooserAtBottomRight();
        }

        private void FrmCombinedPartyBalanceReport_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            DateTime today = DateTime.Today;
            _dtFrom.Value = new DateTime(today.Year, today.Month, 1);
            _dtTo.Value = today;
            _cmbPartyType.SelectedIndex = 0;
            _chkOpenOnly.Checked = true;
            _chkMatchedOnly.Checked = false;
            _isLoading = false;
            LoadReport();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = "Combined Party Balance Report";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(243, 244, 246);
            Font = new Font("Segoe UI", 9F);
            KeyPreview = true;
            KeyDown += Form_KeyDown;

            _filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = Color.FromArgb(236, 240, 245),
                Padding = new Padding(12)
            };

            _summaryPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 86,
                BackColor = Color.FromArgb(241, 248, 252),
                Padding = new Padding(14, 8, 14, 8)
            };

            _gridReport = new UltraGrid { Dock = DockStyle.Fill };
            _gridReport.InitializeLayout += GridReport_InitializeLayout;
            _gridReport.InitializeRow += GridReport_InitializeRow;
            _gridReport.DoubleClickRow += GridReport_DoubleClickRow;

            _dtFrom = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = 90 };
            _dtTo = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = 90 };
            _cmbPartyType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            _cmbPartyType.Items.AddRange(new object[] { "All", "Customer", "Vendor" });
            _txtSearch = new TextBox { Width = 180 };
            _chkOpenOnly = new CheckBox { Text = "Open Only", AutoSize = true, Margin = new Padding(0, 22, 12, 0) };
            _chkMatchedOnly = new CheckBox { Text = "Matched Only", AutoSize = true, Margin = new Padding(0, 22, 12, 0) };

            _btnSearch = CreateButton("Search", Color.FromArgb(25, 118, 210));
            _btnClear = CreateButton("Clear", Color.FromArgb(245, 124, 0));
            _btnExport = CreateButton("Export", Color.FromArgb(0, 121, 107));
            _btnClose = CreateButton("Close", Color.FromArgb(96, 125, 139));

            _btnSearch.Click += (s, e) => LoadReport();
            _btnClear.Click += (s, e) => ClearFilters();
            _btnExport.Click += (s, e) => ExportCsv();
            _btnClose.Click += (s, e) => Close();
            _cmbPartyType.SelectedIndexChanged += FilterValueChanged;
            _chkOpenOnly.CheckedChanged += FilterValueChanged;
            _chkMatchedOnly.CheckedChanged += FilterValueChanged;
            _txtSearch.KeyDown += TxtSearch_KeyDown;
            _dtFrom.ValueChanged += FilterValueChanged;
            _dtTo.ValueChanged += FilterValueChanged;

            FlowLayoutPanel filterLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                AutoScroll = true
            };

            filterLayout.Controls.Add(CreateLabeledHost("From", _dtFrom, 90));
            filterLayout.Controls.Add(CreateLabeledHost("To", _dtTo, 90));
            filterLayout.Controls.Add(CreateLabeledHost("Party Type", _cmbPartyType, 120));
            filterLayout.Controls.Add(CreateLabeledHost("Search", _txtSearch, 180));
            filterLayout.Controls.Add(_chkOpenOnly);
            filterLayout.Controls.Add(_chkMatchedOnly);
            filterLayout.Controls.Add(_btnSearch);
            filterLayout.Controls.Add(_btnClear);
            filterLayout.Controls.Add(_btnExport);
            filterLayout.Controls.Add(_btnClose);

            _filterPanel.Controls.Add(filterLayout);

            FlowLayoutPanel summaryLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true
            };

            summaryLayout.Controls.Add(CreateSummaryCard("Parties", Color.FromArgb(25, 118, 210), out _lblCount));
            summaryLayout.Controls.Add(CreateSummaryCard("Old Balance", Color.FromArgb(13, 71, 161), out _lblManual));
            summaryLayout.Controls.Add(CreateSummaryCard("Current Outstanding", Color.FromArgb(191, 54, 12), out _lblOutstanding));
            summaryLayout.Controls.Add(CreateSummaryCard("Net Total", Color.FromArgb(106, 27, 154), out _lblTotal));

            _summaryPanel.Controls.Add(summaryLayout);

            Controls.Add(_gridReport);
            Controls.Add(_summaryPanel);
            Controls.Add(_filterPanel);

            ConfigureGrid();
            SetupColumnChooserMenu();
            ResumeLayout(false);
        }

        private void ConfigureGrid()
        {
            _gridReport.UseAppStyling = false;
            _gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = _gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = true;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
            layout.Override.RowSelectorWidth = 40;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.AllowColMoving = AllowColMoving.WithinBand;
            layout.Override.AllowColSizing = AllowColSizing.Free;

            layout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(69, 90, 100);
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.Override.HeaderAppearance.BackColor = Color.FromArgb(55, 71, 79);
            layout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(69, 90, 100);
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 250, 252);
            layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
            layout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(33, 33, 33);
            layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(66, 165, 245);
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.MinRowHeight = 25;
            layout.Override.DefaultRowHeight = 25;
        }

        private void GridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
                return;

            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
            }

            ConfigureGridColumn(band, "PartyType", "Party Type", 90, null, HAlign.Left);
            ConfigureGridColumn(band, "PartyName", "Party Name", 220, null, HAlign.Left);
            ConfigureGridColumn(band, "ManualBalance", "Old Balance", 120, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "OutstandingBalance", "Current Outstanding", 145, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "TotalBalance", "Net Total", 120, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "ManualEntryCount", "Old Rows", 85, null, HAlign.Center);
            ConfigureGridColumn(band, "OutstandingEntryCount", "Current Rows", 95, null, HAlign.Center);
            ConfigureGridColumn(band, "ManualLastDate", "Last Old", 95, "dd-MMM-yyyy", HAlign.Left);
            ConfigureGridColumn(band, "OutstandingLastDate", "Last Current", 105, "dd-MMM-yyyy", HAlign.Left);
            ConfigureGridColumn(band, "MatchStatus", "Status", 110, null, HAlign.Center);

            if (band.Columns.Exists("PartyName"))
            {
                band.Columns["PartyName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("ManualBalance"))
            {
                band.Columns["ManualBalance"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
            }

            if (band.Columns.Exists("OutstandingBalance"))
            {
                band.Columns["OutstandingBalance"].CellAppearance.ForeColor = Color.FromArgb(191, 54, 12);
            }

            if (band.Columns.Exists("TotalBalance"))
            {
                band.Columns["TotalBalance"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                band.Columns["TotalBalance"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void GridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (!e.Row.Cells.Exists("MatchStatus"))
                return;

            string status = Convert.ToString(e.Row.Cells["MatchStatus"].Value);
            e.Row.Cells["MatchStatus"].Appearance.FontData.Bold = DefaultableBoolean.True;

            if (string.Equals(status, "Matched", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.Cells["MatchStatus"].Appearance.ForeColor = Color.FromArgb(21, 128, 61);
            }
            else if (string.Equals(status, "Manual Only", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.Cells["MatchStatus"].Appearance.ForeColor = Color.FromArgb(25, 118, 210);
            }
            else
            {
                e.Row.Cells["MatchStatus"].Appearance.ForeColor = Color.FromArgb(191, 54, 12);
            }
        }

        private void LoadReport()
        {
            if (_dtFrom.Value.Date > _dtTo.Value.Date)
            {
                MessageBox.Show("From date cannot be greater than to date.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<CombinedPartyBalanceReportRow> rows = _repository.GetReport(new CombinedPartyBalanceReportFilter
            {
                FromDate = _dtFrom.Value.Date,
                ToDate = _dtTo.Value.Date,
                PartyType = Convert.ToString(_cmbPartyType.SelectedItem),
                SearchText = _txtSearch.Text,
                OpenOnly = _chkOpenOnly.Checked,
                MatchedOnly = _chkMatchedOnly.Checked
            });

            _gridReport.DataSource = rows;
            ApplySavedLayoutIfAvailable();
            PopulateColumnChooserListBox();
            _lblCount.Text = rows.Count.ToString("N0");
            _lblManual.Text = $"Rs. {rows.Sum(x => x.ManualBalance):N2}";
            _lblOutstanding.Text = $"Rs. {rows.Sum(x => x.OutstandingBalance):N2}";
            _lblTotal.Text = $"Rs. {rows.Sum(x => x.TotalBalance):N2}";
        }

        private void ClearFilters()
        {
            _isLoading = true;
            DateTime today = DateTime.Today;
            _dtFrom.Value = new DateTime(today.Year, today.Month, 1);
            _dtTo.Value = today;
            _cmbPartyType.SelectedIndex = 0;
            _txtSearch.Clear();
            _chkOpenOnly.Checked = true;
            _chkMatchedOnly.Checked = false;
            _isLoading = false;
            LoadReport();
        }

        private void ExportCsv()
        {
            List<CombinedPartyBalanceReportRow> rows = _gridReport.DataSource as List<CombinedPartyBalanceReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = $"CombinedPartyBalance_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Party Type,Party Name,Old Balance,Current Outstanding,Net Total,Old Rows,Current Rows,Last Old,Last Current,Status");

                foreach (CombinedPartyBalanceReportRow row in rows)
                {
                    builder.AppendLine(string.Join(",",
                        EscapeCsv(row.PartyType),
                        EscapeCsv(row.PartyName),
                        row.ManualBalance.ToString("F2"),
                        row.OutstandingBalance.ToString("F2"),
                        row.TotalBalance.ToString("F2"),
                        row.ManualEntryCount.ToString(),
                        row.OutstandingEntryCount.ToString(),
                        EscapeCsv(row.ManualLastDate.HasValue ? row.ManualLastDate.Value.ToString("yyyy-MM-dd") : string.Empty),
                        EscapeCsv(row.OutstandingLastDate.HasValue ? row.OutstandingLastDate.Value.ToString("yyyy-MM-dd") : string.Empty),
                        EscapeCsv(row.MatchStatus)));
                }

                File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
                MessageBox.Show("Report exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                LoadReport();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                ExportCsv();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F6)
            {
                ClearFilters();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                OpenSelectedDetails();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private void FilterValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
            {
                return;
            }

            LoadReport();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadReport();
                e.Handled = true;
            }
        }

        private CombinedPartyBalanceReportRow GetSelectedSummaryRow()
        {
            if (_gridReport.ActiveRow == null)
            {
                return null;
            }

            return _gridReport.ActiveRow.ListObject as CombinedPartyBalanceReportRow;
        }

        private void OpenSelectedDetails()
        {
            CombinedPartyBalanceReportRow selectedRow = GetSelectedSummaryRow();
            if (selectedRow == null)
            {
                return;
            }

            List<PartyBalanceDetailRowDto> details = BuildDetailRows(selectedRow);
            string caption = $"Details: {selectedRow.PartyType} - {selectedRow.PartyName} | Old: Rs. {selectedRow.ManualBalance:N2} | Current: Rs. {selectedRow.OutstandingBalance:N2} | Net: Rs. {selectedRow.TotalBalance:N2}";

            using (FrmCombinedPartyBalanceDetailDialog dialog = new FrmCombinedPartyBalanceDetailDialog(caption, details))
            {
                dialog.ShowDialog(this);
            }
        }

        private List<PartyBalanceDetailRowDto> BuildDetailRows(CombinedPartyBalanceReportRow selectedRow)
        {
            List<PartyBalanceDetailRowDto> details = new List<PartyBalanceDetailRowDto>();
            DateTime fromDate = _dtFrom.Value.Date;
            DateTime toDate = _dtTo.Value.Date;

            List<ManualPartyBalanceEntry> manualEntries = _manualRepository.GetEntries(
                selectedRow.PartyType,
                null,
                null,
                _chkOpenOnly.Checked,
                fromDate,
                toDate);

            details.AddRange(manualEntries
                .Where(x => string.Equals(ResolvePartyName(selectedRow.PartyType, x.PartyName, true), selectedRow.PartyName, StringComparison.OrdinalIgnoreCase))
                .Select(x => new PartyBalanceDetailRowDto
                {
                    Source = "Old",
                    Reference = "Manual #" + x.Id,
                    EntryDate = x.EntryDate,
                    DueDate = null,
                    Amount = x.Amount,
                    Adjusted = x.SettledAmount,
                    Balance = x.RemainingAmount,
                    Remarks = x.Remarks,
                    Status = x.Status
                }));

            if (string.Equals(selectedRow.PartyType, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                List<CustomerOutstandingReportRow> customerRows = _customerRepository.GetReport(new CustomerOutstandingReportFilter
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    CompanyId = 0,
                    BranchId = 0,
                    FinYearId = 0,
                    LedgerId = 0
                });

                details.AddRange(customerRows
                    .Where(x => (!_chkOpenOnly.Checked || x.Balance > 0) &&
                                string.Equals(ResolvePartyName("Customer", x.LedgerName, false), selectedRow.PartyName, StringComparison.OrdinalIgnoreCase))
                    .Select(x => new PartyBalanceDetailRowDto
                    {
                        Source = "Current",
                        Reference = x.BillNo.ToString(),
                        EntryDate = x.BillDate,
                        DueDate = x.DueDate,
                        Amount = x.InvoiceAmount,
                        Adjusted = x.ReceivedAmount,
                        Balance = x.Balance,
                        Remarks = string.Empty,
                        Status = x.Balance > 0 ? "Outstanding" : "Closed"
                    }));
            }
            else if (string.Equals(selectedRow.PartyType, "Vendor", StringComparison.OrdinalIgnoreCase))
            {
                List<VendorOutstandingReportRow> vendorRows = _vendorRepository.GetReport(new VendorOutstandingReportFilter
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    CompanyId = 0,
                    BranchId = 0,
                    FinYearId = 0,
                    LedgerId = 0
                });

                details.AddRange(vendorRows
                    .Where(x => (!_chkOpenOnly.Checked || x.Balance > 0) &&
                                string.Equals(ResolvePartyName("Vendor", x.LedgerName, false), selectedRow.PartyName, StringComparison.OrdinalIgnoreCase))
                    .Select(x => new PartyBalanceDetailRowDto
                    {
                        Source = "Current",
                        Reference = "Vendor Outstanding",
                        EntryDate = x.VoucherDate,
                        DueDate = null,
                        Amount = x.TotalOutstanding,
                        Adjusted = x.TotalPaid,
                        Balance = x.Balance,
                        Remarks = string.Empty,
                        Status = x.Balance > 0 ? "Outstanding" : "Closed"
                    }));
            }

            List<PartyBalanceDetailRowDto> orderedDetails = details
                .OrderBy(x => x.Source == "Old" ? 0 : 1)
                .ThenByDescending(x => x.EntryDate ?? DateTime.MinValue)
                .ToList();

            return orderedDetails;
        }

        private void GridReport_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            OpenSelectedDetails();
        }

        private void SetupColumnChooserMenu()
        {
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += (s, e) => ShowColumnChooser();
            gridContextMenu.Items.Add(columnChooserMenuItem);
            _gridReport.ContextMenuStrip = gridContextMenu;
            SetupDirectHeaderDragDrop();
        }

        private void SetupDirectHeaderDragDrop()
        {
            _gridReport.AllowDrop = true;
            _gridReport.MouseDown += GridReport_MouseDown;
            _gridReport.MouseMove += GridReport_MouseMove;
            _gridReport.MouseUp += GridReport_MouseUp;
            _gridReport.DragOver += GridReport_DragOver;
            _gridReport.DragDrop += GridReport_DragDrop;
            CreateColumnChooserForm();
        }

        private void CreateColumnChooserForm()
        {
            _columnChooserForm = new Form
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
            _columnChooserForm.FormClosing += ColumnChooserForm_FormClosing;
            _columnChooserForm.Shown += (s, e) => PositionColumnChooserAtBottomRight();

            _columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 240, 240),
                ItemHeight = 30,
                IntegralHeight = false
            };

            _columnChooserListBox.DrawItem += (s, evt) =>
            {
                if (evt.Index < 0)
                {
                    return;
                }

                ColumnItem item = _columnChooserListBox.Items[evt.Index] as ColumnItem;
                if (item == null)
                {
                    return;
                }

                Rectangle rect = evt.Bounds;
                rect.Inflate(-3, -3);
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(33, 150, 243)))
                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int radius = 4;
                    int diameter = radius * 2;
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

                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat stringFormat = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Center
                    };
                    evt.Graphics.DrawString(item.DisplayText, evt.Font, textBrush, rect, stringFormat);
                }
            };

            _columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            _columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            _columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;
            _columnChooserForm.Controls.Add(_columnChooserListBox);
            PopulateColumnChooserListBox();
        }

        private void PositionColumnChooserAtBottomRight()
        {
            if (_columnChooserForm != null && !_columnChooserForm.IsDisposed && _columnChooserForm.Visible)
            {
                _columnChooserForm.Location = new Point(
                    Right - _columnChooserForm.Width - 20,
                    Bottom - _columnChooserForm.Height - 20);
                _columnChooserForm.BringToFront();
            }
        }

        private void ShowColumnChooser()
        {
            PopulateColumnChooserListBox();

            if (_columnChooserForm != null && !_columnChooserForm.IsDisposed)
            {
                _columnChooserForm.Show();
                PositionColumnChooserAtBottomRight();
                return;
            }

            CreateColumnChooserForm();
            _columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
        }

        private void PopulateColumnChooserListBox()
        {
            if (_columnChooserListBox == null)
            {
                return;
            }

            _columnChooserListBox.Items.Clear();
            if (_gridReport.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            foreach (UltraGridColumn col in _gridReport.DisplayLayout.Bands[0].Columns)
            {
                if (col.Hidden)
                {
                    string displayText = !string.IsNullOrWhiteSpace(col.Header.Caption) ? col.Header.Caption : col.Key;
                    _columnChooserListBox.Items.Add(new ColumnItem(col.Key, displayText));
                }
            }
        }

        private void GridReport_MouseDown(object sender, MouseEventArgs e)
        {
            _isDraggingColumn = false;
            _columnToMove = null;
            _dragStartPoint = new Point(e.X, e.Y);

            if (e.Y >= 40 || _gridReport.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            int xPos = 0;
            if (_gridReport.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True)
            {
                xPos += _gridReport.DisplayLayout.Override.RowSelectorWidth;
            }

            foreach (UltraGridColumn col in _gridReport.DisplayLayout.Bands[0].Columns)
            {
                if (col.Hidden)
                {
                    continue;
                }

                if (e.X >= xPos && e.X < xPos + col.Width)
                {
                    _columnToMove = col;
                    _isDraggingColumn = true;
                    break;
                }

                xPos += col.Width;
            }
        }

        private void GridReport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingColumn || _columnToMove == null || e.Button != MouseButtons.Left)
            {
                return;
            }

            int deltaX = Math.Abs(e.X - _dragStartPoint.X);
            int deltaY = Math.Abs(e.Y - _dragStartPoint.Y);
            if (deltaX <= SystemInformation.DragSize.Width && deltaY <= SystemInformation.DragSize.Height)
            {
                return;
            }

            bool isDraggingDown = e.Y > _dragStartPoint.Y && deltaY > deltaX;
            if (!isDraggingDown)
            {
                return;
            }

            _gridReport.Cursor = Cursors.No;
            string columnName = !string.IsNullOrWhiteSpace(_columnToMove.Header.Caption) ? _columnToMove.Header.Caption : _columnToMove.Key;
            _toolTip.SetToolTip(_gridReport, $"Drag down to hide '{columnName}' column");

            if (e.Y - _dragStartPoint.Y > 50)
            {
                HideColumn(_columnToMove);
                _columnToMove = null;
                _isDraggingColumn = false;
                _gridReport.Cursor = Cursors.Default;
                _toolTip.SetToolTip(_gridReport, string.Empty);
            }
        }

        private void GridReport_MouseUp(object sender, MouseEventArgs e)
        {
            _gridReport.Cursor = Cursors.Default;
            _toolTip.SetToolTip(_gridReport, string.Empty);
            _isDraggingColumn = false;
            _columnToMove = null;
        }

        private void HideColumn(UltraGridColumn column)
        {
            if (column == null || column.Hidden)
            {
                return;
            }

            _savedColumnWidths[column.Key] = column.Width;
            _gridReport.SuspendLayout();
            column.Hidden = true;

            foreach (UltraGridColumn col in _gridReport.DisplayLayout.Bands[0].Columns)
            {
                if (!col.Hidden && _savedColumnWidths.ContainsKey(col.Key))
                {
                    col.Width = _savedColumnWidths[col.Key];
                }
            }

            _gridReport.ResumeLayout();
            PopulateColumnChooserListBox();
            SaveGridLayout();
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = _columnChooserListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches && _columnChooserListBox.Items[index] is ColumnItem item)
            {
                _columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
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
                column.Hidden = true;
                PopulateColumnChooserListBox();
                SaveGridLayout();
            }
        }

        private void GridReport_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(ColumnItem)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void GridReport_DragDrop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(typeof(ColumnItem)) is ColumnItem item))
            {
                return;
            }

            if (_gridReport.DisplayLayout.Bands.Count == 0 || !_gridReport.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
            {
                return;
            }

            UltraGridColumn column = _gridReport.DisplayLayout.Bands[0].Columns[item.ColumnKey];
            column.Hidden = false;
            if (_savedColumnWidths.ContainsKey(column.Key))
            {
                column.Width = _savedColumnWidths[column.Key];
            }

            PopulateColumnChooserListBox();
            _toolTip.Show($"'{item.DisplayText}' restored", _gridReport, _gridReport.PointToClient(MousePosition), 1500);
            SaveGridLayout();
        }

        private void ColumnChooserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_columnChooserListBox != null)
            {
                _columnChooserListBox.MouseDown -= ColumnChooserListBox_MouseDown;
                _columnChooserListBox.DragOver -= ColumnChooserListBox_DragOver;
                _columnChooserListBox.DragDrop -= ColumnChooserListBox_DragDrop;
                _columnChooserListBox = null;
            }
        }

        private void ApplySavedLayoutIfAvailable()
        {
            if (_gridLayoutLoaded || !File.Exists(GridLayoutPath))
            {
                return;
            }

            if (_gridReport.DisplayLayout.Bands.Count == 0 || _gridReport.DisplayLayout.Bands[0].Columns.Count == 0)
            {
                return;
            }

            try
            {
                _gridReport.DisplayLayout.LoadFromXml(GridLayoutPath);
                _gridLayoutLoaded = true;
            }
            catch
            {
                _gridLayoutLoaded = false;
            }
        }

        private void SaveGridLayout()
        {
            if (_gridReport.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            try
            {
                _gridReport.DisplayLayout.SaveAsXml(GridLayoutPath);
                _gridLayoutLoaded = true;
            }
            catch
            {
            }
        }

        private void FrmCombinedPartyBalanceReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveGridLayout();
            if (_columnChooserForm != null && !_columnChooserForm.IsDisposed)
            {
                _columnChooserForm.Close();
            }
        }

        private static Button CreateButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                Width = 92,
                Height = 30,
                BackColor = backColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Standard,
                Margin = new Padding(0, 18, 10, 0),
                UseVisualStyleBackColor = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private static Panel CreateLabeledHost(string caption, Control control, int width)
        {
            Label label = new Label
            {
                Text = caption,
                AutoSize = true,
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new Point(0, 0)
            };

            control.Location = new Point(0, 18);
            control.Width = width;

            Panel panel = new Panel
            {
                Width = width + 6,
                Height = 52,
                Margin = new Padding(0, 0, 12, 0)
            };

            panel.Controls.Add(label);
            panel.Controls.Add(control);
            return panel;
        }

        private static Panel CreateSummaryCard(string caption, Color accentColor, out Label valueLabel)
        {
            Label titleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = accentColor,
                Text = caption + ":"
            };

            valueLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = accentColor,
                Text = "0",
                TextAlign = ContentAlignment.MiddleLeft
            };

            Panel panel = new Panel
            {
                Width = 230,
                Height = 58,
                Margin = new Padding(0, 4, 18, 0),
                BackColor = Color.Transparent
            };

            panel.Controls.Add(valueLabel);
            panel.Controls.Add(titleLabel);
            return panel;
        }

        private class ColumnItem
        {
            public string ColumnKey { get; private set; }
            public string DisplayText { get; private set; }

            public ColumnItem(string columnKey, string displayText)
            {
                ColumnKey = columnKey;
                DisplayText = displayText;
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        private static void ConfigureGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.Hidden = false;
            column.Header.Caption = header;
            column.Width = width;
            column.CellAppearance.TextHAlign = align;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }

        private static string EscapeCsv(string value)
        {
            string safeValue = value ?? string.Empty;
            if (!safeValue.Contains(",") && !safeValue.Contains("\"") && !safeValue.Contains("\n"))
                return safeValue;

            return $"\"{safeValue.Replace("\"", "\"\"")}\"";
        }

        private static string NormalizeText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return string.Join(" ", value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private string ResolvePartyName(string partyType, string rawName, bool allowApproximate)
        {
            string normalizedName = NormalizeText(rawName);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return null;
            }

            Dictionary<string, string> nameMap = GetNameMap(partyType);
            if (nameMap == null || nameMap.Count == 0)
            {
                return normalizedName;
            }

            string lookupKey = BuildLookupKey(normalizedName);
            string resolvedName;
            if (nameMap.TryGetValue(lookupKey, out resolvedName))
            {
                return resolvedName;
            }

            if (!allowApproximate)
            {
                return normalizedName;
            }

            List<string> candidates = GetMasterNames(partyType)
                .Where(x => IsCloseMatch(normalizedName, x))
                .ToList();

            return candidates.Count == 1 ? candidates[0] : normalizedName;
        }

        private Dictionary<string, string> GetNameMap(string partyType)
        {
            if (string.Equals(partyType, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                return _customerNameMap;
            }

            if (string.Equals(partyType, "Vendor", StringComparison.OrdinalIgnoreCase))
            {
                return _vendorNameMap;
            }

            return null;
        }

        private List<string> GetMasterNames(string partyType)
        {
            if (string.Equals(partyType, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                return _customerMasterNames;
            }

            if (string.Equals(partyType, "Vendor", StringComparison.OrdinalIgnoreCase))
            {
                return _vendorMasterNames;
            }

            return new List<string>();
        }

        private static Dictionary<string, string> BuildNameMap(IEnumerable<string> names)
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string name in names.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                string normalizedName = NormalizeText(name);
                string key = BuildLookupKey(normalizedName);
                if (!map.ContainsKey(key))
                {
                    map.Add(key, normalizedName);
                }
            }

            return map;
        }

        private static string BuildLookupKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return new string(value.ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
        }

        private static bool IsCloseMatch(string source, string target)
        {
            string sourceKey = BuildLookupKey(source);
            string targetKey = BuildLookupKey(target);

            if (string.IsNullOrWhiteSpace(sourceKey) || string.IsNullOrWhiteSpace(targetKey))
            {
                return false;
            }

            if (sourceKey[0] != targetKey[0] || Math.Abs(sourceKey.Length - targetKey.Length) > 2)
            {
                return false;
            }

            return ComputeLevenshteinDistance(sourceKey, targetKey) <= 2;
        }

        private static int ComputeLevenshteinDistance(string source, string target)
        {
            int[,] distance = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; i++)
            {
                distance[i, 0] = i;
            }

            for (int j = 0; j <= target.Length; j++)
            {
                distance[0, j] = j;
            }

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[source.Length, target.Length];
        }
    }
}
