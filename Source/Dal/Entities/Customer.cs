using System;
using System.Collections.Generic;
using CleanModels;

namespace Dal.Entities;

public class Customer : IEntity
{
    public Guid Id { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public Gender Gender { get; set; }

    public string EmailAddress { get; set; } = null!;

    public User User { get; set; } = new ();

    public virtual IEnumerable<Order> Orders { get; } = new List<Order>();
}