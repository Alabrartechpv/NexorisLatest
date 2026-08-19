using Dapper;
using ModelClass.TransactionModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository
{
    public class SyncQueueRepository : BaseRepostitory
    {
        /// <summary>
        /// Atomically enqueues a transaction into the SyncQueue within an existing SqlTransaction using Stored Procedure.
        /// </summary>
        public static void EnqueueTransaction(
            IDbConnection conn,
            IDbTransaction trans,
            int branchId,
            string entityType,
            string entityId,
            Guid transactionGuid,
            string operation,
            int version = 1)
        {
            if (conn == null) throw new ArgumentNullException(nameof(conn));
            if (trans == null) throw new ArgumentNullException(nameof(trans));

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@_Operation", "ENQUEUE");
                parameters.Add("@BranchId", branchId);
                parameters.Add("@EntityType", entityType);
                parameters.Add("@EntityId", entityId);
                parameters.Add("@TransactionGuid", transactionGuid);
                parameters.Add("@OperationType", operation);
                parameters.Add("@Version", version);

                conn.Execute(
                    STOREDPROCEDURE.POS_SyncQueue,
                    parameters,
                    transaction: trans,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SyncQueueRepository.EnqueueTransaction] Warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Fetches existing TransactionGuid for a given entity using Stored Procedure.
        /// </summary>
        public static Guid? GetExistingGuid(
            IDbConnection conn,
            IDbTransaction trans,
            int branchId,
            string entityType,
            string entityId)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@_Operation", "GETGUID");
                parameters.Add("@BranchId", branchId);
                parameters.Add("@EntityType", entityType);
                parameters.Add("@EntityId", entityId);

                return conn.QueryFirstOrDefault<Guid?>(
                    STOREDPROCEDURE.POS_SyncQueue,
                    parameters,
                    transaction: trans,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves top pending or retryable sync items from the local SyncQueue using Stored Procedure.
        /// </summary>
        public List<SyncQueueModel> GetPendingItems(int topN = 50)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@_Operation", "GETPENDING");
                parameters.Add("@TopN", topN);

                return DataConnection.Query<SyncQueueModel>(
                    STOREDPROCEDURE.POS_SyncQueue,
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SyncQueueRepository.GetPendingItems] Error: {ex.Message}");
                return new List<SyncQueueModel>();
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        /// <summary>
        /// Updates the status of a SyncQueue record using Stored Procedure.
        /// </summary>
        public bool UpdateStatus(long syncId, string status, string errorMessage = null)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@_Operation", "UPDATESTATUS");
                parameters.Add("@SyncId", syncId);
                parameters.Add("@Status", status);
                parameters.Add("@ErrorMessage", string.IsNullOrEmpty(errorMessage) ? null : errorMessage);

                int rows = DataConnection.Execute(
                    STOREDPROCEDURE.POS_SyncQueue,
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return rows > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SyncQueueRepository.UpdateStatus] Error: {ex.Message}");
                return false;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }
    }
}
