using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PosBranch_Win.Reports.FinancialReports.FinancialReportGrid;

namespace PosBranch_Win.Reports.FinancialReports
{
    public class frmactivityreport : Form
    {
        internal const string Title = "Activity Report";
        private const string ActivityTable = "Activity";
        private const string DetailsRelation = "ActivityDetails";
        private const string ActivityKey = "ActivityKey";
        private const string Snapshot = "Snapshot";
        private const string ActionLabelColumn = "ActionLabel";
        private enum ReportPeriod
        {
            Range,
            AsOfNow,
            Today,
            Yesterday,
            ThisMonth,
            ThisYear
        }

        private readonly UltraComboEditor period = new UltraComboEditor { Width = 112 };
        private readonly UltraDateTimeEditor fromDate = new UltraDateTimeEditor();
        private readonly UltraDateTimeEditor toDate = new UltraDateTimeEditor();
        private readonly UltraComboEditor branch = new UltraComboEditor { Width = 170 };
        private readonly UltraComboEditor user = new UltraComboEditor { Width = 150 };
        private readonly UltraComboEditor counter = new UltraComboEditor { Width = 150 };
        private readonly UltraComboEditor category = new UltraComboEditor { Width = 125 };
        private readonly UltraComboEditor action = new UltraComboEditor { Width = 135 };
        private readonly UltraTextEditor search = new UltraTextEditor { Width = 280, MaxLength = 200 };
        private readonly UltraGrid gridReport = new UltraGrid { Name = "gridReport", Dock = DockStyle.Fill };
        private readonly UltraPanel gridFooter = new UltraPanel { Dock = DockStyle.Bottom };
        private readonly FinancialReportGrid reportGrid;
        private readonly HashSet<DataRow> requestedDetails = new HashSet<DataRow>();
        private DataSet activityData;
        private string loadedSelection;
        private bool loading;

        public frmactivityreport()
        {
            Name = nameof(frmactivityreport);
            Text = Title;
            BackColor = FormBackColor;
            Font = new Font("Microsoft Sans Serif", 8.25F);
            ClientSize = new Size(1120, 650);
            KeyPreview = true;

            var filterPanel = CreatePanel(DockStyle.Top, FilterPanelBackColor, 74);
            var secondRow = AddFlow(filterPanel, 34);
            var firstRow = AddFlow(filterPanel, 34);
            AddFilter(firstRow, "Period", period);
            AddFilter(firstRow, "From", fromDate);
            AddFilter(firstRow, "To", toDate);
            AddFilter(firstRow, "Branch", branch);
            AddFilter(firstRow, "User", user);
            AddFilter(secondRow, "Category", category);
            AddFilter(secondRow, "Action", action);
            AddFilter(secondRow, "Counter", counter);
            AddFilter(secondRow, "Search", search);
            search.KeyDown += async (sender, e) =>
            {
                if (e.KeyCode != Keys.Enter)
                    return;
                e.SuppressKeyPress = true;
                await LoadReportAsync();
            };

            foreach (UltraDateTimeEditor editor in new[] { fromDate, toDate })
            {
                editor.Width = 125;
                editor.MaskInput = "{date}";
                editor.FormatString = "dd/MM/yyyy";
                StyleDateEditor(editor);
            }

            foreach (UltraComboEditor editor in new[] { period, branch, user, counter, category, action })
                StyleFilterCombo(editor, true);

            period.Items.Add(ReportPeriod.Range, "Range");
            period.Items.Add(ReportPeriod.AsOfNow, "As of Now");
            period.Items.Add(ReportPeriod.Today, "Today");
            period.Items.Add(ReportPeriod.Yesterday, "Yesterday");
            period.Items.Add(ReportPeriod.ThisMonth, "This Month");
            period.Items.Add(ReportPeriod.ThisYear, "This Year");
            period.ValueChanged += (sender, e) => ApplyPeriod();

            category.Items.Add("", "All categories");
            foreach (string name in new[] { "Transaction", "Payment", "Master" })
                category.Items.Add(name, name);
            PopulateOptions(branch, null, "All branches");
            PopulateOptions(user, null, "All users");
            PopulateOptions(counter, null, "All counters");
            PopulateOptions(action, null, "All actions");
            ResetSelection();

            var actionPanel = CreatePanel(DockStyle.Top, ActionPanelBackColor, 47);
            var actions = AddFlow(actionPanel, 43);
            AddAction(actions, "View Grid", async () => await LoadReportAsync());
            AddAction(actions, "Preview Grid", () => Preview(false));
            AddAction(actions, "Preview Report", () => Preview(true));
            AddAction(actions, "Export Grid", ExportGrid);
            AddAction(actions, "Column Chooser", () => reportGrid.ShowColumnChooserForm());
            AddAction(actions, "Reset Filters", async () => await ResetReportAsync());
            var toggle = AddAction(actions, "Hide Selection", null);
            toggle.Click += (sender, e) =>
            {
                filterPanel.Visible = !filterPanel.Visible;
                toggle.Text = filterPanel.Visible ? "Hide Selection" : "View Selection";
            };

            var body = CreatePanel(DockStyle.Fill, FormBackColor, 0);
            body.ClientArea.Controls.Add(gridReport);
            body.ClientArea.Controls.Add(gridFooter);
            Controls.Add(body);
            Controls.Add(actionPanel);
            Controls.Add(filterPanel);

            reportGrid = new FinancialReportGrid(this, gridReport, gridFooter, () => nameof(frmactivityreport));
            reportGrid.UseMatrixFooter("Module", count => $"Total ({count:N0} activities)", null);
            gridReport.InitializeLayout += InitializeGridLayout;
            gridReport.InitializeRow += (sender, e) =>
            {
                if (e.Row.Band.Index == 0)
                    e.Row.ExpansionIndicator = ShowExpansionIndicator.Always;
            };
            gridReport.BeforeRowExpanded += LoadRowDetails;
            gridReport.DoubleClickRow += OpenRowDetails;
            Load += async (sender, e) =>
            {
                reportGrid.Initialize();
                await LoadReportAsync(refreshOptions: true);
            };
            FormClosing += (sender, e) => reportGrid.SaveLayout();
            Disposed += (sender, e) => activityData?.Dispose();
            KeyDown += async (sender, e) =>
            {
                if (e.KeyCode != Keys.F5 && e.KeyCode != Keys.F6 && e.KeyCode != Keys.F7 && e.KeyCode != Keys.F8)
                    return;

                e.Handled = e.SuppressKeyPress = true;
                switch (e.KeyCode)
                {
                    case Keys.F5: await LoadReportAsync(); break;
                    case Keys.F6: Preview(false); break;
                    case Keys.F7: ExportGrid(); break;
                    case Keys.F8: Preview(true); break;
                }
            };
        }

        private void ResetSelection()
        {
            period.Value = ReportPeriod.ThisMonth;
            ApplyPeriod();
            branch.Value = user.Value = counter.Value = category.Value = action.Value = "";
            search.Text = "";
        }

        private void ApplyPeriod()
        {
            if (!(period.Value is ReportPeriod selected))
                return;

            DateTime today = DateTime.Today;
            fromDate.Enabled = toDate.Enabled = selected == ReportPeriod.Range;
            switch (selected)
            {
                case ReportPeriod.Range:
                    if (!(fromDate.Value is DateTime))
                        fromDate.Value = new DateTime(today.Year, today.Month, 1);
                    if (!(toDate.Value is DateTime))
                        toDate.Value = today;
                    break;
                case ReportPeriod.AsOfNow:
                    fromDate.Value = null;
                    toDate.Value = today;
                    break;
                case ReportPeriod.Today:
                    fromDate.Value = toDate.Value = today;
                    break;
                case ReportPeriod.Yesterday:
                    fromDate.Value = toDate.Value = today.AddDays(-1);
                    break;
                case ReportPeriod.ThisMonth:
                    fromDate.Value = new DateTime(today.Year, today.Month, 1);
                    toDate.Value = today;
                    break;
                case ReportPeriod.ThisYear:
                    fromDate.Value = new DateTime(today.Year, 1, 1);
                    toDate.Value = today;
                    break;
            }
        }

        private async Task ResetReportAsync()
        {
            if (loading)
                return;

            ResetSelection();
            reportGrid.ClearFilters();
            await LoadReportAsync(refreshOptions: true);
        }

        private async Task LoadReportAsync(bool refreshOptions = false)
        {
            if (loading)
                return;

            if (!Equals(period.Value, ReportPeriod.Range))
                ApplyPeriod();

            bool asOfNow = Equals(period.Value, ReportPeriod.AsOfNow);
            DateTime? from = asOfNow ? SqlDateTime.MinValue.Value : fromDate.Value as DateTime?;
            if (!from.HasValue || !(toDate.Value is DateTime to) || from.Value.Date > to.Date ||
                from.Value < SqlDateTime.MinValue.Value)
            {
                MessageBox.Show(this, "Select a valid From and To date.", Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? selectedBranch = branch.Value is int branchId ? branchId : (int?)null;
            string selectedUser = user.Value as string;
            string selectedCounter = counter.Value as string;
            string selectedCategory = category.Value as string;
            string selectedAction = action.Value as string;
            string selectedSearch = search.Text.Trim();
            string dateCaption = asOfNow ? "As of Now" : $"{from.Value:dd-MMM-yyyy} to {to:dd-MMM-yyyy}";
            string selection = $"{dateCaption} | {branch.Text} | {category.Text} | {action.Text}";
            loading = UseWaitCursor = true;
            DataSet filters = null;
            DataSet newData = null;

            try
            {
                await Task.Run(() =>
                {
                    using (var repository = new ActivityReportRepository())
                    {
                        if (refreshOptions)
                            filters = repository.GetFilters();

                        using (DataSet data = repository.GetActivity(from.Value, to, selectedBranch,
                            selectedUser, selectedCounter, selectedCategory, selectedSearch, selectedAction, asOfNow))
                            newData = AddActivityDetails(data);
                    }
                });

                if (IsDisposed || Disposing)
                    return;

                if (filters != null)
                {
                    PopulateOptions(branch, filters.Tables[0], "All branches");
                    PopulateOptions(user, filters.Tables[1], "All users");
                    PopulateOptions(counter, filters.Tables[2], "All counters");
                    PopulateOptions(action, filters.Tables[3], "All actions", ActivityReportText.ActionLabel);
                }

                DataSet previous = activityData;
                activityData = newData;
                requestedDetails.Clear();
                newData = null;
                gridReport.SetDataBinding(activityData, ActivityTable);
                reportGrid.DataBound();
                loadedSelection = selection;
                previous?.Dispose();
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.Show(this, "Unable to load activity. " + ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                filters?.Dispose();
                newData?.Dispose();
                loading = false;
                if (!IsDisposed && !Disposing)
                    UseWaitCursor = false;
            }
        }

        private static void PopulateOptions(UltraComboEditor editor, DataTable options, string allCaption,
            Func<string, string> formatCaption = null)
        {
            object selected = editor.Value;
            editor.Items.Clear();
            editor.Items.Add("", allCaption);
            if (options != null)
            {
                foreach (DataRow option in options.Rows)
                {
                    object key = option[0];
                    string caption = Convert.ToString(option[option.Table.Columns.Count - 1]);
                    editor.Items.Add(key, formatCaption == null ? caption : formatCaption(caption));
                }
            }
            editor.Value = editor.Items.Cast<ValueListItem>().Any(item => Equals(item.DataValue, selected)) ? selected : "";
        }

        private static DataSet AddActivityDetails(DataSet result)
        {
            DataTable rows = result.Tables[0];
            var images = result.Tables[1].AsEnumerable().ToLookup(row => row.Field<Guid>("AnchorId"));
            result.Tables.Remove(rows);
            rows.TableName = ActivityTable;
            rows.Columns.Add(ActivityKey, typeof(int));
            rows.Columns.Add(Snapshot, typeof(string));
            rows.Columns.Add(ActionLabelColumn, typeof(string));
            var details = new DataTable("ActivityDetail");
            details.Columns.Add(ActivityKey, typeof(int));
            details.Columns.Add("Section", typeof(string));
            details.Columns.Add("Line", typeof(long));
            details.Columns.Add("Record", typeof(string));
            details.Columns.Add("Details", typeof(string));

            int key = 0;
            foreach (DataRow row in rows.Rows)
            {
                row[ActivityKey] = ++key;
                string module = Convert.ToString(row["Module"]);
                string action = Convert.ToString(row["ActionType"]);
                string source = Convert.ToString(row["Source"]);
                string snapshot = Convert.ToString(row["Details"]);
                string document = $"{module} {row["DocumentNo"]}".Trim();
                row[ActionLabelColumn] = ActivityReportText.ActionLabel(action);
                row[Snapshot] = ActivityReportText.Summary(action, module, snapshot, row["Amount"]);
                if (source == "POS_ItemMasterStatusRules")
                    row[Snapshot] = $"{row[ActionLabelColumn]} · Item status";

                IEnumerable<DataRow> captured = row["AuditGroupId"] is Guid group
                    ? images[group] : Enumerable.Empty<DataRow>();
                var changes = ActivityReportText.CapturedChanges(captured).ToList();

                if (module == "Item Master" && action == "SAVE")
                {
                    var savedDetails = ActivityReportText.SavedItemDetails(snapshot, captured, document);
                    if (savedDetails != null)
                        changes.Insert(0, savedDetails);
                }
                else if (source == "Item activity log" && action == "UPDATE")
                {
                    string classification = ActivityReportText.ItemClassificationChanges(snapshot);
                    if (classification.Length > 0)
                        changes.Add(new ActivityReportText.Detail("Item details", null, document, classification));
                }

                foreach (ActivityReportText.Detail change in changes)
                {
                    string section = action == "SAVE" &&
                        (change.Section == "Changed" || change.Section == "Updated values")
                        ? "Save details" : change.Section;
                    AddDetail(details, key, change, section);
                }

                if (action == "DELETE" && changes.Any(change => change.Text.Contains("Cancelled No → Yes")))
                    row[Snapshot] = $"{row[Snapshot]} · Cancelled";

                if (action == "UPDATE")
                {
                    string summary = string.Join(" · ", changes.Where(change => change.Text.Contains(" → "))
                        .Select(change => change.Text).Take(2));
                    if (summary.Length > 0)
                        row[Snapshot] = summary;
                }

                bool applicationLog = source == "Activity log" || source == "Item activity log";
                // Use the application notes for history without snapshots, or when they can explain an edit.
                string notes = applicationLog && changes.Count == 0
                    ? ActivityReportText.ReadLog(snapshot) : string.Empty;
                if (!string.IsNullOrWhiteSpace(notes))
                    details.Rows.Add(key, "Recorded notes", DBNull.Value, document, notes);
                else if (changes.Count == 0)
                    details.Rows.Add(key, "Activity", DBNull.Value, document, row[Snapshot]);
            }

            var data = new DataSet();
            data.Tables.Add(rows);
            data.Tables.Add(details);
            data.Relations.Add(DetailsRelation, rows.Columns[ActivityKey], details.Columns[ActivityKey], false);
            return data;
        }

        private static void AddDetail(DataTable table, int key, ActivityReportText.Detail detail, string section = null)
        {
            table.Rows.Add(key, section ?? detail.Section, (object)detail.Line ?? DBNull.Value, detail.Record, detail.Text);
        }

        private async void LoadRowDetails(object sender, CancelableRowEventArgs e)
        {
            if (e.Row.Band.Index != 0 || !(e.Row.ListObject is DataRowView activity))
                return;

            DataSet dataset = activity.Row.Table.DataSet;
            if (!ReferenceEquals(dataset, activityData))
                return;

            DataTable detailRows = dataset.Relations[DetailsRelation].ChildTable;
            int key = (int)activity[ActivityKey];
            object document = activity["DocumentNo"];
            string actionType = Convert.ToString(activity["ActionType"]);
            if (!activity.Row.IsNull("AuditGroupId") &&
                (actionType == "UPDATE" || activity.Row.GetChildRows(DetailsRelation)
                    .Any(detail => detail.Field<string>("Section") != "Activity")))
                return;

            if (!requestedDetails.Add(activity.Row))
                return;

            DataRow status = activity.Row.GetChildRows(DetailsRelation)
                .FirstOrDefault(row => (string)row["Section"] == "Lookup failed");
            if (status == null)
                status = detailRows.Rows.Add(key, "Bill details", DBNull.Value, document, "Loading...");
            else
            {
                status["Section"] = "Bill details";
                status["Details"] = "Loading...";
            }
            string module = Convert.ToString(activity["Module"]);
            string source = Convert.ToString(activity["Source"]);
            string docNo = Convert.ToString(document);
            DateTime activityDate = (DateTime)activity["ActivityDate"];
            int? branchId = activity.Row.Field<int?>("EventBranchId");
            int? finYearId = activity.Row.Field<int?>("EventFinYearId");
            int? counterId = activity.Row.Field<int?>("EventCounterId");

            try
            {
                using (DataTable current = await Task.Run(() =>
                {
                    using (var repository = new ActivityReportRepository())
                        return repository.GetDetails(branchId, finYearId, counterId, module, docNo, source,
                            actionType, activityDate);
                }))
                {
                    if (IsDisposed || Disposing || !ReferenceEquals(dataset, activityData))
                        return;

                    if (current.Rows.Count == 0)
                    {
                        status["Details"] = "No matching bill or record is available on file.";
                    }
                    else
                    {
                        var recorded = activity.Row.GetChildRows(DetailsRelation);
                        bool postingsRecorded = recorded.Any(detail => detail.Field<string>("Section") == "Ledger posting");
                        bool statusRecorded = recorded.Any(detail => detail.Field<string>("Section") == "Item status");
                        var currentDetails = current.AsEnumerable().Select(ActivityReportText.CurrentDetail)
                            .Where(detail => detail != null &&
                                !(postingsRecorded && detail.Section == "Current postings") &&
                                !(statusRecorded && detail.Section == "Current item status"))
                            .GroupBy(detail => detail.Key).Select(group => group.First());
                        foreach (ActivityReportText.Detail detail in currentDetails)
                            AddDetail(detailRows, key, detail);
                        detailRows.Rows.Remove(status);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing && ReferenceEquals(dataset, activityData))
                {
                    requestedDetails.Remove(activity.Row);
                    status["Section"] = "Lookup failed";
                    status["Details"] = "Couldn’t load the record. Reopen this row to try again.";
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }

        private void OpenRowDetails(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow)
                return;

            if (e.Row.Band.Index == 0)
            {
                e.Row.Expanded = !e.Row.Expanded;
                return;
            }

            using (var viewer = new Form
            {
                Text = Convert.ToString(e.Row.Cells["Section"].Value),
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(800, 520)
            })
            using (var text = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Text = Convert.ToString(e.Row.Cells["Details"].Value)
            })
            {
                viewer.Controls.Add(text);
                viewer.ShowDialog(this);
            }
        }

        private void InitializeGridLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0 || !e.Layout.Bands[0].Columns.Exists("ActivityDate"))
                return;

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            var main = e.Layout.Bands[0];
            foreach (string key in new[] { ActivityKey, "RowVersion", "Details", "Source", "ActionType", "EventBranchId", "EventFinYearId", "EventCounterId", "AuditGroupId" })
                HideInternalColumn(main.Columns[key]);
            int position = 0;
            void Column(string key, string caption, int width, string format = null, HAlign align = HAlign.Left)
            {
                reportGrid.ConfigureGridColumn(main, key, caption, width, format, align, position++);
            }

            Column("ActivityDate", "Date and Time", 147, "dd-MMM-yyyy HH:mm:ss");
            Column("Module", "Form / Module", 184);
            Column(ActionLabelColumn, "Action", 95);
            Column("DocumentNo", "Document / ID", 110);
            Column("Category", "Category", 95);
            Column("BranchName", "Branch", 145);
            Column("UserName", "User", 125);
            Column("CounterName", "Counter", 125);
            Column("Amount", "Amount", 105, "N2", HAlign.Right);
            Column("DocumentStatus", "Status at Event", 110);
            Column(Snapshot, "Snapshot", 385);
            main.Columns["Module"].CellAppearance.FontData.Bold = DefaultableBoolean.True;

            if (e.Layout.Bands.Count > 1)
            {
                var detail = e.Layout.Bands[1];
                HideInternalColumn(detail.Columns[ActivityKey]);
                reportGrid.ConfigureGridColumn(detail, "Section", "Detail", 145, null, HAlign.Left, 0);
                reportGrid.ConfigureGridColumn(detail, "Line", "Line", 55, null, HAlign.Right, 1);
                reportGrid.ConfigureGridColumn(detail, "Record", "Record / Item", 190, null, HAlign.Left, 2);
                reportGrid.ConfigureGridColumn(detail, "Details", "Details", 730, null, HAlign.Left, 3);
                detail.Columns["Details"].CellMultiLine = DefaultableBoolean.True;
                detail.Columns["Details"].Header.ToolTipText = "Double-click a detail row to read its full text.";
                detail.Override.RowSizing = RowSizing.AutoFree;
                detail.Override.RowSizingAutoMaxLines = 8;
            }

            reportGrid.ApplySavedLayout();
        }

        private static void HideInternalColumn(UltraGridColumn column)
        {
            column.Hidden = true;
            column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
        }

        private bool HasVisibleRows()
        {
            if (reportGrid.GetVisibleDataRows().Any())
                return true;

            MessageBox.Show(this, "No activities match the current selection.", Title,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private void Preview(bool includeReportHeader)
        {
            if (!HasVisibleRows())
                return;

            try
            {
                using (var document = new UltraGridPrintDocument { Grid = gridReport })
                using (var dialog = new PrintPreviewDialog { Document = document, WindowState = FormWindowState.Maximized })
                {
                    document.DefaultPageSettings.Landscape = true;
                    if (includeReportHeader)
                    {
                        document.Header.TextLeft = Title;
                        document.Header.TextRight = loadedSelection;
                        document.Footer.TextLeft = $"Activities {reportGrid.GetVisibleDataRows().Count():N0}";
                    }
                    document.Footer.TextRight = "Page [Page #]";
                    dialog.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Preview failed. " + ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportGrid()
        {
            if (!HasVisibleRows())
                return;

            using (var dialog = new SaveFileDialog
            {
                Filter = ReportPdfExporter.CsvAndPdfFilter,
                FileName = $"ActivityReport_{DateTime.Today:yyyyMMdd}.csv"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    if (ReportPdfExporter.CheckAndExportPdf(dialog, gridReport, Title, loadedSelection))
                        return;

                    var columns = gridReport.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>()
                        .Where(column => !column.Hidden && !column.IsChaptered)
                        .OrderBy(column => column.Header.VisiblePosition).ToArray();

                    if (columns.Length == 0)
                        throw new InvalidOperationException("Show at least one column before exporting.");

                    using (var writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true)))
                    {
                        writer.WriteLine(string.Join(",", columns.Select(column => CsvValue(column.Header.Caption))));
                        foreach (UltraGridRow row in reportGrid.GetVisibleDataRows())
                            writer.WriteLine(string.Join(",", columns.Select(column => CsvValue(row.Cells[column.Key].Value))));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Export failed. " + ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string CsvValue(object value)
        {
            string text = value is DateTime date ? date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                : Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
            string trimmed = text.TrimStart();
            if (value is string && trimmed.Length > 0 && "=+-@".IndexOf(trimmed[0]) >= 0)
                text = "'" + text;
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        private static UltraPanel CreatePanel(DockStyle dock, Color color, int height)
        {
            var panel = new UltraPanel { Dock = dock, BorderStyle = UIElementBorderStyle.Solid };
            if (height > 0)
                panel.Height = height;
            panel.Appearance.BackColor = color;
            panel.Appearance.BorderColor = BorderBlue;
            return panel;
        }

        private static FlowLayoutPanel AddFlow(UltraPanel panel, int height)
        {
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = height,
                Padding = new Padding(8, 4, 0, 0),
                WrapContents = false
            };
            panel.ClientArea.Controls.Add(flow);
            return flow;
        }

        private static void AddFilter(FlowLayoutPanel row, string caption, Control editor)
        {
            var label = new UltraLabel { Text = caption, AutoSize = true, Margin = new Padding(3, 7, 2, 0) };
            StyleLabel(label);
            row.Controls.Add(label);
            row.Controls.Add(editor);
        }

        private static UltraButton AddAction(FlowLayoutPanel row, string caption, Action action)
        {
            var button = new UltraButton { Text = caption, Width = 110, Height = 29 };
            StyleClassicButton(button);
            if (action != null)
                button.Click += (sender, e) => action();
            row.Controls.Add(button);
            return button;
        }
    }
}
