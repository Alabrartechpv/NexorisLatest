using Dapper;
using ModelClass;
using ModelClass.Master;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.MasterRepositry
{
    public class ExcelImportRepository : BaseRepostitory
    {
        public class ImportRow
        {
            public int RowIndex { get; set; }
            public string Barcode { get; set; }
            public string Description { get; set; }
            public string ItemType { get; set; }
            public string Category { get; set; }
            public string Brand { get; set; }
            public string Group { get; set; }
            public string Unit { get; set; }
            public double Cost { get; set; }
            public double RetailPrice { get; set; } // Walking price
            public double WholeSalePrice { get; set; } // Retail price
            public double MRP { get; set; }
            public double CardPrice { get; set; }
            public double CreditPrice { get; set; }
            public double StaffPrice { get; set; }
            public double MinPrice { get; set; }
            public double Packing { get; set; } = 1.0;
            public string IsBaseUnit { get; set; } = "Y";
            public double OpnStk { get; set; }
            public double ReOrder { get; set; }
            public string TaxType { get; set; }
            public double TaxPer { get; set; }
            public string HSNCode { get; set; }
            public string AlternativeBarcodes { get; set; }
            public int OrderCycleDays { get; set; } = 7;
            public double BoxQty { get; set; } = 1.0;
            public string Perishable { get; set; } = "N";
            
            // Validation output fields
            public bool HasError { get; set; }
            public bool HasWarning { get; set; }
            public string StatusMessage { get; set; }
        }

        public class ImportSummary
        {
            public int TotalRows { get; set; }
            public int SucceededCount { get; set; }
            public int FailedCount { get; set; }
            public int NewItemsCreated { get; set; }
            public int ItemsUpdated { get; set; }
            public List<ImportRow> Rows { get; set; } = new List<ImportRow>();
        }

        // Cache collections for O(1) in-memory checks
        public HashSet<string> ExistingBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> CategoryCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> BrandCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> GroupCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> UnitCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> ItemTypeCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> VendorCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private int GetBranchId()
        {
            if (SessionContext.IsInitialized && SessionContext.BranchId > 0)
                return SessionContext.BranchId;
            if (!string.IsNullOrEmpty(DataBase.BranchId) && int.TryParse(DataBase.BranchId, out int dbBranchId) && dbBranchId > 0)
                return dbBranchId;
            return 1;
        }

        private int GetCompanyId()
        {
            if (SessionContext.IsInitialized && SessionContext.CompanyId > 0)
                return SessionContext.CompanyId;
            if (!string.IsNullOrEmpty(DataBase.CompanyId) && int.TryParse(DataBase.CompanyId, out int dbCompanyId) && dbCompanyId > 0)
                return dbCompanyId;
            return 1;
        }

        private int GetFinYearId()
        {
            if (SessionContext.IsInitialized && SessionContext.FinYearId > 0)
                return SessionContext.FinYearId;
            if (!string.IsNullOrEmpty(DataBase.FinyearId) && int.TryParse(DataBase.FinyearId, out int dbFinYearId) && dbFinYearId > 0)
                return dbFinYearId;
            return 1;
        }

        public static string CleanImportedBarcode(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return string.Empty;
            val = val.Trim();

            // 1. Strip Excel text formula format: ="12345" or =12345
            if (val.StartsWith("=") && val.Length > 1)
            {
                val = val.Substring(1).Trim('\"', '\'').Trim();
            }

            // 2. Resolve scientific notation (e.g. 4.53E+10, 1E+14, 2E+12)
            if (val.Contains("E+") || val.Contains("e+") || val.Contains("E-") || val.Contains("e-"))
            {
                if (decimal.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal decVal))
                {
                    return decVal.ToString("F0", System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            return val;
        }

        public static string CleanAlternativeBarcodes(string val)
        {
            if (val == null) return null;
            if (string.IsNullOrWhiteSpace(val)) return string.Empty;
            var parts = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var cleaned = parts.Select(p => CleanImportedBarcode(p)).Where(s => !string.IsNullOrWhiteSpace(s));
            return string.Join(",", cleaned);
        }

        // Load caches from DB
        public void LoadDBCaches()
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                // 1. Barcodes Cache (ItemMaster Barcode, PriceSettings BarCode/AliasBarcode, AlternativeBarcode)
                ExistingBarcodes.Clear();
                var barcodes = DataConnection.Query<string>(@"
                    SELECT Barcode FROM ItemMaster WHERE Barcode IS NOT NULL AND Barcode != ''
                    UNION
                    SELECT BarCode FROM PriceSettings WHERE BarCode IS NOT NULL AND BarCode != ''
                    UNION
                    SELECT AliasBarcode FROM PriceSettings WHERE AliasBarcode IS NOT NULL AND AliasBarcode != ''
                    UNION
                    SELECT Barcode FROM ItemAlternativeBarcode WHERE Barcode IS NOT NULL AND Barcode != ''");
                foreach (var bc in barcodes)
                {
                    if (!string.IsNullOrWhiteSpace(bc))
                        ExistingBarcodes.Add(bc.Trim());
                }

                // 2. Categories Cache
                CategoryCache.Clear();
                var categories = DataConnection.Query<dynamic>("SELECT Id, CategoryName FROM Category WHERE IsDelete = 0");
                foreach (var cat in categories)
                {
                    string name = Convert.ToString(cat.CategoryName);
                    if (!string.IsNullOrWhiteSpace(name))
                        CategoryCache[name.Trim()] = Convert.ToInt32(cat.Id);
                }

                // 3. Brands Cache
                BrandCache.Clear();
                var brands = DataConnection.Query<dynamic>("SELECT Id, BrandName FROM Brands WHERE IsDelete = 0");
                foreach (var b in brands)
                {
                    string name = Convert.ToString(b.BrandName);
                    if (!string.IsNullOrWhiteSpace(name))
                        BrandCache[name.Trim()] = Convert.ToInt32(b.Id);
                }

                // 4. Groups Cache
                GroupCache.Clear();
                var groups = DataConnection.Query<dynamic>("SELECT Id, GroupName FROM [Group]");
                foreach (var g in groups)
                {
                    string name = Convert.ToString(g.GroupName);
                    if (!string.IsNullOrWhiteSpace(name))
                        GroupCache[name.Trim()] = Convert.ToInt32(g.Id);
                }

                // 5. Units Cache
                UnitCache.Clear();
                var units = DataConnection.Query<dynamic>("SELECT UnitID, UnitName FROM UnitMaster WHERE IsDelete = 0");
                foreach (var u in units)
                {
                    string name = Convert.ToString(u.UnitName);
                    if (!string.IsNullOrWhiteSpace(name))
                        UnitCache[name.Trim()] = Convert.ToInt32(u.UnitID);
                }

                // 6. Item Types Cache
                ItemTypeCache.Clear();
                var itemTypes = DataConnection.Query<dynamic>("SELECT Id, ItemType FROM ItemTypes");
                foreach (var it in itemTypes)
                {
                    string name = Convert.ToString(it.ItemType);
                    if (!string.IsNullOrWhiteSpace(name))
                        ItemTypeCache[name.Trim()] = Convert.ToInt32(it.Id);
                }

                // 7. Vendors Cache
                VendorCache.Clear();
                var vendors = DataConnection.Query<dynamic>(STOREDPROCEDURE.POS_Vendor, new { CompanyId = GetCompanyId(), BranchId = GetBranchId(), _Operation = "DDLVendor" }, commandType: CommandType.StoredProcedure);
                foreach (var v in vendors)
                {
                    string name = Convert.ToString(v.LedgerName);
                    if (!string.IsNullOrWhiteSpace(name))
                        VendorCache[name.Trim()] = Convert.ToInt32(v.LedgerID);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading caches: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        // Resolves or auto-creates Category
        private int ResolveOrCreateCategory(string categoryName, int branchId, int companyId, SqlTransaction transaction)
        {
            categoryName = categoryName?.Trim();
            if (string.IsNullOrWhiteSpace(categoryName))
                return 0;

            if (CategoryCache.TryGetValue(categoryName, out int categoryId))
                return categoryId;

            // Make sure Group is resolved/created first to link
            int groupId = ResolveOrCreateGroup("General", branchId, companyId, transaction);

            // Create new Category
            var category = new Category
            {
                CategoryName = categoryName,
                GroupId = groupId,
                _Operation = "CREATE",
                Photo = null
            };

            var list = DataConnection.Query<Category>(
                STOREDPROCEDURE.POS_Category,
                category,
                transaction,
                commandType: CommandType.StoredProcedure
            ).ToList();

            // Category SP might return the created row or set of values
            int newId = 0;
            if (list.Count > 0)
            {
                newId = list[0].Id;
            }
            else
            {
                // Fallback direct select if ID wasn't returned directly by procedure
                newId = DataConnection.QueryFirstOrDefault<int>(
                    "SELECT Id FROM Category WHERE CategoryName = @Name AND IsDelete = 0",
                    new { Name = categoryName },
                    transaction
                );
            }

            if (newId > 0)
            {
                CategoryCache[categoryName] = newId;
                return newId;
            }

            return 0;
        }

        // Resolves or auto-creates Brand
        private int ResolveOrCreateBrand(string brandName, SqlTransaction transaction)
        {
            brandName = brandName?.Trim();
            if (string.IsNullOrWhiteSpace(brandName))
                return 0;

            if (BrandCache.TryGetValue(brandName, out int brandId))
                return brandId;

            var brand = new Brand
            {
                BrandName = brandName,
                _Operation = "CREATE",
                Photo = null
            };

            DataConnection.Execute(
                STOREDPROCEDURE.POS_Brand,
                brand,
                transaction,
                commandType: CommandType.StoredProcedure
            );

            int newId = DataConnection.QueryFirstOrDefault<int>(
                "SELECT Id FROM Brands WHERE BrandName = @Name AND IsDelete = 0",
                new { Name = brandName },
                transaction
            );

            if (newId > 0)
            {
                BrandCache[brandName] = newId;
                return newId;
            }

            return 0;
        }

        // Resolves or auto-creates Group
        private int ResolveOrCreateGroup(string groupName, int branchId, int companyId, SqlTransaction transaction)
        {
            groupName = groupName?.Trim();
            if (string.IsNullOrWhiteSpace(groupName))
                return 0;

            if (GroupCache.TryGetValue(groupName, out int groupId))
                return groupId;

            var group = new Group
            {
                GroupName = groupName,
                BranchId = branchId,
                _Operation = "CREATE",
                Photo = null
            };

            DataConnection.Execute(
                STOREDPROCEDURE.POS_Group,
                group,
                transaction,
                commandType: CommandType.StoredProcedure
            );

            int newId = DataConnection.QueryFirstOrDefault<int>(
                "SELECT Id FROM [Group] WHERE GroupName = @Name",
                new { Name = groupName },
                transaction
            );

            if (newId > 0)
            {
                GroupCache[groupName] = newId;
                return newId;
            }

            return 0;
        }

        // Resolves or auto-creates Unit
        private int ResolveOrCreateUnit(string unitName, SqlTransaction transaction)
        {
            unitName = unitName?.Trim();
            if (string.IsNullOrWhiteSpace(unitName))
                unitName = "PCS";

            if (UnitCache.TryGetValue(unitName, out int unitId))
                return unitId;

            // Use the UnitMasterRepository standard procedure to insert
            DataConnection.Execute(
                STOREDPROCEDURE.POS_UnitMaster,
                new
                {
                    UnitID = (int?)null,
                    UnitName = unitName,
                    UnitSymbol = (string)null,
                    UnitQuantityCode = (int?)null,
                    Packing = (double?)null,
                    NoOfDecimalPlaces = (int?)null,
                    UnitNameInBill = (string)null,
                    IsDelete = 0,
                    _Operation = "Create"
                },
                transaction,
                commandType: CommandType.StoredProcedure
            );

            // Fetch created UnitId using the GetByName operation
            int newId = DataConnection.QueryFirstOrDefault<int>(
                STOREDPROCEDURE.POS_UnitMaster,
                new
                {
                    UnitName = unitName,
                    _Operation = "GetByName",
                    UnitID = (int?)null,
                    UnitSymbol = (string)null,
                    UnitQuantityCode = (int?)null,
                    Packing = (double?)null,
                    NoOfDecimalPlaces = (int?)null,
                    UnitNameInBill = (string)null,
                    IsDelete = (bool?)null
                },
                transaction,
                commandType: CommandType.StoredProcedure
            );

            if (newId > 0)
            {
                UnitCache[unitName] = newId;
                return newId;
            }

            return 0;
        }

        /// <summary>
        /// Prevents "too many arguments specified" errors when POCO has extra properties.
        /// </summary>
        private DynamicParameters BuildFilteredParameters(string storedProcName, ItemMasterPriceSettings src, SqlTransaction transaction)
        {
            var dyn = new DynamicParameters();
            try
            {
                using (var cmd = new SqlCommand(storedProcName, (SqlConnection)DataConnection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlCommandBuilder.DeriveParameters(cmd);
                    foreach (SqlParameter sqlParam in cmd.Parameters)
                    {
                        if (sqlParam.Direction == ParameterDirection.ReturnValue) continue;
                        string pName = sqlParam.ParameterName?.TrimStart('@');
                        if (string.IsNullOrEmpty(pName)) continue;

                        // Try case-insensitive property match on src
                        var prop = typeof(ItemMasterPriceSettings).GetProperties()
                            .FirstOrDefault(pi => string.Equals(pi.Name, pName, StringComparison.OrdinalIgnoreCase));

                        object val = null;
                        if (prop != null)
                        {
                            val = prop.GetValue(src, null);
                        }
                        else
                        {
                            // Common alternate names mapping if needed
                            if (string.Equals(pName, "BranchID", StringComparison.OrdinalIgnoreCase))
                                val = src.BranchId;
                            else if (string.Equals(pName, "CompanyID", StringComparison.OrdinalIgnoreCase))
                                val = src.CompanyId;
                            else if (string.Equals(pName, "FinYearID", StringComparison.OrdinalIgnoreCase))
                                val = src.FinYearId;
                            else if (string.Equals(pName, "Barcode", StringComparison.OrdinalIgnoreCase))
                                val = src.BarCode;
                            else if (string.Equals(pName, "PhotoByteArray", StringComparison.OrdinalIgnoreCase))
                                val = src.PhotoByteArray;
                            else if (string.Equals(pName, "Photo", StringComparison.OrdinalIgnoreCase))
                                val = src.Photo;
                            else if (string.Equals(pName, "Operation", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(pName, "_Operation", StringComparison.OrdinalIgnoreCase))
                                val = src._Operation;
                            else if (string.Equals(pName, "OpnValue", StringComparison.OrdinalIgnoreCase))
                                val = src.OpnStk * src.Cost;
                            else if (string.Equals(pName, "OpeningCost", StringComparison.OrdinalIgnoreCase))
                                val = src.Cost;
                            else if (string.Equals(pName, "OpnDate", StringComparison.OrdinalIgnoreCase))
                                val = DateTime.Now;
                        }

                        // Ensure BranchName is not sent as DBNull
                        if (string.Equals(pName, "BranchName", StringComparison.OrdinalIgnoreCase))
                        {
                            var s = val as string;
                            if (string.IsNullOrWhiteSpace(s))
                            {
                                val = src.BranchName ?? string.Empty;
                            }
                        }

                        // Ensure Unit is not sent as DBNull
                        if (string.Equals(pName, "Unit", StringComparison.OrdinalIgnoreCase))
                        {
                            var s = val as string;
                            if (string.IsNullOrWhiteSpace(s))
                            {
                                val = string.Empty;
                            }
                        }

                        // Ensure BarCode is not sent as DBNull
                        if (string.Equals(pName, "BarCode", StringComparison.OrdinalIgnoreCase))
                        {
                            var s = val as string;
                            if (string.IsNullOrWhiteSpace(s))
                            {
                                val = string.Empty;
                            }
                        }

                        // Ensure TaxType is not sent as DBNull
                        if (string.Equals(pName, "TaxType", StringComparison.OrdinalIgnoreCase))
                        {
                            var s = val as string;
                            if (string.IsNullOrWhiteSpace(s))
                            {
                                val = "EXCL";
                            }
                        }

                        // Ensure IsBaseUnit always has a value if the SP expects it
                        if (string.Equals(pName, "IsBaseUnit", StringComparison.OrdinalIgnoreCase))
                        {
                            var s = Convert.ToString(val);
                            if (string.IsNullOrWhiteSpace(s)) val = "N";
                        }

                        // Ensure Costing always has a value if the SP expects it
                        if (string.Equals(pName, "Costing", StringComparison.OrdinalIgnoreCase))
                        {
                            var s = Convert.ToString(val);
                            if (string.IsNullOrWhiteSpace(s)) val = "AVERAGE";
                        }

                        // Avoid passing DBNull for blob parameters
                        if ((string.Equals(pName, "Photo", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(pName, "PhotoByteArray", StringComparison.OrdinalIgnoreCase)))
                        {
                            var bytes = val as byte[];
                            if (bytes == null || bytes.Length == 0)
                                continue;
                        }

                        // If the SP param is a string type and value is null/DBNull, coerce to empty string
                        if (val == null || val == DBNull.Value)
                        {
                            if (sqlParam.SqlDbType == SqlDbType.VarChar ||
                                sqlParam.SqlDbType == SqlDbType.NVarChar ||
                                sqlParam.SqlDbType == SqlDbType.Char ||
                                sqlParam.SqlDbType == SqlDbType.NChar ||
                                sqlParam.SqlDbType == SqlDbType.Text ||
                                sqlParam.SqlDbType == SqlDbType.NText)
                            {
                                val = string.Empty;
                            }
                            else
                            {
                                val = null;
                            }
                        }

                        dyn.Add("@" + pName, val);
                    }
                }
            }
            catch
            {
                // Fallback: add a minimal safe set if DeriveParameters fails
                dyn.Add("@ItemId", src.ItemId);
                dyn.Add("@CompanyId", src.CompanyId);
                dyn.Add("@BranchId", src.BranchId);
                if (!string.IsNullOrWhiteSpace(src.BranchName)) dyn.Add("@BranchName", src.BranchName);
                dyn.Add("@FinYearId", src.FinYearId);
                dyn.Add("@UnitId", src.UnitId);
                dyn.Add("@Unit", src.Unit);
                dyn.Add("@Packing", src.Packing);
                dyn.Add("@BarCode", src.BarCode);
                dyn.Add("@ReOrder", src.ReOrder);
                dyn.Add("@OpnStk", src.OpnStk);
                dyn.Add("@OpnValue", src.OpnStk * src.Cost);
                dyn.Add("@OpeningCost", src.Cost);
                dyn.Add("@OpnDate", DateTime.Now);
                dyn.Add("@Cost", src.Cost);
                dyn.Add("@MarginAmt", src.MarginAmt);
                dyn.Add("@MarginPer", src.MarginPer);
                dyn.Add("@MRP", src.MRP);
                dyn.Add("@RetailPrice", src.RetailPrice);
                dyn.Add("@WholeSalePrice", src.WholeSalePrice);
                dyn.Add("@CreditPrice", src.CreditPrice);
                dyn.Add("@CardPrice", src.CardPrice);
                dyn.Add("@StaffPrice", src.StaffPrice);
                dyn.Add("@MinPrice", src.MinPrice);
                dyn.Add("@MDRetailPrice", src.MDRetailPrice);
                dyn.Add("@MDWalkinPrice", src.MDWalkinPrice);
                dyn.Add("@MDCreditPrice", src.MDCreditPrice);
                dyn.Add("@MDMrpPrice", src.MDMrpPrice);
                dyn.Add("@MDCardPrice", src.MDCardPrice);
                dyn.Add("@MDStaffPrice", src.MDStaffPrice);
                dyn.Add("@MDMinPrice", src.MDMinPrice);
                dyn.Add("@TaxPer", src.TaxPer);
                dyn.Add("@TaxAmt", src.TaxAmt);
                dyn.Add("@TaxType", src.TaxType);
                dyn.Add("@IsBaseUnit", (object)(string.IsNullOrWhiteSpace(src.IsBaseUnit) ? "N" : src.IsBaseUnit));
                dyn.Add("@Costing", (object)(string.IsNullOrWhiteSpace(src.Costing) ? "AVERAGE" : src.Costing));
                if (src.Photo != null && src.Photo.Length > 0) dyn.Add("@Photo", src.Photo);
                if (src.PhotoByteArray != null && src.PhotoByteArray.Length > 0) dyn.Add("@PhotoByteArray", src.PhotoByteArray);
                dyn.Add("@_Operation", src._Operation);
            }

            return dyn;
        }

        /// <summary>
        /// Prevents "too many arguments specified" errors when POCO has extra properties.
        /// </summary>
        private DynamicParameters BuildFilteredParametersForItem(string storedProcName, Item src, SqlTransaction transaction)
        {
            var dyn = new DynamicParameters();
            try
            {
                using (var cmd = new SqlCommand(storedProcName, (SqlConnection)DataConnection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlCommandBuilder.DeriveParameters(cmd);
                    foreach (SqlParameter sqlParam in cmd.Parameters)
                    {
                        if (sqlParam.Direction == ParameterDirection.ReturnValue) continue;
                        string pName = sqlParam.ParameterName?.TrimStart('@');
                        if (string.IsNullOrEmpty(pName)) continue;

                        // Try case-insensitive property match on src
                        var prop = typeof(Item).GetProperties()
                            .FirstOrDefault(pi => string.Equals(pi.Name, pName, StringComparison.OrdinalIgnoreCase));

                        object val = null;
                        if (prop != null)
                        {
                            val = prop.GetValue(src, null);
                        }
                        else
                        {
                            // Common alternate names mapping if needed
                            if (string.Equals(pName, "BranchID", StringComparison.OrdinalIgnoreCase))
                                val = src.BranchId;
                            else if (string.Equals(pName, "CompanyID", StringComparison.OrdinalIgnoreCase))
                                val = src.CompanyId;
                            else if (string.Equals(pName, "FinYearID", StringComparison.OrdinalIgnoreCase))
                                val = src.FinYearId;
                            else if (string.Equals(pName, "Operation", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(pName, "_Operation", StringComparison.OrdinalIgnoreCase))
                                val = src._Operation;
                        }

                        // If the SP param is a string type and value is null/DBNull, coerce to empty string
                        if (val == null || val == DBNull.Value)
                        {
                            if (sqlParam.SqlDbType == SqlDbType.VarChar ||
                                sqlParam.SqlDbType == SqlDbType.NVarChar ||
                                sqlParam.SqlDbType == SqlDbType.Char ||
                                sqlParam.SqlDbType == SqlDbType.NChar ||
                                sqlParam.SqlDbType == SqlDbType.Text ||
                                sqlParam.SqlDbType == SqlDbType.NText)
                            {
                                val = string.Empty;
                            }
                            else
                            {
                                val = null;
                            }
                        }

                        if (string.Equals(pName, "CategoryId", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(pName, "BrandId", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(pName, "GroupId", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(pName, "VendorId", StringComparison.OrdinalIgnoreCase))
                        {
                            if (val != null && Convert.ToInt32(val) == 0)
                            {
                                val = null;
                            }
                        }

                        dyn.Add("@" + pName, val);
                    }
                }
            }
            catch
            {
                // Fallback: add a minimal safe set if DeriveParameters fails
                dyn.Add("@CompanyId", src.CompanyId);
                dyn.Add("@BranchId", src.BranchId);
                dyn.Add("@FinYearId", src.FinYearId);
                dyn.Add("@ItemId", src.ItemId);
                dyn.Add("@ItemNo", src.ItemNo);
                dyn.Add("@Description", src.Description);
                dyn.Add("@Barcode", src.Barcode);
                dyn.Add("@ItemTypeId", src.ItemTypeId);
                dyn.Add("@VendorId", src.VendorId > 0 ? (object)src.VendorId : null);
                dyn.Add("@BrandId", src.BrandId > 0 ? (object)src.BrandId : null);
                dyn.Add("@GroupId", src.GroupId > 0 ? (object)src.GroupId : null);
                dyn.Add("@CategoryId", src.CategoryId > 0 ? (object)src.CategoryId : null);
                dyn.Add("@BaseUnitId", src.BaseUnitId);
                dyn.Add("@ForCustomerType", src.ForCustomerType);
                dyn.Add("@NameInLocalLanguage", src.NameInLocalLanguage);
                dyn.Add("@HSNCode", src.HSNCode);
                dyn.Add("@Order_Cycle_Days", src.Order_Cycle_Days);
                dyn.Add("@Box_Quantity", src.Box_Quantity);
                dyn.Add("@Is_Perishable", src.Is_Perishable);
                dyn.Add("@_Operation", src._Operation);
            }

            return dyn;
        }

        // Import process main routine
        public ImportSummary ImportProducts(
            List<ImportRow> rawRows, 
            string duplicateBehavior, 
            bool autoCreateMasters, 
            bool autoGenerateBarcodes,
            Action<int, int> progressCallback)
        {
            ImportSummary summary = new ImportSummary { TotalRows = rawRows.Count };
            LoadDBCaches();

            int branchId = GetBranchId();
            int companyId = GetCompanyId();
            int finYearId = GetFinYearId();

            // Step 1: Pre-validation of individual records
            long generatedBarcodeSeed = 200000000001; // Internal EAN barcode base
            HashSet<string> sessionAddedBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int index = 0; index < rawRows.Count; index++)
            {
                var row = rawRows[index];
                row.RowIndex = index + 1;

                // Clean and format numeric codes (handling Excel's scientific notation)
                row.Barcode = CleanImportedBarcode(row.Barcode);
                row.HSNCode = CleanImportedBarcode(row.HSNCode);
                row.AlternativeBarcodes = CleanAlternativeBarcodes(row.AlternativeBarcodes);
                row.Description = row.Description?.Trim();

                // Validate barcode generation
                if (string.IsNullOrWhiteSpace(row.Barcode))
                {
                    if (autoGenerateBarcodes)
                    {
                        // Generate a unique sequential barcode that is not in cached set
                        string genBarcode = generatedBarcodeSeed.ToString();
                        while (ExistingBarcodes.Contains(genBarcode) || sessionAddedBarcodes.Contains(genBarcode))
                        {
                            generatedBarcodeSeed++;
                            genBarcode = generatedBarcodeSeed.ToString();
                        }
                        row.Barcode = genBarcode;
                        sessionAddedBarcodes.Add(genBarcode);
                        row.HasWarning = true;
                        row.StatusMessage += $"[Generated Barcode: {genBarcode}] ";
                    }
                    else
                    {
                        row.HasError = true;
                        row.StatusMessage += "Barcode is missing. ";
                    }
                }
                else
                {
                    sessionAddedBarcodes.Add(row.Barcode);
                }

                // Description validation
                if (string.IsNullOrWhiteSpace(row.Description))
                {
                    row.HasError = true;
                    row.StatusMessage += "Description/Name is missing. ";
                }

                // Prices validation
                if (row.Cost < 0)
                {
                    row.HasError = true;
                    row.StatusMessage += "Cost price cannot be negative. ";
                }
                if (row.RetailPrice < 0)
                {
                    row.HasError = true;
                    row.StatusMessage += "Retail price cannot be negative. ";
                }
                if (row.Cost > row.RetailPrice)
                {
                    row.HasWarning = true;
                    row.StatusMessage += "Warning: Cost is higher than Retail Price. ";
                }

                // Master validation
                if (!autoCreateMasters)
                {
                    if (!string.IsNullOrWhiteSpace(row.Category) && !CategoryCache.ContainsKey(row.Category))
                    {
                        row.HasError = true;
                        row.StatusMessage += $"Category '{row.Category}' does not exist. ";
                    }
                    if (!string.IsNullOrWhiteSpace(row.Brand) && !BrandCache.ContainsKey(row.Brand))
                    {
                        row.HasError = true;
                        row.StatusMessage += $"Brand '{row.Brand}' does not exist. ";
                    }
                    if (!string.IsNullOrWhiteSpace(row.Unit) && !UnitCache.ContainsKey(row.Unit))
                    {
                        row.HasError = true;
                        row.StatusMessage += $"Unit '{row.Unit}' does not exist. ";
                    }
                }
                else
                {
                    // Will be created on-the-fly, mark warning for tracking
                    if (!string.IsNullOrWhiteSpace(row.Category) && !CategoryCache.ContainsKey(row.Category))
                    {
                        row.HasWarning = true;
                        row.StatusMessage += $"[Will create Category: {row.Category}] ";
                    }
                    if (!string.IsNullOrWhiteSpace(row.Brand) && !BrandCache.ContainsKey(row.Brand))
                    {
                        row.HasWarning = true;
                        row.StatusMessage += $"[Will create Brand: {row.Brand}] ";
                    }
                    if (!string.IsNullOrWhiteSpace(row.Unit) && !UnitCache.ContainsKey(row.Unit))
                    {
                        row.HasWarning = true;
                        row.StatusMessage += $"[Will create Unit: {row.Unit}] ";
                    }
                }

                if (row.HasError)
                {
                    summary.FailedCount++;
                }
            }

            // Step 2: Group rows by product description for Multi-UOM insertions
            // Items are unique by Description/Name in this POS database structure (Description has uniqueness check in SP)
            var validRows = rawRows.Where(r => !r.HasError).ToList();
            var groupedItems = validRows.GroupBy(r => r.Description, StringComparer.OrdinalIgnoreCase).ToList();

            // Open database connection
            DataConnection.Open();
            var dbTransaction = ((SqlConnection)DataConnection).BeginTransaction();

            try
            {
                int processedCount = 0;
                int totalGroups = groupedItems.Count;

                foreach (var group in groupedItems)
                {
                    string description = group.Key;
                    var uomRows = group.ToList();

                    // Deduplicate UOMs in the group to prevent duplicate database rows
                    var uniqueUoms = new List<ImportRow>();
                    var seenUoms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var row in uomRows)
                    {
                        string uKey = $"{(row.Unit ?? "PCS").Trim()}_{row.Packing}";
                        if (!seenUoms.Contains(uKey))
                        {
                            seenUoms.Add(uKey);
                            uniqueUoms.Add(row);
                        }
                    }
                    uomRows = uniqueUoms;

                    // Check if this item already exists in the database and get its reorder parameters
                    int existingItemId = 0;
                    int dbOrderCycleDays = 7;
                    double dbBoxQuantity = 1.0;
                    bool dbIsPerishable = false;

                    var existingItemInfo = DataConnection.QueryFirstOrDefault<dynamic>(
                        "SELECT ItemId, Order_Cycle_Days, Box_Quantity, Is_Perishable FROM ItemMaster WHERE Description = @Desc AND Active = 0",
                        new { Desc = description },
                        dbTransaction
                    );

                    if (existingItemInfo != null)
                    {
                        existingItemId = Convert.ToInt32(existingItemInfo.ItemId);
                        dbOrderCycleDays = Convert.ToInt32(existingItemInfo.Order_Cycle_Days ?? 7);
                        dbBoxQuantity = Convert.ToDouble(existingItemInfo.Box_Quantity ?? 1.0);
                        dbIsPerishable = Convert.ToBoolean(existingItemInfo.Is_Perishable ?? false);
                    }

                    bool isUpdate = existingItemId > 0;

                    if (isUpdate && duplicateBehavior == "Skip")
                    {
                        // Skip the entire group
                        foreach (var row in uomRows)
                        {
                            row.HasWarning = true;
                            row.StatusMessage = "Skipped: Product description already exists in database.";
                        }
                        processedCount++;
                        progressCallback?.Invoke(processedCount, totalGroups);
                        continue;
                    }

                    var baseRow = uomRows.FirstOrDefault(r => r.IsBaseUnit.Trim().ToUpper() == "Y" || r.Packing == 1.0) ?? uomRows.First();

                    int targetOrderCycleDays = 7;
                    if (baseRow.OrderCycleDays != -99)
                        targetOrderCycleDays = baseRow.OrderCycleDays;
                    else if (isUpdate)
                        targetOrderCycleDays = dbOrderCycleDays;

                    double targetBoxQty = 1.0;
                    if (baseRow.BoxQty != -99.0)
                        targetBoxQty = baseRow.BoxQty;
                    else if (isUpdate)
                        targetBoxQty = dbBoxQuantity;

                    bool targetIsPerishable = false;
                    if (baseRow.Perishable != null)
                        targetIsPerishable = string.Equals(baseRow.Perishable, "Y", StringComparison.OrdinalIgnoreCase) ||
                                             string.Equals(baseRow.Perishable, "YES", StringComparison.OrdinalIgnoreCase) ||
                                             string.Equals(baseRow.Perishable, "TRUE", StringComparison.OrdinalIgnoreCase) ||
                                             string.Equals(baseRow.Perishable, "1", StringComparison.OrdinalIgnoreCase);
                    else if (isUpdate)
                        targetIsPerishable = dbIsPerishable;
                    
                    // Core Item Info mapping
                    int resolvedCategory = ResolveOrCreateCategory(baseRow.Category, branchId, companyId, dbTransaction);
                    int resolvedBrand = ResolveOrCreateBrand(baseRow.Brand, dbTransaction);
                    int resolvedGroup = ResolveOrCreateGroup(baseRow.Group, branchId, companyId, dbTransaction);
                    int resolvedBaseUnit = ResolveOrCreateUnit(baseRow.Unit, dbTransaction);
                    
                    // Map default stock item type (ID = 1 typically or lookup)
                    int resolvedItemType = 1;
                    if (!string.IsNullOrWhiteSpace(baseRow.ItemType) && ItemTypeCache.TryGetValue(baseRow.ItemType.Trim(), out int itId))
                        resolvedItemType = itId;

                    int resolvedVendor = 0;
                    if (!string.IsNullOrWhiteSpace(baseRow.Category) && VendorCache.TryGetValue(baseRow.Category.Trim(), out int vId))
                        resolvedVendor = vId;

                    int targetItemId = existingItemId;

                    if (!isUpdate)
                    {
                        // 1. Create product in ItemMaster using stored procedure
                        var newItem = new Item
                        {
                            CompanyId = companyId,
                            BranchId = branchId,
                            FinYearId = finYearId,
                            ItemId = 0,
                            ItemNo = 0,
                            Description = description,
                            Barcode = baseRow.Barcode,
                            ItemTypeId = resolvedItemType,
                            VendorId = resolvedVendor,
                            BrandId = resolvedBrand,
                            GroupId = resolvedGroup,
                            CategoryId = resolvedCategory,
                            BaseUnitId = resolvedBaseUnit,
                            ForCustomerType = "ALL",
                            NameInLocalLanguage = "",
                            HSNCode = baseRow.HSNCode ?? "",
                            Order_Cycle_Days = targetOrderCycleDays,
                            Box_Quantity = (int)targetBoxQty,
                            Is_Perishable = targetIsPerishable,
                            _Operation = "CREATE"
                        };

                        var itemParams = BuildFilteredParametersForItem(STOREDPROCEDURE.POS_ItemMaster, newItem, dbTransaction);
                        List<Item> savedItems = DataConnection.Query<Item>(
                            STOREDPROCEDURE.POS_ItemMaster,
                            itemParams,
                            dbTransaction,
                            commandType: CommandType.StoredProcedure
                        ).ToList();

                        if (savedItems.Count == 0 || savedItems[0].ItemId <= 0)
                        {
                            throw new Exception("Failed to save item master using stored procedure");
                        }

                        targetItemId = savedItems[0].ItemId;
                        summary.NewItemsCreated++;
                    }
                    else
                    {
                        // 2. Update existing item in ItemMaster using stored procedure
                        var updateItem = new Item
                        {
                            CompanyId = companyId,
                            BranchId = branchId,
                            FinYearId = finYearId,
                            ItemId = targetItemId,
                            ItemNo = 0,
                            Description = description,
                            Barcode = baseRow.Barcode,
                            ItemTypeId = resolvedItemType,
                            VendorId = resolvedVendor,
                            BrandId = resolvedBrand,
                            GroupId = resolvedGroup,
                            CategoryId = resolvedCategory,
                            BaseUnitId = resolvedBaseUnit,
                            ForCustomerType = "ALL",
                            NameInLocalLanguage = "",
                            HSNCode = baseRow.HSNCode ?? "",
                            Order_Cycle_Days = targetOrderCycleDays,
                            Box_Quantity = (int)targetBoxQty,
                            Is_Perishable = targetIsPerishable,
                            _Operation = "UPDATE"
                        };

                        var itemParams = BuildFilteredParametersForItem(STOREDPROCEDURE.POS_ItemMaster, updateItem, dbTransaction);
                        List<Item> updatedItems = DataConnection.Query<Item>(
                            STOREDPROCEDURE.POS_ItemMaster,
                            itemParams,
                            dbTransaction,
                            commandType: CommandType.StoredProcedure
                        ).ToList();

                        if (updatedItems.Count == 0)
                        {
                            throw new Exception("Failed to update item master using stored procedure");
                        }

                        summary.ItemsUpdated++;
                    }

                    // Preserve stock if in merge/update mode
                    var existingStocks = new Dictionary<int, ItemMasterPriceSettings>();
                    if (isUpdate)
                    {
                        // Query existing price settings using stored procedure
                        var existingPriceSettingsParam = new ItemMasterPriceSettings
                        {
                            _Operation = "GETBYID",
                            ItemId = targetItemId,
                            CompanyId = companyId,
                            BranchId = branchId,
                            FinYearId = finYearId
                        };

                        var priceParams = BuildFilteredParameters(STOREDPROCEDURE.POS_ItemMasterPriceSettings, existingPriceSettingsParam, dbTransaction);
                        var existingPriceList = DataConnection.Query<ItemMasterPriceSettings>(
                            STOREDPROCEDURE.POS_ItemMasterPriceSettings,
                            priceParams,
                            dbTransaction,
                            commandType: CommandType.StoredProcedure
                        ).ToList();

                        foreach (var st in existingPriceList)
                        {
                            existingStocks[st.UnitId] = st;
                        }

                        // Delete existing price settings for this item using stored procedure
                        var deletePriceSettings = new ItemMasterPriceSettings
                        {
                            _Operation = "DELETE",
                            ItemId = targetItemId,
                            CompanyId = companyId,
                            BranchId = branchId,
                            FinYearId = finYearId
                        };

                        var deleteParams = BuildFilteredParameters(STOREDPROCEDURE.POS_ItemMasterPriceSettings, deletePriceSettings, dbTransaction);
                        DataConnection.Execute(
                            STOREDPROCEDURE.POS_ItemMasterPriceSettings,
                            deleteParams,
                            dbTransaction,
                            commandType: CommandType.StoredProcedure
                        );
                    }

                    // 3. Save PriceSettings for each UOM using stored procedure
                    string branchName = DataConnection.QueryFirstOrDefault<string>(
                        "SELECT BranchName FROM Branches WHERE id = @BranchId",
                        new { BranchId = branchId },
                        dbTransaction
                      ) ?? "Main Branch";

                    foreach (var uomRow in uomRows)
                    {
                        int uomUnitId = ResolveOrCreateUnit(uomRow.Unit, dbTransaction);
                        bool isUnitBase = (uomRow == baseRow);

                        // Preserve stock if updating
                        double currentStock = uomRow.OpnStk;
                        double currentStockValue = uomRow.OpnStk * uomRow.Cost;
                        double currentOrderedStock = 0;
                        double currentOpeningCost = 0;
                        double currentOpnValue = 0;
                        DateTime? currentOpnDate = null;

                        if (isUpdate && existingStocks.TryGetValue(uomUnitId, out var cachedStock))
                        {
                            currentStock = Convert.ToDouble(cachedStock.Stock);
                            currentStockValue = Convert.ToDouble(cachedStock.StockValue);
                            currentOrderedStock = Convert.ToDouble(cachedStock.OrderedStock);
                            currentOpeningCost = Convert.ToDouble(cachedStock.OpeningCost);
                            currentOpnValue = Convert.ToDouble(cachedStock.OpnValue);
                            currentOpnDate = cachedStock.OpnDate;
                        }

                        // Tax calculation (inclusive vs exclusive)
                        double taxAmount = 0.0;
                        if (string.Equals(uomRow.TaxType, "INCL", StringComparison.OrdinalIgnoreCase))
                        {
                            taxAmount = (uomRow.RetailPrice * uomRow.TaxPer) / (100.0 + uomRow.TaxPer);
                        }
                        else
                        {
                            taxAmount = (uomRow.RetailPrice * uomRow.TaxPer) / 100.0;
                        }

                        var unitPriceSettings = new ItemMasterPriceSettings
                        {
                            CompanyId = companyId,
                            BranchId = branchId,
                            BranchName = branchName,
                            FinYearId = finYearId,
                            ItemId = targetItemId,
                            UnitId = uomUnitId,
                            Unit = uomRow.Unit,
                            Packing = uomRow.Packing,
                            Cost = uomRow.Cost,
                            OpeningCost = currentOpeningCost,
                            OpnValue = currentOpnValue,
                            OpnDate = currentOpnDate,
                            MarginPer = uomRow.RetailPrice > 0 ? ((uomRow.RetailPrice - uomRow.Cost) / uomRow.RetailPrice * 100.0) : 0,
                            MarginAmt = uomRow.RetailPrice - uomRow.Cost,
                            RetailPrice = uomRow.RetailPrice,
                            WholeSalePrice = uomRow.WholeSalePrice > 0 ? uomRow.WholeSalePrice : uomRow.RetailPrice,
                            CreditPrice = uomRow.CreditPrice > 0 ? uomRow.CreditPrice : uomRow.RetailPrice,
                            CardPrice = uomRow.CardPrice > 0 ? uomRow.CardPrice : uomRow.RetailPrice,
                            MRP = uomRow.MRP > 0 ? uomRow.MRP : uomRow.RetailPrice,
                            MinPrice = uomRow.MinPrice > 0 ? uomRow.MinPrice : uomRow.RetailPrice,
                            StaffPrice = uomRow.StaffPrice > 0 ? uomRow.StaffPrice : uomRow.RetailPrice,
                            MDRetailPrice = 0.0,
                            MDWalkinPrice = 0.0,
                            MDCreditPrice = 0.0,
                            MDMrpPrice = 0.0,
                            MDCardPrice = 0.0,
                            MDStaffPrice = 0.0,
                            MDMinPrice = 0.0,
                            Stock = currentStock,
                            OrderedStock = currentOrderedStock,
                            StockValue = currentStockValue,
                            ReOrder = uomRow.ReOrder,
                            BarCode = baseRow.Barcode,
                            AliasBarcode = isUnitBase ? "" : (string.Equals(uomRow.Barcode, baseRow.Barcode, StringComparison.OrdinalIgnoreCase) ? "" : uomRow.Barcode),
                            OpnStk = uomRow.OpnStk,
                            IsBaseUnit = isUnitBase ? "Y" : "N",
                            Costing = "AVERAGE",
                            TaxPer = uomRow.TaxPer,
                            TaxAmt = taxAmount,
                            TaxType = string.IsNullOrWhiteSpace(uomRow.TaxType) ? "EXCL" : uomRow.TaxType,
                            _Operation = "CREATE"
                        };

                        var createParams = BuildFilteredParameters(STOREDPROCEDURE.POS_ItemMasterPriceSettings, unitPriceSettings, dbTransaction);
                        List<ItemMasterPriceSettings> savedPriceSettings = DataConnection.Query<ItemMasterPriceSettings>(
                            STOREDPROCEDURE.POS_ItemMasterPriceSettings,
                            createParams,
                            dbTransaction,
                            commandType: CommandType.StoredProcedure
                        ).ToList();

                        if (savedPriceSettings.Count == 0)
                        {
                            throw new Exception($"Failed to save price settings for unit: {uomRow.Unit}");
                        }

                        uomRow.StatusMessage = "Succeeded";
                        summary.SucceededCount++;
                    }

                    // Save Alternative Barcodes if mapped
                    string altBarcodesStr = uomRows
                        .Select(r => r.AlternativeBarcodes)
                        .FirstOrDefault(s => s != null);

                    if (altBarcodesStr != null)
                    {
                        // Clean existing alternative barcodes for this Item
                        DataConnection.Execute(
                            "_POS_ItemAlternativeBarcode",
                            new { ItemId = targetItemId, Barcode = "", _Operation = "DELETE" },
                            dbTransaction,
                            commandType: CommandType.StoredProcedure
                        );

                        var altBarcodesList = altBarcodesStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var altBc in altBarcodesList)
                        {
                            var cleanAltBc = altBc.Trim();
                            if (!string.IsNullOrWhiteSpace(cleanAltBc))
                            {
                                DataConnection.Execute(
                                    "_POS_ItemAlternativeBarcode",
                                    new { ItemId = targetItemId, Barcode = cleanAltBc, _Operation = "CREATE" },
                                    dbTransaction,
                                    commandType: CommandType.StoredProcedure
                                );
                            }
                        }
                    }

                    processedCount++;
                    progressCallback?.Invoke(processedCount, totalGroups);
                }

                dbTransaction.Commit();
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                System.Diagnostics.Debug.WriteLine($"Transaction rolled back: {ex.Message}");
                
                // Mark all rows as failed due to database rollback
                foreach (var row in rawRows)
                {
                    row.HasError = true;
                    row.StatusMessage = $"Import rolled back due to error: {ex.Message}";
                }
                summary.SucceededCount = 0;
                summary.FailedCount = rawRows.Count;
                summary.NewItemsCreated = 0;
                summary.ItemsUpdated = 0;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            summary.Rows = rawRows;
            return summary;
        }

        // Fetch product list and format it into a DataTable for Excel/CSV export
        public DataTable GetProductsForExport(int categoryId, int brandId, int groupId, string searchPattern)
        {
            DataTable dt = new DataTable("Products");
            
            // Define standard columns that match our expected import structure
            dt.Columns.Add("Barcode", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("ItemType", typeof(string));
            dt.Columns.Add("Category", typeof(string));
            dt.Columns.Add("Brand", typeof(string));
            dt.Columns.Add("Group", typeof(string));
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("Packing", typeof(double));
            dt.Columns.Add("IsBaseUnit", typeof(string));
            dt.Columns.Add("Cost", typeof(double));
            dt.Columns.Add("RetailPrice", typeof(double));
            dt.Columns.Add("WholeSalePrice", typeof(double));
            dt.Columns.Add("MRP", typeof(double));
            dt.Columns.Add("CardPrice", typeof(double));
            dt.Columns.Add("CreditPrice", typeof(double));
            dt.Columns.Add("StaffPrice", typeof(double));
            dt.Columns.Add("MinPrice", typeof(double));
            dt.Columns.Add("OpeningStock", typeof(double));
            dt.Columns.Add("ReorderLevel", typeof(double));
            dt.Columns.Add("TaxType", typeof(string));
            dt.Columns.Add("TaxPer", typeof(double));
            dt.Columns.Add("HSNCode", typeof(string));
            dt.Columns.Add("AlternativeBarcodes", typeof(string));
            dt.Columns.Add("OrderCycleDays", typeof(int));
            dt.Columns.Add("BoxQty", typeof(double));
            dt.Columns.Add("Perishable", typeof(string));

            int branchId = GetBranchId();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                // Dynamic SQL query based on filters
                StringBuilder sql = new StringBuilder(@"
                    SELECT 
                        CASE WHEN PS.IsBaseUnit = 'Y' THEN PS.BarCode ELSE COALESCE(NULLIF(PS.AliasBarcode, ''), PS.BarCode) END AS Barcode,
                        IM.Description AS Description,
                        IMT.ItemType AS ItemType,
                        CG.CategoryName AS Category,
                        BR.BrandName AS Brand,
                        GP.GroupName AS [Group],
                        PS.Unit AS Unit,
                        PS.Packing AS Packing,
                        PS.IsBaseUnit AS IsBaseUnit,
                        PS.Cost AS Cost,
                        PS.RetailPrice AS RetailPrice,
                        PS.WholeSalePrice AS WholeSalePrice,
                        PS.MRP AS MRP,
                        PS.CardPrice AS CardPrice,
                        PS.CreditPrice AS CreditPrice,
                        PS.StaffPrice AS StaffPrice,
                        PS.MinPrice AS MinPrice,
                        ISNULL(PS.Stock, 0) AS OpeningStock,
                        PS.ReOrder AS ReorderLevel,
                        PS.TaxType AS TaxType,
                        PS.TaxPer AS TaxPer,
                        IM.HSNCode AS HSNCode,
                        CASE WHEN PS.IsBaseUnit = 'Y' THEN 
                            COALESCE(STUFF((SELECT ',' + Barcode FROM ItemAlternativeBarcode WHERE ItemId = IM.ItemId FOR XML PATH('')), 1, 1, ''), '')
                        ELSE '' END AS AlternativeBarcodes,
                        IM.Order_Cycle_Days AS OrderCycleDays,
                        IM.Box_Quantity AS BoxQty,
                        CASE WHEN IM.Is_Perishable = 1 THEN 'Y' ELSE 'N' END AS Perishable
                    FROM ItemMaster IM
                    INNER JOIN PriceSettings PS ON IM.ItemId = PS.ItemId
                    LEFT JOIN Category CG ON IM.CategoryId = CG.Id
                    LEFT JOIN Brands BR ON IM.BrandId = BR.Id
                    LEFT JOIN [Group] GP ON IM.GroupId = GP.Id
                    LEFT JOIN ItemTypes IMT ON IM.ItemTypeId = IMT.Id
                    WHERE IM.Active = 0 AND PS.BranchId = @BranchId");

                var parameters = new DynamicParameters();
                parameters.Add("BranchId", branchId);
                if (categoryId > 0)
                {
                    sql.Append(" AND IM.CategoryId = @CategoryId");
                    parameters.Add("CategoryId", categoryId);
                }
                else if (categoryId == -1)
                {
                    sql.Append(" AND (IM.CategoryId IS NULL OR IM.CategoryId = 0)");
                }

                if (brandId > 0)
                {
                    sql.Append(" AND IM.BrandId = @BrandId");
                    parameters.Add("BrandId", brandId);
                }
                else if (brandId == -1)
                {
                    sql.Append(" AND (IM.BrandId IS NULL OR IM.BrandId = 0)");
                }

                if (groupId > 0)
                {
                    sql.Append(" AND IM.GroupId = @GroupId");
                    parameters.Add("GroupId", groupId);
                }
                else if (groupId == -1)
                {
                    sql.Append(" AND (IM.GroupId IS NULL OR IM.GroupId = 0)");
                }

                if (!string.IsNullOrWhiteSpace(searchPattern))
                {
                    sql.Append(" AND (IM.Description LIKE @SearchPattern OR PS.BarCode LIKE @SearchPattern OR PS.AliasBarcode LIKE @SearchPattern)");
                    parameters.Add("SearchPattern", $"%{searchPattern.Trim()}%");
                }

                sql.Append(" ORDER BY IM.Description, PS.Packing");

                var items = DataConnection.Query<dynamic>(sql.ToString(), parameters);

                foreach (var item in items)
                {
                    dt.Rows.Add(
                        Convert.ToString(item.Barcode),
                        Convert.ToString(item.Description),
                        Convert.ToString(item.ItemType),
                        Convert.ToString(item.Category),
                        Convert.ToString(item.Brand),
                        Convert.ToString(item.Group),
                        Convert.ToString(item.Unit),
                        Convert.ToDouble(item.Packing),
                        Convert.ToString(item.IsBaseUnit),
                        Convert.ToDouble(item.Cost),
                        Convert.ToDouble(item.RetailPrice),
                        Convert.ToDouble(item.WholeSalePrice),
                        Convert.ToDouble(item.MRP),
                        Convert.ToDouble(item.CardPrice),
                        Convert.ToDouble(item.CreditPrice),
                        Convert.ToDouble(item.StaffPrice),
                        Convert.ToDouble(item.MinPrice),
                        Convert.ToDouble(item.OpeningStock),
                        Convert.ToDouble(item.ReorderLevel),
                        Convert.ToString(item.TaxType),
                        Convert.ToDouble(item.TaxPer),
                        Convert.ToString(item.HSNCode),
                        Convert.ToString(item.AlternativeBarcodes),
                        Convert.ToInt32(item.OrderCycleDays),
                        Convert.ToDouble(item.BoxQty),
                        Convert.ToString(item.Perishable)
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting items: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return dt;
        }

        // Self-contained CSV Parsing Utility class (RFC 4180 compliant)
        public static class CSVHelper
        {
            public static List<string[]> ReadCSV(string filePath)
            {
                var rows = new List<string[]>();
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        rows.Add(ParseCSVLine(line));
                    }
                }
                return rows;
            }

            private static string[] ParseCSVLine(string line)
            {
                var parts = new List<string>();
                bool inQuotes = false;
                StringBuilder currentPart = new StringBuilder();

                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];

                    if (c == '"')
                    {
                        // Handle double quotes as escape for a quote
                        if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                        {
                            currentPart.Append('"');
                            i++; // skip next quote
                        }
                        else
                        {
                            inQuotes = !inQuotes;
                        }
                    }
                    else if (c == ',' && !inQuotes)
                    {
                        parts.Add(currentPart.ToString());
                        currentPart.Clear();
                    }
                    else
                    {
                        currentPart.Append(c);
                    }
                }
                parts.Add(currentPart.ToString());
                return parts.ToArray();
            }

            public static void WriteCSV(DataTable dt, string filePath)
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Write headers
                    string[] headers = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
                    writer.WriteLine(string.Join(",", headers.Select(EscapeCSV)));

                    // Write rows
                    foreach (DataRow row in dt.Rows)
                    {
                        var fields = new string[dt.Columns.Count];
                        for (int colIndex = 0; colIndex < dt.Columns.Count; colIndex++)
                        {
                            string colName = dt.Columns[colIndex].ColumnName;
                            string val = Convert.ToString(row[colIndex]);
                            
                            // Prevent Excel from turning long codes (Barcodes, HSN) into scientific notation
                            if ((colName == "Barcode" || colName == "HSNCode") && !string.IsNullOrWhiteSpace(val))
                            {
                                // Only format if it isn't already formatted as an Excel formula
                                if (!val.StartsWith("="))
                                {
                                    val = $"=\"{val}\"";
                                }
                            }
                            fields[colIndex] = val;
                        }
                        writer.WriteLine(string.Join(",", fields.Select(EscapeCSV)));
                    }
                }
            }

            private static string EscapeCSV(string field)
            {
                if (string.IsNullOrEmpty(field)) return "";
                
                bool needsQuotes = field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r");
                if (needsQuotes)
                {
                    return "\"" + field.Replace("\"", "\"\"") + "\"";
                }
                return field;
            }
        }
    }
}
