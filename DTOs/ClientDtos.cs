//DTOs/ClientDtos.cs
using System.ComponentModel.DataAnnotations;

namespace geoback.DTOs;

public class CreateClientRequestDto
{
    [Required]
    [StringLength(50)]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string CustomerNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(200)]
    public string? ProjectName { get; set; }
}

public class UpdateClientRequestDto
{
    [StringLength(50)]
    public string? CustomerId { get; set; }

    [StringLength(50)]
    public string? CustomerNumber { get; set; }

    [StringLength(200)]
    public string? Name { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(200)]
    public string? ProjectName { get; set; }
}