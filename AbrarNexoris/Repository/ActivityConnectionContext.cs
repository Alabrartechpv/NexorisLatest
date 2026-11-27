using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using ModelClass;

namespace Repository
{
    public static class ActivityConnectionContext
    {
        public static void OnConnectionStateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState != ConnectionState.Open || SessionContext.UserId <= 0)
                return;

            SetActivityContext((SqlConnection)sender);
        }

        internal static void MarkDirectVoucherEdit(IDbTransaction transaction)
        {
            if (SessionContext.UserId > 0)
                SetActivityContext((SqlConnection)transaction.Connection, (SqlTransaction)transaction);
        }

        private static void SetActivityContext(SqlConnection connection, SqlTransaction edit = null)
        {
            using (var command = new SqlCommand(STOREDPROCEDURE.POS_SetActivityContext, connection, edit))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 5;
                command.Parameters.Add("@CompanyId", SqlDbType.Int).Value = SessionContext.CompanyId;
                command.Parameters.Add("@BranchId", SqlDbType.Int).Value = SessionContext.BranchId;
                command.Parameters.Add("@FinYearId", SqlDbType.Int).Value = SessionContext.FinYearId;
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = SessionContext.UserId;
                command.Parameters.Add("@CounterId", SqlDbType.Int).Value = SessionContext.CounterId;
                if (edit != null)
                    command.Parameters.Add("@DirectVoucherEdit", SqlDbType.Bit).Value = true;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    // Older databases can continue to use the application before the audit migration.
                    if (ex.Number != 2812 && ex.Number != 8144)
                        Trace.TraceWarning("Could not set POS activity context. SQL error {0}.", ex.Number);
                }
            }
        }
    }
}
