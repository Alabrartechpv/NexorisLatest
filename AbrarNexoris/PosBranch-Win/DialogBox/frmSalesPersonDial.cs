using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using PosBranch_Win.Master;
using Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmSalesPersonDial : Form
    {
        private Dropdowns dp = new Dropdowns();
        private List<SalesPersonDDl> salesPersonListData; 

        public delegate void SelectedSalesPersonHandler(string name);
        public event SelectedSalesPersonHandler OnSalesPersonSelected;

        private Label lblStatus;
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private bool isOriginalOrder = true;
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
        private Point startPoint;
        private UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;

        public frmSalesPersonDial()
        {
            InitializeComponent();
            this.KeyPreview = true;

            InitializeStatusLabel();
            SetupUltraGridStyle();
            SetupUltraPanelStyle();
            InitializeSearchFilterComboBox();
            InitializeColumnOrderComboBox();
            SetupPanelHoverEffects();
            ConnectNavigationPanelEvents();
            SetupColumnChooserMenu();

            this.Load += frmSalesPersonDial_Load;
            this.ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
            this.ultraGrid1.KeyDown += ultraGrid1_KeyDown;
            this.ultraGrid1.DoubleClickRow += ultraGrid1_DoubleClickRow;
            this.textBoxsearch.KeyDown += textBoxsearch_KeyDown;
            this.ultraPictureBox4.Click += UltraPictureBox4_Click;
            this.ultraGrid1.MouseDown += UltraGrid1_MouseDown;
            this.ultraGrid1.MouseMove += UltraGrid1_MouseMove;
            this.ultraGrid1.MouseUp += UltraGrid1_MouseUp;
            this.ultraGrid1.AfterSortChange += (s, e) => PreserveColumnWidths();
            this.ultraGrid1.AfterColPosChanged += (s, e) => PreserveColumnWidths();
            this.SizeChanged += (s, e) => PreserveColumnWidths();
            this.ultraGrid1.Resize += (s, e) => PreserveColumnWidths();
            this.ultraGrid1.AfterRowActivate += ultraGrid1_AfterRowActivate;
            this.FormClosing += (s, e) => {
                if (columnChooserForm != null && !columnChooserForm.IsDisposed)
                {
                    columnChooserForm.Close();
                }
            };
        }

        private void frmSalesPersonDial_Load(object sender, EventArgs e)
        {
            ultraPanel4.Visible = true;
            LoadSalesData();
            this.BeginInvoke(new Action(() =>
            {
                textBoxsearch.Focus();
                textBoxsearch.Select();
            }));
            UpdateRecordCountLabel();
            UpdateStatus("Ready");
        }

        private void LoadSalesData()
        {
            try
            {
                SalesPersonDDlGrid data = dp.GetSalesPerson();
                salesPersonListData = data.List.ToList();
                ultraGrid1.DataSource = salesPersonListData;

                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
                UpdateRecordCountLabel();
                InitializeSavedColumnWidths();
            }
            catch (Exception ex)
            {
                UpdateStatus("Error loading sales person data: " + ex.Message);
            }
        }

        private void InitializeSavedColumnWidths()
        {
            savedColumnWidths.Clear();
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                foreach (UltraGridColumn column in ultraGrid1.DisplayLayout.Bands[0].Columns)
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
                ultraGrid1.SuspendLayout();
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                            col.Width = savedColumnWidths[col.Key];
                    }
                }
                ultraGrid1.ResumeLayout();
            }
            catch (Exception ex) { UpdateStatus($"Error preserving column widths: {ex.Message}"); }
        }

        #region User Interaction
        private void LoadSelectedSalesPerson()
        {
            if (ultraGrid1.ActiveRow == null) return;
            try
            {
                string name = ultraGrid1.ActiveRow.Cells["UserName"].Value?.ToString() ?? "N/A";

                OnSalesPersonSelected?.Invoke(name);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading selected sales person: " + ex.Message);
            }
        }

        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            LoadSelectedSalesPerson();
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadSelectedSalesPerson();
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
            if (e.KeyCode == Keys.Down && ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.Focus();
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
            }
            else if (e.KeyCode == Keys.Enter && ultraGrid1.Rows.Count > 0)
            {
                if (ultraGrid1.Rows.Count == 1) LoadSelectedSalesPerson();
                else ultraGrid1.Focus();
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
            int currentDisplayCount = ultraGrid1.Rows?.Count ?? 0;
            int totalCount = salesPersonListData?.Count ?? 0;
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
            if(panel == null) return;
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
                var o = ultraGrid1.DisplayLayout.Override;
                o.AllowAddNew = AllowAddNew.No;
                o.AllowDelete = DefaultableBoolean.False;
                o.AllowUpdate = DefaultableBoolean.False;
                o.RowSelectors = DefaultableBoolean.True;
                o.SelectTypeRow = SelectType.Single;
                o.HeaderClickAction = HeaderClickAction.SortSingle;
                o.AllowColMoving = AllowColMoving.WithinBand;
                o.CellClickAction = CellClickAction.RowSelect;
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;
                ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
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
                ultraGrid1.DisplayLayout.InterBandSpacing = 0;
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
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;
                if (ultraGrid1.DisplayLayout.ScrollBarLook != null)
                {
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }
                o.CellAppearance.TextVAlign = VAlign.Middle;
                o.CellAppearance.TextHAlign = HAlign.Center;
                ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
                o.ColumnAutoSizeMode = ColumnAutoSizeMode.None;
            }
            catch (Exception ex) { UpdateStatus($"Grid style error: {ex.Message}"); }
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
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
                band.Columns.Cast<UltraGridColumn>().ToList().ForEach(c => c.Hidden = true);

                if (band.Columns.Exists("UserName"))
                {
                    var col = band.Columns["UserName"];
                    col.Hidden = false;
                    col.Header.VisiblePosition = 0;
                    col.Header.Caption = "Username";
                    col.CellAppearance.TextHAlign = HAlign.Left;
                    col.PerformAutoResize(PerformAutoSizeType.AllRowsInBand);
                }
            }
            catch (Exception ex) { UpdateStatus($"Layout error: {ex.Message}"); }
        }
        #endregion

        #region Search, Filter, Sort, Reorder
        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new object[] { "All", "User Name" });
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += (s, e) => FilterData();
            textBoxsearch.TextChanged += (s, e) => FilterData();
        }

        private void FilterData()
        {
            if (salesPersonListData == null) return;
            try
            {
                string searchText = textBoxsearch.Text.Trim().ToLower();
                IEnumerable<SalesPersonDDl> filtered = salesPersonListData;

                if (!string.IsNullOrEmpty(searchText))
                {
                    filtered = salesPersonListData.Where(p => p.UserName.ToLower().Contains(searchText));
                }

                ultraGrid1.DataSource = filtered.ToList();
                if (ultraGrid1.Rows.Count > 0) ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                UpdateRecordCountLabel();
            }
            catch (Exception ex) { UpdateStatus($"Filter error: {ex.Message}"); }
        }


        private void InitializeColumnOrderComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(new object[] { "User Name" });
            comboBox2.SelectedIndex = 0;
            comboBox2.Enabled = false; 
        }

        private void ReorderColumns(string selectedColumn)
        {
            // Not needed for a single column
        }

        private void UltraPictureBox4_Click(object sender, EventArgs e)
        {
            if (salesPersonListData == null) return;
            isOriginalOrder = !isOriginalOrder;
            
            if (isOriginalOrder)
            {
                ultraGrid1.DataSource = salesPersonListData.OrderBy(p => p.UserName).ToList();
            }
            else
            {
                ultraGrid1.DataSource = salesPersonListData.OrderByDescending(p => p.UserName).ToList();
            }
        }
        #endregion

        #region Column Chooser
        private void SetupColumnChooserMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Field/Column Chooser", null, (s, e) => ShowColumnChooser());
            ultraGrid1.ContextMenuStrip = menu;
            SetupDirectHeaderDragDrop(); 
        }
        
        private void SetupDirectHeaderDragDrop()
        {
            ultraGrid1.AllowDrop = true;
            ultraGrid1.MouseDown += UltraGrid1_MouseDown;
            ultraGrid1.MouseMove += UltraGrid1_MouseMove;
            ultraGrid1.MouseUp += UltraGrid1_MouseUp;
            ultraGrid1.DragOver += UltraGrid1_DragOver;
            ultraGrid1.DragDrop += UltraGrid1_DragDrop;
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
                Text = "Customization", Size = new Size(220, 280), FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual, TopMost = true, MaximizeBox = false, MinimizeBox = false,
                BackColor = Color.FromArgb(240, 240, 240), ShowIcon = false, ShowInTaskbar = false
            };
            columnChooserForm.FormClosing += (s, e) => { columnChooserListBox = null; };
            columnChooserForm.Shown += (s, e) => PositionColumnChooserAtBottomRight();

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill, AllowDrop = true, DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None, BackColor = Color.FromArgb(240, 240, 240),
                ItemHeight = 30, IntegralHeight = false
            };
            columnChooserListBox.DrawItem += (s, evt) => {
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
            string[] standardColumns = { "UserName"};
            var displayNames = new Dictionary<string, string>
            {
                { "UserName", "Username" }
            };
            var addedColumns = new HashSet<string>();
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                foreach (string colKey in standardColumns)
                {
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colKey) &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns[colKey].Hidden &&
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

        private void UltraGrid1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(ColumnItem)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void UltraGrid1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnItem)) is ColumnItem item)
            {
                if (ultraGrid1.DisplayLayout.Bands.Count > 0 && ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                {
                    var column = ultraGrid1.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                    column.Hidden = false;
                    columnChooserListBox.Items.Remove(item);
                    toolTip.Show($"'{item.DisplayText}' restored", ultraGrid1, ultraGrid1.PointToClient(MousePosition), 1500);
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
            ultraPanel5.Click += (s, e) => LoadSelectedSalesPerson(); // OK
            ultraPictureBox1.Click += (s, e) => LoadSelectedSalesPerson();
            label5.Click += (s, e) => LoadSelectedSalesPerson();
            ultraPanel6.Click += (s, e) => this.Close(); // Close
            ultraPictureBox2.Click += (s, e) => this.Close();
            label3.Click += (s, e) => this.Close();

            ultraPanel4.Click += OpenUsersForm;
            ultraPictureBox3.Click += OpenUsersForm;
            label4.Click += OpenUsersForm;
        }

        private void OpenUsersForm(object sender, EventArgs e)
        {
            try
            {
                // Find the Home form to open the users form in a tab
                Form homeForm = null;
                // First, check if the owner of this dialog is the Home form
                if (this.Owner is Home)
                {
                    homeForm = this.Owner;
                }
                else if (this.Owner != null && this.Owner.Owner is Home)
                {
                    // Check if owner's owner is Home (for nested dialogs)
                    homeForm = this.Owner.Owner;
                }
                else
                {
                    // If not, search through the application's open forms
                    foreach (Form form in Application.OpenForms)
                    {
                        if (form is Home)
                        {
                            homeForm = form;
                            break;
                        }
                    }
                }

                // If we found the Home form, use its OpenFormInTab method
                if (homeForm is Home home)
                {
                    var usersForm = new FrmUsers();
                    // Use reflection to call the OpenFormInTab method
                    var methodInfo = typeof(Home).GetMethod("OpenFormInTabSafe", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (methodInfo != null)
                    {
                        // Hide this form first to avoid flickering
                        this.Hide();
                        
                        // Call the method to open form in tab
                        methodInfo.Invoke(home, new object[] { usersForm, "Users" });
                        
                        // Use BeginInvoke to close this form after ensuring the users form is shown
                        this.BeginInvoke(new Action(() => {
                            // Close this form
                            this.Close();
                        }));
                    }
                    else
                    {
                        // Fallback to dialog if reflection fails
                        this.Show(); // Show this form again since we hid it
                        usersForm.ShowDialog(this);
                        usersForm.Dispose();
                        LoadSalesData();
                        UpdateStatus("Sales person list refreshed.");
                    }
                }
                else
                {
                    // Fallback to dialog if Home form not found
                    using (var usersForm = new FrmUsers())
                    {
                        usersForm.ShowDialog(this);
                    }
                    LoadSalesData();
                    UpdateStatus("Sales person list refreshed.");
                }
            }
            catch (Exception ex)
            {
                // If there's an error, make sure this form is visible again
                this.Show();

                UpdateStatus($"Error opening Users form: {ex.Message}");
                MessageBox.Show($"Error opening Users form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MoveRowUp(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.Index > 0)
            {
                var newRow = ultraGrid1.Rows[ultraGrid1.ActiveRow.Index - 1];
                ultraGrid1.ActiveRow = newRow;
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(newRow);
            }
        }

        private void MoveRowDown(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.Index < ultraGrid1.Rows.Count - 1)
            {
                var newRow = ultraGrid1.Rows[ultraGrid1.ActiveRow.Index + 1];
                ultraGrid1.ActiveRow = newRow;
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(newRow);
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
            
            Action applyHoverEffect = () => {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
                panel.ClientArea.Cursor = Cursors.Hand;
            };
            Action removeHoverEffect = () => {
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
        private void UltraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingColumn = false;
            columnToMove = null;
            startPoint = new Point(e.X, e.Y);
            if (e.Y < 40)
            {
                int xPos = 0;
                if (ultraGrid1.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True)
                    xPos += ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
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
        private void UltraGrid1_MouseMove(object sender, MouseEventArgs e)
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
                        ultraGrid1.Cursor = Cursors.No;
                        string columnName = !string.IsNullOrEmpty(columnToMove.Header.Caption) ? columnToMove.Header.Caption : columnToMove.Key;
                        toolTip.SetToolTip(ultraGrid1, $"Drag down to hide '{columnName}' column");
                        if (e.Y - startPoint.Y > 50)
                        {
                            HideColumn(columnToMove);
                            columnToMove = null;
                            isDraggingColumn = false;
                            ultraGrid1.Cursor = Cursors.Default;
                            toolTip.SetToolTip(ultraGrid1, "");
                        }
                    }
                }
            }
        }
        private void UltraGrid1_MouseUp(object sender, MouseEventArgs e)
        {
            ultraGrid1.Cursor = Cursors.Default;
            toolTip.SetToolTip(ultraGrid1, "");
            isDraggingColumn = false;
            columnToMove = null;
        }
        #endregion

        private void ultraGrid1_AfterRowActivate(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow != null)
            {
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
            }
        }
    }
}
