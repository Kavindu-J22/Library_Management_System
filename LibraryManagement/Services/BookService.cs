using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement.Services
{
    /// <summary>
    /// Service class for managing book-related operations
    /// </summary>
    public class BookService
    {
        private readonly LibraryContext _context;

        /// <summary>
        /// Constructor for BookService
        /// </summary>
        /// <param name="context">Database context</param>
        public BookService(LibraryContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new book to the library
        /// </summary>
        /// <param name="book">Book to add</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> AddBookAsync(Book book)
        {
            try
            {
                // Check if book with same ISBN already exists
                var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == book.ISBN);
                if (existingBook != null)
                {
                    Console.WriteLine($"Book with ISBN {book.ISBN} already exists. Consider updating the copy count instead.");
                    return false;
                }

                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Book '{book.Title}' added successfully with ID: {book.BookId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding book: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates an existing book
        /// </summary>
        /// <param name="book">Book to update</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateBookAsync(Book book)
        {
            try
            {
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Book '{book.Title}' updated successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating book: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Searches for books by title
        /// </summary>
        /// <param name="title">Title to search for</param>
        /// <returns>List of matching books</returns>
        public async Task<List<Book>> SearchBooksByTitleAsync(string title)
        {
            try
            {
                return await _context.Books
                    .Where(b => b.Title.Contains(title))
                    .OrderBy(b => b.Title)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching books by title: {ex.Message}");
                return new List<Book>();
            }
        }

        /// <summary>
        /// Searches for books by author
        /// </summary>
        /// <param name="author">Author to search for</param>
        /// <returns>List of matching books</returns>
        public async Task<List<Book>> SearchBooksByAuthorAsync(string author)
        {
            try
            {
                return await _context.Books
                    .Where(b => b.Author.Contains(author))
                    .OrderBy(b => b.Author)
                    .ThenBy(b => b.Title)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching books by author: {ex.Message}");
                return new List<Book>();
            }
        }

        /// <summary>
        /// Searches for books by ISBN
        /// </summary>
        /// <param name="isbn">ISBN to search for</param>
        /// <returns>Book if found, null otherwise</returns>
        public async Task<Book?> SearchBookByISBNAsync(string isbn)
        {
            try
            {
                return await _context.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching book by ISBN: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets all available books
        /// </summary>
        /// <returns>List of available books</returns>
        public async Task<List<Book>> GetAvailableBooksAsync()
        {
            try
            {
                return await _context.Books
                    .Where(b => b.AvailableCopies > 0)
                    .OrderBy(b => b.Title)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting available books: {ex.Message}");
                return new List<Book>();
            }
        }

        /// <summary>
        /// Gets all books
        /// </summary>
        /// <returns>List of all books</returns>
        public async Task<List<Book>> GetAllBooksAsync()
        {
            try
            {
                return await _context.Books
                    .OrderBy(b => b.Title)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all books: {ex.Message}");
                return new List<Book>();
            }
        }

        /// <summary>
        /// Gets a book by ID
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <returns>Book if found, null otherwise</returns>
        public async Task<Book?> GetBookByIdAsync(int bookId)
        {
            try
            {
                return await _context.Books.FindAsync(bookId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting book by ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Borrows a book for a borrower
        /// </summary>
        /// <param name="bookId">ID of the book to borrow</param>
        /// <param name="borrowerId">ID of the borrower</param>
        /// <param name="daysToReturn">Number of days until due date (default: 14)</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> BorrowBookAsync(int bookId, int borrowerId, int daysToReturn = 14)
        {
            try
            {
                // Get book and borrower
                var book = await _context.Books.FindAsync(bookId);
                var borrower = await _context.Borrowers.Include(b => b.Transactions).FirstOrDefaultAsync(b => b.BorrowerId == borrowerId);

                if (book == null)
                {
                    Console.WriteLine("Book not found.");
                    return false;
                }

                if (borrower == null)
                {
                    Console.WriteLine("Borrower not found.");
                    return false;
                }

                // Check if book is available
                if (!book.IsAvailable())
                {
                    Console.WriteLine($"Book '{book.Title}' is not available for borrowing.");
                    return false;
                }

                // Check if borrower can borrow more books
                if (!borrower.CanBorrowMoreBooks())
                {
                    Console.WriteLine($"Borrower {borrower.FullName} has reached the maximum borrowing limit or account is inactive.");
                    return false;
                }

                // Check if borrower has overdue books
                if (borrower.HasOverdueBooks())
                {
                    Console.WriteLine($"Borrower {borrower.FullName} has overdue books. Please return them first.");
                    return false;
                }

                // Create transaction
                var transaction = new Transaction
                {
                    BookId = bookId,
                    BorrowerId = borrowerId,
                    TransactionType = TransactionType.Borrow,
                    TransactionDate = DateTime.Now,
                    DueDate = DateTime.Today.AddDays(daysToReturn),
                    Status = TransactionStatus.Active
                };

                // Update book availability
                book.BorrowCopy();

                // Add transaction and save changes
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Book '{book.Title}' borrowed successfully by {borrower.FullName}. Due date: {transaction.DueDate:yyyy-MM-dd}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error borrowing book: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Returns a borrowed book
        /// </summary>
        /// <param name="bookId">ID of the book to return</param>
        /// <param name="borrowerId">ID of the borrower</param>
        /// <param name="returnDate">Return date (defaults to current date)</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ReturnBookAsync(int bookId, int borrowerId, DateTime? returnDate = null)
        {
            try
            {
                // Find the active borrow transaction
                var transaction = await _context.Transactions
                    .Include(t => t.Book)
                    .Include(t => t.Borrower)
                    .FirstOrDefaultAsync(t => t.BookId == bookId && 
                                            t.BorrowerId == borrowerId && 
                                            t.Status == TransactionStatus.Active && 
                                            t.TransactionType == TransactionType.Borrow);

                if (transaction == null)
                {
                    Console.WriteLine("No active borrowing transaction found for this book and borrower.");
                    return false;
                }

                var actualReturnDate = returnDate ?? DateTime.Now;

                // Check if book is overdue and calculate fine
                if (transaction.IsOverdue())
                {
                    var daysOverdue = transaction.DaysOverdue();
                    transaction.FineAmount = transaction.CalculateFine();
                    Console.WriteLine($"Book is {daysOverdue} days overdue. Fine: ${transaction.FineAmount:F2}");
                }

                // Mark transaction as returned
                transaction.MarkAsReturned(actualReturnDate);

                // Update book availability
                transaction.Book.ReturnCopy();

                await _context.SaveChangesAsync();

                Console.WriteLine($"Book '{transaction.Book.Title}' returned successfully by {transaction.Borrower.FullName}.");
                if (transaction.FineAmount > 0)
                {
                    Console.WriteLine($"Fine amount: ${transaction.FineAmount:F2}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error returning book: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deletes a book from the library
        /// </summary>
        /// <param name="bookId">ID of the book to delete</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> DeleteBookAsync(int bookId)
        {
            try
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    Console.WriteLine("Book not found.");
                    return false;
                }

                // Check if book has active transactions
                var activeTransactions = await _context.Transactions
                    .Where(t => t.BookId == bookId && t.Status == TransactionStatus.Active)
                    .CountAsync();

                if (activeTransactions > 0)
                {
                    Console.WriteLine("Cannot delete book with active transactions.");
                    return false;
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Book '{book.Title}' deleted successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting book: {ex.Message}");
                return false;
            }
        }
    }
}
