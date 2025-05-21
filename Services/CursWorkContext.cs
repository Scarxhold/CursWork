using Microsoft.EntityFrameworkCore;

namespace CursWork.Models
{
    public class CursWorkContext : DbContext
    {
        public CursWorkContext(DbContextOptions options) : base(options) { }

        // Оголошуємо таблиці бази даних
        public DbSet<Category> Categories { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Maintenance> Maintenance { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Налаштування зв'язків
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Category)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.Category_Code)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Vehicle)
                .WithMany()
                .HasForeignKey(i => i.Vehicle_Code)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Vehicle)
                .WithMany()
                .HasForeignKey(t => t.Vehicle_Code)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Customer)
                .WithMany()
                .HasForeignKey(t => t.Customer_Code)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Employee)
                .WithMany()
                .HasForeignKey(t => t.Employee_Code)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Maintenance>()
                .HasOne(m => m.Vehicle)
                .WithMany()
                .HasForeignKey(m => m.Vehicle_Code)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>().Property(c => c.Code).HasColumnName("Code");
            modelBuilder.Entity<Vehicle>().Property(v => v.Code).HasColumnName("Code");
            modelBuilder.Entity<Inventory>().Property(i => i.Code).HasColumnName("Code");
            modelBuilder.Entity<Employee>().Property(e => e.Code).HasColumnName("Code");
            modelBuilder.Entity<Transaction>().Property(t => t.Code).HasColumnName("Code");
            modelBuilder.Entity<Maintenance>().Property(m => m.Code).HasColumnName("Code");
            modelBuilder.Entity<Customer>().Property(c => c.Code).HasColumnName("Code");

            base.OnModelCreating(modelBuilder);
        }
    }
}