using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.Services.VoucherService
{
    public interface IVoucherService
    {
        bool ValidateVoucher(Voucher voucher);
    }
}
