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
        /// <param name="companyId">Company ID</param>
        /// <param name="branchId">Branch ID</param>
        /// <param name="finYearId">Financial Year ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="counterId">Counter ID (optional)</param>
        /// <param name="systemMacId">System MAC ID (optional)</param>
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
        /// <param name="companyId">Company ID</param>
        /// <param name="branchId">Branch ID</param>
        /// <param name="finYearId">Financial Year ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="userName">User Name</param>
        /// <param name="userLevel">User Level/Role</param>
        /// <param name="emailId">Email ID</param>
        /// <param name="branchName">Branch Name</param>
        /// <param name="companyName">Company Name (optional)</param>
        /// <param name="counterId">Counter ID (optional)</param>
        /// <param name="systemMacId">System MAC ID (optional)</param>
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

            OnSessionCleared?.Invoke();
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates that the session is initialized and all required values are set.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when session is not properly initialized</exception>
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
        /// <param name="errorMessage">Output parameter containing error message if validation fails</param>
        /// <returns>True if session is valid, false otherwise</returns>
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
        /// This method is called from Login.cs after retrieving settings from repository.
        /// </summary>
        /// <param name="settings">List of settings to apply</param>
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
                // Keep default values if settings fail to apply
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

        /// <summary>
        /// Applies a single setting value to the corresponding SessionContext property.
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <param name="value">Setting value as string</param>
        /// <param name="type">Setting type (Boolean, String, Integer)</param>
        private static void ApplySetting(string key, string value, string type)
        {
            if (string.IsNullOrEmpty(key)) return;

            string cleanKey = key.Trim();
            string cleanValue = value?.Trim();

            switch (cleanKey)
            {
                // Sales Settings
                case "DuplicateItemBehavior":
                    DuplicateItemBehavior = cleanValue ?? "MergeQuantity";
                    break;

                case "AllowNegativeStock":
                    AllowNegativeStock = ParseBoolean(cleanValue, false);
                    break;

                case "AutoPrintAfterSave":
                    AutoPrintAfterSave = ParseBoolean(cleanValue, true); // Default true
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

                // Display Settings
                case "ShowCostToUser":
                    ShowCostToUser = ParseBoolean(cleanValue, false);
                    break;

                case "ShowMarginColumn":
                    ShowMarginColumn = ParseBoolean(cleanValue, false);
                    break;

                // Printing Settings
                case "PrintCopies":
                    PrintCopies = int.TryParse(cleanValue, out var copies) ? copies : 1;
                    break;

                default:
                    // Unknown setting, log for debugging
                    System.Diagnostics.Debug.WriteLine($"Unknown setting key: {key}");
                    break;
            }
        }

        /// <summary>
        /// Marks settings as needing refresh. Call LoadSettings after retrieving from repository.
        /// </summary>
        public static void MarkSettingsForRefresh()
        {
            SettingsLoaded = false;
        }

        /// <summary>
        /// Resets all settings to their default values.
        /// </summary>
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

        /// <summary>
        /// Gets a summary of the current session context.
        /// </summary>
        /// <returns>String containing session information</returns>
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
        /// Loads permissions for the current user's role
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

                    if (!string.IsNullOrWhiteSpace(p.FormKey))
                    {
                        string key = p.FormKey.Trim();
                        _permissions[key] = p;
                        _permissions[key.Replace(" ", "").ToLowerInvariant()] = p;

                        // Add class name and tool key aliases for known keys
                        string cleanKey = key.ToLowerInvariant();
                        if (cleanKey == "itemmaster")
                        {
                            _permissions["frmitemmasternew"] = p;
                            _permissions["frmitemmaster"] = p;
                            _permissions["itemmaster"] = p;
                            _permissions["item master"] = p;
                        }
                        else if (cleanKey == "sales")
                        {
                            _permissions["frmsalesinvoice"] = p;
                            _permissions["sales invoice"] = p;
                            _permissions["sales"] = p;
                        }
                        else if (cleanKey == "salesreturn")
                        {
                            _permissions["frmsalesreturn"] = p;
                            _permissions["sales return"] = p;
                            _permissions["salesreturn"] = p;
                        }
                        else if (cleanKey == "purchase")
                        {
                            _permissions["frmpurchase"] = p;
                            _permissions["purchase invoice"] = p;
                            _permissions["purchase"] = p;
                        }
                        else if (cleanKey == "purchasereturn")
                        {
                            _permissions["frmpurchasereturn"] = p;
                            _permissions["purchase return"] = p;
                            _permissions["purchase r/n"] = p;
                            _permissions["purchasereturn"] = p;
                        }
                        else if (cleanKey == "purchaseorder")
                        {
                            _permissions["frmpurchaseorder"] = p;
                            _permissions["purchase order"] = p;
                            _permissions["purchaseorder"] = p;
                        }
                        else if (cleanKey == "pos")
                        {
                            _permissions["frmpos"] = p;
                            _permissions["pos terminal"] = p;
                            _permissions["pos"] = p;
                        }
                        else if (cleanKey == "grn")
                        {
                            _permissions["goods received"] = p;
                            _permissions["goods received notes"] = p;
                            _permissions["good received notes (grn)"] = p;
                            _permissions["grn"] = p;
                        }
                        else if (cleanKey == "stockadjustment")
                        {
                            _permissions["frmstockadjustment"] = p;
                            _permissions["stock adjustment"] = p;
                            _permissions["stockadjustment"] = p;
                        }
                        else if (cleanKey == "stocktransfer")
                        {
                            _permissions["frmstocktransfer"] = p;
                            _permissions["stock transfer"] = p;
                            _permissions["stocktransfer"] = p;
                        }
                        else if (cleanKey == "users")
                        {
                            _permissions["frmusers"] = p;
                            _permissions["users & accounts"] = p;
                            _permissions["users"] = p;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(p.FormName))
                    {
                        string name = p.FormName.Trim();
                        _permissions[name] = p;
                        _permissions[name.Replace(" ", "").ToLowerInvariant()] = p;
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

        /// <summary>
        /// Checks if the current user has View permission for the specified form
        /// </summary>
        /// <param name="formKey">Form key matching the tool key in ultraToolbarsManager</param>
        /// <returns>True if user can view the form, false otherwise</returns>
        public static bool CanView(string formKey)
        {
            if (string.IsNullOrEmpty(formKey)) return false;
            if (IsAdmin) return true;
            if (_permissions == null || _permissions.Count == 0) return false;

            string key = formKey.Trim();
            if (_permissions.TryGetValue(key, out var p) && p != null)
                return p.CanView;

            string normKey = key.Replace(" ", "").ToLowerInvariant();
            if (_permissions.TryGetValue(normKey, out var pNorm) && pNorm != null)
                return pNorm.CanView;

            foreach (var kvp in _permissions)
            {
                if (kvp.Value == null) continue;
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(kvp.Value.FormKey, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(kvp.Value.FormName, key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value.CanView;
                }
            }

            return false;
        }

        public static bool CanAdd(string formKey)
        {
            if (string.IsNullOrEmpty(formKey)) return false;
            if (IsAdmin) return true;
            if (_permissions == null || _permissions.Count == 0) return false;

            if (_permissions.TryGetValue(formKey, out var p) && p != null)
                return p.CanAdd;

            string cleanKey = formKey.Trim();
            foreach (var kvp in _permissions)
            {
                if (kvp.Value == null) continue;
                if (string.Equals(kvp.Key, cleanKey, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(kvp.Value.FormName, cleanKey, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value.CanAdd;
                }
            }

            return false;
        }

        public static bool CanEdit(string formKey)
        {
            if (string.IsNullOrEmpty(formKey)) return false;
            if (IsAdmin) return true;
            if (_permissions == null || _permissions.Count == 0) return false;

            if (_permissions.TryGetValue(formKey, out var p) && p != null)
                return p.CanEdit;

            string cleanKey = formKey.Trim();
            foreach (var kvp in _permissions)
            {
                if (kvp.Value == null) continue;
                if (string.Equals(kvp.Key, cleanKey, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(kvp.Value.FormName, cleanKey, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value.CanEdit;
                }
            }

            return false;
        }

        public static bool CanDelete(string formKey)
        {
            if (string.IsNullOrEmpty(formKey)) return false;
            if (IsAdmin) return true;
            if (_permissions == null || _permissions.Count == 0) return false;

            if (_permissions.TryGetValue(formKey, out var p) && p != null)
                return p.CanDelete;

            string cleanKey = formKey.Trim();
            foreach (var kvp in _permissions)
            {
                if (kvp.Value == null) continue;
                if (string.Equals(kvp.Key, cleanKey, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(kvp.Value.FormName, cleanKey, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value.CanDelete;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the full permission object for a form
        /// </summary>
        public static Master.RolePermission GetPermission(string formKey)
        {
            if (string.IsNullOrEmpty(formKey)) return null;
            return _permissions.TryGetValue(formKey, out var p) ? p : null;
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
