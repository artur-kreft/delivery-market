using System.Threading.Tasks;

namespace DeliveryMarket.Core.Shipments
{
    // Assumption: code is running on multiple services.
    // Because of that we can face data race conditions.
    // Possible scenarios:
    // 1. Shipper is accepting single offer and multiple
    // Carriers are accepting original request at almost
    // the same time
    // 2. Request status is changes to canceled/expired
    // and multiple Carriers are accepting original
    // request at almost the same time
    // DataCleaners will be executed by scheduler to resolve 
    // such conflicts

    public interface IDataCleanerService
    {
        Task ConfirmShipmentsAsync();
    }
}