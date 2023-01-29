using System;

namespace CleanModels;

public class IUserData
{
    public IUserData()
    {
    }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string Gender { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public short PassportSeries { get; set; }

    public short PassportNumber { get; set; }

    public string JobPosition { get; set; } = null!;

    public DateTime BirthDate { get; set; }

    public string Residence { get; set; } = null!;
}