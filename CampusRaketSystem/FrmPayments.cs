namespace CampusRaketSystem;

public class FrmPayments : FrmTransactionManagerBase
{
    public FrmPayments()
        : base(new PaymentTransactionService(), "Payment Transactions", "Record, revise, and remove payment entries with validation and instant refresh.")
    {
    }
}
