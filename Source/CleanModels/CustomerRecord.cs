namespace Gui.ViewModels;

public class CustomerRecord
{
    public string Login { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string Gender { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;
}