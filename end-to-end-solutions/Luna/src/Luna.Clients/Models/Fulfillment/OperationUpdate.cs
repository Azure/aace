namespace Luna.Clients.Models.Fulfillment
{
    /// <summary>
    /// Response payload for updating the status of an operation
    /// </summary>
    public class OperationUpdate
    {
        public string PlanId { get; set; }

        public int Quantity { get; set; }

        public OperationUpdateStatusEnum Status { get; set; }
    }
}
