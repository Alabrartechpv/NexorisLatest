using Infragistics.Win;
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
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.AuditReport
{
    public partial class frmAuditReport : Form
    {
        private AuditTrailReportRepository _repository;
        private Dropdowns _dropdowns;
        private ItemMasterRepository _itemRepository;
        private readonly List<AuditTrailItem> _items;
        private readonly List<ComboItem> _groupOptions;
        private readonly List<ComboItem> _categoryOptions;
        private readonly List<ComboItem> _brandOptions;
        private readonly List<ComboItem> _modelOptions;
        private readonly List<ComboItem> _itemOptions;
        private Infragistics.Win.Misc.UltraPanel summaryFooterPanel;
        private readonly Dictionary<string, Label> summaryLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, string> columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };
        private readonly HashSet<string> auditSummaryColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Price",
            "Cost",
            "BalanceBF",
            "Quantity",
            "BalanceCF"
        };
        private readonly HashSet<string> internalGridColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ItemId",
            "UserId",
            "GroupId",
            "CategoryId",
            "BrandId",
            "ModelId",
            "TableName"
        };
        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private Point headerDragStartPoint;
        private UltraGridColumn columnToHideByDrag;
        private bool isDraggingHeaderColumn;
        private Point chooserDragStartPoint;
        private ColumnItem chooserDragItem;
        private bool summaryFooterInitialized;
        private readonly System.Windows.Forms.ToolTip columnChooserToolTip = new System.Windows.Forms.ToolTip();

        private sealed class ComboItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
            public string ParentValue { get; set; }
        }

        private sealed class ColumnItem
        {
            public ColumnItem(string columnKey, string displayText)
            {
                ColumnKey = columnKey;
                DisplayText = displayText;
            }

            public string ColumnKey { get; private set; }
            public string DisplayText { get; private set; }

            public override string ToString()
            {
                return DisplayText;
            }
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
            ConfigureButton(btnViewGrid, Color.FromArgb(72, 122, 214), Color.FromArgb(95, 145, 230));
            ConfigureButton(btnPreviewGrid, Color.FromArgb(94, 116, 202), Color.FromArgb(121, 141, 222));
            ConfigureButton(btnPreviewReport, Color.FromArgb(74, 130, 176), Color.FromArgb(104, 155, 196));
            ConfigureButton(btnHideSelection, Color.FromArgb(84, 120, 190), Color.FromArgb(112, 148, 214));

            ConfigureGridAppearance(gridAudit);
            InitializeSummaryFooterPanel();
            InitializeColumnChooserBehavior();
        }

        private void ConfigureButton(Infragistics.Win.Misc.UltraButton button, Color startColor, Color endColor)
        {
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.Appearance.BackColor = startColor;
            button.Appearance.BackColor2 = endColor;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.Appearance.BorderColor = startColor;
            button.HotTrackAppearance.BackColor = endColor;
            button.HotTrackAppearance.ForeColor = Color.White;
        }

        private void ConfigureGridAppearance(UltraGrid targetGrid)
        {
            targetGrid.UseAppStyling = false;
            targetGrid.UseOsThemes = DefaultableBoolean.False;
            targetGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
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
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(247, 250, 255);
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(145, 179, 222);
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(118, 157, 209);
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.FromArgb(17, 52, 102);
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BorderColor = Color.FromArgb(103, 142, 196);
            targetGrid.DisplayLayout.Override.FilterCellAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.FilterCellAppearance.BorderColor = Color.FromArgb(180, 198, 220);
            targetGrid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.CellAppearance.BorderColor = Color.FromArgb(210, 220, 235);
            targetGrid.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
            targetGrid.DisplayLayout.Override.WrapHeaderText = DefaultableBoolean.True;
        }

        private void InitializeSummaryFooterPanel()
        {
            if (summaryFooterInitialized || gridFooterPanel == null || gridAudit == null)
            {
                return;
            }

            summaryFooterInitialized = true;
            summaryFooterPanel = gridFooterPanel;

            summaryFooterPanel.Paint += (s, e) => AlignSummaryLabels();
            summaryFooterPanel.Resize += (s, e) => AlignSummaryLabels();
            gridAudit.AfterColPosChanged += (s, e) => AlignSummaryLabels();
            gridAudit.AfterSortChange += (s, e) => AlignSummaryLabels();
            gridAudit.AfterRowFilterChanged += (s, e) =>
            {
                UpdateFooterValues();
                AlignSummaryLabels();
            };
            gridAudit.InitializeLayout += (s, e) => AlignSummaryLabels();
            gridAudit.SizeChanged += (s, e) => AlignSummaryLabels();

            summaryFooterPanel.ClientArea.MouseUp += (s, e) =>
            {
                if (e.Button != MouseButtons.Right)
                {
                    return;
                }

                Control child = summaryFooterPanel.ClientArea.GetChildAtPoint(e.Location);
                if (child == null || !(child is Label))
                {
                    UltraGridColumn column = GetSummaryColumnAtFooterPoint(e.Location);
                    if (column != null)
                    {
                        CreateFooterLabelMenu(column.Key).Show(summaryFooterPanel.ClientArea, e.Location);
                    }
                }
            };
        }

        private ContextMenuStrip CreateFooterLabelMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            foreach (string type in summaryTypes)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(type) { Tag = type };
                item.Click += (s, e) =>
                {
                    columnAggregations[columnKey] = type;
                    UpdateSummaryFooter();
                };
                menu.Items.Add(item);
            }

            menu.Opening += (s, e) =>
            {
                foreach (ToolStripMenuItem item in menu.Items)
                {
                    item.Checked = columnAggregations.ContainsKey(columnKey) &&
                                   string.Equals(columnAggregations[columnKey], Convert.ToString(item.Tag), StringComparison.OrdinalIgnoreCase);
                }
            };

            return menu;
        }

        private void UpdateSummaryFooter()
        {
            if (summaryFooterPanel == null || summaryFooterPanel.ClientArea == null ||
                gridAudit == null || gridAudit.DisplayLayout == null || gridAudit.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            summaryFooterPanel.ClientArea.SuspendLayout();
            summaryFooterPanel.ClientArea.Controls.Clear();
            summaryLabels.Clear();

            lblCount.Location = new Point(12, 4);
            lblCount.Size = new Size(150, 18);
            lblCount.Text = "Rows: " + GetVisibleRowCount();
            summaryFooterPanel.ClientArea.Controls.Add(lblCount);

            UltraGridBand band = gridAudit.DisplayLayout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                if (column.Hidden || !auditSummaryColumns.Contains(column.Key) || !IsNumericColumn(column))
                {
                    continue;
                }

                string aggregation;
                if (!columnAggregations.TryGetValue(column.Key, out aggregation) ||
                    string.Equals(aggregation, "None", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Label label = new Label
                {
                    Name = "lblSummary_" + column.Key,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Height = summaryFooterPanel.Height - 4,
                    ContextMenuStrip = CreateFooterLabelMenu(column.Key)
                };

                summaryFooterPanel.ClientArea.Controls.Add(label);
                summaryLabels[column.Key] = label;
            }

            UpdateFooterValues();
            AlignSummaryLabels();
            summaryFooterPanel.ClientArea.ResumeLayout();
        }

        private void UpdateFooterValues()
        {
            if (gridAudit == null || gridAudit.DataSource == null)
            {
                return;
            }

            lblCount.Text = "Rows: " + GetVisibleRowCount();

            foreach (KeyValuePair<string, Label> summaryLabel in summaryLabels)
            {
                string columnKey = summaryLabel.Key;
                Label label = summaryLabel.Value;
                string aggregation = columnAggregations.ContainsKey(columnKey) ? columnAggregations[columnKey] : "None";
                List<decimal> values = GetVisibleNumericValues(columnKey);

                switch (aggregation)
                {
                    case "Sum":
                        label.Text = values.Count > 0 ? values.Sum().ToString("N2") : "0.00";
                        break;
                    case "Min":
                        label.Text = values.Count > 0 ? values.Min().ToString("N2") : "-";
                        break;
                    case "Max":
                        label.Text = values.Count > 0 ? values.Max().ToString("N2") : "-";
                        break;
                    case "Average":
                        label.Text = values.Count > 0 ? values.Average().ToString("N2") : "-";
                        break;
                    case "Count":
                        label.Text = values.Count.ToString();
                        break;
                    default:
                        label.Text = string.Empty;
                        break;
                }
            }
        }

        private void AlignSummaryLabels()
        {
            if (summaryFooterPanel == null || summaryFooterPanel.ClientArea == null ||
                gridAudit == null || gridAudit.DisplayLayout == null || gridAudit.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            UltraGridBand band = gridAudit.DisplayLayout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                Label label;
                if (column.Hidden || !summaryLabels.TryGetValue(column.Key, out label))
                {
                    continue;
                }

                UIElement headerElement = column.Header != null ? column.Header.GetUIElement() : null;
                if (headerElement == null || headerElement.Control == null)
                {
                    continue;
                }

                Point headerPoint = headerElement.Control.PointToScreen(headerElement.Rect.Location);
                int columnLeft = headerPoint.X - summaryFooterPanel.PointToScreen(Point.Empty).X;
                label.Left = columnLeft;
                label.Width = headerElement.Rect.Width;
                label.Top = 2;
                label.Height = summaryFooterPanel.Height - 4;
            }
        }

        private UltraGridColumn GetSummaryColumnAtFooterPoint(Point footerPoint)
        {
            if (summaryFooterPanel == null || gridAudit == null || gridAudit.DisplayLayout == null ||
                gridAudit.DisplayLayout.Bands.Count == 0)
            {
                return null;
            }

            UltraGridBand band = gridAudit.DisplayLayout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                if (column.Hidden || !auditSummaryColumns.Contains(column.Key) || !IsNumericColumn(column))
                {
                    continue;
                }

                UIElement headerElement = column.Header != null ? column.Header.GetUIElement() : null;
                if (headerElement == null || headerElement.Control == null)
                {
                    continue;
                }

                Point headerPoint = headerElement.Control.PointToScreen(headerElement.Rect.Location);
                int columnLeft = headerPoint.X - summaryFooterPanel.PointToScreen(Point.Empty).X;
                Rectangle columnBounds = new Rectangle(columnLeft, 0, headerElement.Rect.Width, summaryFooterPanel.Height);
                if (columnBounds.Contains(footerPoint))
                {
                    return column;
                }
            }

            return null;
        }

        private List<decimal> GetVisibleNumericValues(string columnKey)
        {
            List<decimal> values = new List<decimal>();
            foreach (UltraGridRow row in gridAudit.Rows)
            {
                if (row == null || !row.IsDataRow || row.IsFilteredOut || !row.Cells.Exists(columnKey))
                {
                    continue;
                }

                object value = row.Cells[columnKey].Value;
                if (value == null || value == DBNull.Value)
                {
                    continue;
                }

                decimal parsed;
                if (decimal.TryParse(Convert.ToString(value), out parsed))
                {
                    values.Add(parsed);
                }
            }

            return values;
        }

        private int GetVisibleRowCount()
        {
            int count = 0;
            foreach (UltraGridRow row in gridAudit.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
                {
                    count++;
                }
            }

            return count;
        }

        private bool IsNumericColumn(UltraGridColumn column)
        {
            Type type = column.DataType;
            return type == typeof(byte) || type == typeof(short) || type == typeof(int) ||
                   type == typeof(long) || type == typeof(float) || type == typeof(double) ||
                   type == typeof(decimal);
        }

        private void InitializeColumnChooserBehavior()
        {
            gridAudit.AllowDrop = true;
            gridAudit.MouseDown += GridAudit_MouseDown;
            gridAudit.MouseMove += GridAudit_MouseMove;
            gridAudit.MouseUp += GridAudit_MouseUp;
            gridAudit.DragOver += GridAudit_DragOver;
            gridAudit.DragDrop += GridAudit_DragDrop;
            LocationChanged += (s, e) => PositionColumnChooserAtBottomRight();
            SizeChanged += (s, e) => PositionColumnChooserAtBottomRight();
            Activated += (s, e) => PositionColumnChooserAtBottomRight();
        }

        private void GridAudit_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingHeaderColumn = false;
            columnToHideByDrag = null;
            headerDragStartPoint = e.Location;

            UltraGridColumn headerColumn = GetHeaderColumnAtPoint(e.Location);
            if (e.Button == MouseButtons.Right)
            {
                ShowColumnChooser(gridAudit.PointToScreen(e.Location));
                return;
            }

            if (e.Button == MouseButtons.Left && headerColumn != null && IsChooserColumn(headerColumn))
            {
                columnToHideByDrag = headerColumn;
                isDraggingHeaderColumn = true;
            }
        }

        private void GridAudit_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || !isDraggingHeaderColumn || columnToHideByDrag == null)
            {
                return;
            }

            int deltaX = Math.Abs(e.X - headerDragStartPoint.X);
            int deltaY = Math.Abs(e.Y - headerDragStartPoint.Y);
            if (deltaY <= SystemInformation.DragSize.Height || deltaY <= deltaX || e.Y <= headerDragStartPoint.Y)
            {
                return;
            }

            gridAudit.Cursor = Cursors.No;
            string columnName = GetColumnDisplayName(columnToHideByDrag);
            columnChooserToolTip.SetToolTip(gridAudit, "Drag down to hide '" + columnName + "' column");

            if (e.Y - headerDragStartPoint.Y > 50)
            {
                HideReportColumn(columnToHideByDrag);
                columnToHideByDrag = null;
                isDraggingHeaderColumn = false;
                gridAudit.Cursor = Cursors.Default;
                columnChooserToolTip.SetToolTip(gridAudit, string.Empty);
            }
        }

        private void GridAudit_MouseUp(object sender, MouseEventArgs e)
        {
            gridAudit.Cursor = Cursors.Default;
            columnChooserToolTip.SetToolTip(gridAudit, string.Empty);
            isDraggingHeaderColumn = false;
            columnToHideByDrag = null;
        }

        private UltraGridColumn GetHeaderColumnAtPoint(Point point)
        {
            if (gridAudit.DisplayLayout.Bands.Count == 0)
            {
                return null;
            }

            foreach (UltraGridColumn column in gridAudit.DisplayLayout.Bands[0].Columns)
            {
                if (column.Hidden)
                {
                    continue;
                }

                UIElement headerElement = column.Header != null ? column.Header.GetUIElement() : null;
                if (headerElement == null || headerElement.Control == null)
                {
                    continue;
                }

                Point screenPoint = headerElement.Control.PointToScreen(headerElement.Rect.Location);
                Rectangle headerBounds = new Rectangle(gridAudit.PointToClient(screenPoint), headerElement.Rect.Size);
                if (headerBounds.Contains(point))
                {
                    return column;
                }
            }

            return null;
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Column Chooser",
                Size = new Size(235, 260),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                ShowInTaskbar = false,
                BackColor = Color.FromArgb(240, 246, 252)
            };
            columnChooserForm.FormClosing += ColumnChooserForm_FormClosing;
            columnChooserForm.Shown += (s, e) => PositionColumnChooserAtBottomRight();

            Panel commandPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                Padding = new Padding(8, 4, 8, 6),
                BackColor = Color.FromArgb(224, 238, 250)
            };
            Button restoreAllButton = new Button
            {
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                Text = "Restore All",
                BackColor = Color.FromArgb(42, 137, 213),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            restoreAllButton.FlatAppearance.BorderSize = 0;
            restoreAllButton.Click += (s, e) => RestoreAllReportColumns();
            commandPanel.Controls.Add(restoreAllButton);

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 246, 252),
                ItemHeight = 30,
                IntegralHeight = false
            };
            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.MouseMove += ColumnChooserListBox_MouseMove;
            columnChooserListBox.MouseUp += ColumnChooserListBox_MouseUp;
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;
            columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;
            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(commandPanel);
            PopulateColumnChooserListBox();
        }

        private void ShowColumnChooser()
        {
            ShowColumnChooser(null);
        }

        private void ShowColumnChooser(Point? screenLocation)
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }
            else
            {
                PopulateColumnChooserListBox();
            }

            columnChooserForm.Show(this);
            if (screenLocation.HasValue)
            {
                PositionColumnChooserNear(screenLocation.Value);
            }
            else
            {
                PositionColumnChooserAtBottomRight();
            }
            columnChooserForm.BringToFront();
        }

        private void PositionColumnChooserNear(Point screenLocation)
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                return;
            }

            Rectangle workingArea = Screen.FromPoint(screenLocation).WorkingArea;
            int left = Math.Min(screenLocation.X + 8, workingArea.Right - columnChooserForm.Width);
            int top = Math.Min(screenLocation.Y + 8, workingArea.Bottom - columnChooserForm.Height);
            columnChooserForm.Location = new Point(Math.Max(workingArea.Left, left), Math.Max(workingArea.Top, top));
        }

        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed || !columnChooserForm.Visible)
            {
                return;
            }

            Point bottomRight = PointToScreen(new Point(ClientSize.Width, ClientSize.Height));
            columnChooserForm.Location = new Point(
                Math.Max(0, bottomRight.X - columnChooserForm.Width - 20),
                Math.Max(0, bottomRight.Y - columnChooserForm.Height - 20));
        }

        private void ColumnChooserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (columnChooserListBox != null)
            {
                columnChooserListBox.DrawItem -= ColumnChooserListBox_DrawItem;
                columnChooserListBox.MouseDown -= ColumnChooserListBox_MouseDown;
                columnChooserListBox.MouseMove -= ColumnChooserListBox_MouseMove;
                columnChooserListBox.MouseUp -= ColumnChooserListBox_MouseUp;
                columnChooserListBox.DoubleClick -= ColumnChooserListBox_DoubleClick;
                columnChooserListBox.DragOver -= ColumnChooserListBox_DragOver;
                columnChooserListBox.DragDrop -= ColumnChooserListBox_DragDrop;
                columnChooserListBox = null;
            }

            chooserDragItem = null;
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || gridAudit.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            columnChooserListBox.Items.Clear();
            foreach (UltraGridColumn column in gridAudit.DisplayLayout.Bands[0].Columns)
            {
                if (column.Hidden && IsChooserColumn(column))
                {
                    columnChooserListBox.Items.Add(new ColumnItem(column.Key, GetColumnDisplayName(column)));
                }
            }
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            ColumnItem item = columnChooserListBox.Items[e.Index] as ColumnItem;
            if (item == null)
            {
                return;
            }

            e.DrawBackground();
            Rectangle rect = e.Bounds;
            rect.Inflate(-4, -4);
            Color fillColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? Color.FromArgb(58, 112, 196)
                : Color.FromArgb(42, 137, 213);

            using (SolidBrush background = new SolidBrush(fillColor))
            using (SolidBrush foreground = new SolidBrush(Color.White))
            {
                e.Graphics.FillRectangle(background, rect);
                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                e.Graphics.DrawString(item.DisplayText, e.Font, foreground, rect, format);
            }
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            chooserDragItem = null;
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index == ListBox.NoMatches)
            {
                return;
            }

            ColumnItem item = columnChooserListBox.Items[index] as ColumnItem;
            if (item != null)
            {
                columnChooserListBox.SelectedIndex = index;
                chooserDragStartPoint = e.Location;
                chooserDragItem = item;
            }
        }

        private void ColumnChooserListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || chooserDragItem == null)
            {
                return;
            }

            int deltaX = Math.Abs(e.X - chooserDragStartPoint.X);
            int deltaY = Math.Abs(e.Y - chooserDragStartPoint.Y);
            if (deltaX <= SystemInformation.DragSize.Width && deltaY <= SystemInformation.DragSize.Height)
            {
                return;
            }

            ColumnItem dragItem = chooserDragItem;
            chooserDragItem = null;
            columnChooserListBox.DoDragDrop(dragItem, DragDropEffects.Move);
        }

        private void ColumnChooserListBox_MouseUp(object sender, MouseEventArgs e)
        {
            chooserDragItem = null;
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            ColumnItem item = columnChooserListBox != null ? columnChooserListBox.SelectedItem as ColumnItem : null;
            if (item != null)
            {
                RestoreReportColumn(item);
            }
        }

        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(UltraGridColumn)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(UltraGridColumn)))
            {
                return;
            }

            UltraGridColumn column = e.Data.GetData(typeof(UltraGridColumn)) as UltraGridColumn;
            if (column != null)
            {
                HideReportColumn(column);
            }
        }

        private void GridAudit_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(ColumnItem)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void GridAudit_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                return;
            }

            ColumnItem item = e.Data.GetData(typeof(ColumnItem)) as ColumnItem;
            if (item != null)
            {
                RestoreReportColumn(item);
            }
        }

        private void HideReportColumn(UltraGridColumn column)
        {
            if (column == null || column.Hidden || !IsChooserColumn(column) || GetVisibleChooserColumnCount() <= 1)
            {
                return;
            }

            savedColumnWidths[column.Key] = column.Width;
            gridAudit.SuspendLayout();
            column.Hidden = true;
            gridAudit.ResumeLayout();
            AddColumnChooserItem(column);
            UpdateSummaryFooter();
        }

        private void RestoreReportColumn(ColumnItem item)
        {
            if (item == null || gridAudit.DisplayLayout.Bands.Count == 0 ||
                !gridAudit.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
            {
                return;
            }

            UltraGridColumn column = gridAudit.DisplayLayout.Bands[0].Columns[item.ColumnKey];
            gridAudit.SuspendLayout();
            column.Hidden = false;
            if (savedColumnWidths.ContainsKey(column.Key))
            {
                column.Width = savedColumnWidths[column.Key];
            }
            gridAudit.ResumeLayout();

            if (columnChooserListBox != null)
            {
                columnChooserListBox.Items.Remove(item);
            }

            UpdateSummaryFooter();
        }

        private void RestoreAllReportColumns()
        {
            if (gridAudit.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            gridAudit.SuspendLayout();
            foreach (UltraGridColumn column in gridAudit.DisplayLayout.Bands[0].Columns)
            {
                if (IsChooserColumn(column))
                {
                    column.Hidden = false;
                    if (savedColumnWidths.ContainsKey(column.Key))
                    {
                        column.Width = savedColumnWidths[column.Key];
                    }
                }
            }
            gridAudit.ResumeLayout();

            PopulateColumnChooserListBox();
            UpdateSummaryFooter();
        }

        private void AddColumnChooserItem(UltraGridColumn column)
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            if (columnChooserListBox == null)
            {
                return;
            }

            foreach (object existingItem in columnChooserListBox.Items)
            {
                ColumnItem existingColumn = existingItem as ColumnItem;
                if (existingColumn != null && string.Equals(existingColumn.ColumnKey, column.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            columnChooserListBox.Items.Add(new ColumnItem(column.Key, GetColumnDisplayName(column)));
        }

        private bool IsChooserColumn(UltraGridColumn column)
        {
            return column != null && !internalGridColumns.Contains(column.Key);
        }

        private int GetVisibleChooserColumnCount()
        {
            if (gridAudit.DisplayLayout.Bands.Count == 0)
            {
                return 0;
            }

            int count = 0;
            foreach (UltraGridColumn column in gridAudit.DisplayLayout.Bands[0].Columns)
            {
                if (!column.Hidden && IsChooserColumn(column))
                {
                    count++;
                }
            }

            return count;
        }

        private string GetColumnDisplayName(UltraGridColumn column)
        {
            if (column == null)
            {
                return string.Empty;
            }

            return column.Header != null && !string.IsNullOrWhiteSpace(column.Header.Caption)
                ? column.Header.Caption
                : column.Key;
        }

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

            // Removed getItemGetAll preloading.
            // In POS, preloading all items into this filter dropdown causes performance issues.
            // The proper way is a clean, empty input where the user scans the barcode directly.

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
                UpdateSummaryFooter();
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
                column.AllowRowFiltering = DefaultableBoolean.False;
            }

            if (band.Columns.Exists("Action"))
            {
                band.Columns["Action"].AllowRowFiltering = DefaultableBoolean.True;
            }

            SetWidth(band, "SlNo", 55);
            SetWidth(band, "DocDate", 120);
            SetWidth(band, "ReportDate", 120);
            SetWidth(band, "ItemNo", 95);
            SetWidth(band, "Description", 260);
            SetWidth(band, "CategoryName", 120);
            SetWidth(band, "GroupName", 120);
            SetWidth(band, "DocNo", 105);
            SetWidth(band, "Account", 110);
            SetWidth(band, "Reference", 180);
            SetWidth(band, "Price", 70);
            SetWidth(band, "Cost", 70);
            SetWidth(band, "BalanceBF", 90);
            SetWidth(band, "Action", 95);
            SetWidth(band, "Quantity", 80);
            SetWidth(band, "BalanceCF", 90);
            SetWidth(band, "UserName", 100);

            FormatDateColumn(band, "DocDate");
            FormatDateColumn(band, "ReportDate");
            FormatIntegerColumn(band, "SlNo");
            FormatDecimalColumn(band, "Price");
            FormatDecimalColumn(band, "Cost");
            FormatDecimalColumn(band, "BalanceBF");
            FormatDecimalColumn(band, "Quantity");
            FormatDecimalColumn(band, "BalanceCF");
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
