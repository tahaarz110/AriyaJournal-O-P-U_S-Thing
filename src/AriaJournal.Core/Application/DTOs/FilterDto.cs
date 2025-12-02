// =============================================================================
// فایل: src/AriaJournal.Core/Application/DTOs/FilterDto.cs
// توضیح: DTOs مربوط به فیلترها
// =============================================================================

namespace AriaJournal.Core.Application.DTOs;

/// <summary>
/// DTO فیلتر
/// </summary>
public class FilterDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Category { get; set; }
    public bool IsBuiltIn { get; set; }
    public List<FilterConditionDto> Conditions { get; set; } = new();
    public string? SortColumn { get; set; }
    public bool SortAscending { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO شرط فیلتر
/// </summary>
public class FilterConditionDto
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = "Equals";
    public string? Value { get; set; }
    public string? Value2 { get; set; } // برای Between
    public string Logic { get; set; } = "And"; // And, Or
}

/// <summary>
/// DTO ایجاد فیلتر
/// </summary>
public class CreateFilterDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Category { get; set; }
    public List<FilterConditionDto> Conditions { get; set; } = new();
    public string? SortColumn { get; set; }
    public bool SortAscending { get; set; } = true;
}

/// <summary>
/// DTO بروزرسانی فیلتر
/// </summary>
public class UpdateFilterDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Category { get; set; }
    public List<FilterConditionDto>? Conditions { get; set; }
    public string? SortColumn { get; set; }
    public bool? SortAscending { get; set; }
}

/// <summary>
/// DTO اعمال فیلتر
/// </summary>
public class ApplyFilterDto
{
    public string? FilterId { get; set; }
    public List<FilterConditionDto>? Conditions { get; set; }
    public string? SortColumn { get; set; }
    public bool SortAscending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// DTO نتیجه فیلتر
/// </summary>
public class FilterResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Application/DTOs/FilterDto.cs
// =============================================================================