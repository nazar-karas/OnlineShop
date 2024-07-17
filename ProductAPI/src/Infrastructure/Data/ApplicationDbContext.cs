using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<AttachedFile> AttachedFiles { get; set; }
        public DbSet<FeatureGroup> FeatureGroups { get; set; }
        public DbSet<FeatureValue> FeaturesValues { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            bool created = Database.EnsureCreated();
            if (created)
            {
                // embed mock data
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasOne(x => x.FeatureGroup)
                .WithMany(x => x.Products)
                .HasForeignKey(z => z.FeatureGroupId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<AttachedFile>(entity =>
            {
                entity.HasOne(x => x.Product)
                .WithMany(y => y.AttachedFiles)
                .HasForeignKey(z => z.ProductId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<FeatureGroup>(entity =>
            {
                entity.HasMany(x => x.Products)
                .WithOne(y => y.FeatureGroup);

                entity.Property(x => x.Timestamp)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<Feature>(entity =>
            {
                entity.HasOne(x => x.FeatureGroup)
                .WithMany(y => y.Features)
                .HasForeignKey(z => z.FeatureGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FeatureValue>(entity =>
            {
                entity.HasOne(x => x.Feature)
                .WithMany(y => y.FeatureValues)
                .HasForeignKey(z => z.FeatureId);

                entity.HasOne(x => x.Product)
                .WithMany(y => y.FeaturesValues)
                .HasForeignKey(z => z.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<Subcategory>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Category)
                .WithMany(y => y.Subcategories)
                .HasForeignKey(z => z.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
