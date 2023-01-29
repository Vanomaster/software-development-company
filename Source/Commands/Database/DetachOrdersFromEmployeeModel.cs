using System.Collections.Generic;
using Dal.Entities;

namespace CleanModels;

public class DetachOrdersFromEmployeeModel
{
    public Employee Employee { get; set; }

    public List<Order> Orders { get; set; }
}