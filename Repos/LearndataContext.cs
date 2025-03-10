using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using clearAccess.Repos.Models;

namespace clearAccess.Repos;

public partial class LearndataContext : DbContext
{
    public LearndataContext()
    {
    }

    public LearndataContext(DbContextOptions<LearndataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<TblRefreshtoken> TblRefreshtokens { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PK__customer__A25C5AA67A7C2CC6");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.Username).HasName("PK__tbl_user__F3DBC573C404EE72");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
