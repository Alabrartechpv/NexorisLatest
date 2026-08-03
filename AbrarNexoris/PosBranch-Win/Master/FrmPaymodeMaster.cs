using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using Repository;

namespace PosBranch_Win.Master
{
    public partial class FrmPaymodeMaster : Form
    {
        private DataTable dtPaymodes;
        private DataTable dtLedgers;

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
                var baseRepo = new BaseRepostitory();
                if (baseRepo.DataConnection.State == ConnectionState.Open)
                    baseRepo.DataConnection.Close();

                baseRepo.DataConnection.Open();

                string sql = @"
                    SELECT LedgerID, LedgerName 
                    FROM LedgerMaster 
                    ORDER BY LedgerName";

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)baseRepo.DataConnection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        dtLedgers = new DataTable();
                        da.Fill(dtLedgers);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading ledgers list: {ex.Message}");
            }
            finally
            {
                var baseRepo = new BaseRepostitory();
                if (baseRepo.DataConnection.State == ConnectionState.Open)
                    baseRepo.DataConnection.Close();
            }
        }

        private void LoadPaymodeData()
        {
            try
            {
                var baseRepo = new BaseRepostitory();
                if (baseRepo.DataConnection.State == ConnectionState.Open)
                    baseRepo.DataConnection.Close();

                baseRepo.DataConnection.Open();

                string sql = "SELECT * FROM PayMode";

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)baseRepo.DataConnection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        dtPaymodes = new DataTable();
                        da.Fill(dtPaymodes);
                    }
                }

                if (!dtPaymodes.Columns.Contains("LedgerID"))
                {
                    dtPaymodes.Columns.Add("LedgerID", typeof(int));
                }

                foreach (DataRow row in dtPaymodes.Rows)
                {
                    if (row["LedgerID"] == DBNull.Value)
                    {
                        row["LedgerID"] = 0;
                    }
                }

                ultraGridPaymode.DataSource = dtPaymodes;
                ultraGridPaymode.DataBind();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching paymodes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                var baseRepo = new BaseRepostitory();
                if (baseRepo.DataConnection.State == ConnectionState.Open)
                    baseRepo.DataConnection.Close();
            }
        }

        private string GetPaymodeIdColumnName()
        {
            if (dtPaymodes == null) return "PaymodeID";
            if (dtPaymodes.Columns.Contains("PaymodeID")) return "PaymodeID";
            if (dtPaymodes.Columns.Contains("PayModeID")) return "PayModeID";
            if (dtPaymodes.Columns.Contains("ID")) return "ID";
            return dtPaymodes.Columns[0].ColumnName;
        }

        private string GetPaymodeNameColumnName()
        {
            if (dtPaymodes == null) return "PaymodeName";
            if (dtPaymodes.Columns.Contains("PayModeName")) return "PayModeName";
            if (dtPaymodes.Columns.Contains("PaymodeName")) return "PaymodeName";
            if (dtPaymodes.Columns.Contains("PayMode")) return "PayMode";
            if (dtPaymodes.Columns.Contains("Paymode")) return "Paymode";
            return dtPaymodes.Columns.Count > 1 ? dtPaymodes.Columns[1].ColumnName : dtPaymodes.Columns[0].ColumnName;
        }

        private void UltraGridPaymode_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];
            string idCol = GetPaymodeIdColumnName();
            string nameCol = GetPaymodeNameColumnName();

            foreach (var col in band.Columns)
            {
                col.Hidden = true;
            }

            if (band.Columns.Exists(idCol))
            {
                band.Columns[idCol].Hidden = false;
                band.Columns[idCol].Header.Caption = "ID";
                band.Columns[idCol].Width = 60;
                band.Columns[idCol].CellActivation = Activation.NoEdit;
                band.Columns[idCol].CellAppearance.TextHAlign = HAlign.Center;
                band.Columns[idCol].Header.Appearance.TextHAlign = HAlign.Center;
            }

            if (band.Columns.Exists(nameCol))
            {
                band.Columns[nameCol].Hidden = false;
                band.Columns[nameCol].Header.Caption = "Payment Mode";
                band.Columns[nameCol].Width = 220;
                band.Columns[nameCol].CellActivation = Activation.NoEdit;
                band.Columns[nameCol].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                band.Columns[nameCol].Header.Appearance.TextHAlign = HAlign.Left;
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

                if (dtLedgers != null && dtLedgers.Rows.Count > 0)
                {
                    foreach (DataRow r in dtLedgers.Rows)
                    {
                        int id = Convert.ToInt32(r["LedgerID"]);
                        string name = r["LedgerName"].ToString();
                        vl.ValueListItems.Add(id, name);
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
                var baseRepo = new BaseRepostitory();
                if (baseRepo.DataConnection.State == ConnectionState.Open)
                    baseRepo.DataConnection.Close();

                baseRepo.DataConnection.Open();

                string idCol = GetPaymodeIdColumnName();
                int updatedCount = 0;

                foreach (DataRow row in dtPaymodes.Rows)
                {
                    if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Unchanged)
                    {
                        int paymodeId = Convert.ToInt32(row[idCol]);
                        int ledgerId = row["LedgerID"] != DBNull.Value ? Convert.ToInt32(row["LedgerID"]) : 0;

                        string updateSql = $"UPDATE PayMode SET LedgerID = @LedgerID WHERE {idCol} = @PaymodeID";
                        using (SqlCommand cmd = new SqlCommand(updateSql, (SqlConnection)baseRepo.DataConnection))
                        {
                            cmd.Parameters.AddWithValue("@LedgerID", ledgerId > 0 ? (object)ledgerId : DBNull.Value);
                            cmd.Parameters.AddWithValue("@PaymodeID", paymodeId);
                            cmd.ExecuteNonQuery();
                            updatedCount++;
                        }
                    }
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
            finally
            {
                var baseRepo = new BaseRepostitory();
                if (baseRepo.DataConnection.State == ConnectionState.Open)
                    baseRepo.DataConnection.Close();
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
