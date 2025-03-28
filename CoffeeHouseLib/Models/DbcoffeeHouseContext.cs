using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseLib.Models;

public partial class DbcoffeeHouseContext : DbContext
{
    public DbcoffeeHouseContext()
    {
    }

    public DbcoffeeHouseContext(DbContextOptions<DbcoffeeHouseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Aiconfig> Aiconfigs { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartDetail> CartDetails { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ChatRoom> ChatRooms { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Discount> Discounts { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<ImageClass> ImageClasses { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderAppliedFee> OrderAppliedFees { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OrderFee> OrderFees { get; set; }

    public virtual DbSet<OrderLog> OrderLogs { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<OrderTopping> OrderToppings { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductSize> ProductSizes { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<RoomUser> RoomUsers { get; set; }

    public virtual DbSet<Topping> Toppings { get; set; }

    public virtual DbSet<UserConnection> UserConnections { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=103.140.249.182;Database=DBCoffeeHouse;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC0759ADDCE5");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Id, "IX_Account_Id");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AccessToken).HasMaxLength(255);
            entity.Property(e => e.BlockExpire).HasColumnType("datetime");
            entity.Property(e => e.MessageToken).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.RefreshToken).HasMaxLength(255);
            entity.Property(e => e.ResetPasswordExpired).HasColumnType("datetime");
            entity.Property(e => e.ResetPasswordToken).HasMaxLength(255);
            entity.Property(e => e.VerifyTime).HasColumnType("datetime");
            entity.Property(e => e.VerifyToken).HasMaxLength(255);

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Account)
                .HasForeignKey<Account>(d => d.Id)
                .HasConstraintName("FK__Account__Custome__71D1E811");

            entity.HasOne(d => d.RefreshTokenNavigation).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RefreshToken)
                .HasConstraintName("FK__Account__Refresh__0C85DE4D");
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Address__3214EC07B2146725");

            entity.ToTable("Address");

            entity.HasIndex(e => e.CustomerId, "IX_Address_CustomerId");

            entity.HasIndex(e => e.Id, "IX_Address_Id");

            entity.HasIndex(e => e.Id, "UQ__Address__3214EC0683CE60F9").IsUnique();

            entity.Property(e => e.AddressNumber).HasMaxLength(255);
            entity.Property(e => e.District).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Province).HasMaxLength(255);
            entity.Property(e => e.Ward).HasMaxLength(255);

            entity.HasOne(d => d.Customer).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Address__Custome__72C60C4A");
        });

        modelBuilder.Entity<Aiconfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AIConfig__3214EC0776DAF6DC");

            entity.ToTable("AIConfig");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Key).HasMaxLength(200);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attachme__3214EC07029BA2E9");

            entity.ToTable("Attachment");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.FileType).HasMaxLength(50);

            entity.HasOne(d => d.Message).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK__Attachmen__Messa__19AACF41");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cart__3214EC071369CECC");

            entity.ToTable("Cart");

            entity.HasIndex(e => e.CustomerId, "IX_Cart_CustomerId");

            entity.HasIndex(e => e.ProductSizeId, "IX_Cart_ProductSizeId");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithMany(p => p.Carts)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__CustomerId__1CBC4616");

            entity.HasOne(d => d.ProductSize).WithMany(p => p.Carts)
                .HasForeignKey(d => d.ProductSizeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__ProductSiz__1DB06A4F");
        });

        modelBuilder.Entity<CartDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CartDeta__3214EC07954FB0D8");

            entity.ToTable("CartDetail");

            entity.HasIndex(e => e.ToppingId, "IX_CartDetail_ToppingId");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartDetails)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__CartDetai__CartI__2180FB33");

            entity.HasOne(d => d.Topping).WithMany(p => p.CartDetails)
                .HasForeignKey(d => d.ToppingId)
                .HasConstraintName("FK__CartDetai__Toppi__208CD6FA");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC077A29AB17");

            entity.ToTable("Category");

            entity.HasIndex(e => e.IdParent, "IX_Category_IdParent");

            entity.HasIndex(e => e.Id, "UQ__Category__3214EC065893F82D").IsUnique();

            entity.Property(e => e.CategoryName).HasMaxLength(50);
            entity.Property(e => e.IsValid).HasDefaultValue(true);

            entity.HasOne(d => d.IdParentNavigation).WithMany(p => p.InverseIdParentNavigation)
                .HasForeignKey(d => d.IdParent)
                .HasConstraintName("FK__Category__IdPare__19DFD96B");
        });

        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatRoom__3214EC07131888DD");

            entity.ToTable("ChatRoom");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.RoomType).HasMaxLength(50);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC0778922238");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.Email, "IX_CUSTOMER_EMAIL");

            entity.HasIndex(e => e.Id, "UQ__Customer__3214EC0663F39AEF").IsUnique();

            entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.IsValid).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(12);

            entity.HasOne(d => d.AvatarImageNavigation).WithMany(p => p.Customers)
                .HasForeignKey(d => d.AvatarImage)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Customer__Avatar__607251E5");
        });

        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Discount__3214EC07A16D6EA5");

            entity.ToTable("Discount");

            entity.HasIndex(e => e.Id, "UQ__Discount__3214EC0624C4FD91").IsUnique();

            entity.Property(e => e.DiscountName).HasMaxLength(255);
            entity.Property(e => e.DiscountType)
                .HasMaxLength(255)
                .HasDefaultValue("VALUE");
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Image__3214EC07C910BDBF");

            entity.ToTable("Image");

            entity.HasIndex(e => e.Id, "UQ__Image__3214EC06C34BEC6F").IsUnique();

            entity.Property(e => e.ImageName).HasMaxLength(255);
            entity.Property(e => e.ImageType).HasMaxLength(255);

            entity.HasOne(d => d.ImageClass).WithMany(p => p.Images)
                .HasForeignKey(d => d.ImageClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Image__ImageClas__73BA3083");
        });

        modelBuilder.Entity<ImageClass>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ImageCla__3214EC077A5F0BB3");

            entity.ToTable("ImageClass");

            entity.HasIndex(e => e.Id, "UQ__ImageCla__3214EC0679A57027").IsUnique();

            entity.Property(e => e.ImageClassCode).HasMaxLength(255);
            entity.Property(e => e.ImageClassName).HasMaxLength(255);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Message__3214EC07E8E99C79");

            entity.ToTable("Message");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.MessageType).HasMaxLength(50);

            entity.HasOne(d => d.Room).WithMany(p => p.Messages)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__Message__RoomId__13F1F5EB");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Message__SenderI__14E61A24");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Order__3214EC0701D67376");

            entity.ToTable("Order");

            entity.HasIndex(e => e.Id, "IX_ORDER_ID");

            entity.HasIndex(e => e.Id, "UQ__Order__3214EC06C31E2FAF").IsUnique();

            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.OrderPaid).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.OrderValue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentId).HasMaxLength(2255);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);

            entity.HasOne(d => d.Address).WithMany(p => p.Orders)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__AddressId__2B0A656D");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__AccountId__74AE54BC");

            entity.HasOne(d => d.Voucher).WithMany(p => p.Orders)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("FK__Order__VoucherId__75A278F5");
        });

        modelBuilder.Entity<OrderAppliedFee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderApp__3214EC072898EAA1");

            entity.ToTable("OrderAppliedFee");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Fee).WithMany(p => p.OrderAppliedFees)
                .HasForeignKey(d => d.FeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderAppl__FeeId__4865BE2A");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderAppliedFees)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderAppl__Order__477199F1");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderDet__3214EC07BBD8A9EE");

            entity.ToTable("OrderDetail");

            entity.HasIndex(e => e.OrderId, "IX_OrderDetail_OrderId");

            entity.HasIndex(e => e.ProductSizeId, "IX_OrderDetail_ProductSizeId");

            entity.Property(e => e.Note).HasMaxLength(255);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Order__778AC167");

            entity.HasOne(d => d.ProductSize).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductSizeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Produ__282DF8C2");
        });

        modelBuilder.Entity<OrderFee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderFee__3214EC079D90B0CD");

            entity.ToTable("OrderFee");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.FeeType).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<OrderLog>(entity =>
        {
            entity.HasKey(e => new { e.StatusCode, e.OrderId }).HasName("PK__OrderLog__864241410A693A6F");

            entity.ToTable("OrderLog");

            entity.HasIndex(e => e.StatusCode, "IX_ORDERLOG_STATUSCODE");

            entity.HasIndex(e => e.TimeLog, "IX_ORDERLOG_TIMELOG");

            entity.Property(e => e.StatusCode).HasMaxLength(20);
            entity.Property(e => e.TimeLog).HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderLogs)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderLog__OrderI__787EE5A0");

            entity.HasOne(d => d.StatusCodeNavigation).WithMany(p => p.OrderLogs)
                .HasForeignKey(d => d.StatusCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderLog__Status__797309D9");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.StatusCode).HasName("PK__OrderSta__6A7B44FD12F8E292");

            entity.ToTable("OrderStatus");

            entity.Property(e => e.StatusCode).HasMaxLength(20);
            entity.Property(e => e.StatusName).HasMaxLength(255);
        });

        modelBuilder.Entity<OrderTopping>(entity =>
        {
            entity.HasKey(e => new { e.ToppingId, e.OrderDetailId }).HasName("PK__OrderTop__9D59FFA4D2CE8CE8");

            entity.ToTable("OrderTopping");

            entity.HasIndex(e => e.OrderDetailId, "IX_OrderTopping_OrderDetailId");

            entity.HasIndex(e => e.ToppingId, "IX_OrderTopping_ToppingId");

            entity.HasOne(d => d.OrderDetail).WithMany(p => p.OrderToppings)
                .HasForeignKey(d => d.OrderDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderTopp__Order__4D5F7D71");

            entity.HasOne(d => d.Topping).WithMany(p => p.OrderToppings)
                .HasForeignKey(d => d.ToppingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderTopp__Toppi__7B5B524B");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3214EC0775E44AA1");

            entity.ToTable("Product");

            entity.HasIndex(e => e.CategoryId, "IX_Product_CategoryId");

            entity.HasIndex(e => e.Id, "IX_Product_Id");

            entity.HasIndex(e => e.ImageDefault, "IX_Product_ImageDefault");

            entity.HasIndex(e => e.Id, "UQ__Product__3214EC0614DAD9E1").IsUnique();

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsValid).HasDefaultValue(true);
            entity.Property(e => e.ProductName).HasMaxLength(255);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Product__Categor__7C4F7684");

            entity.HasOne(d => d.Discount).WithMany(p => p.Products)
                .HasForeignKey(d => d.DiscountId)
                .HasConstraintName("FK__Product__Discoun__3B40CD36");

            entity.HasOne(d => d.ImageDefaultNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.ImageDefault)
                .HasConstraintName("FK__Product__ImageDe__18EBB532");

            entity.HasMany(d => d.Images).WithMany(p => p.ProductsNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductImage",
                    r => r.HasOne<Image>().WithMany()
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ProductIm__Image__7F2BE32F"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ProductIm__Produ__00200768"),
                    j =>
                    {
                        j.HasKey("ProductId", "ImageId").HasName("PK__ProductI__635DA9BD43845267");
                        j.ToTable("ProductImage");
                    });

            entity.HasMany(d => d.Toppings).WithMany(p => p.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductTopping",
                    r => r.HasOne<Topping>().WithMany()
                        .HasForeignKey("ToppingId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ProductTo__Toppi__17036CC0"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ProductTo__Produ__160F4887"),
                    j =>
                    {
                        j.HasKey("ProductId", "ToppingId");
                        j.ToTable("ProductTopping");
                        j.HasIndex(new[] { "ProductId", "ToppingId" }, "IX_ProductId_ToppingId");
                    });
        });

        modelBuilder.Entity<ProductSize>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductS__3214EC0739ABB0C8");

            entity.ToTable("ProductSize");

            entity.HasIndex(e => e.Id, "IX_ProductSize_Id");

            entity.HasIndex(e => new { e.Id, e.ProductId }, "IX_ProductSize_Id_ProductId");

            entity.HasIndex(e => e.ProductId, "IX_ProductSize_ProductId");

            entity.HasIndex(e => e.Id, "UQ__ProductS__3214EC063D3BE48E").IsUnique();

            entity.Property(e => e.IsValid).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Size).HasMaxLength(30);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductSizes)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductSi__Produ__01142BA1");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshToken1).HasName("PK__RefreshT__DEA298DBD1B6233F");

            entity.ToTable("RefreshToken");

            entity.Property(e => e.RefreshToken1)
                .HasMaxLength(255)
                .HasColumnName("RefreshToken");
            entity.Property(e => e.Created).HasColumnType("datetime");
            entity.Property(e => e.Expire).HasColumnType("datetime");
            entity.Property(e => e.Revoke).HasColumnType("datetime");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Review__3214EC07079FDA38");

            entity.ToTable("Review");

            entity.Property(e => e.Comment).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Review__Customer__2EDAF651");

            entity.HasOne(d => d.Product).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__Review__ProductI__2FCF1A8A");
        });

        modelBuilder.Entity<RoomUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RoomUser__3214EC07EFEA3993");

            entity.ToTable("RoomUser");

            entity.HasIndex(e => new { e.RoomId, e.UserId }, "UQ__RoomUser__E3FEB5FC364E88B7").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("Member");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomUsers)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__RoomUser__RoomId__0C50D423");

            entity.HasOne(d => d.User).WithMany(p => p.RoomUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__RoomUser__UserId__0D44F85C");
        });

        modelBuilder.Entity<Topping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Topping__3214EC0774242ACB");

            entity.ToTable("Topping");

            entity.HasIndex(e => e.Id, "UQ__Topping__3214EC0679293D30").IsUnique();

            entity.Property(e => e.IsValid).HasDefaultValue(true);
            entity.Property(e => e.ToppingName).HasMaxLength(255);
            entity.Property(e => e.ToppingPrice).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<UserConnection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserConn__3214EC07D376F82A");

            entity.ToTable("UserConnection");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ConnectedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ConnectionId).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.UserConnections)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserConne__UserI__1E6F845E");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Voucher__3214EC07FD06D1FF");

            entity.ToTable("Voucher");

            entity.HasIndex(e => e.Id, "UQ__Voucher__3214EC0692E9104C").IsUnique();

            entity.HasIndex(e => e.Code, "UQ__Voucher__A25C5AA7685FC04C").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.DiscountType).HasMaxLength(20);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MinOrderValue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
