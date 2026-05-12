namespace CampusRaketSystem;

public class FrmJobs : FrmTransactionManagerBase
{
    public FrmJobs()
        : base(new JobTransactionService(), "Job Transactions", "Create, update, and review job records using form-based inputs and a live transaction grid.")
    {
    }
}
