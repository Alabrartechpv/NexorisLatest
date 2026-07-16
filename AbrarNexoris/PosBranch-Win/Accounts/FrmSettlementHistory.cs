using ModelClass.Accounts;
using Repository.Accounts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.Accounts
{
    public partial class FrmSettlementHistory : Form
    {
        private readonly int _entryId;
        private readonly ManualPartyBalanceRepository _repository;

        private class SettlementDisplayModel
        {
            public int Id { get; set; }
            public int ManualPartyBalanceId { get; set; }
            public decimal PreviousBalance { get; set; }
            public decimal SettlementAmount { get; set; }
            public decimal RunningBalance { get; set; }
            public DateTime SettlementDate { get; set; }
            public string Remarks { get; set; }
            public int CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        private static readonly Color ClrPrimaryHover = Color.FromArgb(29, 78, 216);
        private static readonly Color ClrDanger = Color.FromArgb(211, 47, 47);
        private static readonly Color ClrDanger2 = Color.FromArgb(244, 67, 54);
        private static readonly Color ClrSurface = Color.White;
        private static readonly Color ClrBg = Color.FromArgb(243, 244, 246);
        private static readonly Color ClrBorder = Color.FromArgb(209, 213, 219);
        private static readonly Color ClrTextPrimary = Color.FromArgb(17, 24, 39);
        private static readonly Color ClrGridHeaderBg = Color.FromArgb(55, 71, 79);
        private static readonly Color ClrGridHeaderBg2 = Color.FromArgb(69, 90, 100);
        private static readonly Color ClrGridAlt = Color.FromArgb(250, 250, 252);
        private static readonly Color ClrGridSel = Color.FromArgb(66, 165, 245);
        private static readonly Color ClrGridHover = Color.FromArgb(227, 242, 253);
        private static readonly Color ClrBtnSlate = Color.FromArgb(84, 110, 122);
        private static readonly Color ClrBtnSlate2 = Color.FromArgb(96, 125, 139);

        public FrmSettlementHistory(int entryId, string partyInfo)
        {
            InitializeComponent();
            _entryId = entryId;
            _repository = new ManualPartyBalanceRepository();
            lblHeader.Text = $"Settlements for: {partyInfo}";

            ApplyStyles();
            gridSettlements.InitializeLayout += GridSettlements_InitializeLayout;
            gridSettlements.InitializeRow += GridSettlements_InitializeRow;
            ConfigureGrid();
            LoadData();

            btnDelete.Click += (s, e) => DeleteSelected();
            btnClose.Click += (s, e) => Close();
        }

        private void ApplyStyles()
        {
            BackColor = ClrBg;
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 9.75f);

            tableLayoutPanel1.BackColor = ClrBg;
            panel1.BackColor = Color.Transparent;
            lblHeader.Appearance.ForeColor = ClrTextPrimary;
            lblHeader.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            lblHeader.Appearance.FontData.SizeInPoints = 12.5f;

            StyleGradientButton(btnDelete, ClrDanger, ClrDanger2, Color.FromArgb(198, 40, 40), Color.FromArgb(239, 83, 80), 132);
            StyleGradientButton(btnClose, ClrBtnSlate, ClrBtnSlate2, Color.FromArgb(69, 90, 100), Color.FromArgb(120, 144, 156), 100);
        }

        private static void StyleGradientButton(
            Infragistics.Win.Misc.UltraButton button,
            Color backColor,
            Color backColor2,
            Color borderColor,
            Color hoverColor,
            int width)
        {
            button.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            button.UseAppStyling = false;
            button.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            button.Size = new Size(width, 36);
            button.Appearance.BackColor = backColor;
            button.Appearance.BackColor2 = backColor2;
            button.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            button.Appearance.FontData.SizeInPoints = 9.5f;
            button.Appearance.BorderColor = borderColor;
            button.HotTrackAppearance.BackColor = hoverColor;
            button.HotTrackAppearance.ForeColor = Color.White;
            button.HotTrackAppearance.BorderColor = borderColor;
        }

        private void StyleGrid(Infragistics.Win.UltraWinGrid.UltraGrid grid)
        {
            grid.UseAppStyling = false;
            grid.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            var layout = grid.DisplayLayout;
            layout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;
            layout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            layout.Appearance.BorderColor = ClrBorder;
            layout.GroupByBox.Hidden = true;

            layout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.Standard;
            layout.Override.HeaderAppearance.BackColor = ClrGridHeaderBg;
            layout.Override.HeaderAppearance.BackColor2 = ClrGridHeaderBg2;
            layout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.FontData.Name = "Segoe UI";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9F;
            layout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            layout.Override.HeaderAppearance.BorderColor = ClrGridHeaderBg2;

            layout.Override.RowAppearance.BackColor = ClrSurface;
            layout.Override.RowAlternateAppearance.BackColor = ClrGridAlt;
            layout.Override.SelectedRowAppearance.BackColor = ClrGridSel;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            layout.Override.ActiveRowAppearance.BackColor = ClrGridHover;
            layout.Override.ActiveRowAppearance.ForeColor = ClrTextPrimary;
            layout.Override.ActiveRowAppearance.BorderColor = ClrGridSel;
            layout.Override.HotTrackRowAppearance.BackColor = ClrGridHover;
            layout.Override.HotTrackRowAppearance.ForeColor = ClrTextPrimary;

            layout.Override.DefaultRowHeight = 30;
            layout.Override.CellAppearance.FontData.SizeInPoints = 9.5f;
            layout.Override.CellAppearance.BorderColor = ClrBorder;
            layout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
            layout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
            layout.Override.RowSelectorAppearance.BackColor = ClrGridHeaderBg2;
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 28;
            layout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.False;
            layout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;
            layout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
            layout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
            layout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
            layout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
        }

        private void ConfigureGrid()
        {
            StyleGrid(gridSettlements);
        }

        private void GridSettlements_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
            {
                return;
            }

            ConfigureGridColumns(e.Layout.Bands[0]);
            e.Layout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;
        }

        private void ConfigureGridColumns(Infragistics.Win.UltraWinGrid.UltraGridBand band)
        {
            foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn column in band.Columns)
            {
                if (column.Key == "Id" || column.Key == "ManualPartyBalanceId" || column.Key == "IsDeleted")
                {
                    column.Hidden = true;
                }
                else
                {
                    column.Hidden = false;
                }
            }

            ConfigureGridColumn(band, "SettlementDate", "Date", 120, "dd-MMM-yyyy");
            ConfigureGridColumn(band, "PreviousBalance", "Balance", 130, "N2", Infragistics.Win.HAlign.Right);
            ConfigureGridColumn(band, "SettlementAmount", "Settlement", 130, "N2", Infragistics.Win.HAlign.Right);
            ConfigureGridColumn(band, "RunningBalance", "Running Balance", 140, "N2", Infragistics.Win.HAlign.Right);
            ConfigureGridColumn(band, "Remarks", "Remarks", 300);
            ConfigureGridColumn(band, "CreatedBy", "User", 100);
            ConfigureGridColumn(band, "CreatedDate", "Created On", 140, "dd-MMM-yyyy HH:mm");
        }

        private static void ConfigureGridColumn(
            Infragistics.Win.UltraWinGrid.UltraGridBand band,
            string key,
            string header,
            int width,
            string format = null,
            Infragistics.Win.HAlign textAlign = Infragistics.Win.HAlign.Left)
        {
            if (!band.Columns.Exists(key))
            {
                return;
            }

            var column = band.Columns[key];
            column.Hidden = false;
            column.Header.Caption = header;
            column.Width = width;
            column.CellAppearance.TextHAlign = textAlign;

            if (!string.IsNullOrEmpty(format))
            {
                column.Format = format;
            }
        }

        private void GridSettlements_InitializeRow(object sender, Infragistics.Win.UltraWinGrid.InitializeRowEventArgs e)
        {
            if (e.Row.Cells.Exists("SettlementAmount"))
            {
                e.Row.Cells["SettlementAmount"].Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                e.Row.Cells["SettlementAmount"].Appearance.ForeColor = ClrPrimaryHover;
            }
            if (e.Row.Cells.Exists("RunningBalance"))
            {
                e.Row.Cells["RunningBalance"].Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                e.Row.Cells["RunningBalance"].Appearance.ForeColor = Color.FromArgb(56, 142, 60);
            }
        }

        private void LoadData()
        {
            var masterEntry = _repository.GetEntryById(_entryId);
            decimal currentBalance = masterEntry?.Amount ?? 0;

            var rawSettlements = _repository.GetSettlements(_entryId);
            var sortedAsc = System.Linq.Enumerable.ToList(System.Linq.Enumerable.ThenBy(System.Linq.Enumerable.OrderBy(rawSettlements, s => s.SettlementDate), s => s.Id));

            var displayList = new List<SettlementDisplayModel>();
            foreach (var s in sortedAsc)
            {
                decimal prevBalance = currentBalance;
                currentBalance -= s.SettlementAmount;
                displayList.Add(new SettlementDisplayModel
                {
                    Id = s.Id,
                    ManualPartyBalanceId = s.ManualPartyBalanceId,
                    PreviousBalance = prevBalance,
                    SettlementAmount = s.SettlementAmount,
                    RunningBalance = currentBalance,
                    SettlementDate = s.SettlementDate,
                    Remarks = s.Remarks,
                    CreatedBy = s.CreatedBy,
                    CreatedDate = s.CreatedDate
                });
            }

            gridSettlements.DataSource = System.Linq.Enumerable.ToList(System.Linq.Enumerable.ThenByDescending(System.Linq.Enumerable.OrderByDescending(displayList, x => x.SettlementDate), x => x.Id));

            if (gridSettlements.DisplayLayout.Bands.Count > 0)
            {
                ConfigureGridColumns(gridSettlements.DisplayLayout.Bands[0]);
            }
        }

        private void DeleteSelected()
        {
            if (gridSettlements.ActiveRow == null || !(gridSettlements.ActiveRow.ListObject is SettlementDisplayModel settlement))
            {
                MessageBox.Show("Please select a settlement to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Delete this settlement record?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _repository.DeleteSettlement(settlement.Id);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
