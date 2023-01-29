using Dal.Entities;

namespace CleanModels;

public class AttachOrderToEmployeeModel
{
    public Employee Employee { get; set; }

    public Order Order { get; set; }
}