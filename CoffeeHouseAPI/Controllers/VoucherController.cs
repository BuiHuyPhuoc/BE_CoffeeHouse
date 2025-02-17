﻿using AutoMapper;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Voucher;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.VoucherService;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MasterAuth]
    public class VoucherController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _mapper;
        readonly IVoucherService _voucherService;

        public VoucherController(DbcoffeeHouseContext context, IMapper mapper, IVoucherService voucherService)
        {
            _context = context;
            _mapper = mapper;
            _voucherService = voucherService;
        }

        [HttpGet]
        [Route("CustomerVoucher")]
        public async Task<IActionResult> CustomerVoucher()
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();

            var vouchers = await _context.Vouchers.Where(x => x.IsActive && x.StartDate <= DateTime.Now).ToListAsync();

            List<VoucherDTO> voucherDTOs = new List<VoucherDTO>();

            foreach (var voucher in vouchers)
            {
                if (_voucherService.ValidateVoucher(voucher))
                {
                    var voucherUsedCount = _context.Orders.Where(x => x.VoucherId == voucher.Id).ToList();

                    if (voucherUsedCount.Count < voucher.LitmitPerUser)
                    {
                        voucherDTOs.Add(_mapper.Map<VoucherDTO>(voucher));
                    }
                }
                else
                {
                    if (voucher.IsActive == true)
                    {
                        voucher.IsActive = false;
                        await this.SaveChanges(_context);
                    }
                }
            }

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Value = voucherDTOs,
                Status = (int)HttpStatusCode.OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET)
            });
        }
    }
}
