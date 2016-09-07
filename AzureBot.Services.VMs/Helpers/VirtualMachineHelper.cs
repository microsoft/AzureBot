namespace AzureBot.Helpers
{
    using System.Collections.Generic;
    using Forms;
    using Models;
    internal class VirtualMachineHelper
    {
        internal static IEnumerable<VirtualMachinePowerState> RetrieveValidPowerStateByOperation(Operations operation)
        {
            var validPowerStates = new List<VirtualMachinePowerState>();

            switch (operation)
            {
                case Operations.Start:
                    validPowerStates.Add(VirtualMachinePowerState.Stopped);
                    validPowerStates.Add(VirtualMachinePowerState.Deallocated);
                    break;

                case Operations.Shutdown:
                    validPowerStates.Add(VirtualMachinePowerState.Running);
                    break;

                case Operations.Stop:
                    validPowerStates.Add(VirtualMachinePowerState.Running);
                    validPowerStates.Add(VirtualMachinePowerState.Stopped);
                    break;

                default:
                    validPowerStates.Add(VirtualMachinePowerState.Unknown);
                    break;
            }

            return validPowerStates;
        }

        internal static object RetrieveOperationTextByOperation(Operations operation)
        {
            switch (operation)
            {
                case Operations.Start:
                    return "started";

                case Operations.Shutdown:
                    return "shut down";

                case Operations.Stop:
                    return "stopped";

                default:
                    return "unknown";
            }
        }
    }
}
