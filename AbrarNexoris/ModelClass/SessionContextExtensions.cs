using System;
using System.Data.SqlClient;

namespace ModelClass
{
    /// <summary>
    /// Extension methods to simplify working with SessionContext in common scenarios.
    /// </summary>
    public static class SessionContextExtensions
    {
        /// <summary>
        /// Adds standard session parameters to a SQL command.
        /// </summary>
        /// <param name="cmd">The SQL command to add parameters to</param>
        /// <param name="includeUserId">Whether to include UserId parameter (default: true)</param>
        public static void AddSessionParameters(this SqlCommand cmd, bool includeUserId = true)
        {
            if (cmd == null)
                throw new ArgumentNullException(nameof(cmd));

            SessionContext.ValidateSession();

            cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
            cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
            cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);

            if (includeUserId)
            {
                cmd.Parameters.AddWithValue("@UserId", SessionContext.UserId);
            }
        }

        /// <summary>
        /// Adds standard session parameters with custom parameter names.
        /// </summary>
        /// <param name="cmd">The SQL command to add parameters to</param>
        /// <param name="companyIdParam">Parameter name for CompanyId (e.g., "@_CompanyID")</param>
        /// <param name="branchIdParam">Parameter name for BranchId (e.g., "@_BranchID")</param>
        /// <param name="finYearIdParam">Parameter name for FinYearId (e.g., "@_FinYearID")</param>
        /// <param name="userIdParam">Parameter name for UserId (e.g., "@_UserID"), null to skip</param>
        public static void AddSessionParameters(
            this SqlCommand cmd,
            string companyIdParam,
            string branchIdParam,
            string finYearIdParam,
            string userIdParam = null)
        {
            if (cmd == null)
                throw new ArgumentNullException(nameof(cmd));

            SessionContext.ValidateSession();

            if (!string.IsNullOrEmpty(companyIdParam))
                cmd.Parameters.AddWithValue(companyIdParam, SessionContext.CompanyId);

            if (!string.IsNullOrEmpty(branchIdParam))
                cmd.Parameters.AddWithValue(branchIdParam, SessionContext.BranchId);

            if (!string.IsNullOrEmpty(finYearIdParam))
                cmd.Parameters.AddWithValue(finYearIdParam, SessionContext.FinYearId);

            if (!string.IsNullOrEmpty(userIdParam))
                cmd.Parameters.AddWithValue(userIdParam, SessionContext.UserId);
        }
    }

    /// <summary>
    /// Base class for models that require session context.
    /// </summary>
    public abstract class SessionAwareModel
    {
        /// <summary>
        /// Company ID from session context.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Branch ID from session context.
        /// </summary>
        public int BranchId { get; set; }

        /// <summary>
        /// Financial Year ID from session context.
        /// </summary>
        public int FinYearId { get; set; }

        /// <summary>
        /// User ID from session context.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Initializes the model with current session context values.
        /// </summary>
        public virtual void InitializeFromSession()
        {
            SessionContext.ValidateSession();

            CompanyId = SessionContext.CompanyId;
            BranchId = SessionContext.BranchId;
            FinYearId = SessionContext.FinYearId;
            UserId = SessionContext.UserId;
        }

        /// <summary>
        /// Initializes the model with current session context values if not already set.
        /// </summary>
        public virtual void InitializeFromSessionIfNotSet()
        {
            if (!SessionContext.IsInitialized)
                return;

            if (CompanyId == 0)
                CompanyId = SessionContext.CompanyId;

            if (BranchId == 0)
                BranchId = SessionContext.BranchId;

            if (FinYearId == 0)
                FinYearId = SessionContext.FinYearId;

            if (UserId == 0)
                UserId = SessionContext.UserId;
        }
    }
}
