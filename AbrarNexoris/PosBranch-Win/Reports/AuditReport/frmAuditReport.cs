using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using ModelClass.Report;
using Repository;
using Repository.MasterRepositry;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.AuditReport
{
    public partial class frmAuditReport : Form
    {
        // ─── Theme Palette (matches frmStockReport / frmItemReport / frmStockReportAdvanced) ────────
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

        private AuditTrailReportRepository _repository;
        private Dropdowns _dropdowns;
        private ItemMasterRepository _itemRepository;
        private readonly List<AuditTrailItem> _items;
        private readonly List<ComboItem> _groupOptions;
        private readonly List<ComboItem> _categoryOptions;
        private readonly List<ComboItem> _brandOptions;
        private readonly List<ComboItem> _modelOptions;
        private readonly List<ComboItem> _itemOptions;

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

        private sealed class ComboItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
            public string ParentValue { get; set; }
        }

        public frmAuditReport()
        {
            _items = new List<AuditTrailItem>();
            _groupOptions = new List<ComboItem>();
            _categoryOptions = new List<ComboItem>();
            _brandOptions = new List<ComboItem>();
            _modelOptions = new List<ComboItem>();
            _itemOptions = new List<ComboItem>();

            InitializeComponent();
            Load += FrmAuditReport_Load;
            FormClosed += FrmAuditReport_FormClosed;

            // Register footer cell sync and column drag-to-hide handlers matching frmPurchaseReturn / frmStockReport
            gridAudit.Resize += (s, e) => UpdateFooterCellPositions();
            gridAudit.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
            gridAudit.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridAudit.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridAudit.Paint += (s, e) => UpdateFooterCellPositions();
            gridAudit.AfterRowFilterChanged += (s, e) =>
            {
                UpdateFooterValues();
                UpdateFooterCellPositions();
            };
            gridAudit.AfterSortChange += (s, e) => UpdateFooterValues();

            SetupHeaderDragToHideAndColumnChooser();
            InitializeGridFooter();
        }

        private void FrmAuditReport_Load(object sender, EventArgs e)
        {
            if (IsDesignTime())
            {
                return;
            }

            InitializeRuntimeAppearance();
            LoadLookupData();
            ResetFilters(false);
            LoadData();
        }

        private void BtnViewGrid_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void CmbItemNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadData();
                e.Handled = true;
            }
        }

        private void BtnPreviewGrid_Click(object sender, EventArgs e)
        {
            LoadData();
            ShowGridPreview("Inventory Audit Trail - Grid Preview");
        }

        private void BtnPreviewReport_Click(object sender, EventArgs e)
        {
            LoadData();
            ShowGridPreview("Inventory Audit Trail - Report Preview");
        }

        private void BtnHideSelection_Click(object sender, EventArgs e)
        {
            ultraPanelSelection.Visible = !ultraPanelSelection.Visible;
            btnHideSelection.Text = ultraPanelSelection.Visible ? "Hide Selection" : "Show Selection";
        }

        private void CmbDatePreset_ValueChanged(object sender, EventArgs e)
        {
            ApplyDatePreset();
        }

        private void CmbGroup_ValueChanged(object sender, EventArgs e)
        {
            ApplyCategoryOptions();
        }

        private void FrmAuditReport_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Close();
                columnChooserForm = null;
            }
        }

        private void InitializeRuntimeAppearance()
        {
            BackColor = FormBackColor;

            if (ultraPanelSelection != null)
            {
                ultraPanelSelection.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelSelection.Appearance.BorderColor = BorderBlue;
                ultraPanelSelection.BorderStyle = UIElementBorderStyle.Solid;
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

            StyleLabel(lblItemNo);
            StyleLabel(lblDate);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblCategory);
            StyleLabel(lblGroup);
            StyleLabel(lblLocation);
            StyleLabel(lblBrand);
            StyleLabel(lblModel);
            StyleLabel(lblMoreOptions);

            StyleFilterCombo(cmbItemNo);
            StyleFilterCombo(cmbDatePreset);
            StyleDateTimeEditor(dtFromDate);
            StyleDateTimeEditor(dtToDate);
            StyleFilterCombo(cmbCategory);
            StyleFilterCombo(cmbGroup);
            StyleFilterCombo(cmbLocation);
            StyleFilterCombo(cmbBrand);
            StyleFilterCombo(cmbModel);
            StyleFilterCombo(cmbAction);

            ConfigureButton(btnViewGrid);
            ConfigureButton(btnPreviewGrid);
            ConfigureButton(btnPreviewReport);
            ConfigureButton(btnHideSelection);

            ConfigureGridAppearance(gridAudit);
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

        private static void StyleDateTimeEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtEditor)
        {
            if (dtEditor == null) return;
            dtEditor.UseAppStyling = false;
            dtEditor.UseOsThemes = DefaultableBoolean.False;
            dtEditor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            dtEditor.BorderStyle = UIElementBorderStyle.Solid;
            dtEditor.Appearance.BackColor = ControlBackColor;
            dtEditor.Appearance.BorderColor = SkyBlueOutline;
            dtEditor.Appearance.ForeColor = ControlTextColor;
            dtEditor.Appearance.FontData.Name = "Microsoft Sans Serif";
            dtEditor.Appearance.FontData.SizeInPoints = 9F;
            dtEditor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void ConfigureButton(Infragistics.Win.Misc.UltraButton btn)
        {
            if (btn == null) return;
            btn.UseAppStyling = false;
            btn.UseOsThemes = DefaultableBoolean.False;
            btn.ButtonStyle = UIElementButtonStyle.Office2013Button;
            btn.Appearance.BackColor = ButtonTopColor;
            btn.Appearance.BackColor2 = ButtonBottomColor;
            btn.Appearance.BackGradientStyle = GradientStyle.Vertical;
            btn.Appearance.BorderColor = ButtonBorderColor;
            btn.Appearance.ForeColor = ButtonTextBlue;
            btn.Appearance.FontData.Bold = DefaultableBoolean.True;
            btn.Appearance.FontData.Name = "Microsoft Sans Serif";
            btn.Appearance.FontData.SizeInPoints = 8.25F;
            btn.HotTrackAppearance.BackColor = PanelHoverTopColor;
            btn.HotTrackAppearance.BackColor2 = PanelHoverBottomColor;
            btn.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            btn.HotTrackAppearance.BorderColor = ButtonBorderColor;
            btn.HotTrackAppearance.ForeColor = ButtonTextBlue;
            btn.PressedAppearance.BackColor = PanelPressedTopColor;
            btn.PressedAppearance.BackColor2 = PanelPressedBottomColor;
            btn.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            btn.PressedAppearance.BorderColor = ButtonBorderColor;
            btn.PressedAppearance.ForeColor = ButtonTextBlue;
        }

        private void ConfigureGridAppearance(UltraGrid targetGrid)
        {
            if (targetGrid == null) return;
            targetGrid.UseAppStyling = false;
            targetGrid.UseOsThemes = DefaultableBoolean.False;
            targetGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
            targetGrid.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            targetGrid.DisplayLayout.Scrollbars = Scrollbars.Both;
            targetGrid.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            targetGrid.DisplayLayout.GroupByBox.Hidden = true;
            targetGrid.DisplayLayout.GroupByBox.BorderStyle = UIElementBorderStyle.None;
            targetGrid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            targetGrid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            targetGrid.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            targetGrid.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.FilterUIType = FilterUIType.HeaderIcons;
            targetGrid.DisplayLayout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            targetGrid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            targetGrid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            targetGrid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.RowSelectorWidth = 28;
            targetGrid.DisplayLayout.Override.MinRowHeight = 24;
            targetGrid.DisplayLayout.Override.DefaultRowHeight = 24;
            targetGrid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BorderColor = BorderBlue;
            targetGrid.DisplayLayout.Override.FilterCellAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.FilterCellAppearance.BorderColor = BorderBlue;
            targetGrid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.CellAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
            targetGrid.DisplayLayout.Override.WrapHeaderText = DefaultableBoolean.True;
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

                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                    {
                        g.FillEllipse(shadowBrush, 5, 5, 24, 24);
                    }

                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(220, 20, 20, 20)))
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
            gridAudit.AllowDrop = true;
            gridAudit.MouseDown += Grid_MouseDown;
            gridAudit.MouseMove += Grid_MouseMove;
            gridAudit.MouseUp += Grid_MouseUp;
            gridAudit.DragOver += Grid_DragOver;
            gridAudit.DragDrop += Grid_DragDrop;

            ContextMenuStrip headerMenu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            chooserItem.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            headerMenu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            headerMenu.Items.Add(showAllItem);

            gridAudit.ContextMenuStrip = headerMenu;
        }

        private void Grid_MouseDown(object sender, MouseEventArgs e)
        {
            if (gridAudit.DisplayLayout == null || gridAudit.DisplayLayout.Bands.Count == 0)
                return;

            UIElement element = gridAudit.DisplayLayout.UIElement?.ElementFromPoint(new Point(e.X, e.Y));
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
                    columnBeingDragged = col;
                    headerDragStartPoint = new Point(e.X, e.Y);
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingHeaderToHide || columnBeingDragged == null || e.Button != MouseButtons.Left)
                return;

            int deltaX = Math.Abs(e.X - headerDragStartPoint.X);
            int deltaY = e.Y - headerDragStartPoint.Y;

            if (deltaY > 20 && deltaY > deltaX)
            {
                gridAudit.Cursor = blackXCursor;
                string colName = !string.IsNullOrEmpty(columnBeingDragged.Header.Caption) ? columnBeingDragged.Header.Caption : columnBeingDragged.Key;
                headerToolTip.SetToolTip(gridAudit, $"✖ Drag down to hide '{colName}' column");
            }
            else
            {
                gridAudit.Cursor = Cursors.Default;
                headerToolTip.SetToolTip(gridAudit, string.Empty);
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
                gridAudit.Cursor = Cursors.Default;
                headerToolTip.SetToolTip(gridAudit, string.Empty);
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
                Point clientPt = gridAudit.PointToClient(new Point(e.X, e.Y));
                int dropPosition = GetTargetColumnPositionFromPoint(clientPt);
                UnhideColumn(item.ColumnKey, dropPosition);
            }
        }

        private int GetTargetColumnPositionFromPoint(Point pt)
        {
            if (gridAudit.DisplayLayout == null || gridAudit.DisplayLayout.Bands.Count == 0)
                return 0;

            UIElement element = gridAudit.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header?.Column != null)
            {
                return headerUI.Header.Column.Header.VisiblePosition;
            }

            UltraGridBand band = gridAudit.DisplayLayout.Bands[0];
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

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            menu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            menu.Items.Add(showAllItem);

            menu.Show(gridAudit, location);
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
            if (columnChooserListBox == null || gridAudit.DisplayLayout.Bands.Count == 0)
                return;

            columnChooserListBox.Items.Clear();
            UltraGridBand band = gridAudit.DisplayLayout.Bands[0];

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Hidden && !col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && !string.Equals(col.Key, "TableName", StringComparison.OrdinalIgnoreCase))
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
            if (gridAudit.DisplayLayout.Bands.Count > 0 && gridAudit.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn col = gridAudit.DisplayLayout.Bands[0].Columns[columnKey];
                col.Hidden = false;
                if (targetVisiblePosition.HasValue)
                {
                    col.Header.VisiblePosition = targetVisiblePosition.Value;
                }
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                PopulateColumnChooserListBox();
            }
        }

        private void UnhideAllColumns()
        {
            userHiddenColumnKeys.Clear();
            if (gridAudit.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = gridAudit.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                if (!col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && !string.Equals(col.Key, "TableName", StringComparison.OrdinalIgnoreCase))
                {
                    col.Hidden = false;
                }
            }
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

        private void InitializeGridFooter()
        {
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void CreateFooterCells()
        {
            if (gridFooterPanel == null) return;
            gridFooterPanel.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (gridAudit.DisplayLayout == null || gridAudit.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridAudit.DisplayLayout.Bands[0];
            int xOffset = gridAudit.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True
                ? gridAudit.DisplayLayout.Override.RowSelectorWidth
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

            bool isNumeric = gridAudit.DisplayLayout.Bands.Count > 0 &&
                             gridAudit.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(gridAudit.DisplayLayout.Bands[0].Columns[columnKey]);

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
            if (gridAudit.DisplayLayout == null || gridAudit.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0 || gridFooterPanel == null)
                return;

            UltraGridBand band = gridAudit.DisplayLayout.Bands[0];
            int rowSelectorWidth = gridAudit.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True
                ? gridAudit.DisplayLayout.Override.RowSelectorWidth
                : 0;
            int scrollOffset = 0;
            if (gridAudit.ActiveColScrollRegion != null)
            {
                scrollOffset = gridAudit.ActiveColScrollRegion.Position;
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
            if (gridAudit.Rows == null) yield break;
            foreach (UltraGridRow row in gridAudit.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
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

            if (gridAudit.DisplayLayout != null &&
                gridAudit.DisplayLayout.Bands.Count > 0 &&
                gridAudit.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn column = gridAudit.DisplayLayout.Bands[0].Columns[columnKey];
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

        private void LoadLookupData()
        {
            EnsureRepositories();
            BindItemCombo();
            BindDateCombo();
            BindActionCombo();
            BindGroupCombo();
            BindCategoryCombo();
            BindBrandCombo();
            BindModelCombo();
            BindLocationCombo();
        }

        private void BindItemCombo()
        {
            _itemOptions.Clear();
            _itemOptions.Add(new ComboItem { Text = "ALL", Value = "ALL" });

            BindCombo(cmbItemNo, _itemOptions, "ALL", true);
        }

        private void BindDateCombo()
        {
            List<ComboItem> items = new List<ComboItem>
            {
                new ComboItem { Text = "By Range", Value = "RANGE" },
                new ComboItem { Text = "Today", Value = "TODAY" },
                new ComboItem { Text = "Yesterday", Value = "YESTERDAY" },
                new ComboItem { Text = "This Week", Value = "THISWEEK" },
                new ComboItem { Text = "Last Week", Value = "LASTWEEK" },
                new ComboItem { Text = "This Month", Value = "THISMONTH" },
                new ComboItem { Text = "Last Month", Value = "LASTMONTH" }
            };

            BindCombo(cmbDatePreset, items, "RANGE", false);
        }

        private void BindActionCombo()
        {
            List<ComboItem> items = new List<ComboItem>
            {
                new ComboItem { Text = "All", Value = "ALL" },
                new ComboItem { Text = "Goods Receive (ADD)", Value = "ADD" },
                new ComboItem { Text = "Purchase Return (PUR-RETURN)", Value = "PUR-RETURN" },
                new ComboItem { Text = "Invoice/Cash Sale (INVOICE)", Value = "INVOICE" },
                new ComboItem { Text = "Goods Return (RETURN)", Value = "RETURN" },
                new ComboItem { Text = "Adjustment In (ADJ-IN)", Value = "ADJ-IN" },
                new ComboItem { Text = "Adjustment Out (ADJ-OUT)", Value = "ADJ-OUT" }
            };

            BindCombo(cmbAction, items, "ALL", false);
        }

        private void BindGroupCombo()
        {
            _groupOptions.Clear();
            _groupOptions.Add(new ComboItem { Text = "ALL", Value = "ALL" });

            GroupDDlGrid grid = _dropdowns.getGroupDDl();
            if (grid != null && grid.List != null)
            {
                foreach (GroupDDL item in grid.List)
                {
                    _groupOptions.Add(new ComboItem
                    {
                        Text = item.GroupName ?? string.Empty,
                        Value = item.Id.ToString()
                    });
                }
            }

            BindCombo(cmbGroup, _groupOptions, "ALL", false);
        }

        private void BindCategoryCombo()
        {
            _categoryOptions.Clear();
            _categoryOptions.Add(new ComboItem { Text = "ALL", Value = "ALL", ParentValue = "ALL" });

            CategoryDDlGrid grid = _dropdowns.getCategoryDDl(string.Empty);
            if (grid != null && grid.List != null)
            {
                foreach (CategoryDDL item in grid.List)
                {
                    _categoryOptions.Add(new ComboItem
                    {
                        Text = item.CategoryName ?? string.Empty,
                        Value = item.Id.ToString(),
                        ParentValue = item.GroupId > 0 ? item.GroupId.ToString() : "ALL"
                    });
                }
            }

            ApplyCategoryOptions();
        }

        private void ApplyCategoryOptions()
        {
            string selectedGroup = GetSelectedValue(cmbGroup);
            List<ComboItem> items = new List<ComboItem>();

            foreach (ComboItem item in _categoryOptions)
            {
                if (item.Value == "ALL" || string.Equals(selectedGroup, "ALL", StringComparison.OrdinalIgnoreCase) || item.ParentValue == selectedGroup)
                {
                    items.Add(item);
                }
            }

            string existingValue = GetSelectedValue(cmbCategory);
            BindCombo(cmbCategory, items, items.Exists(x => x.Value == existingValue) ? existingValue : "ALL", false);
        }

        private void BindBrandCombo()
        {
            _brandOptions.Clear();
            _brandOptions.Add(new ComboItem { Text = "ALL", Value = "ALL" });

            BrandDDLGrid grid = _dropdowns.getBrandDDl();
            if (grid != null && grid.List != null)
            {
                foreach (BrandDDL item in grid.List)
                {
                    _brandOptions.Add(new ComboItem
                    {
                        Text = item.BrandName ?? string.Empty,
                        Value = item.Id.ToString()
                    });
                }
            }

            BindCombo(cmbBrand, _brandOptions, "ALL", false);
        }

        private void BindModelCombo()
        {
            _modelOptions.Clear();
            _modelOptions.Add(new ComboItem { Text = "ALL", Value = "ALL" });

            ItemTypeDDlGrid grid = _dropdowns.getItemTypeDDl();
            if (grid != null && grid.List != null)
            {
                foreach (ItemTypeDDL item in grid.List)
                {
                    _modelOptions.Add(new ComboItem
                    {
                        Text = item.ItemType ?? string.Empty,
                        Value = item.Id.ToString()
                    });
                }
            }

            BindCombo(cmbModel, _modelOptions, "ALL", false);
        }

        private void BindLocationCombo()
        {
            List<ComboItem> items = new List<ComboItem>
            {
                new ComboItem { Text = "ALL", Value = "ALL" }
            };

            BindCombo(cmbLocation, items, "ALL", false);
        }

        private void BindCombo(UltraComboEditor combo, List<ComboItem> items, string defaultValue, bool allowEdit)
        {
            combo.DataSource = null;
            combo.DisplayMember = "Text";
            combo.ValueMember = "Value";
            combo.DataSource = items;
            combo.DropDownStyle = allowEdit ? Infragistics.Win.DropDownStyle.DropDown : Infragistics.Win.DropDownStyle.DropDownList;

            if (!string.IsNullOrWhiteSpace(defaultValue))
            {
                combo.Value = defaultValue;
            }
        }

        public void RibbonClear() => Clear();

        public void Clear()
        {
            ResetFilters(true);
        }

        private void ResetFilters(bool reloadData = true)
        {
            dtFromDate.Value = DateTime.Today.AddDays(-7);
            dtToDate.Value = DateTime.Today;
            cmbItemNo.Value = "ALL";
            cmbItemNo.Text = "ALL";
            cmbDatePreset.Value = "RANGE";
            cmbGroup.Value = "ALL";
            ApplyCategoryOptions();
            cmbCategory.Value = "ALL";
            cmbLocation.Value = "ALL";
            cmbBrand.Value = "ALL";
            cmbModel.Value = "ALL";
            cmbAction.Value = "ALL";
            btnHideSelection.Text = ultraPanelSelection.Visible ? "Hide Selection" : "Show Selection";
            ApplyDatePreset();

            if (reloadData)
            {
                LoadData();
            }
        }

        private void ApplyDatePreset()
        {
            string preset = GetSelectedValue(cmbDatePreset);
            DateTime from = DateTime.Today;
            DateTime to = DateTime.Today;
            bool isRange = string.Equals(preset, "RANGE", StringComparison.OrdinalIgnoreCase);

            switch (preset)
            {
                case "TODAY":
                    break;
                case "YESTERDAY":
                    from = DateTime.Today.AddDays(-1);
                    to = DateTime.Today.AddDays(-1);
                    break;
                case "THISWEEK":
                    from = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    break;
                case "LASTWEEK":
                    from = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek - 7);
                    to = from.AddDays(6);
                    break;
                case "THISMONTH":
                    from = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    break;
                case "LASTMONTH":
                    DateTime firstDayThisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    from = firstDayThisMonth.AddMonths(-1);
                    to = firstDayThisMonth.AddDays(-1);
                    break;
                default:
                    isRange = true;
                    break;
            }

            if (!isRange)
            {
                dtFromDate.Value = from;
                dtToDate.Value = to;
            }

            dtFromDate.Enabled = isRange;
            dtToDate.Enabled = isRange;
        }

        private void LoadData()
        {
            try
            {
                EnsureRepositories();

                AuditTrailFilter filter = new AuditTrailFilter();
                filter.InitializeFromSessionIfNotSet();
                filter.FromDate = Convert.ToDateTime(dtFromDate.Value);
                filter.ToDate = Convert.ToDateTime(dtToDate.Value);
                filter.ActivityKey = null;
                filter.Action = GetSelectedValue(cmbAction);
                
                string selectedItemNo = GetSelectedValue(cmbItemNo);
                filter.ItemNo = string.Equals(selectedItemNo, "ALL", StringComparison.OrdinalIgnoreCase) ? null : selectedItemNo;
                filter.ItemId = null;
                filter.SearchText = filter.ItemNo != null ? null : NormalizeSearchText(cmbItemNo.Text);
                
                filter.GroupId = ToNullableInt(cmbGroup.Value);
                filter.CategoryId = ToNullableInt(cmbCategory.Value);
                filter.BrandId = ToNullableInt(cmbBrand.Value);
                filter.ModelId = ToNullableInt(cmbModel.Value);
                filter.SelectedUserId = null;

                _items.Clear();
                List<AuditTrailItem> loaded = _repository.GetAuditTrail(filter);
                if (loaded != null)
                {
                    for (int i = 0; i < loaded.Count; i++)
                    {
                        loaded[i].SlNo = i + 1;
                    }

                    _items.AddRange(loaded);
                }

                gridAudit.DataSource = null;
                gridAudit.DataSource = _items;
                FormatGrid(gridAudit);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading audit trail: " + ex.Message, "Audit Trail", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatGrid(UltraGrid targetGrid)
        {
            if (targetGrid.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            UltraGridBand band = targetGrid.DisplayLayout.Bands[0];
            HideColumn(band, "ItemId");
            HideColumn(band, "UserId");
            HideColumn(band, "GroupId");
            HideColumn(band, "CategoryId");
            HideColumn(band, "BrandId");
            HideColumn(band, "ModelId");
            HideColumn(band, "TableName");

            SetCaption(band, "SlNo", "Sl.No");
            SetCaption(band, "DocDate", "Doc. Date");
            SetCaption(band, "ReportDate", "Report Date");
            SetCaption(band, "ItemNo", "Item No");
            SetCaption(band, "Description", "Description");
            SetCaption(band, "CategoryName", "Category");
            SetCaption(band, "GroupName", "Group");
            SetCaption(band, "DocNo", "Doc No");
            SetCaption(band, "Account", "Account");
            SetCaption(band, "Reference", "Reference");
            SetCaption(band, "Price", "Price");
            SetCaption(band, "Cost", "Cost");
            SetCaption(band, "BalanceBF", "Balance B/F");
            SetCaption(band, "Action", "Action");
            SetCaption(band, "Quantity", "Quantity");
            SetCaption(band, "BalanceCF", "Balance C/F");
            SetCaption(band, "UserName", "User");

            SetVisiblePosition(band, "SlNo", 0);
            SetVisiblePosition(band, "DocDate", 1);
            SetVisiblePosition(band, "ReportDate", 2);
            SetVisiblePosition(band, "ItemNo", 3);
            SetVisiblePosition(band, "Description", 4);
            SetVisiblePosition(band, "CategoryName", 5);
            SetVisiblePosition(band, "GroupName", 6);
            SetVisiblePosition(band, "DocNo", 7);
            SetVisiblePosition(band, "Account", 8);
            SetVisiblePosition(band, "Reference", 9);
            SetVisiblePosition(band, "Price", 10);
            SetVisiblePosition(band, "Cost", 11);
            SetVisiblePosition(band, "BalanceBF", 12);
            SetVisiblePosition(band, "Action", 13);
            SetVisiblePosition(band, "Quantity", 14);
            SetVisiblePosition(band, "BalanceCF", 15);
            SetVisiblePosition(band, "UserName", 16);

            foreach (UltraGridColumn column in band.Columns)
            {
                column.AllowRowFiltering = DefaultableBoolean.True;
            }

            SetWidth(band, "SlNo", 55);
            SetWidth(band, "DocDate", 130);
            SetWidth(band, "ReportDate", 130);
            SetWidth(band, "ItemNo", 100);
            SetWidth(band, "Description", 260);
            SetWidth(band, "CategoryName", 120);
            SetWidth(band, "GroupName", 120);
            SetWidth(band, "DocNo", 105);
            SetWidth(band, "Account", 110);
            SetWidth(band, "Reference", 180);
            SetWidth(band, "Price", 85);
            SetWidth(band, "Cost", 85);
            SetWidth(band, "BalanceBF", 95);
            SetWidth(band, "Action", 95);
            SetWidth(band, "Quantity", 85);
            SetWidth(band, "BalanceCF", 95);
            SetWidth(band, "UserName", 100);

            FormatDateColumn(band, "DocDate");
            FormatDateColumn(band, "ReportDate");
            FormatIntegerColumn(band, "SlNo");
            FormatDecimalColumn(band, "Price");
            FormatDecimalColumn(band, "Cost");
            FormatDecimalColumn(band, "BalanceBF");
            FormatDecimalColumn(band, "Quantity");
            FormatDecimalColumn(band, "BalanceCF");

            // Re-apply user hidden columns
            if (targetGrid == gridAudit)
            {
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (userHiddenColumnKeys.Contains(col.Key))
                    {
                        col.Hidden = true;
                    }
                }

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                if (columnChooserForm != null && columnChooserForm.Visible)
                {
                    PopulateColumnChooserListBox();
                }
            }
        }

        private void SetCaption(UltraGridBand band, string columnName, string caption)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Header.Caption = caption;
            }
        }

        private void SetVisiblePosition(UltraGridBand band, string columnName, int position)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Header.VisiblePosition = position;
            }
        }

        private void SetWidth(UltraGridBand band, string columnName, int width)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Width = width;
            }
        }

        private void HideColumn(UltraGridBand band, string columnName)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Hidden = true;
            }
        }

        private void FormatDateColumn(UltraGridBand band, string columnName)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Format = "dd/MM/yyyy HH:mm:ss";
                band.Columns[columnName].CellAppearance.TextHAlign = HAlign.Left;
            }
        }

        private void FormatDecimalColumn(UltraGridBand band, string columnName)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Format = "n2";
                band.Columns[columnName].CellAppearance.TextHAlign = HAlign.Right;
            }
        }

        private void FormatIntegerColumn(UltraGridBand band, string columnName)
        {
            if (band.Columns.Exists(columnName))
            {
                band.Columns[columnName].Format = "0";
                band.Columns[columnName].CellAppearance.TextHAlign = HAlign.Center;
            }
        }

        private void ShowGridPreview(string title)
        {
            if (_items.Count == 0)
            {
                return;
            }

            using (Form previewForm = new Form())
            {
                previewForm.Text = title;
                previewForm.StartPosition = FormStartPosition.CenterParent;
                previewForm.WindowState = FormWindowState.Maximized;
                previewForm.BackColor = Color.White;
                previewForm.Font = Font;

                Infragistics.Win.Misc.UltraPanel previewPanel = new Infragistics.Win.Misc.UltraPanel();
                previewPanel.Dock = DockStyle.Fill;
                previewForm.Controls.Add(previewPanel);

                UltraGrid previewGrid = new UltraGrid();
                previewGrid.Dock = DockStyle.Fill;
                previewPanel.ClientArea.Controls.Add(previewGrid);

                ConfigureGridAppearance(previewGrid);
                previewGrid.DataSource = null;
                previewGrid.DataSource = new List<AuditTrailItem>(_items);
                FormatGrid(previewGrid);

                previewForm.ShowDialog(this);
            }
        }

        private static string NormalizeSearchText(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || string.Equals(value.Trim(), "ALL", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return value.Trim();
        }

        private static string GetSelectedValue(UltraComboEditor combo)
        {
            string value = Convert.ToString(combo.Value);
            return string.IsNullOrWhiteSpace(value) ? combo.Text : value;
        }

        private static int? ToNullableInt(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            int parsed;
            return int.TryParse(Convert.ToString(value), out parsed) && parsed > 0 ? parsed : (int?)null;
        }

        private bool IsDesignTime()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                   (Site != null && Site.DesignMode);
        }

        private void EnsureRepositories()
        {
            if (_repository == null)
            {
                _repository = new AuditTrailReportRepository();
            }

            if (_dropdowns == null)
            {
                _dropdowns = new Dropdowns();
            }

            if (_itemRepository == null)
            {
                _itemRepository = new ItemMasterRepository();
            }
        }
    }
}
