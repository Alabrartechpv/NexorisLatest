using System;
using System.Data;
using System.Data.SqlClient;
using ModelClass;

namespace Repository.ReportRepository
{
    public sealed class ActivityReportRepository : BaseRepostitory
    {
        public DataSet GetFilters()
        {
            return Read(STOREDPROCEDURE.POS_ActivityReportFilters);
        }

        public DataTable GetActivity(DateTime from, DateTime to, int? branchId, string user, string counter,
            string category, string search, string action, bool asOfNow)
        {
            using (DataSet result = Read(STOREDPROCEDURE.POS_ActivityReport, parameters =>
            {
                parameters.Add("@FromDate", SqlDbType.Date).Value = from.Date;
                parameters.Add("@ToDate", SqlDbType.Date).Value = to.Date;
                parameters.Add("@BranchId", SqlDbType.Int).Value = (object)branchId ?? DBNull.Value;
                AddOptionalText(parameters, "@UserName", 128, user);
                AddOptionalText(parameters, "@CounterName", 128, counter);
                AddOptionalText(parameters, "@Category", 24, category);
                AddOptionalText(parameters, "@Search", 200, search);
                AddOptionalText(parameters, "@ActionType", 100, action);
                parameters.Add("@AsOfNow", SqlDbType.Bit).Value = asOfNow;
                parameters.Add("@IncludeDetailKeys", SqlDbType.Bit).Value = true;
            }))
            {
                return DetachRows(result);
            }
        }

        public DataTable GetDetails(int? branchId, int? finYearId, int? counterId, string module, string documentNo,
            string source, string action, DateTime activityDate)
        {
            using (DataSet result = Read(STOREDPROCEDURE.POS_ActivityReportDetails, parameters =>
            {
                parameters.Add("@BranchId", SqlDbType.Int).Value = (object)branchId ?? DBNull.Value;
                parameters.Add("@FinYearId", SqlDbType.Int).Value = (object)finYearId ?? DBNull.Value;
                parameters.Add("@CounterId", SqlDbType.Int).Value = (object)counterId ?? DBNull.Value;
                AddOptionalText(parameters, "@Module", 100, module);
                AddOptionalText(parameters, "@DocumentNo", 100, documentNo);
                AddOptionalText(parameters, "@Source", 128, source);
                AddOptionalText(parameters, "@ActionType", 100, action);
                var timestamp = parameters.Add("@ActivityDate", SqlDbType.DateTime2);
                timestamp.Scale = 3;
                timestamp.Value = activityDate;
            }))
            {
                return DetachRows(result);
            }
        }

        private static DataTable DetachRows(DataSet result)
        {
            DataTable rows = result.Tables[0];
            result.Tables.Remove(rows);
            return rows;
        }

        private DataSet Read(string procedure, Action<SqlParameterCollection> addFilters = null)
        {
            if (!SessionContext.IsAdmin || SessionContext.UserId <= 0 || SessionContext.CompanyId <= 0)
                throw new UnauthorizedAccessException("The Activity Report requires an administrator for the current company.");

            using (var command = new SqlCommand(procedure, (SqlConnection)DataConnection))
            using (var adapter = new SqlDataAdapter(command))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 120;
                command.Parameters.Add("@CompanyId", SqlDbType.Int).Value = SessionContext.CompanyId;
                addFilters?.Invoke(command.Parameters);

                var result = new DataSet();
                try
                {
                    adapter.Fill(result);
                    return result;
                }
                catch
                {
                    result.Dispose();
                    throw;
                }
            }
        }

        private static void AddOptionalText(SqlParameterCollection parameters, string name, int length, string value)
        {
            parameters.Add(name, SqlDbType.NVarChar, length).Value =
                string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }
    }
}
