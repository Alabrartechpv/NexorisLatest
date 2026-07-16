namespace Repository.Accounts
{
    public class ContraVoucherRepository : JournalVoucherRepository
    {
        protected override string VoucherType => "Contra";
        protected override string VoucherNumberPrefix => "CV";
    }
}
