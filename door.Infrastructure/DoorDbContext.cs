using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using door.Domain.Entities;

namespace door.Infrastructure
{
    public partial class DoorDbContext : DbContext
    {
        public DoorDbContext()
        {
        }

        public DoorDbContext(DbContextOptions<DoorDbContext> options)
            : base(options)
        {
        }


        public virtual DbSet<DataEntry> DataEntries { get; set; }

        public virtual DbSet<MasterDoorStatus> MasterDoorStatuses { get; set; }


//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//            => optionsBuilder.UseSqlite("Data Source=C:\\Users\\Root\\Documents\\Githubアップロード\\door.UI\\door.UI\\door.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataEntry>(entity =>
            {
                entity.Property(e => e.DoorStatusId).HasColumnName("Door_Status_id");

                entity.HasOne(d => d.DoorStatus).WithMany(p => p.DataEntries)
                    .HasForeignKey(d => d.DoorStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<MasterDoorStatus>(entity =>
            {
                entity.ToTable("MasterDoorStatus");

                entity.Property(e => e.DoorStatusName).HasColumnName("Door_Status_Name");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}