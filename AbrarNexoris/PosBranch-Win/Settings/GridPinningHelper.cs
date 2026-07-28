using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public static class GridPinningHelper
    {
        public static void Attach(DataGridView grid)
        {
            if (grid == null) return;

            grid.ColumnHeaderMouseClick -= Grid_ColumnHeaderMouseClick;
            grid.ColumnHeaderMouseClick += Grid_ColumnHeaderMouseClick;

            grid.CellMouseClick -= Grid_CellMouseClick;
            grid.CellMouseClick += Grid_CellMouseClick;

            grid.MouseDown -= Grid_MouseDown;
            grid.MouseDown += Grid_MouseDown;
        }

        private static void Grid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid == null || e.ColumnIndex < 0 || e.ColumnIndex >= grid.Columns.Count) return;

            var column = grid.Columns[e.ColumnIndex];
            if (!column.Visible) return;

            Point screenPos = Cursor.Position;
            Point clientPos = grid.PointToClient(screenPos);
            ShowHeaderContextMenu(grid, column, clientPos);
        }

        private static void Grid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var grid = sender as DataGridView;
            if (grid == null || e.ColumnIndex < 0 || e.ColumnIndex >= grid.Columns.Count) return;

            var column = grid.Columns[e.ColumnIndex];
            if (!column.Visible) return;

            if (e.RowIndex >= 0 && e.RowIndex < grid.Rows.Count)
            {
                grid.CurrentCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            }

            Point screenPos = Cursor.Position;
            Point clientPos = grid.PointToClient(screenPos);
            ShowCellContextMenu(grid, column, clientPos);
        }

        private static void Grid_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var grid = sender as DataGridView;
            if (grid == null) return;

            var hit = grid.HitTest(e.X, e.Y);
            if (hit.ColumnIndex < 0 || hit.ColumnIndex >= grid.Columns.Count) return;

            var column = grid.Columns[hit.ColumnIndex];
            if (!column.Visible) return;

            ShowCellContextMenu(grid, column, e.Location);
        }

        public static void ShowHeaderContextMenu(DataGridView grid, DataGridViewColumn column, Point location)
        {
            if (grid == null || column == null) return;

            var menu = new ContextMenuStrip
            {
                Font = new Font("Segoe UI", 9F)
            };

            string rawTitle = GetCleanHeaderText(column.HeaderText);
            bool isPinned = column.Frozen;

            var pinMenuItem = new ToolStripMenuItem(
                isPinned ? $"🔓 Unpin '{rawTitle}' Column" : $"📌 Pin / Lock '{rawTitle}' Column",
                null,
                (s, e) => TogglePin(grid, column)
            )
            {
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
            };
            menu.Items.Add(pinMenuItem);

            menu.Items.Add(new ToolStripSeparator());

            var sortAscItem = new ToolStripMenuItem("⬆️ Sort Ascending A-Z / 0-9", null, (s, e) => SortColumn(grid, column, ListSortDirection.Ascending));
            var sortDescItem = new ToolStripMenuItem("⬇️ Sort Descending Z-A / 9-0", null, (s, e) => SortColumn(grid, column, ListSortDirection.Descending));
            menu.Items.Add(sortAscItem);
            menu.Items.Add(sortDescItem);

            int pinnedCount = 0;
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col.Visible && col.Frozen) pinnedCount++;
            }

            if (pinnedCount > 0)
            {
                menu.Items.Add(new ToolStripSeparator());
                var unpinAllMenuItem = new ToolStripMenuItem("🔓 Unpin All Columns", null, (s, e) => UnpinAll(grid));
                menu.Items.Add(unpinAllMenuItem);
            }

            menu.Show(grid, location);
        }

        public static void ShowCellContextMenu(DataGridView grid, DataGridViewColumn column, Point location)
        {
            if (grid == null || column == null) return;

            var menu = new ContextMenuStrip
            {
                Font = new Font("Segoe UI", 9F)
            };

            string rawTitle = GetCleanHeaderText(column.HeaderText);
            bool isPinned = column.Frozen;

            var pinMenuItem = new ToolStripMenuItem(
                isPinned ? $"🔓 Unpin '{rawTitle}' Column" : $"📌 Pin / Lock '{rawTitle}' Column",
                null,
                (s, e) => TogglePin(grid, column)
            )
            {
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
            };
            menu.Items.Add(pinMenuItem);

            int pinnedCount = 0;
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col.Visible && col.Frozen) pinnedCount++;
            }

            if (pinnedCount > 0)
            {
                menu.Items.Add(new ToolStripSeparator());
                var unpinAllMenuItem = new ToolStripMenuItem("🔓 Unpin All Columns", null, (s, e) => UnpinAll(grid));
                menu.Items.Add(unpinAllMenuItem);
            }

            menu.Show(grid, location);
        }

        private static void SortColumn(DataGridView grid, DataGridViewColumn column, ListSortDirection direction)
        {
            try
            {
                if (grid == null || column == null) return;
                if (grid.DataSource is DataTable dt)
                {
                    string prop = string.IsNullOrEmpty(column.DataPropertyName) ? column.Name : column.DataPropertyName;
                    if (dt.Columns.Contains(prop))
                    {
                        dt.DefaultView.Sort = $"[{prop}] {(direction == ListSortDirection.Ascending ? "ASC" : "DESC")}";
                    }
                }
                else
                {
                    grid.Sort(column, direction);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SortColumn error: " + ex.Message);
            }
        }

        public static void TogglePin(DataGridView grid, DataGridViewColumn column)
        {
            if (column == null) return;

            if (column.Frozen)
            {
                UnpinColumn(grid, column);
            }
            else
            {
                PinColumn(grid, column);
            }
        }

        public static void PinColumn(DataGridView grid, DataGridViewColumn targetCol)
        {
            if (grid == null || targetCol == null || !targetCol.Visible) return;
            if (targetCol.Frozen) return;

            var frozenCols = new List<DataGridViewColumn>();
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col.Visible && col.Frozen)
                {
                    frozenCols.Add(col);
                }
            }
            frozenCols.Sort((a, b) => a.DisplayIndex.CompareTo(b.DisplayIndex));
            frozenCols.Add(targetCol);

            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.Frozen = false;
            }

            for (int i = 0; i < frozenCols.Count; i++)
            {
                frozenCols[i].DisplayIndex = i;
                frozenCols[i].Frozen = true;
                string cleanText = GetCleanHeaderText(frozenCols[i].HeaderText);
                frozenCols[i].HeaderText = "📌 " + cleanText;
            }
        }

        public static void UnpinColumn(DataGridView grid, DataGridViewColumn targetCol)
        {
            if (grid == null || targetCol == null) return;

            var remainingFrozen = new List<DataGridViewColumn>();
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col.Visible && col.Frozen && col != targetCol)
                {
                    remainingFrozen.Add(col);
                }
            }
            remainingFrozen.Sort((a, b) => a.DisplayIndex.CompareTo(b.DisplayIndex));

            string cleanTargetText = GetCleanHeaderText(targetCol.HeaderText);
            targetCol.HeaderText = cleanTargetText;

            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.Frozen = false;
            }

            for (int i = 0; i < remainingFrozen.Count; i++)
            {
                remainingFrozen[i].DisplayIndex = i;
                remainingFrozen[i].Frozen = true;
            }
        }

        public static void UnpinAll(DataGridView grid)
        {
            if (grid == null) return;

            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.Frozen = false;
                col.HeaderText = GetCleanHeaderText(col.HeaderText);
            }
        }

        public static string GetCleanHeaderText(string headerText)
        {
            if (string.IsNullOrWhiteSpace(headerText)) return string.Empty;
            if (headerText.StartsWith("📌 ")) return headerText.Substring(3);
            return headerText;
        }
    }
}
