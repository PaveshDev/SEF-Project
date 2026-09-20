namespace LoopWorth.Application.DTOs;
using LoopWorth.Domain.Enums;
using System;
using System.Collections.Generic;

public class CategoryDto {
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class ItemDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? ConditionDescription { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SelectedRecoveryRoute { get; set; }
    public List<ItemImageDto> Images { get; set; } = new();
}

public class ItemImageDto {
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class CreateItemDto {
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? ConditionDescription { get; set; }
}

public class UpdateItemDto : CreateItemDto { }

public class SelectRouteDto {
    public string SelectedRoute { get; set; } = string.Empty;
}
