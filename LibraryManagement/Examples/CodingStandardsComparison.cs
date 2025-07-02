using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Examples
{
    /// <summary>
    /// Demonstrates the difference between good and poor coding practices
    /// This file shows side-by-side comparisons of code quality
    /// </summary>
    public class CodingStandardsComparison
    {
        #region BAD EXAMPLES - What NOT to do

        /// <summary>
        /// ❌ POOR CODE EXAMPLE - Violates multiple coding standards
        /// Problems: Poor naming, no error handling, no documentation, mixed responsibilities
        /// </summary>
        public class badBookSvc
        {
            public LibraryContext db;
            
            public badBookSvc(LibraryContext c) { db = c; }
            
            // Poor method name, no error handling, no validation
            public bool addbook(string t, string a, string i, int copies)
            {
                var b = new Book();
                b.Title = t;
                b.Author = a;
                b.ISBN = i;
                b.TotalCopies = copies;
                b.AvailableCopies = copies;
                db.Books.Add(b);
                db.SaveChanges(); // Could throw exception
                return true;
            }
            
            // No async, poor error handling, unclear logic
            public bool borrowbook(int bid, int uid)
            {
                var book = db.Books.Find(bid);
                var user = db.Borrowers.Find(uid);
                book.AvailableCopies--; // Could cause null reference
                var t = new Transaction();
                t.BookId = bid;
                t.BorrowerId = uid;
                t.TransactionDate = DateTime.Now;
                t.DueDate = DateTime.Now.AddDays(14);
                db.Transactions.Add(t);
                db.SaveChanges();
                return true;
            }
            
            // No documentation, unclear purpose, poor naming
            public List<Book> getbooks()
            {
                return db.Books.ToList(); // Loads all books - performance issue
            }
        }

        #endregion

        #region GOOD EXAMPLES - Professional coding standards

        /// <summary>
        /// ✅ EXCELLENT CODE EXAMPLE - Follows all coding standards
        /// Demonstrates: Proper naming, error handling, documentation, single responsibility
        /// </summary>
        public class BookService
        {
            private readonly LibraryContext _context;

            /// <summary>
            /// Initializes a new instance of the BookService class
            /// </summary>
            /// <param name="context">The database context for library operations</param>
            /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
            public BookService(LibraryContext context)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
            }

            /// <summary>
            /// Adds a new book to the library inventory with comprehensive validation
            /// </summary>
            /// <param name="title">The title of the book</param>
            /// <param name="author">The author of the book</param>
            /// <param name="isbn">The ISBN of the book (must be unique)</param>
            /// <param name="totalCopies">The total number of copies to add</param>
            /// <returns>True if the book was added successfully, false otherwise</returns>
            /// <exception cref="ArgumentException">Thrown when required parameters are invalid</exception>
            /// <example>
            /// <code>
            /// var success = await bookService.AddBookAsync("1984", "George Orwell", "978-0-452-28423-4", 3);
            /// </code>
            /// </example>
            public async Task<bool> AddBookAsync(string title, string author, string isbn, int totalCopies)
            {
                try
                {
                    // Comprehensive input validation with clear error messages
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        Console.WriteLine("❌ Error: Book title cannot be empty");
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(author))
                    {
                        Console.WriteLine("❌ Error: Book author cannot be empty");
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(isbn))
                    {
                        Console.WriteLine("❌ Error: Book ISBN cannot be empty");
                        return false;
                    }

                    if (totalCopies <= 0)
                    {
                        Console.WriteLine("❌ Error: Total copies must be greater than zero");
                        return false;
                    }

                    // Check for duplicate ISBN with proper async operation
                    var existingBook = await _context.Books
                        .FirstOrDefaultAsync(b => b.ISBN == isbn);

                    if (existingBook != null)
                    {
                        Console.WriteLine($"❌ Error: Book with ISBN {isbn} already exists");
                        return false;
                    }

                    // Create book object with proper initialization
                    var book = new Book
                    {
                        Title = title.Trim(),
                        Author = author.Trim(),
                        ISBN = isbn.Trim(),
                        TotalCopies = totalCopies,
                        AvailableCopies = totalCopies,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    // Add to context and save with proper error handling
                    _context.Books.Add(book);
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        Console.WriteLine($"✅ Book '{title}' added successfully with ID: {book.BookId}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("❌ Error: Failed to save book to database");
                        return false;
                    }
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"❌ Database Error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Unexpected Error: {ex.Message}");
                    return false;
                }
            }

            /// <summary>
            /// Allows a borrower to borrow a book with comprehensive business rule validation
            /// </summary>
            /// <param name="bookId">The unique identifier of the book to borrow</param>
            /// <param name="borrowerId">The unique identifier of the borrower</param>
            /// <param name="borrowDays">Number of days to borrow the book (default: 14)</param>
            /// <returns>True if the book was borrowed successfully, false otherwise</returns>
            /// <remarks>
            /// This method performs extensive validation including:
            /// - Book existence and availability
            /// - Borrower existence and active status
            /// - Borrowing limits and overdue books check
            /// - Transaction creation and book availability update
            /// </remarks>
            public async Task<bool> BorrowBookAsync(int bookId, int borrowerId, int borrowDays = 14)
            {
                try
                {
                    // Input validation with clear error messages
                    if (bookId <= 0)
                    {
                        Console.WriteLine("❌ Error: Invalid book ID");
                        return false;
                    }

                    if (borrowerId <= 0)
                    {
                        Console.WriteLine("❌ Error: Invalid borrower ID");
                        return false;
                    }

                    if (borrowDays <= 0 || borrowDays > 365)
                    {
                        Console.WriteLine("❌ Error: Borrow days must be between 1 and 365");
                        return false;
                    }

                    // Retrieve entities with related data using proper async operations
                    var book = await _context.Books.FindAsync(bookId);
                    if (book == null)
                    {
                        Console.WriteLine($"❌ Error: Book with ID {bookId} not found");
                        return false;
                    }

                    var borrower = await _context.Borrowers
                        .Include(b => b.Transactions)
                        .FirstOrDefaultAsync(b => b.BorrowerId == borrowerId);

                    if (borrower == null)
                    {
                        Console.WriteLine($"❌ Error: Borrower with ID {borrowerId} not found");
                        return false;
                    }

                    // Business rule validation with detailed feedback
                    if (!book.IsAvailable())
                    {
                        Console.WriteLine($"❌ Error: Book '{book.Title}' is not available. Available copies: {book.AvailableCopies}");
                        return false;
                    }

                    if (!borrower.IsActive)
                    {
                        Console.WriteLine($"❌ Error: Borrower account is inactive");
                        return false;
                    }

                    if (!borrower.CanBorrowMoreBooks())
                    {
                        Console.WriteLine($"❌ Error: Borrower has reached maximum borrowing limit ({borrower.MaxBooksAllowed})");
                        return false;
                    }

                    if (borrower.HasOverdueBooks())
                    {
                        Console.WriteLine($"❌ Error: Borrower has overdue books. Please return them first");
                        return false;
                    }

                    // Create transaction with proper data
                    var transaction = new Transaction
                    {
                        BookId = bookId,
                        BorrowerId = borrowerId,
                        TransactionType = TransactionType.Borrow,
                        TransactionDate = DateTime.Now,
                        DueDate = DateTime.Today.AddDays(borrowDays),
                        Status = TransactionStatus.Active,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    // Update book availability using business logic method
                    var borrowSuccess = book.BorrowCopy();
                    if (!borrowSuccess)
                    {
                        Console.WriteLine($"❌ Error: Failed to update book availability");
                        return false;
                    }

                    // Save changes with transaction integrity
                    _context.Transactions.Add(transaction);
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        Console.WriteLine($"✅ Book '{book.Title}' borrowed successfully by {borrower.FullName}");
                        Console.WriteLine($"   Due date: {transaction.DueDate:yyyy-MM-dd}");
                        Console.WriteLine($"   Transaction ID: {transaction.TransactionId}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("❌ Error: Failed to save borrowing transaction");
                        return false;
                    }
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"❌ Database Error during borrowing: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Unexpected Error during borrowing: {ex.Message}");
                    return false;
                }
            }

            /// <summary>
            /// Retrieves books with optional filtering and pagination for performance
            /// </summary>
            /// <param name="searchTerm">Optional search term to filter books</param>
            /// <param name="pageNumber">Page number for pagination (1-based)</param>
            /// <param name="pageSize">Number of books per page</param>
            /// <returns>List of books matching the criteria</returns>
            /// <example>
            /// <code>
            /// // Get first 10 books
            /// var books = await bookService.GetBooksAsync(pageNumber: 1, pageSize: 10);
            /// 
            /// // Search for books containing "Orwell"
            /// var orwellBooks = await bookService.GetBooksAsync("Orwell");
            /// </code>
            /// </example>
            public async Task<List<Book>> GetBooksAsync(string searchTerm = null, int pageNumber = 1, int pageSize = 50)
            {
                try
                {
                    // Input validation
                    if (pageNumber <= 0)
                    {
                        Console.WriteLine("❌ Warning: Invalid page number, using default (1)");
                        pageNumber = 1;
                    }

                    if (pageSize <= 0 || pageSize > 1000)
                    {
                        Console.WriteLine("❌ Warning: Invalid page size, using default (50)");
                        pageSize = 50;
                    }

                    // Build query with optional filtering
                    var query = _context.Books.AsQueryable();

                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        var term = searchTerm.Trim().ToLower();
                        query = query.Where(b => 
                            b.Title.ToLower().Contains(term) || 
                            b.Author.ToLower().Contains(term) ||
                            b.ISBN.Contains(term));
                    }

                    // Apply pagination and ordering
                    var books = await query
                        .OrderBy(b => b.Title)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    Console.WriteLine($"✅ Retrieved {books.Count} books (Page {pageNumber}, Size {pageSize})");
                    
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        Console.WriteLine($"   Search term: '{searchTerm}'");
                    }

                    return books;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error retrieving books: {ex.Message}");
                    return new List<Book>();
                }
            }
        }

        #endregion

        #region COMPARISON SUMMARY

        /// <summary>
        /// Summary of coding standards differences demonstrated above
        /// </summary>
        public static class CodingStandardsSummary
        {
            /// <summary>
            /// Key differences between poor and good coding practices
            /// </summary>
            public static readonly Dictionary<string, (string Poor, string Good)> Standards = new()
            {
                ["Naming"] = (
                    "badBookSvc, addbook, db, t, a, i", 
                    "BookService, AddBookAsync, _context, title, author, isbn"
                ),
                ["Documentation"] = (
                    "No XML comments, unclear purpose", 
                    "Comprehensive XML docs with examples and exceptions"
                ),
                ["Error Handling"] = (
                    "No try-catch, operations can fail silently", 
                    "Comprehensive exception handling with specific error types"
                ),
                ["Validation"] = (
                    "No input validation, assumes valid data", 
                    "Thorough input validation with clear error messages"
                ),
                ["Async Operations"] = (
                    "Synchronous database calls, blocking operations", 
                    "Proper async/await pattern throughout"
                ),
                ["Business Logic"] = (
                    "No business rule validation", 
                    "Comprehensive business rule enforcement"
                ),
                ["Performance"] = (
                    "Loads all data, no pagination", 
                    "Pagination, filtering, optimized queries"
                ),
                ["Maintainability"] = (
                    "Hard to modify, tightly coupled", 
                    "Loosely coupled, easy to extend and modify"
                ),
                ["Debugging"] = (
                    "No logging, hard to troubleshoot", 
                    "Detailed logging and error messages"
                ),
                ["Code Organization"] = (
                    "Mixed responsibilities, unclear structure", 
                    "Single responsibility, clear separation of concerns"
                )
            };

            /// <summary>
            /// Prints a comparison of coding standards
            /// </summary>
            public static void PrintComparison()
            {
                Console.WriteLine("=== CODING STANDARDS COMPARISON ===\n");
                
                foreach (var standard in Standards)
                {
                    Console.WriteLine($"📋 {standard.Key}:");
                    Console.WriteLine($"   ❌ Poor: {standard.Value.Poor}");
                    Console.WriteLine($"   ✅ Good: {standard.Value.Good}");
                    Console.WriteLine();
                }
            }
        }

        #endregion
    }
}
