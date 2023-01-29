using System;
using CleanModels;

namespace Dal.Entities;

public class Passport
{
    public short Series { get; set; }

    public int Number { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public Gender Gender { get; set; }

    public DateTime BirthDate { get; set; }

    public string Residence { get; set; } = null!;

    public virtual Employee Employee { get; set; }
}