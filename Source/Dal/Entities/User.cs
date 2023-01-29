using System;
using CleanModels;

namespace Dal.Entities;

public class User : IEntity
{
    public Guid Id { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public UserRole Role { get; set; }

    public Customer Customer { get; set; }

    public Employee Employee { get; set; }
}