using Infragistics.Win.UltraWinGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Reports
{
    /// <summary>
    /// Helper to persist and restore report grid layout state (hidden columns, column widths, and visible positions).
    /// Prevents user-hidden (dragged-down) columns from reappearing when closing and reopening reports.
    /// </summary>
    public static class ReportGridLayoutHelper
    {
        private static string GetLayoutStateFilePath(string reportKey)
        {
            string fileName = $"{reportKey}_LayoutState.txt";
            try
            {
                string startupPath = Application.StartupPath;
                string path = Path.Combine(startupPath, fileName);
                if (File.Exists(path))
                {
                    return path;
                }

                // Check directory writeability
                string testFile = Path.Combine(startupPath, $".test_{Guid.NewGuid():N}.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return path;
            }
            catch
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string folder = Path.Combine(appData, "NexorisPOS");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                return Path.Combine(folder, fileName);
            }
        }

        /// <summary>
        /// Loads the set of user-hidden column keys from disk for the specified report.
        /// </summary>
        public static void LoadHiddenColumns(string reportKey, HashSet<string> userHiddenColumnKeys)
        {
            if (string.IsNullOrWhiteSpace(reportKey) || userHiddenColumnKeys == null)
                return;

            try
            {
                string path = GetLayoutStateFilePath(reportKey);
                if (!File.Exists(path))
                {
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string altPath = Path.Combine(appData, "NexorisPOS", $"{reportKey}_LayoutState.txt");
                    if (File.Exists(altPath))
                    {
                        path = altPath;
                    }
                    else
                    {
                        return;
                    }
                }

                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    if (line.StartsWith("HIDDEN:", StringComparison.OrdinalIgnoreCase))
                    {
                        string raw = line.Substring(7);
                        string[] keys = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        userHiddenColumnKeys.Clear();
                        foreach (string k in keys)
                        {
                            string trimmed = k.Trim();
                            if (!string.IsNullOrEmpty(trimmed))
                            {
                                userHiddenColumnKeys.Add(trimmed);
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReportGridLayoutHelper: Error loading hidden columns for {reportKey}: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves hidden columns, column widths, and visible positions to disk.
        /// </summary>
        public static void SaveLayout(string reportKey, UltraGrid grid, IEnumerable<string> hiddenKeys)
        {
            if (string.IsNullOrWhiteSpace(reportKey))
                return;

            try
            {
                List<string> hiddenList = hiddenKeys != null
                    ? hiddenKeys.Where(k => !string.IsNullOrWhiteSpace(k)).Select(k => k.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                    : new List<string>();

                Dictionary<string, int> widths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, int> positions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                if (grid != null && grid.DisplayLayout != null && grid.DisplayLayout.Bands.Count > 0)
                {
                    for (int b = 0; b < grid.DisplayLayout.Bands.Count; b++)
                    {
                        UltraGridBand band = grid.DisplayLayout.Bands[b];
                        foreach (UltraGridColumn col in band.Columns)
                        {
                            if (col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True)
                                continue;

                            string key = grid.DisplayLayout.Bands.Count > 1 ? $"{b}:{col.Key}" : col.Key;
                            widths[key] = col.Width;
                            positions[key] = col.Header.VisiblePosition;
                        }
                    }
                }

                List<string> lines = new List<string>
                {
                    "HIDDEN:" + string.Join(",", hiddenList)
                };

                if (widths.Count > 0)
                {
                    lines.Add("WIDTHS:" + string.Join(";", widths.Select(kv => $"{kv.Key}={kv.Value}")));
                }
                if (positions.Count > 0)
                {
                    lines.Add("POSITIONS:" + string.Join(";", positions.Select(kv => $"{kv.Key}={kv.Value}")));
                }

                string path = GetLayoutStateFilePath(reportKey);
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReportGridLayoutHelper: Error saving layout for {reportKey}: {ex.Message}");
                try
                {
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string folder = Path.Combine(appData, "NexorisPOS");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    string altPath = Path.Combine(folder, $"{reportKey}_LayoutState.txt");
                    File.WriteAllLines(altPath, new[] { "HIDDEN:" + string.Join(",", hiddenKeys ?? new string[0]) });
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Applies saved hidden state, column widths, and visible positions to the grid.
        /// </summary>
        public static void ApplySavedLayout(string reportKey, UltraGrid grid, HashSet<string> userHiddenColumnKeys)
        {
            if (string.IsNullOrWhiteSpace(reportKey))
                return;

            if (userHiddenColumnKeys != null)
            {
                LoadHiddenColumns(reportKey, userHiddenColumnKeys);
            }

            if (grid == null || grid.DisplayLayout == null || grid.DisplayLayout.Bands.Count == 0)
                return;

            try
            {
                string path = GetLayoutStateFilePath(reportKey);
                if (!File.Exists(path))
                {
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string altPath = Path.Combine(appData, "NexorisPOS", $"{reportKey}_LayoutState.txt");
                    if (File.Exists(altPath))
                    {
                        path = altPath;
                    }
                    else
                    {
                        return;
                    }
                }

                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    if (line.StartsWith("HIDDEN:", StringComparison.OrdinalIgnoreCase))
                    {
                        string raw = line.Substring(7);
                        var hidden = new HashSet<string>(raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
                        for (int b = 0; b < grid.DisplayLayout.Bands.Count; b++)
                        {
                            UltraGridBand currentBand = grid.DisplayLayout.Bands[b];
                            foreach (UltraGridColumn col in currentBand.Columns)
                            {
                                if (col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True)
                                    continue;

                                if (hidden.Contains(col.Key) || hidden.Contains($"{b}:{col.Key}"))
                                {
                                    col.Hidden = true;
                                }
                            }
                        }
                    }
                    else if (line.StartsWith("WIDTHS:", StringComparison.OrdinalIgnoreCase))
                    {
                        string raw = line.Substring(7);
                        var pairs = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string pair in pairs)
                        {
                            var parts = pair.Split('=');
                            if (parts.Length == 2 && int.TryParse(parts[1], out int w) && w > 0)
                            {
                                string colKey = parts[0];
                                int bandIdx = 0;
                                if (colKey.Contains(":"))
                                {
                                    var sub = colKey.Split(':');
                                    if (sub.Length == 2 && int.TryParse(sub[0], out int bi))
                                    {
                                        bandIdx = bi;
                                        colKey = sub[1];
                                    }
                                }

                                if (bandIdx < grid.DisplayLayout.Bands.Count && grid.DisplayLayout.Bands[bandIdx].Columns.Exists(colKey))
                                {
                                    grid.DisplayLayout.Bands[bandIdx].Columns[colKey].Width = w;
                                }
                            }
                        }
                    }
                    else if (line.StartsWith("POSITIONS:", StringComparison.OrdinalIgnoreCase))
                    {
                        string raw = line.Substring(10);
                        var pairs = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        var sorted = pairs
                            .Select(p => p.Split('='))
                            .Where(p => p.Length == 2 && int.TryParse(p[1], out _))
                            .OrderBy(p => int.Parse(p[1]))
                            .ToList();

                        foreach (var p in sorted)
                        {
                            string colKey = p[0];
                            int bandIdx = 0;
                            if (colKey.Contains(":"))
                            {
                                var sub = colKey.Split(':');
                                if (sub.Length == 2 && int.TryParse(sub[0], out int bi))
                                {
                                    bandIdx = bi;
                                    colKey = sub[1];
                                }
                            }

                            if (bandIdx < grid.DisplayLayout.Bands.Count && grid.DisplayLayout.Bands[bandIdx].Columns.Exists(colKey))
                            {
                                grid.DisplayLayout.Bands[bandIdx].Columns[colKey].Header.VisiblePosition = int.Parse(p[1]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReportGridLayoutHelper: Error applying layout for {reportKey}: {ex.Message}");
            }
        }
    }
}
