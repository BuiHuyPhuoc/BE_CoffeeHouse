using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseLib.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseAPI.Services.AddressService
{
    public class AddressService : IAddressService
    {
        readonly DbcoffeeHouseContext _context;

        public AddressService(DbcoffeeHouseContext context)
        {
            _context = context;
        }

        public List<Address> GetAddressWithRelate(bool isTracking = false)
        {
            var addresses = _context.Addresses.Where(x => x.IsValid);
            if (isTracking) {
                addresses = addresses.AsNoTracking();
            }
            return addresses.ToList();
        }
    }
}
