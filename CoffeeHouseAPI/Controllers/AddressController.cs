using AutoMapper;
using CoffeeHouseAPI.DTOs.Address;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.Firebase;
using CoffeeHouseLib.Models;
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

        public AddressController(DbcoffeeHouseContext context, IMapper mapper, FirebaseService firebaseService)
        {
            _context = context;
            _mapper = mapper;
            _firebaseService = firebaseService;
        }

        [HttpGet]
        [Route("GetAddress")]
        public ActionResult GetAddress()
        {
            var loginResponse = GetLoginResponseFromHttpContext();

            var addresses = _context.Addresses.Where(x => x.CustomerId == loginResponse.Id).ToList();

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

            var currentDefaultAddress = await _context.Addresses.Where(x => x.CustomerId == loginResponse.Id && x.IsDefault).FirstOrDefaultAsync();

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
        public async Task<IActionResult> UpdateAddress(int addressId, [FromBody]AddressDTO request)
        {
            var address = await _context.Addresses.Where(x => x.Id == addressId).FirstOrDefaultAsync();
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

            address.AddressNumber = request.AddressNumber;
            address.CustomerId = loginResponse.Id;
            address.PhoneNumber = request.PhoneNumber;
            address.FullName = request.FullName;

            if (address.IsDefault == true && request.IsDefault == false)
            {
                var nonDefaultAddress = _context.Addresses.Where(x => x.IsDefault == false && x.CustomerId == loginResponse.Id).FirstOrDefault();

                if (nonDefaultAddress == null) throw new Exception();

                nonDefaultAddress.IsDefault = true;
                await this.SaveChanges(_context);
            }
            else if (address.IsDefault == false && request.IsDefault == true) { 
                var defaultAddress = _context.Addresses.Where(x => x.IsDefault == true && x.CustomerId == loginResponse.Id).FirstOrDefault();

                if (defaultAddress != null)
                {
                    defaultAddress.IsDefault = false;
                    await this.SaveChanges(_context);
                }
            }

            address.IsDefault = request.IsDefault;
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
            var address = _context.Addresses.Where(x => x.Id == addressId && loginResponse.Id == x.CustomerId).FirstOrDefault();
            
            if (address == null) {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = GENERATE_DATA.API_ACTION_RESPONSE(false, API_ACTION.GET),
                });
            }

            if (address.IsDefault)
            {
                var nonDefaultAddress = _context.Addresses.Where(x => x.IsDefault == false && x.CustomerId == loginResponse.Id).FirstOrDefault();

                if (nonDefaultAddress == null) throw new Exception();

                nonDefaultAddress.IsDefault = true;
                await this.SaveChanges(_context);
            }

            _context.Remove(address);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)HttpStatusCode.OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.DELETE)
            });
        }
    }
}
