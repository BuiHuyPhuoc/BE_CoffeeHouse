using CoffeeHouseLib.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseAPI.Services.VoucherService
{
    public class VoucherService : IVoucherService
    {
        readonly DbcoffeeHouseContext _context;

        public VoucherService(DbcoffeeHouseContext context)
        {
            _context = context;
        }

        public List<Voucher> GetAllVoucherWithRalate(bool asNoTracking)
        {
            var query = _context.Vouchers.Where(x => x.IsActive);

            if (asNoTracking)
                query = query.AsNoTracking();

            List<Voucher> vouchers = new List<Voucher>();

            foreach (var item in query)
            {
                if (ValidateVoucher(item))
                {
                    vouchers.Add(item);
                }
            }

            return vouchers;
        }

        public decimal PriceAfterApplyVoucher(decimal totalPrice, Voucher voucher)
        {
            if (voucher.DiscountType == "PERCENT")
            {
                return totalPrice - (totalPrice * voucher.DiscountValue / 100);
            }
            else
            {
                return totalPrice - voucher.DiscountValue;
            }
        }

        public bool ValidateVoucher(Voucher voucher)
        {
            return (((voucher.EndDate != null && voucher.EndDate >= DateTime.Now) || voucher.EndDate == null)
                && voucher.StartDate <= DateTime.Now && voucher.LitmitPerUser > 0) ? true : false;
        }
    }
}
