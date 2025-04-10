using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TodoApp.Data;

public partial class TodoDbContext : DbContext
{
    public TodoDbContext()
    {
    }

    public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cpr> Cprs { get; set; }

    public virtual DbSet<TodoList> TodoLists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cpr>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cpr__3214EC07F9689AA0");

            entity.ToTable("Cpr");

            entity.Property(e => e.CprNr).HasMaxLength(500);
            entity.Property(e => e.User).HasMaxLength(200);
        });

        modelBuilder.Entity<TodoList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TodoList__3214EC07DA7A60E9");

            entity.ToTable("TodoList");

            entity.Property(e => e.Item).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.TodoLists)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Todolist_Cpr");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
