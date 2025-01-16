using AutoMapper;
using CoffeeHouseAPI.DTOs.Address;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

            if (loginResponse == null)
            {
                return Unauthorized(new APIResponseBase
                {
                    IsSuccess = false,
                    Status = StatusCodes.Status401Unauthorized,
                    Message = GENERATE_DATA.STATUSCODE_MESSAGE(StatusCodes.Status401Unauthorized)
                });
            }

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

            if (loginResponse == null)
            {
                return Unauthorized(new APIResponseBase
                {
                    IsSuccess = false,
                    Status = StatusCodes.Status401Unauthorized,
                    Message = GENERATE_DATA.STATUSCODE_MESSAGE(StatusCodes.Status401Unauthorized)
                });
            }

            request.CustomerId = loginResponse.Id;

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
