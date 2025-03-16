using AutoMapper;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Customer;
using CoffeeHouseAPI.DTOs.Image;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.Firebase;
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
        readonly FirebaseService _firebaseService;
        readonly IMapper _mapper;
        public CustomerController(DbcoffeeHouseContext context, FirebaseService firebaseService, IMapper mapper)
        {
            _firebaseService = firebaseService;
            _context = context;
            _mapper = mapper;
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
            userInformationResponseDTO.AvatarImage = loginResponse.AvatarImage;

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

        [HttpPost]
        [Route("UpdateAvatar")]
        public async Task<IActionResult> UpdateAvatar([FromBody] ImageRequestDTO request)
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();
            var customer = await _context.Customers.Where(x => x.Id == loginResponse.Id && x.IsValid).FirstOrDefaultAsync();
            
            if (customer == null) return Unauthorized();

            var imageClass = await _context.ImageClasses.Where(x => x.Id == request.ImageClassId).AsNoTracking().FirstOrDefaultAsync();
            if (imageClass == null)
            {
                return BadRequest(new APIResponseBase
                {
                    Status = (int)StatusCodes.Status400BadRequest,
                    Message = "Image class not found",
                    IsSuccess = false,
                });
            }

            if (request.ImageName == "default_avatar")
                request.ImageName = DateTime.Now.ToString("yyyyMMddhhssmm");

            // Add image to firebase
            string downloadUrl = await _firebaseService.UploadImageAsync(request);

            // Add to database
            Image image = _mapper.Map<Image>(request);
            
            image.FirebaseImage = downloadUrl;
            _context.Images.Add(image);
            await this.SaveChanges(_context);

            loginResponse.AvatarImage = downloadUrl;

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
