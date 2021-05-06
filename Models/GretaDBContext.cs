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
        public virtual DbSet<OrderLine> OrderLine { get; set; }
        public virtual DbSet<OrderRequest> OrderRequest { get; set; }
        public virtual DbSet<UserProfile> UserProfile { get; set; }
        public virtual DbSet<Naquestions> Naquestions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasIndex(e => e.UserId)
                    .HasName("FK_Cart_UserProfile");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Active).HasColumnType("bit(1)");

                entity.Property(e => e.UserId).HasColumnType("int(11)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Cart)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cart_UserProfile");
            });

            modelBuilder.Entity<OrderLine>(entity =>
            {
                entity.HasIndex(e => e.CartId)
                    .HasName("FK_OrderLine_Cart");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Amount).HasColumnType("int(11)");

                entity.Property(e => e.CartId).HasColumnType("int(11)");

                entity.Property(e => e.ProductId).HasColumnType("int(11)");

                entity.HasOne(d => d.Cart)
                    .WithMany(p => p.OrderLine)
                    .HasForeignKey(d => d.CartId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderLine_Cart");
            });

            modelBuilder.Entity<OrderRequest>(entity =>
            {
                entity.HasIndex(e => e.CartId)
                    .HasName("FK_OrderRequest_Cart");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.CartId).HasColumnType("int(11)");

                entity.Property(e => e.ConfirmationDate).HasColumnType("timestamp");

                entity.Property(e => e.Confirmed).HasColumnType("bit(1)");

                entity.Property(e => e.CreationDate)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.HasOne(d => d.Cart)
                    .WithMany(p => p.OrderRequest)
                    .HasForeignKey(d => d.CartId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderRequest_Cart");
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.BotUserId)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.CreationDate).HasColumnType("int(11)");

                entity.Property(e => e.Permission).HasColumnType("int(11)");

                entity.Property(e => e.PrestashopId).HasColumnType("int(11)");

                entity.Property(e => e.Validated).HasColumnType("bit(1)");
            });

            modelBuilder.Entity<Naquestions>(entity =>
            {
                entity.ToTable("NAQuestions");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.QuestionText)
                    .IsRequired()
                    .HasColumnType("varchar(200)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Naquestions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NAQuestions_UserProfile");
            });
        }
    }
}
