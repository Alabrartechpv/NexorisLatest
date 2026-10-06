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

        public frmItemReport()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            try
            {
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

            // Register footer cell sync and column drag-to-hide handlers matching frmPurchaseReturn / frmStockReport
            ultraGridTransactions.Resize += (s, e) => UpdateFooterCellPositions();
            ultraGridTransactions.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
            ultraGridTransactions.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            ultraGridTransactions.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            ultraGridTransactions.Paint += (s, e) => UpdateFooterCellPositions();

            SetupHeaderDragToHideAndColumnChooser();
            InitializeGridFooter();
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
                if (col.Hidden && !col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
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
                if (!col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
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

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            e.Layout.ScrollBounds = ScrollBounds.ScrollToFill;
            e.Layout.Scrollbars = Scrollbars.Both;
        }

        private void ConfigureColumn(UltraGridBand band, string key, string header,
            int width, string format, HAlign align, int visPos)
        {
            if (!band.Columns.Exists(key)) return;

            UltraGridColumn col = band.Columns[key];
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

                ultraGridTransactions.DataSource = reportData.Transactions;
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
                    Filter = "CSV Files|*.csv",
                    Title = "Save Report"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
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

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
