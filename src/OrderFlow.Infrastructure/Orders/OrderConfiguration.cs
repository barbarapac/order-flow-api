using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Orders;
using OrderFlow.Domain.Orders.ValueObjects;

namespace OrderFlow.Infrastructure.Orders;

[ExcludeFromCodeCoverage]
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.HasIndex(o => o.CustomerId);

        builder.Property(o => o.Currency)
            .HasConversion(currency => currency.Value, value => Currency.Create(value))
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(o => o.ConfirmedAtUtc)
            .HasColumnName("confirmed_at_utc");

        builder.Property(o => o.CanceledAtUtc)
            .HasColumnName("canceled_at_utc");

        builder.Ignore(o => o.Total);

        builder.OwnsMany(o => o.Items, items =>
        {
            items.ToTable("order_items");
            items.WithOwner().HasForeignKey("order_id");

            items.HasKey(i => i.Id);
            items.Property(i => i.Id).ValueGeneratedNever();

            items.Property(i => i.ProductId)
                .HasColumnName("product_id")
                .IsRequired();

            items.Property(i => i.UnitPrice)
                .HasColumnName("unit_price")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            items.Property(i => i.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            items.Ignore(i => i.LineTotal);
        });

        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
