using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.Services.VoucherService
{
    public class VoucherService : IVoucherService
    {
        readonly DbcoffeeHouseContext _context;
         
        public VoucherService(DbcoffeeHouseContext context)
        {
            _context = context;
        }

        public bool ValidateVoucher(Voucher voucher)
        {
            return (((voucher.EndDate != null && voucher.EndDate >= DateTime.Now) || voucher.EndDate == null)
                && voucher.StartDate <= DateTime.Now && voucher.LitmitPerUser > 0) ? true : false;
        }
    }
}
