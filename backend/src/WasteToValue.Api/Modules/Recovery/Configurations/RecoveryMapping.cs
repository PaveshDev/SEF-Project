using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WasteToValue.Api.Modules.Recovery.Configurations;

internal static class RecoveryMapping
{
    public static void Common<T>(EntityTypeBuilder<T> builder, string table, bool mutable = true) where T : class
    {
        builder.ToTable(table);
        builder.HasKey("Id");
        foreach (var property in typeof(T).GetProperties())
        {
            if (property.Name is "PreferredRoutes" or "IsActive") { builder.Ignore(property.Name); continue; }
            builder.Property(property.Name).HasColumnName(JsonNamingPolicy.SnakeCaseLower.ConvertName(property.Name));
        }
        builder.Property<Guid>("Id").HasDefaultValueSql("gen_random_uuid()");
        if (mutable)
        {
            builder.Property<int>("Version").IsConcurrencyToken();
            builder.ToTable(t => t.HasCheckConstraint($"ck_{table}_version", "version > 0"));
        }
    }

    public static void Enum<T, TEnum>(EntityTypeBuilder<T> builder, string property, string column, int max = 30)
        where T : class where TEnum : struct, Enum
    {
        builder.Property<TEnum>(property).HasColumnName(column).HasMaxLength(max)
            .HasConversion(v => Encode(v), v => Decode<TEnum>(v));
        var allowed = string.Join(",", System.Enum.GetValues<TEnum>().Select(v => $"'{Encode(v)}'"));
        builder.ToTable(t => t.HasCheckConstraint($"ck_{typeof(T).Name.ToLowerInvariant()}_{column}", $"{column} IN ({allowed})"));
    }

    public static string Encode<TEnum>(TEnum value) where TEnum : struct, Enum => JsonNamingPolicy.SnakeCaseUpper.ConvertName(value.ToString());
    public static TEnum Decode<TEnum>(string value) where TEnum : struct, Enum
        => System.Enum.GetValues<TEnum>().Single(v => Encode(v) == value);

    public static void Money<T>(EntityTypeBuilder<T> builder, params string[] properties) where T : class
    {
        foreach (var property in properties)
        {
            builder.Property(property).HasPrecision(12, 2);
            var column = JsonNamingPolicy.SnakeCaseLower.ConvertName(property);
            builder.ToTable(t => t.HasCheckConstraint($"ck_{typeof(T).Name.ToLowerInvariant()}_{column}", $"{column} IS NULL OR {column} >= 0"));
        }
    }

    public static void Json<T>(EntityTypeBuilder<T> builder, string property, string column) where T : class
        => builder.Property<string>(property).HasColumnName(column).HasColumnType("jsonb").IsRequired();

    public static void Currency<T>(EntityTypeBuilder<T> builder) where T : class
    {
        builder.Property<string>("Currency").HasColumnType("character(3)");
        builder.ToTable(t => t.HasCheckConstraint($"ck_{typeof(T).Name.ToLowerInvariant()}_currency", "currency ~ '^[A-Z]{3}$'"));
    }
}
