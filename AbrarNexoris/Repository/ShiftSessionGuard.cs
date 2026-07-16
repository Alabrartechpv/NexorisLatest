using ModelClass;

namespace Repository
{
    public static class ShiftSessionGuard
    {
        public static bool CanDoTransaction(out string errorMessage)
        {
            errorMessage = null;

            bool isBillingUser = SessionContext.UserLevel?.Equals("Cashier", System.StringComparison.OrdinalIgnoreCase) == true ||
                                 SessionContext.UserLevel?.Equals("Sales Man", System.StringComparison.OrdinalIgnoreCase) == true;

            if (!isBillingUser)
            {
                return true;
            }

            if (!SessionContext.CanDoTransaction(out errorMessage))
                return false;

            using (var sessionRepo = new ShiftSessionRepo())
            {
                return sessionRepo.IsCurrentSessionOpen(out errorMessage);
            }
        }
    }
}
