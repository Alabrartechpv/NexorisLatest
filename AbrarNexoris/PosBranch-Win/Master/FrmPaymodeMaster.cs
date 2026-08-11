using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using Repository.MasterRepositry;

namespace PosBranch_Win.Master
{
    public partial class FrmPaymodeMaster : Form
    {
        private readonly PaymodeRepository paymodeRepo = new PaymodeRepository();
        private readonly LedgerRepository ledgerRepo = new LedgerRepository();
        private DataTable dtPaymodes;
        private List<AccountLedgerDDL> ledgersList;

        public FrmPaymodeMaster()
        {
            InitializeComponent();
        }

        private void FrmPaymodeMaster_Load(object sender, EventArgs e)
        {
            try
            {
                SetupGridFormatting();
                LoadLedgersList();
                LoadPaymodeData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Paymode Master: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupGridFormatting()
        {
            ultraGridPaymode.DisplayLayout.Reset();
            ultraGridPaymode.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGridPaymode.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGridPaymode.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
            ultraGridPaymode.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGridPaymode.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            ultraGridPaymode.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            ultraGridPaymode.DisplayLayout.GroupByBox.Hidden = true;
            ultraGridPaymode.DisplayLayout.Override.MinRowHeight = 36;
            ultraGridPaymode.DisplayLayout.Override.DefaultRowHeight = 36;

            ultraGridPaymode.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
            ultraGridPaymode.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(0, 122, 204);
            ultraGridPaymode.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(0, 90, 160);
            ultraGridPaymode.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGridPaymode.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraGridPaymode.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGridPaymode.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Segoe UI";
            ultraGridPaymode.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 10f;

            ultraGridPaymode.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            ultraGridPaymode.DisplayLayout.Override.RowAppearance.FontData.Name = "Segoe UI";
            ultraGridPaymode.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 9.5f;

            ultraGridPaymode.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 249, 250);
            ultraGridPaymode.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
            ultraGridPaymode.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.Black;

            ultraGridPaymode.InitializeLayout += UltraGridPaymode_InitializeLayout;
            ultraGridPaymode.ClickCellButton += UltraGridPaymode_ClickCellButton;
            ultraGridPaymode.DoubleClickCell += UltraGridPaymode_DoubleClickCell;
        }

        private void UltraGridPaymode_ClickCellButton(object sender, CellEventArgs e)
        {
            if (e.Cell != null && e.Cell.Column.Key == "LedgerID")
            {
                OpenLedgerSearchForCell(e.Cell);
            }
        }

        private void UltraGridPaymode_DoubleClickCell(object sender, DoubleClickCellEventArgs e)
        {
            if (e.Cell != null && e.Cell.Column.Key == "LedgerID")
            {
                OpenLedgerSearchForCell(e.Cell);
            }
        }

        private void OpenLedgerSearchForCell(UltraGridCell cell)
        {
            try
            {
                using (var searchForm = new PosBranch_Win.DialogBox.FrmLedgerSearch())
                {
                    if (searchForm.ShowDialog(this) == DialogResult.OK && searchForm.SelectedLedgerId > 0)
                    {
                        cell.Value = searchForm.SelectedLedgerId;
                        ultraGridPaymode.UpdateData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Ledger Search: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLedgersList()
        {
            try
            {
                var request = new AccountLedgerDDLRequest
                {
                    BranchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : 1,
                    For = "ALL"
                };
                var gridResult = ledgerRepo.getAccountLedgerDDL(request);
                ledgersList = gridResult?.List?.ToList() ?? new List<AccountLedgerDDL>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading ledgers list: {ex.Message}");
                ledgersList = new List<AccountLedgerDDL>();
            }
        }

        private void LoadPaymodeData()
        {
            try
            {
                List<PaymodeModel> list = paymodeRepo.GetAllPaymodes();
                if (list == null) list = new List<PaymodeModel>();

                // Find CASH-IN-HAND ledger ID from ledgersList if available
                int cashInHandLedgerId = 0;
                if (ledgersList != null && ledgersList.Count > 0)
                {
                    var cashLedger = ledgersList.FirstOrDefault(l =>
                        string.Equals(l.Name, "CASH-IN-HAND", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(l.Name, "CASH", StringComparison.OrdinalIgnoreCase));
                    if (cashLedger != null)
                    {
                        cashInHandLedgerId = cashLedger.Id;
                    }
                }

                foreach (var pm in list)
                {
                    if (string.Equals(pm.PayModeName, "Credit", StringComparison.OrdinalIgnoreCase))
                    {
                        // Credit paymode uses customer ledger dynamically at sale time, so LedgerID is always 0 (unselected)
                        pm.LedgerID = 0;
                    }
                    else if (string.Equals(pm.PayModeName, "Cash", StringComparison.OrdinalIgnoreCase))
                    {
                        if (pm.LedgerID <= 0 && cashInHandLedgerId > 0)
                        {
                            pm.LedgerID = cashInHandLedgerId;
                        }
                    }
                }

                dtPaymodes = new DataTable();
                dtPaymodes.Columns.Add("PayModeID", typeof(int));
                dtPaymodes.Columns.Add("PayModeName", typeof(string));
                dtPaymodes.Columns.Add("LedgerID", typeof(int));

                foreach (var item in list)
                {
                    dtPaymodes.Rows.Add(item.PayModeID, item.PayModeName, item.LedgerID);
                }

                ultraGridPaymode.DataSource = dtPaymodes;
                ultraGridPaymode.DataBind();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching paymodes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraGridPaymode_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];

            foreach (var col in band.Columns)
            {
                col.Hidden = true;
            }

            if (band.Columns.Exists("PayModeID"))
            {
                band.Columns["PayModeID"].Hidden = false;
                band.Columns["PayModeID"].Header.Caption = "ID";
                band.Columns["PayModeID"].Width = 60;
                band.Columns["PayModeID"].CellActivation = Activation.NoEdit;
                band.Columns["PayModeID"].CellAppearance.TextHAlign = HAlign.Center;
                band.Columns["PayModeID"].Header.Appearance.TextHAlign = HAlign.Center;
            }

            if (band.Columns.Exists("PayModeName"))
            {
                band.Columns["PayModeName"].Hidden = false;
                band.Columns["PayModeName"].Header.Caption = "Payment Mode";
                band.Columns["PayModeName"].Width = 220;
                band.Columns["PayModeName"].CellActivation = Activation.NoEdit;
                band.Columns["PayModeName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                band.Columns["PayModeName"].Header.Appearance.TextHAlign = HAlign.Left;
            }

            if (band.Columns.Exists("LedgerID"))
            {
                var col = band.Columns["LedgerID"];
                col.Hidden = false;
                col.Header.Caption = "Mapped Chart of Accounts Ledger (Double-Click or [...] to Search)";
                col.Width = 460;
                col.CellActivation = Activation.AllowEdit;
                col.Header.Appearance.TextHAlign = HAlign.Left;
                col.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.EditButton;
                col.ButtonDisplayStyle = Infragistics.Win.UltraWinGrid.ButtonDisplayStyle.Always;

                ValueList vl = e.Layout.ValueLists.Exists("LedgersValueList")
                    ? e.Layout.ValueLists["LedgersValueList"]
                    : e.Layout.ValueLists.Add("LedgersValueList");

                vl.ValueListItems.Clear();
                vl.ValueListItems.Add(0, "-- Select Account Ledger --");

                if (ledgersList != null && ledgersList.Count > 0)
                {
                    foreach (var l in ledgersList)
                    {
                        vl.ValueListItems.Add(l.Id, l.Name);
                    }
                }

                col.ValueList = vl;
            }

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                ultraGridPaymode.UpdateData();
                int updatedCount = 0;

                foreach (DataRow row in dtPaymodes.Rows)
                {
                    int paymodeId = Convert.ToInt32(row["PayModeID"]);
                    string paymodeName = row["PayModeName"]?.ToString() ?? "";
                    int ledgerId = row["LedgerID"] != DBNull.Value ? Convert.ToInt32(row["LedgerID"]) : 0;

                    PaymodeModel model = paymodeRepo.GetPaymodeById(paymodeId);
                    if (model == null)
                    {
                        model = new PaymodeModel { PayModeID = paymodeId, PayModeName = paymodeName };
                    }

                    if (string.Equals(paymodeName, "Credit", StringComparison.OrdinalIgnoreCase))
                    {
                        model.LedgerID = 0; // Credit sales post to the selected Customer's ledger dynamically at sale time
                    }
                    else
                    {
                        model.LedgerID = ledgerId;
                    }

                    paymodeRepo.SavePaymode(model);
                    updatedCount++;
                }

                MessageBox.Show($"Paymode account mappings saved successfully! ({updatedCount} rows updated)",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadPaymodeData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving paymode mappings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadLedgersList();
            LoadPaymodeData();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
