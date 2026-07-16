using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using Repository;

namespace PosBranch_Win.DialogBox
{
    public partial class StateDialog : Form
    {
        private DataTable fullDataTable = null;
        private Label lblStatus;
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private bool isOriginalOrder = true;

        // Add properties to store selected state information
        public int SelectedStateID { get; private set; }
        public string SelectedStateName { get; private set; }

        public StateDialog()
        {
            InitializeComponent();
            this.KeyPreview = true;
            InitializeStatusLabel();
            SetupUltraGridStyle();
            SetupColumnChooserMenu();
            InitializeSearchFilterComboBox();
            InitializeColumnOrderComboBox();
            SetupPanelHoverEffects();
            ConnectNavigationPanelEvents();

            this.Load += StateDialog_Load;
            this.ultraGridState.InitializeLayout += ultraGrid1_InitializeLayout;
            this.ultraGridState.KeyDown += ultraGrid1_KeyDown;
            this.ultraGridState.DoubleClickRow += ultraGrid1_DoubleClickRow;
            this.textBoxsearch.KeyDown += textBox1_KeyDown;
            this.ultraGridState.AfterSelectChange += ultraGrid1_AfterSelectChange;
            this.KeyDown += StateDialog_KeyDown;
            this.textBoxsearch.GotFocus += textBoxsearch_GotFocus;
            this.textBoxsearch.LostFocus += textBoxsearch_LostFocus;
            this.textBoxsearch.Text = "Search states...";
        }

        private void StateDialog_Load(object sender, EventArgs e)
        {
            LoadStateData();
            this.BeginInvoke(new Action(() =>
            {
                textBoxsearch.Focus();
                textBoxsearch.Select();
            }));
        }

        private void LoadStateData()
        {
            try
            {
                Dropdowns dp = new Dropdowns();
                var stateData = dp.getStateDDl();
                fullDataTable = ToDataTable(stateData.List.ToList());
                PreserveOriginalRowOrder(fullDataTable);
                ultraGridState.DataSource = fullDataTable;
                if (ultraGridState.DisplayLayout.Bands.Count > 0)
                {
                    if (ultraGridState.DisplayLayout.Bands[0].Columns.Exists("StateName"))
                        ultraGridState.DisplayLayout.Bands[0].Columns["StateName"].Width = 350;
                    if (ultraGridState.DisplayLayout.Bands[0].Columns.Exists("StateID"))
                        ultraGridState.DisplayLayout.Bands[0].Columns["StateID"].Width = 80;
                }
                if (ultraGridState.Rows.Count > 0)
                {
                    ultraGridState.ActiveRow = ultraGridState.Rows[0];
                    ultraGridState.Selected.Rows.Clear();
                    ultraGridState.Selected.Rows.Add(ultraGridState.Rows[0]);
                }
                UpdateRecordCountLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading state data: " + ex.Message);
            }
        }

        private DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, System.Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        private void PreserveOriginalRowOrder(DataTable table)
        {
            if (!table.Columns.Contains("OriginalRowOrder"))
            {
                DataColumn orderColumn = new DataColumn("OriginalRowOrder", typeof(int));
                table.Columns.Add(orderColumn);
                int rowIndex = 0;
                foreach (DataRow row in table.Rows)
                {
                    row["OriginalRowOrder"] = rowIndex++;
                }
            }
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                foreach (UltraGridColumn col in e.Layout.Bands[0].Columns)
                    col.Hidden = true;
                string[] columnsToShow = new string[] { "StateID", "StateName" };
                for (int i = 0; i < columnsToShow.Length; i++)
                {
                    string colKey = columnsToShow[i];
                    if (e.Layout.Bands[0].Columns.Exists(colKey))
                    {
                        var col = e.Layout.Bands[0].Columns[colKey];
                        col.Hidden = false;
                        col.Header.VisiblePosition = i;
                        switch (colKey)
                        {
                            case "StateID":
                                col.Header.Caption = "ID";
                                col.Width = 80;
                                break;
                            case "StateName":
                                col.Header.Caption = "State Name";
                                col.Width = 350;
                                break;
                        }
                    }
                }
                if (e.Layout.Bands[0].Columns.Exists("OriginalRowOrder"))
                    e.Layout.Bands[0].Columns["OriginalRowOrder"].Hidden = true;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error initializing grid layout: {ex.Message}");
            }
        }

        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            SelectState();
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectState();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void SelectState()
        {
            if (ultraGridState.ActiveRow != null)
            {
                try
                {
                    UltraGridCell nameCell = ultraGridState.ActiveRow.Cells["StateName"];
                    UltraGridCell idCell = ultraGridState.ActiveRow.Cells["StateID"];
                    if (nameCell != null && idCell != null && nameCell.Value != null && idCell.Value != null)
                    {
                        string name = nameCell.Value.ToString();
                        int id = Convert.ToInt32(idCell.Value.ToString());
                        // Set the properties to expose selected state
                        SelectedStateName = name;
                        SelectedStateID = id;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error selecting state: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (ultraGridState.Rows.Count > 0)
                {
                    ultraGridState.Focus();
                    ultraGridState.ActiveRow = ultraGridState.Rows[0];
                    ultraGridState.Selected.Rows.Clear();
                    ultraGridState.Selected.Rows.Add(ultraGridState.Rows[0]);
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (ultraGridState.Rows.Count > 0)
                {
                    ultraGridState.Focus();
                    ultraGridState.ActiveRow = ultraGridState.Rows[ultraGridState.Rows.Count - 1];
                    ultraGridState.Selected.Rows.Clear();
                    ultraGridState.Selected.Rows.Add(ultraGridState.ActiveRow);
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (ultraGridState.Rows.Count == 1)
                {
                    ultraGridState.ActiveRow = ultraGridState.Rows[0];
                    ultraGridState.Selected.Rows.Clear();
                    ultraGridState.Selected.Rows.Add(ultraGridState.Rows[0]);
                    SelectState();
                }
                else if (ultraGridState.Rows.Count > 0)
                {
                    ultraGridState.Focus();
                    ultraGridState.ActiveRow = ultraGridState.Rows[0];
                    ultraGridState.Selected.Rows.Clear();
                    ultraGridState.Selected.Rows.Add(ultraGridState.Rows[0]);
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void textBoxsearch_GotFocus(object sender, EventArgs e)
        {
            if (textBoxsearch.Text == "Search states...")
            {
                textBoxsearch.Text = "";
                textBoxsearch.ForeColor = Color.Black;
            }
        }

        private void textBoxsearch_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxsearch.Text))
            {
                textBoxsearch.Text = "Search states...";
                textBoxsearch.ForeColor = Color.Gray;
            }
        }

        private void InitializeStatusLabel()
        {
            lblStatus = new Label();
            lblStatus.Name = "lblStatus";
            lblStatus.AutoSize = false;
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 30;
            lblStatus.BackColor = Color.LightYellow;
            lblStatus.BorderStyle = BorderStyle.FixedSingle;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Text = "Ready";
            this.Controls.Add(lblStatus);
        }

        private void UpdateStatus(string message)
        {
            if (lblStatus != null)
            {
                lblStatus.Text = message;
                lblStatus.Update();
            }
        }

        private void SetupUltraGridStyle()
        {
            try
            {
                ultraGridState.DisplayLayout.Reset();
                ultraGridState.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGridState.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGridState.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGridState.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGridState.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGridState.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                ultraGridState.DisplayLayout.Override.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.WithinBand;
                ultraGridState.DisplayLayout.Override.AllowColSizing = Infragistics.Win.UltraWinGrid.AllowColSizing.Free;
                ultraGridState.DisplayLayout.Override.AllowColSwapping = Infragistics.Win.UltraWinGrid.AllowColSwapping.WithinBand;
                ultraGridState.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;
                ultraGridState.DisplayLayout.GroupByBox.Hidden = true;
                ultraGridState.DisplayLayout.GroupByBox.Prompt = string.Empty;
                ultraGridState.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGridState.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGridState.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGridState.DisplayLayout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGridState.DisplayLayout.Override.BorderStyleRowSelector = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGridState.DisplayLayout.Override.RowAppearance.BorderAlpha = Infragistics.Win.Alpha.Opaque;
                ultraGridState.DisplayLayout.Override.CellAppearance.BorderAlpha = Infragistics.Win.Alpha.Opaque;
                ultraGridState.DisplayLayout.Override.CellPadding = 0;
                ultraGridState.DisplayLayout.Override.RowSpacingBefore = 0;
                ultraGridState.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGridState.DisplayLayout.Override.CellSpacing = 0;
                ultraGridState.DisplayLayout.InterBandSpacing = 0;
                ultraGridState.DisplayLayout.Override.MinRowHeight = 30;
                ultraGridState.DisplayLayout.Override.DefaultRowHeight = 30;
                ultraGridState.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGridState.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGridState.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;
                ultraGridState.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.None;
                ultraGridState.DisplayLayout.Override.ColumnAutoSizeMode = Infragistics.Win.UltraWinGrid.ColumnAutoSizeMode.None;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error setting up grid style: {ex.Message}");
            }
        }

        private void SetupColumnChooserMenu()
        {
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += ColumnChooserMenuItem_Click;
            gridContextMenu.Items.Add(columnChooserMenuItem);
            ultraGridState.ContextMenuStrip = gridContextMenu;
            SetupDirectHeaderDragDrop();
        }
        private void SetupDirectHeaderDragDrop()
        {
            ultraGridState.AllowDrop = true;
            ultraGridState.MouseDown += UltraGrid1_MouseDown;
            ultraGridState.MouseMove += UltraGrid1_MouseMove;
            ultraGridState.MouseUp += UltraGrid1_MouseUp;
            ultraGridState.DragOver += UltraGrid1_DragOver;
            ultraGridState.DragDrop += UltraGrid1_DragDrop;
            CreateColumnChooserForm();
        }
        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization",
                Size = new Size(220, 180),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 240, 240),
                ShowIcon = false,
                ShowInTaskbar = false
            };
            columnChooserForm.FormClosing += ColumnChooserForm_FormClosing;
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
                Color bgColor = Color.FromArgb(33, 150, 243);
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    evt.Graphics.FillRectangle(bgBrush, rect);
                }
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat();
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;
                    evt.Graphics.DrawString(item.DisplayText, evt.Font, textBrush, rect, sf);
                }
                if ((evt.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    using (Pen focusPen = new Pen(Color.White, 1.5f))
                    {
                        Rectangle focusRect = rect;
                        focusRect.Inflate(-2, -2);
                        evt.Graphics.DrawRectangle(focusPen, focusRect);
                    }
                }
            };
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;
            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserListBox.ScrollAlwaysVisible = false;
            PopulateColumnChooserListBox();
        }
        private void ColumnChooserMenuItem_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }
        private void ShowColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Show();
                return;
            }
            CreateColumnChooserForm();
            columnChooserForm.Show(this);
        }
        private void ColumnChooserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (columnChooserListBox != null)
            {
                columnChooserListBox.MouseDown -= ColumnChooserListBox_MouseDown;
                columnChooserListBox.DragOver -= ColumnChooserListBox_DragOver;
                columnChooserListBox.DragDrop -= ColumnChooserListBox_DragDrop;
                columnChooserListBox = null;
            }
            UpdateStatus("Column customization closed");
        }
        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;
            columnChooserListBox.Items.Clear();
            string[] standardColumns = new string[] { "StateID", "StateName" };
            Dictionary<string, string> displayNames = new Dictionary<string, string>()
            {
                { "StateID", "ID" },
                { "StateName", "State Name" },
            };
            HashSet<string> addedColumns = new HashSet<string>();
            if (ultraGridState.DisplayLayout.Bands.Count > 0)
            {
                foreach (string colKey in standardColumns)
                {
                    if (ultraGridState.DisplayLayout.Bands[0].Columns.Exists(colKey) &&
                        ultraGridState.DisplayLayout.Bands[0].Columns[colKey].Hidden &&
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
            public ColumnItem(string columnKey, string displayText)
            {
                ColumnKey = columnKey;
                DisplayText = displayText;
            }
            public override string ToString() { return DisplayText; }
        }
        private Point startPoint;
        private Infragistics.Win.UltraWinGrid.UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
        private void UltraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingColumn = false;
            columnToMove = null;
            startPoint = new Point(e.X, e.Y);
            if (e.Y < 40)
            {
                int xPos = 0;
                if (ultraGridState.DisplayLayout.Override.RowSelectors == Infragistics.Win.DefaultableBoolean.True)
                {
                    xPos += ultraGridState.DisplayLayout.Override.RowSelectorWidth;
                }
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGridState.DisplayLayout.Bands[0].Columns)
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
                        ultraGridState.Cursor = Cursors.No;
                        string columnName = !string.IsNullOrEmpty(columnToMove.Header.Caption) ? columnToMove.Header.Caption : columnToMove.Key;
                        toolTip.SetToolTip(ultraGridState, $"Drag down to hide '{columnName}' column");
                        if (e.Y - startPoint.Y > 50)
                        {
                            HideColumn(columnToMove);
                            columnToMove = null;
                            isDraggingColumn = false;
                            ultraGridState.Cursor = Cursors.Default;
                            toolTip.SetToolTip(ultraGridState, "");
                        }
                    }
                }
            }
        }
        private void UltraGrid1_MouseUp(object sender, MouseEventArgs e)
        {
            ultraGridState.Cursor = Cursors.Default;
            toolTip.SetToolTip(ultraGridState, "");
            isDraggingColumn = false;
            columnToMove = null;
        }
        private void HideColumn(Infragistics.Win.UltraWinGrid.UltraGridColumn column)
        {
            if (column != null && !column.Hidden)
            {
                string columnName = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                savedColumnWidths[column.Key] = column.Width;
                ultraGridState.SuspendLayout();
                column.Hidden = true;
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGridState.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                    {
                        col.Width = savedColumnWidths[col.Key];
                    }
                }
                ultraGridState.ResumeLayout();
                if (columnChooserForm == null || columnChooserForm.IsDisposed)
                {
                    CreateColumnChooserForm();
                }
                if (columnChooserListBox != null)
                {
                    bool alreadyExists = false;
                    foreach (object item in columnChooserListBox.Items)
                    {
                        if (item is ColumnItem columnItem && columnItem.ColumnKey == column.Key)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }
                    if (!alreadyExists)
                    {
                        columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                    }
                }
            }
        }
        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                ColumnItem item = columnChooserListBox.Items[index] as ColumnItem;
                if (item != null)
                {
                    columnChooserListBox.SelectedIndex = index;
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }
        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)))
            {
                var column = (Infragistics.Win.UltraWinGrid.UltraGridColumn)e.Data.GetData(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn));
                if (column != null && !column.Hidden)
                {
                    string columnName = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                    column.Hidden = true;
                    columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                }
            }
        }
        private void UltraGrid1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void UltraGrid1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                ColumnItem item = (ColumnItem)e.Data.GetData(typeof(ColumnItem));
                if (item != null)
                {
                    if (ultraGridState.DisplayLayout.Bands.Count > 0 &&
                        ultraGridState.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                    {
                        var column = ultraGridState.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                        ultraGridState.SuspendLayout();
                        column.Hidden = false;
                        columnChooserListBox.Items.Remove(item);
                        ultraGridState.ResumeLayout();
                        toolTip.Show($"Column '{item.DisplayText}' restored",
                                ultraGridState,
                                ultraGridState.PointToClient(Control.MousePosition),
                                2000);
                    }
                }
            }
        }
        private void SetupPanelHoverEffects()
        {
            SetupPanelGroupHoverEffects(ultraPanel4, label4, ultraPictureBox3);
            SetupPanelGroupHoverEffects(ultraPanel6, label3, ultraPictureBox2);
            SetupPanelGroupHoverEffects(ultraPanel3, null, ultraPictureBox5);
            SetupPanelGroupHoverEffects(ultraPanel7, null, ultraPictureBox6);
        }
        private void SetupPanelGroupHoverEffects(
            Infragistics.Win.Misc.UltraPanel panel,
            Label label,
            Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox)
        {
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
            panel.MouseEnter += (s, e) => { applyHoverEffect(); };
            panel.MouseLeave += (s, e) => { removeHoverEffect(); };
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
            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }
        private void ConnectNavigationPanelEvents()
        {
            ultraPanel3.Click += MoveRowUp;
            ultraPanel3.ClientArea.Click += MoveRowUp;
            ultraPictureBox5.Click += MoveRowUp;
            ultraPanel7.Click += MoveRowDown;
            ultraPanel7.ClientArea.Click += MoveRowDown;
            ultraPictureBox6.Click += MoveRowDown;
            ultraPanel5.Click += (s, e) => SelectState();
            label5.Click += (s, e) => SelectState();
            ultraPanel6.Click += (s, e) => this.Close();
            ultraPictureBox2.Click += (s, e) => this.Close();
            label3.Click += (s, e) => this.Close();
            ultraPanel4.Click += OpenStateForm;
            ultraPictureBox3.Click += OpenStateForm;
            label4.Click += OpenStateForm;
        }
        private void OpenStateForm(object sender, EventArgs e)
        {
            // If you have a state editor dialog, open it here. Otherwise, show a stub message.
            MessageBox.Show("Open State Editor dialog here (New/Edit)", "State Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadStateData(); // Refresh data after potential changes
            UpdateStatus("State list refreshed.");
        }
        private void MoveRowUp(object sender, EventArgs e)
        {
            if (ultraGridState.Rows.Count == 0) return;
            if (ultraGridState.ActiveRow == null)
            {
                ultraGridState.ActiveRow = ultraGridState.Rows[ultraGridState.Rows.Count - 1];
                ultraGridState.Selected.Rows.Clear();
                ultraGridState.Selected.Rows.Add(ultraGridState.ActiveRow);
                ultraGridState.ActiveRowScrollRegion.ScrollRowIntoView(ultraGridState.ActiveRow);
                return;
            }
            int currentIndex = ultraGridState.ActiveRow.Index;
            if (currentIndex > 0)
            {
                var rowToActivate = ultraGridState.Rows[currentIndex - 1];
                ultraGridState.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);
                ultraGridState.ActiveRow = rowToActivate;
                ultraGridState.Selected.Rows.Clear();
                ultraGridState.Selected.Rows.Add(rowToActivate);
            }
            UpdateRecordCountLabel();
        }
        private void MoveRowDown(object sender, EventArgs e)
        {
            if (ultraGridState.Rows.Count == 0) return;
            if (ultraGridState.ActiveRow == null)
            {
                ultraGridState.ActiveRow = ultraGridState.Rows[0];
                ultraGridState.Selected.Rows.Clear();
                ultraGridState.Selected.Rows.Add(ultraGridState.ActiveRow);
                ultraGridState.ActiveRowScrollRegion.ScrollRowIntoView(ultraGridState.ActiveRow);
                return;
            }
            int currentIndex = ultraGridState.ActiveRow.Index;
            if (currentIndex < ultraGridState.Rows.Count - 1)
            {
                var rowToActivate = ultraGridState.Rows[currentIndex + 1];
                ultraGridState.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);
                ultraGridState.ActiveRow = rowToActivate;
                ultraGridState.Selected.Rows.Clear();
                ultraGridState.Selected.Rows.Add(rowToActivate);
            }
            UpdateRecordCountLabel();
        }
        private void ultraGrid1_AfterSelectChange(object sender, AfterSelectChangeEventArgs e)
        {
            // Optionally implement selection change logic
        }
        private void StateDialog_KeyDown(object sender, KeyEventArgs e)
        {
            // Optionally implement form-level key handling
        }
        private void UpdateRecordCountLabel()
        {
            int currentDisplayCount = ultraGridState.Rows?.Count ?? 0;
            int totalCount = fullDataTable?.Rows.Count ?? 0;
            if (this.Controls.Find("label1", true).Length > 0)
            {
                Label label1 = this.Controls.Find("label1", true)[0] as Label;
                if (label1 != null)
                {
                    label1.Text = $"Showing {currentDisplayCount} of {totalCount} records";
                    label1.AutoSize = true;
                    label1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
                    label1.ForeColor = Color.FromArgb(0, 70, 170);
                }
            }
            if (textBox3 != null)
            {
                textBox3.Text = currentDisplayCount.ToString();
            }
            UpdateStatus($"Showing {currentDisplayCount} of {totalCount} records");
        }
        private void FilterStates()
        {
            try
            {
                if (fullDataTable == null) return;
                string searchText = textBoxsearch.Text.Trim();
                if (searchText == "Search states...")
                {
                    searchText = "";
                }
                DataView dv = fullDataTable.DefaultView;
                if (!string.IsNullOrEmpty(searchText))
                {
                    string selectedField = comboBox1.SelectedItem?.ToString() ?? "All";
                    string filter = "";
                    string escapedSearchText = searchText.Replace("'", "''");
                    switch (selectedField)
                    {
                        case "ID":
                            filter = $"CONVERT(StateID, 'System.String') LIKE '%{escapedSearchText}%'";
                            break;
                        case "State Name":
                            filter = $"StateName LIKE '%{escapedSearchText}%'";
                            break;
                        case "All":
                        default:
                            filter = $"CONVERT(StateID, 'System.String') LIKE '%{escapedSearchText}%' OR StateName LIKE '%{escapedSearchText}%'";
                            break;
                    }
                    dv.RowFilter = filter;
                }
                else
                {
                    dv.RowFilter = string.Empty;
                }
                if (ultraGridState.Rows.Count > 0)
                {
                    ultraGridState.ActiveRow = ultraGridState.Rows[0];
                    ultraGridState.Selected.Rows.Clear();
                    ultraGridState.Selected.Rows.Add(ultraGridState.Rows[0]);
                }
                UpdateRecordCountLabel();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error filtering states: {ex.Message}");
            }
        }
        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            comboBox1.Items.Add("ID");
            comboBox1.Items.Add("State Name");
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            textBoxsearch.TextChanged += TextBoxsearch_TextChanged;
        }
        private void InitializeColumnOrderComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add("ID");
            comboBox2.Items.Add("State Name");
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterStates();
        }
        private void TextBoxsearch_TextChanged(object sender, EventArgs e)
        {
            FilterStates();
        }
        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Optionally implement column reordering logic here
        }
    }
}
