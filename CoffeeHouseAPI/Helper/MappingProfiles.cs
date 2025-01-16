using System.ComponentModel;
using AutoMapper;
using CoffeeHouseAPI.DTOs.Address;
using CoffeeHouseAPI.DTOs.Auth;
using CoffeeHouseAPI.DTOs.Category;
using CoffeeHouseAPI.DTOs.Image;
using CoffeeHouseAPI.DTOs.Product;
using CoffeeHouseAPI.DTOs.ProductSize;
using CoffeeHouseAPI.Helper;
using CoffeeHouseLib.Models;

namespace OrderService.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<CreateAccountRequest, Account>();
            CreateMap<Account, CreateAccountRequest>();

            CreateMap<Customer, CreateAccountRequest>();
            CreateMap<CreateAccountRequest, Customer>();

            CreateMap<RefreshTokenDTO, RefreshToken>();
            CreateMap<RefreshToken, RefreshTokenDTO>();

            CreateMap<Category, CategoryRequestDTO>();
            CreateMap<CategoryRequestDTO, Category>();
            CreateMap<Category, CategoryResponseDTO>()
                .ForMember(s => s.ChildCategory, option => option.MapFrom(s => s.InverseIdParentNavigation)); ;
            CreateMap<CategoryResponseDTO, Category>()
                .ForMember(s => s.InverseIdParentNavigation, option => option.MapFrom(s => s.ChildCategory));

            CreateMap<Product, ProductRequestDTO>();
            CreateMap<ProductRequestDTO, Product>();
            CreateMap<Product, ProductResponseDTO>();
            CreateMap<Product, ProductResponseDTO>();

            //CreateMap<Image, ImageRequestDTO>()
            //    .ForMember(d => d.Content, option => option.MapFrom(s => Convert.ToBase64String(s.Content)));

            //CreateMap<ImageRequestDTO, Image>()
            //    .ForMember(d => d.Content, option => option.MapFrom(s => Convert.FromBase64String(s.Content.Base64Encode())));

            CreateMap<Image, ImageResponseDTO>();
            CreateMap<ImageResponseDTO, Image>();

            CreateMap<ProductSize, ProductSizeRequestDTO>();
            CreateMap<ProductSizeRequestDTO, ProductSize>();

            CreateMap<AddressDTO, Address>();
            CreateMap<Address, AddressDTO>();
        }
    }
}
