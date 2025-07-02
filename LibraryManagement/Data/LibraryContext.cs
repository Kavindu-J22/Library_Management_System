using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Data
{
    /// <summary>
    /// Entity Framework Core database context for the Library Management System
    /// </summary>
    public class LibraryContext : DbContext
    {
        /// <summary>
        /// Constructor for LibraryContext
        /// </summary>
        /// <param name="options">Database context options</param>
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
        }

        /// <summary>
        /// Default constructor for LibraryContext
        /// </summary>
        public LibraryContext()
        {
        }

        /// <summary>
        /// DbSet for Books table
        /// </summary>
        public DbSet<Book> Books { get; set; } = null!;

        /// <summary>
        /// DbSet for Borrowers table
        /// </summary>
        public DbSet<Borrower> Borrowers { get; set; } = null!;

        /// <summary>
        /// DbSet for Transactions table
        /// </summary>
        public DbSet<Transaction> Transactions { get; set; } = null!;

        /// <summary>
        /// Configure the database connection
        /// </summary>
        /// <param name="optionsBuilder">Options builder for database configuration</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Default connection string - should be moved to configuration file in production
                string connectionString = "Server=localhost;Database=LibraryManagement;Uid=root;Pwd=;";
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                
                // Enable sensitive data logging in development
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.LogTo(Console.WriteLine);
            }
        }

        /// <summary>
        /// Configure entity relationships and constraints
        /// </summary>
        /// <param name="modelBuilder">Model builder for entity configuration</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Book entity
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.BookId);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Author).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ISBN).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.ISBN).IsUnique();
                entity.Property(e => e.Publisher).HasMaxLength(255);
                entity.Property(e => e.Genre).HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
                
                // Add check constraints
                entity.HasCheckConstraint("CK_Book_TotalCopies", "TotalCopies >= 0");
                entity.HasCheckConstraint("CK_Book_AvailableCopies", "AvailableCopies >= 0 AND AvailableCopies <= TotalCopies");
                entity.HasCheckConstraint("CK_Book_PublicationYear", "PublicationYear > 1000 AND PublicationYear <= YEAR(CURDATE())");
            });

            // Configure Borrower entity
            modelBuilder.Entity<Borrower>(entity =>
            {
                entity.HasKey(e => e.BorrowerId);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.MembershipDate).HasDefaultValueSql("CURDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.MaxBooksAllowed).HasDefaultValue(5);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
                
                // Configure enum conversion
                entity.Property(e => e.MembershipType)
                    .HasConversion<string>()
                    .HasMaxLength(20);
                
                // Add check constraint
                entity.HasCheckConstraint("CK_Borrower_MaxBooks", "MaxBooksAllowed > 0 AND MaxBooksAllowed <= 20");
            });

            // Configure Transaction entity
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId);
                entity.Property(e => e.BookId).IsRequired();
                entity.Property(e => e.BorrowerId).IsRequired();
                entity.Property(e => e.TransactionDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.FineAmount).HasColumnType("decimal(10,2)").HasDefaultValue(0.00m);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
                
                // Configure enum conversions
                entity.Property(e => e.TransactionType)
                    .HasConversion<string>()
                    .HasMaxLength(20);
                
                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(TransactionStatus.Active);
                
                // Configure foreign key relationships
                entity.HasOne(e => e.Book)
                    .WithMany(b => b.Transactions)
                    .HasForeignKey(e => e.BookId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Borrower)
                    .WithMany(b => b.Transactions)
                    .HasForeignKey(e => e.BorrowerId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Add check constraints
                entity.HasCheckConstraint("CK_Transaction_FineAmount", "FineAmount >= 0");
            });

            // Create indexes for better performance
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.Title)
                .HasDatabaseName("IX_Books_Title");
            
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.Author)
                .HasDatabaseName("IX_Books_Author");
            
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.Genre)
                .HasDatabaseName("IX_Books_Genre");
            
            modelBuilder.Entity<Borrower>()
                .HasIndex(b => new { b.LastName, b.FirstName })
                .HasDatabaseName("IX_Borrowers_Name");
            
            modelBuilder.Entity<Borrower>()
                .HasIndex(b => new { b.MembershipType, b.IsActive })
                .HasDatabaseName("IX_Borrowers_Membership");
            
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Status)
                .HasDatabaseName("IX_Transactions_Status");
            
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.DueDate)
                .HasDatabaseName("IX_Transactions_DueDate");
            
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => new { t.TransactionType, t.TransactionDate })
                .HasDatabaseName("IX_Transactions_Type_Date");
        }

        /// <summary>
        /// Override SaveChanges to automatically update timestamps
        /// </summary>
        /// <returns>Number of affected records</returns>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically update timestamps
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of affected records</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Updates timestamps for entities being modified
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Book book)
                {
                    if (entry.State == EntityState.Added)
                        book.CreatedDate = DateTime.Now;
                    book.UpdatedDate = DateTime.Now;
                }
                else if (entry.Entity is Borrower borrower)
                {
                    if (entry.State == EntityState.Added)
                        borrower.CreatedDate = DateTime.Now;
                    borrower.UpdatedDate = DateTime.Now;
                }
                else if (entry.Entity is Transaction transaction)
                {
                    if (entry.State == EntityState.Added)
                        transaction.CreatedDate = DateTime.Now;
                    transaction.UpdatedDate = DateTime.Now;
                }
            }
        }
    }
}
