using System;
using System.Collections.Generic;
using CleanModels;

namespace Dal.Entities;

public class Employee : IEntity
{
    public Guid Id { get; set; }

    public short PassportSeries { get; set; }

    public int PassportNumber { get; set; }

    public JobPosition JobPosition { get; set; }

    public string EmailAddress { get; set; } = null!;

    public User User { get; set; } = new ();

    public Passport Passport { get; set; } = new ();

    public List<Order> Orders { get; set; } = new ();
}