using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Items.Entities;

namespace WasteToValue.Api.Modules.Items.Configurations;

public class ItemPhotoConfiguration : IEntityTypeConfiguration<ItemPhoto>
{
    public void Configure(EntityTypeBuilder<ItemPhoto> builder)
    {
        builder.ToTable("ItemPhotos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(2000);
        
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => new { x.ItemId, x.PhotoOrder });
    }
}
