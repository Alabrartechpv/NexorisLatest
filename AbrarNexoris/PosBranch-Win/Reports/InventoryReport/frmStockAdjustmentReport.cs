using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.InventoryReport
{
    public partial class frmStockAdjustmentReport : Form
    {
        private readonly StockAdjustmentReportRepository _repository = new StockAdjustmentReportRepository();
        private BackgroundWorker _searchWorker;
        private List<StockAdjustmentReportRow> _allRows = new List<StockAdjustmentReportRow>();
        private bool _accentPanelsCreated;

        public frmStockAdjustmentReport()
        {
            InitializeComponent();
            Font = new Font("Segoe UI", 9F);
            InitializeBackgroundWorker();
            InitializeForm();
        }

        private void InitializeBackgroundWorker()
        {
            _searchWorker = new BackgroundWorker();
            _searchWorker.WorkerSupportsCancellation = true;
            _searchWorker.DoWork += SearchWorker_DoWork;
            _searchWorker.RunWorkerCompleted += SearchWorker_RunWorkerCompleted;
        }

        private void InitializeForm()
        {
            dtpFromDate.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtpToDate.Value = DateTime.Today;

            comboType.Items.Clear();
            comboType.Items.Add("", "All");
            comboType.Items.Add("Stock IN", "Stock IN");
            comboType.Items.Add("Stock OUT", "Stock OUT");
            comboType.Value = "";
            comboType.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;

            StyleGrid();
            StyleButtons();
            SetupCardControls(cardDocCount, lblDocCountCaption, lblDocCountValue, "ADJUSTMENT DOCUMENTS", Color.FromArgb(25, 118, 210));
            SetupCardControls(cardStockIn, lblStockInCaption, lblStockInValue, "TOTAL STOCK IN QTY", Color.FromArgb(0, 150, 136));
            SetupCardControls(cardStockOut, lblStockOutCaption, lblStockOutValue, "TOTAL STOCK OUT QTY", Color.FromArgb(198, 40, 40));
            SetupCardControls(cardNetValue, lblNetValueCaption, lblNetValueValue, "NET ADJUSTMENT VALUE", Color.FromArgb(81, 45, 168));
            LayoutSummaryCards();

            btnSearch.Click += BtnSearch_Click;
            btnReset.Click += BtnReset_Click;
            btnExport.Click += BtnExport_Click;
            btnPrint.Click += BtnPrint_Click;
            btnClose.Click += BtnClose_Click;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            panelSummary.Resize += (s, e) => LayoutSummaryCards();

            KeyPreview = true;
            KeyDown += FrmStockAdjustmentReport_KeyDown;
            FormClosing += (s, e) =>
            {
                if (_searchWorker != null && _searchWorker.IsBusy)
                    _searchWorker.CancelAsync();
            };
        }

        private void StyleGrid()
        {
            gridReport.UseOsThemes = DefaultableBoolean.False;
            gridReport.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;
            gridReport.DisplayLayout.Override.FilterUIType = FilterUIType.FilterRow;
            gridReport.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            gridReport.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            gridReport.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            gridReport.DisplayLayout.GroupByBox.Hidden = true;

            gridReport.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(30, 40, 55);
            gridReport.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(42, 55, 72);
            gridReport.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            gridReport.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            gridReport.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            gridReport.DisplayLayout.Override.HeaderStyle = HeaderStyle.Standard;

            gridReport.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            gridReport.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 250, 253);
            gridReport.DisplayLayout.Override.MinRowHeight = 24;
            gridReport.DisplayLayout.Override.DefaultRowHeight = 24;
            gridReport.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(66, 165, 245);
            gridReport.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            gridReport.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
            gridReport.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            gridReport.DisplayLayout.Override.FilterRowAppearance.BackColor = Color.FromArgb(255, 255, 230);
            gridReport.DisplayLayout.Override.FilterRowAppearance.ForeColor = Color.FromArgb(33, 33, 33);

            gridReport.InitializeLayout += GridReport_InitializeLayout;
            gridReport.InitializeRow += GridReport_InitializeRow;
        }

        private void GridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;
            UltraGridBand band = e.Layout.Bands[0];

            ConfigureColumn(band, "SlNo", "S.No", 55, HAlign.Center, 0, "N0");
            ConfigureColumn(band, "StockAdjustmentNo", "Adj No", 80, HAlign.Center, 1, "N0");
            ConfigureColumn(band, "StockAdjustmentDate", "Date", 95, HAlign.Center, 2, "dd-MM-yyyy");
            ConfigureColumn(band, "AdjustmentType", "Type", 90, HAlign.Center, 3, null);
            ConfigureColumn(band, "Barcode", "Barcode", 105, HAlign.Left, 4, null);
            ConfigureColumn(band, "ItemName", "Item", 220, HAlign.Left, 5, null);
            ConfigureColumn(band, "UnitName", "Unit", 65, HAlign.Center, 6, null);
            ConfigureColumn(band, "SystemStock", "System Stock", 105, HAlign.Right, 7, "N2");
            ConfigureColumn(band, "PhysicalStock", "Physical Stock", 110, HAlign.Right, 8, "N2");
            ConfigureColumn(band, "QtyDifference", "Diff Qty", 95, HAlign.Right, 9, "N2");
            ConfigureColumn(band, "Cost", "Cost", 85, HAlign.Right, 10, "N2");
            ConfigureColumn(band, "AdjustmentValue", "Value", 100, HAlign.Right, 11, "N2");
            ConfigureColumn(band, "Reason", "Reason", 160, HAlign.Left, 12, null);
            ConfigureColumn(band, "LedgerName", "Ledger", 130, HAlign.Left, 13, null);
            ConfigureColumn(band, "UserName", "User", 90, HAlign.Left, 14, null);

            string[] visibleColumns =
            {
                "SlNo", "StockAdjustmentNo", "StockAdjustmentDate", "AdjustmentType", "Barcode", "ItemName",
                "UnitName", "SystemStock", "PhysicalStock", "QtyDifference", "Cost", "AdjustmentValue",
                "Reason", "LedgerName", "UserName"
            };

            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = !visibleColumns.Contains(column.Key);
            }
        }

        private void ConfigureColumn(UltraGridBand band, string key, string caption, int width, HAlign align, int position, string format)
        {
            if (!band.Columns.Exists(key)) return;
            UltraGridColumn column = band.Columns[key];
            column.Header.Caption = caption;
            column.Width = width;
            column.CellAppearance.TextHAlign = align;
            column.Header.VisiblePosition = position;
            if (!string.IsNullOrEmpty(format))
                column.Format = format;
        }

        private void GridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            try
            {
                if (!e.Row.Cells.Exists("AdjustmentType")) return;
                string type = Convert.ToString(e.Row.Cells["AdjustmentType"].Value);
                if (type == "Stock IN")
                {
                    e.Row.Cells["AdjustmentType"].Appearance.ForeColor = Color.FromArgb(0, 121, 107);
                    e.Row.Cells["AdjustmentType"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
                else if (type == "Stock OUT")
                {
                    e.Row.Cells["AdjustmentType"].Appearance.ForeColor = Color.FromArgb(198, 40, 40);
                    e.Row.Cells["AdjustmentType"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }
            catch { }
        }

        private void StyleButtons()
        {
            StyleButton(btnSearch, Color.FromArgb(25, 118, 210), Color.FromArgb(33, 150, 243), Color.FromArgb(66, 165, 245));
            StyleButton(btnReset, Color.FromArgb(245, 124, 0), Color.FromArgb(255, 152, 0), Color.FromArgb(255, 167, 38));
            StyleButton(btnExport, Color.FromArgb(0, 121, 107), Color.FromArgb(0, 150, 136), Color.FromArgb(38, 166, 154));
            StyleButton(btnPrint, Color.FromArgb(81, 45, 168), Color.FromArgb(103, 58, 183), Color.FromArgb(126, 87, 194));
            StyleButton(btnClose, Color.FromArgb(198, 40, 40), Color.FromArgb(244, 67, 54), Color.FromArgb(229, 115, 115));
        }

        private void StyleButton(Infragistics.Win.Misc.UltraButton button, Color c1, Color c2, Color hover)
        {
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.Appearance.BackColor = c1;
            button.Appearance.BackColor2 = c2;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.HotTrackAppearance.BackColor = hover;
            button.HotTrackAppearance.ForeColor = Color.White;
        }

        private void SetupCardControls(Infragistics.Win.Misc.UltraPanel card, Infragistics.Win.Misc.UltraLabel caption, Infragistics.Win.Misc.UltraLabel value, string captionText, Color valueColor)
        {
            card.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            card.UseAppStyling = false;
            card.UseOsThemes = DefaultableBoolean.False;
            card.Appearance.BackColor = Color.White;
            card.Appearance.BorderColor = Color.FromArgb(226, 232, 240);

            caption.Text = captionText;
            caption.Location = new Point(12, 8);
            caption.Size = new Size(200, 15);
            caption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            caption.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            caption.UseAppStyling = false;
            caption.UseOsThemes = DefaultableBoolean.False;

            value.Location = new Point(12, 26);
            value.Size = new Size(220, 28);
            value.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            value.Text = "0";
            value.Appearance.ForeColor = valueColor;
            value.UseAppStyling = false;
            value.UseOsThemes = DefaultableBoolean.False;
        }

        private void LayoutSummaryCards()
        {
            Infragistics.Win.Misc.UltraPanel[] cards = { cardDocCount, cardStockIn, cardStockOut, cardNetValue };
            Color[] colors =
            {
                Color.FromArgb(25, 118, 210),
                Color.FromArgb(0, 150, 136),
                Color.FromArgb(198, 40, 40),
                Color.FromArgb(81, 45, 168)
            };

            int totalWidth = panelSummary.ClientArea.Width - 30;
            int gap = 15;
            int cardWidth = (totalWidth - (gap * (cards.Length - 1))) / cards.Length;
            if (cardWidth < 190) cardWidth = 190;

            int x = 15;
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].Location = new Point(x, 10);
                cards[i].Size = new Size(cardWidth, 62);

                if (!_accentPanelsCreated)
                {
                    Panel accentLine = new Panel();
                    accentLine.Dock = DockStyle.Top;
                    accentLine.Height = 3;
                    accentLine.BackColor = colors[i];
                    cards[i].ClientArea.Controls.Add(accentLine);
                    accentLine.BringToFront();
                }

                x += cardWidth + gap;
            }

            _accentPanelsCreated = true;
        }

        private void FrmStockAdjustmentReport_Load(object sender, EventArgs e)
        {
            FetchFromDatabase();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            FetchFromDatabase();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            dtpFromDate.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtpToDate.Value = DateTime.Today;
            comboType.Value = "";
            txtSearch.Text = string.Empty;
            _allRows.Clear();
            gridReport.DataSource = null;
            CalculateSummaryValues(new List<StockAdjustmentReportRow>());
            lblStatus.Text = "Ready | Select filters and press Search (F5)";
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterFetchedRows();
        }

        private void FrmStockAdjustmentReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                FetchFromDatabase();
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                e.Handled = true;
                btnExport.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                e.Handled = true;
                btnPrint.PerformClick();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void FetchFromDatabase()
        {
            if (_searchWorker.IsBusy) return;

            if (dtpFromDate.Value.Date > dtpToDate.Value.Date)
            {
                MessageBox.Show("From date cannot be greater than To date.", "Stock Adjustment Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            btnSearch.Enabled = false;
            lblStatus.Text = "Searching stock adjustment records...";

            StockAdjustmentReportFilter filter = new StockAdjustmentReportFilter
            {
                CompanyId = SessionContext.CompanyId,
                BranchId = SessionContext.BranchId,
                FinYearId = SessionContext.FinYearId,
                FromDate = dtpFromDate.Value.Date,
                ToDate = dtpToDate.Value.Date,
                AdjustmentType = Convert.ToString(comboType.Value),
                SearchQuery = txtSearch.Text.Trim()
            };

            _searchWorker.RunWorkerAsync(filter);
        }

        private void SearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = _repository.GetStockAdjustmentReport((StockAdjustmentReportFilter)e.Argument);
        }

        private void SearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed || Disposing) return;

            try
            {
                if (e.Cancelled) return;

                if (e.Error != null)
                {
                    MessageBox.Show("Search failed: " + e.Error.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Search failed.";
                    return;
                }

                _allRows = e.Result as List<StockAdjustmentReportRow> ?? new List<StockAdjustmentReportRow>();
                FilterFetchedRows();
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    btnSearch.Enabled = true;
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void FilterFetchedRows()
        {
            string search = txtSearch.Text.Trim().ToLowerInvariant();
            List<StockAdjustmentReportRow> filtered;

            if (string.IsNullOrEmpty(search))
            {
                filtered = _allRows;
            }
            else
            {
                filtered = _allRows.Where(row =>
                    SafeContains(row.StockAdjustmentNo.ToString(), search) ||
                    SafeContains(row.Barcode, search) ||
                    SafeContains(row.ItemName, search) ||
                    SafeContains(row.Reason, search) ||
                    SafeContains(row.LedgerName, search) ||
                    SafeContains(row.UserName, search)).ToList();
            }

            gridReport.DataSource = filtered;
            CalculateSummaryValues(filtered);
            lblStatus.Text = "Ready | Found " + filtered.Count.ToString("N0") + " records.";
        }

        private bool SafeContains(string value, string search)
        {
            return !string.IsNullOrEmpty(value) && value.ToLowerInvariant().Contains(search);
        }

        private void CalculateSummaryValues(List<StockAdjustmentReportRow> rows)
        {
            if (rows == null || rows.Count == 0)
            {
                lblDocCountValue.Text = "0";
                lblStockInValue.Text = "0.00";
                lblStockOutValue.Text = "0.00";
                lblNetValueValue.Text = "0.00";
                return;
            }

            int documents = rows.Select(r => r.StockAdjustmentId).Distinct().Count();
            decimal stockIn = rows.Sum(r => r.StockInQty);
            decimal stockOut = rows.Sum(r => r.StockOutQty);
            decimal netValue = rows.Sum(r => r.AdjustmentValue);

            lblDocCountValue.Text = documents.ToString("N0");
            lblStockInValue.Text = stockIn.ToString("N2");
            lblStockOutValue.Text = stockOut.ToString("N2");
            lblNetValueValue.Text = netValue.ToString("N2");
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            List<StockAdjustmentReportRow> rows = gridReport.DataSource as List<StockAdjustmentReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Filter = "CSV Files (*.csv)|*.csv";
                    dialog.FileName = "StockAdjustmentReport_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

                    if (dialog.ShowDialog() != DialogResult.OK) return;

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("S.No,Adj No,Date,Type,Barcode,Item,Unit,System Stock,Physical Stock,Diff Qty,Stock In,Stock Out,Cost,Value,Reason,Ledger,User,Comments");

                    foreach (StockAdjustmentReportRow row in rows)
                    {
                        sb.AppendLine(string.Join(",",
                            CsvCell(row.SlNo.ToString()),
                            CsvCell(row.StockAdjustmentNo.ToString()),
                            CsvCell(row.StockAdjustmentDate.ToString("dd-MM-yyyy")),
                            CsvCell(row.AdjustmentType),
                            CsvCell(row.Barcode),
                            CsvCell(row.ItemName),
                            CsvCell(row.UnitName),
                            row.SystemStock.ToString("F2"),
                            row.PhysicalStock.ToString("F2"),
                            row.QtyDifference.ToString("F2"),
                            row.StockInQty.ToString("F2"),
                            row.StockOutQty.ToString("F2"),
                            row.Cost.ToString("F2"),
                            row.AdjustmentValue.ToString("F2"),
                            CsvCell(row.Reason),
                            CsvCell(row.LedgerName),
                            CsvCell(row.UserName),
                            CsvCell(row.Comments)));
                    }

                    File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Report exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows.Count == 0)
            {
                MessageBox.Show("No data to print.", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            gridReport.PrintPreview();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private string CsvCell(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return "\"" + value + "\"";
        }
    }
}
