using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

    /// <summary>
    /// Manager that adds excel-style column header dropdown filtering to any Infragistics UltraGrid report.
    /// Includes search, (Select All) checkbox, distinct values checklist, and [ OK ], [ Cancel ], [ Clear ] buttons.
    /// </summary>
    public class ReportColumnFilterManager
    {
        private readonly UltraGrid _grid;
        private readonly Action _onFilterApplied;
        private readonly Dictionary<string, HashSet<string>> _activeColumnFilters = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Bitmap _filterIconNormal = CreateFilterIcon(false);
        private static readonly Bitmap _filterIconActive = CreateFilterIcon(true);
        private bool _suppressSorting;
        private Form _activeDropdownForm;
        private DateTime _lastDropdownCloseTime;
        private string _lastClosedColumnKey;

        public Dictionary<string, HashSet<string>> ActiveColumnFilters => _activeColumnFilters;
        public bool HasActiveFilters => _activeColumnFilters.Count > 0;

        public ReportColumnFilterManager(UltraGrid grid, Action onFilterApplied = null, bool autoWireMouseEvents = false)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _onFilterApplied = onFilterApplied;

            _grid.BeforeSortChange += (s, e) =>
            {
                if (_suppressSorting)
                {
                    e.Cancel = true;
                    _suppressSorting = false;
                }
            };

            _grid.Resize += (s, e) => CloseActiveFilterDropdown();
            _grid.AfterColRegionScroll += (s, e) => CloseActiveFilterDropdown();
            _grid.AfterRowRegionScroll += (s, e) => CloseActiveFilterDropdown();

            WireFormClosing();

            if (autoWireMouseEvents)
            {
                _grid.MouseDown += (s, e) => HandleMouseDown(e);
                _grid.MouseMove += (s, e) => HandleMouseMove(e);
            }
        }

        private void WireFormClosing()
        {
            try
            {
                Form parentForm = _grid.FindForm();
                if (parentForm != null)
                {
                    parentForm.FormClosing += (s, e) => CloseActiveFilterDropdown();
                }
                else
                {
                    _grid.ParentChanged += (s, e) =>
                    {
                        Form f = _grid.FindForm();
                        if (f != null)
                        {
                            f.FormClosing += (fs, fe) => CloseActiveFilterDropdown();
                        }
                    };
                }
            }
            catch
            {
            }
        }

        public bool HandleHeaderMouseDown(UIElement element, HeaderUIElement headerUI, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || headerUI?.Header?.Column == null)
                return false;

            UltraGridColumn col = headerUI.Header.Column;
            if (col.Hidden || col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || col.Key == "SlNo" || col.IsChaptered)
                return false;

            ImageUIElement imgElem = element as ImageUIElement ?? element?.GetAncestor(typeof(ImageUIElement)) as ImageUIElement;
            if (imgElem != null || e.X >= headerUI.Rect.Right - 24)
            {
                _suppressSorting = true;
                ShowColumnFilterPopup(col, headerUI.Rect);
                return true;
            }

            return false;
        }

        public bool HandleHeaderMouseMove(UIElement element, HeaderUIElement headerUI, MouseEventArgs e, System.Windows.Forms.ToolTip toolTip = null)
        {
            if (headerUI?.Header?.Column == null)
                return false;

            UltraGridColumn col = headerUI.Header.Column;
            if (col.Hidden || col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || col.Key == "SlNo" || col.IsChaptered)
                return false;

            if (e.X >= headerUI.Rect.Right - 24)
            {
                _grid.Cursor = Cursors.Hand;
                string colCaption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                toolTip?.SetToolTip(_grid, $"Filter by '{colCaption}'");
                return true;
            }
            else
            {
                _grid.Cursor = Cursors.Default;
                toolTip?.SetToolTip(_grid, string.Empty);
                return false;
            }
        }

        public bool HandleMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || _grid.DisplayLayout == null)
                return false;

            Point pt = new Point(e.X, e.Y);
            UIElement elem = _grid.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement hUI = elem as HeaderUIElement ?? elem?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
            if (hUI != null)
            {
                return HandleHeaderMouseDown(elem, hUI, e);
            }
            return false;
        }

        public bool HandleMouseMove(MouseEventArgs e, System.Windows.Forms.ToolTip toolTip = null)
        {
            if (_grid.DisplayLayout == null)
                return false;

            Point pt = new Point(e.X, e.Y);
            UIElement elem = _grid.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement hUI = elem as HeaderUIElement ?? elem?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
            if (hUI != null)
            {
                return HandleHeaderMouseMove(elem, hUI, e, toolTip);
            }
            return false;
        }

        public void AddFilterContextMenuItems(ContextMenuStrip menu, UltraGridColumn col, Point location)
        {
            if (col == null || menu == null) return;
            if (col.Hidden || col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || col.Key == "SlNo" || col.IsChaptered) return;

            string colName = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;

            ToolStripMenuItem filterItem = new ToolStripMenuItem($"🔍 Filter '{colName}'...", null, (s, e) =>
            {
                var headerUI = col.Header.GetUIElement();
                Rectangle r = headerUI != null ? headerUI.Rect : new Rectangle(location.X, location.Y, 20, 20);
                ShowColumnFilterPopup(col, r);
            });
            filterItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            menu.Items.Add(filterItem);

            if (_activeColumnFilters.ContainsKey(col.Key))
            {
                ToolStripMenuItem clearColFilterItem = new ToolStripMenuItem($"❌ Clear Filter on '{colName}'", null, (s, e) =>
                {
                    _activeColumnFilters.Remove(col.Key);
                    ApplyActiveColumnFilters();
                });
                menu.Items.Add(clearColFilterItem);
            }

            if (_activeColumnFilters.Count > 0)
            {
                ToolStripMenuItem clearAllFiltersItem = new ToolStripMenuItem("❌ Clear All Column Filters", null, (s, e) =>
                {
                    _activeColumnFilters.Clear();
                    ApplyActiveColumnFilters();
                });
                menu.Items.Add(clearAllFiltersItem);
            }
        }

        public void UpdateHeaderFilterIcons()
        {
            if (_grid.DisplayLayout == null || _grid.DisplayLayout.Bands.Count == 0) return;

            foreach (UltraGridBand band in _grid.DisplayLayout.Bands)
            {
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (col.Hidden || col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || col.Key == "SlNo" || col.IsChaptered)
                        continue;

                    bool isFiltered = _activeColumnFilters.ContainsKey(col.Key);
                    col.Header.Appearance.Image = isFiltered ? _filterIconActive : _filterIconNormal;
                    col.Header.Appearance.ImageHAlign = HAlign.Right;
                    col.Header.Appearance.ImageVAlign = VAlign.Middle;
                }
            }
        }

        public void CloseActiveFilterDropdown()
        {
            if (_activeDropdownForm != null && !_activeDropdownForm.IsDisposed)
            {
                _activeDropdownForm.Close();
                _activeDropdownForm = null;
            }
        }

        public void ShowColumnFilterPopup(UltraGridColumn col, Rectangle headerRect)
        {
            if (col == null || _grid.Rows == null || _grid.Rows.Count == 0)
            {
                MessageBox.Show("No data loaded in the grid to filter.", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            CloseActiveFilterDropdown();

            if (string.Equals(_lastClosedColumnKey, col.Key, StringComparison.OrdinalIgnoreCase) &&
                (DateTime.Now - _lastDropdownCloseTime).TotalMilliseconds < 250)
            {
                _lastClosedColumnKey = null;
                return;
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> allValues = new List<string>();

            foreach (UltraGridRow row in _grid.Rows)
            {
                if (!row.IsDataRow) continue;
                string text = row.Cells.Exists(col.Key) ? (row.Cells[col.Key].Text?.Trim() ?? "") : "";
                if (string.IsNullOrEmpty(text)) text = "(Blanks)";
                if (seen.Add(text))
                {
                    allValues.Add(text);
                }
            }

            allValues.Sort((a, b) =>
            {
                if (a == "(Blanks)") return 1;
                if (b == "(Blanks)") return -1;
                if (decimal.TryParse(a, out decimal da) && decimal.TryParse(b, out decimal db))
                    return da.CompareTo(db);
                return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
            });

            HashSet<string> currentSelected = null;
            if (_activeColumnFilters.ContainsKey(col.Key))
            {
                currentSelected = _activeColumnFilters[col.Key];
            }

            string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;

            int dropWidth = Math.Max(230, Math.Min(Math.Max(headerRect.Width, 230), 300));
            int itemsHeight = Math.Min(Math.Max(allValues.Count * 22, 44), 190);
            int dropHeight = 90 + itemsHeight;
            var popup = new ColumnFilterDropdownForm(col.Key, caption, allValues, currentSelected, dropWidth, dropHeight, (newSelected) =>
            {
                if (newSelected == null || newSelected.Count == allValues.Count)
                {
                    _activeColumnFilters.Remove(col.Key);
                }
                else
                {
                    _activeColumnFilters[col.Key] = new HashSet<string>(newSelected, StringComparer.OrdinalIgnoreCase);
                }

                ApplyActiveColumnFilters();
            });

            Point screenPt = _grid.PointToScreen(new Point(headerRect.Left, headerRect.Bottom));
            Rectangle screenArea = Screen.FromControl(_grid).WorkingArea;

            if (screenPt.X + dropWidth > screenArea.Right)
                screenPt.X = screenArea.Right - dropWidth - 6;
            if (screenPt.X < screenArea.Left)
                screenPt.X = screenArea.Left + 6;

            if (screenPt.Y + dropHeight > screenArea.Bottom)
                screenPt.Y = screenPt.Y - headerRect.Height - dropHeight;

            popup.Location = screenPt;
            _activeDropdownForm = popup;
            popup.FormClosed += (s, e) =>
            {
                _lastClosedColumnKey = col.Key;
                _lastDropdownCloseTime = DateTime.Now;
                if (_activeDropdownForm == popup)
                    _activeDropdownForm = null;
            };
            popup.Show(_grid.FindForm());
        }

        public void ApplyActiveColumnFilters()
        {
            _grid.BeginUpdate();
            try
            {
                bool hasFilters = _activeColumnFilters.Count > 0;

                foreach (UltraGridRow row in _grid.Rows)
                {
                    if (!row.IsDataRow) continue;

                    if (!hasFilters)
                    {
                        row.Hidden = false;
                        continue;
                    }

                    bool visible = true;
                    foreach (var kvp in _activeColumnFilters)
                    {
                        string colKey = kvp.Key;
                        HashSet<string> allowed = kvp.Value;

                        string cellText = row.Cells.Exists(colKey) ? (row.Cells[colKey].Text?.Trim() ?? "") : "";
                        if (string.IsNullOrEmpty(cellText)) cellText = "(Blanks)";

                        if (!allowed.Contains(cellText))
                        {
                            visible = false;
                            break;
                        }
                    }

                    row.Hidden = !visible;
                }
            }
            finally
            {
                _grid.EndUpdate();
            }

            UpdateHeaderFilterIcons();
            _onFilterApplied?.Invoke();
        }

        public void ClearFilters()
        {
            _activeColumnFilters.Clear();
            CloseActiveFilterDropdown();
            ApplyActiveColumnFilters();
        }

        private static Bitmap CreateFilterIcon(bool isActive)
        {
            Bitmap bmp = new Bitmap(18, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);

                Rectangle btnRect = new Rectangle(0, 0, 17, 15);

                if (isActive)
                {
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(255, 179, 0)))
                    using (Pen borderPen = new Pen(Color.FromArgb(180, 120, 0), 1f))
                    using (GraphicsPath path = CreateRoundedRectPath(btnRect, 3))
                    {
                        g.FillPath(bgBrush, path);
                        g.DrawPath(borderPen, path);
                    }

                    using (SolidBrush funnelBrush = new SolidBrush(Color.FromArgb(15, 35, 75)))
                    {
                        PointF[] funnel = new PointF[]
                        {
                            new PointF(3.5f, 3.5f),
                            new PointF(10.5f, 3.5f),
                            new PointF(7.5f, 7.5f),
                            new PointF(7.5f, 11.5f),
                            new PointF(6.5f, 10.5f),
                            new PointF(6.5f, 7.5f)
                        };
                        g.FillPolygon(funnelBrush, funnel);
                    }

                    using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb(15, 35, 75)))
                    {
                        g.FillEllipse(dotBrush, 11.5f, 8.5f, 3.5f, 3.5f);
                    }
                }
                else
                {
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
                    using (Pen borderPen = new Pen(Color.FromArgb(120, 255, 255, 255), 1f))
                    using (GraphicsPath path = CreateRoundedRectPath(btnRect, 3))
                    {
                        g.FillPath(bgBrush, path);
                        g.DrawPath(borderPen, path);
                    }

                    using (SolidBrush funnelBrush = new SolidBrush(Color.White))
                    {
                        PointF[] funnel = new PointF[]
                        {
                            new PointF(3.0f, 3.5f),
                            new PointF(10.0f, 3.5f),
                            new PointF(7.0f, 7.5f),
                            new PointF(7.0f, 11.5f),
                            new PointF(6.0f, 10.5f),
                            new PointF(6.0f, 7.5f)
                        };
                        g.FillPolygon(funnelBrush, funnel);

                        PointF[] arrow = new PointF[]
                        {
                            new PointF(11.5f, 6.0f),
                            new PointF(15.5f, 6.0f),
                            new PointF(13.5f, 8.5f)
                        };
                        g.FillPolygon(funnelBrush, arrow);
                    }
                }
            }
            return bmp;
        }

        private static GraphicsPath CreateRoundedRectPath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private sealed class ColumnFilterDropdownForm : Form
        {
            private readonly string _columnKey;
            private readonly string _columnTitle;
            private readonly List<string> _allValues;
            private readonly HashSet<string> _selectedValues;
            private readonly Action<HashSet<string>> _onApply;

            private Panel pnlHeader;
            private Label lblTitle;
            private Label btnCloseX;
            private Panel pnlSearch;
            private TextBox txtSearch;
            private Panel pnlSelectAll;
            private CheckBox chkSelectAll;
            private CheckedListBox clbItems;
            private Panel pnlBottom;
            private Button btnOK;
            private Button btnCancel;
            private Button btnClear;

            private List<string> _visibleValues = new List<string>();
            private bool _isUpdatingCheckState = false;

            protected override CreateParams CreateParams
            {
                get
                {
                    const int CS_DROPSHADOW = 0x00020000;
                    CreateParams cp = base.CreateParams;
                    cp.ClassStyle |= CS_DROPSHADOW;
                    return cp;
                }
            }

            public ColumnFilterDropdownForm(string columnKey, string columnTitle, List<string> allValues, HashSet<string> currentSelected, int width, int height, Action<HashSet<string>> onApply)
            {
                _columnKey = columnKey;
                _columnTitle = columnTitle;
                _allValues = allValues ?? new List<string>();
                _selectedValues = currentSelected != null
                    ? new HashSet<string>(currentSelected, StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string>(_allValues, StringComparer.OrdinalIgnoreCase);
                _onApply = onApply;

                InitializePopupUi(width, height);
                PopulateItems(string.Empty);
            }

            private void InitializePopupUi(int width, int height)
            {
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                TopMost = true;
                Size = new Size(width, height);
                BackColor = Color.White;
                KeyPreview = true;
                DoubleBuffered = true;

                pnlSearch = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 28,
                    BackColor = Color.FromArgb(240, 244, 248),
                    Padding = new Padding(4, 3, 4, 3)
                };

                txtSearch = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F),
                    BorderStyle = BorderStyle.FixedSingle
                };
                txtSearch.TextChanged += (s, e) => PopulateItems(txtSearch.Text);

                Label lblSearchIcon = new Label
                {
                    Text = "🔍",
                    Dock = DockStyle.Left,
                    Width = 20,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(120, 140, 160),
                    Font = new Font("Segoe UI", 8F)
                };
                pnlSearch.Controls.Add(txtSearch);
                pnlSearch.Controls.Add(lblSearchIcon);

                txtSearch.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape) { Close(); e.Handled = true; }
                    else if (e.KeyCode == Keys.Enter) { ApplyAndClose(); e.Handled = true; }
                    else if (e.KeyCode == Keys.Down && clbItems.Items.Count > 0) { clbItems.Focus(); e.Handled = true; }
                };

                pnlSelectAll = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 28,
                    BackColor = Color.FromArgb(248, 250, 252),
                    Padding = new Padding(6, 2, 6, 2)
                };

                chkSelectAll = new CheckBox
                {
                    Text = "(Select All)",
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 35, 75),
                    ThreeState = true
                };
                chkSelectAll.Click += ChkSelectAll_Click;
                pnlSelectAll.Controls.Add(chkSelectAll);

                pnlBottom = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 34,
                    BackColor = Color.FromArgb(246, 248, 252),
                    Padding = new Padding(4, 4, 4, 4)
                };

                btnOK = new Button
                {
                    Text = "OK",
                    Size = new Size(54, 24),
                    Location = new Point(5, 5),
                    BackColor = Color.FromArgb(67, 118, 184),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnOK.FlatAppearance.BorderSize = 0;
                btnOK.MouseEnter += (s, e) => { btnOK.BackColor = Color.FromArgb(50, 95, 155); };
                btnOK.MouseLeave += (s, e) => { btnOK.BackColor = Color.FromArgb(67, 118, 184); };
                btnOK.Click += (s, e) => ApplyAndClose();

                btnCancel = new Button
                {
                    Text = "Cancel",
                    Size = new Size(54, 24),
                    Location = new Point(64, 5),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(30, 41, 59),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F),
                    Cursor = Cursors.Hand
                };
                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 220);
                btnCancel.MouseEnter += (s, e) => { btnCancel.BackColor = Color.FromArgb(241, 245, 249); };
                btnCancel.MouseLeave += (s, e) => { btnCancel.BackColor = Color.White; };
                btnCancel.Click += (s, e) => Close();

                btnClear = new Button
                {
                    Text = "Clear",
                    Size = new Size(54, 24),
                    Location = new Point(124, 5),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(180, 40, 40),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8.5F),
                    Cursor = Cursors.Hand
                };
                btnClear.FlatAppearance.BorderColor = Color.FromArgb(210, 160, 160);
                btnClear.MouseEnter += (s, e) => { btnClear.BackColor = Color.FromArgb(254, 242, 242); };
                btnClear.MouseLeave += (s, e) => { btnClear.BackColor = Color.White; };
                btnClear.Click += (s, e) => ClearAndClose();

                pnlBottom.Controls.Add(btnOK);
                pnlBottom.Controls.Add(btnCancel);
                pnlBottom.Controls.Add(btnClear);

                clbItems = new CheckedListBox
                {
                    Dock = DockStyle.Fill,
                    CheckOnClick = true,
                    BorderStyle = BorderStyle.None,
                    Font = new Font("Segoe UI", 9F),
                    BackColor = Color.White,
                    IntegralHeight = false
                };
                clbItems.ItemCheck += ClbItems_ItemCheck;
                clbItems.DoubleClick += (s, e) =>
                {
                    int index = clbItems.SelectedIndex;
                    if (index >= 0 && index < _visibleValues.Count)
                    {
                        string val = _visibleValues[index];
                        _selectedValues.Clear();
                        _selectedValues.Add(val);
                        ApplyAndClose();
                    }
                };

                Controls.Add(clbItems);
                Controls.Add(pnlSelectAll);
                Controls.Add(pnlSearch);
                Controls.Add(pnlBottom);

                Deactivate += (s, e) => { if (!IsDisposed) Close(); };
                KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape) Close();
                    else if (e.KeyCode == Keys.Enter && !txtSearch.Focused) ApplyAndClose();
                };
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                using (Pen p = new Pen(Color.FromArgb(160, 178, 200), 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                }
            }

            private void PopulateItems(string filterText)
            {
                _isUpdatingCheckState = true;
                clbItems.BeginUpdate();
                try
                {
                    clbItems.Items.Clear();
                    _visibleValues.Clear();

                    string query = (filterText ?? "").Trim();
                    bool hasQuery = !string.IsNullOrEmpty(query);

                    foreach (string val in _allValues)
                    {
                        if (!hasQuery || val.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _visibleValues.Add(val);
                            bool isChecked = _selectedValues.Contains(val);
                            clbItems.Items.Add(val, isChecked);
                        }
                    }
                }
                finally
                {
                    clbItems.EndUpdate();
                    _isUpdatingCheckState = false;
                }

                UpdateSelectAllState();
            }

            private void ClbItems_ItemCheck(object sender, ItemCheckEventArgs e)
            {
                if (_isUpdatingCheckState) return;

                if (e.Index >= 0 && e.Index < _visibleValues.Count)
                {
                    string itemValue = _visibleValues[e.Index];
                    if (e.NewValue == CheckState.Checked)
                    {
                        _selectedValues.Add(itemValue);
                    }
                    else
                    {
                        _selectedValues.Remove(itemValue);
                    }
                }

                BeginInvoke(new Action(UpdateSelectAllState));
            }

            private void UpdateSelectAllState()
            {
                if (_visibleValues.Count == 0)
                {
                    chkSelectAll.CheckState = CheckState.Unchecked;
                    return;
                }

                int checkedCount = 0;
                foreach (string val in _visibleValues)
                {
                    if (_selectedValues.Contains(val))
                        checkedCount++;
                }

                _isUpdatingCheckState = true;
                try
                {
                    if (checkedCount == _visibleValues.Count)
                    {
                        chkSelectAll.CheckState = CheckState.Checked;
                    }
                    else if (checkedCount == 0)
                    {
                        chkSelectAll.CheckState = CheckState.Unchecked;
                    }
                    else
                    {
                        chkSelectAll.CheckState = CheckState.Indeterminate;
                    }
                }
                finally
                {
                    _isUpdatingCheckState = false;
                }
            }

            private void ChkSelectAll_Click(object sender, EventArgs e)
            {
                bool targetState = (chkSelectAll.CheckState == CheckState.Checked);

                _isUpdatingCheckState = true;
                clbItems.BeginUpdate();
                try
                {
                    for (int i = 0; i < _visibleValues.Count; i++)
                    {
                        string val = _visibleValues[i];
                        if (targetState)
                        {
                            _selectedValues.Add(val);
                            clbItems.SetItemChecked(i, true);
                        }
                        else
                        {
                            _selectedValues.Remove(val);
                            clbItems.SetItemChecked(i, false);
                        }
                    }
                }
                finally
                {
                    clbItems.EndUpdate();
                    _isUpdatingCheckState = false;
                }
            }

            private void ApplyAndClose()
            {
                _onApply?.Invoke(_selectedValues);
                Close();
            }

            private void ClearAndClose()
            {
                _onApply?.Invoke(null);
                Close();
            }
        }
    }
}
