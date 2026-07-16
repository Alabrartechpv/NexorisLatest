using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public class FrmCombinedPartyBalanceDetailDialog : Form
    {
        private readonly UltraGrid _gridDetails;

        public FrmCombinedPartyBalanceDetailDialog(string caption, IList<PartyBalanceDetailRowDto> rows)
        {
            Text = "Party Balance Details";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(980, 420);
            MinimizeBox = false;
            MaximizeBox = false;
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Close();
                    e.Handled = true;
                }
            };

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(12, 10, 12, 6)
            };

            Label titleLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 71, 79),
                Text = caption
            };
            headerPanel.Controls.Add(titleLabel);

            _gridDetails = new UltraGrid
            {
                Dock = DockStyle.Fill
            };
            _gridDetails.InitializeLayout += GridDetails_InitializeLayout;
            _gridDetails.InitializeRow += GridDetails_InitializeRow;
            _gridDetails.DataSource = rows;

            Controls.Add(_gridDetails);
            Controls.Add(headerPanel);

            ConfigureGrid();
        }

        private void ConfigureGrid()
        {
            _gridDetails.UseAppStyling = false;
            _gridDetails.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = _gridDetails.DisplayLayout;
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
            layout.Override.RowSelectorWidth = 40;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

            layout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(69, 90, 100);
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.Override.HeaderAppearance.BackColor = Color.FromArgb(84, 110, 122);
            layout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(96, 125, 139);
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(250, 250, 252);
            layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
            layout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(33, 33, 33);
            layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(66, 165, 245);
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.MinRowHeight = 24;
            layout.Override.DefaultRowHeight = 24;
        }

        private void GridDetails_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
                return;

            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
            }

            ConfigureGridColumn(band, "Source", "Type", 75, null, HAlign.Center);
            ConfigureGridColumn(band, "Reference", "Reference", 130, null, HAlign.Left);
            ConfigureGridColumn(band, "EntryDate", "Date", 95, "dd-MMM-yyyy", HAlign.Left);
            ConfigureGridColumn(band, "DueDate", "Due Date", 95, "dd-MMM-yyyy", HAlign.Left);
            ConfigureGridColumn(band, "Amount", "Amount", 110, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "Adjusted", "Adjusted", 110, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "Balance", "Balance", 110, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "Status", "Status", 95, null, HAlign.Center);
            ConfigureGridColumn(band, "Remarks", "Remarks", 240, null, HAlign.Left);

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void GridDetails_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row.Cells.Exists("Source"))
            {
                string source = Convert.ToString(e.Row.Cells["Source"].Value);
                e.Row.Cells["Source"].Appearance.FontData.Bold = DefaultableBoolean.True;
                e.Row.Cells["Source"].Appearance.ForeColor = string.Equals(source, "Old", StringComparison.OrdinalIgnoreCase)
                    ? Color.FromArgb(25, 118, 210)
                    : Color.FromArgb(191, 54, 12);
            }

            if (e.Row.Cells.Exists("Balance"))
            {
                e.Row.Cells["Balance"].Appearance.FontData.Bold = DefaultableBoolean.True;
                e.Row.Cells["Balance"].Appearance.ForeColor = Color.FromArgb(27, 94, 32);
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
    }

    public class PartyBalanceDetailRowDto
    {
        public string Source { get; set; }
        public string Reference { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal Adjusted { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
    }
}
