using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CoreBot.Models
{
    public partial class GretaDBContext : DbContext
    {
        public GretaDBContext()
        {
        }

        public GretaDBContext(DbContextOptions<GretaDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Naquestions> Naquestions { get; set; }
        public virtual DbSet<OrderLine> OrderLine { get; set; }
        public virtual DbSet<UserProfile> UserProfile { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Naquestions>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("NAQuestions");

                entity.Property(e => e.QuestionText)
                    .IsRequired()
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Naquestions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NAQuestions_UserProfile");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<OrderLine>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.OrderLine)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderLine_UserProfile");
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CreationDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Permission).HasDefaultValueSql("((5))");

                entity.Property(e => e.PrestashopId).HasColumnName("Prestashop_Id");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }
    }
}
