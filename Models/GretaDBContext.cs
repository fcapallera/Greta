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

        public virtual DbSet<Cart> Cart { get; set; }
        public virtual DbSet<Naquestions> Naquestions { get; set; }
        public virtual DbSet<OrderLine> OrderLine { get; set; }
        public virtual DbSet<OrderRequest> OrderRequest { get; set; }
        public virtual DbSet<UserProfile> UserProfile { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Cart)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserCart_UserProfile");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Naquestions>(entity =>
            {
                entity.ToTable("NAQuestions");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
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
                entity.Property(e => e.Amount).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Cart)
                    .WithMany(p => p.OrderLine)
                    .HasForeignKey(d => d.CartId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderLine_OrderLine");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<OrderRequest>(entity =>
            {
                entity.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Cart)
                    .WithMany(p => p.OrderRequest)
                    .HasForeignKey(d => d.CartId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderRequest_Cart");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.Property(e => e.BotUserId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreationDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Permission).HasDefaultValueSql("((5))");

                entity.Property(e => e.PrestashopId).HasColumnName("Prestashop_Id");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }
    }
}
