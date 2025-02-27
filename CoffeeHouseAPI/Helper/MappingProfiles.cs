using System.ComponentModel;
using AutoMapper;
using CoffeeHouseAPI.DTOs.Address;
using CoffeeHouseAPI.DTOs.Auth;
using CoffeeHouseAPI.DTOs.Category;
using CoffeeHouseAPI.DTOs.Customer;
using CoffeeHouseAPI.DTOs.Discount;
using CoffeeHouseAPI.DTOs.Image;
using CoffeeHouseAPI.DTOs.Order;
using CoffeeHouseAPI.DTOs.Product;
using CoffeeHouseAPI.DTOs.ProductSize;
using CoffeeHouseAPI.DTOs.Review;
using CoffeeHouseAPI.DTOs.Topping;
using CoffeeHouseAPI.DTOs.Voucher;
using CoffeeHouseAPI.Helper;
using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Create mapping profile for IMAGE
            CreateMap<Image, ImageResponseDTO>();
            CreateMap<ImageResponseDTO, Image>();
            CreateMap<Image, ImageRequestDTO>();
            CreateMap<ImageRequestDTO, Image>();

            // Create mapping profile for AUTH
            CreateMap<RefreshTokenDTO, RefreshToken>();
            CreateMap<RefreshToken, RefreshTokenDTO>();

            // Create mapping profile for CATEGORY
            CreateMap<Category, CategoryRequestDTO>();
            CreateMap<CategoryRequestDTO, Category>();
            CreateMap<Category, CategoryResponseDTO>()
                .ForMember(s => s.ChildCategory, option => option.MapFrom(s => s.InverseIdParentNavigation)); ;
            CreateMap<CategoryResponseDTO, Category>()
                .ForMember(s => s.InverseIdParentNavigation, option => option.MapFrom(s => s.ChildCategory));
            CreateMap<CategoryDTO, Category>();
            CreateMap<Category, CategoryDTO>();

            // Create mapping profile for PRODUCT
            CreateMap<Product, ProductRequestDTO>();
            CreateMap<ProductRequestDTO, Product>();

            CreateMap<Product, ProductResponseDTO>();
            CreateMap<ProductResponseDTO, Product>();

            CreateMap<ProductSize, ProductSizeRequestDTO>();
            CreateMap<ProductSizeRequestDTO, ProductSize>();

            CreateMap<ProductSize, ProductResponseDTO>();
            CreateMap<ProductResponseDTO, ProductSize>();

            CreateMap<ProductModel, Product>();
            CreateMap<Product, ProductModel>();

            CreateMap<ProductSize, ProductSizeResponseDTO>();
            CreateMap<ProductSizeResponseDTO, ProductSize>();


            // Create mapping profile for ADDRESS
            CreateMap<AddressDTO, Address>();
            CreateMap<Address, AddressDTO>();

            // Create mapping profile for TOPPING
            CreateMap<Topping, ToppingDTO>();
            CreateMap<ToppingDTO, Topping>();

            // Create mapping profile for VOUCHER
            CreateMap<Voucher, VoucherDTO>();
            CreateMap<VoucherDTO, Voucher>();

            // Create mapping profile for DISCOUNT
            CreateMap<Discount, DiscountDTO>();
            CreateMap<DiscountDTO, Discount>();

            // Create mapping profile for REVIEW
            CreateMap<ReviewDTO, Review>();
            CreateMap<Review, ReviewDTO>();

            // Create mapping profile for CUSTOMER

            CreateMap<CreateAccountRequest, Account>();
            CreateMap<Account, CreateAccountRequest>();

            CreateMap<Customer, CreateAccountRequest>();
            CreateMap<CreateAccountRequest, Customer>();

            CreateMap<CustomerDTO, Customer>();
            CreateMap<Customer, CustomerDTO>();

            // Create mapping profile for ORDER

            CreateMap<OrderDetail, OrderDetailDTO>();
            CreateMap<OrderDetailDTO, OrderDetail>();

            CreateMap<Order, OrderResponseDTO>()
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails));
            CreateMap<OrderResponseDTO, Order>();

            CreateMap<OrderLog, OrderLogDTO>();
            CreateMap<OrderLogDTO, OrderLog>();

            CreateMap<Order, OrderResponseDTO>();
            CreateMap<OrderResponseDTO, Order>();

            CreateMap<OrderStatus, OrderStatusDTO>();
            CreateMap<OrderStatusDTO, OrderStatus>();

            CreateMap<OrderTopping, OrderToppingDTO>();
            CreateMap<OrderToppingDTO, OrderTopping>();
        }
    }
}
