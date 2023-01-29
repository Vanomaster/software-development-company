using System;

namespace CleanModels;

[Serializable]
public class EmployeeData : IUserData
{
    public short PassportSeries { get; set; }

    public int PassportNumber { get; set; }

    public JobPosition JobPosition { get; set; }

    public string EmailAddress { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public Gender Gender { get; set; }

    public DateTime BirthDate { get; set; }

    public string Residence { get; set; } = null!;
}