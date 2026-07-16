using ModelClass;
using ModelClass.Accounts;
using ModelClass.Report;
using Repository.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repository.ReportRepository
{
    public class CombinedPartyBalanceReportRepository
    {
        private readonly ManualPartyBalanceRepository _manualRepository;
        private readonly CustomerOutstandingReportRepository _customerRepository;
        private readonly VendorOutstandingReportRepository _vendorRepository;
        private readonly Dictionary<string, string> _customerNameMap;
        private readonly Dictionary<string, string> _vendorNameMap;
        private readonly List<string> _customerMasterNames;
        private readonly List<string> _vendorMasterNames;

        public CombinedPartyBalanceReportRepository()
        {
            _manualRepository = new ManualPartyBalanceRepository();
            _customerRepository = new CustomerOutstandingReportRepository();
            _vendorRepository = new VendorOutstandingReportRepository();
            _customerNameMap = BuildNameMap(_customerRepository.GetCustomers().Select(x => x.LedgerName));
            _vendorNameMap = BuildNameMap(_vendorRepository.GetVendors().Select(x => x.LedgerName));
            _customerMasterNames = _customerNameMap.Values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            _vendorMasterNames = _vendorNameMap.Values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public List<CombinedPartyBalanceReportRow> GetReport(CombinedPartyBalanceReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            string partyType = NormalizePartyType(filter.PartyType);
            string searchText = NormalizeText(filter.SearchText);
            Dictionary<string, CombinedPartyBalanceAccumulator> balances = new Dictionary<string, CombinedPartyBalanceAccumulator>();

            AddManualBalances(balances, partyType, searchText, filter.OpenOnly, filter.FromDate, filter.ToDate);

            if (ShouldIncludeCustomer(partyType))
            {
                AddCustomerOutstanding(balances, searchText, filter.OpenOnly, filter.FromDate, filter.ToDate);
            }

            if (ShouldIncludeVendor(partyType))
            {
                AddVendorOutstanding(balances, searchText, filter.OpenOnly, filter.FromDate, filter.ToDate);
            }

            IEnumerable<CombinedPartyBalanceReportRow> rows = balances.Values
                .Select(x => x.ToRow())
                .Where(x => !filter.MatchedOnly || string.Equals(x.MatchStatus, "Matched", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.PartyType)
                .ThenBy(x => x.PartyName);

            return rows.ToList();
        }

        private void AddManualBalances(
            IDictionary<string, CombinedPartyBalanceAccumulator> balances,
            string partyType,
            string searchText,
            bool openOnly,
            DateTime fromDate,
            DateTime toDate)
        {
            List<ManualPartyBalanceEntry> entries = _manualRepository.GetEntries(
                partyType,
                null,
                searchText,
                openOnly,
                fromDate.Date,
                toDate.Date);

            foreach (ManualPartyBalanceEntry entry in entries)
            {
                string normalizedPartyType = NormalizePartyType(entry.PartyType);
                string normalizedPartyName = ResolvePartyName(normalizedPartyType, entry.PartyName, true);
                if (string.IsNullOrWhiteSpace(normalizedPartyType) || string.IsNullOrWhiteSpace(normalizedPartyName))
                    continue;

                CombinedPartyBalanceAccumulator accumulator = GetOrCreate(balances, normalizedPartyType, normalizedPartyName);
                accumulator.ManualBalance += entry.RemainingAmount;
                accumulator.ManualEntryCount++;

                if (!accumulator.ManualLastDate.HasValue || entry.EntryDate > accumulator.ManualLastDate.Value)
                {
                    accumulator.ManualLastDate = entry.EntryDate;
                }
            }
        }

        private void AddCustomerOutstanding(
            IDictionary<string, CombinedPartyBalanceAccumulator> balances,
            string searchText,
            bool openOnly,
            DateTime fromDate,
            DateTime toDate)
        {
            List<CustomerOutstandingReportRow> rows = _customerRepository.GetReport(new CustomerOutstandingReportFilter
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                CompanyId = SessionContext.CompanyId,
                BranchId = SessionContext.BranchId,
                FinYearId = SessionContext.FinYearId,
                LedgerId = 0
            });

            IEnumerable<CustomerOutstandingReportRow> filteredRows = rows;
            if (openOnly)
            {
                filteredRows = filteredRows.Where(x => x.Balance > 0);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredRows = filteredRows.Where(x =>
                    !string.IsNullOrWhiteSpace(x.LedgerName) &&
                    x.LedgerName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            foreach (CustomerOutstandingReportRow entry in filteredRows)
            {
                string normalizedPartyName = ResolvePartyName("Customer", entry.LedgerName, false);
                if (string.IsNullOrWhiteSpace(normalizedPartyName))
                    continue;

                CombinedPartyBalanceAccumulator accumulator = GetOrCreate(balances, "Customer", normalizedPartyName);
                accumulator.OutstandingBalance += entry.Balance;
                accumulator.OutstandingEntryCount++;

                DateTime? referenceDate = entry.BillDate ?? entry.DueDate;
                if (referenceDate.HasValue && (!accumulator.OutstandingLastDate.HasValue || referenceDate.Value > accumulator.OutstandingLastDate.Value))
                {
                    accumulator.OutstandingLastDate = referenceDate;
                }
            }
        }

        private void AddVendorOutstanding(
            IDictionary<string, CombinedPartyBalanceAccumulator> balances,
            string searchText,
            bool openOnly,
            DateTime fromDate,
            DateTime toDate)
        {
            List<VendorOutstandingReportRow> rows = _vendorRepository.GetReport(new VendorOutstandingReportFilter
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                CompanyId = SessionContext.CompanyId,
                BranchId = SessionContext.BranchId,
                FinYearId = SessionContext.FinYearId,
                LedgerId = 0
            });

            IEnumerable<VendorOutstandingReportRow> filteredRows = rows;
            if (openOnly)
            {
                filteredRows = filteredRows.Where(x => x.Balance > 0);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredRows = filteredRows.Where(x =>
                    !string.IsNullOrWhiteSpace(x.LedgerName) &&
                    x.LedgerName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            foreach (VendorOutstandingReportRow entry in filteredRows)
            {
                string normalizedPartyName = ResolvePartyName("Vendor", entry.LedgerName, false);
                if (string.IsNullOrWhiteSpace(normalizedPartyName))
                    continue;

                CombinedPartyBalanceAccumulator accumulator = GetOrCreate(balances, "Vendor", normalizedPartyName);
                accumulator.OutstandingBalance += entry.Balance;
                accumulator.OutstandingEntryCount++;

                if (entry.VoucherDate.HasValue && (!accumulator.OutstandingLastDate.HasValue || entry.VoucherDate.Value > accumulator.OutstandingLastDate.Value))
                {
                    accumulator.OutstandingLastDate = entry.VoucherDate.Value;
                }
            }
        }

        private static CombinedPartyBalanceAccumulator GetOrCreate(
            IDictionary<string, CombinedPartyBalanceAccumulator> balances,
            string partyType,
            string partyName)
        {
            string key = partyType.Trim().ToUpperInvariant() + "|" + partyName.Trim().ToUpperInvariant();

            CombinedPartyBalanceAccumulator accumulator;
            if (!balances.TryGetValue(key, out accumulator))
            {
                accumulator = new CombinedPartyBalanceAccumulator
                {
                    PartyType = partyType,
                    PartyName = partyName
                };

                balances.Add(key, accumulator);
            }

            return accumulator;
        }

        private static bool ShouldIncludeCustomer(string partyType)
        {
            return string.IsNullOrWhiteSpace(partyType) ||
                   string.Equals(partyType, "Customer", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ShouldIncludeVendor(string partyType)
        {
            return string.IsNullOrWhiteSpace(partyType) ||
                   string.Equals(partyType, "Vendor", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizePartyType(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "All", StringComparison.OrdinalIgnoreCase))
                return null;

            return value.Trim();
        }

        private static string NormalizeText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return string.Join(" ", value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private string ResolvePartyName(string partyType, string rawName, bool allowApproximate)
        {
            string normalizedName = NormalizeText(rawName);
            if (string.IsNullOrWhiteSpace(normalizedName))
                return null;

            Dictionary<string, string> nameMap = GetNameMap(partyType);
            if (nameMap == null || nameMap.Count == 0)
                return normalizedName;

            string exactKey = BuildLookupKey(normalizedName);
            string resolvedName;
            if (nameMap.TryGetValue(exactKey, out resolvedName))
                return resolvedName;

            if (!allowApproximate)
                return normalizedName;

            List<string> candidates = GetMasterNames(partyType)
                .Where(x => IsCloseMatch(normalizedName, x))
                .ToList();

            return candidates.Count == 1 ? candidates[0] : normalizedName;
        }

        private Dictionary<string, string> GetNameMap(string partyType)
        {
            if (string.Equals(partyType, "Customer", StringComparison.OrdinalIgnoreCase))
                return _customerNameMap;

            if (string.Equals(partyType, "Vendor", StringComparison.OrdinalIgnoreCase))
                return _vendorNameMap;

            return null;
        }

        private List<string> GetMasterNames(string partyType)
        {
            if (string.Equals(partyType, "Customer", StringComparison.OrdinalIgnoreCase))
                return _customerMasterNames;

            if (string.Equals(partyType, "Vendor", StringComparison.OrdinalIgnoreCase))
                return _vendorMasterNames;

            return new List<string>();
        }

        private static Dictionary<string, string> BuildNameMap(IEnumerable<string> names)
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string name in names.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                string normalizedName = NormalizeText(name);
                string key = BuildLookupKey(normalizedName);
                if (!map.ContainsKey(key))
                {
                    map.Add(key, normalizedName);
                }
            }

            return map;
        }

        private static string BuildLookupKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return new string(value
                .ToUpperInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray());
        }

        private static bool IsCloseMatch(string source, string target)
        {
            string sourceKey = BuildLookupKey(source);
            string targetKey = BuildLookupKey(target);

            if (string.IsNullOrWhiteSpace(sourceKey) || string.IsNullOrWhiteSpace(targetKey))
                return false;

            if (sourceKey[0] != targetKey[0])
                return false;

            if (Math.Abs(sourceKey.Length - targetKey.Length) > 2)
                return false;

            return ComputeLevenshteinDistance(sourceKey, targetKey) <= 2;
        }

        private static int ComputeLevenshteinDistance(string source, string target)
        {
            int[,] distance = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; i++)
            {
                distance[i, 0] = i;
            }

            for (int j = 0; j <= target.Length; j++)
            {
                distance[0, j] = j;
            }

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[source.Length, target.Length];
        }

        private sealed class CombinedPartyBalanceAccumulator
        {
            public string PartyType { get; set; }
            public string PartyName { get; set; }
            public decimal ManualBalance { get; set; }
            public decimal OutstandingBalance { get; set; }
            public int ManualEntryCount { get; set; }
            public int OutstandingEntryCount { get; set; }
            public DateTime? ManualLastDate { get; set; }
            public DateTime? OutstandingLastDate { get; set; }

            public CombinedPartyBalanceReportRow ToRow()
            {
                return new CombinedPartyBalanceReportRow
                {
                    PartyType = PartyType,
                    PartyName = PartyName,
                    ManualBalance = ManualBalance,
                    OutstandingBalance = OutstandingBalance,
                    TotalBalance = ManualBalance + OutstandingBalance,
                    ManualEntryCount = ManualEntryCount,
                    OutstandingEntryCount = OutstandingEntryCount,
                    ManualLastDate = ManualLastDate,
                    OutstandingLastDate = OutstandingLastDate,
                    MatchStatus = ManualEntryCount > 0 && OutstandingEntryCount > 0
                        ? "Matched"
                        : ManualEntryCount > 0
                            ? "Manual Only"
                            : "Outstanding Only"
                };
            }
        }
    }
}
