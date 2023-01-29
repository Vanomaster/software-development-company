using System;
using System.Collections.Generic;
using CleanModels;

namespace Dal.Entities;

public class Order : IEntity
{
    public Guid Id { get; set; }

    public int Number { get; set; }

    public string Title { get; set; } = null!;

    public string Text { get; set; } = null!;

    public DateTime CreationDate { get; set; }

    public DateTime? DoneDate { get; set; }

    public int? Cost { get; set; }

    public OrderStatus Status { get; set; }

    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; }

    public List<Employee> Employees { get; set; } = new ();

    public StatementOfWork StatementOfWork { get; set; }
}
