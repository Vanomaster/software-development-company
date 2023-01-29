using System;

namespace CleanModels;

[Serializable]
public class CustomerData : IUserData
{
    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public Gender Gender { get; set; }

    public string EmailAddress { get; set; } = null!;
}