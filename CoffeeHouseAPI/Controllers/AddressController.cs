using AutoMapper;
using CoffeeHouseAPI.DTOs.Address;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.AddressService;
using CoffeeHouseAPI.Services.Firebase;
using CoffeeHouseLib.Models;
using Google.Apis.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MasterAuth]
    public class AddressController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _mapper;
        readonly FirebaseService _firebaseService;
        readonly IAddressService _addressService;

        public AddressController(DbcoffeeHouseContext context, IMapper mapper, FirebaseService firebaseService, IAddressService addressService)
        {
            _context = context;
            _mapper = mapper;
            _firebaseService = firebaseService;
            _addressService = addressService;
        }

        [HttpGet]
        [Route("GetAddress")]
        public ActionResult GetAddress()
        {
            var loginResponse = GetLoginResponseFromHttpContext();

            var addresses = _addressService.GetAddressWithRelate().Where(x => x.CustomerId == loginResponse.Id);

            var addressDTOs = _mapper.Map<List<AddressDTO>>(addresses);

            return Ok(new APIResponseBase
            {
                Status = StatusCodes.Status200OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Value = addressDTOs,
                IsSuccess = true
            });
        }

        [HttpPost]
        [Route("AddAddress")]
        public async Task<IActionResult> AddAddress([FromBody] AddressDTO request)
        {
            var loginResponse = GetLoginResponseFromHttpContext();

            request.CustomerId = loginResponse.Id;

            var currentDefaultAddress = _addressService.GetAddressWithRelate()
                                .Where(x => x.CustomerId == loginResponse.Id && x.IsDefault)
                                .FirstOrDefault();

            if (currentDefaultAddress != null && request.IsDefault == true)
            {
                // Set old address is not default
                currentDefaultAddress.IsDefault = false;
                await this.SaveChanges(_context);
            }
            
            var address = _mapper.Map<Address>(request);

            _context.Addresses.Add(address);
            await this.SaveChanges(_context);

            var addressDTO = _mapper.Map<AddressDTO>(address);

            return Ok(new APIResponseBase
            {
                Status = StatusCodes.Status200OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.POST),
                Value = addressDTO,
                IsSuccess = true
            });
        }

        [HttpPost]
        [Route("UpdateAddress")]
        public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] AddressDTO request)
        {
            var address = await _context.Addresses.Where(x => x.Id == addressId && x.IsValid).FirstOrDefaultAsync();
            var loginResponse = this.GetLoginResponseFromHttpContext();

            if (address == null || address.CustomerId != loginResponse.Id)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = GENERATE_DATA.API_ACTION_RESPONSE(false, API_ACTION.GET),
                });
            }

            address.IsValid = false;
            await this.SaveChanges(_context);

            request.Id = null;
            Address newAddress = _mapper.Map<Address>(request);

            if (address.IsDefault == true && request.IsDefault == false)
            {
                var nonDefaultAddress = _context.Addresses.Where(x => x.IsDefault == false && x.CustomerId == loginResponse.Id && x.IsValid).FirstOrDefault();

                if (nonDefaultAddress != null)
                {
                    nonDefaultAddress.IsDefault = true;
                    await this.SaveChanges(_context);
                }
            }
            else if (address.IsDefault == false && request.IsDefault == true)
            {
                var defaultAddress = _context.Addresses.Where(x => x.IsDefault == true && x.CustomerId == loginResponse.Id && x.IsValid).FirstOrDefault();

                if (defaultAddress != null)
                {
                    defaultAddress.IsDefault = false;
                    await this.SaveChanges(_context);
                }
            }

            _context.Addresses.Add(newAddress);
            await this.SaveChanges(_context);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)HttpStatusCode.OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.PUT)
            });
        }

        [HttpPost]
        [Route("DeleteAddress")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            var loginResponse = this.GetLoginResponseFromHttpContext();
            var address = _addressService.GetAddressWithRelate().Where(x => x.CustomerId == loginResponse.Id && x.Id == addressId).FirstOrDefault();

            if (address == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = GENERATE_DATA.API_ACTION_RESPONSE(false, API_ACTION.GET),
                });
            }

            if (address.IsDefault)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = "Địa chỉ này là mặc định, không thể xoá.",
                });
            }

            address.IsValid = false;
            await this.SaveChanges(_context);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)HttpStatusCode.OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.DELETE)
            });
        }
    }
}
