using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.Services.VoucherService
{
    public interface IVoucherService
    {
        bool ValidateVoucher(Voucher voucher);

        List<Voucher> GetAllVoucherWithRalate(bool asNoTracking);

        decimal PriceAfterApplyVoucher(decimal totalPrice, Voucher voucher);
    }
}
