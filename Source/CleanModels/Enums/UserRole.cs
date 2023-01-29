using System.ComponentModel;

namespace CleanModels;

/// <summary>
/// User role.
/// </summary>
public enum UserRole : byte
{
    /// <summary>
    /// Customer.
    /// </summary>
    [Description("Заказчик")]
    Customer = 1,

    /// <summary>
    /// Employee.
    /// </summary>
    [Description("Сотрудник")]
    Employee = 2,

    /// <summary>
    /// Administrator.
    /// </summary>
    [Description("Администратор")]
    Administrator = 3,
}