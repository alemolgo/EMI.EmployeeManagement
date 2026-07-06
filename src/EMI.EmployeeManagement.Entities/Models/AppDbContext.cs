using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EMI.EmployeeManagement.Entities.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeRole> EmployeeRoles { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<PositionHistory> PositionHistories { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("employee_pkey");

            entity.ToTable("employee", "emi");

            entity.Property(e => e.EmployeeId)
                .HasDefaultValueSql("nextval('emi.employee_code_seq'::regclass)")
                .HasColumnName("employee_id");
            entity.Property(e => e.EmployeeCurrentPositionId).HasColumnName("employee_current_position_id");
            entity.Property(e => e.EmployeeName)
                .HasMaxLength(100)
                .HasColumnName("employee_name");
            entity.Property(e => e.EmployeePasswordHash)
                .HasMaxLength(255)
                .HasColumnName("employee_password_hash");
            entity.Property(e => e.EmployeeSalary)
                .HasPrecision(18, 2)
                .HasColumnName("employee_salary");

            entity.HasOne(d => d.EmployeeCurrentPosition).WithMany(p => p.Employees)
                .HasForeignKey(d => d.EmployeeCurrentPositionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("employee_current_position_fkey");
        });

        modelBuilder.Entity<EmployeeRole>(entity =>
        {
            entity.HasKey(e => e.EmployeeRoleId).HasName("employee_role_pkey");

            entity.ToTable("employee_role", "emi");

            entity.HasIndex(e => new { e.EmployeeId, e.RoleId }, "uq_employee_role").IsUnique();

            entity.Property(e => e.EmployeeRoleId)
                .HasDefaultValueSql("nextval('emi.employee_code_seq'::regclass)")
                .HasColumnName("employee_role_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeRoles)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("employee_role_employee_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.EmployeeRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("employee_role_role_fkey");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.PositionId).HasName("position_pkey");

            entity.ToTable("position", "emi");

            entity.HasIndex(e => e.PositionName, "position_ukey").IsUnique();

            entity.Property(e => e.PositionId)
                .HasDefaultValueSql("nextval('emi.position_code_seq'::regclass)")
                .HasColumnName("position_id");
            entity.Property(e => e.PositionIsManager)
                .HasDefaultValue(false)
                .HasColumnName("position_is_manager");
            entity.Property(e => e.PositionName)
                .HasMaxLength(50)
                .HasColumnName("position_name");
        });

        modelBuilder.Entity<PositionHistory>(entity =>
        {
            entity.HasKey(e => e.PositionHistoryId).HasName("position_history_pkey");

            entity.ToTable("position_history", "emi");

            entity.HasIndex(e => e.PositionHistoryEmployeeId, "uq_position_history_employee_active")
                .IsUnique()
                .HasFilter("position_history_is_active");

            entity.Property(e => e.PositionHistoryId)
                .HasDefaultValueSql("nextval('emi.position_history_code_seq'::regclass)")
                .HasColumnName("position_history_id");
            entity.Property(e => e.PositionHistoryEmployeeId).HasColumnName("position_history_employee_id");
            entity.Property(e => e.PositionHistoryEndDate).HasColumnName("position_history_end_date");
            entity.Property(e => e.PositionHistoryIsActive)
                .HasDefaultValue(true)
                .HasColumnName("position_history_is_active");
            entity.Property(e => e.PositionHistoryPositionId).HasColumnName("position_history_position_id");
            entity.Property(e => e.PositionHistoryStartDate).HasColumnName("position_history_start_date");

            entity.HasOne(d => d.PositionHistoryEmployee)
                .WithMany(p => p.PositionHistories)
                .HasForeignKey(d => d.PositionHistoryEmployeeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("position_history_employee_id_fkey");


            entity.HasOne(d => d.PositionHistoryPosition).WithMany(p => p.PositionHistories)
                .HasForeignKey(d => d.PositionHistoryPositionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("position_history_position_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("role_pkey");

            entity.ToTable("role", "emi");

            entity.HasIndex(e => e.RoleName, "role_ukey").IsUnique();

            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("nextval('emi.role_code_seq'::regclass)")
                .HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });
        modelBuilder.HasSequence("employee_code_seq", "emi").HasMax(99999L);
        modelBuilder.HasSequence("position_code_seq", "emi").HasMax(99999L);
        modelBuilder.HasSequence("position_history_code_seq", "emi").HasMax(99999L);
        modelBuilder.HasSequence("role_code_seq", "emi").HasMax(99999L);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
