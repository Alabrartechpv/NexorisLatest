using System;
using System.Collections.Generic;
using ModelClass.Settings;

namespace ModelClass
{
    /// <summary>
    /// Centralized session context management for the application.
    /// Stores user session information including company, branch, financial year, user details, and system information.
    /// </summary>
    public static class SessionContext
    {
        #region Private Fields

        private static int _companyId;
        private static int _branchId;
        private static int _finYearId;
        private static int _userId;
        private static int _counterId;
        private static string _systemMacId;
        private static bool _isInitialized;
        private static DateTime _loginTime;

        #endregion

        #region Core Session Properties

        /// <summary>
        /// Gets or sets the Company ID for the current session.
        /// </summary>
        public static int CompanyId
        {
            get => _companyId;
            set
            {
                _companyId = value;
                OnSessionPropertyChanged?.Invoke(nameof(CompanyId), value);
            }
        }

        /// <summary>
        /// Gets or sets the Branch ID for the current session.
        /// </summary>
        public static int BranchId
        {
            get => _branchId;
            set
            {
                _branchId = value;
                OnSessionPropertyChanged?.Invoke(nameof(BranchId), value);
            }
        }

        /// <summary>
        /// Gets or sets the Financial Year ID for the current session.
        /// </summary>
        public static int FinYearId
        {
            get => _finYearId;
            set
            {
                _finYearId = value;
                OnSessionPropertyChanged?.Invoke(nameof(FinYearId), value);
            }
        }

        /// <summary>
        /// Gets or sets the User ID for the current session.
        /// </summary>
        public static int UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnSessionPropertyChanged?.Invoke(nameof(UserId), value);
            }
        }

        /// <summary>
        /// Gets or sets the Counter ID (POS terminal/workstation ID) for the current session.
        /// </summary>
        public static int CounterId
        {
            get => _counterId;
            set
            {
                _counterId = value;
                OnSessionPropertyChanged?.Invoke(nameof(CounterId), value);
            }
        }

        /// <summary>
        /// Gets or sets the counter name assigned to the current workstation.
        /// </summary>
        public static string CounterName { get; set; }

        /// <summary>
        /// Gets or sets the database session id for the current cashier/counter login.
        /// </summary>
        public static long CounterSessionId { get; set; }

        /// <summary>
        /// Gets or sets the time this cashier session started.
        /// </summary>
        public static DateTime LoginTime
        {
            get => _loginTime;
            set
            {
                _loginTime = value;
                OnSessionPropertyChanged?.Invoke(nameof(LoginTime), value);
            }
        }

        /// <summary>
        /// Indicates that manual closing is required before more transactions are allowed.
        /// </summary>
        public static bool RequiresClosing { get; set; }

        /// <summary>
        /// Time of day when the system should start reminding the user to close.
        /// </summary>
        public static TimeSpan ClosingAlertTime { get; set; } = new TimeSpan(23, 30, 0);

        /// <summary>
        /// Gets or sets the System MAC ID for the current workstation.
        /// </summary>
        public static string SystemMacId
        {
            get => _systemMacId;
            set
            {
                _systemMacId = value;
                OnSessionPropertyChanged?.Invoke(nameof(SystemMacId), value);
            }
        }

        #endregion

        #region Additional Session Properties

        /// <summary>
        /// Gets or sets the User Name for the current session.
        /// </summary>
        public static string UserName { get; set; }

        /// <summary>
        /// Gets or sets the User Level/Role for the current session.
        /// </summary>
        public static string UserLevel { get; set; }

        /// <summary>
        /// Gets or sets the Email ID for the current user.
        /// </summary>
        public static string EmailId { get; set; }

        /// <summary>
        /// Gets or sets the Branch Name for the current session.
        /// </summary>
        public static string BranchName { get; set; }

        /// <summary>
        /// Gets or sets the Company Name for the current session.
        /// </summary>
        public static string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the Financial Year start date.
        /// </summary>
        public static DateTime? FinYearFrom { get; set; }

        /// <summary>
        /// Gets or sets the Financial Year end date.
        /// </summary>
        public static DateTime? FinYearTo { get; set; }

        /// <summary>
        /// Gets or sets the Financial Year status (e.g., "Active", "Closed").
        /// </summary>
        public static string FinYearStatus { get; set; }

        #endregion

        #region System Configuration

        /// <summary>
        /// Gets or sets the database connection status ("Online" or "Local").
        /// </summary>
        public static string Status { get; set; } = "Online";

        /// <summary>
        /// Gets or sets whether tax calculations are enabled.
        /// </summary>
        public static bool IsTaxEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the tax toggle message.
        /// </summary>
        public static string TaxToggleMessage { get; set; } = "Tax calculations are enabled";

        #endregion

        #region POS Settings Properties

        // Sales Settings
        /// <summary>
        /// How to handle scanning the same item: "MergeQuantity" or "SeparateRows"
        /// </summary>
        public static string DuplicateItemBehavior { get; set; } = "MergeQuantity";

        /// <summary>
        /// Allow selling items with negative stock
        /// </summary>
        public static bool AllowNegativeStock { get; set; } = false;

        /// <summary>
        /// Automatically print receipt after saving sale
        /// </summary>
        public static bool AutoPrintAfterSave { get; set; } = true;

        /// <summary>
        /// Default sale type for new sales (Cash, Credit)
        /// </summary>
        public static string DefaultSaleType { get; set; } = "Cash";

        /// <summary>
        /// Default price level for new sales (RetailPrice, WholesalePrice)
        /// </summary>
        public static string DefaultPriceLevel { get; set; } = "RetailPrice";

        /// <summary>
        /// Default credit term for credit sales (in days)
        /// </summary>
        public static int DefaultCreditDays { get; set; } = 30;

        /// <summary>
        /// Rounding method: None, Nearest5, Nearest10, Up, Down
        /// </summary>
        public static string RoundingMethod { get; set; } = "None";

        /// <summary>
        /// Maximum allowed discount percentage (0-100)
        /// </summary>
        public static int MaxDiscountPercent { get; set; } = 100;

        /// <summary>
        /// Allow selling items below cost price
        /// </summary>
        public static bool AllowSaleBelowCost { get; set; } = false;

        // Display Settings
        /// <summary>
        /// Show cost column to regular users
        /// </summary>
        public static bool ShowCostToUser { get; set; } = false;

        /// <summary>
        /// Show margin percentage column
        /// </summary>
        public static bool ShowMarginColumn { get; set; } = false;

        // Printing Settings
        /// <summary>
        /// Number of receipt copies to print
        /// </summary>
        public static int PrintCopies { get; set; } = 1;

        /// <summary>
        /// Whether settings have been loaded from database
        /// </summary>
        public static bool SettingsLoaded { get; private set; } = false;

        #endregion

        #region Session State

        /// <summary>
        /// Gets whether the session has been initialized with all required values.
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// Gets whether the session is valid (all required IDs are set).
        /// </summary>
        public static bool IsValid =>
            CompanyId > 0 &&
            BranchId > 0 &&
            FinYearId > 0 &&
            UserId > 0;

        #endregion

        #region Events

        /// <summary>
        /// Event raised when a session property changes.
        /// </summary>
        public static event Action<string, object> OnSessionPropertyChanged;

        /// <summary>
        /// Event raised when the session is initialized.
        /// </summary>
        public static event Action OnSessionInitialized;

        /// <summary>
        /// Event raised when the session is cleared.
        /// </summary>
        public static event Action OnSessionCleared;

        #endregion

        #region Initialization Methods

        /// <summary>
        /// Initializes the session context with all required values.
        /// </summary>
        public static void Initialize(
            int companyId,
            int branchId,
            int finYearId,
            int userId,
            int counterId = 0,
            string systemMacId = null)
        {
            if (companyId <= 0)
                throw new ArgumentException("Company ID must be greater than 0", nameof(companyId));
            if (branchId <= 0)
                throw new ArgumentException("Branch ID must be greater than 0", nameof(branchId));
            if (finYearId <= 0)
                throw new ArgumentException("Financial Year ID must be greater than 0", nameof(finYearId));
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than 0", nameof(userId));

            CompanyId = companyId;
            BranchId = branchId;
            FinYearId = finYearId;
            UserId = userId;
            CounterId = counterId;
            SystemMacId = systemMacId;
            LoginTime = DateTime.Now;
            RequiresClosing = false;

            _isInitialized = true;
            OnSessionInitialized?.Invoke();
        }

        /// <summary>
        /// Initializes the session context from login response data.
        /// </summary>
        public static void InitializeFromLogin(
            int companyId,
            int branchId,
            int finYearId,
            int userId,
            string userName,
            string userLevel,
            string emailId,
            string branchName,
            string companyName = null,
            int counterId = 0,
            string counterName = null,
            string systemMacId = null)
        {
            Initialize(companyId, branchId, finYearId, userId, counterId, systemMacId);

            UserName = userName;
            UserLevel = userLevel;
            EmailId = emailId;
            BranchName = branchName;
            CompanyName = companyName;
            CounterName = counterName;
        }

        /// <summary>
        /// Clears all session data.
        /// </summary>
        public static void Clear()
        {
            _companyId = 0;
            _branchId = 0;
            _finYearId = 0;
            _userId = 0;
            _counterId = 0;
            _systemMacId = null;
            _isInitialized = false;

            UserName = null;
            UserLevel = null;
            EmailId = null;
            BranchName = null;
            CompanyName = null;
            CounterName = null;
            CounterSessionId = 0;
            LoginTime = DateTime.MinValue;
            RequiresClosing = false;
            FinYearFrom = null;
            FinYearTo = null;
            FinYearStatus = null;

            ClearPermissions();
            OnSessionCleared?.Invoke();
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates that the session is initialized and all required values are set.
        /// </summary>
        public static void ValidateSession()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Session has not been initialized. Please log in first.");

            if (CompanyId <= 0)
                throw new InvalidOperationException("Company ID is not set.");
            if (BranchId <= 0)
                throw new InvalidOperationException("Branch ID is not set.");
            if (FinYearId <= 0)
                throw new InvalidOperationException("Financial Year ID is not set.");
            if (UserId <= 0)
                throw new InvalidOperationException("User ID is not set.");
        }

        public static bool CanDoTransaction(out string errorMessage)
        {
            if (!TryValidateSession(out errorMessage))
                return false;

            if (CounterId <= 0)
            {
                errorMessage = "Counter is not configured for this computer.";
                return false;
            }

            if (LoginTime == DateTime.MinValue)
                LoginTime = DateTime.Now;

            bool isBillingUser = UserLevel?.Equals("Cashier", StringComparison.OrdinalIgnoreCase) == true ||
                                 UserLevel?.Equals("Sales Man", StringComparison.OrdinalIgnoreCase) == true;

            if (!isBillingUser)
            {
                errorMessage = null;
                return true;
            }

            if (RequiresClosing || DateTime.Now.Date > LoginTime.Date)
            {
                RequiresClosing = true;
                errorMessage = "Please complete shift closing before continuing transactions.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        public static bool ShouldShowClosingAlert(DateTime now)
        {
            if (!IsInitialized || RequiresClosing)
                return false;

            if (LoginTime == DateTime.MinValue)
                LoginTime = now;

            return now.Date == LoginTime.Date && now.TimeOfDay >= ClosingAlertTime;
        }

        /// <summary>
        /// Checks if the session is valid without throwing exceptions.
        /// </summary>
        public static bool TryValidateSession(out string errorMessage)
        {
            errorMessage = null;

            if (!IsInitialized)
            {
                errorMessage = "Session has not been initialized. Please log in first.";
                return false;
            }

            if (CompanyId <= 0)
            {
                errorMessage = "Company ID is not set.";
                return false;
            }

            if (BranchId <= 0)
            {
                errorMessage = "Branch ID is not set.";
                return false;
            }

            if (FinYearId <= 0)
            {
                errorMessage = "Financial Year ID is not set.";
                return false;
            }

            if (UserId <= 0)
            {
                errorMessage = "User ID is not set.";
                return false;
            }

            return true;
        }

        #endregion

        #region Settings Loading Methods

        /// <summary>
        /// Loads POS settings from a list of POSSetting objects.
        /// </summary>
        public static void LoadSettings(List<POSSetting> settings)
        {
            try
            {
                if (settings == null || settings.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No POS settings to load, using defaults.");
                    return;
                }

                foreach (var setting in settings)
                {
                    ApplySetting(setting.SettingKey, setting.SettingValue, setting.SettingType);
                }

                SettingsLoaded = true;
                System.Diagnostics.Debug.WriteLine($"Loaded {settings.Count} POS settings.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying POS settings: {ex.Message}");
            }
        }

        private static bool ParseBoolean(string value, bool defaultValue = false)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;
            string trimmed = value.Trim().ToLowerInvariant();
            if (trimmed == "true" || trimmed == "1" || trimmed == "yes" || trimmed == "y")
                return true;
            if (trimmed == "false" || trimmed == "0" || trimmed == "no" || trimmed == "n")
                return false;
            return defaultValue;
        }

        private static void ApplySetting(string key, string value, string type)
        {
            if (string.IsNullOrEmpty(key)) return;

            string cleanKey = key.Trim();
            string cleanValue = value?.Trim();

            switch (cleanKey)
            {
                case "DuplicateItemBehavior":
                    DuplicateItemBehavior = cleanValue ?? "MergeQuantity";
                    break;
                case "AllowNegativeStock":
                    AllowNegativeStock = ParseBoolean(cleanValue, false);
                    break;
                case "AutoPrintAfterSave":
                    AutoPrintAfterSave = ParseBoolean(cleanValue, true);
                    break;
                case "DefaultSaleType":
                    DefaultSaleType = cleanValue ?? "Cash";
                    break;
                case "DefaultPriceLevel":
                    DefaultPriceLevel = cleanValue ?? "RetailPrice";
                    break;
                case "DefaultCreditDays":
                    DefaultCreditDays = int.TryParse(cleanValue, out var defaultCreditDays) ? defaultCreditDays : 30;
                    break;
                case "RoundingMethod":
                    RoundingMethod = cleanValue ?? "None";
                    break;
                case "MaxDiscountPercent":
                    MaxDiscountPercent = int.TryParse(cleanValue, out var maxDisc) ? maxDisc : 100;
                    break;
                case "AllowSaleBelowCost":
                    AllowSaleBelowCost = ParseBoolean(cleanValue, false);
                    break;
                case "ShowCostToUser":
                    ShowCostToUser = ParseBoolean(cleanValue, false);
                    break;
                case "ShowMarginColumn":
                    ShowMarginColumn = ParseBoolean(cleanValue, false);
                    break;
                case "PrintCopies":
                    PrintCopies = int.TryParse(cleanValue, out var copies) ? copies : 1;
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"Unknown setting key: {key}");
                    break;
            }
        }

        public static void MarkSettingsForRefresh()
        {
            SettingsLoaded = false;
        }

        public static void ResetSettingsToDefaults()
        {
            DuplicateItemBehavior = "MergeQuantity";
            AllowNegativeStock = false;
            AutoPrintAfterSave = true;
            DefaultSaleType = "Cash";
            DefaultPriceLevel = "RetailPrice";
            DefaultCreditDays = 30;
            RoundingMethod = "None";
            MaxDiscountPercent = 100;
            AllowSaleBelowCost = false;
            ShowCostToUser = false;
            ShowMarginColumn = false;
            PrintCopies = 1;
            SettingsLoaded = false;
        }

        #endregion

        #region Helper Methods

        public static string GetSessionSummary()
        {
            return $"Session Context:\n" +
                   $"  Company ID: {CompanyId} ({CompanyName ?? "N/A"})\n" +
                   $"  Branch ID: {BranchId} ({BranchName ?? "N/A"})\n" +
                   $"  Financial Year ID: {FinYearId} ({FinYearFrom?.ToString("yyyy-MM-dd") ?? "N/A"} to {FinYearTo?.ToString("yyyy-MM-dd") ?? "N/A"})\n" +
                   $"  User ID: {UserId} ({UserName ?? "N/A"})\n" +
                   $"  User Level: {UserLevel ?? "N/A"}\n" +
                   $"  Counter ID: {CounterId} ({CounterName ?? "N/A"})\n" +
                   $"  Counter Session ID: {CounterSessionId}\n" +
                   $"  Login Time: {(LoginTime == DateTime.MinValue ? "N/A" : LoginTime.ToString("yyyy-MM-dd HH:mm:ss"))}\n" +
                   $"  Requires Closing: {RequiresClosing}\n" +
                   $"  System MAC ID: {SystemMacId ?? "N/A"}\n" +
                   $"  Status: {Status}\n" +
                   $"  Initialized: {IsInitialized}\n" +
                   $"  Valid: {IsValid}";
        }

        #endregion

        #region Permission Management

        /// <summary>
        /// Dictionary storing permissions by FormKey for fast lookup
        /// </summary>
        private static Dictionary<string, Master.RolePermission> _permissions = new Dictionary<string, Master.RolePermission>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the current user's Role ID
        /// </summary>
        public static int RoleId { get; set; }

        /// <summary>
        /// Gets whether permissions have been loaded
        /// </summary>
        public static bool PermissionsLoaded => _permissions.Count > 0;

        /// <summary>
        /// Normalizes any form key, ribbon key, or display name by stripping prefixes ("frm"),
        /// punctuation, spaces, and converting to lower-case.
        /// </summary>
        public static string NormalizeKey(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return string.Empty;
            string s = val.Trim().ToLowerInvariant()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace("/", "")
                .Replace("&", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(".", "")
                .Replace("•", "");

            if (s.StartsWith("frm") && s.Length > 3)
            {
                s = s.Substring(3);
            }
            return s;
        }

        private static void SetOrMergePermission(string key, Master.RolePermission p)
        {
            if (string.IsNullOrWhiteSpace(key) || p == null) return;
            string trimmed = key.Trim();
            if (_permissions.TryGetValue(trimmed, out var existing) && existing != null)
            {
                _permissions[trimmed] = new Master.RolePermission
                {
                    RoleID = p.RoleID > 0 ? p.RoleID : existing.RoleID,
                    FormID = p.CanView ? p.FormID : existing.FormID,
                    FormKey = p.CanView ? p.FormKey : existing.FormKey,
                    FormName = p.CanView ? p.FormName : existing.FormName,
                    Category = p.CanView ? p.Category : existing.Category,
                    CanView = existing.CanView || p.CanView,
                    CanAdd = existing.CanAdd || p.CanAdd,
                    CanEdit = existing.CanEdit || p.CanEdit,
                    CanDelete = existing.CanDelete || p.CanDelete
                };
            }
            else
            {
                _permissions[trimmed] = new Master.RolePermission
                {
                    RoleID = p.RoleID,
                    FormID = p.FormID,
                    FormKey = p.FormKey,
                    FormName = p.FormName,
                    Category = p.Category,
                    CanView = p.CanView,
                    CanAdd = p.CanAdd,
                    CanEdit = p.CanEdit,
                    CanDelete = p.CanDelete
                };
            }
        }

        private static void RegisterAlias(string alias, Master.RolePermission p)
        {
            if (string.IsNullOrWhiteSpace(alias) || p == null) return;
            string trimmed = alias.Trim();
            SetOrMergePermission(trimmed, p);
            string norm = NormalizeKey(trimmed);
            if (!string.IsNullOrEmpty(norm))
            {
                SetOrMergePermission(norm, p);
            }
        }

        /// <summary>
        /// Loads permissions for the current user's role and populates canonical aliases
        /// </summary>
        /// <param name="permissions">Collection of role permissions to load</param>
        public static void LoadPermissions(IEnumerable<Master.RolePermission> permissions)
        {
            _permissions.Clear();
            if (permissions != null)
            {
                foreach (var p in permissions)
                {
                    if (p == null) continue;

                    string formKey = p.FormKey?.Trim() ?? "";
                    string formName = p.FormName?.Trim() ?? "";

                    if (!string.IsNullOrEmpty(formKey))
                    {
                        RegisterAlias(formKey, p);
                    }
                    if (!string.IsNullOrEmpty(formName))
                    {
                        RegisterAlias(formName, p);
                    }

                    string normKey = NormalizeKey(formKey);
                    string normName = NormalizeKey(formName);
                    string combined = normKey + " " + normName;

                    // 1. Executive Dashboard & KPIs
                    if (combined.Contains("executivedashboard") || combined.Contains("dashboard") || combined.Contains("kpi") || combined.Contains("executive"))
                    {
                        RegisterAlias("dashboard", p);
                        RegisterAlias("executivedashboard", p);
                        RegisterAlias("frmexecutivekpidashboard", p);
                        RegisterAlias("bossdashboard", p);
                        RegisterAlias("executivekpi", p);
                        RegisterAlias("executivedashboard(30kpis)", p);
                        RegisterAlias("executivedashboard&kpis", p);
                        RegisterAlias("executive dashboard", p);
                        RegisterAlias("executive dashboard & kpis", p);
                        RegisterAlias("kpi dashboard", p);
                        RegisterAlias("executive dashboard (30 kpis)", p);
                    }

                    // 2. Dashboard Overview
                    if (combined.Contains("overview"))
                    {
                        RegisterAlias("overview", p);
                        RegisterAlias("dashboardoverview", p);
                        RegisterAlias("frmdashboardoverview", p);
                        RegisterAlias("dashboard overview", p);
                    }

                    // 3. Business Summary / Final Analysis
                    if (combined.Contains("businesssummary") || combined.Contains("finalanalysis"))
                    {
                        RegisterAlias("businesssummary", p);
                        RegisterAlias("finalanalysis", p);
                        RegisterAlias("business summary", p);
                        RegisterAlias("frmfinalanalysis", p);
                    }

                    // 4. Smart Reorder Dashboard
                    if (combined.Contains("reorder") || combined.Contains("smartreorder"))
                    {
                        RegisterAlias("smartreorder", p);
                        RegisterAlias("reorder", p);
                        RegisterAlias("frmsmartreorderdashboard", p);
                        RegisterAlias("smart reorder dashboard", p);
                        RegisterAlias("smart reorder", p);
                    }

                    // 5. Item Master
                    if (normKey == "itemmaster" || normName.Contains("itemmaster") || normName == "item")
                    {
                        RegisterAlias("itemmaster", p);
                        RegisterAlias("frmitemmasternew", p);
                        RegisterAlias("frmitemmaster", p);
                        RegisterAlias("item master", p);
                        RegisterAlias("item", p);
                    }

                    // 6. Unit Master
                    if (normKey == "unit" || normKey == "unitmaster" || normName.Contains("unitmaster") || normName.Contains("unit"))
                    {
                        RegisterAlias("unit", p);
                        RegisterAlias("unitmaster", p);
                        RegisterAlias("frmunitmaster", p);
                        RegisterAlias("unit master", p);
                    }

                    // 7. Item Type Master
                    if (normKey == "itemtype" || normName.Contains("itemtype") || normName.Contains("item type"))
                    {
                        RegisterAlias("itemtype", p);
                        RegisterAlias("item type", p);
                        RegisterAlias("frmitemtype", p);
                        RegisterAlias("item type master", p);
                    }

                    // 8. General Paymode Setup
                    if (normKey.Contains("paymode") || normName.Contains("paymode"))
                    {
                        RegisterAlias("paymode", p);
                        RegisterAlias("paymodesetup", p);
                        RegisterAlias("general pm setup", p);
                        RegisterAlias("generalpmsetup", p);
                        RegisterAlias("paymodemaster", p);
                        RegisterAlias("frmgeneralpaymodesetup", p);
                    }

                    // 9. Tax Management
                    if (normKey.Contains("tax") || normName.Contains("tax"))
                    {
                        RegisterAlias("taxmanagement", p);
                        RegisterAlias("frmtaxmanagement", p);
                        RegisterAlias("tax management", p);
                        RegisterAlias("tax management & rates", p);
                    }

                    // 10. Sales Invoice / Sales
                    if (normKey == "sales" || normName == "sales invoice" || normName == "sales")
                    {
                        RegisterAlias("sales", p);
                        RegisterAlias("frmsalesinvoice", p);
                        RegisterAlias("sales invoice", p);
                        RegisterAlias("salesinvoice", p);
                    }

                    // 11. Sales Return
                    if ((normKey == "salesreturn" || normName == "salesreturn" || string.Equals(formName, "Sales Return", StringComparison.OrdinalIgnoreCase)) && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("salesreturn", p);
                        RegisterAlias("frmsalesreturn", p);
                        RegisterAlias("sales return", p);

                    }

                    // 12. Purchase
                    if (normKey == "purchase" || normName == "purchase invoice" || normName == "purchase entry" || normName == "purchase")
                    {
                        RegisterAlias("purchase", p);
                        RegisterAlias("frmpurchase", p);
                        RegisterAlias("purchase invoice", p);
                        RegisterAlias("purchase entry", p);
                        RegisterAlias("goods received", p);
                    }

                    // 13. Purchase Order
                    if (normKey == "purchaseorder" || normName.Contains("purchase order"))
                    {
                        RegisterAlias("purchaseorder", p);
                        RegisterAlias("frmpurchaseorder", p);
                        RegisterAlias("purchase order", p);
                    }

                    // 14. Purchase Return
                    if ((normKey == "purchasereturn" || normName == "purchasereturn" || string.Equals(formName, "Purchase Return", StringComparison.OrdinalIgnoreCase)) && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("purchasereturn", p);
                        RegisterAlias("frmpurchasereturn", p);
                        RegisterAlias("purchase return", p);
                        RegisterAlias("purchase r/n", p);
                        RegisterAlias("purchasereturnwithoutgr", p);
                    }

                    // 15. POS Terminal
                    if (normKey == "pos" || normName.Contains("pos"))
                    {
                        RegisterAlias("pos", p);
                        RegisterAlias("frmpos", p);
                        RegisterAlias("pos terminal", p);
                    }

                    // 16. GRN (Goods Received Notes)
                    if (normKey == "grn" || normName.Contains("grn") || normName.Contains("goods received"))
                    {
                        RegisterAlias("grn", p);
                        RegisterAlias("goods received", p);
                        RegisterAlias("goods received notes", p);
                        RegisterAlias("goods received notes (grn)", p);
                        RegisterAlias("grnreport", p);
                    }

                    // 17. Stock Adjustment
                    if (normKey == "stockadjustment" || normName.Contains("stock adjustment"))
                    {
                        RegisterAlias("stockadjustment", p);
                        RegisterAlias("frmstockadjustment", p);
                        RegisterAlias("stock adjustment", p);
                    }

                    // 18. Stock Transfer
                    if (normKey == "stocktransfer" || normName.Contains("stock transfer"))
                    {
                        RegisterAlias("stocktransfer", p);
                        RegisterAlias("frmstocktransfer", p);
                        RegisterAlias("stock transfer", p);
                    }

                    // 19. Users & Security Setup
                    if (normKey == "users" || normName.Contains("user"))
                    {
                        RegisterAlias("users", p);
                        RegisterAlias("frmusers", p);
                        RegisterAlias("users & accounts", p);
                        RegisterAlias("users & security setup", p);
                    }

                    // 20. Role Permissions
                    if (normKey.Contains("role") || normName.Contains("role"))
                    {
                        RegisterAlias("rolepermissions", p);
                        RegisterAlias("roles", p);
                        RegisterAlias("frmrolepermissions", p);
                        RegisterAlias("role permissions", p);
                        RegisterAlias("role permissions management", p);
                    }

                    // 21. Sales Reports & Summaries
                    if (normKey.Contains("salesreport") || normName.Contains("sales summary") || normName.Contains("daily sales"))
                    {
                        RegisterAlias("salesreport", p);
                        RegisterAlias("salessummary", p);
                        RegisterAlias("salessummaryreport", p);
                        RegisterAlias("dsales", p);
                        RegisterAlias("frmsales_rpt", p);
                        RegisterAlias("daily sales", p);
                        RegisterAlias("daily sales report", p);
                        RegisterAlias("sales summary report", p);
                    }
                    if (normKey.Contains("salesdetails") || normName.Contains("sales details"))
                    {
                        RegisterAlias("salesdetails", p);
                        RegisterAlias("sales details", p);
                        RegisterAlias("salesdetailsreport", p);
                        RegisterAlias("sales details report", p);
                        RegisterAlias("frmsalesreportmasterdetail", p);
                    }
                    if (normKey.Contains("salesprofit") || normName.Contains("sales profit"))
                    {
                        RegisterAlias("salesprofit", p);
                        RegisterAlias("sales profit", p);
                        RegisterAlias("salesprofitreport", p);
                        RegisterAlias("sales profit report", p);
                        RegisterAlias("frmsalesprofit", p);
                    }
                    if (normKey.Contains("salesaudit") || normName.Contains("sales audit"))
                    {
                        RegisterAlias("salesauditreport", p);
                        RegisterAlias("sales audit report", p);
                        RegisterAlias("salesaudit", p);
                        RegisterAlias("frmsalesauditreport", p);
                    }
                    if (normKey.Contains("salesreturnreport") || normName.Contains("salesreturnreport") ||
                        (normKey.Contains("salesreturn") && normKey.Contains("report")) ||
                        (normName.Contains("salesreturn") && normName.Contains("report")) ||
                        formName.IndexOf("Sales Return Report", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        RegisterAlias("salesreturnreport", p);
                        RegisterAlias("sales return report", p);
                        RegisterAlias("frmsalesreturnreport", p);
                    }
                    if (normKey.Contains("salesreturnaudit") || normName.Contains("sales return audit"))
                    {
                        RegisterAlias("salesreturnauditreport", p);
                        RegisterAlias("sales return audit report", p);
                        RegisterAlias("salesreturnaudit", p);
                        RegisterAlias("frmsalesreturnauditreport", p);
                    }
                    if (normKey.Contains("salesmanincentive") || normName.Contains("salesman incentive"))
                    {
                        RegisterAlias("salesmanincentive", p);
                        RegisterAlias("salesman incentive", p);
                        RegisterAlias("salesmanincentivereport", p);
                        RegisterAlias("salesman incentive report", p);
                        RegisterAlias("frmsalesmanincentivereport", p);
                    }
                    if (normKey.Contains("customerwisesales") || normName.Contains("customer-wise sales") || normName.Contains("customerwise sales"))
                    {
                        RegisterAlias("customerwisesalessummary", p);
                        RegisterAlias("customerwisesalessummaryreport", p);
                        RegisterAlias("customer-wise sales summary", p);
                        RegisterAlias("frmcustomerwisesalessummaryreport", p);
                    }
                    if (normKey.Contains("salesmanwisesales") || normName.Contains("salesman-wise sales") || normName.Contains("salesmanwise sales"))
                    {
                        RegisterAlias("salesmanwisesalessummary", p);
                        RegisterAlias("salesmanwisesalessummaryreport", p);
                        RegisterAlias("salesman-wise sales summary", p);
                        RegisterAlias("frmsalesmanwisesalessummaryreport", p);
                    }
                    if (normKey.Contains("itemwisesales") || normName.Contains("item-wise sales") || normName.Contains("itemwise sales"))
                    {
                        RegisterAlias("itemwisesalessummary", p);
                        RegisterAlias("itemwisesalessummaryreport", p);
                        RegisterAlias("item-wise sales summary", p);
                        RegisterAlias("frmitemwisesalessummaryreport", p);
                    }
                    if (normKey.Contains("saleshold") || normName.Contains("sales hold"))
                    {
                        RegisterAlias("salesholdreport", p);
                        RegisterAlias("sales hold report", p);
                        RegisterAlias("tool_salesholdreport", p);
                        RegisterAlias("frmsalesholdreport", p);
                    }
                    if (normKey.Contains("counterreport") || normName.Contains("counter sales") || normName.Contains("counter report"))
                    {
                        RegisterAlias("counterreport", p);
                        RegisterAlias("counter report", p);
                        RegisterAlias("counter sales report", p);
                        RegisterAlias("frmcounterreport", p);
                    }

                    // 22. Purchase Reports
                    if (normKey.Contains("purchasereport") || normName.Contains("purchase summary"))
                    {
                        RegisterAlias("purchasereport", p);
                        RegisterAlias("purchase report", p);
                        RegisterAlias("purchasesummary", p);
                        RegisterAlias("purchase summary report", p);
                        RegisterAlias("frmpurchasereport", p);
                    }
                    if (normKey.Contains("purchasedetails") || normName.Contains("purchase details"))
                    {
                        RegisterAlias("purchasedetails", p);
                        RegisterAlias("purchase details", p);
                        RegisterAlias("purchasedetailsreport", p);
                        RegisterAlias("purchase details report", p);
                        RegisterAlias("frmpurchasereportdetails", p);
                    }
                    if (normKey.Contains("purchasereturnreport") || normName.Contains("purchasereturnreport") ||
                        (normKey.Contains("purchasereturn") && normKey.Contains("report")) ||
                        (normName.Contains("purchasereturn") && normName.Contains("report")) ||
                        formName.IndexOf("Purchase Return Report", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        RegisterAlias("purchasereturnreport", p);
                        RegisterAlias("purchase return report", p);
                        RegisterAlias("frmpurchasereturnreport", p);
                    }
                    if (normKey.Contains("vendorpurchasereport") || normName.Contains("vendor purchase"))
                    {
                        RegisterAlias("vendorpurchasereport", p);
                        RegisterAlias("vendor purchase report", p);
                        RegisterAlias("frmvendorpurchasereport", p);
                    }
                    if (normKey.Contains("purchaseaudit") || normName.Contains("purchase audit"))
                    {
                        RegisterAlias("purchaseauditreport", p);
                        RegisterAlias("purchase audit report", p);
                        RegisterAlias("frmpurchaseauditreport", p);
                    }
                    if (normKey.Contains("purchasereturnaudit") || normName.Contains("purchase return audit"))
                    {
                        RegisterAlias("purchasereturnauditreport", p);
                        RegisterAlias("purchase return audit report", p);
                    }

                    // 23. Stock / Inventory Reports
                    if (normKey.Contains("stockreport") || normName.Contains("stock report") || normName.Contains("stock listing"))
                    {
                        RegisterAlias("stockreport", p);
                        RegisterAlias("stock report", p);
                        RegisterAlias("stocklisting", p);
                        RegisterAlias("stocklistingreport", p);
                        RegisterAlias("stock listing", p);
                        RegisterAlias("stock listing report", p);
                        RegisterAlias("frmstockreport", p);
                    }
                    if (normKey.Contains("stockreportadv") || normName.Contains("advanced stock report"))
                    {
                        RegisterAlias("stockreportadv", p);
                        RegisterAlias("stockreportadvanced", p);
                        RegisterAlias("advanced stock report", p);
                        RegisterAlias("stock report advanced", p);
                        RegisterAlias("frmstockreportadvanced", p);
                    }
                    if (normKey.Contains("stockvaluation") || normName.Contains("stock valuation"))
                    {
                        RegisterAlias("stockvaluation", p);
                        RegisterAlias("stock valuation", p);
                        RegisterAlias("stockvaluationreport", p);
                        RegisterAlias("stock valuation report", p);
                        RegisterAlias("frmstockvaluationreport", p);
                    }
                    if (normKey.Contains("stockadjustmentreport") || normName.Contains("stock adjustment report"))
                    {
                        RegisterAlias("stockadjustmentreport", p);
                        RegisterAlias("stock adjustment report", p);
                        RegisterAlias("frmstockadjustmentreport", p);
                    }
                    if (normKey.Contains("stockadjustmentaudit") || normName.Contains("stock adjustment audit"))
                    {
                        RegisterAlias("stockadjustmentauditreport", p);
                        RegisterAlias("stock adjustment audit report", p);
                        RegisterAlias("stockadjustmentaudit", p);
                    }
                    if (normKey.Contains("lowstock") || normName.Contains("low stock"))
                    {
                        RegisterAlias("lowstockalert", p);
                        RegisterAlias("low stock alert", p);
                        RegisterAlias("lowstockalertreport", p);
                        RegisterAlias("low stock alert report", p);
                        RegisterAlias("frmlowstockalertreport", p);
                    }
                    if (normKey.Contains("inactiveitem") || normName.Contains("inactive item"))
                    {
                        RegisterAlias("inactiveitemsreport", p);
                        RegisterAlias("inactive items report", p);
                        RegisterAlias("frminactiveitemsreport", p);
                    }
                    if (normKey == "itemreport" || normName.Contains("item report"))
                    {
                        RegisterAlias("itemreport", p);
                        RegisterAlias("item report", p);
                        RegisterAlias("item report & details", p);
                        RegisterAlias("frmitemreport", p);
                    }
                    if (normKey.Contains("audittrail") || normKey.Contains("stockaudit") || normName.Contains("inventory audit trail") || normName.Contains("stock audit"))
                    {
                        RegisterAlias("audittrail", p);
                        RegisterAlias("audit trail", p);
                        RegisterAlias("stockaudit", p);
                        RegisterAlias("stock audit report", p);
                        RegisterAlias("inventory audit trail", p);
                        RegisterAlias("frmauditreport", p);
                    }

                    // 24. Accounts & Financial Reports
                    if (normKey.Contains("tradingpl") || normName.Contains("trading") || normName.Contains("trading account"))
                    {
                        RegisterAlias("tradingpl", p);
                        RegisterAlias("tradingplaccount", p);
                        RegisterAlias("tradingaccount", p);
                        RegisterAlias("trading account", p);
                        RegisterAlias("trading & profit / loss account", p);
                        RegisterAlias("frmtradingplaccount", p);
                    }
                    if (normKey.Contains("profitloss") || normName.Contains("profit and loss") || normName.Contains("profit & loss"))
                    {
                        RegisterAlias("profitloss", p);
                        RegisterAlias("profitlossaccount", p);
                        RegisterAlias("profit & loss account", p);
                        RegisterAlias("profit and loss account", p);
                        RegisterAlias("frmprofitlossaccount", p);
                    }
                    if (normKey.Contains("balancesheet") || normName.Contains("balance sheet"))
                    {
                        RegisterAlias("balancesheet", p);
                        RegisterAlias("balance sheet", p);
                        RegisterAlias("balancesheetreport", p);
                        RegisterAlias("frmbalancesheet", p);
                    }
                    if (normKey.Contains("trialbalance") || normName.Contains("trial balance"))
                    {
                        RegisterAlias("trialbalance", p);
                        RegisterAlias("trial balance", p);
                        RegisterAlias("trialbalancereport", p);
                        RegisterAlias("frmtrialbalance", p);
                    }
                    if (normKey.Contains("cashbankbook") || normName.Contains("cash & bank book") || normName.Contains("cash and bank"))
                    {
                        RegisterAlias("cashbankbook", p);
                        RegisterAlias("cash & bank book", p);
                        RegisterAlias("cash and bank book", p);
                        RegisterAlias("frmcashbankbook", p);
                    }
                    if (normKey.Contains("daybook") || normName.Contains("day book"))
                    {
                        RegisterAlias("daybook", p);
                        RegisterAlias("day book", p);
                        RegisterAlias("daybookreport", p);
                        RegisterAlias("frmdaybook", p);
                    }
                    if (normKey.Contains("cashinout") || normName.Contains("cash in / out") || normName.Contains("cash in / cash out"))
                    {
                        RegisterAlias("cashinout", p);
                        RegisterAlias("cash in / out report", p);
                        RegisterAlias("cash in / cash out", p);
                        RegisterAlias("cashinoutreport", p);
                        RegisterAlias("frmcashinoutreport", p);
                    }
                    if (normKey.Contains("bankstatement") || normName.Contains("bank statement"))
                    {
                        RegisterAlias("bankstatement", p);
                        RegisterAlias("bank statement", p);
                        RegisterAlias("bankstatementreport", p);
                        RegisterAlias("bank statement report", p);
                        RegisterAlias("frmbankstatementreport", p);
                    }
                    if (normKey.Contains("shiftreconciliation") || normName.Contains("shift reconciliation") || normName.Contains("shift report") || normName.Contains("counter closing report"))
                    {
                        RegisterAlias("shiftreconciliation", p);
                        RegisterAlias("shift reconciliation", p);
                        RegisterAlias("shiftreconciliationreport", p);
                        RegisterAlias("shift report", p);
                        RegisterAlias("counter closing report", p);
                        RegisterAlias("frmshiftreconciliationreport", p);
                    }
                    if (normKey.Contains("customeroutstanding") || normName.Contains("customer outstanding"))
                    {
                        RegisterAlias("customeroutstanding", p);
                        RegisterAlias("customer outstanding", p);
                        RegisterAlias("customeroutstandingreport", p);
                        RegisterAlias("customer outstanding listing", p);
                        RegisterAlias("frmcustomeroutstandingreport", p);
                    }
                    if (normKey.Contains("customerreceiptreport") || normName.Contains("customer receipt report"))
                    {
                        RegisterAlias("customerreceiptreport", p);
                        RegisterAlias("customer receipt report", p);
                        RegisterAlias("frmcustomerreceiptreport", p);
                    }
                    if (normKey.Contains("customerledger") || normName.Contains("customer ledger"))
                    {
                        RegisterAlias("customerledger", p);
                        RegisterAlias("customer ledger", p);
                        RegisterAlias("customerledgerreport", p);
                        RegisterAlias("customer ledger statement", p);
                        RegisterAlias("frmcustomerledgerreport", p);
                    }
                    if (normKey.Contains("vendoroutstanding") || normName.Contains("vendor outstanding"))
                    {
                        RegisterAlias("vendoroutstanding", p);
                        RegisterAlias("vendor outstanding", p);
                        RegisterAlias("vendoroutstandingreport", p);
                        RegisterAlias("vendor outstanding listing", p);
                        RegisterAlias("unallocatedpurchasereturns", p);
                        RegisterAlias("unallocated purchase returns", p);
                        RegisterAlias("frmvendoroutstandingreport", p);
                    }
                    if (normKey.Contains("vendorpaymentreport") || normName.Contains("vendor payment report") || normKey.Contains("vendordn"))
                    {
                        RegisterAlias("vendorpaymentreport", p);
                        RegisterAlias("vendor payment report", p);
                        RegisterAlias("vendordnpaymentreport", p);
                        RegisterAlias("frmvendorpaymentreport", p);
                    }
                    if ((normKey == "manualpartybalance" || normKey == "manualpartybalanceentry" || normName == "manual party balance entry" || normName == "manual party balance" || string.Equals(formKey, "manualpartybalance", StringComparison.OrdinalIgnoreCase)) && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("manualpartybalance", p);
                        RegisterAlias("manual party balance", p);
                        RegisterAlias("manual party balance entry", p);
                        RegisterAlias("manualpartybalanceentry", p);
                        RegisterAlias("frmmanualpartybalance", p);
                    }
                    if (normKey.Contains("manualpartybalancereport") || (normKey.Contains("manualpartybalance") && normKey.Contains("report")) || (normName.Contains("manual party balance") && normName.Contains("report")))
                    {
                        RegisterAlias("manualpartybalancereport", p);
                        RegisterAlias("manual party balance report", p);
                        RegisterAlias("frmmanualpartybalancereport", p);
                    }
                    if (false)
                    {

                    }
                    if (normKey.Contains("combinedpartybalance") || normName.Contains("combined party balance"))
                    {
                        RegisterAlias("combinedpartybalance", p);
                        RegisterAlias("combinedpartybalancereport", p);
                        RegisterAlias("combined party balance", p);
                        RegisterAlias("combined party balance report", p);
                        RegisterAlias("frmcombinedpartybalancereport", p);
                    }
                    if (normKey.Contains("inputgst") || normName.Contains("input gst"))
                    {
                        RegisterAlias("inputgst", p);
                        RegisterAlias("inputgstreport", p);
                        RegisterAlias("input gst report", p);
                        RegisterAlias("input gst & itc report", p);
                        RegisterAlias("frminputgstreport", p);
                    }
                    if (normKey.Contains("outputgst") || normName.Contains("output gst"))
                    {
                        RegisterAlias("outputgst", p);
                        RegisterAlias("outputgstreport", p);
                        RegisterAlias("output gst report", p);
                        RegisterAlias("frmoutputgstreport", p);
                    }
                    if (normKey.Contains("govtgst") || normName.Contains("government gst"))
                    {
                        RegisterAlias("govtgst", p);
                        RegisterAlias("govtgstreturnreport", p);
                        RegisterAlias("government gst return report", p);
                        RegisterAlias("frmgovtgstreturnreport", p);
                    }

                    // 24b. Accounts Entry Forms & Vouchers
                    if ((normKey == "generalpayment" || normName.Contains("general payment")) && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("generalpayment", p);
                        RegisterAlias("general payment", p);
                        RegisterAlias("frmgeneralpayment", p);
                        RegisterAlias("generalpaymentvoucher", p);
                        RegisterAlias("general payment voucher", p);
                    }
                    if ((normKey == "generalreceipt" || normName.Contains("general receipt")) && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("generalreceipt", p);
                        RegisterAlias("general receipt", p);
                        RegisterAlias("frmgeneralreceipt", p);
                        RegisterAlias("generalreceiptvoucher", p);
                        RegisterAlias("general receipt voucher", p);
                    }
                    if ((normKey == "journal" || normName.Contains("journal voucher")) && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("journal", p);
                        RegisterAlias("journalvoucher", p);
                        RegisterAlias("journal voucher", p);
                        RegisterAlias("frmjournal", p);
                    }
                    if ((normKey == "contra" || normName.Contains("contra voucher")) && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("contra", p);
                        RegisterAlias("contravoucher", p);
                        RegisterAlias("contra voucher", p);
                        RegisterAlias("frmcontra", p);
                    }
                    if ((normKey == "customer" || normName.Contains("customer management")) && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("customer", p);
                        RegisterAlias("customermanagement", p);
                        RegisterAlias("customer management", p);
                        RegisterAlias("frmcustomer", p);
                    }
                    if ((normKey == "vendor" || normName.Contains("vendor management")) && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("vendor", p);
                        RegisterAlias("frmvendor", p);
                        RegisterAlias("vendormanagement", p);
                        RegisterAlias("vendor management", p);
                    }
                    if ((normKey == "ledgers" || normKey == "ledger" || normKey == "accountledgers" || normName == "account ledgers" || normName == "ledger" || normName == "ledgers") && !normKey.Contains("customer") && !normKey.Contains("vendor") && !normName.Contains("customer") && !normName.Contains("vendor") && !normKey.Contains("report") && !normName.Contains("report") && !normName.Contains("statement") && !normName.Contains("summary"))
                    {
                        RegisterAlias("ledgers", p);
                        RegisterAlias("ledger", p);
                        RegisterAlias("account ledgers", p);
                        RegisterAlias("frmledgers", p);
                        RegisterAlias("frmledger", p);
                    }
                    if (normKey == "accountgroup" || normName.Contains("account group"))
                    {
                        RegisterAlias("accountgroup", p);
                        RegisterAlias("account group", p);
                        RegisterAlias("account group setup", p);
                        RegisterAlias("frmaccountgroup", p);
                    }
                    if (normKey == "chartofaccount" || normName.Contains("chart of account"))
                    {
                        RegisterAlias("chartofaccount", p);
                        RegisterAlias("chart of account", p);
                        RegisterAlias("chart of accounts", p);
                        RegisterAlias("frmchartofacc", p);
                        RegisterAlias("frmchartofaccount", p);
                    }
                    if ((normKey == "receipt" || normName == "customer receipt") && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("receipt", p);
                        RegisterAlias("customer receipt", p);
                        RegisterAlias("customerreceipt", p);
                        RegisterAlias("frmreceipt", p);
                    }
                    if ((normKey == "payment" || normName == "vendor payment") && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("payment", p);
                        RegisterAlias("vendor payment", p);
                        RegisterAlias("vendorpayment", p);
                        RegisterAlias("frmpayment", p);
                    }
                    if ((normKey == "debitnote" || normName.Contains("debit note")) && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("debitnote", p);
                        RegisterAlias("debit note", p);
                        RegisterAlias("debit note voucher", p);
                        RegisterAlias("frmdebitnote", p);
                    }
                    if ((normKey == "creditnote" || normName.Contains("credit note")) && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("creditnote", p);
                        RegisterAlias("credit note", p);
                        RegisterAlias("credit note voucher", p);
                        RegisterAlias("frmcreditnote", p);
                    }
                    if ((normKey == "bankreconciliation" || normName.Contains("bank reconciliation")) && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("bankreconciliation", p);
                        RegisterAlias("bank reconciliation", p);
                        RegisterAlias("frmbankreconciliation", p);
                    }
                    if ((normKey == "expenseentry" || normName.Contains("expense voucher")) && !normKey.Contains("report") && !normName.Contains("report"))
                    {
                        RegisterAlias("expenseentry", p);
                        RegisterAlias("expense voucher entry", p);
                        RegisterAlias("frmexpenseentry", p);
                    }

                    // 25. Utilities & Settings
                    if (normKey.Contains("activitylog") || normName.Contains("activity log"))
                    {
                        RegisterAlias("activitylog", p);
                        RegisterAlias("activity log", p);
                        RegisterAlias("activitylogselector", p);
                        RegisterAlias("activitylog ", p);
                    }
                    if (normKey.Contains("barcode") || normName.Contains("barcode"))
                    {
                        RegisterAlias("barcode", p);
                        RegisterAlias("print barcode", p);
                        RegisterAlias("printbarcode", p);
                        RegisterAlias("frmbarcode", p);
                    }
                    if (normKey.Contains("plu") || normName.Contains("plu"))
                    {
                        RegisterAlias("plu", p);
                        RegisterAlias("plu weighing", p);
                        RegisterAlias("pluweighing", p);
                        RegisterAlias("frmplu", p);
                    }
                    if (normKey.Contains("openingstock") || normName.Contains("opening stock"))
                    {
                        RegisterAlias("openingstock", p);
                        RegisterAlias("opening stock", p);
                        RegisterAlias("frmopeningstock", p);
                    }
                    if ((normKey == "counterclosing" || normKey == "btnclosing" || normKey == "closing" || normName == "closing" || normName.Contains("counter shift closing") || normName.Contains("daily closing")) && !normKey.Contains("year") && !normName.Contains("year"))
                    {
                        RegisterAlias("counterclosing", p);
                        RegisterAlias("btnclosing", p);
                        RegisterAlias("closing", p);
                        RegisterAlias("frmclosing", p);
                    }
                    if (normKey.Contains("changeitemno") || normName.Contains("change item no"))
                    {
                        RegisterAlias("changeitemno", p);
                        RegisterAlias("change item no", p);
                        RegisterAlias("frmchangeitemno", p);
                    }
                    if (normKey.Contains("excelimport") || normName.Contains("excel import"))
                    {
                        RegisterAlias("excelimport", p);
                        RegisterAlias("excel import", p);
                        RegisterAlias("excel import/export", p);
                        RegisterAlias("frmexcelimport", p);
                    }
                    if (normKey.Contains("yearclosing") || normName.Contains("year closing") || normName.Contains("financial year closing"))
                    {
                        RegisterAlias("yearclosing", p);
                        RegisterAlias("financialyearclosing", p);
                        RegisterAlias("financial year closing", p);
                        RegisterAlias("frmfinancialyearclosing", p);
                    }
                    if (normKey.Contains("language") || normName.Contains("language"))
                    {
                        RegisterAlias("applanguage", p);
                        RegisterAlias("languagesettings", p);
                        RegisterAlias("language", p);
                        RegisterAlias("app language", p);
                        RegisterAlias("frmlanguagesettings", p);
                    }
                    if (normKey.Contains("nexorisai") || normName.Contains("nexoris ai"))
                    {
                        RegisterAlias("nexorisai", p);
                        RegisterAlias("nexoris ai", p);
                        RegisterAlias("frmnexorisaisettings", p);
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine($"Loaded {_permissions.Count} permission entries for role ID {RoleId}");
        }

        /// <summary>
        /// Indicates whether the logged in user has Administrator / Admin privileges
        /// </summary>
        public static bool IsAdmin
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UserLevel)) return false;
                string ul = UserLevel.Trim();
                return ul.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                       ul.Equals("Administrator", StringComparison.OrdinalIgnoreCase);
            }
        }

        private static IEnumerable<string> GetEquivalentKeys(string key, string normKey)
        {
            yield return key;
            if (!string.IsNullOrEmpty(normKey) && !string.Equals(key, normKey, StringComparison.OrdinalIgnoreCase))
                yield return normKey;

            if (normKey == "ledgers" || normKey == "ledger" || normKey == "accountledgers")
            {
                yield return "ledgers";
                yield return "ledger";
                yield return "account ledgers";
                yield return "accountledgers";
                yield return "frmledgers";
                yield return "frmledger";
            }
            else if (normKey == "closing" || normKey == "btnclosing" || normKey == "counterclosing")
            {
                yield return "closing";
                yield return "btnclosing";
                yield return "counterclosing";
                yield return "frmclosing";
            }
        }

        /// <summary>
        /// </summary>
        public static bool CanView(string formKey)
        {
            if (string.IsNullOrWhiteSpace(formKey)) return false;
            if (IsAdmin) return true;
            if (_permissions == null || _permissions.Count == 0) return false;
            string k = formKey.Trim();
            string nk = NormalizeKey(k);
            foreach (var eq in GetEquivalentKeys(k, nk))
            {
                if (_permissions.TryGetValue(eq, out var pe) && pe != null && pe.CanView) return true;
            }
            return CanViewInternal(formKey);
        }

        private static bool CanViewInternal(string formKey)
        {
            if (string.IsNullOrWhiteSpace(formKey)) return false;
            if (IsAdmin) return true;
            if (_permissions == null || _permissions.Count == 0) return false;

            string key = formKey.Trim();
            if (_permissions.TryGetValue(key, out var p) && p != null)
                if (p.CanView) return true;

            string normKey = NormalizeKey(key);
            if (!string.IsNullOrEmpty(normKey) && _permissions.TryGetValue(normKey, out var pNorm) && pNorm != null)
                if (pNorm.CanView) return true;

            foreach (var kvp in _permissions)
            {
                if (kvp.Value == null) continue;
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeKey(kvp.Key), normKey, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeKey(kvp.Value.FormKey), normKey, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeKey(kvp.Value.FormName), normKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (kvp.Value.CanView) return true;
                }
            }

            return false;
        }

        public static bool CanAdd(string formKey)
        {
            if (string.IsNullOrWhiteSpace(formKey)) return false;
            if (IsAdmin) return true;
            if (_permissions == null || _permissions.Count == 0) return false;
            string k = formKey.Trim();
            string nk = NormalizeKey(k);
            foreach (var eq in GetEquivalentKeys(k, nk))
            {
                if (_permissions.TryGetValue(eq, out var pe) && pe != null && pe.CanAdd) return true;
            }
            return CanAddInternal(formKey);
        }

        private static bool CanAddInternal(string formKey)
        {
            if (string.IsNullOrWhiteSpace(formKey)) return false;
            if (IsAdmin) return true;
            if (_permissions == null || _permissions.Count == 0) return false;

            string key = formKey.Trim();
            if (_permissions.TryGetValue(key, out var p) && p != null)
                if (p.CanAdd) return true;

            string normKey = NormalizeKey(key);
            if (!string.IsNullOrEmpty(normKey) && _permissions.TryGetValue(normKey, out var pNorm) && pNorm != null)
                if (pNorm.CanAdd) return true;

            foreach (var kvp in _permissions)
            {
                if (kvp.Value == null) continue;
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeKey(kvp.Key), normKey, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeKey(kvp.Value.FormKey), normKey, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeKey(kvp.Value.FormName), normKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (kvp.Value.CanAdd) return true;
                }
            }

            return false;
        }

        public static bool CanEdit(string formKey)
        {
            if (string.IsNullOrWhiteSpace(formKey)) return false;
            if (IsAdmin) return true;
            if (_permissions == null || _permissions.Count == 0) return false;
            string k = formKey.Trim();
            string nk = NormalizeKey(k);
            foreach (var eq in GetEquivalentKeys(k, nk))
            {
                if (_permissions.TryGetValue(eq, out var pe) && pe != null && pe.CanEdit) return true;
            }
            return CanEditInternal(formKey);
        }

        private static bool CanEditInternal(string formKey)
        {
            if (string.IsNullOrWhiteSpace(formKey)) return false;
            if (IsAdmin) return true;
            if (_permissions == null || _permissions.Count == 0) return false;

            string key = formKey.Trim();
            if (_permissions.TryGetValue(key, out var p) && p != null)
                if (p.CanEdit) return true;

            string normKey = NormalizeKey(key);
            if (!string.IsNullOrEmpty(normKey) && _permissions.TryGetValue(normKey, out var pNorm) && pNorm != null)
                if (pNorm.CanEdit) return true;

            foreach (var kvp in _permissions)
            {
                if (kvp.Value == null) continue;
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeKey(kvp.Key), normKey, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeKey(kvp.Value.FormKey), normKey, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeKey(kvp.Value.FormName), normKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (kvp.Value.CanEdit) return true;
                }
            }

            return false;
        }

        public static bool CanDelete(string formKey)
        {
            if (string.IsNullOrWhiteSpace(formKey)) return false;
            if (IsAdmin) return true;
            if (_permissions == null || _permissions.Count == 0) return false;
            string k = formKey.Trim();
            string nk = NormalizeKey(k);
            foreach (var eq in GetEquivalentKeys(k, nk))
            {
                if (_permissions.TryGetValue(eq, out var pe) && pe != null && pe.CanDelete) return true;
            }
            return CanDeleteInternal(formKey);
        }

        private static bool CanDeleteInternal(string formKey)
        {
            if (string.IsNullOrWhiteSpace(formKey)) return false;
            if (IsAdmin) return true;
            if (_permissions == null || _permissions.Count == 0) return false;

            string key = formKey.Trim();
            if (_permissions.TryGetValue(key, out var p) && p != null)
                if (p.CanDelete) return true;

            string normKey = NormalizeKey(key);
            if (!string.IsNullOrEmpty(normKey) && _permissions.TryGetValue(normKey, out var pNorm) && pNorm != null)
                if (pNorm.CanDelete) return true;

            foreach (var kvp in _permissions)
            {
                if (kvp.Value == null) continue;
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeKey(kvp.Key), normKey, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeKey(kvp.Value.FormKey), normKey, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeKey(kvp.Value.FormName), normKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (kvp.Value.CanDelete) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the full permission object for a form
        /// </summary>
        public static Master.RolePermission GetPermission(string formKey)
        {
            if (string.IsNullOrWhiteSpace(formKey)) return null;
            string k = formKey.Trim();
            string nk = NormalizeKey(k);
            foreach (var eq in GetEquivalentKeys(k, nk))
            {
                if (_permissions.TryGetValue(eq, out var pe) && pe != null) return pe;
            }
            return GetPermissionInternal(formKey);
        }

        private static Master.RolePermission GetPermissionInternal(string formKey)
        {
            if (string.IsNullOrWhiteSpace(formKey)) return null;
            string key = formKey.Trim();
            if (_permissions.TryGetValue(key, out var p)) return p;
            string norm = NormalizeKey(key);
            if (!string.IsNullOrEmpty(norm) && _permissions.TryGetValue(norm, out var pNorm)) return pNorm;
            return null;
        }

        /// <summary>
        /// Clears all loaded permissions
        /// </summary>
        public static void ClearPermissions()
        {
            _permissions.Clear();
            RoleId = 0;
        }

        #endregion
    }
}
