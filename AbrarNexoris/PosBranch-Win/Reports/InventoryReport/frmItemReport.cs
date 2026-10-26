using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.InventoryReport
{
    public partial class frmItemReport : Form
    {
        // ─── Theme Palette (matches FrmSmartReorderDashboard / frmVendorOutstandingReport) ────────
        private static readonly Color FormBackColor        = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue           = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor     = Color.White;
        private static readonly Color ControlTextColor     = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue       = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark   = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue     = Color.FromArgb(173, 216, 255);
        private static readonly Color GridRowLine          = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow           = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder     = Color.FromArgb(144, 181, 223);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);

        private static readonly Color ButtonTopColor       = Color.FromArgb(234, 244, 255);
        private static readonly Color ButtonBottomColor    = Color.FromArgb(152, 188, 235);
        private static readonly Color ButtonBorderColor    = Color.FromArgb(73, 119, 184);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);

        private static readonly Color PanelHoverTopColor   = Color.FromArgb(245, 250, 255);
        private static readonly Color PanelHoverBottomColor= Color.FromArgb(170, 206, 244);

        private static readonly Color PanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color PanelPressedBottomColor = Color.FromArgb(128, 170, 224);

        private ItemReportRepo itemReportRepo;
        private BaseRepostitory baseRepo;
        private int selectedItemId = 0;
        private string selectedItemName = "";

        private readonly Dictionary<string, Label> _footerLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, string> _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // ─── Column Chooser & Drag-Down to Hide State ────────────────────────────────
        private ListBox columnChooserListBox;
        private Form columnChooserForm;
        private bool isDraggingHeaderToHide;
        private UltraGridColumn columnBeingDragged;
        private Point headerDragStartPoint;
        private readonly System.Windows.Forms.ToolTip headerToolTip = new System.Windows.Forms.ToolTip();
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Cursor blackXCursor = CreateBlackXCursor();

        // ─── Column Header Filtering State ──────────────────────────────────────────
        private readonly Dictionary<string, HashSet<string>> _activeColumnFilters = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Bitmap _filterIconNormal = CreateFilterIcon(false);
        private static readonly Bitmap _filterIconActive = CreateFilterIcon(true);
        private bool _suppressSorting = false;
        private ColumnFilterDropdownForm _activeDropdownForm = null;
        private DateTime _lastDropdownCloseTime = DateTime.MinValue;
        private string _lastClosedColumnKey = null;

        public frmItemReport()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            try
            {
                ReportGridLayoutHelper.LoadHiddenColumns("ItemReport", userHiddenColumnKeys);
                itemReportRepo = new ItemReportRepo();
                baseRepo = new BaseRepostitory();

                // Apply unified theme appearance
                InitializeRuntimeAppearance();

                // Load branches dropdown
                LoadBranches();

                // Configure Grid & Buttons
                ConfigureTransactionGrid();
                StyleButtons();
                SetupSearchIcon();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeRuntimeAppearance()
        {
            BackColor = FormBackColor;

            // Panels
            if (ultraPanelControls != null)
            {
                ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelControls.Appearance.BorderColor = BorderBlue;
                ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (ultraPanelActionBar != null)
            {
                ultraPanelActionBar.Appearance.BackColor = ActionPanelBackColor;
                ultraPanelActionBar.Appearance.BorderColor = BorderBlue;
                ultraPanelActionBar.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelActionBar.Size = new Size(ultraPanelActionBar.Width, 38);
            }

            if (ultraPanelGrid != null)
            {
                ultraPanelGrid.Appearance.BackColor = FormBackColor;
                ultraPanelGrid.Appearance.BorderColor = BorderBlue;
                ultraPanelGrid.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (gridFooterPanel != null)
            {
                gridFooterPanel.Appearance.BackColor = GridHeaderBlue;
                gridFooterPanel.Appearance.BackColor2 = GridHeaderBlue;
                gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.None;
                gridFooterPanel.Appearance.BorderColor = GridFooterBorder;
                gridFooterPanel.BorderStyle = UIElementBorderStyle.Solid;
                gridFooterPanel.Height = 28;
            }

            // Labels
            StyleLabel(ultraLabelBranch);
            StyleLabel(ultraLabelItem);

            // Controls
            StyleFilterCombo(ultraComboBranch);
            StyleTextEditor(txtItemName);
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel lbl)
        {
            if (lbl == null) return;
            lbl.Appearance.BackColor = Color.Transparent;
            lbl.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            lbl.Appearance.FontData.Bold = DefaultableBoolean.False;
            lbl.Appearance.FontData.Name = "Microsoft Sans Serif";
            lbl.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo == null) return;
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Microsoft Sans Serif";
            combo.Appearance.FontData.SizeInPoints = 9F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleTextEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Microsoft Sans Serif";
            editor.Appearance.FontData.SizeInPoints = 9F;
        }

        private void SetupSearchIcon()
        {
            try
            {
                picItemSearch.UseAppStyling = false;
                picItemSearch.UseOsThemes = DefaultableBoolean.False;
                picItemSearch.Appearance.BackColor = Color.FromArgb(72, 122, 214);
                picItemSearch.Appearance.BackColor2 = Color.FromArgb(48, 90, 175);
                picItemSearch.Appearance.BackGradientStyle = GradientStyle.Vertical;
                picItemSearch.Appearance.BorderColor = Color.FromArgb(40, 80, 160);
                picItemSearch.BorderStyle = UIElementBorderStyle.Solid;
                picItemSearch.Cursor = Cursors.Hand;

                Bitmap bmp = new Bitmap(28, 25);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);
                    using (Pen p = new Pen(Color.White, 2.4f))
                    {
                        g.DrawEllipse(p, 6, 5, 10, 10);
                        g.DrawLine(p, 14, 13, 21, 19);
                    }
                }
                picItemSearch.Image = bmp;

                picItemSearch.MouseEnter += (s, e) =>
                {
                    picItemSearch.Appearance.BackColor = Color.FromArgb(95, 145, 230);
                    picItemSearch.Appearance.BackColor2 = Color.FromArgb(72, 122, 214);
                };
                picItemSearch.MouseLeave += (s, e) =>
                {
                    picItemSearch.Appearance.BackColor = Color.FromArgb(72, 122, 214);
                    picItemSearch.Appearance.BackColor2 = Color.FromArgb(48, 90, 175);
                };
            }
            catch { }
        }

        private void LoadBranches()
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)baseRepo.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("_Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        DataRow dr = dt.NewRow();
                        dr["Id"] = 0;
                        dr["BranchName"] = "--Select Branch--";
                        dt.Rows.InsertAt(dr, 0);

                        ultraComboBranch.ValueMember = "Id";
                        ultraComboBranch.DisplayMember = "BranchName";
                        ultraComboBranch.DataSource = dt;

                        if (!string.IsNullOrEmpty(DataBase.BranchId))
                        {
                            if (int.TryParse(DataBase.BranchId, out int currentBranchId))
                            {
                                ultraComboBranch.Value = currentBranchId;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading branches: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void picItemSearch_Click(object sender, EventArgs e)
        {
            OpenItemSearchDialog();
        }

        private void txtItemName_Click(object sender, EventArgs e)
        {
            OpenItemSearchDialog();
        }

        private void txtItemName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7 || e.KeyCode == Keys.Enter)
            {
                OpenItemSearchDialog();
                e.Handled = true;
            }
        }

        private void frmItemReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7)
            {
                OpenItemSearchDialog();
                e.Handled = true;
            }
        }

        private void OpenItemSearchDialog()
        {
            try
            {
                using (var itemDialog = new PosBranch_Win.DialogBox.frmdialForItemMaster("frmItemReport"))
                {
                    itemDialog.StartPosition = FormStartPosition.CenterParent;
                    if (itemDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        Dictionary<string, object> selectedData = itemDialog.GetSelectedItemData();
                        if (selectedData != null)
                        {
                            int itemId = 0;
                            if (selectedData.ContainsKey("ItemId") && selectedData["ItemId"] != null)
                                int.TryParse(selectedData["ItemId"].ToString(), out itemId);
                            else if (selectedData.ContainsKey("ItemID") && selectedData["ItemID"] != null)
                                int.TryParse(selectedData["ItemID"].ToString(), out itemId);
                            else if (selectedData.ContainsKey("Id") && selectedData["Id"] != null)
                                int.TryParse(selectedData["Id"].ToString(), out itemId);

                            string desc = "";
                            if (selectedData.ContainsKey("Description") && selectedData["Description"] != null)
                                desc = selectedData["Description"].ToString();
                            else if (selectedData.ContainsKey("ItemName") && selectedData["ItemName"] != null)
                                desc = selectedData["ItemName"].ToString();

                            if (itemId == 0 && itemDialog.SelectedItemId > 0)
                            {
                                itemId = (int)itemDialog.SelectedItemId;
                            }
                            if (string.IsNullOrEmpty(desc) && !string.IsNullOrEmpty(itemDialog.SelectedItemName))
                            {
                                desc = itemDialog.SelectedItemName;
                            }

                            if (itemId > 0)
                            {
                                selectedItemId = itemId;
                                selectedItemName = desc;
                                txtItemName.Text = desc;
                                btnSearch.Focus();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureTransactionGrid()
        {
            ApplyGridStyling(ultraGridTransactions);
            ultraGridTransactions.InitializeLayout += UltraGridTransactions_InitializeLayout;
            ultraGridTransactions.InitializeRow += UltraGridTransactions_InitializeRow;
            ultraGridTransactions.BeforeSortChange += (s, e) =>
            {
                if (_suppressSorting)
                {
                    e.Cancel = true;
                    _suppressSorting = false;
                }
            };

            // Register footer cell sync and column drag-to-hide handlers matching frmPurchaseReturn / frmStockReport
            ultraGridTransactions.Resize += (s, e) => { CloseActiveFilterDropdown(); UpdateFooterCellPositions(); };
            ultraGridTransactions.AfterColPosChanged += (s, e) =>
            {
                UpdateFooterCellPositions();
                ReportGridLayoutHelper.SaveLayout("ItemReport", ultraGridTransactions, userHiddenColumnKeys);
            };
            ultraGridTransactions.AfterColRegionScroll += (s, e) => { CloseActiveFilterDropdown(); UpdateFooterCellPositions(); };
            ultraGridTransactions.AfterRowRegionScroll += (s, e) => { CloseActiveFilterDropdown(); UpdateFooterCellPositions(); };
            ultraGridTransactions.Paint += (s, e) => UpdateFooterCellPositions();

            SetupHeaderDragToHideAndColumnChooser();
            InitializeGridFooter();

            FormClosing += (s, e) =>
            {
                ReportGridLayoutHelper.SaveLayout("ItemReport", ultraGridTransactions, userHiddenColumnKeys);
                CloseActiveFilterDropdown();
                if (columnChooserForm != null && !columnChooserForm.IsDisposed)
                {
                    columnChooserForm.Dispose();
                    columnChooserForm = null;
                }
            };
        }

        private void InitializeGridFooter()
        {
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        #region Column Chooser & Drag-Down to Hide

        private static Cursor CreateBlackXCursor()
        {
            try
            {
                using (Bitmap bmp = new Bitmap(32, 32))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    using (SolidBrush bgBrush = new SolidBrush(Color.Black))
                    {
                        g.FillEllipse(bgBrush, 4, 4, 24, 24);
                    }

                    using (Pen whitePen = new Pen(Color.White, 3.5f))
                    {
                        whitePen.StartCap = LineCap.Round;
                        whitePen.EndCap = LineCap.Round;
                        g.DrawLine(whitePen, 11, 11, 21, 21);
                        g.DrawLine(whitePen, 21, 11, 11, 21);
                    }

                    IntPtr hIcon = bmp.GetHicon();
                    return new Cursor(hIcon);
                }
            }
            catch
            {
                return Cursors.No;
            }
        }

        private void SetupHeaderDragToHideAndColumnChooser()
        {
            ultraGridTransactions.AllowDrop = true;
            ultraGridTransactions.MouseDown += Grid_MouseDown;
            ultraGridTransactions.MouseMove += Grid_MouseMove;
            ultraGridTransactions.MouseUp += Grid_MouseUp;
            ultraGridTransactions.DragOver += Grid_DragOver;
            ultraGridTransactions.DragDrop += Grid_DragDrop;

            ContextMenuStrip headerMenu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            chooserItem.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            headerMenu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            headerMenu.Items.Add(showAllItem);

            ultraGridTransactions.ContextMenuStrip = headerMenu;
        }

        private void Grid_MouseDown(object sender, MouseEventArgs e)
        {
            if (ultraGridTransactions.DisplayLayout == null || ultraGridTransactions.DisplayLayout.Bands.Count == 0)
                return;

            UIElement element = ultraGridTransactions.DisplayLayout.UIElement?.ElementFromPoint(new Point(e.X, e.Y));
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            UltraGridColumn col = headerUI?.Header?.Column;
            if (headerUI != null && col != null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    ShowHeaderContextMenu(col, e.Location);
                    return;
                }

                if (e.Button == MouseButtons.Left)
                {
                    isDraggingHeaderToHide = true;
                    // Check if clicked the filter button (the image or rightmost 24 pixels of header)
                    ImageUIElement imgElem = element as ImageUIElement ?? element?.GetAncestor(typeof(ImageUIElement)) as ImageUIElement;
                    if (imgElem != null || e.X >= headerUI.Rect.Right - 24)
                    {
                        _suppressSorting = true;
                        isDraggingHeaderToHide = false;
                        columnBeingDragged = null;
                        ShowColumnFilterPopup(col, headerUI.Rect);
                        return;
                    }
                    columnBeingDragged = col;
                    headerDragStartPoint = new Point(e.X, e.Y);
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingHeaderToHide)
            {
                UIElement elem = ultraGridTransactions.DisplayLayout.UIElement?.ElementFromPoint(new Point(e.X, e.Y));
                HeaderUIElement hUI = elem as HeaderUIElement ?? elem?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
                if (hUI != null && hUI.Header?.Column != null)
                {
                    if (e.X >= hUI.Rect.Right - 24)
                    {
                        ultraGridTransactions.Cursor = Cursors.Hand;
                        string colCaption = !string.IsNullOrEmpty(hUI.Header.Column.Header.Caption) ? hUI.Header.Column.Header.Caption : hUI.Header.Column.Key;
                        headerToolTip.SetToolTip(ultraGridTransactions, $"Filter by '{colCaption}'");
                        return;
                    }
                    else
                    {
                        ultraGridTransactions.Cursor = Cursors.Default;
                        headerToolTip.SetToolTip(ultraGridTransactions, string.Empty);
                    }
                }
            }

            if (!isDraggingHeaderToHide || columnBeingDragged == null || e.Button != MouseButtons.Left)
                return;

            int deltaX = Math.Abs(e.X - headerDragStartPoint.X);
            int deltaY = e.Y - headerDragStartPoint.Y;

            if (deltaY > 20 && deltaY > deltaX)
            {
                ultraGridTransactions.Cursor = blackXCursor;
                string colName = !string.IsNullOrEmpty(columnBeingDragged.Header.Caption) ? columnBeingDragged.Header.Caption : columnBeingDragged.Key;
                headerToolTip.SetToolTip(ultraGridTransactions, $"✖ Drag down to hide '{colName}' column");
            }
            else
            {
                ultraGridTransactions.Cursor = Cursors.Default;
                headerToolTip.SetToolTip(ultraGridTransactions, string.Empty);
            }
        }

        private void Grid_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderToHide)
            {
                if (columnBeingDragged != null && (e.Y - headerDragStartPoint.Y) > 40)
                {
                    HideColumn(columnBeingDragged);
                }
                isDraggingHeaderToHide = false;
                columnBeingDragged = null;
                ultraGridTransactions.Cursor = Cursors.Default;
                headerToolTip.SetToolTip(ultraGridTransactions, string.Empty);
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnChooserItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void Grid_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnChooserItem)) is ColumnChooserItem item)
            {
                Point clientPt = ultraGridTransactions.PointToClient(new Point(e.X, e.Y));
                int dropPosition = GetTargetColumnPositionFromPoint(clientPt);
                UnhideColumn(item.ColumnKey, dropPosition);
            }
        }

        private int GetTargetColumnPositionFromPoint(Point pt)
        {
            if (ultraGridTransactions.DisplayLayout == null || ultraGridTransactions.DisplayLayout.Bands.Count == 0)
                return 0;

            UIElement element = ultraGridTransactions.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header?.Column != null)
            {
                return headerUI.Header.Column.Header.VisiblePosition;
            }

            UltraGridBand band = ultraGridTransactions.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (!col.Hidden)
                {
                    UIElement hUI = col.Header.GetUIElement();
                    if (hUI != null && pt.X >= hUI.Rect.Left && pt.X <= hUI.Rect.Right)
                    {
                        return col.Header.VisiblePosition;
                    }
                }
            }

            return band.Columns.Count;
        }

        private void HideColumn(UltraGridColumn col)
        {
            if (col == null) return;
            userHiddenColumnKeys.Add(col.Key);
            col.Hidden = true;
            ReportGridLayoutHelper.SaveLayout("ItemReport", ultraGridTransactions, userHiddenColumnKeys);
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private void ShowHeaderContextMenu(UltraGridColumn col, Point location)
        {
            if (col == null) return;
            ContextMenuStrip menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
            string colName = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;

            ToolStripMenuItem hideItem = new ToolStripMenuItem($"🙈 Hide Column '{colName}'", null, (s, e) => HideColumn(col));
            hideItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            menu.Items.Add(hideItem);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem filterItem = new ToolStripMenuItem($"🔍 Filter '{colName}'...", null, (s, e) => {
                var headerUI = col.Header.GetUIElement();
                Rectangle r = headerUI != null ? headerUI.Rect : new Rectangle(location.X, location.Y, 20, 20);
                ShowColumnFilterPopup(col, r);
            });
            filterItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            menu.Items.Add(filterItem);

            if (_activeColumnFilters.ContainsKey(col.Key))
            {
                ToolStripMenuItem clearColFilterItem = new ToolStripMenuItem($"❌ Clear Filter on '{colName}'", null, (s, e) => {
                    _activeColumnFilters.Remove(col.Key);
                    ApplyActiveColumnFilters();
                });
                menu.Items.Add(clearColFilterItem);
            }

            if (_activeColumnFilters.Count > 0)
            {
                ToolStripMenuItem clearAllFiltersItem = new ToolStripMenuItem("❌ Clear All Column Filters", null, (s, e) => {
                    _activeColumnFilters.Clear();
                    ApplyActiveColumnFilters();
                });
                menu.Items.Add(clearAllFiltersItem);
            }

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            menu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            menu.Items.Add(showAllItem);

            menu.Show(ultraGridTransactions, location);
        }

        private void ShowColumnChooserForm()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox();
            columnChooserForm.Show(this);
            PositionColumnChooser();
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization (Field Chooser)",
                Size = new Size(240, 300),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 244, 248),
                ShowIcon = false,
                ShowInTaskbar = false
            };

            columnChooserForm.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                columnChooserForm.Hide();
            };

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 244, 248),
                ItemHeight = 34,
                IntegralHeight = false
            };

            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;

            columnChooserForm.Controls.Add(columnChooserListBox);
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnChooserListBox != null)
            {
                int index = columnChooserListBox.IndexFromPoint(e.Location);
                if (index >= 0 && index < columnChooserListBox.Items.Count)
                {
                    if (columnChooserListBox.Items[index] is ColumnChooserItem item)
                    {
                        columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                    }
                }
            }
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || ultraGridTransactions.DisplayLayout.Bands.Count == 0)
                return;

            columnChooserListBox.Items.Clear();
            UltraGridBand band = ultraGridTransactions.DisplayLayout.Bands[0];

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Hidden && !col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                {
                    string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnChooserItem item)
            {
                UnhideColumn(item.ColumnKey);
            }
        }

        private void UnhideColumn(string columnKey, int? targetVisiblePosition = null)
        {
            userHiddenColumnKeys.Remove(columnKey);
            if (ultraGridTransactions.DisplayLayout.Bands.Count > 0 && ultraGridTransactions.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn col = ultraGridTransactions.DisplayLayout.Bands[0].Columns[columnKey];
                col.Hidden = false;
                if (targetVisiblePosition.HasValue)
                {
                    col.Header.VisiblePosition = targetVisiblePosition.Value;
                }
                ReportGridLayoutHelper.SaveLayout("ItemReport", ultraGridTransactions, userHiddenColumnKeys);
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                PopulateColumnChooserListBox();
            }
        }

        private void UnhideAllColumns()
        {
            userHiddenColumnKeys.Clear();
            if (ultraGridTransactions.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = ultraGridTransactions.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                if (!col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                {
                    col.Hidden = false;
                }
            }
            ReportGridLayoutHelper.SaveLayout("ItemReport", ultraGridTransactions, userHiddenColumnKeys);
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            PopulateColumnChooserListBox();
        }

        private void PositionColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(
                    Right - columnChooserForm.Width - 30,
                    Bottom - columnChooserForm.Height - 30);
                columnChooserForm.BringToFront();
            }
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || columnChooserListBox == null || e.Index >= columnChooserListBox.Items.Count)
                return;

            if (!(columnChooserListBox.Items[e.Index] is ColumnChooserItem item))
                return;

            Rectangle rect = e.Bounds;
            rect.Inflate(-4, -3);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(0, 121, 211)))
            using (GraphicsPath path = RoundedRect(rect, 4))
            {
                e.Graphics.FillPath(bgBrush, path);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };
                using (Font textFont = new Font("Segoe UI", 9F, FontStyle.Bold))
                {
                    e.Graphics.DrawString(item.DisplayText, textFont, textBrush, rect, sf);
                }
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);

            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private sealed class ColumnChooserItem
        {
            public string ColumnKey { get; }
            public string DisplayText { get; }

            public ColumnChooserItem(string key, string text)
            {
                ColumnKey = key;
                DisplayText = text;
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        #endregion

        #region GridFooterPanel Dynamic Alignment & Calculation

        private void CreateFooterCells()
        {
            if (gridFooterPanel == null) return;
            gridFooterPanel.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (ultraGridTransactions.DisplayLayout == null || ultraGridTransactions.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = ultraGridTransactions.DisplayLayout.Bands[0];
            int xOffset = ultraGridTransactions.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True
                ? ultraGridTransactions.DisplayLayout.Override.RowSelectorWidth
                : 0;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden)
                    continue;

                Label footerLabel = new Label
                {
                    Name = "footer_" + column.Key,
                    Text = string.Empty,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = GridHeaderBlue,
                    BorderStyle = BorderStyle.None,
                    AutoSize = false,
                    Width = column.Width,
                    Height = Math.Max(gridFooterPanel.Height - 2, 20),
                    Left = xOffset,
                    Top = 1,
                    Tag = Tuple.Create(column.Key, string.Empty),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0),
                    ContextMenuStrip = CreateFooterContextMenu(column.Key)
                };

                footerLabel.Paint += FooterLabel_Paint;
                gridFooterPanel.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;

                if (!_columnAggregations.ContainsKey(column.Key))
                {
                    _columnAggregations[column.Key] = "None";
                }

                xOffset += column.Width;
            }
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label lbl = sender as Label;
            if (lbl == null) return;

            using (Pen borderPen = new Pen(Color.FromArgb(118, 154, 198), 1))
            {
                e.Graphics.DrawLine(borderPen, lbl.Width - 1, 0, lbl.Width - 1, lbl.Height);
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Tag = columnKey;

            bool isNumeric = ultraGridTransactions.DisplayLayout.Bands.Count > 0 &&
                             ultraGridTransactions.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(ultraGridTransactions.DisplayLayout.Bands[0].Columns[columnKey]);

            ToolStripMenuItem itemSum = new ToolStripMenuItem("Sum");
            itemSum.Tag = "Sum";
            itemSum.Enabled = isNumeric;
            itemSum.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMin = new ToolStripMenuItem("Min");
            itemMin.Tag = "Min";
            itemMin.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMax = new ToolStripMenuItem("Max");
            itemMax.Tag = "Max";
            itemMax.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemCount = new ToolStripMenuItem("Count");
            itemCount.Tag = "Count";
            itemCount.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemAverage = new ToolStripMenuItem("Average");
            itemAverage.Tag = "Avg";
            itemAverage.Enabled = isNumeric;
            itemAverage.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemNone = new ToolStripMenuItem("None");
            itemNone.Tag = "None";
            itemNone.Click += FooterContextMenu_Click;

            menu.Items.Add(itemSum);
            menu.Items.Add(itemMin);
            menu.Items.Add(itemMax);
            menu.Items.Add(itemCount);
            menu.Items.Add(itemAverage);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemNone);

            menu.Opening += (sender, e) =>
            {
                string currentAggregation = _columnAggregations.ContainsKey(columnKey)
                    ? _columnAggregations[columnKey]
                    : "None";

                foreach (ToolStripItem menuItem in menu.Items)
                {
                    ToolStripMenuItem toolStripMenuItem = menuItem as ToolStripMenuItem;
                    if (toolStripMenuItem != null && toolStripMenuItem.Tag != null)
                    {
                        toolStripMenuItem.Checked = string.Equals(toolStripMenuItem.Tag.ToString(), currentAggregation, StringComparison.OrdinalIgnoreCase);
                    }
                }
            };

            return menu;
        }

        private bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null) return false;
            Type t = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float) ||
                   t == typeof(int) || t == typeof(long) || t == typeof(short);
        }

        private void FooterContextMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null)
                return;

            ContextMenuStrip menu = item.Owner as ContextMenuStrip;
            if (menu == null || menu.Tag == null || item.Tag == null)
                return;

            string columnKey = menu.Tag.ToString();
            string aggregation = item.Tag.ToString();

            _columnAggregations[columnKey] = aggregation;
            UpdateFooterValues();
        }

        private void UpdateFooterCellPositions()
        {
            if (ultraGridTransactions.DisplayLayout == null || ultraGridTransactions.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0 || gridFooterPanel == null)
                return;

            UltraGridBand band = ultraGridTransactions.DisplayLayout.Bands[0];
            int rowSelectorWidth = ultraGridTransactions.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True
                ? ultraGridTransactions.DisplayLayout.Override.RowSelectorWidth
                : 0;
            int scrollOffset = 0;
            if (ultraGridTransactions.ActiveColScrollRegion != null)
            {
                scrollOffset = ultraGridTransactions.ActiveColScrollRegion.Position;
            }

            int calculatedX = rowSelectorWidth - scrollOffset;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || !_footerLabels.ContainsKey(column.Key))
                    continue;

                Label footerLabel = _footerLabels[column.Key];
                var headerUI = column.Header.GetUIElement();
                int left, width;

                if (headerUI != null)
                {
                    left = headerUI.Rect.Left;
                    width = headerUI.Rect.Width;
                }
                else
                {
                    left = calculatedX;
                    width = column.Width;
                }

                calculatedX += column.Width;

                footerLabel.Left = left;
                footerLabel.Width = width;
                footerLabel.Top = 0;
                footerLabel.Height = gridFooterPanel.Height;
                footerLabel.Visible = (left + width > 0 && left < gridFooterPanel.Width);
                footerLabel.Invalidate();
            }
        }

        private void UpdateFooterValues()
        {
            if (_footerLabels.Count == 0)
                return;

            List<UltraGridRow> visibleRows = GetVisibleDataRows().ToList();
            foreach (KeyValuePair<string, Label> footerEntry in _footerLabels)
            {
                string columnKey = footerEntry.Key;
                Label footerLabel = footerEntry.Value;

                if (!_columnAggregations.ContainsKey(columnKey) ||
                    string.Equals(_columnAggregations[columnKey], "None", StringComparison.OrdinalIgnoreCase))
                {
                    footerLabel.Text = string.Empty;
                    footerLabel.Tag = Tuple.Create(columnKey, string.Empty);
                    footerLabel.Invalidate();
                    continue;
                }

                object result = CalculateAggregation(columnKey, _columnAggregations[columnKey], visibleRows);
                string displayValue = FormatAggregationResult(columnKey, _columnAggregations[columnKey], result);

                footerLabel.Text = displayValue;
                footerLabel.Tag = Tuple.Create(columnKey, displayValue);
                footerLabel.ForeColor = Color.White;
                footerLabel.Invalidate();
            }
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            if (ultraGridTransactions.Rows == null) yield break;
            foreach (UltraGridRow row in ultraGridTransactions.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut && !row.Hidden)
                    yield return row;
            }
        }

        private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
        {
            if (visibleRows == null || visibleRows.Count == 0)
                return null;

            switch (aggregation)
            {
                case "Sum":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue)
                        .Sum(value => value.Value);
                case "Min":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value)
                        .Where(HasCellValue)
                        .Cast<IComparable>()
                        .OrderBy(value => value)
                        .FirstOrDefault();
                case "Max":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value)
                        .Where(HasCellValue)
                        .Cast<IComparable>()
                        .OrderByDescending(value => value)
                        .FirstOrDefault();
                case "Count":
                    return visibleRows.Count(row => row.Cells.Exists(columnKey) && HasCellValue(row.Cells[columnKey].Value));
                case "Avg":
                    List<decimal> values = visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue)
                        .Select(value => value.Value)
                        .ToList();
                    return values.Count == 0 ? 0m : values.Average();
                default:
                    return null;
            }
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (string.Equals(aggregation, "None", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            if (result == null)
            {
                if (string.Equals(aggregation, "Count", StringComparison.OrdinalIgnoreCase))
                    return "0";
                if (string.Equals(aggregation, "Sum", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Avg", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Min", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Max", StringComparison.OrdinalIgnoreCase))
                    return "0.00";
                return string.Empty;
            }

            if (aggregation == "Count")
                return Convert.ToString(result);

            if (ultraGridTransactions.DisplayLayout != null &&
                ultraGridTransactions.DisplayLayout.Bands.Count > 0 &&
                ultraGridTransactions.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn column = ultraGridTransactions.DisplayLayout.Bands[0].Columns[columnKey];
                decimal? numericValue = GetNumericValue(result);
                if (numericValue.HasValue)
                {
                    if (!string.IsNullOrWhiteSpace(column.Format))
                        return numericValue.Value.ToString(column.Format);

                    return numericValue.Value.ToString("N2");
                }
            }

            return Convert.ToString(result);
        }

        private static decimal? GetNumericValue(object rawValue)
        {
            if (rawValue == null || rawValue == DBNull.Value)
                return null;

            if (rawValue is decimal decVal) return decVal;
            if (rawValue is double dblVal) return Convert.ToDecimal(dblVal);
            if (rawValue is float fltVal) return Convert.ToDecimal(fltVal);
            if (rawValue is int intVal) return intVal;
            if (rawValue is long longVal) return longVal;
            if (rawValue is short shortVal) return shortVal;

            return decimal.TryParse(Convert.ToString(rawValue), out decimal parsed) ? parsed : (decimal?)null;
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        #endregion

        private void UltraGridTransactions_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || col.Key == "IsBaseUnit")
                {
                    col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }
                else
                {
                    col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.False;
                }
            }

            // Add or configure "Sl No" Column at VisiblePosition = 0
            if (!band.Columns.Exists("SlNo"))
            {
                UltraGridColumn slCol = band.Columns.Add("SlNo", "Sl No");
                slCol.DataType = typeof(int);
                slCol.Header.Caption = "Sl No";
                slCol.Header.VisiblePosition = 0;
                slCol.Width = 55;
                slCol.CellAppearance.TextHAlign = HAlign.Center;
            }
            else
            {
                band.Columns["SlNo"].Header.Caption = "Sl No";
                band.Columns["SlNo"].Header.VisiblePosition = 0;
                band.Columns["SlNo"].Width = 55;
                band.Columns["SlNo"].CellAppearance.TextHAlign = HAlign.Center;
            }

            string[] defaultHiddenCols = new string[] { "BranchId", "UnitId", "RefId", "IsBaseUnit" };
            foreach (string col in defaultHiddenCols)
            {
                if (band.Columns.Exists(col))
                {
                    if (col == "BranchId") band.Columns[col].Header.Caption = "Branch ID";
                    if (col == "UnitId") band.Columns[col].Header.Caption = "Unit ID";
                    if (col == "RefId") band.Columns[col].Header.Caption = "Ref ID";
                    if (col == "IsBaseUnit") band.Columns[col].Header.Caption = "Base Unit";
                    band.Columns[col].Hidden = true;
                }
            }

            ConfigureColumn(band, "SlNo", "Sl No", 55, null, HAlign.Center, 0);
            ConfigureColumn(band, "DT", "Date", 90, "dd-MM-yyyy", HAlign.Center, 1);
            ConfigureColumn(band, "Operation", "Voucher Type", 110, null, HAlign.Left, 2);
            ConfigureColumn(band, "RefNo", "Ref / Bill No", 95, null, HAlign.Center, 3);
            ConfigureColumn(band, "Account", "Party / Ledger Account", 160, null, HAlign.Left, 4);
            ConfigureColumn(band, "Way", "Way", 60, null, HAlign.Center, 5);
            ConfigureColumn(band, "Qty", "Qty", 80, "#,##0.00", HAlign.Right, 6);
            ConfigureColumn(band, "UnitName", "Unit", 70, null, HAlign.Center, 7);
            ConfigureColumn(band, "Packing", "Packing", 70, "#,##0.##", HAlign.Center, 8);
            ConfigureColumn(band, "Cost", "Cost Price", 90, "₹ #,##0.00", HAlign.Right, 9);
            ConfigureColumn(band, "UnitPrice", "Sales Price", 90, "₹ #,##0.00", HAlign.Right, 10);
            ConfigureColumn(band, "Balance", "Stock Balance", 100, "#,##0.00", HAlign.Right, 11);
            ConfigureColumn(band, "BranchName", "Branch", 110, null, HAlign.Left, 12);

            ReportGridLayoutHelper.ApplySavedLayout("ItemReport", ultraGridTransactions, userHiddenColumnKeys);
            UpdateHeaderFilterIcons();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            e.Layout.ScrollBounds = ScrollBounds.ScrollToFill;
            e.Layout.Scrollbars = Scrollbars.Both;
        }

        private void ConfigureColumn(UltraGridBand band, string key, string header,
            int width, string format, HAlign align, int visPos)
        {
            if (!band.Columns.Exists(key)) return;

            UltraGridColumn col = band.Columns[key];
            col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.False;
            col.Header.Caption = header;
            col.Width = width;
            col.Header.VisiblePosition = visPos;
            col.Header.Appearance.BorderColor = Color.FromArgb(197, 217, 241);
            col.CellAppearance.BorderColor = Color.FromArgb(197, 217, 241);
            col.CellAppearance.TextHAlign = align;
            col.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            col.CellAppearance.FontData.SizeInPoints = 8.25F;
            if (!string.IsNullOrWhiteSpace(format))
                col.Format = format;

            col.Hidden = userHiddenColumnKeys.Contains(key);
        }

        private void UltraGridTransactions_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row.Cells.Exists("SlNo"))
            {
                e.Row.Cells["SlNo"].Value = e.Row.Index + 1;
            }

            if (e.Row.Cells.Exists("Way"))
            {
                string way = e.Row.Cells["Way"].Value?.ToString();
                if (string.Equals(way, "IN", StringComparison.OrdinalIgnoreCase))
                {
                    e.Row.Cells["Way"].Appearance.ForeColor = Color.FromArgb(46, 125, 50); // Green
                    e.Row.Cells["Way"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
                else if (string.Equals(way, "OUT", StringComparison.OrdinalIgnoreCase))
                {
                    e.Row.Cells["Way"].Appearance.ForeColor = Color.FromArgb(198, 40, 40); // Red
                    e.Row.Cells["Way"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }
        }

        private void ApplyGridStyling(UltraGrid targetGrid)
        {
            if (targetGrid == null) return;

            targetGrid.UseAppStyling = false;
            targetGrid.UseOsThemes = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Appearance.BackColor = FormBackColor;
            targetGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
            targetGrid.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            targetGrid.DisplayLayout.Scrollbars = Scrollbars.Both;
            targetGrid.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            targetGrid.DisplayLayout.GroupByBox.Hidden = true;
            targetGrid.DisplayLayout.GroupByBox.BorderStyle = UIElementBorderStyle.None;

            targetGrid.DisplayLayout.Override.HeaderStyle = HeaderStyle.Standard;
            targetGrid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            targetGrid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            targetGrid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            targetGrid.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            targetGrid.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

            targetGrid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
            targetGrid.DisplayLayout.Override.RowSelectorWidth = 25;
            targetGrid.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            targetGrid.DisplayLayout.Override.MinRowHeight = 24;
            targetGrid.DisplayLayout.Override.DefaultRowHeight = 24;
            targetGrid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.RowAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.RowAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = ControlTextColor;

            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BorderColor = BorderBlue;
            targetGrid.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            targetGrid.DisplayLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            targetGrid.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.CellAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.CellAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            targetGrid.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            targetGrid.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
        }

        private void StyleButtons()
        {
            StyleButton(btnSearch);
            StyleButton(btnExport);
            StyleButton(btnPrint);
            StyleButton(btnClose);
            StyleButton(btnHideSelection);

            SetupEnhancedSummaryPanel();
        }

        private static void StyleButton(Infragistics.Win.Misc.UltraButton button)
        {
            if (button == null) return;
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Office2013Button;
            button.Appearance.BackColor = ButtonTopColor;
            button.Appearance.BackColor2 = ButtonBottomColor;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.BorderColor = ButtonBorderColor;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.FontData.Name = "Microsoft Sans Serif";
            button.Appearance.FontData.SizeInPoints = 9F;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;

            button.HotTrackAppearance.BackColor = PanelHoverTopColor;
            button.HotTrackAppearance.BackColor2 = PanelHoverBottomColor;
            button.HotTrackAppearance.BorderColor = ButtonBorderColor;
            button.HotTrackAppearance.ForeColor = ButtonTextBlue;

            button.PressedAppearance.BackColor = PanelPressedTopColor;
            button.PressedAppearance.BackColor2 = PanelPressedBottomColor;
            button.PressedAppearance.BorderColor = ButtonBorderColor;
            button.PressedAppearance.ForeColor = ButtonTextBlue;
        }

        private void btnHideSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            btnHideSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "Show Selection";
            LayoutPanels();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutPanels();
        }

        private void LayoutPanels()
        {
            if (ultraPanelActionBar == null || ultraPanelGrid == null || ultraPanelSummary == null || ultraPanelControls == null) return;

            if (TopLevel == false)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Normal;
                Dock = DockStyle.Fill;
            }

            SuspendLayout();

            ultraPanelControls.Dock = DockStyle.Top;
            ultraPanelActionBar.Dock = DockStyle.Top;
            ultraPanelSummary.Dock = DockStyle.Bottom;
            ultraPanelSummary.Height = 72;
            ultraPanelSummary.Visible = true;
            ultraPanelGrid.Dock = DockStyle.Fill;

            if (!Controls.Contains(ultraPanelControls)) Controls.Add(ultraPanelControls);
            if (!Controls.Contains(ultraPanelActionBar)) Controls.Add(ultraPanelActionBar);
            if (!Controls.Contains(ultraPanelSummary)) Controls.Add(ultraPanelSummary);
            if (!Controls.Contains(ultraPanelGrid)) Controls.Add(ultraPanelGrid);

            Controls.SetChildIndex(ultraPanelControls, 3);
            Controls.SetChildIndex(ultraPanelActionBar, 2);
            Controls.SetChildIndex(ultraPanelSummary, 1);
            Controls.SetChildIndex(ultraPanelGrid, 0);

            if (ultraPanelGrid.ClientArea != null)
            {
                ultraPanelGrid.ClientArea.SuspendLayout();
                gridFooterPanel.Dock = DockStyle.Bottom;
                gridFooterPanel.Height = 28;
                ultraGridTransactions.Dock = DockStyle.Fill;

                if (!ultraPanelGrid.ClientArea.Controls.Contains(gridFooterPanel))
                    ultraPanelGrid.ClientArea.Controls.Add(gridFooterPanel);
                if (!ultraPanelGrid.ClientArea.Controls.Contains(ultraGridTransactions))
                    ultraPanelGrid.ClientArea.Controls.Add(ultraGridTransactions);

                ultraPanelGrid.ClientArea.Controls.SetChildIndex(gridFooterPanel, 1);
                ultraPanelGrid.ClientArea.Controls.SetChildIndex(ultraGridTransactions, 0);

                ultraPanelGrid.ClientArea.ResumeLayout(true);
                ultraPanelGrid.ClientArea.PerformLayout();
            }

            ResumeLayout(true);
            PerformLayout();

            AlignSummaryCards();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void AlignSummaryCards()
        {
            if (ultraPanelSummary == null || ultraPanelSummary.ClientArea == null) return;
            int totalWidth = ultraPanelSummary.ClientArea.Width;
            if (totalWidth <= 0) return;

            UltraLabel[] captions = new UltraLabel[]
            {
                ultraLabelSalesCaption, ultraLabelPurchaseCaption, ultraLabelReturnCaption, ultraLabelAdjustCaption,
                ultraLabelTotalInCaption, ultraLabelTotalOutCaption, ultraLabelCurrentStockCaption, ultraLabelStockValueCaption
            };

            UltraLabel[] values = new UltraLabel[]
            {
                ultraLabelSalesValue, ultraLabelPurchaseValue, ultraLabelReturnValue, ultraLabelAdjustValue,
                ultraLabelTotalInValue, ultraLabelTotalOutValue, ultraLabelCurrentStockValue, ultraLabelStockValueValue
            };

            int count = 8;
            int padding = 12;
            int baseCardWidth = 140;

            int availableWidth = totalWidth - (padding * 2);
            if (availableWidth <= 0) return;

            int gap = 10;
            int computedWidth = (availableWidth - (gap * (count - 1))) / count;
            int cardWidth = Math.Max(baseCardWidth, Math.Min(220, computedWidth));

            int remainingForGaps = availableWidth - (count * cardWidth);
            if (count > 1)
            {
                gap = Math.Max(4, remainingForGaps / (count - 1));
            }

            int currentX = padding;
            for (int i = 0; i < count; i++)
            {
                if (captions[i] != null)
                {
                    captions[i].Location = new Point(currentX, 2);
                    captions[i].Size = new Size(cardWidth, 16);
                }
                if (values[i] != null)
                {
                    values[i].Location = new Point(currentX, 18);
                    values[i].Size = new Size(cardWidth, 48);
                }
                currentX += cardWidth + gap;
            }
        }

        private void SetupEnhancedSummaryPanel()
        {
            if (ultraPanelSummary == null) return;

            ultraPanelSummary.Dock = DockStyle.Bottom;
            ultraPanelSummary.Height = 72;
            ultraPanelSummary.Visible = true;

            gridFooterPanel.Dock = DockStyle.Bottom;
            gridFooterPanel.Height = 28;
            ultraGridTransactions.Dock = DockStyle.Fill;

            ultraPanelSummary.ClientArea.AutoScroll = false;
            ultraPanelSummary.Resize += (s, e) => AlignSummaryCards();
        }

        private void frmItemReport_Load(object sender, EventArgs e)
        {
            LayoutPanels();
        }

        public void RibbonClear()
        {
            selectedItemId = 0;
            selectedItemName = string.Empty;
            if (txtItemName != null) txtItemName.Text = string.Empty;
            if (ultraGridTransactions != null) ultraGridTransactions.DataSource = null;
            _activeColumnFilters.Clear();
            CloseActiveFilterDropdown();
            UpdateHeaderFilterIcons();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        public void Clear() => RibbonClear();

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (selectedItemId <= 0)
            {
                MessageBox.Show("Please select an item first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                OpenItemSearchDialog();
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            try
            {
                int itemId = selectedItemId;
                int branchId = ultraComboBranch.Value != null ? Convert.ToInt32(ultraComboBranch.Value) : (int.TryParse(DataBase.BranchId, out int bId) ? bId : 0);

                int finYearId = !string.IsNullOrEmpty(DataBase.FinyearId) ? Convert.ToInt32(DataBase.FinyearId) : 1;
                int companyId = !string.IsNullOrEmpty(DataBase.CompanyId) ? Convert.ToInt32(DataBase.CompanyId) : 1;

                var reportData = itemReportRepo.GetItemReport(finYearId, companyId, branchId, itemId);

                if (reportData.Transactions != null && reportData.Transactions.Count > 0)
                {
                    string defaultUnit = reportData.PriceSettings?.FirstOrDefault(x => !string.IsNullOrEmpty(x.UnitName))?.UnitName;
                    if (string.IsNullOrWhiteSpace(defaultUnit)) defaultUnit = "UNIT";

                    decimal runningBalance = 0;
                    foreach (var t in reportData.Transactions)
                    {
                        if (string.IsNullOrWhiteSpace(t.UnitName))
                        {
                            t.UnitName = defaultUnit;
                        }

                        decimal packing = t.Packing > 0 ? t.Packing : 1;
                        decimal baseQty = t.Qty * packing;

                        if (string.Equals(t.Way, "IN", StringComparison.OrdinalIgnoreCase))
                        {
                            runningBalance += baseQty;
                        }
                        else if (string.Equals(t.Way, "OUT", StringComparison.OrdinalIgnoreCase))
                        {
                            runningBalance -= baseQty;
                        }

                        t.Balance = runningBalance;
                    }
                }

                _activeColumnFilters.Clear();
                ultraGridTransactions.DataSource = reportData.Transactions;
                UpdateHeaderFilterIcons();
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();

                if (reportData.Transactions != null && reportData.Transactions.Count > 0)
                {
                    Func<ItemTransactionModel, decimal> getBaseQty = x => x.Qty * (x.Packing > 0 ? x.Packing : 1);
                    Func<ItemTransactionModel, decimal> getUnitCost = x => x.Packing > 1 ? (x.Cost / x.Packing) : x.Cost;

                    var salesTxns = reportData.Transactions.Where(x => x.Operation.Equals("Sales", StringComparison.OrdinalIgnoreCase));
                    decimal salesQty = salesTxns.Sum(getBaseQty);
                    decimal salesAmt = salesTxns.Sum(x => x.Qty * x.UnitPrice);

                    var purchaseTxns = reportData.Transactions.Where(x => x.Operation.Equals("Purchase", StringComparison.OrdinalIgnoreCase));
                    decimal purchaseQty = purchaseTxns.Sum(getBaseQty);
                    decimal purchaseAmt = purchaseTxns.Sum(x => x.Qty * x.Cost);

                    var returnTxns = reportData.Transactions.Where(x => x.Operation.StartsWith("Sales Return", StringComparison.OrdinalIgnoreCase) || x.Operation.StartsWith("Return", StringComparison.OrdinalIgnoreCase));
                    decimal returnQty = returnTxns.Sum(getBaseQty);
                    decimal returnAmt = returnTxns.Sum(x => x.Qty * (x.UnitPrice > 0 ? x.UnitPrice : x.Cost));

                    var adjustTxns = reportData.Transactions.Where(x => x.Operation.StartsWith("Stock Adjust", StringComparison.OrdinalIgnoreCase) || x.Operation.StartsWith("Adjustment", StringComparison.OrdinalIgnoreCase));
                    decimal adjustQty = adjustTxns.Sum(getBaseQty);

                    decimal totalIn = reportData.Transactions.Where(x => x.Way.Equals("IN", StringComparison.OrdinalIgnoreCase)).Sum(getBaseQty);
                    decimal totalOut = reportData.Transactions.Where(x => x.Way.Equals("OUT", StringComparison.OrdinalIgnoreCase)).Sum(getBaseQty);

                    decimal currentStock = reportData.StockSummary != null && reportData.StockSummary.Count > 0
                        ? reportData.StockSummary.Sum(x => x.Stock)
                        : (totalIn - totalOut);

                    decimal latestCost = reportData.Transactions
                        .Where(x => x.Cost > 0)
                        .Select(getUnitCost)
                        .LastOrDefault();

                    if (latestCost == 0 && reportData.PriceSettings != null && reportData.PriceSettings.Count > 0)
                    {
                        var ps = reportData.PriceSettings.FirstOrDefault(x => x.Cost > 0);
                        if (ps != null)
                        {
                            latestCost = ps.Cost;
                        }
                    }

                    decimal stockValue = currentStock * latestCost;

                    if (ultraLabelSalesValue != null) ultraLabelSalesValue.Text = $"{salesQty:N2} Qty\n₹ {salesAmt:N2}";
                    if (ultraLabelPurchaseValue != null) ultraLabelPurchaseValue.Text = $"{purchaseQty:N2} Qty\n₹ {purchaseAmt:N2}";
                    if (ultraLabelReturnValue != null) ultraLabelReturnValue.Text = $"{returnQty:N2} Qty\n₹ {returnAmt:N2}";
                    if (ultraLabelAdjustValue != null) ultraLabelAdjustValue.Text = $"{adjustQty:N2} Qty";

                    if (ultraLabelTotalInValue != null) ultraLabelTotalInValue.Text = $"{totalIn:N2} Qty";
                    if (ultraLabelTotalOutValue != null) ultraLabelTotalOutValue.Text = $"{totalOut:N2} Qty";
                    if (ultraLabelCurrentStockValue != null) ultraLabelCurrentStockValue.Text = $"{currentStock:N2} Qty";
                    if (ultraLabelStockValueValue != null) ultraLabelStockValueValue.Text = $"₹ {stockValue:N2}";
                }
                else
                {
                    if (ultraLabelSalesValue != null) ultraLabelSalesValue.Text = "0.00 Qty\n₹ 0.00";
                    if (ultraLabelPurchaseValue != null) ultraLabelPurchaseValue.Text = "0.00 Qty\n₹ 0.00";
                    if (ultraLabelReturnValue != null) ultraLabelReturnValue.Text = "0.00 Qty\n₹ 0.00";
                    if (ultraLabelAdjustValue != null) ultraLabelAdjustValue.Text = "0.00 Qty";

                    if (ultraLabelTotalInValue != null) ultraLabelTotalInValue.Text = "0.00 Qty";
                    if (ultraLabelTotalOutValue != null) ultraLabelTotalOutValue.Text = "0.00 Qty";
                    if (ultraLabelCurrentStockValue != null) ultraLabelCurrentStockValue.Text = "0.00 Qty";
                    if (ultraLabelStockValueValue != null) ultraLabelStockValueValue.Text = "₹ 0.00";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = ReportPdfExporter.CsvAndPdfFilter,
                    Title = "Save Report"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (ReportPdfExporter.CheckAndExportPdf(saveFileDialog, ultraGridTransactions, "Item Movement Report", $"Item: {selectedItemName}")) return;
                    if (ultraGridTransactions.Rows.Count > 0)
                    {
                        ExportToCSV(ultraGridTransactions, saveFileDialog.FileName);
                        MessageBox.Show("Export successful.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("No data to export.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(UltraGrid grid, string fileName)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var col in grid.DisplayLayout.Bands[0].Columns)
            {
                if (!col.Hidden)
                    sb.Append(col.Header.Caption + ",");
            }
            if (sb.Length > 0) sb.Length--;
            sb.AppendLine();

            foreach (var row in grid.Rows)
            {
                if (row.Hidden) continue;
                foreach (var col in grid.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        string value = row.Cells[col].Value?.ToString() ?? "";
                        if (value.Contains(",")) value = "\"" + value + "\"";
                        sb.Append(value + ",");
                    }
                }
                if (sb.Length > 0) sb.Length--;
                sb.AppendLine();
            }

            File.WriteAllText(fileName, sb.ToString());
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            ultraGridTransactions.PrintPreview();
        }

        #region Column Header Dropdown Filtering & Popup Window
        private static Bitmap CreateFilterIcon(bool isActive)
        {
            Bitmap bmp = new Bitmap(18, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);

                Rectangle btnRect = new Rectangle(0, 0, 17, 15);

                if (isActive)
                {
                    // Active filter: Golden / amber highlight badge
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(255, 179, 0)))
                    using (Pen borderPen = new Pen(Color.FromArgb(180, 120, 0), 1f))
                    using (GraphicsPath path = RoundedRect(btnRect, 3))
                    {
                        g.FillPath(bgBrush, path);
                        g.DrawPath(borderPen, path);
                    }

                    // Dark navy funnel
                    using (SolidBrush funnelBrush = new SolidBrush(Color.FromArgb(15, 35, 75)))
                    {
                        PointF[] funnel = new PointF[]
                        {
                            new PointF(3.5f, 3.5f),
                            new PointF(10.5f, 3.5f),
                            new PointF(7.5f, 7.5f),
                            new PointF(7.5f, 11.5f),
                            new PointF(6.5f, 10.5f),
                            new PointF(6.5f, 7.5f)
                        };
                        g.FillPolygon(funnelBrush, funnel);
                    }

                    // Small indicator dot
                    using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb(15, 35, 75)))
                    {
                        g.FillEllipse(dotBrush, 11.5f, 8.5f, 3.5f, 3.5f);
                    }
                }
                else
                {
                    // Inactive filter: Subtle light button outline
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
                    using (Pen borderPen = new Pen(Color.FromArgb(120, 255, 255, 255), 1f))
                    using (GraphicsPath path = RoundedRect(btnRect, 3))
                    {
                        g.FillPath(bgBrush, path);
                        g.DrawPath(borderPen, path);
                    }

                    // Crisp white funnel
                    using (SolidBrush funnelBrush = new SolidBrush(Color.White))
                    {
                        PointF[] funnel = new PointF[]
                        {
                            new PointF(3.0f, 3.5f),
                            new PointF(10.0f, 3.5f),
                            new PointF(7.0f, 7.5f),
                            new PointF(7.0f, 11.5f),
                            new PointF(6.0f, 10.5f),
                            new PointF(6.0f, 7.5f)
                        };
                        g.FillPolygon(funnelBrush, funnel);

                        // Down arrow
                        PointF[] arrow = new PointF[]
                        {
                            new PointF(11.5f, 6.0f),
                            new PointF(15.5f, 6.0f),
                            new PointF(13.5f, 8.5f)
                        };
                        g.FillPolygon(funnelBrush, arrow);
                    }
                }
            }
            return bmp;
        }

        private void UpdateHeaderFilterIcons()
        {
            if (ultraGridTransactions.DisplayLayout == null || ultraGridTransactions.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = ultraGridTransactions.DisplayLayout.Bands[0];

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Hidden || col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || col.Key == "SlNo") continue;

                bool isFiltered = _activeColumnFilters.ContainsKey(col.Key);
                col.Header.Appearance.Image = isFiltered ? _filterIconActive : _filterIconNormal;
                col.Header.Appearance.ImageHAlign = HAlign.Right;
                col.Header.Appearance.ImageVAlign = VAlign.Middle;
            }
        }

        private void CloseActiveFilterDropdown()
        {
            if (_activeDropdownForm != null && !_activeDropdownForm.IsDisposed)
            {
                _activeDropdownForm.Close();
                _activeDropdownForm = null;
            }
        }

        private void ShowColumnFilterPopup(UltraGridColumn col, Rectangle headerRect)
        {
            if (col == null || ultraGridTransactions.Rows == null || ultraGridTransactions.Rows.Count == 0)
            {
                MessageBox.Show("No data loaded in the grid to filter.", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_activeDropdownForm != null && !_activeDropdownForm.IsDisposed)
            {
                _activeDropdownForm.Close();
                _activeDropdownForm = null;
            }

            if (string.Equals(_lastClosedColumnKey, col.Key, StringComparison.OrdinalIgnoreCase) && 
                (DateTime.Now - _lastDropdownCloseTime).TotalMilliseconds < 250)
            {
                _lastClosedColumnKey = null;
                return;
            }

            // Collect distinct values for this column
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> allValues = new List<string>();

            foreach (UltraGridRow row in ultraGridTransactions.Rows)
            {
                if (!row.IsDataRow) continue;
                string text = row.Cells.Exists(col.Key) ? (row.Cells[col.Key].Text?.Trim() ?? "") : "";
                if (string.IsNullOrEmpty(text)) text = "(Blanks)";
                if (seen.Add(text))
                {
                    allValues.Add(text);
                }
            }

            // Sort values nicely
            allValues.Sort((a, b) =>
            {
                if (a == "(Blanks)") return 1;
                if (b == "(Blanks)") return -1;
                if (decimal.TryParse(a, out decimal da) && decimal.TryParse(b, out decimal db))
                    return da.CompareTo(db);
                return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
            });

            // Currently active selection
            HashSet<string> currentSelected = null;
            if (_activeColumnFilters.ContainsKey(col.Key))
            {
                currentSelected = _activeColumnFilters[col.Key];
            }

            string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;

            int dropWidth = Math.Max(230, Math.Min(Math.Max(headerRect.Width, 230), 300));
            int itemsHeight = Math.Min(Math.Max(allValues.Count * 22, 44), 190);
            int dropHeight = 90 + itemsHeight;
            var popup = new ColumnFilterDropdownForm(col.Key, caption, allValues, currentSelected, dropWidth, dropHeight, (newSelected) =>
            {
                if (newSelected == null || newSelected.Count == allValues.Count)
                {
                    _activeColumnFilters.Remove(col.Key);
                }
                else
                {
                    _activeColumnFilters[col.Key] = new HashSet<string>(newSelected, StringComparer.OrdinalIgnoreCase);
                }

                ApplyActiveColumnFilters();
            });

            // Position popup right below header
            Point screenPt = ultraGridTransactions.PointToScreen(new Point(headerRect.Left, headerRect.Bottom));
            Rectangle screenArea = Screen.FromControl(ultraGridTransactions).WorkingArea;

            if (screenPt.X + dropWidth > screenArea.Right)
                screenPt.X = screenArea.Right - dropWidth - 6;
            if (screenPt.X < screenArea.Left)
                screenPt.X = screenArea.Left + 6;

            if (screenPt.Y + dropHeight > screenArea.Bottom)
                screenPt.Y = screenPt.Y - headerRect.Height - dropHeight;

            popup.Location = screenPt;
            _activeDropdownForm = popup;
            popup.FormClosed += (s, e) =>
            {
                _lastClosedColumnKey = col.Key;
                _lastDropdownCloseTime = DateTime.Now;
                if (_activeDropdownForm == popup)
                    _activeDropdownForm = null;
            };
            popup.Show(this);
        }

        private void ApplyActiveColumnFilters()
        {
            ultraGridTransactions.BeginUpdate();
            try
            {
                bool hasFilters = _activeColumnFilters.Count > 0;

                foreach (UltraGridRow row in ultraGridTransactions.Rows)
                {
                    if (!row.IsDataRow) continue;

                    if (!hasFilters)
                    {
                        row.Hidden = false;
                        continue;
                    }

                    bool visible = true;
                    foreach (var kvp in _activeColumnFilters)
                    {
                        string colKey = kvp.Key;
                        HashSet<string> allowed = kvp.Value;

                        string cellText = row.Cells.Exists(colKey) ? (row.Cells[colKey].Text?.Trim() ?? "") : "";
                        if (string.IsNullOrEmpty(cellText)) cellText = "(Blanks)";

                        if (!allowed.Contains(cellText))
                        {
                            visible = false;
                            break;
                        }
                    }

                    row.Hidden = !visible;
                }
            }
            finally
            {
                ultraGridTransactions.EndUpdate();
            }

            UpdateHeaderFilterIcons();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private sealed class ColumnFilterDropdownForm : Form
        {
            private readonly string _columnKey;
            private readonly string _columnTitle;
            private readonly List<string> _allValues;
            private readonly HashSet<string> _selectedValues;
            private readonly Action<HashSet<string>> _onApply;

            private Panel pnlHeader;
            private Label lblTitle;
            private Label btnCloseX;
            private Panel pnlSearch;
            private TextBox txtSearch;
            private Panel pnlSelectAll;
            private CheckBox chkSelectAll;
            private CheckedListBox clbItems;
            private Panel pnlBottom;
            private Button btnOK;
            private Button btnCancel;
            private Button btnClear;

            private List<string> _visibleValues = new List<string>();
            private bool _isUpdatingCheckState = false;

            protected override CreateParams CreateParams
            {
                get
                {
                    const int CS_DROPSHADOW = 0x00020000;
                    CreateParams cp = base.CreateParams;
                    cp.ClassStyle |= CS_DROPSHADOW;
                    return cp;
                }
            }

            public ColumnFilterDropdownForm(string columnKey, string columnTitle, List<string> allValues, HashSet<string> currentSelected, int width, int height, Action<HashSet<string>> onApply)
            {
                _columnKey = columnKey;
                _columnTitle = columnTitle;
                _allValues = allValues ?? new List<string>();
                _selectedValues = currentSelected != null 
                    ? new HashSet<string>(currentSelected, StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string>(_allValues, StringComparer.OrdinalIgnoreCase);
                _onApply = onApply;

                InitializePopupUi(width, height);
                PopulateItems(string.Empty);
            }

            private void InitializePopupUi(int width, int height)
            {
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                TopMost = true;
                Size = new Size(width, height);
                BackColor = Color.White;
                KeyPreview = true;
                DoubleBuffered = true;

                // ── Search Panel ──
                pnlSearch = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 28,
                    BackColor = Color.FromArgb(240, 244, 248),
                    Padding = new Padding(4, 3, 4, 3)
                };

                txtSearch = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F),
                    BorderStyle = BorderStyle.FixedSingle
                };
                txtSearch.TextChanged += (s, e) => PopulateItems(txtSearch.Text);

                Label lblSearchIcon = new Label
                {
                    Text = "🔍",
                    Dock = DockStyle.Left,
                    Width = 20,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(120, 140, 160),
                    Font = new Font("Segoe UI", 8F)
                };
                pnlSearch.Controls.Add(txtSearch);
                pnlSearch.Controls.Add(lblSearchIcon);

                txtSearch.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape) { Close(); e.Handled = true; }
                    else if (e.KeyCode == Keys.Enter) { ApplyAndClose(); e.Handled = true; }
                    else if (e.KeyCode == Keys.Down && clbItems.Items.Count > 0) { clbItems.Focus(); e.Handled = true; }
                };

                // ── Select All Panel ──
                pnlSelectAll = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 28,
                    BackColor = Color.FromArgb(248, 250, 252),
                    Padding = new Padding(6, 2, 6, 2)
                };

                chkSelectAll = new CheckBox
                {
                    Text = "(Select All)",
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 35, 75),
                    ThreeState = true
                };
                chkSelectAll.Click += ChkSelectAll_Click;
                pnlSelectAll.Controls.Add(chkSelectAll);

                // ── Bottom Buttons Panel ──
                pnlBottom = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 34,
                    BackColor = Color.FromArgb(246, 248, 252),
                    Padding = new Padding(4, 4, 4, 4)
                };

                btnOK = new Button
                {
                    Text = "OK",
                    Size = new Size(54, 24),
                    Location = new Point(5, 5),
                    BackColor = Color.FromArgb(67, 118, 184),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnOK.FlatAppearance.BorderSize = 0;
                btnOK.MouseEnter += (s, e) => { btnOK.BackColor = Color.FromArgb(50, 95, 155); };
                btnOK.MouseLeave += (s, e) => { btnOK.BackColor = Color.FromArgb(67, 118, 184); };
                btnOK.Click += (s, e) => ApplyAndClose();

                btnCancel = new Button
                {
                    Text = "Cancel",
                    Size = new Size(54, 24),
                    Location = new Point(64, 5),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(30, 41, 59),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F),
                    Cursor = Cursors.Hand
                };
                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 220);
                btnCancel.MouseEnter += (s, e) => { btnCancel.BackColor = Color.FromArgb(241, 245, 249); };
                btnCancel.MouseLeave += (s, e) => { btnCancel.BackColor = Color.White; };
                btnCancel.Click += (s, e) => Close();

                btnClear = new Button
                {
                    Text = "Clear",
                    Size = new Size(54, 24),
                    Location = new Point(124, 5),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(180, 40, 40),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8.5F),
                    Cursor = Cursors.Hand
                };
                btnClear.FlatAppearance.BorderColor = Color.FromArgb(210, 160, 160);
                btnClear.MouseEnter += (s, e) => { btnClear.BackColor = Color.FromArgb(254, 242, 242); };
                btnClear.MouseLeave += (s, e) => { btnClear.BackColor = Color.White; };
                btnClear.Click += (s, e) => ClearAndClose();

                pnlBottom.Controls.Add(btnOK);
                pnlBottom.Controls.Add(btnCancel);
                pnlBottom.Controls.Add(btnClear);

                // ── CheckedListBox ──
                clbItems = new CheckedListBox
                {
                    Dock = DockStyle.Fill,
                    CheckOnClick = true,
                    BorderStyle = BorderStyle.None,
                    Font = new Font("Segoe UI", 9F),
                    BackColor = Color.White,
                    IntegralHeight = false
                };
                clbItems.ItemCheck += ClbItems_ItemCheck;
                clbItems.DoubleClick += (s, e) =>
                {
                    int index = clbItems.SelectedIndex;
                    if (index >= 0 && index < _visibleValues.Count)
                    {
                        string val = _visibleValues[index];
                        _selectedValues.Clear();
                        _selectedValues.Add(val);
                        ApplyAndClose();
                    }
                };

                Controls.Add(clbItems);
                Controls.Add(pnlSelectAll);
                Controls.Add(pnlSearch);
                Controls.Add(pnlBottom);

                Deactivate += (s, e) => { if (!IsDisposed) Close(); };
                KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape) Close();
                    else if (e.KeyCode == Keys.Enter && !txtSearch.Focused) ApplyAndClose();
                };
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                using (Pen p = new Pen(Color.FromArgb(160, 178, 200), 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                }
            }

            private void PopulateItems(string filterText)
            {
                _isUpdatingCheckState = true;
                clbItems.BeginUpdate();
                try
                {
                    clbItems.Items.Clear();
                    _visibleValues.Clear();

                    string query = (filterText ?? "").Trim();
                    bool hasQuery = !string.IsNullOrEmpty(query);

                    foreach (string val in _allValues)
                    {
                        if (!hasQuery || val.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _visibleValues.Add(val);
                            bool isChecked = _selectedValues.Contains(val);
                            clbItems.Items.Add(val, isChecked);
                        }
                    }
                }
                finally
                {
                    clbItems.EndUpdate();
                    _isUpdatingCheckState = false;
                }

                UpdateSelectAllState();
            }

            private void ClbItems_ItemCheck(object sender, ItemCheckEventArgs e)
            {
                if (_isUpdatingCheckState) return;

                if (e.Index >= 0 && e.Index < _visibleValues.Count)
                {
                    string itemValue = _visibleValues[e.Index];
                    if (e.NewValue == CheckState.Checked)
                    {
                        _selectedValues.Add(itemValue);
                    }
                    else
                    {
                        _selectedValues.Remove(itemValue);
                    }
                }

                BeginInvoke(new Action(UpdateSelectAllState));
            }

            private void UpdateSelectAllState()
            {
                if (_visibleValues.Count == 0)
                {
                    chkSelectAll.CheckState = CheckState.Unchecked;
                    return;
                }

                int checkedCount = 0;
                foreach (string val in _visibleValues)
                {
                    if (_selectedValues.Contains(val))
                        checkedCount++;
                }

                _isUpdatingCheckState = true;
                try
                {
                    if (checkedCount == _visibleValues.Count)
                    {
                        chkSelectAll.CheckState = CheckState.Checked;
                    }
                    else if (checkedCount == 0)
                    {
                        chkSelectAll.CheckState = CheckState.Unchecked;
                    }
                    else
                    {
                        chkSelectAll.CheckState = CheckState.Indeterminate;
                    }
                }
                finally
                {
                    _isUpdatingCheckState = false;
                }
            }

            private void ChkSelectAll_Click(object sender, EventArgs e)
            {
                bool targetState = (chkSelectAll.CheckState == CheckState.Checked);

                _isUpdatingCheckState = true;
                clbItems.BeginUpdate();
                try
                {
                    for (int i = 0; i < _visibleValues.Count; i++)
                    {
                        string val = _visibleValues[i];
                        if (targetState)
                        {
                            _selectedValues.Add(val);
                            clbItems.SetItemChecked(i, true);
                        }
                        else
                        {
                            _selectedValues.Remove(val);
                            clbItems.SetItemChecked(i, false);
                        }
                    }
                }
                finally
                {
                    clbItems.EndUpdate();
                    _isUpdatingCheckState = false;
                }
            }

            private void ApplyAndClose()
            {
                _onApply?.Invoke(_selectedValues);
                Close();
            }

            private void ClearAndClose()
            {
                _onApply?.Invoke(null);
                Close();
            }
        }
        #endregion

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
