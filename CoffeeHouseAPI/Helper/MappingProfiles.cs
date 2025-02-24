using System.ComponentModel;
using AutoMapper;
using CoffeeHouseAPI.DTOs.Address;
using CoffeeHouseAPI.DTOs.Auth;
using CoffeeHouseAPI.DTOs.Category;
using CoffeeHouseAPI.DTOs.Discount;
using CoffeeHouseAPI.DTOs.Image;
using CoffeeHouseAPI.DTOs.Product;
using CoffeeHouseAPI.DTOs.ProductSize;
using CoffeeHouseAPI.DTOs.Review;
using CoffeeHouseAPI.DTOs.Topping;
using CoffeeHouseAPI.DTOs.Voucher;
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
            CreateMap<CategoryDTO, Category>();
            CreateMap<Category, CategoryDTO>();

            CreateMap<Product, ProductRequestDTO>();
            CreateMap<ProductRequestDTO, Product>();
            CreateMap<Product, ProductResponseDTO>();
            CreateMap<ProductResponseDTO, Product>();

            CreateMap<Image, ImageResponseDTO>();
            CreateMap<ImageResponseDTO, Image>();
            CreateMap<Image, ImageRequestDTO>();
            CreateMap<ImageRequestDTO, Image>();

            CreateMap<ProductSize, ProductSizeRequestDTO>();
            CreateMap<ProductSizeRequestDTO, ProductSize>();

            CreateMap<AddressDTO, Address>();
            CreateMap<Address, AddressDTO>();

            CreateMap<Topping, ToppingDTO>();
            CreateMap<ToppingDTO, Topping>();

            CreateMap<Voucher, VoucherDTO>();
            CreateMap<VoucherDTO, Voucher>();

            CreateMap<Discount, DiscountDTO>();
            CreateMap<DiscountDTO, Discount>();

            CreateMap<ReviewDTO, Review>();
            CreateMap<Review, ReviewDTO>();
        }
    }
}
