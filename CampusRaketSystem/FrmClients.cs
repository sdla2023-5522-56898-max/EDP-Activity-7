namespace CampusRaketSystem;

public class FrmClients : FrmTransactionManagerBase
{
    public FrmClients()
        : base(new ClientTransactionService(), "Client Transactions", "Manage client records through a dedicated transaction form with validation and record history.")
    {
    }
}
