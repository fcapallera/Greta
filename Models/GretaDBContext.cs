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

        // Unable to generate entity type for table 'dbo.ProductInfo'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=tcp:greta-db.database.windows.net;Database=GretaDB; User ID=fcapallera@greta-db;Password=GretaVitro655&2019;Trusted_Connection=False; Encrypt=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Naquestions>(entity =>
            {
                entity.HasKey(e => e.QuestionId);

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
                entity.Property(e => e.Permission).HasDefaultValueSql("((5))");

                entity.Property(e => e.PrestashopId).HasColumnName("Prestashop_Id");
            });
        }
    }
}
