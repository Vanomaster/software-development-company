using System;
using System.Collections.Generic;
using CleanModels;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Customer = Dal.Entities.Customer;

namespace Dal;

/// <inheritdoc />
public class Context : DbContext
{
    /// <summary>
    /// Only for migrations. Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    public Context()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    /// <param name="options">Context options.</param>
    public Context(DbContextOptions<Context> options)
        : base(options)
    {
    }

    /// <summary>
    /// Users.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Customers.
    /// </summary>
    public DbSet<Customer> Customers { get; set; } = null!;

    /// <summary>
    /// Employees.
    /// </summary>
    public DbSet<Employee> Employees { get; set; } = null!;

    /// <summary>
    /// Passports.
    /// </summary>
    public DbSet<Passport> Passports { get; set; } = null!;

    /// <summary>
    /// Orders.
    /// </summary>
    public DbSet<Order> Orders { get; set; } = null!;

    /// <summary>
    /// StatementOfWorks.
    /// </summary>
    public DbSet<StatementOfWork> StatementOfWorks { get; set; } = null!;

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.EmailAddress).HasMaxLength(200);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(6);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Patronymic).HasMaxLength(100);
            entity.Property(e => e.Gender).HasConversion(
                gender => gender.ToString(),
                value => (Gender)Enum.Parse(typeof(Gender), value));

            entity.HasOne(d => d.User).WithOne(p => p.Customer).HasForeignKey<Customer>(d => d.Id);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.PassportSeries, e.PassportNumber }, "IX_Employees_PassportSeries_PassportNumber");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.EmailAddress).HasMaxLength(200);
            entity.Property(e => e.JobPosition).HasMaxLength(17);
            entity.Property(e => e.JobPosition).HasConversion(
                jobPosition => jobPosition.ToString(),
                value => (JobPosition)Enum.Parse(typeof(JobPosition), value));

            entity.HasOne(d => d.User).WithOne(p => p.Employee).HasForeignKey<Employee>(d => d.Id);

            entity.HasOne(d => d.Passport).WithOne(p => p.Employee).HasForeignKey<Employee>(d => new { d.PassportSeries, d.PassportNumber }).IsRequired();

            entity.HasMany(d => d.Orders).WithMany(p => p.Employees)
                .UsingEntity<Dictionary<string, object>>(
                    "EmployeeOrderRelations",
                    r => r.HasOne<Order>().WithMany()
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_EmployeeOrderRelations_Orders"),
                    l => l.HasOne<Employee>().WithMany()
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_EmployeeOrderRelations_Employees"),
                    j =>
                    {
                        j.HasKey("EmployeeId", "OrderId");
                    });
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.CustomerId, "IX_Orders_CustomerId");

            entity.HasAlternateKey(e => e.Number);
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreationDate).HasColumnType("datetime");
            entity.Property(e => e.DoneDate).HasColumnType("datetime");
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
            entity.Property(e => e.Status).HasMaxLength(26);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Status).HasConversion(
                orderStatus => orderStatus.ToString(),
                value => (OrderStatus)Enum.Parse(typeof(OrderStatus), value));

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders).HasForeignKey(d => d.CustomerId);
        });

        modelBuilder.Entity<Passport>(entity =>
        {
            entity.HasKey(e => new { e.Series, e.Number });

            entity.Property(e => e.BirthDate).HasColumnType("date");
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(6);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Patronymic).HasMaxLength(100);
            entity.Property(e => e.Residence).HasMaxLength(250);
            entity.Property(e => e.Gender).HasConversion(
                gender => gender.ToString(),
                value => (Gender)Enum.Parse(typeof(Gender), value));
        });

        modelBuilder.Entity<StatementOfWork>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_StatementOfWorks_OrderId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DoneDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(7);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Status).HasConversion(
                statementOfWorkStatus => statementOfWorkStatus.ToString(),
                value => (StatementOfWorkStatus)Enum.Parse(typeof(StatementOfWorkStatus), value));

            entity.HasOne(d => d.Order).WithOne(p => p.StatementOfWork).HasForeignKey<StatementOfWork>(d => d.OrderId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.HasAlternateKey(e => e.Login);
            entity.Property(e => e.Login).HasMaxLength(200);
            entity.Property(e => e.PasswordHash).HasMaxLength(100);
            entity.Property(e => e.PasswordSalt).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(13);
            entity.Property(e => e.Role).HasConversion(
                userRole => userRole.ToString(),
                value => (UserRole)Enum.Parse(typeof(UserRole), value));
        });
    }

    /// <summary>
    /// Only for migrations.
    /// </summary>
    /// <param name="optionsBuilder">Options builder only for migrations.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationHelper();
        optionsBuilder.UseSqlServer(configuration.MainConnectionString);
    }
}