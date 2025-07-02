using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace LibraryManagement.Examples
{
    /// <summary>
    /// Practical debugging examples demonstrating common issues and solutions
    /// </summary>
    public class DebuggingExamples
    {
        private readonly LibraryContext _context;

        public DebuggingExamples(LibraryContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Example 1: Database Connection Debugging
        /// Demonstrates how to debug MySQL connection issues
        /// </summary>
        public async Task<bool> TestDatabaseConnectionAsync()
        {
            Console.WriteLine("=== DATABASE CONNECTION DEBUGGING ===");
            
            try
            {
                // 🔴 Set breakpoint here to start debugging
                Console.WriteLine("Testing database connection...");
                
                // Test basic connectivity
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    Console.WriteLine("❌ Cannot connect to database");
                    return false;
                }
                
                // Test query execution
                var bookCount = await _context.Books.CountAsync();
                Console.WriteLine($"✅ Database connected successfully. Books in database: {bookCount}");
                
                return true;
            }
            catch (MySqlException ex)
            {
                // 🔴 Set breakpoint here to examine MySQL-specific errors
                Console.WriteLine($"❌ MySQL Error: {ex.Message}");
                Console.WriteLine($"Error Number: {ex.Number}");
                
                // Common MySQL error codes and solutions
                switch (ex.Number)
                {
                    case 1045: // Access denied
                        Console.WriteLine("💡 Solution: Check username and password in connection string");
                        break;
                    case 1049: // Unknown database
                        Console.WriteLine("💡 Solution: Create the database or check database name");
                        break;
                    case 2003: // Can't connect to server
                        Console.WriteLine("💡 Solution: Check if MySQL server is running and port is correct");
                        break;
                    default:
                        Console.WriteLine($"💡 Check MySQL documentation for error {ex.Number}");
                        break;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                // 🔴 Set breakpoint here for general exceptions
                Console.WriteLine($"❌ General Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Example 2: Null Reference Exception Debugging
        /// Shows how to debug and prevent null reference exceptions
        /// </summary>
        public async Task<string> GetBookTitleDebuggingExample(int bookId)
        {
            Console.WriteLine("=== NULL REFERENCE DEBUGGING ===");
            
            try
            {
                // 🔴 Set breakpoint here to inspect bookId value
                Console.WriteLine($"Looking for book with ID: {bookId}");
                
                // This could return null - common source of NullReferenceException
                var book = await _context.Books.FindAsync(bookId);
                
                // 🔴 Set breakpoint here to check if book is null
                if (book == null)
                {
                    Console.WriteLine($"❌ Book with ID {bookId} not found");
                    return "Book not found";
                }
                
                // 🔴 Set breakpoint here to inspect book properties
                Console.WriteLine($"✅ Found book: {book.Title}");
                
                // Safe access to properties after null check
                return book.Title;
            }
            catch (NullReferenceException ex)
            {
                // 🔴 Set breakpoint here if null reference occurs
                Console.WriteLine($"❌ Null Reference Error: {ex.Message}");
                Console.WriteLine("💡 Check: Did you verify the object is not null before accessing properties?");
                return "Error: Null reference";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected Error: {ex.Message}");
                return "Error occurred";
            }
        }

        /// <summary>
        /// Example 3: Entity Framework Exception Debugging
        /// Demonstrates debugging EF Core issues
        /// </summary>
        public async Task<bool> AddBookWithDebuggingAsync(Book book)
        {
            Console.WriteLine("=== ENTITY FRAMEWORK DEBUGGING ===");
            
            try
            {
                // 🔴 Set breakpoint here to inspect book object
                Console.WriteLine($"Attempting to add book: {book?.Title ?? "NULL"}");
                
                // Input validation with debugging info
                if (book == null)
                {
                    Console.WriteLine("❌ Book object is null");
                    return false;
                }
                
                if (string.IsNullOrEmpty(book.Title))
                {
                    Console.WriteLine("❌ Book title is required");
                    return false;
                }
                
                if (string.IsNullOrEmpty(book.ISBN))
                {
                    Console.WriteLine("❌ Book ISBN is required");
                    return false;
                }
                
                // 🔴 Set breakpoint here before database operation
                Console.WriteLine("Adding book to context...");
                _context.Books.Add(book);
                
                // 🔴 Set breakpoint here before SaveChanges
                Console.WriteLine("Saving changes to database...");
                var result = await _context.SaveChangesAsync();
                
                // 🔴 Set breakpoint here to check result
                Console.WriteLine($"✅ Book added successfully. Changes saved: {result}");
                return true;
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx)
            {
                // 🔴 Set breakpoint here for database constraint violations
                Console.WriteLine($"❌ Database Update Error: {ex.Message}");
                Console.WriteLine($"MySQL Error: {mysqlEx.Message}");
                Console.WriteLine($"MySQL Error Number: {mysqlEx.Number}");
                
                // Handle specific constraint violations
                if (mysqlEx.Number == 1062) // Duplicate entry
                {
                    Console.WriteLine("💡 Solution: Book with this ISBN already exists");
                }
                else if (mysqlEx.Number == 1452) // Foreign key constraint
                {
                    Console.WriteLine("💡 Solution: Check foreign key relationships");
                }
                
                return false;
            }
            catch (DbUpdateException ex)
            {
                // 🔴 Set breakpoint here for general EF update errors
                Console.WriteLine($"❌ Entity Framework Update Error: {ex.Message}");
                Console.WriteLine("💡 Check: Entity validation, constraints, and relationships");
                return false;
            }
            catch (Exception ex)
            {
                // 🔴 Set breakpoint here for unexpected errors
                Console.WriteLine($"❌ Unexpected Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Example 4: Business Logic Validation Debugging
        /// Shows how to debug complex business rules
        /// </summary>
        public async Task<bool> BorrowBookWithDebuggingAsync(int bookId, int borrowerId)
        {
            Console.WriteLine("=== BUSINESS LOGIC DEBUGGING ===");
            
            try
            {
                // 🔴 Set breakpoint here to start debugging business logic
                Console.WriteLine($"Attempting to borrow book {bookId} for borrower {borrowerId}");
                
                // Step 1: Retrieve entities with debugging
                Console.WriteLine("Step 1: Retrieving book...");
                var book = await _context.Books.FindAsync(bookId);
                
                // 🔴 Set breakpoint here to inspect book
                if (book == null)
                {
                    Console.WriteLine($"❌ Book {bookId} not found");
                    return false;
                }
                Console.WriteLine($"✅ Book found: {book.Title}");
                
                // Step 2: Retrieve borrower with related data
                Console.WriteLine("Step 2: Retrieving borrower...");
                var borrower = await _context.Borrowers
                    .Include(b => b.Transactions)
                    .FirstOrDefaultAsync(b => b.BorrowerId == borrowerId);
                
                // 🔴 Set breakpoint here to inspect borrower
                if (borrower == null)
                {
                    Console.WriteLine($"❌ Borrower {borrowerId} not found");
                    return false;
                }
                Console.WriteLine($"✅ Borrower found: {borrower.FullName}");
                
                // Step 3: Check book availability
                Console.WriteLine("Step 3: Checking book availability...");
                // 🔴 Set breakpoint here to inspect availability
                if (!book.IsAvailable())
                {
                    Console.WriteLine($"❌ Book not available. Available copies: {book.AvailableCopies}");
                    return false;
                }
                Console.WriteLine($"✅ Book available. Copies: {book.AvailableCopies}/{book.TotalCopies}");
                
                // Step 4: Check borrower limits
                Console.WriteLine("Step 4: Checking borrower limits...");
                var currentBorrowings = borrower.CurrentBorrowedBooks;
                // 🔴 Set breakpoint here to inspect borrowing limits
                if (!borrower.CanBorrowMoreBooks())
                {
                    Console.WriteLine($"❌ Borrower limit exceeded. Current: {currentBorrowings}, Max: {borrower.MaxBooksAllowed}");
                    return false;
                }
                Console.WriteLine($"✅ Borrower can borrow more books. Current: {currentBorrowings}/{borrower.MaxBooksAllowed}");
                
                // Step 5: Check for overdue books
                Console.WriteLine("Step 5: Checking for overdue books...");
                // 🔴 Set breakpoint here to inspect overdue status
                if (borrower.HasOverdueBooks())
                {
                    Console.WriteLine($"❌ Borrower has overdue books");
                    return false;
                }
                Console.WriteLine($"✅ No overdue books found");
                
                // Step 6: Create transaction
                Console.WriteLine("Step 6: Creating transaction...");
                var transaction = new Transaction
                {
                    BookId = bookId,
                    BorrowerId = borrowerId,
                    TransactionType = TransactionType.Borrow,
                    TransactionDate = DateTime.Now,
                    DueDate = DateTime.Today.AddDays(14),
                    Status = TransactionStatus.Active
                };
                
                // 🔴 Set breakpoint here to inspect transaction
                Console.WriteLine($"Transaction created: Due date {transaction.DueDate:yyyy-MM-dd}");
                
                // Step 7: Update book and save
                Console.WriteLine("Step 7: Updating book availability and saving...");
                book.BorrowCopy();
                _context.Transactions.Add(transaction);
                
                // 🔴 Set breakpoint here before saving
                var result = await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ Book borrowed successfully! Transaction ID: {transaction.TransactionId}");
                return true;
            }
            catch (Exception ex)
            {
                // 🔴 Set breakpoint here for any errors in business logic
                Console.WriteLine($"❌ Error in borrowing process: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Example 5: Performance Debugging
        /// Shows how to identify and debug performance issues
        /// </summary>
        public async Task<List<Book>> GetBooksWithPerformanceDebuggingAsync()
        {
            Console.WriteLine("=== PERFORMANCE DEBUGGING ===");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                // 🔴 Set breakpoint here to start performance monitoring
                Console.WriteLine("Starting book retrieval...");
                
                // Potentially slow query - loads all books with transactions
                var books = await _context.Books
                    .Include(b => b.Transactions)
                    .ThenInclude(t => t.Borrower)
                    .ToListAsync();
                
                stopwatch.Stop();
                
                // 🔴 Set breakpoint here to check performance metrics
                Console.WriteLine($"✅ Retrieved {books.Count} books in {stopwatch.ElapsedMilliseconds}ms");
                
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    Console.WriteLine("⚠️ Performance Warning: Query took longer than 1 second");
                    Console.WriteLine("💡 Consider: Adding indexes, pagination, or reducing included data");
                }
                
                return books;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"❌ Error retrieving books after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
                return new List<Book>();
            }
        }
    }
}
