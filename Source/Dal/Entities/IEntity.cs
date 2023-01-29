using System;

namespace Dal.Entities;

/// <summary>
/// Entity.
/// </summary>
public interface IEntity
{
    public Guid Id { get; set; }
}