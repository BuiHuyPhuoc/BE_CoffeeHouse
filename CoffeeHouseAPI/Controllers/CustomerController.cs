using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Customer;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseLib.Models;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MasterAuth]
    public class CustomerController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;
        public CustomerController(DbcoffeeHouseContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetUserInformation")]
        public async Task<IActionResult> GetUserInformation()
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();
            UserInformationDTO userInformationResponseDTO = new UserInformationDTO();
            userInformationResponseDTO.FullName = loginResponse.FullName;
            userInformationResponseDTO.PhoneNumber = loginResponse.Phone;
            userInformationResponseDTO.DateOfBirth = loginResponse.DateOfBirth;
            userInformationResponseDTO.Email = loginResponse.Email;
            userInformationResponseDTO.Role = loginResponse.IdRole;
            userInformationResponseDTO.Id = loginResponse.Id;

            var getOrder = await _context.Orders.Where(x => x.CustomerId == loginResponse.Id).ToListAsync();
            userInformationResponseDTO.OrderedCount = getOrder.Count();
            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Value = userInformationResponseDTO,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Status = (int)HttpStatusCode.OK
            });
        }

        [HttpPost]
        [Route("UpdateUserInformation")]
        public async Task<IActionResult> UpdateUserInformation([FromBody] UserInformationDTO request)
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();
            
            if (loginResponse.Email != request.Email) return Unauthorized();

            var customer = await _context.Customers.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
            if (customer == null) return Unauthorized();

            customer.FullName = request.FullName;
            customer.DateOfBirth = request.DateOfBirth;
            customer.Phone = request.PhoneNumber;
            customer.IdRole = request.Role;

            await this.SaveChanges(_context);
            loginResponse = MasterAuth.MappingLoginResponseFromAccountAndCustomer(customer);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Value = loginResponse,
                Status = (int)HttpStatusCode.OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.PUT),
            });
        }
    }
}
