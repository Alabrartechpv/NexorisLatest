using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WinFormsToolTip = System.Windows.Forms.ToolTip;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class frmSalesmanIncentiveReport : Form
    {
        private readonly SalesmanIncentiveReportRepository _reportRepository = new SalesmanIncentiveReportRepository();
        private readonly Dropdowns _dropdowns = new Dropdowns();
        private SalesmanIncentiveReportData _currentReport = new SalesmanIncentiveReportData();
        private List<SalesmanIncentiveDetail> _activeDetails = new List<SalesmanIncentiveDetail>();
        private bool _selectionHidden;
        private bool _gridLayoutsLoaded;
        private Form _columnChooserForm;
        private ListBox _columnChooserListBox;
        private UltraGrid _columnChooserTargetGrid;
        private UltraGridColumn _columnToMove;
        private Point _dragStartPoint;
        private bool _isDraggingColumn;
        private readonly Dictionary<string, int> _savedColumnWidths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly WinFormsToolTip _columnToolTip = new WinFormsToolTip();
        private List<LookupItem> _salesmanLookupItems = new List<LookupItem>();
        private List<LookupItem> _userLookupItems = new List<LookupItem>();
        private List<LookupItem> _groupLookupItems = new List<LookupItem>();
        private List<LookupItem> _categoryLookupItems = new List<LookupItem>();
        private List<LookupItem> _brandLookupItems = new List<LookupItem>();
        private List<LookupItem> _vendorLookupItems = new List<LookupItem>();
        private int _selectedSalesmanId;
        private int _selectedUserId;
        private int _selectedGroupId;
        private int _selectedCategoryId;
        private int _selectedBrandId;
        private int _selectedVendorId;

        private const string SummaryGridLayoutFileName = "SalesmanIncentiveSummaryGridLayout.xml";
        private const string DetailGridLayoutFileName = "SalesmanIncentiveDetailGridLayout.xml";
        private string SummaryGridLayoutPath => Path.Combine(Application.StartupPath, SummaryGridLayoutFileName);
        private string DetailGridLayoutPath => Path.Combine(Application.StartupPath, DetailGridLayoutFileName);

        public frmSalesmanIncentiveReport()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            Text = "Salesman Incentive Report";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            KeyDown += frmSalesmanIncentiveReport_KeyDown;
            FormClosing += frmSalesmanIncentiveReport_FormClosing;

            ConfigureButtons();
            ConfigurePresetCombo();
            ConfigureGrids();
            LoadLookups();
            ApplyDefaultFilters();
        }

        private void ConfigureButtons()
        {
            ConfigureButton(btnViewGrid, Color.FromArgb(25, 118, 210));
            ConfigureButton(btnPreviewGrid, Color.FromArgb(0, 121, 107));
            ConfigureButton(btnPreviewReport, Color.FromArgb(123, 31, 162));
            ConfigureButton(btnHideSelection, Color.FromArgb(97, 97, 97));
        }

        private static void ConfigureButton(Infragistics.Win.Misc.UltraButton button, Color backColor)
        {
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.Appearance.BackColor = backColor;
            button.Appearance.BackColor2 = ControlPaint.Light(backColor);
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.ButtonStyle = UIElementButtonStyle.Office2013Button;
        }

        private void ConfigurePresetCombo()
        {
            cmbDatePreset.Items.Clear();
            cmbDatePreset.Items.Add("Today", "Today");
            cmbDatePreset.Items.Add("Yesterday", "Yesterday");
            cmbDatePreset.Items.Add("ThisWeek", "This Week");
            cmbDatePreset.Items.Add("LastWeek", "Last Week");
            cmbDatePreset.Items.Add("ThisMonth", "This Month");
            cmbDatePreset.Items.Add("LastMonth", "Last Month");
            cmbDatePreset.Items.Add("Custom", "Custom Range");
            cmbDatePreset.DropDownStyle = DropDownStyle.DropDownList;
            cmbDatePreset.Value = "ThisMonth";
        }

        private void ConfigureGrids()
        {
            ConfigureGrid(gridSummary);
            ConfigureGrid(gridDetails);
            gridSummary.InitializeLayout += gridSummary_InitializeLayout;
            gridDetails.InitializeLayout += gridDetails_InitializeLayout;
            gridSummary.AfterRowActivate += gridSummary_AfterRowActivate;
            SetupColumnChooserMenu(gridSummary);
            SetupColumnChooserMenu(gridDetails);
        }

        private static void ConfigureGrid(UltraGrid grid)
        {
            grid.DisplayLayout.Reset();
            grid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            grid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            grid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            grid.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            grid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            grid.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            grid.DisplayLayout.Override.RowSelectorWidth = 40;
            grid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            grid.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            grid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            grid.DisplayLayout.GroupByBox.Hidden = true;
            grid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            grid.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 250, 252);
            grid.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(66, 165, 245);
            grid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(55, 71, 79);
            grid.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(69, 90, 100);
            grid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            grid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            grid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            grid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            grid.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            grid.UseOsThemes = DefaultableBoolean.False;
        }

        private void LoadLookups()
        {
            _salesmanLookupItems = BuildLookupItems(_dropdowns.getUsersDDl().List, x => x.UserID, x => x.UserName);
            _userLookupItems = BuildLookupItems(_dropdowns.getUsersDDl().List, x => x.UserID, x => x.UserName);
            _groupLookupItems = BuildLookupItems(_dropdowns.getGroupDDl().List, x => x.Id, x => x.GroupName);
            _categoryLookupItems = BuildLookupItems(_dropdowns.getCategoryDDl(string.Empty).List, x => x.Id, x => x.CategoryName);
            _brandLookupItems = BuildLookupItems(_dropdowns.getBrandDDL().List, x => x.Id, x => x.BrandName);
            _vendorLookupItems = BuildLookupItems(_dropdowns.VendorDDL().List, x => x.LedgerID, x => x.LedgerName);

            ApplyLookupSelection(txtSalesman, _salesmanLookupItems, ref _selectedSalesmanId);
            ApplyLookupSelection(txtUser, _userLookupItems, ref _selectedUserId);
            ApplyLookupSelection(txtGroup, _groupLookupItems, ref _selectedGroupId);
            ApplyLookupSelection(txtCategory, _categoryLookupItems, ref _selectedCategoryId);
            ApplyLookupSelection(txtBrand, _brandLookupItems, ref _selectedBrandId);
            ApplyLookupSelection(txtVendor, _vendorLookupItems, ref _selectedVendorId);
        }

        private static void ApplyLookupSelection(TextBox textBox, List<LookupItem> items, ref int selectedId)
        {
            if (items == null)
            {
                items = new List<LookupItem>();
            }

            int currentSelectedId = selectedId;
            if (!items.Any(x => x.Id == currentSelectedId))
            {
                selectedId = 0;
                currentSelectedId = 0;
            }

            LookupItem selected = items.FirstOrDefault(x => x.Id == currentSelectedId) ?? items.FirstOrDefault(x => x.Id == 0);
            if (textBox != null)
            {
                textBox.Text = selected != null ? selected.Name : "All";
            }
        }

        private void btnLookupSalesman_Click(object sender, EventArgs e)
        {
            string selectedSalesmanName = null;
            using (frmSalesPersonDial dialog = new frmSalesPersonDial())
            {
                dialog.OnSalesPersonSelected += name => selectedSalesmanName = name;
                if (dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(selectedSalesmanName))
                {
                    RefreshLookup(ref _salesmanLookupItems, _dropdowns.getUsersDDl().List, x => x.UserID, x => x.UserName);
                    LookupItem match = _salesmanLookupItems.FirstOrDefault(x => string.Equals(x.Name, selectedSalesmanName, StringComparison.OrdinalIgnoreCase));
                    _selectedSalesmanId = match != null ? match.Id : 0;
                    ApplyLookupSelection(txtSalesman, _salesmanLookupItems, ref _selectedSalesmanId);
                }
            }
        }

        private void btnLookupUser_Click(object sender, EventArgs e)
        {
            SelectLookupValue("Select User", txtUser, _userLookupItems, ref _selectedUserId);
        }

        private void btnLookupGroup_Click(object sender, EventArgs e)
        {
            RefreshLookup(ref _groupLookupItems, _dropdowns.getGroupDDl().List, x => x.Id, x => x.GroupName);
            using (frmGroupDialog dialog = new frmGroupDialog())
            {
                dialog.TargetTextBox = txtGroup;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _selectedGroupId = dialog.SelectedGroupId;
                    ApplyLookupSelection(txtGroup, _groupLookupItems, ref _selectedGroupId);
                }
            }
        }

        private void btnLookupCategory_Click(object sender, EventArgs e)
        {
            RefreshLookup(ref _categoryLookupItems, _dropdowns.getCategoryDDl(string.Empty).List, x => x.Id, x => x.CategoryName);
            using (frmCategoryDialog dialog = new frmCategoryDialog("frmSalesmanIncentiveReport"))
            {
                dialog.TargetTextBox = txtCategory;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _selectedCategoryId = dialog.SelectedCategoryId;
                    ApplyLookupSelection(txtCategory, _categoryLookupItems, ref _selectedCategoryId);
                }
            }
        }

        private void btnLookupBrand_Click(object sender, EventArgs e)
        {
            RefreshLookup(ref _brandLookupItems, _dropdowns.getBrandDDL().List, x => x.Id, x => x.BrandName);
            using (frmBrandDialog dialog = new frmBrandDialog())
            {
                dialog.TargetTextBox = txtBrand;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _selectedBrandId = dialog.SelectedBrandId;
                    ApplyLookupSelection(txtBrand, _brandLookupItems, ref _selectedBrandId);
                }
            }
        }

        private void btnLookupVendor_Click(object sender, EventArgs e)
        {
            using (frmVendorDig dialog = new frmVendorDig())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshLookup(ref _vendorLookupItems, _dropdowns.VendorDDL().List, x => x.LedgerID, x => x.LedgerName);
                    _selectedVendorId = dialog.SelectedVendorId;
                    ApplyLookupSelection(txtVendor, _vendorLookupItems, ref _selectedVendorId);
                }
            }
        }

        private static void RefreshLookup<T>(ref List<LookupItem> target, IEnumerable<T> source, Func<T, int> idSelector, Func<T, string> nameSelector)
        {
            target = BuildLookupItems(source, idSelector, nameSelector);
        }

        private void SelectLookupValue(string title, TextBox textBox, List<LookupItem> items, ref int selectedId)
        {
            using (LookupPickerDialog dialog = new LookupPickerDialog(title, items, selectedId))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    selectedId = dialog.SelectedId;
                    ApplyLookupSelection(textBox, items, ref selectedId);
                }
            }
        }

        private static List<LookupItem> BuildLookupItems<T>(IEnumerable<T> source, Func<T, int> idSelector, Func<T, string> nameSelector)
        {
            List<LookupItem> items = new List<LookupItem> { new LookupItem { Id = 0, Name = "All" } };
            if (source != null)
            {
                items.AddRange(source.Select(item => new LookupItem
                {
                    Id = idSelector(item),
                    Name = string.IsNullOrWhiteSpace(nameSelector(item)) ? "(Blank)" : nameSelector(item)
                }));
            }

            return items.GroupBy(x => x.Id).Select(x => x.First()).OrderBy(x => x.Id == 0 ? -1 : 0).ThenBy(x => x.Name).ToList();
        }

        private void ApplyDefaultFilters()
        {
            dtFromDate.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtToDate.Value = DateTime.Today;
            numIncentivePercent.Value = 5M;
        }

        private void LoadReport()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                SalesmanIncentiveReportFilter filter = new SalesmanIncentiveReportFilter
                {
                    FromDate = dtFromDate.DateTime.Date,
                    ToDate = dtToDate.DateTime.Date,
                    CompanyId = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : Convert.ToInt32(DataBase.CompanyId),
                    BranchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : Convert.ToInt32(DataBase.BranchId),
                    SalesmanId = _selectedSalesmanId,
                    UserId = _selectedUserId,
                    GroupId = _selectedGroupId,
                    CategoryId = _selectedCategoryId,
                    BrandId = _selectedBrandId,
                    VendorId = _selectedVendorId,
                    IncentivePercent = Convert.ToDecimal(numIncentivePercent.Value ?? 0M),
                    IncludeDetails = true
                };

                _currentReport = _reportRepository.GetSalesmanIncentiveReport(filter) ?? new SalesmanIncentiveReportData();
                gridSummary.DataSource = _currentReport.Summary;
                _activeDetails = _currentReport.Details ?? new List<SalesmanIncentiveDetail>();
                gridDetails.DataSource = _activeDetails;
                LoadGridLayouts();
                UpdateFooter(_currentReport.Summary, _activeDetails);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load salesman incentive report.\n" + ex.Message, "Report", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void UpdateFooter(IEnumerable<SalesmanIncentiveSummary> summary, IEnumerable<SalesmanIncentiveDetail> details)
        {
            List<SalesmanIncentiveSummary> summaryList = summary == null ? new List<SalesmanIncentiveSummary>() : summary.ToList();
            List<SalesmanIncentiveDetail> detailList = details == null ? new List<SalesmanIncentiveDetail>() : details.ToList();
            lblSummaryCount.Text = "Summary Rows: " + summaryList.Count.ToString("N0");
            lblDetailCount.Text = "Detail Rows: " + detailList.Count.ToString("N0");
            lblNetProfitValue.Text = summaryList.Sum(x => x.NetProfit).ToString("N3");
            lblIncentiveValue.Text = summaryList.Sum(x => x.IncentiveAmount).ToString("N3");
        }

        private void FilterDetailsForActiveSummary()
        {
            if (gridSummary.ActiveRow == null || _currentReport == null || _currentReport.Details == null)
            {
                gridDetails.DataSource = _activeDetails;
                return;
            }

            int salesmanId = Convert.ToInt32(gridSummary.ActiveRow.Cells["SalesmanId"].Value);
            int branchId = Convert.ToInt32(gridSummary.ActiveRow.Cells["BranchId"].Value);
            List<SalesmanIncentiveDetail> filtered = _currentReport.Details.Where(x => x.SalesmanId == salesmanId && x.BranchId == branchId).ToList();
            gridDetails.DataSource = filtered;
            UpdateFooter(_currentReport.Summary, filtered);
        }

        private void ApplyDatePreset(string preset)
        {
            DateTime today = DateTime.Today;
            DateTime from;
            DateTime to;

            switch (preset)
            {
                case "Today":
                    from = today;
                    to = today;
                    break;
                case "Yesterday":
                    from = today.AddDays(-1);
                    to = today.AddDays(-1);
                    break;
                case "ThisWeek":
                    int diff = today.DayOfWeek == DayOfWeek.Sunday ? 6 : ((int)today.DayOfWeek - 1);
                    from = today.AddDays(-diff);
                    to = today;
                    break;
                case "LastWeek":
                    int currentDiff = today.DayOfWeek == DayOfWeek.Sunday ? 6 : ((int)today.DayOfWeek - 1);
                    to = today.AddDays(-currentDiff - 1);
                    from = to.AddDays(-6);
                    break;
                case "LastMonth":
                    DateTime firstOfThisMonth = new DateTime(today.Year, today.Month, 1);
                    from = firstOfThisMonth.AddMonths(-1);
                    to = firstOfThisMonth.AddDays(-1);
                    break;
                case "Custom":
                    return;
                default:
                    from = new DateTime(today.Year, today.Month, 1);
                    to = today;
                    break;
            }

            dtFromDate.Value = from;
            dtToDate.Value = to;
        }

        private void gridSummary_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];
            ConfigureNumberColumn(band, "SalesQty", "Sales Qty");
            ConfigureNumberColumn(band, "IncentivePercent", "Incentive %");
            ConfigureNumberColumn(band, "SalesAmount", "Sales Amount");
            ConfigureNumberColumn(band, "GrossProfit", "Gross Profit");
            ConfigureNumberColumn(band, "SalesReturnLoss", "Return Loss");
            ConfigureNumberColumn(band, "NetProfit", "Net Profit");
            ConfigureNumberColumn(band, "IncentiveAmount", "Incentive Amount");
            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void gridDetails_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];
            ConfigureNumberColumn(band, "Qty", "Qty");
            ConfigureNumberColumn(band, "CostPerUnit", "Cost/Unit");
            ConfigureNumberColumn(band, "SalesPricePerUnit", "Price/Unit");
            ConfigureNumberColumn(band, "SalesValue", "Sales Value");
            ConfigureNumberColumn(band, "CostValue", "Cost Value");
            ConfigureNumberColumn(band, "ProfitValue", "Profit");
            if (band.Columns.Exists("TransactionDate"))
            {
                band.Columns["TransactionDate"].Format = "dd-MM-yyyy HH:mm";
                band.Columns["TransactionDate"].Width = 125;
            }

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private static void ConfigureNumberColumn(UltraGridBand band, string key, string caption)
        {
            if (!band.Columns.Exists(key))
            {
                return;
            }

            band.Columns[key].Header.Caption = caption;
            band.Columns[key].Format = "0.000";
            band.Columns[key].CellAppearance.TextHAlign = HAlign.Right;
            band.Columns[key].Width = 100;
        }

        private void btnViewGrid_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            LoadReport();
            MessageBox.Show("Grid report is ready. Crystal preview is not wired yet for this report.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHideSelection_Click(object sender, EventArgs e)
        {
            _selectionHidden = !_selectionHidden;
            pnlSelection.Visible = !_selectionHidden;
            btnHideSelection.Text = _selectionHidden ? "Show Selection" : "Hide Selection";
        }

        private void cmbDatePreset_ValueChanged(object sender, EventArgs e)
        {
            if (cmbDatePreset.Value != null)
            {
                ApplyDatePreset(cmbDatePreset.Value.ToString());
            }
        }

        private void gridSummary_AfterRowActivate(object sender, EventArgs e)
        {
            FilterDetailsForActiveSummary();
        }

        private void frmSalesmanIncentiveReport_Load(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void frmSalesmanIncentiveReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                LoadReport();
                e.Handled = true;
            }
        }

        private void LoadGridLayouts()
        {
            if (_gridLayoutsLoaded)
            {
                return;
            }

            TryLoadGridLayout(gridSummary, SummaryGridLayoutPath);
            TryLoadGridLayout(gridDetails, DetailGridLayoutPath);
            _gridLayoutsLoaded = true;
        }

        private void SaveGridLayouts()
        {
            TrySaveGridLayout(gridSummary, SummaryGridLayoutPath);
            TrySaveGridLayout(gridDetails, DetailGridLayoutPath);
        }

        private static void TryLoadGridLayout(UltraGrid grid, string layoutPath)
        {
            try
            {
                if (grid != null && File.Exists(layoutPath))
                {
                    grid.DisplayLayout.LoadFromXml(layoutPath);
                }
            }
            catch
            {
            }
        }

        private static void TrySaveGridLayout(UltraGrid grid, string layoutPath)
        {
            try
            {
                if (grid != null)
                {
                    grid.DisplayLayout.SaveAsXml(layoutPath);
                }
            }
            catch
            {
            }
        }

        private void frmSalesmanIncentiveReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveGridLayouts();
        }

        private void SetupColumnChooserMenu(UltraGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += (s, e) => ShowColumnChooser(grid);
            gridContextMenu.Items.Add(columnChooserMenuItem);
            grid.ContextMenuStrip = gridContextMenu;

            grid.AllowDrop = true;
            grid.MouseDown += Grid_MouseDown;
            grid.MouseMove += Grid_MouseMove;
            grid.MouseUp += Grid_MouseUp;
            grid.DragOver += Grid_DragOver;
            grid.DragDrop += Grid_DragDrop;
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

            _columnChooserForm.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                _columnChooserForm.Hide();
            };
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

            _columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            _columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            _columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            _columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;

            _columnChooserForm.Controls.Add(_columnChooserListBox);
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || _columnChooserListBox == null)
            {
                return;
            }

            ColumnItem item = _columnChooserListBox.Items[e.Index] as ColumnItem;
            if (item == null)
            {
                return;
            }

            Rectangle rect = e.Bounds;
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
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillPath(bgBrush, path);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };
                e.Graphics.DrawString(item.DisplayText, e.Font, textBrush, rect, sf);
            }
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

        private void ShowColumnChooser(UltraGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            _columnChooserTargetGrid = grid;
            if (_columnChooserForm == null || _columnChooserForm.IsDisposed || _columnChooserListBox == null)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox(grid);
            _columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
        }

        private void PopulateColumnChooserListBox(UltraGrid grid)
        {
            if (_columnChooserListBox == null || grid == null)
            {
                return;
            }

            _columnChooserListBox.Items.Clear();
            if (grid.DisplayLayout.Bands.Count <= 0)
            {
                return;
            }

            foreach (UltraGridColumn col in grid.DisplayLayout.Bands[0].Columns)
            {
                if (col.Hidden)
                {
                    string displayText = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    _columnChooserListBox.Items.Add(new ColumnItem(col.Key, displayText, grid.Name));
                }
            }
        }

        private sealed class ColumnItem
        {
            public string ColumnKey { get; }
            public string DisplayText { get; }
            public string GridName { get; }

            public ColumnItem(string key, string text, string gridName)
            {
                ColumnKey = key;
                DisplayText = text;
                GridName = gridName;
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        private void Grid_MouseDown(object sender, MouseEventArgs e)
        {
            UltraGrid grid = sender as UltraGrid;
            _isDraggingColumn = false;
            _columnToMove = null;
            _columnChooserTargetGrid = grid;
            _dragStartPoint = new Point(e.X, e.Y);

            if (grid == null || e.Y >= 40 || grid.DisplayLayout.Bands.Count <= 0)
            {
                return;
            }

            int xPos = 0;
            if (grid.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True)
            {
                xPos += grid.DisplayLayout.Override.RowSelectorWidth;
            }

            foreach (UltraGridColumn col in grid.DisplayLayout.Bands[0].Columns)
            {
                if (!col.Hidden)
                {
                    if (e.X >= xPos && e.X < xPos + col.Width)
                    {
                        _columnToMove = col;
                        _isDraggingColumn = true;
                        break;
                    }
                    xPos += col.Width;
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            UltraGrid grid = sender as UltraGrid;
            if (!_isDraggingColumn || _columnToMove == null || grid == null || e.Button != MouseButtons.Left)
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

            grid.Cursor = Cursors.No;
            string columnName = !string.IsNullOrEmpty(_columnToMove.Header.Caption) ? _columnToMove.Header.Caption : _columnToMove.Key;
            _columnToolTip.SetToolTip(grid, $"Drag down to hide '{columnName}' column");

            if (e.Y - _dragStartPoint.Y > 50)
            {
                HideColumn(grid, _columnToMove);
                _columnToMove = null;
                _isDraggingColumn = false;
                grid.Cursor = Cursors.Default;
                _columnToolTip.SetToolTip(grid, string.Empty);
            }
        }

        private void Grid_MouseUp(object sender, MouseEventArgs e)
        {
            UltraGrid grid = sender as UltraGrid;
            if (grid != null)
            {
                grid.Cursor = Cursors.Default;
                _columnToolTip.SetToolTip(grid, string.Empty);
            }

            _isDraggingColumn = false;
            _columnToMove = null;
        }

        private void HideColumn(UltraGrid grid, UltraGridColumn column)
        {
            if (grid == null || column == null || column.Hidden)
            {
                return;
            }

            if (GetEssentialColumns(grid).Contains(column.Key, StringComparer.OrdinalIgnoreCase))
            {
                MessageBox.Show($"The '{column.Header.Caption}' column is essential and cannot be hidden.", "Cannot Hide Column", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _savedColumnWidths[grid.Name + "|" + column.Key] = column.Width;
            grid.SuspendLayout();
            column.Hidden = true;
            foreach (UltraGridColumn col in grid.DisplayLayout.Bands[0].Columns)
            {
                string widthKey = grid.Name + "|" + col.Key;
                if (!col.Hidden && _savedColumnWidths.ContainsKey(widthKey))
                {
                    col.Width = _savedColumnWidths[widthKey];
                }
            }
            grid.ResumeLayout();

            if (_columnChooserForm == null || _columnChooserForm.IsDisposed || _columnChooserListBox == null)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox(grid);
            if (_columnChooserTargetGrid == grid && _columnChooserForm != null && _columnChooserForm.Visible)
            {
                PositionColumnChooserAtBottomRight();
            }
        }

        private static IEnumerable<string> GetEssentialColumns(UltraGrid grid)
        {
            if (grid == null)
            {
                return Enumerable.Empty<string>();
            }

            if (grid.Name == "gridSummary")
            {
                return new[] { "BranchName", "SalesmanName", "NetProfit", "IncentiveAmount" };
            }

            return new[] { "TransactionDate", "ItemName", "ProfitValue" };
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (_columnChooserListBox == null)
            {
                return;
            }

            int index = _columnChooserListBox.IndexFromPoint(e.Location);
            if (index == ListBox.NoMatches)
            {
                return;
            }

            ColumnItem item = _columnChooserListBox.Items[index] as ColumnItem;
            if (item != null)
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
            UltraGridColumn column = e.Data.GetData(typeof(UltraGridColumn)) as UltraGridColumn;
            if (column == null || column.Hidden)
            {
                return;
            }

            UltraGrid grid = column.Band != null && column.Band.Layout != null ? column.Band.Layout.Grid as UltraGrid : null;
            if (grid == null)
            {
                return;
            }

            column.Hidden = true;
            PopulateColumnChooserListBox(grid);
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(ColumnItem)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void Grid_DragDrop(object sender, DragEventArgs e)
        {
            UltraGrid grid = sender as UltraGrid;
            ColumnItem item = e.Data.GetData(typeof(ColumnItem)) as ColumnItem;
            if (grid == null || item == null || !string.Equals(item.GridName, grid.Name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (grid.DisplayLayout.Bands.Count > 0 && grid.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
            {
                UltraGridColumn column = grid.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                column.Hidden = false;
                string widthKey = grid.Name + "|" + item.ColumnKey;
                if (_savedColumnWidths.ContainsKey(widthKey))
                {
                    column.Width = _savedColumnWidths[widthKey];
                }
                PopulateColumnChooserListBox(grid);
                _columnToolTip.Show($"'{item.DisplayText}' restored", grid, grid.PointToClient(MousePosition), 1500);
            }
        }

        private sealed class LookupItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private sealed class LookupPickerDialog : Form
        {
            private readonly List<LookupItem> _allItems;
            private readonly TextBox _txtSearch;
            private readonly ListBox _lstItems;
            private readonly Button _btnOk;
            private readonly Button _btnCancel;

            public int SelectedId { get; private set; }

            public LookupPickerDialog(string title, List<LookupItem> items, int selectedId)
            {
                _allItems = items ?? new List<LookupItem>();
                SelectedId = selectedId;

                Text = title;
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MinimizeBox = false;
                MaximizeBox = false;
                ShowInTaskbar = false;
                ClientSize = new Size(420, 480);

                Label lblSearch = new Label
                {
                    AutoSize = true,
                    Location = new Point(12, 15),
                    Text = "Search"
                };

                _txtSearch = new TextBox
                {
                    Location = new Point(68, 12),
                    Size = new Size(340, 20)
                };
                _txtSearch.TextChanged += (s, e) => BindItems(_txtSearch.Text);

                _lstItems = new ListBox
                {
                    Location = new Point(12, 42),
                    Size = new Size(396, 386),
                    DisplayMember = "Name"
                };
                _lstItems.DoubleClick += (s, e) => ConfirmSelection();

                _btnOk = new Button
                {
                    Location = new Point(252, 440),
                    Size = new Size(75, 27),
                    Text = "OK"
                };
                _btnOk.Click += (s, e) => ConfirmSelection();

                _btnCancel = new Button
                {
                    Location = new Point(333, 440),
                    Size = new Size(75, 27),
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel
                };

                Controls.Add(lblSearch);
                Controls.Add(_txtSearch);
                Controls.Add(_lstItems);
                Controls.Add(_btnOk);
                Controls.Add(_btnCancel);

                AcceptButton = _btnOk;
                CancelButton = _btnCancel;

                BindItems(string.Empty);
                SelectCurrentItem();
            }

            private void BindItems(string searchText)
            {
                IEnumerable<LookupItem> filtered = _allItems;
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filtered = filtered.Where(x => !string.IsNullOrWhiteSpace(x.Name) &&
                                                   x.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                _lstItems.DataSource = filtered.ToList();
                _lstItems.DisplayMember = "Name";
                SelectCurrentItem();
            }

            private void SelectCurrentItem()
            {
                if (_lstItems.Items.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < _lstItems.Items.Count; i++)
                {
                    LookupItem item = _lstItems.Items[i] as LookupItem;
                    if (item != null && item.Id == SelectedId)
                    {
                        _lstItems.SelectedIndex = i;
                        return;
                    }
                }

                _lstItems.SelectedIndex = 0;
            }

            private void ConfirmSelection()
            {
                LookupItem selected = _lstItems.SelectedItem as LookupItem;
                if (selected == null)
                {
                    return;
                }

                SelectedId = selected.Id;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
