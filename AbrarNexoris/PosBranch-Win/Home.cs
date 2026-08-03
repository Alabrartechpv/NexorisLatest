using ModelClass;
using ModelClass.Report;
using PosBranch_Win.Accounts;
using PosBranch_Win.Master;
using PosBranch_Win.Reports.SalesReports;
using PosBranch_Win.Transaction;
using PosBranch_Win.Utilities;
using PosBranch_Win.Settings;
using Repository;
using Repository.SettingsRepo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;

namespace PosBranch_Win
{
    public partial class Home : Form
    {
        private int childFormNumber = 0;
        private bool isCtrlPressed = false;
        private bool isF2Pressed = false;
        private bool isShiftPressed = false;
        // Track close button rectangles for each tab
        private Dictionary<int, Rectangle> closeButtonRects = new Dictionary<int, Rectangle>();
        private int closeHotTrackTab = -1; // No hot-tracking initially
        private int tabHoverIndex = -1;    // Track which tab is being hovered
        private bool enableTabScrolling = true; // Enable tab scrolling feature
        private Timer scrollTimer = new Timer(); // Timer for smooth scrolling
        private int scrollDirection = 0; // 0 = none, -1 = left, 1 = right
        private string _lastActivatedToolKey = null; // Track which toolbar key opened the current form
        private bool _openingFromFavourite = false; // When true, allow duplicate form instances
        private Timer closingAlertTimer = new Timer();
        private bool closingAlertShown = false;
        private bool closingRequiredAlertShown = false;
        public bool IsLoggingOff { get; set; } = false;

        // Report Navigator fields
        private Infragistics.Win.UltraWinExplorerBar.UltraExplorerBar ultraExplorerBarReportNavigator;
        private Panel panelReportNavigatorWrapper;
        private bool _isReportNavigatorVisible = false;
        private bool _isReportNavigatorPinned = false;
        private Panel _reportNavigatorPinButton;
        private string _activeReportNavigatorItemKey = null;

        private sealed class ReportNavigatorDefinition
        {
            public string Category { get; private set; }
            public string Text { get; private set; }
            public string Key { get; private set; }
            public string ActionKey { get; private set; }

            public ReportNavigatorDefinition(string category, string text, string key, string actionKey = null)
            {
                Category = category;
                Text = text;
                Key = key;
                ActionKey = string.IsNullOrEmpty(actionKey) ? key : actionKey;
            }
        }

        private static readonly ReportNavigatorDefinition[] ReportNavigatorDefinitions = new[]
        {
            new ReportNavigatorDefinition("Item", "Stock Listing", "StockListingReport"),
            new ReportNavigatorDefinition("Item", "Item Report", "ItemReport"),
            new ReportNavigatorDefinition("Item", "Stock Report Advanced", "StockReportAdv", "StockReport"),
            new ReportNavigatorDefinition("Item", "Inventory Audit Trail", "AuditTrail"),
            new ReportNavigatorDefinition("Item", "Stock Valuation Report", "StockValuationReport"),
            new ReportNavigatorDefinition("Item", "Low Stock Alert Report", "LowStockAlertReport"),
            new ReportNavigatorDefinition("Item", "Stock Adjustment Report", "StockAdjustmentReport"),
            new ReportNavigatorDefinition("Sales", "Sales Details", "Sales Details"),
            new ReportNavigatorDefinition("Sales", "Item-wise Sales Summary", "ItemwiseSalesSummaryReport"),
            new ReportNavigatorDefinition("Sales", "Customer-wise Sales Summary", "CustomerwiseSalesSummaryReport"),
            new ReportNavigatorDefinition("Sales", "Salesman-wise Sales Summary", "SalesmanwiseSalesSummaryReport"),
            new ReportNavigatorDefinition("Sales", "Sales Return Report", "SalesReturn"),
            new ReportNavigatorDefinition("Sales", "Sales Profit", "SalesProfit"),
            new ReportNavigatorDefinition("Sales", "Salesman Incentive Report", "SalesmanIncentiveReport"),
            new ReportNavigatorDefinition("Sales", "Daily Sales", "DSales"),
            new ReportNavigatorDefinition("Sales", "Counter Report", "CounterReport"),
            new ReportNavigatorDefinition("Purchase", "Purchase Details", "Purchase Details"),
            new ReportNavigatorDefinition("Purchase", "Purchase Return Report", "PurchaseReturn"),
            new ReportNavigatorDefinition("Purchase", "Vendor Purchase Report", "VendorPurchaseReport"),
            new ReportNavigatorDefinition("Customer", "Customer Outstanding Listing", "CustomerOutstandingReport"),
            new ReportNavigatorDefinition("Customer", "Customer Receipt Report", "CustomerReceiptReport"),
            new ReportNavigatorDefinition("Customer", "Customer Ledger Statement", "CustomerLedgerReport"),
            new ReportNavigatorDefinition("Vendor", "Vendor Outstanding Listing", "VendorOutstandingReport"),
            new ReportNavigatorDefinition("Vendor", "DN/payment", "VendorDNPaymentReport"),
            new ReportNavigatorDefinition("Vendor", "Unallocated Purchase Returns", "UnallocatedPurchaseReturns"),
            new ReportNavigatorDefinition("Analysis", "Trading Account", "TradingAccount"),
            new ReportNavigatorDefinition("Analysis", "Profit & Loss Account", "ProfitLossAccount"),
            new ReportNavigatorDefinition("Analysis", "Balance Sheet", "BalanceSheet"),
            new ReportNavigatorDefinition("Analysis", "Trial Balance", "TrialBalance"),
            new ReportNavigatorDefinition("Analysis", "Cash & Bank Book", "CashBankBook"),
            new ReportNavigatorDefinition("Analysis", "Day Book", "DayBook"),
            new ReportNavigatorDefinition("Analysis", "Counter Closing Report", "ShiftReconciliationReport"),
            new ReportNavigatorDefinition("Analysis", "Bank Statement", "BankStatementReport"),
            new ReportNavigatorDefinition("Others", "Manual Party Balance Report", "ManualPartyBalanceReport"),
            new ReportNavigatorDefinition("Others", "Combined Party Balance Report", "CombinedPartyBalanceReport")
        };

        private static readonly Dictionary<string, string> ReportNavigatorActionAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "StockReportAdv", "StockReport" }
        };

        public Home()
        {
            InitializeComponent();

            // Ribbon tools are now serialized in the designer (Home.Designer.cs)

            // UltraTabControl doesn't need DrawItem handler as it has built-in styling

            // Wire up mouse events for tabs
            tabControlMain.MouseClick += tabControlMain_MouseClick;
            tabControlMain.MouseMove += tabControlMain_MouseMove;
            tabControlMain.MouseUp += tabControlMain_MouseUp;
            tabControlMain.Resize += tabControlMain_Resize;
            tabControlMain.MouseDown += tabControlMain_MouseDown;
            tabControlMain.MouseLeave += tabControlMain_MouseLeave;
            tabControlMain.MouseWheel += tabControlMain_MouseWheel;

            // Wire up mouse events for auto-focusing the Home ribbon tab
            tabControlMain.MouseEnter += MainWorkspace_MouseEnter;
            tabControlMain.MouseMove += MainWorkspace_MouseEnter;
            ultraTabSharedControlsPage1.MouseEnter += MainWorkspace_MouseEnter;
            ultraTabSharedControlsPage1.MouseMove += MainWorkspace_MouseEnter;
            panelMain.MouseEnter += MainWorkspace_MouseEnter;
            panelMain.MouseMove += MainWorkspace_MouseEnter;

            // Set up the timer for smooth scrolling
            scrollTimer.Interval = 50;
            scrollTimer.Tick += ScrollTimer_Tick;

            closingAlertTimer.Interval = 60000;
            closingAlertTimer.Tick += ClosingAlertTimer_Tick;

            // Wire up context menu events
            closeTabToolStripMenuItem.Click += closeTabToolStripMenuItem_Click;
            closeAllTabsToolStripMenuItem.Click += closeAllTabsToolStripMenuItem_Click;

            // Track tab changes for POS-only ribbon actions
            tabControlMain.SelectedTabChanged += TabControlMain_SelectedTabChanged;
            UpdateHoldToolVisibility();



            // Configure specific properties for UltraTabControl v22.2
            // Note: Some properties may not exist in v22.2, so we use try-catch
            try
            {
                // Try to set scroll buttons if the property exists
                var scrollButtonsProp = this.tabControlMain.GetType().GetProperty("ScrollButtons");
                if (scrollButtonsProp != null)
                {
                    scrollButtonsProp.SetValue(this.tabControlMain, true);
                }
            }
            catch { /* Ignore if property doesn't exist */ }

            try
            {
                // Try to set tab orientation if the property exists
                var tabOrientationProp = this.tabControlMain.GetType().GetProperty("TabOrientation");
                if (tabOrientationProp != null)
                {
                    var enumType = tabOrientationProp.PropertyType;
                    try
                    {
                        var topLeftValue = System.Enum.Parse(enumType, "TopLeft");
                        tabOrientationProp.SetValue(this.tabControlMain, topLeftValue);
                    }
                    catch { /* Ignore if enum value doesn't exist */ }
                }
            }
            catch { /* Ignore if property doesn't exist */ }

            // Set appearance - will be themed dynamically

            // Try to enable close buttons (these properties may or may not exist in v22.2)
            try
            {
                // Using reflection to safely set properties that might exist
                var tabControlType = this.tabControlMain.GetType();

                var closeButtonLocationProp = tabControlType.GetProperty("CloseButtonLocation");
                if (closeButtonLocationProp != null)
                {
                    var enumType = closeButtonLocationProp.PropertyType;
                    try
                    {
                        var tabValue = System.Enum.Parse(enumType, "Tab");
                        closeButtonLocationProp.SetValue(this.tabControlMain, tabValue);
                    }
                    catch { /* Ignore if enum value doesn't exist */ }
                }

                var allowTabClosingProp = tabControlType.GetProperty("AllowTabClosing");
                if (allowTabClosingProp != null)
                {
                    allowTabClosingProp.SetValue(this.tabControlMain, true);
                }
            }
            catch
            {
                // If reflection fails, continue without close buttons
            }

        }

        private void MainWorkspace_MouseEnter(object sender, EventArgs e)
        {
            try
            {
                if (ultraToolbarsManager1.Ribbon.SelectedTab != null &&
                    ultraToolbarsManager1.Ribbon.SelectedTab.Key != "Home")
                {
                    if (ultraToolbarsManager1.Ribbon.Tabs.Exists("Home"))
                    {
                        ultraToolbarsManager1.Ribbon.SelectedTab = ultraToolbarsManager1.Ribbon.Tabs["Home"];
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error auto-selecting Home tab: {ex.Message}");
            }
        }

        private void AttachMouseEnterRecursively(Control parentControl)
        {
            try
            {
                // Attach to the current control
                parentControl.MouseEnter -= MainWorkspace_MouseEnter;
                parentControl.MouseMove -= MainWorkspace_MouseEnter;

                parentControl.MouseEnter += MainWorkspace_MouseEnter;
                parentControl.MouseMove += MainWorkspace_MouseEnter;

                // Recursively attach to all child controls
                foreach (Control child in parentControl.Controls)
                {
                    AttachMouseEnterRecursively(child);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error attaching mouse enter recursively: {ex.Message}");
            }
        }

        // Process mouse wheel for tab scrolling
        private void tabControlMain_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!enableTabScrolling || tabControlMain.Tabs.Count <= 1)
                return;

            // Let forms/grids handle normal mouse wheel scrolling.
            // Use Ctrl + MouseWheel over tab header only for tab switching.
            if ((Control.ModifierKeys & Keys.Control) != Keys.Control)
                return;

            if (!IsMouseOverTabHeader(e.Location))
                return;

            if (tabControlMain.ActiveTab == null)
                return;

            // Calculate how many tabs to scroll based on delta
            int scrollCount = (e.Delta > 0) ? -1 : 1;
            int newIndex = Math.Min(Math.Max(0, tabControlMain.ActiveTab.Index + scrollCount),
                tabControlMain.Tabs.Count - 1);

            tabControlMain.ActiveTab = tabControlMain.Tabs[newIndex];
        }

        private bool IsMouseOverTabHeader(Point mouseLocation)
        {
            try
            {
                if (tabControlMain.ActiveTab != null && tabControlMain.ActiveTab.TabPage != null)
                {
                    return mouseLocation.Y < tabControlMain.ActiveTab.TabPage.Top;
                }
            }
            catch
            {
            }

            return mouseLocation.Y <= 32;
        }

        // Timer callback for smooth scrolling animation
        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            if (scrollDirection == 0 || tabControlMain.Tabs.Count <= 1)
            {
                scrollTimer.Stop();
                return;
            }

            int newIndex = tabControlMain.ActiveTab.Index + scrollDirection;
            if (newIndex >= 0 && newIndex < tabControlMain.Tabs.Count)
            {
                tabControlMain.ActiveTab = tabControlMain.Tabs[newIndex];
            }
            else
            {
                scrollTimer.Stop();
                scrollDirection = 0;
            }
        }

        // Track mouse leave to reset hover states
        private void tabControlMain_MouseLeave(object sender, EventArgs e)
        {
            if (tabHoverIndex != -1)
            {
                tabHoverIndex = -1;
                tabControlMain.Invalidate();
            }

            // Stop scrolling when mouse leaves the control
            scrollDirection = 0;
            scrollTimer.Stop();
        }

        // Process mouse down events including middle-click for close
        private void tabControlMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                // Middle click to close tab
                // Find the tab under the mouse cursor and close it
                for (int i = 0; i < tabControlMain.Tabs.Count; i++)
                {
                    var tab = tabControlMain.Tabs[i];
                    if (tab.TabPage.Controls.Count > 0 && tab.TabPage.Controls[0] is Form)
                    {
                        Form form = (Form)tab.TabPage.Controls[0];
                        form.Close();
                        break;
                    }
                }
            }
        }

        // (removed unused UpdateCloseButtonRects)

        // UltraTabControl handles custom drawing automatically - no need for custom DrawItem code

        private void tabControlMain_MouseMove(object sender, MouseEventArgs e)
        {
            // UltraTabControl handles mouse movement automatically
            // No need for custom mouse tracking with UltraTabControl
        }

        private void tabControlMain_MouseClick(object sender, MouseEventArgs e)
        {
            // UltraTabControl handles tab clicking automatically
            // No need for custom click handling with UltraTabControl
        }

        private void tabControlMain_MouseUp(object sender, MouseEventArgs e)
        {
            // UltraTabControl handles context menu automatically
            // No need for custom right-click handling with UltraTabControl
        }

        private void tabControlMain_Resize(object sender, EventArgs e)
        {
            // Force redraw to update close button positions
            tabControlMain.Invalidate();

            // CRITICAL FIX: Ensure all forms in tabs are properly resized
            EnsureAllTabFormsAreProperlySized();
        }

        // CRITICAL FIX: Method to ensure all forms in tabs are properly sized
        private void EnsureAllTabFormsAreProperlySized()
        {
            try
            {
                foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
                {
                    if (tab.TabPage.Controls.Count > 0 && tab.TabPage.Controls[0] is Form form)
                    {
                        if (!form.IsDisposed && form.Visible)
                        {
                            // Force form to fill the tab page
                            form.Dock = DockStyle.Fill;
                            form.Size = tab.TabPage.Size;
                            form.Location = new Point(0, 0);

                            // Force layout updates
                            form.PerformLayout();
                            form.Invalidate();
                            form.Update();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring tab forms are properly sized: {ex.Message}");
            }
        }

        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseCurrentTab();
        }

        private void closeAllTabsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseAllTabs();
        }

        private void CloseCurrentTab()
        {
            if (tabControlMain.ActiveTab != null && tabControlMain.Tabs.Count > 0)
            {
                var activeTab = tabControlMain.ActiveTab;

                // Close the form in the tab
                if (activeTab.TabPage.Controls.Count > 0 && activeTab.TabPage.Controls[0] is Form)
                {
                    Form form = (Form)activeTab.TabPage.Controls[0];

                    // CRITICAL FIX: Properly dispose form to prevent memory leaks
                    try
                    {
                        // Unsubscribe from events first
                        form.FormClosed -= null; // Remove any event handlers

                        // Close the form properly
                        form.Close();

                        // Force disposal
                        form.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error disposing form: {ex.Message}");
                    }
                }

                // Remove the tab after closing the form
                tabControlMain.Tabs.Remove(activeTab);
            }
        }

        private void CloseAllTabs()
        {
            // Close all forms in tabs
            for (int i = tabControlMain.Tabs.Count - 1; i >= 0; i--)
            {
                var tab = tabControlMain.Tabs[i];
                if (tab.TabPage.Controls.Count > 0 && tab.TabPage.Controls[0] is Form)
                {
                    Form form = (Form)tab.TabPage.Controls[0];

                    // CRITICAL FIX: Properly dispose each form
                    try
                    {
                        // Unsubscribe from events first
                        form.FormClosed -= null; // Remove any event handlers

                        // Close the form properly
                        form.Close();

                        // Force disposal
                        form.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error disposing form in CloseAllTabs: {ex.Message}");
                    }
                }
            }

            // UltraTabControl will handle tab removal automatically
            // Clear all close button rectangles
            closeButtonRects.Clear();
        }

        private void LogLogout(string details)
        {
            try
            {
                if (SessionContext.UserId > 0)
                {
                    using (var repo = new UserActivityLogRepository())
                    {
                        repo.SaveUserActivity(
                            userId: SessionContext.UserId,
                            userName: SessionContext.UserName,
                            userRole: SessionContext.UserLevel,
                            counterId: SessionContext.CounterId,
                            counterName: SessionContext.CounterName,
                            activityType: "Logout",
                            activityDetails: details,
                            formName: null,
                            sessionId: SessionContext.CounterSessionId
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging logout: {ex.Message}");
            }
        }

        private void LogFormEntry(string formName)
        {
            try
            {
                if (SessionContext.UserId > 0)
                {
                    using (var repo = new UserActivityLogRepository())
                    {
                        repo.SaveUserActivity(
                            userId: SessionContext.UserId,
                            userName: SessionContext.UserName,
                            userRole: SessionContext.UserLevel,
                            counterId: SessionContext.CounterId,
                            counterName: SessionContext.CounterName,
                            activityType: "FormEntry",
                            activityDetails: $"Entered form: {formName}",
                            formName: formName,
                            sessionId: SessionContext.CounterSessionId
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging form entry: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!IsLoggingOff)
            {
                LogLogout("User closed application");
            }
            SessionContext.Clear();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Opens a form in a new tab or switches to its tab if already open
        /// </summary>
        private void OpenFormInTab(Form formToOpen, string tabName)
        {
            try
            {
                LogFormEntry(tabName);
                // Auto-close Report Navigator when any form is opened, unless the user pinned it.
                if (_isReportNavigatorVisible && !_isReportNavigatorPinned) HideReportNavigator();
                // Check if tab already exists (skip when opening from favourites to allow multiple instances)
                if (!_openingFromFavourite)
                {
                    foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
                    {
                        if (tab.Text == tabName)
                        {
                            tabControlMain.SelectedTab = tab;
                            return;
                        }
                    }
                }
                else
                {
                    // Generate unique tab name for favourite duplicates
                    int count = 1;
                    string originalName = tabName;
                    while (tabControlMain.Tabs.Cast<Infragistics.Win.UltraWinTabControl.UltraTab>().Any(t => t.Text == tabName))
                    {
                        count++;
                        tabName = $"{originalName} ({count})";
                    }
                }

                // Create new tab
                string uniqueKey = $"Tab_{DateTime.Now.Ticks}_{tabName}";
                var newTab = tabControlMain.Tabs.Add(uniqueKey, tabName);
                newTab.Tag = _lastActivatedToolKey; // Store toolbar key for favorites

                // CRITICAL FIX: Ensure form is properly initialized BEFORE setting properties
                EnsureFormIsReady(formToOpen);

                // IMPORTANT: Set form properties BEFORE adding to tab
                formToOpen.TopLevel = false;
                formToOpen.FormBorderStyle = FormBorderStyle.None;
                formToOpen.Dock = DockStyle.Fill;
                formToOpen.Visible = true;
                formToOpen.WindowState = FormWindowState.Normal;

                // CRITICAL FIX: Set form size to match tab page size
                formToOpen.Size = newTab.TabPage.Size;
                formToOpen.Location = new Point(0, 0);

                // Apply current theme color to the new form
                formToOpen.BackColor = Color.FromArgb(
                    Math.Min(255, currentThemeColor.R + 5),
                    Math.Min(255, currentThemeColor.G + 5),
                    Math.Min(255, currentThemeColor.B + 5)
                );

                // Apply theme to all controls in the new form
                ApplyThemeToControls(formToOpen.Controls, currentThemeColor);

                // Add the form to the tab page
                newTab.TabPage.Controls.Add(formToOpen);

                // CRITICAL FIX: Force layout updates after adding to tab
                newTab.TabPage.SuspendLayout();
                formToOpen.SuspendLayout();

                // Show the form AFTER adding to tab page
                formToOpen.Show();
                formToOpen.BringToFront();

                // User Request: Apply auto-focus Home tab event to the fully loaded form
                AttachMouseEnterRecursively(formToOpen);

                // CRITICAL FIX: Resume layout and force proper sizing
                formToOpen.ResumeLayout(false);
                newTab.TabPage.ResumeLayout(false);

                // CRITICAL FIX: Force form to fill the entire tab page
                formToOpen.Dock = DockStyle.Fill;
                formToOpen.PerformLayout();
                newTab.TabPage.PerformLayout();

                // Set the new tab as active/selected
                tabControlMain.SelectedTab = newTab;

                // Apply active styling to the new tab
                ApplyTabStyling(newTab, true);

                // Update Hold/LastBill visibility based on active tab
                UpdateHoldToolVisibility();

                // CRITICAL FIX: Force multiple refresh cycles to ensure proper layout
                Application.DoEvents();
                newTab.TabPage.Refresh();
                formToOpen.Refresh();
                tabControlMain.Refresh();

                // CRITICAL FIX: Additional layout enforcement
                formToOpen.Invalidate();
                formToOpen.Update();
                newTab.TabPage.Invalidate();
                newTab.TabPage.Update();

                // Focus the form and its initial active control immediately
                try
                {
                    formToOpen.Focus();
                    formToOpen.Select();
                    if (formToOpen.ActiveControl != null)
                    {
                        formToOpen.ActiveControl.Focus();
                    }
                }
                catch { }

                // Wire up the form's FormClosed event to remove the tab
                formToOpen.FormClosed += (sender, e) =>
                {
                    try
                    {
                        if (newTab != null && tabControlMain.Tabs.Contains(newTab))
                        {
                            tabControlMain.Tabs.Remove(newTab);
                        }
                        SyncReportNavigatorActiveItemWithOpenTabs();
                        UpdateHoldToolVisibility();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error removing tab: {ex.Message}");
                    }
                };

                System.Diagnostics.Debug.WriteLine($"Successfully created tab '{tabName}' with {formToOpen.Controls.Count} controls");
                System.Diagnostics.Debug.WriteLine($"Form visible: {formToOpen.Visible}, Handle created: {formToOpen.IsHandleCreated}");
                System.Diagnostics.Debug.WriteLine($"Form size: {formToOpen.Size}, Tab page size: {newTab.TabPage.Size}");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening form in tab: {ex.Message}");
                MessageBox.Show($"Error opening form: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Clean up on error
                if (formToOpen != null && !formToOpen.IsDisposed)
                {
                    formToOpen.Dispose();
                }
            }
        }

        // Additional helper method to ensure proper form initialization
        private void EnsureFormIsReady(Form form)
        {
            try
            {
                // CRITICAL FIX: Ensure form is not disposed
                if (form.IsDisposed)
                {
                    throw new ObjectDisposedException("Form is already disposed");
                }

                // Force handle creation if not already created
                if (!form.IsHandleCreated)
                {
                    var handle = form.Handle; // This forces handle creation
                }

                // CRITICAL FIX: Ensure form is in proper state for embedding
                form.WindowState = FormWindowState.Normal;
                form.StartPosition = FormStartPosition.Manual;

                // Ensure all child controls are created
                foreach (Control control in form.Controls)
                {
                    if (!control.IsHandleCreated)
                    {
                        var handle = control.Handle;
                    }
                }

                // CRITICAL FIX: Call PerformLayout to ensure proper layout
                form.SuspendLayout();
                form.PerformLayout();
                form.ResumeLayout(false);

                // CRITICAL FIX: Force form to be ready for embedding
                form.Visible = false; // Will be set to true later

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring form is ready: {ex.Message}");
                throw; // Re-throw to prevent embedding a broken form
            }
        }

        public void OpenSmartReorderPurchaseOrder(IEnumerable<SmartReorderItemModel> items)
        {
            List<SmartReorderItemModel> reorderItems = items == null
                ? new List<SmartReorderItemModel>()
                : items.Where(x => x != null && x.FinalQuantity > 0).ToList();

            if (reorderItems.Count == 0)
                return;

            frmPurchaseOrder existingPurchaseOrder = FindOpenPurchaseOrderForm();
            if (existingPurchaseOrder != null)
            {
                existingPurchaseOrder.LoadSmartReorderItems(reorderItems);
                SelectTabForForm(existingPurchaseOrder);
                existingPurchaseOrder.BringToFront();
                existingPurchaseOrder.Focus();
                return;
            }

            frmPurchaseOrder purchaseOrder = new frmPurchaseOrder();
            purchaseOrder.LoadSmartReorderItems(reorderItems);
            OpenFormInTab(purchaseOrder, "Purchase Order");
        }

        private frmPurchaseOrder FindOpenPurchaseOrderForm()
        {
            foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
            {
                frmPurchaseOrder purchaseOrder = tab.TabPage.Controls.OfType<frmPurchaseOrder>().FirstOrDefault();
                if (purchaseOrder != null)
                {
                    return purchaseOrder;
                }
            }

            return null;
        }

        private void SelectTabForForm(Form form)
        {
            foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
            {
                if (!tab.TabPage.Controls.Contains(form))
                    continue;

                tabControlMain.SelectedTab = tab;
                ApplyTabStyling(tab, true);
                UpdateHoldToolVisibility();
                tab.TabPage.Refresh();
                break;
            }
        }

        // Modified version with additional safety checks
        private void OpenFormInTabSafe(Form formToOpen, string tabName)
        {
            try
            {
                LogFormEntry(tabName);
                // Auto-close Report Navigator when any form is opened, unless the user pinned it.
                if (_isReportNavigatorVisible && !_isReportNavigatorPinned) HideReportNavigator();
                // Check if tab already exists (skip when opening from favourites to allow multiple instances)
                if (!_openingFromFavourite)
                {
                    foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
                    {
                        if (tab.Text == tabName)
                        {
                            if (tab.TabPage.Controls.Count > 0 && tab.TabPage.Controls[0] is Form existingForm)
                            {
                                if (existingForm.IsDisposed)
                                {
                                    tabControlMain.Tabs.Remove(tab);
                                    break;
                                }
                                else
                                {
                                    tabControlMain.SelectedTab = tab;
                                    existingForm.BringToFront();
                                    existingForm.Focus();
                                    return;
                                }
                            }
                            else
                            {
                                tabControlMain.SelectedTab = tab;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    // Generate unique tab name for favourite duplicates
                    int count = 1;
                    string originalName = tabName;
                    while (tabControlMain.Tabs.Cast<Infragistics.Win.UltraWinTabControl.UltraTab>().Any(t => t.Text == tabName))
                    {
                        count++;
                        tabName = $"{originalName} ({count})";
                    }
                }

                // Ensure form is properly initialized before embedding
                EnsureFormIsReady(formToOpen);

                // Set form properties for embedding
                formToOpen.TopLevel = false;
                formToOpen.FormBorderStyle = FormBorderStyle.None;
                formToOpen.Dock = DockStyle.Fill;
                formToOpen.Visible = true;
                formToOpen.WindowState = FormWindowState.Normal;

                // Apply current theme color to the new form
                formToOpen.BackColor = Color.FromArgb(
                    Math.Min(255, currentThemeColor.R + 5),
                    Math.Min(255, currentThemeColor.G + 5),
                    Math.Min(255, currentThemeColor.B + 5)
                );

                // Apply theme to all controls in the new form
                ApplyThemeToControls(formToOpen.Controls, currentThemeColor);

                // Create new tab
                string uniqueKey = $"Tab_{DateTime.Now.Ticks}_{tabName}";
                var newTab = tabControlMain.Tabs.Add(uniqueKey, tabName);
                newTab.Tag = _lastActivatedToolKey; // Store toolbar key for favorites

                // Add form to tab page
                newTab.TabPage.Controls.Add(formToOpen);

                // Activate the tab
                tabControlMain.SelectedTab = newTab;

                // Apply active styling to the new tab
                ApplyTabStyling(newTab, true);

                // Update Hold/LastBill visibility based on active tab
                UpdateHoldToolVisibility();

                // Show and focus the form
                formToOpen.Show();
                formToOpen.BringToFront();
                formToOpen.Focus();

                // User Request: Apply auto-focus Home tab event to the fully loaded form
                AttachMouseEnterRecursively(formToOpen);

                // Force layout and refresh
                newTab.TabPage.PerformLayout();
                formToOpen.PerformLayout();
                Application.DoEvents(); // Process any pending messages

                // Wire up cleanup event
                formToOpen.FormClosed += (sender, e) =>
                {
                    try
                    {
                        if (newTab != null && tabControlMain.Tabs.Contains(newTab))
                        {
                            // Remove all controls first, then the tab
                            newTab.TabPage.Controls.Clear();
                            tabControlMain.Tabs.Remove(newTab);
                            // Force garbage collection of the closed form sooner
                            GC.Collect();
                        }
                        SyncReportNavigatorActiveItemWithOpenTabs();
                        UpdateHoldToolVisibility();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error removing tab: {ex.Message}");
                    }
                };

                System.Diagnostics.Debug.WriteLine($"Successfully opened '{tabName}' in new tab");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OpenFormInTabSafe: {ex.Message}");
                MessageBox.Show($"Error opening form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (formToOpen != null && !formToOpen.IsDisposed)
                {
                    formToOpen.Dispose();
                }
            }
        }

        // (removed legacy template menu handlers and test button handlers)

        private void Home_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;

            toolStripStatusLabel1.Text = DataBase.Branch;
            toolStripStatusUserValLabel3.Text = DataBase.UserName;
            InitializeCustomStatusBar();
            ultraRadialMenu1.Show(this, new Point(Bounds.Right, Bounds.Top));

            // Initialize Report Navigator sidebar
            InitializeReportNavigator();
            closingAlertTimer.Start();

            // Set ExplorerBar Style to Outlook Navigation Pane
            ultraExplorerBarSideMenu.Style = Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarStyle.OutlookNavigationPane;
            try
            {
                // Ensure Sidebar is minimized initially (like IRS POS reference)
                ultraExplorerBarSideMenu.NavigationPaneExpandedState = Infragistics.Win.UltraWinExplorerBar.NavigationPaneExpandedState.Collapsed;
                ultraExplorerBarSideMenu.Style = Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarStyle.OutlookNavigationPane;

                // Handle item removing to bypass any built-in confirmation prompt
                ultraExplorerBarSideMenu.ItemRemoving += (s, args) =>
                {
                    // Just let it remove without prompt, but check if it's the Add button
                    if (args.Item.Key == "AddToFavourite")
                        args.Cancel = true; // Never allow removing the add button
                };

                // Handle item removed to persist the removal to file
                ultraExplorerBarSideMenu.ItemRemoved += (s, args) =>
                {
                    SaveAllFavorites();
                };

                // Add styling globally
                ultraExplorerBarSideMenu.NavigationPaneExpansionMode = Infragistics.Win.UltraWinExplorerBar.NavigationPaneExpansionMode.OnButtonClick;
                ultraExplorerBarSideMenu.NavigationOverflowButtonAreaVisible = false;
                ultraExplorerBarSideMenu.ItemClick += ultraExplorerBarSideMenu_ItemClick;
                ultraExplorerBarSideMenu.ContextMenuInitializing += ultraExplorerBarSideMenu_ContextMenuInitializing;
                LoadFavorites();

                // === STYLING TO MATCH IRS POS DESIGN ===

                // Style the overall ExplorerBar background
                ultraExplorerBarSideMenu.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
                ultraExplorerBarSideMenu.Appearance.BackColor = Color.FromArgb(215, 236, 255);

                // Style the "My Favourite Menu" group header
                var favGroup = ultraExplorerBarSideMenu.Groups["MyFavouriteMenu"];
                if (favGroup != null)
                {
                    // Set collapsed pane text to show "My Favourite Menu" instead of "Navigation Pane"
                    favGroup.Settings.NavigationPaneCollapsedGroupAreaText = "My Favourite Menu";
                    // Group header appearance - vibrant gradient
                    favGroup.Settings.AppearancesSmall.HeaderAppearance.BackColor = Color.FromArgb(0, 174, 219);
                    favGroup.Settings.AppearancesSmall.HeaderAppearance.BackColor2 = Color.FromArgb(0, 140, 186);
                    favGroup.Settings.AppearancesSmall.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                    favGroup.Settings.AppearancesSmall.HeaderAppearance.ForeColor = Color.White;
                    favGroup.Settings.AppearancesSmall.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                    favGroup.Settings.AppearancesSmall.HeaderAppearance.FontData.SizeInPoints = 9f;

                    // Add heart icon to the group header (shows in minimized view)
                    try
                    {
                        favGroup.Settings.AppearancesSmall.HeaderAppearance.Image = PosBranch_Win.Properties.Resources.heart;
                    }
                    catch { }


                    favGroup.Settings.AppearancesSmall.HeaderAppearance.FontData.Name = "Segoe UI";

                    // Hot-track (hover) for group header 
                    favGroup.Settings.AppearancesSmall.HeaderHotTrackAppearance.BackColor = Color.FromArgb(0, 190, 235);
                    favGroup.Settings.AppearancesSmall.HeaderHotTrackAppearance.BackColor2 = Color.FromArgb(0, 155, 200);
                    favGroup.Settings.AppearancesSmall.HeaderHotTrackAppearance.ForeColor = Color.White;

                    // Item area background - clean light blue
                    favGroup.Settings.AppearancesSmall.ItemAreaAppearance.BackColor = Color.FromArgb(215, 236, 255);

                    // Equal padding on both sides so buttons don't touch the border
                    favGroup.Settings.ItemAreaInnerMargins.Left = 8;
                    favGroup.Settings.ItemAreaInnerMargins.Right = 8;
                    favGroup.Settings.ItemAreaInnerMargins.Top = 3;
                    favGroup.Settings.ItemAreaInnerMargins.Bottom = 3;
                    favGroup.Settings.ItemAreaOuterMargins.Left = 0;
                    favGroup.Settings.ItemAreaOuterMargins.Right = 0;
                    favGroup.Settings.ItemAreaOuterMargins.Top = 0;
                    favGroup.Settings.ItemAreaOuterMargins.Bottom = 0;

                    // Style all items in the favourite group as flat blue buttons
                    foreach (Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarItem item in favGroup.Items)
                    {
                        StyleFavouriteItem(item);
                    }

                    // Force refresh after styling
                    ultraExplorerBarSideMenu.Refresh();
                }

                // Style "My Screen Colour" group header
                var screenGroup = ultraExplorerBarSideMenu.Groups["MyScreenColour"];
                if (screenGroup != null)
                {
                    screenGroup.Settings.AppearancesSmall.HeaderAppearance.BackColor = Color.FromArgb(0, 174, 219);
                    screenGroup.Settings.AppearancesSmall.HeaderAppearance.BackColor2 = Color.FromArgb(0, 140, 186);
                    screenGroup.Settings.AppearancesSmall.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                    screenGroup.Settings.AppearancesSmall.HeaderAppearance.ForeColor = Color.White;
                    screenGroup.Settings.AppearancesSmall.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                    screenGroup.Settings.AppearancesSmall.HeaderAppearance.FontData.SizeInPoints = 9f;
                    screenGroup.Settings.AppearancesSmall.HeaderHotTrackAppearance.BackColor = Color.FromArgb(0, 190, 235);
                    screenGroup.Settings.AppearancesSmall.HeaderHotTrackAppearance.ForeColor = Color.White;

                    // Add icon to the group header (shows in minimized view)
                    try
                    {
                        screenGroup.Settings.AppearancesSmall.HeaderAppearance.Image = PosBranch_Win.Properties.Resources.color_selection;
                    }
                    catch { }
                }

                // Populate color palette blocks in MyScreenColour group
                PopulateColorPalette();

                // Style "My Language" group header
                var langGroup = ultraExplorerBarSideMenu.Groups["MyLanguage"];
                if (langGroup != null)
                {
                    langGroup.Settings.AppearancesSmall.HeaderAppearance.BackColor = Color.FromArgb(0, 174, 219);
                    langGroup.Settings.AppearancesSmall.HeaderAppearance.BackColor2 = Color.FromArgb(0, 140, 186);
                    langGroup.Settings.AppearancesSmall.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                    langGroup.Settings.AppearancesSmall.HeaderAppearance.ForeColor = Color.White;
                    langGroup.Settings.AppearancesSmall.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                    langGroup.Settings.AppearancesSmall.HeaderAppearance.FontData.SizeInPoints = 9f;
                    langGroup.Settings.AppearancesSmall.HeaderHotTrackAppearance.BackColor = Color.FromArgb(0, 190, 235);
                    langGroup.Settings.AppearancesSmall.HeaderHotTrackAppearance.ForeColor = Color.White;

                    // Add icon to the group header (shows in minimized view)
                    try
                    {
                        langGroup.Settings.AppearancesSmall.HeaderAppearance.Image = PosBranch_Win.Properties.Resources.languages__2_;
                    }
                    catch { }
                }
            }
            catch { } // If property doesn't exist

            // Fix tab control appearance
            AdjustTabControlAppearance();

            // Apply role-based permissions to toolbar buttons
            ApplyRolePermissions();

            // Apply watermark logo to background
            ApplyWatermark();

            // Set tabControlMain BackColor programmatically - will be themed dynamically



            // Apply initial theme color
            UpdateHoldToolVisibility();
        }

        private void ApplyWatermark()
        {
            try
            {
                string logoPath = null;
                string[] possiblePaths = new string[]
                {
                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources", "splash_logo.png"),
                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Resources", "splash_logo.png"),
                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "Resources", "splash_logo.png")
                };

                foreach (string path in possiblePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        logoPath = path;
                        break;
                    }
                }

                if (logoPath != null)
                {
                    using (Image original = Image.FromFile(logoPath))
                    {
                        // Create a watermark with 15% opacity
                        Bitmap watermark = new Bitmap(original.Width, original.Height);
                        using (Graphics g = Graphics.FromImage(watermark))
                        {
                            System.Drawing.Imaging.ColorMatrix matrix = new System.Drawing.Imaging.ColorMatrix();
                            matrix.Matrix33 = 0.15f; // 15% Opacity for "lil minimal blend in"
                            System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes();
                            attributes.SetColorMatrix(matrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);
                            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                        }

                        // Set as background
                        this.tabControlMain.Appearance.ImageBackground = watermark;
                        this.tabControlMain.Appearance.ImageBackgroundStyle = Infragistics.Win.ImageBackgroundStyle.Centered;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying watermark: {ex.Message}");
            }
        }

        /// <summary>
        /// Adjusts the tab control appearance
        /// </summary>
        private void AdjustTabControlAppearance()
        {
            try
            {
                // Disable OS themes for custom styling
                tabControlMain.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

                // Set matching light blue background
                tabControlMain.BackColor = System.Drawing.Color.FromArgb(240, 248, 255);

                // Configure tab appearance for custom styling
                ConfigureTabAppearance();

                // Wire up tab closing events
                tabControlMain.TabClosed += TabControlMain_TabClosed;
                tabControlMain.TabClosing += TabControlMain_TabClosing;

                tabControlMain.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adjusting tab control appearance: {ex.Message}");
            }
        }

        /// <summary>
        /// Configures tab appearance and handles selection changes
        /// </summary>
        private void ConfigureTabAppearance()
        {
            try
            {
                // Set tab appearance using reflection with matching colors
                SetTabAppearanceProperty("Appearance", "BackColor", System.Drawing.Color.FromArgb(240, 248, 255)); // Light blue background
                SetTabAppearanceProperty("Appearance", "BorderColor", System.Drawing.Color.FromArgb(0, 120, 215)); // Matching blue border
                SetTabAppearanceProperty("ActiveTabAppearance", "BackColor", System.Drawing.Color.FromArgb(0, 120, 215)); // Bright blue active
                SetTabAppearanceProperty("ActiveTabAppearance", "ForeColor", System.Drawing.Color.White); // White text
                SetTabAppearanceProperty("HotTrackAppearance", "BackColor", System.Drawing.Color.FromArgb(173, 216, 230)); // Light blue hover
                SetTabAppearanceProperty("TabHeaderAppearance", "BackColor", System.Drawing.Color.FromArgb(240, 248, 255)); // Light blue tab area

                // Wire up tab selection event
                tabControlMain.SelectedTabChanged += (sender, e) =>
                {
                    if (e.Tab != null) ApplyTabStyling(e.Tab, true);
                    foreach (var tab in tabControlMain.Tabs) if (tab != e.Tab) ApplyTabStyling(tab, false);

                    // CRITICAL FIX: Ensure form in selected tab is properly sized
                    if (e.Tab != null && e.Tab.TabPage.Controls.Count > 0 && e.Tab.TabPage.Controls[0] is Form form)
                    {
                        if (!form.IsDisposed && form.Visible)
                        {
                            // Force form to fill the tab page when tab is selected
                            form.Dock = DockStyle.Fill;
                            form.Size = e.Tab.TabPage.Size;
                            form.Location = new Point(0, 0);

                            // Force layout updates
                            form.PerformLayout();
                            form.Invalidate();
                            form.Update();

                            // Bring form to front
                            form.BringToFront();
                        }
                    }
                };
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Tab config error: {ex.Message}"); }
        }

        /// <summary>
        /// Helper method to set tab appearance properties
        /// </summary>
        private void SetTabAppearanceProperty(string propertyName, string subProperty, object value)
        {
            try
            {
                var prop = tabControlMain.GetType().GetProperty(propertyName);
                var obj = prop?.GetValue(tabControlMain);
                var subProp = obj?.GetType().GetProperty(subProperty);
                subProp?.SetValue(obj, value);
            }
            catch { /* Ignore reflection errors */ }
        }

        /// <summary>
        /// Applies styling to a tab (active or inactive)
        /// </summary>
        private void ApplyTabStyling(Infragistics.Win.UltraWinTabControl.UltraTab tab, bool isActive)
        {
            try
            {
                var appearance = tab.GetType().GetProperty("Appearance")?.GetValue(tab);
                if (appearance != null)
                {
                    appearance.GetType().GetProperty("BackColor")?.SetValue(appearance,
                        isActive ? System.Drawing.Color.FromArgb(0, 120, 215) : System.Drawing.SystemColors.Control);
                    appearance.GetType().GetProperty("ForeColor")?.SetValue(appearance,
                        isActive ? System.Drawing.Color.White : System.Drawing.SystemColors.ControlText);
                }
            }
            catch { /* Ignore reflection errors */ }
        }

        /// <summary>
        /// Tries to enable close buttons using reflection for UltraTabControl v22.2
        /// </summary>
        private void EnableCloseButtons()
        {
            try
            {
                // Use reflection to access properties that might exist in v22.2
                var type = tabControlMain.GetType();

                // Try to set CloseButtonVisibility
                var closeButtonProp = type.GetProperty("CloseButtonVisibility");
                if (closeButtonProp != null)
                {
                    // Try to find the enum value
                    var enumType = closeButtonProp.PropertyType;
                    var alwaysValue = Enum.Parse(enumType, "Always");
                    closeButtonProp.SetValue(tabControlMain, alwaysValue);
                }

                // Try to set AllowTabClosing
                var allowClosingProp = type.GetProperty("AllowTabClosing");
                if (allowClosingProp != null)
                {
                    allowClosingProp.SetValue(tabControlMain, true);
                }

                // Try alternative property names
                var closeButtonProp2 = type.GetProperty("ShowCloseButton");
                if (closeButtonProp2 != null)
                {
                    closeButtonProp2.SetValue(tabControlMain, true);
                }

                var closeButtonProp3 = type.GetProperty("CloseButton");
                if (closeButtonProp3 != null)
                {
                    closeButtonProp3.SetValue(tabControlMain, true);
                }
            }
            catch (Exception ex)
            {
                // If reflection fails, just continue without close buttons
                System.Diagnostics.Debug.WriteLine($"Could not enable close buttons: {ex.Message}");
            }
        }


        /// <summary>
        /// Handles tab closing event
        /// </summary>
        private void TabControlMain_TabClosed(object sender, Infragistics.Win.UltraWinTabControl.TabClosedEventArgs e)
        {
            try
            {
                // Clean up any resources when a tab is closed
                if (e.Tab.TabPage.Controls.Count > 0 && e.Tab.TabPage.Controls[0] is Form)
                {
                    Form form = (Form)e.Tab.TabPage.Controls[0];
                    if (!form.IsDisposed)
                    {
                        try
                        {
                            // Attempt to unsubscribe generic handlers
                            form.FormClosed -= null;
                        }
                        catch { }
                        form.Close();
                        form.Dispose();
                    }
                    // Clear the tab page to drop references
                    e.Tab.TabPage.Controls.Clear();
                    e.Tab.TabPage.Dispose();
                }
                System.Diagnostics.Debug.WriteLine($"Tab closed: {e.Tab.Text}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TabClosed event: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles tab closing event (before closing)
        /// </summary>
        private void TabControlMain_TabClosing(object sender, Infragistics.Win.UltraWinTabControl.TabClosingEventArgs e)
        {
            try
            {
                // You can cancel the closing here if needed
                // e.Cancel = true;
                System.Diagnostics.Debug.WriteLine($"Tab closing: {e.Tab.Text}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TabClosing event: {ex.Message}");
            }
        }

        private void TabControlMain_SelectedTabChanged(object sender, Infragistics.Win.UltraWinTabControl.SelectedTabChangedEventArgs e)
        {
            UpdateHoldToolVisibility();
            try
            {
                var activeForm = GetActiveTabForm();
                if (activeForm != null)
                {
                    activeForm.Focus();
                    activeForm.Select();
                    if (activeForm.ActiveControl != null)
                    {
                        activeForm.ActiveControl.Focus();
                    }
                }
            }
            catch { }
        }

        private void UpdateHoldToolVisibility()
        {
            try
            {
                bool isPosActive = false;
                var activeForm = GetActiveTabForm();
                if (activeForm is frmPos || activeForm is Transaction.frmSalesInvoice)
                {
                    isPosActive = true;
                }

                if (ultraToolbarsManager1?.Ribbon != null &&
                    ultraToolbarsManager1.Ribbon.Tabs.Exists("Home"))
                {
                    var homeTab = ultraToolbarsManager1.Ribbon.Tabs["Home"];
                    if (homeTab.Groups.Exists("ribbonGroupHold"))
                    {
                        homeTab.Groups["ribbonGroupHold"].Visible = isPosActive;
                    }
                    if (homeTab.Groups.Exists("ribbonGroupLastBill"))
                    {
                        homeTab.Groups["ribbonGroupLastBill"].Visible = isPosActive;
                    }
                }

                if (ultraToolbarsManager1.Tools.Exists("Hold"))
                {
                    ultraToolbarsManager1.Tools["Hold"].SharedProps.Visible = isPosActive;
                    ultraToolbarsManager1.Tools["Hold"].SharedProps.Enabled = isPosActive;
                }
                if (ultraToolbarsManager1.Tools.Exists("LastBill"))
                {
                    ultraToolbarsManager1.Tools["LastBill"].SharedProps.Visible = isPosActive;
                    ultraToolbarsManager1.Tools["LastBill"].SharedProps.Enabled = isPosActive;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating Hold/LastBill visibility: {ex.Message}");
            }
        }








        // (test button click handlers removed)

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {

        }

        // (removed unused barcode menu handler)

        // (removed unused POS menu handlers)

        // (removed obsolete ToolClick handler, using ultraToolbarsManager1_ToolClick_1)

        /// <summary>
        /// Applies role-based permissions to toolbar buttons.
        /// Disables buttons that the user does not have CanView permission for.
        /// </summary>
        private void ApplyRolePermissions()
        {
            try
            {
                // Get all tools from the toolbar manager
                foreach (Infragistics.Win.UltraWinToolbars.ToolBase tool in ultraToolbarsManager1.Tools)
                {
                    try
                    {
                        string toolKey = tool.Key?.Trim();

                        // Check if this is a global action tool that should always be enabled
                        bool isGlobalAction = toolKey == "Save" ||
                                             toolKey == "Delet" ||
                                             toolKey == "Clear" ||
                                             toolKey == "Remove" ||
                                             toolKey == "Exit" ||
                                             toolKey == "Report" ||
                                             toolKey == "LogOff" ||
                                             toolKey == "LogIn" ||
                                             toolKey == "ReOrder" ||
                                             toolKey == "Overview" ||
                                             toolKey == "Hold" ||
                                             toolKey == "LastBill" ||
                                             toolKey == "Database";

                        bool hasPermission = isGlobalAction || SessionContext.CanView(toolKey) || SessionContext.CanView(tool.Key);
                        
                        if (tool.SharedProps != null)
                        {
                            tool.SharedProps.Enabled = hasPermission;
                        }

                        System.Diagnostics.Debug.WriteLine($"Tool '{tool.Key}': Enabled={hasPermission}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error applying permission to tool '{tool?.Key}': {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Applied permissions to {ultraToolbarsManager1.Tools.Count} toolbar items");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying role permissions: {ex.Message}");
                // Continue without disabling buttons on error
            }
        }

        /// <summary>
        /// Checks if user has CanView permission for the specified form key.
        /// Shows access denied message if not permitted.
        /// </summary>
        /// <param name="formKey">Form key to check</param>
        /// <returns>True if permitted, false if denied</returns>
        private bool CheckViewPermission(string formKey, params string[] alternateFormKeys)
        {
            var formKeys = new[] { formKey }.Concat(alternateFormKeys ?? Enumerable.Empty<string>());
            if (!formKeys.Any(key => SessionContext.CanView(key)))
            {
                MessageBox.Show($"You do not have permission to access this module.\n\nRequired permission: View access to '{formKey}'",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the Form embedded in the currently active/selected tab.
        /// </summary>
        private Form GetActiveTabForm()
        {
            try
            {
                var activeTab = tabControlMain.ActiveTab ?? tabControlMain.SelectedTab;
                if (activeTab != null &&
                    activeTab.TabPage.Controls.Count > 0 &&
                    activeTab.TabPage.Controls[0] is Form form &&
                    !form.IsDisposed)
                {
                    return form;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting active tab form: {ex.Message}");
            }
            return null;
        }

        private void ultraToolbarsManager1_ToolClick_1(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            string toolKey = e.Tool.Key?.Trim();

            // Debug the clicked tool key
            System.Diagnostics.Debug.WriteLine($"Tool Clicked: {e.Tool.Key}");
            _lastActivatedToolKey = toolKey; // Store for favorites tracking

            // Handle LogOff immediately before permission checks
            if (toolKey == "LogIn" || toolKey == "LogOff")
            {
                // Confirm logoff
                var result = MessageBox.Show("Are you sure you want to log off?", "Confirm Log Off",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    LogLogout("User logged off via menu");
                    // Close all tabs to free resources
                    CloseAllTabs();

                    SessionContext.Clear();

                    // Find and reuse the original Login form without hardcoding
                    var existingLoginForm = Application.OpenForms.OfType<Login>().FirstOrDefault();
                    if (existingLoginForm != null)
                    {
                        existingLoginForm.ResetLoginFields();
                        
                        this.IsLoggingOff = true;
                        this.Close();
                        
                        existingLoginForm.Show();
                    }
                    else
                    {
                        // Fallback logic
                        Login loginForm = new Login();
                        loginForm.Show();
                        loginForm.FormClosed += (s, args) => Application.Exit();
                        
                        this.IsLoggingOff = true;
                        this.Close();
                    }
                    return;
                }
                return;
            }

            if (toolKey == "Nexoris AI")
            {
                using (var frmSettings = new FrmNexorisAISettings())
                {
                    if (frmSettings.ShowDialog(this) == DialogResult.OK)
                    {
                        if (_nexorisAI != null && !_nexorisAI.IsDisposed)
                        {
                            var method = _nexorisAI.GetType().GetMethod("ApplyTheme", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (method != null) method.Invoke(_nexorisAI, null);
                        }
                    }
                }
                return;
            }

            if (toolKey == "Overview")
            {
                Dashboard.FrmDashboardOverview overview = new Dashboard.FrmDashboardOverview(OpenFormInTabSafe);
                OpenFormInTabSafe(overview, "Overview");
                return;
            }

            if (toolKey == "BusinessSummary")
            {
                Settings.FinalAnalysis finalAnalysis = new Settings.FinalAnalysis();
                OpenFormInTabSafe(finalAnalysis, "Business Summary");
                return;
            }

            if (toolKey == "ExcelImport")
            {
                Utilities.FrmExcelImport importForm = new Utilities.FrmExcelImport();
                OpenFormInTabSafe(importForm, "Excel Import/Export");
                return;
            }

            // Map aliases to permission keys
            string permissionKey = toolKey;
            if (toolKey == "Roles") permissionKey = "RolePermissions";

            // Check permission before opening any form (skip for global action tools)
            bool isGlobalAction = toolKey == "Save" ||
                                 toolKey == "Delet" ||
                                 toolKey == "Clear" ||
                                 toolKey == "Remove" ||
                                 toolKey == "Exit" ||
                                 toolKey == "Report" ||
                                 toolKey == "LogOff" ||
                                 toolKey == "LogIn" ||
                                 toolKey == "ReOrder" ||
                                 toolKey == "Overview" ||
                                 toolKey == "Hold" ||
                                 toolKey == "LastBill";

            if (!isGlobalAction)
            {
                if (!CheckViewPermission(permissionKey, e.Tool.Key))
                    return;
            }

            // Universal Save button — delegates to the active tab's form save method
            // Covers all forms: FrmPurchase (SavePurchase), most Master/Accounts/Settings forms (btnSave_Click),
            // frmSalesReturn/frmPurchaseReturn (pbxSave_Click), and any form with a public Save()/SaveData() method.
            if (toolKey == "Save")
            {
                try
                {
                    Form activeForm = GetActiveTabForm();
                    if (activeForm == null || activeForm.IsDisposed)
                    {
                        MessageBox.Show("No active form to save.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    if (!EnsureTransactionAllowed(activeForm))
                        return;

                    bool saved = false;

                    // 1. FrmPurchase — has dedicated public SavePurchase() / UpdatePurchase() methods
                    //    pbxSave visible = save mode, ultraPictureBox4 visible = update mode
                    if (activeForm is Transaction.FrmPurchase purchaseForm)
                    {
                        var pbxSaveControls = activeForm.Controls.Find("pbxSave", true);
                        var updateControls = activeForm.Controls.Find("ultraPictureBox4", true);
                        if (updateControls.Length > 0 && updateControls[0].Visible)
                        {
                            purchaseForm.UpdatePurchase();
                        }
                        else
                        {
                            purchaseForm.SavePurchase();
                        }
                        saved = true;
                    }

                    // 2. frmSalesInvoice - use explicit RibbonSave to avoid generic button collisions
                    //    (button3_Click opens the Hold dialog, which shouldn't be called on Save)
                    if (!saved && activeForm is Transaction.frmSalesInvoice salesInvoice)
                    {
                        salesInvoice.RibbonSave();
                        saved = true;
                    }

                    // 2. Generic fallback: try public Save() or SaveData() methods FIRST
                    //    This allows forms to explicitly implement save logic and avoid button name collisions
                    if (!saved)
                    {
                        var saveMethod = activeForm.GetType().GetMethod("Save",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                            null, Type.EmptyTypes, null)
                            ?? activeForm.GetType().GetMethod("SaveData",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                            null, Type.EmptyTypes, null);
                        if (saveMethod != null)
                        {
                            saveMethod.Invoke(activeForm, null);
                            saved = true;
                        }
                    }

                    // 3. Generic handler for all other forms — searches for click handlers by name
                    //    and checks if the associated control is visible to pick save vs update.
                    //    Covers: frmItemMasterNew (button3/btnUpdate), frmSalesInvoice (ultraPictureBox4/updtbtn),
                    //    frmSalesReturn/frmPurchaseReturn (pbxSave), and all Master/Accounts/Settings forms (btnSave)
                    if (!saved)
                    {
                        string[] candidateMethods = { "btnSave_Click", "button3_Click", "ultraPictureBox4_Click",
                            "pbxSave_Click", "btnUpdate_Click", "updtbtn_Click" };
                        foreach (var methodName in candidateMethods)
                        {
                            var method = activeForm.GetType().GetMethod(methodName,
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (method != null)
                            {
                                // Derive control name by stripping "_Click" (e.g. "button3_Click" → "button3")
                                string controlName = methodName.Replace("_Click", "");
                                var controls = activeForm.Controls.Find(controlName, true);
                                // If the control exists, only invoke when it's visible (respects save/update mode)
                                // If no matching control found, invoke anyway (safe fallback)
                                if (controls.Length == 0 || controls[0].Visible)
                                {
                                    method.Invoke(activeForm, new object[] { this, EventArgs.Empty });
                                    saved = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!saved)
                    {
                        MessageBox.Show("Save is not supported for this form.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (System.Reflection.TargetInvocationException tie)
                {
                    // Unwrap the inner exception from reflection
                    var innerEx = tie.InnerException ?? tie;
                    System.Diagnostics.Debug.WriteLine($"Error in universal save: {innerEx.Message}");
                    MessageBox.Show($"Error saving: {innerEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in universal save: {ex.Message}");
                    MessageBox.Show($"Error saving: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            if (toolKey == "Clear" || toolKey == "Delet" || toolKey == "Update" || toolKey == "Hold" || toolKey == "LastBill")
            {
                try
                {
                    Form activeForm = GetActiveTabForm();
                    if (activeForm == null || activeForm.IsDisposed) return;

                    string[] candidateMethods = new string[0];
                    if (toolKey == "Clear")
                        candidateMethods = new[] { "RibbonClear", "Clear", "Reset", "ClearForm", "ResetForm", "btnClear_Click", "BtnClear_Click", "brnClear_Click", "btnReset_Click", "BtnNew_Click", "btnNew_Click", "button7_Click", "ultraPictureBox1_Click", "button2_Click", "button4_Click", "btnClear_Click_1", "ultraBtnClear_Click", "btn_clear_Click" };
                    else if (toolKey == "Delet")
                        candidateMethods = new[] { "RibbonDeleteInvoice", "Delete", "DeleteRecord", "DeleteItem", "DeletePurchase", "DeletePurchaseReturn", "DeleteReturn", "btnDelete_Click", "BtnDelete_Click", "btn_delete_Click", "ultraPictureBox2_Click", "ultraBtnDelete_Click" };
                    else if (toolKey == "Update")
                        candidateMethods = new[] { "btnUpdate_Click", "BtnUpdate_Click", "updtbtn_Click", "ultraPictureBox7_Click", "UpdateRecord", "UpdateData", "btn_update_Click", "ultraBtnUpdate_Click" };
                    else if (toolKey == "Hold")
                        candidateMethods = new[] { "pbxHold_Click" };
                    else if (toolKey == "LastBill")
                        candidateMethods = new[] { "RibbonLastBill" };

                    bool handled = false;

                    foreach (var methodName in candidateMethods)
                    {
                        var method = activeForm.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (method != null)
                        {
                            var parameters = method.GetParameters();
                            if (parameters.Length == 0)
                            {
                                method.Invoke(activeForm, null);
                                handled = true;
                                break;
                            }
                            else if (parameters.Length == 2)
                            {
                                method.Invoke(activeForm, new object[] { this, EventArgs.Empty });
                                handled = true;
                                break;
                            }
                        }
                    }

                    if (!handled)
                    {
                        MessageBox.Show($"{toolKey} is not supported for this form.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (System.Reflection.TargetInvocationException tie)
                {
                    var innerEx = tie.InnerException ?? tie;
                    System.Diagnostics.Debug.WriteLine($"Error in universal clear: {innerEx.Message}");
                    MessageBox.Show($"Error clearing: {innerEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in universal clear: {ex.Message}");
                    MessageBox.Show($"Error clearing: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // Universal Delete button — delegates to the active tab's form delete method
            if (e.Tool.Key == "Delete")
            {
                try
                {
                    Form activeForm = GetActiveTabForm();
                    if (activeForm == null || activeForm.IsDisposed)
                    {
                        MessageBox.Show("No active form to delete from.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    bool deleted = false;
                    string formName = activeForm.GetType().Name;

                    // 1. Try form-specific delete methods first
                    string[] deleteMethods;

                    if (formName == "FrmPurchase")
                    {
                        deleteMethods = new string[] { "DeletePurchase" };
                    }
                    else if (formName == "frmPurchaseReturn")
                    {
                        deleteMethods = new string[] { "DeletePurchaseReturn", "DeleteReturn" };
                    }
                    else
                    {
                        deleteMethods = new string[] { "Delete", "DeleteRecord", "DeleteItem" };
                    }

                    foreach (var methodName in deleteMethods)
                    {
                        var method = activeForm.GetType().GetMethod(methodName,
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                            null, Type.EmptyTypes, null);
                        if (method != null)
                        {
                            method.Invoke(activeForm, null);
                            deleted = true;
                            break;
                        }
                    }

                    // 2. Fallback: try common delete button click handlers
                    if (!deleted)
                    {
                        string[] candidateMethods = { "btnDelete_Click", "BtnDelete_Click", "btn_delete_Click", "ultraPictureBox2_Click", "ultraBtnDelete_Click" };
                        foreach (var methodName in candidateMethods)
                        {
                            var method = activeForm.GetType().GetMethod(methodName,
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                            if (method != null)
                            {
                                method.Invoke(activeForm, new object[] { this, EventArgs.Empty });
                                deleted = true;
                                break;
                            }
                        }
                    }

                    if (!deleted)
                    {
                        MessageBox.Show($"{e.Tool.Key} is not supported for this form.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (System.Reflection.TargetInvocationException tie)
                {
                    var innerEx = tie.InnerException ?? tie;
                    System.Diagnostics.Debug.WriteLine($"Error in universal delete: {innerEx.Message}");
                    MessageBox.Show($"Error deleting: {innerEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in universal delete: {ex.Message}");
                    MessageBox.Show($"Error deleting: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // Universal Exit button — closes the active tab in ultraTabSharedControlsPage1
            if (e.Tool.Key == "Exit" || e.Tool.Key == "Remove")
            {
                try
                {
                    if (tabControlMain.ActiveTab != null && tabControlMain.Tabs.Count > 0)
                    {
                        CloseCurrentTab();
                    }
                    else
                    {
                        MessageBox.Show("No active tab to close.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error executing {toolKey}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            if (!EnsureTransactionAllowed(e.Tool.Key))
                return;

            if (e.Tool.Key == "Pos")
            {
                frmPos frmPos = new frmPos();
                OpenFormInTab(frmPos, "POS");
            }

            if (e.Tool.Key == "Sales")
            {
                // Prevent duplicate instances by using the safe opener
                frmSalesInvoice frmsales = new frmSalesInvoice();
                OpenFormInTabSafe(frmsales, "Sales Invoice");
            }
            if (e.Tool.Key == "Sales Return")
            {
                frmSalesReturn frmsalesreturn = new frmSalesReturn();
                OpenFormInTab(frmsalesreturn, "Sales Return");
            }
            if (e.Tool.Key == "Branch")
            {
                FrmBranch frmbranch = new FrmBranch();
                OpenFormInTab(frmbranch, "Branch");
            }
            if (e.Tool.Key == "Category")
            {
                Master.FrmCategory frmcategory = new Master.FrmCategory();
                OpenFormInTab(frmcategory, "Category");
            }
            if (e.Tool.Key == "Group")
            {
                Master.FrmGroup frmcategory = new Master.FrmGroup();
                OpenFormInTab(frmcategory, "Group");
            }
            if (e.Tool.Key == "ItemMaster")
            {
                // FrmItemMaster ItemMaster = new FrmItemMaster();
                frmItemMasterNew ItemMaster = new frmItemMasterNew();
                OpenFormInTab(ItemMaster, "Item Master");
            }
            if (e.Tool.Key == "Line")
            {
                frmLine line = new frmLine();
                OpenFormInTab(line, "Line");
            }
            if (e.Tool.Key == "Company")
            {
                frmCompany compa = new frmCompany();
                OpenFormInTab(compa, "Company");
            }
            if (e.Tool.Key == "Rack")
            {
                frmRack rack = new frmRack();
                OpenFormInTab(rack, "Rack");
            }
            if (e.Tool.Key == "Row")
            {
                FrmRow Row = new FrmRow();
                OpenFormInTab(Row, "Row");
            }
            if (e.Tool.Key == "State")
            {
                FrmState state = new FrmState();
                OpenFormInTab(state, "State");
            }
            if (e.Tool.Key == "Brand")
            {
                FrmBrand Brand = new FrmBrand();
                OpenFormInTab(Brand, "Brand");
            }
            if (e.Tool.Key == "Customer")
            {
                FrmCustomer Customer = new FrmCustomer();
                OpenFormInTab(Customer, "Customer");
            }
            if (e.Tool.Key == "Vendor")
            {
                Accounts.FrmVendor vendor = new FrmVendor();
                OpenFormInTab(vendor, "Vendor");
            }
            if (e.Tool.Key == "Ledger")
            {
                Accounts.FrmLedgers ledgers = new FrmLedgers();
                OpenFormInTab(ledgers, "Ledger");
            }
            if (e.Tool.Key == "AccountGroup")
            {
                Accounts.FrmAccountGroup AccountGroup = new FrmAccountGroup();
                OpenFormInTab(AccountGroup, "AccountGroup");
            }
            if (e.Tool.Key == "Receipt")
            {
                Accounts.FrmReceipt receipt = new FrmReceipt();
                OpenFormInTab(receipt, "Receipt");
            }
            if (e.Tool.Key == "ManualPartyBalance")
            {
                PosBranch_Win.Accounts.FrmManualPartyBalance frmManualBalance = new PosBranch_Win.Accounts.FrmManualPartyBalance();
                OpenFormInTab(frmManualBalance, "Manual Party Balance");
            }
            if (e.Tool.Key == "TradingPLAccount")
            {
                PosBranch_Win.Reports.FinancialReports.FrmTradingPLAccount tradingAccount = new PosBranch_Win.Reports.FinancialReports.FrmTradingPLAccount();
                OpenFormInTab(tradingAccount, "Trading Account");
            }
            if (e.Tool.Key == "TradingAccount")
            {
                PosBranch_Win.Reports.FinancialReports.FrmTradingPLAccount tradingAccount = new PosBranch_Win.Reports.FinancialReports.FrmTradingPLAccount();
                OpenFormInTab(tradingAccount, "Trading Account");
            }
            if (e.Tool.Key == "ProfitLossAccount")
            {
                PosBranch_Win.Reports.FinancialReports.FrmProfitLossAccount plAccount = new PosBranch_Win.Reports.FinancialReports.FrmProfitLossAccount();
                OpenFormInTab(plAccount, "Profit & Loss Account");
            }
            if (e.Tool.Key == "BalanceSheet")
            {
                PosBranch_Win.Reports.FinancialReports.FrmBalanceSheet bsReport = new PosBranch_Win.Reports.FinancialReports.FrmBalanceSheet();
                OpenFormInTab(bsReport, "Balance Sheet");
            }
            if (e.Tool.Key == "TrialBalance")
            {
                PosBranch_Win.Reports.FinancialReports.FrmTrialBalance tbReport = new PosBranch_Win.Reports.FinancialReports.FrmTrialBalance();
                OpenFormInTab(tbReport, "Trial Balance");
            }
            if (e.Tool.Key == "CashBankBook")
            {
                PosBranch_Win.Reports.FinancialReports.FrmCashBankBook cbBook = new PosBranch_Win.Reports.FinancialReports.FrmCashBankBook();
                OpenFormInTab(cbBook, "Cash & Bank Book");
            }
            if (e.Tool.Key == "DayBook")
            {
                PosBranch_Win.Reports.FinancialReports.FrmDayBook dbBook = new PosBranch_Win.Reports.FinancialReports.FrmDayBook();
                OpenFormInTab(dbBook, "Day Book");
            }
            if (e.Tool.Key == "Payment")
            {
                Accounts.FrmPayment payment = new FrmPayment();
                OpenFormInTab(payment, "Payment");
            }
            if (e.Tool.Key == "GeneralPayment")
            {
                Accounts.FrmGeneralPayment payment = new FrmGeneralPayment();
                OpenFormInTab(payment, "General Payment");
            }
            if (e.Tool.Key == "GeneralReceipt")
            {
                Accounts.FrmGeneralReceipt receipt = new FrmGeneralReceipt();
                OpenFormInTab(receipt, "General Receipt");
            }
            if (e.Tool.Key == "BankReconciliation")
            {
                Accounts.FrmBankReconciliation bankRec = new Accounts.FrmBankReconciliation();
                OpenFormInTab(bankRec, "Bank Reconciliation");
            }
            if (e.Tool.Key == "PaymodeMaster" || e.Tool.Key == "Paymode")
            {
                Master.FrmPaymodeMaster paymodeMaster = new Master.FrmPaymodeMaster();
                OpenFormInTab(paymodeMaster, "Paymode Account Setup");
            }
            if (e.Tool.Key == "BankStatementReport")
            {
                PosBranch_Win.Reports.FinancialReports.FrmBankStatementReport bankStatement = new PosBranch_Win.Reports.FinancialReports.FrmBankStatementReport();
                OpenFormInTab(bankStatement, "Bank Statement");
            }
            if (e.Tool.Key == "ShiftReconciliationReport")
            {
                PosBranch_Win.Reports.FinancialReports.FrmShiftReconciliationReport shiftReport = new PosBranch_Win.Reports.FinancialReports.FrmShiftReconciliationReport();
                OpenFormInTab(shiftReport, "Shift Report");
            }
            if (e.Tool.Key == "Contra")
            {
                Accounts.FrmContra contra = new FrmContra();
                OpenFormInTab(contra, "Contra");
            }
            if (e.Tool.Key == "Journal")
            {
                Accounts.FrmJournal journal = new FrmJournal();
                OpenFormInTab(journal, "Journal");
            }
            if (e.Tool.Key == "ChartOfAccount")
            {
                //ChartOfAccount.FrmChartOfAccount chartOfAcc = new ChartOfAccount.FrmChartOfAccount();
                ChartOfAccount.FrmChartOfAcc chartOfAcc = new ChartOfAccount.FrmChartOfAcc();
                OpenFormInTab(chartOfAcc, "ChartOfAccount");
            }
            if (e.Tool.Key == "DebitNote")
            {
                Accounts.FrmDebitNote debitNote = new FrmDebitNote();
                OpenFormInTab(debitNote, "DebitNote");
            }
            if (e.Tool.Key == "CreditNote")
            {
                Accounts.FrmCreditNote creditNote = new FrmCreditNote();
                OpenFormInTab(creditNote, "CreditNote");
            }
            if (e.Tool.Key == "ManualPartyBalanceReport")
            {
                PosBranch_Win.Reports.FinancialReports.FrmManualPartyBalanceReport manualPartyBalanceReport = new PosBranch_Win.Reports.FinancialReports.FrmManualPartyBalanceReport();
                OpenFormInTab(manualPartyBalanceReport, "Manual Party Balance Report");
            }
            if (e.Tool.Key == "CombinedPartyBalanceReport")
            {
                PosBranch_Win.Reports.FinancialReports.FrmCombinedPartyBalanceReport combinedPartyBalanceReport = new PosBranch_Win.Reports.FinancialReports.FrmCombinedPartyBalanceReport();
                OpenFormInTab(combinedPartyBalanceReport, "Combined Party Balance Report");
            }
            if (e.Tool.Key == "Users")
            {
                FrmUsers users = new FrmUsers();
                OpenFormInTab(users, "Users");
            }
            if (e.Tool.Key == "Purchase")
            {
                FrmPurchase purchase = new FrmPurchase();
                OpenFormInTab(purchase, "Purchase");
            }
            if (e.Tool.Key == "Purchase Order")
            {
                frmPurchaseOrder purchaseOrder = new frmPurchaseOrder();
                OpenFormInTab(purchaseOrder, "Purchase Order");
            }
            if (e.Tool.Key == "Purchase R/n")
            {
                frmPurchaseReturn preturn = new frmPurchaseReturn();
                OpenFormInTab(preturn, "Purchase Return");
            }
            if (e.Tool.Key == "Country")
            {
                FrmCountry country = new FrmCountry();
                OpenFormInTab(country, "Country");
            }
            if (e.Tool.Key == "stockadjustment")
            {
                FrmStockAdjustment stokadj = new FrmStockAdjustment();
                OpenFormInTab(stokadj, "Stock Adjustment");
            }
            if (e.Tool.Key == "stocktransfer")
            {
                FrmStockTransfer stoktransfer = new FrmStockTransfer();
                OpenFormInTab(stoktransfer, "Stock Transfer");
            }
            if (e.Tool.Key == "frmvendor")
            {
                FrmVendor vendor = new FrmVendor();
                OpenFormInTab(vendor, "Vendor");
            }
            if (e.Tool.Key == "Goods Received")
            {
                FrmPurchase purchase = new FrmPurchase();
                OpenFormInTab(purchase, "Goods Received");
            }

            // ADD THIS NEW CONDITION FOR PRINT BARCODE
            if (e.Tool.Key == "Print Barcode")
            {
                Utilities.frmBarcode barcodeForm = new Utilities.frmBarcode();
                OpenFormInTab(barcodeForm, "Print Barcode");
            }

            if (e.Tool.Key == "PLU Weighing")
            {
                Utilities.FrmPlu frmPlu = new Utilities.FrmPlu();
                OpenFormInTab(frmPlu, "PLU Weighing");
            }
            if (e.Tool.Key == "OpeningStock")
            {
                Utilities.frmOpeningStock frmOpnstk = new Utilities.frmOpeningStock();
                OpenFormInTab(frmOpnstk, "OpeningStock");
            }
            if (e.Tool.Key == "BtnClosing")
            {
                Utilities.frmClosing frmClosing = new Utilities.frmClosing();
                OpenFormInTab(frmClosing, "Closing");
            }

            if (e.Tool.Key == "Change Item No")
            {
                Utilities.frmChangeItemNo frmChangeItemNo = new Utilities.frmChangeItemNo();
                frmChangeItemNo.ShowDialog(this);
               
            }

            if (e.Tool.Key == "Database")
            {
                Utilities.frmDataBase frmDb = new Utilities.frmDataBase();
                OpenFormInTab(frmDb, "Database Backup");
            }


            if (e.Tool.Key == "Sales Details")
            {
                Reports.SalesReports.frmSalesReportMasterDetail frmSalesDetails = new Reports.SalesReports.frmSalesReportMasterDetail();
                OpenFormInTab(frmSalesDetails, "Sales Details");
            }
            if (e.Tool.Key == "SalesReturn")
            {
                Reports.SalesReports.SalesReturnReport frmSalesRetun = new Reports.SalesReports.SalesReturnReport();
                OpenFormInTab(frmSalesRetun, "SalesReturn");
            }
            if (e.Tool.Key == "SalesProfit")
            {
                Reports.SalesReports.frmSalesProfit frmSalesProfit = new Reports.SalesReports.frmSalesProfit();
                OpenFormInTab(frmSalesProfit, "Sales Profit");
            }
            if (e.Tool.Key == "SalesmanIncentiveReport")
            {
                Reports.SalesReports.frmSalesmanIncentiveReport frmSalesmanIncentiveReport = new Reports.SalesReports.frmSalesmanIncentiveReport();
                OpenFormInTab(frmSalesmanIncentiveReport, "Salesman Incentive Report");
            }
            if (e.Tool.Key == "Purchase Details")
            {

                Reports.PurchaseReports.frmPurchaseReportDetails frmPurchaseReport = new Reports.PurchaseReports.frmPurchaseReportDetails();
                OpenFormInTab(frmPurchaseReport, "Purchase Details");
            }
            if (e.Tool.Key == "PurchaseReturn")
            {

                Reports.PurchaseReports.PurchaseReturnReport frmPurchaseReturnReport = new Reports.PurchaseReports.PurchaseReturnReport();
                OpenFormInTab(frmPurchaseReturnReport, "PurchaseReturn");
            }
            if (e.Tool.Key == "VendorPurchaseReport")
            {
                Reports.PurchaseReports.frmvendorpurchasereport vendorPurchaseReport = new Reports.PurchaseReports.frmvendorpurchasereport();
                OpenFormInTab(vendorPurchaseReport, "Vendor Purchase Report");
            }
            if (e.Tool.Key == "AuditTrail")
            {
                Reports.AuditReport.frmAuditReport frmAuditTrail = new Reports.AuditReport.frmAuditReport();
                OpenFormInTab(frmAuditTrail, "Inventory Audit Trail");
            }
            if (e.Tool.Key == "ItemReport")
            {
                Reports.InventoryReport.frmItemReport frmitemRpt = new Reports.InventoryReport.frmItemReport();
                OpenFormInTab(frmitemRpt, "ItemReport");

            }
            if (e.Tool.Key == "StockListingReport")
            {
                Reports.InventoryReport.frmStockReport frmStockRpt = new Reports.InventoryReport.frmStockReport();
                OpenFormInTab(frmStockRpt, "Stock Report");
            }
            if (e.Tool.Key == "StockReport")
            {
                Reports.InventoryReport.frmStockReportAdvanced frmStkRptAdv = new Reports.InventoryReport.frmStockReportAdvanced();
                OpenFormInTab(frmStkRptAdv, "StockReport");
            }
            if (e.Tool.Key == "StockValuationReport")
            {
                Reports.InventoryReport.frmStockValuationReport frmStkVal = new Reports.InventoryReport.frmStockValuationReport();
                OpenFormInTab(frmStkVal, "Stock Valuation Report");
            }
            if (e.Tool.Key == "LowStockAlertReport")
            {
                Reports.InventoryReport.frmLowStockAlertReport frmLowStock = new Reports.InventoryReport.frmLowStockAlertReport();
                OpenFormInTab(frmLowStock, "Low Stock Alert Report");
            }
            if (e.Tool.Key == "StockAdjustmentReport")
            {
                Reports.InventoryReport.frmStockAdjustmentReport frmStockAdjustment = new Reports.InventoryReport.frmStockAdjustmentReport();
                OpenFormInTab(frmStockAdjustment, "Stock Adjustment Report");
            }
            if (e.Tool.Key == "CustomerwiseSalesSummaryReport")
            {
                Reports.SalesReports.frmCustomerwiseSalesSummaryReport frmCustSales = new Reports.SalesReports.frmCustomerwiseSalesSummaryReport();
                OpenFormInTab(frmCustSales, "Customer-wise Sales Summary");
            }
            if (e.Tool.Key == "SalesmanwiseSalesSummaryReport")
            {
                Reports.SalesReports.frmSalesmanwiseSalesSummaryReport frmSalesmanSales = new Reports.SalesReports.frmSalesmanwiseSalesSummaryReport();
                OpenFormInTab(frmSalesmanSales, "Salesman-wise Sales Summary");
            }
            if (e.Tool.Key == "ItemwiseSalesSummaryReport")
            {
                Reports.SalesReports.frmItemwiseSalesSummaryReport frmItemSales = new Reports.SalesReports.frmItemwiseSalesSummaryReport();
                OpenFormInTab(frmItemSales, "Item-wise Sales Summary");
            }
            if (e.Tool.Key == "ReOrder")
            {
                Reports.InventoryReport.FrmSmartReorderDashboard reorderDashboard = new Reports.InventoryReport.FrmSmartReorderDashboard();
                OpenFormInTab(reorderDashboard, "Smart Reorder Dashboard");
            }

            // ADD THIS NEW CONDITION FOR UNIT MASTER
            if (e.Tool.Key == "UnitMaster")
            {
                Master.FrmUnitMaster unitMasterForm = new Master.FrmUnitMaster();
                OpenFormInTab(unitMasterForm, "Unit Master");
            }

            // ADD THIS NEW CONDITION FOR TAX MANAGEMENT
            if (e.Tool.Key == "TaxManagement")
            {
                Master.frmTaxManagement taxManagementForm = new Master.frmTaxManagement();
                if (taxManagementForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh tax status display after settings change
                    UpdateTaxStatusDisplay();
                }
            }
            if (e.Tool.Key == "CustomerOutstandingReport")
            {
                PosBranch_Win.Reports.FinancialReports.frmCustomerOutstandingReport customerOutstandingReport = new PosBranch_Win.Reports.FinancialReports.frmCustomerOutstandingReport();
                OpenFormInTab(customerOutstandingReport, "Customer Outstanding Report");
            }
            if (e.Tool.Key == "CustomerReceiptReport")
            {
                PosBranch_Win.Reports.FinancialReports.frmCustomerReceiptReport customerReceiptReport = new PosBranch_Win.Reports.FinancialReports.frmCustomerReceiptReport();
                OpenFormInTab(customerReceiptReport, "Customer Receipt Report");
            }
            if (e.Tool.Key == "CustomerLedgerReport")
            {
                PosBranch_Win.Reports.FinancialReports.frmCustomerLedgerReport customerLedgerReport = new PosBranch_Win.Reports.FinancialReports.frmCustomerLedgerReport();
                OpenFormInTab(customerLedgerReport, "Customer Ledger Statement");
            }
            if (e.Tool.Key == "VendorOutstandingReport")
            {
                PosBranch_Win.Reports.FinancialReports.frmVendorOutstandingReport vendorOutstandingReport = new PosBranch_Win.Reports.FinancialReports.frmVendorOutstandingReport();
                OpenFormInTab(vendorOutstandingReport, "Vendor Outstanding Report");
            }
            if (e.Tool.Key == "VendorDNPaymentReport")
            {
                PosBranch_Win.Reports.FinancialReports.frmVendorPaymentReport vendorPaymentReport = new PosBranch_Win.Reports.FinancialReports.frmVendorPaymentReport();
                OpenFormInTab(vendorPaymentReport, "Vendor DN/Payment Report");
            }
            if (e.Tool.Key == "UnallocatedPurchaseReturns")
            {
                PosBranch_Win.Reports.FinancialReports.frmVendorOutstandingReport unallocatedReturnsReport = new PosBranch_Win.Reports.FinancialReports.frmVendorOutstandingReport(true);
                OpenFormInTab(unallocatedReturnsReport, "Unallocated Purchase Returns");
            }

            #region here for reports sections menu
            if (e.Tool.Key == "DSales")
            {
                frmSales_RPT salesRptv = new frmSales_RPT();
                OpenFormInTab(salesRptv, "Daily Sales Report");
            }
            if (e.Tool.Key == "CounterReport")
            {
                Reports.SalesReports.frmCounterReport frmCounterReport = new Reports.SalesReports.frmCounterReport();
                OpenFormInTab(frmCounterReport, "Counter Report");
            }

            // Activity Log
            if (toolKey == "ActivityLog")
            {
                ActivityLogSelector activityLog = new ActivityLogSelector();
                OpenFormInTab(activityLog, "Activity Log");
            }

            // POS Settings
            if (toolKey == "POSSettings" || toolKey == "Settings")
            {
                frmPOSSettings posSettings = new frmPOSSettings();
                OpenFormInTab(posSettings, "Sale Settings");
            }

            // Financial Year Closing
            if (toolKey == "FinancialYearClosing")
            {
                FrmFinancialYearClosing yearClosing = new FrmFinancialYearClosing();
                OpenFormInTab(yearClosing, "Financial Year Closing");
            }

            // Role Permissions (Admin only)
            if (toolKey == "RolePermissions" || toolKey == "Roles")
            {
                FrmRolePermissions rolePerms = new FrmRolePermissions();
                OpenFormInTab(rolePerms, "Role Permissions");
            }

            // Report Navigator Toggle
            if (toolKey == "Report")
            {
                if (_isReportNavigatorVisible)
                    HideReportNavigator();
                else
                    ShowReportNavigator();
            }

            #endregion
        }

        private void Home_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        /// <summary>
        /// Updates the tax status display in the status bar or form
        /// </summary>
        private void UpdateTaxStatusDisplay()
        {
            try
            {
                // You can add a status label or status bar item to show tax status
                // For now, we'll use debug output and could add a status bar item
                System.Diagnostics.Debug.WriteLine($"Tax Status: {DataBase.TaxToggleMessage}");

                // If you have a status bar, you could update it here:
                // statusStrip1.Items["taxStatus"].Text = DataBase.TaxToggleMessage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating tax status display: {ex.Message}");
            }
        }

        private FrmNexorisAI _nexorisAI;
        private Panel _pnlAIBase;

        private void ToggleNexorisAI()
        {
            // If forms were disposed or not initialized, build them
            if (_pnlAIBase == null || _pnlAIBase.IsDisposed || _nexorisAI == null || _nexorisAI.IsDisposed)
            {
                if (_pnlAIBase != null) _pnlAIBase.Dispose();
                if (_nexorisAI != null) _nexorisAI.Dispose();

                _pnlAIBase = new Panel();
                _pnlAIBase.Dock = DockStyle.Right;
                _pnlAIBase.Width = 400;
                _pnlAIBase.BackColor = Color.FromArgb(18, 18, 18);

                this.Controls.Add(_pnlAIBase);
                _pnlAIBase.BringToFront();

                _nexorisAI = new FrmNexorisAI();
                _nexorisAI.TopLevel = false;
                _nexorisAI.Dock = DockStyle.Fill;

                _nexorisAI.VisibleChanged += delegate
                {
                    if (_pnlAIBase != null && !_pnlAIBase.IsDisposed)
                    {
                        _pnlAIBase.Visible = _nexorisAI.Visible;
                        if (_pnlAIBase.Visible)
                        {
                            _pnlAIBase.BringToFront();
                        }
                    }
                };

                _pnlAIBase.Controls.Add(_nexorisAI);
                _pnlAIBase.Visible = true;
                _nexorisAI.Show();
                _pnlAIBase.BringToFront();
                _nexorisAI.Focus();
            }
            else
            {
                // Explicitly toggle state based on AI form validity
                if (_nexorisAI.Visible)
                {
                    // If focused in txtbox, FrmNexorisAI usually intercepts and fires this natively.
                    // But if Home clicked, we manually hide here.
                    _nexorisAI.Hide();
                    _pnlAIBase.Visible = false;
                }
                else
                {
                    _pnlAIBase.Visible = true;
                    _pnlAIBase.BringToFront();
                    _nexorisAI.Show();
                    _nexorisAI.Focus();
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys keyCode = keyData & Keys.KeyCode;
            bool ctrl = (keyData & Keys.Control) == Keys.Control;
            bool alt = (keyData & Keys.Alt) == Keys.Alt;

            if (ctrl && keyCode == Keys.I)
            {
                ToggleNexorisAI();
                return true;
            }

            // Shortcuts to open forms: I (Item Master), P (Purchase), S (Sales Invoice), A (Stock Adjustment), R (Sales Return), E (Purchase Return)
            if (!IsTextInputControlFocused() || alt || (ctrl && keyCode != Keys.I))
            {
                switch (keyCode)
                {
                    case Keys.I:
                        if (!ctrl)
                        {
                            OpenFormInTab(new Master.frmItemMasterNew(), "Item Master");
                            return true;
                        }
                        break;
                    case Keys.P:
                        OpenFormInTab(new Transaction.FrmPurchase(), "Purchase");
                        return true;
                    case Keys.S:
                        OpenFormInTab(new Transaction.frmSalesInvoice(), "Sales Invoice");
                        return true;
                    case Keys.A:
                        OpenFormInTab(new Transaction.FrmStockAdjustment(), "Stock Adjustment");
                        return true;
                    case Keys.R:
                        OpenFormInTab(new Transaction.frmSalesReturn(), "Sales Return");
                        return true;
                    case Keys.E:
                        OpenFormInTab(new Transaction.frmPurchaseReturn(), "Purchase Return");
                        return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool IsTextInputControlFocused()
        {
            try
            {
                Control focused = GetFocusedControl(this);
                if (focused == null) return false;

                if (focused is TextBoxBase ||
                    focused is DateTimePicker ||
                    focused is ComboBox ||
                    focused.GetType().Name.Contains("TextBox") ||
                    focused.GetType().Name.Contains("UltraTextEditor") ||
                    focused.GetType().Name.Contains("UltraComboEditor") ||
                    focused.GetType().Name.Contains("UltraDateTimeEditor") ||
                    focused.GetType().Name.Contains("UltraNumericEditor"))
                {
                    return true;
                }

                if (focused is Infragistics.Win.UltraWinGrid.UltraGrid grid && grid.ActiveCell != null && grid.ActiveCell.IsInEditMode)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private Control GetFocusedControl(Control parent)
        {
            if (parent == null) return null;
            if (parent is ContainerControl container && container.ActiveControl != null)
            {
                return GetFocusedControl(container.ActiveControl);
            }
            return parent;
        }

        private void Home_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                //frmSalesInvoice frmsales = new frmSalesInvoice();
                //OpenFormInTab(frmsales, "Sales Invoice");
            }
            if (e.KeyCode == Keys.F5)
            {
                //Accounts.FrmCustomer ObjCustomer = new FrmCustomer();
                //OpenFormInTab(ObjCustomer, "Customer");
            }
            if (e.KeyCode == Keys.F6)
            {
                //Accounts.FrmVendor Objvendor = new FrmVendor();
                //OpenFormInTab(Objvendor, "Vendor");
            }

            if (e.KeyCode == Keys.ControlKey)
            {
                isCtrlPressed = true;
            }
            if (e.KeyCode == Keys.F2)
            {
                isF2Pressed = true;
            }
            if (isCtrlPressed && isF2Pressed)
            {

            }

            // Global Shortcuts
            if (e.Control && e.KeyCode == Keys.F2)
            {
                // Open Sales Invoice tab
                Transaction.frmSalesInvoice frmsales = new Transaction.frmSalesInvoice();
                OpenFormInTab(frmsales, "Sales Invoice");
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.I)
            {
                // Open Item Master tab
                Master.frmItemMasterNew itemMaster = new Master.frmItemMasterNew();
                OpenFormInTab(itemMaster, "Item Master");
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F2)
            {
                if (_isReportNavigatorVisible)
                    HideReportNavigator();
                else
                    ShowReportNavigator();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Resets modifier key flags on key release
        /// </summary>
        private void Home_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey) isCtrlPressed = false;
            if (e.KeyCode == Keys.F2) isF2Pressed = false;
            if (e.KeyCode == Keys.ShiftKey) isShiftPressed = false;
        }



        /// <summary>
        /// Current application theme color
        /// </summary>
        private Color currentThemeColor = Color.FromArgb(240, 248, 255);

        /// <summary>
        /// Applies the selected color to the entire application like IRS POS
        /// </summary>

        private bool IsHeaderControl(Control control)
        {
            if (control == null) return false;
            if (!string.IsNullOrEmpty(control.Name) && control.Name.IndexOf("header", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (control.Parent != null) return IsHeaderControl(control.Parent);
            return false;
        }

        private void ApplyThemeToControls(Control.ControlCollection controls, Color themeColor)
        {
            try
            {
                foreach (Control control in controls)
                {
                    if (IsHeaderControl(control))
                    {
                        if (control.HasChildren)
                        {
                            ApplyThemeToControls(control.Controls, themeColor);
                        }
                        continue;
                    }

                    bool isIrsBlue = (themeColor.R == 0 && themeColor.G == 174 && themeColor.B == 219);

                    // Apply theme based on control type (clean IRS POS palette: 90% light/white, 10% blue accents)
                    if (control is Panel panel && !panel.Name.Contains("color") && !panel.Name.Contains("language"))
                    {
                        panel.BackColor = isIrsBlue ? Color.FromArgb(247, 251, 255) : themeColor;
                    }
                    else if (control is Button button && !button.Name.StartsWith("color") && !button.Name.StartsWith("colorButton"))
                    {
                        if (button.Name.Equals("btnBackup", StringComparison.OrdinalIgnoreCase) || 
                            button.Name.Equals("btnRestore", StringComparison.OrdinalIgnoreCase))
                        {
                            if (control.HasChildren)
                            {
                                ApplyThemeToControls(control.Controls, themeColor);
                            }
                            continue;
                        }

                        if (isIrsBlue)
                        {
                            button.FlatStyle = FlatStyle.Flat;
                            button.FlatAppearance.BorderSize = 1;
                            button.FlatAppearance.BorderColor = Color.FromArgb(0, 140, 186);
                            button.BackColor = Color.FromArgb(0, 174, 219);
                            button.ForeColor = Color.White;
                            button.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                        }
                        else
                        {
                            button.BackColor = Color.FromArgb(
                                Math.Min(255, themeColor.R + 20),
                                Math.Min(255, themeColor.G + 20),
                                Math.Min(255, themeColor.B + 20)
                            );
                        }
                    }
                    else if (control is TextBox || control is ComboBox)
                    {
                        control.BackColor = Color.White;
                        control.ForeColor = Color.FromArgb(45, 55, 72);
                        control.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
                    }
                    else if (control is DataGridView grid)
                    {
                        grid.BackgroundColor = isIrsBlue ? Color.FromArgb(247, 251, 255) : themeColor;
                        grid.DefaultCellStyle.BackColor = Color.White;
                        grid.DefaultCellStyle.ForeColor = Color.FromArgb(45, 55, 72);
                    }
                    else if (control is Label || control is GroupBox)
                    {
                        control.BackColor = Color.Transparent;
                        if (isIrsBlue)
                        {
                            if (control.Name.Equals("lblledger", StringComparison.OrdinalIgnoreCase))
                            {
                                control.ForeColor = Color.FromArgb(0, 102, 204);
                                control.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                            }
                            else
                            {
                                control.ForeColor = Color.FromArgb(45, 55, 72);
                                if (!control.Font.Bold)
                                {
                                    control.Font = new Font("Segoe UI", control.Font.Size > 12 ? control.Font.Size : 9.5f, control.Font.Style);
                                }
                            }
                        }
                        else
                        {
                            control.BackColor = Color.FromArgb(
                                Math.Min(255, themeColor.R + 5),
                                Math.Min(255, themeColor.G + 5),
                                Math.Min(255, themeColor.B + 5)
                            );
                        }
                    }

                    // Recursively apply to child controls
                    if (control.HasChildren)
                    {
                        ApplyThemeToControls(control.Controls, themeColor);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to controls: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies theme to UltraPanel controls and their child controls
        /// </summary>
        /// <param name="ultraPanel">UltraPanel to theme</param>
        /// <param name="themeColor">Theme color to apply</param>
        private void ApplyThemeToUltraPanelControls(Infragistics.Win.Misc.UltraPanel ultraPanel, Color themeColor)
        {
            try
            {
                if (ultraPanel != null)
                {
                    // Apply theme to the UltraPanel itself
                    ultraPanel.Appearance.BackColor = themeColor;

                    // Apply theme to all controls in the UltraPanel's ClientArea
                    ApplyThemeToControls(ultraPanel.ClientArea.Controls, themeColor);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to UltraPanel: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies theme to all open MDI child forms (Sales, Purchase, etc.)
        /// </summary>
        /// <param name="themeColor">Theme color to apply</param>
        private void ApplyThemeToMDIChildren(Color themeColor)
        {
            try
            {
                foreach (Form childForm in this.MdiChildren)
                {
                    childForm.BackColor = Color.FromArgb(
                        Math.Min(255, themeColor.R + 5),
                        Math.Min(255, themeColor.G + 5),
                        Math.Min(255, themeColor.B + 5)
                    );

                    // Apply to all controls in the child form
                    ApplyThemeToControls(childForm.Controls, themeColor);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to MDI children: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies theme to all open tabbed forms (Sales, Purchase, etc.)
        /// </summary>
        /// <param name="themeColor">Theme color to apply</param>
        private void ApplyThemeToTabbedForms(Color themeColor)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Starting to theme {tabControlMain.Tabs.Count} tabbed forms...");

                // Apply to all forms in tabs
                foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
                {
                    System.Diagnostics.Debug.WriteLine($"Processing tab: {tab.Text}");

                    if (tab.TabPage.Controls.Count > 0 && tab.TabPage.Controls[0] is Form)
                    {
                        Form tabbedForm = (Form)tab.TabPage.Controls[0];
                        System.Diagnostics.Debug.WriteLine($"Found form: {tabbedForm.GetType().Name}");

                        // Use the comprehensive form theming method
                        ApplyThemeToForm(tabbedForm, themeColor);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Tab {tab.Text} has no form or {tab.TabPage.Controls.Count} controls");
                    }

                    // Also apply theme to the tab page itself
                    tab.TabPage.BackColor = Color.FromArgb(
                        Math.Min(255, themeColor.R + 10),
                        Math.Min(255, themeColor.G + 10),
                        Math.Min(255, themeColor.B + 10)
                    );
                }

                // Apply theme to the tab control itself
                tabControlMain.BackColor = themeColor;

                System.Diagnostics.Debug.WriteLine($"Applied theme to {tabControlMain.Tabs.Count} tabbed forms");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to tabbed forms: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Applies comprehensive theming to any form, overriding hardcoded Designer colors
        /// </summary>
        /// <param name="form">Form to theme</param>
        /// <param name="themeColor">Theme color to apply</param>
        private void ApplyThemeToForm(Form form, Color themeColor)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== THEMING FORM: {form.GetType().Name} ===");

                bool isIrsBlue = (themeColor.R == 0 && themeColor.G == 174 && themeColor.B == 219);

                // Apply theme color to the form background (soft #F4FAFF background like IRS POS)
                form.BackColor = isIrsBlue ? Color.FromArgb(244, 250, 255) : Color.FromArgb(
                    Math.Min(255, themeColor.R + 5),
                    Math.Min(255, themeColor.G + 5),
                    Math.Min(255, themeColor.B + 5)
                );
                System.Diagnostics.Debug.WriteLine($"Set form BackColor to: {form.BackColor.Name}");

                // Apply comprehensive theming to all controls
                ApplyThemeToControls(form.Controls, themeColor);

                // Special handling for specific form types
                ApplySpecialFormTheming(form, themeColor);

                // Force refresh to show changes immediately
                form.Refresh();

                System.Diagnostics.Debug.WriteLine($"=== COMPLETED THEMING: {form.GetType().Name} ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error theming form {form.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies special theming for specific form types that need extra attention
        /// </summary>
        /// <param name="form">Form to apply special theming to</param>
        /// <param name="themeColor">Theme color to apply</param>
        private void ApplySpecialFormTheming(Form form, Color themeColor)
        {
            try
            {
                // Handle Infragistics UltraPanel controls (common in forms like frmBarcode)
                ApplyThemeToUltraPanels(form.Controls, themeColor);

                // Handle Infragistics UltraGrid controls
                ApplyThemeToUltraGrids(form.Controls, themeColor);

                // Handle Infragistics UltraButton controls
                ApplyThemeToUltraButtons(form.Controls, themeColor);

                // Handle Infragistics UltraLabel controls
                ApplyThemeToUltraLabels(form.Controls, themeColor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in special form theming: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively applies theming to Infragistics UltraPanel controls
        /// </summary>
        private void ApplyThemeToUltraPanels(Control.ControlCollection controls, Color themeColor)
        {
            try
            {
                bool isIrsBlue = (themeColor.R == 0 && themeColor.G == 174 && themeColor.B == 219);
                Color panelBg = isIrsBlue ? Color.FromArgb(247, 251, 255) : themeColor;

                foreach (Control control in controls)
                {
                    // Handle UltraPanel controls
                    if (control.GetType().Name == "UltraPanel")
                    {
                        string pnlName = control.Name ?? "";

                        if (pnlName.Equals("ultraPanel9", StringComparison.OrdinalIgnoreCase))
                        {
                            if (isIrsBlue)
                            {
                                var appearanceProp = control.GetType().GetProperty("Appearance");
                                if (appearanceProp != null)
                                {
                                    var appearance = appearanceProp.GetValue(control);
                                    if (appearance != null)
                                    {
                                        appearance.GetType().GetProperty("BackColor")?.SetValue(appearance, Color.FromArgb(170, 50, 0));
                                        appearance.GetType().GetProperty("BackColor2")?.SetValue(appearance, Color.FromArgb(80, 10, 0));
                                        appearance.GetType().GetProperty("BackGradientStyle")?.SetValue(appearance, Infragistics.Win.GradientStyle.Vertical);
                                    }
                                }
                            }
                        }
                        else if (pnlName.Equals("ultraPanel7", StringComparison.OrdinalIgnoreCase))
                        {
                            // ultraPanel7 is the dark navy info widget (Total/Pymt/Change)
                            // Keep its original dark appearance for contrast - do not override.
                        }
                        else if (pnlName.Equals("gridFooterPanel", StringComparison.OrdinalIgnoreCase))
                        {
                            // gridFooterPanel is the thin splitter bar below the grid.
                            // For IRS Blue: keep it as light sky blue (matches the form background).
                            // For other themes: use the theme color.
                            Color footerColor = isIrsBlue ? Color.FromArgb(200, 228, 248) : themeColor;
                            control.BackColor = footerColor;
                            var appearanceProp = control.GetType().GetProperty("Appearance");
                            if (appearanceProp != null)
                            {
                                var appearance = appearanceProp.GetValue(control);
                                if (appearance != null)
                                {
                                    appearance.GetType().GetProperty("BackColor")?.SetValue(appearance, footerColor);
                                    appearance.GetType().GetProperty("BackColor2")?.SetValue(appearance, footerColor);
                                    appearance.GetType().GetProperty("BackGradientStyle")?.SetValue(appearance, Infragistics.Win.GradientStyle.None);
                                }
                            }
                        }
                        else
                        {
                            // Use reflection to set appearance properties
                            var appearanceProp = control.GetType().GetProperty("Appearance");
                            if (appearanceProp != null)
                            {
                                var appearance = appearanceProp.GetValue(control);
                                if (appearance != null)
                                {
                                    appearance.GetType().GetProperty("BackColor")?.SetValue(appearance, panelBg);
                                    // Clear any gradient so solid color shows
                                    if (isIrsBlue)
                                    {
                                        appearance.GetType().GetProperty("BackColor2")?.SetValue(appearance, panelBg);
                                        appearance.GetType().GetProperty("BackGradientStyle")?.SetValue(appearance, Infragistics.Win.GradientStyle.None);
                                    }
                                    System.Diagnostics.Debug.WriteLine($"Set UltraPanel appearance BackColor: {control.Name}");
                                }
                            }

                            // Also set regular BackColor as fallback
                            control.BackColor = panelBg;
                        }
                    }

                    // Recursively apply to child controls
                    if (control.HasChildren)
                    {
                        ApplyThemeToUltraPanels(control.Controls, themeColor);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error theming UltraPanels: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively applies theming to Infragistics UltraGrid controls
        /// </summary>
        private void ApplyThemeToUltraGrids(Control.ControlCollection controls, Color themeColor)
        {
            try
            {
                bool isIrsBlue = (themeColor.R == 0 && themeColor.G == 174 && themeColor.B == 219);
                Color headerTop = isIrsBlue ? Color.FromArgb(0, 174, 219) : themeColor;
                Color headerBottom = isIrsBlue ? Color.FromArgb(0, 140, 186) : Color.FromArgb(Math.Max(0, themeColor.R - 20), Math.Max(0, themeColor.G - 20), Math.Max(0, themeColor.B - 20));

                foreach (Control control in controls)
                {
                    // Handle UltraGrid controls
                    if (control.GetType().Name == "UltraGrid")
                    {
                        // Theme the grid background
                        control.BackColor = isIrsBlue ? Color.FromArgb(247, 251, 255) : Color.FromArgb(
                            Math.Min(255, themeColor.R + 20),
                            Math.Min(255, themeColor.G + 20),
                            Math.Min(255, themeColor.B + 20)
                        );

                        // Use reflection to theme grid appearance properties
                        try
                        {
                            var displayLayoutProp = control.GetType().GetProperty("DisplayLayout");
                            if (displayLayoutProp != null)
                            {
                                var displayLayout = displayLayoutProp.GetValue(control);
                                if (displayLayout != null)
                                {
                                    // Theme header appearance
                                    var overrideProp = displayLayout.GetType().GetProperty("Override");
                                    if (overrideProp != null)
                                    {
                                        var overrideObj = overrideProp.GetValue(displayLayout);
                                        if (overrideObj != null)
                                        {
                                            var headerStyleProp = overrideObj.GetType().GetProperty("HeaderStyle");
                                            if (headerStyleProp != null)
                                            {
                                                headerStyleProp.SetValue(overrideObj, Infragistics.Win.HeaderStyle.WindowsXPCommand);
                                            }

                                            var headerAppearanceProp = overrideObj.GetType().GetProperty("HeaderAppearance");
                                            if (headerAppearanceProp != null)
                                            {
                                                var headerAppearance = headerAppearanceProp.GetValue(overrideObj);
                                                if (headerAppearance != null)
                                                {
                                                    headerAppearance.GetType().GetProperty("BackColor")?.SetValue(headerAppearance, headerTop);
                                                    headerAppearance.GetType().GetProperty("BackColor2")?.SetValue(headerAppearance, headerBottom);
                                                    headerAppearance.GetType().GetProperty("BackGradientStyle")?.SetValue(headerAppearance, Infragistics.Win.GradientStyle.Vertical);
                                                    headerAppearance.GetType().GetProperty("ForeColor")?.SetValue(headerAppearance, Color.White);
                                                    headerAppearance.GetType().GetProperty("ThemedElementAlpha")?.SetValue(headerAppearance, Infragistics.Win.Alpha.Transparent);
                                                    System.Diagnostics.Debug.WriteLine($"Set UltraGrid header BackColor: {control.Name}");
                                                }
                                            }

                                            if (isIrsBlue)
                                            {
                                                var rowAltProp = overrideObj.GetType().GetProperty("RowAlternateAppearance");
                                                if (rowAltProp != null)
                                                {
                                                    var rowAlt = rowAltProp.GetValue(overrideObj);
                                                    rowAlt?.GetType().GetProperty("BackColor")?.SetValue(rowAlt, Color.FromArgb(242, 248, 255));
                                                }

                                                var activeRowProp = overrideObj.GetType().GetProperty("ActiveRowAppearance");
                                                if (activeRowProp != null)
                                                {
                                                    var activeRow = activeRowProp.GetValue(overrideObj);
                                                    activeRow?.GetType().GetProperty("BackColor")?.SetValue(activeRow, Color.FromArgb(190, 225, 255));
                                                    activeRow?.GetType().GetProperty("ForeColor")?.SetValue(activeRow, Color.Black);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception gridEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error theming UltraGrid details: {gridEx.Message}");
                        }
                    }

                    // Recursively apply to child controls
                    if (control.HasChildren)
                    {
                        ApplyThemeToUltraGrids(control.Controls, themeColor);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error theming UltraGrids: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively applies theming to Infragistics UltraButton controls (preserve functional colors)
        /// </summary>
        private void ApplyThemeToUltraButtons(Control.ControlCollection controls, Color themeColor)
        {
            try
            {
                foreach (Control control in controls)
                {
                    // Handle UltraButton controls
                    if (control.GetType().Name == "UltraButton")
                    {
                        string buttonName = control.Name?.ToLower() ?? "";
                        bool isIrsBlue = (themeColor.R == 0 && themeColor.G == 174 && themeColor.B == 219);

                        // For IRS Blue: theme ALL buttons with cyan-blue except special delete/cancel/verify
                        // For other themes: only skip destructive-action buttons
                        bool skipButton = buttonName.Contains("delete") || buttonName.Contains("cancel") ||
                                          buttonName.Contains("verify") || buttonName.Contains("closing");

                        if (!skipButton)
                        {
                            var appearanceProp = control.GetType().GetProperty("Appearance");
                            if (appearanceProp != null)
                            {
                                var appearance = appearanceProp.GetValue(control);
                                if (appearance != null)
                                {
                                    if (isIrsBlue)
                                    {
                                        appearance.GetType().GetProperty("BackColor")?.SetValue(appearance, Color.FromArgb(0, 174, 219));
                                        appearance.GetType().GetProperty("BackColor2")?.SetValue(appearance, Color.FromArgb(0, 140, 186));
                                        appearance.GetType().GetProperty("BackGradientStyle")?.SetValue(appearance, Infragistics.Win.GradientStyle.Vertical);
                                        appearance.GetType().GetProperty("ForeColor")?.SetValue(appearance, Color.White);
                                        appearance.GetType().GetProperty("FontData")?.GetType()
                                            .GetProperty("Bold")?.SetValue(appearance.GetType().GetProperty("FontData")?.GetValue(appearance), Infragistics.Win.DefaultableBoolean.True);
                                        System.Diagnostics.Debug.WriteLine($"Set UltraButton IRS Blue: {control.Name}");
                                    }
                                    else
                                    {
                                        if (!buttonName.Contains("print") && !buttonName.Contains("save") && !buttonName.Contains("close"))
                                        {
                                            var backColorProp = appearance.GetType().GetProperty("BackColor");
                                            if (backColorProp != null)
                                            {
                                                backColorProp.SetValue(appearance, Color.FromArgb(
                                                    Math.Min(255, themeColor.R + 30),
                                                    Math.Min(255, themeColor.G + 30),
                                                    Math.Min(255, themeColor.B + 30)
                                                ));
                                                System.Diagnostics.Debug.WriteLine($"Set UltraButton appearance BackColor: {control.Name}");
                                            }
                                        }
                                    }
                                }
                            }

                            // Also set control.BackColor for buttons that respond to it
                            if (isIrsBlue)
                            {
                                control.BackColor = Color.FromArgb(0, 174, 219);
                                control.ForeColor = Color.White;
                            }
                        }
                    }

                    // Recursively apply to child controls
                    if (control.HasChildren)
                    {
                        ApplyThemeToUltraButtons(control.Controls, themeColor);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error theming UltraButtons: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively applies theming to Infragistics UltraLabel controls
        /// </summary>
        private void ApplyThemeToUltraLabels(Control.ControlCollection controls, Color themeColor)
        {
            try
            {
                foreach (Control control in controls)
                {
                    // Handle UltraLabel controls
                    if (control.GetType().Name == "UltraLabel")
                    {
                        bool isIrsBlue = (themeColor.R == 0 && themeColor.G == 174 && themeColor.B == 219);
                        string labelName = control.Name?.ToLower() ?? "";

                        if (isIrsBlue)
                        {
                            if (labelName.Equals("txtnettotal", StringComparison.OrdinalIgnoreCase))
                            {
                                // Net total digital display - white text on dark orange background
                                var appearanceProp = control.GetType().GetProperty("Appearance");
                                if (appearanceProp != null)
                                {
                                    var appearance = appearanceProp.GetValue(control);
                                    appearance?.GetType().GetProperty("ForeColor")?.SetValue(appearance, Color.FromArgb(255, 230, 100));
                                }
                                control.ForeColor = Color.FromArgb(255, 230, 100);
                            }
                            else
                            {
                                // Standard labels - transparent background, dark text
                                var appearanceProp = control.GetType().GetProperty("Appearance");
                                if (appearanceProp != null)
                                {
                                    var appearance = appearanceProp.GetValue(control);
                                    if (appearance != null)
                                    {
                                        appearance.GetType().GetProperty("BackColor")?.SetValue(appearance, Color.Transparent);
                                        appearance.GetType().GetProperty("ForeColor")?.SetValue(appearance, Color.FromArgb(20, 30, 50));
                                    }
                                }
                                control.BackColor = Color.Transparent;
                                control.ForeColor = Color.FromArgb(20, 30, 50);
                            }
                        }
                        else
                        {
                            // Only theme background labels, not text labels
                            if (control.BackColor != Color.Transparent)
                            {
                                control.BackColor = Color.FromArgb(
                                    Math.Min(255, themeColor.R + 10),
                                    Math.Min(255, themeColor.G + 10),
                                    Math.Min(255, themeColor.B + 10)
                                );
                                System.Diagnostics.Debug.WriteLine($"Set UltraLabel BackColor: {control.Name}");
                            }
                        }
                    }

                    // Recursively apply to child controls
                    if (control.HasChildren)
                    {
                        ApplyThemeToUltraLabels(control.Controls, themeColor);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error theming UltraLabels: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies the selected language
        /// </summary>
        private void ApplyLanguage(string language)
        {
            try
            {
                // Here you would implement language switching logic
                System.Diagnostics.Debug.WriteLine($"Language changed to: {language}");
                MessageBox.Show($"Language changed to: {language}", "Language Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying language: {ex.Message}");
            }
        }

        private string GetFavoritesFilePath()
        {
            return System.IO.Path.Combine(Application.StartupPath, "favorites.txt");
        }

        private void LoadFavorites()
        {
            try
            {
                string path = GetFavoritesFilePath();
                if (System.IO.File.Exists(path))
                {
                    var lines = System.IO.File.ReadAllLines(path);
                    var group = ultraExplorerBarSideMenu.Groups["MyFavouriteMenu"];
                    if (group != null)
                    {
                        foreach (var line in lines)
                        {
                            var parts = line.Split('|');
                            if (parts.Length == 2)
                            {
                                string toolKey = parts[0];
                                string caption = parts[1];

                                // Prevent duplicates
                                if (!group.Items.Exists(toolKey))
                                {
                                    var newItem = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarItem();
                                    newItem.Key = toolKey;
                                    newItem.Text = caption;
                                    group.Items.Add(newItem);
                                    StyleFavouriteItem(newItem);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load favorites: {ex.Message}");
            }
        }

        /// <summary>
        /// Rewrites the favourites file from the current items in the group (persists removals)
        /// </summary>
        private void SaveAllFavorites()
        {
            try
            {
                var group = ultraExplorerBarSideMenu.Groups["MyFavouriteMenu"];
                if (group != null)
                {
                    var lines = new System.Collections.Generic.List<string>();
                    foreach (Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarItem item in group.Items)
                    {
                        if (item.Key != "AddToFavourite")
                        {
                            lines.Add($"{item.Key}|{item.Text}");
                        }
                    }
                    System.IO.File.WriteAllLines(GetFavoritesFilePath(), lines);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save favorites: {ex.Message}");
            }
        }

        private void ultraExplorerBarSideMenu_ItemClick(object sender, Infragistics.Win.UltraWinExplorerBar.ItemEventArgs e)
        {
            try
            {
                if (e.Item.Key == "AddToFavourite")
                {
                    if (tabControlMain.SelectedTab != null)
                    {
                        string activeTabText = tabControlMain.SelectedTab.Text;
                        // Read the tool key stored in the Tab's Tag property
                        string matchedToolKey = tabControlMain.SelectedTab.Tag as string;

                        if (string.IsNullOrEmpty(matchedToolKey))
                        {
                            MessageBox.Show("Could not determine the toolbar action for this form.");
                            return;
                        }

                        var group = ultraExplorerBarSideMenu.Groups["MyFavouriteMenu"];
                        if (group != null)
                        {
                            if (!group.Items.Exists(matchedToolKey))
                            {
                                var newItem = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarItem();
                                newItem.Key = matchedToolKey;
                                newItem.Text = activeTabText;
                                group.Items.Add(newItem);
                                StyleFavouriteItem(newItem);

                                // Save to file
                                SaveAllFavorites();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please open a form first to add it to favorites.");
                    }
                }
                else
                {
                    // It's a favorite item click — always open a new instance
                    string keyToExecute = e.Item.Key;
                    if (ultraToolbarsManager1.Tools.Exists(keyToExecute))
                    {
                        _openingFromFavourite = true;
                        try
                        {
                            var tool = ultraToolbarsManager1.Tools[keyToExecute];
                            var eventArgs = new Infragistics.Win.UltraWinToolbars.ToolClickEventArgs(tool, null);
                            ultraToolbarsManager1_ToolClick_1(this, eventArgs);
                        }
                        finally
                        {
                            _openingFromFavourite = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Could not find the action for this favorite item: {e.Item.Text}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in favorites: {ex.Message}");
            }
        }

        private void ultraExplorerBarSideMenu_ContextMenuInitializing(object sender, Infragistics.Win.UltraWinExplorerBar.CancelableContextMenuInitializingEventArgs e)
        {
            try
            {
                // Only modify context menu for items, not groups
                if (e.Item != null)
                {
                    // "Add To Favourite" cannot be renamed or removed
                    if (e.Item.Key == "AddToFavourite")
                    {
                        e.Cancel = true;
                        return;
                    }

                    // Cancel the default context menu entirely
                    e.Cancel = true;

                    // Build our own simple context menu with just "Remove"
                    var customMenu = new System.Windows.Forms.ContextMenuStrip();
                    var removeMenuItem = new System.Windows.Forms.ToolStripMenuItem("Remove");
                    var itemToRemove = e.Item;
                    removeMenuItem.Click += (s2, e2) =>
                    {
                        var group = itemToRemove.Group;
                        if (group != null)
                        {
                            group.Items.Remove(itemToRemove);
                            SaveAllFavorites();
                        }
                    };
                    customMenu.Items.Add(removeMenuItem);
                    customMenu.Show(ultraExplorerBarSideMenu, ultraExplorerBarSideMenu.PointToClient(System.Windows.Forms.Cursor.Position));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing context menu: {ex.Message}");
            }
        }

        /// <summary>
        /// Populates the MyScreenColour group with a grid of clickable color square blocks
        /// </summary>
        private void PopulateColorPalette()
        {
            try
            {
                var screenGroup = ultraExplorerBarSideMenu.Groups["MyScreenColour"];
                if (screenGroup == null) return;

                // Define the preset colors
                // Each entry: { baseColor, highlightColor (top), shadowColor (bottom), label }
                var presets = new[]
                {
                    new { Base = Color.FromArgb(0, 174, 219),   Hi = Color.FromArgb(100, 220, 255), Lo = Color.FromArgb(0, 140, 186),   Label = "IRS Blue" },
                    new { Base = Color.FromArgb(0, 200, 220),   Hi = Color.FromArgb(120, 240, 255), Lo = Color.FromArgb(0, 140, 160),   Label = "Cyan" },
                    new { Base = Color.FromArgb(170, 200, 235), Hi = Color.FromArgb(220, 235, 255), Lo = Color.FromArgb(120, 155, 195), Label = "Pearl Blue" },
                    new { Base = Color.FromArgb(240, 230, 210), Hi = Color.FromArgb(255, 250, 240), Lo = Color.FromArgb(200, 185, 160), Label = "Pearl Cream" },
                    new { Base = Color.FromArgb(160, 160, 165), Hi = Color.FromArgb(210, 210, 215), Lo = Color.FromArgb(110, 110, 115), Label = "Grey" },
                    new { Base = Color.FromArgb(30, 30, 35),    Hi = Color.FromArgb(80, 80, 90),    Lo = Color.FromArgb(5, 5, 10),      Label = "Black" }
                };

                int blockSize = 36;
                int spacing = 4;

                // Create a container panel for the color blocks
                FlowLayoutPanel colorPanel = new FlowLayoutPanel();
                colorPanel.FlowDirection = FlowDirection.LeftToRight;
                colorPanel.WrapContents = true;
                colorPanel.AutoSize = false;
                colorPanel.Width = (blockSize + spacing + 2) * presets.Length + spacing + 8;
                colorPanel.Height = blockSize + spacing * 2 + 20; // extra height for label
                colorPanel.Padding = new Padding(4);
                colorPanel.BackColor = Color.FromArgb(215, 236, 255);
                colorPanel.Name = "colorPalettePanel";

                foreach (var preset in presets)
                {
                    // Outer wrapper to hold the metallic block + label
                    Panel wrapper = new Panel();
                    wrapper.Size = new Size(blockSize + 2, blockSize + 16);
                    wrapper.Margin = new Padding(spacing / 2 + 1);
                    wrapper.BackColor = Color.Transparent;

                    // The metallic color block (owner-drawn for gradient)
                    Panel block = new Panel();
                    block.Size = new Size(blockSize, blockSize);
                    block.Location = new Point(1, 0);
                    block.Cursor = Cursors.Hand;
                    block.Tag = preset.Base;
                    block.Name = $"colorBlock_{preset.Label.Replace(" ", "")}";

                    // Owner-draw metallic gradient
                    Color hi = preset.Hi;
                    Color lo = preset.Lo;
                    block.Paint += (s, pev) =>
                    {
                        Panel p = (Panel)s;
                        using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                            p.ClientRectangle, hi, lo, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                        {
                            pev.Graphics.FillRectangle(brush, p.ClientRectangle);
                        }
                        // Draw a subtle highlight line at the top for metallic shine
                        using (var pen = new Pen(Color.FromArgb(80, 255, 255, 255), 1))
                        {
                            pev.Graphics.DrawLine(pen, 1, 1, p.Width - 2, 1);
                        }
                        // Draw border
                        using (var pen = new Pen(Color.FromArgb(100, 0, 0, 0), 1))
                        {
                            pev.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
                        }
                    };

                    // Apply theme on click
                    Color baseColor = preset.Base;
                    block.Click += (s, ev) =>
                    {
                        currentThemeColor = baseColor;

                        // Apply to the Home form itself
                        this.BackColor = Color.FromArgb(
                            Math.Min(255, baseColor.R + 5),
                            Math.Min(255, baseColor.G + 5),
                            Math.Min(255, baseColor.B + 5)
                        );

                        // Apply to all open tabbed forms
                        ApplyThemeToTabbedForms(baseColor);

                        System.Diagnostics.Debug.WriteLine($"Theme color changed to: {preset.Label}");
                    };

                    // Hover effect
                    block.MouseEnter += (s, ev) => { ((Panel)s).Invalidate(); };
                    block.MouseLeave += (s, ev) => { ((Panel)s).Invalidate(); };

                    // Label below the block
                    Label lbl = new Label();
                    lbl.Text = preset.Label;
                    lbl.Font = new Font("Segoe UI", 6.5f, FontStyle.Regular);
                    lbl.ForeColor = Color.FromArgb(60, 60, 60);
                    lbl.BackColor = Color.Transparent;
                    lbl.TextAlign = ContentAlignment.TopCenter;
                    lbl.Size = new Size(blockSize + 2, 14);
                    lbl.Location = new Point(0, blockSize + 1);

                    wrapper.Controls.Add(block);
                    wrapper.Controls.Add(lbl);
                    colorPanel.Controls.Add(wrapper);
                }

                // Add the color panel as an embedded control in the MyScreenColour group
                // Use an UltraExplorerBarContainerControl to host the FlowLayoutPanel
                screenGroup.Settings.Style = Infragistics.Win.UltraWinExplorerBar.GroupStyle.ControlContainer;

                // Find or create the container control
                string containerKey = "MyScreenColourContainer";
                Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl container = null;

                // Check if container exists
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl existingContainer
                        && existingContainer.Name == "MyScreenColourContainer")
                    {
                        container = existingContainer;
                        break;
                    }
                }

                if (container == null)
                {
                    container = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl();
                    container.Name = containerKey;
                    this.Controls.Add(container);
                }

                screenGroup.Container = container;
                container.Controls.Clear();
                container.Controls.Add(colorPanel);
                container.Height = colorPanel.Height + 8;

                System.Diagnostics.Debug.WriteLine("Color palette populated successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error populating color palette: {ex.Message}");
            }
        }

        private void StyleFavouriteItem(Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarItem item)
        {
            try
            {
                item.Settings.ReserveImageSpace = Infragistics.Win.DefaultableBoolean.False;
                item.Settings.UseDefaultImage = Infragistics.Win.DefaultableBoolean.False;

                // ===== BUTTON STYLE =====
                item.Settings.Style = Infragistics.Win.UltraWinExplorerBar.ItemStyle.Button;
                item.Settings.MaxLines = 2;

                // ===== HEIGHT TO MATCH IRS POS =====
                item.Settings.Height = 28;

                // ===== REMOVE THEMED ELEMENT =====
                item.Settings.AppearancesSmall.Appearance.ThemedElementAlpha =
                    Infragistics.Win.Alpha.Transparent;
                item.Settings.AppearancesSmall.HotTrackAppearance.ThemedElementAlpha =
                    Infragistics.Win.Alpha.Transparent;
                item.Settings.AppearancesSmall.ActiveAppearance.ThemedElementAlpha =
                    Infragistics.Win.Alpha.Transparent;

                // ===== BLUE GRADIENT BACKGROUND =====
                item.Settings.AppearancesSmall.Appearance.BackColor = SidebarTheme.AccentPrimary;
                item.Settings.AppearancesSmall.Appearance.BackColor2 = SidebarTheme.AccentDark;
                item.Settings.AppearancesSmall.Appearance.BackGradientStyle =
                    Infragistics.Win.GradientStyle.Vertical;

                // ===== REMOVE HARD BORDER =====
                item.Settings.AppearancesSmall.Appearance.BorderAlpha =
                    Infragistics.Win.Alpha.Transparent;

                // ===== FONT STYLING =====
                item.Settings.AppearancesSmall.Appearance.FontData.Bold =
                    Infragistics.Win.DefaultableBoolean.True;
                item.Settings.AppearancesSmall.Appearance.FontData.SizeInPoints = 8.5f;
                item.Settings.AppearancesSmall.Appearance.FontData.Name = SidebarTheme.FontFamily;

                // ===== CENTER TEXT =====
                item.Settings.AppearancesSmall.Appearance.TextHAlignAsString = "Center";
                item.Settings.AppearancesSmall.Appearance.TextVAlignAsString = "Middle";

                // ===== HOVER APPEARANCE =====
                item.Settings.AppearancesSmall.HotTrackAppearance.BackColor =
                    Color.FromArgb(50, 170, 250);
                item.Settings.AppearancesSmall.HotTrackAppearance.BackColor2 =
                    SidebarTheme.AccentDark;
                item.Settings.AppearancesSmall.HotTrackAppearance.BackGradientStyle =
                    Infragistics.Win.GradientStyle.Vertical;
                item.Settings.AppearancesSmall.HotTrackAppearance.BorderAlpha =
                    Infragistics.Win.Alpha.Transparent;
                item.Settings.AppearancesSmall.HotTrackAppearance.FontData.Bold =
                    Infragistics.Win.DefaultableBoolean.True;
                item.Settings.AppearancesSmall.HotTrackAppearance.FontData.SizeInPoints = 8.5f;
                item.Settings.AppearancesSmall.HotTrackAppearance.FontData.Name = SidebarTheme.FontFamily;

                // ===== ADD TO FAVOURITE vs REGULAR ITEMS =====
                if (item.Key == "AddToFavourite")
                {
                    // Blue background, RED text (IRS POS style)
                    item.Settings.AppearancesSmall.Appearance.ForeColor = Color.Red;
                    item.Settings.AppearancesSmall.HotTrackAppearance.ForeColor = Color.Red;
                }
                else
                {
                    // Blue background, WHITE text (IRS POS style)
                    item.Settings.AppearancesSmall.Appearance.ForeColor = Color.White;
                    item.Settings.AppearancesSmall.HotTrackAppearance.ForeColor = Color.White;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error styling favourite item: {ex.Message}");
            }
        }

        #region Report Navigator Logic

        private void InitializeReportNavigator()
        {
            try
            {
                // 1. Create Wrapper Panel to host Header and ExplorerBar seamlessly
                panelReportNavigatorWrapper = new Panel();
                panelReportNavigatorWrapper.Dock = DockStyle.Left;
                panelReportNavigatorWrapper.Width = 285;
                panelReportNavigatorWrapper.Name = "panelReportNavigatorWrapper";
                panelReportNavigatorWrapper.Visible = false;
                panelReportNavigatorWrapper.BackColor = Color.FromArgb(215, 236, 255); // soft office-blue background
                panelReportNavigatorWrapper.Padding = new Padding(2); // subtle border and breathing room

                // 2. Create standard IRS POS Report Navigator Header
                Panel headerPanel = new Panel();
                headerPanel.Dock = DockStyle.Top;
                headerPanel.Height = 28;
                headerPanel.BackColor = Color.FromArgb(202, 224, 247); // softer, cleaner header tone
                headerPanel.Margin = new Padding(0);

                // Add gradient paint to header to match IRS Office2007 Blue Dock Pane exactly
                headerPanel.Paint += (s, e) => 
                {
                    Rectangle rect = headerPanel.ClientRectangle;
                    if (rect.Width <= 0 || rect.Height <= 0) return;

                    using (LinearGradientBrush b = new LinearGradientBrush(rect, Color.FromArgb(241, 247, 255), Color.FromArgb(191, 216, 243), LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(b, rect);
                    }
                    using (Pen borderPen = new Pen(Color.FromArgb(142, 179, 220)))
                    using (Font headerFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    using (SolidBrush headerBrush = new SolidBrush(Color.FromArgb(18, 64, 126)))
                    {
                        e.Graphics.DrawString("Report Navigator", headerFont, headerBrush, new PointF(8, 6));
                        e.Graphics.DrawLine(borderPen, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
                    }
                };

                // Add IRS POS matched Close button
                AddPaneCaptionButton(headerPanel, 20, true, (s, e) => HideReportNavigator());
                
                // Add IRS POS matched Pin button, now used to keep the navigator open
                AddPaneCaptionButton(headerPanel, 40, false, (s, e) => ToggleReportNavigatorPin());

                panelReportNavigatorWrapper.Controls.Add(headerPanel);

                // 3. Initialize ExplorerBar
                ultraExplorerBarReportNavigator = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBar();
                ((System.ComponentModel.ISupportInitialize)(ultraExplorerBarReportNavigator)).BeginInit();

                ultraExplorerBarReportNavigator.Dock = DockStyle.Fill;
                ultraExplorerBarReportNavigator.Name = "ultraExplorerBarReportNavigator";
                
                // IRS POS uses standard ExplorerBar style
                ultraExplorerBarReportNavigator.Style = Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarStyle.ExplorerBar;
                ultraExplorerBarReportNavigator.GroupSettings.HeaderVisible = Infragistics.Win.DefaultableBoolean.True;
                ultraExplorerBarReportNavigator.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
                ultraExplorerBarReportNavigator.ImageListSmall = null;
                ultraExplorerBarReportNavigator.ImageListLarge = null;
                ultraExplorerBarReportNavigator.Appearance.BackColor = Color.FromArgb(215, 236, 255); // Seamless match
                ultraExplorerBarReportNavigator.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;
                ultraExplorerBarReportNavigator.ContextMenuInitializing += (s, e) =>
                {
                    e.Cancel = true;
                };
                
                panelReportNavigatorWrapper.Controls.Add(ultraExplorerBarReportNavigator);
                ultraExplorerBarReportNavigator.BringToFront();

                this.Controls.Add(panelReportNavigatorWrapper);
                
                // CRITICAL FIX: Z-Order must match ultraExplorerBarSideMenu so Docking works without overlapping the main panel or ribbon
                int sideMenuIndex = this.Controls.GetChildIndex(ultraExplorerBarSideMenu);
                this.Controls.SetChildIndex(panelReportNavigatorWrapper, sideMenuIndex);

                // Define categories from the item catalog so empty groups are not shown
                foreach (string category in ReportNavigatorDefinitions.Select(def => def.Category).Distinct())
                {
                    var group = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup();
                    group.Key = $"Grp_{category}";
                    group.Text = category;
                    group.Settings.ShowExpansionIndicator = Infragistics.Win.DefaultableBoolean.True;
                    group.Expanded = false;
                    ultraExplorerBarReportNavigator.Groups.Add(group);
                }

                foreach (ReportNavigatorDefinition definition in ReportNavigatorDefinitions)
                {
                    AddReportItem(definition);
                }

                StyleReportNavigator();

                ultraExplorerBarReportNavigator.ItemClick += ReportNavigator_ItemClick;
                
                ((System.ComponentModel.ISupportInitialize)(ultraExplorerBarReportNavigator)).EndInit();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing Report Navigator: {ex.Message}");
            }
        }

        private void AddPaneCaptionButton(Panel headerPanel, int rightOffset, bool isClose, EventHandler clickHandler)
        {
            Panel btn = new Panel();
            btn.Size = new Size(16, 15);
            btn.Location = new Point(headerPanel.Width - rightOffset, 4);
            btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn.BackColor = Color.Transparent;
            btn.Cursor = Cursors.Hand;
            btn.TabStop = true;
            btn.AccessibleRole = AccessibleRole.PushButton;
            btn.AccessibleName = isClose ? "Close report navigator" : "Pin report navigator";
            btn.AccessibleDescription = isClose ? "Closes the report navigator pane" : "Pins or unpins the report navigator pane";
            
            bool isHovered = false;
            btn.MouseEnter += (s, e) => { isHovered = true; btn.Invalidate(); };
            btn.MouseLeave += (s, e) => { isHovered = false; btn.Invalidate(); };
            if (clickHandler != null) btn.Click += clickHandler;
            if (!isClose)
            {
                _reportNavigatorPinButton = btn;
                toolTip.SetToolTip(btn, "Pin report navigator");
            }
            else
            {
                toolTip.SetToolTip(btn, "Close report navigator");
            }

            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None; // Crisp lines matching IRS
                
                if (isHovered)
                {
                    // Soft orange/yellow hover effect typical of Office 2007 dock buttons
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(255, 244, 214)))
                    using (Pen p = new Pen(Color.FromArgb(255, 177, 86)))
                    {
                        e.Graphics.FillRectangle(b, 0, 0, 15, 14);
                        e.Graphics.DrawRectangle(p, 0, 0, 15, 14);
                    }
                }
                else
                {
                    // Gradient matched to IRS button box
                    using (LinearGradientBrush b = new LinearGradientBrush(new Rectangle(0, 0, 16, 16), Color.FromArgb(245, 250, 255), Color.FromArgb(194, 218, 244), LinearGradientMode.Vertical))
                    using (Pen p = new Pen(Color.FromArgb(115, 151, 196))) 
                    {
                        e.Graphics.FillRectangle(b, 0, 0, 15, 14);
                        e.Graphics.DrawRectangle(p, 0, 0, 15, 14);
                    }
                }

                // White drop shadow for icons
                using (Pen shadowPen = new Pen(Color.FromArgb(150, 255, 255, 255), 1.5f))
                using (Pen iconPen = new Pen(Color.FromArgb(21, 66, 139), 1.5f))
                {
                    if (isClose)
                    {
                        // Shadow
                        e.Graphics.DrawLine(shadowPen, 4, 5, 11, 12);
                        e.Graphics.DrawLine(shadowPen, 4, 12, 11, 5);
                        // Main X
                        e.Graphics.DrawLine(iconPen, 4, 4, 11, 11);
                        e.Graphics.DrawLine(iconPen, 4, 11, 11, 4);
                    }
                    else
                    {
                        // Pin icon
                        if (_isReportNavigatorPinned)
                        {
                            e.Graphics.DrawLine(iconPen, 8, 10, 8, 13); // Needle
                            using (SolidBrush bodyBrush = new SolidBrush(Color.FromArgb(21, 66, 139)))
                            {
                                e.Graphics.FillRectangle(bodyBrush, 5, 6, 7, 4); // Body
                            }
                            e.Graphics.DrawLine(iconPen, 4, 5, 12, 5); // Head
                        }
                        else
                        {
                            e.Graphics.DrawLine(iconPen, 8, 7, 8, 13); // Needle
                            e.Graphics.DrawLine(iconPen, 5, 6, 11, 6); // Head
                            e.Graphics.DrawLine(iconPen, 6, 8, 10, 8); // Body
                        }
                    }
                }
            };
            
            headerPanel.Controls.Add(btn);
        }

        private void AddReportItem(ReportNavigatorDefinition definition)
        {
            var group = ultraExplorerBarReportNavigator.Groups[$"Grp_{definition.Category}"];
            if (group != null)
            {
                var item = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarItem();
                item.Key = definition.Key;
                item.Text = "  •  " + definition.Text;
                item.ToolTipText = definition.Text;
                group.Items.Add(item);
            }
        }

        private void StyleReportNavigator()
        {
            try
            {
                // Setup overall background to match IRS perfectly 
                ultraExplorerBarReportNavigator.Appearance.BackColor = Color.FromArgb(215, 236, 255);
                
                ultraExplorerBarReportNavigator.Margins.Top = 2;
                ultraExplorerBarReportNavigator.Margins.Bottom = 2;
                ultraExplorerBarReportNavigator.Margins.Left = 2;
                ultraExplorerBarReportNavigator.Margins.Right = 2;
                ultraExplorerBarReportNavigator.GroupSpacing = 3;
                
                foreach (var group in ultraExplorerBarReportNavigator.Groups)
                {
                    // Modern, flat solid design for group headers
                    group.Settings.AppearancesSmall.HeaderAppearance.BackColor = Color.FromArgb(198, 222, 246);
                    group.Settings.AppearancesSmall.HeaderAppearance.BackColor2 = Color.Empty;
                    group.Settings.AppearancesSmall.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    group.Settings.AppearancesSmall.HeaderAppearance.ForeColor = Color.FromArgb(24, 66, 115);
                    group.Settings.AppearancesSmall.HeaderAppearance.FontData.Name = "Segoe UI";
                    group.Settings.AppearancesSmall.HeaderAppearance.FontData.SizeInPoints = 9.5f;
                    group.Settings.AppearancesSmall.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                    group.Settings.AppearancesSmall.HeaderAppearance.BorderColor = Color.FromArgb(165, 195, 225);
                    group.Settings.AppearancesSmall.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                    group.Settings.AppearancesSmall.HeaderHotTrackAppearance.BackColor = Color.FromArgb(215, 235, 255);
                    group.Settings.AppearancesSmall.HeaderHotTrackAppearance.BackColor2 = Color.Empty;
                    group.Settings.AppearancesSmall.HeaderHotTrackAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    group.Settings.AppearancesSmall.HeaderHotTrackAppearance.ForeColor = Color.FromArgb(24, 66, 115);
                    group.Settings.AppearancesSmall.HeaderHotTrackAppearance.BorderColor = Color.FromArgb(165, 195, 225);
                    group.Settings.AppearancesSmall.HeaderHotTrackAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                    // Item area styling - richer soft blue background to avoid looking "full in white"
                    group.Settings.AppearancesSmall.ItemAreaAppearance.BackColor = Color.FromArgb(215, 230, 246);
                    group.Settings.AppearancesSmall.ItemAreaAppearance.BorderColor = Color.FromArgb(165, 190, 215);
                    group.Settings.ItemAreaInnerMargins.Left = 6;
                    group.Settings.ItemAreaInnerMargins.Top = 6;
                    group.Settings.ItemAreaInnerMargins.Right = 6;
                    group.Settings.ItemAreaInnerMargins.Bottom = 6;
                    
                    // Set Group Header Icons to match IRS categories
                    try
                    {
                        if (group.Key == "Grp_Item") group.Settings.AppearancesSmall.HeaderAppearance.Image = PosBranch_Win.Properties.Resources.list_items;
                        else if (group.Key == "Grp_Sales") group.Settings.AppearancesSmall.HeaderAppearance.Image = PosBranch_Win.Properties.Resources.transaction1;
                        else if (group.Key == "Grp_Purchase") group.Settings.AppearancesSmall.HeaderAppearance.Image = PosBranch_Win.Properties.Resources.commercial__1_;
                        else if (group.Key == "Grp_Discount") group.Settings.AppearancesSmall.HeaderAppearance.Image = PosBranch_Win.Properties.Resources.rate_of_return;
                        else if (group.Key == "Grp_Customer") group.Settings.AppearancesSmall.HeaderAppearance.Image = PosBranch_Win.Properties.Resources.icons8_roles_96;
                        else if (group.Key == "Grp_Vendor") group.Settings.AppearancesSmall.HeaderAppearance.Image = PosBranch_Win.Properties.Resources.transaction;
                        else if (group.Key == "Grp_Analysis") group.Settings.AppearancesSmall.HeaderAppearance.Image = PosBranch_Win.Properties.Resources.profit_and_loss;
                        else if (group.Key == "Grp_Others") group.Settings.AppearancesSmall.HeaderAppearance.Image = PosBranch_Win.Properties.Resources.report1;
                    } catch { }

                    foreach (Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarItem item in group.Items)
                    {
                        ApplyReportNavigatorItemStyle(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error styling Report Navigator: {ex.Message}");
            }
        }

        private void ClosingAlertTimer_Tick(object sender, EventArgs e)
        {
            if (SessionContext.UserLevel?.Equals("Administrator", StringComparison.OrdinalIgnoreCase) == true)
            {
                return;
            }

            DateTime now = DateTime.Now;

            if (SessionContext.IsInitialized && SessionContext.LoginTime != DateTime.MinValue &&
                now.Date > SessionContext.LoginTime.Date)
            {
                SessionContext.RequiresClosing = true;

                if (!closingRequiredAlertShown)
                {
                    closingRequiredAlertShown = true;
                    MessageBox.Show("Please complete shift closing before continuing transactions.",
                        "Shift Closing Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                return;
            }

            if (!closingAlertShown && SessionContext.ShouldShowClosingAlert(now))
            {
                closingAlertShown = true;
                MessageBox.Show("Please complete shift closing before day end.",
                    "Shift Closing Reminder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool IsBlockedTransactionTool(string toolKey)
        {
            if (string.IsNullOrWhiteSpace(toolKey))
                return false;

            string[] blockedTools =
            {
                "Pos",
                "Sales",
                "Sales Return",
                "Receipt",
                "Payment",
                "DebitNote",
                "CreditNote",
                "Purchase",
                "Purchase Order",
                "Purchase R/n",
                "stockadjustment",
                "Goods Received"
            };

            return blockedTools.Any(key => key.Equals(toolKey, StringComparison.OrdinalIgnoreCase));
        }

        private bool EnsureTransactionAllowed(string toolKey)
        {
            if (!IsBlockedTransactionTool(toolKey))
                return true;

            if (ShiftSessionGuard.CanDoTransaction(out string transactionError))
                return true;

            MessageBox.Show(transactionError, "Shift Closing Required",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private bool IsTransactionForm(Form form)
        {
            return form is frmPos ||
                   form is Transaction.frmSalesInvoice ||
                   form is Transaction.frmSalesReturn ||
                   form is Transaction.FrmPurchase ||
                   form is Transaction.frmPurchaseOrder ||
                   form is Transaction.frmPurchaseReturn ||
                   form is Transaction.FrmStockAdjustment ||
                   form is Accounts.FrmReceipt ||
                   form is Accounts.FrmPayment ||
                   form is Accounts.FrmDebitNote ||
                   form is Accounts.FrmCreditNote;
        }

        private bool EnsureTransactionAllowed(Form form)
        {
            if (!IsTransactionForm(form))
                return true;

            if (ShiftSessionGuard.CanDoTransaction(out string transactionError))
                return true;

            MessageBox.Show(transactionError, "Shift Closing Required",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void ApplyReportNavigatorItemStyle(Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarItem item)
        {
            item.Settings.Style = Infragistics.Win.UltraWinExplorerBar.ItemStyle.Button;
            item.Settings.ReserveImageSpace = Infragistics.Win.DefaultableBoolean.False;
            item.Settings.UseDefaultImage = Infragistics.Win.DefaultableBoolean.False;
            item.Settings.Height = SidebarTheme.ItemHeight;
            item.Settings.MaxLines = 1;
            item.Settings.Indent = SidebarTheme.ItemIndent;

            var normal = item.Settings.AppearancesSmall.Appearance;
            normal.ForeColor = SidebarTheme.TextPrimary;
            normal.FontData.SizeInPoints = SidebarTheme.ItemFontSize;
            normal.FontData.Name = SidebarTheme.FontFamily;
            normal.FontData.Underline = Infragistics.Win.DefaultableBoolean.False;
            normal.Cursor = Cursors.Hand;
            normal.TextHAlignAsString = "Left";
            normal.TextVAlignAsString = "Middle";
            normal.BackColor = Color.Transparent;
            normal.BackColor2 = Color.Empty;
            normal.BackGradientStyle = Infragistics.Win.GradientStyle.None;
            normal.BorderAlpha = Infragistics.Win.Alpha.Transparent;
            normal.Image = null;
            normal.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

            item.Settings.AppearancesLarge.Appearance.Image = null;
            item.Settings.AppearancesSmall.ActiveAppearance.Image = null;
            item.Settings.AppearancesLarge.ActiveAppearance.Image = null;
            item.Settings.AppearancesSmall.HotTrackAppearance.Image = null;
            item.Settings.AppearancesLarge.HotTrackAppearance.Image = null;

            item.Settings.AppearancesLarge.Appearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;
            item.Settings.AppearancesLarge.HotTrackAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;
            item.Settings.AppearancesLarge.ActiveAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

            // Active/selected — FLAT light blue fill, dark blue bold text. Modern, not dated.
            var active = item.Settings.AppearancesSmall.ActiveAppearance;
            active.BackColor = Color.FromArgb(214, 235, 255);
            active.BackColor2 = Color.Empty;
            active.BackGradientStyle = Infragistics.Win.GradientStyle.None;
            active.BorderAlpha = Infragistics.Win.Alpha.Transparent;
            active.ForeColor = SidebarTheme.TextPrimary;
            active.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
            active.FontData.Underline = Infragistics.Win.DefaultableBoolean.False;
            active.FontData.SizeInPoints = SidebarTheme.ItemFontSize;
            active.FontData.Name = SidebarTheme.FontFamily;
            active.TextHAlignAsString = "Left";
            active.TextVAlignAsString = "Middle";
            active.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

            // Hover — soft tint, flat, no underline
            var hot = item.Settings.AppearancesSmall.HotTrackAppearance;
            hot.BackColor = SidebarTheme.HoverBackground;
            hot.BackColor2 = Color.Empty;
            hot.BackGradientStyle = Infragistics.Win.GradientStyle.None;
            hot.BorderAlpha = Infragistics.Win.Alpha.Transparent; // no border either — cleaner than the boxed hover before
            hot.ForeColor = SidebarTheme.HoverText;
            hot.FontData.Bold = Infragistics.Win.DefaultableBoolean.False; // don't bold on hover — only active should feel "selected"
            hot.FontData.Underline = Infragistics.Win.DefaultableBoolean.False;
            hot.FontData.SizeInPoints = SidebarTheme.ItemFontSize;
            hot.FontData.Name = SidebarTheme.FontFamily;
            hot.TextHAlignAsString = "Left";
            hot.TextVAlignAsString = "Middle";
            hot.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;
        }

        private void SetActiveReportNavigatorItem(Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarItem activeItem)
        {
            if (activeItem == null)
                return;

            _activeReportNavigatorItemKey = activeItem.Key;
            try
            {
                ultraExplorerBarReportNavigator.ActiveItem = activeItem;
            }
            catch
            {
            }
        }

        private void ClearActiveReportNavigatorItem()
        {
            _activeReportNavigatorItemKey = null;
            try
            {
                if (ultraExplorerBarReportNavigator != null)
                {
                    ultraExplorerBarReportNavigator.ActiveItem = null;
                }
            }
            catch
            {
            }
        }

        private void SyncReportNavigatorActiveItemWithOpenTabs()
        {
            if (string.IsNullOrEmpty(_activeReportNavigatorItemKey) || ultraExplorerBarReportNavigator == null)
                return;

            string actionKey = ResolveReportNavigatorActionKey(_activeReportNavigatorItemKey);

            foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
            {
                string tabToolKey = tab.Tag as string;
                if (string.Equals(tabToolKey, actionKey, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            ClearActiveReportNavigatorItem();
        }

        private void ShowReportNavigator()
        {
            if (ultraExplorerBarSideMenu != null)
                ultraExplorerBarSideMenu.Visible = false;

            if (panelReportNavigatorWrapper != null)
                panelReportNavigatorWrapper.Visible = true;

            _isReportNavigatorVisible = true;
        }

        private void HideReportNavigator()
        {
            if (panelReportNavigatorWrapper != null)
                panelReportNavigatorWrapper.Visible = false;

            if (ultraExplorerBarSideMenu != null)
                ultraExplorerBarSideMenu.Visible = true;

            _isReportNavigatorPinned = false;
            UpdateReportNavigatorPinButtonState();
            _isReportNavigatorVisible = false;
        }

        private void ToggleReportNavigatorPin()
        {
            _isReportNavigatorPinned = !_isReportNavigatorPinned;
            UpdateReportNavigatorPinButtonState();
        }

        private void UpdateReportNavigatorPinButtonState()
        {
            if (_reportNavigatorPinButton == null)
            {
                return;
            }

            toolTip.SetToolTip(_reportNavigatorPinButton, _isReportNavigatorPinned ? "Unpin report navigator" : "Pin report navigator");
            _reportNavigatorPinButton.AccessibleName = _isReportNavigatorPinned ? "Unpin report navigator" : "Pin report navigator";
            _reportNavigatorPinButton.AccessibleDescription = _isReportNavigatorPinned ? "Keeps the report navigator open after report clicks" : "Allows the report navigator to auto-hide after report clicks";
            _reportNavigatorPinButton.Invalidate();
        }

        private string ResolveReportNavigatorActionKey(string navigatorKey)
        {
            string actionKey;
            if (ReportNavigatorActionAliases.TryGetValue(navigatorKey, out actionKey))
            {
                return actionKey;
            }

            return navigatorKey;
        }

        private void ReportNavigator_ItemClick(object sender, Infragistics.Win.UltraWinExplorerBar.ItemEventArgs e)
        {
            try
            {
                SetActiveReportNavigatorItem(e.Item);

                // Standard report items use ToolClick handler mechanisms already built
                string keyToExecute = ResolveReportNavigatorActionKey(e.Item.Key);
                
                if (ultraToolbarsManager1.Tools.Exists(keyToExecute) ||
                    keyToExecute == "AuditTrail" ||
                    keyToExecute == "VendorPurchaseReport" ||
                    keyToExecute == "VendorDNPaymentReport" ||
                    keyToExecute == "CustomerReceiptReport" ||
                    keyToExecute == "CustomerLedgerReport" ||
                    keyToExecute == "StockValuationReport" ||
                    keyToExecute == "LowStockAlertReport" ||
                    keyToExecute == "StockAdjustmentReport" ||
                    keyToExecute == "CustomerwiseSalesSummaryReport" ||
                    keyToExecute == "SalesmanwiseSalesSummaryReport" ||
                    keyToExecute == "ItemwiseSalesSummaryReport" ||
                    keyToExecute == "SalesmanIncentiveReport" ||
                    keyToExecute == "CounterReport" ||
                    keyToExecute == "ShiftReconciliationReport" ||
                    keyToExecute == "TradingAccount" ||
                    keyToExecute == "UnallocatedPurchaseReturns" ||
                    keyToExecute == "ProfitLossAccount" ||
                    keyToExecute == "StockListingReport")
                {
                    Infragistics.Win.UltraWinToolbars.ToolBase toolToExecute;

                    if (ultraToolbarsManager1.Tools.Exists(keyToExecute))
                    {
                        toolToExecute = ultraToolbarsManager1.Tools[keyToExecute];
                    }
                    else
                    {
                        // Inventory Audit Trail is launched through the shared ToolClick
                        // path even though it does not have its own ribbon button.
                        toolToExecute = new Infragistics.Win.UltraWinToolbars.ButtonTool(keyToExecute);
                    }

                    var eventArgs = new Infragistics.Win.UltraWinToolbars.ToolClickEventArgs(toolToExecute, null);
                    ultraToolbarsManager1_ToolClick_1(this, eventArgs);

                    // Auto-close only when the navigator is not pinned open
                    if (!_isReportNavigatorPinned)
                    {
                        HideReportNavigator();
                    }
                }
                else
                {
                    MessageBox.Show($"Report form for '{e.Item.Text}' is currently unavailable or a placeholder.", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening report: {ex.Message}");
            }
        }

        #endregion

        #region Custom Status Bar (Image 3 Look)

        private Panel pnlCustomStatusBar;
        private Label lblUserVal;
        private Label lblDateVal;
        private Label lblVersionVal;

        private void InitializeCustomStatusBar()
        {
            try
            {
                if (statusStrip != null)
                {
                    statusStrip.Visible = false;
                }

                if (_Home_Toolbars_Dock_Area_Bottom != null)
                {
                    _Home_Toolbars_Dock_Area_Bottom.Visible = false;
                }

                // Remove dark 3D sunken border from MdiClient workspace using Win32 API
                RemoveMdiClientBorder();

                if (pnlCustomStatusBar != null)
                {
                    this.Controls.Remove(pnlCustomStatusBar);
                    pnlCustomStatusBar.Dispose();
                }

                pnlCustomStatusBar = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 22,
                    Padding = new Padding(4, 1, 6, 1)
                };

                pnlCustomStatusBar.Paint += (s, e) =>
                {
                    Rectangle rect = pnlCustomStatusBar.ClientRectangle;
                    if (rect.Width <= 0 || rect.Height <= 0) return;

                    using (LinearGradientBrush b = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(195, 235, 255),
                        Color.FromArgb(150, 210, 250),
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(b, rect);
                    }

                    // Soft sky-blue top line
                    using (Pen p = new Pen(Color.FromArgb(135, 198, 242), 1))
                    {
                        e.Graphics.DrawLine(p, 0, 0, pnlCustomStatusBar.Width, 0);
                    }
                };

                Font labelFont = new Font("Segoe UI", 8.25F, FontStyle.Regular);
                Font boxFont = new Font("Segoe UI", 8.25F, FontStyle.Regular);
                Color darkTextColor = Color.FromArgb(0, 30, 80);
                Color borderColor = Color.FromArgb(0, 145, 230);

                // Left Flow Panel
                FlowLayoutPanel leftFlow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Left,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BackColor = Color.Transparent,
                    WrapContents = false,
                    Margin = new Padding(0)
                };

                // 1. User
                Label lblUserHead = new Label
                {
                    Text = "User",
                    Font = labelFont,
                    ForeColor = darkTextColor,
                    AutoSize = true,
                    Margin = new Padding(2, 2, 2, 0)
                };

                Panel pnlUserBox = CreateStatusBox(out lblUserVal, 68, 17, borderColor, boxFont, darkTextColor);
                string currentUser = !string.IsNullOrWhiteSpace(SessionContext.UserName) 
                    ? SessionContext.UserName 
                    : (!string.IsNullOrWhiteSpace(DataBase.UserName) ? DataBase.UserName : "admin2");
                lblUserVal.Text = currentUser;

                // 2. Business Date (Textbox look without combobox dropdown arrow)
                Label lblDateHead = new Label
                {
                    Text = "Business Date",
                    Font = labelFont,
                    ForeColor = darkTextColor,
                    AutoSize = true,
                    Margin = new Padding(10, 2, 2, 0)
                };

                Panel pnlDateBox = CreateStatusBox(out lblDateVal, 85, 17, borderColor, boxFont, darkTextColor);
                lblDateVal.Text = DateTime.Now.ToString("dd-MMM-yyyy");

                leftFlow.Controls.Add(lblUserHead);
                leftFlow.Controls.Add(pnlUserBox);
                leftFlow.Controls.Add(lblDateHead);
                leftFlow.Controls.Add(pnlDateBox);

                // Right Flow Panel
                FlowLayoutPanel rightFlow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Right,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BackColor = Color.Transparent,
                    WrapContents = false,
                    Margin = new Padding(0)
                };

                // 3. SQL File Size (Accurate dynamic indicator box matching Image 2)
                Label lblSqlHead = new Label
                {
                    Text = "SQL File Size",
                    Font = labelFont,
                    ForeColor = darkTextColor,
                    AutoSize = true,
                    Margin = new Padding(2, 2, 2, 0)
                };

                Panel pnlSqlBox = new Panel
                {
                    Size = new Size(40, 17),
                    BackColor = Color.White,
                    Margin = new Padding(2, 1, 2, 0)
                };

                string sqlSizeText = "0.0 MB";
                float fillRatio = GetSqlDatabaseFillRatio(out sqlSizeText);

                ToolTip ttSql = new ToolTip();
                ttSql.SetToolTip(pnlSqlBox, $"SQL Database Size: {sqlSizeText}");
                ttSql.SetToolTip(lblSqlHead, $"SQL Database Size: {sqlSizeText}");

                pnlSqlBox.Paint += (s, e) =>
                {
                    if (pnlSqlBox.Width <= 0 || pnlSqlBox.Height <= 0) return;

                    using (Pen p = new Pen(borderColor, 1))
                    {
                        e.Graphics.DrawRectangle(p, 0, 0, pnlSqlBox.Width - 1, pnlSqlBox.Height - 1);
                    }

                    // Accurate dynamic blue vertical indicator bar calculated from real database size
                    int maxBarWidth = pnlSqlBox.Width - 4;
                    if (maxBarWidth <= 0) return;
                    int barWidth = Math.Max(1, (int)(maxBarWidth * fillRatio));
                    int barHeight = Math.Max(1, pnlSqlBox.Height - 4);

                    Rectangle fillRect = new Rectangle(2, 2, barWidth, barHeight);
                    if (fillRect.Width > 0 && fillRect.Height > 0)
                    {
                        using (LinearGradientBrush gb = new LinearGradientBrush(fillRect, Color.FromArgb(0, 160, 235), Color.FromArgb(0, 120, 200), LinearGradientMode.Vertical))
                        {
                            e.Graphics.FillRectangle(gb, fillRect);
                        }
                    }
                };

                // 4. Version
                lblVersionVal = new Label
                {
                    Text = "Nexoris Version ( version 2.00.00)",
                    Font = labelFont,
                    ForeColor = darkTextColor,
                    AutoSize = true,
                    Margin = new Padding(12, 2, 4, 0)
                };

                rightFlow.Controls.Add(lblSqlHead);
                rightFlow.Controls.Add(pnlSqlBox);
                rightFlow.Controls.Add(lblVersionVal);

                pnlCustomStatusBar.Controls.Add(leftFlow);
                pnlCustomStatusBar.Controls.Add(rightFlow);

                // Insert into Controls collection in correct z-order to prevent workspace overflow
                int statusIndex = statusStrip != null ? this.Controls.IndexOf(statusStrip) : -1;
                if (statusIndex >= 0)
                {
                    this.Controls.Add(pnlCustomStatusBar);
                    this.Controls.SetChildIndex(pnlCustomStatusBar, statusIndex);
                }
                else
                {
                    this.Controls.Add(pnlCustomStatusBar);
                    pnlCustomStatusBar.SendToBack();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing custom status bar: {ex.Message}");
            }
        }

        private float GetSqlDatabaseFillRatio(out string sizeText)
        {
            sizeText = "0.0 MB";
            try
            {
                using (var baseRepo = new Repository.BaseRepostitory())
                using (var conn = (System.Data.SqlClient.SqlConnection)baseRepo.DataConnection)
                {
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();
                    string sql = "SELECT ISNULL(SUM(size * 8.0 / 1024), 0) FROM sys.database_files";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                    {
                        var val = cmd.ExecuteScalar();
                        if (val != null && val != DBNull.Value)
                        {
                            double dbSizeMB = Convert.ToDouble(val);
                            sizeText = $"{dbSizeMB:N1} MB";
                            double maxLimitMB = 10240.0; // SQL Express 10GB limit
                            float ratio = (float)(dbSizeMB / maxLimitMB);
                            return Math.Max(0.15f, Math.Min(1.0f, ratio));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error querying database size: {ex.Message}");
            }
            return 0.15f;
        }

        private Panel CreateStatusBox(out Label lblVal, int width, int height, Color borderColor, Font font, Color textColor)
        {
            Panel box = new Panel
            {
                Size = new Size(width, height),
                BackColor = Color.White,
                Margin = new Padding(2, 1, 2, 0)
            };

            box.Paint += (s, e) =>
            {
                using (Pen p = new Pen(borderColor, 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, box.Width - 1, box.Height - 1);
                }
            };

            lblVal = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = font,
                ForeColor = textColor,
                BackColor = Color.Transparent
            };

            box.Controls.Add(lblVal);
            return box;
        }

        #region Win32 MdiClient Border Removal

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_CLIENTEDGE = 0x00000200;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        private void RemoveMdiClientBorder()
        {
            try
            {
                foreach (Control c in this.Controls)
                {
                    MdiClient mdi = c as MdiClient;
                    if (mdi != null)
                    {
                        int style = GetWindowLong(mdi.Handle, GWL_EXSTYLE);
                        if ((style & WS_EX_CLIENTEDGE) != 0)
                        {
                            SetWindowLong(mdi.Handle, GWL_EXSTYLE, style & ~WS_EX_CLIENTEDGE);
                            SetWindowPos(mdi.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                        }
                    }
                }
            }
            catch { }
        }

        #endregion

        #endregion

        private void ultraTabSharedControlsPage1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
