using System;
using CleanModels;

namespace Dal.Entities;

public class StatementOfWork : IEntity
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Text { get; set; } = null!;

    public DateTime? DoneDate { get; set; }

    public StatementOfWorkStatus Status { get; set; }

    public Guid OrderId { get; set; }

    public virtual Order Order { get; set; }
}