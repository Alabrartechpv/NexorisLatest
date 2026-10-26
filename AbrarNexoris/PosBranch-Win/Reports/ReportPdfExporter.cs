using Infragistics.Win.UltraWinGrid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports
{
    /// <summary>
    /// Pure C# PDF Document Generator for Reports.
    /// Exports UltraGrid, DataTable, or CSV content directly into clean, professional A4 vector PDF documents
    /// with zero third-party dependencies.
    /// </summary>
    public static class ReportPdfExporter
    {
        public const string CsvAndPdfFilter = "CSV Files (*.csv)|*.csv|PDF Documents (*.pdf)|*.pdf";
        public const string ExcelCsvPdfFilter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv|PDF Documents (*.pdf)|*.pdf";
        public const string ExcelAndPdfFilter = "Excel Files (*.xlsx)|*.xlsx|Excel Files (*.xls)|*.xls|PDF Documents (*.pdf)|*.pdf";

        #region Public Check & Export Helpers
        /// <summary>
        /// Checks if the selected file path in SaveFileDialog is a PDF. If so, exports the UltraGrid to PDF.
        /// </summary>
        public static bool CheckAndExportPdf(SaveFileDialog sfd, UltraGrid grid, string reportTitle, string subtitle = null, Dictionary<string, string> footers = null)
        {
            if (sfd == null || string.IsNullOrEmpty(sfd.FileName)) return false;
            if (sfd.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ExportGridToPdf(grid, sfd.FileName, reportTitle, subtitle, footers);
                MessageBox.Show($"Report exported successfully to PDF:\n{sfd.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the selected file path in SaveFileDialog is a PDF. If so, exports the DataTable to PDF.
        /// </summary>
        public static bool CheckAndExportPdf(SaveFileDialog sfd, DataTable table, string reportTitle, string subtitle = null)
        {
            if (sfd == null || string.IsNullOrEmpty(sfd.FileName)) return false;
            if (sfd.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ExportDataTableToPdf(table, sfd.FileName, reportTitle, subtitle);
                MessageBox.Show($"Report exported successfully to PDF:\n{sfd.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the selected file path in SaveFileDialog is a PDF. If so, exports CSV content to PDF.
        /// </summary>
        public static bool CheckAndExportPdf(SaveFileDialog sfd, string csvContent, string reportTitle, string subtitle = null)
        {
            if (sfd == null || string.IsNullOrEmpty(sfd.FileName)) return false;
            if (sfd.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ExportCsvToPdf(csvContent, sfd.FileName, reportTitle, subtitle);
                MessageBox.Show($"Report exported successfully to PDF:\n{sfd.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            return false;
        }
        #endregion

        #region UltraGrid Export
        public static void ExportGridToPdf(UltraGrid grid, string filePath, string reportTitle, string subtitle = null, Dictionary<string, string> footers = null)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            // Extract visible columns from Band 0
            var masterBand = grid.DisplayLayout.Bands[0];
            var visibleCols = masterBand.Columns
                .Cast<UltraGridColumn>()
                .Where(c => !c.Hidden && !c.IsChaptered && !string.Equals(c.Key, "BillDetails", StringComparison.OrdinalIgnoreCase) && !string.Equals(c.Key, "DetailID", StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.Header.VisiblePosition)
                .ToList();

            if (visibleCols.Count == 0)
            {
                throw new InvalidOperationException("No visible columns found to export.");
            }

            var colHeaders = visibleCols.Select(c => !string.IsNullOrEmpty(c.Header.Caption) ? c.Header.Caption : c.Key).ToList();
            var colAlignments = visibleCols.Select(c => GetColumnAlignment(c)).ToList();
            var colWidthWeights = visibleCols.Select(c => Math.Max(c.Width, 50)).ToList();

            // Extract data rows
            var dataRows = new List<PdfRowData>();
            bool hasChildBands = grid.DisplayLayout.Bands.Count > 1;

            foreach (var gridRow in grid.Rows.GetFilteredInNonGroupByRows())
            {
                if (gridRow.Hidden) continue;

                var rowValues = new List<string>();
                foreach (var col in visibleCols)
                {
                    object val = gridRow.Cells.Exists(col.Key) ? gridRow.Cells[col.Key].Value : null;
                    rowValues.Add(FormatCellValue(val, col));
                }

                var rowData = new PdfRowData { Cells = rowValues, IsMaster = true };

                // Check child bands for hierarchical master-detail rows
                if (hasChildBands && gridRow.ChildBands != null && gridRow.ChildBands.Count > 0)
                {
                    var childBand = gridRow.ChildBands[0];
                    if (childBand.Rows.Count > 0)
                    {
                        var detailCols = childBand.Band.Columns
                            .Cast<UltraGridColumn>()
                            .Where(c => !c.Hidden && !c.IsChaptered && !string.Equals(c.Key, "DetailID", StringComparison.OrdinalIgnoreCase) && !string.Equals(c.Key, "BillNo", StringComparison.OrdinalIgnoreCase))
                            .OrderBy(c => c.Header.VisiblePosition)
                            .ToList();

                        foreach (var cRow in childBand.Rows)
                        {
                            var dValues = new List<string>();
                            foreach (var dCol in detailCols)
                            {
                                object dVal = cRow.Cells.Exists(dCol.Key) ? cRow.Cells[dCol.Key].Value : null;
                                dValues.Add(FormatCellValue(dVal, dCol));
                            }
                            string childSummary = string.Join(" | ", detailCols.Select((dc, idx) => $"{dc.Header.Caption}: {dValues[idx]}"));
                            rowData.ChildRows.Add(childSummary);
                        }
                    }
                }

                dataRows.Add(rowData);
            }

            // Extract footer totals if available
            List<string> footerValues = null;
            if (footers != null && footers.Count > 0)
            {
                footerValues = new List<string>();
                for (int i = 0; i < visibleCols.Count; i++)
                {
                    string colKey = visibleCols[i].Key;
                    if (footers.TryGetValue(colKey, out var fVal))
                    {
                        footerValues.Add(fVal);
                    }
                    else if (i == 0)
                    {
                        footerValues.Add("TOTAL");
                    }
                    else
                    {
                        footerValues.Add(string.Empty);
                    }
                }
            }

            GeneratePdfFile(filePath, reportTitle, subtitle, colHeaders, colAlignments, colWidthWeights, dataRows, footerValues);
        }

        private static TextAlignment GetColumnAlignment(UltraGridColumn col)
        {
            if (col.CellAppearance.TextHAlign == Infragistics.Win.HAlign.Right)
                return TextAlignment.Right;
            if (col.CellAppearance.TextHAlign == Infragistics.Win.HAlign.Center)
                return TextAlignment.Center;

            string key = col.Key.ToLowerInvariant();
            if (key.Contains("slno") || key.Contains("sno") || key.Contains("date") || key.Contains("code") || key.Contains("status"))
                return TextAlignment.Center;
            if (key.Contains("amount") || key.Contains("total") || key.Contains("price") || key.Contains("cost") || key.Contains("rate") || key.Contains("profit") || key.Contains("margin") || key.Contains("tax") || key.Contains("qty") || key.Contains("quantity"))
                return TextAlignment.Right;

            return TextAlignment.Left;
        }

        private static string FormatCellValue(object val, UltraGridColumn col)
        {
            if (val == null || val == DBNull.Value) return string.Empty;

            if (val is DateTime dt)
            {
                if (!string.IsNullOrEmpty(col.Format))
                {
                    try { return dt.ToString(col.Format); } catch { }
                }
                return dt.ToString("dd-MM-yyyy");
            }

            if (val is decimal || val is double || val is float)
            {
                decimal num = Convert.ToDecimal(val);
                if (!string.IsNullOrEmpty(col.Format))
                {
                    try { return num.ToString(col.Format); } catch { }
                }
                return num.ToString("N2", CultureInfo.InvariantCulture);
            }

            return val.ToString();
        }
        #endregion

        #region DataTable Export
        public static void ExportDataTableToPdf(DataTable table, string filePath, string reportTitle, string subtitle = null)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            var colHeaders = new List<string>();
            var colAlignments = new List<TextAlignment>();
            var colWidthWeights = new List<int>();

            foreach (DataColumn col in table.Columns)
            {
                colHeaders.Add(col.ColumnName);
                if (col.DataType == typeof(decimal) || col.DataType == typeof(double) || col.DataType == typeof(float) || col.DataType == typeof(int) || col.DataType == typeof(long))
                {
                    colAlignments.Add(TextAlignment.Right);
                    colWidthWeights.Add(70);
                }
                else if (col.DataType == typeof(DateTime) || col.ColumnName.ToLowerInvariant().Contains("date") || col.ColumnName.ToLowerInvariant().Contains("slno"))
                {
                    colAlignments.Add(TextAlignment.Center);
                    colWidthWeights.Add(80);
                }
                else
                {
                    colAlignments.Add(TextAlignment.Left);
                    colWidthWeights.Add(110);
                }
            }

            var dataRows = new List<PdfRowData>();
            foreach (DataRow row in table.Rows)
            {
                var cells = new List<string>();
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    object v = row[i];
                    if (v == null || v == DBNull.Value)
                    {
                        cells.Add(string.Empty);
                    }
                    else if (v is DateTime dt)
                    {
                        cells.Add(dt.ToString("dd-MM-yyyy"));
                    }
                    else if (v is decimal || v is double)
                    {
                        cells.Add(Convert.ToDecimal(v).ToString("N2", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        cells.Add(v.ToString());
                    }
                }
                dataRows.Add(new PdfRowData { Cells = cells, IsMaster = true });
            }

            GeneratePdfFile(filePath, reportTitle, subtitle, colHeaders, colAlignments, colWidthWeights, dataRows, null);
        }
        #endregion

        #region CSV Export
        public static void ExportCsvToPdf(string csvContent, string filePath, string reportTitle, string subtitle = null)
        {
            if (string.IsNullOrEmpty(csvContent)) return;

            string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return;

            // Header line
            var headerCells = ParseCsvLine(lines[0]);
            var colHeaders = headerCells;
            var colAlignments = new List<TextAlignment>();
            var colWidthWeights = new List<int>();

            foreach (var h in colHeaders)
            {
                string hl = h.ToLowerInvariant();
                if (hl.Contains("amount") || hl.Contains("total") || hl.Contains("price") || hl.Contains("cost") || hl.Contains("profit") || hl.Contains("margin") || hl.Contains("tax") || hl.Contains("qty"))
                {
                    colAlignments.Add(TextAlignment.Right);
                    colWidthWeights.Add(75);
                }
                else if (hl.Contains("date") || hl.Contains("slno") || hl.Contains("sno") || hl.Contains("mode"))
                {
                    colAlignments.Add(TextAlignment.Center);
                    colWidthWeights.Add(80);
                }
                else
                {
                    colAlignments.Add(TextAlignment.Left);
                    colWidthWeights.Add(110);
                }
            }

            var dataRows = new List<PdfRowData>();
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.TrimStart().StartsWith("-> Item Details:") || line.TrimStart().StartsWith("=== ") || line.TrimStart().StartsWith("---"))
                {
                    // Descriptive section / sub-header
                    continue;
                }

                var parsed = ParseCsvLine(line);
                while (parsed.Count < colHeaders.Count) parsed.Add(string.Empty);
                if (parsed.Count > colHeaders.Count) parsed = parsed.Take(colHeaders.Count).ToList();

                dataRows.Add(new PdfRowData { Cells = parsed, IsMaster = true });
            }

            GeneratePdfFile(filePath, reportTitle, subtitle, colHeaders, colAlignments, colWidthWeights, dataRows, null);
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        sb.Append('\"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            result.Add(sb.ToString().Trim());
            return result;
        }
        #endregion

        #region PDF Rendering Engine (Pure Vector Standard PDF 1.4)
        private enum TextAlignment { Left, Center, Right }

        private class PdfRowData
        {
            public List<string> Cells { get; set; } = new List<string>();
            public bool IsMaster { get; set; } = true;
            public List<string> ChildRows { get; set; } = new List<string>();
        }

        private static void GeneratePdfFile(
            string filePath,
            string reportTitle,
            string subtitle,
            List<string> colHeaders,
            List<TextAlignment> colAlignments,
            List<int> colWidthWeights,
            List<PdfRowData> rows,
            List<string> footerTotals)
        {
            // Page Dimensions: A4 Landscape (842 pt width x 595 pt height)
            const float PageWidth = 842.0f;
            const float PageHeight = 595.0f;
            const float MarginLeft = 36.0f;
            const float MarginRight = 36.0f;
            const float MarginTop = 38.0f;
            const float MarginBottom = 38.0f;
            const float ContentWidth = PageWidth - MarginLeft - MarginRight; // 770 pt

            // Calculate proportional column widths
            float totalWeight = colWidthWeights.Sum();
            if (totalWeight <= 0) totalWeight = colHeaders.Count;

            var colWidths = new float[colHeaders.Count];
            for (int i = 0; i < colHeaders.Count; i++)
            {
                colWidths[i] = (colWidthWeights[i] / totalWeight) * ContentWidth;
            }

            // Ensure min column width
            for (int i = 0; i < colWidths.Length; i++)
            {
                if (colWidths[i] < 35.0f) colWidths[i] = 35.0f;
            }
            float adjustedTotal = colWidths.Sum();
            float scaleRatio = ContentWidth / adjustedTotal;
            for (int i = 0; i < colWidths.Length; i++)
            {
                colWidths[i] *= scaleRatio;
            }

            var colLefts = new float[colHeaders.Count];
            float currentLeft = MarginLeft;
            for (int i = 0; i < colHeaders.Count; i++)
            {
                colLefts[i] = currentLeft;
                currentLeft += colWidths[i];
            }

            const float HeaderHeight = 22.0f;
            const float RowHeight = 17.0f;
            const float ChildRowHeight = 14.0f;

            // Page building
            var pageStreams = new List<StringBuilder>();
            StringBuilder currentStream = new StringBuilder();
            pageStreams.Add(currentStream);

            float currentY = PageHeight - MarginTop;

            Action<StringBuilder, int> renderPageHeader = (sb, pageNum) =>
            {
                float y = PageHeight - MarginTop;

                // Title
                sb.AppendLine("BT /F2 13 Tf 0.08 0.35 0.70 rg");
                sb.AppendLine($"{MarginLeft:F2} {y:F2} Td ({EscapePdfString(reportTitle)}) Tj ET");

                // Generated Date/Time on right
                string genText = $"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}";
                float genWidth = genText.Length * 4.8f;
                float genX = (PageWidth - MarginRight) - genWidth;
                sb.AppendLine("BT /F1 8 Tf 0.45 0.45 0.45 rg");
                sb.AppendLine($"{genX:F2} {y:F2} Td ({EscapePdfString(genText)}) Tj ET");

                y -= 15.0f;

                // Subtitle (Period, Filters)
                if (!string.IsNullOrEmpty(subtitle))
                {
                    sb.AppendLine("BT /F1 8.5 Tf 0.25 0.25 0.25 rg");
                    sb.AppendLine($"{MarginLeft:F2} {y:F2} Td ({EscapePdfString(subtitle)}) Tj ET");
                    y -= 13.0f;
                }

                // Divider line
                sb.AppendLine("0.75 0.82 0.90 RG 0.75 w");
                sb.AppendLine($"{MarginLeft:F2} {y:F2} m {(PageWidth - MarginRight):F2} {y:F2} l S");
                y -= 10.0f;

                // Table Header background
                sb.AppendLine("0.26 0.46 0.72 rg");
                sb.AppendLine($"{MarginLeft:F2} {(y - HeaderHeight):F2} {ContentWidth:F2} {HeaderHeight:F2} re f");

                // Table Header border
                sb.AppendLine("0.20 0.38 0.62 RG 0.5 w");
                sb.AppendLine($"{MarginLeft:F2} {(y - HeaderHeight):F2} {ContentWidth:F2} {HeaderHeight:F2} re S");

                // Header cell texts & vertical dividers
                for (int i = 0; i < colHeaders.Count; i++)
                {
                    float cx = colLefts[i];
                    float cw = colWidths[i];

                    // Vertical divider
                    if (i > 0)
                    {
                        sb.AppendLine($"{cx:F2} {(y - HeaderHeight):F2} m {cx:F2} {y:F2} l S");
                    }

                    string caption = colHeaders[i];
                    float textWidth = caption.Length * 5.0f;
                    float textX = cx + 3.0f;
                    if (colAlignments[i] == TextAlignment.Right)
                    {
                        textX = cx + cw - textWidth - 3.0f;
                    }
                    else if (colAlignments[i] == TextAlignment.Center)
                    {
                        textX = cx + (cw - textWidth) / 2.0f;
                    }
                    if (textX < cx + 2.0f) textX = cx + 2.0f;

                    sb.AppendLine("BT /F2 8 Tf 1 1 1 rg");
                    sb.AppendLine($"{textX:F2} {(y - HeaderHeight + 6.0f):F2} Td ({EscapePdfString(TruncateToFit(caption, cw - 4.0f, 5.0f))}) Tj ET");
                }

                currentY = y - HeaderHeight;
            };

            // Render first page header
            renderPageHeader(currentStream, 1);

            int rowIndex = 0;
            foreach (var rowData in rows)
            {
                float neededHeight = RowHeight + (rowData.ChildRows.Count * ChildRowHeight);
                if (currentY - neededHeight < MarginBottom + 20.0f)
                {
                    // Add new page
                    currentStream = new StringBuilder();
                    pageStreams.Add(currentStream);
                    renderPageHeader(currentStream, pageStreams.Count);
                }

                // Master row background (alternating soft tint)
                if (rowIndex % 2 == 1)
                {
                    currentStream.AppendLine("0.96 0.98 1.0 rg");
                    currentStream.AppendLine($"{MarginLeft:F2} {(currentY - RowHeight):F2} {ContentWidth:F2} {RowHeight:F2} re f");
                }

                // Row borders
                currentStream.AppendLine("0.84 0.89 0.94 RG 0.4 w");
                currentStream.AppendLine($"{MarginLeft:F2} {(currentY - RowHeight):F2} {ContentWidth:F2} {RowHeight:F2} re S");

                // Vertical column borders
                for (int i = 1; i < colHeaders.Count; i++)
                {
                    float cx = colLefts[i];
                    currentStream.AppendLine($"{cx:F2} {(currentY - RowHeight):F2} m {cx:F2} {currentY:F2} l S");
                }

                // Cell values
                for (int i = 0; i < colHeaders.Count; i++)
                {
                    float cx = colLefts[i];
                    float cw = colWidths[i];
                    string cellVal = i < rowData.Cells.Count ? (rowData.Cells[i] ?? string.Empty) : string.Empty;
                    cellVal = CleanCurrencySymbols(cellVal);

                    string fitVal = TruncateToFit(cellVal, cw - 4.0f, 4.4f);
                    float textWidth = fitVal.Length * 4.4f;

                    float textX = cx + 3.0f;
                    if (colAlignments[i] == TextAlignment.Right)
                    {
                        textX = cx + cw - textWidth - 3.0f;
                    }
                    else if (colAlignments[i] == TextAlignment.Center)
                    {
                        textX = cx + (cw - textWidth) / 2.0f;
                    }
                    if (textX < cx + 2.0f) textX = cx + 2.0f;

                    currentStream.AppendLine("BT /F1 7.5 Tf 0.12 0.18 0.28 rg");
                    currentStream.AppendLine($"{textX:F2} {(currentY - RowHeight + 5.0f):F2} Td ({EscapePdfString(fitVal)}) Tj ET");
                }

                currentY -= RowHeight;

                // Child detail rows (if hierarchical)
                if (rowData.ChildRows.Count > 0)
                {
                    foreach (var childLine in rowData.ChildRows)
                    {
                        currentStream.AppendLine("0.98 0.99 1.0 rg");
                        currentStream.AppendLine($"{(MarginLeft + 15.0f):F2} {(currentY - ChildRowHeight):F2} {(ContentWidth - 15.0f):F2} {ChildRowHeight:F2} re f");
                        currentStream.AppendLine("0.88 0.92 0.96 RG 0.3 w");
                        currentStream.AppendLine($"{(MarginLeft + 15.0f):F2} {(currentY - ChildRowHeight):F2} {(ContentWidth - 15.0f):F2} {ChildRowHeight:F2} re S");

                        currentStream.AppendLine("BT /F3 6.8 Tf 0.25 0.35 0.45 rg");
                        currentStream.AppendLine($"{(MarginLeft + 20.0f):F2} {(currentY - ChildRowHeight + 4.0f):F2} Td ({EscapePdfString(TruncateToFit(CleanCurrencySymbols(childLine), ContentWidth - 25.0f, 4.0f))}) Tj ET");
                        currentY -= ChildRowHeight;
                    }
                }

                rowIndex++;
            }

            // Footer Totals Row if provided
            if (footerTotals != null && footerTotals.Count > 0)
            {
                if (currentY - (RowHeight + 2.0f) < MarginBottom + 20.0f)
                {
                    currentStream = new StringBuilder();
                    pageStreams.Add(currentStream);
                    renderPageHeader(currentStream, pageStreams.Count);
                }

                // Summary background
                currentStream.AppendLine("0.90 0.94 0.98 rg");
                currentStream.AppendLine($"{MarginLeft:F2} {(currentY - RowHeight - 2.0f):F2} {ContentWidth:F2} {(RowHeight + 2.0f):F2} re f");

                // Summary double border
                currentStream.AppendLine("0.26 0.46 0.72 RG 0.8 w");
                currentStream.AppendLine($"{MarginLeft:F2} {(currentY - RowHeight - 2.0f):F2} {ContentWidth:F2} {(RowHeight + 2.0f):F2} re S");

                for (int i = 0; i < colHeaders.Count; i++)
                {
                    float cx = colLefts[i];
                    float cw = colWidths[i];
                    string totVal = i < footerTotals.Count ? (footerTotals[i] ?? string.Empty) : string.Empty;
                    totVal = CleanCurrencySymbols(totVal);

                    if (!string.IsNullOrEmpty(totVal))
                    {
                        string fitVal = TruncateToFit(totVal, cw - 4.0f, 4.8f);
                        float textWidth = fitVal.Length * 4.8f;
                        float textX = cx + 3.0f;
                        if (colAlignments[i] == TextAlignment.Right)
                        {
                            textX = cx + cw - textWidth - 3.0f;
                        }
                        else if (colAlignments[i] == TextAlignment.Center)
                        {
                            textX = cx + (cw - textWidth) / 2.0f;
                        }

                        currentStream.AppendLine("BT /F2 8 Tf 0.08 0.28 0.63 rg");
                        currentStream.AppendLine($"{textX:F2} {(currentY - RowHeight + 4.5f):F2} Td ({EscapePdfString(fitVal)}) Tj ET");
                    }
                }
                currentY -= (RowHeight + 2.0f);
            }

            // Append page footers ("Page X of Y | Nexoris POS")
            int totalPages = pageStreams.Count;
            for (int p = 0; p < totalPages; p++)
            {
                var sb = pageStreams[p];
                float footerY = MarginBottom - 12.0f;

                // Footer divider
                sb.AppendLine("0.78 0.84 0.92 RG 0.5 w");
                sb.AppendLine($"{MarginLeft:F2} {(footerY + 14.0f):F2} m {(PageWidth - MarginRight):F2} {(footerY + 14.0f):F2} l S");

                // Footer text
                string pageStr = $"Page {p + 1} of {totalPages}";
                float pWidth = pageStr.Length * 4.5f;
                float pX = (PageWidth - pWidth) / 2.0f;

                sb.AppendLine("BT /F1 7.5 Tf 0.45 0.45 0.45 rg");
                sb.AppendLine($"{MarginLeft:F2} {footerY:F2} Td (Nexoris POS - Confidential) Tj ET");
                sb.AppendLine($"BT /F2 7.5 Tf 0.30 0.30 0.30 rg");
                sb.AppendLine($"{pX:F2} {footerY:F2} Td ({EscapePdfString(pageStr)}) Tj ET");
            }

            // Write PDF Binary Stream to disk
            WritePdfFileBinary(filePath, PageWidth, PageHeight, pageStreams);
        }

        private static string TruncateToFit(string text, float maxWidth, float charWidth)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            int maxChars = (int)(maxWidth / charWidth);
            if (maxChars <= 0) return string.Empty;
            if (text.Length <= maxChars) return text;
            if (maxChars <= 3) return text.Substring(0, maxChars);
            return text.Substring(0, maxChars - 2) + "..";
        }

        private static string CleanCurrencySymbols(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Replace("₹", "Rs. ").Replace("?", "").Trim();
        }

        private static string EscapePdfString(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c == '\\') sb.Append("\\\\");
                else if (c == '(') sb.Append("\\(");
                else if (c == ')') sb.Append("\\)");
                else if (c == '\r' || c == '\n') sb.Append(" ");
                else if (c < 32 || c > 126)
                {
                    if (c >= 128 && c <= 255)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "\\{0:000}", (int)c);
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static void WritePdfFileBinary(string filePath, float pageWidth, float pageHeight, List<StringBuilder> pageStreams)
        {
            int pageCount = pageStreams.Count;

            // PDF Object Numbering:
            // 1: Catalog
            // 2: Pages container
            // 3: Font Helvetica
            // 4: Font Helvetica-Bold
            // 5: Font Helvetica-Oblique
            // Then for each page i (0 to pageCount-1):
            // Page Obj: 6 + (i * 2)
            // Stream Obj: 7 + (i * 2)

            int totalObjects = 5 + (pageCount * 2);

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var bw = new BinaryWriter(fs, Encoding.ASCII))
            {
                var offsets = new long[totalObjects + 1];

                // PDF Header
                bw.Write(Encoding.ASCII.GetBytes("%PDF-1.4\r\n%\xE2\xE3\xCF\xD3\r\n"));

                // Obj 1: Catalog
                offsets[1] = fs.Position;
                bw.Write(Encoding.ASCII.GetBytes("1 0 obj\r\n<< /Type /Catalog /Pages 2 0 R >>\r\nendobj\r\n"));

                // Obj 2: Pages
                offsets[2] = fs.Position;
                var kidsSb = new StringBuilder();
                for (int i = 0; i < pageCount; i++)
                {
                    int pageObjNum = 6 + (i * 2);
                    kidsSb.Append($"{pageObjNum} 0 R ");
                }
                bw.Write(Encoding.ASCII.GetBytes($"2 0 obj\r\n<< /Type /Pages /Kids [ {kidsSb.ToString().Trim()} ] /Count {pageCount} >>\r\nendobj\r\n"));

                // Obj 3: Font Helvetica
                offsets[3] = fs.Position;
                bw.Write(Encoding.ASCII.GetBytes("3 0 obj\r\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\r\nendobj\r\n"));

                // Obj 4: Font Helvetica-Bold
                offsets[4] = fs.Position;
                bw.Write(Encoding.ASCII.GetBytes("4 0 obj\r\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>\r\nendobj\r\n"));

                // Obj 5: Font Helvetica-Oblique
                offsets[5] = fs.Position;
                bw.Write(Encoding.ASCII.GetBytes("5 0 obj\r\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Oblique /Encoding /WinAnsiEncoding >>\r\nendobj\r\n"));

                // Pages and Content Streams
                for (int i = 0; i < pageCount; i++)
                {
                    int pageObjNum = 6 + (i * 2);
                    int contentObjNum = 7 + (i * 2);

                    // Page Object
                    offsets[pageObjNum] = fs.Position;
                    string pageHeader = $"{pageObjNum} 0 obj\r\n<< /Type /Page /Parent 2 0 R /MediaBox [ 0 0 {pageWidth:F1} {pageHeight:F1} ] /Resources << /Font << /F1 3 0 R /F2 4 0 R /F3 5 0 R >> >> /Contents {contentObjNum} 0 R >>\r\nendobj\r\n";
                    bw.Write(Encoding.ASCII.GetBytes(pageHeader));

                    // Content Stream Object
                    byte[] streamBytes = Encoding.ASCII.GetBytes(pageStreams[i].ToString());
                    offsets[contentObjNum] = fs.Position;
                    bw.Write(Encoding.ASCII.GetBytes($"{contentObjNum} 0 obj\r\n<< /Length {streamBytes.Length} >>\r\nstream\r\n"));
                    bw.Write(streamBytes);
                    bw.Write(Encoding.ASCII.GetBytes("\r\nendstream\r\nendobj\r\n"));
                }

                // XRef table
                long xrefOffset = fs.Position;
                bw.Write(Encoding.ASCII.GetBytes($"xref\r\n0 {totalObjects + 1}\r\n"));
                bw.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \r\n"));
                for (int i = 1; i <= totalObjects; i++)
                {
                    bw.Write(Encoding.ASCII.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D10} 00000 n \r\n", offsets[i])));
                }

                // Trailer
                bw.Write(Encoding.ASCII.GetBytes($"trailer\r\n<< /Size {totalObjects + 1} /Root 1 0 R >>\r\n"));
                bw.Write(Encoding.ASCII.GetBytes($"startxref\r\n{xrefOffset}\r\n%%EOF\r\n"));
            }
        }
        #endregion
    }
}
