using AutoMapper;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Image;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Mvc;

using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder.Extensions;
using FirebaseAdmin;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using CoffeeHouseAPI.Services.Firebase;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _mapper;
        readonly FirebaseService _firebaseService;

        public ImageController(DbcoffeeHouseContext context, IMapper mapper, FirebaseService firebaseService)
        {
            _context = context;
            _mapper = mapper;
            _firebaseService = firebaseService;
        }

        [HttpPost]
        [Route("AddImage")]
        public async Task<IActionResult> AddImage([FromBody] ImageRequestDTO request)
        {
            var imageClass = await _context.ImageClasses.Where(x => x.Id == request.ImageClassId).FirstOrDefaultAsync();
            if (imageClass == null)
            {
                return BadRequest(new APIResponseBase
                {
                    Status = (int)StatusCodes.Status400BadRequest,
                    Message = "Image class not found",
                    IsSuccess = false,
                });
            }

            byte[] imageContentByte = Convert.FromBase64String(request.Content);

            Image image = _mapper.Map<Image>(imageContentByte);
            image.ImageName = image.ImageName ?? DateTime.Now.Millisecond.ToString();
            string downloadUrl = await _firebaseService.UploadImageAsync(request);
            image.FirebaseImage = downloadUrl;
            _context.Images.Add(image);
            await this.SaveChanges(_context);

            return Ok(new APIResponseBase
            {
                Status = (int)StatusCodes.Status200OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.POST),
                IsSuccess = true,
            });
        }
    }
}
