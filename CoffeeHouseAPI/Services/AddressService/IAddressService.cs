using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.Services.AddressService
{
    public interface IAddressService
    {
        List<Address> GetAddressWithRelate(bool isTracking = false);
    }
}
