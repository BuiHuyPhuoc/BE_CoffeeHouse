using AutoMapper;
using CoffeeHouseAPI.DTOs.Address;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MasterAuth]
    public class AddressController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _mapper;

        public AddressController(DbcoffeeHouseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
            
            if (currentDefaultAddress != null && request.IsDefault == true) {
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
    }
}
