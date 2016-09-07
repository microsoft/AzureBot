namespace AzureBot.Models
{
    public enum VirtualMachinePowerState
    {
        Unknown = 0,
        Stopping,
        Stopped,
        Starting,
        Running,
        Deallocating,
        Deallocated
    }
}
