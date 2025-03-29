using System.ComponentModel;
using AutoMapper;
using CoffeeHouseAPI.DTOs.Address;
using CoffeeHouseAPI.DTOs.Auth;
using CoffeeHouseAPI.DTOs.Category;
using CoffeeHouseAPI.DTOs.Chat;
using CoffeeHouseAPI.DTOs.Customer;
using CoffeeHouseAPI.DTOs.Discount;
using CoffeeHouseAPI.DTOs.Image;
using CoffeeHouseAPI.DTOs.Order;
using CoffeeHouseAPI.DTOs.OrderFee;
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

            CreateMap<ProductResponseForOrderDetailDTO, Product>();
            CreateMap<Product, ProductResponseForOrderDetailDTO>();

            CreateMap<ProductSizeResponseForOrderDetailDTO, ProductSize>();
            CreateMap<ProductSize, ProductSizeResponseForOrderDetailDTO>();

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
            CreateMap<ReviewResponseDTO, Review>();
            CreateMap<Review, ReviewResponseDTO>()
                .ForMember(x => x.Customer, options => options.MapFrom(s => MasterAuth.MappingLoginResponseFromAccountAndCustomer(s.Customer)));

            // Create mapping profile for CUSTOMER

            CreateMap<CreateAccountRequest, Account>();
            CreateMap<Account, CreateAccountRequest>();

            CreateMap<Customer, CreateAccountRequest>();
            CreateMap<CreateAccountRequest, Customer>();

            CreateMap<CustomerDTO, Customer>();
            CreateMap<Customer, CustomerDTO>();

            CreateMap<CustomerInforDTO, Customer>();
            CreateMap<Customer, CustomerInforDTO>()
                .ForMember(x => x.BlockExpire, options => options.MapFrom(s => s.Account!.BlockExpire ?? null))
                .ForMember(x => x.VerifyTime, options => options.MapFrom(s => s.Account!.VerifyTime ?? null));

            // Create mapping profile for ORDER

            CreateMap<OrderDetail, OrderDetailDTO>();
            CreateMap<OrderDetailDTO, OrderDetail>();

            CreateMap<Order, OrderResponseDTO>();
            CreateMap<OrderResponseDTO, Order>();

            CreateMap<OrderLog, OrderLogDTO>();
            CreateMap<OrderLogDTO, OrderLog>();

            CreateMap<Order, OrderResponseDTO>();
            CreateMap<OrderResponseDTO, Order>();

            CreateMap<OrderStatus, OrderStatusDTO>();
            CreateMap<OrderStatusDTO, OrderStatus>();

            CreateMap<OrderTopping, OrderToppingDTO>();
            CreateMap<OrderToppingDTO, OrderTopping>();

            CreateMap<OrderFee, OrderFeeResponseDTO>();
            CreateMap<OrderFeeResponseDTO, OrderFee>();

            CreateMap<SendMessageDTO, Message>();
            CreateMap<Message, SendMessageDTO>();
        }
    }
}
